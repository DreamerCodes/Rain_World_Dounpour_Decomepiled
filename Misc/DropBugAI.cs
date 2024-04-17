using System;
using System.Collections.Generic;
using Noise;
using RWCustom;
using UnityEngine;

public class DropBugAI : ArtificialIntelligence, IUseARelationshipTracker, IAINoiseReaction, IUseItemTracker
{
	public class Behavior : ExtEnum<Behavior>
	{
		public static readonly Behavior Idle = new Behavior("Idle", register: true);

		public static readonly Behavior Flee = new Behavior("Flee", register: true);

		public static readonly Behavior Hunt = new Behavior("Hunt", register: true);

		public static readonly Behavior EscapeRain = new Behavior("EscapeRain", register: true);

		public static readonly Behavior ReturnPrey = new Behavior("ReturnPrey", register: true);

		public static readonly Behavior GetUnstuck = new Behavior("GetUnstuck", register: true);

		public static readonly Behavior SitInCeiling = new Behavior("SitInCeiling", register: true);

		public static readonly Behavior LeaveRoom = new Behavior("LeaveRoom", register: true);

		public static readonly Behavior Injured = new Behavior("Injured", register: true);

		public Behavior(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class CeilingSitModule : AIModule
	{
		public DebugSprite dbSpr;

		public WorldCoordinate ceilingPos;

		public WorldCoordinate stayAwayFromPos;

		public IntVector2 randomAttractor;

		public int winStreak;

		public int spearDanger;

		public int roomSize;

		public float dropDelay;

		private bool anywhereToSit;

		private int anywhereToSitCounter;

		private bool anticipateDrop;

		private bool lastAnticipateDrop;

		public List<AbstractCreature> otherWigsInRoom;

		public Room room => AI.creature.Room.realizedRoom;

		public DropBugAI bugAI => AI as DropBugAI;

		public bool AnyWhereToSitInRoom
		{
			get
			{
				if (room == null)
				{
					return false;
				}
				if (!anywhereToSit)
				{
					return anywhereToSitCounter < room.ceilingTiles.Length;
				}
				return true;
			}
		}

		public bool SittingInCeiling
		{
			get
			{
				if (bugAI.bug.safariControlled)
				{
					if (bugAI.behavior == Behavior.SitInCeiling && Custom.DistLess(bugAI.bug.bodyChunks[1].pos, room.MiddleOfTile(ceilingPos), 40f))
					{
						return ValidCeilingSpotControlled(room, ceilingPos.Tile);
					}
					return false;
				}
				if (bugAI.behavior == Behavior.SitInCeiling && Custom.DistLess(bugAI.bug.bodyChunks[1].pos, room.MiddleOfTile(ceilingPos), 40f) && AI.pathFinder.GetDestination == ceilingPos)
				{
					return ValidCeilingSpot(room, ceilingPos.Tile);
				}
				return false;
			}
		}

		public CeilingSitModule(ArtificialIntelligence AI)
			: base(AI)
		{
			otherWigsInRoom = new List<AbstractCreature>();
		}

		public override void NewRoom(Room newRoom)
		{
			base.NewRoom(newRoom);
			ceilingPos = new WorldCoordinate(newRoom.abstractRoom.index, UnityEngine.Random.Range(0, newRoom.TileWidth), UnityEngine.Random.Range(0, newRoom.TileHeight), -1);
			stayAwayFromPos = new WorldCoordinate(newRoom.abstractRoom.index, UnityEngine.Random.Range(0, newRoom.TileWidth), UnityEngine.Random.Range(0, newRoom.TileHeight), -1);
			randomAttractor = new IntVector2(UnityEngine.Random.Range(0, newRoom.TileWidth), UnityEngine.Random.Range(0, newRoom.TileHeight));
			roomSize = Mathf.Max(newRoom.TileWidth, newRoom.TileHeight);
			anywhereToSitCounter = 0;
			anywhereToSit = false;
			UpdateOtherWigsList(newRoom, recurse: true);
		}

		public void UpdateOtherWigsList(Room newRoom, bool recurse)
		{
			otherWigsInRoom.Clear();
			for (int i = 0; i < newRoom.abstractRoom.creatures.Count; i++)
			{
				if (newRoom.abstractRoom.creatures[i] != AI.creature && newRoom.abstractRoom.creatures[i].creatureTemplate.type == CreatureTemplate.Type.DropBug)
				{
					otherWigsInRoom.Add(newRoom.abstractRoom.creatures[i]);
					if (recurse && newRoom.abstractRoom.creatures[i].realizedCreature != null && newRoom.abstractRoom.creatures[i].realizedCreature.room != null)
					{
						(newRoom.abstractRoom.creatures[i].realizedCreature as DropBug).AI.ceilingModule.UpdateOtherWigsList(newRoom.abstractRoom.creatures[i].realizedCreature.room, recurse: false);
					}
				}
			}
		}

		public void Dislodge()
		{
			if (SittingInCeiling)
			{
				stayAwayFromPos = ceilingPos;
				ceilingPos = new WorldCoordinate(room.abstractRoom.index, UnityEngine.Random.Range(0, room.TileWidth), UnityEngine.Random.Range(0, room.TileHeight), -1);
				randomAttractor = new IntVector2(UnityEngine.Random.Range(0, room.TileWidth), UnityEngine.Random.Range(0, room.TileHeight));
				winStreak = 0;
				bugAI.utilityComparer.GetUtilityTracker(bugAI.ceilingModule).smoothedUtility = 0f;
				bugAI.bug.JumpFromCeiling(null, new Vector2(0f, -1f));
			}
		}

		public override void Update()
		{
			base.Update();
			if (room == null)
			{
				return;
			}
			if (anywhereToSitCounter < room.ceilingTiles.Length)
			{
				if (ValidCeilingSpot(room, room.ceilingTiles[anywhereToSitCounter]))
				{
					anywhereToSit = true;
					anywhereToSitCounter = room.ceilingTiles.Length;
				}
				else
				{
					anywhereToSitCounter++;
				}
			}
			if (bugAI.bug.safariControlled && bugAI.bug.inputWithoutDiagonals.HasValue && bugAI.bug.inputWithoutDiagonals.Value.pckp && ValidCeilingSpotControlled(room, bugAI.bug.abstractCreature.pos.Tile))
			{
				anywhereToSit = true;
				anywhereToSitCounter = room.ceilingTiles.Length;
				ceilingPos = bugAI.bug.abstractCreature.pos;
			}
			if (SittingInCeiling)
			{
				SitUpdate();
				return;
			}
			if (room.ceilingTiles.Length != 0)
			{
				WorldCoordinate worldCoordinate = room.GetWorldCoordinate(room.ceilingTiles[UnityEngine.Random.Range(0, room.ceilingTiles.Length)]);
				if (DynamicCeilingSpotScore(worldCoordinate) < DynamicCeilingSpotScore(ceilingPos))
				{
					ceilingPos = worldCoordinate;
					winStreak = 0;
				}
				else
				{
					winStreak++;
				}
			}
			if (!ValidCeilingSpot(room, ceilingPos.Tile))
			{
				winStreak = 0;
			}
		}

		public void SitUpdate()
		{
			if (bugAI.bug.safariControlled)
			{
				return;
			}
			lastAnticipateDrop = anticipateDrop;
			anticipateDrop = false;
			bool flag = false;
			for (int i = 0; i < AI.tracker.CreaturesCount; i++)
			{
				if (!AI.tracker.GetRep(i).VisualContact)
				{
					continue;
				}
				if ((AI.tracker.GetRep(i).representedCreature.creatureTemplate.type == CreatureTemplate.Type.Slugcat || AI.tracker.GetRep(i).representedCreature.creatureTemplate.type == CreatureTemplate.Type.Scavenger) && Mathf.Abs(bugAI.bug.mainBodyChunk.pos.x - AI.tracker.GetRep(i).representedCreature.realizedCreature.mainBodyChunk.pos.x) < 200f && Mathf.Abs(bugAI.bug.mainBodyChunk.pos.y - AI.tracker.GetRep(i).representedCreature.realizedCreature.mainBodyChunk.pos.y) < 30f)
				{
					for (int j = 0; j < AI.tracker.GetRep(i).representedCreature.realizedCreature.grasps.Length; j++)
					{
						if (AI.tracker.GetRep(i).representedCreature.realizedCreature.grasps[j] != null && AI.tracker.GetRep(i).representedCreature.realizedCreature.grasps[j].grabbed is Spear)
						{
							flag = true;
							break;
						}
					}
				}
				if (!(AI.tracker.GetRep(i).dynamicRelationship.currentRelationship.type == CreatureTemplate.Relationship.Type.Eats) || !(Mathf.Abs(bugAI.bug.mainBodyChunk.pos.x - AI.tracker.GetRep(i).representedCreature.realizedCreature.mainBodyChunk.pos.x) < 100f) || !(bugAI.bug.mainBodyChunk.pos.y > AI.tracker.GetRep(i).representedCreature.realizedCreature.mainBodyChunk.pos.y))
				{
					continue;
				}
				BodyChunk bodyChunk = AI.tracker.GetRep(i).representedCreature.realizedCreature.bodyChunks[UnityEngine.Random.Range(0, AI.tracker.GetRep(i).representedCreature.realizedCreature.bodyChunks.Length)];
				if (!room.VisualContact(bugAI.bug.mainBodyChunk.pos, bodyChunk.pos))
				{
					continue;
				}
				Vector2 vector = bodyChunk.pos + Vector2.ClampMagnitude(bodyChunk.pos - bodyChunk.lastPos, 6f) * Custom.LerpMap(Vector2.Distance(bugAI.bug.mainBodyChunk.pos, bodyChunk.pos), 40f, 500f, 0f, 30f, 0.8f);
				if (room.GetTilePosition(bodyChunk.pos).x == ceilingPos.x)
				{
					if (!room.VisualContact(bugAI.bug.mainBodyChunk.pos, vector))
					{
						vector = bodyChunk.pos;
					}
					if (room.VisualContact(bugAI.bug.mainBodyChunk.pos, vector))
					{
						dropDelay += Custom.LerpMap(bugAI.bug.mainBodyChunk.pos.y - vector.y, 50f, 180f, 1f / 12f, 1f, 2f);
						if (dropDelay >= 1f)
						{
							JumpFromCeiling(bodyChunk, Custom.DirVec(bugAI.bug.mainBodyChunk.pos, vector));
							return;
						}
					}
				}
				else if (Math.Abs(room.GetTilePosition(vector).x - ceilingPos.x) < 2)
				{
					anticipateDrop = true;
				}
			}
			if (anticipateDrop && !lastAnticipateDrop)
			{
				bugAI.bug.AnticipateDrop();
			}
			if (dropDelay > 0f)
			{
				dropDelay += 0.05f;
				if (dropDelay >= 1f)
				{
					JumpFromCeiling(null, new Vector2(0f, -1f));
				}
			}
			if (flag)
			{
				spearDanger++;
				bugAI.bug.dropAnticipation = Mathf.Max(bugAI.bug.dropAnticipation, Mathf.InverseLerp(4f, 20f, spearDanger));
				if (spearDanger > 40)
				{
					Dislodge();
					bugAI.attackCounter = 150;
				}
			}
			else
			{
				spearDanger = 0;
			}
			if (bugAI.utilityComparer.GetSmoothedNonWeightedUtility(bugAI.threatTracker) > 0.95f)
			{
				Dislodge();
			}
		}

		private void JumpFromCeiling(BodyChunk targetChunk, Vector2 attackDir)
		{
			bugAI.bug.JumpFromCeiling(targetChunk, attackDir);
			winStreak = 0;
			stayAwayFromPos = ceilingPos;
			ceilingPos = new WorldCoordinate(room.abstractRoom.index, UnityEngine.Random.Range(0, room.TileWidth), UnityEngine.Random.Range(0, room.TileHeight), -1);
			randomAttractor = new IntVector2(UnityEngine.Random.Range(0, room.TileWidth), UnityEngine.Random.Range(0, room.TileHeight));
			bugAI.attackCounter = 500;
			bugAI.utilityComparer.GetUtilityTracker(bugAI.preyTracker).smoothedUtility = 1f;
			bugAI.utilityComparer.GetUtilityTracker(bugAI.ceilingModule).smoothedUtility = 0f;
			dropDelay = 0f;
			if (targetChunk != null && targetChunk.owner is Creature)
			{
				bugAI.targetCreature = (targetChunk.owner as Creature).abstractCreature;
			}
		}

		public override float Utility()
		{
			if (bugAI.bug.safariControlled && (!bugAI.bug.inputWithoutDiagonals.HasValue || !bugAI.bug.inputWithoutDiagonals.Value.pckp))
			{
				return 0f;
			}
			if (bugAI.bug.grasps[0] != null && bugAI.bug.grasps[0].grabbed is Creature)
			{
				return 0f;
			}
			if (!AnyWhereToSitInRoom)
			{
				return 0f;
			}
			if (SittingInCeiling)
			{
				return 1f;
			}
			if (bugAI.bug.safariControlled && bugAI.bug.inputWithoutDiagonals.HasValue && bugAI.bug.inputWithoutDiagonals.Value.pckp && ValidCeilingSpotControlled(room, bugAI.bug.abstractCreature.pos.Tile))
			{
				return 1f;
			}
			return Mathf.InverseLerp(20f, 40f, winStreak);
		}

		public float DynamicCeilingSpotScore(WorldCoordinate test)
		{
			float num = CeilingSpotScore(room, test.Tile);
			if (num == float.MaxValue)
			{
				return num;
			}
			num -= Custom.LerpMap(test.Tile.FloatDist(bugAI.creature.pos.Tile), 20f, 60f, 100f, 0f);
			num += Custom.LerpMap(test.Tile.FloatDist(stayAwayFromPos.Tile), 0f, 15f, 400f, 0f);
			num -= Custom.LerpMap(test.Tile.FloatDist(randomAttractor), roomSize / 4, roomSize / 2, 150f, 0f);
			for (int i = 0; i < AI.tracker.CreaturesCount; i++)
			{
				if (AI.tracker.GetRep(i).dynamicRelationship.state.alive && !AI.tracker.GetRep(i).representedCreature.creatureTemplate.smallCreature && AI.tracker.GetRep(i).BestGuessForPosition().room == test.room)
				{
					float num2 = 0f;
					if (AI.tracker.GetRep(i).dynamicRelationship.currentRelationship.type != CreatureTemplate.Relationship.Type.Ignores)
					{
						num2 = AI.tracker.GetRep(i).dynamicRelationship.currentRelationship.intensity;
					}
					if (num2 > 0f)
					{
						num += Custom.LerpMap(test.Tile.FloatDist(AI.tracker.GetRep(i).BestGuessForPosition().Tile), 3f + 8f * num2, 2f, 0f, num2 * 70f, 2f);
					}
				}
			}
			for (int j = 0; j < otherWigsInRoom.Count; j++)
			{
				if (otherWigsInRoom[j].realizedCreature != null && (otherWigsInRoom[j].realizedCreature as DropBug).AI.ceilingModule.ceilingPos.room == test.room)
				{
					num += Custom.LerpMap(test.Tile.FloatDist((otherWigsInRoom[j].realizedCreature as DropBug).AI.ceilingModule.ceilingPos.Tile), 50f, 1f, 0f, 1000f, 3f);
				}
			}
			return num;
		}
	}

	public DropBug bug;

	private DebugDestinationVisualizer debugDestinationVisualizer;

	public float currentUtility;

	public int noiseRectionDelay;

	public AbstractCreature targetCreature;

	public Behavior behavior;

	public WorldCoordinate? walkWithBug;

	public Tracker.CreatureRepresentation focusCreature;

	private int idlePosCounter;

	public WorldCoordinate tempIdlePos;

	public CeilingSitModule ceilingModule;

	public bool stayAway;

	public int attackCounter;

	public AbstractPhysicalObject baitItem;

	public override float CurrentPlayerAggression(AbstractCreature player)
	{
		return base.CurrentPlayerAggression(player) * (1f - bug.inCeilingMode * 0.85f) * ((behavior == Behavior.Hunt && base.preyTracker.MostAttractivePrey != null && base.preyTracker.MostAttractivePrey.representedCreature == player) ? 1f : 0.5f);
	}

	public DropBugAI(AbstractCreature creature, World world)
		: base(creature, world)
	{
		bug = creature.realizedCreature as DropBug;
		bug.AI = this;
		AddModule(new StandardPather(this, world, creature));
		base.pathFinder.stepsPerFrame = 15;
		AddModule(new Tracker(this, 10, 10, 1500, 0.5f, 5, 5, 10));
		AddModule(new ItemTracker(this, 10, 10, 400, 30, stopTrackingCarried: true));
		AddModule(new ThreatTracker(this, 3));
		AddModule(new PreyTracker(this, 5, 1.5f, 5f, 40f, 0.65f));
		AddModule(new RainTracker(this));
		AddModule(new DenFinder(this, creature));
		AddModule(new StuckTracker(this, trackPastPositions: true, trackNotFollowingCurrentGoal: false));
		AddModule(new NoiseTracker(this, base.tracker));
		AddModule(new UtilityComparer(this));
		AddModule(new RelationshipTracker(this, base.tracker));
		AddModule(new InjuryTracker(this, 0.4f));
		ceilingModule = new CeilingSitModule(this);
		AddModule(ceilingModule);
		base.utilityComparer.AddComparedModule(base.threatTracker, null, 1f, 1.1f);
		FloatTweener.FloatTween smoother = new FloatTweener.FloatTweenBasic(FloatTweener.TweenType.Tick, 0.01f);
		base.utilityComparer.AddComparedModule(base.preyTracker, smoother, 1f, 1.1f);
		base.utilityComparer.AddComparedModule(base.rainTracker, null, 1f, 1.1f);
		base.utilityComparer.AddComparedModule(base.stuckTracker, null, 1f, 1.1f);
		smoother = new FloatTweener.FloatTweenBasic(FloatTweener.TweenType.Tick, 1f / 150f);
		base.utilityComparer.AddComparedModule(base.injuryTracker, smoother, 1f, 1.1f);
		smoother = new FloatTweener.FloatTweenUpAndDown(new FloatTweener.FloatTweenBasic(FloatTweener.TweenType.Lerp, 0.2f), new FloatTweener.FloatTweenBasic(FloatTweener.TweenType.Tick, 0.005f));
		base.utilityComparer.AddComparedModule(ceilingModule, smoother, 1f, 1.1f);
		behavior = Behavior.Idle;
	}

	public override void Update()
	{
		base.Update();
		if (bug.room == null)
		{
			return;
		}
		if (baitItem != null)
		{
			if (behavior != Behavior.SitInCeiling || baitItem.realizedObject == null || baitItem.realizedObject.room != bug.room)
			{
				baitItem = null;
			}
			else if (baitItem.realizedObject.grabbedBy.Count > 0)
			{
				ceilingModule.dropDelay += 0.3f;
				baitItem = null;
			}
		}
		base.pathFinder.walkPastPointOfNoReturn = stranded || !base.denFinder.GetDenPosition().HasValue || !base.pathFinder.CoordinatePossibleToGetBackFrom(base.denFinder.GetDenPosition().Value) || base.threatTracker.Utility() > 0.95f;
		if (bug.sitting)
		{
			base.noiseTracker.hearingSkill = 1f;
		}
		else
		{
			base.noiseTracker.hearingSkill = 0.3f;
		}
		base.utilityComparer.GetUtilityTracker(base.preyTracker).weight = 0.07f + ((targetCreature != null) ? 0.83f : 0.57f) * Mathf.InverseLerp(0f, 100f, attackCounter);
		base.utilityComparer.GetUtilityTracker(ceilingModule).weight = Mathf.InverseLerp(200f, 0f, attackCounter) * 0.7f;
		if (attackCounter > 0)
		{
			attackCounter--;
		}
		else
		{
			targetCreature = null;
		}
		AIModule aIModule = base.utilityComparer.HighestUtilityModule();
		currentUtility = base.utilityComparer.HighestUtility();
		if (aIModule != null)
		{
			if (aIModule is ThreatTracker)
			{
				behavior = Behavior.Flee;
			}
			else if (aIModule is PreyTracker)
			{
				behavior = Behavior.Hunt;
			}
			else if (aIModule is RainTracker)
			{
				behavior = Behavior.EscapeRain;
			}
			else if (aIModule is StuckTracker)
			{
				behavior = Behavior.GetUnstuck;
			}
			else if (aIModule is CeilingSitModule)
			{
				behavior = Behavior.SitInCeiling;
			}
			else if (aIModule is InjuryTracker)
			{
				behavior = Behavior.Injured;
			}
		}
		if (currentUtility < 0.05f)
		{
			behavior = Behavior.Idle;
		}
		if (currentUtility < 0.6f && !ceilingModule.AnyWhereToSitInRoom)
		{
			currentUtility = 0.6f;
			behavior = Behavior.LeaveRoom;
		}
		if (behavior != Behavior.Flee && bug.grasps[0] != null && bug.grasps[0].grabbed is Creature && DynamicRelationship((bug.grasps[0].grabbed as Creature).abstractCreature).type == CreatureTemplate.Relationship.Type.Eats)
		{
			behavior = Behavior.ReturnPrey;
			currentUtility = 1f;
		}
		if (behavior != Behavior.Idle)
		{
			tempIdlePos = creature.pos;
		}
		if (behavior == Behavior.Injured && base.preyTracker.Utility() > 0.4f && base.preyTracker.MostAttractivePrey != null && creature.pos.room == base.preyTracker.MostAttractivePrey.BestGuessForPosition().room && creature.pos.Tile.FloatDist(base.preyTracker.MostAttractivePrey.BestGuessForPosition().Tile) < 6f)
		{
			behavior = Behavior.Hunt;
			base.utilityComparer.GetUtilityTracker(base.preyTracker).weight = 1f;
		}
		if (behavior == Behavior.GetUnstuck)
		{
			if (UnityEngine.Random.value < 0.02f)
			{
				creature.abstractAI.SetDestination(bug.room.GetWorldCoordinate(bug.mainBodyChunk.pos + Custom.RNV() * 100f));
			}
		}
		else if (behavior == Behavior.LeaveRoom)
		{
			creature.abstractAI.AbstractBehavior(1);
			if (base.pathFinder.GetDestination != creature.abstractAI.MigrationDestination)
			{
				creature.abstractAI.SetDestination(creature.abstractAI.MigrationDestination);
			}
		}
		else if (behavior == Behavior.Idle)
		{
			if (UnityEngine.Random.value < 0.01f)
			{
				creature.abstractAI.AbstractBehavior(1);
			}
			if (!base.pathFinder.CoordinateReachableAndGetbackable(base.pathFinder.GetDestination))
			{
				creature.abstractAI.SetDestination(creature.pos);
			}
			if (walkWithBug.HasValue)
			{
				creature.abstractAI.SetDestination(walkWithBug.Value);
				if (UnityEngine.Random.value < 0.02f || Custom.ManhattanDistance(creature.pos, walkWithBug.Value) < 4)
				{
					walkWithBug = null;
				}
			}
			else if (!creature.abstractAI.WantToMigrate)
			{
				WorldCoordinate coord = new WorldCoordinate(bug.room.abstractRoom.index, UnityEngine.Random.Range(0, bug.room.TileWidth), UnityEngine.Random.Range(0, bug.room.TileHeight), -1);
				if (IdleScore(coord) < IdleScore(tempIdlePos))
				{
					tempIdlePos = coord;
				}
				if (IdleScore(tempIdlePos) < IdleScore(base.pathFinder.GetDestination) + Custom.LerpMap(idlePosCounter, 0f, 300f, 100f, -300f))
				{
					SetDestination(tempIdlePos);
					idlePosCounter = UnityEngine.Random.Range(200, 800);
					tempIdlePos = new WorldCoordinate(bug.room.abstractRoom.index, UnityEngine.Random.Range(0, bug.room.TileWidth), UnityEngine.Random.Range(0, bug.room.TileHeight), -1);
				}
				idlePosCounter--;
			}
			else if (base.pathFinder.GetDestination != creature.abstractAI.MigrationDestination)
			{
				creature.abstractAI.SetDestination(creature.abstractAI.MigrationDestination);
			}
		}
		else if (behavior == Behavior.Hunt)
		{
			if (base.preyTracker.MostAttractivePrey != null)
			{
				focusCreature = base.preyTracker.MostAttractivePrey;
				creature.abstractAI.SetDestination(base.preyTracker.MostAttractivePrey.BestGuessForPosition());
				if (!bug.safariControlled && base.preyTracker.MostAttractivePrey.representedCreature.realizedCreature != null && bug.grasps[0] == null && !bug.jumping && bug.attemptBite == 0f && bug.charging == 0f && bug.Footing && base.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.room == bug.room)
				{
					BodyChunk bodyChunk = base.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.bodyChunks[UnityEngine.Random.Range(0, base.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.bodyChunks.Length)];
					if (Custom.DistLess(bug.mainBodyChunk.pos, bodyChunk.pos, 120f) && (bug.room.aimap.TileAccessibleToCreature(bug.room.GetTilePosition(bug.bodyChunks[1].pos - Custom.DirVec(bug.bodyChunks[1].pos, bodyChunk.pos) * 30f), bug.Template) || bug.room.GetTile(bug.bodyChunks[1].pos - Custom.DirVec(bug.bodyChunks[1].pos, bodyChunk.pos) * 30f).Solid) && bug.room.VisualContact(bug.mainBodyChunk.pos, bodyChunk.pos))
					{
						if (Vector2.Dot((bug.mainBodyChunk.pos - bodyChunk.pos).normalized, (bug.bodyChunks[1].pos - bug.mainBodyChunk.pos).normalized) > 0.2f)
						{
							bug.InitiateJump(bodyChunk);
						}
						else
						{
							bug.mainBodyChunk.vel += Custom.DirVec(bug.mainBodyChunk.pos, bodyChunk.pos) * 2f;
							bug.bodyChunks[1].vel -= Custom.DirVec(bug.mainBodyChunk.pos, bodyChunk.pos) * 2f;
						}
					}
				}
			}
		}
		else if (behavior == Behavior.Flee)
		{
			creature.abstractAI.SetDestination(base.threatTracker.FleeTo(creature.pos, 6, 30, considerLeavingRoom: true));
			focusCreature = base.threatTracker.mostThreateningCreature;
		}
		else if (behavior == Behavior.EscapeRain || behavior == Behavior.ReturnPrey || behavior == Behavior.Injured)
		{
			focusCreature = null;
			if (base.denFinder.GetDenPosition().HasValue)
			{
				creature.abstractAI.SetDestination(base.denFinder.GetDenPosition().Value);
			}
		}
		else if (behavior == Behavior.SitInCeiling)
		{
			creature.abstractAI.SetDestination(ceilingModule.ceilingPos);
			if (!ceilingModule.SittingInCeiling && Custom.DistLess(bug.bodyChunks[1].pos, bug.room.MiddleOfTile(ceilingModule.ceilingPos), 50f) && !Custom.DistLess(bug.bodyChunks[1].pos, bug.room.MiddleOfTile(ceilingModule.ceilingPos), 5f) && bug.room.VisualContact(bug.bodyChunks[1].pos, bug.room.MiddleOfTile(ceilingModule.ceilingPos)))
			{
				bug.bodyChunks[1].pos += Custom.DirVec(bug.bodyChunks[1].pos, bug.room.MiddleOfTile(ceilingModule.ceilingPos));
			}
		}
		if (noiseRectionDelay > 0)
		{
			noiseRectionDelay--;
		}
	}

	private float IdleScore(WorldCoordinate coord)
	{
		if (coord.room != creature.pos.room || !base.pathFinder.CoordinateReachableAndGetbackable(coord))
		{
			return float.MaxValue;
		}
		float num = 1f;
		if (bug.room.aimap.getAItile(coord).narrowSpace)
		{
			num += 300f;
		}
		if (bug.room.GetTile(coord).AnyWater)
		{
			num += 1000f;
		}
		if (bug.room.aimap.getAItile(coord.Tile).acc == AItile.Accessibility.Ceiling)
		{
			num -= 100f;
		}
		if (bug.room.GetTile(coord + new IntVector2(0, 1)).Solid)
		{
			num -= 300f;
		}
		num += Mathf.Max(0f, creature.pos.Tile.FloatDist(coord.Tile) - 80f) / 2f;
		num += (float)bug.room.aimap.getAItile(coord).visibility / 800f;
		num -= Mathf.Max(bug.room.aimap.getAItile(coord).smoothedFloorAltitude, 16f) * 2f;
		if (ceilingModule.ceilingPos.room == coord.room)
		{
			num += Mathf.Max(ceilingModule.ceilingPos.Tile.FloatDist(coord.Tile) - 20f, 0f);
		}
		return num;
	}

	public override bool WantToStayInDenUntilEndOfCycle()
	{
		return base.rainTracker.Utility() > 0.01f;
	}

	public void CollideWithKin(DropBug otherBug)
	{
		if (Custom.ManhattanDistance(creature.pos, otherBug.AI.pathFinder.GetDestination) >= 4)
		{
			if ((otherBug.abstractCreature.personality.dominance > bug.abstractCreature.personality.dominance && !otherBug.sitting) || bug.sitting || otherBug.AI.pathFinder.GetDestination.room != otherBug.room.abstractRoom.index)
			{
				walkWithBug = otherBug.AI.pathFinder.GetDestination;
			}
			if (ceilingModule.SittingInCeiling && creature.personality.dominance < otherBug.abstractCreature.personality.dominance && ceilingModule.ceilingPos.Tile.FloatDist(otherBug.AI.ceilingModule.ceilingPos.Tile) < 3f)
			{
				ceilingModule.Dislodge();
			}
		}
	}

	public override float VisualScore(Vector2 lookAtPoint, float bonus)
	{
		return base.VisualScore(lookAtPoint, bonus) - Mathf.Pow(Mathf.InverseLerp(0.4f, -0.6f, Vector2.Dot((bug.bodyChunks[1].pos - bug.bodyChunks[0].pos).normalized, (bug.bodyChunks[0].pos - lookAtPoint).normalized)), 2f);
	}

	public override Tracker.CreatureRepresentation CreateTrackerRepresentationForCreature(AbstractCreature otherCreature)
	{
		if (otherCreature.creatureTemplate.smallCreature)
		{
			return new Tracker.SimpleCreatureRepresentation(base.tracker, otherCreature, 0f, forgetWhenNotVisible: false);
		}
		return new Tracker.ElaborateCreatureRepresentation(base.tracker, otherCreature, 1f, 3);
	}

	AIModule IUseARelationshipTracker.ModuleToTrackRelationship(CreatureTemplate.Relationship relationship)
	{
		if (relationship.type == CreatureTemplate.Relationship.Type.Afraid)
		{
			return base.threatTracker;
		}
		if (relationship.type == CreatureTemplate.Relationship.Type.Eats || relationship.type == CreatureTemplate.Relationship.Type.Attacks)
		{
			return base.preyTracker;
		}
		return null;
	}

	RelationshipTracker.TrackedCreatureState IUseARelationshipTracker.CreateTrackedCreatureState(RelationshipTracker.DynamicRelationship rel)
	{
		return new RelationshipTracker.TrackedCreatureState();
	}

	CreatureTemplate.Relationship IUseARelationshipTracker.UpdateDynamicRelationship(RelationshipTracker.DynamicRelationship dRelation)
	{
		if (dRelation.trackerRep.representedCreature.creatureTemplate.smallCreature)
		{
			return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f);
		}
		if (dRelation.trackerRep.VisualContact)
		{
			dRelation.state.alive = dRelation.trackerRep.representedCreature.state.alive;
		}
		CreatureTemplate.Relationship result = StaticRelationship(dRelation.trackerRep.representedCreature);
		if (result.type != CreatureTemplate.Relationship.Type.Eats && !dRelation.trackerRep.representedCreature.creatureTemplate.smallCreature && dRelation.trackerRep.representedCreature.realizedCreature != null && dRelation.trackerRep.representedCreature.realizedCreature.dead && dRelation.trackerRep.representedCreature.realizedCreature.TotalMass < bug.TotalMass * 1.15f)
		{
			result = new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, Mathf.InverseLerp(bug.TotalMass * 0.3f, bug.TotalMass * 1.15f, dRelation.trackerRep.representedCreature.realizedCreature.TotalMass) * 0.5f);
		}
		if (result.type == CreatureTemplate.Relationship.Type.Eats)
		{
			if (ceilingModule.SittingInCeiling && dRelation.state.alive && dRelation.trackerRep.representedCreature.creatureTemplate.CreatureRelationship(bug).type == CreatureTemplate.Relationship.Type.Eats && dRelation.trackerRep.BestGuessForPosition().room == creature.pos.room && Math.Abs(dRelation.trackerRep.BestGuessForPosition().Tile.y - (creature.pos.Tile.y - 1)) < 4 && dRelation.trackerRep.BestGuessForPosition().Tile.FloatDist(creature.pos.Tile) < 6f)
			{
				result = new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f);
			}
			else
			{
				result.intensity *= Mathf.InverseLerp(0.1f, 1.5f, dRelation.trackerRep.representedCreature.creatureTemplate.bodySize);
				if (targetCreature != null)
				{
					result.intensity = Mathf.Pow(result.intensity, (dRelation.trackerRep.representedCreature == targetCreature) ? 0.1f : 3f);
				}
			}
		}
		if (result.type == CreatureTemplate.Relationship.Type.Afraid)
		{
			if (!dRelation.state.alive)
			{
				result.intensity = 0f;
			}
			else if (ceilingModule.SittingInCeiling)
			{
				for (int i = 0; i < base.tracker.CreaturesCount; i++)
				{
					if (base.tracker.GetRep(i) != dRelation.trackerRep && dRelation.trackerRep.representedCreature.creatureTemplate.CreatureRelationship(base.tracker.GetRep(i).representedCreature.creatureTemplate).type == CreatureTemplate.Relationship.Type.Eats && base.tracker.GetRep(i).BestGuessForPosition().Tile.FloatDist(dRelation.trackerRep.BestGuessForPosition().Tile) < creature.pos.Tile.FloatDist(dRelation.trackerRep.BestGuessForPosition().Tile))
					{
						result.intensity *= 0.1f;
						break;
					}
				}
			}
		}
		return result;
	}

	public void ReactToNoise(NoiseTracker.TheorizedSource source, InGameNoise noise)
	{
		if (!bug.sitting || noiseRectionDelay > 0 || UnityEngine.Random.value < Mathf.Lerp(1f, 0.1f, bug.inCeilingMode))
		{
			return;
		}
		bug.bodyChunks[1].pos += Custom.RNV() * 4f;
		if (bug.graphicsModule != null)
		{
			for (int i = 0; i < 2; i++)
			{
				(bug.graphicsModule as DropBugGraphics).antennae[i].pos += Custom.DirVec(bug.mainBodyChunk.pos, noise.pos) * UnityEngine.Random.value * Mathf.Lerp(20f, 1f, (bug.graphicsModule as DropBugGraphics).deepCeilingMode);
			}
		}
		noiseRectionDelay = UnityEngine.Random.Range(0, 30 + (int)(30f * bug.inCeilingMode));
	}

	public static bool ValidCeilingSpot(Room room, IntVector2 test)
	{
		if (room.GetTile(test).Terrain != 0 || !room.GetTile(test + new IntVector2(0, 1)).Solid || !room.GetTile(test + new IntVector2(0, 2)).Solid || room.GetTile(test + new IntVector2(0, -1)).Solid)
		{
			return false;
		}
		if (room.aimap.getAItile(test).narrowSpace)
		{
			return false;
		}
		if (room.aimap.getAItile(test).smoothedFloorAltitude < 6 || room.aimap.getAItile(test).floorAltitude < 6)
		{
			return false;
		}
		IntVector2 fallRiskTile = room.aimap.getAItile(test).fallRiskTile;
		if (room.CameraViewingPoint(room.MiddleOfTile(test)) != room.CameraViewingPoint(room.MiddleOfTile(fallRiskTile)))
		{
			return false;
		}
		if (!ModManager.MMF)
		{
			for (int i = 0; i < room.abstractRoom.exits; i++)
			{
				if (room.aimap.ExitReachableFromTile(fallRiskTile, i, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.StandardGroundCreature)))
				{
					return true;
				}
			}
		}
		return true;
	}

	public static bool ValidCeilingSpotControlled(Room room, IntVector2 test)
	{
		if (room.GetTile(test).Terrain != 0 || !room.GetTile(test + new IntVector2(0, 1)).Solid || !room.GetTile(test + new IntVector2(0, 2)).Solid || room.GetTile(test + new IntVector2(0, -1)).Solid)
		{
			return false;
		}
		if (room.aimap.getAItile(test).narrowSpace)
		{
			return false;
		}
		IntVector2 fallRiskTile = room.aimap.getAItile(test).fallRiskTile;
		return room.CameraViewingPoint(room.MiddleOfTile(test)) == room.CameraViewingPoint(room.MiddleOfTile(fallRiskTile));
	}

	public static float CeilingSpotScore(Room room, IntVector2 test)
	{
		if (!ValidCeilingSpot(room, test))
		{
			return float.MaxValue;
		}
		float num = Mathf.Abs(16f - (float)room.aimap.getAItile(test).smoothedFloorAltitude) + Mathf.Abs(16f - (float)room.aimap.getAItile(test).floorAltitude);
		num += (float)room.aimap.getAItile(test).visibility / 900f;
		IntVector2 fallRiskTile = room.aimap.getAItile(test).fallRiskTile;
		for (int i = -2; i <= 2; i++)
		{
			if (room.GetTile(test + new IntVector2(i, 0)).Solid)
			{
				num += 10f;
			}
			if (room.GetTile(test + new IntVector2(i, 0)).AnyBeam)
			{
				num += 1f;
			}
			if (room.aimap.getAItile(fallRiskTile + new IntVector2(i, 0)).acc != AItile.Accessibility.Floor)
			{
				num += 2f;
			}
		}
		int num2 = int.MinValue;
		int num3 = int.MaxValue;
		for (int j = 0; j < room.abstractRoom.exits; j++)
		{
			if (room.aimap.ExitReachableFromTile(fallRiskTile, j, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.StandardGroundCreature)))
			{
				num -= 3f;
			}
			num2 = Math.Max(num2, room.aimap.ExitDistanceForCreature(fallRiskTile, j, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.StandardGroundCreature)));
			num3 = Math.Min(num3, room.aimap.ExitDistanceForCreature(fallRiskTile, j, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.StandardGroundCreature)));
		}
		if (num3 < 20)
		{
			num += 500f;
		}
		num -= (float)num3 / (float)num2 * 100f;
		for (int k = 0; k < room.shortcuts.Length; k++)
		{
			if (room.shortcuts[k].StartTile.FloatDist(test) < 5f)
			{
				num += ((room.shortcuts[k].shortCutType == ShortcutData.Type.RoomExit) ? 500f : 50f);
			}
		}
		return num - (float)room.aimap.getAItile(fallRiskTile).visibility / 900f;
	}

	bool IUseItemTracker.TrackItem(AbstractPhysicalObject obj)
	{
		if (!bug.safariControlled && creature.world.game.SeededRandom(obj.ID.RandomSeed + 5) > creature.personality.dominance)
		{
			return false;
		}
		if (!(obj.type == AbstractPhysicalObject.AbstractObjectType.Spear) && !(obj.type == AbstractPhysicalObject.AbstractObjectType.DangleFruit) && !(obj.type == AbstractPhysicalObject.AbstractObjectType.ScavengerBomb) && (!(obj.type == AbstractPhysicalObject.AbstractObjectType.DataPearl) || !((obj as DataPearl.AbstractDataPearl).dataPearlType == DataPearl.AbstractDataPearl.DataPearlType.Misc)) && !(obj.type == AbstractPhysicalObject.AbstractObjectType.FlareBomb) && !(obj.type == AbstractPhysicalObject.AbstractObjectType.PuffBall) && !(obj.type == AbstractPhysicalObject.AbstractObjectType.BubbleGrass) && !(obj.type == AbstractPhysicalObject.AbstractObjectType.EggBugEgg) && !(obj.type == AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant) && !(obj.type == AbstractPhysicalObject.AbstractObjectType.FlyLure) && !(obj.type == AbstractPhysicalObject.AbstractObjectType.JellyFish) && !(obj.type == AbstractPhysicalObject.AbstractObjectType.Lantern) && !(obj.type == AbstractPhysicalObject.AbstractObjectType.Mushroom) && !(obj.type == AbstractPhysicalObject.AbstractObjectType.SlimeMold) && !(obj.type == AbstractPhysicalObject.AbstractObjectType.VultureMask))
		{
			return obj.type == AbstractPhysicalObject.AbstractObjectType.WaterNut;
		}
		return true;
	}

	void IUseItemTracker.SeeThrownWeapon(PhysicalObject obj, Creature thrower)
	{
	}
}
