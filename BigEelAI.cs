using RWCustom;
using UnityEngine;

public class BigEelAI : ArtificialIntelligence, IUseARelationshipTracker
{
	public class Behavior : ExtEnum<Behavior>
	{
		public static readonly Behavior Idle = new Behavior("Idle", register: true);

		public static readonly Behavior Hunt = new Behavior("Hunt", register: true);

		public static readonly Behavior EscapeRain = new Behavior("EscapeRain", register: true);

		public static readonly Behavior ReturnPrey = new Behavior("ReturnPrey", register: true);

		public Behavior(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public BigEel eel;

	public int hungerDelay;

	public float timeInThisRoom;

	private DebugDestinationVisualizer debugDestinationVisualizer;

	public float currentUtility;

	public Behavior behavior;

	public Tracker.CreatureRepresentation focusCreature;

	public BigEelAI(AbstractCreature creature, World world)
		: base(creature, world)
	{
		eel = creature.realizedCreature as BigEel;
		eel.AI = this;
		AddModule(new BigEelPather(this, world, creature));
		AddModule(new Tracker(this, 10, 5, 250, 0.5f, 5, 5, 20));
		AddModule(new PreyTracker(this, 5, 1f, 5f, 150f, 0.05f));
		AddModule(new RainTracker(this));
		AddModule(new RelationshipTracker(this, base.tracker));
		AddModule(new UtilityComparer(this));
		base.utilityComparer.AddComparedModule(base.preyTracker, null, 0.9f, 2f);
		base.utilityComparer.AddComparedModule(base.rainTracker, null, 1f, 1.1f);
		hungerDelay = 300;
		behavior = Behavior.Idle;
	}

	public override void NewRoom(Room room)
	{
		base.NewRoom(room);
		timeInThisRoom = 0f;
	}

	public override void Update()
	{
		focusCreature = null;
		base.Update();
		if (eel.room.IsPositionInsideBoundries(creature.pos.Tile))
		{
			timeInThisRoom += 1f;
		}
		AIModule aIModule = base.utilityComparer.HighestUtilityModule();
		currentUtility = base.utilityComparer.HighestUtility();
		if (aIModule != null)
		{
			if (aIModule is RainTracker)
			{
				behavior = Behavior.EscapeRain;
			}
			else if (aIModule is PreyTracker)
			{
				behavior = Behavior.Hunt;
			}
		}
		if (currentUtility < 0.1f)
		{
			behavior = Behavior.Idle;
		}
		if (hungerDelay > 0 && eel.room.IsPositionInsideBoundries(creature.pos.Tile))
		{
			hungerDelay--;
		}
		base.utilityComparer.GetUtilityTracker(base.preyTracker).weight = Mathf.InverseLerp(300f, 0f, hungerDelay) * Mathf.InverseLerp(1400f, 1000f, timeInThisRoom) * 0.9f;
		if (behavior == Behavior.Idle)
		{
			creature.abstractAI.AbstractBehavior(1);
			if (creature.abstractAI.destination.room == creature.pos.room && (!creature.abstractAI.destination.TileDefined || Custom.DistLess(creature.pos.Tile, base.pathFinder.GetDestination.Tile, 10f) || !eel.room.IsPositionInsideBoundries(base.pathFinder.GetDestination.Tile) || !base.pathFinder.CoordinateReachableAndGetbackable(base.pathFinder.GetDestination)))
			{
				WorldCoordinate worldCoordinate = eel.room.GetWorldCoordinate(new IntVector2(Random.Range(0, eel.room.TileWidth), Random.Range(0, Random.Range(0, eel.room.defaultWaterLevel))));
				if (base.pathFinder.CoordinateReachableAndGetbackable(worldCoordinate) && !Custom.DistLess(creature.pos.Tile, worldCoordinate.Tile, 10f))
				{
					creature.abstractAI.SetDestination(worldCoordinate);
				}
			}
		}
		else if (!(behavior == Behavior.EscapeRain))
		{
			if (behavior == Behavior.Hunt)
			{
				focusCreature = base.preyTracker.MostAttractivePrey;
				creature.abstractAI.SetDestination(focusCreature.BestGuessForPosition());
				if (focusCreature.VisualContact && Custom.DistLess(eel.mainBodyChunk.pos, focusCreature.representedCreature.realizedCreature.mainBodyChunk.pos, 400f))
				{
					eel.attackPos = focusCreature.representedCreature.realizedCreature.mainBodyChunk.pos;
				}
			}
			else
			{
				_ = behavior == Behavior.ReturnPrey;
			}
		}
		if (Random.value < 0.0033333334f && base.pathFinder.GetDestination.TileDefined)
		{
			creature.abstractAI.SetDestination(base.pathFinder.GetDestination + Custom.eightDirections[Random.Range(0, 8)]);
		}
	}

	public bool WantToSnapJaw()
	{
		if (eel.safariControlled && eel.inputWithDiagonals.HasValue && eel.inputWithDiagonals.Value.pckp)
		{
			return true;
		}
		for (int i = 0; i < base.tracker.CreaturesCount; i++)
		{
			if (base.tracker.GetRep(i).dynamicRelationship.currentRelationship.type == CreatureTemplate.Relationship.Type.Eats && base.tracker.GetRep(i).VisualContact && eel.InBiteArea(base.tracker.GetRep(i).representedCreature.realizedCreature.bodyChunks[Random.Range(0, base.tracker.GetRep(i).representedCreature.realizedCreature.bodyChunks.Length)].pos, Mathf.Lerp(-10f, 100f, Mathf.Pow(eel.jawChargeFatigue, 5f))))
			{
				return true;
			}
		}
		return false;
	}

	public bool WantToChargeJaw()
	{
		if (!eel.safariControlled)
		{
			if (behavior == Behavior.Hunt && focusCreature != null)
			{
				return Custom.DistLess(eel.mainBodyChunk.pos, eel.room.MiddleOfTile(focusCreature.BestGuessForPosition()), 200f);
			}
			return false;
		}
		if (eel.inputWithDiagonals.HasValue)
		{
			return eel.inputWithDiagonals.Value.pckp;
		}
		return false;
	}

	public override float VisualScore(Vector2 lookAtPoint, float targetSpeed)
	{
		return base.VisualScore(lookAtPoint, targetSpeed);
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
		if (eel.AmIHoldingCreature(otherCreature))
		{
			return null;
		}
		if (otherCreature.creatureTemplate.smallCreature)
		{
			return null;
		}
		return new Tracker.ElaborateCreatureRepresentation(base.tracker, otherCreature, 1f, 3);
	}

	AIModule IUseARelationshipTracker.ModuleToTrackRelationship(CreatureTemplate.Relationship relationship)
	{
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
		if (eel.AmIHoldingCreature(dRelation.trackerRep.representedCreature) || dRelation.trackerRep.representedCreature.creatureTemplate.smallCreature)
		{
			return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.DoesntTrack, 0f);
		}
		CreatureTemplate.Relationship result = StaticRelationship(dRelation.trackerRep.representedCreature);
		if (result.type == CreatureTemplate.Relationship.Type.Eats)
		{
			float intensity = result.intensity;
			intensity *= Mathf.InverseLerp(eel.room.defaultWaterLevel + 6, eel.room.defaultWaterLevel - 40, dRelation.trackerRep.BestGuessForPosition().y);
			intensity *= Mathf.InverseLerp(0f, 5f, dRelation.trackerRep.representedCreature.creatureTemplate.bodySize);
			intensity = Mathf.Pow(intensity, 0.2f);
			result = new CreatureTemplate.Relationship(result.type, intensity);
		}
		return result;
	}
}
