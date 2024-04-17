using System;
using RWCustom;
using UnityEngine;

namespace Menu;

public class MenuLabel : RectangularMenuObject
{
	private string myText;

	public FLabel label;

	public string text
	{
		get
		{
			return myText;
		}
		set
		{
			myText = value;
			label.text = value;
		}
	}

	public MenuLabel(Menu menu, MenuObject owner, string text, Vector2 pos, Vector2 size, bool bigText, FTextParams textParams = null)
		: base(menu, owner, pos, size)
	{
		myText = text;
		label = new FLabel(bigText ? Custom.GetDisplayFont() : Custom.GetFont(), text, (textParams == null) ? new FTextParams() : textParams);
		label.alignment = FLabelAlignment.Center;
		label.y = -10000f;
		Container.AddChild(label);
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		label.x = DrawX(timeStacker) + size.x / 2f;
		label.y = DrawY(timeStacker) + size.y / 2f;
	}

	public override void RemoveSprites()
	{
		base.RemoveSprites();
		label.RemoveFromContainer();
	}

	public static void WordWrapLabel(FLabel label, float maxWidth)
	{
		string text = "";
		string[] array = label.text.Split(Environment.NewLine.ToCharArray());
		for (int i = 0; i < array.Length; i++)
		{
			string text2 = "";
			if (array[i].Length == 0)
			{
				continue;
			}
			string[] array2 = array[i].Split(' ');
			if (array2.Length > 1)
			{
				for (int j = 0; j < array2.Length; j++)
				{
					text2 = (label.text = text2 + array2[j] + " ");
					if (label.textRect.width > maxWidth)
					{
						text = text + Environment.NewLine + array2[j] + " ";
						text2 = array2[j] + " ";
					}
					else
					{
						text = text + array2[j] + " ";
					}
				}
				if (i != array.Length - 1)
				{
					text += Environment.NewLine;
				}
				continue;
			}
			for (int k = 0; k < array[i].Length; k++)
			{
				text2 = (label.text = text2 + array[i][k]);
				if (label.textRect.width > maxWidth)
				{
					text = text + Environment.NewLine + array[i][k];
					text2 = array[i][k].ToString();
				}
				else
				{
					text += array[i][k];
				}
			}
		}
		label.text = text;
	}
}
