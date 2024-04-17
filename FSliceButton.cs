using UnityEngine;

public class FSliceButton : FContainer, FSingleTouchableInterface, FCapturedTouchableInterface
{
	public delegate void FSliceButtonSignalDelegate(FSliceButton button);

	protected FAtlasElement _upElement;

	protected FAtlasElement _downElement;

	protected Color _upColor;

	protected Color _downColor;

	protected FSliceSprite _bg;

	protected string _soundName;

	protected FLabel _labelA;

	protected FLabel _labelB;

	private float _anchorX = 0.5f;

	private float _anchorY = 0.5f;

	public float expansionAmount = 10f;

	private bool _isEnabled = true;

	public FSprite sprite => _bg;

	public float anchorX
	{
		get
		{
			return _anchorX;
		}
		set
		{
			_anchorX = value;
			_bg.anchorX = _anchorX;
			if (_labelA != null)
			{
				_labelA.x = (0f - _anchorX) * _bg.width + _bg.width / 2f;
			}
			if (_labelB != null)
			{
				_labelB.x = (0f - _anchorX) * _bg.width + _bg.width / 2f;
			}
		}
	}

	public float anchorY
	{
		get
		{
			return _anchorY;
		}
		set
		{
			_anchorY = value;
			_bg.anchorY = _anchorY;
			if (_labelA != null)
			{
				_labelA.y = (0f - _anchorY) * _bg.height + _bg.height / 2f;
			}
			if (_labelB != null)
			{
				_labelB.y = (0f - _anchorY) * _bg.height + _bg.height / 2f;
			}
		}
	}

	public bool isEnabled
	{
		get
		{
			return _isEnabled;
		}
		set
		{
			if (_isEnabled != value)
			{
				_isEnabled = value;
				if (_isEnabled)
				{
					_bg.color = _upColor;
					return;
				}
				RXColorHSL rXColorHSL = RXColor.HSLFromColor(_upColor);
				rXColorHSL.s = 0.25f;
				rXColorHSL.l = 0.6f;
				Color color = RXColor.ColorFromHSL(rXColorHSL);
				_bg.color = color;
			}
		}
	}

	public FLabel labelA => _labelA;

	public FLabel labelB => _labelB;

	public float width
	{
		get
		{
			return _bg.width;
		}
		set
		{
			_bg.width = value;
		}
	}

	public float height
	{
		get
		{
			return _bg.height;
		}
		set
		{
			_bg.height = value;
		}
	}

	public event FSliceButtonSignalDelegate SignalPress;

	public event FSliceButtonSignalDelegate SignalRelease;

	public event FSliceButtonSignalDelegate SignalReleaseOutside;

	public FSliceButton(float width, float height, string upElementName, string downElementName, Color upColor, Color downColor, string soundName)
	{
		_upElement = Futile.atlasManager.GetElementWithName(upElementName);
		_downElement = Futile.atlasManager.GetElementWithName(downElementName);
		_upColor = upColor;
		_downColor = downColor;
		_soundName = soundName;
		_bg = new FSliceSprite(_upElement.name, width, height, 16f, 16f, 16f, 16f);
		_bg.anchorX = _anchorX;
		_bg.anchorY = _anchorY;
		_bg.color = _upColor;
		AddChild(_bg);
	}

	public FSliceButton(float width, float height, string upElementName, string downElementName, Color color, string soundName)
		: this(width, height, upElementName, downElementName, color, color, soundName)
	{
	}

	public FSliceButton(float width, float height, string upElementName, string downElementName, Color color)
		: this(width, height, upElementName, downElementName, color, color, null)
	{
	}

	public FSliceButton(float width, float height, string upElementName, string downElementName, string soundName)
		: this(width, height, upElementName, downElementName, Color.white, Color.white, soundName)
	{
	}

	public FSliceButton(float width, float height, string upElementName, string downElementName)
		: this(width, height, upElementName, downElementName, Color.white, Color.white, null)
	{
	}

	public FLabel AddLabelA(string fontName, string text, float scale, float offsetY, Color color)
	{
		if (_labelA != null)
		{
			RemoveChild(_labelA);
		}
		_labelA = new FLabel(fontName, text);
		AddChild(_labelA);
		_labelA.color = color;
		FLabel fLabel = _labelA;
		float num2 = (_labelA.anchorY = 0.5f);
		fLabel.anchorX = num2;
		_labelA.x = (0f - _anchorX) * _bg.width + _bg.width / 2f;
		_labelA.y = (0f - _anchorY) * _bg.height + _bg.height / 2f + offsetY;
		_labelA.scale = scale;
		return _labelA;
	}

	public FLabel AddLabelB(string fontName, string text, float scale, float offsetY, Color color)
	{
		if (_labelB != null)
		{
			RemoveChild(_labelB);
		}
		_labelB = new FLabel(fontName, text);
		AddChild(_labelB);
		_labelB.color = color;
		FLabel fLabel = _labelB;
		float num2 = (_labelB.anchorY = 0.5f);
		fLabel.anchorX = num2;
		_labelB.x = (0f - _anchorX) * _bg.width + _bg.width / 2f;
		_labelB.y = (0f - _anchorY) * _bg.height + _bg.height / 2f + offsetY;
		_labelB.scale = scale;
		return _labelB;
	}

	public override void HandleAddedToStage()
	{
		base.HandleAddedToStage();
		Futile.touchManager.AddSingleTouchTarget(this);
	}

	public override void HandleRemovedFromStage()
	{
		base.HandleRemovedFromStage();
		Futile.touchManager.RemoveSingleTouchTarget(this);
	}

	public bool HandleSingleTouchBegan(FTouch touch)
	{
		if (!_isEnabled)
		{
			return false;
		}
		Vector2 point = _bg.GlobalToLocal(touch.position);
		if (_bg.textureRect.Contains(point))
		{
			_bg.element = _downElement;
			_bg.color = _downColor;
			if (_soundName != null)
			{
				FSoundManager.PlaySound(_soundName);
			}
			if (this.SignalPress != null)
			{
				this.SignalPress(this);
			}
			return true;
		}
		return false;
	}

	public void HandleSingleTouchMoved(FTouch touch)
	{
		Vector2 point = _bg.GlobalToLocal(touch.position);
		if (_bg.textureRect.CloneWithExpansion(expansionAmount).Contains(point))
		{
			_bg.element = _downElement;
			_bg.color = _downColor;
		}
		else
		{
			_bg.element = _upElement;
			_bg.color = _upColor;
		}
	}

	public void HandleSingleTouchEnded(FTouch touch)
	{
		_bg.element = _upElement;
		_bg.color = _upColor;
		Vector2 point = _bg.GlobalToLocal(touch.position);
		if (_bg.textureRect.CloneWithExpansion(expansionAmount).Contains(point))
		{
			if (this.SignalRelease != null)
			{
				this.SignalRelease(this);
			}
		}
		else if (this.SignalReleaseOutside != null)
		{
			this.SignalReleaseOutside(this);
		}
	}

	public void HandleSingleTouchCanceled(FTouch touch)
	{
		_bg.element = _upElement;
		_bg.color = _upColor;
		if (this.SignalReleaseOutside != null)
		{
			this.SignalReleaseOutside(this);
		}
	}

	public void SetAnchor(float x, float y)
	{
		anchorX = x;
		anchorY = y;
	}
}
