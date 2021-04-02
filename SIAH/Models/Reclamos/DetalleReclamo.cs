using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIAH.Models.Reclamos
{
    public class DetalleReclamo
    {
        [Key]
        [Column(Order = 1)]
        public int pedidoId { get; set; }
        public Reclamo reclamo { get; set; }

        [Key]
        [Column(Order = 2)]
        public int insumoId { get; set; }
        public int cantidad { get; set; }

        public Insumos.Insumo insumo { get; set; }
        public int tipoReclamoId { get; set; }
        public TipoReclamo tipoReclamo { get; set; }

        public string observacion { get; set; }

        public string respuesta { get; set; }
        public bool accepted { get; set; }

    }
}