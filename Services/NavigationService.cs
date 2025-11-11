using System;
using System.Collections.Generic;
using System.Linq;
using AlbionNavigator.Resources;

namespace AlbionNavigator.Services;

public class NavigationService
{
    private static NavigationService _instance;
    public static NavigationService Instance => _instance ??= new NavigationService();
    
    public event Action<int[]> ShortestPathUpdated;
    private int[] _lastShortestPath;
    public int[] LastShortestPath
    {
        get =>  _lastShortestPath;
        set
        {
            _lastShortestPath = value;
            ShortestPathUpdated?.Invoke(value);
        }
    }
    
    /// <returns>
    /// index array of path zones
    /// </returns>
    public int[] FindShortestPath(int source, int target)
    {
        var zones = ZoneService.Instance.Zones;
        var queue = new List<List<int>> { new () };
        queue.First().Add(source);
        
        var visited = new HashSet<int> { source };
        
        while (queue.Count > 0)
        {
            var path = queue.First();
            queue.RemoveAt(0);
            
            var node = path.Last();
            var neighbors = zones[node].Connections;

            foreach (var neighbor in neighbors)
            {
                var extendedPath = new List<int>(path) { neighbor };
                if (neighbor == target)
                {
                    LastShortestPath = extendedPath.ToArray();
                    return LastShortestPath;
                }

                if (!visited.Add(neighbor)) continue;
                queue.Add(extendedPath);
            }
        }

        LastShortestPath = [];
        return LastShortestPath;
    }
    
    public event Action<int[][]> AllPathsOutUpdated;
    private int[][] _lastAllPathsOut = [];
    public int[][] LastAllPathsOut
    {
        get =>  _lastAllPathsOut;
        set
        {
            _lastAllPathsOut = value;
            AllPathsOutUpdated?.Invoke(value);
        }
    }
    
    /// <returns>
    /// Returns sorted array of shortest zone index arrays representing paths out from source.
    /// Searches for any land connections by default. Royal exit search looks for non-road and non-black zone connections.
    /// </returns>
    public int[][] FindAllPathsOut(int source, bool searchForRoyalExit = false)
    {
        var results = new List<int[]>();
        var invalidExits = searchForRoyalExit 
            ? new[] { Zone.ZoneType.Black, Zone.ZoneType.OutlandCity, Zone.ZoneType.Road } 
            : new[] { Zone.ZoneType.Road };

        if (!invalidExits.Contains(ZoneService.Instance.Zones[source].Type))
        {
            LastAllPathsOut = [];
            return LastAllPathsOut;
        }

        var zones = ZoneService.Instance.Zones;
        var queue = new List<List<int>> { new () };
        queue.First().Add(source);
        
        var visited = new HashSet<int> { source };

        while (queue.Count > 0)
        {
            var path = queue.First();
            queue.RemoveAt(0);
            
            var node = path.Last();
            var neighbors = zones[node].Connections;

            foreach (var neighbor in neighbors)
            {
                var newPath = new List<int>(path) { neighbor };
                var neighborZone = ZoneService.Instance.Zones[neighbor];

                if (invalidExits.Contains(neighborZone.Type))
                {
                    if (!visited.Add(neighbor)) continue;
                    
                    var copy = new List<int>(path) { neighbor };
                    queue.Add(copy);
                    continue;
                }
                
                results.Add(newPath.ToArray());
            }
        }

        LastAllPathsOut = results.ToArray();
        return LastAllPathsOut;
    }
    
    public static string GetPathExpiration(int[] path)
    {
        string earliestExpiration = null;
        for (var i = 1; i < path.Length; i++)
        {
            var target = ZoneService.Instance.Zones[path[i]];
            var source = ZoneService.Instance.Zones[path[i - 1]];

            if (target.Type != Zone.ZoneType.Road && source.Type != Zone.ZoneType.Road) continue;
            var link = LinkService.Instance.Links.Find(link =>
                (link.Source == source.Id && link.Target == target.Id)
                || (link.Source == target.Id && link.Target == source.Id)
            );
			
            if (link is { Source: 0, Target: 0 }) continue;
            earliestExpiration ??= link.Expiration;
            if (link.IsLaterThanExpiration(earliestExpiration)) earliestExpiration = link.Expiration;
        }

        return earliestExpiration;
    }
    
    public event Action<int[]> LastInspectedPathUpdated;
    private int[] _lastInspectedPath;
    public int[] LastInspectedPath
    {
        get =>  _lastInspectedPath;
        set
        {
            _lastInspectedPath = value;
            LastInspectedPathUpdated?.Invoke(value);
        }
    }
}