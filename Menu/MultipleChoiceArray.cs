using System;
using RWCustom;
using UnityEngine;

namespace Menu;

public class MultipleChoiceArray : RectangularMenuObject
{
	public interface IOwnMultipleChoiceArray
	{
		int GetSelected(MultipleChoiceArray array);

		void SetSelected(MultipleChoiceArray array, int i);
	}

	public class MultipleChoiceButton : ButtonTemplate
	{
		public int index;

		public RoundedRect roundedRect;

		public FSprite symbolSprite;

		public MenuLabel label;

		public float symbolHalfVisible;

		public float lastSymbolHalfVisible;

		public bool Checked => multipleChoiceArray.CheckedButton == index;

		public MultipleChoiceArray multipleChoiceArray => owner as MultipleChoiceArray;

		public override Color MyColor(float timeStacker)
		{
			if (buttonBehav.greyedOut)
			{
				return HSLColor.Lerp(Menu.MenuColor(Menu.MenuColors.VeryDarkGrey), Menu.MenuColor(Menu.MenuColors.Black), black).rgb;
			}
			float a = Mathf.Lerp(buttonBehav.lastCol, buttonBehav.col, timeStacker);
			a = Mathf.Max(a, Mathf.Lerp(buttonBehav.lastFlash, buttonBehav.flash, timeStacker));
			return Color.Lerp(Color.Lerp(multipleChoiceArray.MyColor(timeStacker), Menu.MenuRGB(Menu.MenuColors.MediumGrey), a), Menu.MenuRGB(Menu.MenuColors.Black), black);
		}

		public MultipleChoiceButton(Menu menu, MultipleChoiceArray mcOwner, Vector2 pos, int index)
			: base(menu, mcOwner, pos, new Vector2(24f, 24f))
		{
			this.index = index;
			roundedRect = new RoundedRect(menu, this, new Vector2(0f, 0f), size, filled: true);
			subObjects.Add(roundedRect);
			if (mcOwner.textInBoxes)
			{
				label = new MenuLabel(menu, this, index.ToString(), new Vector2(0f, -3f), new Vector2(24f, 30f), bigText: false);
				subObjects.Add(label);
			}
			else
			{
				symbolSprite = new FSprite("Menu_Symbol_Clear_All");
				Container.AddChild(symbolSprite);
			}
		}

