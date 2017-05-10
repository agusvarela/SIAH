using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SIAH.Models.Pedidos
{
    public class DetallePedido
    {
        [Key]
        [Column(Order=1)]
        public int pedidoId { get; set; }

        public Pedido pedido { get; set; }

        [Key]
        [Column(Order =2)]
        public int insumoId { get; set; }

        public Insumos.Insumo insumo { get; set; }

        public int cantidad { get; set; }

    }
}