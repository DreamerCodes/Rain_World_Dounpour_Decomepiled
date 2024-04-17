using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace Menu.Remix.MixedUI;

public class OpColorPicker : UIconfig
{
	protected enum MiniFocus
	{
		ModeRGB = -3,
		ModeHSL = -2,
		ModePLT = -1,
		RGB_Red = 1,
		RGB_Green = 2,
		RGB_Blue = 3,
		HSL_Hue = 11,
		HSL_Saturation = 12,
		HSL_Lightness = 13,
		PLT_Selector = 21,
		HEX = 31,
		None = 99
	}

	protected enum PickerMode
	{
		RGB,
		HSL,
		Palette
	}

	public class Queue : ConfigQueue
	{
		protected override float sizeY => 150f;

		public Queue(Configurable<Color> config, object sign = null)
			: base(config, sign)
		{
		}

		protected override List<UIelement> _InitializeThisQueue(IHoldUIelements holder, float posX, ref float offsetY)
		{
			offsetY += sizeY;
			float posY = GetPosY(holder, offsetY);
			List<UIelement> list = new List<UIelement>();
			OpColorPicker opColorPicker = new OpColorPicker(config as Configurable<Color>, new Vector2(posX, posY))
			{
				sign = sign
			};
			if (onChange != null)
			{
				opColorPicker.OnChange += onChange;
			}
			if (onHeld != null)
			{
				opColorPicker.OnHeld += onHeld;
			}
			if (onValueUpdate != null)
			{
				opColorPicker.OnValueUpdate += onValueUpdate;
			}
			if (onValueChanged != null)
			{
				opColorPicker.OnValueChanged += onValueChanged;
			}
			mainFocusable = opColorPicker;
			list.Add(opColorPicker);
			if (!string.IsNullOrEmpty(config.info?.description))
			{
				opColorPicker.description = UIQueue.GetFirstSentence(UIQueue.Translate(config.info.description));
			}
			if (!string.IsNullOrEmpty(config.key))
			{
				OpLabel item = new OpLabel(new Vector2(posX + 160f, posY + 100f), new Vector2(holder.CanvasSize.x - posX - 165f, 50f), UIQueue.Translate(config.key), FLabelAlignment.Left)
				{
					autoWrap = true,
					bumpBehav = opColorPicker.bumpBehav,
					verticalAlignment = OpLabel.LabelVAlignment.Top,
					description = opColorPicker.description
				};
				list.Add(item);
			}
			opColorPicker.ShowConfig();
			hasInitialized = true;
			return list;
		}
	}

	protected readonly DyeableRect _rect;

	protected FSprite _cursor;

	private int _clickDelay;

	protected FLabel _lblHex;

	protected FLabel _lblRGB;

	protected FLabel _lblHSL;

	protected FLabel _lblPLT;

	protected FLabel _lblR;

	protected FLabel _lblG;

	protected FLabel _lblB;

	protected FLabel _lblP;

	protected FTexture _ftxr1;

	protected FTexture _ftxr2;

	protected FTexture _ftxr3;

	protected Texture2D _ttre1;

	protected Texture2D _ttre2;

	protected Texture2D _ttre3;

	protected FSprite _cdis0;

	protected FSprite _cdis1;

	protected FSprite _sprPltCover;

	protected bool _greyTrigger;

	protected bool _ctor;

	public Color colorEdge = MenuColorEffect.rgbMediumGrey;

	public Color colorText = MenuColorEffect.rgbMediumGrey;

	public Color colorFill = MenuColorEffect.rgbBlack;

	protected bool _typeMode;

	private bool _typed;

	private string _typeHex;

	protected static readonly string[] _allowedHexKeys = new string[16]
	{
		"0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
		"a", "b", "c", "d", "e", "f"
	};

	protected bool _isDirty;

	protected GlowGradient _focusGlow;

	private int _PLTFocus;

	protected MiniFocus _curFocus;

	protected int _r;

	protected int _g;

	protected int _b;

	protected int _h;

	protected int _s;

	protected int _l;

	protected int _pi;

	protected PickerMode _mode = PickerMode.HSL;

	public string[] PaletteHex;

	public string[] PaletteName;

	private static readonly string[] _PaletteHexDefault = new string[154]
	{
		"F0F8FF", "FAEBD7", "00FFFF", "7FFFD4", "F0FFFF", "F5F5DC", "FFE4C4", "000000", "FFEBCD", "0000FF",
		"8A2BE2", "A52A2A", "DEB887", "5F9EA0", "7FFF00", "D2691E", "FF7F50", "6495ED", "FFF8DC", "DC143C",
		"00FFFF", "00008B", "008B8B", "B8860B", "A9A9A9", "006400", "BDB76B", "8B008B", "556B2F", "FF8C00",
		"9932CC", "8B0000", "E9967A", "8FBC8F", "483D8B", "2F4F4F", "00CED1", "9400D3", "FF1493", "00BFFF",
		"696969", "1E90FF", "B22222", "FFFAF0", "228B22", "FF00FF", "DCDCDC", "F8F8FF", "FFD700", "DAA520",
		"808080", "008000", "ADFF2F", "F0FFF0", "FF69B4", "CD5C5C", "4B0082", "FFFFF0", "F0E68C", "E6E6FA",
		"FFF0F5", "7CFC00", "FFFACD", "ADD8E6", "F08080", "E0FFFF", "FAFAD2", "D3D3D3", "D3D3D3", "90EE90",
		"FFB6C1", "FFA07A", "20B2AA", "87CEFA", "778899", "778899", "B0C4DE", "FFFFE0", "00FF00", "32CD32",
		"FAF0E6", "FF00FF", "800000", "66CDAA", "0000CD", "BA55D3", "9370DB", "3CB371", "7B68EE", "00FA9A",
		"48D1CC", "C71585", "191970", "F5FFFA", "FFE4E1", "FFE4B5", "FFDEAD", "000080", "FDF5E6", "808000",
		"6B8E23", "FFA500", "FF4500", "DA70D6", "EEE8AA", "98FB98", "AFEEEE", "DB7093", "FFEFD5", "FFDAB9",
		"CD853F", "FFC0CB", "DDA0DD", "B0E0E6", "800080", "663399", "FF0000", "BC8F8F", "4169E1", "8B4513",
		"FA8072", "F4A460", "2E8B57", "FFF5EE", "A0522D", "C0C0C0", "87CEEB", "6A5ACD", "708090", "FFFAFA",
		"00FF7F", "4682B4", "D2B48C", "008080", "D8BFD8", "FF6347", "40E0D0", "EE82EE", "F5DEB3", "FFFFFF",
		"F5F5F5", "FFFF00", "9ACD32", "FFFFFF", "FFFF73", "FF7373", "010101", "FF66CB", "1B4557", "4F2E69",
		"ABF257", "F0C296", "91CCF0", "70243D"
	};

	private static readonly string[] _PaletteNameDefault = new string[154]
	{
		"AliceBlue", "AntiqueWhite", "Aqua", "Aquamarine", "Azure", "Beige", "Bisque", "Black", "BlanchedAlmond", "Blue",
		"BlueViolet", "Brown", "BurlyWood", "CadetBlue", "Chartreuse", "Chocolate", "Coral", "CornflowerBlue", "Cornsilk", "Crimson",
		"Cyan", "DarkBlue", "DarkCyan", "DarkGoldenRod", "DarkGray", "DarkGreen", "DarkKhaki", "DarkMagenta", "DarkOliveGreen", "DarkOrange",
		"DarkOrchid", "DarkRed", "DarkSalmon", "DarkSeaGreen", "DarkSlateBlue", "DarkSlateGray", "DarkTurquoise", "DarkViolet", "DeepPink", "DeepSkyBlue",
		"DimGray", "DodgerBlue", "FireBrick", "FloralWhite", "ForestGreen", "Fuchsia", "Gainsboro", "GhostWhite", "Gold", "GoldenRod",
		"Gray", "Green", "GreenYellow", "HoneyDew", "HotPink", "IndianRed", "Indigo", "Ivory", "Khaki", "Lavender",
		"LavenderBlush", "LawnGreen", "LemonChiffon", "LightBlue", "LightCoral", "LightCyan", "LightGoldenRodYellow", "LightGray", "LightGrey", "LightGreen",
		"LightPink", "LightSalmon", "LightSeaGreen", "LightSkyBlue", "LightSlateGray", "LightSlateGrey", "LightSteelBlue", "LightYellow", "Lime", "LimeGreen",
		"Linen", "Magenta", "Maroon", "MediumAquaMarine", "MediumBlue", "MediumOrchid", "MediumPurple", "MediumSeaGreen", "MediumSlateBlue", "MediumSpringGreen",
		"MediumTurquoise", "MediumVioletRed", "MidnightBlue", "MintCream", "MistyRose", "Moccasin", "NavajoWhite", "Navy", "OldLace", "Olive",
		"OliveDrab", "Orange", "OrangeRed", "Orchid", "PaleGoldenRod", "PaleGreen", "PaleTurquoise", "PaleVioletRed", "PapayaWhip", "PeachPuff",
		"Peru", "Pink", "Plum", "PowderBlue", "Purple", "RebeccaPurple", "Red", "RosyBrown", "RoyalBlue", "SaddleBrown",
		"Salmon", "SandyBrown", "SeaGreen", "SeaShell", "Sienna", "Silver", "SkyBlue", "SlateBlue", "SlateGray", "Snow",
		"SpringGreen", "SteelBlue", "Tan", "Teal", "Thistle", "Tomato", "Turquoise", "Violet", "Wheat", "White",
		"WhiteSmoke", "Yellow", "YellowGreen", "The Survivor", "The Monk", "The Hunter", "The Nightcat", "5P", "LTTM", "Spearmaster",
		"Saint", "Gourmand", "Rivulet", "Artificer"
	};

