using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace Menu.Remix.MixedUI;

public class OpHoldButton : UIfocusable
{
	public class QueueCircular : FocusableQueue
	{
		public string description = "";

		protected readonly string displayText;

		protected readonly float fillTime;

		protected readonly string label;

		public OnSignalHandler onPressInit;

		public OnSignalHandler onPressDone;

		public OnSignalHandler onClick;

		protected override float sizeY => 110f;

		public QueueCircular(string displayText, string label = "", float fillTime = 80f, object sign = null)
			: base(sign)
		{
			this.displayText = displayText;
			this.fillTime = fillTime;
			this.label = label;
		}

		protected override List<UIelement> _InitializeThisQueue(IHoldUIelements holder, float posX, ref float offsetY)
		{
			offsetY += sizeY;
			float posY = GetPosY(holder, offsetY);
			List<UIelement> list = new List<UIelement>();
			OpHoldButton opHoldButton = new OpHoldButton(new Vector2(posX, posY), 55f, UIQueue.Translate(displayText), fillTime)
			{
				sign = sign,
				description = UIQueue.Translate(description)
			};
			if (onPressInit != null)
			{
				opHoldButton.OnPressInit += onPressInit;
			}
			if (onPressDone != null)
			{
				opHoldButton.OnPressDone += onPressDone;
			}
			if (onClick != null)
			{
				opHoldButton.OnClick += onClick;
			}
			mainFocusable = opHoldButton;
			list.Add(opHoldButton);
			if (!string.IsNullOrEmpty(label))
			{
				OpLabel item = new OpLabel(new Vector2(posX + 120f, posY + 70f), new Vector2(holder.CanvasSize.x - posX - 120f, 30f), UIQueue.Translate(label), FLabelAlignment.Left)
				{
					autoWrap = true,
					bumpBehav = opHoldButton.bumpBehav,
					description = opHoldButton.description
				};
				list.Add(item);
			}
			hasInitialized = true;
			return list;
		}
	}

	public class QueueRectangular : FocusableQueue
	{
		public string description = "";

		protected readonly string displayText;

		protected readonly float fillTime;

		protected readonly string label;

		public OnSignalHandler onPressInit;

		public OnSignalHandler onPressDone;

		public OnSignalHandler onClick;

		protected override float sizeY => 30f;

		public QueueRectangular(string displayText, string label = "", float fillTime = 80f, object sign = null)
			: base(sign)
		{
			this.displayText = displayText;
			this.fillTime = fillTime;
			this.label = label;
		}

		protected override List<UIelement> _InitializeThisQueue(IHoldUIelements holder, float posX, ref float offsetY)
		{
			offsetY += sizeY;
			float posY = GetPosY(holder, offsetY);
			float width = GetWidth(holder, posX, Mathf.Min(300f, LabelTest.GetWidth(UIQueue.Translate(displayText)) + 100f), Mathf.Max(50f, LabelTest.GetWidth(UIQueue.Translate(displayText)) + 20f));
			List<UIelement> list = new List<UIelement>();
			OpHoldButton opHoldButton = new OpHoldButton(new Vector2(posX, posY), new Vector2(width, sizeY), UIQueue.Translate(displayText), fillTime)
			{
				sign = sign,
				description = UIQueue.Translate(description)
			};
			if (onChange != null)
			{
				opHoldButton.OnChange += onChange;
			}
			if (onHeld != null)
			{
				opHoldButton.OnHeld += onHeld;
			}
			if (onPressInit != null)
			{
				opHoldButton.OnPressInit += onPressInit;
			}
			if (onPressDone != null)
			{
				opHoldButton.OnPressDone += onPressDone;
			}
			if (onClick != null)
			{
				opHoldButton.OnClick += onClick;
			}
			mainFocusable = opHoldButton;
			list.Add(opHoldButton);
			if (!string.IsNullOrEmpty(label))
			{
				OpLabel item = new OpLabel(new Vector2(posX + width + 10f, posY), new Vector2(holder.CanvasSize.x - posX - width - 10f, 30f), UIQueue.Translate(label), FLabelAlignment.Left)
				{
					autoWrap = true,
					bumpBehav = opHoldButton.bumpBehav,
					description = opHoldButton.description
				};
				list.Add(item);
			}
			hasInitialized = true;
			return list;
		}
	}

