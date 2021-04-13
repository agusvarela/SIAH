namespace SIAH.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class agregado_saldos_tablas_historicos : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.HistoricoFarmacia", "saldo", c => c.Int(nullable: false));
            AddColumn("dbo.HistoricoFisico", "saldo", c => c.Int(nullable: false));
            AddColumn("dbo.HistoricoSIAH", "saldo", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.HistoricoSIAH", "saldo");
            DropColumn("dbo.HistoricoFisico", "saldo");
            DropColumn("dbo.HistoricoFarmacia", "saldo");
        }
    }
}
