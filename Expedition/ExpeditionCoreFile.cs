using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Kittehface.Framework20;
using Menu;
using Menu.Remix;
using Modding.Expedition;
using RWCustom;
using UnityEngine;

namespace Expedition;

public class ExpeditionCoreFile
{
	private static readonly UserData.FileDefinition CORE_FILE_DEFINITION = new UserData.FileDefinition("expCore", useRawData: false, cloudEnabled: false, useEncryption: false, prettyPrint: false, useBinarySerialization: false, null, ps4Definition: new UserData.FileDefinition.PS4Definition(null, null, null, null, null, null, "Media/StreamingAssets/SaveIcon.png", 1048576L), switchDefinition: new UserData.FileDefinition.SwitchDefinition("rainworld", 1048576L));

	private RainWorld rainWorld;

	private const string CORE_KEY = "core";

	internal UserData.File CoreFileInstance;

	private bool coreFileCanSave = true;

	private bool runEnded;

	public bool coreLoaded { get; private set; }

	internal UserData.File coreFile
	{
		get
		{
			return CoreFileInstance;
		}
		set
		{
			Custom.LogImportant("@M@ CORE FILE IS BEING CHANGED! :: " + ((value == null) ? "NULL" : value.GetHashCode().ToString()));
			CoreFileInstance = value;
		}
	}

	public ExpeditionCoreFile(RainWorld rainWorld)
	{
		this.rainWorld = rainWorld;
		Load();
	}

	public void Load()
	{
		ExpLog.Log("Loading core save data");
		coreLoaded = false;
		coreFile = null;
		coreFileCanSave = true;
		if (Platform.initialized)
		{
			LoadData();
		}
		else
		{
			Platform.OnRequestUserDataRead += Platform_OnRequestUserDataRead;
		}
	}

	public void Save(bool runEnded)
	{
		this.runEnded = runEnded;
		if (coreLoaded && coreFile != null && coreFileCanSave)
		{
			ExpLog.Log("Saving core save data");
			coreFile.Set("core", ToString(), UserData.WriteMode.Immediate);
		}
		else
		{
			ExpLog.Log("FAILED to save core save data, core file was not successfully initialized in advance.");
		}
	}

