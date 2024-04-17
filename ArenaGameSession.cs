using System;
using System.Collections.Generic;
using ArenaBehaviors;
using HUD;
using Menu;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public abstract class ArenaGameSession : GameSession, IOwnAHUD
{
	public class PlayerStopController : Player.PlayerController
	{
		public override Player.InputPackage GetInput()
		{
			return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, 0, 0, jmp: false, thrw: false, pckp: false, mp: false, crouchToggle: false);
		}
	}

	private static readonly AGLog<ArenaGameSession> Log = new AGLog<ArenaGameSession>();

	public ArenaSitting arenaSitting;

	public float rainCycleTimeInMinutes;

	public int endSessionCounter = -1;

	public bool sessionEnded;

	public bool initiated;

	public bool playersSpawned;

	public int thisFrameActivePlayers;

	public bool playersGlowing;

	public List<ArenaGameBehavior> behaviors = new List<ArenaGameBehavior>();

	public EarlyRain earlyRain;

	public RespawnFlies respawnFlies;

	public ExitManager exitManager;

	public NoRain noRain;

	public int counter;

	public bool outsidePlayersCountAsDead = true;

	public SlugcatStats[] characterStats_Mplayer;

	public ChallengeInformation.ChallengeMeta chMeta;

	public bool challengeCompleted;

	public ArenaSetup.GameTypeSetup GameTypeSetup => arenaSitting.gameTypeSetup;

	public bool SessionStillGoing
	{
		get
		{
			if (!sessionEnded)
			{
				return endSessionCounter < 0;
			}
			return false;
		}
	}

	public Room room => game.cameras[0].room;

	public virtual bool SpawnDefaultRoomItems => GameTypeSetup.levelItems;

	public int CurrentFood => 0;

	public Player.InputPackage MapInput => default(Player.InputPackage);

	public bool RevealMap => false;

	public Vector2 MapOwnerInRoomPosition => new Vector2(0f, 0f);

	public bool MapDiscoveryActive => false;

	public int MapOwnerRoom => 0;

	public ArenaGameSession(RainWorldGame game)
		: base(game)
	{
		characterStats = new SlugcatStats(SlugcatStats.Name.White, malnourished: false);
		arenaSitting = game.manager.arenaSitting;
		arenaSitting.SessionStartReset();
		if (ModManager.MSC)
		{
			if (arenaSitting.gameTypeSetup.gameType == MoreSlugcatsEnums.GameTypeID.Challenge)
			{
				chMeta = arenaSitting.gameTypeSetup.challengeMeta;
				if (chMeta.winMethod == ChallengeInformation.ChallengeMeta.WinCondition.TAME || chMeta.secondaryWinMethod == ChallengeInformation.ChallengeMeta.WinCondition.TAME || chMeta.nonAggressive)
				{
					arenaSitting.gameTypeSetup.evilAI = false;
				}
				else
				{
					arenaSitting.gameTypeSetup.evilAI = true;
				}
				arenaSitting.gameTypeSetup.fliesSpawn = chMeta.batflies;
				arenaSitting.gameTypeSetup.foodScore = 1;
				arenaSitting.gameTypeSetup.rainWhenOnePlayerLeft = false;
				arenaSitting.gameTypeSetup.levelItems = false;
				arenaSitting.gameTypeSetup.spearHitScore = 0;
				arenaSitting.gameTypeSetup.spearsHitPlayers = true;
				arenaSitting.gameTypeSetup.survivalScore = 0;
				arenaSitting.gameTypeSetup.saveCreatures = false;
				arenaSitting.gameTypeSetup.repeatSingleLevelForever = true;
				arenaSitting.gameTypeSetup.savingAndLoadingSession = false;
				arenaSitting.gameTypeSetup.denEntryRule = ArenaSetup.GameTypeSetup.DenEntryRule.Standard;
				arenaSitting.gameTypeSetup.wildLifeSetting = ArenaSetup.GameTypeSetup.WildLifeSetting.Medium;
				arenaSitting.sandboxPlayMode = true;
				SandboxSettingsInterface.DefaultKillScores(ref arenaSitting.gameTypeSetup.killScores);
			}
			characterStats_Mplayer = new SlugcatStats[4];
			if (chMeta == null)
			{
				for (int i = 0; i < arenaSitting.players.Count; i++)
				{
					characterStats_Mplayer[arenaSitting.players[i].playerNumber] = new SlugcatStats(arenaSitting.players[i].playerClass, malnourished: false);
				}
			}
			else
			{
				characterStats_Mplayer[0] = new SlugcatStats(chMeta.slugcatClass, malnourished: false);
				if (chMeta.spawnDen2 >= 0)
				{
					characterStats_Mplayer[1] = new SlugcatStats(chMeta.slugcatClass, malnourished: false);
				}
				if (chMeta.spawnDen3 >= 0)
				{
					characterStats_Mplayer[2] = new SlugcatStats(chMeta.slugcatClass, malnourished: false);
				}
				if (chMeta.spawnDen4 >= 0)
				{
					characterStats_Mplayer[3] = new SlugcatStats(chMeta.slugcatClass, malnourished: false);
				}
			}
		}
		else
		{
			chMeta = null;
		}
		if (!game.manager.rainWorld.progression.miscProgressionData.everPlayedArenaLevels.Contains(arenaSitting.GetCurrentLevel))
		{
			game.rainWorld.progression.miscProgressionData.everPlayedArenaLevels.Add(arenaSitting.GetCurrentLevel);
			game.rainWorld.progression.SaveProgression(saveMaps: false, saveMiscProg: true);
		}
		if (!(this is SandboxGameSession) || arenaSitting.sandboxPlayMode)
		{
			if (GameTypeSetup.rainWhenOnePlayerLeft && arenaSitting.players.Count > 1)
			{
				AddBehavior(new EarlyRain(this));
			}
			if (GameTypeSetup.fliesSpawn)
			{
				AddBehavior(new RespawnFlies(this));
			}
			AddBehavior(new StartBump(this));
		}
		AddBehavior(new ExitManager(this));
		thisFrameActivePlayers = arenaSitting.players.Count;
		if (chMeta == null)
		{
			if (GameTypeSetup.sessionTimeLengthIndex < 0 || GameTypeSetup.sessionTimeLengthIndex >= ArenaSetup.GameTypeSetup.SessionTimesInMinutesArray.Length)
			{
				AddBehavior(new NoRain(this));
			}
			else
			{
				rainCycleTimeInMinutes = ArenaSetup.GameTypeSetup.SessionTimesInMinutesArray[GameTypeSetup.sessionTimeLengthIndex];
			}
		}
		else if (chMeta.rainTime <= 0)
		{
			AddBehavior(new NoRain(this));
		}
		else
		{
			rainCycleTimeInMinutes = (float)chMeta.rainTime / 60f;
		}
		if (chMeta != null)
		{
			AddBehavior(new ChallengeBehavior(this));
		}
		if (GameTypeSetup.evilAI)
		{
			AddBehavior(new Evilifier(this));
		}
	}

	public virtual void ProcessShutDown()
	{
	}

	public void AddBehavior(ArenaGameBehavior behav)
	{
		behaviors.Add(behav);
		if (behav is EarlyRain)
		{
			earlyRain = behav as EarlyRain;
		}
		else if (behav is RespawnFlies)
		{
			respawnFlies = behav as RespawnFlies;
		}
		else if (behav is ExitManager)
		{
			exitManager = behav as ExitManager;
		}
		else if (behav is NoRain)
		{
			noRain = behav as NoRain;
		}
	}

	public void RemoveBehavior(ArenaGameBehavior behav)
	{
		for (int num = behaviors.Count - 1; num >= 0; num--)
		{
			if (behaviors[num] == behav)
			{
				RemoveBehavior(num);
			}
		}
	}

	private void RemoveBehavior(int i)
	{
		if (behaviors[i] == earlyRain)
		{
			earlyRain = null;
		}
		else if (behaviors[i] == respawnFlies)
		{
			respawnFlies = null;
		}
		else if (behaviors[i] == exitManager)
		{
			exitManager = null;
		}
		else if (behaviors[i] == noRain)
		{
			noRain = null;
		}
		behaviors.RemoveAt(i);
	}

	public virtual void SpawnCreatures()
	{
		if (!GameTypeSetup.saveCreatures)
		{
			arenaSitting.creatures.Clear();
		}
		if (ModManager.MSC && arenaSitting.gameTypeSetup.gameType == MoreSlugcatsEnums.GameTypeID.Challenge && arenaSitting.gameTypeSetup.challengeMeta != null && arenaSitting.gameTypeSetup.challengeMeta.arenaSpawns != ArenaSetup.GameTypeSetup.WildLifeSetting.Off)
		{
			ArenaCreatureSpawner.allowLockedCreatures = true;
			ArenaCreatureSpawner.SpawnArenaCreatures(game, arenaSitting.gameTypeSetup.challengeMeta.arenaSpawns, ref arenaSitting.creatures, ref arenaSitting.multiplayerUnlocks);
		}
	}

	public virtual void Initiate()
	{
		initiated = true;
		for (int i = 0; i < behaviors.Count; i++)
		{
			behaviors[i].Initiate();
		}
	}

	protected void AddHUD()
	{
		game.cameras[0].hud = new global::HUD.HUD(new FContainer[2]
		{
			game.cameras[0].ReturnFContainer("HUD"),
			game.cameras[0].ReturnFContainer("HUD2")
		}, game.rainWorld, this);
		game.cameras[0].hud.InitMultiplayerHud(this);
	}

	public virtual void Update()
	{
		if (arenaSitting.attempLoadInGame && arenaSitting.gameTypeSetup.savingAndLoadingSession)
		{
			arenaSitting.attempLoadInGame = false;
			arenaSitting.LoadFromFile(this, game.world, game.rainWorld);
		}
		if (initiated)
		{
			counter++;
		}
		else if (room != null && room.shortCutsReady)
		{
			Initiate();
		}
		if (room != null && chMeta != null && chMeta.deferred)
		{
			room.deferred = true;
		}
		thisFrameActivePlayers = PlayersStillActive(addToAliveTime: true, dontCountSandboxLosers: false);
		if (!sessionEnded)
		{
			if (endSessionCounter > 0)
			{
				if (ShouldSessionEnd())
				{
					endSessionCounter--;
					if (endSessionCounter == 0)
					{
						EndSession();
					}
				}
				else
				{
					endSessionCounter = -1;
				}
			}
			else if (endSessionCounter == -1 && ShouldSessionEnd())
			{
				endSessionCounter = 30;
			}
		}
		for (int i = 0; i < behaviors.Count; i++)
		{
			if (behaviors[i].slatedForDeletion)
			{
				RemoveBehavior(i);
			}
			else
			{
				behaviors[i].Update();
			}
		}
		if (game.world.rainCycle.TimeUntilRain < -1000 && !sessionEnded)
		{
			outsidePlayersCountAsDead = true;
			EndSession();
		}
	}

	public virtual bool ShouldSessionEnd()
	{
		return false;
	}

	public void EndSession()
	{
		if (chMeta != null && exitManager.IsPlayerInDen(Players[0]) && exitManager.challengeCompleted)
		{
			challengeCompleted = true;
			if (game.rainWorld.progression.miscProgressionData.completedChallengeTimes == null)
			{
				game.rainWorld.progression.miscProgressionData.completedChallengeTimes = new List<int>();
			}
			while (game.rainWorld.progression.miscProgressionData.completedChallengeTimes.Count <= GameTypeSetup.challengeID - 1)
			{
				game.rainWorld.progression.miscProgressionData.completedChallengeTimes.Add(-1);
			}
			if (game.rainWorld.progression.miscProgressionData.completedChallengeTimes[GameTypeSetup.challengeID - 1] > counter)
			{
				game.rainWorld.progression.miscProgressionData.completedChallengeTimes[GameTypeSetup.challengeID - 1] = counter;
			}
			int num = 0;
			for (int i = 0; i < game.rainWorld.progression.miscProgressionData.completedChallenges.Count; i++)
			{
				if (game.rainWorld.progression.miscProgressionData.completedChallenges[i])
				{
					num++;
				}
			}
			if (game.rainWorld.progression.miscProgressionData.completedChallenges.Count < GameTypeSetup.challengeID)
			{
				for (int j = game.rainWorld.progression.miscProgressionData.completedChallenges.Count; j < GameTypeSetup.challengeID; j++)
				{
					if (j == GameTypeSetup.challengeID - 1)
					{
						game.rainWorld.progression.miscProgressionData.completedChallenges.Add(item: true);
					}
					else
					{
						game.rainWorld.progression.miscProgressionData.completedChallenges.Add(item: false);
					}
				}
			}
			else
			{
				game.rainWorld.progression.miscProgressionData.completedChallenges[GameTypeSetup.challengeID - 1] = true;
			}
			if (game.rainWorld.progression.miscProgressionData.completedChallenges.Count > 34 && game.rainWorld.progression.miscProgressionData.completedChallenges[28] && game.rainWorld.progression.miscProgressionData.completedChallenges[31] && game.rainWorld.progression.miscProgressionData.completedChallenges[34] && game.rainWorld.progression.miscProgressionData.SetTokenCollected(MoreSlugcatsEnums.SandboxUnlockID.JokeRifle))
			{
				game.manager.specialUnlockText = game.rainWorld.inGameTranslator.Translate("The Rifle item is now available in Sandbox Mode.");
			}
			int num2 = 0;
			for (int k = 0; k < game.rainWorld.progression.miscProgressionData.completedChallenges.Count; k++)
			{
				if (game.rainWorld.progression.miscProgressionData.completedChallenges[k])
				{
					num2++;
				}
			}
			if (num < MultiplayerUnlocks.TOTAL_CHALLENGES && num2 >= MultiplayerUnlocks.TOTAL_CHALLENGES)
			{
				game.rainWorld.options.commentary = true;
				game.manager.specialUnlockText = game.rainWorld.inGameTranslator.Translate("Developer Commentary mode is now available in the Options menu.");
			}
			if (chMeta.specialUnlock && !game.rainWorld.progression.miscProgressionData.challengeArenaUnlocks.Contains(chMeta.arena))
			{
				game.rainWorld.progression.miscProgressionData.challengeArenaUnlocks.Add(chMeta.arena);
			}
			game.rainWorld.progression.SaveProgression(saveMaps: false, saveMiscProg: true);
		}
		arenaSitting.SessionEnded(this);
		for (int l = 0; l < Players.Count; l++)
		{
			if (Players[l].realizedCreature != null)
			{
				if (outsidePlayersCountAsDead)
				{
					Players[l].realizedCreature.Die();
				}
				else
				{
					(Players[l].realizedCreature as Player).controller = new PlayerStopController();
				}
			}
		}
		sessionEnded = true;
	}

	public bool EndOfSessionLogPlayerAsAlive(int playerNumber)
	{
		if (exitManager == null)
		{
			return true;
		}
		for (int i = 0; i < exitManager.playersInDens.Count; i++)
		{
			if ((exitManager.playersInDens[i].creature.abstractCreature.state as PlayerState).playerNumber == playerNumber)
			{
				return true;
			}
		}
		if (outsidePlayersCountAsDead)
		{
			return false;
		}
		for (int j = 0; j < Players.Count; j++)
		{
			if ((Players[j].state as PlayerState).playerNumber == playerNumber)
			{
				return Players[j].state.alive;
			}
		}
		return false;
	}

	public bool IsCreatureAllowedToEmergeFromDen(AbstractCreature crit)
	{
		if (room == null)
		{
			return false;
		}
		if (crit.Room != null && crit.Room != room.abstractRoom)
		{
			return true;
		}
		int abstractNode = crit.pos.abstractNode;
		if (abstractNode < 0 || abstractNode >= crit.Room.nodes.Length || crit.Room.nodes[abstractNode].type != AbstractRoomNode.Type.Den)
		{
			return true;
		}
		if (!room.readyForAI)
		{
			return false;
		}
		for (int i = 0; i < Players.Count; i++)
		{
			if (Players[i].realizedCreature != null && !Players[i].realizedCreature.dead && Players[i].realizedCreature.room == room)
			{
				int num = room.aimap.ExitDistanceForCreatureAndCheckNeighbours(room.GetTilePosition(Players[i].realizedCreature.firstChunk.pos), room.abstractRoom.CommonToCreatureSpecificNodeIndex(abstractNode, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Fly)), StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Fly));
				if (num > 0 && num < 15)
				{
					return false;
				}
			}
		}
		return true;
	}

	public float DarkenExitSymbol(int exit)
	{
		if (exitManager == null)
		{
			return 1f;
		}
		return exitManager.DarkenExitSymbol(exit);
	}

	public void Killing(Player player, Creature killedCrit)
	{
		if (sessionEnded || (ModManager.MSC && player.AI != null))
		{
			return;
		}
		IconSymbol.IconSymbolData iconSymbolData = CreatureSymbol.SymbolDataFromCreature(killedCrit.abstractCreature);
		for (int i = 0; i < arenaSitting.players.Count; i++)
		{
			if (player.playerState.playerNumber == arenaSitting.players[i].playerNumber)
			{
				if (CreatureSymbol.DoesCreatureEarnATrophy(killedCrit.Template.type))
				{
					arenaSitting.players[i].roundKills.Add(iconSymbolData);
					arenaSitting.players[i].allKills.Add(iconSymbolData);
				}
				int index = MultiplayerUnlocks.SandboxUnlockForSymbolData(iconSymbolData).Index;
				if (index >= 0)
				{
					arenaSitting.players[i].AddSandboxScore(arenaSitting.gameTypeSetup.killScores[index]);
				}
				else
				{
					arenaSitting.players[i].AddSandboxScore(0);
				}
				break;
			}
		}
		if (!CreatureSymbol.DoesCreatureEarnATrophy(killedCrit.Template.type))
		{
			return;
		}
		for (int j = 0; j < game.cameras[0].hud.parts.Count; j++)
		{
			if (game.cameras[0].hud.parts[j] is PlayerSpecificMultiplayerHud && (game.cameras[0].hud.parts[j] as PlayerSpecificMultiplayerHud).abstractPlayer == player.abstractCreature)
			{
				(game.cameras[0].hud.parts[j] as PlayerSpecificMultiplayerHud).killsList.Killing(iconSymbolData);
				break;
			}
		}
	}

	public void PlayerLandSpear(Player player, Creature target)
	{
		if (sessionEnded || GameTypeSetup.spearHitScore == 0 || !CreatureSymbol.DoesCreatureEarnATrophy(target.Template.type))
		{
			return;
		}
		for (int i = 0; i < arenaSitting.players.Count; i++)
		{
			if (player.playerState.playerNumber == arenaSitting.players[i].playerNumber)
			{
				arenaSitting.players[i].AddSandboxScore(GameTypeSetup.spearHitScore);
			}
		}
	}

	public int ScoreOfPlayer(Player player, bool inHands)
	{
		if (player == null)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < arenaSitting.players.Count; i++)
		{
			if (arenaSitting.players[i].playerNumber != player.playerState.playerNumber)
			{
				continue;
			}
			float num2 = 0f;
			if (inHands && arenaSitting.gameTypeSetup.foodScore != 0)
			{
				for (int j = 0; j < player.grasps.Length; j++)
				{
					if (player.grasps[j] != null && player.grasps[j].grabbed is IPlayerEdible)
					{
						IPlayerEdible playerEdible = player.grasps[j].grabbed as IPlayerEdible;
						num2 = ((!ModManager.MSC || !(player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint) || (!(playerEdible is JellyFish) && !(playerEdible is Centipede) && !(playerEdible is Fly) && !(playerEdible is VultureGrub) && !(playerEdible is SmallNeedleWorm) && !(playerEdible is Hazer))) ? (num2 + (float)(player.grasps[j].grabbed as IPlayerEdible).FoodPoints) : (num2 + 0f));
					}
				}
			}
			if (Math.Abs(arenaSitting.gameTypeSetup.foodScore) > 99)
			{
				if (player.FoodInStomach > 0 || num2 > 0f)
				{
					arenaSitting.players[i].AddSandboxScore(arenaSitting.gameTypeSetup.foodScore);
				}
				num += arenaSitting.players[i].score;
			}
			num += (int)((float)arenaSitting.players[i].score + ((float)player.FoodInStomach + num2) * (float)arenaSitting.gameTypeSetup.foodScore);
		}
		return num;
	}

	public bool PlayerTryingToEnterDen(ShortcutHandler.ShortCutVessel shortcutVessel)
	{
		if (exitManager != null)
		{
			return exitManager.PlayerTryingToEnterDen(shortcutVessel);
		}
		return true;
	}

	public void PlayerSpitOutOfShortCut(AbstractCreature player)
	{
		for (int i = 0; i < arenaSitting.players.Count; i++)
		{
			if (arenaSitting.players[i].playerNumber == (player.state as PlayerState).playerNumber)
			{
				arenaSitting.players[i].hasEnteredGameArea = true;
				break;
			}
		}
	}

	protected int PlayersStillActive(bool addToAliveTime, bool dontCountSandboxLosers)
	{
		int num = 0;
		for (int i = 0; i < Players.Count; i++)
		{
			bool flag = true;
			if (!Players[i].state.alive)
			{
				flag = false;
			}
			if (flag && exitManager != null && exitManager.IsPlayerInDen(Players[i]))
			{
				flag = false;
			}
			if (flag && Players[i].realizedCreature != null && (Players[i].realizedCreature as Player).dangerGrasp != null)
			{
				flag = false;
			}
			if (flag)
			{
				for (int j = 0; j < arenaSitting.players.Count; j++)
				{
					if ((Players[i].state as PlayerState).playerNumber == arenaSitting.players[j].playerNumber)
					{
						if (Players[i].Room == game.world.offScreenDen && arenaSitting.players[j].hasEnteredGameArea)
						{
							flag = false;
						}
						if (addToAliveTime && flag && !sessionEnded && game.pauseMenu == null)
						{
							arenaSitting.players[j].timeAlive++;
						}
						if (dontCountSandboxLosers && arenaSitting.players[j].sandboxWin < 0)
						{
							flag = false;
						}
						break;
					}
				}
			}
			if (flag)
			{
				num++;
			}
		}
		return num;
	}

	public void SpawnPlayers(Room room, List<int> suggestedDens)
	{
		List<ArenaSitting.ArenaPlayer> list = new List<ArenaSitting.ArenaPlayer>();
		if (ModManager.MSC && GameTypeSetup.gameType == MoreSlugcatsEnums.GameTypeID.Challenge)
		{
			for (int i = 0; i < suggestedDens.Count; i++)
			{
				list.Add(new ArenaSitting.ArenaPlayer(i)
				{
					playerClass = GameTypeSetup.challengeMeta.slugcatClass
				});
			}
			arenaSitting.players = list;
		}
		else
		{
			List<ArenaSitting.ArenaPlayer> list2 = new List<ArenaSitting.ArenaPlayer>();
			for (int j = 0; j < arenaSitting.players.Count; j++)
			{
				list2.Add(arenaSitting.players[j]);
			}
			while (list2.Count > 0)
			{
				int index = UnityEngine.Random.Range(0, list2.Count);
				list.Add(list2[index]);
				list2.RemoveAt(index);
			}
		}
		int exits = game.world.GetAbstractRoom(0).exits;
		int[] array = new int[exits];
		if (suggestedDens != null)
		{
			for (int k = 0; k < suggestedDens.Count; k++)
			{
				if (suggestedDens[k] >= 0 && suggestedDens[k] < array.Length)
				{
					array[suggestedDens[k]] -= 1000;
				}
			}
		}
		for (int l = 0; l < list.Count; l++)
		{
			int num = UnityEngine.Random.Range(0, exits);
			float num2 = float.MinValue;
			for (int m = 0; m < exits; m++)
			{
				float num3 = UnityEngine.Random.value - (float)array[m] * 1000f;
				IntVector2 startTile = room.ShortcutLeadingToNode(m).StartTile;
				for (int n = 0; n < exits; n++)
				{
					if (n != m && array[n] > 0)
					{
						num3 += Mathf.Clamp(startTile.FloatDist(room.ShortcutLeadingToNode(n).StartTile), 8f, 17f) * UnityEngine.Random.value;
					}
				}
				if (num3 > num2)
				{
					num = m;
					num2 = num3;
				}
			}
			array[num]++;
			AbstractCreature abstractCreature = new AbstractCreature(game.world, StaticWorld.GetCreatureTemplate("Slugcat"), null, new WorldCoordinate(0, -1, -1, -1), new EntityID(-1, list[l].playerNumber));
			if (ModManager.MSC && l == 0)
			{
				game.cameras[0].followAbstractCreature = abstractCreature;
			}
			if (chMeta != null)
			{
				abstractCreature.state = new PlayerState(abstractCreature, list[l].playerNumber, characterStats_Mplayer[0].name, isGhost: false);
			}
			else
			{
				abstractCreature.state = new PlayerState(abstractCreature, list[l].playerNumber, new SlugcatStats.Name(ExtEnum<SlugcatStats.Name>.values.GetEntry(list[l].playerNumber)), isGhost: false);
			}
			abstractCreature.Realize();
			ShortcutHandler.ShortCutVessel shortCutVessel = new ShortcutHandler.ShortCutVessel(new IntVector2(-1, -1), abstractCreature.realizedCreature, game.world.GetAbstractRoom(0), 0);
			shortCutVessel.entranceNode = num;
			shortCutVessel.room = game.world.GetAbstractRoom(0);
			abstractCreature.pos.room = game.world.offScreenDen.index;
			game.shortcuts.betweenRoomsWaitingLobby.Add(shortCutVessel);
			AddPlayer(abstractCreature);
			if (ModManager.MSC)
			{
				if ((abstractCreature.realizedCreature as Player).SlugCatClass == SlugcatStats.Name.Red)
				{
					creatureCommunities.SetLikeOfPlayer(CreatureCommunities.CommunityID.All, -1, l, -0.75f);
					creatureCommunities.SetLikeOfPlayer(CreatureCommunities.CommunityID.Scavengers, -1, l, 0.5f);
				}
				if ((abstractCreature.realizedCreature as Player).SlugCatClass == SlugcatStats.Name.Yellow)
				{
					creatureCommunities.SetLikeOfPlayer(CreatureCommunities.CommunityID.All, -1, l, 0.75f);
					creatureCommunities.SetLikeOfPlayer(CreatureCommunities.CommunityID.Scavengers, -1, l, 0.3f);
				}
				if ((abstractCreature.realizedCreature as Player).SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
				{
					creatureCommunities.SetLikeOfPlayer(CreatureCommunities.CommunityID.All, -1, l, -0.5f);
					creatureCommunities.SetLikeOfPlayer(CreatureCommunities.CommunityID.Scavengers, -1, l, -1f);
				}
			}
		}
		playersSpawned = true;
		if (ModManager.MSC && room.abstractRoom.name == "Chal_AI" && GameTypeSetup.gameType == MoreSlugcatsEnums.GameTypeID.Challenge)
		{
			Oracle obj = new Oracle(new AbstractPhysicalObject(game.world, AbstractPhysicalObject.AbstractObjectType.Oracle, null, new WorldCoordinate(room.abstractRoom.index, 15, 15, -1), game.GetNewID()), room);
			room.AddObject(obj);
		}
	}

	public void SpawnItem(Room room, PlacedObject placedObj)
	{
		if (!SpawnDefaultRoomItems || UnityEngine.Random.value > (placedObj.data as PlacedObject.MultiplayerItemData).chance)
		{
			return;
		}
		AbstractPhysicalObject.AbstractObjectType abstractObjectType = AbstractPhysicalObject.AbstractObjectType.Rock;
		bool flag = false;
		if (!((placedObj.data as PlacedObject.MultiplayerItemData).type == PlacedObject.MultiplayerItemData.Type.Rock))
		{
			if ((placedObj.data as PlacedObject.MultiplayerItemData).type == PlacedObject.MultiplayerItemData.Type.Spear)
			{
				abstractObjectType = AbstractPhysicalObject.AbstractObjectType.Spear;
			}
			else if ((placedObj.data as PlacedObject.MultiplayerItemData).type == PlacedObject.MultiplayerItemData.Type.ExplosiveSpear)
			{
				abstractObjectType = AbstractPhysicalObject.AbstractObjectType.Spear;
				flag = true;
			}
			else if ((placedObj.data as PlacedObject.MultiplayerItemData).type == PlacedObject.MultiplayerItemData.Type.Bomb)
			{
				abstractObjectType = AbstractPhysicalObject.AbstractObjectType.ScavengerBomb;
			}
			else if ((placedObj.data as PlacedObject.MultiplayerItemData).type == PlacedObject.MultiplayerItemData.Type.SporePlant)
			{
				abstractObjectType = AbstractPhysicalObject.AbstractObjectType.SporePlant;
			}
		}
		if (arenaSitting.multiplayerUnlocks.ExoticItems < 1f && (placedObj.data as PlacedObject.MultiplayerItemData).type != PlacedObject.MultiplayerItemData.Type.Rock && (placedObj.data as PlacedObject.MultiplayerItemData).type != PlacedObject.MultiplayerItemData.Type.Spear)
		{
			MultiplayerUnlocks.SandboxUnlockID sandboxUnlockID = MultiplayerUnlocks.SandboxUnlockForSymbolData(new IconSymbol.IconSymbolData(CreatureTemplate.Type.StandardGroundCreature, abstractObjectType, (abstractObjectType == AbstractPhysicalObject.AbstractObjectType.Spear && flag) ? 1 : 0));
			if (sandboxUnlockID == MultiplayerUnlocks.SandboxUnlockID.Slugcat)
			{
				return;
			}
			if (!arenaSitting.multiplayerUnlocks.SandboxItemUnlocked(sandboxUnlockID) && UnityEngine.Random.value > arenaSitting.multiplayerUnlocks.ExoticItems)
			{
				abstractObjectType = AbstractPhysicalObject.AbstractObjectType.Spear;
				flag = false;
			}
		}
		if (abstractObjectType == AbstractPhysicalObject.AbstractObjectType.Spear)
		{
			AbstractSpear item = new AbstractSpear(room.world, null, room.GetWorldCoordinate(placedObj.pos), game.GetNewID(), flag);
			room.abstractRoom.entities.Add(item);
		}
		else if (abstractObjectType == AbstractPhysicalObject.AbstractObjectType.Rock)
		{
			AbstractPhysicalObject item2 = new AbstractPhysicalObject(room.world, AbstractPhysicalObject.AbstractObjectType.Rock, null, room.GetWorldCoordinate(placedObj.pos), game.GetNewID());
			room.abstractRoom.entities.Add(item2);
		}
		else if (abstractObjectType == AbstractPhysicalObject.AbstractObjectType.ScavengerBomb)
		{
			AbstractPhysicalObject item3 = new AbstractPhysicalObject(room.world, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null, room.GetWorldCoordinate(placedObj.pos), game.GetNewID());
			room.abstractRoom.entities.Add(item3);
		}
		else if (abstractObjectType == AbstractPhysicalObject.AbstractObjectType.SporePlant)
		{
			AbstractPhysicalObject item4 = new SporePlant.AbstractSporePlant(room.world, null, room.GetWorldCoordinate(placedObj.pos), game.GetNewID(), -2, -2, null, used: false, pacified: true);
			room.abstractRoom.entities.Add(item4);
		}
	}

	public global::HUD.HUD.OwnerType GetOwnerType()
	{
		return global::HUD.HUD.OwnerType.ArenaSession;
	}

	public void PlayHUDSound(SoundID soundID)
	{
		room.game.cameras[0].virtualMicrophone.PlaySound(soundID, 0f, 1f, 1f);
	}

	public void FoodCountDownDone()
	{
	}
}
