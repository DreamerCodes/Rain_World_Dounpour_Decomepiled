using RWCustom;
using UnityEngine;

public class SmallNeedleWormAI : NeedleWormAI, IUseARelationshipTracker
{
	public new SmallNeedleWorm worm;

	public AbstractCreature Mother => (creature.abstractAI as NeedleWormAbstractAI).mother;

	public AbstractCreature NeedleWormMother
	{
		get
		{
			if ((creature.abstractAI as NeedleWormAbstractAI).mother != null && (creature.abstractAI as NeedleWormAbstractAI).mother.creatureTemplate.type == CreatureTemplate.Type.BigNeedleWorm)
			{
				return (creature.abstractAI as NeedleWormAbstractAI).mother;
			}
			return null;
		}
	}

	public SmallNeedleWormAI(AbstractCreature creature, World world)
		: base(creature, world)
	{
		worm = creature.realizedCreature as SmallNeedleWorm;
		worm.AI = this;
	}

	public override void Update()
	{
		base.Update();
		if (worm.room == null)
		{
			return;
		}
		AIModule aIModule = base.utilityComparer.HighestUtilityModule();
		float num = base.utilityComparer.HighestUtility();
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
			else if (aIModule is StuckTracker && !worm.OffscreenSuperSpeed)
			{
				behavior = Behavior.GetUnstuck;
			}
		}
		if (num < 0.1f)
		{
			behavior = Behavior.Idle;
		}
		if (creature.abstractAI.followCreature != null)
		{
			if (creature.abstractAI.followCreature.pos.room == creature.pos.room)
			{
				base.tracker.SeeCreature(creature.abstractAI.followCreature);
			}
			else if (num < 0.8f && creature.abstractAI.followCreature.pos.room != creature.pos.room)
			{
				behavior = Behavior.Migrate;
				num = 0.8f;
			}
		}
		if (behavior == Behavior.Migrate)
		{
			if (creature.abstractAI.followCreature != null)
			{
				creature.abstractAI.SetDestination(creature.abstractAI.followCreature.pos.WashTileData());
			}
		}
		else if (behavior == Behavior.Idle)
		{
			if (creature.abstractAI.followCreature != null && creature.abstractAI.followCreature.pos.room != creature.pos.room)
			{
				creature.abstractAI.SetDestination(creature.abstractAI.followCreature.pos.WashTileData());
			}
			else if (creature.abstractAI.WantToMigrate)
			{
				creature.abstractAI.SetDestination(creature.abstractAI.MigrationDestination);
			}
			else
			{
				IdleBehavior();
			}
			if (Mother != null && (!ModManager.MSC || !creature.controlled))
			{
				if (Mother.pos.room != creature.pos.room || Mother.pos.Tile.FloatDist(creature.pos.Tile) > 5f)
				{
					flySpeed = 1f;
				}
				if (Random.value < 0.003125f && worm.screaming == 0f && base.tracker.RepresentationForCreature(Mother, addIfMissing: false) != null && base.tracker.RepresentationForCreature(Mother, addIfMissing: false).TicksSinceSeen > Random.Range(80, 300))
				{
					worm.SmallScream(motherRespond: true);
				}
			}
		}
		else if (behavior == Behavior.Flee)
		{
			creature.abstractAI.SetDestination(base.threatTracker.FleeTo(creature.pos, 10, 20, considerLeavingRoom: true));
			if (base.threatTracker.mostThreateningCreature != null)
			{
				focusCreature = base.threatTracker.mostThreateningCreature;
			}
		}
		else if (behavior == Behavior.EscapeRain)
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

