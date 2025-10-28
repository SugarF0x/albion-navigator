using System;

namespace AlbionNavigator.Utils.ForceDirectedGraph.Force;

public class Link : Force
{
    public int Iterations = 1;
    
    private int[] NodeIndexToConnectionsCountMap;
    private float[] LinkIndexToPullBiasMap;
    private float[] LinkIndexToStrengthMap;
    private float[] LinkIndexToDistanceMap;

    private Datum.Link[] _links = [];
    public Datum.Link[] Links
    {
        get => _links;
        set
        {
            _links = value;
            Setup();
        }
    }

    private Func<Datum.Link, float> _getLinkStrength;
    public Func<Datum.Link, float> GetLinkStrength
    {
        get => _getLinkStrength;
        set
        {
            _getLinkStrength = value;
            InitializeStrength();
        }
    }
    
    private Func<Datum.Link, float> _getLinkDistance;
    public Func<Datum.Link, float> GetLinkDistance
    {
        get =>  _getLinkDistance;
        set
        {
            _getLinkDistance = value;
            InitializeDistance();
        }
    }

    public Link()
    {
        AssignDefaultGetters();
    }

    private void AssignDefaultGetters()
    {
        GetLinkStrength = link => 1f / int.Min(NodeIndexToConnectionsCountMap[link.Source.Index], NodeIndexToConnectionsCountMap[link.Target.Index]);
        GetLinkDistance = _ => 30f;
    }

    protected override void Setup()
    {
        NodeIndexToConnectionsCountMap = new int[Nodes.Length];
        var count = NodeIndexToConnectionsCountMap;
        
        for (var i = 0; i < Links.Length; i++)
        {
            var link = Links[i];
            link.Index = i;
            count[link.Source.Index]++;
            count[link.Target.Index]++;
        }

        LinkIndexToPullBiasMap = new float[Links.Length];
        for (var i = 0; i < Links.Length; i++)
        {
            var link = Links[i];
            LinkIndexToPullBiasMap[i] = (float)count[link.Source.Index] / (count[link.Source.Index] + count[link.Target.Index]);
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
            foreach (var (source, target, index) in Links)
            {
                var springVelocity = target.Position + target.Velocity - source.Position - source.Velocity;
                if (springVelocity.X == 0f) springVelocity.X = Jiggle;
                if (springVelocity.Y == 0f) springVelocity.Y = Jiggle;

                var length = float.Sqrt(springVelocity.X * springVelocity.X + springVelocity.Y * springVelocity.Y);
                var adjustedLengthMultiplier = (length - LinkIndexToDistanceMap[index]) / length * alpha * LinkIndexToStrengthMap[index];
                springVelocity *= adjustedLengthMultiplier;
            
                target.Velocity -= springVelocity * LinkIndexToPullBiasMap[index];
                source.Velocity += springVelocity * (1f - LinkIndexToPullBiasMap[index]);
            }
        }
    }
}