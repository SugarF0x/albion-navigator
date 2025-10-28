namespace AlbionNavigator.Utils.ForceDirectedGraph.Internal.QuadTree;

public partial class QuadTree<T>
{
    public QuadTree<T> Subdivide(int quadIndex)
    {
        if (IsBranch) throw new QuadTreeSubdivisionException("Subdivision failed: already branch");
        
        var leaf = Leaf;
        if (leaf == null) throw new QuadTreeSubdivisionException("Subdivision failed: leaf is null");
        
        Leaf = null;
        Children = new QuadTree<T>[4];
        Children[quadIndex] = new QuadTree<T>
        {
            Rect = Rect.ShrinkToQuadrantIndex(quadIndex),
            Leaf = leaf 
        };
        
        return this;
    }
}