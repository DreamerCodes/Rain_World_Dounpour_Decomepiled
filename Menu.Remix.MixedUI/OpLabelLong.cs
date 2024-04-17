using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace Menu.Remix.MixedUI;

public class OpLabelLong : OpLabel
{
	public new class Queue : UIQueue
	{
		protected readonly string text;

		protected readonly float height;

		protected readonly FLabelAlignment alignment;

		public string description = "";

		protected override float sizeY => height;

		public Queue(string text, float height, FLabelAlignment alignment = FLabelAlignment.Left)
		{
			this.text = text;
			this.height = height;
			this.alignment = alignment;
		}

		protected override List<UIelement> _InitializeThisQueue(IHoldUIelements holder, float posX, ref float offsetY)
		{
			float posY = GetPosY(holder, offsetY);
			float x = Mathf.Clamp(holder.CanvasSize.x - posX, 50f, 600f);
			List<UIelement> list = new List<UIelement>();
			OpLabelLong opLabelLong = new OpLabelLong(new Vector2(posX, posY), new Vector2(x, height), UIQueue.Translate(text), autoWrap: true, alignment)
			{
				allowOverflow = true,
				description = UIQueue.Translate(description)
			};
			float num = Mathf.Max(30f, opLabelLong.GetDisplaySize().y + 6f);
			offsetY += num;
			opLabelLong.size = new Vector2(opLabelLong.size.x, num);
			opLabelLong.PosY -= num;
			list.Add(opLabelLong);
			hasInitialized = true;
			return list;
		}
	}

	public bool allowOverflow;

	public List<FLabel> labels;

	protected float LineHeight => LabelTest.LineHeight(bigText: false);

	public OpLabelLong(Vector2 pos, Vector2 size, string text = "TEXT", bool autoWrap = true, FLabelAlignment alignment = FLabelAlignment.Left)
		: base(pos, size, text, alignment)
	{
		base.autoWrap = autoWrap;
		labels = new List<FLabel>();
		allowOverflow = true;
		Change();
	}

	protected internal override void Change()
	{
		base.Change();
		string[] array = _displayText.Replace(Environment.NewLine, "\n").Split('\n');
		List<string> list = new List<string> { string.Empty };
		int num = 0;
		int num2 = 0;
		int num3 = (allowOverflow ? int.MaxValue : Mathf.FloorToInt(base.size.y / LineHeight));
		int num4 = 0;
		for (int i = 0; i < array.Length && i <= num3; i++)
		{
			string text = array[i].Trim('\n');
			if (string.IsNullOrEmpty(text))
			{
				text = " ";
			}
			if (num2 + text.Length > LabelTest.CharLimit(bigText: false) || num4 > 8)
			{
				num4 = 0;
				num2 = 0;
				num++;
				list.Add(string.Empty);
			}
			if (num2 > 0)
			{
				list[num] += "\n";
			}
			if (i == num3)
			{
				text = LabelTest.TrimText(text, base.size.x, addDots: true);
			}
			list[num] += text;
			num2 += text.Length + 1;
			num4++;
		}
		while (labels.Count < list.Count)
		{
			FLabel fLabel = new FLabel(LabelTest.GetFont(_bigText), "")
			{
				text = "",
				alignment = _alignment,
				color = color,
				y = -10000f,
				anchorY = 1f
			};
			labels.Add(fLabel);
			myContainer.AddChild(fLabel);
		}
		num = 0;
		float num5 = base.size.y;
		for (int j = 0; j < labels.Count; j++)
		{
			if (list.Count <= j)
			{
				labels[j].text = string.Empty;
				continue;
			}
			labels[j].text = list[j];
			int num6 = 0;
			string[] array2 = list[j].Split('\n');
			for (int k = 0; k < array2.Length; k++)
			{
				if (!string.IsNullOrEmpty(array2[k]))
				{
					num6++;
				}
			}
			switch (_alignment)
			{
			default:
				labels[j].alignment = FLabelAlignment.Center;
				labels[j].x = base.size.x / 2f;
				break;
			case FLabelAlignment.Left:
				labels[j].alignment = FLabelAlignment.Left;
				labels[j].x = 0f;
				break;
			case FLabelAlignment.Right:
				labels[j].alignment = FLabelAlignment.Right;
				labels[j].x = base.size.x;
				break;
			}
			num += num6;
			labels[j].y = num5;
			num5 -= labels[j].textRect.height;
		}
		if (base.verticalAlignment == LabelVAlignment.Top)
		{
			num5 = 0f;
		}
		if (base.verticalAlignment == LabelVAlignment.Center)
		{
			num5 /= 2f;
		}
		if (InGameTranslator.LanguageID.UsesLargeFont(Custom.rainWorld.inGameTranslator.currentLanguage))
		{
			num5 -= LabelTest.LineHalfHeight(bigText: false);
		}
		for (int l = 0; l < labels.Count; l++)
		{
			labels[l].y -= num5;
		}
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		Color color = ((bumpBehav != null) ? bumpBehav.GetColor(base.color) : base.color);
		foreach (FLabel label in labels)
		{
			label.color = color;
			label.alpha = alpha;
		}
	}

	public override Vector2 GetDisplaySize()
	{
		float num = 0f;
		float y = 0f;
		float num2 = labels[0].y + labels[0].textRect.height / 2f;
		for (int i = 0; i < labels.Count && !string.IsNullOrEmpty(labels[i].text); i++)
		{
			num = Mathf.Max(num, labels[i].textRect.width);
			y = num2 - (labels[i].y - labels[i].textRect.height / 2f);
		}
		return new Vector2(num, y);
	}
}
