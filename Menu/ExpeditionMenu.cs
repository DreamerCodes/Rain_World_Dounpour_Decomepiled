using System;
using System.Collections.Generic;
using Expedition;
using JollyCoop;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace Menu;

public class ExpeditionMenu : Menu, SelectOneButton.SelectOneButtonOwner, Slider.ISliderOwner
{
	public float leftAnchor;

	public float rightAnchor;

	public FSprite shade;

	public FSprite title;

	public FSprite shadow;

	public int currentSelection;

	public bool pendingBackgroundChange;

	public MenuScene.SceneID currentScene;

	public bool firstTimeLoad;

	public bool fullyLoaded;

	private bool lastPauseButton;

	public SimpleButton manualButton;

	public SimpleButton exitButton;

	public MuteButton muteButton;

	public Vector2 newPagePos;

	public Vector2[] oldPagePos;

	public bool pagesMoving;

	public float movementCounter;

	public float menuAlpha;

	public float menuLastAlpha;

	public bool muted;

	public bool demoUnlocks;

	public bool flatIllust;

	public int counter;

	private bool checkingForBackup;

	private bool checkedForBackup;

	private bool backupExists;

	private bool startedBackupRestore;

	private bool waitingForBackupRestore;

	private bool backupRestoreSuccess;

	private bool reportCorruptedDialogDisplaying;

	public FSprite shroud;

	public CharacterSelectPage characterSelect;

	public ChallengeSelectPage challengeSelect;

	public ProgressionPage progressionPage;

	public Dictionary<string, List<ManualPage>> manualContent;

	public ExpeditionMenu(ProcessManager manager)
		: base(manager, ExpeditionEnums.ProcessID.ExpeditionMenu)
	{
		if (!manager.rainWorld.flatIllustrations)
		{
			flatIllust = true;
		}
		if (manager.musicPlayer != null)
		{
			manager.musicPlayer.FadeOutAllSongs(25f);
		}
		float[] screenOffsets = Custom.GetScreenOffsets();
		leftAnchor = screenOffsets[0];
		rightAnchor = screenOffsets[1];
		for (int i = 0; i < 20; i++)
		{
		}
		ExpeditionGame.playableCharacters = ExpeditionData.GetPlayableCharacters();
		if (manager.rainWorld.options.saveSlot >= 0)
		{
			ExpeditionGame.unlockedExpeditionSlugcats = new List<SlugcatStats.Name>();
			for (int j = 0; j < ExpeditionGame.playableCharacters.Count; j++)
			{
				if (ExpeditionProgression.CheckUnlocked(manager, ExpeditionGame.playableCharacters[j]))
				{
					ExpeditionGame.unlockedExpeditionSlugcats.Add(ExpeditionGame.playableCharacters[j]);
				}
			}
			ExpLog.Log("Storing save slot: " + manager.rainWorld.options.saveSlot);
			ExpeditionData.saveSlot = manager.rainWorld.options.saveSlot;
			int saveSlot = manager.rainWorld.options.saveSlot;
			ExpLog.Log("Expedition Menu entered from slot: " + saveSlot);
			manager.rainWorld.options.saveSlot = -(manager.rainWorld.options.saveSlot + 1);
			ExpLog.Log("Save slot switched to: " + manager.rainWorld.options.saveSlot);
			manager.rainWorld.progression.Destroy(saveSlot);
			manager.rainWorld.progression = new PlayerProgression(manager.rainWorld, tryLoad: true, saveAfterLoad: false);
		}
		global::Expedition.Expedition.coreFile.Load();
		ExpeditionData.completedChallengeList = new List<Challenge>();
		ExpeditionGame.expeditionComplete = false;
		ExpeditionGame.runData = null;
		ExpeditionGame.runKills = null;
		ExpeditionSetup();
		ExpeditionProgression.SetupPerkGroups();
		ExpeditionProgression.SetupBurdenGroups();
		ExpeditionProgression.ParseQuestFiles();
		ExpeditionProgression.ParseMissionFiles();
		pages = new List<Page>
		{
			new Page(this, null, "SCENE", 0),
			new Page(this, null, "SLUGCAT", 1),
			new Page(this, null, "CHALLENGE", 2),
			new Page(this, null, "PROGRESS", 3)
		};
		currentScene = MenuScene.SceneID.Empty;
		scene = new InteractiveMenuScene(this, null, currentScene);
		scene.blurMax = 250f;
		scene.blurMin = 150f;
		pages[0].subObjects.Add(scene);
		shade = new FSprite("Futile_White");
		shade.scaleX = 1000f;
		shade.scaleY = 1000f;
		shade.x = 0f;
		shade.y = 0f;
		shade.color = new Color(0f, 0f, 0f);
		shade.alpha = 0.9f;
		pages[0].Container.AddChild(shade);
		float y = ((manager.rainWorld.options.ScreenSize.x != 1024f) ? 695f : 728f);
		exitButton = new SimpleButton(this, pages[1], Translate("BACK"), "EXIT", new Vector2(leftAnchor + 50f, y), new Vector2(100f, 30f));
		pages[1].subObjects.Add(exitButton);
		backObject = exitButton;
		muteButton = new MuteButton(this, pages[1], "Futile_White", "MUTED", exitButton.pos + new Vector2(110f, 0f), new Vector2(30f, 30f));
		pages[1].subObjects.Add(muteButton);
		manualButton = new SimpleButton(this, pages[1], Translate("MANUAL"), "MANUAL", new Vector2(rightAnchor - 150f, y), new Vector2(100f, 30f));
		pages[1].subObjects.Add(manualButton);
		shroud = new FSprite("Futile_White");
		shroud.x = 683f;
		shroud.y = 440f;
		shroud.scaleX = 1000f;
		shroud.scaleY = 1000f;
		shroud.color = new Color(0f, 0f, 0f);
		container.AddChild(shroud);
		menuAlpha = 1f;
	}