	protected readonly FLabel _label;

	protected float _filled;

	protected float _pulse;

	protected readonly float _fillTime;

	protected bool _hasSignalled;

	protected int _releaseCounter;

	protected readonly FSprite[] _circles;

	protected readonly DyeableRect _rect;

	protected readonly DyeableRect _rectH;

	protected readonly GlowGradient _glow;

	protected readonly FSprite _fillSprite;

	private string _text;

	public Color colorEdge = MenuColorEffect.rgbMediumGrey;

	public Color colorFill = MenuColorEffect.rgbBlack;

	protected MenuMicrophone.MenuSoundLoop soundLoop;

	private bool _isProgress;

	public byte progressDeci = 2;

	public string text
	{
		get
		{
			return _text;
		}
		set
		{
			if (_text != value)
			{
				_text = value;
				Change();
			}
		}
	}

	protected internal override Rect FocusRect
	{
		get
		{
			Rect focusRect = base.FocusRect;
			if (!isRectangular)
			{
				focusRect.x -= 15f;
				focusRect.y -= 15f;
				focusRect.width += 30f;
				focusRect.height += 30f;
			}
			return focusRect;
		}
	}

	public float progress { get; private set; }

	public event OnSignalHandler OnPressInit;

	public event OnSignalHandler OnPressDone;

	public event OnSignalHandler OnClick;

	public OpHoldButton(Vector2 pos, float rad, string displayText, float fillTime = 80f)
		: base(pos, Mathf.Max(40f, rad))
	{
		OnPressInit += base.FocusMoveDisallow;
		OnClick += base.FocusMoveDisallow;
		OnPressDone += base.FocusMoveDisallow;
		_fillTime = Mathf.Max(0f, fillTime);
		_text = displayText;
		_circles = new FSprite[5];
		_circles[0] = new FSprite("Futile_White")
		{
			shader = Custom.rainWorld.Shaders["VectorCircleFadable"]
		};
		_circles[1] = new FSprite("Futile_White")
		{
			shader = Custom.rainWorld.Shaders["VectorCircle"]
		};
		_circles[2] = new FSprite("Futile_White")
		{
			shader = Custom.rainWorld.Shaders["HoldButtonCircle"]
		};
		_circles[3] = new FSprite("Futile_White")
		{
			shader = Custom.rainWorld.Shaders["VectorCircle"]
		};
		_circles[4] = new FSprite("Futile_White")
		{
			shader = Custom.rainWorld.Shaders["VectorCircleFadable"]
		};
		for (int i = 0; i < _circles.Length; i++)
		{
			myContainer.AddChild(_circles[i]);
			_circles[i].SetPosition(base.rad, base.rad);
		}
		_label = UIelement.FLabelCreate(text);
		myContainer.AddChild(_label);
		UIelement.FLabelPlaceAtCenter(_label, Vector2.zero, 2f * base.rad * Vector2.one);
	}

	public OpHoldButton(Vector2 pos, Vector2 size, string displayText, float fillTime = 80f)
		: base(pos, size)
	{
		_fillTime = Mathf.Max(0f, fillTime);
		_size = new Vector2(Mathf.Max(24f, size.x), Mathf.Max(24f, size.y));
		_text = displayText;
		_rect = new DyeableRect(myContainer, Vector2.zero, base.size);
		_rectH = new DyeableRect(myContainer, Vector2.zero, base.size, filled: false);
		_fillSprite = new FSprite("pixel")
		{
			anchorX = 0f,
			anchorY = 0f,
			x = _rect.sprites[8].x,
			y = _rect.sprites[8].y,
			scaleX = 9f,
			scaleY = _rect.sprites[8].scaleY,
			color = colorEdge,
			alpha = 1f
		};
		myContainer.AddChild(_fillSprite);
		_glow = new GlowGradient(myContainer, Vector2.zero, Vector2.one);
		_glow.Hide();
		_label = UIelement.FLabelCreate(text);
		UIelement.FLabelPlaceAtCenter(_label, Vector2.zero, base.size);
		myContainer.AddChild(_label);
	}

