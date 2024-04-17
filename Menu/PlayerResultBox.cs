using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace Menu;

public abstract class PlayerResultBox : RectangularMenuObject
{
	public class SymbolAndLabel : PositionedMenuObject
	{
		public Symbol symbol;

		public MenuLabel menuLabel;

		public float lastLightUp;

		public float lightUp;

		public int counter;

		public bool litUp;

		public bool countedAndDone;

		public int displayValue { get; private set; }

		public float CurrLightUp(float timeStacker)
		{
			return ((counter % 8 < 4) ? 0.75f : 1f) * Mathf.Lerp(lastLightUp, lightUp, timeStacker);
		}

		public void ChangeLabelText(string newString)
		{
			menuLabel.text = newString;
		}

		public void Tick()
		{
			Tick(1);
		}

		public void Tick(int tickAdd)
		{
			if (tickAdd != 0)
			{
				displayValue += tickAdd;
			}
		}

		public void Set(int setTo)
		{
			if (displayValue != setTo)
			{
				displayValue = setTo;
			}
		}

		public void Start()
		{
			displayValue = 0;
		}

		public SymbolAndLabel(Menu menu, MenuObject owner, Vector2 pos, string symbolName, string initialLabel, float labelXOffset)
			: base(menu, owner, pos)
		{
			symbol = new Symbol(menu, this, new Vector2(0f, 0f), symbolName);
			if (symbolName == "Multiplayer_Bats")
			{
				symbol.pos.y += 2f;
			}
			subObjects.Add(symbol);
			menuLabel = new MenuLabel(menu, this, initialLabel, new Vector2(labelXOffset, -9f), new Vector2(100f, 20f), bigText: true);
			menuLabel.label.alignment = FLabelAlignment.Left;
			subObjects.Add(menuLabel);
			displayValue = -1;
		}

		public override void Update()
		{
			base.Update();
			lastLightUp = lightUp;
			lightUp = Custom.LerpAndTick(lightUp, litUp ? 1f : 0f, 0.06f, 1f / 30f);
			if (lightUp > 0f)
			{
				counter++;
			}
		}

		public override void GrafUpdate(float timeStacker)
		{
			base.GrafUpdate(timeStacker);
			Color a = ((owner is PlayerResultBox) ? (owner as PlayerResultBox).UseTextBaseColor(timeStacker) : Menu.MenuRGB(Menu.MenuColors.MediumGrey));
			menuLabel.label.color = Color.Lerp(a, Menu.MenuRGB(Menu.MenuColors.White), CurrLightUp(timeStacker));
			symbol.symbolSprite.color = Color.Lerp(a, Menu.MenuRGB(Menu.MenuColors.White), CurrLightUp(timeStacker));
		}
	}

	public class Symbol : PositionedMenuObject
	{
		public FSprite symbolSprite;

		public Symbol(Menu menu, MenuObject owner, Vector2 pos, string symbolName)
			: base(menu, owner, pos)
		{
			symbolSprite = new FSprite(symbolName);
			Container.AddChild(symbolSprite);
			symbolSprite.color = Menu.MenuRGB(Menu.MenuColors.MediumGrey);
		}

		public override void GrafUpdate(float timeStacker)
		{
			base.GrafUpdate(timeStacker);
			symbolSprite.x = DrawX(timeStacker);
			symbolSprite.y = DrawY(timeStacker);
		}

		public override void RemoveSprites()
		{
			symbolSprite.RemoveFromContainer();
			base.RemoveSprites();
		}
	}

	public class KillTrophy : PositionedMenuObject
	{
		public CreatureSymbol symbol;

		public Vector2 relativePos;

		public bool collapsed;

		public bool prevCollapsed;

		public float graphWidth => symbol.graphWidth;

		public float MyWidth(bool compress)
		{
			if (compress && collapsed)
			{
				return 7f;
			}
			return graphWidth + 5f;
		}

		public KillTrophy(Menu menu, MenuObject owner, Vector2 pos, IconSymbol.IconSymbolData creatureSymbolData)
			: base(menu, owner, pos)
		{
			symbol = new CreatureSymbol(creatureSymbolData, Container);
		}

		public override void Update()
		{
			base.Update();
			symbol.Update();
		}

