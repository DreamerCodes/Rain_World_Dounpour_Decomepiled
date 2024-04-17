using System.Collections.Generic;
using UnityEngine;

namespace Menu.Remix.MixedUI;

public class OpSimpleButton : UIfocusable
{
	public class Queue : FocusableQueue
	{
		public string description = "";

		protected readonly string displayText;

		protected readonly string label;

		public OnSignalHandler onPressInit;

		public OnSignalHandler onPressHold;

		public OnSignalHandler onClick;

		protected override float sizeY => 30f;

		public Queue(string displayText, string label = "", object sign = null)
			: base(sign)
		{
			this.displayText = displayText;
			this.label = label;
		}

		protected override List<UIelement> _InitializeThisQueue(IHoldUIelements holder, float posX, ref float offsetY)
		{
			offsetY += sizeY;
			float posY = GetPosY(holder, offsetY);
			float width = GetWidth(holder, posX, 300f, Mathf.Max(50f, LabelTest.GetWidth(UIQueue.Translate(displayText)) + 20f));
			List<UIelement> list = new List<UIelement>();
			OpSimpleButton opSimpleButton = new OpSimpleButton(new Vector2(posX, posY), new Vector2(width, 30f), UIQueue.Translate(displayText))
			{
				sign = sign,
				description = UIQueue.Translate(description)
			};
			if (onChange != null)
			{
				opSimpleButton.OnChange += onChange;
			}
			if (onHeld != null)
			{
				opSimpleButton.OnHeld += onHeld;
			}
			if (onPressInit != null)
			{
				opSimpleButton.OnPressInit += onPressInit;
			}
			if (onPressHold != null)
			{
				opSimpleButton.OnPressHold += onPressHold;
			}
			if (onClick != null)
			{
				opSimpleButton.OnClick += onClick;
			}
			mainFocusable = opSimpleButton;
			list.Add(opSimpleButton);
			if (!string.IsNullOrEmpty(label))
			{
				OpLabel item = new OpLabel(new Vector2(posX + width + 10f, posY), new Vector2(holder.CanvasSize.x - posX - width - 10f, 30f), UIQueue.Translate(label), FLabelAlignment.Left)
				{
					autoWrap = true,
					bumpBehav = opSimpleButton.bumpBehav,
					description = opSimpleButton.description
				};
				list.Add(item);
			}
			hasInitialized = true;
			return list;
		}
	}

	protected int _heldCounter;

	public SoundID soundClick = SoundID.MENU_Button_Standard_Button_Pressed;

	private FLabelAlignment _alignment;

	private string _text;

	public Color colorEdge = MenuColorEffect.rgbMediumGrey;

	public Color colorFill = MenuColorEffect.rgbBlack;

	protected readonly FLabel _label;

	protected readonly DyeableRect _rect;

