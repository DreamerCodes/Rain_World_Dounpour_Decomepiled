using UnityEngine;

public class MirosBirdAI : ArtificialIntelligence, IUseARelationshipTracker, ILookingAtCreatures
{
	public class Behavior : ExtEnum<Behavior>
	{
		public static readonly Behavior Idle = new Behavior("Idle", register: true);

		public static readonly Behavior Flee = new Behavior("Flee", register: true);

		public static readonly Behavior Hunt = new Behavior("Hunt", register: true);

		public static readonly Behavior EscapeRain = new Behavior("EscapeRain", register: true);

		public static readonly Behavior ReturnPrey = new Behavior("ReturnPrey", register: true);

		public static readonly Behavior GetUnstuck = new Behavior("GetUnstuck", register: true);

		public Behavior(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public MirosBird bird;

	public CreatureLooker creatureLooker;

	private DebugDestinationVisualizer debugDestinationVisualizer;

	public float currentUtility;

	private bool enteredRoom;

	public Behavior behavior;

	public Tracker.CreatureRepresentation focusCreature;

	public bool AllowMovementBetweenRooms => enteredRoom;

	public MirosBirdAI(AbstractCreature creature, World world)
		: base(creature, world)
	{
		bird = creature.realizedCreature as MirosBird;
		bird.AI = this;
		AddModule(new MirosBirdPather(this, world, creature));
		AddModule(new Tracker(this, 4, 10, 160, 0.25f, 5, 1, 5));
		AddModule(new PreyTracker(this, 5, 1f, 5f, 15f, 0.95f));
		AddModule(new ThreatTracker(this, 3));
		AddModule(new RainTracker(this));
		AddModule(new DenFinder(this, creature));
		AddModule(new StuckTracker(this, trackPastPositions: true, trackNotFollowingCurrentGoal: false));
		base.stuckTracker.AddSubModule(new StuckTracker.GetUnstuckPosCalculator(base.stuckTracker));
		AddModule(new RelationshipTracker(this, base.tracker));
		AddModule(new UtilityComparer(this));
		base.utilityComparer.AddComparedModule(base.threatTracker, null, 0.9f, 1.1f);
		base.utilityComparer.AddComparedModule(base.preyTracker, null, 0.5f, 1.1f);
		base.utilityComparer.AddComparedModule(base.rainTracker, null, 0.9f, 1.1f);
		base.utilityComparer.AddComparedModule(base.stuckTracker, null, 1f, 1.1f);
		behavior = Behavior.Idle;
		creatureLooker = new CreatureLooker(this, base.tracker, creature.realizedCreature, 0.0025f, 30);
	}

	public override void NewRoom(Room room)
	{
		base.NewRoom(room);
		enteredRoom = false;
	}

	public override void Update()
	{
		base.Update();
		creatureLooker.Update();
		for (int num = base.tracker.CreaturesCount - 1; num >= 0; num--)
		{
			if (base.tracker.GetRep(num).TicksSinceSeen > 160)
			{
				base.tracker.ForgetCreature(base.tracker.GetRep(num).representedCreature);
			}
		}
		if (!enteredRoom && bird.room != null && creature.pos.x > 2 && creature.pos.x < bird.room.TileWidth - 3)
		{
			enteredRoom = true;
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
		if (currentUtility < 0.1f)
		{
			behavior = Behavior.Idle;
		}
		if (bird.grasps[0] != null && behavior != Behavior.Flee && behavior != Behavior.EscapeRain)
		{
			behavior = Behavior.ReturnPrey;
		}
		if (behavior == Behavior.Idle)
		{
			creature.abstractAI.SetDestination(base.denFinder.GetDenPosition().Value);
		}
		else if (behavior == Behavior.Flee)
		{
			WorldCoordinate destination = base.threatTracker.FleeTo(creature.pos, 5, 30, currentUtility > 0.3f);
			if (base.threatTracker.mostThreateningCreature != null)
			{
				focusCreature = base.threatTracker.mostThreateningCreature;
			}
			creature.abstractAI.SetDestination(destination);
		}
		else if (behavior == Behavior.EscapeRain)
		{
			if (base.denFinder.GetDenPosition().HasValue)
			{
				creature.abstractAI.SetDestination(base.denFinder.GetDenPosition().Value);
			}
		}
		else if (behavior == Behavior.Hunt)
		{
			focusCreature = base.preyTracker.MostAttractivePrey;
			creature.abstractAI.SetDestination(base.preyTracker.MostAttractivePrey.BestGuessForPosition());
		}
		else if (behavior == Behavior.ReturnPrey)
		{
			if (base.denFinder.GetDenPosition().HasValue)
			{
				creature.abstractAI.SetDestination(base.denFinder.GetDenPosition().Value);
			}
		}
		else if (behavior == Behavior.GetUnstuck)
		{
			creature.abstractAI.SetDestination(base.stuckTracker.getUnstuckPosCalculator.unstuckGoalPosition);
		}
	}

	public override float VisualScore(Vector2 lookAtPoint, float targetSpeed)
	{
		return base.VisualScore(lookAtPoint, targetSpeed) - Mathf.InverseLerp(0.7f, 0.3f, Vector2.Dot((bird.neck.Tip.pos - lookAtPoint).normalized, (bird.neck.Tip.pos - bird.Head.pos).normalized));
	}

	public bool DoIWantToBiteCreature(AbstractCreature creature)
	{
		if (creature.creatureTemplate.type == CreatureTemplate.Type.MirosBird || creature.creatureTemplate.smallCreature)
		{
			return false;
		}
		return true;
	}

	public override bool WantToStayInDenUntilEndOfCycle()
	{
		return base.rainTracker.Utility() > 0.01f;
	}

	public override void CreatureSpotted(bool firstSpot, Tracker.CreatureRepresentation creatureRep)
	{
		creatureLooker.ReevaluateLookObject(creatureRep, firstSpot ? 6f : 2f);
	}

	public override Tracker.CreatureRepresentation CreateTrackerRepresentationForCreature(AbstractCreature otherCreature)
	{
		if (otherCreature.creatureTemplate.smallCreature)
		{
			return new Tracker.SimpleCreatureRepresentation(base.tracker, otherCreature, 0f, forgetWhenNotVisible: false);
		}
		return new Tracker.ElaborateCreatureRepresentation(base.tracker, otherCreature, 1f, 3);
	}

	public override PathCost TravelPreference(MovementConnection coord, PathCost cost)
	{
		if (!coord.destinationCoord.TileDefined || coord.destinationCoord.room != bird.room.abstractRoom.index)
		{
			return cost;
		}
		return new PathCost(cost.resistance + Mathf.Abs(5f - (float)bird.room.aimap.getAItile(coord.DestTile).floorAltitude) * 30f * Mathf.InverseLerp(150f, 40f, bird.stuckCounter), cost.legality);
	}

	AIModule IUseARelationshipTracker.ModuleToTrackRelationship(CreatureTemplate.Relationship relationship)
	{
		if (relationship.type == CreatureTemplate.Relationship.Type.Afraid)
		{
			return base.threatTracker;
		}
		if (relationship.type == CreatureTemplate.Relationship.Type.Eats)
		{
			return base.preyTracker;
		}
		return null;
	}

	RelationshipTracker.TrackedCreatureState IUseARelationshipTracker.CreateTrackedCreatureState(RelationshipTracker.DynamicRelationship rel)
	{
		return null;
	}

	CreatureTemplate.Relationship IUseARelationshipTracker.UpdateDynamicRelationship(RelationshipTracker.DynamicRelationship dRelation)
	{
		return dRelation.currentRelationship;
	}

	public float CreatureInterestBonus(Tracker.CreatureRepresentation crit, float score)
	{
		if (crit.representedCreature.creatureTemplate.smallCreature)
		{
			return -1f;
		}
		if (crit == focusCreature)
		{
			return score * 10f;
		}
		if (Mathf.Abs(bird.moveDir.x) > 0.2f && crit.BestGuessForPosition().room == creature.pos.room && crit.BestGuessForPosition().x < creature.pos.x == bird.moveDir.x > 0f)
		{
			return -1f;
		}
		return score;
	}

	public Tracker.CreatureRepresentation ForcedLookCreature()
	{
		return null;
	}

	public void LookAtNothing()
	{
	}
}
