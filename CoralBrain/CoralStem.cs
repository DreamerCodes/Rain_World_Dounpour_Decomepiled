using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace CoralBrain;

public class CoralStem : UpdatableAndDeletable, IClimbableVine, IOwnProjectedCircles
{
	public CoralNeuronSystem system;

	public Vector2 rootPos;

	public Vector2 rootDirection;

	public IntVector2 rootTile;

	public List<StemSegment> segments;

	public float size;

	public bool withNeurons;

	public CoralStem(CoralNeuronSystem system, Vector2 rootPos, float rad, Room room, bool withNeurons)
	{
		this.system = system;
		base.room = room;
		this.withNeurons = withNeurons;
		size = Mathf.Clamp(rad / 600f, 0f, 1f);
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState((int)rootPos.x + (int)rootPos.y);
		if (room.climbableVines == null)
		{
			room.climbableVines = new ClimbableVinesSystem();
			room.AddObject(room.climbableVines);
		}
		room.climbableVines.vines.Add(this);
		this.rootPos = rootPos;
		segments = new List<StemSegment>();
		rootDirection = new Vector2(0f, 1f);
		rootTile = room.GetTilePosition(rootPos);
		for (int i = 0; i < 4; i++)
		{
			if (room.GetTile(rootTile + Custom.fourDirections[i]).Solid)
			{
				rootDirection = -Custom.fourDirections[i].ToVector2();
				if (Custom.fourDirections[i].x != 0)
				{
					rootPos.x = room.MiddleOfTile(rootTile).x - rootDirection.x * 9f;
				}
				else
				{
					rootPos.y = room.MiddleOfTile(rootTile).y - rootDirection.y * 9f;
				}
			}
		}
		IntVector2 intVector = rootTile;
		List<IntVector2> list = new List<IntVector2>();
		for (int j = 0; (float)j < Mathf.Lerp(2f, 60f, size); j++)
		{
			float num = float.MinValue;
			IntVector2 intVector2 = IntVector2.FromVector2(rootDirection);
			for (int k = 0; k < 4; k++)
			{
				if (!room.GetTile(intVector + Custom.fourDirections[k]).Solid && !room.aimap.getAItile(intVector + Custom.fourDirections[k]).narrowSpace)
				{
					float num2 = 0f;
					for (int l = 0; l < 4; l++)
					{
						num2 += Mathf.Pow(Math.Min(room.aimap.getTerrainProximity(intVector + Custom.fourDirections[k] + Custom.fourDirections[l]), 5), 3f);
					}
					num2 += UnityEngine.Random.value * 10f * Mathf.InverseLerp(10f, 30f, j);
					int num3 = list.Count - 1;
					while (num3 >= 0 && num3 > list.Count - 10)
					{
						num2 += list[num3].FloatDist(intVector + Custom.fourDirections[k]) * 0.3f;
						num3--;
					}
					if (num2 > num)
					{
						intVector2 = Custom.fourDirections[k];
						num = num2;
					}
				}
			}
			if (num == float.MinValue)
			{
				break;
			}
			list.Add(intVector);
			intVector += intVector2;
		}
		float num4 = 0f;
		while (num4 < (float)list.Count * 20f)
		{
			float num5 = 1f - Mathf.InverseLerp(0f, (float)list.Count * 20f, num4);
			num5 *= Mathf.Lerp(size, 1f, 0.05f);
			num5 = 1f - num5;
			num4 += Mathf.Lerp(6f, 12f, num5);
			int num6 = Mathf.FloorToInt(num4 / 20f);
			Vector2 vector = Vector2.Lerp(room.MiddleOfTile(list[Math.Min(list.Count - 1, num6)]), room.MiddleOfTile(list[Math.Min(list.Count - 1, num6 + 1)]), Mathf.InverseLerp((float)num6 * 20f, (float)(num6 + 1) * 20f, num4));
			num4 += Mathf.Lerp(6f, 68f, num5);
			num6 = Mathf.FloorToInt(num4 / 20f);
			Vector2 posB = Vector2.Lerp(room.MiddleOfTile(list[Math.Min(list.Count - 1, num6)]), room.MiddleOfTile(list[Math.Min(list.Count - 1, num6 + 1)]), Mathf.InverseLerp((float)num6 * 20f, (float)(num6 + 1) * 20f, num4));
			if (num4 > (float)list.Count * 20f)
			{
				num5 = 1f;
			}
			StemSegment stemSegment = new StemSegment(new AbstractPhysicalObject(room.world, AbstractPhysicalObject.AbstractObjectType.Creature, null, room.GetWorldCoordinate(vector), room.game.GetNewID()), this, 1f - num5, vector, posB);
			room.AddObject(stemSegment);
			if (segments.Count > 0)
			{
				stemSegment.prevSegment = segments[segments.Count - 1];
				stemSegment.prevSegment.nextSegment = stemSegment;
			}
			else
			{
				stemSegment.first = true;
			}
			segments.Add(stemSegment);
		}
		if (room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.SuperStructureProjector) > 0f && 0 == 0)
		{
			room.AddObject(new ProjectedCircle(room, this, 0, Mathf.Lerp(0.2f, 0.8f, size)));
		}
		UnityEngine.Random.state = state;
	}

	public Vector2 Pos(int index)
	{
		return segments[index / 2].bodyChunks[index % 2].pos;
	}

	public int TotalPositions()
	{
		return segments.Count * 2;
	}

	public float Rad(int index)
	{
		return segments[index / 2].bodyChunks[index % 2].rad;
	}

	public float Mass(int index)
	{
		return segments[index / 2].bodyChunks[index % 2].mass;
	}

	public void Push(int index, Vector2 movement)
	{
		segments[index / 2].bodyChunks[index % 2].pos += movement;
		segments[index / 2].bodyChunks[index % 2].vel += movement;
	}

	public void BeingClimbedOn(Creature crit)
	{
	}

	public bool CurrentlyClimbable()
	{
		return true;
	}

	public Vector2 CircleCenter(int index, float timeStacker)
	{
		return Vector2.Lerp(segments[segments.Count - 1].bodyChunks[1].lastPos, segments[segments.Count - 1].bodyChunks[1].pos, timeStacker);
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
