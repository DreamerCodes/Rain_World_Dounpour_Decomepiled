using System;
using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace Menu;

public class SandboxSettingsInterface : PositionedMenuObject
{
	public abstract class ScoreController : PositionedMenuObject
	{
		public class ScoreDragger : ButtonTemplate
		{
			public RoundedRect roundedRect;

			public MenuLabel label;

			private bool held;

			public int lastY;

			public float savMouse;

			public int savScore;

			private int forgetClicked;

			public int yHeldCounter;

			private int allowNegativeCounter;

			private float flash;

			private float lastFlash;

			private float greyFade;

			private float lastGreyFade;

			public override Color MyColor(float timeStacker)
			{
				if (buttonBehav.greyedOut)
				{
					return HSLColor.Lerp(Menu.MenuColor(Menu.MenuColors.VeryDarkGrey), Menu.MenuColor(Menu.MenuColors.Black), black).rgb;
				}
				float a = Mathf.Lerp(buttonBehav.lastCol, buttonBehav.col, timeStacker);
				a = Mathf.Max(a, Mathf.Lerp(buttonBehav.lastFlash, buttonBehav.flash, timeStacker));
				HSLColor from = HSLColor.Lerp(Menu.MenuColor(Menu.MenuColors.DarkGrey), Menu.MenuColor(Menu.MenuColors.MediumGrey), a);
				from = HSLColor.Lerp(from, Menu.MenuColor(Menu.MenuColors.Black), black);
				return HSLColor.Lerp(from, Menu.MenuColor(Menu.MenuColors.VeryDarkGrey), Mathf.Lerp(lastGreyFade, greyFade, timeStacker)).rgb;
			}

			public void UpdateScoreText()
			{
				if ((owner as ScoreController).Score == 100)
				{
					label.text = menu.Translate("W");
				}
				else if ((owner as ScoreController).Score == -100)
				{
					label.text = menu.Translate("L");
				}
				else
				{
					label.text = (owner as ScoreController).Score.ToString();
				}
			}

			public ScoreDragger(Menu menu, MenuObject owner, Vector2 pos)
				: base(menu, owner, pos, new Vector2(24f, 24f))
			{
				roundedRect = new RoundedRect(menu, this, new Vector2(0f, 0f), size, filled: true);
				subObjects.Add(roundedRect);
				label = new MenuLabel(menu, this, "", new Vector2(0f, 2f), new Vector2(24f, 20f), bigText: false);
				subObjects.Add(label);
			}

