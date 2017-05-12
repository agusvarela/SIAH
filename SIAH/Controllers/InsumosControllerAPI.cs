using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using SIAH.Context;
using SIAH.Models.Insumos;

namespace SIAH.Controllers
{
    public class InsumosControllerAPI : ApiController
    {
        private SIAHContext db = new SIAHContext();

        // GET: api/InsumosControllerAPI
          public IEnumerable<Insumo> GetInsumos(string searchText)
        {
            return db.Insumos.Where(m => m.nombre.Contains(searchText)).ToList();
        }

        // GET: api/InsumosControllerAPI/5
        [ResponseType(typeof(Insumo))]
        public IHttpActionResult GetInsumo(int id)
        {
            Insumo insumo = db.Insumos.Find(id);
            if (insumo == null)
            {
                return NotFound();
            }

            return Ok(insumo);
        }

        // PUT: api/InsumosControllerAPI/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutInsumo(int id, Insumo insumo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != insumo.id)
            {
                return BadRequest();
            }

            db.Entry(insumo).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InsumoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/InsumosControllerAPI
        [ResponseType(typeof(Insumo))]
        public IHttpActionResult PostInsumo(Insumo insumo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Insumos.Add(insumo);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = insumo.id }, insumo);
        }

        // DELETE: api/InsumosControllerAPI/5
        [ResponseType(typeof(Insumo))]
        public IHttpActionResult DeleteInsumo(int id)
        {
            Insumo insumo = db.Insumos.Find(id);
            if (insumo == null)
            {
                return NotFound();
            }

            db.Insumos.Remove(insumo);
            db.SaveChanges();

            return Ok(insumo);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool InsumoExists(int id)
        {
            return db.Insumos.Count(e => e.id == id) > 0;
        }
    }
}