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
    }
    
    // TODO: make links connecting roads to mainland semitransparent 
    
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

    private void ClosePortal()
    {
        AudioServer.Play(AudioPlayer.SoundId.PortalClose);
        QueueFree();
    }

    public override void DrawLink(ForceGraphNode[] nodes)
    {
        base.DrawLink(nodes);
        
        var sourceNode = nodes[Source];
        var targetNode = nodes[Target];

        if (sourceNode is not ZoneNode sourceZoneNode || targetNode is not ZoneNode targetZoneNode) return;
        if (sourceZoneNode.Type == Zone.ZoneType.City && targetZoneNode.Type == Zone.ZoneType.City) Line.DefaultColor = Colors.Orange;
        else Line.DefaultColor = Colors.White;
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