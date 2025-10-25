using System.Numerics;
using GraphNode = AlbionNavigator.Utils.ForceDirectedGraph.Datum.Node;

namespace AlbionNavigator.Utils.ForceDirectedGraph;

public partial class Simulation
{
    private GraphNode[] _nodes = [];
    public GraphNode[] Nodes
    {
        get => _nodes;
        set
        {
            _nodes = value;
            InitializeNodes();
            InitializeAllForces();
        }
    }

    private void InitializeNodes()
    {
        for (var i = 0; i < Nodes.Length; i++)
        {
            var node = Nodes[i];
            node.Index = i;
            if (node.Position != Vector2.Zero) continue;
            
            var radius = InitialRadius * float.Sqrt(.5f + i);
            var angle = InitialAngle * i;
            node.Position.X = radius * float.Cos(angle);
            node.Position.Y = radius * float.Sin(angle);
        }
    }
}