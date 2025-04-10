using CensusFieldSurvey.DataBase.Interceptors;
using CensusFieldSurvey.Model.EntitesBD;
using Gestao.Data;
using Gestao.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CensusFieldSurvey.DataBase
{
    // Change the class name to be consistent
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Research> Researchs { get; set; }

        // aqui é o do projeto gfestão....
        public DbSet<Company> Companies { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<FinancialTransaction> FinancialTransactions { get; set; }
        public DbSet<Document> Documents { get; set; }

        public AppDbContext()
        {
            Database.Migrate();
        }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            // Remove Database.Migrate() from here
            // It should be called during application startup, not in the constructor
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = "Host=localhost;Port=5432;Database=API4;Username=postgres;Password=postgres";

                optionsBuilder.UseNpgsql(connectionString);
            }

            optionsBuilder.AddInterceptors(new SoftDeleteInterceptor());
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure Identity tables
            builder.Entity<IdentityUser>(entity =>
            {
                entity.Property(e => e.Id).HasColumnType("text");
                entity.Property(e => e.UserName).HasColumnType("text");
                entity.Property(e => e.NormalizedUserName).HasColumnType("text");
                entity.Property(e => e.Email).HasColumnType("text");
                entity.Property(e => e.NormalizedEmail).HasColumnType("text");
                entity.Property(e => e.PasswordHash).HasColumnType("text");
                entity.Property(e => e.SecurityStamp).HasColumnType("text");
                entity.Property(e => e.ConcurrencyStamp).HasColumnType("text");
                entity.Property(e => e.PhoneNumber).HasColumnType("text");
            });

            builder.Entity<IdentityRole>(entity =>
            {
                entity.Property(e => e.Id).HasColumnType("text");
                entity.Property(e => e.Name).HasColumnType("text");
                entity.Property(e => e.NormalizedName).HasColumnType("text");
                entity.Property(e => e.ConcurrencyStamp).HasColumnType("text");
            });


            builder.Entity<FinancialTransaction>()
                .Property(a => a.Repeat)
                .HasConversion<string>();

            builder.Entity<FinancialTransaction>()
                .Property(a => a.TypeFinancialTransaction)
                .HasConversion<string>();

            builder.Entity<Company>().HasIndex(a => a.TaxId).IsUnique();

            builder.Entity<ApplicationUser>().HasQueryFilter(a => a.DeletedAt == null);
            builder.Entity<Account>().HasQueryFilter(a => a.DeletedAt == null);
            builder.Entity<Company>().HasQueryFilter(a => a.DeletedAt == null);
            builder.Entity<Category>().HasQueryFilter(a => a.DeletedAt == null);
            builder.Entity<FinancialTransaction>().HasQueryFilter(a => a.DeletedAt == null);
            builder.Entity<Document>().HasQueryFilter(a => a.DeletedAt == null);
        }
    }
}