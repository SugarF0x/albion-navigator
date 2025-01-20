using System;
using System.Linq;
using AlbionNavigator.Data;
using AlbionNavigator.Graph;
using Godot;

namespace AlbionNavigator.Entities;

[GlobalClass]
public partial class ZoneMap : ForceDirectedGraph
{
    [ExportGroup("Entities")] 
    [Export] public PackedScene NodeScene;
    [Export] public PackedScene LinkScene;

    private AudioPlayer AudioServer;
    
    public override void _Ready()
    {
        if (NodeScene?.Instantiate() is not ZoneNode) throw new InvalidCastException("NodeScene is not a ZoneNode");
        if (LinkScene?.Instantiate() is not ZoneLink) throw new InvalidCastException("LinkScene is not a ZoneLink");
        
        AudioServer = GetNode<AudioPlayer>("/root/AudioPlayer");
        
        PopulateZones();
        base._Ready();
    }

    public void AddPortal(int source, int target, string expiration)
    {
        if (LinkScene.Instantiate() is not ZoneLink link) return;
        if (ZoneLink.IsStampExpired(expiration)) return;
        
        link.Connect(source, target);
        link.ExpiresAt = expiration;
        AddLink(link);
        AudioServer.Play(AudioPlayer.SoundId.PortalOpen);
    }

    private void PopulateZones()
    {
        var zones = Zone.LoadZoneBinaries();

        for (var i = 0; i < zones.Length; i++)
        {
            var zone = zones[i];
            if (NodeScene.Instantiate() is not ZoneNode node) continue;
            
            node.Position = zone.Position;
            node.Index = zone.Id;
            node.Connections = zone.Connections.ToList();
            if (node.Position != Vector2.Zero) node.Frozen = true;

            node.Type = zone.Type;
            node.DisplayName = zone.DisplayName;
            
            AddNode(node);

            foreach (var connection in zone.Connections.Where(index => index > i))
            {
                if (LinkScene.Instantiate() is not ForceGraphLink link) continue;
                link.Connect(i, connection);
                AddLink(link);
            }
        }
    }
}