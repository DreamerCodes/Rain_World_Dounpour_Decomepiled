using System;
using System.Collections.Generic;
using UnityEngine;

namespace Menu.Remix.MixedUI;

public class OpLabel : UIelement
{
	public enum LabelVAlignment
	{
		Top,
		Center,
		Bottom
	}

	public class Queue : UIQueue
	{
		protected readonly string text;

		protected readonly FLabelAlignment alignment;

		protected readonly bool bigText;

		public string description = "";

		protected override float sizeY
		{
			get
			{
				if (!bigText)
				{
					return 30f;
				}
				return 50f;
			}
		}

		public Queue(string text, FLabelAlignment alignment = FLabelAlignment.Left, bool bigText = false)
		{
			this.text = text;
			this.alignment = alignment;
			this.bigText = bigText;
		}

		protected override List<UIelement> _InitializeThisQueue(IHoldUIelements holder, float posX, ref float offsetY)
		{
			offsetY += sizeY;
			float posY = GetPosY(holder, offsetY);
			float x = Mathf.Clamp(holder.CanvasSize.x - posX, 50f, 600f);
			List<UIelement> list = new List<UIelement>();
			OpLabel item = new OpLabel(new Vector2(posX, posY), new Vector2(x, sizeY), UIQueue.Translate(text), alignment, bigText)
			{
				description = UIQueue.Translate(description)
			};
			list.Add(item);
			hasInitialized = true;
			return list;
		}
	}

	private LabelVAlignment _verticalAlignment;

	public FLabel label;

	protected readonly bool _bigText;

	public bool autoWrap;

	protected FLabelAlignment _alignment;

	public Color color = MenuColorEffect.rgbMediumGrey;

	public float alpha = 1f;

	public BumpBehaviour bumpBehav;

	protected string _text;

	protected string _displayText;

	public LabelVAlignment verticalAlignment
	{
		get
		{
			return _verticalAlignment;
		}
		set
		{
			if (_verticalAlignment != value)
			{
				_verticalAlignment = value;
				Change();
			}
		}
	}

	protected bool _IsLong => this is OpLabelLong;

	public FLabelAlignment alignment
	{
		get
		{
			return _alignment;
		}
		set
		{
			if (_alignment != value)
			{
				_alignment = value;
				Change();
			}
		}
	}

	public string text
	{
		get
		{
			return _text;
		}
		set
		{
			if (_text != value)
			{
				_text = value;
				Change();
			}
		}
	}

	public OpLabel(Vector2 pos, Vector2 size, string text = "TEXT", FLabelAlignment alignment = FLabelAlignment.Center, bool bigText = false, FTextParams textParams = null)
		: base(pos, size)
	{
		_size = new Vector2(Mathf.Max(size.x, 20f), Mathf.Max(size.y, 20f));
		_bigText = bigText;
		autoWrap = false;
		if (text == "TEXT")
		{
			int num = ((!_IsLong) ? 1 : Math.Max(1, Mathf.FloorToInt(_size.y * 0.65f / LabelTest.LineHeight(_bigText))));
			int num2 = Mathf.FloorToInt(_size.x / LabelTest.CharMean(_bigText)) * num;
			int num3 = num2 / 60;
			if (_IsLong)
			{
				int num4 = UnityEngine.Random.Range(1, Mathf.CeilToInt((float)num / 1.5f));
				text = LoremIpsum.Generate(Math.Max(1, (num3 - 2) / num4), Math.Max(2, num3 / num4), num4);
			}
			else
			{
				text = LoremIpsum.Generate(Math.Max(1, num3 - 2), Math.Max(1, num3));
			}
			text = ((text.Length > num2) ? (text.Substring(0, num2 - 2).TrimEnd() + ".") : text);
		}
		if (_IsLong)
		{
			_text = text;
			_verticalAlignment = LabelVAlignment.Top;
		}
		else
		{
			_text = text;
			_verticalAlignment = LabelVAlignment.Center;
		}
		_alignment = alignment;
		if (!_IsLong)
		{
			label = new FLabel(LabelTest.GetFont(_bigText), _text, (textParams == null) ? new FTextParams() : textParams)
			{
				alignment = _alignment,
				color = color,
				y = -10000f
			};
			myContainer.AddChild(label);
			Change();
		}
	}

	public OpLabel(float posX, float posY, string text = "TEXT", bool bigText = false)
		: this(new Vector2(posX, posY), new Vector2(100f, 20f), text, FLabelAlignment.Left, bigText)
	{
		_text = ((text.Length < LabelTest.CharLimit(_bigText) / 3) ? text : text.Substring(0, LabelTest.CharLimit(_bigText) / 3));
		_size = new Vector2((_bigText ? 10f : 7f) * (float)text.Length + 10f, _bigText ? 30f : 20f);
		label.text = _text;
		Change();
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		if (!_IsLong)
		{
			if (bumpBehav == null)
			{
				label.color = color;
			}
			else
			{
				label.color = bumpBehav.GetColor(color);
			}
			label.alpha = alpha;
		}
	}

	protected int _IndexOfLineBreakOccurence(int occurence)
	{
		int i = 1;
		int num = 0;
		for (; i <= occurence; i++)
		{
			if ((num = _displayText.IndexOf('\n', num + 1)) == -1)
			{
				break;
			}
			if (i == occurence)
			{
				return num;
			}
		}
		return -1;
	}

	protected internal override void Change()
	{
		_size = new Vector2(Mathf.Max(_size.x, 20f), Mathf.Max(_size.y, 20f));
		base.Change();
		if (bumpBehav != null && bumpBehav.owner == this)
		{
			bumpBehav.Update();
		}
		if (string.IsNullOrEmpty(_text))
		{
			_displayText = "";
		}
		else if (!autoWrap)
		{
			if (_IsLong)
			{
				_displayText = _text;
			}
			else
			{
				_displayText = ((_text.Length < LabelTest.CharLimit(_bigText)) ? _text : _text.Substring(0, LabelTest.CharLimit(_bigText)));
			}
		}
		else
		{
			string text = ((!_IsLong) ? ((_text.Length < LabelTest.CharLimit(_bigText)) ? _text : _text.Substring(0, LabelTest.CharLimit(_bigText))) : _text);
			_displayText = text.WrapText(_bigText, _size.x);
		}
		if (_IsLong)
		{
			return;
		}
		if (GetLineCount() > 10)
		{
			int num = _IndexOfLineBreakOccurence(10);
			if (num > 0)
			{
				_displayText = _displayText.Substring(0, num);
			}
		}
		label.text = _displayText;
		switch (_alignment)
		{
		default:
			label.alignment = FLabelAlignment.Center;
			label.x = base.size.x / 2f;
			break;
		case FLabelAlignment.Left:
			label.alignment = FLabelAlignment.Left;
			label.x = 0f;
			break;
		case FLabelAlignment.Right:
			label.alignment = FLabelAlignment.Right;
			label.x = base.size.x;
			break;
		}
		float num2 = LabelTest.LineHeight(_bigText) * (float)GetLineCount();
		FLabel fLabel = label;
		fLabel.y = _verticalAlignment switch
		{
			LabelVAlignment.Top => base.size.y - num2 / 2f, 
			LabelVAlignment.Bottom => num2 / 2f, 
			_ => base.size.y / 2f, 
		};
	}

	public int GetLineCount()
	{
		return _displayText.Split('\n').Length;
	}

	public virtual Vector2 GetDisplaySize()
	{
		return new Vector2(label.textRect.width, label.textRect.height);
	}
}
