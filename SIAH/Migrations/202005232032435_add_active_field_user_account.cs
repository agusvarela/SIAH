namespace SIAH.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_active_field_user_account : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserAccount", "active", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserAccount", "active");
        }
    }
}