	protected readonly DyeableRect _rectH;

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
			if (!(_text == value) && !_IsImageButton)
			{
				_text = value;
				_label.text = LabelTest.TrimText(_text, base.size.x, addDots: true);
				Change();
			}
		}
	}

	internal bool _IsImageButton => this is OpSimpleImageButton;

	public event OnSignalHandler OnClick;

	public event OnSignalHandler OnPressInit;

	public event OnSignalHandler OnPressHold;

	public OpSimpleButton(Vector2 pos, Vector2 size, string displayText = "")
		: base(pos, size)
	{
		OnPressInit += base.FocusMoveDisallow;
		OnClick += base.FocusMoveDisallow;
		OnPressHold += base.FocusMoveDisallow;
		_size = new Vector2(Mathf.Max(24f, size.x), Mathf.Max(24f, size.y));
		_rect = new DyeableRect(myContainer, Vector2.zero, base.size);
		_rectH = new DyeableRect(myContainer, Vector2.zero, base.size, filled: false);
		if (!_IsImageButton)
		{
			_label = UIelement.FLabelCreate(displayText);
			_text = displayText;
			myContainer.AddChild(_label);
			UIelement.FLabelPlaceAtCenter(_label, Vector2.zero, base.size);
		}
	}

	protected internal override string DisplayDescription()
	{
		if (!string.IsNullOrEmpty(description))
		{
			return description;
		}
		return OptionalText.GetText(base.MenuMouseMode ? OptionalText.ID.OpSimpleButton_MouseTuto : OptionalText.ID.OpSimpleButton_NonMouseTuto);
	}

	protected internal override void Change()
	{
		_size = new Vector2(Mathf.Max(24f, base.size.x), Mathf.Max(24f, base.size.y));
		base.Change();
		if (!_IsImageButton)
		{
			UIelement.FLabelPlaceAtCenter(_label, Vector2.zero, base.size);
			if (alignment != 0)
			{
				_label.alignment = alignment;
				if (alignment == FLabelAlignment.Right)
				{
					_label.x = base.size.x - 5f;
				}
				else
				{
					_label.x = 5f;
				}
			}
		}
		_rect.size = base.size;
		_rectH.size = base.size;
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		_rect.GrafUpdate(timeStacker);
		_rectH.GrafUpdate(timeStacker);
		_rect.addSize = new Vector2(6f, 6f) * base.bumpBehav.AddSize;
		if (!_IsImageButton)
		{
			_label.color = base.bumpBehav.GetColor(colorEdge);
		}
		if (greyedOut)
		{
			_rect.colorEdge = base.bumpBehav.GetColor(colorEdge);
			_rect.colorFill = base.bumpBehav.GetColor(colorFill);
			_rectH.Hide();
			return;
		}
		_rectH.Show();
		_rectH.colorEdge = base.bumpBehav.GetColor(colorEdge);
		_rectH.addSize = new Vector2(-2f, -2f) * base.bumpBehav.AddSize;
		float alpha = (((base.Focused || MouseOver) && !held) ? ((0.5f + 0.5f * base.bumpBehav.Sin(10f)) * base.bumpBehav.AddSize) : 0f);
		for (int i = 0; i < 8; i++)
		{
			_rectH.sprites[i].alpha = alpha;
		}
		_rect.colorEdge = base.bumpBehav.GetColor(colorEdge);
		_rect.fillAlpha = base.bumpBehav.FillAlpha;
		_rect.colorFill = colorFill;
	}

	protected internal override void NonMouseSetHeld(bool newHeld)
	{
		base.NonMouseSetHeld(newHeld);
		if (newHeld)
		{
			this.OnPressInit?.Invoke(this);
		}
		else
		{
			_heldCounter = 0;
		}
	}

	public override void Update()
	{
		if (greyedOut && held)
		{
			held = false;
			PlaySound(soundClick);
			this.OnClick?.Invoke(this);
		}
		base.Update();
		_rect.Update();
		_rectH.Update();
		if (greyedOut)
		{
			_heldCounter = 0;
			return;
		}
		if (base.MenuMouseMode)
		{
			if (MouseOver)
			{
				if (Input.GetMouseButton(0))
				{
					if (!held)
					{
						this.OnPressInit?.Invoke(this);
					}
					held = true;
					_heldCounter++;
				}
				else if (held)
				{
					held = false;
					if (this.OnClick != null)
					{
						this.OnClick(this);
						PlaySound(soundClick);
					}
					_heldCounter = 0;
				}
			}
			else if (!Input.GetMouseButton(0))
			{
				held = false;
				_heldCounter = 0;
			}
		}
		else if (held)
		{
			if (base.CtlrInput.jmp)
			{
				_heldCounter++;
			}
			else
			{
				held = false;
				if (this.OnClick != null)
				{
					this.OnClick(this);
					PlaySound(soundClick);
				}
				_heldCounter = 0;
			}
		}
		if (this.OnPressHold != null && _heldCounter > ModdingMenu.DASinit && _heldCounter % ModdingMenu.DASdelay == 1)
		{
			this.OnPressHold(this);
			base.bumpBehav.sin = 0.5f;
		}
	}
}
