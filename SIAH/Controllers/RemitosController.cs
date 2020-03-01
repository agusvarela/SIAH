﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SIAH.Context;
using SIAH.Models.Remitos;
using System.IO;
using System.Data.SqlClient;
using SIAH.Models.Pedidos;
using SIAH.Models.Insumos;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Mail;
using SIAH.Models;

namespace SIAH.Controllers
{
    public class RemitosController : Controller
    {
        private SIAHContext db = new SIAHContext();

        // GET: Remitos
        public ActionResult Index()
        {
            var remitos = db.Remitos.Include(r => r.pedido);
            return View(remitos.ToList());
        }
        [AuthorizeUserAccessLevel(UserRole = "RespAutorizacion")]
        public ActionResult ListadoPedidos()
        {
            var pedidos = db.Pedidos.Include(r => r.hospital);
            var remitos = db.Remitos.Include(p => p.estado).ToList();
            var remitosConPedido = db.Remitos.Join(db.Pedidos, s => s.pedidoId, r => r.id, (s, r) => new { s, r }).Select(x => new { id = x.s.pedidoId }).ToList();
            var pedidosEstadoEntregado = pedidos.Where(x => x.estadoId == 5).ToList();
            /*foreach (var i in remitosConPedido)
            {
                pedidosEstadoEntregado.Remove(pedidosEstadoEntregado.Find(x => x.id == i.id));
            }*/

            var tuplaPedidoRemito = new Tuple<List<Pedido>, List<Remito>>(pedidosEstadoEntregado, remitos);
            return View(tuplaPedidoRemito);

        }

        // GET: Remitos/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Remito remito = db.Remitos.Find(id);
            if (remito == null)
            {
                return HttpNotFound();
            }
            return View(remito);
        }

        [HttpPost]
        public HttpResponseMessage CargarRemito(Remito remito)
        {
            try
            {
                remito.id = remito.pedidoId;
                remito.estadoId = 1;
                foreach (DetalleRemito detalle in remito.detallesRemito)
                {
                    detalle.remitoId = remito.id;
                }
                db.Remitos.Add(remito);
                db.SaveChanges();
                ActualizarStockConDetallesRemito(remito.id);
                return new HttpResponseMessage(HttpStatusCode.Accepted);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }

        [AuthorizeUserAccessLevel(UserRole = "RespAutorizacion")]
        //GET: Remitos/ControlPedidoRemito
        public ActionResult ControlPedidoRemito(int? id)
        {
            Remito remito = db.Remitos.Find(id);
            Pedido pedido = db.Pedidos.Find(id);
            var tuplaPedidoRemito = new Tuple<Pedido, Remito>(pedido, remito);
            return View(tuplaPedidoRemito);
        }
        [AuthorizeUserAccessLevel(UserRole = "RespAutorizacion")]
        //POST: Remitos/ControlPedidoRemito
        [HttpPost]
        public ActionResult ControlPedidoRemito(int id)
        {
            Remito remito = db.Remitos.Find(id);
            changeRemitoState(remito, 2);
            return RedirectToAction("ListadoPedidos", "Remitos");
        }

        private void ActualizarStockConDetallesRemito(int idPedido)
        {
            var detallesPedido = db.DetallesPedido.Include(d => d.insumo).Include(d => d.pedido).
                Where(d => d.pedidoId == idPedido).
                Select(x => new
                {
                    insumoId = x.insumoId,
                    cantidadAutorizada = x.cantidadAutorizada
                });
            var detallesRemito = db.DetallesRemito.Include(d => d.remito).
                Where(d => d.remitoId == idPedido).
                Select(x => new { insumoRemitoId = x.insumoId, cantidadEntregada = x.cantidadEntregada });
            var detallesPedidoRemito = detallesPedido.Join(detallesRemito, s => s.insumoId, r => r.insumoRemitoId, (s, r) => new { s, r }).
                Select(x => new
                {
                    insumoId = x.s.insumoId,
                    cantidadAutorizada = x.s.cantidadAutorizada,
                    cantidadEntregada = x.r.cantidadEntregada
                }).ToList();
            foreach (var i in detallesPedidoRemito)
            {
                var diff = i.cantidadEntregada - i.cantidadAutorizada;
                Insumo insumo = db.Insumos.Find(i.insumoId);
                //Check resta de stock fisico
                var newStockFisico = insumo.stockFisico - i.cantidadEntregada;
                insumo.stockFisico = newStockFisico > 0 ? newStockFisico : 0;
                //Si la diferencia existe se debe restar para sumar si fue menor o restar si fue mayor,
                //Si es 0 no modifica el stock
                var newStock = insumo.stock - diff;
                insumo.stock = newStock > 0 ? newStock: 0;
                db.Entry(insumo).State = EntityState.Modified;
                db.SaveChanges();
            }
        }
        [AuthorizeUserAccessLevel(UserRole = "RespAutorizacion")]
        //GET: Remitos/DetallesPedidoRemito
        public JsonResult GetDetallesPedidoRemito(int idPedido)
        {
            var detallesPedido = db.DetallesPedido.Include(d => d.insumo).Include(d => d.pedido).
                Where(d => d.pedidoId == idPedido).
                Select(x => new
                {
                    pedidoId = x.pedidoId,
                    insumoId = x.insumoId,
                    nombre = x.insumo.nombre,
                    precioUnitario = x.insumo.precioUnitario,
                    cantidad = x.cantidad,
                    cantidadAutorizada = x.cantidadAutorizada,
                    tipo = x.insumo.tiposInsumo.nombre,
                    stock = x.insumo.stock
                });
            var detallesRemito = db.DetallesRemito.Include(d => d.remito).
                Where(d => d.remitoId == idPedido).
                Select(x => new { remitoId = x.remitoId, insumoRemitoId = x.insumoId, cantidadEntregada = x.cantidadEntregada, observacion = x.observacion });
            var detallesPedidoRemito = detallesPedido.Join(detallesRemito, s => s.insumoId, r => r.insumoRemitoId, (s, r) => new { s, r }).
                Select(x => new
                {
                    pedidoId = x.s.pedidoId,
                    insumoId = x.s.insumoId,
                    nombre = x.s.nombre,
                    precioUnitario = x.s.precioUnitario,
                    cantidad = x.s.cantidad,
                    cantidadAutorizada = x.s.cantidadAutorizada,
                    tipo = x.s.tipo,
                    stock = x.s.stock,
                    cantidadEntregada = x.r.cantidadEntregada,
                    observacion = x.r.observacion
                });
            return Json(detallesPedidoRemito, JsonRequestBehavior.AllowGet);
        }
        // GET: Remitos/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Remito remito = db.Remitos.Find(id);
            if (remito == null)
            {
                return HttpNotFound();
            }
            ViewBag.pedidoId = new SelectList(db.Pedidos, "id", "id", remito.pedidoId);
            return View(remito);
        }

