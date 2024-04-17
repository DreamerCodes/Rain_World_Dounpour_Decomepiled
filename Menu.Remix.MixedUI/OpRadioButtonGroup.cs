using System;
using System.Collections.Generic;
using Menu.Remix.MixedUI.ValueTypes;
using UnityEngine;

namespace Menu.Remix.MixedUI;

public class OpRadioButtonGroup : UIconfig, IValueInt, IValueFloat, IValueType
{
	public class Queue : ConfigQueue
	{
		protected readonly bool _labeled;

		protected readonly string[] names;

		protected readonly string[] descriptions;

		protected override float sizeY
		{
			get
			{
				if (!_labeled)
				{
					return 24f;
				}
				return 54f;
			}
		}

		public Queue(Configurable<int> config, string[] names, string[] descriptions = null, object sign = null)
			: base(config, sign)
		{
			if (names == null || names.Length < 2)
			{
				throw new ArgumentNullException("names must contain more than two items");
			}
			if (descriptions != null && descriptions.Length < names.Length)
			{
				throw new ArgumentException("descs must contain the same number of items with names");
			}
			this.names = names;
			this.descriptions = descriptions;
			_labeled = !string.IsNullOrEmpty(config.key);
		}

		protected override List<UIelement> _InitializeThisQueue(IHoldUIelements holder, float posX, ref float offsetY)
		{
			offsetY += sizeY;
			float posY = GetPosY(holder, offsetY);
			float width = GetWidth(holder, posX, (holder.CanvasSize.x - posX) / (float)names.Length, 50f);
			List<UIelement> list = new List<UIelement>();
			OpRadioButtonGroup opRadioButtonGroup = new OpRadioButtonGroup(config as Configurable<int>)
			{
				sign = sign
			};
			if (onChange != null)
			{
				opRadioButtonGroup.OnChange += onChange;
			}
			if (onHeld != null)
			{
				opRadioButtonGroup.OnHeld += onHeld;
			}
			if (onValueUpdate != null)
			{
				opRadioButtonGroup.OnValueUpdate += onValueUpdate;
			}
			if (onValueChanged != null)
			{
				opRadioButtonGroup.OnValueChanged += onValueChanged;
			}
			list.Add(opRadioButtonGroup);
			if (_labeled)
			{
				OpLabel opLabel = new OpLabel(new Vector2(posX, posY + 24f), new Vector2(holder.CanvasSize.x - posX - 10f, 24f), UIQueue.Translate(config.key), FLabelAlignment.Left)
				{
					autoWrap = true,
					bumpBehav = opRadioButtonGroup.bumpBehav
				};
				list.Add(opLabel);
				if (!string.IsNullOrEmpty(config.info?.description))
				{
					opLabel.description = UIQueue.GetFirstSentence(UIQueue.Translate(config.info.description));
				}
			}
			OpRadioButton[] array = new OpRadioButton[names.Length];
			for (int i = 0; i < names.Length; i++)
			{
				OpRadioButton opRadioButton = new OpRadioButton(posX + width * (float)i, posY);
				if (i == 0)
				{
					mainFocusable = opRadioButton;
				}
				OpLabel opLabel2 = new OpLabel(new Vector2(posX + width * (float)i + 30f, posY), new Vector2(width - 30f, 24f), UIQueue.Translate(names[i]), FLabelAlignment.Left)
				{
					autoWrap = true,
					bumpBehav = opRadioButton.bumpBehav
				};
				if (descriptions != null)
				{
					opRadioButton.description = UIQueue.Translate(descriptions[i]);
					opLabel2.description = opRadioButton.description;
				}
				list.Add(opRadioButton);
				list.Add(opLabel2);
				array[i] = opRadioButton;
				if (i > 0)
				{
					UIfocusable.MutualHorizontalFocusableBind(array[i - 1], opRadioButton);
				}
			}
			opRadioButtonGroup.SetButtons(array);
			opRadioButtonGroup.ShowConfig();
			hasInitialized = true;
			return list;
		}
	}

	internal bool _greyedOut;

	public OpRadioButton[] buttons;

	protected internal override bool CurrentlyFocusableMouse => false;

	protected internal override bool CurrentlyFocusableNonMouse => false;

	public new bool greyedOut
	{
		get
		{
			return _greyedOut;
		}
		set
		{
			_greyedOut = value;
			for (int i = 0; i < buttons.Length; i++)
			{
				buttons[i].greyedOut = value;
			}
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
			if (!(base.value == value) && int.TryParse(value, out var result) && result >= 0 && result < buttons.Length)
			{
				base.value = value;
				SwitchButtons(result);
			}
		}
	}

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

	public OpRadioButtonGroup(Configurable<int> config)
		: base(config, -5000f * Vector2.one, Vector2.one)
	{
		_greyedOut = false;
	}

	public virtual void SetButtons(OpRadioButton[] buttons)
	{
		this.buttons = buttons;
		if (base.InScrollBox)
		{
			OpScrollBox opScrollBox = base.scrollBox;
			UIelement[] items = this.buttons;
			opScrollBox.AddItems(items);
		}
		else if (tab != null)
		{
			OpTab opTab = tab;
			UIelement[] items = this.buttons;
			opTab.AddItems(items);
		}
		for (int i = 0; i < buttons.Length; i++)
		{
			this.buttons[i].group = this;
			this.buttons[i].index = i;
			if (i == this.GetValueInt())
			{
				this.buttons[i]._value = "true";
			}
			else
			{
				this.buttons[i]._value = "false";
			}
			this.buttons[i].Change();
		}
	}

	public override void Update()
	{
		base.Update();
		if (held)
		{
			bool flag = false;
			OpRadioButton[] array = buttons;
			foreach (OpRadioButton opRadioButton in array)
			{
				opRadioButton.Update();
				flag = flag || opRadioButton._click;
			}
			if (!flag)
			{
				held = false;
			}
		}
	}

	public void SetColorEdge(Color newColor)
	{
		OpRadioButton[] array = buttons;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].colorEdge = newColor;
		}
	}

	public void SetColorFill(Color newColor)
	{
		OpRadioButton[] array = buttons;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].colorFill = newColor;
		}
	}

	protected internal virtual void SwitchButtons(int index)
	{
		if (buttons != null)
		{
			for (int i = 0; i < buttons.Length; i++)
			{
				buttons[i]._value = ((i == index) ? "true" : "false");
				buttons[i].Change();
			}
		}
	}

	protected internal override void Change()
	{
		base.Change();
		SwitchButtons(int.Parse(value));
	}

	protected internal override void Deactivate()
	{
		base.Deactivate();
		for (int i = 0; i < buttons.Length; i++)
		{
			buttons[i].Deactivate();
		}
	}

	protected internal override void Reactivate()
	{
		base.Reactivate();
		if (!base.Hidden)
		{
			for (int i = 0; i < buttons.Length; i++)
			{
				buttons[i].Reactivate();
			}
		}
	}
}
