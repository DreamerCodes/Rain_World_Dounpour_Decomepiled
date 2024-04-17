using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using RWCustom;
using UnityEngine;

namespace Menu.Remix.MixedUI;

public static class LabelTest
{
	private static MenuLabel _tester;

	private static MenuLabel _testerB;

	private static InGameTranslator.LanguageID lastLanguage = null;

	private static float _lineHeight = 15f;

	private static float _lineHeightB = 30f;

	private static float _lineHalfHeight = 7.5f;

	private static float _lineHalfHeightB = 15f;

	private static float _textHeight = 15f;

	private static float _textHeightB = 30f;

	private static float _charMean = 6.5f;

	private static float _charMeanB = 11.1f;

	private static int _charLim = 615;

	private static int _charLimB = 180;

	private const string _omitDots = "...";

	private static float[] _omit = new float[2] { -1f, -1f };

	internal static void Initialize(Menu menu)
	{
		InGameTranslator.LanguageID currentLanguage = Custom.rainWorld.inGameTranslator.currentLanguage;
		if (!(lastLanguage == currentLanguage))
		{
			lastLanguage = currentLanguage;
			if (_tester != null)
			{
				_tester.RemoveSprites();
				_tester = null;
			}
			if (_testerB != null)
			{
				_testerB.RemoveSprites();
				_testerB = null;
			}
			_tester = new MenuLabel(menu, menu.pages[0], "", new Vector2(10000f, 10000f), new Vector2(10000f, 100f), bigText: false);
			_tester.label.alpha = 0f;
			_tester.label.RemoveFromContainer();
			_testerB = new MenuLabel(menu, menu.pages[0], "", new Vector2(10000f, 10500f), new Vector2(10000f, 300f), bigText: true);
			_testerB.label.alpha = 0f;
			_testerB.label.RemoveFromContainer();
			_omit = new float[2] { -1f, -1f };
			_tester.text = GetTesterText(currentLanguage);
			_testerB.text = _tester.text;
			_textHeight = _tester.label.textRect.height;
			_textHeightB = _testerB.label.textRect.height;
			_tester.text = _tester.text + "\n" + _tester.text;
			_testerB.text = _tester.text;
			_lineHeight = _tester.label.textRect.height - _textHeight;
			_lineHeightB = _testerB.label.textRect.height - _textHeightB;
			_tester.text = "ABCDEFGHIJKLMNOPQRSTUVWXYZ, abcdefghijklmnopqrstuvwxyz. abcdefghijklmnopqrstuvwxyz! abcdefghijklmnopqrstuvwxyz?";
			_testerB.text = "ABCDEFGHIJKLMNOPQRSTUVWXYZ, abcdefghijklmnopqrstuvwxyz. abcdefghijklmnopqrstuvwxyz! abcdefghijklmnopqrstuvwxyz?";
			_charMean = _tester.label.textRect.width / (float)"ABCDEFGHIJKLMNOPQRSTUVWXYZ, abcdefghijklmnopqrstuvwxyz. abcdefghijklmnopqrstuvwxyz! abcdefghijklmnopqrstuvwxyz?".Length;
			_charMeanB = _testerB.label.textRect.width / (float)"ABCDEFGHIJKLMNOPQRSTUVWXYZ, abcdefghijklmnopqrstuvwxyz. abcdefghijklmnopqrstuvwxyz! abcdefghijklmnopqrstuvwxyz?".Length;
			_charLim = Mathf.FloorToInt(60000f / (_lineHeight * _charMean));
			_charLimB = Mathf.FloorToInt(60000f / (_lineHeightB * _charMeanB));
			if (InGameTranslator.LanguageID.UsesLargeFont(currentLanguage))
			{
				_lineHeight /= 2f;
				_lineHeightB /= 2f;
				_textHeight /= 2f;
				_textHeightB /= 2f;
			}
			_lineHalfHeight = _lineHeight / 2f;
			_lineHalfHeightB = _lineHeightB / 2f;
			MachineConnector.LogInfo($"LabelTest Initialized for {currentLanguage.value}) th: {_textHeight:0.0} thB: {_textHeightB:0.00} / lh: {_lineHeight:0.0} lhB: {_lineHeightB:0.0} / cm: {_charMean:0.0} cmB: {_charMeanB:0.0} / cl: {_charLim} clB: {_charLimB}");
		}
	}

	private static string GetTesterText(InGameTranslator.LanguageID lang)
	{
		if (lang == InGameTranslator.LanguageID.Japanese || lang == InGameTranslator.LanguageID.Chinese)
		{
			return "用";
		}
		if (lang == InGameTranslator.LanguageID.Korean)
		{
			return "클";
		}
		return "A";
	}

	public static FLabel CreateFLabel(string text, bool bigText = false)
	{
		return UIelement.FLabelCreate(text, bigText);
	}

