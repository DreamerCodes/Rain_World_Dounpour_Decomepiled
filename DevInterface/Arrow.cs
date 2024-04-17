using UnityEngine;

namespace DevInterface;

public class Arrow : RectangularDevUINode
{
	public Arrow(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, float rotation)
		: base(owner, IDstring, parentNode, pos, new Vector2(9f, 9f))
	{
		fSprites.Add(new FSprite("ShortcutArrow"));
		fSprites[fSprites.Count - 1].color = new Color(1f, 1f, 1f);
		fSprites[fSprites.Count - 1].rotation = rotation;
		if (owner != null)
		{
			Futile.stage.AddChild(fSprites[fSprites.Count - 1]);
		}
	}

	public override void Refresh()
	{
		base.Refresh();
		MoveSprite(0, absPos + new Vector2(4.5f, 4.5f));
	}
}
