using Train.Station.API.Services;
using Train.Station.Client;

namespace Train.Station.API;

public class RoutePollingService(
    RouteCache routeCache,
    IHttpClientFactory httpClientFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            foreach (var lp in SolverRegistry.Snapshot())
            {
                try
                {
                    var name = lp.Key;
                    var baseAddress = lp.Value.ToString();

                    var trainSilverClient = new TrainSolverApiClient(
                        baseAddress, 
                        httpClientFactory.CreateClient(name));

                    var routesResponse = await trainSilverClient.RoutesAsync();

                    if (routesResponse == null || routesResponse.Error != null || !routesResponse.Data.Any())
                    {
                        continue;
                    }

                    routeCache.AddOrUpdateRoute(name, routesResponse.Data, TimeSpan.FromMinutes(10));
                }
                catch (Exception ex)
                {
                }
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}