using AlbionNavigator.Utils.ForceDirectedGraph.Internal;

namespace AlbionNavigator.Utils.ForceDirectedGraph;

public partial class Simulation
{
    private readonly LinearCongruentialGenerator LinearCongruentialGenerator = new ();
    private float Random() => LinearCongruentialGenerator.Next();
    private float Jiggle() => (Random() - .5f) * 1e-6f;
}