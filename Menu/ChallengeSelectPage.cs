using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Expedition;
using Kittehface.Framework20;
using Menu.Remix;
using Menu.Remix.MixedUI;
using RWCustom;
using UnityEngine;

namespace Menu;

public class ChallengeSelectPage : PositionedMenuObject
{
	public SlugcatStats.Name currentSlugcat;

	public float leftAnchor;

	public float rightAnchor;

	public FSprite pageTitle;

	public FSprite easySprite;

	public FSprite hardSprite;

	public FSprite[] indicators;

	public SimpleButton backButton;

	public BigSimpleButton[] challengeButtons;

	public SymbolButton[] hiddenToggles;

	public SymbolButton minusButton;

	public SymbolButton plusButton;

	public HorizontalSlider difficultySlider;

	public MenuLabel difficultyLabel;

	public MenuLabel easyLabel;

	public MenuLabel hardLabel;

	public bool updatePending;

	public float colorCounter;

	public MenuLabel pointsLabel;

	public MenuScene.SceneID sceneID = MenuScene.SceneID.Intro_3_In_Tree;

	public SymbolButton leftPage;

	public SymbolButton rightPage;

	public SymbolButton filterButton;

	public SymbolButton randomButton;

	public MenuLabel missionLabel;

	public SimpleButton deselectMissionButton;

	public HSLColor missionColor = new HSLColor(0.6f, 1f, 0.6f);

	public FSprite missionGradient;

	public FSprite sideSpriteLeft;

	public FSprite sideSpriteRight;

	public FSprite slugcatSprite;

	public UIelementWrapper unlockWrapper;

	public UIelementWrapper startWrapper;

	public string missionName = "";

	public bool firstUpdate;

	public MenuLabel localizedSubtitle;

	public MenuLabel missionTime;

	public SymbolButton symbolButton1;

	private bool pendingStart;

	private bool pressedStartButton;

	private bool requestingControllerConnections;

	public SimpleButton doomedBurden;

	public SimpleButton blindedBurden;

	public SimpleButton huntedBurden;

	public SimpleButton pursuedBurden;

	public OpHoldButton unlocksButton;

	public MenuTabWrapper menuTabWrapper;

	public OpHoldButton startButton;

	public UnlocksIndicator unlocksIndicator;

	public FSprite levelSprite;

	public FSprite levelSprite2;

	public FSprite levelContainer;

	public MenuLabel currentLevelLabel;

	public MenuLabel nextLevelLabel;

	public MenuLabel levelOverloadLabel;

	public float progressFade;

	public bool progressInvert;

	public int lastEstimate;

