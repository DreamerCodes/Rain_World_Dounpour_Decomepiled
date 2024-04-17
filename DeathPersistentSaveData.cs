using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class DeathPersistentSaveData
{
	public class Tutorial : ExtEnum<Tutorial>
	{
		public static readonly Tutorial GoExplore = new Tutorial("GoExplore", register: true);

		public static readonly Tutorial ScavToll = new Tutorial("ScavToll", register: true);

		public static readonly Tutorial ScavMerchant = new Tutorial("ScavMerchant", register: true);

		public static readonly Tutorial DangleFruitInWater = new Tutorial("DangleFruitInWater", register: true);

		public static readonly Tutorial KarmaFlower = new Tutorial("KarmaFlower", register: true);

		public static readonly Tutorial PoleMimic = new Tutorial("PoleMimic", register: true);

		public Tutorial(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class SongPlayRecord
	{
		public string songName;

		public int cycleLastPlayed;

		public SongPlayRecord(string songName, int cycleLastPlayed)
		{
			this.songName = songName;
			this.cycleLastPlayed = cycleLastPlayed;
		}
	}

	public class SessionRecord
	{
		public bool survived;

		public bool travelled;

		public string unrecognizedRecord = "";

		public SessionRecord(bool survived, bool travelled)
		{
			this.survived = survived;
			this.travelled = travelled;
		}

		public override string ToString()
		{
			return (survived ? "1" : "0") + (travelled ? "1" : "0") + unrecognizedRecord;
		}

		public static SessionRecord MakeFromString(string s)
		{
			SessionRecord sessionRecord = new SessionRecord(s[0] == '1', s[1] == '1');
			if (s.Length > 2)
			{
				sessionRecord.unrecognizedRecord = s.Substring(2);
			}
			return sessionRecord;
		}
	}

	public int karma;

	public int karmaCap;

	public bool reinforcedKarma;

	public bool theMark;

	public bool redsDeath;

	public bool ascended;

	public bool pebblesHasIncreasedRedsKarmaCap;

	public Dictionary<GhostWorldPresence.GhostID, int> ghostsTalkedTo;

	public List<string> ghostsTalkedToUnrecognized;

	public List<SongPlayRecord> songsPlayRecords;

	public List<SessionRecord> sessionTrackRecord;

	public WinState winState;

	public WorldCoordinate? karmaFlowerPosition;

	public List<RegionState.ConsumedItem> consumedFlowers;

	private bool fresh = true;

	public float howWellIsPlayerDoing = -1f;

	public List<Tutorial> tutorialMessages;

	public List<WinState.EndgameID> endGameMetersEverShown;

	public int foodReplenishBonus;

	public int worldVersion;

	public int deaths;

	public int survives;

	public int quits;

	public List<WorldCoordinate> deathPositions;

	public List<string> unlockedGates;

	public List<string> unrecognizedSaveStrings;

	public bool SLSiren;

	public bool altEnding;

	public bool ripPebbles;

	public bool ripMoon;

	public List<ChatlogData.ChatlogID> prePebChatlogsRead;

	public List<ChatlogData.ChatlogID> chatlogsRead;

	public int deathTime;

	public int friendsSaved;

	public int tipCounter;

	public int tipSeed;

	public bool GoExploreMessage
	{
		get
		{
			return tutorialMessages.Contains(Tutorial.GoExplore);
		}
		set
		{
			SetTutorialValue(Tutorial.GoExplore, value);
		}
	}

	public bool ScavTollMessage
	{
		get
		{
			return tutorialMessages.Contains(Tutorial.ScavToll);
		}
		set
		{
			SetTutorialValue(Tutorial.ScavToll, value);
		}
	}

	public bool ScavMerchantMessage
	{
		get
		{
			return tutorialMessages.Contains(Tutorial.ScavMerchant);
		}
		set
		{
			SetTutorialValue(Tutorial.ScavMerchant, value);
		}
	}

	public bool DangleFruitInWaterMessage
	{
		get
		{
			return tutorialMessages.Contains(Tutorial.DangleFruitInWater);
		}
		set
		{
			SetTutorialValue(Tutorial.DangleFruitInWater, value);
		}
	}

	public bool KarmaFlowerMessage
	{
		get
		{
			return tutorialMessages.Contains(Tutorial.KarmaFlower);
		}
		set
		{
			SetTutorialValue(Tutorial.KarmaFlower, value);
		}
	}

	public bool PoleMimicEverSeen
	{
		get
		{
			return tutorialMessages.Contains(Tutorial.PoleMimic);
		}
		set
		{
			SetTutorialValue(Tutorial.PoleMimic, value);
		}
	}

	public bool ArtificerTutorialMessage
	{
		get
		{
			if (ModManager.MSC)
			{
				return tutorialMessages.Contains(MoreSlugcatsEnums.Tutorial.Artificer);
			}
			return false;
		}
		set
		{
			if (ModManager.MSC)
			{
				SetTutorialValue(MoreSlugcatsEnums.Tutorial.Artificer, value);
			}
		}
	}

	public bool TongueTutorialMessage
	{
		get
		{
			if (ModManager.MSC)
			{
				return tutorialMessages.Contains(MoreSlugcatsEnums.Tutorial.SaintTongue);
			}
			return false;
		}
		set
		{
			if (ModManager.MSC)
			{
				SetTutorialValue(MoreSlugcatsEnums.Tutorial.SaintTongue, value);
			}
		}
	}

	public bool SaintEnlightMessage
	{
		get
		{
			if (ModManager.MSC)
			{
				return tutorialMessages.Contains(MoreSlugcatsEnums.Tutorial.SaintEnlight);
			}
			return false;
		}
		set
		{
			if (ModManager.MSC)
			{
				SetTutorialValue(MoreSlugcatsEnums.Tutorial.SaintEnlight, value);
			}
		}
	}

	public bool KarmicBurstMessage
	{
		get
		{
			if (ModManager.MSC)
			{
				return tutorialMessages.Contains(MoreSlugcatsEnums.Tutorial.KarmicBurst);
			}
			return false;
		}
		set
		{
			if (ModManager.MSC)
			{
				SetTutorialValue(MoreSlugcatsEnums.Tutorial.KarmicBurst, value);
			}
		}
	}

	public bool SMTutorialMessage
	{
		get
		{
			if (ModManager.MSC)
			{
				return tutorialMessages.Contains(MoreSlugcatsEnums.Tutorial.Spearmaster);
			}
			return false;
		}
		set
		{
			if (ModManager.MSC)
			{
				SetTutorialValue(MoreSlugcatsEnums.Tutorial.Spearmaster, value);
			}
		}
	}

	public bool SMEatTutorial
	{
		get
		{
			if (ModManager.MSC)
			{
				return tutorialMessages.Contains(MoreSlugcatsEnums.Tutorial.SpearmasterEat);
			}
			return false;
		}
		set
		{
			if (ModManager.MSC)
			{
				SetTutorialValue(MoreSlugcatsEnums.Tutorial.SpearmasterEat, value);
			}
		}
	}

	public bool ArtificerMaulTutorial
	{
		get
		{
			if (ModManager.MSC)
			{
				return tutorialMessages.Contains(MoreSlugcatsEnums.Tutorial.ArtificerMaul);
			}
			return false;
		}
		set
		{
			if (ModManager.MSC)
			{
				SetTutorialValue(MoreSlugcatsEnums.Tutorial.ArtificerMaul, value);
			}
		}
	}

	public bool GateStandTutorial
	{
		get
		{
			if (ModManager.MMF)
			{
				return tutorialMessages.Contains(MMFEnums.Tutorial.GateStand);
			}
			return false;
		}
		set
		{
			if (ModManager.MMF)
			{
				SetTutorialValue(MMFEnums.Tutorial.GateStand, value);
			}
		}
	}

	public DeathPersistentSaveData(SlugcatStats.Name slugcat)
	{
		karmaCap = 4;
		if (ModManager.MSC)
		{
			if (slugcat == MoreSlugcatsEnums.SlugcatStatsName.Saint)
			{
				karmaCap = 1;
				karma = 1;
			}
			else if (slugcat == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
			{
				karmaCap = 0;
			}
			friendsSaved = 0;
			chatlogsRead = new List<ChatlogData.ChatlogID>();
			prePebChatlogsRead = new List<ChatlogData.ChatlogID>();
			if (ModManager.Expedition && Custom.rainWorld.ExpeditionMode)
			{
				karmaCap = 4;
				karma = 1;
			}
		}
		ghostsTalkedTo = new Dictionary<GhostWorldPresence.GhostID, int>();
		ghostsTalkedToUnrecognized = new List<string>();
		songsPlayRecords = new List<SongPlayRecord>();
		sessionTrackRecord = new List<SessionRecord>();
		winState = new WinState();
		consumedFlowers = new List<RegionState.ConsumedItem>();
		tutorialMessages = new List<Tutorial>();
		endGameMetersEverShown = new List<WinState.EndgameID>();
		deathPositions = new List<WorldCoordinate>();
		unrecognizedSaveStrings = new List<string>();
		if (CanUseUnlockedGates(slugcat))
		{
			unlockedGates = new List<string>();
		}
	}

	public bool CanUseUnlockedGates(SlugcatStats.Name slugcat)
	{
		if (!(slugcat == SlugcatStats.Name.Yellow))
		{
			if (ModManager.MMF && MMF.cfgGlobalMonkGates != null)
			{
				return MMF.cfgGlobalMonkGates.Value;
			}
			return false;
		}
		return true;
	}

	public void SetTutorialValue(Tutorial tut, bool flag)
	{
		if (flag)
		{
			if (!tutorialMessages.Contains(tut))
			{
				tutorialMessages.Add(tut);
			}
		}
		else if (tutorialMessages.Contains(tut))
		{
			tutorialMessages.Remove(tut);
		}
	}

	public override string ToString()
	{
		Custom.LogWarning("---------------DEATH PERSISTENT DATA SAV ERROR");
		return base.ToString();
	}

	public string SaveToString(bool saveAsIfPlayerDied, bool saveAsIfPlayerQuit)
	{
		Custom.Log("Saving death persistent data", saveAsIfPlayerDied.ToString(), saveAsIfPlayerQuit.ToString());
		if (fresh)
		{
			Custom.LogWarning("saving death persistent data that hasn't been loaded properly!!!");
		}
		string text = "";
		if (saveAsIfPlayerDied || saveAsIfPlayerQuit || redsDeath)
		{
			text += "REDSDEATH<dpA>";
		}
		if (ascended)
		{
			text += "ASCENDED<dpA>";
		}
		if (saveAsIfPlayerDied)
		{
			text += "REINFORCEDKARMA<dpB>0<dpA>";
			text = ((!reinforcedKarma) ? (text + string.Format(CultureInfo.InvariantCulture, "KARMA<dpB>{0}<dpA>", karma - 1)) : (text + string.Format(CultureInfo.InvariantCulture, "KARMA<dpB>{0}<dpA>", karma)));
		}
		else
		{
			text = text + "REINFORCEDKARMA<dpB>" + (reinforcedKarma ? "1" : "0") + "<dpA>";
			text += string.Format(CultureInfo.InvariantCulture, "KARMA<dpB>{0}<dpA>", karma);
		}
		text += string.Format(CultureInfo.InvariantCulture, "KARMACAP<dpB>{0}<dpA>", karmaCap);
		if (theMark)
		{
			text += "HASTHEMARK<dpA>";
		}
		if (karmaFlowerPosition.HasValue)
		{
			string text2 = karmaFlowerPosition.Value.ResolveRoomName();
			text += string.Format(CultureInfo.InvariantCulture, "FLOWERPOS<dpB>{0}.{1}.{2}.{3}<dpA>", (text2 != null) ? text2 : karmaFlowerPosition.Value.room.ToString(), karmaFlowerPosition.Value.x, karmaFlowerPosition.Value.y, karmaFlowerPosition.Value.abstractNode);
		}
		text += "GHOSTS<dpB>";
		bool flag = true;
		foreach (KeyValuePair<GhostWorldPresence.GhostID, int> item in ghostsTalkedTo)
		{
			text += string.Format(CultureInfo.InvariantCulture, flag ? "{0}:{1}" : ",{0}:{1}", item.Key, item.Value);
			flag = false;
		}
		foreach (string item2 in ghostsTalkedToUnrecognized)
		{
			text += (flag ? item2 : ("," + item2));
			flag = false;
		}
		text += "<dpA>";
		if (songsPlayRecords.Count > 0)
		{
			text += "SONGSPLAYRECORDS<dpB>";
			for (int i = 0; i < songsPlayRecords.Count; i++)
			{
				text += string.Format(CultureInfo.InvariantCulture, "{0}<dpD>{1}{2}", songsPlayRecords[i].songName, songsPlayRecords[i].cycleLastPlayed, (i < songsPlayRecords.Count - 1) ? "<dpC>" : "");
			}
			text += "<dpA>";
		}
		if (sessionTrackRecord.Count > 0)
		{
			text += "SESSIONRECORDS<dpB>";
			for (int j = 0; j < sessionTrackRecord.Count; j++)
			{
				text = text + sessionTrackRecord[j].ToString() + ((j < sessionTrackRecord.Count - 1) ? "<dpC>" : "");
			}
			text += "<dpA>";
		}
		string text3 = winState.SaveToString(saveAsIfPlayerDied);
		if (text3 != "")
		{
			text = text + "WINSTATE<dpB>" + text3 + "<dpA>";
		}
		if (consumedFlowers.Count > 0)
		{
			text += "CONSUMEDFLOWERS<dpB>";
			for (int k = 0; k < consumedFlowers.Count; k++)
			{
				text = text + consumedFlowers[k].ToString() + ((k < consumedFlowers.Count - 1) ? "<dpC>" : "");
			}
			text += "<dpA>";
		}
		text += "TUTMESSAGES<dpB>";
		for (int l = 0; l < tutorialMessages.Count; l++)
		{
			text += tutorialMessages[l];
			if (l < tutorialMessages.Count - 1)
			{
				text += ",";
			}
		}
		text += "<dpA>";
		text += "METERSSHOWN<dpB>";
		for (int m = 0; m < endGameMetersEverShown.Count; m++)
		{
			text += endGameMetersEverShown[m];
			if (m < endGameMetersEverShown.Count - 1)
			{
				text += ",";
			}
		}
		text += "<dpA>";
		if (foodReplenishBonus > 0)
		{
			text += string.Format(CultureInfo.InvariantCulture, "FOODREPBONUS<dpB>{0}<dpA>", foodReplenishBonus);
		}
		if (worldVersion > 0)
		{
			text += string.Format(CultureInfo.InvariantCulture, "DDWORLDVERSION<dpB>{0}<dpA>", worldVersion);
		}
		text += string.Format(CultureInfo.InvariantCulture, "DEATHS<dpB>{0}<dpA>", deaths);
		text += string.Format(CultureInfo.InvariantCulture, "SURVIVES<dpB>{0}<dpA>", survives);
		text += string.Format(CultureInfo.InvariantCulture, "QUITS<dpB>{0}<dpA>", quits + (saveAsIfPlayerQuit ? 1 : 0));
		if (pebblesHasIncreasedRedsKarmaCap)
		{
			text += "PHIRKC<dpA>";
		}
		if (unlockedGates != null && unlockedGates.Count > 0)
		{
			text += "UNLOCKEDGATES<dpB>";
			for (int n = 0; n < unlockedGates.Count; n++)
			{
				text = text + unlockedGates[n] + ((n < unlockedGates.Count - 1) ? "<dpC>" : "");
			}
			text += "<dpA>";
		}
		if (deathPositions.Count > 0)
		{
			text += "DEATHPOSS<dpB>";
			for (int num = 0; num < deathPositions.Count; num++)
			{
				text = text + deathPositions[num].SaveToString() + ((num < deathPositions.Count - 1) ? "<dpC>" : "");
			}
			text += "<dpA>";
		}
		if (ModManager.MSC)
		{
			if (altEnding)
			{
				text += "ALTENDING<dpA>";
			}
			if (ripPebbles)
			{
				text += "ZEROPEBBLES<dpA>";
			}
			if (ripMoon)
			{
				text += "LOOKSTOTHEDOOM<dpA>";
			}
			if (SLSiren)
			{
				text += "SLSIREN<dpA>";
			}
			text += string.Format(CultureInfo.InvariantCulture, "DEATHTIME<dpB>{0}<dpA>", deathTime);
			if (friendsSaved > 0)
			{
				text += string.Format(CultureInfo.InvariantCulture, "FRIENDSAVEBONUS<dpB>{0}<dpA>", friendsSaved);
			}
			text += "CHATLOGS<dpB>";
			for (int num2 = 0; num2 < chatlogsRead.Count; num2++)
			{
				text += chatlogsRead[num2];
				if (num2 < chatlogsRead.Count - 1)
				{
					text += ",";
				}
			}
			text += "<dpA>";
			text += "PREPEBCHATLOGS<dpB>";
			for (int num3 = 0; num3 < prePebChatlogsRead.Count; num3++)
			{
				text += prePebChatlogsRead[num3];
				if (num3 < prePebChatlogsRead.Count - 1)
				{
					text += ",";
				}
			}
			text += "<dpA>";
		}
		if (ModManager.MMF)
		{
			text += string.Format(CultureInfo.InvariantCulture, "TIPS<dpB>{0}<dpA>", tipCounter);
			text += string.Format(CultureInfo.InvariantCulture, "TIPSEED<dpB>{0}<dpA>", (tipSeed == 0) ? ((int)(UnityEngine.Random.value * 100f)) : tipSeed);
		}
		foreach (string unrecognizedSaveString in unrecognizedSaveStrings)
		{
			text = text + unrecognizedSaveString + "<dpA>";
		}
		return text;
	}

	public void FromString(string s)
	{
		Custom.Log("Loading death persistent data");
		fresh = false;
		redsDeath = false;
		ascended = false;
		pebblesHasIncreasedRedsKarmaCap = false;
		bool flag = false;
		unrecognizedSaveStrings.Clear();
		string[] array = Regex.Split(s, "<dpA>");
		for (int i = 0; i < array.Length; i++)
		{
			bool flag2 = false;
			string[] array2 = Regex.Split(array[i], "<dpB>");
			switch (array2[0])
			{
			case "KARMA":
				karma = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "KARMACAP":
				karmaCap = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "REINFORCEDKARMA":
				reinforcedKarma = array2[1] == "1";
				break;
			case "FLOWERPOS":
			{
				string[] array6 = array2[1].Split('.');
				int? num2 = BackwardsCompatibilityRemix.ParseRoomIndex(array6[0]);
				if (num2.HasValue)
				{
					karmaFlowerPosition = new WorldCoordinate(num2.Value, int.Parse(array6[1], NumberStyles.Any, CultureInfo.InvariantCulture), int.Parse(array6[2], NumberStyles.Any, CultureInfo.InvariantCulture), int.Parse(array6[3], NumberStyles.Any, CultureInfo.InvariantCulture));
				}
				else
				{
					karmaFlowerPosition = new WorldCoordinate(array6[0], int.Parse(array6[1], NumberStyles.Any, CultureInfo.InvariantCulture), int.Parse(array6[2], NumberStyles.Any, CultureInfo.InvariantCulture), int.Parse(array6[3], NumberStyles.Any, CultureInfo.InvariantCulture));
				}
				break;
			}
			case "GHOSTS":
			{
				if (array2[1].Contains("."))
				{
					BackwardsCompatibilityRemix.ParseGhostString(array2[1], ghostsTalkedTo);
					break;
				}
				ghostsTalkedTo.Clear();
				ghostsTalkedToUnrecognized.Clear();
				string[] array3 = array2[1].Split(',');
				foreach (string text in array3)
				{
					if (text == string.Empty)
					{
						continue;
					}
					if (text.Contains(":"))
					{
						string[] array4 = text.Split(':');
						try
						{
							ghostsTalkedTo[new GhostWorldPresence.GhostID(array4[0])] = int.Parse(array4[1], NumberStyles.Any, CultureInfo.InvariantCulture);
						}
						catch (ArgumentException)
						{
							ghostsTalkedToUnrecognized.Add(text);
						}
					}
					else
					{
						ghostsTalkedToUnrecognized.Add(text);
					}
				}
				break;
			}
			case "SONGSPLAYRECORDS":
			{
				string[] array8 = Regex.Split(array2[1], "<dpC>");
				for (int num = 0; num < array8.Length; num++)
				{
					if (!(array8[num] == string.Empty))
					{
						songsPlayRecords.Add(new SongPlayRecord(Regex.Split(array8[num], "<dpD>")[0], int.Parse(Regex.Split(array8[num], "<dpD>")[1], NumberStyles.Any, CultureInfo.InvariantCulture)));
					}
				}
				break;
			}
			case "SESSIONRECORDS":
			{
				string[] array7 = Regex.Split(array2[1], "<dpC>");
				for (int n = 0; n < array7.Length; n++)
				{
					if (!(array7[n] == string.Empty))
					{
						sessionTrackRecord.Add(SessionRecord.MakeFromString(array7[n]));
					}
				}
				Custom.Log("loaded", sessionTrackRecord.Count.ToString(), "session records");
				UpdateDynamicDifficulty();
				break;
			}
			case "WINSTATE":
				winState.FromString(array2[1]);
				break;
			case "CONSUMEDFLOWERS":
			{
				string[] array6 = Regex.Split(array2[1], "<dpC>");
				for (int l = 0; l < array6.Length; l++)
				{
					if (array6[l].Length > 0 && !(array6[l] == string.Empty))
					{
						RegionState.ConsumedItem consumedItem = new RegionState.ConsumedItem(0, 0, 0);
						consumedItem.FromString(array6[l]);
						consumedFlowers.Add(consumedItem);
					}
				}
				break;
			}
			case "HASTHEMARK":
				theMark = true;
				break;
			case "TUTMESSAGES":
			{
				if (Custom.IsDigitString(array2[1]))
				{
					BackwardsCompatibilityRemix.ParseTutorialMessages(array2[1], tutorialMessages);
					break;
				}
				tutorialMessages.Clear();
				string[] array3 = array2[1].Split(',');
				foreach (string text5 in array3)
				{
					if (!(text5 == string.Empty))
					{
						tutorialMessages.Add(new Tutorial(text5));
					}
				}
				break;
			}
			case "METERSSHOWN":
			{
				if (Custom.IsDigitString(array2[1]))
				{
					BackwardsCompatibilityRemix.ParseMetersShown(array2[1], endGameMetersEverShown);
					break;
				}
				endGameMetersEverShown.Clear();
				string[] array3 = array2[1].Split(',');
				foreach (string text4 in array3)
				{
					if (!(text4 == string.Empty))
					{
						endGameMetersEverShown.Add(new WinState.EndgameID(text4));
					}
				}
				break;
			}
			case "FOODREPBONUS":
				foodReplenishBonus = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "DDWORLDVERSION":
				worldVersion = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "DEATHS":
				deaths = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "SURVIVES":
				survives = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "QUITS":
				quits = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "DEATHPOSS":
			{
				deathPositions.Clear();
				string[] array6 = Regex.Split(array2[1], "<dpC>");
				for (int m = 0; m < array6.Length; m++)
				{
					if (!(array6[m] == string.Empty))
					{
						deathPositions.Add(WorldCoordinate.FromString(array6[m]));
					}
				}
				break;
			}
			case "REDSDEATH":
				redsDeath = true;
				break;
			case "ASCENDED":
				ascended = true;
				break;
			case "PHIRKC":
				pebblesHasIncreasedRedsKarmaCap = true;
				break;
			case "UNLOCKEDGATES":
			{
				if (unlockedGates == null)
				{
					unlockedGates = new List<string>();
				}
				string[] array5 = Regex.Split(array2[1], "<dpC>");
				for (int k = 0; k < array5.Length; k++)
				{
					if (!(array5[k] == string.Empty) && k < array5.Length)
					{
						unlockedGates.Add(array5[k]);
					}
				}
				break;
			}
			case "FRIENDSAVEBONUS":
				if (ModManager.MSC)
				{
					friendsSaved = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				}
				else
				{
					flag2 = true;
				}
				break;
			case "DEATHTIME":
				if (ModManager.MSC)
				{
					deathTime = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				}
				else
				{
					flag2 = true;
				}
				break;
			case "ALTENDING":
				if (ModManager.MSC)
				{
					altEnding = true;
				}
				else
				{
					flag2 = true;
				}
				break;
			case "ZEROPEBBLES":
				if (ModManager.MSC)
				{
					ripPebbles = true;
				}
				else
				{
					flag2 = true;
				}
				break;
			case "LOOKSTOTHEDOOM":
				if (ModManager.MSC)
				{
					ripMoon = true;
				}
				else
				{
					flag2 = true;
				}
				break;
			case "SLSIREN":
				if (ModManager.MSC)
				{
					SLSiren = true;
				}
				else
				{
					flag2 = true;
				}
				break;
			case "CHATLOGS":
				if (ModManager.MSC)
				{
					chatlogsRead.Clear();
					string[] array3 = array2[1].Split(',');
					foreach (string text3 in array3)
					{
						if (!(text3 == string.Empty))
						{
							ChatlogData.ChatlogID chatlogID2 = new ChatlogData.ChatlogID(text3);
							if (chatlogID2.Index >= 0)
							{
								chatlogsRead.Add(chatlogID2);
							}
						}
					}
				}
				else
				{
					flag2 = true;
				}
				break;
			case "PREPEBCHATLOGS":
				if (ModManager.MSC)
				{
					prePebChatlogsRead.Clear();
					string[] array3 = array2[1].Split(',');
					foreach (string text2 in array3)
					{
						if (!(text2 == string.Empty))
						{
							ChatlogData.ChatlogID chatlogID = new ChatlogData.ChatlogID(text2);
							if (chatlogID.Index >= 0)
							{
								prePebChatlogsRead.Add(chatlogID);
							}
						}
					}
					flag = true;
				}
				else
				{
					flag2 = true;
				}
				break;
			case "TIPS":
				if (ModManager.MMF)
				{
					tipCounter = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				}
				else
				{
					flag2 = true;
				}
				break;
			case "TIPSEED":
				if (ModManager.MMF)
				{
					tipSeed = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				}
				else
				{
					flag2 = true;
				}
				break;
			default:
				flag2 = true;
				break;
			}
			if (flag2 && array[i].Trim().Length > 0 && array2.Length >= 1)
			{
				unrecognizedSaveStrings.Add(array[i]);
			}
		}
		if (ModManager.MSC && !flag)
		{
			prePebChatlogsRead = new List<ChatlogData.ChatlogID>(chatlogsRead);
		}
		karma = Custom.IntClamp(karma, 0, karmaCap);
	}

	public void UpdateDynamicDifficulty()
	{
		howWellIsPlayerDoing = 0f;
		if (sessionTrackRecord.Count == 0)
		{
			return;
		}
		if (ModManager.MMF && MMF.cfgNewDynamicDifficulty != null && MMF.cfgNewDynamicDifficulty.Value)
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			for (int i = 0; i < sessionTrackRecord.Count; i++)
			{
				if (sessionTrackRecord[i].travelled)
				{
					num2++;
				}
				else
				{
					num3++;
				}
			}
			float min = Mathf.Lerp(-15f, -7f, Mathf.InverseLerp(0f, 20f, num2));
			float max = Mathf.Lerp(10f, 30f, Mathf.InverseLerp(0f, 12f, num2));
			for (int j = 0; j < sessionTrackRecord.Count; j++)
			{
				num += (sessionTrackRecord[j].survived ? 1 : (-1));
				num = (int)Mathf.Clamp(num, min, max);
			}
			Custom.Log("Dynamic difficulty updated");
			Custom.Log("Regions survived", num.ToString());
			Custom.Log("Regions travelled", num2.ToString());
			float fromA = Mathf.Lerp(-7f, -20f, Mathf.InverseLerp(5f, 15f, num2));
			float toA = Mathf.Lerp(12f, 7f, Mathf.InverseLerp(0f, 5f, num2));
			Custom.Log("Clampings,  E:", fromA.ToString(), "H:", toA.ToString());
			Custom.Log("weighings,  E:", min.ToString(), "H:", max.ToString());
			Custom.Log("POS", ((float)num).ToString());
			howWellIsPlayerDoing = Custom.LerpMap(num, fromA, toA, -1f, 1f);
			return;
		}
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		for (int k = 0; k < sessionTrackRecord.Count; k++)
		{
			num4 += (sessionTrackRecord[k].survived ? 1 : (-1));
			if (sessionTrackRecord[k].travelled)
			{
				num5++;
			}
			else
			{
				num6++;
			}
		}
		howWellIsPlayerDoing = Custom.LerpMap(num4, -7f, 7f, -1f, 1f);
	}

	public void ReportConsumedFlower(int originRoom, int placedObjectIndex, int waitCycles)
	{
		for (int num = consumedFlowers.Count - 1; num >= 0; num--)
		{
			if (consumedFlowers[num].originRoom == originRoom && consumedFlowers[num].placedObjectIndex == placedObjectIndex)
			{
				consumedFlowers.RemoveAt(num);
			}
		}
		consumedFlowers.Add(new RegionState.ConsumedItem(originRoom, placedObjectIndex, waitCycles));
	}

	public bool FlowerConsumed(int originRoom, int placedObjectIndex)
	{
		for (int num = consumedFlowers.Count - 1; num >= 0; num--)
		{
			if (consumedFlowers[num].originRoom == originRoom && consumedFlowers[num].placedObjectIndex == placedObjectIndex)
			{
				return true;
			}
		}
		return false;
	}

	public void TickFlowerDepletion(int ticks)
	{
		Custom.Log("ticking karma flower depletion by:", ticks.ToString());
		for (int num = consumedFlowers.Count - 1; num >= 0; num--)
		{
			if (consumedFlowers[num].waitCycles > -1)
			{
				consumedFlowers[num].waitCycles -= ticks;
				if (consumedFlowers[num].waitCycles < 1)
				{
					consumedFlowers.RemoveAt(num);
				}
			}
		}
	}

	public void AddDeathPosition(int room, Vector2 pos)
	{
		pos.y = Mathf.Max(-50f, pos.y);
		WorldCoordinate item = new WorldCoordinate(room, Mathf.RoundToInt(pos.x / 20f), Mathf.RoundToInt(pos.y / 20f), 0);
		deathPositions.Add(item);
	}
}
