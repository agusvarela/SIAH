﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SIAH.Models
{
    public class UserAccount
    {  
        [Key]
        public int id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string nombre { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio")]
        public string apellido { get; set; }

        [EmailAddress(ErrorMessage = "El e-mail es obligatorio y debe ser válido")]
        public string email { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        public string password { get; set; }

       [Required(ErrorMessage = "Debe confirmar su contraseña")]
       [DataType(DataType.Password)]
        public string confirmPassword { get; set; }
    }
}