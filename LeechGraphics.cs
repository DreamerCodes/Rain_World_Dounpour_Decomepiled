using System;
using RWCustom;
using UnityEngine;

public class LeechGraphics : GraphicsModule
{
	private GenericBodyPart[] body;

	private float[] radiuses;

	private float sinCounter;

	private Color blackColor;

	private int vibrate;

	private Leech leech => base.owner as Leech;

	private float Radius(float bodyPos)
	{
		if (leech.seaLeech)
		{
			return 1.5f + Mathf.Sin(bodyPos * (float)Math.PI) * 1.5f;
		}
		return 2f + Mathf.Sin(bodyPos * (float)Math.PI);
	}

	public LeechGraphics(PhysicalObject ow)
		: base(ow, internalContainers: false)
	{
		body = new GenericBodyPart[leech.seaLeech ? 11 : 5];
		bodyParts = new BodyPart[leech.seaLeech ? 11 : 5];
		for (int i = 0; i < body.Length; i++)
		{
			body[i] = new GenericBodyPart(this, 1f, 0.7f, 1f, leech.mainBodyChunk);
			bodyParts[i] = body[i];
		}
		radiuses = new float[body.Length];
		for (int j = 0; j < body.Length; j++)
		{
			radiuses[j] = Radius((float)j / (float)body.Length);
		}
		sinCounter = UnityEngine.Random.value;
	}

	public override void Reset()
	{
		for (int i = 0; i < body.Length; i++)
		{
			body[i].pos = leech.mainBodyChunk.pos + Custom.DegToVec(UnityEngine.Random.value * 360f);
			body[i].vel = leech.mainBodyChunk.vel;
		}
		base.Reset();
	}

	public override void Update()
	{
		base.Update();
		if (culled)
		{
			return;
		}
		float num = 4f;
		if (!leech.dead && !leech.ChargingAttack && !leech.Attacking)
		{
			sinCounter -= 1f / 15f;
		}
		if (vibrate > 0)
		{
			vibrate--;
		}
		Vector2 vector = Custom.PerpendicularVector(leech.swimDirection);
		float num2 = Mathf.Lerp(0.99f, 0.8f, leech.mainBodyChunk.submersion);
		float num3 = 0.9f * (1f - leech.mainBodyChunk.submersion);
		for (int num4 = body.Length - 1; num4 >= 0; num4--)
		{
			body[num4].Update();
		}
		for (int num5 = body.Length - 1; num5 >= 0; num5--)
		{
			body[num5].vel *= num2;
			body[num5].vel.y -= num3;
			if (leech.mainBodyChunk.submersion == 0f && leech.mainBodyChunk.ContactPoint.y == -1)
			{
				body[num5].vel.x -= (float)leech.landWalkDir * leech.landWalkCycle;
				if (num5 == 2 || num5 == 1)
				{
					body[num5].vel.y += num3 * (1f + Mathf.Sin(leech.landWalkCycle * (float)Math.PI * 2f));
				}
			}
			if (!Custom.DistLess(body[num5].pos, leech.mainBodyChunk.pos, 7f * (float)(num5 + 1)))
			{
				body[num5].pos = leech.mainBodyChunk.pos + Custom.DirVec(leech.mainBodyChunk.pos, body[num5].pos) * 7f * (num5 + 1);
			}
			Vector2 pos = leech.mainBodyChunk.pos;
			if (num5 > 0)
			{
				pos = body[num5 - 1].pos;
			}
			body[num5].vel += Custom.DirVec(pos, body[num5].pos) * 0.1f - leech.swimDirection * (leech.seaLeech ? 0.005f : 0.05f);
			if (!leech.dead && (leech.grasps[0] != null || leech.airDrown > 0.7f) && UnityEngine.Random.value < 0.1f)
			{
				body[num5].vel += Custom.DegToVec(UnityEngine.Random.value * 360f) * UnityEngine.Random.value * UnityEngine.Random.value;
			}
			if (num5 == 0)
			{
				body[num5].pos = leech.mainBodyChunk.pos + leech.swimDirection * 4f;
				if (leech.Consious)
				{
					if (leech.grasps[0] == null && leech.mainBodyChunk.submersion > 0.5f)
					{
						body[num5].pos += vector * Mathf.Sin(sinCounter * (float)Math.PI * 2f) * 2f;
					}
					else if (leech.mainBodyChunk.submersion == 0f && leech.mainBodyChunk.ContactPoint.y == -1)
					{
						body[num5].pos += new Vector2(1.5f * Mathf.Sin(leech.landWalkCycle * (float)Math.PI * 2f) * (0f - (float)leech.landWalkDir), -3f);
					}
				}
				body[num5].vel = leech.mainBodyChunk.vel;
			}
			else
			{
				float num6 = Vector2.Distance(body[num5].pos, body[num5 - 1].pos);
				Vector2 vector2 = Custom.DirVec(body[num5].pos, body[num5 - 1].pos);
				body[num5].vel -= (num - num6) * vector2 * 0.5f;
				body[num5].pos -= (num - num6) * vector2 * (leech.seaLeech ? 0.5f : 0.15f);
				body[num5 - 1].vel += (num - num6) * vector2 * 0.5f;
				body[num5 - 1].pos += (num - num6) * vector2 * (leech.seaLeech ? 0.5f : 0.15f);
				radiuses[num5] = Radius((float)num5 / (float)body.Length);
				if (!leech.Attacking)
				{
					if (leech.mainBodyChunk.submersion == 1f && leech.grasps[0] == null)
					{
						body[num5].vel += vector * Mathf.Sin((sinCounter + (float)num5 / (leech.seaLeech ? 5f : 3f)) * (float)Math.PI * 2f) * (leech.seaLeech ? 1.2f : 0.2f);
					}
					radiuses[num5] *= 1f + Mathf.Sin((sinCounter + (float)num5 / 4f) * (float)Math.PI * 2f) * 0.2f;
				}
				if (num6 > num)
				{
					radiuses[num5] /= 1f + Mathf.Pow((num6 - num) * (leech.seaLeech ? 0.06f : 0.1f), 1.5f);
				}
				else if (!leech.seaLeech)
				{
					radiuses[num5] *= Mathf.Clamp(Mathf.Pow(1f + (num - num6) * 1.5f, 0.5f), 1f, 3f);
				}
			}
		}
		if (leech.seaLeech)
		{
			for (int i = 1; i < body.Length; i++)
			{
				float num7 = Vector2.Distance(body[i].pos, body[i - 1].pos);
				Vector2 vector3 = Custom.DirVec(body[i].pos, body[i - 1].pos);
				body[i].vel -= (num - num7) * vector3 * 0.5f;
				body[i].pos -= (num - num7) * vector3 * 0.5f;
				body[i - 1].vel += (num - num7) * vector3 * 0.5f;
				body[i - 1].pos += (num - num7) * vector3 * 0.5f;
			}
		}
	}

