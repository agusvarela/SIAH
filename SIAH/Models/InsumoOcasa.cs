using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SIAH.Models
{
    public class InsumoOcasa
    {
        [Key]
        public int id { get; set; }

        public String nombre { get; set; }

        public int stockFisico { get; set; }
    }
}