using System;
using RWCustom;
using UnityEngine;

public class CentipedeShell : CosmeticSprite
{
	public float rotation;

	public float lastRotation;

	public float rotVel;

	public float lastDarkness = -1f;

	public float darkness;

	private float hue;

	private float saturation;

	private float scaleX;

	private float scaleY;

	private float zRotation;

	private float lastZRotation;

	private float zRotVel;

	private SharedPhysics.TerrainCollisionData scratchTerrainCollisionData;

	public int counter;

	public int dissapearCounter;

	public bool lavaImmune;

	public Color? overrideColor;

	public string overrideSprite;

	private Color blackColor;

	private Color earthColor;

	public CentipedeShell(Vector2 pos, Vector2 vel, float hue, float saturation, float scaleX, float scaleY)
	{
		base.pos = pos + vel;
		lastPos = pos;
		base.vel = vel;
		this.hue = hue;
		this.saturation = saturation;
		this.scaleX = scaleX;
		this.scaleY = scaleY;
		rotation = UnityEngine.Random.value * 360f;
		lastRotation = rotation;
		rotVel = Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * Custom.LerpMap(vel.magnitude, 0f, 18f, 5f, 26f);
		zRotation = UnityEngine.Random.value * 360f;
		lastZRotation = rotation;
		zRotVel = Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * Custom.LerpMap(vel.magnitude, 0f, 18f, 2f, 16f);
	}

	public CentipedeShell(Vector2 pos, Vector2 vel, Color overrideColor, float scaleX, float scaleY, string overrideSprite)
		: this(pos, vel, 0f, 0f, scaleX, scaleY)
	{
		this.overrideColor = overrideColor;
		this.overrideSprite = overrideSprite;
	}