	public ChallengeSelectPage(Menu menu, MenuObject owner, Vector2 pos)
		: base(menu, owner, pos)
	{
		leftAnchor = (menu as ExpeditionMenu).leftAnchor;
		rightAnchor = (menu as ExpeditionMenu).rightAnchor;
		pageTitle = new FSprite("challengeselect");
		pageTitle.SetAnchor(0.5f, 0f);
		pageTitle.x = 683f;
		pageTitle.y = 680f;
		pageTitle.shader = menu.manager.rainWorld.Shaders["MenuText"];
		Container.AddChild(pageTitle);
		missionGradient = new FSprite("LinearGradient200");
		missionGradient.SetAnchor(0.5f, 0f);
		missionGradient.x = 683f;
		missionGradient.y = 0f;
		missionGradient.scaleX = 680f;
		missionGradient.scaleY = 3.6f;
		missionGradient.alpha = 0f;
		Container.AddChild(missionGradient);
		sideSpriteLeft = new FSprite("LinearGradient200");
		sideSpriteLeft.SetAnchor(0.5f, 0f);
		sideSpriteLeft.x = 343f;
		sideSpriteLeft.y = 0f;
		sideSpriteLeft.scaleX = 3f;
		sideSpriteLeft.scaleY = 3.6f;
		sideSpriteLeft.alpha = 0f;
		Container.AddChild(sideSpriteLeft);
		sideSpriteRight = new FSprite("LinearGradient200");
		sideSpriteRight.SetAnchor(0.5f, 0f);
		sideSpriteRight.x = 1020f;
		sideSpriteRight.y = 0f;
		sideSpriteRight.scaleX = 3f;
		sideSpriteRight.scaleY = 3.6f;
		sideSpriteRight.alpha = 0f;
		Container.AddChild(sideSpriteRight);
		slugcatSprite = new FSprite("Kill_Slugcat");
		slugcatSprite.x = 683f;
		slugcatSprite.y = 640f;
		slugcatSprite.alpha = 0f;
		Container.AddChild(slugcatSprite);
		if (menu.manager.rainWorld.options.language != InGameTranslator.LanguageID.English)
		{
			localizedSubtitle = new MenuLabel(menu, this, menu.Translate("-CHALLENGES-"), new Vector2(683f, 740f), default(Vector2), bigText: false);
			localizedSubtitle.label.color = new Color(0.5f, 0.5f, 0.5f);
			subObjects.Add(localizedSubtitle);
		}
		MenuLabel item = new MenuLabel(menu, this, menu.Translate("Slugcat Select"), new Vector2(443f, 710f), default(Vector2), bigText: false)
		{
			label = 
			{
				alignment = FLabelAlignment.Right,
				color = new Color(0.7f, 0.7f, 0.7f)
			}
		};
		subObjects.Add(item);
		leftPage = new SymbolButton(menu, this, "Big_Menu_Arrow", "LEFT", new Vector2(453f, 685f));
		leftPage.symbolSprite.rotation = 270f;
		leftPage.size = new Vector2(45f, 45f);
		leftPage.roundedRect.size = leftPage.size;
		subObjects.Add(leftPage);
		MenuLabel item2 = new MenuLabel(menu, this, menu.Translate("Progression"), new Vector2(933f, 710f), default(Vector2), bigText: false)
		{
			label = 
			{
				alignment = FLabelAlignment.Left,
				color = new Color(0.7f, 0.7f, 0.7f)
			}
		};
		subObjects.Add(item2);
		rightPage = new SymbolButton(menu, this, "Big_Menu_Arrow", "RIGHT", new Vector2(873f, 685f));
		rightPage.symbolSprite.rotation = 90f;
		rightPage.size = new Vector2(45f, 45f);
		rightPage.roundedRect.size = rightPage.size;
		subObjects.Add(rightPage);
		difficultyLabel = new MenuLabel(menu, this, menu.Translate("CHALLENGE DIFFICULTY"), new Vector2(680f, 645f), default(Vector2), bigText: false);
		difficultyLabel.label.color = new Color(0.8f, 0.8f, 0.8f);
		subObjects.Add(difficultyLabel);
		difficultySlider = new HorizontalSlider(menu, this, "", new Vector2(470f, 600f), new Vector2(400f, 0f), ExpeditionEnums.SliderID.ChallengeDifficulty, subtleSlider: false);
		subObjects.Add(difficultySlider);
		easySprite = new FSprite("SaintA");
		easySprite.color = new Color(0.2f, 0.75f, 0.2f);
		Container.AddChild(easySprite);
		easyLabel = new MenuLabel(menu, this, menu.Translate("EASY"), new Vector2(430f, 595f), default(Vector2), bigText: false);
		subObjects.Add(easyLabel);
		hardSprite = new FSprite("OutlawA");
		hardSprite.color = new Color(0.75f, 0.2f, 0.2f);
		Container.AddChild(hardSprite);
		hardLabel = new MenuLabel(menu, this, menu.Translate("HARD"), new Vector2(930f, 595f), default(Vector2), bigText: false);
		subObjects.Add(hardLabel);
		challengeButtons = new BigSimpleButton[5];
		hiddenToggles = new SymbolButton[5];
		for (int i = 0; i < challengeButtons.Length; i++)
		{
			float num = 50f * (float)i;
			challengeButtons[i] = new BigSimpleButton(menu, this, "Challenge " + (i + 1), "CHA" + (i + 1), new Vector2(360f, 510f - num), new Vector2(600f, 40f), FLabelAlignment.Left, bigText: true);
			subObjects.Add(challengeButtons[i]);
			if (i < 3 && ExpeditionData.challengeList.Count < 1)
			{
				ChallengeOrganizer.AssignChallenge(i, hidden: false);
			}
			hiddenToggles[i] = new SymbolButton(menu, this, "hiddenopen", "HIDDEN" + (i + 1), new Vector2(970f, 510f - num));
			hiddenToggles[i].size = new Vector2(40f, 40f);
			hiddenToggles[i].roundedRect.size = hiddenToggles[i].size;
			subObjects.Add(hiddenToggles[i]);
		}
		filterButton = new SymbolButton(menu, this, "filter", "FILTER", new Vector2(380f, 250f));
		filterButton.size = new Vector2(40f, 40f);
		filterButton.roundedRect.size = filterButton.size;
		subObjects.Add(filterButton);
		randomButton = new SymbolButton(menu, this, "Sandbox_Randomize", "RANDOM", new Vector2(430f, 250f));
		randomButton.size = new Vector2(40f, 40f);
		randomButton.roundedRect.size = randomButton.size;
		subObjects.Add(randomButton);
		minusButton = new SymbolButton(menu, this, "minus", "MINUS", new Vector2(900f, 250f));
		minusButton.size = new Vector2(40f, 40f);
		minusButton.roundedRect.size = minusButton.size;
		subObjects.Add(minusButton);
		plusButton = new SymbolButton(menu, this, "plus", "PLUS", new Vector2(950f, 250f));
		plusButton.size = new Vector2(40f, 40f);
		plusButton.roundedRect.size = plusButton.size;
		subObjects.Add(plusButton);
		pointsLabel = new MenuLabel(menu, this, menu.Translate("POINTS: "), new Vector2(680f, 270f), default(Vector2), bigText: true);
		subObjects.Add(pointsLabel);
		missionLabel = new MenuLabel(menu, this, menu.Translate("MISSION SELECTED"), new Vector2(683f, 600f), default(Vector2), bigText: true);
		missionLabel.label.shader = menu.manager.rainWorld.Shaders["MenuTextCustom"];
		missionLabel.label.color = new HSLColor(0.6f, 1f, 0.6f).rgb;
		subObjects.Add(missionLabel);
		menuTabWrapper = new MenuTabWrapper(menu, this);
		subObjects.Add(menuTabWrapper);
		float num2 = 150f;
		float num3 = 15f;
		string[] array = Regex.Split(menu.Translate("CONFIGURE<LINE>PERKS & BURDENS"), "<LINE>");
		string[] array2 = Regex.Split(menu.Translate("START NEW<LINE>EXPEDITION"), "<LINE>");
		List<string> list = new List<string>();
		for (int j = 0; j < array.Length; j++)
		{
			if (array[j] != null && array[j].Length > 0)
			{
				list.Add(array[j]);
			}
		}
		for (int k = 0; k < array2.Length; k++)
		{
			if (array2[k] != null && array2[k].Length > 0)
			{
				list.Add(array2[k]);
			}
		}
		for (int l = 0; l < list.Count; l++)
		{
			float width = LabelTest.GetWidth(list[l]);
			if (num2 < width)
			{
				num2 = width;
			}
		}
		num2 += num3;
		unlocksButton = new OpHoldButton(new Vector2(683f - (num2 + 30f), 40f), new Vector2(num2, 50f), menu.Translate("CONFIGURE<LINE>PERKS & BURDENS").Replace("<LINE>", "\r\n"), 20f);
		unlocksButton.OnPressDone += UnlocksButton_OnPressDone;
		unlocksButton.description = " ";
		unlockWrapper = new UIelementWrapper(menuTabWrapper, unlocksButton);
		startButton = new OpHoldButton(new Vector2(713f, 40f), new Vector2(num2, 50f), menu.Translate("START NEW<LINE>EXPEDITION").Replace("<LINE>", "\r\n"), 100f);
		startButton.OnPressDone += StartButton_OnPressDone;
		startButton.description = " ";
		startWrapper = new UIelementWrapper(menuTabWrapper, startButton);
		if (ExpeditionData.ints.Sum() >= 8)
		{
			symbolButton1 = new SymbolButton(menu, this, "GuidanceSlugcat", "BUTTON", new Vector2(663f, 45f));
			symbolButton1.roundedRect.size = new Vector2(40f, 40f);
			symbolButton1.size = symbolButton1.roundedRect.size;
			subObjects.Add(symbolButton1);
		}
		else if (ModManager.MSC)
		{
			indicators = new FSprite[8];
			for (int m = 0; m < indicators.Length; m++)
			{
				indicators[m] = new FSprite("Futile_White");
				indicators[m].shader = menu.manager.rainWorld.Shaders["VectorCircle"];
				indicators[m].scale = 0.4f;
				indicators[m].color = ExpeditionGame.ExIndexToColor(m);
				Container.AddChild(indicators[m]);
			}
		}
		levelSprite = new FSprite("pixel");
		levelSprite.color = new Color(0.9f, 0.9f, 0.9f);
		levelSprite.SetAnchor(0f, 0f);
		levelSprite.scaleX = 220f;
		levelSprite.scaleY = 35f;
		Container.AddChild(levelSprite);
		levelSprite2 = new FSprite("pixel");
		levelSprite2.color = new Color(0.6f, 1f, 0.6f);
		levelSprite2.SetAnchor(0f, 0f);
		levelSprite2.scaleX = 120f;
		levelSprite2.scaleY = 35f;
		Container.AddChild(levelSprite2);
		levelContainer = new FSprite("levelcontainer");
		levelContainer.color = new Color(0.55f, 0.55f, 0.55f);
		levelContainer.SetAnchor(0f, 0f);
		Container.AddChild(levelContainer);
		currentLevelLabel = new MenuLabel(menu, this, "LEVEL 1", new Vector2(405f, 160f), default(Vector2), bigText: false);
		currentLevelLabel.label.color = new Color(0.6f, 0.6f, 0.6f);
		subObjects.Add(currentLevelLabel);
		nextLevelLabel = new MenuLabel(menu, this, "LEVEL 2", new Vector2(965f, 160f), default(Vector2), bigText: false);
		nextLevelLabel.label.color = new Color(0.6f, 0.6f, 0.6f);
		subObjects.Add(nextLevelLabel);
		levelOverloadLabel = new MenuLabel(menu, this, "", new Vector2(1000f, 200f), default(Vector2), bigText: true);
		levelOverloadLabel.label.alignment = FLabelAlignment.Left;
		subObjects.Add(levelOverloadLabel);
		UpdateChallengeButtons();
	}

