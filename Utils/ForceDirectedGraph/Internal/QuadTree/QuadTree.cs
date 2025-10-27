using System.Collections.Generic;
using System.Numerics;
using AlbionNavigator.Utils.ForceDirectedGraph.Internal.QuadTree.Internal;

namespace AlbionNavigator.Utils.ForceDirectedGraph.Internal.QuadTree;

public partial class QuadTree<T>
{
    private Rect Rect;
    
    private QuadTree<T> Parent;
    public QuadTree<T>[] Children;
    public Leaf<T> Leaf;

    public bool IsBranch => Children != null;
    public bool IsLeaf => !IsBranch && Leaf != null;
    public bool IsVoid => !IsBranch && !IsLeaf;
}