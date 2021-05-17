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
using SIAH.Models.Historico;

namespace SIAH.Controllers
{
    public class ComprasController : Controller
    {
        private SIAHContext db = new SIAHContext();

        // GET: Compras
        [AuthorizeUserAccessLevel(UserRole = "Compras", UserRole2 = "DirectorArea")]
        public ActionResult Index(String param)
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
            var compraCargada = db.Compras.Any(compraDB => compraDB.numeroComprobante == compra.numeroComprobante && compraDB.cuilProveedor == compra.cuilProveedor);

            if (compraCargada)
            {
                string acceptValue = "La compra ya se encuentra cargada.";
                var result = Content(JsonConvert.SerializeObject(new { error = acceptValue }), "application/json; charset=utf-8");
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return result;
            }

            compra.id = db.Insumos.ToList().Last().id + 1;
            var nuevoIdCompra = db.Compras.ToList().Last().id + 1;
            compra.fechaCargaCompra = DateTime.UtcNow;

            try
            {
                db.Compras.Add(compra);
                foreach(var detalle in compra.detallesCompra) {
                    ActualizarDatos(compra.id, detalle, compra.fechaCargaCompra, nuevoIdCompra);
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
                string acceptValue = "Ocurrio un error inesperado al intentar guardar la compra.";
                var result = Content(JsonConvert.SerializeObject(new { error = acceptValue }), "application/json; charset=utf-8");
                HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return result;
            }
        }

        //GET: Compras/Details
        [AuthorizeUserAccessLevel(UserRole = "Compras", UserRole2 = "DirectorArea")]
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

        private void ActualizarDatos(int compraId, DetalleCompra detalleCompra, DateTime fechaCargaCompra, int nuevoIdCompra)
        {
            detalleCompra.compraId = compraId;
            db.DetallesCompra.Add(detalleCompra);
            ActualizarStock(detalleCompra.insumoId, detalleCompra.cantidadComprada, fechaCargaCompra, nuevoIdCompra);
        }

        private void ActualizarStock(int insumoId, int cantidadComprada, DateTime fechaCargaCompra, int nuevoIdCompra)
        {
            Insumo insumo = db.Insumos.Find(insumoId);
            insumo.stock += cantidadComprada;
            insumo.stockFisico += cantidadComprada;
            db.Entry(insumo).State = EntityState.Modified;

            if (cantidadComprada > 0)
            {
                AgregarHistorico(insumoId, cantidadComprada, insumo.stock, fechaCargaCompra, nuevoIdCompra);
            }
        }

        private void AgregarHistorico(int insumoId, int cantidadComprada, int saldo, DateTime fechaCargaCompra, int nuevoIdCompra)
        {
            HistoricoSIAH historicoSIAH = new HistoricoSIAH();
            historicoSIAH.insumoId = insumoId;
            historicoSIAH.fechaMovimiento = fechaCargaCompra;
            historicoSIAH.descripcion = "Compra registrada, Compra número: " + nuevoIdCompra;
            historicoSIAH.saldo = saldo;
            historicoSIAH.isNegative = false;
            historicoSIAH.cantidad = cantidadComprada;

            db.HistoricoSIAH.Add(historicoSIAH);


            HistoricoFisico historicoFisico = new HistoricoFisico();
            historicoFisico.insumoId = insumoId;
            historicoFisico.fechaMovimiento = fechaCargaCompra;
            historicoFisico.descripcion = "Compra registrada, Compra número: " + nuevoIdCompra;
            historicoFisico.saldo = saldo;
            historicoFisico.isNegative = false;
            historicoFisico.cantidad = cantidadComprada;

            db.HistoricoFisico.Add(historicoFisico);
        }
    }
}