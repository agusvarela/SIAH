using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SIAH.Context;
using SIAH.Models.Pedidos;
using SIAH.Models.Reclamos;

namespace SIAH.Controllers
{
    public class ReclamosController : Controller
    {
        private SIAHContext db = new SIAHContext();

        // GET: Reclamos
        public ActionResult Index()
        {
            var reclamoes = db.Reclamoes.Include(r => r.estadoReclamo).Include(r => r.hospital).Include(r => r.Pedido).Include(r => r.responsableAsignado).Include(r => r.tipoReclamo);
            return View(reclamoes.ToList());
        }

        // GET: Reclamos/Details/5
        [AuthorizeUserAccessLevel(UserRole = "RespAutorizacion", UserRole2 = "DirectorArea", UserRole3 = "RespFarmacia")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reclamo reclamo = db.Reclamoes.Include(r => r.hospital).Include(r => r.Pedido).
                Include(r => r.tipoReclamo).Include(r => r.estadoReclamo).Include(r => r.responsableAsignado)
                .Where(r => r.pedidoId == id).First();
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
            Pedido pedido = db.Pedidos.Find(reclamo.pedidoId);
            if (ModelState.IsValid)
            {
                //Cambio de estado del pedido
                pedido.estadoId = 7; //Reclamado
                db.Entry(pedido).State = EntityState.Modified;
                //Generacion del reclamo
                db.Reclamoes.Add(reclamo);
                reclamo.estadoReclamoId = 1; //Generado
                reclamo.fechaInicioReclamo = DateTime.Today;
                reclamo.responsableAsignadoId = null;
                //Guardar toda la transacción en DB
                db.SaveChanges();
                return RedirectToAction("ReclamosRespFarmacia", "Reclamos");
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
        [AuthorizeUserAccessLevel(UserRole = "RespFarmacia")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reclamo reclamo = db.Reclamoes.Include(x => x.hospital).Where(x => x.pedidoId == id).First();
            if (reclamo == null)
            {
                return HttpNotFound();
            }
            ViewBag.tipoReclamoId = new SelectList(db.TipoReclamoes, "id", "tipo");

            return View(reclamo);
        }
        
        public ActionResult Resolucion (int? id)
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
            ViewBag.hospitalId = reclamo.hospitalId;
            ViewBag.hospital = db.Hospitales.Find(reclamo.hospitalId).nombre;
            ViewBag.tipo = db.TipoReclamoes.Find(reclamo.tipoReclamoId).tipo;
            ViewBag.pedidoId = reclamo.pedidoId;
            ViewBag.tipoReclamoId = reclamo.tipoReclamoId;
            return View(reclamo);
        }

        //POST: Reclamos/Resolucion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Resolucion([Bind(Include = "observacionFamacia,respuesta,fechaInicioReclamo,fechaFinReclamo,tipoReclamoId,pedidoId,hospitalId,responsableAsignadoId")] Reclamo reclamo)
        {
            if (ModelState.IsValid)
            {
                reclamo.estadoReclamoId = 4; //Resuelto
                //reclamo.fechaFinReclamo = DateTime.Now;
                Pedido p = db.Pedidos.Find(reclamo.pedidoId);
                p.estadoId = 8; //Reclamo Resuelto
                db.Entry(reclamo).State = EntityState.Modified;
                db.Entry(p).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("ListadoReclamos", "Reclamos");
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
                reclamo.estadoReclamoId = 1; //Generado
                db.Entry(reclamo).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("ReclamosRespFarmacia", "Reclamos");
            }

            return View(reclamo);
        }

        // POST: Reclamos/AutoAsignacion
        public ActionResult AutoAsignacion(int? idReclamo, String idResponsable)
        {
            if (idReclamo == null || idResponsable == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reclamo reclamo = db.Reclamoes.Find(idReclamo);
            if (reclamo == null)
            {
                return HttpNotFound();
            }
            reclamo.responsableAsignadoId = int.Parse(idResponsable);
            reclamo.estadoReclamoId = 2; //En Revision
            db.Entry(reclamo).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("ListadoReclamos", "Reclamos");
        }

        public ActionResult Unassign(int idReclamo)
        {
            Reclamo reclamo = db.Reclamoes.Find(idReclamo);
            reclamo.responsableAsignadoId = null;
            reclamo.responsableAsignado = null;
            reclamo.estadoReclamoId = 1; //Generado
            db.Entry(reclamo).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("ListadoReclamos", "Reclamos");
        }

        //GET: Reclamos/MoreInfo
        public ActionResult MoreInfo(int idReclamo)
        {
            Reclamo reclamo = db.Reclamoes.Find(idReclamo);
            reclamo.responsableAsignadoId = null;
            reclamo.responsableAsignado = null;
            reclamo.estadoReclamoId = 3; //Informacion Solicitada
            db.Entry(reclamo).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("ListadoReclamos", "Reclamos");
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
        [AuthorizeUserAccessLevel(UserRole = "RespFarmacia")]
        public ActionResult ReclamosRespFarmacia()
        {
            var hospitalActual = Int32.Parse(Session["hospitalId"].ToString());
            var reclamoes = db.Reclamoes.Where(r => r.hospitalId == hospitalActual).Include(p => p.hospital).
                Include(r => r.Pedido).Include(r => r.tipoReclamo).Include(r => r.estadoReclamo).Include(r => r.responsableAsignado);
            return View(reclamoes.ToList());

        }

        //GET:Reclamos/ListadoReclamos
        [AuthorizeUserAccessLevel(UserRole = "RespAutorizacion", UserRole2 = "DirectorArea", UserRole3 = "RespFarmacia")]
        public ActionResult ListadoReclamos()
        {
            var reclamoes = db.Reclamoes.Include(r => r.hospital).Include(r => r.Pedido).
                Include(r => r.tipoReclamo).Include(r => r.estadoReclamo).Include(r => r.responsableAsignado);
            var rol = Session["rol"].ToString();
            if (rol == "RespFarmacia")
            {
                return RedirectToAction("ReclamosRespFarmacia", "Reclamos");
            }
            ViewBag.session = Session["userId"].ToString();
            return View(reclamoes.ToList());
        }

        //GET: Reclamos/ReclamosDatasetBI
        public JsonResult ReclamosDatasetBI()
        {
            var dataset = db.Reclamoes.Include(x => x.hospital).Include(x => x.tipoReclamo)
                .Select(x => new { IdReclamo = x.pedidoId, Tipo = x.tipoReclamo.tipo, Hospital = x.hospital.nombre, FechaInicioReclamo = x.fechaInicioReclamo, FechaFinReclamo = x.fechaFinReclamo})
                .ToList().Select(x => new {
                    IdReclamo = x.IdReclamo,
                    Tipo = x.Tipo,
                    Hospital = x.Hospital, 
                    FechaInicioReclamo = string.Format("{0:MM/dd/yyyy}", x.FechaInicioReclamo), 
                    FechaFinReclamo = string.Format("{0:MM/dd/yyyy}", x.FechaFinReclamo), 
                    EstaResuelto = IsResolved(x.IdReclamo) }); ;
            return Json(dataset, JsonRequestBehavior.AllowGet);
        }

        private bool IsResolved(int idReclamo)
        {
            Reclamo reclamo = db.Reclamoes.Include(x => x.estadoReclamo).Where(x => x.pedidoId == idReclamo).First();
            return reclamo.estadoReclamo.isFinal; //The only final state is resolved
        }
    }
}
