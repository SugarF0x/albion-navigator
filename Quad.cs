using System;
using Godot;

namespace AlbionNavigator;

public struct Quad(QuadTreeNode node, Rect2 rect)
{
	public QuadTreeNode Node = node;
	public Rect2 Rect = rect;

	public enum Quadrant {
		TopLeft,
		TopRight,
		BottomLeft,
		BottomRight,
	}

	public const int QuadCount = 4;

	public static Quadrant GetRelativeQuadrant(Vector2 source, Vector2 target)
	{
		return (Quadrant)((target.Y >= source.Y ? 1 : 0) << 1 | (target.X >= source.X ? 1 : 0));
	}

	public static Rect2 ShrinkRectToQuadrant(Rect2 rect, Quadrant quadrant)
	{
		rect.Size /= 2;
		switch (quadrant)
		{
			case Quadrant.TopLeft: break;
			case Quadrant.TopRight: rect.Position += rect.Size with { Y = 0 }; break;
			case Quadrant.BottomLeft: rect.Position += rect.Size with { X = 0 }; break;
			case Quadrant.BottomRight: rect.Position += rect.Size; break;
			default: throw new ArgumentOutOfRangeException(nameof(quadrant), quadrant, null);
		}
		return rect;
	}

	public static Rect2 ExpandRectFromQuadrant(Rect2 rect, Quadrant quadrant)
	{
		rect.Size *= 2;
		switch (quadrant)
		{
			case Quadrant.TopLeft: break;
			case Quadrant.TopRight: rect.Position -= rect.Size with { Y = 0 }; break;
			case Quadrant.BottomLeft: rect.Position -= rect.Size with { X = 0 }; break;
			case Quadrant.BottomRight: rect.Position -= rect.Size; break;
			default: throw new ArgumentOutOfRangeException(nameof(quadrant), quadrant, null);
		}
		return rect;
	}
}