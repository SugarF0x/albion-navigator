using System.Numerics;

namespace AlbionNavigator.Utils.ForceDirectedGraph.Internal.QuadTree.Internal;

public static class Vector2QuadExtension
{
    public static int GetRelativeQuadrantIndex(this Vector2 vector, Vector2 point) => (point.Y >= vector.Y ? 1 : 0) << 1 | (point.X >= vector.X ? 1 : 0);
}