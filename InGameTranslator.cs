using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Kittehface.Build;
using Kittehface.Framework20;
using Menu;
using Menu.Remix.MixedUI;
using RWCustom;
using UnityEngine;

public class InGameTranslator
{
	public class LanguageID : ExtEnum<LanguageID>
	{
		public static readonly LanguageID English = new LanguageID("English", register: true);

		public static readonly LanguageID French = new LanguageID("French", register: true);

		public static readonly LanguageID Italian = new LanguageID("Italian", register: true);

		public static readonly LanguageID German = new LanguageID("German", register: true);

		public static readonly LanguageID Spanish = new LanguageID("Spanish", register: true);

		public static readonly LanguageID Portuguese = new LanguageID("Portuguese", register: true);

		public static readonly LanguageID Japanese = new LanguageID("Japanese", register: true);

		public static readonly LanguageID Korean = new LanguageID("Korean", register: true);

		public static readonly LanguageID Russian = new LanguageID("Russian", register: true);

		public static readonly LanguageID Chinese = new LanguageID("Chinese", register: true);

		public LanguageID(string value, bool register = false)
			: base(value, register)
		{
		}

		public static LanguageID Parse(int index)
		{
			if (index < 0 || index >= ExtEnum<LanguageID>.values.entries.Count)
			{
				return null;
			}
			return new LanguageID(ExtEnum<LanguageID>.values.entries[index]);
		}

		public static int EncryptIndex(LanguageID lang)
		{
			if (lang == English)
			{
				return 0;
			}
			if (lang == French)
			{
				return 1;
			}
			if (lang == Italian)
			{
				return 2;
			}
			if (lang == German)
			{
				return 3;
			}
			if (lang == Spanish)
			{
				return 4;
			}
			if (lang == Portuguese)
			{
				return 5;
			}
			if (lang == Japanese)
			{
				return 6;
			}
			if (lang == Korean)
			{
				return 7;
			}
			if (lang == Russian)
			{
				return 8;
			}
			if (lang == Chinese)
			{
				return 9;
			}
			return 0;
		}

		public static bool UsesCapitals(LanguageID lang)
		{
			if (lang != Japanese && lang != Korean)
			{
				return lang != Chinese;
			}
			return false;
		}

		public static bool UsesLargeFont(LanguageID lang)
		{
			if (!(lang == Japanese) && !(lang == Korean))
			{
				return lang == Chinese;
			}
			return true;
		}

		public static bool UsesSpaces(LanguageID lang)
		{
			if (!(lang == Japanese))
			{
				return !(lang == Chinese);
			}
			return false;
		}

		public static bool WordWrappingAllowed(LanguageID lang)
		{
			return lang != Japanese;
		}
	}

	private RainWorld rainWorld;

	private LanguageID lastLanguage;

	public bool loadedAOC;

	public Dictionary<string, string> shortStrings;

	public LanguageID currentLanguage => rainWorld.options.language;

	public static LanguageID systemLanguage
	{
		get
		{
			switch (Platform.systemLanguage)
			{
			case Language.French:
			case Language.FrenchCanadian:
				return LanguageID.French;
			case Language.Italian:
				return LanguageID.Italian;
			case Language.German:
				return LanguageID.German;
			case Language.Spanish:
			case Language.SpanishLatinAmerican:
				return LanguageID.Spanish;
			case Language.Portuguese:
			case Language.BrazilianPortuguese:
				return LanguageID.Portuguese;
			case Language.Japanese:
				return LanguageID.Japanese;
			case Language.Korean:
				return LanguageID.Korean;
			case Language.Chinese:
			case Language.ChineseSimplified:
			case Language.ChineseTraditional:
				return LanguageID.Chinese;
			case Language.Belarusian:
			case Language.Estonian:
			case Language.Latvian:
			case Language.Lithuanian:
			case Language.Russian:
			case Language.Ukrainian:
				return LanguageID.Russian;
			default:
				return LanguageID.English;
			}
		}
	}

	public InGameTranslator(RainWorld rainWorld)
	{
		this.rainWorld = rainWorld;
		shortStrings = new Dictionary<string, string>();
	}