	public Color valueColor
	{
		get
		{
			return MenuColorEffect.HexToColor(value);
		}
		set
		{
			this.value = MenuColorEffect.ColorToHex(value);
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
			if (!(base.value == value) && MenuColorEffect.IsStringHexColor(value))
			{
				_r = Mathf.RoundToInt((float)Convert.ToInt32(value.Substring(0, 2), 16) / 255f * 100f);
				_g = Mathf.RoundToInt((float)Convert.ToInt32(value.Substring(2, 2), 16) / 255f * 100f);
				_b = Mathf.RoundToInt((float)Convert.ToInt32(value.Substring(4, 2), 16) / 255f * 100f);
				RXColorHSL rXColorHSL = RXColor.HSLFromColor(new Color((float)_r / 100f, (float)_g / 100f, (float)_b / 100f));
				_h = Mathf.RoundToInt(rXColorHSL.h * 100f);
				_s = Mathf.RoundToInt(rXColorHSL.s * 100f);
				_l = Mathf.RoundToInt(rXColorHSL.l * 100f);
				base.value = value;
				Change();
				if (greyedOut)
				{
					GreyOut();
				}
			}
		}
	}

	public OpColorPicker(Configurable<Color> config, Vector2 pos)
		: base(config, pos, new Vector2(150f, 150f))
	{
		_ctor = false;
		fixedSize = new Vector2(150f, 150f);
		PaletteHex = _PaletteHexDefault;
		PaletteName = _PaletteNameDefault;
		_rect = new DyeableRect(myContainer, Vector2.zero, fixedSize.Value)
		{
			fillAlpha = 0.8f
		};
		_size = fixedSize.Value;
		_focusGlow = new GlowGradient(myContainer, Vector2.zero, 8f, 8f)
		{
			color = colorText
		};
		_focusGlow.Hide();
		_r = 0;
		_g = 0;
		_b = 0;
		_h = 0;
		_s = 0;
		_l = 0;
		_value = "000000";
		_lblB = UIelement.FLabelCreate(_r.ToString());
		_lblB.alignment = FLabelAlignment.Right;
		_lblB.x = 130f;
		_lblB.y = 40f;
		myContainer.AddChild(_lblB);
		_lblG = UIelement.FLabelCreate(_r.ToString());
		_lblG.alignment = FLabelAlignment.Right;
		_lblG.x = 130f;
		_lblG.y = 80f;
		myContainer.AddChild(_lblG);
		_lblR = UIelement.FLabelCreate(_r.ToString());
		_lblR.alignment = FLabelAlignment.Right;
		_lblR.x = 130f;
		_lblR.y = 120f;
		myContainer.AddChild(_lblR);
		_lblP = UIelement.FLabelCreate("X");
		UIelement.FLabelPlaceAtCenter(_lblP, 15f, 88f, 120f, 20f);
		_lblP.isVisible = false;
		_lblP.color = colorText;
		myContainer.AddChild(_lblP);
		_lblHex = UIelement.FLabelCreate(_value);
		_lblHex.alignment = FLabelAlignment.Left;
		_lblHex.x = 60f;
		_lblHex.y = 15f;
		myContainer.AddChild(_lblHex);
		_lblRGB = UIelement.FLabelCreate("RGB");
		UIelement.FLabelPlaceAtCenter(_lblRGB, 20f, 130f, 30f, 15f);
		myContainer.AddChild(_lblRGB);
		_lblHSL = UIelement.FLabelCreate("HSL");
		UIelement.FLabelPlaceAtCenter(_lblHSL, 60f, 130f, 30f, 15f);
		myContainer.AddChild(_lblHSL);
		_lblPLT = UIelement.FLabelCreate("PLT");
		UIelement.FLabelPlaceAtCenter(_lblPLT, 100f, 130f, 30f, 15f);
		myContainer.AddChild(_lblPLT);
		_RecalculateTexture();
		_ftxr1 = new FTexture(_ttre1, "cpk1" + base.Key);
		myContainer.AddChild(_ftxr1);
		_ftxr2 = new FTexture(_ttre2, "cpk2" + base.Key);
		myContainer.AddChild(_ftxr2);
		_ftxr3 = new FTexture(_ttre2, "cpk3" + base.Key);
		myContainer.AddChild(_ftxr3);
		_ftxr1.SetPosition(new Vector2(60f, 80f));
		_ftxr2.SetPosition(new Vector2(140f, 80f));
		_ftxr3.SetPosition(new Vector2(60f, 40f));
		_ftxr3.isVisible = false;
		_cdis0 = new FSprite("pixel")
		{
			color = new Color(0f, 0f, 0f),
			scaleX = 18f,
			scaleY = 12f,
			alpha = 1f,
			x = 135f,
			y = 15f
		};
		myContainer.AddChild(_cdis0);
		_cdis1 = new FSprite("pixel")
		{
			color = new Color(0f, 0f, 0f),
			scaleX = 12f,
			scaleY = 12f,
			alpha = 1f,
			x = 45f,
			y = 15f,
			isVisible = false
		};
		myContainer.AddChild(_cdis1);
		_ctor = true;
		_value = "XXXXXX";
		value = base.defaultValue;
	}

	public override void Reset()
	{
		base.Reset();
		if (_mode != PickerMode.HSL)
		{
			_SwitchMode(PickerMode.HSL);
		}
	}

	protected internal override string DisplayDescription()
	{
		if (!string.IsNullOrEmpty(description))
		{
			return description;
		}
		if (_typeMode)
		{
			return OptionalText.GetText(OptionalText.ID.OpColorPicker_MouseTypeTuto);
		}
		if (base.MenuMouseMode)
		{
			switch (_mode)
			{
			case PickerMode.RGB:
				return OptionalText.GetText(OptionalText.ID.OpColorPicker_MouseRGBTuto);
			case PickerMode.HSL:
				return OptionalText.GetText(OptionalText.ID.OpColorPicker_MouseHSLTuto);
			case PickerMode.Palette:
				return OptionalText.GetText(OptionalText.ID.OpColorPicker_MousePLTTuto);
			}
		}
		if (_curFocus < (MiniFocus)0)
		{
			return OptionalText.GetText(OptionalText.ID.OpColorPicker_NonMouseModeSelect);
		}
		if (_curFocus < (MiniFocus)10)
		{
			return OptionalText.GetText(OptionalText.ID.OpColorPicker_NonMouseSliders);
		}
		if (_curFocus == MiniFocus.PLT_Selector)
		{
			OptionalText.GetText(OptionalText.ID.OpColorPicker_MousePLTTuto);
		}
		return "";
	}

	protected internal override void Reactivate()
	{
		base.Reactivate();
		if (!base.Hidden)
		{
			_RecalculateTexture();
		}
	}

