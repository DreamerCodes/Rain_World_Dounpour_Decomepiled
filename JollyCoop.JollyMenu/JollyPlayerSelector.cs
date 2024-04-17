using System;
using System.Text.RegularExpressions;
using Menu;
using Menu.Remix;
using Menu.Remix.MixedUI;
using RWCustom;
using UnityEngine;

namespace JollyCoop.JollyMenu;

public class JollyPlayerSelector : PositionedMenuObject
{
	public int index;

	private JollySetupDialog dialog;

	public Color bodyTintColor;

	public Color faceTintColor;

	public Color uniqueTintColor;

	private bool portraitTint;

	public SlugcatStats.Name slugName;

	public RoundedRect portraitRectangle;

	public SimpleButton classButton;

	public MenuIllustration portrait;

	public SymbolButtonTogglePupButton pupButton;

	public OpTextBox playerLabelSelector;

	private readonly string DefaultPlayerLabel;

	public UIelementWrapper pLabelSelectorWrapper;

	public MenuLabel playerLabelConsole;

	public SimpleButton colorConfig;

	public bool dirty;

	private float labelFade;

	public int labelFadeCounter;

	private float portraitBlack = 1f;

	private float lastPortraitBlack = 1f;

	public const string COLOR_BUTTON = "JOLLYCOLORDIALOG";

	public bool Joined
	{
		get
		{
			if (index == 0)
			{
				return true;
			}
			return JollyOptions(index).joined;
		}
		set
		{
			if (index != 0)
			{
				JollyCustom.Log("Player " + index + " joining?..." + value);
				JollyOptions(index).joined = value;
			}
		}
	}

	public JollyPlayerSelector(JollySetupDialog menu, MenuObject owner, Vector2 pos, int index)
		: base(menu, owner, pos)
	{
		Vector2 vector = new Vector2(100f, 100f);
		dialog = menu;
		this.index = index;
		portrait = new MenuIllustration(menu, this, "", "MultiplayerPortrait" + index + "1", vector / 2f, crispPixels: true, anchorCenter: true);
		subObjects.Add(portrait);
		bodyTintColor = Color.white;
		portraitRectangle = new RoundedRect(menu, this, new Vector2(0f, 0f), new Vector2(100f, 100f), filled: false);
		subObjects.Add(portraitRectangle);
		float x = 100f;
		if (menu.CurrLang == InGameTranslator.LanguageID.German)
		{
			x = 150f;
		}
		classButton = new SimpleButton(menu, this, menu.Translate("Player"), "CLASSCHANGE" + index, new Vector2(0f, -35.5f), new Vector2(x, 30f));
		subObjects.Add(classButton);
		string s = "PLAYER_CLASS_INSTRUCTION";
		menu.elementDescription.Add(classButton.signalText, menu.Translate(s).Replace("<p_n>", (index + 1).ToString()));
		DefaultPlayerLabel = menu.Translate("Player <p_n>").Replace("<p_n>", (index + 1).ToString());
		playerLabelSelector = new OpTextBox(menu.oi.config.Bind("_playerName" + index, DefaultPlayerLabel), pos + new Vector2(0f, 105f), 100f);
		playerLabelSelector.alignment = FLabelAlignment.Center;
		playerLabelSelector.allowSpace = true;
		playerLabelSelector.description = menu.Translate("PLAYER_CUSTOM_NAME_DESCRIPTION").Replace("<p_n>", (index + 1).ToString());
		playerLabelSelector.OnHeld += OnPlayerLabelSelectorHeld;
		pLabelSelectorWrapper = new UIelementWrapper(menu.tabWrapper, playerLabelSelector);
		playerLabelSelector.value = JollyCustom.GetPlayerName(index);
		JollyCustom.Log("Player name: " + playerLabelSelector.value);
		playerLabelSelector.OnValueUpdate += PlayerLabel_OnValueUpdate;
		pupButton = new SymbolButtonTogglePupButton(menu, this, "toggle_pup_" + index, new Vector2(classButton.size.x + 10f, -35.5f), new Vector2(45f, 45f), "pup_on", GetPupButtonOffName(), JollyOptions(index).isPup);
		subObjects.Add(pupButton);
		menu.elementDescription.Add($"toggle_pup_{index}_on", menu.Translate("description_pup_off").Replace("<p_n>", (index + 1).ToString()));
		menu.elementDescription.Add($"toggle_pup_{index}_off", menu.Translate("description_pup_on").Replace("<p_n>", (index + 1).ToString()));
		dirty = true;
	}