	public void SetUpSelectables()
	{
		leftPage.nextSelectable[3] = difficultySlider;
		leftPage.nextSelectable[1] = unlockWrapper;
		rightPage.nextSelectable[3] = difficultySlider;
		rightPage.nextSelectable[1] = startWrapper;
		if (difficultySlider.floatValue >= 0.5f)
		{
			difficultySlider.nextSelectable[1] = rightPage;
		}
		else
		{
			difficultySlider.nextSelectable[1] = leftPage;
		}
		difficultySlider.nextSelectable[3] = challengeButtons[0];
		challengeButtons[0].nextSelectable[1] = difficultySlider;
		int num = 0;
		for (int i = 0; i < challengeButtons.Length; i++)
		{
			challengeButtons[i].nextSelectable[0] = hiddenToggles[i];
			challengeButtons[i].nextSelectable[2] = hiddenToggles[i];
			hiddenToggles[i].nextSelectable[2] = challengeButtons[i];
			hiddenToggles[i].nextSelectable[0] = challengeButtons[i];
			hiddenToggles[i].nextSelectable[3] = ((hiddenToggles.Count() > i + 1) ? hiddenToggles[i + 1] : plusButton);
			challengeButtons[i].nextSelectable[3] = challengeButtons[(i + 1) % challengeButtons.Length];
			if (ExpeditionData.activeMission != "")
			{
				challengeButtons[i].nextSelectable[0] = challengeButtons[i];
				challengeButtons[i].nextSelectable[2] = challengeButtons[i];
			}
			if (!challengeButtons[i].buttonBehav.greyedOut)
			{
				num = i;
			}
		}
		challengeButtons[num].nextSelectable[3] = minusButton;
		hiddenToggles[num].nextSelectable[3] = plusButton;
		minusButton.nextSelectable[1] = challengeButtons[num];
		minusButton.nextSelectable[3] = startWrapper;
		plusButton.nextSelectable[1] = challengeButtons[num];
		plusButton.nextSelectable[3] = startWrapper;
		filterButton.nextSelectable[1] = challengeButtons[num];
		filterButton.nextSelectable[3] = unlockWrapper;
		randomButton.nextSelectable[1] = challengeButtons[num];
		randomButton.nextSelectable[3] = unlockWrapper;
		randomButton.nextSelectable[2] = minusButton;
		minusButton.nextSelectable[0] = randomButton;
		plusButton.nextSelectable[2] = filterButton;
		filterButton.nextSelectable[0] = plusButton;
		unlockWrapper.nextSelectable[0] = startWrapper;
		startWrapper.nextSelectable[2] = unlockWrapper;
		unlockWrapper.nextSelectable[1] = randomButton;
		startWrapper.nextSelectable[1] = minusButton;
		unlockWrapper.nextSelectable[3] = leftPage;
		startWrapper.nextSelectable[3] = rightPage;
		(menu as ExpeditionMenu).muteButton.nextSelectable[3] = challengeButtons[0];
		(menu as ExpeditionMenu).muteButton.nextSelectable[2] = backButton;
		(menu as ExpeditionMenu).muteButton.nextSelectable[1] = unlockWrapper;
		(menu as ExpeditionMenu).muteButton.nextSelectable[0] = (menu as ExpeditionMenu).exitButton;
		if (ExpeditionData.activeMission != "")
		{
			leftPage.nextSelectable[3] = challengeButtons[0];
			rightPage.nextSelectable[3] = challengeButtons[0];
			unlockWrapper.nextSelectable[1] = deselectMissionButton;
			startWrapper.nextSelectable[1] = deselectMissionButton;
			challengeButtons[num].nextSelectable[3] = deselectMissionButton;
			hiddenToggles[num].nextSelectable[3] = deselectMissionButton;
			challengeButtons[0].nextSelectable[1] = rightPage;
			deselectMissionButton.nextSelectable[3] = unlockWrapper;
			deselectMissionButton.nextSelectable[0] = deselectMissionButton;
			deselectMissionButton.nextSelectable[2] = deselectMissionButton;
			deselectMissionButton.nextSelectable[1] = challengeButtons[num];
		}
	}

