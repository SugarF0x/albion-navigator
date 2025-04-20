using System;
using System.Linq;
using AlbionNavigator.Graph;
using AlbionNavigator.Services;
using Godot;

namespace AlbionNavigator.Entities;

[GlobalClass]
public partial class ZoneLink : ForceGraphLink
{
    [Export] public Line2D Outline;
    [Export] public float OutlineWidth = 5f;
    
    public string ExpiresAt { get; set; }
    
    private AudioPlayer AudioServer;
    private HighlightType DefaultHighlightType = HighlightType.Default;

    public override void _Ready()
    {
        AudioServer = GetNode<AudioPlayer>("/root/AudioPlayer");
        InitHighlight();
        DrawLink();
    }

    public override bool DrawLink()
    {
        Outline.ClearPoints();
        if (!base.DrawLink()) return false;
        
        Outline.AddPoint(ZoneService.Instance.Zones[Target].Position);
        Outline.AddPoint(ZoneService.Instance.Zones[Source].Position);
        
        return true;
    }

    public override void Initialize(int graphIndex, ForceGraphNode[] nodes)
    {
        base.Initialize(graphIndex, nodes);
        InitExpiry();
    }
    
    protected override void InitStrength(ForceGraphNode[] nodes)
    {
        var sourceNode = nodes[Source];
        var targetNode = nodes[Target];
    
        if (sourceNode is ZoneNode sourceZoneNode && targetNode is ZoneNode targetZoneNode)
        {
            Zone.ZoneType[] zoneTypes = [sourceZoneNode.Type, targetZoneNode.Type];
            if (zoneTypes.All(type => type == Zone.ZoneType.Road)) Strength = .5f;
            else if (zoneTypes.Any(type => type == Zone.ZoneType.Road) && zoneTypes.Any(type => type != Zone.ZoneType.Road)) Strength = 0f;
            else Strength = 1f;
            return;
        }
        
        base.InitStrength(nodes);
    }

    private void InitExpiry()
    {
        if (ExpiresAt == null) return;
        
        if (IsStampExpired(ExpiresAt))
        {
            QueueFree();
            return;
        }
        
        GetTree().CreateTimer(GetExpirationInSeconds(ExpiresAt)).Timeout += ClosePortal;
    }

    private void InitHighlight()
    {
        var sourceNode = ZoneService.Instance.Zones[Source];
        var targetNode = ZoneService.Instance.Zones[Target];

        Zone.ZoneType[] zoneTypes = [sourceNode.Type, targetNode.Type];
        
        if (zoneTypes.All(type => type == Zone.ZoneType.City)) DefaultHighlightType = HighlightType.CityPortal;
        if (zoneTypes.Any(type => type == Zone.ZoneType.Road) && zoneTypes.Any(type => type != Zone.ZoneType.Road)) DefaultHighlightType = HighlightType.RoadToContinent;
        
        Highlight();
    }

    private void ClosePortal()
    {
        AudioServer.Play(AudioPlayer.SoundId.PortalClose);
        QueueFree();
    }

    public enum HighlightType
    {
        Default,
        CityPortal,
        RoadToContinent,
        Path,
        WayOut,
    }

    public void Highlight(HighlightType type)
    {
        SetWidth(5f);
        switch (type)
        {
            case HighlightType.Default: Line.DefaultColor = Colors.White; return;
            case HighlightType.CityPortal: Line.DefaultColor = Colors.Orange; return;
            case HighlightType.RoadToContinent: Line.DefaultColor = Colors.Cyan with { A = .5f }; return;
            case HighlightType.Path: 
                Line.DefaultColor = Colors.Purple;
                SetWidth(10f, true);
                return;
            case HighlightType.WayOut: 
                Line.DefaultColor = Colors.Red;
                SetWidth(10f, true);
                return;
            default: GD.Print("Unknown highlight type"); return;
        }
    }

    public void Highlight()
    {
        Highlight(DefaultHighlightType);
    }

    public void SetWidth(float width, bool withOutline = false)
    {
        Line.Width = width;
        Outline.Width = width + OutlineWidth;
        Outline.Visible = withOutline;
    }

    public static float GetExpirationInSeconds(string timestamp)
    {
        var targetDateTime = DateTime.Parse(timestamp);
        var currentDateTime = DateTime.UtcNow;
        var difference = targetDateTime - currentDateTime;
        return (float)difference.TotalSeconds;
    }

    public static bool IsStampExpired(string timestamp)
    {
        return GetExpirationInSeconds(timestamp) <= 0;
    }
}