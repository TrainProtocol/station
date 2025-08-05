using Newtonsoft.Json;

namespace Train.Station.Client;

public partial class QuoteWithSolverDto
{
    [JsonProperty("solverName", NullValueHandling = NullValueHandling.Ignore)]
    public string SolverName { get; set; }
}