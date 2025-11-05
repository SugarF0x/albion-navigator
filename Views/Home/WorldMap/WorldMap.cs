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
	[Export] public Control PanWrapper;
	
	public override void _Ready()
	{
		CallDeferred(nameof(SetupPosition));
		SetupMapLayout();
		SetupNodesSimulation();
		NodesSimulation.StartSimulation();
	}

	public override void _PhysicsProcess(double delta)
	{
		ProcessLinkExpirationThreadQueue();
	}

	private void SetupPosition()
	{
		PanWrapper.Position = Size / 2;
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
		LinkService.Instance.LinkExpirationUpdated += OnLinkExpirationUpdated;
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

	private void OnLinkExpirationUpdated(ZoneLink link, int from, int to)
	{
		NodesSimulation.MoveLinkElementByIndex(from, to);
	}
	
	private bool IsDragging;
	private Vector2 DragStartPos;

	public override void _GuiInput(InputEvent @event)
	{
		switch (@event)
		{
			case InputEventMouseButton buttonEvent:
			{
				if (buttonEvent.ButtonIndex == MouseButton.Left)
				{
					IsDragging = buttonEvent.Pressed;
					DragStartPos = buttonEvent.Position;
				}

				break;
			}
			case InputEventMouseMotion motionEvent when IsDragging:
				PanWrapper.Position += motionEvent.Relative;
				break;
		}
	}
}