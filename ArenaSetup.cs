using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using MoreSlugcats;
using RWCustom;

public class ArenaSetup
{
	public class GameTypeID : ExtEnum<GameTypeID>
	{
		public static readonly GameTypeID Competitive = new GameTypeID("Competitive", register: true);

		public static readonly GameTypeID Sandbox = new GameTypeID("Sandbox", register: true);

		public GameTypeID(string value, bool register = false)
			: base(value, register)
		{
		}

		public static void Init()
		{
			ExtEnum<GameTypeID>.values = new ExtEnumType();
			ExtEnum<GameTypeID>.values.AddEntry(Competitive.value);
			ExtEnum<GameTypeID>.values.AddEntry(Sandbox.value);
		}
	}

	public class GameTypeSetup
	{
		public class WildLifeSetting : ExtEnum<WildLifeSetting>
		{
			public static readonly WildLifeSetting Off = new WildLifeSetting("Off", register: true);

			public static readonly WildLifeSetting Low = new WildLifeSetting("Low", register: true);

			public static readonly WildLifeSetting Medium = new WildLifeSetting("Medium", register: true);

			public static readonly WildLifeSetting High = new WildLifeSetting("High", register: true);

			public WildLifeSetting(string value, bool register = false)
				: base(value, register)
			{
			}
		}

		public class DenEntryRule : ExtEnum<DenEntryRule>
		{
			public static readonly DenEntryRule Score = new DenEntryRule("Score", register: true);

			public static readonly DenEntryRule Standard = new DenEntryRule("Standard", register: true);

			public static readonly DenEntryRule Always = new DenEntryRule("Always", register: true);

			public DenEntryRule(string value, bool register = false)
				: base(value, register)
			{
			}
		}

		public GameTypeID gameType;

		public List<string> playList;

		public List<string> unrecognizedSaveStrings;

		public int[] ints;

		public int[] intDefaults;

		public int[] unrecognizedInts;

		public static float[] SessionTimesInMinutesArray = new float[6] { 0.5f, 1f, 1.5f, 2f, 3f, 10f };

		public static int[] ScoresToEnterDenArray = new int[6] { 1, 10, 20, 30, 50, 90 };

		public WildLifeSetting wildLifeSetting = WildLifeSetting.Medium;

		public DenEntryRule denEntryRule = DenEntryRule.Standard;

		public bool repeatSingleLevelForever;

		public bool savingAndLoadingSession;

		public int[] killScores = new int[ExtEnum<MultiplayerUnlocks.SandboxUnlockID>.values.Count];

		public bool customWinCondition;

		public int[] mscInts;

		public int[] mscIntDefaults;

		public int[] unrecognizedMSCInts;

		public ChallengeInformation.ChallengeMeta challengeMeta;

		public int levelRepeats
		{
			get
			{
				return ints[0];
			}
			set
			{
				ints[0] = value;
			}
		}

		public int allLevelsScroll
		{
			get
			{
				return ints[1];
			}
			set
			{
				ints[1] = value;
			}
		}

		public int playListScroll
		{
			get
			{
				return ints[2];
			}
			set
			{
				ints[2] = value;
			}
		}

		public bool shufflePlaylist
		{
			get
			{
				return ints[3] > 0;
			}
			set
			{
				ints[3] = (value ? 1 : 0);
			}
		}

		public bool spearsHitPlayers
		{
			get
			{
				return ints[5] > 0;
			}
			set
			{
				ints[5] = (value ? 1 : 0);
			}
		}

		public int sessionTimeLengthIndex
		{
			get
			{
				return ints[6];
			}
			set
			{
				ints[6] = value;
			}
		}

		public bool allLevelsThumbs
		{
			get
			{
				return ints[8] > 0;
			}
			set
			{
				ints[8] = (value ? 1 : 0);
			}
		}

		public bool playListThumbs
		{
			get
			{
				return ints[9] > 0;
			}
			set
			{
				ints[9] = (value ? 1 : 0);
			}
		}

