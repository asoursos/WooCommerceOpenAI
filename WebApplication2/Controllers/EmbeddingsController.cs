using Microsoft.AspNetCore.Mvc;
using WebApplication2.Data;
using WebApplication2.Services;

namespace WebApplication2.Controllers;

public record EmbeddingRequest(long Id, string[] Items);

[ApiController]
[Route("[controller]")]
public class EmbeddingsController : ControllerBase
{
    public EmbeddingsController()
    {
        
    }

    [Route("sync")]
    [HttpPost]
    public async Task<IActionResult> SyncAsync([FromServices] ItemDbContext db,
        [FromServices] ITokensService tokens,
        [FromServices] IEmbeddingsService embeddings,
        [FromServices] IWooCommerceService wooCommerce)
    {
        var products = await wooCommerce.GetProductsAsync();

        foreach (var item in products)
        {
            var builder = new EmbeddingsOptionsBuilder()
                .WithContent(item.name)
                .WithContent(tokens.Normalize(OpenAIModel.Ada002, item.description));

            var result = await embeddings.CreateAsync(builder);
            if (result.Data.Any() == false || item.id.HasValue == false)
            {
                continue;
            }

            var nameVector = new Pgvector.Vector(result.Data.First().Embedding.ToArray());
            var descVector = new Pgvector.Vector(result.Data.Last().Embedding.ToArray());

            var dbItem = await db.Posts.FindAsync((long)item.id);
            if (dbItem == null)
            {
                dbItem = new Post { Id = (long)item.id.Value, Name = item.name, NameVector = nameVector, DescriptionVector = descVector };
                await db.Posts.AddAsync(dbItem);
            }
            else
            {
                dbItem.NameVector = nameVector;
                dbItem.DescriptionVector = descVector;
            }

            await db.SaveChangesAsync();
        }


        return Ok(products);
    }

    [Route("")]
    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromServices] IEmbeddingsService embeddings,
        [FromServices] ITokensService tokens,
        [FromBody] EmbeddingRequest request)
    {
        var builder = new EmbeddingsOptionsBuilder();
        foreach (var item in request.Items)
        {
            builder.WithContent(tokens.Normalize(OpenAIModel.Ada002, item));
        }

        var result = await embeddings.CreateAsync(builder);
        return Ok(result);
    }
}