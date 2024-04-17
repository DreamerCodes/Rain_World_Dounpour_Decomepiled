using System;
using RWCustom;
using UnityEngine;

namespace CoralBrain;

public class StemSegment : PhysicalObject, IDrawable, IOwnMycelia
{
	public CoralStem stem;

	public StemSegment prevSegment;

	public StemSegment nextSegment;

	public bool first;

	public Vector2 direction;

	public Vector2 lastDirection;

	private float conRad;

	private int meshSegs;

	private float size;

	public int dots;

	public CoralNeuron neuron;

	private Mycelium[] mycelia;

	private Color BaseColor => Color.Lerp(Custom.HSL2RGB(0.025f, Mathf.Lerp(0.4f, 0.1f, Mathf.Pow(size, 0.5f)), Mathf.Lerp(0.05f, 0.7f - 0.5f * room.Darkness(base.firstChunk.pos), Mathf.Pow(size, 0.45f))), new Color(0f, 0f, 0.1f), Mathf.Pow(Mathf.InverseLerp(0.45f, -0.05f, size), 0.9f) * 0.5f);

	private Color HighLightColor => Color.Lerp(Custom.HSL2RGB(0.025f, Mathf.Lerp(0.5f, 0.1f, Mathf.Pow(size, 0.5f)), Mathf.Lerp(0.15f, 0.85f - 0.65f * room.Darkness(base.firstChunk.pos), Mathf.Pow(size, 0.45f))), new Color(0f, 0f, 0.15f), Mathf.Pow(Mathf.InverseLerp(0.45f, -0.05f, size), 0.9f) * 0.4f);

	public Room OwnerRoom => room;