		public void Show()
		{
			symbol.Show(prevCollapsed);
		}

		public override void GrafUpdate(float timeStacker)
		{
			base.GrafUpdate(timeStacker);
			Vector2 drawPos = DrawPos(timeStacker);
			if (prevCollapsed)
			{
				drawPos.x += graphWidth / 2f * Mathf.Lerp(symbol.lastShowFlash, symbol.showFlash, timeStacker);
			}
			symbol.Draw(timeStacker, drawPos);
		}

		public override void RemoveSprites()
		{
			symbol.RemoveSprites();
			base.RemoveSprites();
		}
	}

	public RoundedRect backgroundRect;

	public RoundedRect extraRect;

	public MenuIllustration portrait;

	public ArenaSitting.ArenaPlayer player;

	public MenuLabel playerNameLabel;

	public SymbolAndLabel killsSymbol;

	public int counter;

	public int index;

	public bool showAsAlive;

	public bool showAsWinner;

	public float textWhite;

	public float lastTextWhite;

	public float bump;

	public float lastBump;

	public float readyForNext;

	public List<KillTrophy> killTrophies;

	public Vector2 originalSize;

	private float rightMostKillSymbol;

	private float rightMostKillSymbolWidth;

	private int killsymbolRows;

	public virtual float KillsSymbolXpos => 245f;

	public virtual bool DeadPortraint => false;

	public virtual float AvailableXDistForTrophies => 0f;

	public float AllBoxesInPlaceFac(float timeStacker)
	{
		return (menu as PlayerResultMenu).AllBoxesInPlaceFac(timeStacker);
	}

	public virtual Vector2 IdealPos()
	{
		return new Vector2((menu as PlayerResultMenu).topMiddle.x - size.x / 2f + 0.01f, (menu as PlayerResultMenu).topMiddle.y - 120f * (float)index);
	}

	public PlayerResultBox(Menu menu, MenuObject owner, Vector2 pos, Vector2 size, ArenaSitting.ArenaPlayer player, int index)
		: base(menu, owner, pos, size)
	{
		this.player = player;
		this.index = index;
		originalSize = size;
		backgroundRect = new RoundedRect(menu, this, new Vector2(0.01f, 0.01f), size, filled: true);
		subObjects.Add(backgroundRect);
		if (player.winner)
		{
			extraRect = new RoundedRect(menu, this, new Vector2(0.01f, 0.01f), size, filled: true);
			subObjects.Add(extraRect);
		}
		if (ModManager.MSC)
		{
			int num = player.playerNumber;
			if (menu.manager.arenaSitting.gameTypeSetup.gameType == MoreSlugcatsEnums.GameTypeID.Challenge)
			{
				num = ((!(player.playerClass == SlugcatStats.Name.White)) ? ((player.playerClass == SlugcatStats.Name.Yellow) ? 1 : ((!(player.playerClass == SlugcatStats.Name.Red)) ? 4 : 2)) : 0);
			}
			portrait = new MenuIllustration(menu, this, "", "MultiplayerPortrait" + num + (DeadPortraint ? "0" : "1") + "-" + player.playerClass.value, new Vector2(size.y / 2f, size.y / 2f), crispPixels: true, anchorCenter: true);
		}
		else
		{
			portrait = new MenuIllustration(menu, this, "", "MultiplayerPortrait" + player.playerNumber + (DeadPortraint ? "0" : "1"), new Vector2(size.y / 2f, size.y / 2f), crispPixels: true, anchorCenter: true);
		}
		subObjects.Add(portrait);
		playerNameLabel = new MenuLabel(menu, this, menu.manager.rainWorld.inGameTranslator.Translate("Player") + " " + (player.playerNumber + 1), new Vector2(75f, 57f), new Vector2(100f, 20f), bigText: true);
		playerNameLabel.label.alignment = FLabelAlignment.Left;
		subObjects.Add(playerNameLabel);
		killsSymbol = new SymbolAndLabel(menu, this, new Vector2(145f + KillsSymbolXpos, 23f), "Multiplayer_Bones", " ~", -39f);
		subObjects.Add(killsSymbol);
		killTrophies = new List<KillTrophy>();
	}

