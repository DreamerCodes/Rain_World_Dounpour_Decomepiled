using System;
using System.Collections.Generic;
using Expedition;
using Noise;
using RWCustom;
using UnityEngine;

public abstract class ArtificialIntelligence
{
	public AbstractCreature creature;

	public List<AIModule> modules;

	public bool stranded;

	public bool seeThroughWalls;

	public int timeInRoom;

	public int lastRoom = -1;

	public PathFinder pathFinder { get; private set; }

	public Tracker tracker { get; private set; }

	public NoiseTracker noiseTracker
	{
		get
		{
			if (tracker == null)
			{
				return null;
			}
			return tracker.noiseTracker;
		}
	}

	public ObstacleTracker obstacleTracker { get; private set; }

	public ThreatTracker threatTracker { get; private set; }

	public PreyTracker preyTracker { get; private set; }

	public RainTracker rainTracker { get; private set; }

	public DenFinder denFinder { get; private set; }

	public AgressionTracker agressionTracker { get; private set; }

	public UtilityComparer utilityComparer { get; private set; }

	public MissionTracker missionTracker { get; private set; }

	public NodeFinder secondaryNodeFinder { get; private set; }

	public RelationshipTracker relationshipTracker { get; private set; }

	public ItemTracker itemTracker { get; private set; }

	public DiscomfortTracker discomfortTracker { get; private set; }

	public StuckTracker stuckTracker { get; private set; }

	public InjuryTracker injuryTracker { get; private set; }

	public FriendTracker friendTracker { get; private set; }

	public ArtificialIntelligence(AbstractCreature creature, World world)
	{
		this.creature = creature;
		if (modules == null)
		{
			modules = new List<AIModule>();
		}
	}

	public void AddModule(AIModule module)
	{
		if (modules == null)
		{
			modules = new List<AIModule>();
		}
		modules.Add(module);
		if (module is PathFinder)
		{
			pathFinder = module as PathFinder;
		}
		else if (module is Tracker)
		{
			tracker = module as Tracker;
		}
		else if (module is ObstacleTracker)
		{
			obstacleTracker = module as ObstacleTracker;
		}
		else if (module is ThreatTracker)
		{
			threatTracker = module as ThreatTracker;
		}
		else if (module is PreyTracker)
		{
			preyTracker = module as PreyTracker;
		}
		else if (module is RainTracker)
		{
			rainTracker = module as RainTracker;
		}
		else if (module is DenFinder)
		{
			denFinder = module as DenFinder;
		}
		else if (module is NodeFinder)
		{
			secondaryNodeFinder = module as NodeFinder;
		}
		else if (module is UtilityComparer)
		{
			utilityComparer = module as UtilityComparer;
		}
		else if (module is AgressionTracker)
		{
			agressionTracker = module as AgressionTracker;
		}
		else if (module is MissionTracker)
		{
			missionTracker = module as MissionTracker;
		}
		else if (module is RelationshipTracker)
		{
			relationshipTracker = module as RelationshipTracker;
		}
		else if (module is ItemTracker)
		{
			itemTracker = module as ItemTracker;
		}
		else if (module is DiscomfortTracker)
		{
			discomfortTracker = module as DiscomfortTracker;
		}
		else if (module is StuckTracker)
		{
			stuckTracker = module as StuckTracker;
		}
		else if (module is InjuryTracker)
		{
			injuryTracker = module as InjuryTracker;
		}
		else if (module is FriendTracker)
		{
			friendTracker = module as FriendTracker;
		}
	}

	public bool VisualContact(WorldCoordinate lookAtCoord, float bonus)
	{
		if (lookAtCoord.room == creature.pos.room && lookAtCoord.TileDefined)
		{
			return VisualContact(creature.realizedCreature.room.MiddleOfTile(lookAtCoord.Tile), bonus);
		}
		return false;
	}

	public bool VisualContact(Vector2 lookAtPoint, float bonus)
	{
		if (creature.realizedCreature.Blinded)
		{
			return false;
		}
		if (VisualScore(lookAtPoint, bonus) <= 0f)
		{
			return false;
		}
		return creature.Room.realizedRoom.VisualContact(creature.realizedCreature.VisionPoint, lookAtPoint);
	}

	public bool VisualContact(BodyChunk chunk)
	{
		return VisualContact(chunk.pos, chunk.VisibilityBonus(creature.creatureTemplate.movementBasedVision));
	}

	public virtual float VisualScore(Vector2 lookAtPoint, float bonus)
	{
		try
		{
			if (!Custom.DistLess(creature.realizedCreature.VisionPoint, lookAtPoint, creature.creatureTemplate.visualRadius * (1f + bonus)))
			{
				return 0f;
			}
			if (creature.Room.realizedRoom == null)
			{
				return 0f;
			}
			float num = Mathf.InverseLerp(creature.creatureTemplate.visualRadius * (1f + bonus), 0f, Vector2.Distance(creature.realizedCreature.VisionPoint, lookAtPoint));
			if (creature.Room.realizedRoom.water)
			{
				if (creature.Room.realizedRoom.water && creature.Room.realizedRoom.GetTile(creature.realizedCreature.VisionPoint).DeepWater != creature.Room.realizedRoom.GetTile(lookAtPoint).DeepWater)
				{
					num -= 1f - creature.creatureTemplate.throughSurfaceVision;
				}
				if (creature.Room.realizedRoom.GetTile(creature.realizedCreature.VisionPoint).DeepWater || creature.Room.realizedRoom.GetTile(lookAtPoint).DeepWater)
				{
					num -= 1f - creature.creatureTemplate.waterVision;
				}
			}
			if (creature.Room.realizedRoom.aimap.getAItile(lookAtPoint).narrowSpace)
			{
				num -= 0.5f;
			}
			for (int num2 = creature.Room.realizedRoom.visionObscurers.Count - 1; num2 >= 0; num2--)
			{
				num = creature.Room.realizedRoom.visionObscurers[num2].VisionScore(creature.realizedCreature.VisionPoint, lookAtPoint, num);
			}
			return num;
		}
		catch (NullReferenceException)
		{
			return 0f;
		}
	}

