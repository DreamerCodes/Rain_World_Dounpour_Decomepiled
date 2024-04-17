using Menu.Remix.MixedUI;
using UnityEngine;

namespace Menu;

public abstract class MenuDialogBox : RectangularMenuObject
{
	private FSprite darkSprite;

	private RoundedRect roundedRect;

	protected MenuLabel descriptionLabel;

	public MenuDialogBox(Menu menu, MenuObject owner, string text, Vector2 pos, Vector2 size, bool forceWrapping = false)
		: base(menu, owner, pos, size)
	{
		darkSprite = new FSprite("pixel");
		darkSprite.color = new Color(0f, 0f, 0f);
		darkSprite.anchorX = 0f;
		darkSprite.anchorY = 0f;
		darkSprite.scaleX = 1368f;
		darkSprite.scaleY = 770f;
		darkSprite.x = -1f;
		darkSprite.y = -1f;
		darkSprite.alpha = 0.75f;
		owner.Container.AddChild(darkSprite);
		roundedRect = new RoundedRect(menu, owner, new Vector2(pos.x, pos.y), new Vector2(size.x, size.y), filled: true);
		roundedRect.fillAlpha = 0.95f;
		owner.subObjects.Add(roundedRect);
		string text2 = text.WrapText(bigText: false, size.x - 40f, forceWrapping);
		descriptionLabel = new MenuLabel(menu, owner, text2, new Vector2(pos.x + size.x * 0.07f, pos.y + 30f + size.y * 0.08f), new Vector2(size.x * 0.86f, size.y * 0.88f - 30f), bigText: false);
		owner.subObjects.Add(descriptionLabel);
	}

	public override void RemoveSprites()
	{
		base.RemoveSprites();
		roundedRect.RemoveSprites();
		descriptionLabel.RemoveSprites();
		owner.subObjects.Remove(roundedRect);
		owner.subObjects.Remove(descriptionLabel);
		darkSprite.RemoveFromContainer();
	}
}
