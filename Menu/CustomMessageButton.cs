using UnityEngine;

namespace Menu;

public class CustomMessageButton : SimpleButton
{
	public string message;

	public CustomMessageButton(Menu menu, MenuObject owner, string displayText, string signalText, Vector2 pos, Vector2 size, string message)
		: base(menu, owner, displayText, signalText, pos, size)
	{
		this.message = message;
	}

	public override void Clicked()
	{
		Singal(this, message.ToString());
	}
}
