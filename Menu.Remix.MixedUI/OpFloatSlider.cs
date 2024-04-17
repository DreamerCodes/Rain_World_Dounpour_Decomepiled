using System;
using System.Collections.Generic;
using System.Globalization;
using Menu.Remix.MixedUI.ValueTypes;
using UnityEngine;

namespace Menu.Remix.MixedUI;

public class OpFloatSlider : UIconfig, IValueFloat, IValueType
{
	public class Queue : ConfigQueue
	{
		protected readonly bool _labeled;

		protected override float sizeY
		{
			get
			{
				if (!_labeled)
				{
					return 30f;
				}
				return 64f;
			}
		}

		public Queue(ConfigurableBase configFloat, object sign = null)
			: base(configFloat, sign)
		{
			if (ValueConverter.GetTypeCategory(configFloat.settingType) != ValueConverter.TypeCategory.Floats)
			{
				throw new ArgumentException("OpFloatSlider only accepts float Configurable.");
			}
			_labeled = !string.IsNullOrEmpty(config.key);
		}

		protected override List<UIelement> _InitializeThisQueue(IHoldUIelements holder, float posX, ref float offsetY)
		{
			offsetY += sizeY;
			float posY = GetPosY(holder, offsetY);
			float width = GetWidth(holder, posX, 500f, 100f);
			List<UIelement> list = new List<UIelement>();
			OpFloatSlider opFloatSlider = new OpFloatSlider(config, new Vector2(posX, posY), (int)width, 1)
			{
				sign = sign
			};
			if (onChange != null)
			{
				opFloatSlider.OnChange += onChange;
			}
			if (onHeld != null)
			{
				opFloatSlider.OnHeld += onHeld;
			}
			if (onValueUpdate != null)
			{
				opFloatSlider.OnValueUpdate += onValueUpdate;
			}
			if (onValueChanged != null)
			{
				opFloatSlider.OnValueChanged += onValueChanged;
			}
			mainFocusable = opFloatSlider;
			list.Add(opFloatSlider);
			if (!string.IsNullOrEmpty(config.info?.description))
			{
				opFloatSlider.description = UIQueue.GetFirstSentence(UIQueue.Translate(config.info.description));
			}
			if (_labeled)
			{
				OpLabel item = new OpLabel(new Vector2(posX, posY + 34f), new Vector2(holder.CanvasSize.x - posX, 30f), UIQueue.Translate(config.key), FLabelAlignment.Left)
				{
					autoWrap = true,
					bumpBehav = opFloatSlider.bumpBehav,
					description = opFloatSlider.description
				};
				list.Add(item);
			}
			opFloatSlider.ShowConfig();
			hasInitialized = true;
			return list;
		}
	}

	public int mousewheelTick;

	protected internal readonly byte _dNum;

	public float min;

	public float max;

	public readonly bool vertical;

	protected FSprite[] _lineSprites;

	protected DyeableRect _rect;

	protected FLabel _label;

	private GlowGradient _labelGlow;

	public bool hideLabel;

	public Color colorEdge = MenuColorEffect.rgbMediumGrey;

	public Color colorFill = MenuColorEffect.rgbBlack;

	public Color colorLine = MenuColorEffect.rgbMediumGrey;

	private float tickHoldStart;

	private int _increment = 1;

