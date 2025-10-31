namespace AlbionNavigator.Utils.ForceDirectedGraph.Datum;

public struct Link(int sourceIndex, int targetIndex)
{
    public int Index;
    public int Source = sourceIndex;
    public int Target = targetIndex;

    public void Deconstruct(out int sourceIndex, out int targetIndex, out int linkIndex)
    {
        sourceIndex = Source;
        targetIndex = Target;
        linkIndex = Index;
    }
}