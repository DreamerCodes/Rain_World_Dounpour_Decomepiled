using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class CentipedeAI : ArtificialIntelligence, IUseARelationshipTracker
{
	public class Behavior : ExtEnum<Behavior>
	{
		public static readonly Behavior Idle = new Behavior("Idle", register: true);

		public static readonly Behavior Flee = new Behavior("Flee", register: true);

		public static readonly Behavior Hunt = new Behavior("Hunt", register: true);

		public static readonly Behavior EscapeRain = new Behavior("EscapeRain", register: true);

		public static readonly Behavior Injured = new Behavior("Injured", register: true);

		public static readonly Behavior InvestigateSound = new Behavior("InvestigateSound", register: true);

		public Behavior(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class CentipedeTrackState : RelationshipTracker.TrackedCreatureState
	{
		public int annoyingCollisions;
	}

	public Centipede centipede;

	private DebugDestinationVisualizer debugDestinationVisualizer;

	public int annoyingCollisions;

	public float currentUtility;

	public Behavior behavior;

	public float excitement;

	public float run;

	public WorldCoordinate forbiddenIdlePos;

	public WorldCoordinate tempIdlePos;

	public int idleCounter;

	private List<PlacedObject> centipedeAttractors;

	public CentipedeAI(AbstractCreature creature, World world)
		: base(creature, world)
	{
		centipede = creature.realizedCreature as Centipede;
		centipede.AI = this;
		AddModule(new CentipedePather(this, world, creature));
		base.pathFinder.accessibilityStepsPerFrame = 40;
		if (centipede.Red)
		{
			base.pathFinder.stepsPerFrame = 15;
		}
		AddModule(new Tracker(this, 10, 10, -1, 0.5f, 5, 5, 20));
		if (!centipede.Centiwing)
		{
			AddModule(new NoiseTracker(this, base.tracker));
		}
		AddModule(new PreyTracker(this, 5, 1f, centipede.Red ? 100f : 5f, 150f, 0.05f));
		AddModule(new ThreatTracker(this, 3));
		AddModule(new RainTracker(this));
		AddModule(new DenFinder(this, creature));
		AddModule(new RelationshipTracker(this, base.tracker));
		AddModule(new UtilityComparer(this));
		AddModule(new InjuryTracker(this, 0.6f));
		base.utilityComparer.AddComparedModule(base.threatTracker, null, 1f, 1.1f);
		base.utilityComparer.AddComparedModule(base.preyTracker, null, centipede.Centiwing ? 0.12f : 0.9f, 1.1f);
		base.utilityComparer.AddComparedModule(base.rainTracker, null, 1f, 1.1f);
		base.utilityComparer.AddComparedModule(base.injuryTracker, null, 0.7f, 1.1f);
		if (base.noiseTracker != null)
		{
			base.utilityComparer.AddComparedModule(base.noiseTracker, null, 0.2f, 1.2f);
		}
		behavior = Behavior.Idle;
	}

	public override void NewRoom(Room room)
	{
		base.NewRoom(room);
		forbiddenIdlePos = creature.pos;
		tempIdlePos = creature.pos;
		centipedeAttractors = new List<PlacedObject>();
		for (int i = 0; i < room.roomSettings.placedObjects.Count; i++)
		{
			if (room.roomSettings.placedObjects[i].type == PlacedObject.Type.CentipedeAttractor)
			{
				centipedeAttractors.Add(room.roomSettings.placedObjects[i]);
			}
		}
	}

	public override void Update()
	{
		base.Update();
		if (ModManager.MSC && centipede.LickedByPlayer != null)
		{
			base.tracker.SeeCreature(centipede.LickedByPlayer.abstractCreature);
			AnnoyingCollision(centipede.LickedByPlayer.abstractCreature);
		}
		if (annoyingCollisions > 0)
		{
			annoyingCollisions--;
		}
		if (base.noiseTracker != null)
		{
			base.noiseTracker.hearingSkill = (centipede.moving ? 0f : 1.5f);
		}
		if (base.preyTracker.MostAttractivePrey != null && !centipede.Red)
		{
			base.utilityComparer.GetUtilityTracker(base.preyTracker).weight = Mathf.InverseLerp(50f, 10f, base.preyTracker.MostAttractivePrey.TicksSinceSeen);
		}
		if (base.threatTracker.mostThreateningCreature != null)
		{
			base.utilityComparer.GetUtilityTracker(base.threatTracker).weight = Mathf.InverseLerp(500f, 100f, base.threatTracker.mostThreateningCreature.TicksSinceSeen);
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
			else if (aIModule is NoiseTracker)
			{
				behavior = Behavior.InvestigateSound;
			}
			else if (aIModule is InjuryTracker)
			{
				behavior = Behavior.Injured;
			}
		}
		if (currentUtility < 0.1f)
		{
			behavior = Behavior.Idle;
		}
		float b = 0f;
		if (behavior == Behavior.Idle)
		{
			WorldCoordinate testPos = creature.pos + new IntVector2(Random.Range(-10, 11), Random.Range(-10, 11));
			if (Random.value < 1f / (centipede.Centiwing ? 3f : 100f))
			{
				testPos = new WorldCoordinate(creature.pos.room, Random.Range(0, centipede.room.TileWidth), Random.Range(0, centipede.room.TileHeight), -1);
			}
			else if (centipedeAttractors.Count > 0 && Random.value < 0.025f)
			{
				PlacedObject placedObject = centipedeAttractors[Random.Range(0, centipedeAttractors.Count)];
				testPos = centipede.room.GetWorldCoordinate(placedObject.pos + Custom.RNV() * (placedObject.data as PlacedObject.ResizableObjectData).Rad * Random.value);
			}
			if (IdleScore(testPos) > IdleScore(tempIdlePos))
			{
				tempIdlePos = testPos;
				idleCounter = 0;
			}
			else
			{
				idleCounter++;
				if ((centipede.Centiwing || centipede.AquaCenti) && creature.pos.room == tempIdlePos.room && creature.pos.Tile.FloatDist(tempIdlePos.Tile) < 15f)
				{
					idleCounter += 2;
				}
				if (idleCounter > ((centipede.Centiwing && centipede.room.aimap.getAItile(tempIdlePos.Tile).acc > AItile.Accessibility.Climb) ? 400 : 1400) || (centipede.flying && !centipede.RatherClimbThanFly(tempIdlePos.Tile) && creature.pos.room == tempIdlePos.room && creature.pos.Tile.FloatDist(tempIdlePos.Tile) < 5f) || centipede.outsideLevel)
				{
					idleCounter = 0;
					forbiddenIdlePos = tempIdlePos;
				}
			}
			if (tempIdlePos != base.pathFinder.GetDestination && IdleScore(tempIdlePos) > IdleScore(base.pathFinder.GetDestination) + 100f)
			{
				creature.abstractAI.SetDestination(tempIdlePos);
			}
		}
		else if (behavior == Behavior.Flee)
		{
			b = 1f;
			WorldCoordinate destination = base.threatTracker.FleeTo(creature.pos, 1, 30, currentUtility > 0.3f);
			creature.abstractAI.SetDestination(destination);
		}
		else if (behavior == Behavior.EscapeRain)
		{
			b = 0.5f;
			if (base.denFinder.GetDenPosition().HasValue)
			{
				creature.abstractAI.SetDestination(base.denFinder.GetDenPosition().Value);
			}
		}
		else if (behavior == Behavior.Injured)
		{
			b = 1f;
			if (base.denFinder.GetDenPosition().HasValue)
			{
				creature.abstractAI.SetDestination(base.denFinder.GetDenPosition().Value);
			}
		}
		else if (behavior == Behavior.Hunt)
		{
			b = DynamicRelationship(base.preyTracker.MostAttractivePrey).intensity;
			creature.abstractAI.SetDestination(base.preyTracker.MostAttractivePrey.BestGuessForPosition());
		}
		else if (behavior == Behavior.InvestigateSound)
		{
			b = 0.2f;
			creature.abstractAI.SetDestination(base.noiseTracker.ExaminePos);
		}
		excitement = Mathf.Lerp(excitement, b, 0.1f);
		if (centipede.Centiwing || (centipede.Red && behavior == Behavior.Hunt))
		{
			run = 500f;
			return;
		}
		run -= 1f;
		if (run < Mathf.Lerp(-50f, -5f, excitement))
		{
			run = Mathf.Lerp(30f, 50f, excitement);
		}
		int num = 0;
		float num2 = 0f;
		for (int i = 0; i < base.tracker.CreaturesCount; i++)
		{
			if (base.tracker.GetRep(i).representedCreature.creatureTemplate.type == CreatureTemplate.Type.Centipede && base.tracker.GetRep(i).representedCreature.realizedCreature != null && base.tracker.GetRep(i).representedCreature.Room == creature.Room && (base.tracker.GetRep(i).representedCreature.realizedCreature as Centipede).AI.run > 0f == run > 0f)
			{
				num2 += (base.tracker.GetRep(i).representedCreature.realizedCreature as Centipede).AI.run;
				num++;
			}
		}
		if (num > 0)
		{
			run = Mathf.Lerp(run, num2 / (float)num, 0.1f);
		}
	}

	public void CheckRandomIdlePos()
	{
		WorldCoordinate testPos = new WorldCoordinate(creature.pos.room, Random.Range(0, centipede.room.TileWidth), Random.Range(0, centipede.room.TileHeight), -1);
		if (IdleScore(testPos) > IdleScore(tempIdlePos))
		{
			tempIdlePos = testPos;
			idleCounter = 0;
		}
	}

	public float IdleScore(WorldCoordinate testPos)
	{
		if (!testPos.TileDefined)
		{
			return float.MinValue;
		}
		if (testPos.room != creature.pos.room)
		{
			return float.MinValue;
		}
		if (!base.pathFinder.CoordinateReachableAndGetbackable(testPos))
		{
			return float.MinValue;
		}
		if (this.centipede.AquaCenti && !this.centipede.room.PointSubmerged(testPos.Tile.ToVector2()))
		{
			return float.MinValue;
		}
		float num = 1000f;
		if (!this.centipede.Centiwing)
		{
			num /= Mathf.Max(1f, (float)this.centipede.room.aimap.getTerrainProximity(testPos) - 1f);
		}
		num -= Custom.LerpMap(testPos.Tile.FloatDist(forbiddenIdlePos.Tile), 0f, 10f, 1000f, 0f);
		for (int i = 0; i < base.tracker.CreaturesCount; i++)
		{
			if (base.tracker.GetRep(i).representedCreature.creatureTemplate.type == creature.creatureTemplate.type && base.tracker.GetRep(i).representedCreature.realizedCreature != null && (base.tracker.GetRep(i).representedCreature.realizedCreature as Centipede).size > this.centipede.size && base.tracker.GetRep(i).BestGuessForPosition().room == creature.pos.room && (base.tracker.GetRep(i).representedCreature.realizedCreature as Centipede).AI.behavior == Behavior.Idle)
			{
				Centipede centipede = base.tracker.GetRep(i).representedCreature.realizedCreature as Centipede;
				num -= Custom.LerpMap(testPos.Tile.FloatDist(centipede.AI.tempIdlePos.Tile), 0f, 20f, 1000f, 0f) * Mathf.InverseLerp(this.centipede.size, 1f, centipede.size);
				num -= Custom.LerpMap(testPos.Tile.FloatDist(centipede.AI.pathFinder.GetDestination.Tile), 0f, 20f, 1000f, 0f) * Mathf.InverseLerp(this.centipede.size, 1f, centipede.size);
			}
		}
		if (this.centipede.room.aimap.getAItile(testPos).fallRiskTile.y < 0)
		{
			num -= Custom.LerpMap(testPos.y, 10f, 30f, 1000f, 0f);
		}
		for (int j = 0; j < centipedeAttractors.Count; j++)
		{
			if (Custom.DistLess(this.centipede.room.MiddleOfTile(testPos), centipedeAttractors[j].pos, (centipedeAttractors[j].data as PlacedObject.ResizableObjectData).Rad))
			{
				num += 1000f;
				break;
			}
		}
		return num;
	}

	public void AnnoyingCollision(AbstractCreature critter)
	{
		if (!critter.state.dead && !centipede.Small)
		{
			annoyingCollisions += 10;
			if (annoyingCollisions >= 150 && base.tracker.RepresentationForCreature(critter, addIfMissing: false) != null)
			{
				(base.tracker.RepresentationForCreature(critter, addIfMissing: false).dynamicRelationship.state as CentipedeTrackState).annoyingCollisions++;
			}
		}
	}

	public bool DoIWantToShockCreature(AbstractCreature critter)
	{
		if (centipede.safariControlled)
		{
			return true;
		}
		if (annoyingCollisions < 150 && (behavior == Behavior.Flee || behavior == Behavior.EscapeRain) && currentUtility > 0.1f)
		{
			return false;
		}
		if (critter.state.dead)
		{
			return false;
		}
		if (critter.realizedCreature != null)
		{
			Tracker.CreatureRepresentation creatureRepresentation = base.tracker.RepresentationForObject(critter.realizedCreature, AddIfMissing: false);
			if (!centipede.Small && annoyingCollisions > 150 && (creatureRepresentation == null || (creatureRepresentation.dynamicRelationship.state as CentipedeTrackState).annoyingCollisions > (int)(Mathf.Lerp(210f, 30f, centipede.size) * centipede.CentiState.health)))
			{
				return true;
			}
			if (creatureRepresentation != null)
			{
				return creatureRepresentation.dynamicRelationship.currentRelationship.type == CreatureTemplate.Relationship.Type.Eats;
			}
		}
		return StaticRelationship(critter).type == CreatureTemplate.Relationship.Type.Eats;
	}

	public override bool TrackerToDiscardDeadCreature(AbstractCreature crit)
	{
		return true;
	}

	public override bool WantToStayInDenUntilEndOfCycle()
	{
		return base.rainTracker.Utility() > 0.01f;
	}

	public override float VisualScore(Vector2 lookAtPoint, float bonus)
	{
		Vector2 pos;
		Vector2 pos2;
		if (centipede.visionDirection)
		{
			pos = centipede.bodyChunks[1].pos;
			pos2 = centipede.bodyChunks[0].pos;
		}
		else
		{
			pos = centipede.bodyChunks[centipede.bodyChunks.Length - 2].pos;
			pos2 = centipede.bodyChunks[centipede.bodyChunks.Length - 1].pos;
		}
		return base.VisualScore(lookAtPoint, bonus) - Mathf.InverseLerp(1f, (centipede.Centiwing || centipede.Red) ? 0.3f : 0.7f, Vector2.Dot((pos - pos2).normalized, (pos2 - lookAtPoint).normalized)) - ((centipede.moving && !centipede.Centiwing && !centipede.Red) ? 0.75f : 0f);
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

	public override PathCost TravelPreference(MovementConnection coord, PathCost cost)
	{
		if (centipede.Centiwing && coord.destinationCoord.TileDefined)
		{
			if (!centipede.flying && !centipede.RatherClimbThanFly(coord.DestTile))
			{
				return new PathCost(cost.resistance + 1000f, cost.legality);
			}
			if (centipede.flying)
			{
				return new PathCost(cost.resistance + ((centipede.room.aimap.getTerrainProximity(coord.destinationCoord) < 2) ? 0f : Custom.LerpMap(centipede.room.aimap.getTerrainProximity(coord.destinationCoord), 1f, 6f, 500f, 0f)), cost.legality);
			}
		}
		return base.TravelPreference(coord, cost);
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
		if (centipede.Small)
		{
			return new RelationshipTracker.TrackedCreatureState();
		}
		return new CentipedeTrackState();
	}

	CreatureTemplate.Relationship IUseARelationshipTracker.UpdateDynamicRelationship(RelationshipTracker.DynamicRelationship dRelation)
	{
		CreatureTemplate.Relationship result = StaticRelationship(dRelation.trackerRep.representedCreature);
		if (result.type == CreatureTemplate.Relationship.Type.Ignores)
		{
			return result;
		}
		if (dRelation.trackerRep.representedCreature.realizedCreature != null)
		{
			if (dRelation.trackerRep.representedCreature.realizedCreature.dead)
			{
				return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f);
			}
			if (result.type == CreatureTemplate.Relationship.Type.Eats && dRelation.trackerRep.representedCreature.realizedCreature.TotalMass < centipede.TotalMass)
			{
				if (centipede.Centiwing)
				{
					float num = Mathf.Pow(Mathf.InverseLerp(0f, centipede.TotalMass, dRelation.trackerRep.representedCreature.realizedCreature.TotalMass), 0.75f);
					if (dRelation.trackerRep.age < 300)
					{
						num *= 1f - OverChasm(dRelation.trackerRep.BestGuessForPosition().Tile);
						return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, num * Mathf.InverseLerp(300f, 0f, dRelation.trackerRep.age));
					}
					num *= 1f - OverChasm(dRelation.trackerRep.BestGuessForPosition().Tile);
					return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, num * Mathf.InverseLerp(300f, 800f, dRelation.trackerRep.age));
				}
				if (centipede.Red)
				{
					return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, Mathf.Pow(Mathf.InverseLerp(0f, centipede.TotalMass, dRelation.trackerRep.representedCreature.realizedCreature.TotalMass), Custom.LerpMap(dRelation.trackerRep.representedCreature.realizedCreature.TotalMass, 0.2f, 0.7f, 3f, 0.1f)) * result.intensity);
				}
				return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, Mathf.InverseLerp(0f, centipede.TotalMass, dRelation.trackerRep.representedCreature.realizedCreature.TotalMass) * result.intensity);
			}
			return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.2f + 0.8f * Mathf.InverseLerp(centipede.TotalMass, centipede.TotalMass * 1.5f, dRelation.trackerRep.representedCreature.realizedCreature.TotalMass));
		}
		return result;
	}

	public float OverChasm(IntVector2 testPos)
	{
		float num = ((centipede.room.aimap.getAItile(testPos).fallRiskTile.y < 0) ? 1f : 0f);
		for (int i = -1; i < 2; i += 2)
		{
			for (int j = 1; j < 7 && !centipede.room.GetTile(testPos + new IntVector2(j * i, 0)).Solid; j++)
			{
				if (centipede.room.aimap.getAItile(testPos + new IntVector2(j * i, 0)).fallRiskTile.y < 0)
				{
					num += 1f / (float)j;
				}
			}
		}
		return Mathf.InverseLerp(0f, 5.9f, num);
	}
}
