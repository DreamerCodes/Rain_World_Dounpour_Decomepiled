using System;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class GhostWorldPresence : World.IMigrationInfluence
{
	public class GhostID : ExtEnum<GhostID>
	{
		public static readonly GhostID CC = new GhostID("CC", register: true);

		public static readonly GhostID SI = new GhostID("SI", register: true);

		public static readonly GhostID LF = new GhostID("LF", register: true);

		public static readonly GhostID SH = new GhostID("SH", register: true);

		public static readonly GhostID UW = new GhostID("UW", register: true);

		public static readonly GhostID SB = new GhostID("SB", register: true);

		public static readonly GhostID NoGhost = new GhostID("NoGhost", register: true);

		public GhostID(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public World world;

	public AbstractRoom ghostRoom;

	public GhostID ghostID = GhostID.NoGhost;

	public string songName;

	private int lastSepDeg;

	private int lastSepDegTestRoom;

	public static GhostID GetGhostID(string regionName)
	{
		GhostID result = GhostID.NoGhost;
		switch (regionName)
		{
		case "CC":
			result = GhostID.CC;
			break;
		case "SI":
			result = GhostID.SI;
			break;
		case "LF":
			result = GhostID.LF;
			break;
		case "SH":
			result = GhostID.SH;
			break;
		case "UW":
			result = GhostID.UW;
			break;
		case "SB":
			result = GhostID.SB;
			break;
		case "LC":
			if (ModManager.MSC)
			{
				result = MoreSlugcatsEnums.GhostID.LC;
			}
			break;
		case "UG":
			if (ModManager.MSC)
			{
				result = MoreSlugcatsEnums.GhostID.UG;
			}
			break;
		case "SL":
			if (ModManager.MSC)
			{
				result = MoreSlugcatsEnums.GhostID.SL;
			}
			break;
		case "CL":
			if (ModManager.MSC)
			{
				result = MoreSlugcatsEnums.GhostID.CL;
			}
			break;
		case "MS":
			if (ModManager.MSC)
			{
				result = MoreSlugcatsEnums.GhostID.MS;
			}
			break;
		}
		return result;
	}

	public static RainWorld.AchievementID PassageAchievementID(GhostID ghostID)
	{
		if (ghostID == GhostID.CC)
		{
			return RainWorld.AchievementID.GhostCC;
		}
		if (ghostID == GhostID.SI)
		{
			return RainWorld.AchievementID.GhostSI;
		}
		if (ghostID == GhostID.LF)
		{
			return RainWorld.AchievementID.GhostLF;
		}
		if (ghostID == GhostID.SH)
		{
			return RainWorld.AchievementID.GhostSH;
		}
		if (ghostID == GhostID.UW)
		{
			return RainWorld.AchievementID.GhostUW;
		}
		if (ghostID == GhostID.SB)
		{
			return RainWorld.AchievementID.GhostSB;
		}
		return RainWorld.AchievementID.None;
	}

	public static bool SpawnGhost(GhostID ghostID, int karma, int karmaCap, int ghostPreviouslyEncountered, bool playingAsRed)
	{
		if (ModManager.Expedition && Custom.rainWorld.ExpeditionMode && Custom.rainWorld.progression.currentSaveState.cycleNumber == 0)
		{
			return false;
		}
		if (Custom.rainWorld.safariMode)
		{
			return false;
		}
		if (ghostID == GhostID.UW || ghostID == GhostID.SB)
		{
			return ghostPreviouslyEncountered < 2;
		}
		if (ModManager.MSC && (ghostID == MoreSlugcatsEnums.GhostID.LC || ghostID == MoreSlugcatsEnums.GhostID.MS))
		{
			return ghostPreviouslyEncountered < 2;
		}
		if (playingAsRed)
		{
			if (ghostPreviouslyEncountered > 1)
			{
				return false;
			}
		}
		else if (ghostPreviouslyEncountered != 1)
		{
			return false;
		}
		switch (karmaCap)
		{
		case 4:
			return karma >= 4;
		case 6:
			return karma >= 5;
		default:
			if (karma < 6)
			{
				if (!ModManager.MSC || Custom.rainWorld.progression.currentSaveState.saveStateNumber != MoreSlugcatsEnums.SlugcatStatsName.Artificer)
				{
					return karma >= karmaCap;
				}
				return false;
			}
			return true;
		}
	}

	public GhostWorldPresence(World world, GhostID ghostID)
	{
		this.ghostID = ghostID;
		this.world = world;
		string text = "";
		if (ghostID == GhostID.CC)
		{
			text = "CC_C12";
			songName = "NA_32 - Else1";
		}
		else if (ghostID == GhostID.SI)
		{
			text = "SI_B11";
			songName = "NA_38 - Else7";
		}
		else if (ghostID == GhostID.LF)
		{
			text = "LF_B01";
			songName = "NA_36 - Else5";
		}
		else if (ghostID == GhostID.SH)
		{
			text = "SH_A08";
			songName = "NA_34 - Else3";
		}
		else if (ghostID == GhostID.UW)
		{
			text = "UW_A14";
			songName = "NA_35 - Else4";
		}
		else if (ghostID == GhostID.SB)
		{
			text = "SB_A10";
			songName = "NA_33 - Else2";
		}
		else if (ModManager.MSC && ghostID == MoreSlugcatsEnums.GhostID.LC)
		{
			text = "LC_highestpoint";
			songName = "NA_37 - Else6";
		}
		else if (ModManager.MSC && ghostID == MoreSlugcatsEnums.GhostID.UG)
		{
			text = "UG_C02";
			songName = "NA_06 - Past Echoes";
		}
		else if (ModManager.MSC && ghostID == MoreSlugcatsEnums.GhostID.SL)
		{
			text = "SL_WALL06";
			songName = "NA_42 - Else8";
		}
		else if (ModManager.MSC && ghostID == MoreSlugcatsEnums.GhostID.CL)
		{
			text = "CL_D05";
			songName = "NA_35 - Else4";
		}
		else if (ModManager.MSC && ghostID == MoreSlugcatsEnums.GhostID.MS)
		{
			text = "MS_COMMS";
			songName = "NA_35 - Else4";
		}
		ghostRoom = world.GetAbstractRoom(text);
		if (ghostRoom == null)
		{
			Custom.LogWarning("GHOST ROOM NOT FOUND!", text);
		}
	}

	public bool CreaturesSleepInRoom(AbstractRoom room)
	{
		if (room == null || ghostRoom == null)
		{
			return false;
		}
		if (room.index == ghostRoom.index)
		{
			return true;
		}
		return GhostMode(room, world.RoomToWorldPos(room.size.ToVector2() * 10f, room.index)) > 0.05f;
	}

	public float GhostMode(Room room, int camPos)
	{
		return GhostMode(room.abstractRoom, world.RoomToWorldPos(room.cameraPositions[camPos] + new Vector2(700f, 400f), room.abstractRoom.index));
	}

	public float GhostMode(AbstractRoom testRoom, Vector2 worldPos)
	{
		if (testRoom == null || ghostRoom == null)
		{
			return 0f;
		}
		if (testRoom.index == ghostRoom.index)
		{
			return 1f;
		}
		if (ModManager.MSC && ghostID == MoreSlugcatsEnums.GhostID.SL)
		{
			if (testRoom.name == "SL_WALL06")
			{
				return 1f;
			}
			if (testRoom.name == "SL_ROOF01")
			{
				float x = world.RoomToWorldPos(new Vector2(0f, 0f), testRoom.index).x;
				return Mathf.Lerp(0.3f, 1f, Mathf.InverseLerp(x + 5500f, x + 800f, worldPos.x));
			}
			if (testRoom.name == "SL_TEMPLE")
			{
				return 0.3f;
			}
			if (testRoom.name == "SL_STOP")
			{
				return 0.2f;
			}
			if (testRoom.name == "SL_ROOF03")
			{
				return 0.25f;
			}
			if (testRoom.name == "GATE_SL_MS")
			{
				return 0.2f;
			}
			if (testRoom.name == "SL_ACCSHAFT")
			{
				return 0.2f;
			}
			if (testRoom.name == "SL_ROOF04")
			{
				return 0.18f;
			}
			if (testRoom.name == "SL_MOONTOP")
			{
				return 0.15f;
			}
			if (testRoom.name == "SL_AI")
			{
				return 0.125f;
			}
			if (testRoom.name == "SL_A15")
			{
				return 0.11f;
			}
			if (testRoom.name == "SL_I01")
			{
				return 0.1f;
			}
			if (testRoom.name == "SL_A16")
			{
				return 0.08f;
			}
			if (testRoom.name == "SL_C09")
			{
				return 0.09f;
			}
			if (testRoom.name == "SL_H03")
			{
				float x2 = world.RoomToWorldPos(new Vector2(0f, 0f), testRoom.index).x;
				return Mathf.Lerp(0f, 0.1f, Mathf.InverseLerp(x2 + 800f, x2 + 2780f, worldPos.x));
			}
		}
		if (ModManager.MSC && ghostID == MoreSlugcatsEnums.GhostID.MS)
		{
			if (testRoom.name == "MS_COMMS")
			{
				return 1f;
			}
			if (testRoom.name == "MS_bitteraerie6")
			{
				return 0.8f;
			}
			if (testRoom.name == "MS_bitteraeriedown")
			{
				return 0.7f;
			}
			if (testRoom.name == "MS_bitteredge")
			{
				return 0.6f;
			}
			if (testRoom.name == "MS_bittersafe")
			{
				return 0.5f;
			}
			if (testRoom.name == "MS_S10")
			{
				return 0.4f;
			}
			if (testRoom.name == "MS_bitterstart")
			{
				return 0.4f;
			}
			if (testRoom.name == "MS_bitterunderground")
			{
				return 0.5f;
			}
			if (testRoom.name == "MS_bitterentrance")
			{
				return 0.5f;
			}
			if (testRoom.name == "MS_bitteraeriepipeu")
			{
				return 0.4f;
			}
			if (testRoom.name == "MS_bitteraerie2")
			{
				return 0.4f;
			}
			if (testRoom.name == "MS_pumps")
			{
				return 0.4f;
			}
			if (testRoom.name == "MS_Jtrap")
			{
				return 0.4f;
			}
			if (testRoom.name == "MS_bitteraerie3")
			{
				return 0.3f;
			}
			if (testRoom.name == "MS_bittershelter")
			{
				return 0.2f;
			}
			if (testRoom.name == "MS_bitteraerie1")
			{
				return 0.3f;
			}
			if (testRoom.name == "MS_splitsewers")
			{
				return 0.3f;
			}
			if (testRoom.name == "MS_bitterpipe")
			{
				return 0.3f;
			}
			if (testRoom.name == "MS_bitteraerie5")
			{
				return 0.2f;
			}
			if (testRoom.name == "MS_bitteraerie4")
			{
				return 0.2f;
			}
			if (testRoom.name == "MS_bittermironest")
			{
				return 0.1f;
			}
			if (testRoom.name == "MS_bittervents")
			{
				return 0.2f;
			}
			if (testRoom.name == "MS_sewerbridge")
			{
				return 0.2f;
			}
			if (testRoom.name == "MS_bitteraccess")
			{
				return 0.1f;
			}
		}
		if (ModManager.MSC && ghostID == MoreSlugcatsEnums.GhostID.CL)
		{
			if (testRoom.name == "CL_D05")
			{
				return 1f;
			}
			if (testRoom.name == "CL_D02")
			{
				return 0.6f;
			}
			if (testRoom.name == "CL_E04")
			{
				return 0.3f;
			}
			if (testRoom.name == "CL_C04")
			{
				return 0.15f;
			}
			if (testRoom.name == "CL_E03")
			{
				return 0.1f;
			}
			if (testRoom.name == "CL_A04")
			{
				return 0.2f;
			}
			if (testRoom.name == "CL_S02")
			{
				return 0.3f;
			}
		}
		if (ghostID == GhostID.SB)
		{
			return 0f;
		}
		Vector2 vector = Custom.RestrictInRect(worldPos, FloatRect.MakeFromVector2(world.RoomToWorldPos(default(Vector2), ghostRoom.index), world.RoomToWorldPos(ghostRoom.size.ToVector2() * 20f, ghostRoom.index)));
		if (!Custom.DistLess(worldPos, vector, 4000f))
		{
			if (ModManager.MSC && ghostID == MoreSlugcatsEnums.GhostID.SL && testRoom.name == "SL_ROOF02")
			{
				return 0.6f;
			}
			return 0f;
		}
		int num = DegreesOfSeparation(testRoom);
		if (ghostID == GhostID.UW)
		{
			if (num != 1)
			{
				return 0f;
			}
			return 0.3f;
		}
		if (num == -1)
		{
			return 0f;
		}
		if (ModManager.MSC && ghostID == MoreSlugcatsEnums.GhostID.UG && testRoom.name != "UG_C02" && testRoom.name != "UG_A26" && testRoom.name != "UG_D03" && testRoom.name != "UG_B06")
		{
			return 0f;
		}
		float num2 = Mathf.Pow(Mathf.InverseLerp(4000f, 500f, Vector2.Distance(worldPos, vector)), 2f) * Custom.LerpMap(num, 1f, 3f, 0.6f, 0.15f) * ((testRoom.layer == ghostRoom.layer) ? 1f : 0.6f);
		if (ModManager.MSC && ghostID == MoreSlugcatsEnums.GhostID.SL && testRoom.name == "SL_ROOF02")
		{
			return Mathf.Max(num2, 0.6f);
		}
		return num2;
	}

	private int DegreesOfSeparation(AbstractRoom testRoom)
	{
		if (testRoom.index == lastSepDegTestRoom)
		{
			return lastSepDeg;
		}
		lastSepDegTestRoom = testRoom.index;
		if (testRoom.index == ghostRoom.index)
		{
			lastSepDeg = 0;
			return 0;
		}
		int num = 100;
		for (int i = 0; i < ghostRoom.connections.Length; i++)
		{
			if (ghostRoom.connections[i] == testRoom.index)
			{
				lastSepDeg = 1;
				return 1;
			}
			if (ghostRoom.connections[i] <= -1)
			{
				continue;
			}
			AbstractRoom abstractRoom = world.GetAbstractRoom(ghostRoom.connections[i]);
			for (int j = 0; j < abstractRoom.connections.Length; j++)
			{
				if (abstractRoom.connections[j] == testRoom.index)
				{
					num = Math.Min(num, 2);
					break;
				}
				if (abstractRoom.connections[j] <= -1)
				{
					continue;
				}
				AbstractRoom abstractRoom2 = world.GetAbstractRoom(abstractRoom.connections[j]);
				for (int k = 0; k < abstractRoom2.connections.Length; k++)
				{
					if (abstractRoom2.connections[k] == testRoom.index)
					{
						num = Math.Min(num, 3);
						break;
					}
				}
			}
		}
		if (num > 3)
		{
			return -1;
		}
		lastSepDeg = num;
		return num;
	}

	public void CleanSeperationDistance()
	{
		lastSepDegTestRoom = -1;
	}

	public float AttractionValueForCreature(AbstractRoom room, CreatureTemplate.Type tp, float defValue)
	{
		if (room.index == ghostRoom.index)
		{
			return 0f;
		}
		float num = CreaturesAllowedInThisRoom(room);
		return Mathf.Lerp(Mathf.Min(defValue, num), defValue * num, 0.5f);
	}

	private float CreaturesAllowedInThisRoom(AbstractRoom room)
	{
		return Mathf.Pow(1f - GhostMode(room, world.RoomToWorldPos(room.size.ToVector2() * 10f, room.index)), 7f);
	}
}
