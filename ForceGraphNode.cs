using System.Collections.Generic;
using Godot;

namespace AlbionNavigator;

[GlobalClass]
public partial class ForceGraphNode : Node2D
{
    [ExportGroup("Forces")]
    [Export] public bool Frozen { get; set; }
    [Export] public float Strength { get; set; } = -40f;
    [Export] public float VelocityDecay { get; set; } = 0.6f;

    [ExportGroup("Initial Node Position")]
    [Export] public float InitialRadius { get; set; } = 30f;
    [Export] public float InitialAngle { get; set; } = float.Pi * (3f - float.Sqrt(5f));
    
    public int Index { get; set; } = -1;
    public Vector2 Velocity { get; set; } = Vector2.Zero;
    public List<int> Connections { get; } = [];
    
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
    
    public void Initialize(int graphIndex)
    {
        InitIndex(graphIndex);
        InitPosition();
        InitConnections();
    }

    private void InitIndex(int graphIndex)
    {
        if (Index < 0) Index = graphIndex;
    }

    private void InitPosition()
    {
        if (Frozen) return;
        if (Position != Vector2.Zero) return;
        PlaceNodeSpirally();
    }

    private void InitConnections()
    {
        Connections.Clear();
    }
}