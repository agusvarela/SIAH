namespace SIAH.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class creado_tablas_historico : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.HistoricoFarmacia",
                c => new
                    {
                        hospitalId = c.Int(nullable: false),
                        id = c.Int(nullable: false),
                        insumoId = c.Int(nullable: false),
                        fechaMovimiento = c.DateTime(nullable: false),
                        descripcion = c.String(),
                        cantidad = c.Int(nullable: false),
                        saldo = c.Int(nullable: false),
                        isNegative = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => new { t.hospitalId, t.id })
                .ForeignKey("dbo.Hospital", t => t.hospitalId, cascadeDelete: true)
                .ForeignKey("dbo.Insumo", t => t.insumoId, cascadeDelete: true)
                .Index(t => t.hospitalId)
                .Index(t => t.insumoId);
            
            CreateTable(
                "dbo.HistoricoFisico",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        insumoId = c.Int(nullable: false),
                        fechaMovimiento = c.DateTime(nullable: false),
                        descripcion = c.String(),
                        cantidad = c.Int(nullable: false),
                        saldo = c.Int(nullable: false),
                        isNegative = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Insumo", t => t.insumoId, cascadeDelete: true)
                .Index(t => t.insumoId);
            
            CreateTable(
                "dbo.HistoricoSIAH",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        insumoId = c.Int(nullable: false),
                        fechaMovimiento = c.DateTime(nullable: false),
                        descripcion = c.String(),
                        cantidad = c.Int(nullable: false),
                        saldo = c.Int(nullable: false),
                        isNegative = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Insumo", t => t.insumoId, cascadeDelete: true)
                .Index(t => t.insumoId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.HistoricoSIAH", "insumoId", "dbo.Insumo");
            DropForeignKey("dbo.HistoricoFisico", "insumoId", "dbo.Insumo");
            DropForeignKey("dbo.HistoricoFarmacia", "insumoId", "dbo.Insumo");
            DropForeignKey("dbo.HistoricoFarmacia", "hospitalId", "dbo.Hospital");
            DropIndex("dbo.HistoricoSIAH", new[] { "insumoId" });
            DropIndex("dbo.HistoricoFisico", new[] { "insumoId" });
            DropIndex("dbo.HistoricoFarmacia", new[] { "insumoId" });
            DropIndex("dbo.HistoricoFarmacia", new[] { "hospitalId" });
            DropTable("dbo.HistoricoSIAH");
            DropTable("dbo.HistoricoFisico");
            DropTable("dbo.HistoricoFarmacia");
        }
    }
}
