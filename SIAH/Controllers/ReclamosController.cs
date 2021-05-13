using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using SIAH.Context;
using SIAH.Models;
using SIAH.Models.Historico;
using SIAH.Models.Insumos;
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
            List<DetallesReclamoDTO> detallesReclamo = db.DetallesReclamo.
                Include(x => x.insumo).Include("insumo.tiposInsumo").Include(x => x.tipoReclamo).
                Where(x => x.pedidoId == reclamo.pedidoId).
                Select(x => new DetallesReclamoDTO
                {
                    nombre = x.insumo.nombre,
                    tipo = x.insumo.tiposInsumo.nombre,
                    cantidad = x.cantidad,
                    precioUnitario = x.insumo.precioUnitario,
                    insumoId = x.insumoId,
                    tipoReclamo = x.tipoReclamo.tipo,
                    observacion = x.observacion,
                    accepted = x.accepted,
                    respuesta = x.respuesta,
                }).ToList();

            ViewBag.DetallesReclamo = detallesReclamo;
            return View(reclamo);
        }

        // GET: Reclamos/Create
        public ActionResult Create(int? pedidoId, int? hospitalId)
        {
            if (pedidoId == null || hospitalId == null)
            {
                return HttpNotFound();
            }
            ViewBag.pedidoId = pedidoId;
            ViewBag.estadoReclamoId = new SelectList(db.EstadoReclamoes, "id", "nombreEstado");
            ViewBag.hospital = db.Hospitales.Where(p => p.id == hospitalId).First().nombre;
            ViewBag.hospitalId = hospitalId;
            ViewBag.responsableAsignadoId = new SelectList(db.UserAccounts, "id", "nombre");
            ViewBag.tipoReclamoId = new SelectList(db.TipoReclamoes, "id", "tipo");
            var detallesRemito = db.DetallesRemito.Include(d => d.remito).
               Where(d => d.remitoId == pedidoId).
               Select(x => new { insumoRemitoId = x.insumoId, cantidadEntregada = x.cantidadEntregada });
            var detallesPedido = db.DetallesPedido.Include(x => x.insumo).Include("insumo.tiposInsumo").Where(x => x.pedidoId == pedidoId)
            .Select(x => new DetallesReclamoDTO
            {
                nombre = x.insumo.nombre,
                tipo = x.insumo.tiposInsumo.nombre,
                cantidad = x.cantidad,
                precioUnitario = x.insumo.precioUnitario,
                insumoId = x.insumoId,
            });
            var detallesPedidoRemito = detallesPedido.Join(detallesRemito, s => s.insumoId, r => r.insumoRemitoId, (s, r) => new { s, r }).
                Select(x => new DetallesReclamoDTO
                {
                    nombre = x.s.nombre,
                    tipo = x.s.tipo,
                    cantidad = x.r.cantidadEntregada,
                    precioUnitario = x.s.precioUnitario,
                    insumoId = x.s.insumoId,
                }).ToList();
            ViewBag.detallesPedido = detallesPedidoRemito;
            return View();
        }

        public class DetallesReclamoDTO
        {
            public string nombre { get; set; }
            public string tipo { get; set; }
            public int cantidad { get; set; }
            public int insumoId { get; set; }
            public decimal precioUnitario { get; set; }
            public string tipoReclamo { get; set; }
            public string observacion { get; set; }
            public string respuesta { get; set; }
            public bool accepted { get; set; }
        }

        // POST: Reclamos/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "id,observacionFamacia,fechaInicioReclamo,tipoReclamoId,pedidoId,hospitalId,estadoReclamoId,detallesReclamo")] Reclamo reclamo)
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
                reclamo.fechaInicioReclamo = DateTime.Now;
                reclamo.responsableAsignadoId = null;

                impactarStockFarmaciaYPresupuesto(reclamo);

                //Guardar toda la transacción en DB
                db.SaveChanges();
                await SendEmailReclamo(reclamo);
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

        private void impactarStockFarmaciaYPresupuesto(Reclamo reclamo)
        {
            decimal montoReclamado = 0;

            foreach (DetalleReclamo detalleReclamo in reclamo.detallesReclamo)
            {
                StockFarmacia stockFarmacia  = db.StockFarmacias.Where(s => s.insumoId == detalleReclamo.insumoId && s.hospitalId == reclamo.hospitalId)
                    .FirstOrDefault();

                stockFarmacia.stockFarmacia -= detalleReclamo.cantidad;
                db.Entry(stockFarmacia).State = EntityState.Modified;
                var precioInsumo = db.Insumos.Where(p => p.id == detalleReclamo.insumoId).First().precioUnitario;
                montoReclamado += precioInsumo*detalleReclamo.cantidad;
                agregarHistoricoFarmacia(detalleReclamo, reclamo.fechaInicioReclamo, reclamo.pedidoId, stockFarmacia.stockFarmacia, reclamo.hospitalId);
            }
            var hospital = db.Hospitales.Where(h => h.id == reclamo.hospitalId).First();
            hospital.presupuestoDisponible += montoReclamado;
            db.Entry(hospital).State = EntityState.Modified;
            db.SaveChanges();

        }

        private void agregarHistoricoFarmacia(DetalleReclamo detalleReclamo, DateTime fechaInicioReclamo, int pedidoId, int saldo, int hospitalId)
        {
            HistoricoFarmacia historicoFarmacia = new HistoricoFarmacia();
            historicoFarmacia.insumoId = detalleReclamo.insumoId;
            historicoFarmacia.hospitalId = hospitalId;
            historicoFarmacia.fechaMovimiento = fechaInicioReclamo;
            historicoFarmacia.descripcion = "Se realizó un reclamo, Pedido número: " + pedidoId;
            historicoFarmacia.saldo = saldo;
            historicoFarmacia.isNegative = true;
            historicoFarmacia.cantidad = detalleReclamo.cantidad * (-1);

            db.HistoricoFarmacia.Add(historicoFarmacia);
        }

        private async Task SendEmailReclamo(Reclamo reclamo)
        {
            var users = db.UserAccounts.Where(ua => ua.rolID == 2 || ua.rolID == 3).ToList();
            var messages = new List<MailMessage>();
            foreach (var user in users)
            {
                var message = new MailMessage();
                message.To.Add(new MailAddress(user.email));
                message.From = new MailAddress("siah.reclamos@gmail.com");
                message.Subject = string.Format("[SIAH] Se generó un reclamo para el pedido N°{0}", reclamo.pedidoId);
                string body = string.Empty;
                using (StreamReader reader = new StreamReader(Server.MapPath("~/Views/Shared/EmailReclamoPedido.html")))
                {
                    body = reader.ReadToEnd();
                }
                body = body.Replace("{pedidoId}", reclamo.pedidoId.ToString());
                var hospital = db.Hospitales.Find(reclamo.hospitalId);
                body = body.Replace("{hospital}", hospital.nombre.ToString());
                message.Body = body;
                message.IsBodyHtml = true;
                messages.Add(message);
            }

            using (var smtp = new SmtpClient())
            {
                foreach (var message in messages)
                {
                    await smtp.SendMailAsync(message);
                }
            }
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

        public ActionResult Resolucion(int? id)
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
            List<DetallesReclamoDTO> detallesReclamo = db.DetallesReclamo.
                Include(x => x.insumo).Include("insumo.tiposInsumo").Include(x => x.tipoReclamo).
                Where(x => x.pedidoId == reclamo.pedidoId).
                Select(x => new DetallesReclamoDTO
                {
                    nombre = x.insumo.nombre,
                    tipo = x.insumo.tiposInsumo.nombre,
                    cantidad = x.cantidad,
                    precioUnitario = x.insumo.precioUnitario,
                    insumoId = x.insumoId,
                    tipoReclamo = x.tipoReclamo.tipo,
                    observacion = x.observacion,
                    accepted = x.accepted,
                }).ToList();

            ViewBag.DetallesReclamo = detallesReclamo;
            return View(reclamo);
        }

        //POST: Reclamos/Resolucion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Resolucion([Bind(Include = "observacionFamacia,respuesta,fechaInicioReclamo,fechaFinReclamo,tipoReclamoId,pedidoId,hospitalId,responsableAsignadoId,detallesReclamo")] Reclamo reclamo)
        {
            if (ModelState.IsValid)
            {
                reclamo.estadoReclamoId = 4; //Resuelto
                //reclamo.fechaFinReclamo = DateTime.Now;
                Pedido p = db.Pedidos.Find(reclamo.pedidoId);
                p.estadoId = 8; //Reclamo Resuelto
                foreach(var detalle in reclamo.detallesReclamo)
                {
                    var rowDetalle = db.DetallesReclamo.Where(x => x.insumoId == detalle.insumoId && x.pedidoId == detalle.pedidoId).FirstOrDefault();
                    rowDetalle.respuesta = detalle.respuesta;
                    rowDetalle.accepted = detalle.accepted;
                    db.Entry(rowDetalle).State = EntityState.Modified;
                }
                reclamo.detallesReclamo = null;
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
                .Select(x => new { IdReclamo = x.pedidoId, Tipo = x.tipoReclamo.tipo, Hospital = x.hospital.nombre, FechaInicioReclamo = x.fechaInicioReclamo, FechaFinReclamo = x.fechaFinReclamo })
                .ToList().Select(x => new
                {
                    IdReclamo = x.IdReclamo,
                    Tipo = x.Tipo,
                    Hospital = x.Hospital,
                    FechaInicioReclamo = string.Format("{0:dd/MM/yyyy}", x.FechaInicioReclamo),
                    FechaFinReclamo = string.Format("{0:dd/MM/yyyy}", x.FechaFinReclamo),
                    EstaResuelto = IsResolved(x.IdReclamo),
                    Periodo = string.Format("{0:MM/dd/yyyy}", new DateTime(x.FechaInicioReclamo.Year, x.FechaInicioReclamo.Month, 1))
                }); ;
            return Json(dataset, JsonRequestBehavior.AllowGet);
        }

        private bool IsResolved(int idReclamo)
        {
            Reclamo reclamo = db.Reclamoes.Include(x => x.estadoReclamo).Where(x => x.pedidoId == idReclamo).First();
            return reclamo.estadoReclamo.isFinal; //The only final state is resolved
        }
    }
}