	public static float LineHeight(bool bigText)
	{
		if (bigText)
		{
			return _lineHeightB;
		}
		return _lineHeight;
	}

	public static float LineHalfHeight(bool bigText)
	{
		if (ModManager.NonPrepackagedModsInstalled)
		{
			return 0f;
		}
		if (bigText)
		{
			return _lineHalfHeightB;
		}
		return _lineHalfHeight;
	}

	public static float CharMean(bool bigText)
	{
		if (bigText)
		{
			return _charMeanB;
		}
		return _charMean;
	}

	public static int CharLimit(bool bigText)
	{
		if (bigText)
		{
			return _charLimB;
		}
		return _charLim;
	}

	public static string GetFont(bool bigText)
	{
		if (bigText)
		{
			return Custom.GetDisplayFont();
		}
		return Custom.GetFont();
	}

	public static float GetWidth(string text, bool bigText = false)
	{
		if (string.IsNullOrEmpty(text))
		{
			return 0f;
		}
		FFont fontWithName = Futile.atlasManager.GetFontWithName(GetFont(bigText));
		float num = 0f;
		FLetterQuadLine[] quadInfoForText = fontWithName.GetQuadInfoForText(text, new FTextParams());
		for (int i = 0; i < quadInfoForText.Length; i++)
		{
			FLetterQuadLine fLetterQuadLine = quadInfoForText[i];
			float a = num;
			Rect bounds = fLetterQuadLine.bounds;
			num = Mathf.Max(a, bounds.width);
		}
		return num;
	}

	public static string TrimText(string text, float width, bool addDots = false, bool bigText = false)
	{
		float num = 0f;
		string text2 = _UglyHardcodedOmitDots();
		if (addDots)
		{
			num = _omit[bigText ? 1 : 0];
			if (num < 0f)
			{
				num = GetWidth(text2, bigText);
				_omit[bigText ? 1 : 0] = num;
			}
		}
		if (GetWidth(text, bigText) < width)
		{
			return text;
		}
		if (addDots)
		{
			width -= num;
		}
		int num2 = 0;
		int num3 = text.Length;
		while (num2 <= num3)
		{
			int num4 = (num2 + num3) / 2;
			if (GetWidth(text.Substring(0, num4), bigText) < width)
			{
				num2 = num4 + 1;
			}
			else
			{
				num3 = num4 - 1;
			}
		}
		return text.Substring(0, num2 - 1).TrimEnd() + (addDots ? text2 : "");
	}

	private static string _UglyHardcodedOmitDots()
	{
		InGameTranslator.LanguageID currentLanguage = Custom.rainWorld.inGameTranslator.currentLanguage;
		if (currentLanguage == InGameTranslator.LanguageID.German)
		{
			return " ...";
		}
		if (currentLanguage == InGameTranslator.LanguageID.Chinese)
		{
			return "……";
		}
		return "...";
	}

	public static string WrapText(this string text, bool bigText, float width, bool forceWrapping = false)
	{
		FFont fontWithName = Futile.atlasManager.GetFontWithName(GetFont(bigText));
		string text2 = Custom.ReplaceWordWrapLineDelimeters(text);
		if ((!InGameTranslator.LanguageID.WordWrappingAllowed(Custom.rainWorld.inGameTranslator.currentLanguage) || text.Contains("<WWLINE>")) && !forceWrapping)
		{
			return text2;
		}
		string[] array = text2.Replace("\r\n", "\n").Split('\n');
		StringBuilder stringBuilder = new StringBuilder();
		if (width > CharMean(bigText) * 20f)
		{
			width -= CharMean(bigText);
		}
		for (int j = 0; j < array.Length; j++)
		{
			stringBuilder.Append(_WrapLine(fontWithName, array[j], width));
			if (j < array.Length - 1)
			{
				stringBuilder.Append('\n');
			}
		}
		return stringBuilder.Replace("\n", Environment.NewLine).ToString();
		static void _TrimWhitespace(ref int i, string t)
		{
			while (i < t.Length - 1 && t[i] != '\n' && char.IsWhiteSpace(t[i]))
			{
				i++;
			}
		}
		static string _WrapLine(FFont font, string t, float width)
		{
			StringBuilder stringBuilder2 = new StringBuilder(t);
			List<FLetterQuad> list = new List<FLetterQuad>();
			FLetterQuadLine[] quadInfoForText = font.GetQuadInfoForText(t, new FTextParams());
			for (int k = 0; k < quadInfoForText.Length; k++)
			{
				FLetterQuadLine fLetterQuadLine = quadInfoForText[k];
				list.AddRange(fLetterQuadLine.quads);
				list.Add(new FLetterQuad
				{
					charInfo = new FCharInfo()
				});
			}
			int index = 0;
			int num = -1;
			for (int l = 0; l < t.Length; l++)
			{
				float num2 = list[l].rect.x + list[l].rect.width - (list[index].rect.x + list[index].rect.width);
				char c = stringBuilder2[l];
				if (c == '\n')
				{
					_TrimWhitespace(ref l, t);
					num = -1;
					index = l;
				}
				else
				{
					if (num2 < 0.01f)
					{
						_TrimWhitespace(ref l, t);
					}
					else if (char.IsWhiteSpace(c))
					{
						num = l;
					}
					if (num2 > width)
					{
						if (num == -1)
						{
							stringBuilder2.Insert(l + 1, '\n');
						}
						else
						{
							stringBuilder2.Insert(num + 1, '\n');
							l = num;
						}
					}
				}
			}
			return stringBuilder2.ToString();
		}
	}

