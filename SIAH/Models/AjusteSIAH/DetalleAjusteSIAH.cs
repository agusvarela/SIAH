using SIAH.Models.Insumos;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIAH.Models.AjusteSIAH
{
    public class DetalleAjusteSIAH
    {
        [Key]
        [Column(Order = 1)]
        public int ajusteId { get; set; }
        public AjusteSIAH ajuste { get; set; }

        [Key]
        [Column(Order = 2)]
        public int insumoId { get; set; }
        public Insumo insumo { get; set; }
        public int cantidad { get; set; }
        public bool isNegative { get; set; }
        public string info { get; set; }

    }
}