using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SIAH.Models.Compras
{
    public class Compra
    {

        [Display(Name = "Id Compra")]
        [Key]
        public int id { get; set; }

        [Display(Name = "Fecha de Entrega Efectiva")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime fechaEntregaEfectiva { get; set; }

        public ICollection<DetalleCompra> detallesCompra { get; set; }
        public Compra()
        {
            this.detallesCompra = new HashSet<DetalleCompra>();
        }
    }
}