namespace Menu.Remix.MixedUI;

public struct ListItem
{
	public readonly string name;

	public readonly int value;

	internal int index;

	public string desc;

	public string displayName;

	public string EffectiveDisplayName
	{
		get
		{
			if (!string.IsNullOrEmpty(displayName))
			{
				return displayName;
			}
			return name;
		}
	}

	public ListItem(string name, int value = int.MaxValue)
	{
		this.name = name;
		this.value = value;
		index = -1;
		desc = "";
		displayName = name;
	}

	public ListItem(string name, string displayName, int value = int.MaxValue)
	{
		this.name = name;
		this.value = value;
		index = -1;
		desc = "";
		this.displayName = displayName;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ListItem listItem))
		{
			return false;
		}
		if (name == listItem.name)
		{
			return value == listItem.value;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return name.GetHashCode();
	}

	public static bool operator ==(ListItem left, ListItem right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(ListItem left, ListItem right)
	{
		return !(left == right);
	}

	public static int Comparer(ListItem x, ListItem y)
	{
		if (x.value == y.value)
		{
			return GetRealName(x.name).CompareTo(GetRealName(y.name));
		}
		return x.value.CompareTo(y.value);
	}

	public static string GetRealName(string text)
	{
		text = text.ToLower();
		if (text.StartsWith("a "))
		{
			return text.Remove(0, 2);
		}
		if (text.StartsWith("an "))
		{
			return text.Remove(0, 3);
		}
		if (text.StartsWith("the "))
		{
			return text.Remove(0, 4);
		}
		return text;
	}

	public static bool SearchMatch(string query, string text)
	{
		if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(query))
		{
			return false;
		}
		query = query.Trim().ToLower();
		text = text.ToLower();
		if (query.Contains(" "))
		{
			string[] array = query.Split(' ');
			foreach (string text2 in array)
			{
				if (!string.IsNullOrEmpty(text2) && !text.Contains(text2))
				{
					return false;
				}
			}
			return true;
		}
		if (text.Contains(query))
		{
			return true;
		}
		if (!text.Contains(query.Substring(0, 1)))
		{
			return false;
		}
		if (query.Length < 2)
		{
			return false;
		}
		string text3 = text.Substring(text.IndexOf(query[0]) + 1);
		for (int j = 1; j < query.Length; j++)
		{
			if (text3.Contains(query[j].ToString()))
			{
				text3 = text3.Substring(text3.IndexOf(query[j]) + 1);
				continue;
			}
			return false;
		}
		return true;
	}
}
