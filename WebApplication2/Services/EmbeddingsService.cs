using Azure.AI.OpenAI;
using Microsoft.Extensions.Options;
using WebApplication2.Helpers;
using WebApplication2.Models;

namespace WebApplication2.Services;

public interface IEmbeddingsService
{
    Task<Embeddings> CreateAsync(EmbeddingsOptionsBuilder builder);
}

public class EmbeddingsService : IEmbeddingsService
{
    private readonly OpenAIClient _client;

    public EmbeddingsService(IOptions<OpenAISettings> options)
    {
        _client = new OpenAIClient(options.Value.ApiKey);
    }

    public async Task<Embeddings> CreateAsync(EmbeddingsOptionsBuilder builder)
    {
        var options = builder.Build();
        var response = await _client.GetEmbeddingsAsync(OpenAIModel.Ada002.GetDescription(), options);
        return response.Value;
    }
}


public class EmbeddingsOptionsBuilder
{
    private readonly EmbeddingsOptions _options;

    public EmbeddingsOptionsBuilder()
    {
        _options = new EmbeddingsOptions(new List<string>());
    }

    public EmbeddingsOptionsBuilder WithContent(string text)
    {
        if (string.IsNullOrWhiteSpace(text) == false)
        {
            _options.Input.Add(text);
        }

        return this;
    }

    public EmbeddingsOptions Build()
    {
        return _options;
    }
}
