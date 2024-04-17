using System;
using UnityEngine;

namespace Menu;

public class WrappedDialogBoxNotify : Dialog
{
	public MenuDialogBox dialogBox;

	public Action onContinue;

	public WrappedDialogBoxNotify(ProcessManager manager, string text, Action onContinue)
		: base(manager)
	{
		this.onContinue = onContinue;
		dialogBox = new DialogBoxNotify(this, pages[0], text, "CONTINUE", new Vector2(manager.rainWorld.options.ScreenSize.x / 2f - 240f + (1366f - manager.rainWorld.options.ScreenSize.x) / 2f, 224f), new Vector2(480f, 320f));
		pages[0].subObjects.Add(dialogBox);
	}

	public override void Singal(MenuObject sender, string message)
	{
		base.Singal(sender, message);
		if (message == "CONTINUE")
		{
			onContinue?.Invoke();
			manager.StopSideProcess(this);
		}
	}
}
