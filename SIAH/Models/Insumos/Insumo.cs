using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SIAH.Models.Insumos
{
    public class Insumo
    {
        public int id { get; set; }

        [StringLength(255)]
        [Required]
        public String nombre { get; set; }

        [Required]
        public Double precioUnitario { get; set; }
        public int tipoInsumoId { get; set; }

        public TipoInsumo tiposInsumo { get; set; }

        public ICollection<Pedidos.DetallePedido> DetallesPedido { get; set; }
    }
}