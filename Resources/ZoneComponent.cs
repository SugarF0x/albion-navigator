using Godot;

namespace AlbionNavigator.Resources;

[Tool]
[GlobalClass]
public partial class ZoneComponent : Resource
{
    public enum ZoneComponentType
    {
        Dungeon,
        Chest,
        Gather,
        MistsCity,
    }

    public enum ZoneComponentProperty
    {
        Small,
        Big,
        Keeper,
        Heretic,
        Morgana,
        Undead,
        Avalonian,
        Green,
        Blue,
        Gold,
        Ore,
        Stone,
        Wood,
        Fiber,
        Hide,
    }
    
    [Export] public int Id { get; set; }
    [Export] public ZoneComponentType Type { get; set; }
    [Export] public string DisplayName { get; set; }
    [Export] public int Tier { get; set; }
    [Export] public Godot.Collections.Array<ZoneComponentProperty> Properties { get; set; } = [];
}