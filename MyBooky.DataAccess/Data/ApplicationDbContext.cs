using Microsoft.EntityFrameworkCore;
using MyBooky.Models;

namespace MyBookyWeb.DataAccess.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) 
        {
            
        }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>().HasData(
                new Category { Id=1,Name="Mathematics",DisplayOrder=1},
                new Category { Id = 2, Name = "Physics", DisplayOrder = 2 },
                new Category { Id = 3, Name = "History", DisplayOrder = 3 });
        }
    }
}