	protected virtual void GreyOut()
	{
		if (_greyTrigger)
		{
			Color color = MenuColorEffect.Greyscale(MenuColorEffect.MidToDark(colorText));
			_lblHex.color = color;
			_lblRGB.color = color;
			_lblHSL.color = color;
			_lblPLT.color = color;
			_rect.colorEdge = color;
			_RecalculateTexture();
			_cdis1.isVisible = false;
			if (_mode == PickerMode.RGB)
			{
				_lblR.color = color;
				_lblG.color = color;
				_lblB.color = color;
				MenuColorEffect.TextureGreyscale(ref _ttre1);
				MenuColorEffect.TextureGreyscale(ref _ttre2);
				MenuColorEffect.TextureGreyscale(ref _ttre3);
				_cdis0.color = new Color((float)_r / 100f, (float)_g / 100f, (float)_b / 100f);
				_ftxr1.SetTexture(_ttre1);
				_ftxr2.SetTexture(_ttre2);
				_ftxr3.SetTexture(_ttre3);
				_ftxr3.SetPosition(new Vector2(60f, 40f));
				_ftxr2.SetPosition(new Vector2(60f, 80f));
				_ftxr1.SetPosition(new Vector2(60f, 120f));
			}
			else if (_mode == PickerMode.HSL)
			{
				_lblR.color = color;
				_lblG.color = color;
				_lblB.color = color;
				MenuColorEffect.TextureGreyscale(ref _ttre1);
				MenuColorEffect.TextureGreyscale(ref _ttre2);
				_cdis0.color = Custom.HSL2RGB((float)_h / 100f, (float)_s / 100f, (float)_l / 100f);
				_ftxr1.SetTexture(_ttre1);
				_ftxr2.SetTexture(_ttre2);
				_ftxr1.SetPosition(new Vector2(60f, 80f));
				_ftxr2.SetPosition(new Vector2(140f, 80f));
				_lblHex.text = "#" + value.ToString();
			}
			else
			{
				_lblP.isVisible = false;
				_sprPltCover.isVisible = false;
				MenuColorEffect.TextureGreyscale(ref _ttre1);
				MenuColorEffect.TextureGreyscale(ref _ttre2);
				_cdis0.color = PaletteColor(_pi);
				_ftxr1.SetTexture(_ttre1);
				_ftxr1.SetPosition(new Vector2(75f, 80f));
				_ftxr2.SetTexture(_ttre2);
				_ftxr2.SetPosition(_GetPICenterPos(_pi));
			}
		}
		else
		{
			_RecalculateTexture();
			Change();
		}
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		_rect.addSize = new Vector2(4f, 4f) * base.bumpBehav.AddSize;
		if (greyedOut)
		{
			_rect.colorEdge = base.bumpBehav.GetColor(colorEdge);
			_rect.colorFill = base.bumpBehav.GetColor(colorFill);
			_rect.GrafUpdate(timeStacker);
			_focusGlow.Hide();
			return;
		}
		Color rgbWhite = MenuColorEffect.rgbWhite;
		_lblRGB.color = colorText;
		_lblHSL.color = colorText;
		_lblPLT.color = colorText;
		_lblR.color = colorText;
		_lblG.color = colorText;
		_lblB.color = colorText;
		_lblP.color = colorText;
		if (_typeMode)
		{
			_lblHex.color = Color.Lerp(rgbWhite, colorText, base.bumpBehav.Sin());
			_cursor.color = Color.Lerp(rgbWhite, colorText, base.bumpBehav.Sin());
		}
		else if (_MouseOverHex())
		{
			_lblHex.color = (Input.GetMouseButton(0) ? MenuColorEffect.MidToDark(colorText) : Color.Lerp(rgbWhite, colorText, base.bumpBehav.Sin(10f)));
		}
		else
		{
			_lblHex.color = colorText;
		}
		_rect.fillAlpha = Mathf.Lerp(0.6f, 0.8f, base.bumpBehav.col);
		if (base.MenuMouseMode && held && !MouseOver)
		{
			_rect.addSize += new Vector2(4f, 4f) * base.bumpBehav.Sin(10f);
		}
		_rect.colorEdge = base.bumpBehav.GetColor(colorEdge);
		_rect.GrafUpdate(timeStacker);
		if (_curFocus == MiniFocus.None)
		{
			_focusGlow.Hide();
			return;
		}
		_focusGlow.Show();
		_focusGlow.color = colorText;
		switch (_curFocus)
		{
		case MiniFocus.ModeRGB:
			if (Input.GetMouseButton(0))
			{
				_lblRGB.color = MenuColorEffect.MidToDark(colorText);
			}
			else
			{
				_lblRGB.color = Color.Lerp(colorText, rgbWhite, base.bumpBehav.Sin(10f));
			}
			_focusGlow.size = new Vector2(40f, 25f);
			_focusGlow.pos = new Vector2(15f, 125f);
			_focusGlow.alpha = base.bumpBehav.Sin(10f) * 0.5f + 0.2f;
			break;
		case MiniFocus.ModeHSL:
			if (Input.GetMouseButton(0))
			{
				_lblHSL.color = MenuColorEffect.MidToDark(colorText);
			}
			else
			{
				_lblHSL.color = Color.Lerp(colorText, rgbWhite, base.bumpBehav.Sin(10f));
			}
			_focusGlow.size = new Vector2(40f, 25f);
			_focusGlow.pos = new Vector2(55f, 125f);
			_focusGlow.alpha = base.bumpBehav.Sin(10f) * 0.5f + 0.2f;
			break;
		case MiniFocus.ModePLT:
			if (Input.GetMouseButton(0))
			{
				_lblPLT.color = MenuColorEffect.MidToDark(colorText);
			}
			else
			{
				_lblPLT.color = Color.Lerp(colorText, rgbWhite, base.bumpBehav.Sin(10f));
			}
			_focusGlow.size = new Vector2(40f, 25f);
			_focusGlow.pos = new Vector2(95f, 125f);
			_focusGlow.alpha = base.bumpBehav.Sin(10f) * 0.5f + 0.2f;
			break;
		case MiniFocus.RGB_Red:
			_lblR.color = Color.Lerp(colorText, rgbWhite, base.bumpBehav.Sin(10f));
			_focusGlow.size = new Vector2(40f, 25f);
			_focusGlow.pos = new Vector2(110f, 105f);
			_focusGlow.alpha = base.bumpBehav.Sin() * 0.4f + 0.1f;
			break;
		case MiniFocus.RGB_Green:
			_lblG.color = Color.Lerp(colorText, rgbWhite, base.bumpBehav.Sin(10f));
			_focusGlow.size = new Vector2(40f, 25f);
			_focusGlow.pos = new Vector2(110f, 65f);
			_focusGlow.alpha = base.bumpBehav.Sin() * 0.4f + 0.1f;
			break;
		case MiniFocus.RGB_Blue:
			_lblB.color = Color.Lerp(colorText, rgbWhite, base.bumpBehav.Sin(10f));
			_focusGlow.size = new Vector2(40f, 25f);
			_focusGlow.pos = new Vector2(110f, 25f);
			_focusGlow.alpha = base.bumpBehav.Sin() * 0.4f + 0.1f;
			break;
		case MiniFocus.HSL_Hue:
			_lblR.color = Color.Lerp(colorText, rgbWhite, base.bumpBehav.Sin(10f));
			if (base.MenuMouseMode)
			{
				_lblG.color = _lblR.color;
			}
			_focusGlow.size = new Vector2(40f, 25f);
			_focusGlow.pos = new Vector2(104f, 105f);
			_focusGlow.alpha = base.bumpBehav.Sin() * 0.4f + 0.1f;
			break;
		case MiniFocus.HSL_Saturation:
			_lblG.color = Color.Lerp(colorText, rgbWhite, base.bumpBehav.Sin(10f));
			_focusGlow.size = new Vector2(40f, 25f);
			_focusGlow.pos = new Vector2(104f, 65f);
			_focusGlow.alpha = base.bumpBehav.Sin() * 0.4f + 0.1f;
			break;
		case MiniFocus.HSL_Lightness:
			_lblB.color = Color.Lerp(colorText, rgbWhite, base.bumpBehav.Sin(10f));
			_focusGlow.size = new Vector2(40f, 25f);
			_focusGlow.pos = new Vector2(104f, 25f);
			_focusGlow.alpha = base.bumpBehav.Sin() * 0.4f + 0.1f;
			break;
		case MiniFocus.PLT_Selector:
			_focusGlow.size = new Vector2(140f, 126f);
			_focusGlow.centerPos = new Vector2(75f, 80f);
			_focusGlow.alpha = base.bumpBehav.Sin() * 0.4f + 0.1f;
			break;
		case MiniFocus.HEX:
			_focusGlow.size = new Vector2(60f, 25f);
			_focusGlow.pos = new Vector2(60f, 5f);
			_focusGlow.alpha = 0.5f - base.bumpBehav.Sin(_typeMode ? 1f : 10f) * 0.4f;
			break;
		default:
			_focusGlow.Hide();
			break;
		}
	}