        // GET: Remitos/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Remito remito = db.Remitos.Find(id);
            if (remito == null)
            {
                return HttpNotFound();
            }
            return View(remito);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public async Task<ActionResult> reclamar(int pedidoId, string obs)
        {
            Remito remito = db.Remitos.Find(pedidoId);
            Pedido pedido = db.Pedidos.Find(pedidoId);

            await sendEmailAsync(pedido, remito, obs);

            changeRemitoState(remito, 3);

            return RedirectToAction("ListadoPedidos");
        }

        private async Task sendEmailAsync(Pedido pedido, Remito remito, string obs)
        {
            var message = new MailMessage();
            message.To.Add(new MailAddress("ocasa.reclamos@gmail.com"));
            message.From = new MailAddress("siah.reclamos@gmail.com");
            message.Subject = string.Format("[RECLAMO SIAH] El Remito nro {0} se encuentra con inconsistencias", remito.id);
            string body = string.Empty;
            using (StreamReader reader = new StreamReader(Server.MapPath("../Views/Shared/EmailReclamo.html")))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{obs}", obs);
            body = body.Replace("{remitoId}", remito.id.ToString());
            body = body.Replace("{pedidoId}", pedido.id.ToString());
            Hospital hospital = db.Hospitales.Find(pedido.hospitalId);
            body = body.Replace("{hospitalName}", hospital.nombre);
            //body = body.Replace("{tn}", pedido.trackingNumber.ToString());

            message.Body = body;

            message.IsBodyHtml = true;

            using (var smtp = new SmtpClient())
            {
                await smtp.SendMailAsync(message);

            }
        }

        private Remito changeRemitoState(Remito remito, int estadoId)
        {
            remito.estadoId = estadoId;
            db.Entry(remito).State = EntityState.Modified;
            db.SaveChanges();

            return remito;
        }

    }
}
