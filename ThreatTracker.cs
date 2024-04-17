using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class ThreatTracker : AIModule
{
	public class ThreatPoint
	{
		public CreatureTemplate crit;

		public WorldCoordinate pos;

		public float severity;

		public ThreatPoint(CreatureTemplate crit, WorldCoordinate pos, float severity)
		{
			this.crit = crit;
			this.pos = pos;
			this.severity = severity;
		}
	}

	public class ThreatCreature
	{
		private ThreatTracker owner;

		public Tracker.CreatureRepresentation creature;

		private float severity;

		private ThreatPoint threatPoint;

		public float CurrentThreat
		{
			get
			{
				if (creature.deleteMeNextFrame)
				{
					return 0f;
				}
				return Mathf.InverseLerp(0.05f, 0.5f, Mathf.Pow(creature.EstimatedChanceOfFinding, 0.75f) * severity);
			}
		}

		public ThreatCreature(ThreatTracker owner, Tracker.CreatureRepresentation creature)
		{
			this.owner = owner;
			this.creature = creature;
			severity = owner.AI.DynamicRelationship(creature).intensity;
			threatPoint = owner.AddThreatPoint(creature.representedCreature.creatureTemplate, creature.BestGuessForPosition(), severity);
		}

		public void Update()
		{
			severity = owner.AI.DynamicRelationship(creature).intensity;
			creature.forgetCounter = 0;
			if (creature.BestGuessForPosition().room == owner.AI.creature.pos.room)
			{
				threatPoint.severity = CurrentThreat;
				threatPoint.pos = creature.BestGuessForPosition();
				return;
			}
			threatPoint.severity = 0f;
			int num = owner.AI.creature.Room.ExitIndex(creature.BestGuessForPosition().room);
			if (num > -1)
			{
				threatPoint.severity = CurrentThreat * 0.5f;
				threatPoint.pos = owner.AI.creature.Room.realizedRoom.ShortcutLeadingToNode(num).startCoord;
			}
		}

		public void Destroy(bool stopTracking)
		{
			owner.AI.relationshipTracker.ModuleHasAbandonedCreature(creature, owner);
			owner.threatPoints.Remove(threatPoint);
			owner.threatCreatures.Remove(this);
		}
	}

	private AImap aiMap;

	private List<ThreatPoint> threatPoints;

	private List<ThreatCreature> threatCreatures;

	public Tracker.CreatureRepresentation mostThreateningCreature;

	public WorldCoordinate savedFleeDest;

	public WorldCoordinate testFleeDest;

	private int antiFlickerCounter;

	private int resetCounter;

	private int maxRememberedCreatures;

	public float accessibilityConsideration;

	private float currentThreat;

	private List<IntVector2> scratchPath;

	private List<IntVector2> testRandomPath;

	public int TotalTrackedThreats => threatPoints.Count;

	public int TotalTrackedThreatCreatures => threatPoints.Count;

	public float Panic => 1f - 1f / (currentThreat + 1f);

	public override float Utility()
	{
		if (ModManager.MSC && AI.creature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.ScavengerKing)
		{
			return 0f;
		}
		return Mathf.Clamp(Mathf.Lerp(-0.01f, 1.01f, currentThreat), 0f, 1f);
	}

	public ThreatTracker(ArtificialIntelligence AI, int maxRememberedCreatures)
		: base(AI)
	{
		this.maxRememberedCreatures = maxRememberedCreatures;
		accessibilityConsideration = 5f;
		threatPoints = new List<ThreatPoint>();
		threatCreatures = new List<ThreatCreature>();
	}

	public override void NewRoom(Room room)
	{
		aiMap = room.aimap;
		savedFleeDest = new WorldCoordinate(room.abstractRoom.index, -1, -1, Random.Range(0, room.abstractRoom.nodes.Length));
	}

	public override void Update()
	{
		currentThreat = ThreatOfArea(AI.creature.pos, accountThreatCreatureAccessibility: false);
		if (currentThreat <= 0f)
		{
			resetCounter++;
		}
		else
		{
			resetCounter = 0;
		}
		if (antiFlickerCounter > 0)
		{
			antiFlickerCounter--;
		}
		if (resetCounter < 200)
		{
			float num = 0f;
			mostThreateningCreature = null;
			for (int num2 = threatCreatures.Count - 1; num2 >= 0; num2--)
			{
				threatCreatures[num2].Update();
				if (threatCreatures[num2].creature.deleteMeNextFrame)
				{
					threatCreatures[num2].Destroy(stopTracking: false);
				}
				else if (threatCreatures[num2].CurrentThreat > num)
				{
					num = threatCreatures[num2].CurrentThreat;
					mostThreateningCreature = threatCreatures[num2].creature;
				}
			}
			if (mostThreateningCreature != null && mostThreateningCreature.BestGuessForPosition().room == AI.creature.pos.room)
			{
				currentThreat = Mathf.Pow(currentThreat, 1f / (1f + num * 3f));
			}
		}
		else if (resetCounter >= 200 && threatCreatures.Count > 0)
		{
			for (int num3 = threatCreatures.Count - 1; num3 >= 0; num3--)
			{
				threatCreatures[num3].Destroy(stopTracking: true);
			}
		}
	}

	public WorldCoordinate FleeTo(WorldCoordinate occupyTile, int reevalutaions, int maximumDistance, bool considerLeavingRoom)
	{
		return FleeTo(occupyTile, reevalutaions, maximumDistance, considerLeavingRoom, considerGoingHome: false);
	}

	public WorldCoordinate FleeTo(WorldCoordinate occupyTile, int reevalutaions, int maximumDistance, bool considerLeavingRoom, bool considerGoingHome)
	{
		reevalutaions = AI.creature.world.game.pathfinderResourceDivider.RequestPathfinderUpdates(reevalutaions);
		if (!AI.pathFinder.CoordinateViable(occupyTile))
		{
			for (int i = 0; i < 4; i++)
			{
				if (AI.pathFinder.CoordinateViable(occupyTile + Custom.fourDirections[i]))
				{
					occupyTile += Custom.fourDirections[i];
					break;
				}
			}
		}
		int num = 0;
		float num2 = EvaluateFlightDestThreat(occupyTile, testFleeDest, maximumDistance, ref scratchPath);
		for (int j = 0; j < reevalutaions * 5; j++)
		{
			WorldCoordinate worldCoordinate = new WorldCoordinate(occupyTile.room, occupyTile.x + Random.Range(0, maximumDistance) * ((!(Random.value < 0.5f)) ? 1 : (-1)), occupyTile.y + Random.Range(0, maximumDistance) * ((!(Random.value < 0.5f)) ? 1 : (-1)), -1);
			worldCoordinate.x = Custom.IntClamp(worldCoordinate.x, 0, AI.creature.Room.realizedRoom.TileWidth - 1);
			worldCoordinate.y = Custom.IntClamp(worldCoordinate.y, 0, AI.creature.Room.realizedRoom.TileHeight - 1);
			if (testFleeDest.Tile.FloatDist(worldCoordinate.Tile) > 3f && aiMap.WorldCoordinateAccessibleToCreature(worldCoordinate, AI.creature.creatureTemplate))
			{
				float num3 = EvaluateFlightDestThreat(occupyTile, worldCoordinate, maximumDistance, ref scratchPath);
				if (num3 < num2)
				{
					testFleeDest = worldCoordinate;
					num2 = num3;
				}
				num++;
			}
			if (num >= reevalutaions)
			{
				break;
			}
		}
		if (antiFlickerCounter < 1 && savedFleeDest != testFleeDest && num2 < EvaluateFlightDestThreat(occupyTile, savedFleeDest, maximumDistance, ref scratchPath) - 0.5f * Mathf.InverseLerp(1f, 0.5f, currentThreat))
		{
			savedFleeDest = testFleeDest;
			antiFlickerCounter = 20;
		}
		if (considerLeavingRoom)
		{
			int num4 = -1;
			int num5 = int.MaxValue;
			int num6 = AI.creature.Room.NodesRelevantToCreature(AI.creature.creatureTemplate);
			for (int k = 0; k < num6; k++)
			{
				int num7 = AI.creature.Room.realizedRoom.aimap.ExitDistanceForCreatureAndCheckNeighbours(occupyTile.Tile, k, AI.creature.creatureTemplate);
				int num8 = AI.creature.Room.CreatureSpecificToCommonNodeIndex(k, AI.creature.creatureTemplate);
				if (AI.pathFinder != null && AI.timeInRoom < 100 && AI.pathFinder.forbiddenEntrance.abstractNode == num8)
				{
					num7 = -1;
				}
				if (AI.creature.Room.nodes[num8].type == AbstractRoomNode.Type.Exit && AI.creature.Room.connections[num8] > -1 && mostThreateningCreature != null && mostThreateningCreature.BestGuessForPosition().room == AI.creature.Room.connections[num8])
				{
					num7 = -1;
				}
				if (num7 > 0)
				{
					int num9 = AI.creature.Room.realizedRoom.aimap.ExitDistanceForCreatureAndCheckNeighbours(occupyTile.Tile, k, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Fly));
					if (num9 > 5)
					{
						for (int l = 0; l < threatPoints.Count; l++)
						{
							if (threatPoints[l].pos.room != AI.creature.Room.index)
							{
								continue;
							}
							int num10 = AI.creature.Room.realizedRoom.aimap.ExitDistanceForCreatureAndCheckNeighbours(threatPoints[l].pos.Tile, k, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Fly));
							if (num10 > 0)
							{
								num10 += 10;
								if (num10 < num9)
								{
									num7 = -1;
									break;
								}
							}
						}
					}
				}
				if (num7 > 0 && num7 < num5 && ((AI.creature.Room.nodes[num8].type == AbstractRoomNode.Type.Exit && AI.creature.Room.connections[num8] > -1) || (AI.creature.Room.nodes[num8].borderExit && considerGoingHome) || (AI.creature.Room.nodes[num8].type == AbstractRoomNode.Type.RegionTransportation && considerGoingHome)))
				{
					num4 = num8;
					num5 = num7;
				}
			}
			if (num4 > -1 && num5 < maximumDistance)
			{
				if (AI.creature.Room.nodes[num4].type == AbstractRoomNode.Type.Exit)
				{
					AbstractRoom abstractRoom = AI.creature.world.GetAbstractRoom(AI.creature.Room.connections[num4]);
					int num11 = abstractRoom.ExitIndex(AI.creature.Room.index);
					int abstractNode = num11;
					int num12 = int.MinValue;
					for (int m = 0; m < abstractRoom.nodes.Length; m++)
					{
						if (abstractRoom.ConnectionAndBackPossible(num11, m, AI.creature.creatureTemplate) && abstractRoom.ConnectionLength(num11, m, AI.creature.creatureTemplate) > num12)
						{
							abstractNode = m;
							num12 = abstractRoom.ConnectionLength(num11, m, AI.creature.creatureTemplate);
						}
					}
					return new WorldCoordinate(abstractRoom.index, -1, -1, abstractNode);
				}
				if (AI.creature.Room.nodes[num4].type == AbstractRoomNode.Type.RegionTransportation || AI.creature.Room.nodes[num4].type == AbstractRoomNode.Type.SeaExit || AI.creature.Room.nodes[num4].type == AbstractRoomNode.Type.SideExit || AI.creature.Room.nodes[num4].type == AbstractRoomNode.Type.SkyExit)
				{
					return new WorldCoordinate(AI.creature.world.offScreenDen.index, -1, -1, 0);
				}
			}
		}
		return savedFleeDest;
	}

	public List<IntVector2> GenerateRandomPath(WorldCoordinate occupyTile, int length)
	{
		if (!AI.pathFinder.CoordinateViable(occupyTile))
		{
			return null;
		}
		Room realizedRoom = AI.creature.Room.realizedRoom;
		IntVector2 tile = occupyTile.Tile;
		List<IntVector2> list = new List<IntVector2>();
		IntVector2 intVector = tile;
		for (int i = 0; i < length; i++)
		{
			IntVector2 intVector2 = intVector;
			float num = 0f;
			for (int j = 0; j < realizedRoom.aimap.getAItile(intVector).outgoingPaths.Count; j++)
			{
				MovementConnection movementConnection = realizedRoom.aimap.getAItile(intVector).outgoingPaths[j];
				if (!AI.creature.creatureTemplate.ConnectionResistance(movementConnection.type).Allowed || !AI.creature.creatureTemplate.AccessibilityResistance(realizedRoom.aimap.getAItile(movementConnection.destinationCoord).acc).Allowed)
				{
					continue;
				}
				float num2 = Random.value;
				if (!(num2 > num))
				{
					continue;
				}
				for (int k = 0; k < list.Count; k++)
				{
					if (!(num2 > 0f))
					{
						break;
					}
					if (list[k] == movementConnection.DestTile)
					{
						num2 = 0f;
					}
				}
				if (num2 > num)
				{
					intVector2 = movementConnection.DestTile;
					num = num2;
				}
			}
			if (!(num > 0f))
			{
				break;
			}
			list.Add(intVector);
			intVector = intVector2;
		}
		return list;
	}

	private float EvaluateFlightDestThreat(WorldCoordinate occupyTile, WorldCoordinate coord, int maximumDistance, ref List<IntVector2> scratchPath)
	{
		if (coord.room != occupyTile.room)
		{
			return float.MaxValue;
		}
		if (Custom.ManhattanDistance(coord, occupyTile) >= maximumDistance * 2)
		{
			return float.MaxValue;
		}
		if (!AI.pathFinder.CoordinateViable(coord))
		{
			return float.MaxValue;
		}
		int num = AI.creature.Room.realizedRoom.RayTraceTilesList(occupyTile.x, occupyTile.y, coord.x, coord.y, ref scratchPath);
		bool flag = true;
		for (int i = 0; i < num && flag; i++)
		{
			if (!aiMap.TileAccessibleToCreature(scratchPath[i], AI.creature.creatureTemplate))
			{
				flag = false;
			}
		}
		if (flag)
		{
			return ThreatOfPath(scratchPath, num);
		}
		int maxGenerations = 500;
		num = QuickConnectivity.QuickPath(AI.creature.Room.realizedRoom, AI.creature.creatureTemplate, occupyTile.Tile, coord.Tile, maximumDistance * 2, maxGenerations, inOpenMedium: true, ref scratchPath);
		return ThreatOfPath(scratchPath, num);
	}

	private float ThreatOfPath(List<IntVector2> path, int pathCount)
	{
		if (path == null || pathCount == 0)
		{
			return float.MaxValue;
		}
		float num = 0f;
		for (int i = 0; i < pathCount; i++)
		{
			num += ThreatOfTile(AI.creature.Room.realizedRoom.GetWorldCoordinate(path[i]), accountThreatCreatureAccessibility: true);
			for (int j = 0; j < threatPoints.Count; j++)
			{
				ThreatPoint threatPoint = threatPoints[j];
				if (threatPoint.pos.Tile == path[i])
				{
					num += 10f * threatPoint.severity;
				}
			}
		}
		if (pathCount < 2 && ThreatOfTile(AI.creature.Room.realizedRoom.GetWorldCoordinate(path[pathCount - 1]), accountThreatCreatureAccessibility: false) < 0.5f)
		{
			num += 100f;
		}
		if (ThreatOfArea(AI.creature.Room.realizedRoom.GetWorldCoordinate(path[pathCount - 1]), accountThreatCreatureAccessibility: true) > ThreatOfArea(AI.creature.Room.realizedRoom.GetWorldCoordinate(path[0]), accountThreatCreatureAccessibility: true))
		{
			num += 1000f;
		}
		num /= (float)pathCount;
		return num + ThreatOfArea(AI.creature.Room.realizedRoom.GetWorldCoordinate(path[pathCount - 1]), accountThreatCreatureAccessibility: true);
	}

	public int FindMostAttractiveExit()
	{
		if (aiMap == null)
		{
			return -1;
		}
		float num = float.MinValue;
		int result = -1;
		for (int i = 0; i < AI.creature.Room.nodes.Length; i++)
		{
			if (!AI.creature.creatureTemplate.mappedNodeTypes[(int)AI.creature.Room.nodes[i].type] || (!(AI.creature.Room.nodes[i].type != AbstractRoomNode.Type.Exit) && AI.creature.Room.connections[i] <= -1))
			{
				continue;
			}
			int num2 = aiMap.ExitDistanceForCreature(AI.creature.pos.Tile, AI.creature.Room.CommonToCreatureSpecificNodeIndex(i, AI.creature.creatureTemplate), AI.creature.creatureTemplate);
			float num3 = 0f;
			for (int j = 0; j < threatPoints.Count; j++)
			{
				ThreatPoint threatPoint = threatPoints[j];
				if (threatPoint.pos.room == AI.creature.pos.room)
				{
					int num4 = ((threatPoint.crit == null) ? aiMap.ExitDistanceForCreature(threatPoint.pos.Tile, AI.creature.Room.CommonToCreatureSpecificNodeIndex(i, AI.creature.creatureTemplate), AI.creature.creatureTemplate) : aiMap.ExitDistanceForCreature(threatPoint.pos.Tile, AI.creature.Room.CommonToCreatureSpecificNodeIndex(i, threatPoint.crit), threatPoint.crit));
					if (num4 < num2)
					{
						num3 = -1f;
						break;
					}
					num3 += (float)(num4 - num2);
				}
			}
			float num5 = ThreatOfArea(aiMap.room.LocalCoordinateOfNode(i), accountThreatCreatureAccessibility: true);
			num3 /= num5;
			if (AI.creature.Room.nodes[i].type != AbstractRoomNode.Type.Exit)
			{
				num3 = float.MinValue;
			}
			if (num3 > num)
			{
				num = num3;
				result = i;
			}
		}
		if (num >= 0f)
		{
			return result;
		}
		return -1;
	}

	public ThreatPoint AddThreatPoint(CreatureTemplate crit, WorldCoordinate pos, float severity)
	{
		ThreatPoint threatPoint = new ThreatPoint(crit, pos, severity);
		threatPoints.Add(threatPoint);
		return threatPoint;
	}

	public void RemoveThreatPoint(ThreatPoint tp)
	{
		for (int num = threatPoints.Count - 1; num >= 0; num--)
		{
			if (threatPoints[num] == tp)
			{
				threatPoints.RemoveAt(num);
			}
		}
	}

	public void AddThreatCreature(Tracker.CreatureRepresentation creature)
	{
		foreach (ThreatCreature threatCreature2 in threatCreatures)
		{
			if (threatCreature2.creature == creature)
			{
				return;
			}
		}
		threatCreatures.Add(new ThreatCreature(this, creature));
		resetCounter = 0;
		if (threatCreatures.Count <= maxRememberedCreatures)
		{
			return;
		}
		float num = float.MaxValue;
		ThreatCreature threatCreature = null;
		foreach (ThreatCreature threatCreature3 in threatCreatures)
		{
			if (threatCreature3.CurrentThreat < num)
			{
				num = threatCreature3.CurrentThreat;
				threatCreature = threatCreature3;
			}
		}
		threatCreature?.Destroy(stopTracking: true);
	}

	public void RemoveThreatCreature(AbstractCreature crit)
	{
		for (int num = threatCreatures.Count - 1; num >= 0; num--)
		{
			if (threatCreatures[num].creature.representedCreature == crit)
			{
				threatCreatures[num].Destroy(stopTracking: false);
				break;
			}
		}
	}

	public ThreatCreature GetThreatCreature(AbstractCreature crit)
	{
		for (int num = threatCreatures.Count - 1; num >= 0; num--)
		{
			if (threatCreatures[num].creature.representedCreature == crit)
			{
				return threatCreatures[num];
			}
		}
		return null;
	}

	public float ThreatOfArea(WorldCoordinate coord, bool accountThreatCreatureAccessibility)
	{
		float num = 0f;
		for (int i = 0; i < 9; i++)
		{
			num += ThreatOfTile(WorldCoordinate.AddIntVector(coord, Custom.eightDirectionsAndZero[i]), accountThreatCreatureAccessibility);
		}
		return num / 9f;
	}

	public float ThreatOfTile(WorldCoordinate coord, bool accountThreatCreatureAccessibility)
	{
		if (coord.room != AI.creature.pos.room)
		{
			return 0f;
		}
		float num = 0f;
		for (int i = 0; i < threatPoints.Count; i++)
		{
			ThreatPoint threatPoint = threatPoints[i];
			if (threatPoint.pos.room == AI.creature.pos.room)
			{
				float num2 = Mathf.Sqrt(Mathf.Pow(coord.Tile.x - threatPoint.pos.Tile.x, 2f) + Mathf.Pow(coord.Tile.y - threatPoint.pos.Tile.y, 2f));
				float num3 = Mathf.Clamp(Mathf.InverseLerp(2f, 10f, num2), 0f, 1f);
				num2 = Mathf.Pow(num2, 1.25f);
				num2 = Mathf.Pow(threatPoint.severity, 1.5f) * 10f / Mathf.Max(1f, num2);
				if (threatPoint.crit != null && aiMap.AccessibilityForCreature(coord.Tile, threatPoint.crit) > 0f)
				{
					num2 *= Mathf.Pow(aiMap.AccessibilityForCreature(coord.Tile, threatPoint.crit), accessibilityConsideration * num3);
				}
				num2 *= Mathf.Lerp(1f, (float)aiMap.getAItile(coord).visibility / (float)(aiMap.width * aiMap.height), Mathf.InverseLerp(15f, 25f, num2));
				num += num2;
			}
		}
		return num;
	}
}
