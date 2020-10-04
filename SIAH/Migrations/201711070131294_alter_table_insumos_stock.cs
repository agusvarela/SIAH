namespace SIAH.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class alter_table_insumos_stock : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Insumo", "stock", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Insumo", "stock");
        }
    }
}
