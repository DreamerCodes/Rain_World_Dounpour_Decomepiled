using UnityEngine;

namespace DevInterface;

public class SwitchPageButton : Button
{
	private int index;

	public SwitchPageButton(DevUI owner, DevUINode parentNode, Vector2 pos, float width, int index)
		: base(owner, "Switch_Page_" + owner.pages[index], parentNode, pos, width, owner.pages[index])
	{
		this.index = index;
	}

	public override void Clicked()
	{
		owner.SwitchPage(index);
	}
}
