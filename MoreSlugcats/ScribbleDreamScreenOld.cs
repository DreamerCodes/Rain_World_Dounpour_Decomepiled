using System;
using System.IO;
using Menu;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class ScribbleDreamScreenOld : global::Menu.Menu
{
	public new MenuIllustration scene;

	public float frame;

	public SimpleButton exitButton;

	public int counter;

	private KarmaLadderScreen.SleepDeathScreenDataPackage fromGameDataPackage;

	public DreamsState.DreamID dreamID;

	private bool initSound;

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

	public ScribbleDreamScreenOld(ProcessManager manager)
		: base(manager, ProcessManager.ProcessID.Dream)
	{
		pages.Add(new Page(this, null, "main", 0));
		mySoundLoopID = SoundID.None;
		frame = 1f;
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
		if (exitButton != null)
		{
			exitButton.buttonBehav.greyedOut = FreezeMenuFunctions;
			exitButton.black = Math.Max(0f, exitButton.black - 0.0125f);
		}
		else if (counter > ((!manager.rainWorld.setup.devToolsActive) ? 340 : 100))
		{
			exitButton = new SimpleButton(this, pages[0], Translate("CONTINUE"), "CONTINUE", new Vector2(manager.rainWorld.options.ScreenSize.x - 320f + (1366f - manager.rainWorld.options.ScreenSize.x), 15f), new Vector2(110f, 30f));
			pages[0].subObjects.Add(exitButton);
			pages[0].lastSelectedObject = exitButton;
			exitButton.black = 1f;
		}
		if (scene != null)
		{
			frame += 0.075f;
			if (frame >= 3f)
			{
				frame = 1f;
			}
			string text = SceneFromDream(dreamID);
			string text2 = ((int)frame).ToString("0");
			if (!File.Exists(AssetManager.ResolveFilePath("Illustrations" + Path.DirectorySeparatorChar + text + "_" + text2 + ".png")))
			{
				frame = 1f;
				text2 = ((int)frame).ToString("0");
			}
			scene.fileName = text + "_" + text2;
			scene.LoadFile();
			scene.sprite.SetElementByName(scene.fileName);
		}
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
	}

	public void StartGame()
	{
		manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Game);
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
		string text = SceneFromDream(this.dreamID);
		scene = new MenuIllustration(this, scene, "Illustrations", text + "_1", new Vector2((0f - (1366f - manager.rainWorld.options.ScreenSize.x)) / 2f, 0f), crispPixels: false, anchorCenter: false);
		pages[0].subObjects.Add(scene);
	}

	public override void CommunicateWithUpcomingProcess(MainLoopProcess nextProcess)
	{
		base.CommunicateWithUpcomingProcess(nextProcess);
		if (nextProcess is SleepAndDeathScreen)
		{
			(nextProcess as SleepAndDeathScreen).GetDataFromGame(fromGameDataPackage);
		}
	}

	public string SceneFromDream(DreamsState.DreamID dreamID)
	{
		if (dreamID == MoreSlugcatsEnums.DreamID.Gourmand1)
		{
			return "gourmanddream_1";
		}
		if (dreamID == MoreSlugcatsEnums.DreamID.Gourmand2)
		{
			return "gourmanddream_2";
		}
		if (dreamID == MoreSlugcatsEnums.DreamID.Gourmand3)
		{
			return "gourmanddream_3";
		}
		if (dreamID == MoreSlugcatsEnums.DreamID.Gourmand4)
		{
			return "gourmanddream_4";
		}
		if (dreamID == MoreSlugcatsEnums.DreamID.Gourmand5)
		{
			return "gourmanddream_5";
		}
		return "";
	}
}
