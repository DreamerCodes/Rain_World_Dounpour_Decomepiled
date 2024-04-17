using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RWCustom;
using UnityEngine;

public static class SharedPhysics
{
	public struct CollisionResult
	{
		public PhysicalObject obj;

		public BodyChunk chunk;

		public PhysicalObject.Appendage.Pos onAppendagePos;

		public bool hitSomething;

		public Vector2 collisionPoint;

		public CollisionResult(PhysicalObject obj, BodyChunk chunk, PhysicalObject.Appendage.Pos onAppendagePos, bool hitSomething, Vector2 collisionPoint)
		{
			this.obj = obj;
			this.chunk = chunk;
			this.hitSomething = hitSomething;
			this.collisionPoint = collisionPoint;
			this.onAppendagePos = onAppendagePos;
		}
	}

	public interface IProjectileTracer
	{
		bool HitThisObject(PhysicalObject obj);

		bool HitThisChunk(BodyChunk chunk);
	}

	public struct TerrainCollisionData
	{
		public Vector2 pos;

		public Vector2 vel;

		public Vector2 lastPos;

		public IntVector2 contactPoint;

		public float rad;

		public bool goThroughFloors;

		public TerrainCollisionData(Vector2 pos, Vector2 lastPos, Vector2 vel, float rad, IntVector2 contactPoint, bool goThroughFloors)
		{
			this.pos = pos;
			this.vel = vel;
			this.lastPos = lastPos;
			this.rad = rad;
			this.contactPoint = contactPoint;
			this.goThroughFloors = goThroughFloors;
		}

		public TerrainCollisionData Set(Vector2 pos, Vector2 lastPos, Vector2 vel, float rad, IntVector2 contactPoint, bool goThroughFloors)
		{
			this.pos = pos;
			this.vel = vel;
			this.lastPos = lastPos;
			this.rad = rad;
			this.contactPoint = contactPoint;
			this.goThroughFloors = goThroughFloors;
			return this;
		}
	}

	private static TerrainCollisionData scratchTerrainCollisionData = default(TerrainCollisionData);

	public static int MaxRepeats = 100000;

	public static CollisionResult TraceProjectileAgainstBodyChunks(IProjectileTracer projTracer, Room room, Vector2 lastPos, ref Vector2 pos, float rad, int collisionLayer, PhysicalObject exemptObject, bool hitAppendages)
	{
		float num = float.MaxValue;
		CollisionResult result = new CollisionResult(null, null, null, hitSomething: false, pos);
		int num2 = collisionLayer;
		int num3 = collisionLayer;
		if (collisionLayer < 0)
		{
			num2 = 0;
			num3 = room.physicalObjects.Length - 1;
		}
		for (int i = num2; i <= num3; i++)
		{
			foreach (PhysicalObject item in room.physicalObjects[i])
			{
				if (item == exemptObject || !item.canBeHitByWeapons || (projTracer != null && !projTracer.HitThisObject(item)))
				{
					continue;
				}
				bool flag = false;
				for (int j = 0; j < item.grabbedBy.Count; j++)
				{
					if (flag)
					{
						break;
					}
					flag = item.grabbedBy[j].grabber == exemptObject;
				}
				if (flag)
				{
					continue;
				}
				BodyChunk[] bodyChunks = item.bodyChunks;
				foreach (BodyChunk bodyChunk in bodyChunks)
				{
					if (projTracer == null || projTracer.HitThisChunk(bodyChunk))
					{
						float num4 = Custom.CirclesCollisionTime(lastPos.x, lastPos.y, bodyChunk.pos.x, bodyChunk.pos.y, pos.x - lastPos.x, pos.y - lastPos.y, rad, bodyChunk.rad);
						if (num4 > 0f && num4 < 1f && num4 < num)
						{
							num = num4;
							result = new CollisionResult(item, bodyChunk, null, hitSomething: true, Vector2.Lerp(lastPos, pos, num4));
						}
					}
				}
				if (!hitAppendages || result.chunk != null || item.appendages == null)
				{
					continue;
				}
				foreach (PhysicalObject.Appendage appendage in item.appendages)
				{
					if (!appendage.canBeHit)
					{
						continue;
					}
					for (int l = 1; l < appendage.segments.Length; l++)
					{
						Vector2 vector = Custom.LineIntersection(lastPos, pos, appendage.segments[l - 1], appendage.segments[l]);
						if (Mathf.InverseLerp(0f, Vector2.Distance(lastPos, pos), Vector2.Distance(lastPos, vector)) < num && Custom.DistLess(vector, lastPos, Vector2.Distance(lastPos, pos)) && Custom.DistLess(vector, pos, Vector2.Distance(lastPos, pos)) && Custom.DistLess(vector, appendage.segments[l - 1], Vector2.Distance(appendage.segments[l - 1], appendage.segments[l])) && Custom.DistLess(vector, appendage.segments[l], Vector2.Distance(appendage.segments[l - 1], appendage.segments[l])))
						{
							result = new CollisionResult(item, null, new PhysicalObject.Appendage.Pos(appendage, l - 1, Mathf.InverseLerp(0f, Vector2.Distance(appendage.segments[l - 1], appendage.segments[l]), Vector2.Distance(appendage.segments[l - 1], vector))), hitSomething: true, vector);
						}
					}
				}
			}
		}
		return result;
	}

