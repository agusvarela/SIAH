namespace SIAH.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ajuste_detalle : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AjusteSIAH",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        fechaGeneracion = c.DateTime(nullable: false),
                        info = c.String(),
                        usuarioId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.UserAccount", t => t.usuarioId, cascadeDelete: true)
                .Index(t => t.usuarioId);
            
            CreateTable(
                "dbo.DetalleAjusteSIAH",
                c => new
                    {
                        ajusteId = c.Int(nullable: false),
                        insumoId = c.Int(nullable: false),
                        cantidad = c.Int(nullable: false),
                        isNegative = c.Boolean(nullable: false),
                        info = c.String(),
                    })
                .PrimaryKey(t => new { t.ajusteId, t.insumoId })
                .ForeignKey("dbo.AjusteSIAH", t => t.ajusteId, cascadeDelete: true)
                .ForeignKey("dbo.Insumo", t => t.insumoId, cascadeDelete: true)
                .Index(t => t.ajusteId)
                .Index(t => t.insumoId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DetalleAjusteSIAH", "insumoId", "dbo.Insumo");
            DropForeignKey("dbo.DetalleAjusteSIAH", "ajusteId", "dbo.AjusteSIAH");
            DropForeignKey("dbo.AjusteSIAH", "usuarioId", "dbo.UserAccount");
            DropIndex("dbo.DetalleAjusteSIAH", new[] { "insumoId" });
            DropIndex("dbo.DetalleAjusteSIAH", new[] { "ajusteId" });
            DropIndex("dbo.AjusteSIAH", new[] { "usuarioId" });
            DropTable("dbo.DetalleAjusteSIAH");
            DropTable("dbo.AjusteSIAH");
        }
    }
}