	public string SecondsToMinutesAndSecondsString(int secs)
	{
		int num = Mathf.FloorToInt((float)secs / 60f);
		secs -= num * 60;
		return num.ToString("D2") + ":" + secs.ToString("D2");
	}

	public override void Update()
	{
		base.Update();
		counter++;
		lastBump = bump;
		lastTextWhite = textWhite;
		backgroundRect.addSize = new Vector2(20.01f, 10.01f) * bump;
		if (showAsWinner)
		{
			extraRect.addSize = backgroundRect.addSize;
			backgroundRect.addSize += new Vector2(10f, 10f) * Mathf.Pow(Mathf.InverseLerp(0f, 0.5f, AllBoxesInPlaceFac(1f)), 0.3f);
		}
		readyForNext = Custom.LerpAndTick(readyForNext, player.readyForNextRound ? 1f : 0f, 0.09f, 0.05f);
		pos = IdealPos();
		bool flag = this is ArenaOverlayResultBox && (this as ArenaOverlayResultBox).showWinnerStar;
		for (int i = 0; i < killTrophies.Count; i++)
		{
			killTrophies[i].pos.x = Custom.LerpMap(killTrophies[i].relativePos.x, 0f, rightMostKillSymbol, killsSymbol.pos.x + 40f, size.x - rightMostKillSymbolWidth / 2f - (flag ? 150f : 20f));
			killTrophies[i].pos.y = killsSymbol.pos.y + killTrophies[i].relativePos.y;
		}
		if (killsSymbol.displayValue > -1 && (menu as PlayerResultMenu).KillsTick)
		{
			if (killTrophies.Count > 0)
			{
				if (killsSymbol.displayValue < killTrophies.Count)
				{
					killTrophies[killsSymbol.displayValue].Show();
					killsSymbol.Tick();
					(menu as PlayerResultMenu).PlaySingleSound(SoundID.UI_Multiplayer_Player_Result_Box_Kill_Tick);
				}
				else
				{
					killsSymbol.countedAndDone = true;
				}
			}
			else
			{
				killsSymbol.countedAndDone = true;
			}
		}
		killsSymbol.litUp = killsSymbol.displayValue > -1 && !killsSymbol.countedAndDone;
	}

	public static bool CollapseCreatureSymbols(IconSymbol.IconSymbolData A, IconSymbol.IconSymbolData B)
	{
		if (A.critType == CreatureTemplate.Type.Centipede && B.critType == CreatureTemplate.Type.SmallCentipede && A.intData == 1)
		{
			return true;
		}
		if (B.critType == CreatureTemplate.Type.Centipede && A.critType == CreatureTemplate.Type.SmallCentipede && B.intData == 1)
		{
			return true;
		}
		if (A.critType == B.critType)
		{
			if (A.critType == CreatureTemplate.Type.Slugcat || A.critType == CreatureTemplate.Type.Centipede)
			{
				return A.intData == B.intData;
			}
			return true;
		}
		return false;
	}

	public virtual void BumpAndReveal()
	{
		showAsWinner = player.winner;
		owner.menu.PlaySound(SoundID.UI_Multiplayer_Player_Result_Box_Bump);
		List<IconSymbol.IconSymbolData> list = ((this is ArenaOverlayResultBox) ? player.roundKills : player.allKills);
		for (int i = 0; i < list.Count; i++)
		{
			killTrophies.Add(new KillTrophy(menu, this, default(Vector2), list[i]));
			subObjects.Add(killTrophies[killTrophies.Count - 1]);
		}
		for (int j = 0; j < killTrophies.Count - 1; j++)
		{
			if (CollapseCreatureSymbols(killTrophies[j].symbol.iconData, killTrophies[j + 1].symbol.iconData))
			{
				killTrophies[j].collapsed = true;
				killTrophies[j + 1].prevCollapsed = true;
			}
		}
		Vector2 vector = new Vector2(0f, 0f);
		killsymbolRows = 0;
		float availableXDistForTrophies = AvailableXDistForTrophies;
		for (int k = 0; k < killTrophies.Count; k++)
		{
			killTrophies[k].relativePos.x = killTrophies[k].graphWidth / 2f + vector.x;
			killTrophies[k].relativePos.y = vector.y;
			vector.x += killTrophies[k].MyWidth(compress: true);
			if (vector.x + ((k < killTrophies.Count - 1) ? killTrophies[k + 1].MyWidth(compress: false) : 0f) > availableXDistForTrophies)
			{
				vector.x = 0f;
				vector.y += 25f;
				killsymbolRows++;
			}
			if (killTrophies[k].relativePos.x + killTrophies[k].graphWidth / 2f > rightMostKillSymbol)
			{
				rightMostKillSymbol = killTrophies[k].relativePos.x + killTrophies[k].graphWidth / 2f;
				rightMostKillSymbolWidth = killTrophies[k].graphWidth;
			}
		}
		if (killsymbolRows == 0 && killTrophies.Count > 0)
		{
			rightMostKillSymbol = availableXDistForTrophies;
			rightMostKillSymbolWidth = killTrophies[killTrophies.Count - 1].graphWidth;
		}
	}

