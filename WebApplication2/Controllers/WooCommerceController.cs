using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;
using NpgsqlTypes;
using WebApplication2.Data;
using WebApplication2.Models;
using WebApplication2.Services;

namespace WebApplication2.Controllers;

public record SearchRequest(string? Term, float Threshold = 0.78f, int Limit = 5);
public record FuzzySearchRequest(string? Term, int Limit = 5);

[ApiController]
[Route("[controller]")]
public class WooCommerceController : ControllerBase
{
    [Route("")]
    [HttpGet]
    public async Task<IActionResult> GetAsync([FromServices] IWooCommerceService wooCommerce)
    {
        var products = await wooCommerce.GetProductsAsync();
        return Ok(products);
    }

    [Route("search")]
    [HttpGet]
    public async Task<IActionResult> SearchAsync([FromServices] ItemDbContext db,
        [FromServices] IEmbeddingsService embeddings,
        [FromServices] IOptions<WooCommerceSettings> options,
        [FromQuery] SearchRequest? request)
    {
        if (string.IsNullOrWhiteSpace(request?.Term)
            || request.Threshold > 1
            || request.Threshold < 0
            || request.Limit < 1)
        {
            return BadRequest();
        }


        var builder = new EmbeddingsOptionsBuilder().WithContent(request.Term);
        var result = await embeddings.CreateAsync(builder);
        var embedding = new Pgvector.Vector(result.Data.First().Embedding.ToArray());
        var parameters = new NpgsqlParameter[] {
            new NpgsqlParameter("@query_embedding", typeof(Pgvector.Vector)) { Value = embedding },
            new NpgsqlParameter("@match_threshold", NpgsqlDbType.Numeric) { Value = request.Threshold },
            new NpgsqlParameter("@match_count", NpgsqlDbType.Integer) { Value = request.Limit },
        };
        var items = await db.Search.FromSqlRaw($"SELECT id, name, name_similarity, description_similarity from match_posts(@query_embedding, @match_threshold, @match_count)", parameters).ToArrayAsync();
        
        var apiUri = new Uri(options.Value.ApiUrl!);
        var woocommerceUrl = apiUri.OriginalString.Replace(apiUri.PathAndQuery, "");
        var searchResult = items
            .OrderByDescending(x => Math.Max(x.NameSimilarity, x.DescriptionSimilarity))
            .Select(x => new SearchResult(x, new Uri($"{woocommerceUrl}?page_id={x.Id}")))
            .ToArray();

        return Ok(searchResult);
    }

    public record SearchResult(SearchResultItem Item, Uri Link);

    [Route("search/similarity")]
    [HttpGet]
    public async Task<IActionResult> FuzzySearchAsync([FromServices] ItemDbContext db,
        [FromQuery] FuzzySearchRequest? request)
    {
        if (string.IsNullOrWhiteSpace(request?.Term)
            || request.Limit < 1)
        {
            return BadRequest();
        }

        var parameters = new NpgsqlParameter[] {
            new NpgsqlParameter("@query_term", NpgsqlDbType.Text) { Value = request.Term },
            new NpgsqlParameter("@match_count", NpgsqlDbType.Integer) { Value = request.Limit },
        };
        var items = await db.FuzzySearch.FromSqlRaw($"SELECT id, name, name_similarity from match_posts_similarity(@query_term, @match_count)", parameters).ToArrayAsync();

        return Ok(items);
    }
}