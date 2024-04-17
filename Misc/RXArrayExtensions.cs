using System.Text;

public static class RXArrayExtensions
{
	public static int IndexOf<T>(this T[] items, T itemToFind) where T : class
	{
		int num = items.Length;
		for (int i = 0; i < num; i++)
		{
			if (items[i] == itemToFind)
			{
				return i;
			}
		}
		return -1;
	}

	public static void RemoveItem<T>(this T[] items, T itemToRemove, ref int count) where T : class
	{
		bool flag = false;
		for (int i = 0; i < count; i++)
		{
			if (flag)
			{
				T val = items[i];
				items[i - 1] = val;
			}
			else if (items[i] == itemToRemove)
			{
				flag = true;
			}
		}
		if (flag)
		{
			count--;
		}
	}

	public static void Log<T>(this T[] items)
	{
		items.Log("");
	}

	public static void Log<T>(this T[] items, string name)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (name != "")
		{
			stringBuilder.Append(name);
			stringBuilder.Append(": ");
		}
		stringBuilder.Append('[');
		int num = items.Length;
		for (int i = 0; i < num; i++)
		{
			stringBuilder.Append(items[i].ToString());
			if (i < num - 1)
			{
				stringBuilder.Append(',');
			}
		}
		stringBuilder.Append(']');
	}
}
