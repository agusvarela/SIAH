using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using SIAH.Models.Insumos;

namespace SIAH.Models
{
    public class StockFarmacia
    {
        [Key]
        public int id { get; set; }

        public int hospitalId { get; set; }
        public Hospital hospital { get; set; }

        public int insumoId { get; set; }
        public Insumo insumo { get; set; }

        [Display(Name = "Stock de farmacia")]
        public int stockFarmacia { get; set; }
    }
}