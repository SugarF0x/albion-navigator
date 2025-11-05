using System;
using AlbionNavigator.Services;
using Godot;

namespace AlbionNavigator.Views.Home.ControlPanel.SettingsTab;

public partial class SettingsTab : MarginContainer
{
	private FoldableContainer DebugContainer;
	private Button LoadSampleLinksButton;
	private Button AddOneSampleLinkButton;
	private Button FlushStorageButton;
	private Button AddShortPortalPathButton;

	public override void _Ready()
	{
		DebugContainer = GetNode<FoldableContainer>("%DebugContainer");
		LoadSampleLinksButton = GetNode<Button>("%LoadSampleLinksButton");
		AddOneSampleLinkButton = GetNode<Button>("%AddOneSampleLinkButton");
		FlushStorageButton = GetNode<Button>("%FlushStorageButton");
		AddShortPortalPathButton = GetNode<Button>("%AddShortPortalPathButton");

		LoadSampleLinksButton.Pressed += () => LinkService.Instance.LoadSampleLinks();;
		AddOneSampleLinkButton.Pressed += () => LinkService.Instance.AddLink(new ZoneLink(0, 450, DateTimeOffset.UtcNow.AddSeconds(3).ToString("O")));
		FlushStorageButton.Pressed += () => LinkService.Instance.FlushStorage();
		AddShortPortalPathButton.Pressed += () => LinkService.Instance.AddLink([
			new ZoneLink(0, 450, DateTimeOffset.UtcNow.AddSeconds(3).ToString("O")),
			new ZoneLink(450, 580, DateTimeOffset.UtcNow.AddSeconds(3).ToString("O")),
			new ZoneLink(580, 630, DateTimeOffset.UtcNow.AddSeconds(3).ToString("O")),
		]);
		
		if (!OS.HasFeature("debug")) DebugContainer.QueueFree();
	}
}