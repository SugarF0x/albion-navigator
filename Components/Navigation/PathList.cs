using System;
using System.Collections.Generic;
using System.Linq;
using AlbionNavigator.Resources;
using AlbionNavigator.Services;
using Godot;

namespace AlbionNavigator.Components.Navigation;

public partial class PathList : VBoxContainer
{
	[Export] public PackedScene PathListItemScene;
	private Label ExpirationLabel;
	private Button CopyButton;
	private VBoxContainer PathItemsContainer;

	private Zone[] _zones = [];
	public Zone[] Zones
	{
		get =>  _zones;
		set
		{
			_zones = value;
			UpdateList();
		}
	}
	
	public override void _Ready()
	{
		ExpirationLabel = GetNode<Label>("%ExpirationLabel");
		CopyButton = GetNode<Button>("%CopyButton");
		PathItemsContainer = GetNode<VBoxContainer>("%PathItemsContainer");
		
		CopyButton.Pressed += CopyPath;
		
		UpdateList();
	}

	private void UpdateList()
	{
		foreach (var child in PathItemsContainer.GetChildren()) child.QueueFree();
		for (var i = 0; i < Zones.Length; i++)
		{
			var zone = Zones[i];
			if (PathListItemScene.Instantiate() is not PathListItem pathListItem) return;
			PathItemsContainer.AddChild(pathListItem);
			pathListItem.Index = i;
			pathListItem.Zone = zone;
		}
		
		var expiration = NavigationService.GetPathExpiration(Zones.Select(zone => zone.Id).ToArray());
		var formattedExpiration = expiration != null ? (DateTimeOffset.Parse(expiration) - DateTimeOffset.UtcNow).ToString(@"hh\:mm\:ss") : string.Empty;
		ExpirationLabel.Text = $"Expires in: {formattedExpiration}";
	}
	
	private void CopyPath()
	{
		var items = new List<string>
		{
			$"Expires: <t:{DateTimeOffset.Parse(NavigationService.GetPathExpiration(Zones.Select(zone => zone.Id).ToArray())).ToUnixTimeSeconds()}:R>"
		};
		items.AddRange(Zones.Select((zone, i) => $"{i + 1}. {SettingsService.Instance.ZoneTypeToChatIconCode.Value[zone.Type]} {zone.DisplayName}"));

		DisplayServer.ClipboardSet(string.Join("\n", items));
	}
}