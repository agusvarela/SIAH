﻿using SIAH.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SIAH.Models;
using SIAH.Context;
using System.Data.Entity;
using System.Net;

namespace SIAH.Controllers
{
    public class StockFarmaciaController : Controller
    {
        private SIAHContext db = new SIAHContext();

        // GET: StockFarmacia
        [AuthorizeUserAccessLevel(UserRole = "RespFarmacia")]
        public ActionResult Index(int? hospitalId, bool vieneDelDashboard)
        {
            if (hospitalId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (SIAH.Context.SIAHContext db = new Context.SIAHContext())
            {
                ViewBag.vieneDelDashboard = vieneDelDashboard; 
                ViewBag.hospital = db.Hospitales.Find(hospitalId).nombre;
                return View(db.StockFarmacias.Where(s => s.hospitalId == hospitalId).Include(u => u.hospital).Include(p => p.insumo).ToList());
            }
        }

        // POST: SubmitStock 
        [HttpPost]
        public ActionResult Index(IList<StockFarmacia> stocks)
        {
            foreach(StockFarmacia stock in stocks)
            {
                var stockToModify = db.StockFarmacias.Where(x => x.hospitalId == stock.hospitalId && x.insumoId == stock.insumoId).FirstOrDefault();
                stockToModify.stockFarmacia = stock.stockFarmacia;
                db.Entry(stockToModify).State = EntityState.Modified;
            }

            db.SaveChanges();
            return RedirectToAction("RespFarmaciaDashboard", "Home"); //TODO: Add success param
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
    }
}