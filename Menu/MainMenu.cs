using System;
using System.Collections.Generic;
using Expedition;
using Kittehface.Framework20;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace Menu;

public class MainMenu : Menu
{
	private class UnlockCheatSequence
	{
		private int sequenceState;

		private bool buttonDown;

		private bool checkUnlock = true;

		private bool shouldCheckUnlock = true;

		public bool FreezeMenuFuctions
		{
			get
			{
				if (checkUnlock)
				{
					return sequenceState >= 9;
				}
				return false;
			}
		}

		public void Update(RainWorld rainWorld)
		{
			if (!checkUnlock)
			{
				return;
			}
			if (shouldCheckUnlock)
			{
				shouldCheckUnlock = false;
				bool flag = true;
				if (!rainWorld.progression.miscProgressionData.redUnlocked)
				{
					flag = false;
				}
				if (flag)
				{
					int count = ExtEnum<MultiplayerUnlocks.LevelUnlockID>.values.Count;
					for (int i = 0; i < count; i++)
					{
						if (!rainWorld.progression.miscProgressionData.GetTokenCollected(new MultiplayerUnlocks.LevelUnlockID(ExtEnum<MultiplayerUnlocks.LevelUnlockID>.values.GetEntry(i))))
						{
							flag = false;
							break;
						}
					}
				}
				if (flag)
				{
					int count2 = ExtEnum<MultiplayerUnlocks.SandboxUnlockID>.values.Count;
					for (int j = 0; j < count2; j++)
					{
						if (!rainWorld.progression.miscProgressionData.GetTokenCollected(new MultiplayerUnlocks.SandboxUnlockID(ExtEnum<MultiplayerUnlocks.SandboxUnlockID>.values.GetEntry(j))))
						{
							flag = false;
							break;
						}
					}
				}
				if (flag)
				{
					checkUnlock = false;
				}
			}
			_ = rainWorld.options.controls[0].handler.profile;
			string text = "uuddlrlrba+";
			if (sequenceState < text.Length)
			{
				if (RWInput.CheckSpecificAxis(0, 7) > 0.5f)
				{
					if (!buttonDown)
					{
						if (text[sequenceState] == 'u')
						{
							sequenceState++;
						}
						else
						{
							sequenceState = 0;
						}
					}
					buttonDown = true;
				}
				else if (RWInput.CheckSpecificAxis(0, 7) < -0.5f)
				{
					if (!buttonDown)
					{
						if (text[sequenceState] == 'd')
						{
							sequenceState++;
						}
						else
						{
							sequenceState = 0;
						}
					}
					buttonDown = true;
				}
				else if (RWInput.CheckSpecificAxis(0, 6) < -0.5f)
				{
					if (!buttonDown)
					{
						if (text[sequenceState] == 'l')
						{
							sequenceState++;
						}
						else
						{
							sequenceState = 0;
						}
					}
					buttonDown = true;
				}
				else if (RWInput.CheckSpecificAxis(0, 6) > 0.5f)
				{
					if (!buttonDown)
					{
						if (text[sequenceState] == 'r')
						{
							sequenceState++;
						}
						else
						{
							sequenceState = 0;
						}
					}
					buttonDown = true;
				}
				else if (RWInput.CheckSpecificButton(0, 8))
				{
					if (!buttonDown)
					{
						if (text[sequenceState] == 'a')
						{
							sequenceState++;
						}
						else
						{
							sequenceState = 0;
						}
					}
					buttonDown = true;
				}
				else if (RWInput.CheckSpecificButton(0, 9))
				{
					if (!buttonDown)
					{
						if (text[sequenceState] == 'b')
						{
							sequenceState++;
						}
						else
						{
							sequenceState = 0;
						}
					}
					buttonDown = true;
				}
				else if (RWInput.CheckSpecificButton(0, 5))
				{
					if (!buttonDown)
					{
						if (text[sequenceState] == '+')
						{
							sequenceState++;
						}
						else
						{
							sequenceState = 0;
						}
					}
					buttonDown = true;
				}
				else
				{
					buttonDown = false;
				}
			}
			else
			{
				Custom.LogImportant("Cheat code success!");
				rainWorld.progression.miscProgressionData.redUnlocked = true;
				int count3 = ExtEnum<MultiplayerUnlocks.LevelUnlockID>.values.Count;
				int count4 = ExtEnum<MultiplayerUnlocks.SandboxUnlockID>.values.Count;
				for (int k = 0; k < count3; k++)
				{
					rainWorld.progression.miscProgressionData.SetTokenCollected(new MultiplayerUnlocks.LevelUnlockID(ExtEnum<MultiplayerUnlocks.LevelUnlockID>.values.GetEntry(k)));
				}
				for (int l = 0; l < count4; l++)
				{
					rainWorld.progression.miscProgressionData.SetTokenCollected(new MultiplayerUnlocks.SandboxUnlockID(ExtEnum<MultiplayerUnlocks.SandboxUnlockID>.values.GetEntry(l)));
				}
				rainWorld.progression.SaveProgression(saveMaps: false, saveMiscProg: true);
				sequenceState = 0;
				checkUnlock = false;
			}
		}
	}

