using System;
using System.Collections.Generic;
using System.Text;

public static class RXListExtensions
{
	public static void Log<T>(this List<T> list)
	{
		list.Log("");
	}

	public static void Log<T>(this List<T> list, string name)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (name != "")
		{
			stringBuilder.Append(name);
			stringBuilder.Append(": ");
		}
		stringBuilder.Append('[');
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			stringBuilder.Append(list[i].ToString());
			if (i < count - 1)
			{
				stringBuilder.Append(',');
			}
		}
		stringBuilder.Append(']');
	}

	public static T Unshift<T>(this List<T> list)
	{
		T result = list[0];
		list.RemoveAt(0);
		return result;
	}

	public static T Pop<T>(this List<T> list)
	{
		T result = list[list.Count - 1];
		list.RemoveAt(list.Count - 1);
		return result;
	}

	public static T GetLastObject<T>(this List<T> list)
	{
		return list[list.Count - 1];
	}

	public static void InsertionSort<T>(this List<T> list, Comparison<T> comparison)
	{
		int count = list.Count;
		for (int i = 1; i < count; i++)
		{
			T val = list[i];
			int num = i - 1;
			while (num >= 0 && comparison(list[num], val) > 0)
			{
				list[num + 1] = list[num];
				num--;
			}
			list[num + 1] = val;
		}
	}
}
