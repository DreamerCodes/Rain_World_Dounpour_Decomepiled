using MoreSlugcats;
using Noise;
using RWCustom;
using UnityEngine;

public class EggBugAI : ArtificialIntelligence, IUseARelationshipTracker, IAINoiseReaction
{
	public class Behavior : ExtEnum<Behavior>
	{
		public static readonly Behavior Idle = new Behavior("Idle", register: true);

		public static readonly Behavior Flee = new Behavior("Flee", register: true);

		public static readonly Behavior Hunt = new Behavior("Hunt", register: true);

		public static readonly Behavior EscapeRain = new Behavior("EscapeRain", register: true);

		public static readonly Behavior ReturnPrey = new Behavior("ReturnPrey", register: true);

		public Behavior(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public EggBug bug;

	private DebugDestinationVisualizer debugDestinationVisualizer;

	public float currentUtility;

	public float fear;

	public int noiseRectionDelay;

	public Behavior behavior;

	public WorldCoordinate? walkWithBug;

	private int idlePosCounter;

	public WorldCoordinate tempIdlePos;

	public Tracker.CreatureRepresentation focusCreature;

	public EggBugAI(AbstractCreature creature, World world)
		: base(creature, world)
	{
		bug = creature.realizedCreature as EggBug;
		bug.AI = this;
		AddModule(new StandardPather(this, world, creature));
		base.pathFinder.stepsPerFrame = 50;
		AddModule(new Tracker(this, 10, 10, bug.FireBug ? 150 : 450, 0.5f, 5, 5, 10));
		AddModule(new ThreatTracker(this, 3));
		AddModule(new RainTracker(this));
		AddModule(new DenFinder(this, creature));
		if (bug.FireBug)
		{
			AddModule(new PreyTracker(this, 5, 0.9f, 3f, 70f, 0.5f));
		}
		AddModule(new NoiseTracker(this, base.tracker));
		AddModule(new UtilityComparer(this));
		AddModule(new RelationshipTracker(this, base.tracker));
		FloatTweener.FloatTween smoother = new FloatTweener.FloatTweenUpAndDown(new FloatTweener.FloatTweenBasic(FloatTweener.TweenType.Lerp, 0.5f), new FloatTweener.FloatTweenBasic(FloatTweener.TweenType.Tick, 0.005f));
		base.utilityComparer.AddComparedModule(base.threatTracker, smoother, 1f, 1.1f);
		base.utilityComparer.AddComparedModule(base.rainTracker, null, 1f, 1.1f);
		if (bug.FireBug)
		{
			smoother = new FloatTweener.FloatTweenBasic(FloatTweener.TweenType.Lerp, 0.15f);
			base.utilityComparer.AddComparedModule(base.preyTracker, smoother, 0.6f, 1f);
		}
		behavior = Behavior.Idle;
	}

	public override void Update()
	{
		base.Update();
		if (bug.room == null)
		{
			return;
		}
		if (ModManager.MSC && bug.LickedByPlayer != null)
		{
			base.tracker.SeeCreature(bug.LickedByPlayer.abstractCreature);
		}
		base.pathFinder.walkPastPointOfNoReturn = stranded || !base.denFinder.GetDenPosition().HasValue || !base.pathFinder.CoordinatePossibleToGetBackFrom(base.denFinder.GetDenPosition().Value) || base.threatTracker.Utility() > 0.95f;
		if (bug.sitting)
		{
			base.noiseTracker.hearingSkill = 2f;
		}
		else
		{
			base.noiseTracker.hearingSkill = 0.2f;
		}
		base.utilityComparer.GetUtilityTracker(base.threatTracker).weight = Custom.LerpMap(base.threatTracker.ThreatOfTile(creature.pos, accountThreatCreatureAccessibility: true), 0.1f, 2f, 0.1f, 1f, 0.5f);
		AIModule aIModule = base.utilityComparer.HighestUtilityModule();
		currentUtility = base.utilityComparer.HighestUtility();
		int num;
		if (bug.FireBug)
		{
			num = ((bug.eggsLeft <= 0) ? 1 : 0);
			if (num != 0 && currentUtility < 0.02f)
			{
				behavior = Behavior.Hunt;
			}
		}
		else
		{
			num = 0;
		}
		if (aIModule != null)
		{
			if (aIModule is ThreatTracker)
			{
				behavior = Behavior.Flee;
			}
			else if (aIModule is RainTracker)
			{
				behavior = Behavior.EscapeRain;
			}
			else if (bug.FireBug && aIModule is PreyTracker && bug.eggsLeft <= 0 && base.preyTracker.MostAttractivePrey != null && base.preyTracker.MostAttractivePrey.representedCreature.realizedCreature != null && !base.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.dead && bug.grasps[0] == null)
			{
				behavior = MoreSlugcatsEnums.EggBugBehavior.Kill;
			}
		}
		if (num == 0 && currentUtility < 0.02f)
		{
			behavior = Behavior.Idle;
		}
		if (behavior == Behavior.Idle || behavior == Behavior.Hunt)
		{
			bug.runSpeed = Custom.LerpAndTick(bug.runSpeed, 0.5f + 0.5f * Mathf.Max(base.threatTracker.Utility(), fear), 0.01f, 1f / 60f);
			if (walkWithBug.HasValue)
			{
				creature.abstractAI.SetDestination(walkWithBug.Value);
				if (Random.value < 0.02f || Custom.ManhattanDistance(creature.pos, walkWithBug.Value) < 4)
				{
					walkWithBug = null;
				}
			}
			else
			{
				bool flag = base.pathFinder.GetDestination.room != bug.room.abstractRoom.index;
				if (!flag && idlePosCounter <= 0)
				{
					int abstractNode = bug.room.abstractRoom.RandomNodeInRoom().abstractNode;
					if (bug.room.abstractRoom.nodes[abstractNode].type == AbstractRoomNode.Type.Exit)
					{
						int num2 = bug.room.abstractRoom.CommonToCreatureSpecificNodeIndex(abstractNode, bug.Template);
						if (num2 > -1)
						{
							int num3 = bug.room.aimap.ExitDistanceForCreatureAndCheckNeighbours(bug.abstractCreature.pos.Tile, num2, bug.Template);
							if (num3 > -1 && num3 < 400)
							{
								AbstractRoom abstractRoom = bug.room.game.world.GetAbstractRoom(bug.room.abstractRoom.connections[abstractNode]);
								if (abstractRoom != null)
								{
									WorldCoordinate worldCoordinate = abstractRoom.RandomNodeInRoom();
									if (base.pathFinder.CoordinateReachableAndGetbackable(worldCoordinate))
									{
										Custom.Log("bug leaving room");
										creature.abstractAI.SetDestination(worldCoordinate);
										idlePosCounter = Random.Range(200, 500);
										flag = true;
									}
								}
							}
						}
					}
				}
				if (!flag)
				{
					WorldCoordinate coord = new WorldCoordinate(bug.room.abstractRoom.index, Random.Range(0, bug.room.TileWidth), Random.Range(0, bug.room.TileHeight), -1);
					if (IdleScore(coord) < IdleScore(tempIdlePos))
					{
						tempIdlePos = coord;
					}
					if (IdleScore(tempIdlePos) < IdleScore(base.pathFinder.GetDestination) + Custom.LerpMap(idlePosCounter, 0f, 300f, 100f, -300f))
					{
						SetDestination(tempIdlePos);
						idlePosCounter = Random.Range(200, 800);
						tempIdlePos = new WorldCoordinate(bug.room.abstractRoom.index, Random.Range(0, bug.room.TileWidth), Random.Range(0, bug.room.TileHeight), -1);
					}
				}
				idlePosCounter--;
			}
		}
		else if (behavior == Behavior.Flee)
		{
			bug.runSpeed = Custom.LerpAndTick(bug.runSpeed, 1f, 0.01f, 0.1f);
			creature.abstractAI.SetDestination(base.threatTracker.FleeTo(creature.pos, 10, 30, considerLeavingRoom: true));
			if (Random.value < base.threatTracker.Panic && base.threatTracker.mostThreateningCreature != null && base.threatTracker.mostThreateningCreature.representedCreature.realizedCreature != null && base.threatTracker.mostThreateningCreature.representedCreature.realizedCreature.room == bug.room)
			{
				BodyChunk bodyChunk = base.threatTracker.mostThreateningCreature.representedCreature.realizedCreature.bodyChunks[Random.Range(0, base.threatTracker.mostThreateningCreature.representedCreature.realizedCreature.bodyChunks.Length)];
				if (!bug.safariControlled && Custom.DistLess(bug.mainBodyChunk.pos, bodyChunk.pos, bug.mainBodyChunk.rad + bodyChunk.rad + 40f * fear))
				{
					bug.TryJump(bodyChunk.pos);
				}
			}
		}
		else if (behavior == Behavior.EscapeRain)
		{
			bug.runSpeed = Custom.LerpAndTick(bug.runSpeed, 1f, 0.01f, 0.1f);
			if (base.denFinder.GetDenPosition().HasValue)
			{
				creature.abstractAI.SetDestination(base.denFinder.GetDenPosition().Value);
			}
		}
		else if (bug.FireBug && behavior == MoreSlugcatsEnums.EggBugBehavior.Kill)
		{
			Tracker.CreatureRepresentation mostAttractivePrey = base.preyTracker.MostAttractivePrey;
			if (mostAttractivePrey != null)
			{
				LethalBehavior(mostAttractivePrey);
			}
			bug.runSpeed = Custom.LerpAndTick(bug.runSpeed, 1f, 0.025f, 0.1f);
		}
		fear = Custom.LerpAndTick(fear, Mathf.Max(base.utilityComparer.GetUtilityTracker(base.threatTracker).SmoothedUtility(), Mathf.Pow(base.threatTracker.Panic, 0.7f)), 0.07f, 1f / 30f);
		if (noiseRectionDelay > 0)
		{
			noiseRectionDelay--;
		}
	}

	private float IdleScore(WorldCoordinate coord)
	{
		if (coord.room != creature.pos.room || !base.pathFinder.CoordinateReachableAndGetbackable(coord) || bug.room.aimap.getAItile(coord).acc >= AItile.Accessibility.Wall)
		{
			return float.MaxValue;
		}
		float num = 1f;
		if (base.pathFinder.CoordinateReachableAndGetbackable(coord + new IntVector2(0, -1)))
		{
			num += 10f;
		}
		if (bug.room.aimap.getAItile(coord).narrowSpace)
		{
			num += 50f;
		}
		num += base.threatTracker.ThreatOfTile(coord, accountThreatCreatureAccessibility: true) * 1000f;
		num += base.threatTracker.ThreatOfTile(bug.room.GetWorldCoordinate((bug.room.MiddleOfTile(coord) + bug.room.MiddleOfTile(creature.pos)) / 2f), accountThreatCreatureAccessibility: true) * 1000f;
		for (int i = 0; i < base.noiseTracker.sources.Count; i++)
		{
			num += Custom.LerpMap(Vector2.Distance(bug.room.MiddleOfTile(coord), base.noiseTracker.sources[i].pos), 40f, 400f, 100f, 0f);
		}
		return num;
	}

	public override bool WantToStayInDenUntilEndOfCycle()
	{
		return base.rainTracker.Utility() > 0.01f;
	}

	public void CollideWithKin(EggBug otherBug)
	{
		if (otherBug.FireBug == bug.FireBug && Custom.ManhattanDistance(creature.pos, otherBug.AI.pathFinder.GetDestination) >= 4 && ((otherBug.abstractCreature.personality.dominance > bug.abstractCreature.personality.dominance && !otherBug.sitting) || bug.sitting || otherBug.AI.pathFinder.GetDestination.room != otherBug.room.abstractRoom.index))
		{
			walkWithBug = otherBug.AI.pathFinder.GetDestination;
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
		if (bug.FireBug && relationship.type == CreatureTemplate.Relationship.Type.Eats)
		{
			return base.preyTracker;
		}
		if (relationship.type == CreatureTemplate.Relationship.Type.Afraid)
		{
			return base.threatTracker;
		}
		return null;
	}

	RelationshipTracker.TrackedCreatureState IUseARelationshipTracker.CreateTrackedCreatureState(RelationshipTracker.DynamicRelationship rel)
	{
		return new RelationshipTracker.TrackedCreatureState();
	}

	CreatureTemplate.Relationship IUseARelationshipTracker.UpdateDynamicRelationship(RelationshipTracker.DynamicRelationship dRelation)
	{
		if (dRelation.trackerRep.VisualContact)
		{
			dRelation.state.alive = dRelation.trackerRep.representedCreature.state.alive;
		}
		CreatureTemplate.Relationship result = StaticRelationship(dRelation.trackerRep.representedCreature);
		if (result.type == CreatureTemplate.Relationship.Type.Afraid && !dRelation.state.alive)
		{
			result.intensity = 0f;
		}
		return result;
	}

	public override PathCost TravelPreference(MovementConnection coord, PathCost cost)
	{
		float val = Mathf.Max(0f, base.threatTracker.ThreatOfTile(coord.destinationCoord, accountThreatCreatureAccessibility: false) - base.threatTracker.ThreatOfTile(creature.pos, accountThreatCreatureAccessibility: false));
		return new PathCost(cost.resistance + Custom.LerpMap(val, 0f, 1.5f, 0f, 10000f, 5f), cost.legality);
	}

	public override void CreatureSpotted(bool firstSpot, Tracker.CreatureRepresentation otherCreature)
	{
		if (!firstSpot && Random.value > fear)
		{
			return;
		}
		CreatureTemplate.Relationship relationship = DynamicRelationship(otherCreature);
		if (!(relationship.type == CreatureTemplate.Relationship.Type.Ignores))
		{
			if (!bug.safariControlled && firstSpot && relationship.type == CreatureTemplate.Relationship.Type.Afraid && relationship.intensity > 0.06f && Custom.DistLess(bug.DangerPos, bug.room.MiddleOfTile(otherCreature.BestGuessForPosition()), Custom.LerpMap(relationship.intensity, 0.06f, 0.5f, 50f, 300f)))
			{
				bug.TryJump(bug.room.MiddleOfTile(otherCreature.BestGuessForPosition()));
			}
			if (relationship.intensity > (firstSpot ? 0.02f : 0.1f))
			{
				bug.Suprise(bug.room.MiddleOfTile(otherCreature.BestGuessForPosition()));
			}
		}
	}

	public void ReactToNoise(NoiseTracker.TheorizedSource source, InGameNoise noise)
	{
		bug.antennaDir = noise.pos;
		bug.antennaAttention = 1f;
		if (noiseRectionDelay <= 0)
		{
			if (!bug.safariControlled && noise.strength > 160f && Custom.DistLess(noise.pos, bug.mainBodyChunk.pos, Mathf.Lerp(bug.sitting ? 300f : 100f, 600f, fear)))
			{
				bug.TryJump(noise.pos);
			}
			bug.Suprise(noise.pos);
			noiseRectionDelay = Random.Range(0, 30);
		}
	}

	public bool UnpleasantFallRisk(IntVector2 tile)
	{
		if (!bug.room.GetTile(bug.room.aimap.getAItile(tile).fallRiskTile).AnyWater && bug.room.aimap.getAItile(tile).fallRiskTile.y >= 0 && bug.room.aimap.getAItile(tile).fallRiskTile.y >= tile.y - 20)
		{
			if (bug.room.aimap.getAItile(tile).fallRiskTile.y < tile.y - 10)
			{
				return !base.pathFinder.CoordinatePossibleToGetBackFrom(bug.room.GetWorldCoordinate(bug.room.aimap.getAItile(tile).fallRiskTile));
			}
			return false;
		}
		return true;
	}

	private bool FallRisk(IntVector2 tile)
	{
		return bug.room.aimap.getAItile(tile).fallRiskTile.y < creature.pos.y - 5;
	}

	private void LethalBehavior(Tracker.CreatureRepresentation target)
	{
		WorldCoordinate destination = target.BestGuessForPosition();
		creature.abstractAI.SetDestination(destination);
		for (int i = 0; i < target.representedCreature.realizedCreature.bodyChunks.Length; i++)
		{
			float dst = 15f;
			if (target.representedCreature.realizedCreature == null || target.representedCreature.realizedCreature.room == null || bug.room == null || target.representedCreature.realizedCreature.room.abstractRoom.index != bug.room.abstractRoom.index || target.representedCreature.realizedCreature.inShortcut || !Custom.DistLess(target.representedCreature.realizedCreature.bodyChunks[i].pos, bug.mainBodyChunk.pos, dst))
			{
				continue;
			}
			Creature realizedCreature = target.representedCreature.realizedCreature;
			if (realizedCreature.dead)
			{
				continue;
			}
			BodyChunk[] bodyChunks = realizedCreature.bodyChunks;
			foreach (BodyChunk bodyChunk in bodyChunks)
			{
				if (Custom.DistLess(bug.mainBodyChunk.pos + Custom.DirVec(bug.bodyChunks[1].pos, bug.mainBodyChunk.pos) * 20f, bodyChunk.pos, bodyChunk.rad + 25f) && bug.Grab(bodyChunk.owner, 0, bodyChunk.index, Creature.Grasp.Shareability.CanOnlyShareWithNonExclusive, Random.value, overrideEquallyDominant: true, pacifying: true))
				{
					break;
				}
			}
		}
	}

	private void AggressiveBehavior(Tracker.CreatureRepresentation target, float spitChance)
	{
		WorldCoordinate destination = target.BestGuessForPosition();
		creature.abstractAI.SetDestination(destination);
		focusCreature = target;
		float dst = 130f;
		float num = 0.033f;
		float num2 = 110f;
		if (focusCreature.VisualContact && num2 < 1f && Random.value < num && !UnpleasantFallRisk(creature.pos.Tile) && !FallRisk(creature.pos.Tile) && !UnpleasantFallRisk(focusCreature.representedCreature.pos.Tile) && Custom.DistLess(creature.realizedCreature.mainBodyChunk.pos, focusCreature.representedCreature.realizedCreature.mainBodyChunk.pos, dst) && !bug.room.aimap.getAItile(focusCreature.representedCreature.pos.Tile).narrowSpace && !bug.room.aimap.getAItile(creature.pos).narrowSpace && bug.room.GetTile(bug.mainBodyChunk.pos + new Vector2(0f, -20f)).Solid && bug.Footing)
		{
			bug.TryJump(new Vector2(destination.x, destination.y));
		}
	}
}