	public static void LoadFonts(LanguageID lang, global::Menu.Menu menu)
	{
		string text = LocalizationTranslator.LangShort(lang);
		if (ModManager.NonPrepackagedModsInstalled && ModManager.InitializationScreenFinished && (lang == LanguageID.Japanese || lang == LanguageID.Korean || lang == LanguageID.Chinese))
		{
			if (!Futile.atlasManager.DoesContainAtlas("Atlases/fontAtlas" + text + "Full"))
			{
				Futile.atlasManager.LoadAtlasFromAddress("Atlases/fontAtlas" + text + "Full", "Assets/FullFontAtlases/fontAtlas" + text + "Full.png");
			}
			if (!Futile.atlasManager.DoesContainFontWithName("font" + text + "Full"))
			{
				Futile.atlasManager.LoadFont("font" + text + "Full", "font" + text + "Full", "Atlases/font" + text + "Full", 0f, 4f, new FTextParams(), 0.5f);
				Futile.atlasManager.CombineFonts("font" + text + "Full", "fontSolo", "fontSolo", "Atlases/font", 4f, new FTextParams(), 1f);
				Futile.atlasManager.CombineFonts("font" + text + "Full", "ps4GlyphsAtlas", "ps4GlyphsAtlas", "Atlases/ps4Glyphs", 0f, new FTextParams(), 0.5f);
			}
			if (!Futile.atlasManager.DoesContainAtlas("Atlases/displayFontAtlas" + text + "Full"))
			{
				Futile.atlasManager.LoadAtlasFromAddress("Atlases/displayFontAtlas" + text + "Full", "Assets/FullFontAtlases/displayFontAtlas" + text + "Full.png");
			}
			if (!Futile.atlasManager.DoesContainFontWithName("DisplayFont" + text + "Full"))
			{
				Futile.atlasManager.LoadFont("DisplayFont" + text + "Full", "DisplayFont" + text + "Full", "Atlases/DisplayFont" + text + "Full", 0f, 0f, new FTextParams(), 0.5f);
				Futile.atlasManager.CombineFonts("DisplayFont" + text + "Full", "DisplayFontSolo", "DisplayFontSolo", "Atlases/DisplayFont", 0f, new FTextParams(), 1f);
			}
		}
		else if (lang == LanguageID.Japanese || lang == LanguageID.Korean || lang == LanguageID.Chinese)
		{
			if (!Futile.atlasManager.DoesContainAtlas("Atlases/fontAtlas" + text))
			{
				Futile.atlasManager.LoadAtlas("Atlases/fontAtlas" + text);
			}
			if (!Futile.atlasManager.DoesContainFontWithName("font" + text))
			{
				Futile.atlasManager.LoadFont("font" + text, "font" + text, "Atlases/font" + text, 0f, -1f, new FTextParams(), 0.5f);
				Futile.atlasManager.CombineFonts("font" + text, "fontSolo", "fontSolo", "Atlases/font", -1f, new FTextParams(), 1f);
				Futile.atlasManager.CombineFonts("font" + text, "ps4GlyphsAtlas", "ps4GlyphsAtlas", "Atlases/ps4Glyphs", 0f, new FTextParams(), 0.5f);
			}
			if (!Futile.atlasManager.DoesContainAtlas("Atlases/displayFontAtlas" + text))
			{
				Futile.atlasManager.LoadAtlas("Atlases/displayFontAtlas" + text);
			}
			if (!Futile.atlasManager.DoesContainFontWithName("DisplayFont" + text))
			{
				Futile.atlasManager.LoadFont("DisplayFont" + text, "DisplayFont" + text, "Atlases/DisplayFont" + text, 0f, -4f, new FTextParams(), 0.5f);
				Futile.atlasManager.CombineFonts("DisplayFont" + text, "DisplayFontSolo", "DisplayFontSolo", "Atlases/DisplayFont", -4f, new FTextParams(), 1f);
			}
		}
		else
		{
			string text2 = ((lang == LanguageID.Russian) ? text : "");
			if (!Futile.atlasManager.DoesContainAtlas("Atlases/fontAtlas" + text2))
			{
				Futile.atlasManager.LoadAtlas("Atlases/fontAtlas" + text2);
			}
			if (lang == LanguageID.Russian)
			{
				Futile.atlasManager.LoadAtlas("Atlases/displayFontAtlas" + text2);
			}
			if (!Futile.atlasManager.DoesContainFontWithName("font" + text2))
			{
				Futile.atlasManager.LoadFont("font" + text2, "font" + text2, "Atlases/font" + text2, 0f, 0f);
				Futile.atlasManager.CombineFonts("font" + text2, (text2 == "") ? "ps4Glyphs" : "ps4GlyphsAtlas", (text2 == "") ? "ps4Glyphs" : "ps4GlyphsAtlas", "Atlases/ps4Glyphs", 0f, new FTextParams(), 0.5f);
			}
			if (!Futile.atlasManager.DoesContainFontWithName("DisplayFont" + text2))
			{
				Futile.atlasManager.LoadFont("DisplayFont" + text2, "DisplayFont" + text2, "Atlases/DisplayFont" + text2, 0f, 0f);
			}
		}
		if (menu != null)
		{
			LabelTest.Initialize(menu);
		}
	}

