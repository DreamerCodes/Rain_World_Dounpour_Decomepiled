using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using RWCustom;
using UnityEngine;

namespace Menu;

public class HoldButton : CircularMenuObject, SelectableMenuObject, ButtonMenuObject
{
	public MenuLabel menuLabel;

	public ButtonBehavior buttonBehav;

	public FSprite[] circleSprites;

	public float filled;

	public float lastFilled;

	public float fillTime;

	private bool hasSignalled;

	public int buttonReleasedCounter;

	private float pulse;

	private float lastPulse;

	public string signalText;

	public bool controlledFromOutside;

	public bool held;

	public bool lastHeld;

	public MenuMicrophone.MenuSoundLoop soundLoop;

	public bool warningMode;

	public bool FillingUp => filled > 0f;

	public bool IsMouseOverMe => base.MouseOver;

	public bool CurrentlySelectableMouse
	{
		get
		{
			if (!buttonBehav.greyedOut)
			{
				return !hasSignalled;
			}
			return false;
		}
	}

	public bool CurrentlySelectableNonMouse => true;

	public ButtonBehavior GetButtonBehavior => buttonBehav;

	public Color MyColor(float timeStacker)
	{
		if (buttonBehav.greyedOut)
		{
			return Menu.MenuColor(Menu.MenuColors.DarkGrey).rgb;
		}
		if (warningMode && FillingUp)
		{
			return new Color(1f, 0.5f + Mathf.Sin(filled * 50f + (float)Math.PI / 2f) * 0.5f, 0.5f + Mathf.Sin(filled * 50f + (float)Math.PI / 2f) * 0.5f);
		}
		float lrp = Mathf.Lerp(buttonBehav.lastCol, buttonBehav.col, timeStacker);
		return HSLColor.Lerp(Menu.MenuColor(Menu.MenuColors.MediumGrey), Menu.MenuColor(Menu.MenuColors.White), lrp).rgb;
	}

	public HoldButton(Menu menu, MenuObject owner, string displayText, string singalText, Vector2 pos, float fillTime)
		: base(menu, owner, pos, 55f)
	{
		this.fillTime = fillTime;
		signalText = singalText;
		buttonBehav = new ButtonBehavior(this);
		base.page.selectables.Add(this);
		circleSprites = new FSprite[5];
		circleSprites[0] = new FSprite("Futile_White");
		circleSprites[0].shader = menu.manager.rainWorld.Shaders["VectorCircleFadable"];
		circleSprites[1] = new FSprite("Futile_White");
		circleSprites[1].shader = menu.manager.rainWorld.Shaders["VectorCircle"];
		circleSprites[2] = new FSprite("Futile_White");
		circleSprites[2].shader = menu.manager.rainWorld.Shaders["HoldButtonCircle"];
		circleSprites[3] = new FSprite("Futile_White");
		circleSprites[3].shader = menu.manager.rainWorld.Shaders["VectorCircle"];
		circleSprites[4] = new FSprite("Futile_White");
		circleSprites[4].shader = menu.manager.rainWorld.Shaders["VectorCircleFadable"];
		for (int i = 0; i < circleSprites.Length; i++)
		{
			Container.AddChild(circleSprites[i]);
		}
		switch (displayText)
		{
		case "APAGAR PROGRESSO":
			displayText = Regex.Replace(displayText, " ", "\r\n");
			break;
		case "進捗のリセット":
			displayText = "進捗\r\nのリセット";
			break;
		case "СБРОСИТЬ ПРОГРЕСС":
			displayText = "СБРОСИТЬ\r\nПРОГРЕСС";
			break;
		default:
		{
			List<int> list = new List<int>();
			int num = 0;
			for (int j = 0; j < displayText.Length; j++)
			{
				num++;
				if (displayText[j] == ' ' && num > 9)
				{
					num = 0;
					list.Add(j);
				}
			}
			int num2 = 0;
			foreach (int item in list)
			{
				int length = displayText.Length;
				int num3 = item + num2;
				displayText = displayText.Substring(0, num3) + "\r\n" + displayText.Substring(num3 + 1, displayText.Length - (num3 + 1));
				num2 += displayText.Length - length;
			}
			break;
		}
		}
		menuLabel = new MenuLabel(menu, this, displayText, new Vector2(-50f, -15f), new Vector2(100f, 30f), bigText: false);
		subObjects.Add(menuLabel);
	}

