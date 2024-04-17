using System;
using Expedition;
using HUD;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace Menu;

public class SleepAndDeathScreen : KarmaLadderScreen
{
	public SimpleButton exitButton;

	public SimpleButton passageButton;

	public SimpleButton expPassage;

	public EndgameTokens endgameTokens;

	public bool startMalnourished;

	public new bool goalMalnourished;

	private int food;

	private int playerRoom;

	private Vector2 playerPos;

	private Map.MapData mapData;

	public int endGameSceneCounter = -1;

	public WinState.EndgameID proceedWithEndgameID;

	private bool forceWatchAnimation;

	private bool showFlower;

	public float fadeOutIllustration;

	private SleepScreenKills killsDisplay;

	public MenuLabel starvedLabel;

	public int starvedWarningCounter = -1;

	public override bool ButtonsGreyedOut
	{
		get
		{
			if (!FreezeMenuFunctions && (!forceWatchAnimation || karmaLadder.AllAnimationDone) && endGameSceneCounter < 0)
			{
				if (endgameTokens != null && endgameTokens.forceShowTokenAdd)
				{
					return !endgameTokens.AllAnimationsDone;
				}
				return false;
			}
			return true;
		}
	}

	public bool AllowFoodMeterTick
	{
		get
		{
			if (killsDisplay != null)
			{
				return killsDisplay.countedAndDone;
			}
			return true;
		}
	}

	public bool IsSleepScreen => ID == ProcessManager.ProcessID.SleepScreen;

	public bool IsDeathScreen => ID == ProcessManager.ProcessID.DeathScreen;

	public bool IsStarveScreen => ID == ProcessManager.ProcessID.StarveScreen;

	public bool IsAnyDeath
	{
		get
		{
			if (!IsDeathScreen)
			{
				return IsStarveScreen;
			}
			return true;
		}
	}

	protected override bool FreezeMenuFunctions
	{
		get
		{
			if (base.hud != null && base.hud.rainWorld != null && RWInput.PlayerInput(0).mp)
			{
				return true;
			}
			return base.FreezeMenuFunctions;
		}
	}

	public override bool RevealMap
	{
		get
		{
			if (base.hud != null && endGameSceneCounter < 0 && RWInput.PlayerInput(0).mp)
			{
				return karmaLadder.movementShown;
			}
			return false;
		}
	}

	public override int CurrentFood => food;

	public override Vector2 MapOwnerInRoomPosition => playerPos;

	public override int MapOwnerRoom => playerRoom;

	public float StarveLabelAlpha(float timeStacker)
	{
		return Mathf.InverseLerp(40f, 60f, (float)starvedWarningCounter + timeStacker);
	}

	public float FoodMeterXPos(float down)
	{
		return Custom.LerpMap(manager.rainWorld.options.ScreenSize.x, 1024f, 1366f, manager.rainWorld.options.ScreenSize.x / 2f - 110f, 540f);
	}

	public SleepAndDeathScreen(ProcessManager manager, ProcessManager.ProcessID ID)
		: base(manager, ID)
	{
		AddContinueButton(black: false);
		if (ModManager.Expedition && manager.rainWorld.ExpeditionMode && ExpeditionGame.activeUnlocks.Contains("unl-passage"))
		{
			ExpLog.Log("Add Expedition Passage");
			AddExpeditionPassageButton();
		}
		exitButton = new SimpleButton(this, pages[0], Translate("EXIT"), "EXIT", new Vector2(base.ContinueAndExitButtonsXPos - 320f - manager.rainWorld.options.SafeScreenOffset.x, Mathf.Max(manager.rainWorld.options.SafeScreenOffset.y, 15f)), new Vector2(110f, 30f));
		pages[0].subObjects.Add(exitButton);
		infoLabel.y = manager.rainWorld.options.ScreenSize.y - 30f;
		mySoundLoopID = (IsSleepScreen ? SoundID.MENU_Sleep_Screen_LOOP : SoundID.MENU_Death_Screen_LOOP);
		PlaySound(IsSleepScreen ? SoundID.MENU_Enter_Sleep_Screen : SoundID.MENU_Enter_Death_Screen);
		if (manager.rainWorld.options.validation)
		{
			forceWatchAnimation = true;
		}
	}

