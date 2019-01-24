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
using SIAH.Models.Insumos;
using SIAH.Models;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
namespace SIAH.Controllers
{
    public class PedidosController : Controller
    {
        private SIAHContext db = new SIAHContext();
        private static readonly HttpClient client = new HttpClient();

        //POST: EntregaPedido
        public HttpResponseMessage EntregaPedido(int? id)
        {
            HttpResponseMessage response;
            if (id == null)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
            try
            {
                var pedido = db.Pedidos.Where(p => p.id == id).Include(t => t.estado).Include(d => d.detallesPedido).First();
                if(pedido.estadoId == 3)
                {
                    pedido.estadoId = 6;
                    foreach (DetallePedido item in pedido.detallesPedido)
                    {
                        StockFarmacia insumo = db.StockFarmacias.Where(p => p.hospitalId == pedido.hospitalId && 
                                                        p.insumoId == item.insumoId).First();
                        if(insumo != null)
                        {
                            insumo.stockFarmacia = insumo.stockFarmacia + item.cantidadAutorizada;
                            db.Entry(insumo).State = EntityState.Modified;
                        }
                        //TODO: Si el insumo no existe se deberia insertar en la BD
                    }
                    db.Entry(pedido).State = EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    response = new HttpResponseMessage(HttpStatusCode.NotAcceptable);
                    /*response.Content = new StringContent("El pedido ingresado debe estar en estado " +
                        "En Proceso de Envío y el estado del pedido ingresado es " + pedido.estado.nombreEstado);*/
                    return response;
                }
            } catch (Exception e)
            {
                response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                //response.Content = new StringContent("Ocurrió un error al intentar realizar el cambio de estado");
                return response;
            }
            response = new HttpResponseMessage(HttpStatusCode.Accepted);
            //response.Content = new StringContent("El pedido cambió correctamente a estado Entregado");
            return response;
        }

