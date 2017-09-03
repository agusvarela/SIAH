using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SIAH.Models
{
    public class Rol
    {
        [Key]
        public int id { get; set; }

        [StringLength(150)]
        [Required]
        public String nombre { get; set; }

        public ICollection<UserAccount> Usuarios { get; set; }
    }
}