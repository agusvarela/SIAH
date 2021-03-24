namespace SIAH.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ajuste_compras_registro_presupuesto : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Compra", "fechaCargaCompra", c => c.DateTime(nullable: false));
            AddColumn("dbo.Compra", "proveedor", c => c.String());
            AddColumn("dbo.Compra", "cuilProveedor", c => c.String());
            AddColumn("dbo.Compra", "numeroComprobante", c => c.String());
            AddColumn("dbo.Hospital", "presupuestoDisponible", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.DetalleRegistro", "isNegative", c => c.Boolean(nullable: false));
            AddColumn("dbo.Registro", "info", c => c.String());
            AddColumn("dbo.Registro", "tipo", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Registro", "tipo");
            DropColumn("dbo.Registro", "info");
            DropColumn("dbo.DetalleRegistro", "isNegative");
            DropColumn("dbo.Hospital", "presupuestoDisponible");
            DropColumn("dbo.Compra", "numeroComprobante");
            DropColumn("dbo.Compra", "cuilProveedor");
            DropColumn("dbo.Compra", "proveedor");
            DropColumn("dbo.Compra", "fechaCargaCompra");
        }
    }
}