	public override string ToString()
	{
		List<string> list = new List<string>
		{
			"SLOT:" + ExpeditionData.saveSlot,
			"LEVEL:" + ValueConverter.ConvertToString(ExpeditionData.level),
			"POINTS:" + ValueConverter.ConvertToString(ExpeditionData.currentPoints),
			"SLUG:" + ValueConverter.ConvertToString(ExpeditionData.slugcatPlayer.value),
			"PERKLIMIT:" + ValueConverter.ConvertToString(ExpeditionData.perkLimit),
			"TOTALPOINTS:" + ValueConverter.ConvertToString(ExpeditionData.totalPoints),
			"TOTALCHALLENGES:" + ValueConverter.ConvertToString(ExpeditionData.totalChallengesCompleted),
			"TOTALHIDDENCHALLENGES:" + ValueConverter.ConvertToString(ExpeditionData.totalHiddenChallengesCompleted),
			"WINS:" + ValueConverter.ConvertToString(ExpeditionData.totalWins),
			"INTS:" + string.Join(",", Array.ConvertAll(ExpeditionData.ints, ValueConverter.ConvertToString)),
			"MANUAL:" + ValueConverter.ConvertToString(ExpeditionData.hasViewedManual ? 1 : 0)
		};
		string text = "";
		for (int i = 0; i < ExpeditionData.slugcatWins.Count; i++)
		{
			text = text + ExpeditionData.slugcatWins.ElementAt(i).Key + "#" + ValueConverter.ConvertToString(ExpeditionData.slugcatWins.ElementAt(i).Value);
			if (i < ExpeditionData.slugcatWins.Count - 1)
			{
				text += "<>";
			}
		}
		list.Add("SLUGWINS:" + text);
		string text2 = "";
		if (ExpeditionData.challengeTypes.Count > 0)
		{
			for (int j = 0; j < ExpeditionData.challengeTypes.Count; j++)
			{
				text2 = text2 + ExpeditionData.challengeTypes.ElementAt(j).Key + "#" + ValueConverter.ConvertToString(ExpeditionData.challengeTypes.ElementAt(j).Value);
				if (j < ExpeditionData.challengeTypes.Count - 1)
				{
					text2 += "<>";
				}
			}
			list.Add("CHALLENGETYPES:" + text2);
		}
		string text3 = "";
		for (int k = 0; k < ExpeditionData.unlockables.Count; k++)
		{
			if (ExpeditionData.unlockables[k] != "")
			{
				text3 += ValueConverter.ConvertToString(ExpeditionData.unlockables[k]);
				if (k < ExpeditionData.unlockables.Count - 1)
				{
					text3 += "<>";
				}
			}
		}
		if (text3 != "")
		{
			list.Add("UNLOCKS:" + text3);
		}
		string text4 = "";
		for (int l = 0; l < ExpeditionData.newSongs.Count; l++)
		{
			if (ExpeditionData.newSongs[l] != "")
			{
				text4 += ValueConverter.ConvertToString(ExpeditionData.newSongs[l]);
				if (l < ExpeditionData.newSongs.Count - 1)
				{
					text4 += "<>";
				}
			}
		}
		list.Add("NEWSONGS:" + text4);
		string text5 = "";
		for (int m = 0; m < ExpeditionData.completedQuests.Count; m++)
		{
			text5 += ValueConverter.ConvertToString(ExpeditionData.completedQuests[m]);
			if (m < ExpeditionData.completedQuests.Count - 1)
			{
				text5 += "<>";
			}
		}
		list.Add("QUESTS:" + text5);
		string text6 = "";
		for (int n = 0; n < ExpeditionData.completedMissions.Count; n++)
		{
			text6 += ValueConverter.ConvertToString(ExpeditionData.completedMissions[n]);
			if (n < ExpeditionData.completedMissions.Count - 1)
			{
				text6 += "<>";
			}
		}
		list.Add("MISSIONS:" + text6);
		list.Add("MENUSONG:" + ExpeditionData.menuSong);
		list.Add("[CHALLENGES]");
		foreach (KeyValuePair<SlugcatStats.Name, List<Challenge>> allChallengeList in ExpeditionData.allChallengeLists)
		{
			if (allChallengeList.Key.value != ExpeditionData.slugcatPlayer.value)
			{
				for (int num = 0; num < allChallengeList.Value.Count; num++)
				{
					list.Add(allChallengeList.Key.value + "#" + allChallengeList.Value[num].ToString());
				}
			}
		}
		if (!runEnded)
		{
			for (int num2 = 0; num2 < ExpeditionData.challengeList.Count; num2++)
			{
				list.Add(ExpeditionData.slugcatPlayer.value + "#" + ExpeditionData.challengeList[num2].ToString());
			}
		}
		list.Add("[END CHALLENGES]");
		list.Add("[UNLOCKS]");
		foreach (KeyValuePair<SlugcatStats.Name, List<string>> allUnlock in ExpeditionGame.allUnlocks)
		{
			if (allUnlock.Key.value != ExpeditionData.slugcatPlayer.value)
			{
				list.Add(allUnlock.Key.value + "#" + ActiveUnlocksString(allUnlock.Value));
			}
		}
		if (!runEnded && ExpeditionGame.activeUnlocks != null && ExpeditionGame.activeUnlocks.Count > 0)
		{
			list.Add(ExpeditionData.slugcatPlayer.value + "#" + ActiveUnlocksString(ExpeditionGame.activeUnlocks));
		}
		list.Add("[END UNLOCKS]");
		if (!runEnded)
		{
			list.Add("[PASSAGES]");
			foreach (KeyValuePair<string, int> allEarnedPassage in ExpeditionData.allEarnedPassages)
			{
				list.Add(allEarnedPassage.Key + "#" + allEarnedPassage.Value);
			}
			list.Add("[END PASSAGES]");
		}
		if (ExpeditionData.allActiveMissions.Count > 0)
		{
			ExpLog.Log("SAVING ACTIVE MISSIONS");
			list.Add("[MISSION]");
			foreach (KeyValuePair<string, string> allActiveMission in ExpeditionData.allActiveMissions)
			{
				if (allActiveMission.Value != "")
				{
					ExpLog.Log("SLUG: " + allActiveMission.Key + " | " + allActiveMission.Value);
					list.Add(allActiveMission.Key + "#" + allActiveMission.Value);
				}
			}
			list.Add("[END MISSION]");
		}
		if (ExpeditionData.missionBestTimes.Keys.Count > 0)
		{
			list.Add("[TIMES]");
			for (int num3 = 0; num3 < ExpeditionData.missionBestTimes.Keys.Count; num3++)
			{
				ExpLog.Log("TIME: " + ExpeditionData.missionBestTimes.Keys.ElementAt(num3) + "#" + ValueConverter.ConvertToString(ExpeditionData.missionBestTimes.ElementAt(num3).Value));
				list.Add(ExpeditionData.missionBestTimes.Keys.ElementAt(num3) + "#" + ValueConverter.ConvertToString(ExpeditionData.missionBestTimes.ElementAt(num3).Value));
			}
			list.Add("[END TIMES]");
		}
		if (ExpeditionData.requiredExpeditionContent != null && ExpeditionData.requiredExpeditionContent.Keys.Count > 0)
		{
			list.Add("[CONTENT]");
			for (int num4 = 0; num4 < ExpeditionData.requiredExpeditionContent.Keys.Count; num4++)
			{
				string text7 = ExpeditionData.requiredExpeditionContent.Keys.ElementAt(num4);
				string text8 = text7 + "#";
				for (int num5 = 0; num5 < ExpeditionData.requiredExpeditionContent[text7].Count; num5++)
				{
					text8 += ExpeditionData.requiredExpeditionContent[text7][num5];
					if (num5 < ExpeditionData.requiredExpeditionContent[text7].Count - 1)
					{
						text8 += "<mod>";
					}
				}
				list.Add(text8);
			}
			list.Add("[END CONTENT]");
		}
		return string.Join("<expC>", list.ToArray());
	}