	public override void Update()
	{
		base.Update();
		buttonBehav.Update();
		lastHeld = held;
		if (held)
		{
			if (soundLoop == null)
			{
				soundLoop = menu.PlayLoop(SoundID.MENU_Security_Button_LOOP, 0f, 0f, 1f, isBkgLoop: false);
			}
			soundLoop.loopVolume = Mathf.Lerp(soundLoop.loopVolume, 1f, 0.85f);
			soundLoop.loopPitch = Mathf.Lerp(0.3f, 1.5f, filled) - 0.15f * Mathf.Sin(pulse * (float)Math.PI * 2f);
		}
		else if (!held && soundLoop != null)
		{
			soundLoop.loopVolume = Mathf.Max(0f, soundLoop.loopVolume - 0.125f);
			if (soundLoop.loopVolume <= 0f)
			{
				soundLoop.Destroy();
				soundLoop = null;
			}
		}
		if (controlledFromOutside ? held : buttonBehav.clicked)
		{
			lastPulse = pulse;
			pulse += filled / 20f;
		}
		else
		{
			pulse = 0f;
			lastPulse = 0f;
		}
		lastFilled = filled;
		if (!controlledFromOutside)
		{
			held = Selected && !buttonBehav.greyedOut && !hasSignalled && menu.holdButton;
		}
		else
		{
			buttonBehav.sizeBump = (held ? 1f : 0f);
		}
		if (held)
		{
			buttonBehav.sin = pulse;
			if (warningMode)
			{
				filled = Custom.LerpAndTick(filled, 1f, 0.007f, 1f / (fillTime * 4f));
			}
			else
			{
				filled = Custom.LerpAndTick(filled, 1f, 0.007f, 1f / fillTime);
			}
			if (filled >= 1f && !hasSignalled)
			{
				Singal(this, signalText);
				hasSignalled = true;
				menu.ResetSelection();
			}
			buttonReleasedCounter = 0;
			if (!lastHeld)
			{
				menu.PlaySound(SoundID.MENU_Security_Button_Init);
			}
			return;
		}
		if (lastHeld && !hasSignalled)
		{
			menu.PlaySound(SoundID.MENU_Security_Button_Release);
		}
		if (hasSignalled)
		{
			buttonReleasedCounter++;
			if (buttonReleasedCounter > 30)
			{
				filled = Custom.LerpAndTick(filled, 0f, 0.04f, 0.025f);
				if (filled < 0.5f)
				{
					hasSignalled = false;
				}
			}
			else
			{
				filled = 1f;
			}
		}
		else
		{
			filled = Custom.LerpAndTick(filled, 0f, 0.04f, 0.025f);
		}
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		menuLabel.label.color = MyColor(timeStacker);
		float num = Mathf.Lerp(lastRad, rad, timeStacker) + 8f * (buttonBehav.sizeBump + 0.5f * Mathf.Sin(buttonBehav.extraSizeBump * (float)Math.PI)) * ((controlledFromOutside ? held : buttonBehav.clicked) ? (0.5f + 0.5f * Mathf.Sin(Mathf.Lerp(lastPulse, pulse, timeStacker) * (float)Math.PI * 2f)) : 1f);
		Vector2 vector = DrawPos(timeStacker);
		num += 0.5f;
		for (int i = 0; i < circleSprites.Length; i++)
		{
			circleSprites[i].x = vector.x;
			circleSprites[i].y = vector.y;
			circleSprites[i].scale = num / 8f;
		}
		circleSprites[0].color = new Color(1f / 51f, 0f, Mathf.Lerp(0.3f, 0.6f, buttonBehav.col));
		circleSprites[1].color = MyColor(timeStacker);
		circleSprites[1].alpha = 2f / num;
		circleSprites[2].scale = (num + 10f) / 8f;
		circleSprites[2].alpha = Mathf.Lerp(lastFilled, filled, timeStacker);
		circleSprites[3].color = Color.Lerp(MyColor(timeStacker), Menu.MenuRGB(Menu.MenuColors.DarkGrey), 0.5f);
		circleSprites[3].scale = (num + 15f) / 8f;
		circleSprites[3].alpha = 2f / (num + 15f);
		float num2 = 0.5f + 0.5f * Mathf.Sin(Mathf.Lerp(buttonBehav.lastSin, buttonBehav.sin, timeStacker) / 30f * (float)Math.PI * 2f);
		num2 *= buttonBehav.sizeBump;
		if (buttonBehav.greyedOut)
		{
			num2 = 0f;
		}
		circleSprites[4].scale = (num - 8f * buttonBehav.sizeBump) / 8f;
		circleSprites[4].alpha = 2f / (num - 8f * buttonBehav.sizeBump);
		circleSprites[4].color = new Color(0f, 0f, num2);
	}

	public override void RemoveSprites()
	{
		base.RemoveSprites();
		for (int i = 0; i < circleSprites.Length; i++)
		{
			circleSprites[i].RemoveFromContainer();
		}
		if (soundLoop != null)
		{
			soundLoop.Destroy();
		}
	}

	public virtual void Clicked()
	{
	}
}
