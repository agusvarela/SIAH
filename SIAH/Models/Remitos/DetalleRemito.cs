using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SIAH.Models.Remitos
{
    public class DetalleRemito
    {
        [Key]
        [Column(Order = 1)]
        public int remitoId { get; set; }

        public Remito remito { get; set; }

        [Key]
        [Column(Order = 2)]
        public int insumoId { get; set; }

        public Insumos.Insumo insumo { get; set; }

        public int cantidadEntregada { get; set; }

        public String observacion { get; set; }

        public DetalleRemito()
        {

        }

        public DetalleRemito(int remitoId, int insumoId, int cantidadEntregada)
        {
            this.remitoId = remitoId;
            this.insumoId = insumoId;
            this.cantidadEntregada = cantidadEntregada;
        }
    }
}