namespace AlbionNavigator.Utils.ForceDirectedGraph.Internal;

public class LinearCongruentialGenerator(long seed = 1)
{
    private const long Multiplier = 1664525;
    private const long Increment = 1013904223;
    private const long Modulus = 4294967296; // 2^32

    private long Seed = seed;

    public float Next()
    {
        Seed = (Multiplier * Seed + Increment) % Modulus;
        return (float)Seed / Modulus;
    }
}