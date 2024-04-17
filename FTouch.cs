using UnityEngine;

public class FTouch
{
	public int fingerId;

	public Vector2 position;

	public Vector2 deltaPosition;

	public float deltaTime;

	public int tapCount;

	public TouchPhase phase;

	public FTouchSlot slot;

	public void Copy(FTouch touch)
	{
		fingerId = touch.fingerId;
		position = touch.position;
		deltaPosition = touch.deltaPosition;
		deltaTime = touch.deltaTime;
		tapCount = touch.tapCount;
		phase = touch.phase;
		slot = touch.slot;
	}

	public void Copy(Touch touch, float offsetX, float offsetY, float touchScale)
	{
		fingerId = touch.fingerId;
		position.x = (touch.position.x + offsetX) * touchScale;
		position.y = (touch.position.y + offsetY) * touchScale;
		deltaPosition.x = touch.deltaPosition.x * touchScale;
		deltaPosition.y = touch.deltaPosition.y * touchScale;
		deltaTime = touch.deltaTime;
		tapCount = touch.tapCount;
		phase = touch.phase;
		slot = null;
	}
}
