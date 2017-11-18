namespace SIAH.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class create_table_compras_detalleCompras : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Compra",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        fechaEntregaEfectiva = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.DetalleCompra",
                c => new
                    {
                        compraId = c.Int(nullable: false),
                        insumoId = c.Int(nullable: false),
                        cantidadComprada = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.compraId, t.insumoId })
                .ForeignKey("dbo.Compra", t => t.compraId, cascadeDelete: true)
                .ForeignKey("dbo.Insumo", t => t.insumoId, cascadeDelete: true)
                .Index(t => t.compraId)
                .Index(t => t.insumoId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DetalleCompra", "insumoId", "dbo.Insumo");
            DropForeignKey("dbo.DetalleCompra", "compraId", "dbo.Compra");
            DropIndex("dbo.DetalleCompra", new[] { "insumoId" });
            DropIndex("dbo.DetalleCompra", new[] { "compraId" });
            DropTable("dbo.DetalleCompra");
            DropTable("dbo.Compra");
        }
    }
}