	protected void _SwitchMode(PickerMode newmod)
	{
		_ftxr1.isVisible = false;
		_ftxr2.alpha = 1f;
		_ftxr2.isVisible = false;
		_ftxr3.isVisible = false;
		_lblR.isVisible = false;
		_lblG.isVisible = false;
		_lblB.isVisible = false;
		_lblP.isVisible = false;
		if (_mode == PickerMode.Palette)
		{
			myContainer.RemoveChild(_sprPltCover);
			_sprPltCover.RemoveFromContainer();
			_sprPltCover = null;
		}
		_ctor = false;
		string text = base.value;
		value = "000000";
		_mode = newmod;
		value = text;
		_mode = newmod;
		_RecalculateTexture();
		switch (_mode)
		{
		case PickerMode.RGB:
			_lblR.isVisible = true;
			_lblG.isVisible = true;
			_lblB.isVisible = true;
			_lblB.x = 136f;
			_lblG.x = 136f;
			_lblR.x = 136f;
			_ftxr1.SetTexture(_ttre1);
			_ftxr2.SetTexture(_ttre2);
			_ftxr3.SetTexture(_ttre3);
			_ftxr1.isVisible = true;
			_ftxr2.isVisible = true;
			_ftxr3.isVisible = true;
			_ftxr3.SetPosition(new Vector2(60f, 40f));
			_ftxr2.SetPosition(new Vector2(60f, 80f));
			_ftxr1.SetPosition(new Vector2(60f, 120f));
			break;
		case PickerMode.HSL:
			_lblR.isVisible = true;
			_lblG.isVisible = true;
			_lblB.isVisible = true;
			_lblB.x = 130f;
			_lblG.x = 130f;
			_lblR.x = 130f;
			_ftxr1.SetTexture(_ttre1);
			_ftxr2.SetTexture(_ttre2);
			_ftxr1.isVisible = true;
			_ftxr2.isVisible = true;
			_ftxr1.SetPosition(new Vector2(60f, 80f));
			_ftxr2.SetPosition(new Vector2(140f, 80f));
			break;
		case PickerMode.Palette:
		{
			int num = int.MaxValue;
			_pi = 0;
			int num2 = Convert.ToInt32(text.Substring(0, 2), 16);
			int num3 = Convert.ToInt32(text.Substring(2, 2), 16);
			int num4 = Convert.ToInt32(text.Substring(4, 2), 16);
			for (int i = 0; i < PaletteHex.Length; i++)
			{
				int num5 = Math.Abs(num2 - Convert.ToInt32(PaletteHex[i].Substring(0, 2), 16));
				int num6 = Math.Abs(num3 - Convert.ToInt32(PaletteHex[i].Substring(2, 2), 16));
				int num7 = Math.Abs(num4 - Convert.ToInt32(PaletteHex[i].Substring(4, 2), 16));
				if (num > num5 + num6 + num7)
				{
					num = num5 + num6 + num7;
					_pi = i;
				}
			}
			_ftxr1.SetTexture(_ttre1);
			_ftxr1.isVisible = true;
			_ftxr1.SetPosition(new Vector2(75f, 80f));
			_sprPltCover = new FSprite("pixel")
			{
				color = colorFill,
				scaleX = 120f,
				scaleY = 48f,
				alpha = 0.7f,
				isVisible = false
			};
			_sprPltCover.SetPosition(new Vector2(75f, 56f));
			myContainer.AddChild(_sprPltCover);
			_ftxr2.SetTexture(_ttre2);
			_ftxr3.SetTexture(_ttre2);
			_ftxr2.isVisible = true;
			_ftxr2.SetPosition(_GetPICenterPos(_pi));
			_cdis1.isVisible = false;
			break;
		}
		}
		_ctor = true;
		Change();
	}

	private static Vector2 _GetPICenterPos(int pi)
	{
		return new Vector2((float)(pi % 15) * 8f + 19f, 125f - (float)Mathf.FloorToInt(pi / 15) * 8f);
	}

	protected bool _MouseOverHex()
	{
		if (base.MousePos.y < 3f || base.MousePos.y > 27f)
		{
			return false;
		}
		if (base.MousePos.x < 55f || base.MousePos.x > 120f)
		{
			return false;
		}
		return true;
	}

	public override void Update()
	{
		base.Update();
		_rect.Update();
		_cdis1.isVisible = false;
		_cdis1.color = _cdis0.color;
		if (greyedOut)
		{
			_curFocus = MiniFocus.None;
			if (!_greyTrigger)
			{
				_greyTrigger = true;
				GreyOut();
			}
			return;
		}
		if (_greyTrigger)
		{
			_greyTrigger = false;
			GreyOut();
		}
		if (_typeMode)
		{
			_curFocus = MiniFocus.HEX;
			held = true;
			ForceMenuMouseMode(true);
			if (!_typed && Input.anyKey)
			{
				_typed = true;
				for (int i = 0; i < _allowedHexKeys.Length; i++)
				{
					if (Input.GetKey(_allowedHexKeys[i]))
					{
						_typeHex += _allowedHexKeys[i].Substring(0, 1).ToUpper();
						_lblHex.text = "#" + _typeHex;
						_cursor.x = 74f + LabelTest.GetWidth(_typeHex);
						PlaySound(SoundID.MENY_Already_Selected_MultipleChoice_Clicked);
						break;
					}
				}
			}
			else if (_typed && !Input.anyKey)
			{
				_typed = false;
			}
			if (_typeHex.Length >= 6)
			{
				if (_mode == PickerMode.Palette)
				{
					_SwitchMode(PickerMode.RGB);
				}
				value = _typeHex;
				_typeMode = false;
				held = false;
				myContainer.RemoveChild(_cursor);
				_cursor = null;
				PlaySound(SoundID.MENU_Player_Unjoin_Game);
			}
			else if (Input.GetMouseButton(0) && !_MouseOverHex())
			{
				if (_mode == PickerMode.Palette)
				{
					_SwitchMode(PickerMode.RGB);
				}
				_lblHex.text = "#" + value;
				_typeMode = false;
				held = false;
				myContainer.RemoveChild(_cursor);
				_cursor = null;
				PlaySound(SoundID.MENU_Player_Unjoin_Game);
			}
		}
		else if (base.MenuMouseMode)
		{
			MouseModeUpdate();
		}
		else
		{
			_NonMouseModeUpdate();
		}
	}

	private void _HSLSetValue()
	{
		Color color = Custom.HSL2RGB((float)_h / 100f, (float)_s / 100f, (float)_l / 100f);
		_r = Mathf.RoundToInt(color.r * 100f);
		_g = Mathf.RoundToInt(color.g * 100f);
		_b = Mathf.RoundToInt(color.b * 100f);
		value = Mathf.RoundToInt((float)_r * 255f / 100f).ToString("X2") + Mathf.RoundToInt((float)_g * 255f / 100f).ToString("X2") + Mathf.RoundToInt((float)_b * 255f / 100f).ToString("X2");
	}

	private void _MouseTrySwitchMode(PickerMode newMode)
	{
		if (_mode != newMode)
		{
			PlaySound(SoundID.MENU_MultipleChoice_Clicked);
			_SwitchMode(newMode);
		}
		else
		{
			PlaySound(SoundID.MENY_Already_Selected_MultipleChoice_Clicked);
		}
	}

	private void _RGBSetValue()
	{
		PlaySound(SoundID.MENU_Scroll_Tick);
		value = Mathf.RoundToInt((float)_r * 255f / 100f).ToString("X2") + Mathf.RoundToInt((float)_g * 255f / 100f).ToString("X2") + Mathf.RoundToInt((float)_b * 255f / 100f).ToString("X2");
	}

