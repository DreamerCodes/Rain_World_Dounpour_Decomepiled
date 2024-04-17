using UnityEngine;

public struct FloatRect
{
	public enum CornerLabel
	{
		A,
		B,
		C,
		D,
		None
	}

	public float left;

	public float bottom;

	public float right;

	public float top;

	public Vector2 Center => new Vector2((left + right) / 2f, (bottom + top) / 2f);

	public FloatRect(float left, float bottom, float right, float top)
	{
		this.left = left;
		this.bottom = bottom;
		this.right = right;
		this.top = top;
	}

	public Vector2 GetCorner(int corner)
	{
		return corner switch
		{
			0 => new Vector2(left, top), 
			1 => new Vector2(right, top), 
			2 => new Vector2(right, bottom), 
			3 => new Vector2(left, bottom), 
			_ => new Vector2(0f, 0f), 
		};
	}

	public Vector2 GetCorner(CornerLabel corner)
	{
		return corner switch
		{
			CornerLabel.A => new Vector2(left, top), 
			CornerLabel.B => new Vector2(right, top), 
			CornerLabel.C => new Vector2(right, bottom), 
			CornerLabel.D => new Vector2(left, bottom), 
			_ => new Vector2(0f, 0f), 
		};
	}

	public static FloatRect MakeFromVector2(Vector2 lowerLeft, Vector2 upperRight)
	{
		return new FloatRect(lowerLeft.x, lowerLeft.y, upperRight.x, upperRight.y);
	}

	public bool Vector2Inside(Vector2 v2)
	{
		if (v2.x > left && v2.x < right && v2.y > bottom)
		{
			return v2.y < top;
		}
		return false;
	}

	public FloatRect Grow(float grow)
	{
		left -= grow;
		bottom -= grow;
		right += grow;
		top += grow;
		return this;
	}

	public FloatRect Shrink(float shrink)
	{
		left += shrink;
		bottom += shrink;
		right -= shrink;
		top -= shrink;
		return this;
	}

	public override string ToString()
	{
		return "rect: " + left + ", " + bottom + ", " + right + ", " + top;
	}
}
