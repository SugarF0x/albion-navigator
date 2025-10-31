using AlbionNavigator.Services;
using Godot;

namespace AlbionNavigator.Views.Home.ControlPanel.NavigationTab;

public partial class NavigationTab : MarginContainer
{
	public OptionButton FromOptionButton;
	public OptionButton ToOptionButton;
	
	public override void _Ready()
	{
		FromOptionButton = GetNode<OptionButton>("%FromOptionButton"); 
		ToOptionButton = GetNode<OptionButton>("%ToOptionButton");

		if (ZoneService.Instance.IsReady) AssignOptions();
		else ZoneService.Instance.AllResourcesLoaded += AssignOptions;
	}

	public void AssignOptions()
	{
		foreach (var zone in ZoneService.Instance.Zones)
		{
			FromOptionButton.AddItem(zone.DisplayName, zone.Id);
			ToOptionButton.AddItem(zone.DisplayName, zone.Id);
		}
	}
}