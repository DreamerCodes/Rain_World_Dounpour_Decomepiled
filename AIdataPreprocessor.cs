using System;
using System.Collections.Generic;
using System.Linq;
using RWCustom;
using UnityEngine;

internal class AIdataPreprocessor
{
	private class SubRoutine
	{
		protected AImap aiMap;

		protected Visualizer visualizer;

		public bool done;

		public SubRoutine(AImap aiMap, Visualizer visualizer)
		{
			this.aiMap = aiMap;
			this.visualizer = visualizer;
		}

		public virtual void Update()
		{
		}

		public void Finish()
		{
			done = true;
		}
	}

	private class DijkstraMapper : SubRoutine
	{
		public class Cell
		{
			public IntVector2 pos;

			public Cell parent;

			public int generation;

			public PathCost cost;

			public int x => pos.x;

			public int y => pos.y;

			public Cell(IntVector2 pos, Cell parent, int generation, PathCost cost)
			{
				this.pos = pos;
				this.parent = parent;
				this.generation = generation;
				this.cost = cost;
			}
		}

		protected List<IntVector2> procreateNextRound;

		protected CreatureTemplate crit;

		protected Cell[,] cellGrid;

		public DijkstraMapper(AImap aiMap, CreatureTemplate crit, Visualizer visualizer)
			: base(aiMap, visualizer)
		{
			this.crit = crit;
			visualizer?.InitWithCrit(crit);
		}

		protected virtual void AddCell(IntVector2 pos, Cell parent, PathCost cost, int generation)
		{
			Cell cell = new Cell(pos, parent, generation, cost);
			if (cellGrid[pos.x, pos.y] != null && cellGrid[pos.x, pos.y].cost < cell.cost)
			{
				return;
			}
			if (cellGrid[pos.x, pos.y] != null)
			{
				cellGrid[pos.x, pos.y].parent = cell.parent;
				cellGrid[pos.x, pos.y].cost = cell.cost;
				cellGrid[pos.x, pos.y].generation = cell.generation;
				if (procreateNextRound.IndexOf(pos) == -1)
				{
					procreateNextRound.Add(pos);
				}
			}
			else
			{
				cellGrid[pos.x, pos.y] = cell;
				procreateNextRound.Add(pos);
			}
			if (visualizer != null)
			{
				visualizer.ColorCell(pos, 1, cellGrid);
			}
		}
	}

	private class NodeMapper : DijkstraMapper
	{
		private int currentNodeForCreature;

		private ConnectivityMapper connectivityMapper;

		private int currentNodeCommon => CreatureSpecificToCommonNodeIndex(aiMap.room, currentNodeForCreature, crit);

		public NodeMapper(AImap aiMap, CreatureTemplate crit, int currentNodeForCreature, ConnectivityMapper connectivityMapper, Visualizer visualizer, bool dontBake)
			: base(aiMap, crit, visualizer)
		{
			this.currentNodeForCreature = currentNodeForCreature;
			this.connectivityMapper = connectivityMapper;
			IntVector2[] array;
			if (currentNodeCommon >= aiMap.room.exitAndDenIndex.Length)
			{
				array = ((currentNodeCommon >= aiMap.room.exitAndDenIndex.Length + aiMap.room.borderExits.Length) ? aiMap.room.hives[currentNodeCommon - aiMap.room.exitAndDenIndex.Length - aiMap.room.borderExits.Length] : aiMap.room.borderExits[currentNodeCommon - aiMap.room.exitAndDenIndex.Length].borderTiles);
			}
			else
			{
				array = new IntVector2[1] { aiMap.room.ShortcutLeadingToNode(currentNodeCommon).StartTile };
				if (visualizer == null)
				{
				}
			}
			procreateNextRound = new List<IntVector2>();
			cellGrid = new Cell[aiMap.width, aiMap.height];
			for (int i = 0; i < array.Length; i++)
			{
				if (aiMap.TileAccessibleToCreature(array[i], crit))
				{
					AddCell(array[i], null, new PathCost(0f, PathCost.Legality.Allowed), 0);
				}
			}
			if (dontBake)
			{
				procreateNextRound.Clear();
				Finish();
			}
		}

