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
    public class ReclamosController : Controller
    {
        private SIAHContext db = new SIAHContext();

        // GET: Reclamos
        public ActionResult Index()
        {
            var reclamoes = db.Reclamoes.Include(r => r.hospital).Include(r => r.pedido);
            return View(reclamoes.ToList());
        }

        // GET: Reclamos/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reclamo reclamo = db.Reclamoes.Find(id);
            if (reclamo == null)
            {
                return HttpNotFound();
            }
            return View(reclamo);
        }

        // GET: Reclamos/Create
        public ActionResult Create()
        {
            ViewBag.hospitalId = new SelectList(db.Hospitales, "id", "nombre");
            ViewBag.pedidoId = new SelectList(db.Pedidos, "id", "id");
            return View();
        }

        // POST: Reclamos/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,observacionFamacia,respuesta,fechaInicioReclamo,fechaFinReclamo,reclamoId,pedidoId,hospitalId,estadoId")] Reclamo reclamo)
        {
            if (ModelState.IsValid)
            {
                db.Reclamoes.Add(reclamo);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.hospitalId = new SelectList(db.Hospitales, "id", "nombre", reclamo.hospitalId);
            ViewBag.pedidoId = new SelectList(db.Pedidos, "id", "id", reclamo.pedidoId);
            return View(reclamo);
        }

        // GET: Reclamos/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reclamo reclamo = db.Reclamoes.Find(id);
            if (reclamo == null)
            {
                return HttpNotFound();
            }
            ViewBag.hospitalId = new SelectList(db.Hospitales, "id", "nombre", reclamo.hospitalId);
            ViewBag.pedidoId = new SelectList(db.Pedidos, "id", "id", reclamo.pedidoId);
            return View(reclamo);
        }

        // POST: Reclamos/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,observacionFamacia,respuesta,fechaInicioReclamo,fechaFinReclamo,reclamoId,pedidoId,hospitalId,estadoId")] Reclamo reclamo)
        {
            if (ModelState.IsValid)
            {
                db.Entry(reclamo).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.hospitalId = new SelectList(db.Hospitales, "id", "nombre", reclamo.hospitalId);
            ViewBag.pedidoId = new SelectList(db.Pedidos, "id", "id", reclamo.pedidoId);
            return View(reclamo);
        }

        // GET: Reclamos/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reclamo reclamo = db.Reclamoes.Find(id);
            if (reclamo == null)
            {
                return HttpNotFound();
            }
            return View(reclamo);
        }

        // POST: Reclamos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Reclamo reclamo = db.Reclamoes.Find(id);
            db.Reclamoes.Remove(reclamo);
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
