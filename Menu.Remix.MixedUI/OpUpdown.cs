using System;
using System.Collections.Generic;
using System.Globalization;
using Menu.Remix.MixedUI.ValueTypes;
using RWCustom;
using UnityEngine;

namespace Menu.Remix.MixedUI;

public class OpUpdown : OpTextBox, IValueInt, IValueFloat, IValueType
{
	public class QueueInt : ConfigQueue
	{
		protected override float sizeY => 30f;

		public QueueInt(Configurable<int> config, object sign = null)
			: base(config, sign)
		{
		}

		protected override List<UIelement> _InitializeThisQueue(IHoldUIelements holder, float posX, ref float offsetY)
		{
			offsetY += sizeY;
			float posY = GetPosY(holder, offsetY);
			float width = GetWidth(holder, posX, 300f, 80f);
			List<UIelement> list = new List<UIelement>();
			OpUpdown opUpdown = new OpUpdown(config as Configurable<int>, new Vector2(posX, posY), width)
			{
				sign = sign
			};
			if (onChange != null)
			{
				opUpdown.OnChange += onChange;
			}
			if (onHeld != null)
			{
				opUpdown.OnHeld += onHeld;
			}
			if (onValueUpdate != null)
			{
				opUpdown.OnValueUpdate += onValueUpdate;
			}
			if (onValueChanged != null)
			{
				opUpdown.OnValueChanged += onValueChanged;
			}
			mainFocusable = opUpdown;
			list.Add(opUpdown);
			if (!string.IsNullOrEmpty(config.info?.description))
			{
				opUpdown.description = UIQueue.GetFirstSentence(UIQueue.Translate(config.info.description));
			}
			if (!string.IsNullOrEmpty(config.key))
			{
				OpLabel item = new OpLabel(new Vector2(posX + width + 10f, posY), new Vector2(holder.CanvasSize.x - posX - width - 10f, 30f), UIQueue.Translate(config.key), FLabelAlignment.Left)
				{
					autoWrap = true,
					bumpBehav = opUpdown.bumpBehav,
					description = opUpdown.description
				};
				list.Add(item);
			}
			opUpdown.ShowConfig();
			hasInitialized = true;
			return list;
		}
	}

	public class QueueFloat : ConfigQueue
	{
		protected readonly byte decimalNum;

		protected override float sizeY => 30f;

		public QueueFloat(Configurable<float> config, byte decimalNum = 1, object sign = null)
			: base(config, sign)
		{
			this.decimalNum = decimalNum;
		}

		protected override List<UIelement> _InitializeThisQueue(IHoldUIelements holder, float posX, ref float offsetY)
		{
			offsetY += sizeY;
			float posY = GetPosY(holder, offsetY);
			float width = GetWidth(holder, posX, 300f, 80f);
			List<UIelement> list = new List<UIelement>();
			OpUpdown opUpdown = new OpUpdown(config as Configurable<float>, new Vector2(posX, posY), width, decimalNum)
			{
				sign = sign
			};
			width = Mathf.Max((float)(opUpdown._bumpDeciMax + 2) * LabelTest.CharMean(bigText: false) + 30f, 50f);
			opUpdown.size = new Vector2(width, opUpdown.size.y);
			if (onChange != null)
			{
				opUpdown.OnChange += onChange;
			}
			if (onHeld != null)
			{
				opUpdown.OnHeld += onHeld;
			}
			if (onValueUpdate != null)
			{
				opUpdown.OnValueUpdate += onValueUpdate;
			}
			if (onValueChanged != null)
			{
				opUpdown.OnValueChanged += onValueChanged;
			}
			mainFocusable = opUpdown;
			list.Add(opUpdown);
			if (!string.IsNullOrEmpty(config.info?.description))
			{
				opUpdown.description = UIQueue.GetFirstSentence(UIQueue.Translate(config.info.description));
			}
			if (!string.IsNullOrEmpty(config.key))
			{
				OpLabel item = new OpLabel(new Vector2(posX + width + 10f, posY), new Vector2(holder.CanvasSize.x - posX - width - 10f, 30f), UIQueue.Translate(config.key), FLabelAlignment.Left)
				{
					autoWrap = true,
					bumpBehav = opUpdown.bumpBehav,
					description = opUpdown.description
				};
				list.Add(item);
			}
			opUpdown.ShowConfig();
			hasInitialized = true;
			return list;
		}
	}