		public override void Update()
		{
			if (procreateNextRound.Count > 0)
			{
				Cell cell = null;
				PathCost pathCost = new PathCost(0f, PathCost.Legality.Unallowed);
				for (int i = 0; i < procreateNextRound.Count; i++)
				{
					Cell cell2 = cellGrid[procreateNextRound[i].x, procreateNextRound[i].y];
					if (cell2.cost < pathCost)
					{
						cell = cell2;
						pathCost = cell2.cost;
					}
				}
				procreateNextRound.Remove(cell.pos);
				{
					foreach (MovementConnection incomingPath in aiMap.getAItile(cell.pos).incomingPaths)
					{
						if (aiMap.IsConnectionAllowedForCreature(incomingPath, crit))
						{
							AddCell(incomingPath.StartTile, cell, cell.cost + aiMap.ConnectionCostForCreature(incomingPath, crit), 0);
						}
					}
					return;
				}
			}
			Finish();
		}

		public void WriteDijkstraMap(CreatureSpecificAImap m, int n)
		{
			for (int i = 0; i < aiMap.width; i++)
			{
				for (int j = 0; j < aiMap.height; j++)
				{
					if (cellGrid[i, j] != null)
					{
						m.SetDistanceToExit(i, j, n, cellGrid[i, j].generation);
					}
				}
			}
		}

		protected override void AddCell(IntVector2 pos, Cell parent, PathCost cost, int generation)
		{
			base.AddCell(pos, parent, cost, (parent == null) ? 1 : (parent.generation + 1));
			connectivityMapper.TileDiscovered(currentNodeCommon, pos, crit, cost, (parent == null) ? 1 : (parent.generation + 1));
		}
	}

	private class AccessibilityDijkstraMapper : DijkstraMapper
	{
		private int gen;

		public AccessibilityDijkstraMapper(AImap aiMap, CreatureTemplate crit, Visualizer visualizer, bool dontBake)
			: base(aiMap, crit, visualizer)
		{
			cellGrid = new Cell[aiMap.width, aiMap.height];
			procreateNextRound = new List<IntVector2>();
			for (int i = 0; i < aiMap.room.TileWidth; i++)
			{
				for (int j = 0; j < aiMap.room.TileWidth; j++)
				{
					if (aiMap.AnyExitReachableFromTile(new IntVector2(i, j), crit))
					{
						float resistance = new IntVector2(i, j).FloatDist(new IntVector2(aiMap.room.TileWidth / 2, aiMap.room.TileHeight / 2)) / new IntVector2(0, 0).FloatDist(new IntVector2(aiMap.room.TileWidth / 2, aiMap.room.TileHeight / 2));
						AddCell(new IntVector2(i, j), null, new PathCost(resistance, PathCost.Legality.Allowed), 0);
						procreateNextRound.Add(new IntVector2(i, j));
					}
				}
			}
			if (dontBake)
			{
				procreateNextRound.Clear();
				Finish();
			}
		}

