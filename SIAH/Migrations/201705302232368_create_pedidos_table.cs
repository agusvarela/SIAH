namespace SIAH.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class create_pedidos_table : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Pedido",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        periodo = c.DateTime(nullable: false),
                        fechaGeneracion = c.DateTime(nullable: false),
                        fechaEntrega = c.DateTime(nullable: true),
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
            DropIndex("dbo.Pedido", new[] { "hospitalId" });
            DropTable("dbo.Pedido");
        }
    }
}
