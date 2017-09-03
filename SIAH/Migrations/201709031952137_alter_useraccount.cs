namespace SIAH.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class alter_useraccount : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.UserAccount", "rolID", "dbo.Rol");
            DropIndex("dbo.UserAccount", new[] { "rolID" });
            AlterColumn("dbo.UserAccount", "rolID", c => c.Int());
            CreateIndex("dbo.UserAccount", "rolID");
            AddForeignKey("dbo.UserAccount", "rolID", "dbo.Rol", "id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserAccount", "rolID", "dbo.Rol");
            DropIndex("dbo.UserAccount", new[] { "rolID" });
            AlterColumn("dbo.UserAccount", "rolID", c => c.Int(nullable: false));
            CreateIndex("dbo.UserAccount", "rolID");
            AddForeignKey("dbo.UserAccount", "rolID", "dbo.Rol", "id", cascadeDelete: true);
        }
    }
}
