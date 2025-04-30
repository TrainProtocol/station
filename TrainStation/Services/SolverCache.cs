using System.Collections.Concurrent;
using System.Text.Json;
using Train.Station.API.Models;

namespace Train.Station.API.Services;

public class SolverCache(IWebHostEnvironment env) 
    : JsonFileCache<Dictionary<string, Solver>, Solver>(
        env, "solvers.json", list => list.ToDictionary(x => x.Name))
{
    public IReadOnlyDictionary<string, Solver> GetAll() => Items;
}