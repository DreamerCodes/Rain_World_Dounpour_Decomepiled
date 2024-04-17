using System;
using RWCustom;
using UnityEngine;

namespace Menu;

public class CheckBox : ButtonTemplate
{
	public interface IOwnCheckBox
	{
		bool GetChecked(CheckBox box);

		void SetChecked(CheckBox box, bool c);
	}

	public RoundedRect roundedRect;

	public FSprite symbolSprite;

	public MenuLabel label;

	public float symbolHalfVisible;

	public float lastSymbolHalfVisible;

	public bool selectable = true;

	public bool brightColor = true;

	public string displayText;

	public string IDString;

	private IOwnCheckBox reportTo;

	public bool Checked
	{
		get
		{
			return reportTo.GetChecked(this);
		}
		set
		{
			reportTo.SetChecked(this, value);
		}
	}

	public override bool CurrentlySelectableMouse
	{
		get
		{
			if (base.CurrentlySelectableMouse)
			{
				return selectable;
			}
			return false;
		}
	}

	public override bool CurrentlySelectableNonMouse
	{
		get
		{
			if (base.CurrentlySelectableNonMouse)
			{
				return selectable;
			}
			return false;
		}
	}

	public override Color MyColor(float timeStacker)
	{
		if (buttonBehav.greyedOut)
		{
			return HSLColor.Lerp(Menu.MenuColor(Menu.MenuColors.VeryDarkGrey), Menu.MenuColor(Menu.MenuColors.Black), black).rgb;
		}
		float a = Mathf.Lerp(buttonBehav.lastCol, buttonBehav.col, timeStacker);
		a = Mathf.Max(a, Mathf.Lerp(buttonBehav.lastFlash, buttonBehav.flash, timeStacker));
		return Color.Lerp(brightColor ? Color.Lerp(Menu.MenuRGB(Menu.MenuColors.MediumGrey), Menu.MenuRGB(Menu.MenuColors.White), a) : Color.Lerp(Menu.MenuRGB(Menu.MenuColors.DarkGrey), Menu.MenuRGB(Menu.MenuColors.MediumGrey), a), Menu.MenuRGB(Menu.MenuColors.Black), black);
	}

	public CheckBox(Menu menu, MenuObject owner, IOwnCheckBox reportTo, Vector2 pos, float textWidth, string displayText, string IDString, bool textOnRight = false)
		: base(menu, owner, pos, new Vector2(24f, 24f))
	{
		this.displayText = displayText;
		this.reportTo = reportTo;
		this.IDString = IDString;
		roundedRect = new RoundedRect(menu, this, new Vector2(0f, 0f), size, filled: true);
		subObjects.Add(roundedRect);
		label = new MenuLabel(menu, this, displayText, new Vector2(textOnRight ? 9f : ((0f - textWidth) * 1.5f), 3f), new Vector2(textWidth, 20f), bigText: false);
		label.label.alignment = FLabelAlignment.Left;
		label.label.anchorX = 0f;
		subObjects.Add(label);
		symbolSprite = new FSprite("Menu_Symbol_CheckBox");
		Container.AddChild(symbolSprite);
	}

	public override void Update()
	{
		base.Update();
		buttonBehav.Update();
		lastSymbolHalfVisible = symbolHalfVisible;
		if (Selected)
		{
			symbolHalfVisible = Custom.LerpAndTick(symbolHalfVisible, 1f, 0.07f, 1f / 60f);
		}
		else
		{
			symbolHalfVisible = 0f;
		}
		roundedRect.fillAlpha = Mathf.Lerp(0.3f, 0.6f, buttonBehav.col);
		roundedRect.addSize = new Vector2(4f, 4f) * (buttonBehav.sizeBump + 0.5f * Mathf.Sin(buttonBehav.extraSizeBump * (float)Math.PI)) * (buttonBehav.clicked ? 0f : 1f);
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		float num = 0.5f - 0.5f * Mathf.Sin(Mathf.Lerp(buttonBehav.lastSin, buttonBehav.sin, timeStacker) / 30f * (float)Math.PI * 2f);
		num *= buttonBehav.sizeBump;
		label.label.color = MyColor(timeStacker);
		Color color = Color.Lerp(Menu.MenuRGB(Menu.MenuColors.Black), Menu.MenuRGB(Menu.MenuColors.White), Mathf.Lerp(buttonBehav.lastFlash, buttonBehav.flash, timeStacker));
		for (int i = 0; i < 9; i++)
		{
			roundedRect.sprites[i].color = color;
		}
		color = (buttonBehav.greyedOut ? Menu.MenuRGB(Menu.MenuColors.VeryDarkGrey) : Color.Lerp(base.MyColor(timeStacker), Menu.MenuRGB(Menu.MenuColors.VeryDarkGrey), num));
		symbolSprite.color = color;
		symbolSprite.x = DrawX(timeStacker) + DrawSize(timeStacker).x / 2f;
		symbolSprite.y = DrawY(timeStacker) + DrawSize(timeStacker).y / 2f;
		if (Checked)
		{
			symbolSprite.alpha = 1f;
		}
		else
		{
			symbolSprite.alpha = Mathf.Lerp(lastSymbolHalfVisible, symbolHalfVisible, timeStacker) * 0.25f;
		}
	}

	public override void RemoveSprites()
	{
		if (symbolSprite != null)
		{
			symbolSprite.RemoveFromContainer();
		}
		base.RemoveSprites();
	}

	public override void Clicked()
	{
		if (!buttonBehav.greyedOut)
		{
			Checked = !Checked;
			menu.PlaySound(Checked ? SoundID.MENU_Checkbox_Check : SoundID.MENU_Checkbox_Uncheck);
			symbolHalfVisible = 0f;
			lastSymbolHalfVisible = 0f;
		}
	}
}