			public override void Update()
			{
				base.Update();
				buttonBehav.Update();
				lastFlash = flash;
				lastGreyFade = greyFade;
				flash = Mathf.Max(0f, flash - 1f / 7f);
				greyFade = Custom.LerpAndTick(greyFade, ((owner.owner as SandboxSettingsInterface).freezeMenu && !held) ? 1f : 0f, 0.05f, 0.025f);
				if (buttonBehav.clicked)
				{
					forgetClicked++;
				}
				else
				{
					forgetClicked = 0;
				}
				roundedRect.fillAlpha = Mathf.Lerp(0.3f, 0.6f, buttonBehav.col);
				roundedRect.addSize = new Vector2(4f, 4f) * (buttonBehav.sizeBump + 0.5f * Mathf.Sin(buttonBehav.extraSizeBump * (float)Math.PI)) * (buttonBehav.clicked ? Mathf.InverseLerp(7f, 14f, forgetClicked) : 1f);
				bool flag = false;
				int num = (owner as ScoreController).Score;
				if (held)
				{
					int num2 = num;
					if (menu.manager.menuesMouseMode)
					{
						num = savScore + (int)Custom.LerpMap(Futile.mousePosition.y, savMouse - 300f, savMouse + 300f, -100f, 100f);
						if (num < 0)
						{
							flag = true;
						}
						num = Custom.IntClamp(num, (allowNegativeCounter > 60) ? (-100) : 0, 100);
					}
					else
					{
						int y = menu.NonMouseInputDisregardingFreeze.y;
						if (y != lastY || (yHeldCounter > 20 && yHeldCounter % ((yHeldCounter > 60) ? 2 : 4) == 0))
						{
							num += y * ((yHeldCounter <= 60) ? 1 : 2);
						}
						if (y != 0)
						{
							yHeldCounter++;
						}
						else
						{
							yHeldCounter = 0;
						}
						if (num < 0)
						{
							flag = true;
						}
						num = Custom.IntClamp(num, (allowNegativeCounter > 60) ? (-100) : 0, 100);
						lastY = y;
					}
					if (num != num2)
					{
						flash = 1f;
						menu.PlaySound(SoundID.MENU_Scroll_Tick);
						buttonBehav.sizeBump = Mathf.Min(2.5f, buttonBehav.sizeBump + 1f);
					}
				}
				else
				{
					lastY = 0;
					yHeldCounter = 0;
				}
				if (menu.manager.menuesMouseMode && MouseOver)
				{
					int num3 = num;
					num -= menu.mouseScrollWheelMovement;
					if (num < 0)
					{
						flag = true;
					}
					num = Custom.IntClamp(num, (allowNegativeCounter > 60) ? (-100) : 0, 100);
					if (num != num3)
					{
						flash = 1f;
						menu.PlaySound(SoundID.MENU_Scroll_Tick);
						buttonBehav.sizeBump = Mathf.Min(2.5f, buttonBehav.sizeBump + 1f);
						savScore = num;
					}
				}
				if (held && !menu.HoldButtonDisregardingFreeze)
				{
					(owner.owner as SandboxSettingsInterface).freezeMenu = false;
					held = false;
				}
				else if (!held && Selected && menu.pressButton)
				{
					(owner.owner as SandboxSettingsInterface).freezeMenu = true;
					savMouse = Futile.mousePosition.y;
					savScore = (owner as ScoreController).Score;
					held = true;
				}
				(owner as ScoreController).Score = num;
				if (num < 0)
				{
					allowNegativeCounter = 120;
				}
				else
				{
					allowNegativeCounter = Custom.IntClamp(allowNegativeCounter + ((!flag) ? (-1) : (menu.manager.menuesMouseMode ? 1 : 3)), 0, 120);
				}
			}

			public override void GrafUpdate(float timeStacker)
			{
				base.GrafUpdate(timeStacker);
				float num = 0.5f - 0.5f * Mathf.Sin(Mathf.Lerp(buttonBehav.lastSin, buttonBehav.sin, timeStacker) / 30f * (float)Math.PI * 2f);
				num *= buttonBehav.sizeBump;
				Color color = Color.Lerp(Menu.MenuRGB(Menu.MenuColors.Black), Menu.MenuRGB(Menu.MenuColors.White), Mathf.Lerp(buttonBehav.lastFlash, buttonBehav.flash, timeStacker));
				for (int i = 0; i < 9; i++)
				{
					roundedRect.sprites[i].color = color;
				}
				if (owner is LockedScore)
				{
					label.label.color = Menu.MenuRGB(Menu.MenuColors.VeryDarkGrey);
				}
				else
				{
					color = ((!held) ? Color.Lerp(base.MyColor(timeStacker), Menu.MenuRGB(Menu.MenuColors.VeryDarkGrey), Mathf.Max(num, Mathf.Lerp(lastGreyFade, greyFade, timeStacker))) : Color.Lerp(Color.Lerp(Menu.MenuRGB(Menu.MenuColors.DarkGrey), Menu.MenuRGB(Menu.MenuColors.MediumGrey), num), Menu.MenuRGB(Menu.MenuColors.White), Mathf.Lerp(lastFlash, flash, timeStacker)));
					label.label.color = color;
				}
				color = ((!held) ? MyColor(timeStacker) : Color.Lerp(Menu.MenuRGB(Menu.MenuColors.VeryDarkGrey), Menu.MenuRGB(Menu.MenuColors.DarkGrey), Mathf.Lerp(lastFlash, flash, timeStacker)));
				for (int j = 9; j < 17; j++)
				{
					roundedRect.sprites[j].color = color;
				}
			}

			public override void Clicked()
			{
			}
		}

		public ScoreDragger scoreDragger;

		public ArenaSetup.GameTypeSetup GetGameTypeSetup => (owner as SandboxSettingsInterface).GetGameTypeSetup;

