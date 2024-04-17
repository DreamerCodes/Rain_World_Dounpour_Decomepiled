using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using JollyCoop.JollyMenu;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace JollyCoop;

public static class JollyCustom
{
	private static Queue<LogElement> logCache;

	private static IntVector2[] _cachedTls = new IntVector2[100];

	public static void CreateJollyLog()
	{
		if (RainWorld.ShowLogs)
		{
			using (StreamWriter streamWriter = File.CreateText(Custom.RootFolderDirectory() + Path.DirectorySeparatorChar + "jollyLog.txt"))
			{
				streamWriter.WriteLine($"############################################\n Jolly Coop Log {0} [DEBUG LEVEL: {Custom.rainWorld.buildType}]\n");
			}
			logCache = new Queue<LogElement>();
		}
	}

	public static void Log(string logText, bool throwException = false)
	{
		if (RainWorld.ShowLogs)
		{
			if (!File.Exists(Custom.RootFolderDirectory() + Path.DirectorySeparatorChar + "jollyLog.txt"))
			{
				CreateJollyLog();
			}
			if (logCache == null)
			{
				logCache = new Queue<LogElement>();
			}
			logCache.Enqueue(new LogElement(logText, throwException));
		}
	}

	public static void WriteToLog()
	{
		if (!RainWorld.ShowLogs)
		{
			return;
		}
		if (logCache == null)
		{
			logCache = new Queue<LogElement>();
		}
		while (logCache.Count != 0)
		{
			try
			{
				using StreamWriter streamWriter = new StreamWriter(Custom.RootFolderDirectory() + Path.DirectorySeparatorChar + "jollyLog.txt", append: true);
				LogElement logElement = logCache.Dequeue();
				if (logElement.shouldThrow)
				{
					logElement.logText = "[ERROR] " + logElement.logText + "\n";
				}
				streamWriter.WriteLine(logElement.logText);
			}
			catch (Exception ex)
			{
				logCache.Enqueue(new LogElement(ex.ToString(), shouldThrow: false));
				break;
			}
		}
	}

	public static Color GenerateComplementaryColor(Color colorBase)
	{
		return GenerateComplementaryColor(RGB2HSL(colorBase));
	}

	public static Color GenerateComplementaryColor(HSLColor colorBase)
	{
		Vector3 vector = new Vector3((colorBase.hue + 0.225f) % 1f, Mathf.Clamp((colorBase.saturation + 0.225f) % 1f, 0.35f, 0.75f), Mathf.Clamp((colorBase.lightness + 0.225f) % 1f, 0.4f, 0.7f));
		return Custom.HSL2RGB(vector.x, vector.y, vector.z);
	}

	public static Color GenerateClippedInverseColor(Color colorBase)
	{
		return GenerateClippedInverseColor(RGB2HSL(colorBase));
	}

	public static Color GenerateClippedInverseColor(HSLColor colorBase)
	{
		Vector3 vector = new Vector3(colorBase.hue, Mathf.Max(1f - colorBase.saturation, 0.1f), Mathf.Max(1f - colorBase.lightness, 0.15f));
		return Custom.HSL2RGB(vector.x, vector.y, vector.z);
	}

	public static HSLColor RGB2HSL(Color color)
	{
		Vector3 vector = Custom.RGB2HSL(color);
		return new HSLColor(Mathf.Clamp(vector.x, 0f, 0.99f), vector.y, Mathf.Clamp(vector.z, 0.01f, 1f));
	}

	public static Color ColorClamp(Color color, float hue_min = -1f, float hue_max = 360f, float sat_min = -1f, float sat_max = 360f, float lit_min = -1f, float lit_max = 360f)
	{
		HSLColor hSLColor = ColorClamp(RGB2HSL(color), hue_min, hue_max, sat_min, sat_max, lit_min, lit_max);
		return Custom.HSL2RGB(hSLColor.hue, hSLColor.saturation, hSLColor.lightness);
	}

	public static HSLColor ColorClamp(HSLColor color, float hue_min = -1f, float hue_max = 360f, float sat_min = -1f, float sat_max = 360f, float lit_min = -1f, float lit_max = 360f)
	{
		return new HSLColor(Mathf.Clamp(color.hue, hue_min, hue_max), Mathf.Clamp(color.saturation, sat_min, sat_max), Mathf.Clamp(color.lightness, lit_min, lit_max));
	}

	public static string Test1()
	{
		return Encoding.UTF8.GetString(Convert.FromBase64String("R2FycmFreA=="));
	}

