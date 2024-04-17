using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public abstract class NeedleWorm : InsectoidCreature, ILookingAtCreatures
{
	public NeedleWormAI AI;

	public Rope[] connectionRopes;

	public Vector2[,] tail;

	public int[] segmentsInRopeMode;

	public bool small;

	public Vector2 lookDir;

	public Vector2 getToLookDir;

	public Vector2[] prevPositions;

	public int lastPosCounter;

	public float stuckAtSamePos;

	public float brokenLineOfSight;

	public float segmentsStuckOnTerrain;

	public float reallyStuckAtSamePos;

	public float extraMovementForce;

	public float flying;

	public bool flyingThisFrame;

	public bool atDestThisFrame;

	public float crawlSin;

	private int noShortcuts;

	public CreatureLooker creatureLooker;

	public float screaming;

	public WorldCoordinate quickMoveToExit;

	public int quickMoveToExitCounter;

	private SharedPhysics.TerrainCollisionData scratchTerrainCollisionData;

	public new NeedleWormAbstractAI.NeedleWormState State => base.abstractCreature.state as NeedleWormAbstractAI.NeedleWormState;

	public bool OffscreenSuperSpeed
	{
		get
		{
			if (AI.behavior == NeedleWormAI.Behavior.Migrate)
			{
				return !room.BeingViewed;
			}
			return false;
		}
	}

	public int TotalSegments => base.bodyChunks.Length + tail.GetLength(0);

	private float SlowFlySpeed => Mathf.Lerp(0.25f, small ? 0.7f : 0.5f, AI.flySpeed);

	public NeedleWorm(AbstractCreature abstractCreature, World world)
		: base(abstractCreature, world)
	{
		small = base.Template.type == CreatureTemplate.Type.SmallNeedleWorm;
		base.bodyChunks = new BodyChunk[small ? 3 : 5];
		float num = (small ? 0.7f : 1f);
		float num2 = 0f;
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			float num3 = Mathf.InverseLerp(0f, base.bodyChunks.Length - 1, i);
			float t = Mathf.Lerp(0.6f, Mathf.Clamp01(Mathf.Sin(Mathf.Pow(num3, 0.5f) * (float)Math.PI)), 0.5f + 0.5f * num3);
			base.bodyChunks[i] = new BodyChunk(this, i, default(Vector2), Mathf.Lerp(2f, 5f, t) * num, Mathf.Lerp(0.05f, 0.15f, t) * num);
			num2 += base.bodyChunks[i].mass;
		}
		_ = small;
		bodyChunkConnections = new BodyChunkConnection[base.bodyChunks.Length * (base.bodyChunks.Length - 1) / 2];
		int num4 = 0;
		for (int j = 0; j < base.bodyChunks.Length; j++)
		{
			for (int k = j + 1; k < base.bodyChunks.Length; k++)
			{
				bodyChunkConnections[num4] = new BodyChunkConnection(base.bodyChunks[j], base.bodyChunks[k], base.bodyChunks[j].rad + base.bodyChunks[k].rad, BodyChunkConnection.Type.Push, 1f, -1f);
				num4++;
			}
		}
		tail = new Vector2[small ? 4 : 10, 4];
		segmentsInRopeMode = new int[TotalSegments];
		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		bounce = 0.3f;
		surfaceFriction = 0.4f;
		collisionLayer = 1;
		base.waterFriction = 0.96f;
		base.buoyancy = 1.05f;
		prevPositions = new Vector2[10];
		quickMoveToExit = new WorldCoordinate(-1, -1, -1, -1);
	}

	public override void NewRoom(Room newRoom)
	{
		base.NewRoom(newRoom);
		connectionRopes = new Rope[TotalSegments - 1];
		for (int i = 0; i < connectionRopes.Length; i++)
		{
			connectionRopes[i] = new Rope(newRoom, GetSegmentPos(i), GetSegmentPos(i + 1), 0.1f);
		}
		ResetTail(base.bodyChunks[base.bodyChunks.Length - 1].pos);
	}

	public void ResetTail(Vector2 ps)
	{
		for (int i = 0; i < tail.GetLength(0); i++)
		{
			tail[i, 0] = ps + Custom.RNV() * UnityEngine.Random.value;
			tail[i, 1] = tail[i, 0];
			tail[i, 2] *= 0f;
		}
		for (int j = base.bodyChunks.Length; j < TotalSegments; j++)
		{
			segmentsInRopeMode[j] = 0;
		}
		if (connectionRopes != null)
		{
			for (int k = 0; k < connectionRopes.Length; k++)
			{
				connectionRopes[k].Reset();
			}
		}
	}

	public override void SpitOutOfShortCut(IntVector2 pos, Room newRoom, bool spitOutAllSticks)
	{
		base.SpitOutOfShortCut(pos, newRoom, spitOutAllSticks);
		Vector2 vector = (getToLookDir = (lookDir = Custom.IntVector2ToVector2(newRoom.ShorcutEntranceHoleDirection(pos))));
		flying = 0f;
		flyingThisFrame = false;
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			base.bodyChunks[i].HardSetPosition(newRoom.MiddleOfTile(pos) + vector * Custom.LerpMap(i, 0f, base.bodyChunks.Length, 7f, -2f) * (small ? 0.3f : 1f));
			base.bodyChunks[i].vel = vector * Custom.LerpMap(i, 0f, base.bodyChunks.Length, 10f, 1f);
		}
		ResetTail(base.bodyChunks[base.bodyChunks.Length - 1].pos);
		noShortcuts = 80;
	}

	public override void InitiateGraphicsModule()
	{
		if (base.graphicsModule == null)
		{
			base.graphicsModule = new NeedleWormGraphics(this);
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		atDestThisFrame = false;
		if (noShortcuts > 0)
		{
			noShortcuts--;
		}
		if (base.safariControlled)
		{
			noShortcuts = 0;
		}
		screaming = Mathf.Max(0f, screaming - 1f / 70f);
		surfaceFriction = (base.Consious ? 0.8f : 0.1f);
		int num = 0;
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			if (base.bodyChunks[i].ContactPoint.x != 0 || base.bodyChunks[i].ContactPoint.y != 0)
			{
				num++;
			}
		}
		if (room == null)
		{
			return;
		}
		if (base.abstractCreature.abstractAI.followCreature != null && base.Consious && base.abstractCreature.abstractAI.followCreature.pos.room != room.abstractRoom.index && !room.BeingViewed && base.abstractCreature.world.GetAbstractRoom(base.abstractCreature.abstractAI.followCreature.pos.room) != null && base.abstractCreature.world.GetAbstractRoom(base.abstractCreature.abstractAI.followCreature.pos.room).AttractionForCreature(base.Template.type) != AbstractRoom.CreatureRoomAttraction.Forbidden)
		{
			if (quickMoveToExitCounter > 0)
			{
				quickMoveToExitCounter--;
				if (quickMoveToExitCounter == 0 && quickMoveToExit.room == room.abstractRoom.index && quickMoveToExit.NodeDefined)
				{
					for (int j = 0; j < base.bodyChunks.Length; j++)
					{
						base.bodyChunks[j].HardSetPosition(room.MiddleOfTile(room.ShortcutLeadingToNode(quickMoveToExit.abstractNode).StartTile));
					}
					enteringShortCut = room.ShortcutLeadingToNode(quickMoveToExit.abstractNode).StartTile;
					quickMoveToExit = new WorldCoordinate(room.abstractRoom.index, -1, -1, -1);
				}
			}
			else
			{
				quickMoveToExit = new WorldCoordinate(room.abstractRoom.index, -1, -1, -1);
				for (int k = 0; k < room.abstractRoom.connections.Length; k++)
				{
					if (room.abstractRoom.connections[k] == base.abstractCreature.abstractAI.followCreature.pos.room)
					{
						int num2 = room.aimap.ExitDistanceForCreature(base.mainBodyChunk.pos, room.abstractRoom.CommonToCreatureSpecificNodeIndex(k, base.Template), base.Template);
						if (num2 > -1)
						{
							quickMoveToExit.abstractNode = k;
							quickMoveToExitCounter = num2 * 2;
						}
						break;
					}
				}
			}
		}
		else
		{
			quickMoveToExitCounter = 0;
		}
		if (room.game.devToolsActive && Input.GetKey("b") && room.game.cameras[0].room == room)
		{
			base.bodyChunks[0].vel += Custom.DirVec(base.bodyChunks[0].pos, (Vector2)Futile.mousePosition + room.game.cameras[0].pos) * 14f;
			Stun(12);
		}
		for (int l = 0; l < segmentsInRopeMode.Length; l++)
		{
			if (segmentsInRopeMode[l] < 10 && !room.GetTile(GetSegmentPos(l)).Solid && (l == TotalSegments - 1 || room.VisualContact(GetSegmentPos(l), GetSegmentPos(l + 1))))
			{
				segmentsInRopeMode[l]++;
			}
		}
		if (!enteringShortCut.HasValue)
		{
			base.bodyChunks[1].vel += Custom.RNV() * flying * UnityEngine.Random.value * 0.2f;
			float num3 = 200f;
			bool flag = false;
			Vector2 vector = new Vector2(0f, 0f - base.gravity) * Mathf.InverseLerp(60f, 0f, noShortcuts);
			if (!small && (this as BigNeedleWorm).chargingAttack > 0.5f)
			{
				vector = Vector2.Lerp(vector, Custom.DirVec((AI as BigNeedleWormAI).attackFromPos, (AI as BigNeedleWormAI).attackTargetPos) * -6f, Mathf.InverseLerp(0.5f, 0.8f, (this as BigNeedleWorm).chargingAttack));
			}
			for (int m = 0; m < tail.GetLength(0); m++)
			{
				tail[m, 1] = tail[m, 0];
				tail[m, 0] += tail[m, 2];
				if (!Custom.DistLess(base.bodyChunks[base.bodyChunks.Length - 1].pos, tail[m, 0], num3) && !room.VisualContact(tail[m, 0], GetSegmentPos(base.bodyChunks.Length + m - 1)))
				{
					segmentsInRopeMode[base.bodyChunks.Length + m] = 0;
				}
				if (!flag && segmentsInRopeMode[base.bodyChunks.Length + m] >= 10)
				{
					if (room.PointSubmerged(tail[m, 0]))
					{
						tail[m, 2] *= 0.8f;
						tail[m, 2] += new Vector2(0f, 0.3f);
					}
					else
					{
						tail[m, 2] *= base.airFriction;
						tail[m, 2] += vector;
					}
				}
				else
				{
					tail[m, 2] *= 0.5f;
					AddSegmentVel(base.bodyChunks.Length + m - 1, Custom.DirVec(tail[m, 0], GetSegmentPos(base.bodyChunks.Length + m - 1)) * 2f);
					tail[m, 2] += Custom.DirVec(tail[m, 0], GetSegmentPos(base.bodyChunks.Length + m - 1)) * 2f;
					tail[m, 0] += Custom.DirVec(tail[m, 0], GetSegmentPos(base.bodyChunks.Length + m - 1)) * 2f;
					if (!flag && !room.VisualContact(tail[m, 0], GetSegmentPos(base.bodyChunks.Length + m - 1)))
					{
						flag = true;
					}
				}
				if (!flag && segmentsInRopeMode[base.bodyChunks.Length + m] >= 10)
				{
					SharedPhysics.TerrainCollisionData cd = scratchTerrainCollisionData.Set(tail[m, 0], tail[m, 1], tail[m, 2], GetSegmentRadForCollision(base.bodyChunks.Length + m), new IntVector2(0, 0), goThroughFloors: true);
					cd = SharedPhysics.VerticalCollision(room, cd);
					cd = SharedPhysics.HorizontalCollision(room, cd);
					tail[m, 0] = cd.pos;
					tail[m, 2] = cd.vel;
					if (cd.contactPoint.x != 0 || cd.contactPoint.y != 0)
					{
						tail[m, 2] *= 0.5f;
						if (base.Consious && m > 0)
						{
							tail[m, 2] += Custom.DirVec(tail[m, 0], tail[m - 1, 0]) * extraMovementForce;
						}
						num++;
					}
				}
				num3 += GetSegmentRadForRopeLength(base.bodyChunks.Length + m) * 5f;
			}
			for (int n = 0; n < connectionRopes.Length; n++)
			{
				float num4 = GetSegmentRadForRopeLength(n) + GetSegmentRadForRopeLength(n + 1);
				float num5 = GetSegmentMass(n + 1) / (GetSegmentMass(n) + GetSegmentMass(n + 1));
				if ((connectionRopes[n].bends.Count > 2 || !Custom.DistLess(GetSegmentPos(n), GetSegmentPos(n + 1), num4 * 4f)) && !room.VisualContact(GetSegmentPos(n), GetSegmentPos(n + 1)))
				{
					segmentsInRopeMode[n] = 0;
				}
				if (segmentsInRopeMode[n] >= 10)
				{
					connectionRopes[n].Update(GetSegmentPos(n), GetSegmentPos(n + 1));
					float totalLength = connectionRopes[n].totalLength;
					if (totalLength > num4)
					{
						Vector2 vector2 = Custom.DirVec(GetSegmentPos(n), connectionRopes[n].AConnect);
						AddSegmentVel(n, vector2 * (totalLength - num4) * num5);
						AddSegmentPos(n, vector2 * (totalLength - num4) * num5);
						vector2 = Custom.DirVec(GetSegmentPos(n + 1), connectionRopes[n].BConnect);
						AddSegmentVel(n + 1, vector2 * (totalLength - num4) * (1f - num5));
						AddSegmentPos(n + 1, vector2 * (totalLength - num4) * (1f - num5));
					}
				}
				else
				{
					connectionRopes[n].Reset();
					connectionRopes[n].Update(GetSegmentPos(n), GetSegmentPos(n + 1));
					float num6 = Vector2.Distance(GetSegmentPos(n), GetSegmentPos(n + 1));
					if (num6 > num4)
					{
						Vector2 vector3 = Custom.DirVec(GetSegmentPos(n), GetSegmentPos(n + 1));
						AddSegmentVel(n, vector3 * (num6 - num4) * num5);
						AddSegmentPos(n, vector3 * (num6 - num4) * num5);
						AddSegmentVel(n + 1, -vector3 * (num6 - num4) * (1f - num5));
						AddSegmentPos(n + 1, -vector3 * (num6 - num4) * (1f - num5));
					}
				}
			}
			if (reallyStuckAtSamePos < 1f)
			{
				for (int num7 = 2; num7 < 5; num7++)
				{
					for (int num8 = 0; num8 < TotalSegments - num7; num8++)
					{
						float num9 = Mathf.InverseLerp(0f, TotalSegments - num7, num8);
						num9 = Mathf.Lerp(3f, 1.5f, Mathf.Sin(num9 * (float)Math.PI)) / (0.5f + (float)num7 * 0.25f);
						num9 *= 1f - reallyStuckAtSamePos;
						float num10 = GetSegmentMass(num8 + num7) / (GetSegmentMass(num8) + GetSegmentMass(num8 + num7));
						AddSegmentVel(num8, Custom.DirVec(GetSegmentPos(num8 + num7), GetSegmentPos(num8)) * num9 * num10);
						AddSegmentVel(num8 + num7, Custom.DirVec(GetSegmentPos(num8), GetSegmentPos(num8 + num7)) * num9 * (1f - num10));
					}
				}
			}
			segmentsStuckOnTerrain = Custom.LerpAndTick(segmentsStuckOnTerrain, (float)num / (float)TotalSegments, 0.07f, 0.025f);
		}
		else
		{
			for (int num11 = 0; num11 < tail.GetLength(0); num11++)
			{
				tail[num11, 1] = tail[num11, 0];
				tail[num11, 0] = Vector2.Lerp(tail[num11, 0], room.MiddleOfTile(enteringShortCut.Value), 0.4f);
			}
		}
	}

	public virtual void AfterUpdate()
	{
		flying = Custom.LerpAndTick(flying, flyingThisFrame ? 1f : 0f, 0.11f, 1f / 30f);
		if (!base.Consious || atDestThisFrame)
		{
			stuckAtSamePos = 0f;
			brokenLineOfSight = 0f;
			segmentsStuckOnTerrain = 0f;
			reallyStuckAtSamePos = 0f;
		}
	}

	public virtual void Act()
	{
		AI.Update();
		flyingThisFrame = true;
		extraMovementForce = Custom.LerpAndTick(extraMovementForce, 0.25f * brokenLineOfSight + 0.25f * segmentsStuckOnTerrain + 0.5f * stuckAtSamePos, 0.07f, 0.025f);
		for (int i = 0; i < TotalSegments; i++)
		{
			float value = Mathf.InverseLerp(0f, TotalSegments - 1, i);
			if (room.aimap.getAItile(GetSegmentPos(i)).narrowSpace)
			{
				AddSegmentVelY(i, base.gravity);
			}
			else
			{
				AddSegmentVelY(i, Mathf.Sin(Mathf.InverseLerp(0f, 0.5f, value) * (float)Math.PI) * 1.6f * flying);
			}
		}
		lastPosCounter++;
		if (lastPosCounter > 5)
		{
			lastPosCounter = 0;
			for (int num = prevPositions.Length - 1; num > 0; num--)
			{
				prevPositions[num] = prevPositions[num - 1];
			}
			prevPositions[0] = base.mainBodyChunk.pos;
		}
		stuckAtSamePos = Custom.LerpAndTick(stuckAtSamePos, Mathf.InverseLerp(100f, 40f, Vector2.Distance(base.mainBodyChunk.pos, prevPositions[prevPositions.Length - 1])) * SlowFlySpeed * 2f, 0.07f, 0.025f);
		if (stuckAtSamePos > 0.99f)
		{
			reallyStuckAtSamePos = Mathf.Min(1f, reallyStuckAtSamePos + 1f / 90f);
		}
		else if (stuckAtSamePos < 0.5f)
		{
			reallyStuckAtSamePos = 0f;
		}
		else
		{
			reallyStuckAtSamePos = Mathf.Max(0f, reallyStuckAtSamePos - 1f / 90f);
		}
		if (AI.stuckTracker != null)
		{
			reallyStuckAtSamePos = Mathf.Max(reallyStuckAtSamePos, AI.stuckTracker.Utility());
		}
		if (!base.safariControlled && reallyStuckAtSamePos > 0.5f)
		{
			for (int j = 0; j < segmentsInRopeMode.Length; j++)
			{
				segmentsInRopeMode[j] = 0;
			}
			extraMovementForce = 1f;
			for (int k = 0; k < TotalSegments; k++)
			{
				if (UnityEngine.Random.value < 0.2f)
				{
					AddSegmentVel(k, Custom.RNV() * UnityEngine.Random.value * 11f * Mathf.InverseLerp(0.5f, 1f, reallyStuckAtSamePos));
				}
			}
		}
		MovementConnection movementConnection = (AI.pathFinder as StandardPather).FollowPath(room.GetWorldCoordinate(base.mainBodyChunk.pos), actuallyFollowingThisPath: true);
		if (base.abstractCreature.controlled && (movementConnection == default(MovementConnection) || !AllowableControlledAIOverride(movementConnection.type)))
		{
			movementConnection = default(MovementConnection);
			stuckAtSamePos = 0f;
			reallyStuckAtSamePos = 0f;
			if (inputWithDiagonals.HasValue && room != null)
			{
				MovementConnection.MovementType type = MovementConnection.MovementType.Standard;
				if (room.GetTile(base.mainBodyChunk.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
				{
					type = MovementConnection.MovementType.ShortCut;
				}
				if ((inputWithDiagonals.Value.x != 0 || inputWithDiagonals.Value.y != 0) && !inputWithDiagonals.Value.pckp)
				{
					movementConnection = new MovementConnection(type, room.GetWorldCoordinate(base.mainBodyChunk.pos), room.GetWorldCoordinate(base.mainBodyChunk.pos + new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y) * 80f), 2);
				}
				if (inputWithDiagonals.Value.y < 0)
				{
					base.GoThroughFloors = true;
				}
				else
				{
					base.GoThroughFloors = false;
				}
			}
		}
		if (movementConnection == default(MovementConnection) && base.abstractCreature.controlled)
		{
			for (int l = 0; l < TotalSegments; l++)
			{
				Vector2 segmentVel = GetSegmentVel(l);
				SetSegmentVel(l, new Vector2(Mathf.Lerp(segmentVel.x, 0f, 0.05f), Mathf.Lerp(segmentVel.y, 0f, 0.05f)));
			}
		}
		if (movementConnection == default(MovementConnection) && !base.safariControlled)
		{
			base.mainBodyChunk.vel *= 0.6f;
			if (base.abstractCreature.abstractAI.destination.room != base.abstractCreature.pos.room)
			{
				int num2 = -1;
				for (int m = 0; m < room.abstractRoom.exits; m++)
				{
					if (room.abstractRoom.connections[m] == base.abstractCreature.abstractAI.destination.room)
					{
						num2 = m;
						break;
					}
				}
				if (num2 > -1)
				{
					num2 = room.abstractRoom.CommonToCreatureSpecificNodeIndex(num2, base.Template);
					int num3 = int.MaxValue;
					MovementConnection movementConnection2 = default(MovementConnection);
					for (int n = 0; n < room.aimap.getAItile(base.mainBodyChunk.pos).outgoingPaths.Count; n++)
					{
						if (room.aimap.IsConnectionAllowedForCreature(room.aimap.getAItile(base.mainBodyChunk.pos).outgoingPaths[n], base.Template))
						{
							int num4 = room.aimap.ExitDistanceForCreature(room.aimap.getAItile(base.mainBodyChunk.pos).outgoingPaths[n].DestTile, num2, base.Template);
							if (num4 < num3)
							{
								num3 = num4;
								movementConnection2 = room.aimap.getAItile(base.mainBodyChunk.pos).outgoingPaths[n];
							}
						}
					}
					if (num3 < 3)
					{
						enteringShortCut = room.ShortcutLeadingToNode(num2).StartTile;
					}
					else
					{
						movementConnection = movementConnection2;
					}
				}
			}
			if (movementConnection == default(MovementConnection))
			{
				if (flying > 0.5f)
				{
					flyingThisFrame = true;
					Fly(default(MovementConnection));
				}
				return;
			}
		}
		if (movementConnection == default(MovementConnection))
		{
			return;
		}
		if (OffscreenSuperSpeed && movementConnection.destinationCoord.TileDefined)
		{
			Vector2 vector = Custom.DirVec(room.MiddleOfTile(movementConnection.StartTile), room.MiddleOfTile(movementConnection.DestTile));
			for (int num5 = 0; num5 < base.bodyChunks.Length; num5++)
			{
				base.bodyChunks[num5].HardSetPosition(room.MiddleOfTile(movementConnection.DestTile) + vector * (3 - num5));
				base.bodyChunks[num5].vel *= 0f;
			}
			flyingThisFrame = !room.aimap.getAItile(movementConnection.destinationCoord).narrowSpace;
			return;
		}
		if (reallyStuckAtSamePos > 0.5f)
		{
			MovementConnection movementConnection3 = (AI.pathFinder as StandardPather).FollowPath(movementConnection.destinationCoord, actuallyFollowingThisPath: false);
			if (movementConnection3.type == MovementConnection.MovementType.ShortCut || movementConnection3.type == MovementConnection.MovementType.NPCTransportation)
			{
				movementConnection = movementConnection3;
			}
		}
		if (movementConnection.type == MovementConnection.MovementType.ShortCut || movementConnection.type == MovementConnection.MovementType.NPCTransportation)
		{
			if (noShortcuts >= 1 || !movementConnection.startCoord.TileDefined)
			{
				return;
			}
			enteringShortCut = movementConnection.StartTile;
			if (base.abstractCreature.controlled)
			{
				bool flag = false;
				List<IntVector2> list = new List<IntVector2>();
				ShortcutData[] shortcuts = room.shortcuts;
				for (int num6 = 0; num6 < shortcuts.Length; num6++)
				{
					ShortcutData shortcutData = shortcuts[num6];
					if (shortcutData.shortCutType == ShortcutData.Type.NPCTransportation && shortcutData.StartTile != movementConnection.StartTile)
					{
						list.Add(shortcutData.StartTile);
					}
					if (shortcutData.shortCutType == ShortcutData.Type.NPCTransportation && shortcutData.StartTile == movementConnection.StartTile)
					{
						flag = true;
					}
				}
				if (flag)
				{
					if (list.Count > 0)
					{
						list.Shuffle();
						NPCTransportationDestination = room.GetWorldCoordinate(list[0]);
					}
					else
					{
						NPCTransportationDestination = movementConnection.destinationCoord;
					}
				}
			}
			else if (movementConnection.type == MovementConnection.MovementType.NPCTransportation)
			{
				NPCTransportationDestination = movementConnection.destinationCoord;
			}
			flyingThisFrame = false;
		}
		else if (room.aimap.getAItile(movementConnection.StartTile).narrowSpace || (room.aimap.getAItile(movementConnection.DestTile).narrowSpace && !base.safariControlled))
		{
			Crawl(movementConnection);
			flyingThisFrame = false;
		}
		else
		{
			Fly(movementConnection);
		}
	}

	public Vector2 MoveUpFromFloor(IntVector2 tile)
	{
		int num = AI.MinFlyHeight(tile);
		if (room.aimap.getAItile(tile).smoothedFloorAltitude > num)
		{
			return room.MiddleOfTile(tile);
		}
		int num2 = num - room.aimap.getAItile(tile).smoothedFloorAltitude;
		for (int i = 0; i < num2; i++)
		{
			if (room.aimap.getTerrainProximity(tile + new IntVector2(0, 1)) <= 1)
			{
				break;
			}
			if (tile.y >= room.TileHeight - 3)
			{
				break;
			}
			tile += new IntVector2(0, 1);
		}
		return room.MiddleOfTile(tile);
	}

	public virtual void Fly(MovementConnection followingConnection)
	{
		base.GoThroughFloors = true;
		bool flag = false;
		Vector2 vector;
		if (base.safariControlled)
		{
			if (followingConnection == default(MovementConnection))
			{
				return;
			}
			vector = ((!inputWithDiagonals.HasValue || !inputWithDiagonals.Value.AnyDirectionalInput || inputWithDiagonals.Value.pckp) ? room.MiddleOfTile(followingConnection.DestTile) : (base.mainBodyChunk.pos + new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y) * 200f));
		}
		else if (AI.pathFinder.GetDestination.room == base.abstractCreature.pos.room && (followingConnection == default(MovementConnection) || (Custom.DistLess(base.abstractCreature.pos, AI.pathFinder.GetDestination, 20f) && room.VisualContact(base.abstractCreature.pos, AI.pathFinder.GetDestination))))
		{
			vector = room.MiddleOfTile(AI.pathFinder.GetDestination);
			flag = followingConnection == default(MovementConnection);
		}
		else
		{
			if (!(followingConnection != default(MovementConnection)))
			{
				return;
			}
			vector = room.MiddleOfTile(followingConnection.DestTile);
			MovementConnection movementConnection = followingConnection;
			for (int i = 0; i < 10; i++)
			{
				movementConnection = (AI.pathFinder as StandardPather).FollowPath(movementConnection.destinationCoord, actuallyFollowingThisPath: false);
				if (movementConnection.destinationCoord.TileDefined && room.VisualContact(base.mainBodyChunk.pos, room.MiddleOfTile(movementConnection.destinationCoord)))
				{
					vector = room.MiddleOfTile(movementConnection.DestTile);
					if (movementConnection.destinationCoord == AI.pathFinder.GetDestination)
					{
						break;
					}
					continue;
				}
				flag = true;
				break;
			}
		}
		if (!base.safariControlled && !room.GetWorldCoordinate(vector).CompareDisregardingNode(AI.pathFinder.GetDestination))
		{
			Vector2 vector2 = MoveUpFromFloor(room.GetTilePosition(vector));
			if (room.VisualContact(base.mainBodyChunk.pos, vector2))
			{
				vector = vector2;
			}
		}
		if (stuckAtSamePos > 0.25f && !room.GetTile(base.abstractCreature.pos + new IntVector2(0, 2)).Solid && !AI.TileInEnclosedArea(base.abstractCreature.pos.Tile))
		{
			vector += Custom.DegToVec(Mathf.Lerp(-15f, 15f, UnityEngine.Random.value)) * 80f * stuckAtSamePos * (1f - reallyStuckAtSamePos);
		}
		if (!small && (AI as BigNeedleWormAI).behavior == NeedleWormAI.Behavior.Attack)
		{
			lookDir = Custom.MoveTowards(lookDir, Custom.DirVec(Vector2.Lerp(base.bodyChunks[1].pos, (AI as BigNeedleWormAI).attackFromPos, Mathf.InverseLerp(500f, 200f, Vector2.Distance(base.bodyChunks[1].pos, (AI as BigNeedleWormAI).attackFromPos))), (AI as BigNeedleWormAI).attackTargetPos), 0.34f);
		}
		else
		{
			if (creatureLooker != null)
			{
				creatureLooker.Update();
				if (creatureLooker.lookCreature != null && creatureLooker.lookCreature.TicksSinceSeen < 40)
				{
					if (creatureLooker.lookCreature.VisualContact && creatureLooker.lookCreature.representedCreature.realizedCreature != null)
					{
						getToLookDir = Vector2.Lerp(getToLookDir, Custom.DirVec(base.bodyChunks[1].pos, creatureLooker.lookCreature.representedCreature.realizedCreature.DangerPos), Mathf.InverseLerp(800f, 300f, Vector2.Distance(base.bodyChunks[1].pos, creatureLooker.lookCreature.representedCreature.realizedCreature.DangerPos)) * Mathf.InverseLerp(40f, 20f, creatureLooker.lookCreature.TicksSinceSeen));
					}
					else if (creatureLooker.lookCreature.BestGuessForPosition().room == room.abstractRoom.index)
					{
						getToLookDir = Vector2.Lerp(getToLookDir, Custom.DirVec(base.bodyChunks[1].pos, room.MiddleOfTile(creatureLooker.lookCreature.BestGuessForPosition())), Mathf.InverseLerp(800f, 300f, Vector2.Distance(base.bodyChunks[1].pos, room.MiddleOfTile(creatureLooker.lookCreature.BestGuessForPosition()))) * Mathf.InverseLerp(40f, 20f, creatureLooker.lookCreature.TicksSinceSeen));
					}
				}
			}
			if (!Custom.DistLess(base.mainBodyChunk.pos, vector, 90f * (1f - extraMovementForce)))
			{
				getToLookDir = Custom.MoveTowards(getToLookDir, Custom.DirVec(base.mainBodyChunk.pos, vector), 0.05f);
			}
			else if (UnityEngine.Random.value < 1f / 30f)
			{
				getToLookDir = (lookDir + Custom.RNV() * UnityEngine.Random.value).normalized;
			}
			getToLookDir += Custom.RNV() * 0.07f;
			lookDir = Custom.MoveTowards(lookDir, getToLookDir, 0.14f);
		}
		float num = 0.6f * (1f - extraMovementForce) * (small ? 0.5f : 1f);
		Vector2 vector3 = Vector3.Slerp(lookDir, new Vector2(0f, -1f), 0.25f * (1f - extraMovementForce) * ((!small && (AI as BigNeedleWormAI).behavior == NeedleWormAI.Behavior.Attack) ? 0f : 1f));
		base.bodyChunks[0].vel += vector3 * (small ? 0.8f : 1.2f) * num / base.bodyChunks[0].mass;
		base.bodyChunks[2].vel -= vector3 * (small ? 0.8f : 1.2f) * 0.95f * num / base.bodyChunks[2].mass;
		brokenLineOfSight = Custom.LerpAndTick(brokenLineOfSight, flag ? 1f : 0f, 0.09f, 0.025f);
		if (!flag && Vector2.Distance(base.bodyChunks[1].pos, room.MiddleOfTile(AI.pathFinder.GetDestination.Tile)) < 60f)
		{
			base.bodyChunks[1].vel = Vector2.Lerp(base.bodyChunks[1].vel, Vector2.ClampMagnitude(vector - base.bodyChunks[1].pos, 40f) / 4f, 0.2f);
			base.bodyChunks[0].vel *= 0.9f;
			base.bodyChunks[2].vel *= 0.9f;
			atDestThisFrame = true;
		}
		else
		{
			float num2 = Mathf.Lerp(Mathf.InverseLerp(30f, 200f, Vector2.Distance(base.bodyChunks[1].pos, vector)), 1f, extraMovementForce) * Mathf.Lerp(SlowFlySpeed, 1f, extraMovementForce);
			Vector2 vector4 = new Vector2(0f, 0f);
			float num3 = 0f;
			for (int j = 0; j < base.bodyChunks.Length; j++)
			{
				vector4 += base.bodyChunks[j].vel * base.bodyChunks[j].mass;
				num3 += base.bodyChunks[j].mass;
			}
			vector4 /= num3;
			vector4 = Vector2.ClampMagnitude(vector4 * 15f, 200f);
			if (room.aimap.getTerrainProximity(base.mainBodyChunk.pos) > 1 && !room.VisualContact(base.mainBodyChunk.pos, base.mainBodyChunk.pos + vector4))
			{
				num2 = Mathf.Lerp(num2, 1f, 1f - stuckAtSamePos);
				extraMovementForce = Mathf.Lerp(extraMovementForce, 1f, 1f - stuckAtSamePos);
				vector = Vector2.Lerp(vector, base.mainBodyChunk.pos - vector4, 1f - stuckAtSamePos);
			}
			int terrainProximity = room.aimap.getTerrainProximity(base.mainBodyChunk.pos);
			if (terrainProximity > 1 && terrainProximity < 5 && room.aimap.getTerrainProximity(base.mainBodyChunk.pos + Custom.DirVec(base.mainBodyChunk.pos, vector) * 60f) < terrainProximity)
			{
				IntVector2 pos = room.GetTilePosition(base.mainBodyChunk.pos);
				float num4 = float.MinValue;
				for (int k = 0; k < 8; k++)
				{
					if (room.GetTile(room.GetTilePosition(base.mainBodyChunk.pos) + Custom.eightDirections[k]).Solid)
					{
						continue;
					}
					float num5 = Vector2.Dot((base.mainBodyChunk.pos - vector).normalized, (base.mainBodyChunk.pos - room.MiddleOfTile(room.GetTilePosition(base.mainBodyChunk.pos) + Custom.eightDirections[k])).normalized);
					if (num5 > 0f)
					{
						num5 += (float)room.aimap.getTerrainProximity(room.GetTilePosition(base.mainBodyChunk.pos) + Custom.eightDirections[k]);
						if (num5 > num4)
						{
							num4 = num5;
							pos = room.GetTilePosition(base.mainBodyChunk.pos) + Custom.eightDirections[k];
						}
					}
				}
				if (num4 > 0f)
				{
					num2 = Mathf.Lerp(num2, 1f, 1f - stuckAtSamePos);
					vector = Vector2.Lerp(vector, room.MiddleOfTile(pos), 1f - stuckAtSamePos);
				}
			}
			if (followingConnection != default(MovementConnection))
			{
				base.mainBodyChunk.vel += Custom.DirVec(base.mainBodyChunk.pos, room.MiddleOfTile(followingConnection.DestTile)) * num2 * 5f * extraMovementForce * (small ? 0.7f : 1f);
			}
			else
			{
				base.mainBodyChunk.vel += Custom.DirVec(base.mainBodyChunk.pos, vector) * num2 * 5f * extraMovementForce * (small ? 0.7f : 1f);
			}
			base.GoThroughFloors = stuckAtSamePos > 0.5f || Custom.DirVec(base.bodyChunks[1].pos, vector).y < 0f;
			for (int l = 0; l < base.bodyChunks.Length; l++)
			{
				float num6 = Mathf.InverseLerp(0f, base.bodyChunks.Length - 1, l);
				base.bodyChunks[l].vel *= Mathf.Lerp(0.8f, 1f, Mathf.Pow(num6, 0.3f));
				base.bodyChunks[l].vel += Custom.DirVec(base.bodyChunks[l].pos, vector) * Mathf.Lerp(3f + segmentsStuckOnTerrain, 0.5f, num6) * num2 * (small ? 0.7f : 1f);
			}
			for (int m = 1; m < TotalSegments / 2; m++)
			{
				float f = Mathf.InverseLerp(0f, TotalSegments / 2 - 1, m);
				Vector2 vector5 = Custom.DirVec(GetSegmentPos(m), GetSegmentPos(m - 1));
				Vector2 rhs = Custom.DirVec(GetSegmentPos(m), vector);
				AddSegmentVel(m, vector5 * Mathf.InverseLerp(-1f, 1f, Vector2.Dot(vector5, rhs)) * Mathf.Lerp(1.3f, 0f, Mathf.Pow(f, 0.5f)) * num2 * (small ? 0.7f : 1f));
			}
		}
		if (!base.safariControlled && UnityEngine.Random.value < 1f / 30f)
		{
			if (base.bodyChunks[1].pos.x < 0f)
			{
				base.bodyChunks[1].vel.x += UnityEngine.Random.value * 8f;
			}
			else if (base.bodyChunks[1].pos.x > room.PixelWidth)
			{
				base.bodyChunks[1].vel.x -= UnityEngine.Random.value * 8f;
			}
			if (base.bodyChunks[1].pos.y < 0f)
			{
				base.bodyChunks[1].vel.y += UnityEngine.Random.value * 8f;
			}
			else if (base.bodyChunks[1].pos.y > room.PixelHeight)
			{
				base.bodyChunks[1].vel.y -= UnityEngine.Random.value * 8f;
			}
		}
	}

	public void Crawl(MovementConnection followingConnection)
	{
		base.GoThroughFloors = true;
		if (followingConnection.startCoord == AI.pathFinder.GetDestination)
		{
			atDestThisFrame = true;
			return;
		}
		float num = Mathf.Lerp(3f, 7f, stuckAtSamePos);
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			float t = Mathf.InverseLerp(0f, base.bodyChunks.Length - 1, i);
			base.bodyChunks[i].vel *= 0.6f;
			base.bodyChunks[i].vel += Custom.DirVec(base.bodyChunks[i].pos, room.MiddleOfTile(followingConnection.DestTile)) * Mathf.Lerp(num, num * -0.3f, t) * (small ? 0.6f : 1f);
			base.bodyChunks[i].pos += Custom.DirVec(base.bodyChunks[i].pos, room.MiddleOfTile(followingConnection.DestTile)) * Mathf.Lerp(num / 7f, num / 70f, t) * (small ? 0.6f : 1f);
		}
		crawlSin += 0.2f;
		SinMovementInBody(1.5f * (small ? 0.6f : 1f), 1.5f * (small ? 0.6f : 1f), 1.6f, small ? 1.2f : 0.2f, addToPositions: true);
	}

	public void SinMovementInBody(float amp1, float amp2, float wave1, float wave2)
	{
		for (int i = 1; i < TotalSegments; i++)
		{
			AddSegmentVel(i, Custom.PerpendicularVector(GetSegmentPos(i), GetSegmentPos(i - 1)) * Mathf.Sin(crawlSin + (float)i * Custom.LerpMap(i, 1f, TotalSegments - 1, wave1, wave2)) * Custom.LerpMap(i, 1f, TotalSegments - 1, amp1, amp2));
		}
	}

	public void SinMovementInBody(float amp1, float amp2, float wave1, float wave2, bool addToPositions)
	{
		if (!addToPositions)
		{
			SinMovementInBody(amp1, amp2, wave1, wave2);
			return;
		}
		Vector2[] array = new Vector2[TotalSegments];
		for (int i = 1; i < TotalSegments; i++)
		{
			array[i] = Custom.PerpendicularVector(GetSegmentPos(i), GetSegmentPos(i - 1)) * Mathf.Sin(crawlSin + (float)i * Custom.LerpMap(i, 1f, TotalSegments - 1, wave1, wave2)) * Custom.LerpMap(i, 1f, TotalSegments - 1, amp1, amp2);
		}
		for (int j = 1; j < TotalSegments; j++)
		{
			AddSegmentVel(j, array[j]);
			AddSegmentPos(j, array[j]);
		}
	}

	public float GetSegmentMass(int seg)
	{
		if (seg < base.bodyChunks.Length)
		{
			return base.bodyChunks[seg].mass;
		}
		return Custom.LerpMap(seg, base.bodyChunks.Length, TotalSegments - 1, base.bodyChunks[base.bodyChunks.Length - 1].mass * 0.9f, base.bodyChunks[base.bodyChunks.Length - 1].mass * 0.01f, 0.3f);
	}

	public float GetSegmentRadForCollision(int seg)
	{
		if (seg < base.bodyChunks.Length)
		{
			return base.bodyChunks[seg].rad;
		}
		return 1f;
	}

	public float GetSegmentRadForRopeLength(int seg)
	{
		if (seg < base.bodyChunks.Length)
		{
			return base.bodyChunks[seg].rad * (small ? 0.8f : 1f);
		}
		return Custom.LerpMap(seg, base.bodyChunks.Length, TotalSegments - 1, base.bodyChunks[base.bodyChunks.Length - 1].rad, small ? 8f : 11f);
	}

	public Vector2 GetSegmentPos(int seg)
	{
		if (seg < base.bodyChunks.Length)
		{
			return base.bodyChunks[seg].pos;
		}
		return tail[seg - base.bodyChunks.Length, 0];
	}

	public Vector2 GetSegmentPos(int seg, float timeStacker)
	{
		if (seg < base.bodyChunks.Length)
		{
			return Vector2.Lerp(base.bodyChunks[seg].lastPos, base.bodyChunks[seg].pos, timeStacker);
		}
		return Vector2.Lerp(tail[seg - base.bodyChunks.Length, 1], tail[seg - base.bodyChunks.Length, 0], timeStacker);
	}

	public void SetSegmentPos(int seg, Vector2 to)
	{
		if (seg < base.bodyChunks.Length)
		{
			base.bodyChunks[seg].pos = to;
		}
		else
		{
			tail[seg - base.bodyChunks.Length, 0] = to;
		}
	}

	public void AddSegmentPos(int seg, Vector2 add)
	{
		if (seg < base.bodyChunks.Length)
		{
			base.bodyChunks[seg].pos += add;
		}
		else
		{
			tail[seg - base.bodyChunks.Length, 0] += add;
		}
	}

	public Vector2 GetSegmentVel(int seg)
	{
		if (seg < base.bodyChunks.Length)
		{
			return base.bodyChunks[seg].vel;
		}
		return tail[seg - base.bodyChunks.Length, 2];
	}

	public void SetSegmentVel(int seg, Vector2 to)
	{
		if (seg < base.bodyChunks.Length)
		{
			base.bodyChunks[seg].vel = to;
		}
		else
		{
			tail[seg - base.bodyChunks.Length, 2] = to;
		}
	}

	public void AddSegmentVel(int seg, Vector2 add)
	{
		if (seg < base.bodyChunks.Length)
		{
			base.bodyChunks[seg].vel += add;
		}
		else
		{
			tail[seg - base.bodyChunks.Length, 2] += add;
		}
	}

	public void AddSegmentVelY(int seg, float add)
	{
		if (seg < base.bodyChunks.Length)
		{
			base.bodyChunks[seg].vel.y += add;
		}
		else
		{
			tail[seg - base.bodyChunks.Length, 2].y += add;
		}
	}

	public Vector2 GetSegmentDir(int seg, float timeStacker)
	{
		if (seg == 0)
		{
			return Custom.DirVec(GetSegmentPos(1, timeStacker), GetSegmentPos(0, timeStacker));
		}
		return Custom.DirVec(GetSegmentPos(seg, timeStacker), GetSegmentPos(seg - 1, timeStacker));
	}

	public Vector2 OnBodyPos(float f, float timeStacker)
	{
		f *= (float)TotalSegments - 1.1f;
		int num = Mathf.FloorToInt(f);
		int num2 = Custom.IntClamp(num + 1, 0, TotalSegments - 1);
		return Vector2.Lerp(GetSegmentPos(num, timeStacker), GetSegmentPos(num2, timeStacker), Mathf.InverseLerp(num, num2, f));
	}

	public Vector2 OnBodyDir(float f, float timeStacker)
	{
		f *= (float)TotalSegments - 1.1f;
		int num = Mathf.FloorToInt(f);
		int num2 = Custom.IntClamp(num + 1, 0, TotalSegments - 1);
		return Vector3.Slerp(GetSegmentDir(num, timeStacker), GetSegmentDir(num2, timeStacker), Mathf.InverseLerp(num, num2, f));
	}

	public float OnBodyRad(float f)
	{
		f *= (float)TotalSegments - 1.1f;
		int num = Mathf.FloorToInt(f);
		int num2 = Custom.IntClamp(num + 1, 0, TotalSegments - 1);
		return Mathf.Lerp(GetSegmentRadForCollision(num), GetSegmentRadForCollision(num2), Mathf.InverseLerp(num, num2, f));
	}

	public float CreatureInterestBonus(Tracker.CreatureRepresentation crit, float score)
	{
		if (crit.representedCreature.creatureTemplate.type == CreatureTemplate.Type.SmallNeedleWorm)
		{
			return 0f;
		}
		if (small && crit.representedCreature.creatureTemplate.type == CreatureTemplate.Type.BigNeedleWorm)
		{
			return score / 10f;
		}
		return score * ((crit == AI.focusCreature) ? 2f : 1f);
	}

	public Tracker.CreatureRepresentation ForcedLookCreature()
	{
		if (this is BigNeedleWorm && (this as BigNeedleWorm).BigAI.keepCloseToCreature != null)
		{
			return AI.tracker.RepresentationForCreature((this as BigNeedleWorm).BigAI.keepCloseToCreature.abstractCreature, addIfMissing: false);
		}
		return null;
	}

	public void LookAtNothing()
	{
	}
}
