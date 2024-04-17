using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class YeekAI : ArtificialIntelligence, IUseARelationshipTracker
{
	public class Behavior : ExtEnum<Behavior>
	{
		public static readonly Behavior Idle = new Behavior("Idle", register: true);

		public static readonly Behavior GetUnstuck = new Behavior("GetUnstuck", register: true);

		public static readonly Behavior EscapeRain = new Behavior("EscapeRain", register: true);

		public static readonly Behavior Fear = new Behavior("Fear", register: true);

		public static readonly Behavior Hungry = new Behavior("Hungry", register: true);

		public static readonly Behavior ReturnFood = new Behavior("ReturnFood", register: true);

		public Behavior(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public Tracker.CreatureRepresentation focusCreature;

	private Yeek yeek;

	public Behavior behavior;

	public WorldCoordinate CurrentIdlePos;

	private int idleBordomCountdown;

	private List<AbstractCreature> yeekSquad;

	public float fearCounter;

	private List<AbstractConsumable> fruitInRoom;

	public AbstractConsumable goalFruit;

	private int roomInterest;

	private Room rescanRoom;

	public YeekAI(AbstractCreature abstractCreature, World world)
		: base(abstractCreature, world)
	{
		yeek = abstractCreature.realizedCreature as Yeek;
		yeek.AI = this;
		AddModule(new StandardPather(this, world, abstractCreature));
		base.pathFinder.stepsPerFrame = 20;
		AddModule(new Tracker(this, 10, 10, 250, 0.5f, 5, 5, 20));
		AddModule(new ThreatTracker(this, 3));
		AddModule(new RainTracker(this));
		AddModule(new DenFinder(this, abstractCreature));
		AddModule(new StuckTracker(this, trackPastPositions: true, trackNotFollowingCurrentGoal: true));
		AddModule(new RelationshipTracker(this, base.tracker));
		base.stuckTracker.AddSubModule(new StuckTracker.GetUnstuckPosCalculator(base.stuckTracker));
		AddModule(new UtilityComparer(this));
		base.utilityComparer.AddComparedModule(base.threatTracker, null, 1f, 1.1f);
		base.utilityComparer.AddComparedModule(base.preyTracker, null, 0.5f, 1.1f);
		base.utilityComparer.AddComparedModule(base.rainTracker, null, 1f, 1.1f);
		base.utilityComparer.AddComparedModule(base.stuckTracker, null, 1f, 1.1f);
		yeekSquad = new List<AbstractCreature>();
		fruitInRoom = new List<AbstractConsumable>();
		roomInterest = Random.Range(2, 7);
	}

	public override void Update()
	{
		base.Update();
		if (rescanRoom != null)
		{
			ScanFruitInRoom(rescanRoom);
			rescanRoom = null;
		}
		if (yeek.LickedByPlayer != null)
		{
			base.tracker.SeeCreature(yeek.LickedByPlayer.abstractCreature);
		}
		if (yeek.abstractCreature.abstractAI.destination.room == yeek.room.abstractRoom.index)
		{
			idleBordomCountdown--;
		}
		if (idleBordomCountdown <= 0 || FeelsAlone())
		{
			getNewIdlePos(20);
		}
		behavior = Behavior.Idle;
		if ((yeek.abstractCreature.state as YeekState).HungerIntensity(yeek.room.world.rainCycle.timer) > 0.8f && fruitInRoom.Count > 0)
		{
			behavior = Behavior.Hungry;
		}
		if (goalFruit != null && yeek.grasps[0] != null && yeek.grasps[0].grabbed.abstractPhysicalObject == goalFruit)
		{
			behavior = Behavior.ReturnFood;
			fearCounter = Mathf.Lerp(fearCounter, 0.3f, 0.1f);
			if (base.denFinder.status == NodeFinder.Status.NoAccessible)
			{
				behavior = Behavior.Idle;
			}
		}
		AIModule aIModule = base.utilityComparer.HighestUtilityModule();
		float num = base.utilityComparer.HighestUtility();
		if (num > 0.01f && aIModule != null)
		{
			if (aIModule is RainTracker)
			{
				behavior = Behavior.EscapeRain;
			}
			if (aIModule is ThreatTracker)
			{
				behavior = Behavior.Fear;
			}
			if (aIModule is StuckTracker)
			{
				behavior = Behavior.GetUnstuck;
			}
		}
		if (behavior != Behavior.Hungry)
		{
			if (behavior == Behavior.Idle || (behavior == Behavior.Fear && num <= 0.2f))
			{
				if (behavior == Behavior.Fear)
				{
					fearCounter += 0.0001f;
					if (fearCounter > 1f)
					{
						fearCounter = 1f;
						if (Random.value < 0.5f)
						{
							yeek.YeekCall();
							MakeCreatureLeaveRoom();
						}
					}
					focusCreature = base.threatTracker.mostThreateningCreature;
				}
				else
				{
					fearCounter -= 0.005f;
					if (fearCounter < 0f)
					{
						fearCounter = 0f;
					}
				}
				if (CurrentIdlePos != yeek.abstractCreature.abstractAI.destination && yeek.abstractCreature.abstractAI.destination.room != yeek.room.abstractRoom.index)
				{
					CurrentIdlePos = yeek.abstractCreature.abstractAI.destination;
				}
				if (CurrentIdlePos != yeek.abstractCreature.abstractAI.destination)
				{
					creature.abstractAI.SetDestination(CurrentIdlePos);
				}
			}
			else if (behavior == Behavior.Fear)
			{
				fearCounter += 0.005f;
				if (fearCounter > 1f)
				{
					fearCounter = 1f;
				}
				idleBordomCountdown = 40;
				focusCreature = null;
				if (yeek.OnGround || yeek.GetClimbingMode)
				{
					creature.abstractAI.SetDestination(base.threatTracker.FleeTo(creature.pos, 3, 90, considerLeavingRoom: true));
				}
				if (base.threatTracker.mostThreateningCreature != null && base.threatTracker.mostThreateningCreature.BestGuessForPosition().room == yeek.room.abstractRoom.index && Vector2.Distance(yeek.mainBodyChunk.pos, yeek.room.MiddleOfTile(base.threatTracker.mostThreateningCreature.BestGuessForPosition().Tile)) < 80f && Random.value < 0.6f)
				{
					Vector2 vector = Custom.DirVec(default(Vector2), yeek.mainBodyChunk.vel);
					vector.x *= 10f;
					vector.y *= 25f;
					yeek.Hop(yeek.mainBodyChunk.pos, yeek.mainBodyChunk.pos + vector);
				}
			}
			else if (behavior == Behavior.EscapeRain)
			{
				fearCounter += 0.005f;
				if (fearCounter > 1f)
				{
					fearCounter = 1f;
				}
				focusCreature = null;
				idleBordomCountdown = 40;
				if (base.denFinder.GetDenPosition().HasValue)
				{
					creature.abstractAI.SetDestination(base.denFinder.GetDenPosition().Value);
				}
			}
			else if (behavior == Behavior.ReturnFood)
			{
				idleBordomCountdown = 40;
				if (base.denFinder.GetDenPosition().HasValue)
				{
					creature.abstractAI.SetDestination(base.denFinder.GetDenPosition().Value);
				}
			}
			else if (behavior == Behavior.GetUnstuck)
			{
				idleBordomCountdown = 40;
				creature.abstractAI.SetDestination(base.stuckTracker.getUnstuckPosCalculator.unstuckGoalPosition);
				yeek.Hop(yeek.firstChunk.pos, base.stuckTracker.getUnstuckPosCalculator.unstuckGoalPosition.Tile.ToVector2() * 20f + new Vector2(Random.Range(-10f, 10f), 60f), 24f);
			}
			return;
		}
		if (goalFruit == null)
		{
			UpdateFruitInRoom();
			if (fruitInRoom.Count > 0)
			{
				goalFruit = fruitInRoom[Random.Range(0, fruitInRoom.Count)];
			}
			CurrentIdlePos = new WorldCoordinate(yeek.room.abstractRoom.index, 0, 0, -1);
		}
		if (!FruitIsValid(goalFruit))
		{
			CancelWantToEat();
		}
		else
		{
			if (!(CurrentIdlePos != yeek.abstractCreature.abstractAI.destination))
			{
				return;
			}
			if (base.pathFinder.CoordinateReachableAndGetbackable(goalFruit.pos))
			{
				CurrentIdlePos = goalFruit.pos;
				creature.abstractAI.SetDestination(CurrentIdlePos);
				return;
			}
			WorldCoordinate pos = goalFruit.pos;
			List<IntVector2> path = new List<IntVector2>();
			yeek.room.RayTraceTilesList(pos.x, pos.y, pos.x, Mathf.Clamp(pos.y - 20, 0, yeek.room.TileHeight), ref path);
			IntVector2 intVector = goalFruit.pos.Tile;
			foreach (IntVector2 item in path)
			{
				if (yeek.room.GetTile(item).Solid)
				{
					intVector = item;
					intVector.y++;
					break;
				}
			}
			pos = new WorldCoordinate(yeek.room.abstractRoom.index, intVector.x, intVector.y, -1);
			if (base.pathFinder.CoordinateReachableAndGetbackable(pos))
			{
				CurrentIdlePos = pos;
				creature.abstractAI.SetDestination(CurrentIdlePos);
			}
			else
			{
				CancelWantToEat();
			}
		}
	}

	public float IdlePosScore(IntVector2 pos)
	{
		WorldCoordinate coord = new WorldCoordinate(yeek.room.abstractRoom.index, pos.x, pos.y, -1);
		float num = 1000f;
		List<AbstractCreature> yeekSquadInRoom = GetYeekSquadInRoom();
		if (CurrentIdlePos.room == coord.room)
		{
			num -= 800f * Mathf.InverseLerp(20f, 0f, Vector2.Distance(CurrentIdlePos.Tile.ToVector2(), coord.Tile.ToVector2()));
		}
		if (yeekSquadInRoom.Count > 0)
		{
			foreach (AbstractCreature item in yeekSquadInRoom)
			{
				if (item.realizedCreature != null)
				{
					float groupLeaderPotential = (item.realizedCreature as Yeek).GroupLeaderPotential;
					num += 150f * Mathf.InverseLerp(12f + 80f * groupLeaderPotential, 4f, Vector2.Distance(item.pos.Tile.ToVector2(), coord.Tile.ToVector2()));
				}
				else
				{
					num += 1f;
				}
			}
		}
		if (base.pathFinder.CoordinateReachable(coord) && !base.pathFinder.CoordinateReachableAndGetbackable(coord))
		{
			num *= 0.8f;
		}
		if (base.threatTracker.mostThreateningCreature != null && base.threatTracker.mostThreateningCreature.BestGuessForPosition().room == coord.room)
		{
			num /= Mathf.InverseLerp(400f, 100f, Vector2.Distance(base.threatTracker.mostThreateningCreature.BestGuessForPosition().Tile.ToVector2(), coord.Tile.ToVector2()));
		}
		if (yeek.room.GetTile(pos).Solid || !yeek.room.GetTile(new IntVector2(pos.x, pos.y - 1)).Solid || yeek.room.GetTile(pos).wormGrass)
		{
			num = 0f;
		}
		return num;
	}

	public override void NewRoom(Room room)
	{
		base.NewRoom(room);
		rescanRoom = room;
		roomInterest = Random.Range(3, 7);
		idleBordomCountdown = 0;
		CurrentIdlePos = yeek.abstractCreature.pos;
		yeekSquad.Clear();
		GetYeekSquadInRoom();
	}

	public void getNewIdlePos(int chances)
	{
		if (roomInterest < 0 || (roomInterest < 2 && yeekSquad.Count < 2))
		{
			MakeCreatureLeaveRoom();
			return;
		}
		roomInterest--;
		WorldCoordinate currentIdlePos = CurrentIdlePos;
		float num = 0f;
		for (int i = 0; i < chances; i++)
		{
			IntVector2 pos = yeek.room.RandomTile();
			float num2 = IdlePosScore(pos);
			if (num2 > num)
			{
				currentIdlePos = new WorldCoordinate(yeek.room.abstractRoom.index, pos.x, pos.y, -1);
				num = num2;
				if (num2 == 1000f)
				{
					break;
				}
			}
		}
		idleBordomCountdown = (int)(Random.Range(400f, 900f) + Mathf.Lerp(800f, 300f, yeek.abstractCreature.personality.energy));
		CurrentIdlePos = currentIdlePos;
	}

	public override float VisualScore(Vector2 lookAtPoint, float targetSpeed)
	{
		return base.VisualScore(lookAtPoint, targetSpeed) * Mathf.InverseLerp(500f, 190f, Vector2.Distance(yeek.VisionPoint, lookAtPoint));
	}

	AIModule IUseARelationshipTracker.ModuleToTrackRelationship(CreatureTemplate.Relationship relationship)
	{
		if (relationship.type != CreatureTemplate.Relationship.Type.Afraid)
		{
			return null;
		}
		return base.threatTracker;
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
		}
		CreatureTemplate.Relationship result = StaticRelationship(dRelation.trackerRep.representedCreature);
		if (result.type == CreatureTemplate.Relationship.Type.Afraid)
		{
			if (dRelation.trackerRep != null && dRelation.trackerRep.representedCreature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.Yeek)
			{
				result.type = CreatureTemplate.Relationship.Type.Pack;
				result.intensity += 1f;
				if (!yeekSquad.Contains(dRelation.trackerRep.representedCreature))
				{
					yeekSquad.Add(dRelation.trackerRep.representedCreature);
				}
				return result;
			}
			if (!dRelation.state.alive)
			{
				result.intensity = 0f;
			}
			else if (dRelation.trackerRep.BestGuessForPosition().room == yeek.room.abstractRoom.index && !dRelation.trackerRep.representedCreature.creatureTemplate.canFly)
			{
				if (dRelation.trackerRep.VisualContact && ((fearCounter < 0.4f && Vector2.Distance(yeek.DangerPos, yeek.room.MiddleOfTile(dRelation.trackerRep.representedCreature.pos)) < 300f) || Vector2.Distance(yeek.DangerPos, yeek.room.MiddleOfTile(dRelation.trackerRep.representedCreature.pos)) < 30f))
				{
					result = SurprisedFearReaction(dRelation);
				}
				else
				{
					float a = Mathf.Lerp(0f, 1.2f, Mathf.InverseLerp(-100f, 500f, yeek.room.MiddleOfTile(dRelation.trackerRep.BestGuessForPosition().Tile).y - yeek.mainBodyChunk.pos.y));
					float value = float.MaxValue;
					a = Mathf.Lerp(a, 1f, Mathf.InverseLerp(50f, 500f, value));
					result.intensity *= a;
				}
			}
		}
		return result;
	}

	private List<AbstractCreature> GetYeekSquadInRoom()
	{
		List<AbstractCreature> list = new List<AbstractCreature>();
		for (int i = 0; i < yeekSquad.Count; i++)
		{
			if (yeekSquad[i].Room == yeek.room.abstractRoom && !yeekSquad[i].InDen && !yeekSquad[i].state.dead)
			{
				list.Add(yeekSquad[i]);
			}
		}
		yeekSquad = list;
		return list;
	}

	private bool FeelsAlone()
	{
		if (yeekSquad.Count > 4 && Random.value < 0.001f)
		{
			bool flag = false;
			foreach (AbstractCreature item in yeekSquad)
			{
				if (item.realizedCreature != null && Vector2.Distance(yeek.firstChunk.pos, item.realizedCreature.DangerPos) < 100f)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				yeek.YeekCall();
			}
			return !flag;
		}
		return false;
	}

	private void ScanFruitInRoom(Room room)
	{
		fruitInRoom.Clear();
		foreach (PhysicalObject item2 in room.physicalObjects[1])
		{
			if (item2.abstractPhysicalObject is AbstractConsumable)
			{
				AbstractConsumable item = item2.abstractPhysicalObject as AbstractConsumable;
				fruitInRoom.Add(item);
			}
		}
	}

	public List<AbstractConsumable> UpdateFruitInRoom()
	{
		List<AbstractConsumable> list = new List<AbstractConsumable>();
		for (int i = 0; i < fruitInRoom.Count; i++)
		{
			if (FruitIsValid(fruitInRoom[i]))
			{
				list.Add(fruitInRoom[i]);
			}
		}
		fruitInRoom = list;
		return list;
	}

	private bool FruitIsValid(AbstractConsumable fruit)
	{
		if (fruit != null && fruit.Room == yeek.abstractCreature.Room && fruit.realizedObject != null)
		{
			if (fruit.realizedObject.grabbedBy.Count != 0)
			{
				return fruit.realizedObject.grabbedBy[0].grabber == yeek;
			}
			return true;
		}
		return false;
	}

	private void CancelWantToEat()
	{
		goalFruit = null;
		(yeek.abstractCreature.state as YeekState).Feed(yeek.room.world.rainCycle.timer);
		behavior = Behavior.Fear;
		fearCounter = 1f;
	}

	public override Tracker.CreatureRepresentation CreateTrackerRepresentationForCreature(AbstractCreature otherCreature)
	{
		if (otherCreature.creatureTemplate.smallCreature)
		{
			return new Tracker.SimpleCreatureRepresentation(base.tracker, otherCreature, 0f, forgetWhenNotVisible: false);
		}
		return new Tracker.ElaborateCreatureRepresentation(base.tracker, otherCreature, 1f, 3);
	}

	private CreatureTemplate.Relationship SurprisedFearReaction(RelationshipTracker.DynamicRelationship dRelation)
	{
		fearCounter = 1f;
		dRelation.currentRelationship.intensity = 1f;
		foreach (AbstractCreature item in yeekSquad)
		{
			if (item.realizedCreature != null)
			{
				Yeek yeek = item.realizedCreature as Yeek;
				yeek.AI.fearCounter = 1f;
				yeek.AI.CreateTrackerRepresentationForCreature(dRelation.trackerRep.representedCreature);
				yeek.AI.threatTracker.mostThreateningCreature = dRelation.trackerRep;
				yeek.yeekCallCounter = 0f;
				MakeCreatureLeaveRoom();
				this.yeek.YeekCall();
				if (this.yeek.graphicsModule != null)
				{
					(this.yeek.graphicsModule as YeekGraphics).eyeShudder = 3f * this.yeek.abstractCreature.personality.nervous;
				}
				Vector2 vector = Custom.DirVec(yeek.mainBodyChunk.pos, dRelation.trackerRep.representedCreature.realizedCreature.DangerPos);
				vector.x *= -1f;
				vector.y = 1f;
				yeek.Jump(yeek.mainBodyChunk.pos, yeek.mainBodyChunk.pos + vector * 600f);
			}
		}
		return dRelation.currentRelationship;
	}

	public void MakeCreatureLeaveRoom()
	{
		if (yeek.abstractCreature.abstractAI.destination.room != yeek.room.abstractRoom.index)
		{
			return;
		}
		int num = yeek.AI.threatTracker.FindMostAttractiveExit();
		if (num > -1 && num < yeek.room.abstractRoom.nodes.Length && yeek.room.abstractRoom.nodes[num].type == AbstractRoomNode.Type.Exit)
		{
			int num2 = yeek.room.world.GetAbstractRoom(yeek.room.abstractRoom.connections[num]).ExitIndex(yeek.room.abstractRoom.index);
			if (num2 > -1)
			{
				yeek.AI.creature.abstractAI.MigrateTo(new WorldCoordinate(yeek.room.abstractRoom.connections[num], -1, -1, num2));
			}
		}
	}

	public override PathCost TravelPreference(MovementConnection coord, PathCost cost)
	{
		if (behavior != Behavior.Fear)
		{
			return cost;
		}
		return new PathCost(cost.resistance + base.threatTracker.ThreatOfTile(coord.destinationCoord, accountThreatCreatureAccessibility: true) * 100f, cost.legality);
	}
}
