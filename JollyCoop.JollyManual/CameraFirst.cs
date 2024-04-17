using Menu;

namespace JollyCoop.JollyManual;

internal class CameraFirst : JollyManualPage
{
	public CameraFirst(global::Menu.Menu menu, MenuObject owner)
		: base(menu, owner)
	{
		MenuIllustration menuIllustration = new MenuIllustration(menu, owner, "", "manual_jolly_camera", belowHeaderPosCentered - verticalBuffer, crispPixels: true, anchorCenter: true);
		menuIllustration.sprite.SetAnchor(0.5f, 1f);
		subObjects.Add(menuIllustration);
		AddManualText(TranslateManual("MANUAL_CAMERA_1") + "\n\n" + TranslateManual("MANUAL_CAMERA_2"), belowHeaderPos.y - menuIllustration.sprite.height - spaceBuffer * 2f);
	}
}
