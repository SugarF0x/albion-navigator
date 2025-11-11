using System.Linq;
using AlbionNavigator.Components.Navigation;
using AlbionNavigator.Services;
using Godot;

namespace AlbionNavigator.Views.Home.ControlPanel.NavigationTab;

public partial class FindShortestPath : FoldableContainer
{
	private OptionButton FromOptionButton;
	private OptionButton ToOptionButton;
	private Button SearchShortestPathButton;
	private PathList PathList;
	private VBoxContainer FoundPathContainer;
	
	public override void _Ready()
	{
		FromOptionButton = GetNode<OptionButton>("%FromOptionButton"); 
		ToOptionButton = GetNode<OptionButton>("%ToOptionButton");
		SearchShortestPathButton = GetNode<Button>("%SearchShortestPathButton");
		FoundPathContainer = GetNode<VBoxContainer>("%FoundPathContainer");
		PathList = GetNode<PathList>("%PathList");
		
		if (ZoneService.Instance.IsReady) AssignOptions();
		else ZoneService.Instance.AllResourcesLoaded += AssignOptions;

		SearchShortestPathButton.Pressed += SearchShortestPath;
		NavigationService.Instance.ShortestPathUpdated += SyncPathList;

		SyncPathList([]);
	}

	private void AssignOptions()
	{
		foreach (var zone in ZoneService.Instance.Zones)
		{
			FromOptionButton.AddItem(zone.DisplayName, zone.Id);
			ToOptionButton.AddItem(zone.DisplayName, zone.Id);
		}
	}

	private void SearchShortestPath()
	{
		if (FromOptionButton.Selected < 0 || ToOptionButton.Selected < 0) return;
		NavigationService.Instance.FindShortestPath(FromOptionButton.Selected, ToOptionButton.Selected);
	}

	private void SyncPathList(int[] path)
	{
		PathList.Zones = path.Select(index => ZoneService.Instance.Zones[index]).ToArray();
		FoundPathContainer.Visible = PathList.Zones.Length > 0;
	}
}