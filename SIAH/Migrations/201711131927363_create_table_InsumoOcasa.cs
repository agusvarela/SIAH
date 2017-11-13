namespace SIAH.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class create_table_InsumoOcasa : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.InsumoOcasa",
                c => new
                    {
                        id = c.Int(nullable: false, identity: false),
                        nombre = c.String(),
                        stockDisponible = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.InsumoOcasa");
        }
    }
}
