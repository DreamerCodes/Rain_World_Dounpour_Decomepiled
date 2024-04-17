using UnityEngine;

public struct FLetterQuad
{
	public FCharInfo charInfo;

	public Rect rect;

	public Vector2 topLeft;

	public Vector2 topRight;

	public Vector2 bottomRight;

	public Vector2 bottomLeft;

	public void CalculateVectors()
	{
		topLeft.Set(rect.xMin, rect.yMax);
		topRight.Set(rect.xMax, rect.yMax);
		bottomRight.Set(rect.xMax, rect.yMin);
		bottomLeft.Set(rect.xMin, rect.yMin);
	}

	public void CalculateVectors(float offsetX, float offsetY)
	{
		topLeft.Set(rect.xMin + offsetX, rect.yMax + offsetY);
		topRight.Set(rect.xMax + offsetX, rect.yMax + offsetY);
		bottomRight.Set(rect.xMax + offsetX, rect.yMin + offsetY);
		bottomLeft.Set(rect.xMin + offsetX, rect.yMin + offsetY);
	}

	public void CalculateVectorsToWholePixels(float offsetX, float offsetY)
	{
		float displayScaleInverse = Futile.displayScaleInverse;
		float num = (rect.xMin + offsetX) % displayScaleInverse;
		float num2 = (rect.yMin + offsetY) % displayScaleInverse;
		offsetX -= num;
		offsetY -= num2;
		float x = rect.xMin + offsetX;
		float x2 = rect.xMax + offsetX;
		float y = rect.yMax + offsetY;
		float y2 = rect.yMin + offsetY;
		topLeft.x = x;
		topLeft.y = y;
		topRight.x = x2;
		topRight.y = y;
		bottomRight.x = x2;
		bottomRight.y = y2;
		bottomLeft.x = x;
		bottomLeft.y = y2;
	}
}
