using System;
using System.Collections.Generic;
using System.Linq;
using AlbionNavigator.Entities;
using AlbionNavigator.Graph;
using Godot;

namespace AlbionNavigator.Autoload;

public partial class PersistedStore : Node
{
    private const int VERSION = 0;
    private const string SAVE_PATH = "user://store.save";
    private const string SAMPLE_SAVE_PATH = "user://sample_store.save";
    
    private ZoneMap Graph;
    
    public override void _Ready()
    {
        var graph = GetTree().GetFirstNodeInGroup("ForceGraph");
        if (graph is not ZoneMap zoneMap) return;
        Graph = zoneMap;
        
        Graph.ChildrenRegisteredNative += Hydrate;
    }

    private void Hydrate(ForceGraphNode[] nodes, ForceGraphLink[] links)
    {
        Graph.ChildrenRegisteredNative -= Hydrate;
        Graph.ChildrenRegisteredNative += Persist;

        if (!FileAccess.FileExists(SAVE_PATH)) return;
        using var file = FileAccess.Open(SAVE_PATH, FileAccess.ModeFlags.Read);

        var content = file.GetAsText().Split("|");
        var version = int.Parse(content[0]);
        var data = content[1].Split(";");

        if (version != VERSION) return;

        foreach (var item in data)
        {
            if (item == "") continue;
            
            try
            {
                var chunks = item.Split(',');
                var source = int.Parse(chunks[0]);
                var target = int.Parse(chunks[1]);
                var expiresAt = chunks[2];
                Graph.AddPortal(source, target, expiresAt);
            }
            catch
            {
                // ignored
            }
        }
    }

    public void LoadSampleData()
    {
        if (!FileAccess.FileExists(SAMPLE_SAVE_PATH)) return;
        using var file = FileAccess.Open(SAMPLE_SAVE_PATH, FileAccess.ModeFlags.Read);

        var content = file.GetAsText().Split("|");
        var version = int.Parse(content[0]);
        var data = content[1].Split(";");

        if (version != VERSION) return;

        foreach (var item in data)
        {
            if (item == "") continue;
            
            try
            {
                var chunks = item.Split(',');
                var source = int.Parse(chunks[0]);
                var target = int.Parse(chunks[1]);
                var expiresAt = DateTime.UtcNow;
                expiresAt = expiresAt.AddMinutes(10);
                Graph.AddPortal(source, target, expiresAt.ToString("s"));
            }
            catch
            {
                // ignored
            }
        }
    }
    
    private void Persist(ForceGraphNode[] nodes, ForceGraphLink[] links)
    {
        var portalConnections = new List<ZoneLink>();
        for (var i = links.Length - 1; i >= 0; i--)
        {
            var link = links[i];
            if (link is not ZoneLink zoneLink || zoneLink.ExpiresAt == null) break;
            portalConnections.Add(zoneLink);
        }

        portalConnections.Reverse();
        var portalLinks = portalConnections.ToArray();

        var dataString = portalLinks.Aggregate($"{VERSION}|", (current, link) => current + $"{link.Source},{link.Target},{link.ExpiresAt};");
        
        using var file = FileAccess.Open(SAVE_PATH, FileAccess.ModeFlags.Write);
        file.StoreString(dataString);
    }
}