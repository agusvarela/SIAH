namespace SIAH.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class comparePassword : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.UserAccount", "confirmPassword", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.UserAccount", "confirmPassword", c => c.String(nullable: false));
        }
    }
}