	protected override void AddBkgIllustration()
	{
		if (IsSleepScreen)
		{
			scene = new InteractiveMenuScene(this, pages[0], MenuScene.SceneID.SleepScreen);
			pages[0].subObjects.Add(scene);
		}
		else if (IsDeathScreen)
		{
			scene = new InteractiveMenuScene(this, pages[0], MenuScene.SceneID.NewDeath);
			pages[0].subObjects.Add(scene);
		}
		else if (IsStarveScreen)
		{
			scene = new InteractiveMenuScene(this, pages[0], MenuScene.SceneID.StarveScreen);
			pages[0].subObjects.Add(scene);
		}
	}

	public override void Update()
	{
		if (starvedWarningCounter >= 0)
		{
			starvedWarningCounter++;
		}
		base.Update();
		if (exitButton != null)
		{
			exitButton.buttonBehav.greyedOut = ButtonsGreyedOut;
		}
		if (passageButton != null)
		{
			passageButton.buttonBehav.greyedOut = ButtonsGreyedOut || goalMalnourished;
			passageButton.black = Mathf.Max(0f, passageButton.black - 0.0125f);
		}
		if (ModManager.Expedition && manager.rainWorld.ExpeditionMode && expPassage != null)
		{
			expPassage.buttonBehav.greyedOut = ButtonsGreyedOut;
		}
		if (endGameSceneCounter >= 0)
		{
			endGameSceneCounter++;
			if (endGameSceneCounter > 140)
			{
				manager.RequestMainProcessSwitch(ProcessManager.ProcessID.CustomEndGameScreen);
			}
		}
		if (RevealMap)
		{
			fadeOutIllustration = Custom.LerpAndTick(fadeOutIllustration, 1f, 0.02f, 0.025f);
		}
		else
		{
			fadeOutIllustration = Custom.LerpAndTick(fadeOutIllustration, 0f, 0.02f, 0.025f);
		}
		if (!manager.rainWorld.flatIllustrations && (!ModManager.MMF || (!(manager.rainWorld.options.quality == Options.Quality.MEDIUM) && !(manager.rainWorld.options.quality == Options.Quality.LOW))))
		{
			if (IsSleepScreen)
			{
				scene.depthIllustrations[0].setAlpha = Mathf.Lerp(1f, 0.2f, fadeOutIllustration);
				scene.depthIllustrations[1].setAlpha = Mathf.Lerp(0.24f, 0.1f, fadeOutIllustration);
				scene.depthIllustrations[2].setAlpha = Mathf.Lerp(1f, 0.35f, fadeOutIllustration);
			}
			else if (IsStarveScreen)
			{
				scene.depthIllustrations[0].setAlpha = Mathf.Lerp(0.85f, 0.4f, fadeOutIllustration);
			}
			else if (IsDeathScreen)
			{
				scene.depthIllustrations[0].setAlpha = Mathf.Lerp(1f, 0.1f, fadeOutIllustration);
				scene.depthIllustrations[2].setAlpha = Mathf.Lerp(1f, 0.25f, fadeOutIllustration);
				scene.depthIllustrations[3].setAlpha = Mathf.Lerp(1f, 0.5f, fadeOutIllustration);
			}
		}
		if (ModManager.Expedition && expPassage != null)
		{
			expPassage.buttonBehav.greyedOut = ExpeditionData.earnedPassages <= 0;
		}
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		if (starvedLabel != null)
		{
			starvedLabel.label.color = Color.Lerp(Menu.MenuRGB(MenuColors.MediumGrey), Color.red, 0.5f - 0.5f * Mathf.Sin((timeStacker + (float)starvedWarningCounter) / 30f * (float)Math.PI * 2f));
			starvedLabel.label.alpha = StarveLabelAlpha(timeStacker);
		}
	}

