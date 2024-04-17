using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class VultureAI : ArtificialIntelligence, ILookingAtCreatures, IUseARelationshipTracker
{
	public class Behavior : ExtEnum<Behavior>
	{
		public static readonly Behavior Idle = new Behavior("Idle", register: true);

		public static readonly Behavior Hunt = new Behavior("Hunt", register: true);

		public static readonly Behavior EscapeRain = new Behavior("EscapeRain", register: true);

		public static readonly Behavior ReturnPrey = new Behavior("ReturnPrey", register: true);

		public static readonly Behavior GetUnstuck = new Behavior("GetUnstuck", register: true);

		public static readonly Behavior Disencouraged = new Behavior("Disencouraged", register: true);

		public static readonly Behavior GoToMask = new Behavior("GoToMask", register: true);

		public Behavior(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class DisencouragedTracker : AIModule
	{
		public float disencouraged;

		public DisencouragedTracker(ArtificialIntelligence AI)
			: base(AI)
		{
		}

		public override void Update()
		{
			base.Update();
			if (disencouraged > 1f)
			{
				disencouraged = Mathf.Lerp(disencouraged, 1f, (AI as VultureAI).IsMiros ? 0.075f : 0.05f);
			}
		}

		public override float Utility()
		{
			return Mathf.Pow(Mathf.Clamp(disencouraged, 0f, 1f), 3f);
		}
	}

	public DebugDestinationVisualizer debugDestinationVisualizer;

	public CreatureLooker creatureLooker;

	public Tracker.CreatureRepresentation focusCreature;

	public Behavior behavior;

	public new int timeInRoom;

	public WorldCoordinate kingTuskShootPos;

	public bool preyInTuskChargeRange;

	private DisencouragedTracker disencouragedTracker;

	public Vulture vulture => creature.realizedCreature as Vulture;

	private bool IsKing => creature.creatureTemplate.type == CreatureTemplate.Type.KingVulture;

	private bool IsMiros
	{
		get
		{
			if (ModManager.MSC)
			{
				return creature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.MirosVulture;
			}
			return false;
		}
	}

	public float disencouraged
	{
		get
		{
			return disencouragedTracker.disencouraged;
		}
		set
		{
			disencouragedTracker.disencouraged = value;
		}
	}

	public VultureAI(AbstractCreature creature, World world)
		: base(creature, world)
	{
		vulture.AI = this;
		AddModule(new VulturePather(this, world, creature));
		base.pathFinder.accessibilityStepsPerFrame = 60;
		base.pathFinder.stepsPerFrame = (IsKing ? 50 : 30);
		AddModule(new Tracker(this, 10, 5, -1, 0.5f, 5, 5, 10));
		AddModule(new RainTracker(this));
		AddModule(new DenFinder(this, creature));
		AddModule(new PreyTracker(this, 5, IsMiros ? 1.8f : (IsKing ? 1.5f : 1.1f), 60f, IsMiros ? 600f : 800f, IsKing ? 0.2f : 0.75f));
		AddModule(new RelationshipTracker(this, base.tracker));
		disencouragedTracker = new DisencouragedTracker(this);
		AddModule(disencouragedTracker);
		AddModule(new StuckTracker(this, trackPastPositions: true, trackNotFollowingCurrentGoal: true));
		base.stuckTracker.AddSubModule(new StuckTracker.GetUnstuckPosCalculator(base.stuckTracker));
		base.stuckTracker.AddSubModule(new StuckTracker.CloseToGoalButNotSeeingItTracker(base.stuckTracker, 5f));
		base.pathFinder.walkPastPointOfNoReturn = true;
		AddModule(new UtilityComparer(this));
		base.utilityComparer.AddComparedModule(base.preyTracker, null, 1f, 1.1f);
		base.utilityComparer.AddComparedModule(base.rainTracker, null, 1f, 1.1f);
		base.utilityComparer.AddComparedModule(base.stuckTracker, null, 1f, 1.1f);
		base.utilityComparer.AddComparedModule(disencouragedTracker, null, 0.95f, 1.1f);
		creatureLooker = new CreatureLooker(this, base.tracker, creature.realizedCreature, 1f / 60f, 70);
	}

	public override void NewRoom(Room room)
	{
		kingTuskShootPos = creature.pos;
		base.NewRoom(room);
		timeInRoom = 0;
	}

	public override void Update()
	{
		if (behavior == Behavior.Hunt && !RainWorldGame.RequestHeavyAi(vulture))
		{
			return;
		}
		if (ModManager.MSC && vulture.LickedByPlayer != null)
		{
			base.tracker.SeeCreature(vulture.LickedByPlayer.abstractCreature);
			if (timeInRoom - 2 > 6000)
			{
				timeInRoom -= 2;
			}
		}
		if (debugDestinationVisualizer != null)
		{
			debugDestinationVisualizer.Update();
		}
		if (creatureLooker != null)
		{
			creatureLooker.Update();
		}
		timeInRoom++;
		if (vulture.room.game.IsStorySession && vulture.room.game.StoryCharacter == SlugcatStats.Name.Yellow)
		{
			timeInRoom++;
		}
		disencouraged = Mathf.Max(0f, disencouraged - 1f / Mathf.Lerp(600f, 4800f, disencouraged));
		preyInTuskChargeRange = false;
		behavior = Behavior.Idle;
		base.utilityComparer.GetUtilityTracker(base.preyTracker).weight = 0.05f + 0.95f * Mathf.InverseLerp(IsMiros ? 4000f : 9600f, IsMiros ? 7600f : 6000f, timeInRoom);
		if (IsMiros)
		{
			base.utilityComparer.GetUtilityTracker(disencouragedTracker).weight += Mathf.InverseLerp(2000f, 13600f, timeInRoom);
		}
		if (ModManager.MMF && vulture.bodyChunks[0].pos.y < 0f - vulture.bodyChunks[0].restrictInRoomRange + 1f)
		{
			creature.abstractAI.SetDestination(vulture.room.GetWorldCoordinate(new Vector2(vulture.bodyChunks[0].pos.x, 500f)));
			return;
		}
		AIModule aIModule = base.utilityComparer.HighestUtilityModule();
		if (base.utilityComparer.HighestUtility() > 0.01f && aIModule != null)
		{
			if (aIModule is PreyTracker)
			{
				behavior = Behavior.Hunt;
			}
			if (aIModule is StuckTracker)
			{
				behavior = Behavior.GetUnstuck;
			}
			if (aIModule is DisencouragedTracker)
			{
				behavior = Behavior.Disencouraged;
			}
		}
		if (vulture.grasps[0] != null && vulture.grasps[0].grabbed is Creature && vulture.Template.CreatureRelationship(vulture.grasps[0].grabbed as Creature).type == CreatureTemplate.Relationship.Type.Eats)
		{
			behavior = (base.denFinder.GetDenPosition().HasValue ? Behavior.ReturnPrey : Behavior.Idle);
		}
		if (!IsMiros && (creature.abstractAI as VultureAbstractAI).lostMask != null && base.utilityComparer.HighestUtility() < 0.4f && (creature.abstractAI as VultureAbstractAI).lostMask.Room.realizedRoom == vulture.room && (creature.abstractAI as VultureAbstractAI).lostMask.realizedObject != null)
		{
			behavior = Behavior.GoToMask;
			WorldCoordinate worldCoordinate = vulture.room.GetWorldCoordinate((creature.abstractAI as VultureAbstractAI).lostMask.realizedObject.firstChunk.pos);
			if (creature.world.GetAbstractRoom(worldCoordinate.room).AttractionForCreature(creature.creatureTemplate.type) != AbstractRoom.CreatureRoomAttraction.Forbidden)
			{
				SetDestination(worldCoordinate);
			}
		}
		if (!(behavior == Behavior.GoToMask))
		{
			if (behavior == Behavior.Idle)
			{
				creature.abstractAI.AbstractBehavior(1);
				if (creature.world.GetAbstractRoom(creature.abstractAI.destination.room).AttractionForCreature(creature.creatureTemplate.type) != AbstractRoom.CreatureRoomAttraction.Forbidden && creature.abstractAI.destination.room == creature.pos.room && creature.abstractAI.destination.NodeDefined && creature.world.GetNode(creature.abstractAI.destination).type == AbstractRoomNode.Type.SkyExit && (!creature.abstractAI.destination.TileDefined || creature.abstractAI.destination.Tile.FloatDist(creature.pos.Tile) < 10f))
				{
					RoomBorderExit roomBorderExit = vulture.room.borderExits[creature.abstractAI.destination.abstractNode - vulture.room.exitAndDenIndex.Length];
					if (roomBorderExit.borderTiles.Length != 0)
					{
						IntVector2 intVector = roomBorderExit.borderTiles[Random.Range(0, roomBorderExit.borderTiles.Length)];
						IntVector2 intVector2 = new IntVector2(0, 1);
						if (intVector.x == 0)
						{
							intVector2 = new IntVector2(-1, 0);
						}
						else if (intVector.x == vulture.room.TileWidth - 1)
						{
							intVector2 = new IntVector2(1, 0);
						}
						else if (intVector.y == 0)
						{
							intVector2 = new IntVector2(0, -1);
						}
						intVector += intVector2 * ((intVector2.y == 1) ? Random.Range(0, 40) : Random.Range(0, 10));
						creature.abstractAI.SetDestination(new WorldCoordinate(creature.abstractAI.destination.room, intVector.x, intVector.y, creature.abstractAI.destination.abstractNode));
					}
				}
			}
			else if (behavior == Behavior.ReturnPrey || behavior == Behavior.EscapeRain || behavior == Behavior.Disencouraged)
			{
				focusCreature = null;
				if (base.denFinder.GetDenPosition().HasValue)
				{
					creature.abstractAI.SetDestination(base.denFinder.GetDenPosition().Value);
				}
			}
			else if (behavior == Behavior.Hunt)
			{
				focusCreature = base.preyTracker.MostAttractivePrey;
				if (focusCreature.dynamicRelationship.currentRelationship.type == CreatureTemplate.Relationship.Type.Attacks)
				{
					timeInRoom = 0;
				}
				WorldCoordinate destination = focusCreature.BestGuessForPosition();
				bool flag = ((!IsMiros) ? focusCreature.representedCreature.creatureTemplate.IsVulture : (focusCreature.representedCreature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.MirosVulture));
				if (flag && focusCreature.VisualContact && focusCreature.representedCreature.realizedCreature != null)
				{
					destination = vulture.room.GetWorldCoordinate(focusCreature.representedCreature.realizedCreature.bodyChunks[4].pos);
				}
				if (!IsMiros && vulture.kingTusks != null && vulture.kingTusks.ReadyToShoot && focusCreature != null && focusCreature.TicksSinceSeen < 80 && vulture.kingTusks.WantToShoot(checkVisualOnAnyTargetChunk: false, checkMinDistance: true))
				{
					WorldCoordinate worldCoordinate2 = vulture.room.GetWorldCoordinate(vulture.room.MiddleOfTile(focusCreature.BestGuessForPosition()) + Custom.DegToVec(180f * Random.value * Random.value * ((Random.value < 0.5f) ? (-1f) : 1f)) * Mathf.Lerp(KingTusks.Tusk.minShootRange, KingTusks.Tusk.shootRange, Random.value));
					float num = KingTuskShootPosScore(kingTuskShootPos);
					if (KingTuskShootPosScore(worldCoordinate2) < num)
					{
						kingTuskShootPos = worldCoordinate2;
					}
					creature.abstractAI.SetDestination(kingTuskShootPos);
				}
				else if (creature.world.GetAbstractRoom(destination.room).AttractionForCreature(creature.creatureTemplate.type) != AbstractRoom.CreatureRoomAttraction.Forbidden)
				{
					creature.abstractAI.SetDestination(destination);
				}
				if (focusCreature.VisualContact)
				{
					Creature realizedCreature = focusCreature.representedCreature.realizedCreature;
					if (realizedCreature.bodyChunks.Length != 0)
					{
						BodyChunk bodyChunk = realizedCreature.bodyChunks[Random.Range(0, realizedCreature.bodyChunks.Length)];
						preyInTuskChargeRange = Custom.DistLess(vulture.mainBodyChunk.pos, bodyChunk.pos, 230f);
						if ((!vulture.AirBorne || Random.value < 1f / 60f) && vulture.tuskCharge == 1f && vulture.snapFrames == 0 && !vulture.isLaserActive() && !vulture.safariControlled && Custom.DistLess(vulture.mainBodyChunk.pos, bodyChunk.pos, 130f) && vulture.room.VisualContact(vulture.bodyChunks[4].pos, bodyChunk.pos))
						{
							vulture.Snap(bodyChunk);
						}
					}
				}
			}
			else if (behavior == Behavior.GetUnstuck)
			{
				creature.abstractAI.SetDestination(base.stuckTracker.getUnstuckPosCalculator.unstuckGoalPosition);
			}
		}
		base.Update();
	}

	public float KingTuskShootPosScore(WorldCoordinate test)
	{
		if (!base.pathFinder.CoordinateReachable(test))
		{
			return float.MaxValue;
		}
		float num = Vector2.Distance(vulture.room.MiddleOfTile(test), vulture.room.MiddleOfTile(focusCreature.BestGuessForPosition()));
		float num2 = Mathf.Abs(KingTusks.Tusk.shootRange * 0.9f - num);
		num2 += Vector2.Distance(vulture.room.MiddleOfTile(test), vulture.room.MiddleOfTile(kingTuskShootPos)) / 40f;
		if (test.y >= vulture.room.TileHeight - 1)
		{
			num2 += 1000f;
		}
		num2 -= (float)Mathf.Min(vulture.room.aimap.getTerrainProximity(test), 10) * 20f;
		num2 += Custom.LerpMap(num, KingTusks.Tusk.shootRange - 100f, KingTusks.Tusk.shootRange, 0f, 1000f);
		num2 += Custom.LerpMap(num, KingTusks.Tusk.minShootRange, KingTusks.Tusk.minShootRange / 2f, 0f, 1000f);
		if (!vulture.room.VisualContact(vulture.room.MiddleOfTile(test), vulture.room.MiddleOfTile(focusCreature.BestGuessForPosition())))
		{
			num2 += 10000f;
		}
		return num2;
	}

	public override float VisualScore(Vector2 lookAtPoint, float targetSpeed)
	{
		float num = base.VisualScore(lookAtPoint, targetSpeed);
		if (!Custom.DistLess(vulture.mainBodyChunk.pos, lookAtPoint, 40f))
		{
			num -= Mathf.Pow(Mathf.InverseLerp(1f, -0.3f, Vector2.Dot((vulture.neck.tChunks[vulture.neck.tChunks.Length - 1].pos - vulture.bodyChunks[4].pos).normalized, (vulture.neck.tChunks[vulture.neck.tChunks.Length - 1].pos - lookAtPoint).normalized)), IsKing ? 0.5f : 0.15f);
		}
		return num;
	}

	public bool OnlyHurtDontGrab(PhysicalObject testObj)
	{
		if (testObj is Creature && base.tracker.RepresentationForCreature((testObj as Creature).abstractCreature, addIfMissing: false) != null && base.tracker.RepresentationForCreature((testObj as Creature).abstractCreature, addIfMissing: false).dynamicRelationship.currentRelationship.type == CreatureTemplate.Relationship.Type.Attacks)
		{
			return true;
		}
		return false;
	}

	public bool DoIWantToBiteCreature(AbstractCreature creature)
	{
		if (IsMiros)
		{
			return !creature.creatureTemplate.smallCreature;
		}
		return false;
	}

	public override bool TrackerToDiscardDeadCreature(AbstractCreature crit)
	{
		return false;
	}

	public override void CreatureSpotted(bool firstSpot, Tracker.CreatureRepresentation creatureRep)
	{
		creatureLooker.ReevaluateLookObject(creatureRep, firstSpot ? 3f : 2f);
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
		if (relationship.type == CreatureTemplate.Relationship.Type.Eats || relationship.type == CreatureTemplate.Relationship.Type.Attacks)
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
		if (!IsMiros && dRelation.trackerRep.representedCreature.creatureTemplate.IsVulture && dRelation.trackerRep.representedCreature.state.alive && (vulture.State as Vulture.VultureState).mask != (dRelation.trackerRep.representedCreature.state as Vulture.VultureState).mask)
		{
			return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, (vulture.State as Vulture.VultureState).mask ? 0.8f : 0.1f);
		}
		if (!(vulture.State as Vulture.VultureState).mask || IsMiros)
		{
			if (vulture.State.socialMemory.GetLike(dRelation.trackerRep.representedCreature.ID) < -0.25f)
			{
				return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, 1f);
			}
			if (IsMiros && dRelation.trackerRep.representedCreature.creatureTemplate.type == CreatureTemplate.Type.Slugcat)
			{
				return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f);
			}
			return new CreatureTemplate.Relationship(StaticRelationship(dRelation.trackerRep.representedCreature).type, StaticRelationship(dRelation.trackerRep.representedCreature).intensity * (IsMiros ? 1f : 0.1f));
		}
		CreatureTemplate.Relationship result = StaticRelationship(dRelation.trackerRep.representedCreature);
		if (dRelation.trackerRep.representedCreature.creatureTemplate.type == CreatureTemplate.Type.Slugcat)
		{
			result = ((!vulture.room.game.IsStorySession || !(vulture.room.game.StoryCharacter == SlugcatStats.Name.Yellow)) ? new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, Custom.LerpMap(creature.world.game.session.difficulty, -1f, 1f, 0.2f, 0.5f) + (IsKing ? 0.15f : 0f)) : new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.15f));
		}
		if (IsKing && result.type == CreatureTemplate.Relationship.Type.Eats)
		{
			if (vulture.kingTusks.AnyCreatureImpaled)
			{
				result.intensity = (vulture.kingTusks.ThisCreatureImpaled(dRelation.trackerRep.representedCreature) ? 1f : 0.5f);
			}
			else
			{
				result.intensity *= Custom.LerpMap(vulture.room.aimap.AccessibilityForCreature(dRelation.trackerRep.BestGuessForPosition().Tile, vulture.Template), 0.9f, 0.6f, 1f, 0.5f + 0.5f * Mathf.InverseLerp(180f, 10f, dRelation.trackerRep.TicksSinceSeen));
				if (vulture.kingTusks.targetRep != null && vulture.kingTusks.ReadyToShoot && vulture.kingTusks.eyesHomeIn > 0f && vulture.kingTusks.targetRep.TicksSinceSeen < 30)
				{
					result.intensity = Mathf.Pow(result.intensity, (vulture.kingTusks.targetRep == dRelation.trackerRep) ? 0.5f : 1.5f);
				}
			}
		}
		return result;
	}

	public override PathCost TravelPreference(MovementConnection connection, PathCost cost)
	{
		int num = (IsKing ? 8 : 6);
		if (IsMiros)
		{
			num = 10;
		}
		int terrainProximity = vulture.room.aimap.getTerrainProximity(connection.destinationCoord);
		cost.resistance += (float)Custom.IntClamp(num - terrainProximity, 0, num) * 5f;
		if (IsMiros && terrainProximity <= 3)
		{
			cost.resistance += 100f;
		}
		return cost;
	}

	public float CreatureInterestBonus(Tracker.CreatureRepresentation crit, float score)
	{
		if (crit == focusCreature)
		{
			score *= 5f;
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
