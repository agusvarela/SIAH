using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SIAH.Models.AjusteSIAH
{
    public class AjusteSIAH
    {
        [Display(Name = "Id Ajuste")]
        [Key]
        public int id { get; set; }

        [Display(Name = "Fecha y hora")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy hh:mm}", ApplyFormatInEditMode = true)]
        public DateTime fechaGeneracion { get; set; }

        public string info { get; set; }

        public int usuarioId { get; set; }
        public UserAccount usuario { get; set; }
    }
}