using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SIAH.Context;
using SIAH.Models.Reclamos;

namespace SIAH.Controllers
{
    public class TipoReclamosController : Controller
    {
        private SIAHContext db = new SIAHContext();

        // GET: TipoReclamos
        public ActionResult Index()
        {
            return View(db.TipoReclamoes.ToList());
        }

        // GET: TipoReclamos/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TipoReclamo tipoReclamo = db.TipoReclamoes.Find(id);
            if (tipoReclamo == null)
            {
                return HttpNotFound();
            }
            return View(tipoReclamo);
        }

        // GET: TipoReclamos/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: TipoReclamos/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,tipo,descripcion")] TipoReclamo tipoReclamo)
        {
            if (ModelState.IsValid)
            {
                db.TipoReclamoes.Add(tipoReclamo);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(tipoReclamo);
        }

        // GET: TipoReclamos/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TipoReclamo tipoReclamo = db.TipoReclamoes.Find(id);
            if (tipoReclamo == null)
            {
                return HttpNotFound();
            }
            return View(tipoReclamo);
        }

        // POST: TipoReclamos/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,tipo,descripcion")] TipoReclamo tipoReclamo)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tipoReclamo).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tipoReclamo);
        }

        // GET: TipoReclamos/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TipoReclamo tipoReclamo = db.TipoReclamoes.Find(id);
            if (tipoReclamo == null)
            {
                return HttpNotFound();
            }
            return View(tipoReclamo);
        }

        // POST: TipoReclamos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            TipoReclamo tipoReclamo = db.TipoReclamoes.Find(id);
            db.TipoReclamoes.Remove(tipoReclamo);
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
