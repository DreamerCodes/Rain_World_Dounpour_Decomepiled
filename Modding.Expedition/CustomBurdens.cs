using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Scripting;

namespace Modding.Expedition;

public static class CustomBurdens
{
	public static readonly List<CustomBurden> RegisteredBurdens = new List<CustomBurden>();

	public static List<string> CustomBurdenGroups => RegisteredBurdens.Select((CustomBurden x) => x.Group).Distinct().ToList();

	public static CustomBurden BurdenForID(string id)
	{
		return RegisteredBurdens.FirstOrDefault((CustomBurden x) => x.ID == id);
	}

	[Preserve]
	public static void Register(params CustomBurden[] burdens)
	{
		foreach (CustomBurden burden in burdens)
		{
			if (!RegisteredBurdens.Any((CustomBurden x) => x.ID == burden.ID))
			{
				if (!burden.ID.StartsWith("bur-"))
				{
					throw new Exception("Burden IDs must start with \"bur-\" (" + burden.ID + ")");
				}
				RegisteredBurdens.Add(burden);
				burden.ApplyHooks();
			}
		}
	}
}
