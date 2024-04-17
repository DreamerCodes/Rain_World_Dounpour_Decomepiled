using System;
using UnityEngine;

public class FSliceSprite : FSprite
{
	private float _insetTop;

	private float _insetRight;

	private float _insetBottom;

	private float _insetLeft;

	private float _width;

	private float _height;

	private int _sliceCount;

	private Vector2[] _uvVertices;

	public override float width
	{
		get
		{
			return _width;
		}
		set
		{
			_width = value;
			_areLocalVerticesDirty = true;
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
			_height = value;
			_areLocalVerticesDirty = true;
		}
	}

	public FSliceSprite(string elementName, float width, float height, float insetTop, float insetRight, float insetBottom, float insetLeft)
		: this(Futile.atlasManager.GetElementWithName(elementName), width, height, insetTop, insetRight, insetBottom, insetLeft)
	{
	}

	public FSliceSprite(FAtlasElement element, float width, float height, float insetTop, float insetRight, float insetBottom, float insetLeft)
	{
		_width = width;
		_height = height;
		_insetTop = insetTop;
		_insetRight = insetRight;
		_insetBottom = insetBottom;
		_insetLeft = insetLeft;
		Init(FFacetType.Quad, element, 0);
		_isAlphaDirty = true;
		UpdateLocalVertices();
	}

	public override void HandleElementChanged()
	{
		SetupSlices();
	}

	public void SetupSlices()
	{
		_insetTop = Math.Max(0f, _insetTop);
		_insetRight = Math.Max(0f, _insetRight);
		_insetBottom = Math.Max(0f, _insetBottom);
		_insetLeft = Math.Max(0f, _insetLeft);
		_sliceCount = 1;
		if (_insetTop > 0f)
		{
			_sliceCount++;
		}
		if (_insetRight > 0f)
		{
			_sliceCount++;
		}
		if (_insetLeft > 0f)
		{
			_sliceCount++;
		}
		if (_insetBottom > 0f)
		{
			_sliceCount++;
		}
		if (_insetTop > 0f && _insetRight > 0f)
		{
			_sliceCount++;
		}
		if (_insetTop > 0f && _insetLeft > 0f)
		{
			_sliceCount++;
		}
		if (_insetBottom > 0f && _insetRight > 0f)
		{
			_sliceCount++;
		}
		if (_insetBottom > 0f && _insetLeft > 0f)
		{
			_sliceCount++;
		}
		_numberOfFacetsNeeded = _sliceCount;
		_localVertices = new Vector2[_sliceCount * 4];
		_uvVertices = new Vector2[_sliceCount * 4];
		_areLocalVerticesDirty = true;
		if (_numberOfFacetsNeeded != _sliceCount)
		{
			_numberOfFacetsNeeded = _sliceCount;
			if (_isOnStage)
			{
				_stage.HandleFacetsChanged();
			}
		}
		UpdateLocalVertices();
	}

