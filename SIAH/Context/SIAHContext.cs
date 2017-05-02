using SIAH.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace SIAH.Context
{
    public class SIAHContext : DbContext
    {
        public SIAHContext() : base(ConfigurationManager.ConnectionStrings["SIAHConnection"].ConnectionString) { }

        public DbSet<Localidad> Localidades { get; set; }
    }

}