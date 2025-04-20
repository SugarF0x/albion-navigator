using System;
using System.Linq;
using AlbionNavigator.Services;
using Godot;

namespace AlbionNavigator.Graph;

[GlobalClass]
public partial class ForceGraphLink : Node2D
{
    [Export] public Line2D Line;
    [Export] public float DesiredDistance = 30f * 4f;

    public int Source = -1;
    public int Target = -1;
    public float Bias = 1;
    public float Strength = 1;

    public ForceGraphLink() { }
    public ForceGraphLink(int? source = null, int? target = null)
    {
        if (!source.HasValue || !target.HasValue) return;
        Source = source.Value;
        Target = target.Value;
    }

    public void Connect(int from, int to)
    {
        Source = from;
        Target = to;
    }

    public virtual bool DrawLink()
    {
        Line.ClearPoints();
        if (Source < 0 || Source > ZoneService.Instance.Zones.Length) return false;
        if (Target < 0 || Target > ZoneService.Instance.Zones.Length) return false;
        
        Line.AddPoint(ZoneService.Instance.Zones[Source].Position);
        Line.AddPoint(ZoneService.Instance.Zones[Target].Position);

        return true;
    }

    public virtual void Initialize(int graphIndex, ForceGraphNode[] nodes)
    {
        try
        {
            InitConnectionsCount(graphIndex, nodes);
            InitBias(nodes);
            InitStrength(nodes);
        }
        catch
        {
            QueueFree();
        }
    }

    private void InitConnectionsCount(int graphIndex, ForceGraphNode[] nodes)
    {
        if (Source >= nodes.Length || Target >= nodes.Length) throw new IndexOutOfRangeException("Cannot add link: no node to connect to");
        if (nodes[Source].Connections.Contains(Target)) throw new ArgumentException("Cannot add link: source already connected");
        nodes[Source].Connections.Add(Target);
        nodes[Source].ConnectionLinkIndexes.Add(graphIndex);
        nodes[Target].Connections.Add(Source);
        nodes[Target].ConnectionLinkIndexes.Add(graphIndex);
    }

    private void InitBias(ForceGraphNode[] nodes)
    {
        var sourceConnectionsCount = (float)nodes[Source].Connections.Count;
        var targetConnectionsCount = (float)nodes[Target].Connections.Count;
        Bias = sourceConnectionsCount / (sourceConnectionsCount + targetConnectionsCount);
    }

    protected virtual void InitStrength(ForceGraphNode[] nodes)
    {
        var sourceConnectionsCount = (float)nodes[Source].Connections.Count;
        var targetConnectionsCount = (float)nodes[Target].Connections.Count;
        Strength = 1f / float.Min(sourceConnectionsCount, targetConnectionsCount);
    }
}