	protected virtual void MouseModeUpdate()
	{
		if (_clickDelay > 0)
		{
			_clickDelay--;
		}
		if (!held)
		{
			_curFocus = MiniFocus.None;
			if (MouseOver)
			{
				if (!_isDirty)
				{
					PlaySound(SoundID.MENU_Button_Select_Mouse);
					_isDirty = true;
				}
				if (base.MousePos.y > 135f)
				{
					if (base.MousePos.x > 20f && base.MousePos.x < 50f)
					{
						_curFocus = MiniFocus.ModeRGB;
					}
					else if (base.MousePos.x > 60f && base.MousePos.x < 90f)
					{
						_curFocus = MiniFocus.ModeHSL;
					}
					else if (base.MousePos.x > 100f && base.MousePos.x < 130f)
					{
						_curFocus = MiniFocus.ModePLT;
					}
					if (Input.GetMouseButton(0) && _curFocus != MiniFocus.None)
					{
						held = true;
					}
					return;
				}
				switch (_mode)
				{
				case PickerMode.RGB:
				{
					_lblR.text = _r.ToString();
					_lblG.text = _g.ToString();
					_lblB.text = _b.ToString();
					int num4 = _r;
					int num5 = _g;
					int num6 = _b;
					if (base.MousePos.x >= 10f && base.MousePos.y > 30f && base.MousePos.x <= 110f && base.MousePos.y < 130f)
					{
						if (base.MousePos.y < 50f)
						{
							num6 = Mathf.RoundToInt(base.MousePos.x - 10f);
							_lblB.text = num6.ToString();
							_cdis1.isVisible = true;
							_curFocus = MiniFocus.RGB_Blue;
						}
						else if (base.MousePos.y > 70f && base.MousePos.y < 90f)
						{
							num5 = Mathf.RoundToInt(base.MousePos.x - 10f);
							_lblG.text = num5.ToString();
							_cdis1.isVisible = true;
							_curFocus = MiniFocus.RGB_Green;
						}
						else if (base.MousePos.y > 110f)
						{
							num4 = Mathf.RoundToInt(base.MousePos.x - 10f);
							_lblR.text = num4.ToString();
							_cdis1.isVisible = true;
							_curFocus = MiniFocus.RGB_Red;
						}
					}
					_cdis1.color = new Color((float)num4 / 100f, (float)num5 / 100f, (float)num6 / 100f);
					if (Input.GetMouseButton(0) && _curFocus != MiniFocus.None)
					{
						held = true;
						PlaySound(SoundID.MENU_First_Scroll_Tick);
					}
					break;
				}
				case PickerMode.HSL:
					_lblR.text = _h.ToString();
					_lblG.text = _s.ToString();
					_lblB.text = _l.ToString();
					if (base.MousePos.x > 135f && base.MousePos.x < 145f && base.MousePos.y >= 30f && base.MousePos.y <= 130f)
					{
						_curFocus = MiniFocus.HSL_Lightness;
						int num = Mathf.RoundToInt(base.MousePos.y - 30f);
						_lblB.text = num.ToString();
						_cdis1.color = Custom.HSL2RGB((float)_h / 100f, (float)_s / 100f, (float)num / 100f);
						_cdis1.isVisible = true;
						if (Input.GetMouseButton(0))
						{
							held = true;
							PlaySound(SoundID.MENU_First_Scroll_Tick);
						}
					}
					else if (base.MousePos.x <= 110f && base.MousePos.x >= 10f && base.MousePos.y >= 30f && base.MousePos.y <= 130f)
					{
						_curFocus = MiniFocus.HSL_Hue;
						int num2 = Mathf.RoundToInt(base.MousePos.x - 10f);
						int num3 = Mathf.RoundToInt(base.MousePos.y - 30f);
						_lblR.text = num2.ToString();
						_lblG.text = num3.ToString();
						_cdis1.color = Custom.HSL2RGB((float)num2 / 100f, (float)num3 / 100f, (float)_l / 100f);
						_cdis1.isVisible = true;
						if (Input.GetMouseButton(0))
						{
							held = true;
							PlaySound(SoundID.MENU_First_Scroll_Tick);
						}
					}
					break;
				case PickerMode.Palette:
					if (base.MousePos.x <= 135f && base.MousePos.x >= 15f && base.MousePos.y >= 32f && base.MousePos.y <= 132f)
					{
						_curFocus = MiniFocus.PLT_Selector;
						_lblP.isVisible = true;
						_sprPltCover.isVisible = true;
						_PLTFocus = Custom.IntClamp(Mathf.FloorToInt((132f - base.MousePos.y) / 8f), 0, 12) * 15 + Custom.IntClamp(Mathf.FloorToInt((base.MousePos.x - 15f) / 8f), 0, 14);
						if (_PLTFocus < PaletteHex.Length)
						{
							_ftxr3.isVisible = true;
							_ftxr3.SetPosition(_GetPICenterPos(_PLTFocus));
							_lblP.text = PaletteName[_PLTFocus];
							_cdis1.color = PaletteColor(_PLTFocus);
							_cdis1.isVisible = true;
							if (Input.GetMouseButton(0))
							{
								held = true;
							}
						}
						else
						{
							_ftxr3.isVisible = false;
							_lblP.text = "";
						}
						UIelement.FLabelPlaceAtCenter(_lblP, 15f, (base.MousePos.y < 80f) ? 88f : 52f, 120f, 20f);
						_sprPltCover.x = 75f;
						_sprPltCover.y = ((base.MousePos.y < 80f) ? 104f : 56f);
						_sprPltCover.MoveToFront();
						_lblP.MoveToFront();
					}
					else
					{
						_ftxr3.isVisible = false;
						_cdis1.isVisible = false;
						_lblP.isVisible = false;
						_sprPltCover.isVisible = false;
					}
					break;
				}
				if (!_MouseOverHex())
				{
					return;
				}
				_curFocus = MiniFocus.HEX;
				if (Input.GetMouseButton(0))
				{
					if (!_typed)
					{
						_clickDelay += UIelement.FrameMultiply(60);
						if (_clickDelay > UIelement.FrameMultiply(100))
						{
							_typeMode = true;
							_clickDelay = 0;
							_typed = true;
							_typeHex = "";
							_lblHex.text = "#";
							PlaySound(SoundID.MENU_Player_Join_Game);
							_cursor = new FSprite("modInputCursor")
							{
								x = 73f,
								y = 12f
							};
							myContainer.AddChild(_cursor);
						}
					}
					_typed = true;
				}
				else
				{
					_typed = false;
				}
			}
			else if (_isDirty)
			{
				_cdis1.isVisible = false;
				switch (_mode)
				{
				case PickerMode.RGB:
					_lblR.text = _r.ToString();
					_lblG.text = _g.ToString();
					_lblB.text = _b.ToString();
					break;
				case PickerMode.HSL:
					_lblR.text = _h.ToString();
					_lblG.text = _s.ToString();
					_lblB.text = _l.ToString();
					break;
				case PickerMode.Palette:
					_lblP.isVisible = false;
					_sprPltCover.isVisible = false;
					break;
				}
				_isDirty = false;
			}
			return;
		}
		switch (_curFocus)
		{
		case MiniFocus.ModeRGB:
			if (!Input.GetMouseButton(0))
			{
				if (base.MousePos.y > 135f && base.MousePos.y < 150f && base.MousePos.x > 20f && base.MousePos.x < 50f)
				{
					_MouseTrySwitchMode(PickerMode.RGB);
				}
				held = false;
			}
			break;
		case MiniFocus.ModeHSL:
			if (!Input.GetMouseButton(0))
			{
				if (base.MousePos.y > 135f && base.MousePos.y < 150f && base.MousePos.x > 60f && base.MousePos.x < 90f)
				{
					_MouseTrySwitchMode(PickerMode.HSL);
				}
				held = false;
			}
			break;
		case MiniFocus.ModePLT:
			if (!Input.GetMouseButton(0))
			{
				if (base.MousePos.y > 135f && base.MousePos.y < 150f && base.MousePos.x > 100f && base.MousePos.x < 130f)
				{
					_MouseTrySwitchMode(PickerMode.Palette);
				}
				held = false;
			}
			break;
		case MiniFocus.RGB_Red:
		{
			if (!Input.GetMouseButton(0))
			{
				held = false;
				break;
			}
			int num8 = Mathf.RoundToInt(Mathf.Clamp(base.MousePos.x - 10f, 0f, 100f));
			_cdis1.color = new Color((float)num8 / 100f, (float)_g / 100f, (float)_b / 100f);
			_cdis1.isVisible = true;
			if (_r != num8)
			{
				_r = num8;
				_RGBSetValue();
			}
			break;
		}
		case MiniFocus.RGB_Green:
		{
			if (!Input.GetMouseButton(0))
			{
				held = false;
				break;
			}
			int num9 = Mathf.RoundToInt(Mathf.Clamp(base.MousePos.x - 10f, 0f, 100f));
			_cdis1.color = new Color((float)_r / 100f, (float)num9 / 100f, (float)_b / 100f);
			_cdis1.isVisible = true;
			if (_g != num9)
			{
				_g = num9;
				_RGBSetValue();
			}
			break;
		}
		case MiniFocus.RGB_Blue:
		{
			if (!Input.GetMouseButton(0))
			{
				held = false;
				break;
			}
			int num7 = Mathf.RoundToInt(Mathf.Clamp(base.MousePos.x - 10f, 0f, 100f));
			_cdis1.color = new Color((float)_r / 100f, (float)_g / 100f, (float)num7 / 100f);
			_cdis1.isVisible = true;
			if (_b != num7)
			{
				_b = num7;
				_RGBSetValue();
			}
			break;
		}
		case MiniFocus.HSL_Hue:
		case MiniFocus.HSL_Saturation:
		{
			if (!Input.GetMouseButton(0))
			{
				held = false;
				break;
			}
			int num11 = Mathf.RoundToInt(Mathf.Clamp(base.MousePos.x - 10f, 0f, 99f));
			int num12 = Mathf.RoundToInt(Mathf.Clamp(base.MousePos.y - 30f, 0f, 100f));
			_lblR.text = num11.ToString();
			_lblG.text = num12.ToString();
			_cdis1.color = Custom.HSL2RGB((float)num11 / 100f, (float)num12 / 100f, (float)_l / 100f);
			_cdis1.isVisible = true;
			if (_h != num11 || _s != num12)
			{
				_h = num11;
				_s = num12;
				PlaySound(SoundID.MENU_Scroll_Tick);
				_HSLSetValue();
			}
			break;
		}
		case MiniFocus.HSL_Lightness:
		{
			if (!Input.GetMouseButton(0))
			{
				held = false;
				break;
			}
			int num10 = Mathf.RoundToInt(Mathf.Clamp(base.MousePos.y - 30f, 0f, 100f));
			_lblB.text = num10.ToString();
			_cdis1.color = Custom.HSL2RGB((float)_h / 100f, (float)_s / 100f, (float)num10 / 100f);
			_cdis1.isVisible = true;
			if (_l != num10)
			{
				_l = num10;
				PlaySound(SoundID.MENU_Scroll_Tick);
				_HSLSetValue();
			}
			break;
		}
		case MiniFocus.PLT_Selector:
			if (!Input.GetMouseButton(0))
			{
				held = false;
				break;
			}
			_lblP.isVisible = true;
			_sprPltCover.isVisible = true;
			_PLTFocus = Custom.IntClamp(Mathf.FloorToInt((132f - base.MousePos.y) / 8f), 0, 12) * 15 + Custom.IntClamp(Mathf.FloorToInt((base.MousePos.x - 15f) / 8f), 0, 14);
			if (_PLTFocus < PaletteHex.Length)
			{
				_ftxr3.isVisible = true;
				_ftxr3.SetPosition(_GetPICenterPos(_PLTFocus));
				_lblP.text = PaletteName[_PLTFocus];
				_cdis1.color = PaletteColor(_PLTFocus);
				_cdis1.isVisible = true;
				if (_pi != _PLTFocus)
				{
					_pi = _PLTFocus;
					PlaySound(SoundID.Mouse_Scurry);
					value = PaletteHex[_pi];
				}
			}
			else
			{
				_ftxr3.isVisible = false;
				_lblP.text = "";
			}
			UIelement.FLabelPlaceAtCenter(_lblP, 15f, (base.MousePos.y < 80f) ? 88f : 52f, 120f, 20f);
			_sprPltCover.x = 75f;
			_sprPltCover.y = ((base.MousePos.y < 80f) ? 104f : 56f);
			_sprPltCover.MoveToFront();
			_lblP.MoveToFront();
			break;
		default:
			held = false;
			break;
		}
	}

