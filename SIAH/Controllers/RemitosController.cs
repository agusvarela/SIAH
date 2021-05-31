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
using System.IO;
using System.Data.SqlClient;
using SIAH.Models.Pedidos;
using SIAH.Models.Insumos;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Mail;
using SIAH.Models;
using Newtonsoft.Json;
using SIAH.Models.Historico;

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
        [AuthorizeUserAccessLevel(UserRole = "RespAutorizacion", UserRole2 = "DirectorArea")]
        public ActionResult ListadoPedidos()
        {
            var pedidos = db.Pedidos.Include(r => r.hospital);
            var remitos = db.Remitos.Include(p => p.estado).ToList();
            var remitosConPedido = db.Remitos.Join(db.Pedidos, s => s.pedidoId, r => r.id, (s, r) => new { s, r }).Select(x => new { id = x.s.pedidoId }).ToList();
            var pedidosEstadoEntregado = pedidos.Where(x => x.estadoId >= 5).ToList();
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
        public ActionResult CargarRemito(Remito remito)
        {
            if (!db.Pedidos.Where(p => p.id.Equals(remito.pedidoId)).Any())
            {
                string errorValue = "El numero de pedido ingresado no existe en el sistema.";
                var result = Content(JsonConvert.SerializeObject(new { error = errorValue }), "application/json; charset=utf-8");
                HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return result;
            }

            var listaItems = db.DetallesPedido
                                .Where(d => d.pedidoId == remito.pedidoId)
                                .Select(x => new
                                {
                                    insumoId = x.insumoId,
                                    cantAutorizada = x.cantidadAutorizada
                                }).ToList();

            if (listaItems.Count != remito.detallesRemito.Count)
            {
                string errorValue = "La cantidad de insumos ingresadas no corresponde con la cantidad de insumos pedidos.";
                var result = Content(JsonConvert.SerializeObject(new { error = errorValue }), "application/json; charset=utf-8");
                HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return result;
            }

            remito.id = remito.pedidoId;
            remito.estadoId = 1;

            foreach (DetalleRemito detalle in remito.detallesRemito)
            {
                var item = listaItems.Find(l => l.insumoId == detalle.insumoId);

                if (item == null)
                {
                    string errorValue = "El insumoId: " + detalle.insumoId + " no existe para el número de pedido ingresado.";
                    var result = Content(JsonConvert.SerializeObject(new { error = errorValue }), "application/json; charset=utf-8");
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return result;
                }
                else if (detalle.cantidadEntregada < 0)
                {
                    string errorValue = "El insumoId: " + detalle.insumoId + " posee una canidad incorrecta.";
                    var result = Content(JsonConvert.SerializeObject(new { error = errorValue }), "application/json; charset=utf-8");
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return result;
                }

                detalle.remitoId = remito.id;
            }

            try
            {
                db.Remitos.Add(remito);
                db.SaveChanges();
                ActualizarStockSiahConDetallesRemito(remito.id, remito.fechaEntregaEfectiva);
                ActualizarStockFarmacia(remito);
                ActualizarPedido(remito.pedidoId);

                string acceptValue = "La peticion fue exitosa.";
                var result = Content(JsonConvert.SerializeObject(new { message = acceptValue }), "application/json; charset=utf-8");
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Accepted;
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                string acceptValue = "El remito ya se encuentra cargado.";
                var result = Content(JsonConvert.SerializeObject(new { error = acceptValue }), "application/json; charset=utf-8");
                HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return result;
            }
        }

        [AuthorizeUserAccessLevel(UserRole = "RespAutorizacion", UserRole2 = "DirectorArea")]
        //GET: Remitos/ControlPedidoRemito
        public ActionResult ControlPedidoRemito(int? id)
        {
            Remito remito = db.Remitos.Find(id);
            Pedido pedido = db.Pedidos.Find(id);
            var tuplaPedidoRemito = new Tuple<Pedido, Remito>(pedido, remito);
            ViewBag.hospital = db.Hospitales.Include(hospital => hospital.nombre).Where(hospital => hospital.id == pedido.hospitalId).Select(r => new { hospital = r.nombre }).First().hospital;
            return View(tuplaPedidoRemito);
        }
        [AuthorizeUserAccessLevel(UserRole = "RespAutorizacion", UserRole2 = "DirectorArea")]
        //POST: Remitos/ControlPedidoRemito
        [HttpPost]
        public ActionResult ControlPedidoRemito(int id)
        {
            Remito remito = db.Remitos.Find(id);
            changeRemitoState(remito, 2);
            var detallesRemito = db.DetallesRemito.Where(dr => dr.remitoId == id).ToList();
            actualizarPresupuestoHospitalEntregaPedido(remito.pedidoId, detallesRemito);
            return RedirectToAction("ListadoPedidos", "Remitos");
        }
        private void actualizarPresupuestoHospitalEntregaPedido(int pedidoId, ICollection<DetalleRemito> detalleRemitos)
        {
            decimal montoPedidoAutorizado = 0;
            decimal montoEntregado = 0;
            var detallesPedidos = db.DetallesPedido.Where(dp => dp.pedidoId == pedidoId).ToList();
            foreach (var detalle in detallesPedidos)
            {
                var precioInsumo = db.Insumos.Where(p => p.id == detalle.insumoId).First().precioUnitario;
                montoPedidoAutorizado += precioInsumo * detalle.cantidadAutorizada;
            }

            foreach (var detalle in detalleRemitos)
            {
                var precioInsumo = db.Insumos.Where(p => p.id == detalle.insumoId).First().precioUnitario;
                montoEntregado += precioInsumo * detalle.cantidadEntregada;
            }
            var montoAjuste = montoPedidoAutorizado - montoEntregado;
            var hospitalId = db.Pedidos.Where(p => p.id == pedidoId).First().hospitalId;
            var hospital = db.Hospitales.Where(h => h.id == hospitalId).First();
            hospital.presupuestoDisponible += montoAjuste;
            db.Entry(hospital).State = EntityState.Modified;
            db.SaveChanges();
        }
        private void ActualizarPedido(int idPedido)
        {
            var pedido = db.Pedidos.Where(p => p.id == idPedido).Include(t => t.estado).Include(d => d.detallesPedido).First();
            pedido.estadoId = 6;
            db.Entry(pedido).State = EntityState.Modified;
            db.SaveChanges();
        }

        private void ActualizarStockFarmacia(Remito remito)
        {
            var hospitalId = db.Pedidos.Where(p => p.id == remito.pedidoId).First().hospitalId;
            foreach (DetalleRemito item in remito.detallesRemito)
            {
                ActualizarItem(item, hospitalId, remito);
            }
        }

        private void ActualizarItem(DetalleRemito item, int hospitalId, Remito remito)
        {
            StockFarmacia insumo = db.StockFarmacias.Where(p => p.hospitalId == hospitalId &&
                                            p.insumoId == item.insumoId).First();
            if (insumo != null)
            {
                insumo.stockFarmacia = insumo.stockFarmacia + item.cantidadEntregada;
                db.Entry(insumo).State = EntityState.Modified;

                agregarHistoricoFarmacia(item, insumo.stockFarmacia, remito, hospitalId);
            }
            else
            {
                var newStock = new StockFarmacia();
                newStock.hospitalId = hospitalId;
                newStock.insumoId = item.insumoId;
                newStock.stockFarmacia = item.cantidadEntregada;
                db.StockFarmacias.Add(newStock);

                agregarHistoricoFarmacia(item, insumo.stockFarmacia, remito, hospitalId);
            }
        }

        private void agregarHistoricoFarmacia(DetalleRemito detalleRemito, int saldo, Remito remito, int hospitalId)
        {
            HistoricoFarmacia historicoFarmacia = new HistoricoFarmacia();
            historicoFarmacia.insumoId = detalleRemito.insumoId;
            historicoFarmacia.hospitalId = hospitalId;
            historicoFarmacia.fechaMovimiento = DateTime.UtcNow;
            historicoFarmacia.descripcion = "Se recibió una entrega del ministerio. Remito número: " + remito.pedidoId;
            historicoFarmacia.saldo = saldo;
            historicoFarmacia.isNegative = false;
            historicoFarmacia.cantidad = detalleRemito.cantidadEntregada;

            db.HistoricoFarmacia.Add(historicoFarmacia);
        }

        private void ActualizarStockSiahConDetallesRemito(int idPedido, DateTime fechaEntregaEfectiva)
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
                insumo.stock = newStock > 0 ? newStock : 0;
                db.Entry(insumo).State = EntityState.Modified;

                agregarHistoricoFisico(i.insumoId, i.cantidadEntregada, insumo.stockFisico, fechaEntregaEfectiva, idPedido);

                if (diff != 0)
                {
                    agregarHistoricoSIAH(i.insumoId, -diff, insumo.stock, fechaEntregaEfectiva, idPedido);
                }

                db.SaveChanges();
            }
        }

        private void agregarHistoricoSIAH(int insumoId, int cantidadEntregada, int saldo, DateTime fechaEntregaEfectiva, int remitoId)
        {
            HistoricoSIAH historicoSIAH = new HistoricoSIAH();
            historicoSIAH.insumoId = insumoId;
            historicoSIAH.fechaMovimiento = DateTime.UtcNow;
            historicoSIAH.descripcion = "Ajuste de entrega realizada, Remito número: " + remitoId;
            historicoSIAH.saldo = saldo;
            historicoSIAH.isNegative = false;
            historicoSIAH.cantidad = cantidadEntregada;

            db.HistoricoSIAH.Add(historicoSIAH);
        }

        private void agregarHistoricoFisico(int insumoId, int cantidadEntregada, int saldo, DateTime fechaEntregaEfectiva, int remitoId)
        {
            HistoricoFisico historicoFisico = new HistoricoFisico();
            historicoFisico.insumoId = insumoId;
            historicoFisico.fechaMovimiento = DateTime.UtcNow;
            historicoFisico.descripcion = "Entrega realizada, Remito número: " + remitoId;
            historicoFisico.saldo = saldo;
            historicoFisico.isNegative = true;
            historicoFisico.cantidad = cantidadEntregada * (-1);

            db.HistoricoFisico.Add(historicoFisico);
        }

        [AuthorizeUserAccessLevel(UserRole = "RespAutorizacion", UserRole2 = "DirectorArea")]
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

        public async Task<ActionResult> reclamar(int pedidoId)
        {
            Remito remito = db.Remitos.Find(pedidoId);
            Pedido pedido = db.Pedidos.Find(pedidoId);

            await sendEmailAsync(pedido, remito);

            changeRemitoState(remito, 3);

            return RedirectToAction("ListadoPedidos");
        }

        private async Task sendEmailAsync(Pedido pedido, Remito remito)
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
