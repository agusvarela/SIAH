using Newtonsoft.Json;
using SIAH.Context;
using SIAH.Models;
using SIAH.Models.Insumos;
using SIAH.Models.Pedidos;
using SIAH.Models.Remitos;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
namespace SIAH.Controllers
{
    public class PedidosController : Controller
    {
        private SIAHContext db = new SIAHContext();
        private static readonly HttpClient client = new HttpClient();

        //POST: EntregaPedido
        public async Task<HttpResponseMessage> EntregaPedido(int? id)
        {
            HttpResponseMessage response;
            if (id == null)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
            try
            {
                var pedido = db.Pedidos.Where(p => p.id == id).Include(t => t.estado).Include(d => d.detallesPedido).First();
                if (pedido.estadoId == 3)
                {
                    pedido.estadoId = 6;
                    foreach (DetallePedido item in pedido.detallesPedido)
                    {
                        StockFarmacia insumo = db.StockFarmacias.Where(p => p.hospitalId == pedido.hospitalId &&
                                                        p.insumoId == item.insumoId).First();
                        if (insumo != null)
                        {
                            insumo.stockFarmacia = insumo.stockFarmacia + item.cantidadAutorizada;
                            db.Entry(insumo).State = EntityState.Modified;
                        }
                        //TODO: Si el insumo no existe se deberia insertar en la BD
                    }
                    db.Entry(pedido).State = EntityState.Modified;
                    db.SaveChanges();
                    await SendEmailCambioEstadoAsync(pedido);
                }
                else
                {
                    response = new HttpResponseMessage(HttpStatusCode.NotAcceptable);
                    /*response.Content = new StringContent("El pedido ingresado debe estar en estado " +
                        "En Proceso de Envío y el estado del pedido ingresado es " + pedido.estado.nombreEstado);*/
                    return response;
                }
            }
            catch (Exception e)
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
        [AuthorizeUserAccessLevel(UserRole = "RespFarmacia", UserRole2 = "RespAutorizacion", UserRole3 = "DirectorArea")]
        public async Task<ActionResult> Cancelar(int? pedidoId)
        {
            if (pedidoId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            try
            {

                var pedido = db.Pedidos.Where(p => p.id == pedidoId).First();
                pedido.estadoId = 4;
                pedido.responsableAsignadoId = int.Parse(Session["userid"].ToString());
                db.Entry(pedido).State = EntityState.Modified;
                db.SaveChanges();
                await SendEmailCambioEstadoAsync(pedido);
            }
            catch (Exception e)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            return RedirectToAction("Listado", "Pedidos");
        }

        // POST: Pedidos/Recibido
        public async Task<ActionResult> Recibido(int? pedidoId)
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
                await SendEmailCambioEstadoAsync(pedido);
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
            ViewBag.estado = db.Pedidos.Include(p => p.estado).Where(x => x.id == id).Select(r => new { estado = r.estado.nombreEstado }).First().estado;
            Pedido pedido = db.Pedidos.Find(id);
            ViewBag.hospital = db.Hospitales.Include(hospital => hospital.nombre).Where(hospital => hospital.id == pedido.hospitalId).Select(r => new { hospital = r.nombre }).First().hospital;

            if (pedido.responsableAsignadoId != null)
            {
                UserAccount responsable = db.UserAccounts.Find(pedido.responsableAsignadoId);
                pedido.responsableAsignado = responsable;
            }
            
            if (pedido == null)
            {
                return HttpNotFound();
            }
            return View(pedido);
        }

        //POST: Pedidos/GenerateSendOcasa
        public async Task<ActionResult> GenerateSendOcasa()
        {
            var detalles = db.DetallesPedido.Include(p => p.pedido).Where(x => x.pedido.estadoId == 2);
            //Si no hay Pedidos en estado Autorizado, no se debe cambiar de estado nada ni enviar a OCASA 
            if (!detalles.Any())
            {
                return RedirectToAction("Listado", "Pedidos");
            }
            //Listado de pedidos en estado autorizado agregados a un JSON
            var listPedidos = new List<Pedido>();
            foreach (var pedido in db.Pedidos.Include(p => p.detallesPedido).Where(x => x.estadoId == 2).ToList())
            {
                listPedidos.Add(pedido);
            }

            //Body que se enviará a OCASA con los pedidos en formato JSON 
            var jsonPedidos = JsonConvert.SerializeObject(listPedidos, Formatting.Indented, new JsonSerializerSettings() { MaxDepth = 1, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            var content = new StringContent(jsonPedidos, Encoding.UTF8, "application/json");
            //LLamada a API ficticia que devuelve siempre un 200 (OK) con el tracking number de cada pedido
            try
            {
                var response = await client.PostAsync("http://localhost:3000/envio", content);
                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        var responseArray = JsonConvert.DeserializeAnonymousType(responseContent, new[] { new { idPedido = "sample", tracking = "sample" }, new { idPedido = "sample", tracking = "sample" } });

                        foreach (var responseObject in responseArray)
                        {
                            int idPedido = int.Parse(responseObject.idPedido);
                            Pedido pedido = db.Pedidos.Find(idPedido);
                            pedido.trackingNumber = responseObject.tracking;
                            pedido.estadoId = 3;
                            db.Entry(pedido).State = EntityState.Modified;
                            await SendEmailCambioEstadoAsync(pedido);
                        }
                        db.SaveChanges();
                        return RedirectToAction("Listado", "Pedidos", new { param = "Success" });
                    }
                    catch (Exception e)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, e.Message);
                    }
                }
            }
            catch (Exception e)
            {
                return RedirectToAction("Listado", "Pedidos", new { param = "Hubo un problema inesperado" });
            }

