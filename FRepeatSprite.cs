using UnityEngine;

public class FRepeatSprite : FSprite
{
	protected float _width;

	protected float _height;

	protected float _scrollX;

	protected float _scrollY;

	protected float _textureWidth;

	protected float _textureHeight;

	public override Rect textureRect => _textureRect;

	public override Rect localRect => _localRect;

	public override float width
	{
		get
		{
			return _width;
		}
		set
		{
			if (_width != value)
			{
				_width = value;
				_areLocalVerticesDirty = true;
			}
		}
	}

	public override float height
	{
		get
		{
			return _height;
		}
		set
		{
			if (_height != value)
			{
				_height = value;
				_areLocalVerticesDirty = true;
			}
		}
	}

	public override float anchorX
	{
		get
		{
			return _anchorX;
		}
		set
		{
			if (_anchorX != value)
			{
				_anchorX = value;
				_areLocalVerticesDirty = true;
			}
		}
	}

	public override float anchorY
	{
		get
		{
			return _anchorY;
		}
		set
		{
			if (_anchorY != value)
			{
				_anchorY = value;
				_areLocalVerticesDirty = true;
			}
		}
	}

	public float scrollX
	{
		get
		{
			return _scrollX;
		}
		set
		{
			if (_scrollX != value)
			{
				_scrollX = value;
				_isMeshDirty = true;
			}
		}
	}

	public float scrollY
	{
		get
		{
			return _scrollY;
		}
		set
		{
			if (_scrollY != value)
			{
				_scrollY = value;
				_isMeshDirty = true;
			}
		}
	}

	public FRepeatSprite(string elementName, float width, float height)
		: this(elementName, width, height, 0f, 0f)
	{
	}

	public FRepeatSprite(string elementName, float width, float height, float scrollX, float scrollY)
	{
		_width = width;
		_height = height;
		_scrollX = scrollX;
		_scrollY = scrollY;
		Init(FFacetType.Quad, Futile.atlasManager.GetElementWithName(elementName), 1);
		if (!_element.atlas.isSingleImage)
		{
			throw new FutileException("ScrollingSprite must be used with a single image, not an atlas! Use Futile.atlasManager.LoadImage()");
		}
		_isAlphaDirty = true;
		UpdateLocalVertices();
	}

	public override void HandleElementChanged()
	{
		base.HandleElementChanged();
		_textureWidth = _element.atlas.textureSize.x * Futile.resourceScaleInverse;
		_textureHeight = _element.atlas.textureSize.y * Futile.resourceScaleInverse;
		_areLocalVerticesDirty = true;
		UpdateLocalVertices();
	}

	public override void UpdateLocalVertices()
	{
		_areLocalVerticesDirty = false;
		_textureRect.width = _width;
		_textureRect.height = _height;
		_textureRect.x = (0f - _anchorX) * _width;
		_textureRect.y = (0f - _anchorY) * _height;
		_localRect = _textureRect;
		_localVertices[0].Set(_textureRect.xMin, _textureRect.yMax);
		_localVertices[1].Set(_textureRect.xMax, _textureRect.yMax);
		_localVertices[2].Set(_textureRect.xMax, _textureRect.yMin);
		_localVertices[3].Set(_textureRect.xMin, _textureRect.yMin);
		_isMeshDirty = true;
	}

	public override void PopulateRenderLayer()
	{
		if (_isOnStage && _firstFacetIndex != -1)
		{
			_isMeshDirty = false;
			int num = _firstFacetIndex * 4;
			int num2 = num + 1;
			int num3 = num + 2;
			int num4 = num + 3;
			Vector3[] vertices = _renderLayer.vertices;
			Vector2[] uvs = _renderLayer.uvs;
			Color[] colors = _renderLayer.colors;
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num], _localVertices[0], 0f);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num2], _localVertices[1], 0f);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num3], _localVertices[2], 0f);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num4], _localVertices[3], 0f);
			uvs[num] = new Vector2(_scrollX / _textureWidth, _scrollY / _textureHeight + _height / _textureHeight);
			uvs[num2] = new Vector2(_scrollX / _textureWidth + _width / _textureWidth, _scrollY / _textureHeight + _height / _textureHeight);
			uvs[num3] = new Vector2(_scrollX / _textureWidth + _width / _textureWidth, _scrollY / _textureHeight);
			uvs[num4] = new Vector2(_scrollX / _textureWidth, _scrollY / _textureHeight);
			colors[num] = _alphaColor;
			colors[num2] = _alphaColor;
			colors[num3] = _alphaColor;
			colors[num4] = _alphaColor;
			_renderLayer.HandleVertsChange();
		}
	}
}
