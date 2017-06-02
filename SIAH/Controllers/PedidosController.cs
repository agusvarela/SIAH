﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SIAH.Context;
using SIAH.Models.Pedidos;
using SIAH.Models.Insumos;
using SIAH.Models;
using System.Globalization;

namespace SIAH.Controllers
{
    public class PedidosController : Controller
    {
        private SIAHContext db = new SIAHContext();

        // GET: Pedidos
        public ActionResult Index()
        {
            var pedidos = db.Pedidos.Include(p => p.hospital);
            return View(pedidos.ToList());
        }

        // GET: Pedidos/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Pedido pedido = db.Pedidos.Find(id);

            if (pedido == null)
            {
                return HttpNotFound();
            }
            return View(pedido);
        }

        //GET: Pedidos/GetHospital
        public String GetHospital(int? hospitalId)
        {
            Hospital hospital = db.Hospitales.Find(hospitalId);
            return hospital.nombre;
        }

        // GET: Pedidos/Create
        public ActionResult Create()
        {
            ViewBag.tipoInsumo = new SelectList(db.TiposInsumo, "id", "nombre");
            ViewBag.hospitalId = new SelectList(db.Hospitales, "id", "nombre");
            return View(new Pedido());
        }

        // GET: Pedidos/Listado
        public ActionResult Listado()
        {
            var pedidos = db.Pedidos.Include(p => p.hospital);
            return View(pedidos.ToList());
        }

        public JsonResult GetInsumos(String id)
        {
            int idTipo = int.Parse(id);
            var insumosBD = new SelectList(db.Insumos.Where(m => m.tipoInsumoId == idTipo), "id", "nombre");

            return Json(insumosBD);
        }

        //GET: Pedidos/DetallesPedido
        public JsonResult GetDetalles(int idPedido)
        {
            var detallesPedido = db.DetallesPedido.Include(d => d.insumo).Include(d => d.pedido).Where(d => d.pedidoId == idPedido)
                                .Select(x => new { nombre = x.insumo.nombre, precio = x.insumo.precioUnitario, cantidad = x.cantidad, cantidadAutorizada = x.cantidadAutorizada, tipo = x.insumo.tiposInsumo.nombre });
            return Json(detallesPedido, JsonRequestBehavior.AllowGet);
        }

        // POST: Pedidos/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,periodo,fechaGeneracion,esUrgente,estaAutorizado,hospitalId,detallesPedido")] Pedido pedido)
        {
            pedido.fechaEntrega = null;

            foreach (var detalle in pedido.detallesPedido)
            {
                detalle.insumo = null;
            }

            if (ModelState.IsValid)
            {
                db.Pedidos.Add(pedido);
                db.SaveChanges();
                return RedirectToAction("Listado");
            }

            ViewBag.tipoInsumo = new SelectList(db.TiposInsumo, "id", "nombre");
            ViewBag.hospitalId = new SelectList(db.Hospitales, "id", "nombre", pedido.hospitalId);
            return View(pedido);
        }

        // GET: Pedidos/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Pedido pedido = db.Pedidos.Find(id);
            if (pedido == null)
            {
                return HttpNotFound();
            }
            ViewBag.hospitalId = new SelectList(db.Hospitales, "id", "nombre", pedido.hospitalId);
            return View(pedido);
        }

        // POST: Pedidos/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,periodo,fechaGeneracion,fechaEntrega,esUrgente,estaAutorizado, hospitalId")] Pedido pedido)
        {
            if (ModelState.IsValid)
            {
                db.Entry(pedido).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Listado");
            }
            ViewBag.hospitalId = new SelectList(db.Hospitales, "id", "nombre", pedido.hospitalId);
            return View(pedido);
        }


        // GET: Pedidos/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Pedido pedido = db.Pedidos.Find(id);
            if (pedido == null)
            {
                return HttpNotFound();
            }
            return View(pedido);
        }

        // POST: Pedidos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Pedido pedido = db.Pedidos.Find(id);
            db.Pedidos.Remove(pedido);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult Autorizacion(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Pedido pedido = db.Pedidos.Find(id);
            Session["pedido"] = pedido;
            if (pedido == null)
            {
                return HttpNotFound();
            }
            return View(pedido);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Autorizacion()
        {
            Pedido pedido = Session["pedido"] as Pedido;
            //for (int i = 0; i < cantAutorizadas.Length; i++)
            //{
            //    pedido.detallesPedido.ElementAt(i).cantidadAutorizada = cantAutorizadas.GetValue(i);
            //}
            pedido.estaAutorizado = true;
                db.Entry(pedido).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Listado");
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