	public override void UpdateLocalVertices()
	{
		_areLocalVerticesDirty = false;
		Rect uvRect = base.element.uvRect;
		float num = Math.Max(0f, Math.Min(_insetTop, _element.sourceSize.y - _insetBottom));
		float num2 = Math.Max(0f, Math.Min(_insetRight, _element.sourceSize.x - _insetLeft));
		float num3 = Math.Max(0f, Math.Min(_insetBottom, _element.sourceSize.y - _insetTop));
		float num4 = Math.Max(0f, Math.Min(_insetLeft, _element.sourceSize.x - _insetRight));
		float num5 = uvRect.height * (num / _element.sourceSize.y);
		float num6 = uvRect.width * (num4 / _element.sourceSize.x);
		float num7 = uvRect.height * (num3 / _element.sourceSize.y);
		float num8 = uvRect.width * (num2 / _element.sourceSize.x);
		_textureRect.x = (0f - _anchorX) * _width;
		_textureRect.y = (0f - _anchorY) * _height;
		_textureRect.width = _width;
		_textureRect.height = _height;
		_localRect = _textureRect;
		float xMin = _localRect.xMin;
		float xMax = _localRect.xMax;
		float yMin = _localRect.yMin;
		float yMax = _localRect.yMax;
		float xMin2 = uvRect.xMin;
		float xMax2 = uvRect.xMax;
		float yMin2 = uvRect.yMin;
		float yMax2 = uvRect.yMax;
		int num9 = 0;
		for (int i = 0; i < 9; i++)
		{
			switch (i)
			{
			case 0:
				_localVertices[num9].Set(xMin + num4, yMax - num);
				_localVertices[num9 + 1].Set(xMax - num2, yMax - num);
				_localVertices[num9 + 2].Set(xMax - num2, yMin + num3);
				_localVertices[num9 + 3].Set(xMin + num4, yMin + num3);
				_uvVertices[num9].Set(xMin2 + num6, yMax2 - num5);
				_uvVertices[num9 + 1].Set(xMax2 - num8, yMax2 - num5);
				_uvVertices[num9 + 2].Set(xMax2 - num8, yMin2 + num7);
				_uvVertices[num9 + 3].Set(xMin2 + num6, yMin2 + num7);
				num9 += 4;
				continue;
			case 1:
				if (_insetTop > 0f)
				{
					_localVertices[num9].Set(xMin + num4, yMax);
					_localVertices[num9 + 1].Set(xMax - num2, yMax);
					_localVertices[num9 + 2].Set(xMax - num2, yMax - num);
					_localVertices[num9 + 3].Set(xMin + num4, yMax - num);
					_uvVertices[num9].Set(xMin2 + num6, yMax2);
					_uvVertices[num9 + 1].Set(xMax2 - num8, yMax2);
					_uvVertices[num9 + 2].Set(xMax2 - num8, yMax2 - num5);
					_uvVertices[num9 + 3].Set(xMin2 + num6, yMax2 - num5);
					num9 += 4;
					continue;
				}
				break;
			}
			if (i == 2 && _insetRight > 0f)
			{
				_localVertices[num9].Set(xMax - num2, yMax - num);
				_localVertices[num9 + 1].Set(xMax, yMax - num);
				_localVertices[num9 + 2].Set(xMax, yMin + num3);
				_localVertices[num9 + 3].Set(xMax - num2, yMin + num3);
				_uvVertices[num9].Set(xMax2 - num8, yMax2 - num5);
				_uvVertices[num9 + 1].Set(xMax2, yMax2 - num5);
				_uvVertices[num9 + 2].Set(xMax2, yMin2 + num7);
				_uvVertices[num9 + 3].Set(xMax2 - num8, yMin2 + num7);
				num9 += 4;
			}
			else if (i == 3 && _insetBottom > 0f)
			{
				_localVertices[num9].Set(xMin + num4, yMin + num3);
				_localVertices[num9 + 1].Set(xMax - num2, yMin + num3);
				_localVertices[num9 + 2].Set(xMax - num2, yMin);
				_localVertices[num9 + 3].Set(xMin + num4, yMin);
				_uvVertices[num9].Set(xMin2 + num6, yMin2 + num7);
				_uvVertices[num9 + 1].Set(xMax2 - num8, yMin2 + num7);
				_uvVertices[num9 + 2].Set(xMax2 - num8, yMin2);
				_uvVertices[num9 + 3].Set(xMin2 + num6, yMin2);
				num9 += 4;
			}
			else if (i == 4 && _insetLeft > 0f)
			{
				_localVertices[num9].Set(xMin, yMax - num);
				_localVertices[num9 + 1].Set(xMin + num4, yMax - num);
				_localVertices[num9 + 2].Set(xMin + num4, yMin + num3);
				_localVertices[num9 + 3].Set(xMin, yMin + num3);
				_uvVertices[num9].Set(xMin2, yMax2 - num5);
				_uvVertices[num9 + 1].Set(xMin2 + num6, yMax2 - num5);
				_uvVertices[num9 + 2].Set(xMin2 + num6, yMin2 + num7);
				_uvVertices[num9 + 3].Set(xMin2, yMin2 + num7);
				num9 += 4;
			}
			else if (i == 5 && _insetTop > 0f && _insetLeft > 0f)
			{
				_localVertices[num9].Set(xMin, yMax);
				_localVertices[num9 + 1].Set(xMin + num4, yMax);
				_localVertices[num9 + 2].Set(xMin + num4, yMax - num);
				_localVertices[num9 + 3].Set(xMin, yMax - num);
				_uvVertices[num9].Set(xMin2, yMax2);
				_uvVertices[num9 + 1].Set(xMin2 + num6, yMax2);
				_uvVertices[num9 + 2].Set(xMin2 + num6, yMax2 - num5);
				_uvVertices[num9 + 3].Set(xMin2, yMax2 - num5);
				num9 += 4;
			}
			else if (i == 6 && _insetTop > 0f && _insetRight > 0f)
			{
				_localVertices[num9].Set(xMax - num2, yMax);
				_localVertices[num9 + 1].Set(xMax, yMax);
				_localVertices[num9 + 2].Set(xMax, yMax - num);
				_localVertices[num9 + 3].Set(xMax - num2, yMax - num);
				_uvVertices[num9].Set(xMax2 - num8, yMax2);
				_uvVertices[num9 + 1].Set(xMax2, yMax2);
				_uvVertices[num9 + 2].Set(xMax2, yMax2 - num5);
				_uvVertices[num9 + 3].Set(xMax2 - num8, yMax2 - num5);
				num9 += 4;
			}
			else if (i == 7 && _insetBottom > 0f && _insetRight > 0f)
			{
				_localVertices[num9].Set(xMax - num2, yMin + num3);
				_localVertices[num9 + 1].Set(xMax, yMin + num3);
				_localVertices[num9 + 2].Set(xMax, yMin);
				_localVertices[num9 + 3].Set(xMax - num2, yMin);
				_uvVertices[num9].Set(xMax2 - num8, yMin2 + num7);
				_uvVertices[num9 + 1].Set(xMax2, yMin2 + num7);
				_uvVertices[num9 + 2].Set(xMax2, yMin2);
				_uvVertices[num9 + 3].Set(xMax2 - num8, yMin2);
				num9 += 4;
			}
			else if (i == 8 && _insetBottom > 0f && _insetLeft > 0f)
			{
				_localVertices[num9].Set(xMin, yMin + num3);
				_localVertices[num9 + 1].Set(xMin + num4, yMin + num3);
				_localVertices[num9 + 2].Set(xMin + num4, yMin);
				_localVertices[num9 + 3].Set(xMin, yMin);
				_uvVertices[num9].Set(xMin2, yMin2 + num7);
				_uvVertices[num9 + 1].Set(xMin2 + num6, yMin2 + num7);
				_uvVertices[num9 + 2].Set(xMin2 + num6, yMin2);
				_uvVertices[num9 + 3].Set(xMin2, yMin2);
				num9 += 4;
			}
		}
		_isMeshDirty = true;
	}

