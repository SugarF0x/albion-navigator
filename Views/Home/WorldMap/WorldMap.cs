using System;
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
	
	// TODO: should these map controls for grabbing and zoom inside NodesSimulation instead of WorldMap??
	
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
				var zoomFactor = 1f;
				switch (buttonEvent.ButtonIndex)
				{
					case MouseButton.Left:
						IsDragging = buttonEvent.Pressed;
						DragStartPos = buttonEvent.Position;
						break;
					case MouseButton.WheelUp:
						zoomFactor = 1.1f;
						break;
					case MouseButton.WheelDown:
						zoomFactor = 0.9f;
						break;
					case MouseButton.None:
					case MouseButton.Right:
					case MouseButton.Middle:
					case MouseButton.WheelLeft:
					case MouseButton.WheelRight:
					case MouseButton.Xbutton1:
					case MouseButton.Xbutton2:
					default:
						break;
				}

				if (zoomFactor is > 1f or < 1f)
				{
					var mouseContentPos = (buttonEvent.Position - PanWrapper.Position) / PanWrapper.Scale;
					PanWrapper.Scale *= zoomFactor;
					PanWrapper.Position = buttonEvent.Position - mouseContentPos * PanWrapper.Scale;
				}
				
				break;
			}
			case InputEventMouseMotion motionEvent when IsDragging:
				PanWrapper.Position += motionEvent.Relative;
				break;
		}
	}
}