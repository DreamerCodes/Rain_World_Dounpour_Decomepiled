using RWCustom;
using UnityEngine;

public class BodyPart
{
	protected GraphicsModule owner;

	public Vector2 lastPos;

	public Vector2 pos;

	public Vector2 vel;

	public float rad;

	protected float surfaceFric;

	protected float airFriction;

	public bool terrainContact;

	public int bodyPartArrayIndex;

	public BodyPart(GraphicsModule ow)
	{
		owner = ow;
		bodyPartArrayIndex = -1;
	}

	public virtual void Update()
	{
	}

	public virtual void Reset(Vector2 resetPoint)
	{
		pos = resetPoint + Custom.DegToVec(Random.value * 360f);
		lastPos = pos;
		vel = new Vector2(0f, 0f);
	}

	public void ConnectToPoint(Vector2 pnt, float connectionRad, bool push, float elasticMovement, Vector2 hostVel, float adaptVel, float exaggerateVel)
	{
		if (elasticMovement > 0f)
		{
			vel += Custom.DirVec(pos, pnt) * Vector2.Distance(pos, pnt) * elasticMovement;
		}
		vel += hostVel * exaggerateVel;
		if (push || !Custom.DistLess(pos, pnt, connectionRad))
		{
			float num = Vector2.Distance(pos, pnt);
			Vector2 vector = Custom.DirVec(pos, pnt);
			pos -= (connectionRad - num) * vector * 1f;
			vel -= (connectionRad - num) * vector * 1f;
		}
		vel -= hostVel;
		vel *= 1f - adaptVel;
		vel += hostVel;
	}

	public void PushFromPoint(Vector2 pnt, float pushRad, float elasticity)
	{
		if (Custom.DistLess(pos, pnt, pushRad))
		{
			float num = Vector2.Distance(pos, pnt);
			Vector2 vector = Custom.DirVec(pos, pnt);
			pos -= (pushRad - num) * vector * elasticity;
			vel -= (pushRad - num) * vector * elasticity;
		}
	}

	public bool OnOtherSideOfTerrain(Vector2 conPos, float minAffectRadius)
	{
		if (Custom.DistLess(pos, conPos, minAffectRadius))
		{
			return false;
		}
		if (owner.owner.room.GetTile(pos).Solid)
		{
			return true;
		}
		IntVector2 a = owner.owner.room.GetTilePosition(conPos) - owner.owner.room.GetTilePosition(pos);
		a = IntVector2.ClampAtOne(a);
		if (a.x != 0 && a.y != 0)
		{
			if (Mathf.Abs(conPos.x - pos.x) > Mathf.Abs(conPos.y - pos.y))
			{
				a.y = 0;
			}
			else
			{
				a.x = 0;
			}
		}
		return owner.owner.room.GetTile(owner.owner.room.GetTilePosition(pos) + a).Solid;
	}

