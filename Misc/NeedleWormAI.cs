using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class NeedleWormAI : ArtificialIntelligence
{
	public class Behavior : ExtEnum<Behavior>
	{
		public static readonly Behavior Idle = new Behavior("Idle", register: true);

		public static readonly Behavior Flee = new Behavior("Flee", register: true);

		public static readonly Behavior Hunt = new Behavior("Hunt", register: true);

		public static readonly Behavior Attack = new Behavior("Attack", register: true);

		public static readonly Behavior FauxAttack = new Behavior("FauxAttack", register: true);

		public static readonly Behavior GetUnstuck = new Behavior("GetUnstuck", register: true);

		public static readonly Behavior EscapeRain = new Behavior("EscapeRain", register: true);

		public static readonly Behavior Migrate = new Behavior("Migrate", register: true);

		public Behavior(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class NeedleWormTrackState : RelationshipTracker.TrackedCreatureState
	{
		public bool holdingChild;

		public bool close;
	}

	public NeedleWorm worm;

	public DebugDestinationVisualizer debugDestinationVisualizer;

	public Behavior behavior;

	public Tracker.CreatureRepresentation focusCreature;

	public WorldCoordinate idlePos;

	public WorldCoordinate lastAssignedIdlePos;

	public int idleCounter;

	public List<WorldCoordinate> oldIdlePositions;

	private int addOldIdlePosDelay;

	public float flySpeed;

	public new int lastRoom = -1;

	public int inFreeSpaceCounter;

	public int inRoomCounter;

	private float flyHeightAdd;

	public NeedleWormAI(AbstractCreature creature, World world)
		: base(creature, world)
	{
		worm = creature.realizedCreature as NeedleWorm;
		worm.AI = this;
		AddModule(new StandardPather(this, world, creature));
		base.pathFinder.stepsPerFrame = ((this is BigNeedleWormAI) ? 20 : 15);
		(base.pathFinder as StandardPather).heuristicCostFac = 1f;
		(base.pathFinder as StandardPather).heuristicDestFac = 2f;
		base.pathFinder.accessibilityStepsPerFrame = 60;
		AddModule(new Tracker(this, 5, 10, 450, 0.5f, 5, 5, 10));
		AddModule(new ThreatTracker(this, 3));
		AddModule(new RainTracker(this));
		AddModule(new DenFinder(this, creature));
		AddModule(new UtilityComparer(this));
		AddModule(new RelationshipTracker(this, base.tracker));
		base.utilityComparer.AddComparedModule(base.threatTracker, null, 1f, 1.1f);
		base.utilityComparer.AddComparedModule(base.preyTracker, null, 0.5f, 1.1f);
		base.utilityComparer.AddComparedModule(base.rainTracker, null, 1f, 1.1f);
		worm.creatureLooker = new CreatureLooker(worm, base.tracker, worm, 0.02f, 30);
		behavior = Behavior.Idle;
		inFreeSpaceCounter = 100;
		oldIdlePositions = new List<WorldCoordinate>();
	}

	public bool LikeRoom()
	{
		if (creature.Room.AttractionForCreature(creature.creatureTemplate.type) == AbstractRoom.CreatureRoomAttraction.Avoid || creature.Room.AttractionForCreature(creature.creatureTemplate.type) == AbstractRoom.CreatureRoomAttraction.Forbidden)
		{
			return false;
		}
		if (worm.State.confirmedNarrowRooms.Contains(creature.pos.room))
		{
			return false;
		}
		return true;
	}

	public bool MigrationBehaviorRoll()
	{
		if (creature.abstractAI.followCreature != null)
		{
			return true;
		}
		if (Random.value < Mathf.InverseLerp(1900f, 3200f, inRoomCounter) / ((behavior == Behavior.Idle) ? 6f : 30f))
		{
			return true;
		}
		if (!LikeRoom())
		{
			return true;
		}
		return false;
	}

	public override void NewRoom(Room room)
	{
		base.NewRoom(room);
		if (room.abstractRoom.index != lastRoom)
		{
			lastRoom = room.abstractRoom.index;
			inRoomCounter = 0;
			oldIdlePositions.Clear();
			idlePos = creature.pos;
			lastAssignedIdlePos = creature.pos;
		}
	}

	public override void Update()
	{
		base.Update();
		if (addOldIdlePosDelay > 0)
		{
			addOldIdlePosDelay--;
		}
		inRoomCounter++;
		creature.abstractAI.AbstractBehavior(1);
		if (focusCreature != null && focusCreature.TicksSinceSeen > 40)
		{
			focusCreature = null;
		}
		if (worm.room.aimap.getAItile(creature.pos).narrowSpace)
		{
			inFreeSpaceCounter -= 3;
		}
		else if (worm.room.aimap.getTerrainProximity(creature.pos) < 3)
		{
			inFreeSpaceCounter--;
		}
		else if (worm.room.aimap.getTerrainProximity(creature.pos) > 5)
		{
			inFreeSpaceCounter++;
		}
		inFreeSpaceCounter = Custom.IntClamp(inFreeSpaceCounter, 0, 100);
		if (worm.safariControlled)
		{
			if (creature.realizedCreature != null && creature.realizedCreature.inputWithDiagonals.HasValue && (creature.realizedCreature.inputWithDiagonals.Value.x != 0 || creature.realizedCreature.inputWithDiagonals.Value.y != 0))
			{
				flySpeed = 1f;
			}
			else
			{
				flySpeed = 0f;
			}
		}
		else if (behavior == Behavior.Idle)
		{
			flySpeed = Custom.LerpAndTick(flySpeed, (creature.abstractAI.followCreature != null && creature.abstractAI.destination.room != creature.pos.room) ? 0.5f : 0f, 0.06f, 1f / 60f);
		}
		else
		{
			flySpeed = Custom.LerpAndTick(flySpeed, 1f, 0.06f, 1f / 60f);
		}
		if (behavior == Behavior.Flee)
		{
			flyHeightAdd = Mathf.Min(1f, flyHeightAdd + 1f / 60f);
		}
		else
		{
			flyHeightAdd = Mathf.Max(0f, flyHeightAdd - 0.0045454544f);
		}
	}

	public int MinFlyHeight(IntVector2 tile)
	{
		if (worm.room.GetTile(worm.room.aimap.getAItile(tile).fallRiskTile).wormGrass)
		{
			return 16;
		}
		if (TileInEnclosedArea(tile))
		{
			return 0;
		}
		return 4 + (int)(4f * flyHeightAdd);
	}

	public int MinFlyHeight(IntVector2 tile, bool enclosed)
	{
		if (worm.room.GetTile(worm.room.aimap.getAItile(tile).fallRiskTile).wormGrass)
		{
			return 16;
		}
		if (enclosed)
		{
			return 0;
		}
		return 4 + (int)(4f * flyHeightAdd);
	}

	public void IdleBehavior()
	{
		Vector2 pos = ((Random.value < 0.15f) ? new Vector2(Random.value * worm.room.PixelWidth, Random.value * worm.room.PixelHeight) : ((!(Random.value < 0.5f)) ? (worm.mainBodyChunk.pos + Custom.RNV() * Random.value * 400f) : (worm.room.MiddleOfTile(idlePos) + Custom.RNV() * Random.value * 400f)));
		if (IdleScore(worm.room.GetWorldCoordinate(pos)) < IdleScore(idlePos))
		{
			idlePos = worm.room.GetWorldCoordinate(pos);
		}
		if (this is SmallNeedleWormAI && (this as SmallNeedleWormAI).Mother != null)
		{
			creature.abstractAI.SetDestination(idlePos);
		}
		else if (IdleScore(idlePos) + (float)idleCounter * 2f < IdleScore(base.pathFinder.GetDestination))
		{
			if (addOldIdlePosDelay < 1 && timeInRoom > 120)
			{
				addOldIdlePosDelay = 90;
				oldIdlePositions.Add(lastAssignedIdlePos);
				if (oldIdlePositions.Count > 5 && !Custom.DistLess(worm.room.MiddleOfTile(oldIdlePositions[0]), worm.room.MiddleOfTile(oldIdlePositions[oldIdlePositions.Count - 1]), 300f))
				{
					int num = 0;
					for (int i = 0; i < oldIdlePositions.Count; i++)
					{
						if (TileInEnclosedArea(oldIdlePositions[i].Tile))
						{
							num++;
						}
					}
					if ((float)num / (float)oldIdlePositions.Count >= 0.5f && !worm.State.confirmedNarrowRooms.Contains(creature.pos.room))
					{
						worm.State.confirmedNarrowRooms.Add(creature.pos.room);
						Custom.Log("worm doesnt like room", creature.Room.name);
					}
				}
				if (oldIdlePositions.Count > ((this is SmallNeedleWormAI) ? 5 : 20))
				{
					oldIdlePositions.RemoveAt(0);
				}
				idleCounter = Random.Range(100, 400);
			}
			creature.abstractAI.SetDestination(idlePos);
			lastAssignedIdlePos = idlePos;
		}
		idleCounter--;
		if (base.pathFinder.GetDestination.room != worm.room.abstractRoom.index || !(Vector2.Distance(worm.bodyChunks[1].pos, worm.room.MiddleOfTile(base.pathFinder.GetDestination.Tile)) < 60f))
		{
			return;
		}
		idleCounter--;
		if (inFreeSpaceCounter < 50)
		{
			idleCounter--;
			if (TileInEnclosedArea(base.pathFinder.GetDestination.Tile))
			{
				idleCounter -= 2;
			}
		}
	}

	protected virtual float IdleScore(WorldCoordinate coord)
	{
		return 0f;
	}

	public override bool WantToStayInDenUntilEndOfCycle()
	{
		return base.rainTracker.Utility() > 0.01f;
	}

	public override float VisualScore(Vector2 lookAtPoint, float bonus)
	{
		return base.VisualScore(lookAtPoint, bonus) * Custom.LerpMap(Vector2.Distance(worm.mainBodyChunk.pos, lookAtPoint), 60f, 100f, 1f, Mathf.InverseLerp(-0.2f, 0.1f, Vector2.Dot((worm.bodyChunks[1].pos - worm.bodyChunks[0].pos).normalized, (worm.bodyChunks[0].pos - lookAtPoint).normalized)));
	}

	public CreatureTemplate.Relationship UncomfortableToAfraidRelationshipModifier(RelationshipTracker.DynamicRelationship dRel, CreatureTemplate.Relationship currRel)
	{
		if ((dRel.state as NeedleWormTrackState).close)
		{
			if (inFreeSpaceCounter > 50 && Custom.WorldCoordFloatDist(creature.pos, dRel.trackerRep.BestGuessForPosition()) > 10f)
			{
				(dRel.state as NeedleWormTrackState).close = false;
			}
		}
		else if (Custom.WorldCoordFloatDist(creature.pos, dRel.trackerRep.BestGuessForPosition()) < 5f)
		{
			(dRel.state as NeedleWormTrackState).close = true;
		}
		if ((dRel.state as NeedleWormTrackState).close || (dRel.trackerRep.BestGuessForPosition().room == creature.pos.room && worm.room.aimap.getAItile(dRel.trackerRep.BestGuessForPosition()).narrowSpace))
		{
			return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, currRel.intensity * ((dRel.trackerRep.representedCreature.realizedCreature != null) ? Custom.LerpMap(dRel.trackerRep.representedCreature.realizedCreature.TotalMass, 0.25f, 4f, 0.1f, 1f) : 0.5f));
		}
		return currRel;
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
		float num = Custom.LerpMap(Mathf.Min(worm.room.aimap.getTerrainProximity(creature.pos), worm.room.aimap.getTerrainProximity(coord.DestTile)), 1f, 6f, 100f, 0f, 2f);
		if (!worm.room.aimap.getAItile(creature.pos).narrowSpace && worm.room.aimap.getAItile(coord.DestTile).narrowSpace)
		{
			num += 300f;
		}
		return new PathCost(cost.resistance + num, cost.legality);
	}

	public bool TileInEnclosedArea(IntVector2 tile)
	{
		if (worm.room.aimap.getTerrainProximity(tile) < 2)
		{
			return true;
		}
		if (worm.room.aimap.getTerrainProximity(tile) > 4)
		{
			return false;
		}
		int num = 0;
		for (int i = 0; i < 8; i++)
		{
			if (!worm.room.GetTile(tile + Custom.eightDirections[i] * 2).Solid && !worm.room.GetTile(tile + Custom.eightDirections[i] * 3).Solid && !worm.room.GetTile(tile + Custom.eightDirections[i] * 4).Solid)
			{
				continue;
			}
			num++;
			if (num > 3)
			{
				return true;
			}
			for (int j = 2; j < 6; j++)
			{
				if (worm.room.GetTile(tile - Custom.eightDirections[i] * j).Solid)
				{
					return true;
				}
			}
		}
		return false;
	}

	public override void CreatureSpotted(bool firstSpot, Tracker.CreatureRepresentation otherCreature)
	{
	}
}
