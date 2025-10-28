using System.Collections.Generic;
using System.Linq;

namespace AlbionNavigator.Utils.ForceDirectedGraph.Internal.QuadTree;

public partial class QuadTree<T>
{
    public delegate bool VisitCallback(QuadTree<T> tree);
    
    public QuadTree<T> Visit(VisitCallback callback)
    {
        List<QuadTree<T>> trees = [];
        if (!Root.IsVoid) trees.Add(Root);
        
        while (true)
        {
            if (trees.Count == 0) break;
            var tree = trees.Last();
            trees.RemoveAt(trees.Count - 1);
            
            if (tree.IsVoid) continue;
            if (callback(tree)) continue;

            for (var childQuadrantIndex = 3; childQuadrantIndex >= 0; childQuadrantIndex--)
            {
                var child = tree.Children?[childQuadrantIndex];
                if (child == null) continue;
                trees.Add(child);
            }
        }

        return Root;
    }
}