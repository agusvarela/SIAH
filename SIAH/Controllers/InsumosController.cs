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

namespace SIAH.Controllers
{
    public class InsumosController : Controller
    {
        private SIAHContext db = new SIAHContext();

        // GET: Insumos
        public ActionResult Index()
        {
            var insumos = db.Insumos.Include(i => i.tiposInsumo);
            return View(insumos.ToList());
        }

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
            var insumos = db.Insumos.Include(i => i.tiposInsumo);
            return View(insumos.ToList());
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
                db.Insumos.Add(insumo);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.tipoInsumoId = new SelectList(db.TiposInsumo, "id", "nombre", insumo.tipoInsumoId);
            return View(insumo);
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
                return RedirectToAction("Index");
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
            return RedirectToAction("Index");
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
