using RWCustom;
using UnityEngine;

public class GrassHopper : CosmeticInsect
{
	public Vector2? sitPos;

	public float stressed;

	public Vector2 rot;

	public Vector2 lastRot;

	public Vector2[,] legs;

	public float colorFac;

	public float colorFac2;

	public int recover;

	public GrassHopper(Room room, Vector2 pos)
		: base(room, pos, Type.GrassHopper)
	{
		creatureAvoider = new CreatureAvoider(this, 20, 300f, 0.2f);
		colorFac = Mathf.Pow(Random.value, 0.2f);
		colorFac2 = Random.value;
		legs = new Vector2[2, 2];
	}

	public override void Update(bool eu)
	{
		lastRot = rot;
		base.Update(eu);
		if (!sitPos.HasValue && !submerged)
		{
			vel.y -= 0.8f;
		}
		if (submerged)
		{
			vel *= 0.9f;
			vel.y += 1.2f;
			rot = Vector3.Slerp(rot, Custom.RNV(), 0.2f);
			return;
		}
		for (int i = 0; i < 2; i++)
		{
			legs[i, 1] = legs[i, 0];
			if (!sitPos.HasValue)
			{
				legs[i, 0] = Vector3.Slerp(legs[i, 0], (-vel + Custom.PerpendicularVector(vel.normalized) * ((i == 0) ? (-1f) : 1f) * -5f + new Vector2((i == 0) ? (-1f) : 1f, 0.7f)).normalized, 0.2f);
			}
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
		if (wantToBurrow)
		{
			if (sitPos.HasValue)
			{
				sitPos = null;
			}
			if (submerged)
			{
				rot = Vector3.Slerp(rot, new Vector2(0f, -1f), 0.3f);
			}
			return;
		}
		float num = Mathf.Pow(creatureAvoider.FleeSpeed, 0.3f);
		if (num > stressed)
		{
			stressed = Custom.LerpAndTick(stressed, num, 0.1f, 0.05f);
		}
		else
		{
			stressed = Custom.LerpAndTick(stressed, num, 0.02f, 0.005f);
		}
		if (Random.value < 0.05f && room.GetTile(room.GetTilePosition(pos) + new IntVector2(-1, 0)).Solid && room.GetTile(room.GetTilePosition(pos) + new IntVector2(1, 0)).Solid)
		{
			stressed = 1f;
		}
		if (sitPos.HasValue)
		{
			pos = sitPos.Value;
			vel *= 0f;
			if (recover > 0)
			{
				recover--;
			}
			else if (Random.value < 1f / Mathf.Lerp(400f, 30f, Mathf.Pow(stressed, 0.5f)) || (creatureAvoider.currentWorstCrit != null && Custom.DistLess(pos, creatureAvoider.currentWorstCrit.DangerPos, 30f)))
			{
				Jump();
			}
		}
		else
		{
			rot = Vector3.Slerp(rot, vel.normalized, 0.1f);
		}
	}

	public void Jump()
	{
		if (sitPos.HasValue && !burrowPos.HasValue)
		{
			float num = Mathf.Pow(Random.value, 0.75f) * ((Random.value < 0.5f) ? (-1f) : 1f);
			if (base.OutOfBounds)
			{
				num = Mathf.Pow(Random.value, 0.75f) * ((mySwarm.placedObject.pos.x > pos.x) ? 1f : (-1f));
			}
			else if (creatureAvoider.currentWorstCrit != null)
			{
				num = Mathf.Pow(Random.value, 0.75f) * ((creatureAvoider.currentWorstCrit.DangerPos.x < pos.x) ? 1f : (-1f));
			}
			if (room.GetTile(room.GetTilePosition(sitPos.Value) + new IntVector2((int)Mathf.Sign(num), 0)).Solid)
			{
				num *= -1f;
			}
			Vector2 vector = Custom.DegToVec(Mathf.Lerp(-20f, 20f, Random.value) + Mathf.Lerp(30f, 60f, Random.value) * num);
			pos = sitPos.Value + vector * 2f;
			vel = vector * Mathf.Lerp(7f, 14f, Mathf.Pow(Random.value, Mathf.Lerp(1.2f, 0.6f, stressed)));
			sitPos = null;
			rot = new Vector2(Mathf.Sign(vector.x), 0f);
			recover = Random.Range(5, 20);
		}
	}

	public override void WallCollision(IntVector2 dir, bool first)
	{
		if (sitPos.HasValue || dir.y >= 1 || wantToBurrow)
		{
			return;
		}
		sitPos = pos;
		Vector2 vector = Custom.RNV() * 0.1f;
		for (int i = 0; i < 8; i++)
		{
			if (room.GetTile(pos + Custom.eightDirections[i].ToVector2() * 20f).Solid)
			{
				vector += Custom.eightDirections[i].ToVector2().normalized;
			}
		}
		legs[0, 0] = (vector + new Vector2(-1f, 0.7f)).normalized;
		legs[1, 0] = (vector + new Vector2(1f, 0.7f)).normalized;
		rot = (Custom.PerpendicularVector(vector) * ((Random.value < 0.5f) ? (-1f) : 1f) + Custom.RNV() * 0.2f).normalized;
	}

	public override void EmergeFromGround(Vector2 emergePos)
	{
		base.EmergeFromGround(emergePos);
		pos = emergePos;
		sitPos = emergePos + new Vector2(0f, 4f);
		Jump();
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		base.InitiateSprites(sLeaser, rCam);
		sLeaser.sprites = new FSprite[3];
		sLeaser.sprites[0] = new FSprite("pixel");
		sLeaser.sprites[0].anchorY = 0.2f;
		for (int i = 0; i < 2; i++)
		{
			sLeaser.sprites[1 + i] = new FSprite("pixel");
			sLeaser.sprites[1 + i].anchorY = 0f;
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
		float num = Mathf.Lerp(lastInGround, inGround, timeStacker);
		Vector2 v = Vector3.Slerp(lastRot, rot, timeStacker);
		vector.y -= 5f * num;
		sLeaser.sprites[0].x = vector.x - camPos.x;
		sLeaser.sprites[0].y = vector.y - camPos.y;
		sLeaser.sprites[0].rotation = Custom.VecToDeg(v);
		sLeaser.sprites[0].scaleX = 1.2f * (1f - num);
		sLeaser.sprites[0].scaleY = 6f * (1f - num);
		for (int i = 0; i < 2; i++)
		{
			Vector2 v2 = Vector3.Slerp(legs[i, 1], legs[i, 0], timeStacker);
			sLeaser.sprites[1 + i].x = vector.x + Custom.PerpendicularVector(v2).x * ((i == 0) ? (-1f) : 1f) * 1f - camPos.x;
			sLeaser.sprites[1 + i].y = vector.y + Custom.PerpendicularVector(v2).y * ((i == 0) ? (-1f) : 1f) * 1f - camPos.y;
			sLeaser.sprites[1 + i].rotation = Custom.VecToDeg(v2);
			sLeaser.sprites[1 + i].scaleY = 3f * (1f - num);
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		base.ApplyPalette(sLeaser, rCam, palette);
		Color color = Color.Lerp(palette.blackColor, Color.Lerp(palette.texture.GetPixel(30, 3), palette.texture.GetPixel(30, 2), colorFac2), colorFac);
		sLeaser.sprites[0].color = color;
		sLeaser.sprites[1].color = color;
		sLeaser.sprites[2].color = color;
	}
}
