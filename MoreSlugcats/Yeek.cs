using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class Yeek : AirBreatherCreature, Weapon.INotifyOfFlyingWeapons
{
	public class YeekTail
	{
		public Vector2[,] parts;

		private Yeek owner;

		public float maxDist;

		public int tailLength;

		public SharedPhysics.TerrainCollisionData scratchTerrainCollisionData;

		public int TailLength => tailLength;

		public YeekTail(int length, Yeek owner)
		{
			this.owner = owner;
			tailLength = length;
			parts = new Vector2[tailLength, 4];
			maxDist = 5f;
		}

		public void Update()
		{
			if (owner.enteringShortCut.HasValue)
			{
				for (int i = 0; i < TailLength; i++)
				{
					parts[i, 1] = parts[i, 0];
					parts[i, 0] = Vector2.Lerp(parts[i, 0], owner.room.MiddleOfTile(owner.enteringShortCut.Value), 0.4f);
				}
				return;
			}
			if (!owner.dead)
			{
				for (int j = 0; j < TailLength; j++)
				{
					parts[j, 2] += new Vector2(0f, owner.room.gravity * 0.5f);
				}
			}
			if (Random.value < 0.4f)
			{
				Vector2 vector = Custom.RNV() * 2f;
				for (int k = 0; k < TailLength; k++)
				{
					if (k > 3)
					{
						parts[k, 2] += vector;
					}
				}
			}
			for (int l = 0; l < TailLength; l++)
			{
				float t = (float)l / (float)(parts.GetLength(0) - 1);
				parts[l, 1] = parts[l, 0];
				parts[l, 0] += parts[l, 2];
				if (owner.room.PointSubmerged(parts[l, 0]))
				{
					parts[l, 2] *= Custom.LerpMap(parts[l, 2].magnitude, 1f, 10f, 1f, 0.5f, Mathf.Lerp(1.4f, 0.4f, t));
					parts[l, 2].y += 0.05f;
					parts[l, 2] += Custom.RNV() * 0.1f;
					continue;
				}
				parts[l, 2] *= Custom.LerpMap(Vector2.Distance(parts[l, 0], parts[l, 1]), 1f, 6f, 0.999f, 0.8f, Mathf.Lerp(1.5f, 0.5f, t));
				parts[l, 2].y -= owner.room.gravity * Custom.LerpMap(Vector2.Distance(parts[l, 0], parts[l, 1]), 1f, 6f, 0.6f, 0f);
				if (l % 3 == 2 || l == TailLength - 1)
				{
					SharedPhysics.TerrainCollisionData cd = scratchTerrainCollisionData.Set(parts[l, 0], parts[l, 1], parts[l, 2], 1f, new IntVector2(0, 0), goThroughFloors: false);
					cd = SharedPhysics.HorizontalCollision(owner.room, cd);
					cd = SharedPhysics.VerticalCollision(owner.room, cd);
					cd = SharedPhysics.SlopesVertically(owner.room, cd);
					parts[l, 0] = cd.pos;
					parts[l, 2] = cd.vel;
					if (cd.contactPoint.x != 0)
					{
						parts[l, 2].y *= 0.6f;
					}
					if (cd.contactPoint.y != 0)
					{
						parts[l, 2].x *= 0.6f;
					}
				}
			}
			for (int m = 0; m < TailLength; m++)
			{
				if (m > 0)
				{
					Vector2 normalized = (parts[m, 0] - parts[m - 1, 0]).normalized;
					float num = Vector2.Distance(parts[m, 0], parts[m - 1, 0]);
					float num2 = ((num > maxDist) ? 0.5f : 0.25f);
					parts[m, 0] += normalized * (maxDist - num) * num2;
					parts[m, 2] += normalized * (maxDist - num) * num2;
					parts[m - 1, 0] -= normalized * (maxDist - num) * num2;
					parts[m - 1, 2] -= normalized * (maxDist - num) * num2;
					if (m > 1)
					{
						normalized = (parts[m, 0] - parts[m - 2, 0]).normalized;
						parts[m, 2] += normalized * 0.6f;
						parts[m - 2, 2] -= normalized * 0.6f;
					}
					if (m < 2)
					{
						parts[m, 0] = parts[m - 1, 0] + Custom.DirVec(owner.bodyChunks[1].pos, owner.bodyChunks[0].pos) * maxDist * 0.8f;
						parts[m, 2] *= 0f;
					}
				}
				else
				{
					parts[m, 0] = owner.bodyChunks[0].pos;
					parts[m, 2] *= 0f;
				}
			}
		}

		public void Reset(Vector2 ps)
		{
			for (int i = 0; i < parts.GetLength(0); i++)
			{
				parts[i, 0] = ps + Custom.RNV() * Random.value;
				parts[i, 1] = parts[i, 0];
				parts[i, 2] *= 0f;
			}
		}

		public Vector2 GetPos(int index, float timeStacker)
		{
			return Vector2.Lerp(parts[index, 1], parts[index, 0], timeStacker);
		}

		public Vector2 GetVel(int index)
		{
			return parts[index, 2];
		}
	}

	public int timeSinceHop;

	public Vector2 bodyDirection;

	private int timeSinceJump;

	public float headLeadingCounter;

	public int maxJumpCounter;

	private bool climbingMode;

	public Vector2 climbingOrientation;

	private float interestInClimbingPoles;

	private bool tunnelCrawlingMode;

	public float yeekCallCounter;

	public int hardFacingDir;

	private float grabCooldown;

	private Vector2 lastSafeClimb;

	public YeekAI AI;

	public Vector2[,] tail;

	public Vector2[,] secondTail;

	public Vector2 lastbodyDirection;

	public YeekTail[] tails;

	public float ceilingClearance;

	public bool usingStandardMass;

	public Vector2 controlledJumpVelocity;

	public int TailSegments => tails[0].TailLength;

	public bool OnGround => CheckOnGround(base.mainBodyChunk.pos);

	public override Vector2 VisionPoint => base.bodyChunks[1].pos;

	public bool GetClimbingMode
	{
		get
		{
			if (climbingMode)
			{
				return grabbedBy.Count == 0;
			}
			return false;
		}
	}

	public bool GetTunnelMode
	{
		get
		{
			if (tunnelCrawlingMode)
			{
				return grabbedBy.Count == 0;
			}
			return false;
		}
	}

	public float GroupLeaderPotential => (base.abstractCreature.personality.energy + base.abstractCreature.personality.sympathy + base.abstractCreature.personality.bravery) * Mathf.Clamp(base.abstractCreature.personality.dominance, 0f, 1f);

	public Yeek(AbstractCreature abstractCreature, World world)
		: base(abstractCreature, world)
	{
		base.bodyChunks = new BodyChunk[2];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 7f, 0.6f);
		base.bodyChunks[1] = new BodyChunk(this, 1, new Vector2(0f, 0f), 3f, 0.05f);
		bodyChunkConnections = new BodyChunkConnection[1];
		bodyChunkConnections[0] = new BodyChunkConnection(base.bodyChunks[0], base.bodyChunks[1], 15f, BodyChunkConnection.Type.Pull, 1f, 0.01f);
		base.airFriction = 0.99f;
		base.gravity = 0.9f;
		collisionLayer = 1;
		base.waterFriction = 0.98f;
		base.buoyancy = 1.05f;
		maxJumpCounter = 20;
		climbingMode = false;
		tails = new YeekTail[2];
		tails[0] = new YeekTail(10, this);
		tails[1] = new YeekTail(10, this);
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (room == null || enteringShortCut.HasValue)
		{
			timeSinceHop = 0;
			return;
		}
		if (grabbedBy.Count > 0 && grabbedBy[0]?.grabber is Player)
		{
			SetPlayerHoldingBodyMass();
		}
		else
		{
			SetStandardBodyMass();
		}
		if (room.game.devToolsActive && Input.GetKey("b") && room.game.cameras[0].room == room)
		{
			base.bodyChunks[0].vel += Custom.DirVec(base.bodyChunks[0].pos, new Vector2(Input.mousePosition.x, Input.mousePosition.y) + room.game.cameras[0].pos) * 14f;
			Stun(12);
		}
		grabCooldown -= 0.01f;
		lastbodyDirection = bodyDirection;
		bodyDirection = Vector2.Lerp(bodyDirection, base.mainBodyChunk.vel.normalized, 0.3f);
		if (Mathf.Abs(bodyDirection.x) < 0.04f)
		{
			bodyDirection.x = 0f;
		}
		if (Mathf.Sign(bodyDirection.x) != 0f)
		{
			hardFacingDir = (int)Mathf.Sign(bodyDirection.x);
		}
		headLeadingCounter -= 1f;
		yeekCallCounter -= 0.01f;
		if (Mathf.Abs(base.mainBodyChunk.vel.x) > 1f)
		{
			headLeadingCounter = 60f;
		}
		if (Mathf.Abs(base.mainBodyChunk.pos.x - base.mainBodyChunk.lastPos.x) < 0.01f)
		{
			headLeadingCounter = 0f;
		}
		timeSinceHop++;
		timeSinceJump++;
		ceilingClearance = 10f;
		for (int i = 0; i < 10; i++)
		{
			if (room.GetTile(base.abstractCreature.pos.Tile + new IntVector2(0, i)).Solid)
			{
				ceilingClearance = i;
				break;
			}
		}
		if (controlledJumpVelocity.y > 0f)
		{
			controlledJumpVelocity.y -= base.gravity;
			if (grabbedBy.Count > 0 && grabbedBy[0].grabber is Player && !base.dead)
			{
				grabbedBy[0].grabber.firstChunk.vel.y = controlledJumpVelocity.y;
				if (room.GetTile(grabbedBy[0].grabber.abstractCreature.pos.Tile + new IntVector2(0, 1)).Solid || grabbedBy[0].grabber.firstChunk.ContactPoint.y != 0)
				{
					controlledJumpVelocity = Vector2.zero;
				}
			}
			else
			{
				controlledJumpVelocity = Vector2.zero;
			}
		}
		for (int j = 0; j < tails.GetLength(0); j++)
		{
			tails[j].Update();
		}
		if (grabbedBy.Count > 0 && grabbedBy[0].grabber is Player player && !base.dead)
		{
			base.stun = 0;
			bodyDirection = Custom.RNV();
			headLeadingCounter = 0f;
			if (base.grasps[0] != null)
			{
				ReleaseGrasp(0);
			}
			base.bodyChunks[0].vel += Custom.RNV() * 0.32f;
			base.bodyChunks[1].vel += Custom.RNV() * 0.32f;
			bodyChunkConnections[0].distance = 2f;
			tunnelCrawlingMode = false;
			if (room.aimap.getAItile(base.mainBodyChunk.pos).narrowSpace || room.GetTile(base.mainBodyChunk.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
			{
				tunnelCrawlingMode = true;
			}
			EndClimb();
			WeightedPush(1, 0, climbingOrientation, 1.2f);
			bool flag = player.input[0].jmp && (player.input[0].y > -1 || player.bodyMode == Player.BodyModeIndex.ClimbingOnBeam);
			if (!((Random.value > 0.95f && player.bodyMode != Player.BodyModeIndex.ClimbingOnBeam) || flag) || timeSinceHop <= maxJumpCounter)
			{
				return;
			}
			if ((OnGround || player.bodyMode == Player.BodyModeIndex.ClimbingOnBeam) && !tunnelCrawlingMode)
			{
				maxJumpCounter = (int)Random.Range(40f, 30f);
				yeekCallCounter = 0f;
				if (player.input[0].y > -1)
				{
					base.mainBodyChunk.vel *= 0.01f;
					Hop(base.mainBodyChunk.pos, base.mainBodyChunk.pos + new Vector2(Random.Range(-20f, 20f), Random.Range(20f, 30f)) + new Vector2(0f, flag ? 60f : 0f), forced: true);
					base.mainBodyChunk.vel *= 0.6f;
					grabbedBy[0].grabber.mainBodyChunk.vel.y = base.mainBodyChunk.vel.y;
					controlledJumpVelocity = base.mainBodyChunk.vel;
				}
			}
			YeekCall();
			return;
		}
		maxJumpCounter = (int)Mathf.Lerp(15f, 6f, AI.fearCounter);
		if (!base.Consious)
		{
			bodyChunkConnections[0].distance = 10f;
			tunnelCrawlingMode = false;
			EndClimb();
			return;
		}
		WeightedPush(1, 0, Custom.DirVec(base.bodyChunks[1].pos, base.bodyChunks[0].pos), 0.5f * EffectiveRoomGravity);
		if (GetClimbingMode)
		{
			base.bodyChunks[1].vel = base.bodyChunks[0].vel + new Vector2(climbingOrientation.x * 0.8f, 2f) * EffectiveRoomGravity;
		}
		else if (GetTunnelMode)
		{
			base.bodyChunks[1].vel = base.bodyChunks[0].vel + climbingOrientation.normalized * 0.4f * EffectiveRoomGravity;
		}
		else
		{
			base.bodyChunks[1].vel = base.bodyChunks[0].vel + new Vector2(Mathf.Lerp(0f, Mathf.Sign(base.bodyChunks[0].vel.x) * 0.2f, headLeadingCounter / 60f), Mathf.Lerp(2f, 1f, headLeadingCounter / 60f) * EffectiveRoomGravity);
		}
		if (base.grasps[0] != null)
		{
			CarryObject(eu);
		}
		Act();
	}

	public override void InitiateGraphicsModule()
	{
		if (base.graphicsModule == null)
		{
			base.graphicsModule = new YeekGraphics(this);
		}
	}

	public void Act()
	{
		MovementConnection movementConnection = default(MovementConnection);
		if (!base.abstractCreature.controlled)
		{
			AI.Update();
			movementConnection = (AI.pathFinder as StandardPather).FollowPath(room.GetWorldCoordinate(base.mainBodyChunk.pos), actuallyFollowingThisPath: true);
		}
		else if (inputWithDiagonals.HasValue && lastInputWithDiagonals.HasValue)
		{
			AI.stuckTracker.satisfiedWithThisPosition = true;
			MovementConnection.MovementType type = MovementConnection.MovementType.Standard;
			base.GoThroughFloors = inputWithDiagonals.Value.y < 0;
			if (room.GetTile(base.mainBodyChunk.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
			{
				type = MovementConnection.MovementType.ShortCut;
			}
			if (room.aimap.getAItile(base.mainBodyChunk.pos).narrowSpace || room.GetTile(base.mainBodyChunk.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
			{
				tunnelCrawlingMode = true;
			}
			else
			{
				tunnelCrawlingMode = false;
			}
			if (!lastInputWithDiagonals.Value.pckp && inputWithDiagonals.Value.pckp && base.grasps[0] != null)
			{
				grabCooldown = 1f;
				base.grasps[0].Release();
			}
			if (!lastInputWithDiagonals.Value.jmp && inputWithDiagonals.Value.jmp)
			{
				EndClimb();
				timeSinceHop = 10000;
				timeSinceJump = 10000;
				AI.behavior = YeekAI.Behavior.Hungry;
				Jump(base.firstChunk.pos, base.firstChunk.pos + new Vector2(inputWithDiagonals.Value.x, 15f));
				if (!climbingMode)
				{
					interestInClimbingPoles = 1f;
				}
				else
				{
					interestInClimbingPoles = -1f;
				}
			}
			else if (inputWithDiagonals.Value.x != 0 || inputWithDiagonals.Value.y != 0)
			{
				if (room.GetTile(base.abstractCreature.pos.Tile + new IntVector2(inputWithDiagonals.Value.IntVec.x, 0)).Solid && (!room.GetTile(base.abstractCreature.pos.Tile + new IntVector2(inputWithDiagonals.Value.IntVec.x, -1)).Solid ^ room.GetTile(base.abstractCreature.pos.Tile + new IntVector2(inputWithDiagonals.Value.IntVec.x, -2)).Solid) && (room.GetTile(base.abstractCreature.pos.Tile + new IntVector2(0, -1)).Solid || base.Submersion > 0.5f))
				{
					base.firstChunk.vel += new Vector2(inputWithDiagonals.Value.IntVec.x, 3f);
				}
				if ((room.GetTile(base.abstractCreature.pos.Tile + new IntVector2(0, -1)).Solid && !room.GetTile(base.abstractCreature.pos.Tile + new IntVector2(inputWithDiagonals.Value.IntVec.x, -1)).Solid) || (!room.GetTile(base.abstractCreature.pos.Tile + new IntVector2(inputWithDiagonals.Value.IntVec.x, -1)).Solid && room.aimap.getAItile(base.abstractCreature.pos.Tile + new IntVector2(inputWithDiagonals.Value.IntVec.x, -1)).narrowSpace))
				{
					timeSinceHop = 1;
					timeSinceJump = 1;
					base.GoThroughFloors = true;
				}
				else
				{
					base.GoThroughFloors = false;
				}
				if (tunnelCrawlingMode)
				{
					movementConnection = new MovementConnection(type, room.GetWorldCoordinate(base.mainBodyChunk.pos), room.GetWorldCoordinate(base.mainBodyChunk.pos + new Vector2((float)inputWithDiagonals.Value.x * 20f, (float)inputWithDiagonals.Value.y * 20f)), 2);
					base.GoThroughFloors = true;
				}
				else if (climbingMode)
				{
					if (!room.GetTile(room.MiddleOfTile(base.abstractCreature.pos)).AnyBeam)
					{
						EndClimb();
					}
					else
					{
						bool anyBeam = room.GetTile(room.MiddleOfTile(base.abstractCreature.pos) + new Vector2(-20f, 0f)).AnyBeam;
						bool anyBeam2 = room.GetTile(room.MiddleOfTile(base.abstractCreature.pos) + new Vector2(20f, 0f)).AnyBeam;
						bool anyBeam3 = room.GetTile(room.MiddleOfTile(base.abstractCreature.pos) + new Vector2(0f, -20f)).AnyBeam;
						bool anyBeam4 = room.GetTile(room.MiddleOfTile(base.abstractCreature.pos) + new Vector2(0f, 20f)).AnyBeam;
						float num = inputWithDiagonals.Value.x;
						float num2 = inputWithDiagonals.Value.y;
						if (!anyBeam && num < 0f)
						{
							num = 0f;
						}
						if (!anyBeam2 && num > 0f)
						{
							num = 0f;
						}
						if (!anyBeam3 && num2 < 0f)
						{
							num2 = 0f;
						}
						if (!anyBeam4 && num2 > 0f)
						{
							num2 = 0f;
						}
						movementConnection = new MovementConnection(type, room.GetWorldCoordinate(base.mainBodyChunk.pos), room.GetWorldCoordinate(base.mainBodyChunk.pos + new Vector2(num * 40f, num2 * 40f)), 2);
					}
				}
				else
				{
					if (room.GetTile(room.MiddleOfTile(base.abstractCreature.pos)).AnyBeam)
					{
						if (base.abstractCreature.controlled && inputWithDiagonals.Value.y > 0)
						{
							interestInClimbingPoles += 0.2f;
						}
						else
						{
							interestInClimbingPoles += 0.2f;
						}
						if (interestInClimbingPoles >= 1f)
						{
							interestInClimbingPoles = 1f;
							movementConnection = new MovementConnection(type, room.GetWorldCoordinate(base.mainBodyChunk.pos), room.GetWorldCoordinate(base.mainBodyChunk.pos + new Vector2(0f, 10f)), 1);
						}
					}
					else
					{
						interestInClimbingPoles = -1f;
					}
					if (OnGround)
					{
						movementConnection = new MovementConnection(type, room.GetWorldCoordinate(base.mainBodyChunk.pos), room.GetWorldCoordinate(base.mainBodyChunk.pos + new Vector2((float)inputWithDiagonals.Value.x * 20f, 0f)), 2);
					}
					else
					{
						base.firstChunk.vel.x = base.firstChunk.vel.x + (float)inputWithDiagonals.Value.x / 10f;
					}
				}
			}
			else if (climbingMode)
			{
				movementConnection = default(MovementConnection);
				base.mainBodyChunk.vel *= 0.2f;
				base.mainBodyChunk.vel.y = base.mainBodyChunk.vel.y + base.gravity;
			}
			AI.behavior = YeekAI.Behavior.Idle;
			AI.fearCounter = Mathf.Lerp(AI.fearCounter, inputWithDiagonals.Value.thrw ? 0.5f : 0f, 0.1f);
		}
		if (!GetClimbingMode && !tunnelCrawlingMode && (room.GetTile(base.mainBodyChunk.pos).wormGrass || AI.stranded) && Random.value < 0.2f)
		{
			Vector2 vector = new Vector2(Mathf.Lerp(Random.Range(-1f, 1f), Mathf.Sign(base.firstChunk.pos.x - room.MiddleOfTile(AI.pathFinder.GetDestination).x), 0.3f) * 30f, 0f);
			if (room.GetTile(base.mainBodyChunk.pos).wormGrass)
			{
				vector.x *= 2f;
			}
			AI.fearCounter = 1f;
			YeekCall();
			Hop(base.firstChunk.pos, base.firstChunk.pos + vector + new Vector2(0f, 38f + Random.value * 20f));
			return;
		}
		if (movementConnection == default(MovementConnection))
		{
			if (climbingMode)
			{
				if (room.GetTile(room.MiddleOfTile(base.abstractCreature.pos)).AnyBeam)
				{
					lastSafeClimb = room.MiddleOfTile(base.abstractCreature.pos);
				}
				if (Vector2.Distance(base.firstChunk.pos, lastSafeClimb) > 40f)
				{
					EndClimb();
				}
			}
			return;
		}
		if (base.graphicsModule != null)
		{
			(base.graphicsModule as YeekGraphics).debugPos = base.bodyChunks[0].pos;
		}
		if (!base.abstractCreature.controlled)
		{
			base.GoThroughFloors = (float)movementConnection.destinationCoord.y <= (float)base.abstractCreature.pos.y;
		}
		if (room.GetTile(room.MiddleOfTile(base.abstractCreature.pos)).AnyBeam && !room.aimap.getAItile(base.mainBodyChunk.pos).narrowSpace)
		{
			AI.stuckTracker.satisfiedWithThisPosition = false;
			interestInClimbingPoles += 0.15f;
			if (base.Submersion >= 0.5f)
			{
				interestInClimbingPoles += 0.1f;
			}
			if (interestInClimbingPoles > 1f)
			{
				interestInClimbingPoles = 1f;
			}
		}
		else
		{
			if (interestInClimbingPoles > 0f)
			{
				interestInClimbingPoles -= 0.1f;
			}
			if (interestInClimbingPoles < 0f)
			{
				interestInClimbingPoles += 0.01f;
			}
		}
		if ((interestInClimbingPoles >= 1f && room.GetTile(base.firstChunk.pos).AnyBeam) || climbingMode)
		{
			if (room.GetTile(base.firstChunk.pos).AnyBeam)
			{
				lastSafeClimb = room.MiddleOfTile(base.firstChunk.pos);
			}
			base.GoThroughFloors = true;
			if (base.graphicsModule != null)
			{
				room.MiddleOfTile(base.firstChunk.pos);
				_ = room.GetTile(base.firstChunk.pos).horizontalBeam;
				_ = room.GetTile(base.firstChunk.pos).verticalBeam;
				AI.stuckTracker.satisfiedWithThisPosition = true;
			}
			IntVector2 intVector = movementConnection.DestTile;
			if (base.abstractCreature.controlled)
			{
				intVector = base.abstractCreature.pos.Tile + new IntVector2(inputWithDiagonals.Value.x * 2, inputWithDiagonals.Value.y * 2);
			}
			if (room.GetTile(intVector).Terrain == Room.Tile.TerrainType.Slope)
			{
				intVector = base.abstractCreature.pos.Tile;
			}
			if (base.Submersion > 0.5f)
			{
				BodyChunk bodyChunk = base.firstChunk;
				bodyChunk.vel.y = bodyChunk.vel.y + 12f;
			}
			Climb(intVector);
			if (movementConnection.type == MovementConnection.MovementType.Slope || movementConnection.type == MovementConnection.MovementType.CeilingSlope || (movementConnection.type == MovementConnection.MovementType.ShortCut && shortcutDelay > 0) || movementConnection.type == MovementConnection.MovementType.OpenDiagonal || movementConnection.type == MovementConnection.MovementType.SemiDiagonalReach || movementConnection.type == MovementConnection.MovementType.ReachUp)
			{
				AI.stuckTracker.satisfiedWithThisPosition = false;
				interestInClimbingPoles = 1f;
				Vector2 vector2 = Custom.DirVec(base.firstChunk.pos, room.MiddleOfTile(AI.pathFinder.GetDestination));
				Hop(base.firstChunk.pos, room.MiddleOfTile(movementConnection.destinationCoord) + vector2 + new Vector2(0f, 4f));
				base.firstChunk.vel += new Vector2(0f, 1f) + vector2 * 3f;
				EndClimb();
			}
			if (!room.GetTile(movementConnection.DestTile).AnyBeam)
			{
				EndClimb();
			}
		}
		else if (!base.abstractCreature.controlled && (movementConnection.type == MovementConnection.MovementType.DropToFloor || movementConnection.type == MovementConnection.MovementType.DropToWater || movementConnection.type == MovementConnection.MovementType.DropToClimb || movementConnection.type == MovementConnection.MovementType.ReachDown))
		{
			Vector2 vector3 = Custom.DirVec(base.firstChunk.pos, room.MiddleOfTile(movementConnection.destinationCoord));
			vector3.y = 0f;
			Hop(base.firstChunk.pos, room.MiddleOfTile(movementConnection.destinationCoord) + vector3 + new Vector2(0f, 5f));
			EndClimb();
		}
		else if (movementConnection.type == MovementConnection.MovementType.Standard || movementConnection.type == MovementConnection.MovementType.Slope || movementConnection.type == MovementConnection.MovementType.CeilingSlope || movementConnection.type == MovementConnection.MovementType.OpenDiagonal || movementConnection.type == MovementConnection.MovementType.SemiDiagonalReach || movementConnection.type == MovementConnection.MovementType.ReachUp || (movementConnection.type == MovementConnection.MovementType.ShortCut && shortcutDelay > 0))
		{
			if (base.abstractCreature.abstractAI.destination.room == room.abstractRoom.index && Custom.DistLess(base.firstChunk.pos, room.MiddleOfTile(base.abstractCreature.abstractAI.destination), 16f))
			{
				AI.stuckTracker.satisfiedWithThisPosition = true;
			}
			else
			{
				AI.stuckTracker.satisfiedWithThisPosition = false;
			}
			if (!base.abstractCreature.controlled && !AI.stuckTracker.satisfiedWithThisPosition && AI.stuckTracker.Utility() >= 1f && Random.value < 0.01f)
			{
				interestInClimbingPoles = 0f;
				Hop(base.firstChunk.pos, base.firstChunk.pos + Custom.RNV() * 2f);
			}
			if (room.aimap.getAItile(movementConnection.DestTile).narrowSpace || (tunnelCrawlingMode && room.aimap.getAItile(base.firstChunk.lastPos).narrowSpace) || room.GetTile(base.mainBodyChunk.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
			{
				Vector2 vector4 = room.MiddleOfTile(movementConnection.DestTile);
				if (base.abstractCreature.controlled)
				{
					vector4 = base.firstChunk.pos + new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y);
				}
				Vector2 vector5 = Custom.DirVec(base.firstChunk.pos, vector4);
				climbingOrientation = vector5.normalized;
				float y = vector5.y;
				if (base.graphicsModule == null)
				{
					base.firstChunk.vel = (vector5 + new Vector2(0f, base.gravity * (2f * Mathf.InverseLerp(-1f, 1f, y)))) * Mathf.InverseLerp(3f, 15f, Vector2.Distance(base.firstChunk.pos, vector4));
				}
				else if ((base.graphicsModule as YeekGraphics).CanAdvanceClimb)
				{
					base.firstChunk.vel = vector5;
					(base.graphicsModule as YeekGraphics).AdvanceClimb();
				}
				base.firstChunk.vel += new Vector2(0f, base.gravity * (base.TotalMass * 3.35f));
				base.firstChunk.vel += Custom.RNV() * AI.stuckTracker.Utility();
				base.bodyChunks[1].vel += Custom.DirVec(base.bodyChunks[1].pos, base.bodyChunks[0].pos) / 10f;
				tunnelCrawlingMode = true;
				EndClimb();
				if (!room.aimap.getAItile(movementConnection.DestTile).narrowSpace && room.aimap.getAItile(base.abstractCreature.pos).narrowSpace)
				{
					tunnelCrawlingMode = false;
					base.GoThroughFloors = false;
					base.firstChunk.vel += vector5.normalized * 4f;
				}
				if (room.GetTile(movementConnection.DestTile).AnyBeam && room.aimap.getAItile(base.abstractCreature.pos).narrowSpace && !room.aimap.getAItile(movementConnection.DestTile).narrowSpace)
				{
					tunnelCrawlingMode = false;
					interestInClimbingPoles = 1f;
					base.GoThroughFloors = true;
					climbingMode = true;
				}
				return;
			}
			tunnelCrawlingMode = false;
			if (base.Submersion > 0.5f)
			{
				Vector2 p = room.MiddleOfTile(movementConnection.destinationCoord);
				base.firstChunk.vel += Custom.DirVec(base.firstChunk.pos, room.MiddleOfTile(movementConnection.destinationCoord)) / 15f;
				if (base.abstractCreature.controlled)
				{
					p = base.firstChunk.pos + new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y);
				}
				Vector2 vector6 = Custom.DirVec(base.firstChunk.pos, p);
				base.firstChunk.vel = vector6 * 0.8f;
				float y2 = Mathf.InverseLerp(0.4f, 1f, base.Submersion);
				base.firstChunk.vel += new Vector2(0f, y2);
				if (base.graphicsModule != null)
				{
					(base.graphicsModule as YeekGraphics).swimCounter += 0.1f;
				}
				if (!climbingMode && room.GetTile(movementConnection.destinationCoord).AnyBeam && !room.PointSubmerged(room.MiddleOfTile(movementConnection.destinationCoord)))
				{
					interestInClimbingPoles = 1f;
					Climb(movementConnection.destinationCoord.Tile);
					base.firstChunk.vel += Custom.DirVec(base.firstChunk.pos, room.MiddleOfTile(movementConnection.destinationCoord)) / 4f;
				}
				return;
			}
			Vector2 vector7 = Custom.DirVec(base.firstChunk.pos, room.MiddleOfTile(AI.pathFinder.GetDestination));
			if (AI.behavior == YeekAI.Behavior.Fear)
			{
				Jump(base.firstChunk.pos, room.MiddleOfTile(movementConnection.destinationCoord) + vector7 + new Vector2(0f, 20f));
			}
			if (!base.abstractCreature.controlled && !climbingMode && base.abstractCreature.pos.room == base.abstractCreature.abstractAI.destination.room && Vector2.Distance(base.abstractCreature.pos.Tile.ToVector2(), base.abstractCreature.abstractAI.destination.Tile.ToVector2()) < 8f)
			{
				AI.stuckTracker.satisfiedWithThisPosition = true;
				if (AI.behavior == YeekAI.Behavior.Hungry || base.abstractCreature.abstractAI.destination.room != base.abstractCreature.Room.index || room.GetTile(base.mainBodyChunk.pos).wormGrass)
				{
					if (AI.behavior == YeekAI.Behavior.Hungry && Random.value < 0.75f && base.abstractCreature.pos.Tile.y >= base.abstractCreature.abstractAI.destination.Tile.y)
					{
						Jump(base.firstChunk.pos, room.MiddleOfTile(base.abstractCreature.abstractAI.destination));
					}
					else
					{
						Hop(base.firstChunk.pos, room.MiddleOfTile(movementConnection.destinationCoord) + vector7 + new Vector2(0f, 9f));
					}
				}
				return;
			}
			if (base.abstractCreature.controlled)
			{
				float num3 = 0.8f;
				if (inputWithDiagonals.HasValue && inputWithDiagonals.Value.y < 0)
				{
					num3 = 0.2f;
				}
				Hop(base.firstChunk.pos, room.MiddleOfTile(movementConnection.destinationCoord) + vector7 * num3 + new Vector2(0f, 10f));
				return;
			}
			AI.stuckTracker.satisfiedWithThisPosition = false;
			if (movementConnection.type == MovementConnection.MovementType.Slope)
			{
				Hop(base.firstChunk.pos, room.MiddleOfTile(movementConnection.destinationCoord) + vector7 * 0.8f + new Vector2(0f, 6f));
			}
			else if (!climbingMode && Mathf.Abs(base.firstChunk.pos.x - room.MiddleOfTile(AI.pathFinder.GetDestination).x) < 10f && Mathf.Abs(base.firstChunk.pos.y - room.MiddleOfTile(AI.pathFinder.GetDestination).y) > 50f && room.MiddleOfTile(AI.pathFinder.GetDestination).y > base.firstChunk.pos.y)
			{
				Jump(base.firstChunk.pos, room.MiddleOfTile(movementConnection.destinationCoord) + vector7 + new Vector2(0f, 40f));
			}
			else if (!climbingMode && base.firstChunk.pos.y + 15f < room.MiddleOfTile(AI.pathFinder.GetDestination).y && Mathf.Abs(base.firstChunk.pos.x - room.MiddleOfTile(AI.pathFinder.GetDestination).x) < 30f)
			{
				Hop(base.firstChunk.pos, room.MiddleOfTile(movementConnection.destinationCoord) + vector7 + new Vector2(0f, 20f));
			}
			else if (!climbingMode)
			{
				Hop(base.firstChunk.pos, room.MiddleOfTile(movementConnection.destinationCoord) + vector7 + new Vector2(0f, 10f));
			}
		}
		else if (!base.abstractCreature.controlled && !climbingMode && movementConnection.type == MovementConnection.MovementType.ReachOverGap)
		{
			Vector2 vector8 = Custom.DirVec(base.firstChunk.pos, room.MiddleOfTile(movementConnection.destinationCoord));
			Hop(base.firstChunk.pos, room.MiddleOfTile(movementConnection.destinationCoord) + vector8 * 20f + new Vector2(0f, 15f));
		}
		else
		{
			if (enteringShortCut.HasValue || (movementConnection.type != MovementConnection.MovementType.ShortCut && movementConnection.type != MovementConnection.MovementType.NPCTransportation) || shortcutDelay > 0 || grabbedBy.Count > 0)
			{
				return;
			}
			enteringShortCut = movementConnection.StartTile;
			if (base.abstractCreature.controlled)
			{
				bool flag = false;
				List<IntVector2> list = new List<IntVector2>();
				ShortcutData[] shortcuts = room.shortcuts;
				for (int i = 0; i < shortcuts.Length; i++)
				{
					ShortcutData shortcutData = shortcuts[i];
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
			if (AI.behavior == YeekAI.Behavior.ReturnFood && base.grasps[0] != null)
			{
				base.grasps[0].grabbed.abstractPhysicalObject.Abstractize(AI.denFinder.GetDenPosition().Value);
				base.grasps[0].grabbed.abstractPhysicalObject.Destroy();
				base.grasps[0].Release();
				AI.getNewIdlePos(50);
				(base.abstractCreature.state as YeekState).Feed(room.world.rainCycle.timer);
			}
		}
	}

	public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
	{
		base.Collide(otherObject, myChunk, otherChunk);
		bool flag = false;
		if (base.abstractCreature.controlled && inputWithDiagonals.HasValue && base.grasps[0] == null && grabCooldown <= 0f)
		{
			flag = inputWithDiagonals.Value.pckp;
		}
		if (!base.abstractCreature.controlled && grabCooldown <= 0f)
		{
			flag = true;
		}
		if (!base.Consious)
		{
			flag = false;
		}
		if (!flag)
		{
			return;
		}
		if (base.grasps[0] == null && otherObject.grabbedBy.Count == 0 && otherObject.abstractPhysicalObject is AbstractConsumable && otherObject is IPlayerEdible && ((AI.goalFruit != null && otherObject.abstractPhysicalObject == AI.goalFruit) || AI.goalFruit == null || base.abstractCreature.controlled))
		{
			if (AI.goalFruit == null)
			{
				AI.goalFruit = otherObject.abstractPhysicalObject as AbstractConsumable;
			}
			room.PlaySound(SoundID.Vulture_Grab_NPC, base.bodyChunks[0]);
			Grab(otherObject, 0, 0, Grasp.Shareability.NonExclusive, 0.1f, overrideEquallyDominant: false, pacifying: false);
		}
		else if (base.grasps[0] == null && otherObject.grabbedBy.Count == 0 && (base.abstractCreature.state as YeekState).HungerIntensity(room.world.rainCycle.timer) > 0.5f && otherObject.abstractPhysicalObject is AbstractConsumable && otherObject is IPlayerEdible && (otherObject.abstractPhysicalObject as AbstractConsumable).isConsumed)
		{
			AI.goalFruit = otherObject.abstractPhysicalObject as AbstractConsumable;
			room.PlaySound(SoundID.Vulture_Grab_NPC, base.bodyChunks[0]);
			Grab(otherObject, 0, 0, Grasp.Shareability.NonExclusive, 0.1f, overrideEquallyDominant: false, pacifying: false);
		}
	}

	public void SetPlayerHoldingBodyMass()
	{
		if (usingStandardMass)
		{
			usingStandardMass = false;
			BodyChunk[] array = base.bodyChunks;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].mass = 0.01f;
			}
		}
	}

	public void SetStandardBodyMass()
	{
		if (!usingStandardMass)
		{
			usingStandardMass = true;
			base.bodyChunks[0].mass = 0.6f;
			base.bodyChunks[1].mass = 0.05f;
		}
	}

	public void Hop(Vector2 currentPos, Vector2 goalPos, bool forced = false, bool allowInTunnel = false, bool calledFromJump = false)
	{
		if (base.Consious && (OnGround || forced || allowInTunnel || allowInTunnel || climbingMode) && !tunnelCrawlingMode && timeSinceHop > maxJumpCounter && (!tunnelCrawlingMode || allowInTunnel))
		{
			EndClimb();
			timeSinceHop = 0;
			if ((base.State as HealthState).health < 1f)
			{
				float num = 1f - (base.State as HealthState).health;
				Stun((int)(40f * num));
				goalPos.x += (float)Random.Range(-25, 25) * num;
			}
			Vector2 vector = Custom.DirVec(currentPos, goalPos);
			if (Mathf.Abs((base.mainBodyChunk.vel + vector / 10f).x) < 10f)
			{
				vector.x *= 0.1f * Vector2.Distance(new Vector2(currentPos.x, 0f), new Vector2(goalPos.x, 0f));
			}
			vector.y *= Mathf.Lerp(0f, Vector2.Distance(currentPos, goalPos) * 0.6f, base.gravity);
			if (vector.y < 3f)
			{
				vector.y = 4f;
			}
			if (grabbedBy.Count > 0 && grabbedBy[0].grabber is Player player && player.animation == Player.AnimationIndex.ClimbOnBeam)
			{
				vector.y *= 0.75f;
			}
			float num2 = 1f;
			if (!forced && !calledFromJump)
			{
				num2 = Random.Range(0.2f, 1f);
			}
			Vector2 vector2 = new Vector2(vector.x * (1f + 1.4f * AI.fearCounter), vector.y) * ((usingStandardMass ? 1f : 0.85f) * num2);
			if (base.mainBodyChunk.submersion > 0.2f)
			{
				vector2 *= 0.35f;
			}
			if (vector2.magnitude > (usingStandardMass ? 50f : 40f))
			{
				vector2 = vector2.normalized * (usingStandardMass ? 50f : 40f);
			}
			if (vector2.y > ceilingClearance * 5f)
			{
				vector2.y = ceilingClearance * 5f;
			}
			base.mainBodyChunk.vel += vector2;
			room.PlaySound(SoundID.Slugcat_Wall_Jump, base.bodyChunks[0], loop: false, Mathf.InverseLerp(0f, 10f, vector.y), 2.5f + Mathf.InverseLerp(0f, 100f, vector.y));
		}
	}

	public void Hop(Vector2 currentPos, Vector2 goalPos, float cappedIntensity)
	{
		Vector2 vector = Custom.DirVec(currentPos, goalPos);
		vector *= Mathf.Min(Vector2.Distance(currentPos, goalPos), cappedIntensity);
		Hop(currentPos, currentPos + vector);
	}

	public void Jump(Vector2 currentPos, Vector2 goalPos, bool forced = false, bool allowInTunnel = false)
	{
		if (timeSinceJump > 30)
		{
			timeSinceHop = 10000;
			timeSinceJump = 0;
			Vector2 vector = Custom.DirVec(currentPos, goalPos);
			vector.x *= 0.45f;
			vector.y *= 15f + Random.Range(0f, 10f);
			if (AI.behavior == YeekAI.Behavior.Hungry || (AI.behavior == YeekAI.Behavior.Fear && AI.fearCounter > 0.5f))
			{
				vector.y *= 2f;
			}
			if (AI.behavior == YeekAI.Behavior.Fear && AI.fearCounter > 0.8f)
			{
				vector.x *= 16f;
			}
			Hop(currentPos, currentPos + vector, forced, allowInTunnel, calledFromJump: true);
		}
	}

	public override void SpitOutOfShortCut(IntVector2 pos, Room newRoom, bool spitOutAllSticks)
	{
		base.SpitOutOfShortCut(pos, newRoom, spitOutAllSticks);
		Vector2 vector = Custom.IntVector2ToVector2(newRoom.ShorcutEntranceHoleDirection(pos));
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			base.bodyChunks[i].pos = newRoom.MiddleOfTile(pos) - vector * ((i != 1) ? 5f : 10f) + Custom.DegToVec(Random.value * 360f);
			base.bodyChunks[i].lastPos = newRoom.MiddleOfTile(pos);
			base.bodyChunks[i].vel = vector * 5f;
		}
		Hop(base.firstChunk.pos, base.firstChunk.pos + new Vector2(vector.x * 10f, vector.y));
		EndClimb();
		tunnelCrawlingMode = true;
		interestInClimbingPoles = 1f;
		shortcutDelay = 80;
		for (int j = 0; j < tails.Length; j++)
		{
			tails[j].Reset(base.bodyChunks[0].pos);
		}
		if (base.graphicsModule != null)
		{
			base.graphicsModule.Reset();
		}
	}

	public override void NewRoom(Room newRoom)
	{
		base.NewRoom(newRoom);
		for (int i = 0; i < tails.Length; i++)
		{
			tails[i].Reset(base.bodyChunks[0].pos);
		}
	}

	public void Climb(IntVector2 climbTile)
	{
		if (!climbingMode)
		{
			AI.stuckTracker.satisfiedWithThisPosition = true;
			climbingMode = true;
			climbingOrientation = Vector2.Lerp(climbingOrientation, Custom.DirVec(base.firstChunk.pos, room.MiddleOfTile(climbTile)).normalized * 10f, 0.1f);
			base.mainBodyChunk.vel *= 0.1f;
			base.bodyChunks[1].vel *= 0.2f;
			if (base.graphicsModule != null)
			{
				(base.graphicsModule as YeekGraphics).StartClimbing(base.firstChunk.pos);
				AI.stuckTracker.satisfiedWithThisPosition = true;
			}
			return;
		}
		if (base.graphicsModule != null)
		{
			YeekGraphics yeekGraphics = base.graphicsModule as YeekGraphics;
			if (yeekGraphics.CanAdvanceClimb || Random.value < 0.05f)
			{
				climbingOrientation = Custom.DirVec(room.MiddleOfTile(base.firstChunk.pos), room.MiddleOfTile(climbTile));
				yeekGraphics.AdvanceClimb();
				base.firstChunk.vel += climbingOrientation * 0.7f;
			}
			base.firstChunk.vel *= 0.6f;
			base.bodyChunks[1].vel *= 0.6f;
			base.firstChunk.vel += climbingOrientation * 0.1f;
			base.bodyChunks[1].vel += climbingOrientation * 0.12f;
			float num = Mathf.Lerp(1.12f, 1.04f, Mathf.InverseLerp(-1f, 1f, (base.firstChunk.pos - room.MiddleOfTile(climbTile)).y));
			base.firstChunk.vel += new Vector2(0f, base.gravity * num);
			if (base.abstractCreature.controlled && inputWithDiagonals.HasValue && inputWithDiagonals.Value.y < 0)
			{
				base.firstChunk.vel += new Vector2(0f, base.gravity * (0f - num));
			}
		}
		else
		{
			AI.stuckTracker.satisfiedWithThisPosition = true;
			climbingOrientation = Custom.DirVec(room.MiddleOfTile(base.firstChunk.pos), room.MiddleOfTile(climbTile));
			base.firstChunk.pos = Vector2.Lerp(base.firstChunk.pos, base.firstChunk.pos + climbingOrientation + new Vector2(0f, 5f), 0.6f);
			base.firstChunk.vel *= 0f;
			base.bodyChunks[1].vel *= 0f;
		}
		climbingOrientation.Normalize();
	}

	public void CarryObject(bool eu)
	{
		if (base.graphicsModule != null)
		{
			YeekGraphics yeekGraphics = base.graphicsModule as YeekGraphics;
			base.grasps[0].grabbedChunk.MoveFromOutsideMyUpdate(eu, base.bodyChunks[1].pos + yeekGraphics.headDrawDirection * 10f);
		}
		else
		{
			base.grasps[0].grabbedChunk.MoveFromOutsideMyUpdate(eu, base.bodyChunks[1].pos);
		}
		base.grasps[0].grabbedChunk.vel = base.mainBodyChunk.vel;
		if (Vector2.Distance(base.grasps[0].grabbedChunk.pos, base.bodyChunks[1].pos) > 100f)
		{
			base.grasps[0].Release();
		}
	}

	public void YeekCall()
	{
		if (yeekCallCounter <= 0f)
		{
			yeekCallCounter = 1f;
			if (AI.fearCounter > 0.3f)
			{
				room.PlaySound(SoundID.Small_Needle_Worm_Little_Trumpet, base.bodyChunks[0], loop: false, 1f, 1.5f + Mathf.InverseLerp(0f, 1f, base.abstractCreature.personality.dominance));
			}
			else
			{
				room.PlaySound(SoundID.Big_Needle_Worm_Small_Trumpet, base.bodyChunks[0], loop: false, 1f, 1.5f + Mathf.InverseLerp(0f, 1f, base.abstractCreature.personality.dominance));
			}
		}
	}

	public void FlyingWeapon(Weapon weapon)
	{
		if (base.Consious && Vector2.Distance(base.firstChunk.pos, weapon.firstChunk.pos) < 80f && Vector2.Distance(base.firstChunk.pos, weapon.firstChunk.pos) < Vector2.Distance(base.firstChunk.lastPos, weapon.firstChunk.pos))
		{
			AI.fearCounter = 1f;
			YeekCall();
			AI.MakeCreatureLeaveRoom();
			if (weapon.firstChunk.pos.y <= base.firstChunk.pos.y + 15f)
			{
				Jump(base.firstChunk.pos, base.firstChunk.pos - Custom.DirVec(base.firstChunk.pos, weapon.firstChunk.pos));
			}
		}
	}

	public bool CheckOnGround(Vector2 Pos)
	{
		if (enteringShortCut.HasValue || room == null)
		{
			return false;
		}
		if (grabbedBy.Count > 0 && grabbedBy[0].grabber is Player player)
		{
			return player.lowerBodyFramesOffGround < 10;
		}
		float num = 1f;
		bool flag = room.GetTile(Pos + new Vector2(0f, -20f)).Terrain == Room.Tile.TerrainType.Slope || room.GetTile(Pos + new Vector2(0f - base.mainBodyChunk.rad * num, -10f)).Terrain == Room.Tile.TerrainType.Slope || room.GetTile(Pos + new Vector2(base.mainBodyChunk.rad * num, -10f)).Terrain == Room.Tile.TerrainType.Slope;
		bool flag2 = room.GetTile(Pos + new Vector2(0f, -20f)).Terrain == Room.Tile.TerrainType.Solid || room.GetTile(Pos + new Vector2(0f - base.mainBodyChunk.rad * num, -10f)).Terrain == Room.Tile.TerrainType.Solid || room.GetTile(Pos + new Vector2(base.mainBodyChunk.rad * num, -10f)).Terrain == Room.Tile.TerrainType.Solid;
		bool flag3 = false;
		if (room.IsPositionInsideBoundries(room.GetTilePosition(Pos + new Vector2(0f, -20f))))
		{
			flag3 = room.GetTile(Pos + new Vector2(0f, -20f)).Terrain == Room.Tile.TerrainType.ShortcutEntrance;
		}
		return (!base.GoThroughFloors && (room.GetTile(Pos + new Vector2(0f, -10f)).Terrain == Room.Tile.TerrainType.Floor || room.GetTile(Pos + new Vector2(0f - base.mainBodyChunk.rad * num, -10f)).Terrain == Room.Tile.TerrainType.Floor || room.GetTile(Pos + new Vector2(base.mainBodyChunk.rad * num, -10f)).Terrain == Room.Tile.TerrainType.Floor)) || flag2 || flag || flag3;
	}

	public void EndClimb()
	{
		if (climbingMode)
		{
			climbingMode = false;
		}
		interestInClimbingPoles = -1f;
	}
}