		public override void Update()
		{
			if (procreateNextRound.Count > 0)
			{
				Cell cell = null;
				PathCost pathCost = new PathCost(float.MaxValue, PathCost.Legality.Unallowed);
				IntVector2 pos = new IntVector2(-1, -1);
				for (int num = procreateNextRound.Count - 1; num >= 0; num--)
				{
					Cell cell2 = cellGrid[procreateNextRound[num].x, procreateNextRound[num].y];
					bool flag = true;
					foreach (MovementConnection outgoingPath in aiMap.getAItile(cell2.pos).outgoingPaths)
					{
						PathCost pathCost2 = cell2.cost + aiMap.ConnectionCostForCreature(outgoingPath, crit);
						if (cellGrid[outgoingPath.destinationCoord.x, outgoingPath.destinationCoord.y] == null || cellGrid[outgoingPath.destinationCoord.x, outgoingPath.destinationCoord.y].cost > pathCost2)
						{
							flag = false;
							if (pathCost2 < pathCost)
							{
								cell = cell2;
								pathCost = pathCost2;
								pos = outgoingPath.DestTile;
							}
						}
					}
					if (flag)
					{
						procreateNextRound.RemoveAt(num);
					}
				}
				if (cell != null)
				{
					gen++;
					AddCell(pos, cell, pathCost, gen);
				}
				else
				{
					procreateNextRound.Clear();
					Finish();
				}
			}
			else
			{
				Finish();
			}
		}

		public void WriteAccessibilityMap(CreatureSpecificAImap m)
		{
			for (int i = 0; i < aiMap.width; i++)
			{
				for (int j = 0; j < aiMap.height; j++)
				{
					if (cellGrid[i, j] != null)
					{
						m.SetAccessibility(i, j, 1f - (float)cellGrid[i, j].generation / (float)gen);
					}
				}
			}
		}

		protected override void AddCell(IntVector2 pos, Cell parent, PathCost cost, int generation)
		{
			base.AddCell(pos, parent, cost, gen);
		}
	}

	private class VisibilityMapper : SubRoutine
	{
		private int x;

		private int y;

		private int[,] grid;

		private int horizontalMargin;

		private int topMargin;

		private int bottomMargin;

		public VisibilityMapper(AImap aiMap, Visualizer visualizer, bool dontBake)
			: base(aiMap, visualizer)
		{
			grid = new int[aiMap.room.TileWidth, aiMap.room.TileHeight];
			if (aiMap.room.borderExits.Length != 0)
			{
				horizontalMargin = 10;
				topMargin = 20;
				bottomMargin = 5;
			}
			visualizer?.InitNoCrit();
			if (dontBake)
			{
				Finish();
			}
		}

		public override void Update()
		{
			if (done)
			{
				return;
			}
			if (!aiMap.room.GetTile(x, y).Solid)
			{
				VisitTile();
			}
			x++;
			if (x >= aiMap.room.TileWidth)
			{
				x = 0;
				y++;
				if (y >= aiMap.room.TileHeight)
				{
					Finish();
				}
			}
		}

		private void VisitTile()
		{
			int num = 0;
			for (int i = Math.Max(x - 50, -horizontalMargin); i < Math.Min(x + 50, aiMap.room.TileWidth + horizontalMargin); i++)
			{
				for (int j = Math.Max(y - 50, -bottomMargin); j < Math.Min(y + 50, aiMap.room.TileHeight + topMargin); j++)
				{
					if (aiMap.room.RayTraceTilesForTerrain(x, y, i, j))
					{
						num++;
					}
				}
			}
			grid[x, y] = num;
			if (visualizer != null)
			{
				visualizer.ColorCell(new IntVector2(x, y), grid);
			}
		}

		public void WriteVisibilityMap()
		{
			for (int i = 0; i < aiMap.room.TileWidth; i++)
			{
				for (int j = 0; j < aiMap.room.TileHeight; j++)
				{
					aiMap.getAItile(i, j).visibility = grid[i, j];
				}
			}
		}
	}

	private class ConnectivityMapper
	{
		public int[,,,] connectivityMap;

		public List<ShortcutData> shortCutsInNodeOrder;

		private int roomNodes;

		private Room room;

		private int exitNodes;

		private int denNodes;

