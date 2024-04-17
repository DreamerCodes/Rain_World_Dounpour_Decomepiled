using Menu;

namespace JollyCoop.JollyManual;

internal class Piggybacking : JollyManualPage
{
	public Piggybacking(global::Menu.Menu menu, MenuObject owner)
		: base(menu, owner)
	{
		MenuIllustration menuIllustration = new MenuIllustration(menu, owner, "", "manual_jolly_piggybacking", belowHeaderPosCentered - verticalBuffer, crispPixels: true, anchorCenter: true);
		menuIllustration.sprite.SetAnchor(0.5f, 1f);
		subObjects.Add(menuIllustration);
		AddManualText(TranslateManual("MANUAL_PIGGYBACKING_1") + "\n\n" + TranslateManual("MANUAL_PIGGYBACKING_2"), belowHeaderPos.y - menuIllustration.sprite.height - 2f * spaceBuffer);
	}
}
