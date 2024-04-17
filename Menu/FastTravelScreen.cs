using System.Collections.Generic;
using System.Threading;
using HUD;
using Menu.Remix.MixedUI;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace Menu;

public class FastTravelScreen : Menu, IOwnAHUD
{
	public global::HUD.HUD hud;

	private Map.MapData mapData;

	private MenuContainer[] hudContainers;

	public Region[] allRegions;

	private WorldLoader worldLoader;

	public World activeWorld;

	public List<int> discoveredSheltersInRegion;

	public int selectedShelter;

	public List<int> accessibleRegions;

	public int currentRegion;

	public int upcomingRegion;

	public FSprite fadeSprite;

	public float blackFade;

	public float lastBlackFade;

	public int fullBlackCounter;

	public bool lastMapButton;

	public bool showMap;

	public BigArrowButton prevButton;

	public BigArrowButton nextButton;

	public SimpleButton backButton;

	public HoldButton startButton;

	public MenuLabel mapButtonPrompt;

	public MenuLabel buttonInstruction;

	private float instructionAlpha;

	private float lastInstructionAlpha;

	public int freezeCounter;

	private bool noRegions;

	private string currentShelter;

	public bool initiateCharacterFastTravel;

	public string[] playerShelters;

	public SimpleButton chooseButton;

	public List<SimpleButton> choiceButtons;

	public RoundedRect choiceBackground;

	public World currentlyLoadingWorld;

	public Map.MapData currentlyLoadingMapData;

	public List<SimpleButton> slugcatButtons;

	public List<FSprite> slugcatLabels;

	public SlugcatStats.Name activeMenuSlugcat;

	public MenuLabel subtitleLabel;

	private bool lastPauseButton;

	private int choiceButtonsPerRow = 3;

	protected override bool FreezeMenuFunctions
	{
		get
		{
			if (!base.FreezeMenuFunctions)
			{
				return freezeCounter > 0;
			}
			return true;
		}
	}

	public bool IsFastTravelScreen => ID == ProcessManager.ProcessID.FastTravelScreen;

	public bool IsRegionsScreen => ID == ProcessManager.ProcessID.RegionsOverviewScreen;

	public int CurrentFood => 0;

	public Player.InputPackage MapInput => RWInput.PlayerInput(0);

	public bool RevealMap
	{
		get
		{
			if (hud != null)
			{
				return showMap;
			}
			return false;
		}
	}

	public Vector2 MapOwnerInRoomPosition
	{
		get
		{
			if (selectedShelter < 0 || activeWorld.GetAbstractRoom(selectedShelter) == null)
			{
				return activeWorld.GetAbstractRoom(MapOwnerRoom).mapPos;
			}
			return mapData.ShelterMarkerPosOfRoom(selectedShelter);
		}
	}

	public int MapOwnerRoom
	{
		get
		{
			if (selectedShelter < 0)
			{
				for (int i = 0; i < hud.map.mapObjects.Count; i++)
				{
					if (hud.map.mapObjects[i] is Map.FadeInMarker && !hud.map.notRevealedFadeMarkers.Contains(hud.map.mapObjects[i] as Map.FadeInMarker))
					{
						return (hud.map.mapObjects[i] as Map.FadeInMarker).room;
					}
				}
				return activeWorld.firstRoomIndex + Random.Range(0, activeWorld.NumberOfRooms - 1);
			}
			return selectedShelter;
		}
	}

	public bool MapDiscoveryActive => false;

	public static List<string> GetRegionOrder()
	{
		return new List<string>
		{
			"SU", "HI", "DS", "CC", "GW", "SH", "SL", "SI", "LF", "UW",
			"SS", "SB"
		};
	}