		public ConnectivityMapper(int roomNodes, Room room)
		{
			this.roomNodes = roomNodes;
			this.room = room;
			connectivityMap = new int[StaticWorld.preBakedPathingCreatures.Length, roomNodes, roomNodes, 2];
			for (int i = 0; i < StaticWorld.preBakedPathingCreatures.Length; i++)
			{
				for (int j = 0; j < roomNodes; j++)
				{
					for (int k = 0; k < roomNodes; k++)
					{
						for (int l = 0; l < 2; l++)
						{
							connectivityMap[i, j, k, l] = -1;
						}
					}
				}
			}
			List<ShortcutData> list = new List<ShortcutData>();
			for (int m = 0; m < room.shortcuts.Length; m++)
			{
				if (room.shortcuts[m].shortCutType == ShortcutData.Type.RoomExit)
				{
					list.Add(room.shortcuts[m]);
					exitNodes++;
				}
				else if (room.shortcuts[m].shortCutType == ShortcutData.Type.CreatureHole)
				{
					list.Add(room.shortcuts[m]);
					denNodes++;
				}
				else if (room.shortcuts[m].shortCutType == ShortcutData.Type.RegionTransportation)
				{
					list.Add(room.shortcuts[m]);
				}
			}
			IEnumerable<ShortcutData> source = list.OrderBy((ShortcutData pet) => pet.destNode);
			shortCutsInNodeOrder = source.ToList();
			_ = shortCutsInNodeOrder.Count;
			_ = room.exitAndDenIndex.Length;
			for (int n = 0; n < shortCutsInNodeOrder.Count; n++)
			{
				_ = shortCutsInNodeOrder[n].destNode;
			}
		}

		public void TileDiscovered(int b, IntVector2 tile, CreatureTemplate crit, PathCost cost, int generation)
		{
			for (int i = 0; i < shortCutsInNodeOrder.Count; i++)
			{
				if (shortCutsInNodeOrder[i].startCoord.x == tile.x && shortCutsInNodeOrder[i].startCoord.y == tile.y)
				{
					if (i != b && cost.Allowed && ((crit.mappedNodeTypes[(int)AbstractRoomNode.Type.Exit] && shortCutsInNodeOrder[i].shortCutType == ShortcutData.Type.RoomExit) || (crit.mappedNodeTypes[(int)AbstractRoomNode.Type.Den] && shortCutsInNodeOrder[i].shortCutType == ShortcutData.Type.CreatureHole) || (crit.mappedNodeTypes[(int)AbstractRoomNode.Type.RegionTransportation] && shortCutsInNodeOrder[i].shortCutType == ShortcutData.Type.RegionTransportation)))
					{
						connectivityMap[StaticWorld.preBakedPathingCreatures.IndexOf(crit), i, b, 0] = (int)cost.resistance;
						connectivityMap[StaticWorld.preBakedPathingCreatures.IndexOf(crit), i, b, 1] = generation;
					}
					break;
				}
			}
			if (crit.mappedNodeTypes[(int)AbstractRoomNode.Type.SeaExit] || crit.mappedNodeTypes[(int)AbstractRoomNode.Type.SkyExit] || crit.mappedNodeTypes[(int)AbstractRoomNode.Type.SideExit])
			{
				for (int j = 0; j < room.borderExits.Length; j++)
				{
					if (room.borderExits[j].type.Index == -1 || !crit.mappedNodeTypes[room.borderExits[j].type.Index])
					{
						continue;
					}
					int num = shortCutsInNodeOrder.Count + j;
					if (connectivityMap[StaticWorld.preBakedPathingCreatures.IndexOf(crit), num, b, 1] != -1)
					{
						continue;
					}
					for (int k = 0; k < room.borderExits[j].borderTiles.Length; k++)
					{
						if (tile == room.borderExits[j].borderTiles[k] && num != b && cost.Allowed)
						{
							connectivityMap[StaticWorld.preBakedPathingCreatures.IndexOf(crit), num, b, 0] = (int)cost.resistance;
							connectivityMap[StaticWorld.preBakedPathingCreatures.IndexOf(crit), num, b, 1] = generation;
							break;
						}
					}
				}
			}
			if (!crit.mappedNodeTypes[(int)AbstractRoomNode.Type.BatHive])
			{
				return;
			}
			for (int l = 0; l < room.hives.Length; l++)
			{
				int num2 = shortCutsInNodeOrder.Count + room.borderExits.Length + l;
				if (connectivityMap[StaticWorld.preBakedPathingCreatures.IndexOf(crit), num2, b, 1] != -1)
				{
					continue;
				}
				for (int m = 0; m < room.hives[l].Length; m++)
				{
					if (tile == room.hives[l][m] && num2 != b && cost.Allowed)
					{
						connectivityMap[StaticWorld.preBakedPathingCreatures.IndexOf(crit), num2, b, 0] = (int)cost.resistance;
						connectivityMap[StaticWorld.preBakedPathingCreatures.IndexOf(crit), num2, b, 1] = generation;
						break;
					}
				}
			}
		}

