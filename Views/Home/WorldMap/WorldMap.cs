using System.Collections.Generic;
using AlbionNavigator.Components.NodesSimulation;
using AlbionNavigator.Services;
using Godot;

namespace AlbionNavigator.Views.Home.WorldMap;

public partial class WorldMap : Control
{
	[Export] public float MapScale = 2f;
	[Export] public TextureRect MapBackground;
	[Export] public NodesSimulation NodesSimulation;
	
	public override void _Ready()
	{
		SetupMapLayout();
		SetupNodesSimulation();
		NodesSimulation.StartSimulation();
	}

	public override void _Process(double delta)
	{
		ProcessLinkExpirationThreadQueue();
	}

	private void SetupMapLayout()
	{
		MapBackground.Scale *= MapScale;
		MapBackground.Position = MapBackground.Size * MapBackground.Scale / -2;
	}

	private void SetupNodesSimulation()
	{
		NodesSimulation.PositionScale = MapScale;
		NodesSimulation.Zones = ZoneService.Instance.Zones;
		NodesSimulation.Links = LinkService.Instance.Links;

		LinkService.Instance.NewLinkAdded += OnLinkAdded;
		LinkService.Instance.ExpiredLinkRemoved += OnLinkExpired;
	}

	public override void _ExitTree()
	{
		LinkService.Instance.NewLinkAdded -= OnLinkAdded;
		LinkService.Instance.ExpiredLinkRemoved -= OnLinkExpired;
	}

	private readonly Queue<(ZoneLink, int)> ExpirationQueue = new ();
	private void OnLinkExpired(ZoneLink link, int index) { lock (ExpirationQueue) ExpirationQueue.Enqueue((link, index)); }
	private void ProcessLinkExpirationThreadQueue()
	{
		lock (ExpirationQueue)
		{
			if (ExpirationQueue.Count == 0) return;
			while (ExpirationQueue.Count > 0)
			{
				var (link, index) = ExpirationQueue.Dequeue();
				NodesSimulation.RemoveLink(link, index);
			}
			
			NodesSimulation.StartSimulation();
		}
	}
	
	private void OnLinkAdded(ZoneLink link, int index)
	{
		NodesSimulation.AddLink(link, index);
		NodesSimulation.StartSimulation();
	}
}