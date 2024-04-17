using UnityEngine;

namespace Menu;

public class ManualPage : PositionedMenuObject
{
	public string topicName;

	public ManualPage(Menu menu, MenuObject owner)
		: base(menu, owner, new Vector2(0f, 0f))
	{
		topicName = "WIP";
	}
}