	public FastTravelScreen(ProcessManager manager, ProcessManager.ProcessID ID)
		: base(manager, ID)
	{
		bool flag = false;
		selectedShelter = -1;
		if (ModManager.ModdedRegionsEnabled)
		{
			slugcatButtons = new List<SimpleButton>();
			slugcatLabels = new List<FSprite>();
		}
		blackFade = 1f;
		lastBlackFade = 1f;
		accessibleRegions = new List<int>();
		discoveredSheltersInRegion = new List<int>();
		pages.Add(new Page(this, null, "main", 0));
		playerShelters = new string[ExtEnum<SlugcatStats.Name>.values.Count];
		for (int i = 0; i < playerShelters.Length; i++)
		{
			if (manager.rainWorld.progression.IsThereASavedGame(new SlugcatStats.Name(ExtEnum<SlugcatStats.Name>.values.GetEntry(i))))
			{
				playerShelters[i] = manager.rainWorld.progression.ShelterOfSaveGame(new SlugcatStats.Name(ExtEnum<SlugcatStats.Name>.values.GetEntry(i)));
			}
		}
		int num = -1;
		while (accessibleRegions.Count == 0 && num < ExtEnum<SlugcatStats.Name>.values.Count)
		{
			currentShelter = "SU_S01";
			accessibleRegions.Clear();
			activeMenuSlugcat = ((num == -1) ? manager.rainWorld.progression.PlayingAsSlugcat : new SlugcatStats.Name(ExtEnum<SlugcatStats.Name>.values.GetEntry(num)));
			if (ModManager.ModdedRegionsEnabled)
			{
				if (activeMenuSlugcat.Index >= 0 && activeMenuSlugcat.Index < playerShelters.Length && playerShelters[activeMenuSlugcat.Index] != null)
				{
					currentShelter = playerShelters[activeMenuSlugcat.Index];
				}
				else
				{
					for (int j = 0; j < playerShelters.Length; j++)
					{
						if (playerShelters[j] != null)
						{
							currentShelter = playerShelters[j];
							break;
						}
					}
				}
			}
			allRegions = Region.LoadAllRegions(activeMenuSlugcat);
			List<string> fullRegionOrder = Region.GetFullRegionOrder();
			if (ModManager.ModdedRegionsEnabled)
			{
				for (int k = 0; k < fullRegionOrder.Count; k++)
				{
					for (int l = 0; l < allRegions.Length; l++)
					{
						if (!(fullRegionOrder[k] == allRegions[l].name))
						{
							continue;
						}
						if (flag)
						{
							accessibleRegions.Add(l);
							continue;
						}
						foreach (PlayerProgression.MiscProgressionData.ConditionalShelterData item in manager.rainWorld.progression.miscProgressionData.GetDiscoveredSheltersInRegion(allRegions[l].name))
						{
							if (item.checkSlugcatIndex(activeMenuSlugcat) && !accessibleRegions.Contains(l))
							{
								accessibleRegions.Add(l);
							}
						}
					}
				}
			}
			else
			{
				for (int m = 0; m < fullRegionOrder.Count; m++)
				{
					for (int n = 0; n < allRegions.Length; n++)
					{
						if (fullRegionOrder[m] == allRegions[n].name && (flag || GetAccessibleShelterNamesOfRegion(allRegions[n].name) != null))
						{
							accessibleRegions.Add(n);
						}
					}
				}
			}
			num++;
		}
		if (accessibleRegions.Count == 0)
		{
			Custom.LogWarning("NO ACCESSIBLE REGIONS!");
			backButton = new SimpleButton(this, pages[0], Translate("BACK"), "BACK", new Vector2(200f, 100f), new Vector2(100f, 30f));
			pages[0].subObjects.Add(backButton);
			backObject = backButton;
			noRegions = true;
		}
		else
		{
			currentRegion = 0;
			upcomingRegion = -1;
			pages.Add(new Page(this, null, allRegions[accessibleRegions[0]].name, 1));
			pages[1].Container = new FContainer();
			container.AddChild(pages[1].Container);
			fadeSprite = new FSprite("Futile_White");
			fadeSprite.scaleX = 87.5f;
			fadeSprite.scaleY = 50f;
			fadeSprite.x = manager.rainWorld.screenSize.x / 2f;
			fadeSprite.y = manager.rainWorld.screenSize.y / 2f;
			fadeSprite.color = new Color(0f, 0f, 0f);
			container.AddChild(fadeSprite);
			gradientsContainer = new GradientsContainer(this, pages[0], new Vector2(0f, 0f), 0.5f);
			pages[0].subObjects.Add(gradientsContainer);
			Options.ControlSetup.Preset activePreset = manager.rainWorld.options.controls[0].GetActivePreset();
			mapButtonPrompt = new MenuLabel(text: (activePreset == Options.ControlSetup.Preset.PS4DualShock) ? (IsFastTravelScreen ? Translate("ps4_fast_travel_map_help") : Translate("ps4_region_map_help")) : ((!(activePreset == Options.ControlSetup.Preset.SwitchHandheld) && !(activePreset == Options.ControlSetup.Preset.SwitchDualJoycon) && !(activePreset == Options.ControlSetup.Preset.SwitchProController)) ? ((!(activePreset == Options.ControlSetup.Preset.SwitchSingleJoyconL) && !(activePreset == Options.ControlSetup.Preset.SwitchSingleJoyconR)) ? (IsFastTravelScreen ? Translate("Press the MAP button to select the shelter you wish to continue from") : Translate("Press the MAP button to view regional map")) : (IsFastTravelScreen ? Translate("switch_single_joycon_fast_travel_map_help") : Translate("switch_single_joycon_region_map_help"))) : (IsFastTravelScreen ? Translate("switch_fast_travel_map_help") : Translate("switch_region_map_help"))), menu: this, owner: pages[0], pos: new Vector2(583f, Mathf.Max(manager.rainWorld.options.SafeScreenOffset.y, 5f)), size: new Vector2(200f, 30f), bigText: false);
			mapButtonPrompt.label.color = Menu.MenuRGB(MenuColors.MediumGrey);
			pages[0].subObjects.Add(mapButtonPrompt);
			prevButton = new BigArrowButton(this, pages[0], "PREVIOUS", new Vector2(200f, 90f + Mathf.Max(manager.rainWorld.options.SafeScreenOffset.y - 5f, 0f)), -1);
			pages[0].subObjects.Add(prevButton);
			nextButton = new BigArrowButton(this, pages[0], "NEXT", new Vector2(1116f, 90f + Mathf.Max(manager.rainWorld.options.SafeScreenOffset.y - 5f, 0f)), 1);
			pages[0].subObjects.Add(nextButton);
			if (ModManager.MMF)
			{
				chooseButton = new SimpleButton(this, pages[0], Translate("CHOOSE"), "CHOOSE", new Vector2(nextButton.pos.x - 100f + nextButton.size.x, 668f), new Vector2(100f, 30f));
				pages[0].subObjects.Add(chooseButton);
				choiceButtons = new List<SimpleButton>();
			}
			if (IsFastTravelScreen)
			{
				startButton = new HoldButton(this, pages[0], Translate("HOLD TO START"), "HOLD TO START", new Vector2(683f, 115f + Mathf.Max(manager.rainWorld.options.SafeScreenOffset.y - 5f, 0f)), 80f);
				pages[0].subObjects.Add(startButton);
			}
			else if (IsRegionsScreen)
			{
				if (manager.rainWorld.inGameTranslator.currentLanguage == InGameTranslator.LanguageID.Portuguese)
				{
					backButton = new SimpleButton(this, pages[0], Translate("EXIT"), "BACK", new Vector2(prevButton.pos.x, 668f), new Vector2(100f, 30f));
				}
				else
				{
					backButton = new SimpleButton(this, pages[0], Translate("BACK"), "BACK", new Vector2(prevButton.pos.x, 668f), new Vector2(100f, 30f));
				}
				pages[0].subObjects.Add(backButton);
				backObject = backButton;
				SpawnSlugcatButtons();
			}
			hudContainers = new MenuContainer[2];
			for (int num2 = 0; num2 < 2; num2++)
			{
				hudContainers[num2] = new MenuContainer(this, pages[0], new Vector2(0f, 0f));
				pages[0].subObjects.Add(hudContainers[num2]);
			}
			string text2;
			if (activePreset == Options.ControlSetup.Preset.PS4DualShock || activePreset == Options.ControlSetup.Preset.PS5DualSense)
			{
				text2 = (IsFastTravelScreen ? "JUMP/THROW buttons - Switch layers_2a" : "JUMP/THROW buttons - Switch layers_2");
			}
			else if (activePreset == Options.ControlSetup.Preset.SwitchHandheld || activePreset == Options.ControlSetup.Preset.SwitchDualJoycon || activePreset == Options.ControlSetup.Preset.SwitchProController)
			{
				text2 = (IsFastTravelScreen ? "JUMP/THROW buttons - Switch layers_3a" : "JUMP/THROW buttons - Switch layers_3");
			}
			else if (activePreset == Options.ControlSetup.Preset.SwitchSingleJoyconL)
			{
				text2 = (IsFastTravelScreen ? "JUMP/THROW buttons - Switch layers_4a" : "JUMP/THROW buttons - Switch layers_4");
			}
			else if (activePreset == Options.ControlSetup.Preset.SwitchSingleJoyconR)
			{
				text2 = (IsFastTravelScreen ? "JUMP/THROW buttons - Switch layers_5a" : "JUMP/THROW buttons - Switch layers_5");
			}
			else
			{
				text2 = "JUMP/THROW buttons - Switch layers";
				if (IsFastTravelScreen)
				{
					text2 += "<LINE>PICK UP button - Select shelter";
				}
			}
			text2 = Translate(text2);
			text2 = text2.Replace("<LINE>", "     ");
			buttonInstruction = new MenuLabel(this, pages[0], text2, new Vector2(583f, Mathf.Max(manager.rainWorld.options.SafeScreenOffset.y, 5f)), new Vector2(200f, 30f), bigText: false);
			buttonInstruction.label.color = Menu.MenuRGB(MenuColors.DarkGrey);
			pages[0].subObjects.Add(buttonInstruction);
			selectedObject = null;
			if (currentShelter != null)
			{
				for (int num3 = 0; num3 < accessibleRegions.Count; num3++)
				{
					if (allRegions[accessibleRegions[num3]].name == currentShelter.Substring(0, 2))
					{
						Custom.Log(currentShelter);
						Custom.Log("found start region:", num3.ToString(), allRegions[accessibleRegions[num3]].name);
						currentRegion = num3;
						break;
					}
				}
			}
			InitiateRegionSwitch(currentRegion);
			worldLoader.NextActivity();
			while (!worldLoader.Finished)
			{
				worldLoader.Update();
				Thread.Sleep(1);
			}
			AddWorldLoaderResultToLoadedWorlds(currentRegion);
			FinalizeRegionSwitch(currentRegion);
			worldLoader = null;
			hud = new global::HUD.HUD(new FContainer[2]
			{
				hudContainers[1].Container,
				hudContainers[0].Container
			}, manager.rainWorld, this);
		}
		mySoundLoopID = ((ID == ProcessManager.ProcessID.RegionsOverviewScreen) ? SoundID.MENU_Main_Menu_LOOP : SoundID.MENU_Fast_Travel_Screen_LOOP);
		if (IsFastTravelScreen)
		{
			RainWorld.ShelterBeforePassage = currentShelter;
		}
	}

