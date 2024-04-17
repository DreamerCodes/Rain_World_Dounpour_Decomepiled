using System;
using UnityEngine;

public class FRadialWipeSprite : FSprite
{
	protected float _baseAngle;

	protected float _percentage;

	protected bool _isClockwise;

	protected Vector2[] _meshVertices = new Vector2[7];

	protected Vector2[] _uvVertices = new Vector2[7];

	public float baseAngle
	{
		get
		{
			return _baseAngle;
		}
		set
		{
			value = (value + 36000000f) % 360f;
			if (_baseAngle != value)
			{
				_baseAngle = value;
				_isMeshDirty = true;
			}
		}
	}

	public float percentage
	{
		get
		{
			return _percentage;
		}
		set
		{
			value = Mathf.Max(0f, Mathf.Min(1f, value));
			if (_percentage != value)
			{
				_percentage = value;
				_isMeshDirty = true;
			}
		}
	}

	public bool isClockwise
	{
		get
		{
			return _isClockwise;
		}
		set
		{
			if (_isClockwise != value)
			{
				_isClockwise = value;
				_isMeshDirty = true;
			}
		}
	}

	public FRadialWipeSprite(string elementName, bool isClockwise, float baseAngle, float percentage)
	{
		_isClockwise = isClockwise;
		_baseAngle = (baseAngle + 36000000f) % 360f;
		_percentage = Mathf.Clamp01(percentage);
		Init(FFacetType.Triangle, Futile.atlasManager.GetElementWithName(elementName), 5);
		_isAlphaDirty = true;
		UpdateLocalVertices();
	}

	private void CalculateTheRadialVertices()
	{
		float num = ((!_isClockwise) ? (360f - _baseAngle) : _baseAngle);
		float num2 = num * ((float)Math.PI / 180f);
		float num3 = num2 + _percentage * ((float)Math.PI * 2f);
		float num4 = _localRect.width * 0.5f;
		float num5 = _localRect.height * 0.5f;
		Vector2 vector = new Vector2(num5, num4);
		Vector2 vector2 = new Vector2(0f - num5, num4);
		Vector2 vector3 = new Vector2(0f - num5, 0f - num4);
		Vector2 vector4 = new Vector2(num5, 0f - num4);
		float num6 = 0f - Mathf.Atan2(vector.x, vector.y) + (float)Math.PI / 2f;
		float num7 = 0f - Mathf.Atan2(vector2.x, vector2.y) + (float)Math.PI / 2f;
		float num8 = 0f - Mathf.Atan2(vector3.x, vector3.y) + (float)Math.PI / 2f;
		float num9 = 0f - Mathf.Atan2(vector4.x, vector4.y) + (float)Math.PI / 2f;
		num6 = (num6 + (float)Math.PI * 20000f) % ((float)Math.PI * 2f);
		num7 = (num7 + (float)Math.PI * 20000f) % ((float)Math.PI * 2f);
		num8 = (num8 + (float)Math.PI * 20000f) % ((float)Math.PI * 2f);
		num9 = (num9 + (float)Math.PI * 20000f) % ((float)Math.PI * 2f);
		float num10;
		float num11;
		float num12;
		float num13;
		if (num2 < num6)
		{
			num10 = num6;
			num11 = num7;
			num12 = num8;
			num13 = num9;
		}
		else if (num2 >= num6 && num2 < num7)
		{
			num10 = num7;
			num11 = num8;
			num12 = num9;
			num13 = num6 + (float)Math.PI * 2f;
		}
		else if (num2 >= num7 && num2 < num8)
		{
			num10 = num8;
			num11 = num9;
			num12 = num6 + (float)Math.PI * 2f;
			num13 = num7 + (float)Math.PI * 2f;
		}
		else if (num2 >= num8 && num2 < num9)
		{
			num10 = num9;
			num11 = num6 + (float)Math.PI * 2f;
			num12 = num7 + (float)Math.PI * 2f;
			num13 = num8 + (float)Math.PI * 2f;
		}
		else
		{
			num10 = num6 + (float)Math.PI * 2f;
			num11 = num7 + (float)Math.PI * 2f;
			num12 = num8 + (float)Math.PI * 2f;
			num13 = num9 + (float)Math.PI * 2f;
		}
		float num14 = 1000000f;
		for (int i = 0; i < 6; i++)
		{
			float num15 = 0f;
			if (i < 5)
			{
				num15 = num2 + (num3 - num2) / 5f * (float)i;
				if (i != 0)
				{
					if (i == 1 && num3 > num10)
					{
						num15 = num10;
					}
					else if (i == 2 && num3 > num11)
					{
						num15 = num11;
					}
					else if (i == 3 && num3 > num12)
					{
						num15 = num12;
					}
					else if (i == 4 && num3 > num13)
					{
						num15 = num13;
					}
					else if (num3 > num13)
					{
						num15 = Mathf.Max(num15, num13);
					}
					else if (num3 > num12)
					{
						num15 = Mathf.Max(num15, num12);
					}
					else if (num3 > num11)
					{
						num15 = Mathf.Max(num15, num11);
					}
					else if (num3 > num10)
					{
						num15 = Mathf.Max(num15, num10);
					}
				}
			}
			else
			{
				num15 = num3;
			}
			num15 = (num15 + (float)Math.PI * 20000f) % ((float)Math.PI * 2f);
			float num16 = Mathf.Cos(0f - num15 + (float)Math.PI / 2f) * num14;
			float num17 = Mathf.Sin(0f - num15 + (float)Math.PI / 2f) * num14;
			if (num15 < num6)
			{
				num16 *= num5 / num17;
				num17 = num5;
			}
			else if (num15 >= num6 && num15 < num7)
			{
				num17 *= num4 / num16;
				num16 = num4;
			}
			else if (num15 >= num7 && num15 < num8)
			{
				num16 *= (0f - num5) / num17;
				num17 = 0f - num5;
			}
			else if (num15 >= num8 && num15 < num9)
			{
				num17 *= (0f - num4) / num16;
				num16 = 0f - num4;
			}
			else if (num15 >= num9)
			{
				num16 *= num5 / num17;
				num17 = num5;
			}
			if (!_isClockwise)
			{
				num16 = 0f - num16;
			}
			_meshVertices[i] = new Vector2(num16, num17);
		}
		_meshVertices[6] = new Vector2(0f, 0f);
		Rect uvRect = _element.uvRect;
		Vector2 center = uvRect.center;
		for (int j = 0; j < 7; j++)
		{
			_uvVertices[j].x = center.x + _meshVertices[j].x / _localRect.width * uvRect.width;
			_uvVertices[j].y = center.y + _meshVertices[j].y / _localRect.height * uvRect.height;
		}
		float num18 = _localRect.center.x;
		float num19 = _localRect.center.y;
		for (int k = 0; k < 7; k++)
		{
			_meshVertices[k].x += num18;
			_meshVertices[k].y += num19;
		}
	}

