using System.Collections.Generic;

namespace AlbionNavigator.Utils.ForceDirectedGraph;

public partial class Simulation
{
    private readonly List<Force.Force> Forces = [];

    private void InitializeForce(Force.Force force)
    {
        force.Initialize(Nodes, Jiggle);
    }

    private void InitializeAllForces()
    {
        foreach (var force in Forces) InitializeForce(force);
    }

    public void AddForce(Force.Force force)
    {
        Forces.Add(force);
        InitializeForce(force);
    }
}