	public override void PopulateRenderLayer()
	{
		if (_isOnStage && _firstFacetIndex != -1)
		{
			_isMeshDirty = false;
			for (int i = 0; i < _sliceCount; i++)
			{
				int num = i * 4;
				int num2 = (_firstFacetIndex + i) * 4;
				int num3 = num2 + 1;
				int num4 = num2 + 2;
				int num5 = num2 + 3;
				Vector3[] vertices = _renderLayer.vertices;
				Vector2[] uvs = _renderLayer.uvs;
				Color[] colors = _renderLayer.colors;
				_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num2], _localVertices[num], 0f);
				_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num3], _localVertices[num + 1], 0f);
				_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num4], _localVertices[num + 2], 0f);
				_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num5], _localVertices[num + 3], 0f);
				uvs[num2] = _uvVertices[num];
				uvs[num3] = _uvVertices[num + 1];
				uvs[num4] = _uvVertices[num + 2];
				uvs[num5] = _uvVertices[num + 3];
				colors[num2] = _alphaColor;
				colors[num3] = _alphaColor;
				colors[num4] = _alphaColor;
				colors[num5] = _alphaColor;
				_renderLayer.HandleVertsChange();
			}
		}
	}

	public void SetInsets(float insetTop, float insetRight, float insetBottom, float insetLeft)
	{
		_insetTop = insetTop;
		_insetRight = insetRight;
		_insetBottom = insetBottom;
		_insetLeft = insetLeft;
		SetupSlices();
	}
}
