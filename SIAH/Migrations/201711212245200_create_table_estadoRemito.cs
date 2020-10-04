namespace SIAH.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class create_table_estadoRemito : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.EstadoRemito",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        nombreEstado = c.String(),
                    })
                .PrimaryKey(t => t.id);
            
            AddColumn("dbo.Remito", "estadoId", c => c.Int(nullable: false));
            CreateIndex("dbo.Remito", "estadoId");
            AddForeignKey("dbo.Remito", "estadoId", "dbo.EstadoRemito", "id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Remito", "estadoId", "dbo.EstadoRemito");
            DropIndex("dbo.Remito", new[] { "estadoId" });
            DropColumn("dbo.Remito", "estadoId");
            DropTable("dbo.EstadoRemito");
        }
    }
}
