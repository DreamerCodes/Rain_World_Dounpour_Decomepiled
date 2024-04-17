using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using RWCustom;
using UnityEngine;

namespace Menu.Remix.MixedUI;

public class OpTextBox : UIconfig, ICanBeTyped
{
	public enum Accept
	{
		Int,
		Float,
		StringEng,
		StringASCII
	}

	public class Queue : ConfigQueue
	{
		protected override float sizeY => 24f;

		public Queue(ConfigurableBase config, object sign = null)
			: base(config, sign)
		{
		}

		protected override List<UIelement> _InitializeThisQueue(IHoldUIelements holder, float posX, ref float offsetY)
		{
			offsetY += sizeY;
			float posY = GetPosY(holder, offsetY);
			float width = GetWidth(holder, posX, (float)Mathf.Max(6, config.defaultValue.Length) * 2f * LabelTest.CharMean(bigText: false) + 20f, Mathf.Max(80f, (float)config.defaultValue.Length * LabelTest.CharMean(bigText: false) + 20f));
			List<UIelement> list = new List<UIelement>();
			OpTextBox opTextBox = new OpTextBox(config, new Vector2(posX, posY), width)
			{
				sign = sign
			};
			if (onChange != null)
			{
				opTextBox.OnChange += onChange;
			}
			if (onHeld != null)
			{
				opTextBox.OnHeld += onHeld;
			}
			if (onValueUpdate != null)
			{
				opTextBox.OnValueUpdate += onValueUpdate;
			}
			if (onValueChanged != null)
			{
				opTextBox.OnValueChanged += onValueChanged;
			}
			mainFocusable = opTextBox;
			list.Add(opTextBox);
			if (!string.IsNullOrEmpty(config.info?.description))
			{
				opTextBox.description = UIQueue.GetFirstSentence(UIQueue.Translate(config.info.description));
			}
			if (!string.IsNullOrEmpty(config.key))
			{
				OpLabel item = new OpLabel(new Vector2(posX + width + 10f, posY), new Vector2(holder.CanvasSize.x - posX - width - 10f, 30f), UIQueue.Translate(config.key), FLabelAlignment.Left)
				{
					autoWrap = true,
					bumpBehav = opTextBox.bumpBehav,
					description = opTextBox.description
				};
				list.Add(item);
			}
			opTextBox.ShowConfig();
			hasInitialized = true;
			return list;
		}
	}

	public FLabel label;

	public readonly DyeableRect rect;

	protected readonly FSprite _cursor;

	protected float _cursorAlpha;

	protected float _lastCursorAlpha;

	public Color colorEdge = MenuColorEffect.rgbMediumGrey;

	public Color colorText = MenuColorEffect.rgbMediumGrey;

	public Color colorFill = MenuColorEffect.rgbBlack;

	private FLabelAlignment _alignment = FLabelAlignment.Left;

	private float _col;

	protected internal float _curTextWidth;

	protected bool _mouseDown;

	protected bool _keyboardOn;

	public bool password;

	public bool allowSpace;

	public readonly Accept accept;

	protected int _maxLength;

	public bool IsUpdown => this is OpUpdown;

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

	public Action<char> OnKeyDown { get; set; }

	protected bool _KeyboardOn
	{
		get
		{
			return _keyboardOn;
		}
		set
		{
			if (_keyboardOn != value)
			{
				held = value;
				if (_keyboardOn && !value)
				{
					NumberTypingClean();
					_cursor.isVisible = false;
				}
				_keyboardOn = value;
				Change();
			}
		}
	}

	public int maxLength
	{
		get
		{
			return _maxLength;
		}
		set
		{
			if (value >= 2 && _maxLength != value)
			{
				_maxLength = value;
				if (this.value.Length > _maxLength)
				{
					this.value = this.value.Substring(0, _maxLength);
				}
			}
		}
	}

