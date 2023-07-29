using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Options;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication2.Helpers;
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

    public async Task UpdateVectorsAsync(IEmbeddingsService embeddings,
        ITokensService tokens,
        string name,
        string description)
    {

        var nameNormalizedText = tokens.Normalize(OpenAIModel.Ada002, name);
        var nameHashId = Hasher.CalculateDeterministicHash(nameNormalizedText);
        var nameHasChanges = nameHashId != NameEmbedding?.HashId;

        var descNormalizedText = tokens.Normalize(OpenAIModel.Ada002, description);
        var descHashId = Hasher.CalculateDeterministicHash(descNormalizedText);
        var descHasChanges = descHashId != DescriptionEmbedding?.HashId;
        if (nameHasChanges == false && descHasChanges == false)
        {
            return;
        }

        var builder = new EmbeddingsOptionsBuilder();
        if (nameHasChanges)
        {
            builder.WithContent(nameNormalizedText);
        }

        if (descHasChanges)
        {
            builder.WithContent(descNormalizedText);
        }

        var resultEmbeddings = await embeddings.CreateAsync(builder);
        if (nameHasChanges)
        {
            NameEmbedding = new EmbeddingData
            {
                HashId = nameHashId,
                Vector = new Vector(resultEmbeddings.Data[0].Embedding.ToArray())
            };
        }

        if (descHasChanges)
        {
            var index = nameHasChanges ? 1 : 0;
            DescriptionEmbedding = new EmbeddingData
            {
                HashId = descHashId,
                Vector = new Vector(resultEmbeddings.Data[index].Embedding.ToArray())
            };
        }
    }
}

public class EmbeddingData
{
    public ulong HashId { get; set; }

    public Vector Vector { get; set; }
}
