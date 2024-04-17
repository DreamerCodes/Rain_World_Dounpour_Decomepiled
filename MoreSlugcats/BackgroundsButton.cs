using Menu;
using UnityEngine;

namespace MoreSlugcats;

public class BackgroundsButton : SimpleButton
{
	public MenuIllustration buttonImage;

	private float extraSelectCol;

	private float lastExtraSelectCol;

	private string displayName;

	public BackgroundsButton(global::Menu.Menu menu, MenuObject owner, Vector2 pos, string displayName)
		: base(menu, owner, displayName, "BACKGROUND", pos, new Vector2(110f, 80f))
	{
		this.displayName = displayName;
		roundedRect = new RoundedRect(menu, this, new Vector2(0f, 0f), size, filled: true);
		subObjects.Add(roundedRect);
		selectRect = new RoundedRect(menu, this, new Vector2(0f, 0f), size, filled: false);
		subObjects.Add(selectRect);
		buttonImage = new MenuIllustration(menu, this, string.Empty, "BackgroundIcon", size / 2f, crispPixels: true, anchorCenter: true);
		subObjects.Add(buttonImage);
		MenuLabel menuLabel = base.menuLabel;
		menuLabel.pos.y = menuLabel.pos.y - 50f;
	}

	public override void Update()
	{
		base.Update();
		lastExtraSelectCol = extraSelectCol;
	}

	public override void GrafUpdate(float timeStacker)
	{
		Mathf.Lerp(lastExtraSelectCol, extraSelectCol, timeStacker);
		Mathf.Lerp(buttonBehav.lastCol, buttonBehav.col, timeStacker);
		base.GrafUpdate(timeStacker);
		buttonImage.color = MyColor(timeStacker);
		menuLabel.label.color = MyColor(timeStacker);
	}
}