		public virtual string DescriptorString => "";

		public virtual int Score
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		public ScoreController(Menu menu, MenuObject owner)
			: base(menu, owner, default(Vector2))
		{
			scoreDragger = new ScoreDragger(menu, this, new Vector2(0f, 0f));
			subObjects.Add(scoreDragger);
		}
	}

	public class MiscScore : ScoreController
	{
		public MenuLabel label;

		public string signalText;

		public override string DescriptorString
		{
			get
			{
				string text = (menu.manager.menuesMouseMode ? menu.Translate("Hold and drag to adjust score awarded") : menu.Translate("Press and hold, then use up/down to adjust score awarded"));
				return signalText switch
				{
					"FOODSCORE" => text + " " + menu.Translate("per point of food"), 
					"SURVIVESCORE" => text + " " + menu.Translate("for surviving"), 
					"SPEARHITSCORE" => text + " " + menu.Translate("per spear hit"), 
					_ => base.DescriptorString, 
				};
			}
		}

		public override int Score
		{
			get
			{
				return signalText switch
				{
					"FOODSCORE" => base.GetGameTypeSetup.foodScore, 
					"SURVIVESCORE" => base.GetGameTypeSetup.survivalScore, 
					"SPEARHITSCORE" => base.GetGameTypeSetup.spearHitScore, 
					_ => 0, 
				};
			}
			set
			{
				int score = Score;
				switch (signalText)
				{
				case "FOODSCORE":
					base.GetGameTypeSetup.foodScore = value;
					break;
				case "SURVIVESCORE":
					base.GetGameTypeSetup.survivalScore = value;
					break;
				case "SPEARHITSCORE":
					base.GetGameTypeSetup.spearHitScore = value;
					break;
				}
				scoreDragger.UpdateScoreText();
				if (Math.Abs(Score) > 99 != Math.Abs(score) > 99)
				{
					base.GetGameTypeSetup.UpdateCustomWinCondition();
				}
			}
		}

		public MiscScore(Menu menu, MenuObject owner, string text, string signalText)
			: base(menu, owner)
		{
			this.signalText = signalText;
			label = new MenuLabel(menu, this, text, new Vector2(-26f, 2f), new Vector2(40f, 20f), bigText: false);
			label.label.alignment = FLabelAlignment.Right;
			subObjects.Add(label);
		}

		public override void GrafUpdate(float timeStacker)
		{
			base.GrafUpdate(timeStacker);
			label.label.color = scoreDragger.MyColor(timeStacker);
		}
	}

	public class KillScore : ScoreController
	{
		public MultiplayerUnlocks.SandboxUnlockID unlockID;

		public CreatureSymbol symbol;

		public override string DescriptorString => (menu.manager.menuesMouseMode ? menu.Translate("Hold and drag to adjust score awarded") : menu.Translate("Press and hold, then use up/down to adjust score awarded")) + " " + menu.Translate("for killing creature");

		public override int Score
		{
			get
			{
				if (unlockID.Index == -1)
				{
					return 0;
				}
				return base.GetGameTypeSetup.killScores[unlockID.Index];
			}
			set
			{
				int score = Score;
				if (unlockID.Index != -1)
				{
					base.GetGameTypeSetup.killScores[unlockID.Index] = value;
				}
				scoreDragger.UpdateScoreText();
				if (Math.Abs(Score) > 99 != Math.Abs(score) > 99)
				{
					base.GetGameTypeSetup.UpdateCustomWinCondition();
				}
			}
		}

		public KillScore(Menu menu, MenuObject owner, MultiplayerUnlocks.SandboxUnlockID unlockID)
			: base(menu, owner)
		{
			this.unlockID = unlockID;
			symbol = new CreatureSymbol(MultiplayerUnlocks.SymbolDataForSandboxUnlock(unlockID), Container);
			symbol.Show(showShadowSprites: false);
			symbol.showFlash = 0f;
			symbol.lastShowFlash = 0f;
		}

		public override void Update()
		{
			base.Update();
			symbol.Update();
		}

