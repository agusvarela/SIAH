using SIAH.Context;
using SIAH.Models;
using SIAH.Models.Compras;
using SIAH.Models.Insumos;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SIAH.Controllers
{
    public class ComprasController : Controller
    {
        private SIAHContext db = new SIAHContext();

        // GET: Compras
        public ActionResult Index()
        {
            return View(db.Compras.ToList());
        }

        // GET: Cargar compra
        public ActionResult CargarCompra()
        { 
            return View();
        }

        [HttpPost]

        public ActionResult CargarCompra(HttpPostedFileBase file)
        {
            try
            {
                if (file.ContentLength > 0)
                {
                    string _FileName = Path.GetFileName(file.FileName);
                    string _path = Path.Combine(Server.MapPath("~/CargaRemitosCSV"), _FileName);
                    file.SaveAs(_path);
                }

                ViewBag.Message = "Archivo Subido";
                Session.Add("archivo","_path");
                return Redirect("Remito");
            }
            catch (Exception e)
            {
                ViewBag.Message = "No selecciono ningun archivo";
                Console.WriteLine(e.Message);
                return View();
            }
        }

        //GET: Remito
        public ActionResult Remito()
        {
            String ultimo_archivo = (from f in System.IO.Directory.GetFiles(Server.MapPath("~/CargaRemitosCSV"))
                                  orderby f descending
                                  select f).First();

            List<Insumo> compraInsumos = new List<Insumo>();
            Boolean bandera = true;
            List<Insumo> insumos = buscarNombreInsumo();

            using (var reader = new StreamReader(ultimo_archivo))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(';');

                    if (bandera == true)
                    {
                        compraInsumos.Add( new Insumo { id = Int32.Parse(values[0]) } );
                        bandera = false;
                    }

                    int idInsumo = Int32.Parse(values[1]);

                    String nombreInsumo = insumos.Find(x => x.id == idInsumo).nombre;

                    compraInsumos.Add(new Insumo { id = idInsumo, nombre = nombreInsumo, stockFisico = Int32.Parse(values[2]) });
                }
            }

            return View(compraInsumos);
        }

        [HttpPost]
        public ActionResult Remito([Bind(Include = "id,fechaEntregaEfectiva")] Compra compra)
        {
            String ultimo_archivo = (from f in System.IO.Directory.GetFiles(Server.MapPath("~/CargaRemitosCSV"))
                                  orderby f descending
                                  select f).First();

            ejecutarProcedimiento(ultimo_archivo, compra);

            actualizarInsumos(ultimo_archivo);

            return Redirect("Index");
        }


        public void actualizarInsumos(String nombreArchivo)
        {
            //List<Insumo> insumoStock = obtenerStock();
            List<Insumo> compraInsumos = new List<Insumo>();

            using (var reader = new StreamReader(nombreArchivo))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(';');

                    compraInsumos.Add(new Insumo { id = Int32.Parse(values[1]), stockFisico = Int32.Parse(values[2]) });
                }
            }

            actualizarStock(compraInsumos);
        }


        public void actualizarStock(List<Insumo> insumosActualizar)
        {
            String procedimientoSQL, cadenaAuxiliar;
            using (var sr = new StreamReader(Server.MapPath("~/CargaRemitosCSV/0_ActualizarStock.sql")))
            {
                procedimientoSQL = sr.ReadToEnd();
                sr.Close();
            }

            string sqlConnectionString = ConfigurationManager.ConnectionStrings["SIAHConnection"].ConnectionString;
            SqlConnection conn = new SqlConnection(sqlConnectionString);
            conn.Open();

            foreach (var insumo in insumosActualizar)
            {
                cadenaAuxiliar = procedimientoSQL;

                cadenaAuxiliar = cadenaAuxiliar.Replace("CANTIDADCOMPROMETIDA", insumo.stockFisico.ToString());
                cadenaAuxiliar = cadenaAuxiliar.Replace("CANTIDADFISICA", insumo.stockFisico.ToString());
                cadenaAuxiliar = cadenaAuxiliar.Replace("ID", insumo.id.ToString());

                SqlCommand cm1 = new SqlCommand(cadenaAuxiliar, conn);
                cm1.ExecuteNonQuery();
            }

            conn.Close();
        }

        
        public void ejecutarProcedimiento(String nombreArchivo, Compra compra)
        {
            String procedimientoCompra = "INSERT INTO [dbo].[Compra] ([id] ,[fechaEntregaEfectiva])" +
                " VALUES (" + compra.id +",'" + compra.fechaEntregaEfectiva + "')";

            String procedimientoSQL = "BULK INSERT DetalleCompra FROM '"
                + nombreArchivo + "' WITH ( FIELDTERMINATOR = ';', ROWTERMINATOR = '\n' )";

            string sqlConnectionString = ConfigurationManager.ConnectionStrings["SIAHConnection"].ConnectionString;
            SqlConnection conn = new SqlConnection(sqlConnectionString);
            conn.Open();

            SqlCommand command1 = new SqlCommand(procedimientoCompra, conn);
            command1.ExecuteNonQuery();

            SqlCommand command2 = new SqlCommand(procedimientoSQL, conn);
            command2.ExecuteNonQuery();

            conn.Close();
        }


        public List<Insumo> buscarNombreInsumo()
        {
            String procedimientoSQL = "SELECT [id] ,[nombre] FROM [dbo].[Insumo]";

            string sqlConnectionString = ConfigurationManager.ConnectionStrings["SIAHConnection"].ConnectionString;

            SqlConnection conn = new SqlConnection(sqlConnectionString);
            conn.Open();

            SqlCommand command = new SqlCommand(procedimientoSQL, conn);

            SqlDataReader reader = command.ExecuteReader();

            List<Insumo> insumos = new List<Insumo>();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    insumos.Add(new Insumo { id = reader.GetInt32(0), nombre = reader.GetString(1) } );
                }
            }
            else
            {
                Console.WriteLine("No rows found.");
            }
            reader.Close();

            conn.Close();

            return insumos;
        }
    }
}