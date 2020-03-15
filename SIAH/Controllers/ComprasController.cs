using SIAH.Context;
using SIAH.Models.Compras;
using SIAH.Models.Insumos;
using System;
using System.IO;
using System.Linq;
using System.Data;
using System.Data.Entity;
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
        [AuthorizeUserAccessLevel(UserRole = "Compras")]
        public ActionResult CargarCompra(String param)
        {

            if (param != null)
            {
                if (param.CompareTo("success") == 0)
                {
                    ViewBag.success = true;
                }
                else
                {
                    ViewBag.success = false;
                    ViewBag.problem = param;
                };
            }
            return View();
        }

        // POST: Cargar compra
        [AuthorizeUserAccessLevel(UserRole = "Compras")]
        [HttpPost]
        public ActionResult CargarCompra(HttpPostedFileBase file, DateTime fechaEntregaEfectiva)
        {
            try
            {
                string fileExtension = Path.GetExtension(file.FileName);
                if (fileExtension != ".xls" && fileExtension != ".xlsx")
                {
                    return RedirectToAction("CargarCompra", new { param = "failure" });
                }
                string fileName = Path.GetFileName(file.FileName);
                string path = Path.Combine(Server.MapPath("~/CargaRemitosCSV"), fileName);
                file.SaveAs(path);

                ViewBag.Message = "";
                ExportToCSV(path);
                System.IO.File.Delete(path);
                CargarRemitoCompra(fechaEntregaEfectiva);
                System.IO.File.Delete($"{Server.MapPath("~/CargaRemitosCSV")}/RemitoCompra.csv");
                return RedirectToAction("CargarCompra", new { param = "success" });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return RedirectToAction("CargarCompra", new { param = "failure" });
            }
        }

        //GET: Compras/Details
        public ActionResult Details(int id)
        {
            Compra compra = db.Compras.Include(d => d.detallesCompra).Include(c => c.detallesCompra.Select(x => x.insumo)).Where(r => r.id == id).First();
            return View(compra);
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
                var headerLine = reader.ReadLine();
                while (!reader.EndOfStream)
                {
                    ActualizarDatos(compraId, reader);
                }
            }

            db.SaveChanges();
        }

        private void ActualizarDatos(int compraId, StreamReader reader)
        {
            var line = reader.ReadLine();
            var values = line.Split(',');
            var insumoId = int.Parse(values[0]);
            var cantidadComprada = int.Parse(values[1]);
            DetalleCompra detalleCompra = new DetalleCompra();
            detalleCompra.compraId = compraId;
            detalleCompra.insumoId = insumoId;
            detalleCompra.cantidadComprada = cantidadComprada;
            db.DetallesCompra.Add(detalleCompra);
            ActualizarStock(insumoId, cantidadComprada);
        }

        private void ActualizarStock(int insumoId, int cantidadComprada)
        {
            Insumo insumo = db.Insumos.Find(insumoId);
            insumo.stock += cantidadComprada;
            insumo.stockFisico += cantidadComprada;
        }

    }
}