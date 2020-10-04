namespace SIAH.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class alter_table_pedido_reclamoId : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Reclamo", "pedidoId", "dbo.Pedido");
            DropPrimaryKey("dbo.Reclamo");
            AddColumn("dbo.Pedido", "reclamoId", c => c.Int());
            AddPrimaryKey("dbo.Reclamo", "pedidoId");
            AddForeignKey("dbo.Reclamo", "pedidoId", "dbo.Pedido", "id");
            DropColumn("dbo.Reclamo", "id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Reclamo", "id", c => c.Int(nullable: false, identity: true));
            DropForeignKey("dbo.Reclamo", "pedidoId", "dbo.Pedido");
            DropPrimaryKey("dbo.Reclamo");
            DropColumn("dbo.Pedido", "reclamoId");
            AddPrimaryKey("dbo.Reclamo", "id");
            AddForeignKey("dbo.Reclamo", "pedidoId", "dbo.Pedido", "id", cascadeDelete: true);
        }
    }
}
