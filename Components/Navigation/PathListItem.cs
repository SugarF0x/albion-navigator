using Godot;

namespace AlbionNavigator.Components.Navigation;

public partial class PathListItem : HBoxContainer
{
	private TextureRect Icon;
	private Label IndexLabel;
	private Label DisplayName;

	private Resources.Zone _zone;
	[Export] public Resources.Zone Zone
	{
		get =>  _zone;
		set
		{
			_zone = value;
			SyncZoneData();
		}
	}

	private int _index = -1;
	[Export] public int Index
	{
		get =>  _index;
		set
		{
			_index = value;
			SyncIndexLabel();
		}
	}
	
	public override void _Ready()
	{
		Icon = GetNode<TextureRect>("%Icon");
		DisplayName = GetNode<Label>("%DisplayName");
		IndexLabel = GetNode<Label>("%IndexLabel");

		SyncIndexLabel();
		SyncZoneData();
	}

	private void SyncIndexLabel()
	{
		IndexLabel.Text = $"{Index + 1}.";
		IndexLabel.Visible = Index >= 0;
	}

	private void SyncZoneData()
	{
		if (Zone == null) return;
		Icon.Texture = Zone.GetZoneTexture();
		DisplayName.Text = Zone.DisplayName;
	}
}