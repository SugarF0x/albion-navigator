using Godot;

namespace AlbionNavigator.Components.NodesSimulation;

public partial class NodeScene : Control
{
	public TextureRect NodeIcon;
	public Label DisplayNameLabel;

	private Resources.Zone _value;
	public Resources.Zone Value
	{
		get => _value;
		set
		{
			_value = value;
			SyncData();
		}
	}

	public override void _Ready()
	{
		NodeIcon = GetNode<TextureRect>("%NodeIcon");
		DisplayNameLabel = GetNode<Label>("%DisplayNameLabel");

		SyncData();
	}

	private void SyncData()
	{
		if (!IsNodeReady()) return;
		
		NodeIcon.Texture = Value?.GetZoneTexture();
		Position = Value?.Position ?? Vector2.Zero;
		DisplayNameLabel.Text = Value?.DisplayName ?? "";
	}
}