	private UnlockCheatSequence unlockCheatSequence = new UnlockCheatSequence();

	private List<SimpleButton> mainMenuButtons = new List<SimpleButton>();

	private List<Action> mainMenuButtonCallbacks = new List<Action>();

	private int demoResetCounter;

	private SimpleButton regionButton;

	private SimpleButton expeditionButton;

	private DialogBoxNotify popupAlert;

	private int eeinput;

	private bool eepending;

	protected override bool FreezeMenuFunctions
	{
		get
		{
			if (!base.FreezeMenuFunctions)
			{
				return unlockCheatSequence.FreezeMenuFuctions;
			}
			return true;
		}
	}

	public MainMenu(ProcessManager manager, bool showRegionSpecificBkg)
		: base(manager, ProcessManager.ProcessID.MainMenu)
	{
		if (ModManager.MSC)
		{
			CleanMSCSessionState();
		}
		ModManager.CoopAvailable = ModManager.JollyCoop;
		bool flag = manager.rainWorld.progression.IsThereASavedGame(SlugcatStats.Name.White);
		UserInput.SetForceDisconnectControllers(forceDisconnect: false);
		if (manager.rainWorld.progression.miscProgressionData.currentlySelectedSinglePlayerSlugcat.Index == -1)
		{
			manager.rainWorld.progression.miscProgressionData.currentlySelectedSinglePlayerSlugcat = SlugcatStats.Name.Yellow;
		}
		pages.Add(new Page(this, null, "main", 0));
		MenuScene.SceneID sceneID = MenuScene.SceneID.MainMenu;
		if (!manager.rainWorld.options.dlcTutorialShown && manager.rainWorld.dlcVersion > 0)
		{
			manager.rainWorld.options.titleBackground = MenuScene.SceneID.MainMenu_Downpour;
		}
		if (manager.rainWorld.dlcVersion > 0)
		{
			sceneID = MenuScene.SceneID.MainMenu_Downpour;
		}
		if (ModManager.MMF)
		{
			sceneID = manager.rainWorld.options.TitleBackground;
		}
		if (showRegionSpecificBkg && flag)
		{
			sceneID = BackgroundScene();
		}
		scene = new InteractiveMenuScene(this, pages[0], sceneID);
		pages[0].subObjects.Add(scene);
		float num = 0.3f;
		float num2 = 0.5f;
		if (ModManager.MSC)
		{
			CleanMSCSessionState();
		}
		if (scene != null)
		{
			if (scene.sceneID == MenuScene.SceneID.Landscape_SU)
			{
				num = 0.6f;
			}
			else if (scene.sceneID == MenuScene.SceneID.Landscape_DS)
			{
				num = 0.5f;
			}
			else if (scene.sceneID == MenuScene.SceneID.Landscape_CC)
			{
				num = 0.65f;
				num2 = 0.65f;
			}
			else if (scene.sceneID == MenuScene.SceneID.Landscape_SI)
			{
				num = 0.55f;
				num2 = 0.75f;
			}
			else if (scene.sceneID == MenuScene.SceneID.Landscape_LF)
			{
				num = 0.65f;
				num2 = 0.4f;
			}
			else if (scene.sceneID == MenuScene.SceneID.Landscape_SB)
			{
				num = 0f;
				num2 = 0f;
			}
			else if (scene.sceneID == MenuScene.SceneID.Landscape_SH)
			{
				num = 0.2f;
				num2 = 0.2f;
			}
			else if (scene.sceneID == MenuScene.SceneID.Landscape_SS)
			{
				num = 0f;
				num2 = 0f;
			}
			else if (scene.sceneID == MenuScene.SceneID.Landscape_GW)
			{
				num = 0.45f;
				num2 = 0.6f;
			}
			else if (scene.sceneID == MenuScene.SceneID.Landscape_UW)
			{
				num = 0f;
				num2 = 0f;
			}
		}
		if (num2 > 0f)
		{
			gradientsContainer = new GradientsContainer(this, pages[0], new Vector2(0f, 0f), num2);
			pages[0].subObjects.Add(gradientsContainer);
			if (num > 0f)
			{
				gradientsContainer.subObjects.Add(new DarkGradient(this, gradientsContainer, new Vector2(683f, 580f), 600f, 350f, num));
			}
		}
		float buttonWidth = GetButtonWidth(base.CurrLang);
		Vector2 pos = new Vector2(683f - buttonWidth / 2f, 0f);
		Vector2 size = new Vector2(buttonWidth, 30f);
		AddMainMenuButton(new SimpleButton(this, pages[0], Translate("STORY"), "STORY", pos, size), SinglePlayerButtonPressed, 0);
		if (ModManager.Expedition)
		{
			expeditionButton = new SimpleButton(this, pages[0], Translate("EXPEDITION"), "EXPEDITION", pos, size);
			AddMainMenuButton(expeditionButton, ExpeditionButtonPressed, 0);
		}
		regionButton = new SimpleButton(this, pages[0], Translate("REGIONS"), "REGIONS", pos, size);
		AddMainMenuButton(regionButton, RegionsButtonPressed, 0);
		AddMainMenuButton(new SimpleButton(this, pages[0], Translate("ARENA"), "ARENA", pos, size), ArenaButtonPressed, 0);
		AddMainMenuButton(new SimpleButton(this, pages[0], Translate("REMIX"), "REMIX", pos, size), ModListButtonPressed, 0);
		if (ModManager.MSC)
		{
			AddMainMenuButton(new SimpleButton(this, pages[0], Translate("COLLECTION"), "COLLECTION", pos, size), CollectionButtonPressed, 0);
		}
		AddMainMenuButton(new SimpleButton(this, pages[0], Translate("OPTIONS"), "OPTIONS", pos, size), OptionsButtonPressed, 0);
		AddMainMenuButton(new SimpleButton(this, pages[0], Translate("EXIT"), "EXIT", pos, size), ExitButtonPressed, 0);
		pages[0].subObjects.Add(new MenuIllustration(this, pages[0], "", "MainTitleShadow", new Vector2(378f, 440f), crispPixels: true, anchorCenter: false));
		pages[0].subObjects.Add(new MenuIllustration(this, pages[0], "", "MainTitleBevel", new Vector2(378f, 440f), crispPixels: true, anchorCenter: false));
		(pages[0].subObjects[pages[0].subObjects.Count - 1] as MenuIllustration).sprite.shader = manager.rainWorld.Shaders["MenuText"];
		(pages[0].subObjects[pages[0].subObjects.Count - 1] as MenuIllustration).sprite.color = new Color(0f, 1f, 1f);
		for (int i = 0; i < pages[0].subObjects.Count; i++)
		{
			if (pages[0].subObjects[i] is SimpleButton)
			{
				pages[0].subObjects[i].nextSelectable[0] = pages[0].subObjects[i];
				pages[0].subObjects[i].nextSelectable[2] = pages[0].subObjects[i];
			}
		}
		mySoundLoopID = SoundID.MENU_Main_Menu_LOOP;
		UserInput.SetUserCount(1);
		manager.rainWorld.options.Save();
		if (!manager.rainWorld.options.dlcTutorialShown && manager.rainWorld.dlcVersion > 0)
		{
			popupAlert = new DialogBoxNotify(this, pages[0], Translate("remix_dlc_launch"), "ALERT", new Vector2(manager.rainWorld.options.ScreenSize.x / 2f - 240f + (1366f - manager.rainWorld.options.ScreenSize.x) / 2f, 284f), new Vector2(480f, 180f));
			pages[0].subObjects.Add(popupAlert);
		}
		else if (!manager.rainWorld.options.remixTutorialShown)
		{
			popupAlert = new DialogBoxNotify(this, pages[0], Translate("remix_first_launch"), "ALERT", new Vector2(manager.rainWorld.options.ScreenSize.x / 2f - 240f + (1366f - manager.rainWorld.options.ScreenSize.x) / 2f, 284f), new Vector2(480f, 180f));
			pages[0].subObjects.Add(popupAlert);
		}
		if (!manager.rainWorld.options.optionsFileCanSave)
		{
			manager.rainWorld.options = new Options(manager.rainWorld);
		}
		manager.rainWorld.options.jollyControllersNeedUpdating = true;
	}