            return RedirectToAction("Listado", "Pedidos", new { param = "Hubo un problema inesperado" });
            //Si la respuesta de la API da cualquier resultado que no sea Success, se vuelve a la pagina con el mensaje de error
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
        public ActionResult Create(int idHospital)
        {
            ViewBag.tipoInsumo = new SelectList(db.TiposInsumo.OrderBy(tipo => tipo.nombre), "id", "nombre");
            //ViewBag.hospitalId = new SelectList(db.Hospitales, "id", "nombre");
            bool hasLastPedido = true;
            var pedidos = db.Pedidos.Where(r => r.hospitalId == idHospital).Include(p => p.detallesPedido).OrderByDescending(o => o.fechaGeneracion).ToList();
            if(pedidos.Count() == 0)
            {
                hasLastPedido = false;
            }
            ViewBag.hasLastPedido = hasLastPedido;
            return View(new Pedido());
        }

        // GET: Pedidos/Listado
        //public ActionResult Listado()
        //{
        //    var pedidos = db.Pedidos.Include(p => p.hospital);
        //    return View(pedidos.ToList());
        //}

        [AuthorizeUserAccessLevel(UserRole = "RespAutorizacion", UserRole2 = "DirectorArea", UserRole3 = "RespFarmacia")]
        public ActionResult Listado(String param)
        {
            if (Session["rol"].ToString() == "RespFarmacia")
            {
                return RedirectToAction("RespFarmacia", "Pedidos");
            }

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
            var pedidos = db.Pedidos.Include(p => p.hospital); // OrderBy(o => o.fechaGeneracion);
            return View(pedidos.OrderByDescending(p => p.id).ToList());
        }

