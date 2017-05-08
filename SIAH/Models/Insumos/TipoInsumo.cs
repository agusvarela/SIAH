using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SIAH.Models.Insumos
{
    public class TipoInsumo
    {
        public int id { get; set; }

        [StringLength(255)]
        [Required]
        public String nombre { get; set; }

        public ICollection<Insumo> Insumos { get; set; }
    }
}