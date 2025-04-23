using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using Train.Station.Client;

namespace Train.Station.API.Services;

public class RouteCache(IMemoryCache cache, NetworkConfigurationCache networkConfigCache)
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, bool>> _routeToLps = new();
    private readonly HashSet<string> _validNetworkNames = networkConfigCache
        .GetAll()
        .Select(n => n.Name)
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    public void AddOrUpdateRoute(string lpId, IEnumerable<RouteDto> routes, TimeSpan ttl)
    {
        foreach (var route in routes)
        {
            if (!_validNetworkNames.Contains(route.Source.Network.Name) ||
                !_validNetworkNames.Contains(route.Destionation.Network.Name))
            {
                continue;
            }

            string key = GetRouteKey(route);

            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl,
                PostEvictionCallbacks =
                {
                    new PostEvictionCallbackRegistration
                    {
                        EvictionCallback = (evictedKey, value, reason, state) =>
                        {
                            var routeKey = (string)evictedKey;

                            if (_routeToLps.TryGetValue(routeKey, out var lpSet))
                            {
                                lpSet.TryRemove(lpId, out _);

                                if (lpSet.IsEmpty)
                                    _routeToLps.TryRemove(routeKey, out _);
                            }
                        }
                    }
                }
            };

            cache.Set(key, route, cacheEntryOptions);

            if (_routeToLps.TryGetValue(key, out var lpMap))
            {
                lpMap[lpId] = true;
            }
            else
            {
                _routeToLps[key] = new ConcurrentDictionary<string, bool>(
                    [new KeyValuePair<string, bool>(lpId, true)]);
            }
        }
    }

    public HashSet<RouteDto> GetAll()
    {
        var routes = new HashSet<RouteDto>();

        foreach (var key in _routeToLps.Keys)
        {
            if (cache.TryGetValue<RouteDto>(key, out var route))
            {
                routes.Add(route);
            }
        }

        return routes;
    }

    public HashSet<TokenNetworkDto> GetAllSources()
    {
        var sources = new HashSet<TokenNetworkDto>();
        foreach (var key in _routeToLps.Keys)
        {
            if (cache.TryGetValue<RouteDto>(key, out var route))
            {
                sources.Add(route.Source);
            }
        }
        return sources;
    }

    public HashSet<TokenNetworkDto> GetAllDestinations()
    {
        var destinations = new HashSet<TokenNetworkDto>();
        foreach (var key in _routeToLps.Keys)
        {
            if (cache.TryGetValue<RouteDto>(key, out var route))
            {
                destinations.Add(route.Destionation);
            }
        }

        return destinations;
    }

    public IEnumerable<string> GetLpsByRoute(string sourceNetwork, string sourceToken, string destinationNetwork, string destinationToken)
    {
        var requestedRouteKey = GetRouteKey(sourceNetwork, sourceToken, destinationNetwork, destinationToken);

        if (_routeToLps.TryGetValue(requestedRouteKey, out var lps))
        {
            return lps.Keys;
        }

        return Enumerable.Empty<string>();
    }

    private static string GetRouteKey(string sourceNetwork, string sourceToken, string destNetwork, string destToken)
    {
        return $"{sourceNetwork}.{sourceToken}->{destNetwork}.{destToken}";
    }

    private static string GetRouteKey(RouteDto r)
    {
        return GetRouteKey(
            r.Source.Network.Name,
            r.Source.Token.Symbol,
            r.Destionation.Network.Name,
            r.Destionation.Token.Symbol
        );
    }
}
