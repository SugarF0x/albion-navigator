using Godot;

[GlobalClass]
public partial class Zone : Resource
{
    public enum ZoneType
    {
        StartingCity,
        City,
        SafeArea,
        Yellow,
        Red,
        Black,
        Road,
        OutlandCity,
    }

    public enum ZoneLayer
    {
        NonApplicable,
        L1Royal,
        L1RoyalRed,
        L1Outer,
        L1Middle,
        L1Inner,
        L2Outer,
        L3Hub,
        L2Middle,
        L2Inner,
        L3Deep,
        L2Rest,
        L3DeepRest,
    }

    [Export] public int Id { get; set; }
    [Export] public ZoneType Type { get; set; }
    [Export] public ZoneLayer Layer { get; set; }
    [Export] public string DisplayName { get; set; }
    [Export] public Vector2 Position { get; set; }
    [Export] public Godot.Collections.Array<int> Connections { get; set; } = [];
    [Export] public Godot.Collections.Array<ZoneComponent> Components { get; set; } = [];
    // TODO: add zone tier (also need to repull source data since some zones have changed tiers)
}
