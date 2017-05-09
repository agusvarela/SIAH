namespace SIAH.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class create_hospitales_table : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Hospital",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        nombre = c.String(nullable: false, maxLength: 255),
                        localidadId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Localidad", t => t.localidadId, cascadeDelete: true)
                .Index(t => t.localidadId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Hospital", "localidadId", "dbo.Localidad");
            DropIndex("dbo.Hospital", new[] { "localidadId" });
            DropTable("dbo.Hospital");
        }
    }
}
