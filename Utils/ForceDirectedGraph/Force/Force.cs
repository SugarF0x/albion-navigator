using System;
using AlbionNavigator.Utils.ForceDirectedGraph.Datum;

namespace AlbionNavigator.Utils.ForceDirectedGraph.Force;

public abstract class Force
{
    protected Node[] Nodes = [];
    private Func<float> Random = () => new Random().NextSingle();
    
    public void Initialize(Node[] nodes, Func<float> random)
    {
        Nodes = nodes;
        Random = random;
        Setup();
    }
    
    protected abstract void Setup();
    public abstract void Apply(float alpha);
    
    protected float Jiggle => (Random() - .5f) * 1e-6f;
}