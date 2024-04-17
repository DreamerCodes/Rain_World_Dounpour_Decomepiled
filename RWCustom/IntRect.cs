using System;

namespace RWCustom;

public struct IntRect
{
	public int left;

	public int bottom;

	public int right;

	public int top;

	public int Height => Math.Abs(top - bottom);

	public int Width => Math.Abs(right - left);

	public int Area => Height * Width;

	public IntRect(int left, int bottom, int right, int top)
	{
		this.left = left;
		this.bottom = bottom;
		this.right = right;
		this.top = top;
	}

	public FloatRect ToFloatRect()
	{
		return new FloatRect(left, bottom, right, top);
	}

	public static IntRect MakeFromIntVector2(IntVector2 intVec2)
	{
		return new IntRect(intVec2.x, intVec2.y, intVec2.x, intVec2.y);
	}

	public void ExpandToInclude(IntVector2 iv2)
	{
		if (iv2.x < left)
		{
			left = iv2.x;
		}
		if (iv2.x > right)
		{
			right = iv2.x;
		}
		if (iv2.y < bottom)
		{
			bottom = iv2.y;
		}
		if (iv2.y > top)
		{
			top = iv2.y;
		}
	}

	public void Grow(int grow)
	{
		left -= grow;
		right += grow;
		bottom -= grow;
		top += grow;
	}
}
