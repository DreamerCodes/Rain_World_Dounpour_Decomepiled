using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Expedition;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class SaveState
{
	public PlayerProgression progression;

	public int food;

	public bool malnourished;

	public bool lastMalnourished;

	public bool theGlow;

	public bool guideOverseerDead;

	public string denPosition;

	public string lastVanillaDen;

	public int cycleNumber;

	public int nextIssuedID;

	public SlugcatStats.Name saveStateNumber;

	public RegionState[] regionStates;

	public string[] regionLoadStrings;

	public List<string> unrecognizedRegionLoadStrings;

	public string creatureCommunitiesString;

	public List<int> respawnCreatures;

	public List<int> waitRespawnCreatures;

	public string[] swallowedItems;

	public string[] playerGrasps;

	public List<string> unrecognizedSwallowedItems;

	public List<string> unrecognizedPlayerGrasps;

	public int worldVersion;

	public int gameVersion;

	public int initiatedInGameVersion;

	public int seed;

	public int cyclesInCurrentWorldVersion;

	public MiscWorldSaveData miscWorldSaveData;

	public DeathPersistentSaveData deathPersistentSaveData;

	public DreamsState dreamsState;

	public int totFood;

	public int totTime;

	public bool redExtraCycles;

	public List<string> unrecognizedSaveStrings;

	public List<KeyValuePair<IconSymbol.IconSymbolData, int>> kills;

	public List<string> unrecognizedKills;

	public bool loaded;

	public bool hasRobo;

	public bool karmaDream;

	public bool wearingCloak;

	public int forcePupsNextCycle;

	public List<PersistentObjectTracker> objectTrackers;

	public List<string> pendingObjects;

	public List<string> pendingFriendCreatures;

	public List<string> oeEncounters;

	public static string forcedEndRoomToAllowwSave;

	public bool justBeatGame;

	public float SlowFadeIn => Mathf.Max(malnourished ? 4f : 0.8f, (saveStateNumber == SlugcatStats.Name.Red && cycleNumber >= RedsIllness.RedsCycles(redExtraCycles) && !Custom.rainWorld.ExpeditionMode) ? Custom.LerpMap(cycleNumber, RedsIllness.RedsCycles(redExtraCycles), RedsIllness.RedsCycles(redExtraCycles) + 5, 4f, 15f) : 0.8f);

	public bool CanSeeVoidSpawn
	{
		get
		{
			if (!theGlow)
			{
				if (ModManager.MSC)
				{
					return saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Saint;
				}
				return false;
			}
			return true;
		}
	}

	public void WarpDenPosition(RainWorldGame game)
	{
	}

	public SaveState(SlugcatStats.Name saveStateNumber, PlayerProgression progression)
	{
		this.saveStateNumber = saveStateNumber;
		this.progression = progression;
		regionStates = new RegionState[progression.regionNames.Length];
		regionLoadStrings = new string[progression.regionNames.Length];
		respawnCreatures = new List<int>();
		waitRespawnCreatures = new List<int>();
		worldVersion = RainWorld.worldVersion;
		gameVersion = RainWorld.gameVersion;
		initiatedInGameVersion = RainWorld.gameVersion;
		lastVanillaDen = "";
		objectTrackers = new List<PersistentObjectTracker>();
		pendingObjects = new List<string>();
		pendingFriendCreatures = new List<string>();
		oeEncounters = new List<string>();
		miscWorldSaveData = new MiscWorldSaveData(saveStateNumber);
		deathPersistentSaveData = new DeathPersistentSaveData(saveStateNumber);
		if (saveStateNumber == SlugcatStats.Name.White || saveStateNumber == SlugcatStats.Name.Yellow || (ModManager.MSC && (saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Artificer || saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Saint || saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)))
		{
			dreamsState = new DreamsState();
		}
		if (saveStateNumber != SlugcatStats.Name.White && saveStateNumber != SlugcatStats.Name.Yellow)
		{
			deathPersistentSaveData.PoleMimicEverSeen = true;
			deathPersistentSaveData.ScavTollMessage = true;
			deathPersistentSaveData.KarmaFlowerMessage = true;
			deathPersistentSaveData.GoExploreMessage = true;
		}
		if (saveStateNumber == SlugcatStats.Name.Red)
		{
			deathPersistentSaveData.theMark = true;
			deathPersistentSaveData.karma = 2;
		}
		if (ModManager.MSC && (saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Rivulet || saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel))
		{
			deathPersistentSaveData.theMark = true;
		}
		if (ModManager.Expedition && Custom.rainWorld.ExpeditionMode)
		{
			deathPersistentSaveData.PoleMimicEverSeen = true;
			deathPersistentSaveData.ScavTollMessage = true;
			deathPersistentSaveData.KarmaFlowerMessage = true;
			deathPersistentSaveData.GoExploreMessage = true;
			deathPersistentSaveData.ScavMerchantMessage = true;
			deathPersistentSaveData.ScavTollMessage = true;
			deathPersistentSaveData.SMEatTutorial = true;
			deathPersistentSaveData.SMTutorialMessage = true;
			deathPersistentSaveData.TongueTutorialMessage = true;
			deathPersistentSaveData.ArtificerMaulTutorial = true;
			deathPersistentSaveData.ArtificerTutorialMessage = true;
			deathPersistentSaveData.DangleFruitInWaterMessage = true;
			deathPersistentSaveData.GateStandTutorial = true;
			deathPersistentSaveData.GoExploreMessage = true;
			miscWorldSaveData.SLOracleState.neuronGiveConversationCounter = 1;
			if (saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Rivulet || saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Saint)
			{
				miscWorldSaveData.moonHeartRestored = true;
				miscWorldSaveData.pebblesEnergyTaken = true;
			}
			deathPersistentSaveData.karma = ExpeditionGame.tempKarma;
			deathPersistentSaveData.karmaCap = 4;
			deathPersistentSaveData.theMark = true;
			if (ExpeditionGame.activeUnlocks.Contains("unl-glow"))
			{
				theGlow = true;
			}
			if (ExpeditionGame.activeUnlocks.Contains("unl-karma"))
			{
				if (ExpeditionGame.tempReinforce)
				{
					deathPersistentSaveData.reinforcedKarma = true;
				}
				if (ExpeditionGame.tempKarmaPos.HasValue)
				{
					deathPersistentSaveData.karmaFlowerPosition = ExpeditionGame.tempKarmaPos;
				}
			}
			miscWorldSaveData.SLOracleState.playerEncountersWithMark = ((saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Spear) ? 5 : 2);
			miscWorldSaveData.SLOracleState.neuronsLeft = 5;
			miscWorldSaveData.SSaiConversationsHad = 1;
			miscWorldSaveData.cyclesSinceSSai = 10;
			miscWorldSaveData.SSaiThrowOuts = -1;
			if (ExpeditionGame.unlockedExpeditionSlugcats.Contains(MoreSlugcatsEnums.SlugcatStatsName.Gourmand))
			{
				progression.miscProgressionData.beaten_Gourmand = true;
			}
		}
		forcedEndRoomToAllowwSave = "";
		kills = new List<KeyValuePair<IconSymbol.IconSymbolData, int>>();
		unrecognizedSaveStrings = new List<string>();
		unrecognizedRegionLoadStrings = new List<string>();
		unrecognizedKills = new List<string>();
		unrecognizedSwallowedItems = new List<string>();
		unrecognizedPlayerGrasps = new List<string>();
		seed = UnityEngine.Random.Range(0, 10000);
	}

	public static string SetCustomData(AbstractPhysicalObject apo, string baseString)
	{
		return baseString;
	}

	public static string SetCustomData(AbstractCreature ac, string baseString)
	{
		return baseString;
	}

	public static string SetCustomData(PlacedObject po, string baseString)
	{
		return baseString;
	}

	public static string SetCustomData(PlacedObject.Data pod, string baseString)
	{
		return baseString;
	}

	public void SessionEnded(RainWorldGame game, bool survived, bool newMalnourished)
	{
		lastMalnourished = malnourished;
		malnourished = newMalnourished;
		deathPersistentSaveData.sessionTrackRecord.Add(new DeathPersistentSaveData.SessionRecord(survived, game.GetStorySession.playerSessionRecords[0].wokeUpInRegion != game.world.region.name));
		if (deathPersistentSaveData.sessionTrackRecord.Count > 20)
		{
			deathPersistentSaveData.sessionTrackRecord.RemoveAt(0);
		}
		for (int num = deathPersistentSaveData.deathPositions.Count - 1; num >= 0; num--)
		{
			if (deathPersistentSaveData.deathPositions[num].Valid)
			{
				deathPersistentSaveData.deathPositions[num] = new WorldCoordinate(deathPersistentSaveData.deathPositions[num].room, deathPersistentSaveData.deathPositions[num].x, deathPersistentSaveData.deathPositions[num].y, deathPersistentSaveData.deathPositions[num].abstractNode + 1);
			}
			else
			{
				deathPersistentSaveData.deathPositions[num] = new WorldCoordinate(deathPersistentSaveData.deathPositions[num].unknownName, deathPersistentSaveData.deathPositions[num].x, deathPersistentSaveData.deathPositions[num].y, deathPersistentSaveData.deathPositions[num].abstractNode + 1);
			}
			if (deathPersistentSaveData.deathPositions[num].abstractNode >= 7)
			{
				deathPersistentSaveData.deathPositions.RemoveAt(num);
			}
		}
		if (survived)
		{
			deathPersistentSaveData.foodReplenishBonus = 0;
			Custom.Log("resetting food rep bonus");
			RainCycleTick(game, depleteSwarmRoom: true);
			cyclesInCurrentWorldVersion++;
			if (ModManager.MMF && progression.miscProgressionData.returnExplorationTutorialCounter > 0)
			{
				progression.miscProgressionData.returnExplorationTutorialCounter = 3;
			}
			food = 0;
			if (ModManager.CoopAvailable)
			{
				if (!(game.session.Players[0].state as PlayerState).permaDead && game.session.Players[0].realizedCreature != null && game.session.Players[0].realizedCreature.room != null)
				{
					food = (game.session.Players[0].realizedCreature as Player).FoodInRoom(eatAndDestroy: true);
				}
				else if (game.AlivePlayers.Count > 0 && game.FirstAlivePlayer != null)
				{
					food = (game.FirstAlivePlayer.realizedCreature as Player).FoodInRoom(eatAndDestroy: true);
				}
			}
			else
			{
				for (int i = 0; i < game.session.Players.Count; i++)
				{
					food += (game.session.Players[i].realizedCreature as Player).FoodInRoom(eatAndDestroy: true);
				}
			}
			food = Custom.IntClamp(food, 0, game.GetStorySession.characterStats.maxFood);
			if (malnourished)
			{
				food -= game.GetStorySession.characterStats.foodToHibernate;
			}
			else if (lastMalnourished)
			{
				if (game.devToolsActive && food < game.GetStorySession.characterStats.maxFood)
				{
					Custom.LogWarning("FOOD COUNT ISSUE!", food.ToString(), game.GetStorySession.characterStats.maxFood.ToString());
				}
				food = 0;
			}
			else
			{
				food -= game.GetStorySession.characterStats.foodToHibernate;
			}
			if (game.IsStorySession)
			{
				game.GetStorySession.saveState.justBeatGame = false;
			}
			BringUpToDate(game);
			for (int j = 0; j < game.GetStorySession.playerSessionRecords.Length; j++)
			{
				if (game.GetStorySession.playerSessionRecords[j] == null || (ModManager.CoopAvailable && game.world.GetAbstractRoom(game.Players[j].pos) == null))
				{
					continue;
				}
				game.GetStorySession.playerSessionRecords[j].pupCountInDen = 0;
				bool flag = false;
				game.GetStorySession.playerSessionRecords[j].wentToSleepInRegion = game.world.region.name;
				for (int k = 0; k < game.world.GetAbstractRoom(game.Players[j].pos).creatures.Count; k++)
				{
					if (!game.world.GetAbstractRoom(game.Players[j].pos).creatures[k].state.alive || game.world.GetAbstractRoom(game.Players[j].pos).creatures[k].state.socialMemory == null || game.world.GetAbstractRoom(game.Players[j].pos).creatures[k].realizedCreature == null || game.world.GetAbstractRoom(game.Players[j].pos).creatures[k].abstractAI == null || game.world.GetAbstractRoom(game.Players[j].pos).creatures[k].abstractAI.RealAI == null || game.world.GetAbstractRoom(game.Players[j].pos).creatures[k].abstractAI.RealAI.friendTracker == null || game.world.GetAbstractRoom(game.Players[j].pos).creatures[k].abstractAI.RealAI.friendTracker.friend == null || game.world.GetAbstractRoom(game.Players[j].pos).creatures[k].abstractAI.RealAI.friendTracker.friend != game.Players[j].realizedCreature || !(game.world.GetAbstractRoom(game.Players[j].pos).creatures[k].state.socialMemory.GetLike(game.Players[j].ID) > 0f))
					{
						continue;
					}
					if (ModManager.MSC && game.world.GetAbstractRoom(game.Players[j].pos).creatures[k].creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)
					{
						if ((game.world.GetAbstractRoom(game.Players[j].pos).creatures[k].state as PlayerNPCState).foodInStomach - ((game.world.GetAbstractRoom(game.Players[j].pos).creatures[k].state as PlayerNPCState).Malnourished ? SlugcatStats.SlugcatFoodMeter(MoreSlugcatsEnums.SlugcatStatsName.Slugpup).x : SlugcatStats.SlugcatFoodMeter(MoreSlugcatsEnums.SlugcatStatsName.Slugpup).y) >= 0)
						{
							game.GetStorySession.playerSessionRecords[j].pupCountInDen++;
						}
					}
					else if (!flag)
					{
						flag = true;
						game.GetStorySession.playerSessionRecords[j].friendInDen = game.world.GetAbstractRoom(game.Players[j].pos).creatures[k];
						SocialMemory.Relationship orInitiateRelationship = game.world.GetAbstractRoom(game.Players[j].pos).creatures[k].state.socialMemory.GetOrInitiateRelationship(game.Players[j].ID);
						orInitiateRelationship.like = Mathf.Lerp(orInitiateRelationship.like, 1f, 0.5f);
					}
				}
			}
			AppendKills(game.GetStorySession.playerSessionRecords[0].kills);
			if (ModManager.CoopAvailable)
			{
				for (int l = 1; l < game.GetStorySession.playerSessionRecords.Length; l++)
				{
					AppendKills(game.GetStorySession.playerSessionRecords[l].kills);
				}
			}
			game.GetStorySession.AppendTimeOnCycleEnd(deathOrGhost: false);
			deathPersistentSaveData.survives++;
			deathPersistentSaveData.winState.CycleCompleted(game);
			if (!ModManager.CoopAvailable)
			{
				deathPersistentSaveData.friendsSaved += ((game.GetStorySession.playerSessionRecords[0].friendInDen != null) ? 1 : 0);
			}
			else
			{
				List<AbstractCreature> list = new List<AbstractCreature>();
				PlayerSessionRecord[] playerSessionRecords = game.GetStorySession.playerSessionRecords;
				foreach (PlayerSessionRecord playerSessionRecord in playerSessionRecords)
				{
					if (!list.Contains(playerSessionRecord.friendInDen))
					{
						list.Add(playerSessionRecord.friendInDen);
					}
				}
				deathPersistentSaveData.friendsSaved += list.Count;
			}
			deathPersistentSaveData.karma++;
			if (malnourished)
			{
				deathPersistentSaveData.reinforcedKarma = false;
			}
			game.rainWorld.progression.SaveWorldStateAndProgression(malnourished);
			return;
		}
		game.GetStorySession.AppendTimeOnCycleEnd(deathOrGhost: true);
		deathPersistentSaveData.AddDeathPosition(game.cameras[0].hud.textPrompt.deathRoom, game.cameras[0].hud.textPrompt.deathPos);
		deathPersistentSaveData.deaths++;
		if (deathPersistentSaveData.karma == 0 || (saveStateNumber == SlugcatStats.Name.White && UnityEngine.Random.value < 0.5f) || saveStateNumber == SlugcatStats.Name.Yellow)
		{
			deathPersistentSaveData.foodReplenishBonus++;
			Custom.Log("Ticking up food rep bonus to:", deathPersistentSaveData.foodReplenishBonus.ToString());
		}
		else
		{
			Custom.Log("death screen, no food bonus");
		}
		deathPersistentSaveData.TickFlowerDepletion(1);
		if (ModManager.MMF && MMF.cfgExtraTutorials.Value)
		{
			Custom.Log("Exploration tutorial counter :", progression.miscProgressionData.returnExplorationTutorialCounter.ToString());
			if (game.IsStorySession && (game.world.region.name == "SB" || game.world.region.name == "SL" || game.world.region.name == "UW" || deathPersistentSaveData.karmaCap > 8 || miscWorldSaveData.SSaiConversationsHad > 0))
			{
				progression.miscProgressionData.returnExplorationTutorialCounter = -1;
				Custom.Log("CANCEL exploration counter");
			}
			else if (game.IsStorySession && (game.world.region.name == "SH" || (ModManager.MSC && game.world.region.name == "VS") || game.world.region.name == "DS" || game.world.region.name == "CC" || game.world.region.name == "LF" || game.world.region.name == "SI"))
			{
				Custom.Log("Exploration counter ticked to", progression.miscProgressionData.returnExplorationTutorialCounter.ToString());
				if (progression.miscProgressionData.returnExplorationTutorialCounter > 0)
				{
					progression.miscProgressionData.returnExplorationTutorialCounter--;
				}
			}
			else if (progression.miscProgressionData.returnExplorationTutorialCounter > 0)
			{
				progression.miscProgressionData.returnExplorationTutorialCounter = 3;
				Custom.Log("Reset exploration counter");
			}
		}
		if (ModManager.Expedition && game.rainWorld.ExpeditionMode)
		{
			ExpLog.Log("Loading previous cycle challenge progression");
			global::Expedition.Expedition.coreFile.Load();
			if (ExpeditionGame.expeditionComplete)
			{
				ExpeditionGame.expeditionComplete = false;
			}
		}
		game.rainWorld.progression.SaveProgressionAndDeathPersistentDataOfCurrentState(saveAsDeath: true, saveAsQuit: false);
	}

	public void ApplyCustomEndGame(RainWorldGame game, bool addFiveCycles)
	{
		Custom.Log("---- CUSTOM END GAME! ");
		for (int i = 0; i < ((!addFiveCycles) ? 1 : 5); i++)
		{
			RainCycleTick(game, depleteSwarmRoom: false);
		}
		if (game.IsStorySession)
		{
			game.GetStorySession.saveState.justBeatGame = false;
		}
		BringUpToDate(game);
		deathPersistentSaveData.karma = deathPersistentSaveData.karmaCap;
		deathPersistentSaveData.karmaFlowerPosition = null;
		deathPersistentSaveData.winState.ConsumeEndGame();
		food = SlugcatStats.SlugcatFoodMeter(saveStateNumber).x - SlugcatStats.SlugcatFoodMeter(saveStateNumber).y;
		game.rainWorld.progression.miscProgressionData.starvationTutorialCounter = Math.Min(game.rainWorld.progression.miscProgressionData.starvationTutorialCounter, 5);
		game.rainWorld.progression.SaveWorldStateAndProgression(malnourished: false);
	}

	public void RainCycleTick(RainWorldGame game, bool depleteSwarmRoom)
	{
		Custom.Log("-------- Global world ticking forward one sleep cycle!");
		if (depleteSwarmRoom)
		{
			DepleteOneSwarmRoom(game);
		}
		game.session.creatureCommunities.CycleTick(cycleNumber, saveStateNumber);
		deathPersistentSaveData.TickFlowerDepletion(1);
		for (int num = waitRespawnCreatures.Count - 1; num >= 0; num--)
		{
			bool flag = UnityEngine.Random.value < 1f / 3f;
			if (saveStateNumber == SlugcatStats.Name.Red || (ModManager.MSC && (saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Artificer || saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Spear)))
			{
				flag = UnityEngine.Random.value < 0.5f;
			}
			if (ModManager.MSC && game.world != null && game.world.region != null && game.world.region.name == "HR")
			{
				flag = true;
			}
			if (flag)
			{
				respawnCreatures.Add(waitRespawnCreatures[num]);
				waitRespawnCreatures.RemoveAt(num);
			}
		}
		cycleNumber++;
		if (miscWorldSaveData.SSaiConversationsHad > 0)
		{
			miscWorldSaveData.cyclesSinceSSai++;
		}
	}

	public void AppendCycleToStatistics(Player player, StoryGameSession session, bool death, int playerIndex)
	{
		AppendKills(session.playerSessionRecords[player.playerState.playerNumber].kills);
		if (playerIndex == 0)
		{
			session.AppendTimeOnCycleEnd(death);
			if (death)
			{
				deathPersistentSaveData.deaths++;
			}
		}
	}

	public void TrySetVanillaDen(string roomName)
	{
		string text = "";
		if (roomName.Contains("_"))
		{
			text = roomName.Split('_')[0];
		}
		if (File.Exists(Custom.RootFolderDirectory() + (Path.DirectorySeparatorChar + "World" + Path.DirectorySeparatorChar + text + "-Rooms" + Path.DirectorySeparatorChar + roomName + ".txt").ToLowerInvariant()))
		{
			lastVanillaDen = roomName;
		}
		else if (File.Exists(Custom.RootFolderDirectory() + (Path.DirectorySeparatorChar + "World" + Path.DirectorySeparatorChar + "Gate Shelters" + Path.DirectorySeparatorChar + roomName + ".txt").ToLowerInvariant()))
		{
			lastVanillaDen = roomName;
		}
	}

	public string GetSaveStateDenToUse()
	{
		if (RainWorld.roomNameToIndex.ContainsKey(denPosition))
		{
			return denPosition;
		}
		if (RainWorld.roomNameToIndex.ContainsKey(lastVanillaDen))
		{
			return lastVanillaDen;
		}
		return GetFinalFallbackShelter(saveStateNumber);
	}

	public static string GetFinalFallbackShelter(SlugcatStats.Name saveStateNumber)
	{
		if (!(saveStateNumber == SlugcatStats.Name.Red))
		{
			return "SU_S01";
		}
		return "LF_S02";
	}

	public void BringUpToDate(RainWorldGame game)
	{
		Custom.Log("----Adapt save state to world");
		AbstractCreature abstractCreature = game.FirstAlivePlayer;
		if (abstractCreature == null)
		{
			abstractCreature = game.FirstAnyPlayer;
		}
		WorldCoordinate pos = abstractCreature.pos;
		if (pos == default(WorldCoordinate))
		{
			Custom.LogWarning("(SaveState.BringUpToDate) Player Pos was null!!");
		}
		AbstractRoom abstractRoom = game.world.GetAbstractRoom(pos);
		if (pos == default(WorldCoordinate) && abstractRoom == null)
		{
			for (int i = 0; i < game.Players.Count; i++)
			{
				if (game.Players[i] != null && game.Players[i].pos != default(WorldCoordinate) && game.world.GetAbstractRoom(game.Players[i].pos) != null)
				{
					pos = game.Players[i].pos;
					abstractRoom = game.world.GetAbstractRoom(pos);
					break;
				}
			}
		}
		denPosition = abstractRoom.name;
		TrySetVanillaDen(abstractRoom.name);
		nextIssuedID = game.nextIssuedId;
		List<string> list = new List<string>();
		for (int j = 0; j < game.session.Players.Count; j++)
		{
			if (!(game.session.Players[j].realizedCreature is Player player))
			{
				continue;
			}
			for (int k = 0; k < player.grasps.Length; k++)
			{
				if (player.grasps[k] != null && player.grasps[k].grabbed != null)
				{
					AbstractPhysicalObject abstractPhysicalObject = player.grasps[k].grabbed.abstractPhysicalObject;
					if (abstractPhysicalObject is AbstractCreature critter)
					{
						list.Add(AbstractCreatureToStringStoryWorld(critter));
					}
					else
					{
						list.Add(abstractPhysicalObject.ToString());
					}
				}
			}
		}
		if (list.Count > 0)
		{
			playerGrasps = list.ToArray();
		}
		else
		{
			playerGrasps = null;
		}
		game.world.regionState.AdaptRegionStateToWorld(pos.room, -1);
		creatureCommunitiesString = game.session.creatureCommunities.ToString();
		bool flag = false;
		for (int l = 0; l < game.session.Players.Count; l++)
		{
			if ((game.session.Players[l].realizedCreature as Player)?.objectInStomach != null || (ModManager.CoopAvailable && ((PlayerState)game.session.Players[l].state)?.swallowedItem != null))
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			swallowedItems = new string[game.session.Players.Count];
			for (int m = 0; m < game.session.Players.Count; m++)
			{
				if (game.session.Players[m].realizedCreature is Player { objectInStomach: not null } player2)
				{
					if (player2.objectInStomach is AbstractCreature abstractCreature2)
					{
						if (game.world.GetAbstractRoom(abstractCreature2.pos.room) == null)
						{
							abstractCreature2.pos = player2.coord;
						}
						swallowedItems[m] = AbstractCreatureToStringStoryWorld(abstractCreature2);
					}
					else
					{
						swallowedItems[m] = player2.objectInStomach.ToString();
					}
				}
				else
				{
					swallowedItems[m] = "0";
				}
				PlayerState playerState = game.session.Players[m].state as PlayerState;
				if (ModManager.CoopAvailable && playerState != null && swallowedItems[m].Equals("0") && playerState.swallowedItem != null)
				{
					swallowedItems[m] = playerState.swallowedItem;
					playerState.swallowedItem = null;
				}
			}
		}
		else
		{
			swallowedItems = null;
		}
	}

	public string SaveToString()
	{
		Custom.Log("SAVE!");
		string text = "";
		text += string.Format(CultureInfo.InvariantCulture, "SAV STATE NUMBER<svB>{0}<svA>", saveStateNumber);
		text += string.Format(CultureInfo.InvariantCulture, "SEED<svB>{0}<svA>", seed);
		text += string.Format(CultureInfo.InvariantCulture, "VERSION<svB>{0}<svA>", gameVersion);
		text += string.Format(CultureInfo.InvariantCulture, "INITVERSION<svB>{0}<svA>", initiatedInGameVersion);
		text += string.Format(CultureInfo.InvariantCulture, "WORLDVERSION<svB>{0}<svA>", worldVersion);
		text = text + "DENPOS<svB>" + denPosition + "<svA>";
		text = text + "LASTVDENPOS<svB>" + lastVanillaDen + "<svA>";
		text += string.Format(CultureInfo.InvariantCulture, "CYCLENUM<svB>{0}<svA>", cycleNumber);
		text += string.Format(CultureInfo.InvariantCulture, "FOOD<svB>{0}<svA>", food);
		text += string.Format(CultureInfo.InvariantCulture, "NEXTID<svB>{0}<svA>", nextIssuedID);
		if (theGlow)
		{
			text += "HASTHEGLOW<svA>";
		}
		if (guideOverseerDead)
		{
			text += "GUIDEOVERSEERDEAD<svA>";
		}
		if (!progression.rainWorld.setup.cleanSpawns)
		{
			if (respawnCreatures.Count > 0)
			{
				text += "RESPAWNS<svB>";
				for (int i = 0; i < respawnCreatures.Count; i++)
				{
					text += string.Format(CultureInfo.InvariantCulture, "{0}{1}", respawnCreatures[i], (i < respawnCreatures.Count - 1) ? "." : "");
				}
				text += "<svA>";
			}
			if (waitRespawnCreatures.Count > 0)
			{
				text += "WAITRESPAWNS<svB>";
				for (int j = 0; j < waitRespawnCreatures.Count; j++)
				{
					text += string.Format(CultureInfo.InvariantCulture, "{0}{1}", waitRespawnCreatures[j], (j < waitRespawnCreatures.Count - 1) ? "." : "");
				}
				text += "<svA>";
			}
			if (creatureCommunitiesString != null)
			{
				text = text + "COMMUNITIES<svB>" + creatureCommunitiesString + "<svA>";
			}
			for (int k = 0; k < regionStates.Length; k++)
			{
				if (regionStates[k] != null)
				{
					text = text + "REGIONSTATE<svB>" + regionStates[k].SaveToString() + "<svA>";
				}
				else if (regionLoadStrings[k] != null)
				{
					text = text + "REGIONSTATE<svB>" + regionLoadStrings[k] + "<svA>";
				}
			}
			for (int l = 0; l < unrecognizedRegionLoadStrings.Count; l++)
			{
				text = text + "REGIONSTATE<svB>" + unrecognizedRegionLoadStrings[l] + "<svA>";
			}
		}
		if (swallowedItems != null)
		{
			text += "SWALLOWEDITEMS<svB>";
			for (int m = 0; m < swallowedItems.Length; m++)
			{
				text = text + swallowedItems[m] + ((m < swallowedItems.Length - 1) ? "<svB>" : "");
			}
			text += "<svA>";
		}
		if (unrecognizedSwallowedItems != null && unrecognizedSwallowedItems.Count > 0)
		{
			text += "UNRECOGNIZEDSWALLOWED<svB>";
			for (int n = 0; n < unrecognizedSwallowedItems.Count; n++)
			{
				text = text + unrecognizedSwallowedItems[n] + ((n < unrecognizedSwallowedItems.Count - 1) ? "<svB>" : "");
			}
			text += "<svA>";
		}
		if (playerGrasps != null)
		{
			text += "PLAYERGRASPS<svB>";
			for (int num = 0; num < playerGrasps.Length; num++)
			{
				text = text + playerGrasps[num] + ((num < playerGrasps.Length - 1) ? "<svB>" : "");
			}
			text += "<svA>";
		}
		if (unrecognizedPlayerGrasps != null && unrecognizedPlayerGrasps.Count > 0)
		{
			text += "UNRECOGNIZEDPLAYERGRASPS<svB>";
			for (int num2 = 0; num2 < unrecognizedPlayerGrasps.Count; num2++)
			{
				text = text + unrecognizedPlayerGrasps[num2] + ((num2 < unrecognizedPlayerGrasps.Count - 1) ? "<svB>" : "");
			}
			text += "<svA>";
		}
		text = text + "DEATHPERSISTENTSAVEDATA<svB>" + deathPersistentSaveData.SaveToString(saveAsIfPlayerDied: false, saveAsIfPlayerQuit: false) + "<svA>";
		string text2 = miscWorldSaveData.ToString();
		if (text2 != "")
		{
			text = text + "MISCWORLDSAVEDATA<svB>" + text2 + "<svA>";
		}
		if (dreamsState != null)
		{
			text = text + "DREAMSSTATE<svB>" + dreamsState.ToString() + "<svA>";
		}
		text += string.Format(CultureInfo.InvariantCulture, "TOTFOOD<svB>{0}<svA>", totFood);
		text += string.Format(CultureInfo.InvariantCulture, "TOTTIME<svB>{0}<svA>", totTime);
		text += string.Format(CultureInfo.InvariantCulture, "CURRVERCYCLES<svB>{0}<svA>", cyclesInCurrentWorldVersion);
		if (kills.Count > 0)
		{
			text += "KILLS<svB>";
			for (int num3 = 0; num3 < kills.Count; num3++)
			{
				text += string.Format(CultureInfo.InvariantCulture, "{0}<svD>{1}{2}", kills[num3].Key, kills[num3].Value, (num3 < kills.Count - 1) ? "<svC>" : "");
			}
			for (int num4 = 0; num4 < unrecognizedKills.Count; num4++)
			{
				text = text + "<svC>" + unrecognizedKills[num4];
			}
			text += "<svA>";
		}
		if (redExtraCycles)
		{
			text += "REDEXTRACYCLES<svA>";
		}
		if (justBeatGame)
		{
			text += "JUSTBEATGAME<svA>";
		}
		if (ModManager.MSC)
		{
			if (hasRobo)
			{
				text += "HASROBO<svA>";
			}
			if (wearingCloak)
			{
				text += "CLOAK<svA>";
			}
			if (karmaDream)
			{
				text += "KARMADREAM<svA>";
			}
			text += string.Format(CultureInfo.InvariantCulture, "FORCEPUPS<svB>{0}<svA>", forcePupsNextCycle);
		}
		if (ModManager.MMF)
		{
			if (objectTrackers.Count > 0)
			{
				text += "OBJECTTRACKERS<svB>";
				for (int num5 = 0; num5 < objectTrackers.Count; num5++)
				{
					if (progression.rainWorld.processManager.currentMainLoop.manager.menuSetup.FastTravelInitCondition && MMF.cfgKeyItemPassaging.Value && objectTrackers[num5].lastSeenRoom == RainWorld.ShelterBeforePassage)
					{
						objectTrackers[num5].ChangeDesiredSpawnLocation(new WorldCoordinate((progression.rainWorld.processManager.currentMainLoop as RainWorldGame).world.GetAbstractRoom(RainWorld.ShelterAfterPassage).index, 0, 0, -1));
						objectTrackers[num5].lastSeenRoom = RainWorld.ShelterAfterPassage;
						objectTrackers[num5].lastSeenRegion = RainWorld.ShelterAfterPassage.Substring(0, 2);
					}
					text += objectTrackers[num5].ToString();
					if (num5 < objectTrackers.Count - 1)
					{
						text += "<svC>";
					}
				}
				text += "<svA>";
			}
			if (!progression.rainWorld.setup.cleanSpawns)
			{
				if (pendingObjects.Count > 0)
				{
					text += "OBJECTS<svB>";
					for (int num6 = 0; num6 < pendingObjects.Count; num6++)
					{
						text = text + pendingObjects[num6] + "<svC>";
					}
					text += "<svA>";
				}
				if (pendingFriendCreatures.Count > 0)
				{
					text += "FRIENDS<svB>";
					for (int num7 = 0; num7 < pendingFriendCreatures.Count; num7++)
					{
						text = text + pendingFriendCreatures[num7] + "<svC>";
					}
					text += "<svA>";
				}
				if (oeEncounters.Count > 0)
				{
					text += "OEENCOUNTERS<svB>";
					for (int num8 = 0; num8 < oeEncounters.Count; num8++)
					{
						text = text + oeEncounters[num8] + "<svC>";
					}
					text += "<svA>";
				}
			}
		}
		foreach (string unrecognizedSaveString in unrecognizedSaveStrings)
		{
			text = text + unrecognizedSaveString + "<svA>";
		}
		return text;
	}

	public void LoadGame(string str, RainWorldGame game)
	{
		loaded = true;
		redExtraCycles = false;
		initiatedInGameVersion = 0;
		unrecognizedSaveStrings.Clear();
		unrecognizedRegionLoadStrings.Clear();
		unrecognizedKills.Clear();
		unrecognizedSwallowedItems.Clear();
		unrecognizedPlayerGrasps.Clear();
		RainWorld.loadedWorldVersion = RainWorld.worldVersion;
		if (str == "" || progression.rainWorld.safariMode)
		{
			Custom.LogImportant("NOTHING TO LOAD - START CLEAR");
			setDenPosition();
		}
		else
		{
			string[] array = Regex.Split(str, "<svA>");
			List<string[]> list = new List<string[]>();
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = Regex.Split(array[i], "<svB>");
				if (array2.Length != 0 && array2[0].Length > 0)
				{
					list.Add(array2);
				}
			}
			for (int j = 0; j < list.Count; j++)
			{
				switch (list[j][0])
				{
				case "DENPOS":
					if (progression.rainWorld.setup.startMap != "")
					{
						denPosition = progression.rainWorld.setup.startMap;
					}
					else if (ModManager.MSC && progression.rainWorld.safariMode)
					{
						SetDenPositionForSafari();
					}
					else
					{
						denPosition = list[j][1];
					}
					break;
				case "LASTVDENPOS":
					lastVanillaDen = list[j][1];
					break;
				case "CYCLENUM":
					cycleNumber = int.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case "FOOD":
					food = int.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case "NEXTID":
					if (game != null)
					{
						game.nextIssuedId = int.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
					}
					break;
				case "HASTHEGLOW":
					theGlow = true;
					break;
				case "GUIDEOVERSEERDEAD":
					guideOverseerDead = true;
					break;
				case "RESPAWNS":
				{
					string[] array7 = list[j][1].Split('.');
					for (int num4 = 0; num4 < array7.Length; num4++)
					{
						if (num4 < array7.Length && array7[num4] != string.Empty)
						{
							respawnCreatures.Add(int.Parse(array7[num4], NumberStyles.Any, CultureInfo.InvariantCulture));
						}
					}
					break;
				}
				case "WAITRESPAWNS":
				{
					string[] array8 = list[j][1].Split('.');
					for (int num5 = 0; num5 < array8.Length; num5++)
					{
						if (num5 < array8.Length && array8[num5] != string.Empty)
						{
							waitRespawnCreatures.Add(int.Parse(array8[num5], NumberStyles.Any, CultureInfo.InvariantCulture));
						}
					}
					break;
				}
				case "REGIONSTATE":
				{
					string[] array10 = Regex.Split(list[j][1], "<rgA>");
					for (int num8 = 0; num8 < array10.Length; num8++)
					{
						if (!(Regex.Split(array10[num8], "<rgB>")[0] == "REGIONNAME"))
						{
							continue;
						}
						bool flag = false;
						for (int num9 = 0; num9 < progression.regionNames.Length; num9++)
						{
							if (progression.regionNames[num9] == Regex.Split(array10[num8], "<rgB>")[1])
							{
								regionLoadStrings[num9] = list[j][1];
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							unrecognizedRegionLoadStrings.Add(list[j][1]);
						}
						break;
					}
					break;
				}
				case "COMMUNITIES":
					creatureCommunitiesString = list[j][1];
					break;
				case "MISCWORLDSAVEDATA":
					miscWorldSaveData.FromString(list[j][1]);
					break;
				case "DEATHPERSISTENTSAVEDATA":
					deathPersistentSaveData.FromString(list[j][1]);
					if (saveStateNumber == SlugcatStats.Name.Yellow)
					{
						deathPersistentSaveData.howWellIsPlayerDoing = -1f;
					}
					else if (saveStateNumber == SlugcatStats.Name.Red)
					{
						deathPersistentSaveData.howWellIsPlayerDoing = 1f;
					}
					break;
				case "SWALLOWEDITEMS":
				{
					List<string> list3 = new List<string>();
					for (int num6 = 1; num6 < list[j].Length; num6++)
					{
						string text2 = list[j][num6];
						if (text2.Contains("<oA>"))
						{
							AbstractPhysicalObject abstractPhysicalObject2 = AbstractPhysicalObjectFromString(null, text2);
							if (abstractPhysicalObject2 == null || abstractPhysicalObject2.type.Index == -1)
							{
								list3.Add("");
								unrecognizedSwallowedItems.Add(text2);
							}
							else
							{
								list3.Add(text2);
							}
						}
						else if (text2.Contains("<cA>"))
						{
							if (AbstractCreatureFromString(null, text2, onlyInCurrentRegion: false) == null)
							{
								list3.Add("");
								unrecognizedSwallowedItems.Add(text2);
							}
							else
							{
								list3.Add(text2);
							}
						}
						else
						{
							list3.Add("");
							unrecognizedSwallowedItems.Add(text2);
						}
					}
					swallowedItems = list3.ToArray();
					break;
				}
				case "UNRECOGNIZEDSWALLOWED":
				{
					for (int num3 = 1; num3 < list[j].Length; num3++)
					{
						unrecognizedSwallowedItems.Add(list[j][num3]);
					}
					break;
				}
				case "PLAYERGRASPS":
				{
					List<string> list2 = new List<string>();
					for (int num = 1; num < list[j].Length; num++)
					{
						string text = list[j][num];
						if (text.Contains("<oA>"))
						{
							AbstractPhysicalObject abstractPhysicalObject = AbstractPhysicalObjectFromString(null, text);
							if (abstractPhysicalObject == null || abstractPhysicalObject.type.Index == -1)
							{
								unrecognizedPlayerGrasps.Add(text);
							}
							else
							{
								list2.Add(text);
							}
						}
						else if (text.Contains("<cA>"))
						{
							if (AbstractCreatureFromString(null, text, onlyInCurrentRegion: false) == null)
							{
								unrecognizedPlayerGrasps.Add(text);
							}
							else
							{
								list2.Add(text);
							}
						}
						else
						{
							unrecognizedPlayerGrasps.Add(text);
						}
					}
					playerGrasps = list2.ToArray();
					break;
				}
				case "UNRECOGNIZEDPLAYERGRASPS":
				{
					for (int l = 1; l < list[j].Length; l++)
					{
						unrecognizedPlayerGrasps.Add(list[j][l]);
					}
					break;
				}
				case "VERSION":
					gameVersion = int.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case "INITVERSION":
					initiatedInGameVersion = int.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case "WORLDVERSION":
					worldVersion = int.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
					RainWorld.loadedWorldVersion = worldVersion;
					break;
				case "SEED":
					seed = int.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case "DREAMSSTATE":
					if (dreamsState != null)
					{
						dreamsState.FromString(list[j][1]);
					}
					break;
				case "TOTFOOD":
					totFood = int.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case "TOTTIME":
					totTime = int.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case "CURRVERCYCLES":
					cyclesInCurrentWorldVersion = int.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case "KILLS":
				{
					kills.Clear();
					string[] array9 = Regex.Split(list[j][1], "<svC>");
					for (int num7 = 0; num7 < array9.Length; num7++)
					{
						if (!(array9[num7] == string.Empty))
						{
							IconSymbol.IconSymbolData key = IconSymbol.IconSymbolData.IconSymbolDataFromString(Regex.Split(array9[num7], "<svD>")[0]);
							if ((key.critType != null && key.critType.Index == -1) || (key.itemType != null && key.itemType.Index == -1))
							{
								unrecognizedKills.Add(array9[num7]);
							}
							else
							{
								kills.Add(new KeyValuePair<IconSymbol.IconSymbolData, int>(key, int.Parse(Regex.Split(array9[num7], "<svD>")[1], NumberStyles.Any, CultureInfo.InvariantCulture)));
							}
						}
					}
					break;
				}
				case "REDEXTRACYCLES":
					redExtraCycles = true;
					break;
				case "JUSTBEATGAME":
					justBeatGame = true;
					break;
				case "HASROBO":
					if (ModManager.MSC)
					{
						hasRobo = true;
					}
					else
					{
						AddUnrecognized(list[j]);
					}
					break;
				case "CLOAK":
					if (ModManager.MSC)
					{
						wearingCloak = true;
					}
					else
					{
						AddUnrecognized(list[j]);
					}
					break;
				case "KARMADREAM":
					if (ModManager.MSC)
					{
						karmaDream = true;
					}
					else
					{
						AddUnrecognized(list[j]);
					}
					break;
				case "FORCEPUPS":
					if (ModManager.MSC)
					{
						forcePupsNextCycle = int.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
					}
					else
					{
						AddUnrecognized(list[j]);
					}
					break;
				case "OBJECTTRACKERS":
					if (ModManager.MMF && MMF.cfgKeyItemTracking.Value)
					{
						objectTrackers.Clear();
						string[] array6 = Regex.Split(list[j][1], "<svC>");
						for (int num2 = 0; num2 < array6.Length; num2++)
						{
							if (!(array6[num2] == string.Empty))
							{
								PersistentObjectTracker persistentObjectTracker = new PersistentObjectTracker(null);
								persistentObjectTracker.FromString(array6[num2], (game != null && game.IsStorySession) ? game.GetStorySession.saveStateNumber : SlugcatStats.Name.White);
								objectTrackers.Add(persistentObjectTracker);
							}
						}
					}
					else
					{
						AddUnrecognized(list[j]);
					}
					break;
				case "OBJECTS":
				{
					string[] array5 = Regex.Split(list[j][1], "<svC>");
					for (int n = 0; n < array5.Length; n++)
					{
						if (array5[n] != string.Empty)
						{
							pendingObjects.Add(array5[n]);
						}
					}
					break;
				}
				case "FRIENDS":
				{
					string[] array4 = Regex.Split(list[j][1], "<svC>");
					for (int m = 0; m < array4.Length; m++)
					{
						if (array4[m] != string.Empty)
						{
							pendingFriendCreatures.Add(array4[m]);
						}
					}
					break;
				}
				case "OEENCOUNTERS":
					if (ModManager.MMF)
					{
						string[] array3 = Regex.Split(list[j][1], "<svC>");
						for (int k = 0; k < array3.Length; k++)
						{
							if (array3[k] != string.Empty)
							{
								oeEncounters.Add(array3[k]);
							}
						}
					}
					else
					{
						AddUnrecognized(list[j]);
					}
					break;
				default:
					AddUnrecognized(list[j]);
					break;
				case "SAV STATE NUMBER":
					break;
				}
			}
		}
		if (game != null)
		{
			if (game.setupValues.cheatKarma > 0)
			{
				deathPersistentSaveData.karma = game.setupValues.cheatKarma - 1;
				deathPersistentSaveData.karmaCap = Math.Max(deathPersistentSaveData.karmaCap, deathPersistentSaveData.karma);
			}
			if (game.setupValues.theMark)
			{
				deathPersistentSaveData.theMark = true;
			}
			if (worldVersion < RainWorld.worldVersion)
			{
				if (worldVersion == 0)
				{
					game.manager.rainWorld.progression.miscProgressionData.redUnlocked = true;
				}
				for (int num10 = worldVersion + 1; num10 <= RainWorld.worldVersion; num10++)
				{
					BackwardsCompability.UpdateWorldVersion(this, num10, game.rainWorld.progression);
				}
				cyclesInCurrentWorldVersion = 0;
			}
		}
		else
		{
			Custom.LogWarning("LOADING SAV WITH NULL GAME");
		}
		if (deathPersistentSaveData.redsDeath && cycleNumber < RedsIllness.RedsCycles(redExtraCycles))
		{
			deathPersistentSaveData.redsDeath = false;
		}
		WarpDenPosition(game);
	}

	public void AddUnrecognized(string[] lines)
	{
		if (lines[0].Trim().Length > 0 && lines.Length >= 2)
		{
			unrecognizedSaveStrings.Add(lines[0] + "<svB>" + lines[1]);
		}
		else if (lines[0].Trim().Length > 0 && lines.Length >= 1)
		{
			unrecognizedSaveStrings.Add(lines[0]);
		}
	}

	public void AppendKills(List<PlayerSessionRecord.KillRecord> recordKills)
	{
		Custom.Log("appending", recordKills.Count.ToString(), "kills to kills record");
		for (int i = 0; i < recordKills.Count; i++)
		{
			bool flag = true;
			for (int j = 0; j < kills.Count; j++)
			{
				if (kills[j].Key == recordKills[i].symbolData)
				{
					kills[j] = new KeyValuePair<IconSymbol.IconSymbolData, int>(kills[j].Key, kills[j].Value + 1);
					flag = false;
					break;
				}
			}
			if (flag)
			{
				kills.Add(new KeyValuePair<IconSymbol.IconSymbolData, int>(recordKills[i].symbolData, 1));
			}
		}
	}

	public void AddCreatureToRespawn(AbstractCreature critter)
	{
		Custom.Log($"CRITTER TO RESPAWN : {critter}");
		if (critter.ID.spawner == -1)
		{
			Custom.LogWarning("critter has no spawner (-1). not adding to respawn list.");
		}
		else if (critter.realizedCreature != null && critter.realizedCreature.killTag != null && critter.realizedCreature.killTag.creatureTemplate.type == CreatureTemplate.Type.Slugcat)
		{
			Custom.Log("killed by player");
			if (critter.world.GetSpawner(critter.ID) != null && critter.world.GetSpawner(critter.ID) is World.Lineage)
			{
				(critter.world.GetSpawner(critter.ID) as World.Lineage).ChanceToProgress(critter.world);
			}
			waitRespawnCreatures.Add(critter.ID.spawner);
			Custom.Log("add crit to waiting list");
		}
		else
		{
			respawnCreatures.Add(critter.ID.spawner);
			Custom.Log("not killed by player - immediate respawn");
		}
	}

	public void GhostEncounter(GhostWorldPresence.GhostID ghost, RainWorld rainWorld)
	{
		Custom.Log($"Save state ghost encounter! {ghost}");
		deathPersistentSaveData.ghostsTalkedTo[ghost] = 2;
		int num = 0;
		foreach (KeyValuePair<GhostWorldPresence.GhostID, int> item in deathPersistentSaveData.ghostsTalkedTo)
		{
			if (item.Value > 1)
			{
				num++;
			}
		}
		if (deathPersistentSaveData.pebblesHasIncreasedRedsKarmaCap)
		{
			num++;
		}
		int num2 = SlugcatStats.SlugcatStartingKarma(saveStateNumber);
		while (num2 < 9 && num > 0)
		{
			num2++;
			if (num2 == 5)
			{
				num2++;
			}
			num--;
		}
		if (num2 >= deathPersistentSaveData.karmaCap)
		{
			deathPersistentSaveData.karmaCap = num2;
		}
		if (ModManager.MSC && saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Artificer && deathPersistentSaveData.altEnding)
		{
			deathPersistentSaveData.karmaCap = 0;
		}
		if (ModManager.Expedition && rainWorld.ExpeditionMode)
		{
			num2 = 0;
		}
		deathPersistentSaveData.karma = deathPersistentSaveData.karmaCap;
		if (ModManager.MSC)
		{
			deathPersistentSaveData.winState.UpdateGhostTracker(this, deathPersistentSaveData.winState.GetTracker(MoreSlugcatsEnums.EndgameID.Pilgrim, addIfMissing: true) as WinState.BoolArrayTracker);
		}
		rainWorld.progression.SaveProgressionAndDeathPersistentDataOfCurrentState(saveAsDeath: false, saveAsQuit: false);
	}

	public void IncreaseKarmaCapOneStep()
	{
		if (deathPersistentSaveData.karmaCap == 4)
		{
			deathPersistentSaveData.karmaCap = 6;
		}
		else if (deathPersistentSaveData.karmaCap < 9)
		{
			deathPersistentSaveData.karmaCap++;
		}
		deathPersistentSaveData.karma = deathPersistentSaveData.karmaCap;
	}

	private void DepleteOneSwarmRoom(RainWorldGame game)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < regionStates.Length; i++)
		{
			if (regionStates[i] != null && regionStates[i].candidatesForDepleteSwarmRooms.Count > 0 && game.world.regionState != regionStates[i])
			{
				list.Add(i);
			}
		}
		if (list.Count == 0)
		{
			list.Add(-1);
		}
		int num = list[UnityEngine.Random.Range(0, list.Count)];
		RegionState regionState = game.world.regionState;
		if (num >= 0 && num < regionStates.Length)
		{
			regionState = regionStates[num];
		}
		if (regionState.candidatesForDepleteSwarmRooms.Count <= 0)
		{
			return;
		}
		int num2 = 0;
		if (regionState != null)
		{
			foreach (KeyValuePair<string, int> swarmRoomCounter in regionState.swarmRoomCounters)
			{
				if (regionState.SwarmRoomActive(swarmRoomCounter.Key))
				{
					num2++;
				}
			}
		}
		string key = regionState.candidatesForDepleteSwarmRooms[UnityEngine.Random.Range(0, regionState.candidatesForDepleteSwarmRooms.Count)];
		regionState.swarmRoomCounters[key] = game.world.region.CyclesToDepleteASwarmRoom(num2);
	}

	public void ReportConsumedItem(World world, bool karmaFlower, int originroom, int placedObjectIndex, int waitCycles)
	{
		Custom.Log("Item consumed. Flower:", karmaFlower.ToString());
		if (!world.singleRoomWorld && originroom >= 0 && placedObjectIndex >= 0)
		{
			if (karmaFlower)
			{
				deathPersistentSaveData.ReportConsumedFlower(originroom, placedObjectIndex, waitCycles);
			}
			else if (originroom >= world.firstRoomIndex && originroom < world.firstRoomIndex + world.NumberOfRooms && regionStates[world.region.regionNumber] != null)
			{
				regionStates[world.region.regionNumber].ReportConsumedItem(originroom, placedObjectIndex, waitCycles);
			}
		}
	}

	public bool ItemConsumed(World world, bool karmaFlower, int originroom, int placedObjectIndex)
	{
		if (world.singleRoomWorld)
		{
			return false;
		}
		if (originroom < 0 || placedObjectIndex < 0)
		{
			return false;
		}
		if (karmaFlower)
		{
			return deathPersistentSaveData.FlowerConsumed(originroom, placedObjectIndex);
		}
		if (originroom < world.firstRoomIndex || originroom >= world.firstRoomIndex + world.NumberOfRooms)
		{
			return false;
		}
		if (regionStates[world.region.regionNumber] == null)
		{
			return false;
		}
		return regionStates[world.region.regionNumber].ItemConsumed(originroom, placedObjectIndex);
	}

	public static AbstractPhysicalObject AbstractPhysicalObjectFromString(World world, string objString)
	{
		try
		{
			string[] array = Regex.Split(objString, "<oA>");
			EntityID iD = EntityID.FromString(array[0]);
			AbstractPhysicalObject.AbstractObjectType abstractObjectType = new AbstractPhysicalObject.AbstractObjectType(array[1]);
			WorldCoordinate pos = WorldCoordinate.FromString(array[2]);
			AbstractPhysicalObject abstractPhysicalObject = null;
			if (abstractObjectType == AbstractPhysicalObject.AbstractObjectType.Spear)
			{
				int fromIndex = 5;
				AbstractSpear abstractSpear;
				if (!ModManager.MSC || array.Length == 5)
				{
					abstractSpear = new AbstractSpear(world, null, pos, iD, array[4] == "1");
				}
				else if (array.Length > 6 && array[6] == "1")
				{
					abstractSpear = new AbstractSpear(world, null, pos, iD, array[4] == "1", array[6] == "1");
					fromIndex = 7;
				}
				else
				{
					abstractSpear = new AbstractSpear(world, null, pos, iD, array[4] == "1", float.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture));
					fromIndex = 6;
				}
				abstractSpear.stuckInWallCycles = int.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
				if (ModManager.MSC && array.Length > 7)
				{
					abstractSpear.electricCharge = int.Parse(array[7], NumberStyles.Any, CultureInfo.InvariantCulture);
					fromIndex = 8;
				}
				if (ModManager.MSC && array.Length > 8)
				{
					abstractSpear.needle = array[8] == "1";
					fromIndex = 9;
				}
				abstractPhysicalObject = abstractSpear;
				abstractPhysicalObject.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, fromIndex);
			}
			else if (abstractObjectType == AbstractPhysicalObject.AbstractObjectType.WaterNut)
			{
				abstractPhysicalObject = new WaterNut.AbstractWaterNut(world, null, pos, iD, int.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture), int.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture), null, array[5] == "1");
				abstractPhysicalObject.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 6);
			}
			else if (abstractObjectType == AbstractPhysicalObject.AbstractObjectType.PebblesPearl)
			{
				abstractPhysicalObject = new PebblesPearl.AbstractPebblesPearl(world, null, pos, iD, int.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture), int.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture), null, int.Parse(array[6], NumberStyles.Any, CultureInfo.InvariantCulture), int.Parse(array[7], NumberStyles.Any, CultureInfo.InvariantCulture));
				abstractPhysicalObject.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 8);
			}
			else if (abstractObjectType == AbstractPhysicalObject.AbstractObjectType.DataPearl)
			{
				DataPearl.AbstractDataPearl.DataPearlType dataPearlType = new DataPearl.AbstractDataPearl.DataPearlType(array[5]);
				if (int.TryParse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
				{
					dataPearlType = BackwardsCompatibilityRemix.ParseDataPearl(result);
				}
				abstractPhysicalObject = new DataPearl.AbstractDataPearl(world, AbstractPhysicalObject.AbstractObjectType.DataPearl, null, pos, iD, int.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture), int.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture), null, dataPearlType);
				abstractPhysicalObject.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 6);
			}
			else if (abstractObjectType == AbstractPhysicalObject.AbstractObjectType.SporePlant)
			{
				abstractPhysicalObject = new SporePlant.AbstractSporePlant(world, null, pos, iD, int.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture), int.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture), null, array[5] == "1", array[6] == "1");
				abstractPhysicalObject.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 7);
			}
			else if (abstractObjectType == AbstractPhysicalObject.AbstractObjectType.EggBugEgg)
			{
				abstractPhysicalObject = new EggBugEgg.AbstractBugEgg(world, null, pos, iD, float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture));
				abstractPhysicalObject.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 4);
			}
			else if (abstractObjectType == AbstractPhysicalObject.AbstractObjectType.BubbleGrass)
			{
				abstractPhysicalObject = new BubbleGrass.AbstractBubbleGrass(world, null, pos, iD, float.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture), int.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture), int.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture), null);
				abstractPhysicalObject.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 6);
			}
			else if (abstractObjectType == AbstractPhysicalObject.AbstractObjectType.VultureMask)
			{
				int fromIndex2 = 4;
				if (array.Length < 5)
				{
					abstractPhysicalObject = new VultureMask.AbstractVultureMask(world, null, pos, iD, iD.RandomSeed, king: false);
				}
				else if (!ModManager.MSC || array.Length < 6)
				{
					abstractPhysicalObject = new VultureMask.AbstractVultureMask(world, null, pos, iD, int.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture), array[4] == "1");
					fromIndex2 = 5;
				}
				else if (array.Length < 7)
				{
					abstractPhysicalObject = new VultureMask.AbstractVultureMask(world, null, pos, iD, int.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture), array[4] == "1", array[5] == "1", "");
					fromIndex2 = 6;
				}
				else
				{
					abstractPhysicalObject = new VultureMask.AbstractVultureMask(world, null, pos, iD, int.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture), array[4] == "1", array[5] == "1", array[6]);
					fromIndex2 = 7;
				}
				abstractPhysicalObject.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, fromIndex2);
			}
			else if (abstractObjectType == AbstractPhysicalObject.AbstractObjectType.OverseerCarcass)
			{
				int fromIndex3 = 7;
				abstractPhysicalObject = new OverseerCarcass.AbstractOverseerCarcass(world, null, pos, iD, new Color(float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture)), int.Parse(array[6], NumberStyles.Any, CultureInfo.InvariantCulture));
				if (ModManager.MSC && array.Length > 7)
				{
					(abstractPhysicalObject as OverseerCarcass.AbstractOverseerCarcass).InspectorMode = array[7] == "1";
					fromIndex3 = 8;
				}
				abstractPhysicalObject.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, fromIndex3);
			}
			else if (ModManager.MSC && abstractObjectType == MoreSlugcatsEnums.AbstractObjectType.Spearmasterpearl)
			{
				int fromIndex4 = 5;
				abstractPhysicalObject = new SpearMasterPearl.AbstractSpearMasterPearl(world, null, pos, iD, int.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture), int.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture), null);
				if (array.Length > 6)
				{
					(abstractPhysicalObject as SpearMasterPearl.AbstractSpearMasterPearl).broadcastTagged = array[6] == "1";
					fromIndex4 = 7;
				}
				abstractPhysicalObject.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, fromIndex4);
			}
			else if (ModManager.MSC && abstractObjectType == MoreSlugcatsEnums.AbstractObjectType.HalcyonPearl)
			{
				DataPearl.AbstractDataPearl.DataPearlType dataPearlType2 = new DataPearl.AbstractDataPearl.DataPearlType(array[5]);
				abstractPhysicalObject = new DataPearl.AbstractDataPearl(world, MoreSlugcatsEnums.AbstractObjectType.HalcyonPearl, null, pos, iD, int.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture), int.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture), null, dataPearlType2);
				abstractPhysicalObject.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 6);
			}
			else if (ModManager.MSC && abstractObjectType == MoreSlugcatsEnums.AbstractObjectType.JokeRifle)
			{
				JokeRifle.AbstractRifle.AmmoType ammoType = new JokeRifle.AbstractRifle.AmmoType(array[3]);
				abstractPhysicalObject = new JokeRifle.AbstractRifle(world, null, pos, iD, ammoType);
				(abstractPhysicalObject as JokeRifle.AbstractRifle).AmmoFromString(array[4]);
				abstractPhysicalObject.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 5);
			}
			else if (ModManager.MSC && abstractObjectType == MoreSlugcatsEnums.AbstractObjectType.LillyPuck)
			{
				abstractPhysicalObject = new LillyPuck.AbstractLillyPuck(world, null, pos, iD, int.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture), int.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture), int.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture), null);
				abstractPhysicalObject.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 6);
			}
			else if (ModManager.MSC && abstractObjectType == MoreSlugcatsEnums.AbstractObjectType.FireEgg)
			{
				abstractPhysicalObject = new FireEgg.AbstractBugEgg(world, null, pos, iD, float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture));
				(abstractPhysicalObject as FireEgg.AbstractBugEgg).stuckInWall = array[4] == "1";
				abstractPhysicalObject.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 5);
			}
			else if (ModManager.MSC && abstractObjectType == MoreSlugcatsEnums.AbstractObjectType.Bullet)
			{
				JokeRifle.AbstractRifle.AmmoType type = new JokeRifle.AbstractRifle.AmmoType(array[3]);
				abstractPhysicalObject = new AbstractBullet(world, null, pos, iD, type, int.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture));
				abstractPhysicalObject.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 5);
			}
			else if (abstractObjectType == AbstractPhysicalObject.AbstractObjectType.CollisionField)
			{
				CollisionField.Type fieldType = new CollisionField.Type(array[5]);
				abstractPhysicalObject = new CollisionField.AbstractCollisionField(world, null, pos, iD, fieldType, float.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture), int.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture));
				abstractPhysicalObject.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 6);
			}
			else if (AbstractConsumable.IsTypeConsumable(abstractObjectType))
			{
				abstractPhysicalObject = new AbstractConsumable(world, abstractObjectType, null, pos, iD, int.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture), int.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture), null);
				abstractPhysicalObject.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 5);
			}
			else
			{
				abstractPhysicalObject = new AbstractPhysicalObject(world, abstractObjectType, null, pos, iD);
				abstractPhysicalObject.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 3);
			}
			return abstractPhysicalObject;
		}
		catch (Exception ex)
		{
			Custom.LogWarning("[EXCEPTION] AbstractPhysicalObjectFromString:", objString, "--", ex.Message, "--", ex.StackTrace);
			return null;
		}
	}

	public static AbstractCreature AbstractCreatureFromString(World world, string creatureString, bool onlyInCurrentRegion)
	{
		string[] array = Regex.Split(creatureString, "<cA>");
		CreatureTemplate.Type type = new CreatureTemplate.Type(array[0]);
		if (type.Index == -1)
		{
			Custom.LogWarning("Unknown creature:", array[0], "creature not spawning");
			return null;
		}
		string[] array2 = array[2].Split('.');
		EntityID iD = EntityID.FromString(array[1]);
		int? num = BackwardsCompatibilityRemix.ParseRoomIndex(array2[0]);
		if (!num.HasValue)
		{
			Custom.LogWarning("Spawn room does not exist:", array2[0], "~", iD.spawner.ToString(), "creature not spawning");
			return null;
		}
		WorldCoordinate worldCoordinate = new WorldCoordinate(num.Value, -1, -1, int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture));
		if (onlyInCurrentRegion && (worldCoordinate.room < world.firstRoomIndex || worldCoordinate.room >= world.firstRoomIndex + world.NumberOfRooms))
		{
			Custom.LogWarning($"Creature trying to spawn out of region: {creatureString} r:{worldCoordinate.room} fr:{world.firstRoomIndex} lr:{world.firstRoomIndex + world.NumberOfRooms}");
			if (world.GetSpawner(iD) == null)
			{
				Custom.LogWarning("Spawner out of region", (world.region != null) ? world.region.name : "", "~", iD.spawner.ToString(), "creature not spawning");
				return null;
			}
			worldCoordinate = world.GetSpawner(iD).den;
			Custom.Log($"Spawner is in region. Moving to original spawn den : {worldCoordinate}");
		}
		AbstractCreature abstractCreature = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(type), null, worldCoordinate, iD);
		if (world != null)
		{
			abstractCreature.state.LoadFromString(Regex.Split(array[3], "<cB>"));
			if (abstractCreature.Room == null)
			{
				Custom.LogWarning("Spawn room does not exist:", array2[0], "~", iD.spawner.ToString(), "creature not spawning");
				return null;
			}
			abstractCreature.setCustomFlags();
		}
		return abstractCreature;
	}

	public static string AbstractCreatureToStringStoryWorld(AbstractCreature critter)
	{
		return AbstractCreatureToStringStoryWorld(critter, critter.pos);
	}

	public static string AbstractCreatureToStringStoryWorld(AbstractCreature critter, WorldCoordinate pos)
	{
		string text = pos.ResolveRoomName();
		string text2 = critter.creatureTemplate.type.ToString() + "<cA>";
		text2 = text2 + critter.ID.ToString() + "<cA>";
		text2 += string.Format(CultureInfo.InvariantCulture, "{0}.{1}<cA>", (text != null) ? text : pos.room.ToString(), pos.abstractNode);
		text2 += critter.state.ToString();
		return SetCustomData(critter, text2);
	}

	public static string AbstractCreatureToStringSingleRoomWorld(AbstractCreature critter)
	{
		return AbstractCreatureToStringSingleRoomWorld(critter, critter.pos);
	}

	public static string AbstractCreatureToStringSingleRoomWorld(AbstractCreature critter, WorldCoordinate pos)
	{
		string text = critter.creatureTemplate.type.ToString() + "<cA>";
		text = text + critter.ID.ToString() + "<cA>";
		text += string.Format(CultureInfo.InvariantCulture, "{0}.{1}<cA>", critter.world.GetAbstractRoom(pos.room).name, pos.abstractNode);
		text += critter.state.ToString();
		return SetCustomData(critter, text);
	}

	public void LogOEEncounter(string roomname)
	{
		if (!oeEncounters.Contains(roomname))
		{
			oeEncounters.Add(roomname);
		}
	}

	public List<AbstractPhysicalObject> GrabSavedObjects(AbstractCreature player, WorldCoordinate atPos)
	{
		List<AbstractPhysicalObject> list = new List<AbstractPhysicalObject>();
		Custom.Log("Spawning saved objects! pending objects count is:", pendingObjects.Count.ToString());
		for (int i = 0; i < pendingObjects.Count; i++)
		{
			AbstractPhysicalObject abstractPhysicalObject = null;
			if (pendingObjects[i].Contains("<oA>"))
			{
				abstractPhysicalObject = AbstractPhysicalObjectFromString(player.world.game.world, pendingObjects[i]);
			}
			else if (pendingObjects[i].Contains("<cA>"))
			{
				abstractPhysicalObject = AbstractCreatureFromString(player.world.game.world, pendingObjects[i], onlyInCurrentRegion: false);
			}
			if (abstractPhysicalObject == null)
			{
				continue;
			}
			abstractPhysicalObject.pos = atPos;
			if (AbstractConsumable.IsTypeConsumable(abstractPhysicalObject.type))
			{
				(abstractPhysicalObject as AbstractConsumable).isFresh = false;
			}
			bool flag = false;
			bool flag2 = false;
			for (int j = 0; j < objectTrackers.Count; j++)
			{
				if (!flag && objectTrackers[j].CompatibleWithTracker(abstractPhysicalObject))
				{
					flag = true;
					objectTrackers[j].ChangeDesiredSpawnLocation(atPos);
				}
				if (!flag2 && objectTrackers[j].LinkObjectToTracker(abstractPhysicalObject))
				{
					flag2 = true;
					objectTrackers[j].ChangeDesiredSpawnLocation(atPos);
				}
			}
			if (!flag || flag2)
			{
				Custom.Log($"Recreating {abstractPhysicalObject.type} object from pending objects");
				list.Add(abstractPhysicalObject);
			}
		}
		pendingObjects.Clear();
		return list;
	}

	public List<string> GrabSavedCreatures(AbstractCreature player, WorldCoordinate atPos)
	{
		List<string> list = new List<string>();
		Custom.Log("Spawning saved friends! pending friends count is:", pendingFriendCreatures.Count.ToString());
		for (int i = 0; i < pendingFriendCreatures.Count; i++)
		{
			string[] array = Regex.Split(pendingFriendCreatures[i], "<cA>");
			string text = array[0] + "<cA>" + array[1] + "<cA>";
			string text2 = text;
			string text3 = atPos.room.ToString();
			if (player.world.game.world.GetAbstractRoom(atPos) != null)
			{
				Custom.Log("Getting room 1");
				text3 = player.world.game.world.GetAbstractRoom(atPos).name;
			}
			else if (RainWorld.roomIndexToName.ContainsKey(atPos.room))
			{
				Custom.Log("Getting room 2");
				text3 = RainWorld.roomIndexToName[atPos.room];
			}
			Custom.Log("Room name is:", text3);
			Custom.Log("Player room is:", player.world.game.Players[0].Room.name);
			text = text2 + text3 + "." + atPos.abstractNode + "<cA>";
			text += array[3];
			Custom.Log("Recreating", text, "creature from pending friends");
			list.Add(text);
		}
		pendingFriendCreatures.Clear();
		return list;
	}

	public void setDenPosition()
	{
		if (ModManager.MSC && progression.rainWorld.safariMode)
		{
			SetDenPositionForSafari();
		}
		else if (progression.rainWorld.setup.startMap != "")
		{
			denPosition = progression.rainWorld.setup.startMap;
		}
		else if (ModManager.Expedition && progression.rainWorld.ExpeditionMode)
		{
			ExpLog.Log("Loading Expedition Den");
			string text = ExpeditionGame.ExpeditionRandomStarts(progression.rainWorld, saveStateNumber);
			if (ExpeditionData.startingDen != null)
			{
				ExpLog.Log("Load existing first cycle den from ExpeditionData | " + ExpeditionData.startingDen);
				denPosition = ExpeditionData.startingDen;
			}
			else if (text != "")
			{
				ExpLog.Log("No existing den found, using new one | " + text);
				denPosition = text;
				lastVanillaDen = text;
			}
		}
		else
		{
			denPosition = GetStoryDenPosition(saveStateNumber, out var isVanilla);
			if (isVanilla)
			{
				lastVanillaDen = denPosition;
			}
		}
	}

	public static string GetStoryDenPosition(SlugcatStats.Name slugcat, out bool isVanilla)
	{
		string result = "SU_C04";
		isVanilla = false;
		if (slugcat == SlugcatStats.Name.Red)
		{
			result = "LF_H01";
			isVanilla = true;
		}
		else if (ModManager.MSC && slugcat == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel)
		{
			result = "SH_E01";
			isVanilla = true;
		}
		else if (ModManager.MSC && slugcat == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
		{
			result = "SH_GOR02";
		}
		else if (ModManager.MSC && slugcat == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
		{
			result = "DS_RIVSTART";
		}
		else if (ModManager.MSC && slugcat == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
		{
			result = "GW_A24";
			isVanilla = true;
		}
		else if (ModManager.MSC && slugcat == MoreSlugcatsEnums.SlugcatStatsName.Saint)
		{
			result = "SI_SAINTINTRO";
		}
		else if (ModManager.MSC && slugcat == MoreSlugcatsEnums.SlugcatStatsName.Spear)
		{
			result = "GATE_OE_SU";
		}
		else
		{
			isVanilla = true;
		}
		return result;
	}

	public void SetDenPositionForSafari()
	{
		if (progression.rainWorld.safariRegion == "SL" || progression.rainWorld.safariRegion == "LM")
		{
			denPosition = progression.rainWorld.safariRegion + "_S02";
		}
		else if (progression.rainWorld.safariRegion == "DS" || progression.rainWorld.safariRegion == "UG")
		{
			denPosition = progression.rainWorld.safariRegion + "_S01r";
		}
		else if (progression.rainWorld.safariRegion == "HR")
		{
			denPosition = progression.rainWorld.safariRegion + "_S02";
		}
		else if (progression.rainWorld.safariRegion == "SI")
		{
			denPosition = progression.rainWorld.safariRegion + "_S03";
		}
		else
		{
			denPosition = progression.rainWorld.safariRegion + "_S01";
		}
	}
}
