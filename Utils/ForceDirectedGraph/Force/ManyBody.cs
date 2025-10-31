using System;
using System.Collections.Generic;
using System.Linq;
using AlbionNavigator.Utils.ForceDirectedGraph.Internal.QuadTree;
using Godot;
using Node = AlbionNavigator.Utils.ForceDirectedGraph.Datum.Node;
using Vector2 = System.Numerics.Vector2;

namespace AlbionNavigator.Utils.ForceDirectedGraph.Force;

public class ManyBody : Force
{
    public float DistanceMinSquared = 1f;
    public float DistanceMaxSquared = float.MaxValue;
    public float ThetaSquared = 0.81f;

    private float Alpha;

    private float[] Strengths;
    private readonly Dictionary<QuadTree<Node>, NodeChargeAccumulator> TreeToAccumulatorMap = new();
    
    private Func<Node, float> _getNodeStrength;
    public Func<Node, float> GetNodeStrength
    {
        get => _getNodeStrength;
        set
        {
            _getNodeStrength = value;
            InitializeStrength();
        }
    }
    
    public ManyBody()
    {
        AssignDefaultGetters();
    }

    private void AssignDefaultGetters()
    {
        GetNodeStrength = _ => -30f;
    }

    protected override void Setup()
    {
        Strengths = new float[Nodes.Length];
        InitializeStrength();
    }

    private void InitializeStrength()
    {
        foreach (var node in Nodes) Strengths[node.Index] = GetNodeStrength(node);
    }

    public override void Apply(float alpha)
    {
        Alpha = alpha;
        TreeToAccumulatorMap.Clear();
        var quadTree = new QuadTree<Node>().Add(Nodes.Select(node => (node.Position, node)).ToArray()).VisitAfter(AccumulateForce);
        foreach (var node in Nodes.Where(node => !node.IsFrozen)) quadTree.Visit(tree => ApplyAccumulatedForce(tree, node));
    }

    private void AccumulateForce(QuadTree<Node> tree)
    {
        var strength = 0f;
        var weight = 0f;
        
        var treeAccumulator = new NodeChargeAccumulator();

        if (tree.IsLeaf)
        {
            treeAccumulator.CenterOfMass = tree.Leaf.Position;
            strength += tree.Leaf.Data.Sum(node => Strengths[node.Index]);
        }
        else
        {
            var centerOfMass = Vector2.Zero;
            
            foreach (var branch in tree.Children.Where(child => child != null))
            {
                var branchAccumulator = TreeToAccumulatorMap[branch];
                var charge = float.Abs(branchAccumulator.Charge);
                if (charge == 0f) continue;

                strength += branchAccumulator.Charge;
                weight += charge;
                centerOfMass += branchAccumulator.CenterOfMass * charge;
            }

            treeAccumulator.CenterOfMass = centerOfMass / weight;
        }

        treeAccumulator.Charge = strength;
        TreeToAccumulatorMap[tree] = treeAccumulator;
    }

    private bool ApplyAccumulatedForce(QuadTree<Node> tree, Node node)
    {
        var accumulator = TreeToAccumulatorMap[tree];
        if (accumulator.Charge == 0f) return true;

        var attractionDirection = accumulator.CenterOfMass - node.Position;
        var quadWidth = tree.Rect.Size.X;
        var distanceToCenterOfMassSquared = Vector2.DistanceSquared(node.Position, accumulator.CenterOfMass);

        if (quadWidth * quadWidth / ThetaSquared < distanceToCenterOfMassSquared)
        {
            if (!(distanceToCenterOfMassSquared < DistanceMaxSquared)) return true;
            
            if (attractionDirection.X == 0f)
            {
                attractionDirection.X = Jiggle;
                distanceToCenterOfMassSquared += float.Pow(attractionDirection.X, 2f);
            }

            if (attractionDirection.Y == 0f)
            {
                attractionDirection.Y = Jiggle;
                distanceToCenterOfMassSquared += float.Pow(attractionDirection.Y, 2f);
            }

            if (distanceToCenterOfMassSquared < DistanceMinSquared) distanceToCenterOfMassSquared = float.Sqrt(DistanceMinSquared * distanceToCenterOfMassSquared);

            node.Velocity += attractionDirection * accumulator.Charge * Alpha / distanceToCenterOfMassSquared;

            return true;
        }

        if (tree.IsBranch || distanceToCenterOfMassSquared >= DistanceMaxSquared) return false;

        if (tree.Leaf.Data.First() != node || tree.Leaf.Data.Count > 1)
        {
            if (attractionDirection.X == 0f)
            {
                attractionDirection.X = Jiggle;
                distanceToCenterOfMassSquared += float.Pow(attractionDirection.X, 2f);
            }

            if (attractionDirection.Y == 0f)
            {
                attractionDirection.Y = Jiggle;
                distanceToCenterOfMassSquared += float.Pow(attractionDirection.Y, 2f);
            }
            
            if (distanceToCenterOfMassSquared < DistanceMinSquared) distanceToCenterOfMassSquared = float.Sqrt(DistanceMinSquared * distanceToCenterOfMassSquared);
        }

        foreach (var _ in tree.Leaf.Data.Where(leaf => leaf != node))
        {
            quadWidth = Strengths[node.Index] * Alpha / distanceToCenterOfMassSquared;
            node.Velocity += attractionDirection * quadWidth;
        }

        return false;
    }
}

public struct NodeChargeAccumulator(float charge, Vector2 centerOfMass)
{
    public float Charge = charge;
    public Vector2 CenterOfMass = centerOfMass;
};