		public AbstractRoomNode[] ReturnNodes()
		{
			AbstractRoomNode[] array = new AbstractRoomNode[roomNodes];
			for (int i = 0; i < roomNodes; i++)
			{
				AbstractRoomNode.Type exit = AbstractRoomNode.Type.Exit;
				exit = ((i < exitNodes) ? AbstractRoomNode.Type.Exit : ((i < exitNodes + denNodes) ? AbstractRoomNode.Type.Den : ((i < shortCutsInNodeOrder.Count) ? AbstractRoomNode.Type.RegionTransportation : ((i < shortCutsInNodeOrder.Count + room.borderExits.Length) ? room.borderExits[i - shortCutsInNodeOrder.Count].type : ((i >= shortCutsInNodeOrder.Count + room.borderExits.Length + room.hives.GetLength(0)) ? AbstractRoomNode.Type.GarbageHoles : AbstractRoomNode.Type.BatHive)))));
				int shortCutLength = -1;
				int viewedByCamera = -1;
				bool submerged = false;
				int entranceWidth = 1;
				if (exit == AbstractRoomNode.Type.Exit || exit == AbstractRoomNode.Type.Den)
				{
					shortCutLength = shortCutsInNodeOrder[i].length;
					viewedByCamera = room.CameraViewingNode(i);
					submerged = room.GetTile(shortCutsInNodeOrder[i].StartTile).AnyWater;
				}
				else if (exit == AbstractRoomNode.Type.SideExit || exit == AbstractRoomNode.Type.SkyExit || exit == AbstractRoomNode.Type.SeaExit)
				{
					submerged = room.borderExits[i - room.exitAndDenIndex.Length].type == AbstractRoomNode.Type.SeaExit;
					entranceWidth = room.borderExits[i - room.exitAndDenIndex.Length].borderTiles.Length;
				}
				array[i] = new AbstractRoomNode(exit, shortCutLength, roomNodes, submerged, viewedByCamera, entranceWidth);
				for (int j = 0; j < roomNodes; j++)
				{
					for (int k = 0; k < StaticWorld.preBakedPathingCreatures.Length; k++)
					{
						for (int l = 0; l < 2; l++)
						{
							array[i].connectivity[k, j, l] = connectivityMap[k, i, j, l];
						}
					}
				}
			}
			return array;
		}
	}

	private class Visualizer
	{
		private AImap aiMap;

		private DebugSprite[,] sprites;

		private int highest;

		public Visualizer(AImap aiMap)
		{
			this.aiMap = aiMap;
			sprites = new DebugSprite[aiMap.width, aiMap.height];
			for (int i = 0; i < aiMap.width; i++)
			{
				for (int j = 0; j < aiMap.height; j++)
				{
					FSprite sp = new FSprite("pixel")
					{
						scale = 22f,
						color = new Color(0.5f, 0.5f, 0.5f),
						alpha = 0f
					};
					sprites[i, j] = new DebugSprite(aiMap.room.MiddleOfTile(i, j), sp, aiMap.room);
					aiMap.room.AddObject(sprites[i, j]);
				}
			}
			highest = 0;
		}

