using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SIAH.Models
{
    public class Hospital
    {
        public int id { get; set; }
        [StringLength(255)]
        [Required]
        public String nombre { get; set; }
        
        public int localidadId { get; set; }
        public Localidad localidad { get; set; }

    }
}