using RWCustom;
using UnityEngine;

namespace Menu;

public class ButtonBehavior
{
	public MenuObject owner;

	public float lastCol;

	public float col;

	public bool bump;

	public bool clicked;

	public float sizeBump;

	public float extraSizeBump;

	public float lastExtraSizeBump;

	public float flash;

	public float lastFlash;

	public bool flashBool;

	public float sin;

	public float lastSin;

	public bool greyedOut;

	public ButtonBehavior(MenuObject owner)
	{
		this.owner = owner;
	}

	public void Update()
	{
		lastCol = col;
		lastFlash = flash;
		lastSin = sin;
		flash = Custom.LerpAndTick(flash, 0f, 0.03f, 1f / 6f);
		if (owner.Selected && (!greyedOut || !owner.menu.manager.menuesMouseMode))
		{
			if (!bump)
			{
				bump = true;
			}
			sizeBump = Custom.LerpAndTick(sizeBump, 1f, 0.1f, 0.1f);
			sin += 1f;
			if (!flashBool)
			{
				flashBool = true;
				flash = 1f;
			}
			if (!greyedOut)
			{
				if (owner.menu.pressButton)
				{
					if (!clicked)
					{
						owner.menu.PlaySound(SoundID.MENU_Button_Press_Init);
					}
					clicked = true;
				}
				if (!owner.menu.holdButton)
				{
					if (clicked)
					{
						(owner as ButtonMenuObject).Clicked();
					}
					clicked = false;
				}
				col = Mathf.Min(1f, col + 0.1f);
			}
		}
		else
		{
			clicked = false;
			bump = false;
			flashBool = false;
			sizeBump = Custom.LerpAndTick(sizeBump, 0f, 0.1f, 0.05f);
			col = Mathf.Max(0f, col - 1f / 30f);
		}
		if (owner.toggled)
		{
			sizeBump = Custom.LerpAndTick(sizeBump, 1f, 0.1f, 0.1f);
			sin = 7.5f;
			bump = true;
			if (flash < 0.75f)
			{
				flash = 0.75f;
			}
		}
		lastExtraSizeBump = extraSizeBump;
		if (bump)
		{
			extraSizeBump = Mathf.Min(1f, extraSizeBump + 0.1f);
		}
		else
		{
			extraSizeBump = 0f;
		}
	}
}
