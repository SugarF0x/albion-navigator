using System.Numerics;
using AlbionNavigator.Utils.ForceDirectedGraph.Internal.QuadTree;

namespace UnitTests;

[TestClass]
public sealed class QuadTreeTests
{
    // public static IEnumerable<object[]> Cases =>
    // [
    //     [Vector2.One, new Data(0, "someValue")],
    // ];
    //
    // [DataTestMethod]
    // [DynamicData(nameof(Cases))]
    // public void AddTests(Vector2 point, Data data)

    [TestMethod]
    public void AddTests()
    {
        var tree = new QuadTree<Data>();
        Assert.IsTrue(tree.IsVoid);
        
        tree.Add(Vector2.One, new Data(0, "a"));
        Assert.IsTrue(tree.IsLeaf);
        Assert.AreEqual(tree, tree.Root);
        
        tree.Add(new Vector2(0.5f, 0.5f), new Data(1, "b"));
        Assert.IsTrue(tree.IsBranch);
        Assert.AreNotEqual(tree, tree.Root);
    }

    [TestMethod]
    public void VisitTests()
    {
        var tree = new QuadTree<Data>().Add([
            (new Vector2(0, 0), new Data(0, "a")),
            (new Vector2(0.5f, 0.5f), new Data(1, "b")),
            (new Vector2(2.5f, 2.5f), new Data(2, "c")),
        ]);

        tree.Visit(item =>
        {
            Console.WriteLine(item);
            return false;
        });
    }
}

public record Data(int Index, string Value);