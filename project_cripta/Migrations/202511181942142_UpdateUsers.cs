namespace project_cripta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateUsers : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Transactions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        Type = c.String(nullable: false, maxLength: 10),
                        FromCurrency = c.String(nullable: false, maxLength: 10),
                        ToCurrency = c.String(nullable: false, maxLength: 10),
                        FromAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ToAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ExchangeRate = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Fee = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.UserBalances",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        Currency = c.String(nullable: false, maxLength: 10),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Transactions", "UserId", "dbo.Users");
            DropForeignKey("dbo.UserBalances", "UserId", "dbo.Users");
            DropIndex("dbo.UserBalances", new[] { "UserId" });
            DropIndex("dbo.Transactions", new[] { "UserId" });
            DropTable("dbo.UserBalances");
            DropTable("dbo.Transactions");
        }
    }
}
