using RWCustom;
using UnityEngine;

public class Beetle : CosmeticInsect
{
	public Vector2? sitPos;

	public float stressed;

	public Vector2 rot;

	public Vector2 lastRot;

	private Vector2? goalPos;

	private Vector2 wallDir;

	private Vector2 flyDir;

	public float wingsOut;

	public float lastWingsOut;

	public Beetle(Room room, Vector2 pos)
		: base(room, pos, Type.Beetle)
	{
		creatureAvoider = new CreatureAvoider(this, 20, 300f, 0.3f);
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

	public override void Act()
	{
		base.Act();
		if (wantToBurrow && sitPos.HasValue)
		{
			sitPos = null;
		}
		float num = Mathf.Pow(creatureAvoider.FleeSpeed, 0.3f);
		if (num > stressed)
		{
			stressed = Custom.LerpAndTick(stressed, num, 0.05f, 1f / 60f);
		}
		else
		{
			stressed = Custom.LerpAndTick(stressed, num, 0.02f, 0.005f);
		}
		if (sitPos.HasValue)
		{
			pos = sitPos.Value;
			vel *= 0f;
			wingsOut = Mathf.Max(0f, wingsOut - 1f / Mathf.Lerp(10f, 200f, Random.value));
			if (Random.value < 1f / Mathf.Lerp(400f, 40f, stressed) || (creatureAvoider.currentWorstCrit != null && Custom.DistLess(pos, creatureAvoider.currentWorstCrit.DangerPos, 30f)))
			{
				TakeOff();
			}
			return;
		}
		vel.y += 0.7f;
		vel *= 0.8f;
		flyDir += Custom.RNV() * Random.value * 0.3f;
		if (base.OutOfBounds)
		{
			flyDir += Custom.DirVec(pos, mySwarm.placedObject.pos) * Random.value * 0.1f;
		}
		else if (mySwarm == null)
		{
			if (pos.x < 0f)
			{
				flyDir.x += 0.05f * Random.value;
			}
			else if (pos.x > room.PixelWidth)
			{
				flyDir.x -= 0.05f * Random.value;
			}
			if (pos.y < 0f)
			{
				flyDir.y += 0.05f * Random.value;
			}
			else if (pos.y > room.PixelHeight)
			{
				flyDir.y -= 0.05f * Random.value;
			}
		}
		if (wantToBurrow)
		{
			flyDir.y -= 0.5f;
		}
		else if (submerged)
		{
			flyDir.y += 1f;
			vel.y += 0.3f;
		}
		flyDir.Normalize();
		vel += flyDir * 0.9f + Custom.RNV() * 0.3f;
		rot = Vector3.Slerp(rot, (-vel - flyDir + new Vector2(0f, -8f)).normalized, 0.2f);
		wingsOut = 1f;
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
		rot = (Custom.PerpendicularVector(wallDir) * ((Random.value < 0.5f) ? (-1f) : 1f) + Custom.RNV() * 0.2f).normalized;
	}

	public override void EmergeFromGround(Vector2 emergePos)
	{
		base.EmergeFromGround(emergePos);
		pos = emergePos;
		sitPos = emergePos + new Vector2(0f, 4f);
		TakeOff();
	}

	private int WingSprite(int wing, int part)
	{
		return 1 + part * 2 + wing;
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		base.InitiateSprites(sLeaser, rCam);
		sLeaser.sprites = new FSprite[5];
		sLeaser.sprites[0] = new FSprite("Circle20");
		sLeaser.sprites[0].anchorY = 0.2f;
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				sLeaser.sprites[WingSprite(i, j)] = new FSprite("pixel");
			}
			sLeaser.sprites[WingSprite(i, 1)].anchorY = 0f;
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
		float num = Mathf.Lerp(lastInGround, inGround, timeStacker);
		Vector2 v = Vector3.Slerp(lastRot, rot, timeStacker);
		float num2 = Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(lastWingsOut, wingsOut, timeStacker)), 0.6f);
		float num3 = Custom.VecToDeg(v);
		vector.y -= 5f * num;
		sLeaser.sprites[0].x = vector.x - camPos.x;
		sLeaser.sprites[0].y = vector.y - camPos.y;
		sLeaser.sprites[0].rotation = num3;
		sLeaser.sprites[0].scaleX = (3.5f - num2 * 0.5f) * (1f - num) / 20f;
		sLeaser.sprites[0].scaleY = 6f * (1f - num) / 20f;
		for (int i = 0; i < 2; i++)
		{
			sLeaser.sprites[WingSprite(i, 0)].x = vector.x - camPos.x;
			sLeaser.sprites[WingSprite(i, 0)].y = vector.y - camPos.y;
			sLeaser.sprites[WingSprite(i, 1)].x = vector.x - camPos.x;
			sLeaser.sprites[WingSprite(i, 1)].y = vector.y - camPos.y;
			sLeaser.sprites[WingSprite(i, 0)].scaleY = 3f * (1f - num);
			sLeaser.sprites[WingSprite(i, 0)].scaleX = num2;
			sLeaser.sprites[WingSprite(i, 0)].rotation = (45f + Random.value * 90f * Mathf.Pow(num2, 4f)) * num2 * ((i == 0) ? (-1f) : 1f) + num3;
			sLeaser.sprites[WingSprite(i, 0)].anchorY = -1f * num2;
			sLeaser.sprites[WingSprite(i, 1)].scaleY = 5f * (1f - num);
			sLeaser.sprites[WingSprite(i, 1)].rotation = Mathf.Lerp(0f, 135f, num2) * ((i == 0) ? (-1f) : 1f) + num3;
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		base.ApplyPalette(sLeaser, rCam, palette);
		sLeaser.sprites[0].color = palette.blackColor;
		for (int i = 0; i < 2; i++)
		{
			sLeaser.sprites[WingSprite(i, 0)].color = Color.Lerp(new Color(1f, 1f, 1f), palette.fogColor, 0.5f + 0.5f * palette.darkness);
			sLeaser.sprites[WingSprite(i, 1)].color = palette.blackColor;
		}
	}
}
