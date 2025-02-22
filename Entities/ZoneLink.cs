using System;
using System.Linq;
using AlbionNavigator.Data;
using AlbionNavigator.Graph;
using Godot;

namespace AlbionNavigator.Entities;

[GlobalClass]
public partial class ZoneLink : ForceGraphLink
{
    [Export] public Line2D Outline;
    [Export] public float OutlineWidth = 1f;
    
    public string ExpiresAt { get; set; }
    
    private AudioPlayer AudioServer;
    private HighlightType DefaultHighlightType = HighlightType.Default;

    public override void _Ready()
    {
        AudioServer = GetNode<AudioPlayer>("/root/AudioPlayer");
    }

    public override bool DrawLink(ForceGraphNode[] nodes)
    {
        Outline.ClearPoints();
        if (!base.DrawLink(nodes)) return false;
        
        Outline.AddPoint(nodes[Target].Position);
        Outline.AddPoint(nodes[Source].Position);
        
        return true;
    }

    public override void Initialize(int graphIndex, ForceGraphNode[] nodes)
    {
        base.Initialize(graphIndex, nodes);
        InitExpiry();
        InitHighlight(nodes);
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

    private void InitHighlight(ForceGraphNode[] nodes)
    {
        var sourceNode = nodes[Source];
        var targetNode = nodes[Target];

        if (sourceNode is not ZoneNode sourceZoneNode || targetNode is not ZoneNode targetZoneNode) return;
        Zone.ZoneType[] zoneTypes = [sourceZoneNode.Type, targetZoneNode.Type];
        
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
        SetWidth(0f);
        switch (type)
        {
            case HighlightType.Default: Line.DefaultColor = Colors.White; return;
            case HighlightType.CityPortal: Line.DefaultColor = Colors.Orange; return;
            case HighlightType.RoadToContinent: Line.DefaultColor = Colors.Cyan with { A = .5f }; return;
            case HighlightType.Path: 
                Line.DefaultColor = Colors.Purple;
                SetWidth(5f);
                return;
            case HighlightType.WayOut: 
                Line.DefaultColor = Colors.Red;
                SetWidth(5f);
                return;
            default: GD.Print("Unknown highlight type"); return;
        }
    }

    public void Highlight()
    {
        Highlight(DefaultHighlightType);
    }

    public void SetWidth(float width)
    {
        Line.Width = width;
        Outline.Width = width + OutlineWidth;
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