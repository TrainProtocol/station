using Microsoft.AspNetCore.Mvc;
using System.Text;
using Train.Station.API.Models;
using Train.Station.API.Services;
using Train.Station.Client;

namespace Train.Station.API.Endpoints;

public static class StationEndpoints
{
    public const int UsdPrecision = 6;

    public static RouteGroupBuilder MapEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/networks", GetNetworksAsync)
            .Produces<IEnumerable<NetworkConfiguration>>();

        group.MapGet("/routes", GetAllRoutesAsync)
            .Produces<IEnumerable<RouteDto>>();

        group.MapGet("/quote-sse", GetQuoteAsync)
            .Produces(statusCode: 200, contentType: "text/event-stream");

        group.MapGet("/{solver}/swaps/{commitId}", GetSwapAsync)
            .Produces<ApiResponseSwapDto>();

        group.MapPost("/{solver}/swaps/{commitId}/addLockSig", AddLockSigAsync)
            .Produces<ApiResponse>();

        return group;
    }

    private static async Task<IResult> GetSwapAsync(
        HttpContext httpContext,
        [FromRoute] string solver,
        [FromRoute] string commitId,
        IHttpClientFactory httpClientFactory)
    {
        var httpClient = httpClientFactory.CreateClient(solver);
        var trainSilverClient = new TrainSolverApiClient(
            solver, httpClient);
        var swap = await trainSilverClient.Swaps2Async(commitId);

        return Results.Ok(swap);
    }

    private static async Task<IResult> AddLockSigAsync(
        HttpContext httpContext,
        [FromRoute] string solver,
        [FromRoute] string commitId,
        IHttpClientFactory httpClientFactory,
        AddLockSignatureModel addLock)
    {
        var httpClient = httpClientFactory.CreateClient(solver);
        var trainSilverClient = new TrainSolverApiClient(
            solver, httpClient);
        var swap = await trainSilverClient.Swaps2Async(commitId);

        if (swap == null)
        {
            return Results.NotFound();
        }

        var lockSig = await trainSilverClient.AddLockSigAsync(commitId, addLock);
        return Results.Ok(lockSig);
    }

    private static async Task<IResult> GetNetworksAsync(
        NetworkConfigurationCache networkConfigurationCache)
    {
        return Results.Ok(networkConfigurationCache.GetAll());
    }

    private static async Task<IResult> GetAllRoutesAsync(
        RouteCache routeCache)
    {
        var routes = await routeCache.GetAllAsync();
        return Results.Ok(routes);
    }

    private static async Task GetQuoteAsync(
        HttpContext httpContext,
        RouteCache routeCache,
        SolverCache solverCache,
        IHttpClientFactory httpClientFactory,
        [FromQuery] string sourceNetwork,
        [FromQuery] string sourceToken,
        [FromQuery] string destinationNetwork,
        [FromQuery] string destinationToken,
        [FromQuery] double amount)
    {
        httpContext.Response.Headers.ContentType = "text/event-stream";

        var lps = await routeCache.GetLpsByRouteAsync(
            sourceNetwork,
            sourceToken,
            destinationNetwork,
            destinationToken);

        var cancellationToken = httpContext.RequestAborted;

            foreach (var lpName in lps)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    var solverInfo = solverCache.GetAll()[lpName];
                    var httpClient = httpClientFactory.CreateClient(lpName);

                    var trainSilverClient = new TrainSolverApiClient(
                        solverInfo.Url.ToString(), httpClient);

                    var quote = await trainSilverClient.QuoteAsync(
                        amount,
                        sourceNetwork,
                        sourceToken,
                        destinationNetwork,
                        destinationToken);

                    var message = new
                    {
                        Provider = lpName,
                        Quote = quote.Data
                    };

                    var json = System.Text.Json.JsonSerializer.Serialize(message);

                    await httpContext.Response.WriteAsync($"data: {json}\n\n");
                    await httpContext.Response.Body.FlushAsync();

                }
                catch
                {
                }
            }

         

        await httpContext.Response.Body.FlushAsync();
    }
}
