using Noise;
using RWCustom;
using UnityEngine;

public class DaddyAI : ArtificialIntelligence, IUseARelationshipTracker, IAINoiseReaction
{
	public class Behavior : ExtEnum<Behavior>
	{
		public static readonly Behavior Idle = new Behavior("Idle", register: true);

		public static readonly Behavior Hunt = new Behavior("Hunt", register: true);

		public static readonly Behavior EscapeRain = new Behavior("EscapeRain", register: true);

		public static readonly Behavior ExamineSound = new Behavior("ExamineSound", register: true);

		public Behavior(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public DebugDestinationVisualizer debugDestinationVisualizer;

	public WorldCoordinate reactTarget;

	public int reactNoiseTime;

	public Behavior behavior;

	public int newIdlePosCounter;

	public DaddyLongLegs daddy => creature.realizedCreature as DaddyLongLegs;

	public DaddyAI(AbstractCreature creature, World world)
		: base(creature, world)
	{
		daddy.AI = this;
		AddModule(new StandardPather(this, world, creature));
		(base.pathFinder as StandardPather).heuristicCostFac = 1f;
		(base.pathFinder as StandardPather).heuristicDestFac = 1f;
		if (daddy.HDmode)
		{
			AddModule(new Tracker(this, 20, 10, -1, 0.25f, 50, 1, 1));
			base.pathFinder.stepsPerFrame = 100;
		}
		else
		{
			AddModule(new Tracker(this, 10, 10, -1, 0.25f, 50, 1, 1));
		}
		if (daddy.SizeClass)
		{
			AddModule(new NoiseTracker(this, base.tracker));
			base.noiseTracker.forgetTime = 320;
			base.noiseTracker.ignoreSeenNoises = false;
		}
		else if (daddy.HDmode)
		{
			AddModule(new NoiseTracker(this, base.tracker));
			base.noiseTracker.forgetTime = 640;
			base.noiseTracker.ignoreSeenNoises = false;
		}
		AddModule(new RainTracker(this));
		AddModule(new DenFinder(this, creature));
		if (daddy.HDmode)
		{
			AddModule(new PreyTracker(this, 5, 6f, 60f, 600f, 0.75f));
			base.preyTracker.giveUpOnUnreachablePrey = -1;
		}
		else if (ModManager.MSC && creature.superSizeMe)
		{
			AddModule(new PreyTracker(this, 7, 6f, 60f, 800f, 0.75f));
		}
		else
		{
			AddModule(new PreyTracker(this, 5, 1.1f, 60f, 800f, 0.75f));
			base.preyTracker.giveUpOnUnreachablePrey = -1;
		}
		AddModule(new RelationshipTracker(this, base.tracker));
		AddModule(new UtilityComparer(this));
		base.utilityComparer.AddComparedModule(base.preyTracker, null, 1f, 1f);
		base.utilityComparer.AddComparedModule(base.noiseTracker, null, 0.1f, 1f);
		base.utilityComparer.AddComparedModule(base.rainTracker, null, 1f, 1f);
	}

	public override void NewRoom(Room room)
	{
		base.NewRoom(room);
	}

	public override void Update()
	{
		if (base.noiseTracker != null)
		{
			if (daddy.HDmode)
			{
				base.noiseTracker.hearingSkill = 5f;
			}
			else
			{
				base.noiseTracker.hearingSkill = ((daddy.eyesClosed < 1) ? 2f : 0f);
			}
		}
		if (ModManager.MSC && daddy.LickedByPlayer != null)
		{
			base.tracker.SeeCreature(daddy.LickedByPlayer.abstractCreature);
		}
		behavior = Behavior.Idle;
		reactNoiseTime--;
		AIModule aIModule = base.utilityComparer.HighestUtilityModule();
		if (base.utilityComparer.HighestUtility() > 0.01f && aIModule != null)
		{
			if (aIModule is PreyTracker)
			{
				behavior = Behavior.Hunt;
			}
			if (aIModule is NoiseTracker)
			{
				behavior = Behavior.ExamineSound;
			}
			if (aIModule is RainTracker)
			{
				behavior = Behavior.EscapeRain;
			}
		}
		if (daddy.HDmode && behavior == Behavior.Idle && (reactNoiseTime > 0 || (base.noiseTracker != null && base.noiseTracker.Utility() > 0f)))
		{
			behavior = Behavior.ExamineSound;
		}
		if (daddy.safariControlled && daddy.inputWithDiagonals.HasValue && daddy.inputWithDiagonals.Value.pckp)
		{
			behavior = Behavior.Hunt;
		}
		if (daddy.safariControlled && behavior == Behavior.Hunt && (!daddy.inputWithDiagonals.HasValue || !daddy.inputWithDiagonals.Value.pckp))
		{
			behavior = Behavior.Idle;
		}
		if (behavior == Behavior.Idle)
		{
			newIdlePosCounter--;
			if (newIdlePosCounter < 1 || !base.pathFinder.CoordinateReachableAndGetbackable(base.pathFinder.GetDestination))
			{
				WorldCoordinate worldCoordinate = new WorldCoordinate(creature.pos.room, Random.Range(0, daddy.room.TileWidth), Random.Range(0, daddy.room.TileHeight), -1);
				if (base.pathFinder.CoordinateReachableAndGetbackable(worldCoordinate))
				{
					creature.abstractAI.SetDestination(worldCoordinate);
					if (daddy.HDmode)
					{
						newIdlePosCounter = Random.Range(30, 500);
					}
					else
					{
						newIdlePosCounter = Random.Range(300, 2000);
					}
				}
			}
			else if (base.pathFinder.GetDestination.room == creature.pos.room && base.pathFinder.GetDestination.TileDefined && daddy.room.aimap.getTerrainProximity(base.pathFinder.GetDestination) < 6)
			{
				WorldCoordinate worldCoordinate2 = base.pathFinder.GetDestination + Custom.fourDirections[Random.Range(0, 4)];
				if (base.pathFinder.CoordinateReachableAndGetbackable(worldCoordinate2) && daddy.room.aimap.getTerrainProximity(worldCoordinate2) > daddy.room.aimap.getTerrainProximity(base.pathFinder.GetDestination))
				{
					creature.abstractAI.SetDestination(worldCoordinate2);
				}
			}
			if (!daddy.SizeClass && base.pathFinder.GetDestination.room == creature.pos.room && base.pathFinder.GetDestination.TileDefined && base.pathFinder.GetDestination.Tile.FloatDist(creature.pos.Tile) < 7f && daddy.room.VisualContact(creature.pos.Tile, base.pathFinder.GetDestination.Tile))
			{
				WorldCoordinate worldCoordinate3 = base.pathFinder.GetDestination + new IntVector2(Random.Range(-20, 21), Random.Range(-20, 21));
				if (base.pathFinder.CoordinateReachableAndGetbackable(worldCoordinate3) && worldCoordinate3.Tile.FloatDist(creature.pos.Tile) >= 7f)
				{
					creature.abstractAI.SetDestination(worldCoordinate3);
				}
			}
		}
		else if (behavior == Behavior.ExamineSound)
		{
			if (ModManager.MSC && reactNoiseTime > 0)
			{
				creature.abstractAI.SetDestination(reactTarget);
			}
			else
			{
				creature.abstractAI.SetDestination(base.noiseTracker.ExaminePos);
			}
		}
		else if (behavior == Behavior.Hunt)
		{
			if (base.preyTracker.MostAttractivePrey != null)
			{
				WorldCoordinate worldCoordinate4 = base.preyTracker.MostAttractivePrey.BestGuessForPosition();
				if (daddy.HDmode)
				{
					worldCoordinate4 = base.preyTracker.MostAttractivePrey.representedCreature.pos;
				}
				if (!worldCoordinate4.TileDefined || worldCoordinate4.room != creature.pos.room)
				{
					creature.abstractAI.SetDestination(worldCoordinate4);
				}
				else
				{
					bool flag = false;
					for (int i = 0; i < 5; i++)
					{
						if (flag)
						{
							break;
						}
						if (base.pathFinder.CoordinateReachableAndGetbackable(worldCoordinate4 + Custom.fourDirectionsAndZero[i]))
						{
							creature.abstractAI.SetDestination(worldCoordinate4 + Custom.fourDirectionsAndZero[i]);
							flag = true;
						}
					}
					for (int j = 0; j < daddy.tentacles.Length; j++)
					{
						if (flag)
						{
							break;
						}
						if (daddy.tentacles[j].huntCreature != base.preyTracker.MostAttractivePrey || !daddy.room.aimap.TileAccessibleToCreature(daddy.tentacles[j].grabPath[0], creature.creatureTemplate))
						{
							continue;
						}
						for (int k = 0; k < daddy.tentacles[j].grabPath.Count - 1; k++)
						{
							if (flag)
							{
								break;
							}
							bool flag2 = false;
							for (int l = 0; l < 5; l++)
							{
								if (flag2)
								{
									break;
								}
								if (base.pathFinder.CoordinateReachableAndGetbackable(daddy.room.GetWorldCoordinate(daddy.tentacles[j].grabPath[k + 1] + Custom.fourDirectionsAndZero[l])))
								{
									flag2 = true;
								}
							}
							if (!flag2)
							{
								creature.abstractAI.SetDestination(daddy.room.GetWorldCoordinate(daddy.tentacles[j].grabPath[k]));
								flag = true;
							}
						}
					}
					if (!flag)
					{
						creature.abstractAI.SetDestination(base.preyTracker.MostAttractivePrey.BestGuessForPosition());
					}
				}
			}
		}
		else if (behavior == Behavior.EscapeRain && base.denFinder.GetDenPosition().HasValue)
		{
			creature.abstractAI.SetDestination(base.denFinder.GetDenPosition().Value);
		}
		if (base.tracker.CreaturesCount > 0)
		{
			Tracker.CreatureRepresentation rep = base.tracker.GetRep(Random.Range(0, base.tracker.CreaturesCount));
			if (rep.LowestGenerationAvailable > 100)
			{
				rep.Destroy();
			}
		}
		base.Update();
	}

	public override void CreatureSpotted(bool firstSpot, Tracker.CreatureRepresentation otherCreature)
	{
		base.CreatureSpotted(firstSpot, otherCreature);
	}

	public override float VisualScore(Vector2 lookAtPoint, float targetSpeed)
	{
		return base.VisualScore(lookAtPoint, targetSpeed);
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
		if (ModManager.MMF && !daddy.CheckDaddyConsumption(dRelation.trackerRep.representedCreature.realizedCreature))
		{
			return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f);
		}
		if (!ModManager.MMF && daddy.SizeClass && !daddy.colorClass && dRelation.trackerRep.representedCreature.creatureTemplate.type == CreatureTemplate.Type.BrotherLongLegs)
		{
			return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f);
		}
		return StaticRelationship(dRelation.trackerRep.representedCreature);
	}

