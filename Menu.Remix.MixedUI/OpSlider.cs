using System;
using System.Collections.Generic;
using System.Globalization;
using Menu.Remix.MixedUI.ValueTypes;
using RWCustom;
using UnityEngine;

namespace Menu.Remix.MixedUI;

public class OpSlider : UIconfig, IValueInt, IValueFloat, IValueType
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

		public Queue(ConfigurableBase configIntegral, object sign = null)
			: base(configIntegral, sign)
		{
			if (ValueConverter.GetTypeCategory(configIntegral.settingType) != ValueConverter.TypeCategory.Integrals)
			{
				throw new ArgumentException("OpSlider only accepts integral Configurable.");
			}
			_labeled = !string.IsNullOrEmpty(config.key);
		}

		protected override List<UIelement> _InitializeThisQueue(IHoldUIelements holder, float posX, ref float offsetY)
		{
			offsetY += sizeY;
			float posY = GetPosY(holder, offsetY);
			float width = GetWidth(holder, posX, 500f, 100f);
			List<UIelement> list = new List<UIelement>();
			OpSlider opSlider = new OpSlider(config, new Vector2(posX, posY), (int)width)
			{
				sign = sign
			};
			if (onChange != null)
			{
				opSlider.OnChange += onChange;
			}
			if (onHeld != null)
			{
				opSlider.OnHeld += onHeld;
			}
			if (onValueUpdate != null)
			{
				opSlider.OnValueUpdate += onValueUpdate;
			}
			if (onValueChanged != null)
			{
				opSlider.OnValueChanged += onValueChanged;
			}
			mainFocusable = opSlider;
			list.Add(opSlider);
			if (!string.IsNullOrEmpty(config.info?.description))
			{
				opSlider.description = UIQueue.GetFirstSentence(UIQueue.Translate(config.info.description));
			}
			if (_labeled)
			{
				OpLabel item = new OpLabel(new Vector2(posX, posY + 34f), new Vector2(holder.CanvasSize.x - posX, 30f), UIQueue.Translate(config.key), FLabelAlignment.Left)
				{
					autoWrap = true,
					bumpBehav = opSlider.bumpBehav,
					description = opSlider.description
				};
				list.Add(item);
			}
			opSlider.ShowConfig();
			hasInitialized = true;
			return list;
		}
	}

	public int mousewheelTick;

	public int min;

	public int max;

	public readonly bool vertical;

	protected FSprite[] _lineSprites;

	protected DyeableRect _rect;

	protected FLabel _label;

	protected GlowGradient _labelGlow;

	public bool hideLabel;

	public Color colorEdge = MenuColorEffect.rgbMediumGrey;

	public Color colorFill = MenuColorEffect.rgbBlack;

	public Color colorLine = MenuColorEffect.rgbMediumGrey;

	private int tickHoldStart;

	private int _increment = 1;

	protected bool _IsTick => this is OpSliderTick;

	public int Span => max - min + 1;

	protected float _mul => (vertical ? base.size.y : base.size.x) / (float)(Span - 1);

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
			if (_IsTick)
			{
				_increment = 1;
			}
			else if (_increment != value && value > 0)
			{
				_increment = value;
				this.SetValueInt(ClampValue(this.GetValueInt()));
			}
		}
	}

	public OpSlider(Configurable<int> config, Vector2 pos, float multi = 1f, bool vertical = false)
		: this(config, pos, -1, vertical)
	{
		if (multi <= 0f)
		{
			throw new ArgumentException("multi must be a positive number.");
		}
		if (multi < 1f)
		{
			_increment = Mathf.CeilToInt(1f / multi);
		}
		_size = (this.vertical ? new Vector2(30f, (float)(Span - 1) * multi) : new Vector2((float)(Span - 1) * multi, 30f));
		fixedSize = _size;
		Initialize();
	}

	public OpSlider(Configurable<int> config, Vector2 pos, int length, bool vertical = false)
		: this((ConfigurableBase)config, pos, length, vertical)
	{
	}

	public OpSlider(ConfigurableBase configIntegral, Vector2 pos, int length, bool vertical = false)
		: base(configIntegral, pos, Vector2.one)
	{
		if (ValueConverter.GetTypeCategory(configIntegral.settingType) != ValueConverter.TypeCategory.Integrals)
		{
			throw new ElementFormatException("OpSlider only accepts integral Configurable.");
		}
		this.vertical = vertical;
		if (configIntegral != null && configIntegral.info != null && configIntegral.info.acceptable != null)
		{
			min = (int)configIntegral.info.acceptable.Clamp(int.MinValue);
			max = (int)configIntegral.info.acceptable.Clamp(int.MaxValue);
		}
		else
		{
			min = 0;
			max = (_IsTick ? 15 : 100);
		}
		int span = Span;
		mousewheelTick = ((span <= 5) ? 1 : Math.Max(Mathf.CeilToInt((float)span / 12f), 4));
		if (length >= 0)
		{
			length = Math.Max(length, 30);
			if (length < span - 1)
			{
				_increment = Mathf.CeilToInt((float)length / (float)(span - 1));
			}
			_size = (this.vertical ? new Vector2(30f, length) : new Vector2(length, 30f));
			Initialize();
		}
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
			if (!_IsTick)
			{
				_rect = new DyeableRect(myContainer, new Vector2(4f, -8f), new Vector2(24f, 16f));
			}
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
			if (!_IsTick)
			{
				_rect = new DyeableRect(myContainer, new Vector2(-8f, 4f), new Vector2(16f, 24f));
			}
		}
		if (!_IsTick)
		{
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
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		float num = (greyedOut ? 0f : base.bumpBehav.AddSize);
		int valueInt = this.GetValueInt();
		if (!vertical)
		{
			Vector2 cutPos;
			Vector2 vector;
			if (!_IsTick)
			{
				float num2 = _mul * (float)(valueInt - min);
				if (base.MenuMouseMode && held)
				{
					num2 = Mathf.Clamp(base.MousePos.x, 0f, base.size.x);
				}
				_rect.pos.y = 4f;
				_rect.pos.x = num2 - 8f;
				_rect.addSize = new Vector2(4f, 4f) * num;
				UIelement.FLabelPlaceAtCenter(_label, _rect.pos.x - 14f, _rect.pos.y + 25f, 40f, 20f);
				cutPos = _rect.pos - _rect.addSize / 2f + Vector2.one;
				vector = _rect.size + _rect.addSize - Vector2.one;
			}
			else
			{
				float x = (float)(valueInt - min) * _mul;
				float num3 = Mathf.Clamp(num, 0f, 1f);
				num3 = 1f + num3 * 0.3f;
				cutPos = new Vector2(x, 15f);
				vector = new Vector2(17f * num3, 17f * num3);
				cutPos -= vector / 2f;
			}
			_lineSprites[1].x = 0f;
			_lineSprites[1].y = 15f;
			_lineSprites[1].scaleX = cutPos.x;
			_lineSprites[2].x = base.size.x;
			_lineSprites[2].y = 15f;
			_lineSprites[2].scaleX = base.size.x - (cutPos.x + vector.x);
			_lineSprites[0].x = 0f;
			_lineSprites[0].y = 15f;
			_lineSprites[0].scaleY = 10f + 6f * num;
			_lineSprites[3].x = base.size.x;
			_lineSprites[3].y = 15f;
			_lineSprites[3].scaleY = 10f + 6f * num;
			_LineVisibility(cutPos, vector);
		}
		else
		{
			Vector2 cutPos;
			Vector2 vector;
			if (!_IsTick)
			{
				float num4 = _mul * (float)(valueInt - min);
				if (base.MenuMouseMode && held)
				{
					num4 = Mathf.Clamp(base.MousePos.y, 0f, base.size.y);
				}
				_rect.pos.x = 4f;
				_rect.pos.y = num4 - 8f;
				_rect.addSize = new Vector2(4f, 4f) * num;
				UIelement.FLabelPlaceAtCenter(_label, _rect.pos.x - 10f, _rect.pos.y + 18f, 40f, 20f);
				cutPos = _rect.pos - _rect.addSize / 2f + Vector2.one;
				vector = _rect.size + _rect.addSize - Vector2.one;
			}
			else
			{
				float y = (float)(valueInt - min) * _mul;
				float num5 = Mathf.Clamp(num, 0f, 1f);
				num5 = 1f + num5 * 0.3f;
				cutPos = new Vector2(15f, y);
				vector = new Vector2(17f * num5, 17f * num5);
				cutPos -= vector / 2f;
			}
			_lineSprites[1].y = 0f;
			_lineSprites[1].x = 15f;
			_lineSprites[1].scaleY = cutPos.y;
			_lineSprites[2].y = base.size.y;
			_lineSprites[2].x = 15f;
			_lineSprites[2].scaleY = base.size.y - (cutPos.y + vector.y);
			_lineSprites[0].y = 0f;
			_lineSprites[0].x = 15f;
			_lineSprites[0].scaleX = 10f + 6f * num;
			_lineSprites[3].y = base.size.y;
			_lineSprites[3].x = 15f;
			_lineSprites[3].scaleX = 10f + 6f * num;
			_LineVisibility(cutPos, vector);
		}
		if (!_IsTick)
		{
			_label.isVisible = !hideLabel;
			_labelGlow.centerPos = new Vector2(_label.x, _label.y);
			_labelGlow.alpha = _label.alpha * 0.6f;
		}
		FSprite[] lineSprites;
		if (greyedOut)
		{
			lineSprites = _lineSprites;
			for (int i = 0; i < lineSprites.Length; i++)
			{
				lineSprites[i].color = base.bumpBehav.GetColor(colorLine);
			}
			if (!_IsTick)
			{
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
			}
			return;
		}
		if (!_IsTick)
		{
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
		}
		Color color = base.bumpBehav.GetColor(colorLine);
		lineSprites = _lineSprites;
		for (int i = 0; i < lineSprites.Length; i++)
		{
			lineSprites[i].color = color;
		}
		if (!_IsTick)
		{
			_label.color = Color.Lerp(_rect.colorEdge, Color.white, 0.5f);
		}
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
		if (!_IsTick)
		{
			_rect.Update();
		}
		int valueInt = this.GetValueInt();
		if (valueInt != Custom.IntClamp(valueInt, min, max))
		{
			this.SetValueInt(ClampValue(valueInt));
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
					int num = ClampValue(Mathf.RoundToInt((vertical ? base.MousePos.y : base.MousePos.x) / _mul) + min);
					if (num != this.GetValueInt())
					{
						this.SetValueInt(num);
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
					int v = this.GetValueInt() - (int)Mathf.Sign(base.Menu.mouseScrollWheelMovement) * mousewheelTick * Increment;
					v = ClampValue(v);
					if (v != this.GetValueInt())
					{
						this.SetValueInt(v);
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
				tickHoldStart = this.GetValueInt();
				_Tick(first: true);
				return;
			}
			tick = base.bumpBehav.JoystickHeldAxis(vertical, _IsTick ? 1f : 2f);
			if (tick != 0)
			{
				_Tick(first: false);
			}
		}
		void _Tick(bool first)
		{
			int num2 = Increment;
			if (!_IsTick && Math.Abs(tickHoldStart - this.GetValueInt()) > num2 * Math.Max(10, 2 * mousewheelTick))
			{
				num2 *= mousewheelTick;
			}
			int num3 = ClampValue(this.GetValueInt() + tick * num2);
			if (num3 != this.GetValueInt())
			{
				PlaySound(first ? SoundID.MENU_First_Scroll_Tick : SoundID.MENU_Scroll_Tick);
				this.SetValueInt(num3);
			}
			else
			{
				PlaySound(first ? SoundID.MENU_First_Scroll_Tick : SoundID.None);
			}
		}
	}

	protected int ClampValue(int v)
	{
		return Custom.IntClamp((Increment < 2) ? v : ((v + min + Increment / 2) / Increment * Increment - min), min, max);
	}

	protected internal override void Change()
	{
		base.Change();
		if (MouseOver || held)
		{
			base.bumpBehav.flash = Mathf.Lerp(base.bumpBehav.flash, 1f, 0.4f);
			base.bumpBehav.sizeBump = Mathf.Lerp(base.bumpBehav.sizeBump, 2.5f, 0.4f);
		}
		if (!_IsTick)
		{
			_label.text = this.GetValueInt().ToString("N0", NumberFormatInfo.InvariantInfo);
		}
	}

	protected internal override void Reactivate()
	{
		base.Reactivate();
		if (base.Hidden)
		{
			return;
		}
		float num = (greyedOut ? 0f : base.bumpBehav.AddSize);
		int valueInt = this.GetValueInt();
		Vector2 cutPos;
		Vector2 vector;
		if (!vertical)
		{
			if (!_IsTick)
			{
				float num2 = _mul * (float)(valueInt - min);
				_rect.pos.y = 4f;
				_rect.pos.x = num2 - 8f;
				_rect.addSize = new Vector2(4f, 4f) * num;
				_label.x = _rect.pos.x - 14f;
				_label.y = _rect.pos.y + 25f;
				cutPos = _rect.pos - base.pos;
				vector = _rect.size;
				cutPos -= _rect.addSize / 2f;
				vector += _rect.addSize;
			}
			else
			{
				float x = (float)(valueInt - min) * _mul;
				float num3 = Mathf.Clamp(num, 0f, 1f);
				num3 = 1f + num3 * 0.3f;
				cutPos = new Vector2(x, 15f);
				vector = new Vector2(17f * num3, 17f * num3);
				cutPos -= vector / 2f;
			}
		}
		else if (!_IsTick)
		{
			float num4 = _mul * (float)(valueInt - min);
			_rect.pos.x = 4f;
			_rect.pos.y = num4 - 8f;
			_rect.addSize = new Vector2(4f, 4f) * num;
			_label.x = _rect.pos.x - 10f;
			_label.y = _rect.pos.y + 18f;
			cutPos = _rect.pos - base.pos;
			vector = _rect.size;
			cutPos -= _rect.addSize / 2f;
			vector += _rect.addSize;
		}
		else
		{
			float y = (float)(valueInt - min) * _mul;
			float num5 = Mathf.Clamp(num, 0f, 1f);
			num5 = 1f + num5 * 0.3f;
			cutPos = new Vector2(15f, y);
			vector = new Vector2(17f * num5, 17f * num5);
			cutPos -= vector / 2f;
		}
		_LineVisibility(cutPos, vector);
	}

	protected internal override bool CopyFromClipboard(string value)
	{
		held = false;
		return base.CopyFromClipboard(value);
	}
}
