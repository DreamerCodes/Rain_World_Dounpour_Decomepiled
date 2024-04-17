using System;
using System.Collections.Generic;
using Expedition;
using Menu.Remix;
using Menu.Remix.MixedUI;
using RWCustom;
using UnityEngine;

namespace Menu;

public class ExpeditionGameOver : Menu
{
	public float leftAnchor;

	public float rightAnchor;

	public FSprite shadow;

	public FSprite title;

	public SimpleButton continueButton;

	public List<FSprite> strikethroughs;

	public List<MenuLabel> challengePreviews;

	public List<MenuLabel> challengeLabels;

	public bool randomStartingLocation;

	private MenuTabWrapper menuTabWrapper;

	private OpHoldButton abandonButton;

	public ExpeditionGameOver(ProcessManager manager)
		: base(manager, ExpeditionEnums.ProcessID.ExpeditionGameOver)
	{
		float[] screenOffsets = Custom.GetScreenOffsets();
		leftAnchor = screenOffsets[0];
		rightAnchor = screenOffsets[1];
		bool flag = false;
		pages = new List<Page>();
		pages.Add(new Page(this, null, "Main", 0));
		scene = new InteractiveMenuScene(this, pages[0], MenuScene.SceneID.StarveScreen);
		pages[0].subObjects.Add(scene);
		manager.musicPlayer?.MenuRequestsSong("RW_65 - Garden", 100f, 50f);
		MenuIllustration item = new MenuIllustration(this, pages[0], "illustrations", "gameover", new Vector2(leftAnchor + 40f, 638f), crispPixels: false, anchorCenter: false)
		{
			sprite = 
			{
				shader = manager.rainWorld.Shaders["MenuTextCustom"]
			}
		};
		pages[0].subObjects.Add(item);
		MenuLabel menuLabel = new MenuLabel(this, pages[0], Translate("CHALLENGES"), new Vector2(leftAnchor + 50f, 600f), default(Vector2), bigText: true)
		{
			label = 
			{
				alignment = FLabelAlignment.Left
			}
		};
		pages[0].subObjects.Add(menuLabel);
		challengeLabels = new List<MenuLabel>();
		Vector2 vector = default(Vector2);
		for (int i = 0; i < ExpeditionData.challengeList.Count; i++)
		{
			Challenge challenge = ExpeditionData.challengeList[i];
			if (challenge.hidden)
			{
				challenge.revealed = true;
				challenge.UpdateDescription();
				flag = true;
			}
			MenuLabel menuLabel2 = new MenuLabel(this, pages[0], ExpeditionData.challengeList[i].description, menuLabel.pos + new Vector2(0f, -30f - 30f * (float)i), default(Vector2), bigText: true)
			{
				label = 
				{
					alignment = FLabelAlignment.Left
				}
			};
			if (challenge.completed)
			{
				menuLabel2.label.color = (challenge.hidden ? new Color(1f, 0.75f, 0f) : new Color(1f, 1f, 1f));
			}
			else
			{
				menuLabel2.label.color = Color.Lerp(challenge.hidden ? new Color(1f, 0.75f, 0f) : new Color(1f, 1f, 1f), new Color(0f, 0f, 0f), 0.6f);
			}
			vector = menuLabel2.pos;
			pages[0].subObjects.Add(menuLabel2);
		}
		pages[0].subObjects.Add(new UnlocksIndicator(this, pages[0], new Vector2(leftAnchor + 50f, vector.y - 38f), centered: false));
		SlugcatSelectMenu.SaveGameData runData = ExpeditionGame.runData;
		if (runData != null && runData.cycle > 0)
		{
			string text = ValueConverter.ConvertToString(runData.cycle);
			string text2 = ValueConverter.ConvertToString(Custom.IntClamp(runData.karma, 0, runData.karmaCap));
			string text3 = Custom.SecondsToMinutesAndSecondsString((int)TimeSpan.FromSeconds((double)runData.gameTimeAlive + (double)runData.gameTimeDead).TotalSeconds);
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
			array[1] = new MenuLabel(this, pages[0], text, new Vector2(rightAnchor - 50f, 715f), default(Vector2), bigText: true);
			array[1].label.alignment = FLabelAlignment.Right;
			array[2] = new MenuLabel(this, pages[0], Translate("KARMA :"), new Vector2(rightAnchor - 200f, 690f), default(Vector2), bigText: true);
			array[2].label.alignment = FLabelAlignment.Left;
			array[3] = new MenuLabel(this, pages[0], text2, new Vector2(rightAnchor - 50f, 690f), default(Vector2), bigText: true);
			array[3].label.alignment = FLabelAlignment.Right;
			array[4] = new MenuLabel(this, pages[0], Translate("TIME :"), new Vector2(rightAnchor - 200f, 665f), default(Vector2), bigText: true);
			array[4].label.alignment = FLabelAlignment.Left;
			array[5] = new MenuLabel(this, pages[0], text3, new Vector2(rightAnchor - 50f, 665f), default(Vector2), bigText: true);
			array[5].label.alignment = FLabelAlignment.Right;
			for (int j = 0; j < array.Length; j++)
			{
				array[j].label.color = new Color(0.5f, 0.5f, 0.5f);
				pages[0].subObjects.Add(array[j]);
			}
		}
		FSprite fSprite = new FSprite("LinearGradient200");
		fSprite.SetAnchor(0.5f, 0f);
		fSprite.x = 280f;
		fSprite.y = 280f;
		fSprite.rotation = 270f;
		fSprite.scaleY = 1.3f;
		fSprite.color = new Color(0.8f, 0.8f, 0.8f);
		fSprite.shader = manager.rainWorld.Shaders["MenuTextCustom"];
		pages[0].Container.AddChild(fSprite);
		FSprite fSprite2 = new FSprite("LinearGradient200");
		fSprite2.SetAnchor(0.5f, 0f);
		fSprite2.x = 279f;
		fSprite2.y = 280f;
		fSprite2.rotation = 90f;
		fSprite2.scaleY = 1.3f;
		fSprite2.color = new Color(0.8f, 0.8f, 0.8f);
		fSprite2.shader = manager.rainWorld.Shaders["MenuTextCustom"];
		pages[0].Container.AddChild(fSprite2);
		HoldButton holdButton = new HoldButton(this, pages[0], Translate("RETRY<LINE>EXPEDITION").Replace("<LINE>", "\n"), "RETRY", new Vector2(leftAnchor + 270f, 170f), 50f);
		pages[0].subObjects.Add(holdButton);
		menuTabWrapper = new MenuTabWrapper(this, pages[0]);
		pages[0].subObjects.Add(menuTabWrapper);
		abandonButton = new OpHoldButton(holdButton.pos + new Vector2(-55f, -120f), new Vector2(110f, 30f), Translate("ABANDON"), 90f);
		abandonButton.colorEdge = new Color(0.6f, 0f, 0f);
		abandonButton.OnPressDone += AbandonButton_OnPressDone;
		abandonButton.description = " ";
		new UIelementWrapper(menuTabWrapper, abandonButton);
		if (flag)
		{
			MenuLabel item2 = new MenuLabel(this, pages[0], Translate("Warning: Hidden challenges will become normal challenges when retrying"), holdButton.pos + new Vector2(0f, 98f), default(Vector2), bigText: false)
			{
				label = 
				{
					color = new Color(0.6f, 0.6f, 0.6f)
				}
			};
			pages[0].subObjects.Add(item2);
		}
	}

