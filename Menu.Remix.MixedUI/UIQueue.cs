using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace Menu.Remix.MixedUI;

public abstract class UIQueue
{
	public OnUIQueueEventHandler OnPreInitialize;

	public OnUIQueueEventHandler OnPostInitialize;

	public bool hasInitialized;

	public const float GAP_Y = 10f;

	protected virtual float sizeY { get; }

	protected UIQueue()
	{
		hasInitialized = false;
	}

	protected virtual List<UIelement> _InitializeThisQueue(IHoldUIelements holder, float posX, ref float offsetY)
	{
		throw new NotImplementedException("This UIQueue has not overriden its default _InitializeThisQueue");
	}

	protected float GetPosY(IHoldUIelements holder, float offsetY)
	{
		return holder.CanvasSize.y - offsetY;
	}

	protected float GetWidth(IHoldUIelements holder, float posX, float max, float min = 0f)
	{
		return Mathf.Clamp(holder.CanvasSize.x - posX - 100f, min, max);
	}

	protected float GetHeight(float posY, float max, float min = 0f)
	{
		return Mathf.Clamp(posY - 5f, min, max);
	}

	public static List<UIelement> InitializeQueues(IHoldUIelements holder, float posX, ref float offsetY, params UIQueue[] queues)
	{
		List<UIelement> list = new List<UIelement>();
		bool flag = false;
		float num = CalculateSizeY(queues);
		if (holder.CanvasSize.y < num + offsetY)
		{
			flag = _ResizeCanvas(ref holder, num + offsetY + 10f);
		}
		foreach (UIQueue uIQueue in queues)
		{
			uIQueue.OnPreInitialize?.Invoke(uIQueue);
			list.AddRange(uIQueue._InitializeThisQueue(holder, posX, ref offsetY));
			uIQueue.OnPostInitialize?.Invoke(uIQueue);
			if (uIQueue.sizeY > 0f)
			{
				offsetY += 10f;
			}
		}
		holder.AddItems(list.ToArray());
		UIfocusable uIfocusable = null;
		for (int j = 0; j < queues.Length; j++)
		{
			if (queues[j] is UIfocusable.FocusableQueue)
			{
				UIfocusable mainFocusable = (queues[j] as UIfocusable.FocusableQueue).mainFocusable;
				if (uIfocusable == null)
				{
					uIfocusable = mainFocusable;
					continue;
				}
				UIfocusable.MutualVerticalFocusableBind(mainFocusable, uIfocusable);
				uIfocusable = mainFocusable;
			}
		}
		if (flag)
		{
			list.Insert(0, holder as OpScrollBox);
		}
		return list;
	}

	public static float CalculateSizeY(params UIQueue[] queues)
	{
		float num = 0f;
		foreach (UIQueue uIQueue in queues)
		{
			num += uIQueue.sizeY + ((uIQueue.sizeY > 0f) ? 10f : 0f);
		}
		return num;
	}

	private static bool _ResizeCanvas(ref IHoldUIelements holder, float newSize)
	{
		if (newSize > 10000f)
		{
			throw new OverflowException("Expected size exceeds maximum 10,000 pixels!");
		}
		if (holder.IsTab)
		{
			holder = new OpScrollBox(holder as OpTab, newSize);
			return true;
		}
		if (holder is OpScrollBox)
		{
			(holder as OpScrollBox).SetContentSize(newSize);
		}
		return false;
	}

	public static string GetFirstSentence(string text)
	{
		if (!text.Contains(".") && !text.Contains("<LINE>") && !text.Contains("<WWLINE>") && !text.Contains("\n"))
		{
			return text;
		}
		return Custom.ReplaceWordWrapLineDelimeters(text).Replace("\r\n", "\n").Split('\n')[0].Split('.')[0];
	}

	public static string Translate(string text)
	{
		return OptionInterface.Translate(text);
	}
}
