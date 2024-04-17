using System.Text.RegularExpressions;
using Menu.Remix.MixedUI;
using RWCustom;
using UnityEngine;

namespace Menu;

public class JukeboxManualPage : ManualPage
{
	public FSprite headingSeparator;

	public FSprite repeatSprite;

	public FSprite shuffleSprite;

	public FSprite favouriteSprite;

	public JukeboxManualPage(Menu menu, MenuObject owner)
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
		MenuIllustration menuIllustration = new MenuIllustration(menu, owner, "", "manual6", new Vector2(-2f + (menu as ExpeditionManualDialog).contentOffX, 349f), crispPixels: true, anchorCenter: true);
		menuIllustration.sprite.SetAnchor(0f, 0.5f);
		subObjects.Add(menuIllustration);
		string[] array = Regex.Split(menu.Translate("The Jukebox menu lets you listen to your favourite tracks from Rain World and Rain World: Downpour. All music tracks can be unlocked as rewards for completing quests.").WrapText(bigText: true, 570f + (menu as ExpeditionManualDialog).wrapTextMargin), "\n");
		MenuLabel menuLabel = null;
		FTextParams fTextParams = new FTextParams();
		for (int i = 0; i < array.Length; i++)
		{
			float num = 0f;
			if (menuLabel != null)
			{
				num = menuLabel.label.textRect.height;
			}
			bool bigText = true;
			if (menu.CurrLang == InGameTranslator.LanguageID.Japanese)
			{
				bigText = false;
				fTextParams.lineHeightOffset = -10f;
			}
			MenuLabel menuLabel2 = new MenuLabel(menu, owner, array[i], new Vector2(295f + (menu as ExpeditionManualDialog).contentOffX, 245f - num * (float)i), default(Vector2), bigText, fTextParams);
			menuLabel2.label.SetAnchor(0.5f, 1f);
			menuLabel2.label.color = new Color(0.7f, 0.7f, 0.7f);
			subObjects.Add(menuLabel2);
			menuLabel = menuLabel2;
		}
		float num2 = 0f;
		float num3 = 0f;
		if (menu.CurrLang != InGameTranslator.LanguageID.Japanese)
		{
			num2 = ((!(menu.CurrLang == InGameTranslator.LanguageID.German) && !(menu.CurrLang == InGameTranslator.LanguageID.English) && !(menu.CurrLang == InGameTranslator.LanguageID.Spanish)) ? (-40f) : (-20f));
			num3 = -20f;
		}
		MenuLabel item2 = new MenuLabel(menu, owner, menu.Translate("CONTROLS"), new Vector2(295f + (menu as ExpeditionManualDialog).contentOffX, 135f + num2), default(Vector2), bigText: true)
		{
			label = 
			{
				color = new Color(0.7f, 0.7f, 0.7f)
			}
		};
		subObjects.Add(item2);
		fTextParams = new FTextParams();
		if (InGameTranslator.LanguageID.UsesLargeFont(menu.CurrLang))
		{
			fTextParams.lineHeightOffset = -10f;
		}
		MenuLabel item3 = new MenuLabel(menu, owner, Custom.ReplaceLineDelimeters(menu.Translate("Repeats the currently selected track")), new Vector2(145f + (menu as ExpeditionManualDialog).contentOffX, 90f + num3), default(Vector2), bigText: false, fTextParams)
		{
			label = 
			{
				color = new Color(0.7f, 0.7f, 0.7f)
			}
		};
		subObjects.Add(item3);
		repeatSprite = new FSprite("mediarepeat");
		repeatSprite.color = new Color(0.7f, 0.7f, 0.7f);
		Container.AddChild(repeatSprite);
		MenuLabel item4 = new MenuLabel(menu, owner, Custom.ReplaceLineDelimeters(menu.Translate("Plays unlocked tracks at random")), new Vector2(445f + (menu as ExpeditionManualDialog).contentOffX, 90f + num3), default(Vector2), bigText: false, fTextParams)
		{
			label = 
			{
				color = new Color(0.7f, 0.7f, 0.7f)
			}
		};
		subObjects.Add(item4);
		shuffleSprite = new FSprite("mediashuffle");
		shuffleSprite.color = new Color(0.7f, 0.7f, 0.7f);
		Container.AddChild(shuffleSprite);
		MenuLabel item5 = new MenuLabel(menu, owner, Custom.ReplaceLineDelimeters(menu.Translate("Set a track to play as the menu theme")), new Vector2(295f + (menu as ExpeditionManualDialog).contentOffX, -20f), default(Vector2), bigText: false, fTextParams)
		{
			label = 
			{
				color = new Color(0.7f, 0.7f, 0.7f)
			}
		};
		subObjects.Add(item5);
		favouriteSprite = new FSprite("mediafav");
		favouriteSprite.color = new Color(0.7f, 0.7f, 0.7f);
		Container.AddChild(favouriteSprite);
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		float num = 0f;
		if (menu.CurrLang != InGameTranslator.LanguageID.Japanese)
		{
			num = -20f;
		}
		headingSeparator.x = base.page.pos.x + 295f + (menu as ExpeditionManualDialog).contentOffX;
		headingSeparator.y = base.page.pos.y + 450f;
		repeatSprite.x = base.page.pos.x + 145f + (menu as ExpeditionManualDialog).contentOffX;
		repeatSprite.y = base.page.pos.y + 40f + num;
		shuffleSprite.x = base.page.pos.x + 445f + (menu as ExpeditionManualDialog).contentOffX;
		shuffleSprite.y = base.page.pos.y + 40f + num;
		favouriteSprite.x = base.page.pos.x + 295f + (menu as ExpeditionManualDialog).contentOffX;
		favouriteSprite.y = base.page.pos.y + 40f + num;
	}

	public override void RemoveSprites()
	{
		base.RemoveSprites();
		headingSeparator.RemoveFromContainer();
		repeatSprite.RemoveFromContainer();
		shuffleSprite.RemoveFromContainer();
		favouriteSprite.RemoveFromContainer();
	}
}
