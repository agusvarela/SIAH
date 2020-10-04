namespace SIAH.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class alter_table_insumo_insumoOcasa_stockFisico : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Insumo", "stockFisico", c => c.Int(nullable: false));
            AddColumn("dbo.InsumoOcasa", "stockFisico", c => c.Int(nullable: false));
            DropColumn("dbo.InsumoOcasa", "stockDisponible");
        }
        
        public override void Down()
        {
            AddColumn("dbo.InsumoOcasa", "stockDisponible", c => c.Int(nullable: false));
            DropColumn("dbo.InsumoOcasa", "stockFisico");
            DropColumn("dbo.Insumo", "stockFisico");
        }
    }
}
