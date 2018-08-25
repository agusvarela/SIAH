namespace SIAH.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class create_table_stockFarmacia : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.StockFarmacia",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        hospitalId = c.Int(nullable: false),
                        insumoId = c.Int(nullable: false),
                        stockFarmacia = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Hospital", t => t.hospitalId, cascadeDelete: true)
                .ForeignKey("dbo.Insumo", t => t.insumoId, cascadeDelete: true)
                .Index(t => t.hospitalId)
                .Index(t => t.insumoId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.StockFarmacia", "insumoId", "dbo.Insumo");
            DropForeignKey("dbo.StockFarmacia", "hospitalId", "dbo.Hospital");
            DropIndex("dbo.StockFarmacia", new[] { "insumoId" });
            DropIndex("dbo.StockFarmacia", new[] { "hospitalId" });
            DropTable("dbo.StockFarmacia");
        }
    }
}