	public void ExpeditionSetup()
	{
		ChallengeTools.GenerateCreatureScores(ref ChallengeTools.creatureScores);
		ChallengeTools.ParseCreatureSpawns();
		ChallengeTools.CreatureName(ref ChallengeTools.creatureNames);
		ChallengeTools.EchoScore(ref ChallengeTools.echoScores);
		ChallengeTools.GenerateAchievementScores();
		ChallengeTools.GenerateVistaLocations();
		ChallengeTools.GenerateObjectTypes();
	}

	public override void Update()
	{
		base.Update();
		bool flag = RWInput.CheckPauseButton(0);
		if (fullyLoaded && flag && !lastPauseButton && manager.dialog == null && !exitButton.buttonBehav.greyedOut)
		{
			Singal(exitButton, exitButton.signalText);
		}
		lastPauseButton = flag;
		counter++;
		menuLastAlpha = menuAlpha;
		if (fullyLoaded && firstTimeLoad && !pendingBackgroundChange && shroud != null && menuAlpha > 0f)
		{
			menuAlpha -= 0.025f;
		}
		if (shroud == null)
		{
			menuAlpha = Mathf.Clamp(menuAlpha, 0.8f, 1.1f);
		}
		if (pendingBackgroundChange)
		{
			menuAlpha += 0.01f;
		}
		else
		{
			menuAlpha -= 0.01f;
		}
		if (!firstTimeLoad && global::Expedition.Expedition.coreFile.coreLoaded)
		{
			firstTimeLoad = true;
			InitMenuPages();
			if (ExpeditionData.validateQuests)
			{
				ExpeditionData.validateQuests = false;
				ValidateQuestRewards();
			}
		}
		if (!fullyLoaded && firstTimeLoad && manager.rainWorld.progression != null && manager.rainWorld.progression.progressionLoaded)
		{
			PlayerProgression.ProgressionLoadResult progressionLoadedResult = manager.rainWorld.progression.progressionLoadedResult;
			switch (progressionLoadedResult)
			{
			case PlayerProgression.ProgressionLoadResult.SUCCESS_LOAD_EXISTING_FILE:
			case PlayerProgression.ProgressionLoadResult.SUCCESS_CREATE_NEW_FILE:
			case PlayerProgression.ProgressionLoadResult.ERROR_SAVE_DATA_MISSING:
				fullyLoaded = true;
				break;
			case PlayerProgression.ProgressionLoadResult.ERROR_CORRUPTED_FILE:
				ReportCorruptFileAndDeleteData();
				break;
			default:
				if (!reportCorruptedDialogDisplaying)
				{
					reportCorruptedDialogDisplaying = true;
					string text = manager.rainWorld.inGameTranslator.Translate("ps4_load_expedition_run_failed");
					string text2 = progressionLoadedResult.ToString();
					if (progressionLoadedResult == PlayerProgression.ProgressionLoadResult.ERROR_READ_FAILED && manager.rainWorld.progression.SaveDataReadFailureError != null)
					{
						text2 = text2 + Environment.NewLine + manager.rainWorld.progression.SaveDataReadFailureError;
					}
					string text3 = text.Replace("{ERROR}", text2);
					DialogNotify dialog = new DialogNotify(size: DialogBoxNotify.CalculateDialogBoxSize(text3, dialogUsesWordWrapping: false), description: Custom.ReplaceLineDelimeters(text3), manager: manager.rainWorld.processManager, onOK: delegate
					{
						reportCorruptedDialogDisplaying = false;
						fullyLoaded = true;
					});
					manager.rainWorld.processManager.ShowDialog(dialog);
				}
				break;
			}
		}
		if (scene != null && pendingBackgroundChange && shade.alpha >= 1f)
		{
			SwitchBackground();
		}
		if (characterSelect != null && characterSelect.nowPlaying != null && manager.musicPlayer != null)
		{
			if (manager.musicPlayer.song != null)
			{
				if (muted)
				{
					manager.musicPlayer.FadeOutAllSongs(30f);
					characterSelect.nowPlaying.label.text = "";
				}
				else if (characterSelect.nowPlaying.label.text == "")
				{
					characterSelect.nowPlaying.label.text = Translate("Now Playing:") + "  " + ExpeditionProgression.TrackName(manager.musicPlayer.song.name);
				}
			}
			else if (!muted)
			{
				if (ExpeditionData.menuSong == null || ExpeditionData.menuSong == "")
				{
					manager.musicPlayer.MenuRequestsSong("RW_27 - Train Tunnels", 1f, 1f);
					characterSelect.nowPlaying.label.text = Translate("Now Playing:") + "  " + ExpeditionProgression.TrackName("RW_27 - Train Tunnels");
					ExpLog.Log("Playing default menu song");
				}
				else
				{
					manager.musicPlayer.MenuRequestsSong(ExpeditionData.menuSong, 1f, 1f);
					characterSelect.nowPlaying.label.text = Translate("Now Playing:") + "  " + ExpeditionProgression.TrackName(ExpeditionData.menuSong);
					ExpLog.Log("Playing custom menu song: " + ExpeditionData.menuSong + " | " + ExpeditionProgression.TrackName(ExpeditionData.menuSong));
				}
			}
		}
		if (!pagesMoving)
		{
			return;
		}
		movementCounter += 0.195f;
		float num = Mathf.Lerp(8f, 125f, Custom.SCurve(movementCounter, 0.85f));
		for (int i = 1; i < pages.Count; i++)
		{
			float a = Vector2.Distance(oldPagePos[i], oldPagePos[i] + newPagePos);
			float value = Vector2.Distance(pages[i].pos, oldPagePos[i] + newPagePos);
			float num2 = Mathf.Lerp(1f, 0.01f, Mathf.InverseLerp(a, 0.1f, value));
			pages[i].pos = Custom.MoveTowards(pages[i].pos, oldPagePos[i] + newPagePos, num * num2);
			if (pages[i].pos == oldPagePos[i] + newPagePos)
			{
				pagesMoving = false;
			}
		}
		if (!pagesMoving)
		{
			PlaySound(SoundID.MENU_Checkbox_Check);
		}
		float y = ((manager.rainWorld.options.ScreenSize.x != 1024f) ? 695f : 728f);
		exitButton.pos = new Vector2(50f, y) - exitButton.page.pos;
		exitButton.lastPos = new Vector2(50f, y) - exitButton.page.lastPos;
		muteButton.pos = new Vector2(160.01f, y) - muteButton.page.pos;
		muteButton.lastPos = new Vector2(160.01f, y) - muteButton.page.lastPos;
		manualButton.pos = new Vector2(rightAnchor - (leftAnchor + 150f), y) - manualButton.page.pos;
		manualButton.lastPos = new Vector2(rightAnchor - (leftAnchor + 150f), y) - manualButton.page.lastPos;
	}

