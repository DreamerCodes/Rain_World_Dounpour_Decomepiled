using System;
using Menu.Remix.MixedUI;
using UnityEngine;

public class FLabel : FFacetElementNode
{
	public static float defaultAnchorX = 0.5f;

	public static float defaultAnchorY = 0.5f;

	protected FFont _font;

	protected string _fontName;

	protected string _text;

	protected Color _color = Futile.white;

	protected Color _alphaColor = Futile.white;

	protected FLetterQuadLine[] _letterQuadLines;

	protected bool _isMeshDirty;

	protected float _anchorX = defaultAnchorX;

	protected float _anchorY = defaultAnchorY;

	protected bool _doesTextNeedUpdate;

	protected bool _doesLocalPositionNeedUpdate;

	protected bool _doQuadsNeedUpdate;

	protected Rect _textRect;

	protected FTextParams _textParams;

	public float FontMaxCharWidth => _font.maxCharWidth;

	public float FontLineHeight => _font.lineHeight;

	public FLabelAlignment alignment
	{
		get
		{
			if (_anchorX == 0.5f)
			{
				return FLabelAlignment.Center;
			}
			if (_anchorX == 0f)
			{
				return FLabelAlignment.Left;
			}
			if (_anchorX == 1f)
			{
				return FLabelAlignment.Right;
			}
			return FLabelAlignment.Custom;
		}
		set
		{
			switch (value)
			{
			case FLabelAlignment.Center:
				anchorX = 0.5f;
				break;
			case FLabelAlignment.Left:
				anchorX = 0f;
				break;
			case FLabelAlignment.Right:
				anchorX = 1f;
				break;
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
			if (_text != value)
			{
				_text = value;
				_doesTextNeedUpdate = true;
				CreateTextQuads();
			}
		}
	}

	public float anchorX
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
				_doesLocalPositionNeedUpdate = true;
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
			if (_anchorY != value)
			{
				_anchorY = value;
				_doesLocalPositionNeedUpdate = true;
			}
		}
	}

	public virtual Color color
	{
		get
		{
			return _color;
		}
		set
		{
			if (_color != value)
			{
				_color = value;
				_isAlphaDirty = true;
			}
		}
	}

	public virtual Rect textRect
	{
		get
		{
			if (_doesTextNeedUpdate)
			{
				CreateTextQuads();
			}
			if (_doesLocalPositionNeedUpdate)
			{
				UpdateLocalPosition();
			}
			return _textRect;
		}
	}

	[Obsolete("FLabel's boundsRect is obsolete, use textRect instead")]
	public Rect boundsRect
	{
		get
		{
			throw new NotSupportedException("boundsRect is obsolete! Use textRect instead");
		}
	}

	public FLabel(string fontName, string text)
		: this(fontName, text, new FTextParams())
	{
	}

	public FLabel(string fontName, string text, FTextParams textParams)
	{
		_fontName = fontName;
		_text = text;
		_font = Futile.atlasManager.GetFontWithName(_fontName);
		_textParams = textParams;
		Init(FFacetType.Quad, _font.element, 0);
		CreateTextQuads();
	}

	public void CreateTextQuads()
	{
		_doesTextNeedUpdate = false;
		int numberOfFacetsNeeded = _numberOfFacetsNeeded;
		_text = LabelTest.GlobalTextModifier(_text);
		_letterQuadLines = _font.GetQuadInfoForText(_text, _textParams);
		_numberOfFacetsNeeded = 0;
		int num = _letterQuadLines.Length;
		for (int i = 0; i < num; i++)
		{
			_numberOfFacetsNeeded += _letterQuadLines[i].quads.Length;
		}
		if (_isOnStage && _numberOfFacetsNeeded - numberOfFacetsNeeded != 0)
		{
			_stage.HandleFacetsChanged();
		}
		UpdateLocalPosition();
	}

	public void UpdateLocalPosition()
	{
		_doesLocalPositionNeedUpdate = false;
		float num = float.MaxValue;
		float num2 = float.MinValue;
		float num3 = float.MaxValue;
		float num4 = float.MinValue;
		int num5 = _letterQuadLines.Length;
		for (int i = 0; i < num5; i++)
		{
			FLetterQuadLine fLetterQuadLine = _letterQuadLines[i];
			num = Math.Min(fLetterQuadLine.bounds.yMin, num);
			num2 = Math.Max(fLetterQuadLine.bounds.yMax, num2);
		}
		float num6 = 0f - (num + (num2 - num) * _anchorY);
		for (int j = 0; j < num5; j++)
		{
			FLetterQuadLine fLetterQuadLine2 = _letterQuadLines[j];
			float num7 = (0f - fLetterQuadLine2.bounds.width) * _anchorX;
			num3 = Math.Min(num7, num3);
			num4 = Math.Max(num7 + fLetterQuadLine2.bounds.width, num4);
			int num8 = fLetterQuadLine2.quads.Length;
			for (int k = 0; k < num8; k++)
			{
				fLetterQuadLine2.quads[k].CalculateVectors(num7 + _font.offsetX, num6 + _font.offsetY);
			}
		}
		_textRect.x = num3;
		_textRect.y = num + num6;
		_textRect.width = num4 - num3;
		_textRect.height = num2 - num;
		_isMeshDirty = true;
	}

	public override void Redraw(bool shouldForceDirty, bool shouldUpdateDepth)
	{
		bool num = _isMatrixDirty;
		bool isAlphaDirty = _isAlphaDirty;
		UpdateDepthMatrixAlpha(shouldForceDirty, shouldUpdateDepth);
		if (shouldUpdateDepth)
		{
			UpdateFacets();
		}
		if (num || shouldForceDirty || shouldUpdateDepth)
		{
			_isMeshDirty = true;
		}
		if (isAlphaDirty || shouldForceDirty)
		{
			_isMeshDirty = true;
			_alphaColor = _color.CloneWithMultipliedAlpha(_concatenatedAlpha);
		}
		if (_doesLocalPositionNeedUpdate)
		{
			UpdateLocalPosition();
		}
		if (_isMeshDirty)
		{
			PopulateRenderLayer();
		}
	}

	public override void PopulateRenderLayer()
	{
		if (!_isOnStage || _firstFacetIndex == -1)
		{
			return;
		}
		_isMeshDirty = false;
		Vector3[] vertices = _renderLayer.vertices;
		Vector2[] uvs = _renderLayer.uvs;
		Color[] colors = _renderLayer.colors;
		int num = _firstFacetIndex * 4;
		int num2 = num + 1;
		int num3 = num + 2;
		int num4 = num + 3;
		int num5 = _letterQuadLines.Length;
		for (int i = 0; i < num5; i++)
		{
			FLetterQuad[] quads = _letterQuadLines[i].quads;
			int num6 = quads.Length;
			for (int j = 0; j < num6; j++)
			{
				FLetterQuad fLetterQuad = quads[j];
				FCharInfo charInfo = fLetterQuad.charInfo;
				_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num], fLetterQuad.topLeft, 0f);
				_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num2], fLetterQuad.topRight, 0f);
				_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num3], fLetterQuad.bottomRight, 0f);
				_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num4], fLetterQuad.bottomLeft, 0f);
				uvs[num] = charInfo.uvTopLeft;
				uvs[num2] = charInfo.uvTopRight;
				uvs[num3] = charInfo.uvBottomRight;
				uvs[num4] = charInfo.uvBottomLeft;
				colors[num] = _alphaColor;
				colors[num2] = _alphaColor;
				colors[num3] = _alphaColor;
				colors[num4] = _alphaColor;
				num += 4;
				num2 += 4;
				num3 += 4;
				num4 += 4;
			}
		}
		_renderLayer.HandleVertsChange();
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
