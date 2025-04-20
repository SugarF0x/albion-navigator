using System;
using System.Linq;
using GodotResourceGroups;

namespace AlbionNavigator.Services;

public class ZoneService
{
    private static ZoneService _instance;
    public static ZoneService Instance {
        set => ArgumentNullException.ThrowIfNull(value);
        get => _instance ??= new ZoneService();
    }

    public UnboundResourceLoader Loader;
    public bool IsReady;

    public delegate void AllResourcesLoadedHandler();
    public event AllResourcesLoadedHandler AllResourcesLoaded;
    
    public Zone[] Zones;

    private ZoneService()
    {
        LoadZones();
    }

    private void LoadZones()
    {
        var zoneGroup = ResourceGroup.Of("res://Resources/ZoneGroup.tres");
        Loader = zoneGroup.LoadAllInBackgroundUnbound(OnAllLoaded);
        return;
        
        void OnAllLoaded()
        {
            Zones = Loader.Resources.Cast<Zone>().ToArray();
            Array.Sort(Zones, (a, b) => a.Id - b.Id);
            IsReady = true;
            AllResourcesLoaded?.Invoke();
        }
    }
}