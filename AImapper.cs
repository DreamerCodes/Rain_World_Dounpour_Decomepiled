using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using RWCustom;
using UnityEngine;

public class AImapper
{
	private class TerrainProximityMapper
	{
		private AImap aiMap;

		private List<IntVector2> checkNextList;

		public bool done;

		public TerrainProximityMapper(AImap aiMap)
		{
			this.aiMap = aiMap;
			checkNextList = new List<IntVector2>();
			for (int i = 0; i < aiMap.width; i++)
			{
				for (int j = 0; j < aiMap.height; j++)
				{
					aiMap.terrainProximity[ExtraExtentions.ind(i, j, aiMap.height)] = -1;
					if (!aiMap.room.GetTile(i, j).Solid)
					{
						for (int k = 0; k < 4; k++)
						{
							if (aiMap.room.GetTile(i + Custom.fourDirections[k].x, j + Custom.fourDirections[k].y).Solid)
							{
								checkNextList.Add(new IntVector2(i, j));
								aiMap.terrainProximity[ExtraExtentions.ind(i, j, aiMap.height)] = 1;
								break;
							}
						}
					}
					else
					{
						aiMap.terrainProximity[ExtraExtentions.ind(i, j, aiMap.height)] = 0;
					}
				}
			}
		}

		public void Update()
		{
			if (checkNextList.Count == 0)
			{
				done = true;
				return;
			}
			IntVector2 intVector = checkNextList[0];
			int terrainProximity = aiMap.getTerrainProximity(intVector.x, intVector.y);
			checkNextList.RemoveAt(0);
			for (int i = 0; i < 4; i++)
			{
				int num = intVector.x + Custom.fourDirections[i].x;
				int num2 = intVector.y + Custom.fourDirections[i].y;
				if (num >= 0 && num2 >= 0 && num < aiMap.width && num2 < aiMap.height && aiMap.getTerrainProximity(num, num2) < 0)
				{
					aiMap.terrainProximity[ExtraExtentions.ind(num, num2, aiMap.height)] = terrainProximity + 1;
					checkNextList.Add(new IntVector2(num, num2));
				}
			}
		}
	}

	private int x;

	private int y;

	private AImap map;

	private Room room;

	private int pass;

	private TerrainProximityMapper TPXM;

	public bool done;

	public AImapper(Room rm)
	{
		room = rm;
		x = 0;
		y = 0;
		map = new AImap(room, room.TileWidth, room.TileHeight);
		pass = 0;
		done = false;
		if (ModManager.MMF)
		{
			new Thread(AIMappingThread).Start();
		}
	}

	private void AIMappingThread()
	{
		CultureInfo.CurrentCulture = CultureInfo.GetCultureInfoByIetfLanguageTag("en-US");
		for (y = 0; y < room.TileHeight; y++)
		{
			for (x = 0; x < room.TileWidth; x++)
			{
				FindAccessibilityOfCurrentTile();
			}
		}
		for (y = 0; y < room.TileHeight; y++)
		{
			for (x = 0; x < room.TileWidth; x++)
			{
				FindPassagesOfCurrentTile();
				FindFallRiskOfCurrentTile();
			}
		}
		TPXM = new TerrainProximityMapper(map);
		while (!TPXM.done)
		{
			TPXM.Update();
		}
		done = true;
	}

	public void Update()
	{
		if (ModManager.MMF || done)
		{
			return;
		}
		switch (pass)
		{
		case 0:
			FindAccessibilityOfCurrentTile();
			break;
		case 1:
			FindPassagesOfCurrentTile();
			FindFallRiskOfCurrentTile();
			break;
		case 2:
			TPXM = new TerrainProximityMapper(map);
			pass++;
			break;
		case 3:
			if (TPXM.done)
			{
				pass++;
			}
			else
			{
				TPXM.Update();
			}
			break;
		case 4:
			done = true;
			break;
		}
		if (pass >= 2)
		{
			return;
		}
		x++;
		if (x >= room.TileWidth)
		{
			x = 0;
			y++;
			if (y >= room.TileHeight)
			{
				pass++;
				y = 0;
			}
		}
	}

	public AImap ReturnAIMap()
	{
		if (done)
		{
			return map;
		}
		return null;
	}

	public WorldCoordinate WrldCrd(IntVector2 pos)
	{
		return new WorldCoordinate(room.abstractRoom.index, pos.x, pos.y, -1);
	}