	public static float TerrainCollisionTime(Room room, Vector2 pos, Vector2 lastPos, float rad, bool goThroughFloors)
	{
		Vector2 b = TraceTerrainCollision(room, pos, lastPos, rad, goThroughFloors);
		return Mathf.InverseLerp(0f, Vector2.Distance(lastPos, pos), Vector2.Distance(lastPos, b));
	}

	public static CollisionResult CollisionResultTraceTerrainCollision(Room room, Vector2 pos, Vector2 lastPos, float rad, bool goThroughFloors)
	{
		Vector2 vector = TraceTerrainCollision(room, pos, lastPos, rad, goThroughFloors);
		return new CollisionResult(null, null, null, pos != vector, vector);
	}

	public static Vector2? ExactTerrainRayTracePos(Room room, Vector2 A, Vector2 B)
	{
		FloatRect? floatRect = ExactTerrainRayTrace(room, A, B);
		if (floatRect.HasValue)
		{
			return new Vector2(floatRect.Value.left, floatRect.Value.bottom);
		}
		return null;
	}

	public static FloatRect? ExactTerrainRayTrace(Room room, Vector2 A, Vector2 B)
	{
		IntVector2? intVector = RayTraceTilesForTerrainReturnFirstSolid(room, A, B);
		if (!intVector.HasValue)
		{
			return null;
		}
		return Custom.RectCollision(B, A, room.TileRect(intVector.Value));
	}

	public static Vector2 TraceTerrainCollision(Room room, Vector2 pos, Vector2 lastPos, float rad, bool goThroughFloors)
	{
		TerrainCollisionData cd = scratchTerrainCollisionData.Set(pos, lastPos, lastPos - pos, rad, new IntVector2(0, 0), goThroughFloors);
		cd = VerticalCollision(room, cd);
		cd = SlopesVertically(room, cd);
		cd = HorizontalCollision(room, cd);
		return pos;
	}

