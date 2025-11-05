using AlbionNavigator.Components.NodesSimulation;
using Godot;

namespace AlbionNavigator.Views.Home.WorldMap;

public partial class WorldMap : Control
{
	[Export] public float MapScale = 2f;
	[Export] public TextureRect MapBackground;
	[Export] public NodesSimulation NodesSimulation;
	
	public override void _Ready()
	{
		NodesSimulation.PositionScale = MapScale;
		UpdateMapLayout();
	}

	private void UpdateMapLayout()
	{
		MapBackground.Scale *= MapScale;
		MapBackground.Position = MapBackground.Size * MapBackground.Scale / -2;
	}
}