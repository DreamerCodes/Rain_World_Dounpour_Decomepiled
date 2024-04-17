using System;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class JetFishAI : ArtificialIntelligence, IUseARelationshipTracker, IReactToSocialEvents
{
	public class Behavior : ExtEnum<Behavior>
	{
		public static readonly Behavior Idle = new Behavior("Idle", register: true);

		public static readonly Behavior Flee = new Behavior("Flee", register: true);

		public static readonly Behavior Hunt = new Behavior("Hunt", register: true);

		public static readonly Behavior EscapeRain = new Behavior("EscapeRain", register: true);

		public static readonly Behavior ReturnPrey = new Behavior("ReturnPrey", register: true);

		public static readonly Behavior GoToFood = new Behavior("GoToFood", register: true);

		public static readonly Behavior GetUnstuck = new Behavior("GetUnstuck", register: true);

		public Behavior(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public JetFish fish;

	private Vector2? fgp;

	public WorldCoordinate? exploreCoordinate;

	public AbstractCreature getAwayFromCreature;

	public int getAwayCounter;

	private DebugDestinationVisualizer debugDestinationVisualizer;

	public int attackCounter;

	public float currentUtility;

	public PhysicalObject goToFood;

	public Behavior behavior;

	public Tracker.CreatureRepresentation focusCreature;

	public DebugSprite dbSprite;

	private SocialEventRecognizer.OwnedItemOnGround pendingGiftRecieved;

	public Vector2? floatGoalPos
	{
		get
		{
			return fgp;
		}
		set
		{
			fgp = value;
			if (value.HasValue)
			{
				creature.abstractAI.SetDestination(fish.room.GetWorldCoordinate(value.Value));
			}
		}
	}

	public JetFishAI(AbstractCreature creature, World world)
		: base(creature, world)
	{
		fish = creature.realizedCreature as JetFish;
		fish.AI = this;
		AddModule(new FishPather(this, world, creature));
		AddModule(new Tracker(this, 10, 10, 250, 0.5f, 5, 5, 20));
		AddModule(new PreyTracker(this, 5, 1f, 5f, 150f, 0.05f));
		AddModule(new ThreatTracker(this, 3));
		AddModule(new RainTracker(this));
		AddModule(new DenFinder(this, creature));
		AddModule(new RelationshipTracker(this, base.tracker));
		AddModule(new UtilityComparer(this));
		AddModule(new StuckTracker(this, trackPastPositions: true, trackNotFollowingCurrentGoal: false));
		base.stuckTracker.AddSubModule(new StuckTracker.GetUnstuckPosCalculator(base.stuckTracker));
		base.stuckTracker.AddSubModule(new StuckTracker.StuckCloseToShortcutModule(base.stuckTracker));
		base.utilityComparer.AddComparedModule(base.threatTracker, null, 1f, 1.1f);
		base.utilityComparer.AddComparedModule(base.preyTracker, null, 0.5f, 1.1f);
		base.utilityComparer.AddComparedModule(base.rainTracker, null, 1f, 1.1f);
		base.utilityComparer.AddComparedModule(base.stuckTracker, null, 1f, 1.1f);
		behavior = Behavior.Idle;
	}

	public override void NewRoom(Room room)
	{
		base.NewRoom(room);
	}

	public override void Update()
	{
		focusCreature = null;
		base.Update();
		if (getAwayCounter > 0)
		{
			getAwayCounter--;
		}
		if (ModManager.MMF && pendingGiftRecieved != null && fish.grasps[0] != null && fish.grasps[0].grabbed == pendingGiftRecieved.item)
		{
			Custom.Log("Jetfish recieved gift!");
			SocialMemory.Relationship orInitiateRelationship = creature.realizedCreature.State.socialMemory.GetOrInitiateRelationship(pendingGiftRecieved.owner.abstractCreature.ID);
			if (orInitiateRelationship.like > -0.9f)
			{
				orInitiateRelationship.InfluenceLike(1.2f);
				orInitiateRelationship.InfluenceTempLike(1.7f);
			}
			if (pendingGiftRecieved.owner is Player)
			{
				creature.world.game.session.creatureCommunities.InfluenceLikeOfPlayer(creature.creatureTemplate.communityID, creature.world.RegionNumber, (!(creature.world.game.session is StoryGameSession)) ? (pendingGiftRecieved.owner as Player).playerState.playerNumber : 0, 0.1f, 0.2f, 0.1f);
			}
			pendingGiftRecieved = null;
		}
		AIModule aIModule = base.utilityComparer.HighestUtilityModule();
		currentUtility = base.utilityComparer.HighestUtility();
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
			else if (aIModule is PreyTracker)
			{
				behavior = Behavior.Hunt;
			}
			else if (aIModule is StuckTracker)
			{
				behavior = Behavior.GetUnstuck;
			}
		}
		if (goToFood != null)
		{
			if (!WantToEatObject(goToFood))
			{
				goToFood = null;
			}
			else if (currentUtility < 0.75f && fish.grasps[0] == null)
			{
				currentUtility = 0.75f;
				behavior = Behavior.GoToFood;
			}
		}
		if (currentUtility < 0.1f)
		{
			behavior = Behavior.Idle;
		}
		if (behavior != Behavior.Flee && fish.grasps[0] != null)
		{
			behavior = Behavior.ReturnPrey;
		}
		if (behavior == Behavior.Idle)
		{
			if (exploreCoordinate.HasValue)
			{
				creature.abstractAI.SetDestination(exploreCoordinate.Value);
				if (Custom.ManhattanDistance(creature.pos, exploreCoordinate.Value) < 5 || (UnityEngine.Random.value < 0.0125f && base.pathFinder.DoneMappingAccessibility && fish.room.aimap.TileAccessibleToCreature(creature.pos.x, creature.pos.y, creature.creatureTemplate) && !base.pathFinder.CoordinateReachableAndGetbackable(exploreCoordinate.Value)))
				{
					exploreCoordinate = null;
				}
			}
			else if (Custom.ManhattanDistance(creature.pos, base.pathFinder.GetDestination) < 5 || !base.pathFinder.CoordinateReachableAndGetbackable(base.pathFinder.GetDestination))
			{
				WorldCoordinate worldCoordinate = fish.room.GetWorldCoordinate(Custom.RestrictInRect(fish.mainBodyChunk.pos, new FloatRect(0f, 0f, fish.room.PixelWidth, fish.room.PixelHeight)) + Custom.RNV() * 200f);
				if (fish.room.IsPositionInsideBoundries(worldCoordinate.Tile) && base.pathFinder.CoordinateReachableAndGetbackable(worldCoordinate) && fish.room.VisualContact(creature.pos.Tile, worldCoordinate.Tile))
				{
					creature.abstractAI.SetDestination(worldCoordinate);
				}
			}
			if (UnityEngine.Random.value < 1f / (exploreCoordinate.HasValue ? 1600f : 80f))
			{
				WorldCoordinate worldCoordinate2 = new WorldCoordinate(fish.room.abstractRoom.index, UnityEngine.Random.Range(0, fish.room.TileWidth), UnityEngine.Random.Range(0, fish.room.TileHeight), -1);
				if (fish.room.aimap.TileAccessibleToCreature(worldCoordinate2.Tile, creature.creatureTemplate) && base.pathFinder.CoordinateReachableAndGetbackable(worldCoordinate2))
				{
					exploreCoordinate = worldCoordinate2;
				}
			}
			if (Custom.DistLess(creature.pos, base.pathFinder.GetDestination, 20f) && fish.room.VisualContact(creature.pos, base.pathFinder.GetDestination))
			{
				floatGoalPos = fish.room.MiddleOfTile(base.pathFinder.GetDestination);
			}
			else
			{
				floatGoalPos = null;
			}
		}
		else if (behavior == Behavior.Flee)
		{
			WorldCoordinate destination = base.threatTracker.FleeTo(creature.pos, 3, 30, currentUtility > 0.3f);
			if (base.threatTracker.mostThreateningCreature != null)
			{
				focusCreature = base.threatTracker.mostThreateningCreature;
			}
			creature.abstractAI.SetDestination(destination);
			floatGoalPos = null;
		}
		else if (behavior == Behavior.EscapeRain)
		{
			if (base.denFinder.GetDenPosition().HasValue)
			{
				creature.abstractAI.SetDestination(base.denFinder.GetDenPosition().Value);
			}
			floatGoalPos = null;
		}
		else if (behavior == Behavior.GoToFood)
		{
			creature.abstractAI.SetDestination(fish.room.GetWorldCoordinate(goToFood.firstChunk.pos));
		}
		else if (behavior == Behavior.Hunt)
		{
			attackCounter--;
			focusCreature = base.preyTracker.MostAttractivePrey;
			if (attackCounter > 0)
			{
				if (focusCreature.VisualContact)
				{
					floatGoalPos = focusCreature.representedCreature.realizedCreature.mainBodyChunk.pos;
				}
				else
				{
					creature.abstractAI.SetDestination(focusCreature.BestGuessForPosition());
				}
			}
			else if (focusCreature.VisualContact)
			{
				floatGoalPos = focusCreature.representedCreature.realizedCreature.mainBodyChunk.pos + Custom.DirVec(focusCreature.representedCreature.realizedCreature.mainBodyChunk.pos, fish.mainBodyChunk.pos) * 200f;
			}
			if (attackCounter < -50)
			{
				attackCounter = UnityEngine.Random.Range(200, 400);
			}
			if (focusCreature.VisualContact && focusCreature.representedCreature.realizedCreature.collisionLayer != fish.collisionLayer && Custom.DistLess(focusCreature.representedCreature.realizedCreature.mainBodyChunk.pos, fish.mainBodyChunk.pos, fish.mainBodyChunk.rad + focusCreature.representedCreature.realizedCreature.mainBodyChunk.rad))
			{
				fish.Collide(focusCreature.representedCreature.realizedCreature, 0, 0);
			}
		}
		else if (behavior == Behavior.ReturnPrey)
		{
			if (base.denFinder.GetDenPosition().HasValue)
			{
				creature.abstractAI.SetDestination(base.denFinder.GetDenPosition().Value);
			}
			floatGoalPos = null;
		}
		else if (behavior == Behavior.GetUnstuck)
		{
			creature.abstractAI.SetDestination(base.stuckTracker.getUnstuckPosCalculator.unstuckGoalPosition);
			if (UnityEngine.Random.value < Custom.LerpMap(base.stuckTracker.Utility(), 0.9f, 1f, 0f, 0.1f) && fish.room.GetTile(fish.mainBodyChunk.pos).AnyWater && !fish.enteringShortCut.HasValue && base.stuckTracker.stuckCloseToShortcutModule.foundShortCut.HasValue)
			{
				fish.enteringShortCut = base.stuckTracker.stuckCloseToShortcutModule.foundShortCut;
				base.stuckTracker.Reset();
			}
		}
	}

	public override float VisualScore(Vector2 lookAtPoint, float targetSpeed)
	{
		return base.VisualScore(lookAtPoint, targetSpeed) - Mathf.Pow(Mathf.InverseLerp(1f, -1f, Vector2.Dot((fish.bodyChunks[1].pos - fish.bodyChunks[0].pos).normalized, (fish.bodyChunks[1].pos - lookAtPoint).normalized)), 0.85f);
	}

	public override bool WantToStayInDenUntilEndOfCycle()
	{
		return base.rainTracker.Utility() > 0.01f;
	}

	public override void CreatureSpotted(bool firstSpot, Tracker.CreatureRepresentation creatureRep)
	{
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
		if (relationship.type == CreatureTemplate.Relationship.Type.Eats || relationship.type == CreatureTemplate.Relationship.Type.Antagonizes)
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
		if (dRelation.trackerRep.VisualContact)
		{
			dRelation.state.alive = dRelation.trackerRep.representedCreature.state.alive;
			if (dRelation.trackerRep.representedCreature.realizedCreature.grasps != null)
			{
				for (int i = 0; i < dRelation.trackerRep.representedCreature.realizedCreature.grasps.Length; i++)
				{
					if (dRelation.trackerRep.representedCreature.realizedCreature.grasps[i] != null && dRelation.trackerRep.representedCreature.realizedCreature.grasps[i].grabbed is JetFish)
					{
						SocialMemory.Relationship orInitiateRelationship = fish.State.socialMemory.GetOrInitiateRelationship(dRelation.trackerRep.representedCreature.ID);
						orInitiateRelationship.like = Mathf.Lerp(orInitiateRelationship.like, 0f, 5E-05f);
						break;
					}
				}
			}
		}
		if (getAwayCounter > 0 && dRelation.trackerRep.representedCreature == getAwayFromCreature)
		{
			return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f);
		}
		if (!dRelation.state.alive && dRelation.trackerRep.representedCreature.creatureTemplate.bodySize < creature.creatureTemplate.bodySize * 1.5f)
		{
			return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f);
		}
		CreatureTemplate.Relationship result = StaticRelationship(dRelation.trackerRep.representedCreature).Duplicate();
		if (result.type == CreatureTemplate.Relationship.Type.SocialDependent)
		{
			float like = creature.state.socialMemory.GetLike(dRelation.trackerRep.representedCreature.ID);
			if (like < 0.3f)
			{
				result = new CreatureTemplate.Relationship((like < -0.5f) ? CreatureTemplate.Relationship.Type.Eats : CreatureTemplate.Relationship.Type.Antagonizes, Mathf.Pow(Mathf.InverseLerp(0.3f, -1f, like), 2f));
			}
			for (int j = 0; j < base.tracker.CreaturesCount; j++)
			{
				if (base.tracker.GetRep(j).representedCreature.creatureTemplate.type == CreatureTemplate.Type.JetFish)
				{
					result.intensity = Mathf.Lerp(result.intensity, 1f, Mathf.InverseLerp(100f, 30f, Custom.ManhattanDistance(base.tracker.GetRep(j).BestGuessForPosition(), creature.pos)) * 0.5f);
				}
			}
			if (result.type == CreatureTemplate.Relationship.Type.Antagonizes)
			{
				result.intensity *= Mathf.InverseLerp(10f + Mathf.InverseLerp(0f, -1f, like) * 40f, 5f, Math.Abs(dRelation.trackerRep.BestGuessForPosition().y - fish.room.defaultWaterLevel));
			}
		}
		if (result.intensity == 0f)
		{
			result.type = CreatureTemplate.Relationship.Type.Ignores;
		}
		return result;
	}

	public bool WantToEatObject(PhysicalObject obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (!(obj is DangleFruit) && !(obj is SwollenWaterNut) && !(obj is LillyPuck) && !(obj is GlowWeed))
		{
			return false;
		}
		if (obj.room != null && obj.room == fish.room && obj.grabbedBy.Count == 0 && !obj.slatedForDeletetion && (base.pathFinder.CoordinateReachableAndGetbackable(fish.room.GetWorldCoordinate(obj.firstChunk.pos)) || base.pathFinder.CoordinateReachableAndGetbackable(fish.room.GetWorldCoordinate(obj.firstChunk.pos) + new IntVector2(0, -1)) || base.pathFinder.CoordinateReachableAndGetbackable(fish.room.GetWorldCoordinate(obj.firstChunk.pos) + new IntVector2(0, -2))))
		{
			return base.threatTracker.ThreatOfArea(fish.room.GetWorldCoordinate(obj.firstChunk.pos), accountThreatCreatureAccessibility: true) < 0.55f;
		}
		return false;
	}

	public void SocialEvent(SocialEventRecognizer.EventID ID, Creature subjectCrit, Creature objectCrit, PhysicalObject involvedItem)
	{
		if (!ModManager.MMF || !(subjectCrit is Player))
		{
			return;
		}
		Tracker.CreatureRepresentation creatureRepresentation = base.tracker.RepresentationForObject(subjectCrit, AddIfMissing: false);
		if (creatureRepresentation == null)
		{
			return;
		}
		Tracker.CreatureRepresentation creatureRepresentation2 = null;
		bool flag = objectCrit == fish;
		if (!flag)
		{
			creatureRepresentation2 = base.tracker.RepresentationForObject(objectCrit, AddIfMissing: false);
			if (creatureRepresentation2 == null)
			{
				return;
			}
		}
		if ((creatureRepresentation2 == null || creatureRepresentation.TicksSinceSeen <= 40 || creatureRepresentation2.TicksSinceSeen <= 40) && ID == SocialEventRecognizer.EventID.ItemOffering && flag && involvedItem != null && involvedItem is DangleFruit)
		{
			GiftRecieved(involvedItem.room.socialEventRecognizer.ItemOwnership(involvedItem));
		}
	}

	public void GiftRecieved(SocialEventRecognizer.OwnedItemOnGround giftOfferedToMe)
	{
		Custom.Log("Jetfish noticed gift!");
		pendingGiftRecieved = giftOfferedToMe;
	}
}
