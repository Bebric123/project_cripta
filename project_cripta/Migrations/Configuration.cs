namespace project_cripta.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<project_cripta.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "project_cripta.Models.ApplicationDbContext";
        }

        protected override void Seed(project_cripta.Models.ApplicationDbContext context)
        {
        }
    }
}