	public void PushOutOfTerrain(Room room, Vector2 basePoint)
	{
		terrainContact = false;
		for (int i = 0; i < 9; i++)
		{
			if (room.GetTile(room.GetTilePosition(pos) + Custom.eightDirectionsAndZero[i]).Terrain == Room.Tile.TerrainType.Solid)
			{
				Vector2 vector = room.MiddleOfTile(room.GetTilePosition(pos) + Custom.eightDirectionsAndZero[i]);
				float num = 0f;
				float num2 = 0f;
				if (pos.y >= vector.y - 10f && pos.y <= vector.y + 10f)
				{
					if (lastPos.x < vector.x)
					{
						if (pos.x > vector.x - 10f - rad && room.GetTile(room.GetTilePosition(pos) + Custom.eightDirectionsAndZero[i] + new IntVector2(-1, 0)).Terrain != Room.Tile.TerrainType.Solid)
						{
							num = vector.x - 10f - rad;
						}
					}
					else if (pos.x < vector.x + 10f + rad && room.GetTile(room.GetTilePosition(pos) + Custom.eightDirectionsAndZero[i] + new IntVector2(1, 0)).Terrain != Room.Tile.TerrainType.Solid)
					{
						num = vector.x + 10f + rad;
					}
				}
				if (pos.x >= vector.x - 10f && pos.x <= vector.x + 10f)
				{
					if (lastPos.y < vector.y)
					{
						if (pos.y > vector.y - 10f - rad && room.GetTile(room.GetTilePosition(pos) + Custom.eightDirectionsAndZero[i] + new IntVector2(0, -1)).Terrain != Room.Tile.TerrainType.Solid)
						{
							num2 = vector.y - 10f - rad;
						}
					}
					else if (pos.y < vector.y + 10f + rad && room.GetTile(room.GetTilePosition(pos) + Custom.eightDirectionsAndZero[i] + new IntVector2(0, 1)).Terrain != Room.Tile.TerrainType.Solid)
					{
						num2 = vector.y + 10f + rad;
					}
				}
				if (Mathf.Abs(pos.x - num) < Mathf.Abs(pos.y - num2) && num != 0f)
				{
					pos.x = num;
					vel.x = num - pos.x;
					vel.y *= surfaceFric;
					terrainContact = true;
					continue;
				}
				if (num2 != 0f)
				{
					pos.y = num2;
					vel.y = num2 - pos.y;
					vel.x *= surfaceFric;
					terrainContact = true;
					continue;
				}
				Vector2 vector2 = new Vector2(Mathf.Clamp(pos.x, vector.x - 10f, vector.x + 10f), Mathf.Clamp(pos.y, vector.y - 10f, vector.y + 10f));
				if (Custom.DistLess(pos, vector2, rad))
				{
					float num3 = Vector2.Distance(pos, vector2);
					Vector2 vector3 = Custom.DirVec(pos, vector2);
					vel *= surfaceFric;
					pos -= (rad - num3) * vector3;
					vel -= (rad - num3) * vector3;
					terrainContact = true;
				}
			}
			else
			{
				if (Custom.eightDirectionsAndZero[i].x != 0 || room.GetTile(room.GetTilePosition(pos) + Custom.eightDirectionsAndZero[i]).Terrain != Room.Tile.TerrainType.Slope)
				{
					continue;
				}
				Vector2 vector4 = room.MiddleOfTile(room.GetTilePosition(pos) + Custom.eightDirectionsAndZero[i]);
				if (room.IdentifySlope(room.GetTilePosition(pos) + Custom.eightDirectionsAndZero[i]) == Room.SlopeDirection.UpLeft)
				{
					if (pos.y < vector4.y - (vector4.x - pos.x) + rad)
					{
						pos.y = vector4.y - (vector4.x - pos.x) + rad;
						vel.y = 0f;
						vel.x *= surfaceFric;
						terrainContact = true;
					}
				}
				else if (room.IdentifySlope(room.GetTilePosition(pos) + Custom.eightDirectionsAndZero[i]) == Room.SlopeDirection.UpRight)
				{
					if (pos.y < vector4.y + (vector4.x - pos.x) + rad)
					{
						pos.y = vector4.y + (vector4.x - pos.x) + rad;
						vel.y = 0f;
						vel.x *= surfaceFric;
						terrainContact = true;
					}
				}
				else if (room.IdentifySlope(room.GetTilePosition(pos) + Custom.eightDirectionsAndZero[i]) == Room.SlopeDirection.DownLeft)
				{
					if (pos.y > vector4.y + (vector4.x - pos.x) - rad)
					{
						pos.y = vector4.y + (vector4.x - pos.x) - rad;
						vel.y = 0f;
						vel.x *= surfaceFric;
						terrainContact = true;
					}
				}
				else if (room.IdentifySlope(room.GetTilePosition(pos) + Custom.eightDirectionsAndZero[i]) == Room.SlopeDirection.DownRight && pos.y > vector4.y - (vector4.x - pos.x) - rad)
				{
					pos.y = vector4.y - (vector4.x - pos.x) - rad;
					vel.y = 0f;
					vel.x *= surfaceFric;
					terrainContact = true;
				}
			}
		}
	}
}
