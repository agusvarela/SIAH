﻿using SIAH.Context;
using SIAH.Models;
using SIAH.Models.Insumos;
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
        public ActionResult ReporteConsolidado(String fechaInicio, String fechaFin, String listaTipoInsumo)
        {


            if (fechaFin != null && fechaInicio != null)
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

                IEnumerable<String[]> datos;
                if (listaTipoInsumo != null && listaTipoInsumo != "")
                {
                    String[] listaString;
                    listaString = listaTipoInsumo.Split(',');
                    int[] listadoTipoInsumo = Array.ConvertAll(listaString, int.Parse);
                    datos = this.GenerarReporte(fInicio, fFin, listadoTipoInsumo);
                    String names = "";
                    bool flag = false;
                    foreach (var id in listadoTipoInsumo)
                    {
                        String nameInsumo = db.TiposInsumo.Find(id).nombre;
                        if (flag == false)
                        {
                            names += nameInsumo;
                        }
                        else
                        {
                            names += ", " + nameInsumo;
                        }
                        flag = true;
                    }
                    ViewBag.nombresInsumo = names;

                }
                else
                {
                    datos = this.GenerarReporteLegacy(fInicio, fFin);
                }

                ViewBag.fechaInicio = fechaInicio;
                ViewBag.fechaFin = fechaFin;
                return View(datos);
            }
            else
            {
                return RedirectToAction("Index");
            }

        }
        // GET: DetallesPedido/GenerarReporte
        [AllowAnonymousAttribute]
        public IEnumerable<String[]> GenerarReporte(DateTime fechaInicio, DateTime fechaFin, int[] listaTipoInsumo)
        {
            //Consulta a la BD para traer cada detalle con su hospital, su insumo y su cantidad
            var result = db.DetallesRemito.Join(db.Insumos, d => d.insumoId, s => s.id, (d, s) => new { d, s }).
                Join(db.Pedidos, x => x.d.remitoId, p => p.id, (x, p) => new { x, p }).
                Join(db.Hospitales, t => t.p.hospitalId, h => h.id, (t, h) => new { t, h }).
                Join(db.Remitos, m => m.t.p.id, a => a.id, (m, r) => new { m, r }).
                Where(i => listaTipoInsumo.Contains(i.m.t.x.s.tipoInsumoId)).
                //Filtro por fecha
                Where(k => DbFunctions.TruncateTime(k.m.t.p.fechaGeneracion) >= DbFunctions.TruncateTime(fechaInicio) && DbFunctions.TruncateTime(k.m.t.p.fechaGeneracion) <= DbFunctions.TruncateTime(fechaFin)).
               // Para mostrar el total
               //GroupBy(x => x.t.x.s.nombre, x => x.t.x.d.cantidad, (key, g) => new { Insumo = key, Total = g.Sum() }).
               GroupBy(x => new { hospital = x.m.h.nombre, insumo = x.m.t.x.s.nombre }, x => x.m.t.x.d.cantidadEntregada, (key, g) => new { Hospital = key.hospital, Insumo = key.insumo, Cantidad = g.Sum() }).
               // Select(x => new { Hospital = x.Hospital, Insumo = x.Insumo, Cantidad = String.Format("{0:n0}", x.Cantidad) }).
               ToList();

            //Declaro la cantidad de filas y de columnas
            var rows = db.Insumos.ToList().Where(i => listaTipoInsumo.Contains(i.tipoInsumoId)).Count() + 1;
            var columns = db.Hospitales.Join(db.Pedidos, x => x.id, p => p.hospitalId, (x, p) => new { x, p }).
                Join(db.Remitos, m => m.p.id, a => a.id, (m, r) => new { m, r }).
                Where(k => DbFunctions.TruncateTime(k.m.p.fechaGeneracion) >= DbFunctions.TruncateTime(fechaInicio) &&
                DbFunctions.TruncateTime(k.m.p.fechaGeneracion) <= DbFunctions.TruncateTime(fechaFin)).
                GroupBy(x => new { hospital = x.m.x.nombre }, (key, g) => new { Hospital = key.hospital }).
                Count() + 2;

            //Armo el Vector
            String[][] report = new String[rows][]; //columns en segundo argumento
            for (int i = 0; i < rows; i++) { report[i] = new String[columns]; }
            report[0][0] = "Id";
            report[0][1] = "Insumo";

            /* var hospitales = db.Hospitales.Select(x => new { x.nombre }).ToList();
             var p = 1;
             foreach( var h in hospitales) { report[0, p] = h.nombre; p++; }*/

            var insumos = db.Insumos.Select(x => new { x.id, x.nombre, x.tipoInsumoId }).Where(x => listaTipoInsumo.Contains(x.tipoInsumoId)).ToList();
            var q = 1;
            foreach (var s in insumos)
            {
                report[q][0] = s.id.ToString();
                report[q][1] = s.nombre;
                q++;
            }

            var j = 1;
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
                            if (report[i][1] == r.Insumo)
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

                            if (report[i][1] == r.Insumo)
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
            ViewBag.tipoInsumo = new SelectList(db.TiposInsumo, "id", "nombre");
            List<String[]> list = report.ToList();
            return list;
        }

        // GET: DetallesPedido/GenerarReporte
        [AllowAnonymousAttribute]
        public IEnumerable<String[]> GenerarReporteLegacy(DateTime fechaInicio, DateTime fechaFin)
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
               // Select(x => new { Hospital = x.Hospital, Insumo = x.Insumo, Cantidad = String.Format("{0:n0}", x.Cantidad) }).
               ToList();

            //Declaro la cantidad de filas y de columnas
            var rows = db.Insumos.ToList().Count() + 1;
            var columns = db.Hospitales.Join(db.Pedidos, x => x.id, p => p.hospitalId, (x, p) => new { x, p }).
                Where(k => DbFunctions.TruncateTime(k.p.fechaGeneracion) >= DbFunctions.TruncateTime(fechaInicio) &&
                DbFunctions.TruncateTime(k.p.fechaGeneracion) <= DbFunctions.TruncateTime(fechaFin)).
                GroupBy(x => new { hospital = x.x.nombre }, (key, g) => new { Hospital = key.hospital }).
                Count() + 2;

            //Armo el Vector
            String[][] report = new String[rows][]; //columns en segundo argumento
            for (int i = 0; i < rows; i++) { report[i] = new String[columns]; }
            report[0][0] = "Id";
            report[0][1] = "Insumo";
            /* var hospitales = db.Hospitales.Select(x => new { x.nombre }).ToList();
             var p = 1;
             foreach( var h in hospitales) { report[0, p] = h.nombre; p++; }*/

            var insumos = db.Insumos.Select(x => new { x.nombre, x.id }).ToList();
            var q = 1;
            foreach (var s in insumos) { 
                report[q][0] = s.id.ToString();
                report[q][1] = s.nombre;
                q++; }

            var j = 1;
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
                            if (report[i][1] == r.Insumo)
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

                            if (report[i][1] == r.Insumo)
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
            ViewBag.tipoInsumo = new SelectList(db.TiposInsumo, "id", "nombre");
            List<String[]> list = report.ToList();
            return list;
        }

        //GET Reportes/ReporteBI
        public ActionResult ReporteBI()
        {
            return View();
        }

        //GET Reportes/ReporteStockFarmacias
        public ActionResult ReporteStockFarmacias()
        {
            return View();
        }

        //GET Reportes/ReporteStockFarmaciaData
        public JsonResult ReporteStockFarmaciaData()
        {
            List<List<int>> values = new List<List<int>>();
            List<Hospital> hospitales;
            List<Insumo> insumos;
            hospitales = db.Hospitales.Select(x => x).OrderBy(x => x.id).ToList();
            var hospitalList = hospitales.Select(x => x.nombre).ToList();
            insumos = db.Insumos.Select(x => x).OrderBy(x => x.id).ToList();
            var insumolist = insumos.Select(x => x.nombre).ToList();

            var stockFarmacia = db.StockFarmacias
                .Select(x => x).ToList();

            foreach (var insumo in insumos)
            {
                var row = new List<int>(new int[40]);
                var stockRow = stockFarmacia.Where(x => x.insumoId == insumo.id)
                    .Select(x => new { hosp = x.hospitalId, stock = x.stockFarmacia })
                    .ToList();
                foreach (var stock in stockRow)
                {
                    row[stock.hosp - 2] = stock.stock; // Hack porque los ID arrancan en 2
                }
                values.Add(row);
            }

            var response = new ResponseStockFarmacia(values, hospitalList, insumolist);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

    }
}

internal class ResponseStockFarmacia
{
    public List<List<int>> values = new List<List<int>>();
    public List<string> hospitales;
    public List<string> insumos;
    public ResponseStockFarmacia(List<List<int>> vls, List<string> hosp, List<string> ins)
    {
        values = vls;
        hospitales = hosp;
        insumos = ins;
    }
}