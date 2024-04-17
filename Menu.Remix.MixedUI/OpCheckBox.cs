using System.Collections.Generic;
using Menu.Remix.MixedUI.ValueTypes;
using RWCustom;
using UnityEngine;

namespace Menu.Remix.MixedUI;

public class OpCheckBox : UIconfig, IValueBool, IValueType
{
	public class Queue : ConfigQueue
	{
		protected override float sizeY => 24f;

		public Queue(Configurable<bool> config, object sign = null)
			: base(config, sign)
		{
		}

		protected override List<UIelement> _InitializeThisQueue(IHoldUIelements holder, float posX, ref float offsetY)
		{
			offsetY += sizeY;
			float posY = GetPosY(holder, offsetY);
			List<UIelement> list = new List<UIelement>();
			OpCheckBox opCheckBox = new OpCheckBox(config as Configurable<bool>, posX, posY)
			{
				sign = sign
			};
			if (onChange != null)
			{
				opCheckBox.OnChange += onChange;
			}
			if (onHeld != null)
			{
				opCheckBox.OnHeld += onHeld;
			}
			if (onValueUpdate != null)
			{
				opCheckBox.OnValueUpdate += onValueUpdate;
			}
			if (onValueChanged != null)
			{
				opCheckBox.OnValueChanged += onValueChanged;
			}
			mainFocusable = opCheckBox;
			list.Add(opCheckBox);
			if (!string.IsNullOrEmpty(config.info?.description))
			{
				opCheckBox.description = UIQueue.GetFirstSentence(UIQueue.Translate(config.info.description));
			}
			if (!string.IsNullOrEmpty(config.key))
			{
				OpLabel item = new OpLabel(new Vector2(posX + 30f, posY), new Vector2(holder.CanvasSize.x - posX - 40f, 24f), UIQueue.Translate(config.key), FLabelAlignment.Left)
				{
					autoWrap = true,
					bumpBehav = opCheckBox.bumpBehav,
					description = opCheckBox.description
				};
				list.Add(item);
			}
			opCheckBox.ShowConfig();
			hasInitialized = true;
			return list;
		}
	}

	public DyeableRect rect;

	public FSprite symbolSprite;

	public Color colorEdge = MenuColorEffect.rgbMediumGrey;

	public Color colorFill = MenuColorEffect.rgbBlack;

	private float _symbolHalfVisible;

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

	public OpCheckBox(Configurable<bool> config, Vector2 pos)
		: base(config, pos, new Vector2(24f, 24f))
	{
		fixedSize = new Vector2(24f, 24f);
		rect = new DyeableRect(myContainer, Vector2.zero, base.size);
		symbolSprite = new FSprite("Menu_Symbol_CheckBox")
		{
			anchorX = 0.5f,
			anchorY = 0.5f,
			x = 12f,
			y = 12f
		};
		myContainer.AddChild(symbolSprite);
	}

	public OpCheckBox(Configurable<bool> config, float posX, float posY)
		: this(config, new Vector2(posX, posY))
	{
	}

	protected internal override string DisplayDescription()
	{
		if (!string.IsNullOrEmpty(description))
		{
			return description;
		}
		return OptionalText.GetText((!base.MenuMouseMode) ? OptionalText.ID.OpCheckBox_NonMouseTuto : OptionalText.ID.OpCheckBox_MouseTuto);
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		rect.addSize = new Vector2(4f, 4f) * base.bumpBehav.AddSize;
		if (greyedOut)
		{
			if (this.GetValueBool())
			{
				symbolSprite.alpha = 1f;
				symbolSprite.color = base.bumpBehav.GetColor(colorEdge);
			}
			else
			{
				symbolSprite.alpha = 0f;
			}
			rect.colorEdge = base.bumpBehav.GetColor(colorEdge);
			rect.colorFill = base.bumpBehav.GetColor(colorFill);
			rect.GrafUpdate(timeStacker);
			return;
		}
		Color color = base.bumpBehav.GetColor(colorEdge);
		if (base.Focused || MouseOver)
		{
			_symbolHalfVisible = Custom.LerpAndTick(_symbolHalfVisible, 1f, 0.07f, 1f / 60f / UIelement.frameMulti);
			symbolSprite.color = Color.Lerp(MenuColorEffect.MidToDark(color), color, base.bumpBehav.Sin(10f));
		}
		else
		{
			_symbolHalfVisible = 0f;
			symbolSprite.color = color;
		}
		rect.colorEdge = color;
		if (this.GetValueBool())
		{
			symbolSprite.alpha = 1f - _symbolHalfVisible * 0.2f;
		}
		else
		{
			symbolSprite.alpha = _symbolHalfVisible * 0.2f;
		}
		rect.fillAlpha = base.bumpBehav.FillAlpha;
		rect.colorFill = colorFill;
		rect.GrafUpdate(timeStacker);
	}

	public override void Update()
	{
		base.Update();
		rect.Update();
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
					held = true;
				}
				else if (held)
				{
					this.SetValueBool(!this.GetValueBool());
					PlaySound((!this.GetValueBool()) ? SoundID.MENU_Checkbox_Check : SoundID.MENU_Checkbox_Uncheck);
					held = false;
				}
			}
			else if (held && !Input.GetMouseButton(0))
			{
				held = false;
			}
		}
		else if (held && !base.CtlrInput.jmp)
		{
			this.SetValueBool(!this.GetValueBool());
			PlaySound((!this.GetValueBool()) ? SoundID.MENU_Checkbox_Check : SoundID.MENU_Checkbox_Uncheck);
			held = false;
		}
	}
}