	private static float GetButtonWidth(InGameTranslator.LanguageID lang)
	{
		if (!(lang == InGameTranslator.LanguageID.Italian) && !(lang == InGameTranslator.LanguageID.Japanese))
		{
			return 110f;
		}
		return 150f;
	}

	public override void Update()
	{
		base.Update();
		bool flag = false;
		foreach (SimpleButton mainMenuButton in mainMenuButtons)
		{
			mainMenuButton.buttonBehav.greyedOut = false;
		}
		if (popupAlert != null)
		{
			foreach (SimpleButton mainMenuButton2 in mainMenuButtons)
			{
				mainMenuButton2.buttonBehav.greyedOut = true;
			}
			selectedObject = popupAlert.continueButton;
		}
		else
		{
			if (flag)
			{
				foreach (SimpleButton mainMenuButton3 in mainMenuButtons)
				{
					mainMenuButton3.buttonBehav.greyedOut = mainMenuButton3.signalText == "REMIX";
				}
			}
			else
			{
				foreach (SimpleButton mainMenuButton4 in mainMenuButtons)
				{
					mainMenuButton4.buttonBehav.greyedOut = false;
				}
			}
			if ((ModManager.Expedition && !global::Expedition.Expedition.coreFile.coreLoaded) || (manager.rainWorld.progression != null && !manager.rainWorld.progression.progressionLoaded))
			{
				if (expeditionButton != null)
				{
					expeditionButton.buttonBehav.greyedOut = true;
				}
			}
			else if (ModManager.Expedition && expeditionButton != null)
			{
				expeditionButton.buttonBehav.greyedOut = false;
			}
			regionButton.buttonBehav.greyedOut = !manager.rainWorld.progression.miscProgressionData.AreThereAnyDiscoveredShelters;
		}
		if (!manager.rainWorld.OptionsReady)
		{
			foreach (SimpleButton mainMenuButton5 in mainMenuButtons)
			{
				mainMenuButton5.buttonBehav.greyedOut = true;
			}
		}
		if (!flag)
		{
			unlockCheatSequence.Update(manager.rainWorld);
			if (ModManager.MSC)
			{
				eeCheck();
			}
		}
	}