	private void UnlocksButton_OnPressDone(UIfocusable trigger)
	{
		UnlockDialog unlockDialog = new UnlockDialog(menu.manager, this);
		unlocksButton.Reset();
		unlocksButton.greyedOut = true;
		startButton.greyedOut = true;
		menu.manager.ShowDialog(unlockDialog);
		unlockDialog.pages[0].Container.AddChild(levelSprite);
		unlockDialog.pages[0].Container.AddChild(levelSprite2);
		unlockDialog.pages[0].Container.AddChild(levelContainer);
		unlockDialog.pages[0].Container.AddChild(currentLevelLabel.label);
		unlockDialog.pages[0].Container.AddChild(nextLevelLabel.label);
		unlockDialog.pages[0].Container.AddChild(levelOverloadLabel.label);
		unlockDialog.pages[0].Container.AddChild(pointsLabel.label);
	}

	private void UserInput_OnControllerConfigurationChanged()
	{
		UserInput.OnControllerConfigurationChanged -= UserInput_OnControllerConfigurationChanged;
		requestingControllerConnections = false;
		if (pendingStart)
		{
			StartGame();
		}
	}

	private void StartButton_OnPressDone(UIfocusable trigger)
	{
		menu.manager.rainWorld.options.ResetJollyProfileRequest();
		pressedStartButton = true;
		StartGame();
	}

	private void StartGame()
	{
		if (ModManager.CoopAvailable)
		{
			Custom.Log("JollyCoop Player Count is:", menu.manager.rainWorld.options.JollyPlayerCount.ToString());
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
		menu.manager.rainWorld.progression.WipeSaveState(ExpeditionData.slugcatPlayer);
		if (ExpeditionData.activeMission == "")
		{
			ExpeditionData.startingDen = ExpeditionGame.ExpeditionRandomStarts(menu.manager.rainWorld, ExpeditionData.slugcatPlayer);
		}
		else if (ExpeditionProgression.missionList.Find((ExpeditionProgression.Mission x) => x.key == ExpeditionData.activeMission).den != "")
		{
			ExpeditionData.startingDen = ExpeditionProgression.missionList.Find((ExpeditionProgression.Mission x) => x.key == ExpeditionData.activeMission).den;
		}
		ExpeditionGame.PrepareExpedition();
		ExpeditionData.AddExpeditionRequirements(ExpeditionData.slugcatPlayer, ended: false);
		global::Expedition.Expedition.coreFile.Save(runEnded: false);
		menu.manager.menuSetup.startGameCondition = ProcessManager.MenuSetup.StoryGameInitCondition.New;
		menu.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Game);
		menu.PlaySound(SoundID.MENU_Start_New_Game);
	}

