using Menu;

namespace JollyCoop.JollyManual;

public class IntroductionPage : JollyManualPage
{
	public IntroductionPage(global::Menu.Menu menu, MenuObject owner)
		: base(menu, owner)
	{
		MenuIllustration menuIllustration = new MenuIllustration(menu, owner, "", "manual_jolly_introduction", belowHeaderPos, crispPixels: true, anchorCenter: true);
		menuIllustration.sprite.SetAnchor(0f, 1f);
		subObjects.Add(menuIllustration);
		_ = menu.CurrLang == InGameTranslator.LanguageID.Russian;
		AddManualText(menu.Translate("MANUAL_INTRODUCTION_1"), belowHeaderPos.y - 50f - menuIllustration.sprite.height + 20f);
	}
}
