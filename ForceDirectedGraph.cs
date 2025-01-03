﻿using System;
using System.Linq;
using Godot;
using AlbionNavigator.Data;

namespace AlbionNavigator;

[GlobalClass]
public partial class ForceDirectedGraph : Node2D
{
    [ExportGroup("Packed Scenes")] 
    [Export] public PackedScene NodeScene;
    [Export] public PackedScene LinkScene;
    
    [ExportGroup("Graph Heat")] 
    [Export] public float Alpha = 1f;
    [Export] public float AlphaMin = 0.0001f;
    [Export] public float AlphaTarget;
    [Export] public float AlphaDecay = 0.0302f;
    [Export] public float AlphaReheat = 0.5f;
    [Export] public bool ReheatOnNodesAdded = true;

    [ExportGroup("Many body simulation")]
    [Export] public float ThetaSquared = 0.91f;
    [Export] public float DistanceMinSquared = 1f;
    [Export] public float DistanceMaxSquared = float.PositiveInfinity;

    [ExportGroup("Gravitational pull")]
    [Export] public float GravityStrength = 0.1f;
    [Export] public float CentralStrength = 1f;
    [Export] public float GravityDesiredDistance = 200f;

    [ExportGroup("Debug")] 
    [Export] public bool DrawQuadTree;
    [Export] public bool DrawCenterOfMass;

    private RandomNumberGenerator _random = new ();
    private Node2D _linksContainer;
    private Node2D _nodesContainer;

    public ForceGraphNode[] Nodes = [];
    public ForceGraphLink[] Links = [];

    public override void _Ready()
    {
        if (NodeScene?.Instantiate() is not ForceGraphNode) throw new InvalidCastException("NodeScene is not a ForceGraphNode");
        if (LinkScene?.Instantiate() is not ForceGraphLink) throw new InvalidCastException("LinkScene is not a ForceGraphLink");
        
        _random.Seed = "peepee-poopoo".Hash();
        _linksContainer = GetNode<Node2D>("%LinksContainer");
        _nodesContainer = GetNode<Node2D>("%NodesContainer");

        PopulateZones();
        ConnectChildListeners();
    }

    public override void _Process(double _)
    {
        if (Alpha >= AlphaMin) Step();
    }
    

    public void AddNode(ForceGraphNode node) => _nodesContainer.AddChild(node);
    public void AddLink(ForceGraphLink link) => _linksContainer.AddChild(link);

    private void PopulateZones()
    {
        var zones = Zone.LoadZoneBinaries();

        // TODO: replace with some proper zone and link nodes later on
        for (var i = 0; i < zones.Length; i++)
        {
            var zone = zones[i];
            if (NodeScene.Instantiate() is not ForceGraphNode node) continue;
            
            node.Position = zone.Position;
            node.Index = zone.Id;
            if (node.Position != Vector2.Zero) node.Frozen = true;
            AddNode(node);

            foreach (var connection in zone.Connections.Where(index => index > i))
            {
                if (LinkScene.Instantiate() is not ForceGraphLink link) continue;
                link.Connect(i, connection);
                AddLink(link);
            }
        }
    }
    
    private bool _shouldRegisterChildren = true;

    private void OnChildrenChanged(Node _)
    {
        _shouldRegisterChildren = true;
        if (ReheatOnNodesAdded) Reheat();
    }

    private void ConnectChildListeners()
    {
        _nodesContainer.ChildEnteredTree += OnChildrenChanged;
        _nodesContainer.ChildExitingTree += OnChildrenChanged;
        _linksContainer.ChildEnteredTree += OnChildrenChanged;
        _linksContainer.ChildExitingTree += OnChildrenChanged;
    }

    private void RegisterChildren()
    {
        if (!_shouldRegisterChildren) return;
        _shouldRegisterChildren = false;

        Nodes = _nodesContainer.GetChildren().Select(child => child as ForceGraphNode).ToArray();
        Links = _linksContainer.GetChildren().Select(child => child as ForceGraphLink).ToArray();

        InitializeChildren();
    }

    private void InitializeChildren()
    {
        for (var i = 0; i < Nodes.Length; i++) Nodes[i].Initialize(i);
        foreach (var link in Links) link.Initialize(Nodes);
    }

    public void Reheat()
    {
        Alpha = AlphaReheat;
    }
    
    public void Reheat(float value)
    {
        Alpha = float.Max(Alpha, value);
    }

