namespace SIAH.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class create_insumos_table : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Insumo",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        nombre = c.String(nullable: false, maxLength: 255),
                        precioUnitario = c.Decimal(nullable: false, precision:15, scale:2),
                        tipoInsumoId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.TipoInsumo", t => t.tipoInsumoId, cascadeDelete: true)
                .Index(t => t.tipoInsumoId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Insumo", "tipoInsumoId", "dbo.TipoInsumo");
            DropIndex("dbo.Insumo", new[] { "tipoInsumoId" });
            DropTable("dbo.Insumo");
        }
    }
}
