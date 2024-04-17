using UnityEngine;

public class FParticle
{
	public float timeRemaining;

	public float x;

	public float y;

	public float speedX;

	public float speedY;

	public float scale;

	public float scaleDeltaPerSecond;

	public Color color;

	public float redDeltaPerSecond;

	public float greenDeltaPerSecond;

	public float blueDeltaPerSecond;

	public float alphaDeltaPerSecond;

	public float elementHalfWidth;

	public float elementHalfHeight;

	public Vector2 uvTopLeft;

	public Vector2 uvTopRight;

	public Vector2 uvBottomRight;

	public Vector2 uvBottomLeft;

	public Vector2 initialTopLeft;

	public Vector2 initialTopRight;

	public Vector2 initialBottomRight;

	public Vector2 initialBottomLeft;

	public float resultTopLeftX;

	public float resultTopLeftY;

	public float resultTopRightX;

	public float resultTopRightY;

	public float resultBottomRightX;

	public float resultBottomRightY;

	public float resultBottomLeftX;

	public float resultBottomLeftY;

	public double rotation;

	public double rotationDeltaPerSecond;

	public bool doesNeedRotationUpdates;
}
