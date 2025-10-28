using System.Collections.Generic;
using System.Linq;

namespace AlbionNavigator.Utils.ForceDirectedGraph.Internal.QuadTree;

public partial class QuadTree<T>
{
    public delegate void VisitAfterCallback(QuadTree<T> tree);

    public QuadTree<T> VisitAfter(VisitAfterCallback callback)
    {
        List<QuadTree<T>> trees = [];
        List<QuadTree<T>> next = [];
        
        if (!Root.IsVoid) trees.Add(Root);
        
        while (true)
        {
            if (trees.Count == 0) break;
            var tree = trees.Last();
            trees.RemoveAt(trees.Count - 1);
            
            if (tree.IsVoid) continue;
            next.Add(tree);

            for (var childQuadrantIndex = 0; childQuadrantIndex <= 3; childQuadrantIndex++)
            {
                var child = tree.Children?[childQuadrantIndex];
                if (child == null || child.IsVoid) continue;
                trees.Add(child);
            }
        }

        next.Reverse();
        foreach (var tree in next) callback(tree);
        return Root;
    }
}