namespace SIAH.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class cantidad_detalle_reclamo : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DetalleReclamo",
                c => new
                    {
                        pedidoId = c.Int(nullable: false),
                        insumoId = c.Int(nullable: false),
                        cantidad = c.Int(nullable: false),
                        observacion = c.String(),
                        respuesta = c.String(),
                        accepted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => new { t.pedidoId, t.insumoId })
                .ForeignKey("dbo.Insumo", t => t.insumoId, cascadeDelete: true)
                .ForeignKey("dbo.Reclamo", t => t.pedidoId, cascadeDelete: true)
                .Index(t => t.pedidoId)
                .Index(t => t.insumoId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DetalleReclamo", "pedidoId", "dbo.Reclamo");
            DropForeignKey("dbo.DetalleReclamo", "insumoId", "dbo.Insumo");
            DropIndex("dbo.DetalleReclamo", new[] { "insumoId" });
            DropIndex("dbo.DetalleReclamo", new[] { "pedidoId" });
            DropTable("dbo.DetalleReclamo");
        }
    }
}
