namespace SIAH.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class create_reclamos_tables : DbMigration
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
                        reclamoId = c.Int(nullable: false),
                        pedidoId = c.Int(nullable: false),
                        hospitalId = c.Int(nullable: false),
                        estadoId = c.Int(nullable: false),
                        estadoReclamo_id = c.Int(),
                        responsableAsignado_id = c.Int(),
                        tipoReclamo_id = c.Int(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.EstadoReclamo", t => t.estadoReclamo_id)
                .ForeignKey("dbo.Hospital", t => t.hospitalId, cascadeDelete: true)
                .ForeignKey("dbo.Pedido", t => t.pedidoId, cascadeDelete: false)
                .ForeignKey("dbo.UserAccount", t => t.responsableAsignado_id)
                .ForeignKey("dbo.TipoReclamo", t => t.tipoReclamo_id)
                .Index(t => t.pedidoId)
                .Index(t => t.hospitalId)
                .Index(t => t.estadoReclamo_id)
                .Index(t => t.responsableAsignado_id)
                .Index(t => t.tipoReclamo_id);
            
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
            DropForeignKey("dbo.Reclamo", "tipoReclamo_id", "dbo.TipoReclamo");
            DropForeignKey("dbo.Reclamo", "responsableAsignado_id", "dbo.UserAccount");
            DropForeignKey("dbo.Reclamo", "pedidoId", "dbo.Pedido");
            DropForeignKey("dbo.Reclamo", "hospitalId", "dbo.Hospital");
            DropForeignKey("dbo.Reclamo", "estadoReclamo_id", "dbo.EstadoReclamo");
            DropIndex("dbo.Reclamo", new[] { "tipoReclamo_id" });
            DropIndex("dbo.Reclamo", new[] { "responsableAsignado_id" });
            DropIndex("dbo.Reclamo", new[] { "estadoReclamo_id" });
            DropIndex("dbo.Reclamo", new[] { "hospitalId" });
            DropIndex("dbo.Reclamo", new[] { "pedidoId" });
            DropTable("dbo.TipoReclamo");
            DropTable("dbo.Reclamo");
            DropTable("dbo.EstadoReclamo");
        }
    }
}
