using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class FFont
{
	public const int ASCII_NEWLINE = 10;

	public const int ASCII_SPACE = 32;

	public const int ASCII_HYPHEN_MINUS = 45;

	public const int ASCII_LINEHEIGHT_REFERENCE = 77;

	private string _name;

	private FAtlasElement _element;

	private string _configPath;

	private FCharInfo[] _charInfos;

	private Dictionary<uint, FCharInfo> _charInfosByID;

	private FKerningInfo[] _kerningInfos;

	private int _kerningCount;

	private FKerningInfo _nullKerning = new FKerningInfo();

	private float _lineHeight;

	private int _configWidth;

	private float _configRatio;

	private float _maxCharWidth;

	private FTextParams _textParams;

	private float _offsetX;

	private float _offsetY;

	public string name => _name;

	public FAtlasElement element => _element;

	public FTextParams textParams => _textParams;

	public float offsetX => _offsetX;

	public float offsetY => _offsetY;

	public float maxCharWidth => _maxCharWidth;

	public float lineHeight => _lineHeight;

	public FFont(string name, FAtlasElement element, string configPath, float offsetX, float offsetY, FTextParams textParams, float fontScale)
	{
		_name = name;
		_element = element;
		_configPath = configPath;
		_textParams = textParams;
		_offsetX = offsetX * Futile.displayScale / Futile.resourceScale;
		_offsetY = offsetY * Futile.displayScale / Futile.resourceScale;
		LoadAndParseConfigFile(fontScale);
	}

	private void LoadAndParseConfigFile(float fontScale)
	{
		TextAsset textAsset = (TextAsset)Resources.Load(_configPath, typeof(TextAsset));
		if (textAsset == null)
		{
			throw new FutileException("Couldn't find font config file " + _configPath);
		}
		string[] array = new string[1] { "\n" };
		string[] array2 = textAsset.text.Split(array, StringSplitOptions.RemoveEmptyEntries);
		if (array2.Length <= 1)
		{
			array[0] = "\r\n";
			array2 = textAsset.text.Split(array, StringSplitOptions.RemoveEmptyEntries);
		}
		if (array2.Length <= 1)
		{
			array[0] = "\r";
			array2 = textAsset.text.Split(array, StringSplitOptions.RemoveEmptyEntries);
		}
		if (array2.Length <= 1)
		{
			throw new FutileException("Your font file is messed up");
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		_charInfosByID = new Dictionary<uint, FCharInfo>(127);
		FCharInfo value = new FCharInfo();
		_charInfosByID[0u] = value;
		float resourceScaleInverse = Futile.resourceScaleInverse;
		Vector2 textureSize = _element.atlas.textureSize;
		bool flag = false;
		int num4 = array2.Length;
		for (int i = 0; i < num4; i++)
		{
			string[] array3 = array2[i].Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			if (array3[0] == "common")
			{
				_configWidth = int.Parse(array3[3].Split('=')[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				_configRatio = _element.sourcePixelSize.x / (float)_configWidth;
				_lineHeight = (float)int.Parse(array3[1].Split('=')[1], NumberStyles.Any, CultureInfo.InvariantCulture) * _configRatio * resourceScaleInverse;
			}
			else if (array3[0] == "chars")
			{
				int num5 = int.Parse(array3[1].Split('=')[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				_charInfos = new FCharInfo[num5 + 1];
			}
			else if (array3[0] == "char")
			{
				FCharInfo fCharInfo = new FCharInfo();
				num = array3.Length;
				for (int j = 1; j < num; j++)
				{
					string[] array4 = array3[j].Split('=');
					string text = array4[0];
					if (text == "letter")
					{
						if (array4[1].Length >= 3)
						{
							fCharInfo.letter = array4[1].Substring(1, 1);
						}
					}
					else if (!(text == "\r"))
					{
						int num6 = int.Parse(array4[1], NumberStyles.Any, CultureInfo.InvariantCulture);
						float num7 = num6;
						switch (text)
						{
						case "id":
							fCharInfo.charID = num6;
							break;
						case "x":
							fCharInfo.x = num7 * _configRatio - _element.sourceRect.x * Futile.resourceScale;
							break;
						case "y":
							fCharInfo.y = num7 * _configRatio - _element.sourceRect.y * Futile.resourceScale;
							break;
						case "width":
							fCharInfo.width = num7 * _configRatio;
							break;
						case "height":
							fCharInfo.height = num7 * _configRatio;
							break;
						case "xoffset":
							fCharInfo.offsetX = num7 * _configRatio;
							break;
						case "yoffset":
							fCharInfo.offsetY = num7 * _configRatio;
							break;
						case "xadvance":
							fCharInfo.xadvance = num7 * _configRatio;
							break;
						case "page":
							fCharInfo.page = num6;
							break;
						}
					}
				}
				Rect rect = (fCharInfo.uvRect = new Rect(_element.uvRect.x + fCharInfo.x / textureSize.x, (textureSize.y - fCharInfo.y - fCharInfo.height) / textureSize.y - (1f - _element.uvRect.yMax), fCharInfo.width / textureSize.x, fCharInfo.height / textureSize.y));
				fCharInfo.uvTopLeft.Set(rect.xMin, rect.yMax);
				fCharInfo.uvTopRight.Set(rect.xMax, rect.yMax);
				fCharInfo.uvBottomRight.Set(rect.xMax, rect.yMin);
				fCharInfo.uvBottomLeft.Set(rect.xMin, rect.yMin);
				fCharInfo.width *= resourceScaleInverse * fontScale;
				fCharInfo.height *= resourceScaleInverse * fontScale;
				fCharInfo.offsetX *= resourceScaleInverse * fontScale;
				fCharInfo.offsetY *= resourceScaleInverse * fontScale;
				fCharInfo.xadvance *= resourceScaleInverse * fontScale;
				_charInfosByID[(uint)fCharInfo.charID] = fCharInfo;
				_charInfos[num2] = fCharInfo;
				num2++;
			}
			else if (array3[0] == "kernings")
			{
				flag = true;
				int num8 = int.Parse(array3[1].Split('=')[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				_kerningInfos = new FKerningInfo[num8 + 100];
			}
			else
			{
				if (!(array3[0] == "kerning"))
				{
					continue;
				}
				FKerningInfo fKerningInfo = new FKerningInfo();
				fKerningInfo.first = -1;
				num = array3.Length;
				for (int k = 1; k < num; k++)
				{
					string[] array5 = array3[k].Split('=');
					if (array5.Length >= 2)
					{
						string text2 = array5[0];
						int num9 = int.Parse(array5[1], NumberStyles.Any, CultureInfo.InvariantCulture);
						switch (text2)
						{
						case "first":
							fKerningInfo.first = num9;
							break;
						case "second":
							fKerningInfo.second = num9;
							break;
						case "amount":
							fKerningInfo.amount = (float)num9 * _configRatio * resourceScaleInverse;
							break;
						}
					}
				}
				if (fKerningInfo.first != -1)
				{
					_kerningInfos[num3] = fKerningInfo;
				}
				num3++;
			}
		}
		_kerningCount = num3;
		if (!flag)
		{
			_kerningInfos = new FKerningInfo[0];
		}
		if (_charInfosByID.ContainsKey(32u))
		{
			_charInfosByID[32u].offsetX = 0f;
			_charInfosByID[32u].offsetY = 0f;
		}
		for (int l = 0; l < _charInfos.Length; l++)
		{
			if (_charInfos[l] != null && _charInfos[l].width > _maxCharWidth)
			{
				_maxCharWidth = _charInfos[l].width;
			}
		}
	}

	public FLetterQuadLine[] GetQuadInfoForText(string text, FTextParams labelTextParams)
	{
		int num = 0;
		int num2 = 0;
		char[] array = text.ToCharArray();
		List<FLetterQuadLine> list = new List<FLetterQuadLine>();
		int num3 = array.Length;
		for (int i = 0; i < num3; i++)
		{
			if (array[i] == '\n')
			{
				FLetterQuadLine item = default(FLetterQuadLine);
				item.letterCount = num2;
				item.quads = new FLetterQuad[num2];
				list.Add(item);
				num++;
				num2 = 0;
			}
			else
			{
				num2++;
			}
		}
		FLetterQuadLine item2 = default(FLetterQuadLine);
		item2.letterCount = num2;
		item2.quads = new FLetterQuad[num2];
		list.Add(item2);
		FLetterQuadLine[] array2 = list.ToArray();
		FLetterQuadLine[] array3 = new FLetterQuadLine[num + 1];
		for (int j = 0; j < num + 1; j++)
		{
			array3[j] = array2[j];
		}
		num = 0;
		num2 = 0;
		float num4 = 0f;
		float num5 = 0f;
		FCharInfo fCharInfo = null;
		char c = '\0';
		float num6 = float.MaxValue;
		float num7 = float.MinValue;
		float num8 = float.MaxValue;
		float num9 = float.MinValue;
		float num10 = _lineHeight + labelTextParams.scaledLineHeightOffset + _textParams.scaledLineHeightOffset;
		for (int k = 0; k < num3; k++)
		{
			char c2 = array[k];
			if (c2 == '\n')
			{
				if (fCharInfo != null && fCharInfo.specificLineHeight != 0f)
				{
					num10 = fCharInfo.specificLineHeight + labelTextParams.scaledLineHeightOffset + _textParams.scaledLineHeightOffset;
				}
				if (num2 == 0)
				{
					array3[num].bounds = new Rect(0f, 0f, num5, num5 - num10);
				}
				else
				{
					array3[num].bounds = new Rect(num6, num8, num7 - num6, num9 - num8);
				}
				num6 = float.MaxValue;
				num7 = float.MinValue;
				num8 = float.MaxValue;
				num9 = float.MinValue;
				num4 = 0f;
				num5 -= num10;
				num++;
				num2 = 0;
			}
			else
			{
				FKerningInfo fKerningInfo = _nullKerning;
				for (int l = 0; l < _kerningCount; l++)
				{
					FKerningInfo fKerningInfo2 = _kerningInfos[l];
					if (fKerningInfo2.first == c && fKerningInfo2.second == c2)
					{
						fKerningInfo = fKerningInfo2;
					}
				}
				FLetterQuad fLetterQuad = default(FLetterQuad);
				bool flag = false;
				FCharInfo fCharInfo2;
				if (_charInfosByID.ContainsKey(c2))
				{
					fCharInfo2 = _charInfosByID[c2];
					flag = true;
				}
				else
				{
					fCharInfo2 = _charInfosByID[0u];
				}
				float num11 = fKerningInfo.amount + labelTextParams.scaledKerningOffset + _textParams.scaledKerningOffset;
				num4 = ((num2 != 0) ? (num4 + num11) : (0f - fCharInfo2.offsetX));
				fLetterQuad.charInfo = fCharInfo2;
				Rect rect = (fLetterQuad.rect = new Rect(num4 + fCharInfo2.offsetX, num5 - fCharInfo2.offsetY - fCharInfo2.height, fCharInfo2.width, fCharInfo2.height));
				array3[num].quads[num2] = fLetterQuad;
				num6 = Math.Min(num6, rect.xMin);
				num7 = Math.Max(num7, rect.xMax);
				num8 = Math.Min(num8, num5 - num10);
				num9 = Math.Max(num9, num5);
				num4 += fCharInfo2.xadvance;
				num2++;
				if (flag)
				{
					fCharInfo = fCharInfo2;
				}
			}
			c = c2;
		}
		if (num2 == 0)
		{
			array3[num].bounds = new Rect(0f, 0f, num5, num5 - num10);
		}
		else
		{
			array3[num].bounds = new Rect(num6, num8, num7 - num6, num9 - num8);
		}
		return array3;
	}

	public float LineHeight()
	{
		return _lineHeight;
	}

	public void CombineWithFont(FFont addFont, float offsetY)
	{
		List<FCharInfo> list = null;
		foreach (KeyValuePair<uint, FCharInfo> item in addFont._charInfosByID)
		{
			if (!_charInfosByID.ContainsKey(item.Key))
			{
				FCharInfo value = item.Value;
				value.specificLineHeight = addFont.LineHeight();
				value.offsetY += (_lineHeight - addFont.LineHeight()) / 2f + offsetY;
				_charInfosByID.Add(item.Key, value);
				if (list == null)
				{
					list = new List<FCharInfo>();
				}
				list.Add(value);
			}
		}
		if (list != null)
		{
			FCharInfo[] array = new FCharInfo[_charInfos.Length + list.Count];
			for (int i = 0; i < array.Length; i++)
			{
				if (i < _charInfos.Length)
				{
					array[i] = _charInfos[i];
				}
				else
				{
					array[i] = list[i - _charInfos.Length];
				}
			}
		}
		if (addFont._maxCharWidth > _maxCharWidth)
		{
			_maxCharWidth = addFont._maxCharWidth;
		}
	}
}