	public static TerrainCollisionData HorizontalCollision(Room room, TerrainCollisionData cd)
	{
		IntVector2 tilePosition = room.GetTilePosition(cd.lastPos);
		int num = 0;
		if (cd.vel.x > 0f)
		{
			int x = room.GetTilePosition(new Vector2(cd.pos.x + cd.rad + 0.01f, 0f)).x;
			int x2 = room.GetTilePosition(new Vector2(cd.lastPos.x + cd.rad, 0f)).x;
			int y = room.GetTilePosition(new Vector2(0f, cd.pos.y + cd.rad - 1f)).y;
			int y2 = room.GetTilePosition(new Vector2(0f, cd.pos.y - cd.rad + 1f)).y;
			bool flag = false;
			for (int i = x2; i <= x; i++)
			{
				if (flag)
				{
					break;
				}
				for (int j = y2; j <= y; j++)
				{
					if (flag)
					{
						break;
					}
					if (room.GetTile(i, j).Terrain == Room.Tile.TerrainType.Solid && room.GetTile(i - 1, j).Terrain != Room.Tile.TerrainType.Solid && (tilePosition.x < i || room.GetTile(cd.lastPos).Terrain == Room.Tile.TerrainType.Solid))
					{
						cd.pos.x = (float)i * 20f - cd.rad;
						cd.vel.x = 0f;
						cd.contactPoint.x = 1;
						flag = true;
					}
					num++;
					if (num > MaxRepeats)
					{
						Custom.LogWarning("!!!!! sharedphysics emergency breakout of terrain check!");
						flag = true;
					}
				}
			}
		}
		else if (cd.vel.x < 0f)
		{
			int x3 = room.GetTilePosition(new Vector2(cd.pos.x - cd.rad - 0.01f, 0f)).x;
			int x4 = room.GetTilePosition(new Vector2(cd.lastPos.x - cd.rad, 0f)).x;
			int y3 = room.GetTilePosition(new Vector2(0f, cd.pos.y + cd.rad - 1f)).y;
			int y4 = room.GetTilePosition(new Vector2(0f, cd.pos.y - cd.rad + 1f)).y;
			bool flag2 = false;
			int num2 = x4;
			while (num2 >= x3 && !flag2)
			{
				for (int k = y4; k <= y3; k++)
				{
					if (flag2)
					{
						break;
					}
					if (room.GetTile(num2, k).Terrain == Room.Tile.TerrainType.Solid && room.GetTile(num2 + 1, k).Terrain != Room.Tile.TerrainType.Solid && (tilePosition.x > num2 || room.GetTile(cd.lastPos).Terrain == Room.Tile.TerrainType.Solid))
					{
						cd.pos.x = ((float)num2 + 1f) * 20f + cd.rad;
						cd.vel.x = 0f;
						cd.contactPoint.x = -1;
						flag2 = true;
					}
					num++;
					if (num > MaxRepeats)
					{
						Custom.LogWarning("!!!!! sharedphysics emergency breakout of terrain check!");
						flag2 = true;
					}
				}
				num2--;
			}
		}
		return cd;
	}

