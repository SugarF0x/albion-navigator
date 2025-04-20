using System.Collections.Generic;
using Godot;

namespace AlbionNavigator.Graph;

[GlobalClass]
public partial class ForceGraphNode : Node2D
{
    [ExportGroup("Forces")]
    [Export] public bool Frozen { get; set; }
    [Export] public float Strength { get; set; } = -40f * 25f;
    [Export] public float VelocityDecay { get; set; } = 0.6f;

    [ExportGroup("Initial Node Position")]
    [Export] public float InitialRadius { get; set; } = 30f * 5.1f;
    [Export] public float InitialAngle { get; set; } = float.Pi * (3f - float.Sqrt(5f * 5.1f));
    
    // TODO: now that i think about it, i am not convinced Index and Connections should even be here to begin with but rather be part of force directed graph instead
    public int Index { get; set; } = -1;
    public Vector2 Velocity { get; set; } = Vector2.Zero;
    public List<int> Connections { get; set; } = [];
    public List<int> ConnectionLinkIndexes { get; set; } = [];
    
    public void UpdatePosition()
    {
        if (Frozen)
        {
            Velocity = Vector2.Zero;
            return;
        }

        Velocity *= VelocityDecay;
        Position += Velocity;
    }

    public void PlaceNodeSpirally(int? orderIndex = null, float? placementRadius = null, float? placementAngle = null)
    {
        orderIndex ??= Index;
        placementRadius ??= InitialRadius;
        placementAngle ??= InitialAngle;
        
        var radius = placementRadius.Value * float.Sqrt(0.5f + orderIndex.Value);
        var angle = orderIndex.Value * placementAngle.Value;
        Position = new Vector2(radius * float.Cos(angle), radius * float.Sin(angle));
    }
    
    public void Initialize()
    {
        InitConnections();
        QueueRedraw();
    }

    private void InitConnections()
    {
        Connections.Clear();
        ConnectionLinkIndexes.Clear();
    }
}