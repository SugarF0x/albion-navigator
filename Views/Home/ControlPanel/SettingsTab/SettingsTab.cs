using AlbionNavigator.Services;
using Godot;

namespace AlbionNavigator.Views.Home.ControlPanel.SettingsTab;

public partial class SettingsTab : MarginContainer
{
	private Button LoadSampleLinksButton;
	
	public override void _Ready()
	{
		LoadSampleLinksButton = GetNode<Button>("%LoadSampleLinksButton");
		LoadSampleLinksButton.Pressed += LoadSampleLinks;
		if (!Engine.IsEditorHint()) LoadSampleLinksButton.QueueFree();
	}

	private void LoadSampleLinks()
	{
		LinkService.Instance.LoadSampleLinks();
	}
}