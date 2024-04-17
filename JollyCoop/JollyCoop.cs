using JollyCoop.JollyMenu;
using RWCustom;
using UnityEngine;

namespace JollyCoop;

public class JollyCoop
{
	public static string MOD_ID = "jollycoop";

	public static string ValidationString()
	{
		string text = "[" + MOD_ID + "]  ";
		text = text + Custom.rainWorld.options.jollyDifficulty?.ToString() + " ";
		text = text + Custom.rainWorld.options.jollyCameraInputSpeed?.ToString() + " ";
		bool[] array = new bool[6]
		{
			Custom.rainWorld.options.friendlyLizards,
			Custom.rainWorld.options.jollyHud,
			Custom.rainWorld.options.friendlyFire,
			Custom.rainWorld.options.friendlySteal,
			Custom.rainWorld.options.smartShortcuts,
			Custom.rainWorld.options.cameraCycling
		};
		int num = 0;
		for (int i = 0; i < array.Length; i++)
		{
			num += (int)Mathf.Pow(2f, i % 4) * (array[i] ? 1 : 0);
			if (i % 4 == 3 || i == array.Length - 1)
			{
				text += num.ToString("X");
				num = 0;
			}
		}
		for (int j = 0; j < Custom.rainWorld.options.jollyPlayerOptionsArray.Length; j++)
		{
			JollyPlayerOptions jollyPlayerOptions = Custom.rainWorld.options.jollyPlayerOptionsArray[j];
			if (jollyPlayerOptions.joined)
			{
				text = text + " " + ((jollyPlayerOptions.playerClass != null) ? jollyPlayerOptions.playerClass.value : "Def");
				if (jollyPlayerOptions.backSpear)
				{
					text += "_bs";
				}
				if (jollyPlayerOptions.isPup)
				{
					text += "_pup";
				}
			}
		}
		return text;
	}
}
