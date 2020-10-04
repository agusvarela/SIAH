namespace SIAH.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class alter_table_hospital : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Hospital", "latitud", c => c.String());
            AddColumn("dbo.Hospital", "longitud", c => c.String());
            AddColumn("dbo.Hospital", "telefono", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Hospital", "telefono");
            DropColumn("dbo.Hospital", "longitud");
            DropColumn("dbo.Hospital", "latitud");
        }
    }
}
