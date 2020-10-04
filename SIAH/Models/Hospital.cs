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
        [Display(Name = "Hospital")]
        [StringLength(255)]
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [RegularExpression(@"[a-zA-ZñÑáéíóúÁÉÍÓÚ\s]+", ErrorMessage = "No se permiten números o símbolos en el nombre del Hospital")]
        public String nombre { get; set; }
        
        public int localidadId { get; set; }
        public Localidad localidad { get; set; }

        public Decimal presupuesto { get; set; }

        [Display(Name = "Latitud")]
        public String latitud { get; set; }
        
        [Display(Name = "Longitud")]
        public String longitud { get; set; }

        [Display(Name = "Teléfono")]
        public String telefono { get; set; }

        public ICollection<Pedidos.Pedido> Pedidos { get; set; }

        public ICollection<StockFarmacia> StockFarmacias { get; set; }
    }
}