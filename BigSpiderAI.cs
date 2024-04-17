using System;
using System.Collections.Generic;
using Noise;
using RWCustom;
using UnityEngine;

public class BigSpiderAI : ArtificialIntelligence, IUseARelationshipTracker, IAINoiseReaction
{
	public class Behavior : ExtEnum<Behavior>
	{
		public static readonly Behavior Idle = new Behavior("Idle", register: true);

		public static readonly Behavior Flee = new Behavior("Flee", register: true);

		public static readonly Behavior Hunt = new Behavior("Hunt", register: true);

		public static readonly Behavior EscapeRain = new Behavior("EscapeRain", register: true);

		public static readonly Behavior ReturnPrey = new Behavior("ReturnPrey", register: true);

		public static readonly Behavior GetUnstuck = new Behavior("GetUnstuck", register: true);

		public static readonly Behavior ReviveBuddy = new Behavior("ReviveBuddy", register: true);

		public Behavior(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class SpiderTrackState : RelationshipTracker.TrackedCreatureState
	{
		public int accustomed;

		public bool consious;

		public bool close;

		public bool armed;

		public int tagged;

		public float totalMass = 1f;

		public bool ReadyForSpitter
		{
			get
			{
				if (consious)
				{
					return tagged > 0;
				}
				return true;
			}
		}
	}

	public class SpiderSpitModule : AIModule
	{
		public Tracker.CreatureRepresentation spitAtCrit;

		private BigSpiderAI bugAI;

		public bool goToSpitPos;

		public int goToSpitPosAntiFlicker;

		public WorldCoordinate spitPos;

		public Vector2 targetPoint;

		public Vector2 aimDir;

		public int noSitDelay;

		private int targetChunk;

		public List<Vector2> targetTrail;

		public int targetTrailMaxLength = 7;

		public int ammo = 4;

		private float ammoRegen;

		public bool fastAmmoRegen;

		public Tracker.CreatureRepresentation taggedCreature;

		public int randomCritSpitDelay;

		public int sameScreenDelay;

		private BigSpider bug => bugAI.bug;

		private Room room => bugAI.bug.room;

		public Vector2 TargetChunkPos => spitAtCrit.representedCreature.realizedCreature.bodyChunks[targetChunk % spitAtCrit.representedCreature.realizedCreature.bodyChunks.Length].pos;

		public bool CanSpitBetweenScreens
		{
			get
			{
				if (spitAtCrit != null)
				{
					return spitAtCrit.representedCreature.creatureTemplate.type != CreatureTemplate.Type.Slugcat;
				}
				return true;
			}
		}

		public bool SameScreen(Vector2 a, Vector2 b)
		{
			if (sameScreenDelay > 0)
			{
				return true;
			}
			int num = room.CameraViewingPoint(a);
			if (num == -1)
			{
				return true;
			}
			int num2 = room.CameraViewingPoint(b);
			if (num2 == -1)
			{
				return true;
			}
			return num == num2;
		}

		public bool CanSpit()
		{
			if (bug.safariControlled)
			{
				return ammo > 0;
			}
			if (ammo > 0 && spitAtCrit != null && spitAtCrit.VisualContact && targetTrail.Count >= targetTrailMaxLength)
			{
				if ((spitAtCrit.dynamicRelationship.state as SpiderTrackState).tagged >= 1)
				{
					return (spitAtCrit.dynamicRelationship.state as SpiderTrackState).tagged > DartMaggot.UntilSleepDelay + 80;
				}
				return true;
			}
			return false;
		}

		public SpiderSpitModule(BigSpiderAI AI)
			: base(AI)
		{
			bugAI = AI;
			targetChunk = UnityEngine.Random.Range(0, 100);
			targetTrail = new List<Vector2>();
		}

		public void SpiderHasSpit()
		{
			if (!bug.safariControlled)
			{
				ammo--;
			}
			ammoRegen = 0f;
			if (ammo < 1)
			{
				fastAmmoRegen = true;
				bugAI.stayAway = true;
				randomCritSpitDelay = 0;
			}
			else
			{
				fastAmmoRegen = false;
			}
		}

		public override void Update()
		{
			base.Update();
			if (taggedCreature != null && (taggedCreature.dynamicRelationship.state as SpiderTrackState).tagged < 1)
			{
				Custom.Log("untag target");
				taggedCreature = null;
			}
			if (ammo < 4)
			{
				ammoRegen += 1f / (fastAmmoRegen ? 60f : 1200f);
				if (ammoRegen > 1f)
				{
					ammo++;
					ammoRegen -= 1f;
					if (ammo > 3)
					{
						ammoRegen = 0f;
						fastAmmoRegen = false;
						bugAI.stayAway = false;
					}
				}
			}
			if (randomCritSpitDelay < 1 && ammo > 0 && UnityEngine.Random.value < 1f / Mathf.Lerp(100f, 2f, AI.threatTracker.Utility()) && AI.tracker.CreaturesCount > 0)
			{
				Tracker.CreatureRepresentation rep = AI.tracker.GetRep(UnityEngine.Random.Range(0, AI.tracker.CreaturesCount));
				if (rep.VisualContact && rep.representedCreature.realizedCreature.Consious && rep.dynamicRelationship.currentRelationship.type != CreatureTemplate.Relationship.Type.Ignores && UnityEngine.Random.value < rep.dynamicRelationship.currentRelationship.intensity && (rep.dynamicRelationship.state as SpiderTrackState).tagged < 1 && Custom.DistLess(bug.mainBodyChunk.pos, rep.representedCreature.realizedCreature.DangerPos, 400f + 200f * rep.dynamicRelationship.currentRelationship.intensity) && bug.grasps[0] == null && Vector2.Dot((bug.bodyChunks[1].pos - bug.mainBodyChunk.pos).normalized, (bug.mainBodyChunk.pos - rep.representedCreature.realizedCreature.DangerPos).normalized) > -0.3f)
				{
					Custom.Log("spit at random creature");
					spitAtCrit = rep;
					randomCritSpitDelay = 140;
				}
			}
			bool flag = spitAtCrit != null && spitAtCrit.BestGuessForPosition().room == bug.abstractCreature.pos.room && spitAtCrit.TicksSinceSeen < ((spitPos.Tile.FloatDist(bug.abstractCreature.pos.Tile) < 4f) ? 100 : 600) && !(spitAtCrit.dynamicRelationship.state as SpiderTrackState).ReadyForSpitter && room.VisualContact(spitPos.Tile, spitAtCrit.BestGuessForPosition().Tile);
			if (flag != goToSpitPos)
			{
				goToSpitPosAntiFlicker++;
				if (goToSpitPosAntiFlicker > 20)
				{
					goToSpitPos = flag;
				}
			}
			else
			{
				goToSpitPosAntiFlicker = 0;
			}
			if (!CanSpitBetweenScreens)
			{
				if (room.ViewedByAnyCamera(bug.mainBodyChunk.pos, 0f))
				{
					sameScreenDelay = 40;
				}
				else if (sameScreenDelay > 0)
				{
					sameScreenDelay--;
				}
			}
			if (randomCritSpitDelay > 0)
			{
				randomCritSpitDelay--;
			}
			else if (bugAI.preyTracker.MostAttractivePrey != null)
			{
				spitAtCrit = bugAI.preyTracker.MostAttractivePrey;
			}
			if ((bugAI.behavior != Behavior.Hunt && randomCritSpitDelay < 1) || spitAtCrit == null || ammo < 1 || bugAI.bug.room == null)
			{
				spitPos = AI.creature.pos;
				targetTrail.Clear();
				targetPoint = bug.mainBodyChunk.pos;
				return;
			}
			if (UnityEngine.Random.value < 1f / 70f)
			{
				targetChunk = UnityEngine.Random.Range(0, 100);
			}
			float num = SpitPosScore(spitPos);
			for (int i = 0; i < (goToSpitPos ? 20 : 5); i++)
			{
				WorldCoordinate worldCoordinate = room.GetWorldCoordinate((UnityEngine.Random.value < 0.5f) ? (bug.mainBodyChunk.pos + Custom.RNV() * UnityEngine.Random.value * UnityEngine.Random.value * 1700f) : (room.MiddleOfTile(spitPos) + Custom.RNV() * UnityEngine.Random.value * 200f));
				float num2 = SpitPosScore(worldCoordinate);
				if (num2 < num)
				{
					spitPos = worldCoordinate;
					num = num2;
				}
			}
			if (noSitDelay > 0)
			{
				noSitDelay--;
			}
			if (spitAtCrit.VisualContact)
			{
				targetPoint = TargetChunkPos;
			}
			else
			{
				targetPoint = room.MiddleOfTile(spitAtCrit.BestGuessForPosition());
			}
			if (spitAtCrit.TicksSinceSeen < 40)
			{
				targetTrail.Insert(0, targetPoint);
				if (targetTrail.Count > targetTrailMaxLength)
				{
					targetTrail.RemoveAt(targetTrail.Count - 1);
				}
			}
			else if (targetTrail.Count > 0)
			{
				targetTrail.Clear();
			}
			Vector2 vector = targetPoint;
			if (targetTrail.Count > 0 && randomCritSpitDelay < 1)
			{
				vector = targetTrail[targetTrail.Count - 1];
			}
			vector.y += Custom.LerpMap(Vector2.Distance(bug.mainBodyChunk.pos, vector), 70f, 900f, 0f, 80f, 2f);
			aimDir = Custom.DirVec(bug.mainBodyChunk.pos, vector);
			if (!bug.safariControlled && bug.CanSpit(initiate: true) && spitAtCrit.VisualContact && (CanSpitBetweenScreens || SameScreen(bug.firstChunk.pos, targetPoint)) && bug.charging == 0f && (randomCritSpitDelay > 0 || (Custom.DistLess(bug.mainBodyChunk.pos, targetPoint, bug.spitPos.HasValue ? 1200f : (100f + (float)ammo * 100f)) && !Custom.DistLess(bug.mainBodyChunk.pos, targetPoint, bug.spitPos.HasValue ? 0f : 150f) && targetTrail.Count >= targetTrailMaxLength && room.VisualContact(bug.mainBodyChunk.pos, targetTrail[targetTrail.Count - 1]))) && room.VisualContact(bug.mainBodyChunk.pos, targetPoint))
			{
				bug.TryInitiateSpit();
			}
		}

		public float SpitPosScore(WorldCoordinate test)
		{
			if (test.room != room.abstractRoom.index || Custom.DistLess(room.MiddleOfTile(test), targetPoint, 220f) || !bugAI.pathFinder.CoordinateReachableAndGetbackable(test) || (!CanSpitBetweenScreens && !SameScreen(room.MiddleOfTile(spitAtCrit.BestGuessForPosition()), room.MiddleOfTile(test))) || !room.VisualContact(test.Tile, spitAtCrit.BestGuessForPosition().Tile))
			{
				return float.MaxValue;
			}
			if (randomCritSpitDelay > 0)
			{
				return bugAI.creature.pos.Tile.FloatDist(test.Tile);
			}
			float num = Mathf.Abs(20f - spitAtCrit.BestGuessForPosition().Tile.FloatDist(test.Tile)) * 2f;
			num += Mathf.Clamp(spitPos.Tile.FloatDist(test.Tile), 10f, 40f);
			num += Mathf.Min(bugAI.creature.pos.Tile.FloatDist(test.Tile), AI.pathFinder.GetDestination.Tile.FloatDist(test.Tile)) * Custom.LerpMap(spitAtCrit.TicksSinceSeen, 0f, 100f, 2f, 0.5f);
			num -= (float)room.aimap.getAItile(test).visibility / 400f;
			num += (float)room.aimap.getTerrainProximity(test) * 10f;
			num += (float)Custom.IntClamp(spitAtCrit.BestGuessForPosition().Tile.y + 5 - test.Tile.y, -10, 10) * 2f;
			if (!Custom.DistLess(bug.mainBodyChunk.pos, room.MiddleOfTile(test), 900f))
			{
				num += 800f;
			}
			if (room.aimap.getAItile(test).narrowSpace)
			{
				num += 1000f;
			}
			Vector2 vector = targetPoint + Custom.DirVec(bug.mainBodyChunk.pos, targetPoint) * (Vector2.Distance(bug.mainBodyChunk.pos, targetPoint) + 200f);
			if (Custom.DistLess(room.MiddleOfTile(test), vector, Vector2.Distance(vector, targetPoint) + 100f))
			{
				num += 900f;
			}
			for (int i = 0; i < AI.tracker.CreaturesCount; i++)
			{
				num -= Mathf.Min(20f, test.Tile.FloatDist(AI.tracker.GetRep(i).BestGuessForPosition().Tile) * AI.DynamicRelationship(AI.tracker.GetRep(i).representedCreature).intensity);
				if (AI.tracker.GetRep(i).representedCreature.creatureTemplate.type == CreatureTemplate.Type.SpitterSpider && AI.tracker.GetRep(i).representedCreature.personality.dominance > AI.creature.personality.dominance && AI.tracker.GetRep(i).representedCreature.abstractAI.RealAI != null)
				{
					num -= Mathf.Min(20f, test.Tile.FloatDist((AI.tracker.GetRep(i).representedCreature.abstractAI.RealAI as BigSpiderAI).spitModule.spitPos.Tile)) * 5f;
				}
			}
			return num;
		}

		public bool SitAndSpit()
		{
			if (goToSpitPos && noSitDelay < 1 && bugAI.behavior == Behavior.Hunt && Custom.DistLess(bug.bodyChunks[0].pos, room.MiddleOfTile(spitPos), 80f) && !Custom.DistLess(room.MiddleOfTile(spitPos), targetPoint, 300f) && spitAtCrit != null && spitAtCrit.VisualContact && (room.aimap.TileAccessibleToCreature(bug.bodyChunks[0].pos, bug.Template) || room.aimap.TileAccessibleToCreature(bug.bodyChunks[1].pos, bug.Template)))
			{
				return room.VisualContact(bug.bodyChunks[0].pos, spitAtCrit.representedCreature.realizedCreature.DangerPos);
			}
			return false;
		}

		public bool AbandonSitAndSpit()
		{
			if (goToSpitPos && Custom.DistLess(bug.mainBodyChunk.pos, room.MiddleOfTile(spitPos), 120f) && spitAtCrit != null && spitAtCrit.TicksSinceSeen <= 20 && !Custom.DistLess(room.MiddleOfTile(spitPos), targetPoint, 220f))
			{
				return bugAI.behavior != Behavior.Hunt;
			}
			return true;
		}

		public void CreatureHitByDart(AbstractCreature crit)
		{
			if (AI.tracker.RepresentationForCreature(crit, addIfMissing: false) != null)
			{
				if (AI.StaticRelationship(crit).type == CreatureTemplate.Relationship.Type.Eats && (taggedCreature == null || AI.StaticRelationship(taggedCreature.representedCreature).intensity < AI.StaticRelationship(crit).intensity))
				{
					taggedCreature = AI.tracker.RepresentationForCreature(crit, addIfMissing: false);
				}
				(AI.tracker.RepresentationForCreature(crit, addIfMissing: false).dynamicRelationship.state as SpiderTrackState).tagged = DartMaggot.UntilSleepDelay + 200;
				if (taggedCreature != null)
				{
					bugAI.stayAway = false;
				}
				if (UnityEngine.Random.value < 0.5f)
				{
					randomCritSpitDelay = 0;
				}
			}
		}
	}

	public class LightThreat
	{
		public LightSource light;

		public BigSpiderAI AI;

		public ThreatTracker.ThreatPoint threatPoint;

		public bool slatedForDeletion;

		public LightThreat(BigSpiderAI AI, LightSource light)
		{
			this.AI = AI;
			this.light = light;
			threatPoint = AI.threatTracker.AddThreatPoint(null, light.room.GetWorldCoordinate(light.Pos), 0f);
		}

		public void Update()
		{
			if (!LightThreatening(light, AI.creature.Room.realizedRoom))
			{
				Destroy();
				return;
			}
			threatPoint.pos = light.room.GetWorldCoordinate(light.Pos);
			threatPoint.severity = ThreatSeverityOfLight(light, AI.creature.Room.realizedRoom) * AI.ShyFromLight;
		}

		public void Destroy()
		{
			slatedForDeletion = true;
		}

		private static float ThreatSeverityOfLight(LightSource light, Room room)
		{
			return Custom.LerpMap(light.Rad, 20f, 400f, 0.1f, 0.1f + Mathf.Pow(light.Alpha, 0.5f) * LerpedDarkness(room, light.Pos));
		}

		public static bool LightThreatening(LightSource light, Room room)
		{
			if (!light.slatedForDeletetion && light.room != null && room != null && light.room.abstractRoom.index == room.abstractRoom.index)
			{
				return ThreatSeverityOfLight(light, room) > 0f;
			}
			return false;
		}

		public static float LerpedDarkness(Room room, Vector2 pos)
		{
			return Mathf.Pow(Mathf.InverseLerp(0.37f, 0.75f, room.Darkness(pos)), 0.5f);
		}
	}

	public BigSpider bug;

	private DebugDestinationVisualizer debugDestinationVisualizer;

	public float currentUtility;

	public float fear;

	public int noiseRectionDelay;

	private bool arenaMode;

	public Behavior behavior;

	public WorldCoordinate? idleTowardsPosition;

	public List<WorldCoordinate> previdlePositions;

	private int idlePosCounter;

	public WorldCoordinate tempIdlePos;

	public WorldCoordinate idlePos;

	public List<LightThreat> lightThreats;

	public bool stayAway;

	public float shyLightCycle;

	public SpiderSpitModule spitModule;

	public List<BigSpiderAI> otherSpiders = new List<BigSpiderAI>();

	public Tracker.CreatureRepresentation reviveBuddy;

	public float ShyFromLight
	{
		get
		{
			if (bug.spitter || bug.mother)
			{
				if (base.preyTracker.MostAttractivePrey != null && (base.preyTracker.MostAttractivePrey.dynamicRelationship.state as SpiderTrackState).ReadyForSpitter)
				{
					return 0f;
				}
				return LightThreat.LerpedDarkness(creature.Room.realizedRoom, bug.mainBodyChunk.pos) * (stayAway ? 1f : Mathf.Clamp01(Mathf.Sin(shyLightCycle * (float)Math.PI)));
			}
			return LightThreat.LerpedDarkness(creature.Room.realizedRoom, bug.mainBodyChunk.pos) * (stayAway ? 1f : Custom.PushFromHalf(0.5f + 0.5f * Mathf.Sin(shyLightCycle * (float)Math.PI), 3f));
		}
	}

	public BigSpiderAI(AbstractCreature creature, World world)
		: base(creature, world)
	{
		arenaMode = world.game.IsArenaSession;
		bug = creature.realizedCreature as BigSpider;
		bug.AI = this;
		AddModule(new StandardPather(this, world, creature));
		base.pathFinder.stepsPerFrame = 15;
		base.pathFinder.accessibilityStepsPerFrame = 15;
		AddModule(new Tracker(this, 10, 10, 1500, 0.5f, 5, 5, 10));
		AddModule(new ThreatTracker(this, 3));
		AddModule(new PreyTracker(this, 5, 2f, 10f, 70f, 0.5f));
		AddModule(new RainTracker(this));
		AddModule(new DenFinder(this, creature));
		AddModule(new StuckTracker(this, trackPastPositions: true, trackNotFollowingCurrentGoal: false));
		AddModule(new NoiseTracker(this, base.tracker));
		AddModule(new UtilityComparer(this));
		AddModule(new RelationshipTracker(this, base.tracker));
		if (bug.spitter)
		{
			spitModule = new SpiderSpitModule(this);
			AddModule(spitModule);
		}
		FloatTweener.FloatTween smoother = new FloatTweener.FloatTweenUpAndDown(new FloatTweener.FloatTweenBasic(FloatTweener.TweenType.Lerp, 0.5f), new FloatTweener.FloatTweenBasic(FloatTweener.TweenType.Tick, 0.005f));
		base.utilityComparer.AddComparedModule(base.threatTracker, smoother, 1f, 1.1f);
		smoother = new FloatTweener.FloatTweenUpAndDown(new FloatTweener.FloatTweenBasic(FloatTweener.TweenType.Lerp, 0.2f), new FloatTweener.FloatTweenBasic(FloatTweener.TweenType.Tick, 0.01f));
		base.utilityComparer.AddComparedModule(base.preyTracker, smoother, 0.65f, 1.1f);
		base.utilityComparer.AddComparedModule(base.rainTracker, null, 1f, 1.1f);
		base.utilityComparer.AddComparedModule(base.stuckTracker, null, 0.4f, 1.1f);
		lightThreats = new List<LightThreat>();
		behavior = Behavior.Idle;
		previdlePositions = new List<WorldCoordinate>();
	}

	public override bool TrackerToDiscardDeadCreature(AbstractCreature crit)
	{
		if (crit.creatureTemplate.type == CreatureTemplate.Type.SpitterSpider || crit.creatureTemplate.type == CreatureTemplate.Type.BigSpider)
		{
			return false;
		}
		return base.TrackerToDiscardDeadCreature(crit);
	}

	private void TryAddReviveBuddy(Tracker.CreatureRepresentation candidate)
	{
		if (reviveBuddy != null || (candidate.EstimatedChanceOfFinding < 0.05f && !bug.safariControlled))
		{
			return;
		}
		for (int i = 0; i < otherSpiders.Count; i++)
		{
			if (otherSpiders[i].reviveBuddy == candidate)
			{
				return;
			}
		}
		if (candidate.representedCreature.realizedCreature == null || !(candidate.representedCreature.realizedCreature is BigSpider) || !(candidate.representedCreature.realizedCreature as BigSpider).CanIBeRevived)
		{
			return;
		}
		if (candidate.BestGuessForPosition().room == creature.pos.room)
		{
			for (int j = 0; j < 9; j++)
			{
				if (base.pathFinder.CoordinateReachableAndGetbackable(candidate.BestGuessForPosition() + Custom.eightDirectionsAndZero[j]))
				{
					reviveBuddy = candidate;
					break;
				}
			}
		}
		else
		{
			reviveBuddy = candidate;
		}
	}

	public override void Update()
	{
		if (behavior == Behavior.Flee && !RainWorldGame.RequestHeavyAi(bug))
		{
			return;
		}
		base.Update();
		if (bug.room == null)
		{
			return;
		}
		if (ModManager.MSC && bug.LickedByPlayer != null)
		{
			base.tracker.SeeCreature(bug.LickedByPlayer.abstractCreature);
			stayAway = false;
		}
		shyLightCycle += 0.0025f;
		if (base.tracker.CreaturesCount > 0)
		{
			Tracker.CreatureRepresentation rep = base.tracker.GetRep(UnityEngine.Random.Range(0, base.tracker.CreaturesCount));
			if ((rep.representedCreature.creatureTemplate.type == CreatureTemplate.Type.BigSpider || rep.representedCreature.creatureTemplate.type == CreatureTemplate.Type.SpitterSpider) && rep.representedCreature.realizedCreature != null && !otherSpiders.Contains((rep.representedCreature.realizedCreature as BigSpider).AI))
			{
				if (rep.representedCreature.state.alive)
				{
					if (rep.VisualContact)
					{
						otherSpiders.Add((rep.representedCreature.realizedCreature as BigSpider).AI);
						if (rep.representedCreature.personality.dominance > creature.personality.dominance)
						{
							shyLightCycle = (rep.representedCreature.realizedCreature as BigSpider).AI.shyLightCycle;
						}
					}
				}
				else if (!bug.spitter)
				{
					TryAddReviveBuddy(rep);
				}
			}
		}
		for (int num = otherSpiders.Count - 1; num >= 0; num--)
		{
			if (otherSpiders[num].bug.dead || otherSpiders[num].creature.realizedCreature == null || otherSpiders[num].creature.pos.room != creature.pos.room)
			{
				otherSpiders.RemoveAt(num);
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
		if (bug.spitter)
		{
			base.utilityComparer.GetUtilityTracker(base.threatTracker).weight = Mathf.InverseLerp(40f, 10f, spitModule.randomCritSpitDelay);
		}
		else if (base.preyTracker.MostAttractivePrey != null)
		{
			base.utilityComparer.GetUtilityTracker(base.preyTracker).weight = Custom.LerpMap(creature.pos.Tile.FloatDist(base.preyTracker.MostAttractivePrey.BestGuessForPosition().Tile), 26f, 36f, 1f, 0.1f);
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
		}
		if (currentUtility < 0.05f)
		{
			behavior = Behavior.Idle;
		}
		if (reviveBuddy != null)
		{
			if ((reviveBuddy.EstimatedChanceOfFinding < 0.05f && !bug.safariControlled) || reviveBuddy.representedCreature.realizedCreature == null || !(reviveBuddy.representedCreature.realizedCreature as BigSpider).CanIBeRevived || !reviveBuddy.representedCreature.realizedCreature.dead)
			{
				reviveBuddy = null;
			}
			else if (currentUtility < 0.3f + ShyFromLight * 0.7f)
			{
				behavior = Behavior.ReviveBuddy;
				currentUtility = 0.3f + ShyFromLight * 0.7f;
			}
		}
		if (behavior != Behavior.Flee && bug.grasps[0] != null && bug.grasps[0].grabbed is Creature && DynamicRelationship((bug.grasps[0].grabbed as Creature).abstractCreature).type == CreatureTemplate.Relationship.Type.Eats)
		{
			behavior = Behavior.ReturnPrey;
			currentUtility = 1f;
		}
		if (behavior != Behavior.Idle && behavior != Behavior.GetUnstuck)
		{
			idlePos = creature.pos;
		}
		if (bug.sitting)
		{
			bug.runSpeed = Custom.LerpAndTick(bug.runSpeed, 0f, 0.01f, 1f / 60f);
		}
		idlePosCounter--;
		if (behavior == Behavior.GetUnstuck)
		{
			bug.runSpeed = 1f;
			if (UnityEngine.Random.value < 1f / 150f)
			{
				creature.abstractAI.SetDestination(bug.room.GetWorldCoordinate(bug.mainBodyChunk.pos + Custom.RNV() * 100f));
			}
		}
		else if (behavior == Behavior.Idle)
		{
			if (!bug.sitting)
			{
				if (bug.mother)
				{
					bug.runSpeed = Custom.LerpAndTick(bug.runSpeed, 0.15f, 0.01f, 1f / 60f);
				}
				else
				{
					bug.runSpeed = Custom.LerpAndTick(bug.runSpeed, 0.5f, 0.01f, 1f / 60f);
				}
			}
			if (bug.borrowedTime > 0 && base.denFinder.GetDenPosition().HasValue)
			{
				creature.abstractAI.SetDestination(base.denFinder.GetDenPosition().Value);
			}
			else if (idleTowardsPosition.HasValue)
			{
				creature.abstractAI.SetDestination(idleTowardsPosition.Value);
				if (UnityEngine.Random.value < 0.002f || Custom.ManhattanDistance(creature.pos, idleTowardsPosition.Value) < 4)
				{
					idleTowardsPosition = null;
				}
			}
			else if (!creature.abstractAI.WantToMigrate)
			{
				WorldCoordinate coord = new WorldCoordinate(bug.room.abstractRoom.index, UnityEngine.Random.Range(0, bug.room.TileWidth), UnityEngine.Random.Range(0, bug.room.TileHeight), -1);
				if (base.pathFinder.CoordinateReachableAndGetbackable(coord) && IdleScore(coord) < IdleScore(tempIdlePos))
				{
					tempIdlePos = coord;
				}
				creature.abstractAI.SetDestination(idlePos);
				if (Custom.ManhattanDistance(creature.pos, idlePos) < 3 && (bug.room.aimap.getAItile(creature.pos).narrowSpace || TileInEnclosedArea(creature.pos.Tile)))
				{
					idlePosCounter -= 4;
				}
				if (idlePosCounter < 1)
				{
					idlePosCounter = UnityEngine.Random.Range(200, 800);
					previdlePositions.Add(idlePos);
					if (previdlePositions.Count > 9)
					{
						previdlePositions.RemoveAt(0);
					}
					idlePos = tempIdlePos;
					tempIdlePos = new WorldCoordinate(bug.room.abstractRoom.index, UnityEngine.Random.Range(0, bug.room.TileWidth), UnityEngine.Random.Range(0, bug.room.TileHeight), -1);
				}
			}
		}
		else if (behavior == Behavior.Hunt)
		{
			if (!bug.sitting)
			{
				if (bug.mother)
				{
					bug.runSpeed = Custom.LerpAndTick(bug.runSpeed, 0.15f, 0.01f, 0.1f);
				}
				else
				{
					bug.runSpeed = Custom.LerpAndTick(bug.runSpeed, 1f, 0.01f, 0.1f);
				}
			}
			if (base.preyTracker.MostAttractivePrey != null && !bug.safariControlled)
			{
				if (bug.spitter)
				{
					if (spitModule.goToSpitPos)
					{
						creature.abstractAI.SetDestination(spitModule.spitPos);
						idleTowardsPosition = spitModule.spitPos;
					}
					else
					{
						creature.abstractAI.SetDestination(base.preyTracker.MostAttractivePrey.BestGuessForPosition());
						if (base.preyTracker.MostAttractivePrey.VisualContact && !base.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.Consious && Custom.DistLess(bug.mainBodyChunk.pos, base.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.mainBodyChunk.pos, 30f))
						{
							bug.mainBodyChunk.vel += Custom.DirVec(bug.mainBodyChunk.pos, base.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.mainBodyChunk.pos) * 2f;
						}
					}
				}
				else
				{
					creature.abstractAI.SetDestination(base.preyTracker.MostAttractivePrey.BestGuessForPosition());
					if (base.preyTracker.MostAttractivePrey.representedCreature.realizedCreature != null && bug.CanJump && !bug.jumping && bug.charging == 0f && bug.Footing && base.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.room == bug.room)
					{
						Vector2 pos = base.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.mainBodyChunk.pos;
						if (Custom.DistLess(bug.mainBodyChunk.pos, pos, 120f) && (bug.room.aimap.TileAccessibleToCreature(bug.room.GetTilePosition(bug.bodyChunks[1].pos - Custom.DirVec(bug.bodyChunks[1].pos, pos) * 30f), bug.Template) || bug.room.GetTile(bug.bodyChunks[1].pos - Custom.DirVec(bug.bodyChunks[1].pos, pos) * 30f).Solid) && bug.room.VisualContact(bug.mainBodyChunk.pos, pos))
						{
							if (Vector2.Dot((bug.mainBodyChunk.pos - pos).normalized, (bug.bodyChunks[1].pos - bug.mainBodyChunk.pos).normalized) > 0.2f)
							{
								bug.InitiateJump(base.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.mainBodyChunk.pos);
							}
							else
							{
								bug.mainBodyChunk.vel += Custom.DirVec(bug.mainBodyChunk.pos, pos);
								bug.bodyChunks[1].vel -= Custom.DirVec(bug.mainBodyChunk.pos, pos);
							}
						}
					}
				}
			}
		}
		else if (behavior == Behavior.Flee)
		{
			if (!bug.sitting)
			{
				if (bug.mother)
				{
					bug.runSpeed = Custom.LerpAndTick(bug.runSpeed, 0.2f, 0.01f, 0.1f);
				}
				else
				{
					bug.runSpeed = Custom.LerpAndTick(bug.runSpeed, 1f, 0.01f, 0.1f);
				}
			}
			creature.abstractAI.SetDestination(base.threatTracker.FleeTo(creature.pos, 6, 30, considerLeavingRoom: true));
		}
		else if (behavior == Behavior.EscapeRain || behavior == Behavior.ReturnPrey)
		{
			if (!bug.sitting)
			{
				bug.runSpeed = Custom.LerpAndTick(bug.runSpeed, 1f, 0.01f, 0.1f);
			}
			if (base.denFinder.GetDenPosition().HasValue)
			{
				creature.abstractAI.SetDestination(base.denFinder.GetDenPosition().Value);
			}
		}
		else if (behavior == Behavior.ReviveBuddy && reviveBuddy != null)
		{
			creature.abstractAI.SetDestination(reviveBuddy.BestGuessForPosition());
			if (!bug.safariControlled && reviveBuddy.representedCreature.realizedCreature != null && reviveBuddy.representedCreature.realizedCreature.room == bug.room && bug.revivingBuddy == null && Custom.DistLess(bug.mainBodyChunk.pos, reviveBuddy.representedCreature.realizedCreature.mainBodyChunk.pos, 80f) && bug.room.VisualContact(bug.mainBodyChunk.pos, reviveBuddy.representedCreature.realizedCreature.mainBodyChunk.pos))
			{
				bug.mainBodyChunk.vel += Custom.DirVec(bug.mainBodyChunk.pos, reviveBuddy.representedCreature.realizedCreature.mainBodyChunk.pos) * 3f;
			}
		}
		if (noiseRectionDelay > 0)
		{
			noiseRectionDelay--;
		}
		for (int num2 = lightThreats.Count - 1; num2 >= 0; num2--)
		{
			if (lightThreats[num2].slatedForDeletion)
			{
				lightThreats.RemoveAt(num2);
			}
			else
			{
				lightThreats[num2].Update();
			}
		}
		if (ShyFromLight > 0f && creature.Room.realizedRoom != null && creature.Room.realizedRoom.lightSources.Count > 0)
		{
			TryAddLightThreat(creature.Room.realizedRoom.lightSources[UnityEngine.Random.Range(0, creature.Room.realizedRoom.lightSources.Count)]);
		}
	}

	private void TryAddLightThreat(LightSource light)
	{
		if (light.environmentalLight || !LightThreat.LightThreatening(light, creature.Room.realizedRoom) || (light.tiedToObject != null && (light.tiedToObject is Lizard || light.tiedToObject is GreenSparks.GreenSpark || light.tiedToObject is CosmeticInsect || light.tiedToObject is LightFixture)))
		{
			return;
		}
		for (int i = 0; i < lightThreats.Count; i++)
		{
			if (lightThreats[i].light == light)
			{
				return;
			}
		}
		lightThreats.Add(new LightThreat(this, light));
	}

	private float IdleScore(WorldCoordinate coord)
	{
		if (ModManager.MMF)
		{
			if (coord.room != creature.pos.room || !base.pathFinder.CoordinateReachableAndGetbackable(coord))
			{
				return 50f;
			}
			if (!bug.room.aimap.WorldCoordinateAccessibleToCreature(coord, creature.creatureTemplate) || !base.pathFinder.CoordinateReachableAndGetbackable(coord))
			{
				return float.MaxValue;
			}
		}
		else if (coord.room != creature.pos.room || !base.pathFinder.CoordinateReachableAndGetbackable(coord))
		{
			return 100000f;
		}
		float num = 1f;
		if (bug.room.aimap.getAItile(coord).narrowSpace)
		{
			num += 600f;
		}
		if (TileInEnclosedArea(coord.Tile))
		{
			num += 400f;
		}
		if (bug.room.aimap.getTerrainProximity(coord) > 1)
		{
			num += 200f;
		}
		num += base.threatTracker.ThreatOfTile(coord, accountThreatCreatureAccessibility: false) * 500f;
		for (int i = 0; i < previdlePositions.Count; i++)
		{
			num += Mathf.Pow(Mathf.InverseLerp(80f, 5f, previdlePositions[i].Tile.FloatDist(coord.Tile)), 2f) * Custom.LerpMap(i, 0f, 8f, 70f, 15f);
		}
		num += Mathf.Max(0f, creature.pos.Tile.FloatDist(coord.Tile) - 40f) / 20f;
		num += Mathf.Clamp(Mathf.Abs(800f - (float)bug.room.aimap.getAItile(coord).visibility), 300f, 1000f) / 30f;
		return num - Mathf.Max(bug.room.aimap.getAItile(coord).smoothedFloorAltitude, 6f) * 2f;
	}

	public bool TileInEnclosedArea(IntVector2 testTile)
	{
		int num = 0;
		for (int i = 0; i < 4; i++)
		{
			if (bug.room.GetTile(testTile + Custom.fourDirections[i] * 2).Solid)
			{
				num++;
				if (num > 1)
				{
					return true;
				}
			}
		}
		return false;
	}

	public override bool WantToStayInDenUntilEndOfCycle()
	{
		return base.rainTracker.Utility() > 0.01f;
	}

	public void CollideWithKin(BigSpider otherBug)
	{
		if (Custom.ManhattanDistance(creature.pos, otherBug.AI.pathFinder.GetDestination) >= 4 && ((otherBug.abstractCreature.personality.dominance > bug.abstractCreature.personality.dominance && !otherBug.sitting) || bug.sitting || otherBug.AI.pathFinder.GetDestination.room != otherBug.room.abstractRoom.index))
		{
			idleTowardsPosition = otherBug.AI.pathFinder.GetDestination;
		}
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
		return new SpiderTrackState();
	}

	CreatureTemplate.Relationship IUseARelationshipTracker.UpdateDynamicRelationship(RelationshipTracker.DynamicRelationship dRelation)
	{
		if (dRelation.trackerRep.VisualContact)
		{
			dRelation.state.alive = dRelation.trackerRep.representedCreature.state.alive;
			if (dRelation.trackerRep.representedCreature.realizedCreature != null)
			{
				(dRelation.state as SpiderTrackState).consious = dRelation.trackerRep.representedCreature.realizedCreature.Consious;
				if (bug.spitter && (dRelation.trackerRep.representedCreature.creatureTemplate.type == CreatureTemplate.Type.Slugcat || dRelation.trackerRep.representedCreature.creatureTemplate.type == CreatureTemplate.Type.Scavenger))
				{
					(dRelation.state as SpiderTrackState).armed = false;
					for (int i = 0; i < dRelation.trackerRep.representedCreature.realizedCreature.grasps.Length; i++)
					{
						if (dRelation.trackerRep.representedCreature.realizedCreature.grasps[i] != null && dRelation.trackerRep.representedCreature.realizedCreature.grasps[i].grabbed is Spear)
						{
							(dRelation.state as SpiderTrackState).armed = true;
							break;
						}
					}
				}
				if ((dRelation.state as SpiderTrackState).totalMass == 1f)
				{
					if (dRelation.trackerRep.representedCreature.realizedCreature.Template.type == CreatureTemplate.Type.Slugcat)
					{
						(dRelation.state as SpiderTrackState).totalMass = 0.84f;
					}
					else
					{
						(dRelation.state as SpiderTrackState).totalMass = dRelation.trackerRep.representedCreature.realizedCreature.TotalMass;
					}
				}
			}
		}
		CreatureTemplate.Relationship result = StaticRelationship(dRelation.trackerRep.representedCreature);
		if (dRelation.trackerRep.representedCreature.creatureTemplate.type == CreatureTemplate.Type.Centipede && (dRelation.state as SpiderTrackState).consious && (dRelation.state as SpiderTrackState).totalMass > bug.TotalMass * 2f)
		{
			result = new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, Mathf.InverseLerp(bug.TotalMass, bug.TotalMass * 7f, (dRelation.state as SpiderTrackState).totalMass));
		}
		if (bug.spitter)
		{
			if ((dRelation.state as SpiderTrackState).tagged > 0)
			{
				(dRelation.state as SpiderTrackState).tagged--;
			}
			if (result.type == CreatureTemplate.Relationship.Type.Eats && (dRelation.trackerRep.representedCreature.creatureTemplate.CreatureRelationship(bug.Template).type == CreatureTemplate.Relationship.Type.Eats || (dRelation.state as SpiderTrackState).armed))
			{
				(dRelation.state as SpiderTrackState).close = creature.pos.Tile.FloatDist(dRelation.trackerRep.BestGuessForPosition().Tile) < ((dRelation.state as SpiderTrackState).close ? 10f : 6f);
				if ((dRelation.state as SpiderTrackState).consious)
				{
					if ((dRelation.state as SpiderTrackState).close)
					{
						result = new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.6f);
					}
					else if ((dRelation.state as SpiderTrackState).tagged > 200 && (dRelation.state as SpiderTrackState).tagged < DartMaggot.UntilSleepDelay + 160)
					{
						result = new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f);
					}
				}
			}
			if (spitModule.randomCritSpitDelay > 0 && dRelation.trackerRep == spitModule.spitAtCrit)
			{
				result.type = CreatureTemplate.Relationship.Type.Attacks;
			}
		}
		if (result.type == CreatureTemplate.Relationship.Type.Afraid)
		{
			if (!dRelation.state.alive)
			{
				result.intensity = 0f;
			}
		}
		else if (result.type == CreatureTemplate.Relationship.Type.Eats || result.type == CreatureTemplate.Relationship.Type.Attacks)
		{
			if (bug.spitter)
			{
				if (spitModule.taggedCreature != null)
				{
					if (dRelation.trackerRep == spitModule.taggedCreature)
					{
						result.intensity = Mathf.Lerp(result.intensity, 1f, 0.5f);
					}
					else
					{
						result.intensity *= 0.2f;
					}
				}
				return result;
			}
			if (dRelation.trackerRep.VisualContact)
			{
				(dRelation.state as SpiderTrackState).accustomed += 2;
			}
			else if (dRelation.trackerRep.TicksSinceSeen < 300)
			{
				(dRelation.state as SpiderTrackState).accustomed++;
			}
			else
			{
				(dRelation.state as SpiderTrackState).accustomed--;
			}
			(dRelation.state as SpiderTrackState).accustomed = Custom.IntClamp((dRelation.state as SpiderTrackState).accustomed, 0, 1800);
			float num;
			if (stayAway)
			{
				num = 0f;
			}
			else
			{
				num = Mathf.InverseLerp(200f, Custom.LerpMap((dRelation.state as SpiderTrackState).totalMass, 0.1f, 4f, 200f, 1800f, 0.29f), (dRelation.state as SpiderTrackState).accustomed * ((!arenaMode) ? 1 : 5) * Math.Max(1, otherSpiders.Count) * ((bug.borrowedTime <= 0) ? 1 : 5));
				if (!(dRelation.state as SpiderTrackState).consious)
				{
					num = 1f;
				}
				if (dRelation.trackerRep.representedCreature.realizedCreature != null && ShyFromLight > 0f)
				{
					bool flag = false;
					if (dRelation.trackerRep.representedCreature.realizedCreature is Player && (dRelation.trackerRep.representedCreature.realizedCreature as Player).glowing)
					{
						flag = true;
					}
					if (!flag && dRelation.trackerRep.representedCreature.realizedCreature is LanternMouse && (float)(dRelation.trackerRep.representedCreature.realizedCreature as LanternMouse).State.battery > 0.4f)
					{
						flag = true;
					}
					if (!flag && dRelation.trackerRep.representedCreature.realizedCreature.grasps != null)
					{
						for (int j = 0; j < dRelation.trackerRep.representedCreature.realizedCreature.grasps.Length; j++)
						{
							if (dRelation.trackerRep.representedCreature.realizedCreature.grasps[j] != null && dRelation.trackerRep.representedCreature.realizedCreature.grasps[j].grabbed is Lantern)
							{
								flag = true;
								break;
							}
						}
					}
					if (flag)
					{
						num *= 1f - ShyFromLight;
					}
				}
			}
			if (num < 0.5f)
			{
				result = new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, Mathf.InverseLerp(0.5f, 0f, num) * Mathf.Pow(Mathf.InverseLerp(0.2f, 4f, dRelation.trackerRep.representedCreature.creatureTemplate.bodySize), 0.5f));
			}
			else
			{
				result.intensity *= Mathf.InverseLerp(0.5f, 0.75f, num);
			}
		}
		return result;
	}

