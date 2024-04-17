using System;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace Menu;

public class DreamScreen : Menu
{
	public SimpleButton exitButton;

	public int counter;

	private KarmaLadderScreen.SleepDeathScreenDataPackage fromGameDataPackage;

	public DreamsState.DreamID dreamID;

	private bool initSound;

	public int MoonBetrayalNeurons
	{
		get
		{
			if (fromGameDataPackage == null || fromGameDataPackage.saveState == null)
			{
				return 4;
			}
			return fromGameDataPackage.saveState.miscWorldSaveData.SLOracleState.neuronsLeft;
		}
	}

	public override bool ForceNoMouseMode
	{
		get
		{
			if (exitButton == null || exitButton.black > 0.5f)
			{
				return true;
			}
			return base.ForceNoMouseMode;
		}
	}

	protected override bool FreezeMenuFunctions
	{
		get
		{
			if (!base.FreezeMenuFunctions)
			{
				return counter < 20;
			}
			return true;
		}
	}

	public DreamScreen(ProcessManager manager)
		: base(manager, ProcessManager.ProcessID.Dream)
	{
		pages.Add(new Page(this, null, "main", 0));
		scene = new InteractiveMenuScene(this, pages[0], MenuScene.SceneID.SleepScreen);
		pages[0].subObjects.Add(scene);
		mySoundLoopID = SoundID.MENU_Dream_LOOP;
		if (manager.musicPlayer != null)
		{
			manager.musicPlayer.FadeOutAllSongs(30f);
		}
	}

