using System;
using System.Numerics;

namespace AlbionNavigator.Utils.ForceDirectedGraph.Internal.QuadTree.Internal;

public struct Rect(Vector2 position, Vector2 size)
{
    public Vector2 Position = position;
    public Vector2 Size = size;

    public Vector2 Center => Position + Size / 2;
    public Vector2 End => Position + Size;
    public bool HasArea => Size != Vector2.Zero;

    public bool Includes(Vector2 point)
    {
        return point.X >= Position.X && point.Y >= Position.Y && point.X < Position.X + Size.X && point.Y < Position.Y + Size.Y;
    }
    
    public Rect ExpandFromQuadrantIndex(int index)
    {
        var rect = this;
        
        rect.Size *= 2;
        switch (index)
        {
            case 0: break;
            case 1: rect.Position -= rect.Size with { Y = 0 }; break;
            case 2: rect.Position -= rect.Size with { X = 0 }; break;
            case 3: rect.Position -= rect.Size; break;
            default: throw new ArgumentOutOfRangeException(nameof(index), index, null);
        }

        return rect;
    }
    
    public Rect ShrinkToQuadrantIndex(int index)
    {
        var rect = this;
        
        rect.Size /= 2;
        switch (index)
        {
            case 0: break;
            case 1: rect.Position += rect.Size with { Y = 0 }; break;
            case 2: rect.Position += rect.Size with { X = 0 }; break;
            case 3: rect.Position += rect.Size; break;
            default: throw new ArgumentOutOfRangeException(nameof(index), index, null);
        }

        return rect;
    }

    public Rect Merge(Rect rect)
    {
        return new Rect(Vector2.Zero, Vector2.Zero)
        {
            Position = Vector2.Min(Position, rect.Position),
            Size = Vector2.Max(End, rect.End) - Vector2.Min(Position, rect.Position),
        };
    }
}