		public override void GrafUpdate(float timeStacker)
		{
			base.GrafUpdate(timeStacker);
			symbol.Draw(timeStacker, DrawPos(timeStacker) + new Vector2(-23f, 12f));
		}

		public override void RemoveSprites()
		{
			base.RemoveSprites();
			symbol.RemoveSprites();
		}
	}

	public class LockedScore : ScoreController
	{
		private FSprite symbolSprite;

		private FSprite shadowSprite1;

		private FSprite shadowSprite2;

		public LockedScore(Menu menu, MenuObject owner)
			: base(menu, owner)
		{
			shadowSprite2 = new FSprite("Sandbox_SmallQuestionmark");
			shadowSprite2.color = Color.black;
			Container.AddChild(shadowSprite2);
			shadowSprite1 = new FSprite("Sandbox_SmallQuestionmark");
			shadowSprite1.color = Color.black;
			Container.AddChild(shadowSprite1);
			symbolSprite = new FSprite("Sandbox_SmallQuestionmark");
			symbolSprite.color = Menu.MenuRGB(Menu.MenuColors.VeryDarkGrey);
			Container.AddChild(symbolSprite);
			scoreDragger.buttonBehav.greyedOut = true;
		}

		public override void GrafUpdate(float timeStacker)
		{
			base.GrafUpdate(timeStacker);
			Vector2 vector = DrawPos(timeStacker) + new Vector2(-23f, 12f);
			symbolSprite.x = vector.x;
			symbolSprite.y = vector.y;
			shadowSprite1.x = vector.x - 2f;
			shadowSprite1.y = vector.y - 1f;
			shadowSprite2.x = vector.x - 1f;
			shadowSprite2.y = vector.y + 1f;
		}

		public override void RemoveSprites()
		{
			base.RemoveSprites();
			shadowSprite2.RemoveFromContainer();
			shadowSprite1.RemoveFromContainer();
			symbolSprite.RemoveFromContainer();
		}
	}

	public class PageButton : SymbolButton
	{
		public int direction;

		public PageButton(Menu menu, MenuObject owner, string singalText, Vector2 pos, int direction)
			: base(menu, owner, "Menu_Symbol_Arrow", singalText, pos)
		{
			this.direction = direction;
		}

		public override void GrafUpdate(float timeStacker)
		{
			base.GrafUpdate(timeStacker);
			symbolSprite.rotation = 90f * (float)direction;
		}
	}

	public FSprite divSprite;

	private Vector2 divSpritePos;

	public List<ScoreController> scoreControllers;

	private SymbolButton clearButton;

	public bool freezeMenu;

	public int currentPage;

	private PageButton prevPage;

	private PageButton nextPage;

	public MultiplayerMenu GetMultiplayerMenu => menu as MultiplayerMenu;

	public ArenaSetup GetArenaSetup => menu.manager.arenaSetup;

	public ArenaSetup.GameTypeSetup GetGameTypeSetup => GetArenaSetup.GetOrInitiateGameTypeSetup(GetMultiplayerMenu.currentGameType);

	public SandboxSettingsInterface(Menu menu, MenuObject owner)
		: base(menu, owner, new Vector2(440f, 385f))
	{
		ReinitInterface();
	}