		public bool evilAI
		{
			get
			{
				return ints[10] > 0;
			}
			set
			{
				ints[10] = (value ? 1 : 0);
			}
		}

		public int scoreToEnterDenIndex
		{
			get
			{
				return ints[12];
			}
			set
			{
				ints[12] = value;
			}
		}

		public int ScoreToEnterDen
		{
			get
			{
				if (denEntryRule != DenEntryRule.Score || scoreToEnterDenIndex < 0 || scoreToEnterDenIndex >= ScoresToEnterDenArray.Length)
				{
					return 0;
				}
				return ScoresToEnterDenArray[scoreToEnterDenIndex];
			}
		}

		public bool rainWhenOnePlayerLeft
		{
			get
			{
				return ints[13] > 0;
			}
			set
			{
				ints[13] = (value ? 1 : 0);
			}
		}

		public bool levelItems
		{
			get
			{
				return ints[14] > 0;
			}
			set
			{
				ints[14] = (value ? 1 : 0);
			}
		}

		public bool fliesSpawn
		{
			get
			{
				return ints[15] > 0;
			}
			set
			{
				ints[15] = (value ? 1 : 0);
			}
		}

		public int foodScore
		{
			get
			{
				return ints[16];
			}
			set
			{
				ints[16] = value;
			}
		}

		public int survivalScore
		{
			get
			{
				return ints[17];
			}
			set
			{
				ints[17] = value;
			}
		}

		public int spearHitScore
		{
			get
			{
				return ints[18];
			}
			set
			{
				ints[18] = value;
			}
		}

		public bool saveCreatures
		{
			get
			{
				return ints[19] > 0;
			}
			set
			{
				ints[19] = (value ? 1 : 0);
			}
		}

		public int challengeID
		{
			get
			{
				return mscInts[0];
			}
			set
			{
				mscInts[0] = value;
				challengeMeta = new ChallengeInformation.ChallengeMeta(value);
			}
		}

		public int safariID
		{
			get
			{
				return mscInts[1];
			}
			set
			{
				mscInts[1] = value;
			}
		}

		public int safariSlugcatID
		{
			get
			{
				return mscInts[2];
			}
			set
			{
				mscInts[2] = value;
			}
		}

		public int safariRainDisabled
		{
			get
			{
				return mscInts[3];
			}
			set
			{
				mscInts[3] = value;
			}
		}

		public GameTypeSetup()
		{
			unrecognizedSaveStrings = new List<string>();
			playList = new List<string>();
			ints = new int[20];
			mscInts = new int[4];
			wildLifeSetting = WildLifeSetting.Off;
			denEntryRule = DenEntryRule.Score;
			unrecognizedInts = null;
			unrecognizedMSCInts = null;
		}

		public void InitAsGameType(GameTypeID gameType)
		{
			this.gameType = gameType;
			intDefaults = new int[ints.Length];
			intDefaults[0] = 1;
			intDefaults[5] = 1;
			intDefaults[6] = 4;
			wildLifeSetting = WildLifeSetting.Medium;
			intDefaults[7] = wildLifeSetting.Index;
			intDefaults[8] = 1;
			intDefaults[9] = 1;
			intDefaults[10] = 0;
			denEntryRule = DenEntryRule.Standard;
			intDefaults[11] = denEntryRule.Index;
			intDefaults[13] = 1;
			intDefaults[14] = ((gameType == GameTypeID.Competitive) ? 1 : 0);
			intDefaults[15] = ((gameType == GameTypeID.Competitive) ? 1 : 0);
			intDefaults[16] = 1;
			intDefaults[19] = ((gameType == GameTypeID.Competitive) ? 1 : 0);
			for (int i = 0; i < ints.Length; i++)
			{
				ints[i] = intDefaults[i];
			}
			if (ModManager.MSC)
			{
				mscIntDefaults = new int[mscInts.Length];
				mscIntDefaults[0] = 1;
				mscIntDefaults[1] = 0;
				mscIntDefaults[2] = 0;
				mscIntDefaults[3] = 0;
				for (int j = 0; j < mscInts.Length; j++)
				{
					mscInts[j] = mscIntDefaults[j];
				}
			}
			if (ModManager.MSC && gameType == MoreSlugcatsEnums.GameTypeID.Challenge)
			{
				challengeMeta = new ChallengeInformation.ChallengeMeta(challengeID);
			}
			else if (gameType == GameTypeID.Competitive)
			{
				foodScore = 1;
				survivalScore = 0;
				spearHitScore = 0;
				repeatSingleLevelForever = false;
				savingAndLoadingSession = true;
				denEntryRule = DenEntryRule.Standard;
				rainWhenOnePlayerLeft = true;
				levelItems = true;
				fliesSpawn = true;
				saveCreatures = true;
			}
			if (gameType == GameTypeID.Sandbox || (ModManager.MSC && gameType == MoreSlugcatsEnums.GameTypeID.Challenge))
			{
				playListThumbs = true;
				repeatSingleLevelForever = true;
				savingAndLoadingSession = false;
				wildLifeSetting = WildLifeSetting.Medium;
			}
		}

