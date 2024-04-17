using System.Collections.Generic;

public class ExtEnumType
{
	public List<string> entries = new List<string>();

	public int version;

	public int Count => entries.Count;

	public void AddEntry(string name)
	{
		if (!entries.Contains(name))
		{
			entries.Add(name);
		}
	}

	public void RemoveEntry(string name)
	{
		if (entries.Contains(name))
		{
			entries.Remove(name);
		}
	}

	public string GetEntry(int index)
	{
		if (index < 0 || index >= entries.Count)
		{
			return null;
		}
		return entries[index];
	}
}