	public override void Singal(MenuObject sender, string message)
	{
		if (message == "ALERT")
		{
			PlaySound(SoundID.MENU_Switch_Arena_Gametype);
			pages[0].subObjects.Remove(popupAlert);
			popupAlert.RemoveSprites();
			popupAlert = null;
			selectedObject = mainMenuButtons[0];
			if (manager.rainWorld.dlcVersion > 0)
			{
				manager.rainWorld.options.dlcTutorialShown = true;
			}
			manager.rainWorld.options.remixTutorialShown = true;
			if (manager.rainWorld.OptionsReady)
			{
				manager.rainWorld.options.Save();
			}
		}
		for (int i = 0; i < mainMenuButtons.Count; i++)
		{
			if (message == mainMenuButtons[i].signalText)
			{
				mainMenuButtonCallbacks[i]();
			}
		}
	}

	public void AddMainMenuButton(SimpleButton button, Action callback, int indexFromBottomOfList)
	{
		float buttonWidth = GetButtonWidth(base.CurrLang);
		mainMenuButtons.Insert(Math.Max(0, mainMenuButtons.Count - indexFromBottomOfList), button);
		mainMenuButtonCallbacks.Insert(Math.Max(0, mainMenuButtonCallbacks.Count - indexFromBottomOfList), callback);
		pages[0].subObjects.Add(button);
		int num = 8;
		float num2 = mainMenuButtons[0].size.y + 10f;
		float num3 = mainMenuButtons[0].size.x + 10f;
		float num4 = (float)Math.Min(num - 1, mainMenuButtons.Count - 1) * num2;
		int num5 = Math.Max(1, (int)((float)(mainMenuButtons.Count - 1) / (float)num) + 1);
		float num6 = 290f + num4 / 2f;
		float num7 = 683f - buttonWidth / 2f;
		if (mainMenuButtons.Count > 5)
		{
			num6 -= (float)(Math.Min(num, mainMenuButtons.Count) - 5) * 10f;
		}
		if (num5 > 1)
		{
			num7 -= num3 * (float)num5 / 2f - mainMenuButtons[0].size.x / 2f;
		}
		for (int i = 0; i < mainMenuButtons.Count; i++)
		{
			mainMenuButtons[i].pos.x = num7 + num3 * (float)(i / num);
			mainMenuButtons[i].pos.y = num6 - num2 * (float)(i % num);
			mainMenuButtons[i].nextSelectable[0] = mainMenuButtons[i];
			mainMenuButtons[i].nextSelectable[2] = mainMenuButtons[i];
			mainMenuButtons[i].nextSelectable[1] = ((i == 0) ? mainMenuButtons[mainMenuButtons.Count - 1] : mainMenuButtons[i - 1]);
			mainMenuButtons[i].nextSelectable[3] = ((i == mainMenuButtons.Count - 1) ? mainMenuButtons[0] : mainMenuButtons[i + 1]);
			if (num5 > 1)
			{
				if (i >= num)
				{
					mainMenuButtons[i].nextSelectable[0] = mainMenuButtons[i - num];
				}
				if (i + num <= mainMenuButtons.Count - 1)
				{
					mainMenuButtons[i].nextSelectable[2] = mainMenuButtons[i + num];
				}
			}
		}
	}

