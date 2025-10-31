using System.Linq;
using AlbionNavigator.Services;
using Godot;

public partial class Startup : Control
{
	private Label TaskLabel;
	private ProgressBar TaskProgressBar;
	
	public override void _Ready()
	{
		TaskLabel = GetNode<Label>("%TaskLabel");
		TaskProgressBar = GetNode<ProgressBar>("%TaskProgressBar");
		ZoneService.Instance.AllResourcesLoaded += () => CallDeferred(nameof(OnAllResourcesLoaded));
		ZoneService.Instance.LoadAllZones();
	}
	
	public override void _Process(double delta)
	{
		var loader = ZoneService.Instance.Loader;
		if (loader.Resources.Count == 0) return;
		
		var zone = (Zone)loader.Resources.Last();
		TaskLabel.Text = zone.DisplayName;
		TaskProgressBar.Value = loader.Progress;
	}

	private void OnAllResourcesLoaded()
	{
		GetTree().ChangeSceneToFile("res://Views/Home/Home.tscn");
	}
}
