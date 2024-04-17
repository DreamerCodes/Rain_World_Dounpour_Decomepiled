using System;
using System.Collections.Generic;
using System.Linq;
using RWCustom;
using UnityEngine;

namespace Menu.Remix.MixedUI;

public class OpTab : IHoldUIelements
{
	public static readonly Vector2 _offset = new Vector2(558.01f, 120.01f);

	public readonly OptionInterface owner;

	public Color colorButton;

	public Color colorCanvas;

	public readonly string name;

	protected internal readonly FContainer _container;

	public bool isInactive { get; internal set; }

	public HashSet<UIelement> items { get; protected internal set; }

	public HashSet<UIfocusable> focusables { get; protected internal set; }

	public bool IsTab => true;

	public Vector2 CanvasSize => new Vector2(600f, 600f);

	public event OnEventHandler OnPreUpdate;

	public event OnEventHandler OnPostUpdate;

	public event OnGrafUpdateHandler OnPreGrafUpdate;

	public event OnGrafUpdateHandler OnPostGrafUpdate;

	public event OnEventHandler OnPreActivate;

	public event OnEventHandler OnPostActivate;

	public event OnEventHandler OnPreDeactivate;

	public event OnEventHandler OnPostDeactivate;

	public event OnEventHandler OnPreUnload;

	public OpTab(OptionInterface owner, string name = "")
	{
		this.owner = owner;
		this.name = name;
		_container = new FContainer
		{
			x = _offset.x,
			y = _offset.y,
			isVisible = false
		};
		if (!(this is MenuTab))
		{
			ConfigContainer.instance.Container.AddChild(_container);
		}
		isInactive = true;
		items = new HashSet<UIelement>();
		focusables = new HashSet<UIfocusable>();
		colorButton = MenuColorEffect.rgbMediumGrey;
		colorCanvas = MenuColorEffect.rgbMediumGrey;
	}

	private void _AddItem(UIelement element)
	{
		if (items.Contains(element))
		{
			return;
		}
		if (element.tab != null)
		{
			if (element.tab == this)
			{
				return;
			}
			RemoveItemsFromTab(element);
		}
		items.Add(element);
		_container.AddChild(element.myContainer);
		if (element is UIfocusable)
		{
			focusables.Add(element as UIfocusable);
		}
		element._SetTab(this);
	}

	public void AddItems(params UIelement[] elements)
	{
		foreach (UIelement element in elements)
		{
			_AddItem(element);
		}
	}

	public void RemoveItems(params UIelement[] items)
	{
		foreach (UIelement item in items)
		{
			_RemoveItem(item);
		}
	}

	private void _RemoveItem(UIelement item)
	{
		if (item.InScrollBox)
		{
			item._RemoveFromScrollBox();
		}
		while (items.Contains(item))
		{
			items.Remove(item);
		}
		_container.RemoveChild(item.myContainer);
		while (item is UIfocusable && focusables.Contains(item as UIfocusable))
		{
			focusables.Remove(item as UIfocusable);
		}
		item._SetTab(null);
		if (!(this is MenuTab) && ConfigContainer.FocusedElement == item)
		{
			ConfigContainer.instance._FocusNewElementInDirection(new IntVector2(-1, 0));
		}
	}

	public static void ShowItems(params UIelement[] items)
	{
		for (int i = 0; i < items.Length; i++)
		{
			items[i].Show();
		}
	}

	public static void HideItems(params UIelement[] items)
	{
		for (int i = 0; i < items.Length; i++)
		{
			items[i].Hide();
		}
	}

	public static void DestroyItems(params UIelement[] items)
	{
		foreach (UIelement uIelement in items)
		{
			uIelement.Hide();
			uIelement.tab.RemoveItems(uIelement);
			uIelement.Unload();
		}
	}

	public static void RemoveItemsFromTab(params UIelement[] items)
	{
		foreach (UIelement uIelement in items)
		{
			uIelement.tab._RemoveItem(uIelement);
		}
	}

	internal void _GrafUpdate(float timeStacker)
	{
		this.OnPreGrafUpdate?.Invoke(timeStacker);
		UIelement[] array = items.ToArray();
		foreach (UIelement uIelement in array)
		{
			if (!uIelement.IsInactive)
			{
				uIelement.GrafUpdate(timeStacker);
			}
		}
		this.OnPostGrafUpdate?.Invoke(timeStacker);
	}

	internal void _Update()
	{
		this.OnPreUpdate?.Invoke();
		UIelement[] array = items.ToArray();
		foreach (UIelement uIelement in array)
		{
			if (!uIelement.IsInactive)
			{
				uIelement.Update();
			}
		}
		this.OnPostUpdate?.Invoke();
	}

	internal void _Deactivate()
	{
		this.OnPreDeactivate?.Invoke();
		isInactive = true;
		_container.isVisible = false;
		UIelement[] array = items.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Deactivate();
		}
		this.OnPostDeactivate?.Invoke();
	}

	internal void _Activate()
	{
		this.OnPreActivate?.Invoke();
		isInactive = false;
		_container.isVisible = true;
		UIelement[] array = items.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Reactivate();
		}
		this.OnPostActivate?.Invoke();
	}

	internal void _Unload()
	{
		try
		{
			this.OnPreUnload?.Invoke();
		}
		catch (Exception msg)
		{
			MachineConnector.LogError(msg);
		}
		UIelement[] array = items.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Unload();
		}
		_container.RemoveAllChildren();
		_container.RemoveFromContainer();
	}
}
