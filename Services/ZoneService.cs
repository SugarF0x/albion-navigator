using System.Linq;
using AlbionNavigator.Resources;
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
}