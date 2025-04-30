using Train.Station.Client;

namespace Train.Station.API.Services;

public class RoutePollingService(
    RouteCache routeCache,
    SolverCache solverCache,
    IHttpClientFactory httpClientFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            foreach (var lp in solverCache.GetAll())
            {
                try
                {
                    var name = lp.Key;
                    var baseAddress = lp.Value.Url.ToString();

                    var trainSilverClient = new TrainSolverApiClient(
                        baseAddress, 
                        httpClientFactory.CreateClient(name));

                    var routesResponse = await trainSilverClient.RoutesAsync();

                    if (routesResponse == null || routesResponse.Error != null || !routesResponse.Data.Any())
                    {
                        continue;
                    }

                    await routeCache.AddOrUpdateRouteAsync(name, routesResponse.Data, TimeSpan.FromMinutes(5));
                }
                catch (Exception ex)
                {
                }
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}