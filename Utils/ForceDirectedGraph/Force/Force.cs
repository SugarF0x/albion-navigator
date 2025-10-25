using System;
using AlbionNavigator.Utils.ForceDirectedGraph.Datum;

namespace AlbionNavigator.Utils.ForceDirectedGraph.Force;

public abstract class Force
{
    public abstract void Initialize(Node[] nodes, Func<float> random);
    public abstract void Apply(float alpha);
}