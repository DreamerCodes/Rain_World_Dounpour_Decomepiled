using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Expedition;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace Menu;

public class BurdenManualPage : ManualPage
{
	public FSprite headingSeparator;

	public int pageNumber;

	public BurdenManualPage(Menu menu, MenuObject owner)
		: this(menu, owner, 0)
	{
	}

	public BurdenManualPage(Menu menu, MenuObject owner, int pageNumber)
		: base(menu, owner)
	{
		this.pageNumber = pageNumber;
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
		if (pageNumber == 0)
		{
			string[] array = Regex.Split(menu.Translate("Burdens are modifiers you can add to your Expedition to increase the difficulty. In return, a multiplier is applied to your final score, increasing the number of points it's possible to earn.").WrapText(bigText: true, 570f + (menu as ExpeditionManualDialog).wrapTextMargin), "\n");
			for (int i = 0; i < array.Length; i++)
			{
				MenuLabel menuLabel = new MenuLabel(menu, owner, array[i], new Vector2(295f + (menu as ExpeditionManualDialog).contentOffX, 440f - 25f * (float)i), default(Vector2), bigText: true);
				menuLabel.label.SetAnchor(0.5f, 1f);
				menuLabel.label.color = new Color(0.7f, 0.7f, 0.7f);
				subObjects.Add(menuLabel);
			}
		}
		float num = 0f;
		if (menu.CurrLang == InGameTranslator.LanguageID.German)
		{
			num = -15f;
		}
		List<string> list = ExpeditionProgression.burdenGroups.SelectMany((KeyValuePair<string, List<string>> x) => x.Value).ToList();
		int num2 = Mathf.Min(list.Count - pageNumber * 4, 4);
		for (int j = 0; j < num2; j++)
		{
			string key = list[pageNumber * 4 + j];
			MenuLabel menuLabel2 = new MenuLabel(menu, owner, ExpeditionProgression.BurdenName(key) + " +" + ExpeditionProgression.BurdenScoreMultiplier(key) + "%", new Vector2(35f + (menu as ExpeditionManualDialog).contentOffX, ((pageNumber == 0) ? (300f - (float)j * 90f) : (400f - (float)(120 * j))) + num), default(Vector2), bigText: true)
			{
				label = 
				{
					alignment = FLabelAlignment.Left,
					color = ExpeditionProgression.BurdenMenuColor(key)
				}
			};
			subObjects.Add(menuLabel2);
			string[] array2 = Regex.Split(ExpeditionProgression.BurdenManualDescription(key).WrapText(bigText: false, 500f + (menu as ExpeditionManualDialog).wrapTextMargin), "\n");
			for (int k = 0; k < array2.Length; k++)
			{
				MenuLabel menuLabel3 = new MenuLabel(menu, owner, array2[k], new Vector2(35f + (menu as ExpeditionManualDialog).contentOffX, menuLabel2.pos.y - 15f - 15f * (float)k + num), default(Vector2), bigText: false);
				menuLabel3.label.SetAnchor(0f, 1f);
				menuLabel3.label.color = new Color(0.7f, 0.7f, 0.7f);
				subObjects.Add(menuLabel3);
			}
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