	public static void UnloadFonts(LanguageID lang)
	{
		string text = ((lang == LanguageID.Japanese || lang == LanguageID.Korean || lang == LanguageID.Chinese || lang == LanguageID.Russian) ? LocalizationTranslator.LangShort(lang) : "");
		Futile.atlasManager.UnloadFont("font" + text);
		Futile.atlasManager.UnloadFont("DisplayFont" + text);
		if (Futile.atlasManager.DoesContainAtlas("Atlases/fontAtlas" + text))
		{
			Futile.atlasManager.UnloadAtlas("Atlases/fontAtlas" + text);
		}
		if (Futile.atlasManager.DoesContainAtlas("Atlases/displayFontAtlas" + text))
		{
			Futile.atlasManager.UnloadAtlas("Atlases/displayFontAtlas" + text);
		}
		if (ModManager.NonPrepackagedModsInstalled && ModManager.InitializationScreenFinished && (lang == LanguageID.Japanese || lang == LanguageID.Korean || lang == LanguageID.Chinese))
		{
			text += "Full";
			Futile.atlasManager.UnloadFont("font" + text);
			Futile.atlasManager.UnloadFont("DisplayFont" + text);
			if (Futile.atlasManager.DoesContainAtlas("Atlases/fontAtlas" + text))
			{
				Futile.atlasManager.UnloadAtlas("Atlases/fontAtlas" + text);
			}
			if (Futile.atlasManager.DoesContainAtlas("Atlases/displayFontAtlas" + text))
			{
				Futile.atlasManager.UnloadAtlas("Atlases/displayFontAtlas" + text);
			}
		}
	}

	public string Translate(string s)
	{
		if (lastLanguage == null || lastLanguage != currentLanguage)
		{
			shortStrings.Clear();
		}
		lastLanguage = currentLanguage;
		if (!loadedAOC && ModManager.ActiveMods.Count > 0)
		{
			shortStrings.Clear();
			LoadShortStrings();
			loadedAOC = true;
		}
		if (shortStrings.Count == 0)
		{
			LoadShortStrings();
		}
		if (shortStrings.ContainsKey(s))
		{
			return shortStrings[s];
		}
		if (rainWorld.buildType == RainWorld.BuildType.Distribution)
		{
			return s;
		}
		if (!Utilities.isDebugBuild)
		{
			return s;
		}
		Custom.LogWarning("UNABLE TO TRANSLATE:", s, "(", s.Length.ToString(), "chars)");
		if (s.Length > 0 && s[0] == '!')
		{
			return "DOUBLE TRANSLATION";
		}
		return "!NO TRANSLATION!";
	}

	public bool HasShortstringTranslation(string s)
	{
		if (lastLanguage == null || lastLanguage != currentLanguage)
		{
			shortStrings.Clear();
		}
		lastLanguage = currentLanguage;
		if (shortStrings.Count == 0)
		{
			LoadShortStrings();
		}
		if (shortStrings.ContainsKey(s))
		{
			return true;
		}
		return false;
	}

	public bool TryTranslate(string text, out string res)
	{
		res = Translate(text);
		if (string.IsNullOrEmpty(res) || res == "!NO TRANSLATION!")
		{
			return false;
		}
		return res != text;
	}

	public static string EvenSplit(string s, int splits)
	{
		if (splits < 1)
		{
			return s;
		}
		float num = (float)s.Length / (float)(1 + splits);
		List<int> list = new List<int>();
		for (int i = 0; i < s.Length; i++)
		{
			if (s[i] == ' ')
			{
				list.Add(i);
			}
		}
		List<int> list2 = new List<int>();
		for (int j = 0; j < splits; j++)
		{
			float num2 = num * (float)(1 + j);
			float num3 = float.MaxValue;
			int num4 = -1;
			for (int k = 0; k < list.Count; k++)
			{
				float num5 = Mathf.Abs(num2 - (float)list[k]);
				if (num5 < num3)
				{
					num3 = num5;
					num4 = list[k];
				}
			}
			if (num4 > -1 && !list2.Contains(num4))
			{
				list2.Add(num4);
			}
		}
		if (list2.Count < 1)
		{
			return s;
		}
		int num6 = 0;
		foreach (int item in list2)
		{
			int length = s.Length;
			int num7 = item + num6;
			s = s.Substring(0, num7) + "\r\n" + s.Substring(num7 + 1, s.Length - (num7 + 1));
			num6 += s.Length - length;
		}
		return s;
	}

