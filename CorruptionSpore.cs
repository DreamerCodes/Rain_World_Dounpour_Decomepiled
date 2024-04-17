using System;
using RWCustom;
using UnityEngine;

public class CorruptionSpore : GenericZeroGSpeck
{
	private float scale;

	private float rot;

	private float lastRot;

	private float yRot;

	private float lastYRot;

	private float rotSpeed;

	private float yRotSpeed;

	private float col;

	public override void Update(bool eu)
	{
		base.Update(eu);
		lastRot = rot;
		rot += rotSpeed;
		rotSpeed = Mathf.Clamp(rotSpeed + Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) / 30f, -10f, 10f);
		lastYRot = yRot;
		yRot += yRotSpeed;
		yRotSpeed = Mathf.Clamp(yRotSpeed + Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) / 320f, -0.05f, 0.05f);
	}

	public override void ResetMe()
	{
		myFloatSpeed = Mathf.Lerp(0.1f, 3f, UnityEngine.Random.value);
		vel = Custom.RNV() * myFloatSpeed;
		col = UnityEngine.Random.value;
		scale = UnityEngine.Random.value;
		rot = UnityEngine.Random.value * 360f;
		lastRot = rot;
		rotSpeed = Mathf.Lerp(-10f, 10f, UnityEngine.Random.value);
		yRot = UnityEngine.Random.value * (float)Math.PI;
		lastYRot = yRot;
		yRotSpeed = Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 0.05f;
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1];
		sLeaser.sprites[0] = new FSprite("Pebble" + UnityEngine.Random.Range(1, 15));
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		sLeaser.sprites[0].isVisible = !reset;
		sLeaser.sprites[0].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x;
		sLeaser.sprites[0].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y;
		sLeaser.sprites[0].color = new Color(0f, 0f, col);
		sLeaser.sprites[0].scaleX = Mathf.Lerp(0.15f, 0.35f, scale) * Mathf.Sin(Mathf.Lerp(lastYRot, yRot, timeStacker) * (float)Math.PI);
		sLeaser.sprites[0].scaleY = Mathf.Lerp(0.25f, 0.55f, scale);
		sLeaser.sprites[0].rotation = Mathf.Lerp(lastRot, rot, timeStacker);
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}
}
