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

        public ActionResult ListadoPedidos()
        {
            var pedidos = db.Pedidos.Include(r => r.hospital);
            var remitos = db.Remitos.Include(p => p.estado).ToList();
            var remitosConPedido = db.Remitos.Join(db.Pedidos, s=>s.pedidoId, r=> r.id, (s,r) => new {s,r }).Select(x => new { id = x.s.pedidoId }).ToList();
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

        // GET: Remitos/Create
        public ActionResult Create(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Remito remito = new Remito();
            ViewBag.remitoId = (int) id;
            ViewBag.pedidoId = (int)id;
            return View(remito);
        }



          [HttpPost]
          [ValidateAntiForgeryToken]
          public ActionResult Create(HttpPostedFileBase file, int remitoId, int pedidoId)
          {
            try
            {
                int remitoIdEnArchivo;
                if (file.ContentLength > 0)
                {
                    string _FileName = Path.GetFileName(file.FileName);
                    string _path = Path.Combine(Server.MapPath("~/UploadedFiles"), _FileName);
                    file.SaveAs(_path);
                    string lecturaArchivo;
                    using (var sr = new StreamReader(_path))
                    {
                        lecturaArchivo = sr.ReadLine();
                        var campos = lecturaArchivo.Split(';');
                        remitoIdEnArchivo = Int32.Parse(campos[0]);
                        sr.Close();
                    }
                    if (remitoIdEnArchivo == remitoId)
                    {
                        ViewBag.remitoId = remitoId;
                        ViewBag.pedidoId = pedidoId;
                        ViewBag.Message = "El archivo se cargó correctamente";
                        ViewBag.path = _path;
                        
                    }
                    else {
                        ViewBag.Message = "El id de Remito de los detalles no coincide con el Pedido que intenta cargar";
                        ViewBag.remitoId = remitoId;
                        ViewBag.pedidoId = pedidoId;
                        System.IO.File.Delete(_path);
                        return View();
                    }
                    

                }
                return View();

            }
            catch (Exception e)
            {
                ViewBag.Message = "Falló la carga del archivo, intentelo nuevamente";
                ViewBag.remitoId = remitoId;
                ViewBag.pedidoId = pedidoId;
                Console.WriteLine(e.Message);
                return View();
            }
        }
    
    
        public ActionResult CrearRemito(int remitoId, String fechaEntregaEfectiva, int pedidoId, String pathDetalles)
        {
            
            String procedimientoRemito = "INSERT INTO [dbo].[Remito] (id,fechaEntregaEfectiva,estadoId,pedidoId) VALUES ("+remitoId+", "+fechaEntregaEfectiva+", 1, "+pedidoId+")";
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
               // System.IO.File.Delete(pathDetalles); //Borrar el archivo del Servidor una vez que se cargo en la BD
                return RedirectToAction("ListadoPedidos","Remitos");
            
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

        //GET: Remitos/ControlPedidoRemito
        public ActionResult ControlPedidoRemito(int? id)
        {
            Pedido pedido = db.Pedidos.Find(id);
            return View(pedido);
        }

        //POST: Remitos/ControlPedidoRemito
        [HttpPost]
        public ActionResult ControlPedidoRemito(int id)
        {
            Remito remito = db.Remitos.Find(id);
            remito.estadoId = 2;
            db.Entry(remito).State = EntityState.Modified;
            db.SaveChanges();
            this.ActualizarStockConDetallesRemito(id);
            return RedirectToAction("ListadoPedidos", "Remitos");
        }

        public void ActualizarStockConDetallesRemito(int idPedido)
        {
            var detallesPedido = db.DetallesPedido.Include(d => d.insumo).Include(d => d.pedido).
                Where(d => d.pedidoId == idPedido).
                Select(x => new {
                    insumoId = x.insumoId,
                    cantidadAutorizada = x.cantidadAutorizada
                });
            var detallesRemito = db.DetallesRemito.Include(d => d.remito).
                Where(d => d.remitoId == idPedido).
                Select(x => new {  insumoRemitoId = x.insumoId, cantidadEntregada = x.cantidadEntregada });
            var detallesPedidoRemito = detallesPedido.Join(detallesRemito, s => s.insumoId, r => r.insumoRemitoId, (s, r) => new { s, r }).
                Select(x => new
                {
                    insumoId = x.s.insumoId,
                    cantidadAutorizada = x.s.cantidadAutorizada,
                    cantidadEntregada = x.r.cantidadEntregada
                }).ToList();
            foreach(var i in detallesPedidoRemito)
            {
                var diff = i.cantidadEntregada - i.cantidadAutorizada;
                Insumo insumo = db.Insumos.Find(i.insumoId);
                insumo.stockFisico = i.cantidadEntregada;
                if ( diff != 0)
                {
                    insumo.stock = insumo.stock - diff;
                }
                db.Entry(insumo).State = EntityState.Modified;
                db.SaveChanges();
            }
        }
        //GET: Remitos/DetallesPedidoRemito
        public JsonResult GetDetallesPedidoRemito(int idPedido)
        {
            var detallesPedido = db.DetallesPedido.Include(d => d.insumo).Include(d => d.pedido).
                Where(d => d.pedidoId == idPedido).
                Select(x => new { pedidoId = x.pedidoId, insumoId = x.insumoId, nombre = x.insumo.nombre,
                                precioUnitario = x.insumo.precioUnitario, cantidad = x.cantidad,
                                 cantidadAutorizada = x.cantidadAutorizada, tipo = x.insumo.tiposInsumo.nombre,
                                    stock = x.insumo.stock });
            var detallesRemito = db.DetallesRemito.Include(d => d.remito).
                Where(d => d.remitoId == idPedido).
                Select(x => new { remitoId = x.remitoId, insumoRemitoId = x.insumoId, cantidadEntregada = x.cantidadEntregada });
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
                    cantidadEntregada = x.r.cantidadEntregada
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