	private void OnBackupChecked(bool response)
	{
		backupExists = response;
		checkedForBackup = true;
	}

	private void OnBackupRestored(bool response)
	{
		backupRestoreSuccess = response;
		waitingForBackupRestore = false;
	}

	private void ReportCorruptFileAndDeleteData()
	{
		if (!reportCorruptedDialogDisplaying)
		{
			reportCorruptedDialogDisplaying = true;
			string text = manager.rainWorld.inGameTranslator.Translate("ps4_load_expedition_failed");
			DialogNotify dialog = new DialogNotify(size: DialogBoxNotify.CalculateDialogBoxSize(text, dialogUsesWordWrapping: false), description: Custom.ReplaceLineDelimeters(text), manager: manager.rainWorld.processManager, onOK: delegate
			{
				manager.rainWorld.progression.DeleteSaveFile();
				reportCorruptedDialogDisplaying = false;
				fullyLoaded = true;
			});
			manager.rainWorld.processManager.ShowDialog(dialog);
		}
	}

	public void InitMenuPages()
	{
		currentPage = 1;
		characterSelect = new CharacterSelectPage(this, pages[1], default(Vector2));
		pages[1].subObjects.Add(characterSelect);
		challengeSelect = new ChallengeSelectPage(this, pages[2], default(Vector2));
		pages[2].subObjects.Add(challengeSelect);
		pages[2].pos.x += 1500f;
		progressionPage = new ProgressionPage(this, pages[3], default(Vector2));
		pages[3].subObjects.Add(progressionPage);
		pages[3].pos.x += 3000f;
		if (ExpeditionData.completedQuests.Count >= 75)
		{
			Custom.rainWorld.processManager.CueAchievement(RainWorld.AchievementID.Quests, 5f);
		}
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		if (shade != null)
		{
			shade.alpha = Mathf.Lerp(menuLastAlpha, menuAlpha, timeStacker);
			shade.alpha = Mathf.Clamp(shade.alpha, 0.8f, 1.1f);
		}
		if (shroud != null)
		{
			shroud.alpha = Mathf.Lerp(menuLastAlpha, menuAlpha, timeStacker);
			if (shroud.alpha <= 0f)
			{
				shroud.RemoveFromContainer();
				shroud = null;
				return;
			}
			shroud.MoveToFront();
		}
		if (manualButton != null)
		{
			if (!ExpeditionData.hasViewedManual)
			{
				Vector3 vector = Custom.RGB2HSL(Color.Lerp(new Color(0.7f, 0.7f, 0.7f), new Color(1f, 0.7f, 0f), Mathf.Sin((float)counter / 5f)));
				HSLColor hSLColor = new HSLColor(vector.x, vector.y, vector.z);
				manualButton.rectColor = hSLColor;
				manualButton.labelColor = hSLColor;
			}
			else
			{
				manualButton.rectColor = Menu.MenuColor(MenuColors.MediumGrey);
				manualButton.labelColor = Menu.MenuColor(MenuColors.MediumGrey);
			}
		}
	}

