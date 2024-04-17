using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Kittehface.Framework20;
using MoreSlugcats;
using Rewired;
using RWCustom;
using UnityEngine;

namespace Menu;

public class MultiplayerMenu : Menu, CheckBox.IOwnCheckBox
{
	public LevelSelector levelSelector;

	private ArenaSettingsInterface arenaSettingsInterface;

	private SandboxSettingsInterface sandboxSettingsInterface;

	public ArenaSetup.GameTypeID nextGameType;

	public List<string> allLevels;

	public MenuLabel abovePlayButtonLabel;

	private float APBLPulse;

	private float APBLLastPulse;

	private float APBLSin;

	private float APBLLastSin;

	private SimpleButton backButton;

	private SimpleButton playButton;

	private SimpleButton resumeButton;

	public PlayerJoinButton[] playerJoinButtons;

	public bool[] joinButtonClickFlag;

	private FSprite darkSprite;

	public MultiplayerUnlocks multiplayerUnlocks;

	public List<string> thumbsToBeLoaded;

	public List<string> loadedThumbTextures;

	public FSprite blackFadeSprite;

	public float blackFade;

	public float lastBlackFade;

	public int fullBlackCounter;

	public SymbolButton infoButton;

	private SelectableMenuObject scrollSelectKeeper;

	private BigArrowButton prevButton;

	private BigArrowButton nextButton;

	public InfoWindow infoWindow;

	private bool requestingControllerConnections;

	private bool exiting;

	private bool lastPauseButton;

	public SimpleButton[] playerClassButtons;

	public SimpleButton[] challengeButtons;

	public FSprite[] challengeChecks;

	public ChallengeInformation challengeInfo;

	public SimpleButton[] safariButtons;

	public List<SimpleButton> safariSlugcatButtons;

	public List<FSprite> safariSlugcatLabels;

	public MenuIllustration[] safariIllustrations;

	public MenuLabel safariTitle;

	public CheckBox safariDisableRain;

	public bool firstSafariSlugcatsButtonPopulate;

	public SimpleButton nextSafariPageButton;

	public SimpleButton prevSafariPageButton;

	public int safariPageNum;

	public SimpleButton nextChallengePageButton;

	public SimpleButton prevChallengePageButton;

	public int challengePageNum;

	public int totalChallenges;

	public ArenaSetup.GameTypeID currentGameType
	{
		get
		{
			return GetArenaSetup.currentGameType;
		}
		set
		{
			GetArenaSetup.currentGameType = value;
		}
	}

	public ArenaSetup GetArenaSetup => manager.arenaSetup;

	public ArenaSetup.GameTypeSetup GetGameTypeSetup => GetArenaSetup.GetOrInitiateGameTypeSetup(currentGameType);

	protected override bool FreezeMenuFunctions
	{
		get
		{
			if (base.FreezeMenuFunctions)
			{
				return true;
			}
			if (sandboxSettingsInterface != null)
			{
				return sandboxSettingsInterface.freezeMenu;
			}
			return false;
		}
	}

	public int TotalSafariPages => Mathf.CeilToInt((float)ExtEnum<MultiplayerUnlocks.SafariUnlockID>.values.Count / (float)SafariButtonsPerPage);

	public int TotalChallengePages => Mathf.CeilToInt((float)totalChallenges / (float)MultiplayerUnlocks.TOTAL_CHALLENGES);

	public int SafariButtonsPerPage => 21;

	public MultiplayerMenu(ProcessManager manager)
		: base(manager, ProcessManager.ProcessID.MultiplayerMenu)
	{
		if (manager.arenaSetup == null)
		{
			manager.arenaSetup = new ArenaSetup(manager);
		}
		if (ModManager.MSC)
		{
			manager.rainWorld.safariMode = false;
			totalChallenges = MultiplayerUnlocks.TOTAL_CHALLENGES;
			while (File.Exists(ChallengeInformation.ChallengePath(totalChallenges + 1)))
			{
				totalChallenges++;
			}
		}
		for (int i = 1; i < manager.arenaSetup.playersJoined.Length; i++)
		{
			if (!manager.arenaSetup.playersJoined[i])
			{
				manager.rainWorld.DeactivatePlayer(i);
			}
		}
		nextGameType = currentGameType;
		blackFade = 1f;
		lastBlackFade = 1f;
		pages.Add(new Page(this, null, "main", 0));
		scene = new InteractiveMenuScene(this, pages[0], ModManager.MMF ? manager.rainWorld.options.subBackground : MenuScene.SceneID.Landscape_SU);
		pages[0].subObjects.Add(scene);
		darkSprite = new FSprite("pixel");
		darkSprite.color = new Color(0f, 0f, 0f);
		darkSprite.anchorX = 0f;
		darkSprite.anchorY = 0f;
		darkSprite.scaleX = 1368f;
		darkSprite.scaleY = 770f;
		darkSprite.x = -1f;
		darkSprite.y = -1f;
		darkSprite.alpha = 0.85f;
		pages[0].Container.AddChild(darkSprite);
		blackFadeSprite = new FSprite("Futile_White");
		blackFadeSprite.scaleX = 87.5f;
		blackFadeSprite.scaleY = 50f;
		blackFadeSprite.x = manager.rainWorld.screenSize.x / 2f;
		blackFadeSprite.y = manager.rainWorld.screenSize.y / 2f;
		blackFadeSprite.color = new Color(0f, 0f, 0f);
		Futile.stage.AddChild(blackFadeSprite);
		string[] array = AssetManager.ListDirectory("Levels");
		allLevels = new List<string>();
		for (int j = 0; j < array.Length; j++)
		{
			if (array[j].Substring(array[j].Length - 4, 4) == ".txt" && array[j].Substring(array[j].Length - 13, 13) != "_settings.txt" && array[j].Substring(array[j].Length - 10, 10) != "_arena.txt" && !array[j].Contains("unlockall"))
			{
				string[] array2 = array[j].Substring(0, array[j].Length - 4).Split(Path.DirectorySeparatorChar);
				allLevels.Add(array2[array2.Length - 1]);
			}
		}
		multiplayerUnlocks = new MultiplayerUnlocks(manager.rainWorld.progression, allLevels);
		for (int num = allLevels.Count - 1; num >= 0; num--)
		{
			if (!multiplayerUnlocks.IsLevelUnlocked(allLevels[num]))
			{
				allLevels.RemoveAt(num);
			}
		}
		allLevels.Sort((string A, string B) => multiplayerUnlocks.LevelListSortString(A).CompareTo(multiplayerUnlocks.LevelListSortString(B)));
		thumbsToBeLoaded = new List<string>();
		loadedThumbTextures = new List<string>();
		for (int k = 0; k < allLevels.Count; k++)
		{
			thumbsToBeLoaded.Add(allLevels[k]);
		}
		playButton = new SimpleButton(this, pages[0], "", "PLAY!", new Vector2(-1000f, -1000f), new Vector2(110f, 30f));
		pages[0].subObjects.Add(playButton);
		backButton = new SimpleButton(this, pages[0], Translate("BACK"), "EXIT", new Vector2(200f, 50f), new Vector2(110f, 30f));
		pages[0].subObjects.Add(backButton);
		backObject = backButton;
		prevButton = new BigArrowButton(this, pages[0], "PREV", new Vector2(200f, 668f), -1);
		pages[0].subObjects.Add(prevButton);
		nextButton = new BigArrowButton(this, pages[0], "NEXT", new Vector2(1116f, 668f), 1);
		pages[0].subObjects.Add(nextButton);
		abovePlayButtonLabel = new MenuLabel(this, pages[0], "", playButton.pos + new Vector2((0f - playButton.size.x) / 2f + 0.01f, 50.01f), new Vector2(playButton.size.x, 20f), bigText: false);
		abovePlayButtonLabel.label.alignment = FLabelAlignment.Left;
		abovePlayButtonLabel.label.color = Menu.MenuRGB(MenuColors.DarkGrey);
		pages[0].subObjects.Add(abovePlayButtonLabel);
		if (manager.rainWorld.options.ScreenSize.x < 1280f)
		{
			abovePlayButtonLabel.label.alignment = FLabelAlignment.Right;
			abovePlayButtonLabel.pos.x = playButton.pos.x + 55f;
		}
		infoButton = new SymbolButton(this, pages[0], "Menu_InfoI", "INFO", new Vector2(1142f, 624f));
		pages[0].subObjects.Add(infoButton);
		InitiateGameTypeSpecificButtons();
		mySoundLoopID = SoundID.MENU_Main_Menu_LOOP;
	}

	protected override void Init()
	{
		base.Init();
		for (int i = 0; i < scene.flatIllustrations.Count; i++)
		{
			scene.flatIllustrations[i].pos.x += Menu.HorizontalMoveToGetCentered(manager);
		}
	}