		public void UpdateCustomWinCondition()
		{
			customWinCondition = false;
			if (gameType != GameTypeID.Sandbox)
			{
				return;
			}
			if (Math.Abs(foodScore) > 99)
			{
				customWinCondition = true;
				return;
			}
			if (Math.Abs(foodScore) > 99)
			{
				customWinCondition = true;
				return;
			}
			for (int i = 0; i < killScores.Length; i++)
			{
				if (Math.Abs(killScores[i]) > 99)
				{
					customWinCondition = true;
					break;
				}
			}
		}

		public override string ToString()
		{
			string text = "";
			bool flag = false;
			text = text + "NAME<gmtB>" + gameType.ToString() + "<gmtA>";
			if (playList.Count > 0)
			{
				text += "PLAYLIST<gmtB>";
				for (int i = 0; i < playList.Count; i++)
				{
					text = text + playList[i] + ((i < playList.Count - 1) ? "<gmtC>" : "");
				}
				text += "<gmtA>";
				flag = true;
			}
			bool flag2 = false;
			for (int j = 0; j < ints.Length; j++)
			{
				if (ints[j] != intDefaults[j])
				{
					flag2 = true;
					break;
				}
			}
			ints[7] = -1;
			ints[11] = -1;
			if (flag2)
			{
				text += "INTEGERS<gmtB>";
				text += SaveUtils.SaveIntegerArray('.', ints, unrecognizedInts);
				text += "<gmtA>";
				flag = true;
			}
			if (ModManager.MSC)
			{
				bool flag3 = false;
				for (int k = 0; k < mscInts.Length; k++)
				{
					if (mscInts[k] != mscIntDefaults[k])
					{
						flag3 = true;
						break;
					}
				}
				if (flag3)
				{
					text += "MSCINTEGERS<gmtB>";
					text += SaveUtils.SaveIntegerArray('.', mscInts, unrecognizedMSCInts);
					text += "<gmtA>";
					flag = true;
				}
			}
			text += "WILDLIFE<gmtB>";
			text += wildLifeSetting;
			text += "<gmtA>";
			text += "DENENTRY<gmtB>";
			text += denEntryRule;
			text += "<gmtA>";
			if (gameType == GameTypeID.Sandbox)
			{
				text += "KILLSCORES<gmtB>";
				for (int l = 0; l < killScores.Length; l++)
				{
					text += string.Format(CultureInfo.InvariantCulture, "{0}{1}", killScores[l], (l < killScores.Length - 1) ? "." : "");
					if (killScores[l] > 0)
					{
						flag = true;
					}
				}
				text += "<gmtA>";
			}
			foreach (string unrecognizedSaveString in unrecognizedSaveStrings)
			{
				text = text + unrecognizedSaveString + "<gmtA>";
			}
			if (flag)
			{
				return text;
			}
			return null;
		}

