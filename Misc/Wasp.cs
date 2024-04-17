using System;
using RWCustom;
using UnityEngine;

public class Wasp : CosmeticInsect
{
	public Vector2? sitPos;

	public Vector2 rot;

	public Vector2 lastRot;

	public Vector2 flyDir;

	public Vector2 wallDir;

	public Vector2 bigFlyDir;

	public float wingsOut;

	public float lastWingsOut;

	public float colorFac;

	public Wasp(Room room, Vector2 pos)
		: base(room, pos, Type.Wasp)
	{
		colorFac = UnityEngine.Random.value;
		bigFlyDir = Custom.RNV();
	}

	public override void Update(bool eu)
	{
		lastRot = rot;
		lastWingsOut = wingsOut;
		base.Update(eu);
		if (!sitPos.HasValue)
		{
			vel.y -= 0.8f;
		}
		if (submerged)
		{
			vel *= 0.8f;
			rot = Vector3.Slerp(rot, Custom.RNV(), 0.5f);
		}
	}

	public override void Reset(Vector2 resetPos)
	{
		base.Reset(resetPos);
		sitPos = null;
	}

	public override void Act()
	{
		base.Act();
		if (wantToBurrow && sitPos.HasValue)
		{
			sitPos = null;
		}
		if (sitPos.HasValue)
		{
			pos = sitPos.Value;
			vel *= 0f;
			wingsOut = Mathf.Max(0f, wingsOut - 1f / Mathf.Lerp(10f, 200f, UnityEngine.Random.value));
			if (UnityEngine.Random.value < 1f / 120f)
			{
				TakeOff();
			}
			return;
		}
		vel.y += 0.7f;
		vel *= 0.8f;
		bigFlyDir += Custom.RNV() * UnityEngine.Random.value * 0.1f;
		bigFlyDir.Normalize();
		flyDir += Custom.RNV() * UnityEngine.Random.value * 0.6f;
		flyDir += bigFlyDir * 2f * UnityEngine.Random.value;
		if (wantToBurrow)
		{
			flyDir.y -= 0.5f;
		}
		else
		{
			if (submerged)
			{
				flyDir.y += 1f;
				vel.y += 0.3f;
			}
			else if (room.readyForAI)
			{
				IntVector2 tilePosition = room.GetTilePosition(pos + flyDir * 40f);
				for (int i = -5; i <= 5; i++)
				{
					for (int j = -3; j <= 3; j++)
					{
						if ((i != 0 || j != 0) && Math.Min(5, room.aimap.getTerrainProximity(tilePosition + new IntVector2(i, j))) > Math.Min(5, room.aimap.getTerrainProximity(tilePosition)))
						{
							flyDir += new IntVector2(i, j).ToVector2().normalized * 0.48f * UnityEngine.Random.value / Mathf.Pow(new IntVector2(i, j).ToVector2().magnitude, 1.4f);
						}
					}
				}
			}
			if (pos.x < 0f)
			{
				flyDir.x += UnityEngine.Random.value * 0.05f;
			}
			else if (pos.x > room.PixelWidth)
			{
				flyDir.x -= UnityEngine.Random.value * 0.05f;
			}
			if (pos.y < 0f)
			{
				flyDir.y += UnityEngine.Random.value * 0.05f;
			}
			else if (pos.y > room.PixelHeight)
			{
				flyDir.y -= UnityEngine.Random.value * 0.05f;
			}
		}
		flyDir.Normalize();
		vel += flyDir * 1.8f;
		bigFlyDir += vel * 0.02f;
		rot = Vector3.Slerp(rot, (-vel - flyDir + new Vector2(0f, -1f)).normalized, 0.3f);
		wingsOut = 1f;
	}

	public void TakeOff()
	{
		if (sitPos.HasValue && !burrowPos.HasValue)
		{
			flyDir = (-wallDir + Custom.RNV() * UnityEngine.Random.value).normalized;
			pos = sitPos.Value + flyDir * 2f;
			vel = flyDir * 6f;
			sitPos = null;
			bigFlyDir = Custom.RNV();
		}
	}

