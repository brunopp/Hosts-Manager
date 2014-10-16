namespace HostsManager.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Mappings",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Domain = c.String(maxLength: 254),
                        IP = c.String(maxLength: 15),
                        Active = c.Boolean(nullable: false),
                        ShowInfoBox = c.Boolean(nullable: false),
                        XLeft = c.Int(),
                        XRight = c.Int(),
                        YTop = c.Int(),
                        YBottom = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Mappings");
        }
    }
}
