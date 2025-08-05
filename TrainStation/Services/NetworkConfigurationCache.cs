using System.Text.Json;
using Train.Station.API.Models;

namespace Train.Station.API.Services;

public class NetworkConfigurationCache(
    IWebHostEnvironment env) 
    : JsonFileCache<List<NetworkConfiguration>, NetworkConfiguration>(
        env, "networks.json", list => list)
{
    public IReadOnlyList<NetworkConfiguration> GetAll() => Items;
}