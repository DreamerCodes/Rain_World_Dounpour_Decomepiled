using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using RWCustom;

public class LocalizationTranslator
{
	public class TranslationProcess : ExtEnum<TranslationProcess>
	{
		public static readonly TranslationProcess FindingCoordinates = new TranslationProcess("FindingCoordinates", register: true);

		public static readonly TranslationProcess FindingNoMatches = new TranslationProcess("FindingNoMatches", register: true);

		public static readonly TranslationProcess WritingToFiles = new TranslationProcess("WritingToFiles", register: true);

		public static readonly TranslationProcess WritingShortStringLookupTable = new TranslationProcess("WritingShortStringLookupTable", register: true);

		public static readonly TranslationProcess CheckMoon = new TranslationProcess("CheckMoon", register: true);

		public static readonly TranslationProcess Done = new TranslationProcess("Done", register: true);

		public TranslationProcess(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	private string[,] sheet;

	private string[][] engTextfiles;

	public int[][] sheetRowForLine;

	private string[] pauseData;

	private TranslationProcess currentProcess = TranslationProcess.FindingCoordinates;

	public int counter;

	public List<int> matchesNotFound;

	public int textFiles = 57;

	public int currentWritingLanguage;

	public static string LangShort(InGameTranslator.LanguageID ID)
	{
		return ID.ToString().Substring(0, 3);
	}

	public LocalizationTranslator()
	{
		Custom.Log("TRANSLATING");
		string[] array = File.ReadAllLines(Custom.RootFolderDirectory() + (Path.DirectorySeparatorChar + "Text processing" + Path.DirectorySeparatorChar + "Rain World Localisation October 2017 TRANSLATED.csv").ToLowerInvariant());
		sheet = new string[array[0].Split('|').Length, array.Length];
		for (int i = 0; i < sheet.GetLength(1); i++)
		{
			string[] array2 = array[i].Split('|');
			for (int j = 0; j < sheet.GetLength(0) && j < array2.Length; j++)
			{
				sheet[j, i] = FormatString(array2[j]);
			}
		}
		pauseData = new string[array.Length];
		engTextfiles = new string[textFiles][];
		sheetRowForLine = new int[textFiles][];
		for (int k = 1; k <= textFiles; k++)
		{
			engTextfiles[k - 1] = File.ReadAllLines(Custom.RootFolderDirectory() + (Path.DirectorySeparatorChar + "Text processing" + Path.DirectorySeparatorChar + "Text_Eng" + Path.DirectorySeparatorChar + k + ".txt").ToLowerInvariant(), Encoding.Default);
			sheetRowForLine[k - 1] = new int[engTextfiles[k - 1].Length];
			for (int l = 0; l < engTextfiles[k - 1].Length; l++)
			{
				engTextfiles[k - 1][l] = FormatString(engTextfiles[k - 1][l]);
				sheetRowForLine[k - 1][l] = -1;
			}
		}
		matchesNotFound = new List<int>();
		InitNewProcess(TranslationProcess.FindingCoordinates);
	}

	private string FormatString(string s)
	{
		s = s.Replace("’", "'");
		s = s.Replace("${PlayerName}", "<PlayerName>");
		s = s.Replace("${CapPlayerName}", "<CapPlayerName>");
		s = s.Replace("…", "...");
		return s;
	}

	private bool StringWorthyToConsider(string s)
	{
		if (s == null)
		{
			return false;
		}
		if (s.Length == 0)
		{
			return false;
		}
		if (s == " ")
		{
			return false;
		}
		return true;
	}

	private bool SheetCoordinateValid(InGameTranslator.LanguageID language, int y)
	{
		int num = (int)language + 1;
		y--;
		if (num < 0 || num >= sheet.GetLength(0) || y < 0 || y >= sheet.GetLength(1))
		{
			return false;
		}
		return true;
	}

	private string SampleCell(InGameTranslator.LanguageID language, int y)
	{
		int num = (int)language + 1;
		y--;
		if (num < 0 || num >= sheet.GetLength(0) || y < 0 || y >= sheet.GetLength(1))
		{
			return "";
		}
		return sheet[num, y];
	}

	private string GetPauseData(int y)
	{
		y--;
		if (y < 0 || y >= sheet.GetLength(1) || pauseData[y] == null)
		{
			return "";
		}
		return pauseData[y];
	}

	public void Update()
	{
		if (currentProcess == TranslationProcess.FindingCoordinates)
		{
			FindingCoordinates();
		}
		else if (currentProcess == TranslationProcess.FindingNoMatches)
		{
			FindingNoMatches();
		}
		else if (currentProcess == TranslationProcess.WritingToFiles)
		{
			WritingToFiles();
		}
		else if (currentProcess == TranslationProcess.WritingShortStringLookupTable)
		{
			WriteShortStringLookupTable();
		}
		else if (currentProcess == TranslationProcess.CheckMoon)
		{
			CheckMoon();
		}
	}

	private void InitNewProcess(TranslationProcess newProcess)
	{
		Custom.Log($"new process: {newProcess}");
		if (newProcess == TranslationProcess.FindingCoordinates)
		{
			counter = 1;
		}
		else if (newProcess == TranslationProcess.FindingNoMatches)
		{
			counter = 0;
		}
		else if (newProcess == TranslationProcess.WritingToFiles)
		{
			counter = 1;
			currentWritingLanguage = 1;
		}
		else if (newProcess == TranslationProcess.Done)
		{
			Custom.Log("DONE");
		}
		currentProcess = newProcess;
	}

	private void FindingCoordinates()
	{
		if (!SheetCoordinateValid(InGameTranslator.LanguageID.English, counter))
		{
			InitNewProcess(TranslationProcess.FindingNoMatches);
			return;
		}
		string text = SampleCell(InGameTranslator.LanguageID.English, counter);
		if (StringWorthyToConsider(text))
		{
			Custom.Log($"row : {counter} string: {text}");
			bool flag = false;
			for (int i = 0; i < textFiles; i++)
			{
				for (int j = 0; j < engTextfiles[i].Length; j++)
				{
					if (sheetRowForLine[i][j] == -1 && StringWorthyToConsider(engTextfiles[i][j]))
					{
						if (engTextfiles[i][j] == text || engTextfiles[i][j] == text + " " || engTextfiles[i][j] + " " == text)
						{
							sheetRowForLine[i][j] = counter;
							Custom.Log($"Match found for {counter}({i + 1},{j})");
							flag = true;
							break;
						}
						if (WashPauseDataFromConversationLine(engTextfiles[i][j]) == text)
						{
							sheetRowForLine[i][j] = counter;
							pauseData[counter - 1] = ExtractPauseDataFromConversationLine(engTextfiles[i][j]);
							Custom.Log($"Pause data found on sheet row : {counter} {pauseData[counter - 1]}");
							flag = true;
							break;
						}
					}
				}
				if (flag)
				{
					break;
				}
			}
			if (!flag)
			{
				Custom.LogWarning("no match found for:", counter.ToString(), ":", text);
				matchesNotFound.Add(counter);
			}
		}
		counter++;
	}

	public static string[] ConsolidateLineInstructions(string s)
	{
		string[] array = Regex.Split(s, " : ");
		if (array.Length <= 1)
		{
			return array;
		}
		List<string> list = new List<string>();
		bool flag = false;
		bool flag2 = false;
		string text = "";
		for (int i = 0; i < array.Length; i++)
		{
			int result;
			if (i == 0)
			{
				if (array[i] == "PEBBLESWAIT" || array[i] == "SPECEVENT")
				{
					list.Add(array[i]);
				}
				else if (int.TryParse(array[i], NumberStyles.Any, CultureInfo.InvariantCulture, out result))
				{
					list.Add(array[i]);
					flag = true;
				}
				else
				{
					text += array[i];
				}
			}
			else if (i != array.Length - 1)
			{
				if (!flag || flag2 || !int.TryParse(array[i], NumberStyles.Any, CultureInfo.InvariantCulture, out result))
				{
					text = ((!(text == "")) ? (text + " : " + array[i]) : (text + array[i]));
					continue;
				}
				list.Add(array[i]);
				flag2 = true;
			}
			else if (flag && !flag2 && int.TryParse(array[i], NumberStyles.Any, CultureInfo.InvariantCulture, out result))
			{
				list.Add(text);
				list.Add(array[i]);
				flag2 = true;
			}
			else
			{
				text = ((!(text == "")) ? (text + " : " + array[i]) : (text + array[i]));
				list.Add(text);
			}
		}
		return list.ToArray();
	}

	private bool IsSpecialInstruction(string s)
	{
		string[] array = ConsolidateLineInstructions(s);
		if (array.Length < 2)
		{
			return false;
		}
		if (!(array[0] == "PEBBLESWAIT"))
		{
			return array[0] == "SPECEVENT";
		}
		return true;
	}

	private string WashPauseDataFromConversationLine(string s)
	{
		string[] array = ConsolidateLineInstructions(s);
		if (array.Length == 3)
		{
			return array[2];
		}
		return s;
	}

	private string ExtractPauseDataFromConversationLine(string s)
	{
		string[] array = ConsolidateLineInstructions(s);
		if (array.Length == 3)
		{
			return array[0] + " : " + array[1] + " : ";
		}
		return "";
	}

	private void FindingNoMatches()
	{
		if (counter >= matchesNotFound.Count)
		{
			InitNewProcess(TranslationProcess.WritingToFiles);
			return;
		}
		string text = SampleCell(InGameTranslator.LanguageID.English, matchesNotFound[counter]);
		int num = int.MaxValue;
		IntVector2 intVector = new IntVector2(-1, -1);
		if (StringWorthyToConsider(text))
		{
			for (int i = 0; i < textFiles; i++)
			{
				for (int j = 0; j < engTextfiles[i].Length; j++)
				{
					if (StringWorthyToConsider(engTextfiles[i][j]))
					{
						int num2 = LevenshteinDistance(text, engTextfiles[i][j]);
						if (num2 < num)
						{
							num = num2;
							intVector = new IntVector2(i, j);
						}
					}
				}
			}
		}
		if (intVector.x < 0)
		{
			Custom.LogWarning("no approx. match found for:", text);
		}
		else if (num < text.Length / 2 || num < engTextfiles[intVector.x][intVector.y].Length / 2)
		{
			Custom.LogImportant("---POSSIBLE MATCH");
			Custom.LogImportant("Best match for:", text);
			Custom.LogImportant("is:", engTextfiles[intVector.x][intVector.y]);
			Custom.LogImportant("Are equal:", (text == engTextfiles[intVector.x][intVector.y]).ToString());
			Custom.LogImportant("Excel doc row:", matchesNotFound[counter].ToString());
			Custom.LogImportant("File:", (intVector.x + 1).ToString());
			Custom.LogImportant("Line:", intVector.y.ToString());
		}
		counter++;
	}

	private void WritingToFiles()
	{
		if (counter > textFiles)
		{
			counter = 1;
			currentWritingLanguage++;
			if (currentWritingLanguage >= ExtEnum<InGameTranslator.LanguageID>.values.Count)
			{
				InitNewProcess(TranslationProcess.WritingShortStringLookupTable);
				return;
			}
		}
		InGameTranslator.LanguageID languageID = InGameTranslator.LanguageID.Parse(currentWritingLanguage);
		Custom.Log($"writing file {counter} {languageID}");
		using (StreamWriter streamWriter = new StreamWriter(File.Open(Custom.RootFolderDirectory() + (Path.DirectorySeparatorChar + "Text processing" + Path.DirectorySeparatorChar + "Text_" + LangShort(languageID) + Path.DirectorySeparatorChar + counter + ".txt").ToLowerInvariant(), FileMode.Create), Encoding.Default))
		{
			for (int i = 0; i < engTextfiles[counter - 1].Length; i++)
			{
				if (i == 0 || !StringWorthyToConsider(engTextfiles[counter - 1][i]) || IsSpecialInstruction(engTextfiles[counter - 1][i]))
				{
					streamWriter.WriteLine(engTextfiles[counter - 1][i]);
					continue;
				}
				if (sheetRowForLine[counter - 1][i] > -1)
				{
					streamWriter.WriteLine(GetPauseData(sheetRowForLine[counter - 1][i]) + SampleCell(languageID, sheetRowForLine[counter - 1][i]));
					continue;
				}
				Custom.LogWarning("missing translation file:", counter.ToString(), "line:", (i + 1).ToString());
				streamWriter.WriteLine("MSSNG TRNSL");
			}
		}
		counter++;
	}

	public void WriteShortStringLookupTable()
	{
		int count = ExtEnum<InGameTranslator.LanguageID>.values.Count;
		List<int> list = new List<int>();
		Dictionary<int, List<int>> dictionary = new Dictionary<int, List<int>>();
		for (int i = 0; i < matchesNotFound.Count; i++)
		{
			string text = SampleCell(InGameTranslator.LanguageID.English, matchesNotFound[i]);
			if (dictionary.ContainsKey(text.Length))
			{
				dictionary[text.Length].Add(matchesNotFound[i]);
				continue;
			}
			list.Add(text.Length);
			dictionary.Add(text.Length, new List<int> { matchesNotFound[i] });
		}
		for (int j = 0; j < list.Count; j++)
		{
			int key = list[j];
			using StreamWriter streamWriter = new StreamWriter(File.Open(Custom.RootFolderDirectory() + (Path.DirectorySeparatorChar + "Text processing" + Path.DirectorySeparatorChar + "Short_Strings" + Path.DirectorySeparatorChar + key + ".txt").ToLowerInvariant(), FileMode.Create), Encoding.Default);
			streamWriter.Write("0");
			for (int k = 0; k < dictionary[key].Count; k++)
			{
				string text2 = "";
				for (int l = 0; l < count; l++)
				{
					text2 = text2 + SampleCell(InGameTranslator.LanguageID.Parse(l), dictionary[key][k]) + ((l < count - 1) ? "|" : "");
				}
				streamWriter.WriteLine(text2);
			}
		}
		InitNewProcess(TranslationProcess.CheckMoon);
	}

	private void CheckMoon()
	{
		Custom.Log("Checking moon");
		string[] array = File.ReadAllLines(Custom.RootFolderDirectory() + (Path.DirectorySeparatorChar + "Text processing" + Path.DirectorySeparatorChar + "moonscode.txt").ToLowerInvariant());
		List<string> list = new List<string>();
		string text = null;
		bool flag = false;
		for (int i = 0; i < array.Length; i++)
		{
			for (int j = 0; j < array[i].Length; j++)
			{
				if (j < array[i].Length - 1 && array[i][j] == '/' && array[i][j + 1] == '/')
				{
					flag = true;
				}
				if (array[i][j] == '"')
				{
					if (text == null)
					{
						text = "";
						continue;
					}
					if (StringWorthyToConsider(text))
					{
						list.Add(text);
					}
					text = null;
				}
				else if (text != null && !flag)
				{
					text += array[i][j];
				}
			}
			flag = false;
		}
		List<string> list2 = new List<string>();
		List<string> list3 = new List<string>();
		List<string> list4 = new List<string>();
		for (int k = 0; k < list.Count; k++)
		{
			string text2 = list[k];
			bool flag2 = false;
			for (int l = 0; l < sheet.GetLength(1); l++)
			{
				if (text2 == sheet[1, l])
				{
					flag2 = true;
					break;
				}
			}
			if (flag2)
			{
				list2.Add(text2);
				continue;
			}
			list3.Add(text2);
			int num = int.MaxValue;
			int num2 = -1;
			for (int m = 0; m < sheet.GetLength(1); m++)
			{
				if (StringWorthyToConsider(sheet[1, m]))
				{
					int num3 = LevenshteinDistance(text2, sheet[1, m]);
					if (num3 < num)
					{
						num = num3;
						num2 = m;
					}
				}
			}
			list4.Add((num2 > -1) ? sheet[1, num2] : "NONE");
		}
		using (StreamWriter streamWriter = new StreamWriter(File.Open(Custom.RootFolderDirectory() + (Path.DirectorySeparatorChar + "Text processing" + Path.DirectorySeparatorChar + "moonsCodeResult.txt").ToLowerInvariant(), FileMode.Create), Encoding.Default))
		{
			streamWriter.WriteLine("--Lines that have translation: ");
			for (int n = 0; n < list2.Count; n++)
			{
				streamWriter.WriteLine("(X)   " + list2[n]);
				streamWriter.WriteLine(" ");
			}
			streamWriter.WriteLine("--Lines that LACK translation: ");
			for (int num4 = 0; num4 < list3.Count; num4++)
			{
				streamWriter.WriteLine("LN: " + list3[num4]);
				streamWriter.WriteLine("BC: " + list4[num4]);
				if (list3[num4].Length != list4[num4].Length)
				{
					streamWriter.WriteLine("DIFF LENGHT");
				}
				else
				{
					string text3 = "";
					for (int num5 = 0; num5 < list3[num4].Length; num5++)
					{
						if (list3[num4][num5] != list4[num4][num5])
						{
							text3 = text3 + num5 + ", ";
						}
					}
					streamWriter.WriteLine("DIFFS: " + text3);
				}
				streamWriter.WriteLine(" ");
			}
		}
		InitNewProcess(TranslationProcess.Done);
	}

	private void LogRow(int y)
	{
		for (int i = 0; i < ExtEnum<InGameTranslator.LanguageID>.values.Count; i++)
		{
			LogCell(InGameTranslator.LanguageID.Parse(i), y);
		}
	}

	private void LogCell(InGameTranslator.LanguageID language, int y)
	{
		Custom.Log(LangShort(language), y.ToString(), ":", SampleCell(language, y));
	}

	public static int LevenshteinDistance(string s, string t)
	{
		int length = s.Length;
		int length2 = t.Length;
		int[,] array = new int[length + 1, length2 + 1];
		if (length == 0)
		{
			return length2;
		}
		if (length2 == 0)
		{
			return length;
		}
		int num = 0;
		while (num <= length)
		{
			array[num, 0] = num++;
		}
		int num2 = 0;
		while (num2 <= length2)
		{
			array[0, num2] = num2++;
		}
		for (int i = 1; i <= length; i++)
		{
			for (int j = 1; j <= length2; j++)
			{
				int num3 = ((t[j - 1] != s[i - 1]) ? 1 : 0);
				array[i, j] = Math.Min(Math.Min(array[i - 1, j] + 1, array[i, j - 1] + 1), array[i - 1, j - 1] + num3);
			}
		}
		return array[length, length2];
	}
}
