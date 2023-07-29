using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Options;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication2.Models;
using WebApplication2.Services;
using static System.Net.Mime.MediaTypeNames;

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

    public DbSet<WoocommercePost> Posts { get; set; }
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
        modelBuilder.Entity<WoocommercePost>()
            .ToTable("woocommerce_posts")
            .OwnsOne(o => o.NameEmbedding, o =>
            {
                o.Property(a => a.Vector).HasColumnType("vector(1536)");

                o.HasIndex(i => i.Vector)
                  .HasMethod("ivfflat")
                  .HasOperators("vector_cosine_ops");
            });

        modelBuilder.Entity<WoocommercePost>()
            .OwnsOne(o => o.DescriptionEmbedding, o =>
            {
                o.Property(a => a.Vector).HasColumnType("vector(1536)");

                o.HasIndex(i => i.Vector)
                  .HasMethod("ivfflat")
                  .HasOperators("vector_cosine_ops");
            });

        modelBuilder.Entity<SearchResultItem>()
            .HasNoKey()
            .ToView(null);
    }
}

public class WoocommercePost
{
    public long Id { get; set; }

    [StringLength(128)]
    public string? Name { get; set; }

    public EmbeddingData? NameEmbedding { get; set; }
    public EmbeddingData? DescriptionEmbedding { get; set; }
}

public class EmbeddingData
{
    public string? HashId { get; set; }

    public Vector? Vector { get; set; }

    public static async Task<IList<EmbeddingData?>> CreateAsync(IEmbeddingsService embeddings, 
        ITokensService tokens,
        params string[] texts)
    {
        var hash = new HashidsNet.Hashids();
        var builder = new EmbeddingsOptionsBuilder();

        var hashes = new List<string>();
        foreach (var item in texts)
        {
            var normalizedText = tokens.Normalize(OpenAIModel.Ada002, item);
            var tokensIds = tokens.Encode(OpenAIModel.Ada002, normalizedText);
            hashes.Add(hash.Encode(tokensIds));

            builder.WithContent(normalizedText);
        }

        var resultEmbeddings = await embeddings.CreateAsync(builder);
        var result = new List<EmbeddingData?>();
        for (int i = 0; i < resultEmbeddings.Data.Count; i++)
        {
            var item = resultEmbeddings.Data[i];
            result.Add(new EmbeddingData
            {
                HashId = hashes[i],
                Vector = new Vector(item.Embedding.ToArray())
            });
        }

        return result;
    }
}