	public static string GetPlayerName(int playerNumber)
	{
		InGameTranslator inGameTranslator = Custom.rainWorld.inGameTranslator;
		if (!string.IsNullOrEmpty(JollyOptions(playerNumber).customPlayerName))
		{
			return JollyOptions(playerNumber).customPlayerName;
		}
		return inGameTranslator.Translate("Player <p_n>").Replace("<p_n>", (playerNumber + 1).ToString());
	}

	public static SlugcatStats.Name SlugClassMenu(int playerNumber, SlugcatStats.Name fallBack)
	{
		SlugcatStats.Name name = JollyOptions(playerNumber).playerClass;
		if (name == null || SlugcatStats.HiddenOrUnplayableSlugcat(name) || (SlugcatStats.IsSlugcatFromMSC(name) && !ModManager.MSC) || !SlugcatStats.SlugcatUnlocked(name, Custom.rainWorld))
		{
			JollyOptions(playerNumber).playerClass = fallBack;
			name = fallBack;
		}
		return name;
	}

	private static JollyPlayerOptions JollyOptions(int playerNumber)
	{
		return Custom.rainWorld.options.jollyPlayerOptionsArray[playerNumber];
	}

	public static bool ForceActivateWithMSC()
	{
		return File.Exists((Custom.RootFolderDirectory() + Path.DirectorySeparatorChar + "garrito_feature.txt").ToLowerInvariant());
	}

	public static void WarpAndRevivePlayer(AbstractCreature absPlayer, AbstractRoom newRoom, WorldCoordinate position)
	{
		if ((absPlayer.state as PlayerState).permaDead && absPlayer.realizedCreature != null && absPlayer.realizedCreature.room != null)
		{
			Player player = absPlayer.realizedCreature as Player;
			player.room.RemoveObject(absPlayer.realizedCreature);
			if (player.grasps[0] != null)
			{
				player.ReleaseGrasp(0);
			}
			if (player.grasps[1] != null)
			{
				player.ReleaseGrasp(1);
			}
			List<AbstractPhysicalObject> allConnectedObjects = player.abstractCreature.GetAllConnectedObjects();
			if (player.room != null)
			{
				for (int i = 0; i < allConnectedObjects.Count; i++)
				{
					if (allConnectedObjects[i].realizedObject != null)
					{
						player.room.RemoveObject(allConnectedObjects[i].realizedObject);
					}
				}
			}
		}
		if (absPlayer.realizedCreature == null || absPlayer.Room.realizedRoom == null || (absPlayer.state as PlayerState).permaDead || absPlayer.world != newRoom.world)
		{
			Log("Reviving null player to " + newRoom.name);
			if (absPlayer.world != newRoom.world)
			{
				absPlayer.world = newRoom.world;
				absPlayer.pos = position;
				absPlayer.Room.RemoveEntity(absPlayer);
			}
			newRoom.AddEntity(absPlayer);
			absPlayer.Move(position);
			absPlayer.realizedCreature.PlaceInRoom(newRoom.realizedRoom);
		}
		else if (absPlayer.Room.name != newRoom.name)
		{
			Log($"Moving dead player to {newRoom}, reason: null [{absPlayer.realizedCreature == null}], roomNull [{absPlayer.Room.realizedRoom == null}]");
			MovePlayerWithItems(absPlayer.realizedCreature as Player, absPlayer.Room.realizedRoom, newRoom.name, position);
		}
		(absPlayer.state as PlayerState).permaDead = false;
		if (absPlayer.realizedCreature != null)
		{
			absPlayer.realizedCreature.Stun(100);
		}
	}

	public static void MovePlayerWithItems(Player p, Room currentRoom, string newRoomName, WorldCoordinate position)
	{
		AbstractRoom abstractRoom = currentRoom.world.GetAbstractRoom(newRoomName);
		if (abstractRoom == null)
		{
			p.abstractCreature.world.GetAbstractRoom(newRoomName);
		}
		if (abstractRoom.realizedRoom == null)
		{
			currentRoom.game.world.ActivateRoom(abstractRoom);
		}
		List<AbstractPhysicalObject> allConnectedObjects = p.abstractCreature.GetAllConnectedObjects();
		for (int i = 0; i < allConnectedObjects.Count; i++)
		{
			allConnectedObjects[i].pos = abstractRoom.realizedRoom.LocalCoordinateOfNode(0);
			allConnectedObjects[i].Room.RemoveEntity(allConnectedObjects[i]);
			abstractRoom.AddEntity(allConnectedObjects[i]);
			allConnectedObjects[i].realizedObject.sticksRespawned = true;
		}
		Spear spear = null;
		if (p != null && p.spearOnBack != null && p.spearOnBack.spear != null)
		{
			spear = p.spearOnBack.spear;
		}
		if (p != null && p.grasps != null)
		{
			for (int j = 0; j < p.grasps.Length; j++)
			{
				if (p.grasps[j] != null && p.grasps[j].grabbed != null && !p.grasps[j].discontinued && p.grasps[j].grabbed is Creature)
				{
					p.ReleaseGrasp(j);
				}
			}
		}
		MSCRoomSpecificScript.RoomWarp(p, currentRoom, newRoomName, position, releaseGrasps: false);
		if (p != null && spear != null && p.spearOnBack != null && p.spearOnBack.spear != spear)
		{
			p.spearOnBack.SpearToBack(spear);
			p.abstractPhysicalObject.stuckObjects.Add(p.spearOnBack.abstractStick);
		}
	}

