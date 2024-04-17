using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class BodyChunk
{
	public Vector2 pos;

	public Vector2 lastPos;

	public Vector2 lastLastPos;

	public Vector2? setPos;

	public Vector2 vel;

	public float rad;

	public float terrainSqueeze;

	private float slopeRad;

	public float mass;

	public BodyChunk rotationChunk;

	public bool collideWithTerrain;

	public bool collideWithSlopes;

	private bool privGoThroughFloors;

	public bool collideWithObjects = true;

	public float loudness = 1f;

	public int index;

	public int splashStop;

	public float restrictInRoomRange = 1200f;

	public float defaultRestrictInRoomRange = 1200f;

	private IntVector2 contactPoint;

	public IntVector2 lastContactPoint;

	public bool actAsTrigger;

	public bool grabsDisableFloors;

	private static int MaxRepeats = 100000;

	private float TerrainRad
	{
		get
		{
			return Mathf.Max(rad * terrainSqueeze, 1f);
		}
		set
		{
			rad = value / terrainSqueeze;
		}
	}

	public Vector2 Rotation
	{
		get
		{
			if (rotationChunk == null)
			{
				return new Vector2(0f, 1f);
			}
			return (pos - rotationChunk.pos).normalized;
		}
	}

	public bool goThroughFloors
	{
		get
		{
			if (owner.grabbedBy.Count <= 0 || !grabsDisableFloors)
			{
				return privGoThroughFloors;
			}
			return true;
		}
		set
		{
			privGoThroughFloors = value;
		}
	}

	public IntVector2 ContactPoint => contactPoint;

	public int onSlope { get; private set; }

	public PhysicalObject owner { get; private set; }

	public float submersion
	{
		get
		{
			if (owner.room == null)
			{
				return 0f;
			}
			if (owner.room.waterInverted)
			{
				return 1f - Mathf.InverseLerp(pos.y - rad, pos.y + rad, owner.room.FloatWaterLevel(pos.x));
			}
			float num = owner.room.FloatWaterLevel(pos.x);
			if (ModManager.MMF && !MMF.cfgVanillaExploits.Value && num > (float)(owner.room.abstractRoom.size.y + 20) * 20f)
			{
				return 1f;
			}
			return Mathf.InverseLerp(pos.y - rad, pos.y + rad, num);
		}
	}

	public float VisibilityBonus(float movementBasedVision)
	{
		return Mathf.Clamp(Mathf.Lerp(0f - movementBasedVision, 1f, Mathf.Pow(Mathf.InverseLerp(1f, 10f, Vector2.Distance(lastPos, pos)), 1f)) * movementBasedVision + owner.VisibilityBonus * (1f - movementBasedVision * 0.5f), -1f, 1f);
	}

	public BodyChunk(PhysicalObject owner, int index, Vector2 pos, float rad, float mass)
	{
		this.owner = owner;
		this.index = index;
		this.pos = pos;
		lastPos = pos;
		lastLastPos = pos;
		vel = new Vector2(0f, 0f);
		this.rad = rad;
		this.mass = mass;
		contactPoint = new IntVector2(0, 0);
		onSlope = 0;
		terrainSqueeze = 1f;
		collideWithTerrain = true;
		goThroughFloors = false;
		collideWithSlopes = true;
		grabsDisableFloors = true;
	}

	public void Update()
	{
		if (float.IsNaN(vel.y))
		{
			Custom.LogWarning("VELY IS NAN");
			vel.y = 0f;
		}
		if (float.IsNaN(vel.x))
		{
			Custom.LogWarning("VELX IS NAN");
			vel.x = 0f;
		}
		vel.y -= owner.gravity;
		bool flag = ((!ModManager.MSC) ? (pos.y - rad <= owner.room.FloatWaterLevel(pos.x)) : owner.room.PointSubmerged(new Vector2(pos.x, pos.y - rad)));
		if (owner.room.water && flag)
		{
			if (vel.x > vel.y * 5f && ((Mathf.Abs(vel.x) > 10f) & (vel.y < 0f)) && submersion < 0.5f)
			{
				vel.y *= -0.5f;
				vel.x *= 0.75f;
			}
			else
			{
				float effectiveRoomGravity = owner.EffectiveRoomGravity;
				vel.y += owner.buoyancy * effectiveRoomGravity * submersion;
				vel *= Mathf.Lerp(owner.airFriction, Mathf.Lerp(owner.waterFriction * owner.waterRetardationImmunity, owner.waterFriction, Mathf.Pow(1f / Mathf.Max(1f, vel.magnitude - 10f), 0.5f)), submersion);
			}
		}
		else
		{
			vel *= owner.airFriction;
		}
		lastLastPos = lastPos;
		lastPos = pos;
		if (setPos.HasValue)
		{
			pos = setPos.Value;
			setPos = null;
		}
		else
		{
			pos += vel;
		}
		onSlope = 0;
		slopeRad = TerrainRad;
		lastContactPoint = contactPoint;
		if (collideWithTerrain)
		{
			CheckVerticalCollision();
			if (collideWithSlopes)
			{
				checkAgainstSlopesVertically();
			}
			CheckHorizontalCollision();
		}
		else
		{
			contactPoint.x = 0;
			contactPoint.y = 0;
		}
		if (owner.grabbedBy.Count == 0)
		{
			if (pos.x < 0f - restrictInRoomRange)
			{
				vel.x = 0f;
				pos.x = 0f - restrictInRoomRange;
			}
			else if (pos.x > owner.room.PixelWidth + restrictInRoomRange)
			{
				vel.x = 0f;
				pos.x = owner.room.PixelWidth + restrictInRoomRange;
			}
			if (pos.y < 0f - restrictInRoomRange)
			{
				vel.y = 0f;
				pos.y = 0f - restrictInRoomRange;
			}
			else if (pos.y > owner.room.PixelHeight + restrictInRoomRange)
			{
				vel.y = 0f;
				pos.y = owner.room.PixelHeight + restrictInRoomRange;
			}
		}
		if ((splashStop == 10 && (submersion == 0f || submersion == 1f)) || (splashStop != 10 && splashStop > 0))
		{
			splashStop--;
		}
	}

	public void HardSetPosition(Vector2 newPos)
	{
		pos = newPos;
		lastPos = newPos;
		lastLastPos = newPos;
		if (setPos.HasValue)
		{
			setPos = newPos;
		}
	}

	public void MoveFromOutsideMyUpdate(bool eu, Vector2 moveTo)
	{
		if (owner.evenUpdate == eu)
		{
			pos = moveTo;
		}
		else
		{
			setPos = moveTo;
		}
	}

	public void RelativeMoveFromOutsideMyUpdate(bool eu, Vector2 move)
	{
		if (owner.evenUpdate == eu)
		{
			pos += move;
		}
		else
		{
			setPos = pos + move;
		}
	}

	public void MoveWithOtherObject(bool eu, BodyChunk otherChunk, Vector2 relativePosition)
	{
		if (otherChunk.owner.evenUpdate == eu)
		{
			if (owner.evenUpdate == eu)
			{
				pos = otherChunk.pos + relativePosition;
			}
			else
			{
				setPos = otherChunk.pos + relativePosition;
			}
			return;
		}
		if (owner.room.chunkGlue == null)
		{
			owner.room.chunkGlue = new List<ChunkGlue>();
		}
		owner.room.chunkGlue.Add(new ChunkGlue(this, otherChunk, relativePosition));
	}

	private void CheckHorizontalCollision()
	{
		contactPoint.x = 0;
		IntVector2 tilePosition = owner.room.GetTilePosition(lastPos);
		int num = 0;
		if (vel.x > 0f)
		{
			int x = owner.room.GetTilePosition(new Vector2(pos.x + TerrainRad + 0.01f, 0f)).x;
			int x2 = owner.room.GetTilePosition(new Vector2(lastPos.x + TerrainRad, 0f)).x;
			int y = owner.room.GetTilePosition(new Vector2(0f, pos.y + TerrainRad - 1f)).y;
			int y2 = owner.room.GetTilePosition(new Vector2(0f, pos.y - TerrainRad + 1f)).y;
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
					if (owner.room.GetTile(i, j).Terrain == Room.Tile.TerrainType.Solid && owner.room.GetTile(i - 1, j).Terrain != Room.Tile.TerrainType.Solid && (tilePosition.x < i || owner.room.GetTile(lastPos).Terrain == Room.Tile.TerrainType.Solid))
					{
						pos.x = (float)i * 20f - TerrainRad;
						if (vel.x > owner.impactTreshhold)
						{
							owner.TerrainImpact(index, new IntVector2(1, 0), Mathf.Abs(vel.x), lastContactPoint.x < 1);
						}
						contactPoint.x = 1;
						vel.x = (0f - Mathf.Abs(vel.x)) * owner.bounce;
						if (Mathf.Abs(vel.x) < 1f + 9f * (1f - owner.bounce))
						{
							vel.x = 0f;
						}
						vel.y *= Mathf.Clamp(owner.surfaceFriction * 2f, 0f, 1f);
						flag = true;
					}
					num++;
					if (num > MaxRepeats)
					{
						Custom.LogWarning($"!!!!! {owner} emergency breakout of terrain check!");
						flag = true;
					}
				}
			}
		}
		else
		{
			if (!(vel.x < 0f))
			{
				return;
			}
			int x3 = owner.room.GetTilePosition(new Vector2(pos.x - TerrainRad - 0.01f, 0f)).x;
			int x4 = owner.room.GetTilePosition(new Vector2(lastPos.x - TerrainRad, 0f)).x;
			int y3 = owner.room.GetTilePosition(new Vector2(0f, pos.y + TerrainRad - 1f)).y;
			int y4 = owner.room.GetTilePosition(new Vector2(0f, pos.y - TerrainRad + 1f)).y;
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
					if (owner.room.GetTile(num2, k).Terrain == Room.Tile.TerrainType.Solid && owner.room.GetTile(num2 + 1, k).Terrain != Room.Tile.TerrainType.Solid && (tilePosition.x > num2 || owner.room.GetTile(lastPos).Terrain == Room.Tile.TerrainType.Solid))
					{
						pos.x = ((float)num2 + 1f) * 20f + TerrainRad;
						if (vel.x < 0f - owner.impactTreshhold)
						{
							owner.TerrainImpact(index, new IntVector2(-1, 0), Mathf.Abs(vel.x), lastContactPoint.x > -1);
						}
						contactPoint.x = -1;
						vel.x = Mathf.Abs(vel.x) * owner.bounce;
						if (Mathf.Abs(vel.x) < 1f + 9f * (1f - owner.bounce))
						{
							vel.x = 0f;
						}
						vel.y *= Mathf.Clamp(owner.surfaceFriction * 2f, 0f, 1f);
						flag2 = true;
					}
					num++;
					if (num > MaxRepeats)
					{
						Custom.LogWarning($"!!!!! {owner} emergency breakout of terrain check!");
						flag2 = true;
					}
				}
				num2--;
			}
		}
	}

	private void CheckVerticalCollision()
	{
		contactPoint.y = 0;
		IntVector2 tilePosition = owner.room.GetTilePosition(lastPos);
		int num = 0;
		if (vel.y > 0f)
		{
			int y = owner.room.GetTilePosition(new Vector2(0f, pos.y + TerrainRad + 0.01f)).y;
			int y2 = owner.room.GetTilePosition(new Vector2(0f, lastPos.y + TerrainRad)).y;
			int x = owner.room.GetTilePosition(new Vector2(pos.x - TerrainRad + 1f, 0f)).x;
			int x2 = owner.room.GetTilePosition(new Vector2(pos.x + TerrainRad - 1f, 0f)).x;
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
					if (owner.room.GetTile(j, i).Terrain == Room.Tile.TerrainType.Solid && owner.room.GetTile(j, i - 1).Terrain != Room.Tile.TerrainType.Solid && (tilePosition.y < i || owner.room.GetTile(lastPos).Terrain == Room.Tile.TerrainType.Solid))
					{
						pos.y = (float)i * 20f - TerrainRad;
						if (vel.y > owner.impactTreshhold)
						{
							owner.TerrainImpact(index, new IntVector2(0, 1), Mathf.Abs(vel.y), lastContactPoint.y < 1);
						}
						contactPoint.y = 1;
						vel.y = (0f - Mathf.Abs(vel.y)) * owner.bounce;
						if (Mathf.Abs(vel.y) < 1f + 9f * (1f - owner.bounce))
						{
							vel.y = 0f;
						}
						vel.x *= Mathf.Clamp(owner.surfaceFriction * 2f, 0f, 1f);
						flag = true;
					}
					num++;
					if (num > MaxRepeats)
					{
						Custom.LogWarning($"!!!!! {owner} emergency breakout of terrain check!");
						flag = true;
					}
				}
			}
		}
		else
		{
			if (!(vel.y < 0f))
			{
				return;
			}
			int y3 = owner.room.GetTilePosition(new Vector2(0f, pos.y - TerrainRad - 0.01f)).y;
			int y4 = owner.room.GetTilePosition(new Vector2(0f, lastPos.y - TerrainRad)).y;
			int x3 = owner.room.GetTilePosition(new Vector2(pos.x - TerrainRad + 1f, 0f)).x;
			int x4 = owner.room.GetTilePosition(new Vector2(pos.x + TerrainRad - 1f, 0f)).x;
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
					if (SolidFloor(k, num2) && !SolidFloor(k, num2 + 1) && (tilePosition.y > num2 || owner.room.GetTile(lastPos).Terrain == Room.Tile.TerrainType.Solid))
					{
						pos.y = ((float)num2 + 1f) * 20f + TerrainRad;
						if (vel.y < 0f - owner.impactTreshhold)
						{
							owner.TerrainImpact(index, new IntVector2(0, -1), Mathf.Abs(vel.y), lastContactPoint.y > -1);
						}
						contactPoint.y = -1;
						vel.y = Mathf.Abs(vel.y) * owner.bounce;
						if (vel.y < owner.gravity || vel.y < 1f + 9f * (1f - owner.bounce))
						{
							vel.y = 0f;
						}
						vel.x *= Mathf.Clamp(owner.surfaceFriction * 2f, 0f, 1f);
						flag2 = true;
					}
					num++;
					if (num > MaxRepeats)
					{
						Custom.LogWarning($"!!!!! {owner} emergency breakout of terrain check!");
						flag2 = true;
					}
				}
				num2--;
			}
		}
	}

	private void checkAgainstSlopesVertically()
	{
		IntVector2 tilePosition = owner.room.GetTilePosition(pos);
		IntVector2 intVector = new IntVector2(0, 0);
		Room.SlopeDirection slopeDirection = owner.room.IdentifySlope(pos);
		if (owner.room.GetTile(pos).Terrain != Room.Tile.TerrainType.Slope)
		{
			if (owner.room.IdentifySlope(tilePosition.x - 1, tilePosition.y) != Room.SlopeDirection.Broken && pos.x - slopeRad <= owner.room.MiddleOfTile(pos).x - 10f)
			{
				slopeDirection = owner.room.IdentifySlope(tilePosition.x - 1, tilePosition.y);
				intVector.x = -1;
			}
			else if (owner.room.IdentifySlope(tilePosition.x + 1, tilePosition.y) != Room.SlopeDirection.Broken && pos.x + slopeRad >= owner.room.MiddleOfTile(pos).x + 10f)
			{
				slopeDirection = owner.room.IdentifySlope(tilePosition.x + 1, tilePosition.y);
				intVector.x = 1;
			}
			else if (pos.y - slopeRad < owner.room.MiddleOfTile(pos).y - 10f)
			{
				if (owner.room.IdentifySlope(tilePosition.x, tilePosition.y - 1) != Room.SlopeDirection.Broken)
				{
					slopeDirection = owner.room.IdentifySlope(tilePosition.x, tilePosition.y - 1);
					intVector.y = -1;
				}
			}
			else if (pos.y + slopeRad > owner.room.MiddleOfTile(pos).y + 10f && owner.room.IdentifySlope(tilePosition.x, tilePosition.y + 1) != Room.SlopeDirection.Broken)
			{
				slopeDirection = owner.room.IdentifySlope(tilePosition.x, tilePosition.y + 1);
				intVector.y = 1;
			}
		}
		if (slopeDirection != Room.SlopeDirection.Broken)
		{
			Vector2 vector = owner.room.MiddleOfTile(owner.room.GetTilePosition(pos) + intVector);
			int num = 0;
			float num2 = 0f;
			int num3 = 0;
			if (slopeDirection == Room.SlopeDirection.UpLeft)
			{
				num = -1;
				num2 = pos.x - (vector.x - 10f) + (vector.y - 10f);
				num3 = -1;
			}
			else if (slopeDirection == Room.SlopeDirection.UpRight)
			{
				num = 1;
				num2 = 20f - (pos.x - (vector.x - 10f)) + (vector.y - 10f);
				num3 = -1;
			}
			else if (slopeDirection == Room.SlopeDirection.DownLeft)
			{
				num2 = 20f - (pos.x - (vector.x - 10f)) + (vector.y - 10f);
				num3 = 1;
			}
			else
			{
				num2 = pos.x - (vector.x - 10f) + (vector.y - 10f);
				num3 = 1;
			}
			if (num3 == -1 && pos.y <= num2 + slopeRad + slopeRad)
			{
				pos.y = num2 + slopeRad + slopeRad;
				contactPoint.y = -1;
				vel.x *= 1f - owner.surfaceFriction;
				vel.x += Mathf.Abs(vel.y) * Mathf.Clamp(0.5f - owner.surfaceFriction, 0f, 0.5f) * (float)num * 0.2f;
				vel.y = 0f;
				onSlope = num;
				slopeRad = TerrainRad - 1f;
			}
			else if (num3 == 1 && pos.y >= num2 - slopeRad - slopeRad)
			{
				pos.y = num2 - slopeRad - slopeRad;
				contactPoint.y = 1;
				vel.y = 0f;
				vel.x *= 1f - owner.surfaceFriction;
				slopeRad = TerrainRad - 1f;
			}
		}
	}

	private bool SolidFloor(int X, int Y)
	{
		if (owner.room.GetTile(X, Y).Terrain == Room.Tile.TerrainType.Solid)
		{
			return true;
		}
		if (owner.room.GetTile(X, Y).Terrain == Room.Tile.TerrainType.Floor && !goThroughFloors)
		{
			float num = owner.room.PixelHeight;
			for (int i = 0; i < owner.bodyChunks.Length; i++)
			{
				if (owner.bodyChunks[i].lastPos.y < num)
				{
					num = owner.bodyChunks[i].lastPos.y;
				}
			}
			if (owner.room.GetTilePosition(new Vector2(0f, num)).y > Y)
			{
				return true;
			}
			return false;
		}
		return false;
	}
}