	internal void LoadShortStrings()
	{
		string[] array = ShortStringsFilePaths(LanguageID.English);
		if (!File.Exists(array[0]))
		{
			return;
		}
		LoadShortStrings(array);
		if (!(currentLanguage == LanguageID.English))
		{
			array = ShortStringsFilePaths(currentLanguage);
			if (File.Exists(array[0]))
			{
				LoadShortStrings(array);
			}
		}
		void LoadShortStrings(string[] paths)
		{
			for (int i = 0; i < paths.Length; i++)
			{
				string text = File.ReadAllText(paths[i], Encoding.UTF8);
				if (text[0] == '1')
				{
					text = Custom.xorEncrypt(text, 12467);
				}
				else if (text[0] == '0')
				{
					text = text.Remove(0, 1);
				}
				string[] array2 = Regex.Split(text, "\r\n");
				for (int j = 0; j < array2.Length; j++)
				{
					if (array2[j].Contains("///"))
					{
						array2[j] = array2[j].Split('/')[0].TrimEnd();
					}
					string[] array3 = array2[j].Split('|');
					if (array3.Length >= 2 && !string.IsNullOrEmpty(array3[1]))
					{
						shortStrings[array3[0]] = array3[1];
					}
				}
			}
		}
		static string[] ShortStringsFilePaths(LanguageID id)
		{
			List<string> list = new List<string> { Custom.RootFolderDirectory() + (Path.DirectorySeparatorChar + "Text" + Path.DirectorySeparatorChar + "Text_" + LocalizationTranslator.LangShort(id) + Path.DirectorySeparatorChar + "strings.txt").ToLowerInvariant() };
			for (int k = 0; k < ModManager.ActiveMods.Count; k++)
			{
				string text2 = (ModManager.ActiveMods[k].path + Path.DirectorySeparatorChar + "Text" + Path.DirectorySeparatorChar + "Text_" + LocalizationTranslator.LangShort(id) + Path.DirectorySeparatorChar + "strings.txt").ToLowerInvariant();
				if (File.Exists(text2))
				{
					list.Add(text2);
				}
			}
			return list.ToArray();
		}
	}

	public string SpecificTextFolderDirectory()
	{
		return "Text" + Path.DirectorySeparatorChar + "Text_" + LocalizationTranslator.LangShort(currentLanguage);
	}

	public string SpecificTextFolderDirectory(LanguageID targetLanguage)
	{
		return "Text" + Path.DirectorySeparatorChar + "Text_" + LocalizationTranslator.LangShort(targetLanguage);
	}

	public static string EncryptDecryptFile(string path, bool encryptMode, bool returnOnly = false)
	{
		if (path.ToLowerInvariant().Contains("strings.txt"))
		{
			return File.ReadAllText(path, Encoding.UTF8);
		}
		string text = File.ReadAllText(path, Encoding.UTF8);
		if (text[0] == (encryptMode ? 48 : 49))
		{
			string text2 = Path.GetDirectoryName(path).ToLowerInvariant();
			LanguageID languageID = null;
			if (text2.Contains("text_"))
			{
				string value = text2.Substring(text2.LastIndexOf("text_") + 5, 3);
				for (int i = 0; i < ExtEnum<LanguageID>.values.Count; i++)
				{
					if (ExtEnum<LanguageID>.values.entries[i].ToLowerInvariant().StartsWith(value))
					{
						languageID = new LanguageID(ExtEnum<LanguageID>.values.entries[i]);
						break;
					}
				}
			}
			string text3 = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
			string s = text3;
			if (text3.Contains("-"))
			{
				s = text3.Substring(0, text3.IndexOf("-"));
			}
			if (int.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
			{
				string text4 = null;
				if (path.ToLowerInvariant().Contains("strings.txt"))
				{
					text4 = Custom.xorEncrypt(text, 12467 - result);
				}
				else if (languageID != null)
				{
					text4 = Custom.xorEncrypt(text, 54 + result + LanguageID.EncryptIndex(languageID) * 7);
				}
				if (text4 != null)
				{
					text4 = (encryptMode ? '1' : '0') + text4.Remove(0, 1);
					if (!returnOnly)
					{
						File.WriteAllText(path, text4, Encoding.UTF8);
					}
					return text4;
				}
			}
			else if (languageID != null)
			{
				int num = 0;
				for (int j = 0; j < text3.Length; j++)
				{
					num += text3[j] - 48;
				}
				string text5 = null;
				text5 = string.Concat(str1: ((!path.ToLowerInvariant().Contains("strings.txt")) ? Custom.xorEncrypt(text, 54 + num + LanguageID.EncryptIndex(languageID) * 7) : Custom.xorEncrypt(text, 12467)).Remove(0, 1), str0: (encryptMode ? '1' : '0').ToString());
				if (!returnOnly)
				{
					File.WriteAllText(path, text5, Encoding.UTF8);
				}
				return text5;
			}
		}
		return null;
	}
}
