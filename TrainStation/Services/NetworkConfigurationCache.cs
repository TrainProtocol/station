using System.Text.Json;
using Train.Station.API.Models;

namespace Train.Station.API.Services;

public class NetworkConfigurationCache
{
    private readonly List<NetworkConfiguration> _networks;

    public NetworkConfigurationCache(IWebHostEnvironment env)
    {
        var filePath = Path.Combine(env.ContentRootPath, "networks.json");
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Could not find networks.json", filePath);
        }

        var json = File.ReadAllText(filePath);
        _networks = JsonSerializer.Deserialize<List<NetworkConfiguration>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new List<NetworkConfiguration>();
    }

    public IReadOnlyList<NetworkConfiguration> GetAll() => _networks;
}