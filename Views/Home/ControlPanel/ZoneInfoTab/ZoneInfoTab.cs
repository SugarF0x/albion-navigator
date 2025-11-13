using System;
using AlbionNavigator.Components.Zone;
using AlbionNavigator.Services;
using Godot;

namespace AlbionNavigator.Views.Home.ControlPanel.ZoneInfoTab;

public partial class ZoneInfoTab : MarginContainer
{
	[ExportGroup("Internals")]
	[Export] public PackedScene ZoneInfoCardScene;
	
	private VBoxContainer ZoneCardsContainer;
	
	public override void _Ready()
	{
		if (ZoneInfoCardScene.Instantiate() is not ZoneInfoCard) throw new InvalidCastException("ZoneInfoCardScene is not of ZoneInfoCard type");
		
		ZoneCardsContainer = GetNode<VBoxContainer>("%ZoneCardsContainer");
		
		NavigationService.Instance.LastInspectedPathUpdated += DisplayZoneCards;
	}

	private void DisplayZoneCards(int[] path)
	{
		foreach (var child in ZoneCardsContainer.GetChildren()) child.QueueFree();
		foreach (var zoneIndex in path)
		{
			var child = ZoneInfoCardScene.Instantiate<ZoneInfoCard>();
			child.Zone = ZoneService.Instance.Zones[zoneIndex];
			ZoneCardsContainer.AddChild(child);
		}
	}
}