	public void InitiateGameTypeSpecificButtons()
	{
		for (int i = 0; i < pages[0].selectables.Count; i++)
		{
			if (pages[0].selectables[i] is MenuObject)
			{
				for (int j = 0; j < 4; j++)
				{
					(pages[0].selectables[i] as MenuObject).nextSelectable[j] = null;
				}
			}
		}
		if (currentGameType == ArenaSetup.GameTypeID.Sandbox || currentGameType == ArenaSetup.GameTypeID.Competitive)
		{
			float num = 120f;
			float num2 = 0f;
			if (base.CurrLang == InGameTranslator.LanguageID.German)
			{
				num = 140f;
				num2 = 15f;
			}
			float num3 = num - num2;
			if (ModManager.MSC)
			{
				playerClassButtons = new SimpleButton[4];
				for (int k = 0; k < playerClassButtons.Length; k++)
				{
					playerClassButtons[k] = new SimpleButton(this, pages[0], Translate(SlugcatStats.getSlugcatName(GetArenaSetup.playerClass[k])), "CLASSCHANGE" + k, new Vector2(600f + (float)k * num3, 500f) + new Vector2(106f, -60f) - new Vector2((num3 - 120f) * (float)playerClassButtons.Length, 0f), new Vector2(num - 20f, 30f));
					pages[0].subObjects.Add(playerClassButtons[k]);
				}
			}
			playerJoinButtons = new PlayerJoinButton[4];
			joinButtonClickFlag = new bool[4];
			for (int l = 0; l < playerJoinButtons.Length; l++)
			{
				playerJoinButtons[l] = new PlayerJoinButton(this, pages[0], new Vector2(600f + (float)l * num3, 500f) + new Vector2(106f, -20f) + new Vector2((num - 120f) / 2f, 0f) - new Vector2((num3 - 120f) * (float)playerJoinButtons.Length, 0f), l);
				if (ModManager.MSC)
				{
					playerJoinButtons[l].portrait.fileName = ArenaImage(GetArenaSetup.playerClass[l], l);
					playerJoinButtons[l].portrait.LoadFile();
					playerJoinButtons[l].portrait.sprite.SetElementByName(playerJoinButtons[l].portrait.fileName);
					MutualVerticalButtonBind(playerClassButtons[l], playerJoinButtons[l]);
				}
				pages[0].subObjects.Add(playerJoinButtons[l]);
			}
			if (ModManager.MSC)
			{
				for (int m = 0; m < GetArenaSetup.playerClass.Length; m++)
				{
					while (GetArenaSetup.playerClass[m] != null && GetArenaSetup.playerClass[m] != SlugcatStats.Name.White && GetArenaSetup.playerClass[m] != SlugcatStats.Name.Yellow && !multiplayerUnlocks.ClassUnlocked(GetArenaSetup.playerClass[m]))
					{
						GetArenaSetup.playerClass[m] = NextClass(GetArenaSetup.playerClass[m]);
						playerClassButtons[m].menuLabel.text = Translate(SlugcatStats.getSlugcatName(GetArenaSetup.playerClass[m]));
						playerJoinButtons[m].portrait.fileName = ArenaImage(GetArenaSetup.playerClass[m], m);
						playerJoinButtons[m].portrait.LoadFile();
						playerJoinButtons[m].portrait.sprite.SetElementByName(playerJoinButtons[m].portrait.fileName);
					}
				}
			}
			for (int n = 1; n < playerJoinButtons.Length; n++)
			{
				playerJoinButtons[n].nextSelectable[0] = playerJoinButtons[n - 1];
			}
			for (int num4 = 0; num4 < playerJoinButtons.Length; num4++)
			{
				playerJoinButtons[num4].nextSelectable[2] = ((num4 == playerJoinButtons.Length - 1) ? playerJoinButtons[num4] : playerJoinButtons[num4 + 1]);
			}
			arenaSettingsInterface = new ArenaSettingsInterface(this, pages[0]);
			pages[0].subObjects.Add(arenaSettingsInterface);
		}
		if (!ModManager.MSC || currentGameType != MoreSlugcatsEnums.GameTypeID.Safari)
		{
			levelSelector = new LevelSelector(this, pages[0], currentGameType == ArenaSetup.GameTypeID.Sandbox);
			pages[0].subObjects.Add(levelSelector);
		}
		if (ModManager.MSC && currentGameType == MoreSlugcatsEnums.GameTypeID.Challenge)
		{
			levelSelector.RemoveSprites();
			pages[0].RemoveSubObject(levelSelector);
		}
		if (GetGameTypeSetup.savingAndLoadingSession && manager.rainWorld.options.ContainsArenaSitting())
		{
			float num5 = 130f;
			if (base.CurrLang == InGameTranslator.LanguageID.English)
			{
				num5 = 110f;
			}
			else if (base.CurrLang == InGameTranslator.LanguageID.French || base.CurrLang == InGameTranslator.LanguageID.German)
			{
				num5 = 150f;
			}
			resumeButton = new SimpleButton(this, pages[0], Translate("RESUME SESSION"), "RESUME", new Vector2(1166f - num5 - 120f, 50f), new Vector2(num5, 30f));
			pages[0].subObjects.Add(resumeButton);
		}
		playButton.menuLabel.text = ((resumeButton == null) ? Translate("PLAY!") : Translate("NEW SESSION"));
		if (resumeButton != null && base.CurrLang == InGameTranslator.LanguageID.French)
		{
			resumeButton.pos.x -= 20f;
			playButton.SetSize(new Vector2(130f, 30f));
			playButton.pos = new Vector2(1036f, 50f);
		}
		else
		{
			playButton.SetSize(new Vector2(110f, 30f));
			playButton.pos = new Vector2(1056f, 50f);
		}
		if (currentGameType == ArenaSetup.GameTypeID.Sandbox)
		{
			sandboxSettingsInterface = new SandboxSettingsInterface(this, pages[0]);
			pages[0].subObjects.Add(sandboxSettingsInterface);
			scene.AddIllustration(new MenuIllustration(this, scene, "", "SandboxShadow", new Vector2(-2.99f, 265.01f), crispPixels: true, anchorCenter: false));
			scene.AddIllustration(new MenuIllustration(this, scene, "", "SandboxTitle", new Vector2(-2.99f, 265.01f), crispPixels: true, anchorCenter: false));
			scene.flatIllustrations[scene.flatIllustrations.Count - 1].sprite.shader = manager.rainWorld.Shaders["MenuText"];
			MutualVerticalButtonBind(arenaSettingsInterface.enterDenReqs[0], arenaSettingsInterface.levelItemsCheckbox);
			MutualVerticalButtonBind(arenaSettingsInterface.enterDenReqs[2], arenaSettingsInterface.levelFoodCheckbox);
			arenaSettingsInterface.rainTimer.buttons[3].nextSelectable[1] = arenaSettingsInterface.scoreToEnterDen.buttons[2];
			MutualVerticalButtonBind(arenaSettingsInterface.scoreToEnterDen.buttons[1], arenaSettingsInterface.enterDenReqs[0]);
			MutualVerticalButtonBind(arenaSettingsInterface.scoreToEnterDen.buttons[0], arenaSettingsInterface.enterDenReqs[0]);
			MutualVerticalButtonBind(arenaSettingsInterface.scoreToEnterDen.buttons[2], arenaSettingsInterface.enterDenReqs[1]);
			MutualVerticalButtonBind(arenaSettingsInterface.scoreToEnterDen.buttons[3], arenaSettingsInterface.enterDenReqs[1]);
			MutualVerticalButtonBind(arenaSettingsInterface.scoreToEnterDen.buttons[4], arenaSettingsInterface.enterDenReqs[2]);
			MutualVerticalButtonBind(arenaSettingsInterface.scoreToEnterDen.buttons[5], arenaSettingsInterface.enterDenReqs[2]);
			if (ModManager.MSC)
			{
				MutualVerticalButtonBind(arenaSettingsInterface.enterDenReqs[1], playerClassButtons[2]);
				MutualVerticalButtonBind(arenaSettingsInterface.evilAICheckBox, playerClassButtons[3]);
				for (int num6 = 0; num6 <= 1; num6++)
				{
					MutualVerticalButtonBind(arenaSettingsInterface.spearsHitCheckbox, playerClassButtons[num6]);
				}
			}
			else
			{
				MutualVerticalButtonBind(arenaSettingsInterface.enterDenReqs[1], playerJoinButtons[2]);
				MutualVerticalButtonBind(arenaSettingsInterface.evilAICheckBox, playerJoinButtons[3]);
				for (int num7 = 0; num7 <= 1; num7++)
				{
					MutualVerticalButtonBind(arenaSettingsInterface.spearsHitCheckbox, playerJoinButtons[num7]);
				}
			}
		}
		else if (currentGameType == ArenaSetup.GameTypeID.Competitive)
		{
			scene.AddIllustration(new MenuIllustration(this, scene, "", "CompetitiveShadow", new Vector2(-2.99f, 265.01f), crispPixels: true, anchorCenter: false));
			scene.AddIllustration(new MenuIllustration(this, scene, "", "CompetitiveTitle", new Vector2(-2.99f, 265.01f), crispPixels: true, anchorCenter: false));
			scene.flatIllustrations[scene.flatIllustrations.Count - 1].sprite.shader = manager.rainWorld.Shaders["MenuText"];
			MutualVerticalButtonBind(playButton, arenaSettingsInterface.wildlifeArray.buttons[arenaSettingsInterface.wildlifeArray.buttons.Length - 1]);
			if (resumeButton != null)
			{
				MutualVerticalButtonBind(resumeButton, arenaSettingsInterface.wildlifeArray.buttons[arenaSettingsInterface.wildlifeArray.buttons.Length - 2]);
			}
			for (int num8 = 0; num8 < arenaSettingsInterface.wildlifeArray.buttons.Length; num8++)
			{
				arenaSettingsInterface.wildlifeArray.buttons[num8].nextSelectable[3] = ((num8 < arenaSettingsInterface.wildlifeArray.buttons.Length / 2 && resumeButton != null) ? resumeButton : playButton);
			}
			if (ModManager.MSC)
			{
				for (int num9 = 1; num9 >= 0; num9--)
				{
					MutualVerticalButtonBind(arenaSettingsInterface.spearsHitCheckbox, playerClassButtons[num9]);
				}
				for (int num10 = 2; num10 <= 3; num10++)
				{
					MutualVerticalButtonBind(arenaSettingsInterface.evilAICheckBox, playerClassButtons[num10]);
				}
			}
			else
			{
				for (int num11 = 1; num11 >= 0; num11--)
				{
					MutualVerticalButtonBind(arenaSettingsInterface.spearsHitCheckbox, playerJoinButtons[num11]);
				}
				for (int num12 = 2; num12 <= 3; num12++)
				{
					MutualVerticalButtonBind(arenaSettingsInterface.evilAICheckBox, playerJoinButtons[num12]);
				}
			}
		}
		else if (ModManager.MSC && currentGameType == MoreSlugcatsEnums.GameTypeID.Challenge)
		{
			scene.AddIllustration(new MenuIllustration(this, scene, string.Empty, "ChallengeShadow", new Vector2(-2.99f, 265.01f), crispPixels: true, anchorCenter: false));
			scene.AddIllustration(new MenuIllustration(this, scene, string.Empty, "ChallengeTitle", new Vector2(-2.99f, 265.01f), crispPixels: true, anchorCenter: false));
			scene.flatIllustrations[scene.flatIllustrations.Count - 1].sprite.shader = manager.rainWorld.Shaders["MenuText"];
			GetGameTypeSetup.challengeID = Mathf.Clamp(GetGameTypeSetup.challengeID, 1, totalChallenges);
			challengePageNum = (int)((float)(GetGameTypeSetup.challengeID - 1) / (float)MultiplayerUnlocks.TOTAL_CHALLENGES);
			PopulateChallengeButtons();
			challengeButtons[(GetGameTypeSetup.challengeID - 1) % MultiplayerUnlocks.TOTAL_CHALLENGES].toggled = true;
			challengeInfo = new ChallengeInformation(this, pages[0], GetGameTypeSetup.challengeID);
			pages[0].subObjects.Add(challengeInfo);
			playButton.inactive = !challengeInfo.unlocked;
			int num13 = 0;
			for (int num14 = 0; num14 < Math.Min(MultiplayerUnlocks.TOTAL_CHALLENGES, manager.rainWorld.progression.miscProgressionData.completedChallenges.Count); num14++)
			{
				if (manager.rainWorld.progression.miscProgressionData.completedChallenges[num14])
				{
					num13++;
				}
			}
			if (num13 >= MultiplayerUnlocks.TOTAL_CHALLENGES)
			{
				manager.CueAchievement(RainWorld.AchievementID.ChallengeMode, 1f);
			}
		}
		else if (ModManager.MSC && currentGameType == MoreSlugcatsEnums.GameTypeID.Safari)
		{
			List<string> entries = ExtEnum<MultiplayerUnlocks.SafariUnlockID>.values.entries;
			scene.AddIllustration(new MenuIllustration(this, scene, string.Empty, "SafariShadow", new Vector2(-2.99f, 265.01f), crispPixels: true, anchorCenter: false));
			scene.AddIllustration(new MenuIllustration(this, scene, string.Empty, "SafariTitle", new Vector2(-2.99f, 265.01f), crispPixels: true, anchorCenter: false));
			scene.flatIllustrations[scene.flatIllustrations.Count - 1].sprite.shader = manager.rainWorld.Shaders["MenuText"];
			float num15 = 550f;
			float num16 = 120f;
			safariPageNum = Mathf.CeilToInt((float)(GetGameTypeSetup.safariID + 1) / (float)SafariButtonsPerPage) - 1;
			PopulateSafariButtons();
			safariDisableRain = new CheckBox(this, pages[0], this, new Vector2(playButton.pos.x - 150f, playButton.pos.y), -40f, Translate("Disable Rain"), "DISABLERAIN");
			manager.rainWorld.safariRainDisable = safariDisableRain.Checked;
			safariDisableRain.selectable = true;
			pages[0].subObjects.Add(safariDisableRain);
			safariSlugcatButtons = new List<SimpleButton>();
			safariSlugcatLabels = new List<FSprite>();
			int num17 = SafariButtonsPerPage * safariPageNum;
			if (GetGameTypeSetup.safariID >= num17 && GetGameTypeSetup.safariID < num17 + SafariButtonsPerPage && !safariButtons[GetGameTypeSetup.safariID % SafariButtonsPerPage].buttonBehav.greyedOut)
			{
				safariTitle = new MenuLabel(this, pages[0], Translate(Region.GetRegionFullName(entries[GetGameTypeSetup.safariID], null)), new Vector2(manager.rainWorld.options.ScreenSize.x / 2f + (1366f - manager.rainWorld.options.ScreenSize.x) / 2f, num15 - num16 * 3f), Vector2.zero, bigText: true);
				PopulateSafariSlugcatButtons(entries[GetGameTypeSetup.safariID]);
			}
			else
			{
				safariTitle = new MenuLabel(this, pages[0], "", new Vector2(manager.rainWorld.options.ScreenSize.x / 2f + (1366f - manager.rainWorld.options.ScreenSize.x) / 2f, num15 - num16 * 3f), Vector2.zero, bigText: true);
			}
			pages[0].subObjects.Add(safariTitle);
			if (TotalSafariPages > 1)
			{
				prevSafariPageButton = new SimpleButton(this, pages[0], Translate("PREVIOUS"), "PREVSAFPAGE", new Vector2(400f, 50f), new Vector2(110f, 30f));
				pages[0].subObjects.Add(prevSafariPageButton);
				nextSafariPageButton = new SimpleButton(this, pages[0], Translate("NEXT"), "NEXTSAFPAGE", new Vector2(530f, 50f), new Vector2(110f, 30f));
				pages[0].subObjects.Add(nextSafariPageButton);
			}
		}
		for (int num18 = 0; num18 < scene.flatIllustrations.Count; num18++)
		{
			scene.flatIllustrations[num18].pos.x -= Menu.HorizontalMoveToGetCentered(manager);
		}
		if (ModManager.MSC && currentGameType == MoreSlugcatsEnums.GameTypeID.Safari)
		{
			MutualHorizontalButtonBind(safariDisableRain, (resumeButton == null) ? playButton : resumeButton);
			if (nextSafariPageButton == null)
			{
				MutualHorizontalButtonBind(backButton, safariDisableRain);
			}
			else
			{
				MutualHorizontalButtonBind(nextSafariPageButton, safariDisableRain);
				MutualHorizontalButtonBind(prevSafariPageButton, nextSafariPageButton);
				MutualHorizontalButtonBind(backButton, prevSafariPageButton);
			}
		}
		else if (!ModManager.MSC || currentGameType != MoreSlugcatsEnums.GameTypeID.Challenge)
		{
			MutualHorizontalButtonBind(backButton, (resumeButton == null) ? playButton : resumeButton);
		}
		MutualHorizontalButtonBind(prevButton, nextButton);
		MutualHorizontalButtonBind(nextButton, prevButton);
		if (currentGameType == ArenaSetup.GameTypeID.Competitive || currentGameType == ArenaSetup.GameTypeID.Sandbox)
		{
			MutualVerticalButtonBind(backButton, levelSelector.allLevelsList.scrollDownButton);
			for (int num19 = 0; num19 < playerJoinButtons.Length; num19++)
			{
				MutualVerticalButtonBind(playerJoinButtons[num19], infoButton);
			}
			MutualVerticalButtonBind(levelSelector.allLevelsList.scrollUpButton, prevButton);
			if (ModManager.MSC)
			{
				MutualVerticalButtonBind(arenaSettingsInterface.evilAICheckBox, playerClassButtons[3]);
			}
			else
			{
				MutualVerticalButtonBind(arenaSettingsInterface.evilAICheckBox, playerJoinButtons[3]);
			}
		}
		selectedObject = scrollSelectKeeper as MenuObject;
	}

