using System;
using System.Collections.Generic;
using UnityEngine;

public class RXProfiler : MonoBehaviour
{
	public static Dictionary<Type, List<WeakReference>> instancesByType;

	static RXProfiler()
	{
		instancesByType = new Dictionary<Type, List<WeakReference>>();
	}

	public void Update()
	{
	}

	private static void CheckInstanceCounts()
	{
		foreach (KeyValuePair<Type, List<WeakReference>> item in instancesByType)
		{
			int num = 0;
			List<WeakReference> value = item.Value;
			for (int num2 = value.Count - 1; num2 >= 0; num2--)
			{
				if (value[num2].Target == null)
				{
					num++;
					value.RemoveAt(num2);
				}
			}
			_ = 0;
		}
	}

	public static void TrackLifeCycle(object thing)
	{
	}
}