	public static TerrainCollisionData VerticalCollision(Room room, TerrainCollisionData cd)
	{
		IntVector2 tilePosition = room.GetTilePosition(cd.lastPos);
		int num = 0;
		if (cd.vel.y > 0f)
		{
			int y = room.GetTilePosition(new Vector2(0f, cd.pos.y + cd.rad + 0.01f)).y;
			int y2 = room.GetTilePosition(new Vector2(0f, cd.lastPos.y + cd.rad)).y;
			int x = room.GetTilePosition(new Vector2(cd.pos.x - cd.rad + 1f, 0f)).x;
			int x2 = room.GetTilePosition(new Vector2(cd.pos.x + cd.rad - 1f, 0f)).x;
			bool flag = false;
			for (int i = y2; i <= y; i++)
			{
				if (flag)
				{
					break;
				}
				for (int j = x; j <= x2; j++)
				{
					if (flag)
					{
						break;
					}
					if (room.GetTile(j, i).Terrain == Room.Tile.TerrainType.Solid && room.GetTile(j, i - 1).Terrain != Room.Tile.TerrainType.Solid && (tilePosition.y < i || room.GetTile(cd.lastPos).Terrain == Room.Tile.TerrainType.Solid))
					{
						cd.pos.y = (float)i * 20f - cd.rad;
						cd.vel.y = 0f;
						cd.contactPoint.y = 1;
						flag = true;
					}
					num++;
					if (num > MaxRepeats)
					{
						Custom.LogWarning("!!!!! sharedphysics emergency breakout of terrain check!");
						flag = true;
					}
				}
			}
		}
		else if (cd.vel.y < 0f)
		{
			int y3 = room.GetTilePosition(new Vector2(0f, cd.pos.y - cd.rad - 0.01f)).y;
			int y4 = room.GetTilePosition(new Vector2(0f, cd.lastPos.y - cd.rad)).y;
			int x3 = room.GetTilePosition(new Vector2(cd.pos.x - cd.rad + 1f, 0f)).x;
			int x4 = room.GetTilePosition(new Vector2(cd.pos.x + cd.rad - 1f, 0f)).x;
			bool flag2 = false;
			int num2 = y4;
			while (num2 >= y3 && !flag2)
			{
				for (int k = x3; k <= x4; k++)
				{
					if (flag2)
					{
						break;
					}
					if (SolidFloor(room, k, num2, cd.goThroughFloors, cd.lastPos) && !SolidFloor(room, k, num2 + 1, cd.goThroughFloors, cd.lastPos) && (tilePosition.y > num2 || room.GetTile(cd.lastPos).Terrain == Room.Tile.TerrainType.Solid))
					{
						cd.pos.y = ((float)num2 + 1f) * 20f + cd.rad;
						cd.vel.y = 0f;
						cd.contactPoint.y = -1;
						flag2 = true;
					}
					num++;
					if (num > MaxRepeats)
					{
						Custom.LogWarning("!!!!! sharedphysics emergency breakout of terrain check!");
						flag2 = true;
					}
				}
				num2--;
			}
		}
		return cd;
	}

	public static TerrainCollisionData SlopesVertically(Room room, TerrainCollisionData cd)
	{
		IntVector2 tilePosition = room.GetTilePosition(cd.pos);
		IntVector2 intVector = new IntVector2(0, 0);
		Room.SlopeDirection slopeDirection = room.IdentifySlope(cd.pos);
		if (room.GetTile(cd.pos).Terrain != Room.Tile.TerrainType.Slope)
		{
			if (room.IdentifySlope(tilePosition.x - 1, tilePosition.y) != Room.SlopeDirection.Broken && cd.pos.x - cd.rad <= room.MiddleOfTile(cd.pos).x - 10f)
			{
				slopeDirection = room.IdentifySlope(tilePosition.x - 1, tilePosition.y);
				intVector.x = -1;
			}
			else if (room.IdentifySlope(tilePosition.x + 1, tilePosition.y) != Room.SlopeDirection.Broken && cd.pos.x + cd.rad >= room.MiddleOfTile(cd.pos).x + 10f)
			{
				slopeDirection = room.IdentifySlope(tilePosition.x + 1, tilePosition.y);
				intVector.x = 1;
			}
			else if (cd.pos.y - cd.rad < room.MiddleOfTile(cd.pos).y - 10f)
			{
				if (room.IdentifySlope(tilePosition.x, tilePosition.y - 1) != Room.SlopeDirection.Broken)
				{
					slopeDirection = room.IdentifySlope(tilePosition.x, tilePosition.y - 1);
					intVector.y = -1;
				}
			}
			else if (cd.pos.y + cd.rad > room.MiddleOfTile(cd.pos).y + 10f && room.IdentifySlope(tilePosition.x, tilePosition.y + 1) != Room.SlopeDirection.Broken)
			{
				slopeDirection = room.IdentifySlope(tilePosition.x, tilePosition.y + 1);
				intVector.y = 1;
			}
		}
		if (slopeDirection != Room.SlopeDirection.Broken)
		{
			Vector2 vector = room.MiddleOfTile(room.GetTilePosition(cd.pos) + intVector);
			float num = 0f;
			int num2 = 0;
			if (slopeDirection == Room.SlopeDirection.UpLeft)
			{
				num = cd.pos.x - (vector.x - 10f) + (vector.y - 10f);
				num2 = -1;
			}
			else if (slopeDirection == Room.SlopeDirection.UpRight)
			{
				num = 20f - (cd.pos.x - (vector.x - 10f)) + (vector.y - 10f);
				num2 = -1;
			}
			else if (slopeDirection == Room.SlopeDirection.DownLeft)
			{
				num = 20f - (cd.pos.x - (vector.x - 10f)) + (vector.y - 10f);
				num2 = 1;
			}
			else
			{
				num = cd.pos.x - (vector.x - 10f) + (vector.y - 10f);
				num2 = 1;
			}
			if (num2 == -1 && cd.pos.y <= num + cd.rad + cd.rad)
			{
				cd.pos.y = num + cd.rad + cd.rad;
			}
			else if (num2 == 1 && cd.pos.y >= num - cd.rad - cd.rad)
			{
				cd.pos.y = num - cd.rad - cd.rad;
			}
		}
		return cd;
	}