	private void FindAccessibilityOfCurrentTile()
	{
		if (room.GetTile(x, y).Terrain != Room.Tile.TerrainType.Solid)
		{
			AItile.Accessibility accessibility = AItile.Accessibility.Air;
			Color color = new Color(0.3f, 0.3f, 0.3f);
			if (room.GetTile(x, y - 1).Terrain == Room.Tile.TerrainType.Solid || room.GetTile(x, y - 1).Terrain == Room.Tile.TerrainType.Floor || (room.GetTile(x - 1, y - 1).Terrain == Room.Tile.TerrainType.Solid && room.GetTile(x - 1, y).Terrain != Room.Tile.TerrainType.Solid) || (room.GetTile(x + 1, y - 1).Terrain == Room.Tile.TerrainType.Solid && room.GetTile(x + 1, y).Terrain != Room.Tile.TerrainType.Solid))
			{
				accessibility = AItile.Accessibility.Floor;
				color.g = 1f;
			}
			else if (room.GetTile(x - 1, y).Terrain == Room.Tile.TerrainType.Solid && room.GetTile(x + 1, y).Terrain == Room.Tile.TerrainType.Solid)
			{
				accessibility = AItile.Accessibility.Corridor;
				color = new Color(1f, 0.7f, 0f);
			}
			else if (room.GetTile(x, y).verticalBeam || room.GetTile(x, y).horizontalBeam)
			{
				accessibility = AItile.Accessibility.Climb;
				color = new Color(1f, 0f, 1f);
			}
			else if (room.GetTile(x, y).wallbehind || room.GetTile(x - 1, y).Terrain == Room.Tile.TerrainType.Solid || room.GetTile(x + 1, y).Terrain == Room.Tile.TerrainType.Solid || (room.GetTile(x - 1, y + 1).Terrain == Room.Tile.TerrainType.Solid && room.GetTile(x, y + 1).Terrain != Room.Tile.TerrainType.Solid) || (room.GetTile(x + 1, y + 1).Terrain == Room.Tile.TerrainType.Solid && room.GetTile(x, y + 1).Terrain != Room.Tile.TerrainType.Solid))
			{
				accessibility = AItile.Accessibility.Wall;
				color.b = 1f;
			}
			else if (room.GetTile(x, y + 1).Terrain == Room.Tile.TerrainType.Solid)
			{
				accessibility = AItile.Accessibility.Ceiling;
				color.r = 1f;
			}
			map.map[x, y] = new AItile(accessibility, (room.GetTile(x, y).DeepWater ? 1 : 0) + (room.GetTile(x, y).WaterSurface ? 2 : 0));
			if (room.game != null && room.game.showAImap && accessibility != AItile.Accessibility.Air)
			{
				FSprite fSprite = new FSprite("pixel");
				fSprite.color = color;
				fSprite.scale = 19f;
				fSprite.alpha = 0.3f;
				room.AddObject(new DebugSprite(room.MiddleOfTile(new IntVector2(x, y)), fSprite, room));
			}
		}
		else
		{
			map.map[x, y] = new AItile(AItile.Accessibility.Solid, (room.GetTile(x, y).DeepWater ? 1 : 0) + (room.GetTile(x, y).WaterSurface ? 2 : 0));
		}
		map.map[x, y].fallRiskTile = new IntVector2(x, y);
	}

