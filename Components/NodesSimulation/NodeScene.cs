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
			NodeIcon.Texture = value.GetZoneTexture();
			Position = value.Position;
		}
	}

	public override void _Ready()
	{
		NodeIcon = GetNode<TextureRect>("%NodeIcon");
	}
}