	public override void PopulateRenderLayer()
	{
		if (_isOnStage && _firstFacetIndex != -1)
		{
			_isMeshDirty = false;
			CalculateTheRadialVertices();
			Vector3[] vertices = _renderLayer.vertices;
			Vector2[] uvs = _renderLayer.uvs;
			Color[] colors = _renderLayer.colors;
			int num = _firstFacetIndex * 3;
			for (int i = 0; i < 15; i++)
			{
				colors[num + i] = _alphaColor;
			}
			uvs[num] = _uvVertices[6];
			uvs[num + 1] = _uvVertices[0];
			uvs[num + 2] = _uvVertices[1];
			uvs[num + 3] = _uvVertices[6];
			uvs[num + 4] = _uvVertices[1];
			uvs[num + 5] = _uvVertices[2];
			uvs[num + 6] = _uvVertices[6];
			uvs[num + 7] = _uvVertices[2];
			uvs[num + 8] = _uvVertices[3];
			uvs[num + 9] = _uvVertices[6];
			uvs[num + 10] = _uvVertices[3];
			uvs[num + 11] = _uvVertices[4];
			uvs[num + 12] = _uvVertices[6];
			uvs[num + 13] = _uvVertices[4];
			uvs[num + 14] = _uvVertices[5];
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num], _meshVertices[6], 0f);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num + 1], _meshVertices[0], 0f);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num + 2], _meshVertices[1], 0f);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num + 3], _meshVertices[6], 0f);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num + 4], _meshVertices[1], 0f);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num + 5], _meshVertices[2], 0f);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num + 6], _meshVertices[6], 0f);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num + 7], _meshVertices[2], 0f);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num + 8], _meshVertices[3], 0f);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num + 9], _meshVertices[6], 0f);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num + 10], _meshVertices[3], 0f);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num + 11], _meshVertices[4], 0f);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num + 12], _meshVertices[6], 0f);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num + 13], _meshVertices[4], 0f);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num + 14], _meshVertices[5], 0f);
			_renderLayer.HandleVertsChange();
		}
	}
}
