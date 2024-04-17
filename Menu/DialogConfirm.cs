using System;
using UnityEngine;

namespace Menu;

public class DialogConfirm : Dialog
{
	private SimpleButton okButton;

	private SimpleButton cancelButton;

	private Action onOK;

	private Action onCancel;

	public DialogConfirm(string description, ProcessManager manager, Action onOK, Action onCancel)
		: base(description, manager)
	{
		Init(onOK, onCancel);
	}

	public DialogConfirm(string description, Vector2 size, ProcessManager manager, Action onOK, Action onCancel)
		: base(description, size, manager)
	{
		Init(onOK, onCancel);
	}

	public override void Singal(MenuObject sender, string message)
	{
		switch (message)
		{
		case "OK":
			if (onOK != null)
			{
				onOK();
			}
			manager.StopSideProcess(this);
			break;
		case "CANCEL":
			if (onCancel != null)
			{
				onCancel();
			}
			manager.StopSideProcess(this);
			break;
		}
	}

	private void Init(Action onOK, Action onCancel)
	{
		this.onOK = onOK;
		this.onCancel = onCancel;
		okButton = new SimpleButton(this, pages[0], Translate("ok"), "OK", new Vector2(pos.x + (size.x * 0.5f - 110f) * 0.5f, pos.y + Mathf.Max(size.y * 0.04f, 7f)), new Vector2(110f, 30f));
		cancelButton = new SimpleButton(this, pages[0], Translate("cancel"), "CANCEL", new Vector2(1366f - pos.x - (size.x * 0.5f - 110f) * 0.5f - 110f, pos.y + Mathf.Max(size.y * 0.04f, 7f)), new Vector2(110f, 30f));
		pages[0].subObjects.Add(okButton);
		pages[0].subObjects.Add(cancelButton);
	}
}
