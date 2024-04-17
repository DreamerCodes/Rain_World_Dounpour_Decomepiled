using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Expedition;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace Menu;

public class PerkManualPage : ManualPage
{
	public FSprite headingSeparator;

	public FSprite[] sprites;

	public int pageNumber;

	public PerkManualPage(Menu menu, MenuObject owner)
		: this(menu, owner, 0)
	{
	}

	public PerkManualPage(Menu menu, MenuObject owner, int pageNumber)
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
			string[] array = Regex.Split(menu.Translate("Unlocked via quests, perks can be applied to an expedition to even the odds, granting new tools and abilities for making completing challenges easier. See below for a full list of unlockable perks.").WrapText(bigText: false, 570f + (menu as ExpeditionManualDialog).wrapTextMargin), "\n");
			for (int i = 0; i < array.Length; i++)
			{
				MenuLabel menuLabel = new MenuLabel(menu, owner, array[i], new Vector2(295f + (menu as ExpeditionManualDialog).contentOffX, 425f - 20f * (float)i), default(Vector2), bigText: false);
				menuLabel.label.SetAnchor(0.5f, 1f);
				menuLabel.label.color = new Color(0.7f, 0.7f, 0.7f);
				subObjects.Add(menuLabel);
			}
		}
		List<string> list = ExpeditionProgression.perkGroups.SelectMany((KeyValuePair<string, List<string>> x) => x.Value).ToList();
		sprites = new FSprite[Mathf.Min(list.Count - pageNumber * 4, 4)];
		for (int j = 0; j < sprites.Length; j++)
		{
			string key = list[pageNumber * 4 + j];
			sprites[j] = new FSprite(ExpeditionProgression.UnlockSprite(key, alwaysShow: true));
			sprites[j].color = Color.Lerp(ExpeditionProgression.UnlockColor(key), new Color(0.8f, 0.8f, 0.8f), 0.25f);
			Container.AddChild(sprites[j]);
			MenuLabel item2 = new MenuLabel(menu, owner, ExpeditionProgression.UnlockName(key), new Vector2(120f + (menu as ExpeditionManualDialog).contentOffX, (pageNumber == 0) ? (310f - (float)(90 * j)) : (410f - (float)(120 * j))), default(Vector2), bigText: true)
			{
				label = 
				{
					color = sprites[j].color,
					alignment = FLabelAlignment.Left
				}
			};
			subObjects.Add(item2);
			string[] array2 = Regex.Split((menu as ExpeditionManualDialog).PerkManualDescription(key).WrapText(bigText: false, 450f + (menu as ExpeditionManualDialog).wrapTextMargin), "\n");
			for (int k = 0; k < array2.Length; k++)
			{
				MenuLabel menuLabel2 = new MenuLabel(menu, owner, array2[k], new Vector2(120f + (menu as ExpeditionManualDialog).contentOffX, ((pageNumber == 0) ? (295f - 90f * (float)j) : (395f - 120f * (float)j)) - 15f * (float)k), default(Vector2), bigText: false);
				menuLabel2.label.SetAnchor(0f, 1f);
				menuLabel2.label.color = new Color(0.7f, 0.7f, 0.7f);
				subObjects.Add(menuLabel2);
			}
		}
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		headingSeparator.x = base.page.pos.x + 295f + (menu as ExpeditionManualDialog).contentOffX;
		headingSeparator.y = base.page.pos.y + 450f;
		for (int i = 0; i < sprites.Length; i++)
		{
			sprites[i].x = base.page.pos.x + 60f + (menu as ExpeditionManualDialog).contentOffX;
			sprites[i].y = base.page.pos.y + ((pageNumber == 0) ? (290f - 90f * (float)i) : (390f - 120f * (float)i));
		}
	}

	public override void RemoveSprites()
	{
		base.RemoveSprites();
		headingSeparator.RemoveFromContainer();
		for (int i = 0; i < sprites.Length; i++)
		{
			sprites[i].RemoveFromContainer();
		}
	}
}
