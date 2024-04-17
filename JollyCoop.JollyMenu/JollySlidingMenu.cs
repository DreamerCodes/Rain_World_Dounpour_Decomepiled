using System.Globalization;
using Expedition;
using Menu;
using Menu.Remix;
using Menu.Remix.MixedUI;
using RWCustom;
using UnityEngine;

namespace JollyCoop.JollyMenu;

public class JollySlidingMenu : PositionedMenuObject, SelectOneButton.SelectOneButtonOwner
{
	public JollyPlayerSelector[] playerSelector;

	public ControlsButton controlsButton;

	public OpSliderTick numberPlayersSlider;

	public UIelementWrapper sliderWrapper;

	private SelectOneButton[] difficultyButtons;

	public SelectOneButton[] cameraSwitchQuickness;

	private SelectOneButton[] colorModeButtons;

	private new JollySetupDialog menu;

	public SimpleButton manualButton;

	public SymbolButtonToggle friendlyToggle;

	public SymbolButtonToggle cameraCyclesToggle;

	public SymbolButtonToggle smartShortcutToggle;

	private SymbolButtonToggle friendlyLizardsToggle;

	private SymbolButtonToggle friendlySteal;

	private SymbolButtonToggle hudToggle;

	public SymbolButton colorInfoSymbol;

	public SymbolButton cameraInfoSymbol;

	public SymbolButton difficultyInfoSymbol;

	public Options Options => menu.manager.rainWorld.options;