	public static void CallVulture(Room room, Vector2 pos, IntVector2? skyPos)
	{
		AbstractCreature abstractCreature = null;
		AbstractRoom abstractRoom = room.world.GetAbstractRoom(UnityEngine.Random.Range(room.world.firstRoomIndex, room.world.firstRoomIndex + room.world.NumberOfRooms));
		if (!ModManager.MMF || !room.game.IsArenaSession)
		{
			if (abstractRoom == room.abstractRoom || (abstractRoom.AttractionForCreature(CreatureTemplate.Type.Vulture) == AbstractRoom.CreatureRoomAttraction.Forbidden && abstractRoom.AttractionForCreature(CreatureTemplate.Type.KingVulture) == AbstractRoom.CreatureRoomAttraction.Forbidden && (!ModManager.MSC || abstractRoom.AttractionForCreature(MoreSlugcatsEnums.CreatureTemplateType.MirosVulture) == AbstractRoom.CreatureRoomAttraction.Forbidden)))
			{
				return;
			}
		}
		else
		{
			abstractRoom = room.world.offScreenDen;
		}
		CreatureTemplate.Type[] array = ((!ModManager.MSC) ? new CreatureTemplate.Type[2]
		{
			CreatureTemplate.Type.Vulture,
			CreatureTemplate.Type.KingVulture
		} : new CreatureTemplate.Type[3]
		{
			CreatureTemplate.Type.Vulture,
			CreatureTemplate.Type.KingVulture,
			MoreSlugcatsEnums.CreatureTemplateType.MirosVulture
		});
		for (int i = 0; i < abstractRoom.creatures.Count; i++)
		{
			for (int j = 0; j < array.Length; j++)
			{
				if (abstractRoom.creatures[i].state.alive && abstractRoom.creatures[i].creatureTemplate.type == array[j] && abstractRoom.AttractionForCreature(array[j]) != AbstractRoom.CreatureRoomAttraction.Forbidden)
				{
					abstractCreature = abstractRoom.creatures[i];
				}
			}
			if (abstractCreature != null)
			{
				break;
			}
		}
		if (abstractCreature == null || abstractCreature.realizedCreature != null)
		{
			return;
		}
		int num = int.MaxValue;
		int num2 = -1;
		for (int k = 0; k < room.borderExits.Length; k++)
		{
			if (!(room.borderExits[k].type == AbstractRoomNode.Type.SkyExit))
			{
				continue;
			}
			for (int l = 0; l < room.borderExits[k].borderTiles.Length; l++)
			{
				if (Custom.ManhattanDistance(room.borderExits[k].borderTiles[l], skyPos.Value) < num)
				{
					num = Custom.ManhattanDistance(room.borderExits[k].borderTiles[l], skyPos.Value);
					num2 = k + room.exitAndDenIndex.Length;
				}
			}
		}
		if (num2 < 0)
		{
			return;
		}
		int num3;
		for (num3 = SharedPhysics.RayTracedTilesArray(pos, room.MiddleOfTile(skyPos.Value), _cachedTls); num3 >= _cachedTls.Length; num3 = SharedPhysics.RayTracedTilesArray(pos, room.MiddleOfTile(skyPos.Value), _cachedTls))
		{
			Custom.LogWarning($"CallVulture ray tracing limit exceeded, extending cache to {_cachedTls.Length + 100} and trying again!");
			Array.Resize(ref _cachedTls, _cachedTls.Length + 100);
		}
		IntVector2? intVector = null;
		for (int m = 0; m < num3; m++)
		{
			if (room.aimap.TileAccessibleToCreature(_cachedTls[m], StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Vulture)))
			{
				intVector = _cachedTls[m];
				break;
			}
		}
		if (intVector.HasValue)
		{
			abstractCreature.abstractAI.SetDestination(new WorldCoordinate(room.abstractRoom.index, intVector.Value.x, intVector.Value.y, num2));
			if (abstractCreature.realizedCreature == null)
			{
				abstractCreature.Move(new WorldCoordinate(room.abstractRoom.index, -1, -1, num2));
			}
		}
	}
}
