using System.Numerics;
using AlbionNavigator.Utils.ForceDirectedGraph.Internal.QuadTree.Internal;

namespace AlbionNavigator.Utils.ForceDirectedGraph.Internal.QuadTree;

public partial class QuadTree<T> where T : class
{
    public QuadTree<T> Cover(Vector2 point)
    {
        if (Parent != null) return Parent.Cover(point);
        if (!Rect.HasArea) InitializeRect(point);
        return Rect.Includes(point) ? this : ExpandToCoverPoint(point);
    }

    private void InitializeRect(Vector2 point)
    {
        Rect.Position = new Vector2(float.Floor(point.X), float.Floor(point.Y));
        Rect.Size = Vector2.One;
    }

    private QuadTree<T> ExpandToCoverPoint(Vector2 point)
    {
        Parent = new QuadTree<T>
        {
            Rect = Rect.ExpandFromQuadrantIndex(point.GetRelativeQuadrantIndex(Rect.Position))
        };
        
        return Parent.Cover(point);
    }
}