	private static bool SolidFloor(Room room, int X, int Y, bool goThroughFloors, Vector2 lastPos)
	{
		if (room.GetTile(X, Y).Terrain == Room.Tile.TerrainType.Solid)
		{
			return true;
		}
		if (room.GetTile(X, Y).Terrain == Room.Tile.TerrainType.Floor && !goThroughFloors)
		{
			if (room.GetTilePosition(new Vector2(0f, lastPos.y)).y > Y)
			{
				return true;
			}
			return false;
		}
		return false;
	}

	public static bool RayTraceTilesForTerrain(Room room, IntVector2 a, IntVector2 b)
	{
		return RayTraceTilesForTerrain(room, a.x, a.y, b.x, b.y);
	}

	public static bool RayTraceTilesForTerrain(Room room, int x0, int y0, int x1, int y1)
	{
		int num = Math.Abs(x1 - x0);
		int num2 = Math.Abs(y1 - y0);
		int num3 = x0;
		int num4 = y0;
		int num5 = 1 + num + num2;
		int num6 = ((x1 > x0) ? 1 : (-1));
		int num7 = ((y1 > y0) ? 1 : (-1));
		int num8 = num - num2;
		num *= 2;
		num2 *= 2;
		while (num5 > 0)
		{
			if (room.GetTile(num3, num4).Solid)
			{
				return false;
			}
			if (num8 > 0)
			{
				num3 += num6;
				num8 -= num2;
			}
			else
			{
				num4 += num7;
				num8 += num;
			}
			num5--;
		}
		return true;
	}

	public static bool RayTraceTilesForTerrainAcceptStartInSolid(Room room, IntVector2 a, IntVector2 b)
	{
		return RayTraceTilesForTerrainAcceptStartInSolid(room, a.x, a.y, b.x, b.y);
	}

	public static bool RayTraceTilesForTerrainAcceptStartInSolid(Room room, int x0, int y0, int x1, int y1)
	{
		int num = Math.Abs(x1 - x0);
		int num2 = Math.Abs(y1 - y0);
		int num3 = x0;
		int num4 = y0;
		int num5 = 1 + num + num2;
		int num6 = ((x1 > x0) ? 1 : (-1));
		int num7 = ((y1 > y0) ? 1 : (-1));
		int num8 = num - num2;
		num *= 2;
		num2 *= 2;
		bool flag = !room.GetTile(num3, num4).Solid;
		while (num5 > 0)
		{
			if (room.GetTile(num3, num4).Solid)
			{
				if (flag)
				{
					return false;
				}
			}
			else
			{
				flag = true;
			}
			if (num8 > 0)
			{
				num3 += num6;
				num8 -= num2;
			}
			else
			{
				num4 += num7;
				num8 += num;
			}
			num5--;
		}
		return true;
	}

	public static IntVector2? RayTraceTilesForTerrainReturnFirstSolid(Room room, IntVector2 a, IntVector2 b)
	{
		return RayTraceTilesForTerrainReturnFirstSolid(room, a.x, a.y, b.x, b.y);
	}

