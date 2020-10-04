namespace SIAH.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class create_userAccounts_table : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserAccount",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        nombre = c.String(nullable: false),
                        apellido = c.String(nullable: false),
                        email = c.String(),
                        password = c.String(nullable: false),
                        confirmPassword = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.id);
            
            AlterColumn("dbo.Pedido", "fechaEntrega", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Pedido", "fechaEntrega", c => c.DateTime(nullable: false));
            DropTable("dbo.UserAccount");
        }
    }
}