		public override void Update()
		{
			base.Update();
			buttonBehav.Update();
			lastSymbolHalfVisible = symbolHalfVisible;
			if (Selected)
			{
				multipleChoiceArray.anyButtonSelected = true;
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
			Color color = Color.Lerp(Menu.MenuRGB(Menu.MenuColors.Black), Menu.MenuRGB(Menu.MenuColors.White), Mathf.Lerp(buttonBehav.lastFlash, buttonBehav.flash, timeStacker));
			for (int i = 0; i < 9; i++)
			{
				roundedRect.sprites[i].color = color;
			}
			color = (buttonBehav.greyedOut ? Menu.MenuRGB(Menu.MenuColors.VeryDarkGrey) : Color.Lerp(base.MyColor(timeStacker), Menu.MenuRGB(Menu.MenuColors.VeryDarkGrey), num));
			if (multipleChoiceArray.textInBoxes)
			{
				label.label.color = color;
				if (Checked)
				{
					label.label.alpha = 1f;
				}
				else
				{
					label.label.alpha = Mathf.Lerp(lastSymbolHalfVisible, symbolHalfVisible, timeStacker) * 0.5f;
				}
				return;
			}
			symbolSprite.color = color;
			symbolSprite.x = DrawX(timeStacker) + DrawSize(timeStacker).x / 2f;
			symbolSprite.y = DrawY(timeStacker) + DrawSize(timeStacker).y / 2f;
			if (Checked)
			{
				symbolSprite.alpha = 1f;
			}
			else
			{
				symbolSprite.alpha = Mathf.Lerp(lastSymbolHalfVisible, symbolHalfVisible, timeStacker) * 0.5f;
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
			Singal(this, index.ToString());
		}
	}

	public MenuLabel label;

	public MultipleChoiceButton[] buttons;

	public FSprite[] lines;

	private float lightUp;

	private float lastLightUp;

	public bool anyButtonSelected;

	private bool gOut;

	private IOwnMultipleChoiceArray reportTo;

	public bool textInBoxes;

	public string IDString;

	public bool greyedOut
	{
		get
		{
			return gOut;
		}
		set
		{
			gOut = value;
			for (int i = 0; i < buttons.Length; i++)
			{
				buttons[i].buttonBehav.greyedOut = gOut;
			}
		}
	}

	public int CheckedButton
	{
		get
		{
			return reportTo.GetSelected(this);
		}
		set
		{
			reportTo.SetSelected(this, value);
		}
	}

	private float XPosOfButton(int i)
	{
		if (buttons.Length < 2)
		{
			return 0f;
		}
		return (size.x - 24f) / (float)(buttons.Length - 1) * (float)i;
	}

	public Color MyColor(float timeStacker)
	{
		if (greyedOut)
		{
			return Menu.MenuRGB(Menu.MenuColors.VeryDarkGrey);
		}
		return Color.Lerp(Menu.MenuRGB(Menu.MenuColors.DarkGrey), Menu.MenuRGB(Menu.MenuColors.MediumGrey), Mathf.Lerp(lastLightUp, lightUp, timeStacker));
	}

	public MultipleChoiceArray(Menu menu, MenuObject owner, IOwnMultipleChoiceArray reportTo, Vector2 pos, string text, string IDString, float textWidth, float width, int buttonsCount, bool textInBoxes, bool splitText)
		: base(menu, owner, pos, new Vector2(width, 24f))
	{
		this.textInBoxes = textInBoxes;
		this.reportTo = reportTo;
		this.IDString = IDString;
		if (splitText)
		{
			text = InGameTranslator.EvenSplit(text, 1);
		}
		label = new MenuLabel(menu, this, text, new Vector2((0f - textWidth) * 1.5f, 3f), new Vector2(textWidth, 20f), bigText: false);
		label.label.alignment = FLabelAlignment.Left;
		label.label.anchorX = 0f;
		subObjects.Add(label);
		lines = new FSprite[buttonsCount - 1];
		for (int i = 0; i < lines.Length; i++)
		{
			lines[i] = new FSprite("pixel");
			lines[i].anchorX = 0f;
			lines[i].scaleY = 2f;
			Container.AddChild(lines[i]);
		}
		buttons = new MultipleChoiceButton[buttonsCount];
		for (int j = 0; j < buttons.Length; j++)
		{
			Vector2 vector = new Vector2(XPosOfButton(j), 0f);
			buttons[j] = new MultipleChoiceButton(menu, this, vector, j);
			subObjects.Add(buttons[j]);
		}
		buttons[buttons.Length - 1].nextSelectable[2] = buttons[0];
	}

	public override void Update()
	{
		base.Update();
		lastLightUp = lightUp;
		lightUp = Custom.LerpAndTick(lightUp, anyButtonSelected ? 1f : 0f, 0.07f, 0.025f);
		anyButtonSelected = false;
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		if (greyedOut)
		{
			label.label.color = Menu.MenuRGB(Menu.MenuColors.VeryDarkGrey);
		}
		else
		{
			label.label.color = Color.Lerp(Menu.MenuRGB(Menu.MenuColors.DarkGrey), Menu.MenuRGB(Menu.MenuColors.White), Mathf.Lerp(lastLightUp, lightUp, timeStacker));
		}
		Color color = MyColor(timeStacker);
		for (int i = 0; i < lines.Length; i++)
		{
			float num = buttons[i].DrawX(timeStacker) + buttons[i].size.x + Mathf.Lerp(buttons[i].roundedRect.lastAddSize.x, buttons[i].roundedRect.addSize.x, timeStacker) / 2f;
			float num2 = buttons[i + 1].DrawX(timeStacker) - Mathf.Lerp(buttons[i + 1].roundedRect.lastAddSize.x, buttons[i + 1].roundedRect.addSize.x, timeStacker) / 2f;
			lines[i].x = num;
			lines[i].scaleX = num2 - num;
			lines[i].y = DrawY(timeStacker) + 12f;
			lines[i].color = color;
		}
	}

	public override void RemoveSprites()
	{
		for (int i = 0; i < lines.Length; i++)
		{
			lines[i].RemoveFromContainer();
		}
		base.RemoveSprites();
	}

	public override void Singal(MenuObject sender, string message)
	{
		for (int i = 0; i < buttons.Length; i++)
		{
			if (buttons[i] == sender)
			{
				menu.PlaySound((CheckedButton == i) ? SoundID.MENY_Already_Selected_MultipleChoice_Clicked : SoundID.MENU_MultipleChoice_Clicked);
				CheckedButton = i;
				break;
			}
		}
	}
}