	public static IntVector2? RayTraceTilesForTerrainReturnFirstSolid(Room room, int x0, int y0, int x1, int y1)
	{
		int num = Math.Abs(x1 - x0);
		int num2 = Math.Abs(y1 - y0);
		int num3 = x0;
		int num4 = y0;
		int num5 = 1 + num + num2;
		int num6 = ((x1 > x0) ? 1 : (-1));
		int num7 = ((y1 > y0) ? 1 : (-1));
		int num8 = num - num2;
		num *= 2;
		num2 *= 2;
		int num9 = 0;
		while (num5 > 0)
		{
			if (room.GetTile(num3, num4).Solid)
			{
				return new IntVector2(num3, num4);
			}
			if (num8 > 0)
			{
				num3 += num6;
				num8 -= num2;
			}
			else
			{
				num4 += num7;
				num8 += num;
			}
			num9++;
			if (num9 > 10000)
			{
				Custom.LogWarning("GAH!!!!!!", x0.ToString(), y0.ToString(), x1.ToString(), y1.ToString());
				break;
			}
			num5--;
		}
		return null;
	}

	public static IntVector2? RayTraceTilesForTerrainReturnFirstSolid(Room room, Vector2 A, Vector2 B)
	{
		float num = A.x / 20f;
		float num2 = A.y / 20f;
		float num3 = B.x / 20f;
		float num4 = B.y / 20f;
		float num5 = Mathf.Abs(num3 - num);
		float num6 = Mathf.Abs(num4 - num2);
		int num7 = Mathf.FloorToInt(num);
		int num8 = Mathf.FloorToInt(num2);
		float num9 = 1f / num5;
		float num10 = 1f / num6;
		int num11 = 1;
		int num12;
		float num13;
		if (num5 == 0f)
		{
			num12 = 0;
			num13 = num9;
		}
		else if (num3 > num)
		{
			num12 = 1;
			num11 += Mathf.FloorToInt(num3) - num7;
			num13 = ((float)(Mathf.FloorToInt(num) + 1) - num) * num9;
		}
		else
		{
			num12 = -1;
			num11 += num7 - Mathf.FloorToInt(num3);
			num13 = (num - (float)Mathf.FloorToInt(num)) * num9;
		}
		int num14;
		float num15;
		if (num6 == 0f)
		{
			num14 = 0;
			num15 = num10;
		}
		else if (num4 > num2)
		{
			num14 = 1;
			num11 += Mathf.FloorToInt(num4) - num8;
			num15 = ((float)(Mathf.FloorToInt(num2) + 1) - num2) * num10;
		}
		else
		{
			num14 = -1;
			num11 += num8 - Mathf.FloorToInt(num4);
			num15 = (num2 - (float)Mathf.FloorToInt(num2)) * num10;
		}
		while (num11 > 0)
		{
			if (room.GetTile(num7, num8).Solid)
			{
				return new IntVector2(num7, num8);
			}
			if (num15 < num13)
			{
				num8 += num14;
				num15 += num10;
			}
			else
			{
				num7 += num12;
				num13 += num9;
			}
			num11--;
		}
		return null;
	}