	protected FSprite _arrUp;

	protected FSprite _arrDown;

	protected int _iMin = int.MinValue;

	protected int _iMax = int.MaxValue;

	protected float _fMin = -2.1474836E+09f;

	protected float _fMax = 2.1474836E+09f;

	protected internal readonly byte _dNum;

	protected BumpBehaviour _bumpUp;

	protected BumpBehaviour _bumpDown;

	protected int _arrowCounter;

	protected int _bumpCount;

	protected int _bumpDeci;

	protected int _bumpDeciMax;

	private Vector2 _scrollHeldPos;

	private float _scrollHeldTickPos;

	private int _increment = 1;

	private bool lastGreyedOut;

	private float _arrX;

	private float _lastArrX;

	private float _targetArrX;

	public int Increment
	{
		get
		{
			return _increment;
		}
		set
		{
			if (_increment != value && value > 0)
			{
				_increment = value;
				if (IsInt)
				{
					base.valueInt = ClampValue(base.valueInt);
				}
				else
				{
					base.valueFloat = ClampValue(base.valueFloat);
				}
			}
		}
	}

	protected internal override bool held
	{
		get
		{
			return base.held;
		}
		set
		{
			base.held = value;
			_UpdateArrowX();
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
			if (!base._KeyboardOn)
			{
				string text = ((!IsInt) ? Mathf.Clamp(float.Parse(value, NumberFormatInfo.InvariantInfo), _fMin, _fMax).ToString("F" + _dNum, NumberFormatInfo.InvariantInfo) : Custom.IntClamp(int.Parse(value, NumberFormatInfo.InvariantInfo), _iMin, _iMax).ToString(NumberFormatInfo.InvariantInfo));
				base.value = text;
				return;
			}
			int num = _bumpDeciMax + 1;
			if (allowSpace)
			{
				num++;
			}
			if (!IsInt)
			{
				num += 1 + _dNum;
			}
			num = Math.Min(num, Mathf.FloorToInt((base.size.x - 30f) / LabelTest.CharMean(bigText: false)));
			if (value.Length > num)
			{
				base.value = value.Substring(0, num);
			}
			else
			{
				base.value = value;
			}
		}
	}

	public bool IsInt => accept == Accept.Int;

	string IValueType.valueString
	{
		get
		{
			return value;
		}
		set
		{
			this.value = value;
		}
	}

	public OpUpdown(Configurable<int> config, Vector2 pos, float sizeX)
		: base(config, pos, sizeX)
	{
		_dNum = 0;
		_Initialize();
	}

	internal OpUpdown(bool isInt, ConfigurableBase configBase, Vector2 pos, float sizeX)
		: base(configBase, pos, sizeX)
	{
		_dNum = (byte)((!isInt) ? 2u : 0u);
		_Initialize();
	}

	public OpUpdown(Configurable<float> config, Vector2 pos, float sizeX, byte decimalNum = 1)
		: base(config, pos, sizeX)
	{
		_dNum = Math.Min(decimalNum, (byte)9);
		_Initialize();
	}

