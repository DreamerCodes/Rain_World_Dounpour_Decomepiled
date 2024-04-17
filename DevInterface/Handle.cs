using RWCustom;
using UnityEngine;

namespace DevInterface;

public class Handle : PositionedDevUINode
{
	public bool dragged;

	public Vector2 mouseOffset;

	public bool MouseOver
	{
		get
		{
			if (owner != null)
			{
				return Custom.DistLess(absPos, owner.mousePos, 10f);
			}
			return false;
		}
	}

	public Handle(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos)
		: base(owner, IDstring, parentNode, pos)
	{
		fSprites.Add(new FSprite("Circle20"));
		fSprites[0].scale = 0.5f;
		owner?.placedObjectsContainer.AddChild(fSprites[0]);
	}

	public override void Update()
	{
		base.Update();
		if (owner != null && dragged)
		{
			SetColor(new Color(0f, 0f, 1f));
			Move(owner.mousePos + mouseOffset);
			if (!owner.mouseDown)
			{
				dragged = false;
			}
		}
		else if (MouseOver)
		{
			SetColor(new Color(1f, 0f, 0f));
			if (owner != null && owner.mouseClick)
			{
				mouseOffset = pos - owner.mousePos;
				dragged = true;
			}
		}
		else
		{
			SetColor(Input.GetKey("b") ? new Color(0f, 0f, 0f) : new Color(1f, 1f, 1f));
		}
		if (owner == null)
		{
			dragged = false;
		}
		else if (owner.draggedNode != null)
		{
			dragged = false;
		}
		else if (dragged)
		{
			owner.draggedNode = this;
		}
	}

	public virtual void SetColor(Color col)
	{
		fSprites[0].color = col;
	}

	public override void Refresh()
	{
		base.Refresh();
		MoveSprite(0, absPos);
	}
}
