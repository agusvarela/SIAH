using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Net.Http;
using SIAH.Context;
using SIAH.Models.Insumos;
using System.Dynamic;
using SIAH.Models;
using System.Threading.Tasks;
using System.Net.Mail;
using System.IO;
using System.Reflection;
using SIAH.Models.AjusteSIAH;
using SIAH.Models.Historico;

namespace SIAH.Controllers
{
    public class InsumosController : Controller
    {
        private SIAHContext db = new SIAHContext();
        private static readonly HttpClient client = new HttpClient();

        // GET: Insumos/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Insumo insumo = db.Insumos.Find(id);
            if (insumo == null)
            {
                return HttpNotFound();
            }
            return View(insumo);
        }

        // GET: Insumos/Create
        [AuthorizeUserAccessLevel(UserRole = "DirectorArea")]
        public ActionResult Create()
        {
            ViewBag.tipoInsumoId = new SelectList(db.TiposInsumo.OrderBy(tipo => tipo.nombre), "id", "nombre");
            return View();
        }

        //GET: Insumos como array
        public JsonResult getInsumos()
        {
            String[] insumos = db.Insumos.OrderBy(tipo => tipo.nombre).Select(x => x.nombre).ToArray();
            return Json(new { data = insumos.ToArray() }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ControlStock()
        {
            var insumos = db.Insumos.Include(i => i.tiposInsumo).Join(db.InsumoOcasa, d => d.id, s => s.id, (d, s) => new { d, s })
                .Where(x => x.d.stockFisico != x.s.stockFisico)
                .Select(x => new { id = x.d.id, nombre = x.d.nombre, tipo = x.d.tiposInsumo.nombre, stock = x.d.stockFisico, stockOcasa = x.s.stockFisico }).ToList();
            ViewBag.insumos = insumos;
            return View();
        }

        public async Task<ActionResult> ReclamarOcasa()
        {
            await sendEmailAsync();
            return RedirectToAction("DirectorArea", "Home", new { param = "Reclamo" });
        }

        public ActionResult ActualizarStock(string[] syncData, string userId)
        {
            try
            {
                // TODO: crear un ajuste nuevo y al recorrer los insumos ir agregando los detalles
                // No olvidar crear los historicos para cada insumo
                var ajuste = new AjusteSIAH();
                ajuste.fechaGeneracion = DateTime.Today;
                ajuste.info = "Sincronización con stock OCASA";
                ajuste.usuarioId = int.Parse(userId);
                Uri baseUri = new Uri("http://localhost:3000");
                var detalles = new HashSet<DetalleAjusteSIAH>();
                foreach (var id in syncData)
                {
                    var insumoActual = db.Insumos.Find(int.Parse(id));
                    var insumoOcasaActual = db.InsumoOcasa.Find(int.Parse(id));
                    if (insumoActual != null)
                    {   //Asumimos que el Stock Fisico (entregado) es mayor al stock (comprometido por las autorizaciones)
                        //por lo tanto calculamos la diferencia para mantener la misma respecto al nuevo stock
                        //Ej: Stock fisico = 100; Stock comprometido = 30; Stock Ocasa = 90
                        //Resultado -> Stock fisico 90; Stock comprometido = 20
                        var diferenciaStocks = insumoOcasaActual.stockFisico - insumoActual.stockFisico;
                        var diff = insumoActual.stockFisico - insumoActual.stock; 
                        insumoActual.stockFisico = insumoOcasaActual.stockFisico <= 0 ? 0 : insumoOcasaActual.stockFisico;
                        insumoActual.stock = insumoActual.stockFisico - diff <= 0 ? 0 : insumoActual.stockFisico - diff;
                        db.Entry(insumoActual).State = EntityState.Modified;
                        var detalleAjuste = new DetalleAjusteSIAH();
                        detalleAjuste.insumoId = insumoActual.id;
                        detalleAjuste.info = "Ajuste por sincronizacion con OCASA";
                        detalleAjuste.cantidad = diferenciaStocks;
                        detalles.Add(detalleAjuste);
                        agregarHistoricoSIAH(ajuste, insumoActual.stock, detalleAjuste);
                        agregarHistoricoFisico(ajuste, insumoActual.stockFisico, detalleAjuste);
                    }
                }
                ajuste.detallesAjuste = detalles;
                db.AjusteSIAHs.Add(ajuste);
                db.SaveChanges();
                return RedirectToAction("DirectorArea", "Home", new { param = "Success" });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return RedirectToAction("DirectorArea", "Home", new
                {
                    param = e.Message
                });
            }

        }


        private void agregarHistoricoSIAH(AjusteSIAH ajusteSIAH, int saldo, DetalleAjusteSIAH detalle)
        {
            HistoricoSIAH historicoSIAH = new HistoricoSIAH();
            historicoSIAH.insumoId = detalle.insumoId;
            historicoSIAH.fechaMovimiento = ajusteSIAH.fechaGeneracion;
            historicoSIAH.descripcion = "Ajuste de stock: " + detalle.info;
            historicoSIAH.saldo = saldo;
            historicoSIAH.isNegative = detalle.cantidad < 0 ? true : false;
            historicoSIAH.cantidad = detalle.cantidad;

            db.HistoricoSIAH.Add(historicoSIAH);
        }

        private void agregarHistoricoFisico(AjusteSIAH ajusteSIAH, int saldo, DetalleAjusteSIAH detalle)
        {
            HistoricoFisico historicoFisico = new HistoricoFisico();
            historicoFisico.insumoId = detalle.insumoId;
            historicoFisico.fechaMovimiento = ajusteSIAH.fechaGeneracion;
            historicoFisico.descripcion = "Ajuste de stock: " + detalle.info;
            historicoFisico.saldo = saldo;
            historicoFisico.isNegative = detalle.cantidad < 0 ? true : false;
            historicoFisico.cantidad = detalle.cantidad;

            db.HistoricoFisico.Add(historicoFisico);
        }
        private async Task sendEmailAsync()
        {

            var insumos = db.Insumos.Include(i => i.tiposInsumo).Join(db.InsumoOcasa, d => d.id, s => s.id, (d, s) => new { d, s }).
            Select(x => new { id = x.d.id, nombre = x.d.nombre, tipo = x.d.tiposInsumo.nombre, stock = x.d.stockFisico, stockOcasa = x.s.stockFisico, diferencia = x.s.stockFisico - x.d.stockFisico })
            .Where( x => x.diferencia != 0)
            .ToList();

            var message = new MailMessage();
            message.To.Add(new MailAddress("ocasa.reclamos@gmail.com"));
            message.From = new MailAddress("siah.reclamos@gmail.com");
            string date = DateTime.Now.ToString("dd'-'MM'-'yyyy");
            message.Subject = string.Format("[RECLAMO SIAH] [{0}] Encontramos inconsistencias en el Stock de Insumos",date);
            string body = string.Empty;
            string table = string.Empty;
            string tablesample = "<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td><td>{5}</td></tr>";
            using (StreamReader reader = new StreamReader(Server.MapPath("../Views/Shared/EmailDiferenciaStock.html")))
            {
                body = reader.ReadToEnd();
            }
            foreach (var insumo in insumos)
            {
                table += string.Format(tablesample, insumo.id, insumo.nombre, insumo.tipo, insumo.stock, insumo.stockOcasa,insumo.diferencia);
            }
            body = body.Replace("{table}", table);

            string path = string.Format(Server.MapPath("../DiferenciaOcasa{0}.csv"),date);
            WriteCSV(insumos,path);

            Attachment attachment;

            attachment = new Attachment (path);
            message.Attachments.Add(attachment);

            message.Body = body;

            message.IsBodyHtml = true;

            using (var smtp = new SmtpClient())
            {
                await smtp.SendMailAsync(message);
            }
            message.Dispose();
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
        }

        public void WriteCSV<T>(IEnumerable<T> items, string path)
        {
            Type itemType = typeof(T);
            var props = itemType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                .OrderBy(p => p.Name);
            using (var writer = new StreamWriter(path))
            {
                writer.AutoFlush = true;
                writer.WriteLine(string.Join(", ", props.Select(p => p.Name)));
                foreach (var item in items)
                {
                    writer.WriteLine(string.Join(", ", props.Select(p => p.GetValue(item, null))));
                }
                writer.Close();
            }
        }

        //GET: Insumos/StockInsumos
        [AuthorizeUserAccessLevel(UserRole = "DirectorArea")]
        public ActionResult StockInsumos(string param)
        {
            if (param != null)
            {
                if (param.CompareTo("Success") == 0)
                {
                    ViewBag.success = true;
                }
                else
                {
                    ViewBag.success = false;
                    ViewBag.problem = param;
                };
            }
            var insumos = db.Insumos.Include(i => i.tiposInsumo);
            return View(insumos.ToList());
        }


        // GET: Insumos/Palabra/search
        public JsonResult BuscarInsumos(string term, int? id)
        {
            if (id != null)
            {
                var results_id = db.Insumos.Join(db.TiposInsumo, s => s.tipoInsumoId, t => t.id, (s, t) => new { s, t })
                    .Where(s => term == null || (s.s.nombre.ToLower().Contains(term.ToLower()) && s.s.tipoInsumoId == id))
                    .Select(x => new { id = x.s.id, nombre = x.s.nombre, x.s.precioUnitario, tipo = x.t.nombre }).Take(5).ToList();

                return Json(results_id, JsonRequestBehavior.AllowGet);
            }

            var results = db.Insumos.Join(db.TiposInsumo, s => s.tipoInsumoId, t => t.id, (s, t) => new { s, t })
                .Where(s => term == null || (s.s.nombre.ToLower().Contains(term.ToLower())))
                .Select(x => new { id = x.s.id, nombre = x.s.nombre, x.s.precioUnitario, tipo = x.t.nombre }).Take(5).ToList();

            return Json(results, JsonRequestBehavior.AllowGet);
        }

        // POST: Insumos/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeUserAccessLevel(UserRole = "DirectorArea")]
        public ActionResult Create([Bind(Include = "id,nombre,precioUnitario,tipoInsumoId,stock")] Insumo insumo)
        {
            if (ModelState.IsValid)
            {
                //TODO: mejorar este comportamiento
                int nextId = 0;
                try
                {
                    nextId = db.Insumos.ToList().Last().id + 1;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                insumo.id = nextId;
                db.Database.ExecuteSqlCommand(" INSERT INTO[dbo].[Insumo] " +
                    "([id], [nombre], [precioUnitario], [tipoInsumoId], [stock], [stockFisico])" +
                    " VALUES({0}, {1}, {2}, {3}, {4}, {5})",
                    insumo.id, insumo.nombre, insumo.precioUnitario, insumo.tipoInsumoId, insumo.stock, insumo.stockFisico);
                AddToStockFarmacia(insumo.id);
                try
                {
                    if (db.SaveChanges() > 0)
                    {
                        return RedirectToAction("StockInsumos", new { param = "Success" });
                    }
                }
                catch (Exception e)
                {
                    return RedirectToAction("StockInsumos", new { param = e.Message });
                }
            }

            ViewBag.tipoInsumoId = new SelectList(db.TiposInsumo.OrderBy(tipo => tipo.nombre), "id", "nombre", insumo.tipoInsumoId);
            return View(insumo);
        }

        private void AddToStockFarmacia(int idInsumo)
        {
            var hospitalesList = db.Hospitales.Select(x => x.id).ToList();
            foreach ( int idHospital in hospitalesList)
            {
                StockFarmacia stockFarmacia = new StockFarmacia();
                stockFarmacia.insumoId = idInsumo;
                stockFarmacia.hospitalId = idHospital;
                stockFarmacia.stockFarmacia = 0;
                db.StockFarmacias.Add(stockFarmacia);
            }
        }

        // GET: Insumos/Edit/5
        [AuthorizeUserAccessLevel(UserRole = "DirectorArea")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Insumo insumo = db.Insumos.Find(id);
            if (insumo == null)
            {
                return HttpNotFound();
            }
            ViewBag.tipoInsumoId = new SelectList(db.TiposInsumo.OrderBy(tipo => tipo.nombre), "id", "nombre", insumo.tipoInsumoId);
            return View(insumo);
        }

        // POST: Insumos/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeUserAccessLevel(UserRole = "DirectorArea")]
        public ActionResult Edit([Bind(Include = "id,nombre,precioUnitario,tipoInsumoId,stock")] Insumo insumo)
        {
            if (ModelState.IsValid)
            {
                db.Entry(insumo).State = EntityState.Modified;
                try
                {
                    if (db.SaveChanges() > 0)
                    {
                        return RedirectToAction("StockInsumos", new { param = "Success" });
                    }
                }
                catch (Exception e)
                {
                    return RedirectToAction("StockInsumos", new { param = e.Message });
                }
            }
            ViewBag.tipoInsumoId = new SelectList(db.TiposInsumo.OrderBy(tipo => tipo.nombre), "id", "nombre", insumo.tipoInsumoId);
            return View(insumo);
        }

        // GET: Insumos/Delete/5
        [AuthorizeUserAccessLevel(UserRole = "DirectorArea")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Insumo insumo = db.Insumos.Find(id);
            if (insumo == null)
            {
                return HttpNotFound();
            }
            return View(insumo);
        }

        // POST: Insumos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizeUserAccessLevel(UserRole = "DirectorArea")]
        public ActionResult DeleteConfirmed(int id)
        {
            Insumo insumo = db.Insumos.Find(id);
            db.Insumos.Remove(insumo);
            db.SaveChanges();
            return RedirectToAction("StockInsumos");
        }

        // GET: Insumos/OcasaInventario
        public ActionResult OcasaInventario(string param = null)
        {
            if (param != null)
            {
                if (param.CompareTo("Success") == 0)
                {
                    ViewBag.success = true;
                }
                else
                {
                    ViewBag.success = false;
                    ViewBag.problem = param;
                };
            }
            var insumosList = db.InsumoOcasa.ToList();

            return View(insumosList);
        }

        // POST: Insumos/OcasaInventario
        [HttpPost]
        public ActionResult OcasaInventario(IList<InsumoOcasa> newStock)
        {
            var oldStockList = db.InsumoOcasa.ToList();
            foreach (var insumo in newStock)
            {
                var oldInsumo = oldStockList.Find(x => x.id == insumo.id && x.stockFisico != insumo.stockFisico);
                if (oldInsumo != null)
                {
                    oldInsumo.stockFisico = insumo.stockFisico;
                    db.Entry(oldInsumo).State = EntityState.Modified;
                }
            }
            try
            {
                db.SaveChanges();
                return RedirectToAction("OcasaInventario", "Insumos", new { param = "Success" });
            }
            catch (Exception e)
            {
                return RedirectToAction("OcasaInventario", "Insumos", new { param = e.Message });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        // GET: Historico SIAH
        [AuthorizeUserAccessLevel(UserRole = "DirectorArea")]
        public ActionResult HistoricoSIAH(int insumoId)
        {
            ViewBag.insumo = db.Insumos.Find(insumoId).nombre;
            return View(db.HistoricoSIAH.Where(h => h.insumoId == insumoId).ToList());
        }

        // GET: Historico Fisico
        [AuthorizeUserAccessLevel(UserRole = "DirectorArea")]
        public ActionResult HistoricoFisico(int insumoId)
        {
            ViewBag.insumo = db.Insumos.Find(insumoId).nombre;
            return View(db.HistoricoFisico.Where(h => h.insumoId == insumoId).ToList());
        }
    }
}
