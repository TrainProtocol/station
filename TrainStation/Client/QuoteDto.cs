using Newtonsoft.Json;

namespace Train.Station.Client;

public partial class QuoteDto
{
    [JsonProperty("lpName", NullValueHandling = NullValueHandling.Ignore)]
    public string LPName { get; set; }
}