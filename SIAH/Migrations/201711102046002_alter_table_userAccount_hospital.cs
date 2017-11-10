namespace SIAH.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class alter_table_userAccount_hospital : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserAccount", "hospitalID", c => c.Int());
            CreateIndex("dbo.UserAccount", "hospitalID");
            AddForeignKey("dbo.UserAccount", "hospitalID", "dbo.Hospital", "id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserAccount", "hospitalID", "dbo.Hospital");
            DropIndex("dbo.UserAccount", new[] { "hospitalID" });
            DropColumn("dbo.UserAccount", "hospitalID");
        }
    }
}
