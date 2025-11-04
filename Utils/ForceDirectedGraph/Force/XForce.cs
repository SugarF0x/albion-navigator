using System;
using System.Linq;
using AlbionNavigator.Utils.ForceDirectedGraph.Datum;

namespace AlbionNavigator.Utils.ForceDirectedGraph.Force;

public class XForce : Force
{
    private Func<Node, float> _getDesiredNodeValue;
    public Func<Node, float> GetDesiredNodeValue
    {
        get => _getDesiredNodeValue;
        set
        {
            _getDesiredNodeValue = value;
            InitializeDesiredValues();
        }
    }
    
    private Func<Node, float> _getNodeStrength;
    public Func<Node, float> GetNodeStrength
    {
        get => _getNodeStrength;
        set
        {
            _getNodeStrength = value;
            InitializeStrengths();
        }
    }

    private float[] Strengths;
    private float[] DesiredPositions;

    public XForce()
    {
        AssignDefaultGetters();
    }

    private void AssignDefaultGetters()
    {
        GetNodeStrength = _ => .1f;
        GetDesiredNodeValue = _ => 0f;
    }

    protected override void Setup()
    {
        Strengths = new float[Nodes.Length];
        DesiredPositions = new float[Nodes.Length];
        InitializeStrengths();
        InitializeDesiredValues();
    }

    private void InitializeDesiredValues()
    {
        foreach (var node in Nodes) DesiredPositions[node.Index] = GetDesiredNodeValue(node);
    }

    private void InitializeStrengths()
    {
        foreach (var node in Nodes) Strengths[node.Index] = GetNodeStrength(node);
    }
    
    public override void Apply(float alpha)
    {
        foreach (var node in Nodes.Where(node => !node.IsFrozen))
            node.Velocity = node.Velocity with
            {
                X = node.Velocity.X + (DesiredPositions[node.Index] - node.Position.X) * Strengths[node.Index] * alpha
            };
    }
}