	public JollySlidingMenu(JollySetupDialog menu, MenuObject owner, Vector2 pos)
		: base(menu, owner, pos)
	{
		this.menu = menu;
		menu.tabWrapper = new MenuTabWrapper(menu, this);
		subObjects.Add(menu.tabWrapper);
		AddJollyTitle();
		int num = 100;
		float num2 = (1024f - (float)num * 4f) / 5f;
		float num3 = 171f;
		Vector2 vector = new Vector2(num3 + num2, 0f);
		Vector2 vector2 = vector + new Vector2(0f, menu.manager.rainWorld.screenSize.y * 0.55f);
		playerSelector = new JollyPlayerSelector[4];
		for (int i = 0; i < 4; i++)
		{
			playerSelector[i] = new JollyPlayerSelector(menu, this, vector2, i);
			subObjects.Add(playerSelector[i]);
			vector2 += new Vector2(num2 + (float)num, 0f);
		}
		colorModeButtons = new SelectOneButton[ExtEnum<Options.JollyColorMode>.values.Count];
		cameraSwitchQuickness = new SelectOneButton[ExtEnum<Options.JollyCameraInputSpeed>.values.Count];
		difficultyButtons = new SelectOneButton[ExtEnum<Options.JollyDifficulty>.values.Count];
		float[] array = new float[Mathf.Max(colorModeButtons.Length, cameraSwitchQuickness.Length, difficultyButtons.Length)];
		float[] array2 = new float[array.Length];
		for (int j = 0; j < array.Length; j++)
		{
			array[j] = 125f;
			array2[j] = 100f;
			if (menu.CurrLang == InGameTranslator.LanguageID.Italian || menu.CurrLang == InGameTranslator.LanguageID.Russian)
			{
				array2[j] = 115f;
			}
			else if (menu.CurrLang == InGameTranslator.LanguageID.German)
			{
				array[j] = 110f;
				if (j == 2)
				{
					array2[j] = 130f;
				}
			}
			else if (menu.CurrLang == InGameTranslator.LanguageID.Spanish || menu.CurrLang == InGameTranslator.LanguageID.Portuguese)
			{
				array[j] = 120f;
				if (j == 2)
				{
					array2[j] = 110f;
				}
			}
		}
		float num4 = 0f;
		for (int k = 0; k < colorModeButtons.Length; k++)
		{
			AddSelectOneButton(colorModeButtons, menu.Translate(ExtEnum<Options.JollyColorMode>.values.GetEntry(k)), "ColorMode", k, vector + new Vector2(num4, 175f), array2[k]);
			num4 += array[k];
		}
		AddLabelToSelectOne(colorModeButtons, menu.Translate("Color Mode"), menu.Translate("ADJUST_COLOR"));
		colorInfoSymbol = AddSymbolToSelectOne(colorModeButtons, "INFO_COLOR");
		num4 = 0f;
		for (int l = 0; l < cameraSwitchQuickness.Length; l++)
		{
			AddSelectOneButton(cameraSwitchQuickness, menu.Translate(ExtEnum<Options.JollyCameraInputSpeed>.values.GetEntry(l)), "CameraInput", l, vector + new Vector2(num4, 125f), array2[l]);
			num4 += array[l];
		}
		AddLabelToSelectOne(cameraSwitchQuickness, menu.Translate("Camera Input"), menu.Translate("ADJUST_CAMERA"));
		cameraInfoSymbol = AddSymbolToSelectOne(cameraSwitchQuickness, "INFO_CAMERA");
		num4 = 0f;
		for (int m = 0; m < difficultyButtons.Length; m++)
		{
			AddSelectOneButton(difficultyButtons, menu.Translate(ExtEnum<Options.JollyDifficulty>.values.GetEntry(m)), "Difficulty", m, vector + new Vector2(num4, 75f), array2[m]);
			num4 += array[m];
		}
		AddLabelToSelectOne(difficultyButtons, menu.Translate("Difficulty"), menu.Translate("ADJUST_DIFFICULTY"));
		difficultyInfoSymbol = AddSymbolToSelectOne(difficultyButtons, "INFO_DIFF");
		numberPlayersSlider = new OpSliderTick(menu.oi.config.Bind("_cosmetic", Custom.rainWorld.options.JollyPlayerCount, new ConfigAcceptableRange<int>(1, 4)), playerSelector[0].pos + new Vector2((float)num / 2f, 130f), (int)(playerSelector[3].pos - playerSelector[0].pos).x);
		numberPlayersSlider.description = menu.Translate("Adjust the number of players");
		sliderWrapper = new UIelementWrapper(menu.tabWrapper, numberPlayersSlider);
		numberPlayersSlider.OnValueUpdate += NumberPlayersChange;
		MenuLabel item = new MenuLabel(menu, this, menu.Translate("Adjust the number of players"), new Vector2(623f, numberPlayersSlider.PosY + 25f), new Vector2(120f, 30f), bigText: false)
		{
			label = 
			{
				alignment = FLabelAlignment.Center
			}
		};
		subObjects.Add(item);
		FTextParams fTextParams = new FTextParams();
		FTextParams fTextParams2 = new FTextParams();
		if (InGameTranslator.LanguageID.UsesLargeFont(menu.CurrLang))
		{
			fTextParams.lineHeightOffset = -15f;
			fTextParams2.lineHeightOffset = -10f;
		}
		bool textAboveButton = false;
		if (menu.CurrLang == InGameTranslator.LanguageID.French || menu.CurrLang == InGameTranslator.LanguageID.Russian)
		{
			textAboveButton = true;
		}
		Vector2 vector3 = new Vector2(playerSelector[3].pos.x + 100f - 70f, 160f);
		friendlyToggle = AddSymbolToggleButton(Custom.rainWorld.options.friendlyFire, "friendly_fire", menu.Translate("description_spears_on"), menu.Translate("description_spears_off"), vector3, new Vector2(70f, 70f), "fire", Custom.ReplaceLineDelimeters(menu.Translate("Spears hit")), Custom.ReplaceLineDelimeters(menu.Translate("Spears miss")), textAboveButton, fTextParams2);
		hudToggle = AddSymbolToggleButton(Custom.rainWorld.options.jollyHud, "hud", menu.Translate("description_hud_on"), menu.Translate("description_hud_off"), vector3 - new Vector2(130f, 0f), new Vector2(70f, 70f), "hud", Custom.ReplaceLineDelimeters(menu.Translate("HUD on")), Custom.ReplaceLineDelimeters(menu.Translate("HUD off")), textAboveButton, fTextParams2);
		cameraCyclesToggle = AddSymbolToggleButton(Custom.rainWorld.options.cameraCycling, "camera_cycle", menu.Translate("description_camera_toggle_off"), menu.Translate("description_camera_toggle_on"), vector3 - new Vector2(0f, 100f), new Vector2(70f, 70f), "cyclecamera", Custom.ReplaceLineDelimeters(menu.Translate("Camera cycles")), Custom.ReplaceLineDelimeters(menu.Translate("Camera doesn't cycle")), textAboveButton: false, fTextParams2);
		smartShortcutToggle = AddSymbolToggleButton(Custom.rainWorld.options.smartShortcuts, "smartpipe", menu.Translate("description_smart_shorcuts_off"), menu.Translate("description_smart_shorcuts_on"), vector3 - new Vector2(130f, 100f), new Vector2(70f, 70f), "smartpipe", Custom.ReplaceLineDelimeters(menu.Translate("Smart shortcuts")), Custom.ReplaceLineDelimeters(menu.Translate("Vanilla shortcuts")), textAboveButton: false, fTextParams);
		friendlyLizardsToggle = AddSymbolToggleButton(Custom.rainWorld.options.friendlyLizards, "friendlylizard", menu.Translate("description_friendlylizards_off"), menu.Translate("description_friendlylizards_on"), vector3 - new Vector2(260f, 0f), new Vector2(70f, 70f), "friendlylizard", Custom.ReplaceLineDelimeters(menu.Translate("Friendly lizards")), Custom.ReplaceLineDelimeters(menu.Translate("Vanilla lizards")), textAboveButton, fTextParams2);
		friendlySteal = AddSymbolToggleButton(Custom.rainWorld.options.friendlySteal, "friendlysteal", menu.Translate("description_friendlystealing_off"), menu.Translate("description_friendlystealing_on"), vector3 - new Vector2(260f, 100f), new Vector2(70f, 70f), "friendlystealing", Custom.ReplaceLineDelimeters(menu.Translate("Friendly stealing")), Custom.ReplaceLineDelimeters(menu.Translate("No stealing")), textAboveButton: false, fTextParams);
		controlsButton = new ControlsButton(menu, this, new Vector2(200f, Custom.rainWorld.screenSize.y - 100f - 40f), menu.Translate("Input Settings"));
		subObjects.Add(controlsButton);
		manualButton = new SimpleButton(menu, this, menu.Translate("JOLLY_MANUAL_BUTTON").ToUpperInvariant(), "JOLLY_MANUAL", menu.cancelButton.pos - new Vector2(0f, -45f), new Vector2(110f, 30f));
		subObjects.Add(manualButton);
		BindButtons();
	}

