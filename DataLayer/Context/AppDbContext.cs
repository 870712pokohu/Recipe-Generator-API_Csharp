using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using DataLayer.Model;

namespace DataLayer.Context
{
    public class AppDbContext: DbContext
    {
        public DbSet<Keyword> Keyword { get; set; }
        public DbSet<Detail> Detail { get; set; }
        public DbSet<RecipeKeyword> RecipeKeyword { get; set; }
        public DbSet<RecipeLink> RecipeLink { get; set; }

        private readonly IConfiguration _configuration;

        public AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration configuration): base(options)
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Avoid configuring if options have already been set (e.g., through Startup.cs)
            if (!optionsBuilder.IsConfigured)
            {
                string? dbConnectionString = _configuration.GetConnectionString("MySQL_Connection_String");
                optionsBuilder.UseMySql(dbConnectionString, ServerVersion.AutoDetect(dbConnectionString));
            }
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
                entity.HasKey(e => new{e.KeywordId,e.DetailId});
            });

        }

        

    }
}