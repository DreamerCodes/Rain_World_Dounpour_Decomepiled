using System;
using System.Collections.Generic;
using Expedition;
using Menu.Remix;
using RWCustom;
using UnityEngine;

namespace Menu;

public class ExpeditionWinScreen : Menu
{
	public SimpleButton continueButton;

	public ScoreCalculator scoreCalculator;

	public FSprite shadow;

	public FSprite title;

	public FSprite gradient;

	public SleepScreenKills kills;

	public ExpeditionProgression.WinPackage package;

	public MenuLabel missionTime;

	public MenuLabel bestMissionTime;

	public bool isShowingDialog;

	public bool evaluateExpedition;

	public int questsDisplayed;

	public int startingLevel;

	public bool showLevelUp;

	public float leftAnchor;

	public float rightAnchor;

	public ExpeditionWinScreen(ProcessManager manager)
		: base(manager, ExpeditionEnums.ProcessID.ExpeditionWinScreen)
	{
		startingLevel = ExpeditionData.level;
		showLevelUp = ExpeditionData.level > startingLevel;
		pages = new List<Page>();
		pages.Add(new Page(this, null, "Main", 0));
		scene = (ExpeditionGame.voidSeaFinish ? new InteractiveMenuScene(this, pages[0], MenuScene.SceneID.Void_Slugcat_Down) : new InteractiveMenuScene(this, pages[0], MenuScene.SceneID.SleepScreen));
		scene.camPos.x -= 400f;
		pages[0].subObjects.Add(scene);
		leftAnchor = Custom.GetScreenOffsets()[0];
		rightAnchor = Custom.GetScreenOffsets()[1];
		base.manager.musicPlayer?.MenuRequestsSong("RW_65 - Garden", 100f, 50f);
		gradient = new FSprite("LinearGradient200");
		gradient.x = 0f;
		gradient.y = 0f;
		gradient.rotation = 90f;
		gradient.SetAnchor(1f, 0f);
		gradient.scaleY = 3f;
		gradient.scaleX = 1500f;
		gradient.color = new Color(0f, 0f, 0f);
		pages[0].Container.AddChild(gradient);
		shadow = new FSprite("expeditionshadow");
		shadow.x = 10f;
		shadow.y = 638f;
		shadow.SetAnchor(0f, 0f);
		shadow.shader = manager.rainWorld.Shaders["MenuText"];
		pages[0].Container.AddChild(shadow);
		title = new FSprite("expeditiontitle");
		title.x = 10f;
		title.y = 638f;
		title.SetAnchor(0f, 0f);
		pages[0].Container.AddChild(title);
		FSprite fSprite = new FSprite("LinearGradient200");
		fSprite.rotation = 90f;
		fSprite.scaleY = 2.5f;
		fSprite.scaleX = 2.5f;
		fSprite.SetAnchor(new Vector2(0.5f, 0f));
		fSprite.x = 40f;
		fSprite.y = 646f;
		fSprite.shader = manager.rainWorld.Shaders["MenuText"];
		pages[0].Container.AddChild(fSprite);
		if (ExpeditionGame.runKills != null && ExpeditionGame.runKills.Count > 0)
		{
			MenuLabel menuLabel = new MenuLabel(this, pages[0], Translate("KILLS"), new Vector2(50f, 290f), default(Vector2), bigText: true)
			{
				label = 
				{
					alignment = FLabelAlignment.Left,
					shader = manager.rainWorld.Shaders["MenuText"]
				}
			};
			pages[0].subObjects.Add(menuLabel);
			FSprite fSprite2 = new FSprite("LinearGradient200");
			fSprite2.rotation = 90f;
			fSprite2.scaleY = 0.8f;
			fSprite2.scaleX = 2f;
			fSprite2.SetAnchor(new Vector2(0.5f, 0f));
			fSprite2.x = menuLabel.pos.x;
			fSprite2.y = menuLabel.pos.y - 15f;
			fSprite2.shader = manager.rainWorld.Shaders["MenuText"];
			pages[0].Container.AddChild(fSprite2);
			pages[0].subObjects.Add(new KillsDisplayer(this, pages[0], new Vector2(50f, 50f), ExpeditionGame.runKills));
		}
		SlugcatSelectMenu.SaveGameData runData = ExpeditionGame.runData;
		if (runData.gameTimeAlive > 0 || runData.gameTimeDead > 0)
		{
			int num = (int)TimeSpan.FromSeconds((double)runData.gameTimeAlive + (double)runData.gameTimeDead).TotalSeconds;
			if (ExpeditionData.activeMission != "")
			{
				bool flag = false;
				ExpLog.Log("Mission time recorded:");
				if (!ExpeditionData.missionBestTimes.ContainsKey(ExpeditionData.activeMission))
				{
					ExpLog.Log("First mission completion, time: " + num);
					ExpeditionData.missionBestTimes.Add(ExpeditionData.activeMission, num);
					flag = true;
				}
				else
				{
					ExpLog.Log("Check mission time against existing record");
					if (num < ExpeditionData.missionBestTimes[ExpeditionData.activeMission])
					{
						ExpLog.Log("Mission record beaten, logging new time: " + num);
						ExpeditionData.missionBestTimes[ExpeditionData.activeMission] = num;
						flag = true;
					}
				}
				string text = TimeSpan.FromSeconds(ExpeditionData.missionBestTimes[ExpeditionData.activeMission]).ToString("hh\\:mm\\:ss");
				missionTime = new MenuLabel(this, pages[0], (flag ? Translate("NEW BEST TIME:") : Translate("BEST TIME:")) + "  " + text, new Vector2(683f, 50f), default(Vector2), bigText: true);
				missionTime.label.shader = (flag ? manager.rainWorld.Shaders["MenuTextCustom"] : manager.rainWorld.Shaders["Basic"]);
				missionTime.label.color = (flag ? new Color(1f, 0.7f, 0f) : new Color(0.6f, 0.6f, 0.6f));
				pages[0].subObjects.Add(missionTime);
			}
		}
		if (runData != null && runData.cycle > 0)
		{
			string text2 = ValueConverter.ConvertToString(runData.cycle);
			string text3 = ValueConverter.ConvertToString(Custom.IntClamp(runData.karma, 0, runData.karmaCap));
			string text4 = Custom.SecondsToMinutesAndSecondsString((int)TimeSpan.FromSeconds((double)runData.gameTimeAlive + (double)runData.gameTimeDead).TotalSeconds);
			MenuLabel[] array = new MenuLabel[6]
			{
				new MenuLabel(this, pages[0], Translate("CYCLE :"), new Vector2(rightAnchor - 200f, 715f), default(Vector2), bigText: true),
				null,
				null,
				null,
				null,
				null
			};
			array[0].label.alignment = FLabelAlignment.Left;
			array[1] = new MenuLabel(this, pages[0], text2, new Vector2(rightAnchor - 50f, 715f), default(Vector2), bigText: true);
			array[1].label.alignment = FLabelAlignment.Right;
			array[2] = new MenuLabel(this, pages[0], Translate("KARMA :"), new Vector2(rightAnchor - 200f, 690f), default(Vector2), bigText: true);
			array[2].label.alignment = FLabelAlignment.Left;
			array[3] = new MenuLabel(this, pages[0], text3, new Vector2(rightAnchor - 50f, 690f), default(Vector2), bigText: true);
			array[3].label.alignment = FLabelAlignment.Right;
			array[4] = new MenuLabel(this, pages[0], Translate("TIME :"), new Vector2(rightAnchor - 200f, 665f), default(Vector2), bigText: true);
			array[4].label.alignment = FLabelAlignment.Left;
			array[5] = new MenuLabel(this, pages[0], text4, new Vector2(rightAnchor - 50f, 665f), default(Vector2), bigText: true);
			array[5].label.alignment = FLabelAlignment.Right;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].label.color = new Color(0.5f, 0.5f, 0.5f);
				pages[0].subObjects.Add(array[i]);
			}
		}
		continueButton = new SimpleButton(this, pages[0], Translate("CONTINUE"), "CONTINUE", new Vector2(rightAnchor - 150f, 40f), new Vector2(100f, 30f));
		pages[0].subObjects.Add(continueButton);
		ExpeditionGame.FinishExpedition();
		scoreCalculator = new ScoreCalculator(this, pages[0], new Vector2(leftAnchor + 250f, 50f));
		pages[0].subObjects.Add(scoreCalculator);
		ExpeditionData.ClearActiveChallengeList();
	}

	public override void Update()
	{
		base.Update();
		if (scoreCalculator.phase == ScoreCalculatorPhase.Done && showLevelUp && !isShowingDialog)
		{
			MenuLabel menuLabel = new MenuLabel(this, pages[0], Translate("LEVEL UP"), new Vector2(leftAnchor + 50f, 80f), default(Vector2), bigText: true);
			menuLabel.label.alignment = FLabelAlignment.Left;
			menuLabel.label.color = new Color(1f, 0.7f, 0f);
			menuLabel.label.shader = manager.rainWorld.Shaders["MenuTextCustom"];
			pages[0].subObjects.Add(menuLabel);
			PlaySound(SoundID.MENU_Player_Join_Game);
			showLevelUp = false;
		}
		else if (scoreCalculator.phase == ScoreCalculatorPhase.Done && questsDisplayed < ExpeditionGame.pendingCompletedQuests.Count && !isShowingDialog)
		{
			ExpLog.Log("Creating QuestCompleteDialog for quest: " + ExpeditionGame.pendingCompletedQuests[questsDisplayed]);
			QuestCompleteDialog dialog = new QuestCompleteDialog(manager, this, ExpeditionGame.pendingCompletedQuests[questsDisplayed]);
			manager.ShowDialog(dialog);
			isShowingDialog = true;
			questsDisplayed++;
		}
		else if (questsDisplayed >= ExpeditionGame.pendingCompletedQuests.Count)
		{
			continueButton.buttonBehav.greyedOut = false;
		}
		else
		{
			continueButton.buttonBehav.greyedOut = true;
		}
	}

	public override void Singal(MenuObject sender, string message)
	{
		if (message == "CONTINUE")
		{
			manager.RequestMainProcessSwitch(ExpeditionEnums.ProcessID.ExpeditionMenu);
		}
	}
}
