using System.Linq;
using AlbionNavigator.Resources;
using FuzzySharp;
using GodotResourceGroups;

namespace AlbionNavigator.Services;

public class ZoneService
{
    private static ZoneService _instance;
    public static ZoneService Instance => _instance ??= new ZoneService();

    public UnboundResourceLoader Loader;
    public bool IsReady;

    public delegate void AllResourcesLoadedHandler();
    public event AllResourcesLoadedHandler AllResourcesLoaded;
    
    public Zone[] Zones;

    public void LoadAllZones()
    {
        var zoneGroup = ResourceGroup.Of("res://Resources/ZoneGroup.tres");
        Loader = zoneGroup.LoadResourcesAsync(() =>
        {
            Zones = Loader.Resources.Cast<Zone>().OrderBy(a => a.Id).ToArray();
            IsReady = true;
            AllResourcesLoaded?.Invoke();
        });
    }

    public int GetProbableZoneIndexFromDisplayName(string name) => Process.ExtractOne(name, Zones.Select(zone => zone.DisplayName)).Index;

    public void ExtendConnection(int from, int to)
    {
        var source = Zones[from];
        var target = Zones[to];
        if (source.Connections.Contains(to) || target.Connections.Contains(from)) return;
        
        source.Connections.Add(to);
        target.Connections.Add(from);
    }

    public void PopConnection(int from, int to)
    {
        var source = Zones[from];
        var target = Zones[to];

        source.Connections = new Godot.Collections.Array<int>(source.Connections.Where(index => index != to));
        target.Connections = new Godot.Collections.Array<int>(source.Connections.Where(index => index != from));
    }
}