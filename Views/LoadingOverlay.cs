using System;
using System.Linq;
using System.Threading.Tasks;
using AlbionNavigator.Entities;
using AlbionNavigator.Services;
using Godot;

namespace AlbionNavigator.Views;

public partial class LoadingOverlay : CanvasLayer
{
    private ProgressBar _majorProgressBar;
    private ProgressBar _minorProgressBar;
    private Label _progressLabel;

    private Action _updateLabels;
    
    public override void _Ready()
    {
        OnReady();
        LoadServices();
    }

    public override void _Process(double delta)
    {
        _updateLabels?.Invoke();
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
        
        zoneService.AllResourcesLoaded += OnAllResourcesLoaded;
        _updateLabels = () =>
        {
            var loader = ZoneService.Instance.Loader;
            var resource = (Zone)loader.Resources.Last();
            if (resource == null) return;
            
            _progressLabel.Text = $"Loaded Zone: {resource.DisplayName}";
            _minorProgressBar.Value = loader.Progress;
        };

        return tcs.Task;
        
        void OnAllResourcesLoaded()
        {
            zoneService.AllResourcesLoaded -= OnAllResourcesLoaded;
            _updateLabels = null;
            _minorProgressBar.Value = 1f;
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
        
        _progressLabel.Text = "Populating map... (app might freeze for a second or two)";
        _minorProgressBar.Value = 0.0;

        ToSignal(GetTree(), "process_frame").OnCompleted(() => CallDeferred("StartSimulation"));
    }

    private void StartSimulation()
    {
        ZoneMap.Instance.PopulateZones();
        ZoneMap.Instance.Reheat(0f);
        
        QueueFree();
    }
}