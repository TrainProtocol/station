using Microsoft.AspNetCore.Mvc;
using Train.Station.API.Services;
using Train.Station.Client;

namespace Train.Station.API.Endpoints;

public static class SolverEndpoints
{
    public const int UsdPrecision = 6;

    public static RouteGroupBuilder MapEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/networks", GetNetworksAsync)
            .Produces<ApiResponseListDetailedNetworkDto>();

        group.MapGet("/routes", GetAllRoutesAsync)
            .Produces<ApiResponseListDetailedNetworkDto>();

        //group.MapGet("/limits", GetSwapRouteLimitsAsync)
        //  .Produces<ApiResponseLimitDto>();

        //group.MapGet("/quote", GetQuoteAsync)
        //    .Produces<ApiResponseQuoteDto>();

        //group.MapGet("/{solver}/swaps", GetAllSwapsAsync)
        //    .Produces<ApiResponseSwapDto>();

        //group.MapGet("/{solver}/swaps/{commitId}", GetSwapAsync)
        //    .Produces<ApiResponseSwapDto>();

        //group.MapPost("/{solver}/swaps/{commitId}/addLockSig", AddLockSigAsync)
        //    .Produces<ApiResponse>();

        group.MapGet("/health", () => Results.Ok())
            .Produces(StatusCodes.Status200OK);

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

    //private static async Task<IResult> GetQuoteAsync(
    //    IRouteService routeService,
    //    HttpContext httpContext,
    //    [AsParameters] GetQuoteQueryParams queryParams)
    //{
    //    var quoteRequest = new QuoteRequest
    //    {
    //        SourceNetwork = queryParams.SourceNetwork!,
    //        SourceToken = queryParams.SourceToken!,
    //        DestinationNetwork = queryParams.DestinationNetwork!,
    //        DestinationToken = queryParams.DestinationToken!,
    //        Amount = queryParams.Amount!.Value,
    //    };

    //    var quote = await routeService.GetValidatedQuoteAsync(quoteRequest);

    //    if (quote == null)
    //    {
    //        return Results.NotFound(new ApiResponse()
    //        {
    //            Error = new ApiError()
    //            {
    //                Code = "QUOTE_NOT_FOUND",
    //                Message = "Quote not found",
    //            }
    //        });
    //    }

    //    return Results.Ok(new ApiResponseQuoteDto { Data = quote });
    //}
}
