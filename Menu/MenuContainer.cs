using UnityEngine;

namespace Menu;

public class MenuContainer : PositionedMenuObject
{
	public MenuContainer(Menu menu, MenuObject owner, Vector2 pos)
		: base(menu, owner, pos)
	{
		myContainer = new FContainer();
		owner.Container.AddChild(myContainer);
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		myContainer.x = pos.x;
		myContainer.y = pos.y;
	}

	public override void RemoveSprites()
	{
		base.RemoveSprites();
		myContainer.RemoveFromContainer();
	}
}