	private void BindButtons()
	{
		JollyPlayerSelector[] array = playerSelector;
		foreach (JollyPlayerSelector obj in array)
		{
			obj.pLabelSelectorWrapper.nextSelectable[1] = sliderWrapper;
			obj.classButton.nextSelectable[1] = sliderWrapper;
			obj.pupButton.nextSelectable[1] = sliderWrapper;
		}
		UpdatePlayerSlideSelectable(menu.manager.rainWorld.options.JollyPlayerCount - 1);
		manualButton.nextSelectable[0] = manualButton;
		manualButton.nextSelectable[1] = manualButton;
		manualButton.nextSelectable[2] = manualButton;
		manualButton.nextSelectable[3] = menu.cancelButton;
		menu.cancelButton.nextSelectable[0] = menu.cancelButton;
		menu.cancelButton.nextSelectable[1] = manualButton;
		menu.cancelButton.nextSelectable[2] = menu.cancelButton;
		numberPlayersSlider.wrapper.nextSelectable[1] = menu.cancelButton;
		colorModeButtons[0].nextSelectable[0] = colorModeButtons[0];
		colorModeButtons[colorModeButtons.Length - 1].nextSelectable[2] = colorInfoSymbol;
		cameraSwitchQuickness[0].nextSelectable[0] = cameraSwitchQuickness[0];
		cameraSwitchQuickness[cameraSwitchQuickness.Length - 1].nextSelectable[2] = cameraInfoSymbol;
		cameraInfoSymbol.nextSelectable[2] = friendlyLizardsToggle;
		difficultyButtons[0].nextSelectable[0] = difficultyButtons[0];
		difficultyButtons[0].nextSelectable[1] = cameraSwitchQuickness[0];
		difficultyButtons[difficultyButtons.Length - 1].nextSelectable[2] = difficultyInfoSymbol;
		difficultyInfoSymbol.nextSelectable[3] = difficultyInfoSymbol;
		for (int j = 0; j < difficultyButtons.Length; j++)
		{
			colorModeButtons[j].nextSelectable[3] = cameraSwitchQuickness[j];
			cameraSwitchQuickness[j].nextSelectable[1] = colorModeButtons[j];
			cameraSwitchQuickness[j].nextSelectable[3] = difficultyButtons[j];
			difficultyButtons[j].nextSelectable[1] = cameraSwitchQuickness[j];
			difficultyButtons[j].nextSelectable[3] = difficultyButtons[j];
		}
		friendlyToggle.nextSelectable[2] = friendlyToggle;
		cameraCyclesToggle.nextSelectable[2] = cameraCyclesToggle;
		cameraCyclesToggle.nextSelectable[3] = cameraCyclesToggle;
		smartShortcutToggle.nextSelectable[3] = smartShortcutToggle;
		smartShortcutToggle.nextSelectable[0] = friendlySteal;
		hudToggle.nextSelectable[0] = friendlyLizardsToggle;
		friendlySteal.nextSelectable[3] = friendlySteal;
		controlsButton.nextSelectable[0] = controlsButton;
		controlsButton.nextSelectable[1] = controlsButton;
		controlsButton.nextSelectable[2] = menu.cancelButton;
		menu.cancelButton.nextSelectable[0] = controlsButton;
		controlsButton.nextSelectable[3] = sliderWrapper;
		sliderWrapper.nextSelectable[0] = controlsButton;
		sliderWrapper.nextSelectable[2] = menu.cancelButton;
		menu.cancelButton.nextSelectable[3] = sliderWrapper;
		menu.cancelButton.nextSelectable[2] = menu.cancelButton;
	}

