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
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [RegularExpression(@"[a-zA-ZñÑáéíóúÁÉÍÓÚ\s]+", ErrorMessage = "No se permiten números o símbolos en el nombre del Tipo Insumo")]
        public String nombre { get; set; }

        public ICollection<Insumo> Insumos { get; set; }
    }
}