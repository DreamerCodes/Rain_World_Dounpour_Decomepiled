using UnityEngine;

namespace Menu;

public abstract class RectangularMenuObject : PositionedMenuObject
{
	public Vector2 size;

	public Vector2 lastSize;

	public virtual bool MouseOver
	{
		get
		{
			Vector2 screenPos = base.ScreenPos;
			if (menu.mousePosition.x > screenPos.x && menu.mousePosition.y > screenPos.y && menu.mousePosition.x < screenPos.x + size.x)
			{
				return menu.mousePosition.y < screenPos.y + size.y;
			}
			return false;
		}
	}

	public Vector2 DrawSize(float timeStacker)
	{
		return Vector2.Lerp(lastSize, size, timeStacker);
	}

	public RectangularMenuObject(Menu menu, MenuObject owner, Vector2 pos, Vector2 size)
		: base(menu, owner, pos)
	{
		this.size = size;
	}

	public override void Update()
	{
		base.Update();
		lastSize = size;
	}
}
