namespace SIAH.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class alter_table_pedido : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Pedido", "trackingNumber", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Pedido", "trackingNumber");
        }
    }
}
