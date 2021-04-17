using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SIAH.Context;
using SIAH.Models.AjusteSIAH;

namespace SIAH.Controllers
{
    public class AjusteSIAHsController : Controller
    {
        private SIAHContext db = new SIAHContext();

        // GET: AjusteSIAHs
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
            var ajusteSIAHs = db.AjusteSIAHs.Include(a => a.usuario);
            return View(ajusteSIAHs.OrderByDescending(o => o.id).ToList());
        }

        //GET: AjustesSIAHs/DetallesAjuste
        public JsonResult GetDetalles(int idAjuste)
        {
            var detallesAjuste = db.DetalleAjusteSIAHs.Include(d => d.insumo).Include(d => d.ajuste).Where(d => d.ajusteId == idAjuste)
                                .Select(x => new
                                {
                                    ajusteId = x.ajusteId,
                                    insumoId = x.insumoId,
                                    nombre = x.insumo.nombre,
                                    cantidad = x.cantidad,
                                    justificacion = x.info,
                                    tipo = x.insumo.tiposInsumo.nombre,
                                });

            return Json(detallesAjuste, JsonRequestBehavior.AllowGet);
        }
        // GET: AjusteSIAHs/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AjusteSIAH ajusteSIAH = db.AjusteSIAHs.Include(x => x.usuario).Where(x => x.id == id).FirstOrDefault();
            if (ajusteSIAH == null)
            {
                return HttpNotFound();
            }
            return View(ajusteSIAH);
        }

        // GET: AjusteSIAHs/Create
        public ActionResult Create()
        {
            ViewBag.tipoInsumo = new SelectList(db.TiposInsumo.OrderBy(tipo => tipo.nombre), "id", "nombre");
            ViewBag.hospitalId = new SelectList(db.Hospitales, "id", "nombre");
            ViewBag.usuarioId = new SelectList(db.UserAccounts, "id", "nombre");
            return View();
        }

        // POST: AjusteSIAHs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,fechaGeneracion,info,usuarioId,detallesAjuste")] AjusteSIAH ajusteSIAH)
        {
            foreach (var detalle in ajusteSIAH.detallesAjuste)
            {
                detalle.insumo = null;
                var insumo = db.Insumos.Find(detalle.insumoId);
                // Si es negativo se resta, si es positivo se suma
                insumo.stock = insumo.stock + detalle.cantidad;
                insumo.stockFisico = insumo.stockFisico + detalle.cantidad;

                db.Entry(insumo).State = EntityState.Modified;

            }
            if (ModelState.IsValid)
            {
                try
                {
                    db.AjusteSIAHs.Add(ajusteSIAH);
                    // Guardar el registro en la DB
                    if (db.SaveChanges() > 0)
                    {
                        return RedirectToAction("Index", new { param = "Success" });

                    }
                    return RedirectToAction("Index");
                }
                catch (Exception e)
                {
                    return RedirectToAction("Index", new { param = "Ocurrio un error inesperado al enviar el registro" });

                }
            }

            ViewBag.usuarioId = new SelectList(db.UserAccounts, "id", "nombre", ajusteSIAH.usuarioId);
            return View(ajusteSIAH);
        }

        // GET: AjusteSIAHs/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AjusteSIAH ajusteSIAH = db.AjusteSIAHs.Find(id);
            if (ajusteSIAH == null)
            {
                return HttpNotFound();
            }
            ViewBag.usuarioId = new SelectList(db.UserAccounts, "id", "nombre", ajusteSIAH.usuarioId);
            return View(ajusteSIAH);
        }

        // POST: AjusteSIAHs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,fechaGeneracion,info,usuarioId")] AjusteSIAH ajusteSIAH)
        {
            if (ModelState.IsValid)
            {
                db.Entry(ajusteSIAH).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.usuarioId = new SelectList(db.UserAccounts, "id", "nombre", ajusteSIAH.usuarioId);
            return View(ajusteSIAH);
        }

        // GET: AjusteSIAHs/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AjusteSIAH ajusteSIAH = db.AjusteSIAHs.Find(id);
            if (ajusteSIAH == null)
            {
                return HttpNotFound();
            }
            return View(ajusteSIAH);
        }

        // POST: AjusteSIAHs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            AjusteSIAH ajusteSIAH = db.AjusteSIAHs.Find(id);
            db.AjusteSIAHs.Remove(ajusteSIAH);
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
