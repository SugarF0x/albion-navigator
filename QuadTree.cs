using System.Collections.Generic;
using System.Linq;
using Godot;

namespace AlbionNavigator;

public class QuadTree
{
    public Rect2 Rect { get; set; }
    public QuadTreeNode Root { get; set; } = new();

    public QuadTree Cover(Vector2 point)
    {
        if (!Rect.HasArea())
        {
            var modifiedRect = Rect;
            modifiedRect.Position = new Vector2(float.Floor(point.X), float.Floor(point.Y));
            modifiedRect.Size = new Vector2(1f, 1f);
            Rect = modifiedRect;
            return this;
        }

        while (!Rect.HasPoint(point))
        {
            var quadrant = Quad.GetRelativeQuadrant(point, Rect.Position);
            Rect = Quad.ExpandRectFromQuadrant(Rect, quadrant);
            if (Root.IsVoid) continue;

            var parent = new QuadTreeNode();
            parent.CreateEmptyBranches();
            parent.Branches[(int)quadrant] = Root;
            Root = parent;
        }

        return this;
    }

    public QuadTree Add(Vector2 node, bool shouldCover = true)
    {
        if (shouldCover) Cover(node);

        if (Root.IsVoid)
        {
            Root.AttachLeaf(node);
            return this;
        }

        var exploredNode = Root;
        var exploredRect = Rect;

        while (exploredNode.IsBranch)
        {
            var midPoint = exploredRect.GetCenter();
            var exploredNodeQuadrant = Quad.GetRelativeQuadrant(midPoint, node);
            exploredRect = Quad.ShrinkRectToQuadrant(exploredRect, exploredNodeQuadrant);
            exploredNode = exploredNode.Branches[(int)exploredNodeQuadrant];
        }

        if (exploredNode.IsVoid)
        {
            exploredNode.AttachLeaf(node);
            return this;
        }

        while (true)
        {
            var midPoint = exploredRect.GetCenter();
            var oldNodeQuadrant = Quad.GetRelativeQuadrant(midPoint, exploredNode.Leaves.First());
            var newNodeQuadrant = Quad.GetRelativeQuadrant(midPoint, node);
            
            exploredNode.BranchOut(oldNodeQuadrant);
            
            if (oldNodeQuadrant != newNodeQuadrant)
            {
                exploredNode.Branches[(int)newNodeQuadrant].AttachLeaf(node);
                break;
            }
            
            exploredNode = exploredNode.Branches[(int)oldNodeQuadrant];
            exploredRect = Quad.ShrinkRectToQuadrant(exploredRect, newNodeQuadrant);
        }
        
        return this;
    }

    public QuadTree AddAll(Vector2[] nodes)
    {
        switch (nodes.Length)
        {
            case 0: return this;
            case 1: return Add(nodes.First());
        }

        var boundingBox = new Rect2(nodes.First(), Vector2.Zero);
        for (var i = 1; i < nodes.Length; i++)
        {
            boundingBox = boundingBox.Merge(new Rect2(nodes[i], Vector2.Zero));
        }

        Cover(boundingBox.Position).Cover(boundingBox.End);
        foreach (var node in nodes) Add(node, false);

        return this;
    }
    
    public delegate void VisitAfterCallback(Quad quad);
    
    public QuadTree VisitAfter(VisitAfterCallback callback)
    {
        List<Quad> quads = [];
        List<Quad> next = [];
        
        if (!Root.IsVoid) quads.Add(new Quad(Root, Rect));
        
        while (true)
        {
            if (quads.Count == 0) break;
            var quad = quads.Last();
            quads.RemoveAt(quads.Count - 1);
            
            if (quad.Node.IsVoid) continue;
            next.Add(quad);

            for (var branchQuadrantIndex = 0; branchQuadrantIndex < Quad.QuadCount; branchQuadrantIndex++)
            {
                var branch = quad.Node.Branches[branchQuadrantIndex];
                if (branch.IsVoid) continue;
                quads.Add(new Quad(branch, Quad.ShrinkRectToQuadrant(quad.Rect, (Quad.Quadrant)branchQuadrantIndex)));
            }
        }

        next.Reverse();
        foreach (var quad in next) callback(quad);
        return this;
    }
    
    public delegate bool VisitCallback(Quad quad);
    
    /**
     * Traverses top-down, stops when callback returns true
     */
    public QuadTree Visit(VisitCallback callback)
    {
        List<Quad> quads = [];
        
        if (Root.IsVoid) quads.Add(new Quad(Root, Rect));

        while (true)
        {
            if (quads.Count == 0) break;
            var quad = quads.Last();
            quads.RemoveAt(quads.Count - 1);
            
            if (quad.Node.IsVoid) continue;
            if (callback(quad)) continue;

            for (var branchQuadrantIndex = Quad.QuadCount - 1; branchQuadrantIndex >= 0; branchQuadrantIndex--)
            {
                var branch = quad.Node.Branches[branchQuadrantIndex];
                if (branch.IsVoid) continue;
                quads.Add(new Quad(branch, Quad.ShrinkRectToQuadrant(quad.Rect, (Quad.Quadrant)branchQuadrantIndex)));
            }
        }

        return this;
    }
}