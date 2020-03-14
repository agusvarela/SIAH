using SIAH.Context;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SIAH.Controllers
{
    public class ReportesController : Controller
    {
        private SIAHContext db = new SIAHContext();

        //GET: Reportes
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult /* Esto es lo que retorna a la vista o sea el @Model IEnumerable<String[]>*/ ReporteComparacion()
        {
            var result = db.DetallesPedido.Include(d => d.insumo).Include(d => d.pedido).
           GroupBy(d => d.insumo.nombre)
           .Select(d => new { Insumo = d.Key, Cantidad = d.Sum(c => c.cantidad), CantidadAutorizada = d.Sum(c => c.cantidadAutorizada) }).ToList();
            //Declaro la cantidad de filas y de columnas
            var rows = db.Insumos.ToList().Count() + 1;
            var columns = 3;

            //Armo el Vector
            String[][] report = new String[rows][]; //columns en segundo argumento
            for (int i = 0; i < rows; i++) { report[i] = new String[columns]; }

            var insumos = db.Insumos.Select(x => new { x.nombre }).ToList();
            var q = 0;
            foreach (var s in insumos) { report[q][0] = s.nombre; q++; }

            var j = 1;
            foreach (var r in result)
            {
                report[j][0] = r.Insumo;
                report[j][1] = r.Cantidad.ToString();
                report[j][2] = r.CantidadAutorizada.ToString();
                j++;

            }

            List<String[]> list = report.ToList();
            return View(list);

        }

        // GET: Reportes/ReporteConsolidado
        public ActionResult ReporteConsolidado(String fechaInicio, String fechaFin)
        {
            var start = fechaInicio.Split('/');
            var end = fechaFin.Split('/');

            var d1 = Int32.Parse(start[0]);
            var m1 = Int32.Parse(start[1]);
            var y1 = Int32.Parse(start[2]);

            var d2 = Int32.Parse(end[0]);
            var m2 = Int32.Parse(end[1]);
            var y2 = Int32.Parse(end[2]);

            var fInicio = new DateTime(y1, m1, d1);
            var fFin = new DateTime(y2, m2, d2);
            var datos = this.GenerarReporte(fInicio, fFin);
            ViewBag.fechaInicio = fechaInicio;
            ViewBag.fechaFin = fechaFin;
            return View(datos);
            //return View();
        }

        // GET: DetallesPedido/GenerarReporte
        [AllowAnonymousAttribute]
        public IEnumerable<String[]> GenerarReporte(DateTime fechaInicio, DateTime fechaFin)
        {
            //Consulta a la BD para traer cada detalle con su hospital, su insumo y su cantidad
            var result = db.DetallesPedido.Join(db.Insumos, d => d.insumoId, s => s.id, (d, s) => new { d, s }).
                Join(db.Pedidos, x => x.d.pedidoId, p => p.id, (x, p) => new { x, p }).
                Join(db.Hospitales, t => t.p.hospitalId, h => h.id, (t, h) => new { t, h }).
                //Filtro por fecha
                Where(k => DbFunctions.TruncateTime(k.t.p.fechaGeneracion) >= DbFunctions.TruncateTime(fechaInicio) && DbFunctions.TruncateTime(k.t.p.fechaGeneracion) <= DbFunctions.TruncateTime(fechaFin)).
               // Para mostrar el total
               //GroupBy(x => x.t.x.s.nombre, x => x.t.x.d.cantidad, (key, g) => new { Insumo = key, Total = g.Sum() }).
               GroupBy(x => new { hospital = x.h.nombre, insumo = x.t.x.s.nombre }, x => x.t.x.d.cantidad, (key, g) => new { Hospital = key.hospital, Insumo = key.insumo, Cantidad = g.Sum() }).
               ToList();

            //Declaro la cantidad de filas y de columnas
            var rows = db.Insumos.ToList().Count() + 1;
            var columns = db.Hospitales.Join(db.Pedidos, x => x.id, p => p.hospitalId, (x, p) => new { x, p }).
                Where(k => DbFunctions.TruncateTime(k.p.fechaGeneracion) >= DbFunctions.TruncateTime(fechaInicio) &&
                DbFunctions.TruncateTime(k.p.fechaGeneracion) <= DbFunctions.TruncateTime(fechaFin)).
                GroupBy(x => new { hospital = x.x.nombre }, (key, g) => new { Hospital = key.hospital }).
                Count() + 1;

            //Armo el Vector
            String[][] report = new String[rows][]; //columns en segundo argumento
            for (int i = 0; i < rows; i++) { report[i] = new String[columns]; }
            report[0][0] = "Insumo";
            /* var hospitales = db.Hospitales.Select(x => new { x.nombre }).ToList();
             var p = 1;
             foreach( var h in hospitales) { report[0, p] = h.nombre; p++; }*/

            var insumos = db.Insumos.Select(x => new { x.nombre }).ToList();
            var q = 1;
            foreach (var s in insumos) { report[q][0] = s.nombre; q++; }

            var j = 0;
            foreach (var r in result)
            {
                if (j < columns)
                {
                    if (report[0][j] != r.Hospital)
                    {
                        j++;
                        report[0][j] = r.Hospital;
                        for (var i = 1; i < rows; i++)
                        {
                            if (report[i][0] == r.Insumo)
                            {
                                report[i][j] = r.Cantidad.ToString();
                            }
                            else
                            {
                                if (report[i][j] == null) { report[i][j] = "0"; }
                            }

                        }
                    }
                    else
                    {
                        for (var i = 1; i < rows; i++)
                        {

                            if (report[i][0] == r.Insumo)
                            {
                                report[i][j] = r.Cantidad.ToString();
                            }
                            else
                            {
                                if (report[i][j] == null) { report[i][j] = "0"; }
                            }
                        }
                    }
                }

            }

            List<String[]> list = report.ToList();
            return list;
        }

        //GET Reportes/ReporteBI
        public ActionResult ReporteBI()
        {
            return View();
        }

    }
}