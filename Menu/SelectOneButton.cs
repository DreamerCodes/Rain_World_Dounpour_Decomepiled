using System;
using RWCustom;
using UnityEngine;

namespace Menu;

public class SelectOneButton : SimpleButton
{
	public interface SelectOneButtonOwner
	{
		int GetCurrentlySelectedOfSeries(string series);

		void SetCurrentlySelectedOfSeries(string series, int to);
	}

	public SelectOneButton[] buttonArray;

	public int buttonArrayIndex;

	public RoundedRect outerRect;

	public float selectedCol;

	public float lastSelectedCol;

	public bool handleSelectedColInChild;

	private SelectOneButtonOwner ReportTo
	{
		get
		{
			if (owner is SelectOneButtonOwner)
			{
				return owner as SelectOneButtonOwner;
			}
			return menu as SelectOneButtonOwner;
		}
	}

	public bool AmISelected => ReportTo.GetCurrentlySelectedOfSeries(signalText) == buttonArrayIndex;

	public override Color MyColor(float timeStacker)
	{
		return Color.Lerp(Menu.MenuRGB(Menu.MenuColors.DarkGrey), base.MyColor(timeStacker), Mathf.Lerp(lastSelectedCol, selectedCol, timeStacker));
	}

	public SelectOneButton(Menu menu, MenuObject owner, string displayText, string signalText, Vector2 pos, Vector2 size, SelectOneButton[] buttonArray, int buttonArrayIndex)
		: base(menu, owner, displayText, signalText, pos, size)
	{
		this.buttonArray = buttonArray;
		this.buttonArrayIndex = buttonArrayIndex;
		outerRect = new RoundedRect(menu, this, new Vector2(0f, 0f), size, filled: false);
		subObjects.Add(outerRect);
	}

	public override void Update()
	{
		base.Update();
		lastSelectedCol = selectedCol;
		if (!handleSelectedColInChild)
		{
			if (AmISelected)
			{
				if (menu.selectedObject is SelectOneButton && (menu.selectedObject as SelectOneButton).signalText == signalText)
				{
					buttonBehav.col = 1f;
				}
				selectedCol = Custom.LerpAndTick(selectedCol, 1f, 0.06f, 0.05f);
			}
			else if (Selected)
			{
				selectedCol = Custom.LerpAndTick(selectedCol, 1f, 0.06f, 0.05f);
			}
			else
			{
				selectedCol = Custom.LerpAndTick(selectedCol, 0f, 0.06f, 0.05f);
			}
		}
		outerRect.addSize = new Vector2(8f, 8f) * (1f + 0.5f * Mathf.Sin(buttonBehav.extraSizeBump * (float)Math.PI)) * (AmISelected ? 1f : 0f) + new Vector2(10f, 6f) * buttonBehav.sizeBump * (buttonBehav.clicked ? 0f : 1f);
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
	}

	public override void Clicked()
	{
		if (AmISelected)
		{
			menu.PlaySound(SoundID.MENY_Already_Selected_MultipleChoice_Clicked);
		}
		else
		{
			menu.PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
		}
		ReportTo.SetCurrentlySelectedOfSeries(signalText, buttonArrayIndex);
	}
}
