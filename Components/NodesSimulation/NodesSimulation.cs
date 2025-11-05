using System;
using System.Collections.Generic;
using System.Linq;
using AlbionNavigator.Resources;
using AlbionNavigator.Services;
using AlbionNavigator.Utils.ForceDirectedGraph;
using AlbionNavigator.Utils.ForceDirectedGraph.Datum;
using AlbionNavigator.Utils.ForceDirectedGraph.Force;
using Godot;
using Node = AlbionNavigator.Utils.ForceDirectedGraph.Datum.Node;

namespace AlbionNavigator.Components.NodesSimulation;

public partial class NodesSimulation : Control
{
	[Export] public PackedScene NodeScene;
	[Export] public PackedScene LinkScene;
	[Export] public float PositionScale = 1f;
	[Export] public float AlphaReheatValue = .5f;
	
	private Zone[] _zones = [];
	public Zone[] Zones
	{
		get => _zones;
		set
		{
			_zones = value;
			InitializeZones();
		}
	}
	
	private List<ZoneLink> _links = [];
	public List<ZoneLink> Links
	{
		get => _links;
		set
		{
			_links = value;
			InitializeLinks();
		}
	}

	private Control NodesContainer;
	private Control LinksContainer;

	private List<NodeScene> NodeElements = [];
	private List<LinkScene> LinkElements = [];

	private readonly Simulation Simulation = new ()
	{
		InitialRadius = 50
	};
	
	public override void _Ready()
	{
		NodesContainer = GetNode<Control>("%NodesContainer");
		LinksContainer = GetNode<Control>("%LinksContainer");
		
		InitializeZones();
		InitializeLinks();
		InitializeSimulation();
	}
	
	public override void _PhysicsProcess(double delta)
	{
		if (!Simulation.IsSimulationRunning) return;
		
		for (var i = 0; i < NodeElements.Count; i++)
		{
			var node = NodeElements[i];
			var simulationNode = Simulation.Nodes[i];
			node.Position = new Vector2(simulationNode.Position.X, simulationNode.Position.Y) * PositionScale;
		}

		for (var i = 0; i < LinkElements.Count; i++)
		{
			try
			{
				var link = Links[i];
				var linkElement = LinkElements[i];
				linkElement.Points =
				[
					NodeElements[link.Source].Position,
					NodeElements[link.Target].Position
				];
			}
			catch
			{
				// sometimes links or link elements get updated mid for loop resulting in accessing index outside bounds
				// i only ever managed to see this once accidentally and never reproduce again
				// this does not impact negatively in any way really and since i have no good clue on how to fix it
				// might as well just suppress it for now
			}
		}
	}

	public void StartSimulation()
	{
		Simulation.Alpha = float.Max(Simulation.Alpha, AlphaReheatValue);
		Simulation.StartAsync();
	}
	
	private void InitializeZones()
	{
		foreach (var child in NodesContainer.GetChildren()) child.QueueFree();
		NodeElements.Clear();
		
		foreach (var zone in Zones)
		{
			if (NodeScene.Instantiate() is not NodeScene nodeScene) continue;
			NodesContainer.AddChild(nodeScene);
			NodeElements.Add(nodeScene);
			
			nodeScene.Value = zone;
		}

		InitializeSimulationNodes();
	}

	private void InitializeLinks()
	{
		foreach (var child in LinksContainer.GetChildren()) child.QueueFree();
		LinkElements.Clear();

		foreach (var link in Links) AddLink(link);
		InitializeSimulationLinks();
	}

	public void AddLink(ZoneLink link, int index = -1)
	{
		if (LinkScene.Instantiate() is not LinkScene linkScene) return;
		LinksContainer.AddChild(linkScene);
		if (index >=  0) LinksContainer.MoveChild(linkScene, index);
		LinkElements.Insert(index < 0 ? LinkElements.Count : index, linkScene);
			
		var source = ZoneService.Instance.Zones[link.Source];
		var target = ZoneService.Instance.Zones[link.Target];
		linkScene.Points = [source.Position, target.Position];
		linkScene.Link = link;

		if (index >= 0) InitializeSimulationLinks();
	}

	public void RemoveLink(ZoneLink _, int index)
	{
		var child = LinksContainer.GetChild(index);
		child.QueueFree();
		LinkElements.RemoveAt(index);
		InitializeSimulationLinks();
	}

	private readonly LinkForce LinkForce = new ();
	private readonly ManyBodyForce ManyBodyForce = new ();
	
	private void InitializeSimulation()
	{
		LinkForce.GetLinkStrength = link =>
			new List<Zone.ZoneType> { Zones[link.Source].Type, Zones[link.Target].Type }.Count(type =>
					type == Zone.ZoneType.Road) switch
				{
					1 => 0,
					2 => .5f,
					_ => 1
				};

		ManyBodyForce.GetNodeStrength = _ => -40;
		
		Simulation.AddForce(LinkForce);
		Simulation.AddForce(ManyBodyForce);
		Simulation.AddForce(new XForce());
		Simulation.AddForce(new YForce());

		InitializeSimulationNodes();
		InitializeSimulationLinks();
	}

	private void InitializeSimulationNodes()
	{
		Simulation.Nodes = Enumerable.Range(0, NodeElements.Count)
			.Select(i => new Node
			{
				IsFrozen = NodeElements[i].Position != Vector2.Zero,
				Position = new System.Numerics.Vector2(NodeElements[i].Position.X, NodeElements[i].Position.Y)
			})
			.ToArray();
	}

	private void InitializeSimulationLinks()
	{
		LinkForce.Links = Links.Select(link => new Link
		{
			Source = link.Source,
			Target = link.Target
		}).ToArray();
	}

	public void MoveLinkElementByIndex(int from, int to)
	{
		LinksContainer.MoveChild(LinksContainer.GetChild(from), to);
		var element = LinkElements[from];
		LinkElements.RemoveAt(from);
		LinkElements.Insert(to, element);
	}
}