	internal static string GlobalTextModifier(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return text;
		}
		if (Custom.rainWorld.inGameTranslator.currentLanguage == InGameTranslator.LanguageID.Korean)
		{
			return _KoreanParticleCleaner(text);
		}
		return text;
	}

	internal static string _KoreanParticleCleaner(string orig)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(orig[0]);
		for (int i = 1; i < orig.Length; i++)
		{
			if (orig[i] != '(' || i > orig.Length - 4 || orig[i + 2] != ')')
			{
				stringBuilder.Append(orig[i]);
				continue;
			}
			string[] array = new string[2];
			switch (orig[i + 1])
			{
			case '와':
				if (orig[i + 3] != '과')
				{
					stringBuilder.Append(orig[i]);
					continue;
				}
				array[0] = "와";
				array[1] = "과";
				break;
			case '과':
				if (orig[i + 3] != '와')
				{
					stringBuilder.Append(orig[i]);
					continue;
				}
				array[0] = "와";
				array[1] = "과";
				break;
			case '을':
				if (orig[i + 3] != '를')
				{
					stringBuilder.Append(orig[i]);
					continue;
				}
				array[0] = "를";
				array[1] = "을";
				break;
			case '를':
				if (orig[i + 3] != '을')
				{
					stringBuilder.Append(orig[i]);
					continue;
				}
				array[0] = "를";
				array[1] = "을";
				break;
			case '은':
				if (orig[i + 3] != '는')
				{
					stringBuilder.Append(orig[i]);
					continue;
				}
				array[0] = "는";
				array[1] = "은";
				break;
			case '는':
				if (orig[i + 3] != '은')
				{
					stringBuilder.Append(orig[i]);
					continue;
				}
				array[0] = "는";
				array[1] = "은";
				break;
			case '아':
				if (orig[i + 3] != '야')
				{
					stringBuilder.Append(orig[i]);
					continue;
				}
				array[0] = "야";
				array[1] = "아야";
				break;
			case '으':
				if (orig[i + 3] != '로')
				{
					stringBuilder.Append(orig[i]);
					continue;
				}
				array[0] = "로";
				array[1] = "으로";
				break;
			case '이':
				if (orig[i + 3] == '가')
				{
					array[0] = "가";
					array[1] = "이";
				}
				else
				{
					array[0] = orig[i + 3].ToString() ?? "";
					array[1] = "이" + orig[i + 3];
				}
				break;
			case '가':
				if (orig[i + 3] != '이')
				{
					stringBuilder.Append(orig[i]);
					continue;
				}
				array[0] = "가";
				array[1] = "이";
				break;
			default:
				stringBuilder.Append(orig[i]);
				continue;
			}
			char c2 = orig[i - 1];
			if (char.IsWhiteSpace(c2) && i > 1)
			{
				c2 = orig[i - 2];
			}
			switch (VibeCheck(c2))
			{
			case 0:
				if (Regex.IsMatch(string.Empty + c2, "[013678]$"))
				{
					stringBuilder.Append(array[1]);
				}
				else
				{
					stringBuilder.Append(array[0]);
				}
				break;
			case 1:
				stringBuilder.Append(array[0]);
				break;
			case 2:
				stringBuilder.Append(array[1]);
				break;
			case 3:
				if (Regex.IsMatch(string.Empty + c2, "[AEIOUYaeiouy]$"))
				{
					stringBuilder.Append(array[0]);
				}
				else
				{
					stringBuilder.Append(array[1]);
				}
				break;
			default:
				stringBuilder.Append(orig[i]);
				break;
			}
			i += 3;
		}
		return stringBuilder.ToString();
		static byte VibeCheck(char c)
		{
			if (Regex.IsMatch(c.ToString(), "\\p{IsHangulSyllables}"))
			{
				if ((c - 44032) % 28 == 0)
				{
					return 1;
				}
				return 2;
			}
			if (Regex.IsMatch(c.ToString(), "^[0-9]*$"))
			{
				return 0;
			}
			if (Regex.IsMatch(c.ToString(), "^[A-Za-z]*$"))
			{
				return 3;
			}
			return 4;
		}
	}
}
