namespace project_cripta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBalanceToUser : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "Balance", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "Balance");
        }
    }
}
