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
        tree.Add(Vector2.One, new Data(0, "e"));
        
        Assert.IsTrue(tree.IsLeaf);
    }
}

public record Data(int Index, string Value);