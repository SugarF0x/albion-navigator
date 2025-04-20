using System;
using System.Collections;
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
    private float _lastProcessTime;
    
    public override void _Ready()
    {
        OnReady();
        LoadServices();
    }

    public override void _Process(double delta)
    {
        if (_lastProcessTime == 0f) _lastProcessTime = (float)delta;
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
        Func<Task>[] serviceLoaders = [LoadZones, PopulateMap];

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

    private Task PopulateMap()
    {
        var lastLoadedZoneInfo = new LastLoadedZoneInfo();
        var queue = new Queue(ZoneService.Instance.Zones);
        
        var tcs = new TaskCompletionSource();

        _updateLabels = () =>
        {
            _progressLabel.Text = $"Added Zone: {lastLoadedZoneInfo.Name}";
            _minorProgressBar.Value = lastLoadedZoneInfo.Progress;
        };
        
        ProcessQueue();
        
        return tcs.Task;

        async void ProcessQueue()
        {
            try
            {
                var initialReheatValue = ZoneMap.Instance.ReheatOnNodesAdded;
                ZoneMap.Instance.ReheatOnNodesAdded = false;
                
                while (queue.Count > 0)
                {
                    var frameStart = Time.GetTicksUsec();

                    while (queue.Count > 0)
                    {
                        var zone = (Zone)queue.Dequeue();
                        if (zone == null) continue;
                        
                        ZoneMap.Instance.InstantiateNode(zone);
                        lastLoadedZoneInfo.Name = zone.DisplayName;
                        lastLoadedZoneInfo.Progress = (float)(ZoneService.Instance.Zones.Length - queue.Count) / ZoneService.Instance.Zones.Length;
                        
                        var elapsed = Time.GetTicksUsec() - frameStart;
                        if (!(elapsed / 1000.0 >= _lastProcessTime)) continue;
                        
                        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                        break;
                    }
                }
                
                ZoneMap.Instance.ReheatOnNodesAdded = initialReheatValue;
                OnAllZonesInserted();
            }
            catch
            {
                // ignored
            }
        }
        
        void OnAllZonesInserted()
        {
            _updateLabels = null;
            _minorProgressBar.Value = 1f;
            tcs.SetResult();
        }
    }

    private void OnAllServicesLoaded()
    {
        if (ZoneMap.Instance == null) throw new ArgumentNullException(nameof(ZoneMap.Instance));
        
        _progressLabel.Text = "Starting simulation...";
        _majorProgressBar.Value = 1.0;
        _minorProgressBar.Value = 1.0;

        ToSignal(GetTree(), "process_frame").OnCompleted(() => CallDeferred("StartSimulation"));
    }

    private void StartSimulation()
    {
        ZoneMap.Instance.Reheat(0f);
        QueueFree();
    }

    private class LastLoadedZoneInfo()
    {
        public string Name;
        public float Progress;
    }
}