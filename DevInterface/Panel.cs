using RWCustom;
using UnityEngine;

namespace DevInterface;

public class Panel : RectangularDevUINode
{
	public class HorizontalDivider : PositionedDevUINode
	{
		public HorizontalDivider(DevUI owner, string IDstring, Panel parentNode, float height)
			: base(owner, IDstring, parentNode, new Vector2(0f, height))
		{
			fSprites.Add(new FSprite("pixel"));
			fSprites[0].scaleX = parentNode.size.x;
			fSprites[0].anchorX = 0f;
			if (owner != null)
			{
				Futile.stage.AddChild(fSprites[0]);
			}
		}

		public override void Refresh()
		{
			base.Refresh();
			MoveSprite(0, absPos);
		}
	}

	public bool dragged;

	public Vector2 moveOffset;

	public bool collapsed;

	public string Title
	{
		get
		{
			return fLabels[0].text;
		}
		set
		{
			fLabels[0].text = value;
		}
	}

	private Vector2 MoveButtonPos => nonCollapsedAbsPos + size + new Vector2(-10f, 10f);

	private Vector2 CollapseButtonPos => nonCollapsedAbsPos + size + new Vector2(-25f, 10f);

	public virtual Vector2 nonCollapsedAbsPos => base.absPos;

	public override Vector2 absPos
	{
		get
		{
			if (collapsed)
			{
				return new Vector2(-1000f, -1000f);
			}
			return base.absPos;
		}
		set
		{
			base.absPos = value;
		}
	}

	public Panel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, Vector2 size, string title)
		: base(owner, IDstring, parentNode, pos, size)
	{
		fLabels.Add(new FLabel(Custom.GetFont(), title));
		fLabels[0].anchorX = 0f;
		fLabels[0].anchorY = 0f;
		fSprites.Add(new FSprite("pixel"));
		fSprites.Add(new FSprite("pixel"));
		fSprites.Add(new FSprite("pixel"));
		fSprites.Add(new FSprite("pixel"));
		fSprites.Add(new FSprite("pixel"));
		fSprites.Add(new FSprite("Circle20"));
		fSprites.Add(new FSprite("Circle20"));
		fSprites[0].color = new Color(0f, 0f, 0f);
		fSprites[0].alpha = 0.5f;
		fSprites[5].scale = 0.5f;
		fSprites[6].scale = 0.5f;
		for (int i = 0; i < 5; i++)
		{
			fSprites[i].anchorX = 0f;
			fSprites[i].anchorY = 0f;
			if (owner != null)
			{
				Futile.stage.AddChild(fSprites[i]);
			}
		}
		if (owner != null)
		{
			Futile.stage.AddChild(fSprites[5]);
			Futile.stage.AddChild(fSprites[6]);
			Futile.stage.AddChild(fLabels[0]);
		}
	}

	public override void Update()
	{
		if (!collapsed)
		{
			base.Update();
		}
		if (fSprites.Count == 0)
		{
			return;
		}
		fSprites[5].color = new Color(1f, 1f, 1f);
		if (owner != null && Custom.DistLess(owner.mousePos, MoveButtonPos, 5f))
		{
			fSprites[5].color = new Color(1f, 0f, 0f);
			if (owner.mouseClick)
			{
				dragged = true;
				moveOffset = nonCollapsedAbsPos - owner.mousePos;
			}
		}
		if (dragged && owner != null)
		{
			fSprites[5].color = new Color(0f, 0f, 1f);
			AbsMove(owner.mousePos + moveOffset);
			if (!owner.mouseDown)
			{
				dragged = false;
			}
		}
		fSprites[6].color = new Color(1f, 1f, 1f);
		if (owner != null && Custom.DistLess(owner.mousePos, CollapseButtonPos, 5f))
		{
			fSprites[6].color = (owner.mouseDown ? new Color(0f, 0f, 1f) : new Color(1f, 0f, 0f));
			if (owner.mouseClick)
			{
				ToggleCollapse();
			}
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

	public override void Refresh()
	{
		base.Refresh();
		if (collapsed)
		{
			if (fSprites.Count > 0)
			{
				fSprites[0].scaleY = 20f;
			}
			MoveSprite(0, nonCollapsedAbsPos + new Vector2(0f, size.y));
		}
		else
		{
			if (fSprites.Count > 0)
			{
				fSprites[0].scaleY = size.y + 20f;
			}
			MoveSprite(0, nonCollapsedAbsPos);
		}
		if (fSprites.Count > 4)
		{
			fSprites[0].scaleX = size.x;
			fSprites[1].scaleX = size.x;
			fSprites[3].scaleX = size.x;
			fSprites[2].scaleY = size.y;
			fSprites[4].scaleY = size.y;
		}
		MoveSprite(1, absPos + new Vector2(1f, 0f));
		MoveSprite(2, absPos);
		MoveSprite(3, nonCollapsedAbsPos + new Vector2(0f, size.y));
		MoveSprite(4, absPos + new Vector2(size.x, 1f));
		MoveLabel(0, nonCollapsedAbsPos + new Vector2(0f, size.y + 2f));
		MoveSprite(5, MoveButtonPos);
		MoveSprite(6, CollapseButtonPos);
	}

	public void ToggleCollapse()
	{
		collapsed = !collapsed;
		Refresh();
	}
}