	public override PathCost TravelPreference(MovementConnection connection, PathCost cost)
	{
		if (daddy.stuckPos != null)
		{
			if (connection.destinationCoord.room != daddy.room.abstractRoom.index)
			{
				return new PathCost(0f, PathCost.Legality.Unallowed);
			}
			if (!Custom.DistLess(daddy.room.MiddleOfTile(connection.destinationCoord), daddy.stuckPos.pos, (daddy.stuckPos.data as PlacedObject.ResizableObjectData).Rad + 20f))
			{
				return new PathCost(0f, PathCost.Legality.Unallowed);
			}
		}
		return cost;
	}

	public override bool WantToStayInDenUntilEndOfCycle()
	{
		if (!(behavior == Behavior.EscapeRain))
		{
			return creature.world.rainCycle.TimeUntilRain < 40;
		}
		return true;
	}

	public void ReactToNoise(NoiseTracker.TheorizedSource source, InGameNoise noise)
	{
		if (daddy.graphicsModule != null)
		{
			(daddy.graphicsModule as DaddyGraphics).ReactToNoise(source, noise);
		}
		if (daddy.HDmode)
		{
			reactTarget = Custom.MakeWorldCoordinate(new IntVector2((int)(noise.pos.x / 20f), (int)(noise.pos.y / 20f)), daddy.room.abstractRoom.index);
			reactNoiseTime = 60;
			newIdlePosCounter = 300;
			creature.abstractAI.SetDestination(reactTarget);
			base.pathFinder.ForceNextDestination();
		}
		else
		{
			daddy.Deafen((int)Custom.LerpMap(noise.strength, 2000f, 8000f, 10f, 200f));
		}
	}
}
