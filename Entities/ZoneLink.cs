using System;
using System.Linq;
using AlbionNavigator.Data;
using AlbionNavigator.Graph;
using Godot;

namespace AlbionNavigator.Entities;

[GlobalClass]
public partial class ZoneLink : ForceGraphLink
{
    public string ExpiresAt { get; set; }
    
    private AudioPlayer AudioServer;

    public override void _Ready()
    {
        AudioServer = GetNode<AudioPlayer>("/root/AudioPlayer");
    }

    public override void Initialize(ForceGraphNode[] nodes)
    {
        base.Initialize(nodes);
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
        Highlight();
        
        var sourceNode = nodes[Source];
        var targetNode = nodes[Target];

        if (sourceNode is not ZoneNode sourceZoneNode || targetNode is not ZoneNode targetZoneNode) return;
        Zone.ZoneType[] zoneTypes = [sourceZoneNode.Type, targetZoneNode.Type];
        
        if (zoneTypes.All(type => type == Zone.ZoneType.City)) Highlight(HighlightType.CityPortal);
        if (zoneTypes.Any(type => type == Zone.ZoneType.Road) && zoneTypes.Any(type => type != Zone.ZoneType.Road)) Highlight(HighlightType.RoadToContinent);
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

    public void Highlight(HighlightType type = HighlightType.Default)
    {
        switch (type)
        {
            case HighlightType.Default: Line.DefaultColor = Colors.White; return;
            case HighlightType.CityPortal: Line.DefaultColor = Colors.Orange; return;
            case HighlightType.RoadToContinent: Line.DefaultColor = Colors.Cyan with { A = .5f }; return;
            case HighlightType.Path: Line.DefaultColor = Colors.Purple; return;
            case HighlightType.WayOut: Line.DefaultColor = Colors.Red; return;
        }
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