using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SIAH.Models.Pedidos
{
    public class Pedido
    {
        [Display(Name = "Id Pedido")]
        [Key]
        public int id { get; set; }
        [Display(Name = "Período")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime periodo { get; set; }
        [Display(Name = "Fecha de Generación")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime fechaGeneracion { get; set; }

        [Display(Name = "Fecha de entrega")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? fechaEntrega { get; set; }

        [Display(Name = "¿Es Urgente?")]
        public Boolean esUrgente { get; set; }
        [Display(Name = "¿Esta Autorizado?")]
        public Boolean estaAutorizado { get; set; }

        public int hospitalId { get; set; }
        public Hospital hospital { get; set; }

        public ICollection<DetallePedido> detallesPedido { get; set; }

        //public Pedido()
        //{
        //    this.detallesPedido = new HashSet<DetallePedido>();
        //}


    }
}