	private void _Initialize()
	{
		base.alignment = FLabelAlignment.Right;
		mouseOverStopsScrollwheel = true;
		if (cfgEntry.info != null && cfgEntry.info.acceptable != null)
		{
			if (ValueConverter.GetConverter(cfgEntry.settingType).category == ValueConverter.TypeCategory.Floats)
			{
				_fMin = (float)cfgEntry.info.acceptable.Clamp(float.MinValue);
				_fMax = (float)cfgEntry.info.acceptable.Clamp(float.MaxValue);
				if (_fMin < 0f)
				{
					allowSpace = true;
				}
			}
			else
			{
				_iMin = (int)cfgEntry.info.acceptable.Clamp(int.MinValue);
				_iMax = (int)cfgEntry.info.acceptable.Clamp(int.MaxValue);
				if (_iMin < 0)
				{
					allowSpace = true;
				}
			}
		}
		_bumpDeciMax = Mathf.FloorToInt(Mathf.Log10(IsInt ? ((float)Mathf.Max(Mathf.Abs(_iMax), Mathf.Abs(_iMin))) : Mathf.Max(Mathf.Abs(_fMax), Mathf.Abs(_fMin))));
		_arrUp = new FSprite("Big_Menu_Arrow")
		{
			scale = 0.5f,
			rotation = 0f,
			anchorX = 0.5f,
			anchorY = 0.5f,
			x = base.size.x - 15f,
			y = 20f,
			color = colorText
		};
		_arrDown = new FSprite("Big_Menu_Arrow")
		{
			scale = 0.5f,
			rotation = 180f,
			anchorX = 0.5f,
			anchorY = 0.5f,
			x = base.size.x - 15f,
			y = 10f,
			color = colorText
		};
		myContainer.AddChild(_arrUp);
		myContainer.AddChild(_arrDown);
		_bumpUp = new BumpBehaviour(this);
		_bumpDown = new BumpBehaviour(this);
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
			return OptionalText.GetText(OptionalText.ID.OpUpdown_MouseTuto);
		}
		return OptionalText.GetText(held ? OptionalText.ID.OpUpdown_NonMouseUse : OptionalText.ID.OpUpdown_NonMouseGrab);
	}

	protected internal override void Change()
	{
		base.Change();
		_UpdateArrowX();
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		_arrUp.color = _bumpUp.GetColor(colorText);
		_arrDown.color = _bumpDown.GetColor(colorText);
		_arrUp.x = Mathf.Lerp(_lastArrX, _arrX, timeStacker);
		_arrDown.x = Mathf.Lerp(_lastArrX, _arrX, timeStacker);
		float num = ((held && (!base.MenuMouseMode || (base.MenuMouseMode && _arrowCounter < 0))) ? 20f : 5f);
		_arrUp.y = base.size.y / 2f + num + _bumpUp.AddSize * 5f;
		_arrDown.y = base.size.y / 2f - num - _bumpDown.AddSize * 5f;
	}

	public override void Update()
	{
		_lastArrX = _arrX;
		bool flag = false;
		if (greyedOut)
		{
			_arrX = _targetArrX;
			_bumpUp.held = false;
			_bumpDown.held = false;
			_bumpUp.greyedOut = true;
			_bumpDown.greyedOut = true;
			_bumpUp.Focused = false;
			_bumpDown.Focused = false;
		}
		else
		{
			_arrX = Custom.LerpAndTick(_arrX, _targetArrX, 2f, 0.2f);
			_bumpUp.greyedOut = greyedOut;
			_bumpDown.greyedOut = greyedOut;
			_bumpUp.Focused = false;
			_bumpDown.Focused = false;
			if (!base.MenuMouseMode && base.Focused)
			{
				_bumpDown.Focused = true;
				_bumpUp.Focused = true;
			}
			else if (base.MenuMouseMode && base.MousePos.x > base.size.x - 25f && base.MousePos.x < base.size.x - 5f && base.MousePos.y > 5f)
			{
				if (base.MousePos.y < 15f)
				{
					_bumpDown.Focused = true;
					flag = true;
				}
				else if (base.MousePos.y < 25f)
				{
					_bumpUp.Focused = true;
					flag = true;
				}
			}
		}
		base.Update();
		_bumpUp.Update();
		_bumpDown.Update();
		if (greyedOut)
		{
			if (!lastGreyedOut)
			{
				_UpdateArrowX();
			}
			lastGreyedOut = true;
			return;
		}
		byte b = 200;
		if (base.MenuMouseMode)
		{
			if (held && !base._KeyboardOn)
			{
				_mouseDown = Input.GetMouseButton((_arrowCounter <= 0) ? 2 : 0);
				if (_mouseDown)
				{
					if (_arrowCounter > 0)
					{
						_arrowCounter--;
						if (_arrowCounter < 1)
						{
							_arrowCounter = Math.Max(2, Mathf.RoundToInt((float)ModdingMenu.DASdelay / 2f));
							b = (byte)(_TryBump(_bumpUp.held) ? (_bumpUp.held ? 1u : 2u) : 0u);
							if (_bumpCount >= 10 && Increment < 2)
							{
								_bumpCount = 1;
								_bumpDeci = Math.Min(_bumpDeci + 1, _bumpDeciMax);
							}
						}
					}
					else
					{
						int num = Custom.IntClamp(Mathf.FloorToInt((Mathf.Abs(base.MousePos.x - _scrollHeldPos.x) - 50f) / 30f) - _dNum, -_dNum, _bumpDeciMax);
						if (_bumpDeci != num)
						{
							_bumpDeci = num;
							_UpdateArrowX();
						}
						if (Mathf.Abs(base.MousePos.y - _scrollHeldTickPos) > 10f)
						{
							bool flag2 = base.MousePos.y - _scrollHeldTickPos > 0f;
							_scrollHeldTickPos = base.MousePos.y;
							if (IsInt)
							{
								int num2 = base.valueInt;
								base.valueInt = ClampValue(num2 + Mathf.RoundToInt(Mathf.Pow(10f, _bumpDeci) * (float)Increment) * (flag2 ? 1 : (-1)));
								b = (byte)((num2 != base.valueInt) ? ((byte)(flag2 ? 1u : 2u)) : 0);
							}
							else
							{
								float num3 = base.valueFloat;
								base.valueFloat = ClampValue(num3 + Mathf.Pow(10f, _bumpDeci) * (float)Increment * (flag2 ? 1f : (-1f)));
								b = (byte)((!Mathf.Approximately(base.valueFloat, num3)) ? ((byte)(flag2 ? 1u : 2u)) : 0);
							}
						}
					}
				}
				else
				{
					held = false;
					_arrowCounter = 0;
					_bumpUp.held = false;
					_bumpDown.held = false;
				}
			}
			else if (MouseOver && base.Menu.mouseScrollWheelMovement != 0)
			{
				_bumpDeci = ((!IsInt) ? (-_dNum) : 0);
				b = (byte)(_TryBump((float)base.Menu.mouseScrollWheelMovement < 0f) ? (((float)base.Menu.mouseScrollWheelMovement < 0f) ? 1u : 2u) : 0u);
			}
			if (MouseOver && !_mouseDown)
			{
				if (flag && Input.GetMouseButton(0))
				{
					_arrowCounter = ModdingMenu.DASinit;
					_mouseDown = true;
					held = true;
					if (_bumpUp.Focused)
					{
						_bumpUp.held = true;
						_bumpUp.flash = 2f;
					}
					else
					{
						_bumpDown.held = true;
						_bumpDown.flash = 2f;
					}
					_bumpCount = 0;
					_bumpDeci = ((!IsInt) ? (-_dNum) : 0);
					b = (byte)(_TryBump(_bumpUp.held) ? (_bumpUp.held ? 1u : 2u) : 0u);
				}
				else if (Input.GetMouseButton(2))
				{
					_arrowCounter = -1;
					_mouseDown = true;
					held = true;
					_scrollHeldPos = base.MousePos;
					_scrollHeldTickPos = _scrollHeldPos.y;
					PlaySound(SoundID.MENU_First_Scroll_Tick);
				}
			}
		}
		else
		{
			_bumpUp.held = false;
			_bumpDown.held = false;
			if (held)
			{
				if (base.bumpBehav.ButtonPress(BumpBehaviour.ButtonType.Jump))
				{
					held = false;
					PlaySound(SoundID.MENU_Checkbox_Check);
					return;
				}
				if (base.bumpBehav.ButtonPress(BumpBehaviour.ButtonType.Throw))
				{
					value = lastValue;
					held = false;
					PlaySound(SoundID.MENU_Checkbox_Uncheck);
					return;
				}
				int num4 = base.bumpBehav.JoystickPressAxis(vertical: false);
				if (num4 != 0)
				{
					int num5 = Custom.IntClamp(_bumpDeci - num4, -_dNum, _bumpDeciMax);
					if (_bumpDeci != num5)
					{
						_bumpDeci = num5;
						PlaySound(SoundID.MENU_First_Scroll_Tick);
						_UpdateArrowX();
					}
					else
					{
						PlaySound(SoundID.MENU_Checkbox_Uncheck);
					}
				}
				_arrowCounter = 1;
				num4 = base.bumpBehav.JoystickPressAxis(vertical: true);
				if (num4 != 0)
				{
					if (num4 > 0)
					{
						_bumpUp.held = true;
					}
					else
					{
						_bumpDown.held = true;
					}
					b = (byte)(_TryBump(_bumpUp.held) ? (_bumpUp.held ? 1u : 2u) : 0u);
				}
				num4 = base.bumpBehav.JoystickHeldAxis(vertical: true, 2f);
				if (num4 != 0)
				{
					if (num4 > 0)
					{
						_bumpUp.held = true;
					}
					else
					{
						_bumpDown.held = true;
					}
					_arrowCounter = 0;
					b = (byte)(_TryBump(_bumpUp.held) ? (_bumpUp.held ? 1u : 2u) : 0u);
				}
			}
		}
		if (b >= 200)
		{
			return;
		}
		base.bumpBehav.sizeBump = Mathf.Min(2.5f, (base.bumpBehav.sizeBump + (float)_arrowCounter > 0f) ? 2f : 1f);
		switch (b)
		{
		case 0:
			PlaySound((_arrowCounter > 0) ? SoundID.MENU_Checkbox_Uncheck : SoundID.MENU_Scroll_Tick, 0f, 1f, 0.7f);
			break;
		case 1:
		case 2:
			base.bumpBehav.flash = 1f;
			PlaySound((_arrowCounter > 0) ? SoundID.MENU_First_Scroll_Tick : SoundID.MENU_Scroll_Tick);
			if (b == 1)
			{
				_bumpUp.flash += 0.7f;
				_bumpUp.sizeBump = base.bumpBehav.sizeBump + Mathf.Min(1f, (_arrowCounter > 0) ? 0.5f : 0.2f);
			}
			else
			{
				_bumpDown.flash += 0.7f;
				_bumpDown.sizeBump = base.bumpBehav.sizeBump + Mathf.Min(1f, (_arrowCounter > 0) ? 0.5f : 0.2f);
			}
			break;
		}
		bool _TryBump(bool plus)
		{
			if (IsInt)
			{
				int num6 = base.valueInt;
				base.valueInt = ClampValue(num6 + (plus ? 1 : (-1)) * Mathf.RoundToInt(Mathf.Pow(10f, _bumpDeci) * (float)Increment));
				if (num6 != base.valueInt)
				{
					_bumpCount++;
					return true;
				}
				return false;
			}
			float num7 = base.valueFloat;
			base.valueFloat = ClampValue(num7 + (plus ? 1f : (-1f)) * Mathf.Pow(10f, _bumpDeci) * (float)Increment);
			if (!Mathf.Approximately(base.valueFloat, num7))
			{
				_bumpCount++;
				return true;
			}
			return false;
		}
	}

	protected internal override void NonMouseSetHeld(bool newHeld)
	{
		base.NonMouseSetHeld(newHeld);
		_bumpDeci = ((!IsInt) ? (-_dNum) : 0);
		_UpdateArrowX();
	}

	private void _UpdateArrowX()
	{
		if (!held || base._KeyboardOn || (base.MenuMouseMode && _arrowCounter > 0))
		{
			_targetArrX = base.size.x - 15f;
			return;
		}
		string text = "N" + _dNum;
		string text2;
		if (_bumpDeci >= 0)
		{
			float num = Mathf.Min(Mathf.Abs(base.valueFloat), Mathf.Pow(10f, _bumpDeci + 1) - 1f);
			if (num < Mathf.Pow(10f, _bumpDeci))
			{
				num += Mathf.Pow(10f, _bumpDeci);
			}
			text2 = num.ToString(text, NumberFormatInfo.InvariantInfo);
		}
		else
		{
			text2 = base.valueFloat.ToString(text, NumberFormatInfo.InvariantInfo);
			text2 = text2.Substring(text2.Length - _dNum - _bumpDeci - 1);
		}
		switch (base.alignment)
		{
		case FLabelAlignment.Left:
		case FLabelAlignment.Custom:
			_targetArrX = 5f + _curTextWidth;
			break;
		case FLabelAlignment.Center:
			_targetArrX = label.x + _curTextWidth / 2f;
			break;
		case FLabelAlignment.Right:
			_targetArrX = base.size.x - 30f;
			break;
		}
		_targetArrX -= (LabelTest.GetWidth((text2.Length > 0) ? text2.Substring(1) : "") + LabelTest.GetWidth(text2)) / 2f;
	}

	protected internal int ClampValue(int v)
	{
		return Custom.IntClamp((Increment < 2) ? v : ((v + Increment / 2) / Increment * Increment), _iMin, _iMax);
	}

	protected internal float ClampValue(float v)
	{
		if (Increment < 2)
		{
			return Mathf.Clamp(v, _fMin, _fMax);
		}
		float num = (float)Increment * Mathf.Pow(10f, -_dNum);
		return Mathf.Clamp(Mathf.Round(v / num) * num, _fMin, _fMax);
	}

	public void SetValueFloat(float value)
	{
		base.valueFloat = value;
	}

	public float GetValueFloat()
	{
		return base.valueFloat;
	}

	public void SetValueInt(int value)
	{
		base.valueInt = value;
	}

	public int GetValueInt()
	{
		return base.valueInt;
	}
}