	public override void Singal(MenuObject sender, string message)
	{
		base.Singal(sender, message);
		switch (message)
		{
		case "PASSAGE":
			if (proceedWithEndgameID == null)
			{
				proceedWithEndgameID = winState.GetNextEndGame();
				if (proceedWithEndgameID != null)
				{
					endgameTokens.Passage(proceedWithEndgameID);
					endGameSceneCounter = 1;
					PlaySound(SoundID.MENU_Passage_Button);
				}
			}
			break;
		case "EXPPASSAGE":
			if (ModManager.Expedition && manager.rainWorld.ExpeditionMode && ExpeditionData.earnedPassages > 0)
			{
				ExpeditionData.earnedPassages--;
				manager.RequestMainProcessSwitch(ProcessManager.ProcessID.FastTravelScreen);
				PlaySound(SoundID.MENU_Passage_Button);
			}
			break;
		}
	}

	public void AddPassageButton(bool buttonBlack)
	{
		if (ModManager.Expedition && saveState.progression.rainWorld.ExpeditionMode)
		{
			ExpLog.LogOnce("Disable passage");
		}
		else
		{
			if ((saveState != null && (saveState.saveStateNumber == SlugcatStats.Name.Red || (ModManager.MSC && saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Saint))) || (ModManager.MSC && saveState != null && saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Rivulet && saveState.miscWorldSaveData.moonHeartRestored && !saveState.deathPersistentSaveData.altEnding))
			{
				return;
			}
			Custom.Log("Add passage button");
			if (passageButton == null)
			{
				passageButton = new SimpleButton(this, pages[0], Translate("PASSAGE"), "PASSAGE", new Vector2(base.LeftHandButtonsPosXAdd + manager.rainWorld.options.SafeScreenOffset.x, Mathf.Max(manager.rainWorld.options.SafeScreenOffset.y, 15f)), new Vector2(110f, 30f));
				pages[0].subObjects.Add(passageButton);
				if (buttonBlack)
				{
					passageButton.black = 1f;
				}
				passageButton.lastPos = passageButton.pos;
			}
		}
	}

	public void AddExpeditionPassageButton()
	{
		expPassage = new SimpleButton(this, pages[0], Translate("PASSAGE"), "EXPPASSAGE", new Vector2(base.LeftHandButtonsPosXAdd + manager.rainWorld.options.SafeScreenOffset.x, Mathf.Max(manager.rainWorld.options.SafeScreenOffset.y, 15f)), new Vector2(110f, 30f));
		pages[0].subObjects.Add(expPassage);
		expPassage.lastPos = expPassage.pos;
		MenuLabel menuLabel = new MenuLabel(this, pages[0], Translate("AVAILABLE: ") + ExpeditionData.earnedPassages, new Vector2(expPassage.pos.x + expPassage.size.x / 2f, expPassage.pos.y + 45f), default(Vector2), bigText: false);
		menuLabel.label.color = new Color(0.7f, 0.7f, 0.7f);
		pages[0].subObjects.Add(menuLabel);
	}

	public override string UpdateInfoText()
	{
		if (selectedObject is SimpleButton)
		{
			SimpleButton simpleButton = selectedObject as SimpleButton;
			if (simpleButton.signalText == "EXIT")
			{
				return Translate("Exit to title screen");
			}
			if (simpleButton.signalText == "PASSAGE")
			{
				return Translate("(One time use) Fast travel to any discovered shelter, recovering all karma");
			}
			if (simpleButton.signalText == "CONTINUE")
			{
				return Translate("Continue to the next cycle");
			}
			if (ModManager.Expedition && manager.rainWorld.ExpeditionMode && simpleButton.signalText == "EXPPASSAGE")
			{
				return Translate("Recover all karma and fast travel to any discovered shelter, consumes an earned passage");
			}
		}
		return base.UpdateInfoText();
	}

