using RWCustom;
using UnityEngine;

public class BlinkSpeck : GenericZeroGSpeck
{
	public float roomDarkness;

	public float blink;

	public float lastBlink;

	public Color col;

	public Color lastCol;

	public override void Update(bool eu)
	{
		base.Update(eu);
		lastBlink = blink;
		if (Random.value < 1f / 3f)
		{
			blink = Random.value;
		}
		roomDarkness = room.Darkness(pos);
		if (roomDarkness > 0f)
		{
			lastCol = col;
			col = room.LightSourceColor(pos);
		}
	}

	public override void ResetMe()
	{
		myFloatSpeed = Mathf.Lerp(0.1f, 3f, Random.value);
		vel = Custom.RNV() * myFloatSpeed;
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1];
		sLeaser.sprites[0] = new FSprite("pixel");
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		sLeaser.sprites[0].isVisible = !reset;
		sLeaser.sprites[0].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x;
		sLeaser.sprites[0].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y;
		if (roomDarkness == 0f)
		{
			sLeaser.sprites[0].color = new Color(Mathf.Lerp(lastBlink, blink, timeStacker), Mathf.Lerp(lastBlink, blink, timeStacker), Mathf.Lerp(lastBlink, blink, timeStacker));
		}
		else
		{
			sLeaser.sprites[0].color = Color.Lerp(new Color(Mathf.Lerp(lastBlink, blink, timeStacker), Mathf.Lerp(lastBlink, blink, timeStacker), Mathf.Lerp(lastBlink, blink, timeStacker)), Color.Lerp(lastCol, col, timeStacker), roomDarkness);
		}
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}
}
