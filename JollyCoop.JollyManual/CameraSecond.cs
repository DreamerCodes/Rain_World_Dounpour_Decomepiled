using Menu;
using UnityEngine;

namespace JollyCoop.JollyManual;

internal class CameraSecond : JollyManualPage
{
	public CameraSecond(global::Menu.Menu menu, MenuObject owner)
		: base(menu, owner)
	{
		MenuIllustration menuIllustration = new MenuIllustration(menu, owner, "", "jollymeter_arrow", belowHeaderPosCentered - verticalBuffer, crispPixels: true, anchorCenter: true);
		menuIllustration.sprite.SetAnchor(0.5f, 1f);
		subObjects.Add(menuIllustration);
		float num = AddManualText(TranslateManual("MANUAL_CAMERA_3"), belowHeaderPos.y - menuIllustration.sprite.height - 2f * spaceBuffer);
		MenuIllustration menuIllustration2 = new MenuIllustration(menu, owner, "", "jollymeter_lock", new Vector2(belowHeaderPosCentered.x, num - spaceBuffer), crispPixels: true, anchorCenter: true);
		menuIllustration.sprite.SetAnchor(0.5f, 1f);
		subObjects.Add(menuIllustration2);
		float num2 = 0f;
		if (menu.CurrLang == InGameTranslator.LanguageID.German)
		{
			num2 = 20f;
		}
		num = AddManualText(TranslateManual("MANUAL_CAMERA_4") + "\n" + TranslateManual("MANUAL_CAMERA_5"), num - 2f * spaceBuffer - menuIllustration2.sprite.height + num2);
	}
}
