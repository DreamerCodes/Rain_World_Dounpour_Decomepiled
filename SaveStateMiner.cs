using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using RWCustom;

public static class SaveStateMiner
{
	public class Target
	{
		public string dataID;

		public string divider;

		public string cap;

		public int maxChars;

		public string SearchString => dataID + ((divider != null) ? divider : "");

		public Target(string dataID, string divider, string cap, int maxChars)
		{
			this.dataID = dataID;
			this.divider = divider;
			this.cap = cap;
			this.maxChars = maxChars;
		}
	}

	public class Result
	{
		public string name;

		public string data;

		public Result(string name, string data)
		{
			this.name = name;
			this.data = data;
		}
	}

	public static List<Result> Mine(RainWorld rainWorld, string saveStateString, List<Target> targets)
	{
		List<Result> list = new List<Result>();
		for (int i = 0; i < targets.Count; i++)
		{
			int num = saveStateString.IndexOf(targets[i].SearchString);
			if (num == -1)
			{
				Custom.LogWarning("Savestate miner couldn't find:", targets[i].SearchString);
				continue;
			}
			int count = Math.Min(targets[i].SearchString.Length + targets[i].maxChars, saveStateString.Length - num);
			int num2 = saveStateString.IndexOf(targets[i].cap, num, count);
			if (num2 == -1)
			{
				Custom.LogWarning($"Savestate miner couldn't find cap for {targets[i].SearchString} (cap: {targets[i].cap}, length: {targets[i].maxChars})");
				continue;
			}
			string input = saveStateString.Substring(num, num2 - num);
			if (targets[i].divider == null)
			{
				list.Add(new Result(targets[i].dataID, null));
				continue;
			}
			string[] array = Regex.Split(input, targets[i].divider);
			if (array.Length > 1)
			{
				list.Add(new Result(array[0], array[1]));
			}
		}
		return list;
	}
}