		public void ColorCell(IntVector2 pos, int col, DijkstraMapper.Cell[,] cellGrid)
		{
			switch (col)
			{
			case 0:
				sprites[pos.x, pos.y].sprite.color = new Color(1f, 1f, 0f);
				break;
			case 1:
				sprites[pos.x, pos.y].sprite.color = GenerationColor(cellGrid[pos.x, pos.y].generation);
				if (cellGrid[pos.x, pos.y].generation >= highest)
				{
					UpdateAllWithNewHighest(cellGrid, cellGrid[pos.x, pos.y].generation);
				}
				break;
			case 2:
				sprites[pos.x, pos.y].sprite.color = new Color(1f, 0f, 1f);
				break;
			}
		}

		public void UpdateAllWithNewHighest(DijkstraMapper.Cell[,] cellGrid, int newHighest)
		{
			highest = newHighest;
			for (int i = 0; i < aiMap.width; i++)
			{
				for (int j = 0; j < aiMap.height; j++)
				{
					if (cellGrid[i, j] != null)
					{
						sprites[i, j].sprite.color = GenerationColor(cellGrid[i, j].generation);
					}
				}
			}
		}

		public void ColorCell(IntVector2 pos, int[,] intGrid)
		{
			sprites[pos.x, pos.y].sprite.color = GenerationColor(intGrid[pos.x, pos.y]);
			if (intGrid[pos.x, pos.y] >= highest)
			{
				UpdateAllWithNewHighest(intGrid, intGrid[pos.x, pos.y]);
			}
		}

		public void ColorCell(IntVector2 pos, Color col)
		{
			sprites[pos.x, pos.y].sprite.color = col;
		}

		public void UpdateAllWithNewHighest(int[,] intGrid, int newHighest)
		{
			highest = newHighest;
			for (int i = 0; i < aiMap.width; i++)
			{
				for (int j = 0; j < aiMap.height; j++)
				{
					sprites[i, j].sprite.color = GenerationColor(intGrid[i, j]);
				}
			}
		}

		private Color GenerationColor(int i)
		{
			return Custom.HSL2RGB((float)i / (float)highest * 0.7f, 1f, (i == 0) ? 0.2f : 0.5f);
		}

		public void InitWithCrit(CreatureTemplate crit)
		{
			for (int i = 0; i < aiMap.width; i++)
			{
				for (int j = 0; j < aiMap.height; j++)
				{
					sprites[i, j].sprite.alpha = 0.75f;
					if (aiMap.TileAccessibleToCreature(new IntVector2(i, j), crit))
					{
						sprites[i, j].sprite.color = new Color(0.5f, 0.5f, 0.5f);
					}
					else
					{
						sprites[i, j].sprite.color = new Color(0f, 0f, 0f);
					}
				}
			}
			highest = 0;
		}

		public void InitNoCrit()
		{
			for (int i = 0; i < aiMap.width; i++)
			{
				for (int j = 0; j < aiMap.height; j++)
				{
					sprites[i, j].sprite.alpha = 0.75f;
				}
			}
			highest = 0;
		}
	}

	private static readonly AGLog<AIdataPreprocessor> Log = new AGLog<AIdataPreprocessor>();

	private AImap aiMap;

	private SubRoutine currentSubRoutine;

	private int currentCreatureIndex;

	private int currentNode;

	private int roomNodes;

	private ConnectivityMapper connectivityMapper;

	public bool done;

	private bool viz;

	private Visualizer visualizer;

	public bool falseBake;

