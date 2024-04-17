using System;
using RWCustom;
using UnityEngine;

public class WaterGlowworm : CosmeticInsect
{
	public float stressed;

	public Vector2 rot;

	public Vector2 lastRot;

	private Vector2 swimDir;

	private float breath;

	private float lastBreath;

	public Vector2[,] segments;

	public WaterGlowworm(Room room, Vector2 pos)
		: base(room, pos, Type.WaterGlowworm)
	{
		creatureAvoider = new CreatureAvoider(this, 10, 300f, 0.3f);
		breath = UnityEngine.Random.value;
		segments = new Vector2[UnityEngine.Random.Range(2, 4), 3];
		Reset(pos);
	}

	public override void Update(bool eu)
	{
		if (room != null && room.PointDeferred(pos))
		{
			return;
		}
		lastRot = rot;
		lastBreath = breath;
		base.Update(eu);
		if (submerged)
		{
			vel *= 0.8f;
		}
		else
		{
			vel.y -= 0.9f;
		}
		for (int i = 0; i < segments.GetLength(0); i++)
		{
			segments[i, 1] = segments[i, 0];
			segments[i, 0] += segments[i, 2];
			if (room.PointSubmerged(segments[i, 0]))
			{
				segments[i, 2] *= 0.8f;
			}
			else
			{
				segments[i, 2].y -= 0.9f;
			}
			if (i == 0)
			{
				Vector2 vector = Custom.DirVec(segments[i, 0], pos);
				float num = Vector2.Distance(segments[i, 0], pos);
				pos += vector * (4f - num) * 0.5f;
				vel += vector * (4f - num) * 0.5f;
				segments[i, 0] -= vector * (4f - num) * 0.5f;
				segments[i, 2] -= vector * (4f - num) * 0.5f;
				vel += vector * Mathf.Lerp(0.8f, 1.2f, stressed);
			}
			else
			{
				Vector2 vector2 = Custom.DirVec(segments[i, 0], segments[i - 1, 0]);
				float num2 = Vector2.Distance(segments[i, 0], segments[i - 1, 0]);
				segments[i - 1, 0] += vector2 * (4f - num2) * 0.5f;
				segments[i - 1, 2] += vector2 * (4f - num2) * 0.5f;
				segments[i, 0] -= vector2 * (4f - num2) * 0.5f;
				segments[i, 2] -= vector2 * (4f - num2) * 0.5f;
				segments[i - 1, 2] += vector2 * Mathf.Lerp(0.8f, 1.2f, stressed);
			}
		}
	}

	public override void Reset(Vector2 resetPos)
	{
		base.Reset(resetPos);
		for (int i = 0; i < segments.GetLength(0); i++)
		{
			segments[i, 0] = resetPos + Custom.RNV();
			segments[i, 1] = resetPos;
			segments[i, 2] = Custom.RNV() * UnityEngine.Random.value;
		}
	}

	public override void Act()
	{
		base.Act();
		breath -= 1f / Mathf.Lerp(60f, 10f, stressed);
		float num = Mathf.Pow(creatureAvoider.FleeSpeed, 0.3f);
		if (num > stressed)
		{
			stressed = Custom.LerpAndTick(stressed, num, 0.05f, 1f / 60f);
		}
		else
		{
			stressed = Custom.LerpAndTick(stressed, num, 0.02f, 0.005f);
		}
		if (submerged)
		{
			swimDir += Custom.RNV() * UnityEngine.Random.value * 0.5f;
			if (wantToBurrow)
			{
				swimDir.y -= 0.5f;
			}
			if (pos.x < 0f)
			{
				swimDir.x += 1f;
			}
			else if (pos.x > room.PixelWidth)
			{
				swimDir.x -= 1f;
			}
			if (pos.y < 0f)
			{
				swimDir.y += 1f;
			}
			if (creatureAvoider.currentWorstCrit != null)
			{
				swimDir -= Custom.DirVec(pos, creatureAvoider.currentWorstCrit.DangerPos) * creatureAvoider.FleeSpeed;
			}
			if (room.water)
			{
				swimDir = Vector3.Slerp(swimDir, new Vector2(0f, -1f), Mathf.InverseLerp(room.FloatWaterLevel(pos.x) - 100f, room.FloatWaterLevel(pos.x), pos.y) * 0.5f);
			}
			swimDir.Normalize();
			vel += swimDir * Mathf.Lerp(0.8f, 1.1f, stressed) + Custom.RNV() * UnityEngine.Random.value * 0.1f;
		}
		rot = Vector3.Slerp(rot, (-vel - swimDir).normalized, 0.2f);
	}

	public override void WallCollision(IntVector2 dir, bool first)
	{
		swimDir -= Custom.RNV() * UnityEngine.Random.value + dir.ToVector2();
		swimDir.Normalize();
	}