        [AuthorizeUserAccessLevel(UserRole = "RespAutorizacion", UserRole2 = "DirectorArea")]
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
                                .Select(x => new
                                {
                                    pedidoId = x.d.pedidoId,
                                    insumoId = x.d.insumoId,
                                    nombre = x.d.insumo.nombre,
                                    precioUnitario = x.d.insumo.precioUnitario,
                                    cantidad = x.d.cantidad,
                                    cantidadAutorizada = x.d.cantidadAutorizada
                                    ,
                                    tipo = x.d.insumo.tiposInsumo.nombre,
                                    stock = x.d.insumo.stock,
                                    stockFarmacia = x.s.stockFarmacia
                                });

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
                try
                {
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

            return RedirectToAction("RespFarmacia", new { param = "Ocurrio un error inesperado al enviar el pedido" });
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


        //GET: Pedidos/Autorizacion
        [AuthorizeUserAccessLevel(UserRole = "RespAutorizacion", UserRole2 = "DirectorArea")]
        public ActionResult Autorizacion(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Pedido pedido = db.Pedidos.Find(id);
            ViewBag.hospital = db.Hospitales.Include(hospital => hospital.nombre).Where(hospital => hospital.id == pedido.hospitalId).Select(r => new { hospital = r.nombre }).First().hospital;
            Session["pedido"] = pedido;
            if (pedido == null)
            {
                return HttpNotFound();
            }
            return View(pedido);
        }

        //POST: Pedidos/Autorizacion
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeUserAccessLevel(UserRole = "RespAutorizacion", UserRole2 = "DirectorArea")]
        public async Task<ActionResult> Autorizacion([Bind(Include = "id,periodo,fechaGeneracion, fechaEntrega, esUrgente,estaAutorizado,hospitalId,estadoID,detallesPedido")] Pedido pedido)
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
                pedido.responsableAsignadoId = int.Parse(Session["userid"].ToString());
                db.Entry(pedido).State = EntityState.Modified;
                await SendEmailCambioEstadoAsync(pedido);
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
        public ActionResult RespFarmacia(string param, int? stateFilter, string fechaI, string fechaF)
        {
            DateTime fechaInicio;
            DateTime fechaFin;

            if (fechaI == null || fechaF == null)
            {
                DateTime today = DateTime.Today;
                fechaFin = today;
                fechaInicio = today.AddYears(-1);
            }
            else
            {
                fechaInicio = DateTime.Parse(fechaI);
                fechaFin = DateTime.Parse(fechaF);
            }

            ViewBag.fechaFin = fechaFin;
            ViewBag.fechaInicio = fechaInicio;
            ViewBag.fechaFinDefault = fechaFin.ToShortDateString();
            ViewBag.fechaInicioDefault = fechaInicio.ToShortDateString();

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
            var pedidos = db.Pedidos.Where(r => r.hospitalId == hospitalActual)
                                    .Include(p => p.hospital)
                                    .Where(p => p.fechaGeneracion >= fechaInicio && p.fechaGeneracion <= fechaFin);

            if (stateFilter != null && stateFilter >= 1 && stateFilter <= 8)
            {
                if (stateFilter == 5)
                {
                    pedidos = pedidos.Where(p => p.estadoId == stateFilter || p.estadoId == 8);
                }
                else
                {
                    pedidos = pedidos.Where(p => p.estadoId == stateFilter);
                }

                ViewBag.stateFilter = stateFilter;
            }

            return View(pedidos.OrderByDescending(o => o.id).ToList());
        }

        //GET: Pedidos/PedidosDatasetBI
        public JsonResult PedidosDatasetBI()
        {
            var dataset = db.Pedidos.Include(x => x.hospital).Select(x => new { IdPedido = x.id, Hospital = x.hospital.nombre, FechaMes = x.fechaGeneracion, Periodo = x.periodo })
                .ToList().Select(x => new { 
                    IdPedido = x.IdPedido, 
                    Hospital = x.Hospital, 
                    FechaMes = string.Format("{0:MM/dd/yyyy}", x.FechaMes), 
                    TotalPedidoPorMes = GetTotalPedido(x.IdPedido),
                    Periodo = string.Format("{0:MM/dd/yyyy}", x.Periodo) });
            return Json(dataset, JsonRequestBehavior.AllowGet);
        }

