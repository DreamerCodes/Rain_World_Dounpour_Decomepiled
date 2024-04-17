using Menu;

namespace JollyCoop.JollyManual;

internal class SelectingSlugcat : JollyManualPage
{
	public SelectingSlugcat(global::Menu.Menu menu, MenuObject owner)
		: base(menu, owner)
	{
		MenuIllustration menuIllustration = new MenuIllustration(menu, owner, "", "manual_jolly_selecting", belowHeaderPos, crispPixels: true, anchorCenter: true);
		menuIllustration.sprite.SetAnchor(0f, 1f);
		subObjects.Add(menuIllustration);
		float num = 0f;
		if (menu.CurrLang == InGameTranslator.LanguageID.French || menu.CurrLang == InGameTranslator.LanguageID.Spanish)
		{
			num = 20f;
		}
		AddManualText(menu.Translate("MANUAL_SELECTING_1") + "\n\n" + menu.Translate("MANUAL_SELECTING_2"), belowHeaderPos.y - menuIllustration.sprite.height - spaceBuffer + num);
	}
}
