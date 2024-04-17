using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Scripting;

namespace Modding.Expedition;

public static class CustomPerks
{
	public static readonly List<CustomPerk> RegisteredPerks = new List<CustomPerk>();

	public static List<string> CustomPerkGroups => RegisteredPerks.Select((CustomPerk x) => x.Group).Distinct().ToList();

	public static CustomPerk PerkForID(string id)
	{
		return RegisteredPerks.FirstOrDefault((CustomPerk x) => x.ID == id);
	}

	[Preserve]
	public static void Register(params CustomPerk[] perks)
	{
		foreach (CustomPerk perk in perks)
		{
			if (!RegisteredPerks.Any((CustomPerk x) => x.ID == perk.ID))
			{
				if (!perk.ID.StartsWith("unl-"))
				{
					throw new Exception("Perk IDs must start with \"unl-\" (" + perk.ID + ")");
				}
				RegisteredPerks.Add(perk);
				perk.ApplyHooks();
			}
		}
	}
}
