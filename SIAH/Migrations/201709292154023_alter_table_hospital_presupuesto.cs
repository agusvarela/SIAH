namespace SIAH.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class alter_table_hospital_presupuesto : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Hospital", "presupuesto", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Hospital", "presupuesto");
        }
    }
}