	public override void Update()
	{
		base.Update();
		if (ExpeditionData.devMode && Input.GetKey(KeyCode.Slash))
		{
			ExpeditionGame.ExpeditionRandomStarts(menu.manager.rainWorld, ExpeditionData.slugcatPlayer);
		}
		if (currentSlugcat != ExpeditionData.slugcatPlayer && global::Expedition.Expedition.coreFile.coreLoaded)
		{
			currentSlugcat = ExpeditionData.slugcatPlayer;
		}
		if (menu.currentPage != 2)
		{
			updatePending = true;
		}
		else if (updatePending && global::Expedition.Expedition.coreFile.coreLoaded)
		{
			UpdateChallengeButtons();
			updatePending = false;
		}
		if (menu.manager.rainWorld.options.IsJollyProfileRequesting())
		{
			if (pressedStartButton)
			{
				StartGame();
			}
		}
		else
		{
			pressedStartButton = false;
		}
		float num = ((levelOverloadLabel.text != "") ? 0.1f : 0.03f);
		progressFade += num;
		colorCounter += 1f;
		if (ExpeditionData.activeMission != "")
		{
			if (unlocksIndicator == null)
			{
				unlocksIndicator = new UnlocksIndicator(menu, this, new Vector2(683f, 230f), centered: true);
				unlocksIndicator.color = missionColor.rgb;
				subObjects.Add(unlocksIndicator);
			}
			else
			{
				unlocksIndicator.color = missionColor.rgb;
			}
			missionGradient.color = missionColor.rgb;
			sideSpriteLeft.color = missionColor.rgb;
			sideSpriteRight.color = missionColor.rgb;
			pointsLabel.text = "";
			missionLabel.text = missionName;
			missionLabel.label.color = missionColor.rgb;
			slugcatSprite.color = missionColor.rgb;
			startButton.text = menu.Translate("COMMENCE<LINE>MISSION").Replace("<LINE>", "\r\n");
			unlocksButton.colorEdge = missionColor.rgb;
			startButton.colorEdge = missionColor.rgb;
			plusButton.buttonBehav.greyedOut = true;
			minusButton.buttonBehav.greyedOut = true;
			filterButton.buttonBehav.greyedOut = true;
			randomButton.buttonBehav.greyedOut = true;
			difficultySlider.buttonBehav.greyedOut = true;
			difficultySlider.pos.y = 1000f;
			for (int i = 0; i < challengeButtons.Length; i++)
			{
				if (i <= ExpeditionData.challengeList.Count)
				{
					challengeButtons[i].rectColor = missionColor;
				}
			}
			for (int j = 0; j < hiddenToggles.Length; j++)
			{
				hiddenToggles[j].buttonBehav.greyedOut = true;
			}
		}
		else
		{
			for (int k = 0; k < challengeButtons.Length; k++)
			{
				if (k <= ExpeditionData.challengeList.Count)
				{
					challengeButtons[k].rectColor = new HSLColor(0f, 0f, 0.7f);
				}
			}
			if (deselectMissionButton != null)
			{
				deselectMissionButton.RemoveSprites();
				deselectMissionButton.RemoveSubObject(deselectMissionButton);
				deselectMissionButton = null;
			}
			if (unlocksIndicator != null)
			{
				unlocksIndicator.RemoveSprites();
				RemoveSubObject(unlocksIndicator);
				unlocksIndicator = null;
			}
			difficultySlider.pos.y = 600f;
			difficultySlider.buttonBehav.greyedOut = false;
			missionLabel.text = "";
		}
		if (unlocksButton.Focused)
		{
			unlocksButton.colorEdge = new Color(0.2f, 0.85f, 0.85f);
		}
		else if (ExpeditionData.activeMission == "")
		{
			unlocksButton.colorEdge = new Color(0.45f, 0.45f, 0.45f);
		}
		if (startButton.Focused)
		{
			startButton.colorEdge = new Color(0.2f, 0.85f, 0.2f);
		}
		else if (ExpeditionData.activeMission == "")
		{
			startButton.colorEdge = new Color(0.45f, 0.45f, 0.45f);
		}
		for (int l = 0; l < challengeButtons.Length; l++)
		{
			if (challengeButtons[l].MouseOver || challengeButtons[l].Selected)
			{
				menu.infoLabel.text = ((ExpeditionData.challengeList.Count >= l + 1) ? ExpeditionData.challengeList[l].description : "");
				menu.infoLabelFade = 1f;
			}
		}
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		pageTitle.x = Mathf.Lerp(owner.page.lastPos.x, owner.page.pos.x, timeStacker) + 680f;
		pageTitle.y = Mathf.Lerp(owner.page.lastPos.y, owner.page.pos.y, timeStacker) + 680f;
		missionGradient.x = Mathf.Lerp(owner.page.lastPos.x, owner.page.pos.x, timeStacker) + 683f;
		missionGradient.y = Mathf.Lerp(owner.page.lastPos.y, owner.page.pos.y, timeStacker) - 5f;
		sideSpriteLeft.x = Mathf.Lerp(owner.page.lastPos.x, owner.page.pos.x, timeStacker) + 683f - 340f;
		sideSpriteLeft.y = Mathf.Lerp(owner.page.lastPos.y, owner.page.pos.y, timeStacker) - 5f;
		sideSpriteRight.x = Mathf.Lerp(owner.page.lastPos.x, owner.page.pos.x, timeStacker) + 683f + 340f;
		sideSpriteRight.y = Mathf.Lerp(owner.page.lastPos.y, owner.page.pos.y, timeStacker) - 5f;
		slugcatSprite.x = Mathf.Lerp(owner.page.lastPos.x, owner.page.pos.x, timeStacker) + 683f;
		slugcatSprite.y = Mathf.Lerp(owner.page.lastPos.y, owner.page.pos.y, timeStacker) + 640f;
		easySprite.x = Mathf.Lerp(owner.page.lastPos.x, owner.page.pos.x, timeStacker) + 680f - 250f;
		easySprite.y = Mathf.Lerp(owner.page.lastPos.y, owner.page.pos.y, timeStacker) + 620f;
		hardSprite.x = Mathf.Lerp(owner.page.lastPos.x, owner.page.pos.x, timeStacker) + 680f + 250f;
		hardSprite.y = Mathf.Lerp(owner.page.lastPos.y, owner.page.pos.y, timeStacker) + 620f;
		if (indicators != null)
		{
			for (int i = 0; i < indicators.Length; i++)
			{
				indicators[i].alpha = ((ExpeditionData.ints[i] >= 1) ? 1f : 0f);
			}
			indicators[0].x = Mathf.Lerp(owner.page.lastPos.x, owner.page.pos.x, timeStacker) + 683f;
			indicators[0].y = Mathf.Lerp(owner.page.lastPos.y, owner.page.pos.y, timeStacker) + 65f;
			indicators[1].x = Mathf.Lerp(owner.page.lastPos.x, owner.page.pos.x, timeStacker) + 683f - 10f;
			indicators[1].y = Mathf.Lerp(owner.page.lastPos.y, owner.page.pos.y, timeStacker) + 65f;
			indicators[2].x = Mathf.Lerp(owner.page.lastPos.x, owner.page.pos.x, timeStacker) + 683f + 10f;
			indicators[2].y = Mathf.Lerp(owner.page.lastPos.y, owner.page.pos.y, timeStacker) + 65f;
			indicators[3].x = Mathf.Lerp(owner.page.lastPos.x, owner.page.pos.x, timeStacker) + 683f - 20f;
			indicators[3].y = Mathf.Lerp(owner.page.lastPos.y, owner.page.pos.y, timeStacker) + 45f;
			indicators[4].x = Mathf.Lerp(owner.page.lastPos.x, owner.page.pos.x, timeStacker) + 683f - 10f;
			indicators[4].y = Mathf.Lerp(owner.page.lastPos.y, owner.page.pos.y, timeStacker) + 45f;
			indicators[5].x = Mathf.Lerp(owner.page.lastPos.x, owner.page.pos.x, timeStacker) + 683f;
			indicators[5].y = Mathf.Lerp(owner.page.lastPos.y, owner.page.pos.y, timeStacker) + 45f;
			indicators[6].x = Mathf.Lerp(owner.page.lastPos.x, owner.page.pos.x, timeStacker) + 683f + 10f;
			indicators[6].y = Mathf.Lerp(owner.page.lastPos.y, owner.page.pos.y, timeStacker) + 45f;
			indicators[7].x = Mathf.Lerp(owner.page.lastPos.x, owner.page.pos.x, timeStacker) + 683f + 20f;
			indicators[7].y = Mathf.Lerp(owner.page.lastPos.y, owner.page.pos.y, timeStacker) + 45f;
		}
		levelSprite.x = Mathf.Lerp(owner.page.lastPos.x, owner.page.pos.x, timeStacker) + 383f;
		levelSprite.y = Mathf.Lerp(owner.page.lastPos.y, owner.page.pos.y, timeStacker) + 183f;
		levelSprite2.x = levelSprite.x + levelSprite.scaleX;
		levelSprite2.y = levelSprite.y;
		levelContainer.x = Mathf.Lerp(owner.page.lastPos.x, owner.page.pos.x, timeStacker) + 380f;
		levelContainer.y = Mathf.Lerp(owner.page.lastPos.y, owner.page.pos.y, timeStacker) + 180f;
		if (symbolButton1 != null)
		{
			int num = ExpeditionGame.ExIndex(ExpeditionData.slugcatPlayer);
			if (num > -1)
			{
				symbolButton1.symbolSprite.color = ((ExpeditionData.ints[num] == 2) ? new HSLColor(Mathf.Sin(colorCounter / 20f), 1f, 0.75f).rgb : new Color(0.3f, 0.3f, 0.3f));
			}
		}
		if (ExpeditionData.activeMission != "")
		{
			if (pageTitle.element.name != "mission")
			{
				pageTitle.SetElementByName("mission");
			}
			if (sideSpriteLeft.shader != menu.manager.rainWorld.Shaders["MenuTextCustom"])
			{
				sideSpriteLeft.shader = menu.manager.rainWorld.Shaders["MenuTextCustom"];
			}
			if (sideSpriteRight.shader != menu.manager.rainWorld.Shaders["MenuTextCustom"])
			{
				sideSpriteRight.shader = menu.manager.rainWorld.Shaders["MenuTextCustom"];
			}
			if (missionTime != null)
			{
				missionTime.label.alpha = 1f;
			}
			missionGradient.alpha = 0.15f;
			sideSpriteLeft.alpha = 0.8f;
			sideSpriteRight.alpha = 0.8f;
			slugcatSprite.alpha = 1f;
			levelContainer.alpha = 0f;
			levelSprite.alpha = 0f;
			levelSprite2.alpha = 0f;
			currentLevelLabel.label.alpha = 0f;
			nextLevelLabel.label.alpha = 0f;
			levelOverloadLabel.label.alpha = 0f;
			pointsLabel.label.alpha = 0f;
			difficultyLabel.label.alpha = 0f;
			easyLabel.label.alpha = 0f;
			hardLabel.label.alpha = 0f;
			easySprite.alpha = 0f;
			hardSprite.alpha = 0f;
			for (int j = 0; j < difficultySlider.lineSprites.Length; j++)
			{
				difficultySlider.lineSprites[j].alpha = 0f;
			}
		}
		else
		{
			if (pageTitle.element.name != "challengeselect")
			{
				pageTitle.SetElementByName("challengeselect");
			}
			if (sideSpriteLeft.shader != menu.manager.rainWorld.Shaders["Basic"])
			{
				sideSpriteLeft.shader = menu.manager.rainWorld.Shaders["Basic"];
			}
			if (sideSpriteRight.shader != menu.manager.rainWorld.Shaders["Basic"])
			{
				sideSpriteRight.shader = menu.manager.rainWorld.Shaders["Basic"];
			}
			if (missionTime != null)
			{
				missionTime.label.alpha = 0f;
			}
			missionGradient.alpha = 0f;
			sideSpriteLeft.alpha = 0f;
			sideSpriteRight.alpha = 0f;
			slugcatSprite.alpha = 0f;
			levelContainer.alpha = 1f;
			levelSprite.alpha = 1f;
			pointsLabel.label.alpha = 1f;
			currentLevelLabel.label.alpha = 1f;
			nextLevelLabel.label.alpha = 1f;
			levelOverloadLabel.label.alpha = 1f;
			levelSprite2.alpha = Mathf.Lerp(0.2f, 1f, Mathf.PingPong(progressFade, 1f));
			difficultyLabel.label.alpha = 1f;
			easyLabel.label.alpha = 1f;
			hardLabel.label.alpha = 1f;
			easySprite.alpha = 1f;
			hardSprite.alpha = 1f;
			for (int k = 0; k < difficultySlider.lineSprites.Length; k++)
			{
				difficultySlider.lineSprites[k].alpha = 1f;
			}
		}
	}

