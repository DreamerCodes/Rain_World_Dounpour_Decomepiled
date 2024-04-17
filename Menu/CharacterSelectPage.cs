using System;
using System.Collections.Generic;
using System.Linq;
using Expedition;
using JollyCoop.JollyMenu;
using Kittehface.Framework20;
using Menu.Remix;
using Menu.Remix.MixedUI;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace Menu;

public class CharacterSelectPage : PositionedMenuObject
{
	public MenuLabel localizedSubtitle;

	public FContainer sprites;

	public SelectOneButton[] slugcatButtons;

	public List<MenuIllustration> slugcatPortraits;

	public MenuLabel slugcatName;

	public List<MenuLabel> challengePreviews;

	public FSprite pageTitle;

	public FSprite nameSeparatorLeft;

	public FSprite nameSeparatorRight;

	public FSprite bottomSeparatorLeft;

	public FSprite bottomSeparatorRight;

	public MenuLabel[] statsLabels;

	public MenuLabel unlockHeading;

	public MenuLabel burdenHeading;

	public MenuLabel runUnlocks;

	public MenuLabel runBurdens;

	public HoldButton confirmExpedition;

	public MenuLabel slugcatDescription;

	public MenuScene.SceneID slugcatScene;

	public OpHoldButton abandonButton;

	public MenuTabWrapper menuTabWrapper;

	public List<FSprite> strikethroughs;

	public SymbolButton jukeBoxButton;

	public UnlocksIndicator unlocksIndicator;

	public UIelementWrapper abandonWrapper;

	public MenuLabel nowPlaying;

	public float leftAnchor;

	public float rightAnchor;

	public bool waitForSaveData;

	public bool firstLoad;

	public bool muted;

	private bool pendingStart;

	private bool requestingControllerConnections;

	private bool pressedLoadButton;

	public SimpleButton debugToggle;

	public SymbolButton jollyToggleConfigMenu;

	public MenuLabel jollyOptionsLabel;

	public MenuLabel jollyPlayerCountLabel;

