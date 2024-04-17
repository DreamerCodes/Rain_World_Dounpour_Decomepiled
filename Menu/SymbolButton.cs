using System;
using UnityEngine;

namespace Menu;

public class SymbolButton : ButtonTemplate
{
	public RoundedRect roundedRect;

	public string signalText;

	public FSprite symbolSprite;

	public bool maintainOutlineColorWhenGreyedOut;

	public override Color MyColor(float timeStacker)
	{
		if (buttonBehav.greyedOut)
		{
			if (maintainOutlineColorWhenGreyedOut)
			{
				return Menu.MenuRGB(Menu.MenuColors.DarkGrey);
			}
			return HSLColor.Lerp(Menu.MenuColor(Menu.MenuColors.VeryDarkGrey), Menu.MenuColor(Menu.MenuColors.Black), black).rgb;
		}
		float a = Mathf.Lerp(buttonBehav.lastCol, buttonBehav.col, timeStacker);
		a = Mathf.Max(a, Mathf.Lerp(buttonBehav.lastFlash, buttonBehav.flash, timeStacker));
		HSLColor from = HSLColor.Lerp(Menu.MenuColor(Menu.MenuColors.DarkGrey), Menu.MenuColor(Menu.MenuColors.MediumGrey), a);
		return HSLColor.Lerp(from, Menu.MenuColor(Menu.MenuColors.Black), black).rgb;
	}

	public SymbolButton(Menu menu, MenuObject owner, string symbolName, string singalText, Vector2 pos)
		: base(menu, owner, pos, new Vector2(24f, 24f))
	{
		signalText = singalText;
		roundedRect = new RoundedRect(menu, this, new Vector2(0f, 0f), size, filled: true);
		subObjects.Add(roundedRect);
		symbolSprite = new FSprite(symbolName);
		Container.AddChild(symbolSprite);
	}

	public override void Update()
	{
		base.Update();
		buttonBehav.Update();
		roundedRect.fillAlpha = Mathf.Lerp(0.3f, 0.6f, buttonBehav.col);
		roundedRect.addSize = new Vector2(4f, 4f) * (buttonBehav.sizeBump + 0.5f * Mathf.Sin(buttonBehav.extraSizeBump * (float)Math.PI)) * (buttonBehav.clicked ? 0f : 1f);
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		float num = 0.5f - 0.5f * Mathf.Sin(Mathf.Lerp(buttonBehav.lastSin, buttonBehav.sin, timeStacker) / 30f * (float)Math.PI * 2f);
		num *= buttonBehav.sizeBump;
		symbolSprite.color = (buttonBehav.greyedOut ? Menu.MenuRGB(Menu.MenuColors.VeryDarkGrey) : Color.Lerp(base.MyColor(timeStacker), Menu.MenuRGB(Menu.MenuColors.VeryDarkGrey), num));
		symbolSprite.x = DrawX(timeStacker) + DrawSize(timeStacker).x / 2f;
		symbolSprite.y = DrawY(timeStacker) + DrawSize(timeStacker).y / 2f;
		Color color = Color.Lerp(Menu.MenuRGB(Menu.MenuColors.Black), Menu.MenuRGB(Menu.MenuColors.White), Mathf.Lerp(buttonBehav.lastFlash, buttonBehav.flash, timeStacker));
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
