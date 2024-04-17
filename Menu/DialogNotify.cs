using System;
using UnityEngine;

namespace Menu;

public class DialogNotify : Dialog
{
	protected SimpleButton okButton;

	private Action onOK;

	public float timeOut;

	public DialogNotify(string description, ProcessManager manager, bool longLabel = false)
		: base(description, manager, longLabel)
	{
	}

	public DialogNotify(string description, Vector2 size, ProcessManager manager)
		: base(description, size, manager)
	{
	}

	public DialogNotify(string description, ProcessManager manager, Action onOK)
		: base(description, manager)
	{
		Init(onOK);
	}

	public DialogNotify(string description, Vector2 size, ProcessManager manager, Action onOK)
		: base(description, size, manager)
	{
		Init(onOK);
	}

	public DialogNotify(string longDescription, string title, Vector2 size, ProcessManager manager, Action onOK, bool longLabel = false)
		: base(longDescription, title, size, manager, longLabel)
	{
		Init(onOK);
	}

	public override void Update()
	{
		base.Update();
		if (okButton != null)
		{
			timeOut -= 0.025f;
			if (timeOut < 0f)
			{
				timeOut = 0f;
				okButton.buttonBehav.greyedOut = false;
			}
			else
			{
				okButton.buttonBehav.greyedOut = true;
			}
		}
	}

	public override void Singal(MenuObject sender, string message)
	{
		if (message != null && message == "OK")
		{
			if (onOK != null)
			{
				onOK();
			}
			manager.StopSideProcess(this);
		}
	}

	private void Init(Action onOK)
	{
		this.onOK = onOK;
		okButton = new SimpleButton(this, pages[0], Translate("ok"), "OK", new Vector2(pos.x + (size.x - 110f) * 0.5f, pos.y + Mathf.Max(size.y * 0.04f, 7f)), new Vector2(110f, 30f));
		pages[0].subObjects.Add(okButton);
	}
}
