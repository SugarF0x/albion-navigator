using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotResourceGroups;

namespace AlbionNavigator.Autoload.Services;

public class ZoneService
{
    private static ZoneService _instance;
    public static ZoneService Instance {
        set => ArgumentNullException.ThrowIfNull(value);
        get => _instance ??= new ZoneService();
    }

    public bool IsReady;

    private ResourceGroupBackgroundLoader _loader;
    public delegate void ResourceLoadedHandler(ResourceGroupBackgroundLoader.ResourceLoadingInfo loadingInfo);
    public event ResourceLoadedHandler ResourceLoaded;
    private void EmitResourceLoaded(ResourceGroupBackgroundLoader.ResourceLoadingInfo loadingInfo) => ResourceLoaded?.Invoke(loadingInfo);

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
        _loader = zoneGroup.LoadAllInBackground(EmitResourceLoaded);

        List<Resource> zoneList = [];
        ResourceLoaded += Handler;
        
        return;

        void Handler(ResourceGroupBackgroundLoader.ResourceLoadingInfo info)
        {
            zoneList.Add(info.Resource);
            if (!info.Last) return;
            
            Zones = zoneList.Cast<Zone>().ToArray();
            Array.Sort(Zones, (a, b) => a.Id - b.Id);
            
            IsReady = true;
            ResourceLoaded -= Handler;
            AllResourcesLoaded?.Invoke();
        }
    }
}