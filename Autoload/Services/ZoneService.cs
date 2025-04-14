using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using GodotResourceGroups;

namespace AlbionNavigator.Autoload.Services;

public class ZoneService
{
    // TODO: go back from Tasked instnace to static one and rely on IsReady flag
    
    private static Task<ZoneService> _instance;
    public static Task<ZoneService> Instance {
        set => ArgumentNullException.ThrowIfNull(value);
        get => _instance ??= Init();
    }

    private ResourceGroupBackgroundLoader _loader;
    public delegate void ResourceLoadedHandler(ResourceGroupBackgroundLoader.ResourceLoadingInfo loadingInfo);
    public event ResourceLoadedHandler ResourceLoaded;
    private void EmitResourceLoaded(ResourceGroupBackgroundLoader.ResourceLoadingInfo loadingInfo) => ResourceLoaded?.Invoke(loadingInfo);

    public delegate void AllResourcesLoadedHandler();
    public event AllResourcesLoadedHandler AllResourcesLoaded;
    
    public Zone[] Zones;

    private static async Task<ZoneService> Init() 
    {
        var instance = new ZoneService();
        await instance.LoadZones();
        return instance;
    }

    private Task LoadZones()
    {
        var zoneGroup = ResourceGroup.Of("res://Resources/ZoneGroup.tres");
        _loader = zoneGroup.LoadAllInBackground(EmitResourceLoaded);

        var tcs = new TaskCompletionSource();
        List<Resource> zoneList = [];

        ResourceLoaded += Handler;
        return tcs.Task;

        void Handler(ResourceGroupBackgroundLoader.ResourceLoadingInfo info)
        {
            zoneList.Add(info.Resource);
            if (!info.Last) return;

            Zones = zoneList.Cast<Zone>().ToArray();
            Array.Sort(Zones, (a, b) => a.Id - b.Id);
            
            ResourceLoaded -= Handler;
            AllResourcesLoaded?.Invoke();
        }
    }
}