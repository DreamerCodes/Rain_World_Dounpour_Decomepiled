using UnityEngine;

namespace DevInterface;

public abstract class PositionedDevUINode : DevUINode
{
	public Vector2 pos;

	public virtual Vector2 absPos
	{
		get
		{
			if (parentNode == null || !(parentNode is PositionedDevUINode))
			{
				return pos;
			}
			return (parentNode as PositionedDevUINode).absPos + pos;
		}
		set
		{
			if (parentNode == null || !(parentNode is PositionedDevUINode))
			{
				pos = value;
			}
			else
			{
				pos = (parentNode as PositionedDevUINode).absPos + value;
			}
		}
	}

	public PositionedDevUINode(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos)
		: base(owner, IDstring, parentNode)
	{
		this.pos = pos;
	}

	public virtual void Move(Vector2 newPos)
	{
		pos = newPos;
		Refresh();
	}

	public void AbsMove(Vector2 newPos)
	{
		if (parentNode == null || !(parentNode is PositionedDevUINode))
		{
			Move(newPos);
		}
		else
		{
			Move(newPos - (parentNode as PositionedDevUINode).absPos);
		}
	}
}
