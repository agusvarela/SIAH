using SIAH.Models.Insumos;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIAH.Models.Registro
{
    public class DetalleRegistro
    {
        [Key]
        [Column(Order = 1)]
        public int registroId { get; set; }
        public Registro registro { get; set; }

        [Key]
        [Column(Order = 2)]
        public int insumoId { get; set; }
        public Insumo insumo { get; set; }
        public int cantidad { get; set; }
    }
}