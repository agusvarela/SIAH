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
    public class EstadoReclamosController : Controller
    {
        private SIAHContext db = new SIAHContext();

        // GET: EstadoReclamos
        public ActionResult Index()
        {
            return View(db.EstadoReclamoes.ToList());
        }

        // GET: EstadoReclamos/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EstadoReclamo estadoReclamo = db.EstadoReclamoes.Find(id);
            if (estadoReclamo == null)
            {
                return HttpNotFound();
            }
            return View(estadoReclamo);
        }

        // GET: EstadoReclamos/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: EstadoReclamos/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,nombreEstado,isFinal")] EstadoReclamo estadoReclamo)
        {
            if (ModelState.IsValid)
            {
                db.EstadoReclamoes.Add(estadoReclamo);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(estadoReclamo);
        }

        // GET: EstadoReclamos/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EstadoReclamo estadoReclamo = db.EstadoReclamoes.Find(id);
            if (estadoReclamo == null)
            {
                return HttpNotFound();
            }
            return View(estadoReclamo);
        }

        // POST: EstadoReclamos/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,nombreEstado,isFinal")] EstadoReclamo estadoReclamo)
        {
            if (ModelState.IsValid)
            {
                db.Entry(estadoReclamo).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(estadoReclamo);
        }

        // GET: EstadoReclamos/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EstadoReclamo estadoReclamo = db.EstadoReclamoes.Find(id);
            if (estadoReclamo == null)
            {
                return HttpNotFound();
            }
            return View(estadoReclamo);
        }

        // POST: EstadoReclamos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            EstadoReclamo estadoReclamo = db.EstadoReclamoes.Find(id);
            db.EstadoReclamoes.Remove(estadoReclamo);
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
