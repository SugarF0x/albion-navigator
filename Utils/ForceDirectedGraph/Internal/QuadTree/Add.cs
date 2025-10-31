using System.Linq;
using System.Numerics;
using AlbionNavigator.Utils.ForceDirectedGraph.Internal.QuadTree.Internal;

namespace AlbionNavigator.Utils.ForceDirectedGraph.Internal.QuadTree;

public partial class QuadTree<T>
{
    public QuadTree<T> Add(Vector2 point, T data, bool cover = true)
    {
        if (Parent != null) return Parent.Add(point, data, cover);
        
        if (cover)
        {
            var coverTree = Cover(point);
            if (coverTree != this) return coverTree.Add(point, data);
        }

        if (IsVoid)
        {
            Leaf = new Leaf<T>
            {
                Position = point,
                Data = [data]
            };
            return this;
        }

        var tree = this;
        while (tree.IsBranch)
        {
            var nextQuadrantIndex = tree.Rect.Center.GetRelativeQuadrantIndex(point);
            var nextNode = tree.Children[nextQuadrantIndex] ??= new QuadTree<T>
            {
                Rect = tree.Rect.ShrinkToQuadrantIndex(nextQuadrantIndex)
            };
            
            tree = nextNode;
        }

        if (tree.IsVoid || (tree.IsLeaf && tree.Leaf.Position == point))
        {
            tree.Leaf ??= new Leaf<T> { Position = point, Data = [] };
            tree.Leaf.Data.Add(data);
            return this;
        }

        while (true)
        {
            var midPoint = tree.Rect.Center;
            var oldNodeQuadrant = midPoint.GetRelativeQuadrantIndex(tree.Leaf.Position);
            var newNodeQuadrant = midPoint.GetRelativeQuadrantIndex(point);

            tree.Subdivide(oldNodeQuadrant);
            
            if (oldNodeQuadrant != newNodeQuadrant)
            {
                tree.Children[newNodeQuadrant] = new QuadTree<T>
                {
                    Rect = tree.Rect.ShrinkToQuadrantIndex(newNodeQuadrant),
                    Leaf = new Leaf<T>
                    {
                        Position = point,
                        Data = [data]
                    }
                };
                break;
            }
            
            tree = tree.Children[oldNodeQuadrant];
        }
        
        return this;
    }

    public QuadTree<T> Add((Vector2 point, T data)[] items)
    {
        if (Parent != null) return Parent.Add(items);
        
        switch (items.Length)
        {
            case 0: return this;
            case 1: return Add(items.First().point, items.First().data);
        }
        
        var boundingBox = new Rect(items.First().point, Vector2.Zero);
        boundingBox = items.Skip(1).Aggregate(boundingBox, (current, node) => current.Merge(new Rect(node.point, Vector2.Zero)));

        Cover(boundingBox.Position).Cover(boundingBox.End);
        foreach (var (point, data) in items) Add(point, data, false);
        
        return this;
    }
}