using System;
using System.Collections.Generic;
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
	private Label ExpirationLabel;
	private Button CopyButton;
	
	public override void _Ready()
	{
		FromOptionButton = GetNode<OptionButton>("%FromOptionButton"); 
		ToOptionButton = GetNode<OptionButton>("%ToOptionButton");
		SearchShortestPathButton = GetNode<Button>("%SearchShortestPathButton");
		FoundPathContainer = GetNode<VBoxContainer>("%FoundPathContainer");
		PathList = GetNode<PathList>("%PathList");
		ExpirationLabel = GetNode<Label>("%ExpirationLabel");
		CopyButton = GetNode<Button>("%CopyButton");
		
		if (ZoneService.Instance.IsReady) AssignOptions();
		else ZoneService.Instance.AllResourcesLoaded += AssignOptions;

		SearchShortestPathButton.Pressed += SearchShortestPath;
		CopyButton.Pressed += CopyPath;
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

		var expiration = NavigationService.GetPathExpiration(path);
		var formattedExpiration = expiration != null ? (DateTimeOffset.Parse(expiration) - DateTimeOffset.UtcNow).ToString(@"hh\:mm\:ss") : string.Empty;
		ExpirationLabel.Text = $"Expires in: {formattedExpiration}";
	}

	private void CopyPath()
	{
		var items = new List<string>
		{
			$"Expires: <t:{DateTimeOffset.Parse(NavigationService.GetPathExpiration(NavigationService.Instance.LastShortestPath)).ToUnixTimeSeconds()}:R>"
		};
		items.AddRange(PathList.Zones.Select((zone, i) => $"{i + 1}. {SettingsService.Instance.ZoneTypeToChatIconCode.Value[zone.Type]} {zone.DisplayName}"));

		DisplayServer.ClipboardSet(string.Join("\n", items));
	}
}