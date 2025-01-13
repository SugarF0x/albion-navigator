using System.Collections.Generic;
using AlbionNavigator.Data;
using AlbionNavigator.Graph;
using Godot;

namespace AlbionNavigator.Entities;

[Tool]
[GlobalClass]
public partial class ZoneNode : ForceGraphNode
{
	[Export]
	public Zone.ZoneType Type
	{
		get => _type;
		set
		{
			_type = value;
			QueueRedraw();
		}
	}

	[Export] public string DisplayName { get; set; } = "";

	private Zone.ZoneType _type = Zone.ZoneType.StartingCity; 

	public readonly Dictionary<Zone.ZoneType, Color> TypeToColorMap = new()
	{
		{ Zone.ZoneType.StartingCity, Colors.Purple },
		{ Zone.ZoneType.City, Colors.Green },
		{ Zone.ZoneType.SafeArea, Colors.Blue },
		{ Zone.ZoneType.Yellow, Colors.Yellow },
		{ Zone.ZoneType.Red, Colors.Red },
		{ Zone.ZoneType.Black, Colors.Black },
		{ Zone.ZoneType.Road, Colors.White }
	};
	
	public override void _Draw()
	{
		DrawCircle(Vector2.Zero, 5.0f, Colors.White);
		DrawCircle(Vector2.Zero, 4.0f, TypeToColorMap[Type]);
		Modulate = Modulate with { A = Connections.Count > 0 ? 1f : 0.25f };
	}
}