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
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Net;

namespace SIAH.Controllers
{
    public class ComprasController : Controller
    {
        private SIAHContext db = new SIAHContext();

        // GET: Compras
        [AuthorizeUserAccessLevel(UserRole = "DirectorArea")]
        public ActionResult Index()
        {
            return View(db.Compras.ToList());
        }

        // GET: Cargar compra
        [AuthorizeUserAccessLevel(UserRole = "Compras", UserRole2 = "DirectorArea")]
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
            return View(new Compra());
        }

        // POST: Cargar compra
        [HttpPost]
        public ActionResult CargarCompra(Compra compra)
        {
            compra.id = db.Insumos.ToList().Last().id + 1;

            try
            {
                db.Compras.Add(compra);
                foreach(var detalle in compra.detallesCompra) {
                    ActualizarDatos(compra.id, detalle);
                }
                db.SaveChanges();

                string acceptValue = "La compra se cargo exitosamente.";
                var result = Content(JsonConvert.SerializeObject(new { message = acceptValue }), "application/json; charset=utf-8");
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Accepted;
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                string acceptValue = "La compra ya se encuentra cargada.";
                var result = Content(JsonConvert.SerializeObject(new { error = acceptValue }), "application/json; charset=utf-8");
                HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return result;
            }
        }

        //GET: Compras/Details
        [AuthorizeUserAccessLevel(UserRole = "DirectorArea")]
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

        private void ActualizarDatos(int compraId, DetalleCompra detalleCompra)
        {
            detalleCompra.compraId = compraId;
            db.DetallesCompra.Add(detalleCompra);
            ActualizarStock(detalleCompra.insumoId, detalleCompra.cantidadComprada);
        }

        private void ActualizarStock(int insumoId, int cantidadComprada)
        {
            Insumo insumo = db.Insumos.Find(insumoId);
            insumo.stock += cantidadComprada;
            insumo.stockFisico += cantidadComprada;
            db.Entry(insumo).State = EntityState.Modified;
        }
    }
}