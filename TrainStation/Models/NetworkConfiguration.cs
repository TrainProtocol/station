namespace Train.Station.API.Models;

public class NetworkConfiguration
{
    public string Name { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public string Logo { get; set; } = null!;

    public string TransactionExplorerTemplate { get; set; } = null!;

    public string AccountExplorerTemplate { get; set; } = null!;

    public string NativeSymbol { get; set; } = null!;

    public int Decimals { get; set; }

    public string RpcUrl { get; set; } = null!;

    public string? HTLCNativeContractAddress { get; set; }

    public string? HTLCTokenContractAddress { get; set; }

    public string? GasPriceOracleContract { get; set; }

    public string? EvmMultiCallContract { get; set; }
}
