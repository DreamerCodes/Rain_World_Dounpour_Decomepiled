using UnityEngine;

namespace Menu;

public class FinalResultbox : PlayerResultBox
{
	public SymbolAndLabel winsSymbol;

	public SymbolAndLabel scoreSymbol;

	public SymbolAndLabel deathsSymbol;

	public bool winsSymbolTakePlayerLabelColor;

	public override float AvailableXDistForTrophies => 471f;

	public override bool DeadPortraint => false;

	public override float KillsSymbolXpos => 320f;

	public FinalResultbox(MultiplayerResults resultPage, MenuObject owner, ArenaSitting.ArenaPlayer player, int index)
		: base(resultPage, owner, new Vector2(-1000f, -1000f), new Vector2(966f, 100f), player, index)
	{
		winsSymbol = new SymbolAndLabel(menu, this, new Vector2(135f, 23f), "Multiplayer_Star", " ~ 0", -39f);
		subObjects.Add(winsSymbol);
		scoreSymbol = new SymbolAndLabel(menu, this, new Vector2(245f, 23f), "Multiplayer_Bat", " ~ 0", -35f);
		subObjects.Add(scoreSymbol);
		deathsSymbol = new SymbolAndLabel(menu, this, new Vector2(355f, 23f), "Multiplayer_Death", " ~ 0", -35f);
		subObjects.Add(deathsSymbol);
		showAsAlive = true;
		BumpAndReveal();
	}

	public override void Update()
	{
		base.Update();
		winsSymbol.litUp = winsSymbol.displayValue > -1 && !winsSymbol.countedAndDone;
		if (winsSymbol.displayValue > -1)
		{
			if (winsSymbol.displayValue < player.wins)
			{
				if ((menu as PlayerResultMenu).NumbersTick)
				{
					winsSymbol.Tick();
					(menu as PlayerResultMenu).PlaySingleSound(SoundID.UI_Multiplayer_Player_Result_Box_Number_Tick);
				}
			}
			else
			{
				winsSymbol.countedAndDone = true;
			}
			winsSymbol.ChangeLabelText(" ~ " + winsSymbol.displayValue);
		}
		if (scoreSymbol.displayValue > -1)
		{
			if (scoreSymbol.displayValue < player.totScore)
			{
				if ((menu as PlayerResultMenu).NumbersTick)
				{
					scoreSymbol.Tick();
					(menu as PlayerResultMenu).PlaySingleSound(SoundID.UI_Multiplayer_Player_Result_Box_Number_Tick);
				}
			}
			else
			{
				scoreSymbol.countedAndDone = true;
			}
			scoreSymbol.ChangeLabelText(" ~ " + scoreSymbol.displayValue);
		}
		scoreSymbol.litUp = scoreSymbol.displayValue > -1 && !scoreSymbol.countedAndDone;
		if (deathsSymbol.displayValue > -1)
		{
			if (deathsSymbol.displayValue < player.deaths)
			{
				if ((menu as PlayerResultMenu).NumbersTick)
				{
					deathsSymbol.Tick();
					(menu as PlayerResultMenu).PlaySingleSound(SoundID.UI_Multiplayer_Player_Result_Box_Number_Tick);
				}
			}
			else
			{
				deathsSymbol.countedAndDone = true;
			}
			deathsSymbol.ChangeLabelText(" ~ " + deathsSymbol.displayValue);
		}
		deathsSymbol.litUp = deathsSymbol.displayValue > -1 && !deathsSymbol.countedAndDone;
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
	}

	public override void BumpAndReveal()
	{
		base.BumpAndReveal();
		showAsAlive = true;
	}

	public override void RemoveSprites()
	{
		base.RemoveSprites();
	}
}
