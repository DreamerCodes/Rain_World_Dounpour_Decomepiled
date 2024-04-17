using System.Threading;
using RWCustom;
using UnityEngine;

namespace DevInterface;

public class MapObject
{
	public class RoomRepresentation
	{
		public MapObject owner;

		public AbstractRoom room;

		public Texture2D texture;

		public FAtlasElement mapTex;

		public Vector2[] nodePositions;

		public int[] exitDirections;

		public int waterLevel;

		public RoomRepresentation(MapObject owner, AbstractRoom room, bool forceRenderMode)
		{
			this.owner = owner;
			this.room = room;
			nodePositions = new Vector2[room.nodes.Length];
			exitDirections = new int[room.connections.Length];
			if (forceRenderMode && room.realizedRoom == null)
			{
				room.realizedRoom = new Room(owner.world.game, owner.world, room);
				RoomPreparer roomPreparer = new RoomPreparer(room.realizedRoom, loadAiHeatMaps: true, falseBake: false, shortcutsOnly: true);
				while (!roomPreparer.done)
				{
					roomPreparer.Update();
					Thread.Sleep(1);
				}
				if (roomPreparer.failed)
				{
					room.realizedRoom = null;
				}
				else
				{
					room.realizedRoom.loadingProgress = 3;
				}
			}
			CreateMapTexture(room.realizedRoom);
		}

		public void CreateMapTexture(Room useRoom)
		{
			if (mapTex != null)
			{
				return;
			}
			try
			{
				mapTex = Futile.atlasManager.GetElementWithName("MapTex_" + room.name);
			}
			catch
			{
				if (useRoom == null || !useRoom.readyForAI)
				{
					return;
				}
				waterLevel = useRoom.defaultWaterLevel;
				texture = new Texture2D(useRoom.TileWidth, useRoom.TileHeight);
				for (int i = 0; i < useRoom.TileWidth; i++)
				{
					for (int num = useRoom.TileHeight - 1; num >= 0; num--)
					{
						Color color;
						if (useRoom.GetTile(i, num).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
						{
							color = ((useRoom.shortcutData(new IntVector2(i, num)).shortCutType == ShortcutData.Type.RoomExit) ? new Color(0f, 1f, 0.2f) : ((useRoom.shortcutData(new IntVector2(i, num)).shortCutType == ShortcutData.Type.CreatureHole) ? new Color(1f, 0f, 1f) : ((useRoom.shortcutData(new IntVector2(i, num)).shortCutType == ShortcutData.Type.NPCTransportation) ? new Color(0.7f, 0f, 0f) : ((useRoom.shortcutData(new IntVector2(i, num)).shortCutType == ShortcutData.Type.RegionTransportation) ? new Color(0f, 0f, 0f) : ((!(useRoom.shortcutData(new IntVector2(i, num)).shortCutType == ShortcutData.Type.Normal)) ? new Color(0.3f, 0.3f, 0.3f) : new Color(1f, 1f, 1f))))));
						}
						else if (useRoom.GetTile(i, num).Solid)
						{
							color = new Color(0.3f, 0.3f, 0.3f);
						}
						else
						{
							color = (useRoom.GetTile(i, num).wallbehind ? new Color(0.5f, 0.5f, 0.5f) : new Color(0.6f, 0.6f, 0.6f));
							if (useRoom.GetTile(i, num).Terrain == Room.Tile.TerrainType.Floor || useRoom.GetTile(i, num).Terrain == Room.Tile.TerrainType.Slope || useRoom.GetTile(i, num).verticalBeam || useRoom.GetTile(i, num).horizontalBeam)
							{
								color = new Color(0.5f, 0.3f, 0.3f);
							}
						}
						if (useRoom.GetTile(i, num).AnyWater)
						{
							color = Color.Lerp(color, new Color(0f, 0f, 1f), 0.3f);
						}
						texture.SetPixel(i, num, color);
					}
				}
				for (int j = 0; j < nodePositions.Length; j++)
				{
					nodePositions[j] = useRoom.LocalCoordinateOfNode(j).Tile.ToVector2();
				}
				for (int k = 0; k < exitDirections.Length; k++)
				{
					if (k >= useRoom.abstractRoom.nodes.Length)
					{
						Custom.LogWarning("Room has more connections than nodes:", useRoom.abstractRoom.name);
					}
					else
					{
						IntVector2 intVector = useRoom.ShorcutEntranceHoleDirection(useRoom.LocalCoordinateOfNode(k).Tile);
						int num2 = -1;
						num2 = ((intVector.x != 0) ? ((intVector.x != -1) ? 2 : 0) : ((intVector.y == -1) ? 1 : 3));
						exitDirections[k] = num2;
					}
				}
				texture.wrapMode = TextureWrapMode.Clamp;
				texture.filterMode = FilterMode.Point;
				HeavyTexturesCache.LoadAndCacheAtlasFromTexture("MapTex_" + room.name, texture, textureFromAsset: false);
				texture.Apply();
				mapTex = Futile.atlasManager.GetElementWithName("MapTex_" + room.name);
			}
		}
	}

	public World world;

	public RoomRepresentation[] roomReps;

	public int[] toRefreshRooms;

	private int roomLoaderIndex;

	public RoomPreparer roomPrep;

	private int refreshNextFrameRoom;

	public MapObject(World world, bool forceRenderMode)
	{
		this.world = world;
		roomReps = new RoomRepresentation[world.NumberOfRooms];
		for (int i = 0; i < world.NumberOfRooms; i++)
		{
			roomReps[i] = new RoomRepresentation(this, world.GetAbstractRoom(i + world.firstRoomIndex), forceRenderMode);
		}
		toRefreshRooms = new int[world.NumberOfRooms];
	}

	public void Update()
	{
		if (roomPrep != null)
		{
			for (int i = 0; i < 1000; i++)
			{
				if (roomPrep.done)
				{
					break;
				}
				roomPrep.Update();
				Thread.Sleep(1);
			}
			if (roomPrep.done)
			{
				roomReps[roomLoaderIndex].CreateMapTexture(roomPrep.room);
				toRefreshRooms[roomLoaderIndex] = 2;
				roomPrep = null;
				roomLoaderIndex++;
			}
		}
		else if (roomLoaderIndex < world.NumberOfRooms - 1)
		{
			if (roomReps[roomLoaderIndex].mapTex == null)
			{
				Room room = new Room(null, world, roomReps[roomLoaderIndex].room);
				roomPrep = new RoomPreparer(room, loadAiHeatMaps: true, falseBake: false, shortcutsOnly: false);
			}
			else
			{
				roomLoaderIndex++;
			}
		}
		for (int j = 0; j < world.NumberOfRooms; j++)
		{
			if (world.GetAbstractRoom(j + world.firstRoomIndex).realizedRoom != null)
			{
				toRefreshRooms[j] = 2;
			}
		}
		toRefreshRooms[refreshNextFrameRoom] = 2;
		if (world.game != null)
		{
			toRefreshRooms[world.game.cameras[0].room.abstractRoom.index - world.firstRoomIndex] = 2;
			refreshNextFrameRoom = world.game.cameras[0].room.abstractRoom.index - world.firstRoomIndex;
		}
	}
}
