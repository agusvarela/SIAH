using SIAH.Models;
using SIAH.Models.Insumos;
using SIAH.Models.Pedidos;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;
using System.Web.Configuration;

namespace SIAH.Context
{
    public class SIAHContext : DbContext
    {
        public SIAHContext() 
            : base(ConfigurationManager.ConnectionStrings["SIAHConnection"].ConnectionString)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }


        public DbSet<Localidad> Localidades { get; set; }

        public DbSet<TipoInsumo> TiposInsumo { get; set; }

        public DbSet<Insumo> Insumos { get; set; }

        public DbSet<Hospital> Hospitales { get; set; }

        public DbSet<Pedido> Pedidos { get; set; }

        public DbSet<DetallePedido> DetallesPedido { get; set; }

        public DbSet<UserAccount> UserAccounts { get; set; }

    }

}