	public AIdataPreprocessor(AImap aiMap, bool falseBake)
	{
		this.aiMap = aiMap;
		this.falseBake = falseBake;
		if (viz)
		{
			visualizer = new Visualizer(aiMap);
		}
		aiMap.creatureSpecificAImaps = new CreatureSpecificAImap[StaticWorld.preBakedPathingCreatures.Length];
		roomNodes = TotalNodes(aiMap.room);
		for (int i = 0; i < StaticWorld.preBakedPathingCreatures.Length; i++)
		{
			aiMap.creatureSpecificAImaps[i] = new CreatureSpecificAImap(aiMap, StaticWorld.preBakedPathingCreatures[i]);
		}
		connectivityMapper = new ConnectivityMapper(roomNodes, aiMap.room);
		currentNode = -1;
		currentCreatureIndex = -1;
		currentSubRoutine = new VisibilityMapper(aiMap, visualizer, falseBake);
	}

	public void Update()
	{
		if (done)
		{
			return;
		}
		currentSubRoutine.Update();
		if (!currentSubRoutine.done)
		{
			return;
		}
		if (currentCreatureIndex == -1)
		{
			NodeDone();
			NextCreature();
			return;
		}
		NodeDone();
		currentNode++;
		if (currentNode < NodesRelevantToCreature(aiMap.room, StaticWorld.preBakedPathingCreatures[currentCreatureIndex]))
		{
			NextNode();
			return;
		}
		if (currentNode == NodesRelevantToCreature(aiMap.room, StaticWorld.preBakedPathingCreatures[currentCreatureIndex]))
		{
			MapAccessibility();
			return;
		}
		CreatureDone();
		NextCreature();
	}

	private void CreatureDone()
	{
		_ = viz;
		if (currentCreatureIndex >= StaticWorld.preBakedPathingCreatures.Length)
		{
			done = true;
		}
	}

	private void NextCreature()
	{
		currentCreatureIndex++;
		while (currentCreatureIndex < StaticWorld.preBakedPathingCreatures.Length && NodesRelevantToCreature(aiMap.room, StaticWorld.preBakedPathingCreatures[currentCreatureIndex]) == 0)
		{
			_ = viz;
			currentCreatureIndex++;
		}
		if (currentCreatureIndex >= StaticWorld.preBakedPathingCreatures.Length)
		{
			Finish();
			return;
		}
		_ = viz;
		if (viz)
		{
			visualizer.InitWithCrit(StaticWorld.preBakedPathingCreatures[currentCreatureIndex]);
		}
		currentNode = 0;
		NextNode();
	}

	private void NodeDone()
	{
		_ = viz;
		if (currentSubRoutine is NodeMapper)
		{
			(currentSubRoutine as NodeMapper).WriteDijkstraMap(aiMap.creatureSpecificAImaps[currentCreatureIndex], currentNode);
		}
		else if (currentSubRoutine is AccessibilityDijkstraMapper)
		{
			(currentSubRoutine as AccessibilityDijkstraMapper).WriteAccessibilityMap(aiMap.creatureSpecificAImaps[currentCreatureIndex]);
		}
		else if (currentSubRoutine is VisibilityMapper)
		{
			(currentSubRoutine as VisibilityMapper).WriteVisibilityMap();
		}
	}

	private void NextNode()
	{
		_ = viz;
		currentSubRoutine = new NodeMapper(aiMap, StaticWorld.preBakedPathingCreatures[currentCreatureIndex], currentNode, connectivityMapper, visualizer, falseBake);
		if (viz)
		{
			visualizer.InitWithCrit(StaticWorld.preBakedPathingCreatures[currentCreatureIndex]);
		}
	}

	private void MapAccessibility()
	{
		_ = viz;
		currentSubRoutine = new AccessibilityDijkstraMapper(aiMap, StaticWorld.preBakedPathingCreatures[currentCreatureIndex], visualizer, falseBake);
		if (viz)
		{
			visualizer.InitWithCrit(StaticWorld.preBakedPathingCreatures[currentCreatureIndex]);
		}
	}

	private void Finish()
	{
		done = true;
	}

	public AbstractRoomNode[] Connectivity()
	{
		return connectivityMapper.ReturnNodes();
	}

	public static int TotalNodes(Room room)
	{
		return room.exitAndDenIndex.Length + room.hives.Length + room.borderExits.Length + ((room.garbageHoles != null) ? 1 : 0);
	}

