using UnityEngine;

namespace Menu;

public class ControlsButton : SimpleButton
{
	public MenuIllustration deviceImage;

	public ControlsButton(Menu menu, MenuObject owner, Vector2 pos, string displayName)
		: base(menu, owner, displayName, "INPUT", pos, new Vector2(110f, 80f))
	{
		roundedRect = new RoundedRect(menu, this, new Vector2(0f, 0f), size, filled: true);
		subObjects.Add(roundedRect);
		selectRect = new RoundedRect(menu, this, new Vector2(0f, 0f), size, filled: false);
		subObjects.Add(selectRect);
		deviceImage = new MenuIllustration(menu, this, "", "GamepadIcon", size / 2f, crispPixels: true, anchorCenter: true);
		subObjects.Add(deviceImage);
		menuLabel.pos.y -= 50f;
	}

	public override void Update()
	{
		base.Update();
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		deviceImage.color = MyColor(timeStacker);
		menuLabel.label.color = MyColor(timeStacker);
	}
}