	public CharacterSelectPage(Menu menu, MenuObject owner, Vector2 pos)
		: base(menu, owner, pos)
	{
		leftAnchor = (menu as ExpeditionMenu).leftAnchor;
		rightAnchor = (menu as ExpeditionMenu).rightAnchor;
		pageTitle = new FSprite("expeditionpage");
		pageTitle.SetAnchor(0.5f, 0f);
		pageTitle.x = 680f;
		pageTitle.y = 680f;
		pageTitle.shader = menu.manager.rainWorld.Shaders["MenuText"];
		Container.AddChild(pageTitle);
		menuTabWrapper = new MenuTabWrapper(menu, this);
		subObjects.Add(menuTabWrapper);
		abandonButton = new OpHoldButton(new Vector2(910f, 75f), new Vector2(110f, 30f), menu.Translate("ABANDON"), 200f);
		abandonButton.colorEdge = new Color(0.6f, 0f, 0f);
		abandonButton.OnPressDone += AbandonButton_OnPressDone;
		abandonButton.description = " ";
		abandonWrapper = new UIelementWrapper(menuTabWrapper, abandonButton);
		slugcatButtons = new SelectOneButton[ExpeditionGame.playableCharacters.Count];
		slugcatPortraits = new List<MenuIllustration>();
		challengePreviews = new List<MenuLabel>();
		strikethroughs = new List<FSprite>();
		MenuLabel item = new MenuLabel(menu, this, menu.Translate("CHOOSE YOUR SLUGCAT"), new Vector2(680f, 640f), default(Vector2), bigText: false)
		{
			label = 
			{
				color = new Color(0.5f, 0.5f, 0.5f)
			}
		};
		subObjects.Add(item);
		for (int i = 0; i < ExpeditionGame.playableCharacters.Count; i++)
		{
			bool greyedOut = !ExpeditionGame.unlockedExpeditionSlugcats.Contains(ExpeditionGame.playableCharacters[i]);
			if (i < 3)
			{
				slugcatButtons[i] = new SelectOneButton(menu, this, "", "SLUG-" + i, new Vector2(525f + 110f * (float)i, 525f), new Vector2(94f, 94f), slugcatButtons, i);
				subObjects.Add(slugcatButtons[i]);
			}
			else if (i <= 7)
			{
				slugcatButtons[i] = new SelectOneButton(menu, this, "", "SLUG-" + i, new Vector2(415f + 110f * (float)(i - 3), 410f), new Vector2(94f, 94f), slugcatButtons, i);
				subObjects.Add(slugcatButtons[i]);
			}
			slugcatButtons[i].buttonBehav.greyedOut = greyedOut;
			slugcatPortraits.Add(GetSlugcatPortrait(ExpeditionGame.playableCharacters[i], slugcatButtons[i].pos + new Vector2(5f, 5f)));
			slugcatPortraits[i].sprite.SetAnchor(0f, 0f);
			subObjects.Add(slugcatPortraits[i]);
		}
		MenuLabel item2 = new MenuLabel(menu, this, menu.Translate("Jukebox"), new Vector2(900f, 530f), default(Vector2), bigText: false)
		{
			label = 
			{
				color = new Color(0.7f, 0.7f, 0.7f)
			}
		};
		subObjects.Add(item2);
		jukeBoxButton = new SymbolButton(menu, this, "musicSymbol", "JUKEBOX", new Vector2(875f, 550f));
		jukeBoxButton.roundedRect.size = new Vector2(50f, 50f);
		jukeBoxButton.size = jukeBoxButton.roundedRect.size;
		subObjects.Add(jukeBoxButton);
		if (menu.manager.rainWorld.options.language != InGameTranslator.LanguageID.English)
		{
			localizedSubtitle = new MenuLabel(menu, this, menu.Translate("-EXPEDITION-"), new Vector2(683f, 740f), default(Vector2), bigText: false);
			localizedSubtitle.label.color = new Color(0.5f, 0.5f, 0.5f);
			subObjects.Add(localizedSubtitle);
		}
		slugcatName = new MenuLabel(menu, this, menu.Translate(SlugcatStats.getSlugcatName(ExpeditionData.slugcatPlayer)), new Vector2(680f, 380f), default(Vector2), bigText: true);
		slugcatName.label.shader = menu.manager.rainWorld.Shaders["MenuText"];
		subObjects.Add(slugcatName);
		slugcatDescription = new MenuLabel(menu, this, menu.Translate(""), new Vector2(680f, 310f), default(Vector2), bigText: true);
		slugcatDescription.label.color = new Color(0.8f, 0.8f, 0.8f);
		subObjects.Add(slugcatDescription);
		nameSeparatorLeft = new FSprite("LinearGradient200");
		nameSeparatorLeft.SetAnchor(0.5f, 0f);
		nameSeparatorLeft.x = 680f;
		nameSeparatorLeft.y = 360f;
		nameSeparatorLeft.rotation = 270f;
		nameSeparatorLeft.scaleY = 2f;
		Container.AddChild(nameSeparatorLeft);
		nameSeparatorRight = new FSprite("LinearGradient200");
		nameSeparatorRight.SetAnchor(0.5f, 0f);
		nameSeparatorRight.x = 680f;
		nameSeparatorRight.y = 360f;
		nameSeparatorRight.rotation = 90f;
		nameSeparatorRight.scaleY = 2f;
		Container.AddChild(nameSeparatorRight);
		bottomSeparatorLeft = new FSprite("LinearGradient200");
		bottomSeparatorLeft.SetAnchor(0.5f, 0f);
		bottomSeparatorLeft.x = 680f;
		bottomSeparatorLeft.y = 65f;
		bottomSeparatorLeft.rotation = 270f;
		bottomSeparatorLeft.scaleY = 2f;
		Container.AddChild(bottomSeparatorLeft);
		bottomSeparatorRight = new FSprite("LinearGradient200");
		bottomSeparatorRight.SetAnchor(0.5f, 0f);
		bottomSeparatorRight.x = 680f;
		bottomSeparatorRight.y = 65f;
		bottomSeparatorRight.rotation = 90f;
		bottomSeparatorRight.scaleY = 2f;
		Container.AddChild(bottomSeparatorRight);
		nowPlaying = new MenuLabel(menu, owner, "", new Vector2(683f, 35f), default(Vector2), bigText: true);
		nowPlaying.label.color = new Color(0.5f, 0.5f, 0.5f);
		nowPlaying.label.shader = menu.manager.rainWorld.Shaders["MenuTextCustom"];
		subObjects.Add(nowPlaying);
		if (ExpeditionData.devMode)
		{
			debugToggle = new SimpleButton(menu, this, ExpeditionData.devMode ? "DEBUG ON" : "DEBUG OFF", "DEBUG", new Vector2(leftAnchor + 20f, 20f), new Vector2(75f, 30f));
			debugToggle.rectColor = (ExpeditionData.devMode ? new HSLColor(0.31f, 1f, 0.5f) : new HSLColor(0f, 1f, 0.5f));
			debugToggle.labelColor = debugToggle.rectColor.Value;
			subObjects.Add(debugToggle);
		}
		if (ModManager.JollyCoop)
		{
			new Vector2(50f, menu.manager.rainWorld.screenSize.y - 100f);
			jollyToggleConfigMenu = new SymbolButton(menu, this, "coop", "JOLLY_TOGGLE_CONFIG", new Vector2(440f, 550f));
			jollyToggleConfigMenu.roundedRect.size = new Vector2(50f, 50f);
			jollyToggleConfigMenu.size = jollyToggleConfigMenu.roundedRect.size;
			subObjects.Add(jollyToggleConfigMenu);
			jollyPlayerCountLabel = new MenuLabel(menu, this, menu.Translate("Expedition-Players").Replace("<num_p>", ValueConverter.ConvertToString(Custom.rainWorld.options.JollyPlayerCount)), jollyToggleConfigMenu.pos + new Vector2(jollyToggleConfigMenu.size.x / 2f, -20f), Vector2.zero, bigText: false);
			jollyPlayerCountLabel.label.color = new Color(0.7f, 0.7f, 0.7f);
			subObjects.Add(jollyPlayerCountLabel);
		}
		try
		{
			(menu as ExpeditionMenu).currentSelection = ExpeditionGame.playableCharacters.IndexOf(ExpeditionData.slugcatPlayer);
		}
		catch
		{
			(menu as ExpeditionMenu).currentSelection = 0;
		}
		menu.selectedObject = slugcatButtons[1];
	}

