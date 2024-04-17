using UnityEngine;

namespace Menu;

public class IntMessageButton : SimpleButton
{
	public int message;

	public IntMessageButton(Menu menu, MenuObject owner, string displayText, string signalText, Vector2 pos, Vector2 size, int message)
		: base(menu, owner, displayText, signalText, pos, size)
	{
		this.message = message;
	}

	public override void Clicked()
	{
		Singal(this, message.ToString());
	}
}
