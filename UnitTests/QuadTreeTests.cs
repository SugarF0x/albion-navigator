using System.Globalization;
using System.Numerics;
using System.Text.RegularExpressions;
using AlbionNavigator.Utils.ForceDirectedGraph.Internal.QuadTree;

namespace UnitTests;

[TestClass]
public sealed partial class QuadTreeTests
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

    [TestMethod]
    public void VisitAfterTests()
    {
        var tree = new QuadTree<Data>().Add([
            (new Vector2(0, 0), new Data(0, "a")),
            (new Vector2(0.5f, 0.5f), new Data(1, "b")),
            (new Vector2(2.5f, 2.5f), new Data(2, "c")),
        ]);

        tree.VisitAfter(Console.WriteLine);
    }

    [TestMethod]
    public void VisitAfterFailingTest()
    {
        const string sample =
            "<7,071068 0> <-9,030887 8,273034> <1,3823192 -15,750847> <11,382857 14,846906> <-20,88893 -3,6949496> <19,787813 -12,587393> <-6,6186123 24,621008> <-12,62248 -24,303766> <27,385693 10,001189> <-28,490234 11,760374> <13,734167 -29,349148> <10,1492195 32,357277> <-30,58987 -17,727373> <35,885345 -7,8893633> <-21,900223 31,150927> <-5,059532 -39,04358> <31,060228 26,177517> <-41,797276 1,728502> <30,487873 -30,339573> <-2,0397158 44,11167> <-29,009369 -34,762863> <45,954006 6,183015> <-38,936714 27,091183> <10,639736 -47,294777> <24,609371 42,94623> <-48,10868 -15,347775> <46,73133 -21,591272> <-20,24504 48,37498> <-18,06858 -50,234715> <48,07818 25,268341> <-53,40262 14,076939> <30,3543 -47,20822>";
        
        var matches = Vector2SampleRegex().Matches(sample);
        var points = matches
            .Select(m => m.Groups[1].Value.Split(' '))
            .Select(parts => new Vector2(
                float.Parse(parts[0].Replace(',', '.'), CultureInfo.InvariantCulture),
                float.Parse(parts[1].Replace(',', '.'), CultureInfo.InvariantCulture)
            ))
            .ToArray();

        var tree = new QuadTree<Data>();
        var data = points.Select((point, index) => (point, new Data(index, $"{index}"))).ToArray();
        tree.Add(data);
        tree.VisitAfter(Console.WriteLine);
    }

    [GeneratedRegex(@"<([^>]+)>")]
    private static partial Regex Vector2SampleRegex();
}

public record Data(int Index, string Value);