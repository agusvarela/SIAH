namespace SIAH.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class create_tables_reclamos : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.EstadoReclamo",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        nombreEstado = c.String(),
                        isFinal = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.Reclamo",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        observacionFamacia = c.String(),
                        respuesta = c.String(),
                        fechaInicioReclamo = c.DateTime(nullable: false),
                        fechaFinReclamo = c.DateTime(),
                        tipoReclamoId = c.Int(nullable: false),
                        pedidoId = c.Int(nullable: false),
                        hospitalId = c.Int(nullable: false),
                        responsableAsignadoId = c.Int(nullable: false),
                        estadoReclamoId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.EstadoReclamo", t => t.estadoReclamoId, cascadeDelete: true)
                .ForeignKey("dbo.Hospital", t => t.hospitalId, cascadeDelete: true)
                .ForeignKey("dbo.Pedido", t => t.pedidoId, cascadeDelete: false)
                .ForeignKey("dbo.UserAccount", t => t.responsableAsignadoId, cascadeDelete: true)
                .ForeignKey("dbo.TipoReclamo", t => t.tipoReclamoId, cascadeDelete: true)
                .Index(t => t.tipoReclamoId)
                .Index(t => t.pedidoId)
                .Index(t => t.hospitalId)
                .Index(t => t.responsableAsignadoId)
                .Index(t => t.estadoReclamoId);
            
            CreateTable(
                "dbo.TipoReclamo",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        tipo = c.String(),
                        descripcion = c.String(),
                    })
                .PrimaryKey(t => t.id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Reclamo", "tipoReclamoId", "dbo.TipoReclamo");
            DropForeignKey("dbo.Reclamo", "responsableAsignadoId", "dbo.UserAccount");
            DropForeignKey("dbo.Reclamo", "pedidoId", "dbo.Pedido");
            DropForeignKey("dbo.Reclamo", "hospitalId", "dbo.Hospital");
            DropForeignKey("dbo.Reclamo", "estadoReclamoId", "dbo.EstadoReclamo");
            DropIndex("dbo.Reclamo", new[] { "estadoReclamoId" });
            DropIndex("dbo.Reclamo", new[] { "responsableAsignadoId" });
            DropIndex("dbo.Reclamo", new[] { "hospitalId" });
            DropIndex("dbo.Reclamo", new[] { "pedidoId" });
            DropIndex("dbo.Reclamo", new[] { "tipoReclamoId" });
            DropTable("dbo.TipoReclamo");
            DropTable("dbo.Reclamo");
            DropTable("dbo.EstadoReclamo");
        }
    }
}