	private void _NonMouseRGBTick(int tick0, bool first)
	{
		switch (_curFocus)
		{
		case MiniFocus.RGB_Red:
		{
			int num2 = Custom.IntClamp(_r + tick0, 0, 100);
			if (num2 == _r)
			{
				PlaySound(first ? SoundID.MENU_Greyed_Out_Button_Select_Gamepad_Or_Keyboard : SoundID.None);
				return;
			}
			_r = num2;
			break;
		}
		case MiniFocus.RGB_Green:
		{
			int num3 = Custom.IntClamp(_g + tick0, 0, 100);
			if (num3 == _g)
			{
				PlaySound(first ? SoundID.MENU_Greyed_Out_Button_Select_Gamepad_Or_Keyboard : SoundID.None);
				return;
			}
			_g = num3;
			break;
		}
		case MiniFocus.RGB_Blue:
		{
			int num = Custom.IntClamp(_b + tick0, 0, 100);
			if (num == _b)
			{
				PlaySound(first ? SoundID.MENU_Greyed_Out_Button_Select_Gamepad_Or_Keyboard : SoundID.None);
				return;
			}
			_b = num;
			break;
		}
		}
		PlaySound(first ? SoundID.MENU_First_Scroll_Tick : SoundID.MENU_Scroll_Tick);
		_RGBSetValue();
	}

	private void _NonMouseHSLTick(int tick1, bool first)
	{
		switch (_curFocus)
		{
		case MiniFocus.HSL_Hue:
		{
			int num2 = Custom.IntClamp(_h + tick1, 0, 100);
			if (num2 == _h)
			{
				PlaySound(first ? SoundID.MENU_Greyed_Out_Button_Select_Gamepad_Or_Keyboard : SoundID.None);
				return;
			}
			_h = num2;
			break;
		}
		case MiniFocus.HSL_Saturation:
		{
			int num3 = Custom.IntClamp(_s + tick1, 0, 100);
			if (num3 == _s)
			{
				PlaySound(first ? SoundID.MENU_Greyed_Out_Button_Select_Gamepad_Or_Keyboard : SoundID.None);
				return;
			}
			_s = num3;
			break;
		}
		case MiniFocus.HSL_Lightness:
		{
			int num = Custom.IntClamp(_l + tick1, 0, 100);
			if (num == _l)
			{
				PlaySound(first ? SoundID.MENU_Greyed_Out_Button_Select_Gamepad_Or_Keyboard : SoundID.None);
				return;
			}
			_l = num;
			break;
		}
		}
		PlaySound(first ? SoundID.MENU_First_Scroll_Tick : SoundID.MENU_Scroll_Tick);
		_HSLSetValue();
	}

	private void _NonMousePLTTick(int tick2, bool first)
	{
		int num = _PLTFocus + tick2;
		if (num < 0 || num >= PaletteHex.Length)
		{
			PlaySound(first ? SoundID.MENU_Greyed_Out_Button_Select_Gamepad_Or_Keyboard : SoundID.None);
			return;
		}
		_PLTFocus = num;
		PlaySound(first ? SoundID.Mouse_Scurry : SoundID.MENU_Scroll_Tick);
	}

