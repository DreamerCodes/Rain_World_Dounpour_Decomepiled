using UnityEngine;

namespace Menu;

public class ArenaSettingsInterface : PositionedMenuObject, CheckBox.IOwnCheckBox, MultipleChoiceArray.IOwnMultipleChoiceArray, SelectOneButton.SelectOneButtonOwner
{
	public MultipleChoiceArray scoreToEnterDen;

	public MultipleChoiceArray rainTimer;

	public FSprite divSprite;

	private Vector2 divSpritePos;

	public SelectOneButton[] enterDenReqs;

	public CheckBox earlyRainCheckbox;

	public CheckBox spearsHitCheckbox;

	public CheckBox evilAICheckBox;

	public CheckBox levelItemsCheckbox;

	public CheckBox levelFoodCheckbox;

	public MultipleChoiceArray wildlifeArray;

	public MultiplayerMenu GetMultiplayerMenu => menu as MultiplayerMenu;

	public ArenaSetup GetArenaSetup => menu.manager.arenaSetup;

	public ArenaSetup.GameTypeSetup GetGameTypeSetup => GetArenaSetup.GetOrInitiateGameTypeSetup(GetMultiplayerMenu.currentGameType);

	public ArenaSettingsInterface(Menu menu, MenuObject owner)
		: base(menu, owner, new Vector2(0f, 0f))
	{
		Vector2 vector = new Vector2(826.01f, 140.01f);
		float num = 340f;
		bool flag = menu.CurrLang != InGameTranslator.LanguageID.English && menu.CurrLang != InGameTranslator.LanguageID.Korean && menu.CurrLang != InGameTranslator.LanguageID.Chinese;
		if (GetGameTypeSetup.gameType == ArenaSetup.GameTypeID.Competitive)
		{
			spearsHitCheckbox = new CheckBox(menu, this, this, vector + new Vector2(0f, 220f), 120f, menu.Translate("Spears Hit:"), "SPEARSHIT");
			subObjects.Add(spearsHitCheckbox);
			evilAICheckBox = new CheckBox(menu, this, this, vector + new Vector2(num - 24f, 220f), InGameTranslator.LanguageID.UsesLargeFont(menu.CurrLang) ? 140f : 120f, menu.Translate("Aggressive AI:"), "EVILAI");
			subObjects.Add(evilAICheckBox);
			divSprite = new FSprite("pixel");
			divSprite.anchorX = 0f;
			divSprite.scaleX = num;
			divSprite.scaleY = 2f;
			Container.AddChild(divSprite);
			divSprite.color = Menu.MenuRGB(Menu.MenuColors.VeryDarkGrey);
			divSpritePos = vector + new Vector2(0f, 197f);
			MultipleChoiceArray multipleChoiceArray = new MultipleChoiceArray(menu, this, this, vector + new Vector2(0f, 150f), menu.Translate("Repeat Rooms:"), "ROOMREPEAT", InGameTranslator.LanguageID.UsesLargeFont(menu.CurrLang) ? 140f : 120f, num, 5, textInBoxes: true, splitText: false);
			subObjects.Add(multipleChoiceArray);
			for (int i = 0; i < multipleChoiceArray.buttons.Length; i++)
			{
				multipleChoiceArray.buttons[i].label.text = i + 1 + "x";
			}
			rainTimer = new MultipleChoiceArray(menu, this, this, vector + new Vector2(0f, 100f), menu.Translate("Rain Timer:"), "SESSIONLENGTH", InGameTranslator.LanguageID.UsesLargeFont(menu.CurrLang) ? 125f : 120f, num, 6, textInBoxes: false, menu.CurrLang == InGameTranslator.LanguageID.French || menu.CurrLang == InGameTranslator.LanguageID.Spanish || menu.CurrLang == InGameTranslator.LanguageID.Portuguese);
			subObjects.Add(rainTimer);
			wildlifeArray = new MultipleChoiceArray(menu, this, this, vector + new Vector2(0f, 50f), menu.Translate("Wildlife:"), "WILDLIFE", 120f, num, 4, textInBoxes: false, splitText: false);
			subObjects.Add(wildlifeArray);
		}
		else if (GetGameTypeSetup.gameType == ArenaSetup.GameTypeID.Sandbox)
		{
			vector.x += 57f;
			num -= 57f;
			spearsHitCheckbox = new CheckBox(menu, this, this, vector + new Vector2(flag ? 52f : 0f, 245f), flag ? 142f : 90f, menu.Translate("Spears Hit:"), "SPEARSHIT");
			subObjects.Add(spearsHitCheckbox);
			evilAICheckBox = new CheckBox(menu, this, this, vector + new Vector2(num - 24f, 245f), flag ? 142f : 90f, menu.Translate("Aggressive AI:"), "EVILAI");
			subObjects.Add(evilAICheckBox);
			levelItemsCheckbox = new CheckBox(menu, this, this, vector + new Vector2(flag ? 52f : 0f, 215f), flag ? 142f : 90f, menu.Translate("Level Items:"), "LEVELITEMS");
			subObjects.Add(levelItemsCheckbox);
			levelFoodCheckbox = new CheckBox(menu, this, this, vector + new Vector2(num - 24f, 215f), flag ? 142f : 90f, menu.Translate("Level Food:"), "FLIESSPAWN");
			subObjects.Add(levelFoodCheckbox);
			enterDenReqs = new SelectOneButton[3];
			enterDenReqs[0] = new SelectOneButton(menu, this, menu.Translate("SCORE"), "DENENTRYRULE", vector + new Vector2(InGameTranslator.LanguageID.UsesLargeFont(menu.CurrLang) ? (-40f) : 0f, 140f), new Vector2(num / 3f - (InGameTranslator.LanguageID.UsesLargeFont(menu.CurrLang) ? (-10f) : 10f), 24f), enterDenReqs, 0);
			enterDenReqs[1] = new SelectOneButton(menu, this, menu.Translate("STANDARD"), "DENENTRYRULE", vector + new Vector2((InGameTranslator.LanguageID.UsesLargeFont(menu.CurrLang) ? (-20f) : 10f) + num / 3f, 140f), new Vector2(num / 3f - (InGameTranslator.LanguageID.UsesLargeFont(menu.CurrLang) ? (-10f) : 10f), 24f), enterDenReqs, 1);
			enterDenReqs[2] = new SelectOneButton(menu, this, menu.Translate("ALWAYS"), "DENENTRYRULE", vector + new Vector2((InGameTranslator.LanguageID.UsesLargeFont(menu.CurrLang) ? 0f : 20f) + num * 2f / 3f, 140f), new Vector2(num / 3f - (InGameTranslator.LanguageID.UsesLargeFont(menu.CurrLang) ? (-10f) : 10f), 24f), enterDenReqs, 2);
			for (int j = 0; j < enterDenReqs.Length; j++)
			{
				subObjects.Add(enterDenReqs[j]);
			}
			MenuLabel menuLabel = new MenuLabel(menu, this, menu.Translate("Allow exit:"), vector + new Vector2(InGameTranslator.LanguageID.UsesLargeFont(menu.CurrLang) ? (-130f) : (-90f), 95f), new Vector2(0f, 100f), bigText: false);
			if (menu.CurrLang != InGameTranslator.LanguageID.English && menu.CurrLang != InGameTranslator.LanguageID.Spanish && menu.CurrLang != InGameTranslator.LanguageID.Korean && menu.CurrLang != InGameTranslator.LanguageID.Chinese)
			{
				menuLabel.text = InGameTranslator.EvenSplit(menuLabel.text, 1);
				menuLabel.pos.y -= 10f;
			}
			menuLabel.label.color = Menu.MenuRGB(Menu.MenuColors.DarkGrey);
			menuLabel.label.alignment = FLabelAlignment.Left;
			subObjects.Add(menuLabel);
			scoreToEnterDen = new MultipleChoiceArray(menu, this, this, vector + new Vector2(0f, 95f), "", "SCORETOENTERDEN", 90f, num, ArenaSetup.GameTypeSetup.ScoresToEnterDenArray.Length, textInBoxes: true, splitText: false);
			for (int k = 0; k < scoreToEnterDen.buttons.Length; k++)
			{
				scoreToEnterDen.buttons[k].label.text = ArenaSetup.GameTypeSetup.ScoresToEnterDenArray[k].ToString();
			}
			subObjects.Add(scoreToEnterDen);
			rainTimer = new MultipleChoiceArray(menu, this, this, vector + new Vector2(0f, 5f), menu.Translate("Rain Timer:"), "SESSIONLENGTH", InGameTranslator.LanguageID.UsesLargeFont(menu.CurrLang) ? 125f : 90f, num, 7, textInBoxes: false, menu.CurrLang == InGameTranslator.LanguageID.French || menu.CurrLang == InGameTranslator.LanguageID.Spanish || menu.CurrLang == InGameTranslator.LanguageID.Portuguese);
			subObjects.Add(rainTimer);
			earlyRainCheckbox = new CheckBox(menu, this, this, rainTimer.pos + new Vector2(num - 24f, 30f), GetEarlyRainCheckboxWidth(menu.CurrLang), menu.Translate("Early Rain:"), "EARLYRAIN");
			subObjects.Add(earlyRainCheckbox);
		}
	}

