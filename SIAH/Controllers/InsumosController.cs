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

        public ActionResult ActualizarStock()
        {
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
