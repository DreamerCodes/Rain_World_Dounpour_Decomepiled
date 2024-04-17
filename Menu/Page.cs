using System.Collections.Generic;
using UnityEngine;

namespace Menu;

public class Page : PositionedMenuObject
{
	public string name;

	public int index;

	public List<SelectableMenuObject> selectables;

	public MenuObject lastSelectedObject;

	public MouseCursor mouseCursor;

	public Page(Menu menu, MenuObject owner, string name, int index)
		: base(menu, owner, new Vector2(1f / 3f, 1f / 3f))
	{
		this.name = name;
		this.index = index;
		selectables = new List<SelectableMenuObject>();
		mouseCursor = new MouseCursor(menu, this, new Vector2(-100f, -100f));
		subObjects.Add(mouseCursor);
	}
}