	private static float GetEarlyRainCheckboxWidth(InGameTranslator.LanguageID lang)
	{
		if (!(lang == InGameTranslator.LanguageID.Italian) && !(lang == InGameTranslator.LanguageID.Spanish) && !(lang == InGameTranslator.LanguageID.Japanese))
		{
			return 90f;
		}
		return 110f;
	}

	public override void Update()
	{
		base.Update();
		if (scoreToEnterDen != null)
		{
			scoreToEnterDen.greyedOut = GetGameTypeSetup.denEntryRule != ArenaSetup.GameTypeSetup.DenEntryRule.Score;
		}
		int num = 0;
		for (int i = 0; i < GetArenaSetup.playersJoined.Length; i++)
		{
			if (GetArenaSetup.playersJoined[i])
			{
				num++;
			}
		}
		if (earlyRainCheckbox != null)
		{
			earlyRainCheckbox.buttonBehav.greyedOut = num < 2;
		}
		if (spearsHitCheckbox != null)
		{
			spearsHitCheckbox.buttonBehav.greyedOut = num < 2;
		}
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		if (divSprite != null)
		{
			divSprite.x = DrawX(timeStacker) + divSpritePos.x;
			divSprite.y = DrawY(timeStacker) + divSpritePos.y;
		}
	}

