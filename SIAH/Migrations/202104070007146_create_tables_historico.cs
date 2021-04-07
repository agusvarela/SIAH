namespace SIAH.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class create_tables_historico : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.HistoricoFarmacia",
                c => new
                    {
                        hospitalId = c.Int(nullable: false),
                        insumoId = c.Int(nullable: false),
                        fechaMovimiento = c.DateTime(nullable: false),
                        descripcion = c.String(),
                        cantidad = c.Int(nullable: false),
                        isNegative = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => new { t.hospitalId, t.insumoId })
                .ForeignKey("dbo.Hospital", t => t.hospitalId, cascadeDelete: true)
                .ForeignKey("dbo.Insumo", t => t.insumoId, cascadeDelete: true)
                .Index(t => t.hospitalId)
                .Index(t => t.insumoId);
            
            CreateTable(
                "dbo.HistoricoFisico",
                c => new
                    {
                        insumoId = c.Int(nullable: false, identity: true),
                        fechaMovimiento = c.DateTime(nullable: false),
                        descripcion = c.String(),
                        cantidad = c.Int(nullable: false),
                        isNegative = c.Boolean(nullable: false),
                        insumo_id = c.Int(),
                    })
                .PrimaryKey(t => t.insumoId)
                .ForeignKey("dbo.Insumo", t => t.insumo_id)
                .Index(t => t.insumo_id);
            
            CreateTable(
                "dbo.HistoricoSIAH",
                c => new
                    {
                        insumoId = c.Int(nullable: false, identity: true),
                        fechaMovimiento = c.DateTime(nullable: false),
                        descripcion = c.String(),
                        cantidad = c.Int(nullable: false),
                        isNegative = c.Boolean(nullable: false),
                        insumo_id = c.Int(),
                    })
                .PrimaryKey(t => t.insumoId)
                .ForeignKey("dbo.Insumo", t => t.insumo_id)
                .Index(t => t.insumo_id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.HistoricoSIAH", "insumo_id", "dbo.Insumo");
            DropForeignKey("dbo.HistoricoFisico", "insumo_id", "dbo.Insumo");
            DropForeignKey("dbo.HistoricoFarmacia", "insumoId", "dbo.Insumo");
            DropForeignKey("dbo.HistoricoFarmacia", "hospitalId", "dbo.Hospital");
            DropIndex("dbo.HistoricoSIAH", new[] { "insumo_id" });
            DropIndex("dbo.HistoricoFisico", new[] { "insumo_id" });
            DropIndex("dbo.HistoricoFarmacia", new[] { "insumoId" });
            DropIndex("dbo.HistoricoFarmacia", new[] { "hospitalId" });
            DropTable("dbo.HistoricoSIAH");
            DropTable("dbo.HistoricoFisico");
            DropTable("dbo.HistoricoFarmacia");
        }
    }
}
