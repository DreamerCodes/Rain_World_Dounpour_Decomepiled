using RWCustom;
using UnityEngine;

public class Moth : CosmeticInsect
{
	public Vector2? sitPos;

	public Vector2 rot;

	public Vector2 lastRot;

	public Vector2 flyDir;

	public Vector2 wallDir;

	public float wingsOut;

	public float lastWingsOut;

	public float flap;

	public float lastFlap;

	private float colorFac;

	private int sitStill;

	public Moth(Room room, Vector2 pos)
		: base(room, pos, Type.Moth)
	{
		creatureAvoider = new CreatureAvoider(this, 30, 80f, 0.1f);
		wingsOut = 1f;
		for (int i = 0; i < 4; i++)
		{
			if (room.GetTile(pos + Custom.fourDirections[i].ToVector2() * 5f).Solid)
			{
				sitPos = pos;
				wingsOut = 0f;
				wallDir = -Custom.fourDirections[i].ToVector2();
				break;
			}
		}
		lastWingsOut = wingsOut;
		if (burrowPos.HasValue && sitPos.HasValue)
		{
			burrowPos = sitPos.Value;
		}
		rot = Custom.RNV();
		lastRot = rot;
		sitStill = Random.Range(20, 90);
	}

	public override void Update(bool eu)
	{
		lastRot = rot;
		lastWingsOut = wingsOut;
		lastFlap = flap;
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
			wingsOut = Mathf.Max(0f, wingsOut - 1f / Mathf.Lerp(20f, 40f, Random.value));
			if (flap == 1f)
			{
				flap = 0f;
			}
			else
			{
				flap = Mathf.Min(1f, flap + 1f / (5f - 2f * wingsOut));
			}
			if (creatureAvoider.currentWorstCrit != null && Custom.DistLess(creatureAvoider.currentWorstCrit.DangerPos, pos, 70f))
			{
				TakeOff();
			}
			else if (sitStill > 0)
			{
				sitStill--;
			}
			else if (Random.value < 1f / 60f)
			{
				TakeOff();
			}
			return;
		}
		vel.y += 0.4f;
		vel *= 0.82f;
		flyDir += Custom.RNV() * Random.value * 0.6f;
		if (wantToBurrow)
		{
			flyDir.y -= 0.5f;
		}
		else if (submerged)
		{
			flyDir.y += 1f;
			vel.y += 0.3f;
		}
		else if (base.OutOfBounds)
		{
			flyDir += Custom.DirVec(pos, mySwarm.placedObject.pos) * Random.value * 0.075f;
		}
		else
		{
			if (creatureAvoider.currentWorstCrit != null)
			{
				flyDir += Custom.DirVec(creatureAvoider.currentWorstCrit.DangerPos, pos) * creatureAvoider.FleeSpeed * Random.value * 3f;
			}
			if (pos.x < 0f)
			{
				flyDir.x += Random.value * 0.05f;
			}
			else if (pos.x > room.PixelWidth)
			{
				flyDir.x -= Random.value * 0.05f;
			}
			if (pos.y < 0f)
			{
				flyDir.y += Random.value * 0.05f;
			}
			else if (pos.y > room.PixelHeight)
			{
				flyDir.y -= Random.value * 0.05f;
			}
		}
		flyDir.Normalize();
		vel += flyDir * 0.5f + Custom.RNV() * 0.5f * Random.value;
		rot = Vector3.Slerp(rot, (-vel - flyDir + new Vector2(0f, -8f)).normalized, 0.2f);
		wingsOut = 1f;
		if (flap == 1f)
		{
			flap = 0f;
			vel.y += 2.4f;
			rot = Vector3.Slerp(rot, new Vector2(0f, -1f), 0.5f);
		}
		else
		{
			flap = Mathf.Min(1f, flap + 1f / 3f);
		}
	}

	public void TakeOff()
	{
		if (sitPos.HasValue && !burrowPos.HasValue)
		{
			flyDir = (-wallDir + Custom.RNV() * Random.value).normalized;
			pos = sitPos.Value + flyDir * 2f;
			vel = flyDir * 6f;
			sitPos = null;
		}
	}

	public override void WallCollision(IntVector2 dir, bool first)
	{
		if (wantToBurrow)
		{
			return;
		}
		if (!sitPos.HasValue && Random.value < 0.5f)
		{
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
			rot = (Custom.PerpendicularVector(wallDir) * ((Random.value < 0.5f) ? (-1f) : 1f) + Custom.RNV() * 0.2f).normalized;
			if (Random.value < 0.5f)
			{
				sitStill = Random.Range(20, 90);
			}
		}
		else
		{
			vel -= dir.ToVector2() * 1f * Random.value;
		}
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
		sLeaser.sprites[0].anchorY = 0.2f;
		sLeaser.sprites[1] = new FSprite("pixel");
		sLeaser.sprites[2] = new FSprite("pixel");
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
		float num3 = Custom.VecToDeg(vector2);
		float num4 = Custom.SCurve(Mathf.Lerp(lastFlap, flap, timeStacker), 0.6f);
		vector.y -= 5f * num;
		sLeaser.sprites[0].x = vector.x - camPos.x;
		sLeaser.sprites[0].y = vector.y - camPos.y;
		sLeaser.sprites[0].rotation = num3;
		sLeaser.sprites[0].scaleX = (2.5f - num2 * 0.5f) * (1f - num) / 20f;
		sLeaser.sprites[0].scaleY = 5f * (1f - num) / 20f;
		for (int i = 0; i < 2; i++)
		{
			Vector2 vector3 = vector - vector2 * 3f + Custom.PerpendicularVector(vector2) * ((i == 0) ? (-1f) : 1f) * 1.6f;
			sLeaser.sprites[1 + i].x = vector3.x - camPos.x;
			sLeaser.sprites[1 + i].y = vector3.y - camPos.y;
			sLeaser.sprites[3 + i].x = vector.x - camPos.x;
			sLeaser.sprites[3 + i].y = vector.y - camPos.y;
			sLeaser.sprites[3 + i].scaleY = 6f * (1f - num);
			sLeaser.sprites[3 + i].scaleX = num2 * 1.2f;
			sLeaser.sprites[3 + i].rotation = (30f + num4 * 120f * Mathf.Pow(num2, 4f)) * num2 * ((i == 0) ? (-1f) : 1f) + num3;
			sLeaser.sprites[3 + i].anchorY = -0.1f;
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		base.ApplyPalette(sLeaser, rCam, palette);
		Color color = Color.Lerp(Color.Lerp(palette.texture.GetPixel((int)Mathf.Lerp(14f, 20f, colorFac), 2), new Color(1f, 1f, 1f), 0.85f), palette.fogColor, 0.1f * palette.fogAmount + 0.75f * palette.darkness);
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i].color = color;
		}
	}
}