	public override void Update()
	{
		base.Update();
		counter++;
		if (ModManager.MSC && scene.sceneID == MenuScene.SceneID.SleepScreen && dreamID != null && dreamID == MoreSlugcatsEnums.DreamID.SaintKarma)
		{
			manager.fadeToBlack = 1f;
			scene.RemoveSprites();
			pages[0].subObjects.Remove(scene);
			scene = new InteractiveMenuScene(this, pages[0], SceneFromDream(dreamID));
			pages[0].subObjects.Add(scene);
			dreamID = null;
		}
		if (!initSound)
		{
			PlaySound(SoundID.MENU_Dream_Init);
			initSound = true;
		}
		if (scene.sceneID == MenuScene.SceneID.SleepScreen && dreamID != null)
		{
			int num = 35;
			bool flag = ModManager.MSC && dreamID.Index >= MoreSlugcatsEnums.DreamID.ArtificerFamilyA.Index && dreamID.Index <= MoreSlugcatsEnums.DreamID.ArtificerNightmare.Index;
			if (flag)
			{
				if (soundLoop != null)
				{
					soundLoop.loopVolume = 0f;
				}
				num = 120;
				if (counter == 20)
				{
					if (dreamID.Index >= MoreSlugcatsEnums.DreamID.ArtificerFamilyE.Index)
					{
						PlaySound(MoreSlugcatsEnums.MSCSoundID.DreamDN, 0f, 1f, 1f);
					}
					else if (dreamID.Index >= MoreSlugcatsEnums.DreamID.ArtificerFamilyC.Index && dreamID.Index <= MoreSlugcatsEnums.DreamID.ArtificerFamilyD.Index)
					{
						PlaySound(MoreSlugcatsEnums.MSCSoundID.DreamN, 0f, 1f, 1f);
					}
					else
					{
						PlaySound(SoundID.MENU_Dream_Switch);
					}
				}
			}
			if (counter <= num)
			{
				manager.fadeToBlack = Custom.LerpAndTick(manager.fadeToBlack, 0f, 0f, 0.0125f);
				return;
			}
			manager.fadeToBlack = Custom.LerpAndTick(manager.fadeToBlack, 1f, 0f, 1f / Mathf.Lerp(450f, 210f, manager.fadeToBlack));
			if (manager.fadeToBlack > 0.9f)
			{
				manager.fadeToBlack = 1f;
				scene.RemoveSprites();
				if (flag)
				{
					manager.artificerDreamNumber = dreamID.Index - MoreSlugcatsEnums.DreamID.ArtificerFamilyA.Index;
					manager.menuSetup.startGameCondition = ProcessManager.MenuSetup.StoryGameInitCondition.New;
					manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Game);
				}
				else
				{
					PlaySound(SoundID.MENU_Dream_Switch);
					pages[0].subObjects.Remove(scene);
					scene = new InteractiveMenuScene(this, pages[0], SceneFromDream(dreamID));
					pages[0].subObjects.Add(scene);
				}
				dreamID = null;
			}
		}
		else
		{
			manager.fadeToBlack = Custom.LerpAndTick(manager.fadeToBlack, 0f, 0f, 0.0125f);
			if (exitButton != null)
			{
				exitButton.buttonBehav.greyedOut = FreezeMenuFunctions;
				exitButton.black = Math.Max(0f, exitButton.black - 0.0125f);
			}
			else if (counter > (manager.rainWorld.setup.devToolsActive ? 100 : 340))
			{
				exitButton = new SimpleButton(this, pages[0], Translate("CONTINUE"), "CONTINUE", new Vector2(manager.rainWorld.options.ScreenSize.x - 320f + (1366f - manager.rainWorld.options.ScreenSize.x) - manager.rainWorld.options.SafeScreenOffset.x, Mathf.Max(manager.rainWorld.options.SafeScreenOffset.y, 15f)), new Vector2(110f, 30f));
				pages[0].subObjects.Add(exitButton);
				pages[0].lastSelectedObject = exitButton;
				exitButton.black = 1f;
			}
		}
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		for (int i = 0; i < scene.depthIllustrations.Count; i++)
		{
			Vector2 pos = scene.depthIllustrations[i].pos;
			pos.x += (1366f - manager.rainWorld.options.ScreenSize.x) / 2f;
		}
	}

	public void StartGame()
	{
		manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Game);
	}

	public override void Singal(MenuObject sender, string message)
	{
		if (message == "CONTINUE")
		{
			if (ModManager.MSC && scene.sceneID == MoreSlugcatsEnums.MenuSceneID.SaintMaxKarma)
			{
				manager.menuSetup.startGameCondition = ProcessManager.MenuSetup.StoryGameInitCondition.Load;
				StartGame();
			}
			else
			{
				manager.RequestMainProcessSwitch(ProcessManager.ProcessID.SleepScreen);
			}
			PlaySound(SoundID.MENU_Dream_Button);
		}
	}

	public void GetDataFromGame(DreamsState.DreamID dreamID, KarmaLadderScreen.SleepDeathScreenDataPackage package)
	{
		fromGameDataPackage = package;
		this.dreamID = dreamID;
	}

	public override void CommunicateWithUpcomingProcess(MainLoopProcess nextProcess)
	{
		base.CommunicateWithUpcomingProcess(nextProcess);
		if (nextProcess is SleepAndDeathScreen)
		{
			(nextProcess as SleepAndDeathScreen).GetDataFromGame(fromGameDataPackage);
		}
	}

	public MenuScene.SceneID SceneFromDream(DreamsState.DreamID dreamID)
	{
		if (dreamID == DreamsState.DreamID.FamilyA)
		{
			return MenuScene.SceneID.Dream_Sleep;
		}
		if (dreamID == DreamsState.DreamID.FamilyB)
		{
			return MenuScene.SceneID.Dream_Sleep_Fade;
		}
		if (dreamID == DreamsState.DreamID.FamilyC)
		{
			return MenuScene.SceneID.Dream_Acceptance;
		}
		if (dreamID == DreamsState.DreamID.GuideA)
		{
			return MenuScene.SceneID.Dream_Iggy;
		}
		if (dreamID == DreamsState.DreamID.GuideB)
		{
			return MenuScene.SceneID.Dream_Iggy_Image;
		}
		if (dreamID == DreamsState.DreamID.GuideC)
		{
			return MenuScene.SceneID.Dream_Iggy_Doubt;
		}
		if (dreamID == DreamsState.DreamID.Pebbles)
		{
			return MenuScene.SceneID.Dream_Pebbles;
		}
		if (dreamID == DreamsState.DreamID.MoonThief)
		{
			return MenuScene.SceneID.Dream_Moon_Betrayal;
		}
		if (dreamID == DreamsState.DreamID.MoonFriend)
		{
			return MenuScene.SceneID.Dream_Moon_Friend;
		}
		if (dreamID == DreamsState.DreamID.VoidDreamSlugcatUp)
		{
			return MenuScene.SceneID.Void_Slugcat_Upright;
		}
		if (dreamID == DreamsState.DreamID.VoidDreamSlugcatDown)
		{
			return MenuScene.SceneID.Void_Slugcat_Down;
		}
		if (ModManager.MSC && dreamID == MoreSlugcatsEnums.DreamID.SaintKarma)
		{
			return MoreSlugcatsEnums.MenuSceneID.SaintMaxKarma;
		}
		return MenuScene.SceneID.Empty;
	}
}
