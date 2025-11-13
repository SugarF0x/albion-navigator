using AlbionNavigator.Services;
using Godot;

namespace AlbionNavigator.Views.Home.ControlPanel.NavigationTab;

public partial class ConnectedPortals : FoldableContainer
{
	private OptionButton FromOptionButton;
	private Button HighlightConnectedPortalsButton; 
	
	public override void _Ready()
	{
		FromOptionButton = GetNode<OptionButton>("%FromOptionButton");
		HighlightConnectedPortalsButton = GetNode<Button>("%HighlightConnectedPortalsButton");
		
		if (ZoneService.Instance.IsReady) AssignOptions();
		else ZoneService.Instance.AllResourcesLoaded += AssignOptions;

		HighlightConnectedPortalsButton.Pressed += Highlight;
	}
	
	private void AssignOptions()
	{
		foreach (var zone in ZoneService.Instance.Zones) FromOptionButton.AddItem(zone.DisplayName, zone.Id);
	}

	private void Highlight()
	{
		if (FromOptionButton.Selected < 0) return;
		var path = NavigationService.Instance.GetAllAdjacentPortals(FromOptionButton.Selected);
		NavigationService.Instance.LastInspectedPath = path;
	}
}