	public override void RemoveSprites()
	{
		base.RemoveSprites();
		pageTitle.RemoveFromContainer();
		missionGradient.RemoveFromContainer();
		sideSpriteLeft.RemoveFromContainer();
		sideSpriteRight.RemoveFromContainer();
		easySprite.RemoveFromContainer();
		hardSprite.RemoveFromContainer();
		levelSprite.RemoveFromContainer();
		levelContainer.RemoveFromContainer();
		slugcatSprite.RemoveFromContainer();
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
		if (message == "POINTS")
		{
			ExpeditionData.currentPoints += lastEstimate;
			ExpeditionProgression.CheckLevelUp();
			UpdateLevelSprite();
		}
		if (message == "RIGHT")
		{
			(menu as ExpeditionMenu).UpdatePage(3);
			(menu as ExpeditionMenu).MovePage(new Vector2(-1500f, 0f));
		}
		if (message == "LEFT")
		{
			if (ExpeditionData.activeMission != "")
			{
				Singal(null, "DESELECT");
			}
			(menu as ExpeditionMenu).UpdatePage(1);
			(menu as ExpeditionMenu).MovePage(new Vector2(1500f, 0f));
		}
		if (message.StartsWith("CHA"))
		{
			if (ExpeditionData.activeMission != "")
			{
				menu.PlaySound(SoundID.MENU_Error_Ping);
				return;
			}
			int.TryParse(message.Remove(0, 3), NumberStyles.Any, CultureInfo.InvariantCulture, out var result);
			bool hidden = ExpeditionData.challengeList[result - 1].hidden;
			Challenge challenge = ExpeditionData.challengeList[result - 1];
			ChallengeOrganizer.AssignChallenge(result - 1, hidden);
			if (ExpeditionData.challengeList[result - 1] == challenge)
			{
				menu.PlaySound(SoundID.MENU_Error_Ping);
				return;
			}
			UpdateChallengeButtons();
			menu.PlaySound(SoundID.MENU_Button_Successfully_Assigned);
		}
		if (message == "DESELECT")
		{
			ExpeditionData.activeMission = "";
			plusButton.buttonBehav.greyedOut = false;
			minusButton.buttonBehav.greyedOut = false;
			filterButton.buttonBehav.greyedOut = false;
			randomButton.buttonBehav.greyedOut = false;
			startButton.text = menu.Translate("START NEW<LINE>EXPEDITION").Replace("<LINE>", "\r\n");
			ExpeditionData.allChallengeLists[ExpeditionData.slugcatPlayer] = new List<Challenge>();
			for (int i = 0; i < 3; i++)
			{
				ChallengeOrganizer.AssignChallenge(i, hidden: false);
			}
			UpdateChallengeButtons();
			if (menu.currentPage == base.page.index)
			{
				menu.PlaySound(SoundID.Slugcat_Ghost_Dissappear);
			}
			menu.selectedObject = challengeButtons[0];
		}
		if (message == "RANDOM")
		{
			for (int j = 0; j < ExpeditionData.challengeList.Count; j++)
			{
				bool hidden2 = ExpeditionData.challengeList[j].hidden;
				ChallengeOrganizer.AssignChallenge(j, hidden2);
			}
			UpdateChallengeButtons();
			menu.PlaySound(SoundID.MENU_Player_Join_Game);
		}
		if (message == "MINUS")
		{
			int num = 0;
			for (int k = 0; k < ExpeditionData.challengeList.Count - 1; k++)
			{
				if (ExpeditionData.challengeList[k].hidden)
				{
					num++;
				}
			}
			if (ExpeditionData.challengeList.Count == 1 || num >= ExpeditionData.challengeList.Count - 1)
			{
				menu.PlaySound(SoundID.MENU_Error_Ping);
				return;
			}
			ExpeditionData.challengeList.RemoveAt(ExpeditionData.challengeList.Count - 1);
			UpdateChallengeButtons();
			menu.PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
		}
		if (message == "PLUS")
		{
			if (ExpeditionData.challengeList.Count + 1 == 6)
			{
				menu.PlaySound(SoundID.MENU_Error_Ping);
				return;
			}
			ChallengeOrganizer.AssignChallenge(ExpeditionData.challengeList.Count, hidden: false);
			UpdateChallengeButtons();
			menu.PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
		}
		if (message.StartsWith("HIDDEN"))
		{
			int.TryParse(message.Remove(0, 6), NumberStyles.Any, CultureInfo.InvariantCulture, out var result2);
			int num2 = 0;
			for (int l = 0; l < ExpeditionData.challengeList.Count; l++)
			{
				if (ExpeditionData.challengeList[l].hidden)
				{
					num2++;
				}
			}
			if (ExpeditionData.challengeList[result2 - 1].hidden)
			{
				ExpeditionData.challengeList[result2 - 1].hidden = false;
				menu.PlaySound(SoundID.Slugcat_Ghost_Dissappear);
			}
			else
			{
				if (num2 + 1 >= ExpeditionData.challengeList.Count || result2 - 1 >= ExpeditionData.challengeList.Count)
				{
					menu.PlaySound(SoundID.MENU_Error_Ping);
					return;
				}
				ChallengeOrganizer.AssignChallenge(result2 - 1, hidden: true);
				menu.PlaySound(SoundID.Slugcat_Ghost_Appear);
			}
			UpdateChallengeButtons();
		}
		if (message == "FILTER")
		{
			FilterDialog dialog = new FilterDialog(menu.manager, this);
			unlocksButton.Reset();
			unlocksButton.greyedOut = true;
			startButton.greyedOut = true;
			menu.manager.ShowDialog(dialog);
		}
		if (!(message == "BUTTON"))
		{
			return;
		}
		menu.PlaySound(SoundID.MENU_Player_Join_Game);
		if (ExpeditionGame.ExIndex(ExpeditionData.slugcatPlayer) > -1)
		{
			if (ExpeditionData.ints[ExpeditionGame.ExIndex(ExpeditionData.slugcatPlayer)] == 1)
			{
				ExpeditionData.ints[ExpeditionGame.ExIndex(ExpeditionData.slugcatPlayer)] = 2;
			}
			else
			{
				ExpeditionData.ints[ExpeditionGame.ExIndex(ExpeditionData.slugcatPlayer)] = 1;
			}
		}
	}

