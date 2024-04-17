using UnityEngine;

public class FWipeSprite : FSprite
{
	protected float _wipeTopAmount = 1f;

	protected float _wipeRightAmount = 1f;

	protected float _wipeBottomAmount = 1f;

	protected float _wipeLeftAmount = 1f;

	public float wipeTopAmount
	{
		get
		{
			return _wipeTopAmount;
		}
		set
		{
			value = Mathf.Clamp01(value);
			if (_wipeTopAmount != value)
			{
				_wipeTopAmount = value;
				_isMeshDirty = true;
			}
		}
	}

	public float wipeRightAmount
	{
		get
		{
			return _wipeRightAmount;
		}
		set
		{
			value = Mathf.Clamp01(value);
			if (_wipeRightAmount != value)
			{
				_wipeRightAmount = value;
				_isMeshDirty = true;
			}
		}
	}

	public float wipeBottomAmount
	{
		get
		{
			return _wipeBottomAmount;
		}
		set
		{
			value = Mathf.Clamp01(value);
			if (_wipeBottomAmount != value)
			{
				_wipeBottomAmount = value;
				_isMeshDirty = true;
			}
		}
	}

	public float wipeLeftAmount
	{
		get
		{
			return _wipeLeftAmount;
		}
		set
		{
			value = Mathf.Clamp01(value);
			if (_wipeLeftAmount != value)
			{
				_wipeLeftAmount = value;
				_isMeshDirty = true;
			}
		}
	}

	public FWipeSprite(string elementName)
	{
		Init(FFacetType.Quad, Futile.atlasManager.GetElementWithName(elementName), 1);
		_isAlphaDirty = true;
		UpdateLocalVertices();
	}

	public override void PopulateRenderLayer()
	{
		if (_isOnStage && _firstFacetIndex != -1)
		{
			_isMeshDirty = false;
			float num = Mathf.Max(1f - wipeRightAmount, wipeLeftAmount);
			float num2 = Mathf.Min(1f - wipeRightAmount, num);
			float num3 = Mathf.Max(1f - wipeTopAmount, wipeBottomAmount);
			float num4 = Mathf.Min(1f - wipeTopAmount, num3);
			int num5 = _firstFacetIndex * 4;
			int num6 = num5 + 1;
			int num7 = num5 + 2;
			int num8 = num5 + 3;
			Vector3[] vertices = _renderLayer.vertices;
			Vector2[] uvs = _renderLayer.uvs;
			Color[] colors = _renderLayer.colors;
			float num9 = _localVertices[1].x - _localVertices[0].x;
			float num10 = _localVertices[1].y - _localVertices[2].y;
			Vector2 localVector = new Vector2(_localVertices[0].x + num9 * num2, _localVertices[3].y + num10 * num3);
			Vector2 localVector2 = new Vector2(_localVertices[0].x + num9 * num, _localVertices[3].y + num10 * num3);
			Vector2 localVector3 = new Vector2(_localVertices[0].x + num9 * num, _localVertices[3].y + num10 * num4);
			Vector2 localVector4 = new Vector2(_localVertices[0].x + num9 * num2, _localVertices[3].y + num10 * num4);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num5], localVector, 0f);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num6], localVector2, 0f);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num7], localVector3, 0f);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num8], localVector4, 0f);
			float num11 = _element.uvTopRight.x - _element.uvTopLeft.x;
			float num12 = _element.uvTopRight.y - _element.uvBottomRight.y;
			uvs[num5] = new Vector2(_element.uvTopLeft.x + num11 * num2, _element.uvBottomLeft.y + num12 * num3);
			uvs[num6] = new Vector2(_element.uvTopLeft.x + num11 * num, _element.uvBottomLeft.y + num12 * num3);
			uvs[num7] = new Vector2(_element.uvTopLeft.x + num11 * num, _element.uvBottomLeft.y + num12 * num4);
			uvs[num8] = new Vector2(_element.uvTopLeft.x + num11 * num2, _element.uvBottomLeft.y + num12 * num4);
			colors[num5] = _alphaColor;
			colors[num6] = _alphaColor;
			colors[num7] = _alphaColor;
			colors[num8] = _alphaColor;
			_renderLayer.HandleVertsChange();
		}
	}
}
