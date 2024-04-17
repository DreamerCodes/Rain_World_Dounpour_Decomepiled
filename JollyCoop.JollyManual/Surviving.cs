using Menu;
using UnityEngine;

namespace JollyCoop.JollyManual;

internal class Surviving : JollyManualPage
{
	public Surviving(global::Menu.Menu menu, MenuObject owner)
		: base(menu, owner)
	{
		MenuIllustration menuIllustration = new MenuIllustration(menu, owner, "", "manual_jolly_surviving1", belowHeaderPosCentered - verticalBuffer, crispPixels: true, anchorCenter: true);
		menuIllustration.sprite.SetAnchor(0.5f, 1f);
		subObjects.Add(menuIllustration);
		float num = AddManualText(menu.Translate("MANUAL_SURVIVING_1"), belowHeaderPos.y - spaceBuffer * 2f - menuIllustration.sprite.height);
		MenuIllustration item = new MenuIllustration(menu, owner, "", "manual_jolly_surviving2", new Vector2(belowHeaderPosCentered.x, num - spaceBuffer * 2f), crispPixels: true, anchorCenter: true);
		menuIllustration.sprite.SetAnchor(0.5f, 1f);
		subObjects.Add(item);
	}
}