	public StemSegment(AbstractPhysicalObject abstractPhysicalObject, CoralStem stem, float size, Vector2 posA, Vector2 posB)
		: base(abstractPhysicalObject)
	{
		this.size = size;
		this.stem = stem;
		base.bodyChunks = new BodyChunk[2];
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			base.bodyChunks[i] = new BodyChunk(this, i, new Vector2(0f, 0f), Mathf.Lerp(4f, 9f, size), Mathf.Lerp(0.015f, 2.8f, size));
		}
		conRad = Mathf.Lerp(6f, 12f, size);
		base.bodyChunks[0].HardSetPosition(posA);
		base.bodyChunks[1].HardSetPosition(posB);
		base.bodyChunks[0].vel = Custom.RNV();
		bodyChunkConnections = new BodyChunkConnection[1];
		bodyChunkConnections[0] = new BodyChunkConnection(base.bodyChunks[0], base.bodyChunks[1], Mathf.Lerp(6f, 68f, size), BodyChunkConnection.Type.Normal, 1f, 0.5f);
		meshSegs = (int)Custom.LerpMap(bodyChunkConnections[0].distance, 6f, 68f, 5f, 16f);
		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		bounce = 0.5f;
		surfaceFriction = 0.5f;
		collisionLayer = 0;
		base.waterFriction = 0.96f;
		base.buoyancy = 0.95f;
		dots = (int)Mathf.Lerp(1f, 5f, size);
		if (stem.withNeurons && size > 0f && UnityEngine.Random.value > size && UnityEngine.Random.value < 0.33f)
		{
			neuron = new CoralNeuron(stem.system, stem.system.room, Mathf.Lerp(50f, 200f, UnityEngine.Random.value), null, base.bodyChunks[1].pos);
			stem.system.room.AddObject(neuron);
			for (int j = 0; j < neuron.segments.GetLength(0); j++)
			{
				neuron.segments[j, 0] = base.bodyChunks[1].pos;
				neuron.segments[j, 1] = base.bodyChunks[1].pos;
			}
		}
		if (size == 0f)
		{
			mycelia = new Mycelium[20];
		}
		else if (UnityEngine.Random.value < 0.5f)
		{
			mycelia = new Mycelium[2];
		}
		else
		{
			mycelia = new Mycelium[0];
		}
		for (int k = 0; k < mycelia.Length; k++)
		{
			mycelia[k] = new Mycelium(stem.system, this, k, Mathf.Max(Mathf.Lerp(30f, 40f, UnityEngine.Random.value), bodyChunkConnections[0].distance), base.bodyChunks[1].pos);
			mycelia[k].color = new Color(0f, 0f, 0.1f);
			mycelia[k].useStaticCulling = false;
		}
	}

	public override void Update(bool eu)
	{
		if (stem.system.Frozen)
		{
			return;
		}
		base.Update(eu);
		for (int i = 0; i < mycelia.Length; i++)
		{
			Vector2 vector = Custom.DegToVec(Custom.AimFromOneVectorToAnother(base.bodyChunks[0].pos, base.bodyChunks[1].pos) - 30f + (float)i / (float)(mycelia.Length - 1) * 60f);
			mycelia[i].points[1, 0] = Vector2.Lerp(mycelia[i].points[1, 0], base.bodyChunks[1].pos + vector * mycelia[i].conRad, 0.01f);
			mycelia[i].points[1, 2] += vector * 5f;
			mycelia[i].Update();
		}
		if (neuron != null)
		{
			Vector2 vector2 = Custom.DirVec(base.bodyChunks[1].pos, neuron.segments[0, 0]);
			float num = Vector2.Distance(base.bodyChunks[1].pos, neuron.segments[0, 0]);
			float num2 = base.bodyChunks[1].mass / (((IClimbableVine)neuron).Mass(0) + base.bodyChunks[1].mass);
			base.bodyChunks[0].vel += vector2 * (num - 1f) * (1f - num2);
			base.bodyChunks[0].pos += vector2 * (num - 1f) * (1f - num2);
			neuron.segments[0, 2] -= vector2 * (num - 1f) * num2;
			neuron.segments[0, 0] -= vector2 * (num - 1f) * num2;
			if (prevSegment != null)
			{
				num2 = 0.8f;
				vector2 = Custom.DirVec(prevSegment.bodyChunks[0].pos, neuron.segments[1, 0]);
				vector2 *= Mathf.InverseLerp(1f, -1f, Vector2.Dot(Custom.DirVec(base.bodyChunks[0].pos, base.bodyChunks[1].pos), Custom.DirVec(base.bodyChunks[1].pos, neuron.segments[1, 0])));
				prevSegment.bodyChunks[0].vel -= vector2 * (1f - num2) * 1.8f;
				neuron.segments[1, 2] += vector2 * num2 * 1.8f;
			}
		}
		if (prevSegment != null)
		{
			Vector2 vector3 = Custom.DirVec(base.bodyChunks[0].pos, prevSegment.bodyChunks[1].pos);
			float num3 = Vector2.Distance(base.bodyChunks[0].pos, prevSegment.bodyChunks[1].pos);
			float num4 = prevSegment.bodyChunks[1].mass / (prevSegment.bodyChunks[1].mass + base.bodyChunks[0].mass);
			base.bodyChunks[0].pos += vector3 * (num3 - conRad) * num4;
			base.bodyChunks[0].vel += vector3 * (num3 - conRad) * num4;
			prevSegment.bodyChunks[1].pos -= vector3 * (num3 - conRad) * (1f - num4);
			prevSegment.bodyChunks[1].vel -= vector3 * (num3 - conRad) * (1f - num4);
			if (prevSegment.prevSegment != null)
			{
				vector3 = Custom.DirVec(prevSegment.prevSegment.bodyChunks[0].pos, base.bodyChunks[1].pos);
				num4 = prevSegment.prevSegment.bodyChunks[0].mass / (prevSegment.prevSegment.bodyChunks[0].mass + base.bodyChunks[1].mass);
				float num5 = 0.5f + 0.5f * Mathf.InverseLerp(0f, -1f, Vector2.Dot((prevSegment.bodyChunks[0].pos - base.bodyChunks[1].pos).normalized, (prevSegment.bodyChunks[1].pos - base.bodyChunks[0].pos).normalized));
				base.bodyChunks[1].vel += vector3 * 1.6f * num5 * num4;
				prevSegment.prevSegment.bodyChunks[0].vel -= vector3 * 1.6f * num5 * (1f - num4);
			}
		}
		if (first)
		{
			base.bodyChunks[0].collideWithTerrain = false;
			base.bodyChunks[0].pos = stem.rootPos - stem.rootDirection * base.bodyChunks[0].rad;
			base.bodyChunks[0].vel *= 0f;
			base.bodyChunks[1].vel += stem.rootDirection * 2f;
		}
		if (prevSegment != null)
		{
			for (int j = 0; j < 2; j++)
			{
				IntVector2 tilePosition = room.GetTilePosition(base.bodyChunks[j].pos);
				Vector2 vector4 = new Vector2(0f, 0f);
				for (int k = 0; k < 4; k++)
				{
					if (!room.GetTile(tilePosition + Custom.fourDirections[k]).Solid && !room.aimap.getAItile(tilePosition + Custom.fourDirections[k]).narrowSpace)
					{
						float num6 = 0f;
						for (int l = 0; l < 4; l++)
						{
							num6 += (float)Mathf.Min(4, room.aimap.getTerrainProximity(tilePosition + Custom.fourDirections[k] + Custom.fourDirections[l]));
						}
						vector4 += Custom.fourDirections[k].ToVector2() * num6;
					}
				}
				base.bodyChunks[j].vel += vector4.normalized * 0.1f + stem.system.wind * 0.005f;
			}
		}
		lastDirection = direction;
		direction = Custom.DirVec(base.bodyChunks[0].pos, base.bodyChunks[1].pos);
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[3 + mycelia.Length + dots * 3];
		for (int i = 0; i < mycelia.Length; i++)
		{
			mycelia[i].InitiateSprites(i, sLeaser, rCam);
		}
		sLeaser.sprites[mycelia.Length] = new FSprite("Circle20");
		sLeaser.sprites[1 + mycelia.Length] = TriangleMesh.MakeLongMesh(meshSegs, pointyTip: false, customColor: false);
		sLeaser.sprites[2 + mycelia.Length] = TriangleMesh.MakeLongMesh(meshSegs, pointyTip: false, customColor: false);
		sLeaser.sprites[mycelia.Length].scaleX = base.bodyChunks[1].rad / 20f;
		int num = 3 + mycelia.Length;
		for (int j = 0; j < dots; j++)
		{
			for (int k = 0; k < 3; k++)
			{
				sLeaser.sprites[num + k] = new FSprite("Circle20");
				sLeaser.sprites[num + k].scale = Mathf.Lerp(1.2f, 7f, size) * ((k == 2) ? 0.9f : 1f) / 20f;
			}
			num += 3;
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		for (int i = 0; i < mycelia.Length; i++)
		{
			mycelia[i].DrawSprites(i, sLeaser, rCam, timeStacker, camPos);
		}
		Vector2 vector = Vector2.Lerp(base.bodyChunks[0].lastPos, base.bodyChunks[0].pos, timeStacker);
		Vector2 vector2 = Vector2.Lerp(base.bodyChunks[1].lastPos, base.bodyChunks[1].pos, timeStacker);
		Vector2 cA = vector;
		Vector2 cB = vector2;
		Vector2 vector3;
		if (prevSegment != null)
		{
			vector3 = Custom.DirVec(Vector2.Lerp(prevSegment.bodyChunks[1].lastPos, prevSegment.bodyChunks[1].pos, timeStacker), vector);
			vector = Vector2.Lerp(vector, Vector2.Lerp(prevSegment.bodyChunks[1].lastPos, prevSegment.bodyChunks[1].pos, timeStacker), 0.35f);
			cA = vector + vector3 * bodyChunkConnections[0].distance * 0.45f;
		}
		else
		{
			vector3 = Vector2.Lerp(lastDirection, direction, timeStacker).normalized;
			vector -= vector3 * bodyChunkConnections[0].distance * 0.15f;
		}
		Vector2 vector5;
		if (nextSegment != null)
		{
			Vector2 vector4 = Vector2.Lerp(nextSegment.bodyChunks[0].lastPos, nextSegment.bodyChunks[0].pos, timeStacker);
			vector5 = Custom.DirVec(vector2, vector4);
			vector2 = Vector2.Lerp(vector2, vector4, 0.35f);
			cB = vector2 - vector5 * bodyChunkConnections[0].distance * 0.45f;
			sLeaser.sprites[mycelia.Length].x = Mathf.Lerp(vector2.x, vector4.x, 0.5f) - camPos.x;
			sLeaser.sprites[mycelia.Length].y = Mathf.Lerp(vector2.y, vector4.y, 0.5f) - camPos.y;
			sLeaser.sprites[mycelia.Length].scaleY = Vector2.Distance(vector2, vector4) / 20f;
			sLeaser.sprites[mycelia.Length].rotation = Custom.AimFromOneVectorToAnother(vector2, vector4);
		}
		else
		{
			vector5 = Vector2.Lerp(lastDirection, direction, timeStacker).normalized;
			vector2 += vector5 * bodyChunkConnections[0].distance * 0.15f;
		}
		Vector2 vector6 = vector;
		Vector2 v = vector3;
		float num = 0.5f;
		Vector2 b = Custom.Bezier(vector, cA, vector2, cB, 0.5f);
		for (int j = 1; j <= meshSegs; j++)
		{
			float num2 = (float)j / (float)(meshSegs - 1);
			float num3 = 0.6f + 0.5f * (1f - Mathf.Sin(num2 * (float)Math.PI)) + 0.3f * Mathf.Max(Mathf.Sin(Mathf.InverseLerp(0f, 0.3f, num2) * (float)Math.PI), Mathf.Sin(Mathf.InverseLerp(0.7f, 1f, num2) * (float)Math.PI));
			if (num2 == 1f)
			{
				num3 = 0.5f;
			}
			Vector2 vector7 = Custom.Bezier(vector, cA, vector2, cB, num2);
			Vector2 vector8 = ((!(num2 < 1f)) ? vector5 : Custom.DirVec(vector7, Custom.Bezier(vector, cA, vector2, cB, (float)(j + 1) / (float)(meshSegs - 1))));
			(sLeaser.sprites[mycelia.Length + 1] as TriangleMesh).MoveVertice(j * 4 - 4, (vector6 + vector7) / 2f + Custom.PerpendicularVector(v) * base.bodyChunks[0].rad * (num3 + num) * 0.5f - camPos);
			(sLeaser.sprites[mycelia.Length + 1] as TriangleMesh).MoveVertice(j * 4 - 3, (vector6 + vector7) / 2f - Custom.PerpendicularVector(v) * base.bodyChunks[0].rad * (num3 + num) * 0.5f - camPos);
			(sLeaser.sprites[mycelia.Length + 1] as TriangleMesh).MoveVertice(j * 4 - 2, vector7 + Custom.PerpendicularVector(vector8) * base.bodyChunks[0].rad * num3 - camPos);
			(sLeaser.sprites[mycelia.Length + 1] as TriangleMesh).MoveVertice(j * 4 - 1, vector7 - Custom.PerpendicularVector(vector8) * base.bodyChunks[0].rad * num3 - camPos);
			for (int k = 1; k < 5; k++)
			{
				if (k < 2)
				{
					(sLeaser.sprites[mycelia.Length + 2] as TriangleMesh).MoveVertice(j * 4 - k, Vector2.Lerp((sLeaser.sprites[mycelia.Length + 1] as TriangleMesh).vertices[j * 4 - k] + camPos, Vector2.Lerp(vector7, b, 0.5f), 0.5f) - camPos + new Vector2((0f - num3) * 0.25f, num3 * 0.25f) * base.bodyChunks[0].rad);
				}
				else
				{
					(sLeaser.sprites[mycelia.Length + 2] as TriangleMesh).MoveVertice(j * 4 - k, Vector2.Lerp((sLeaser.sprites[mycelia.Length + 1] as TriangleMesh).vertices[j * 4 - k] + camPos, Vector2.Lerp(vector6, b, 0.5f), 0.5f) - camPos + new Vector2((0f - num) * 0.25f, num * 0.25f) * base.bodyChunks[0].rad);
				}
			}
			vector6 = vector7;
			v = vector8;
			num = num3;
		}
		int num4 = mycelia.Length + 3;
		for (int l = 0; l < dots; l++)
		{
			Vector2 vector9 = Custom.Bezier(vector, cA, vector2, cB, (dots == 1) ? 0.5f : Mathf.Lerp((float)l / (float)(dots - 1), 0.5f, Custom.LerpMap(dots, 1f, 5f, 0.6f, 0.35f)));
			for (int m = 0; m < 3; m++)
			{
				sLeaser.sprites[num4 + m].x = vector9.x - camPos.x - ((m == 2) ? 0f : Mathf.Lerp(1f, 1.5f, size)) * ((m == 0) ? 1f : (-1f));
				sLeaser.sprites[num4 + m].y = vector9.y - camPos.y + ((m == 2) ? 0f : Mathf.Lerp(1f, 1.5f, size)) * ((m == 0) ? 1f : (-1f));
			}
			sLeaser.sprites[num4 + 2].color = ((UnityEngine.Random.value < 0.5f) ? Custom.HSL2RGB(2f / 3f, 1f, 0.1f + 0.4f * UnityEngine.Random.value) : new Color(0f, 0f, 0.1f));
			num4 += 3;
		}
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		sLeaser.sprites[mycelia.Length].color = Custom.HSL2RGB(0.025f, 1f, 0.1f);
		sLeaser.sprites[mycelia.Length + 1].color = BaseColor;
		sLeaser.sprites[mycelia.Length + 2].color = HighLightColor;
		int num = 3 + mycelia.Length;
		for (int i = 0; i < dots; i++)
		{
			sLeaser.sprites[num].color = BaseColor;
			sLeaser.sprites[num + 1].color = HighLightColor;
			num += 3;
		}
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i].RemoveFromContainer();
		}
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Items");
		}
		for (int j = 0; j < sLeaser.sprites.Length; j++)
		{
			newContatiner.AddChild(sLeaser.sprites[j]);
		}
		if (rCam.room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.WaterLights) > 0f)
		{
			FContainer fContainer = rCam.ReturnFContainer("Water");
			int num = 3 + mycelia.Length;
			for (int k = 0; k < dots; k++)
			{
				sLeaser.sprites[num + 2].RemoveFromContainer();
				fContainer.AddChild(sLeaser.sprites[num + 2]);
				num += 3;
			}
		}
	}

	public Vector2 ConnectionPos(int index, float timeStacker)
	{
		return Vector2.Lerp(base.bodyChunks[1].lastPos, base.bodyChunks[1].pos, timeStacker);
	}

	public Vector2 ResetDir(int index)
	{
		return default(Vector2);
	}
}
