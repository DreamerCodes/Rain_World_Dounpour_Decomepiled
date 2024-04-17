using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class StuckTracker : AIModule
{
	public abstract class StuckTrackerModule
	{
		public StuckTracker parent;

		public ArtificialIntelligence AI => parent.AI;

		public virtual bool AddToStuckCounter => false;

		public StuckTrackerModule(StuckTracker parent)
		{
			this.parent = parent;
		}

		public virtual void Update()
		{
		}
	}

	public class CloseToGoalButNotSeeingItTracker : StuckTrackerModule
	{
		public float tileDistance;

		public int counter;

		public int counterMin = 20;

		public int counterMax = 200;

		public float Stuck => Mathf.InverseLerp(counterMin, counterMax, counter);

		public override bool AddToStuckCounter => counter > counterMin;

		public CloseToGoalButNotSeeingItTracker(StuckTracker parent, float tileDistance)
			: base(parent)
		{
			this.tileDistance = tileDistance;
		}

		public override void Update()
		{
			if (base.AI.pathFinder.GetDestination.room == base.AI.creature.pos.room && base.AI.creature.realizedCreature != null && base.AI.creature.realizedCreature.room != null && base.AI.pathFinder.GetDestination.Tile.FloatDist(base.AI.creature.pos.Tile) < tileDistance && !base.AI.VisualContact(base.AI.creature.realizedCreature.room.MiddleOfTile(base.AI.pathFinder.GetDestination), 0f))
			{
				counter++;
			}
			else
			{
				counter--;
			}
			counter = Custom.IntClamp(counter, 0, counterMax);
		}
	}

	public class GetUnstuckPosCalculator : StuckTrackerModule
	{
		public WorldCoordinate unstuckGoalPosition;

		public GetUnstuckPosCalculator(StuckTracker parent)
			: base(parent)
		{
			unstuckGoalPosition = base.AI.creature.pos;
		}

		public override void Update()
		{
			if (parent.stuckCounter > 0)
			{
				WorldCoordinate worldCoordinate = base.AI.creature.realizedCreature.room.GetWorldCoordinate(base.AI.creature.realizedCreature.mainBodyChunk.pos + Custom.RNV() * UnityEngine.Random.value * 15f * 20f);
				if (UnstuckGoalScore(worldCoordinate) < UnstuckGoalScore(unstuckGoalPosition))
				{
					unstuckGoalPosition = worldCoordinate;
				}
			}
		}

		private float UnstuckGoalScore(WorldCoordinate testPos)
		{
			if (!base.AI.pathFinder.CoordinateReachableAndGetbackable(testPos))
			{
				return float.MaxValue;
			}
			float num = Mathf.Abs(10f - testPos.Tile.FloatDist(base.AI.creature.pos.Tile));
			if (testPos.Tile.FloatDist(base.AI.creature.pos.Tile) < 20f && base.AI.creature.realizedCreature.room.VisualContact(testPos.Tile, base.AI.creature.pos.Tile))
			{
				num -= 1000f;
			}
			return num;
		}
	}

	public class StuckCloseToShortcutModule : StuckTrackerModule
	{
		private bool activeWhenStuck = true;

		public IntVector2? foundShortCut;

		public StuckCloseToShortcutModule(StuckTracker parent)
			: base(parent)
		{
		}

		public override void Update()
		{
			foundShortCut = null;
			if ((activeWhenStuck && parent.stuckCounter < 1) || base.AI.creature.Room.realizedRoom == null || base.AI.creature.realizedCreature.enteringShortCut.HasValue)
			{
				return;
			}
			int num = 0;
			while (!foundShortCut.HasValue && num < base.AI.creature.Room.realizedRoom.shortcuts.Length)
			{
				for (int i = 0; !foundShortCut.HasValue & (i < ((!(base.AI.creature.Room.realizedRoom.shortcuts[num].shortCutType == ShortcutData.Type.Normal)) ? 1 : 2)); i++)
				{
					IntVector2 intVector = ((i == 0) ? base.AI.creature.Room.realizedRoom.shortcuts[num].StartTile : base.AI.creature.Room.realizedRoom.shortcuts[num].DestTile);
					if (base.AI.creature.Room.realizedRoom.GetTile(intVector).Terrain != Room.Tile.TerrainType.ShortcutEntrance)
					{
						continue;
					}
					for (int j = 0; !foundShortCut.HasValue & (j < base.AI.creature.realizedCreature.bodyChunks.Length); j++)
					{
						if (Custom.ManhattanDistance(intVector, base.AI.creature.Room.realizedRoom.GetTilePosition(base.AI.creature.realizedCreature.bodyChunks[j].pos)) < 3)
						{
							foundShortCut = intVector;
							break;
						}
					}
				}
				num++;
			}
		}
	}

	public class MoveBacklog : StuckTrackerModule
	{
		public int trackedMoves = 10;

		public List<MovementConnection> log;

		public MoveBacklog(StuckTracker parent)
			: base(parent)
		{
			log = new List<MovementConnection>();
		}

		public void Reset()
		{
			log.Clear();
		}

		public void ReportNewMove(MovementConnection connection)
		{
			if (!(connection == default(MovementConnection)) && (log.Count == 0 || log[0] != connection))
			{
				log.Insert(0, connection);
				if (log.Count > trackedMoves)
				{
					log.RemoveAt(log.Count - 1);
				}
			}
		}

		public bool IsMoveInLog(MovementConnection testConnection, int howManyStepsBack)
		{
			if (testConnection == default(MovementConnection))
			{
				return false;
			}
			if (log.Count == 0)
			{
				return false;
			}
			for (int i = 0; i < Math.Min(howManyStepsBack, log.Count); i++)
			{
				if (log[i] == testConnection)
				{
					return true;
				}
			}
			return false;
		}
	}

	public List<IntVector2> pastPositions;

	public int stuckCounter;

	public int notFollowingPathToCurrentGoalCounter;

	public int notFollowingCurrentGoalMax = 200;

	public int goalSatisfactionDistance = 3;

	public int totalTrackedLastPositions = 80;

	public int checkPastPositionsFrom = 40;

	public int pastPosStuckDistance = 4;

	public int pastStuckPositionsCloseToIncrementStuckCounter = 30;

	public int minStuckCounter = 100;

	public int maxStuckCounter = 200;

	public bool trackPastPositions;

	public bool trackNotFollowingCurrentGoal;

	public bool calculateGetUnstuckPosition;

	public bool satisfiedWithThisPosition;

	private List<StuckTrackerModule> subModules;

	public CloseToGoalButNotSeeingItTracker closeToGoalButNotSeeingItTracker;

	public GetUnstuckPosCalculator getUnstuckPosCalculator;

	public StuckCloseToShortcutModule stuckCloseToShortcutModule;

	public MoveBacklog moveBacklog;

	public StuckTracker(ArtificialIntelligence AI, bool trackPastPositions, bool trackNotFollowingCurrentGoal)
		: base(AI)
	{
		this.trackPastPositions = trackPastPositions;
		this.trackNotFollowingCurrentGoal = trackNotFollowingCurrentGoal;
		if (trackPastPositions)
		{
			pastPositions = new List<IntVector2>();
		}
		subModules = new List<StuckTrackerModule>();
	}

	public override void NewRoom(Room room)
	{
		base.NewRoom(room);
		if (moveBacklog != null)
		{
			moveBacklog.Reset();
		}
	}

	public override void Update()
	{
		for (int i = 0; i < subModules.Count; i++)
		{
			subModules[i].Update();
		}
		if (trackNotFollowingCurrentGoal)
		{
			if (notFollowingPathToCurrentGoalCounter < notFollowingCurrentGoalMax && AI.pathFinder.GetEffectualDestination != AI.pathFinder.GetDestination)
			{
				notFollowingPathToCurrentGoalCounter++;
			}
			else if (notFollowingPathToCurrentGoalCounter > 0)
			{
				notFollowingPathToCurrentGoalCounter--;
			}
		}
		bool flag = false;
		if (trackPastPositions)
		{
			int num = 0;
			if (!satisfiedWithThisPosition && (Custom.ManhattanDistance(AI.creature.pos, AI.pathFinder.GetEffectualDestination) > goalSatisfactionDistance || notFollowingPathToCurrentGoalCounter > notFollowingCurrentGoalMax / 2))
			{
				pastPositions.Insert(0, AI.creature.pos.Tile);
				if (pastPositions.Count > totalTrackedLastPositions)
				{
					pastPositions.RemoveAt(pastPositions.Count - 1);
				}
				for (int j = checkPastPositionsFrom; j < pastPositions.Count; j++)
				{
					if (Custom.DistLess(AI.creature.pos.Tile, pastPositions[j], pastPosStuckDistance))
					{
						num++;
					}
				}
			}
			if (num > pastStuckPositionsCloseToIncrementStuckCounter)
			{
				flag = true;
			}
		}
		int num2 = 0;
		while (!flag && num2 < subModules.Count)
		{
			flag = subModules[num2].AddToStuckCounter;
			num2++;
		}
		if (flag)
		{
			stuckCounter++;
		}
		else
		{
			stuckCounter -= 2;
		}
		stuckCounter = Custom.IntClamp(stuckCounter, 0, maxStuckCounter);
	}

	public void Reset()
	{
		stuckCounter = 0;
	}

	public override float Utility()
	{
		return Mathf.InverseLerp(minStuckCounter, maxStuckCounter, stuckCounter);
	}

	public void AddSubModule(StuckTrackerModule newModule)
	{
		subModules.Add(newModule);
		if (newModule is CloseToGoalButNotSeeingItTracker)
		{
			closeToGoalButNotSeeingItTracker = newModule as CloseToGoalButNotSeeingItTracker;
		}
		else if (newModule is GetUnstuckPosCalculator)
		{
			getUnstuckPosCalculator = newModule as GetUnstuckPosCalculator;
		}
		else if (newModule is StuckCloseToShortcutModule)
		{
			stuckCloseToShortcutModule = newModule as StuckCloseToShortcutModule;
		}
		else if (newModule is MoveBacklog)
		{
			moveBacklog = newModule as MoveBacklog;
		}
	}
}
