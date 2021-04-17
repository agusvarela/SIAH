using SIAH.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SIAH.Models;
using System.Data.Entity;
using System.Net;

namespace SIAH.Controllers
{
    public class StockFarmaciaController : Controller
    {
        private SIAHContext db = new SIAHContext();

        // GET: StockFarmacia
        [AuthorizeUserAccessLevel(UserRole = "RespFarmacia")]
        public ActionResult Index(int? hospitalId, bool fromDashboard = false, string param = null)
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
            if (hospitalId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (SIAH.Context.SIAHContext db = new Context.SIAHContext())
            {
                ViewBag.fromDashboard = fromDashboard;
                ViewBag.hospital = db.Hospitales.Find(hospitalId).nombre;
                return View(db.StockFarmacias.Where(s => s.hospitalId == hospitalId).Include(u => u.hospital).Include(p => p.insumo).ToList());
            }
        }

        // POST: SubmitStock 
        [HttpPost]
        public ActionResult Index(IList<StockFarmacia> stocks)
        {
            int hospitalId = stocks.First().hospitalId;
            var oldStocks = db.StockFarmacias.Where(x => x.hospitalId == hospitalId).ToList();
            foreach (StockFarmacia stock in stocks)
            {
                var stockToModify = oldStocks.Find(x => x.hospitalId == stock.hospitalId && x.insumoId == stock.insumoId && x.stockFarmacia != stock.stockFarmacia);
                if (stockToModify != null)
                {
                    stockToModify.stockFarmacia = stock.stockFarmacia;
                    db.Entry(stockToModify).State = EntityState.Modified;
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (db.SaveChanges() > 0)
                    {
                        return RedirectToAction("Index", "StockFarmacia",
                                                new { hospitalId = hospitalId, fromDashboard=false, param = "Success" });

                    }
                }
                catch (Exception e)
                {
                    return RedirectToAction("Index", "StockFarmacia", new { hospitalId = hospitalId, fromDashboard = false, param = e.Message });

                }
            }
            return RedirectToAction("Index", "StockFarmacia", new { hospitalId = hospitalId, fromDashboard = false, param = "Ocurrio un error inesperado al enviar el pedido" });
        }

        // GET: Pedidos/Edit/5
        [AuthorizeUserAccessLevel(UserRole = "RespFarmacia")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StockFarmacia insumo = db.StockFarmacias.Where(p => p.id == id).Include(s => s.insumo).First();

            if (insumo == null)
            {
                return HttpNotFound();
            }

            return View(insumo);
        }

        // POST: Hospitals/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeUserAccessLevel(UserRole = "RespFarmacia")]
        public ActionResult Edit(int stockFarmacia, int id, int hospitalId)
        {
            StockFarmacia insumo = db.StockFarmacias.Find(id);
            insumo.stockFarmacia = stockFarmacia;
            db.Entry(insumo).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index", "StockFarmacia", new { hospitalId = hospitalId });
        }

        // GET: Historico
        [AuthorizeUserAccessLevel(UserRole = "RespFarmacia")]
        public ActionResult HistoricoFarmacia(int insumoId, int hospitalId)
        {
            ViewBag.insumo = db.Insumos.Find(insumoId).nombre;
            ViewBag.hospitalId = hospitalId;
            return View(db.HistoricoFarmacia.Where(h => h.hospitalId == hospitalId && h.insumoId == insumoId).ToList());
        }
    }
}