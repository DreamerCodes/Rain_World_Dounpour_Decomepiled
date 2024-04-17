using System;
using MoreSlugcats;
using UnityEngine;

namespace Menu;

public class SimpleButton : ButtonTemplate
{
	public MenuLabel menuLabel;

	public RoundedRect roundedRect;

	public RoundedRect selectRect;

	public HSLColor labelColor;

	public string signalText;

	public SimpleButton(Menu menu, MenuObject owner, string displayText, string singalText, Vector2 pos, Vector2 size)
		: base(menu, owner, pos, size)
	{
		signalText = singalText;
		labelColor = Menu.MenuColor(Menu.MenuColors.MediumGrey);
		roundedRect = new RoundedRect(menu, this, new Vector2(0f, 0f), size, filled: true);
		subObjects.Add(roundedRect);
		selectRect = new RoundedRect(menu, this, new Vector2(0f, 0f), size, filled: false);
		subObjects.Add(selectRect);
		menuLabel = new MenuLabel(menu, this, displayText, new Vector2(0f, 0f), size, bigText: false);
		subObjects.Add(menuLabel);
	}

	public void SetSize(Vector2 newSize)
	{
		size = newSize;
		roundedRect.size = size;
		selectRect.size = size;
		menuLabel.size = size;
	}

	public override void Update()
	{
		base.Update();
		buttonBehav.Update();
		roundedRect.fillAlpha = Mathf.Lerp(0.3f, 0.6f, buttonBehav.col);
		roundedRect.addSize = new Vector2(10f, 6f) * (buttonBehav.sizeBump + 0.5f * Mathf.Sin(buttonBehav.extraSizeBump * (float)Math.PI)) * (buttonBehav.clicked ? 0f : 1f);
		selectRect.addSize = new Vector2(2f, -2f) * (buttonBehav.sizeBump + 0.5f * Mathf.Sin(buttonBehav.extraSizeBump * (float)Math.PI)) * (buttonBehav.clicked ? 0f : 1f);
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		menuLabel.label.color = InterpColor(timeStacker, labelColor);
		if (ModManager.MSC && owner.menu.manager.currentMainLoop?.ID == MoreSlugcatsEnums.ProcessID.DatingSim)
		{
			menuLabel.label.color = new Color(1f - menuLabel.label.color.r, 1f - menuLabel.label.color.g, 1f - menuLabel.label.color.b);
		}
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
			if (ModManager.MSC && owner.menu.manager.currentMainLoop?.ID == MoreSlugcatsEnums.ProcessID.DatingSim)
			{
				selectRect.sprites[j].color = new Color(1f - selectRect.sprites[j].color.r, 1f - selectRect.sprites[j].color.g, 1f - selectRect.sprites[j].color.b);
			}
			selectRect.sprites[j].alpha = num;
		}
	}

	public override void Clicked()
	{
		Singal(this, signalText);
	}
}