	public virtual void CreatureSpotted(bool firstSpot, Tracker.CreatureRepresentation otherCreature)
	{
	}

	public void SetDestination(WorldCoordinate destination)
	{
		if (pathFinder != null)
		{
			pathFinder.SetDestination(destination);
		}
	}

	public virtual void Update()
	{
		timeInRoom++;
		for (int i = 0; i < modules.Count; i++)
		{
			modules[i].Update();
		}
		if (!ModManager.Expedition || !creature.world.game.rainWorld.ExpeditionMode || !ExpeditionGame.activeUnlocks.Contains("bur-hunted") || !(creature.world.rainCycle.CycleProgression > 0.05f) || tracker == null || creature.world.game.Players == null)
		{
			return;
		}
		for (int j = 0; j < creature.world.game.Players.Count; j++)
		{
			if (creature.world.game.Players[j].realizedCreature != null && !(creature.world.game.Players[j].realizedCreature as Player).dead)
			{
				if (creature.Room != creature.world.game.Players[j].Room)
				{
					tracker.SeeCreature(creature.world.game.Players[j]);
				}
				break;
			}
		}
	}

	public virtual void NewRoom(Room room)
	{
		if (lastRoom != room.abstractRoom.index)
		{
			lastRoom = room.abstractRoom.index;
			timeInRoom = 0;
			for (int i = 0; i < modules.Count; i++)
			{
				modules[i].NewRoom(room);
			}
			if (creature.abstractAI.destination.room == room.abstractRoom.index && !creature.abstractAI.destination.TileDefined)
			{
				SetDestination(new WorldCoordinate(room.abstractRoom.index, UnityEngine.Random.Range(0, room.TileWidth), UnityEngine.Random.Range(0, room.TileHeight), -1));
			}
		}
	}

	public virtual PathCost TravelPreference(MovementConnection coord, PathCost cost)
	{
		return cost;
	}

	public virtual void NewArea(bool strandedFromExits)
	{
		stranded = strandedFromExits;
		if (denFinder != null && (pathFinder == null || !DenPosition().HasValue || !pathFinder.CoordinateReachable(DenPosition().Value)))
		{
			denFinder.ResetMapping(strandedFromExits);
		}
		if (secondaryNodeFinder != null)
		{
			secondaryNodeFinder.ResetMappingIfNecessary(strandedFromExits);
		}
	}

	public WorldCoordinate? DenPosition()
	{
		if (denFinder != null)
		{
			return denFinder.GetDenPosition();
		}
		return creature.abstractAI.denPosition;
	}

	public virtual bool WantToStayInDenUntilEndOfCycle()
	{
		return false;
	}

	public virtual Tracker.CreatureRepresentation CreateTrackerRepresentationForCreature(AbstractCreature otherCreature)
	{
		return new Tracker.SimpleCreatureRepresentation(tracker, otherCreature, 0f, forgetWhenNotVisible: true);
	}

	public CreatureTemplate.Relationship DynamicRelationship(AbstractCreature absCrit)
	{
		return DynamicRelationship(null, absCrit);
	}

	public CreatureTemplate.Relationship DynamicRelationship(Tracker.CreatureRepresentation rep)
	{
		return DynamicRelationship(rep, null);
	}

	public CreatureTemplate.Relationship DynamicRelationship(Tracker.CreatureRepresentation rep, AbstractCreature absCrit)
	{
		if (rep == null)
		{
			rep = tracker.RepresentationForCreature(absCrit, addIfMissing: false);
		}
		if (rep == null)
		{
			return StaticRelationship(absCrit);
		}
		if (rep.dynamicRelationship != null)
		{
			return rep.dynamicRelationship.currentRelationship;
		}
		return StaticRelationship(rep.representedCreature);
	}

	public CreatureTemplate.Relationship StaticRelationship(AbstractCreature otherCreature)
	{
		return creature.creatureTemplate.CreatureRelationship(otherCreature.creatureTemplate);
	}

	public virtual bool TrackerToDiscardDeadCreature(AbstractCreature crit)
	{
		if (obstacleTracker != null && obstacleTracker.KnownObstacleObject(crit.realizedCreature))
		{
			return false;
		}
		return DynamicRelationship(crit).type != CreatureTemplate.Relationship.Type.Eats;
	}

	public virtual void HeardNoise(InGameNoise noise)
	{
		if (noiseTracker != null)
		{
			noiseTracker.HeardNoise(noise);
		}
	}

	public virtual float CurrentPlayerAggression(AbstractCreature player)
	{
		return 1f;
	}
}
