using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Menu;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class ArenaSitting
{
	public class ArenaPlayer
	{
		public int playerNumber;

		public int wins;

		public int deaths;

		public int totScore;

		public List<IconSymbol.IconSymbolData> allKills;

		public int score;

		public int timeAlive;

		public int randomPlacement;

		public int sandboxWin;

		public bool alive;

		public bool readyForNextRound;

		public bool winner;

		public bool hasEnteredGameArea;

		public List<IconSymbol.IconSymbolData> roundKills;

		public List<string> unrecognizedSaveStrings;

		public SlugcatStats.Name playerClass;

		public int parries;

		public ArenaPlayer(int playerNumber)
		{
			this.playerNumber = playerNumber;
			allKills = new List<IconSymbol.IconSymbolData>();
			roundKills = new List<IconSymbol.IconSymbolData>();
			unrecognizedSaveStrings = new List<string>();
			Reset();
		}

		public void AddSandboxScore(int scoreAdd)
		{
			if (Math.Abs(scoreAdd) > 99)
			{
				if (sandboxWin == 0)
				{
					sandboxWin = Math.Sign(scoreAdd);
				}
			}
			else
			{
				score += scoreAdd;
			}
		}

		public void Reset()
		{
			score = 0;
			timeAlive = 0;
			alive = true;
			readyForNextRound = false;
			winner = false;
			hasEnteredGameArea = false;
			roundKills.Clear();
			randomPlacement = UnityEngine.Random.Range(0, 10000);
			sandboxWin = 0;
			parries = 0;
		}

		public void SortAllKills()
		{
			List<IconSymbol.IconSymbolData> list = new List<IconSymbol.IconSymbolData>();
			for (int i = 0; i < allKills.Count; i++)
			{
				IconSymbol.IconSymbolData iconSymbolData = allKills[i];
				bool flag = false;
				for (int j = 0; j < list.Count; j++)
				{
					if (KillSortValue(iconSymbolData) < KillSortValue(list[j]))
					{
						list.Insert(j, iconSymbolData);
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					list.Add(iconSymbolData);
				}
			}
			allKills = list;
		}

		private int KillSortValue(IconSymbol.IconSymbolData symbolData)
		{
			int num = (int)symbolData.critType * 100 + symbolData.intData;
			if (symbolData.critType == CreatureTemplate.Type.Slugcat)
			{
				num -= 1000;
			}
			return num;
		}

		public override string ToString()
		{
			string text = "";
			text += string.Format(CultureInfo.InvariantCulture, "playerNumber<sbpB>{0}<sbpA>", playerNumber);
			text += string.Format(CultureInfo.InvariantCulture, "wins<sbpB>{0}<sbpA>", wins);
			text += string.Format(CultureInfo.InvariantCulture, "deaths<sbpB>{0}<sbpA>", deaths);
			text += string.Format(CultureInfo.InvariantCulture, "totScore<sbpB>{0}<sbpA>", totScore);
			if (allKills.Count > 0)
			{
				text += "playerKills<sbpB>";
				for (int i = 0; i < allKills.Count; i++)
				{
					text = text + allKills[i].ToString() + ((i < allKills.Count - 1) ? "." : "");
				}
				text += "<sbpA>";
			}
			if (ModManager.MSC)
			{
				text += string.Format(CultureInfo.InvariantCulture, "playerClass<sbpB>{0}<sbpA>", playerClass.value);
			}
			foreach (string unrecognizedSaveString in unrecognizedSaveStrings)
			{
				text = text + unrecognizedSaveString + "<sbpA>";
			}
			return text;
		}

		public void LoadFromString(string inpt)
		{
			string[] array = Regex.Split(inpt, "<sbpA>");
			allKills.Clear();
			unrecognizedSaveStrings.Clear();
			for (int i = 0; i < array.Length; i++)
			{
				bool flag = false;
				string[] array2 = Regex.Split(array[i], "<sbpB>");
				if (array2.Length > 1)
				{
					switch (array2[0])
					{
					case "playerNumber":
						playerNumber = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
						break;
					case "wins":
						wins = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
						break;
					case "deaths":
						deaths = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
						break;
					case "totScore":
						totScore = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
						break;
					case "playerKills":
					{
						string[] array3 = array2[1].Split('.');
						for (int j = 0; j < array3.Length; j++)
						{
							if (array3[j].Length > 2)
							{
								allKills.Add(IconSymbol.IconSymbolData.IconSymbolDataFromString(array3[j]));
							}
						}
						break;
					}
					case "playerClass":
						if (ModManager.MSC)
						{
							playerClass = new SlugcatStats.Name(array2[1]);
						}
						else
						{
							flag = true;
						}
						break;
					default:
						flag = true;
						break;
					}
				}
				if (flag && array[i].Trim().Length > 0 && array2.Length >= 2)
				{
					unrecognizedSaveStrings.Add(array[i]);
				}
			}
		}
	}

	private static readonly AGLog<ArenaSitting> Log = new AGLog<ArenaSitting>();

	public const string ARENA_SITTING_KEY = "ArenaSitting";

	public List<string> levelPlaylist;

	public int currentLevel;

	public List<ArenaPlayer> players;

	public CreatureCommunities savCommunities;

	public ArenaSetup.GameTypeSetup gameTypeSetup;

	public List<AbstractCreature> creatures;

	public bool attempLoadInGame;

	public bool firstGameAfterMenu;

	public MultiplayerUnlocks multiplayerUnlocks;

	public bool sandboxPlayMode;

	public bool ReadyToStart
	{
		get
		{
			if (ModManager.MSC && gameTypeSetup.gameType == MoreSlugcatsEnums.GameTypeID.Challenge)
			{
				return true;
			}
			if (gameTypeSetup.gameType == ArenaSetup.GameTypeID.Sandbox || players.Count > 0)
			{
				return levelPlaylist.Count > 0;
			}
			return false;
		}
	}

	public string GetCurrentLevel
	{
		get
		{
			if (!gameTypeSetup.repeatSingleLevelForever)
			{
				return levelPlaylist[currentLevel];
			}
			return levelPlaylist[0];
		}
	}

	public bool ShowLevelName
	{
		get
		{
			if (gameTypeSetup.repeatSingleLevelForever)
			{
				return firstGameAfterMenu;
			}
			if (!firstGameAfterMenu && currentLevel >= 1)
			{
				return levelPlaylist[currentLevel - 1] != levelPlaylist[currentLevel];
			}
			return true;
		}
	}

	public ArenaSitting(ArenaSetup.GameTypeSetup gameTypeSetup, MultiplayerUnlocks multiplayerUnlocks)
	{
		this.gameTypeSetup = gameTypeSetup;
		this.multiplayerUnlocks = multiplayerUnlocks;
		levelPlaylist = new List<string>();
		players = new List<ArenaPlayer>();
		creatures = new List<AbstractCreature>();
		firstGameAfterMenu = true;
	}

	public void AddPlayer(int playerNumber)
	{
		for (int num = players.Count - 1; num >= 0; num--)
		{
			if (players[num].playerNumber == playerNumber)
			{
				players.RemoveAt(num);
			}
		}
		players.Add(new ArenaPlayer(playerNumber));
	}

	public void AddPlayerWithClass(int playerNumber, SlugcatStats.Name playerClass)
	{
		for (int num = players.Count - 1; num >= 0; num--)
		{
			if (players[num].playerNumber == playerNumber)
			{
				players.RemoveAt(num);
			}
		}
		ArenaPlayer arenaPlayer = new ArenaPlayer(playerNumber);
		arenaPlayer.playerClass = playerClass;
		players.Add(arenaPlayer);
	}

	public void SessionEnded(ArenaGameSession session)
	{
		int num = 0;
		for (int i = 0; i < players.Count; i++)
		{
			for (int j = 0; j < session.Players.Count; j++)
			{
				if (players[i].playerNumber != (session.Players[j].state as PlayerState).playerNumber)
				{
					continue;
				}
				players[i].alive = session.EndOfSessionLogPlayerAsAlive(players[i].playerNumber);
				if (players[i].alive)
				{
					players[i].AddSandboxScore(gameTypeSetup.survivalScore);
				}
				if (gameTypeSetup.foodScore != 0 && Math.Abs(gameTypeSetup.foodScore) < 100)
				{
					players[i].score += (session.Players[j].state as PlayerState).foodInStomach * gameTypeSetup.foodScore;
					if (session.Players[j].realizedCreature != null)
					{
						for (int k = 0; k < session.Players[j].realizedCreature.grasps.Length; k++)
						{
							if (session.Players[j].realizedCreature.grasps[k] != null && session.Players[j].realizedCreature.grasps[k].grabbed is IPlayerEdible)
							{
								players[i].score += (session.Players[j].realizedCreature.grasps[k].grabbed as IPlayerEdible).FoodPoints * gameTypeSetup.foodScore;
							}
						}
					}
				}
				players[i].score += 100 * players[i].sandboxWin;
				num += players[i].score;
			}
		}
		List<ArenaPlayer> list = new List<ArenaPlayer>();
		if (ModManager.MSC && gameTypeSetup.challengeMeta != null)
		{
			players[0].score = num;
			List<IconSymbol.IconSymbolData> list2 = new List<IconSymbol.IconSymbolData>();
			for (int l = 0; l < players.Count; l++)
			{
				foreach (IconSymbol.IconSymbolData roundKill in players[l].roundKills)
				{
					if (roundKill.critType != CreatureTemplate.Type.Slugcat)
					{
						list2.Add(roundKill);
					}
				}
				players[l].roundKills.Clear();
			}
			players[0].roundKills = list2;
			list.Add(players[0]);
		}
		else
		{
			for (int m = 0; m < players.Count; m++)
			{
				ArenaPlayer arenaPlayer = players[m];
				bool flag = false;
				for (int n = 0; n < list.Count; n++)
				{
					if (PlayerSessionResultSort(arenaPlayer, list[n]))
					{
						list.Insert(n, arenaPlayer);
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					list.Add(arenaPlayer);
				}
			}
		}
		if (gameTypeSetup.gameType == ArenaSetup.GameTypeID.Competitive)
		{
			if (list.Count == 1)
			{
				list[0].winner = list[0].alive;
			}
			else if (list.Count > 1)
			{
				if (list[0].alive && !list[1].alive)
				{
					list[0].winner = true;
				}
				else if (list[0].score > list[1].score)
				{
					list[0].winner = true;
				}
			}
		}
		else if (gameTypeSetup.gameType == ArenaSetup.GameTypeID.Sandbox)
		{
			if (list.Count == 1)
			{
				if (gameTypeSetup.customWinCondition)
				{
					list[0].winner = list[0].sandboxWin > 0;
				}
				else
				{
					list[0].winner = list[0].alive;
				}
			}
			else if (list.Count > 1)
			{
				if (list[0].sandboxWin > list[1].sandboxWin)
				{
					list[0].winner = true;
				}
				else if (list[0].score > list[1].score)
				{
					list[0].winner = true;
				}
				else if (list[0].alive && !list[1].alive)
				{
					list[0].winner = true;
				}
			}
		}
		for (int num2 = 0; num2 < list.Count; num2++)
		{
			if (list[num2].winner)
			{
				list[num2].wins++;
			}
			if (!players[num2].alive)
			{
				players[num2].deaths++;
			}
			players[num2].totScore += players[num2].score;
		}
		session.game.arenaOverlay = new ArenaOverlay(session.game.manager, this, list);
		session.game.manager.sideProcesses.Add(session.game.arenaOverlay);
	}

	public List<ArenaPlayer> FinalSittingResult()
	{
		List<ArenaPlayer> list = new List<ArenaPlayer>();
		for (int i = 0; i < players.Count; i++)
		{
			ArenaPlayer arenaPlayer = players[i];
			arenaPlayer.Reset();
			arenaPlayer.SortAllKills();
			bool flag = false;
			for (int j = 0; j < list.Count; j++)
			{
				if (PlayerSittingResultSort(arenaPlayer, list[j]))
				{
					list.Insert(j, arenaPlayer);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				list.Add(arenaPlayer);
			}
		}
		if (list.Count > 1 && list[0].wins > list[1].wins)
		{
			list[0].winner = true;
		}
		return list;
	}

	public void SessionStartReset()
	{
		for (int i = 0; i < players.Count; i++)
		{
			players[i].Reset();
		}
		if (ModManager.MSC)
		{
			Scavenger.ArenaScavID = 0;
		}
	}

	public void NextLevel(ProcessManager manager)
	{
		if (manager.currentMainLoop is RainWorldGame)
		{
			ArenaGameSession getArenaGameSession = (manager.currentMainLoop as RainWorldGame).GetArenaGameSession;
			if (gameTypeSetup.saveCreatures)
			{
				for (int i = 0; i < getArenaGameSession.game.world.NumberOfRooms; i++)
				{
					for (int j = 0; j < getArenaGameSession.game.world.GetAbstractRoom(getArenaGameSession.game.world.firstRoomIndex + i).creatures.Count; j++)
					{
						if (getArenaGameSession.game.world.GetAbstractRoom(getArenaGameSession.game.world.firstRoomIndex + i).creatures[j].state.alive)
						{
							creatures.Add(getArenaGameSession.game.world.GetAbstractRoom(getArenaGameSession.game.world.firstRoomIndex + i).creatures[j]);
						}
					}
					for (int k = 0; k < getArenaGameSession.game.world.GetAbstractRoom(getArenaGameSession.game.world.firstRoomIndex + i).entitiesInDens.Count; k++)
					{
						if (getArenaGameSession.game.world.GetAbstractRoom(getArenaGameSession.game.world.firstRoomIndex + i).entitiesInDens[k] is AbstractCreature && (getArenaGameSession.game.world.GetAbstractRoom(getArenaGameSession.game.world.firstRoomIndex + i).entitiesInDens[k] as AbstractCreature).state.alive)
						{
							creatures.Add(getArenaGameSession.game.world.GetAbstractRoom(getArenaGameSession.game.world.firstRoomIndex + i).entitiesInDens[k] as AbstractCreature);
						}
					}
				}
				savCommunities = getArenaGameSession.creatureCommunities;
				savCommunities.session = null;
			}
			else
			{
				creatures.Clear();
				savCommunities = null;
			}
			firstGameAfterMenu = false;
			if (ModManager.MSC && getArenaGameSession.challengeCompleted)
			{
				manager.RequestMainProcessSwitch(ProcessManager.ProcessID.MultiplayerMenu);
				return;
			}
		}
		currentLevel++;
		if (currentLevel >= levelPlaylist.Count && !gameTypeSetup.repeatSingleLevelForever)
		{
			manager.RequestMainProcessSwitch(ProcessManager.ProcessID.MultiplayerResults);
			return;
		}
		manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Game);
		if (gameTypeSetup.savingAndLoadingSession)
		{
			SaveToFile(manager.rainWorld);
		}
	}

	private bool PlayerSessionResultSort(ArenaPlayer A, ArenaPlayer B)
	{
		if (gameTypeSetup.gameType == ArenaSetup.GameTypeID.Competitive)
		{
			if (A.alive != B.alive)
			{
				return A.alive;
			}
			if (A.score != B.score)
			{
				return A.score > B.score;
			}
		}
		else if (gameTypeSetup.gameType == ArenaSetup.GameTypeID.Sandbox)
		{
			if (A.sandboxWin != B.sandboxWin)
			{
				return A.sandboxWin > B.sandboxWin;
			}
			if (A.score != B.score)
			{
				return A.score > B.score;
			}
			if (A.alive != B.alive)
			{
				return A.alive;
			}
		}
		if (A.roundKills.Count != B.roundKills.Count)
		{
			return A.roundKills.Count > B.roundKills.Count;
		}
		return A.timeAlive > B.timeAlive;
	}

	private bool PlayerSittingResultSort(ArenaPlayer A, ArenaPlayer B)
	{
		if (A.wins != B.wins)
		{
			return A.wins > B.wins;
		}
		if (A.totScore != B.totScore)
		{
			return A.totScore > B.totScore;
		}
		if (A.deaths != B.deaths)
		{
			return A.deaths < B.deaths;
		}
		return A.allKills.Count > B.allKills.Count;
	}

	public void LoadFromFile(ArenaGameSession session, World world, RainWorld rainWorld)
	{
		if (!rainWorld.options.ContainsArenaSitting())
		{
			return;
		}
		Custom.Log("loading sitting from file");
		string[] array = Regex.Split(rainWorld.options.LoadArenaSitting(), "<ssA>");
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].Length <= 0)
			{
				continue;
			}
			string[] array2 = Regex.Split(array[i], "<ssB>");
			if (array2.Length <= 1)
			{
				continue;
			}
			switch (array2[0])
			{
			case "CURRLEV":
				currentLevel = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "PLAYLIST":
			{
				levelPlaylist.Clear();
				string[] array3 = Regex.Split(array2[1], "<ssC>");
				for (int l = 0; l < array3.Length; l++)
				{
					levelPlaylist.Add(array3[l]);
				}
				break;
			}
			case "PLAYER":
			{
				for (int k = 0; k < players.Count; k++)
				{
					if (players[k].playerNumber == int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture))
					{
						players[k].LoadFromString(array2[2]);
						break;
					}
				}
				break;
			}
			case "COMMUNITIES":
				if (session != null)
				{
					savCommunities = session.creatureCommunities;
					savCommunities.FromString(array2[1]);
				}
				break;
			case "CREATURES":
			{
				if (world == null)
				{
					break;
				}
				creatures.Clear();
				string[] array3 = Regex.Split(array2[1], "<ssC>");
				for (int j = 0; j < array3.Length; j++)
				{
					AbstractCreature abstractCreature = SaveState.AbstractCreatureFromString(world, array3[j], onlyInCurrentRegion: false);
					if (abstractCreature != null)
					{
						creatures.Add(abstractCreature);
					}
				}
				break;
			}
			}
		}
	}

	public void SaveToFile(RainWorld rainWorld)
	{
		string text = "";
		text += string.Format(CultureInfo.InvariantCulture, "CURRLEV<ssB>{0}<ssA>", currentLevel);
		text += "PLAYLIST<ssB>";
		for (int i = 0; i < levelPlaylist.Count; i++)
		{
			text = text + levelPlaylist[i] + ((i < levelPlaylist.Count - 1) ? "<ssC>" : "");
		}
		text += "<ssA>";
		for (int j = 0; j < players.Count; j++)
		{
			text += string.Format(CultureInfo.InvariantCulture, "PLAYER<ssB>{0}<ssB>{1}<ssA>", players[j].playerNumber, players[j].ToString());
		}
		if (savCommunities != null)
		{
			text = text + "COMMUNITIES<ssB>" + savCommunities.ToString() + "<ssA>";
		}
		text += "CREATURES<ssB>";
		for (int k = 0; k < creatures.Count; k++)
		{
			text = text + SaveState.AbstractCreatureToStringSingleRoomWorld(creatures[k]) + ((k < creatures.Count - 1) ? "<ssC>" : "");
		}
		text += "<ssA>";
		rainWorld.options.SaveArenaSitting(text);
	}
}
