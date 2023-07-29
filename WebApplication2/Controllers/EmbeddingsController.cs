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
            var result = await EmbeddingData.CreateAsync(embeddings, tokens, item.name, item.description);
            var nameEmbedding = result[0];
            var descriptionEmbedding = result[1];

            var dbItem = await db.Posts.FindAsync((long)item.id);
            if (dbItem == null)
            {
                dbItem = new WoocommercePost { Id = (long)item.id.Value, Name = item.name, NameEmbedding = nameEmbedding, DescriptionEmbedding = descriptionEmbedding };
                await db.Posts.AddAsync(dbItem);
            }
            else
            {
                // todo: check if text hasnt changed with hashid.
                dbItem.NameEmbedding = nameEmbedding;
                dbItem.DescriptionEmbedding = descriptionEmbedding;
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