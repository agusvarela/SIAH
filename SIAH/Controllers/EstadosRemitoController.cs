using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SIAH.Context;
using SIAH.Models.Remitos;

namespace SIAH.Controllers
{
    public class EstadosRemitoController : Controller
    {
        private SIAHContext db = new SIAHContext();

        // GET: EstadosRemito
        public ActionResult Index()
        {
            return View(db.EstadoRemitoes.ToList());
        }

        // GET: EstadosRemito/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EstadoRemito estadoRemito = db.EstadoRemitoes.Find(id);
            if (estadoRemito == null)
            {
                return HttpNotFound();
            }
            return View(estadoRemito);
        }

        // GET: EstadosRemito/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: EstadosRemito/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,nombreEstado")] EstadoRemito estadoRemito)
        {
            if (ModelState.IsValid)
            {
                db.EstadoRemitoes.Add(estadoRemito);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(estadoRemito);
        }

        // GET: EstadosRemito/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EstadoRemito estadoRemito = db.EstadoRemitoes.Find(id);
            if (estadoRemito == null)
            {
                return HttpNotFound();
            }
            return View(estadoRemito);
        }

        // POST: EstadosRemito/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,nombreEstado")] EstadoRemito estadoRemito)
        {
            if (ModelState.IsValid)
            {
                db.Entry(estadoRemito).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(estadoRemito);
        }

        // GET: EstadosRemito/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EstadoRemito estadoRemito = db.EstadoRemitoes.Find(id);
            if (estadoRemito == null)
            {
                return HttpNotFound();
            }
            return View(estadoRemito);
        }

        // POST: EstadosRemito/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            EstadoRemito estadoRemito = db.EstadoRemitoes.Find(id);
            db.EstadoRemitoes.Remove(estadoRemito);
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
