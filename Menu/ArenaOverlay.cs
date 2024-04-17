using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace Menu;

public class ArenaOverlay : PlayerResultMenu
{
	public class Phase : ExtEnum<Phase>
	{
		public static readonly Phase Init = new Phase("Init", register: true);

		public static readonly Phase BumpAndCountScore = new Phase("BumpAndCountScore", register: true);

		public static readonly Phase CountTime = new Phase("CountTime", register: true);

		public static readonly Phase CountKills = new Phase("CountKills", register: true);

		public static readonly Phase Done = new Phase("Done", register: true);

		public Phase(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public FSprite blackSprite;

	public float blackFade;

	public float lastBlackFade;

	private bool wantToContinue;

	private bool lastPauseButton;

	public MenuLabel headingLabel;

	public FSprite[] fadeSprites;

	public bool[] playersContinueButtons;

	private bool nextLevelCall;

	public int countdownToNextRound = -1;

	public int soundCounter;

	public Phase phase = Phase.Init;

	public FSprite darkFade => fadeSprites[0];

	public FSprite headingFade => fadeSprites[1];

	public void ResultBoxInPlace()
	{
		for (int i = 0; i < resultBoxes.Count; i++)
		{
			if (!(resultBoxes[i] as ArenaOverlayResultBox).inPlace)
			{
				return;
			}
		}
		allResultBoxesInPlaceCounter = 0;
	}

	public ArenaOverlay(ProcessManager manager, ArenaSitting ArenaSitting, List<ArenaSitting.ArenaPlayer> result)
		: base(manager, ArenaSitting, result, ProcessManager.ProcessID.PauseMenu)
	{
		pages.Add(new Page(this, null, "main", 0));
		blackSprite = new FSprite("pixel");
		blackSprite.color = Menu.MenuRGB(MenuColors.Black);
		blackSprite.scaleX = 1400f;
		blackSprite.scaleY = 800f;
		blackSprite.x = manager.rainWorld.options.ScreenSize.x / 2f;
		blackSprite.y = manager.rainWorld.options.ScreenSize.y / 2f;
		blackSprite.alpha = 0.5f;
		pages[0].Container.AddChild(blackSprite);
		fadeSprites = new FSprite[2];
		for (int i = 0; i < fadeSprites.Length; i++)
		{
			fadeSprites[i] = new FSprite("Futile_White");
			fadeSprites[i].color = new Color(0f, 0f, 0f);
			fadeSprites[i].shader = manager.rainWorld.Shaders["FlatLight"];
			fadeSprites[i].alpha = 0f;
			pages[0].Container.AddChild(fadeSprites[i]);
		}
		blackFade = 0f;
		lastBlackFade = 0f;
		topMiddle = new Vector2(683f, 550.01f);
		topMiddle.y += Custom.LerpMap(result.Count, 1f, 4f, -100f, 0f);
		headingLabel = new MenuLabel(this, pages[0], Translate("ROUND") + " " + (ArenaSitting.currentLevel + 1) + " - " + ((result.Count < 2 || result[0].winner) ? Translate("GAME OVER") : Translate("IT'S A DRAW!")), topMiddle + new Vector2(-150f, 1000f), new Vector2(300f, 40f), bigText: true);
		pages[0].subObjects.Add(headingLabel);
		playersContinueButtons = new bool[result.Count];
		for (int num = result.Count - 1; num >= 0; num--)
		{
			resultBoxes.Add(new ArenaOverlayResultBox(this, pages[0], result[num], num, result[num].winner && result.Count > 1));
			pages[0].subObjects.Add(resultBoxes[resultBoxes.Count - 1]);
		}
		soundCounter = 10;
	}

	public override void Update()
	{
		base.Update();
		headingLabel.pos.y = Mathf.Lerp(900f, topMiddle.y + 120f, Custom.SCurve(Mathf.InverseLerp(0f, 40f, counter), 0.7f));
		lastBlackFade = blackFade;
		blackFade = Mathf.Min(1f, blackFade + 0.0625f);
		if (soundCounter > 0)
		{
			soundCounter--;
			if (soundCounter == 0)
			{
				PlaySound(SoundID.UI_Multiplayer_Game_Over, 0f, 1f, 1f);
			}
		}
		for (int i = 0; i < result.Count; i++)
		{
			if (allResultBoxesInPlaceCounter > 10 && !result[i].readyForNextRound)
			{
				Player.InputPackage inputPackage = RWInput.PlayerInput(result[i].playerNumber);
				if ((inputPackage.jmp || inputPackage.thrw || inputPackage.pckp) && !playersContinueButtons[i])
				{
					result[i].readyForNextRound = true;
					PlayerPressedContinue();
				}
			}
		}
		if (countdownToNextRound > 0)
		{
			countdownToNextRound--;
		}
		else if (countdownToNextRound == 0 && !nextLevelCall)
		{
			ArenaSitting.NextLevel(manager);
			nextLevelCall = true;
		}
		if (resultBoxes.Count == 0 && countdownToNextRound == -1)
		{
			countdownToNextRound = 80;
		}
		bool flag = false;
		if (phase == Phase.Init)
		{
			return;
		}
		if (phase == Phase.BumpAndCountScore)
		{
			for (int j = 0; j < resultBoxes.Count; j++)
			{
				if (!(resultBoxes[j] as ArenaOverlayResultBox).scoreSymbol.countedAndDone)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				phase = Phase.CountTime;
				for (int k = 0; k < resultBoxes.Count; k++)
				{
					(resultBoxes[k] as ArenaOverlayResultBox).timeSymbol.Start();
				}
			}
		}
		else if (phase == Phase.CountTime)
		{
			for (int l = 0; l < resultBoxes.Count; l++)
			{
				if (!(resultBoxes[l] as ArenaOverlayResultBox).timeSymbol.countedAndDone)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				phase = Phase.CountKills;
				for (int m = 0; m < resultBoxes.Count; m++)
				{
					resultBoxes[m].killsSymbol.Start();
				}
			}
		}
		else
		{
			if (!(phase == Phase.CountKills))
			{
				return;
			}
			for (int n = 0; n < resultBoxes.Count; n++)
			{
				if (!resultBoxes[n].killsSymbol.countedAndDone)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				phase = Phase.Done;
			}
		}
	}

	public void PlayerPressedContinue()
	{
		if (countdownToNextRound == -1)
		{
			countdownToNextRound = 240;
		}
		bool flag = true;
		for (int i = 0; i < result.Count; i++)
		{
			if (!result[i].readyForNextRound)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			PlaySound(SoundID.UI_Multiplayer_All_Players_Ready);
			countdownToNextRound = Math.Min(countdownToNextRound, 10);
		}
		else
		{
			PlaySound(SoundID.UI_Multiplayer_Player_Result_Box_Player_Ready);
		}
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		float num = Custom.SCurve(Mathf.Lerp(lastBlackFade, blackFade, timeStacker), 0.6f);
		blackSprite.alpha = num * 0.5f;
		float num2 = 0.5f * Custom.SCurve(Mathf.InverseLerp(20f, 80f, (float)counter + timeStacker), 0.4f) + 0.5f * Custom.SCurve(Mathf.InverseLerp(10f, 40f, (float)allResultBoxesInPlaceCounter + timeStacker), 0.4f);
		darkFade.alpha = 0.5f * num2;
		if (resultBoxes.Count == 0)
		{
			darkFade.isVisible = false;
			return;
		}
		float num3 = resultBoxes[resultBoxes.Count - 1].DrawX(timeStacker);
		float num4 = resultBoxes[resultBoxes.Count - 1].DrawX(timeStacker) + resultBoxes[resultBoxes.Count - 1].DrawSize(timeStacker).x;
		float num5 = resultBoxes[resultBoxes.Count - 1].DrawY(timeStacker) + resultBoxes[resultBoxes.Count - 1].DrawSize(timeStacker).y;
		float num6 = resultBoxes[resultBoxes.Count - 1].DrawY(timeStacker);
		for (int i = 0; i < resultBoxes.Count; i++)
		{
			if (resultBoxes[i].DrawX(timeStacker) < num3)
			{
				num3 = Mathf.Lerp(num3, resultBoxes[i].DrawX(timeStacker), Mathf.Pow((resultBoxes[i] as ArenaOverlayResultBox).InPlace(timeStacker), 1f));
			}
			if (resultBoxes[i].DrawX(timeStacker) + resultBoxes[i].DrawSize(timeStacker).x > num4)
			{
				num4 = Mathf.Lerp(num4, resultBoxes[i].DrawX(timeStacker) + resultBoxes[i].DrawSize(timeStacker).x, Mathf.Pow((resultBoxes[i] as ArenaOverlayResultBox).InPlace(timeStacker), 1f));
			}
			if (resultBoxes[i].DrawY(timeStacker) < num6)
			{
				num6 = Mathf.Lerp(num6, resultBoxes[i].pos.y, Mathf.Pow((resultBoxes[i] as ArenaOverlayResultBox).InPlace(timeStacker), 1f));
			}
			if (resultBoxes[i].DrawY(timeStacker) + resultBoxes[i].DrawSize(timeStacker).y > num5)
			{
				num5 = Mathf.Lerp(num5, resultBoxes[i].DrawY(timeStacker) + resultBoxes[i].DrawSize(timeStacker).y, Mathf.Pow((resultBoxes[i] as ArenaOverlayResultBox).InPlace(timeStacker), 1f));
			}
		}
		num3 -= 300f * num2;
		num4 += 300f * num2;
		num6 -= 300f * num2;
		num5 += 300f * num2;
		darkFade.x = (num3 + num4) / 2f;
		darkFade.y = (num6 + num5) / 2f;
		darkFade.scaleX = (num4 - num3) / 8f;
		darkFade.scaleY = (num5 - num6) / 8f;
		headingFade.x = headingLabel.DrawX(timeStacker) + headingLabel.size.x / 2f;
		headingFade.y = headingLabel.DrawY(timeStacker) + headingLabel.size.y / 2f;
		headingFade.scaleX = 50f;
		headingFade.scaleY = 12.5f;
		headingFade.alpha = 0.15f * Custom.SCurve(Mathf.InverseLerp(20f, 90f, (float)counter + timeStacker), 0.6f) + 0.15f * num2;
		headingLabel.label.color = Color.Lerp(Menu.MenuRGB(MenuColors.DarkGrey), Menu.MenuRGB(MenuColors.MediumGrey), Mathf.InverseLerp(20f, 70f, (float)counter + timeStacker));
	}

	public override void ShutDownProcess()
	{
		blackSprite.RemoveFromContainer();
		for (int i = 0; i < fadeSprites.Length; i++)
		{
			fadeSprites[i].RemoveFromContainer();
		}
		base.ShutDownProcess();
	}
}
