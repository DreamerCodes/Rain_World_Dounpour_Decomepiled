using Menu;

namespace JollyCoop.JollyManual;

public class DifficultiesPage : JollyManualPage
{
	public DifficultiesPage(global::Menu.Menu menu, MenuObject owner)
		: base(menu, owner)
	{
		MenuIllustration menuIllustration = new MenuIllustration(menu, owner, "", "manual_jolly_difficulties", belowHeaderPosCentered - verticalBuffer, crispPixels: true, anchorCenter: true);
		menuIllustration.sprite.SetAnchor(0.5f, 1f);
		subObjects.Add(menuIllustration);
		float startY = AddManualText(menu.Translate("MANUAL_DIFFICULTY_1"), belowHeaderPos.y - menuIllustration.sprite.height - spaceBuffer * 2f);
		string text = menu.Translate("MANUAL_DIFFICULTY_2") + "\n" + menu.Translate("MANUAL_DIFFICULTY_3") + "\n" + menu.Translate("MANUAL_DIFFICULTY_4");
		AddManualText(text, startY, bigText: false, centered: false, (float)JollyManualPage.rectWidth * 0.6f);
	}
}
