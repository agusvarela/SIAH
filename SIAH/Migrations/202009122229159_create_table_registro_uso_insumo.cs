namespace SIAH.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class create_table_registro_uso_insumo : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DetalleRegistro",
                c => new
                    {
                        registroId = c.Int(nullable: false),
                        insumoId = c.Int(nullable: false),
                        cantidad = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.registroId, t.insumoId })
                .ForeignKey("dbo.Insumo", t => t.insumoId, cascadeDelete: true)
                .ForeignKey("dbo.Registro", t => t.registroId, cascadeDelete: true)
                .Index(t => t.registroId)
                .Index(t => t.insumoId);
            
            CreateTable(
                "dbo.Registro",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        fechaGeneracion = c.DateTime(nullable: false),
                        destinatario = c.String(),
                        usuarioId = c.Int(nullable: false),
                        hospitalId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Hospital", t => t.hospitalId, cascadeDelete: true)
                .ForeignKey("dbo.UserAccount", t => t.usuarioId, cascadeDelete: true)
                .Index(t => t.usuarioId)
                .Index(t => t.hospitalId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DetalleRegistro", "registroId", "dbo.Registro");
            DropForeignKey("dbo.Registro", "usuarioId", "dbo.UserAccount");
            DropForeignKey("dbo.Registro", "hospitalId", "dbo.Hospital");
            DropForeignKey("dbo.DetalleRegistro", "insumoId", "dbo.Insumo");
            DropIndex("dbo.Registro", new[] { "hospitalId" });
            DropIndex("dbo.Registro", new[] { "usuarioId" });
            DropIndex("dbo.DetalleRegistro", new[] { "insumoId" });
            DropIndex("dbo.DetalleRegistro", new[] { "registroId" });
            DropTable("dbo.Registro");
            DropTable("dbo.DetalleRegistro");
        }
    }
}
