namespace SIAH.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class create_pedidos_detalles_pedido : DbMigration
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
                    })
                .PrimaryKey(t => new { t.pedidoId, t.insumoId })
                .ForeignKey("dbo.Insumo", t => t.insumoId, cascadeDelete: true)
                .ForeignKey("dbo.Pedido", t => t.pedidoId, cascadeDelete: true)
                .Index(t => t.pedidoId)
                .Index(t => t.insumoId);
            
            CreateTable(
                "dbo.Pedido",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        periodo = c.DateTime(nullable: false),
                        fechaGeneracion = c.DateTime(nullable: false),
                        fechaEntrega = c.DateTime(nullable: false),
                        esUrgente = c.Boolean(nullable: false),
                        estaAutorizado = c.Boolean(nullable: false),
                        hospitalId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Hospital", t => t.hospitalId, cascadeDelete: true)
                .Index(t => t.hospitalId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Pedido", "hospitalId", "dbo.Hospital");
            DropForeignKey("dbo.DetallePedido", "pedidoId", "dbo.Pedido");
            DropForeignKey("dbo.DetallePedido", "insumoId", "dbo.Insumo");
            DropIndex("dbo.Pedido", new[] { "hospitalId" });
            DropIndex("dbo.DetallePedido", new[] { "insumoId" });
            DropIndex("dbo.DetallePedido", new[] { "pedidoId" });
            DropTable("dbo.Pedido");
            DropTable("dbo.DetallePedido");
        }
    }
}