	public override void GetDataFromGame(SleepDeathScreenDataPackage package)
	{
		base.GetDataFromGame(package);
		food = package.food;
		playerRoom = package.playerRoom;
		playerPos = package.playerPos;
		mapData = package.mapData;
		startMalnourished = package.startMalnourished;
		goalMalnourished = package.goalMalnourished;
		base.hud.InitSleepHud(this, mapData, package.characterStats);
		if (IsAnyDeath)
		{
			if (manager.rainWorld.progression.miscProgressionData.watchedDeathScreens < 2)
			{
				forceWatchAnimation = true;
			}
			manager.rainWorld.progression.miscProgressionData.watchedDeathScreens++;
			if (package.karmaReinforced)
			{
				if (manager.rainWorld.progression.miscProgressionData.watchedDeathScreensWithFlower < 2)
				{
					forceWatchAnimation = true;
				}
				manager.rainWorld.progression.miscProgressionData.watchedDeathScreensWithFlower++;
			}
			showFlower = package.karmaReinforced || (package.saveState != null && package.saveState.saveStateNumber == SlugcatStats.Name.Yellow);
		}
		else if (IsSleepScreen)
		{
			if (manager.rainWorld.progression.miscProgressionData.watchedSleepScreens < 2)
			{
				forceWatchAnimation = true;
			}
			manager.rainWorld.progression.miscProgressionData.watchedSleepScreens++;
			if (package.sessionRecord != null && package.sessionRecord.kills.Count > 0)
			{
				killsDisplay = new SleepScreenKills(this, pages[0], new Vector2(base.LeftHandButtonsPosXAdd, 728f), package.sessionRecord.kills);
				pages[0].subObjects.Add(killsDisplay);
				killsDisplay.started = true;
			}
			if (ModManager.MSC && package.characterStats.name == MoreSlugcatsEnums.SlugcatStatsName.Saint)
			{
				if (soundLoop != null)
				{
					soundLoop.Destroy();
				}
				mySoundLoopID = MoreSlugcatsEnums.MSCSoundID.Sleep_Blizzard_Loop;
			}
			if (goalMalnourished)
			{
				starvedLabel = new MenuLabel(this, pages[0], Translate("You are starving. Your game has not been saved."), new Vector2(0f, 24f), new Vector2(1366f, 20f), bigText: true);
				pages[0].subObjects.Add(starvedLabel);
				if (manager.rainWorld.progression.miscProgressionData.watchedMalnourishScreens < 6)
				{
					forceWatchAnimation = true;
				}
				manager.rainWorld.progression.miscProgressionData.watchedMalnourishScreens++;
			}
			if (startMalnourished)
			{
				base.hud.foodMeter.NewShowCount(base.hud.foodMeter.maxFood);
				if (manager.rainWorld.progression.miscProgressionData.watchedMalnourishScreens < 4)
				{
					forceWatchAnimation = true;
				}
				manager.rainWorld.progression.miscProgressionData.watchedMalnourishScreens++;
			}
			if (goalMalnourished || startMalnourished)
			{
				base.hud.foodMeter.MoveSurvivalLimit(base.hud.foodMeter.showCount, smooth: false);
				base.hud.foodMeter.eatCircles = base.hud.foodMeter.maxFood;
			}
		}
		if (startMalnourished)
		{
			base.hud.foodMeter.MoveSurvivalLimit(base.hud.foodMeter.maxFood, smooth: false);
		}
		bool flag = false;
		if (IsSleepScreen || IsDeathScreen || IsStarveScreen)
		{
			if (ModManager.Expedition && manager.rainWorld.ExpeditionMode)
			{
				return;
			}
			if (ModManager.MMF)
			{
				pages[0].subObjects.Add(new CollectiblesTracker(this, pages[0], new Vector2(manager.rainWorld.options.ScreenSize.x - 50f + (1366f - manager.rainWorld.options.ScreenSize.x) / 2f, manager.rainWorld.options.ScreenSize.y - 15f), container, package.characterStats.name));
			}
			if (package.characterStats.name != SlugcatStats.Name.Red)
			{
				endgameTokens = new EndgameTokens(this, pages[0], new Vector2(base.LeftHandButtonsPosXAdd + manager.rainWorld.options.SafeScreenOffset.x + 140f, Mathf.Max(15f, manager.rainWorld.options.SafeScreenOffset.y)), container, karmaLadder);
				pages[0].subObjects.Add(endgameTokens);
			}
			if (IsSleepScreen && package.saveState != null)
			{
				for (int i = 0; i < karmaLadder.endGameMeters.Count; i++)
				{
					if (!package.saveState.deathPersistentSaveData.endGameMetersEverShown.Contains(karmaLadder.endGameMeters[i].tracker.ID))
					{
						forceWatchAnimation = true;
						package.saveState.deathPersistentSaveData.endGameMetersEverShown.Add(karmaLadder.endGameMeters[i].tracker.ID);
						manager.rainWorld.progression.SaveDeathPersistentDataOfCurrentState(saveAsIfPlayerDied: false, saveAsIfPlayerQuit: false);
						break;
					}
				}
			}
			if (IsSleepScreen)
			{
				for (int j = 0; j < karmaLadder.endGameMeters.Count; j++)
				{
					if (karmaLadder.endGameMeters[j].fullfilledNow)
					{
						flag = true;
					}
				}
			}
		}
		if (manager.rainWorld.setup.devToolsActive || (ModManager.MMF && MMF.cfgFasterShelterOpen.Value) || manager.rainWorld.options.validation)
		{
			forceWatchAnimation = false;
		}
		if (flag)
		{
			forceWatchAnimation = true;
		}
		if (!manager.rainWorld.progression.CanSave)
		{
			manager.rainWorld.ReloadProgression();
		}
	}

