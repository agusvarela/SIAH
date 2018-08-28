using SIAH.Context;
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
        public ActionResult Index(int? hospitalId)
        {
            if (hospitalId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (SIAH.Context.SIAHContext db = new Context.SIAHContext())
            {
                ViewBag.hospital = db.Hospitales.Find(hospitalId).nombre;
                return View(db.StockFarmacias.Where(s => s.hospitalId == hospitalId).Include(u => u.hospital).Include(p => p.insumo).ToList());
            }
        }
    }
}