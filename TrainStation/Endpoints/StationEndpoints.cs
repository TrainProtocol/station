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

        //group.MapGet("/{solver}/swaps", GetAllSwapsAsync)
        //    .Produces<ApiResponseSwapDto>();

        //group.MapGet("/{solver}/swaps/{commitId}", GetSwapAsync)
        //    .Produces<ApiResponseSwapDto>();

        //group.MapPost("/{solver}/swaps/{commitId}/addLockSig", AddLockSigAsync)
        //    .Produces<ApiResponse>();

        return group;
    }


    //private static async Task<IResult> GetSwapRouteLimitsAsync(
    //    HttpContext httpContext,
    //    IRouteService routeService,
    //    [AsParameters] GetRouteLimitsQueryParams queryParams)
    //{
    //    var limit = await routeService.GetLimitAsync(
    //        new()
    //        {
    //            SourceNetwork = queryParams.SourceNetwork!,
    //            SourceToken = queryParams.SourceToken!,
    //            DestinationNetwork = queryParams.DestinationNetwork!,
    //            DestinationToken = queryParams.DestinationToken!,
    //        });

    //    if (limit == null)
    //    {
    //        return Results.NotFound(new ApiResponse()
    //        {
    //            Error = new ApiError()
    //            {
    //                Code = "LIMIT_NOT_FOUND",
    //                Message = "Limit not found",
    //            }
    //        });
    //    }

    //    return Results.Ok(new ApiResponse<LimitDto> { Data = limit });
    //}

    private static async Task<IResult> GetNetworksAsync(
        HttpContext httpContext,
        NetworkConfigurationCache networkConfigurationCache)
    {
        var networks = networkConfigurationCache.GetAll();

        return Results.Ok(networks);
    }

    //private static async Task<IResult> GetAllSourcesAsync(
    //    IRouteService routeService,
    //    INetworkRepository networkRepository,
    //    [FromQuery] string? destinationNetwork,
    //    [FromQuery] string? destinationToken)
    //{
    //    var sources = await routeService.GetSourcesAsync(
    //        networkName: destinationNetwork,
    //        token: destinationToken);

    //    if (sources == null || !sources.Any())
    //    {
    //        return Results.NotFound(new ApiResponse()
    //        {
    //            Error = new ApiError()
    //            {
    //                Code = "REACHABLE_POINTS_NOT_FOUND",
    //                Message = "No reachable points found",
    //            }
    //        });
    //    }

    //    return Results.Ok(new ApiResponseListDetailedNetworkDto { Data = sources });
    //}

    private static async Task<IResult> GetAllRoutesAsync(
        RouteCache routeCache)
    {
        var routes = routeCache.GetAll();
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

        var lps = routeCache.GetLpsByRoute(
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
