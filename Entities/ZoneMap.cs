using System;
using System.Collections.Generic;
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

    /// <returns>index array of path links</returns>
    public int[] FindShortestPath(int source, int target)
    {
        var queue = new List<List<int>> { new () };
        queue.First().Add(source);
        
        var visited = new HashSet<int> { source };

        while (queue.Count > 0)
        {
            var path = queue.First();
            queue.RemoveAt(0);
            
            var node = path.Last();
            var neighbors = Nodes[node].Connections;

            foreach (var neighbor in neighbors)
            {
                if (neighbor == target)
                {
                    var fullPath = new List<int>(path) { neighbor };

                    var links = new List<int>();
                    for (var i = 0; i < fullPath.Count - 1; i++)
                    {
                        var from = fullPath[i];
                        var to  = fullPath[i + 1];
                        links.Add(Nodes[from].ConnectionLinkIndexes[Nodes[from].Connections.IndexOf(to)]);
                    }
                    
                    return links.ToArray();
                }

                if (!visited.Add(neighbor)) continue;
                
                var copy = new List<int>(path) { neighbor };
                queue.Add(copy);
            }
        }

        return [];
    }

    public void HighlightLinks(int[] indexes, ZoneLink.HighlightType type)
    {
        foreach (var index in indexes)
        {
            var link = Links[index];
            if (link is not ZoneLink zoneLink) continue;
            zoneLink.Highlight(type);
        }
    }
}