	protected internal override string DisplayDescription()
	{
		if (!string.IsNullOrEmpty(description))
		{
			return description;
		}
		if (_isProgress)
		{
			return "";
		}
		return OptionalText.GetText(base.MenuMouseMode ? OptionalText.ID.OpHoldButton_MouseTuto : OptionalText.ID.OpHoldButton_NonMouseTuto);
	}

	public override void Reset()
	{
		base.Reset();
		SoundLoopKill();
		_filled = 0f;
		_pulse = 0f;
		_hasSignalled = false;
		held = false;
		SetProgress(-1f);
	}

	protected internal override void Unload()
	{
		base.Unload();
		SoundLoopKill();
	}

	protected internal override void Change()
	{
		base.Change();
		if (!isRectangular)
		{
			UIelement.FLabelPlaceAtCenter(_label, Vector2.zero, 2f * base.rad * Vector2.one);
		}
		else
		{
			_size = new Vector2(Mathf.Max(24f, base.size.x), Mathf.Max(24f, base.size.y));
			UIelement.FLabelPlaceAtCenter(_label, Vector2.zero, base.size);
			_rect.size = base.size;
			_rectH.size = base.size;
		}
		if (!string.IsNullOrEmpty(text) || !_isProgress)
		{
			_label.text = text;
		}
		else
		{
			_label.text = progress.ToString("N" + Custom.IntClamp(progressDeci, 0, 4)) + "%";
		}
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		float num = Mathf.Clamp01((!_isProgress) ? _filled : (progress / 100f));
		Color color = base.bumpBehav.GetColor(colorEdge);
		_label.color = color;
		if (!isRectangular)
		{
			float num2 = base.rad - 15f + 8f * (base.bumpBehav.sizeBump + 0.5f * Mathf.Sin(base.bumpBehav.extraSizeBump * (float)Math.PI)) * ((!held) ? 1f : (0.5f + 0.5f * Mathf.Sin(_pulse * (float)Math.PI * 2f))) + 0.5f;
			for (int i = 0; i < _circles.Length; i++)
			{
				_circles[i].scale = num2 / 8f;
				_circles[i].SetPosition(base.rad, base.rad);
			}
			_circles[0].color = new Color(1f / 51f, 0f, Mathf.Lerp(0.3f, 0.6f, base.bumpBehav.col));
			_circles[1].color = color;
			_circles[1].alpha = 2f / num2;
			_circles[2].scale = (num2 + 10f) / 8f;
			_circles[2].alpha = num;
			_circles[2].color = Color.Lerp(Color.white, colorEdge, 0.7f);
			_circles[3].color = Color.Lerp(color, MenuColorEffect.MidToDark(color), 0.5f);
			_circles[3].scale = (num2 + 15f) / 8f;
			_circles[3].alpha = 2f / (num2 + 15f);
			float num3 = 0.5f + 0.5f * Mathf.Sin(base.bumpBehav.sin / 30f * (float)Math.PI * 2f);
			num3 *= base.bumpBehav.sizeBump;
			if (greyedOut)
			{
				num3 = 0f;
			}
			_circles[4].scale = (num2 - 8f * base.bumpBehav.sizeBump) / 8f;
			_circles[4].alpha = 2f / (num2 - 8f * base.bumpBehav.sizeBump);
			_circles[4].color = new Color(0f, 0f, num3);
			return;
		}
		if (greyedOut)
		{
			_rect.colorEdge = base.bumpBehav.GetColor(colorEdge);
			_rect.colorFill = base.bumpBehav.GetColor(colorFill);
			_rectH.colorEdge = base.bumpBehav.GetColor(colorEdge);
			_rect.GrafUpdate(timeStacker);
			_rectH.GrafUpdate(timeStacker);
			_fillSprite.scaleX = 0f;
			return;
		}
		_rectH.colorEdge = base.bumpBehav.GetColor(colorEdge);
		_rectH.addSize = new Vector2(-2f, -2f) * base.bumpBehav.AddSize;
		float alpha = (((base.Focused || MouseOver) && !held) ? ((0.5f + 0.5f * base.bumpBehav.Sin(10f)) * base.bumpBehav.AddSize) : 0f);
		for (int j = 0; j < 8; j++)
		{
			_rectH.sprites[j].alpha = alpha;
		}
		_rect.colorEdge = base.bumpBehav.GetColor(colorEdge);
		_rect.fillAlpha = base.bumpBehav.FillAlpha;
		_rect.addSize = 6f * base.bumpBehav.AddSize * Vector2.one;
		_rect.colorFill = colorFill;
		_rect.GrafUpdate(timeStacker);
		_rectH.GrafUpdate(timeStacker);
		if (num > 0f)
		{
			for (int k = 0; k < (Mathf.Approximately(num, 1f) ? 4 : 2); k++)
			{
				_rect.sprites[_rect.FillCornerSprite(k)].alpha = 1f;
				_rect.sprites[_rect.FillCornerSprite(k)].color = _rect.colorEdge;
			}
			_rect.sprites[_rect.FillSideSprite(0)].alpha = 1f;
			_rect.sprites[_rect.FillSideSprite(0)].color = _rect.colorEdge;
			if (Mathf.Approximately(num, 1f))
			{
				_rect.sprites[_rect.FillSideSprite(2)].alpha = 1f;
				_rect.sprites[_rect.FillSideSprite(2)].color = _rect.colorEdge;
			}
			_fillSprite.x = 6f - _rect.addSize.x / 2f;
			_fillSprite.y = 0f - _rect.addSize.y / 2f + 1.5f;
			_fillSprite.scaleX = (base.size.x - 14f + _rect.addSize.x) * num + 2.5f;
			_fillSprite.scaleY = base.size.y + _rect.addSize.y - 3f;
			_fillSprite.color = _rect.colorEdge;
			_rect.sprites[8].x = (base.size.x - 14f) * num + 7f;
			_rect.sprites[8].scaleX = (base.size.x - 14f) * (1f - num);
			_rect.sprites[_rect.FillSideSprite(1)].x = (base.size.x - 14f) * num + 7f;
			_rect.sprites[_rect.FillSideSprite(1)].scaleX = (base.size.x - 14f) * (1f - num);
			_rect.sprites[_rect.FillSideSprite(3)].x = (base.size.x - 14f) * num + 7f;
			_rect.sprites[_rect.FillSideSprite(3)].scaleX = (base.size.x - 14f) * (1f - num);
			_glow.size = new Vector2(Mathf.Min(base.size.x + 10f, _label.textRect.size.x * 1.5f), Mathf.Min(base.size.y + 10f, _label.textRect.size.y * 1.5f));
			_glow.centerPos = base.size / 2f;
			float t = Custom.SCurve(num, 0.6f);
			_glow.color = Color.Lerp(MenuColorEffect.rgbBlack, color, t);
			_glow.alpha = Mathf.Clamp01(1.5f - _fillSprite.scaleX / _glow.pos.x) * 0.6f;
			_glow.Show();
			_label.color = Color.Lerp(color, MenuColorEffect.MidToVeryDark(color), t);
		}
		else
		{
			_fillSprite.scaleX = 0f;
			_glow.Hide();
		}
	}

