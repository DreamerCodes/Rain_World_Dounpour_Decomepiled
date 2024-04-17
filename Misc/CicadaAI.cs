using System;
using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class CicadaAI : ArtificialIntelligence, IUseARelationshipTracker, IReactToSocialEvents, FriendTracker.IHaveFriendTracker
{
	public class Behavior : ExtEnum<Behavior>
	{
		public static readonly Behavior Idle = new Behavior("Idle", register: true);

		public static readonly Behavior Flee = new Behavior("Flee", register: true);

		public static readonly Behavior Hunt = new Behavior("Hunt", register: true);

		public static readonly Behavior EscapeRain = new Behavior("EscapeRain", register: true);

		public static readonly Behavior Antagonize = new Behavior("Antagonize", register: true);

		public static readonly Behavior ReturnPrey = new Behavior("ReturnPrey", register: true);

		public static readonly Behavior GetUnstuck = new Behavior("GetUnstuck", register: true);

		public static readonly Behavior FollowFriend = new Behavior("FollowFriend", register: true);

		public Behavior(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class CicadaTrackState : RelationshipTracker.TrackedCreatureState
	{
		public bool armed;

		public bool caught;

		public bool gotACicada;

		public bool gotCicadaFood;
	}

	public class CircleGroup
	{
		public Room room;

		public List<AbstractCreature> group;

		public Vector2 center;

		public float radius;

		public float rotation;

		public int lifeTime;

		public bool slatedForDeletion;

		public float rotationDir;

		public CircleGroup(Room room, AbstractCreature originalCicada, Vector2 center)
		{
			group = new List<AbstractCreature> { originalCicada };
			this.center = center;
			radius = Mathf.Lerp(40f, (float)(room.aimap.getTerrainProximity(center) - 3) * 15f, UnityEngine.Random.value);
			rotationDir = ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f);
			rotation = UnityEngine.Random.value;
			this.room = room;
			lifeTime = 0;
			slatedForDeletion = false;
		}

		public void Update()
		{
			if (UnityEngine.Random.value < 0.00066666666f)
			{
				rotationDir = 0f - rotationDir;
			}
			lifeTime++;
			if (lifeTime > 300 && UnityEngine.Random.value < 0.0023809525f)
			{
				slatedForDeletion = true;
			}
			radius += Mathf.Lerp(-2f, 2f, UnityEngine.Random.value);
			float max = (float)(room.aimap.getTerrainProximity(center) - 3) * 15f;
			radius = Mathf.Clamp(radius, 40f, max);
			rotation += 1f / 90f * rotationDir * 120f / radius;
			if (rotation < 0f)
			{
				rotation += 1f;
			}
			else if (rotation > 1f)
			{
				rotation -= 1f;
			}
			center += Custom.DegToVec(UnityEngine.Random.value * 360f) * UnityEngine.Random.value * 3f;
			for (int i = 0; i < group.Count; i++)
			{
				if (group[i].pos.room != room.abstractRoom.index)
				{
					if (group[i].realizedCreature != null)
					{
						(group[i].realizedCreature as Cicada).AI.RemoveFromCircle();
					}
					else
					{
						group.RemoveAt(i);
					}
					break;
				}
			}
		}

		public Vector2 MyPos(AbstractCreature cicada)
		{
			int num = 0;
			for (int i = 0; i < group.Count; i++)
			{
				if (group[i] == cicada)
				{
					num = i;
					break;
				}
			}
			return center + Custom.DegToVec((rotation + (float)num / (float)group.Count) * 360f) * radius;
		}

		public void RemoveCicada(AbstractCreature cicada)
		{
			for (int i = 0; i < group.Count; i++)
			{
				if (group[i] == cicada)
				{
					group.RemoveAt(i);
					break;
				}
			}
		}
	}

	public Cicada cicada;

	private DebugDestinationVisualizer debugDestinationVisualizer;

	public float currentUtility;

	public Behavior behavior;

	public WorldCoordinate idleSitSpot;

	public WorldCoordinate forbiddenIdleSitSpot;

	public int idleSitCounter;

	public CircleGroup circleGroup;

	public Vector2? swooshToPos;

	public int noCircleGroupCounter;

	public int huntAttackCounter;

	public AbstractCreature tiredOfHuntingCreature;

	public int tiredOfHuntingCounter;

	public bool antagonizeMethod;

	public int antagonizeMethodCounter;

	public Creature panicFleeCrit;

	public bool migrateToSwarmRoom;

	public Tracker.CreatureRepresentation focusCreature;

	public CicadaAI(AbstractCreature creature, World world)
		: base(creature, world)
	{
		cicada = creature.realizedCreature as Cicada;
		cicada.AI = this;
		migrateToSwarmRoom = !cicada.gender;
		AddModule(new CicadaPather(this, world, creature));
		base.pathFinder.accessibilityStepsPerFrame = 60;
		AddModule(new Tracker(this, 10, 10, 250, 0.5f, 5, 5, 20));
		AddModule(new PreyTracker(this, 5, 1f, 5f, 15f, 0.95f));
		AddModule(new ThreatTracker(this, 3));
		AddModule(new RainTracker(this));
		AddModule(new DenFinder(this, creature));
		AddModule(new StuckTracker(this, trackPastPositions: true, trackNotFollowingCurrentGoal: true));
		AddModule(new FriendTracker(this));
		base.friendTracker.followClosestFriend = false;
		base.stuckTracker.AddSubModule(new StuckTracker.GetUnstuckPosCalculator(base.stuckTracker));
		if (!cicada.gender)
		{
			AddModule(new SwarmRoomFinder(this, creature));
		}
		AddModule(new RelationshipTracker(this, base.tracker));
		AddModule(new UtilityComparer(this));
		base.utilityComparer.AddComparedModule(base.threatTracker, null, 1f, 1.1f);
		base.utilityComparer.AddComparedModule(base.preyTracker, null, 0.5f, 1.1f);
		base.utilityComparer.AddComparedModule(base.rainTracker, null, 1f, 1.1f);
		base.utilityComparer.AddComparedModule(base.stuckTracker, null, 1f, 1.1f);
		behavior = Behavior.Idle;
	}

	public override void NewRoom(Room room)
	{
		idleSitSpot = room.GetWorldCoordinate(new IntVector2(UnityEngine.Random.Range(0, room.TileWidth), UnityEngine.Random.Range(0, room.TileHeight)));
		if (base.pathFinder.GetDestination.room == room.abstractRoom.index && !base.pathFinder.GetDestination.TileDefined && base.pathFinder.GetDestination.CompareDisregardingTile(creature.pos))
		{
			creature.abstractAI.SetDestination(idleSitSpot);
		}
		base.NewRoom(room);
	}

	public override void Update()
	{
		focusCreature = null;
		if (debugDestinationVisualizer != null)
		{
			debugDestinationVisualizer.Update();
		}
		if (noCircleGroupCounter > 0)
		{
			noCircleGroupCounter--;
		}
		base.Update();
		if (ModManager.MSC && cicada.LickedByPlayer != null)
		{
			base.tracker.SeeCreature(cicada.LickedByPlayer.abstractCreature);
		}
		if (panicFleeCrit != null)
		{
			swooshToPos = cicada.mainBodyChunk.pos - Custom.DirVec(cicada.mainBodyChunk.pos, panicFleeCrit.mainBodyChunk.pos) * 100f;
			if (!Custom.DistLess(cicada.mainBodyChunk.pos, panicFleeCrit.mainBodyChunk.pos, 300f) || cicada.mainBodyChunk.ContactPoint.x != 0 || cicada.mainBodyChunk.ContactPoint.y != 0)
			{
				panicFleeCrit = null;
			}
			return;
		}
		AIModule aIModule = base.utilityComparer.HighestUtilityModule();
		currentUtility = base.utilityComparer.HighestUtility();
		if (aIModule != null && cicada.safariControlled)
		{
			currentUtility = 0f;
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
			else if (aIModule is PreyTracker)
			{
				if (base.preyTracker.MostAttractivePrey == null)
				{
					behavior = Behavior.Idle;
				}
				else if (DynamicRelationship(base.preyTracker.MostAttractivePrey).type == CreatureTemplate.Relationship.Type.Antagonizes)
				{
					behavior = Behavior.Antagonize;
				}
				else
				{
					behavior = Behavior.Hunt;
				}
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
		swooshToPos = null;
		if (circleGroup != null && (behavior != Behavior.Idle || circleGroup.room != cicada.room))
		{
			RemoveFromCircle();
		}
		if (cicada.grasps[0] != null && (currentUtility < 0.7f || behavior == Behavior.Hunt))
		{
			if (cicada.grasps[0].grabbed is Creature && StaticRelationship((cicada.grasps[0].grabbed as Creature).abstractCreature).type == CreatureTemplate.Relationship.Type.Eats)
			{
				if (base.denFinder.GetDenPosition().HasValue && (behavior != Behavior.Flee || currentUtility < 0.2f))
				{
					behavior = Behavior.ReturnPrey;
				}
				else if (behavior == Behavior.Hunt)
				{
					behavior = Behavior.Idle;
				}
			}
			else if (UnityEngine.Random.value < 0.025f)
			{
				cicada.LoseAllGrasps();
			}
		}
		base.stuckTracker.satisfiedWithThisPosition = !cicada.AtSitDestination;
		if (behavior == Behavior.Idle)
		{
			bool flag = false;
			if (cicada.gender)
			{
				if (base.denFinder.GetDenPosition().HasValue && base.denFinder.GetDenPosition().Value.room != creature.pos.room)
				{
					creature.abstractAI.SetDestination(base.denFinder.GetDenPosition().Value);
				}
			}
			else if (migrateToSwarmRoom && !cicada.room.abstractRoom.swarmRoom && (base.secondaryNodeFinder as SwarmRoomFinder).SwarmPosition.HasValue)
			{
				creature.abstractAI.SetDestination((base.secondaryNodeFinder as SwarmRoomFinder).SwarmPosition.Value);
				flag = true;
			}
			if (flag)
			{
				return;
			}
			if (circleGroup != null)
			{
				if (circleGroup.group.Count < 2)
				{
					RemoveFromCircle();
					return;
				}
				swooshToPos = circleGroup.MyPos(creature);
				if (circleGroup.group[0] == creature)
				{
					circleGroup.Update();
				}
				if ((circleGroup.slatedForDeletion && UnityEngine.Random.value < 1f / 160f) || cicada.mainBodyChunk.ContactPoint.x != 0 || cicada.mainBodyChunk.ContactPoint.y != 0 || ((float)circleGroup.group.Count * 40f > (float)Math.PI * 2f * circleGroup.radius && UnityEngine.Random.value < 1f / 160f) || UnityEngine.Random.value < 0.002173913f || !VisualContact(swooshToPos.Value, 0f))
				{
					RemoveFromCircle();
				}
				return;
			}
			if (!idleSitSpot.TileDefined || idleSitSpot.room != cicada.room.abstractRoom.index || !cicada.Climbable(idleSitSpot.Tile))
			{
				idleSitSpot = cicada.room.GetWorldCoordinate(new IntVector2(UnityEngine.Random.Range(0, cicada.room.TileWidth), UnityEngine.Random.Range(0, cicada.room.TileHeight)));
			}
			creature.abstractAI.SetDestination(idleSitSpot);
			idleSitCounter--;
			if (idleSitCounter < 1 || cicada.room.aimap.getAItile(idleSitSpot).narrowSpace)
			{
				idleSitCounter = UnityEngine.Random.Range(0, UnityEngine.Random.Range(0, 650));
				forbiddenIdleSitSpot = idleSitSpot;
			}
			if (idleSitSpot == forbiddenIdleSitSpot)
			{
				IntVector2 intVector = new IntVector2(UnityEngine.Random.Range(0, cicada.room.TileWidth), UnityEngine.Random.Range(0, cicada.room.TileHeight));
				if (cicada.Climbable(intVector) && base.pathFinder.CoordinateReachable(cicada.room.GetWorldCoordinate(intVector)) && (UnityEngine.Random.value < 0.3f || VisualContact(cicada.room.MiddleOfTile(intVector), 0f)))
				{
					idleSitSpot = cicada.room.GetWorldCoordinate(intVector);
				}
			}
			if (!(UnityEngine.Random.value < 0.5f) || noCircleGroupCounter != 0 || circleGroup != null || base.tracker.CreaturesCount <= 0 || cicada.room.aimap.getTerrainProximity(cicada.mainBodyChunk.pos) <= 6 || cicada.room.aimap.getAItile(cicada.mainBodyChunk.pos).floorAltitude >= 12 || cicada.room.aimap.getAItile(cicada.mainBodyChunk.pos).floorAltitude <= 6)
			{
				return;
			}
			Tracker.CreatureRepresentation rep = base.tracker.GetRep(UnityEngine.Random.Range(0, base.tracker.CreaturesCount));
			if (rep.representedCreature.creatureTemplate.IsCicada && rep.VisualContact)
			{
				if ((rep.representedCreature.abstractAI.RealAI as CicadaAI).InviteToDance())
				{
					circleGroup = new CircleGroup(cicada.room, creature, cicada.mainBodyChunk.pos);
					(rep.representedCreature.abstractAI.RealAI as CicadaAI).AddToCircle(circleGroup);
				}
				else if ((rep.representedCreature.abstractAI.RealAI as CicadaAI).circleGroup != null)
				{
					AddToCircle((rep.representedCreature.abstractAI.RealAI as CicadaAI).circleGroup);
				}
			}
		}
		else if (behavior == Behavior.Flee)
		{
			WorldCoordinate destination = base.threatTracker.FleeTo(creature.pos, 1, 30, currentUtility > 0.3f);
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
			if (base.preyTracker.MostAttractivePrey.VisualContact && Custom.DistLess(base.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.mainBodyChunk.pos, cicada.mainBodyChunk.pos, 200f) && Custom.InsideRect(base.preyTracker.MostAttractivePrey.BestGuessForPosition().Tile, new IntRect(-30, -30, cicada.room.TileWidth + 30, cicada.room.TileHeight + 30)))
			{
				if (huntAttackCounter < 50)
				{
					huntAttackCounter++;
					swooshToPos = base.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.mainBodyChunk.pos;
					if (Custom.DistLess(cicada.mainBodyChunk.pos, base.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.mainBodyChunk.pos, 50f))
					{
						cicada.TryToGrabPrey(base.preyTracker.MostAttractivePrey.representedCreature.realizedCreature);
					}
				}
				else if (UnityEngine.Random.value < 0.1f)
				{
					huntAttackCounter++;
					if (huntAttackCounter > 200)
					{
						huntAttackCounter = 0;
					}
				}
			}
			tiredOfHuntingCounter++;
			if (tiredOfHuntingCounter > 200)
			{
				tiredOfHuntingCreature = base.preyTracker.MostAttractivePrey.representedCreature;
				tiredOfHuntingCounter = 0;
				base.preyTracker.ForgetPrey(tiredOfHuntingCreature);
				base.tracker.ForgetCreature(tiredOfHuntingCreature);
			}
		}
		else if (behavior == Behavior.Antagonize)
		{
			focusCreature = base.preyTracker.MostAttractivePrey;
			antagonizeMethodCounter++;
			if (antagonizeMethod)
			{
				if ((UnityEngine.Random.value < 0.0016666667f && antagonizeMethodCounter > 800) || (UnityEngine.Random.value < 0.1f && focusCreature.VisualContact && Custom.DistLess(focusCreature.representedCreature.realizedCreature.mainBodyChunk.pos, cicada.mainBodyChunk.pos, 50f) && antagonizeMethodCounter > 120))
				{
					antagonizeMethodCounter = 0;
					antagonizeMethod = false;
				}
			}
			else if (UnityEngine.Random.value < 0.0125f && antagonizeMethodCounter > 80)
			{
				antagonizeMethodCounter = 0;
				antagonizeMethod = true;
			}
			if (focusCreature.VisualContact && antagonizeMethod == cicada.gender)
			{
				Vector2 vector = cicada.room.MiddleOfTile(base.pathFinder.GetDestination);
				Vector2 vector2 = focusCreature.representedCreature.realizedCreature.mainBodyChunk.pos + focusCreature.representedCreature.realizedCreature.mainBodyChunk.vel * 4.5f;
				float dst = float.MaxValue;
				if (Custom.DistLess(vector2, vector, 120f) && VisualContact(vector, 0f))
				{
					dst = Vector2.Distance(cicada.mainBodyChunk.pos, vector);
				}
				for (int i = 0; i < 5; i++)
				{
					Vector2 a = vector2 + Custom.DegToVec(Mathf.Lerp(-75f, 75f, UnityEngine.Random.value)) * 80f;
					a = Vector2.Lerp(a, vector2 + Custom.DirVec(vector2, cicada.mainBodyChunk.pos) * 80f, 0.8f * Mathf.InverseLerp(400f, 30f, Vector2.Distance(a, cicada.mainBodyChunk.pos)));
					for (int j = 0; j < base.tracker.CreaturesCount; j++)
					{
						if (base.tracker.GetRep(j).BestGuessForPosition().room == creature.pos.room && base.tracker.GetRep(j).representedCreature.creatureTemplate.IsCicada && Custom.DistLess(a, cicada.room.MiddleOfTile(base.tracker.GetRep(j).BestGuessForPosition()), 100f))
						{
							a += Custom.DirVec(cicada.room.MiddleOfTile(base.tracker.GetRep(j).BestGuessForPosition()), a) * 50f;
						}
					}
					if (Custom.DistLess(cicada.mainBodyChunk.pos, a, dst) && cicada.room.VisualContact(vector2, a))
					{
						vector = a;
						dst = Vector2.Distance(cicada.mainBodyChunk.pos, a);
					}
				}
				creature.abstractAI.SetDestination(cicada.room.GetWorldCoordinate(vector));
				if (!cicada.Charging && UnityEngine.Random.value < 1f / 30f && Custom.DistLess(cicada.mainBodyChunk.pos, vector, 40f) && Custom.DistLess(vector2, vector, 120f) && cicada.room.aimap.getTerrainProximity(cicada.mainBodyChunk.pos) > 1 && VisualContact(vector, 0f) && cicada.room.VisualContact(vector, vector2))
				{
					cicada.Charge(vector2);
				}
			}
			else
			{
				creature.abstractAI.SetDestination(focusCreature.BestGuessForPosition());
			}
			if (cicada.flying && focusCreature.representedCreature.creatureTemplate.type == CreatureTemplate.Type.Slugcat && focusCreature.age > 100 && VisualContact(focusCreature.representedCreature.realizedCreature.mainBodyChunk))
			{
				cicada.cantPickUpCounter = 5;
				cicada.cantPickUpPlayer = focusCreature.representedCreature.realizedCreature as Player;
			}
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

	public bool InviteToDance()
	{
		if (noCircleGroupCounter == 0 && behavior == Behavior.Idle)
		{
			return circleGroup == null;
		}
		return false;
	}

	public void AddToCircle(CircleGroup newGroup)
	{
		newGroup.group.Add(creature);
		circleGroup = newGroup;
	}

	public void RemoveFromCircle()
	{
		if (circleGroup != null)
		{
			circleGroup.RemoveCicada(creature);
			noCircleGroupCounter = UnityEngine.Random.Range(10, 140);
			circleGroup = null;
		}
	}

	public override float VisualScore(Vector2 lookAtPoint, float targetSpeed)
	{
		return base.VisualScore(lookAtPoint, targetSpeed) - Mathf.InverseLerp(1f, -0.3f, Vector2.Dot((cicada.bodyChunks[1].pos - cicada.bodyChunks[0].pos).normalized, (cicada.bodyChunks[1].pos - lookAtPoint).normalized));
	}

	public override bool WantToStayInDenUntilEndOfCycle()
	{
		return base.rainTracker.Utility() > 0.01f;
	}

	public override void CreatureSpotted(bool firstSpot, Tracker.CreatureRepresentation creatureRep)
	{
		if (cicada.graphicsModule != null)
		{
			(cicada.graphicsModule as CicadaGraphics).creatureLooker.ReevaluateLookObject(creatureRep, 2f);
		}
	}

	public override Tracker.CreatureRepresentation CreateTrackerRepresentationForCreature(AbstractCreature otherCreature)
	{
		Tracker.CreatureRepresentation creatureRepresentation = ((!otherCreature.creatureTemplate.smallCreature) ? ((Tracker.CreatureRepresentation)new Tracker.ElaborateCreatureRepresentation(base.tracker, otherCreature, 1f, 3)) : ((Tracker.CreatureRepresentation)new Tracker.SimpleCreatureRepresentation(base.tracker, otherCreature, 0f, forgetWhenNotVisible: false)));
		if (cicada.graphicsModule != null)
		{
			(cicada.graphicsModule as CicadaGraphics).creatureLooker.ReevaluateLookObject(creatureRepresentation, 2f);
		}
		return creatureRepresentation;
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
		return new CicadaTrackState();
	}

	CreatureTemplate.Relationship IUseARelationshipTracker.UpdateDynamicRelationship(RelationshipTracker.DynamicRelationship dRelation)
	{
		if (dRelation.trackerRep.VisualContact && dRelation.state != null)
		{
			(dRelation.state as CicadaTrackState).caught = false;
			(dRelation.state as CicadaTrackState).gotACicada = false;
			(dRelation.state as CicadaTrackState).gotCicadaFood = false;
			(dRelation.state as CicadaTrackState).armed = false;
			foreach (AbstractPhysicalObject.AbstractObjectStick stuckObject in dRelation.trackerRep.representedCreature.stuckObjects)
			{
				if (!(stuckObject is AbstractPhysicalObject.CreatureGripStick))
				{
					continue;
				}
				if (stuckObject.A == dRelation.trackerRep.representedCreature)
				{
					if (stuckObject.B is AbstractCreature && (stuckObject.B as AbstractCreature).creatureTemplate.IsCicada)
					{
						(dRelation.state as CicadaTrackState).gotACicada = true;
					}
					else if (stuckObject.B is AbstractCreature && DynamicRelationship(stuckObject.B as AbstractCreature).type == CreatureTemplate.Relationship.Type.Eats)
					{
						(dRelation.state as CicadaTrackState).gotCicadaFood = true;
					}
					else if (stuckObject.B.type == AbstractPhysicalObject.AbstractObjectType.Spear)
					{
						(dRelation.state as CicadaTrackState).armed = true;
					}
				}
				else if (stuckObject.B == dRelation.trackerRep.representedCreature && (stuckObject as AbstractPhysicalObject.CreatureGripStick).carry)
				{
					(dRelation.state as CicadaTrackState).caught = true;
				}
			}
		}
		if (base.friendTracker.giftOfferedToMe != null && base.friendTracker.giftOfferedToMe.active && base.friendTracker.giftOfferedToMe.item == dRelation.trackerRep.representedCreature.realizedCreature)
		{
			return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, dRelation.trackerRep.representedCreature.state.dead ? 1f : 0.65f);
		}
		CreatureTemplate.Relationship relationship = StaticRelationship(dRelation.trackerRep.representedCreature);
		bool flag = ModManager.MSC && dRelation.trackerRep.representedCreature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC;
		if (dRelation.trackerRep.representedCreature.creatureTemplate.type == CreatureTemplate.Type.Slugcat || flag)
		{
			float num = LikeOfPlayer(dRelation.trackerRep);
			if ((dRelation.state as CicadaTrackState).gotACicada)
			{
				num -= Mathf.InverseLerp(0.9f, 0.1f, num) * 0.7f;
			}
			num -= ContextualDislikeOfRival(dRelation, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.AgressiveRival, cicada.gender ? 0.6f : 0.9f)) * Mathf.InverseLerp(0.9f, 0.1f, num);
			if (num < -0.1f)
			{
				if (num < -0.5f && creature.personality.bravery < 0.5f && dRelation.state != null && (dRelation.state as CicadaTrackState).armed)
				{
					return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, Mathf.InverseLerp(-0.5f, -1f, num));
				}
				return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Antagonizes, Mathf.InverseLerp(-0.1f, -1f, num));
			}
			if (num > 0.5f)
			{
				return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f);
			}
			return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.StayOutOfWay, Custom.LerpMap(num, 0.5f, -0.1f, 0f, 0.8f, 8f));
		}
		if ((dRelation.state as CicadaTrackState).gotACicada)
		{
			return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Antagonizes, 0.7f);
		}
		if (relationship.type == CreatureTemplate.Relationship.Type.AgressiveRival)
		{
			float num2 = ContextualDislikeOfRival(dRelation, relationship);
			if (num2 > 0f)
			{
				return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Antagonizes, num2);
			}
			return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.StayOutOfWay, 0.2f);
		}
		if (relationship.type == CreatureTemplate.Relationship.Type.Eats)
		{
			if ((dRelation.state as CicadaTrackState).caught)
			{
				return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0f);
			}
			return relationship;
		}
		return relationship;
	}

	private float ContextualDislikeOfRival(RelationshipTracker.DynamicRelationship dRelation, CreatureTemplate.Relationship rel)
	{
		if (cicada.grasps[0] == null && dRelation.state != null && (dRelation.state as CicadaTrackState).gotCicadaFood)
		{
			return (1f + rel.intensity) / 2f;
		}
		float num = 0f;
		num = ((!dRelation.trackerRep.VisualContact) ? (Mathf.InverseLerp(Mathf.Lerp(80f, 180f, rel.intensity), 40f, Vector2.Distance(cicada.mainBodyChunk.pos, cicada.room.MiddleOfTile(dRelation.trackerRep.BestGuessForPosition()))) * 0.5f) : Mathf.InverseLerp(Mathf.Lerp(100f, 300f, rel.intensity), 40f, Vector2.Distance(cicada.mainBodyChunk.pos, dRelation.trackerRep.representedCreature.realizedCreature.mainBodyChunk.pos)));
		return num * rel.intensity;
	}

	public float LikeOfPlayer(Tracker.CreatureRepresentation player)
	{
		if (player == null)
		{
			return 0f;
		}
		float a = creature.world.game.session.creatureCommunities.LikeOfPlayer(creature.creatureTemplate.communityID, creature.world.RegionNumber, (player.representedCreature.state as PlayerState).playerNumber);
		float tempLike = creature.state.socialMemory.GetTempLike(player.representedCreature.ID);
		a = Mathf.Lerp(a, tempLike, Mathf.Abs(tempLike));
		if (base.friendTracker.giftOfferedToMe != null && base.friendTracker.giftOfferedToMe.owner == player.representedCreature.realizedCreature)
		{
			a = Custom.LerpMap(a, -0.5f, 1f, 0f, 1f, 0.8f);
		}
		return a;
	}

	public override float CurrentPlayerAggression(AbstractCreature player)
	{
		Tracker.CreatureRepresentation creatureRepresentation = base.tracker.RepresentationForCreature(player, addIfMissing: false);
		if (creatureRepresentation == null || creatureRepresentation.dynamicRelationship == null)
		{
			return 1f;
		}
		return Mathf.InverseLerp(0.5f, 0f, LikeOfPlayer(creatureRepresentation));
	}

	public void SocialEvent(SocialEventRecognizer.EventID ID, Creature subjectCrit, Creature objectCrit, PhysicalObject involvedItem)
	{
		if (!(subjectCrit is Player))
		{
			return;
		}
		Tracker.CreatureRepresentation creatureRepresentation = base.tracker.RepresentationForObject(subjectCrit, AddIfMissing: false);
		if (creatureRepresentation == null)
		{
			return;
		}
		Tracker.CreatureRepresentation creatureRepresentation2 = null;
		bool flag = objectCrit == cicada;
		if (!flag)
		{
			creatureRepresentation2 = base.tracker.RepresentationForObject(objectCrit, AddIfMissing: false);
			if (creatureRepresentation2 == null)
			{
				return;
			}
		}
		if ((!flag && cicada.dead) || (creatureRepresentation2 != null && creatureRepresentation.TicksSinceSeen > 40 && creatureRepresentation2.TicksSinceSeen > 40))
		{
			return;
		}
		if (ID == SocialEventRecognizer.EventID.ItemOffering)
		{
			if (flag)
			{
				base.friendTracker.ItemOffered(creatureRepresentation, involvedItem);
			}
			return;
		}
		float num = 0f;
		if (ID == SocialEventRecognizer.EventID.NonLethalAttack)
		{
			num = 0.2f;
		}
		else if (ID == SocialEventRecognizer.EventID.LethalAttackAttempt)
		{
			num = 0.6f;
		}
		else if (ID == SocialEventRecognizer.EventID.LethalAttack)
		{
			num = 0.7f;
		}
		else if (ID == SocialEventRecognizer.EventID.Killing)
		{
			num = 1f;
		}
		if (num == 0f)
		{
			return;
		}
		if (objectCrit.dead)
		{
			num /= 3f;
		}
		if (flag)
		{
			CicadaPlayerRelationChange(0f - num, subjectCrit.abstractCreature);
		}
		else if (creatureRepresentation2.dynamicRelationship.currentRelationship.type == CreatureTemplate.Relationship.Type.Afraid)
		{
			float num2 = 0.1f;
			if (base.threatTracker.GetThreatCreature(objectCrit.abstractCreature) != null)
			{
				num2 += 0.7f * Custom.LerpMap(Vector2.Distance(cicada.mainBodyChunk.pos, objectCrit.DangerPos), 120f, 320f, 1f, 0.1f);
			}
			bool flag2 = false;
			for (int i = 0; i < objectCrit.grasps.Length; i++)
			{
				if (flag2)
				{
					break;
				}
				if (objectCrit.grasps[i] != null && objectCrit.grasps[i].grabbed == cicada)
				{
					flag2 = true;
				}
			}
			if (flag2)
			{
				if (ID == SocialEventRecognizer.EventID.NonLethalAttack || ID == SocialEventRecognizer.EventID.LethalAttack)
				{
					num = 1f;
				}
				num2 = 2f;
			}
			CicadaPlayerRelationChange(Mathf.Pow(num, 0.5f) * num2, subjectCrit.abstractCreature);
		}
		else if (creatureRepresentation2.dynamicRelationship.currentRelationship.type == CreatureTemplate.Relationship.Type.Pack)
		{
			CicadaPlayerRelationChange((0f - num) * 0.75f, subjectCrit.abstractCreature);
		}
	}

	private void CicadaPlayerRelationChange(float change, AbstractCreature player)
	{
		SocialMemory.Relationship orInitiateRelationship = creature.state.socialMemory.GetOrInitiateRelationship(player.ID);
		orInitiateRelationship.InfluenceTempLike(change * 1.5f);
		orInitiateRelationship.InfluenceLike(change * 0.75f);
		orInitiateRelationship.InfluenceKnow(Mathf.Abs(change) * 0.25f);
		creature.world.game.session.creatureCommunities.InfluenceLikeOfPlayer(creature.creatureTemplate.communityID, creature.world.RegionNumber, (player.state as PlayerState).playerNumber, change * 0.15f, 0.1f, 0.1f);
	}

	public void GiftRecieved(SocialEventRecognizer.OwnedItemOnGround gift)
	{
		Custom.Log("cicada recieve gift");
		CicadaPlayerRelationChange(0.5f, gift.owner.abstractCreature);
	}
}
