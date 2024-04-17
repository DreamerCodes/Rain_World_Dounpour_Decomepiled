using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class ClimbableVine : Vine, IClimbableVine
{
	public class ClimbVineGraphic : VineGraphic
	{
		public ClimbVineGraphic(Vine owner, int parts, int firstSprite)
			: base(owner, parts, firstSprite)
		{
			sprites = 1;
			if (owner.room.abstractRoom.singleRealizedRoom)
			{
				leaves = new Leaf[Math.Max(0, parts / 2 + UnityEngine.Random.Range(-1, 6))];
			}
			else
			{
				leaves = new Leaf[Math.Max(0, parts + UnityEngine.Random.Range(-5, 6))];
			}
			for (int i = 0; i < leaves.Length; i++)
			{
				leaves[i] = new Leaf(new Vector2(Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 2f, UnityEngine.Random.value), Mathf.Pow(UnityEngine.Random.value, 1.4f));
				sprites++;
			}
		}

		public override void Update()
		{
			for (int i = 0; i < segments.Length; i++)
			{
				segments[i].lastPos = segments[i].pos;
				segments[i].pos = (owner as ClimbableVine).OnVinePos((float)i / (float)(segments.Length - 1));
			}
		}

		public override float OnVineEffectColorFac(float floatPos)
		{
			return Mathf.Clamp(Mathf.Sin(floatPos * (float)Math.PI) * 0.4f, 0.3f, 1f);
		}
	}

	public Rope[] ropes;

	public List<Vector2> possList;

	public ClimbableVine(Room room, int firstSprite, PlacedObject placedObject)
		: base(room, ((placedObject.data as PlacedObject.ResizableObjectData).handlePos.magnitude * 1.1f + 50f) / 3f, placedObject.pos, placedObject.pos + (placedObject.data as PlacedObject.ResizableObjectData).handlePos, stuckAtA: true, stuckAtB: true)
	{
		base.room = room;
		graphic = new ClimbVineGraphic(this, Custom.IntClamp((int)(((placedObject.data as PlacedObject.ResizableObjectData).handlePos.magnitude * 1.1f + 50f) / 10f), 1, 200), firstSprite);
		if (room.climbableVines == null)
		{
			room.climbableVines = new ClimbableVinesSystem();
			room.AddObject(room.climbableVines);
		}
		room.climbableVines.vines.Add(this);
		ropes = new Rope[segments.GetLength(0) - 1];
		for (int i = 0; i < ropes.Length; i++)
		{
			ropes[i] = new Rope(room, segments[i, 0], segments[i + 1, 0], 2f);
		}
		conRad *= 3f;
		pushApart /= 3f;
		possList = new List<Vector2>();
		for (int j = 0; j < segments.GetLength(0); j++)
		{
			possList.Add(segments[j, 0]);
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		for (int i = 0; i < segments.GetLength(0); i++)
		{
			possList[i] = segments[i, 0];
			segments[i, 2] += Custom.RNV() * 0.15f * UnityEngine.Random.value * Mathf.Sin((float)i / (float)(segments.GetLength(0) - 1) * (float)Math.PI) * (1f - room.gravity);
		}
		for (int j = 0; j < ropes.Length; j++)
		{
			if (ropes[j].bends.Count > 3)
			{
				ropes[j].Reset();
			}
			ropes[j].Update(segments[j, 0], segments[j + 1, 0]);
			if (ropes[j].totalLength > conRad)
			{
				Vector2 vector = Custom.DirVec(segments[j, 0], ropes[j].AConnect);
				segments[j, 0] += vector * (ropes[j].totalLength - conRad) * 0.5f;
				segments[j, 2] += vector * (ropes[j].totalLength - conRad) * 0.5f;
				vector = Custom.DirVec(segments[j + 1, 0], ropes[j].BConnect);
				segments[j + 1, 0] += vector * (ropes[j].totalLength - conRad) * 0.5f;
				segments[j + 1, 2] += vector * (ropes[j].totalLength - conRad) * 0.5f;
			}
		}
		graphic.Update();
	}

	public override float GravityAffected(int seg)
	{
		return Mathf.Min(Mathf.InverseLerp(2f, 5f, seg), Mathf.InverseLerp(segments.GetLength(0) - 3, segments.GetLength(0) - 6, seg));
	}

	public Vector2 OnVinePos(float ps)
	{
		int num = Custom.IntClamp(Mathf.FloorToInt(ps * (float)(segments.GetLength(0) - 1)), 0, segments.GetLength(0) - 1);
		int num2 = Custom.IntClamp(num + 1, 0, segments.GetLength(0) - 1);
		float f = Mathf.InverseLerp(num, num2, ps * (float)(segments.GetLength(0) - 1));
		Vector2 cA = segments[num, 0] - (segments[Custom.IntClamp(num - 1, 0, segments.GetLength(0) - 1), 0] - segments[num, 0]).normalized * Vector2.Distance(segments[num, 0], segments[num2, 0]) * 0.25f;
		Vector2 cB = segments[num2, 0] - (segments[Custom.IntClamp(num2 + 1, 0, segments.GetLength(0) - 1), 0] - segments[num2, 0]).normalized * Vector2.Distance(segments[num, 0], segments[num2, 0]) * 0.25f;
		return Custom.Bezier(segments[num, 0], cA, segments[num2, 0], cB, f);
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
		return 0.25f;
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
}
