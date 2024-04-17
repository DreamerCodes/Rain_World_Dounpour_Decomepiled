using Menu.Remix.MixedUI;
using RWCustom;
using UnityEngine;

namespace Menu;

public class DialogBoxNotify : MenuDialogBox
{
	public SimpleButton continueButton;

	public float timeOut = 1f;

	public DialogBoxNotify(Menu menu, MenuObject owner, string text, string signalText, Vector2 pos, Vector2 size, bool forceWrapping = false)
		: base(menu, owner, text, pos, size, forceWrapping)
	{
		continueButton = new SimpleButton(menu, owner, menu.Translate("CONTINUE"), signalText, new Vector2((int)(pos.x + size.x / 2f - 55f), (int)(pos.y + 20f)), new Vector2(110f, 30f));
		owner.subObjects.Add(continueButton);
		base.page.selectables.Add(continueButton);
		for (int i = 0; i < 4; i++)
		{
			continueButton.nextSelectable[i] = continueButton;
		}
		menu.selectedObject = continueButton;
		base.page.lastSelectedObject = continueButton;
		continueButton.buttonBehav.greyedOut = true;
	}

	public override void RemoveSprites()
	{
		base.RemoveSprites();
		continueButton.RemoveSprites();
		owner.subObjects.Remove(continueButton);
		while (base.page.selectables.Contains(continueButton))
		{
			base.page.selectables.Remove(continueButton);
		}
		menu.selectedObject = null;
		base.page.lastSelectedObject = null;
	}

	public override void Update()
	{
		base.Update();
		timeOut -= 0.025f;
		if (timeOut < 0f)
		{
			timeOut = 0f;
			continueButton.buttonBehav.greyedOut = false;
		}
	}

	public static Vector2 CalculateDialogBoxSize(string displayText, bool dialogUsesWordWrapping = true)
	{
		string text = Custom.ReplaceWordWrapLineDelimeters(displayText).Replace("\r\n", "\n");
		float num = Mathf.Max(LabelTest.GetWidth(text) + 44f, 200f);
		if (dialogUsesWordWrapping)
		{
			text = displayText.WrapText(bigText: false, num, forceWrapping: true);
		}
		float num2 = LabelTest.LineHeight(bigText: false);
		if (InGameTranslator.LanguageID.UsesLargeFont(Custom.rainWorld.inGameTranslator.currentLanguage))
		{
			num2 *= 2f;
		}
		return new Vector2(num, Mathf.Max(num2 * (float)text.Split('\n').Length + 100f, 120f));
	}
}
