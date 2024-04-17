using RWCustom;
using UnityEngine;

public class ForcedVisibilityVisualizer
{
	private FloatRect rct = new FloatRect(300f, 300f, 450f, 650f);

	private FSprite rectSprite;

	private FSprite[] projectionLines;

	private FSprite[] projectionLinesClosestPoints;

	private FSprite[] projectionPointsOnRect;

	private FSprite point1;

	private FSprite point2;

	private FSprite movedToVisiblePlacePoint;

	private Vector2 p1;

	private Vector2 p2;

	private FSprite moveLine;

	private Vector2 lastPoint;

	public ForcedVisibilityVisualizer()
	{
		rectSprite = new FSprite("pixel");
		rectSprite.scaleX = rct.right - rct.left;
		rectSprite.scaleY = rct.top - rct.bottom;
		rectSprite.anchorX = 0f;
		rectSprite.anchorY = 0f;
		rectSprite.x = rct.left;
		rectSprite.y = rct.bottom;
		rectSprite.color = new Color(1f, 0f, 1f);
		rectSprite.alpha = 0.3f;
		Futile.stage.AddChild(rectSprite);
		projectionLines = new FSprite[2];
		projectionLinesClosestPoints = new FSprite[2];
		for (int i = 0; i < 2; i++)
		{
			projectionLines[i] = new FSprite("pixel");
			projectionLines[i].scaleY = 1000f;
			projectionLines[i].anchorY = 0f;
			projectionLines[i].color = ((i == 0) ? new Color(1f, 0f, 0f) : new Color(0.1f, 1f, 0f));
			Futile.stage.AddChild(projectionLines[i]);
			projectionLinesClosestPoints[i] = new FSprite("pixel");
			projectionLinesClosestPoints[i].scale = 4f;
			projectionLinesClosestPoints[i].color = ((i == 0) ? new Color(1f, 0f, 0f) : new Color(0.1f, 1f, 0f));
			Futile.stage.AddChild(projectionLinesClosestPoints[i]);
		}
		moveLine = new FSprite("pixel");
		moveLine.anchorY = 0f;
		moveLine.color = new Color(0f, 0f, 1f);
		Futile.stage.AddChild(moveLine);
		projectionPointsOnRect = new FSprite[4];
		for (int j = 0; j < 4; j++)
		{
			projectionPointsOnRect[j] = new FSprite("pixel");
			projectionPointsOnRect[j].scale = 4f;
			projectionPointsOnRect[j].color = new Color(1f, 0f, 1f);
			Futile.stage.AddChild(projectionPointsOnRect[j]);
		}
		point1 = new FSprite("Circle20");
		point1.color = new Color(0f, 1f, 0f);
		Futile.stage.AddChild(point1);
		movedToVisiblePlacePoint = new FSprite("Circle20");
		movedToVisiblePlacePoint.color = new Color(0f, 0f, 1f);
		Futile.stage.AddChild(movedToVisiblePlacePoint);
		point2 = new FSprite("Circle20");
		point2.color = new Color(0f, 0f, 1f);
		point2.scale = 0.5f;
		Futile.stage.AddChild(point2);
		lastPoint = new Vector2(100f, 100f);
	}

	public void Update()
	{
		if (Input.GetKey("b"))
		{
			p1 = Futile.mousePosition;
		}
		if (Input.GetKey("n"))
		{
			lastPoint = Futile.mousePosition;
		}
		p2 = Futile.mousePosition;
		point1.x = p1.x;
		point1.y = p1.y;
		point2.x = p2.x;
		point2.y = p2.y;
		projectionPointsOnRect[0].x = Custom.VerticalCrossPoint(p1, p2, rct.left).x;
		projectionPointsOnRect[0].y = Custom.VerticalCrossPoint(p1, p2, rct.left).y;
		projectionPointsOnRect[1].x = Custom.VerticalCrossPoint(p1, p2, rct.right).x;
		projectionPointsOnRect[1].y = Custom.VerticalCrossPoint(p1, p2, rct.right).y;
		projectionPointsOnRect[2].x = Custom.HorizontalCrossPoint(p1, p2, rct.top).x;
		projectionPointsOnRect[2].y = Custom.HorizontalCrossPoint(p1, p2, rct.top).y;
		projectionPointsOnRect[3].x = Custom.HorizontalCrossPoint(p1, p2, rct.bottom).x;
		projectionPointsOnRect[3].y = Custom.HorizontalCrossPoint(p1, p2, rct.bottom).y;
		FloatRect.CornerLabel[] array = Custom.VisibleCornersOnRect(p1, rct);
		for (int i = 0; i < 2; i++)
		{
			projectionLines[i].x = rct.GetCorner(array[i]).x;
			projectionLines[i].y = rct.GetCorner(array[i]).y;
			projectionLines[i].rotation = Custom.AimFromOneVectorToAnother(p1, rct.GetCorner(array[i]));
			projectionLinesClosestPoints[i].x = Custom.ClosestPointOnLine(p1, rct.GetCorner(array[i]), p2).x;
			projectionLinesClosestPoints[i].y = Custom.ClosestPointOnLine(p1, rct.GetCorner(array[i]), p2).y;
		}
		moveLine.x = lastPoint.x;
		moveLine.y = lastPoint.y;
		moveLine.rotation = Custom.AimFromOneVectorToAnother(lastPoint, p2);
		moveLine.scaleY = Vector2.Distance(lastPoint, p2);
		movedToVisiblePlacePoint.x = Custom.RectCollision(p2, lastPoint, rct).GetCorner(FloatRect.CornerLabel.D).x;
		movedToVisiblePlacePoint.y = Custom.RectCollision(p2, lastPoint, rct).GetCorner(FloatRect.CornerLabel.D).y;
		movedToVisiblePlacePoint.x -= Custom.RectCollision(p2, lastPoint, rct).GetCorner(FloatRect.CornerLabel.B).x * 10f;
		movedToVisiblePlacePoint.y -= Custom.RectCollision(p2, lastPoint, rct).GetCorner(FloatRect.CornerLabel.B).y * 10f;
	}
}
