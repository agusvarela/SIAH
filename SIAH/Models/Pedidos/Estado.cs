﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SIAH.Models.Pedidos
{
    public class Estado
    {
        [Display(Name = "Id Estado")]
        [Key]
        public int id { get; set; }

        [Display(Name = "Estado")]
        public String nombreEstado { get; set; }

        [Display(Name = "Estado Final")]
        public Boolean isFinal { get; set; }
    }
}