	private void AddLabelToSelectOne(SelectOneButton[] buttonArray, string labelString, string description)
	{
		if (buttonArray != null)
		{
			for (int i = 0; i < buttonArray.Length; i++)
			{
				if (i == 0)
				{
					buttonArray[i].nextSelectable[0] = buttonArray[i];
				}
				else
				{
					buttonArray[i].nextSelectable[0] = buttonArray[i - 1];
				}
			}
			menu.elementDescription.Add(buttonArray[0].signalText, description);
		}
		subObjects.Add(new MenuLabel(menu, this, labelString, buttonArray[0].pos - new Vector2(25f + buttonArray[0].size.x, 0f), new Vector2(120f, 30f), bigText: false));
	}

	public SymbolButton AddSymbolToSelectOne(SelectOneButton[] buttonArray, string symbolSignal)
	{
		SymbolButton symbolButton = new SymbolButton(menu, this, "Menu_InfoI", symbolSignal, buttonArray[buttonArray.Length - 1].pos + new Vector2(buttonArray[buttonArray.Length - 1].size.x + 25f, 3f));
		subObjects.Add(symbolButton);
		menu.elementDescription.Add(symbolSignal, menu.Translate(symbolSignal + "_DESCRIPTION"));
		return symbolButton;
	}

	private void AddSelectOneButton(SelectOneButton[] buttonArray, string enumValue, string displayString, int i, Vector2 pos, float buttonWidth)
	{
		buttonArray[i] = new SelectOneButton(menu, this, enumValue, displayString, pos, new Vector2(buttonWidth, 30f), buttonArray, i);
		subObjects.Add(buttonArray[i]);
		if (i < buttonArray.Length - 1)
		{
			buttonArray[i].nextSelectable[2] = buttonArray[i + 1];
		}
		if (i != 0)
		{
			buttonArray[i].nextSelectable[0] = buttonArray[i - 1];
		}
	}

