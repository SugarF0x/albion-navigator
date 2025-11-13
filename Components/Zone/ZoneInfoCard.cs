using System.Linq;
using Godot;

namespace AlbionNavigator.Components.Zone;

public partial class ZoneInfoCard : PanelContainer
{
	private Resources.Zone _zone;
	[Export] public Resources.Zone Zone
	{
		get => _zone;
		set
		{
			_zone = value;
			SyncZoneData();
		}
	}
	
	private Label DisplayNameLabel;
	private ZoneComponentStack ComponentStack;
	
	public override void _Ready()
	{
		DisplayNameLabel = GetNode<Label>("%DisplayNameLabel");
		ComponentStack = GetNode<ZoneComponentStack>("%ZoneComponentStack");
		
		SyncZoneData();
	}

	private void SyncZoneData()
	{
		if (!IsNodeReady()) return;
		if (Zone == null) return;

		DisplayNameLabel.Text = Zone.DisplayName;
		ComponentStack.Components = Zone.Components.ToArray();
		ComponentStack.Visible = ComponentStack.Components.Length > 0;
	}
}