        private string GetTotalPedido(int idPedido)
        {
            decimal totalPedido = 0;
            List<DetallePedido> detalles = db.DetallesPedido.Where(x => x.pedidoId == idPedido).Include(x => x.insumo).ToList();
            foreach (var detalle in detalles)
            {
                decimal totalDetalle = detalle.insumo.precioUnitario * detalle.cantidadAutorizada;
                totalPedido += totalDetalle;
            }

            return totalPedido.ToString();
        }

        //GET: Pedidos/UbicacionDatasetBI
        public JsonResult UbicacionDatasetBI()
        {
            var dataset = db.Hospitales.Include(x => x.localidad)
                .Select(x => new { Zona = (x.localidad.nombre == "Cordoba") ? "Capital" : "Interior", Hospital = x.nombre, Latitud = x.latitud, Longitud = x.longitud, Localidad = x.localidad.nombre });
            return Json(dataset, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public JsonResult LastPedido(int idHospital)
        {
            var pedido = db.Pedidos.Where(r => r.hospitalId == idHospital).Include(p => p.detallesPedido).OrderByDescending(o => o.id).ToList().First();

            var detallesLastPedido = db.DetallesPedido.Include(d => d.insumo).Include(d => d.pedido).Where(d => d.pedidoId == pedido.id)
                                .Select(x => new
                                {
                                    pedidoId = x.pedidoId,
                                    insumoId = x.insumoId,
                                    nombre = x.insumo.nombre,
                                    precioUnitario = x.insumo.precioUnitario,
                                    cantidad = x.cantidad,
                                    cantidadAutorizada = x.cantidadAutorizada,
                                    tipo = x.insumo.tiposInsumo.nombre,
                                    stock = x.insumo.stock
                                });

            return Json(detallesLastPedido, JsonRequestBehavior.AllowGet);

        }
        private async Task SendEmailCambioEstadoAsync(Pedido pedido)
        {
            var message = new MailMessage();
            var user = db.UserAccounts.Where(ua => ua.hospitalID == pedido.hospitalId).First();
            message.To.Add(new MailAddress(user.email));
            message.From = new MailAddress("siah.reclamos@gmail.com");
            message.Subject = string.Format("[SIAH] El pedido N°{0} fue actualizado", pedido.id);
            string body = string.Empty;
            using (StreamReader reader = new StreamReader(Server.MapPath("~/Views/Shared/EmailCambioEstado.html")))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{pedidoId}", pedido.id.ToString());
            body = body.Replace("{fechaPedido}", pedido.fechaGeneracion.ToShortDateString());
            Hospital hospital = db.Hospitales.Find(pedido.hospitalId);
            body = body.Replace("{hospitalName}", hospital.nombre);
            var estado = db.Estados.Find(pedido.estadoId);
            body = body.Replace("{estadoName}", estado.nombreEstado);

            message.Body = body;

            message.IsBodyHtml = true;

            using (var smtp = new SmtpClient())
            {
                await smtp.SendMailAsync(message);
            }
        }

        public JsonResult cantidadPedidosPorEstado(string fechaI, string fechaF, int hospital)
        {
            DateTime fechaInicio = DateTime.Parse(fechaI);
            DateTime fechaFin = DateTime.Parse(fechaF);

            var cantidadPedidosPorEstado = db.Pedidos
                    .Include(p => p.estadoId)
                    .Where(p => p.fechaGeneracion >= fechaInicio && p.fechaGeneracion <= fechaFin && p.hospitalId == hospital)
                    .GroupBy(p => p.estadoId)
                    .Select(x => new
                    {
                        estadoId = x.Key,
                        cantidad = x.Count()
                    }).ToList();

            int[] cantidadPedidos = new int[7];
            foreach(var estado in cantidadPedidosPorEstado)
            {
                if (estado.estadoId == 8) { cantidadPedidos[4] += estado.cantidad; }
                else { cantidadPedidos[estado.estadoId - 1] = estado.cantidad; }
            }
            return Json(cantidadPedidos, JsonRequestBehavior.AllowGet);
        }
    }
}