	private void OnPlayerLabelSelectorHeld(bool held)
	{
		if (held)
		{
			if (!Regex.IsMatch(playerLabelSelector.value, "^[ -~/s]+$"))
			{
				playerLabelSelector.value = "";
			}
		}
		else if (string.IsNullOrEmpty(playerLabelSelector.value))
		{
			playerLabelSelector.value = DefaultPlayerLabel;
		}
	}

	public JollyPlayerOptions JollyOptions(int index)
	{
		return dialog.JollyOptions(index);
	}

	private void PlayerLabel_OnValueUpdate(UIconfig config, string value, string oldValue)
	{
		playerLabelSelector.colorFill = (string.IsNullOrEmpty(value) ? MenuColorEffect.rgbDarkGrey : MenuColorEffect.rgbBlack);
		JollyOptions(index).customPlayerName = value;
		JollyCustom.Log($"Saved name for [{index}]: {value}");
	}

	public override void Update()
	{
		base.Update();
		if (Joined)
		{
			labelFade = Custom.LerpAndTick(labelFade, 0f, 0.12f, 0.1f);
			labelFadeCounter = ((labelFade == 0f) ? 40 : 0);
		}
		else
		{
			if (labelFadeCounter > 0 && !Selected)
			{
				labelFadeCounter--;
			}
			if (labelFadeCounter < 1)
			{
				labelFade = Custom.LerpAndTick(labelFade, 1f, 0.02f, 1f / 60f);
			}
		}
		lastPortraitBlack = portraitBlack;
		portraitBlack = Custom.LerpAndTick(portraitBlack, Joined ? 0f : 1f, 0.06f, 0.05f);
		SlugcatStats.Name name = JollyCustom.SlugClassMenu(index, dialog.currentSlugcatPageName);
		portraitTint = dialog.Options.jollyColorMode == Options.JollyColorMode.CUSTOM || (dialog.Options.jollyColorMode == Options.JollyColorMode.AUTO && index != 0);
		if (dirty)
		{
			bodyTintColor = PlayerGraphics.JollyBodyColorMenu(new SlugcatStats.Name("JollyPlayer" + (index + 1)), JollyOptions(0).playerClass);
			bodyTintColor = JollyCustom.ColorClamp(bodyTintColor, 0.01f, 360f, -1f, 360f, 0.25f);
			faceTintColor = PlayerGraphics.JollyFaceColorMenu(name, JollyOptions(0).playerClass, index);
			uniqueTintColor = PlayerGraphics.JollyUniqueColorMenu(name, JollyOptions(0).playerClass, index);
			JollyCustom.Log("Portrait " + index + " was dirty, refreshing...");
			classButton.menuLabel.text = menu.Translate("The " + SlugcatStats.getSlugcatName(name));
			if (portraitTint)
			{
				SetPortraitImage(name, bodyTintColor);
				JollyCustom.Log("Loading portrait with tint");
			}
			else
			{
				SetPortraitImage(name);
				JollyCustom.Log("Loading portrait without tint");
			}
			pupButton.symbolNameOff = GetPupButtonOffName();
			pupButton.faceSymbol.sprite.color = faceTintColor;
			if (pupButton.uniqueSymbol != null)
			{
				pupButton.uniqueSymbol.sprite.color = uniqueTintColor;
			}
			pupButton.LoadIcon();
		}
		classButton.GetButtonBehavior.greyedOut = !Joined;
		if (index == 0 && ModManager.Expedition && menu.manager.rainWorld.ExpeditionMode)
		{
			classButton.GetButtonBehavior.greyedOut = true;
		}
		pupButton.GetButtonBehavior.greyedOut = !Joined;
		if (colorConfig != null)
		{
			colorConfig.GetButtonBehavior.greyedOut = !Joined;
		}
		playerLabelSelector.greyedOut = !dialog.Options.jollyHud || !Joined;
		if (dialog.Options.jollyColorMode == Options.JollyColorMode.CUSTOM)
		{
			if (colorConfig == null)
			{
				AddColorButton();
			}
		}
		else if (colorConfig != null)
		{
			RemoveColorButton();
		}
	}

