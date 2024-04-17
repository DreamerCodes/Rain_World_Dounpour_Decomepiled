using System.Collections.Generic;
using UnityEngine;

namespace Menu;

public class DialogBoxAsyncWait : MenuDialogBox
{
	private AtlasAnimator loadingSpinner;

	public DialogBoxAsyncWait(Menu menu, MenuObject owner, string text, Vector2 pos, Vector2 size, bool forceWrapping = false)
		: base(menu, owner, text, pos, size, forceWrapping)
	{
		loadingSpinner = new AtlasAnimator(0, new Vector2((float)(int)(pos.x + size.x / 2f) - Menu.HorizontalMoveToGetCentered(menu.manager), (int)(pos.y + size.y / 2f - 32f)), "sleep", "sleep", 20, loop: true, reverse: false);
		loadingSpinner.animSpeed = 0.25f;
		loadingSpinner.specificSpeeds = new Dictionary<int, float>();
		loadingSpinner.specificSpeeds[1] = 0.0125f;
		loadingSpinner.specificSpeeds[13] = 0.0125f;
		loadingSpinner.AddToContainer(owner.Container);
	}

	public override void Update()
	{
		base.Update();
		loadingSpinner.Update();
	}

	public override void RemoveSprites()
	{
		base.RemoveSprites();
		loadingSpinner.RemoveFromContainer();
	}

	public void SetText(string caption)
	{
		descriptionLabel.text = caption;
	}
}
