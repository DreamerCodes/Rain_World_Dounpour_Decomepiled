using System;
using System.Collections.Generic;
using System.Globalization;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class PoleMimic : Creature, IClimbableVine, PhysicalObject.IHaveAppendages
{
	private IntVector2 shortCutPos;

	public Vector2 rootPos;

	public Vector2 tipPos;

	public Tentacle tentacle;

	public Vector2 stickOutDir;

	private float length;

	private bool wantToWakeUp;

	private int wakeUpCounter;

	public bool tipAttached;

	public float mimic;

	public IntVector2[] tilePositions;

	private bool freeStanding;

	public float rad = 2f;

	public float extended;

	public float forceIntoShortCut;

	public float getToGoalForce;

	public int mimicDelayCounter;

	public int huntCounter;

	public BodyChunk huntChunk;

	public BodyChunk[] stickChunks;

	public int angeredAndAggressive;

	public InputCircularRegion controlRegion;

	private bool justOutOfShortCut;

	public override Vector2 VisionPoint => tentacle.tChunks[UnityEngine.Random.Range(0, tentacle.tChunks.Length)].pos;

	public float WakeUp => Mathf.InverseLerp(10f, 80f, wakeUpCounter);

	public float AnyWakeUp => Mathf.InverseLerp(0f, 40f, wakeUpCounter);

	public override float VisibilityBonus => mimic * -0.8f;

	public bool DontSpawnInPoleMode => justOutOfShortCut;

	private bool ChunkInPosition(int chunk)
	{
		if (tipAttached && Custom.DistLess(tentacle.tChunks[chunk].pos, room.MiddleOfTile(tilePositions[chunk]), 5f))
		{
			return tentacle.tChunks[chunk].vel.magnitude < Mathf.Lerp(2f, 8f, mimic);
		}
		return false;
	}

	public PoleMimic(AbstractCreature abstractCreature, World world)
		: base(abstractCreature, world)
	{
		base.bodyChunks = new BodyChunk[2];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), rad, 0.2f);
		base.bodyChunks[1] = new BodyChunk(this, 0, new Vector2(0f, 0f), rad, 0.2f);
		base.bodyChunks[1].collideWithTerrain = false;
		bodyChunkConnections = new BodyChunkConnection[0];
		base.GoThroughFloors = true;
		base.airFriction = 0.99f;
		base.gravity = 0.9f;
		bounce = 0.1f;
		surfaceFriction = 0.47f;
		collisionLayer = 1;
		base.waterFriction = 0.92f;
		base.buoyancy = 0.95f;
		extended = 1f;
	}

	public override void InitiateGraphicsModule()
	{
		if (base.graphicsModule == null && tentacle != null)
		{
			base.graphicsModule = new PoleMimicGraphics(this);
		}
	}

	private void Initiate()
	{
		int num = (room.game.IsStorySession ? 100 : 20);
		if (base.abstractCreature.spawnData != null && base.abstractCreature.spawnData.Length > 2)
		{
			string s = base.abstractCreature.spawnData.Substring(1, base.abstractCreature.spawnData.Length - 2);
			try
			{
				num = int.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture);
			}
			catch
			{
				num = 100;
			}
		}
		shortCutPos = room.LocalCoordinateOfNode(base.abstractCreature.pos.abstractNode).Tile;
		IntVector2 intVector = room.ShorcutEntranceHoleDirection(shortCutPos);
		stickOutDir = intVector.ToVector2();
		rootPos = room.MiddleOfTile(room.LocalCoordinateOfNode(base.abstractCreature.pos.abstractNode)) + stickOutDir * 30f;
		IntVector2 intVector2 = shortCutPos;
		List<IntVector2> list = new List<IntVector2>();
		bool flag = false;
		int num2 = 0;
		freeStanding = false;
		while (!room.GetTile(intVector2 + intVector).Solid && room.IsPositionInsideBoundries(intVector2 + intVector))
		{
			intVector2 += intVector;
			if (flag)
			{
				list.Add(intVector2);
			}
			flag = !flag;
			num2++;
			if (num2 >= num)
			{
				freeStanding = true;
				break;
			}
		}
		if (list.Count < 2)
		{
			list.Insert(0, shortCutPos);
		}
		tilePositions = list.ToArray();
		if (ModManager.MSC)
		{
			float num3 = Mathf.Atan2(stickOutDir.y, stickOutDir.x) * (180f / (float)Math.PI);
			controlRegion = new InputCircularRegion((float)tilePositions.Length * 40f, num3 - 90f, num3 + 90f, 5f, stickOutDir * tilePositions.Length * 40f);
		}
		tipPos = room.MiddleOfTile(intVector2) + stickOutDir * 10f;
		length = Vector2.Distance(rootPos, tipPos);
		tentacle = new Tentacle(this, base.bodyChunks[1], (float)tilePositions.Length * 40f);
		tentacle.tProps = new Tentacle.TentacleProps(stiff: false, rope: true, shorten: true, 0.5f, 0f, 0.2f, 0.5f, 0.05f, 2.2f, 12f, 1f / 3f, 5f, 15, 60, 12, 20);
		tentacle.tChunks = new Tentacle.TentacleChunk[tilePositions.Length];
		for (int i = 0; i < tentacle.tChunks.Length; i++)
		{
			tentacle.tChunks[i] = new Tentacle.TentacleChunk(tentacle, i, (float)(i + 1) / (float)tentacle.tChunks.Length, rad);
		}
		tentacle.stretchAndSqueeze = 0.1f;
		tentacle.NewRoom(room);
		for (int j = 0; j < tentacle.tChunks.Length; j++)
		{
			tentacle.tChunks[j].pos = room.MiddleOfTile(tilePositions[j]);
			tentacle.tChunks[j].lastPos = tentacle.tChunks[j].pos;
		}
		tentacle.segments.Clear();
		for (int k = 0; k < tilePositions.Length; k++)
		{
			tentacle.segments.Add(tilePositions[k]);
			if (k < tilePositions.Length - 1)
			{
				tentacle.segments.Add(tilePositions[k] - IntVector2.ClampAtOne(tilePositions[k] - tilePositions[k + 1]));
			}
		}
		List<IntVector2> path = null;
		tentacle.MoveGrabDest(tipPos, ref path);
		stickChunks = new BodyChunk[tentacle.tChunks.Length];
		base.mainBodyChunk.HardSetPosition(tipPos);
		tipAttached = true;
		mimic = 1f;
		mimicDelayCounter = 40;
		if (room.climbableVines == null)
		{
			room.climbableVines = new ClimbableVinesSystem();
			room.AddObject(room.climbableVines);
		}
		room.climbableVines.vines.Add(this);
		InitiateGraphicsModule();
		room.drawableObjects.Add(base.graphicsModule);
		for (int l = 0; l < room.game.cameras.Length; l++)
		{
			if (room.game.cameras[l].room == room)
			{
				room.game.cameras[l].NewObjectInRoom(base.graphicsModule);
			}
		}
		appendages = new List<Appendage>();
		appendages.Add(new Appendage(this, 0, tentacle.tChunks.Length + 1));
	}

	public override void NewRoom(Room room)
	{
		base.NewRoom(room);
		huntChunk = null;
		tipAttached = false;
		if (tentacle != null)
		{
			List<IntVector2> path = null;
			tentacle.NewRoom(room);
			tentacle.Reset(rootPos);
			tentacle.MoveGrabDest(tipPos, ref path);
			tentacle.idealLength = length;
			for (int i = 0; i < tentacle.tChunks.Length; i++)
			{
				tentacle.tChunks[i].vel = stickOutDir * Mathf.Clamp(i, 1f, 15f);
			}
		}
		else
		{
			Initiate();
		}
	}

	public override void SpitOutOfShortCut(IntVector2 pos, Room newRoom, bool spitOutAllSticks)
	{
		justOutOfShortCut = true;
		base.SpitOutOfShortCut(pos, newRoom, spitOutAllSticks);
		wakeUpCounter = 250;
		mimic = 0f;
		mimicDelayCounter = 0;
		if (base.graphicsModule != null)
		{
			(base.graphicsModule as PoleMimicGraphics).lookLikeAPole = 0f;
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (room == null)
		{
			return;
		}
		if (justOutOfShortCut && base.graphicsModule != null)
		{
			justOutOfShortCut = false;
		}
		if (angeredAndAggressive > 0)
		{
			if (base.stun < 1)
			{
				angeredAndAggressive--;
			}
			wakeUpCounter = Math.Max(wakeUpCounter, angeredAndAggressive);
		}
		base.abstractCreature.pos.Tile = room.GetTilePosition(rootPos);
		if (room == null || enteringShortCut.HasValue)
		{
			return;
		}
		tentacle.Update();
		tentacle.limp = !base.Consious;
		tentacle.retractFac = 1f - extended;
		base.mainBodyChunk.collideWithTerrain = extended == 1f && tentacle.backtrackFrom == -1;
		if (!Custom.DistLess(base.bodyChunks[1].pos, base.bodyChunks[0].pos, tentacle.idealLength * 2f * extended))
		{
			base.bodyChunks[0].pos = base.bodyChunks[1].pos + Custom.DirVec(base.bodyChunks[1].pos, base.bodyChunks[0].pos) * tentacle.idealLength * 2f * extended;
		}
		if (room.game.devToolsActive && Input.GetKey("b"))
		{
			base.mainBodyChunk.vel += Custom.DirVec(base.mainBodyChunk.pos, (Vector2)Futile.mousePosition + room.game.cameras[0].pos) * 3f;
		}
		if (base.Consious)
		{
			Act();
		}
		else
		{
			tipAttached = false;
		}
		tentacle.tProps.goalAttractionSpeed = getToGoalForce * 0.4f;
		tentacle.tProps.alignToSegmentSpeed = getToGoalForce * 0.5f;
		BodyChunk bodyChunk = null;
		float num = 0f;
		for (int i = 0; i < tentacle.tChunks.Length; i++)
		{
			float num2 = (float)i / (float)(tentacle.tChunks.Length - 1);
			tentacle.tChunks[i].vel *= 0.96f;
			if (base.grasps[0] != null && base.grasps[0].grabbed is Creature && (base.grasps[0].grabbed as Creature).enteringShortCut.HasValue)
			{
				ReleaseGrasp(0);
			}
			if (extended > 0.5f && base.grasps[0] != null && tentacle.tChunks[i].phase > -1f)
			{
				ReleaseGrasp(0);
			}
			if ((base.State as HealthState).health < 0.5f)
			{
				tentacle.tChunks[i].vel += Custom.RNV() * Mathf.InverseLerp(0.5f, 0f, (base.State as HealthState).health) * 2f;
			}
			if (tentacle.backtrackFrom == -1 || (tentacle.backtrackFrom > i - 2 && base.Consious))
			{
				if (stickChunks[i] != null)
				{
					wantToWakeUp = true;
					float num3 = Vector2.Distance(tentacle.tChunks[i].pos, stickChunks[i].pos);
					if (num3 > stickChunks[i].rad + Mathf.Lerp(14f, 20f + 5f * WakeUp, AnyWakeUp) || stickChunks[i].owner.room != room)
					{
						stickChunks[i] = null;
						room.PlaySound(SoundID.Pole_Mimic_Unstick, tentacle.tChunks[i].pos, 1f, 1f);
						continue;
					}
					if (ChunkTastyness(i) > num)
					{
						bodyChunk = stickChunks[i];
						num = ChunkTastyness(i);
					}
					Vector2 vector = Custom.DirVec(tentacle.tChunks[i].pos, stickChunks[i].pos);
					float num4 = stickChunks[i].mass / (stickChunks[i].mass + 0.18f);
					float num5 = Mathf.Lerp(0.35f, 0.6f, WakeUp);
					tentacle.tChunks[i].pos += vector * (num3 - stickChunks[i].rad) * num4 * num5;
					tentacle.tChunks[i].vel += vector * (num3 - stickChunks[i].rad) * num4 * num5;
					stickChunks[i].pos -= vector * (num3 - stickChunks[i].rad) * (1f - num4) * num5;
					stickChunks[i].vel -= vector * (num3 - stickChunks[i].rad) * (1f - num4) * num5;
					if (stickChunks[i].owner is Player && UnityEngine.Random.value < (stickChunks[i].owner as Player).GraspWiggle / 11f)
					{
						tentacle.tChunks[i].vel += Custom.DirVec(stickChunks[i].pos, tentacle.tChunks[i].pos) * Mathf.Lerp(4f, 8f, UnityEngine.Random.value);
						stickChunks[i] = null;
					}
					continue;
				}
				BodyChunk bodyChunk2 = null;
				float num6 = 0f;
				if (ChunkTastyness(i - 1) > num6)
				{
					bodyChunk2 = stickChunks[i - 1];
					num6 = ChunkTastyness(i - 1);
				}
				if (ChunkTastyness(i + 1) > num6)
				{
					bodyChunk2 = stickChunks[i + 1];
					num6 = ChunkTastyness(i + 1);
				}
				if (bodyChunk2 != null)
				{
					tentacle.tChunks[i].vel += Vector2.ClampMagnitude(bodyChunk2.pos - tentacle.tChunks[i].pos, 10f) / 5f;
					continue;
				}
				if (room.abstractRoom.creatures.Count == 0)
				{
					return;
				}
				AbstractCreature abstractCreature = room.abstractRoom.creatures[UnityEngine.Random.Range(0, room.abstractRoom.creatures.Count)];
				if (!abstractCreature.InDen && base.abstractCreature.creatureTemplate.CreatureRelationship(abstractCreature.creatureTemplate).type == CreatureTemplate.Relationship.Type.Eats && abstractCreature.realizedCreature != null && abstractCreature.realizedCreature.room == room)
				{
					BodyChunk bodyChunk3 = abstractCreature.realizedCreature.bodyChunks[UnityEngine.Random.Range(0, abstractCreature.realizedCreature.bodyChunks.Length)];
					if (Custom.DistLess(bodyChunk3.pos, tentacle.tChunks[i].pos, bodyChunk3.rad + 2f))
					{
						if ((angeredAndAggressive > 0 && !(bodyChunk3.owner is Player)) || UnityEngine.Random.value < Mathf.Pow(AnyWakeUp, 0.25f) || UnityEngine.Random.value < 1f / 7f)
						{
							room.PlaySound(SoundID.Pole_Mimic_Stick, tentacle.tChunks[i].pos, 1f, 1f);
							stickChunks[i] = bodyChunk3;
						}
						if (base.graphicsModule != null && (base.graphicsModule as PoleMimicGraphics).leavesMimic != null && (base.graphicsModule as PoleMimicGraphics).leaves != null)
						{
							for (int num7 = UnityEngine.Random.Range(2, 6); num7 >= 0; num7--)
							{
								int num8 = Custom.IntClamp((int)(num2 * (float)(base.graphicsModule as PoleMimicGraphics).leafPairs) - 2 + UnityEngine.Random.Range(0, 5), 0, (base.graphicsModule as PoleMimicGraphics).leafPairs - 1);
								int num9 = ((!(UnityEngine.Random.value < 0.5f)) ? 1 : 0);
								(base.graphicsModule as PoleMimicGraphics).leavesMimic[num8, num9, 0] = UnityEngine.Random.value;
								(base.graphicsModule as PoleMimicGraphics).leavesMimic[num8, num9, 2] = UnityEngine.Random.value;
								(base.graphicsModule as PoleMimicGraphics).leaves[num8, num9, 0] += Custom.RNV() * 2f * UnityEngine.Random.value;
							}
						}
					}
				}
				if (mimic == 1f && ChunkInPosition(i))
				{
					tentacle.tChunks[i].pos = room.MiddleOfTile(tilePositions[i]);
					tentacle.tChunks[i].vel *= 0f;
					continue;
				}
				if ((i > 0 && ChunkInPosition(i - 1)) || (i < tentacle.tChunks.Length - 1 && ChunkInPosition(i + 1)))
				{
					tentacle.tChunks[i].vel *= Mathf.Lerp(1f, 0.6f, Mathf.InverseLerp(0.5f, 1f, mimic));
				}
				if (tipAttached || freeStanding)
				{
					tentacle.tChunks[i].vel += Vector2.ClampMagnitude(room.MiddleOfTile(tilePositions[i]) - tentacle.tChunks[i].pos, 10f) / 5f * Mathf.InverseLerp(freeStanding ? 0.75f : 0.5f, 1f, mimic);
				}
				Vector2 p = rootPos - stickOutDir * 30f;
				if (i == 1)
				{
					p = rootPos;
				}
				else if (i > 1)
				{
					p = tentacle.tChunks[i - 2].pos;
					tentacle.tChunks[i - 2].vel += Custom.DirVec(tentacle.tChunks[i].pos, tentacle.tChunks[i - 2].pos);
				}
				tentacle.tChunks[i].vel += Custom.DirVec(p, tentacle.tChunks[i].pos);
				if (base.Consious)
				{
					tentacle.tChunks[i].vel += stickOutDir * Mathf.Lerp(0.3f, 0f, num2);
				}
			}
			else
			{
				stickChunks[i] = null;
			}
		}
		if (bodyChunk != null && Mathf.Pow(UnityEngine.Random.value, 3f) < WakeUp && ChunkTastyness(bodyChunk) > ChunkTastyness(huntChunk))
		{
			huntChunk = bodyChunk;
			huntCounter = UnityEngine.Random.Range(110, 190);
		}
		tentacle.retractFac = 1f - extended;
		if (extended == 0f)
		{
			for (int j = 0; j < 2; j++)
			{
				base.bodyChunks[j].collideWithTerrain = false;
				base.bodyChunks[j].HardSetPosition(rootPos + new Vector2(0f, -50f));
				base.bodyChunks[j].vel *= 0f;
			}
		}
		else
		{
			base.bodyChunks[1].pos = rootPos;
			base.bodyChunks[1].vel *= 0f;
			for (int k = 0; k < 2; k++)
			{
				base.bodyChunks[k].collideWithTerrain = tentacle.backtrackFrom == -1;
			}
			float num10 = 0f;
			if (tentacle.backtrackFrom == -1)
			{
				num10 = 0.5f;
				if (!base.Consious)
				{
					num10 = 0.7f;
				}
			}
			Vector2 vector2 = Custom.DirVec(base.bodyChunks[0].pos, tentacle.Tip.pos);
			float num11 = Vector2.Distance(base.bodyChunks[0].pos, tentacle.Tip.pos);
			base.bodyChunks[0].pos -= (0f - num11) * vector2 * (1f - num10);
			base.bodyChunks[0].vel -= (0f - num11) * vector2 * (1f - num10);
			tentacle.Tip.pos += (0f - num11) * vector2 * num10;
			tentacle.Tip.vel += (0f - num11) * vector2 * num10;
			if (base.Consious)
			{
				base.bodyChunks[0].vel.y += base.gravity;
			}
		}
		if (!base.State.alive && UnityEngine.Random.value < 0.025f)
		{
			Die();
		}
		if (base.dead || Mathf.InverseLerp(0.3f, 0.65f, (base.State as HealthState).health) < 1f - base.abstractCreature.personality.aggression)
		{
			base.stun = 0;
			extended = Mathf.Max(0f, extended - 1f / 60f);
			ReleaseGrasp(0);
			if (extended <= 0f)
			{
				if (base.abstractCreature.pos.abstractNode != -1 && base.abstractCreature.GetNodeType != AbstractRoomNode.Type.Den)
				{
					Destroy();
				}
				else
				{
					enteringShortCut = shortCutPos;
				}
			}
		}
		if (base.grasps[0] != null)
		{
			if (tentacle.TotalRope > 600f || (base.grasps[0].grabbed is Creature && (base.grasps[0].grabbed as Creature).enteringShortCut.HasValue))
			{
				ReleaseGrasp(0);
				return;
			}
			Carry(eu);
			extended -= 1f / Custom.LerpMap(Vector2.Distance(base.grasps[0].grabbedChunk.pos, rootPos), 150f, 50f, 280f, 30f);
			base.grasps[0].grabbedChunk.vel += Vector2.ClampMagnitude(rootPos - base.grasps[0].grabbedChunk.pos, Mathf.Lerp(25f, 400f, forceIntoShortCut)) * (1f - extended) / 20f / Mathf.Lerp(base.grasps[0].grabbedChunk.mass, 0.1f, forceIntoShortCut) * Mathf.InverseLerp(200f, 50f, Vector2.Distance(base.grasps[0].grabbedChunk.pos, rootPos));
			if (extended < 0f)
			{
				if (!(base.grasps[0].grabbed is Creature) || base.abstractCreature.creatureTemplate.CreatureRelationship(base.grasps[0].grabbed as Creature).type != CreatureTemplate.Relationship.Type.Eats)
				{
					ReleaseGrasp(0);
					extended = 0f;
					forceIntoShortCut = 0f;
					return;
				}
				extended = 0f;
				forceIntoShortCut += Mathf.InverseLerp(200f, 50f, Vector2.Distance(base.grasps[0].grabbedChunk.pos, rootPos)) / 30f;
				if (Custom.DistLess(base.grasps[0].grabbedChunk.pos, rootPos, 20f) || forceIntoShortCut > 1f)
				{
					enteringShortCut = shortCutPos;
					forceIntoShortCut = 0f;
				}
			}
			else
			{
				forceIntoShortCut = 0f;
			}
			if (base.grasps[0].grabbed is Player && UnityEngine.Random.value < Mathf.Lerp(0f, 1f / 15f, (base.grasps[0].grabbed as Player).GraspWiggle))
			{
				base.mainBodyChunk.vel += Custom.DirVec(base.grasps[0].grabbedChunk.pos, base.mainBodyChunk.pos) * Mathf.Lerp(4f, 8f, UnityEngine.Random.value);
				ReleaseGrasp(0);
				Stun(UnityEngine.Random.Range(8, 16));
			}
		}
		else if (!base.dead)
		{
			extended = 1f;
			forceIntoShortCut = 0f;
		}
	}

	private float ChunkTastyness(int i)
	{
		if (i < 0 || i >= stickChunks.Length || stickChunks[i] == null)
		{
			return 0f;
		}
		return ChunkTastyness(stickChunks[i]);
	}

	private float ChunkTastyness(BodyChunk chunk)
	{
		if (chunk == null || !(chunk.owner is Creature) || base.abstractCreature.creatureTemplate.CreatureRelationship(chunk.owner as Creature).type != CreatureTemplate.Relationship.Type.Eats)
		{
			return 0f;
		}
		return chunk.mass * base.abstractCreature.creatureTemplate.CreatureRelationship(chunk.owner as Creature).intensity;
	}

	private void Act()
	{
		if (base.safariControlled)
		{
			controlRegion.drawDebugPos = base.bodyChunks[1].pos;
			controlRegion.Update(inputWithDiagonals);
			Vector2 p = base.bodyChunks[0].pos - base.bodyChunks[1].pos;
			float num = Custom.Dist(p, controlRegion.pos);
			if (num > 60f)
			{
				controlRegion.pos.x = Mathf.Lerp(controlRegion.pos.x, p.x, Mathf.Lerp(0f, 0.2f, (num - 60f) / 200f));
				controlRegion.pos.y = Mathf.Lerp(controlRegion.pos.y, p.y, Mathf.Lerp(0f, 0.2f, (num - 60f) / 200f));
			}
			if (inputWithDiagonals.HasValue && inputWithDiagonals.Value.jmp)
			{
				huntCounter = 0;
				wakeUpCounter = 0;
				wantToWakeUp = false;
				if (base.grasps[0] != null && base.grasps[0].grabbed != null)
				{
					ReleaseGrasp(0);
				}
			}
			else if (inputWithDiagonals.HasValue && (inputWithDiagonals.Value.x != 0 || inputWithDiagonals.Value.y != 0) && (base.grasps[0] == null || base.grasps[0].grabbed == null))
			{
				huntCounter = 120;
				huntChunk = new BodyChunk(this, 0, base.bodyChunks[1].pos + controlRegion.pos, 1f, 1f);
			}
			if (huntCounter <= 0)
			{
				controlRegion.pos = stickOutDir * tilePositions.Length * 40f;
			}
		}
		if (ModManager.MSC && base.LickedByPlayer != null)
		{
			BeingClimbedOn(base.LickedByPlayer);
			huntChunk = base.LickedByPlayer.firstChunk;
			huntCounter = UnityEngine.Random.Range(110, 190);
		}
		if (huntCounter > 100)
		{
			wantToWakeUp = true;
			getToGoalForce = Mathf.Min(1f, getToGoalForce + WakeUp / 140f);
		}
		if (wantToWakeUp && wakeUpCounter < 250)
		{
			wakeUpCounter++;
		}
		else if (!wantToWakeUp && wakeUpCounter > 0)
		{
			wakeUpCounter--;
		}
		else if (angeredAndAggressive > 0)
		{
			wantToWakeUp = true;
		}
		else
		{
			wantToWakeUp = false;
		}
		float num2 = Custom.LerpMap(getToGoalForce, 0.7f, 1f, freeStanding ? 5f : 20f, 30f);
		if (tipAttached)
		{
			if (!freeStanding || (Custom.DistLess(base.mainBodyChunk.pos, tipPos, num2) && base.mainBodyChunk.vel.magnitude < num2))
			{
				base.mainBodyChunk.pos = tipPos;
				base.mainBodyChunk.vel *= 0f;
				tentacle.Tip.pos = tipPos;
				tentacle.Tip.vel *= 0f;
				getToGoalForce = Mathf.Max(0f, getToGoalForce - 1f / 30f);
			}
			if (huntChunk != null && (UnityEngine.Random.value < 1f / 30f || freeStanding))
			{
				tipAttached = false;
			}
		}
		else
		{
			if (wakeUpCounter == 0)
			{
				getToGoalForce = Mathf.Min(1f, getToGoalForce + 1f / 140f);
			}
			if (Custom.DistLess(base.mainBodyChunk.pos, tipPos, num2))
			{
				tipAttached = true;
			}
		}
		if (UnityEngine.Random.value < Mathf.Pow(WakeUp, 5f) || (freeStanding && WakeUp > 0f))
		{
			tipAttached = false;
		}
		if (tipAttached && wakeUpCounter == 0)
		{
			mimicDelayCounter++;
		}
		else
		{
			mimicDelayCounter = 0;
		}
		if (mimicDelayCounter > 40)
		{
			mimic = Custom.LerpAndTick(mimic, 1f, 0.01f, 0.0125f);
		}
		else
		{
			mimic = Custom.LerpAndTick(mimic, 0f, 0.01f, 0.0125f);
		}
		tentacle.idealLength = (float)tilePositions.Length * 40f * Mathf.Lerp(1f, 0.75f, (1f - getToGoalForce) * (1f - mimic));
		List<IntVector2> path = null;
		if (huntChunk != null && !tipAttached)
		{
			if (room.VisualContact(base.mainBodyChunk.pos, huntChunk.pos))
			{
				tentacle.MoveGrabDest(huntChunk.pos, ref path);
			}
			if (huntChunk.owner.collisionLayer != collisionLayer && Custom.DistLess(huntChunk.pos, base.mainBodyChunk.pos, huntChunk.rad + base.mainBodyChunk.rad))
			{
				Collide(huntChunk.owner, base.mainBodyChunk.index, huntChunk.index);
			}
		}
		else
		{
			tentacle.MoveGrabDest(tipPos, ref path);
		}
		if (huntCounter > 0)
		{
			huntCounter--;
		}
		if (huntChunk != null && (huntCounter < 1 || huntChunk.owner.room != room))
		{
			huntChunk = null;
		}
	}

	public override void Stun(int st)
	{
		base.Stun(st);
		ReleaseGrasp(0);
	}

	public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
	{
		base.Collide(otherObject, myChunk, otherChunk);
		if (!(otherObject is TentaclePlant) && !(otherObject is PoleMimic) && (!ModManager.MSC || !(otherObject is BigJellyFish)) && !(otherObject is GarbageWorm) && myChunk <= 0 && base.grasps[0] == null)
		{
			BodyChunk bodyChunk = otherObject.bodyChunks[otherChunk];
			bodyChunk.vel = Vector2.Lerp(bodyChunk.vel, base.mainBodyChunk.vel, 0.2f / bodyChunk.mass);
			if (base.Consious && WakeUp > 0.3f && (!base.safariControlled || (inputWithDiagonals.HasValue && inputWithDiagonals.Value.pckp)))
			{
				room.PlaySound((otherObject is Player) ? SoundID.Pole_Mimic_Grab_Player : SoundID.Pole_Mimic_Grab_Creature, base.mainBodyChunk);
				Grab(otherObject, 0, otherChunk, Grasp.Shareability.CanOnlyShareWithNonExclusive, 1f, overrideEquallyDominant: true, pacifying: false);
			}
		}
	}

	public void Carry(bool eu)
	{
		Vector2 vector = Custom.DirVec(base.grasps[0].grabbedChunk.pos, tentacle.Tip.pos);
		float num = Vector2.Distance(base.grasps[0].grabbedChunk.pos, tentacle.Tip.pos);
		float num2 = base.grasps[0].grabbedChunk.mass / (base.grasps[0].grabbedChunk.mass + 0.07f);
		base.grasps[0].grabbedChunk.pos -= (0f - num) * vector * (1f - num2);
		base.grasps[0].grabbedChunk.vel -= (0f - num) * vector * (1f - num2);
		tentacle.Tip.pos += (0f - num) * vector * num2;
		tentacle.Tip.vel += (0f - num) * vector * num2;
	}

	public int TotalPositions()
	{
		return tentacle.tChunks.Length;
	}

	public Vector2 Pos(int index)
	{
		return tentacle.tChunks[index].pos;
	}

	public float Rad(int index)
	{
		return 2f;
	}

	public float Mass(int index)
	{
		return 1f;
	}

	public void Push(int index, Vector2 movement)
	{
		tentacle.tChunks[index].vel += movement / 0.3f;
	}

	public void BeingClimbedOn(Creature crit)
	{
		mimicDelayCounter = 0;
		mimic = 0f;
		if (wakeUpCounter == 0 && room != null)
		{
			room.PlaySound(SoundID.Player_Grab_Pole_Mimic, crit.mainBodyChunk.pos);
		}
		wantToWakeUp = true;
		if (freeStanding)
		{
			tipAttached = false;
		}
	}

	public bool CurrentlyClimbable()
	{
		return WakeUp < 1f;
	}

	public Vector2 AppendagePosition(int appendage, int segment)
	{
		if (segment == 0)
		{
			return rootPos;
		}
		return tentacle.tChunks[segment - 1].pos;
	}

	public void ApplyForceOnAppendage(Appendage.Pos pos, Vector2 momentum)
	{
		if (pos.prevSegment > 0)
		{
			tentacle.tChunks[pos.prevSegment - 1].pos += momentum / 0.1f * (1f - pos.distanceToNext);
			tentacle.tChunks[pos.prevSegment - 1].vel += momentum / 0.05f * (1f - pos.distanceToNext);
		}
		tentacle.tChunks[pos.prevSegment].pos += momentum / 0.1f * pos.distanceToNext;
		tentacle.tChunks[pos.prevSegment].vel += momentum / 0.05f * pos.distanceToNext;
	}

	public override void HitByWeapon(Weapon weapon)
	{
		base.HitByWeapon(weapon);
		angeredAndAggressive = Math.Max(angeredAndAggressive, UnityEngine.Random.Range(80, 120));
	}
}
