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
using System.Collections;

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

        // GET: DetallesPedido/ReporteConsolidado
        public ActionResult ReporteConsolidado()
        {
            var datos = this.GenerarReporte();
            return View(datos);
            //return View();
        }

        // GET: DetallesPedido/GenerarReporte
        [AllowAnonymousAttribute]
        public IEnumerable<String[]> GenerarReporte()
        {
            var result = db.DetallesPedido.Join(db.Insumos, d => d.insumoId, s => s.id, (d, s) => new { d, s }).
                Join(db.Pedidos, x => x.d.pedidoId, p => p.id, (x, p) => new { x, p }).
                Join(db.Hospitales, t => t.p.hospitalId, h => h.id, (t, h) => new { t, h }).
               // Para mostrar el total
               //GroupBy(x => x.t.x.s.nombre, x => x.t.x.d.cantidad, (key, g) => new { Insumo = key, Total = g.Sum() }).
               GroupBy(x => new { hospital = x.h.nombre, insumo = x.t.x.s.nombre }, x => x.t.x.d.cantidad, (key, g) => new { Hospital = key.hospital, Insumo = key.insumo, Cantidad = g.Sum() }).
               ToList();

            var rows = db.Insumos.ToList().Count() + 1;
            var columns = db.Hospitales.Where(h => h.Pedidos.Count > 0).Count() + 1;

            String[,] report = new String[rows,columns];
            report[0, 0] = "Insumo";
           /* var hospitales = db.Hospitales.Select(x => new { x.nombre }).ToList();
            var p = 1;
            foreach( var h in hospitales) { report[0, p] = h.nombre; p++; }*/

            var insumos = db.Insumos.Select(x => new { x.nombre }).ToList();
            var q = 1;
            foreach (var s in insumos){report[q, 0] = s.nombre; q++;}
            
            var j = 0;
            foreach (var r in result)
            {
                if(j < columns)
                { 
                if (report[0, j] != r.Hospital)
                {
                    j++;
                    report[0, j] = r.Hospital;
                    for (var i = 1; i < rows; i++)
                    {
                        if (report[i, 0] == r.Insumo)
                        {
                            report[i, j] = r.Cantidad.ToString();
                        }
                            else {
                                if (report[i, j] == null) { report[i, j] = "0"; }
                            }

                        }
                }
                else
                {
                    for (var i = 1; i < rows; i++)
                    {
                            
                        if (report[i, 0] == r.Insumo)
                        {
                            report[i, j] = r.Cantidad.ToString();
                        }
                            else
                            {
                                if (report[i, j] == null) { report[i, j] = "0"; }
                            }
                        }
                }
            }
                
            }
            //return Json(result, JsonRequestBehavior.AllowGet);
            String[][] jagged = new String[report.GetLength(0)][];

            for (int i = 0; i < report.GetLength(0); i++)
            {
                jagged[i] = new String[report.GetLength(1)];
                for (int t = 0; t < report.GetLength(1); t++)
                {
                    jagged[i][t] = report[i,t];
                }
            }

            List<String[]> list = jagged.ToList();
           // List<Object> final = repor.Cast<Object>().ToList();
            return list;
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