	private void ReinitInterface()
	{
		scoreControllers = new List<ScoreController>();
		int num = 17 * 3 - 3;
		IntVector2 ps = new IntVector2(0, -6);
		List<MultiplayerUnlocks.SandboxUnlockID> sandboxUnlocksToShow = GetSandboxUnlocksToShow();
		int num2 = Math.Max((sandboxUnlocksToShow.Count - 1) / num + 1, 1);
		for (int i = currentPage * num; i < Math.Min(sandboxUnlocksToShow.Count, (currentPage + 1) * num); i++)
		{
			AddScoreButton(sandboxUnlocksToShow[i], ref ps);
		}
		int num3 = 1;
		if (11 - ps.y == 3)
		{
			num3 = 0;
		}
		else if (11 - ps.y < 3)
		{
			num3 = 11 - ps.y;
		}
		for (int j = 0; j < num3; j++)
		{
			AddScoreButton(null, ref ps);
		}
		Vector2 languageOffset = GetLanguageOffset(menu.CurrLang);
		AddPositionedScoreButton(new MiscScore(menu, this, menu.Translate("Food"), "FOODSCORE"), ref ps, languageOffset);
		AddPositionedScoreButton(new MiscScore(menu, this, menu.Translate("Survive"), "SURVIVESCORE"), ref ps, languageOffset);
		AddPositionedScoreButton(new MiscScore(menu, this, menu.Translate("Spear hit"), "SPEARHITSCORE"), ref ps, languageOffset);
		if (menu.CurrLang != InGameTranslator.LanguageID.English)
		{
			for (int k = 1; k < 4; k++)
			{
				scoreControllers[scoreControllers.Count - k].pos.x += 24f;
			}
		}
		if (clearButton == null)
		{
			clearButton = new SymbolButton(menu, this, "Menu_Symbol_Clear_All", "CLEARSCORES", new Vector2(0f, -335f));
			subObjects.Add(clearButton);
		}
		if (num2 > 1)
		{
			if (prevPage == null)
			{
				prevPage = new PageButton(menu, this, "PREV_SYMBOLS", new Vector2(88.666f, -335f), 3);
				subObjects.Add(prevPage);
			}
			prevPage.buttonBehav.greyedOut = currentPage == 0;
			if (nextPage == null)
			{
				nextPage = new PageButton(menu, this, "NEXT_SYMBOLS", new Vector2(177.332f, -335f), 1);
				subObjects.Add(nextPage);
			}
			nextPage.buttonBehav.greyedOut = currentPage == num2 - 1;
		}
		for (int l = 0; l < subObjects.Count; l++)
		{
			if (subObjects[l] is ScoreController)
			{
				(subObjects[l] as ScoreController).scoreDragger.UpdateScoreText();
			}
		}
	}

	private static List<MultiplayerUnlocks.SandboxUnlockID> GetSandboxUnlocksToShow()
	{
		List<MultiplayerUnlocks.SandboxUnlockID> list = new List<MultiplayerUnlocks.SandboxUnlockID>();
		foreach (MultiplayerUnlocks.SandboxUnlockID creatureUnlock in MultiplayerUnlocks.CreatureUnlockList)
		{
			if (IsThisSandboxUnlockVisible(creatureUnlock))
			{
				list.Add(new MultiplayerUnlocks.SandboxUnlockID(creatureUnlock.value));
			}
		}
		return list;
	}

	private static bool IsThisSandboxUnlockVisible(MultiplayerUnlocks.SandboxUnlockID sandboxUnlockID)
	{
		string value = sandboxUnlockID.value;
		if (value != null && value != MultiplayerUnlocks.SandboxUnlockID.Fly.value && value != MultiplayerUnlocks.SandboxUnlockID.Leech.value && value != MultiplayerUnlocks.SandboxUnlockID.SeaLeech.value && (!ModManager.MSC || value != MoreSlugcatsEnums.SandboxUnlockID.JungleLeech.value) && value != MultiplayerUnlocks.SandboxUnlockID.SmallNeedleWorm.value && value != MultiplayerUnlocks.SandboxUnlockID.Spider.value && value != MultiplayerUnlocks.SandboxUnlockID.VultureGrub.value && (ModManager.MSC || value != MultiplayerUnlocks.SandboxUnlockID.BigEel.value) && (ModManager.MSC || value != MultiplayerUnlocks.SandboxUnlockID.Deer.value) && value != MultiplayerUnlocks.SandboxUnlockID.SmallCentipede.value && value != MultiplayerUnlocks.SandboxUnlockID.TubeWorm.value)
		{
			return value != MultiplayerUnlocks.SandboxUnlockID.Hazer.value;
		}
		return false;
	}

	private static Vector2 GetLanguageOffset(InGameTranslator.LanguageID lang)
	{
		Vector2 zero = Vector2.zero;
		if (lang == InGameTranslator.LanguageID.Japanese)
		{
			zero.x = 15f;
		}
		else if (lang == InGameTranslator.LanguageID.French || lang == InGameTranslator.LanguageID.Italian || lang == InGameTranslator.LanguageID.Spanish || lang == InGameTranslator.LanguageID.Portuguese || lang == InGameTranslator.LanguageID.Russian)
		{
			zero.x = 5f;
		}
		return zero;
	}

