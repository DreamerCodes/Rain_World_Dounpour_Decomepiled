using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using HUD;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class OverWorld
{
	public class SpecialWarpType : ExtEnum<SpecialWarpType>
	{
		public static readonly SpecialWarpType WARP_VS_HR = new SpecialWarpType("WARP_VS_HR", register: true);

		public static readonly SpecialWarpType WARP_SINGLEROOM = new SpecialWarpType("WARP_SINGLEROOM", register: true);

		public SpecialWarpType(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public string FIRSTROOM;

	public RainWorldGame game;

	public World activeWorld;

	private WorldLoader worldLoader;

	private RegionGate reportBackToGate;

	public Region[] regions;

	public SpecialWarpType currentSpecialWarp;

	public ISpecialWarp specialWarpCallback;

	private string singleRoomWorldWarpGoal;

	public SlugcatStats.Name PlayerCharacterNumber
	{
		get
		{
			if (!game.IsStorySession)
			{
				return null;
			}
			return game.GetStorySession.saveStateNumber;
		}
	}

	public void WarpUpdate()
	{
	}

	public OverWorld(RainWorldGame game)
	{
		this.game = game;
		regions = Region.LoadAllRegions(PlayerCharacterNumber);
		LoadFirstWorld();
	}

	private void LoadFirstWorld()
	{
		string text = "SU_C04";
		bool flag = false;
		string path = Custom.RootFolderDirectory() + (Path.DirectorySeparatorChar + "setup.txt").ToLowerInvariant();
		bool flag2 = false;
		if (game.IsArenaSession)
		{
			flag = true;
			text = ((!ModManager.MSC || !(game.GetArenaGameSession.arenaSitting.gameTypeSetup.gameType == MoreSlugcatsEnums.GameTypeID.Challenge)) ? game.GetArenaGameSession.arenaSitting.GetCurrentLevel : game.GetArenaGameSession.arenaSitting.gameTypeSetup.challengeMeta.arena);
		}
		else
		{
			if ((game.manager.menuSetup.startGameCondition == ProcessManager.MenuSetup.StoryGameInitCondition.Dev || ModManager.DevTools) && File.Exists(path))
			{
				string[] array = File.ReadAllLines(path);
				text = "";
				string[] array2 = array;
				foreach (string text2 in array2)
				{
					if (text2.Contains(":"))
					{
						string[] array3 = Regex.Split(text2, ":");
						if (array3[0].Trim() == "level")
						{
							text = array3[1].Trim();
							break;
						}
					}
				}
				flag = !game.setupValues.world;
				flag2 = text != "";
			}
			if (!flag2)
			{
				if (game.manager.menuSetup.startGameCondition == ProcessManager.MenuSetup.StoryGameInitCondition.RegionSelect || game.manager.menuSetup.FastTravelInitCondition)
				{
					text = game.manager.menuSetup.regionSelectRoom;
					flag = false;
				}
				else
				{
					text = (game.session as StoryGameSession).saveState.GetSaveStateDenToUse();
					flag = false;
				}
			}
		}
		if (game.startingRoom != "" && !flag2)
		{
			text = game.startingRoom;
		}
		_ = PlayerCharacterNumber;
		string text3 = Regex.Split(text, "_")[0];
		if (!ModManager.MSC || game.manager.artificerDreamNumber == -1)
		{
			if (!flag)
			{
				bool flag3 = false;
				if (Directory.Exists(AssetManager.ResolveDirectory("World" + Path.DirectorySeparatorChar + text3)))
				{
					flag3 = true;
				}
				else if (Regex.Split(text, "_").Length > 2 && Directory.Exists(AssetManager.ResolveDirectory("World" + Path.DirectorySeparatorChar + Regex.Split(text, "_")[1])))
				{
					text3 = Regex.Split(text, "_")[1];
					flag3 = true;
				}
				if (!flag3)
				{
					flag = true;
				}
				if (ModManager.MSC && game.IsStorySession && PlayerCharacterNumber == MoreSlugcatsEnums.SlugcatStatsName.Spear && text == "GATE_OE_SU" && (game.session as StoryGameSession).saveState.cycleNumber == 0)
				{
					text3 = "SU";
				}
			}
		}
		else if (ModManager.MSC)
		{
			text = DreamsState.ArtificerDreamRooms(game.manager.artificerDreamNumber);
			_ = MoreSlugcatsEnums.SlugcatStatsName.Slugpup;
			flag = true;
		}
		if (flag)
		{
			LoadWorld(text, PlayerCharacterNumber, singleRoomWorld: true);
		}
		else
		{
			LoadWorld(text3, PlayerCharacterNumber, singleRoomWorld: false);
		}
		FIRSTROOM = text;
	}

	public void Update()
	{
		if (worldLoader != null)
		{
			worldLoader.Update();
			if (worldLoader.Finished)
			{
				WorldLoaded();
			}
		}
		WarpUpdate();
	}

	private void LoadWorld(string worldName, SlugcatStats.Name playerCharacterNumber, bool singleRoomWorld)
	{
		WorldLoader worldLoader = new WorldLoader(game, playerCharacterNumber, singleRoomWorld, worldName, GetRegion(worldName), game.setupValues);
		worldLoader.NextActivity();
		while (!worldLoader.Finished)
		{
			worldLoader.Update();
			Thread.Sleep(1);
		}
		World world = worldLoader.ReturnWorld();
		activeWorld = world;
	}

	public void SwitchWorlds(AbstractRoom gateRoom)
	{
		for (int i = 0; i < gateRoom.realizedRoom.updateList.Count; i++)
		{
			if (gateRoom.realizedRoom.updateList[i] is RegionGate)
			{
				(gateRoom.realizedRoom.updateList[i] as RegionGate).OPENCLOSE();
			}
		}
	}

	public void GateRequestsSwitchInitiation(RegionGate reportBackToGate)
	{
		this.reportBackToGate = reportBackToGate;
		AbstractRoom abstractRoom = reportBackToGate.room.abstractRoom;
		Custom.Log("Switch Worlds");
		Custom.Log("Gate:", abstractRoom.name, abstractRoom.index.ToString());
		string name = activeWorld.name;
		name = Region.GetVanillaEquivalentRegionAcronym(name);
		string[] array = Regex.Split(abstractRoom.name, "_");
		string baseAcronym = "ERROR!";
		if (array.Length == 3)
		{
			for (int i = 1; i < 3; i++)
			{
				if (array[i] != name)
				{
					baseAcronym = array[i];
					break;
				}
			}
		}
		baseAcronym = Region.GetProperRegionAcronym(game.IsStorySession ? game.StoryCharacter : null, baseAcronym);
		Custom.Log("Old world:", name);
		Custom.Log("New world:", baseAcronym);
		if (baseAcronym == "ERROR!")
		{
			return;
		}
		if (baseAcronym == "GW")
		{
			game.session.creatureCommunities.scavengerShyness = 0f;
		}
		if (ModManager.MSC)
		{
			Region region = GetRegion(name);
			WinState.ListTracker listTracker = game.GetStorySession.saveState.deathPersistentSaveData.winState.GetTracker(MoreSlugcatsEnums.EndgameID.Nomad, addIfMissing: true) as WinState.ListTracker;
			if (!listTracker.GoalAlreadyFullfilled)
			{
				Custom.Log("Journey list before gate:", listTracker.myList.Count.ToString());
				if (listTracker.myLastList.Count > listTracker.myList.Count)
				{
					Custom.Log("Stale journey max progress cleared");
					listTracker.myLastList.Clear();
				}
				if (listTracker.myList.Count == 0 || listTracker.myList[listTracker.myList.Count - 1] != GetRegion(baseAcronym).regionNumber)
				{
					Custom.Log("Journey progress updated with", region.regionNumber.ToString());
					listTracker.myList.Add(region.regionNumber);
				}
				else
				{
					Custom.Log("Journey is backtracking", listTracker.myList[listTracker.myList.Count - 1].ToString());
				}
				Custom.Log("Journey list:", listTracker.myList.Count.ToString());
				Custom.Log("Old Journey list:", listTracker.myLastList.Count.ToString());
			}
		}
		worldLoader = new WorldLoader(game, PlayerCharacterNumber, singleRoomWorld: false, baseAcronym, GetRegion(baseAcronym), game.setupValues);
		worldLoader.NextActivity();
	}

	private void WorldLoaded()
	{
		Custom.Log("New World loaded");
		World world = activeWorld;
		World world2 = worldLoader.ReturnWorld();
		AbstractRoom abstractRoom = null;
		AbstractRoom abstractRoom2 = null;
		if (reportBackToGate == null)
		{
			if (specialWarpCallback != null)
			{
				abstractRoom = specialWarpCallback.getSourceRoom().abstractRoom;
			}
			if (currentSpecialWarp == SpecialWarpType.WARP_VS_HR)
			{
				abstractRoom2 = world2.GetAbstractRoom("HR_C01");
			}
			if (currentSpecialWarp == SpecialWarpType.WARP_SINGLEROOM)
			{
				abstractRoom2 = world2.GetAbstractRoom(singleRoomWorldWarpGoal);
			}
			activeWorld = world2;
			if (game.roomRealizer != null)
			{
				game.roomRealizer = new RoomRealizer(game.roomRealizer.followCreature, world2);
			}
			abstractRoom2.RealizeRoom(world2, game);
			for (int i = 0; i < game.Players.Count; i++)
			{
				if (game.Players[i].realizedCreature != null)
				{
					if (game.Players[i].realizedCreature.room != null)
					{
						game.Players[i].realizedCreature.room.RemoveObject(game.Players[i].realizedCreature);
					}
					world.GetAbstractRoom(game.Players[i].pos).RemoveEntity(game.Players[i]);
					game.Players[i].world = world2;
					WorldCoordinate worldCoordinate = new WorldCoordinate(abstractRoom2.index, 0, 0, -1);
					if (currentSpecialWarp == SpecialWarpType.WARP_VS_HR)
					{
						worldCoordinate.Tile = new IntVector2(70, 42);
					}
					game.Players[i].pos = worldCoordinate;
					world2.GetAbstractRoom(worldCoordinate).AddEntity(game.Players[i]);
					game.Players[i].realizedCreature.PlaceInRoom(abstractRoom2.realizedRoom);
					if (abstractRoom2.realizedRoom.game.session is StoryGameSession && world2.region != null && !(abstractRoom2.realizedRoom.game.session as StoryGameSession).saveState.regionStates[world2.region.regionNumber].roomsVisited.Contains(abstractRoom2.realizedRoom.abstractRoom.name))
					{
						(abstractRoom2.realizedRoom.game.session as StoryGameSession).saveState.regionStates[world2.region.regionNumber].roomsVisited.Add(abstractRoom2.realizedRoom.abstractRoom.name);
					}
				}
			}
			for (int j = 0; j < game.cameras.Length; j++)
			{
				if (currentSpecialWarp == SpecialWarpType.WARP_VS_HR)
				{
					game.cameras[j].virtualMicrophone.AllQuiet();
					game.cameras[j].MoveCamera(abstractRoom2.realizedRoom, 1);
				}
				else
				{
					game.cameras[j].virtualMicrophone.AllQuiet();
					game.cameras[j].MoveCamera(abstractRoom2.realizedRoom, -1);
				}
			}
		}
		else
		{
			abstractRoom = reportBackToGate.room.abstractRoom;
			abstractRoom2 = world2.GetAbstractRoom(abstractRoom.name);
			if (ModManager.MSC)
			{
				SyncFloodLevels(abstractRoom, abstractRoom2);
			}
			abstractRoom2.entities = abstractRoom.entities;
			abstractRoom2.creatures = abstractRoom.creatures;
			abstractRoom.realizedRoom.SetAbstractRoom(abstractRoom2);
			abstractRoom2.realizedRoom = abstractRoom.realizedRoom;
			abstractRoom.realizedRoom.world = world2;
			world2.activeRooms.Add(abstractRoom.realizedRoom);
			activeWorld = world2;
			if (game.roomRealizer != null)
			{
				game.roomRealizer = new RoomRealizer(game.roomRealizer.followCreature, world2);
			}
			int abstractNode = -1;
			for (int k = 0; k < abstractRoom2.nodes.Length; k++)
			{
				if (abstractRoom2.nodes[k].type == AbstractRoomNode.Type.Exit && k < abstractRoom2.connections.Length && abstractRoom2.connections[k] > -1)
				{
					abstractNode = k;
					break;
				}
			}
			for (int l = 0; l < abstractRoom2.entities.Count; l++)
			{
				if (!ShouldEntityBeMovedToNewRegion(abstractRoom2.entities[l]))
				{
					continue;
				}
				abstractRoom.entities[l].world = world2;
				abstractRoom.entities[l].pos.room = abstractRoom2.index;
				abstractRoom.entities[l].pos.abstractNode = abstractNode;
				abstractRoom2.realizedRoom.aimap.NewWorld(abstractRoom2.index);
				if (abstractRoom.entities[l] is AbstractCreature && (abstractRoom.entities[l] as AbstractCreature).creatureTemplate.AI)
				{
					(abstractRoom.entities[l] as AbstractCreature).abstractAI.NewWorld(world2);
					(abstractRoom.entities[l] as AbstractCreature).InitiateAI();
					(abstractRoom.entities[l] as AbstractCreature).abstractAI.RealAI.NewRoom(abstractRoom2.realizedRoom);
					if ((abstractRoom.entities[l] as AbstractCreature).creatureTemplate.type == CreatureTemplate.Type.Overseer && ((abstractRoom.entities[l] as AbstractCreature).abstractAI as OverseerAbstractAI).playerGuide)
					{
						KillPlayerGuideInNewWorld(world2, abstractRoom.entities[l] as AbstractCreature);
					}
				}
			}
		}
		for (int m = 0; m < game.Players.Count; m++)
		{
			if (game.Players[m].realizedCreature != null && (game.Players[m].realizedCreature as Player).objectInStomach != null)
			{
				(game.Players[m].realizedCreature as Player).objectInStomach.world = world2;
			}
		}
		for (int num = game.shortcuts.transportVessels.Count - 1; num >= 0; num--)
		{
			if (!activeWorld.region.IsRoomInRegion(game.shortcuts.transportVessels[num].room.index))
			{
				game.shortcuts.transportVessels.RemoveAt(num);
			}
		}
		for (int num2 = game.shortcuts.betweenRoomsWaitingLobby.Count - 1; num2 >= 0; num2--)
		{
			if (!activeWorld.region.IsRoomInRegion(game.shortcuts.betweenRoomsWaitingLobby[num2].room.index))
			{
				game.shortcuts.betweenRoomsWaitingLobby.RemoveAt(num2);
			}
		}
		for (int num3 = game.shortcuts.borderTravelVessels.Count - 1; num3 >= 0; num3--)
		{
			if (!activeWorld.region.IsRoomInRegion(game.shortcuts.borderTravelVessels[num3].room.index))
			{
				game.shortcuts.borderTravelVessels.RemoveAt(num3);
			}
		}
		bool flag = false;
		if (reportBackToGate == null)
		{
			if (specialWarpCallback != null)
			{
				specialWarpCallback.NewWorldLoaded();
			}
			specialWarpCallback = null;
			flag = true;
		}
		else
		{
			reportBackToGate.NewWorldLoaded();
			reportBackToGate = null;
		}
		worldLoader = null;
		for (int n = 0; n < game.cameras.Length; n++)
		{
			game.cameras[n].hud.ResetMap(new Map.MapData(world2, game.rainWorld));
			game.cameras[n].dayNightNeedsRefresh = true;
			if (game.cameras[n].hud.textPrompt.subregionTracker != null)
			{
				game.cameras[n].hud.textPrompt.subregionTracker.lastShownRegion = 0;
			}
		}
		if (!flag)
		{
			world.regionState.AdaptRegionStateToWorld(-1, abstractRoom2.index);
			world.regionState.gatesPassedThrough[world.GetAbstractRoom(abstractRoom.name).gateIndex] = true;
			world2.regionState.gatesPassedThrough[world2.GetAbstractRoom(abstractRoom.name).gateIndex] = true;
		}
		if (world.regionState != null)
		{
			world.regionState.world = null;
		}
		if (ModManager.MSC)
		{
			world2.SpawnPupNPCs();
		}
		world2.rainCycle.baseCycleLength = world.rainCycle.baseCycleLength;
		world2.rainCycle.cycleLength = world.rainCycle.cycleLength;
		world2.rainCycle.timer = world.rainCycle.timer;
		world2.rainCycle.duskPalette = world.rainCycle.duskPalette;
		world2.rainCycle.nightPalette = world.rainCycle.nightPalette;
		world2.rainCycle.dayNightCounter = world.rainCycle.dayNightCounter;
		if (ModManager.MSC)
		{
			if (world.rainCycle.timer == 0)
			{
				world2.rainCycle.preTimer = world.rainCycle.preTimer;
				world2.rainCycle.maxPreTimer = world.rainCycle.maxPreTimer;
			}
			else
			{
				world2.rainCycle.preTimer = 0;
				world2.rainCycle.maxPreTimer = 0;
			}
		}
		if (ModManager.MMF)
		{
			GC.Collect();
		}
	}

	private bool ShouldEntityBeMovedToNewRegion(AbstractWorldEntity ent)
	{
		return true;
	}

	private void KillPlayerGuideInNewWorld(World newWorld, AbstractCreature oldGuide)
	{
		Custom.Log("looking for player guide in new world");
		for (int i = 0; i < newWorld.NumberOfRooms; i++)
		{
			for (int j = 0; j < newWorld.GetAbstractRoom(i + newWorld.firstRoomIndex).creatures.Count; j++)
			{
				if (newWorld.GetAbstractRoom(i + newWorld.firstRoomIndex).creatures[j].creatureTemplate.type == CreatureTemplate.Type.Overseer && newWorld.GetAbstractRoom(i + newWorld.firstRoomIndex).creatures[j] != oldGuide && (newWorld.GetAbstractRoom(i + newWorld.firstRoomIndex).creatures[j].abstractAI as OverseerAbstractAI).playerGuide)
				{
					AbstractCreature abstractCreature = newWorld.GetAbstractRoom(i + newWorld.firstRoomIndex).creatures[j];
					if (abstractCreature.realizedCreature != null)
					{
						abstractCreature.realizedCreature.Destroy();
					}
					abstractCreature.Destroy();
					newWorld.GetAbstractRoom(i + newWorld.firstRoomIndex).RemoveEntity(abstractCreature);
					Custom.LogWarning("spawned player guide killed in offscreenden!");
					return;
				}
			}
		}
	}

	public Region GetRegion(AbstractRoom room)
	{
		string[] array = Regex.Split(room.name, "_");
		if (array.Length == 2)
		{
			return GetRegion(array[0]);
		}
		return null;
	}

	public Region GetRegion(string rName)
	{
		for (int i = 0; i < regions.Length; i++)
		{
			if (regions[i].name == rName)
			{
				return regions[i];
			}
		}
		Custom.LogWarning("no region by name:", rName);
		return null;
	}

	public void InitiateSpecialWarp(SpecialWarpType warp, ISpecialWarp callback)
	{
		reportBackToGate = null;
		currentSpecialWarp = warp;
		specialWarpCallback = callback;
		Custom.Log($"Switch Worlds Special! {warp}");
		if (warp == SpecialWarpType.WARP_VS_HR)
		{
			worldLoader = new WorldLoader(game, PlayerCharacterNumber, singleRoomWorld: false, "HR", GetRegion("HR"), game.setupValues);
			worldLoader.NextActivity();
		}
	}

	public void InitiateSpecialWarp_SingleRoom(ISpecialWarp callback, string roomName)
	{
		reportBackToGate = null;
		currentSpecialWarp = SpecialWarpType.WARP_SINGLEROOM;
		specialWarpCallback = callback;
		singleRoomWorldWarpGoal = roomName;
		Custom.Log("Switch Worlds Special to single room", singleRoomWorldWarpGoal);
		worldLoader = new WorldLoader(game, PlayerCharacterNumber, singleRoomWorld: true, singleRoomWorldWarpGoal, null, game.setupValues);
		worldLoader.NextActivity();
	}

	private void SyncFloodLevels(AbstractRoom oldWorldRoom, AbstractRoom newWorldRoom)
	{
		if (game.globalRain.drainWorldFlood > 0f)
		{
			float y = oldWorldRoom.world.RoomToWorldPos(default(Vector2), oldWorldRoom.index).y;
			float y2 = newWorldRoom.world.RoomToWorldPos(default(Vector2), newWorldRoom.index).y;
			float num = game.globalRain.drainWorldFlood - y;
			game.globalRain.drainWorldFlood = y2 + num;
		}
		else
		{
			game.globalRain.drainWorldFlood = 0f;
		}
		float y3 = oldWorldRoom.world.RoomToWorldPos(default(Vector2), oldWorldRoom.index).y;
		float y4 = newWorldRoom.world.RoomToWorldPos(default(Vector2), newWorldRoom.index).y;
		float num2 = game.globalRain.flood - y3;
		game.globalRain.flood = y4 + num2 + game.globalRain.drainWorldFlood;
	}
}
