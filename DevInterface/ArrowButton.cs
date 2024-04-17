using UnityEngine;

namespace DevInterface;

public class ArrowButton : RectangularDevUINode
{
	public Color colorA = new Color(1f, 1f, 1f);

	public Color colorB = new Color(1f, 0f, 0f);

	public bool down;

	public Color spriteColor
	{
		get
		{
			if (fSprites.Count <= 0)
			{
				return Color.clear;
			}
			return fSprites[0].color;
		}
		set
		{
			if (fSprites.Count > 0)
			{
				fSprites[0].color = value;
			}
		}
	}

	public Color arrowColor
	{
		get
		{
			if (fSprites.Count <= 1)
			{
				return Color.clear;
			}
			return fSprites[1].color;
		}
		set
		{
			if (fSprites.Count > 0)
			{
				fSprites[1].color = value;
			}
		}
	}

	public ArrowButton(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, float rotation)
		: base(owner, IDstring, parentNode, pos, new Vector2(16f, 16f))
	{
		fSprites.Add(new FSprite("pixel"));
		fSprites[fSprites.Count - 1].color = new Color(1f, 1f, 1f);
		fSprites[fSprites.Count - 1].scale = 16f;
		fSprites[fSprites.Count - 1].anchorX = 0f;
		fSprites[fSprites.Count - 1].anchorY = 0f;
		fSprites[fSprites.Count - 1].alpha = 0.5f;
		fSprites.Add(new FSprite("ShortcutArrow"));
		fSprites[fSprites.Count - 1].color = new Color(1f, 1f, 1f);
		fSprites[fSprites.Count - 1].rotation = rotation;
		if (owner != null)
		{
			Futile.stage.AddChild(fSprites[fSprites.Count - 2]);
			Futile.stage.AddChild(fSprites[fSprites.Count - 1]);
		}
	}

	public override void Update()
	{
		base.Update();
		spriteColor = (base.MouseOver ? colorB : colorA);
		arrowColor = (base.MouseOver ? colorA : colorB);
		if (owner != null && owner.mouseClick && base.MouseOver)
		{
			Clicked();
			down = true;
			colorB = new Color(0f, 0f, 1f);
		}
		if ((down && !base.MouseOver) || owner == null || !owner.mouseDown)
		{
			down = false;
			colorB = new Color(1f, 0f, 0f);
		}
	}

	public override void Refresh()
	{
		base.Refresh();
		MoveSprite(0, absPos);
		MoveSprite(1, absPos + new Vector2(8f, 8f));
	}

	public virtual void Clicked()
	{
		DevUINode devUINode = this;
		while (devUINode != null)
		{
			devUINode = devUINode.parentNode;
			if (devUINode is IDevUISignals)
			{
				(devUINode as IDevUISignals).Signal(DevUISignalType.ButtonClick, this, "");
				break;
			}
		}
	}
}
