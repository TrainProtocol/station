using System.Collections.Concurrent;
using System.Text.Json;
using Train.Station.API.Models;

namespace Train.Station.API.Services;

public class SolverCache
{
    private readonly Dictionary<string, Solver> _solvers;

    public SolverCache(IWebHostEnvironment env)
    {
        var filePath = Path.Combine(env.ContentRootPath, "solvers.json");
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Could not find lp.json", filePath);
        }

        var json = File.ReadAllText(filePath);
        _solvers = JsonSerializer.Deserialize<List<Solver>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }).ToDictionary(x=> x.Name) ?? new ();
    }

    public IReadOnlyDictionary<string, Solver> GetAll() => _solvers;
}