	public void UpdateLevelSprite()
	{
		int num = ExpeditionProgression.LevelCap(ExpeditionData.level);
		int currentPoints = ExpeditionData.currentPoints;
		int num2 = (lastEstimate = ExpeditionGame.CalculateScore(predict: true));
		levelSprite.scaleX = Mathf.Lerp(0f, 600f, Mathf.InverseLerp(0f, num, currentPoints));
		levelSprite2.color = new HSLColor(0f, 0f, Mathf.Lerp(0.3f, 0.9f, Mathf.InverseLerp(0f, 600f, levelSprite.scaleX))).rgb;
		levelSprite2.x = levelSprite.x + levelSprite.scaleX;
		float b = 605f - Mathf.Abs(levelSprite.x - levelSprite2.x);
		levelSprite2.scaleX = Mathf.Lerp(0f, b, Mathf.InverseLerp(currentPoints, num, currentPoints + num2));
		int level = ExpeditionData.level;
		int value = ExpeditionData.level + 1;
		currentLevelLabel.text = menu.Translate("LEVEL <current>").Replace("<current>", ValueConverter.ConvertToString(level));
		nextLevelLabel.text = menu.Translate("LEVEL <next>").Replace("<next>", ValueConverter.ConvertToString(value));
		int num3 = ExpeditionProgression.CalculateOverload(currentPoints + num2);
		levelOverloadLabel.text = ((num3 > 0) ? ("+" + num3) : "");
	}