	public void AbandonButton_OnPressDone(UIfocusable trigger)
	{
		menu.manager.arenaSitting = null;
		menu.manager.rainWorld.progression.currentSaveState = null;
		menu.manager.rainWorld.progression.miscProgressionData.currentlySelectedSinglePlayerSlugcat = ExpeditionData.slugcatPlayer;
		menu.manager.rainWorld.progression.WipeSaveState(ExpeditionData.slugcatPlayer);
		ExpeditionData.AddExpeditionRequirements(ExpeditionData.slugcatPlayer, ended: true);
		global::Expedition.Expedition.coreFile.Save(runEnded: true);
		menu.manager.RequestMainProcessSwitch(ExpeditionEnums.ProcessID.ExpeditionMenu);
		menu.PlaySound(SoundID.MENU_Continue_Game);
	}

	private void UserInput_OnControllerConfigurationChanged()
	{
		UserInput.OnControllerConfigurationChanged -= UserInput_OnControllerConfigurationChanged;
		requestingControllerConnections = false;
		if (pendingStart)
		{
			Singal(null, "LOAD");
		}
	}

	public override void Singal(MenuObject sender, string message)
	{
		if (requestingControllerConnections)
		{
			return;
		}
		base.Singal(sender, message);
		if ((menu as ExpeditionMenu).pagesMoving)
		{
			return;
		}
		if (message == "LOAD")
		{
			menu.manager.rainWorld.options.ResetJollyProfileRequest();
			pressedLoadButton = true;
			LoadGame();
		}
		if (message == "NEW")
		{
			(menu as ExpeditionMenu).challengeSelect.Singal(null, "DESELECT");
			(menu as ExpeditionMenu).UpdatePage(2);
			(menu as ExpeditionMenu).MovePage(new Vector2(-1500f, 0f));
		}
		if (message == "JUKEBOX")
		{
			menu.manager.RequestMainProcessSwitch(ExpeditionEnums.ProcessID.ExpeditionJukebox);
			menu.PlaySound(SoundID.MENU_Switch_Page_Out);
		}
		if (message == "JOLLY_TOGGLE_CONFIG")
		{
			JollySetupDialog dialog = new JollySetupDialog(closeButtonPos: new Vector2(1000f - (1366f - menu.manager.rainWorld.options.ScreenSize.x) / 2f, menu.manager.rainWorld.screenSize.y - 100f), name: ExpeditionData.slugcatPlayer, manager: menu.manager);
			menu.manager.ShowDialog(dialog);
			menu.manager.rainWorld.options.ResetJollyProfileRequest();
		}
		if (message == "DEBUG")
		{
			if (ExpeditionData.devMode)
			{
				ExpeditionData.devMode = false;
			}
			else
			{
				ExpeditionData.devMode = true;
			}
			menu.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.MainMenu);
			menu.PlaySound(SoundID.MENU_Player_Join_Game);
		}
	}

	public void LoadGame()
	{
		if (ModManager.CoopAvailable)
		{
			for (int i = 1; i < menu.manager.rainWorld.options.JollyPlayerCount; i++)
			{
				menu.manager.rainWorld.ActivatePlayer(i);
			}
			for (int j = menu.manager.rainWorld.options.JollyPlayerCount; j < 4; j++)
			{
				menu.manager.rainWorld.DeactivatePlayer(j);
			}
		}
		menu.manager.arenaSitting = null;
		menu.manager.rainWorld.progression.currentSaveState = null;
		menu.manager.rainWorld.progression.miscProgressionData.currentlySelectedSinglePlayerSlugcat = ExpeditionData.slugcatPlayer;
		if (menu.manager.rainWorld.progression.IsThereASavedGame(ExpeditionData.slugcatPlayer))
		{
			global::Expedition.Expedition.coreFile.Save(runEnded: false);
			menu.manager.menuSetup.startGameCondition = ProcessManager.MenuSetup.StoryGameInitCondition.Load;
			menu.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Game);
			menu.PlaySound(SoundID.MENU_Continue_Game);
		}
	}

	public override void Update()
	{
		base.Update();
		if (!firstLoad && (menu as ExpeditionMenu).fullyLoaded)
		{
			UpdateSelectedSlugcat((menu as ExpeditionMenu).currentSelection);
			firstLoad = true;
		}
		if (waitForSaveData && global::Expedition.Expedition.coreFile.coreLoaded && menu.manager.rainWorld.progression != null && menu.manager.rainWorld.progression.progressionLoaded && menu.manager.rainWorld.progression.miscProgressionData != null)
		{
			ExpLog.Log("Save Data Loaded");
			ClearStats();
			UpdateStats();
			UpdateChallengePreview();
			if (ExpeditionData.challengeList != null && ExpeditionData.challengeList.Count == 0)
			{
				for (int i = 0; i < 3; i++)
				{
					ChallengeOrganizer.AssignChallenge(i, hidden: false);
				}
			}
			SetUpSelectables();
			waitForSaveData = false;
		}
		if ((menu as ExpeditionMenu).currentScene != slugcatScene)
		{
			(menu as ExpeditionMenu).currentScene = slugcatScene;
			(menu as ExpeditionMenu).pendingBackgroundChange = true;
		}
		if (ModManager.JollyCoop)
		{
			jollyToggleConfigMenu.GetButtonBehavior.greyedOut = false;
			jollyPlayerCountLabel.text = menu.Translate("Expedition-Players").Replace("<num_p>", ValueConverter.ConvertToString(Custom.rainWorld.options.JollyPlayerCount));
		}
		if (menu.manager.rainWorld.options.IsJollyProfileRequesting())
		{
			if (pressedLoadButton)
			{
				LoadGame();
			}
		}
		else
		{
			pressedLoadButton = false;
		}
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		pageTitle.x = Mathf.Lerp(owner.page.lastPos.x, owner.page.pos.x, timeStacker) + 680f;
		pageTitle.y = Mathf.Lerp(owner.page.lastPos.y, owner.page.pos.y, timeStacker) + 680f;
		nameSeparatorLeft.x = Mathf.Lerp(owner.page.lastPos.x, owner.page.pos.x, timeStacker) + 680f;
		nameSeparatorLeft.y = Mathf.Lerp(owner.page.lastPos.y, owner.page.pos.y, timeStacker) + 360f;
		nameSeparatorRight.x = Mathf.Lerp(owner.page.lastPos.x, owner.page.pos.x, timeStacker) + 680f;
		nameSeparatorRight.y = Mathf.Lerp(owner.page.lastPos.y, owner.page.pos.y, timeStacker) + 360f;
		bottomSeparatorLeft.x = Mathf.Lerp(owner.page.lastPos.x, owner.page.pos.x, timeStacker) + 680f;
		bottomSeparatorLeft.y = Mathf.Lerp(owner.page.lastPos.y, owner.page.pos.y, timeStacker) + 65f;
		bottomSeparatorRight.x = Mathf.Lerp(owner.page.lastPos.x, owner.page.pos.x, timeStacker) + 680f;
		bottomSeparatorRight.y = Mathf.Lerp(owner.page.lastPos.y, owner.page.pos.y, timeStacker) + 65f;
		if (strikethroughs != null && strikethroughs.Count > 0 && challengePreviews != null && challengePreviews.Count > 0)
		{
			for (int i = 0; i < strikethroughs.Count; i++)
			{
				strikethroughs[i].x = Mathf.Lerp(owner.page.lastPos.x, owner.page.pos.x, timeStacker) + challengePreviews[i].pos.x - 5f;
				strikethroughs[i].y = Mathf.Lerp(owner.page.lastPos.y, owner.page.pos.y, timeStacker) + challengePreviews[i].pos.y - 2f;
			}
		}
	}

	public void SetUpSelectables()
	{
		jukeBoxButton.nextSelectable[1] = (menu as ExpeditionMenu).manualButton;
		jukeBoxButton.nextSelectable[2] = (menu as ExpeditionMenu).manualButton;
		(menu as ExpeditionMenu).muteButton.nextSelectable[3] = slugcatButtons[0];
		(menu as ExpeditionMenu).muteButton.nextSelectable[2] = slugcatButtons[0];
		(menu as ExpeditionMenu).muteButton.nextSelectable[1] = confirmExpedition;
		if (ModManager.MSC)
		{
			jukeBoxButton.nextSelectable[3] = slugcatButtons[7];
		}
		else
		{
			jukeBoxButton.nextSelectable[3] = confirmExpedition;
		}
		if (ModManager.JollyCoop)
		{
			jollyToggleConfigMenu.nextSelectable[1] = (menu as ExpeditionMenu).muteButton;
			jollyToggleConfigMenu.nextSelectable[0] = (menu as ExpeditionMenu).muteButton;
			jollyToggleConfigMenu.nextSelectable[2] = slugcatButtons[0];
			if (ModManager.MSC)
			{
				jollyToggleConfigMenu.nextSelectable[3] = slugcatButtons[3];
			}
			else
			{
				jollyToggleConfigMenu.nextSelectable[3] = confirmExpedition;
			}
		}
		for (int i = 0; i < slugcatButtons.Length; i++)
		{
			if (ModManager.MSC)
			{
				slugcatButtons[3].nextSelectable[3] = confirmExpedition;
				slugcatButtons[4].nextSelectable[3] = confirmExpedition;
				slugcatButtons[5].nextSelectable[3] = confirmExpedition;
				slugcatButtons[6].nextSelectable[3] = confirmExpedition;
				slugcatButtons[7].nextSelectable[3] = confirmExpedition;
			}
			else
			{
				slugcatButtons[0].nextSelectable[3] = confirmExpedition;
				slugcatButtons[1].nextSelectable[3] = confirmExpedition;
				slugcatButtons[2].nextSelectable[3] = confirmExpedition;
			}
		}
	}

	public override void RemoveSprites()
	{
		base.RemoveSprites();
		pageTitle.RemoveFromContainer();
		nameSeparatorLeft.RemoveFromContainer();
		nameSeparatorRight.RemoveFromContainer();
	}

	public void UpdateChallengePreview()
	{
		if (strikethroughs != null)
		{
			for (int i = 0; i < strikethroughs.Count; i++)
			{
				if (strikethroughs[i] != null)
				{
					strikethroughs[i].RemoveFromContainer();
				}
			}
		}
		strikethroughs = new List<FSprite>();
		if (challengePreviews == null)
		{
			return;
		}
		for (int j = 0; j < challengePreviews.Count; j++)
		{
			challengePreviews[j].RemoveSprites();
			challengePreviews[j].RemoveSubObject(challengePreviews[j]);
		}
		challengePreviews = new List<MenuLabel>();
		if (!menu.manager.rainWorld.progression.IsThereASavedGame(ExpeditionData.slugcatPlayer) || ExpeditionData.challengeList == null)
		{
			return;
		}
		for (int k = 0; k < ExpeditionData.challengeList.Count; k++)
		{
			ExpeditionData.challengeList[k].UpdateDescription();
			challengePreviews.Add(new MenuLabel(menu, this, ExpeditionData.challengeList[k].description, new Vector2(315f, 340f - 25f * (float)k), default(Vector2), bigText: true));
			challengePreviews[k].label.alignment = FLabelAlignment.Left;
			challengePreviews[k].label.color = (ExpeditionData.challengeList[k].hidden ? new Color(1f, 0.75f, 0f) : new Color(0.7f, 0.7f, 0.7f));
			if (ExpeditionData.activeMission != "")
			{
				Color color = PlayerGraphics.DefaultSlugcatColor(new SlugcatStats.Name(ExpeditionProgression.missionList.Find((ExpeditionProgression.Mission m) => m.key == ExpeditionData.activeMission).slugcat));
				challengePreviews[k].label.color = (ExpeditionData.challengeList[k].hidden ? new Color(1f, 0.75f, 0f) : color);
			}
			subObjects.Add(challengePreviews[k]);
			FSprite fSprite = new FSprite("pixel");
			fSprite.SetAnchor(0f, 0.5f);
			fSprite.scaleY = 2f;
			fSprite.scaleX = challengePreviews[k].label.textRect.size.x + 10f;
			fSprite.alpha = (ExpeditionData.challengeList[k].completed ? 1f : 0f);
			fSprite.color = (ExpeditionData.challengeList[k].hidden ? new Color(1f, 0.75f, 0f) : new Color(0.7f, 0.7f, 0.7f));
			strikethroughs.Add(fSprite);
			Container.AddChild(strikethroughs[k]);
		}
	}

	public void ClearStats()
	{
		if (statsLabels != null)
		{
			for (int i = 0; i < statsLabels.Length; i++)
			{
				statsLabels[i].RemoveSprites();
				statsLabels[i].RemoveSubObject(statsLabels[i]);
			}
		}
		if (confirmExpedition != null)
		{
			confirmExpedition.RemoveSprites();
			confirmExpedition.RemoveSubObject(confirmExpedition);
		}
		if (unlocksIndicator != null)
		{
			unlocksIndicator.RemoveSprites();
			RemoveSubObject(unlocksIndicator);
			unlocksIndicator = null;
		}
	}

	public void UpdateStats()
	{
		SlugcatSelectMenu.SaveGameData saveGameData = SlugcatSelectMenu.MineForSaveData(menu.manager, ExpeditionData.slugcatPlayer);
		if (saveGameData != null)
		{
			string text = ValueConverter.ConvertToString(saveGameData.cycle);
			string text2 = ValueConverter.ConvertToString(Custom.IntClamp(saveGameData.karma, 0, saveGameData.karmaCap) + 1);
			string text3 = Custom.SecondsToMinutesAndSecondsString((int)TimeSpan.FromSeconds((double)saveGameData.gameTimeAlive + (double)saveGameData.gameTimeDead).TotalSeconds);
			statsLabels = new MenuLabel[6];
			statsLabels[0] = new MenuLabel(menu, this, menu.Translate("CYCLE :"), new Vector2(890f, 340f), default(Vector2), bigText: true);
			statsLabels[0].label.alignment = FLabelAlignment.Left;
			statsLabels[1] = new MenuLabel(menu, this, text, new Vector2(1040f, 340f), default(Vector2), bigText: true);
			statsLabels[1].label.alignment = FLabelAlignment.Right;
			statsLabels[2] = new MenuLabel(menu, this, menu.Translate("KARMA :"), new Vector2(890f, 315f), default(Vector2), bigText: true);
			statsLabels[2].label.alignment = FLabelAlignment.Left;
			statsLabels[3] = new MenuLabel(menu, this, text2, new Vector2(1040f, 315f), default(Vector2), bigText: true);
			statsLabels[3].label.alignment = FLabelAlignment.Right;
			statsLabels[4] = new MenuLabel(menu, this, menu.Translate("TIME :"), new Vector2(890f, 290f), default(Vector2), bigText: true);
			statsLabels[4].label.alignment = FLabelAlignment.Left;
			statsLabels[5] = new MenuLabel(menu, this, text3, new Vector2(1040f, 290f), default(Vector2), bigText: true);
			statsLabels[5].label.alignment = FLabelAlignment.Right;
			for (int i = 0; i < statsLabels.Length; i++)
			{
				statsLabels[i].label.color = new Color(0.5f, 0.5f, 0.5f);
				subObjects.Add(statsLabels[i]);
			}
			slugcatDescription.text = "";
			string displayText = ((ExpeditionData.activeMission == "") ? menu.Translate("CONTINUE<LINE>EXPEDITION").Replace("<LINE>", "\n") : menu.Translate("CONTINUE<LINE>MISSION").Replace("<LINE>", "\n"));
			confirmExpedition = new HoldButton(menu, this, displayText, "LOAD", new Vector2(965f, 190f), 30f);
			confirmExpedition.buttonBehav.greyedOut = ExpeditionData.MissingRequirements(ExpeditionData.slugcatPlayer);
			List<string> list = new List<string>();
			for (int j = 0; j < ExpeditionProgression.perkGroups.Keys.Count; j++)
			{
				list.AddRange(ExpeditionProgression.perkGroups.ElementAt(j).Value);
			}
			for (int k = 0; k < ExpeditionProgression.burdenGroups.Keys.Count; k++)
			{
				list.AddRange(ExpeditionProgression.burdenGroups.ElementAt(k).Value);
			}
			if (ExpeditionData.devMode)
			{
				for (int l = 0; l < list.Count; l++)
				{
					ExpLog.Log(list[l]);
				}
			}
			for (int m = 0; m < ExpeditionGame.activeUnlocks.Count; m++)
			{
				if (!list.Contains(ExpeditionGame.activeUnlocks[m]) && ExpeditionGame.activeUnlocks[m] != "")
				{
					confirmExpedition.buttonBehav.greyedOut = true;
				}
			}
			subObjects.Add(confirmExpedition);
			if (unlocksIndicator == null && menu.manager.rainWorld.progression.IsThereASavedGame(ExpeditionData.slugcatPlayer))
			{
				unlocksIndicator = new UnlocksIndicator(menu, this, new Vector2(315f, 195f), centered: false);
				subObjects.Add(unlocksIndicator);
			}
			abandonButton.Show();
		}
		else
		{
			confirmExpedition = new HoldButton(menu, this, menu.Translate("NEW<LINE>EXPEDITION").Replace("<LINE>", "\n"), "NEW", new Vector2(680f, 180f), 30f);
			subObjects.Add(confirmExpedition);
			abandonButton.Hide();
		}
	}

	public void UpdateSelectedSlugcat(int num)
	{
		SlugcatStats.Name name = (ExpeditionData.slugcatPlayer = ExpeditionGame.playableCharacters[num]);
		menu.manager.arenaSitting = null;
		menu.manager.rainWorld.progression.currentSaveState = null;
		menu.manager.rainWorld.progression.miscProgressionData.currentlySelectedSinglePlayerSlugcat = ExpeditionData.slugcatPlayer;
		if (ModManager.JollyCoop)
		{
			menu.manager.rainWorld.options.jollyPlayerOptionsArray[0].playerClass = name;
		}
		if (unlocksIndicator != null)
		{
			unlocksIndicator.RemoveSprites();
			RemoveSubObject(unlocksIndicator);
			unlocksIndicator = null;
		}
		for (int i = 0; i < slugcatPortraits.Count; i++)
		{
			if (i == num)
			{
				slugcatPortraits[i].color = Color.white;
			}
			else
			{
				slugcatPortraits[i].color = (slugcatButtons[i].buttonBehav.greyedOut ? new Color(0.1f, 0.1f, 0.1f) : new Color(0.25f, 0.25f, 0.25f));
			}
			slugcatPortraits[i].alpha = (slugcatButtons[i].buttonBehav.greyedOut ? 0.7f : 1f);
		}
		string text = "";
		string text2 = "";
		if (name == SlugcatStats.Name.White)
		{
			text = menu.Translate("THE SURVIVOR");
			text2 = menu.Translate("With no unique skills to boast about, The Survivor embarks on an<LINE>Expedition to test their resolve").Replace("<LINE>", Environment.NewLine);
			slugcatScene = MenuScene.SceneID.Landscape_SU;
		}
		else if (name == SlugcatStats.Name.Yellow)
		{
			text = menu.Translate("THE MONK");
			text2 = menu.Translate("Attuned with nature and empathetic to its inhabitants, The Monk departs<LINE>to find their place in the world").Replace("<LINE>", Environment.NewLine);
			slugcatScene = MenuScene.SceneID.Yellow_Intro_B;
		}
		else if (name == SlugcatStats.Name.Red)
		{
			text = menu.Translate("THE HUNTER");
			text2 = menu.Translate("Dextrous and handy with a spear, The Hunter sets forth once again<LINE>into a world of danger").Replace("<LINE>", Environment.NewLine);
			slugcatScene = MenuScene.SceneID.Landscape_LF;
		}
		else if (ModManager.MSC && name == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
		{
			text = menu.Translate("THE GOURMAND");
			text2 = menu.Translate("A brave explorer, hungry for their next adventure, or perhaps<LINE>their next meal").Replace("<LINE>", Environment.NewLine);
			slugcatScene = MoreSlugcatsEnums.MenuSceneID.Landscape_OE;
		}
		else if (ModManager.MSC && name == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
		{
			text = menu.Translate("THE ARTIFICER");
			text2 = menu.Translate("An explosive fighter and combat expert, The Artificer journeys once<LINE>again into the unknown").Replace("<LINE>", Environment.NewLine);
			slugcatScene = MoreSlugcatsEnums.MenuSceneID.Landscape_LC;
		}
		else if (ModManager.MSC && name == MoreSlugcatsEnums.SlugcatStatsName.Spear)
		{
			text = menu.Translate("THE SPEARMASTER");
			text2 = menu.Translate("A spear expert forced to fight to survive, their Expedition is<LINE>sure to be one fraught with peril").Replace("<LINE>", Environment.NewLine);
			slugcatScene = MoreSlugcatsEnums.MenuSceneID.Landscape_DM;
		}
		else if (ModManager.MSC && name == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
		{
			text = menu.Translate("THE RIVULET");
			text2 = menu.Translate("Extremely agile and an adept swimmer, The Rivulet speeds<LINE>towards their goal out of necessity").Replace("<LINE>", Environment.NewLine);
			slugcatScene = MoreSlugcatsEnums.MenuSceneID.Landscape_MS;
		}
		else if (ModManager.MSC && name == MoreSlugcatsEnums.SlugcatStatsName.Saint)
		{
			text = menu.Translate("THE SAINT");
			text2 = menu.Translate("A pacifist at heart, The Saint endures the bitter cold and searches<LINE>for their purpose").Replace("<LINE>", Environment.NewLine);
			slugcatScene = MoreSlugcatsEnums.MenuSceneID.Landscape_CL;
		}
		slugcatName.text = text;
		slugcatDescription.text = text2;
		(menu as ExpeditionMenu).pendingBackgroundChange = true;
		ChallengeOrganizer.filterChallengeTypes = new List<string>();
		waitForSaveData = true;
	}

	public void ReloadSlugcatPortraits()
	{
		for (int i = 0; i < ExpeditionGame.playableCharacters.Count; i++)
		{
			slugcatPortraits[i].RemoveSprites();
			RemoveSubObject(slugcatPortraits[i]);
			slugcatPortraits[i] = GetSlugcatPortrait(ExpeditionGame.playableCharacters[i], slugcatButtons[i].pos + new Vector2(5f, 5f));
			slugcatPortraits[i].sprite.SetAnchor(0f, 0f);
			subObjects.Add(slugcatPortraits[i]);
			if (i == ExpeditionGame.playableCharacters.IndexOf(ExpeditionData.slugcatPlayer))
			{
				slugcatPortraits[i].color = Color.white;
			}
			else
			{
				slugcatPortraits[i].color = (slugcatButtons[i].buttonBehav.greyedOut ? new Color(0.1f, 0.1f, 0.1f) : new Color(0.25f, 0.25f, 0.25f));
			}
			slugcatPortraits[i].alpha = (slugcatButtons[i].buttonBehav.greyedOut ? 0.7f : 1f);
		}
	}

	public MenuIllustration GetSlugcatPortrait(SlugcatStats.Name slugcat, Vector2 pos)
	{
		string folderName = "illustrations";
		string fileName = "";
		if (slugcat == SlugcatStats.Name.White)
		{
			fileName = "multiplayerportrait01";
		}
		else if (slugcat == SlugcatStats.Name.Yellow)
		{
			fileName = "multiplayerportrait11";
		}
		else if (slugcat == SlugcatStats.Name.Red)
		{
			fileName = "multiplayerportrait21";
		}
		else if (ModManager.MSC && slugcat == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
		{
			fileName = "multiplayerportrait41-gourmand";
		}
		else if (ModManager.MSC && slugcat == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
		{
			fileName = "multiplayerportrait41-artificer";
		}
		else if (ModManager.MSC && slugcat == MoreSlugcatsEnums.SlugcatStatsName.Spear)
		{
			fileName = "multiplayerportrait41-spear";
		}
		else if (ModManager.MSC && slugcat == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
		{
			fileName = "multiplayerportrait41-rivulet";
		}
		else if (ModManager.MSC && slugcat == MoreSlugcatsEnums.SlugcatStatsName.Saint)
		{
			fileName = "multiplayerportrait41-saint";
		}
		return new MenuIllustration(menu, this, folderName, fileName, pos, crispPixels: true, anchorCenter: true);
	}
}