	public override void Update(bool eu)
	{
		counter++;
		if (room.PointSubmerged(pos))
		{
			vel *= 0.92f;
			vel.y -= room.gravity * 0.1f;
			rotVel *= 0.965f;
			zRotVel *= 0.965f;
		}
		else
		{
			vel *= 0.999f;
			vel.y -= room.gravity * 0.9f;
		}
		if (counter < 10 && UnityEngine.Random.value < 0.1f)
		{
			room.AddObject(new WaterDrip(Vector2.Lerp(lastPos, pos, UnityEngine.Random.value), vel + Custom.RNV() * UnityEngine.Random.value * 2f, waterColor: false));
		}
		lastRotation = rotation;
		rotation += rotVel * Vector2.Distance(lastPos, pos);
		lastZRotation = zRotation;
		zRotation += zRotVel * Vector2.Distance(lastPos, pos);
		if (!Custom.DistLess(lastPos, pos, 3f) && room.GetTile(pos).Solid && !room.GetTile(lastPos).Solid)
		{
			IntVector2? intVector = SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(room, room.GetTilePosition(lastPos), room.GetTilePosition(pos));
			FloatRect floatRect = Custom.RectCollision(pos, lastPos, room.TileRect(intVector.Value).Grow(2f));
			pos = floatRect.GetCorner(FloatRect.CornerLabel.D);
			bool flag = false;
			if (floatRect.GetCorner(FloatRect.CornerLabel.B).x < 0f)
			{
				vel.x = Mathf.Abs(vel.x) * 0.15f;
				flag = true;
			}
			else if (floatRect.GetCorner(FloatRect.CornerLabel.B).x > 0f)
			{
				vel.x = (0f - Mathf.Abs(vel.x)) * 0.15f;
				flag = true;
			}
			else if (floatRect.GetCorner(FloatRect.CornerLabel.B).y < 0f)
			{
				vel.y = Mathf.Abs(vel.y) * 0.15f;
				flag = true;
			}
			else if (floatRect.GetCorner(FloatRect.CornerLabel.B).y > 0f)
			{
				vel.y = (0f - Mathf.Abs(vel.y)) * 0.15f;
				flag = true;
			}
			if (flag)
			{
				rotVel *= 0.8f;
				zRotVel *= 0.8f;
				if (vel.magnitude > 3f)
				{
					rotVel += Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 4f * UnityEngine.Random.value * Mathf.Abs(rotVel / 15f);
					zRotVel += Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 4f * UnityEngine.Random.value * Mathf.Abs(rotVel / 15f);
				}
			}
		}
		SharedPhysics.TerrainCollisionData cd = scratchTerrainCollisionData.Set(pos, lastPos, vel, 3f, new IntVector2(0, 0), goThroughFloors: true);
		cd = SharedPhysics.VerticalCollision(room, cd);
		cd = SharedPhysics.HorizontalCollision(room, cd);
		pos = cd.pos;
		vel = cd.vel;
		if (cd.contactPoint.x != 0)
		{
			vel.y *= 0.6f;
		}
		if (cd.contactPoint.y != 0)
		{
			vel.x *= 0.6f;
		}
		if (cd.contactPoint.y < 0)
		{
			rotVel *= 0.7f;
			zRotVel *= 0.7f;
			if (vel.magnitude < 1f)
			{
				dissapearCounter++;
				if (dissapearCounter > 30)
				{
					counter = Math.Max(counter, 300);
				}
			}
		}
		if (dissapearCounter > 390 || pos.x < -100f || pos.y < -100f)
		{
			Destroy();
		}
		base.Update(eu);
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		if (overrideSprite == null)
		{
			sLeaser.sprites = new FSprite[2];
			sLeaser.sprites[0] = new FSprite("CentipedeBackShell");
			sLeaser.sprites[1] = new FSprite("CentipedeBackShell");
		}
		else
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite(overrideSprite);
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
		float num = Mathf.InverseLerp(305f, 380f, (float)counter + timeStacker);
		vector.y -= 20f * Mathf.Pow(num, 3f);
		float num2 = Mathf.Pow(1f - num, 0.25f);
		lastDarkness = darkness;
		darkness = rCam.room.Darkness(vector);
		darkness *= 1f - 0.5f * rCam.room.LightSourceExposure(vector);
		Vector2 lhs = Custom.DegToVec(Mathf.Lerp(lastZRotation, zRotation, timeStacker));
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i].x = vector.x - camPos.x;
			sLeaser.sprites[i].y = vector.y - camPos.y;
			sLeaser.sprites[i].rotation = Mathf.Lerp(lastRotation, rotation, timeStacker);
			sLeaser.sprites[i].scaleY = num2 * scaleY;
			if (Mathf.Abs(lhs.x) < 0.1f)
			{
				sLeaser.sprites[i].scaleX = 0.1f * Mathf.Sign(lhs.x) * num2 * scaleX;
			}
			else
			{
				sLeaser.sprites[i].scaleX = lhs.x * num2 * scaleX;
			}
		}
		sLeaser.sprites[0].x += Custom.DegToVec(Mathf.Lerp(lastRotation, rotation, timeStacker)).x * 1.5f * num2;
		sLeaser.sprites[0].y += Custom.DegToVec(Mathf.Lerp(lastRotation, rotation, timeStacker)).y * 1.5f * num2;
		if (overrideColor.HasValue)
		{
			sLeaser.sprites[0].color = Color.Lerp(overrideColor.Value, blackColor, darkness);
		}
		else if (ModManager.MSC && lavaImmune)
		{
			sLeaser.sprites[0].color = Color.Lerp(RainWorld.SaturatedGold, blackColor, 0.7f + 0.3f * darkness);
		}
		else
		{
			sLeaser.sprites[0].color = Color.Lerp(Custom.HSL2RGB(hue, saturation, 0.5f), blackColor, 0.7f + 0.3f * darkness);
		}
		if (sLeaser.sprites.Length > 1)
		{
			if (lhs.y > 0f)
			{
				float num3 = Custom.LerpMap(Mathf.Abs(Vector2.Dot(lhs, Custom.DegToVec(Mathf.Lerp(lastRotation, rotation, timeStacker) - 45f))), 0.5f, 1f, 0f, 1f, 2f);
				num3 *= 1f - num;
				if (ModManager.MSC && lavaImmune)
				{
					sLeaser.sprites[1].color = Color.Lerp(RainWorld.SaturatedGold, blackColor, darkness);
				}
				else
				{
					sLeaser.sprites[1].color = Color.Lerp(Custom.HSL2RGB(hue, saturation, 0.5f + 0.25f * num3), blackColor, darkness);
				}
			}
			else if (ModManager.MSC && lavaImmune)
			{
				sLeaser.sprites[1].color = Color.Lerp(RainWorld.SaturatedGold, blackColor, 0.4f + 0.6f * darkness);
			}
			else
			{
				sLeaser.sprites[1].color = Color.Lerp(Custom.HSL2RGB(hue, saturation * 0.8f, 0.4f), blackColor, 0.4f + 0.6f * darkness);
			}
		}
		if (num > 0.3f)
		{
			for (int j = 0; j < sLeaser.sprites.Length; j++)
			{
				sLeaser.sprites[j].color = Color.Lerp(sLeaser.sprites[j].color, earthColor, Mathf.Pow(Mathf.InverseLerp(0.3f, 1f, num), 1.6f));
			}
		}
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		blackColor = palette.blackColor;
		earthColor = Color.Lerp(palette.fogColor, palette.blackColor, 0.5f);
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		base.AddToContainer(sLeaser, rCam, newContatiner);
	}
}
