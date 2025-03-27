using System.Collections.Generic;
using AlbionNavigator.Graph;
using Godot;

namespace AlbionNavigator.Entities;

[Tool]
[GlobalClass]
public partial class ZoneNode : ForceGraphNode
{
	private Zone.ZoneType _type = Zone.ZoneType.StartingCity; 
	[Export] public Zone.ZoneType Type
	{
		get => _type;
		set
		{
			_type = value;
			QueueRedraw();
		}
	}

	private string _displayName = "";
	[Export] public string DisplayName
	{
		get => _displayName;
		set
		{
			_displayName = value;
			if (NameLabel == null || value == null) return;
			
			NameLabel.Text = value.Replace(" ", "\n");
		}
	}

	private string _layerName = "";
	[Export] public string LayerName
	{
		get => _layerName;
		set
		{
			_layerName = value;
			if (LayerLabel == null) return;
			
			LayerLabel.Text = value;
		}
	}

	private Zone _zone;
	[Export] public Zone Zone
	{
		get => _zone;
		set
		{
			_zone = value;
			if (value == null) return;
			
			LayerName = value.Layer != Zone.ZoneLayer.NonApplicable ? value.Layer.ToString() : "";
			DisplayName = value.DisplayName;
		}
	}
	
	private const float NodeRadius = 16f;

	private Label NameLabel;
	private Label LayerLabel;

	public readonly Dictionary<Zone.ZoneType, Color> TypeToColorMap = new()
	{
		{ Zone.ZoneType.StartingCity, Colors.Purple },
		{ Zone.ZoneType.City, Colors.Green },
		{ Zone.ZoneType.SafeArea, Colors.Blue },
		{ Zone.ZoneType.Yellow, Colors.Yellow },
		{ Zone.ZoneType.Red, Colors.Red },
		{ Zone.ZoneType.Black, Colors.Black },
		{ Zone.ZoneType.Road, Colors.White },
		{ Zone.ZoneType.OutlandCity, Colors.Orange },
	};

	public override void _Ready()
	{
		NameLabel = GetNode<Label>("%DisplayNameLabel");
		LayerLabel = GetNode<Label>("%LayerLabel");
		
		if (Engine.IsEditorHint()) return;
		Zone = Zone;
		HideName();
	}
	
	public override void _Draw()
	{
		DrawCircle(Vector2.Zero, NodeRadius, Colors.White);
		DrawCircle(Vector2.Zero, NodeRadius * .8f, TypeToColorMap[Type]);
		ApplyOpacity();
	}

	public void ShowName() { NameLabel.Show(); LayerLabel.Show(); }
	public void HideName() { NameLabel.Hide(); LayerLabel.Hide(); }
	
	private void ApplyOpacity()
	{
		if (Engine.IsEditorHint()) return;
		Modulate = Modulate with { A = Connections.Count > 0 ? 1f : 0.25f };
	}
}