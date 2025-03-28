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
	private Control HoverArea;
	private Control ZoneInfoPopup;

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
		HoverArea = GetNode<Control>("%HoverArea");
		ZoneInfoPopup = GetNode<Control>("%ZoneInfoPopup");
		if (Engine.IsEditorHint()) return;
		
		Zone = Zone;
		ZoneInfoPopup.Set("zone", Zone);
		HideName();
		ZoneInfoPopup.Visible = false;
		HoverArea.MouseEntered += ShowPopup;
		HoverArea.MouseExited += HidePopup;
	}

	private float ElementOpacity => Engine.IsEditorHint() ? 1f : Connections.Count > 0 ? 1f : 0.25f;
	
	public override void _Draw()
	{
		DrawCircle(Vector2.Zero, NodeRadius, Colors.White with { A = ElementOpacity });
		DrawCircle(Vector2.Zero, NodeRadius * .8f, TypeToColorMap[Type] with { A = ElementOpacity });
		ApplyElementOpacity();
	}

	public void ShowName() { NameLabel.Show(); LayerLabel.Show(); }
	public void HideName() { NameLabel.Hide(); LayerLabel.Hide(); }

	public void ShowPopup() { ZoneInfoPopup.Call("fade", false); }
	public void HidePopup() { ZoneInfoPopup.Call("fade", true); }

	private void ApplyElementOpacity()
	{
		NameLabel.Modulate = NameLabel.Modulate with { A = ElementOpacity };
		LayerLabel.Modulate = LayerLabel.Modulate with { A = ElementOpacity };
	}
}