	protected float _mul => (vertical ? base.size.y : base.size.x) / (max - min);

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
				this.SetValueFloat(ClampValue(this.GetValueFloat()));
			}
		}
	}

	public OpFloatSlider(Configurable<float> config, Vector2 pos, int length, byte decimalNum = 1, bool vertical = false)
		: this((ConfigurableBase)config, pos, length, decimalNum, vertical)
	{
	}

	public OpFloatSlider(ConfigurableBase configFloat, Vector2 pos, int length, byte decimalNum = 1, bool vertical = false)
		: base(configFloat, pos, Vector2.one)
	{
		_dNum = decimalNum;
		if (ValueConverter.GetTypeCategory(configFloat.settingType) != ValueConverter.TypeCategory.Floats)
		{
			throw new ElementFormatException("OpFloatSlider only accepts float Configurable.");
		}
		this.vertical = vertical;
		if (configFloat != null && configFloat.info != null && configFloat.info.acceptable != null)
		{
			min = (float)configFloat.info.acceptable.Clamp(float.MinValue);
			max = (float)configFloat.info.acceptable.Clamp(float.MaxValue);
		}
		else
		{
			min = 0f;
			max = 1f;
			_dNum = 2;
		}
		mousewheelTick = 5;
		length = Math.Max(length, 30);
		if ((float)length < (max - min) * Mathf.Pow(10f, (int)_dNum))
		{
			_increment = Mathf.CeilToInt((float)length / (max - min) * Mathf.Pow(10f, (int)_dNum));
		}
		_size = (this.vertical ? new Vector2(30f, length) : new Vector2(length, 30f));
		Initialize();
	}

	protected internal override string DisplayDescription()
	{
		if (!string.IsNullOrEmpty(description))
		{
			return description;
		}
		if (base.MenuMouseMode)
		{
			return OptionalText.GetText(vertical ? OptionalText.ID.OpSlider_MouseTutoVrtc : OptionalText.ID.OpSlider_MouseTutoHrzt);
		}
		return OptionalText.GetText(held ? OptionalText.ID.OpSlider_NonMouseAdjustTuto : OptionalText.ID.OpSlider_NonMouseGrabTuto);
	}

	protected internal virtual void Initialize()
	{
		mouseOverStopsScrollwheel = true;
		_lineSprites = new FSprite[4];
		for (int i = 0; i < _lineSprites.Length; i++)
		{
			_lineSprites[i] = new FSprite("pixel");
			myContainer.AddChild(_lineSprites[i]);
		}
		if (vertical)
		{
			_lineSprites[0].scaleY = 2f;
			_lineSprites[0].scaleX = 6f;
			_lineSprites[1].scaleX = 2f;
			_lineSprites[1].anchorY = 0f;
			_lineSprites[2].scaleX = 2f;
			_lineSprites[2].anchorY = 1f;
			_lineSprites[3].scaleY = 2f;
			_lineSprites[3].scaleX = 6f;
			_rect = new DyeableRect(myContainer, new Vector2(4f, -8f), new Vector2(24f, 16f));
		}
		else
		{
			_lineSprites[0].scaleX = 2f;
			_lineSprites[0].scaleY = 6f;
			_lineSprites[1].scaleY = 2f;
			_lineSprites[1].anchorX = 0f;
			_lineSprites[2].scaleY = 2f;
			_lineSprites[2].anchorX = 1f;
			_lineSprites[3].scaleX = 2f;
			_lineSprites[3].scaleY = 6f;
			_rect = new DyeableRect(myContainer, new Vector2(-8f, 4f), new Vector2(16f, 24f));
		}
		_labelGlow = new GlowGradient(myContainer, Vector2.zero, Vector2.one);
		_label = UIelement.FLabelCreate(value);
		_label.alpha = 0f;
		myContainer.AddChild(_label);
		UIelement.FLabelPlaceAtCenter(_label, _rect.pos, new Vector2(40f, 20f));
		_labelGlow.pos = new Vector2(_label.x, _label.y);
		_labelGlow.radH = 30f;
		_labelGlow.radV = 15f;
		_labelGlow.color = MenuColorEffect.rgbBlack;
		_labelGlow.alpha = 0f;
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		float num = (greyedOut ? 0f : base.bumpBehav.AddSize);
		float valueFloat = this.GetValueFloat();
		if (!vertical)
		{
			float num2 = _mul * (valueFloat - min);
			if (base.MenuMouseMode && held)
			{
				num2 = Mathf.Clamp(base.MousePos.x, 0f, base.size.x);
			}
			_rect.pos.y = 4f;
			_rect.pos.x = num2 - 8f;
			_rect.addSize = new Vector2(4f, 4f) * num;
			UIelement.FLabelPlaceAtCenter(_label, _rect.pos.x - 14f, _rect.pos.y + 25f, 40f, 20f);
			Vector2 cutPos = _rect.pos - _rect.addSize / 2f + Vector2.one;
			Vector2 cutSize = _rect.size + _rect.addSize - Vector2.one;
			_lineSprites[1].x = 0f;
			_lineSprites[1].y = 15f;
			_lineSprites[1].scaleX = cutPos.x;
			_lineSprites[2].x = base.size.x;
			_lineSprites[2].y = 15f;
			_lineSprites[2].scaleX = base.size.x - (cutPos.x + cutSize.x);
			_lineSprites[0].x = 0f;
			_lineSprites[0].y = 15f;
			_lineSprites[0].scaleY = 10f + 6f * num;
			_lineSprites[3].x = base.size.x;
			_lineSprites[3].y = 15f;
			_lineSprites[3].scaleY = 10f + 6f * num;
			_LineVisibility(cutPos, cutSize);
		}
		else
		{
			float num3 = _mul * (valueFloat - min);
			if (base.MenuMouseMode && held)
			{
				num3 = Mathf.Clamp(base.MousePos.y, 0f, base.size.y);
			}
			_rect.pos.x = 4f;
			_rect.pos.y = num3 - 8f;
			_rect.addSize = new Vector2(4f, 4f) * num;
			UIelement.FLabelPlaceAtCenter(_label, _rect.pos.x - 10f, _rect.pos.y + 18f, 40f, 20f);
			Vector2 cutPos = _rect.pos - _rect.addSize / 2f + Vector2.one;
			Vector2 cutSize = _rect.size + _rect.addSize - Vector2.one;
			_lineSprites[1].y = 0f;
			_lineSprites[1].x = 15f;
			_lineSprites[1].scaleY = cutPos.y;
			_lineSprites[2].y = base.size.y;
			_lineSprites[2].x = 15f;
			_lineSprites[2].scaleY = base.size.y - (cutPos.y + cutSize.y);
			_lineSprites[0].y = 0f;
			_lineSprites[0].x = 15f;
			_lineSprites[0].scaleX = 10f + 6f * num;
			_lineSprites[3].y = base.size.y;
			_lineSprites[3].x = 15f;
			_lineSprites[3].scaleX = 10f + 6f * num;
			_LineVisibility(cutPos, cutSize);
		}
		_label.isVisible = !hideLabel;
		_labelGlow.centerPos = new Vector2(_label.x, _label.y);
		_labelGlow.alpha = _label.alpha * 0.6f;
		FSprite[] lineSprites;
		if (greyedOut)
		{
			lineSprites = _lineSprites;
			for (int i = 0; i < lineSprites.Length; i++)
			{
				lineSprites[i].color = base.bumpBehav.GetColor(colorLine);
			}
			_rect.colorFill = base.bumpBehav.GetColor(colorFill);
			_rect.colorEdge = base.bumpBehav.GetColor(colorEdge);
			_label.color = base.bumpBehav.GetColor(colorEdge);
			if (base.Focused || MouseOver)
			{
				_label.alpha = Mathf.Min(0.5f, _label.alpha + 0.05f);
			}
			else
			{
				_label.alpha = Mathf.Max(0f, _label.alpha - 0.1f);
			}
			return;
		}
		if (held || base.Focused || MouseOver)
		{
			_label.alpha = Mathf.Min(_label.alpha + 0.1f, 1f);
		}
		else
		{
			_label.alpha = Mathf.Max(_label.alpha - 0.15f, 0f);
		}
		_rect.colorEdge = base.bumpBehav.GetColor(colorEdge);
		_rect.colorFill = base.bumpBehav.GetColor(held ? colorEdge : colorFill);
		_rect.fillAlpha = (held ? 1f : base.bumpBehav.FillAlpha);
		_rect.GrafUpdate(timeStacker);
		Color color = base.bumpBehav.GetColor(colorLine);
		lineSprites = _lineSprites;
		for (int i = 0; i < lineSprites.Length; i++)
		{
			lineSprites[i].color = color;
		}
		_label.color = Color.Lerp(_rect.colorEdge, Color.white, 0.5f);
	}

	protected virtual void _LineVisibility(Vector2 cutPos, Vector2 cutSize)
	{
		if (!vertical)
		{
			_lineSprites[1].isVisible = cutPos.x > 0f;
			_lineSprites[2].isVisible = base.size.x > cutPos.x + cutSize.x;
			_lineSprites[0].isVisible = cutPos.x > 0f;
			_lineSprites[3].isVisible = base.size.x > cutPos.x + cutSize.x;
		}
		else
		{
			_lineSprites[1].isVisible = cutPos.y > 0f;
			_lineSprites[2].isVisible = base.size.y > cutPos.y + cutSize.y;
			_lineSprites[0].isVisible = cutPos.y > 0f;
			_lineSprites[3].isVisible = base.size.y > cutPos.y + cutSize.y;
		}
	}

	public override void Update()
	{
		base.Update();
		_rect.Update();
		float valueFloat = this.GetValueFloat();
		if (!Mathf.Approximately(valueFloat, Mathf.Clamp(valueFloat, min, max)))
		{
			this.SetValueFloat(ClampValue(valueFloat));
		}
		if (greyedOut)
		{
			return;
		}
		int tick;
		if (base.MenuMouseMode)
		{
			if (held)
			{
				if (Input.GetMouseButton(0))
				{
					float b = ClampValue((vertical ? base.MousePos.y : base.MousePos.x) / _mul + min);
					if (!Mathf.Approximately(this.GetValueFloat(), b))
					{
						this.SetValueFloat(b);
						PlaySound(SoundID.MENU_Scroll_Tick);
					}
				}
				else
				{
					held = false;
				}
			}
			else
			{
				if (held || !MouseOver)
				{
					return;
				}
				if (Input.GetMouseButton(0))
				{
					held = true;
					PlaySound(SoundID.MENU_First_Scroll_Tick);
				}
				else if (base.Menu.mouseScrollWheelMovement != 0)
				{
					float v = this.GetValueFloat() - (float)((int)Mathf.Sign(base.Menu.mouseScrollWheelMovement) * mousewheelTick * Increment) * Mathf.Pow(10f, -_dNum);
					v = ClampValue(v);
					if (!Mathf.Approximately(this.GetValueFloat(), v))
					{
						this.SetValueFloat(v);
						PlaySound(SoundID.MENU_Scroll_Tick);
					}
					else
					{
						PlaySound(SoundID.MENU_First_Scroll_Tick);
					}
				}
			}
		}
		else
		{
			if (!held)
			{
				return;
			}
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
			tick = base.bumpBehav.JoystickPressAxis(vertical);
			if (tick != 0)
			{
				tickHoldStart = this.GetValueFloat();
				_Tick(first: true);
				return;
			}
			tick = base.bumpBehav.JoystickHeldAxis(vertical, 3f);
			if (tick != 0)
			{
				_Tick(first: false);
			}
		}
		void _Tick(bool first)
		{
			float num = (float)Increment * Mathf.Pow(10f, -_dNum);
			if (Math.Abs(tickHoldStart - this.GetValueFloat()) > num * Mathf.Max(10f, 2f * (float)mousewheelTick))
			{
				num *= (float)mousewheelTick;
			}
			float b2 = ClampValue(this.GetValueFloat() + (float)tick * num);
			if (!Mathf.Approximately(this.GetValueFloat(), b2))
			{
				PlaySound(first ? SoundID.MENU_First_Scroll_Tick : SoundID.MENU_Scroll_Tick);
				this.SetValueFloat(b2);
			}
			else
			{
				PlaySound(first ? SoundID.MENU_First_Scroll_Tick : SoundID.None);
			}
		}
	}

	protected float ClampValue(float v)
	{
		if (Increment < 2)
		{
			return Mathf.Clamp(v, min, max);
		}
		float num = (float)Increment * Mathf.Pow(10f, -_dNum);
		return Mathf.Clamp(Mathf.Round(v / num) * num, min, max);
	}

	protected internal override void Change()
	{
		base.Change();
		if (MouseOver || held)
		{
			base.bumpBehav.flash = Mathf.Lerp(base.bumpBehav.flash, 1f, 0.4f);
			base.bumpBehav.sizeBump = Mathf.Lerp(base.bumpBehav.sizeBump, 2.5f, 0.4f);
		}
		_label.text = this.GetValueFloat().ToString("N" + _dNum, NumberFormatInfo.InvariantInfo);
	}

	protected internal override void Reactivate()
	{
		base.Reactivate();
		if (!base.Hidden)
		{
			float num = (greyedOut ? 0f : base.bumpBehav.AddSize);
			float valueFloat = this.GetValueFloat();
			Vector2 cutPos;
			Vector2 cutSize;
			if (!vertical)
			{
				float num2 = _mul * (valueFloat - min);
				_rect.pos.y = 4f;
				_rect.pos.x = num2 - 8f;
				_rect.addSize = new Vector2(4f, 4f) * num;
				_label.x = _rect.pos.x - 14f;
				_label.y = _rect.pos.y + 25f;
				cutPos = _rect.pos - base.pos;
				cutSize = _rect.size;
				cutPos -= _rect.addSize / 2f;
				cutSize += _rect.addSize;
			}
			else
			{
				float num3 = _mul * (valueFloat - min);
				_rect.pos.x = 4f;
				_rect.pos.y = num3 - 8f;
				_rect.addSize = new Vector2(4f, 4f) * num;
				_label.x = _rect.pos.x - 10f;
				_label.y = _rect.pos.y + 18f;
				cutPos = _rect.pos - base.pos;
				cutSize = _rect.size;
				cutPos -= _rect.addSize / 2f;
				cutSize += _rect.addSize;
			}
			_LineVisibility(cutPos, cutSize);
		}
	}

	protected internal override bool CopyFromClipboard(string value)
	{
		held = false;
		return base.CopyFromClipboard(value);
	}
}
