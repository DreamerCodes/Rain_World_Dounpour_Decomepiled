using RWCustom;
using UnityEngine;

namespace DevInterface;

public class DevUILabel : RectangularDevUINode
{
	private string t;

	public string Text
	{
		get
		{
			return t;
		}
		set
		{
			if (fLabels.Count > 0)
			{
				fLabels[0].text = value;
				t = value;
			}
		}
	}

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

	public Color textColor
	{
		get
		{
			if (fLabels.Count <= 0)
			{
				return Color.clear;
			}
			return fLabels[0].color;
		}
		set
		{
			if (fLabels.Count > 0)
			{
				fLabels[0].color = value;
			}
		}
	}

	public DevUILabel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, float width, string text)
		: base(owner, IDstring, parentNode, pos, new Vector2(width, 16f))
	{
		t = text;
		fSprites.Add(new FSprite("pixel"));
		fSprites[fSprites.Count - 1].scaleX = width;
		fSprites[fSprites.Count - 1].scaleY = 16f;
		fSprites[fSprites.Count - 1].anchorX = 0f;
		fSprites[fSprites.Count - 1].anchorY = 0f;
		fSprites[fSprites.Count - 1].color = new Color(1f, 1f, 1f);
		fSprites[fSprites.Count - 1].alpha = 0.5f;
		if (owner != null)
		{
			Futile.stage.AddChild(fSprites[fSprites.Count - 1]);
		}
		fLabels.Add(new FLabel(Custom.GetFont(), text));
		fLabels[fLabels.Count - 1].alignment = FLabelAlignment.Left;
		fLabels[fLabels.Count - 1].color = new Color(0f, 0f, 0f);
		fLabels[fLabels.Count - 1].anchorX = 0f;
		fLabels[fLabels.Count - 1].anchorY = 0f;
		if (owner != null)
		{
			Futile.stage.AddChild(fLabels[fLabels.Count - 1]);
		}
	}

	public override void Move(Vector2 newPos)
	{
		base.Move(newPos);
	}

	public override void Refresh()
	{
		base.Refresh();
		MoveSprite(0, absPos);
		MoveLabel(0, absPos);
	}
}
