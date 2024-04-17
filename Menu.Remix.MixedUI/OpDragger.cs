using System;
using System.Collections.Generic;
using Menu.Remix.MixedUI.ValueTypes;
using RWCustom;
using UnityEngine;

namespace Menu.Remix.MixedUI;

public class OpDragger : UIconfig, IValueInt, IValueFloat, IValueType
{
	public class Queue : ConfigQueue
	{
		protected override float sizeY => 24f;

		public Queue(ConfigurableBase configIntegral, object sign = null)
			: base(configIntegral, sign)
		{
			if (ValueConverter.GetTypeCategory(configIntegral.settingType) != ValueConverter.TypeCategory.Integrals)
			{
				throw new ArgumentException("settingType of Configurable<T> must be integrals.");
			}
		}

		protected override List<UIelement> _InitializeThisQueue(IHoldUIelements holder, float posX, ref float offsetY)
		{
			offsetY += sizeY;
			float posY = GetPosY(holder, offsetY);
			List<UIelement> list = new List<UIelement>();
			OpDragger opDragger = new OpDragger(config, new Vector2(posX, posY))
			{
				sign = sign
			};
			if (onChange != null)
			{
				opDragger.OnChange += onChange;
			}
			if (onHeld != null)
			{
				opDragger.OnHeld += onHeld;
			}
			if (onValueUpdate != null)
			{
				opDragger.OnValueUpdate += onValueUpdate;
			}
			if (onValueChanged != null)
			{
				opDragger.OnValueChanged += onValueChanged;
			}
			mainFocusable = opDragger;
			list.Add(opDragger);
			if (!string.IsNullOrEmpty(config.info?.description))
			{
				opDragger.description = UIQueue.GetFirstSentence(UIQueue.Translate(config.info.description));
			}
			if (!string.IsNullOrEmpty(config.key))
			{
				OpLabel item = new OpLabel(new Vector2(posX + 30f, posY), new Vector2(holder.CanvasSize.x - posX - 40f, 24f), UIQueue.Translate(config.key), FLabelAlignment.Left)
				{
					autoWrap = true,
					bumpBehav = opDragger.bumpBehav,
					description = opDragger.description
				};
				list.Add(item);
			}
			opDragger.ShowConfig();
			hasInitialized = true;
			return list;
		}
	}

	public readonly int min;

	public readonly int max;

	private string[] _customText;

	private bool _useCT;

	public DyeableRect rect;

	public FLabel label;

	public Color colorText = MenuColorEffect.rgbMediumGrey;

	public Color colorEdge = MenuColorEffect.rgbMediumGrey;

	public Color colorFill = MenuColorEffect.rgbBlack;

	private float _greyFade;

	private float _savMouse;

	private int _savValue;

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

	public OpDragger(Configurable<int> config, float posX, float posY)
		: this(config, new Vector2(posX, posY))
	{
	}

	public OpDragger(Configurable<int> config, Vector2 pos)
		: this((ConfigurableBase)config, pos)
	{
	}

	public OpDragger(ConfigurableBase configIntegral, Vector2 pos)
		: base(configIntegral, pos, new Vector2(24f, 24f))
	{
		mouseOverStopsScrollwheel = true;
		fixedSize = new Vector2(24f, 24f);
		if (ValueConverter.GetTypeCategory(configIntegral.settingType) != ValueConverter.TypeCategory.Integrals)
		{
			throw new ElementFormatException("settingType of Configurable<T> must be integrals.");
		}
		rect = new DyeableRect(myContainer, Vector2.zero, base.size);
		label = UIelement.FLabelCreate(base.defaultValue);
		UIelement.FLabelPlaceAtCenter(label, 0f, 2f, 24f, 20f);
		myContainer.AddChild(label);
		if (configIntegral.info != null && configIntegral.info.acceptable != null)
		{
			min = (int)configIntegral.info.acceptable.Clamp(int.MinValue);
			max = (int)configIntegral.info.acceptable.Clamp(int.MaxValue);
		}
		else
		{
			min = 0;
			max = 99;
		}
		_useCT = false;
	}

	protected internal override string DisplayDescription()
	{
		if (!string.IsNullOrEmpty(description))
		{
			return description;
		}
		if (base.MenuMouseMode)
		{
			return OptionalText.GetText(OptionalText.ID.OpDragger_MouseTuto);
		}
		return OptionalText.GetText((!held) ? OptionalText.ID.OpDragger_NonMouseGrabTuto : OptionalText.ID.OpDragger_NonMouseUseTuto);
	}