	public void ClearGameTypeSpecificButtons()
	{
		if (levelSelector != null)
		{
			if (!ModManager.MSC || currentGameType != MoreSlugcatsEnums.GameTypeID.Challenge)
			{
				levelSelector.RemoveSprites();
				pages[0].RemoveSubObject(levelSelector);
			}
			levelSelector = null;
		}
		if (arenaSettingsInterface != null)
		{
			arenaSettingsInterface.RemoveSprites();
			pages[0].RemoveSubObject(arenaSettingsInterface);
			arenaSettingsInterface = null;
		}
		if (sandboxSettingsInterface != null)
		{
			sandboxSettingsInterface.RemoveSprites();
			pages[0].RemoveSubObject(sandboxSettingsInterface);
			sandboxSettingsInterface = null;
		}
		if (infoWindow != null)
		{
			infoWindow.RemoveSprites();
			infoWindow.owner.RemoveSubObject(infoWindow);
			infoWindow = null;
		}
		if (resumeButton != null)
		{
			resumeButton.RemoveSprites();
			pages[0].RemoveSubObject(resumeButton);
			resumeButton = null;
		}
		for (int num = scene.flatIllustrations.Count - 1; num >= 0; num--)
		{
			if (!ModManager.MSC || scene.flatIllustrations[num].fileName.ToLowerInvariant().EndsWith("shadow") || scene.flatIllustrations[num].fileName.ToLowerInvariant().EndsWith("title"))
			{
				scene.flatIllustrations[num].RemoveSprites();
				scene.RemoveSubObject(scene.flatIllustrations[num]);
			}
		}
		if (ModManager.MSC)
		{
			if (challengeInfo != null)
			{
				challengeInfo.RemoveSprites();
				pages[0].RemoveSubObject(challengeInfo);
				challengeInfo = null;
			}
			if (playerClassButtons != null)
			{
				for (int i = 0; i < playerClassButtons.Length; i++)
				{
					playerClassButtons[i].RemoveSprites();
					pages[0].RemoveSubObject(playerClassButtons[i]);
				}
				playerClassButtons = null;
			}
			if (playerJoinButtons != null)
			{
				for (int j = 0; j < playerJoinButtons.Length; j++)
				{
					playerJoinButtons[j].RemoveSprites();
					pages[0].RemoveSubObject(playerJoinButtons[j]);
				}
				playerJoinButtons = null;
			}
			if (challengeButtons != null)
			{
				for (int k = 0; k < challengeButtons.Length; k++)
				{
					challengeButtons[k].RemoveSprites();
					pages[0].RemoveSubObject(challengeButtons[k]);
					challengeChecks[k].RemoveFromContainer();
				}
				challengeButtons = null;
			}
			if (prevChallengePageButton != null)
			{
				prevChallengePageButton.RemoveSprites();
				pages[0].RemoveSubObject(prevChallengePageButton);
				prevChallengePageButton = null;
			}
			if (nextChallengePageButton != null)
			{
				nextChallengePageButton.RemoveSprites();
				pages[0].RemoveSubObject(nextChallengePageButton);
				nextChallengePageButton = null;
			}
			if (safariButtons != null)
			{
				for (int l = 0; l < safariButtons.Length; l++)
				{
					safariButtons[l].RemoveSprites();
					pages[0].RemoveSubObject(safariButtons[l]);
					safariIllustrations[l].RemoveSprites();
					pages[0].RemoveSubObject(safariIllustrations[l]);
				}
				for (int m = 0; m < safariSlugcatButtons.Count; m++)
				{
					safariSlugcatButtons[m].RemoveSprites();
					pages[0].RemoveSubObject(safariSlugcatButtons[m]);
					safariSlugcatLabels[m].RemoveFromContainer();
				}
				safariTitle.RemoveSprites();
				pages[0].RemoveSubObject(safariTitle);
				safariButtons = null;
				safariDisableRain.RemoveSprites();
				pages[0].RemoveSubObject(safariDisableRain);
				safariDisableRain = null;
			}
			if (prevSafariPageButton != null)
			{
				prevSafariPageButton.RemoveSprites();
				pages[0].RemoveSubObject(prevSafariPageButton);
				prevSafariPageButton = null;
			}
			if (nextSafariPageButton != null)
			{
				nextSafariPageButton.RemoveSprites();
				pages[0].RemoveSubObject(nextSafariPageButton);
				nextSafariPageButton = null;
			}
		}
		ResetSelection();
	}

