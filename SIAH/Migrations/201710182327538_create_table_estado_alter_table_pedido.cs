namespace SIAH.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class create_table_estado_alter_table_pedido : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Estado",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        nombreEstado = c.String(),
                        isFinal = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.id);
            
            AddColumn("dbo.Pedido", "estadoId", c => c.Int(nullable: false));
            CreateIndex("dbo.Pedido", "estadoId");
            AddForeignKey("dbo.Pedido", "estadoId", "dbo.Estado", "id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Pedido", "estadoId", "dbo.Estado");
            DropIndex("dbo.Pedido", new[] { "estadoId" });
            DropColumn("dbo.Pedido", "estadoId");
            DropTable("dbo.Estado");
        }
    }
}