	protected virtual void _NonMouseModeUpdate()
	{
		_clickDelay = 20;
		_lblP.isVisible = false;
		if (_mode == PickerMode.Palette)
		{
			_ftxr3.isVisible = false;
			_sprPltCover.isVisible = false;
		}
		if (!held)
		{
			_curFocus = MiniFocus.None;
			lastValue = _value;
			return;
		}
		if (base.bumpBehav.ButtonPress(BumpBehaviour.ButtonType.Throw))
		{
			held = false;
			value = lastValue;
			PlaySound(SoundID.MENU_Checkbox_Uncheck);
			return;
		}
		if (_curFocus < (MiniFocus)0)
		{
			if (base.bumpBehav.JoystickPress(-1, 0) && _curFocus != MiniFocus.ModeRGB)
			{
				_curFocus--;
				PlaySound(SoundID.MENU_Button_Select_Gamepad_Or_Keyboard);
			}
			else if (base.bumpBehav.JoystickPress(1, 0) && _curFocus != MiniFocus.ModePLT)
			{
				_curFocus++;
				PlaySound(SoundID.MENU_Button_Select_Gamepad_Or_Keyboard);
			}
			else if (base.bumpBehav.JoystickPress(0, -1))
			{
				switch (_mode)
				{
				case PickerMode.RGB:
					_curFocus = MiniFocus.RGB_Red;
					break;
				default:
					_curFocus = MiniFocus.HSL_Hue;
					break;
				case PickerMode.Palette:
					_curFocus = MiniFocus.PLT_Selector;
					_PLTFocus = 0;
					break;
				}
				PlaySound(SoundID.MENU_Button_Select_Gamepad_Or_Keyboard);
			}
			else if (base.bumpBehav.JoystickPress(0, 1))
			{
				PlaySound(SoundID.MENU_Greyed_Out_Button_Select_Gamepad_Or_Keyboard);
			}
			else if (base.bumpBehav.ButtonPress(BumpBehaviour.ButtonType.Jump))
			{
				PickerMode pickerMode = _curFocus switch
				{
					MiniFocus.ModeRGB => PickerMode.RGB, 
					MiniFocus.ModePLT => PickerMode.Palette, 
					_ => PickerMode.HSL, 
				};
				if (pickerMode == _mode)
				{
					PlaySound(SoundID.MENY_Already_Selected_MultipleChoice_Clicked);
					return;
				}
				_SwitchMode(pickerMode);
				PlaySound(SoundID.MENU_MultipleChoice_Clicked);
			}
			return;
		}
		switch (_mode)
		{
		case PickerMode.RGB:
		{
			_lblR.text = _r.ToString();
			_lblG.text = _g.ToString();
			_lblB.text = _b.ToString();
			int num2 = base.bumpBehav.JoystickPressAxis(vertical: false);
			if (num2 != 0)
			{
				_NonMouseRGBTick(num2, first: true);
			}
			else
			{
				num2 = base.bumpBehav.JoystickHeldAxis(vertical: false, 3f);
				if (num2 != 0)
				{
					_NonMouseRGBTick(num2, first: false);
				}
			}
			switch (_curFocus)
			{
			case MiniFocus.RGB_Red:
				if (base.bumpBehav.JoystickPress(0, 1))
				{
					_curFocus = MiniFocus.ModeRGB;
					PlaySound(SoundID.MENU_Button_Select_Gamepad_Or_Keyboard);
				}
				else if (base.bumpBehav.JoystickPress(0, -1))
				{
					_curFocus = MiniFocus.RGB_Green;
					PlaySound(SoundID.MENU_Button_Select_Gamepad_Or_Keyboard);
				}
				break;
			case MiniFocus.RGB_Green:
				if (base.bumpBehav.JoystickPress(0, 1))
				{
					_curFocus = MiniFocus.RGB_Red;
					PlaySound(SoundID.MENU_Button_Select_Gamepad_Or_Keyboard);
				}
				else if (base.bumpBehav.JoystickPress(0, -1))
				{
					_curFocus = MiniFocus.RGB_Blue;
					PlaySound(SoundID.MENU_Button_Select_Gamepad_Or_Keyboard);
				}
				break;
			case MiniFocus.RGB_Blue:
				if (base.bumpBehav.JoystickPress(0, 1))
				{
					_curFocus = MiniFocus.RGB_Green;
					PlaySound(SoundID.MENU_Button_Select_Gamepad_Or_Keyboard);
				}
				else if (base.bumpBehav.JoystickPress(0, -1))
				{
					PlaySound(SoundID.MENU_Greyed_Out_Button_Select_Gamepad_Or_Keyboard);
				}
				break;
			}
			if (base.bumpBehav.ButtonPress(BumpBehaviour.ButtonType.Jump))
			{
				lastValue = _value;
				held = false;
				PlaySound(SoundID.MENU_Checkbox_Check);
			}
			break;
		}
		case PickerMode.HSL:
		{
			_lblR.text = _h.ToString();
			_lblG.text = _s.ToString();
			_lblB.text = _l.ToString();
			int num3 = base.bumpBehav.JoystickPressAxis(vertical: false);
			if (num3 != 0)
			{
				_NonMouseHSLTick(num3, first: true);
			}
			else
			{
				num3 = base.bumpBehav.JoystickHeldAxis(vertical: false, 3f);
				if (num3 != 0)
				{
					_NonMouseHSLTick(num3, first: false);
				}
			}
			switch (_curFocus)
			{
			case MiniFocus.HSL_Hue:
				if (base.bumpBehav.JoystickPress(0, 1))
				{
					_curFocus = MiniFocus.ModeHSL;
					PlaySound(SoundID.MENU_Button_Select_Gamepad_Or_Keyboard);
				}
				else if (base.bumpBehav.JoystickPress(0, -1))
				{
					_curFocus = MiniFocus.HSL_Saturation;
					PlaySound(SoundID.MENU_Button_Select_Gamepad_Or_Keyboard);
				}
				break;
			case MiniFocus.HSL_Saturation:
				if (base.bumpBehav.JoystickPress(0, 1))
				{
					_curFocus = MiniFocus.HSL_Hue;
					PlaySound(SoundID.MENU_Button_Select_Gamepad_Or_Keyboard);
				}
				else if (base.bumpBehav.JoystickPress(0, -1))
				{
					_curFocus = MiniFocus.HSL_Lightness;
					PlaySound(SoundID.MENU_Button_Select_Gamepad_Or_Keyboard);
				}
				break;
			case MiniFocus.HSL_Lightness:
				if (base.bumpBehav.JoystickPress(0, 1))
				{
					_curFocus = MiniFocus.HSL_Saturation;
					PlaySound(SoundID.MENU_Button_Select_Gamepad_Or_Keyboard);
				}
				else if (base.bumpBehav.JoystickPress(0, -1))
				{
					PlaySound(SoundID.MENU_Greyed_Out_Button_Select_Gamepad_Or_Keyboard);
				}
				break;
			}
			if (base.bumpBehav.ButtonPress(BumpBehaviour.ButtonType.Jump))
			{
				lastValue = _value;
				held = false;
				PlaySound(SoundID.MENU_Checkbox_Check);
			}
			break;
		}
		case PickerMode.Palette:
		{
			int num = -base.bumpBehav.JoystickPressAxis(vertical: true);
			if (num != 0)
			{
				num *= 15;
				_NonMousePLTTick(num, first: true);
			}
			else
			{
				num = -base.bumpBehav.JoystickHeldAxis(vertical: true, 2f);
				if (num != 0)
				{
					num *= 15;
					_NonMousePLTTick(num, first: false);
				}
			}
			if (num == 0)
			{
				num = base.bumpBehav.JoystickPressAxis(vertical: false);
				if (num != 0)
				{
					_NonMousePLTTick(num, first: true);
				}
				else
				{
					num = base.bumpBehav.JoystickHeldAxis(vertical: false, 2f);
					if (num != 0)
					{
						_NonMousePLTTick(num, first: false);
					}
				}
			}
			_ftxr3.isVisible = true;
			_ftxr3.SetPosition(_GetPICenterPos(_PLTFocus));
			_lblP.text = PaletteName[_PLTFocus];
			_cdis1.color = PaletteColor(_PLTFocus);
			_cdis1.isVisible = true;
			UIelement.FLabelPlaceAtCenter(_lblP, 15f, (_PLTFocus >= 90) ? 88f : 52f, 120f, 20f);
			_sprPltCover.x = 75f;
			_sprPltCover.y = ((_PLTFocus >= 90) ? 104f : 56f);
			_sprPltCover.isVisible = true;
			_lblP.isVisible = true;
			_sprPltCover.MoveToFront();
			_lblP.MoveToFront();
			if (_PLTFocus < 15 && base.bumpBehav.JoystickPress(0, 1))
			{
				_curFocus = MiniFocus.ModePLT;
				PlaySound(SoundID.MENU_Button_Select_Gamepad_Or_Keyboard);
			}
			else if (base.bumpBehav.ButtonPress(BumpBehaviour.ButtonType.Jump))
			{
				if (_pi == _PLTFocus)
				{
					lastValue = _value;
					held = false;
					PlaySound(SoundID.MENU_Checkbox_Check);
				}
				else
				{
					_pi = _PLTFocus;
					value = PaletteHex[_pi];
					PlaySound(SoundID.MENU_MultipleChoice_Clicked);
				}
			}
			break;
		}
		}
	}

	protected internal override void NonMouseSetHeld(bool newHeld)
	{
		base.NonMouseSetHeld(newHeld);
		if (newHeld)
		{
			switch (_mode)
			{
			case PickerMode.RGB:
				_curFocus = MiniFocus.RGB_Red;
				break;
			case PickerMode.HSL:
				_curFocus = MiniFocus.HSL_Hue;
				break;
			case PickerMode.Palette:
				_curFocus = MiniFocus.PLT_Selector;
				_PLTFocus = 0;
				break;
			}
		}
	}

	protected internal override void Change()
	{
		base.Change();
		if (_ctor)
		{
			_RecalculateTexture();
			switch (_mode)
			{
			case PickerMode.RGB:
				_lblR.text = _r.ToString();
				_lblG.text = _g.ToString();
				_lblB.text = _b.ToString();
				_cdis0.color = new Color((float)_r / 100f, (float)_g / 100f, (float)_b / 100f);
				_ftxr1.SetTexture(_ttre1);
				_ftxr2.SetTexture(_ttre2);
				_ftxr3.SetTexture(_ttre3);
				_lblHex.text = "#" + value;
				break;
			case PickerMode.HSL:
				_lblR.text = _h.ToString();
				_lblG.text = _s.ToString();
				_lblB.text = _l.ToString();
				_cdis0.color = Custom.HSL2RGB((float)_h / 100f, (float)_s / 100f, (float)_l / 100f);
				_ftxr1.SetTexture(_ttre1);
				_ftxr2.SetTexture(_ttre2);
				_lblHex.text = "#" + value;
				break;
			case PickerMode.Palette:
				_cdis0.color = PaletteColor(_pi);
				_lblHex.text = "#" + PaletteHex[_pi];
				_ftxr2.SetPosition(_GetPICenterPos(_pi));
				break;
			}
		}
	}