	public static IntVector2? RayTraceTilesForTerrainReturnFirstSolidOrPole(Room room, Vector2 A, Vector2 B)
	{
		float num = A.x / 20f;
		float num2 = A.y / 20f;
		float num3 = B.x / 20f;
		float num4 = B.y / 20f;
		float num5 = Mathf.Abs(num3 - num);
		float num6 = Mathf.Abs(num4 - num2);
		int num7 = Mathf.FloorToInt(num);
		int num8 = Mathf.FloorToInt(num2);
		float num9 = 1f / num5;
		float num10 = 1f / num6;
		int num11 = 1;
		int num12;
		float num13;
		if (num5 == 0f)
		{
			num12 = 0;
			num13 = num9;
		}
		else if (num3 > num)
		{
			num12 = 1;
			num11 += Mathf.FloorToInt(num3) - num7;
			num13 = ((float)(Mathf.FloorToInt(num) + 1) - num) * num9;
		}
		else
		{
			num12 = -1;
			num11 += num7 - Mathf.FloorToInt(num3);
			num13 = (num - (float)Mathf.FloorToInt(num)) * num9;
		}
		int num14;
		float num15;
		if (num6 == 0f)
		{
			num14 = 0;
			num15 = num10;
		}
		else if (num4 > num2)
		{
			num14 = 1;
			num11 += Mathf.FloorToInt(num4) - num8;
			num15 = ((float)(Mathf.FloorToInt(num2) + 1) - num2) * num10;
		}
		else
		{
			num14 = -1;
			num11 += num8 - Mathf.FloorToInt(num4);
			num15 = (num2 - (float)Mathf.FloorToInt(num2)) * num10;
		}
		while (num11 > 0)
		{
			if (room.GetTile(num7, num8).IsSolid() || room.GetTile(num7, num8).verticalBeam || room.GetTile(num7, num8).horizontalBeam)
			{
				return new IntVector2(num7, num8);
			}
			if (num15 < num13)
			{
				num8 += num14;
				num15 += num10;
			}
			else
			{
				num7 += num12;
				num13 += num9;
			}
			num11--;
		}
		return null;
	}

	public static bool RayTraceTilesForTerrain(Room room, Vector2 A, Vector2 B)
	{
		float num = A.x / 20f;
		float num2 = A.y / 20f;
		float num3 = B.x / 20f;
		float num4 = B.y / 20f;
		float num5 = Mathf.Abs(num3 - num);
		float num6 = Mathf.Abs(num4 - num2);
		int num7 = Mathf.FloorToInt(num);
		int num8 = Mathf.FloorToInt(num2);
		float num9 = 1f / num5;
		float num10 = 1f / num6;
		int num11 = 1;
		int num12;
		float num13;
		if (num5 == 0f)
		{
			num12 = 0;
			num13 = num9;
		}
		else if (num3 > num)
		{
			num12 = 1;
			num11 += Mathf.FloorToInt(num3) - num7;
			num13 = ((float)(Mathf.FloorToInt(num) + 1) - num) * num9;
		}
		else
		{
			num12 = -1;
			num11 += num7 - Mathf.FloorToInt(num3);
			num13 = (num - (float)Mathf.FloorToInt(num)) * num9;
		}
		int num14;
		float num15;
		if (num6 == 0f)
		{
			num14 = 0;
			num15 = num10;
		}
		else if (num4 > num2)
		{
			num14 = 1;
			num11 += Mathf.FloorToInt(num4) - num8;
			num15 = ((float)(Mathf.FloorToInt(num2) + 1) - num2) * num10;
		}
		else
		{
			num14 = -1;
			num11 += num8 - Mathf.FloorToInt(num4);
			num15 = (num2 - (float)Mathf.FloorToInt(num2)) * num10;
		}
		while (num11 > 0)
		{
			if (room.GetTile(num7, num8).IsSolid())
			{
				return false;
			}
			if (num15 < num13)
			{
				num8 += num14;
				num15 += num10;
			}
			else
			{
				num7 += num12;
				num13 += num9;
			}
			num11--;
		}
		return true;
	}

	public static void RayTracedTilesArray(Vector2 A, Vector2 B, List<IntVector2> result)
	{
		int y;
		float dt_dx;
		float dt_dy;
		int n;
		int x_inc;
		int y_inc;
		float t_next_vertical;
		float t_next_horizontal;
		int num = RayTracedTilesArray_Internal(A, B, out y, out dt_dx, out dt_dy, out n, out x_inc, out y_inc, out t_next_vertical, out t_next_horizontal);
		result.Clear();
		while (n > 0)
		{
			result.Add(new IntVector2(num, y));
			if (t_next_vertical < t_next_horizontal)
			{
				y += y_inc;
				t_next_vertical += dt_dy;
			}
			else
			{
				num += x_inc;
				t_next_horizontal += dt_dx;
			}
			n--;
		}
	}

