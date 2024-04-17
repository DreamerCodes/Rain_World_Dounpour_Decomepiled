using System;
using RWCustom;
using UnityEngine;

namespace CoralBrain;

public class CoralNeuron : UpdatableAndDeletable, IClimbableVine, IDrawable, IOwnMycelia, IOwnProjectedCircles
{
	public CoralNeuronSystem system;

	public Vector2[,] segments;

	public float conRad;

	public Vector2[] bumps;

	public float[,] bumpPings;

	public Vector2? posA;

	public Vector2? posB;

	public Vector2? rootDirA;

	public Vector2? rootDirB;

	public Mycelium[,] mycelia;

	public Room OwnerRoom => room;

	public Color MeshColor(float f)
	{
		f = Mathf.Abs(f - 0.5f) * 2f;
		return Custom.HSL2RGB(Custom.Decimal(Mathf.Lerp(1.025f, 0.9638889f, 0.5f + 0.5f * Mathf.Pow(f, 3f))), Custom.LerpMap(f, 0.8f, 1f, 1f, 0.5f), Custom.LerpMap(f, 0.7f, 1f, 0.5f, 0.15f));
	}

	public CoralNeuron(CoralNeuronSystem system, Room room, float length, Vector2? posA, Vector2? posB)
	{
		this.system = system;
		base.room = room;
		this.posA = posA;
		this.posB = posB;
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState((int)length);
		if (posA.HasValue)
		{
			IntVector2 tilePosition = room.GetTilePosition(posA.Value);
			for (int i = 0; i < 4; i++)
			{
				if (room.GetTile(tilePosition + Custom.fourDirections[i]).Solid)
				{
					rootDirA = -Custom.fourDirections[i].ToVector2();
					posA = ((Custom.fourDirections[i].x == 0) ? new Vector2?(new Vector2(posA.Value.x, room.MiddleOfTile(tilePosition).y - rootDirA.Value.y * 20f)) : new Vector2?(new Vector2(room.MiddleOfTile(tilePosition).x - rootDirA.Value.x * 20f, posA.Value.y)));
				}
			}
		}
		if (posB.HasValue)
		{
			IntVector2 tilePosition2 = room.GetTilePosition(posB.Value);
			for (int j = 0; j < 4; j++)
			{
				if (room.GetTile(tilePosition2 + Custom.fourDirections[j]).Solid)
				{
					rootDirB = -Custom.fourDirections[j].ToVector2();
					posB = ((Custom.fourDirections[j].x == 0) ? new Vector2?(new Vector2(posB.Value.x, room.MiddleOfTile(tilePosition2).y - rootDirB.Value.y * 20f)) : new Vector2?(new Vector2(room.MiddleOfTile(tilePosition2).x - rootDirB.Value.x * 20f, posB.Value.y)));
				}
			}
		}
		if (room.climbableVines == null)
		{
			room.climbableVines = new ClimbableVinesSystem();
			room.AddObject(room.climbableVines);
		}
		room.climbableVines.vines.Add(this);
		segments = new Vector2[(int)Mathf.Clamp(length / 20f, 1f, 200f), 3];
		for (int k = 0; k < segments.GetLength(0); k++)
		{
			float t = (float)k / (float)(segments.GetLength(0) - 1);
			if (posA.HasValue && posB.HasValue)
			{
				segments[k, 0] = Vector2.Lerp(posA.Value, posB.Value, t) + Custom.RNV() * UnityEngine.Random.value;
			}
			else if (posB.HasValue)
			{
				segments[k, 0] = posB.Value;
			}
			segments[k, 1] = segments[k, 0];
			segments[k, 2] = Custom.RNV() * UnityEngine.Random.value;
		}
		mycelia = new Mycelium[Math.Min(segments.GetLength(0) - 2, 20), 2];
		for (int l = 0; l < mycelia.GetLength(0); l++)
		{
			for (int m = 0; m < 2; m++)
			{
				mycelia[l, m] = new Mycelium(system, this, l * 2 + m, Mathf.Min(Mathf.Lerp(30f, 300f, UnityEngine.Random.value), Mathf.Lerp(30f, 300f, MycLengthContour(l))), segments[SegmentOfMycelium(l * 2 + 1), 0]);
				mycelia[l, m].color = MeshColor(Mathf.InverseLerp(0f, segments.GetLength(0) - 1, SegmentOfMycelium(l * 2 + m)));
				mycelia[l, m].useStaticCulling = false;
			}
		}
		conRad = length / (float)segments.GetLength(0) * 1.5f;
		bumps = new Vector2[(int)((float)segments.GetLength(0) * 1.2f)];
		bumpPings = new float[bumps.Length, 2];
		for (int n = 0; n < bumps.Length; n++)
		{
			bumps[n] = new Vector2(Mathf.Lerp(-1f, 1f, UnityEngine.Random.value), UnityEngine.Random.value);
		}
		if (room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.SuperStructureProjector) > 0f)
		{
			if (!rootDirA.HasValue && (rootDirB.HasValue || length > 100f))
			{
				room.AddObject(new ProjectedCircle(room, this, 0, Mathf.InverseLerp(40f, 700f, length)));
			}
			if (!rootDirB.HasValue)
			{
				room.AddObject(new ProjectedCircle(room, this, 1, Mathf.InverseLerp(40f, 700f, length)));
			}
		}
		UnityEngine.Random.state = state;
	}

	private float MycLengthContour(int i)
	{
		if (rootDirA.HasValue && rootDirB.HasValue)
		{
			return Mathf.Min(Mathf.InverseLerp(0f, 4f, i), Mathf.InverseLerp(mycelia.GetLength(0) - 1, mycelia.GetLength(0) - 5, i));
		}
		if (rootDirA.HasValue && !rootDirB.HasValue)
		{
			return Mathf.InverseLerp(0f, mycelia.GetLength(0) / 2, i);
		}
		if (!rootDirA.HasValue && rootDirB.HasValue)
		{
			return Mathf.InverseLerp(mycelia.GetLength(0) - 1, mycelia.GetLength(0) / 2, i);
		}
		return Mathf.Max(Mathf.InverseLerp(10f, 0f, i), Mathf.InverseLerp(mycelia.GetLength(0) - 11, mycelia.GetLength(0) - 1, i));
	}

	public override void Update(bool eu)
	{
		if (system.Frozen)
		{
			return;
		}
		base.Update(eu);
		for (int i = 2; i < segments.GetLength(0); i++)
		{
			Vector2 vector = Custom.DirVec(segments[i - 2, 0], segments[i, 0]);
			segments[i - 2, 2] -= vector * 0.15f;
			segments[i, 2] += vector * 0.15f;
		}
		for (int j = 0; j < segments.GetLength(0); j++)
		{
			float num = (float)j / (float)(segments.GetLength(0) - 1);
			segments[j, 1] = segments[j, 0];
			segments[j, 0] += segments[j, 2];
			segments[j, 2] *= 0.999f;
			if (room.aimap.getTerrainProximity(segments[j, 0]) < 4)
			{
				IntVector2 tilePosition = room.GetTilePosition(segments[j, 0]);
				Vector2 vector2 = new Vector2(0f, 0f);
				for (int k = 0; k < 4; k++)
				{
					if (!room.GetTile(tilePosition + Custom.fourDirections[k]).Solid && !room.aimap.getAItile(tilePosition + Custom.fourDirections[k]).narrowSpace)
					{
						float num2 = 0f;
						for (int l = 0; l < 4; l++)
						{
							num2 += (float)room.aimap.getTerrainProximity(tilePosition + Custom.fourDirections[k] + Custom.fourDirections[l]);
						}
						vector2 += Custom.fourDirections[k].ToVector2() * num2;
					}
				}
				segments[j, 2] += vector2.normalized * Custom.LerpMap(room.aimap.getTerrainProximity(segments[j, 0]), 0f, 3f, 2f, 0.2f);
			}
			segments[j, 2] += system.wind * 0.005f;
			if (num < 0.5f)
			{
				if (rootDirA.HasValue)
				{
					segments[j, 2] += rootDirA.Value * Mathf.InverseLerp(0.25f, 0f, num) * 0.5f;
				}
				else if (posA.HasValue)
				{
					segments[j, 2] += Vector2.ClampMagnitude(posA.Value - segments[j, 0], 40f) / 420f * Mathf.InverseLerp(0.25f, 0f, num);
				}
			}
			else if (num > 0.5f)
			{
				if (rootDirB.HasValue)
				{
					segments[j, 2] += rootDirB.Value * Mathf.InverseLerp(0.75f, 1f, num) * 0.5f;
				}
				else if (posB.HasValue)
				{
					segments[j, 2] += Vector2.ClampMagnitude(posB.Value - segments[j, 0], 40f) / 420f * Mathf.InverseLerp(0.75f, 1f, num);
				}
			}
		}
		ConnectToWalls();
		for (int num3 = segments.GetLength(0) - 1; num3 > 0; num3--)
		{
			Connect(num3, num3 - 1);
		}
		ConnectToWalls();
		for (int m = 1; m < segments.GetLength(0); m++)
		{
			Connect(m, m - 1);
		}
		ConnectToWalls();
		for (int n = 0; n < mycelia.GetLength(0); n++)
		{
			Vector2 vector3 = Custom.PerpendicularVector(Custom.DirVec(segments[SegmentOfMycelium(n * 2), 0], segments[SegmentOfMycelium(n * 2) + 1, 0]));
			for (int num4 = 0; num4 < 2; num4++)
			{
				mycelia[n, num4].Update();
				mycelia[n, num4].points[1, 2] += vector3 * ((num4 == 0) ? (-1f) : 1f);
				if (mycelia[n, num4].points.GetLength(0) > 2)
				{
					mycelia[n, num4].points[2, 2] += vector3 * ((num4 == 0) ? (-1f) : 1f) * 0.5f;
				}
			}
		}
		for (int num5 = 0; num5 < bumpPings.GetLength(0); num5++)
		{
			bumpPings[num5, 1] = bumpPings[num5, 0];
			bumpPings[num5, 0] = Mathf.Max(0f, bumpPings[num5, 0] - 0.1f);
			if (bumpPings[num5, 0] == 0f && UnityEngine.Random.value < 1f / 60f)
			{
				bumpPings[num5, 0] = 1f;
			}
		}
	}

	private void ConnectToWalls()
	{
		if (rootDirA.HasValue)
		{
			segments[0, 0] = posA.Value - rootDirA.Value * 10f;
			segments[0, 2] *= 0f;
		}
		if (rootDirB.HasValue)
		{
			segments[segments.GetLength(0) - 1, 0] = posB.Value - rootDirB.Value * 10f;
			segments[segments.GetLength(0) - 1, 2] *= 0f;
		}
	}

	private void Connect(int A, int B)
	{
		Vector2 normalized = (segments[A, 0] - segments[B, 0]).normalized;
		float num = Vector2.Distance(segments[A, 0], segments[B, 0]);
		float num2 = Mathf.InverseLerp(0f, conRad, num);
		segments[A, 0] += normalized * (conRad - num) * 0.5f * num2;
		segments[A, 2] += normalized * (conRad - num) * 0.5f * num2;
		segments[B, 0] -= normalized * (conRad - num) * 0.5f * num2;
		segments[B, 2] -= normalized * (conRad - num) * 0.5f * num2;
	}

	public Vector2 Pos(int index)
	{
		return segments[index, 0];
	}

	public int TotalPositions()
	{
		return segments.GetLength(0);
	}

	public float Rad(int index)
	{
		return 2f;
	}

	public float Mass(int index)
	{
		return 0.5f;
	}

	public void Push(int index, Vector2 movement)
	{
		segments[index, 0] += movement;
		segments[index, 2] += movement;
	}

	public void BeingClimbedOn(Creature crit)
	{
	}

	public bool CurrentlyClimbable()
	{
		return true;
	}

	private int SegmentOfMycelium(int myc)
	{
		if (segments.GetLength(0) - 2 <= mycelia.GetLength(0))
		{
			return myc / 2 + 1;
		}
		int num = myc / 2;
		if (rootDirA.HasValue && rootDirB.HasValue)
		{
			return segments.GetLength(0) / 2 - mycelia.GetLength(0) / 2 + num;
		}
		if (rootDirA.HasValue && !rootDirB.HasValue)
		{
			return segments.GetLength(0) - 1 - mycelia.GetLength(0) + num;
		}
		if (!rootDirA.HasValue && rootDirB.HasValue)
		{
			return num + 1;
		}
		if (num < mycelia.GetLength(0) / 2)
		{
			return num + 1;
		}
		return segments.GetLength(0) - 1 - mycelia.GetLength(0) + num;
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[2 + mycelia.Length + bumps.Length * 3];
		sLeaser.sprites[0] = TriangleMesh.MakeLongMesh(segments.GetLength(0), pointyTip: false, customColor: true);
		sLeaser.sprites[1] = TriangleMesh.MakeLongMesh(segments.GetLength(0), pointyTip: false, customColor: false);
		sLeaser.sprites[1].shader = rCam.room.game.rainWorld.Shaders["CoralNeuron"];
		sLeaser.sprites[1].alpha = 1f / (float)segments.GetLength(0);
		for (int i = 0; i < (sLeaser.sprites[0] as TriangleMesh).verticeColors.Length; i++)
		{
			float f = (float)i / (float)((sLeaser.sprites[0] as TriangleMesh).verticeColors.Length - 1);
			(sLeaser.sprites[0] as TriangleMesh).verticeColors[i] = MeshColor(f);
		}
		int num = 2;
		for (int j = 0; j < mycelia.GetLength(0); j++)
		{
			for (int k = 0; k < 2; k++)
			{
				mycelia[j, k].InitiateSprites(num, sLeaser, rCam);
				num++;
			}
		}
		for (int l = 0; l < bumps.Length; l++)
		{
			sLeaser.sprites[num] = new FSprite("deerEyeB");
			sLeaser.sprites[num].color = Color.Lerp(MeshColor(bumps[l].y), new Color(0f, 0f, 0.2f), 0.25f);
			sLeaser.sprites[num].scale = Mathf.InverseLerp(0f, 0.6f, Rad(bumps[l].y));
			sLeaser.sprites[num + 1] = new FSprite("deerEyeB");
			sLeaser.sprites[num + 1].color = new Color(1f, 1f, 1f);
			sLeaser.sprites[num + 1].alpha = 0.3f;
			sLeaser.sprites[num + 1].scale = Mathf.InverseLerp(0f, 0.6f, Rad(bumps[l].y)) * 0.5f;
			sLeaser.sprites[num + 2] = new FSprite("deerEyeB");
			sLeaser.sprites[num + 2].color = new Color(0f, 0f, 0.1f);
			sLeaser.sprites[num + 2].scale = Mathf.InverseLerp(0f, 0.6f, Rad(bumps[l].y)) * 0.5f;
			num += 3;
		}
		AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Items"));
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = Vector2.Lerp(segments[0, 1], segments[0, 0], timeStacker);
		for (int i = 0; i < segments.GetLength(0); i++)
		{
			Vector2 vector2 = Vector2.Lerp(segments[i, 1], segments[i, 0], timeStacker);
			Vector2 normalized = (vector - vector2).normalized;
			Vector2 vector3 = Custom.PerpendicularVector(normalized);
			float num = Vector2.Distance(vector, vector2) / 5f;
			float num2 = Rad((float)i / (float)(segments.GetLength(0) - 1));
			if (i == 0)
			{
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4, vector - vector3 - camPos);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 1, vector + vector3 - camPos);
			}
			else
			{
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4, vector - normalized * num - vector3 * 1.5f * num2 - camPos);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 1, vector - normalized * num + vector3 * 1.5f * num2 - camPos);
			}
			if (i == segments.GetLength(0) - 1)
			{
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 2, vector2 - vector3 - camPos);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 3, vector2 + vector3 - camPos);
			}
			else
			{
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 2, vector2 + normalized * num - vector3 * 3.5f * num2 - camPos);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 3, vector2 + normalized * num + vector3 * 3.5f * num2 - camPos);
			}
			for (int j = 0; j < 4; j++)
			{
				(sLeaser.sprites[1] as TriangleMesh).MoveVertice(i * 4 + j, (sLeaser.sprites[0] as TriangleMesh).vertices[i * 4 + j]);
			}
			vector = vector2;
		}
		int num3 = 2;
		for (int k = 0; k < mycelia.GetLength(0); k++)
		{
			for (int l = 0; l < 2; l++)
			{
				mycelia[k, l].DrawSprites(num3, sLeaser, rCam, timeStacker, camPos);
				num3++;
			}
		}
		for (int m = 0; m < bumps.Length; m++)
		{
			Vector2 vector4 = Pos(bumps[m].y, timeStacker) + Custom.PerpendicularVector(Direction(bumps[m].y, timeStacker)) * bumps[m].x * 4f * Rad(bumps[m].y);
			sLeaser.sprites[num3].x = vector4.x - camPos.x;
			sLeaser.sprites[num3].y = vector4.y - camPos.y;
			sLeaser.sprites[num3 + 1].x = vector4.x - camPos.x - Rad(bumps[m].y) * 1.5f;
			sLeaser.sprites[num3 + 1].y = vector4.y - camPos.y + Rad(bumps[m].y) * 1.5f;
			sLeaser.sprites[num3 + 2].x = vector4.x - camPos.x;
			sLeaser.sprites[num3 + 2].y = vector4.y - camPos.y;
			sLeaser.sprites[num3 + 2].color = Custom.HSL2RGB(2f / 3f, 1f, 0.1f + 0.9f * Mathf.Lerp(bumpPings[m, 1], bumpPings[m, 0], timeStacker));
			num3 += 3;
		}
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	private float Rad(float f)
	{
		return Mathf.Lerp(0.2f, 1f, Mathf.Pow(Mathf.Clamp(Mathf.Sin(f * (float)Math.PI), 0f, 1f), 0.5f));
	}

	private Vector2 Direction(float f, float timeStacker)
	{
		int num = Mathf.FloorToInt(f * (float)(segments.GetLength(0) - 1));
		return Vector3.Slerp(DirAtSegment(num, timeStacker), DirAtSegment(num + 1, timeStacker), Mathf.InverseLerp(num / segments.GetLength(0), (num + 1) / segments.GetLength(0), f));
	}

	private Vector2 Pos(float f, float timeStacker)
	{
		int num = Custom.IntClamp(Mathf.FloorToInt(f * (float)(segments.GetLength(0) - 1)), 0, segments.GetLength(0) - 1);
		return Vector2.Lerp(Vector2.Lerp(segments[num, 1], segments[num, 0], timeStacker), Vector2.Lerp(segments[Math.Min(num + 1, segments.GetLength(0) - 1), 1], segments[Math.Min(num + 1, segments.GetLength(0) - 1), 0], timeStacker), Mathf.InverseLerp(num, num + 1, f * (float)(segments.GetLength(0) - 1)));
	}

	private Vector2 DirAtSegment(int seg, float timeStacker)
	{
		if (seg >= segments.GetLength(0) - 1)
		{
			return Custom.DirVec(segments[segments.GetLength(0) - 2, 0], segments[segments.GetLength(0) - 1, 0]);
		}
		return Custom.DirVec(Vector2.Lerp(segments[seg, 1], segments[seg, 0], timeStacker), Vector2.Lerp(segments[seg + 1, 1], segments[seg + 1, 0], timeStacker));
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Midground");
		}
		FSprite[] sprites = sLeaser.sprites;
		foreach (FSprite fSprite in sprites)
		{
			fSprite.RemoveFromContainer();
			newContatiner.AddChild(fSprite);
		}
		if (!(rCam.room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.WaterLights) > 0f))
		{
			return;
		}
		FContainer fContainer = rCam.ReturnFContainer("Water");
		int num = 2;
		for (int j = 0; j < mycelia.GetLength(0); j++)
		{
			for (int k = 0; k < 2; k++)
			{
				num++;
			}
		}
		for (int l = 0; l < bumps.Length; l++)
		{
			sLeaser.sprites[num + 2].RemoveFromContainer();
			fContainer.AddChild(sLeaser.sprites[num + 2]);
			num += 3;
		}
	}

	public Vector2 ConnectionPos(int index, float timeStacker)
	{
		return Vector2.Lerp(segments[SegmentOfMycelium(index), 1], segments[SegmentOfMycelium(index), 0], timeStacker);
	}

	public Vector2 ResetDir(int index)
	{
		if (SegmentOfMycelium(index) >= segments.GetLength(0))
		{
			new Vector2(0f, 0f);
		}
		return Custom.PerpendicularVector(Custom.DirVec(segments[SegmentOfMycelium(index), 0], segments[SegmentOfMycelium(index) + 1, 0])) * ((index % 2 == 0) ? (-1f) : 1f);
	}

	public Vector2 CircleCenter(int index, float timeStacker)
	{
		if (index == 0)
		{
			return Vector2.Lerp(segments[0, 1], segments[0, 0], timeStacker);
		}
		return Vector2.Lerp(segments[segments.GetLength(0) - 1, 1], segments[segments.GetLength(0) - 1, 0], timeStacker);
	}

	public Room HostingCircleFromRoom()
	{
		return room;
	}

	public bool CanHostCircle()
	{
		return !base.slatedForDeletetion;
	}
}