	private int ApproximatePlayTime()
	{
		float num = ((arenaSettingsInterface.GetGameTypeSetup.sessionTimeLengthIndex < 0 || arenaSettingsInterface.GetGameTypeSetup.sessionTimeLengthIndex >= ArenaSetup.GameTypeSetup.SessionTimesInMinutesArray.Length) ? 0f : ArenaSetup.GameTypeSetup.SessionTimesInMinutesArray[arenaSettingsInterface.GetGameTypeSetup.sessionTimeLengthIndex]);
		int num2 = 0;
		for (int i = 0; i < GetArenaSetup.playersJoined.Length; i++)
		{
			if (GetArenaSetup.playersJoined[i])
			{
				num2++;
			}
		}
		if (num2 > 1)
		{
			float num3 = Mathf.InverseLerp(2f, 10f, num);
			num = Mathf.Lerp(num, 0.5f, (Custom.LerpMap(num2, 2f, 4f, 0.65f, 0.1f, 1f + num3) + 0.1f * num3) * (arenaSettingsInterface.GetGameTypeSetup.spearsHitPlayers ? 1f : 0.05f));
		}
		if (arenaSettingsInterface.GetGameTypeSetup.wildLifeSetting != ArenaSetup.GameTypeSetup.WildLifeSetting.Off)
		{
			num = Mathf.Lerp(num, 0.25f, Custom.LerpMap(arenaSettingsInterface.GetGameTypeSetup.wildLifeSetting.Index, 1f, 3f, 0.05f, 0.25f) + (arenaSettingsInterface.GetGameTypeSetup.evilAI ? 0.2f : 0f));
		}
		return Math.Max(1, Mathf.RoundToInt((float)(GetGameTypeSetup.playList.Count * GetGameTypeSetup.levelRepeats) * num));
	}

	public void OnExit()
	{
		if (!exiting)
		{
			exiting = true;
			manager.rainWorld.DeactivateAllPlayers();
			manager.arenaSetup.SaveToFile();
			manager.RequestMainProcessSwitch(ProcessManager.ProcessID.MainMenu);
			PlaySound(SoundID.MENU_Switch_Page_Out);
		}
	}

	public override void Update()
	{
		if (!requestingControllerConnections && !exiting)
		{
			for (int i = 1; i < manager.arenaSetup.playersJoined.Length; i++)
			{
				PlayerHandler playerHandler = manager.rainWorld.GetPlayerHandler(i);
				if (playerHandler != null)
				{
					Rewired.Player rewiredPlayer = UserInput.GetRewiredPlayer(playerHandler.profile, i);
					manager.arenaSetup.playersJoined[i] = rewiredPlayer.controllers.joystickCount > 0 || rewiredPlayer.controllers.hasKeyboard;
				}
				else
				{
					manager.arenaSetup.playersJoined[i] = false;
				}
				manager.rainWorld.GetPlayerSigningIn(i);
			}
		}
		base.Update();
		bool flag = RWInput.CheckPauseButton(0);
		if (flag && !lastPauseButton && manager.dialog == null)
		{
			OnExit();
		}
		lastPauseButton = flag;
		lastBlackFade = blackFade;
		float num = 0f;
		if (nextGameType != currentGameType)
		{
			num = 1f;
			if (blackFade == 1f && lastBlackFade == 1f)
			{
				ClearGameTypeSpecificButtons();
				currentGameType = nextGameType;
				InitiateGameTypeSpecificButtons();
			}
		}
		if (ModManager.MSC && currentGameType == MoreSlugcatsEnums.GameTypeID.Challenge && levelSelector != null && thumbsToBeLoaded.Count > 0)
		{
			levelSelector.Update();
		}
		if (blackFade < num)
		{
			blackFade = Custom.LerpAndTick(blackFade, num, 0.05f, 1f / 15f);
		}
		else
		{
			blackFade = Custom.LerpAndTick(blackFade, num, 0.05f, 0.125f);
		}
		bool flag2 = false;
		int num2 = 0;
		for (int j = 0; j < GetArenaSetup.playersJoined.Length; j++)
		{
			if (GetArenaSetup.playersJoined[j])
			{
				num2++;
			}
		}
		if (currentGameType == ArenaSetup.GameTypeID.Sandbox)
		{
			abovePlayButtonLabel.text = ((num2 == 0) ? Translate("No players joined!") : "");
			flag2 = true;
		}
		else if (num2 == 0)
		{
			abovePlayButtonLabel.text = Translate("No players joined!");
		}
		else if (currentGameType == ArenaSetup.GameTypeID.Competitive)
		{
			if (levelSelector.levelsPlaylist != null && levelSelector.levelsPlaylist.mismatchCounter > 20)
			{
				abovePlayButtonLabel.text = Translate("ERROR");
			}
			else
			{
				int num3 = GetGameTypeSetup.playList.Count * GetGameTypeSetup.levelRepeats;
				if (num3 == 0)
				{
					abovePlayButtonLabel.text = Regex.Replace(Translate("Select which levels to play<LINE>in the level selector"), "<LINE>", "\r\n");
				}
				else
				{
					int num4 = ApproximatePlayTime();
					string text = "";
					switch (num3)
					{
					case 1:
						text = Translate("ROUND SESSION");
						break;
					case 2:
					case 3:
					case 4:
						text = Translate("ROUNDS SESSION-ru2");
						break;
					default:
						text = Translate("ROUNDS SESSION");
						break;
					}
					text = ((!text.Contains("#")) ? (num3 + " " + text) : text.Replace("#", num3.ToString()));
					abovePlayButtonLabel.text = text + ((num4 > 0) ? ("\r\n" + Translate("Approximately") + " " + num4 + " " + ((num4 == 1) ? Translate("minute") : Translate("minutes"))) : "");
					flag2 = true;
				}
			}
		}
		APBLLastSin = APBLSin;
		APBLLastPulse = APBLPulse;
		if (!ModManager.MSC || currentGameType == MoreSlugcatsEnums.GameTypeID.Challenge || currentGameType == MoreSlugcatsEnums.GameTypeID.Safari)
		{
			flag2 = true;
		}
		if (ModManager.MSC && currentGameType == MoreSlugcatsEnums.GameTypeID.Safari && !manager.rainWorld.progression.miscProgressionData.GetTokenCollected(new MultiplayerUnlocks.SafariUnlockID(ExtEnum<MultiplayerUnlocks.SafariUnlockID>.values.GetEntry(GetGameTypeSetup.safariID))) && !MultiplayerUnlocks.CheckUnlockSafari())
		{
			flag2 = false;
		}
		if (!flag2)
		{
			APBLSin += 1f;
			APBLPulse = Custom.LerpAndTick(APBLPulse, 1f, 0.04f, 0.025f);
			playButton.buttonBehav.greyedOut = true;
		}
		else
		{
			APBLPulse = Custom.LerpAndTick(APBLPulse, 0f, 0.04f, 0.025f);
			playButton.buttonBehav.greyedOut = false;
		}
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		blackFadeSprite.alpha = Mathf.Lerp(lastBlackFade, blackFade, timeStacker);
		abovePlayButtonLabel.label.color = Color.Lerp(Menu.MenuRGB(MenuColors.DarkGrey), Color.Lerp(Menu.MenuRGB(MenuColors.DarkGrey), Menu.MenuRGB(MenuColors.White), 0.5f + 0.5f * Mathf.Sin(Mathf.Lerp(APBLLastSin, APBLSin, timeStacker) / 8f)), Mathf.Lerp(APBLLastPulse, APBLPulse, timeStacker));
	}