	[Obsolete("Use function with void return value instead.")]
	public static List<IntVector2> RayTracedTilesArray(Vector2 A, Vector2 B)
	{
		List<IntVector2> result = new List<IntVector2>();
		RayTracedTilesArray(A, B, result);
		return result;
	}

	public static int RayTracedTilesArray(Vector2 A, Vector2 B, IntVector2[] result)
	{
		int y;
		float dt_dx;
		float dt_dy;
		int n;
		int x_inc;
		int y_inc;
		float t_next_vertical;
		float t_next_horizontal;
		int num = RayTracedTilesArray_Internal(A, B, out y, out dt_dx, out dt_dy, out n, out x_inc, out y_inc, out t_next_vertical, out t_next_horizontal);
		int num2 = n;
		if (n < result.Length)
		{
			while (n > 0)
			{
				result[num2 - n] = new IntVector2(num, y);
				if (t_next_vertical < t_next_horizontal)
				{
					y += y_inc;
					t_next_vertical += dt_dy;
				}
				else
				{
					num += x_inc;
					t_next_horizontal += dt_dx;
				}
				n--;
			}
		}
		return num2;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int RayTracedTilesArray_Internal(Vector2 A, Vector2 B, out int y, out float dt_dx, out float dt_dy, out int n, out int x_inc, out int y_inc, out float t_next_vertical, out float t_next_horizontal)
	{
		float num = A.x / 20f;
		float num2 = A.y / 20f;
		float num3 = B.x / 20f;
		float num4 = B.y / 20f;
		float num5 = Mathf.Abs(num3 - num);
		float num6 = Mathf.Abs(num4 - num2);
		int num7 = Mathf.FloorToInt(num);
		y = Mathf.FloorToInt(num2);
		dt_dx = 1f / num5;
		dt_dy = 1f / num6;
		n = 1;
		if (num5 == 0f)
		{
			x_inc = 0;
			t_next_horizontal = dt_dx;
		}
		else if (num3 > num)
		{
			x_inc = 1;
			n += Mathf.FloorToInt(num3) - num7;
			t_next_horizontal = ((float)(Mathf.FloorToInt(num) + 1) - num) * dt_dx;
		}
		else
		{
			x_inc = -1;
			n += num7 - Mathf.FloorToInt(num3);
			t_next_horizontal = (num - (float)Mathf.FloorToInt(num)) * dt_dx;
		}
		if (num6 == 0f)
		{
			y_inc = 0;
			t_next_vertical = dt_dy;
		}
		else if (num4 > num2)
		{
			y_inc = 1;
			n += Mathf.FloorToInt(num4) - y;
			t_next_vertical = ((float)(Mathf.FloorToInt(num2) + 1) - num2) * dt_dy;
		}
		else
		{
			y_inc = -1;
			n += y - Mathf.FloorToInt(num4);
			t_next_vertical = (num2 - (float)Mathf.FloorToInt(num2)) * dt_dy;
		}
		return num7;
	}

	public static void ConnectChunks(BodyChunk A, BodyChunk B, float dist, float skewToMassFac, float massFacSkew, float elasticity)
	{
		Vector2 vector = Custom.DirVec(A.pos, B.pos);
		float num = Vector2.Distance(A.pos, B.pos);
		float num2 = Mathf.Lerp(A.mass / (A.mass + B.mass), skewToMassFac, massFacSkew);
		A.pos -= (dist - num) * vector * (1f - num2) * elasticity;
		A.vel -= (dist - num) * vector * (1f - num2) * elasticity;
		B.pos += (dist - num) * vector * num2 * elasticity;
		B.vel += (dist - num) * vector * num2 * elasticity;
	}
}