	public override void Update()
	{
		base.Update();
		bool flag = RWInput.CheckPauseButton(0);
		if (flag && !lastPauseButton)
		{
			if (showMap)
			{
				showMap = false;
				if (IsFastTravelScreen)
				{
					selectedObject = startButton;
				}
			}
			else
			{
				manager.RequestMainProcessSwitch(ProcessManager.ProcessID.MainMenu);
				PlaySound(SoundID.MENU_Switch_Page_Out);
			}
		}
		lastPauseButton = flag;
		if (noRegions)
		{
			return;
		}
		lastInstructionAlpha = instructionAlpha;
		if (hud != null && hud.map != null)
		{
			instructionAlpha = Mathf.Min(Custom.LerpAndTick(instructionAlpha, hud.map.fade, 0.015f, 0.0020833334f), hud.map.fade);
		}
		else
		{
			instructionAlpha = 0f;
		}
		lastBlackFade = blackFade;
		float num = blackFade;
		if (hud != null)
		{
			if (hud.map == null && mapData != null)
			{
				hud.InitFastTravelHud(mapData);
			}
			else
			{
				hud.Update();
			}
			Player.InputPackage inputPackage = RWInput.PlayerInput(0);
			if (inputPackage.mp && !lastMapButton)
			{
				showMap = hud.map != null && !showMap;
				if (IsFastTravelScreen)
				{
					if (showMap)
					{
						selectedObject = null;
					}
					else
					{
						selectedObject = startButton;
					}
				}
			}
			lastMapButton = inputPackage.mp;
			if (hud.map != null)
			{
				num = Mathf.Lerp(0f, 0.5f, hud.map.fade);
			}
		}
		else
		{
			num = 1f;
		}
		if (worldLoader != null)
		{
			showMap = false;
			if (blackFade >= 1f && lastBlackFade >= 1f)
			{
				fullBlackCounter++;
				if (fullBlackCounter > 5)
				{
					if (worldLoader.Finished)
					{
						AddWorldLoaderResultToLoadedWorlds(upcomingRegion);
						worldLoader = null;
						fullBlackCounter = 0;
					}
					else
					{
						worldLoader.NextActivity();
						while (!worldLoader.Finished)
						{
							worldLoader.Update();
							Thread.Sleep(1);
						}
					}
				}
			}
			else
			{
				fullBlackCounter = 0;
			}
			num = 1f;
		}
		else if (upcomingRegion != -1 && upcomingRegion != currentRegion && currentlyLoadingWorld != null)
		{
			showMap = false;
			if (blackFade >= 1f && lastBlackFade >= 1f)
			{
				FinalizeRegionSwitch(upcomingRegion);
			}
			num = 1f;
		}
		if (blackFade < num)
		{
			blackFade = Custom.LerpAndTick(blackFade, num, 0.05f, 1f / 15f);
		}
		else
		{
			blackFade = Custom.LerpAndTick(blackFade, num, 0.05f, 0.125f);
		}
		if (showMap)
		{
			DestroyChoiceMenu();
			freezeCounter = 20;
		}
		else if ((float)freezeCounter > 0f)
		{
			freezeCounter--;
		}
		prevButton.buttonBehav.greyedOut = showMap || accessibleRegions.Count < 2;
		nextButton.buttonBehav.greyedOut = showMap || accessibleRegions.Count < 2;
		if (startButton != null)
		{
			startButton.buttonBehav.greyedOut = showMap;
		}
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		if (!noRegions)
		{
			if (hud != null)
			{
				hud.Draw(timeStacker);
			}
			fadeSprite.alpha = Mathf.Lerp(lastBlackFade, blackFade, timeStacker);
			mapButtonPrompt.label.alpha = 1f - Mathf.Lerp(lastBlackFade, blackFade, timeStacker);
			if (hud != null && hud.map != null)
			{
				mapButtonPrompt.label.alpha *= 1f - Mathf.Lerp(hud.map.lastFade, hud.map.fade, timeStacker);
				buttonInstruction.label.alpha = Custom.SCurve(Mathf.Min(Mathf.Lerp(hud.map.lastFade, hud.map.fade, timeStacker), Mathf.InverseLerp(0.5f, 1f, Mathf.Lerp(lastInstructionAlpha, instructionAlpha, timeStacker))), 0.6f);
			}
			else
			{
				buttonInstruction.label.alpha = 0f;
			}
		}
	}

