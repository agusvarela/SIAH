namespace SIAH.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class create_table_remito_detalle_remito : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DetalleRemito",
                c => new
                    {
                        remitoId = c.Int(nullable: false),
                        insumoId = c.Int(nullable: false),
                        cantidadEntregada = c.Int(nullable: false),
                        observacion = c.String(),
                    })
                .PrimaryKey(t => new { t.remitoId, t.insumoId })
                .ForeignKey("dbo.Insumo", t => t.insumoId, cascadeDelete: true)
                .ForeignKey("dbo.Remito", t => t.remitoId, cascadeDelete: true)
                .Index(t => t.remitoId)
                .Index(t => t.insumoId);
            
            CreateTable(
                "dbo.Remito",
                c => new
                    {
                        id = c.Int(nullable: false, identity: false),
                        fechaEntregaEfectiva = c.DateTime(nullable: false),
                        pedidoId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Pedido", t => t.pedidoId, cascadeDelete: true)
                .Index(t => t.pedidoId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Remito", "pedidoId", "dbo.Pedido");
            DropForeignKey("dbo.DetalleRemito", "remitoId", "dbo.Remito");
            DropForeignKey("dbo.DetalleRemito", "insumoId", "dbo.Insumo");
            DropIndex("dbo.Remito", new[] { "pedidoId" });
            DropIndex("dbo.DetalleRemito", new[] { "insumoId" });
            DropIndex("dbo.DetalleRemito", new[] { "remitoId" });
            DropTable("dbo.Remito");
            DropTable("dbo.DetalleRemito");
        }
    }
}