		public static bool LevelExists(string levelName)
		{
			return File.Exists(AssetManager.ResolveFilePath("Levels" + Path.DirectorySeparatorChar + levelName + ".txt"));
		}

		public void FromString(string s)
		{
			string[] array = Regex.Split(s, "<gmtA>");
			unrecognizedSaveStrings.Clear();
			for (int i = 0; i < array.Length; i++)
			{
				bool flag = false;
				string[] array2 = Regex.Split(array[i], "<gmtB>");
				switch (array2[0])
				{
				case "NAME":
					InitAsGameType(new GameTypeID(array2[1]));
					break;
				case "PLAYLIST":
				{
					string[] array3 = Regex.Split(array2[1], "<gmtC>");
					for (int k = 0; k < array3.Length; k++)
					{
						if (LevelExists(array3[k]))
						{
							playList.Add(array3[k]);
						}
					}
					if (playList.Count == 0)
					{
						playList.Add("thepit");
					}
					break;
				}
				case "INTEGERS":
					unrecognizedInts = SaveUtils.LoadIntegersArray(array2[1], '.', ints);
					if (ints[7] != -1)
					{
						wildLifeSetting = BackwardsCompatibilityRemix.ParseWildLifeSetting(ints[7]);
					}
					if (ints[11] != -1)
					{
						denEntryRule = BackwardsCompatibilityRemix.ParseDenEntryRule(ints[11]);
					}
					break;
				case "MSCINTEGERS":
					if (ModManager.MSC)
					{
						unrecognizedMSCInts = SaveUtils.LoadIntegersArray(array2[1], '.', mscInts);
					}
					else
					{
						flag = true;
					}
					break;
				case "KILLSCORES":
				{
					string[] array3 = array2[1].Split('.');
					for (int j = 0; j < array3.Length && j < killScores.Length; j++)
					{
						killScores[j] = int.Parse(array3[j], NumberStyles.Any, CultureInfo.InvariantCulture);
					}
					break;
				}
				case "WILDLIFE":
					wildLifeSetting = new WildLifeSetting(array2[1]);
					break;
				case "DENENTRY":
					denEntryRule = new DenEntryRule(array2[1]);
					break;
				default:
					flag = true;
					break;
				}
				if (flag && array[i].Trim().Length > 0 && array2.Length >= 2)
				{
					unrecognizedSaveStrings.Add(array[i]);
				}
			}
			if (ModManager.MSC && gameType == MoreSlugcatsEnums.GameTypeID.Challenge)
			{
				challengeMeta = new ChallengeInformation.ChallengeMeta(challengeID);
			}
		}
	}

	private static readonly AGLog<ArenaSetup> Log = new AGLog<ArenaSetup>();

	public const string ARENA_SETUP_KEY = "ArenaSetup";

	public List<GameTypeSetup> gametypeSetups;

	public GameTypeID currentGameType = GameTypeID.Competitive;

	private string savFilePath;

	public bool[] playersJoined;

	public bool[] scrolledToShowNewLevels;

	private ProcessManager manager;

	public SlugcatStats.Name[] playerClass;

	public ArenaSetup(ProcessManager manager)
	{
		this.manager = manager;
		gametypeSetups = new List<GameTypeSetup>();
		savFilePath = Custom.RootFolderDirectory() + (Path.DirectorySeparatorChar + "UserData" + Path.DirectorySeparatorChar + "arenaSetup.txt").ToLowerInvariant();
		playersJoined = new bool[4];
		playersJoined[0] = true;
		scrolledToShowNewLevels = new bool[ExtEnum<GameTypeID>.values.Count];
		if (ModManager.MSC)
		{
			playerClass = new SlugcatStats.Name[4];
			for (int i = 0; i < playerClass.Length; i++)
			{
				playerClass[i] = SlugcatStats.Name.White;
			}
		}
		else
		{
			playerClass = new SlugcatStats.Name[0];
		}
		LoadFromFile();
	}

