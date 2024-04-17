using UnityEngine;

namespace DevInterface;

public class MouseOverSwitchColorLabel : DevUILabel
{
	public Color colorA = new Color(1f, 1f, 1f);

	public Color colorB = new Color(1f, 0f, 0f);

	public MouseOverSwitchColorLabel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, float width, string text)
		: base(owner, IDstring, parentNode, pos, width, text)
	{
	}

	public override void Update()
	{
		base.Update();
		base.spriteColor = (base.MouseOver ? colorB : colorA);
		base.textColor = (base.MouseOver ? colorA : colorB);
	}
}