	private void FindPassagesOfCurrentTile()
	{
		IntVector2 intVector = new IntVector2(x, y);
		if (room.GetTile(x, y).Terrain == Room.Tile.TerrainType.Floor || (room.GetTile(x, y).Terrain == Room.Tile.TerrainType.Solid && room.GetTile(x, y + 1).Terrain != Room.Tile.TerrainType.Solid))
		{
			for (int i = y + 1; i < map.height && room.GetTile(x, i).Terrain != Room.Tile.TerrainType.Solid; i++)
			{
				map.map[x, i].floorAltitude = i - y;
			}
		}
		for (int j = 0; j < 4; j++)
		{
			if (intVector.x + Custom.fourDirections[j].x >= 0 && intVector.x + Custom.fourDirections[j].x < room.TileWidth && intVector.y + Custom.fourDirections[j].y >= 0 && intVector.y + Custom.fourDirections[j].y < room.TileHeight)
			{
				map.map[x, y].outgoingPaths.Add(new MovementConnection(MovementConnection.MovementType.Standard, WrldCrd(intVector), WrldCrd(intVector + Custom.fourDirections[j]), 1));
			}
			if (room.GetTile(intVector + Custom.eightDirections[j]).Terrain == Room.Tile.TerrainType.Solid && room.GetTile(intVector - Custom.eightDirections[j]).Terrain == Room.Tile.TerrainType.Solid)
			{
				map.map[x, y].narrowSpace = true;
			}
		}
		IntVector2 intVector2 = new IntVector2(0, 1);
		if (map.getAItile(intVector + intVector2).acc != AItile.Accessibility.Solid && map.getAItile(intVector + intVector2 * 2).walkable && map.getAItile(intVector + intVector2).acc > map.getAItile(intVector).acc && map.getAItile(intVector + intVector2 * 2).acc < map.getAItile(intVector + intVector2).acc)
		{
			map.map[x, y].outgoingPaths.Add(new MovementConnection(MovementConnection.MovementType.ReachUp, WrldCrd(intVector), WrldCrd(intVector + intVector2 * 2), 2));
		}
		if (map.getAItile(intVector + intVector2 * 3).walkable && map.getAItile(intVector + intVector2).acc != AItile.Accessibility.Solid && map.getAItile(intVector + intVector2 * 2).acc != AItile.Accessibility.Solid && map.getAItile(intVector + intVector2).acc > map.getAItile(intVector).acc && map.getAItile(intVector + intVector2).acc > map.getAItile(intVector + intVector2 * 3).acc && map.getAItile(intVector + intVector2 * 2).acc > map.getAItile(intVector).acc && map.getAItile(intVector + intVector2 * 2).acc > map.getAItile(intVector + intVector2 * 3).acc)
		{
			map.map[x, y].outgoingPaths.Add(new MovementConnection(MovementConnection.MovementType.DoubleReachUp, WrldCrd(intVector), WrldCrd(intVector + intVector2 * 3), 3));
		}
		if (map.getAItile(intVector - intVector2).acc != AItile.Accessibility.Solid && map.getAItile(intVector - intVector2 * 2).walkable && map.getAItile(intVector - intVector2).acc > map.getAItile(intVector).acc && map.getAItile(intVector - intVector2 * 2).acc < map.getAItile(intVector + intVector2).acc)
		{
			map.map[x, y].outgoingPaths.Add(new MovementConnection(MovementConnection.MovementType.ReachDown, WrldCrd(intVector), WrldCrd(intVector - intVector2 * 2), 2));
		}
		intVector2 = new IntVector2(0, -1);
		if (map.getAItile(intVector + intVector2).acc != AItile.Accessibility.Solid && map.getAItile(intVector + intVector2).acc > map.getAItile(intVector).acc)
		{
			int num = 0;
			List<int> list = new List<int>();
			for (int num2 = intVector.y - 1; num2 > 0; num2--)
			{
				if (map.getAItile(intVector.x, num2).acc == AItile.Accessibility.Floor && (room.GetTile(intVector.x, num2 - 1).Terrain == Room.Tile.TerrainType.Solid || room.GetTile(intVector.x, num2 - 1).Terrain == Room.Tile.TerrainType.Floor))
				{
					map.map[x, y].outgoingPaths.Add(new MovementConnection(MovementConnection.MovementType.DropToFloor, WrldCrd(intVector), WrldCrd(new IntVector2(intVector.x, num2)), intVector.y - num2));
					list.Add(num2);
					list.Add(num2 - 1);
					list.Add(num2 + 1);
					break;
				}
				if (map.getAItile(intVector.x, num2).acc == AItile.Accessibility.Climb && map.getAItile(intVector.x, num2 + 1).acc > map.getAItile(intVector.x, num2).acc)
				{
					map.map[x, y].outgoingPaths.Add(new MovementConnection(MovementConnection.MovementType.DropToClimb, WrldCrd(intVector), WrldCrd(new IntVector2(intVector.x, num2)), intVector.y - num2));
					list.Add(num2);
				}
				if (room.GetTile(intVector.x, num2).WaterSurface)
				{
					map.map[x, y].outgoingPaths.Add(new MovementConnection(MovementConnection.MovementType.DropToWater, WrldCrd(intVector), WrldCrd(new IntVector2(intVector.x, num2)), intVector.y - num2));
					list.Add(num2);
				}
				num++;
			}
			if (num >= 4)
			{
				for (int num3 = 1; num3 >= -1; num3 -= 2)
				{
					int num4 = intVector.y - 4;
					while (num4 > 0 && map.getAItile(intVector.x + num3, num4).acc != AItile.Accessibility.Solid && map.getAItile(intVector.x, num4).acc != AItile.Accessibility.Solid)
					{
						if (map.getAItile(intVector.x + num3, num4).acc == AItile.Accessibility.Floor && map.getAItile(intVector.x + num3, num4 + 1).acc != AItile.Accessibility.Solid && map.getAItile(intVector.x + num3, num4 + 2).acc != AItile.Accessibility.Solid && (room.GetTile(intVector.x + num3, num4 - 1).Terrain == Room.Tile.TerrainType.Solid || room.GetTile(intVector.x + num3, num4 - 1).Terrain == Room.Tile.TerrainType.Floor) && !IsThereAStraightDropBetweenTiles(new IntVector2(intVector.x - 1, intVector.y), new IntVector2(intVector.x + num3, num4)) && !IsThereAStraightDropBetweenTiles(new IntVector2(intVector.x + 1, intVector.y), new IntVector2(intVector.x + num3, num4)))
						{
							if (!list.Contains(num4))
							{
								map.map[x, y].outgoingPaths.Add(new MovementConnection(MovementConnection.MovementType.DropToFloor, WrldCrd(intVector), WrldCrd(new IntVector2(intVector.x + num3, num4)), intVector.y - num4));
							}
							break;
						}
						if (map.getAItile(intVector.x + num3, num4).acc == AItile.Accessibility.Climb && map.getAItile(intVector.x + num3, num4 + 1).acc != AItile.Accessibility.Solid && map.getAItile(intVector.x + num3, num4 + 2).acc != AItile.Accessibility.Solid && map.getAItile(intVector.x + num3, num4 + 1).acc > map.getAItile(intVector.x + num3, num4).acc && !IsThereAStraightDropBetweenTiles(new IntVector2(intVector.x - 1, intVector.y), new IntVector2(intVector.x + num3, num4)) && !IsThereAStraightDropBetweenTiles(new IntVector2(intVector.x + 1, intVector.y), new IntVector2(intVector.x + num3, num4)) && !list.Contains(num4))
						{
							map.map[x, y].outgoingPaths.Add(new MovementConnection(MovementConnection.MovementType.DropToClimb, WrldCrd(intVector), WrldCrd(new IntVector2(intVector.x + num3, num4)), intVector.y - num4));
						}
						num4--;
					}
				}
			}
		}
		for (int k = 0; k < 2; k++)
		{
			intVector2 = new IntVector2((k != 0) ? 1 : (-1), 0);
			if (map.getAItile(intVector + intVector2).acc != AItile.Accessibility.Solid && map.getAItile(intVector + intVector2 * 2).walkable && map.getAItile(intVector + intVector2).acc > map.getAItile(intVector).acc && map.getAItile(intVector + intVector2 * 2).acc < map.getAItile(intVector + intVector2).acc)
			{
				map.map[x, y].outgoingPaths.Add(new MovementConnection(MovementConnection.MovementType.ReachOverGap, WrldCrd(intVector), WrldCrd(intVector + intVector2 * 2), 2));
			}
			if (map.getAItile(intVector).acc == AItile.Accessibility.Floor && map.getAItile(intVector + intVector2).acc == AItile.Accessibility.Floor && map.getAItile(intVector + new IntVector2(0, 1)).acc != AItile.Accessibility.Solid && map.getAItile(intVector + intVector2 + new IntVector2(0, 1)).acc != AItile.Accessibility.Solid)
			{
				map.map[x, y].outgoingPaths.Add(new MovementConnection(MovementConnection.MovementType.LizardTurn, WrldCrd(intVector), WrldCrd(intVector + intVector2), 1));
			}
			if (map.getAItile(intVector).acc == AItile.Accessibility.Floor && room.GetTile(intVector + intVector2).Terrain == Room.Tile.TerrainType.Slope && map.getAItile(intVector + new IntVector2(intVector2.x, 1)).acc == AItile.Accessibility.Floor)
			{
				map.map[x, y].outgoingPaths.Add(new MovementConnection(MovementConnection.MovementType.Slope, WrldCrd(intVector), WrldCrd(intVector + new IntVector2(intVector2.x, 1)), 1));
				map.map[x + intVector2.x, y + 1].outgoingPaths.Add(new MovementConnection(MovementConnection.MovementType.Slope, WrldCrd(intVector + new IntVector2(intVector2.x, 1)), WrldCrd(intVector), 1));
			}
			else if (map.getAItile(intVector + new IntVector2(intVector2.x, 0)).acc != AItile.Accessibility.Solid && map.getAItile(intVector + new IntVector2(0, 1)).acc != AItile.Accessibility.Solid)
			{
				int num5 = Math.Max((int)map.getAItile(intVector).acc, (int)map.getAItile(intVector + new IntVector2(intVector2.x, 1)).acc);
				if ((int)map.getAItile(intVector + new IntVector2(intVector2.x, 0)).acc > num5 && (int)map.getAItile(intVector + new IntVector2(0, 1)).acc > num5)
				{
					map.map[x, y].outgoingPaths.Add(new MovementConnection(MovementConnection.MovementType.OpenDiagonal, WrldCrd(intVector), WrldCrd(intVector + new IntVector2(intVector2.x, 1)), 1));
					map.map[x + intVector2.x, y + 1].outgoingPaths.Add(new MovementConnection(MovementConnection.MovementType.OpenDiagonal, WrldCrd(intVector + new IntVector2(intVector2.x, 1)), WrldCrd(intVector), 1));
				}
			}
			if ((map.getAItile(intVector).acc == AItile.Accessibility.Ceiling || map.getAItile(intVector).acc == AItile.Accessibility.Wall) && room.GetTile(intVector + intVector2).Terrain == Room.Tile.TerrainType.Slope && (map.getAItile(intVector + new IntVector2(intVector2.x, -1)).acc == AItile.Accessibility.Ceiling || map.getAItile(intVector + new IntVector2(intVector2.x, -1)).acc == AItile.Accessibility.Wall))
			{
				map.map[x, y].outgoingPaths.Add(new MovementConnection(MovementConnection.MovementType.CeilingSlope, WrldCrd(intVector), WrldCrd(intVector + new IntVector2(intVector2.x, -1)), 1));
				map.map[x + intVector2.x, y - 1].outgoingPaths.Add(new MovementConnection(MovementConnection.MovementType.CeilingSlope, WrldCrd(intVector + new IntVector2(intVector2.x, -1)), WrldCrd(intVector), 1));
				map.map[x, y].outgoingPaths.Add(map.map[x + intVector2.x, y - 1].outgoingPaths[map.map[x + intVector2.x, y - 1].outgoingPaths.Count - 1]);
			}
			if (map.getAItile(intVector + new IntVector2(intVector2.x, -2)).walkable && map.getAItile(intVector + new IntVector2(0, -1)).acc != AItile.Accessibility.Solid && map.getAItile(intVector + new IntVector2(intVector2.x, -1)).acc != AItile.Accessibility.Solid && (map.getAItile(intVector + new IntVector2(intVector2.x, 0)).acc != AItile.Accessibility.Solid || map.getAItile(intVector + new IntVector2(0, -2)).acc != AItile.Accessibility.Solid) && map.getAItile(intVector + new IntVector2(intVector2.x, 0)).acc > map.getAItile(intVector).acc && map.getAItile(intVector + new IntVector2(intVector2.x, 0)).acc > map.getAItile(intVector + new IntVector2(intVector2.x, -2)).acc && map.getAItile(intVector + new IntVector2(0, -2)).acc > map.getAItile(intVector).acc && map.getAItile(intVector + new IntVector2(0, -2)).acc > map.getAItile(intVector + new IntVector2(intVector2.x, -2)).acc && map.getAItile(intVector + new IntVector2(0, -1)).acc > map.getAItile(intVector).acc && map.getAItile(intVector + new IntVector2(0, -1)).acc > map.getAItile(intVector + new IntVector2(intVector2.x, -2)).acc && map.getAItile(intVector + new IntVector2(intVector2.x, -1)).acc > map.getAItile(intVector).acc && map.getAItile(intVector + new IntVector2(intVector2.x, -1)).acc > map.getAItile(intVector + new IntVector2(intVector2.x, -2)).acc)
			{
				map.map[x, y].outgoingPaths.Add(new MovementConnection(MovementConnection.MovementType.SemiDiagonalReach, WrldCrd(intVector), WrldCrd(intVector + new IntVector2(intVector2.x, -2)), 3));
				map.map[x + intVector2.x, y - 2].outgoingPaths.Add(new MovementConnection(MovementConnection.MovementType.SemiDiagonalReach, WrldCrd(intVector + new IntVector2(intVector2.x, -2)), WrldCrd(intVector), 3));
				map.map[x, y].outgoingPaths.Add(map.map[x + intVector2.x, y - 2].outgoingPaths[map.map[x + intVector2.x, y - 2].outgoingPaths.Count - 1]);
			}
			if (map.getAItile(intVector + new IntVector2(-2, intVector2.x)).walkable && map.getAItile(intVector + new IntVector2(-1, 0)).acc != AItile.Accessibility.Solid && map.getAItile(intVector + new IntVector2(-1, intVector2.x)).acc != AItile.Accessibility.Solid && (map.getAItile(intVector + new IntVector2(0, intVector2.x)).acc != AItile.Accessibility.Solid || map.getAItile(intVector + new IntVector2(-2, 0)).acc != AItile.Accessibility.Solid) && map.getAItile(intVector + new IntVector2(0, intVector2.x)).acc > map.getAItile(intVector).acc && map.getAItile(intVector + new IntVector2(0, intVector2.x)).acc > map.getAItile(intVector + new IntVector2(-2, intVector2.x)).acc && map.getAItile(intVector + new IntVector2(-2, 0)).acc > map.getAItile(intVector).acc && map.getAItile(intVector + new IntVector2(-2, 0)).acc > map.getAItile(intVector + new IntVector2(-2, intVector2.x)).acc && map.getAItile(intVector + new IntVector2(-1, 0)).acc > map.getAItile(intVector).acc && map.getAItile(intVector + new IntVector2(-1, 0)).acc > map.getAItile(intVector + new IntVector2(-2, intVector2.x)).acc && map.getAItile(intVector + new IntVector2(-1, intVector2.x)).acc > map.getAItile(intVector).acc && map.getAItile(intVector + new IntVector2(-1, intVector2.x)).acc > map.getAItile(intVector + new IntVector2(-2, intVector2.x)).acc)
			{
				map.map[x, y].outgoingPaths.Add(new MovementConnection(MovementConnection.MovementType.SemiDiagonalReach, WrldCrd(intVector), WrldCrd(intVector + new IntVector2(-2, intVector2.x)), 3));
				map.map[x - 2, y + intVector2.x].outgoingPaths.Add(new MovementConnection(MovementConnection.MovementType.SemiDiagonalReach, WrldCrd(intVector + new IntVector2(-2, intVector2.x)), WrldCrd(intVector), 3));
				map.map[x, y].outgoingPaths.Add(map.map[x - 2, y + intVector2.x].outgoingPaths[map.map[x - 2, y + intVector2.x].outgoingPaths.Count - 1]);
			}
		}
		if (room.GetTile(intVector).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
		{
			IntVector2 intVector3 = room.ShorcutEntranceHoleDirection(intVector);
			for (int l = 2; l < 4; l++)
			{
				for (int m = ((l == 3) ? (-1) : 0); m < ((l != 3) ? 1 : 2); m++)
				{
					IntVector2 pos = intVector + intVector3 * l + Custom.PerpIntVec(intVector3) * m;
					if (!room.GetTile(pos).Solid && room.IsPositionInsideBoundries(pos))
					{
						map.map[x, y].outgoingPaths.Add(new MovementConnection(MovementConnection.MovementType.BigCreatureShortCutSqueeze, WrldCrd(intVector), WrldCrd(pos), l));
						MovementConnection item = new MovementConnection(MovementConnection.MovementType.BigCreatureShortCutSqueeze, WrldCrd(pos), WrldCrd(intVector), l);
						map.map[pos.x, pos.y].outgoingPaths.Add(item);
						if (pos.x < x || pos.y < y)
						{
							map.map[x, y].incomingPaths.Add(item);
						}
					}
				}
			}
			if (room.shortcutData(intVector).shortCutType == ShortcutData.Type.Normal)
			{
				map.map[x, y].outgoingPaths.Add(new MovementConnection(MovementConnection.MovementType.ShortCut, WrldCrd(intVector), WrldCrd(room.shortcutData(intVector).DestTile), room.shortcutData(intVector).length));
			}
			else if (room.shortcutData(intVector).shortCutType == ShortcutData.Type.NPCTransportation)
			{
				ShortcutData[] shortcuts = room.shortcuts;
				for (int n = 0; n < shortcuts.Length; n++)
				{
					ShortcutData shortcutData = shortcuts[n];
					if (shortcutData.shortCutType == ShortcutData.Type.NPCTransportation && (shortcutData.startCoord.x != x || shortcutData.startCoord.y != y))
					{
						map.map[x, y].outgoingPaths.Add(new MovementConnection(MovementConnection.MovementType.NPCTransportation, WrldCrd(intVector), WrldCrd(shortcutData.StartTile), (int)Vector2.Distance(IntVector2.ToVector2(room.shortcutData(intVector).StartTile), IntVector2.ToVector2(shortcutData.StartTile)) + room.shortcutData(intVector).length + shortcutData.length));
					}
				}
			}
			else
			{
				_ = room.shortcutData(intVector).shortCutType == ShortcutData.Type.RoomExit;
			}
		}
		if (room.game == null || !room.game.showAImap)
		{
			foreach (MovementConnection outgoingPath in map.map[x, y].outgoingPaths)
			{
				map.map[outgoingPath.destinationCoord.x, outgoingPath.destinationCoord.y].incomingPaths.Add(outgoingPath);
			}
			return;
		}
		foreach (MovementConnection outgoingPath2 in map.map[x, y].outgoingPaths)
		{
			map.map[outgoingPath2.destinationCoord.x, outgoingPath2.destinationCoord.y].incomingPaths.Add(outgoingPath2);
			if (outgoingPath2.type == MovementConnection.MovementType.ReachOverGap)
			{
				FSprite fSprite = new FSprite("pixel");
				fSprite.color = new Color(0f, 1f, 0f);
				fSprite.scale = 6f;
				fSprite.alpha = 1f;
				room.AddObject(new DebugSprite(room.MiddleOfTile(new IntVector2(x, y)) + new Vector2((outgoingPath2.destinationCoord.x < outgoingPath2.startCoord.x) ? (-7f) : 7f, 0f), fSprite, room));
			}
			else if (outgoingPath2.type == MovementConnection.MovementType.ReachUp)
			{
				FSprite fSprite = new FSprite("pixel");
				fSprite.color = new Color(0f, 1f, 0f);
				fSprite.scale = 6f;
				fSprite.alpha = 1f;
				room.AddObject(new DebugSprite(room.MiddleOfTile(new IntVector2(x, y)) + new Vector2(0f, 7f), fSprite, room));
			}
			else if (outgoingPath2.type == MovementConnection.MovementType.DoubleReachUp)
			{
				FSprite fSprite = new FSprite("pixel");
				fSprite.color = new Color(0f, 1f, 0f);
				fSprite.scale = 12f;
				fSprite.alpha = 1f;
				room.AddObject(new DebugSprite(room.MiddleOfTile(new IntVector2(x, y)) + new Vector2(0f, 7f), fSprite, room));
			}
			else if (outgoingPath2.type == MovementConnection.MovementType.SemiDiagonalReach)
			{
				FSprite fSprite = new FSprite("pixel");
				fSprite.color = new Color(1f, 0.5f, 0.5f);
				fSprite.scaleX = 3f;
				fSprite.scaleY = 5f;
				fSprite.alpha = 1f;
				fSprite.rotation = Custom.AimFromOneVectorToAnother(room.MiddleOfTile(outgoingPath2.StartTile), room.MiddleOfTile(outgoingPath2.DestTile));
				room.AddObject(new DebugSprite(Vector2.Lerp(room.MiddleOfTile(outgoingPath2.StartTile), room.MiddleOfTile(outgoingPath2.DestTile), 0.25f), fSprite, room));
			}
			else if (outgoingPath2.type == MovementConnection.MovementType.DropToFloor)
			{
				FSprite fSprite = new FSprite("pixel");
				fSprite.color = new Color(1f, 0f, 0f);
				fSprite.scale = 6f;
				fSprite.alpha = 1f;
				room.AddObject(new DebugSprite(room.MiddleOfTile(new IntVector2(x, y)) + new Vector2(0f, -7f), fSprite, room));
				if (outgoingPath2.destinationCoord.x != outgoingPath2.startCoord.x)
				{
					fSprite = new FSprite("pixel");
					fSprite.color = new Color(1f, 0f, 0f);
					fSprite.scaleX = 1f;
					fSprite.alpha = 0.5f;
					fSprite.anchorY = 0f;
					Vector2 vector = room.MiddleOfTile(outgoingPath2.startCoord);
					Vector2 vector2 = room.MiddleOfTile(outgoingPath2.destinationCoord);
					fSprite.scaleY = Vector2.Distance(vector, vector2);
					fSprite.rotation = Custom.AimFromOneVectorToAnother(vector, vector2);
					room.AddObject(new DebugSprite(vector, fSprite, room));
				}
			}
			else if (outgoingPath2.type == MovementConnection.MovementType.DropToClimb)
			{
				FSprite fSprite = new FSprite("pixel");
				fSprite.color = new Color(1f, 0f, 1f);
				fSprite.scale = 6f;
				fSprite.alpha = 1f;
				room.AddObject(new DebugSprite(room.MiddleOfTile(new IntVector2(x, y)) + new Vector2(-4f, -7f), fSprite, room));
				if (outgoingPath2.destinationCoord.x != outgoingPath2.startCoord.x)
				{
					fSprite = new FSprite("pixel");
					fSprite.color = new Color(1f, 0f, 1f);
					fSprite.scaleX = 1f;
					fSprite.alpha = 0.5f;
					fSprite.anchorY = 0f;
					Vector2 vector3 = room.MiddleOfTile(outgoingPath2.startCoord);
					Vector2 vector4 = room.MiddleOfTile(outgoingPath2.destinationCoord);
					fSprite.scaleY = Vector2.Distance(vector3, vector4);
					fSprite.rotation = Custom.AimFromOneVectorToAnother(vector3, vector4);
					room.AddObject(new DebugSprite(vector3, fSprite, room));
				}
			}
			else if (outgoingPath2.type == MovementConnection.MovementType.DropToWater)
			{
				FSprite fSprite = new FSprite("pixel");
				fSprite.color = new Color(0f, 0f, 1f);
				fSprite.scale = 6f;
				fSprite.alpha = 1f;
				room.AddObject(new DebugSprite(room.MiddleOfTile(new IntVector2(x, y)) + new Vector2(-4f, -7f), fSprite, room));
				if (outgoingPath2.destinationCoord.x != outgoingPath2.startCoord.x)
				{
					fSprite = new FSprite("pixel");
					fSprite.color = new Color(0f, 0f, 1f);
					fSprite.scaleX = 1f;
					fSprite.alpha = 0.5f;
					fSprite.anchorY = 0f;
					Vector2 vector5 = room.MiddleOfTile(outgoingPath2.startCoord);
					Vector2 vector6 = room.MiddleOfTile(outgoingPath2.destinationCoord);
					fSprite.scaleY = Vector2.Distance(vector5, vector6);
					fSprite.rotation = Custom.AimFromOneVectorToAnother(vector5, vector6);
					room.AddObject(new DebugSprite(vector5, fSprite, room));
				}
			}
			else if (outgoingPath2.type == MovementConnection.MovementType.LizardTurn)
			{
				FSprite fSprite = new FSprite("pixel");
				fSprite.color = new Color(0f, 1f, 1f);
				fSprite.scale = 4f;
				fSprite.alpha = 1f;
				room.AddObject(new DebugSprite(room.MiddleOfTile(new IntVector2(x, y)) + new Vector2((outgoingPath2.destinationCoord.x < outgoingPath2.startCoord.x) ? (-8f) : 8f, -6f), fSprite, room));
			}
			else if (outgoingPath2.type == MovementConnection.MovementType.Slope)
			{
				FSprite fSprite = new FSprite("pixel");
				fSprite.color = new Color(1f, 0f, 1f);
				fSprite.scale = 5f;
				fSprite.alpha = 1f;
				fSprite.rotation = 45f;
				room.AddObject(new DebugSprite(Vector2.Lerp(room.MiddleOfTile(outgoingPath2.StartTile), room.MiddleOfTile(outgoingPath2.DestTile), 0.25f), fSprite, room));
			}
			else if (outgoingPath2.type == MovementConnection.MovementType.OpenDiagonal)
			{
				FSprite fSprite = new FSprite("pixel");
				fSprite.color = new Color(1f, 0f, 0f);
				fSprite.scale = 5f;
				fSprite.alpha = 1f;
				fSprite.rotation = 45f;
				room.AddObject(new DebugSprite(Vector2.Lerp(room.MiddleOfTile(outgoingPath2.StartTile), room.MiddleOfTile(outgoingPath2.DestTile), 0.25f), fSprite, room));
			}
			else if (outgoingPath2.type == MovementConnection.MovementType.CeilingSlope)
			{
				FSprite fSprite = new FSprite("pixel");
				fSprite.color = new Color(0f, 1f, 1f);
				fSprite.scale = 5f;
				fSprite.alpha = 1f;
				fSprite.rotation = 45f;
				room.AddObject(new DebugSprite(Vector2.Lerp(room.MiddleOfTile(outgoingPath2.StartTile), room.MiddleOfTile(outgoingPath2.DestTile), 0.25f), fSprite, room));
			}
			else if (outgoingPath2.type == MovementConnection.MovementType.ShortCut)
			{
				FSprite fSprite = new FSprite("pixel");
				fSprite.color = new Color(1f, 1f, 1f);
				fSprite.scale = 11f;
				fSprite.alpha = 1f;
				room.AddObject(new DebugSprite(room.MiddleOfTile(new IntVector2(x, y)), fSprite, room));
			}
			else if (outgoingPath2.type == MovementConnection.MovementType.BigCreatureShortCutSqueeze)
			{
				FSprite fSprite = new FSprite("pixel");
				fSprite.color = new Color(1f, 1f, 0f);
				fSprite.scaleX = 3f;
				fSprite.scaleY = 15f;
				fSprite.anchorY = 0f;
				fSprite.alpha = 1f;
				fSprite.rotation = Custom.AimFromOneVectorToAnother(room.MiddleOfTile(outgoingPath2.StartTile), room.MiddleOfTile(outgoingPath2.DestTile));
				room.AddObject(new DebugSprite(room.MiddleOfTile(outgoingPath2.StartTile), fSprite, room));
			}
		}
	}

	private bool IsThereAStraightDropBetweenTiles(IntVector2 A, IntVector2 B)
	{
		IntVector2 intVector = new IntVector2(0, -1);
		if (map.getAItile(A + intVector).acc != AItile.Accessibility.Solid && map.getAItile(A + intVector).acc > map.getAItile(A).acc)
		{
			for (int num = A.y - 1; num > 0; num--)
			{
				if (map.getAItile(A.x, num).acc == AItile.Accessibility.Floor && (room.GetTile(A.x, num - 1).Terrain == Room.Tile.TerrainType.Solid || room.GetTile(A.x, num - 1).Terrain == Room.Tile.TerrainType.Floor))
				{
					return new IntVector2(A.x, num) == B;
				}
				if (map.getAItile(A.x, num).acc == AItile.Accessibility.Climb && map.getAItile(A.x, num + 1).acc > map.getAItile(A.x, num).acc && new IntVector2(A.x, num) == B)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void FindFallRiskOfCurrentTile()
	{
		if (room.GetTile(x, y).Solid)
		{
			return;
		}
		if (y == 0 || room.GetTile(x, y - 1).Solid)
		{
			for (int i = y; i < room.TileHeight && !room.GetTile(x, i).Solid; i++)
			{
				IntVector2 fallRiskTile = ((y != 0 || room.GetTile(x, -1).Solid) ? new IntVector2(x, y) : new IntVector2(-1, -1));
				if (map.getAItile(x, i).fallRiskTile.y >= fallRiskTile.y)
				{
					map.getAItile(x, i).fallRiskTile = fallRiskTile;
				}
				if (!room.GetTile(x - 1, i).Solid && map.getAItile(x - 1, i).fallRiskTile.y > fallRiskTile.y)
				{
					map.getAItile(x - 1, i).fallRiskTile = fallRiskTile;
				}
				if (!room.GetTile(x + 1, i).Solid && map.getAItile(x + 1, i).fallRiskTile.y > fallRiskTile.y)
				{
					map.getAItile(x + 1, i).fallRiskTile = fallRiskTile;
				}
			}
		}
		int num = map.getAItile(x, y).floorAltitude;
		int num2 = Custom.IntClamp(num, 0, 5);
		for (int j = -1; j < 2; j += 2)
		{
			for (int k = 1; k < num2 && x + j * k >= 0 && x + j * k < room.TileWidth && !room.GetTile(x + j * k, y).Solid; k++)
			{
				if (map.getAItile(x + j * k, y).floorAltitude + k / 2 < num)
				{
					num = map.getAItile(x + j * k, y).floorAltitude + k / 2;
				}
			}
		}
		map.getAItile(x, y).smoothedFloorAltitude = num;
	}
}
