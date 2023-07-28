using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Options;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication2.Models;

namespace WebApplication2.Data;

public class DatabaseFactory : IDesignTimeDbContextFactory<ItemDbContext>
{
    public ItemDbContext CreateDbContext(string[] args)
    {
        var connectionString = "Host=127.0.0.1;Port=5433;Username=postgres;Password=postgres;Database=dbvector-design";

        var optionsBuilder = new DbContextOptionsBuilder<ItemDbContext>();
        optionsBuilder.UseNpgsql(connectionString, o => o.UseVector()).UseSnakeCaseNamingConvention()
            .EnableDetailedErrors()
            .EnableSensitiveDataLogging();

        return new ItemDbContext(optionsBuilder.Options, null);
    }
}

[Keyless]
public record SearchResultItem(long Id, string Name, double NameSimilarity, double DescriptionSimilarity);


public class ItemDbContext : DbContext
{
    private readonly DatabaseSettings? _settings;

    public ItemDbContext(DbContextOptions<ItemDbContext> dbOptions, IOptions<DatabaseSettings>? options = null) 
        : base(dbOptions)
    {
        _settings = options?.Value;
    }

    public DbSet<Post> Posts { get; set; }
    public virtual DbSet<SearchResultItem> Search { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured == false && _settings != null)
        {
            var builder = new Npgsql.NpgsqlConnectionStringBuilder()
            {
                Port = _settings.Port,
                Database = _settings.DatabaseName,
                Host = _settings.Server,
                Username = _settings.UserId,
                Password = _settings.Password,
            };

            var connString = builder.ConnectionString;
            optionsBuilder.UseNpgsql(connString, o => o.UseVector()).UseSnakeCaseNamingConvention();
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("vector");

        // TODO set lists = 1
        modelBuilder.Entity<Post>()
            .HasIndex(i => i.NameVector)
            .HasMethod("ivfflat")
            .HasOperators("vector_cosine_ops");

        modelBuilder.Entity<Post>()
            .HasIndex(i => i.DescriptionVector)
            .HasMethod("ivfflat")
            .HasOperators("vector_cosine_ops");

        modelBuilder.Entity<SearchResultItem>()
            .HasNoKey()
            .ToView(null);
    }
}

[Table("woocommerce_posts")]
public class Post
{
    public long Id { get; set; }

    [StringLength(128)]
    public string? Name { get; set; }

    [Column(TypeName = "vector(1536)")]
    public Vector? NameVector { get; set; }

    [Column(TypeName = "vector(1536)")]
    public Vector? DescriptionVector { get; set; }
}