	protected void _RecalculateTexture()
	{
		if (_mode == PickerMode.RGB)
		{
			_ttre1 = new Texture2D(101, 20)
			{
				wrapMode = TextureWrapMode.Clamp,
				filterMode = FilterMode.Point
			};
			_ttre2 = new Texture2D(101, 20)
			{
				wrapMode = TextureWrapMode.Clamp,
				filterMode = FilterMode.Point
			};
			_ttre3 = new Texture2D(101, 20)
			{
				wrapMode = TextureWrapMode.Clamp,
				filterMode = FilterMode.Point
			};
			for (int i = 0; i <= 100; i++)
			{
				for (int j = 0; j < 20; j++)
				{
					Color color = new Color((float)i / 100f, (float)_g / 100f, (float)_b / 100f);
					_ttre1.SetPixel(i, j, color);
					color = new Color((float)_r / 100f, (float)i / 100f, (float)_b / 100f);
					_ttre2.SetPixel(i, j, color);
					color = new Color((float)_r / 100f, (float)_g / 100f, (float)i / 100f);
					_ttre3.SetPixel(i, j, color);
				}
			}
			Color b = new Color(1f - (float)_r / 100f, 1f - (float)_g / 100f, 1f - (float)_b / 100f);
			b = Color.Lerp(Color.white, b, Mathf.Pow(Mathf.Abs(b.grayscale - 0.5f) * 2f, 0.3f));
			for (int k = Math.Max(0, _r - 4); k <= Math.Min(100, _r + 4); k++)
			{
				int num = 5 - Math.Abs(_r - k);
				for (int l = 0; l < num; l++)
				{
					_ttre1.SetPixel(k, l, b);
				}
				for (int m = 20 - num; m < 20; m++)
				{
					_ttre1.SetPixel(k, m, b);
				}
			}
			for (int n = Math.Max(0, _g - 4); n <= Math.Min(100, _g + 4); n++)
			{
				int num2 = 5 - Math.Abs(_g - n);
				for (int num3 = 0; num3 < num2; num3++)
				{
					_ttre2.SetPixel(n, num3, b);
				}
				for (int num4 = 20 - num2; num4 < 20; num4++)
				{
					_ttre2.SetPixel(n, num4, b);
				}
			}
			for (int num5 = Math.Max(0, _b - 4); num5 <= Math.Min(100, _b + 4); num5++)
			{
				int num6 = 5 - Math.Abs(_b - num5);
				for (int num7 = 0; num7 < num6; num7++)
				{
					_ttre3.SetPixel(num5, num7, b);
				}
				for (int num8 = 20 - num6; num8 < 20; num8++)
				{
					_ttre3.SetPixel(num5, num8, b);
				}
			}
			_ttre1.Apply();
			_ttre2.Apply();
			_ttre3.Apply();
			return;
		}
		if (_mode == PickerMode.HSL)
		{
			_ttre1 = new Texture2D(100, 101)
			{
				wrapMode = TextureWrapMode.Clamp,
				filterMode = FilterMode.Point
			};
			_ttre2 = new Texture2D(10, 101)
			{
				wrapMode = TextureWrapMode.Clamp,
				filterMode = FilterMode.Point
			};
			for (int num9 = 0; num9 <= 100; num9++)
			{
				Color color2;
				for (int num10 = 0; num10 < 100; num10++)
				{
					color2 = Custom.HSL2RGB((float)num10 / 100f, (float)num9 / 100f, (float)_l / 100f);
					_ttre1.SetPixel(num10, num9, color2);
				}
				color2 = Custom.HSL2RGB((float)_h / 100f, (float)_s / 100f, (float)num9 / 100f);
				for (int num11 = 0; num11 < 10; num11++)
				{
					_ttre2.SetPixel(num11, num9, color2);
				}
			}
			Color b2 = new Color(1f - (float)_r / 100f, 1f - (float)_g / 100f, 1f - (float)_b / 100f);
			b2 = Color.Lerp(Color.white, b2, Mathf.Pow(Mathf.Abs(b2.grayscale - 0.5f) * 2f, 0.3f));
			for (int num12 = Math.Max(0, _h - 4); num12 <= Math.Min(100, _h + 4); num12++)
			{
				int num13 = 5 - Math.Abs(_h - num12);
				if (_s > 50)
				{
					for (int num14 = 0; num14 < num13; num14++)
					{
						_ttre1.SetPixel(num12, num14, b2);
					}
				}
				else
				{
					for (int num15 = 101 - num13; num15 < 101; num15++)
					{
						_ttre1.SetPixel(num12, num15, b2);
					}
				}
			}
			for (int num16 = Math.Max(0, _s - 4); num16 <= Math.Min(100, _s + 4); num16++)
			{
				int num17 = 5 - Math.Abs(_s - num16);
				if (_h > 50)
				{
					for (int num18 = 0; num18 < num17; num18++)
					{
						_ttre1.SetPixel(num18, num16, b2);
					}
				}
				else
				{
					for (int num19 = 101 - num17; num19 < 101; num19++)
					{
						_ttre1.SetPixel(num19, num16, b2);
					}
				}
			}
			for (int num20 = Math.Max(0, _l - 4); num20 <= Math.Min(100, _l + 4); num20++)
			{
				int num21 = 5 - Math.Abs(_l - num20);
				for (int num22 = 0; num22 < num21; num22++)
				{
					_ttre2.SetPixel(num22, num20, b2);
				}
			}
			_ttre1.Apply();
			_ttre2.Apply();
			return;
		}
		_ttre1 = new Texture2D(120, 96)
		{
			wrapMode = TextureWrapMode.Clamp,
			filterMode = FilterMode.Point
		};
		for (int num23 = 0; num23 < 12; num23++)
		{
			for (int num24 = 0; num24 < 15; num24++)
			{
				int num25 = num23 * 15 + num24;
				for (int num26 = 0; num26 < 8; num26++)
				{
					for (int num27 = 0; num27 < 8; num27++)
					{
						if (num26 == 7 || num27 == 7)
						{
							_ttre1.SetPixel(num24 * 8 + num26, 95 - (num23 * 8 + num27), new Color(0f, 0f, 0f));
						}
						else if (num25 < PaletteHex.Length)
						{
							_ttre1.SetPixel(num24 * 8 + num26, 95 - (num23 * 8 + num27), PaletteColor(num25));
						}
						else
						{
							_ttre1.SetPixel(num24 * 8 + num26, 95 - (num23 * 8 + num27), new Color(0f, 0f, 0f));
						}
					}
				}
			}
		}
		_ttre1.Apply();
		_ttre2 = new Texture2D(11, 11)
		{
			wrapMode = TextureWrapMode.Clamp,
			filterMode = FilterMode.Point
		};
		for (int num28 = 0; num28 < 11; num28++)
		{
			for (int num29 = 0; num29 < 11; num29++)
			{
				if (num28 < 2 || num28 > 7 || num29 < 2 || num29 > 7)
				{
					_ttre1.SetPixel(num28, num29, colorText);
				}
				else
				{
					_ttre2.SetPixel(num28, num29, Color.clear);
				}
			}
		}
		_ttre2.Apply();
	}

	protected internal override void Unload()
	{
		base.Unload();
		myContainer.RemoveChild(_cdis0);
		myContainer.RemoveChild(_cdis1);
		_cdis0.RemoveFromContainer();
		_cdis1.RemoveFromContainer();
		if (_mode == PickerMode.Palette)
		{
			myContainer.RemoveChild(_sprPltCover);
			_sprPltCover.RemoveFromContainer();
		}
		_ftxr1.Destroy();
		_ftxr2.Destroy();
		_ftxr3.Destroy();
		_mode = PickerMode.RGB;
	}

	public Color PaletteColor(int index)
	{
		string obj = PaletteHex[index];
		float r = (float)Convert.ToInt32(obj.Substring(0, 2), 16) / 255f;
		float g = (float)Convert.ToInt32(obj.Substring(2, 2), 16) / 255f;
		float b = (float)Convert.ToInt32(obj.Substring(4, 2), 16) / 255f;
		return new Color(r, g, b);
	}

	protected internal override string CopyToClipboard()
	{
		_typeHex = value;
		_lblHex.text = "#" + _typeHex;
		return value;
	}

	protected internal override bool CopyFromClipboard(string value)
	{
		value = value.Trim().TrimStart('#');
		if (MenuColorEffect.IsStringHexColor(value))
		{
			_typeHex = value.Substring(0, 6).ToUpper();
			_lblHex.text = "#" + _typeHex;
			return true;
		}
		return false;
	}
}
