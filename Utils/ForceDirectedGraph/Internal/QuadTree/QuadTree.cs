using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AlbionNavigator.Utils.ForceDirectedGraph.Internal.QuadTree.Internal;

namespace AlbionNavigator.Utils.ForceDirectedGraph.Internal.QuadTree;

public partial class QuadTree<T>
{
    public Rect Rect;
    
    private QuadTree<T> Parent;
    public QuadTree<T>[] Children;
    public Leaf<T> Leaf;

    public bool IsBranch => Children != null;
    public bool IsLeaf => !IsBranch && Leaf != null;
    public bool IsVoid => !IsBranch && !IsLeaf;

    public QuadTree<T> Root => Parent != null ? Parent.Root : this;

    public override string ToString() => ToString(0);
    public string ToString(int depth)
    {
        string[] log =
        [
            $"QuadTree<{typeof(T).Name}> {{",
            $"  {Rect.ToString(depth + 1)}",
            $"  IsLeaf: {IsLeaf}",
            $"}}",
        ];

        return string.Join('\n' + new string(' ', depth * 2), log);
    }
}