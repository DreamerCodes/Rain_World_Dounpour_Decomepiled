using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using RWCustom;

namespace Expedition;

public static class ExpLog
{
	public static List<string> onceText = new List<string>();

	public static void LogChallengeTypes()
	{
		if (!RainWorld.ShowLogs)
		{
			return;
		}
		Log("Challenge Types");
		foreach (Type item in from TheType in Assembly.GetAssembly(typeof(Challenge)).GetTypes()
			where TheType.IsClass && !TheType.IsAbstract && TheType.IsSubclassOf(typeof(Challenge))
			select TheType)
		{
			Log("[" + item.Name + "]");
		}
	}

	public static void ClearLog()
	{
		if (RainWorld.ShowLogs)
		{
			File.WriteAllText(Custom.RootFolderDirectory() + Path.DirectorySeparatorChar + "ExpLog.txt", "[EXPEDITION LOGGER] - " + DateTime.Now.ToString());
		}
	}

	public static void Log(string text)
	{
		if (RainWorld.ShowLogs)
		{
			string path = Custom.RootFolderDirectory() + Path.DirectorySeparatorChar + "ExpLog.txt";
			if (!File.Exists(path))
			{
				ClearLog();
			}
			File.AppendAllText(path, "\n" + text);
		}
	}

	public static void LogOnce(string text)
	{
		if (RainWorld.ShowLogs)
		{
			string path = Custom.RootFolderDirectory() + Path.DirectorySeparatorChar + "ExpLog.txt";
			if (!File.Exists(path))
			{
				ClearLog();
			}
			if (!onceText.Contains(text))
			{
				File.AppendAllText(path, "\n" + text);
				onceText.Add(text);
			}
		}
	}
}
