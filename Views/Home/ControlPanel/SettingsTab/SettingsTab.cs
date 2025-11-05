using AlbionNavigator.Services;
using Godot;

namespace AlbionNavigator.Views.Home.ControlPanel.SettingsTab;

public partial class SettingsTab : MarginContainer
{
	private Button LoadSampleLinksButton;
	private FoldableContainer DebugContainer;
	
	public override void _Ready()
	{
		LoadSampleLinksButton = GetNode<Button>("%LoadSampleLinksButton");
		DebugContainer = GetNode<FoldableContainer>("%DebugContainer");
		
		LoadSampleLinksButton.Pressed += LoadSampleLinks;
		if (!OS.HasFeature("debug")) DebugContainer.QueueFree();
	}

	private void LoadSampleLinks()
	{
		LinkService.Instance.LoadSampleLinks();
	}
}