namespace SIAH.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class alter_table_remito_non_incremental_id : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.DetalleRemito", "remitoId", "dbo.Remito");
            DropPrimaryKey("dbo.Remito");
            AlterColumn("dbo.Remito", "id", c => c.Int(nullable: false));
            AddPrimaryKey("dbo.Remito", "id");
            AddForeignKey("dbo.DetalleRemito", "remitoId", "dbo.Remito", "id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DetalleRemito", "remitoId", "dbo.Remito");
            DropPrimaryKey("dbo.Remito");
            AlterColumn("dbo.Remito", "id", c => c.Int(nullable: false, identity: true));
            AddPrimaryKey("dbo.Remito", "id");
            AddForeignKey("dbo.DetalleRemito", "remitoId", "dbo.Remito", "id", cascadeDelete: true);
        }
    }
}
