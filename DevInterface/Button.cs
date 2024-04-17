using UnityEngine;

namespace DevInterface;

public class Button : MouseOverSwitchColorLabel
{
	public bool down;

	public Color? overrideTextColor;

	public Button(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, float width, string text)
		: base(owner, IDstring, parentNode, pos, width, text)
	{
		overrideTextColor = null;
	}

	public override void Update()
	{
		base.Update();
		if (owner != null && owner.mouseClick && base.MouseOver)
		{
			Clicked();
			down = true;
			colorB = new Color(0f, 0f, 1f);
		}
		if ((down && !base.MouseOver) || owner == null || !owner.mouseDown)
		{
			down = false;
			colorB = (overrideTextColor.HasValue ? overrideTextColor.Value : new Color(1f, 0f, 0f));
		}
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
