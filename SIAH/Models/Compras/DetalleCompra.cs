using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SIAH.Models.Compras
{
    public class DetalleCompra
    {
        [Key]
        [Column(Order = 1)]
        public int compraId { get; set; }

        public Compra compra { get; set; }

        [Key]
        [Column(Order = 2)]
        public int insumoId { get; set; }

        public Insumos.Insumo insumo { get; set; }

        public int cantidadComprada { get; set; }
    }
}