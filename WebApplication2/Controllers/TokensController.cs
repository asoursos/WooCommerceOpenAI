using Microsoft.AspNetCore.Mvc;
using WebApplication2.Services;

namespace WebApplication2.Controllers;

[ApiController]
[Route("[controller]")]
public class TokensController : ControllerBase
{
    [Route("encode")]
    [HttpGet]
    public IActionResult Encode([FromServices] ITokensService tokens,
        [FromQuery] string term,
        [FromQuery] OpenAIModel model = OpenAIModel.Ada002)
    {
        var result = tokens.Encode(model, term);
        return Ok(result);
    }

    [Route("decode")]
    [HttpGet]
    public IActionResult Decode([FromServices] ITokensService tokens,
        [FromQuery] List<int> ids,
        [FromQuery] OpenAIModel model = OpenAIModel.Ada002)
    {
        var result = tokens.Decode(model, ids);
        return Ok(result);
    }
}
