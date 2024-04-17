using System;
using UnityEngine;

namespace Menu;

public class BigArrowButton : ButtonTemplate
{
	public RoundedRect roundedRect;

	public RoundedRect selectRect;

	public int direction;

	public string signalText;

	public FSprite symbolSprite;

	public BigArrowButton(Menu menu, MenuObject owner, string singalText, Vector2 pos, int direction)
		: base(menu, owner, pos, new Vector2(50f, 50f))
	{
		this.direction = direction;
		signalText = singalText;
		roundedRect = new RoundedRect(menu, this, new Vector2(0f, 0f), size, filled: true);
		subObjects.Add(roundedRect);
		selectRect = new RoundedRect(menu, this, new Vector2(0f, 0f), size, filled: false);
		subObjects.Add(selectRect);
		symbolSprite = new FSprite("Big_Menu_Arrow");
		symbolSprite.rotation = (float)direction * 90f;
		Container.AddChild(symbolSprite);
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
		symbolSprite.color = MyColor(timeStacker);
		symbolSprite.x = DrawX(timeStacker) + DrawSize(timeStacker).x / 2f;
		symbolSprite.y = DrawY(timeStacker) + DrawSize(timeStacker).y / 2f;
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