	public override void EmergeFromGround(Vector2 emergePos)
	{
		base.EmergeFromGround(emergePos);
		pos = emergePos;
		swimDir = new Vector2(0f, 1f);
	}

	private int LegSprite(int segment, int leg)
	{
		return segment * 2 + leg;
	}

	private int SegmentSprite(int segment, int part)
	{
		return (segments.GetLength(0) + 1) * 2 + segment * 2 + part;
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		base.InitiateSprites(sLeaser, rCam);
		sLeaser.sprites = new FSprite[(segments.GetLength(0) + 1) * 4];
		for (int i = 0; i < segments.GetLength(0) + 1; i++)
		{
			sLeaser.sprites[SegmentSprite(i, 0)] = new FSprite("Circle20");
			sLeaser.sprites[SegmentSprite(i, 1)] = new FSprite("Circle20");
			sLeaser.sprites[SegmentSprite(i, 0)].anchorY = 0.3f;
			sLeaser.sprites[SegmentSprite(i, 1)].anchorY = 0.4f;
			sLeaser.sprites[LegSprite(i, 0)] = new FSprite("pixel");
			sLeaser.sprites[LegSprite(i, 1)] = new FSprite("pixel");
			sLeaser.sprites[LegSprite(i, 0)].anchorY = 0f;
			sLeaser.sprites[LegSprite(i, 1)].anchorY = 0f;
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		if (room != null && room.PointDeferred(pos))
		{
			return;
		}
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		float num = Mathf.Lerp(lastInGround, inGround, timeStacker);
		for (int i = 0; i < segments.GetLength(0) + 1; i++)
		{
			float num2 = Mathf.Sin((Mathf.Lerp(lastBreath, breath, timeStacker) + 0.04f * (float)i) * 2f * (float)Math.PI);
			float t = 0.5f + 0.5f * Mathf.Sin((Mathf.Lerp(lastBreath, breath, timeStacker) + 0.1f * (float)i) * 7f * (float)Math.PI);
			Vector2 p = ((i == 0) ? Vector2.Lerp(lastPos, pos, timeStacker) : Vector2.Lerp(segments[i - 1, 1], segments[i - 1, 0], timeStacker));
			Vector2 v = i switch
			{
				0 => -Vector3.Slerp(lastRot, rot, timeStacker), 
				1 => Custom.DirVec(p, Vector2.Lerp(lastPos, pos, timeStacker)), 
				_ => Custom.DirVec(p, Vector2.Lerp(segments[i - 2, 1], segments[i - 2, 0], timeStacker)), 
			};
			p.y -= 5f * num;
			float num3 = Custom.LerpMap(i, 0f, segments.GetLength(0), 1f, 0.5f, 1.2f) * (1f - num);
			for (int j = 0; j < 2; j++)
			{
				sLeaser.sprites[SegmentSprite(i, j)].x = p.x - camPos.x;
				sLeaser.sprites[SegmentSprite(i, j)].y = p.y - camPos.y;
				sLeaser.sprites[SegmentSprite(i, j)].rotation = Custom.VecToDeg(v);
				sLeaser.sprites[LegSprite(i, j)].x = p.x - Custom.PerpendicularVector(v).x * 2f * num3 * ((j == 0) ? (-1f) : 1f) - camPos.x;
				sLeaser.sprites[LegSprite(i, j)].y = p.y - Custom.PerpendicularVector(v).y * 2f * num3 * ((j == 0) ? (-1f) : 1f) - camPos.y;
				sLeaser.sprites[LegSprite(i, j)].rotation = Custom.VecToDeg(v) + (Mathf.Lerp(-20f, 70f, t) + Custom.LerpMap(i, 0f, segments.GetLength(0), 70f, 140f)) * ((j == 0) ? (-1f) : 1f);
				sLeaser.sprites[LegSprite(i, j)].scaleY = Mathf.Lerp(3.5f + (float)(i * 2), 3f, Mathf.Sin(Mathf.InverseLerp(0f, segments.GetLength(0), i) * (float)Math.PI)) * num3;
			}
			sLeaser.sprites[SegmentSprite(i, 0)].scaleX = 4f * num3 * (1f - num) / 20f;
			sLeaser.sprites[SegmentSprite(i, 0)].scaleY = 6.5f * num3 * (1f - num) / 20f;
			sLeaser.sprites[SegmentSprite(i, 1)].scaleX = 3f * num3 * (1f - num) * num2 / 20f;
			sLeaser.sprites[SegmentSprite(i, 1)].scaleY = 5.5f * num3 * (1f - num) * num2 / 20f;
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		base.ApplyPalette(sLeaser, rCam, palette);
		for (int i = 0; i < segments.GetLength(0) + 1; i++)
		{
			sLeaser.sprites[SegmentSprite(i, 0)].color = palette.blackColor;
			sLeaser.sprites[SegmentSprite(i, 1)].color = new Color(0f, 0.003921569f, 0f);
		}
	}
}
