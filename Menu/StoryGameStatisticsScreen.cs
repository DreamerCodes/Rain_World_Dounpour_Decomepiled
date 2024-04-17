using System;
using System.Collections.Generic;
using HUD;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace Menu;

public class StoryGameStatisticsScreen : KarmaLadderScreen
{
	public class TickerID : ExtEnum<TickerID>
	{
		public static readonly TickerID Food = new TickerID("Food", register: true);

		public static readonly TickerID Survives = new TickerID("Survives", register: true);

		public static readonly TickerID Deaths = new TickerID("Deaths", register: true);

		public static readonly TickerID Quits = new TickerID("Quits", register: true);

		public static readonly TickerID Time = new TickerID("Time", register: true);

		public static readonly TickerID Payload = new TickerID("Payload", register: true);

		public static readonly TickerID FivePebbles = new TickerID("FivePebbles", register: true);

		public static readonly TickerID Ascension = new TickerID("Ascension", register: true);

		public static readonly TickerID Kill = new TickerID("Kill", register: true);

		public TickerID(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class TickMode : ExtEnum<TickMode>
	{
		public static readonly TickMode OnlyTicker = new TickMode("OnlyTicker", register: true);

		public static readonly TickMode OnlyScore = new TickMode("OnlyScore", register: true);

		public static readonly TickMode Both = new TickMode("Both", register: true);

		public TickMode(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public abstract class Ticker : PositionedMenuObject
	{
		public bool visible;

		public int getToValue;

		public int displayValue;

		public MenuLabel numberLabel;

		public float showFlash;

		public float lastShowFlash;

		public TickerID ID;

		public virtual bool FastTick => displayValue <= getToValue - 10;

		public virtual Color MyColor(float timeStacker)
		{
			return Color.Lerp(Menu.MenuRGB(MenuColors.MediumGrey), Menu.MenuRGB(MenuColors.White), Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(lastShowFlash, showFlash, timeStacker)), 3f));
		}

		public Ticker(Menu menu, MenuObject owner, Vector2 pos, int getToValue, TickerID ID)
			: base(menu, owner, pos)
		{
			this.getToValue = getToValue;
			this.ID = ID;
			numberLabel = new MenuLabel(menu, this, "", new Vector2(10f, -10f), new Vector2(60f, 20f), bigText: true);
			subObjects.Add(numberLabel);
		}

		public override void Update()
		{
			base.Update();
			lastShowFlash = showFlash;
			showFlash = Custom.LerpAndTick(showFlash, 0f, 0.08f, 0.1f);
		}

		public virtual void Tick()
		{
			menu.PlaySound(SoundID.UI_Multiplayer_Player_Result_Box_Number_Tick);
			if (displayValue < getToValue)
			{
				if ((menu as StoryGameStatisticsScreen).FastMode)
				{
					displayValue += 5;
					if (displayValue > getToValue)
					{
						displayValue = getToValue;
					}
				}
				else
				{
					displayValue++;
				}
			}
			showFlash = Mathf.Max(0.5f, showFlash);
			UpdateText();
		}

		public virtual void UpdateText()
		{
			numberLabel.text = displayValue.ToString();
		}

		public virtual void Show()
		{
			menu.PlaySound(SoundID.UI_Multiplayer_Player_Result_Box_Kill_Tick);
			visible = true;
			showFlash = 1f;
			lastShowFlash = 1f;
		}

		public override void GrafUpdate(float timeStacker)
		{
			base.GrafUpdate(timeStacker);
			numberLabel.label.isVisible = visible;
			numberLabel.label.color = MyColor(timeStacker);
		}
	}

	public class LabelTicker : Ticker
	{
		public MenuLabel nameLabel;

		public LabelTicker(Menu menu, MenuObject owner, Vector2 pos, int getToValue, TickerID ID, string name)
			: base(menu, owner, pos, getToValue, ID)
		{
			nameLabel = new MenuLabel(menu, this, name, new Vector2(-180f, -10f), new Vector2(60f, 20f), bigText: true);
			nameLabel.label.alignment = FLabelAlignment.Left;
			subObjects.Add(nameLabel);
			numberLabel.label.alignment = FLabelAlignment.Right;
			numberLabel.pos.x += 50f;
		}

		public override void GrafUpdate(float timeStacker)
		{
			base.GrafUpdate(timeStacker);
			nameLabel.label.color = MyColor(timeStacker);
			nameLabel.label.isVisible = visible;
		}
	}

	public class Popper : Ticker
	{
		private string text;

		public Popper(Menu menu, MenuObject owner, Vector2 pos, string text, TickerID ID)
			: base(menu, owner, pos, 1, ID)
		{
			this.text = text;
			numberLabel.pos.x = -150f;
			numberLabel.size.x = 240f;
			UpdateText();
		}

		public override void UpdateText()
		{
			numberLabel.text = text;
		}
	}

	public class TimeTicker : LabelTicker
	{
		private bool secondsTicking;

		public override bool FastTick
		{
			get
			{
				if (secondsTicking)
				{
					return base.FastTick;
				}
				return false;
			}
		}

		public TimeTicker(Menu menu, MenuObject owner, Vector2 pos, int getToValue)
			: base(menu, owner, pos, getToValue, TickerID.Time, menu.Translate("Time :"))
		{
		}

		public override void Tick()
		{
			menu.PlaySound(SoundID.UI_Multiplayer_Player_Result_Box_Number_Tick);
			int num = getToValue - displayValue;
			if (num > 0)
			{
				int num2 = Mathf.FloorToInt((float)num / 60f);
				int num3 = Mathf.FloorToInt((float)num / 3600f);
				num2 -= num3 * 60;
				num -= num3 * 60 * 60;
				num -= num2 * 60;
				secondsTicking = num > 0;
				if (num > 0)
				{
					displayValue = Math.Min(displayValue + 1, getToValue);
				}
				else if (num2 > 0)
				{
					displayValue = Math.Min(displayValue + 60, getToValue);
				}
				else
				{
					displayValue = Math.Min(displayValue + 3600, getToValue);
				}
				UpdateText();
			}
		}

		public override void UpdateText()
		{
			numberLabel.text = SecondsToMinutesAndSecondsString(displayValue);
		}

		public string SecondsToMinutesAndSecondsString(int seconds)
		{
			int num = Mathf.FloorToInt((float)seconds / 60f);
			int num2 = Mathf.FloorToInt((float)seconds / 3600f);
			num -= num2 * 60;
			seconds -= num2 * 60 * 60;
			seconds -= num * 60;
			if (num2 > 0)
			{
				return num2.ToString("D2") + ":" + num.ToString("D2") + ":" + seconds.ToString("D2");
			}
			if (num > 0)
			{
				return num.ToString("D2") + ":" + seconds.ToString("D2");
			}
			return seconds.ToString("D2");
		}
	}

	public class KillsTable : PositionedMenuObject
	{
		public class KillTicker : Ticker
		{
			public IconSymbol symbol;

			public KillTicker(Menu menu, MenuObject owner, IconSymbol.IconSymbolData creatureType, int kills)
				: base(menu, owner, default(Vector2), kills, TickerID.Kill)
			{
				symbol = IconSymbol.CreateIconSymbol(creatureType, Container);
			}

			public override Color MyColor(float timeStacker)
			{
				if (symbol.symbolSprite != null)
				{
					return symbol.symbolSprite.color;
				}
				return base.MyColor(timeStacker);
			}

			public override void Tick()
			{
				base.Tick();
				symbol.showFlash = Mathf.Max(symbol.showFlash, 0.5f);
			}

			public override void Show()
			{
				base.Show();
				symbol.Show(showShadowSprites: false);
			}

			public override void Update()
			{
				base.Update();
				symbol.Update();
			}

			public override void GrafUpdate(float timeStacker)
			{
				base.GrafUpdate(timeStacker);
				if (visible)
				{
					symbol.Draw(timeStacker, DrawPos(timeStacker));
				}
				else
				{
					symbol.Draw(timeStacker, new Vector2(-1000f, -1000f));
				}
			}

			public override void RemoveSprites()
			{
				symbol.RemoveSprites();
				base.RemoveSprites();
			}
		}

		public List<KillTicker> killCounts;

		public KillsTable(Menu menu, MenuObject owner, Vector2 pos, List<KeyValuePair<IconSymbol.IconSymbolData, int>> killsData)
			: base(menu, owner, pos)
		{
			killCounts = new List<KillTicker>();
			for (int i = 0; i < killsData.Count; i++)
			{
				if (!CreatureSymbol.DoesCreatureEarnATrophy(killsData[i].Key.critType))
				{
					continue;
				}
				KillTicker killTicker = new KillTicker(menu, this, killsData[i].Key, killsData[i].Value);
				bool flag = false;
				for (int j = 0; j < killCounts.Count; j++)
				{
					if (KillSort(killTicker, killCounts[j]))
					{
						killCounts.Insert(j, killTicker);
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					killCounts.Add(killTicker);
				}
			}
			float num = menu.manager.rainWorld.screenSize.x / 2f - pos.x;
			float num2 = ((menu.manager.rainWorld.screenSize.x <= 1024f) ? 85f : 100f);
			IntVector2 intVector = new IntVector2(0, 0);
			int num3 = Custom.IntClamp(Mathf.RoundToInt((float)killCounts.Count * num2 / num), 4, 6);
			int num4 = killCounts.Count / num3;
			if (num4 > 7)
			{
				num2 = 70f;
				num3 = Custom.IntClamp(Mathf.RoundToInt((float)killCounts.Count * num2 / num), 4, 6);
				num4 = killCounts.Count / num3;
			}
			if (num4 > 10)
			{
				num2 = 50f;
				num3 = Custom.IntClamp(Mathf.RoundToInt((float)killCounts.Count * num2 / num), 4, 7);
				for (int k = 0; k < killCounts.Count; k++)
				{
					killCounts[k].numberLabel.pos.x -= 15f;
				}
			}
			for (int l = 0; l < killCounts.Count; l++)
			{
				subObjects.Add(killCounts[l]);
				(menu as StoryGameStatisticsScreen).allTickers.Add(killCounts[l]);
				killCounts[l].pos = new Vector2((float)intVector.x * num2, (float)(num3 - intVector.y) * 30f);
				killCounts[l].lastPos = killCounts[l].pos;
				intVector.y++;
				if (intVector.y >= num3)
				{
					intVector.y = 0;
					intVector.x++;
				}
			}
		}

		private static bool KillSort(KillTicker a, KillTicker b)
		{
			return SortNumber(a.symbol.iconData) > SortNumber(b.symbol.iconData);
		}

		private static int SortNumber(IconSymbol.IconSymbolData dt)
		{
			int num = MultiplayerUnlocks.SandboxUnlockForSymbolData(dt).Index;
			if (num == (int)MultiplayerUnlocks.SandboxUnlockID.Slugcat)
			{
				num += 1000;
			}
			return num * -100000 + (int)dt.critType * 1000 + (int)dt.itemType * 10 - dt.intData;
		}
	}

	public class ScoreKeeper : PositionedMenuObject
	{
		public class ScoreAdder : PositionedMenuObject
		{
			public int stackHeight;

			public int score;

			public int times;

			public MenuLabel numberLabel;

			public float alpha;

			public float lastAlpha;

			public int wait = 10;

			private ScoreKeeper scoreKeeper => owner as ScoreKeeper;

			public int MultipliedScore => score * times;

			public bool InPlaceForTick
			{
				get
				{
					if (stackHeight == 0)
					{
						return pos.y < scoreKeeper.IdealHeightForAdder(stackHeight) + 1f;
					}
					return false;
				}
			}

			public ScoreAdder(Menu menu, MenuObject owner, Vector2 pos, int score, int times, int stackHeight)
				: base(menu, owner, pos)
			{
				this.stackHeight = stackHeight;
				this.score = score;
				this.times = times;
				numberLabel = new MenuLabel(menu, this, "", new Vector2(10f, -10f), new Vector2(60f, 20f), bigText: true);
				numberLabel.label.alignment = FLabelAlignment.Right;
				numberLabel.pos.x += 50f;
				numberLabel.label.color = ((MultipliedScore >= 0) ? Menu.MenuRGB(MenuColors.MediumGrey) : Color.red);
				numberLabel.label.alpha = 0f;
				subObjects.Add(numberLabel);
				lastAlpha = 0f;
				alpha = 0f;
				UpdateLabel();
				menu.PlaySound(SoundID.UI_Multiplayer_Player_Result_Box_Number_Tick, 0f, 0.5f, 1.2f);
			}

			public override void Update()
			{
				base.Update();
				lastAlpha = alpha;
				pos.y = Custom.LerpAndTick(pos.y, scoreKeeper.IdealHeightForAdder(stackHeight), 0.12f, 1f / 6f);
				bool flag = false;
				if (InPlaceForTick && (menu as StoryGameStatisticsScreen).tickMode != TickMode.OnlyTicker)
				{
					if (wait > 0)
					{
						wait--;
					}
					else if (score == 0)
					{
						flag = true;
					}
					else if ((menu as StoryGameStatisticsScreen).counter % 2 == 0)
					{
						Tick();
					}
				}
				if (flag)
				{
					alpha = Mathf.Max(0f, alpha - 1f / 7f);
					if (alpha <= 0f && lastAlpha <= 0f)
					{
						scoreKeeper.RemoveFirstScoreAdder();
					}
				}
				else
				{
					alpha = 1f;
				}
			}

			private void UpdateLabel()
			{
				numberLabel.text = ((times == -1) ? "-" : ((times != 1) ? (times + "x") : "")) + score;
			}

			private void Tick()
			{
				if (score > 100)
				{
					MoveScore(30);
				}
				else if (score > 10)
				{
					MoveScore(5);
				}
				else if (score > 0)
				{
					MoveScore(1);
				}
			}

			private void MoveScore(int amount)
			{
				score -= amount;
				scoreKeeper.Tick(amount * times);
				UpdateLabel();
			}

			public override void GrafUpdate(float timeStacker)
			{
				base.GrafUpdate(timeStacker);
				numberLabel.label.alpha = Mathf.Lerp(lastAlpha, alpha, timeStacker);
			}
		}

		public int score;

		public int multiplier;

		public int baseScore;

		public int multiAddScore;

		public MenuLabel nameLabel;

		public MenuLabel numberLabel;

		public MenuLabel multiplierLabel;

		public float showFlash;

		public float lastShowFlash;

		public float multiFlash;

		public float lastMultiFlash;

		public float multiAlpha;

		public float lastMultiAlpha;

		public List<ScoreAdder> scoreAdders;

		public FSprite lineSprite;

		public bool applyMulti;

		public virtual Color MyColor(float timeStacker)
		{
			return Color.Lerp(Menu.MenuRGB(MenuColors.MediumGrey), Menu.MenuRGB(MenuColors.White), Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(lastShowFlash, showFlash, timeStacker)), 3f));
		}

		public float IdealHeightForAdder(int i)
		{
			return 5f + 25f * (1f + (float)i);
		}

		public ScoreKeeper(Menu menu, MenuObject owner, Vector2 pos)
			: base(menu, owner, pos)
		{
			numberLabel = new MenuLabel(menu, this, "0", new Vector2(10f, -10f), new Vector2(60f, 20f), bigText: true);
			numberLabel.label.alignment = FLabelAlignment.Right;
			numberLabel.pos.x += 50f;
			subObjects.Add(numberLabel);
			nameLabel = new MenuLabel(menu, this, menu.Translate("Ranking") + " :", new Vector2(-180f, -10f), new Vector2(60f, 20f), bigText: true);
			nameLabel.label.alignment = FLabelAlignment.Left;
			subObjects.Add(nameLabel);
			multiplierLabel = new MenuLabel(menu, this, (menu as StoryGameStatisticsScreen).anyMultipliers ? "X 1" : "", new Vector2(10f, 20f), new Vector2(60f, 20f), bigText: true);
			multiplierLabel.label.alignment = FLabelAlignment.Right;
			multiplierLabel.pos.x += 50f;
			subObjects.Add(multiplierLabel);
			lineSprite = new FSprite("pixel");
			lineSprite.scaleX = 240f;
			lineSprite.anchorX = 0f;
			Container.AddChild(lineSprite);
			scoreAdders = new List<ScoreAdder>();
		}

		public override void Update()
		{
			base.Update();
			lastShowFlash = showFlash;
			lastMultiFlash = multiFlash;
			lastMultiAlpha = multiAlpha;
			showFlash = Custom.LerpAndTick(showFlash, 0f, 0.08f, 0.1f);
			multiFlash = Custom.LerpAndTick(multiFlash, 0f, 0.08f, 0.1f);
			multiAlpha = Custom.LerpAndTick(multiAlpha, ((menu as StoryGameStatisticsScreen).tickersDone && (menu as StoryGameStatisticsScreen).anyMultipliers && !(menu as StoryGameStatisticsScreen).allDone && scoreAdders.Count == 0) ? 1f : 0f, 0.02f, 0.02f);
			if (multiplier == 0 && multiAlpha > 0f && multiAlpha < 1f && (menu as StoryGameStatisticsScreen).anyMultipliers && (menu as StoryGameStatisticsScreen).allDone)
			{
				multiplierLabel.text = "X 0";
			}
			if (applyMulti && (menu as StoryGameStatisticsScreen).delay < 1)
			{
				if (multiAddScore > 0)
				{
					if (multiAddScore > 100)
					{
						MultiTick(30);
					}
					else if (multiAddScore > 10)
					{
						MultiTick(5);
					}
					else if (multiAddScore > 0)
					{
						MultiTick(1);
					}
					if (multiAddScore == 0)
					{
						(menu as StoryGameStatisticsScreen).delay = 7;
					}
				}
				else if (multiplier > 0)
				{
					multiplier--;
					multiplierLabel.text = "X " + (multiplier + 1);
					multiFlash = 1f;
					multiAddScore = baseScore;
					(menu as StoryGameStatisticsScreen).delay = 7;
					menu.PlaySound(SoundID.UI_Multiplayer_Player_Result_Box_Number_Tick);
				}
				else
				{
					(menu as StoryGameStatisticsScreen).allDone = true;
				}
			}
			for (int i = 0; i < scoreAdders.Count; i++)
			{
				scoreAdders[i].stackHeight = i;
			}
		}

		public void AddMulti()
		{
			menu.PlaySound(SoundID.UI_Multiplayer_Player_Result_Box_Kill_Tick);
			multiplier++;
			multiplierLabel.text = "X " + (multiplier + 1);
			multiFlash = 1f;
			(menu as KarmaLadderScreen).karmaLadder.hud.fadeCircles.Add(new FadeCircle((menu as KarmaLadderScreen).karmaLadder.hud, 10f, 6f, 0.92f, 20f, 2f, pos + new Vector2(80f, 30f), (menu as KarmaLadderScreen).ladderContainers[2]));
			baseScore = score;
		}

		public void AddScoreAdder(int score, int times)
		{
			if (score * times != 0)
			{
				scoreAdders.Add(new ScoreAdder(menu, this, new Vector2(0f, IdealHeightForAdder(scoreAdders.Count) + 15f), score, times, scoreAdders.Count));
				subObjects.Add(scoreAdders[scoreAdders.Count - 1]);
				if (scoreAdders.Count < 2)
				{
					scoreAdders[0].wait = 40;
				}
			}
		}

		public void RemoveFirstScoreAdder()
		{
			scoreAdders[0].RemoveSprites();
			RemoveSubObject(scoreAdders[0]);
			scoreAdders.RemoveAt(0);
			if (scoreAdders.Count == 0 && (menu as StoryGameStatisticsScreen).tickMode == TickMode.OnlyScore)
			{
				(menu as StoryGameStatisticsScreen).tickMode = TickMode.Both;
			}
		}

		public void MultiTick(int add)
		{
			multiAddScore -= add;
			score += add;
			showFlash = Mathf.Max(0.75f, showFlash);
			menu.PlaySound(SoundID.UI_Multiplayer_Player_Result_Box_Number_Tick);
			UpdateText();
		}

		public void Tick(int add)
		{
			menu.PlaySound(SoundID.UI_Multiplayer_Player_Result_Box_Number_Tick, 0f, ((menu as StoryGameStatisticsScreen).tickMode != TickMode.Both || (menu as StoryGameStatisticsScreen).tickersDone) ? 1f : 0.5f, 1f);
			score += add;
			showFlash = Mathf.Max(0.5f, showFlash);
			UpdateText();
		}

		public void UpdateText()
		{
			numberLabel.text = score.ToString();
		}

		public override void GrafUpdate(float timeStacker)
		{
			base.GrafUpdate(timeStacker);
			float num = Mathf.InverseLerp(60f, 180f, (float)(menu as StoryGameStatisticsScreen).counter + timeStacker);
			numberLabel.label.color = MyColor(timeStacker);
			numberLabel.label.alpha = num;
			nameLabel.label.color = MyColor(timeStacker);
			nameLabel.label.alpha = num;
			multiplierLabel.label.color = Color.Lerp(Menu.MenuRGB(MenuColors.MediumGrey), Menu.MenuRGB(MenuColors.White), Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(lastMultiFlash, multiFlash, timeStacker)), 3f));
			multiplierLabel.label.alpha = Mathf.Lerp(lastMultiAlpha, multiAlpha, timeStacker) * num;
			lineSprite.x = DrawX(timeStacker) - 150f;
			lineSprite.y = DrawY(timeStacker) + 14f;
			lineSprite.color = Menu.MenuRGB(MenuColors.VeryDarkGrey);
			lineSprite.alpha = num;
		}