    private void Step()
    {
        RegisterChildren();
        Alpha += (AlphaTarget - Alpha) * AlphaDecay;

        ApplyForces();
        
        foreach (var node in Nodes) node.UpdatePosition();
        foreach (var link in Links) link.DrawLink(Nodes);
    }

    private void ApplyForces()
    {
        ApplyForceCenter();
        ApplyForceLink();
        ApplyManyBodyForce();
        ApplyGravityForce();
    }
    
    private void ApplyForceCenter()
    {
        var center = Vector2.Zero;
        var shift = Nodes.Aggregate(Vector2.Zero, (current, node) => current + node.Position);
        shift = shift / Nodes.Length - center * CentralStrength;
        foreach (var node in Nodes) if (!node.Frozen) node.Position -= shift;
    }
    
    private void ApplyForceLink() 
    {
        foreach (var link in Links)
        {
            var sourceNode = Nodes[link.Source];
            var targetNode = Nodes[link.Target];

            var springVelocity = targetNode.Position + targetNode.Velocity - sourceNode.Position - sourceNode.Velocity;
            if (springVelocity.X == 0f) springVelocity.X = Jiggle;
            if (springVelocity.Y == 0f) springVelocity.Y = Jiggle;

            var length = targetNode.Position.DistanceTo(sourceNode.Position);
            var adjustedLengthMultiplier = (length - link.DesiredDistance) / length * Alpha * link.Strength;
            springVelocity *= adjustedLengthMultiplier;
            
            targetNode.Velocity -= springVelocity * link.Bias;
            sourceNode.Velocity += springVelocity * (1f - link.Bias);
        }
    }

    private QuadTree _tree;
    
    private void ApplyGravityForce()
    {
        foreach (var node in Nodes) node.Velocity -= node.Position * GravityStrength * Alpha * (node.Position.DistanceTo(Vector2.Zero) > GravityDesiredDistance ? 1f : -1f);
    }
    
    private void ApplyManyBodyForce()
    {
        _tree = new QuadTree().AddAll(Nodes).VisitAfter(Accumulate);
        foreach (var node in Nodes) _tree.Visit(quad => Apply(quad, node));
        QueueRedraw();
    }

    private void Accumulate(Quad quad)
    {
        var strength = 0f;
        var weight = 0f;

        if (quad.Node.IsBranch)
        {
            var centerOfMass = Vector2.Zero;
            
            foreach (var branch in quad.Node.Branches)
            {
                var charge = float.Abs(branch.Charge);
                if (charge == 0f) continue;

                strength += branch.Charge;
                weight += charge;
                centerOfMass += branch.CenterOfMass * charge;
            }

            quad.Node.CenterOfMass = centerOfMass / weight;
        }
        else
        {
            quad.Node.CenterOfMass = quad.Node.Leaves.First().Position;
            strength += quad.Node.Leaves.Sum(leaf => leaf.Strength);
        }

        quad.Node.Charge = strength;
    }

    private bool Apply(Quad quad, ForceGraphNode node)
    {
        if (quad.Node.Charge == 0f) return true;

        var attractionDirection = quad.Node.CenterOfMass - node.Position;
        var quadWidth = quad.Rect.Size.X;
        var distanceToCenterOfMassSquared = node.Position.DistanceSquaredTo(quad.Node.CenterOfMass);

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

            node.Velocity += attractionDirection * quad.Node.Charge * Alpha / distanceToCenterOfMassSquared;

            return true;
        }

        if (quad.Node.IsBranch || distanceToCenterOfMassSquared >= DistanceMaxSquared) return false;

        if (quad.Node.Leaves[0] != node || quad.Node.Leaves.Count > 1)
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

        foreach (var _ in quad.Node.Leaves.Where(leaf => leaf != node))
        {
            quadWidth = node.Strength * Alpha / distanceToCenterOfMassSquared;
            node.Velocity += attractionDirection * quadWidth;
        }

        return false;
    }

    private float Jiggle => (_random.Randf() - 0.5f) * 1e-6f;

    public override void _Draw()
    {
        if (!DrawQuadTree && !DrawCenterOfMass) return;
        _tree?.Visit(quad =>
        {
            if (DrawQuadTree) DrawRect(quad.Rect, Colors.Red, false);
            if (DrawCenterOfMass) DrawCircle(quad.Node.CenterOfMass, 5f, Colors.Green);
            return false;
        });
    }
}