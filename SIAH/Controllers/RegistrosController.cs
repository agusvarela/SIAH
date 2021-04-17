using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SIAH.Context;
using SIAH.Models.Historico;
using SIAH.Models.Registro;

namespace SIAH.Controllers
{
    public class RegistrosController : Controller
    {
        private SIAHContext db = new SIAHContext();

        [AuthorizeUserAccessLevel(UserRole = "RespFarmacia")]
        // GET: Registros
        public ActionResult Index(string param)
        {
            if (param != null)
            {
                if (param.CompareTo("Success") == 0)
                {
                    ViewBag.success = true;
                }
                else
                {
                    ViewBag.success = false;
                    ViewBag.problem = param;
                };
            }
            var hospitalActual = Int32.Parse(Session["hospitalId"].ToString());
            var registros = db.Registros.Where(r => r.hospitalId == hospitalActual).Include(p => p.hospital);
            return View(registros.OrderByDescending(o => o.id).ToList());
        }

        //GET: Registros/DetallesRegistro
        public JsonResult GetDetalles(int idRegistro)
        {
            var idHospital = db.Registros.Find(idRegistro).hospitalId;
            var detallesPedido = db.DetallesRegistro.Include(d => d.insumo).Include(d => d.registro).Where(d => d.registroId == idRegistro)
                                .Select(x => new
                                {
                                    registroId = x.registroId,
                                    insumoId = x.insumoId,
                                    nombre = x.insumo.nombre,
                                    cantidad = x.cantidad,
                                    tipo = x.insumo.tiposInsumo.nombre,
                                });

            return Json(detallesPedido, JsonRequestBehavior.AllowGet);
        }

        [AuthorizeUserAccessLevel(UserRole = "RespFarmacia")]
        // GET: Registros/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Registro registro = db.Registros.Find(id);
            ViewBag.hospital = db.Registros.Include(r => r.hospital).First(x => x.id == id).hospital.nombre;
            var queryName = db.Registros.Include(r => r.usuario).First(x => x.id == id).usuario;
            ViewBag.userName = queryName.nombre + " " + queryName.apellido;
            if (registro == null)
            {
                return HttpNotFound();
            }
            return View(registro);
        }

        [AuthorizeUserAccessLevel(UserRole = "RespFarmacia")]
        // GET: Registros/Create
        public ActionResult Create()
        {
            ViewBag.tipoInsumo = new SelectList(db.TiposInsumo.OrderBy(tipo => tipo.nombre), "id", "nombre");
            ViewBag.hospitalId = new SelectList(db.Hospitales, "id", "nombre");
            ViewBag.usuarioId = new SelectList(db.UserAccounts, "id", "nombre");
            return View();
        }

        // POST: Registros/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,fechaGeneracion,destinatario,usuarioId,hospitalId,detallesRegistro")] Registro registro)
        {
            foreach (var detalle in registro.detallesRegistro)
            {
                detalle.insumo = null;
            }

            if (ModelState.IsValid)
            {
                db.Registros.Add(registro); 

                try
                {
                    // Actualizar el stock
                    ActualizarStockFarmacia(registro.hospitalId, registro.detallesRegistro, registro);
                    // Guardar el registro en la DB
                    if (db.SaveChanges() > 0)
                    {
                        return RedirectToAction("Index", new { param = "Success" });

                    }
                }
                catch (Exception e)
                {
                    return RedirectToAction("Index", new { param = "Ocurrio un error inesperado al enviar el registro" });

                }
            }
            return RedirectToAction("Index", new { param = "Ocurrio un error inesperado al enviar el registro" });

        }

        private void ActualizarStockFarmacia(int idHospital, ICollection<DetalleRegistro> detalles, Registro registro)
        {
            foreach(var detalle in detalles)
            {
                // Buscar el stock farmacia de ese hospital y ese insumo
                var stockFarmacia = db.StockFarmacias.Where(x => x.hospitalId == idHospital && x.insumoId == detalle.insumoId).FirstOrDefault();
                if (stockFarmacia != null)
                {
                    stockFarmacia.stockFarmacia -= detalle.cantidad;
                    if(stockFarmacia.stockFarmacia < 0)
                    {
                        stockFarmacia.stockFarmacia = 0;
                    }
                    db.Entry(stockFarmacia).State = EntityState.Modified;

                    agregarHistorico(detalle, stockFarmacia.stockFarmacia, registro);
                }
            }
        }

        private void agregarHistorico(DetalleRegistro detalleRegistro, int saldo, Registro registro)
        {
            HistoricoFarmacia historicoFarmacia = new HistoricoFarmacia();
            historicoFarmacia.insumoId = detalleRegistro.insumoId;
            historicoFarmacia.fechaMovimiento = registro.fechaGeneracion;
            historicoFarmacia.descripcion = "Entrega de uso a: " + registro.destinatario;
            historicoFarmacia.saldo = saldo;
            historicoFarmacia.isNegative = detalleRegistro.isNegative;
            historicoFarmacia.cantidad = detalleRegistro.cantidad;

            db.HistoricoFarmacia.Add(historicoFarmacia);
        }

        // GET: Registros/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Registro registro = db.Registros.Find(id);
            if (registro == null)
            {
                return HttpNotFound();
            }
            ViewBag.hospitalId = new SelectList(db.Hospitales, "id", "nombre", registro.hospitalId);
            ViewBag.usuarioId = new SelectList(db.UserAccounts, "id", "nombre", registro.usuarioId);
            return View(registro);
        }

        // POST: Registros/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,fechaGeneracion,destinatario,usuarioId,hospitalId")] Registro registro)
        {
            if (ModelState.IsValid)
            {
                db.Entry(registro).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.hospitalId = new SelectList(db.Hospitales, "id", "nombre", registro.hospitalId);
            ViewBag.usuarioId = new SelectList(db.UserAccounts, "id", "nombre", registro.usuarioId);
            return View(registro);
        }

        // GET: Registros/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Registro registro = db.Registros.Find(id);
            if (registro == null)
            {
                return HttpNotFound();
            }
            return View(registro);
        }

        // POST: Registros/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Registro registro = db.Registros.Find(id);
            db.Registros.Remove(registro);
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