	private void SinglePlayerButtonPressed()
	{
		PlaySound(SoundID.MENU_Switch_Page_In);
		manager.RequestMainProcessSwitch(ProcessManager.ProcessID.SlugcatSelect);
	}

	private void ExpeditionButtonPressed()
	{
		manager.RequestMainProcessSwitch(ExpeditionEnums.ProcessID.ExpeditionMenu);
		if (manager.musicPlayer != null)
		{
			manager.musicPlayer.FadeOutAllSongs(25f);
		}
		PlaySound(SoundID.MENU_Switch_Page_In);
	}

	private void RegionsButtonPressed()
	{
		manager.RequestMainProcessSwitch(ProcessManager.ProcessID.RegionsOverviewScreen);
		PlaySound(SoundID.MENU_Switch_Page_In);
	}

	private void ArenaButtonPressed()
	{
		manager.RequestMainProcessSwitch(ProcessManager.ProcessID.MultiplayerMenu);
		PlaySound(SoundID.MENU_Switch_Page_In);
	}

	private void ModListButtonPressed()
	{
		manager.RequestMainProcessSwitch(ProcessManager.ProcessID.ModdingMenu);
		PlaySound(SoundID.MENU_Switch_Page_In);
	}

	private void OptionsButtonPressed()
	{
		manager.RequestMainProcessSwitch(ProcessManager.ProcessID.OptionsMenu);
		PlaySound(SoundID.MENU_Switch_Page_In);
	}

