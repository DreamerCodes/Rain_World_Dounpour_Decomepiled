using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class DropBug : InsectoidCreature, Weapon.INotifyOfFlyingWeapons
{
	public DropBugAI AI;

	private int footingCounter;

	public int outOfWaterFooting;

	public int specialMoveCounter;

	public IntVector2 specialMoveDestination;

	private MovementConnection lastFollowedConnection;

	public float runCycle;

	public bool sitting;

	public bool swimming;

	public Vector2 travelDir;

	public float charging;

	public BodyChunk jumpAtChunk;

	public float attemptBite;

	public bool jumping;

	public bool fromCeilingJump;

	public float dropAnticipation;

	public int grabOnNextAttack;

	public int grabbedCounter;

	public int releaseGrabbedCounter;

	public int luredToDropCounter;

	public int afterDropAttackDelay;

	private float carryObjectMass;

	private float walkBackwardsDist;

	public float stuckShake;

	public float inCeilingMode;

	public ChunkSoundEmitter voiceSound;

	public Vector2 jumpAtPos;

	public new HealthState State => base.abstractCreature.state as HealthState;

	public bool Footing
	{
		get
		{
			if (footingCounter <= 10)
			{
				return outOfWaterFooting > 0;
			}
			return true;
		}
	}

	public override float VisibilityBonus => inCeilingMode * -0.8f;

	public bool MoveBackwards
	{
		get
		{
			if (AI.behavior == DropBugAI.Behavior.SitInCeiling && AI.ceilingModule.ceilingPos.room == base.abstractCreature.pos.room && AI.ceilingModule.ceilingPos.Tile.FloatDist(base.abstractCreature.pos.Tile) < walkBackwardsDist)
			{
				return AI.stuckTracker.Utility() == 0f;
			}
			return false;
		}
	}

	public DropBug(AbstractCreature abstractCreature, World world)
		: base(abstractCreature, world)
	{
		float num = 0.8f;
		base.bodyChunks = new BodyChunk[3];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 6f, num * 0.4f);
		base.bodyChunks[1] = new BodyChunk(this, 1, new Vector2(0f, 0f), 8f, num * 0.4f);
		base.bodyChunks[2] = new BodyChunk(this, 2, new Vector2(0f, 0f), 6f, num * 0.2f);
		bodyChunkConnections = new BodyChunkConnection[3];
		bodyChunkConnections[0] = new BodyChunkConnection(base.bodyChunks[0], base.bodyChunks[1], 12f, BodyChunkConnection.Type.Normal, 1f, -1f);
		bodyChunkConnections[1] = new BodyChunkConnection(base.bodyChunks[1], base.bodyChunks[2], 14f, BodyChunkConnection.Type.Normal, 1f, -1f);
		bodyChunkConnections[2] = new BodyChunkConnection(base.bodyChunks[0], base.bodyChunks[2], 8f, BodyChunkConnection.Type.Push, 1f, -1f);
		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		bounce = 0.1f;
		surfaceFriction = 0.4f;
		collisionLayer = 1;
		base.waterFriction = 0.96f;
		base.buoyancy = 0.95f;
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(abstractCreature.ID.RandomSeed);
		UnityEngine.Random.state = state;
		jumpAtPos = Vector2.zero;
	}

	public override void InitiateGraphicsModule()
	{
		if (base.graphicsModule == null)
		{
			base.graphicsModule = new DropBugGraphics(this);
		}
		base.graphicsModule.Reset();
	}

	public override void RecreateSticksFromAbstract()
	{
		for (int i = 0; i < base.abstractCreature.stuckObjects.Count; i++)
		{
			if (base.abstractCreature.stuckObjects[i] is AbstractPhysicalObject.CreatureGripStick && base.abstractCreature.stuckObjects[i].A == base.abstractCreature)
			{
				Grab(base.abstractCreature.stuckObjects[i].B.realizedObject, 0, 0, Grasp.Shareability.CanNotShare, 0.5f, overrideEquallyDominant: true, pacifying: true);
				break;
			}
		}
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		if (placeRoom.game.IsStorySession && base.abstractCreature.timeSpentHere > UnityEngine.Random.Range(-10, 10))
		{
			List<IntVector2> list = new List<IntVector2>();
			if (!DropBugAI.ValidCeilingSpot(placeRoom, base.abstractCreature.pos.Tile) && placeRoom.ceilingTiles.Length != 0)
			{
				for (int i = 0; i < 100; i++)
				{
					IntVector2 intVector = placeRoom.ceilingTiles[UnityEngine.Random.Range(0, placeRoom.ceilingTiles.Length)];
					if (DropBugAI.ValidCeilingSpot(placeRoom, intVector))
					{
						list.Add(intVector);
						if (list.Count > 10)
						{
							break;
						}
					}
				}
			}
			AI.ceilingModule.NewRoom(placeRoom);
			if (list.Count > 0)
			{
				IntVector2 pos = list[UnityEngine.Random.Range(0, list.Count)];
				float num = float.MaxValue;
				for (int j = 0; j < list.Count; j++)
				{
					float num2 = DropBugAI.CeilingSpotScore(placeRoom, list[j]);
					num2 += Custom.LerpMap(list[j].FloatDist(AI.ceilingModule.stayAwayFromPos.Tile), 0f, 15f, 400f, 0f);
					num2 -= Custom.LerpMap(list[j].FloatDist(AI.ceilingModule.randomAttractor), AI.ceilingModule.roomSize / 4, AI.ceilingModule.roomSize / 2, 150f, 0f);
					for (int k = 0; k < placeRoom.abstractRoom.creatures.Count; k++)
					{
						if (placeRoom.abstractRoom.creatures[k].creatureTemplate.type == CreatureTemplate.Type.DropBug)
						{
							num2 += Custom.LerpMap(list[j].FloatDist(placeRoom.abstractRoom.creatures[k].pos.Tile), 50f, 2f, 0f, 500f, 3f);
						}
						num2 += Custom.LerpMap(list[j].FloatDist(placeRoom.abstractRoom.creatures[k].pos.Tile), 1f, 9f, 200f, 0f, 0.5f);
					}
					if (num2 < num)
					{
						pos = list[j];
						num = num2;
					}
				}
				base.abstractCreature.pos = placeRoom.GetWorldCoordinate(pos);
			}
		}
		base.PlaceInRoom(placeRoom);
		if (!DropBugAI.ValidCeilingSpot(placeRoom, base.abstractCreature.pos.Tile))
		{
			return;
		}
		inCeilingMode = 1f;
		AI.ceilingModule.ceilingPos = base.abstractCreature.pos;
		AI.ceilingModule.winStreak = 1000;
		AI.utilityComparer.GetUtilityTracker(AI.ceilingModule).smoothedUtility = 1f;
		AI.utilityComparer.GetUtilityTracker(AI.ceilingModule).weight = 1f;
		base.abstractCreature.abstractAI.SetDestination(base.abstractCreature.pos);
		footingCounter = 100;
		AI.behavior = DropBugAI.Behavior.SitInCeiling;
		for (int l = 0; l < base.abstractCreature.stuckObjects.Count; l++)
		{
			if (!(base.abstractCreature.stuckObjects[l] is AbstractPhysicalObject.CreatureGripStick) || base.abstractCreature.stuckObjects[l].A != base.abstractCreature)
			{
				continue;
			}
			AI.baitItem = base.abstractCreature.stuckObjects[l].B;
			WorldCoordinate worldCoordinate = placeRoom.GetWorldCoordinate(placeRoom.aimap.getAItile(base.abstractCreature.pos).fallRiskTile);
			base.abstractCreature.pos = worldCoordinate;
			AI.baitItem.pos = worldCoordinate;
			if (AI.baitItem.realizedObject != null)
			{
				for (int m = 0; m < AI.baitItem.realizedObject.bodyChunks.Length; m++)
				{
					AI.baitItem.realizedObject.bodyChunks[m].HardSetPosition(placeRoom.MiddleOfTile(worldCoordinate) + Custom.RNV());
				}
			}
			base.abstractCreature.stuckObjects[l].Deactivate();
			break;
		}
	}

	public override void Update(bool eu)
	{
		if (room.game.devToolsActive && Input.GetKey("b") && room.game.cameras[0].room == room)
		{
			base.bodyChunks[0].vel += Custom.DirVec(base.bodyChunks[0].pos, (Vector2)Futile.mousePosition + room.game.cameras[0].pos) * 14f;
			Stun(12);
		}
		if (!base.dead)
		{
			if (State.health < 0f && UnityEngine.Random.value < 0f - State.health && UnityEngine.Random.value < 1f / (float)(base.Consious ? 80 : 800))
			{
				Die();
			}
			if (UnityEngine.Random.value < 1f / 30f && (UnityEngine.Random.value * 0.2f > State.health || UnityEngine.Random.value < 0f - State.health))
			{
				Stun(UnityEngine.Random.Range(1, UnityEngine.Random.Range(1, 27 - Custom.IntClamp((int)(20f * State.health), 0, 10))));
			}
			if (State.health > 0f && State.health < 1f && UnityEngine.Random.value < 0.01f && poison < 0.1f)
			{
				State.health = Mathf.Min(1f, State.health + 1f / Mathf.Lerp(550f, 70f, State.health));
			}
			if (grabOnNextAttack > 0)
			{
				grabOnNextAttack--;
			}
			if (!base.Consious && UnityEngine.Random.value < 0.05f)
			{
				base.bodyChunks[1].vel += Custom.RNV() * UnityEngine.Random.value * 3f;
			}
			if (base.stun < 35 && grabbedBy.Count > 0 && !(grabbedBy[0].grabber is Vulture) && !(grabbedBy[0].grabber is Leech))
			{
				grabbedCounter++;
				if (grabbedCounter == 25 && UnityEngine.Random.value < 0.85f * State.health)
				{
					Slash(grabbedBy[0].grabber, null);
					if (grabbedBy.Count == 0)
					{
						base.stun = 0;
					}
				}
				else if (grabbedCounter < 25)
				{
					for (int i = 0; i < base.bodyChunks.Length; i++)
					{
						base.bodyChunks[i].pos += Custom.RNV() * UnityEngine.Random.value * 6f;
						base.bodyChunks[i].pos += Custom.RNV() * UnityEngine.Random.value * 6f;
					}
				}
			}
			else
			{
				grabbedCounter = 0;
			}
			if (releaseGrabbedCounter > 0)
			{
				releaseGrabbedCounter--;
				if (base.grasps[0] != null && base.grasps[0].grabbed is Creature && !(base.grasps[0].grabbed as Creature).dead && (base.grasps[0].grabbed as Creature).TotalMass > base.TotalMass * 0.3f)
				{
					Vector2 vector = Custom.RNV();
					base.mainBodyChunk.pos += vector * 4f;
					base.mainBodyChunk.vel += vector * 4f;
					base.grasps[0].grabbedChunk.pos += Vector2.ClampMagnitude(vector * 2f / base.grasps[0].grabbedChunk.mass, 5f);
					base.grasps[0].grabbedChunk.vel += Vector2.ClampMagnitude(vector * 2f / base.grasps[0].grabbedChunk.mass, 7f);
					if (releaseGrabbedCounter == 1)
					{
						Slash(base.grasps[0].grabbed as Creature, null);
						LoseAllGrasps();
					}
				}
				else
				{
					releaseGrabbedCounter = 0;
				}
			}
		}
		if (outOfWaterFooting > 0)
		{
			outOfWaterFooting--;
		}
		attemptBite = Mathf.Max(0f, attemptBite - 0.025f);
		if (fromCeilingJump)
		{
			attemptBite = 1f;
			for (int j = 0; j < base.bodyChunks.Length; j++)
			{
				if (base.bodyChunks[j].ContactPoint.y < 0 || base.bodyChunks[j].submersion > 0.5f)
				{
					afterDropAttackDelay = 20;
					fromCeilingJump = false;
					attemptBite = 0f;
					jumping = false;
					jumpAtChunk = null;
					charging = 0f;
					break;
				}
			}
		}
		base.Update(eu);
		dropAnticipation = Mathf.Max(dropAnticipation - 1f / 120f, 0f);
		if (afterDropAttackDelay > 0)
		{
			afterDropAttackDelay--;
		}
		base.mainBodyChunk.vel += Custom.DirVec(base.bodyChunks[2].pos, base.mainBodyChunk.pos) * 0.5f;
		base.bodyChunks[2].vel -= Custom.DirVec(base.bodyChunks[2].pos, base.mainBodyChunk.pos) * 1f;
		if (room == null)
		{
			return;
		}
		sitting = false;
		if (base.Consious)
		{
			if (room.aimap.TileAccessibleToCreature(base.bodyChunks[0].pos, base.Template) || room.aimap.TileAccessibleToCreature(base.bodyChunks[1].pos, base.Template))
			{
				footingCounter++;
			}
			Act();
		}
		else
		{
			footingCounter = 0;
			charging = 0f;
			jumping = false;
			fromCeilingJump = false;
			inCeilingMode = 0f;
		}
		if (Footing)
		{
			for (int k = 0; k < 2; k++)
			{
				base.bodyChunks[k].vel *= 0.8f;
				base.bodyChunks[k].vel.y += base.gravity;
			}
			if (MoveBackwards)
			{
				base.bodyChunks[2].vel *= 0.8f;
				base.bodyChunks[2].vel.y += base.gravity;
			}
			else
			{
				base.bodyChunks[2].vel.y += base.gravity * Mathf.Lerp(0.5f, 1f, AI.stuckTracker.Utility());
			}
		}
		travelDir *= (sitting ? 0.5f : 0.9995f);
		base.bodyChunks[2].collideWithTerrain = inCeilingMode < 0.5f;
		base.bodyChunks[1].collideWithTerrain = inCeilingMode < 0.5f;
		bodyChunkConnections[0].distance = Mathf.Lerp(12f, 5f, inCeilingMode);
		bodyChunkConnections[1].distance = Mathf.Lerp(14f, 2f, inCeilingMode);
		bodyChunkConnections[2].distance = Mathf.Lerp(8f, 0f, inCeilingMode);
		if (base.grasps[0] != null)
		{
			CarryObject(eu);
		}
		else
		{
			carryObjectMass = 0f;
		}
	}

	private void CarryObject(bool eu)
	{
		if (UnityEngine.Random.value < 0.025f && base.grasps[0].grabbed is Creature && AI.DynamicRelationship((base.grasps[0].grabbed as Creature).abstractCreature).type != CreatureTemplate.Relationship.Type.Eats && !base.safariControlled)
		{
			LoseAllGrasps();
			return;
		}
		if (!(base.grasps[0].grabbed is Creature))
		{
			if (!base.safariControlled && (AI.behavior == DropBugAI.Behavior.Hunt || AI.behavior == DropBugAI.Behavior.Flee || AI.behavior == DropBugAI.Behavior.Injured))
			{
				LoseAllGrasps();
				return;
			}
			if (base.grasps[0].grabbed is Weapon)
			{
				(base.grasps[0].grabbed as Weapon).setRotation = Custom.PerpendicularVector(base.mainBodyChunk.pos, base.grasps[0].grabbed.firstChunk.pos);
			}
		}
		PhysicalObject grabbed = base.grasps[0].grabbed;
		carryObjectMass = grabbed.bodyChunks[base.grasps[0].chunkGrabbed].owner.TotalMass;
		if (carryObjectMass <= base.TotalMass * 1.1f)
		{
			carryObjectMass /= 2f;
		}
		else if (carryObjectMass <= base.TotalMass / 5f)
		{
			carryObjectMass = 0f;
		}
		float num = base.mainBodyChunk.rad + base.grasps[0].grabbed.bodyChunks[base.grasps[0].chunkGrabbed].rad;
		Vector2 vector = -Custom.DirVec(base.mainBodyChunk.pos, grabbed.bodyChunks[base.grasps[0].chunkGrabbed].pos) * (num - Vector2.Distance(base.mainBodyChunk.pos, grabbed.bodyChunks[base.grasps[0].chunkGrabbed].pos));
		float num2 = grabbed.bodyChunks[base.grasps[0].chunkGrabbed].mass / (base.mainBodyChunk.mass + grabbed.bodyChunks[base.grasps[0].chunkGrabbed].mass);
		num2 *= 0.2f * (1f - AI.stuckTracker.Utility());
		base.mainBodyChunk.pos += vector * num2;
		base.mainBodyChunk.vel += vector * num2;
		grabbed.bodyChunks[base.grasps[0].chunkGrabbed].pos -= vector * (1f - num2);
		grabbed.bodyChunks[base.grasps[0].chunkGrabbed].vel -= vector * (1f - num2);
		Vector2 vector2 = base.mainBodyChunk.pos + Custom.DirVec(base.bodyChunks[1].pos, base.mainBodyChunk.pos) * num;
		Vector2 vector3 = grabbed.bodyChunks[base.grasps[0].chunkGrabbed].vel - base.mainBodyChunk.vel;
		grabbed.bodyChunks[base.grasps[0].chunkGrabbed].vel = base.mainBodyChunk.vel;
		if (!enteringShortCut.HasValue && (vector3.magnitude * grabbed.bodyChunks[base.grasps[0].chunkGrabbed].mass > 30f || !Custom.DistLess(vector2, grabbed.bodyChunks[base.grasps[0].chunkGrabbed].pos, 70f + grabbed.bodyChunks[base.grasps[0].chunkGrabbed].rad)))
		{
			LoseAllGrasps();
		}
		else
		{
			grabbed.bodyChunks[base.grasps[0].chunkGrabbed].MoveFromOutsideMyUpdate(eu, vector2);
		}
		if (base.grasps[0] != null)
		{
			for (int i = 0; i < 2; i++)
			{
				base.grasps[0].grabbed.PushOutOf(base.bodyChunks[i].pos, base.bodyChunks[i].rad, base.grasps[0].chunkGrabbed);
			}
		}
	}

	private void Swim()
	{
		base.bodyChunks[0].vel *= 1f - 0.1f * base.bodyChunks[0].submersion;
		base.bodyChunks[1].vel *= 1f - 0.2f * base.bodyChunks[1].submersion;
		runCycle += 0.125f;
		base.GoThroughFloors = true;
		MovementConnection movementConnection = (AI.pathFinder as StandardPather).FollowPath(room.GetWorldCoordinate(base.bodyChunks[0].pos), actuallyFollowingThisPath: true);
		if (movementConnection == default(MovementConnection))
		{
			movementConnection = (AI.pathFinder as StandardPather).FollowPath(room.GetWorldCoordinate(base.bodyChunks[1].pos), actuallyFollowingThisPath: true);
		}
		if (movementConnection == default(MovementConnection) && Math.Abs(base.abstractCreature.pos.y - room.defaultWaterLevel) < 4)
		{
			movementConnection = (AI.pathFinder as StandardPather).FollowPath(new WorldCoordinate(base.abstractCreature.pos.room, base.abstractCreature.pos.x, room.defaultWaterLevel, base.abstractCreature.pos.abstractNode), actuallyFollowingThisPath: true);
		}
		if (base.safariControlled && (movementConnection == default(MovementConnection) || !AllowableControlledAIOverride(movementConnection.type)))
		{
			movementConnection = default(MovementConnection);
			if (inputWithoutDiagonals.HasValue)
			{
				MovementConnection.MovementType type = MovementConnection.MovementType.Standard;
				if (room.GetTile(base.mainBodyChunk.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
				{
					type = MovementConnection.MovementType.ShortCut;
				}
				if (inputWithoutDiagonals.Value.AnyDirectionalInput)
				{
					movementConnection = new MovementConnection(type, room.GetWorldCoordinate(base.mainBodyChunk.pos), room.GetWorldCoordinate(base.mainBodyChunk.pos + new Vector2(inputWithoutDiagonals.Value.x, inputWithoutDiagonals.Value.y) * 40f), 2);
				}
			}
		}
		if (movementConnection != default(MovementConnection))
		{
			if (movementConnection.StartTile.y == movementConnection.DestTile.y && movementConnection.DestTile.y == room.defaultWaterLevel)
			{
				base.mainBodyChunk.vel.x -= Mathf.Sign(room.MiddleOfTile(movementConnection.StartTile).x - room.MiddleOfTile(movementConnection.DestTile).x) * 1.2f * base.bodyChunks[0].submersion;
				base.bodyChunks[1].vel.x += Mathf.Sign(room.MiddleOfTile(movementConnection.StartTile).x - room.MiddleOfTile(movementConnection.DestTile).x) * 0.5f * base.bodyChunks[1].submersion;
				footingCounter = 0;
				return;
			}
			base.bodyChunks[0].vel *= 0.8f;
			base.mainBodyChunk.vel += Custom.DirVec(base.mainBodyChunk.pos, room.MiddleOfTile(movementConnection.destinationCoord)) * 1.4f;
			if (!base.safariControlled || base.Submersion < 0.5f)
			{
				footingCounter = Math.Max(footingCounter, 25);
				Run(movementConnection);
				outOfWaterFooting = 20;
			}
			else
			{
				base.mainBodyChunk.vel *= 0.75f;
				footingCounter = 0;
				Run(movementConnection);
				outOfWaterFooting = 0;
			}
		}
		else
		{
			base.mainBodyChunk.vel.y += 0.5f;
		}
	}

	private void Act()
	{
		AI.Update();
		if (base.Submersion > 0.3f)
		{
			Swim();
			swimming = true;
			return;
		}
		swimming = false;
		if (UnityEngine.Random.value < 0.005f)
		{
			walkBackwardsDist = UnityEngine.Random.value * 20f;
		}
		if (AI.ceilingModule.SittingInCeiling)
		{
			inCeilingMode = Mathf.Min(1f, inCeilingMode + 0.025f);
			Vector2 vector = room.MiddleOfTile(AI.ceilingModule.ceilingPos);
			base.bodyChunks[0].pos = Vector2.Lerp(base.bodyChunks[0].pos, vector + new Vector2(0f, -5f * inCeilingMode), 0.05f * inCeilingMode);
			base.bodyChunks[1].pos = Vector2.Lerp(base.bodyChunks[1].pos, vector + new Vector2(0f, 9f * inCeilingMode), 0.4f * inCeilingMode);
			base.bodyChunks[2].pos = Vector2.Lerp(base.bodyChunks[2].pos, vector + new Vector2(0f, 10f * inCeilingMode), 0.5f * inCeilingMode);
			for (int i = 0; i < base.bodyChunks.Length; i++)
			{
				base.bodyChunks[i].vel *= 1f - inCeilingMode;
			}
			footingCounter = 20;
			if (!base.safariControlled && UnityEngine.Random.value < 0.0125f && inCeilingMode >= 1f && base.grasps[0] != null && !(base.grasps[0].grabbed is Creature))
			{
				AI.baitItem = base.grasps[0].grabbed.abstractPhysicalObject;
				ReleaseGrasp(0);
			}
			sitting = true;
			if (base.safariControlled && inputWithoutDiagonals.HasValue && inputWithoutDiagonals.Value.jmp && !lastInputWithoutDiagonals.Value.jmp)
			{
				AI.ceilingModule.Dislodge();
			}
			if (base.safariControlled && inputWithoutDiagonals.HasValue && inputWithoutDiagonals.Value.thrw && !lastInputWithoutDiagonals.Value.thrw)
			{
				LoseAllGrasps();
			}
			if (ModManager.MSC && base.LickedByPlayer != null && luredToDropCounter == 0)
			{
				luredToDropCounter = 25;
			}
			if (luredToDropCounter > 0 && !base.safariControlled)
			{
				luredToDropCounter--;
				dropAnticipation = Mathf.Lerp(dropAnticipation, UnityEngine.Random.value, 0.2f);
				if (luredToDropCounter < 1)
				{
					AI.ceilingModule.Dislodge();
				}
			}
			return;
		}
		inCeilingMode = 0f;
		luredToDropCounter = 0;
		if (jumping)
		{
			bool flag = false;
			for (int j = 0; j < base.bodyChunks.Length; j++)
			{
				if ((base.bodyChunks[j].ContactPoint.x != 0 || base.bodyChunks[j].ContactPoint.y != 0) && room.aimap.TileAccessibleToCreature(base.bodyChunks[j].pos, base.Template))
				{
					flag = true;
				}
			}
			if (flag)
			{
				footingCounter++;
			}
			else
			{
				footingCounter = 0;
			}
			if (jumpAtChunk != null && room.VisualContact(base.mainBodyChunk.pos, jumpAtChunk.pos))
			{
				base.bodyChunks[0].vel += Custom.DirVec(base.bodyChunks[0].pos, jumpAtChunk.pos) * 1.2f;
				base.bodyChunks[1].vel -= Custom.DirVec(base.bodyChunks[0].pos, jumpAtChunk.pos) * 0.4f;
				base.bodyChunks[2].vel -= Custom.DirVec(base.bodyChunks[0].pos, jumpAtChunk.pos) * 0.4f;
				if (fromCeilingJump && base.bodyChunks[0].pos.y > jumpAtChunk.pos.y + 250f && Custom.DistLess(base.bodyChunks[0].pos, jumpAtChunk.pos, 350f))
				{
					base.bodyChunks[0].vel.x += Custom.DirVec(base.bodyChunks[0].pos, jumpAtChunk.pos + jumpAtChunk.vel).x * 3f;
				}
			}
			else if (jumpAtPos != Vector2.zero)
			{
				base.bodyChunks[0].vel += Custom.DirVec(base.bodyChunks[0].pos, jumpAtPos) * 1.2f;
				base.bodyChunks[1].vel -= Custom.DirVec(base.bodyChunks[0].pos, jumpAtPos) * 0.4f;
				base.bodyChunks[2].vel -= Custom.DirVec(base.bodyChunks[0].pos, jumpAtPos) * 0.4f;
				if (fromCeilingJump && base.bodyChunks[0].pos.y > jumpAtPos.y + 250f && Custom.DistLess(base.bodyChunks[0].pos, jumpAtPos, 350f))
				{
					BodyChunk bodyChunk = base.bodyChunks[0];
					bodyChunk.vel.x = bodyChunk.vel.x + Custom.DirVec(base.bodyChunks[0].pos, jumpAtPos).x * 3f;
				}
			}
			if (Footing)
			{
				jumping = false;
				jumpAtChunk = null;
			}
			return;
		}
		if (AI.stuckTracker.Utility() > 0.9f)
		{
			stuckShake = Custom.LerpAndTick(stuckShake, 1f, 0.07f, 1f / 70f);
		}
		else if (AI.stuckTracker.Utility() < 0.2f)
		{
			stuckShake = Custom.LerpAndTick(stuckShake, 0f, 0.07f, 0.05f);
		}
		if (stuckShake > 0f && (!base.safariControlled || (inputWithoutDiagonals.HasValue && inputWithoutDiagonals.Value.AnyDirectionalInput)))
		{
			for (int k = 0; k < base.bodyChunks.Length; k++)
			{
				base.bodyChunks[k].vel += Custom.RNV() * UnityEngine.Random.value * 5f * stuckShake;
				base.bodyChunks[k].pos += Custom.RNV() * UnityEngine.Random.value * 5f * stuckShake;
			}
		}
		if (specialMoveCounter > 0)
		{
			specialMoveCounter--;
			MoveTowards(room.MiddleOfTile(specialMoveDestination));
			travelDir = Vector2.Lerp(travelDir, Custom.DirVec(base.mainBodyChunk.pos, room.MiddleOfTile(specialMoveDestination)), 0.4f);
			if (Custom.DistLess(base.mainBodyChunk.pos, room.MiddleOfTile(specialMoveDestination), 5f))
			{
				specialMoveCounter = 0;
			}
		}
		else
		{
			if (!room.aimap.TileAccessibleToCreature(base.mainBodyChunk.pos, base.Template) && !room.aimap.TileAccessibleToCreature(base.bodyChunks[1].pos, base.Template))
			{
				footingCounter = Custom.IntClamp(footingCounter - 3, 0, 35);
			}
			if (Footing && charging > 0f)
			{
				sitting = true;
				base.GoThroughFloors = false;
				charging += 1f / 15f;
				Vector2? vector2 = null;
				if (jumpAtPos != Vector2.zero)
				{
					vector2 = Custom.DirVec(base.mainBodyChunk.pos, jumpAtPos);
				}
				if (jumpAtChunk != null)
				{
					vector2 = Custom.DirVec(base.mainBodyChunk.pos, jumpAtChunk.pos);
				}
				if (vector2.HasValue)
				{
					base.bodyChunks[0].vel += vector2.Value * Mathf.Pow(charging, 2f);
					base.bodyChunks[1].vel -= vector2.Value * 4f * charging;
				}
				if (charging >= 1f)
				{
					Attack();
				}
			}
			else if ((room.GetWorldCoordinate(base.mainBodyChunk.pos) == AI.pathFinder.GetDestination || room.GetWorldCoordinate(base.bodyChunks[1].pos) == AI.pathFinder.GetDestination) && AI.threatTracker.Utility() < 0.5f && !base.safariControlled)
			{
				sitting = true;
				base.GoThroughFloors = false;
			}
			else
			{
				MovementConnection movementConnection = (AI.pathFinder as StandardPather).FollowPath(room.GetWorldCoordinate(base.bodyChunks[MoveBackwards ? 2 : 0].pos), actuallyFollowingThisPath: true);
				if (movementConnection == default(MovementConnection))
				{
					movementConnection = (AI.pathFinder as StandardPather).FollowPath(room.GetWorldCoordinate(base.bodyChunks[(!MoveBackwards) ? 2 : 0].pos), actuallyFollowingThisPath: true);
				}
				if (movementConnection == default(MovementConnection))
				{
					movementConnection = (AI.pathFinder as StandardPather).FollowPath(room.GetWorldCoordinate(base.bodyChunks[1].pos), actuallyFollowingThisPath: true);
				}
				if (base.safariControlled && (movementConnection == default(MovementConnection) || !AllowableControlledAIOverride(movementConnection.type)))
				{
					movementConnection = default(MovementConnection);
					if (inputWithoutDiagonals.HasValue)
					{
						MovementConnection.MovementType type = MovementConnection.MovementType.Standard;
						if (room.GetTile(base.mainBodyChunk.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
						{
							type = MovementConnection.MovementType.ShortCut;
						}
						if (inputWithoutDiagonals.Value.AnyDirectionalInput && (Footing || base.mainBodyChunk.submersion != 0f))
						{
							movementConnection = new MovementConnection(type, room.GetWorldCoordinate(base.mainBodyChunk.pos), room.GetWorldCoordinate(base.mainBodyChunk.pos + new Vector2(inputWithoutDiagonals.Value.x, inputWithoutDiagonals.Value.y) * 40f), 2);
						}
						if (inputWithoutDiagonals.Value.thrw && !inputWithoutDiagonals.Value.thrw)
						{
							LoseAllGrasps();
						}
						if (inputWithoutDiagonals.Value.y < 0)
						{
							base.GoThroughFloors = true;
						}
						else
						{
							base.GoThroughFloors = false;
						}
					}
					if (inputWithDiagonals.HasValue && inputWithDiagonals.Value.jmp && !lastInputWithDiagonals.Value.jmp)
					{
						if (inputWithDiagonals.Value.AnyDirectionalInput)
						{
							InitiateJump(base.mainBodyChunk.pos + new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y) * 40f);
						}
						else
						{
							InitiateJump(base.mainBodyChunk.pos + travelDir * 40f);
						}
					}
				}
				if (movementConnection != default(MovementConnection))
				{
					Run(movementConnection);
				}
				else
				{
					base.GoThroughFloors = false;
				}
			}
		}
		float num = runCycle;
		if (base.Consious && !Custom.DistLess(base.mainBodyChunk.pos, base.mainBodyChunk.lastPos, 2f))
		{
			runCycle += 0.125f;
		}
		if (num < Mathf.Floor(runCycle))
		{
			room.PlaySound(SoundID.Drop_Bug_Step, base.mainBodyChunk);
		}
	}

	public override void Stun(int st)
	{
		base.Stun(st);
		if (st > 4 && UnityEngine.Random.value < 0.5f)
		{
			LoseAllGrasps();
		}
	}

	public override void NewTile()
	{
		base.NewTile();
		if ((base.safariControlled && (!inputWithoutDiagonals.HasValue || !inputWithoutDiagonals.Value.pckp)) || !base.Consious || base.grasps[0] != null || AI.behavior == DropBugAI.Behavior.Hunt || AI.behavior == DropBugAI.Behavior.Flee || AI.behavior == DropBugAI.Behavior.Injured)
		{
			return;
		}
		for (int i = 0; i < AI.itemTracker.ItemCount; i++)
		{
			if (AI.itemTracker.GetRep(i).representedItem.realizedObject != null && Custom.DistLess(AI.itemTracker.GetRep(i).representedItem.realizedObject.firstChunk.pos, base.mainBodyChunk.pos, 40f) && AI.itemTracker.GetRep(i).representedItem.stuckObjects.Count < 1 && (!(AI.itemTracker.GetRep(i).representedItem.realizedObject is Weapon) || (AI.itemTracker.GetRep(i).representedItem.realizedObject as Weapon).mode == Weapon.Mode.Free))
			{
				Grab(AI.itemTracker.GetRep(i).representedItem.realizedObject, 0, 0, Grasp.Shareability.CanNotShare, 0.5f, overrideEquallyDominant: true, pacifying: true);
			}
		}
	}

	public void InitiateJump(BodyChunk target)
	{
		if (!(charging > 0f) && !jumping && base.grasps[0] == null && afterDropAttackDelay <= 0)
		{
			charging = 0.01f;
			jumpAtChunk = target;
			jumpAtPos = Vector2.zero;
			room.PlaySound(SoundID.Drop_Bug_Prepare_Jump, base.mainBodyChunk);
		}
	}

	public void InitiateJump(Vector2 target)
	{
		if (!(charging > 0f) && !jumping && base.grasps[0] == null && afterDropAttackDelay <= 0)
		{
			charging = 0.01f;
			jumpAtChunk = null;
			jumpAtPos = target;
			room.PlaySound(SoundID.Drop_Bug_Prepare_Jump, base.mainBodyChunk);
		}
	}

	public void JumpFromCeiling(BodyChunk target, Vector2 dir)
	{
		inCeilingMode = 0f;
		fromCeilingJump = true;
		Jump(dir);
		jumpAtChunk = target;
		if (jumpAtChunk == null)
		{
			jumpAtPos = dir;
		}
		if (base.graphicsModule != null)
		{
			(base.graphicsModule as DropBugGraphics).ceilingJump = true;
		}
		room.PlaySound(SoundID.Drop_Bug_Drop_From_Ceiling, base.mainBodyChunk);
		if (voiceSound == null || voiceSound.slatedForDeletetion)
		{
			voiceSound = room.PlaySound(SoundID.Drop_Bug_Voice, base.mainBodyChunk);
		}
	}

	public void AnticipateDrop()
	{
		if (dropAnticipation > 0f)
		{
			return;
		}
		dropAnticipation = 1f;
		base.mainBodyChunk.pos += Custom.RNV() * 2f;
		if (base.graphicsModule != null)
		{
			for (int i = 0; i < 2; i++)
			{
				(base.graphicsModule as DropBugGraphics).antennae[i].pos += Custom.RNV() * 7f * UnityEngine.Random.value;
			}
		}
	}

	private void Attack()
	{
		if (base.grasps[0] != null || (jumpAtChunk == null && jumpAtPos == Vector2.zero) || (jumpAtChunk != null && (jumpAtChunk.owner.room != room || !room.VisualContact(base.mainBodyChunk.pos, jumpAtChunk.pos))))
		{
			charging = 0f;
			jumpAtChunk = null;
			return;
		}
		Vector2? vector = null;
		if (jumpAtPos != Vector2.zero)
		{
			vector = jumpAtPos;
		}
		if (jumpAtChunk != null)
		{
			vector = jumpAtChunk.pos;
		}
		if (!vector.HasValue)
		{
			return;
		}
		Vector2 p = new Vector2(vector.Value.x, vector.Value.y);
		if (!room.GetTile(vector.Value + new Vector2(0f, 20f)).Solid)
		{
			vector += new Vector2(0f, Mathf.InverseLerp(40f, 200f, Vector2.Distance(base.mainBodyChunk.pos, vector.Value)) * 20f);
		}
		Vector2 vector2 = Custom.DirVec(base.mainBodyChunk.pos, vector.Value);
		if (!Custom.DistLess(base.mainBodyChunk.pos, p, Custom.LerpMap(Vector2.Dot(vector2, Custom.DirVec(base.bodyChunks[1].pos, base.bodyChunks[0].pos)), -0.1f, 0.8f, 0f, 300f, 0.4f)))
		{
			charging = 0f;
			jumpAtChunk = null;
			return;
		}
		if (!room.GetTile(base.mainBodyChunk.pos + new Vector2(0f, 20f)).Solid && !room.GetTile(base.bodyChunks[1].pos + new Vector2(0f, 20f)).Solid)
		{
			vector2 = Vector3.Slerp(vector2, new Vector2(0f, 1f), Custom.LerpMap(Vector2.Distance(base.mainBodyChunk.pos, vector.Value), 40f, 200f, 0.05f, 0.2f));
		}
		room.PlaySound(SoundID.Drop_Bug_Jump, base.mainBodyChunk);
		if (voiceSound == null || voiceSound.slatedForDeletetion)
		{
			voiceSound = room.PlaySound(SoundID.Drop_Bug_Voice, base.mainBodyChunk);
		}
		Jump(vector2);
	}

	private void Jump(Vector2 jumpDir)
	{
		float num = Custom.LerpMap(jumpDir.y, -1f, 1f, 0.7f, 1.2f, 1.1f);
		footingCounter = 0;
		base.mainBodyChunk.vel *= 0.5f;
		base.bodyChunks[1].vel *= 0.5f;
		base.mainBodyChunk.vel += jumpDir * 21f * num;
		base.bodyChunks[1].vel += jumpDir * 16f * num;
		attemptBite = 1f;
		charging = 0f;
		jumping = true;
	}

	private void Run(MovementConnection followingConnection)
	{
		if (followingConnection.type == MovementConnection.MovementType.ReachUp)
		{
			(AI.pathFinder as StandardPather).pastConnections.Clear();
		}
		if (followingConnection.type == MovementConnection.MovementType.ShortCut || followingConnection.type == MovementConnection.MovementType.NPCTransportation)
		{
			enteringShortCut = followingConnection.StartTile;
			if (base.safariControlled)
			{
				bool flag = false;
				List<IntVector2> list = new List<IntVector2>();
				ShortcutData[] shortcuts = room.shortcuts;
				for (int i = 0; i < shortcuts.Length; i++)
				{
					ShortcutData shortcutData = shortcuts[i];
					if (shortcutData.shortCutType == ShortcutData.Type.NPCTransportation && shortcutData.StartTile != followingConnection.StartTile)
					{
						list.Add(shortcutData.StartTile);
					}
					if (shortcutData.shortCutType == ShortcutData.Type.NPCTransportation && shortcutData.StartTile == followingConnection.StartTile)
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
						NPCTransportationDestination = followingConnection.destinationCoord;
					}
				}
			}
			else if (followingConnection.type == MovementConnection.MovementType.NPCTransportation)
			{
				NPCTransportationDestination = followingConnection.destinationCoord;
			}
		}
		else if (followingConnection.type == MovementConnection.MovementType.OpenDiagonal || followingConnection.type == MovementConnection.MovementType.ReachOverGap || followingConnection.type == MovementConnection.MovementType.ReachUp || followingConnection.type == MovementConnection.MovementType.ReachDown || followingConnection.type == MovementConnection.MovementType.SemiDiagonalReach)
		{
			specialMoveCounter = 30;
			specialMoveDestination = followingConnection.DestTile;
		}
		else
		{
			MovementConnection movementConnection = followingConnection;
			if (AI.stuckTracker.Utility() == 0f)
			{
				MovementConnection movementConnection2 = (AI.pathFinder as StandardPather).FollowPath(movementConnection.destinationCoord, actuallyFollowingThisPath: false);
				if (movementConnection2.destinationCoord == followingConnection.startCoord)
				{
					sitting = true;
					return;
				}
				if (movementConnection2.destinationCoord.TileDefined && room.aimap.getAItile(movementConnection2.DestTile).acc < AItile.Accessibility.Ceiling)
				{
					bool flag2 = false;
					for (int j = Math.Min(followingConnection.StartTile.x, movementConnection2.DestTile.x); j < Math.Max(followingConnection.StartTile.x, movementConnection2.DestTile.x); j++)
					{
						if (flag2)
						{
							break;
						}
						for (int k = Math.Min(followingConnection.StartTile.y, movementConnection2.DestTile.y); k < Math.Max(followingConnection.StartTile.y, movementConnection2.DestTile.y); k++)
						{
							if (!room.aimap.TileAccessibleToCreature(j, k, base.Template))
							{
								flag2 = true;
								break;
							}
						}
					}
					if (!flag2)
					{
						movementConnection = movementConnection2;
					}
				}
			}
			Vector2 vector = room.MiddleOfTile(movementConnection.DestTile);
			travelDir = Vector2.Lerp(travelDir, Custom.DirVec(base.bodyChunks[MoveBackwards ? 2 : 0].pos, vector), 0.4f);
			if (lastFollowedConnection != default(MovementConnection) && lastFollowedConnection.type == MovementConnection.MovementType.ReachUp)
			{
				base.bodyChunks[MoveBackwards ? 2 : 0].vel += Custom.DirVec(base.bodyChunks[MoveBackwards ? 2 : 0].pos, vector) * 4f;
			}
			if (Footing)
			{
				for (int l = 0; l < base.bodyChunks.Length; l++)
				{
					if (followingConnection.startCoord.x == followingConnection.destinationCoord.x)
					{
						base.bodyChunks[l].vel.x += Mathf.Min((vector.x - base.bodyChunks[l].pos.x) / 8f, 1.2f);
					}
					else if (followingConnection.startCoord.y == followingConnection.destinationCoord.y)
					{
						base.bodyChunks[l].vel.y += Mathf.Min((vector.y - base.bodyChunks[l].pos.y) / 8f, 1.2f);
					}
				}
			}
			if (lastFollowedConnection != default(MovementConnection) && (Footing || room.aimap.TileAccessibleToCreature(base.bodyChunks[MoveBackwards ? 2 : 0].pos, base.Template)) && ((followingConnection.startCoord.x != followingConnection.destinationCoord.x && lastFollowedConnection.startCoord.x == lastFollowedConnection.destinationCoord.x) || (followingConnection.startCoord.y != followingConnection.destinationCoord.y && lastFollowedConnection.startCoord.y == lastFollowedConnection.destinationCoord.y)))
			{
				base.bodyChunks[MoveBackwards ? 2 : 0].vel *= 0.7f;
				base.bodyChunks[(!MoveBackwards) ? 2 : 0].vel *= 0.5f;
			}
			if (followingConnection.type == MovementConnection.MovementType.DropToFloor)
			{
				footingCounter = 0;
			}
			MoveTowards(vector);
		}
		lastFollowedConnection = followingConnection;
	}

	private void MoveTowards(Vector2 moveTo)
	{
		if (UnityEngine.Random.value > State.health)
		{
			return;
		}
		Vector2 vector = Custom.DirVec(base.mainBodyChunk.pos, moveTo);
		if (!Footing)
		{
			vector *= 0.3f;
		}
		if (!MoveBackwards && IsTileSolid(1, 0, -1) && (((double)vector.x < -0.5 && base.mainBodyChunk.pos.x > base.bodyChunks[1].pos.x + 5f) || ((double)vector.x > 0.5 && base.mainBodyChunk.pos.x < base.bodyChunks[1].pos.x - 5f)))
		{
			base.mainBodyChunk.vel.x -= ((vector.x < 0f) ? (-1f) : 1f) * 1.3f;
			base.bodyChunks[1].vel.x += ((vector.x < 0f) ? (-1f) : 1f) * 0.5f;
			if (!IsTileSolid(0, 0, 1))
			{
				base.mainBodyChunk.vel.y += 3.2f;
			}
		}
		float num = Custom.LerpMap(carryObjectMass, 0f, 4f, 1f, 0.2f, 0.7f) * Mathf.Lerp(1f, 1.5f, stuckShake);
		if (MoveBackwards)
		{
			base.bodyChunks[2].vel += vector * 7.5f * num;
			base.bodyChunks[1].vel += vector * 0.2f * num;
			base.mainBodyChunk.vel -= vector * 0.45f * num;
			base.GoThroughFloors = moveTo.y < base.bodyChunks[2].pos.y - 5f;
		}
		else
		{
			base.mainBodyChunk.vel += vector * 4.5f * num;
			base.bodyChunks[1].vel -= vector * 0.45f * num;
			base.bodyChunks[2].vel -= vector * 0.2f * num;
			base.GoThroughFloors = moveTo.y < base.mainBodyChunk.pos.y - 5f;
		}
	}

	public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
	{
		base.Collide(otherObject, myChunk, otherChunk);
		if (otherObject is DropBug)
		{
			AI.CollideWithKin(otherObject as DropBug);
			if (base.bodyChunks[myChunk].pos.y > otherObject.bodyChunks[otherChunk].pos.y)
			{
				base.bodyChunks[myChunk].vel.y += 2f;
				otherObject.bodyChunks[otherChunk].vel.y -= 2f;
			}
		}
		if (!(otherObject is Creature) || !base.Consious)
		{
			return;
		}
		AI.tracker.SeeCreature((otherObject as Creature).abstractCreature);
		bool flag = myChunk == 0 && base.grasps[0] == null && attemptBite > 0f && (jumping || fromCeilingJump || attemptBite > 0.75f) && ((fromCeilingJump && base.bodyChunks[2].pos.y > otherObject.bodyChunks[otherChunk].pos.y) || Vector2.Dot(Custom.DirVec(base.bodyChunks[1].pos, base.mainBodyChunk.pos), Custom.DirVec(base.mainBodyChunk.pos, otherObject.bodyChunks[otherChunk].pos)) > Mathf.Lerp(0.7f, -0.2f, attemptBite)) && AI.DynamicRelationship((otherObject as Creature).abstractCreature).type == CreatureTemplate.Relationship.Type.Eats;
		if (base.safariControlled)
		{
			flag = myChunk == 0 && base.grasps[0] == null && (jumping || fromCeilingJump || (inputWithoutDiagonals.HasValue && inputWithoutDiagonals.Value.pckp)) && AI.DynamicRelationship((otherObject as Creature).abstractCreature).type == CreatureTemplate.Relationship.Type.Eats;
		}
		if (!flag)
		{
			return;
		}
		if (jumping)
		{
			base.mainBodyChunk.vel *= 0.5f;
		}
		for (int i = 0; i < 4; i++)
		{
			room.AddObject(new WaterDrip(Vector2.Lerp(base.mainBodyChunk.pos, otherObject.bodyChunks[otherChunk].pos, UnityEngine.Random.value), Custom.RNV() * UnityEngine.Random.value * 14f, waterColor: false));
		}
		if (grabOnNextAttack > 0 || (otherObject as Creature).dead || UnityEngine.Random.value > (fromCeilingJump ? 0.8f : 0.2f) || (base.safariControlled && inputWithoutDiagonals.HasValue && inputWithoutDiagonals.Value.pckp))
		{
			if (Grab(otherObject, 0, otherChunk, Grasp.Shareability.CanNotShare, 0.5f, overrideEquallyDominant: false, pacifying: true))
			{
				(otherObject as Creature).Violence(base.mainBodyChunk, Custom.DirVec(base.mainBodyChunk.pos, otherObject.bodyChunks[otherChunk].pos) * 4f, otherObject.bodyChunks[otherChunk], null, DamageType.Bite, (fromCeilingJump && UnityEngine.Random.value < 0.2f) ? 1.1f : 0.4f, 0f);
				Custom.Log("drop bug grab");
				room.PlaySound(SoundID.Drop_Bug_Grab_Creature, base.mainBodyChunk);
				if (grabOnNextAttack < 1)
				{
					releaseGrabbedCounter = 35;
				}
				grabOnNextAttack = 0;
			}
			else
			{
				Slash(otherObject as Creature, otherObject.bodyChunks[otherChunk]);
			}
		}
		else
		{
			Slash(otherObject as Creature, otherObject.bodyChunks[otherChunk]);
		}
		attemptBite = 0f;
		charging = 0f;
		jumping = false;
		fromCeilingJump = false;
	}

	public void Slash(Creature creature, BodyChunk chunk)
	{
		if (chunk == null)
		{
			chunk = creature.mainBodyChunk;
			float dst = float.MaxValue;
			for (int i = 0; i < creature.bodyChunks.Length; i++)
			{
				if (Custom.DistLess(base.mainBodyChunk.pos, creature.bodyChunks[i].pos, dst))
				{
					dst = Vector2.Distance(base.mainBodyChunk.pos, creature.bodyChunks[i].pos);
					chunk = creature.bodyChunks[i];
				}
			}
		}
		bool flag = UnityEngine.Random.value < 1f / 3f;
		bool flag2 = UnityEngine.Random.value < 0.2f;
		creature.Violence(base.mainBodyChunk, Custom.DirVec(base.mainBodyChunk.pos, chunk.pos) * 8f, chunk, null, DamageType.Bite, flag2 ? 1.1f : 0.4f, flag ? 50f : 15f);
		base.mainBodyChunk.vel = Custom.DirVec(chunk.pos, base.mainBodyChunk.pos) * 8f;
		grabOnNextAttack = 180;
		for (int j = 0; j < 5; j++)
		{
			room.AddObject(new WaterDrip(Vector2.Lerp(base.mainBodyChunk.pos, chunk.pos, UnityEngine.Random.value), Custom.RNV() * UnityEngine.Random.value * (flag2 ? 24f : 14f), waterColor: false));
		}
		if (AI.DynamicRelationship(creature.abstractCreature).type == CreatureTemplate.Relationship.Type.Eats)
		{
			if (flag || flag2 || creature.dead)
			{
				AI.attackCounter = Math.Max(70, AI.attackCounter);
			}
			AI.targetCreature = creature.abstractCreature;
		}
		room.PlaySound(SoundID.Drop_Bug_Grab_Creature, base.mainBodyChunk);
	}

	public override void Die()
	{
		base.Die();
		LoseAllGrasps();
	}

	public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos hitAppendage, DamageType type, float damage, float stunBonus)
	{
		damage = ((hitChunk.index != 0) ? (damage * Mathf.Lerp(0.9f, 1.1f, UnityEngine.Random.value)) : (damage * Mathf.Lerp(0.975f, 1.25f, UnityEngine.Random.value)));
		if (!base.dead && (damage > 0.1f || stunBonus > 20f) && UnityEngine.Random.value < Custom.LerpMap(damage, 0.3f, 1f, 0.4f, 0.95f) && (voiceSound == null || voiceSound.slatedForDeletetion))
		{
			voiceSound = room.PlaySound(SoundID.Drop_Bug_Voice, base.mainBodyChunk);
		}
		base.Violence(source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
		if (UnityEngine.Random.value < 0.5f && State.health < 0f)
		{
			Die();
		}
	}

	public override void SpitOutOfShortCut(IntVector2 pos, Room newRoom, bool spitOutAllSticks)
	{
		base.SpitOutOfShortCut(pos, newRoom, spitOutAllSticks);
		Vector2 vector = Custom.IntVector2ToVector2(newRoom.ShorcutEntranceHoleDirection(pos));
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			base.bodyChunks[i].pos = newRoom.MiddleOfTile(pos) - vector * (-1.5f + (float)i) * 15f;
			base.bodyChunks[i].lastPos = newRoom.MiddleOfTile(pos);
			base.bodyChunks[i].vel = vector * 2f;
		}
		if (base.graphicsModule != null)
		{
			base.graphicsModule.Reset();
		}
	}

	public void FlyingWeapon(Weapon weapon)
	{
		if (!base.safariControlled && base.Consious && AI.ceilingModule.SittingInCeiling && Mathf.Abs(weapon.firstChunk.pos.x - weapon.thrownPos.x) < 200f && Custom.DistLess(weapon.firstChunk.pos + weapon.firstChunk.vel.normalized * 100f, base.mainBodyChunk.pos, 120f))
		{
			if (Mathf.Abs(Custom.DistanceToLine(base.bodyChunks[0].pos, weapon.firstChunk.pos, weapon.firstChunk.pos + weapon.firstChunk.vel)) < 15f && AI.VisualContact(weapon.firstChunk.pos, 1f))
			{
				AI.ceilingModule.Dislodge();
			}
			else if (weapon.firstChunk.pos.y < base.bodyChunks[0].pos.y - 10f && Mathf.Abs(Custom.DistanceToLine(base.bodyChunks[0].pos, weapon.firstChunk.pos, weapon.firstChunk.pos + weapon.firstChunk.vel)) < 35f)
			{
				luredToDropCounter = 30;
			}
		}
	}
}
