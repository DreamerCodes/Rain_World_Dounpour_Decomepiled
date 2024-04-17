using System.Text.RegularExpressions;
using Expedition;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace Menu;

public class PerkManualPageTwo : ManualPage
{
	public FSprite headingSeparator;

	public FSprite[] sprites;

	public PerkManualPageTwo(Menu menu, MenuObject owner)
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
		sprites = new FSprite[4];
		for (int i = 0; i < sprites.Length; i++)
		{
			string key = ExpeditionProgression.perkGroups["expedition"][4 + i];
			sprites[i] = new FSprite(ExpeditionProgression.UnlockSprite(key, alwaysShow: true));
			sprites[i].color = Color.Lerp(ExpeditionProgression.UnlockColor(key), new Color(0.8f, 0.8f, 0.8f), 0.25f);
			Container.AddChild(sprites[i]);
			MenuLabel item2 = new MenuLabel(menu, owner, ExpeditionProgression.UnlockName(key), new Vector2(120f + (menu as ExpeditionManualDialog).contentOffX, 410f - (float)(120 * i)), default(Vector2), bigText: true)
			{
				label = 
				{
					color = sprites[i].color,
					alignment = FLabelAlignment.Left
				}
			};
			subObjects.Add(item2);
			string[] array = Regex.Split((menu as ExpeditionManualDialog).PerkManualDescription(key).WrapText(bigText: false, 450f + (menu as ExpeditionManualDialog).wrapTextMargin), "\n");
			for (int j = 0; j < array.Length; j++)
			{
				MenuLabel menuLabel = new MenuLabel(menu, owner, array[j], new Vector2(120f + (menu as ExpeditionManualDialog).contentOffX, 395f - 120f * (float)i - 15f * (float)j), default(Vector2), bigText: false);
				menuLabel.label.SetAnchor(0f, 1f);
				menuLabel.label.color = new Color(0.7f, 0.7f, 0.7f);
				subObjects.Add(menuLabel);
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
			sprites[i].y = base.page.pos.y + 390f - 120f * (float)i;
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