	private void AbandonButton_OnPressDone(UIfocusable trigger)
	{
		manager.RequestMainProcessSwitch(ExpeditionEnums.ProcessID.ExpeditionMenu);
	}

	public override void Singal(MenuObject sender, string message)
	{
		if (!(message == "RETRY"))
		{
			return;
		}
		if (ModManager.CoopAvailable)
		{
			for (int i = 1; i < manager.rainWorld.options.JollyPlayerCount; i++)
			{
				manager.rainWorld.ActivatePlayer(i);
			}
			for (int j = manager.rainWorld.options.JollyPlayerCount; j < 4; j++)
			{
				manager.rainWorld.DeactivatePlayer(j);
			}
		}
		manager.arenaSitting = null;
		manager.rainWorld.progression.currentSaveState = null;
		manager.rainWorld.progression.miscProgressionData.currentlySelectedSinglePlayerSlugcat = ExpeditionData.slugcatPlayer;
		manager.rainWorld.progression.WipeSaveState(ExpeditionData.slugcatPlayer);
		ExpeditionGame.PrepareExpedition();
		if (ExpeditionData.activeMission != "" && ExpeditionProgression.missionList.Find((ExpeditionProgression.Mission x) => x.key == ExpeditionData.activeMission).den != "")
		{
			ExpeditionData.startingDen = ExpeditionProgression.missionList.Find((ExpeditionProgression.Mission x) => x.key == ExpeditionData.activeMission).den;
		}
		else if (ExpeditionData.startingDen == null)
		{
			ExpeditionData.startingDen = ExpeditionGame.ExpeditionRandomStarts(manager.rainWorld, ExpeditionData.slugcatPlayer);
		}
		for (int k = 0; k < ExpeditionData.challengeList.Count; k++)
		{
			ExpeditionData.challengeList[k].Reset();
		}
		global::Expedition.Expedition.coreFile.Save(runEnded: false);
		manager.menuSetup.startGameCondition = ProcessManager.MenuSetup.StoryGameInitCondition.New;
		manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Game);
		PlaySound(SoundID.MENU_Start_New_Game);
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		if (strikethroughs != null && strikethroughs.Count > 0 && challengePreviews != null && challengePreviews.Count > 0)
		{
			for (int i = 0; i < strikethroughs.Count; i++)
			{
				strikethroughs[i].x = pages[0].pos.x + challengePreviews[i].pos.x - 5f;
				strikethroughs[i].y = pages[0].pos.y + challengePreviews[i].pos.y - 2f;
			}
		}
	}
}
