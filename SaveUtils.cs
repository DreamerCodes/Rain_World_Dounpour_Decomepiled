using System;
using System.Collections.Generic;
using System.Globalization;

public static class SaveUtils
{
	public static string SaveEnumIntDict<T>(Dictionary<T, int> validsDict, List<string> unrecognizedList)
	{
		string text = "";
		bool flag = true;
		foreach (KeyValuePair<T, int> item in validsDict)
		{
			text += string.Format(CultureInfo.InvariantCulture, flag ? "{0}:{1}" : ",{0}:{1}", Enum.GetName(typeof(T), item.Key), item.Value);
			flag = false;
		}
		for (int i = 0; i < unrecognizedList.Count; i++)
		{
			text += (flag ? unrecognizedList[i] : ("," + unrecognizedList[i]));
			flag = false;
		}
		return text;
	}

	public static void LoadEnumIntDict<T>(string commaSeparatedList, Dictionary<T, int> validsDict, List<string> unrecognizedList)
	{
		validsDict.Clear();
		unrecognizedList.Clear();
		string[] array = commaSeparatedList.Split(',');
		foreach (string text in array)
		{
			if (text.Contains(":"))
			{
				string[] array2 = text.Split(':');
				try
				{
					T key = (T)Enum.Parse(typeof(T), array2[0]);
					int value = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					validsDict[key] = value;
				}
				catch (ArgumentException)
				{
					unrecognizedList.Add(text);
				}
			}
			else
			{
				unrecognizedList.Add(text);
			}
		}
	}

	public static void LoadStringIntDict(string commaSeparatedList, Dictionary<string, int> populateDict)
	{
		populateDict.Clear();
		string[] array = commaSeparatedList.Split(',');
		foreach (string text in array)
		{
			if (text.Contains(":"))
			{
				string[] array2 = text.Split(':');
				int value = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				populateDict[array2[0]] = value;
			}
		}
	}

	public static string SaveEnumList<T>(List<T> validsList, List<string> unrecognizedList)
	{
		string text = "";
		for (int i = 0; i < validsList.Count; i++)
		{
			text += Enum.GetName(typeof(T), validsList[i]);
			if (i < validsList.Count - 1 || unrecognizedList.Count > 0)
			{
				text += ",";
			}
		}
		for (int j = 0; j < unrecognizedList.Count; j++)
		{
			text += unrecognizedList[j];
			if (j < unrecognizedList.Count - 1)
			{
				text += ",";
			}
		}
		return text;
	}

	public static void LoadEnumList<T>(string commaSeparatedList, List<T> validsList, List<string> unrecognizedList)
	{
		validsList.Clear();
		unrecognizedList.Clear();
		string[] array = commaSeparatedList.Split(',');
		foreach (string text in array)
		{
			try
			{
				T item = (T)Enum.Parse(typeof(T), text);
				validsList.Add(item);
			}
			catch (ArgumentException)
			{
				unrecognizedList.Add(text);
			}
		}
	}

	public static string SaveIntegerArray(char delimiter, int[] integers, int[] unrecognizedIntegers)
	{
		string text = "";
		bool flag = unrecognizedIntegers != null && unrecognizedIntegers.Length != 0;
		for (int i = 0; i < integers.Length; i++)
		{
			text += string.Format(CultureInfo.InvariantCulture, "{0}{1}", integers[i], (i < integers.Length - 1 || flag) ? delimiter.ToString() : "");
		}
		if (flag)
		{
			for (int j = 0; j < unrecognizedIntegers.Length; j++)
			{
				text += string.Format(CultureInfo.InvariantCulture, "{0}{1}", unrecognizedIntegers[j], (j < unrecognizedIntegers.Length - 1) ? delimiter.ToString() : "");
			}
		}
		return text;
	}

	public static int[] LoadIntegersArray(string saveString, char delimiter, int[] integers)
	{
		string[] array = saveString.Split(delimiter);
		int[] array2 = null;
		for (int i = 0; i < array.Length && i < integers.Length; i++)
		{
			integers[i] = int.Parse(array[i], NumberStyles.Any, CultureInfo.InvariantCulture);
		}
		if (array.Length > integers.Length)
		{
			array2 = new int[array.Length - integers.Length];
			for (int j = integers.Length; j < array.Length; j++)
			{
				array2[j - integers.Length] = int.Parse(array[j], NumberStyles.Any, CultureInfo.InvariantCulture);
			}
		}
		return array2;
	}

	public static string SaveBooleanArray(bool[] booleans, bool[] unrecognizedBooleans)
	{
		string text = "";
		for (int i = 0; i < booleans.Length; i++)
		{
			text += (booleans[i] ? "1" : "0");
		}
		if (unrecognizedBooleans != null)
		{
			for (int j = 0; j < unrecognizedBooleans.Length; j++)
			{
				text += (unrecognizedBooleans[j] ? "1" : "0");
			}
		}
		return text;
	}

	public static bool[] LoadBooleanArray(string saveString, bool[] booleans)
	{
		bool[] array = null;
		for (int i = 0; i < saveString.Length && i < booleans.Length; i++)
		{
			booleans[i] = saveString[i] == '1';
		}
		if (saveString.Length > booleans.Length)
		{
			array = new bool[saveString.Length - booleans.Length];
			for (int j = booleans.Length; j < saveString.Length; j++)
			{
				array[j - booleans.Length] = saveString[j] == '1';
			}
		}
		return array;
	}

	public static string AppendUnrecognizedStringAttrs(string normalSaveString, string delimiter, string[] unknownAttributes)
	{
		string text = normalSaveString;
		if (unknownAttributes != null && unknownAttributes.Length != 0)
		{
			text = text + delimiter + string.Join(delimiter, unknownAttributes);
		}
		return text;
	}

	public static string[] PopulateUnrecognizedStringAttrs(string[] normalStringSplit, int fromIndex)
	{
		List<string> list = new List<string>();
		for (int i = fromIndex; i < normalStringSplit.Length; i++)
		{
			if (normalStringSplit[i].Trim().Length != 0)
			{
				list.Add(normalStringSplit[i]);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list.ToArray();
	}
}
