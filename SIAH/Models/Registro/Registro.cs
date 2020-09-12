using System;
using System.ComponentModel.DataAnnotations;

namespace SIAH.Models.Registro
{
    public class Registro
    {
        [Display(Name = "Id Registro")]
        [Key]
        public int id { get; set; }

        [Display(Name = "Fecha y hora")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd--hh-mm}", ApplyFormatInEditMode = true)]
        public DateTime fechaGeneracion { get; set; }

        [Display(Name = "Destinatario")]
        public string destinatario { get; set; }
        public int usuarioId { get; set; }
        public UserAccount usuario { get; set; }
        public int hospitalId { get; set; }
        public Hospital hospital { get; set; }
    }
}