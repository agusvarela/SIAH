using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SIAH.Models.Pedidos
{
    public class Pedido
    {
        [Key]
        public int id { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime periodo { get; set; }
        public DateTime fechaGeneracion { get; set; }

        [Required(AllowEmptyStrings = true)]
        public DateTime fechaEntrega { get; set; }

        public Boolean esUrgente { get; set; }
        public Boolean estaAutorizado { get; set; }

        public int hospitalId { get; set; }
        public Hospital hospital { get; set; }

        public ICollection<DetallePedido> detallesPedido { get; set; }


    }
}