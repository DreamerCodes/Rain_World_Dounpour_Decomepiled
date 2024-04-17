using System;
using Menu;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class ScribbleDreamScreen : global::Menu.Menu
{
	public float frame;

	public float lastFrame;

	public SimpleButton exitButton;

	public int counter;

	private KarmaLadderScreen.SleepDeathScreenDataPackage fromGameDataPackage;

	public DreamsState.DreamID dreamID;

	private bool initSound;

	private Room room;

	private int fadeIn;

	private int wait;

	private int fadeOut;

	private int initWait;

	public bool slatedForDeletetion;

	public FSprite blackSprite;

	public override bool ForceNoMouseMode
	{
		get
		{
			if (exitButton != null && !(exitButton.black > 0.5f))
			{
				return base.ForceNoMouseMode;
			}
			return true;
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

	public ScribbleDreamScreen(ProcessManager manager)
		: base(manager, ProcessManager.ProcessID.Dream)
	{
		pages.Add(new Page(this, null, "main", 0));
		mySoundLoopID = SoundID.None;
		frame = 1f;
		lastFrame = frame;
		scene = null;
		if (manager.musicPlayer != null)
		{
			manager.musicPlayer.FadeOutAllSongs(30f);
		}
	}

	public override void Update()
	{
		base.Update();
		counter++;
		if (room == null)
		{
			if (counter > 840)
			{
				mySoundLoopID = SoundID.MENU_Dream_LOOP;
			}
			if (!initSound)
			{
				PlaySound(MoreSlugcatsEnums.MSCSoundID.BM_GOR02, 0f, 1f, 1f);
				initSound = true;
			}
			manager.fadeToBlack = Custom.LerpAndTick(manager.fadeToBlack, 0f, 0f, 0.0125f);
			if (blackSprite != null)
			{
				blackSprite.alpha = 0f;
			}
			if (exitButton != null)
			{
				exitButton.buttonBehav.greyedOut = FreezeMenuFunctions;
				exitButton.black = Math.Max(0f, exitButton.black - 0.0125f);
			}
			else if (counter > ((!manager.rainWorld.setup.devToolsActive) ? 340 : 100))
			{
				if (ContinueButtonLeftSide())
				{
					exitButton = new SimpleButton(this, pages[0], Translate("CONTINUE"), "CONTINUE", new Vector2(270f + (1366f - manager.rainWorld.options.ScreenSize.x), 55f), new Vector2(110f, 30f));
				}
				else
				{
					exitButton = new SimpleButton(this, pages[0], Translate("CONTINUE"), "CONTINUE", new Vector2(manager.rainWorld.options.ScreenSize.x - 60f - 320f + (1366f - manager.rainWorld.options.ScreenSize.x), 55f), new Vector2(110f, 30f));
				}
				exitButton.labelColor = new HSLColor(0f, 0f, 0f);
				exitButton.rectColor = new HSLColor(0f, 0f, 0f);
				pages[0].subObjects.Add(exitButton);
				pages[0].lastSelectedObject = exitButton;
				exitButton.black = 1f;
			}
		}
		else
		{
			if (counter >= initWait + fadeIn + wait + fadeOut)
			{
				slatedForDeletetion = true;
				room.game.cameras[0].virtualMicrophone.globalSoundMuffle = 0f;
			}
			if (counter == initWait)
			{
				PlaySound(MoreSlugcatsEnums.MSCSoundID.BM_GOR01, 0f, 1f, 1f);
			}
			else if (counter < initWait + fadeIn + wait)
			{
				if (counter > 10)
				{
					room.game.paused = true;
				}
				room.game.cameras[0].virtualMicrophone.globalSoundMuffle = 1f;
			}
			else
			{
				room.game.paused = false;
				room.game.cameras[0].virtualMicrophone.globalSoundMuffle = Mathf.Max(0f, 1f - ((float)counter - (float)(initWait + fadeIn + wait)) / (float)fadeOut);
			}
			if (counter < initWait)
			{
				manager.fadeToBlack = 1f;
			}
			else if (counter < initWait + fadeIn)
			{
				manager.fadeToBlack = 1f - (float)(counter - initWait) / (float)fadeIn;
			}
			else
			{
				manager.fadeToBlack = 0f;
			}
			if (blackSprite != null)
			{
				blackSprite.alpha = manager.fadeToBlack;
			}
			if ((float)counter >= (float)(initWait + fadeIn + wait) && (float)counter < (float)(initWait + fadeIn + wait + fadeOut))
			{
				float value = 1f - ((float)counter - (float)(initWait + fadeIn + wait)) / (float)fadeOut;
				for (int i = 0; i < scene.depthIllustrations.Count; i++)
				{
					scene.depthIllustrations[i].setAlpha = value;
				}
				for (int j = 0; j < scene.flatIllustrations.Count; j++)
				{
					scene.flatIllustrations[j].setAlpha = value;
				}
			}
		}
		if (scene != null)
		{
			lastFrame = frame;
			frame += 0.075f;
			if (frame >= 3f)
			{
				frame = 1f;
			}
			if ((int)frame != (int)lastFrame)
			{
				scene.FlipScribble();
			}
		}
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
	}

	public override void Singal(MenuObject sender, string message)
	{
		if (message == "CONTINUE")
		{
			manager.RequestMainProcessSwitch(ProcessManager.ProcessID.SleepScreen);
			PlaySound(SoundID.MENU_Dream_Button);
		}
	}

	public void GetDataFromGame(DreamsState.DreamID dreamID, KarmaLadderScreen.SleepDeathScreenDataPackage package)
	{
		fromGameDataPackage = package;
		this.dreamID = dreamID;
		MenuScene.SceneID sceneID = SceneFromDream(this.dreamID);
		scene = new InteractiveMenuScene(this, pages[0], sceneID);
		pages[0].subObjects.Add(scene);
		blackSprite = new FSprite("pixel");
		blackSprite.color = global::Menu.Menu.MenuRGB(MenuColors.Black);
		blackSprite.scaleX = 1400f;
		blackSprite.scaleY = 800f;
		blackSprite.x = manager.rainWorld.options.ScreenSize.x / 2f;
		blackSprite.y = manager.rainWorld.options.ScreenSize.y / 2f;
		blackSprite.alpha = 1f;
		pages[0].Container.AddChild(blackSprite);
	}

	public override void CommunicateWithUpcomingProcess(MainLoopProcess nextProcess)
	{
		base.CommunicateWithUpcomingProcess(nextProcess);
		if (nextProcess is SleepAndDeathScreen)
		{
			(nextProcess as SleepAndDeathScreen).GetDataFromGame(fromGameDataPackage);
		}
	}

	public bool ContinueButtonLeftSide()
	{
		if (!(dreamID == MoreSlugcatsEnums.DreamID.Gourmand0) && !(dreamID == MoreSlugcatsEnums.DreamID.Gourmand3))
		{
			return dreamID == MoreSlugcatsEnums.DreamID.Gourmand4;
		}
		return true;
	}

	public void SetupFadeOut(Room room, int initWait, int fadeIn, int wait, int fadeOut)
	{
		this.room = room;
		this.initWait = initWait;
		this.fadeIn = fadeIn;
		this.fadeOut = fadeOut;
		this.wait = wait;
	}

	public override void ShutDownProcess()
	{
		if (blackSprite != null)
		{
			blackSprite.RemoveFromContainer();
		}
		base.ShutDownProcess();
	}

	public MenuScene.SceneID SceneFromDream(DreamsState.DreamID dreamID)
	{
		if (dreamID == MoreSlugcatsEnums.DreamID.Gourmand1)
		{
			return MoreSlugcatsEnums.MenuSceneID.Gourmand_Dream1;
		}
		if (dreamID == MoreSlugcatsEnums.DreamID.Gourmand2)
		{
			return MoreSlugcatsEnums.MenuSceneID.Gourmand_Dream2;
		}
		if (dreamID == MoreSlugcatsEnums.DreamID.Gourmand3)
		{
			return MoreSlugcatsEnums.MenuSceneID.Gourmand_Dream3;
		}
		if (dreamID == MoreSlugcatsEnums.DreamID.Gourmand4)
		{
			return MoreSlugcatsEnums.MenuSceneID.Gourmand_Dream4;
		}
		if (dreamID == MoreSlugcatsEnums.DreamID.Gourmand5)
		{
			return MoreSlugcatsEnums.MenuSceneID.Gourmand_Dream5;
		}
		if (dreamID == MoreSlugcatsEnums.DreamID.Gourmand0)
		{
			return MoreSlugcatsEnums.MenuSceneID.Gourmand_Dream_Start;
		}
		return null;
	}
}