	public void Vibrate()
	{
		if (leech.Consious)
		{
			vibrate = 5;
		}
		for (int i = 0; i < body.Length; i++)
		{
			body[i].pos += Custom.DegToVec(UnityEngine.Random.value * 360f) * (leech.dead ? 3f : 6f);
			body[i].vel = Custom.DegToVec(UnityEngine.Random.value * 360f) * (leech.dead ? 3f : 6f);
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1];
		TriangleMesh.Triangle[] array = new TriangleMesh.Triangle[(body.Length - 1) * 4 + 1];
		for (int i = 0; i < body.Length - 1; i++)
		{
			int num = i * 4;
			for (int j = 0; j < 4; j++)
			{
				array[num + j] = new TriangleMesh.Triangle(num + j, num + j + 1, num + j + 2);
			}
		}
		array[(body.Length - 1) * 4] = new TriangleMesh.Triangle((body.Length - 1) * 4, (body.Length - 1) * 4 + 1, (body.Length - 1) * 4 + 2);
		sLeaser.sprites[0] = new TriangleMesh("Futile_White", array, customColor: false);
		AddToContainer(sLeaser, rCam, null);
		base.InitiateSprites(sLeaser, rCam);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		if (culled)
		{
			return;
		}
		Vector2 vector = Vector2.Lerp(body[0].lastPos, body[0].pos, timeStacker) + leech.swimDirection * 2f;
		float num = radiuses[0];
		if (leech.grasps[0] != null)
		{
			vector = leech.grasps[0].grabbedChunk.pos + Custom.DirVec(leech.grasps[0].grabbedChunk.pos, leech.mainBodyChunk.pos) * leech.grasps[0].grabbedChunk.rad * 0.6f;
			num = 0f;
		}
		if (leech.seaLeech)
		{
			sLeaser.sprites[0].color = Color.Lerp(new Color(0f, 0.4f, 0.8f), blackColor, 0.85f);
		}
		else if (leech.jungleLeech)
		{
			sLeaser.sprites[0].color = Color.Lerp(new Color(0.1f, 0.8f, 0.2f), blackColor, 0.85f);
		}
		else
		{
			sLeaser.sprites[0].color = Color.Lerp(new Color(0.8f, 0f, 0.1f), blackColor, 0.5f + 0.5f * Mathf.InverseLerp(0.05f, 0.2f, leech.airDrown));
		}
		for (int i = 0; i < body.Length; i++)
		{
			Vector2 vector2 = Vector2.Lerp(body[i].lastPos, body[i].pos, timeStacker);
			if (vibrate > 0)
			{
				vector2 += Custom.DegToVec(UnityEngine.Random.value * 360f) * UnityEngine.Random.value * 5f;
			}
			Vector2 normalized = (vector2 - vector).normalized;
			Vector2 vector3 = Custom.PerpendicularVector(normalized);
			float num2 = Vector2.Distance(vector2, vector) / 5f;
			(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4, vector - vector3 * (num + radiuses[i]) * 0.5f + normalized * num2 - camPos);
			(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 1, vector + vector3 * (num + radiuses[i]) * 0.5f + normalized * num2 - camPos);
			if (i < body.Length - 1)
			{
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 2, vector2 - vector3 * radiuses[i] - normalized * num2 - camPos);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 3, vector2 + vector3 * radiuses[i] - normalized * num2 - camPos);
			}
			else
			{
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 2, vector2 - camPos);
			}
			num = radiuses[i];
			vector = vector2;
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		blackColor = palette.blackColor;
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		base.AddToContainer(sLeaser, rCam, newContatiner);
	}
}
