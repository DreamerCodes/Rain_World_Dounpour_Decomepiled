using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class CentipedeShellCosmetic
{
	public float rotation;

	public float lastRotation;

	public float lastDarkness;

	public float darkness;

	public float hue;

	public float saturation;

	public float scaleX;

	public float scaleY;

	public float zRotation;

	private float lastZRotation;

	public int counter;

	public int firstSprite;

	public bool visible;

	public Vector2 pos;

	public Vector2 lastPos;

	private Color blackColor;

	private Color earthColor;

	public int TotalSprites => 2;

	public CentipedeShellCosmetic(int firstSprite, Vector2 pos, float hue, float saturation, float scaleX, float scaleY)
	{
		lastDarkness = -1f;
		this.firstSprite = firstSprite;
		this.pos = pos;
		lastPos = pos;
		this.hue = hue;
		this.saturation = saturation;
		this.scaleX = scaleX;
		this.scaleY = scaleY;
		rotation = Random.value * 360f;
		lastRotation = rotation;
		zRotation = Random.value * 360f;
		lastZRotation = zRotation;
		visible = true;
	}

	public void Update()
	{
		counter++;
		lastPos = pos;
		lastRotation = rotation;
		lastZRotation = zRotation;
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites[firstSprite] = new FSprite("CentipedeBackShell");
		sLeaser.sprites[firstSprite + 1] = new FSprite("CentipedeBackShell");
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
		lastDarkness = darkness;
		darkness = rCam.room.Darkness(vector);
		darkness *= 1f - 0.5f * rCam.room.LightSourceExposure(vector);
		Vector2 lhs = Custom.DegToVec(Mathf.Lerp(lastZRotation, zRotation, timeStacker));
		for (int i = 0; i < 2; i++)
		{
			float num = 1f;
			if (i == 1)
			{
				num = 0.75f;
			}
			sLeaser.sprites[firstSprite + i].x = vector.x - camPos.x;
			sLeaser.sprites[firstSprite + i].y = vector.y - camPos.y;
			sLeaser.sprites[firstSprite + i].rotation = Mathf.Lerp(lastRotation, rotation, timeStacker);
			sLeaser.sprites[firstSprite + i].scaleY = scaleY * num;
			sLeaser.sprites[firstSprite + i].scaleX = lhs.x * scaleX * num;
			sLeaser.sprites[firstSprite + i].isVisible = visible;
		}
		sLeaser.sprites[firstSprite].x += Custom.DegToVec(Mathf.Lerp(lastRotation, rotation, timeStacker)).x * 1.5f;
		sLeaser.sprites[firstSprite].y += Custom.DegToVec(Mathf.Lerp(lastRotation, rotation, timeStacker)).y * 1.5f;
		sLeaser.sprites[firstSprite].color = Color.Lerp(Custom.HSL2RGB(hue, saturation, 0.5f), blackColor, 0.7f + 0.3f * darkness);
		if (lhs.y > 0f)
		{
			float num2 = Custom.LerpMap(Mathf.Abs(Vector2.Dot(lhs, Custom.DegToVec(Mathf.Lerp(lastRotation, rotation, timeStacker) - 45f))), 0.5f, 1f, 0f, 1f, 2f);
			sLeaser.sprites[firstSprite + 1].color = Color.Lerp(Custom.HSL2RGB(hue, saturation, 0.5f + 0.25f * num2), blackColor, darkness * 0.1f);
		}
		else
		{
			sLeaser.sprites[firstSprite + 1].color = Color.Lerp(Custom.HSL2RGB(hue, saturation * 0.8f, 0.4f), blackColor, 0.25f + 0.6f * darkness * 0.1f);
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		blackColor = palette.blackColor;
		earthColor = Color.Lerp(palette.fogColor, palette.blackColor, 0.5f);
	}
}