	public override void Singal(MenuObject sender, string message)
	{
		if (message != "CHOOSE")
		{
			DestroyChoiceMenu();
		}
		switch (message)
		{
		case "HOLD TO START":
			if (initiateCharacterFastTravel)
			{
				manager.menuSetup.startGameCondition = ProcessManager.MenuSetup.StoryGameInitCondition.StartWithFastTravel;
			}
			else
			{
				manager.menuSetup.startGameCondition = ProcessManager.MenuSetup.StoryGameInitCondition.FastTravel;
			}
			manager.menuSetup.regionSelectRoom = activeWorld.GetAbstractRoom(selectedShelter).name;
			RainWorld.ShelterAfterPassage = manager.menuSetup.regionSelectRoom;
			manager.rainWorld.progression.miscProgressionData.menuRegion = activeWorld.name;
			manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Game);
			PlaySound(SoundID.MENU_Continue_Game);
			break;
		case "PREVIOUS":
			StepRegion(-1);
			break;
		case "NEXT":
			StepRegion(1);
			break;
		case "BACK":
			if (IsFastTravelScreen)
			{
				manager.RequestMainProcessSwitch(ProcessManager.ProcessID.SlugcatSelect);
			}
			else
			{
				manager.RequestMainProcessSwitch(ProcessManager.ProcessID.MainMenu);
			}
			PlaySound(SoundID.MENU_Switch_Page_Out);
			break;
		case "CHOOSE":
			PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
			if (choiceButtons.Count == 0)
			{
				SpawnChoiceMenu();
			}
			else
			{
				DestroyChoiceMenu();
			}
			break;
		}
		if (message.StartsWith("SLUG"))
		{
			string value = message.Substring(4);
			manager.rainWorld.progression.miscProgressionData.currentlySelectedSinglePlayerSlugcat = new SlugcatStats.Name(value);
			PlaySound(SoundID.MENU_Regions_Switch_Region);
			manager.RequestMainProcessSwitch(ProcessManager.ProcessID.RegionsOverviewScreen);
		}
		else if (message.Contains("CHOICE"))
		{
			int num = int.Parse(message.Substring("CHOICE".Length));
			if (num == currentRegion)
			{
				PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
				return;
			}
			PlaySound(SoundID.MENU_Regions_Switch_Region);
			InitiateRegionSwitch(num);
		}
	}

	public override void ShutDownProcess()
	{
		base.ShutDownProcess();
		DestroySlugcatButtons();
		if (hud != null && hud.map != null)
		{
			hud.map.ClearSprites();
		}
		currentlyLoadingWorld = null;
		currentlyLoadingMapData = null;
		worldLoader = null;
		activeWorld = null;
	}

	private void StepRegion(int change)
	{
		PlaySound(SoundID.MENU_Regions_Switch_Region);
		int num = currentRegion + change;
		if (num < 0)
		{
			num = accessibleRegions.Count - 1;
		}
		else if (num >= accessibleRegions.Count)
		{
			num = 0;
		}
		InitiateRegionSwitch(num);
	}

	private void InitiateRegionSwitch(int switchToRegion)
	{
		upcomingRegion = switchToRegion;
		if (currentlyLoadingWorld == null)
		{
			worldLoader = new WorldLoader(null, activeMenuSlugcat, singleRoomWorld: false, allRegions[accessibleRegions[upcomingRegion]].name, allRegions[accessibleRegions[upcomingRegion]], manager.rainWorld.setup, WorldLoader.LoadingContext.FASTTRAVEL);
		}
	}

	public void FinalizeRegionSwitch(int newRegion)
	{
		activeWorld = currentlyLoadingWorld;
		currentlyLoadingWorld = null;
		mapData = currentlyLoadingMapData;
		currentlyLoadingMapData = null;
		List<string> list = GetAccessibleShelterNamesOfRegion(allRegions[accessibleRegions[newRegion]].name);
		if (list == null)
		{
			list = new List<string>();
		}
		discoveredSheltersInRegion.Clear();
		int num = 0;
		while (list.Count > 0 && num < activeWorld.NumberOfRooms)
		{
			if (activeWorld.GetAbstractRoom(activeWorld.firstRoomIndex + num).shelter)
			{
				AbstractRoom abstractRoom = activeWorld.GetAbstractRoom(activeWorld.firstRoomIndex + num);
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i] == abstractRoom.name)
					{
						list.RemoveAt(i);
						discoveredSheltersInRegion.Add(abstractRoom.index);
						break;
					}
				}
			}
			num++;
		}
		if (discoveredSheltersInRegion.Count > 0)
		{
			selectedShelter = discoveredSheltersInRegion[Random.Range(0, discoveredSheltersInRegion.Count)];
			bool flag = false;
			if (currentShelter != null && currentShelter.Substring(0, 2) == allRegions[accessibleRegions[newRegion]].name)
			{
				for (int j = 0; j < discoveredSheltersInRegion.Count; j++)
				{
					if (currentShelter == activeWorld.GetAbstractRoom(discoveredSheltersInRegion[j]).name)
					{
						selectedShelter = discoveredSheltersInRegion[j];
						flag = true;
						break;
					}
				}
			}
			if (!flag && IsRegionsScreen)
			{
				for (int k = 0; k < playerShelters.Length; k++)
				{
					if (playerShelters[k] != null && playerShelters[k].Substring(0, 2) == allRegions[accessibleRegions[newRegion]].name)
					{
						for (int l = 0; l < discoveredSheltersInRegion.Count; l++)
						{
							if (playerShelters[k] == activeWorld.GetAbstractRoom(discoveredSheltersInRegion[l]).name)
							{
								selectedShelter = discoveredSheltersInRegion[l];
								flag = true;
								break;
							}
						}
					}
					if (flag)
					{
						break;
					}
				}
			}
			if (!flag)
			{
				for (int m = 0; m < 10; m++)
				{
					if (activeWorld.GetAbstractRoom(selectedShelter) != null && activeWorld.GetAbstractRoom(selectedShelter).shelterIndex >= 0 && !activeWorld.brokenShelters[activeWorld.GetAbstractRoom(selectedShelter).shelterIndex])
					{
						break;
					}
					selectedShelter = discoveredSheltersInRegion[Random.Range(0, discoveredSheltersInRegion.Count)];
				}
			}
		}
		else
		{
			selectedShelter = -1;
		}
		currentRegion = newRegion;
		upcomingRegion = -1;
		if (manager.rainWorld.progression.currentSaveState == null || (IsRegionsScreen && activeMenuSlugcat != manager.rainWorld.progression.currentSaveState.saveStateNumber))
		{
			manager.rainWorld.progression.currentSaveState = manager.rainWorld.progression.GetOrInitiateSaveState(activeMenuSlugcat, null, manager.menuSetup, saveAsDeathOrQuit: false);
		}
		Region region = allRegions[accessibleRegions[currentRegion]];
		if (manager.rainWorld.progression.currentSaveState.regionStates[region.regionNumber] == null)
		{
			manager.rainWorld.progression.currentSaveState.regionStates[region.regionNumber] = new RegionState(manager.rainWorld.progression.currentSaveState, activeWorld);
		}
		if (hud != null && hud.map != null)
		{
			hud.InitFastTravelHud(mapData);
		}
		bool flag2 = false;
		if (scene != null)
		{
			scene.Hide();
			scene.RemoveSprites();
			pages[1].RemoveSubObject(scene);
			AssetManager.HardCleanFutileAssets();
			flag2 = true;
		}
		scene = new InteractiveMenuScene(this, pages[1], Region.GetRegionLandscapeScene(allRegions[accessibleRegions[newRegion]].name));
		pages[1].subObjects.Add(scene);
		if (flag2)
		{
			scene.HorizontalDisplace(0f - Menu.HorizontalMoveToGetCentered(manager));
		}
		scene.Show();
		if (subtitleLabel != null)
		{
			subtitleLabel.RemoveSprites();
			pages[1].RemoveSubObject(subtitleLabel);
		}
		string text = Translate(Region.GetRegionFullName(manager.rainWorld.progression.regionNames[accessibleRegions[newRegion]], activeMenuSlugcat));
		if (text != Region.GetRegionFullName(manager.rainWorld.progression.regionNames[accessibleRegions[newRegion]], SlugcatStats.Name.White))
		{
			Vector2 subtitleLabelOffset = GetSubtitleLabelOffset(Region.GetRegionLandscapeScene(allRegions[accessibleRegions[newRegion]].name));
			subtitleLabel = new MenuLabel(this, pages[1], "~ " + text + " ~", new Vector2(383.01f, 460.01f) + subtitleLabelOffset, new Vector2(600f, 40f), bigText: true);
			subtitleLabel.label.shader = manager.rainWorld.Shaders["MenuText"];
			FSprite node = new FSprite("Futile_White")
			{
				scaleX = LabelTest.GetWidth(text, bigText: true) / 7f + 5f,
				scaleY = 6f,
				shader = manager.rainWorld.Shaders["FlatLight"],
				color = MenuColorEffect.rgbBlack,
				alpha = 0.5f,
				anchorX = 0.5f,
				anchorY = 0.5f,
				x = 683f - Menu.HorizontalMoveToGetCentered(manager) + subtitleLabelOffset.x,
				y = 480f + subtitleLabelOffset.y
			};
			subtitleLabel.Container.AddChild(node);
			pages[1].subObjects.Add(subtitleLabel);
		}
	}

	private static Vector2 GetSubtitleLabelOffset(MenuScene.SceneID sceneID)
	{
		if (ModManager.MSC && sceneID == MoreSlugcatsEnums.MenuSceneID.Landscape_MS)
		{
			return new Vector2(0f, -80f);
		}
		return Vector2.zero;
	}

	private List<string> GetAccessibleShelterNamesOfRegion(string regionAcronym)
	{
		if (ModManager.ModdedRegionsEnabled)
		{
			SlugcatStats.Name playingAsSlugcat = manager.rainWorld.progression.PlayingAsSlugcat;
			if (activeMenuSlugcat != null)
			{
				playingAsSlugcat = activeMenuSlugcat;
			}
			List<string> list = new List<string>();
			foreach (PlayerProgression.MiscProgressionData.ConditionalShelterData item in manager.rainWorld.progression.miscProgressionData.GetDiscoveredSheltersInRegion(regionAcronym))
			{
				if (item.checkSlugcatIndex(playingAsSlugcat))
				{
					list.Add(item.shelterName);
				}
			}
			if (list.Count == 0)
			{
				return null;
			}
			return list;
		}
		int num = -1;
		for (int i = 0; i < allRegions.Length; i++)
		{
			if (regionAcronym == allRegions[i].name)
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			return null;
		}
		if (!manager.rainWorld.progression.miscProgressionData.discoveredShelters.ContainsKey(regionAcronym) || manager.rainWorld.progression.miscProgressionData.discoveredShelters[regionAcronym] == null)
		{
			return null;
		}
		List<string> list2 = new List<string>();
		for (int j = 0; j < manager.rainWorld.progression.miscProgressionData.discoveredShelters[regionAcronym].Count; j++)
		{
			list2.Add(manager.rainWorld.progression.miscProgressionData.discoveredShelters[regionAcronym][j]);
		}
		if (list2.Count < 1)
		{
			return null;
		}
		return list2;
	}

	public void AddWorldLoaderResultToLoadedWorlds(int reg)
	{
		currentlyLoadingWorld = worldLoader.ReturnWorld();
		currentlyLoadingMapData = new Map.MapData(currentlyLoadingWorld, manager.rainWorld);
		worldLoader = null;
	}

	public void SelectNewShelter(int shelter)
	{
		selectedShelter = shelter;
	}

	public global::HUD.HUD.OwnerType GetOwnerType()
	{
		if (IsFastTravelScreen)
		{
			return global::HUD.HUD.OwnerType.FastTravelScreen;
		}
		return global::HUD.HUD.OwnerType.RegionOverview;
	}

	public void PlayHUDSound(SoundID soundID)
	{
		PlaySound(soundID);
	}

	public void FoodCountDownDone()
	{
	}

	public void SpawnChoiceMenu()
	{
		if (!ModManager.MMF || choiceButtons.Count != 0)
		{
			return;
		}
		float num = 200f;
		float num2 = 10f;
		float num3 = nextButton.pos.x + nextButton.size.x;
		float num4 = 638f - num2;
		float num5 = (num + num2) * (float)Mathf.Min(choiceButtonsPerRow, accessibleRegions.Count) + num2 * 2f;
		float num6 = (float)((int)Mathf.Floor((accessibleRegions.Count - 1) / choiceButtonsPerRow) + 1) * (30f + num2) + num2 * 2f;
		choiceBackground = new RoundedRect(this, pages[0], new Vector2(num3 - num5, num4 + 15f + num2 * 2f - num6), new Vector2(num5, num6), filled: true);
		choiceBackground.fillAlpha = 0.85f;
		pages[0].subObjects.Add(choiceBackground);
		for (int i = 0; i < accessibleRegions.Count; i++)
		{
			float x = num3 - (num + num2) * ((float)(i % choiceButtonsPerRow) + 1f);
			float y = num4 - num2 - Mathf.Floor(i / choiceButtonsPerRow) * (30f + num2);
			SimpleButton simpleButton = new SimpleButton(this, pages[0], Translate(Region.GetRegionFullName(manager.rainWorld.progression.regionNames[accessibleRegions[i]], activeMenuSlugcat)), "CHOICE" + i, new Vector2(x, y), new Vector2(num, 30f));
			if (i % choiceButtonsPerRow == choiceButtonsPerRow - 1 || i == accessibleRegions.Count - 1)
			{
				simpleButton.nextSelectable[0] = ((backButton == null) ? simpleButton : backButton);
			}
			if (i % choiceButtonsPerRow == 0)
			{
				simpleButton.nextSelectable[2] = simpleButton;
			}
			if (i + choiceButtonsPerRow >= accessibleRegions.Count)
			{
				if (slugcatButtons != null && slugcatButtons.Count > 0)
				{
					if (i % choiceButtonsPerRow == 0)
					{
						simpleButton.nextSelectable[3] = nextButton;
					}
					else if (i % choiceButtonsPerRow == choiceButtonsPerRow - 1)
					{
						simpleButton.nextSelectable[3] = slugcatButtons[0];
					}
					else
					{
						simpleButton.nextSelectable[3] = slugcatButtons[slugcatButtons.Count - 1];
					}
				}
				else
				{
					simpleButton.nextSelectable[3] = nextButton;
				}
			}
			if (i < choiceButtonsPerRow)
			{
				simpleButton.nextSelectable[1] = chooseButton;
			}
			choiceButtons.Add(simpleButton);
			pages[0].subObjects.Add(simpleButton);
		}
		chooseButton.nextSelectable[3] = choiceButtons[0];
		nextButton.nextSelectable[1] = choiceButtons[(int)Mathf.Floor((choiceButtons.Count - 1) / choiceButtonsPerRow) * choiceButtonsPerRow];
		if (slugcatButtons == null || slugcatButtons.Count <= 0)
		{
			return;
		}
		for (int j = 0; j < slugcatButtons.Count; j++)
		{
			int num7 = -1;
			for (int num8 = choiceButtons.Count - 1; num8 >= 0; num8--)
			{
				if (num8 % choiceButtonsPerRow == choiceButtonsPerRow - 1)
				{
					num7 = num8;
					break;
				}
			}
			if (num7 == -1)
			{
				num7 = choiceButtons.Count - 1;
			}
			slugcatButtons[j].nextSelectable[1] = choiceButtons[num7];
		}
	}

	public void DestroyChoiceMenu()
	{
		if (!ModManager.MMF || choiceButtons == null || choiceButtons.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < choiceButtons.Count; i++)
		{
			choiceButtons[i].RemoveSprites();
			pages[0].RemoveSubObject(choiceButtons[i]);
		}
		choiceButtons.Clear();
		choiceBackground.RemoveSprites();
		pages[0].RemoveSubObject(choiceBackground);
		choiceBackground = null;
		chooseButton.nextSelectable[3] = nextButton;
		nextButton.nextSelectable[1] = chooseButton;
		if (slugcatButtons != null && slugcatButtons.Count > 0)
		{
			for (int j = 0; j < slugcatButtons.Count; j++)
			{
				slugcatButtons[j].nextSelectable[1] = ((chooseButton == null) ? backButton : chooseButton);
			}
		}
	}

	public List<string> GetRegionsVisited(SlugcatStats.Name saveSlot)
	{
		List<string> list = new List<string>();
		if (manager.rainWorld.progression.miscProgressionData.ConditionalShelterDiscovery.Count > 0)
		{
			foreach (PlayerProgression.MiscProgressionData.ConditionalShelterData item in manager.rainWorld.progression.miscProgressionData.ConditionalShelterDiscovery)
			{
				if (item.checkSlugcatIndex(saveSlot) && !list.Contains(item.GetShelterRegion()))
				{
					list.Add(item.GetShelterRegion());
				}
			}
		}
		return list;
	}

	public void SpawnSlugcatButtons()
	{
		if (!ModManager.ModdedRegionsEnabled)
		{
			return;
		}
		DestroySlugcatButtons();
		foreach (string entry in ExtEnum<SlugcatStats.Name>.values.entries)
		{
			SlugcatStats.Name name = new SlugcatStats.Name(entry);
			if (!SlugcatStats.HiddenOrUnplayableSlugcat(name) && GetRegionsVisited(name).Count > 0)
			{
				SimpleButton simpleButton = new SimpleButton(this, pages[0], "", "SLUG" + entry, new Vector2(manager.rainWorld.options.ScreenSize.x / 2f + (1366f - manager.rainWorld.options.ScreenSize.x) / 2f, 90f), new Vector2(48f, 48f));
				if (activeMenuSlugcat == name)
				{
					simpleButton.toggled = true;
				}
				simpleButton.nextSelectable[3] = simpleButton;
				simpleButton.nextSelectable[1] = ((chooseButton == null) ? backButton : chooseButton);
				FSprite fSprite = new FSprite("Kill_Slugcat");
				fSprite.color = PlayerGraphics.DefaultSlugcatColor(name);
				slugcatLabels.Add(fSprite);
				slugcatButtons.Add(simpleButton);
				pages[0].Container.AddChild(fSprite);
				pages[0].subObjects.Add(simpleButton);
			}
		}
		float num = (float)slugcatButtons.Count * 56f;
		for (int i = 0; i < slugcatButtons.Count; i++)
		{
			SimpleButton simpleButton2 = slugcatButtons[i];
			simpleButton2.pos.x = simpleButton2.pos.x + ((float)i * 56f - num * 0.5f);
			slugcatLabels[i].x = simpleButton2.pos.x + simpleButton2.size.x * 0.5f - (1366f - manager.rainWorld.options.ScreenSize.x) / 2f;
			slugcatLabels[i].y = simpleButton2.pos.y + simpleButton2.size.y * 0.5f;
		}
	}

	public void DestroySlugcatButtons()
	{
		if (!ModManager.ModdedRegionsEnabled)
		{
			return;
		}
		if (slugcatButtons != null)
		{
			for (int i = 0; i < slugcatButtons.Count; i++)
			{
				slugcatButtons[i].RemoveSprites();
				pages[0].RemoveSubObject(slugcatButtons[i]);
			}
			slugcatButtons.Clear();
		}
		if (slugcatLabels != null)
		{
			for (int j = 0; j < slugcatLabels.Count; j++)
			{
				slugcatLabels[j].RemoveFromContainer();
			}
			slugcatLabels.Clear();
		}
	}
}