	public override void Singal(MenuObject sender, string message)
	{
		if (requestingControllerConnections)
		{
			return;
		}
		for (int i = 0; i < 4; i++)
		{
			if (manager.rainWorld.GetPlayerSigningIn(i))
			{
				return;
			}
		}
		switch (message)
		{
		case "PLAY!":
		case "RESUME":
			PlayerGraphics.customColors = null;
			if (ModManager.MSC && currentGameType == MoreSlugcatsEnums.GameTypeID.Challenge && playButton.inactive)
			{
				PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
				return;
			}
			if (message != "RESUME")
			{
				manager.rainWorld.options.DeleteArenaSitting();
			}
			PlaySound(SoundID.MENU_Start_New_Game);
			manager.arenaSetup.SaveToFile();
			if (ModManager.MSC && currentGameType == MoreSlugcatsEnums.GameTypeID.Safari)
			{
				manager.rainWorld.safariMode = true;
				List<string> entries = ExtEnum<MultiplayerUnlocks.SafariUnlockID>.values.entries;
				manager.rainWorld.safariRegion = entries[GetGameTypeSetup.safariID];
				int num = 0;
				bool flag = false;
				foreach (string entry in ExtEnum<SlugcatStats.Name>.values.entries)
				{
					SlugcatStats.Name name = new SlugcatStats.Name(entry);
					List<string> list = SlugcatStats.SlugcatStoryRegions(name).Concat(SlugcatStats.SlugcatOptionalRegions(name)).ToList();
					for (int j = 0; j < list.Count; j++)
					{
						bool flag2 = false;
						if (manager.rainWorld.progression.miscProgressionData.regionsVisited.ContainsKey(entries[GetGameTypeSetup.safariID]))
						{
							flag2 = manager.rainWorld.progression.miscProgressionData.regionsVisited[entries[GetGameTypeSetup.safariID]].Contains(entry);
						}
						if (entries[GetGameTypeSetup.safariID] == list[j] && (flag2 || (MultiplayerUnlocks.CheckUnlockSafari() && !SlugcatStats.HiddenOrUnplayableSlugcat(name))))
						{
							if (num == GetGameTypeSetup.safariSlugcatID)
							{
								manager.rainWorld.safariSlugcat = name;
								flag = true;
							}
							num++;
							break;
						}
					}
					if (flag)
					{
						break;
					}
				}
				manager.arenaSitting = null;
				manager.menuSetup.startGameCondition = ProcessManager.MenuSetup.StoryGameInitCondition.Load;
				manager.rainWorld.progression.ClearOutSaveStateFromMemory();
				manager.rainWorld.progression.GetOrInitiateSaveState(manager.rainWorld.safariSlugcat, null, manager.menuSetup, saveAsDeathOrQuit: false);
				UserInput.SetUserCount(1);
				manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Game);
				break;
			}
			InitializeSitting();
			manager.rainWorld.progression.ClearOutSaveStateFromMemory();
			if (manager.arenaSitting.ReadyToStart)
			{
				int num2 = 0;
				for (int k = 0; k < manager.arenaSetup.playersJoined.Length; k++)
				{
					if (manager.arenaSetup.playersJoined[k])
					{
						num2++;
					}
				}
				if (ModManager.MSC && currentGameType == MoreSlugcatsEnums.GameTypeID.Challenge)
				{
					UserInput.SetUserCount(1);
				}
				else
				{
					UserInput.SetUserCount(Mathf.Max(num2, 1));
				}
				manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Game);
			}
			else
			{
				manager.arenaSitting = null;
			}
			break;
		case "EXIT":
			OnExit();
			break;
		case "PREV":
			firstSafariSlugcatsButtonPopulate = false;
			nextGameType = GetArenaSetup.CycleGameType(-1);
			scrollSelectKeeper = prevButton;
			PlaySound(SoundID.MENU_Switch_Arena_Gametype);
			break;
		case "NEXT":
			firstSafariSlugcatsButtonPopulate = false;
			nextGameType = GetArenaSetup.CycleGameType(1);
			scrollSelectKeeper = nextButton;
			PlaySound(SoundID.MENU_Switch_Arena_Gametype);
			break;
		case "INFO":
			if (infoWindow != null)
			{
				infoWindow.wantToGoAway = true;
				PlaySound(SoundID.MENU_Remove_Level);
			}
			else
			{
				infoWindow = new InfoWindow(this, sender, new Vector2(0f, 0f));
				sender.subObjects.Add(infoWindow);
				PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
			}
			break;
		}
		if (!ModManager.MSC)
		{
			return;
		}
		if (message.Contains("CHALLENGE"))
		{
			int num3 = int.Parse(message.Substring("CHALLENGE".Length), NumberStyles.Any, CultureInfo.InvariantCulture) % MultiplayerUnlocks.TOTAL_CHALLENGES;
			if (num3 == 0)
			{
				num3 = MultiplayerUnlocks.TOTAL_CHALLENGES;
			}
			int num4 = MultiplayerUnlocks.TOTAL_CHALLENGES * challengePageNum;
			for (int l = 0; l < challengeButtons.Length; l++)
			{
				challengeButtons[l].toggled = false;
			}
			challengeButtons[num3 - 1].toggled = true;
			GetGameTypeSetup.challengeID = num3 + num4;
			challengeInfo.RemoveSprites();
			pages[0].RemoveSubObject(challengeInfo);
			challengeInfo = new ChallengeInformation(this, pages[0], num3 + num4);
			pages[0].subObjects.Add(challengeInfo);
			playButton.inactive = !challengeInfo.unlocked;
			PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
		}
		else if (message.Contains("SAFARI"))
		{
			string text = message.Substring("SAFARI".Length);
			List<string> entries2 = ExtEnum<MultiplayerUnlocks.SafariUnlockID>.values.entries;
			int num5 = SafariButtonsPerPage * safariPageNum;
			for (int m = num5; m < Math.Min(num5 + SafariButtonsPerPage, entries2.Count); m++)
			{
				if (entries2[m] == text)
				{
					safariButtons[m - num5].toggled = true;
					GetGameTypeSetup.safariID = m;
				}
				else
				{
					safariButtons[m - num5].toggled = false;
				}
			}
			safariTitle.text = Translate(Region.GetRegionFullName(text, null));
			PopulateSafariSlugcatButtons(text);
			PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
		}
		else if (message.Contains("SAFSLUG"))
		{
			for (int n = 0; n < safariSlugcatButtons.Count; n++)
			{
				if (safariSlugcatButtons[n].signalText == message)
				{
					safariSlugcatButtons[n].toggled = true;
					GetGameTypeSetup.safariSlugcatID = n;
					List<string> entries3 = ExtEnum<MultiplayerUnlocks.SafariUnlockID>.values.entries;
					string s = message.Substring("SAFSLUG".Length);
					SlugcatStats.Name slugcatIndex = SlugcatStats.Name.White;
					int result = 0;
					if (int.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
					{
						slugcatIndex = new SlugcatStats.Name(ExtEnum<SlugcatStats.Name>.values.GetEntry(result));
					}
					safariTitle.text = Translate(Region.GetRegionFullName(entries3[GetGameTypeSetup.safariID], slugcatIndex));
				}
				else
				{
					safariSlugcatButtons[n].toggled = false;
				}
			}
			PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
		}
		else
		{
			switch (message)
			{
			case "NEXTSAFPAGE":
				PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
				safariPageNum++;
				if (safariPageNum >= TotalSafariPages)
				{
					safariPageNum = 0;
				}
				PopulateSafariButtons();
				return;
			case "PREVSAFPAGE":
				PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
				safariPageNum--;
				if (safariPageNum < 0)
				{
					safariPageNum = TotalSafariPages - 1;
				}
				PopulateSafariButtons();
				return;
			case "NEXTCHALPAGE":
				PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
				challengePageNum++;
				if (challengePageNum >= TotalChallengePages)
				{
					challengePageNum = 0;
				}
				PopulateChallengeButtons();
				break;
			case "PREVCHALPAGE":
				PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
				challengePageNum--;
				if (challengePageNum < 0)
				{
					challengePageNum = TotalChallengePages - 1;
				}
				PopulateChallengeButtons();
				break;
			}
		}
		if (!(currentGameType == ArenaSetup.GameTypeID.Competitive) && !(currentGameType == ArenaSetup.GameTypeID.Sandbox))
		{
			return;
		}
		for (int num6 = 0; num6 < playerClassButtons.Length; num6++)
		{
			if (message == "CLASSCHANGE" + num6)
			{
				GetArenaSetup.playerClass[num6] = NextClass(GetArenaSetup.playerClass[num6]);
				playerClassButtons[num6].menuLabel.text = Translate(SlugcatStats.getSlugcatName(GetArenaSetup.playerClass[num6]));
				playerJoinButtons[num6].portrait.fileName = ArenaImage(GetArenaSetup.playerClass[num6], num6);
				playerJoinButtons[num6].portrait.LoadFile();
				playerJoinButtons[num6].portrait.sprite.SetElementByName(playerJoinButtons[num6].portrait.fileName);
				PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
			}
		}
	}

	public override string UpdateInfoText()
	{
		if (selectedObject is CheckBox)
		{
			switch ((selectedObject as CheckBox).IDString)
			{
			case "SPEARSHIT":
				if (!arenaSettingsInterface.GetGameTypeSetup.spearsHitPlayers)
				{
					return Translate("Eating contest");
				}
				return Translate("Player vs player deathmatch");
			case "EVILAI":
				if (!arenaSettingsInterface.GetGameTypeSetup.evilAI)
				{
					return Translate("Normal Rain World AI");
				}
				return Translate("Creatures are vicious and aggressive");
			case "LEVELITEMS":
				if (!GetGameTypeSetup.levelItems)
				{
					return Translate("No items except what you place in the editor");
				}
				return Translate("Standard items spawn on level");
			case "FLIESSPAWN":
				if (!GetGameTypeSetup.fliesSpawn)
				{
					return Translate("Barren level");
				}
				return Translate("Free food!");
			case "EARLYRAIN":
				if (!GetGameTypeSetup.rainWhenOnePlayerLeft)
				{
					return Translate("No rain timer exception");
				}
				return Translate("When only one player left, rain approaches");
			}
		}
		if (selectedObject is SandboxSettingsInterface.ScoreController.ScoreDragger)
		{
			return (selectedObject.owner as SandboxSettingsInterface.ScoreController).DescriptorString;
		}
		if (selectedObject is BigArrowButton)
		{
			switch ((selectedObject as BigArrowButton).signalText)
			{
			case "PREV":
				return Translate("Previous gametype");
			case "NEXT":
				return Translate("Next gametype");
			}
		}
		if (selectedObject is MultipleChoiceArray.MultipleChoiceButton)
		{
			switch ((selectedObject.owner as MultipleChoiceArray).IDString)
			{
			case "ROOMREPEAT":
				switch ((selectedObject as MultipleChoiceArray.MultipleChoiceButton).index)
				{
				case 0:
					return Translate("Play each level once");
				case 1:
					return Translate("Play each level twice");
				case 2:
					return Translate("Play each level three times");
				case 3:
					return Translate("Play each level four times");
				case 4:
					return Translate("Play each level five times");
				}
				break;
			case "SESSIONLENGTH":
				if ((selectedObject as MultipleChoiceArray.MultipleChoiceButton).index < 0 || (selectedObject as MultipleChoiceArray.MultipleChoiceButton).index >= ArenaSetup.GameTypeSetup.SessionTimesInMinutesArray.Length)
				{
					return Translate("No rain");
				}
				return ArenaSetup.GameTypeSetup.SessionTimesInMinutesArray[(selectedObject as MultipleChoiceArray.MultipleChoiceButton).index] + " " + (((selectedObject as MultipleChoiceArray.MultipleChoiceButton).index == 1) ? Translate("minute until rain") : Translate("minutes until rain"));
			case "WILDLIFE":
				switch ((selectedObject as MultipleChoiceArray.MultipleChoiceButton).index)
				{
				case 0:
					return Translate("No wildlife");
				case 1:
					return Translate("Low wildlife");
				case 2:
					return Translate("Medium wildlife");
				case 3:
					return Translate("High wildlife");
				}
				break;
			case "SCORETOENTERDEN":
				return Translate("Score required to exit level");
			}
		}
		if (selectedObject is SelectOneButton && (selectedObject as SelectOneButton).signalText == "DENENTRYRULE")
		{
			switch ((selectedObject as SelectOneButton).buttonArrayIndex)
			{
			case 0:
				return Translate("Allow players above a specific score to exit");
			case 1:
				return Translate("Allow players to exit when rain is close or only one player is left");
			case 2:
				return Translate("Always allow players to exit");
			}
		}
		if (selectedObject is LevelSelector.LevelItem)
		{
			if (selectedObject.owner is LevelSelector.AllLevelsSelectionList)
			{
				if (!GetGameTypeSetup.repeatSingleLevelForever)
				{
					return Translate("Add level to playlist");
				}
				return Translate("Select level");
			}
			if (selectedObject.owner is LevelSelector.LevelsPlaylist)
			{
				return Translate("Remove level from playlist");
			}
		}
		if (selectedObject is PlayerJoinButton)
		{
			if (GetArenaSetup.playersJoined[(selectedObject as PlayerJoinButton).index])
			{
				return Regex.Replace(Translate("Player <X> is in the game!"), "<X>", ((selectedObject as PlayerJoinButton).index + 1).ToString());
			}
			return Regex.Replace(Translate("Press to add Player <X> to the game"), "<X>", ((selectedObject as PlayerJoinButton).index + 1).ToString());
		}
		if (selectedObject is SymbolButton)
		{
			switch ((selectedObject as SymbolButton).signalText)
			{
			case "THUMBS":
				if ((selectedObject.owner as LevelSelector.LevelsList).ShowThumbsStatus)
				{
					return Translate("Showing level thumbnails");
				}
				return Translate("Showing level names");
			case "CLEAR":
				return Translate("Clear playlist");
			case "SHUFFLE":
				if (GetGameTypeSetup.shufflePlaylist)
				{
					return Translate("Playing levels in random order");
				}
				return Translate("Playing levels in selected order");
			case "CLEARSCORES":
				return Translate("Reset score setup");
			case "INFO":
				return Translate("Gametype description");
			case "NEXT_SYMBOLS":
				return Translate("Next Page");
			case "PREV_SYMBOLS":
				return Translate("Previous Page");
			}
		}
		if (selectedObject == playButton)
		{
			if (resumeButton != null)
			{
				return Translate("Start new playlist");
			}
			return Translate("Play!");
		}
		if (selectedObject == resumeButton)
		{
			return Translate("Resume unfinished playlist");
		}
		if (selectedObject == backButton)
		{
			return Translate("Back to main menu");
		}
		if (!ModManager.MSC)
		{
			return base.UpdateInfoText();
		}
		return CustomUpdateInfoText();
	}

	public override void ShutDownProcess()
	{
		base.ShutDownProcess();
		darkSprite.RemoveFromContainer();
		blackFadeSprite.RemoveFromContainer();
		if (!ModManager.MSC)
		{
			return;
		}
		if (challengeChecks != null)
		{
			for (int i = 0; i < challengeChecks.Length; i++)
			{
				challengeChecks[i].RemoveFromContainer();
			}
		}
		if (safariSlugcatLabels != null)
		{
			for (int j = 0; j < safariSlugcatLabels.Count; j++)
			{
				safariSlugcatLabels[j].RemoveFromContainer();
			}
		}
	}

	private void InitializeSitting()
	{
		manager.arenaSitting = new ArenaSitting(GetGameTypeSetup, multiplayerUnlocks);
		if (ModManager.MSC && currentGameType == MoreSlugcatsEnums.GameTypeID.Challenge)
		{
			manager.arenaSitting.AddPlayerWithClass(0, challengeInfo.meta.slugcatClass);
		}
		else
		{
			for (int i = 0; i < GetArenaSetup.playersJoined.Length; i++)
			{
				if (!GetArenaSetup.playersJoined[i])
				{
					continue;
				}
				if (ModManager.MSC)
				{
					if (GetArenaSetup.playerClass[i] == null)
					{
						List<SlugcatStats.Name> list = new List<SlugcatStats.Name>();
						list.Add(SlugcatStats.Name.White);
						list.Add(SlugcatStats.Name.Yellow);
						for (int j = 2; j < ExtEnum<SlugcatStats.Name>.values.Count; j++)
						{
							SlugcatStats.Name name = new SlugcatStats.Name(ExtEnum<SlugcatStats.Name>.values.GetEntry(j));
							if (multiplayerUnlocks.ClassUnlocked(name))
							{
								list.Add(name);
							}
						}
						for (int k = 0; k < list.Count - 2; k++)
						{
							int index = UnityEngine.Random.Range(k, list.Count);
							SlugcatStats.Name value = list[k];
							list[k] = list[index];
							list[index] = value;
						}
						manager.arenaSitting.AddPlayerWithClass(i, list[0]);
					}
					else
					{
						manager.arenaSitting.AddPlayerWithClass(i, GetArenaSetup.playerClass[i]);
					}
				}
				else
				{
					manager.arenaSitting.AddPlayer(i);
				}
			}
		}
		manager.arenaSitting.levelPlaylist = new List<string>();
		if (ModManager.MSC && GetGameTypeSetup.gameType == MoreSlugcatsEnums.GameTypeID.Challenge)
		{
			manager.arenaSitting.levelPlaylist.Add(challengeInfo.meta.arena);
		}
		if (GetGameTypeSetup.shufflePlaylist)
		{
			List<string> list2 = new List<string>();
			for (int l = 0; l < GetGameTypeSetup.playList.Count; l++)
			{
				list2.Add(GetGameTypeSetup.playList[l]);
			}
			while (list2.Count > 0)
			{
				int index2 = UnityEngine.Random.Range(0, list2.Count);
				for (int m = 0; m < GetGameTypeSetup.levelRepeats; m++)
				{
					manager.arenaSitting.levelPlaylist.Add(list2[index2]);
				}
				list2.RemoveAt(index2);
			}
		}
		else
		{
			for (int n = 0; n < GetGameTypeSetup.playList.Count; n++)
			{
				for (int num = 0; num < GetGameTypeSetup.levelRepeats; num++)
				{
					manager.arenaSitting.levelPlaylist.Add(GetGameTypeSetup.playList[n]);
				}
			}
		}
		if (GetGameTypeSetup.savingAndLoadingSession && manager.rainWorld.options.ContainsArenaSitting())
		{
			manager.arenaSitting.LoadFromFile(null, null, manager.rainWorld);
			manager.arenaSitting.attempLoadInGame = true;
		}
	}

	private void UserInput_OnControllerConfigurationChanged()
	{
		UserInput.OnControllerConfigurationChanged -= UserInput_OnControllerConfigurationChanged;
		requestingControllerConnections = false;
	}

	public bool IsChallengeUnlocked(PlayerProgression progression, int challengeNumber)
	{
		int num = MultiplayerUnlocks.TOTAL_CHALLENGES / 10;
		if (!File.Exists(ChallengeInformation.ChallengePath(challengeNumber)))
		{
			return false;
		}
		if (MultiplayerUnlocks.CheckUnlockChallenge())
		{
			return true;
		}
		if (challengeNumber <= num * 2)
		{
			return true;
		}
		if (challengeNumber <= num * 3)
		{
			if (!manager.rainWorld.progression.miscProgressionData.beaten_Survivor)
			{
				return manager.rainWorld.progression.miscProgressionData.redUnlocked;
			}
			return true;
		}
		if (challengeNumber <= num * 4)
		{
			return manager.rainWorld.progression.miscProgressionData.beaten_Gourmand;
		}
		if (challengeNumber <= num * 5)
		{
			return manager.rainWorld.progression.miscProgressionData.beaten_Artificer;
		}
		if (challengeNumber <= num * 6)
		{
			return manager.rainWorld.progression.miscProgressionData.beaten_Rivulet;
		}
		if (challengeNumber <= num * 7)
		{
			return manager.rainWorld.progression.miscProgressionData.beaten_SpearMaster;
		}
		if (challengeNumber <= num * 8)
		{
			return manager.rainWorld.progression.miscProgressionData.beaten_Saint;
		}
		int num2 = 0;
		for (int i = 0; i < Math.Min(MultiplayerUnlocks.TOTAL_CHALLENGES, progression.miscProgressionData.completedChallenges.Count); i++)
		{
			if (progression.miscProgressionData.completedChallenges[i])
			{
				num2++;
			}
		}
		if (challengeNumber <= MultiplayerUnlocks.TOTAL_CHALLENGES - 1)
		{
			return num2 >= (challengeNumber - num * 8) * 5;
		}
		if (challengeNumber == MultiplayerUnlocks.TOTAL_CHALLENGES)
		{
			return num2 >= MultiplayerUnlocks.TOTAL_CHALLENGES - 1;
		}
		return true;
	}

	public string ChallengeUnlockDescription(int challengeNumber)
	{
		int num = MultiplayerUnlocks.TOTAL_CHALLENGES / 10;
		int num2 = -1;
		string text = "";
		string text2 = "";
		if (challengeNumber > MultiplayerUnlocks.TOTAL_CHALLENGES)
		{
			return Translate("Undelete the challenge you just deleted to unlock.");
		}
		if (challengeNumber == MultiplayerUnlocks.TOTAL_CHALLENGES)
		{
			num2 = MultiplayerUnlocks.TOTAL_CHALLENGES - 1;
		}
		else if (challengeNumber > num * 8)
		{
			num2 = (challengeNumber - num * 8) * 5;
		}
		else if (challengeNumber > num * 7)
		{
			text = Translate("The Saint");
		}
		else if (challengeNumber > num * 6)
		{
			text = Translate("The Spearmaster");
		}
		else if (challengeNumber > num * 5)
		{
			text = Translate("The Rivulet");
		}
		else if (challengeNumber > num * 4)
		{
			text = Translate("The Artificer");
		}
		else if (challengeNumber > num * 3)
		{
			text = Translate("The Gourmand");
		}
		else if (challengeNumber > num * 2)
		{
			text = Translate("The Survivor");
			text2 = Translate("The Monk");
		}
		if (num2 >= 0)
		{
			return Translate("Clear a total of ## challenges to unlock.").Replace("##", num2.ToString());
		}
		if (text2 != "")
		{
			return Translate("Clear the game as <X> or <Y> to unlock.").Replace("<X>", text).Replace("<Y>", text2);
		}
		if (text != "")
		{
			return Translate("Clear the game as ## to unlock.").Replace("##", text);
		}
		return "do nothing to unlock.";
	}

	public Color ChallengeTextColor(int challengeNumber)
	{
		int num = MultiplayerUnlocks.TOTAL_CHALLENGES / 10;
		if (challengeNumber > num)
		{
			if (challengeNumber <= num * 2)
			{
				return new Color(1f, 1f, 23f / 51f);
			}
			if (challengeNumber <= num * 3)
			{
				return new Color(1f, 23f / 51f, 23f / 51f);
			}
			if (challengeNumber <= num * 4)
			{
				return new Color(0.94118f, 0.75686f, 0.59216f);
			}
			if (challengeNumber <= num * 5)
			{
				return new Color(31f / 51f, 0.3019608f, 0.4117647f);
			}
			if (challengeNumber <= num * 6)
			{
				return new Color(0.56863f, 0.8f, 0.94118f);
			}
			if (challengeNumber <= num * 7)
			{
				return new Color(0.5137255f, 0.3529412f, 0.6392157f);
			}
			if (challengeNumber <= num * 8)
			{
				return new Color(0.66667f, 0.9451f, 0.33725f);
			}
		}
		return Color.white;
	}

	public bool GetChecked(CheckBox box)
	{
		if (box.IDString == "DISABLERAIN")
		{
			return GetGameTypeSetup.safariRainDisabled > 0;
		}
		return false;
	}

	public void SetChecked(CheckBox box, bool c)
	{
		if (box.IDString == "DISABLERAIN")
		{
			GetGameTypeSetup.safariRainDisabled = (c ? 1 : 0);
			manager.rainWorld.safariRainDisable = c;
		}
	}

	public SlugcatStats.Name NextClass(SlugcatStats.Name curClass)
	{
		SlugcatStats.Name name = null;
		if (curClass == null)
		{
			name = new SlugcatStats.Name(ExtEnum<SlugcatStats.Name>.values.GetEntry(0));
		}
		else
		{
			if (curClass.Index >= ExtEnum<SlugcatStats.Name>.values.Count - 1 || curClass.Index == -1)
			{
				return null;
			}
			name = new SlugcatStats.Name(ExtEnum<SlugcatStats.Name>.values.GetEntry(curClass.Index + 1));
		}
		if (SlugcatStats.HiddenOrUnplayableSlugcat(name))
		{
			return NextClass(name);
		}
		if (name != SlugcatStats.Name.White && name != SlugcatStats.Name.Yellow && !multiplayerUnlocks.ClassUnlocked(name))
		{
			return NextClass(name);
		}
		return name;
	}

	public string ArenaImage(SlugcatStats.Name classID, int color)
	{
		if (classID == null)
		{
			return "MultiplayerPortrait" + color + "2";
		}
		return "MultiplayerPortrait" + color + "1-" + classID.ToString();
	}

	public string CustomUpdateInfoText()
	{
		if (selectedObject is SimpleButton && (selectedObject as SimpleButton).signalText.Contains("CLASSCHANGE"))
		{
			string text = Translate("Change the slugcat class/abilities for Player <X>");
			string pattern = "<X>";
			string replacement = (int.Parse((selectedObject as SimpleButton).signalText[(selectedObject as SimpleButton).signalText.Length - 1].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture) + 1).ToString();
			return Regex.Replace(text, pattern, replacement);
		}
		if (nextSafariPageButton != null && selectedObject == nextSafariPageButton)
		{
			return Translate("Next Page");
		}
		if (prevSafariPageButton != null && selectedObject == prevSafariPageButton)
		{
			return Translate("Previous Page");
		}
		if (nextChallengePageButton != null && selectedObject == nextChallengePageButton)
		{
			return Translate("Next Page");
		}
		if (prevChallengePageButton != null && selectedObject == prevChallengePageButton)
		{
			return Translate("Previous Page");
		}
		return base.UpdateInfoText();
	}

	public void PopulateSafariSlugcatButtons(string regionName)
	{
		for (int i = 0; i < safariSlugcatButtons.Count; i++)
		{
			safariSlugcatButtons[i].RemoveSprites();
			pages[0].RemoveSubObject(safariSlugcatButtons[i]);
		}
		safariSlugcatButtons.Clear();
		for (int j = 0; j < safariSlugcatLabels.Count; j++)
		{
			safariSlugcatLabels[j].RemoveFromContainer();
		}
		safariSlugcatLabels.Clear();
		for (int k = 0; k < ExtEnum<SlugcatStats.Name>.values.Count; k++)
		{
			SlugcatStats.Name name = new SlugcatStats.Name(ExtEnum<SlugcatStats.Name>.values.GetEntry(k));
			List<string> list = SlugcatStats.SlugcatStoryRegions(name);
			list.AddRange(SlugcatStats.SlugcatOptionalRegions(name));
			for (int l = 0; l < list.Count; l++)
			{
				bool flag = false;
				if (manager.rainWorld.progression.miscProgressionData.regionsVisited.ContainsKey(regionName))
				{
					flag = manager.rainWorld.progression.miscProgressionData.regionsVisited[regionName].Contains(name.value);
				}
				if (regionName == list[l] && (flag || (MultiplayerUnlocks.CheckUnlockSafari() && !SlugcatStats.HiddenOrUnplayableSlugcat(name))))
				{
					SimpleButton item = new SimpleButton(this, pages[0], "", "SAFSLUG" + name.Index, new Vector2(manager.rainWorld.options.ScreenSize.x / 2f + (1366f - manager.rainWorld.options.ScreenSize.x) / 2f, 110f), new Vector2(48f, 48f));
					FSprite fSprite = new FSprite("Kill_Slugcat");
					fSprite.color = PlayerGraphics.DefaultSlugcatColor(name);
					safariSlugcatLabels.Add(fSprite);
					safariSlugcatButtons.Add(item);
					pages[0].Container.AddChild(fSprite);
					pages[0].subObjects.Add(item);
					break;
				}
			}
		}
		if (GetGameTypeSetup.safariSlugcatID >= 0 && GetGameTypeSetup.safariSlugcatID < safariSlugcatButtons.Count && !firstSafariSlugcatsButtonPopulate)
		{
			for (int m = 0; m < safariSlugcatButtons.Count; m++)
			{
				safariSlugcatButtons[m].toggled = false;
			}
			safariSlugcatButtons[GetGameTypeSetup.safariSlugcatID].toggled = true;
		}
		else
		{
			for (int n = 0; n < safariSlugcatButtons.Count; n++)
			{
				safariSlugcatButtons[n].toggled = false;
			}
			safariSlugcatButtons[0].toggled = true;
			GetGameTypeSetup.safariSlugcatID = 0;
		}
		firstSafariSlugcatsButtonPopulate = true;
		float num = (float)safariSlugcatButtons.Count * 56f;
		for (int num2 = 0; num2 < safariSlugcatButtons.Count; num2++)
		{
			SimpleButton simpleButton = safariSlugcatButtons[num2];
			simpleButton.pos.x = simpleButton.pos.x + ((float)num2 * 56f - num * 0.5f);
			safariSlugcatLabels[num2].x = simpleButton.pos.x + simpleButton.size.x * 0.5f - (1366f - manager.rainWorld.options.ScreenSize.x) / 2f;
			safariSlugcatLabels[num2].y = simpleButton.pos.y + simpleButton.size.y * 0.5f;
		}
	}

	public int ButtonsOnSafariPage(int num)
	{
		if (num == 0)
		{
			return Math.Min(SafariButtonsPerPage, ExtEnum<MultiplayerUnlocks.SafariUnlockID>.values.Count);
		}
		if (num == TotalSafariPages - 1)
		{
			return ExtEnum<MultiplayerUnlocks.SafariUnlockID>.values.Count % SafariButtonsPerPage;
		}
		return SafariButtonsPerPage;
	}

	public void PopulateSafariButtons()
	{
		if (safariButtons != null)
		{
			for (int i = 0; i < safariButtons.Length; i++)
			{
				safariButtons[i].RemoveSprites();
				pages[0].RemoveSubObject(safariButtons[i]);
			}
			safariButtons = null;
		}
		if (safariIllustrations != null)
		{
			for (int j = 0; j < safariIllustrations.Length; j++)
			{
				safariIllustrations[j].RemoveSprites();
				pages[0].RemoveSubObject(safariIllustrations[j]);
			}
			safariIllustrations = null;
		}
		if (ExtEnum<MultiplayerUnlocks.SafariUnlockID>.values.Count < SafariButtonsPerPage * safariPageNum + ButtonsOnSafariPage(safariPageNum))
		{
			safariPageNum = 0;
			GetGameTypeSetup.safariID = 0;
		}
		List<string> entries = ExtEnum<MultiplayerUnlocks.SafariUnlockID>.values.entries;
		safariButtons = new SimpleButton[ButtonsOnSafariPage(safariPageNum)];
		safariIllustrations = new MenuIllustration[safariButtons.Length];
		float num = 550f;
		float num2 = 120f;
		float num3 = 120f;
		float num4 = 8f;
		int num5 = 7;
		int num6 = SafariButtonsPerPage * safariPageNum;
		for (int k = 0; k < safariButtons.Length; k++)
		{
			Vector2 vector = new Vector2(299f - num2 * 0.5f + (float)(k % num5) * (num2 + num4), num - num3 * 0.5f - (num3 + num4) * (float)(k / num5));
			safariButtons[k] = new SimpleButton(this, pages[0], "", "SAFARI" + entries[k + num6], vector, new Vector2(num2, num3));
			if (manager.rainWorld.progression.miscProgressionData.GetTokenCollected(new MultiplayerUnlocks.SafariUnlockID(ExtEnum<MultiplayerUnlocks.SafariUnlockID>.values.GetEntry(k + num6))) || MultiplayerUnlocks.CheckUnlockSafari())
			{
				safariIllustrations[k] = new MenuIllustration(this, pages[0], string.Empty, "Safari_" + entries[k + num6], vector + new Vector2(10f, 10f), crispPixels: true, anchorCenter: false);
			}
			else
			{
				safariButtons[k].buttonBehav.greyedOut = true;
				safariIllustrations[k] = new MenuIllustration(this, pages[0], string.Empty, "Safari_Locked", vector + new Vector2(10f, 10f), crispPixels: true, anchorCenter: false);
			}
			pages[0].subObjects.Add(safariButtons[k]);
			pages[0].subObjects.Add(safariIllustrations[k]);
		}
		if (GetGameTypeSetup.safariID >= num6 && GetGameTypeSetup.safariID < num6 + SafariButtonsPerPage && !safariButtons[GetGameTypeSetup.safariID % SafariButtonsPerPage].buttonBehav.greyedOut)
		{
			safariButtons[GetGameTypeSetup.safariID % SafariButtonsPerPage].toggled = true;
		}
		MutualVerticalButtonBind(safariButtons[0], prevButton);
		infoButton.nextSelectable[3] = ((resumeButton == null) ? playButton : resumeButton);
		if (safariButtons.Length >= num5)
		{
			MutualVerticalButtonBind(safariButtons[num5 - 1], infoButton);
			MutualHorizontalButtonBind(safariButtons[num5 - 1], safariButtons[0]);
		}
		infoButton.nextSelectable[0] = safariButtons[Math.Min(num5 - 1, safariButtons.Length - 1)];
	}

	public int ButtonsOnChallengePage(int num)
	{
		if (num == 0)
		{
			return Math.Min(MultiplayerUnlocks.TOTAL_CHALLENGES, totalChallenges);
		}
		if (num == TotalChallengePages - 1)
		{
			return totalChallenges % MultiplayerUnlocks.TOTAL_CHALLENGES;
		}
		return MultiplayerUnlocks.TOTAL_CHALLENGES;
	}

	public void PopulateChallengeButtons()
	{
		if (challengeButtons != null)
		{
			for (int i = 0; i < challengeButtons.Length; i++)
			{
				challengeButtons[i].RemoveSprites();
				pages[0].RemoveSubObject(challengeButtons[i]);
			}
			safariButtons = null;
		}
		if (challengeChecks != null)
		{
			for (int j = 0; j < challengeChecks.Length; j++)
			{
				challengeChecks[j].RemoveFromContainer();
			}
		}
		int num = ButtonsOnChallengePage(challengePageNum);
		challengeButtons = new SimpleButton[num];
		challengeChecks = new FSprite[num];
		int num2 = MultiplayerUnlocks.TOTAL_CHALLENGES * challengePageNum;
		int num3 = MultiplayerUnlocks.TOTAL_CHALLENGES / 5;
		for (int k = 0; k < challengeButtons.Length; k++)
		{
			int num4 = k + num2;
			challengeButtons[k] = new SimpleButton(this, pages[0], ((num4 < 9) ? "0" : "") + (num4 + 1), "CHALLENGE" + (num4 + 1), new Vector2(277f + (float)(k % num3) * 58f, 570f - 58f * (float)(k / num3)), new Vector2(52f, 52f));
			challengeChecks[k] = new FSprite("Menu_Symbol_CheckBox");
			challengeChecks[k].x = challengeButtons[k].pos.x + challengeButtons[k].size.x / 2f - (1366f - manager.rainWorld.options.ScreenSize.x) / 2f + 13f;
			challengeChecks[k].y = challengeButtons[k].pos.y + challengeButtons[k].size.y / 2f - 13f;
			challengeChecks[k].color = new Color(7f / 85f, 32f / 51f, 31f / 85f);
			if (manager.rainWorld.progression.miscProgressionData.completedChallenges.Count > num4 && manager.rainWorld.progression.miscProgressionData.completedChallenges[num4])
			{
				challengeChecks[k].isVisible = true;
			}
			else
			{
				challengeChecks[k].isVisible = false;
			}
			pages[0].subObjects.Add(challengeButtons[k]);
			pages[0].Container.AddChild(challengeChecks[k]);
			if (!IsChallengeUnlocked(manager.rainWorld.progression, num4 + 1))
			{
				challengeButtons[k].labelColor = Menu.MenuColor(MenuColors.DarkGrey);
				challengeButtons[k].roundedRect.borderColor = Menu.MenuColor(MenuColors.DarkGrey);
			}
			else
			{
				Vector3 vector = Custom.RGB2HSL(ChallengeTextColor(num4 + 1));
				challengeButtons[k].labelColor = new HSLColor(vector.x, vector.y, vector.z);
			}
		}
		if (nextChallengePageButton == null && prevChallengePageButton == null)
		{
			if (TotalChallengePages > 1)
			{
				prevChallengePageButton = new SimpleButton(this, pages[0], Translate("PREVIOUS"), "PREVCHALPAGE", new Vector2(400f, 50f), new Vector2(110f, 30f));
				pages[0].subObjects.Add(prevChallengePageButton);
				nextChallengePageButton = new SimpleButton(this, pages[0], Translate("NEXT"), "NEXTCHALPAGE", new Vector2(530f, 50f), new Vector2(110f, 30f));
				pages[0].subObjects.Add(nextChallengePageButton);
				MutualHorizontalButtonBind(nextChallengePageButton, playButton);
				MutualHorizontalButtonBind(prevChallengePageButton, nextChallengePageButton);
				MutualHorizontalButtonBind(backButton, prevChallengePageButton);
			}
			else
			{
				MutualHorizontalButtonBind(backButton, (resumeButton == null) ? playButton : resumeButton);
			}
		}
		int num5 = (int)((float)(num - 1) / (float)num3) * num3;
		int num6 = num3;
		if (num < num6)
		{
			num6 = num;
		}
		for (int l = 0; l < challengeButtons.Length; l++)
		{
			if (l % num3 == num3 - 1)
			{
				MutualHorizontalButtonBind(challengeButtons[l], challengeButtons[l - (num3 - 1)]);
			}
			else if (l == num - 1 && l != 0)
			{
				MutualHorizontalButtonBind(challengeButtons[l], challengeButtons[l - l % num3]);
			}
			if (l >= num5)
			{
				if (l < num5 + 3)
				{
					challengeButtons[l].nextSelectable[3] = backButton;
				}
				else if (l > num5 + (num3 - 4) || l == num - 1)
				{
					challengeButtons[l].nextSelectable[3] = playButton;
				}
				else
				{
					challengeButtons[l].nextSelectable[3] = challengeButtons[l - num5];
				}
			}
			if (l >= num6)
			{
				continue;
			}
			if (l < 3)
			{
				challengeButtons[l].nextSelectable[1] = prevButton;
				continue;
			}
			if (l > num3 - 4 || l == num6 - 1)
			{
				challengeButtons[l].nextSelectable[1] = infoButton;
				continue;
			}
			int num7 = l + num5;
			if (num7 >= num)
			{
				num7 -= num3;
			}
			if (num7 > 0)
			{
				challengeButtons[l].nextSelectable[1] = challengeButtons[num7];
			}
		}
		if (num6 - 1 >= 0)
		{
			infoButton.nextSelectable[3] = challengeButtons[num6 - 1];
		}
	}

	public bool MineForGameComplete(SlugcatStats.Name slugcat)
	{
		if (!manager.rainWorld.progression.IsThereASavedGame(slugcat))
		{
			return false;
		}
		if (manager.rainWorld.progression.currentSaveState != null && manager.rainWorld.progression.currentSaveState.saveStateNumber == slugcat)
		{
			if (!manager.rainWorld.progression.currentSaveState.deathPersistentSaveData.ascended)
			{
				return manager.rainWorld.progression.currentSaveState.deathPersistentSaveData.altEnding;
			}
			return true;
		}
		string[] progLinesFromMemory = manager.rainWorld.progression.GetProgLinesFromMemory();
		if (progLinesFromMemory.Length == 0)
		{
			return false;
		}
		for (int i = 0; i < progLinesFromMemory.Length; i++)
		{
			string[] array = Regex.Split(progLinesFromMemory[i], "<progDivB>");
			if (array.Length != 2 || !(array[0] == "SAVE STATE") || !(array[1][21].ToString() == slugcat.value))
			{
				continue;
			}
			List<SaveStateMiner.Target> list = new List<SaveStateMiner.Target>();
			list.Add(new SaveStateMiner.Target(">ASCENDED", null, "<dpA>", 20));
			list.Add(new SaveStateMiner.Target(">ALTENDING", null, "<dpA>", 20));
			List<SaveStateMiner.Result> list2 = SaveStateMiner.Mine(manager.rainWorld, array[1], list);
			bool flag = false;
			bool flag2 = false;
			for (int j = 0; j < list2.Count; j++)
			{
				string name = list2[j].name;
				if (name == ">ASCENDED")
				{
					flag = true;
				}
				else if (name == ">ALTENDING")
				{
					flag2 = true;
				}
			}
			return flag || flag2;
		}
		return false;
	}
}
