namespace SIAH.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class tipo_reclamo_detalle : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DetalleReclamo", "tipoReclamoId", c => c.Int(nullable: false));
            CreateIndex("dbo.DetalleReclamo", "tipoReclamoId");
            AddForeignKey("dbo.DetalleReclamo", "tipoReclamoId", "dbo.TipoReclamo", "id", cascadeDelete: false);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DetalleReclamo", "tipoReclamoId", "dbo.TipoReclamo");
            DropIndex("dbo.DetalleReclamo", new[] { "tipoReclamoId" });
            DropColumn("dbo.DetalleReclamo", "tipoReclamoId");
        }
    }
}