	public override void Singal(MenuObject sender, string message)
	{
		base.Singal(sender, message);
		if (message == "EXIT")
		{
			PlaySound(SoundID.MENU_Switch_Page_Out);
			global::Expedition.Expedition.coreFile.Save(runEnded: false);
			manager.musicPlayer?.FadeOutAllSongs(100f);
			manager.RequestMainProcessSwitch(ProcessManager.ProcessID.MainMenu);
		}
		if (message == "MANUAL")
		{
			ManualDialog dialog = new ExpeditionManualDialog(manager, ExpeditionManualDialog.topicKeys);
			PlaySound(SoundID.MENU_Player_Join_Game);
			manager.ShowDialog(dialog);
			ExpeditionData.hasViewedManual = true;
		}
		if (message == "MUTED")
		{
			if (muted)
			{
				muted = false;
			}
			else
			{
				muted = true;
			}
			(sender as MuteButton).muted = muted;
			PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
		}
	}

	public override void SliderSetValue(Slider slider, float f)
	{
		if (slider.ID == ExpeditionEnums.SliderID.ChallengeDifficulty)
		{
			ExpeditionData.challengeDifficulty = f;
		}
	}

	public override float ValueOfSlider(Slider slider)
	{
		if (slider.ID == ExpeditionEnums.SliderID.ChallengeDifficulty)
		{
			return ExpeditionData.challengeDifficulty;
		}
		return 0.5f;
	}

