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
using Microsoft.Office.Interop.Excel;

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

        [AuthorizeUserAccessLevel(UserRole = "Compras")]
        [HttpPost]
        public ActionResult CargarCompra(HttpPostedFileBase file, DateTime fechaEntregaEfectiva)
        {
            try
            {
                string fileName = Path.GetFileName(file.FileName);
                string path = Path.Combine(Server.MapPath("~/CargaRemitosCSV"), fileName);
                file.SaveAs(path);

                ViewBag.Message = "Archivo Subido";
                ExportToCSV(path);
                System.IO.File.Delete(path);
                CargarRemitoCompra(fechaEntregaEfectiva);
                System.IO.File.Delete($"{Server.MapPath("~/CargaRemitosCSV")}/RemitoCompra.csv");
                return View();
            }
            catch (Exception e)
            {
                ViewBag.Message = "No selecciono ningun archivo";
                Console.WriteLine(e.Message);
                return View();
            }
        }

        private void ExportToCSV(string path)
        {
            Application xlApp = new Application();
            Workbook xlWorkBook = xlApp.Workbooks.Open(path, 0, true, 5, "", "", true, XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
            Worksheet xlWorkSheet = (Worksheet)xlWorkBook.Worksheets.get_Item(1);
            xlWorkBook.SaveAs($"{Server.MapPath("~/CargaRemitosCSV")}/RemitoCompra.csv", XlFileFormat.xlCSV);
            xlWorkBook.Close(false, "", true);
        }

        private void CargarRemitoCompra(DateTime fechaEntregaEfectiva)
        {
            Compra compra = new Compra();
            compra.fechaEntregaEfectiva = fechaEntregaEfectiva;
            db.Compras.Add(compra);
            db.SaveChanges();
            var compraId = compra.id;
            using (var reader = new StreamReader($"{Server.MapPath("~/CargaRemitosCSV")}/RemitoCompra.csv"))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');
                    DetalleCompra detalleCompra = new DetalleCompra();
                    detalleCompra.compraId = compraId;
                    detalleCompra.insumoId = int.Parse(values[0]);
                    detalleCompra.cantidadComprada = int.Parse(values[1]);
                    db.DetallesCompra.Add(detalleCompra);
                    //TODO: Add stock update
                }
            }

            db.SaveChanges();
        }

        //GET: Remito
        [AuthorizeUserAccessLevel(UserRole = "Compras")]
        public ActionResult Remito([Bind(Include = "id")] String id, [Bind(Include = "fechaEntregaEfectiva")] DateTime fechaEntregaEfectiva)
        {
            String fechaEfectivaCompra = fechaEntregaEfectiva.Day + "_" + fechaEntregaEfectiva.Month + "_" + fechaEntregaEfectiva.Year;
            String nombreArchivo = id + "_RemitoOcasa_" + fechaEfectivaCompra + ".csv";

            String ultimo_archivo = (from f in System.IO.Directory.GetFiles(Server.MapPath("~/CargaRemitosCSV"))
                                     orderby f descending
                                     select f)
                                  .First(nombre =>
                                  nombre.Contains(nombreArchivo));

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
                        compraInsumos.Add(new Insumo { id = Int32.Parse(values[0]) });
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