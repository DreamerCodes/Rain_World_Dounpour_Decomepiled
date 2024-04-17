using UnityEngine;

public static class RXRectExtensions
{
	public static Rect CloneWithExpansion(this Rect rect, float expansionAmount)
	{
		return new Rect(rect.x - expansionAmount, rect.y - expansionAmount, rect.width + expansionAmount * 2f, rect.height + expansionAmount * 2f);
	}

	public static bool CheckIntersect(this Rect rect, Rect otherRect)
	{
		if (rect.xMax >= otherRect.xMin && rect.xMin <= otherRect.xMax && rect.yMax >= otherRect.yMin)
		{
			return rect.yMin <= otherRect.yMax;
		}
		return false;
	}

	public static bool CheckIntersectComplex(this Rect rect, Rect otherRect)
	{
		float x = rect.x;
		float y = rect.y;
		float x2 = otherRect.x;
		float y2 = otherRect.y;
		if (Mathf.Max(x, x + rect.width) >= Mathf.Min(x2, x2 + otherRect.width) && Mathf.Min(x, x + rect.width) <= Mathf.Max(x2, x2 + otherRect.width) && Mathf.Max(y, y + rect.height) >= Mathf.Max(y2, y2 + otherRect.height))
		{
			return Mathf.Min(y, y + rect.height) <= Mathf.Min(y2, y2 + otherRect.height);
		}
		return false;
	}

	public static Rect CloneAndMultiply(this Rect rect, float multiplier)
	{
		rect.x *= multiplier;
		rect.y *= multiplier;
		rect.width *= multiplier;
		rect.height *= multiplier;
		return rect;
	}

	public static Rect CloneAndOffset(this Rect rect, float offsetX, float offsetY)
	{
		rect.x += offsetX;
		rect.y += offsetY;
		return rect;
	}

	public static Rect CloneAndScaleThenOffset(this Rect rect, float scaleX, float scaleY, float offsetX, float offsetY)
	{
		rect.x = rect.x * scaleX + offsetX;
		rect.y = rect.y * scaleY + offsetY;
		rect.width *= scaleX;
		rect.height *= scaleY;
		return rect;
	}

	public static Vector2 GetClosestInteriorPointAlongDeltaVector(this Rect rect, Vector2 targetPoint)
	{
		if (targetPoint.x >= rect.xMin && targetPoint.x <= rect.xMax && targetPoint.y >= rect.yMin && targetPoint.y <= rect.yMax)
		{
			return targetPoint;
		}
		targetPoint.Normalize();
		float num = Mathf.Abs(targetPoint.x);
		float num2 = Mathf.Abs(targetPoint.y);
		float num3 = rect.width * 0.5f;
		float num4 = rect.height * 0.5f;
		if (num3 * num2 <= num4 * num)
		{
			return targetPoint * num3 / num;
		}
		return targetPoint * num4 / num2;
	}

	public static Vector2 GetClosestInteriorPoint(this Rect rect, Vector2 targetPoint)
	{
		return new Vector2(Mathf.Clamp(targetPoint.x, rect.xMin, rect.xMax), Mathf.Clamp(targetPoint.y, rect.yMin, rect.yMax));
	}

	public static bool CheckIntersectWithCircle(this Rect rect, RXCircle circle)
	{
		Vector2 closestInteriorPoint = rect.GetClosestInteriorPoint(circle.center);
		return (circle.center - closestInteriorPoint).sqrMagnitude <= circle.radiusSquared;
	}
}