	public string GetPupButtonOffName()
	{
		string text = "pup_off";
		SlugcatStats.Name playerClass = JollyOptions(index).playerClass;
		if (playerClass != null && playerClass.value.Equals("Gourmand"))
		{
			text = "gourmand_" + text;
		}
		else if (playerClass != null && playerClass.value.Equals("Saint"))
		{
			text = "saint_" + text;
		}
		else if (playerClass != null && playerClass.value.Equals("Spear"))
		{
			text = "spear_" + text;
		}
		else if (playerClass != null && playerClass.value.Equals("Rivulet"))
		{
			text = "rivulet_" + text;
		}
		else if (playerClass != null && playerClass.value.Equals("Artificer"))
		{
			text = "artificer_" + text;
		}
		return text;
	}

	public string JollyPortraitName(SlugcatStats.Name className, int colorIndexFile)
	{
		return "MultiplayerPortrait" + colorIndexFile + "1-" + className.ToString();
	}

	public void SetPortraitImage(SlugcatStats.Name className, Color colorTint)
	{
		SetPortraitImage(className, 0);
		portrait.sprite.color = colorTint;
		Color color = colorTint;
		JollyCustom.Log("Tinting portrait... " + color.ToString());
	}

	public void SetPortraitImage(SlugcatStats.Name className, int colorIndexFile = -1)
	{
		if (colorIndexFile < 0)
		{
			colorIndexFile = dialog.GetFileIndex(className);
			portrait.sprite.color = Color.white;
		}
		JollyCustom.Log($"[{index}] Setting portrait, className {className},  colorIndexFile {colorIndexFile}, old fileName [{portrait.fileName}], new fileName [{JollyPortraitName(className, colorIndexFile)}]");
		try
		{
			portrait.fileName = JollyPortraitName(className, colorIndexFile);
			portrait.LoadFile();
			dirty = false;
		}
		catch (Exception ex)
		{
			JollyCustom.Log("Error when loading icon for Jolly config! " + ex, throwException: true);
			portrait.fileName = "multiplayerportrait02";
			portrait.LoadFile();
		}
		slugName = className;
		portrait.sprite.SetElementByName(portrait.fileName);
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		Color color = FadePortraitSprite(bodyTintColor, timeStacker);
		Color color2 = FadePortraitSprite(Color.white, timeStacker);
		Color color3 = FadePortraitSprite(faceTintColor, timeStacker);
		Color color4 = FadePortraitSprite(uniqueTintColor, timeStacker);
		pupButton.symbol.sprite.color = color;
		pupButton.faceSymbol.sprite.color = color3;
		if (pupButton.uniqueSymbol != null)
		{
			pupButton.uniqueSymbol.sprite.color = color4;
		}
		portrait.sprite.color = (portraitTint ? color : color2);
		for (int i = 0; i < portraitRectangle.sprites.Length; i++)
		{
			portraitRectangle.sprites[i].color = pupButton.MyColor(timeStacker);
		}
	}

	public Color FadePortraitSprite(Color toFade, float timeStacker)
	{
		return Color.Lerp(toFade, Color.black, Custom.SCurve(Mathf.Lerp(lastPortraitBlack, portraitBlack, timeStacker), 0.5f) * 0.75f);
	}

	public void AddColorButton()
	{
		colorConfig = new SimpleButton(menu, this, menu.Translate("COLOR_BUTTON"), "JOLLYCOLORDIALOG" + index, new Vector2(0f, -71f), new Vector2(100f, 30f));
		subObjects.Add(colorConfig);
	}

	public void RemoveColorButton()
	{
		RemoveSubObject(colorConfig);
		colorConfig.RemoveSprites();
		colorConfig = null;
	}
}
