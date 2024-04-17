using System.Collections.Generic;
using System.Linq;
using Expedition;
using JollyCoop;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class StoryGameSession : GameSession
{
	public SaveState saveState;

	public bool lastEverMetMoon;

	public bool lastEverMetPebbles;

	public SlugcatStats.Name saveStateNumber;

	public WorldCoordinate? karmaFlowerMapPos;

	public PlayerSessionRecord[] playerSessionRecords;

	private float deltaTimer;

	public SlugcatStats[] characterStatsJollyplayer;

	public bool RedIsOutOfCycles
	{
		get
		{
			if (saveStateNumber == SlugcatStats.Name.Red && saveState.cycleNumber >= RedsIllness.RedsCycles(saveState.redExtraCycles))
			{
				return !Custom.rainWorld.ExpeditionMode;
			}
			return false;
		}
	}

	public int slugPupMaxCount
	{
		get
		{
			if (!ModManager.MSC)
			{
				return 0;
			}
			if (game.setupValues.slugPupsMax > -1)
			{
				Custom.LogImportant("Slugpup OVERRIDE limits", game.setupValues.slugPupsMax.ToString());
				return game.setupValues.slugPupsMax;
			}
			if (saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel)
			{
				return 1000;
			}
			if ((saveState.progression.miscProgressionData.beaten_Gourmand_Full || global::MoreSlugcats.MoreSlugcats.chtUnlockSlugpups.Value) && (saveState.saveStateNumber == SlugcatStats.Name.White || saveState.saveStateNumber == SlugcatStats.Name.Red || saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Gourmand))
			{
				return 2;
			}
			return 0;
		}
	}

	public StoryGameSession(SlugcatStats.Name saveStateNumber, RainWorldGame game)
		: base(game)
	{
		this.saveStateNumber = saveStateNumber;
		RainWorld.lastActiveSaveSlot = saveStateNumber;
		playerSessionRecords = new PlayerSessionRecord[(this != null) ? game.StoryPlayerCount : 4];
		saveState = game.rainWorld.progression.GetOrInitiateSaveState(saveStateNumber, game, game.manager.menuSetup, !ModManager.MSC || (!game.wasAnArtificerDream && !game.manager.rainWorld.safariMode));
		saveState.deathPersistentSaveData.karma = Custom.IntClamp(saveState.deathPersistentSaveData.karma, 0, saveState.deathPersistentSaveData.karmaCap);
		characterStats = new SlugcatStats(saveStateNumber, saveState.malnourished);
		if (saveState.creatureCommunitiesString != null)
		{
			creatureCommunities.FromString(saveState.creatureCommunitiesString);
			difficulty = Custom.LerpMap(saveState.deathPersistentSaveData.howWellIsPlayerDoing, -1f, 1f, -1f, 1f, 2f);
			Custom.LogImportant("DYNAMIC DIFFICULTY:", difficulty.ToString());
		}
		else
		{
			creatureCommunities.LoadDefaultCommunityAlignments(saveStateNumber);
		}
		lastEverMetMoon = saveState.miscWorldSaveData.EverMetMoon;
		lastEverMetPebbles = saveState.miscWorldSaveData.SSaiConversationsHad > 0;
		if (ModManager.MSC && game.setupValues.randomStart)
		{
			saveState.progression.SaveWorldStateAndProgression(malnourished: false);
		}
		if (ModManager.Expedition && game.rainWorld.ExpeditionMode)
		{
			saveState.deathPersistentSaveData.karma = Custom.IntClamp(saveState.deathPersistentSaveData.karma, 0, 4);
			saveState.deathPersistentSaveData.karmaCap = 4;
		}
		if (ModManager.MSC && saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Spear && saveState.miscWorldSaveData.SSaiConversationsHad == 0 && !saveState.deathPersistentSaveData.chatlogsRead.SequenceEqual(saveState.deathPersistentSaveData.prePebChatlogsRead))
		{
			saveState.deathPersistentSaveData.chatlogsRead = new List<ChatlogData.ChatlogID>(saveState.deathPersistentSaveData.prePebChatlogsRead);
		}
	}

	public void PlaceKarmaFlowerOnDeathSpot()
	{
		Creature realizedCreature = Players[0].realizedCreature;
		if (ModManager.CoopAvailable && realizedCreature == null)
		{
			for (int i = 0; i < game.Players.Count; i++)
			{
				if (game.Players[i].realizedCreature != null && game.Players[i].realizedCreature.dead)
				{
					realizedCreature = game.Players[i].realizedCreature;
					break;
				}
			}
		}
		if (realizedCreature != null && (realizedCreature as Player).PlaceKarmaFlower)
		{
			saveState.deathPersistentSaveData.karmaFlowerPosition = (realizedCreature as Player).karmaFlowerGrowPos;
			if (ModManager.Expedition && saveState.progression.rainWorld.ExpeditionMode)
			{
				ExpeditionGame.tempKarmaPos = (realizedCreature as Player).karmaFlowerGrowPos;
			}
		}
		if (RedIsOutOfCycles)
		{
			game.manager.rainWorld.progression.miscProgressionData.redsFlower = (realizedCreature as Player).karmaFlowerGrowPos;
		}
	}

	public override void AddPlayer(AbstractCreature player)
	{
		base.AddPlayer(player);
		playerSessionRecords[(player.state as PlayerState).playerNumber] = new PlayerSessionRecord((player.state as PlayerState).playerNumber);
		if (!game.world.singleRoomWorld)
		{
			playerSessionRecords[0].wokeUpInRegion = game.world.region.name;
		}
	}

	public void CreateJollySlugStats(bool m)
	{
		if (game.Players.Count == 0)
		{
			JollyCustom.Log("[JOLLY] NO PLAYERS IN SESSION!!");
			return;
		}
		characterStatsJollyplayer = new SlugcatStats[4];
		PlayerState playerState = game.Players[0].state as PlayerState;
		SlugcatStats slugcatStats = new SlugcatStats(saveState.saveStateNumber, m);
		for (int i = 0; i < game.world.game.Players.Count; i++)
		{
			playerState = game.Players[i].state as PlayerState;
			SlugcatStats.Name playerClass = game.rainWorld.options.jollyPlayerOptionsArray[playerState.playerNumber].playerClass;
			if (playerClass == null)
			{
				playerClass = saveState.saveStateNumber;
				JollyCustom.Log($"Using savelot stats for p [{i}]: {playerClass} ...");
			}
			characterStatsJollyplayer[playerState.playerNumber] = new SlugcatStats(playerClass, m);
			characterStatsJollyplayer[playerState.playerNumber].foodToHibernate = slugcatStats.foodToHibernate;
			characterStatsJollyplayer[playerState.playerNumber].maxFood = slugcatStats.maxFood;
			characterStatsJollyplayer[playerState.playerNumber].bodyWeightFac = slugcatStats.bodyWeightFac;
		}
	}

	public void TimeTick(float dt)
	{
		deltaTimer += dt * 40f;
		if (deltaTimer >= 1f)
		{
			int num = Mathf.FloorToInt(deltaTimer);
			if (game.cameras[0].hud != null)
			{
				if (game.cameras[0].hud.textPrompt.gameOverMode)
				{
					if (!Players[0].state.dead || (ModManager.CoopAvailable && game.AlivePlayers.Count > 0))
					{
						playerSessionRecords[0].playerGrabbedTime += num;
					}
				}
				else if (!game.cameras[0].voidSeaMode)
				{
					playerSessionRecords[0].time += num;
				}
			}
			deltaTimer -= num;
		}
		SpeedRunTimer.CampaignTimeTracker campaignTimeTracker = SpeedRunTimer.GetCampaignTimeTracker(saveStateNumber);
		if (campaignTimeTracker != null && game.cameras[0].hud != null)
		{
			campaignTimeTracker.UndeterminedFreeTime += SpeedRunTimer.GetTimerTickIncrement(game, dt);
		}
	}

	public void SetRandomSeedToCycleSeed(int modifier)
	{
		int num = (saveState.totTime + saveState.cycleNumber * saveStateNumber.Index + modifier) * 10000;
		Random.InitState(num);
		Random.InitState(num + Random.Range(1000, 9000));
	}

	public void AddNewPersistentTracker(AbstractPhysicalObject obj)
	{
		for (int i = 0; i < saveState.objectTrackers.Count; i++)
		{
			if (saveState.objectTrackers[i].obj != null && saveState.objectTrackers[i].CompatibleWithTracker(obj))
			{
				return;
			}
		}
		PersistentObjectTracker persistentObjectTracker = new PersistentObjectTracker(obj);
		saveState.objectTrackers.Add(persistentObjectTracker);
		game.cameras[0].hud.map.addTracker(persistentObjectTracker);
	}

	public void RemovePersistentTracker(AbstractPhysicalObject obj)
	{
		PersistentObjectTracker persistentObjectTracker = null;
		for (int i = 0; i < saveState.objectTrackers.Count; i++)
		{
			if (saveState.objectTrackers[i].obj != null && saveState.objectTrackers[i].CompatibleWithTracker(obj))
			{
				persistentObjectTracker = saveState.objectTrackers[i];
				break;
			}
		}
		if (persistentObjectTracker != null)
		{
			saveState.objectTrackers.Remove(persistentObjectTracker);
			game.cameras[0].hud.map.removeTracker(persistentObjectTracker);
		}
	}

	public void AppendTimeOnCycleEnd(bool deathOrGhost)
	{
		SpeedRunTimer.CampaignTimeTracker campaignTimeTracker = SpeedRunTimer.GetCampaignTimeTracker(saveStateNumber);
		if (campaignTimeTracker != null && deathOrGhost)
		{
			campaignTimeTracker.ConvertUndeterminedToLostTime();
		}
		RainWorld.lockGameTimer = true;
		int num = saveState.deathPersistentSaveData.deathTime + saveState.totTime;
		if (deathOrGhost)
		{
			saveState.deathPersistentSaveData.deathTime += playerSessionRecords[0].time / 40 + playerSessionRecords[0].playerGrabbedTime / 40;
			playerSessionRecords[0].playerGrabbedTime = 0;
		}
		else
		{
			saveState.deathPersistentSaveData.deathTime += playerSessionRecords[0].playerGrabbedTime / 40;
			saveState.totTime += playerSessionRecords[0].time / 40;
			playerSessionRecords[0].playerGrabbedTime = 0;
		}
		int num2 = saveState.deathPersistentSaveData.deathTime + saveState.totTime - num;
		Custom.rainWorld.options.timeSinceLastSaveCopy += num2;
		if (Custom.rainWorld.options.timeSinceLastSaveCopy > 1800)
		{
			saveState.progression.CreateCopyOfSaves(userCreated: false);
			Custom.rainWorld.options.timeSinceLastSaveCopy = 0;
		}
		playerSessionRecords[0].time = 0;
	}
}
