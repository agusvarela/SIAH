using SIAH.Models.Insumos;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SIAH.Models.Historico
{
    public class HistoricoFarmacia
    {
        [Key]
        [Column(Order = 1)]
        public int hospitalId { get; set; }
        public Hospital hospital { get; set; }

        [Key]
        [Column(Order = 2)]
        public int insumoId { get; set; }
        public Insumo insumo { get; set; }

        [Display(Name = "Fecha de movimiento")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime fechaMovimiento { get; set; }

        public string descripcion { get; set; }

        public int cantidad { get; set; }

        public int saldo { get; set; }

        public bool isNegative { get; set; }
    }
}