	public override void Update()
	{
		if (greyedOut && held)
		{
			held = false;
		}
		base.Update();
		if (isRectangular)
		{
			_rect.Update();
			_rectH.Update();
		}
		if (greyedOut || _isProgress)
		{
			_filled = 0f;
			_pulse = 0f;
			_hasSignalled = false;
			base.bumpBehav.sizeBump = (greyedOut ? 0f : 1f);
			if (greyedOut)
			{
				base.bumpBehav.sin = 0f;
				return;
			}
		}
		if (held && !_isProgress)
		{
			if (soundLoop == null)
			{
				soundLoop = base.Menu.PlayLoop(SoundID.MENU_Security_Button_LOOP, 0f, 0f, 1f, isBkgLoop: false);
			}
			soundLoop.loopVolume = Mathf.Lerp(soundLoop.loopVolume, 1f, 0.85f * UIelement.frameMulti);
			soundLoop.loopPitch = Mathf.Lerp(0.3f, 1.5f, _filled) - 0.15f * Mathf.Sin(_pulse * (float)Math.PI * 2f);
			_pulse += UIelement.frameMulti * _filled / 20f;
		}
		else
		{
			if (soundLoop != null)
			{
				soundLoop.loopVolume = Mathf.Max(0f, soundLoop.loopVolume - 0.125f * UIelement.frameMulti);
				if (soundLoop.loopVolume <= 0f)
				{
					SoundLoopKill();
				}
			}
			_pulse = 0f;
		}
		bool flag = held;
		if (base.MenuMouseMode)
		{
			if (held)
			{
				held = Input.GetMouseButton(0);
			}
			else
			{
				held = MouseOver && Input.GetMouseButton(0);
			}
		}
		else
		{
			held = held && base.CtlrInput.jmp;
		}
		base.bumpBehav.sizeBump = ((!held) ? 0f : 1f);
		if (held)
		{
			if (!flag)
			{
				this.OnPressInit?.Invoke(this);
			}
			if (_isProgress)
			{
				return;
			}
			base.bumpBehav.sin = _pulse;
			_filled = Custom.LerpAndTick(_filled, 1f, 0.007f, UIelement.frameMulti / _fillTime);
			if (_filled >= 1f && !_hasSignalled)
			{
				if (this.OnPressDone != null)
				{
					this.OnPressDone(this);
					base.Menu.PlaySound(SoundID.MENU_Security_Button_Release);
				}
				_hasSignalled = true;
			}
			_releaseCounter = 0;
			return;
		}
		if (flag)
		{
			this.OnClick?.Invoke(this);
		}
		if (flag && !_hasSignalled && !_isProgress)
		{
			PlaySound(SoundID.MENU_Security_Button_Release);
		}
		if (_hasSignalled)
		{
			_releaseCounter++;
			if ((float)_releaseCounter > (float)ModdingMenu.DASinit * 1.5f)
			{
				_filled = Custom.LerpAndTick(_filled, 0f, 0.04f, 0.025f * UIelement.frameMulti);
				if (_filled < 0.5f)
				{
					_hasSignalled = false;
				}
			}
			else
			{
				_filled = 1f;
			}
		}
		else
		{
			_filled = Custom.LerpAndTick(_filled, 0f, 0.04f, 0.025f * UIelement.frameMulti);
		}
	}

	public void SetProgress(float percentage)
	{
		if (percentage < 0f)
		{
			_isProgress = false;
			progress = 0f;
			return;
		}
		if (!_isProgress)
		{
			_isProgress = true;
			text = "";
		}
		progress = Mathf.Clamp(percentage, 0f, 100f);
		Change();
	}

	protected internal override void Deactivate()
	{
		base.Deactivate();
		SoundLoopKill();
		_filled = 0f;
		_pulse = 0f;
		_hasSignalled = false;
	}

	protected void SoundLoopKill()
	{
		if (soundLoop != null)
		{
			soundLoop.Stop();
			soundLoop.Destroy();
			soundLoop = null;
		}
	}

	protected internal override void Freeze()
	{
		base.Freeze();
		SoundLoopKill();
	}
}