	protected override float IdleScore(WorldCoordinate coord)
	{
		if (coord.room != worm.room.abstractRoom.index)
		{
			if (Mother != null && (Mother.pos.room == coord.room || (Mother.abstractAI != null && Mother.abstractAI.MigrationDestination.room == coord.room)))
			{
				return 0f;
			}
			return float.MaxValue;
		}
		if (!base.pathFinder.CoordinateReachableAndGetbackable(coord))
		{
			return float.MaxValue;
		}
		if (coord.Tile.x < 0 || coord.Tile.x >= worm.room.TileWidth || coord.Tile.y < 0 || coord.Tile.y >= worm.room.TileHeight)
		{
			return float.MaxValue;
		}
		float num = 0f;
		if (Mother != null && Mother.realizedCreature != null && Mother.pos.room == worm.room.abstractRoom.index)
		{
			if (NeedleWormMother != null && Mother.realizedCreature.Consious && (Mother.abstractAI.RealAI as BigNeedleWormAI).pathFinder.GetDestination.room == worm.room.abstractRoom.index && (Mother.abstractAI.RealAI as BigNeedleWormAI).pathFinder.creatureFollowingGeneration == (Mother.abstractAI.RealAI as BigNeedleWormAI).pathFinder.pathGeneration)
			{
				num += coord.Tile.FloatDist((Mother.abstractAI.RealAI as BigNeedleWormAI).pathFinder.GetDestination.Tile);
			}
			num += coord.Tile.FloatDist(Mother.pos.Tile) * 1.1f;
		}
		else
		{
			for (int i = 0; i < oldIdlePositions.Count; i++)
			{
				if (oldIdlePositions[i].room == coord.room)
				{
					num -= Mathf.Pow(Mathf.Min(30f, oldIdlePositions[i].Tile.FloatDist(coord.Tile)), 2f) / (30f * (1f + (float)i * 0.2f));
				}
			}
		}
		for (int j = 0; j < base.tracker.CreaturesCount; j++)
		{
			if (Custom.DistLess(coord, base.tracker.GetRep(j).BestGuessForPosition(), (base.tracker.GetRep(j).representedCreature.creatureTemplate.type == CreatureTemplate.Type.BigNeedleWorm || base.tracker.GetRep(j).representedCreature.creatureTemplate.type == CreatureTemplate.Type.SmallNeedleWorm || base.tracker.GetRep(j).representedCreature == Mother) ? 3f : 15f) && base.tracker.GetRep(j).dynamicRelationship.currentRelationship.type != CreatureTemplate.Relationship.Type.Ignores && base.tracker.GetRep(j).dynamicRelationship.state.alive)
			{
				num += Custom.LerpMap(coord.Tile.FloatDist(base.tracker.GetRep(j).BestGuessForPosition().Tile), 1f, (base.tracker.GetRep(j).representedCreature.creatureTemplate.type == CreatureTemplate.Type.BigNeedleWorm || base.tracker.GetRep(j).representedCreature.creatureTemplate.type == CreatureTemplate.Type.SmallNeedleWorm || base.tracker.GetRep(j).representedCreature == Mother) ? 3f : 15f, 30f * base.tracker.GetRep(j).representedCreature.creatureTemplate.bodySize, 0f, 0.5f);
			}
		}
		num += creature.pos.Tile.FloatDist(coord.Tile) / 20f;
		num -= (float)Mathf.Min(worm.room.aimap.getTerrainProximity(coord.Tile), 3) * 5f;
		if (worm.room.aimap.getAItile(coord.Tile).narrowSpace)
		{
			num += 1000f;
		}
		bool flag = TileInEnclosedArea(coord.Tile);
		num = ((!flag) ? (num + Mathf.Min(12f, Mathf.Abs(worm.room.aimap.getAItile(coord.Tile).smoothedFloorAltitude - MinFlyHeight(coord.Tile, flag))) * 0.75f) : (num + 100f));
		if (worm.room.aimap.getAItile(coord.Tile).smoothedFloorAltitude < MinFlyHeight(coord.Tile, flag))
		{
			num += 20f;
		}
		return num;
	}

	AIModule IUseARelationshipTracker.ModuleToTrackRelationship(CreatureTemplate.Relationship relationship)
	{
		if (relationship.type == CreatureTemplate.Relationship.Type.Afraid)
		{
			return base.threatTracker;
		}
		if (relationship.type == CreatureTemplate.Relationship.Type.Attacks)
		{
			return base.preyTracker;
		}
		return null;
	}

	public RelationshipTracker.TrackedCreatureState CreateTrackedCreatureState(RelationshipTracker.DynamicRelationship rel)
	{
		return new NeedleWormTrackState();
	}

	CreatureTemplate.Relationship IUseARelationshipTracker.UpdateDynamicRelationship(RelationshipTracker.DynamicRelationship dRelation)
	{
		if (ModManager.MSC && dRelation.trackerRep.representedCreature.creatureTemplate.type == CreatureTemplate.Type.Overseer)
		{
			CreatureTemplate.Relationship result = StaticRelationship(dRelation.trackerRep.representedCreature).Duplicate();
			if ((dRelation.trackerRep.representedCreature.abstractAI as OverseerAbstractAI).safariOwner)
			{
				result.type = CreatureTemplate.Relationship.Type.Ignores;
				return result;
			}
		}
		if (dRelation.trackerRep.VisualContact)
		{
			dRelation.state.alive = dRelation.trackerRep.representedCreature.state.alive;
		}
		CreatureTemplate.Relationship currRel = StaticRelationship(dRelation.trackerRep.representedCreature);
		currRel = UncomfortableToAfraidRelationshipModifier(dRelation, currRel);
		if (currRel.type == CreatureTemplate.Relationship.Type.Afraid && !dRelation.state.alive)
		{
			currRel.intensity = 0f;
		}
		return currRel;
	}
}