	public override void WallCollision(IntVector2 dir, bool first)
	{
		if (sitPos.HasValue || wantToBurrow)
		{
			return;
		}
		sitPos = pos;
		wallDir = Custom.RNV() * 0.1f;
		for (int i = 0; i < 8; i++)
		{
			if (room.GetTile(pos + Custom.eightDirections[i].ToVector2() * 20f).Solid)
			{
				wallDir += Custom.eightDirections[i].ToVector2().normalized;
			}
		}
		wallDir.Normalize();
		rot = (Custom.PerpendicularVector(wallDir) * ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f) + Custom.RNV() * 0.2f).normalized;
	}

	public override void EmergeFromGround(Vector2 emergePos)
	{
		base.EmergeFromGround(emergePos);
		pos = emergePos;
		sitPos = emergePos + new Vector2(0f, 4f);
		TakeOff();
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		base.InitiateSprites(sLeaser, rCam);
		sLeaser.sprites = new FSprite[5];
		sLeaser.sprites[0] = new FSprite("Circle20");
		sLeaser.sprites[0].anchorY = -0.2f;
		sLeaser.sprites[1] = new FSprite("Circle20");
		sLeaser.sprites[1].anchorY = 0.7f;
		sLeaser.sprites[2] = new FSprite("pixel");
		sLeaser.sprites[2].anchorY = 0f;
		for (int i = 0; i < 2; i++)
		{
			sLeaser.sprites[3 + i] = new FSprite("pixel");
			sLeaser.sprites[3 + i].anchorY = 0f;
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
		float num = Mathf.Lerp(lastInGround, inGround, timeStacker);
		Vector2 vector2 = Vector3.Slerp(lastRot, rot, timeStacker);
		float num2 = Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(lastWingsOut, wingsOut, timeStacker)), 0.6f);
		vector.y -= 5f * num;
		sLeaser.sprites[0].x = vector.x - camPos.x;
		sLeaser.sprites[0].y = vector.y - camPos.y;
		sLeaser.sprites[0].rotation = Custom.VecToDeg(Vector3.Slerp(vector2, new Vector2(0f, -1f), 0.4f));
		sLeaser.sprites[0].scaleX = (3f - num2 * 0.5f) * (1f - num) / 20f;
		sLeaser.sprites[0].scaleY = 5f * (1f - num) / 20f;
		sLeaser.sprites[1].x = vector.x - camPos.x;
		sLeaser.sprites[1].y = vector.y - camPos.y;
		sLeaser.sprites[1].rotation = Custom.VecToDeg(vector2);
		sLeaser.sprites[1].scaleX = 2.5f * (1f - num) / 20f;
		sLeaser.sprites[1].scaleY = 4f * (1f - num) / 20f;
		sLeaser.sprites[2].x = vector.x - camPos.x + Vector3.Slerp(vector2, new Vector2(0f, -1f), 0.4f).x * 4f;
		sLeaser.sprites[2].y = vector.y - camPos.y + Vector3.Slerp(vector2, new Vector2(0f, -1f), 0.4f).y * 4f;
		sLeaser.sprites[2].rotation = Custom.VecToDeg(Vector3.Slerp(vector2, new Vector2(0f, -1f), 0.6f));
		sLeaser.sprites[2].scaleY = 5f * (1f - num);
		for (int i = 0; i < 2; i++)
		{
			sLeaser.sprites[3 + i].x = vector.x - camPos.x;
			sLeaser.sprites[3 + i].y = vector.y - camPos.y;
			sLeaser.sprites[3 + i].scaleY = 5f * (1f - num);
			sLeaser.sprites[3 + i].rotation = (45f + UnityEngine.Random.value * 90f * Mathf.Pow(num2, 4f)) * num2 * ((i == 0) ? (-1f) : 1f) + Custom.VecToDeg(Vector3.Slerp(vector2, new Vector2(0f, 1f), 0.8f * num2));
			sLeaser.sprites[3 + i].anchorY = -0.5f * num2;
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		base.ApplyPalette(sLeaser, rCam, palette);
		for (int i = 0; i < 3; i++)
		{
			sLeaser.sprites[i].color = Color.Lerp(Custom.HSL2RGB(Mathf.Lerp(0.35f, 0.4f, colorFac), 1f, 0.07f), palette.blackColor, 0.2f + 0.8f * palette.darkness);
		}
		for (int j = 0; j < 2; j++)
		{
			sLeaser.sprites[3 + j].color = Color.Lerp(new Color(1f, 1f, 1f), palette.fogColor, 0.5f + 0.5f * palette.darkness);
		}
	}
}
