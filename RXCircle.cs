using UnityEngine;

public class RXCircle
{
	public Vector2 center;

	public float radius;

	public float radiusSquared;

	public RXCircle(Vector2 center, float radius)
	{
		this.center = center;
		this.radius = radius;
		radiusSquared = radius * radius;
	}

	public bool CheckIntersectWithRect(Rect rect)
	{
		return rect.CheckIntersectWithCircle(this);
	}

	public bool CheckIntersectWithCircle(RXCircle circle)
	{
		Vector2 vector = circle.center - center;
		float num = (circle.radius + radius) * (circle.radius + radius);
		return vector.sqrMagnitude <= num;
	}
}
