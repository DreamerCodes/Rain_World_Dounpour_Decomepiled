using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class VoidSpawn : PhysicalObject
{
	public abstract class Behavior
	{
		public VoidSpawn owner;

		public virtual Vector2 SwimTowards => owner.mainBody[0].pos;

		public Behavior(VoidSpawn owner)
		{
			this.owner = owner;
		}
	}

	public class PassThrough : Behavior
	{
		public int toRoom;

		public Vector2 pnt;

		public Vector2 finalDest;

		public override Vector2 SwimTowards => Vector2.Lerp(pnt, finalDest, Mathf.InverseLerp(500f, 10f, Mathf.Abs(Custom.DistanceToLine(owner.mainBody[0].pos, pnt, finalDest))));

		public PassThrough(VoidSpawn owner, int toRoom, Room room)
			: base(owner)
		{
			this.toRoom = toRoom;
			pnt = room.RandomPos();
			Vector2 p = room.world.RoomToWorldPos(owner.mainBody[0].pos, room.abstractRoom.index);
			Vector2 vector = room.world.RoomToWorldPos(new Vector2((float)room.world.GetAbstractRoom(toRoom).size.x * UnityEngine.Random.value * 20f, (float)room.world.GetAbstractRoom(toRoom).size.y * UnityEngine.Random.value * 20f), toRoom);
			finalDest = vector - room.world.RoomToWorldPos(new Vector2(0f, 0f), room.abstractRoom.index);
			finalDest += Custom.DirVec(p, vector) * 2000f;
		}
	}

	public class MillAround : Behavior
	{
		public Room room;

		public Vector2 dest;

		public FloatRect rect;

		public int destShifts;

		public override Vector2 SwimTowards
		{
			get
			{
				if (Custom.DistLess(dest, owner.mainBody[0].pos, 100f))
				{
					NewDest();
				}
				return dest;
			}
		}

		public MillAround(VoidSpawn owner, Room room)
			: base(owner)
		{
			this.room = room;
			rect = MillRectInRoom(room.abstractRoom.name);
			NewDest();
		}

		public void NewDest()
		{
			destShifts++;
			if (destShifts > UnityEngine.Random.Range(4, 8))
			{
				dest = room.RandomPos() + Custom.RNV() * 50000f;
				dest = Custom.RectCollision(room.RandomPos(), dest, room.RoomRect.Grow(700f)).GetCorner(FloatRect.CornerLabel.D);
			}
			else
			{
				dest = Custom.RandomPointInRect(rect);
			}
		}

		public static FloatRect MillRectInRoom(string roomName)
		{
			switch (roomName)
			{
			case "SH_D02":
				return new FloatRect(900f, 260f, 3800f, 360f);
			case "SH_E02":
				return new FloatRect(380f, 260f, 2460f, 360f);
			case "SB_D06":
				if (ModManager.MSC)
				{
					return new FloatRect(-140f, 60f, 1100f, 370f);
				}
				break;
			case "SB_E05SAINT":
				if (ModManager.MSC)
				{
					return new FloatRect(-140f, 846f, 1120f, 1570f);
				}
				break;
			case "SB_L01":
				return new FloatRect(2000f, 1170f, 3500f, 1300f);
			}
			return default(FloatRect);
		}
	}

	public class VoidSeaDive : Behavior
	{
		public Vector2 pnt;

		public Vector2 finalDest;

		public override Vector2 SwimTowards => Vector2.Lerp(pnt, finalDest, Mathf.InverseLerp(700f, 100f, Mathf.Abs(Custom.DistanceToLine(owner.mainBody[0].pos, pnt, finalDest))));

		public VoidSeaDive(VoidSpawn owner, Room room)
			: base(owner)
		{
			finalDest = new Vector2(2500f, 360f);
			pnt = Custom.RandomPointInRect(new FloatRect(2000f, 1170f, 3500f, 1300f));
		}
	}

	public class EggToExit : Behavior
	{
		public int toExit;

		public Vector2 pnt;

		public Vector2 finalDest;

		private bool allTheWayToShortcut;

		private bool phase;

		public override Vector2 SwimTowards
		{
			get
			{
				if (!phase)
				{
					if (Custom.DistLess(owner.mainBody[0].pos, pnt, allTheWayToShortcut ? 70f : 170f))
					{
						phase = true;
					}
					return pnt;
				}
				if (allTheWayToShortcut)
				{
					return finalDest;
				}
				return Vector2.Lerp(pnt, finalDest, Mathf.InverseLerp(200f, 10f, Mathf.Abs(Custom.DistanceToLine(owner.mainBody[0].pos, pnt, finalDest))));
			}
		}

		public EggToExit(VoidSpawn owner, int toExit, Room room, bool allTheWayToShortcut)
			: base(owner)
		{
			this.toExit = toExit;
			this.allTheWayToShortcut = allTheWayToShortcut;
			pnt = room.MiddleOfTile(room.ShortcutLeadingToNode(toExit).StartTile);
			if (!allTheWayToShortcut)
			{
				pnt += Custom.RNV() * 70f * UnityEngine.Random.value;
			}
			int num = room.abstractRoom.connections[toExit];
			if (num > -1)
			{
				Vector2 p = room.world.RoomToWorldPos(owner.mainBody[0].pos, room.abstractRoom.index);
				Vector2 vector = room.world.RoomToWorldPos(new Vector2((float)room.world.GetAbstractRoom(num).size.x * UnityEngine.Random.value * 20f, (float)room.world.GetAbstractRoom(num).size.y * UnityEngine.Random.value * 20f), num);
				finalDest = vector - room.world.RoomToWorldPos(new Vector2(0f, 0f), room.abstractRoom.index);
				finalDest += Custom.DirVec(p, vector) * 2000f;
			}
			else
			{
				finalDest = pnt + Custom.DirVec(room.RandomPos(), pnt) * 10000f;
			}
		}
	}

	public class EggAndAway : Behavior
	{
		public Vector2 pnt;

		public Vector2 finalDest;

		private bool phase;

		public override Vector2 SwimTowards
		{
			get
			{
				if (!phase)
				{
					if (Custom.DistLess(owner.mainBody[0].pos, pnt, 170f))
					{
						phase = true;
					}
					return pnt;
				}
				return Vector2.Lerp(pnt, finalDest, Mathf.InverseLerp(200f, 10f, Mathf.Abs(Custom.DistanceToLine(owner.mainBody[0].pos, pnt, finalDest))));
			}
		}

		public EggAndAway(VoidSpawn owner, Room room)
			: base(owner)
		{
			pnt = owner.firstChunk.pos + Custom.RNV() * UnityEngine.Random.value * 400f;
			finalDest = owner.firstChunk.pos + Custom.RNV() * Mathf.Max(room.PixelWidth, room.PixelHeight) * 3f;
		}
	}

	public class SwimDown : Behavior
	{
		public override Vector2 SwimTowards => new Vector2(owner.mainBody[0].pos.x, owner.mainBody[1].pos.y - 100f);

		public SwimDown(VoidSpawn owner, Room room)
			: base(owner)
		{
		}
	}

	public BodyChunk[] mainBody;

	public float swimCycle;

	public float sizeFac;

	public float swimSpeed;

	public Behavior behavior;

	public bool canBeDestroyed;

	public bool culled;

	public bool lastCulled;

	public float fade;

	public float lastFade;

	public float inEggMode;

	public bool consious = true;

	public VoidSpawnEgg egg;

	public float voidMeltInRoom;

	public bool dayLightMode;

	public VoidSpawn(AbstractPhysicalObject abstractPhysicalObject, float voidMeltInRoom, bool dayLightMode)
		: base(abstractPhysicalObject)
	{
		GenerateBody();
		this.voidMeltInRoom = voidMeltInRoom;
		this.dayLightMode = dayLightMode;
		base.airFriction = 1f;
		base.gravity = 0f;
		bounce = 0f;
		surfaceFriction = 0.4f;
		collisionLayer = 0;
		base.waterFriction = 1f;
		base.buoyancy = 0f;
		swimCycle = UnityEngine.Random.value;
		base.CollideWithTerrain = false;
		base.CollideWithObjects = false;
		canBeHitByWeapons = false;
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		base.PlaceInRoom(placeRoom);
		Vector2 vector = placeRoom.MiddleOfTile(abstractPhysicalObject.pos);
		Vector2 vector2 = Custom.RNV();
		if (behavior != null)
		{
			vector2 = Custom.DirVec(vector, behavior.SwimTowards);
		}
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			base.bodyChunks[i].HardSetPosition(vector + vector2 * 2f);
		}
		if (base.graphicsModule != null)
		{
			base.graphicsModule.Reset();
		}
	}

	private void GenerateBody()
	{
		int num = UnityEngine.Random.Range(3, UnityEngine.Random.Range(3, 16));
		int num2 = 0;
		List<BodyChunk> list = new List<BodyChunk>();
		List<BodyChunkConnection> list2 = new List<BodyChunkConnection>();
		float num3 = Mathf.Lerp(3f, 8f, UnityEngine.Random.value);
		float num4 = Mathf.Lerp(Mathf.Lerp(0.5f, 4f, UnityEngine.Random.value), num3 / 2f, UnityEngine.Random.value);
		float p = Mathf.Lerp(0.1f, 0.7f, UnityEngine.Random.value);
		sizeFac = Mathf.Lerp(0.5f, 1.2f, UnityEngine.Random.value);
		swimSpeed = Mathf.Lerp(0.5f, 1f, UnityEngine.Random.value);
		for (int i = 0; i < num; i++)
		{
			float num5 = (float)i / (float)(num - 1);
			float num6 = Mathf.Lerp(Mathf.Lerp(num3, num4, num5), Mathf.Lerp(num4, num3, Mathf.Sin(Mathf.Pow(num5, p) * (float)Math.PI)), 0.5f) * sizeFac;
			list.Add(new BodyChunk(this, num2, default(Vector2), num6, num6 * 0.1f));
			if (i > 0)
			{
				list2.Add(new BodyChunkConnection(list[i - 1], list[i], Mathf.Lerp((list[i - 1].rad + list[i].rad) * 1.25f, Mathf.Max(list[i - 1].rad, list[i].rad), 0.5f), BodyChunkConnection.Type.Normal, 1f, -1f));
			}
			num2++;
		}
		mainBody = list.ToArray();
		base.bodyChunks = list.ToArray();
		bodyChunkConnections = list2.ToArray();
	}

	public override void InitiateGraphicsModule()
	{
		if (base.graphicsModule == null)
		{
			base.graphicsModule = new VoidSpawnGraphics(this);
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		lastFade = fade;
		if (!dayLightMode && room.PointSubmerged(mainBody[0].pos))
		{
			fade = Mathf.Max(0f, fade - 0.05f);
			if (fade == 0f)
			{
				Destroy();
			}
		}
		else
		{
			fade = Mathf.Min(1f, fade + 0.025f);
		}
		if (inEggMode > 0f)
		{
			inEggMode = Mathf.Max(0f, inEggMode - 1f / 60f);
			if (inEggMode <= 0f)
			{
				egg = null;
			}
		}
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			mainBody[i].vel *= Custom.LerpMap(mainBody[i].vel.magnitude, 0.2f * sizeFac, 6f * sizeFac, 1f, 0.7f);
		}
		if (consious)
		{
			swimCycle += 0.2f * swimSpeed;
			Vector2 p = mainBody[mainBody.Length - 1].pos + Custom.DirVec(mainBody[0].pos, mainBody[mainBody.Length - 1].pos) * 100f;
			if (behavior != null)
			{
				p = behavior.SwimTowards;
			}
			Vector2 vector = Custom.DirVec(mainBody[0].pos, p);
			for (int j = 0; j < mainBody.Length; j++)
			{
				float num = (float)j / ((float)mainBody.Length - 1f);
				mainBody[j].vel += vector * Mathf.Lerp(1f, -1f, Mathf.InverseLerp(0f, 0.5f, num)) * Mathf.InverseLerp(0.5f, 0f, num) * 0.06f * sizeFac * swimSpeed;
				if (j < mainBody.Length - 1)
				{
					Vector2 vector2 = Custom.DirVec(mainBody[j + 1].pos, mainBody[j].pos);
					mainBody[j].vel += (vector2 + Custom.PerpendicularVector(vector2) * Mathf.Sin(swimCycle - (float)j * 1.2f) * 0.8f * Mathf.Pow(num, 0.3f)).normalized * 0.2f * sizeFac * swimSpeed;
				}
			}
			mainBody[0].vel += vector * 0.02f * sizeFac * Mathf.Pow(swimSpeed, 2f);
		}
		for (int k = 2; k < mainBody.Length; k++)
		{
			WeightedPush(k - 2, k, Custom.DirVec(mainBody[k].pos, mainBody[k - 2].pos), 0.1f * sizeFac);
		}
		if (!room.RoomRect.Vector2Inside(mainBody[0].pos) && !room.ViewedByAnyCamera(mainBody[0].pos, 200f))
		{
			if (canBeDestroyed)
			{
				Destroy();
			}
		}
		else
		{
			canBeDestroyed = true;
		}
		if (base.graphicsModule is VoidSpawnGraphics)
		{
			culled = !room.ViewedByAnyCamera(mainBody[0].pos, 300f) || !(base.graphicsModule as VoidSpawnGraphics).VisibleAtGlowDist(mainBody[0].pos, (base.graphicsModule as VoidSpawnGraphics).glowPos, 100f);
			if (!culled && lastCulled)
			{
				(base.graphicsModule as VoidSpawnGraphics).Reset();
			}
			lastCulled = culled;
		}
	}
}
