using System.Collections.Generic;
using UnityEngine;

namespace Menu.Remix.MixedUI;

public interface IHoldUIelements
{
	HashSet<UIelement> items { get; }

	bool IsTab { get; }

	Vector2 CanvasSize { get; }

	void AddItems(params UIelement[] elements);
}
