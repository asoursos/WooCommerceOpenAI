using Pgvector;
using System.ComponentModel.DataAnnotations;
using WebApplication2.Helpers;
using WebApplication2.Services;

namespace WebApplication2.Models;

public class EmbeddingData
{
    public ulong HashId { get; set; }

    public Vector Vector { get; set; }
}

public class WoocommercePost
{
    public long Id { get; set; }

    [StringLength(128)]
    public string? Name { get; set; }

    public EmbeddingData? NameEmbedding { get; set; }
    public EmbeddingData? DescriptionEmbedding { get; set; }

    public async Task UpdateAsync(IEmbeddingsService embeddings,
        ITokensService tokens,
        string name,
        string description)
    {
        Name = name;

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
