using System.Linq;
using AlbionNavigator.Components.Navigation;
using AlbionNavigator.Services;
using Godot;

namespace AlbionNavigator.Views.Home.ControlPanel.NavigationTab;

public partial class FindAllPathsOut : FoldableContainer
{
	// TODO: this could really be a single path list beneath both dropdowns
	//  since only one can be actively selected at a time anyway
	
	private OptionButton FromOptionButton;
	private Button SearchAllPathsOutButton;
	private PathList PathList;
	private VBoxContainer FoundPathContainer;
	private CheckButton RoyalExitSearchToggle;
	private Button PreviousPathButton;
	private Label CurrentPathLabel;
	private Button NextPathButton;
	
	public override void _Ready()
	{
		FromOptionButton = GetNode<OptionButton>("%FromOptionButton");
		SearchAllPathsOutButton = GetNode<Button>("%SearchAllPathsOutButton");
		FoundPathContainer = GetNode<VBoxContainer>("%FoundPathContainer");
		PathList = GetNode<PathList>("%PathList");
		RoyalExitSearchToggle = GetNode<CheckButton>("%RoyalExitSearchToggle");
		PreviousPathButton = GetNode<Button>("%PreviousPathButton");
		CurrentPathLabel = GetNode<Label>("%CurrentPathLabel");
		NextPathButton = GetNode<Button>("%NextPathButton");
		
		if (ZoneService.Instance.IsReady) AssignOptions();
		else ZoneService.Instance.AllResourcesLoaded += AssignOptions;

		SearchAllPathsOutButton.Pressed += SearchAllPathsOut;
		NavigationService.Instance.AllPathsOutUpdated += SyncPathList;

		PreviousPathButton.Pressed += DecIndex;
		NextPathButton.Pressed += IncIndex;

		SyncPathList([]);
	}

	private int RouteIndex = -1;

	private void IncIndex()
	{
		RouteIndex = int.Min(RouteIndex + 1, NavigationService.Instance.LastAllPathsOut.Length - 1);
		UpdateSelectedPath();
		UpdatePathListControlsLabel();
	}

	private void DecIndex()
	{
		RouteIndex = int.Max(RouteIndex - 1, 0);
		UpdateSelectedPath();
		UpdatePathListControlsLabel();
	}

	private void AssignOptions()
	{
		foreach (var zone in ZoneService.Instance.Zones) FromOptionButton.AddItem(zone.DisplayName, zone.Id);
	}

	private void SearchAllPathsOut()
	{
		if (FromOptionButton.Selected < 0) return;
		NavigationService.Instance.FindAllPathsOut(FromOptionButton.Selected, RoyalExitSearchToggle.ButtonPressed);
	}

	private void SyncPathList(int[][] paths)
	{
		RouteIndex = paths.Length > 0 ? 0 : -1;
		UpdateSelectedPath();
		UpdatePathListControlsLabel();
	}

	private void UpdateSelectedPath()
	{
		PathList.Zones = RouteIndex >= 0 ? NavigationService.Instance.LastAllPathsOut[RouteIndex].Select(index => ZoneService.Instance.Zones[index]).ToArray() : [];
		FoundPathContainer.Visible = PathList.Zones.Length > 0;
		NavigationService.Instance.LastInspectedPath = RouteIndex >= 0 ? NavigationService.Instance.LastAllPathsOut[RouteIndex] : [];
	}

	private void UpdatePathListControlsLabel()
	{
		PreviousPathButton.Disabled = RouteIndex <= 0;
		NextPathButton.Disabled = RouteIndex == -1 || RouteIndex == NavigationService.Instance.LastAllPathsOut.Length - 1;
		CurrentPathLabel.Text = $"{RouteIndex + 1}/{NavigationService.Instance.LastAllPathsOut.Length}";
	}
}