namespace SIAH.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class create_tipos_insumo_table : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TipoInsumo",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        nombre = c.String(nullable: false, maxLength: 255),
                    })
                .PrimaryKey(t => t.id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.TipoInsumo");
        }
    }
}