	private void AddJollyTitle()
	{
		MenuIllustration item = new MenuIllustration(menu, this, "", "jolly_title_shadow", new Vector2(688f, Custom.rainWorld.screenSize.y - 75f - 32f), crispPixels: true, anchorCenter: true);
		MenuIllustration menuIllustration = new MenuIllustration(menu, this, "", "jolly_title", new Vector2(688f, Custom.rainWorld.screenSize.y - 75f - 32f), crispPixels: true, anchorCenter: true);
		menuIllustration.sprite.shader = menu.manager.rainWorld.Shaders["MenuText"];
		menuIllustration.sprite.color = new Color(0f, 1f, 1f);
		subObjects.Add(item);
		subObjects.Add(menuIllustration);
	}

	public SymbolButtonToggle AddSymbolToggleButton(bool toggled, string signal, string descriptionOn, string descriptionOff, Vector2 pos, Vector2 size, string symbolName, string labelOn, string labelOff, bool textAboveButton, FTextParams textParams = null)
	{
		SymbolButtonToggle symbolButtonToggle = new SymbolButtonToggle(menu, this, signal, pos, size, symbolName + "_on", symbolName + "_off", toggled, textAboveButton, labelOn, labelOff, textParams);
		menu.elementDescription.Add(signal + "_on", descriptionOn);
		menu.elementDescription.Add(signal + "_off", descriptionOff);
		subObjects.Add(symbolButtonToggle);
		return symbolButtonToggle;
	}

