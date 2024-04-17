using UnityEngine;

public class PositionedSoundEmitter : SoundEmitter
{
	public Vector2 pos;

	public Vector2 lastPos;

	public PositionedSoundEmitter(Vector2 pos, float vol, float ptch)
		: base(vol, ptch)
	{
		this.pos = pos;
		lastPos = pos;
	}
}
