using System.ComponentModel;
using System.Text.RegularExpressions;
using TiktokenSharp;
using WebApplication2.Helpers;

namespace WebApplication2.Services;

public enum OpenAIModel
{
    [Description("text-embedding-ada-002")]
    Ada002
}

public interface ITokensService 
{
    List<int> Encode(OpenAIModel model, string text);
    string Decode(OpenAIModel model, List<int> tokens);
    string Normalize(OpenAIModel model, string text);
}

public class TokensService : ITokensService
{
    // https://platform.openai.com/docs/guides/embeddings/second-generation-models
    private const int ADA002_LIMIT = 8191;

    public string Decode(OpenAIModel model, List<int> tokens)
    {
        TikToken tikToken = TikToken.EncodingForModel(model.GetDescription());
        return tikToken.Decode(tokens);
    }

    public List<int> Encode(OpenAIModel model, string text)
    {
        TikToken tikToken = TikToken.EncodingForModel(model.GetDescription());
        return tikToken.Encode(text);
    }

    public string Normalize(OpenAIModel model, string text)
    {
        text = PrepareText(text);

        TikToken tikToken = TikToken.EncodingForModel(model.GetDescription());
        var tokens = tikToken.Encode(text);
        var limit = GetLimit(model);
        if (tokens.Count > limit)
        {
            return tikToken.Decode(tokens.Take(limit).ToList());
        }

        return text;
    }

    private int GetLimit(OpenAIModel model)
    {
        return model switch
        {
            OpenAIModel.Ada002 => ADA002_LIMIT,
            _ => throw new NotImplementedException(),
        };
    }

    private static string PrepareText(string input)
    {
        if (input == null)
        {
            return string.Empty;
        }

        var result = Regex.Replace(input, "<.*?>", string.Empty);
        return result.Replace('\n', ' ').Replace('\r', ' ').TrimEnd();
    }
}