	private void ExitButtonPressed()
	{
		Application.Quit();
	}

	private MenuScene.SceneID BackgroundScene()
	{
		if (manager.rainWorld.progression.miscProgressionData.menuRegion == null)
		{
			if (ModManager.MMF)
			{
				return manager.rainWorld.options.TitleBackground;
			}
			if (manager.rainWorld.dlcVersion <= 0)
			{
				return MenuScene.SceneID.MainMenu;
			}
			return MenuScene.SceneID.MainMenu_Downpour;
		}
		string menuRegion = manager.rainWorld.progression.miscProgressionData.menuRegion;
		manager.rainWorld.progression.miscProgressionData.menuRegion = null;
		MenuScene.SceneID regionLandscapeScene = Region.GetRegionLandscapeScene(menuRegion);
		if (regionLandscapeScene == MenuScene.SceneID.Empty)
		{
			if (ModManager.MMF)
			{
				return manager.rainWorld.options.TitleBackground;
			}
			if (manager.rainWorld.dlcVersion <= 0)
			{
				return MenuScene.SceneID.MainMenu;
			}
			return MenuScene.SceneID.MainMenu_Downpour;
		}
		return regionLandscapeScene;
	}

	private void CleanMSCSessionState()
	{
		manager.rainWorld.safariMode = false;
		manager.desiredCreditsSong = "RW_8 - Sundown";
		Love.CleanAtlas();
		Love.CleanSounds();
	}

	private void CollectionButtonPressed()
	{
		manager.RequestMainProcessSwitch(MoreSlugcatsEnums.ProcessID.Collections);
		PlaySound(SoundID.MENU_Switch_Page_In);
	}

	private void eeCheck()
	{
		if (Input.anyKey)
		{
			if (Input.GetKey(KeyCode.H) || Input.GetKey(KeyCode.F) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.T) || Input.GetKey(KeyCode.L) || (Input.GetKey(KeyCode.I) | Input.GetKey(KeyCode.E)) || Input.GetKey(KeyCode.N) || Input.GetKey(KeyCode.O) || Input.GetKey(KeyCode.S))
			{
				if ((eeinput == 7 && (Input.GetKey(KeyCode.I) || Input.GetKey(KeyCode.E))) || (eeinput == 8 && (Input.GetKey(KeyCode.I) || Input.GetKey(KeyCode.E))) || (eeinput == 1 && Input.GetKey(KeyCode.O)) || (eeinput == 6 && Input.GetKey(KeyCode.H)) || (eeinput == 3 && Input.GetKey(KeyCode.A)) || (eeinput == 4 && Input.GetKey(KeyCode.N)) || (eeinput == 0 && Input.GetKey(KeyCode.S)) || (eeinput == 2 && Input.GetKey(KeyCode.F)) || (eeinput == 5 && Input.GetKey(KeyCode.T)) || (eeinput == 9 && Input.GetKey(KeyCode.L)))
				{
					eeinput++;
				}
			}
			else
			{
				eeinput = 0;
			}
		}
		if (eeinput == 10)
		{
			manager.rainWorld.progression.miscProgressionData.currentlySelectedSinglePlayerSlugcat = MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel;
			manager.RequestMainProcessSwitch(ProcessManager.ProcessID.IntroRoll);
		}
	}
}
