namespace SIAH.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class added_info_registro : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DetalleRegistro", "info", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.DetalleRegistro", "info");
        }
    }
}
