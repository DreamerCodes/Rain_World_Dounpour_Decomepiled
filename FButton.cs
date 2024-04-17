using UnityEngine;

public class FButton : FContainer, FSingleTouchableInterface, FCapturedTouchableInterface
{
	public delegate void ButtonSignalDelegate(FButton button);

	protected Rect _hitRect;

	protected bool _shouldUseCustomHitRect;

	protected FAtlasElement _upElement;

	protected FAtlasElement _downElement;

	protected FAtlasElement _overElement;

	protected bool _shouldUseCustomColors;

	protected Color _upColor = Color.white;

	protected Color _downColor = Color.white;

	protected Color _overColor = Color.white;

	protected FSprite _sprite;

	protected string _clickSoundName;

	protected FLabel _label;

	private float _anchorX = 0.5f;

	private float _anchorY = 0.5f;

	public float expansionAmount = 10f;

	protected bool _isEnabled = true;

	protected bool _supportsOver;

	protected bool _isTouchDown;

	public FLabel label => _label;

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
				UpdateEnabled();
			}
		}
	}

	public FSprite sprite => _sprite;

	public float anchorX
	{
		get
		{
			return _anchorX;
		}
		set
		{
			_anchorX = value;
			_sprite.anchorX = _anchorX;
			if (_label != null)
			{
				_label.x = (0f - _anchorX) * _sprite.width + _sprite.width / 2f;
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
			_sprite.anchorY = _anchorY;
			if (_label != null)
			{
				_label.y = (0f - _anchorY) * _sprite.height + _sprite.height / 2f;
			}
		}
	}

	public Rect hitRect
	{
		get
		{
			return _hitRect;
		}
		set
		{
			_hitRect = value;
			_shouldUseCustomHitRect = true;
		}
	}

	public event ButtonSignalDelegate SignalPress;

	public event ButtonSignalDelegate SignalRelease;

	public event ButtonSignalDelegate SignalReleaseOutside;

	public FButton(string upElementName, string downElementName, string overElementName, string clickSoundName)
	{
		_upElement = Futile.atlasManager.GetElementWithName(upElementName);
		_downElement = Futile.atlasManager.GetElementWithName(downElementName);
		if (overElementName != null)
		{
			_overElement = Futile.atlasManager.GetElementWithName(overElementName);
			_supportsOver = true;
		}
		_sprite = new FSprite(_upElement.name);
		_sprite.anchorX = _anchorX;
		_sprite.anchorY = _anchorY;
		AddChild(_sprite);
		_hitRect = _sprite.textureRect;
		_clickSoundName = clickSoundName;
		EnableSingleTouch();
		if (_supportsOver)
		{
			ListenForUpdate(HandleUpdate);
		}
	}

	public FButton(string upElementName)
		: this(upElementName, upElementName, null, null)
	{
	}

	public FButton(string upElementName, string downElementName)
		: this(upElementName, downElementName, null, null)
	{
	}

	public FButton(string upElementName, string downElementName, string clickSoundName)
		: this(upElementName, downElementName, null, clickSoundName)
	{
	}

	public virtual void SetElements(string upElementName, string downElementName, string overElementName)
	{
		_upElement = Futile.atlasManager.GetElementWithName(upElementName);
		_downElement = Futile.atlasManager.GetElementWithName(downElementName);
		if (overElementName != null)
		{
			_overElement = Futile.atlasManager.GetElementWithName(overElementName);
			_supportsOver = true;
		}
		if (_isTouchDown)
		{
			_sprite.element = _downElement;
		}
		else
		{
			_sprite.element = _upElement;
		}
	}

	public virtual void SetColors(Color upColor, Color downColor)
	{
		SetColors(upColor, downColor, Color.white);
	}

	public virtual void SetColors(Color upColor, Color downColor, Color overColor)
	{
		_shouldUseCustomColors = true;
		_upColor = upColor;
		_downColor = downColor;
		_overColor = overColor;
		if (_isTouchDown)
		{
			_sprite.color = _downColor;
		}
		else
		{
			_sprite.color = _upColor;
		}
	}

	public virtual FLabel AddLabel(string fontName, string text, Color color)
	{
		return AddLabel(fontName, text, new FTextParams(), color);
	}

	public virtual FLabel AddLabel(string fontName, string text, FTextParams textParams, Color color)
	{
		if (_label != null)
		{
			RemoveChild(_label);
		}
		_label = new FLabel(fontName, text, textParams);
		AddChild(_label);
		_label.color = color;
		FLabel fLabel = _label;
		float num2 = (_label.anchorY = 0.5f);
		fLabel.anchorX = num2;
		_label.x = (0f - _anchorX) * _sprite.width + _sprite.width / 2f;
		_label.y = (0f - _anchorY) * _sprite.height + _sprite.height / 2f;
		return _label;
	}

	protected virtual void HandleUpdate()
	{
		UpdateOverState();
	}

	protected virtual void UpdateOverState()
	{
		if (_isTouchDown)
		{
			return;
		}
		Vector2 localMousePosition = GetLocalMousePosition();
		if (_hitRect.Contains(localMousePosition))
		{
			_sprite.element = _overElement;
			if (_shouldUseCustomColors)
			{
				_sprite.color = _overColor;
			}
		}
		else
		{
			_sprite.element = _upElement;
			if (_shouldUseCustomColors)
			{
				_sprite.color = _upColor;
			}
		}
	}

	protected virtual void UpdateEnabled()
	{
	}

	public virtual bool HandleSingleTouchBegan(FTouch touch)
	{
		_isTouchDown = false;
		if (!IsAncestryVisible())
		{
			return false;
		}
		if (!_shouldUseCustomHitRect)
		{
			_hitRect = _sprite.textureRect;
		}
		Vector2 localTouchPosition = _sprite.GetLocalTouchPosition(touch);
		if (_hitRect.Contains(localTouchPosition))
		{
			if (_isEnabled)
			{
				_sprite.element = _downElement;
				if (_shouldUseCustomColors)
				{
					_sprite.color = _downColor;
				}
				if (_clickSoundName != null)
				{
					FSoundManager.PlaySound(_clickSoundName);
				}
				if (this.SignalPress != null)
				{
					this.SignalPress(this);
				}
				_isTouchDown = true;
			}
			return true;
		}
		return false;
	}

	public virtual void HandleSingleTouchMoved(FTouch touch)
	{
		Vector2 localTouchPosition = _sprite.GetLocalTouchPosition(touch);
		if (_hitRect.CloneWithExpansion(expansionAmount).Contains(localTouchPosition))
		{
			_sprite.element = _downElement;
			if (_shouldUseCustomColors)
			{
				_sprite.color = _downColor;
			}
			_isTouchDown = true;
		}
		else
		{
			_sprite.element = _upElement;
			if (_shouldUseCustomColors)
			{
				_sprite.color = _upColor;
			}
			_isTouchDown = false;
		}
	}

	public virtual void HandleSingleTouchEnded(FTouch touch)
	{
		_isTouchDown = false;
		_sprite.element = _upElement;
		if (_shouldUseCustomColors)
		{
			_sprite.color = _upColor;
		}
		Vector2 localTouchPosition = _sprite.GetLocalTouchPosition(touch);
		if (_hitRect.CloneWithExpansion(expansionAmount).Contains(localTouchPosition))
		{
			if (this.SignalRelease != null)
			{
				this.SignalRelease(this);
			}
			if (_supportsOver && _hitRect.Contains(localTouchPosition))
			{
				_sprite.element = _overElement;
				if (_shouldUseCustomColors)
				{
					_sprite.color = _overColor;
				}
			}
		}
		else if (this.SignalReleaseOutside != null)
		{
			this.SignalReleaseOutside(this);
		}
	}

	public virtual void HandleSingleTouchCanceled(FTouch touch)
	{
		_isTouchDown = false;
		_sprite.element = _upElement;
		if (_shouldUseCustomColors)
		{
			_sprite.color = _upColor;
		}
		if (this.SignalReleaseOutside != null)
		{
			this.SignalReleaseOutside(this);
		}
	}

	public void SetAnchor(float newX, float newY)
	{
		anchorX = newX;
		anchorY = newY;
	}

	public void SetAnchor(Vector2 newAnchor)
	{
		anchorX = newAnchor.x;
		anchorY = newAnchor.y;
	}

	public Vector2 GetAnchor()
	{
		return new Vector2(_anchorX, _anchorY);
	}
}
