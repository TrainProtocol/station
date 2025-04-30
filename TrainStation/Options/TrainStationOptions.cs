namespace Train.Station.API.Options;

public class TrainStationOptions
{
    public const string SectionName = "TrainSolver";

    public string RedisConnectionString { get; set; } = null!;

    public int RedisDatabaseIndex { get; set; } = 4;
}
