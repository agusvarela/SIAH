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
using System.Data.Entity.Core.Objects;

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

        // GET: DetallesPedido
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