	public void SwitchBackground()
	{
		if (scene == null)
		{
			return;
		}
		if (flatIllust)
		{
			manager.rainWorld.flatIllustrations = true;
		}
		scene.RemoveSprites();
		scene.RemoveSubObject(scene);
		scene = new InteractiveMenuScene(this, pages[0], currentScene);
		pages[0].subObjects.Add(scene);
		if (scene.depthIllustrations != null && scene.depthIllustrations.Count > 0)
		{
			int count = scene.depthIllustrations.Count;
			while (count-- > 0)
			{
				scene.depthIllustrations[count].sprite.MoveToBack();
			}
		}
		else
		{
			int count2 = scene.flatIllustrations.Count;
			while (count2-- > 0)
			{
				scene.flatIllustrations[count2].sprite.MoveToBack();
			}
		}
		characterSelect.ReloadSlugcatPortraits();
		pendingBackgroundChange = false;
		if (flatIllust)
		{
			manager.rainWorld.flatIllustrations = false;
		}
	}

	public void MovePage(Vector2 direction)
	{
		if (!pagesMoving)
		{
			pagesMoving = true;
			movementCounter = 0f;
			newPagePos = direction;
			oldPagePos = new Vector2[pages.Count];
			for (int i = 1; i < oldPagePos.Length; i++)
			{
				oldPagePos[i] = pages[i].pos;
			}
			PlaySound(SoundID.MENU_Next_Slugcat);
		}
	}

	public void UpdatePage(int pageIndex)
	{
		currentPage = pageIndex;
		manager.rainWorld.options.ResetJollyProfileRequest();
		if (currentPage == 1)
		{
			selectedObject = characterSelect.slugcatButtons[1];
		}
		else if (currentPage == 2)
		{
			selectedObject = challengeSelect.challengeButtons[0];
		}
		else if (currentPage == 3)
		{
			selectedObject = progressionPage.missionButtons[0];
		}
		exitButton.RemoveSprites();
		exitButton.RemoveSubObject(exitButton);
		exitButton = new SimpleButton(this, pages[currentPage], Translate("BACK"), "EXIT", new Vector2(leftAnchor + 50f, 695f), new Vector2(100f, 30f));
		pages[currentPage].subObjects.Add(exitButton);
		muteButton.RemoveSprites();
		muteButton.RemoveSubObject(muteButton);
		muteButton = new MuteButton(this, pages[currentPage], "Futile_White", "MUTED", exitButton.pos + new Vector2(110f, 695f), new Vector2(30f, 30f));
		muteButton.muted = muted;
		pages[currentPage].subObjects.Add(muteButton);
		manualButton.RemoveSprites();
		manualButton.RemoveSubObject(manualButton);
		manualButton = new SimpleButton(this, pages[currentPage], Translate("MANUAL"), "MANUAL", new Vector2(rightAnchor - 150f, 695f), new Vector2(100f, 30f));
		pages[currentPage].subObjects.Add(manualButton);
		characterSelect.SetUpSelectables();
		challengeSelect.SetUpSelectables();
		progressionPage.SetUpSelectables();
		backObject = exitButton;
	}

