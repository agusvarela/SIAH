namespace SIAH.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class create_detalles_pedido_table : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DetallePedido",
                c => new
                    {
                        pedidoId = c.Int(nullable: false),
                        insumoId = c.Int(nullable: false),
                        cantidad = c.Int(nullable: false),
                        cantidadAutorizada = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.pedidoId, t.insumoId })
                .ForeignKey("dbo.Insumo", t => t.insumoId, cascadeDelete: true)
                .ForeignKey("dbo.Pedido", t => t.pedidoId, cascadeDelete: true)
                .Index(t => t.pedidoId)
                .Index(t => t.insumoId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DetallePedido", "pedidoId", "dbo.Pedido");
            DropForeignKey("dbo.DetallePedido", "insumoId", "dbo.Insumo");
            DropIndex("dbo.DetallePedido", new[] { "insumoId" });
            DropIndex("dbo.DetallePedido", new[] { "pedidoId" });
            DropTable("dbo.DetallePedido");
        }
    }
}
