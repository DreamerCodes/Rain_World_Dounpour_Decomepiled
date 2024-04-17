using System;
using System.Collections.Generic;
using Expedition;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class AbstractCreatureAI
{
	public AbstractCreature parent;

	public int lastRoom;

	private WorldCoordinate? privDenPos;

	public int strandedInRoom = -1;

	public List<WorldCoordinate> path;

	public World world;

	private WorldCoordinate? migrationDestination;

	public ArtificialIntelligence RealAI;

	public AbstractCreature followCreature;

	public int timeBuffer;

	public bool freezeDestination;

	public WorldCoordinate? denPosition
	{
		get
		{
			if (parent.creatureTemplate.hibernateOffScreen)
			{
				return new WorldCoordinate(world.offScreenDen.index, -1, -1, 0);
			}
			return privDenPos;
		}
		set
		{
			privDenPos = value;
		}
	}

	public WorldCoordinate destination { get; private set; }

	public WorldCoordinate MigrationDestination
	{
		get
		{
			if (WantToMigrate)
			{
				return migrationDestination.Value;
			}
			return destination;
		}
	}

	public bool WantToMigrate
	{
		get
		{
			if (migrationDestination.HasValue)
			{
				return migrationDestination.Value.room != parent.pos.room;
			}
			return false;
		}
	}

	public virtual float offscreenSpeedFac
	{
		get
		{
			if (followCreature == null)
			{
				return Mathf.Lerp(0.1f, 0.9f, Mathf.Pow(1f - parent.Room.AttractionValueForCreature(parent.creatureTemplate.type), 2f));
			}
			return 1f;
		}
	}

	public AbstractCreatureAI(World world, AbstractCreature parent)
	{
		this.world = world;
		this.parent = parent;
		lastRoom = parent.pos.room;
		path = new List<WorldCoordinate>();
		destination = parent.pos;
	}

	public virtual void NewWorld(World newWorld)
	{
		world = newWorld;
		denPosition = null;
		path.Clear();
		destination = parent.pos;
		migrationDestination = parent.pos;
	}

	public void GoToDen()
	{
		if (parent.creatureTemplate.abstractImmobile || parent.creatureTemplate.doesNotUseDens)
		{
			return;
		}
		if (denPosition.HasValue)
		{
			SetDestination(denPosition.Value);
			strandedInRoom = -1;
		}
		else if (parent.pos.room != strandedInRoom)
		{
			AbstractSpaceNodeFinder abstractSpaceNodeFinder = new AbstractSpaceNodeFinder(AbstractSpaceNodeFinder.SearchingFor.Den, AbstractSpaceNodeFinder.FloodMethod.Cost, 1000, parent.pos, parent.creatureTemplate, world, 0f);
			while (!abstractSpaceNodeFinder.finished)
			{
				abstractSpaceNodeFinder.Update();
			}
			List<WorldCoordinate> list = abstractSpaceNodeFinder.ReturnPathToClosest();
			if (list != null)
			{
				path = list;
				denPosition = path[0];
				SetDestination(denPosition.Value);
			}
			else
			{
				strandedInRoom = parent.pos.room;
			}
		}
	}

	public void SetDestination(WorldCoordinate newDest)
	{
		if (freezeDestination || (ModManager.MSC && parent.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.ScavengerKing && newDest.room != parent.pos.room))
		{
			return;
		}
		if (parent.realizedCreature == null && RealAI != null && world.GetAbstractRoom(newDest).realizedRoom == null)
		{
			RealAI = null;
		}
		if (parent.realizedCreature != null && RealAI != null)
		{
			RealAI.SetDestination(newDest);
			destination = newDest;
			if (newDest.room != parent.pos.room && MigrationDestination != newDest && (!migrationDestination.HasValue || world.GetAttractionValueForRoom(newDest, parent.creatureTemplate.type) > world.GetAttractionValueForRoom(migrationDestination.Value, parent.creatureTemplate.type)))
			{
				migrationDestination = newDest;
			}
			return;
		}
		if (!DoIHaveAPathToCoordinate(newDest))
		{
			newDest = QuickConnectivity.DefineNodeOfLocalCoordinate(newDest, world, parent.creatureTemplate);
			FindPath(newDest);
		}
		if (path.Count > 0)
		{
			destination = newDest;
			if (newDest.room != parent.pos.room)
			{
				migrationDestination = newDest;
			}
		}
	}

	public void MigrateTo(WorldCoordinate newDest)
	{
		InternalSetDestination(newDest);
	}

	private void InternalSetDestination(WorldCoordinate newDest)
	{
		if (parent.realizedCreature == null)
		{
			SetDestination(newDest);
		}
		else if (newDest.room != parent.pos.room)
		{
			if (world.game.devToolsActive && migrationDestination != newDest)
			{
				Custom.Log("realized creature migrate to:", world.GetAbstractRoom(newDest).name, "(", parent.ToString(), ")");
			}
			migrationDestination = newDest;
		}
	}

	public void SetDestinationNoPathing(WorldCoordinate newDest, bool migrate)
	{
		destination = newDest;
		if (parent.realizedCreature != null && RealAI != null)
		{
			RealAI.SetDestination(newDest);
		}
		if (migrate)
		{
			migrationDestination = newDest;
		}
	}

	protected bool DoIHaveAPathToCoordinate(WorldCoordinate dest)
	{
		if (path.Count < 1)
		{
			return dest.CompareDisregardingTile(parent.pos);
		}
		return dest.CompareDisregardingTile(path[0]);
	}

	public void FindPath(WorldCoordinate newDest)
	{
		newDest = QuickConnectivity.DefineNodeOfLocalCoordinate(newDest, world, parent.creatureTemplate);
		parent.pos = QuickConnectivity.DefineNodeOfLocalCoordinate(parent.pos, world, parent.creatureTemplate);
		if (newDest.room != parent.pos.room || !world.GetAbstractRoom(parent.pos.room).ConnectionPossible(parent.pos.abstractNode, newDest.abstractNode, parent.creatureTemplate))
		{
			if (DoIHaveAPathToCoordinate(newDest))
			{
				return;
			}
			path = AbstractSpacePathFinder.Path(world, parent.pos, newDest, parent.creatureTemplate, (this is IOwnAnAbstractSpacePathFinder) ? (this as IOwnAnAbstractSpacePathFinder) : null);
			if (path != null)
			{
				return;
			}
			if (RainWorld.ShowLogs)
			{
				if (ModManager.Expedition && world.game.rainWorld.ExpeditionMode && (ExpeditionGame.activeUnlocks.Contains("bur-hunted") || ExpeditionGame.activeUnlocks.Contains("bur-pursued")))
				{
					path = new List<WorldCoordinate>();
					return;
				}
				Custom.LogWarning($"NO PATH TO DESTINATION! {parent.creatureTemplate.type}");
			}
			path = new List<WorldCoordinate>();
		}
		else
		{
			path = new List<WorldCoordinate> { newDest };
		}
	}

	public virtual IntVector2[] PlaceInRealizedRoom()
	{
		Room realizedRoom = world.GetAbstractRoom(parent.pos).realizedRoom;
		if (realizedRoom == null)
		{
			Custom.LogWarning("TRYING TO PLACE IN REALIZED ROOM WITH NO REALIZED ROOM");
			return null;
		}
		WorldCoordinate pos = parent.pos;
		WorldCoordinate worldCoordinate = new WorldCoordinate(parent.pos.room, -1, -1, -1);
		if (!parent.pos.TileDefined)
		{
			pos.Tile = realizedRoom.LocalCoordinateOfNode(parent.pos.abstractNode).Tile;
		}
		pos.x = Custom.IntClamp(pos.x, 0, realizedRoom.TileWidth - 1);
		pos.y = Custom.IntClamp(pos.y, 0, realizedRoom.TileHeight - 1);
		Custom.Log("localStart:", pos.ToString());
		Custom.Log("localGoal:", worldCoordinate.ToString());
		if (pos.room == destination.room && pos.abstractNode == destination.abstractNode && (Custom.ManhattanDistance(pos.Tile, destination.Tile) < 20 || !destination.TileDefined))
		{
			int repeats = (int)((float)timeBuffer * parent.creatureTemplate.offScreenSpeed);
			pos = RandomizeSpawnPositionInRoom(pos, repeats);
			parent.pos.Tile = pos.Tile;
		}
		else
		{
			if (destination.room == parent.pos.room)
			{
				worldCoordinate = destination;
				if (!worldCoordinate.TileDefined)
				{
					worldCoordinate = realizedRoom.LocalCoordinateOfNode(destination.abstractNode);
				}
			}
			else
			{
				int num = DestinationExit(realizedRoom.abstractRoom.index, parent.pos.abstractNode);
				if (num > -1)
				{
					worldCoordinate = realizedRoom.LocalCoordinateOfNode(num);
				}
			}
			if (worldCoordinate.TileDefined)
			{
				QuickPathFinder quickPathFinder = new QuickPathFinder(pos.Tile, worldCoordinate.Tile, realizedRoom.aimap, parent.creatureTemplate);
				while (quickPathFinder.status == 0)
				{
					quickPathFinder.Update();
				}
				if (quickPathFinder.status == 1)
				{
					QuickPath quickPath = quickPathFinder.ReturnPath();
					if (quickPath.Length > 0)
					{
						int val = (int)((float)timeBuffer * parent.creatureTemplate.offScreenSpeed * offscreenSpeedFac);
						val = Custom.IntClamp(val, 0, quickPath.Length - 1);
						Custom.Log($"{parent.creatureTemplate.name} Spawning {val}/{quickPath.tiles.Length} along path between {pos} and {worldCoordinate}");
						parent.pos.Tile = quickPath.tiles[val];
						return quickPath.tiles;
					}
				}
			}
		}
		return null;
	}

	public virtual int DestinationExit(int evaluateRoom, int theDoorICameInThrough)
	{
		if (RealAI != null && RealAI.pathFinder != null)
		{
			return RealAI.pathFinder.DestinationExit(evaluateRoom, theDoorICameInThrough);
		}
		if (path.Count < 1)
		{
			return -1;
		}
		foreach (WorldCoordinate item in path)
		{
			if (item.room == evaluateRoom && (item.abstractNode == theDoorICameInThrough || world.GetAbstractRoom(evaluateRoom).ConnectionPossible(theDoorICameInThrough, item.abstractNode, parent.creatureTemplate)))
			{
				return item.abstractNode;
			}
		}
		return -1;
	}

	public virtual void Update(int time)
	{
		timeBuffer += time;
		if (parent.Room.realizedRoom == null)
		{
			AbstractBehavior(time);
			if (destination.CompareDisregardingTile(parent.pos) && !DoIHaveAPathToCoordinate(destination))
			{
				Custom.Log("abstract", parent.creatureTemplate.name, "has a destination but no path to it.");
				Custom.Log($"position {world.GetAbstractRoom(parent.pos).name} n:{parent.pos.abstractNode} dest:{world.GetAbstractRoom(destination).name} n: {destination.abstractNode}");
				SetDestination(parent.pos);
				migrationDestination = null;
			}
		}
	}

	public virtual void AbstractBehavior(int time)
	{
		if (parent.realizedCreature != null)
		{
			MigrationBehavior(time);
			return;
		}
		bool flag = false;
		if (parent.creatureTemplate.stowFoodInDen && denPosition.HasValue && destination != denPosition.Value)
		{
			for (int i = 0; i < parent.stuckObjects.Count; i++)
			{
				if (parent.stuckObjects[i].B is AbstractCreature && parent.creatureTemplate.CreatureRelationship((parent.stuckObjects[i].B as AbstractCreature).creatureTemplate).type == CreatureTemplate.Relationship.Type.Eats)
				{
					GoToDen();
					flag = true;
				}
			}
		}
		if (followCreature != null && !flag)
		{
			MoveWithCreature(followCreature, goToCreatureDestination: false);
		}
		if (path.Count > 0)
		{
			FollowPath(time);
		}
		else if (!MigrationBehavior(time) && TimeInfluencedRandomRoll(parent.creatureTemplate.roamInRoomChance, time))
		{
			RandomMoveWithinRoom();
		}
		if (!ModManager.Expedition || !parent.world.game.rainWorld.ExpeditionMode || !ExpeditionGame.activeUnlocks.Contains("bur-hunted") || !(parent.world.rainCycle.CycleProgression > 0.05f) || parent.world.game.Players == null)
		{
			return;
		}
		for (int j = 0; j < parent.world.game.Players.Count; j++)
		{
			AbstractCreature abstractCreature = parent.world.game.Players[j];
			if (abstractCreature.realizedCreature != null && !(abstractCreature.realizedCreature as Player).dead)
			{
				if (abstractCreature.pos.NodeDefined && abstractCreature.Room.nodes[abstractCreature.pos.abstractNode].type.Index != -1 && parent.creatureTemplate.mappedNodeTypes[abstractCreature.Room.nodes[abstractCreature.pos.abstractNode].type.Index])
				{
					SetDestination(abstractCreature.pos);
				}
				break;
			}
		}
	}

	protected virtual bool MigrationBehavior(int time)
	{
		if (followCreature != null)
		{
			if (!followCreature.state.dead && !followCreature.slatedForDeletion)
			{
				MoveWithCreature(followCreature, goToCreatureDestination: false);
				if (parent.state.socialMemory != null && parent.state.socialMemory.relationShips.Count > 0)
				{
					SocialMemory.Relationship relationship = parent.state.socialMemory.GetRelationship(followCreature.ID);
					if (relationship != null && Mathf.Abs(relationship.like) < 0.9f)
					{
						Custom.Log($"{parent} stopped following {followCreature}");
						followCreature = null;
					}
				}
				return true;
			}
			parent.state.socialMemory.DiscardRelationship(followCreature.ID);
			followCreature = null;
		}
		else if (parent.state.socialMemory != null && parent.state.socialMemory.relationShips.Count > 0)
		{
			SocialMemory.Relationship relationship2 = parent.state.socialMemory.relationShips[UnityEngine.Random.Range(0, parent.state.socialMemory.relationShips.Count)];
			if (Mathf.Abs(relationship2.like) > 0.95f)
			{
				for (int i = 0; i < world.NumberOfRooms; i++)
				{
					for (int j = 0; j < world.GetAbstractRoom(i + world.firstRoomIndex).creatures.Count; j++)
					{
						if (world.GetAbstractRoom(i + world.firstRoomIndex).creatures[j].ID == relationship2.subjectID)
						{
							followCreature = world.GetAbstractRoom(i + world.firstRoomIndex).creatures[j];
							Custom.Log($"{parent} following creature {followCreature}");
							return true;
						}
					}
				}
			}
		}
		float num = parent.creatureTemplate.roamBetweenRoomsChance;
		if (ModManager.MSC && parent.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC && parent.abstractAI is SlugNPCAbstractAI && (parent.abstractAI as SlugNPCAbstractAI).isTamed)
		{
			num = -1f;
		}
		if (MigrationDestination.room == parent.pos.room && (world.GetAttractionForRoom(parent.pos, parent.creatureTemplate.type) == AbstractRoom.CreatureRoomAttraction.Forbidden || (parent.timeSpentHere > UnityEngine.Random.Range(0, parent.creatureTemplate.abstractedLaziness) && TimeInfluencedRandomRoll(num * Mathf.Lerp(2f, 0.1f, world.GetAbstractRoom(parent.pos).AttractionValueForCreature(parent.creatureTemplate.type)), time))))
		{
			if (UnityEngine.Random.value < 0.5f)
			{
				RandomMoveToOtherRoom((int)(250f * Mathf.Pow(UnityEngine.Random.value, 0.25f)));
			}
			else
			{
				MoveToMoreAttractiveNeighborRoom();
			}
			return true;
		}
		return false;
	}

	protected void MoveWithCreature(AbstractCreature crit, bool goToCreatureDestination)
	{
		WorldCoordinate pos = crit.pos;
		if (goToCreatureDestination && crit.abstractAI != null)
		{
			pos = crit.abstractAI.destination;
		}
		if (pos.room != destination.room || pos.room != MigrationDestination.room)
		{
			TryToGoToRoom(pos);
		}
	}

	protected void TryToGoToRoom(WorldCoordinate coord)
	{
		AbstractRoom abstractRoom = world.GetAbstractRoom(coord);
		if (abstractRoom == null)
		{
			return;
		}
		if (coord.NodeDefined && abstractRoom.nodes[coord.abstractNode].type.Index != -1 && parent.creatureTemplate.mappedNodeTypes[abstractRoom.nodes[coord.abstractNode].type.Index])
		{
			InternalSetDestination(coord);
			return;
		}
		List<WorldCoordinate> list = new List<WorldCoordinate>();
		for (int i = 0; i < abstractRoom.nodes.Length; i++)
		{
			if (abstractRoom.nodes[i].type.Index != -1 && parent.creatureTemplate.mappedNodeTypes[abstractRoom.nodes[i].type.Index])
			{
				list.Add(new WorldCoordinate(coord.room, -1, -1, i));
			}
		}
		if (list.Count > 0)
		{
			InternalSetDestination(list[UnityEngine.Random.Range(0, list.Count)]);
		}
	}

	protected bool TimeInfluencedRandomRoll(float stat, int time)
	{
		for (int i = 0; i < time; i++)
		{
			if (UnityEngine.Random.value < stat)
			{
				return true;
			}
		}
		return false;
	}

	public void FollowPath(int time)
	{
		if (parent.creatureTemplate.abstractImmobile)
		{
			return;
		}
		if (parent.pos.NodeDefined && parent.distanceToMyNode > -1 && timeBuffer > (int)((float)parent.distanceToMyNode / (parent.creatureTemplate.offScreenSpeed * offscreenSpeedFac)))
		{
			if (path.Count > 0)
			{
				if (path[path.Count - 1] == parent.pos)
				{
					path.RemoveAt(path.Count - 1);
					if (path.Count == 0)
					{
						return;
					}
				}
				int num = ((path[path.Count - 1].room == parent.pos.room) ? world.GetAbstractRoom(parent.pos.room).ConnectionLength(parent.pos.abstractNode, path[path.Count - 1].abstractNode, parent.creatureTemplate) : ((world.GetAbstractRoom(parent.pos.room).nodes[parent.pos.abstractNode].type == AbstractRoomNode.Type.Exit || world.GetAbstractRoom(parent.pos.room).nodes[parent.pos.abstractNode].type == AbstractRoomNode.Type.Den) ? world.TotalShortCutLengthBetweenTwoConnectedRooms(parent.pos.room, path[path.Count - 1].room) : ((world.GetAbstractRoom(parent.pos.room).nodes[parent.pos.abstractNode].type == AbstractRoomNode.Type.SkyExit) ? world.SkyHighwayDistanceBetweenNodes(parent.pos, path[path.Count - 1]) : ((world.GetAbstractRoom(parent.pos.room).nodes[parent.pos.abstractNode].type == AbstractRoomNode.Type.SeaExit) ? world.SeaHighwayDistanceBetweenNodes(parent.pos, path[path.Count - 1]) : 0))));
				if ((float)timeBuffer > (float)num / (parent.creatureTemplate.offScreenSpeed * offscreenSpeedFac))
				{
					parent.Move(path[path.Count - 1]);
					timeBuffer -= (int)((float)num / (parent.creatureTemplate.offScreenSpeed * offscreenSpeedFac));
				}
			}
			if (world.GetNode(parent.pos).type == AbstractRoomNode.Type.Den)
			{
				parent.OpportunityToEnterDen(parent.pos);
			}
		}
		if (parent.pos.NodeDefined && (parent.distanceToMyNode >= 0 || timeBuffer <= 200 || !TimeInfluencedRandomRoll(0.5f, time)))
		{
			return;
		}
		List<int> list = new List<int>();
		for (int i = 0; i < parent.Room.nodes.Length; i++)
		{
			if (parent.Room.nodes[i].type.Index != -1 && parent.creatureTemplate.mappedNodeTypes[parent.Room.nodes[i].type.Index] && (!parent.pos.NodeDefined || parent.Room.ConnectionPossible(parent.pos.abstractNode, i, parent.creatureTemplate)))
			{
				list.Add(i);
			}
		}
		if (list.Count > 0)
		{
			parent.Move(new WorldCoordinate(parent.pos.room, parent.pos.x, parent.pos.y, list[UnityEngine.Random.Range(0, list.Count)]));
			SetDestination(parent.pos);
			Custom.Log($"Abstr Stuck {parent.creatureTemplate.type} moved to random accessible node {parent.pos}");
			parent.distanceToMyNode = 0;
		}
	}

	protected void RandomMoveWithinRoom()
	{
		AbstractRoom abstractRoom = world.GetAbstractRoom(parent.pos.room);
		if (abstractRoom.nodes.Length != 0)
		{
			int num = UnityEngine.Random.Range(0, abstractRoom.nodes.Length);
			if (num != parent.pos.abstractNode && abstractRoom.ConnectionAndBackPossible(parent.pos.abstractNode, num, parent.creatureTemplate))
			{
				InternalSetDestination(new WorldCoordinate(abstractRoom.index, -1, -1, num));
				path = new List<WorldCoordinate> { destination };
			}
		}
	}

	protected void RandomMoveToOtherRoom(int maxRoamDistance)
	{
		if (world.GetAbstractRoom(parent.pos).connections.Length < 1)
		{
			return;
		}
		WorldCoordinate worldCoordinate = QuickConnectivity.DefineNodeOfLocalCoordinate(parent.pos, world, parent.creatureTemplate);
		List<WorldCoordinate> list = new List<WorldCoordinate>();
		int num = 0;
		for (int i = 0; i < 100; i++)
		{
			AbstractRoom abstractRoom = world.GetAbstractRoom(worldCoordinate.room);
			List<WorldCoordinate> list2 = new List<WorldCoordinate>();
			for (int j = 0; j < abstractRoom.connections.Length; j++)
			{
				if (abstractRoom.connections[j] <= -1)
				{
					continue;
				}
				bool flag = CanRoamThroughRoom(abstractRoom.connections[j]);
				for (int k = 0; k < list.Count && flag; k++)
				{
					if (list[k].room == abstractRoom.connections[j])
					{
						flag = false;
					}
				}
				if (flag)
				{
					WorldCoordinate worldCoordinate2 = new WorldCoordinate(abstractRoom.connections[j], -1, -1, world.GetAbstractRoom(abstractRoom.connections[j]).ExitIndex(abstractRoom.index));
					if (abstractRoom.ConnectionAndBackPossible(worldCoordinate.abstractNode, j, parent.creatureTemplate) && parent.creatureTemplate.AbstractSubmersionLegal(world.GetNode(worldCoordinate2).submerged))
					{
						list2.Add(worldCoordinate2);
					}
				}
			}
			if (list2.Count <= 0)
			{
				break;
			}
			WorldCoordinate worldCoordinate3 = list2[UnityEngine.Random.Range(0, list2.Count)];
			float num2 = 0f;
			for (int l = 0; l < list2.Count; l++)
			{
				num2 += world.GetAbstractRoom(list2[l]).SizeDependentAttractionValueForCreature(parent.creatureTemplate.type);
			}
			float num3 = UnityEngine.Random.value * num2;
			for (int m = 0; m < list2.Count; m++)
			{
				float num4 = world.GetAbstractRoom(list2[m]).SizeDependentAttractionValueForCreature(parent.creatureTemplate.type);
				if (num3 < num4)
				{
					worldCoordinate3 = list2[m];
					break;
				}
				num3 -= num4;
			}
			list.Insert(0, new WorldCoordinate(abstractRoom.index, -1, -1, abstractRoom.ExitIndex(worldCoordinate3.room)));
			list.Insert(0, worldCoordinate3);
			num += ((worldCoordinate.abstractNode != abstractRoom.ExitIndex(worldCoordinate3.room)) ? abstractRoom.nodes[worldCoordinate.abstractNode].ConnectionLength(abstractRoom.ExitIndex(worldCoordinate3.room), parent.creatureTemplate) : 0);
			if (num > maxRoamDistance)
			{
				break;
			}
			worldCoordinate = worldCoordinate3;
		}
		if (list.Count > 0 && world.GetAbstractRoom(list[0]).SizeDependentAttractionValueForCreature(parent.creatureTemplate.type) < world.GetAbstractRoom(parent.pos).SizeDependentAttractionValueForCreature(parent.creatureTemplate.type))
		{
			while (list.Count > 1 && (list[0].room == list[1].room || world.GetAbstractRoom(list[0]).SizeDependentAttractionValueForCreature(parent.creatureTemplate.type) < world.GetAbstractRoom(list[1]).SizeDependentAttractionValueForCreature(parent.creatureTemplate.type)))
			{
				list.RemoveAt(0);
			}
		}
		if (list.Count > 0)
		{
			path = list;
			InternalSetDestination(list[0]);
		}
	}

	public void MoveToMoreAttractiveNeighborRoom()
	{
		if (world.GetAbstractRoom(parent.pos).AttractionForCreature(parent.creatureTemplate.type) == AbstractRoom.CreatureRoomAttraction.Stay)
		{
			return;
		}
		List<float> list = new List<float>();
		float num = 0f;
		List<int> list2 = new List<int>();
		float num2 = world.GetAbstractRoom(parent.pos).SizeDependentAttractionValueForCreature(parent.creatureTemplate.type);
		float num3 = world.GetAbstractRoom(parent.pos).AttractionValueForCreature(parent.creatureTemplate.type);
		for (int i = 0; i < parent.Room.connections.Length; i++)
		{
			if (parent.Room.connections[i] > -1 && (parent.pos.abstractNode == i || parent.Room.ConnectionPossible(parent.pos.abstractNode, i, parent.creatureTemplate)) && CanRoamThroughRoom(parent.Room.connections[i]) && (UnityEngine.Random.value < 0.025f || world.GetAbstractRoom(parent.Room.connections[i]).AttractionValueForCreature(parent.creatureTemplate.type) > num3))
			{
				float num4 = world.GetAbstractRoom(parent.Room.connections[i]).SizeDependentAttractionValueForCreature(parent.creatureTemplate.type);
				if (num4 < num2)
				{
					num4 /= 10f;
				}
				num += num4;
				list.Add(num4);
				list2.Add(parent.Room.connections[i]);
			}
		}
		float num5 = num * UnityEngine.Random.value;
		for (int j = 0; j < list2.Count; j++)
		{
			if (num5 < list[j])
			{
				InternalSetDestination(new WorldCoordinate(list2[j], -1, -1, world.GetAbstractRoom(list2[j]).ExitIndex(parent.pos.room)));
				break;
			}
			num5 -= list[j];
		}
	}

	public virtual bool CanRoamThroughRoom(int room)
	{
		if (world.GetAbstractRoom(room).AttractionForCreature(parent.creatureTemplate.type) == AbstractRoom.CreatureRoomAttraction.Forbidden)
		{
			return false;
		}
		return true;
	}

	public virtual void Moved()
	{
		for (int num = path.Count - 1; num >= 0; num--)
		{
			if (path[num].room == parent.pos.room && path[num].abstractNode == parent.pos.abstractNode)
			{
				for (int num2 = path.Count - 1; num2 >= num; num2--)
				{
					path.RemoveAt(num2);
				}
				break;
			}
		}
		if (migrationDestination.HasValue && parent.pos.room == migrationDestination.Value.room)
		{
			migrationDestination = null;
		}
	}

	public bool WantToStayInDenUntilEndOfCycle()
	{
		if (RealAI != null)
		{
			return RealAI.WantToStayInDenUntilEndOfCycle();
		}
		if ((!ModManager.MSC || !world.game.IsArenaSession || world.game.GetArenaGameSession.arenaSitting.gameTypeSetup.gameType != MoreSlugcatsEnums.GameTypeID.Challenge) && !parent.nightCreature && !parent.ignoreCycle && world.rainCycle.TimeUntilRain < (world.game.IsStorySession ? 60 : 15) * 40)
		{
			return true;
		}
		if (parent.state.dead)
		{
			return true;
		}
		if (parent.creatureTemplate.TopAncestor().type == CreatureTemplate.Type.LizardTemplate && (parent.state as LizardState).health < 0.5f)
		{
			return true;
		}
		if (parent.state is HealthState && (parent.state as HealthState).health < 0.75f)
		{
			return true;
		}
		return false;
	}

	public virtual void Die()
	{
	}

	public virtual bool DoIwantToDropThisItemInDen(AbstractPhysicalObject item)
	{
		return true;
	}

	private WorldCoordinate RandomizeSpawnPositionInRoom(WorldCoordinate spawnPos, int repeats)
	{
		repeats = Math.Min(repeats, 500);
		Room realizedRoom = world.GetAbstractRoom(parent.pos).realizedRoom;
		Custom.Log(parent.creatureTemplate.name, "Random movement for", repeats.ToString(), "ticks.");
		IntVector2 intVector = new IntVector2(UnityEngine.Random.Range(0, realizedRoom.TileWidth), UnityEngine.Random.Range(0, realizedRoom.TileHeight));
		for (int i = 0; i < repeats; i++)
		{
			AItile aItile = realizedRoom.aimap.getAItile(spawnPos.Tile);
			MovementConnection movementConnection = default(MovementConnection);
			float num = float.MaxValue;
			for (int j = 0; j < aItile.outgoingPaths.Count; j++)
			{
				if (aItile.outgoingPaths.Count == 0)
				{
					break;
				}
				MovementConnection movementConnection2 = aItile.outgoingPaths[UnityEngine.Random.Range(0, aItile.outgoingPaths.Count)];
				if (!(intVector.FloatDist(movementConnection2.DestTile) < num) || !realizedRoom.aimap.IsConnectionAllowedForCreature(movementConnection2, parent.creatureTemplate))
				{
					continue;
				}
				for (int k = 0; k < realizedRoom.aimap.getAItile(movementConnection2.DestTile).outgoingPaths.Count; k++)
				{
					if (parent.creatureTemplate.ConnectionResistance(realizedRoom.aimap.getAItile(movementConnection2.DestTile).outgoingPaths[k].type).Allowed && realizedRoom.aimap.getAItile(movementConnection2.DestTile).outgoingPaths[k].DestTile == spawnPos.Tile)
					{
						movementConnection = movementConnection2;
						num = intVector.FloatDist(movementConnection2.DestTile);
						break;
					}
				}
			}
			if (movementConnection != default(MovementConnection))
			{
				spawnPos.Tile = movementConnection.DestTile;
			}
		}
		return spawnPos;
	}

	public bool HavePrey()
	{
		for (int i = 0; i < parent.stuckObjects.Count; i++)
		{
			if (parent.stuckObjects[i].A == parent && parent.stuckObjects[i].B is AbstractCreature && parent.creatureTemplate.CreatureRelationship((parent.stuckObjects[i].B as AbstractCreature).creatureTemplate).type == CreatureTemplate.Relationship.Type.Eats)
			{
				return true;
			}
		}
		return false;
	}
}
