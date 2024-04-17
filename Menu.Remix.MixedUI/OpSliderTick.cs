using System;
using System.Collections.Generic;
using Menu.Remix.MixedUI.ValueTypes;
using RWCustom;
using UnityEngine;

namespace Menu.Remix.MixedUI;

public class OpSliderTick : OpSlider
{
	public new class Queue : ConfigQueue
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
			OpSliderTick opSliderTick = new OpSliderTick(config, new Vector2(posX, posY), (int)width)
			{
				sign = sign
			};
			if (onChange != null)
			{
				opSliderTick.OnChange += onChange;
			}
			if (onHeld != null)
			{
				opSliderTick.OnHeld += onHeld;
			}
			if (onValueUpdate != null)
			{
				opSliderTick.OnValueUpdate += onValueUpdate;
			}
			if (onValueChanged != null)
			{
				opSliderTick.OnValueChanged += onValueChanged;
			}
			mainFocusable = opSliderTick;
			list.Add(opSliderTick);
			if (!string.IsNullOrEmpty(config.info?.description))
			{
				opSliderTick.description = UIQueue.GetFirstSentence(UIQueue.Translate(config.info.description));
			}
			if (_labeled)
			{
				OpLabel item = new OpLabel(new Vector2(posX, posY + 34f), new Vector2(holder.CanvasSize.x - posX, 30f), UIQueue.Translate(config.key), FLabelAlignment.Left)
				{
					autoWrap = true,
					bumpBehav = opSliderTick.bumpBehav,
					description = opSliderTick.description
				};
				list.Add(item);
			}
			opSliderTick.ShowConfig();
			hasInitialized = true;
			return list;
		}
	}

	protected readonly FSprite[] _Nobs;

	protected readonly FSprite _circle;

	protected readonly FSprite _circleFill;

	public OpSliderTick(Configurable<int> config, Vector2 pos, int length, bool vertical = false)
		: this((ConfigurableBase)config, pos, length, vertical)
	{
	}

	public OpSliderTick(ConfigurableBase configIntegral, Vector2 pos, int length, bool vertical = false)
		: base(configIntegral, pos, length, vertical)
	{
		if (base.Span > 31)
		{
			throw new ElementFormatException(this, "The range of OpSliderSubtle should be lower than 31! Use normal OpSlider instead.", base.Key);
		}
		if (base.Span < 2)
		{
			throw new ElementFormatException(this, "The range of OpSliderSubtle is less than 2! Check config.info.acceptable.", base.Key);
		}
		fixedSize = _size;
		mousewheelTick = 1;
		_Nobs = new FSprite[base.Span];
		for (int i = 0; i < _Nobs.Length; i++)
		{
			_Nobs[i] = new FSprite("pixel")
			{
				anchorX = 0.5f,
				anchorY = 0.5f
			};
			myContainer.AddChild(_Nobs[i]);
		}
		_circle = new FSprite("Futile_White")
		{
			anchorX = 0.5f,
			anchorY = 0.5f,
			shader = Custom.rainWorld.Shaders["VectorCircle"]
		};
		_circleFill = new FSprite("Futile_White")
		{
			anchorX = 0.5f,
			anchorY = 0.5f,
			shader = Custom.rainWorld.Shaders["VectorCircle"]
		};
		myContainer.AddChild(_circle);
		myContainer.AddChild(_circleFill);
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		float num = ((!greyedOut) ? Mathf.Clamp(base.bumpBehav.AddSize, 0f, 1f) : 0f);
		for (int i = 0; i < _Nobs.Length; i++)
		{
			if (vertical)
			{
				_Nobs[i].x = 15.01f;
				_Nobs[i].y = base._mul * (float)i + 0.01f;
				_Nobs[i].scaleX = 6f + num * 4f;
				_Nobs[i].scaleY = 2f;
			}
			else
			{
				_Nobs[i].y = 15.01f;
				_Nobs[i].x = base._mul * (float)i + 0.01f;
				_Nobs[i].scaleY = 6f + num * 4f;
				_Nobs[i].scaleX = 2f;
			}
			_Nobs[i].color = _lineSprites[0].color;
		}
		if (vertical)
		{
			_circle.x = 15.01f;
			_circle.y = base._mul * (float)(this.GetValueInt() - min) + 0.01f;
		}
		else
		{
			_circle.y = 15.01f;
			_circle.x = base._mul * (float)(this.GetValueInt() - min) + 0.01f;
		}
		_circle.scale = 1.1f + num * 0.3f;
		_circle.color = base.bumpBehav.GetColor(colorEdge);
		_circleFill.x = _circle.x;
		_circleFill.y = _circle.y;
		_circleFill.scale = _circle.scale - 0.25f;
		_circleFill.color = base.bumpBehav.GetColor(held ? colorEdge : colorFill);
	}

	protected override void _LineVisibility(Vector2 cutPos, Vector2 cutSize)
	{
		base._LineVisibility(cutPos, cutSize);
		_lineSprites[0].isVisible = false;
		_lineSprites[3].isVisible = false;
		for (int i = 0; i < _Nobs.Length; i++)
		{
			if (this.GetValueInt() - min == i)
			{
				_Nobs[i].isVisible = false;
			}
			else
			{
				_Nobs[i].isVisible = true;
			}
		}
	}
}
