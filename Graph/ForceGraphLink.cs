using System;
using Godot;

namespace AlbionNavigator.Graph;

[GlobalClass]
public partial class ForceGraphLink : Node2D
{
    public Line2D Line;

    [Export] public float DesiredDistance = 30f;

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

    public override void _Ready()
    {
        Line = GetNode<Line2D>("Line2D");
    }

    public void Connect(int from, int to)
    {
        Source = from;
        Target = to;
    }

    public void DrawLink(ForceGraphNode[] nodes)
    {
        Line.ClearPoints();
        if (Source < 0 || Source > nodes.Length) return;
        if (Target < 0 || Target > nodes.Length) return;
        Line.AddPoint(nodes[Source].Position);
        Line.AddPoint(nodes[Target].Position);
    }

    public void Initialize(ForceGraphNode[] nodes)
    {
        try
        {
            InitConnectionsCount(nodes);
            InitBias(nodes);
            InitStrength(nodes);
        }
        catch
        {
            QueueFree();
        }
    }

    private void InitConnectionsCount(ForceGraphNode[] nodes)
    {
        if (Source >= nodes.Length || Target >= nodes.Length) throw new IndexOutOfRangeException("Cannot add link: no node to connect to");
        if (nodes[Source].Connections.Contains(Target)) throw new ArgumentException("Cannot add link: source already connected");
        nodes[Source].Connections.Add(Target);
        nodes[Target].Connections.Add(Source);
    }

    private void InitBias(ForceGraphNode[] nodes)
    {
        var sourceConnectionsCount = (float)nodes[Source].Connections.Count;
        var targetConnectionsCount = (float)nodes[Target].Connections.Count;
        Bias = sourceConnectionsCount / (sourceConnectionsCount + targetConnectionsCount);
    }

    private void InitStrength(ForceGraphNode[] nodes)
    {
        var sourceConnectionsCount = (float)nodes[Source].Connections.Count;
        var targetConnectionsCount = (float)nodes[Target].Connections.Count;
        Strength = 1f / float.Min(sourceConnectionsCount, targetConnectionsCount);
    }
}