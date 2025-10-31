namespace AlbionNavigator.Utils.ForceDirectedGraph.Datum;

public class Link(Node source, Node target)
{
    // TODO: source and targets might as well be indexes => change class to struct
    
    public int Index;
    public Node Source = source;
    public Node Target = target;

    public void Deconstruct(out Node source, out Node target, out int index)
    {
        source = Source;
        target = Target;
        index = Index;
    }
}