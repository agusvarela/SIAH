using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SIAH.Models.Remitos
{
    public class Remito
    {
        [Display(Name = "Id Remito")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int id { get; set; }
        
        [Display(Name = "Fecha de Entrega Efectiva")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime fechaEntregaEfectiva { get; set; }
        
        public int pedidoId { get; set; }
        public Pedidos.Pedido pedido { get; set; }

        public int estadoId { get; set; }
        public EstadoRemito estado { get; set; }

        public ICollection<DetalleRemito> detallesRemito { get; set; }

        public Remito()
        {
            this.detallesRemito = new HashSet<DetalleRemito>();
        }

        public Remito(int id, DateTime fechaEntregaEfectiva, int pedidoId)
        {
            this.id = id;
            this.fechaEntregaEfectiva = fechaEntregaEfectiva;
            this.pedidoId = pedidoId;
            this.detallesRemito = new HashSet<DetalleRemito>();
        }
    }
}