	private void ClearInterface()
	{
		for (int i = 0; i < scoreControllers.Count; i++)
		{
			scoreControllers[i].RemoveSprites();
			RemoveSubObject(scoreControllers[i]);
		}
	}

	private void AddScoreButton(MultiplayerUnlocks.SandboxUnlockID unlockID, ref IntVector2 ps)
	{
		if (unlockID == null)
		{
			AddPositionedScoreButton(null, ref ps, Vector2.zero);
		}
		else if (GetMultiplayerMenu.multiplayerUnlocks.SandboxItemUnlocked(unlockID))
		{
			AddPositionedScoreButton(new KillScore(menu, this, unlockID), ref ps, Vector2.zero);
		}
		else
		{
			AddPositionedScoreButton(new LockedScore(menu, this), ref ps, Vector2.zero);
		}
	}

	private void AddPositionedScoreButton(ScoreController button, ref IntVector2 ps, Vector2 additionalOffset)
	{
		if (button != null)
		{
			scoreControllers.Add(button);
			subObjects.Add(button);
			button.pos = new Vector2((float)ps.x * 88.666f + 0.01f + additionalOffset.x, (float)ps.y * -30f + additionalOffset.y);
		}
		ps.y++;
		if (ps.y > 10)
		{
			ps.y = -6;
			ps.x++;
		}
	}

	public override void Singal(MenuObject sender, string message)
	{
		base.Singal(sender, message);
		if (message == "CLEARSCORES")
		{
			menu.PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
			bool flag = false;
			for (int i = 0; i < subObjects.Count; i++)
			{
				if (subObjects[i] is ScoreController && (subObjects[i] as ScoreController).Score != 0)
				{
					flag = true;
				}
			}
			if (flag)
			{
				ZeroScores();
			}
			else
			{
				DefaultScores();
			}
		}
		if (message == "NEXT_SYMBOLS")
		{
			currentPage++;
			ClearInterface();
			ReinitInterface();
		}
		if (message == "PREV_SYMBOLS")
		{
			currentPage--;
			ClearInterface();
			ReinitInterface();
		}
	}

	public void DefaultScores()
	{
		DefaultKillScores(ref GetGameTypeSetup.killScores);
		GetGameTypeSetup.foodScore = 1;
		GetGameTypeSetup.survivalScore = 5;
		GetGameTypeSetup.spearHitScore = 0;
		for (int i = 0; i < subObjects.Count; i++)
		{
			if (subObjects[i] is ScoreController)
			{
				(subObjects[i] as ScoreController).scoreDragger.UpdateScoreText();
			}
		}
	}

