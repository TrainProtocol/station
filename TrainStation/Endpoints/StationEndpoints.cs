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

        group.MapGet("/quote", GetQuoteAsync)
            .Produces<ApiResponseQuoteWithSolverDto>();

        group.MapGet("/{solver}/swaps/{commitId}", GetSwapAsync)
            .Produces<ApiResponseSwapDto>();

        group.MapPost("/{solver}/swaps/{commitId}/addLockSig", AddLockSigAsync)
            .Produces<ApiResponse>();

        return group;
    }

    private static async Task<IResult> GetSwapAsync(
        HttpContext httpContext,
        SolverCache solverCache,
        [FromRoute] string solver,
        [FromRoute] string commitId,
        IHttpClientFactory httpClientFactory)
    {
        if (!solverCache.GetAll().ContainsKey(solver))
        {
            return Results.NotFound();
        }

        var httpClient = httpClientFactory.CreateClient(solver);
        var trainSilverClient = new TrainSolverApiClient(
            solver, httpClient);
        var swap = await trainSilverClient.SwapsAsync(commitId);

        return Results.Ok(swap);
    }

    private static async Task<IResult> AddLockSigAsync(
        HttpContext httpContext,
        SolverCache solverCache,
        [FromRoute] string solver,
        [FromRoute] string commitId,
        IHttpClientFactory httpClientFactory,
        AddLockSignatureModel addLock)
    {
        if (!solverCache.GetAll().ContainsKey(solver))
        {
            return Results.NotFound();
        }

        var httpClient = httpClientFactory.CreateClient(solver);
        var trainSilverClient = new TrainSolverApiClient(
            solver, httpClient);
        var swap = await trainSilverClient.SwapsAsync(commitId);

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

    private static async Task<IResult> GetQuoteAsync(
        RouteCache routeCache,
        SolverCache solverCache,
        IHttpClientFactory httpClientFactory,
        [FromQuery] string sourceNetwork,
        [FromQuery] string sourceToken,
        [FromQuery] string destinationNetwork,
        [FromQuery] string destinationToken,
        [FromQuery] string amount)
    {
        var solvers = await routeCache.GetSolversByRouteAsync(
            sourceNetwork,
            sourceToken,
            destinationNetwork,
            destinationToken);

        var solverName = solvers.First();

        var solverInfo = solverCache.GetAll()[solverName];
        var httpClient = httpClientFactory.CreateClient(solverName);

        var trainSilverClient = new TrainSolverApiClient(
            solverInfo.Url.ToString(), httpClient);

        var quote = await trainSilverClient.QuoteAsync(
            amount,
            sourceNetwork,
            sourceToken,
            destinationNetwork,
            destinationToken);

        quote.Data.SolverName = solverName;

        return Results.Ok(quote);
    }
}