	public static int NodesRelevantToCreature(Room room, CreatureTemplate crit)
	{
		int num = 0;
		if (crit.mappedNodeTypes[(int)AbstractRoomNode.Type.Exit])
		{
			for (int i = 0; i < room.exitAndDenIndex.Length && room.ShortcutLeadingToNode(i).shortCutType == ShortcutData.Type.RoomExit; i++)
			{
				num++;
			}
		}
		if (crit.mappedNodeTypes[(int)AbstractRoomNode.Type.Den])
		{
			for (int j = 0; j < room.exitAndDenIndex.Length; j++)
			{
				if (room.ShortcutLeadingToNode(j).shortCutType == ShortcutData.Type.CreatureHole)
				{
					num++;
				}
			}
		}
		if (crit.mappedNodeTypes[(int)AbstractRoomNode.Type.RegionTransportation])
		{
			for (int k = 0; k < room.exitAndDenIndex.Length; k++)
			{
				if (room.ShortcutLeadingToNode(k).shortCutType == ShortcutData.Type.RegionTransportation)
				{
					num++;
				}
			}
		}
		for (int l = 0; l < room.borderExits.Length; l++)
		{
			if (room.borderExits[l].type.Index != -1 && crit.mappedNodeTypes[room.borderExits[l].type.Index])
			{
				num++;
			}
		}
		if (crit.mappedNodeTypes[(int)AbstractRoomNode.Type.BatHive])
		{
			num += room.hives.Length;
		}
		if (crit.mappedNodeTypes[(int)AbstractRoomNode.Type.GarbageHoles] && room.garbageHoles != null)
		{
			num++;
		}
		return num;
	}

	public static int CreatureSpecificToCommonNodeIndex(Room room, int n, CreatureTemplate crit)
	{
		int num = 0;
		if (crit.mappedNodeTypes[(int)AbstractRoomNode.Type.Exit])
		{
			for (int i = 0; i < room.exitAndDenIndex.Length; i++)
			{
				if (room.ShortcutLeadingToNode(i).shortCutType == ShortcutData.Type.RoomExit)
				{
					if (n == num)
					{
						return i;
					}
					num++;
				}
			}
		}
		if (crit.mappedNodeTypes[(int)AbstractRoomNode.Type.Den])
		{
			for (int j = 0; j < room.exitAndDenIndex.Length; j++)
			{
				if (room.ShortcutLeadingToNode(j).shortCutType == ShortcutData.Type.CreatureHole)
				{
					if (n == num)
					{
						return j;
					}
					num++;
				}
			}
		}
		if (crit.mappedNodeTypes[(int)AbstractRoomNode.Type.RegionTransportation])
		{
			for (int k = 0; k < room.exitAndDenIndex.Length; k++)
			{
				if (room.ShortcutLeadingToNode(k).shortCutType == ShortcutData.Type.RegionTransportation)
				{
					if (n == num)
					{
						return k;
					}
					num++;
				}
			}
		}
		for (int l = 0; l < room.borderExits.Length; l++)
		{
			if (room.borderExits[l].type.Index != -1 && crit.mappedNodeTypes[room.borderExits[l].type.Index])
			{
				if (n == num)
				{
					return room.exitAndDenIndex.Length + l;
				}
				num++;
			}
		}
		if (crit.mappedNodeTypes[(int)AbstractRoomNode.Type.BatHive])
		{
			for (int m = 0; m < room.hives.Length; m++)
			{
				if (n == num)
				{
					return room.exitAndDenIndex.Length + room.borderExits.Length + m;
				}
				num++;
			}
		}
		if (crit.mappedNodeTypes[(int)AbstractRoomNode.Type.GarbageHoles] && room.garbageHoles != null && n == num)
		{
			return room.exitAndDenIndex.Length + room.borderExits.Length + room.hives.Length;
		}
		return -1;
	}
}
