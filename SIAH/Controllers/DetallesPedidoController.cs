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

namespace SIAH.Controllers
{
    public class DetallesPedidoController : Controller
    {
        private SIAHContext db = new SIAHContext();

        // GET: DetallesPedido
        public ActionResult Index()
        {
            var detallesPedido = db.DetallesPedido.Include(d => d.insumo).Include(d => d.pedido);
            return View(detallesPedido.ToList());
        }

     

        // GET: DetallesPedido/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DetallePedido detallePedido = db.DetallesPedido.Find(id);
            if (detallePedido == null)
            {
                return HttpNotFound();
            }
            return View(detallePedido);
        }

        // GET: DetallesPedido/Create
        public ActionResult Create()
        {
            ViewBag.insumoId = new SelectList(db.Insumos, "id", "nombre");
            ViewBag.pedidoId = new SelectList(db.Pedidos, "id", "id");
            return View();
        }

        // POST: DetallesPedido/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "pedidoId,insumoId,cantidad")] DetallePedido detallePedido)
        {
            if (ModelState.IsValid)
            {
                db.DetallesPedido.Add(detallePedido);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.insumoId = new SelectList(db.Insumos, "id", "nombre", detallePedido.insumoId);
            ViewBag.pedidoId = new SelectList(db.Pedidos, "id", "id", detallePedido.pedidoId);
            return View(detallePedido);
        }

        // GET: DetallesPedido/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DetallePedido detallePedido = db.DetallesPedido.Find(id);
            if (detallePedido == null)
            {
                return HttpNotFound();
            }
            ViewBag.insumoId = new SelectList(db.Insumos, "id", "nombre", detallePedido.insumoId);
            ViewBag.pedidoId = new SelectList(db.Pedidos, "id", "id", detallePedido.pedidoId);
            return View(detallePedido);
        }

        // POST: DetallesPedido/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "pedidoId,insumoId,cantidad")] DetallePedido detallePedido)
        {
            if (ModelState.IsValid)
            {
                db.Entry(detallePedido).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.insumoId = new SelectList(db.Insumos, "id", "nombre", detallePedido.insumoId);
            ViewBag.pedidoId = new SelectList(db.Pedidos, "id", "id", detallePedido.pedidoId);
            return View(detallePedido);
        }

        // GET: DetallesPedido/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DetallePedido detallePedido = db.DetallesPedido.Find(id);
            if (detallePedido == null)
            {
                return HttpNotFound();
            }
            return View(detallePedido);
        }

        // POST: DetallesPedido/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            DetallePedido detallePedido = db.DetallesPedido.Find(id);
            db.DetallesPedido.Remove(detallePedido);
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
