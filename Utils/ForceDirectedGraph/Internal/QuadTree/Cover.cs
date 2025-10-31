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
        var treeQuadrantIndex = point.GetRelativeQuadrantIndex(Rect.Position);
        var expandedRect = Rect.ExpandFromQuadrantIndex(treeQuadrantIndex);

        if (IsBranch)
        {
            Parent = new QuadTree<T>
            {
                Rect = expandedRect,
                Children = new QuadTree<T>[4]
            };
            Parent.Children[treeQuadrantIndex] = this;
        
            return Parent.Cover(point);            
        }

        Rect = expandedRect;
        return Cover(point);
    }
}