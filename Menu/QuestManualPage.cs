using System.Text.RegularExpressions;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace Menu;

public class QuestManualPage : ManualPage
{
	public FSprite headingSeparator;

	public QuestManualPage(Menu menu, MenuObject owner)
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
		MenuIllustration menuIllustration = new MenuIllustration(menu, owner, "", "manual4", new Vector2(-2f + (menu as ExpeditionManualDialog).contentOffX, 349f), crispPixels: true, anchorCenter: true);
		menuIllustration.sprite.SetAnchor(0f, 0.5f);
		subObjects.Add(menuIllustration);
		string[] array = Regex.Split(menu.Translate("Complete quests to unlock additional content such as perks, burdens and music tracks.<LINE><LINE>'Expedition' quests are completed by undertaking an expedition that meets certain requirements, whilst 'Milestone' quests are completed after reaching a specific goal across multiple expeditions.").WrapText(bigText: true, 570f + (menu as ExpeditionManualDialog).wrapTextMargin), "\n");
		for (int i = 0; i < array.Length; i++)
		{
			MenuLabel menuLabel = new MenuLabel(menu, owner, array[i], new Vector2(295f + (menu as ExpeditionManualDialog).contentOffX, 240f - 25f * (float)i), default(Vector2), bigText: true);
			menuLabel.label.SetAnchor(0.5f, 1f);
			menuLabel.label.color = new Color(0.7f, 0.7f, 0.7f);
			subObjects.Add(menuLabel);
		}
		float num = 0f;
		if (menu.CurrLang == InGameTranslator.LanguageID.German)
		{
			num = 30f;
		}
		string[] array2 = Regex.Split(menu.Translate("Highlight a quest to see all of its requirements and rewards. Try to put together an expedition that completes multiple quests at once!").WrapText(bigText: true, 570f + (menu as ExpeditionManualDialog).wrapTextMargin), "\n");
		for (int j = 0; j < array2.Length; j++)
		{
			MenuLabel menuLabel2 = new MenuLabel(menu, owner, array2[j], new Vector2(295f + (menu as ExpeditionManualDialog).contentOffX, 45f - 25f * (float)j + num), default(Vector2), bigText: true);
			menuLabel2.label.SetAnchor(0.5f, 1f);
			menuLabel2.label.color = new Color(0.7f, 0.7f, 0.7f);
			subObjects.Add(menuLabel2);
		}
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		headingSeparator.x = base.page.pos.x + 295f + (menu as ExpeditionManualDialog).contentOffX;
		headingSeparator.y = base.page.pos.y + 450f;
	}

	public override void RemoveSprites()
	{
		base.RemoveSprites();
		headingSeparator.RemoveFromContainer();
	}
}