		public override void RemoveSprites()
		{
			lineSprite.RemoveFromContainer();
			base.RemoveSprites();
		}
	}

	public int counter;

	public int delay;

	public int dispIndex;

	private bool initiated;

	private bool tickersDone;

	private bool allDone;

	public List<Ticker> allTickers;

	private ScoreKeeper scoreKeeper;

	public bool forceWatch;

	public TickMode tickMode = TickMode.OnlyTicker;

	public int[] killScores = new int[ExtEnum<MultiplayerUnlocks.SandboxUnlockID>.values.Count];

	public int passagesBump = -1;

	public int addPassageDelay = -1;

	private bool anyMultipliers;

	public override bool ButtonsGreyedOut
	{
		get
		{
			if (counter >= 80)
			{
				if (forceWatch)
				{
					return !allDone;
				}
				return false;
			}
			return true;
		}
	}

	public bool FastMode
	{
		get
		{
			if (ModManager.MMF)
			{
				return RWInput.PlayerInput(0).mp;
			}
			return false;
		}
	}

	public StoryGameStatisticsScreen(ProcessManager manager)
		: base(manager, ProcessManager.ProcessID.Statistics)
	{
		mySoundLoopID = SoundID.MENU_Main_Menu_LOOP;
		for (int i = 0; i < killScores.Length; i++)
		{
			killScores[i] = 1;
		}
		SandboxSettingsInterface.DefaultKillScores(ref killScores);
		killScores[(int)MultiplayerUnlocks.SandboxUnlockID.Slugcat] = 1;
	}

	public override void GetDataFromGame(SleepDeathScreenDataPackage package)
	{
		base.GetDataFromGame(package);
		initiated = true;
		allTickers = new List<Ticker>();
		if (package.saveState == null)
		{
			package.saveState = new SaveState(SlugcatStats.Name.White, manager.rainWorld.progression);
			package.saveState.totFood = UnityEngine.Random.Range(0, 200);
			package.saveState.deathPersistentSaveData.survives = UnityEngine.Random.Range(15, 22);
			package.saveState.deathPersistentSaveData.deaths = UnityEngine.Random.Range(0, 20);
			package.saveState.deathPersistentSaveData.quits = UnityEngine.Random.Range(0, 20);
			package.saveState.totTime = UnityEngine.Random.Range(0, 3) * 60 * 60 + UnityEngine.Random.Range(0, 60) * 60 + UnityEngine.Random.Range(0, 60);
			package.saveState.miscWorldSaveData.moonRevived = UnityEngine.Random.value < 0.5f;
			package.saveState.miscWorldSaveData.pebblesSeenGreenNeuron = UnityEngine.Random.value < 0.5f;
			package.saveState.deathPersistentSaveData.ascended = UnityEngine.Random.value < 0.5f;
			for (int i = 2; i < ExtEnum<CreatureTemplate.Type>.values.Count; i++)
			{
				if (UnityEngine.Random.value < 0.2f)
				{
					package.saveState.kills.Add(new KeyValuePair<IconSymbol.IconSymbolData, int>(new IconSymbol.IconSymbolData(new CreatureTemplate.Type(ExtEnum<CreatureTemplate.Type>.values.GetEntry(i)), AbstractPhysicalObject.AbstractObjectType.Creature, 0), UnityEngine.Random.Range(1, UnityEngine.Random.Range(1, 10))));
				}
			}
		}
		Vector2 vector = new Vector2(base.ContinueAndExitButtonsXPos - 160f, 535f);
		LabelTicker item = new LabelTicker(this, pages[0], vector + new Vector2(0f, 120f), package.saveState.totFood, TickerID.Food, Translate("Food :"));
		allTickers.Add(item);
		pages[0].subObjects.Add(item);
		item = new LabelTicker(this, pages[0], vector + new Vector2(0f, 90f), package.saveState.deathPersistentSaveData.survives, TickerID.Survives, Translate("Successful cycles :"));
		allTickers.Add(item);
		pages[0].subObjects.Add(item);
		item = new LabelTicker(this, pages[0], vector + new Vector2(0f, 60f), package.saveState.deathPersistentSaveData.deaths, TickerID.Deaths, Translate("Deaths :"));
		allTickers.Add(item);
		pages[0].subObjects.Add(item);
		item = new LabelTicker(this, pages[0], vector + new Vector2(0f, 30f), package.saveState.deathPersistentSaveData.quits, TickerID.Quits, Translate("Quits :"));
		allTickers.Add(item);
		pages[0].subObjects.Add(item);
		TimeTicker item2 = new TimeTicker(this, pages[0], vector, package.saveState.totTime);
		allTickers.Add(item2);
		pages[0].subObjects.Add(item2);
		int num = 2;
		if (package.saveState.saveStateNumber == SlugcatStats.Name.Red)
		{
			bool flag = false;
			if (package.saveState.miscWorldSaveData.moonRevived)
			{
				Popper item3 = new Popper(this, pages[0], vector + new Vector2(0f, -30f * (float)num), "< " + Translate("Delivered Payload") + ">", TickerID.Payload);
				allTickers.Add(item3);
				pages[0].subObjects.Add(item3);
				num++;
				flag = true;
			}
			if (package.saveState.miscWorldSaveData.pebblesSeenGreenNeuron)
			{
				Popper item3 = new Popper(this, pages[0], vector + new Vector2(0f, -30f * (float)num), "< " + Translate("Helped Five Pebbles") + ">", TickerID.FivePebbles);
				allTickers.Add(item3);
				pages[0].subObjects.Add(item3);
				num++;
				flag = true;
			}
			if (package.saveState.deathPersistentSaveData.ascended)
			{
				Popper item3 = new Popper(this, pages[0], vector + new Vector2(0f, -30f * (float)num), "< " + Translate("Ascended") + ">", TickerID.Ascension);
				allTickers.Add(item3);
				pages[0].subObjects.Add(item3);
				num++;
			}
			if (flag)
			{
				manager.CueAchievement(RainWorld.AchievementID.HunterPayload, 5f);
			}
		}
		else if (ModManager.MSC && package.saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
		{
			int count = package.saveState.miscWorldSaveData.SLOracleState.significantPearls.Count;
			if (count > 0)
			{
				LabelTicker item4 = new LabelTicker(this, pages[0], vector + new Vector2(0f, -30f * (float)num), count, MoreSlugcatsEnums.TickerID.PearlsRead, Translate("Unique pearls read :"));
				allTickers.Add(item4);
				pages[0].subObjects.Add(item4);
				num++;
			}
			if (package.saveState.deathPersistentSaveData.ascended)
			{
				Popper item5 = new Popper(this, pages[0], vector + new Vector2(0f, -30f * (float)num), "< " + Translate("Acceptance") + ">", TickerID.Ascension);
				allTickers.Add(item5);
				pages[0].subObjects.Add(item5);
				num++;
			}
		}
		else if (ModManager.MSC)
		{
			int count2 = package.saveState.miscWorldSaveData.SLOracleState.significantPearls.Count;
			if (count2 > 0)
			{
				LabelTicker item6 = new LabelTicker(this, pages[0], vector + new Vector2(0f, -30f * (float)num), count2, MoreSlugcatsEnums.TickerID.PearlsRead, Translate("Unique pearls read :"));
				allTickers.Add(item6);
				pages[0].subObjects.Add(item6);
				num++;
			}
			if (package.saveState.deathPersistentSaveData.friendsSaved > 0)
			{
				LabelTicker item7 = new LabelTicker(this, pages[0], vector + new Vector2(0f, -30f * (float)num), package.saveState.deathPersistentSaveData.friendsSaved, MoreSlugcatsEnums.TickerID.FriendsSaved, Translate("Friends sheltered :"));
				allTickers.Add(item7);
				pages[0].subObjects.Add(item7);
				num++;
			}
			if (package.saveState.miscWorldSaveData.SSaiConversationsHad > 0)
			{
				Popper item8 = new Popper(this, pages[0], vector + new Vector2(0f, -30f * (float)num), "< " + Translate("Met Five Pebbles") + ">", MoreSlugcatsEnums.TickerID.MetPebbles);
				allTickers.Add(item8);
				pages[0].subObjects.Add(item8);
				num++;
			}
			if (package.saveState.miscWorldSaveData.SLOracleState.playerEncounters > 0)
			{
				Popper item9 = new Popper(this, pages[0], vector + new Vector2(0f, -30f * (float)num), "< " + Translate("Met Looks to the Moon") + ">", MoreSlugcatsEnums.TickerID.MetMoon);
				allTickers.Add(item9);
				pages[0].subObjects.Add(item9);
				num++;
			}
			if (package.saveState.deathPersistentSaveData.winState.GetTracker(MoreSlugcatsEnums.EndgameID.Gourmand, addIfMissing: false) is WinState.GourFeastTracker { GoalFullfilled: not false })
			{
				Popper item10 = new Popper(this, pages[0], vector + new Vector2(0f, -30f * (float)num), "< " + Translate("Food quest completed") + ">", MoreSlugcatsEnums.TickerID.GourmandQuestFinished);
				allTickers.Add(item10);
				pages[0].subObjects.Add(item10);
				num++;
			}
		}
		pages[0].subObjects.Add(new KillsTable(this, pages[0], new Vector2(base.LeftHandButtonsPosXAdd + 15f, 4f), package.saveState.kills));
		tickersDone = allTickers.Count == 0;
		delay = 40;
		anyMultipliers = false;
		for (int j = 0; j < karmaLadder.endGameMeters.Count; j++)
		{
			if (karmaLadder.endGameMeters[j].tracker.GoalFullfilled)
			{
				anyMultipliers = true;
				break;
			}
		}
		scoreKeeper = new ScoreKeeper(this, pages[0], new Vector2(vector.x, 70f));
		pages[0].subObjects.Add(scoreKeeper);
	}

	protected override void AddBkgIllustration()
	{
		SlugcatSelectMenu.SaveGameData saveGameData = SlugcatSelectMenu.MineForSaveData(manager, ModManager.MSC ? RainWorld.lastActiveSaveSlot : SlugcatStats.Name.Red);
		if (saveGameData != null && saveGameData.ascended && (!ModManager.MSC || RainWorld.lastActiveSaveSlot != MoreSlugcatsEnums.SlugcatStatsName.Saint))
		{
			scene = new InteractiveMenuScene(this, pages[0], MenuScene.SceneID.Red_Ascend);
			pages[0].subObjects.Add(scene);
		}
		else if (ModManager.MSC && RainWorld.lastActiveSaveSlot != SlugcatStats.Name.Red)
		{
			scene = new InteractiveMenuScene(this, pages[0], MenuScene.SceneID.SleepScreen);
			pages[0].subObjects.Add(scene);
		}
		else
		{
			scene = new InteractiveMenuScene(this, pages[0], MenuScene.SceneID.RedsDeathStatisticsBkg);
			pages[0].subObjects.Add(scene);
		}
	}

	protected void TickerIsDone(Ticker ticker)
	{
		if (ticker.ID == TickerID.Food)
		{
			scoreKeeper.AddScoreAdder(ticker.getToValue, 1);
		}
		else if (ticker.ID == TickerID.Survives)
		{
			scoreKeeper.AddScoreAdder(ticker.getToValue, 10);
		}
		else if (ticker.ID == TickerID.Deaths)
		{
			scoreKeeper.AddScoreAdder(ticker.getToValue, -3);
		}
		else if (ticker.ID == TickerID.Quits)
		{
			scoreKeeper.AddScoreAdder(ticker.getToValue, -3);
		}
		else if (ticker.ID == TickerID.Time)
		{
			scoreKeeper.AddScoreAdder(ticker.getToValue / 60, -1);
		}
		else if (ticker.ID == TickerID.Payload)
		{
			scoreKeeper.AddScoreAdder(100, 1);
		}
		else if (ticker.ID == TickerID.FivePebbles)
		{
			scoreKeeper.AddScoreAdder(40, 1);
		}
		else if (ticker.ID == TickerID.Ascension)
		{
			scoreKeeper.AddScoreAdder(300, 1);
		}
		else if (ticker.ID == TickerID.Kill)
		{
			IconSymbol.IconSymbolData iconData = (ticker as KillsTable.KillTicker).symbol.iconData;
			int num = GetNonSandboxKillscore(iconData.critType);
			if (num == 0)
			{
				num = killScores[(int)MultiplayerUnlocks.SandboxUnlockForSymbolData(iconData)];
			}
			scoreKeeper.AddScoreAdder(num, ticker.getToValue);
		}
		else if (ModManager.MSC)
		{
			if (ticker.ID == MoreSlugcatsEnums.TickerID.MetMoon)
			{
				scoreKeeper.AddScoreAdder(ticker.getToValue, 40);
			}
			else if (ticker.ID == MoreSlugcatsEnums.TickerID.MetPebbles)
			{
				scoreKeeper.AddScoreAdder(ticker.getToValue, 40);
			}
			else if (ticker.ID == MoreSlugcatsEnums.TickerID.PearlsRead)
			{
				scoreKeeper.AddScoreAdder(ticker.getToValue, 20);
			}
			else if (ticker.ID == MoreSlugcatsEnums.TickerID.GourmandQuestFinished)
			{
				scoreKeeper.AddScoreAdder(ticker.getToValue, 300);
			}
			else if (ticker.ID == MoreSlugcatsEnums.TickerID.FriendsSaved)
			{
				scoreKeeper.AddScoreAdder(ticker.getToValue, 15);
			}
		}
	}

	public override void Update()
	{
		base.Update();
		if (karmaLadder == null || !karmaLadder.AllAnimationDone || !initiated || allDone)
		{
			return;
		}
		counter++;
		if (tickersDone)
		{
			if (scoreKeeper.scoreAdders.Count != 0)
			{
				return;
			}
			if (!anyMultipliers)
			{
				allDone = true;
			}
			else
			{
				if (scoreKeeper.multiAlpha != 1f)
				{
					return;
				}
				if (delay > 0)
				{
					delay--;
				}
				else if (addPassageDelay > 0)
				{
					addPassageDelay--;
					if (addPassageDelay == 0)
					{
						scoreKeeper.AddMulti();
						delay = 60;
					}
				}
				else if (passagesBump < karmaLadder.endGameMeters.Count)
				{
					FindNextPassageToBump();
					if (passagesBump > -1 && passagesBump < karmaLadder.endGameMeters.Count)
					{
						karmaLadder.endGameMeters[passagesBump].ScoreFlash();
						addPassageDelay = 40;
					}
					else
					{
						scoreKeeper.applyMulti = true;
						delay = 60;
					}
				}
			}
		}
		else if (delay > 0)
		{
			if (tickMode != TickMode.OnlyScore)
			{
				delay--;
				if (delay == 12)
				{
					allTickers[dispIndex].Show();
				}
			}
		}
		else
		{
			if (counter % 4 != 0 && !allTickers[dispIndex].FastTick && !FastMode)
			{
				return;
			}
			allTickers[dispIndex].Tick();
			if (allTickers[dispIndex].displayValue >= allTickers[dispIndex].getToValue)
			{
				TickerIsDone(allTickers[dispIndex]);
				dispIndex++;
				delay = 24;
				if (dispIndex >= allTickers.Count)
				{
					tickersDone = true;
					delay = 40;
					tickMode = TickMode.OnlyScore;
				}
				else if (tickMode == TickMode.OnlyTicker && allTickers[dispIndex] is KillsTable.KillTicker)
				{
					tickMode = TickMode.OnlyScore;
				}
			}
		}
	}

	private void FindNextPassageToBump()
	{
		passagesBump++;
		for (int i = passagesBump; i <= karmaLadder.endGameMeters.Count; i++)
		{
			if (i == karmaLadder.endGameMeters.Count)
			{
				passagesBump = i;
				break;
			}
			if (karmaLadder.endGameMeters[i].tracker.GoalFullfilled)
			{
				passagesBump = i;
				break;
			}
		}
	}

	public static int GetNonSandboxKillscore(CreatureTemplate.Type critType)
	{
		if (ModManager.MSC)
		{
			if (critType == MoreSlugcatsEnums.CreatureTemplateType.HunterDaddy)
			{
				return 14;
			}
			if (critType == MoreSlugcatsEnums.CreatureTemplateType.ScavengerKing)
			{
				return 25;
			}
			if (critType == MoreSlugcatsEnums.CreatureTemplateType.TrainLizard)
			{
				return 25;
			}
		}
		return 0;
	}

	public override void Singal(MenuObject sender, string message)
	{
		if (message != null && message == "CONTINUE")
		{
			manager.RequestMainProcessSwitch(ProcessManager.ProcessID.SlugcatSelect);
			PlaySound(SoundID.MENU_Switch_Page_Out);
		}
	}

	public override void CommunicateWithUpcomingProcess(MainLoopProcess nextProcess)
	{
		base.CommunicateWithUpcomingProcess(nextProcess);
		if (nextProcess is SlugcatSelectMenu && (!ModManager.MSC || RainWorld.lastActiveSaveSlot == SlugcatStats.Name.Red))
		{
			(nextProcess as SlugcatSelectMenu).ComingFromRedsStatistics();
		}
		if (ModManager.MSC && nextProcess is SlugcatSelectMenu && RainWorld.lastActiveSaveSlot == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
		{
			(nextProcess as SlugcatSelectMenu).ComingFromArtificerStatistics();
		}
	}
}
