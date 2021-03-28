using SIAH.Models;
using SIAH.Models.Insumos;
using SIAH.Models.Pedidos;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using SIAH.Models.Remitos;
using SIAH.Models.Compras;

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

        public DbSet<Rol> Roles { get; set; }

        public DbSet<Estado> Estados { get; set; }

        public DbSet<InsumoOcasa> InsumoOcasa { get; set; }

        public DbSet<Remito> Remitos { get; set; }
        
        public DbSet<DetalleRemito> DetallesRemito { get; set; }

        public DbSet<Compra> Compras { get; set; }

        public DbSet<DetalleCompra> DetallesCompra { get; set; }

        public DbSet<EstadoRemito> EstadoRemitoes { get; set; }

        public DbSet<Models.Reclamos.Reclamo> Reclamoes { get; set; }

        public DbSet<Models.Reclamos.TipoReclamo> TipoReclamoes { get; set; }

        public DbSet<Models.Reclamos.EstadoReclamo> EstadoReclamoes { get; set; }

        public DbSet<Models.StockFarmacia> StockFarmacias { get; set; }

        public DbSet<Models.Registro.Registro> Registros { get; set; }

        public DbSet<Models.Registro.DetalleRegistro> DetallesRegistro { get; set; }

        public DbSet<Models.AjusteSIAH.AjusteSIAH> AjusteSIAHs { get; set; }

        public DbSet<Models.AjusteSIAH.DetalleAjusteSIAH> DetalleAjusteSIAHs { get; set; }

        public DbSet<Models.Reclamos.DetalleReclamo> DetallesReclamo { get; set; }

    }

}