	public void PreLoadMenuScenes()
	{
		List<MenuScene.SceneID> list = new List<MenuScene.SceneID>
		{
			MenuScene.SceneID.Landscape_SU,
			MenuScene.SceneID.Yellow_Intro_B,
			MenuScene.SceneID.Landscape_LF
		};
		if (ModManager.MSC)
		{
			List<MenuScene.SceneID> collection = new List<MenuScene.SceneID>
			{
				MoreSlugcatsEnums.MenuSceneID.Landscape_OE,
				MoreSlugcatsEnums.MenuSceneID.Landscape_LC,
				MoreSlugcatsEnums.MenuSceneID.Landscape_DM,
				MoreSlugcatsEnums.MenuSceneID.Landscape_MS,
				MoreSlugcatsEnums.MenuSceneID.Landscape_CL
			};
			list.AddRange(collection);
		}
		if (flatIllust)
		{
			manager.rainWorld.flatIllustrations = true;
		}
		for (int i = 0; i < list.Count; i++)
		{
			if (scene == null)
			{
				continue;
			}
			scene.RemoveSprites();
			scene.RemoveSubObject(scene);
			scene = new InteractiveMenuScene(this, pages[0], list[i]);
			pages[0].subObjects.Add(scene);
			if (scene.depthIllustrations != null && scene.depthIllustrations.Count > 0)
			{
				int count = scene.depthIllustrations.Count;
				while (count-- > 0)
				{
					scene.depthIllustrations[count].sprite.MoveToBack();
				}
			}
			else
			{
				int count2 = scene.flatIllustrations.Count;
				while (count2-- > 0)
				{
					scene.flatIllustrations[count2].sprite.MoveToBack();
				}
			}
		}
		if (flatIllust)
		{
			manager.rainWorld.flatIllustrations = false;
		}
	}

	public void ValidateQuestRewards()
	{
		ExpLog.Log("VALIDATION CHECK");
		if (ExpeditionProgression.questList == null || ExpeditionProgression.questList.Count <= 0)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		bool flag = false;
		foreach (ExpeditionProgression.Quest quest in ExpeditionProgression.questList)
		{
			for (int i = 0; i < ExpeditionData.completedQuests.Count; i++)
			{
				if (!(quest.key == ExpeditionData.completedQuests[i]))
				{
					continue;
				}
				for (int j = 0; j < quest.reward.Length; j++)
				{
					if (quest.reward[j].StartsWith("per-"))
					{
						ExpLog.Log("PERK INCREASE");
						num++;
					}
					else if (!ExpeditionData.unlockables.Contains(quest.reward[j]))
					{
						ExpLog.Log("ADD MISSING REWARD: " + quest.reward[j]);
						ExpeditionData.unlockables.Add(quest.reward[j]);
						flag = true;
						num2++;
					}
				}
			}
		}
		if (flag || ExpeditionData.perkLimit != num + 1)
		{
			ExpeditionData.unlockables.RemoveAll((string s) => s.StartsWith("per-"));
			ExpLog.Log("REMOVE EXISTING PERK LIMIT");
			for (int k = 0; k < num; k++)
			{
				ExpeditionData.unlockables.Add("per-1");
			}
			ExpLog.Log("SET PERK LIMIT TO: " + num);
			global::Expedition.Expedition.coreFile.Save(runEnded: false);
			manager.RequestMainProcessSwitch(ExpeditionEnums.ProcessID.ExpeditionMenu);
		}
	}

	public int GetCurrentlySelectedOfSeries(string series)
	{
		if (series.StartsWith("SLUG-"))
		{
			return currentSelection;
		}
		return 0;
	}

	public void SetCurrentlySelectedOfSeries(string series, int to)
	{
		if (series.StartsWith("SLUG-") && currentSelection != to)
		{
			currentSelection = to;
			characterSelect.UpdateSelectedSlugcat(to);
		}
	}

	public override void CommunicateWithUpcomingProcess(MainLoopProcess nextProcess)
	{
		base.CommunicateWithUpcomingProcess(nextProcess);
		if (ModManager.CoopAvailable && nextProcess is InputOptionsMenu inputOptionsMenu)
		{
			JollyCustom.Log("Going to input menu, setting flag...");
			inputOptionsMenu.fromJollyMenu = true;
			inputOptionsMenu.previousMenu = ExpeditionEnums.ProcessID.ExpeditionMenu;
		}
	}
}
