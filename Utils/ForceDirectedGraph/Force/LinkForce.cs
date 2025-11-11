using System;
using AlbionNavigator.Utils.ForceDirectedGraph.Datum;

namespace AlbionNavigator.Utils.ForceDirectedGraph.Force;

public class LinkForce : Force
{
    public int Iterations = 1;
    
    private int[] NodeIndexToConnectionsCountMap;
    private float[] LinkIndexToPullBiasMap;
    private float[] LinkIndexToStrengthMap;
    private float[] LinkIndexToDistanceMap;

    private Link[] _links = [];
    public Link[] Links
    {
        get => _links;
        set
        {
            _links = value;
            Setup();
        }
    }

    private Func<Link, float> _getLinkStrength;
    public Func<Link, float> GetLinkStrength
    {
        get => _getLinkStrength;
        set
        {
            _getLinkStrength = value;
            InitializeStrength();
        }
    }
    
    private Func<Link, float> _getLinkDistance;
    public Func<Link, float> GetLinkDistance
    {
        get =>  _getLinkDistance;
        set
        {
            _getLinkDistance = value;
            InitializeDistance();
        }
    }

    public LinkForce()
    {
        AssignDefaultGetters();
    }

    private void AssignDefaultGetters()
    {
        GetLinkStrength = link => 1f / int.Min(NodeIndexToConnectionsCountMap[link.Source], NodeIndexToConnectionsCountMap[link.Target]);
        GetLinkDistance = _ => 30f;
    }

    protected override void Setup()
    {
        NodeIndexToConnectionsCountMap = new int[Nodes.Length];
        if (Nodes.Length == 0) return;

        var count = NodeIndexToConnectionsCountMap;
        for (var i = 0; i < Links.Length; i++)
        {
            var link = Links[i];
            link.Index = i;
            Links[i] = link;
            count[link.Source]++;
            count[link.Target]++;
        }

        LinkIndexToPullBiasMap = new float[Links.Length];
        for (var i = 0; i < Links.Length; i++)
        {
            var link = Links[i];
            LinkIndexToPullBiasMap[i] = (float)count[link.Source] / (count[link.Source] + count[link.Target]);
        }

        InitializeStrength();
        InitializeDistance();
    }

    private void InitializeStrength()
    {
        LinkIndexToStrengthMap = new float[Links.Length];
        foreach (var link in Links) LinkIndexToStrengthMap[link.Index] = GetLinkStrength(link);
    }

    private void InitializeDistance()
    {
        LinkIndexToDistanceMap = new float[Links.Length];
        foreach (var link in Links) LinkIndexToDistanceMap[link.Index] = GetLinkDistance(link);
    }

    public override void Apply(float alpha)
    {
        for (var k = 0; k < Iterations; k++)
        {
            foreach (var (sourceIndex, targetIndex, linkIndex) in Links)
            {
                // due to async nature sometimes mismatch happens on bulk addition, should not happen in real environment really
                if (linkIndex >= LinkIndexToDistanceMap.Length) continue;
                
                var source = Nodes[sourceIndex];
                var target = Nodes[targetIndex];
                
                var springVelocity = target.Position + target.Velocity - source.Position - source.Velocity;
                if (springVelocity.X == 0f) springVelocity.X = Jiggle;
                if (springVelocity.Y == 0f) springVelocity.Y = Jiggle;

                var length = float.Sqrt(springVelocity.X * springVelocity.X + springVelocity.Y * springVelocity.Y);
                var adjustedLengthMultiplier = (length - LinkIndexToDistanceMap[linkIndex]) / length * alpha * LinkIndexToStrengthMap[linkIndex];
                springVelocity *= adjustedLengthMultiplier;
            
                target.Velocity -= springVelocity * LinkIndexToPullBiasMap[linkIndex];
                source.Velocity += springVelocity * (1f - LinkIndexToPullBiasMap[linkIndex]);
            }
        }
    }
}