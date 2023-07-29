using Microsoft.AspNetCore.Mvc;
using WebApplication2.Data;
using WebApplication2.Models;
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
            var dbItem = await db.Posts.FindAsync((long)item.id);
            if (dbItem == null)
            {
                dbItem = new WoocommercePost { Id = (long)item.id.Value };
                await db.Posts.AddAsync(dbItem);
            }
            
            await dbItem.UpdateAsync(embeddings, tokens, item.name, item.description);

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