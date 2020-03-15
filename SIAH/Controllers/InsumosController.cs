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
        public ActionResult Create()
        {
            ViewBag.tipoInsumoId = new SelectList(db.TiposInsumo, "id", "nombre");
            return View();
        }
        public ActionResult ControlStock()
        {
            var insumos = db.Insumos.Include(i => i.tiposInsumo).Join(db.InsumoOcasa, d => d.id, s => s.id, (d, s) => new { d, s }).
                Select(x => new { id = x.d.id, nombre = x.d.nombre, tipo = x.d.tiposInsumo.nombre, stock = x.d.stockFisico, stockOcasa = x.s.stockFisico }).ToList();
            ViewBag.insumos = insumos;
            return View();
        }

        public async Task<ActionResult> ActualizarStock()
        {
            await sendEmailAsync();
            try
            {
                Uri baseUri = new Uri("http://localhost:3000");
                Uri myUri = new Uri(baseUri, "/reclamo?name=success");
                var response = client.GetAsync(myUri);
                if (response.Result.StatusCode != HttpStatusCode.OK) return RedirectToAction("DirectorArea", "Home", new { param = "Failed" });
                var insumosOcasa = db.InsumoOcasa.Select(x => new { id = x.id, stockOcasa = x.stockFisico }).ToList();
               
                foreach (var item in insumosOcasa)
                {
                    var insumoActual = db.Insumos.Find(item.id);
                    if (insumoActual != null)
                    {   //Asumimos que el Stock Fisico (entregado) es mayor al stock (comprometido por las autorizaciones)
                        //por lo tanto calculamos la diferencia para mantener la misma respecto al nuevo stock
                        //Ej: Stock fisico = 100; Stock comprometido = 30; Stock Ocasa = 90
                        //Resultado -> Stock fisico 90; Stock comprometido = 20
                        var diff = insumoActual.stockFisico - insumoActual.stock; 
                        insumoActual.stockFisico = item.stockOcasa;
                        insumoActual.stock = item.stockOcasa - diff;
                        db.Entry(insumoActual).State = EntityState.Modified;
                    }

                }
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
        public ActionResult StockInsumos()
        {
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
                db.SaveChanges();
                return RedirectToAction("StockInsumos");
            }

            ViewBag.tipoInsumoId = new SelectList(db.TiposInsumo, "id", "nombre", insumo.tipoInsumoId);
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
            ViewBag.tipoInsumoId = new SelectList(db.TiposInsumo, "id", "nombre", insumo.tipoInsumoId);
            return View(insumo);
        }

        // POST: Insumos/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,nombre,precioUnitario,tipoInsumoId,stock")] Insumo insumo)
        {
            if (ModelState.IsValid)
            {
                db.Entry(insumo).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("StockInsumos");
            }
            ViewBag.tipoInsumoId = new SelectList(db.TiposInsumo, "id", "nombre", insumo.tipoInsumoId);
            return View(insumo);
        }

        // GET: Insumos/Delete/5
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
        public ActionResult DeleteConfirmed(int id)
        {
            Insumo insumo = db.Insumos.Find(id);
            db.Insumos.Remove(insumo);
            db.SaveChanges();
            return RedirectToAction("StockInsumos");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
