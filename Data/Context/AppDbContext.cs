using Microsoft.EntityFrameworkCore;
using System;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Data.Model;
using Microsoft.EntityFrameworkCore.Storage;

namespace Data.Context
{
    public class AppDbContext: DbContext
    {
        public DbSet<Keyword> Keyword { get; set; }
        public DbSet<Detail> Detail { get; set; }
        public DbSet<RecipeKeyword> RecipeKeyword { get; set; }
        public DbSet<RecipeLink> RecipeLink { get; set; }

        private IConfiguration Configuration {get; set;}
        private string? dbConnectionString;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            Configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, "api"))
            .AddJsonFile("appsettings.json",optional:false, reloadOnChange: true).Build();

            dbConnectionString = Configuration.GetConnectionString("MySQL_Connection_String");
            optionsBuilder.UseMySql(dbConnectionString, ServerVersion.AutoDetect(dbConnectionString));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<RecipeLink>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasAlternateKey(e=>e.Url);
            });


            modelBuilder.Entity<Detail>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Title).IsRequired();
                entity.Property(e => e.Image).IsRequired();
                entity.Property(e => e.TotalTime).IsRequired();
                entity.Property(e => e.Ingredient).IsRequired();
                entity.Property(e => e.Instruction).IsRequired();
                entity.Property(e => e.RecipeId).IsRequired();
                // relationship to recipe link
                entity.HasOne(e => e.RecipeLink).WithOne(e => e.Detail).HasForeignKey<Detail>(e => e.RecipeId).IsRequired();
                
            });

            modelBuilder.Entity<Keyword>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.KeywordName).IsRequired();

            });

            modelBuilder.Entity<RecipeKeyword>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.KeywordId).IsRequired();
                entity.Property(e => e.DetailId).IsRequired();
                entity.HasOne(e => e.Keyword).WithMany(e=>e.RecipeKeyword);
                entity.HasOne(e => e.Detail).WithMany(e => e.RecipeKeyword);
            });

        }

        

    }
}