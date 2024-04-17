using RWCustom;
using UnityEngine;

public class BigNeedleWormAI : NeedleWormAI, IUseARelationshipTracker, IReactToSocialEvents
{
	public new BigNeedleWorm worm;

	public Vector2 attackFromPos;

	public Vector2 attackTargetPos;

	public int attackCounter;

	public int targetChunk;

	private Vector2 targetVel;

	private float idealAttackDist;

	public int respondScreamCounter;

	public Creature keepCloseToCreature;

	public BigNeedleWormAI(AbstractCreature creature, World world)
		: base(creature, world)
	{
		worm = creature.realizedCreature as BigNeedleWorm;
		worm.AI = this;
		AddModule(new PreyTracker(this, 5, 1.3f, 15f, 1000f, 0.35f));
		base.utilityComparer.AddComparedModule(base.preyTracker, new FloatTweener.FloatTweenBasic(FloatTweener.TweenType.Lerp, 0.15f), 0.9f, 1.1f);
		AddModule(new StuckTracker(this, trackPastPositions: true, trackNotFollowingCurrentGoal: true));
		base.stuckTracker.AddSubModule(new StuckTracker.GetUnstuckPosCalculator(base.stuckTracker));
		base.stuckTracker.totalTrackedLastPositions = 40;
		base.stuckTracker.checkPastPositionsFrom = 20;
		base.stuckTracker.pastStuckPositionsCloseToIncrementStuckCounter = 15;
		base.utilityComparer.AddComparedModule(base.stuckTracker, null, 1f, 1.1f);
		targetChunk = Random.Range(0, 100);
		if (world.game.IsArenaSession && world.game.GetArenaGameSession.GameTypeSetup.evilAI)
		{
			for (int i = 0; i < world.game.session.Players.Count; i++)
			{
				creature.state.socialMemory.GetOrInitiateRelationship(world.game.Players[i].ID).like = -1f;
				creature.state.socialMemory.GetOrInitiateRelationship(world.game.Players[i].ID).tempLike = -1f;
			}
		}
	}

	public void SmallRespondCry()
	{
		if (respondScreamCounter == 0)
		{
			respondScreamCounter = Random.Range(10, 50);
		}
	}

	public void BigRespondCry()
	{
		if (respondScreamCounter > -1)
		{
			respondScreamCounter = -Random.Range(6, 16);
		}
	}