	public override void FoodCountDownDone()
	{
		Custom.Log("Karma ladder MOVE!");
		if (IsSleepScreen)
		{
			karmaLadder.GoToKarma(karma.x + 1, displayMetersOnRest: true);
		}
		else if (IsAnyDeath)
		{
			karmaLadder.GoToKarma(karma.x - 1, displayMetersOnRest: true);
			if (showFlower && scene != null && (scene as InteractiveMenuScene).timer < 0)
			{
				(scene as InteractiveMenuScene).timer = 0;
			}
		}
		if (starvedLabel != null)
		{
			starvedWarningCounter = 0;
		}
	}

	public override void CommunicateWithUpcomingProcess(MainLoopProcess nextProcess)
	{
		if (nextProcess.ID == ProcessManager.ProcessID.CustomEndGameScreen)
		{
			(nextProcess as CustomEndGameScreen).GetDataFromSleepScreen(proceedWithEndgameID);
		}
		if ((IsDeathScreen || manager.rainWorld.options.validation || (ModManager.MMF && MMF.cfgFasterShelterOpen.Value) || (ModManager.MMF && goalMalnourished)) && nextProcess is RainWorldGame && (nextProcess as RainWorldGame).world != null && (nextProcess as RainWorldGame).world.rainCycle != null && (!ModManager.MSC || global::MoreSlugcats.MoreSlugcats.cfgDisablePrecycles.Value || (nextProcess as RainWorldGame).world.rainCycle.preTimer == 0))
		{
			(nextProcess as RainWorldGame).world.rainCycle.timer = 340;
		}
		if (nextProcess is TipScreen && saveState != null)
		{
			(nextProcess as TipScreen).GetDataFromGame(saveState.saveStateNumber, saveState.deathPersistentSaveData.tipCounter, saveState.deathPersistentSaveData.tipSeed, (IsDeathScreen || goalMalnourished) ? 340 : 0);
		}
	}
}
