using System.Text.RegularExpressions;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace Menu;

public class ChallengeControlsManualPage : ManualPage
{
	public FSprite headingSeparator;

	public FSprite hiddenSprite;

	public FSprite minusSprite;

	public FSprite plusSprite;

	public FSprite randomSprite;

	public FSprite filterSprite;

	public ChallengeControlsManualPage(Menu menu, MenuObject owner)
		: base(menu, owner)
	{
		topicName = menu.Translate((menu as ExpeditionManualDialog).TopicName((menu as ExpeditionManualDialog).currentTopic));
		MenuLabel item = new MenuLabel(menu, owner, topicName, new Vector2(15f + (menu as ExpeditionManualDialog).contentOffX, 475f), default(Vector2), bigText: true)
		{
			label = 
			{
				alignment = FLabelAlignment.Left
			}
		};
		subObjects.Add(item);
		headingSeparator = new FSprite("pixel");
		headingSeparator.scaleX = 594f;
		headingSeparator.scaleY = 2f;
		headingSeparator.color = new Color(0.7f, 0.7f, 0.7f);
		Container.AddChild(headingSeparator);
		hiddenSprite = new FSprite("hiddenopen");
		hiddenSprite.color = new Color(0.7f, 0.7f, 0.7f);
		Container.AddChild(hiddenSprite);
		minusSprite = new FSprite("minus");
		minusSprite.color = new Color(0.7f, 0.7f, 0.7f);
		Container.AddChild(minusSprite);
		plusSprite = new FSprite("plus");
		plusSprite.color = new Color(0.7f, 0.7f, 0.7f);
		Container.AddChild(plusSprite);
		randomSprite = new FSprite("Sandbox_Randomize");
		randomSprite.color = new Color(0.7f, 0.7f, 0.7f);
		Container.AddChild(randomSprite);
		filterSprite = new FSprite("filter");
		filterSprite.color = new Color(0.7f, 0.7f, 0.7f);
		Container.AddChild(filterSprite);
		string[] array = Regex.Split(menu.Translate("Toggles whether or not the selected challenge is hidden. Hidden challenges are completely random and their requirements are only revealed once all regular challenges have been completed. Hidden challenges are worth twice as many points. An expedition must have at least one non-hidden challenge.").WrapText(bigText: false, 450f + (menu as ExpeditionManualDialog).wrapTextMargin), "\n");
		for (int i = 0; i < array.Length; i++)
		{
			MenuLabel menuLabel = new MenuLabel(menu, owner, array[i], new Vector2(120f + (menu as ExpeditionManualDialog).contentOffX, 420f - 15f * (float)i), default(Vector2), bigText: false);
			menuLabel.label.SetAnchor(0f, 0.5f);
			menuLabel.label.color = new Color(0.7f, 0.7f, 0.7f);
			subObjects.Add(menuLabel);
		}
		float num = 0f;
		if (menu.CurrLang == InGameTranslator.LanguageID.German)
		{
			num = -30f;
		}
		string[] array2 = Regex.Split(menu.Translate("Controls how many challenges the expedition will have, up to a maximum of five. A point bonus is applied for multiple challenges.").WrapText(bigText: false, 450f + (menu as ExpeditionManualDialog).wrapTextMargin), "\n");
		for (int j = 0; j < array2.Length; j++)
		{
			MenuLabel menuLabel2 = new MenuLabel(menu, owner, array2[j], new Vector2(120f + (menu as ExpeditionManualDialog).contentOffX, 330f - 15f * (float)j + num), default(Vector2), bigText: false);
			menuLabel2.label.SetAnchor(0f, 0.5f);
			menuLabel2.label.color = new Color(0.7f, 0.7f, 0.7f);
			subObjects.Add(menuLabel2);
		}
		string[] array3 = Regex.Split(menu.Translate("Randomizes all challenges at once. Can only be used if no challenge filters are applied.").WrapText(bigText: false, 450f + (menu as ExpeditionManualDialog).wrapTextMargin), "\n");
		for (int k = 0; k < array3.Length; k++)
		{
			MenuLabel menuLabel3 = new MenuLabel(menu, owner, array3[k], new Vector2(120f + (menu as ExpeditionManualDialog).contentOffX, 250f - 15f * (float)k), default(Vector2), bigText: false);
			menuLabel3.label.SetAnchor(0f, 0.5f);
			menuLabel3.label.color = new Color(0.7f, 0.7f, 0.7f);
			subObjects.Add(menuLabel3);
		}
		string[] array4 = Regex.Split(menu.Translate("Opens the challenge filter menu, allowing you to toggle which types of challenges can appear when re-rolling.").WrapText(bigText: false, 450f + (menu as ExpeditionManualDialog).wrapTextMargin), "\n");
		for (int l = 0; l < array4.Length; l++)
		{
			MenuLabel menuLabel4 = new MenuLabel(menu, owner, array4[l], new Vector2(120f + (menu as ExpeditionManualDialog).contentOffX, 170f - 15f * (float)l), default(Vector2), bigText: false);
			menuLabel4.label.SetAnchor(0f, 0.5f);
			menuLabel4.label.color = new Color(0.7f, 0.7f, 0.7f);
			subObjects.Add(menuLabel4);
		}
		MenuIllustration menuIllustration = new MenuIllustration(menu, owner, "", "manual3", new Vector2(-6f + (menu as ExpeditionManualDialog).contentOffX, 60f), crispPixels: true, anchorCenter: true);
		menuIllustration.sprite.SetAnchor(0f, 0.5f);
		subObjects.Add(menuIllustration);
		string[] array5 = Regex.Split(menu.Translate("Indicates your progress to the next level and how much the current expedition will award. If the bar blinks rapidly then completing the expedition will grant a level. The number at the end of the bar displays how many levels will granted.").WrapText(bigText: false, 490f + (menu as ExpeditionManualDialog).wrapTextMargin), "\n");
		for (int m = 0; m < array5.Length; m++)
		{
			MenuLabel menuLabel5 = new MenuLabel(menu, owner, array5[m], new Vector2(295f + (menu as ExpeditionManualDialog).contentOffX, 15f - 15f * (float)m), default(Vector2), bigText: false);
			menuLabel5.label.SetAnchor(0.5f, 0.5f);
			menuLabel5.label.color = new Color(0.7f, 0.7f, 0.7f);
			subObjects.Add(menuLabel5);
		}
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		headingSeparator.x = base.page.pos.x + 295f + (menu as ExpeditionManualDialog).contentOffX;
		headingSeparator.y = base.page.pos.y + 450f;
		hiddenSprite.x = base.page.pos.x + 60f + (menu as ExpeditionManualDialog).contentOffX;
		hiddenSprite.y = base.page.pos.y + 400f;
		float num = 0f;
		if (menu.CurrLang == InGameTranslator.LanguageID.German)
		{
			num = -30f;
		}
		minusSprite.x = base.page.pos.x + 75f + (menu as ExpeditionManualDialog).contentOffX;
		minusSprite.y = base.page.pos.y + 320f + num;
		plusSprite.x = base.page.pos.x + 45f + (menu as ExpeditionManualDialog).contentOffX;
		plusSprite.y = base.page.pos.y + 320f + num;
		randomSprite.x = base.page.pos.x + 60f + (menu as ExpeditionManualDialog).contentOffX;
		randomSprite.y = base.page.pos.y + 240f;
		filterSprite.x = base.page.pos.x + 60f + (menu as ExpeditionManualDialog).contentOffX;
		filterSprite.y = base.page.pos.y + 160f;
	}

	public override void RemoveSprites()
	{
		base.RemoveSprites();
		headingSeparator.RemoveFromContainer();
		hiddenSprite.RemoveFromContainer();
		minusSprite.RemoveFromContainer();
		plusSprite.RemoveFromContainer();
		randomSprite.RemoveFromContainer();
		filterSprite.RemoveFromContainer();
	}
}
