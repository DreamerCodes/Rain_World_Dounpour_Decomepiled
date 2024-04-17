using Menu.Remix.MixedUI.ValueTypes;
using RWCustom;
using UnityEngine;

namespace Menu.Remix.MixedUI;

public class OpRadioButton : UIfocusable, IValueBool, IValueType
{
	protected readonly FSprite _symbolSprite;

	internal bool _click;

	protected readonly DyeableRect _rect;

	public Color colorEdge = MenuColorEffect.rgbMediumGrey;

	public Color colorFill = MenuColorEffect.rgbBlack;

	private float _symbolHalfVisible;

	public OpRadioButtonGroup group;

	public string _value;

	public int index { get; internal set; }

	public virtual string value
	{
		get
		{
			return _value;
		}
		set
		{
			if (_value != value)
			{
				_value = value;
				this.OnValueUpdate?.Invoke(this);
				group.value = index.ToString();
				Change();
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

	public event OnSignalHandler OnValueUpdate;

	public OpRadioButton(Vector2 pos)
		: base(pos, new Vector2(24f, 24f))
	{
		_value = "false";
		fixedSize = new Vector2(24f, 24f);
		_rect = new DyeableRect(myContainer, Vector2.zero, base.size);
		_symbolSprite = new FSprite("Menu_Symbol_Clear_All");
		myContainer.AddChild(_symbolSprite);
		_symbolSprite.SetAnchor(0f, 0f);
		_symbolSprite.SetPosition(2f, 2f);
		index = -1;
		greyedOut = false;
		_click = false;
	}

	public OpRadioButton(float posX, float posY)
		: this(new Vector2(posX, posY))
	{
	}

	protected internal override string DisplayDescription()
	{
		if (!string.IsNullOrEmpty(description))
		{
			return description;
		}
		return OptionalText.GetText(base.MenuMouseMode ? OptionalText.ID.OpRadioButton_MouseTuto : OptionalText.ID.OpRadioButton_NonMouseTuto);
	}

	protected internal override void NonMouseSetHeld(bool newHeld)
	{
		base.NonMouseSetHeld(newHeld);
		_click = newHeld;
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		_rect.addSize = new Vector2(4f, 4f) * base.bumpBehav.AddSize;
		Color color = base.bumpBehav.GetColor(colorEdge);
		_symbolSprite.color = color;
		_rect.colorEdge = color;
		_rect.colorFill = base.bumpBehav.GetColor(colorFill);
		if (greyedOut)
		{
			if (this.GetValueBool())
			{
				_symbolSprite.alpha = 1f;
			}
			else
			{
				_symbolSprite.alpha = 0f;
			}
			_rect.GrafUpdate(timeStacker);
			return;
		}
		if (base.Focused || MouseOver)
		{
			_symbolHalfVisible = Custom.LerpAndTick(_symbolHalfVisible, 1f, 0.07f, 1f / 60f / UIelement.frameMulti);
			if (!this.GetValueBool())
			{
				_symbolSprite.color = Color.Lerp(MenuColorEffect.MidToDark(color), color, base.bumpBehav.Sin(10f));
			}
		}
		else
		{
			_symbolHalfVisible = 0f;
		}
		if (this.GetValueBool())
		{
			_symbolSprite.alpha = 1f;
		}
		else
		{
			_symbolSprite.alpha = _symbolHalfVisible * 0.2f;
		}
		_rect.fillAlpha = base.bumpBehav.FillAlpha;
		_rect.colorFill = colorFill;
		_rect.GrafUpdate(timeStacker);
	}

	public override void Update()
	{
		base.Update();
		_rect.Update();
		base.bumpBehav.greyedOut = greyedOut;
		if (greyedOut)
		{
			return;
		}
		if (base.MenuMouseMode)
		{
			if (MouseOver)
			{
				if (Input.GetMouseButton(0))
				{
					_click = true;
					held = true;
					base.bumpBehav.held = true;
				}
				else if (_click)
				{
					held = false;
					base.bumpBehav.held = false;
					_click = false;
					PlaySound((!this.GetValueBool()) ? SoundID.MENU_MultipleChoice_Clicked : SoundID.MENY_Already_Selected_MultipleChoice_Clicked);
					this.SetValueBool(value: true);
				}
			}
			else if (!Input.GetMouseButton(0))
			{
				held = false;
				base.bumpBehav.held = false;
				_click = false;
			}
		}
		else if (_click)
		{
			if (base.CtlrInput.jmp)
			{
				base.bumpBehav.held = true;
				return;
			}
			base.bumpBehav.held = false;
			held = false;
			_click = false;
			PlaySound((!this.GetValueBool()) ? SoundID.MENU_MultipleChoice_Clicked : SoundID.MENY_Already_Selected_MultipleChoice_Clicked);
			this.SetValueBool(value: true);
		}
	}
}
