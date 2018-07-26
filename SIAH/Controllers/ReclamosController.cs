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
            var reclamoes = db.Reclamoes.Include(r => r.estadoReclamo).Include(r => r.hospital).Include(r => r.pedido).Include(r => r.responsableAsignado).Include(r => r.tipoReclamo);
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
        public ActionResult Create(int? pedidoId, int? hospitalId)
        {
            if(pedidoId == null || hospitalId == null)
            {
                return HttpNotFound();
            }
            ViewBag.pedidoId = pedidoId;
            ViewBag.estadoReclamoId = new SelectList(db.EstadoReclamoes, "id", "nombreEstado");
            ViewBag.hospital = db.Hospitales.Where(p => p.id == hospitalId).First().nombre;
            ViewBag.hospitalId = hospitalId;
            ViewBag.responsableAsignadoId = new SelectList(db.UserAccounts, "id", "nombre");
            ViewBag.tipoReclamoId = new SelectList(db.TipoReclamoes, "id", "tipo");
            return View();
        }

        // POST: Reclamos/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,observacionFamacia,fechaInicioReclamo,tipoReclamoId,pedidoId,hospitalId,estadoReclamoId")] Reclamo reclamo)
        {
            //TODO: Falta setear nullable el responsableId y establecer el estado inicial del reclamo mejor
            reclamo.responsableAsignadoId = db.UserAccounts.First().id;
            reclamo.estadoReclamoId = db.EstadoReclamoes.First().id;
            if (ModelState.IsValid)
            {
                db.Reclamoes.Add(reclamo);
                reclamo.estadoReclamoId = 1;
                reclamo.fechaInicioReclamo = DateTime.Today;
                reclamo.responsableAsignadoId = null;
                db.SaveChanges();
                return RedirectToAction("RespFarmacia", "Pedidos");
            }

            ViewBag.estadoReclamoId = new SelectList(db.EstadoReclamoes, "id", "nombreEstado", reclamo.estadoReclamoId);
            ViewBag.hospitalId = reclamo.hospitalId;
            ViewBag.hospital = db.Hospitales.Where(p => p.id == reclamo.hospitalId).First().nombre;
            ViewBag.pedidoId = reclamo.pedidoId;
            ViewBag.responsableAsignadoId = new SelectList(db.UserAccounts, "id", "nombre", reclamo.responsableAsignadoId);
            ViewBag.tipoReclamoId = new SelectList(db.TipoReclamoes, "id", "tipo", reclamo.tipoReclamoId);
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
            ViewBag.estadoReclamoId = new SelectList(db.EstadoReclamoes, "id", "nombreEstado", reclamo.estadoReclamoId);
            ViewBag.hospitalId = new SelectList(db.Hospitales, "id", "nombre", reclamo.hospitalId);
            ViewBag.pedidoId = new SelectList(db.Pedidos, "id", "id", reclamo.pedidoId);
            ViewBag.responsableAsignadoId = new SelectList(db.UserAccounts, "id", "nombre", reclamo.responsableAsignadoId);
            ViewBag.tipoReclamoId = new SelectList(db.TipoReclamoes, "id", "tipo", reclamo.tipoReclamoId);
            return View(reclamo);
        }

        // POST: Reclamos/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,observacionFamacia,respuesta,fechaInicioReclamo,fechaFinReclamo,tipoReclamoId,pedidoId,hospitalId,responsableAsignadoId,estadoReclamoId")] Reclamo reclamo)
        {
            if (ModelState.IsValid)
            {
                db.Entry(reclamo).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.estadoReclamoId = new SelectList(db.EstadoReclamoes, "id", "nombreEstado", reclamo.estadoReclamoId);
            ViewBag.hospitalId = new SelectList(db.Hospitales, "id", "nombre", reclamo.hospitalId);
            ViewBag.pedidoId = new SelectList(db.Pedidos, "id", "id", reclamo.pedidoId);
            ViewBag.responsableAsignadoId = new SelectList(db.UserAccounts, "id", "nombre", reclamo.responsableAsignadoId);
            ViewBag.tipoReclamoId = new SelectList(db.TipoReclamoes, "id", "tipo", reclamo.tipoReclamoId);
            return View(reclamo);
        }
        [HttpPost]
        public ActionResult AutoAsignacion(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reclamo reclamo = db.Reclamoes.Find(id);
            return AutoAsignacion(reclamo);
        }
        [HttpPost]
        public ActionResult AutoAsignacion(Reclamo reclamo)
        {
            if (reclamo == null)
            {
                return HttpNotFound();
            }
            reclamo.responsableAsignadoId= Int32.Parse(Session["userid"].ToString());
            if (ModelState.IsValid)
            {
                db.Entry(reclamo).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("ListadoReclamos");
            }
            return View(reclamo);
        }
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult AutoAsignacion([Bind(Include = "id,observacionFamacia,respuesta,fechaInicioReclamo,fechaFinReclamo,tipoReclamoId,pedidoId,hospitalId,responsableAsignadoId,estadoReclamoId")] Reclamo reclamo)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(reclamo).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("ListadoReclamos");
        //    }
        //    return View(reclamo);
        //}

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
        [AuthorizeUserAccessLevel(UserRole = "RespFarmacia")]
        public ActionResult ReclamosRespFarmacia()
        {

            //var reclamoes = db.Reclamoes.Include(r => r.hospital).Include(r => r.pedido).Include(r => r.tipoReclamo).Include(r => r.estadoReclamo);
            //return View(reclamoes.ToList());
            var hospitalActual = Int32.Parse(Session["hospitalId"].ToString());
            var reclamoes = db.Reclamoes.Where(r => r.hospitalId == hospitalActual).Include(p => p.hospital).Include(r => r.pedido).Include(r => r.tipoReclamo).Include(r => r.estadoReclamo);
            return View(reclamoes.ToList());

        }
        [AuthorizeUserAccessLevel(UserRole = "RespAutorizacion")]
        public ActionResult ListadoReclamos()
        {
            var reclamoes = db.Reclamoes.Include(r => r.hospital).Include(r => r.pedido).Include(r => r.tipoReclamo).Include(r => r.estadoReclamo);
            return View(reclamoes.ToList());
        }
    }
}
