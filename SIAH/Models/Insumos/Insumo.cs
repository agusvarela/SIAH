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
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public String nombre { get; set; }

        //[RegularExpression(@"^\d+\.\d{0,2}$")]
        //[Range(0, 9999999999999999.99)]
        public Decimal precioUnitario { get; set; }
        public int tipoInsumoId { get; set; }

        public TipoInsumo tiposInsumo { get; set; }

        public ICollection<Pedidos.DetallePedido> DetallesPedido { get; set; }


    }
}