	public void SetCustomText(params string[] newTexts)
	{
		_useCT = true;
		_customText = new string[max - min + 1];
		for (int i = 0; i < _customText.Length; i++)
		{
			_customText[i] = (min + i).ToString();
		}
		for (int j = 0; j < newTexts.Length && j < _customText.Length; j++)
		{
			if (!string.IsNullOrEmpty(newTexts[j]))
			{
				_customText[j] = newTexts[j];
			}
		}
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		rect.addSize = new Vector2(4f, 4f) * base.bumpBehav.AddSize;
		if (greyedOut)
		{
			label.color = base.bumpBehav.GetColor(colorText);
			rect.colorEdge = base.bumpBehav.GetColor(colorEdge);
			rect.colorFill = base.bumpBehav.GetColor(colorFill);
			rect.GrafUpdate(timeStacker);
		}
		else
		{
			rect.colorFill = colorFill;
			_greyFade = Custom.LerpAndTick(_greyFade, held ? 0f : 1f, 0.05f, 0.025f / UIelement.frameMulti);
			rect.fillAlpha = base.bumpBehav.FillAlpha;
			label.color = base.bumpBehav.GetColor(colorText);
			rect.colorEdge = base.bumpBehav.GetColor(colorEdge);
			rect.GrafUpdate(timeStacker);
		}
	}

	public override void Update()
	{
		base.Update();
		rect.Update();
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
					int num = Custom.IntClamp(_savValue + Mathf.FloorToInt((Input.mousePosition.y - _savMouse) / 10f), min, max);
					if (this.GetValueInt() != num)
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
				if (held || !base.Menu.manager.menuesMouseMode || !MouseOver)
				{
					return;
				}
				if (Input.GetMouseButton(0))
				{
					held = true;
					_savMouse = Input.mousePosition.y;
					_savValue = this.GetValueInt();
					PlaySound(SoundID.MENU_First_Scroll_Tick);
				}
				else if (base.Menu.mouseScrollWheelMovement != 0)
				{
					int val = this.GetValueInt() - (int)Mathf.Sign(base.Menu.mouseScrollWheelMovement);
					val = Custom.IntClamp(val, min, max);
					if (val != this.GetValueInt())
					{
						base.bumpBehav.flash = 1f;
						PlaySound(SoundID.MENU_Scroll_Tick);
						base.bumpBehav.sizeBump = Mathf.Min(2.5f, base.bumpBehav.sizeBump + 1f);
						this.SetValueInt(val);
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
				PlaySound(SoundID.MENU_Checkbox_Check);
				held = false;
			}
			else if (base.bumpBehav.ButtonPress(BumpBehaviour.ButtonType.Throw))
			{
				value = lastValue;
				PlaySound(SoundID.MENU_Checkbox_Uncheck);
				held = false;
			}
			else
			{
				if (base.CtlrInput.y == 0)
				{
					return;
				}
				tick = base.bumpBehav.JoystickPressAxis(vertical: true);
				if (tick != 0)
				{
					_TryTick(first: true);
					return;
				}
				tick = base.bumpBehav.JoystickHeldAxis(vertical: true, 3f);
				if (tick != 0)
				{
					_TryTick(first: false);
				}
			}
		}
		void _TryTick(bool first)
		{
			int num2 = Custom.IntClamp(this.GetValueInt() + tick, min, max);
			if (num2 != this.GetValueInt())
			{
				PlaySound(first ? SoundID.MENU_First_Scroll_Tick : SoundID.MENU_Scroll_Tick);
				this.SetValueInt(num2);
			}
			else
			{
				PlaySound(first ? SoundID.MENU_Greyed_Out_Button_Select_Gamepad_Or_Keyboard : SoundID.None);
			}
		}
	}

	protected internal override void Change()
	{
		base.Change();
		if (MouseOver || held)
		{
			base.bumpBehav.sizeBump = Mathf.Min(2.5f, base.bumpBehav.sizeBump + 1f);
			base.bumpBehav.flash = Mathf.Min(1f, base.bumpBehav.flash + 0.5f);
		}
		if (_useCT)
		{
			label.text = _customText[this.GetValueInt() - min];
		}
		else
		{
			label.text = value;
		}
		UIelement.FLabelPlaceAtCenter(label, 0f, 2f, 24f, 20f);
	}
}