	private void NumberPlayersChange(UIconfig config, string value, string oldvalue)
	{
		if (int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
		{
			if (result > 4 || result < 1)
			{
				return;
			}
			for (int i = 0; i < Options.jollyPlayerOptionsArray.Length; i++)
			{
				Options.jollyPlayerOptionsArray[i].joined = i <= result - 1;
			}
		}
		UpdatePlayerSlideSelectable(result - 1);
	}

	public void UpdatePlayerSlideSelectable(int pIndex)
	{
		sliderWrapper.nextSelectable[3] = playerSelector[pIndex].pLabelSelectorWrapper;
		if (Options.JollyPlayerCount <= 2)
		{
			sliderWrapper.nextSelectable[1] = controlsButton;
			sliderWrapper.nextSelectable[0] = controlsButton;
		}
		if (Options.JollyPlayerCount > 2)
		{
			sliderWrapper.nextSelectable[1] = menu.cancelButton;
			sliderWrapper.nextSelectable[0] = menu.cancelButton;
		}
	}

	private JollyPlayerOptions JollyOptions(int index)
	{
		return Options.jollyPlayerOptionsArray[index];
	}

	public SlugcatStats.Name NextClass(SlugcatStats.Name curClass)
	{
		if (ModManager.Expedition && menu.manager.rainWorld.ExpeditionMode)
		{
			int num = ExpeditionGame.unlockedExpeditionSlugcats.IndexOf(curClass) + 1;
			if (num > ExpeditionGame.unlockedExpeditionSlugcats.Count - 1)
			{
				return ExpeditionGame.unlockedExpeditionSlugcats[0];
			}
			return ExpeditionGame.unlockedExpeditionSlugcats[num];
		}
		if (curClass != null)
		{
			JollyCustom.Log("Current class: " + curClass);
		}
		else
		{
			JollyCustom.Log("Current class is null!");
		}
		SlugcatStats.Name name;
		if (curClass == null)
		{
			name = new SlugcatStats.Name(ExtEnum<SlugcatStats.Name>.values.GetEntry(0));
		}
		else
		{
			if (curClass.Index >= ExtEnum<SlugcatStats.Name>.values.Count - 1 || curClass.Index == -1)
			{
				return NextClass(null);
			}
			name = new SlugcatStats.Name(ExtEnum<SlugcatStats.Name>.values.GetEntry(curClass.Index + 1));
		}
		if (SlugcatStats.HiddenOrUnplayableSlugcat(name))
		{
			return NextClass(name);
		}
		if (!SlugcatStats.SlugcatUnlocked(name, menu.manager.rainWorld))
		{
			return NextClass(name);
		}
		JollyCustom.Log("Next class: " + name);
		return name;
	}

	public override void Singal(MenuObject sender, string message)
	{
		base.Singal(sender, message);
		JollyCustom.Log("Message received " + message);
		menu.PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
		if (message.Contains("CLASSCHANGE"))
		{
			for (int i = 0; i < playerSelector.Length; i++)
			{
				if (message == "CLASSCHANGE" + i)
				{
					JollyCustom.Log("Changing class for player " + i);
					for (int j = 0; j < ExpeditionGame.unlockedExpeditionSlugcats.Count; j++)
					{
						ExpLog.Log(ExpeditionGame.unlockedExpeditionSlugcats[j].value);
					}
					SlugcatStats.Name name = NextClass(playerSelector[i].slugName);
					if (name == null)
					{
						name = menu.currentSlugcatPageName;
					}
					JollyOptions(i).playerClass = name;
					menu.PlaySound(SoundID.MENU_Error_Ping);
					if (menu.Options.jollyColorMode != Options.JollyColorMode.AUTO)
					{
						playerSelector[i].dirty = true;
					}
					else
					{
						SetPortraitsDirty();
					}
					break;
				}
			}
		}
		else if (message.Contains("toggle_pup"))
		{
			bool isPup = false;
			if (message.Contains("on"))
			{
				isPup = true;
				message = message.Replace("_on", "");
			}
			else
			{
				message = message.Replace("_off", "");
			}
			if (int.TryParse(char.ToString(message[message.Length - 1]), NumberStyles.Any, CultureInfo.InvariantCulture, out var result) && result < playerSelector.Length)
			{
				JollyOptions(result).isPup = isPup;
			}
			else
			{
				JollyCustom.Log("Error parsing signal string: " + message, throwException: true);
			}
		}
		else if (message.Contains("friendly_fire"))
		{
			Options.friendlyFire = message.Contains("on");
		}
		else if (message.Contains("hud"))
		{
			Options.jollyHud = message.Contains("on");
		}
		else if (message.Contains("smartpipe"))
		{
			Options.smartShortcuts = message.Contains("on");
		}
		else if (message.Contains("camera_cycle"))
		{
			Options.cameraCycling = message.Contains("on");
		}
		else if (message.Contains("friendlylizard"))
		{
			Options.friendlyLizards = message.Contains("on");
		}
		else if (message.Contains("friendlysteal"))
		{
			Options.friendlySteal = message.Contains("on");
		}
	}

	public override void RemoveSprites()
	{
		base.RemoveSprites();
		if (playerSelector != null)
		{
			for (int i = 0; i < playerSelector.Length; i++)
			{
				playerSelector[i].RemoveSprites();
			}
		}
	}

	public int GetCurrentlySelectedOfSeries(string series)
	{
		return series switch
		{
			"Difficulty" => (int)Options.jollyDifficulty, 
			"CameraInput" => (int)Options.jollyCameraInputSpeed, 
			"ColorMode" => (int)Options.jollyColorMode, 
			_ => -1, 
		};
	}

	public void SetCurrentlySelectedOfSeries(string series, int to)
	{
		switch (series)
		{
		case "Difficulty":
			if (Options.jollyDifficulty.Index != to)
			{
				Options.jollyDifficulty = new Options.JollyDifficulty(ExtEnum<Options.JollyDifficulty>.values.GetEntry(to));
			}
			break;
		case "CameraInput":
			if (Options.jollyCameraInputSpeed.Index != to)
			{
				Options.jollyCameraInputSpeed = new Options.JollyCameraInputSpeed(ExtEnum<Options.JollyCameraInputSpeed>.values.GetEntry(to));
			}
			break;
		case "ColorMode":
			if (Options.jollyColorMode.Index != to)
			{
				Options.jollyColorMode = new Options.JollyColorMode(ExtEnum<Options.JollyColorMode>.values.GetEntry(to));
				SetPortraitsDirty();
			}
			break;
		}
	}

	public void SetPortraitsDirty()
	{
		PlayerGraphics.jollyColors = null;
		JollyPlayerSelector[] array = playerSelector;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].dirty = true;
		}
	}
}
