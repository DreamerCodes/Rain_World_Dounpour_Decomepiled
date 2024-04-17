using RWCustom;
using UnityEngine;

namespace Menu.Remix.MixedUI;

public class GlowGradient
{
	protected readonly FContainer _container;

	public readonly FSprite sprite;

	private float _radH;

	private float _radV;

	private float _alpha;

	private Vector2 _centerPos;

	private Color _color = new Color(0.01f, 0.01f, 0.01f);

	public float radH
	{
		get
		{
			return _radH;
		}
		set
		{
			if (_radH != value)
			{
				_radH = value;
				OnChange();
			}
		}
	}

	public float radV
	{
		get
		{
			return _radV;
		}
		set
		{
			if (_radV != value)
			{
				_radV = value;
				OnChange();
			}
		}
	}

	public Vector2 size
	{
		get
		{
			return new Vector2(_radH, _radV) * 2f;
		}
		set
		{
			_radH = value.x / 2f;
			_radV = value.y / 2f;
			OnChange();
		}
	}

	public float alpha
	{
		get
		{
			return _alpha;
		}
		set
		{
			if (_alpha != value)
			{
				_alpha = value;
				OnChange();
			}
		}
	}

	public Vector2 centerPos
	{
		get
		{
			return _centerPos;
		}
		set
		{
			if (!(_centerPos == value))
			{
				_centerPos = value;
				OnChange();
			}
		}
	}

	public Vector2 pos
	{
		get
		{
			return _centerPos - new Vector2(_radH, _radV);
		}
		set
		{
			centerPos = value + new Vector2(_radH, _radV);
		}
	}

	public Color color
	{
		get
		{
			return _color;
		}
		set
		{
			if (!(_color == value))
			{
				_color = value;
				OnChange();
			}
		}
	}

	public bool isHidden { get; private set; }

	public GlowGradient(FContainer container, Vector2 centerPos, float radH, float radV, float alpha = 0.5f)
	{
		isHidden = false;
		_container = container;
		_centerPos = centerPos;
		_radH = radH;
		_radV = radV;
		_alpha = alpha;
		sprite = new FSprite("Futile_White")
		{
			scaleX = this.radH / 8f,
			scaleY = this.radV / 8f,
			shader = Custom.rainWorld.Shaders["FlatLight"],
			color = color,
			alpha = this.alpha,
			anchorX = 0.5f,
			anchorY = 0.5f,
			x = this.centerPos.x,
			y = this.centerPos.y
		};
		_container.AddChild(sprite);
	}

	public GlowGradient(FContainer container, Vector2 pos, Vector2 size, float alpha = 0.5f)
		: this(container, pos + size / 2f, size.x / 2f, size.y / 2f, alpha)
	{
	}

	public void OnChange()
	{
		sprite.scaleX = radH / 8f;
		sprite.scaleY = radV / 8f;
		sprite.color = color;
		sprite.x = centerPos.x;
		sprite.y = centerPos.y;
		sprite.alpha = alpha;
	}

	public void Hide()
	{
		if (!isHidden)
		{
			isHidden = true;
			sprite.isVisible = false;
		}
	}

	public void Show()
	{
		if (isHidden)
		{
			isHidden = false;
			sprite.isVisible = true;
		}
	}
}
