namespace SIAH.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class create_table_roles_alter_table_useraccounts : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Rol",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        nombre = c.String(nullable: false, maxLength: 150),
                    })
                .PrimaryKey(t => t.id);
            
            AddColumn("dbo.UserAccount", "rolID", c => c.Int(nullable: false));
            CreateIndex("dbo.UserAccount", "rolID");
            AddForeignKey("dbo.UserAccount", "rolID", "dbo.Rol", "id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserAccount", "rolID", "dbo.Rol");
            DropIndex("dbo.UserAccount", new[] { "rolID" });
            DropColumn("dbo.UserAccount", "rolID");
            DropTable("dbo.Rol");
        }
    }
}