	public override PathCost TravelPreference(MovementConnection coord, PathCost cost)
	{
		float num = Mathf.Max(0f, base.threatTracker.ThreatOfTile(coord.destinationCoord, accountThreatCreatureAccessibility: true) - base.threatTracker.ThreatOfTile(creature.pos, accountThreatCreatureAccessibility: true)) * 40f;
		if (!bug.spitter)
		{
			num += Custom.LerpMap(bug.room.aimap.getAItile(coord.DestTile).smoothedFloorAltitude, 1f, 7f, 60f, 0f);
		}
		return new PathCost(cost.resistance + num, cost.legality);
	}

	public void ReactToNoise(NoiseTracker.TheorizedSource source, InGameNoise noise)
	{
		if (!bug.sitting || noiseRectionDelay > 0)
		{
			return;
		}
		bug.bodyChunks[1].pos += Custom.RNV() * 4f;
		if (bug.graphicsModule != null)
		{
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					(bug.graphicsModule as BigSpiderGraphics).legs[i, j].pos = Vector2.Lerp((bug.graphicsModule as BigSpiderGraphics).legs[i, j].pos, bug.mainBodyChunk.pos, UnityEngine.Random.value);
				}
			}
		}
		noiseRectionDelay = UnityEngine.Random.Range(0, bug.spitter ? 100 : 10);
	}
}
