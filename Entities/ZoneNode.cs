using System;
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
			if (NameLabel == null) return;
			
			NameLabel.Text = value.Replace(" ", "\n");
			NameLabel.ResetSize();
			CallDeferred(nameof(UpdateLabelAttributes));
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
			LayerLabel.ResetSize();
			CallDeferred(nameof(UpdateLabelAttributes));
		}
	}

	private Zone _zone;
	[Export] public Zone Zone
	{
		get => _zone;
		set
		{
			_zone = value;
			LayerName = value.Layer != Zone.ZoneLayer.NonApplicable ? value.Layer.ToString() : "";
		}
	}
	
	private const float NodeRadius = 5f;

	private void UpdateLabelAttributes()
	{
		NameLabel.PivotOffset = NameLabel.Size / 2;
		NameLabel.Position = -NameLabel.PivotOffset with { Y = -NameLabel.PivotOffset.Y + NameLabel.Size.Y / 20 + NodeRadius };
		LayerLabel.PivotOffset = LayerLabel.Size / 2;
		LayerLabel.Position = -LayerLabel.PivotOffset with { Y = -LayerLabel.PivotOffset.Y + LayerLabel.Size.Y / 20 - NodeRadius * 2 };
	}

	[Export] public Label NameLabel;
	[Export] public Label LayerLabel;

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
		DisplayName = _displayName;

		if (Engine.IsEditorHint()) return;
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