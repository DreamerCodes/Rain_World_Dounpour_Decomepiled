using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class FlyAI
{
	public class DropStatus : ExtEnum<DropStatus>
	{
		public static readonly DropStatus HaventDropped = new DropStatus("HaventDropped", register: true);

		public static readonly DropStatus Dropping = new DropStatus("Dropping", register: true);

		public static readonly DropStatus HasDropped = new DropStatus("HasDropped", register: true);

		public DropStatus(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class Behavior : ExtEnum<Behavior>
	{
		public static readonly Behavior Idle = new Behavior("Idle", register: true);

		public static readonly Behavior Swarm = new Behavior("Swarm", register: true);

		public static readonly Behavior Drop = new Behavior("Drop", register: true);

		public static readonly Behavior Flee = new Behavior("Flee", register: true);

		public static readonly Behavior FleeFromRain = new Behavior("FleeFromRain", register: true);

		public static readonly Behavior LeaveRoom = new Behavior("LeaveRoom", register: true);

		public static readonly Behavior Burrow = new Behavior("Burrow", register: true);

		public static readonly Behavior Chain = new Behavior("Chain", register: true);

		public static readonly Behavior Lure = new Behavior("Lure", register: true);

		public Behavior(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	private class TrackedThreat
	{
		public FlyAI owner;

		public AbstractCreature crit;

		public int ticksSinceSeen;

		public bool deleteMeNextFrame;

		public bool visualContact;

		public bool currentlyScaredOf;

		public int ticksToForget;

		public Vector2 lastSeenPos;

		public float Severity => owner.Template.CreatureRelationship(crit.creatureTemplate).intensity * Mathf.Lerp(1f, 0f, (float)ticksSinceSeen / (float)ticksToForget);

		public TrackedThreat(FlyAI owner, AbstractCreature crit)
		{
			this.owner = owner;
			this.crit = crit;
			ticksSinceSeen = 0;
			deleteMeNextFrame = false;
			visualContact = true;
			ticksToForget = (int)Mathf.Lerp(50f, 250f, owner.Template.CreatureRelationship(crit.creatureTemplate).intensity);
		}

		public void Update()
		{
			ticksSinceSeen++;
			if (ticksSinceSeen > 10)
			{
				visualContact = false;
				if (crit.Room == owner.room.abstractRoom && crit.realizedCreature != null)
				{
					BodyChunk bodyChunk = crit.realizedCreature.bodyChunks[Random.Range(0, crit.realizedCreature.bodyChunks.Length)];
					if (owner.VisualContact(bodyChunk.pos, seeIfInsectFlight: false))
					{
						visualContact = true;
						ticksSinceSeen = 0;
					}
				}
				if (ticksSinceSeen > ticksToForget)
				{
					deleteMeNextFrame = true;
				}
			}
			else if (crit.realizedCreature != null && crit.realizedCreature.room != null && crit.realizedCreature.room.abstractRoom == owner.room.abstractRoom)
			{
				lastSeenPos = crit.realizedCreature.mainBodyChunk.pos;
			}
			else
			{
				visualContact = false;
			}
			currentlyScaredOf = visualContact && Custom.DistLess(owner.FlyPos, lastSeenPos, AfraidDistance(crit.creatureTemplate));
		}
	}

	public class FlockBehavior
	{
		public bool drop;

		public bool formChains;

		public float sinCycle;

		public float swarmFlightSinFreq;

		public float swarmFlightSinAmpl;

		public float swarmFlightVerticalDisplace;

		public float distanceToOthers;

		public float exaggerateDirectionOfOthers;

		public float collectiveMovement;

		public Vector2 flockDir;

		public FlyAI leader;

		public float life = 1f;

		public float lifeTime;

		public float dom;

		public Color col;

		public float Dominance => dom * Mathf.InverseLerp(0f, 0.2f, life);

		public FlockBehavior(FlyAI leader)
		{
			this.leader = leader;
			sinCycle = Random.value;
			drop = Random.value < 0.5f;
			distanceToOthers = 25f + Random.value * 75f;
			col = Custom.HSL2RGB(Random.value, 1f, 0.5f);
			swarmFlightVerticalDisplace = Mathf.Lerp(200f, 320f, Random.value);
			swarmFlightSinFreq = 1f / (30f + 120f * Random.value);
			swarmFlightSinAmpl = Random.value * 80f;
			flockDir = Custom.DegToVec(Random.value * 360f) * Random.value * 140f;
			exaggerateDirectionOfOthers = Random.value;
			collectiveMovement = ((Random.value < 0.5f) ? 0f : Mathf.Pow(Random.value, 10f));
			formChains = Random.value < 0.5f;
			lifeTime = Mathf.Lerp(200f, 800f, Random.value);
			dom = Random.value;
		}

		public void Update()
		{
			sinCycle += swarmFlightSinFreq;
			life -= 1f / lifeTime;
		}
	}

	private Fly fly;

	public Vector2 localGoal;

	public int followingDijkstraMap = -1;

	public int leaveRoomDijkstra = -1;

	public Fly otherFly;

	public Vector2 lowestOtherPos;

	public FlockBehavior flockBehavior;

	private List<TrackedThreat> threats;

	public float afraid;

	public int panicFlee;

	public bool fleeFromRain;

	public DropStatus dropStatus;

	public int stuckCounter;

	public Vector2 stuckPos;

	public int getBackToGround;

	public int noSwarmCounter;

	public int luredCounter;

	public FlyLure lure;

	public Behavior behavior;

	public Room room => fly.room;

	public bool CurrentFollowDijkstraIsToHive
	{
		get
		{
			if (followingDijkstraMap < 0)
			{
				return false;
			}
			try
			{
				return room.abstractRoom.nodes[room.abstractRoom.CreatureSpecificToCommonNodeIndex(followingDijkstraMap, Template)].type == AbstractRoomNode.Type.BatHive;
			}
			catch
			{
				return false;
			}
		}
	}

	public bool InActiveSwarmRoom
	{
		get
		{
			if (room.game.session is StoryGameSession && !room.world.singleRoomWorld)
			{
				if (room.abstractRoom.swarmRoom)
				{
					return room.world.regionState.SwarmRoomActive(room.abstractRoom.swarmRoomIndex);
				}
				return false;
			}
			return true;
		}
	}

	public bool Stuck
	{
		get
		{
			if (stuckCounter >= 40 && behavior != Behavior.Chain)
			{
				return behavior != Behavior.Burrow;
			}
			return false;
		}
	}

	private CreatureTemplate Template => fly.Template;

	public Vector2 FlyPos => fly.mainBodyChunk.pos;

	public bool DoingSpecificAnimation
	{
		get
		{
			if (!(behavior == Behavior.Drop))
			{
				return behavior == Behavior.Burrow;
			}
			return true;
		}
	}

	public FlyAI(Fly fly, World world)
	{
		this.fly = fly;
		flockBehavior = new FlockBehavior(this);
		threats = new List<TrackedThreat>();
		fleeFromRain = world.rainCycle.RainApproaching < 0.3f;
	}

	private void ChangeBehavior(Behavior newBehav)
	{
		if (!(behavior == newBehav))
		{
			if (newBehav != Behavior.Chain || newBehav != Behavior.Burrow)
			{
				fly.burrowOrHangSpot = null;
			}
			if (behavior == Behavior.Chain && fly.grasps[0] != null)
			{
				room.PlaySound(SoundID.Bat_Detatch_From_Chain, fly.mainBodyChunk);
			}
			else if (newBehav == Behavior.Flee)
			{
				room.PlaySound(SoundID.Bat_Startled, fly.mainBodyChunk);
			}
			behavior = newBehav;
		}
	}

	public void Update()
	{
		if (getBackToGround > 0)
		{
			getBackToGround--;
		}
		else if (room.aimap.getTerrainProximity(FlyPos) > 8 || room.aimap.getAItile(FlyPos).smoothedFloorAltitude > 8)
		{
			getBackToGround = Random.Range(80, 200);
		}
		if (DoingSpecificAnimation)
		{
			fly.LoseAllGrasps();
		}
		else
		{
			otherFly = room.fliesRoomAi.GetRandomFly().LastInChain();
			if (otherFly != null && (otherFly == fly || !room.VisualContact(FlyPos, otherFly.mainBodyChunk.pos)))
			{
				otherFly = null;
			}
			if (otherFly != null)
			{
				ConsiderOtherFly();
			}
			if (ModManager.MMF && RoomNotACycleHazard(room))
			{
				fleeFromRain = false;
			}
			if (fleeFromRain)
			{
				ChangeBehavior(Behavior.FleeFromRain);
			}
			else if (afraid >= 1f && luredCounter < 1)
			{
				ChangeBehavior(Behavior.Flee);
			}
			if (fly.grasps[0] != null)
			{
				behavior = Behavior.Chain;
			}
			if (luredCounter > 0 && lure != null)
			{
				ChangeBehavior(Behavior.Lure);
			}
			if (dropStatus == DropStatus.Dropping)
			{
				ChangeBehavior(Behavior.Drop);
			}
		}
		if (luredCounter > 0)
		{
			luredCounter--;
			if (luredCounter == 0)
			{
				lure = null;
			}
		}
		fly.movMode = Fly.MovementMode.BatFlight;
		if (behavior == Behavior.Idle)
		{
			IdleUpdate();
		}
		else if (behavior == Behavior.Swarm)
		{
			noSwarmCounter = Random.Range(20, 80);
			fly.movMode = Fly.MovementMode.SwarmFlight;
			SwarmUpdate();
		}
		else if (behavior == Behavior.Flee)
		{
			FleeUpdate();
			if (afraid <= 0f)
			{
				ChangeBehavior(Behavior.Idle);
			}
		}
		else if (behavior == Behavior.Drop)
		{
			fly.movMode = Fly.MovementMode.Passive;
			DropUpdate();
		}
		else if (behavior == Behavior.Burrow)
		{
			fly.movMode = Fly.MovementMode.Burrow;
			if (!fly.burrowOrHangSpot.HasValue || !Custom.DistLess(FlyPos, fly.burrowOrHangSpot.Value, 100f))
			{
				fly.burrowOrHangSpot = null;
				ChangeBehavior(Behavior.Idle);
			}
		}
		else if (behavior == Behavior.FleeFromRain)
		{
			FleeFromRainUpdate();
		}
		else if (behavior == Behavior.Chain)
		{
			if ((fly.burrowOrHangSpot.HasValue && Custom.DistLess(FlyPos, fly.burrowOrHangSpot.Value, 40f)) || fly.grasps[0] != null)
			{
				fly.movMode = Fly.MovementMode.Hang;
				HangInChainUpdate();
			}
			else
			{
				ChangeBehavior(Behavior.Idle);
			}
		}
		else if (behavior == Behavior.Lure && lure != null)
		{
			if (lure.room != fly.room)
			{
				lure = null;
				luredCounter = 0;
			}
			else
			{
				if (Random.value * 7f > Vector2.Distance(lure.firstChunk.lastPos, lure.firstChunk.pos))
				{
					Vector2 vector = lure.firstChunk.pos + Custom.RNV() * Mathf.Pow(Random.value, 3f) * 90f;
					if (!room.GetTile(vector).Solid && room.VisualContact(vector, lure.firstChunk.pos))
					{
						localGoal = vector;
					}
				}
				followingDijkstraMap = -1;
				if (Random.value < 0.5f && Custom.DistLess(lure.firstChunk.lastPos, fly.firstChunk.pos, 9f))
				{
					fly.firstChunk.vel += Custom.RNV() * 6f;
					lure.firstChunk.pos += Custom.DegToVec(Mathf.Lerp(-70f, 70f, Random.value)) + Custom.DirVec(lure.firstChunk.pos, fly.firstChunk.pos) * 1.5f;
					lure.firstChunk.vel += Custom.DegToVec(Mathf.Lerp(-70f, 70f, Random.value)) + Custom.DirVec(lure.firstChunk.pos, fly.firstChunk.pos) * 2f;
					lure.room.PlaySound(SoundID.Fly_Lure_Ruffled_By_Fly, lure.firstChunk);
					lure = null;
					luredCounter = 0;
				}
			}
		}
		if (flockBehavior.leader == this)
		{
			flockBehavior.Update();
		}
		if (flockBehavior.life <= 0f)
		{
			flockBehavior = new FlockBehavior(this);
		}
		if (behavior != Behavior.Swarm && luredCounter < 1)
		{
			UpdateThreats();
		}
		if (fly.safariControlled)
		{
			flockBehavior.leader = this;
			flockBehavior.dom = 1f;
			flockBehavior.distanceToOthers = 25f;
			if (room != null)
			{
				for (int i = 0; i < room.abstractRoom.creatures.Count; i++)
				{
					if (room.abstractRoom.creatures[i].realizedCreature != null && room.abstractRoom.creatures[i].realizedCreature is Fly && Custom.Dist(fly.mainBodyChunk.pos, room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk.pos) < 250f)
					{
						(room.abstractRoom.creatures[i].realizedCreature as Fly).AI.flockBehavior = flockBehavior;
					}
				}
			}
			if (fly.inputWithDiagonals.HasValue)
			{
				localGoal = fly.mainBodyChunk.pos + new Vector2(fly.inputWithDiagonals.Value.x, fly.inputWithDiagonals.Value.y) * 100f + new Vector2(0f, -10f);
				if (!fly.inputWithDiagonals.Value.pckp)
				{
					stuckCounter = 0;
					fly.LoseAllGrasps();
					fly.burrowOrHangSpot = null;
					flockBehavior.formChains = false;
					afraid = 0f;
				}
				else
				{
					flockBehavior.formChains = true;
				}
			}
			else
			{
				localGoal = fly.mainBodyChunk.pos + new Vector2(0f, -10f);
			}
		}
		if (ModManager.MSC && room.world.game.IsArenaSession && room.world.game.GetArenaGameSession.arenaSitting.gameTypeSetup.challengeMeta != null)
		{
			ChallengeInformation.ChallengeMeta challengeMeta = room.world.game.GetArenaGameSession.arenaSitting.gameTypeSetup.challengeMeta;
			if (challengeMeta.fly_max_y != -1 && localGoal.y > (float)challengeMeta.fly_max_y)
			{
				localGoal.y = challengeMeta.fly_max_y;
			}
			if (challengeMeta.fly_min_y != -1 && localGoal.y < (float)challengeMeta.fly_min_y)
			{
				localGoal.y = challengeMeta.fly_min_y;
			}
		}
		if (afraid >= 1f && room.GetTile(FlyPos).hive)
		{
			fly.mainBodyChunk.vel.y -= 1f;
		}
		bool flag = fly.safariControlled && fly.inputWithDiagonals.HasValue && fly.inputWithDiagonals.Value.pckp;
		bool flag2 = !ModManager.MSC || !room.world.game.IsArenaSession || room.world.game.GetArenaGameSession.arenaSitting.gameTypeSetup.challengeMeta == null || !room.world.game.GetArenaGameSession.arenaSitting.gameTypeSetup.challengeMeta.fly_no_burrow;
		if (fly.mainBodyChunk.ContactPoint.y == -1 && (afraid >= 0.75f || flag) && room.GetTile(FlyPos).hive && flag2)
		{
			ChangeBehavior(Behavior.Burrow);
			fly.burrowOrHangSpot = FlyPos;
		}
		if (Random.value < 0.0125f && behavior != Behavior.Flee && room.world.rainCycle.RainApproaching < 0.5f && (!ModManager.MMF || RoomNotACycleHazard(room)))
		{
			fleeFromRain = true;
		}
		if (Random.value < 1f / 60f && behavior != Behavior.Chain && fly.grasps[0] != null)
		{
			fly.LoseAllGrasps();
		}
		if (behavior != Behavior.Chain && behavior != Behavior.Burrow)
		{
			if (stuckCounter < 40)
			{
				if (!Custom.DistLess(FlyPos, stuckPos, 40f))
				{
					stuckCounter = 0;
					stuckPos = FlyPos;
				}
				else
				{
					stuckCounter++;
					if (stuckCounter == 40)
					{
						stuckCounter = 0;
						for (int j = 0; j < 8; j++)
						{
							if (room.GetTile(room.GetTilePosition(FlyPos) + Custom.eightDirections[j]).Terrain == Room.Tile.TerrainType.Solid)
							{
								stuckCounter = 120;
								break;
							}
						}
					}
				}
			}
			else
			{
				stuckCounter--;
			}
		}
		afraid = Mathf.Clamp(afraid - 0.003125f, 0f, 1f);
	}

	private void IdleUpdate()
	{
		GenericFlightUpdate();
		if (otherFly != null)
		{
			GravitateToFlock();
		}
		if (noSwarmCounter > 0)
		{
			noSwarmCounter--;
		}
		else if (InActiveSwarmRoom && CurrentFollowDijkstraIsToHive && room.aimap.ExitDistanceForCreature(FlyPos, followingDijkstraMap, Template) < 30)
		{
			Vector2 testPos = Vector2.Lerp(FlyPos, localGoal, 0.5f);
			if (ValidSwarmPosition(testPos))
			{
				localGoal = testPos;
				ChangeBehavior(Behavior.Swarm);
			}
		}
		if (!(afraid < 0.5f) || !flockBehavior.formChains || fleeFromRain)
		{
			return;
		}
		bool flag = ((!ModManager.MSC) ? (otherFly != null && otherFly.firstChunk.pos.y > room.FloatWaterLevel(otherFly.firstChunk.pos.x) + 40f) : (otherFly != null && !room.PointSubmerged(otherFly.firstChunk.pos, -40f)));
		if (otherFly != null && Random.value < 1f / 3f && CanIHangFromThisFly(otherFly) && room.GetTile(otherFly.mainBodyChunk.pos + new Vector2(0f, -20f)).Terrain != Room.Tile.TerrainType.Solid && flag)
		{
			if (Custom.DistLess(otherFly.mainBodyChunk.pos, this.fly.mainBodyChunk.pos, 20f))
			{
				ChangeBehavior(Behavior.Chain);
				this.fly.Grab(otherFly, 0, 0, Creature.Grasp.Shareability.NonExclusive, 1f, overrideEquallyDominant: false, pacifying: false);
				this.fly.CheckChainForLoops();
				room.PlaySound(SoundID.Bat_Attatch_To_Chain, this.fly.mainBodyChunk);
				Fly fly = otherFly;
				while (true)
				{
					if (fly.graphicsModule != null)
					{
						fly.graphicsModule.BringSpritesToFront();
					}
					if (fly.grasps[0] != null && fly.grasps[0].grabbed is Fly)
					{
						fly = fly.grasps[0].grabbed as Fly;
						continue;
					}
					break;
				}
			}
			else
			{
				ChangeBehavior(Behavior.Idle);
				localGoal = otherFly.mainBodyChunk.pos;
				followingDijkstraMap = -1;
			}
		}
		else if (Random.value < 0.125f && ChainTile(room.GetTilePosition(FlyPos)))
		{
			ChangeBehavior(Behavior.Chain);
			if (room.GetTile(FlyPos).horizontalBeam)
			{
				this.fly.burrowOrHangSpot = new Vector2(FlyPos.x, room.MiddleOfTile(FlyPos).y - 4f);
			}
			else if (!room.GetTile(FlyPos).verticalBeam && room.GetTile(FlyPos + new Vector2(0f, 20f)).verticalBeam)
			{
				this.fly.burrowOrHangSpot = room.MiddleOfTile(FlyPos) + new Vector2(0f, 10f);
			}
			else
			{
				this.fly.burrowOrHangSpot = new Vector2(FlyPos.x, room.MiddleOfTile(FlyPos).y + 10f);
			}
		}
	}

	private void SwarmUpdate()
	{
		localGoal.y += 0.1f;
		if (otherFly != null)
		{
			GravitateToFlock();
		}
		if (FlyPos.y < lowestOtherPos.y && ValidSwarmPosition(FlyPos))
		{
			lowestOtherPos = FlyPos;
		}
		Vector2 vector = new Vector2(Random.value * room.PixelWidth, Random.value * lowestOtherPos.y);
		if (ValidSwarmPosition(vector) && room.VisualContact(FlyPos, vector))
		{
			lowestOtherPos = vector;
		}
		if (!ValidSwarmPosition(localGoal) || room.GetTile(localGoal + new Vector2(0f, 15f)).Terrain == Room.Tile.TerrainType.Solid || localGoal.y > room.PixelHeight)
		{
			if (ValidSwarmPosition(lowestOtherPos))
			{
				localGoal = lowestOtherPos + new Vector2(Mathf.Lerp(-10f, 10f, Random.value), -15f);
				lowestOtherPos = new Vector2(localGoal.x, room.PixelHeight);
			}
			else
			{
				ChangeBehavior(Behavior.Idle);
			}
		}
		if (!Custom.DistLess(FlyPos, localGoal, 400f) || !room.VisualContact(FlyPos, localGoal))
		{
			ChangeBehavior(Behavior.Idle);
		}
	}

	private void FleeUpdate()
	{
		TrackedThreat trackedThreat = null;
		float num = 0f;
		foreach (TrackedThreat threat in threats)
		{
			if (threat.Severity / Vector2.Distance(FlyPos, threat.lastSeenPos) > num)
			{
				num = threat.Severity / Vector2.Distance(FlyPos, threat.lastSeenPos);
				trackedThreat = threat;
			}
		}
		if (panicFlee > 0 && !DoingSpecificAnimation && trackedThreat != null && !Stuck)
		{
			fly.dir = Custom.DirVec(trackedThreat.lastSeenPos, FlyPos);
			fly.movMode = Fly.MovementMode.Panic;
			if (fly.mainBodyChunk.ContactPoint.x != 0 || fly.mainBodyChunk.ContactPoint.y != 0 || !room.IsPositionInsideBoundries(fly.abstractCreature.pos.Tile))
			{
				panicFlee = 0;
			}
		}
		else
		{
			float num2 = float.MinValue;
			for (int i = 0; i < fly.abstractCreature.Room.NodesRelevantToCreature(Template); i++)
			{
				float num3 = 0f;
				foreach (TrackedThreat threat2 in threats)
				{
					num3 += (float)(room.aimap.ExitDistanceForCreature(threat2.lastSeenPos, i, Template) - room.aimap.ExitDistanceForCreature(FlyPos, i, Template)) * Mathf.Lerp(0.5f, 1f, threat2.Severity);
					if (room.aimap.ExitDistanceForCreature(threat2.lastSeenPos, i, Template) < room.aimap.ExitDistanceForCreature(FlyPos, i, Template))
					{
						num3 -= 1000f;
					}
				}
				if (fly.abstractCreature.Room.nodes[fly.abstractCreature.Room.CreatureSpecificToCommonNodeIndex(i, Template)].type == AbstractRoomNode.Type.Den)
				{
					num3 = float.MinValue;
				}
				else if (fly.abstractCreature.Room.nodes[fly.abstractCreature.Room.CreatureSpecificToCommonNodeIndex(i, Template)].type == AbstractRoomNode.Type.Exit)
				{
					num3 -= 20f;
				}
				if (num3 > num2)
				{
					num2 = num3;
					followingDijkstraMap = i;
				}
			}
		}
		GenericFlightUpdate();
		if (InActiveSwarmRoom && !Stuck && trackedThreat != null && !trackedThreat.crit.creatureTemplate.canFly && trackedThreat.currentlyScaredOf && fly.dir.y > -0.5f && Custom.DirVec(FlyPos, trackedThreat.lastSeenPos).y < 0.5f)
		{
			int num4 = 0;
			IntVector2 tilePosition = room.GetTilePosition(FlyPos);
			for (int j = tilePosition.y; j < room.TileHeight && room.GetTile(tilePosition.x, j).Terrain != Room.Tile.TerrainType.Solid; j++)
			{
				num4++;
			}
			if (num4 >= 3)
			{
				Vector2 vector = Custom.DirVec(FlyPos, localGoal);
				vector = Vector3.Slerp(vector, new Vector2(0f, 1f), 0.2f);
				localGoal = FlyPos + vector * localGoal.magnitude;
			}
		}
		if (!(afraid >= 1f))
		{
			return;
		}
		if (room.GetTile(FlyPos).hive)
		{
			fly.mainBodyChunk.vel.y -= 1f;
			return;
		}
		int num5 = fly.abstractCreature.pos.y;
		while (num5 > fly.abstractCreature.pos.y - 10 && room.GetTile(fly.abstractCreature.pos.x, num5).Terrain != Room.Tile.TerrainType.Solid)
		{
			if (room.GetTile(fly.abstractCreature.pos.x, num5).hive)
			{
				bool flag = true;
				IntRect rect = new IntRect(fly.abstractCreature.pos.x - 2, num5 - 1, fly.abstractCreature.pos.x + 2, fly.abstractCreature.pos.y + 1);
				foreach (TrackedThreat threat3 in threats)
				{
					if (Custom.InsideRect(room.GetTilePosition(threat3.lastSeenPos), rect))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					dropStatus = DropStatus.Dropping;
				}
			}
			num5--;
		}
	}

	private void DropUpdate()
	{
		if (fly.mainBodyChunk.ContactPoint.y < 0 || room.GetTile(FlyPos + new Vector2(0f, -20f)).AnyWater || FlyPos.y < -50f)
		{
			dropStatus = DropStatus.HasDropped;
			if (room.GetTile(FlyPos).hive)
			{
				ChangeBehavior(Behavior.Burrow);
				fly.burrowOrHangSpot = FlyPos;
			}
			else
			{
				ChangeBehavior(Behavior.Idle);
			}
		}
		fly.mainBodyChunk.vel.x *= 0.6f;
	}

	private void FleeFromRainUpdate()
	{
		afraid = 2f;
		if (room.hives.Length != 0)
		{
			if (followingDijkstraMap < room.abstractRoom.nodes.Length)
			{
				int num = -1;
				int num2 = int.MaxValue;
				for (int i = room.exitAndDenIndex.Length; i < room.exitAndDenIndex.Length + room.hives.Length; i++)
				{
					if (room.aimap.ExitDistanceForCreature(fly.abstractCreature.pos.Tile, i, Template) < num2)
					{
						num2 = room.aimap.ExitDistanceForCreature(fly.abstractCreature.pos.Tile, i, Template);
						num = i;
					}
				}
				if (num != -1)
				{
					leaveRoomDijkstra = -1;
					followingDijkstraMap = num;
				}
			}
		}
		else
		{
			leaveRoomDijkstra = room.abstractRoom.ExitIndex(room.game.world.fliesWorldAI.MigrationDirection(fly.abstractCreature.pos));
			followingDijkstraMap = leaveRoomDijkstra;
		}
		if (followingDijkstraMap >= 0)
		{
			localGoal = ProgressLocalGoalAlongDijkstraMap(localGoal, followingDijkstraMap);
		}
	}

	private void HangInChainUpdate()
	{
		if (fly.grasps[0] == null)
		{
			if (!fly.burrowOrHangSpot.HasValue)
			{
				ChangeBehavior(Behavior.Idle);
				return;
			}
			float num = Vector2.Distance(fly.mainBodyChunk.pos, fly.burrowOrHangSpot.Value);
			if (num > 8f)
			{
				Vector2 vector = Custom.DirVec(fly.mainBodyChunk.pos, fly.burrowOrHangSpot.Value);
				fly.mainBodyChunk.pos -= (8f - num) * vector;
				fly.mainBodyChunk.vel -= (8f - num) * vector;
			}
			if ((Random.value < 0.05f && afraid > 1.9f) || (Random.value < 1f / 120f && fly.LastInChain() == fly))
			{
				ChangeBehavior(Behavior.Idle);
			}
			if (otherFly != null && otherFly.AI.behavior == Behavior.Chain && otherFly.grasps[0] == null && Custom.DistLess(otherFly.mainBodyChunk.pos, fly.mainBodyChunk.pos, 30f))
			{
				ChangeBehavior(Behavior.Idle);
			}
		}
		else if (!(fly.grasps[0].grabbed is Fly) || (Random.value < 1f / 140f && fly.FirstInChain().movMode != Fly.MovementMode.Hang) || (Random.value < 0.05f && (float)panicFlee > 0f) || (Random.value < 0.0035714286f && afraid == 2f) || leaveRoomDijkstra > -1 || Random.value < 0.0002f)
		{
			ChangeBehavior(Behavior.Idle);
		}
		else if (Random.value < 0.004166667f && fly.LastInChain() == fly && fly.FirstInChain() == fly.grasps[0].grabbed)
		{
			ChangeBehavior(Behavior.Idle);
		}
		bool flag = ((!ModManager.MSC) ? (fly.firstChunk.pos.y < room.FloatWaterLevel(fly.firstChunk.pos.x) + 10f) : room.PointSubmerged(fly.firstChunk.pos, -10f));
		if (fly.firstChunk.ContactPoint.y < 0 || flag || fleeFromRain)
		{
			ChangeBehavior(Behavior.Idle);
		}
	}

	private void GenericFlightUpdate()
	{
		UpdateFollowDijsktra();
		if (followingDijkstraMap >= 0)
		{
			localGoal = ProgressLocalGoalAlongDijkstraMap(localGoal, followingDijkstraMap);
		}
	}

	public void LeaveRoom(WorldCoordinate coord)
	{
		if (coord.room == room.abstractRoom.index)
		{
			leaveRoomDijkstra = room.abstractRoom.CommonToCreatureSpecificNodeIndex(coord.abstractNode, Template);
		}
		else
		{
			leaveRoomDijkstra = room.abstractRoom.CommonToCreatureSpecificNodeIndex(room.game.world.NodeInALeadingToB(room.abstractRoom.index, coord.room).abstractNode, Template);
		}
	}

	private void GravitateToFlock()
	{
		if (otherFly == null || flockBehavior.collectiveMovement == 0f || getBackToGround > 0)
		{
			return;
		}
		Vector2 flyPos = FlyPos;
		if (otherFly.AI.behavior == Behavior.Flee)
		{
			flyPos = FlyPos + Custom.DirVec(otherFly.mainBodyChunk.pos, FlyPos) * 60f;
			flyPos += Custom.DirVec(otherFly.mainBodyChunk.pos, otherFly.localGoal) * 30f;
		}
		else
		{
			flyPos = Vector2.Lerp(otherFly.mainBodyChunk.pos, otherFly.localGoal, flockBehavior.exaggerateDirectionOfOthers);
			if (Custom.DistLess(flyPos, otherFly.mainBodyChunk.pos, flockBehavior.distanceToOthers))
			{
				flyPos = otherFly.mainBodyChunk.pos + Custom.DirVec(otherFly.mainBodyChunk.pos, flyPos) * flockBehavior.distanceToOthers;
			}
			if (Custom.DistLess(flyPos, otherFly.localGoal, flockBehavior.distanceToOthers))
			{
				flyPos = otherFly.localGoal + Custom.DirVec(otherFly.localGoal, flyPos) * flockBehavior.distanceToOthers;
			}
		}
		flyPos = Vector2.Lerp(localGoal, flyPos, Mathf.InverseLerp(Mathf.Lerp(50f, 300f, flockBehavior.collectiveMovement), 40f, Vector2.Distance(FlyPos, otherFly.mainBodyChunk.pos) * flockBehavior.collectiveMovement));
		if (room.aimap.getTerrainProximity(flyPos) >= room.aimap.getTerrainProximity(FlyPos) && VisualContact(flyPos, seeIfInsectFlight: true))
		{
			localGoal = flyPos;
			if (otherFly.AI.followingDijkstraMap >= 0 && Mathf.Pow(Random.value, 0.5f) < flockBehavior.collectiveMovement && room.aimap.ExitDistanceForCreature(room.GetTilePosition(FlyPos), otherFly.AI.followingDijkstraMap, Template) > 10)
			{
				followingDijkstraMap = otherFly.AI.followingDijkstraMap;
			}
		}
	}

	private bool ValidSwarmPosition(Vector2 testPos)
	{
		if (room.aimap.getTerrainProximity(testPos) > 2 && room.aimap.getTerrainProximity(testPos) < 15 && room.aimap.getAItile(localGoal).smoothedFloorAltitude <= 8)
		{
			return room.aimap.getAItile(localGoal).floorAltitude <= 12;
		}
		return false;
	}

	private bool ChainTile(IntVector2 tile)
	{
		if (room.GetTile(tile.x, tile.y + 1).Terrain != Room.Tile.TerrainType.Solid && !room.GetTile(tile).horizontalBeam && (!room.GetTile(tile.x, tile.y + 1).verticalBeam || room.GetTile(tile).verticalBeam))
		{
			return false;
		}
		for (int num = tile.y; num > tile.y - 5; num--)
		{
			if (room.GetTile(tile.x, num).Terrain == Room.Tile.TerrainType.Solid || room.GetTile(tile.x, num).AnyWater)
			{
				return false;
			}
		}
		return true;
	}

	private bool CanIHangFromThisFly(Fly potentialHangFly)
	{
		if (potentialHangFly.AI.behavior != Behavior.Chain || potentialHangFly.NextInChain() != null)
		{
			return false;
		}
		if (potentialHangFly.grasps[0] != null && potentialHangFly.grasps[0].grabbed == fly)
		{
			return false;
		}
		if (potentialHangFly.LastInChain() != potentialHangFly)
		{
			return false;
		}
		if (!potentialHangFly.FirstInChain().burrowOrHangSpot.HasValue || potentialHangFly.FirstInChain().AI.behavior != Behavior.Chain)
		{
			return false;
		}
		return true;
	}

	private void ConsiderOtherFly()
	{
		if (otherFly.mainBodyChunk.pos.y < lowestOtherPos.y && ValidSwarmPosition(otherFly.mainBodyChunk.pos))
		{
			lowestOtherPos = otherFly.mainBodyChunk.pos;
		}
		if (Custom.DistLess(otherFly.localGoal, localGoal, flockBehavior.distanceToOthers))
		{
			Vector2 vector = otherFly.localGoal + Custom.DirVec(otherFly.localGoal, localGoal) * flockBehavior.distanceToOthers;
			if (room.aimap.getTerrainProximity(vector) > 1 && VisualContact(vector, seeIfInsectFlight: true))
			{
				localGoal = vector;
			}
		}
		if (otherFly.AI.flockBehavior.Dominance > flockBehavior.Dominance)
		{
			flockBehavior = otherFly.AI.flockBehavior;
		}
		if (otherFly.AI.threats.Count > 0)
		{
			ConsiderOtherCreature(otherFly.AI.threats[Random.Range(0, otherFly.AI.threats.Count)].crit);
		}
	}

	public static bool RoomNotACycleHazard(Room room)
	{
		if (!(room.roomSettings.DangerType == RoomRain.DangerType.None) && (!ModManager.MSC || !(room.roomSettings.DangerType == MoreSlugcatsEnums.RoomRainDangerType.Blizzard)))
		{
			return room.roomSettings.RainIntensity < 0.2f;
		}
		return true;
	}

	public void NewRoom()
	{
		localGoal = fly.mainBodyChunk.pos;
		followingDijkstraMap = -1;
		leaveRoomDijkstra = -1;
		lowestOtherPos = new Vector2(localGoal.x, room.PixelHeight);
	}

	private void UpdateFollowDijsktra()
	{
		if (leaveRoomDijkstra >= 0)
		{
			followingDijkstraMap = leaveRoomDijkstra;
		}
		else
		{
			if (followingDijkstraMap >= 0 && (!(behavior == Behavior.Idle) || room.aimap.ExitDistanceForCreature(FlyPos, followingDijkstraMap, Template) >= (CurrentFollowDijkstraIsToHive ? 4 : 15)))
			{
				return;
			}
			if (InActiveSwarmRoom)
			{
				int num = Random.Range(room.exitAndDenIndex.Length, room.abstractRoom.NodesRelevantToCreature(fly.Template));
				if (followingDijkstraMap == num)
				{
					followingDijkstraMap = Random.Range(0, room.abstractRoom.NodesRelevantToCreature(fly.Template));
				}
				else
				{
					followingDijkstraMap = num;
				}
			}
			else
			{
				followingDijkstraMap = Random.Range(0, room.abstractRoom.connections.Length);
			}
		}
	}

	private Vector2 ProgressLocalGoalAlongDijkstraMap(Vector2 currentLocalGoal, int followMap)
	{
		currentLocalGoal = Custom.RestrictInRect(currentLocalGoal, new FloatRect(10f, 10f, room.PixelWidth - 10f, room.PixelHeight - 10f));
		if (!Custom.DistLess(FlyPos, currentLocalGoal, 400f))
		{
			if (room.VisualContact(FlyPos, currentLocalGoal))
			{
				return currentLocalGoal;
			}
			return FlyPos + Custom.DirVec(FlyPos, currentLocalGoal);
		}
		if (room.aimap.ExitDistanceForCreature(room.GetTilePosition(localGoal), followMap, Template) < 1)
		{
			return localGoal;
		}
		int num = int.MaxValue;
		MovementConnection movementConnection = default(MovementConnection);
		foreach (MovementConnection outgoingPath in room.aimap.getAItile(currentLocalGoal).outgoingPaths)
		{
			if (room.aimap.IsConnectionAllowedForCreature(outgoingPath, Template) && room.aimap.ExitDistanceForCreature(outgoingPath.DestTile, followMap, Template) < num && room.aimap.ExitDistanceForCreature(outgoingPath.DestTile, followMap, Template) > -1)
			{
				num = room.aimap.ExitDistanceForCreature(outgoingPath.DestTile, followMap, Template);
				movementConnection = outgoingPath;
			}
		}
		if (movementConnection == default(MovementConnection))
		{
			return FlyPos + Custom.DirVec(FlyPos, currentLocalGoal);
		}
		if (movementConnection.type == MovementConnection.MovementType.ShortCut)
		{
			return room.MiddleOfTile(movementConnection.startCoord);
		}
		Vector2 vector = room.MiddleOfTile(movementConnection.destinationCoord);
		if (!room.VisualContact(FlyPos, vector))
		{
			if (room.VisualContact(FlyPos, currentLocalGoal))
			{
				return currentLocalGoal;
			}
			return FlyPos + Custom.DirVec(FlyPos, currentLocalGoal);
		}
		return vector;
	}

	private bool VisualContact(Vector2 point, bool seeIfInsectFlight)
	{
		if (behavior == Behavior.Swarm && !Custom.DistLess(FlyPos, point, 60f))
		{
			return false;
		}
		return room.VisualContact(FlyPos, point);
	}

	private static float AfraidDistance(CreatureTemplate crit)
	{
		if (StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Fly).CreatureRelationship(crit).type != CreatureTemplate.Relationship.Type.Afraid)
		{
			return 0f;
		}
		return Mathf.Lerp(50f, 600f, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Fly).CreatureRelationship(crit).intensity);
	}

	private void UpdateThreats()
	{
		if (panicFlee > 0)
		{
			panicFlee--;
		}
		if (threats.Count > 0)
		{
			float num = 0f;
			foreach (TrackedThreat threat in threats)
			{
				num += 600f * threat.Severity / Vector2.Distance(threat.lastSeenPos, FlyPos);
				if (behavior == Behavior.Chain && threat.lastSeenPos.y < FlyPos.y - 80f)
				{
					num /= 5f;
				}
				else if (panicFlee < 1 && threat.visualContact && Custom.DistLess(FlyPos, threat.lastSeenPos, AfraidDistance(threat.crit.creatureTemplate) / 3f))
				{
					panicFlee = 1;
				}
			}
			if (num > 1f)
			{
				afraid += num / 100f;
			}
			else
			{
				afraid -= 0.0125f;
			}
			afraid = Mathf.Clamp(afraid, (num > 1f) ? 1f : 0f, 2f);
			if (panicFlee > 0)
			{
				afraid = 2f;
			}
		}
		else if (!fleeFromRain)
		{
			afraid = 0f;
		}
		if (fly.mainBodyChunk.ContactPoint.x != 0 || fly.mainBodyChunk.ContactPoint.y != 0)
		{
			panicFlee = 0;
		}
		if (room.abstractRoom.creatures.Count > 0)
		{
			ConsiderOtherCreature(room.abstractRoom.creatures[Random.Range(0, room.abstractRoom.creatures.Count)]);
		}
		for (int num2 = threats.Count - 1; num2 >= 0; num2--)
		{
			if (threats[num2].deleteMeNextFrame)
			{
				threats.RemoveAt(num2);
			}
			else
			{
				threats[num2].Update();
			}
		}
	}

	private void ConsiderOtherCreature(AbstractCreature crit)
	{
		if (crit.realizedCreature == null || !(fly.abstractCreature.creatureTemplate.CreatureRelationship(crit.creatureTemplate).type == CreatureTemplate.Relationship.Type.Afraid))
		{
			return;
		}
		bool flag = true;
		foreach (TrackedThreat threat in threats)
		{
			if (threat.crit == crit)
			{
				flag = false;
				break;
			}
		}
		if (!flag)
		{
			return;
		}
		BodyChunk bodyChunk = crit.realizedCreature.bodyChunks[Random.Range(0, crit.realizedCreature.bodyChunks.Length)];
		if (Custom.DistLess(FlyPos, bodyChunk.pos, AfraidDistance(crit.creatureTemplate)) && VisualContact(bodyChunk.pos, seeIfInsectFlight: false))
		{
			threats.Add(new TrackedThreat(this, crit));
			if (behavior != Behavior.Chain)
			{
				ChangeBehavior(Behavior.Flee);
				panicFlee += 10 + (int)(AfraidDistance(crit.creatureTemplate) - Vector2.Distance(FlyPos, bodyChunk.pos));
			}
		}
	}

	public void Burrowed()
	{
		ChangeBehavior(Behavior.Idle);
		dropStatus = DropStatus.HaventDropped;
		afraid = 0f;
		threats.Clear();
	}
}