	public void UpdateChallengeButtons()
	{
		if (challengeButtons != null && ExpeditionData.challengeList != null)
		{
			for (int i = 0; i < challengeButtons.Length; i++)
			{
				if (i < ExpeditionData.challengeList.Count)
				{
					hiddenToggles[i].buttonBehav.greyedOut = false;
					challengeButtons[i].buttonBehav.greyedOut = false;
					if (ExpeditionData.challengeList[i] != null)
					{
						if (ExpeditionData.challengeList[i].hidden)
						{
							challengeButtons[i].labelColor = new HSLColor(0.12f, 0.8f, 0.55f);
						}
						else
						{
							challengeButtons[i].labelColor = Menu.MenuColor(Menu.MenuColors.MediumGrey);
						}
						ExpeditionData.challengeList[i].UpdateDescription();
						challengeButtons[i].menuLabel.text = ExpeditionData.challengeList[i].description;
						challengeButtons[i].menuLabel.text = LabelTest.TrimText(challengeButtons[i].menuLabel.text, challengeButtons[i].size.x - 19f, addDots: true, bigText: true);
					}
					else
					{
						challengeButtons[i].menuLabel.text = "ERROR";
					}
				}
				else
				{
					hiddenToggles[i].buttonBehav.greyedOut = true;
					challengeButtons[i].buttonBehav.greyedOut = true;
					challengeButtons[i].menuLabel.text = menu.Translate("EMPTY");
				}
				bool flag = hiddenToggles[i].buttonBehav.greyedOut || ExpeditionData.challengeList[i].hidden;
				hiddenToggles[i].symbolSprite.SetElementByName(flag ? "hiddenclose" : "hiddenopen");
			}
			string newValue = ValueConverter.ConvertToString(ExpeditionGame.CalculateScore(predict: true));
			pointsLabel.text = menu.Translate("POINTS: <score>").Replace("<score>", newValue);
			UpdateLevelSprite();
		}
		if (ExpeditionData.activeMission != "" && deselectMissionButton == null)
		{
			float num = 100f;
			float num2 = LabelTest.GetWidth(menu.Translate("DESELECT")) + 10f;
			if (num2 > num)
			{
				num = num2;
			}
			deselectMissionButton = new SimpleButton(menu, this, menu.Translate("DESELECT"), "DESELECT", new Vector2(683f - num / 2f, 255f), new Vector2(num, 30f));
			subObjects.Add(deselectMissionButton);
		}
		SetUpSelectables();
	}
}
