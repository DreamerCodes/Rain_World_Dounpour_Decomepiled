using UnityEngine;

namespace Menu;

public abstract class ButtonTemplate : RectangularMenuObject, SelectableMenuObject, ButtonMenuObject
{
	public ButtonBehavior buttonBehav;

	public float black;

	public HSLColor? rectColor;

	public virtual bool IsMouseOverMe => MouseOver;

	public virtual bool CurrentlySelectableMouse => !buttonBehav.greyedOut;

	public virtual bool CurrentlySelectableNonMouse => true;

	public ButtonBehavior GetButtonBehavior => buttonBehav;

	public virtual Color MyColor(float timeStacker)
	{
		return InterpColor(timeStacker, rectColor.HasValue ? rectColor.Value : Menu.MenuColor(Menu.MenuColors.MediumGrey));
	}

	public ButtonTemplate(Menu menu, MenuObject owner, Vector2 pos, Vector2 size)
		: base(menu, owner, pos, size)
	{
		buttonBehav = new ButtonBehavior(this);
		base.page.selectables.Add(this);
	}

	public override void Update()
	{
		base.Update();
		buttonBehav.Update();
	}

	public virtual Color InterpColor(float timeStacker, HSLColor baseColor)
	{
		if (buttonBehav.greyedOut || inactive)
		{
			return HSLColor.Lerp(Menu.MenuColor(Menu.MenuColors.DarkGrey), Menu.MenuColor(Menu.MenuColors.Black), black).rgb;
		}
		float a = Mathf.Lerp(buttonBehav.lastCol, buttonBehav.col, timeStacker);
		a = Mathf.Max(a, Mathf.Lerp(buttonBehav.lastFlash, buttonBehav.flash, timeStacker));
		return HSLColor.Lerp(HSLColor.Lerp(baseColor, Menu.MenuColor(Menu.MenuColors.White), a), Menu.MenuColor(Menu.MenuColors.Black), black).rgb;
	}

	public virtual void Clicked()
	{
	}
}
