using System.Collections.Generic;
using System.Numerics;

namespace AlbionNavigator.Utils.ForceDirectedGraph.Internal.QuadTree.Internal;

public class Leaf<T>
{
    public Vector2 Position;
    public List<T> Data;
}