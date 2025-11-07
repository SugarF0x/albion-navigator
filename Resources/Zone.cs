using Godot;

namespace AlbionNavigator.Resources;

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

    public Texture2D GetZoneTexture() => GetZoneTexture(this);
    public static Texture2D GetZoneTexture(Zone zone)
    {
        switch (zone.Type)
        {
            case ZoneType.StartingCity: return GD.Load<Texture2D>("res://Assets/Images/NodeIcons/starting-city.png"); 
            case ZoneType.City: return GD.Load<Texture2D>("res://Assets/Images/NodeIcons/city.png"); 
            case ZoneType.SafeArea: return GD.Load<Texture2D>("res://Assets/Images/NodeIcons/blue-zone.png"); 
            case ZoneType.Yellow: return GD.Load<Texture2D>("res://Assets/Images/NodeIcons/yellow-zone.png"); 
            case ZoneType.Red: return GD.Load<Texture2D>("res://Assets/Images/NodeIcons/red-zone.png"); 
            case ZoneType.Black: return GD.Load<Texture2D>("res://Assets/Images/NodeIcons/black-zone.png"); 
            case ZoneType.OutlandCity: return GD.Load<Texture2D>("res://Assets/Images/NodeIcons/outland-city.png");
            case ZoneType.Road:
            default: return GD.Load<Texture2D>("res://Assets/Images/NodeIcons/portal.png"); 
        }
    }
}