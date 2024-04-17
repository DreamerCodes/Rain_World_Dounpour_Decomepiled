using UnityEngine;

public class RectSoundEmitter : PositionedSoundEmitter
{
	public FloatRect rect;

	public RectSoundEmitter(FloatRect rect, float vol, float ptch)
		: base(new Vector2(0f, 0f), vol, ptch)
	{
		this.rect = rect;
	}
}
