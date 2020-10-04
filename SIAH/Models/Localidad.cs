using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SIAH.Models
{
    public class Localidad
    {
        public int id { get; set; }

        [StringLength(150)]
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [RegularExpression(@"[a-zA-ZñÑáéíóúÁÉÍÓÚ\s]+", ErrorMessage = "No se permiten números o símbolos en el nombre de la Localidad")]
        public String nombre { get; set; }

        public ICollection<Hospital> Hospitales { get; set; }
    }
}