	protected Color UseTextBaseColor(float timeStacker)
	{
		float t = Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(lastTextWhite, textWhite, timeStacker)), 2f);
		return Color.Lerp(showAsAlive ? Color.Lerp(Menu.MenuRGB(Menu.MenuColors.DarkGrey), Menu.MenuRGB(Menu.MenuColors.MediumGrey), AllBoxesInPlaceFac(timeStacker)) : Menu.MenuRGB(Menu.MenuColors.DarkGrey), Menu.MenuRGB(showAsAlive ? Menu.MenuColors.White : Menu.MenuColors.MediumGrey), t);
	}

	protected float UseWinnerColor(float timeStacker)
	{
		return (showAsWinner ? (0.5f + 0.5f * Mathf.Sin(((float)counter + timeStacker) / 10f)) : 0f) * AllBoxesInPlaceFac(timeStacker);
	}

	protected float UseTextWhite(float timeStacker)
	{
		return Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(lastTextWhite, textWhite, timeStacker)), 2f);
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		float num = UseWinnerColor(timeStacker);
		float num2 = UseTextWhite(timeStacker);
		Color a = UseTextBaseColor(timeStacker);
		portrait.sprite.color = Color.Lerp(Color.black, Color.white, showAsAlive ? 1f : (0.25f + Mathf.Max(0.75f * num, 0.25f * num2)));
		playerNameLabel.label.color = Color.Lerp(a, PlayerGraphics.DefaultSlugcatColor(SlugcatStats.Name.ArenaColor(player.playerNumber)), num);
		if (this is FinalResultbox && (this as FinalResultbox).winsSymbolTakePlayerLabelColor)
		{
			(this as FinalResultbox).winsSymbol.symbol.symbolSprite.color = Color.Lerp(a, PlayerGraphics.DefaultSlugcatColor(SlugcatStats.Name.ArenaColor(player.playerNumber)), num);
			(this as FinalResultbox).winsSymbol.menuLabel.label.color = Color.Lerp(a, PlayerGraphics.DefaultSlugcatColor(SlugcatStats.Name.ArenaColor(player.playerNumber)), num);
		}
		for (int i = 9; i < 17; i++)
		{
			backgroundRect.sprites[i].color = Color.Lerp(Color.Lerp(Menu.MenuRGB(Menu.MenuColors.VeryDarkGrey), Menu.MenuRGB(Menu.MenuColors.DarkGrey), num), Menu.MenuRGB(Menu.MenuColors.MediumGrey), textWhite);
		}
		for (int j = 0; j < 9; j++)
		{
			backgroundRect.sprites[j].alpha = 0.7f;
		}
		if (extraRect != null)
		{
			float alpha = Mathf.Pow(AllBoxesInPlaceFac(timeStacker), 2f);
			for (int k = 9; k < 17; k++)
			{
				extraRect.sprites[k].color = Color.Lerp(Color.Lerp(Menu.MenuRGB(Menu.MenuColors.VeryDarkGrey), Menu.MenuRGB(Menu.MenuColors.DarkGrey), num), Menu.MenuRGB(Menu.MenuColors.MediumGrey), textWhite);
				extraRect.sprites[k].alpha = alpha;
			}
			for (int l = 0; l < 9; l++)
			{
				extraRect.sprites[l].alpha = 0f;
			}
		}
	}
}
