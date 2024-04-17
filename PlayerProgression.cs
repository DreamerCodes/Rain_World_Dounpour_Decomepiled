using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Kittehface.Framework20;
using Menu;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class PlayerProgression
{
	public enum ProgressionLoadResult
	{
		PENDING_LOAD,
		SUCCESS_LOAD_EXISTING_FILE,
		SUCCESS_CREATE_NEW_FILE,
		ERROR_NEW_FILE_NO_DISK_SPACE,
		ERROR_NEW_FILE_CREATE_FAILED,
		ERROR_SAVE_DATA_MISSING,
		ERROR_NO_PROFILE,
		ERROR_MOUNT_FAILED,
		ERROR_READ_FAILED,
		ERROR_CORRUPTED_FILE
	}

	public class MiscProgressionData
	{
		public class ConditionalShelterData
		{
			public string shelterName;

			private List<SlugcatStats.Name> slugcatFoundList;

			public bool hasAnySlugcats => slugcatFoundList.Count > 0;

			public ConditionalShelterData()
			{
				shelterName = "";
				slugcatFoundList = new List<SlugcatStats.Name>();
			}

			public ConditionalShelterData(string ShelterName)
			{
				shelterName = ShelterName;
				slugcatFoundList = new List<SlugcatStats.Name>();
			}

			public ConditionalShelterData(string ShelterName, SlugcatStats.Name easyIndex)
			{
				shelterName = ShelterName;
				slugcatFoundList = new List<SlugcatStats.Name>();
				slugcatFoundList.Add(easyIndex);
			}

			public void addSlugcatIndex(SlugcatStats.Name slugcat)
			{
				if (!slugcatFoundList.Contains(slugcat))
				{
					slugcatFoundList.Add(slugcat);
				}
			}

			public void removeSlugcatIndex(SlugcatStats.Name slugcat)
			{
				if (slugcatFoundList.Contains(slugcat))
				{
					slugcatFoundList.Remove(slugcat);
				}
			}

			public string GetShelterRegion()
			{
				return shelterName.Substring(0, 2);
			}

			public new string ToString()
			{
				string empty = string.Empty;
				empty += shelterName;
				foreach (SlugcatStats.Name slugcatFound in slugcatFoundList)
				{
					empty += " : ";
					empty += slugcatFound.value;
				}
				return empty + " : <mpdC>";
			}

			public void FromString(string s)
			{
				bool flag = false;
				string[] array = Regex.Split(s, " : ");
				foreach (string value in array)
				{
					if (!flag)
					{
						shelterName = value;
					}
					else
					{
						slugcatFoundList.Add(new SlugcatStats.Name(value));
					}
					flag = true;
				}
			}

			public void DebugData()
			{
				Custom.Log("--------------debugging conditional shelter data:");
				Custom.Log(shelterName);
				foreach (SlugcatStats.Name slugcatFound in slugcatFoundList)
				{
					Custom.Log(slugcatFound.value);
				}
				Custom.Log("--------------");
			}

			public bool checkSlugcatIndex(SlugcatStats.Name slugcat)
			{
				return slugcatFoundList.Contains(slugcat);
			}

			public bool checkSlugcatIndex(string roomname, SlugcatStats.Name slugcat)
			{
				if (roomname == shelterName)
				{
					return slugcatFoundList.Contains(slugcat);
				}
				return false;
			}

			public bool checkAnySlugcatIndex()
			{
				return slugcatFoundList.Count > 0;
			}

			public bool checkAnySlugcatIndex(string roomname)
			{
				if (roomname == shelterName)
				{
					return slugcatFoundList.Count > 0;
				}
				return false;
			}
		}

		public PlayerProgression owner;

		public Dictionary<string, List<string>> discoveredShelters;

		public string menuRegion;

		public List<MultiplayerUnlocks.LevelUnlockID> levelTokens;

		public List<MultiplayerUnlocks.SandboxUnlockID> sandboxTokens;

		public List<string> unrecognizedSaveStrings;

		public int[] integers;

		public int[] unrecognizedIntegers;

		public SlugcatStats.Name currentlySelectedSinglePlayerSlugcat;

		public List<string> everPlayedArenaLevels;

		public WorldCoordinate? redsFlower;

		public Dictionary<string, List<string>> regionsVisited;

		public List<ConditionalShelterData> ConditionalShelterDiscovery;

		public int[] integersMMF;

		public int[] unrecognizedIntegersMMF;

		public Dictionary<string, List<string>> colorChoices;

		public Dictionary<string, bool> colorsEnabled;

		public Dictionary<string, SpeedRunTimer.CampaignTimeTracker> campaignTimers;

		public int[] integersMSC;

		public int[] unrecognizedIntegersMSC;

		public List<MultiplayerUnlocks.SlugcatUnlockID> classTokens;

		public List<MultiplayerUnlocks.SafariUnlockID> safariTokens;

		public List<DataPearl.AbstractDataPearl.DataPearlType> decipheredPearls;

		public List<DataPearl.AbstractDataPearl.DataPearlType> decipheredDMPearls;

		public List<DataPearl.AbstractDataPearl.DataPearlType> decipheredFuturePearls;

		public List<DataPearl.AbstractDataPearl.DataPearlType> decipheredPebblesPearls;

		public List<ChatlogData.ChatlogID> discoveredBroadcasts;

		public List<bool> completedChallenges;

		public List<int> completedChallengeTimes;

		public List<string> challengeArenaUnlocks;

		public SlugcatStats.Name cloakTimelinePosition;

		public bool hasDoneHeartReboot;

		public string saintStomachRolloverObject;

		public static List<DataPearl.AbstractDataPearl.DataPearlType> transferDecipheredPearls = new List<DataPearl.AbstractDataPearl.DataPearlType>();

		public static List<DataPearl.AbstractDataPearl.DataPearlType> transferDecipheredDMPearls = new List<DataPearl.AbstractDataPearl.DataPearlType>();

		public static List<DataPearl.AbstractDataPearl.DataPearlType> transferDecipheredFuturePearls = new List<DataPearl.AbstractDataPearl.DataPearlType>();

		public static List<DataPearl.AbstractDataPearl.DataPearlType> transferDecipheredPebblesPearls = new List<DataPearl.AbstractDataPearl.DataPearlType>();

		public static List<ChatlogData.ChatlogID> transferDiscoveredBroadcasts = new List<ChatlogData.ChatlogID>();

		public int watchedSleepScreens
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

		public int watchedDeathScreens
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

		public int watchedDeathScreensWithFlower
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

		public int watchedMalnourishScreens
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

		public int starvationTutorialCounter
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

		public int warnedAboutKarmaLossOnExit
		{
			get
			{
				return integers[6];
			}
			set
			{
				integers[6] = value;
			}
		}

		public bool redHasVisitedPebbles
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

		public bool redUnlocked
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

		public bool lookedForOldVersionSaveFile
		{
			get
			{
				return integers[9] == 1;
			}
			set
			{
				integers[9] = (value ? 1 : 0);
			}
		}

		public int redMeatEatTutorial
		{
			get
			{
				return integers[10];
			}
			set
			{
				integers[10] = value;
			}
		}

		public bool AreThereAnyDiscoveredShelters
		{
			get
			{
				if (ModManager.ModdedRegionsEnabled)
				{
					if (ConditionalShelterDiscovery.Count > 0)
					{
						List<string> list = new List<string>();
						foreach (ConditionalShelterData item in ConditionalShelterDiscovery)
						{
							if (item.checkAnySlugcatIndex())
							{
								list.Add(item.shelterName);
							}
						}
						if (list.Count > 0)
						{
							return true;
						}
					}
					return false;
				}
				foreach (KeyValuePair<string, List<string>> discoveredShelter in discoveredShelters)
				{
					if (discoveredShelter.Value != null && discoveredShelter.Value.Count > 0)
					{
						return true;
					}
				}
				return false;
			}
		}

		public SlugcatStats.Name CloakTimelinePosition
		{
			get
			{
				if (!(cloakTimelinePosition == null))
				{
					return cloakTimelinePosition;
				}
				return MoreSlugcatsEnums.SlugcatStatsName.Rivulet;
			}
		}

		public int prePebblesBroadcasts
		{
			get
			{
				return integersMSC[0];
			}
			set
			{
				integersMSC[0] = value;
				owner.SaveProgression(saveMaps: false, saveMiscProg: true);
			}
		}

		public int postPebblesBroadcasts
		{
			get
			{
				return integersMSC[1];
			}
			set
			{
				integersMSC[1] = value;
				owner.SaveProgression(saveMaps: false, saveMiscProg: true);
			}
		}

		public bool gateTutorialShown
		{
			get
			{
				return integersMMF[0] == 1;
			}
			set
			{
				integersMMF[0] = (value ? 1 : 0);
			}
		}

		public int returnExplorationTutorialCounter
		{
			get
			{
				return integersMMF[1];
			}
			set
			{
				integersMMF[1] = value;
			}
		}

		public bool beaten_SpearMaster
		{
			get
			{
				return integersMSC[3] > 0;
			}
			set
			{
				integersMSC[3] = (value ? Math.Max(1, integersMSC[3]) : 0);
			}
		}

		public bool beaten_SpearMaster_AltEnd
		{
			get
			{
				return integersMSC[3] > 1;
			}
			set
			{
				integersMSC[3] = ((!value) ? Mathf.Clamp(integersMSC[3], 0, 1) : 2);
			}
		}

		public bool beaten_Artificer
		{
			get
			{
				return integersMSC[4] == 1;
			}
			set
			{
				integersMSC[4] = (value ? 1 : 0);
			}
		}

		public bool beaten_Gourmand
		{
			get
			{
				return integersMSC[5] > 0;
			}
			set
			{
				integersMSC[5] = (value ? Math.Max(1, integersMSC[5]) : 0);
			}
		}

		public bool beaten_Gourmand_Full
		{
			get
			{
				return integersMSC[5] > 1;
			}
			set
			{
				integersMSC[5] = ((!value) ? Mathf.Clamp(integersMSC[5], 0, 1) : 2);
			}
		}

		public bool beaten_Rivulet
		{
			get
			{
				return integersMSC[6] == 1;
			}
			set
			{
				integersMSC[6] = (value ? 1 : 0);
			}
		}

		public bool beaten_Saint
		{
			get
			{
				return integersMSC[7] == 1;
			}
			set
			{
				integersMSC[7] = (value ? 1 : 0);
			}
		}

		public bool beaten_Survivor
		{
			get
			{
				return integersMSC[8] == 1;
			}
			set
			{
				integersMSC[8] = (value ? 1 : 0);
			}
		}

		public bool sporePuffTutorialShown
		{
			get
			{
				return integersMMF[2] == 1;
			}
			set
			{
				integersMMF[2] = (value ? 1 : 0);
			}
		}

		public bool deerControlTutorialShown
		{
			get
			{
				return integersMMF[3] == 1;
			}
			set
			{
				integersMMF[3] = (value ? 1 : 0);
			}
		}

		public bool beaten_Hunter
		{
			get
			{
				return integersMSC[9] == 1;
			}
			set
			{
				integersMSC[9] = (value ? 1 : 0);
			}
		}

		public int survivorEndingID
		{
			get
			{
				return integersMSC[10];
			}
			set
			{
				integersMSC[10] = value;
			}
		}

		public int monkEndingID
		{
			get
			{
				return integersMSC[11];
			}
			set
			{
				integersMSC[11] = value;
			}
		}

		public int survivorPupsAtEnding
		{
			get
			{
				return integersMSC[12];
			}
			set
			{
				integersMSC[12] = value;
			}
		}

		public int artificerEndingID
		{
			get
			{
				return integersMSC[13];
			}
			set
			{
				integersMSC[13] = value;
			}
		}

		public MiscProgressionData(PlayerProgression owner)
		{
			this.owner = owner;
			discoveredShelters = new Dictionary<string, List<string>>();
			levelTokens = new List<MultiplayerUnlocks.LevelUnlockID>();
			sandboxTokens = new List<MultiplayerUnlocks.SandboxUnlockID>();
			unrecognizedSaveStrings = new List<string>();
			integers = new int[11];
			unrecognizedIntegers = null;
			currentlySelectedSinglePlayerSlugcat = SlugcatStats.Name.White;
			everPlayedArenaLevels = new List<string>();
			regionsVisited = new Dictionary<string, List<string>>();
			foreach (string item in Region.GetFullRegionOrder())
			{
				regionsVisited[item] = new List<string>();
			}
			colorsEnabled = new Dictionary<string, bool>();
			colorChoices = new Dictionary<string, List<string>>();
			foreach (string entry in ExtEnum<SlugcatStats.Name>.values.entries)
			{
				colorChoices[entry] = new List<string>();
			}
			campaignTimers = new Dictionary<string, SpeedRunTimer.CampaignTimeTracker>();
			ConditionalShelterDiscovery = new List<ConditionalShelterData>();
			integersMMF = new int[4];
			unrecognizedIntegersMMF = null;
			integersMMF[0] = 0;
			integersMMF[1] = 3;
			integersMMF[2] = 0;
			integersMMF[3] = 0;
			classTokens = new List<MultiplayerUnlocks.SlugcatUnlockID>();
			safariTokens = new List<MultiplayerUnlocks.SafariUnlockID>();
			decipheredPearls = new List<DataPearl.AbstractDataPearl.DataPearlType>();
			decipheredPebblesPearls = new List<DataPearl.AbstractDataPearl.DataPearlType>();
			decipheredDMPearls = new List<DataPearl.AbstractDataPearl.DataPearlType>();
			decipheredFuturePearls = new List<DataPearl.AbstractDataPearl.DataPearlType>();
			discoveredBroadcasts = new List<ChatlogData.ChatlogID>();
			completedChallenges = new List<bool>();
			completedChallengeTimes = new List<int>();
			challengeArenaUnlocks = new List<string>();
			hasDoneHeartReboot = false;
			integersMSC = new int[14];
			unrecognizedIntegersMSC = null;
			cloakTimelinePosition = null;
			saintStomachRolloverObject = "0";
			integersMSC[0] = 0;
			integersMSC[1] = 0;
			integersMSC[3] = 0;
			integersMSC[4] = 0;
			integersMSC[5] = 0;
			integersMSC[6] = 0;
			integersMSC[7] = 0;
			integersMSC[8] = 0;
			integersMSC[9] = 0;
			integersMSC[10] = 0;
			integersMSC[11] = 0;
			integersMSC[12] = 0;
			integersMSC[13] = 0;
		}

		public void SaveDiscoveredShelters(ref List<string> newShelterNames)
		{
			for (int i = 0; i < newShelterNames.Count; i++)
			{
				SaveDiscoveredShelter(newShelterNames[i]);
			}
		}

		private void SaveDiscoveredShelter(string roomName)
		{
			string key = roomName.Substring(0, 2);
			updateConditionalShelters(roomName, currentlySelectedSinglePlayerSlugcat);
			if (!discoveredShelters.ContainsKey(key) || discoveredShelters[key] == null)
			{
				discoveredShelters[key] = new List<string>();
			}
			if (!discoveredShelters[key].Contains(roomName))
			{
				discoveredShelters[key].Add(roomName);
			}
		}

		public void UpdateSaintStomach(Player saintPlayer)
		{
			if (saintPlayer.objectInStomach != null)
			{
				if (saintPlayer.objectInStomach is AbstractCreature)
				{
					AbstractCreature abstractCreature = saintPlayer.objectInStomach as AbstractCreature;
					abstractCreature.pos = saintPlayer.coord;
					saintStomachRolloverObject = SaveState.AbstractCreatureToStringStoryWorld(abstractCreature);
				}
				else
				{
					saintStomachRolloverObject = saintPlayer.objectInStomach.ToString();
				}
			}
			else
			{
				saintStomachRolloverObject = "0";
			}
		}

		public override string ToString()
		{
			Custom.Log("---SAVING MISC PROG DATA");
			string text = "";
			text = text + "CURRENTSLUGCAT<mpdB>" + currentlySelectedSinglePlayerSlugcat.ToString() + "<mpdA>";
			foreach (KeyValuePair<string, List<string>> discoveredShelter in discoveredShelters)
			{
				if (discoveredShelter.Value != null && discoveredShelter.Value.Count > 0)
				{
					text = text + "SHELTERLIST<mpdB>" + discoveredShelter.Key + "<mpdB>";
					for (int i = 0; i < discoveredShelter.Value.Count; i++)
					{
						text = text + discoveredShelter.Value[i] + ((i < discoveredShelter.Value.Count - 1) ? "<mpdC>" : "");
					}
					text += "<mpdA>";
				}
			}
			if (ConditionalShelterDiscovery.Count > 0)
			{
				text += "CONDITIONALSHELTERDATA<mpdB>";
				for (int j = 0; j < ConditionalShelterDiscovery.Count; j++)
				{
					if (ConditionalShelterDiscovery[j] != null)
					{
						text += ConditionalShelterDiscovery[j].ToString();
					}
				}
				text += "<mpdA>";
			}
			if (levelTokens.Count > 0)
			{
				text += "LEVELTOKENS<mpdB>";
				for (int k = 0; k < levelTokens.Count; k++)
				{
					text += levelTokens[k];
					if (k < levelTokens.Count - 1)
					{
						text += ",";
					}
				}
				text += "<mpdA>";
			}
			if (sandboxTokens.Count > 0)
			{
				text += "SANDBOXTOKENS<mpdB>";
				for (int l = 0; l < sandboxTokens.Count; l++)
				{
					text += sandboxTokens[l];
					if (l < sandboxTokens.Count - 1)
					{
						text += ",";
					}
				}
				text += "<mpdA>";
			}
			if (ModManager.MSC)
			{
				if (classTokens.Count > 0)
				{
					text += "CLASSTOKENS<mpdB>";
					for (int m = 0; m < classTokens.Count; m++)
					{
						text += classTokens[m];
						if (m < classTokens.Count - 1)
						{
							text += ",";
						}
					}
					text += "<mpdA>";
				}
				if (safariTokens.Count > 0)
				{
					text += "SAFARITOKENS<mpdB>";
					for (int n = 0; n < safariTokens.Count; n++)
					{
						text += safariTokens[n];
						if (n < safariTokens.Count - 1)
						{
							text += ",";
						}
					}
					text += "<mpdA>";
				}
				text = text + "SAINTSTOMACH<mpdB>" + saintStomachRolloverObject + "<mpdA>";
			}
			if (everPlayedArenaLevels.Count > 0)
			{
				text += "PLAYEDARENAS<mpdB>";
				for (int num = 0; num < everPlayedArenaLevels.Count; num++)
				{
					text = text + everPlayedArenaLevels[num] + ((num < everPlayedArenaLevels.Count - 1) ? "<mpdC>" : "");
				}
				text += "<mpdA>";
			}
			if (ModManager.MSC && CloakTimelinePosition != null)
			{
				text += "CLOAKTIMELINE<mpdB>";
				text += CloakTimelinePosition.value;
				text += "<mpdA>";
			}
			if (ModManager.MSC)
			{
				if (decipheredPearls.Count > 0)
				{
					text += "LORE<mpdB>";
					for (int num2 = 0; num2 < decipheredPearls.Count; num2++)
					{
						text += decipheredPearls[num2];
						if (num2 < decipheredPearls.Count - 1)
						{
							text += ",";
						}
					}
					text += "<mpdA>";
				}
				if (decipheredPebblesPearls.Count > 0)
				{
					text += "LOREP<mpdB>";
					for (int num3 = 0; num3 < decipheredPebblesPearls.Count; num3++)
					{
						text += decipheredPebblesPearls[num3];
						if (num3 < decipheredPebblesPearls.Count - 1)
						{
							text += ",";
						}
					}
					text += "<mpdA>";
				}
				if (decipheredDMPearls.Count > 0)
				{
					text += "LOREDM<mpdB>";
					for (int num4 = 0; num4 < decipheredDMPearls.Count; num4++)
					{
						text += decipheredDMPearls[num4];
						if (num4 < decipheredDMPearls.Count - 1)
						{
							text += ",";
						}
					}
					text += "<mpdA>";
				}
				if (decipheredFuturePearls.Count > 0)
				{
					text += "LOREFUT<mpdB>";
					for (int num5 = 0; num5 < decipheredFuturePearls.Count; num5++)
					{
						text += decipheredFuturePearls[num5];
						if (num5 < decipheredFuturePearls.Count - 1)
						{
							text += ",";
						}
					}
					text += "<mpdA>";
				}
				if (hasDoneHeartReboot)
				{
					text += "HASDONEHEARTREBOOT<mpdB>1<mpdA>";
				}
			}
			if (ModManager.MSC && discoveredBroadcasts.Count > 0)
			{
				text += "BROADCASTS<mpdB>";
				for (int num6 = 0; num6 < discoveredBroadcasts.Count; num6++)
				{
					text += discoveredBroadcasts[num6];
					if (num6 < discoveredBroadcasts.Count - 1)
					{
						text += ",";
					}
				}
				text += "<mpdA>";
			}
			if (ModManager.MSC)
			{
				if (challengeArenaUnlocks.Count > 0)
				{
					text += "CHARENAS<mpdB>";
					for (int num7 = 0; num7 < challengeArenaUnlocks.Count; num7++)
					{
						text = text + challengeArenaUnlocks[num7] + ((num7 >= challengeArenaUnlocks.Count - 1) ? "" : "<mpdC>");
					}
					text += "<mpdA>";
				}
				if (completedChallenges.Count > 0)
				{
					text += "CHCLEAR<mpdB>";
					for (int num8 = 0; num8 < completedChallenges.Count; num8++)
					{
						text += (completedChallenges[num8] ? "1" : "0");
					}
					text += "<mpdA>";
				}
				if (completedChallengeTimes.Count > 0)
				{
					text += "CHCLEARTIMES<mpdB>";
					for (int num9 = 0; num9 < completedChallengeTimes.Count; num9++)
					{
						text = text + completedChallengeTimes[num9] + ((num9 >= completedChallengeTimes.Count - 1) ? "" : "<mpdC>");
					}
					text += "<mpdA>";
				}
			}
			if (ModManager.MMF)
			{
				foreach (KeyValuePair<string, bool> item in colorsEnabled)
				{
					text += "CUSTCOLORS<mpdB>";
					text = text + item.Key + "<mpdB>";
					text = text + (item.Value ? "1" : "0") + "<mpdB>";
					if (colorChoices.ContainsKey(item.Key) && colorChoices[item.Key] != null)
					{
						for (int num10 = 0; num10 < colorChoices[item.Key].Count; num10++)
						{
							text = text + colorChoices[item.Key][num10] + ((num10 >= colorChoices[item.Key].Count - 1) ? "" : "<mpdC>");
						}
					}
					text += "<mpdA>";
				}
			}
			foreach (KeyValuePair<string, SpeedRunTimer.CampaignTimeTracker> campaignTimer in campaignTimers)
			{
				text += "CAMPAIGNTIME<mpdB>";
				text = text + campaignTimer.Key + "<mpdB>";
				text = text + campaignTimer.Value.UndeterminedFreeTime + "<mpdB>";
				text = text + campaignTimer.Value.CompletedFreeTime + "<mpdB>";
				text = text + campaignTimer.Value.LostFreeTime + "<mpdB>";
				text = text + campaignTimer.Value.UndeterminedFixedTime + "<mpdB>";
				text = text + campaignTimer.Value.CompletedFixedTime + "<mpdB>";
				text += campaignTimer.Value.LostFixedTime;
				text += "<mpdA>";
			}
			text += "VISITED<mpdB>";
			bool flag = true;
			foreach (KeyValuePair<string, List<string>> item2 in regionsVisited)
			{
				if (!flag)
				{
					text += "<mpdB>";
				}
				text = text + item2.Key + "<mpdC>";
				for (int num11 = 0; num11 < item2.Value.Count; num11++)
				{
					text += item2.Value[num11];
					if (num11 < item2.Value.Count - 1)
					{
						text += ",";
					}
				}
				flag = false;
			}
			text += "<mpdA>";
			text += "INTEGERS<mpdB>";
			text += SaveUtils.SaveIntegerArray(',', integers, unrecognizedIntegers);
			text += "<mpdA>";
			if (ModManager.MMF)
			{
				text += "INTEGERSMMF<mpdB>";
				text += SaveUtils.SaveIntegerArray(',', integersMMF, unrecognizedIntegersMMF);
				text += "<mpdA>";
			}
			if (ModManager.MSC)
			{
				text += "INTEGERSMSC<mpdB>";
				text += SaveUtils.SaveIntegerArray(',', integersMSC, unrecognizedIntegersMSC);
				text += "<mpdA>";
			}
			if (redsFlower.HasValue)
			{
				string text2 = redsFlower.Value.ResolveRoomName();
				text += string.Format(CultureInfo.InvariantCulture, "REDSFLOWER<mpdB>{0}.{1}.{2}.{3}<mpdA>", (text2 != null) ? text2 : redsFlower.Value.room.ToString(), redsFlower.Value.x, redsFlower.Value.y, redsFlower.Value.abstractNode);
			}
			if (menuRegion != null)
			{
				text = text + "MENUREGION<mpdB>" + menuRegion + "<mpdA>";
			}
			foreach (string unrecognizedSaveString in unrecognizedSaveStrings)
			{
				text = text + unrecognizedSaveString + "<mpdA>";
			}
			return text;
		}

		public void FromString(string s)
		{
			unrecognizedSaveStrings.Clear();
			string[] array = Regex.Split(s, "<mpdA>");
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = Regex.Split(array[i], "<mpdB>");
				switch (array2[0])
				{
				case "CURRENTSLUGCAT":
					if (ExtEnum<SlugcatStats.Name>.values.entries.Contains(array2[1]))
					{
						currentlySelectedSinglePlayerSlugcat = new SlugcatStats.Name(array2[1]);
					}
					else
					{
						currentlySelectedSinglePlayerSlugcat = SlugcatStats.Name.White;
					}
					if (ModManager.MSC && currentlySelectedSinglePlayerSlugcat == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel)
					{
						currentlySelectedSinglePlayerSlugcat = SlugcatStats.Name.White;
					}
					break;
				case "SHELTERLIST":
				{
					discoveredShelters[array2[1]] = new List<string>();
					string[] array4 = Regex.Split(array2[2], "<mpdC>");
					for (int k = 0; k < array4.Length; k++)
					{
						if (array4[k].Length > 0)
						{
							discoveredShelters[array2[1]].Add(array4[k]);
						}
					}
					break;
				}
				case "CONDITIONALSHELTERDATA":
				{
					ConditionalShelterDiscovery.Clear();
					string[] array8 = Regex.Split(array2[1], "<mpdC>");
					if (array8.Length == 0)
					{
						break;
					}
					string[] array3 = array8;
					foreach (string text3 in array3)
					{
						if (text3.Length > 1)
						{
							ConditionalShelterData conditionalShelterData = new ConditionalShelterData();
							conditionalShelterData.FromString(text3);
							ConditionalShelterDiscovery.Add(conditionalShelterData);
						}
					}
					break;
				}
				case "MENUREGION":
					menuRegion = array2[1];
					break;
				case "LEVELTOKENS":
				{
					if (Custom.IsDigitString(array2[1]))
					{
						BackwardsCompatibilityRemix.ParseLevelTokens(array2[1], levelTokens);
						break;
					}
					levelTokens.Clear();
					string[] array3 = array2[1].Split(',');
					foreach (string text2 in array3)
					{
						if (text2 != string.Empty)
						{
							levelTokens.Add(new MultiplayerUnlocks.LevelUnlockID(text2));
						}
					}
					break;
				}
				case "SANDBOXTOKENS":
				{
					if (Custom.IsDigitString(array2[1]))
					{
						BackwardsCompatibilityRemix.ParseSandboxTokens(array2[1], sandboxTokens);
						break;
					}
					sandboxTokens.Clear();
					string[] array3 = array2[1].Split(',');
					foreach (string text4 in array3)
					{
						if (text4 != string.Empty)
						{
							sandboxTokens.Add(new MultiplayerUnlocks.SandboxUnlockID(text4));
						}
					}
					break;
				}
				case "CLASSTOKENS":
					if (ModManager.MSC)
					{
						classTokens.Clear();
						string[] array3 = array2[1].Split(',');
						foreach (string value2 in array3)
						{
							classTokens.Add(new MultiplayerUnlocks.SlugcatUnlockID(value2));
						}
					}
					else
					{
						unrecognizedSaveStrings.Add(array[i]);
					}
					break;
				case "SAFARITOKENS":
					if (ModManager.MSC)
					{
						safariTokens.Clear();
						string[] array3 = array2[1].Split(',');
						foreach (string value6 in array3)
						{
							safariTokens.Add(new MultiplayerUnlocks.SafariUnlockID(value6));
						}
					}
					else
					{
						unrecognizedSaveStrings.Add(array[i]);
					}
					break;
				case "LORE":
					if (ModManager.MSC)
					{
						decipheredPearls.Clear();
						string[] array3 = array2[1].Split(',');
						foreach (string value3 in array3)
						{
							decipheredPearls.Add(new DataPearl.AbstractDataPearl.DataPearlType(value3));
						}
					}
					else
					{
						unrecognizedSaveStrings.Add(array[i]);
					}
					break;
				case "LOREP":
					if (ModManager.MSC)
					{
						decipheredPebblesPearls.Clear();
						string[] array3 = array2[1].Split(',');
						foreach (string value7 in array3)
						{
							decipheredPebblesPearls.Add(new DataPearl.AbstractDataPearl.DataPearlType(value7));
						}
					}
					else
					{
						unrecognizedSaveStrings.Add(array[i]);
					}
					break;
				case "LOREDM":
					if (ModManager.MSC)
					{
						decipheredDMPearls.Clear();
						string[] array3 = array2[1].Split(',');
						foreach (string value5 in array3)
						{
							decipheredDMPearls.Add(new DataPearl.AbstractDataPearl.DataPearlType(value5));
						}
					}
					else
					{
						unrecognizedSaveStrings.Add(array[i]);
					}
					break;
				case "LOREFUT":
					if (ModManager.MSC)
					{
						decipheredFuturePearls.Clear();
						string[] array3 = array2[1].Split(',');
						foreach (string value4 in array3)
						{
							decipheredFuturePearls.Add(new DataPearl.AbstractDataPearl.DataPearlType(value4));
						}
					}
					else
					{
						unrecognizedSaveStrings.Add(array[i]);
					}
					break;
				case "BROADCASTS":
					if (ModManager.MSC)
					{
						discoveredBroadcasts.Clear();
						string[] array3 = array2[1].Split(',');
						foreach (string value in array3)
						{
							discoveredBroadcasts.Add(new ChatlogData.ChatlogID(value));
						}
					}
					else
					{
						unrecognizedSaveStrings.Add(array[i]);
					}
					break;
				case "INTEGERS":
					unrecognizedIntegers = SaveUtils.LoadIntegersArray(array2[1], ',', integers);
					break;
				case "INTEGERSMSC":
					if (ModManager.MSC)
					{
						unrecognizedIntegersMSC = SaveUtils.LoadIntegersArray(array2[1], ',', integersMSC);
					}
					else
					{
						unrecognizedSaveStrings.Add(array[i]);
					}
					break;
				case "INTEGERSMMF":
					if (ModManager.MMF)
					{
						unrecognizedIntegersMMF = SaveUtils.LoadIntegersArray(array2[1], ',', integersMMF);
					}
					else
					{
						unrecognizedSaveStrings.Add(array[i]);
					}
					break;
				case "HASDONEHEARTREBOOT":
					if (ModManager.MSC)
					{
						hasDoneHeartReboot = true;
					}
					else
					{
						unrecognizedSaveStrings.Add(array[i]);
					}
					break;
				case "PLAYEDARENAS":
				{
					everPlayedArenaLevels.Clear();
					string[] array4 = Regex.Split(array2[1], "<mpdC>");
					for (int n = 0; n < array4.Length; n++)
					{
						if (array4[n].Length > 0)
						{
							everPlayedArenaLevels.Add(array4[n]);
						}
					}
					break;
				}
				case "CHARENAS":
					if (ModManager.MSC)
					{
						challengeArenaUnlocks.Clear();
						string[] array5 = Regex.Split(array2[1], "<mpdC>");
						for (int l = 0; l < array5.Length; l++)
						{
							if (array5[l].Length > 0)
							{
								challengeArenaUnlocks.Add(array5[l]);
							}
						}
					}
					else
					{
						unrecognizedSaveStrings.Add(array[i]);
					}
					break;
				case "CHCLEAR":
					if (ModManager.MSC)
					{
						completedChallenges.Clear();
						for (int num4 = 0; num4 < array2[1].Length; num4++)
						{
							completedChallenges.Add(array2[1][num4] == '1');
						}
					}
					else
					{
						unrecognizedSaveStrings.Add(array[i]);
					}
					break;
				case "CHCLEARTIMES":
					if (ModManager.MSC)
					{
						completedChallengeTimes.Clear();
						string[] array9 = Regex.Split(array2[1], "<mpdC>");
						for (int num3 = 0; num3 < array9.Length; num3++)
						{
							if (array9[num3].Length > 0)
							{
								int result = -1;
								int.TryParse(array9[num3], out result);
								completedChallengeTimes.Add(result);
							}
						}
					}
					else
					{
						unrecognizedSaveStrings.Add(array[i]);
					}
					break;
				case "CUSTCOLORS":
					if (ModManager.MMF)
					{
						colorsEnabled[array2[1]] = array2[2] == "1";
						if (!colorChoices.ContainsKey(array2[1]) || colorChoices[array2[1]] == null)
						{
							colorChoices[array2[1]] = new List<string>();
						}
						else
						{
							colorChoices[array2[1]].Clear();
						}
						string[] array7 = Regex.Split(array2[3], "<mpdC>");
						for (int num2 = 0; num2 < array7.Length; num2++)
						{
							if (array7[num2] != "")
							{
								colorChoices[array2[1]].Add(array7[num2]);
							}
						}
					}
					else
					{
						unrecognizedSaveStrings.Add(array[i]);
					}
					break;
				case "CAMPAIGNTIME":
				{
					SpeedRunTimer.CampaignTimeTracker campaignTimeTracker = new SpeedRunTimer.CampaignTimeTracker();
					campaignTimeTracker.UndeterminedFreeTime = double.Parse(array2[2], NumberStyles.Any, CultureInfo.InvariantCulture);
					campaignTimeTracker.CompletedFreeTime = double.Parse(array2[3], NumberStyles.Any, CultureInfo.InvariantCulture);
					campaignTimeTracker.LostFreeTime = double.Parse(array2[4], NumberStyles.Any, CultureInfo.InvariantCulture);
					campaignTimeTracker.UndeterminedFixedTime = double.Parse(array2[5], NumberStyles.Any, CultureInfo.InvariantCulture);
					campaignTimeTracker.CompletedFixedTime = double.Parse(array2[6], NumberStyles.Any, CultureInfo.InvariantCulture);
					campaignTimeTracker.LostFixedTime = double.Parse(array2[7], NumberStyles.Any, CultureInfo.InvariantCulture);
					campaignTimers[array2[1]] = campaignTimeTracker;
					break;
				}
				case "CLOAKTIMELINE":
					if (ModManager.MSC)
					{
						cloakTimelinePosition = new SlugcatStats.Name(array2[1]);
					}
					else
					{
						unrecognizedSaveStrings.Add(array[i]);
					}
					break;
				case "SAINTSTOMACH":
					if (ModManager.MSC)
					{
						saintStomachRolloverObject = array2[1];
					}
					else
					{
						unrecognizedSaveStrings.Add(array[i]);
					}
					break;
				case "VISITED":
				{
					for (int m = 1; m < array2.Length; m++)
					{
						string[] array6 = Regex.Split(array2[m], "<mpdC>");
						regionsVisited[array6[0]] = new List<string>();
						string[] array3 = array6[1].Split(',');
						foreach (string text in array3)
						{
							if (text != "")
							{
								regionsVisited[array6[0]].Add(text);
							}
						}
					}
					break;
				}
				case "REDSFLOWER":
				{
					string[] array4 = array2[1].Split('.');
					int? num = BackwardsCompatibilityRemix.ParseRoomIndex(array4[0]);
					if (num.HasValue)
					{
						redsFlower = new WorldCoordinate(num.Value, int.Parse(array4[1], NumberStyles.Any, CultureInfo.InvariantCulture), int.Parse(array4[2], NumberStyles.Any, CultureInfo.InvariantCulture), int.Parse(array4[3], NumberStyles.Any, CultureInfo.InvariantCulture));
					}
					else
					{
						redsFlower = new WorldCoordinate(array4[0], int.Parse(array4[1], NumberStyles.Any, CultureInfo.InvariantCulture), int.Parse(array4[2], NumberStyles.Any, CultureInfo.InvariantCulture), int.Parse(array4[3], NumberStyles.Any, CultureInfo.InvariantCulture));
					}
					break;
				}
				default:
					if (array[i].Trim().Length > 0 && array2.Length >= 1)
					{
						unrecognizedSaveStrings.Add(array[i]);
					}
					break;
				}
			}
			if (!ModManager.MSC || owner.rainWorld.options == null || owner.rainWorld.options.saveSlot < 0)
			{
				return;
			}
			foreach (DataPearl.AbstractDataPearl.DataPearlType transferDecipheredPearl in transferDecipheredPearls)
			{
				SetPearlDeciphered(transferDecipheredPearl);
			}
			foreach (DataPearl.AbstractDataPearl.DataPearlType transferDecipheredDMPearl in transferDecipheredDMPearls)
			{
				SetDMPearlDeciphered(transferDecipheredDMPearl);
			}
			foreach (DataPearl.AbstractDataPearl.DataPearlType transferDecipheredFuturePearl in transferDecipheredFuturePearls)
			{
				SetFuturePearlDeciphered(transferDecipheredFuturePearl);
			}
			foreach (DataPearl.AbstractDataPearl.DataPearlType transferDecipheredPebblesPearl in transferDecipheredPebblesPearls)
			{
				SetPebblesPearlDeciphered(transferDecipheredPebblesPearl);
			}
			foreach (ChatlogData.ChatlogID transferDiscoveredBroadcast in transferDiscoveredBroadcasts)
			{
				SetBroadcastListened(transferDiscoveredBroadcast);
			}
		}

		public bool GetTokenCollected(string tokenString, bool sandbox)
		{
			if (sandbox)
			{
				try
				{
					return GetTokenCollected(new MultiplayerUnlocks.SandboxUnlockID(tokenString));
				}
				catch
				{
					return false;
				}
			}
			try
			{
				return GetTokenCollected(new MultiplayerUnlocks.LevelUnlockID(tokenString));
			}
			catch
			{
				return false;
			}
		}

		public bool GetTokenCollected(MultiplayerUnlocks.LevelUnlockID levelToken)
		{
			if (levelToken != null)
			{
				return levelTokens.Contains(levelToken);
			}
			return false;
		}

		public bool GetTokenCollected(MultiplayerUnlocks.SandboxUnlockID sandboxToken)
		{
			if (sandboxToken != null)
			{
				return sandboxTokens.Contains(sandboxToken);
			}
			return false;
		}

		public bool SetTokenCollected(MultiplayerUnlocks.LevelUnlockID levelToken)
		{
			if (levelToken == null || GetTokenCollected(levelToken))
			{
				return false;
			}
			levelTokens.Add(levelToken);
			return true;
		}

		public bool SetTokenCollected(MultiplayerUnlocks.SandboxUnlockID sandboxToken)
		{
			if (sandboxToken == null || GetTokenCollected(sandboxToken))
			{
				return false;
			}
			sandboxTokens.Add(sandboxToken);
			return true;
		}

		public bool SetTokenCollected(MultiplayerUnlocks.SlugcatUnlockID classToken)
		{
			if (classToken == null || GetTokenCollected(classToken))
			{
				return false;
			}
			classTokens.Add(classToken);
			return true;
		}

		public bool GetTokenCollected(MultiplayerUnlocks.SlugcatUnlockID classToken)
		{
			if (classToken != null)
			{
				return classTokens.Contains(classToken);
			}
			return false;
		}

		public bool SetTokenCollected(MultiplayerUnlocks.SafariUnlockID safariToken)
		{
			if (safariToken == null || GetTokenCollected(safariToken))
			{
				return false;
			}
			safariTokens.Add(safariToken);
			return true;
		}

		public bool GetTokenCollected(MultiplayerUnlocks.SafariUnlockID safariToken)
		{
			if (safariToken != null)
			{
				return safariTokens.Contains(safariToken);
			}
			return false;
		}

		public bool SetPearlDeciphered(DataPearl.AbstractDataPearl.DataPearlType pearlType)
		{
			if (pearlType == null || GetPearlDeciphered(pearlType))
			{
				return false;
			}
			decipheredPearls.Add(pearlType);
			owner.SaveProgression(saveMaps: false, saveMiscProg: true);
			return true;
		}

		public bool GetPearlDeciphered(DataPearl.AbstractDataPearl.DataPearlType pearlType)
		{
			if (pearlType != null)
			{
				return decipheredPearls.Contains(pearlType);
			}
			return false;
		}

		public bool SetPebblesPearlDeciphered(DataPearl.AbstractDataPearl.DataPearlType pearlType, bool forced = false)
		{
			if (pearlType != null && !forced)
			{
				int num = CollectionsMenu.DataPearlToFileID(pearlType);
				if (num != -1 && !Conversation.EventsFileExists(owner.rainWorld, num, MoreSlugcatsEnums.SlugcatStatsName.Artificer))
				{
					return SetPearlDeciphered(pearlType);
				}
			}
			if (pearlType == null || GetPebblesPearlDeciphered(pearlType))
			{
				return false;
			}
			decipheredPebblesPearls.Add(pearlType);
			owner.SaveProgression(saveMaps: false, saveMiscProg: true);
			return true;
		}

		public bool GetPebblesPearlDeciphered(DataPearl.AbstractDataPearl.DataPearlType pearlType)
		{
			if (pearlType != null)
			{
				return decipheredPebblesPearls.Contains(pearlType);
			}
			return false;
		}

		public bool SetDMPearlDeciphered(DataPearl.AbstractDataPearl.DataPearlType pearlType, bool forced = false)
		{
			if (pearlType != null && !forced)
			{
				int num = CollectionsMenu.DataPearlToFileID(pearlType);
				if (num != -1 && !Conversation.EventsFileExists(owner.rainWorld, num, MoreSlugcatsEnums.SlugcatStatsName.Spear))
				{
					return SetPearlDeciphered(pearlType);
				}
			}
			if (pearlType == null || GetDMPearlDeciphered(pearlType))
			{
				return false;
			}
			decipheredDMPearls.Add(pearlType);
			owner.SaveProgression(saveMaps: false, saveMiscProg: true);
			return true;
		}

		public bool GetDMPearlDeciphered(DataPearl.AbstractDataPearl.DataPearlType pearlType)
		{
			if (pearlType != null)
			{
				return decipheredDMPearls.Contains(pearlType);
			}
			return false;
		}

		public bool SetFuturePearlDeciphered(DataPearl.AbstractDataPearl.DataPearlType pearlType, bool forced = false)
		{
			if (pearlType != null && !forced)
			{
				int num = CollectionsMenu.DataPearlToFileID(pearlType);
				if (num != -1 && !Conversation.EventsFileExists(owner.rainWorld, num, MoreSlugcatsEnums.SlugcatStatsName.Saint))
				{
					return SetPearlDeciphered(pearlType);
				}
			}
			if (pearlType == null || GetFuturePearlDeciphered(pearlType))
			{
				return false;
			}
			decipheredFuturePearls.Add(pearlType);
			owner.SaveProgression(saveMaps: false, saveMiscProg: true);
			return true;
		}

		public bool GetFuturePearlDeciphered(DataPearl.AbstractDataPearl.DataPearlType pearlType)
		{
			if (pearlType != null)
			{
				return decipheredFuturePearls.Contains(pearlType);
			}
			return false;
		}

		public bool SetBroadcastListened(ChatlogData.ChatlogID chat)
		{
			if (chat == null || GetBroadcastListened(chat))
			{
				return false;
			}
			discoveredBroadcasts.Add(chat);
			owner.SaveProgression(saveMaps: false, saveMiscProg: true);
			return true;
		}

		public bool GetBroadcastListened(ChatlogData.ChatlogID chat)
		{
			if (chat != null)
			{
				return discoveredBroadcasts.Contains(chat);
			}
			return false;
		}

		public void SetCloakTimelinePosition(SlugcatStats.Name slugcat)
		{
			int num = -1;
			int num2 = -1;
			SlugcatStats.Name[] array = SlugcatStats.SlugcatTimelineOrder().ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				if (CloakTimelinePosition == array[i])
				{
					num = i;
				}
				if (slugcat == array[i])
				{
					num2 = i;
				}
			}
			if (num == -1 || num2 < num)
			{
				cloakTimelinePosition = slugcat;
			}
		}

		private void updateConditionalShelters(string room, SlugcatStats.Name slugcatIndex)
		{
			bool flag = true;
			using (List<ConditionalShelterData>.Enumerator enumerator = ConditionalShelterDiscovery.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.shelterName == room)
					{
						flag = false;
						enumerator.Current.addSlugcatIndex(slugcatIndex);
						break;
					}
				}
			}
			if (flag)
			{
				ConditionalShelterDiscovery.Add(new ConditionalShelterData(room, slugcatIndex));
			}
		}

		public List<ConditionalShelterData> GetDiscoveredSheltersInRegion(string prefix)
		{
			List<ConditionalShelterData> list = new List<ConditionalShelterData>();
			if (ConditionalShelterDiscovery != null && ConditionalShelterDiscovery.Count > 0)
			{
				foreach (ConditionalShelterData item in ConditionalShelterDiscovery)
				{
					if (item.GetShelterRegion() == prefix)
					{
						list.Add(item);
					}
				}
			}
			return list;
		}

		public List<string> GetDiscoveredShelterStringsInRegion(string prefix)
		{
			List<string> list = new List<string>();
			foreach (ConditionalShelterData item in GetDiscoveredSheltersInRegion(prefix))
			{
				list.Add(item.shelterName);
			}
			return list;
		}

		public void CleanupConditionalShelters()
		{
			List<ConditionalShelterData> list = new List<ConditionalShelterData>();
			foreach (ConditionalShelterData item in ConditionalShelterDiscovery)
			{
				if (item.hasAnySlugcats)
				{
					list.Add(item);
				}
			}
			ConditionalShelterDiscovery.Clear();
			ConditionalShelterDiscovery = list;
		}
	}

	private static readonly UserData.FileDefinition SAVE_FILE_DEFINITION = new UserData.FileDefinition("sav", useRawData: false, cloudEnabled: false, useEncryption: false, prettyPrint: false, useBinarySerialization: false, null, ps4Definition: new UserData.FileDefinition.PS4Definition(null, null, null, null, null, null, "Media/StreamingAssets/SaveIcon.png", 9437184L), switchDefinition: new UserData.FileDefinition.SwitchDefinition("rainworld", 9437184L));

	private static readonly UserData.FileDefinition SAVE_FILE_EXP_DEFINITION = new UserData.FileDefinition("exp", useRawData: false, cloudEnabled: false, useEncryption: false, prettyPrint: false, useBinarySerialization: false, null, ps4Definition: new UserData.FileDefinition.PS4Definition(null, null, null, null, null, null, "Media/StreamingAssets/SaveIcon.png", 2097152L), switchDefinition: new UserData.FileDefinition.SwitchDefinition("rainworld", 2097152L));

	private const string SAVE_KEY = "save";

	private const string SAVE_BACKUP_KEY_FORMAT = "save_{0}";

	public SaveState currentSaveState;

	public SaveState starvedSaveState;

	public Dictionary<string, Texture2D> mapDiscoveryTextures;

	public Dictionary<string, long> mapLastUpdatedTime;

	public MiscProgressionData miscProgressionData;

	public string[] regionNames;

	public string[] karmaLocks;

	public List<string> tempSheltersDiscovered;

	private bool LOAD;

	public bool onLoadMSCState;

	public bool onLoadMMFState;

	public bool onLoadJollyState;

	public bool onLoadExpeditionState;

	public Dictionary<string, bool> onLoadEnabledModsState;

	public bool gameTinkeredWith;

	public RainWorld rainWorld;

	public ProgressionLoadResult progressionLoadedResult;

	public bool suppressProgressionError;

	private UserData.File saveFileDataInMemory;

	private Hashtable restoreDataToTransfer;

	private byte[] restoreRawDataToTransfer;

	private bool canSave = true;

	private bool loadInProgress;

	private bool saveAfterLoad;

	private bool requestLoad;

	private string overrideBaseDir;

	private Action<bool> checkConsoleBackupExistsCallback;

	private Action<bool> restoreBackupCallback;

	public SlugcatStats.Name PlayingAsSlugcat
	{
		get
		{
			if (currentSaveState != null)
			{
				return currentSaveState.saveStateNumber;
			}
			return miscProgressionData.currentlySelectedSinglePlayerSlugcat;
		}
	}

	public bool progressionLoaded { get; private set; }

	public bool HasSaveData
	{
		get
		{
			if (saveFileDataInMemory != null && !loadInProgress)
			{
				return saveFileDataInMemory.Contains("save");
			}
			return false;
		}
	}

	public bool SaveDataBusy
	{
		get
		{
			if (!loadInProgress)
			{
				return UserData.Busy;
			}
			return true;
		}
	}

	public string SaveDataReadFailureError
	{
		get
		{
			if (saveFileDataInMemory != null)
			{
				return saveFileDataInMemory.executeReadError;
			}
			return null;
		}
	}

	public bool CanSave => canSave;

	public PlayerProgression(RainWorld rainWorld, bool tryLoad, bool saveAfterLoad)
		: this(rainWorld, tryLoad, saveAfterLoad, null)
	{
	}

	public PlayerProgression(RainWorld rainWorld, bool tryLoad, bool saveAfterLoad, string overrideBaseDir = null)
	{
		this.rainWorld = rainWorld;
		this.saveAfterLoad = saveAfterLoad;
		onLoadEnabledModsState = new Dictionary<string, bool>();
		LOAD = tryLoad;
		this.overrideBaseDir = overrideBaseDir;
		ReloadRegionsList();
		mapDiscoveryTextures = new Dictionary<string, Texture2D>();
		mapLastUpdatedTime = new Dictionary<string, long>();
		ReloadLocksList();
		RainWorld.LoadIndexMapsIntoMemory(RainWorld.worldVersion);
		miscProgressionData = new MiscProgressionData(this);
		tempSheltersDiscovered = new List<string>();
		if (LOAD)
		{
			LoadProgression();
		}
		if (progressionLoaded && !miscProgressionData.lookedForOldVersionSaveFile)
		{
			miscProgressionData.lookedForOldVersionSaveFile = true;
			LookForOldVersionSaveFile();
		}
		if (!Platform.initialized && !progressionLoaded)
		{
			Platform.OnRequestUserDataRead += Platform_OnRequestUserDataRead;
		}
	}

	public void ReloadRegionsList()
	{
		string path = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + "regions.txt");
		if (File.Exists(path))
		{
			regionNames = File.ReadAllLines(path);
		}
		else
		{
			regionNames = new string[0];
		}
	}

	public void ReloadLocksList()
	{
		string path = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + "Gates" + Path.DirectorySeparatorChar + "locks.txt");
		if (File.Exists(path))
		{
			karmaLocks = File.ReadAllLines(path);
		}
		else
		{
			karmaLocks = new string[0];
		}
	}

	public void ClearOutSaveStateFromMemory()
	{
		Custom.Log("~~~~~ Clear out save state");
		currentSaveState = null;
		starvedSaveState = null;
	}

	public void SyncLoadModState()
	{
		Custom.Log("!! Synced load state");
		ClearOutLoadModState();
		onLoadMMFState = ModManager.MMF;
		onLoadMSCState = ModManager.MSC;
		onLoadExpeditionState = ModManager.Expedition;
		onLoadJollyState = ModManager.JollyCoop;
		for (int i = 0; i < ModManager.InstalledMods.Count; i++)
		{
			onLoadEnabledModsState.Add(ModManager.InstalledMods[i].id, ModManager.InstalledMods[i].enabled);
		}
	}

	public void ClearOutLoadModState()
	{
		onLoadEnabledModsState.Clear();
	}

	public void BackUpSave(string appendName)
	{
		if (saveFileDataInMemory != null && !loadInProgress && saveFileDataInMemory.Contains("save"))
		{
			string value = saveFileDataInMemory.Get("save", "");
			saveFileDataInMemory.Set($"save_{appendName}", value, (!canSave) ? UserData.WriteMode.Deferred : UserData.WriteMode.Immediate);
		}
	}

	public void CheckConsoleBackupExists(Action<bool> callback)
	{
		Custom.LogWarning("CheckConsoleBackupExists was called, but this platform does not support it!");
		callback(obj: false);
	}

	private void OnCheckConsoleBackupExists(UserData.File file, UserData.Result result)
	{
		checkConsoleBackupExistsCallback(result == UserData.Result.Success);
		saveFileDataInMemory.OnBackupExistsCompleted -= OnCheckConsoleBackupExists;
	}

	public void RestoreConsoleBackup(Action<bool> callback)
	{
		Custom.LogWarning("RestoreConsoleBackup was called, but this platform does not support it!");
		callback(obj: false);
	}

	private void OnRestoreBackup(UserData.File file, UserData.Result result)
	{
		restoreBackupCallback(result == UserData.Result.Success);
		saveFileDataInMemory.OnRestoreBackupCompleted -= OnRestoreBackup;
	}

	public void CreateCopyOfSaves(bool userCreated)
	{
		string text = Application.persistentDataPath + Path.DirectorySeparatorChar + "backup";
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		if (!userCreated)
		{
			int num = 50;
			string[] directories = Directory.GetDirectories(text);
			int num2 = 0;
			string[] array = directories;
			for (int i = 0; i < array.Length; i++)
			{
				if (!Path.GetFileName(array[i]).Contains("_USR"))
				{
					num2++;
				}
			}
			if (num2 > num)
			{
				string text2 = null;
				long num3 = 0L;
				array = directories;
				foreach (string text3 in array)
				{
					string fileName = Path.GetFileName(text3);
					if (!fileName.Contains("_USR") && fileName.Contains("_"))
					{
						long result = 0L;
						if (long.TryParse(fileName.Substring(0, fileName.IndexOf("_")), NumberStyles.Any, NumberFormatInfo.InvariantInfo, out result) && (num3 == 0L || result < num3))
						{
							text2 = text3;
							num3 = result;
						}
					}
				}
				if (text2 != null)
				{
					try
					{
						array = Directory.GetFiles(text2);
						for (int i = 0; i < array.Length; i++)
						{
							File.Delete(array[i]);
						}
						Directory.Delete(text2);
					}
					catch (Exception ex)
					{
						Custom.LogWarning("Failed to delete old save file backup", text2, "::", ex.Message);
					}
				}
			}
		}
		double totalSeconds = (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
		string text4 = text + Path.DirectorySeparatorChar + (long)totalSeconds + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm");
		if (userCreated)
		{
			text4 += "_USR";
		}
		if (!Directory.Exists(text4))
		{
			Directory.CreateDirectory(text4);
		}
		CopySaveFile("sav", text4);
		CopySaveFile("sav2", text4);
		CopySaveFile("sav3", text4);
		CopySaveFile("expCore", text4);
		CopySaveFile("expCore1", text4);
		CopySaveFile("expCore2", text4);
		CopySaveFile("expCore3", text4);
		CopySaveFile("exp", text4);
		CopySaveFile("exp1", text4);
	}

	[Obsolete("Use parameterized function instead.")]
	public void CreateCopyOfSaves()
	{
		CreateCopyOfSaves(userCreated: false);
	}

	private void CopySaveFile(string sourceName, string destinationDirectory)
	{
		if (File.Exists(Application.persistentDataPath + Path.DirectorySeparatorChar + sourceName))
		{
			File.Copy(Application.persistentDataPath + Path.DirectorySeparatorChar + sourceName, destinationDirectory + Path.DirectorySeparatorChar + sourceName);
		}
	}

	public void BackupRegionStatePreservation()
	{
		if (MMF.cfgVanillaExploits.Value)
		{
			Custom.Log("Vanilla exploits allowed, skipping regionstate safety save!");
			return;
		}
		Custom.Log("BACKUP REGION STATE CATCH! Preventing double den spawns!");
		for (int i = 0; i < currentSaveState.regionStates.Length; i++)
		{
			if (currentSaveState.regionStates[i] != null && currentSaveState.regionLoadStrings[i] == null)
			{
				Custom.Log("Found empty region state!", i.ToString());
				currentSaveState.regionLoadStrings[i] = currentSaveState.regionStates[i].SaveToString();
				Custom.Log(currentSaveState.regionLoadStrings[i]);
			}
		}
	}

	public string[] GetProgLinesFromMemory()
	{
		if (saveFileDataInMemory != null && !loadInProgress && saveFileDataInMemory.Contains("save"))
		{
			string text = saveFileDataInMemory.Get("save", "");
			if (text.Length == 0)
			{
				Custom.LogWarning("Empty save file at" + ((saveFileDataInMemory != null) ? saveFileDataInMemory.filename : "null") + ". Returns empty prog lines");
				return new string[0];
			}
			string text2 = text.Substring(0, 32);
			text = text.Substring(32, text.Length - 32);
			if (Custom.Md5Sum(text) == text2)
			{
				Custom.Log("Checksum CORRECT!");
			}
			else
			{
				Custom.LogWarning("Checksum WRONG!");
				gameTinkeredWith = true;
			}
			return Regex.Split(text, "<progDivA>");
		}
		Custom.LogWarning("No existing save file at " + ((saveFileDataInMemory != null) ? saveFileDataInMemory.filename : "null") + ". Returns empty prog lines");
		return new string[0];
	}

	public void LoadProgression()
	{
		if (saveFileDataInMemory == null)
		{
			if (!progressionLoaded)
			{
				requestLoad = true;
			}
		}
		else
		{
			if (loadInProgress)
			{
				return;
			}
			if (saveFileDataInMemory.executeReadError != null)
			{
				Custom.LogWarning("!! FAILED TO LOAD SAVE FILE, AN ERROR WAS THROWN WHILE READING THE DATA:", saveFileDataInMemory.executeReadError);
				SyncLoadModState();
				progressionLoadedResult = ProgressionLoadResult.ERROR_READ_FAILED;
				progressionLoaded = true;
				Platform.NotifyUserDataReadCompleted(this);
				return;
			}
			if (!saveFileDataInMemory.Contains("save"))
			{
				Custom.LogWarning("!! FAILED TO LOAD SAVE FILE, THE SAVE DATA WAS MISSING");
				SyncLoadModState();
				if (!progressionLoaded)
				{
					progressionLoadedResult = ProgressionLoadResult.ERROR_SAVE_DATA_MISSING;
				}
				progressionLoaded = true;
				Platform.NotifyUserDataReadCompleted(this);
				return;
			}
			string[] progLinesFromMemory = GetProgLinesFromMemory();
			if (progLinesFromMemory.Length == 0)
			{
				return;
			}
			for (int i = 0; i < progLinesFromMemory.Length; i++)
			{
				string[] array = Regex.Split(progLinesFromMemory[i], "<progDivB>");
				string text = array[0];
				if (text != null && text == "MISCPROG")
				{
					miscProgressionData.FromString(array[1]);
				}
			}
			foreach (KeyValuePair<string, SpeedRunTimer.CampaignTimeTracker> campaignTimer in miscProgressionData.campaignTimers)
			{
				campaignTimer.Value.ConvertUndeterminedToLostTime();
			}
		}
	}

	public void LoadProgressionFromLegacyFile(string saveFilePath)
	{
		string text = File.ReadAllText(saveFilePath);
		if (text.Length <= 32)
		{
			return;
		}
		text.Substring(0, 32);
		text = text.Substring(32, text.Length - 32);
		string[] array = Regex.Split(text, "<progDivA>");
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = Regex.Split(array[i], "<progDivB>");
			string text2 = array2[0];
			if (text2 != null && text2 == "MISCPROG")
			{
				miscProgressionData.FromString(array2[1]);
			}
		}
	}

	public bool SaveProgressionAndDeathPersistentDataOfCurrentState(bool saveAsDeath, bool saveAsQuit)
	{
		bool num = SaveProgression(saveMaps: true, saveMiscProg: true);
		if (num)
		{
			SaveDeathPersistentDataOfCurrentState(saveAsDeath, saveAsQuit);
		}
		return num;
	}

	public bool SaveProgression(bool saveMaps, bool saveMiscProg)
	{
		return SaveToDisk(saveCurrentState: false, saveMaps, saveMiscProg);
	}

	public bool SaveWorldStateAndProgression(bool malnourished)
	{
		bool flag = false;
		if (malnourished)
		{
			Custom.LogImportant("STARVED - NOT SAVING STATE TO DISK");
			flag = SaveProgressionAndDeathPersistentDataOfCurrentState(saveAsDeath: true, saveAsQuit: false);
			if (ModManager.MMF)
			{
				if (currentSaveState != null)
				{
					for (int i = 0; i < currentSaveState.objectTrackers.Count; i++)
					{
						currentSaveState.objectTrackers[i].UninitializeTracker();
					}
				}
				BackupRegionStatePreservation();
			}
			starvedSaveState = currentSaveState;
			currentSaveState = null;
		}
		else
		{
			SpeedRunTimer.GetCampaignTimeTracker(PlayingAsSlugcat)?.ConvertUndeterminedToCompletedTime();
			flag = SaveToDisk(saveCurrentState: true, saveMaps: true, saveMiscProg: true);
		}
		return flag;
	}

	private bool SaveToDisk(bool saveCurrentState, bool saveMaps, bool saveMiscProg)
	{
		Custom.Log("!! Saving data");
		bool flag = ModManager.MMF == onLoadMMFState && ModManager.MSC == onLoadMSCState && ModManager.Expedition == onLoadExpeditionState && ModManager.JollyCoop == onLoadJollyState;
		for (int i = 0; i < ModManager.InstalledMods.Count; i++)
		{
			if (!onLoadEnabledModsState.ContainsKey(ModManager.InstalledMods[i].id) || onLoadEnabledModsState[ModManager.InstalledMods[i].id] != ModManager.InstalledMods[i].enabled)
			{
				flag = false;
				break;
			}
		}
		foreach (string enabledMod in rainWorld.options.enabledMods)
		{
			if (!onLoadEnabledModsState.ContainsKey(enabledMod) || !onLoadEnabledModsState[enabledMod])
			{
				flag = false;
				break;
			}
		}
		foreach (KeyValuePair<string, bool> item in onLoadEnabledModsState)
		{
			bool flag2 = false;
			for (int j = 0; j < ModManager.InstalledMods.Count; j++)
			{
				if (ModManager.InstalledMods[j].id == item.Key && ModManager.InstalledMods[j].enabled == item.Value)
				{
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				flag = false;
				break;
			}
		}
		if (!flag)
		{
			Custom.LogWarning("!! NOT SAVING BECAUSE SAVE DATA IN MEMORY IS OUT OF SYNC AND INCOMPATIBLE WITH CURRENT GAME STATE!");
			Custom.LogWarning($"MSC: {ModManager.MSC} :: {onLoadMSCState}");
			Custom.LogWarning($"MMF: {ModManager.MMF} :: {onLoadMMFState}");
			Custom.LogWarning($"Jolly: {ModManager.JollyCoop} :: {onLoadJollyState}");
			Custom.LogWarning($"Expedition: {ModManager.Expedition} :: {onLoadExpeditionState}");
			Custom.LogWarning("Current mods: ");
			for (int k = 0; k < ModManager.InstalledMods.Count; k++)
			{
				Custom.LogWarning(ModManager.InstalledMods[k].id, "::", ModManager.InstalledMods[k].enabled.ToString());
			}
			Custom.LogWarning("In-memory mods: ");
			foreach (KeyValuePair<string, bool> item2 in onLoadEnabledModsState)
			{
				Custom.LogWarning($"{item2.Key} :: {item2.Value}");
			}
			return false;
		}
		if (!saveCurrentState && !saveMaps && !saveMiscProg)
		{
			Custom.LogWarning("SaveToDisk without anything to save.");
			return true;
		}
		Custom.Log("SAVE PROGRESSION saveCurrentState:", saveCurrentState.ToString(), "saveMaps:", saveMaps.ToString(), "saveMiscProg:", saveMiscProg.ToString());
		bool flag3 = false;
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		bool flag4 = false;
		if (saveMaps)
		{
			miscProgressionData.SaveDiscoveredShelters(ref tempSheltersDiscovered);
		}
		tempSheltersDiscovered.Clear();
		string[] progLinesFromMemory = GetProgLinesFromMemory();
		string text = "";
		for (int l = 0; l < progLinesFromMemory.Length; l++)
		{
			bool flag5 = false;
			string[] array = Regex.Split(progLinesFromMemory[l], "<progDivB>");
			if (array[0] == "SAVE STATE")
			{
				if (saveCurrentState && currentSaveState != null && BackwardsCompatibilityRemix.ParseSaveNumber(array[1]) == currentSaveState.saveStateNumber)
				{
					text = text + "SAVE STATE<progDivB>" + currentSaveState.SaveToString();
					Custom.Log($"successfully saved state {currentSaveState.saveStateNumber} to disc");
					flag3 = true;
				}
				else
				{
					text += progLinesFromMemory[l];
				}
				flag5 = true;
			}
			else if (array[0] == "MAP")
			{
				if (ModManager.ModdedRegionsEnabled || !saveMaps || !mapDiscoveryTextures.ContainsKey(array[1]) || mapDiscoveryTextures[array[1]] == null)
				{
					text += progLinesFromMemory[l];
				}
				else
				{
					text = text + "MAP<progDivB>" + array[1] + "<progDivB>" + Convert.ToBase64String(mapDiscoveryTextures[array[1]].EncodeToPNG());
					if (mapLastUpdatedTime.ContainsKey(array[1]))
					{
						text = text + "<progDivA>MAPUPDATE<progDivB>" + array[1] + string.Format(CultureInfo.InvariantCulture, "<progDivB>{0}", mapLastUpdatedTime[array[1]]);
					}
				}
				flag5 = true;
				list.Add(array[1]);
			}
			else if (array[0].Length > 4 && array[0].Substring(0, 4) == "MAP_")
			{
				if (array[0].Substring(4, array[0].Length - 4) != PlayingAsSlugcat.value)
				{
					text += progLinesFromMemory[l];
					flag5 = true;
				}
				else
				{
					if (!ModManager.ModdedRegionsEnabled || !saveMaps || !mapDiscoveryTextures.ContainsKey(array[1]) || mapDiscoveryTextures[array[1]] == null)
					{
						text += progLinesFromMemory[l];
					}
					else
					{
						text = text + "MAP_" + PlayingAsSlugcat.value + "<progDivB>" + array[1] + "<progDivB>" + Convert.ToBase64String(mapDiscoveryTextures[array[1]].EncodeToPNG());
						if (mapLastUpdatedTime.ContainsKey(array[1]))
						{
							text = text + "<progDivA>MAPUPDATE_" + PlayingAsSlugcat.value + "<progDivB>" + array[1] + string.Format(CultureInfo.InvariantCulture, "<progDivB>{0}", mapLastUpdatedTime[array[1]]);
						}
					}
					flag5 = true;
					list2.Add(array[1]);
				}
			}
			else if (array[0] == "MISCPROG")
			{
				text = (saveMiscProg ? (text + "MISCPROG<progDivB>" + miscProgressionData.ToString()) : (text + progLinesFromMemory[l]));
				flag5 = true;
				flag4 = true;
			}
			if (flag5)
			{
				text += "<progDivA>";
			}
		}
		if (saveCurrentState && !flag3 && currentSaveState != null)
		{
			text = text + "SAVE STATE<progDivB>" + currentSaveState.SaveToString() + "<progDivA>";
			Custom.Log($"successfully saved state {currentSaveState.saveStateNumber} to disc (fresh)");
		}
		if (saveMaps)
		{
			foreach (KeyValuePair<string, Texture2D> mapDiscoveryTexture in mapDiscoveryTextures)
			{
				if (!ModManager.ModdedRegionsEnabled && !list.Contains(mapDiscoveryTexture.Key) && mapDiscoveryTexture.Value != null)
				{
					text = text + "MAP<progDivB>" + mapDiscoveryTexture.Key + "<progDivB>" + Convert.ToBase64String(mapDiscoveryTexture.Value.EncodeToPNG()) + "<progDivA>";
				}
				else if (ModManager.ModdedRegionsEnabled && !list2.Contains(mapDiscoveryTexture.Key) && mapDiscoveryTexture.Value != null)
				{
					text = text + "MAP_" + PlayingAsSlugcat.value + "<progDivB>" + mapDiscoveryTexture.Key + "<progDivB>" + Convert.ToBase64String(mapDiscoveryTexture.Value.EncodeToPNG()) + "<progDivA>";
				}
			}
			foreach (KeyValuePair<string, long> item3 in mapLastUpdatedTime)
			{
				if (!ModManager.ModdedRegionsEnabled && !list.Contains(item3.Key))
				{
					text = text + "MAPUPDATE<progDivB>" + item3.Key + string.Format(CultureInfo.InvariantCulture, "<progDivB>{0}<progDivA>", item3.Value);
				}
				else if (ModManager.ModdedRegionsEnabled && !list2.Contains(item3.Key))
				{
					text = text + "MAPUPDATE_" + PlayingAsSlugcat.value + "<progDivB>" + item3.Key + string.Format(CultureInfo.InvariantCulture, "<progDivB>{0}<progDivA>", item3.Value);
				}
			}
		}
		if (saveMiscProg && !flag4)
		{
			text = text + "MISCPROG<progDivB>" + miscProgressionData.ToString() + "<progDivA>";
		}
		if (saveFileDataInMemory != null && !loadInProgress)
		{
			saveFileDataInMemory.Set("save", Custom.Md5Sum(text) + text, (!canSave) ? UserData.WriteMode.Deferred : UserData.WriteMode.Immediate);
			Custom.Log("Player progression Filestring is:", Custom.Md5Sum(text) + text);
			Custom.Log("Player progression cansave is:", canSave ? "Immediate" : "Deferred");
		}
		return true;
	}

	public void WipeAll()
	{
		Custom.LogImportant("WIPING ALL EXCEPT MISCPROG");
		BackUpSave("_WipeBackup");
		if (progressionLoaded && progressionLoadedResult == ProgressionLoadResult.ERROR_CORRUPTED_FILE)
		{
			Custom.LogWarning("!! Progression was corrupted, so WipeAll is deleting the corrupted save file completely.");
			DeleteSaveFile();
			return;
		}
		miscProgressionData.discoveredShelters.Clear();
		miscProgressionData.menuRegion = null;
		tempSheltersDiscovered.Clear();
		miscProgressionData.ConditionalShelterDiscovery.Clear();
		string text = "MISCPROG<progDivB>" + miscProgressionData.ToString() + "<progDivA>";
		if (saveFileDataInMemory != null && !loadInProgress)
		{
			saveFileDataInMemory.Set("save", Custom.Md5Sum(text) + text, (!canSave) ? UserData.WriteMode.Deferred : UserData.WriteMode.Immediate);
		}
		saveFileDataInMemory.OnReadCompleted += SaveFile_OnReadCompleted;
		saveFileDataInMemory.Read();
		string path = Application.persistentDataPath + Path.DirectorySeparatorChar + "SJ_" + rainWorld.options.saveSlot;
		if (Directory.Exists(path))
		{
			Directory.Delete(path, recursive: true);
		}
	}

	public void WipeSaveState(SlugcatStats.Name saveStateNumber)
	{
		Custom.LogImportant($"WIPING SAVE STATE {saveStateNumber}");
		tempSheltersDiscovered.Clear();
		if (miscProgressionData.ConditionalShelterDiscovery.Count > 0)
		{
			foreach (MiscProgressionData.ConditionalShelterData item in miscProgressionData.ConditionalShelterDiscovery)
			{
				item.removeSlugcatIndex(saveStateNumber);
			}
		}
		miscProgressionData.CleanupConditionalShelters();
		if (saveStateNumber == SlugcatStats.Name.White)
		{
			miscProgressionData.survivorEndingID = 0;
		}
		if (saveStateNumber == SlugcatStats.Name.Yellow)
		{
			miscProgressionData.monkEndingID = 0;
		}
		if (ModManager.MSC && saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
		{
			miscProgressionData.artificerEndingID = 0;
		}
		string[] progLinesFromMemory = GetProgLinesFromMemory();
		string text = "";
		for (int i = 0; i < progLinesFromMemory.Length; i++)
		{
			string[] array = Regex.Split(progLinesFromMemory[i], "<progDivB>");
			if (array[0] == "SAVE STATE" && BackwardsCompatibilityRemix.ParseSaveNumber(array[1]) == saveStateNumber)
			{
				Custom.LogWarning($"not saving save state {BackwardsCompatibilityRemix.ParseSaveNumber(array[1])}");
			}
			else
			{
				text = text + progLinesFromMemory[i] + "<progDivA>";
			}
		}
		if (saveFileDataInMemory != null && !loadInProgress)
		{
			saveFileDataInMemory.Set("save", Custom.Md5Sum(text) + text, (!canSave) ? UserData.WriteMode.Deferred : UserData.WriteMode.Immediate);
		}
		Revert();
		if (ModManager.MSC && saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Saint)
		{
			string path = Application.persistentDataPath + Path.DirectorySeparatorChar + "SJ_" + rainWorld.options.saveSlot;
			if (Directory.Exists(path))
			{
				Directory.Delete(path, recursive: true);
			}
		}
		if (ModManager.MMF && !MMF.cfgVanillaExploits.Value)
		{
			starvedSaveState = null;
		}
		SpeedRunTimer.GetCampaignTimeTracker(saveStateNumber)?.WipeTimes();
	}

	public void Revert()
	{
		currentSaveState = null;
		mapDiscoveryTextures.Clear();
		mapLastUpdatedTime.Clear();
		tempSheltersDiscovered.Clear();
	}

	public void LoadMapTexture(string regionName)
	{
		if (!LOAD || saveFileDataInMemory == null || loadInProgress || !saveFileDataInMemory.Contains("save"))
		{
			return;
		}
		bool flag = mapLastUpdatedTime.ContainsKey(regionName) || !LOAD || saveFileDataInMemory == null || loadInProgress || !saveFileDataInMemory.Contains("save");
		bool flag2 = mapDiscoveryTextures.ContainsKey(regionName) && mapDiscoveryTextures[regionName] != null;
		if (flag2 && flag)
		{
			return;
		}
		Custom.Log("Map load request for:", regionName);
		string[] progLinesFromMemory = GetProgLinesFromMemory();
		if (ModManager.ModdedRegionsEnabled)
		{
			for (int i = 0; i < progLinesFromMemory.Length; i++)
			{
				string[] array = Regex.Split(progLinesFromMemory[i], "<progDivB>");
				if (!flag2 && array[0] == "MAP_" + PlayingAsSlugcat.value && regionName == array[1])
				{
					LoadByteStringIntoMapTexture(regionName, array[2]);
					flag2 = true;
				}
				if (!flag && array[0] == "MAPUPDATE_" + PlayingAsSlugcat.value && regionName == array[1])
				{
					mapLastUpdatedTime[regionName] = long.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
					flag = true;
				}
				if (flag2 && flag)
				{
					return;
				}
			}
		}
		for (int j = 0; j < progLinesFromMemory.Length; j++)
		{
			string[] array2 = Regex.Split(progLinesFromMemory[j], "<progDivB>");
			if (!flag2 && array2[0] == "MAP" && regionName == array2[1])
			{
				LoadByteStringIntoMapTexture(regionName, array2[2]);
				flag2 = true;
			}
			if (!flag && array2[0] == "MAPUPDATE" && regionName == array2[1])
			{
				mapLastUpdatedTime[regionName] = long.Parse(array2[2], NumberStyles.Any, CultureInfo.InvariantCulture);
				flag = true;
			}
			if (flag2 && flag)
			{
				break;
			}
		}
	}

	private void LoadByteStringIntoMapTexture(string regionName, string byteString)
	{
		Custom.Log("Loading map bytes:", regionName);
		byte[] data = Convert.FromBase64String(byteString);
		mapDiscoveryTextures[regionName] = new Texture2D(2, 2);
		mapDiscoveryTextures[regionName].LoadImage(data);
		if (mapDiscoveryTextures[regionName].width < 10)
		{
			Custom.LogWarning("LOADED MAP TEXTURE CORRUPTED:", mapDiscoveryTextures[regionName].width.ToString(), mapDiscoveryTextures[regionName].height.ToString());
			mapDiscoveryTextures[regionName] = null;
		}
	}

	public void TempDiscoverShelter(string shelterName)
	{
		for (int i = 0; i < tempSheltersDiscovered.Count; i++)
		{
			if (tempSheltersDiscovered[i] == shelterName)
			{
				return;
			}
		}
		Custom.Log("shelter temp discovered:", shelterName);
		tempSheltersDiscovered.Add(shelterName);
	}

	public SaveState GetOrInitiateSaveState(SlugcatStats.Name saveStateNumber, RainWorldGame game, ProcessManager.MenuSetup setup, bool saveAsDeathOrQuit)
	{
		if (currentSaveState == null && starvedSaveState != null && (!ModManager.MSC || game.manager.artificerDreamNumber == -1))
		{
			Custom.Log("LOADING STARVED STATE");
			currentSaveState = starvedSaveState;
			currentSaveState.deathPersistentSaveData.winState.ResetLastShownValues();
			starvedSaveState = null;
		}
		if (currentSaveState != null && currentSaveState.saveStateNumber == saveStateNumber)
		{
			if (saveAsDeathOrQuit)
			{
				SaveDeathPersistentDataOfCurrentState(saveAsIfPlayerDied: true, saveAsIfPlayerQuit: true);
			}
			return currentSaveState;
		}
		currentSaveState = new SaveState(saveStateNumber, this);
		if (saveFileDataInMemory == null || loadInProgress || !saveFileDataInMemory.Contains("save") || !setup.LoadInitCondition)
		{
			currentSaveState.LoadGame("", game);
		}
		else
		{
			SaveState saveState = LoadGameState(null, game, saveAsDeathOrQuit);
			if (saveState != null)
			{
				return saveState;
			}
			currentSaveState.LoadGame("", game);
		}
		if (saveAsDeathOrQuit)
		{
			SaveDeathPersistentDataOfCurrentState(saveAsIfPlayerDied: true, saveAsIfPlayerQuit: true);
		}
		return currentSaveState;
	}

	public SaveState LoadGameState(string saveFilePath, RainWorldGame game, bool saveAsDeathOrQuit)
	{
		string[] array;
		if (saveFilePath == null)
		{
			array = GetProgLinesFromMemory();
		}
		else
		{
			string text = File.ReadAllText(saveFilePath);
			array = ((text.Length <= 32) ? new string[0] : Regex.Split(text.Substring(32), "<progDivA>"));
		}
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = Regex.Split(array[i], "<progDivB>");
			if (array2.Length == 2 && array2[0] == "SAVE STATE" && BackwardsCompatibilityRemix.ParseSaveNumber(array2[1]) == currentSaveState.saveStateNumber)
			{
				currentSaveState.LoadGame(array2[1], game);
				if (saveAsDeathOrQuit)
				{
					SaveDeathPersistentDataOfCurrentState(saveAsIfPlayerDied: true, saveAsIfPlayerQuit: true);
				}
				return currentSaveState;
			}
		}
		return null;
	}

	public void SaveDeathPersistentDataOfCurrentState(bool saveAsIfPlayerDied, bool saveAsIfPlayerQuit)
	{
		if (currentSaveState == null)
		{
			Custom.LogWarning("Couldn't save death persistent data because no current save state");
			return;
		}
		if (ModManager.MMF)
		{
			bool flag = false;
			if (rainWorld.processManager != null && rainWorld.processManager.currentMainLoop != null && rainWorld.processManager.currentMainLoop is GhostEncounterScreen)
			{
				Custom.LogImportant("SAVE GHOST, IGNORING STARVATION FOR SAVE");
				flag = true;
			}
			if (currentSaveState.malnourished && !flag)
			{
				if (!MMF.cfgVanillaExploits.Value)
				{
					Custom.Log("MALNOURISHED! Canceling 30 second safety timer.");
					saveAsIfPlayerDied = true;
				}
				else
				{
					Custom.Log("MALNOURISHED! But vanilla exploits enabled, so karmacache!");
				}
			}
		}
		Custom.Log($"save deathPersistent data {currentSaveState.deathPersistentSaveData.karma} sub karma: {saveAsIfPlayerDied} (quit:{saveAsIfPlayerQuit})");
		string text = currentSaveState.deathPersistentSaveData.SaveToString(saveAsIfPlayerDied, saveAsIfPlayerQuit);
		if (text == "")
		{
			Custom.LogWarning("NO DATA TO WRITE");
			return;
		}
		string[] progLinesFromMemory = GetProgLinesFromMemory();
		string text2 = "";
		for (int i = 0; i < progLinesFromMemory.Length; i++)
		{
			Custom.Log("Proglines i =", i.ToString());
			bool flag2 = false;
			string[] array = Regex.Split(progLinesFromMemory[i], "<progDivB>");
			if (array[0] == "SAVE STATE" && BackwardsCompatibilityRemix.ParseSaveNumber(array[1]) == currentSaveState.saveStateNumber)
			{
				Custom.Log("PlayProg Running section 1!");
				string text3 = "";
				string[] array2 = Regex.Split(progLinesFromMemory[i], "<svA>");
				for (int j = 0; j < array2.Length; j++)
				{
					text3 = ((!(Regex.Split(array2[j], "<svB>")[0] == "DEATHPERSISTENTSAVEDATA")) ? (text3 + array2[j] + "<svA>") : (text3 + "DEATHPERSISTENTSAVEDATA<svB>" + text + "<svA>"));
				}
				flag2 = true;
				text2 += text3;
			}
			else
			{
				Custom.Log("PlayProg Running section 2!");
				flag2 = progLinesFromMemory[i] != "";
				text2 += progLinesFromMemory[i];
			}
			if (flag2)
			{
				text2 += "<progDivA>";
				Custom.Log("PlayProg Running section 3!");
			}
		}
		if (saveFileDataInMemory != null && !loadInProgress)
		{
			saveFileDataInMemory.Set("save", Custom.Md5Sum(text2) + text2, (!canSave) ? UserData.WriteMode.Deferred : UserData.WriteMode.Immediate);
			Custom.LogImportant("Playerprog cansave is:", canSave ? "Immediate" : "Deferred");
		}
	}

	public bool IsThereASavedGame(SlugcatStats.Name saveStateNumber)
	{
		if (currentSaveState != null && currentSaveState.saveStateNumber == saveStateNumber)
		{
			return true;
		}
		if (saveFileDataInMemory == null || loadInProgress || !saveFileDataInMemory.Contains("save"))
		{
			return false;
		}
		string[] progLinesFromMemory = GetProgLinesFromMemory();
		for (int i = 0; i < progLinesFromMemory.Length; i++)
		{
			string[] array = Regex.Split(progLinesFromMemory[i], "<progDivB>");
			if (array.Length == 2 && array[0] == "SAVE STATE" && BackwardsCompatibilityRemix.ParseSaveNumber(array[1]) == saveStateNumber)
			{
				return true;
			}
		}
		return false;
	}

	public string ShelterOfSaveGame(SlugcatStats.Name saveStateNumber)
	{
		if (saveStateNumber == null)
		{
			return "SU_S01";
		}
		if (currentSaveState != null && currentSaveState.saveStateNumber == saveStateNumber)
		{
			return currentSaveState.GetSaveStateDenToUse();
		}
		if (saveFileDataInMemory == null || loadInProgress || !saveFileDataInMemory.Contains("save"))
		{
			return SaveState.GetFinalFallbackShelter(saveStateNumber);
		}
		string[] progLinesFromMemory = GetProgLinesFromMemory();
		for (int i = 0; i < progLinesFromMemory.Length; i++)
		{
			string[] array = Regex.Split(progLinesFromMemory[i], "<progDivB>");
			if (array.Length == 2 && array[0] == "SAVE STATE" && BackwardsCompatibilityRemix.ParseSaveNumber(array[1]) == saveStateNumber)
			{
				List<SaveStateMiner.Target> targets = new List<SaveStateMiner.Target>
				{
					new SaveStateMiner.Target(">DENPOS", "<svB>", "<svA>", 20)
				};
				List<SaveStateMiner.Result> list = SaveStateMiner.Mine(rainWorld, array[1], targets);
				if (list.Count > 0 && list[0].data != null && RainWorld.roomNameToIndex.ContainsKey(list[0].data))
				{
					return list[0].data;
				}
				targets = new List<SaveStateMiner.Target>
				{
					new SaveStateMiner.Target(">LASTVDENPOS", "<svB>", "<svA>", 20)
				};
				list = SaveStateMiner.Mine(rainWorld, array[1], targets);
				if (list.Count > 0 && list[0].data != null && RainWorld.roomNameToIndex.ContainsKey(list[0].data))
				{
					return list[0].data;
				}
			}
		}
		return SaveState.GetFinalFallbackShelter(saveStateNumber);
	}

	public void LookForOldVersionSaveFile()
	{
		Custom.Log("LOOKING FOR OLD VERSION SAVE FILE");
		try
		{
			string[] progLinesFromMemory = GetProgLinesFromMemory();
			if (progLinesFromMemory.Length == 0)
			{
				return;
			}
			string text = "";
			for (int i = 0; i < progLinesFromMemory.Length; i++)
			{
				string[] array = Regex.Split(progLinesFromMemory[i], "<progDivB>");
				if (array.Length == 2 && array[0] == "SAVE STATE" && BackwardsCompatibilityRemix.ParseSaveNumber(array[1]) == SlugcatStats.Name.White)
				{
					text = array[1];
					break;
				}
			}
			if (text.Length >= 10)
			{
				List<SaveStateMiner.Result> list = SaveStateMiner.Mine(rainWorld, text, new List<SaveStateMiner.Target>
				{
					new SaveStateMiner.Target(">WORLDVERSION", "<svB>", "<svA>", 20)
				});
				if (list.Count >= 1 && list[0].data == "0")
				{
					Custom.Log("UNLOCK RED");
					miscProgressionData.redUnlocked = true;
				}
			}
		}
		catch
		{
		}
	}

	public void Update()
	{
		if (!requestLoad || !rainWorld.OptionsReady)
		{
			return;
		}
		if (Profiles.ActiveProfiles.Count > 0)
		{
			UserData.FileDefinition fileDefinition = new UserData.FileDefinition((ModManager.Expedition && rainWorld.options.saveSlot < 0) ? SAVE_FILE_EXP_DEFINITION : SAVE_FILE_DEFINITION);
			fileDefinition.fileName = rainWorld.options.GetSaveFileName_SavOrExp();
			if (rainWorld.options.saveSlot >= 0 || !ModManager.Expedition)
			{
				fileDefinition.ps4Definition.title = string.Format(rainWorld.inGameTranslator.Translate("ps4_save_progress_title"), rainWorld.options.saveSlot + 1);
				fileDefinition.ps4Definition.detail = string.Format(rainWorld.inGameTranslator.Translate("ps4_save_progress_description"), rainWorld.options.saveSlot + 1);
			}
			else
			{
				fileDefinition.ps4Definition.title = rainWorld.inGameTranslator.Translate("ps4_expedition_slot_title");
				fileDefinition.ps4Definition.detail = rainWorld.inGameTranslator.Translate("ps4_expedition_slot_description");
			}
			if (!UserData.IsFileUnmounting(Profiles.ActiveProfiles[0], null, fileDefinition))
			{
				requestLoad = false;
				loadInProgress = true;
				UserData.OnFileMounted += UserData_OnFileMounted;
				UserData.Mount(Profiles.ActiveProfiles[0], null, fileDefinition, overrideBaseDir);
			}
		}
		else
		{
			Custom.LogWarning("!! FAILED TO LOAD SAVE FILE, THE PLAYER HANDLER/PROFILE WAS NOT DEFINED!");
			SyncLoadModState();
			if (!progressionLoaded)
			{
				progressionLoadedResult = ProgressionLoadResult.ERROR_NO_PROFILE;
			}
			progressionLoaded = true;
			requestLoad = false;
			Platform.NotifyUserDataReadCompleted(this);
		}
	}

	public void Destroy(int previousSaveSlot = 0)
	{
		if (ModManager.MSC)
		{
			if (previousSaveSlot < 0)
			{
				MiscProgressionData.transferDecipheredPearls = miscProgressionData.decipheredPearls;
				MiscProgressionData.transferDecipheredFuturePearls = miscProgressionData.decipheredFuturePearls;
				MiscProgressionData.transferDecipheredPebblesPearls = miscProgressionData.decipheredPebblesPearls;
				MiscProgressionData.transferDecipheredDMPearls = miscProgressionData.decipheredDMPearls;
				MiscProgressionData.transferDiscoveredBroadcasts = miscProgressionData.discoveredBroadcasts;
			}
			else
			{
				MiscProgressionData.transferDecipheredPearls.Clear();
				MiscProgressionData.transferDecipheredFuturePearls.Clear();
				MiscProgressionData.transferDecipheredPebblesPearls.Clear();
				MiscProgressionData.transferDecipheredDMPearls.Clear();
				MiscProgressionData.transferDiscoveredBroadcasts.Clear();
			}
		}
		Platform.OnRequestUserDataRead -= Platform_OnRequestUserDataRead;
		if (saveFileDataInMemory != null)
		{
			saveFileDataInMemory.Unmount();
			saveFileDataInMemory = null;
		}
		if (!progressionLoaded)
		{
			Platform.NotifyUserDataReadCompleted(this);
		}
		loadInProgress = false;
		progressionLoadedResult = ProgressionLoadResult.PENDING_LOAD;
		progressionLoaded = false;
		requestLoad = false;
	}

	private void LoadOldPS4ProgressionSave()
	{
		string oldPS4ProgressionSave = rainWorld.options.GetOldPS4ProgressionSave();
		if (!string.IsNullOrEmpty(oldPS4ProgressionSave))
		{
			saveFileDataInMemory.Set("save", oldPS4ProgressionSave, UserData.WriteMode.Deferred);
			string text = oldPS4ProgressionSave.Substring(0, 32);
			oldPS4ProgressionSave = oldPS4ProgressionSave.Substring(32, oldPS4ProgressionSave.Length - 32);
			if (Custom.Md5Sum(oldPS4ProgressionSave) == text)
			{
				Custom.Log("Checksum CORRECT!");
			}
			else
			{
				Custom.LogWarning("Checksum WRONG!");
				gameTinkeredWith = true;
			}
			string[] array = Regex.Split(oldPS4ProgressionSave, "<progDivA>");
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = Regex.Split(array[i], "<progDivB>");
				string text2 = array2[0];
				if (text2 != null && text2 == "MISCPROG")
				{
					miscProgressionData.FromString(array2[1]);
				}
			}
			if (!miscProgressionData.lookedForOldVersionSaveFile)
			{
				miscProgressionData.lookedForOldVersionSaveFile = true;
				LookForOldVersionSaveFile();
			}
		}
		SyncLoadModState();
		saveFileDataInMemory.OnWriteCompleted += SaveFile_OnWriteCompleted_NewFile;
		saveFileDataInMemory.Write();
	}

	private void Platform_OnRequestUserDataRead(List<object> pendingUserDataReads)
	{
		if (!progressionLoaded)
		{
			pendingUserDataReads.Add(this);
		}
	}

	private void UserData_OnFileMounted(UserData.File file, UserData.Result result)
	{
		if (!(file.filename == rainWorld.options.GetSaveFileName_SavOrExp()))
		{
			return;
		}
		UserData.OnFileMounted -= UserData_OnFileMounted;
		if (result.IsSuccess() && loadInProgress)
		{
			saveFileDataInMemory = file;
			saveFileDataInMemory.OnReadCompleted += SaveFile_OnReadCompleted;
			saveFileDataInMemory.Read();
			return;
		}
		Custom.LogWarning($"!! SKIPPING SAVE FILE MOUNT, RESULT:{result}, loadInProgress:{loadInProgress}");
		loadInProgress = false;
		SyncLoadModState();
		if (!progressionLoaded)
		{
			progressionLoadedResult = ProgressionLoadResult.ERROR_MOUNT_FAILED;
		}
		progressionLoaded = true;
		Platform.NotifyUserDataReadCompleted(this);
	}

	private void SaveFile_OnReadCompleted(UserData.File file, UserData.Result result)
	{
		file.OnReadCompleted -= SaveFile_OnReadCompleted;
		if (!loadInProgress)
		{
			return;
		}
		if (result.IsSuccess())
		{
			if (result.Contains(UserData.Result.FileNotFound))
			{
				SyncLoadModState();
				saveFileDataInMemory.OnWriteCompleted += SaveFile_OnWriteCompleted_NewFile;
				saveFileDataInMemory.Write();
				return;
			}
			loadInProgress = false;
			LoadProgression();
			if (!miscProgressionData.lookedForOldVersionSaveFile)
			{
				miscProgressionData.lookedForOldVersionSaveFile = true;
				LookForOldVersionSaveFile();
			}
			SyncLoadModState();
			if (!progressionLoaded)
			{
				progressionLoadedResult = ProgressionLoadResult.SUCCESS_LOAD_EXISTING_FILE;
			}
			progressionLoaded = true;
			Platform.NotifyUserDataReadCompleted(this);
			if (saveAfterLoad)
			{
				SaveProgression(saveMaps: false, saveMiscProg: true);
				saveAfterLoad = false;
			}
		}
		else if (result.Contains(UserData.Result.CorruptData))
		{
			loadInProgress = false;
			SyncLoadModState();
			if (!progressionLoaded)
			{
				progressionLoadedResult = ProgressionLoadResult.ERROR_CORRUPTED_FILE;
			}
			progressionLoaded = true;
		}
		else if (result.Contains(UserData.Result.FileNotFound))
		{
			SyncLoadModState();
			saveFileDataInMemory.OnWriteCompleted += SaveFile_OnWriteCompleted_NewFile;
			saveFileDataInMemory.Write();
		}
		else
		{
			Custom.LogWarning($"!! FAILED TO LOAD SAVE FILE! THE FILE EXISTS, BUT READ RESULT WAS {result}");
			loadInProgress = false;
			SyncLoadModState();
			if (!progressionLoaded)
			{
				progressionLoadedResult = ProgressionLoadResult.ERROR_READ_FAILED;
			}
			progressionLoaded = true;
			Platform.NotifyUserDataReadCompleted(this);
		}
	}

	public void DeleteSaveFile()
	{
		loadInProgress = true;
		progressionLoaded = false;
		saveFileDataInMemory.OnDeleteCompleted += SaveFile_OnDeleteCompleted;
		saveFileDataInMemory.Delete();
	}

	private void SaveFile_OnDeleteCompleted(UserData.File file, UserData.Result result)
	{
		saveFileDataInMemory.OnDeleteCompleted -= SaveFile_OnDeleteCompleted;
		if (result.IsSuccess())
		{
			SyncLoadModState();
			saveFileDataInMemory.OnWriteCompleted += SaveFile_OnWriteCompleted_NewFile;
			saveFileDataInMemory.Write();
			return;
		}
		saveFileDataInMemory.Unmount();
		saveFileDataInMemory = null;
		loadInProgress = false;
		Custom.LogWarning($"!! FAILED TO DELETE SAVE FILE. RESULT: {result}");
		progressionLoaded = true;
		Platform.NotifyUserDataReadCompleted(this);
	}

	public void RecreateSaveFile()
	{
		loadInProgress = true;
		progressionLoaded = false;
		SaveWorldStateAndProgression(malnourished: false);
		restoreDataToTransfer = saveFileDataInMemory.GetCopyOfData();
		restoreRawDataToTransfer = saveFileDataInMemory.GetCopyOfRawData();
		saveFileDataInMemory.OnDeleteCompleted += SaveFile_OnDeleteCompletedForRecreation;
		saveFileDataInMemory.Delete();
	}

	public Hashtable GetCopyOfData()
	{
		return saveFileDataInMemory.GetCopyOfData();
	}

	public byte[] GetCopyOfRawData()
	{
		return saveFileDataInMemory.GetCopyOfRawData();
	}

	public void SetData(Hashtable data, byte[] rawData)
	{
		saveFileDataInMemory.SetData(data);
		saveFileDataInMemory.SetRawData(rawData);
	}

	private void SaveFile_OnDeleteCompletedForRecreation(UserData.File file, UserData.Result result)
	{
		saveFileDataInMemory.OnDeleteCompleted -= SaveFile_OnDeleteCompletedForRecreation;
		if (result.IsSuccess())
		{
			saveFileDataInMemory.SetData(restoreDataToTransfer);
			saveFileDataInMemory.SetRawData(restoreRawDataToTransfer);
			SyncLoadModState();
			saveFileDataInMemory.OnWriteCompleted += SaveFile_OnWriteCompleted_NewFile;
			saveFileDataInMemory.Write();
		}
		else
		{
			saveFileDataInMemory.Unmount();
			saveFileDataInMemory = null;
			loadInProgress = false;
			Custom.LogWarning($"!! FAILED TO DELETE SAVE FILE. RESULT: {result}");
			progressionLoaded = true;
			Platform.NotifyUserDataReadCompleted(this);
		}
	}

	private void BypassFailure(UserData.Result result, ProgressionLoadResult loadedResult)
	{
		loadInProgress = false;
		canSave = false;
		Custom.LogWarning($"!! FAILED TO CREATE NEW SAVE FILE, RESULT: {result}");
		SyncLoadModState();
		if (!progressionLoaded)
		{
			progressionLoadedResult = ProgressionLoadResult.ERROR_NEW_FILE_CREATE_FAILED;
		}
		progressionLoaded = true;
		Platform.NotifyUserDataReadCompleted(this);
	}

	private void SaveFile_OnWriteCompleted_NewFile(UserData.File file, UserData.Result result)
	{
		file.OnWriteCompleted -= SaveFile_OnWriteCompleted_NewFile;
		if (!loadInProgress)
		{
			return;
		}
		if (result.IsSuccess())
		{
			SyncLoadModState();
			loadInProgress = false;
			if (!progressionLoaded)
			{
				progressionLoadedResult = ProgressionLoadResult.SUCCESS_CREATE_NEW_FILE;
			}
			progressionLoaded = true;
			Platform.NotifyUserDataReadCompleted(this);
		}
		else if (result.Contains(UserData.Result.NoFreeSpace))
		{
			if (!suppressProgressionError)
			{
				DialogConfirm dialog = new DialogConfirm(rainWorld.inGameTranslator.Translate("ps4_save_progress_failed_free_space"), rainWorld.processManager, delegate
				{
					SyncLoadModState();
					saveFileDataInMemory.OnWriteCompleted += SaveFile_OnWriteCompleted_NewFile;
					saveFileDataInMemory.Write();
				}, delegate
				{
					BypassFailure(result, ProgressionLoadResult.ERROR_NEW_FILE_NO_DISK_SPACE);
				});
				rainWorld.processManager.ShowDialog(dialog);
			}
			else
			{
				BypassFailure(result, ProgressionLoadResult.ERROR_NEW_FILE_NO_DISK_SPACE);
			}
		}
		else if (!suppressProgressionError)
		{
			string text = rainWorld.inGameTranslator.Translate("ps4_save_progress_failed");
			Vector2 size = DialogBoxNotify.CalculateDialogBoxSize(text, dialogUsesWordWrapping: false);
			DialogNotify dialog2 = new DialogNotify(text, size, rainWorld.processManager, delegate
			{
				BypassFailure(result, ProgressionLoadResult.ERROR_NEW_FILE_CREATE_FAILED);
			});
			rainWorld.processManager.ShowDialog(dialog2);
		}
		else
		{
			BypassFailure(result, ProgressionLoadResult.ERROR_NEW_FILE_CREATE_FAILED);
		}
	}

	private void Options_onOldPS4SaveLoaded(bool success)
	{
		rainWorld.options.onOldPS4SaveLoaded -= Options_onOldPS4SaveLoaded;
		LoadOldPS4ProgressionSave();
	}
}
