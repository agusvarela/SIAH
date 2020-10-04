using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;


namespace SIAH.Models.Reclamos
{
    public class TipoReclamo
    {
        [Key]
        public int id { get; set; }

        public String tipo { get; set; }

        public String descripcion { get; set; }

        public ICollection<Reclamo> Reclamos { get; set; }
    }
}