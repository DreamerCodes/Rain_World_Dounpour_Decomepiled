using UnityEngine;

namespace DevInterface;

public abstract class RectangularDevUINode : PositionedDevUINode
{
	public Vector2 size;

	public bool MouseOver
	{
		get
		{
			if (owner != null)
			{
				if (owner.mousePos.x > absPos.x && owner.mousePos.x < absPos.x + size.x && owner.mousePos.y > absPos.y)
				{
					return owner.mousePos.y < absPos.y + size.y;
				}
				return false;
			}
			return false;
		}
	}

	public RectangularDevUINode(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, Vector2 size)
		: base(owner, IDstring, parentNode, pos)
	{
		this.size = size;
	}
}
