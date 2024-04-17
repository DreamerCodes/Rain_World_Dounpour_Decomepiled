using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public abstract class Vine : UpdatableAndDeletable
{
	public abstract class VineGraphic : RopeGraphic
	{
		public struct Leaf
		{
			public Vector2 pos;

			public float size;

			public Leaf(Vector2 pos, float size)
			{
				this.pos = pos;
				this.size = size;
			}
		}

		public Vine owner;

		public int firstSprite;

		public int sprites;

		public Leaf[] leaves;

		public VineGraphic(Vine owner, int segments, int firstSprite)
			: base(segments)
		{
			this.owner = owner;
			this.firstSprite = firstSprite;
		}

		public override void Update()
		{
			for (int i = 0; i < segments.Length; i++)
			{
				segments[i].lastPos = segments[i].pos;
				segments[i].pos = owner.segments[i, 0];
			}
		}

		public override void ConnectPhase(float totalRopeLength)
		{
		}

		public override void MoveSegment(int segment, Vector2 goalPos, Vector2 smoothedGoalPos)
		{
			segments[segment].vel *= 0f;
			if (owner.room.GetTile(smoothedGoalPos).Solid && !owner.room.GetTile(goalPos).Solid)
			{
				FloatRect floatRect = Custom.RectCollision(smoothedGoalPos, goalPos, owner.room.TileRect(owner.room.GetTilePosition(smoothedGoalPos)).Grow(3f));
				segments[segment].pos = new Vector2(floatRect.left, floatRect.bottom);
			}
			else
			{
				segments[segment].pos = smoothedGoalPos;
			}
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites[firstSprite] = TriangleMesh.MakeLongMeshAtlased(segments.Length, pointyTip: false, customColor: true);
			for (int i = 0; i < leaves.Length; i++)
			{
				sLeaser.sprites[firstSprite + 1 + i] = new FSprite("Leaf" + Random.Range(0, 5), quadType: false);
				sLeaser.sprites[firstSprite + 1 + i].scale = 1f;
				sLeaser.sprites[firstSprite + 1 + i].anchorY = 0.9f;
				sLeaser.sprites[firstSprite + 1 + i].rotation = Random.value * 360f;
			}
			ApplyPalette(sLeaser, rCam, rCam.currentPalette);
		}

		public override void DrawSprite(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = Vector2.Lerp(segments[0].lastPos, segments[0].pos, timeStacker);
			vector += Custom.DirVec(Vector2.Lerp(segments[1].lastPos, segments[1].pos, timeStacker), vector) * 1f;
			float num = 2f;
			for (int i = 0; i < segments.Length; i++)
			{
				_ = (float)i / (float)(segments.Length - 1);
				Vector2 vector2 = Vector2.Lerp(segments[i].lastPos, segments[i].pos, timeStacker);
				if (i < segments.Length - 1)
				{
					Vector2.Lerp(segments[i + 1].lastPos, segments[i + 1].pos, timeStacker);
				}
				Vector2 vector3 = Custom.PerpendicularVector((vector - vector2).normalized);
				(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(i * 4, vector - vector3 * num - camPos);
				(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(i * 4 + 1, vector + vector3 * num - camPos);
				(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(i * 4 + 2, vector2 - vector3 * num - camPos);
				(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(i * 4 + 3, vector2 + vector3 * num - camPos);
				vector = vector2;
			}
			for (int j = 0; j < leaves.Length; j++)
			{
				Vector2 vector4 = OnVinePos(leaves[j].pos, timeStacker);
				sLeaser.sprites[firstSprite + 1 + j].x = vector4.x - camPos.x;
				sLeaser.sprites[firstSprite + 1 + j].y = vector4.y - camPos.y;
			}
		}

		public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			for (int i = 0; i < (sLeaser.sprites[firstSprite] as TriangleMesh).vertices.Length; i++)
			{
				float floatPos = (float)i / (float)((sLeaser.sprites[firstSprite] as TriangleMesh).vertices.Length - 1);
				(sLeaser.sprites[firstSprite] as TriangleMesh).verticeColors[i] = Color.Lerp(palette.blackColor, owner.EffectColor, OnVineEffectColorFac(floatPos));
			}
			for (int j = 0; j < leaves.Length; j++)
			{
				sLeaser.sprites[firstSprite + 1 + j].color = Color.Lerp(palette.blackColor, owner.EffectColor, OnVineEffectColorFac(leaves[j].pos.y));
			}
		}

		public virtual float OnVineEffectColorFac(float floatPos)
		{
			return Mathf.Clamp(Mathf.Pow(floatPos, 1.5f) * 0.4f, 0.5f, 1f);
		}

		public Vector2 OnVinePos(Vector2 pos, float timeStacker)
		{
			Vector2 p = OneDimensionalVinePos(pos.y - 1f / (float)(segments.Length - 1), timeStacker);
			Vector2 p2 = OneDimensionalVinePos(pos.y + 1f / (float)(segments.Length - 1), timeStacker);
			return OneDimensionalVinePos(pos.y, timeStacker) + Custom.PerpendicularVector(Custom.DirVec(p, p2)) * pos.x;
		}

		public Vector2 OneDimensionalVinePos(float floatPos, float timeStacker)
		{
			int num = Custom.IntClamp(Mathf.FloorToInt(floatPos * (float)(segments.Length - 1)), 0, segments.Length - 1);
			int num2 = Custom.IntClamp(num + 1, 0, segments.Length - 1);
			float t = Mathf.InverseLerp(num, num2, floatPos * (float)(segments.Length - 1));
			return Vector2.Lerp(Vector2.Lerp(segments[num].lastPos, segments[num2].lastPos, t), Vector2.Lerp(segments[num].pos, segments[num2].pos, t), timeStacker);
		}
	}

	public VineGraphic graphic;

	public Color baseColor;

	public float conRad;

	public float pushApart;

	public Vector2[,] segments;

	public Vector2? stuckPosA;

	public Vector2? stuckPosB;

	public Color EffectColor => baseColor;

	public Vine(Room room, float length, Vector2 spawnPosA, Vector2 spawnPosB, bool stuckAtA, bool stuckAtB)
	{
		base.room = room;
		conRad = 10f;
		pushApart = 0.15f;
		if (stuckAtA)
		{
			stuckPosA = spawnPosA;
		}
		if (stuckAtB)
		{
			stuckPosB = spawnPosB;
		}
		segments = new Vector2[Mathf.Max(2, (int)Mathf.Clamp(length / conRad, 1f, 200f)), 3];
		for (int i = 0; i < segments.GetLength(0); i++)
		{
			float t = (float)i / (float)(segments.GetLength(0) - 1);
			segments[i, 0] = Vector2.Lerp(spawnPosA, spawnPosB, t) + Custom.RNV() * Random.value;
			segments[i, 1] = segments[i, 0];
			segments[i, 2] = Custom.RNV() * Random.value;
		}
		RoomPalette currentPalette = base.room.game.cameras[0].currentPalette;
		baseColor = currentPalette.texture.GetPixel(15 + (int)stuckPosA.Value.x % 9, 2 + (int)stuckPosA.Value.x % 2);
		baseColor += currentPalette.texture.GetPixel(4, 8) / 2f;
		float num = base.room.game.cameras[0].PaletteDarkness() * 0.1f;
		baseColor -= new Color(num, num, num, 1f);
		baseColor.r = Mathf.Clamp(baseColor.r, 0f, 1f);
		baseColor.g = Mathf.Clamp(baseColor.g, 0f, 1f);
		baseColor.b = Mathf.Clamp(baseColor.b, 0f, 1f);
		baseColor.a = Mathf.Clamp(baseColor.a, 0f, 1f);
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		for (int i = 2; i < segments.GetLength(0); i++)
		{
			Vector2 vector = Custom.DirVec(segments[i - 2, 0], segments[i, 0]);
			segments[i - 2, 2] -= vector * pushApart;
			segments[i, 2] += vector * pushApart;
		}
		for (int j = 0; j < segments.GetLength(0); j++)
		{
			_ = (float)j / (float)(segments.GetLength(0) - 1);
			segments[j, 2].y -= 0.9f * room.gravity * GravityAffected(j);
			segments[j, 1] = segments[j, 0];
			segments[j, 0] += segments[j, 2];
			segments[j, 2] *= 0.999f;
			if (room.gravity < 1f && room.readyForAI && room.aimap.getTerrainProximity(segments[j, 0]) < 4)
			{
				IntVector2 tilePosition = room.GetTilePosition(segments[j, 0]);
				Vector2 vector2 = new Vector2(0f, 0f);
				for (int k = 0; k < 4; k++)
				{
					if (!room.GetTile(tilePosition + Custom.fourDirections[k]).Solid && !room.aimap.getAItile(tilePosition + Custom.fourDirections[k]).narrowSpace)
					{
						float num = 0f;
						for (int l = 0; l < 4; l++)
						{
							num += (float)room.aimap.getTerrainProximity(tilePosition + Custom.fourDirections[k] + Custom.fourDirections[l]);
						}
						vector2 += Custom.fourDirections[k].ToVector2() * num;
					}
				}
				segments[j, 2] += vector2.normalized * ((!room.GetTile(segments[j, 0]).Solid) ? Custom.LerpMap(room.aimap.getTerrainProximity(segments[j, 0]), 0f, 3f, 2f, 0.2f) : 1f) * (1f - room.gravity);
			}
			if (j > 2 && room.aimap.getTerrainProximity(segments[j, 0]) < 3)
			{
				SharedPhysics.TerrainCollisionData cd2 = SharedPhysics.VerticalCollision(cd: new SharedPhysics.TerrainCollisionData(segments[j, 0], segments[j, 1], segments[j, 2], 2f, new IntVector2(0, 0), goThroughFloors: true), room: room);
				cd2 = SharedPhysics.HorizontalCollision(room, cd2);
				segments[j, 0] = cd2.pos;
				segments[j, 2] = cd2.vel;
				if (cd2.contactPoint.x != 0)
				{
					segments[j, 2].y *= 0.6f;
				}
				if (cd2.contactPoint.y != 0)
				{
					segments[j, 2].x *= 0.6f;
				}
			}
		}
		ConnectToWalls();
		for (int num2 = segments.GetLength(0) - 1; num2 > 0; num2--)
		{
			Connect(num2, num2 - 1);
		}
		ConnectToWalls();
		for (int m = 1; m < segments.GetLength(0); m++)
		{
			Connect(m, m - 1);
		}
		ConnectToWalls();
		graphic.Update();
	}

	private void ConnectToWalls()
	{
		if (stuckPosA.HasValue)
		{
			segments[0, 0] = stuckPosA.Value;
			segments[0, 2] *= 0f;
		}
		if (stuckPosB.HasValue)
		{
			segments[segments.GetLength(0) - 1, 0] = stuckPosB.Value;
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

	public virtual float GravityAffected(int seg)
	{
		return 1f;
	}
}
