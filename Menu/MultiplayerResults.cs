using UnityEngine;

namespace Menu;

public class MultiplayerResults : PlayerResultMenu
{
	public class Phase : ExtEnum<Phase>
	{
		public static readonly Phase Init = new Phase("Init", register: true);

		public static readonly Phase CountWins = new Phase("CountWins", register: true);

		public static readonly Phase CountScore = new Phase("CountScore", register: true);

		public static readonly Phase CountDeaths = new Phase("CountDeaths", register: true);

		public static readonly Phase CountKills = new Phase("CountKills", register: true);

		public static readonly Phase Done = new Phase("Done", register: true);

		public Phase(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public ArenaSetup.GameTypeID currentGameType;

	public Phase phase = Phase.Init;

	public MenuLabel headingLabel;

	public SimpleButton continueButton;

	public ArenaSetup GetArenaSetup => manager.arenaSetup;

	public ArenaSetup.GameTypeSetup GetGameTypeSetup => GetArenaSetup.GetOrInitiateGameTypeSetup(currentGameType);

	public MultiplayerResults(ProcessManager manager)
		: base(manager, manager.arenaSitting, manager.arenaSitting.FinalSittingResult(), ProcessManager.ProcessID.MultiplayerResults)
	{
		pages.Add(new Page(this, null, "main", 0));
		continueButton = new SimpleButton(this, pages[0], Translate("CONTINUE"), "CONTINUE", new Vector2(1056f, 50f), new Vector2(110f, 30f));
		pages[0].subObjects.Add(continueButton);
		headingLabel = new MenuLabel(this, pages[0], (result.Count < 2 || result[0].winner) ? Translate("SESSION RESULTS") : Translate("IT'S A DRAW!"), topMiddle + new Vector2(-150f, 120f), new Vector2(300f, 40f), bigText: true);
		pages[0].subObjects.Add(headingLabel);
		headingLabel.label.color = Menu.MenuRGB(MenuColors.MediumGrey);
		for (int num = result.Count - 1; num >= 0; num--)
		{
			resultBoxes.Add(new FinalResultbox(this, pages[0], result[num], num));
			pages[0].subObjects.Add(resultBoxes[resultBoxes.Count - 1]);
		}
		allResultBoxesInPlaceCounter = 0;
		mySoundLoopID = SoundID.MENU_Main_Menu_LOOP;
	}

	public override void Update()
	{
		base.Update();
		continueButton.buttonBehav.greyedOut = counter < 80;
		bool flag = false;
		if (phase == Phase.Init)
		{
			if (counter > 40)
			{
				phase = Phase.CountWins;
				for (int i = 0; i < resultBoxes.Count; i++)
				{
					(resultBoxes[i] as FinalResultbox).winsSymbol.Start();
				}
			}
		}
		else if (phase == Phase.CountWins)
		{
			for (int j = 0; j < resultBoxes.Count; j++)
			{
				if (!(resultBoxes[j] as FinalResultbox).winsSymbol.countedAndDone)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				phase = Phase.CountScore;
				for (int k = 0; k < resultBoxes.Count; k++)
				{
					(resultBoxes[k] as FinalResultbox).scoreSymbol.Start();
				}
			}
		}
		else if (phase == Phase.CountScore)
		{
			for (int l = 0; l < resultBoxes.Count; l++)
			{
				if (!(resultBoxes[l] as FinalResultbox).scoreSymbol.countedAndDone)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				phase = Phase.CountDeaths;
				for (int m = 0; m < resultBoxes.Count; m++)
				{
					(resultBoxes[m] as FinalResultbox).deathsSymbol.Start();
				}
			}
		}
		else if (phase == Phase.CountDeaths)
		{
			for (int n = 0; n < resultBoxes.Count; n++)
			{
				if (!(resultBoxes[n] as FinalResultbox).deathsSymbol.countedAndDone)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				phase = Phase.CountKills;
				for (int num = 0; num < resultBoxes.Count; num++)
				{
					(resultBoxes[num] as FinalResultbox).killsSymbol.Start();
				}
			}
		}
		else
		{
			if (!(phase == Phase.CountKills))
			{
				return;
			}
			for (int num2 = 0; num2 < resultBoxes.Count; num2++)
			{
				if (!(resultBoxes[num2] as FinalResultbox).killsSymbol.countedAndDone)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				for (int num3 = 0; num3 < resultBoxes.Count; num3++)
				{
					(resultBoxes[num3] as FinalResultbox).winsSymbolTakePlayerLabelColor = true;
				}
				phase = Phase.Done;
			}
		}
	}

	public override void Singal(MenuObject sender, string message)
	{
		if (message != null && message == "CONTINUE")
		{
			manager.RequestMainProcessSwitch(ProcessManager.ProcessID.MultiplayerMenu);
			manager.rainWorld.options.DeleteArenaSitting();
			PlaySound(SoundID.MENU_Switch_Page_In);
		}
	}
}
