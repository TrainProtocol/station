namespace Train.Station.API.Models;

public class NetworkConfiguration
{
    public string Name { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public string ChainId { get; set; } = null!;

    public string TransactionExplorerTemplate { get; set; } = null!;

    public string AccountExplorerTemplate { get; set; } = null!;

    public string NativeTokenSymbol { get; set; } = null!;

    public int NativeTokenDecimals { get; set; }

    public string RpcUrl { get; set; } = null!;

    public string Type { get; set; } = null!;
}
