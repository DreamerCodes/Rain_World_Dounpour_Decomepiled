using System;
using UnityEngine;

namespace Menu;

public class MuteButton : ButtonTemplate
{
	public RoundedRect roundedRect;

	public string signalText;

	public FSprite symbolSprite;

	public bool maintainOutlineColorWhenGreyedOut;

	public bool muted;

	public MuteButton(Menu menu, MenuObject owner, string symbolName, string singalText, Vector2 pos, Vector2 size)
		: base(menu, owner, pos, size)
	{
		muted = false;
		signalText = singalText;
		roundedRect = new RoundedRect(menu, this, new Vector2(0f, 0f), size, filled: true);
		subObjects.Add(roundedRect);
		symbolSprite = new FSprite(symbolName);
		Container.AddChild(symbolSprite);
		symbolSprite.x = DrawX(1f) + DrawSize(1f).x / 2f;
		symbolSprite.y = DrawY(1f) + DrawSize(1f).y / 2f;
	}

	public override Color MyColor(float timeStacker)
	{
		if (!buttonBehav.greyedOut)
		{
			float a = Mathf.Lerp(buttonBehav.lastCol, buttonBehav.col, timeStacker);
			a = Mathf.Max(a, Mathf.Lerp(buttonBehav.lastFlash, buttonBehav.flash, timeStacker));
			return HSLColor.Lerp(HSLColor.Lerp(Menu.MenuColor(Menu.MenuColors.MediumGrey), Menu.MenuColor(Menu.MenuColors.White), a), Menu.MenuColor(Menu.MenuColors.Black), black).rgb;
		}
		if (maintainOutlineColorWhenGreyedOut)
		{
			return Menu.MenuRGB(Menu.MenuColors.DarkGrey);
		}
		return HSLColor.Lerp(Menu.MenuColor(Menu.MenuColors.VeryDarkGrey), Menu.MenuColor(Menu.MenuColors.Black), black).rgb;
	}

	public override void Update()
	{
		base.Update();
		buttonBehav.Update();
		roundedRect.addSize = new Vector2(4f, 4f) * (buttonBehav.sizeBump + 0.5f * Mathf.Sin(buttonBehav.extraSizeBump * (float)Math.PI)) * ((!buttonBehav.clicked) ? 1f : 0f);
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		if (muted && symbolSprite.element.name != "muted")
		{
			symbolSprite.SetElementByName("muted");
		}
		if (!muted && symbolSprite.element.name != "speaker")
		{
			symbolSprite.SetElementByName("speaker");
		}
		_ = 0.5f - 0.5f * Mathf.Sin(Mathf.Lerp(buttonBehav.lastSin, buttonBehav.sin, timeStacker) / 30f * (float)Math.PI * 2f);
		_ = buttonBehav.sizeBump;
		symbolSprite.color = (muted ? Menu.MenuRGB(Menu.MenuColors.DarkGrey) : Menu.MenuRGB(Menu.MenuColors.White));
		symbolSprite.x = DrawX(timeStacker) + DrawSize(timeStacker).x / 2f;
		symbolSprite.y = DrawY(timeStacker) + DrawSize(timeStacker).y / 2f;
		Color color = (muted ? Menu.MenuRGB(Menu.MenuColors.Black) : Menu.MenuRGB(Menu.MenuColors.White));
		for (int i = 0; i < 9; i++)
		{
			roundedRect.sprites[i].color = color;
		}
	}

	public void UpdateSymbol(string newSymbolName)
	{
		symbolSprite.element = Futile.atlasManager.GetElementWithName(newSymbolName);
	}

	public override void RemoveSprites()
	{
		symbolSprite.RemoveFromContainer();
		base.RemoveSprites();
	}

	public override void Clicked()
	{
		Singal(this, signalText);
	}
}
