using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Text.Json.Nodes;

namespace Flaminco.JsonLocalizer;
public class JsonLocalizerManager
{
    private readonly JsonNode? JsonReader;
    public JsonLocalizerManager(IHttpContextAccessor httpContextAccessor, IWebHostEnvironment configuration)
    {
        JsonReader ??= JsonNode.Parse(json: File.ReadAllText(Path.Combine(configuration.WebRootPath, "Resources", $"{httpContextAccessor.GetAcceptLanguage()}.json")));
    }
    public string this[string text]
    {
        get => JsonReader[text]?.ToString() ?? text;
    }
}

