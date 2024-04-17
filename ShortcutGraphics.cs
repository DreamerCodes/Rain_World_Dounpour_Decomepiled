using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class ShortcutGraphics
{
	private RoomCamera camera;

	private ShortcutHandler shortcutHandler;

	private Dictionary<int, FSprite> sprites = new Dictionary<int, FSprite>();

	private FSprite[,] entranceSprites;

	private Color[] entranceSpriteColors;

	private Vector2[] entranceSpriteLocations;

	private Dictionary<int, float> lightness = new Dictionary<int, float>();

	private FShader[] shortcutShaders;

	public int[] entraceSpriteToRoomExitIndex;

	private bool waitingForRoomToGenerateShortcuts;

	private Room room => camera.room;

	private RoomPalette palette => camera.currentPalette;

	public ShortcutGraphics(RoomCamera camera, ShortcutHandler shortcutHandler, FShader[] shortcutShaders)
	{
		this.camera = camera;
		this.shortcutShaders = shortcutShaders;
		this.shortcutHandler = shortcutHandler;
		entranceSprites = new FSprite[0, 0];
		entranceSpriteColors = new Color[0];
		entraceSpriteToRoomExitIndex = new int[0];
	}

	public void Update()
	{
		for (int i = 0; i < entranceSpriteColors.Length; i++)
		{
			if (entranceSpriteColors[i].a > 0f)
			{
				entranceSpriteColors[i] = Custom.RGB2RGBA(entranceSpriteColors[i], Mathf.Max(0f, entranceSpriteColors[i].a - 0.025f));
			}
		}
	}

	public void ColorEntrance(int entrance, Color color)
	{
		if (entrance >= 0 && entrance < entranceSpriteColors.Length)
		{
			entranceSpriteColors[entrance] = color;
		}
	}

	public void Draw(float timeStacker, Vector2 camPos)
	{
		if (waitingForRoomToGenerateShortcuts)
		{
			if (room.shortCutsReady)
			{
				GenerateSprites();
				waitingForRoomToGenerateShortcuts = false;
			}
			return;
		}
		if (room.shortcutsBlinking != null)
		{
			for (int i = 0; i < room.shortcutsBlinking.GetLength(0); i++)
			{
				if (!(room.shortcutsBlinking[i, 0] > 0f))
				{
					continue;
				}
				float num = -1f;
				if (room.shortcutsBlinking[i, 1] > 0f)
				{
					num = Mathf.Lerp(0f, 2f, room.shortcutsBlinking[i, 1]);
				}
				float num2 = room.shortcutsBlinking[i, 0] + (0.5f - Mathf.Abs(room.shortcutsBlinking[i, 0] - 0.5f)) * Mathf.Lerp(-1.5f, 1.5f, Random.value) * ((Random.value < 1f / 11f) ? 1f : 0f);
				if (Random.value < 1f / 30f)
				{
					num2 = room.shortcutsBlinking[i, 0] * Random.value;
				}
				for (int j = 0; j < room.shortcuts[i].path.Length; j++)
				{
					int roomCoordHash = GetRoomCoordHash(room.shortcuts[i].path[j]);
					if (sprites.ContainsKey(roomCoordHash))
					{
						float num3 = Mathf.Pow(Mathf.Clamp(1f - Mathf.Abs((float)j / (float)room.shortcuts[i].length - num), 0f, 1f), 5f);
						num3 = num3 * 0.5f + num2 * 0.5f;
						if (!lightness.ContainsKey(roomCoordHash))
						{
							lightness.Add(roomCoordHash, 0f);
						}
						lightness[roomCoordHash] = Mathf.Max(lightness[roomCoordHash], num3);
					}
				}
			}
		}
		for (int k = 0; k < entranceSprites.GetLength(0); k++)
		{
			if (entranceSprites[k, 0] == null)
			{
				continue;
			}
			Vector2 vector = camera.ApplyDepth(entranceSpriteLocations[k], -5f) - camPos;
			if (ModManager.MSC)
			{
				if (room.shortcuts[k].shortCutType == ShortcutData.Type.CreatureHole)
				{
					if (room.game.cameras[0].followAbstractCreature != null && room.game.cameras[0].followAbstractCreature.controlled && room.game.cameras[0].followAbstractCreature.creatureTemplate.TopAncestor().usesCreatureHoles)
					{
						entranceSprites[k, 0].isVisible = true;
					}
					else
					{
						entranceSprites[k, 0].isVisible = false;
					}
				}
				if (entranceSprites[k, 0].element.name == "ShortcutDoubleArrow")
				{
					if (room.game.cameras[0].followAbstractCreature != null && room.game.cameras[0].followAbstractCreature.controlled && room.game.cameras[0].followAbstractCreature.creatureTemplate.TopAncestor().usesNPCTransportation)
					{
						entranceSprites[k, 0].isVisible = true;
					}
					else
					{
						entranceSprites[k, 0].isVisible = false;
					}
				}
				if (entranceSprites[k, 0].element.name == "ShortcutTransportArrow")
				{
					if (room.game.cameras[0].followAbstractCreature != null && room.game.cameras[0].followAbstractCreature.controlled && room.game.cameras[0].followAbstractCreature.creatureTemplate.TopAncestor().usesRegionTransportation)
					{
						entranceSprites[k, 0].isVisible = true;
					}
					else
					{
						entranceSprites[k, 0].isVisible = false;
					}
				}
			}
			entranceSprites[k, 0].x = vector.x;
			entranceSprites[k, 0].y = vector.y;
			float t = 0f;
			if (entraceSpriteToRoomExitIndex[k] > -1 && room.game.IsArenaSession)
			{
				t = room.game.GetArenaGameSession.DarkenExitSymbol(entraceSpriteToRoomExitIndex[k]);
			}
			if (room.shortcutsBlinking[k, 3] < 0f)
			{
				entranceSprites[k, 1].x = vector.x;
				entranceSprites[k, 1].y = vector.y;
				entranceSprites[k, 1].alpha = Custom.LerpMap(Mathf.Abs(room.shortcutsBlinking[k, 3]), 0f, 20f, 0f, 0.35f);
				entranceSprites[k, 1].scale = Custom.LerpMap(Mathf.Abs(room.shortcutsBlinking[k, 3]) + Random.value * 10f, 0f, 20f, 1.5f, 2.5f);
				if (Mathf.Abs(room.shortcutsBlinking[k, 3]) > 20f && (int)Mathf.Abs(room.shortcutsBlinking[k, 3]) % 4 < 3)
				{
					entranceSprites[k, 1].scale += Custom.LerpMap(Mathf.Abs(room.shortcutsBlinking[k, 3]), 20f, 50f, 0f, 1.6f, 1.4f);
					entranceSprites[k, 1].alpha += Custom.LerpMap(Mathf.Abs(room.shortcutsBlinking[k, 3]), 20f, 50f, 0f, 0.35f);
				}
				entranceSprites[k, 1].isVisible = true;
				if ((int)Mathf.Abs(room.shortcutsBlinking[k, 3]) % 4 < 3)
				{
					entranceSprites[k, 0].color = Color.Lerp(camera.currentPalette.shortCutSymbol, camera.currentPalette.blackColor, t);
				}
				else
				{
					entranceSprites[k, 0].color = ColorFromLightness(0f);
				}
			}
			else
			{
				entranceSprites[k, 1].isVisible = false;
				int roomCoordHash2 = GetRoomCoordHash(room.shortcuts[k].startCoord);
				if (!lightness.ContainsKey(roomCoordHash2))
				{
					lightness.Add(roomCoordHash2, 0f);
				}
				entranceSprites[k, 0].color = Color.Lerp(Color.Lerp(ColorFromLightness(Mathf.Max(lightness[roomCoordHash2], 0f)), Color.Lerp(palette.shortcutColors[2], camera.currentPalette.shortCutSymbol, 0.75f), room.shortcutsBlinking[k, 2]), camera.currentPalette.blackColor, t);
			}
			entranceSprites[k, 0].color = Color.Lerp(entranceSprites[k, 0].color, Custom.RGBA2RGB(entranceSpriteColors[k]), entranceSpriteColors[k].a);
			entranceSprites[k, 1].color = Color.Lerp(new Color(1f, 1f, 1f), Custom.RGBA2RGB(entranceSpriteColors[k]), entranceSpriteColors[k].a);
			if ((!ModManager.MMF || !MMF.cfgShowUnderwaterShortcuts.Value) && room.water && room.waterInFrontOfTerrain && room.PointSubmerged(entranceSpriteLocations[k] + new Vector2(0f, 5f)))
			{
				entranceSprites[k, 0].color = new Color(0f, 0.007843138f, 0f);
			}
		}
		foreach (KeyValuePair<int, FSprite> sprite in sprites)
		{
			IntVector2 intVector = UnpackRoomCoordHash(sprite.Key);
			Vector2 vector2 = camera.ApplyDepth(room.MiddleOfTile(intVector.x, intVector.y), (DisplayLayer(intVector.x, intVector.y) == 0) ? (-5f) : 5f) - camPos;
			sprite.Value.x = vector2.x;
			sprite.Value.y = vector2.y;
			if (!lightness.ContainsKey(sprite.Key))
			{
				lightness.Add(sprite.Key, 0f);
			}
			sprite.Value.color = ColorFromLightness(lightness[sprite.Key]);
			lightness[sprite.Key] = 0f;
		}
		foreach (ShortcutHandler.ShortCutVessel transportVessel in shortcutHandler.transportVessels)
		{
			int roomCoordHash3 = GetRoomCoordHash(transportVessel.pos);
			if (transportVessel.room != room.abstractRoom || !sprites.ContainsKey(roomCoordHash3))
			{
				continue;
			}
			sprites[roomCoordHash3].color = ShortCutColor(transportVessel.creature, transportVessel.pos);
			if (transportVessel.creature.Template.shortcutSegments <= 1)
			{
				continue;
			}
			for (int l = 0; l < transportVessel.lastPositions.Length; l++)
			{
				int roomCoordHash4 = GetRoomCoordHash(transportVessel.lastPositions[l]);
				if (sprites.ContainsKey(roomCoordHash4))
				{
					sprites[roomCoordHash4].color = ShortCutColor(transportVessel.creature, transportVessel.lastPositions[l]);
				}
			}
		}
	}

	public void ChangeAllExitsToSheltersOrDots(bool toShelters)
	{
		for (int i = 0; i < room.shortcuts.Length; i++)
		{
			if (room.shortcuts[i].shortCutType == ShortcutData.Type.RoomExit && entranceSprites[i, 0] != null)
			{
				entranceSprites[i, 0].element = Futile.atlasManager.GetElementWithName(toShelters ? "ShortcutShelter" : "ShortcutDots");
			}
		}
	}

	private Color ShortCutColor(Creature crit, IntVector2 pos)
	{
		Color color = crit.ShortCutColor();
		if ((!ModManager.MMF || !MMF.cfgShowUnderwaterShortcuts.Value) && room.water && pos.y < room.defaultWaterLevel && room.GetTile(pos).Terrain != Room.Tile.TerrainType.ShortcutEntrance && (DisplayLayer(pos.x, pos.y) > 0 || room.waterInFrontOfTerrain))
		{
			color = new Color(0f, 0.007843138f, 0f);
		}
		if (ModManager.MMF && color.grayscale < 0.15f)
		{
			color = Color.Lerp(color, new Color(1f, 1f, 1f), 0.15f);
		}
		return color;
	}

	public void NewRoom()
	{
		ClearSprites();
		if (!room.shortCutsReady)
		{
			waitingForRoomToGenerateShortcuts = true;
			return;
		}
		sprites.Clear();
		entranceSprites = new FSprite[0, 0];
		entranceSpriteColors = new Color[0];
		GenerateSprites();
	}

	public void GenerateSprites()
	{
		sprites.Clear();
		lightness.Clear();
		for (int i = 0; i < room.TileWidth; i++)
		{
			for (int j = 0; j < room.TileHeight; j++)
			{
				if (room.GetTile(i, j).shortCut == 1)
				{
					int roomCoordHash = GetRoomCoordHash(i, j);
					FSprite fSprite = new FSprite("pixel");
					fSprite.scale = 15f;
					fSprite.color = new Color(1f, 0f, 0f);
					if (room.GetTile(i, j).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
					{
						camera.ReturnFContainer("Background").AddChild(fSprite);
						fSprite.scale = 20f;
					}
					else if (DisplayLayer(i, j) == 0)
					{
						fSprite.shader = shortcutShaders[0];
						camera.ReturnFContainer((j < room.defaultWaterLevel && room.waterInFrontOfTerrain) ? ((ModManager.MMF && MMF.cfgShowUnderwaterShortcuts.Value) ? "GrabShaders" : "Items") : "Shortcuts").AddChild(fSprite);
					}
					else
					{
						fSprite.shader = shortcutShaders[0];
						camera.ReturnFContainer("BackgroundShortcuts").AddChild(fSprite);
					}
					sprites.Add(roomCoordHash, fSprite);
					lightness.Add(roomCoordHash, 0f);
				}
			}
		}
		entranceSpriteLocations = new Vector2[room.shortcuts.Length];
		entranceSprites = new FSprite[room.shortcuts.Length, 2];
		entranceSpriteColors = new Color[room.shortcuts.Length];
		entraceSpriteToRoomExitIndex = new int[room.shortcuts.Length];
		for (int k = 0; k < entraceSpriteToRoomExitIndex.Length; k++)
		{
			entraceSpriteToRoomExitIndex[k] = -1;
		}
		for (int l = 0; l < room.shortcuts.Length; l++)
		{
			entranceSprites[l, 1] = new FSprite("Futile_White");
			entranceSprites[l, 1].shader = room.game.rainWorld.Shaders["FlatLight"];
			entranceSprites[l, 1].scale = 2.5f;
			if (room.shortcuts[l].shortCutType == ShortcutData.Type.Normal)
			{
				entranceSprites[l, 0] = new FSprite("ShortcutArrow");
			}
			else if (ModManager.MSC && room.shortcuts[l].shortCutType == ShortcutData.Type.NPCTransportation)
			{
				entranceSprites[l, 0] = new FSprite("ShortcutDoubleArrow");
			}
			else if (ModManager.MSC && room.shortcuts[l].shortCutType == ShortcutData.Type.RegionTransportation)
			{
				entranceSprites[l, 0] = new FSprite("ShortcutTransportArrow");
			}
			else if (ModManager.MSC && room.shortcuts[l].shortCutType == ShortcutData.Type.CreatureHole)
			{
				entranceSprites[l, 0] = new FSprite("ShortcutShelterSmall");
			}
			else if (room.shortcuts[l].shortCutType == ShortcutData.Type.RoomExit)
			{
				entraceSpriteToRoomExitIndex[l] = room.shortcuts[l].destNode;
				bool flag = false;
				bool flag2 = true;
				bool flag3 = false;
				bool flag4 = false;
				if (room.world.singleRoomWorld)
				{
					for (int m = 0; m < room.roomSettings.placedObjects.Count; m++)
					{
						if (room.roomSettings.placedObjects[m].type == PlacedObject.Type.ExitSymbolAncientShelter && room.roomSettings.placedObjects[m].active && room.GetTilePosition(room.roomSettings.placedObjects[m].pos) == room.shortcuts[l].StartTile)
						{
							flag = true;
							flag4 = true;
							break;
						}
						if (room.roomSettings.placedObjects[m].type == PlacedObject.Type.ExitSymbolShelter && room.roomSettings.placedObjects[m].active && room.GetTilePosition(room.roomSettings.placedObjects[m].pos) == room.shortcuts[l].StartTile)
						{
							flag = true;
							break;
						}
						if (room.roomSettings.placedObjects[m].type == PlacedObject.Type.ExitSymbolHidden && room.roomSettings.placedObjects[m].active && room.GetTilePosition(room.roomSettings.placedObjects[m].pos) == room.shortcuts[l].StartTile)
						{
							flag2 = false;
							break;
						}
					}
				}
				else
				{
					if (room.world.GetAbstractRoom(room.abstractRoom.connections[room.shortcuts[l].destNode]) != null && room.world.GetAbstractRoom(room.abstractRoom.connections[room.shortcuts[l].destNode]).shelter)
					{
						flag = true;
						flag2 = !room.world.brokenShelters[room.world.GetAbstractRoom(room.abstractRoom.connections[room.shortcuts[l].destNode]).shelterIndex];
						flag4 = room.world.GetAbstractRoom(room.abstractRoom.connections[room.shortcuts[l].destNode]).isAncientShelter;
					}
					if (ModManager.MMF && room.world.GetAbstractRoom(room.abstractRoom.connections[room.shortcuts[l].destNode]) != null && room.world.GetAbstractRoom(room.abstractRoom.connections[room.shortcuts[l].destNode]).gate && !room.abstractRoom.shelter)
					{
						flag3 = true;
					}
					for (int n = 0; n < room.roomSettings.placedObjects.Count; n++)
					{
						if (room.roomSettings.placedObjects[n].type == PlacedObject.Type.ExitSymbolAncientShelter && room.roomSettings.placedObjects[n].active && room.GetTilePosition(room.roomSettings.placedObjects[n].pos) == room.shortcuts[l].StartTile)
						{
							flag = room.IsGateRoom() || !flag;
							flag4 = true;
							break;
						}
						if (room.roomSettings.placedObjects[n].type == PlacedObject.Type.ExitSymbolShelter && room.roomSettings.placedObjects[n].active && room.GetTilePosition(room.roomSettings.placedObjects[n].pos) == room.shortcuts[l].StartTile)
						{
							flag = room.IsGateRoom() || !flag;
							break;
						}
						if (room.roomSettings.placedObjects[n].type == PlacedObject.Type.ExitSymbolHidden && room.roomSettings.placedObjects[n].active && room.GetTilePosition(room.roomSettings.placedObjects[n].pos) == room.shortcuts[l].StartTile)
						{
							flag2 = false;
							break;
						}
					}
				}
				if (!room.abstractRoom.gate && !room.world.singleRoomWorld && room.abstractRoom.connections[room.shortcuts[l].destNode] < 0)
				{
					flag2 = false;
				}
				if (flag2)
				{
					if (flag3)
					{
						entranceSprites[l, 0] = new FSprite("ShortcutGate");
					}
					else if (flag)
					{
						entranceSprites[l, 0] = new FSprite(flag4 ? "ShortcutAShelter" : "ShortcutShelter");
					}
					else
					{
						entranceSprites[l, 0] = new FSprite("ShortcutDots");
					}
				}
			}
			if (entranceSprites[l, 0] != null)
			{
				if (entranceSprites[l, 0].element.name != "ShortcutGate")
				{
					entranceSprites[l, 0].rotation = Custom.AimFromOneVectorToAnother(new Vector2(0f, 0f), -IntVector2.ToVector2(room.ShorcutEntranceHoleDirection(room.shortcuts[l].StartTile)));
				}
				entranceSpriteLocations[l] = room.MiddleOfTile(room.shortcuts[l].StartTile) + IntVector2.ToVector2(room.ShorcutEntranceHoleDirection(room.shortcuts[l].StartTile)) * 15f;
				if ((ModManager.MMF && MMF.cfgShowUnderwaterShortcuts.Value) || (room.water && room.waterInFrontOfTerrain && room.PointSubmerged(entranceSpriteLocations[l] + new Vector2(0f, 5f))))
				{
					camera.ReturnFContainer((ModManager.MMF && MMF.cfgShowUnderwaterShortcuts.Value) ? "GrabShaders" : "Items").AddChild(entranceSprites[l, 0]);
					continue;
				}
				camera.ReturnFContainer("Shortcuts").AddChild(entranceSprites[l, 0]);
				camera.ReturnFContainer("Water").AddChild(entranceSprites[l, 1]);
			}
		}
	}

	public void ClearSprites()
	{
		foreach (FSprite value in sprites.Values)
		{
			value.RemoveFromContainer();
		}
		for (int i = 0; i < entranceSprites.GetLength(0); i++)
		{
			if (entranceSprites[i, 0] != null)
			{
				entranceSprites[i, 0].RemoveFromContainer();
				entranceSprites[i, 1].RemoveFromContainer();
			}
		}
	}

	private int DisplayLayer(int x, int y)
	{
		if (room.GetTile(x, y).Terrain != 0)
		{
			return 0;
		}
		return 1;
	}

	private Color ColorFromLightness(float lightness)
	{
		if (lightness < 0.5f)
		{
			return Color.Lerp(palette.shortcutColors[0], palette.shortcutColors[1], Mathf.InverseLerp(0f, 0.5f, lightness));
		}
		return Color.Lerp(palette.shortcutColors[1], palette.shortcutColors[2], Mathf.InverseLerp(0.5f, 1f, lightness));
	}

	private int GetRoomCoordHash(int x, int y)
	{
		return x * room.TileHeight + y;
	}

	private int GetRoomCoordHash(IntVector2 coord)
	{
		return coord.x * room.TileHeight + coord.y;
	}

	private int GetRoomCoordHash(WorldCoordinate coord)
	{
		return coord.x * room.TileHeight + coord.y;
	}

	private IntVector2 UnpackRoomCoordHash(int coordHash)
	{
		return new IntVector2(coordHash / room.TileHeight, coordHash % room.TileHeight);
	}
}
