using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SIAH.Context;
using SIAH.Models;

namespace SIAH.Controllers
{
    public class HospitalesController : Controller
    {
        private SIAHContext db = new SIAHContext();

        //GET: Presupuesto
        public String getPresupuesto(int idHospital)
        {
            Hospital hospital = db.Hospitales.Where(s => s.id == idHospital).First();
            return hospital.presupuestoDisponible.ToString();
            //decimal presupuestoRestante = db.Hospitales.Find(idHospital).presupuesto;
            //var pedidosMesActual = db.Pedidos
            //    .Include(d => d.detallesPedido)
            //    .Include(d => d.detallesPedido.Select(x => x.insumo))
            //    .Where(
            //            x => x.hospitalId == idHospital &&
            //            x.fechaGeneracion.Month == DateTime.Today.Month &&
            //            x.fechaGeneracion.Year == DateTime.Today.Year)
            //    .ToList();
            //decimal presupuestoGastado = 0;
            //foreach(var pedido in pedidosMesActual)
            //{
            //    decimal gastoPedido = 0;
            //    foreach (var detalle in pedido.detallesPedido)
            //    {
            //        gastoPedido += detalle.insumo.precioUnitario * detalle.cantidad;
            //    }
            //    presupuestoGastado += gastoPedido;
            //}

            //presupuestoRestante -= presupuestoGastado;
            //if (presupuestoRestante <= 0) presupuestoRestante = 0;
            //return presupuestoRestante.ToString();
        }

        // GET: Hospitals
        public ActionResult Index()
        {
            var hospitales = db.Hospitales.Include(h => h.localidad);
            return View(hospitales.ToList());
        }

        // GET: Hospitals/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Hospital hospital = db.Hospitales.Include(x => x.localidad).Where(s => s.id == id).First();
            UserAccount responsable = db.UserAccounts.Where(u => u.hospitalID == hospital.id).FirstOrDefault();
            ViewBag.nombreResponsable = responsable?.nombre + " " + responsable?.apellido;
            if (hospital == null)
            {
                return HttpNotFound();
            }
            return View(hospital);
        }

        // GET: Hospitals/Create
        public ActionResult Create()
        {
            ViewBag.localidadId = new SelectList(db.Localidades, "id", "nombre");
            return View();
        }

        // POST: Hospitals/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,nombre,localidadId, presupuesto, latitud, longitud, telefono")] Hospital hospital)
        {
            if (ModelState.IsValid)
            {
                db.Hospitales.Add(hospital);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.localidadId = new SelectList(db.Localidades, "id", "nombre", hospital.localidadId);
            return View(hospital);
        }

        // GET: Hospitals/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Hospital hospital = db.Hospitales.Find(id);
            if (hospital == null)
            {
                return HttpNotFound();
            }
            ViewBag.localidadId = new SelectList(db.Localidades, "id", "nombre", hospital.localidadId);
            return View(hospital);
        }

        // POST: Hospitals/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,nombre,localidadId, presupuesto, latitud, longitud, telefono, direccion")] Hospital hospital)
        {
            if (ModelState.IsValid)
            {
                db.Entry(hospital).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.localidadId = new SelectList(db.Localidades, "id", "nombre", hospital.localidadId);
            return View(hospital);
        }

        // GET: Hospitals/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Hospital hospital = db.Hospitales.Find(id);
            if (hospital == null)
            {
                return HttpNotFound();
            }
            return View(hospital);
        }

        // POST: Hospitals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Hospital hospital = db.Hospitales.Find(id);
            db.Hospitales.Remove(hospital);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Hospitals
        [AuthorizeUserAccessLevel(UserRole = "DirectorArea")]
        public ActionResult Presupuestos()
        {
            return View(db.Hospitales.ToList());
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
