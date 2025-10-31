using AlbionNavigator.Services;
using AlbionNavigator.Utils.ForceDirectedGraph;
using AlbionNavigator.Utils.ForceDirectedGraph.Force;
using Godot;

public partial class FdgTest : Node2D
{
	public Simulation Simulation;
	private const int NodesCount = 800;
	private const int PositionScale = 2;
	private Node2D[] Nodes = new Node2D[NodesCount];
	private Vector2 Midpoint;
	
	public override void _Ready()
	{
		Midpoint = DisplayServer.WindowGetSize() / 2;
		InitSimulation();
		var texture = GD.Load<Texture2D>("res://Assets/icon.png");
		for (var i = 0; i < NodesCount; i++)
		{
			var node = new Node2D();
			AddChild(node);
			var image = new Sprite2D();
			image.Texture = texture;
			image.Scale = new Vector2(0.05f, 0.05f);
			node.AddChild(image);
			Nodes[i] = node;
		}
		Simulation.StartAsync();
	}

	public override void _PhysicsProcess(double delta)
	{
		for (var i = 0; i < Nodes.Length; i++)
		{
			var node = Nodes[i];
			var simulationNode = Simulation.Nodes[i];
			node.Position = new Vector2(simulationNode.Position.X, simulationNode.Position.Y) * PositionScale + Midpoint;
		}
	}

	public void InitSimulation()
	{
		Simulation = new Simulation();
		
		Simulation.OnSimulationStarted += () => GD.Print("Simulation started");
		Simulation.OnSimulationFinished += () => GD.Print("Simulation finished");
		
		// Simulation.AddForce(new Link());
		Simulation.AddForce(new ManyBody
		{
			DistanceMaxSquared = 32f * 32f
		});
		
		Simulation.SetSize(NodesCount);
	}
}
