using System.Text.Json;

namespace Train.Station.API.Services;

public abstract class JsonFileCache<TCollection, TItem>
{
    protected TCollection Items { get; }

    protected JsonFileCache(
        IWebHostEnvironment env,
        string fileName,
        Func<List<TItem>, TCollection> transform)
    {
        var filePath = Path.Combine(env.ContentRootPath, fileName);
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Could not find {fileName}", filePath);
        }

        var json = File.ReadAllText(filePath);
        var deserialized = JsonSerializer.Deserialize<List<TItem>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new InvalidOperationException($"Deserialization of {fileName} returned null.");

        Items = transform(deserialized);
    }
}