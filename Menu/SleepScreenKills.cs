using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace Menu;

public class SleepScreenKills : PositionedMenuObject
{
	public PlayerResultBox.SymbolAndLabel crossBonesSymbol;

	public List<PlayerResultBox.KillTrophy> killTrophies;

	private int displayValue;

	private int counter;

	public bool countedAndDone;

	private float rightMostKillSymbol;

	private float rightMostKillSymbolWidth;

	private int killsymbolRows;

	public bool started;

	public int wait = 40;

	private float AvailableXDist => (menu as SleepAndDeathScreen).ContinueAndExitButtonsXPos - pos.x * 2f;

	public SleepScreenKills(Menu menu, MenuObject owner, Vector2 pos, List<PlayerSessionRecord.KillRecord> killsData)
		: base(menu, owner, pos)
	{
		killTrophies = new List<PlayerResultBox.KillTrophy>();
		for (int i = 0; i < killsData.Count; i++)
		{
			if (CreatureSymbol.DoesCreatureEarnATrophy(killsData[i].symbolData.critType))
			{
				killTrophies.Add(new PlayerResultBox.KillTrophy(menu, this, new Vector2(0f, 0f), killsData[i].symbolData));
			}
		}
		for (int j = 0; j < killTrophies.Count; j++)
		{
			subObjects.Add(killTrophies[j]);
		}
		for (int k = 0; k < killTrophies.Count - 1; k++)
		{
			if (PlayerResultBox.CollapseCreatureSymbols(killTrophies[k].symbol.iconData, killTrophies[k + 1].symbol.iconData))
			{
				killTrophies[k].collapsed = true;
				killTrophies[k + 1].prevCollapsed = true;
			}
		}
		Vector2 vector = new Vector2(48f, 0f);
		killsymbolRows = 0;
		for (int l = 0; l < killTrophies.Count; l++)
		{
			killTrophies[l].relativePos.x = killTrophies[l].graphWidth / 2f + vector.x;
			killTrophies[l].relativePos.y = vector.y;
			vector.x += killTrophies[l].MyWidth(compress: true);
			if (vector.x + ((l < killTrophies.Count - 1) ? killTrophies[l + 1].MyWidth(compress: false) : 0f) > AvailableXDist)
			{
				vector.x = 0f;
				vector.y -= 25f;
				killsymbolRows++;
			}
			if (killTrophies[l].relativePos.x + killTrophies[l].graphWidth / 2f > rightMostKillSymbol)
			{
				rightMostKillSymbol = killTrophies[l].relativePos.x + killTrophies[l].graphWidth / 2f;
				rightMostKillSymbolWidth = killTrophies[l].graphWidth;
			}
		}
		if (killsymbolRows == 0 && killTrophies.Count > 0)
		{
			rightMostKillSymbol = AvailableXDist;
			rightMostKillSymbolWidth = killTrophies[killTrophies.Count - 1].graphWidth;
		}
		if (killTrophies.Count < 1)
		{
			countedAndDone = true;
		}
	}

	public override void Update()
	{
		base.Update();
		if (!started || countedAndDone)
		{
			return;
		}
		bool mp = RWInput.PlayerInput(0).mp;
		if (wait > 0)
		{
			if (wait <= 24 && crossBonesSymbol == null)
			{
				menu.PlaySound(SoundID.UI_Multiplayer_Player_Result_Box_Kill_Tick);
				crossBonesSymbol = new PlayerResultBox.SymbolAndLabel(menu, this, new Vector2(10f, 0f), "Multiplayer_Bones", " ~", -39f);
				subObjects.Add(crossBonesSymbol);
			}
			wait -= ((!mp) ? 1 : 4);
			return;
		}
		counter++;
		for (int i = 0; i < killTrophies.Count; i++)
		{
			killTrophies[i].pos.x = Custom.LerpMap(killTrophies[i].relativePos.x, 0f, rightMostKillSymbol, 0f, AvailableXDist - rightMostKillSymbolWidth / 2f);
			killTrophies[i].pos.y = killTrophies[i].relativePos.y;
		}
		if (displayValue <= -1 || !(counter % 8 == 0 || mp))
		{
			return;
		}
		if (killTrophies.Count > 0)
		{
			if (displayValue < killTrophies.Count)
			{
				menu.PlaySound(SoundID.UI_Multiplayer_Player_Result_Box_Kill_Tick);
				killTrophies[displayValue].Show();
			}
			else
			{
				countedAndDone = true;
			}
			displayValue++;
		}
		else
		{
			countedAndDone = true;
		}
	}
}
