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

        // GET: Remitos/Create
        public ActionResult Create(/*int pedidoId*/)
        {
           // ViewBag.pedidoId = pedidoId;
            return View();
        }



        // POST: Remitos/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        /*  [HttpPost]
          [ValidateAntiForgeryToken]
          public ActionResult Create([Bind(Include = "id,fechaEntregaEfectiva,pedidoId")] Remito remito)
          {
              if (ModelState.IsValid)
              {
                  db.Remitos.Add(remito);
                  db.SaveChanges();
                  return RedirectToAction("Index");
              }

              ViewBag.pedidoId = new SelectList(db.Pedidos, "id", "id", remito.pedidoId);
              return View(remito);
          }*/
    
        public ActionResult CrearRemito(int remitoId, String fechaEntregaEfectiva, int pedidoId, String pathDetalles)
        {
            
            String procedimientoRemito = "INSERT INTO [dbo].[Remito] (id,fechaEntregaEfectiva,pedidoId) VALUES ("+remitoId+", "+fechaEntregaEfectiva+", "+pedidoId+")";
                String procedimientoDetalles;
                using (var sr = new StreamReader(@"C:/Tesis/SIAH/SIAH/UploadedFiles/ProcedimientoAlmacenadoDetallesRemito.sql"))
                {
                     procedimientoDetalles = sr.ReadToEnd();
                    sr.Close();
                }
                procedimientoDetalles = procedimientoDetalles.Replace("PATH", pathDetalles);
           // procedimientoDetalles.Replace("'PATH'\r\n", "'" + pathDetalles + "'\r\n");
            string sqlConnectionString = "Data Source=DESKTOP-QAVKP3R;Initial Catalog=SIAHConnection;Integrated Security=True";
                SqlConnection conn = new SqlConnection(sqlConnectionString);
                conn.Open();
                SqlCommand cm1 = new SqlCommand(procedimientoRemito, conn);
                cm1.ExecuteNonQuery();
                SqlCommand cm2 = new SqlCommand(procedimientoDetalles, conn);
                cm2.ExecuteNonQuery();
                conn.Close();

                return RedirectToAction("Index","Home");
            
        }

        private DateTime parseFecha(String fecha)
        {
            var fechaArray = fecha.Split('-');
            var y1 = Int32.Parse(fechaArray[0]);
            var m1 = Int32.Parse(fechaArray[1]);
            var d1 = Int32.Parse(fechaArray[2]);
            DateTime fechaParseada = new DateTime(d1, m1, y1);
            return fechaParseada;
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

        // POST: Remitos/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,fechaEntregaEfectiva,pedidoId")] Remito remito)
        {
            if (ModelState.IsValid)
            {
                db.Entry(remito).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
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

        // POST: Remitos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Remito remito = db.Remitos.Find(id);
            db.Remitos.Remove(remito);
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
