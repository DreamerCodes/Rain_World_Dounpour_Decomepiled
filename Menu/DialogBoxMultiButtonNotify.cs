using Menu.Remix.MixedUI;
using RWCustom;
using UnityEngine;

namespace Menu;

public class DialogBoxMultiButtonNotify : MenuDialogBox
{
	public SimpleButton[] buttons;

	public float timeOut = 1f;

	public DialogBoxMultiButtonNotify(Menu menu, MenuObject owner, string text, string[] signalTexts, string[] buttonTexts, Vector2 pos, Vector2 size, bool forceWrapping = false)
		: base(menu, owner, text, pos, size, forceWrapping)
	{
		buttons = new SimpleButton[buttonTexts.Length];
		for (int i = 0; i < buttonTexts.Length; i++)
		{
			int num = buttonTexts.Length - 1 - i;
			buttons[i] = new SimpleButton(menu, owner, buttonTexts[i], signalTexts[i], new Vector2((int)(pos.x + size.x / 2f - 55f), (int)(pos.y + 20f + 35f * (float)num)), new Vector2(110f, 30f));
			owner.subObjects.Add(buttons[i]);
			base.page.selectables.Add(buttons[i]);
			buttons[i].buttonBehav.greyedOut = true;
		}
		for (int j = 0; j < 4; j++)
		{
			for (int k = 0; k < buttons.Length; k++)
			{
				if (j == 3 && k != buttons.Length - 1)
				{
					buttons[k].nextSelectable[j] = buttons[k + 1];
				}
				else if (j == 1 && k != 0)
				{
					buttons[k].nextSelectable[j] = buttons[k - 1];
				}
				else
				{
					buttons[k].nextSelectable[j] = buttons[k];
				}
			}
		}
		menu.selectedObject = buttons[0];
		base.page.lastSelectedObject = buttons[0];
		float num2 = 35f * (float)buttonTexts.Length;
		descriptionLabel.size.y = size.y * 0.88f - num2;
		descriptionLabel.pos.y = pos.y + num2 + size.y * 0.08f;
	}

	public override void RemoveSprites()
	{
		base.RemoveSprites();
		for (int i = 0; i < buttons.Length; i++)
		{
			buttons[i].RemoveSprites();
			owner.subObjects.Remove(buttons[i]);
			while (base.page.selectables.Contains(buttons[i]))
			{
				base.page.selectables.Remove(buttons[i]);
			}
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
			for (int i = 0; i < buttons.Length; i++)
			{
				buttons[i].buttonBehav.greyedOut = false;
			}
		}
	}

	public static Vector2 CalculateDialogBoxSize(string displayText, int buttons, bool dialogUsesWordWrapping = true)
	{
		string text = Custom.ReplaceWordWrapLineDelimeters(displayText).Replace("\r\n", "\n");
		float num = Mathf.Clamp(LabelTest.GetWidth(text) + 44f, 200f, 600f);
		if (dialogUsesWordWrapping)
		{
			text = displayText.WrapText(bigText: false, num, forceWrapping: true);
		}
		float num2 = LabelTest.LineHeight(bigText: false);
		if (InGameTranslator.LanguageID.UsesLargeFont(Custom.rainWorld.inGameTranslator.currentLanguage))
		{
			num2 *= 2f;
		}
		return new Vector2(num, Mathf.Max(num2 * (float)text.Split('\n').Length + 65f, 85f) + 35f * (float)buttons);
	}
}
