using System;
using System.Collections.Generic;
using System.Linq;
using AlbionNavigator.Graph;
using Godot;
using GodotResourceGroups;

namespace AlbionNavigator.Entities;

[GlobalClass]
public partial class ZoneMap : ForceDirectedGraph
{
    [ExportGroup("Entities")] 
    [Export] public PackedScene NodeScene;
    [Export] public PackedScene LinkScene;
    
    private ResourceGroup ZoneGroup;
    private Zone[] Zones = [];
    private AudioPlayer AudioServer;
    
    [Signal]
    public delegate void PortalRegisteredEventHandler(int from, int to);
    
    public override void _Ready()
    {
        if (NodeScene?.Instantiate() is not ZoneNode) throw new InvalidCastException("NodeScene is not a ZoneNode");
        if (LinkScene?.Instantiate() is not ZoneLink) throw new InvalidCastException("LinkScene is not a ZoneLink");
        
        AudioServer = GetNode<AudioPlayer>("/root/AudioPlayer");
        ZoneGroup = ResourceGroup.Of("res://Resources/ZoneGroup.tres");
        
        PopulateZones();
        base._Ready();

        SimulationStopped += SyncZoneResourcePositions;
    }

    private void SyncZoneResourcePositions()
    {
        foreach (var node in Nodes)
        {
            Zones[node.Index].Position = node.Position;
        }
    }

    public void AddPortal(int source, int target, string expiration, bool isManual = false)
    {
        if (LinkScene.Instantiate() is not ZoneLink link) return;
        if (ZoneLink.IsStampExpired(expiration)) return;
        
        link.Connect(source, target);
        link.ExpiresAt = expiration;
        AddLink(link);
        AudioServer.Play(AudioPlayer.SoundId.PortalOpen);
        if (isManual) EmitSignal(SignalName.PortalRegistered, source, target);
    }
    
    // TODO: i really need to rethink the approach to ZoneNode; since data is now stored in resources they dont need most of that data

    private void PopulateZones()
    {
        Zones = ZoneGroup.LoadAll().Cast<Zone>().ToArray();
        Array.Sort(Zones, (a, b) => a.Id - b.Id);

        for (var i = 0; i < Zones.Length; i++)
        {
            var zone = Zones[i];
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

    
    // TODO: man, this c#-gd bridging masturbation is driving me crazy, i should just stick to c# for everything, really
    /// <returns>Returns only paths that include portal connection. If none is found, returns a single on-land path. If starting zone is already in exit area, returns empty array</returns>
    public Godot.Collections.Array<Godot.Collections.Array<int>> FindAllPathsOut(int source, bool searchForRoyalExit)
    {
        var results = new Godot.Collections.Array<Godot.Collections.Array<int>>();
        var invalidExits = searchForRoyalExit ? new[] { Zone.ZoneType.Black, Zone.ZoneType.OutlandCity, Zone.ZoneType.Road } : new[] { Zone.ZoneType.Road };
        
        if (!invalidExits.Contains(Zones[source].Type)) return results;

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
                var neighborZone = Zones[neighbor];
                if (invalidExits.Contains(neighborZone.Type))
                {
                    if (!visited.Add(neighbor)) continue;
                    
                    var copy = new List<int>(path) { neighbor };
                    queue.Add(copy);
                    continue;
                }
                
                var fullPath = new List<int>(path) { neighbor };

                var links = new Godot.Collections.Array<int>();
                for (var i = 0; i < fullPath.Count - 1; i++)
                {
                    var from = fullPath[i];
                    var to  = fullPath[i + 1];
                    links.Add(Nodes[from].ConnectionLinkIndexes[Nodes[from].Connections.IndexOf(to)]);
                }
                
                results.Add(links);
            }
        }

        if (results.Count == 0) return results;

        var pathsWithPortal = results.Where(path => path.Any(link => new[] { Zones[Links[link].Source].Type, Zones[Links[link].Target].Type }.Contains(Zone.ZoneType.Road))).ToArray();
        if (pathsWithPortal.Length == 0) return results[..1];

        return new Godot.Collections.Array<Godot.Collections.Array<int>>(pathsWithPortal);
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
    
    public void HighlightLinks(int[] indexes)
    {
        foreach (var index in indexes)
        {
            var link = Links[index];
            if (link is not ZoneLink zoneLink) continue;
            zoneLink.Highlight();
        }
    }
}