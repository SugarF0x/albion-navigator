using System;
using System.Collections.Generic;
using AlbionNavigator.Resources;
using AlbionNavigator.Services;
using Godot;

namespace AlbionNavigator.Views.Home.ControlPanel.SettingsTab;

// TODO: split dropdowns into their own scenes

public partial class SettingsTab : MarginContainer
{
	private FoldableContainer DebugContainer;
	private Button LoadSampleLinksButton;
	private Button AddOneSampleLinkButton;
	private Button FlushStorageButton;
	private Button AddShortPortalPathButton;
	private HSlider VolumeSlider;
	private Label ZoneTypeSampleLabel;
	private TextEdit ZoneTypeSampleEdit;

	public override void _Ready()
	{
		DebugContainer = GetNode<FoldableContainer>("%DebugContainer");
		LoadSampleLinksButton = GetNode<Button>("%LoadSampleLinksButton");
		AddOneSampleLinkButton = GetNode<Button>("%AddOneSampleLinkButton");
		FlushStorageButton = GetNode<Button>("%FlushStorageButton");
		AddShortPortalPathButton = GetNode<Button>("%AddShortPortalPathButton");
		VolumeSlider = GetNode<HSlider>("%VolumeSlider");
		ZoneTypeSampleLabel = GetNode<Label>("%ZoneTypeDisplayNameLabel");
		ZoneTypeSampleEdit = GetNode<TextEdit>("%ZoneTypeIconTextEdit");

		LoadSampleLinksButton.Pressed += () => LinkService.Instance.LoadSampleLinks();;
		AddOneSampleLinkButton.Pressed += () =>
		{
			LinkService.Instance.AddLink(new ZoneLink(0, 450, DateTimeOffset.UtcNow.AddSeconds(3).ToString("O")));
			NavigationService.Instance.LastInspectedPath = [0, 450];
		};
		FlushStorageButton.Pressed += () => LinkService.Instance.FlushStorage();
		AddShortPortalPathButton.Pressed += () =>
		{
			LinkService.Instance.AddLink([
				new ZoneLink(0, 450, DateTimeOffset.UtcNow.AddSeconds(3).ToString("O")),
				new ZoneLink(450, 580, DateTimeOffset.UtcNow.AddSeconds(3).ToString("O")),
				new ZoneLink(580, 630, DateTimeOffset.UtcNow.AddSeconds(3).ToString("O")),
			]);
			NavigationService.Instance.LastInspectedPath = [0, 450, 580, 630];
		};

		VolumeSlider.SetValueNoSignal(SettingsService.Instance.Volume.Value);
		VolumeSlider.ValueChanged += value => { SettingsService.Instance.Volume.Value = (float)value; };
		
		if (!OS.HasFeature("debug")) DebugContainer.QueueFree();

		SetupZoneToIconMap();
	}

	private void SetupZoneToIconMap()
	{
		var container = ZoneTypeSampleLabel.GetParent();
		container.RemoveChild(ZoneTypeSampleLabel);
		container.RemoveChild(ZoneTypeSampleEdit);

		foreach (var type in Enum.GetValues(typeof(Zone.ZoneType)))
		{
			var label = ZoneTypeSampleLabel.Duplicate() as Label;
			var edit = ZoneTypeSampleEdit.Duplicate() as TextEdit;
			if (label == null || edit == null)
			{
				GD.PrintErr("label or edit are not of proper node types");
				return;
			}
			
			container.AddChild(label);
			container.AddChild(edit);

			label.Text = type.ToString();
			edit.Text = SettingsService.Instance.ZoneTypeToChatIconCode.Value[(Zone.ZoneType)type];
			edit.TextChanged += () =>
			{
				SettingsService.Instance.ZoneTypeToChatIconCode.Value =
					new Dictionary<Zone.ZoneType, string>(SettingsService.Instance.ZoneTypeToChatIconCode.Value)
					{
						[(Zone.ZoneType)type] = edit.Text
					};
			};
		}
	}
}