using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIAH.Models.Reclamos
{
    public class DetalleReclamo
    {
        [Key, ForeignKey("Reclamo")]
        [Column(Order = 0)]
        public int pedidoId { get; set; }
        public Reclamo reclamo { get; set; }

        [Key]
        [Column(Order = 2)]
        public int insumoId { get; set; }

        public Insumos.Insumo insumo { get; set; }

        public string observacion { get; set; }

        public string respuesta { get; set; }
        public bool accepted { get; set; }

    }
}