	public override void RemoveSprites()
	{
		if (divSprite != null)
		{
			divSprite.RemoveFromContainer();
		}
		base.RemoveSprites();
	}

	public bool GetChecked(CheckBox box)
	{
		return box.IDString switch
		{
			"SPEARSHIT" => GetGameTypeSetup.spearsHitPlayers, 
			"EVILAI" => GetGameTypeSetup.evilAI, 
			"LEVELITEMS" => GetGameTypeSetup.levelItems, 
			"FLIESSPAWN" => GetGameTypeSetup.fliesSpawn, 
			"EARLYRAIN" => GetGameTypeSetup.rainWhenOnePlayerLeft, 
			_ => false, 
		};
	}

	public void SetChecked(CheckBox box, bool c)
	{
		switch (box.IDString)
		{
		case "SPEARSHIT":
			GetGameTypeSetup.spearsHitPlayers = c;
			break;
		case "EVILAI":
			GetGameTypeSetup.evilAI = c;
			break;
		case "LEVELITEMS":
			GetGameTypeSetup.levelItems = c;
			break;
		case "FLIESSPAWN":
			GetGameTypeSetup.fliesSpawn = c;
			break;
		case "EARLYRAIN":
			GetGameTypeSetup.rainWhenOnePlayerLeft = c;
			break;
		}
	}

	public int GetSelected(MultipleChoiceArray array)
	{
		switch (array.IDString)
		{
		case "ROOMREPEAT":
			return GetGameTypeSetup.levelRepeats - 1;
		case "SESSIONLENGTH":
			return GetGameTypeSetup.sessionTimeLengthIndex;
		case "WILDLIFE":
			if (GetGameTypeSetup.wildLifeSetting.Index != -1)
			{
				return GetGameTypeSetup.wildLifeSetting.Index;
			}
			return 0;
		case "SCORETOENTERDEN":
			return GetGameTypeSetup.scoreToEnterDenIndex;
		default:
			return 0;
		}
	}

	public void SetSelected(MultipleChoiceArray array, int i)
	{
		switch (array.IDString)
		{
		case "ROOMREPEAT":
			GetGameTypeSetup.levelRepeats = i + 1;
			break;
		case "SESSIONLENGTH":
			GetGameTypeSetup.sessionTimeLengthIndex = i;
			break;
		case "WILDLIFE":
			GetGameTypeSetup.wildLifeSetting = new ArenaSetup.GameTypeSetup.WildLifeSetting(ExtEnum<ArenaSetup.GameTypeSetup.WildLifeSetting>.values.GetEntry(i));
			break;
		case "SCORETOENTERDEN":
			GetGameTypeSetup.scoreToEnterDenIndex = i;
			break;
		}
	}

	public int GetCurrentlySelectedOfSeries(string series)
	{
		if (series != null && series == "DENENTRYRULE")
		{
			if (GetGameTypeSetup.denEntryRule.Index != -1)
			{
				return GetGameTypeSetup.denEntryRule.Index;
			}
			return 0;
		}
		return 0;
	}

	public void SetCurrentlySelectedOfSeries(string series, int to)
	{
		if (series != null && series == "DENENTRYRULE")
		{
			GetGameTypeSetup.denEntryRule = new ArenaSetup.GameTypeSetup.DenEntryRule(ExtEnum<ArenaSetup.GameTypeSetup.DenEntryRule>.values.GetEntry(to));
		}
	}
}
