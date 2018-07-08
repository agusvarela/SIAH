using SIAH.Models.Reclamos;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SIAH.Models.Reclamos
{
    public class Reclamo
    {
        [Display(Name = "Id Reclamo")]
        [Key]
        public int id { get; set; }

        [Display(Name = "Observación")]
        public String observacionFamacia { get; set; }

        [Display(Name = "Respuesta")]
        public String respuesta { get; set; }

        [Display(Name = "Fecha de Inicio Reclamo")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime fechaInicioReclamo { get; set; }

        [Display(Name = "Fecha de Fin Reclamo")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? fechaFinReclamo { get; set; }

        public int tipoReclamoId { get; set; }
        public TipoReclamo tipoReclamo { get; set; }

        public int pedidoId { get; set; }
        public Pedidos.Pedido pedido { get; set; }

        public int hospitalId { get; set; }
        public Hospital hospital { get; set; }

        public int responsableAsignadoId { get; set; }
        public UserAccount responsableAsignado { get; set; }

        public int estadoReclamoId { get; set; }
        public EstadoReclamo estadoReclamo { get; set; }
    }
}