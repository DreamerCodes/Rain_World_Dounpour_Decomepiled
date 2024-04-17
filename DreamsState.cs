using System.Collections.Generic;
using System.Text.RegularExpressions;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class DreamsState
{
	public class DreamID : ExtEnum<DreamID>
	{
		public static readonly DreamID MoonFriend = new DreamID("MoonFriend", register: true);

		public static readonly DreamID MoonThief = new DreamID("MoonThief", register: true);

		public static readonly DreamID Pebbles = new DreamID("Pebbles", register: true);

		public static readonly DreamID GuideA = new DreamID("GuideA", register: true);

		public static readonly DreamID GuideB = new DreamID("GuideB", register: true);

		public static readonly DreamID GuideC = new DreamID("GuideC", register: true);

		public static readonly DreamID FamilyA = new DreamID("FamilyA", register: true);

		public static readonly DreamID FamilyB = new DreamID("FamilyB", register: true);

		public static readonly DreamID FamilyC = new DreamID("FamilyC", register: true);

		public static readonly DreamID VoidDreamSlugcatUp = new DreamID("VoidDreamSlugcatUp", register: true);

		public static readonly DreamID VoidDreamSlugcatDown = new DreamID("VoidDreamSlugcatDown", register: true);

		public DreamID(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public int[] integers;

	public int[] unrecognizedIntegers;

	public List<string> unrecognizedSaveStrings;

	public DreamID eventDream;

	private DreamID upcomingDream;

	public bool guideHasShownMoonThisRound;

	public bool guideHasShownHimselfToPlayer;

	public bool hasShownDreamSlugcat;

	public int cyclesSinceLastDream
	{
		get
		{
			return integers[0];
		}
		set
		{
			integers[0] = value;
		}
	}

	public int cyclesSinceLastFamilyDream
	{
		get
		{
			return integers[1];
		}
		set
		{
			integers[1] = value;
		}
	}

	public int cyclesSinceLastGuideDream
	{
		get
		{
			return integers[2];
		}
		set
		{
			integers[2] = value;
		}
	}

	public int familyThread
	{
		get
		{
			return integers[3];
		}
		set
		{
			integers[3] = value;
		}
	}

	public int guideThread
	{
		get
		{
			return integers[4];
		}
		set
		{
			integers[4] = value;
		}
	}

	public int inGWOrSHCounter
	{
		get
		{
			return integers[5];
		}
		set
		{
			integers[5] = value;
		}
	}

	public bool everSleptInSB
	{
		get
		{
			return integers[6] == 1;
		}
		set
		{
			integers[6] = (value ? 1 : 0);
		}
	}

	public bool everSleptInSB_S01
	{
		get
		{
			return integers[7] == 1;
		}
		set
		{
			integers[7] = (value ? 1 : 0);
		}
	}

	public bool everAteMoonNeuron
	{
		get
		{
			return integers[8] == 1;
		}
		set
		{
			integers[8] = (value ? 1 : 0);
		}
	}

	public bool AnyDreamComingUp => upcomingDream != null;

	public DreamID UpcomingDreamID => upcomingDream;

	public DreamsState()
	{
		integers = new int[9];
		unrecognizedIntegers = null;
		unrecognizedSaveStrings = new List<string>();
	}

	public void EndOfCycleProgress(SaveState saveState, string currentRegion, string denPosition)
	{
		bool flag = everSleptInSB;
		bool flag2 = everSleptInSB_S01;
		StaticEndOfCycleProgress(saveState, currentRegion, denPosition, ref integers[0], ref integers[1], ref integers[2], ref integers[5], ref upcomingDream, ref eventDream, ref flag, ref flag2, ref guideHasShownHimselfToPlayer, ref integers[4], ref guideHasShownMoonThisRound, ref integers[3]);
		everSleptInSB = flag;
		everSleptInSB_S01 = flag2;
	}

	public bool IfSleepNowIsThereADreamComingUp(SaveState saveState, string currentRegion, string denPosition, bool updateData = true)
	{
		if (saveState != null && ((ModManager.Expedition && saveState.progression.rainWorld.ExpeditionMode) || saveState.malnourished))
		{
			return false;
		}
		int num = cyclesSinceLastDream;
		int num2 = cyclesSinceLastFamilyDream;
		int num3 = cyclesSinceLastGuideDream;
		int num4 = inGWOrSHCounter;
		DreamID dreamID = upcomingDream;
		DreamID dreamID2 = eventDream;
		bool flag = everSleptInSB;
		bool flag2 = everSleptInSB_S01;
		bool flag3 = guideHasShownHimselfToPlayer;
		int num5 = guideThread;
		bool flag4 = guideHasShownMoonThisRound;
		int num6 = familyThread;
		if (ModManager.MSC && !updateData)
		{
			Custom.Log("Dreamstate safety check");
			int num7 = cyclesSinceLastDream;
			int num8 = num2;
			int num9 = num3;
			int num10 = num4;
			bool flag5 = flag;
			bool flag6 = flag2;
			int num11 = num5;
			int num12 = num6;
			StaticEndOfCycleProgress(saveState, currentRegion, denPosition, ref num, ref num2, ref num3, ref num4, ref dreamID, ref dreamID2, ref flag, ref flag2, ref flag3, ref num5, ref flag4, ref num6);
			num = num7;
			num2 = num8;
			num3 = num9;
			num4 = num10;
			flag = flag5;
			flag2 = flag6;
			num5 = num11;
			num6 = num12;
			return dreamID != null;
		}
		StaticEndOfCycleProgress(saveState, currentRegion, denPosition, ref num, ref num2, ref num3, ref num4, ref dreamID, ref dreamID2, ref flag, ref flag2, ref flag3, ref num5, ref flag4, ref num6);
		return dreamID != null;
	}

	private static void StaticEndOfCycleProgress(SaveState saveState, string currentRegion, string denPosition, ref int cyclesSinceLastDream, ref int cyclesSinceLastFamilyDream, ref int cyclesSinceLastGuideDream, ref int inGWOrSHCounter, ref DreamID upcomingDream, ref DreamID eventDream, ref bool everSleptInSB, ref bool everSleptInSB_S01, ref bool guideHasShownHimselfToPlayer, ref int guideThread, ref bool guideHasShownMoonThisRound, ref int familyThread)
	{
		if (saveState != null && ((ModManager.Expedition && saveState.progression.rainWorld.ExpeditionMode) || saveState.malnourished))
		{
			upcomingDream = null;
			return;
		}
		cyclesSinceLastDream++;
		cyclesSinceLastFamilyDream++;
		cyclesSinceLastGuideDream++;
		if (inGWOrSHCounter > 0)
		{
			inGWOrSHCounter++;
		}
		else if (currentRegion == "GW" || currentRegion == "SH")
		{
			inGWOrSHCounter = 1;
		}
		upcomingDream = null;
		if (eventDream != null)
		{
			cyclesSinceLastDream = 0;
			upcomingDream = eventDream;
		}
		else
		{
			if (saveState == null)
			{
				return;
			}
			if (ModManager.MSC && saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
			{
				Custom.Log("arty family check", cyclesSinceLastFamilyDream.ToString());
				if (saveState.progression.rainWorld.processManager.currentMainLoop is RainWorldGame && (saveState.progression.rainWorld.processManager.currentMainLoop as RainWorldGame).setupValues.artificerDreamTest > -1)
				{
					upcomingDream = new DreamID(ExtEnum<DreamID>.values.GetEntry(MoreSlugcatsEnums.DreamID.ArtificerFamilyA.Index + (saveState.progression.rainWorld.processManager.currentMainLoop as RainWorldGame).setupValues.artificerDreamTest));
				}
				else if (saveState.deathPersistentSaveData.altEnding || familyThread > 5)
				{
					if (cyclesSinceLastDream > 5)
					{
						upcomingDream = MoreSlugcatsEnums.DreamID.ArtificerNightmare;
						cyclesSinceLastDream = Random.Range(-30, -9);
					}
				}
				else
				{
					if (saveState.cycleNumber <= 3 || saveState.deathPersistentSaveData.altEnding)
					{
						return;
					}
					DreamID dreamID = null;
					switch (familyThread)
					{
					case 0:
						if (cyclesSinceLastFamilyDream > 2)
						{
							dreamID = MoreSlugcatsEnums.DreamID.ArtificerFamilyA;
						}
						break;
					case 1:
						if (cyclesSinceLastFamilyDream > 3)
						{
							dreamID = MoreSlugcatsEnums.DreamID.ArtificerFamilyB;
						}
						break;
					case 2:
						if (cyclesSinceLastFamilyDream > 5)
						{
							dreamID = MoreSlugcatsEnums.DreamID.ArtificerFamilyC;
						}
						break;
					case 3:
						if (cyclesSinceLastFamilyDream > 4)
						{
							dreamID = MoreSlugcatsEnums.DreamID.ArtificerFamilyD;
						}
						break;
					case 4:
						if (cyclesSinceLastFamilyDream > 6)
						{
							dreamID = MoreSlugcatsEnums.DreamID.ArtificerFamilyE;
						}
						break;
					case 5:
						if (cyclesSinceLastFamilyDream > 4)
						{
							dreamID = MoreSlugcatsEnums.DreamID.ArtificerFamilyF;
						}
						break;
					}
					if (dreamID != null)
					{
						familyThread++;
						upcomingDream = dreamID;
						cyclesSinceLastDream = 0;
						cyclesSinceLastFamilyDream = 0;
					}
					else if (cyclesSinceLastDream > 7)
					{
						upcomingDream = MoreSlugcatsEnums.DreamID.ArtificerNightmare;
						cyclesSinceLastDream = Random.Range(-6, 5);
					}
				}
				return;
			}
			if (ModManager.MSC && saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
			{
				if (saveState.progression.rainWorld.processManager.currentMainLoop is RainWorldGame && (saveState.progression.rainWorld.processManager.currentMainLoop as RainWorldGame).setupValues.artificerDreamTest > -1)
				{
					upcomingDream = new DreamID(ExtEnum<DreamID>.values.GetEntry(MoreSlugcatsEnums.DreamID.Gourmand1.Index + (saveState.progression.rainWorld.processManager.currentMainLoop as RainWorldGame).setupValues.artificerDreamTest));
				}
				else
				{
					if (cyclesSinceLastDream <= 3)
					{
						return;
					}
					DreamID dreamID2 = null;
					switch (familyThread)
					{
					case 0:
						if (cyclesSinceLastFamilyDream > 2)
						{
							dreamID2 = MoreSlugcatsEnums.DreamID.Gourmand1;
						}
						break;
					case 1:
						if (cyclesSinceLastFamilyDream > 5)
						{
							dreamID2 = MoreSlugcatsEnums.DreamID.Gourmand2;
						}
						break;
					case 2:
						if (cyclesSinceLastFamilyDream > 6)
						{
							dreamID2 = MoreSlugcatsEnums.DreamID.Gourmand3;
						}
						break;
					case 3:
						if (cyclesSinceLastFamilyDream > 7)
						{
							dreamID2 = MoreSlugcatsEnums.DreamID.Gourmand4;
						}
						break;
					case 4:
						if (cyclesSinceLastFamilyDream > 7)
						{
							dreamID2 = MoreSlugcatsEnums.DreamID.Gourmand5;
						}
						break;
					}
					if (dreamID2 != null)
					{
						familyThread++;
						upcomingDream = dreamID2;
						cyclesSinceLastDream = 0;
						cyclesSinceLastFamilyDream = 0;
					}
				}
				return;
			}
			if (!everSleptInSB && currentRegion == "SB")
			{
				everSleptInSB = true;
				cyclesSinceLastDream = 0;
				upcomingDream = DreamID.VoidDreamSlugcatUp;
				return;
			}
			if (!everSleptInSB_S01 && denPosition == "SB_S01")
			{
				everSleptInSB_S01 = true;
				cyclesSinceLastDream = 0;
				upcomingDream = DreamID.VoidDreamSlugcatDown;
				return;
			}
			if (((saveState.cycleNumber > 2 && cyclesSinceLastDream > 1) & guideHasShownHimselfToPlayer) && saveState.miscWorldSaveData.playerGuideState.likesPlayer > -0.75f)
			{
				DreamID dreamID3 = null;
				if (guideThread == 0)
				{
					dreamID3 = DreamID.GuideA;
				}
				else if (guideThread <= (int)DreamID.GuideA && (guideHasShownMoonThisRound || inGWOrSHCounter > 1))
				{
					dreamID3 = DreamID.GuideB;
				}
				else if (guideThread <= (int)DreamID.GuideB && currentRegion == "SL" && (!ModManager.MSC || denPosition != "SL_S13"))
				{
					dreamID3 = DreamID.GuideC;
				}
				if (dreamID3 != null && dreamID3.Index != -1)
				{
					guideThread = (int)dreamID3;
					Custom.Log("guideThread increase to:", guideThread.ToString());
					upcomingDream = dreamID3;
					cyclesSinceLastDream = 0;
					cyclesSinceLastGuideDream = 0;
					return;
				}
			}
			if (cyclesSinceLastDream <= 2)
			{
				return;
			}
			DreamID dreamID4 = null;
			switch (familyThread)
			{
			case 0:
				if (cyclesSinceLastFamilyDream > 6)
				{
					dreamID4 = DreamID.FamilyA;
				}
				break;
			case 1:
				if (cyclesSinceLastFamilyDream > 7)
				{
					dreamID4 = DreamID.FamilyB;
				}
				break;
			case 2:
				if (cyclesSinceLastFamilyDream > 6)
				{
					dreamID4 = DreamID.FamilyC;
				}
				break;
			case 3:
				if (cyclesSinceLastFamilyDream > 14)
				{
					dreamID4 = DreamID.VoidDreamSlugcatUp;
				}
				break;
			}
			if (dreamID4 != null)
			{
				familyThread++;
				upcomingDream = dreamID4;
				cyclesSinceLastDream = 0;
				cyclesSinceLastFamilyDream = 0;
			}
		}
	}

	public void InitiateEventDream(DreamID evDreamID)
	{
		if (evDreamID == DreamID.MoonThief)
		{
			everAteMoonNeuron = true;
		}
		if (eventDream == null || (int)evDreamID > (int)eventDream)
		{
			eventDream = evDreamID;
		}
	}

	public override string ToString()
	{
		string text = "";
		text += "integersArray<dsB>";
		text += SaveUtils.SaveIntegerArray('.', integers, unrecognizedIntegers);
		text += "<dsA>";
		foreach (string unrecognizedSaveString in unrecognizedSaveStrings)
		{
			text = text + unrecognizedSaveString + "<dsA>";
		}
		return text;
	}

	public void FromString(string s)
	{
		unrecognizedSaveStrings.Clear();
		string[] array = Regex.Split(s, "<dsA>");
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = Regex.Split(array[i], "<dsB>");
			string text = array2[0];
			if (text != null && text == "integersArray")
			{
				unrecognizedIntegers = SaveUtils.LoadIntegersArray(array2[1], '.', integers);
			}
			else if (array[i].Trim().Length > 0 && array2.Length >= 2)
			{
				unrecognizedSaveStrings.Add(array[i]);
			}
		}
	}

	public static string ArtificerDreamRooms(int dream)
	{
		switch (dream)
		{
		case 0:
			return "GW_E02_PAST";
		case 1:
			return "GW_TOWER06";
		case 2:
		case 3:
		case 4:
			return "GW_ARTYSCENES";
		default:
			return "GW_ARTYNIGHTMARE";
		}
	}
}