	public void FromString(string saveString)
	{
		ExpeditionData.unlockables = new List<string>();
		ExpeditionData.completedQuests = new List<string>();
		ExpeditionData.allChallengeLists = new Dictionary<SlugcatStats.Name, List<Challenge>>();
		ExpeditionGame.allUnlocks = new Dictionary<SlugcatStats.Name, List<string>>();
		ExpeditionData.challengeTypes = new Dictionary<string, int>();
		ExpeditionData.level = 1;
		ExpeditionData.currentPoints = 0;
		ExpeditionData.totalPoints = 0;
		ExpeditionData.perkLimit = 1;
		ExpeditionData.totalChallengesCompleted = 0;
		ExpeditionData.totalHiddenChallengesCompleted = 0;
		ExpeditionData.totalWins = 0;
		ExpeditionData.slugcatPlayer = SlugcatStats.Name.White;
		ExpeditionData.completedMissions = new List<string>();
		ExpeditionData.slugcatWins = new Dictionary<string, int>();
		ExpeditionData.newSongs = new List<string>();
		ExpeditionData.allActiveMissions = new Dictionary<string, string>();
		ExpeditionData.missionBestTimes = new Dictionary<string, int>();
		ExpeditionData.ints = new int[8];
		ExpeditionData.requiredExpeditionContent = new Dictionary<string, List<string>>();
		ExpeditionData.ClearActiveChallengeList();
		string[] array = Regex.Split(saveString, "<expC>");
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		bool flag5 = false;
		bool flag6 = false;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].StartsWith("SLOT:"))
			{
				int num = int.Parse(Regex.Split(array[i], ":")[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				if (num < 0)
				{
					num = 0;
				}
				ExpeditionData.saveSlot = num;
			}
			if (array[i].StartsWith("LEVEL:"))
			{
				ExpeditionData.level = int.Parse(Regex.Split(array[i], ":")[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			}
			if (array[i].StartsWith("PERKLIMIT:"))
			{
				ExpeditionData.perkLimit = int.Parse(Regex.Split(array[i], ":")[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			}
			if (array[i].StartsWith("POINTS:"))
			{
				ExpeditionData.currentPoints = int.Parse(Regex.Split(array[i], ":")[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			}
			if (array[i].StartsWith("TOTALPOINTS:"))
			{
				ExpeditionData.totalPoints = int.Parse(Regex.Split(array[i], ":")[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			}
			if (array[i].StartsWith("TOTALCHALLENGES:"))
			{
				ExpeditionData.totalChallengesCompleted = int.Parse(Regex.Split(array[i], ":")[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			}
			if (array[i].StartsWith("TOTALHIDDENCHALLENGES:"))
			{
				ExpeditionData.totalHiddenChallengesCompleted = int.Parse(Regex.Split(array[i], ":")[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			}
			if (array[i].StartsWith("WINS:"))
			{
				ExpeditionData.totalWins = int.Parse(Regex.Split(array[i], ":")[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			}
			if (array[i].StartsWith("SLUG:"))
			{
				string text = Regex.Split(array[i], ":")[1];
				switch (text)
				{
				case "Rivulet":
				case "Spear":
				case "Gourmand":
				case "Artificer":
				case "Saint":
					if (!ModManager.MSC)
					{
						text = "White";
					}
					break;
				}
				ExpeditionData.slugcatPlayer = new SlugcatStats.Name(text);
			}
			if (array[i].StartsWith("MANUAL:"))
			{
				ExpeditionData.hasViewedManual = Regex.Split(array[i], ":")[1] == "1";
			}
			if (array[i].StartsWith("MENUSONG:"))
			{
				ExpeditionData.menuSong = Regex.Split(array[i], ":")[1];
			}
			if (array[i].StartsWith("CHALLENGETYPES:") && Regex.Split(array[i], ":")[1] != "")
			{
				string[] array2 = Regex.Split(Regex.Split(array[i], ":")[1], "<>");
				for (int j = 0; j < array2.Length; j++)
				{
					string[] array3 = Regex.Split(array2[j], "#");
					if (ExpeditionData.challengeTypes.ContainsKey(array3[0]))
					{
						ExpeditionData.challengeTypes[array3[0]] = int.Parse(array3[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					}
					else
					{
						ExpeditionData.challengeTypes.Add(array3[0], int.Parse(array3[1], NumberStyles.Any, CultureInfo.InvariantCulture));
					}
				}
			}
			if (array[i].StartsWith("SLUGWINS:") && Regex.Split(array[i], ":")[1] != "")
			{
				string[] array4 = Regex.Split(Regex.Split(array[i], ":")[1], "<>");
				for (int k = 0; k < array4.Length; k++)
				{
					string[] array5 = Regex.Split(array4[k], "#");
					if (ExpeditionData.slugcatWins.ContainsKey(array5[0]))
					{
						ExpeditionData.slugcatWins[array5[0]] = int.Parse(array5[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					}
					else
					{
						ExpeditionData.slugcatWins.Add(array5[0], int.Parse(array5[1], NumberStyles.Any, CultureInfo.InvariantCulture));
					}
				}
			}
			if (array[i].StartsWith("UNLOCKS:") && Regex.Split(array[i], ":")[1] != "")
			{
				string[] array6 = Regex.Split(Regex.Split(array[i], ":")[1], "<>");
				int num2 = 1;
				for (int l = 0; l < array6.Length; l++)
				{
					if (array6[l].StartsWith("per-"))
					{
						int num3 = int.Parse(Regex.Split(array6[l], "-")[1], NumberStyles.Any, CultureInfo.InvariantCulture);
						num2 += num3;
					}
					ExpeditionData.unlockables.Add(array6[l]);
				}
				if (num2 > ExpeditionData.perkLimit)
				{
					ExpeditionData.perkLimit = num2;
				}
			}
			if (array[i].StartsWith("NEWSONGS:") && Regex.Split(array[i], ":")[1] != "")
			{
				string[] array7 = Regex.Split(Regex.Split(array[i], ":")[1], "<>");
				for (int m = 0; m < array7.Length; m++)
				{
					if (!ExpeditionData.newSongs.Contains(array7[m]))
					{
						ExpeditionData.newSongs.Add(array7[m]);
					}
				}
			}
			if (array[i].StartsWith("QUESTS:") && Regex.Split(array[i], ":")[1] != "")
			{
				string[] array8 = Regex.Split(Regex.Split(array[i], ":")[1], "<>");
				for (int n = 0; n < array8.Length; n++)
				{
					if (array8[n] != "")
					{
						ExpeditionData.completedQuests.Add(array8[n]);
					}
				}
			}
			if (array[i].StartsWith("MISSIONS:") && Regex.Split(array[i], ":")[1] != "")
			{
				string[] array9 = Regex.Split(Regex.Split(array[i], ":")[1], "<>");
				for (int num4 = 0; num4 < array9.Length; num4++)
				{
					if (array9[num4] != "")
					{
						ExpeditionData.completedMissions.Add(array9[num4]);
					}
				}
			}
			if (array[i].StartsWith("INTS:") && Regex.Split(array[i], ":")[1] != "")
			{
				ExpeditionData.ints = Array.ConvertAll(Regex.Split(array[i], ":")[1].Split(','), ValueConverter.ConvertToValue<int>);
			}
			if (array[i] == "[END PASSAGES]")
			{
				flag3 = false;
			}
			if (array[i] == "[END UNLOCKS]")
			{
				flag2 = false;
			}
			if (array[i] == "[END CHALLENGES]")
			{
				flag = false;
			}
			if (array[i] == "[END MISSION]")
			{
				flag4 = false;
			}
			if (array[i] == "[END TIMES]")
			{
				flag5 = false;
			}
			if (array[i] == "[END CONTENT]")
			{
				flag6 = false;
			}
			if (flag)
			{
				string[] array10 = Regex.Split(array[i], "#");
				SlugcatStats.Name name = new SlugcatStats.Name(array10[0]);
				string[] array11 = Regex.Split(array10[1], "~");
				string type = array11[0];
				string text2 = array11[1];
				try
				{
					Challenge challenge = (Challenge)Activator.CreateInstance(ChallengeOrganizer.availableChallengeTypes.Find((Challenge c) => c.GetType().Name == type).GetType());
					challenge.FromString(text2);
					ExpLog.Log(challenge.description);
					if (!ExpeditionData.allChallengeLists.ContainsKey(name))
					{
						ExpeditionData.allChallengeLists.Add(name, new List<Challenge>());
					}
					ExpeditionData.allChallengeLists[name].Add(challenge);
					ExpLog.Log("[" + name.value + "] Recreated " + type + " : " + text2);
				}
				catch (Exception ex)
				{
					ExpLog.Log("ERROR: Problem recreating challenge type with reflection: " + ex.Message);
				}
			}
			if (flag2)
			{
				string[] array12 = Regex.Split(array[i], "#");
				SlugcatStats.Name key = new SlugcatStats.Name(array12[0]);
				string[] array13 = Regex.Split(array12[1], "><");
				for (int num5 = 0; num5 < array13.Length; num5++)
				{
					if (!ExpeditionGame.allUnlocks.ContainsKey(key))
					{
						ExpeditionGame.allUnlocks[key] = new List<string>();
					}
					ExpeditionGame.allUnlocks[key].Add(array13[num5]);
				}
			}
			if (flag3)
			{
				string key2 = Regex.Split(array[i], "#")[0];
				int value = int.Parse(Regex.Split(array[i], "#")[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				if (!ExpeditionData.allEarnedPassages.ContainsKey(key2))
				{
					ExpeditionData.allEarnedPassages.Add(key2, value);
				}
				ExpeditionData.allEarnedPassages[key2] = value;
			}
			if (flag4)
			{
				ExpLog.Log("LOADING ACTIVE MISSIONS:");
				string text3 = Regex.Split(array[i], "#")[0];
				string text4 = Regex.Split(array[i], "#")[1];
				ExpLog.Log("SLUG: " + text3 + " | " + text4);
				if (!ExpeditionData.allActiveMissions.ContainsKey(text3))
				{
					ExpeditionData.allActiveMissions.Add(text3, text4);
				}
			}
			if (flag5)
			{
				string[] array14 = Regex.Split(array[i], "#");
				ExpLog.Log("TIME COUNT: " + array14.Length);
				string key3 = array14[0];
				ExpLog.Log("TIME MIS: " + array14[0]);
				int value2 = int.Parse(array14[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				ExpLog.Log("TIME TIME: " + array14[1]);
				if (!ExpeditionData.missionBestTimes.ContainsKey(key3))
				{
					ExpeditionData.missionBestTimes.Add(key3, value2);
				}
				else
				{
					ExpeditionData.missionBestTimes[key3] = value2;
				}
			}
			if (flag6)
			{
				string[] array15 = Regex.Split(array[i], "#");
				string text5 = array15[0];
				List<string> value3 = Regex.Split(array15[1], "<mod>").ToList();
				ExpeditionData.requiredExpeditionContent.Add(text5, value3);
				ExpLog.Log(text5 + " content: " + array15[1]);
			}
			if (array[i] == "[CHALLENGES]")
			{
				flag = true;
			}
			if (array[i] == "[UNLOCKS]")
			{
				flag2 = true;
			}
			if (array[i] == "[PASSAGES]")
			{
				flag3 = true;
			}
			if (array[i] == "[MISSION]")
			{
				flag4 = true;
			}
			if (array[i] == "[TIMES]")
			{
				flag5 = true;
			}
			if (array[i] == "[CONTENT]")
			{
				flag6 = true;
			}
		}
	}

	private string ActiveUnlocksString(List<string> activeUnlocks)
	{
		string text = "";
		if (activeUnlocks != null)
		{
			for (int i = 0; i < activeUnlocks.Count; i++)
			{
				text += activeUnlocks[i];
				if (i != activeUnlocks.Count - 1)
				{
					text += "><";
				}
			}
		}
		return text;
	}

	private void LoadFailedFallback()
	{
		ExpeditionData.level = 1;
		ExpeditionData.currentPoints = 0;
		ExpeditionData.totalPoints = 0;
		ExpeditionData.perkLimit = 1;
		ExpeditionData.totalChallengesCompleted = 0;
		ExpeditionData.totalHiddenChallengesCompleted = 0;
		ExpeditionData.challengeTypes = new Dictionary<string, int>();
		ExpeditionData.totalWins = 0;
		ExpeditionData.slugcatPlayer = SlugcatStats.Name.White;
		ExpeditionData.saveSlot = -1;
		ExpeditionData.unlockables = new List<string> { "mus-1" };
		ExpeditionData.completedQuests = new List<string>();
		ExpeditionData.completedMissions = new List<string>();
		ExpeditionData.slugcatWins = new Dictionary<string, int>();
		ExpeditionData.newSongs = new List<string>();
		ExpeditionData.menuSong = "";
		ExpeditionData.allChallengeLists = new Dictionary<SlugcatStats.Name, List<Challenge>>();
		ExpeditionGame.allUnlocks = new Dictionary<SlugcatStats.Name, List<string>>();
		ExpeditionData.allActiveMissions = new Dictionary<string, string>();
		ExpeditionData.missionBestTimes = new Dictionary<string, int>();
		ExpeditionData.ints = new int[8];
		ExpeditionData.ClearActiveChallengeList();
	}

	private void OnLoadFinished()
	{
		foreach (CustomPerk registeredPerk in CustomPerks.RegisteredPerks)
		{
			if (registeredPerk.UnlockedByDefault && !ExpeditionData.unlockables.Contains(registeredPerk.ID))
			{
				ExpeditionData.unlockables.Add(registeredPerk.ID);
			}
		}
		foreach (CustomBurden registeredBurden in CustomBurdens.RegisteredBurdens)
		{
			if (registeredBurden.UnlockedByDefault && !ExpeditionData.unlockables.Contains(registeredBurden.ID))
			{
				ExpeditionData.unlockables.Add(registeredBurden.ID);
			}
		}
		if (ExpeditionProgression.perkGroups.Count <= 0)
		{
			return;
		}
		foreach (KeyValuePair<SlugcatStats.Name, List<string>> allUnlock in ExpeditionGame.allUnlocks)
		{
			allUnlock.Value.RemoveAll((string unlock) => !ExpeditionProgression.perkGroups.Any((KeyValuePair<string, List<string>> group) => group.Value.Contains(unlock)) && !ExpeditionProgression.burdenGroups.Any((KeyValuePair<string, List<string>> group) => group.Value.Contains(unlock)));
		}
	}

	private string ExpeditionSaveFileName()
	{
		if (rainWorld.options.saveSlot >= 0)
		{
			return "expCore" + (rainWorld.options.saveSlot + 1);
		}
		return "expCore" + Math.Abs(rainWorld.options.saveSlot);
	}

	private void LoadData()
	{
		if (Profiles.ActiveProfiles.Count > 0)
		{
			UserData.OnFileMounted += UserData_OnFileMounted;
			UserData.FileDefinition fileDefinition = new UserData.FileDefinition(CORE_FILE_DEFINITION);
			fileDefinition.fileName = ExpeditionSaveFileName();
			fileDefinition.ps4Definition.title = rainWorld.inGameTranslator.Translate("ps4_expedition_core_title");
			fileDefinition.ps4Definition.detail = rainWorld.inGameTranslator.Translate("ps4_expedition_core_description");
			Custom.LogImportant("@M@ STARTING MOUNT FOR CORE FILE");
			UserData.Mount(Profiles.ActiveProfiles[0], null, fileDefinition);
		}
		else
		{
			LoadFailedFallback();
			coreFile = null;
			coreFileCanSave = false;
			coreLoaded = true;
			Platform.NotifyUserDataReadCompleted(this);
			OnLoadFinished();
		}
	}

	private void UserData_OnFileMounted(UserData.File file, UserData.Result result)
	{
		CORE_FILE_DEFINITION.fileName = ExpeditionSaveFileName();
		UserData.OnFileMounted -= UserData_OnFileMounted;
		if (result.IsSuccess())
		{
			Custom.LogImportant("@M@ ExpeditionCoreFile OnFileMounted SUCCESS :: " + file.GetHashCode());
			coreFile = file;
			coreFile.OnReadCompleted += CoreFile_OnReadCompleted;
			coreFile.Read();
		}
		else
		{
			Custom.LogWarning("@M@ ExpeditionCoreFile OnFileMounted FAILED :: " + ((file == null) ? "NULL" : file.GetHashCode().ToString()));
			LoadFailedFallback();
			coreFile = null;
			coreLoaded = true;
			coreFileCanSave = false;
			Platform.NotifyUserDataReadCompleted(this);
			OnLoadFinished();
		}
	}

	private void CoreFile_OnReadCompleted(UserData.File file, UserData.Result result)
	{
		coreFile.OnReadCompleted -= CoreFile_OnReadCompleted;
		if (result.IsSuccess())
		{
			if (result.Contains(UserData.Result.FileNotFound))
			{
				coreFile.OnWriteCompleted += CoreFile_OnWriteCompleted_NewFile;
				LoadFailedFallback();
				coreFile.Write();
				return;
			}
			coreLoaded = true;
			if (coreFile.Contains("core"))
			{
				FromString(coreFile.Get("core", ""));
			}
			else
			{
				LoadFailedFallback();
			}
			Platform.NotifyUserDataReadCompleted(this);
			OnLoadFinished();
		}
		else if (result.Contains(UserData.Result.CorruptData))
		{
			ReportCorruptFileAndDeleteData();
		}
		else if (result.Contains(UserData.Result.FileNotFound))
		{
			coreFile.OnWriteCompleted += CoreFile_OnWriteCompleted_NewFile;
			LoadFailedFallback();
			coreFile.Write();
		}
		else
		{
			LoadFailedFallback();
			coreFileCanSave = false;
			coreLoaded = true;
			Platform.NotifyUserDataReadCompleted(this);
			OnLoadFinished();
		}
	}

	private void OnCheckConsoleBackupExists(UserData.File file, UserData.Result result)
	{
		coreFile.OnBackupExistsCompleted -= OnCheckConsoleBackupExists;
		if (result == UserData.Result.Success)
		{
			coreFile.OnRestoreBackupCompleted += OnRestoreBackup;
			coreFile.RestoreBackup();
		}
		else
		{
			ReportCorruptFileAndDeleteData();
		}
	}

	private void OnRestoreBackup(UserData.File file, UserData.Result result)
	{
		coreFile.OnRestoreBackupCompleted -= OnRestoreBackup;
		if (result == UserData.Result.Success)
		{
			string text = rainWorld.inGameTranslator.Translate("ps4_load_expedition_restore_corrupt");
			DialogNotify dialog = new DialogNotify(size: DialogBoxNotify.CalculateDialogBoxSize(text, dialogUsesWordWrapping: false), description: Custom.ReplaceLineDelimeters(text), manager: rainWorld.processManager, onOK: delegate
			{
				Load();
			});
			rainWorld.processManager.ShowDialog(dialog);
		}
		else
		{
			string text2 = rainWorld.inGameTranslator.Translate("ps4_load_expedition_restore_corrupt_failed");
			DialogNotify dialog2 = new DialogNotify(size: DialogBoxNotify.CalculateDialogBoxSize(text2, dialogUsesWordWrapping: false), description: Custom.ReplaceLineDelimeters(text2), manager: rainWorld.processManager, onOK: delegate
			{
				FinishLoadWithFailure();
			});
			rainWorld.processManager.ShowDialog(dialog2);
		}
	}

	private void ReportCorruptFileAndDeleteData()
	{
		string text = rainWorld.inGameTranslator.Translate("ps4_load_expedition_core_failed");
		Vector2 size = DialogBoxNotify.CalculateDialogBoxSize(text, dialogUsesWordWrapping: false);
		DialogNotify dialog = new DialogNotify(text, size, rainWorld.processManager, delegate
		{
			coreFile.OnDeleteCompleted += CoreFile_OnDeleteCompleted;
			coreFile.Delete();
		});
		rainWorld.processManager.ShowDialog(dialog);
	}

	private void CoreFile_OnDeleteCompleted(UserData.File file, UserData.Result result)
	{
		coreFile.OnDeleteCompleted -= CoreFile_OnDeleteCompleted;
		if (result.IsSuccess())
		{
			coreFile.OnWriteCompleted += CoreFile_OnWriteCompleted_NewFile;
			LoadFailedFallback();
			coreFile.Write();
		}
		else
		{
			FinishLoadWithFailure();
		}
	}

	private void FinishLoadWithFailure()
	{
		LoadFailedFallback();
		coreFile.Unmount();
		coreFile = null;
		coreLoaded = true;
		coreFileCanSave = false;
		Platform.NotifyUserDataReadCompleted(this);
		OnLoadFinished();
	}

	private void CoreFile_OnWriteCompleted_NewFile(UserData.File file, UserData.Result result)
	{
		coreFile.OnWriteCompleted -= CoreFile_OnWriteCompleted_NewFile;
		if (result.IsSuccess())
		{
			coreLoaded = true;
			Platform.NotifyUserDataReadCompleted(this);
			OnLoadFinished();
		}
		else if (result.Contains(UserData.Result.NoFreeSpace))
		{
			DialogConfirm dialog = new DialogConfirm(rainWorld.inGameTranslator.Translate("ps4_save_expedition_core_failed_free_space"), rainWorld.processManager, delegate
			{
				coreFile.OnWriteCompleted += CoreFile_OnWriteCompleted_NewFile;
				coreFile.Write();
			}, delegate
			{
				coreFileCanSave = false;
				coreLoaded = true;
				Platform.NotifyUserDataReadCompleted(this);
				OnLoadFinished();
			});
			rainWorld.processManager.ShowDialog(dialog);
		}
		else
		{
			coreFileCanSave = false;
			string text = rainWorld.inGameTranslator.Translate("ps4_save_expedition_core_failed");
			Vector2 size = DialogBoxNotify.CalculateDialogBoxSize(text, dialogUsesWordWrapping: false);
			DialogNotify dialog2 = new DialogNotify(text, size, rainWorld.processManager, delegate
			{
				coreLoaded = true;
				Platform.NotifyUserDataReadCompleted(this);
				OnLoadFinished();
			});
			rainWorld.processManager.ShowDialog(dialog2);
		}
	}

	private void Platform_OnRequestUserDataRead(List<object> pendingUserDataReads)
	{
		Platform.OnRequestUserDataRead -= Platform_OnRequestUserDataRead;
		pendingUserDataReads.Add(this);
		LoadData();
	}
}
