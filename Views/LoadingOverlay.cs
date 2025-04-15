using System;
using System.Threading.Tasks;
using AlbionNavigator.Autoload.Services;
using AlbionNavigator.Entities;
using Godot;
using GodotResourceGroups;

public partial class LoadingOverlay : CanvasLayer
{
    private ProgressBar _majorProgressBar;
    private ProgressBar _minorProgressBar;
    private Label _progressLabel;
    
    public override void _Ready()
    {
        OnReady();
        LoadServices();
    }

    private void OnReady()
    {
        _majorProgressBar = GetNode<ProgressBar>("%MajorProgressBar");
        _minorProgressBar = GetNode<ProgressBar>("%MinorProgressBar");
        _progressLabel = GetNode<Label>("%ProgressLabel");
    }
    
    private async void LoadServices()
    {
        Func<Task>[] serviceLoaders = [LoadZones, LoadLinks];

        for (int i = 0; i < serviceLoaders.Length; i++)
        {
            _majorProgressBar.Value = (double)i / serviceLoaders.Length;
            _minorProgressBar.Value = 0.0;
            await serviceLoaders[i]();
        }

        _majorProgressBar.Value = 0.99;

        CallDeferred("OnAllServicesLoaded");
    }

    private Task LoadZones()
    {
        var zoneService = ZoneService.Instance;
        if (zoneService.IsReady) return Task.CompletedTask;

        var tcs = new TaskCompletionSource();
        
        zoneService.ResourceLoaded += OnResourceLoaded;
        zoneService.AllResourcesLoaded += OnAllResourcesLoaded;

        return tcs.Task;
        
        void OnResourceLoaded(ResourceGroupBackgroundLoader.ResourceLoadingInfo info)
        {
            _progressLabel.Text = $"Loaded Zone: {((Zone)info.Resource).DisplayName}";
            _minorProgressBar.Value = info.Progress;
        }
        
        void OnAllResourcesLoaded()
        {
            zoneService.ResourceLoaded -= OnResourceLoaded;
            zoneService.AllResourcesLoaded -= OnAllResourcesLoaded;
            tcs.SetResult();
        }
    }

    private Task LoadLinks()
    {
        _ = LinkService.Instance;
        return Task.CompletedTask;
    }

    private void OnAllServicesLoaded()
    {
        if (ZoneMap.Instance == null) throw new ArgumentNullException(nameof(ZoneMap.Instance));
        
        _progressLabel.Text = "Populating map...";
        _minorProgressBar.Value = 0.0;
        
        ZoneMap.Instance.PopulateZones();
        ZoneMap.Instance.Reheat(0f);
        
        QueueFree();
    }
}
