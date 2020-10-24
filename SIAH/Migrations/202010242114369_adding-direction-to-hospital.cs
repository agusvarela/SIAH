namespace SIAH.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addingdirectiontohospital : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Hospital", "direccion", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Hospital", "direccion");
        }
    }
}
