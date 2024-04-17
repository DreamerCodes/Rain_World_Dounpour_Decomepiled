using System;
using Rewired;
using RWCustom;
using UnityEngine;

namespace Menu;

public class PlayerJoinButton : ButtonTemplate
{
	public MenuLabel menuLabel;

	public RoundedRect roundedRect;

	public RoundedRect selectRect;

	public bool joystickAvailable;

	public Joystick joystickPressed;

	public int assignedJoystick;

	public int index;

	public MenuIllustration portrait;

	private float labelFade;

	private float lastLabelFade;

	public int labelFadeCounter;

	private float portraitBlack = 1f;

	private float lastPortraitBlack = 1f;

	private bool lastInput;

	private MenuIllustration joinButtonImage;

	private bool Joined => (menu as MultiplayerMenu).GetArenaSetup.playersJoined[index];

	public override Color MyColor(float timeStacker)
	{
		if (buttonBehav.greyedOut)
		{
			return HSLColor.Lerp(Menu.MenuColor(Menu.MenuColors.DarkGrey), Menu.MenuColor(Menu.MenuColors.Black), black).rgb;
		}
		float a = Mathf.Lerp(buttonBehav.lastCol, buttonBehav.col, timeStacker);
		a = Mathf.Max(a, Mathf.Lerp(buttonBehav.lastFlash, buttonBehav.flash, timeStacker));
		HSLColor from = HSLColor.Lerp(HSLColor.Lerp(Menu.MenuColor(Menu.MenuColors.MediumGrey), Menu.MenuColor(Menu.MenuColors.DarkGrey), Mathf.Lerp(lastPortraitBlack, portraitBlack, timeStacker)), Menu.MenuColor(Menu.MenuColors.White), a);
		return HSLColor.Lerp(from, Menu.MenuColor(Menu.MenuColors.Black), black).rgb;
	}

	public PlayerJoinButton(Menu menu, MenuObject owner, Vector2 pos, int index)
		: base(menu, owner, pos, new Vector2(100f, 100f))
	{
		this.index = index;
		roundedRect = new RoundedRect(menu, this, new Vector2(0f, 0f), size, filled: true);
		subObjects.Add(roundedRect);
		selectRect = new RoundedRect(menu, this, new Vector2(0f, 0f), size, filled: false);
		subObjects.Add(selectRect);
		portrait = new MenuIllustration(menu, this, "", "MultiplayerPortrait" + index + "1", size / 2f, crispPixels: true, anchorCenter: true);
		subObjects.Add(portrait);
		string text = menu.Translate("Press to join");
		if (menu.CurrLang != InGameTranslator.LanguageID.English && menu.CurrLang != InGameTranslator.LanguageID.Spanish)
		{
			text = InGameTranslator.EvenSplit(text, 1);
		}
		float num = 0f;
		menuLabel = new MenuLabel(menu, this, menu.Translate("PLAYER") + (InGameTranslator.LanguageID.UsesSpaces(menu.CurrLang) ? " " : "") + (index + 1) + "\r\n" + text, new Vector2(0.01f, 0.1f + num), size, bigText: false);
		subObjects.Add(menuLabel);
		string text2 = "";
		if (text2 != "")
		{
			joinButtonImage = new MenuIllustration(menu, this, "", text2, new Vector2(size.x * 0.5f - 15f, size.y * 0.5f - 2.5f - menuLabel.label.textRect.height), crispPixels: false, anchorCenter: false);
			subObjects.Add(joinButtonImage);
		}
	}

	public override void Update()
	{
		base.Update();
		buttonBehav.Update();
		roundedRect.fillAlpha = Mathf.Lerp(0.3f, 0.6f, buttonBehav.col);
		roundedRect.addSize = new Vector2(10f, 6f) * (buttonBehav.sizeBump + 0.5f * Mathf.Sin(buttonBehav.extraSizeBump * (float)Math.PI)) * (buttonBehav.clicked ? 0f : 1f);
		selectRect.addSize = new Vector2(2f, -2f) * (buttonBehav.sizeBump + 0.5f * Mathf.Sin(buttonBehav.extraSizeBump * (float)Math.PI)) * (buttonBehav.clicked ? 0f : 1f);
		lastLabelFade = labelFade;
		if (Joined)
		{
			labelFade = Custom.LerpAndTick(labelFade, 0f, 0.12f, 0.1f);
			labelFadeCounter = ((labelFade == 0f) ? 40 : 0);
		}
		else
		{
			joystickPressed = null;
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
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		menuLabel.label.alpha = Custom.SCurve(Mathf.Lerp(lastLabelFade, labelFade, timeStacker), 0.3f);
		Color color = Color.Lerp(Menu.MenuRGB(Menu.MenuColors.Black), Menu.MenuRGB(Menu.MenuColors.White), Mathf.Lerp(buttonBehav.lastFlash, buttonBehav.flash, timeStacker));
		for (int i = 0; i < 9; i++)
		{
			roundedRect.sprites[i].color = color;
		}
		float num = 0.5f + 0.5f * Mathf.Sin(Mathf.Lerp(buttonBehav.lastSin, buttonBehav.sin, timeStacker) / 30f * (float)Math.PI * 2f);
		num *= buttonBehav.sizeBump;
		for (int j = 0; j < 8; j++)
		{
			selectRect.sprites[j].color = MyColor(timeStacker);
			selectRect.sprites[j].alpha = num;
		}
		if (index == 3)
		{
			menuLabel.label.color = Color.Lerp(Color.Lerp(Custom.Saturate(PlayerGraphics.DefaultSlugcatColor(SlugcatStats.Name.ArenaColor(index)), 0.5f), Color.white, 0.2f), MyColor(timeStacker), num);
		}
		else
		{
			menuLabel.label.color = Color.Lerp(PlayerGraphics.DefaultSlugcatColor(SlugcatStats.Name.ArenaColor(index)), MyColor(timeStacker), num);
		}
		portrait.sprite.color = Color.Lerp(Color.white, Color.black, Custom.SCurve(Mathf.Lerp(lastPortraitBlack, portraitBlack, timeStacker), 0.5f) * 0.75f);
	}

	public override void Clicked()
	{
		ClickedGeneral();
	}

	private void ClickedNintendoSwitch()
	{
		if (index == 0)
		{
			if (Joined)
			{
				(menu as MultiplayerMenu).GetArenaSetup.playersJoined[index] = false;
				menu.PlaySound(SoundID.MENU_Player_Unjoin_Game);
			}
			else
			{
				(menu as MultiplayerMenu).GetArenaSetup.playersJoined[index] = true;
				menu.PlaySound(SoundID.MENU_Player_Join_Game);
			}
		}
	}

	private void ClickedGeneral()
	{
		if (Joined)
		{
			if (index == 0)
			{
				(menu as MultiplayerMenu).GetArenaSetup.playersJoined[index] = false;
				menu.PlaySound(SoundID.MENU_Player_Unjoin_Game);
			}
			else if (!(menu as MultiplayerMenu).manager.rainWorld.GetPlayerSigningIn(index))
			{
				(menu as MultiplayerMenu).manager.rainWorld.DeactivatePlayer(index);
				menu.PlaySound(SoundID.MENU_Player_Unjoin_Game);
			}
		}
		else
		{
			if (index == 0)
			{
				(menu as MultiplayerMenu).GetArenaSetup.playersJoined[index] = true;
			}
			else
			{
				(menu as MultiplayerMenu).manager.rainWorld.RequestPlayerSignIn(index, joystickPressed);
			}
			menu.PlaySound(SoundID.MENU_Player_Join_Game);
		}
		joystickPressed = null;
	}
}
