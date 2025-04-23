namespace Train.Station.API;

public static class SolverRegistry
{
    private static readonly Dictionary<string, Uri> _lpEndpoints = new();
    private static readonly object _lock = new();

    public static void Register(string lpName, Uri baseAddress)
    {
        lock (_lock)
        {
            _lpEndpoints[lpName] = baseAddress;
        }
    }

    public static bool TryGetAddress(string lpName, out Uri? baseAddress)
    {
        lock (_lock)
        {
            return _lpEndpoints.TryGetValue(lpName, out baseAddress);
        }
    }

    public static List<string> GetAllRegisteredLps()
    {
        lock (_lock)
        {
            return _lpEndpoints.Keys.ToList();
        }
    }

    public static void Remove(string lpName)
    {
        lock (_lock)
        {
            _lpEndpoints.Remove(lpName);
        }
    }

    public static Dictionary<string, Uri> Snapshot()
    {
        lock (_lock)
        {
            return new Dictionary<string, Uri>(_lpEndpoints);
        }
    }
}
