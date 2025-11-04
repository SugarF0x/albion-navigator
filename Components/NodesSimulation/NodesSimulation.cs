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
	private const float PositionScale = 1.5f;
	
	[Export] public PackedScene NodeScene;
	[Export] public PackedScene LinkScene;
	
	public Zone[] Zones = ZoneService.Instance.Zones;
	public List<ZoneLink> Links = LinkService.Instance.Links;

	private Control NodesContainer;
	private Control LinksContainer;

	public List<NodeScene> NodeElements = [];
	public List<Line2D> LinkElements = [];

	private readonly Simulation Simulation = new ();
	
	public override void _Ready()
	{
		NodesContainer = GetNode<Control>("%NodesContainer");
		LinksContainer = GetNode<Control>("%LinksContainer");
		
		foreach (var zone in Zones)
		{
			if (NodeScene.Instantiate() is not NodeScene nodeScene) continue;
			NodesContainer.AddChild(nodeScene);
			NodeElements.Add(nodeScene);
			
			nodeScene.Value = zone;
		}

		foreach (var link in Links)
		{
			if (LinkScene.Instantiate() is not Line2D linkScene) continue;
			LinksContainer.AddChild(linkScene);
			LinkElements.Add(linkScene);
			
			var source = ZoneService.Instance.Zones[link.Source];
			var target = ZoneService.Instance.Zones[link.Target];
			linkScene.Points = [source.Position, target.Position];
		}

		InitSimulation();
		Simulation.StartAsync();
	}
	
	public override void _PhysicsProcess(double delta)
	{
		for (var i = 0; i < NodeElements.Count; i++)
		{
			var node = NodeElements[i];
			var simulationNode = Simulation.Nodes[i];
			node.Position = new Vector2(simulationNode.Position.X, simulationNode.Position.Y) * PositionScale;
		}
		
		for (var i = 0; i < LinkElements.Count; i++)
		{
			var link = Links[i];
			var linkElement = LinkElements[i];
			linkElement.Points =
			[
				NodeElements[link.Source].Position,
				NodeElements[link.Target].Position
			];
		}
	}

	private void InitSimulation()
	{
		Simulation.AddForce(new LinkForce
		{
			Links = Enumerable.Range(0, Links.Count).Select(i => new Link
			{
				Source = i,
				Target = Zones.Length - i - 1
			}).ToArray(),
			GetLinkDistance = _ => 15f
		});
		
		Simulation.AddForce(new ManyBodyForce
		{
			DistanceMaxSquared = 32f * 32f
		});
		
		Simulation.Nodes = Enumerable.Range(0, NodeElements.Count)
			.Select(i => new Node
			{
				IsFrozen = NodeElements[i].Position != Vector2.Zero,
				Position = new System.Numerics.Vector2(NodeElements[i].Position.X, NodeElements[i].Position.Y)
			})
			.ToArray();
	}
}