        // POST: Pedidos/Cancelar
        public ActionResult Cancelar(int? pedidoId)
        {
            if (pedidoId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            try
            {

                var pedido = db.Pedidos.Where(p => p.id == pedidoId).First();
                pedido.estadoId = 4;
                db.Entry(pedido).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch (Exception e)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            return RedirectToAction("Listado", "Pedidos");
        }

        // POST: Pedidos/Recibido
        public ActionResult Recibido(int? pedidoId)
        {
            if (pedidoId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            try
            {

                var pedido = db.Pedidos.Where(p => p.id == pedidoId).First();
                pedido.estadoId = 5;
                db.Entry(pedido).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch (Exception e)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            return RedirectToAction("RespFarmacia", "Pedidos");
        }

        // GET: Pedidos
        [AuthorizeUserAccessLevel(UserRole = "RespFarmacia")]
        public ActionResult Index()
        {
            var pedidos = db.Pedidos.Include(p => p.hospital);
            return View(pedidos.ToList());
        }

        // GET: Pedidos/Details/5
        [AuthorizeUserAccessLevel(UserRole = "RespFarmacia", UserRole2 = "RespAutorizacion", UserRole3 = "DirectorArea")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //ViewBag.hospitalId = new SelectList(db.Hospitales, "id", "presupuesto");
            ViewBag.estado = db.Pedidos.Include(p => p.estado).Where(x => x.id == id).Select(r => new { estado = r.estado.nombreEstado }).First().estado;
            Pedido pedido = db.Pedidos.Find(id);
            if (pedido == null)
            {
                return HttpNotFound();
            }
            return View(pedido);
        }

        //POST: Pedidos/GenerateSendOcasa
        public ActionResult GenerateSendOcasa()
        {
            var detalles = db.DetallesPedido.Include(p => p.pedido).Where(x => x.pedido.estadoId == 2);
            //Si no hay Pedidos en estado Autorizado, no se debe cambiar de estado nada ni enviar a OCASA 
            if (!detalles.Any())
            {
                return RedirectToAction("Listado", "Pedidos");
            }
            StringWriter csv = new StringWriter();
            csv.WriteLine(string.Format("{0},{1},{2},{3}", "Pedido", "Insumo", "Cantidad", "Hospital"));
            foreach (var detalle in detalles.ToList())
            {
                csv.WriteLine(string.Format("{0},{1},{2},{3}", detalle.pedidoId, detalle.insumoId, detalle.cantidadAutorizada,detalle.pedido.hospitalId));
            }
            //Body que se enviará a OCASA con los pedidos en formato CSV (podria ser JSON pero no es algo prioritario)
            var values = new Dictionary<string, string>
            {
               { "Pedidos", csv.ToString()}
            };
            var content = new FormUrlEncodedContent(values);
            //LLamada a API ficticia que devuelve siempre un 200 (OK)
            var response = client.PostAsync("https://1cb7e13b-c3ac-4278-902d-2d4ae786f363.mock.pstmn.io/uploadPedido", content);
            if (response.Result.IsSuccessStatusCode)
            {
                try
                {
                    foreach (var i in detalles.Select(x => x.pedidoId).ToList())
                    {
                        Pedido pedido = db.Pedidos.Find(i);
                        pedido.estadoId = 3;
                        db.Entry(pedido).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                    return RedirectToAction("Listado", "Pedidos", new {param="Success"});
                }
                catch (Exception e)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, e.Message);
                }
            }
            //Si la respuesta de la API da cualquier resultado que no sea Success, se vuelve a la pagina con el mensaje de error
            return RedirectToAction("Listado", "Pedidos", new { param = "Hubo un problema inesperado" });
        }

        //GET: Pedidos/GetHospital
        public /*List<String>*/ JsonResult GetHospital(int? hospitalId)
        {
            List<String> data = new List<string>();
            Hospital hospitalData = db.Hospitales.Find(hospitalId);
            String hospital = hospitalData.nombre;
            String presupuesto = hospitalData.presupuesto.ToString();
            data.Add(hospital);
            data.Add(presupuesto);
            //return data;
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // GET: Pedidos/Create
        [AuthorizeUserAccessLevel(UserRole = "RespFarmacia")]
        public ActionResult Create()
        {
            ViewBag.tipoInsumo = new SelectList(db.TiposInsumo, "id", "nombre");
            //ViewBag.hospitalId = new SelectList(db.Hospitales, "id", "nombre");
            return View(new Pedido());
        }

        // GET: Pedidos/Listado
        //public ActionResult Listado()
        //{
        //    var pedidos = db.Pedidos.Include(p => p.hospital);
        //    return View(pedidos.ToList());
        //}
      
        [AuthorizeUserAccessLevel (UserRole = "RespAutorizacion", UserRole2 = "DirectorArea")]
        public ActionResult Listado(String param)
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

                var pedidos = db.Pedidos.Include(p => p.hospital); // OrderBy(o=>o.fechaGeneracion);
                return View(pedidos.OrderByDescending(p => p.id).ToList());
            }
            else
            {
                var pedidos = db.Pedidos.Include(p => p.hospital); // OrderBy(o => o.fechaGeneracion);
                return View(pedidos.OrderByDescending(p => p.id).ToList());
            }


        }

        [AuthorizeUserAccessLevel(UserRole = "RespAutorizacion",UserRole2 = "DirectorArea")]
        [ActionName("OrdenFecha")]
        public ActionResult Listado(string sortOrder, Boolean? b)
        {
            if (Session["fechaGen"] == null) Session["fechaGen"] = "true";
            var pedidos = from p in db.Pedidos
                           select p;
            switch (sortOrder)
            {
                case "fechaGen":
                    if (Session["fechaGen"].ToString().CompareTo("true") == 0)
                    {
                        pedidos = pedidos.OrderByDescending(p => p.fechaGeneracion);
                        Session["fechaGen"] = "false";
                    }
                    else 
                    {
                        pedidos = pedidos.OrderBy(p => p.fechaGeneracion);
                        Session["fechaGen"] = "true";
                                            }
                    break;
            }
            return View("Listado", pedidos.ToList());
        }

        public JsonResult GetInsumos(String id)
        {
            int idTipo = int.Parse(id);
            var insumosBD = new SelectList(db.Insumos.Where(m => m.tipoInsumoId == idTipo), "id", "nombre");

            return Json(insumosBD);
        }

        //GET: Pedidos/DetallesPedido
        public JsonResult GetDetalles(int idPedido)
        {
            var idHospital = db.Pedidos.Find(idPedido).hospitalId;
            var queryStock = db.StockFarmacias.Where(s => s.hospitalId == idHospital);
            var detallesPedido = db.DetallesPedido.Include(d => d.insumo).Include(d => d.pedido).Where(d => d.pedidoId == idPedido)
                                .Join(queryStock, d => d.insumoId, s => s.insumoId, (d, s) => new { d, s }) //TODO: Si el Insumo no existe en el Stock de la farmacia directamente no se muestra
                                .Select(x => new { pedidoId = x.d.pedidoId, insumoId = x.d.insumoId, nombre = x.d.insumo.nombre,
                                    precioUnitario = x.d.insumo.precioUnitario, cantidad = x.d.cantidad, cantidadAutorizada = x.d.cantidadAutorizada
                                    , tipo = x.d.insumo.tiposInsumo.nombre, stock = x.d.insumo.stock, stockFarmacia = x.s.stockFarmacia });

            return Json(detallesPedido, JsonRequestBehavior.AllowGet);
        }

        // POST: Pedidos/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeUserAccessLevel(UserRole = "RespFarmacia")]
        public ActionResult Create([Bind(Include = "id,periodo,fechaGeneracion,esUrgente,estaAutorizado,hospitalId,detallesPedido")] Pedido pedido)
        {
            pedido.fechaEntrega = null;
            pedido.estadoId = db.Estados.Find(1).id;

            foreach (var detalle in pedido.detallesPedido)
            {
                detalle.insumo = null;
            }

            if (ModelState.IsValid)
            {
                db.Pedidos.Add(pedido);
                try { 
                if (db.SaveChanges() > 0)
                {
                       return RedirectToAction("RespFarmacia", new { param = "Success" });
                       
                }
                }
                catch (Exception e)
                {
                    return RedirectToAction("RespFarmacia", new { param = e.Message });
                    
                }
            }

            ViewBag.tipoInsumo = new SelectList(db.TiposInsumo, "id", "nombre");
            ViewBag.hospitalId = new SelectList(db.Hospitales, "id", "nombre", pedido.hospitalId);
            return View(pedido);
        }

        // GET: Pedidos/Edit/5
        [AuthorizeUserAccessLevel(UserRole = "RespFarmacia")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Pedido pedido = db.Pedidos.Find(id);
            if (pedido == null)
            {
                return HttpNotFound();
            }
            ViewBag.hospitalId = new SelectList(db.Hospitales, "id", "nombre", pedido.hospitalId);
            return View(pedido);
        }

        // POST: Pedidos/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeUserAccessLevel(UserRole = "RespAutorizacion")]
        public ActionResult Edit([Bind(Include = "id,periodo,fechaGeneracion,fechaEntrega,esUrgente,estaAutorizado, hospitalId")] Pedido pedido)
        {
            if (ModelState.IsValid)
            {
                db.Entry(pedido).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Listado");
            }
            ViewBag.hospitalId = new SelectList(db.Hospitales, "id", "nombre", pedido.hospitalId);
            return View(pedido);
        }


        // GET: Pedidos/Delete/5
        [AuthorizeUserAccessLevel(UserRole = "RespFarmacia")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Pedido pedido = db.Pedidos.Find(id);
            if (pedido == null)
            {
                return HttpNotFound();
            }
            return View(pedido);
        }

        // POST: Pedidos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizeUserAccessLevel(UserRole = "RespFarmacia")]
        public ActionResult DeleteConfirmed(int id)
        {
            Pedido pedido = db.Pedidos.Find(id);
            db.Pedidos.Remove(pedido);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [AuthorizeUserAccessLevel(UserRole = "RespAutorizacion", UserRole2 = "DirectorArea")]
        public ActionResult Autorizacion(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Pedido pedido = db.Pedidos.Find(id);
            Session["pedido"] = pedido;
            if (pedido == null)
            {
                return HttpNotFound();
            }
            return View(pedido);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeUserAccessLevel(UserRole = "RespAutorizacion", UserRole2 = "DirectorArea")]
        public ActionResult Autorizacion([Bind(Include = "id,periodo,fechaGeneracion, fechaEntrega, esUrgente,estaAutorizado,hospitalId,estadoID,detallesPedido")] Pedido pedido)
        {
            if (ModelState.IsValid)
            {
                //A cada detalle se le modifican los atributos
                foreach (var detalle in pedido.detallesPedido)
                {
                    updateStock(detalle);
                    detalle.insumo = null;
                    db.Entry(detalle).State = EntityState.Modified;
                    db.SaveChanges();
                }
                //Se modifica el estado del pedido en general
                pedido.estaAutorizado = true;
                pedido.estadoId = 2;
                db.Entry(pedido).State = EntityState.Modified;
                try
                {
                    if (db.SaveChanges() > 0)
                    {
                        return RedirectToAction("Listado", new { param = "Success" });
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    return RedirectToAction("Listado", new { param = e.Message });
                }
                //return RedirectToAction("Listado");
            }
            ViewBag.tipoInsumo = new SelectList(db.TiposInsumo, "id", "nombre");
            ViewBag.hospitalId = new SelectList(db.Hospitales, "id", "nombre", pedido.hospitalId);
            return RedirectToAction("Listado", new { param = "Hubo un problema inesperado" });
        }

        private void updateStock(DetallePedido detalle)
        {
            Insumo ins = db.Insumos.Find(detalle.insumoId);
            int updatedStock = ins.stock - detalle.cantidadAutorizada;
            if (updatedStock <= 0)
            {
                ins.stock = 0;
            }
            else
            {
                ins.stock = updatedStock;
            }

            db.Entry(ins).State = EntityState.Modified;
        }

        [AuthorizeUserAccessLevel(UserRole = "RespReporte")]
        public ActionResult ReporteConsolidado()
        {
            var pedidos = db.Pedidos.Include(p => p.hospital);
            return View(pedidos.ToList());
        }

       //[AuthorizeUserAccessLevel(UserRole = "RespFarmacia")]
       // public ActionResult RespFarmacia()
       // {
       //     var pedidos = db.Pedidos.Include(p => p.hospital);
       //     return View(pedidos.ToList());
       // }
        [AuthorizeUserAccessLevel(UserRole = "RespFarmacia")]
        public ActionResult RespFarmacia(string param)
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
                var hospitalActual = Int32.Parse(Session["hospitalId"].ToString());
                var pedidos = db.Pedidos.Where(r=> r.hospitalId == hospitalActual).Include(p => p.hospital);
                return View(pedidos.OrderByDescending(o => o.id).ToList());

            }
            else
            {
                var hospitalActual = Int32.Parse(Session["hospitalId"].ToString());
                var pedidos = db.Pedidos.Where(r => r.hospitalId == hospitalActual).Include(p => p.hospital);
                return View(pedidos.OrderByDescending(o => o.id).ToList());
            }
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
