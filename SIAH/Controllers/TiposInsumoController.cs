using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SIAH.Context;
using SIAH.Models.Insumos;

namespace SIAH.Controllers
{
    public class TiposInsumoController : Controller
    {
        private SIAHContext db = new SIAHContext();

        // GET: TiposInsumo
        public ActionResult Index()
        {
            return View(db.TiposInsumo.ToList());
        }

        // GET: TiposInsumo/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TipoInsumo tipoInsumo = db.TiposInsumo.Find(id);
            if (tipoInsumo == null)
            {
                return HttpNotFound();
            }
            return View(tipoInsumo);
        }

        // GET: TiposInsumo/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: TiposInsumo/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,nombre")] TipoInsumo tipoInsumo)
        {
            if (ModelState.IsValid)
            {
                db.TiposInsumo.Add(tipoInsumo);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(tipoInsumo);
        }

        // GET: TiposInsumo/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TipoInsumo tipoInsumo = db.TiposInsumo.Find(id);
            if (tipoInsumo == null)
            {
                return HttpNotFound();
            }
            return View(tipoInsumo);
        }

        // POST: TiposInsumo/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,nombre")] TipoInsumo tipoInsumo)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tipoInsumo).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tipoInsumo);
        }

        // GET: TiposInsumo/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TipoInsumo tipoInsumo = db.TiposInsumo.Find(id);
            if (tipoInsumo == null)
            {
                return HttpNotFound();
            }
            return View(tipoInsumo);
        }

        // POST: TiposInsumo/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            TipoInsumo tipoInsumo = db.TiposInsumo.Find(id);
            db.TiposInsumo.Remove(tipoInsumo);
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
