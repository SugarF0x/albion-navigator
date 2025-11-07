using AlbionNavigator.Resources;
using Godot;

namespace AlbionNavigator.Components.Navigation;

public partial class PathList : VBoxContainer
{
	[Export] public PackedScene PathListItemScene;

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
		UpdateList();
	}

	private void UpdateList()
	{
		foreach (var child in GetChildren()) child.QueueFree();
		for (var i = 0; i < Zones.Length; i++)
		{
			var zone = Zones[i];
			if (PathListItemScene.Instantiate() is not PathListItem pathListItem) return;
			AddChild(pathListItem);
			pathListItem.Index = i;
			pathListItem.Zone = zone;
		}
	}
}