	public int valueInt
	{
		get
		{
			switch (accept)
			{
			case Accept.Int:
			{
				if (!int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result2))
				{
					return 0;
				}
				return result2;
			}
			default:
			{
				float result;
				return Mathf.FloorToInt(float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result) ? result : 0f);
			}
			}
		}
		set
		{
			this.value = value.ToString(CultureInfo.InvariantCulture);
		}
	}

	public float valueFloat
	{
		get
		{
			if (accept != 0)
			{
				_ = 1;
				if (!float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
				{
					return 0f;
				}
				return result;
			}
			if (!int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result2))
			{
				return 0f;
			}
			return result2;
		}
		set
		{
			this.value = value.ToString(CultureInfo.InvariantCulture);
		}
	}

	public override string value
	{
		get
		{
			return base.value;
		}
		set
		{
			if (base.value == value)
			{
				return;
			}
			if (string.IsNullOrEmpty(value))
			{
				value = "";
			}
			else
			{
				if (accept == Accept.Int || accept == Accept.Float || !allowSpace)
				{
					char[] array = value.ToCharArray();
					for (int i = 0; i < array.Length; i++)
					{
						if (char.IsWhiteSpace(array[i]))
						{
							return;
						}
					}
				}
				switch (accept)
				{
				case Accept.Int:
				{
					if (int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var _) || (allowSpace && value == "-"))
					{
						break;
					}
					return;
				}
				case Accept.Float:
				{
					if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var _) || (_keyboardOn && allowSpace && value == "-"))
					{
						break;
					}
					return;
				}
				default:
					if (Regex.IsMatch(value, "^[a-zA-Z/s]+$"))
					{
						break;
					}
					return;
				case Accept.StringASCII:
					if (!Regex.IsMatch(value, "^[ -~/s]+$"))
					{
						return;
					}
					break;
				}
			}
			if (value.Length > maxLength)
			{
				value = value.Substring(0, maxLength);
				if (base.value == value)
				{
					return;
				}
			}
			if (_KeyboardOn && Input.anyKey && !Input.GetKey(KeyCode.Backspace))
			{
				PlaySound(SoundID.MENU_Checkbox_Uncheck);
			}
			base.value = value;
		}
	}

	public OpTextBox(ConfigurableBase config, Vector2 pos, float sizeX)
		: base(config, pos, new Vector2(30f, 24f))
	{
		_size = new Vector2(Mathf.Max(IsUpdown ? 40f : 30f, sizeX), IsUpdown ? 30f : 24f);
		accept = ValueConverter.GetTypeCategory(config.settingType) switch
		{
			ValueConverter.TypeCategory.Integrals => Accept.Int, 
			ValueConverter.TypeCategory.Floats => Accept.Float, 
			ValueConverter.TypeCategory.Unsupported => throw new ElementFormatException("This type of Configurable<T> is unsupported by OpTextBox."), 
			_ => Regex.IsMatch(base.defaultValue, "^[a-zA-Z]+$") ? Accept.StringEng : Accept.StringASCII, 
		};
		_value = base.defaultValue;
		if (accept == Accept.Float || accept == Accept.Int)
		{
			allowSpace = float.Parse(base.defaultValue, NumberStyles.Any, CultureInfo.InvariantCulture) < 0f;
		}
		else
		{
			allowSpace = base.defaultValue.Contains(" ");
		}
		password = false;
		_mouseDown = false;
		rect = new DyeableRect(myContainer, Vector2.zero, base.size)
		{
			fillAlpha = 0.5f
		};
		label = new FLabel(LabelTest.GetFont(bigText: false), base.defaultValue)
		{
			color = colorText,
			alignment = FLabelAlignment.Left,
			anchorX = 0f,
			anchorY = 1f
		};
		myContainer.AddChild(label);
		mute = true;
		maxLength = Mathf.FloorToInt((base.size.x - 20f) / LabelTest.CharMean(bigText: false));
		value = _value;
		mute = false;
		base.defaultValue = value;
		_cursor = new FSprite("modInputCursor");
		_cursor.SetPosition(LabelTest.GetWidth(value) + LabelTest.CharMean(bigText: false), base.size.y * 0.5f);
		myContainer.AddChild(_cursor);
		OnKeyDown = (Action<char>)Delegate.Combine(OnKeyDown, new Action<char>(KeyboardAccept));
		this.Assign();
		Change();
	}

	protected internal override string DisplayDescription()
	{
		if (!string.IsNullOrEmpty(description))
		{
			return description;
		}
		if (base.MenuMouseMode)
		{
			return OptionalText.GetText(held ? OptionalText.ID.OpTextBox_MouseTutoType : OptionalText.ID.OpTextBox_MouseTutoGrab);
		}
		return OptionalText.GetText(OptionalText.ID.OpTextBox_NonMouseTuto);
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		rect.addSize = 4f * base.bumpBehav.AddSize * Vector2.one;
		if (greyedOut)
		{
			label.color = base.bumpBehav.GetColor(colorText);
			_cursor.alpha = 0f;
			rect.colorEdge = base.bumpBehav.GetColor(colorEdge);
			rect.colorFill = base.bumpBehav.GetColor(colorFill);
			rect.GrafUpdate(timeStacker);
			return;
		}
		Color rgbWhite = MenuColorEffect.rgbWhite;
		if (_KeyboardOn)
		{
			_col = Mathf.Min(1f, base.bumpBehav.col + 0.1f);
		}
		else
		{
			_col = Mathf.Max(0f, base.bumpBehav.col - 1f / 30f);
		}
		_cursor.color = Color.Lerp(rgbWhite, colorText, base.bumpBehav.Sin());
		_cursor.alpha = Mathf.Clamp01(Mathf.Lerp(_lastCursorAlpha, _cursorAlpha, timeStacker));
		switch (alignment)
		{
		case FLabelAlignment.Left:
		case FLabelAlignment.Custom:
			_cursor.x = 7f + _curTextWidth + LabelTest.CharMean(bigText: false);
			break;
		case FLabelAlignment.Center:
			_cursor.x = (base.size.x + _curTextWidth + LabelTest.CharMean(bigText: false) - (IsUpdown ? 30f : 0f)) / 2f + 4f;
			break;
		case FLabelAlignment.Right:
			_cursor.x = base.size.x - (_curTextWidth + LabelTest.CharMean(bigText: false) + (IsUpdown ? 31f : 7f));
			break;
		}
		_cursor.y = base.size.y * 0.5f;
		base.bumpBehav.col = _col;
		rect.fillAlpha = Mathf.Lerp(0.5f, 0.8f, base.bumpBehav.col);
		rect.colorEdge = base.bumpBehav.GetColor(colorEdge);
		rect.colorFill = colorFill;
		rect.GrafUpdate(timeStacker);
		label.color = Color.Lerp(colorText, rgbWhite, Mathf.Clamp(base.bumpBehav.flash, 0f, 1f));
	}

	public void KeyboardAccept(char input)
	{
		if (!_keyboardOn)
		{
			return;
		}
		switch (input)
		{
		case '\b':
			_cursorAlpha = 2.5f;
			base.bumpBehav.flash = 2.5f;
			if (value.Length > 0)
			{
				value = value.Substring(0, value.Length - 1);
				PlaySound(SoundID.MENY_Already_Selected_MultipleChoice_Clicked);
			}
			break;
		case '\n':
		case '\r':
			_KeyboardOn = false;
			PlaySound(SoundID.MENU_Checkbox_Check);
			value = value;
			break;
		default:
			_cursorAlpha = 2.5f;
			base.bumpBehav.flash = 2.5f;
			value += input;
			break;
		}
	}

	public override void Update()
	{
		rect.Update();
		_col = base.bumpBehav.col;
		base.Update();
		base.bumpBehav.col = _col;
		if (greyedOut || !base.MenuMouseMode)
		{
			_KeyboardOn = false;
			return;
		}
		_lastCursorAlpha = _cursorAlpha;
		if (_KeyboardOn)
		{
			_cursorAlpha -= 0.05f / UIelement.frameMulti;
			if (_cursorAlpha < -0.5f)
			{
				_cursorAlpha = 2f;
			}
			ForceMenuMouseMode(true);
		}
		else
		{
			_cursorAlpha = 0f;
		}
		if (Input.GetMouseButton(0))
		{
			if ((MouseOver || _KeyboardOn) && (!IsUpdown || !(base.MousePos.x > base.size.x - 30f)))
			{
				_mouseDown = true;
			}
			return;
		}
		if (_mouseDown)
		{
			if (!held && MouseOver && !_KeyboardOn)
			{
				if (IsUpdown && base.MousePos.x > base.size.x - 30f)
				{
					_mouseDown = false;
					return;
				}
				PlaySound(SoundID.MENU_Button_Select_Gamepad_Or_Keyboard);
				_KeyboardOn = true;
				_cursor.isVisible = true;
				_cursorAlpha = 1f;
				_cursor.SetPosition(LabelTest.GetWidth(label.text) + LabelTest.CharMean(bigText: false), base.size.y * 0.5f);
			}
			else if (held && _KeyboardOn && (!MouseOver || (IsUpdown && base.MousePos.x > base.size.x - 30f)))
			{
				_KeyboardOn = false;
				PlaySound(SoundID.MENU_Checkbox_Uncheck);
			}
		}
		_mouseDown = false;
	}

	protected internal override void NonMouseSetHeld(bool newHeld)
	{
		if (IsUpdown)
		{
			base.NonMouseSetHeld(newHeld);
		}
	}

	protected void NumberTypingClean()
	{
		if (accept == Accept.Int)
		{
			if (value.Length < 1 || value.EndsWith("-"))
			{
				value = "0";
			}
			if (int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
			{
				if (IsUpdown)
				{
					result = (this as OpUpdown).ClampValue(result);
				}
				value = result.ToString(NumberFormatInfo.InvariantInfo);
			}
			else
			{
				value = "0";
			}
		}
		else
		{
			if (accept != Accept.Float)
			{
				return;
			}
			if (value.Length < 1 || value.EndsWith("-"))
			{
				value = "0";
			}
			else if (value.EndsWith("."))
			{
				value = value.Substring(0, value.Length - 1);
			}
			if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result2))
			{
				if (IsUpdown)
				{
					result2 = (this as OpUpdown).ClampValue(result2);
				}
				value = result2.ToString(NumberFormatInfo.InvariantInfo);
			}
			else
			{
				value = "0";
			}
		}
	}

	protected internal override void Change()
	{
		_size = new Vector2(Mathf.Max(IsUpdown ? 40f : 30f, _size.x), IsUpdown ? 30f : 24f);
		base.Change();
		if (!password)
		{
			if (IsUpdown && !_KeyboardOn)
			{
				if ((this as OpUpdown).IsInt)
				{
					label.text = valueInt.ToString("N0", NumberFormatInfo.InvariantInfo);
				}
				else
				{
					label.text = valueFloat.ToString("N" + (this as OpUpdown)._dNum, NumberFormatInfo.InvariantInfo);
				}
			}
			else
			{
				label.text = value;
			}
		}
		else
		{
			string text = "";
			for (int i = 0; i < value.Length; i++)
			{
				text += "#";
			}
			label.text = text;
		}
		rect.size = base.size;
		label.alignment = alignment;
		label.y = (IsUpdown ? 23f : 20f) + (InGameTranslator.LanguageID.UsesLargeFont(Custom.rainWorld.inGameTranslator.currentLanguage) ? LabelTest.LineHalfHeight(bigText: false) : 0f);
		switch (alignment)
		{
		case FLabelAlignment.Left:
		case FLabelAlignment.Custom:
			label.x = 5f;
			break;
		case FLabelAlignment.Right:
			label.x = base.size.x - (IsUpdown ? 30f : 5f);
			break;
		case FLabelAlignment.Center:
			label.x = (base.size.x - (IsUpdown ? 30f : 0f)) / 2f;
			break;
		}
		_curTextWidth = LabelTest.GetWidth(label.text);
	}

	protected internal override void Deactivate()
	{
		base.Deactivate();
		_KeyboardOn = false;
	}

	protected internal override void Unload()
	{
		base.Unload();
		this.Unassign();
	}

	protected internal override bool CopyFromClipboard(string value)
	{
		if (!_KeyboardOn)
		{
			return false;
		}
		try
		{
			string text = this.value;
			this.value = value;
			return this.value != text;
		}
		catch
		{
			return false;
		}
	}

	protected internal override string CopyToClipboard()
	{
		_cursorAlpha = 0f;
		_cursor.alpha = 0f;
		return base.CopyToClipboard();
	}
}
