using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Routing;
using StackExchange.Redis;
using System.Text.Json;
using System.Text.Json.Nodes;
using Train.Station.Client;

namespace Train.Station.API.Services;

public class RouteCache(
    IDatabase cache,
    IConnectionMultiplexer connectionMultiplexer,
    NetworkConfigurationCache networkConfigCache)
{
    private readonly HashSet<string> _validNetworkNames = networkConfigCache
        .GetAll()
        .Select(n => n.Name)
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    public async Task AddOrUpdateRouteAsync(string lpId, IEnumerable<RouteDto> routes, TimeSpan ttl)
    {
        foreach (var route in routes)
        {
            if (!_validNetworkNames.Contains(route.Source.Network.Name) ||
                !_validNetworkNames.Contains(route.Destionation.Network.Name))
            {
                continue;
            }

            await cache.SortedSetAddAsync(
                "ROUTES", 
                JsonSerializer.Serialize(route),
                DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            await cache.SetAddAsync(
                GetRouteKey("LP", route),
                new RedisValue(lpId));
        }
    }

    public async Task<IEnumerable<RouteDto>> GetAllAsync()
    {
        var now = DateTimeOffset.UtcNow;
        var start = now.AddMinutes(-5).ToUnixTimeSeconds();
        var end = now.ToUnixTimeSeconds();

        var members = await cache.SortedSetRangeByScoreAsync("ROUTES", start, end);

        var entries = new List<RouteDto>();

        foreach (var member in members)
        {
            try
            {
                var dto = JsonSerializer.Deserialize<RouteDto>(member);
                if (dto != null)
                    entries.Add(dto);
            }
            catch
            {
            }
        }

        return entries;
    }

    public async Task<IEnumerable<string>> GetLpsByRouteAsync(string sourceNetwork, string sourceToken, string destinationNetwork, string destinationToken)
    {
        var members = await cache.SetMembersAsync(GetRouteKey("LP", sourceNetwork, sourceToken, destinationNetwork, destinationToken));
        return members.Select(m => m.ToString());
    }

    private static string GetRouteKey(string prefix, string sourceNetwork, string sourceToken, string destNetwork, string destToken)
    {
        return $"{prefix}:{sourceNetwork}.{sourceToken}->{destNetwork}.{destToken}";
    }

    private static string GetRouteKey(string prefix, RouteDto r)
    {
        return GetRouteKey(
            prefix,
            r.Source.Network.Name,
            r.Source.Token.Symbol,
            r.Destionation.Network.Name,
            r.Destionation.Token.Symbol
        );
    }
}
