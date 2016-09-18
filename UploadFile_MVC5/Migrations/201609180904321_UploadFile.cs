namespace UploadFile_MVC5.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UploadFile : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserFiles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(maxLength: 128),
                        UserFile = c.Binary(),
                        FileType = c.String(),
                        FileName = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.User",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        UserName = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserFiles", "UserId", "dbo.User");
            DropIndex("dbo.UserFiles", new[] { "UserId" });
            DropTable("dbo.User");
            DropTable("dbo.UserFiles");
        }
    }
}