	public GameTypeID CycleGameType(int dir)
	{
		int num = (int)currentGameType;
		num += dir;
		if (num < 0)
		{
			num = ExtEnum<GameTypeID>.values.Count - 1;
		}
		else if (num >= ExtEnum<GameTypeID>.values.Count)
		{
			num = 0;
		}
		return new GameTypeID(ExtEnum<GameTypeID>.values.GetEntry(num));
	}

	public GameTypeSetup GetOrInitiateGameTypeSetup(GameTypeID gameType)
	{
		for (int i = 0; i < gametypeSetups.Count; i++)
		{
			if (gametypeSetups[i].gameType == gameType)
			{
				return gametypeSetups[i];
			}
		}
		gametypeSetups.Add(new GameTypeSetup());
		gametypeSetups[gametypeSetups.Count - 1].InitAsGameType(gameType);
		return gametypeSetups[gametypeSetups.Count - 1];
	}

	public void SaveToFile()
	{
		string text = "";
		bool flag = false;
		text += string.Format(CultureInfo.InvariantCulture, "CURRGAMETYPE<msuB>{0}<msuA>", currentGameType.value);
		for (int i = 0; i < playersJoined.Length; i++)
		{
			if (playersJoined[i] == (i == 0))
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			text += "PLAYERS<msuB>";
			for (int j = 0; j < playersJoined.Length; j++)
			{
				text += (playersJoined[j] ? "1" : "0");
			}
			text += "<msuA>";
		}
		for (int k = 0; k < gametypeSetups.Count; k++)
		{
			string text2 = gametypeSetups[k].ToString();
			if (text2 != null)
			{
				text = text + "GAMETYPE<msuB>" + text2 + "<msuA>";
				flag = true;
			}
		}
		if (ModManager.MSC)
		{
			text += "CLASSES<msuB>";
			for (int l = 0; l < playerClass.Length; l++)
			{
				text = ((!(playerClass[l] == null)) ? (text + playerClass[l].ToString()) : (text + "NULL"));
				if (l < playerClass.Length - 1)
				{
					text += ",";
				}
			}
			text += "<msuA>";
		}
		if (flag)
		{
			manager.rainWorld.options.SaveArenaSetup(text);
		}
	}

	public void LoadFromFile()
	{
		string text = manager.rainWorld.options.LoadArenaSetup(savFilePath);
		if (text == null)
		{
			return;
		}
		string[] array = Regex.Split(text, "<msuA>");
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = Regex.Split(array[i], "<msuB>");
			switch (array2[0])
			{
			case "CURRGAMETYPE":
			{
				if (int.TryParse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
				{
					currentGameType = ((result == 0) ? GameTypeID.Competitive : GameTypeID.Sandbox);
					break;
				}
				currentGameType = new GameTypeID(array2[1]);
				if (currentGameType.Index == -1)
				{
					currentGameType = GameTypeID.Competitive;
				}
				break;
			}
			case "PLAYERS":
			{
				for (int k = 0; k < array2[1].Length && k < playersJoined.Length; k++)
				{
					playersJoined[k] = array2[1][k] == '1';
				}
				break;
			}
			case "GAMETYPE":
				gametypeSetups.Add(new GameTypeSetup());
				gametypeSetups[gametypeSetups.Count - 1].FromString(array2[1]);
				break;
			case "CLASSES":
			{
				if (!ModManager.MSC)
				{
					break;
				}
				int j = 0;
				if (!array2[1].Contains(","))
				{
					for (; j < array2[1].Length && j < playerClass.Length; j++)
					{
						playerClass[j] = new SlugcatStats.Name(array2[1][j].ToString());
					}
					break;
				}
				for (string[] array3 = Regex.Split(array2[1], ","); j < array3.Length && j < playerClass.Length; j++)
				{
					if (array3[j].ToString() == "NULL")
					{
						playerClass[j] = null;
					}
					else
					{
						playerClass[j] = new SlugcatStats.Name(array3[j].ToString());
					}
				}
				break;
			}
			}
		}
	}
}
