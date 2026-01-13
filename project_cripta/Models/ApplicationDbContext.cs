using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using System.Reflection.Emit;
using project_cripta.Models;

namespace project_cripta.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() : base("DefaultConnection")
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserBalance> UserBalances { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .Property(u => u.CreatedAt)
                .HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}