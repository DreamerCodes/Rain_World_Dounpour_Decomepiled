using UnityEngine;

namespace DevInterface;

public class LineRect : RectangularDevUINode
{
	private Color col;

	public Color LineColor
	{
		get
		{
			return col;
		}
		set
		{
			col = value;
			for (int i = 0; i < fSprites.Count; i++)
			{
				fSprites[i].color = col;
			}
		}
	}

	public LineRect(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, Vector2 size)
		: base(owner, IDstring, parentNode, pos, size)
	{
		for (int i = 0; i < 4; i++)
		{
			fSprites.Add(new FSprite("pixel"));
			fSprites[i].anchorX = 0f;
			fSprites[i].anchorY = 0f;
			if (i % 2 == 0)
			{
				fSprites[i].scaleX = size.x;
			}
			else
			{
				fSprites[i].scaleY = size.y;
			}
			if (owner != null)
			{
				Futile.stage.AddChild(fSprites[i]);
			}
		}
	}

	public override void Refresh()
	{
		MoveSprite(0, absPos);
		MoveSprite(1, absPos);
		MoveSprite(2, absPos + new Vector2(0f, size.y));
		MoveSprite(3, absPos + new Vector2(size.x, 0f));
		base.Refresh();
	}
}
