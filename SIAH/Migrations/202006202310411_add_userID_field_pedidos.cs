namespace SIAH.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_userID_field_pedidos : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Pedido", "responsableAsignadoId", c => c.Int());
            CreateIndex("dbo.Pedido", "responsableAsignadoId");
            AddForeignKey("dbo.Pedido", "responsableAsignadoId", "dbo.UserAccount", "id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Pedido", "responsableAsignadoId", "dbo.UserAccount");
            DropIndex("dbo.Pedido", new[] { "responsableAsignadoId" });
            DropColumn("dbo.Pedido", "responsableAsignadoId");
        }
    }
}