	public static void DefaultKillScores(ref int[] killScores)
	{
		killScores[(int)MultiplayerUnlocks.SandboxUnlockID.Slugcat] = 5;
		killScores[(int)MultiplayerUnlocks.SandboxUnlockID.GreenLizard] = 10;
		killScores[(int)MultiplayerUnlocks.SandboxUnlockID.PinkLizard] = 7;
		killScores[(int)MultiplayerUnlocks.SandboxUnlockID.BlueLizard] = 6;
		killScores[(int)MultiplayerUnlocks.SandboxUnlockID.WhiteLizard] = 8;
		killScores[(int)MultiplayerUnlocks.SandboxUnlockID.BlackLizard] = 7;
		killScores[(int)MultiplayerUnlocks.SandboxUnlockID.YellowLizard] = 6;
		killScores[(int)MultiplayerUnlocks.SandboxUnlockID.CyanLizard] = 9;
		killScores[(int)MultiplayerUnlocks.SandboxUnlockID.RedLizard] = 25;
		killScores[(int)MultiplayerUnlocks.SandboxUnlockID.Salamander] = 7;
		killScores[(int)MultiplayerUnlocks.SandboxUnlockID.CicadaA] = 2;
		killScores[(int)MultiplayerUnlocks.SandboxUnlockID.CicadaB] = 2;
		killScores[(int)MultiplayerUnlocks.SandboxUnlockID.Snail] = 1;
		killScores[(int)MultiplayerUnlocks.SandboxUnlockID.PoleMimic] = 2;
		killScores[(int)MultiplayerUnlocks.SandboxUnlockID.TentaclePlant] = 7;
		killScores[(int)MultiplayerUnlocks.SandboxUnlockID.Scavenger] = 6;
		killScores[(int)MultiplayerUnlocks.SandboxUnlockID.Vulture] = 15;
		killScores[(int)MultiplayerUnlocks.SandboxUnlockID.KingVulture] = 25;
		killScores[(int)MultiplayerUnlocks.SandboxUnlockID.MediumCentipede] = 4;
		killScores[(int)MultiplayerUnlocks.SandboxUnlockID.BigCentipede] = 7;
		killScores[(int)MultiplayerUnlocks.SandboxUnlockID.RedCentipede] = (ModManager.MSC ? 25 : 19);
		killScores[(int)MultiplayerUnlocks.SandboxUnlockID.Centiwing] = 5;
		killScores[(int)MultiplayerUnlocks.SandboxUnlockID.LanternMouse] = 2;
		killScores[(int)MultiplayerUnlocks.SandboxUnlockID.BigSpider] = 4;
		killScores[(int)MultiplayerUnlocks.SandboxUnlockID.SpitterSpider] = 5;
		killScores[(int)MultiplayerUnlocks.SandboxUnlockID.MirosBird] = 16;
		killScores[(int)MultiplayerUnlocks.SandboxUnlockID.BrotherLongLegs] = 14;
		killScores[(int)MultiplayerUnlocks.SandboxUnlockID.DaddyLongLegs] = 25;
		killScores[(int)MultiplayerUnlocks.SandboxUnlockID.TubeWorm] = 1;
		killScores[(int)MultiplayerUnlocks.SandboxUnlockID.EggBug] = 2;
		killScores[(int)MultiplayerUnlocks.SandboxUnlockID.DropBug] = 5;
		killScores[(int)MultiplayerUnlocks.SandboxUnlockID.BigNeedleWorm] = 5;
		killScores[(int)MultiplayerUnlocks.SandboxUnlockID.JetFish] = 4;
		killScores[(int)MultiplayerUnlocks.SandboxUnlockID.BigEel] = 25;
		killScores[(int)MultiplayerUnlocks.SandboxUnlockID.Deer] = 12;
		if (ModManager.MSC)
		{
			killScores[(int)MoreSlugcatsEnums.SandboxUnlockID.MirosVulture] = 25;
			killScores[(int)MoreSlugcatsEnums.SandboxUnlockID.MotherSpider] = 4;
			killScores[(int)MoreSlugcatsEnums.SandboxUnlockID.EelLizard] = 6;
			killScores[(int)MoreSlugcatsEnums.SandboxUnlockID.SpitLizard] = 11;
			killScores[(int)MoreSlugcatsEnums.SandboxUnlockID.TerrorLongLegs] = 25;
			killScores[(int)MoreSlugcatsEnums.SandboxUnlockID.AquaCenti] = 10;
			killScores[(int)MoreSlugcatsEnums.SandboxUnlockID.FireBug] = 5;
			killScores[(int)MoreSlugcatsEnums.SandboxUnlockID.Inspector] = 12;
			killScores[(int)MoreSlugcatsEnums.SandboxUnlockID.Yeek] = 2;
			killScores[(int)MoreSlugcatsEnums.SandboxUnlockID.BigJelly] = 20;
			killScores[(int)MoreSlugcatsEnums.SandboxUnlockID.StowawayBug] = 8;
			killScores[(int)MoreSlugcatsEnums.SandboxUnlockID.ZoopLizard] = 6;
			killScores[(int)MoreSlugcatsEnums.SandboxUnlockID.ScavengerElite] = 12;
			killScores[(int)MoreSlugcatsEnums.SandboxUnlockID.SlugNPC] = 2;
		}
	}

	public void ZeroScores()
	{
		for (int i = 0; i < subObjects.Count; i++)
		{
			if (subObjects[i] is ScoreController)
			{
				(subObjects[i] as ScoreController).Score = 0;
			}
		}
	}
}