	public override void Update()
	{
		base.Update();
		if (ModManager.MSC && worm.LickedByPlayer != null)
		{
			base.tracker.SeeCreature(worm.LickedByPlayer.abstractCreature);
			if (creature.abstractAI.followCreature != worm.LickedByPlayer.abstractCreature)
			{
				worm.abstractCreature.state.socialMemory.GetOrInitiateRelationship(worm.LickedByPlayer.abstractCreature.ID).like = -1f;
				worm.abstractCreature.state.socialMemory.GetOrInitiateRelationship(worm.LickedByPlayer.abstractCreature.ID).tempLike = -1f;
				worm.abstractCreature.abstractAI.followCreature = worm.LickedByPlayer.abstractCreature;
				worm.BigCry();
			}
		}
		if (worm.room == null)
		{
			return;
		}
		if (respondScreamCounter < 0)
		{
			respondScreamCounter++;
			if (respondScreamCounter == 0)
			{
				worm.BigCry();
			}
		}
		else if (respondScreamCounter > 0)
		{
			respondScreamCounter--;
			if (respondScreamCounter == 0)
			{
				worm.SmallCry();
			}
		}
		creature.state.socialMemory.EvenOutAllTemps(0.0005f);
		AIModule aIModule = base.utilityComparer.HighestUtilityModule();
		float num = base.utilityComparer.HighestUtility();
		if (aIModule != null)
		{
			if (aIModule is ThreatTracker)
			{
				behavior = Behavior.Flee;
			}
			else if (aIModule is PreyTracker)
			{
				behavior = Behavior.Attack;
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
		if (creature.abstractAI.followCreature != null && creature.abstractAI.followCreature.pos.room == creature.pos.room)
		{
			if (!Custom.DistLess(creature.pos, creature.abstractAI.followCreature.pos, 30f) && Random.value < 1f / 30f)
			{
				base.tracker.SeeCreature(creature.abstractAI.followCreature);
			}
			else if (num < 0.8f && creature.abstractAI.followCreature.pos.room != creature.pos.room && creature.world.GetAbstractRoom(creature.abstractAI.followCreature.pos.room) != null && creature.world.GetAbstractRoom(creature.abstractAI.followCreature.pos.room).AttractionForCreature(creature.creatureTemplate.type) != AbstractRoom.CreatureRoomAttraction.Forbidden)
			{
				behavior = Behavior.Migrate;
				num = 0.8f;
			}
		}
		if (keepCloseToCreature != null)
		{
			if (behavior != Behavior.Attack)
			{
				behavior = Behavior.FauxAttack;
			}
			if (Random.value < 0.0125f)
			{
				keepCloseToCreature = null;
			}
		}
		attackCounter--;
		if (Random.value < 1f / 120f)
		{
			targetChunk = Random.Range(0, 100);
		}
		Vector2 vector = attackTargetPos;
		targetVel *= 0.99f;
		if (behavior == Behavior.Migrate)
		{
			if (creature.abstractAI.followCreature != null)
			{
				creature.abstractAI.SetDestination(creature.abstractAI.followCreature.pos.WashTileData());
			}
		}
		else if (behavior == Behavior.Idle)
		{
			if (creature.abstractAI.WantToMigrate)
			{
				creature.abstractAI.SetDestination(creature.abstractAI.MigrationDestination);
			}
			else
			{
				IdleBehavior();
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
		else if (behavior == Behavior.Attack)
		{
			vector = AttackBehavior(base.preyTracker.MostAttractivePrey, vector);
		}
		else if (behavior == Behavior.FauxAttack)
		{
			if (keepCloseToCreature != null)
			{
				vector = AttackBehavior(base.tracker.RepresentationForCreature(keepCloseToCreature.abstractCreature, addIfMissing: false), vector);
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
		attackTargetPos = Vector2.Lerp(attackTargetPos, vector + targetVel * Custom.LerpMap(Vector2.Distance(attackFromPos, vector), 80f, 400f, 0f, 30f, 0.35f), Mathf.InverseLerp(0.9f, 0.5f, worm.chargingAttack));
		if (worm.room.abstractRoom.creatures.Count > 0)
		{
			AbstractCreature abstractCreature = worm.room.abstractRoom.creatures[Random.Range(0, worm.room.abstractRoom.creatures.Count)];
			if (abstractCreature.realizedCreature != null && !abstractCreature.state.dead && abstractCreature.realizedCreature.room == worm.room && creature.state.socialMemory.GetTempLike(abstractCreature.ID) < -0.25f && base.tracker.RepresentationForCreature(abstractCreature, addIfMissing: false) != null && base.tracker.RepresentationForCreature(abstractCreature, addIfMissing: false).TicksSinceSeen > 80)
			{
				base.tracker.SeeCreature(abstractCreature);
			}
		}
		attackCounter = Custom.IntClamp(attackCounter, 0, 100);
	}

	protected override float IdleScore(WorldCoordinate coord)
	{
		if (coord.room != worm.room.abstractRoom.index || !base.pathFinder.CoordinateReachableAndGetbackable(coord))
		{
			return float.MaxValue;
		}
		if (coord.Tile.x < 0 || coord.Tile.x >= worm.room.TileWidth || coord.Tile.y < 0 || coord.Tile.y >= worm.room.TileHeight)
		{
			return float.MaxValue;
		}
		float num = 0f;
		for (int i = 0; i < oldIdlePositions.Count; i++)
		{
			if (oldIdlePositions[i].room == coord.room)
			{
				num -= Mathf.Pow(Mathf.Min(30f, oldIdlePositions[i].Tile.FloatDist(coord.Tile)), 2f) / (30f * (1f + (float)i * 0.2f));
			}
		}
		for (int j = 0; j < base.tracker.CreaturesCount; j++)
		{
			if (Custom.DistLess(coord, base.tracker.GetRep(j).BestGuessForPosition(), 25f) && base.tracker.GetRep(j).representedCreature.creatureTemplate.type != CreatureTemplate.Type.SmallNeedleWorm && base.tracker.GetRep(j).dynamicRelationship.currentRelationship.type != CreatureTemplate.Relationship.Type.Ignores && base.tracker.GetRep(j).dynamicRelationship.state.alive)
			{
				num += Custom.LerpMap(coord.Tile.FloatDist(base.tracker.GetRep(j).BestGuessForPosition().Tile), 1f, 25f, 30f * base.tracker.GetRep(j).representedCreature.creatureTemplate.bodySize, 0f, 0.5f);
			}
		}
		num -= Mathf.Pow(Mathf.Min(worm.room.aimap.getTerrainProximity(coord.Tile), 5), 2f) * 2f;
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

	public override float CurrentPlayerAggression(AbstractCreature player)
	{
		if (behavior != Behavior.Attack)
		{
			return 0f;
		}
		Tracker.CreatureRepresentation creatureRepresentation = base.tracker.RepresentationForCreature(player, addIfMissing: false);
		if (creatureRepresentation == null || creatureRepresentation.dynamicRelationship == null || creatureRepresentation.dynamicRelationship.currentRelationship.type != CreatureTemplate.Relationship.Type.Attacks)
		{
			return 0f;
		}
		return Mathf.InverseLerp(0f, 0.5f, creatureRepresentation.dynamicRelationship.currentRelationship.intensity) * ((focusCreature != null && focusCreature.representedCreature == player) ? 1f : 0.3f) * Mathf.InverseLerp(0f, 40f, attackCounter);
	}

	private Vector2 AttackBehavior(Tracker.CreatureRepresentation attackCrit, Vector2 newAttackTargetPos)
	{
		if (attackCrit != null)
		{
			focusCreature = attackCrit;
			if (focusCreature.representedCreature.creatureTemplate.type == CreatureTemplate.Type.BigNeedleWorm)
			{
				idealAttackDist = 400f;
			}
			else
			{
				idealAttackDist = 200f;
			}
			newAttackTargetPos = worm.room.MiddleOfTile(attackCrit.BestGuessForPosition());
			if (attackCrit.BestGuessForPosition().room != creature.pos.room || attackCrit.TicksSinceSeen > 20)
			{
				creature.abstractAI.SetDestination(attackCrit.BestGuessForPosition());
				attackFromPos = worm.room.MiddleOfTile(attackCrit.BestGuessForPosition());
			}
			else
			{
				if (attackCrit.VisualContact)
				{
					BodyChunk bodyChunk = attackCrit.representedCreature.realizedCreature.bodyChunks[targetChunk % attackCrit.representedCreature.realizedCreature.bodyChunks.Length];
					newAttackTargetPos = bodyChunk.pos;
					targetVel = Custom.MoveTowards(targetVel, bodyChunk.pos - bodyChunk.lastPos, 0.075f);
				}
				if (worm.chargingAttack < 0.5f)
				{
					Vector2 test = ((Random.value < 0.5f) ? attackFromPos : attackTargetPos) + Custom.RNV() * 300f * Random.value;
					if (AttackPosScore(test, attackTargetPos) < AttackPosScore(attackFromPos, attackTargetPos))
					{
						attackFromPos = test;
					}
				}
				if (behavior == Behavior.Attack && Custom.DistLess(worm.mainBodyChunk.pos, attackFromPos, 300f))
				{
					attackCounter++;
				}
				creature.abstractAI.SetDestination(worm.room.GetWorldCoordinate(attackFromPos));
			}
			if (Custom.DistLess(worm.mainBodyChunk.pos, attackFromPos, 800f))
			{
				attackCounter++;
			}
		}
		else
		{
			attackFromPos = worm.mainBodyChunk.pos;
		}
		return newAttackTargetPos;
	}

	private float AttackPosScore(Vector2 test, Vector2 targetPos)
	{
		WorldCoordinate worldCoordinate = worm.room.GetWorldCoordinate(test);
		if (!base.pathFinder.CoordinateReachableAndGetbackable(worldCoordinate) || !worm.room.VisualContact(test, targetPos))
		{
			return float.MaxValue;
		}
		int terrainProximity = worm.room.aimap.getTerrainProximity(worldCoordinate.Tile);
		if (terrainProximity < 2)
		{
			return float.MaxValue;
		}
		float num = Mathf.Max(0f, Mathf.Abs(idealAttackDist - Vector2.Distance(test, targetPos)) - 50f);
		num -= (float)terrainProximity * 20f;
		num += Vector2.Distance(test, worm.mainBodyChunk.pos) / 10f;
		if (Custom.DistLess(test, worm.mainBodyChunk.pos, 60f))
		{
			num -= (float)attackCounter * 5f;
		}
		if (test.x < 0f || test.y < 0f || test.x > worm.room.PixelWidth || test.y > worm.room.PixelHeight)
		{
			num += 1000f;
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
		if (dRelation.trackerRep.representedCreature.creatureTemplate.smallCreature)
		{
			return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f);
		}
		if (dRelation.trackerRep.VisualContact)
		{
			dRelation.state.alive = dRelation.trackerRep.representedCreature.state.alive;
			(dRelation.state as NeedleWormTrackState).holdingChild = false;
			if (dRelation.trackerRep.representedCreature.realizedCreature != null && dRelation.trackerRep.representedCreature.realizedCreature.grasps != null)
			{
				for (int i = 0; i < dRelation.trackerRep.representedCreature.realizedCreature.grasps.Length; i++)
				{
					if (dRelation.trackerRep.representedCreature.realizedCreature.grasps[i] != null && (dRelation.trackerRep.representedCreature.realizedCreature.grasps[i].grabbed is SmallNeedleWorm || dRelation.trackerRep.representedCreature.realizedCreature.grasps[i].grabbed is NeedleEgg))
					{
						(dRelation.state as NeedleWormTrackState).holdingChild = true;
						break;
					}
				}
			}
		}
		if ((dRelation.state as NeedleWormTrackState).holdingChild)
		{
			return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, 1f);
		}
		if (creature.state.socialMemory.GetTempLike(dRelation.trackerRep.representedCreature.ID) < -0.25f)
		{
			return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, Mathf.Abs(creature.state.socialMemory.GetTempLike(dRelation.trackerRep.representedCreature.ID)));
		}
		CreatureTemplate.Relationship currRel = StaticRelationship(dRelation.trackerRep.representedCreature);
		return UncomfortableToAfraidRelationshipModifier(dRelation, currRel);
	}

	public void SocialEvent(SocialEventRecognizer.EventID ID, Creature subjectCrit, Creature objectCrit, PhysicalObject involvedItem)
	{
		if (base.tracker.RepresentationForCreature(subjectCrit.abstractCreature, addIfMissing: false) == null || base.tracker.RepresentationForCreature(subjectCrit.abstractCreature, addIfMissing: false).TicksSinceSeen > 80 || (objectCrit != worm && (!(objectCrit.Template.type == CreatureTemplate.Type.SmallNeedleWorm) || (objectCrit as SmallNeedleWorm).Mother != worm)))
		{
			return;
		}
		float num = 0f;
		if (ID == SocialEventRecognizer.EventID.NonLethalAttackAttempt)
		{
			num = 0.1f;
			if (objectCrit != worm)
			{
				num /= 2f;
			}
		}
		else if (ID == SocialEventRecognizer.EventID.NonLethalAttack)
		{
			num = 0.2f;
		}
		else if (ID == SocialEventRecognizer.EventID.LethalAttackAttempt)
		{
			num = 0.4f;
			if (objectCrit != worm)
			{
				num /= 2f;
			}
		}
		else if (ID == SocialEventRecognizer.EventID.LethalAttack)
		{
			num = 0.6f;
		}
		else if (ID == SocialEventRecognizer.EventID.Killing)
		{
			num = 0.9f;
		}
		if (objectCrit.dead)
		{
			num /= 3f;
		}
		if (objectCrit == worm)
		{
			num /= 2f;
		}
		creature.state.socialMemory.GetOrInitiateRelationship(subjectCrit.abstractCreature.ID).InfluenceTempLike(num * -1f);
		creature.state.socialMemory.GetOrInitiateRelationship(subjectCrit.abstractCreature.ID).InfluenceLike(num * -0.1f);
	}
}
