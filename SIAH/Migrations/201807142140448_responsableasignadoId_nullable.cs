namespace SIAH.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class responsableasignadoId_nullable : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Reclamo", "responsableAsignadoId", "dbo.UserAccount");
            DropIndex("dbo.Reclamo", new[] { "responsableAsignadoId" });
            AlterColumn("dbo.Reclamo", "responsableAsignadoId", c => c.Int(nullable: true));
            CreateIndex("dbo.Reclamo", "responsableAsignadoId");
            AddForeignKey("dbo.Reclamo", "responsableAsignadoId", "dbo.UserAccount", "id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Reclamo", "responsableAsignadoId", "dbo.UserAccount");
            DropIndex("dbo.Reclamo", new[] { "responsableAsignadoId" });
            AlterColumn("dbo.Reclamo", "responsableAsignadoId", c => c.Int(nullable: false));
            CreateIndex("dbo.Reclamo", "responsableAsignadoId");
            AddForeignKey("dbo.Reclamo", "responsableAsignadoId", "dbo.UserAccount", "id", cascadeDelete: true);
        }
    }
}
