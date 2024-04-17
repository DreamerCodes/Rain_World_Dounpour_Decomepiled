using System.Collections.Generic;
using System.Linq;
using UnityEngine.Scripting;

namespace Modding.Passages;

public static class CustomPassages
{
	public static readonly List<CustomPassage> RegisteredPassages = new List<CustomPassage>();

	public static CustomPassage PassageForID(WinState.EndgameID id)
	{
		return RegisteredPassages.FirstOrDefault((CustomPassage x) => x.ID == id);
	}

	[Preserve]
	public static void Register(params CustomPassage[] passages)
	{
		foreach (CustomPassage passage in passages)
		{
			if (!RegisteredPassages.Any((CustomPassage x) => x.ID == passage.ID))
			{
				RegisteredPassages.Add(passage);
				passage.ApplyHooks();
			}
		}
	}
}
