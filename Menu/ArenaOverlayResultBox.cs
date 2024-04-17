using System;
using RWCustom;
using UnityEngine;

namespace Menu;

public class ArenaOverlayResultBox : PlayerResultBox
{
	public SymbolAndLabel scoreSymbol;

	public SymbolAndLabel timeSymbol;

	public SymbolAndLabel winSymbol;

	public FSprite winSymbolGlow;

	public float inPlaceF;

	public float lastInPlaceF;

	public bool inPlace;

	public bool showWinnerStar;

	public override bool DeadPortraint => !player.alive;

	public override float KillsSymbolXpos => 245f;

	public override float AvailableXDistForTrophies
	{
		get
		{
			if (!showWinnerStar)
			{
				return 350f;
			}
			return 220f;
		}
	}

	public float InPlace(float timeStacker)
	{
		return Mathf.Lerp(lastInPlaceF, inPlaceF, timeStacker);
	}

	public override Vector2 IdealPos()
	{
		return new Vector2(base.IdealPos().x + 40f * readyForNext, Mathf.Lerp(-140f, base.IdealPos().y, inPlaceF));
	}

	public ArenaOverlayResultBox(ArenaOverlay arenaOverlay, MenuObject owner, ArenaSitting.ArenaPlayer player, int index, bool showWinnerStar)
		: base(arenaOverlay, owner, new Vector2(-1000f, -1000f), new Vector2(800f, 100f), player, index)
	{
		this.showWinnerStar = showWinnerStar;
		scoreSymbol = new SymbolAndLabel(menu, this, new Vector2(140f, 23f), (arenaOverlay.ArenaSitting.gameTypeSetup.gameType == ArenaSetup.GameTypeID.Competitive) ? "Multiplayer_Bat" : "Multiplayer_Star", " ~ 0", -35f);
		subObjects.Add(scoreSymbol);
		timeSymbol = new SymbolAndLabel(menu, this, new Vector2(250f, 23f), "Multiplayer_Time", " ~ 00:00", -39f);
		subObjects.Add(timeSymbol);
		if (showWinnerStar)
		{
			winSymbol = new SymbolAndLabel(menu, this, new Vector2(size.x - 30f, 23f), "Multiplayer_Star", menu.manager.rainWorld.inGameTranslator.Translate("Winner!"), -75f);
			winSymbol.menuLabel.label.alignment = FLabelAlignment.Right;
			winSymbol.symbol.symbolSprite.alpha = 0f;
			winSymbol.menuLabel.label.alpha = 0f;
			subObjects.Add(winSymbol);
			winSymbolGlow = new FSprite("Futile_White");
			winSymbolGlow.shader = menu.manager.rainWorld.Shaders["FlatLight"];
			Container.AddChild(winSymbolGlow);
		}
	}

	public override void Update()
	{
		base.Update();
		bump = Mathf.Min(Mathf.InverseLerp(1f, 5f, (menu as PlayerResultMenu).allResultBoxesInPlaceCounter - 6 * index), Mathf.InverseLerp(12f, 7f, (menu as PlayerResultMenu).allResultBoxesInPlaceCounter - 6 * index));
		size.x = originalSize.x - readyForNext * 20f;
		backgroundRect.size = size;
		if (extraRect != null)
		{
			extraRect.size = size;
		}
		if ((menu as PlayerResultMenu).allResultBoxesInPlaceCounter - 6 * index == 5)
		{
			textWhite = 1f;
		}
		else
		{
			textWhite = Custom.LerpAndTick(textWhite, 0f, 0.01f, 1f / 60f);
		}
		if ((menu as PlayerResultMenu).allResultBoxesInPlaceCounter - 6 * index == 6)
		{
			BumpAndReveal();
		}
		lastInPlaceF = inPlaceF;
		inPlaceF = Mathf.InverseLerp(10 * ((menu as PlayerResultMenu).result.Count - 1), 10 * ((menu as PlayerResultMenu).result.Count - 1) + 20, counter - 10 * index);
		inPlaceF = Mathf.Pow(Custom.SCurve(inPlaceF, 2f), 0.6f);
		if (lastInPlaceF == 1f && !inPlace)
		{
			inPlace = true;
			(menu as ArenaOverlay).ResultBoxInPlace();
		}
		if (winSymbol != null)
		{
			winSymbol.pos.x = size.x - 30f;
			winSymbol.lightUp = UseWinnerColor(1f);
		}
		if (scoreSymbol.displayValue > -1)
		{
			if (scoreSymbol.displayValue < Math.Abs(player.score))
			{
				if ((menu as PlayerResultMenu).NumbersTick)
				{
					scoreSymbol.Tick((Math.Abs(scoreSymbol.displayValue - Math.Abs(player.score)) < 30) ? 1 : 5);
					(menu as PlayerResultMenu).PlaySingleSound(SoundID.UI_Multiplayer_Player_Result_Box_Number_Tick);
				}
			}
			else
			{
				scoreSymbol.countedAndDone = true;
			}
			scoreSymbol.ChangeLabelText(" ~ " + scoreSymbol.displayValue * Math.Sign(player.score));
		}
		scoreSymbol.litUp = scoreSymbol.displayValue > -1 && !scoreSymbol.countedAndDone;
		if (timeSymbol.displayValue > -1)
		{
			if (timeSymbol.displayValue == player.timeAlive / 40)
			{
				timeSymbol.countedAndDone = true;
			}
			else if ((menu as PlayerResultMenu).NumbersTick)
			{
				timeSymbol.Set(Math.Min(player.timeAlive / 40, timeSymbol.displayValue + 3));
				(menu as PlayerResultMenu).PlaySingleSound(SoundID.UI_Multiplayer_Player_Result_Box_Number_Tick);
			}
			timeSymbol.ChangeLabelText(" ~ " + SecondsToMinutesAndSecondsString(timeSymbol.displayValue));
		}
		timeSymbol.litUp = timeSymbol.displayValue > -1 && !timeSymbol.countedAndDone;
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		float num = AllBoxesInPlaceFac(timeStacker);
		float num2 = UseWinnerColor(timeStacker);
		Color a = UseTextBaseColor(timeStacker);
		if (winSymbol != null)
		{
			winSymbol.menuLabel.label.alpha = num;
			winSymbol.symbol.symbolSprite.alpha = num;
			winSymbolGlow.x = winSymbol.DrawX(timeStacker);
			winSymbolGlow.y = winSymbol.DrawY(timeStacker);
			winSymbolGlow.scale = Mathf.Lerp(45f, 50f, num2) / 16f;
			winSymbolGlow.alpha = Mathf.Lerp(0.2f, 0.3f, num2) * num;
			winSymbolGlow.color = Color.Lerp(a, Menu.MenuRGB(Menu.MenuColors.White), 0.5f + 0.5f * num2);
		}
	}

	public override void BumpAndReveal()
	{
		base.BumpAndReveal();
		if ((menu as ArenaOverlay).phase == ArenaOverlay.Phase.Init)
		{
			(menu as ArenaOverlay).phase = ArenaOverlay.Phase.BumpAndCountScore;
		}
		scoreSymbol.Start();
		showAsAlive = player.alive;
	}

	public override void RemoveSprites()
	{
		if (winSymbolGlow != null)
		{
			winSymbolGlow.RemoveFromContainer();
		}
		base.RemoveSprites();
	}
}
