using Menu;

namespace JollyCoop.JollyManual;

internal class Pointing : JollyManualPage
{
	public Pointing(global::Menu.Menu menu, MenuObject owner)
		: base(menu, owner)
	{
		MenuIllustration menuIllustration = new MenuIllustration(menu, owner, "", "manual_jolly_pointing", belowHeaderPosCentered - verticalBuffer, crispPixels: true, anchorCenter: true);
		menuIllustration.sprite.SetAnchor(0.5f, 1f);
		subObjects.Add(menuIllustration);
		string text = TranslateManual("MANUAL_POINTING_1") + " " + TranslateManual("MANUAL_POINTING_2") + " " + TranslateManual("MANUAL_POINTING_3");
		text = text.Replace("\n", " ");
		AddManualText(text, belowHeaderPos.y - menuIllustration.sprite.height - 2f * spaceBuffer);
	}
}
