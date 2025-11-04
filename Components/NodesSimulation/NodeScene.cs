using AlbionNavigator.Resources;
using Godot;

namespace AlbionNavigator.Components.NodesSimulation;

public partial class NodeScene : Control
{
	public TextureRect NodeIcon;

	private Zone _value;
	public Zone Value
	{
		get => _value;
		set
		{
			_value = value;
			NodeIcon.Texture = GetZoneTexture(value);
			Position = value.Position;
		}
	}

	public override void _Ready()
	{
		NodeIcon = GetNode<TextureRect>("%NodeIcon");
	}

	private static Texture2D GetZoneTexture(Zone zone)
	{
		switch (zone.Type)
		{
			case Zone.ZoneType.StartingCity: return GD.Load<Texture2D>("res://Assets/Images/NodeIcons/starting-city.png"); 
			case Zone.ZoneType.City: return GD.Load<Texture2D>("res://Assets/Images/NodeIcons/city.png"); 
			case Zone.ZoneType.SafeArea: return GD.Load<Texture2D>("res://Assets/Images/NodeIcons/blue-zone.png"); 
			case Zone.ZoneType.Yellow: return GD.Load<Texture2D>("res://Assets/Images/NodeIcons/yellow-zone.png"); 
			case Zone.ZoneType.Red: return GD.Load<Texture2D>("res://Assets/Images/NodeIcons/red-zone.png"); 
			case Zone.ZoneType.Black: return GD.Load<Texture2D>("res://Assets/Images/NodeIcons/black-zone.png"); 
			case Zone.ZoneType.OutlandCity: return GD.Load<Texture2D>("res://Assets/Images/NodeIcons/outland-city.png");
			case Zone.ZoneType.Road:
			default: return GD.Load<Texture2D>("res://Assets/Images/NodeIcons/portal.png"); 
		}
	}
}