using System;
using UnityEngine;

namespace Menu;

public abstract class Slider : RectangularMenuObject, SelectableMenuObject, ButtonMenuObject
{
	public interface ISliderOwner
	{
		float ValueOfSlider(Slider slider);

		void SliderSetValue(Slider slider, float setValue);
	}

	public class SliderID : ExtEnum<SliderID>
	{
		public static readonly SliderID SfxVol = new SliderID("SfxVol", register: true);

		public static readonly SliderID MusicVol = new SliderID("MusicVol", register: true);

		public static readonly SliderID ArenaMusicVolume = new SliderID("ArenaMusicVolume", register: true);

		public static readonly SliderID LevelsListScroll = new SliderID("LevelsListScroll", register: true);

		public static readonly SliderID ModDescriptionScroll = new SliderID("ModDescriptionScroll", register: true);

		public SliderID(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public MenuLabel menuLabel;

	public bool subtleSlider;

	public RoundedRect roundedRect;

	public RoundedRect selectRect;

	public SubtleSliderNob subtleSliderNob;

	public ButtonBehavior buttonBehav;

	public float black;

	public Vector2 anchorPoint;

	public float length;

	public FSprite[] lineSprites;

	public float movSpeed;

	public float graphicInDraggedMode;

	public bool mouseDragged;

	public float mouseDragOffset;

	public SliderID ID;

	public Vector2 RelativeAnchorPoint
	{
		get
		{
			if (owner is PositionedMenuObject)
			{
				return anchorPoint + (owner as PositionedMenuObject).ScreenPos;
			}
			return anchorPoint;
		}
	}

	public ISliderOwner GetOwner
	{
		get
		{
			MenuObject menuObject = owner;
			while (true)
			{
				if (menuObject is ISliderOwner)
				{
					return menuObject as ISliderOwner;
				}
				if (menuObject.owner == null)
				{
					break;
				}
				menuObject = menuObject.owner;
			}
			return null;
		}
	}

	public float floatValue
	{
		get
		{
			if (GetOwner != null)
			{
				return GetOwner.ValueOfSlider(this);
			}
			return menu.ValueOfSlider(this);
		}
		set
		{
			if (GetOwner != null)
			{
				GetOwner.SliderSetValue(this, value);
			}
			else
			{
				menu.SliderSetValue(this, value);
			}
		}
	}

	public float ExtraLengthAtEnd
	{
		get
		{
			if (!subtleSlider)
			{
				return 20f;
			}
			return 20f;
		}
	}

	public bool Vertical => this is VerticalSlider;

	public override bool Selected
	{
		get
		{
			if (!mouseDragged)
			{
				return base.Selected;
			}
			return false;
		}
	}

	public override bool MouseOver
	{
		get
		{
			if (subtleSlider)
			{
				if (menu.mousePosition.x > subtleSliderNob.DrawX(1f) && menu.mousePosition.y > subtleSliderNob.DrawY(1f) && menu.mousePosition.x < subtleSliderNob.DrawX(1f) + subtleSliderNob.DrawSize(1f))
				{
					return menu.mousePosition.y < subtleSliderNob.DrawY(1f) + subtleSliderNob.DrawSize(1f);
				}
				return false;
			}
			if (menu.mousePosition.x > roundedRect.DrawX(1f) && menu.mousePosition.y > roundedRect.DrawY(1f) && menu.mousePosition.x < roundedRect.DrawX(1f) + roundedRect.DrawSize(1f).x)
			{
				return menu.mousePosition.y < roundedRect.DrawY(1f) + roundedRect.DrawSize(1f).y;
			}
			return false;
		}
	}

	public bool IsMouseOverMe
	{
		get
		{
			if (!mouseDragged)
			{
				return MouseOver;
			}
			return false;
		}
	}

	public bool CurrentlySelectableMouse => !buttonBehav.greyedOut;

	public bool CurrentlySelectableNonMouse => !subtleSlider;

	public ButtonBehavior GetButtonBehavior => buttonBehav;

	public Color MyColor(float timeStacker)
	{
		if (buttonBehav.greyedOut)
		{
			return HSLColor.Lerp(Menu.MenuColor(Menu.MenuColors.DarkGrey), Menu.MenuColor(Menu.MenuColors.Black), black).rgb;
		}
		float num = Mathf.Lerp(buttonBehav.lastCol, buttonBehav.col, timeStacker);
		num = Mathf.Max(num, Mathf.Lerp(buttonBehav.lastFlash, buttonBehav.flash, timeStacker), graphicInDraggedMode);
		HSLColor from = HSLColor.Lerp(Menu.MenuColor(subtleSlider ? Menu.MenuColors.DarkGrey : Menu.MenuColors.MediumGrey), Menu.MenuColor(Menu.MenuColors.White), num);
		return HSLColor.Lerp(from, Menu.MenuColor(Menu.MenuColors.Black), black).rgb;
	}

	public Slider(Menu menu, MenuObject owner, string text, Vector2 pos, Vector2 size, SliderID ID, bool subtleSlider)
		: base(menu, owner, pos, size)
	{
		this.ID = ID;
		this.subtleSlider = subtleSlider;
		anchorPoint = pos + new Vector2(0.01f, 0.01f);
		if (Vertical)
		{
			length = size.y;
		}
		else
		{
			length = size.x;
		}
		buttonBehav = new ButtonBehavior(this);
		lineSprites = new FSprite[4];
		for (int i = 0; i < lineSprites.Length; i++)
		{
			lineSprites[i] = new FSprite("pixel");
			Container.AddChild(lineSprites[i]);
		}
		if (Vertical)
		{
			lineSprites[0].scaleY = 2f;
			lineSprites[0].scaleX = 6f;
			lineSprites[1].scaleX = 2f;
			lineSprites[1].anchorY = 0f;
			lineSprites[2].scaleX = 2f;
			lineSprites[2].anchorY = 1f;
			lineSprites[3].scaleY = 2f;
			lineSprites[3].scaleX = 6f;
		}
		else
		{
			lineSprites[0].scaleX = 2f;
			lineSprites[0].scaleY = 6f;
			lineSprites[1].scaleY = 2f;
			lineSprites[1].anchorX = 0f;
			lineSprites[2].scaleY = 2f;
			lineSprites[2].anchorX = 1f;
			lineSprites[3].scaleX = 2f;
			lineSprites[3].scaleY = 6f;
		}
		if (subtleSlider)
		{
			subtleSliderNob = new SubtleSliderNob(menu, this, new Vector2(15f, 15f));
			subObjects.Add(subtleSliderNob);
		}
		else
		{
			roundedRect = new RoundedRect(menu, this, new Vector2(0.01f, 0.01f), new Vector2(Vertical ? 30f : 20f, Vertical ? 20f : 30f), filled: true);
			subObjects.Add(roundedRect);
			selectRect = new RoundedRect(menu, this, new Vector2(0.01f, 0.01f), new Vector2(Vertical ? 30f : 20f, Vertical ? 20f : 30f), filled: false);
			subObjects.Add(selectRect);
		}
		if (!subtleSlider)
		{
			if (Vertical)
			{
				menuLabel = new MenuLabel(menu, this, text, new Vector2(-50f, -20f), new Vector2(100f, 30f), bigText: false);
			}
			else
			{
				menuLabel = new MenuLabel(menu, this, text, new Vector2(length + 10f, 0f), size, bigText: false);
				menuLabel.label.alignment = FLabelAlignment.Left;
			}
			subObjects.Add(menuLabel);
		}
		base.page.selectables.Add(this);
	}

	public override void Update()
	{
		base.Update();
		buttonBehav.Update();
		if (!subtleSlider)
		{
			roundedRect.fillAlpha = Mathf.Max(Mathf.Lerp(0.3f, 0.6f, buttonBehav.col), Mathf.InverseLerp(0f, 0.2f, graphicInDraggedMode));
			if (Vertical)
			{
				roundedRect.addSize = Vector2.Lerp(new Vector2(6f, 10f), new Vector2(-6f, 4f), Mathf.Pow(graphicInDraggedMode, 0.4f)) * (buttonBehav.sizeBump + 0.5f * Mathf.Sin(buttonBehav.extraSizeBump * (float)Math.PI)) * (buttonBehav.clicked ? 0f : 1f);
				selectRect.addSize = Vector2.Lerp(new Vector2(-2f, 2f), new Vector2(-6f, 4f), Mathf.Pow(graphicInDraggedMode, 0.4f)) * (buttonBehav.sizeBump + 0.5f * Mathf.Sin(buttonBehav.extraSizeBump * (float)Math.PI)) * (buttonBehav.clicked ? 0f : 1f);
			}
			else
			{
				roundedRect.addSize = Vector2.Lerp(new Vector2(10f, 6f), new Vector2(4f, -6f), Mathf.Pow(graphicInDraggedMode, 0.4f)) * (buttonBehav.sizeBump + 0.5f * Mathf.Sin(buttonBehav.extraSizeBump * (float)Math.PI)) * (buttonBehav.clicked ? 0f : 1f);
				selectRect.addSize = Vector2.Lerp(new Vector2(2f, -2f), new Vector2(4f, -6f), Mathf.Pow(graphicInDraggedMode, 0.4f)) * (buttonBehav.sizeBump + 0.5f * Mathf.Sin(buttonBehav.extraSizeBump * (float)Math.PI)) * (buttonBehav.clicked ? 0f : 1f);
			}
		}
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		Color color = MyColor(timeStacker);
		if (!subtleSlider)
		{
			menuLabel.label.color = color;
			Color color2 = Color.Lerp(Menu.MenuRGB(Menu.MenuColors.Black), Menu.MenuRGB(Menu.MenuColors.White), Mathf.Max(Mathf.Lerp(buttonBehav.lastFlash, buttonBehav.flash, timeStacker), Mathf.InverseLerp(0f, 0.2f, graphicInDraggedMode)));
			for (int i = 0; i < 9; i++)
			{
				roundedRect.sprites[i].color = color2;
			}
			for (int j = 9; j < 17; j++)
			{
				roundedRect.sprites[j].color = color;
			}
			float alpha = 0.5f + 0.5f * Mathf.Sin(Mathf.Lerp(buttonBehav.lastSin, buttonBehav.sin, timeStacker) / 30f * (float)Math.PI * 2f);
			for (int k = 0; k < 8; k++)
			{
				selectRect.sprites[k].color = color;
				selectRect.sprites[k].alpha = alpha;
			}
		}
		for (int l = 0; l < lineSprites.Length; l++)
		{
			lineSprites[l].color = color;
		}
	}

	public override void RemoveSprites()
	{
		base.RemoveSprites();
		for (int i = 0; i < lineSprites.Length; i++)
		{
			lineSprites[i].RemoveFromContainer();
		}
	}

	public virtual void Clicked()
	{
	}
}
