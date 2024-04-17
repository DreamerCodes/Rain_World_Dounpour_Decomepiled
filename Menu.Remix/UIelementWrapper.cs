using System;
using Menu.Remix.MixedUI;

namespace Menu.Remix;

public class UIelementWrapper : RectangularMenuObject, SelectableMenuObject
{
	public readonly UIelement thisElement;

	private readonly GlowGradient glow;

	public readonly MenuTabWrapper tabWrapper;

	public bool IsFocusable => thisElement is UIfocusable;

	public UIfocusable ThisFocusable => thisElement as UIfocusable;

	public bool GreyedOut
	{
		get
		{
			if (IsFocusable)
			{
				return ThisFocusable.bumpBehav.greyedOut;
			}
			return false;
		}
	}

	public bool IsConfig => thisElement is UIconfig;

	public UIconfig ThisConfig => thisElement as UIconfig;

	public bool IsMouseOverMe => thisElement.MouseOver;

	public bool CurrentlySelectableMouse
	{
		get
		{
			if (!IsFocusable)
			{
				return !thisElement.IsInactive;
			}
			if (!thisElement.IsInactive)
			{
				return ThisFocusable.CurrentlyFocusableMouse;
			}
			return false;
		}
	}

	public bool CurrentlySelectableNonMouse
	{
		get
		{
			if (!thisElement.IsInactive && IsFocusable)
			{
				return ThisFocusable.CurrentlyFocusableNonMouse;
			}
			return false;
		}
	}

	public UIelementWrapper(MenuTabWrapper tabWrapper, UIelement element)
		: base(tabWrapper.menu, tabWrapper, element.GetPos(), element.size)
	{
		thisElement = element;
		thisElement.wrapper = this;
		this.tabWrapper = tabWrapper;
		if (this.tabWrapper.wrappers.ContainsKey(element))
		{
			throw new ArgumentException("This element is already bound to this MenuTabWrapper.");
		}
		this.tabWrapper.wrappers.Add(element, this);
		this.tabWrapper.subObjects.Add(this);
		this.tabWrapper._tab.AddItems(element);
		thisElement.Update();
		thisElement.GrafUpdate(0f);
		if (IsFocusable)
		{
			glow = new GlowGradient(this.tabWrapper._glowContainer, pos, size, 0f)
			{
				color = MenuColorEffect.rgbBlack
			};
			base.page.selectables.Add(this);
			if (IsConfig)
			{
				ReloadConfig();
			}
		}
	}

	public void ReloadConfig()
	{
		if (IsConfig)
		{
			if (ThisConfig.cosmetic)
			{
				thisElement.Reset();
				return;
			}
			ThisConfig.cfgEntry.OI._LoadConfigFile();
			ThisConfig.ShowConfig();
		}
	}

	public void ResetConfig()
	{
		if (IsConfig && !ThisConfig.cosmetic)
		{
			ThisConfig.value = ThisConfig.cfgEntry.defaultValue;
		}
	}

	public void SaveConfig()
	{
		if (IsConfig && !ThisConfig.cosmetic)
		{
			ThisConfig.cfgEntry.BoxedValue = ValueConverter.ConvertToValue(ThisConfig.value, ThisConfig.cfgEntry.settingType);
			ThisConfig.cfgEntry.OI._SaveConfigFile();
		}
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		if (IsFocusable)
		{
			glow.size = thisElement.size;
			glow.pos = thisElement.pos;
			glow.alpha = (Selected ? (ThisFocusable.bumpBehav.Sin(ThisFocusable.held ? 60f : 15f) * 0.5f + 0.2f) : 0f);
		}
	}

	public override void Update()
	{
		base.Update();
		if (menu.infoLabel != null && Selected)
		{
			string text = thisElement.DisplayDescription();
			if (!string.IsNullOrEmpty(text))
			{
				menu.infoLabel.text = text;
			}
		}
		if (IsFocusable)
		{
			if (!Selected && ThisFocusable.held)
			{
				ThisFocusable.NonMouseSetHeld(newHeld: false);
			}
			if (!tabWrapper._lastHoldElement && !menu.manager.menuesMouseMode && Selected && !ThisFocusable.greyedOut && !ThisFocusable.held && menu.input.jmp && !menu.lastInput.jmp)
			{
				ThisFocusable.NonMouseSetHeld(newHeld: true);
			}
		}
	}

	public override void Singal(MenuObject sender, string message)
	{
		base.Singal(this, message);
	}
}
