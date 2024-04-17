using Expedition;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace Menu;

public class PauseMenu : Menu
{
	public RainWorldGame game;

	public SimpleButton continueButton;

	public SimpleButton exitButton;

	public SimpleButton confirmYesButton;

	public SimpleButton confirmNoButton;

	public MenuLabel confirmMessage;

	public FSprite blackSprite;

	public float blackFade;

	public float lastBlackFade;

	private bool wantToContinue;

	private bool lastPauseButton;

	private bool pauseWarningActive;

	public float[,] micVolumes;

	public ControlMap controlMap;

	public int counter;

	private float moveLeft;

	public float ContinueAndExitButtonsXPos => manager.rainWorld.options.ScreenSize.x + (1366f - manager.rainWorld.options.ScreenSize.x) / 2f;

	public void WarpPreInit(RainWorldGame game)
	{
	}

	public void WarpInit(RainWorldGame game)
	{
	}

	public void WarpUpdate()
	{
	}

	public void WarpSignal(MenuObject sender, string message)
	{
	}

	public PauseMenu(ProcessManager manager, RainWorldGame game)
		: base(manager, ProcessManager.ProcessID.PauseMenu)
	{
		WarpPreInit(game);
		this.game = game;
		pages.Add(new Page(this, null, "main", 0));
		blackSprite = new FSprite("pixel");
		blackSprite.color = Menu.MenuRGB(MenuColors.Black);
		blackSprite.scaleX = 1400f;
		blackSprite.scaleY = 800f;
		blackSprite.x = manager.rainWorld.options.ScreenSize.x / 2f;
		blackSprite.y = manager.rainWorld.options.ScreenSize.y / 2f;
		blackSprite.alpha = 0.5f;
		pages[0].Container.AddChild(blackSprite);
		Options.ControlSetup.Preset preset = manager.rainWorld.options.controls[0].GetActivePreset();
		if (!manager.rainWorld.options.controls[0].IsDefaultControlMapping(manager.rainWorld.options.controls[0].gameControlMap))
		{
			preset = Options.ControlSetup.Preset.None;
		}
		bool flag = ModManager.MSC && manager.rainWorld.safariMode;
		if (manager.currentMainLoop.ID == ProcessManager.ProcessID.Game && ((manager.currentMainLoop as RainWorldGame).IsStorySession || flag))
		{
			controlMap = new ControlMap(this, pages[0], new Vector2((float)(int)(manager.rainWorld.screenSize.x / 3f) + 170.2f + (float)(int)((1366f - manager.rainWorld.screenSize.x) / 2f), 380f), preset, showPickupInstructions: true);
			pages[0].subObjects.Add(controlMap);
		}
		moveLeft = 0f;
		if (game.IsArenaSession && game.GetArenaGameSession is SandboxGameSession)
		{
			if ((game.GetArenaGameSession as SandboxGameSession).PlayMode)
			{
				if (ModManager.MSC && game.GetArenaGameSession.arenaSitting.gameTypeSetup.gameType == MoreSlugcatsEnums.GameTypeID.Challenge)
				{
					SimpleButton simpleButton = new SimpleButton(this, pages[0], Translate("RESTART"), "RESTART", new Vector2(ContinueAndExitButtonsXPos - 180f - manager.rainWorld.options.SafeScreenOffset.x, Mathf.Max(manager.rainWorld.options.SafeScreenOffset.y, 15f)), new Vector2(110f, 30f));
					pages[0].subObjects.Add(simpleButton);
					simpleButton.nextSelectable[1] = simpleButton;
					simpleButton.nextSelectable[3] = simpleButton;
					MenuLabel menuLabel = new MenuLabel(this, pages[0], Translate("Challenge #<X>").Replace("<X>", game.GetArenaGameSession.chMeta.challengeNumber.ToString()) + ": " + game.GetArenaGameSession.chMeta.name, new Vector2((1366f - manager.rainWorld.screenSize.x) / 2f, game.rainWorld.screenSize.y - 100f), new Vector2(110f, 30f), bigText: true);
					MenuLabel menuLabel2 = new MenuLabel(this, pages[0], game.GetArenaGameSession.chMeta.GetMetaDescription(this), new Vector2((1366f - manager.rainWorld.screenSize.x) / 2f, game.rainWorld.screenSize.y - 140f), new Vector2(110f, 30f), bigText: false);
					menuLabel.label.alignment = FLabelAlignment.Left;
					menuLabel2.label.alignment = FLabelAlignment.Left;
					pages[0].subObjects.Add(menuLabel);
					pages[0].subObjects.Add(menuLabel2);
					moveLeft = 140f;
				}
				else
				{
					SimpleButton simpleButton2 = new SimpleButton(this, pages[0], Translate("RESTART"), "RESTART", new Vector2(ContinueAndExitButtonsXPos - 180f - manager.rainWorld.options.SafeScreenOffset.x, Mathf.Max(manager.rainWorld.options.SafeScreenOffset.y, 15f)), new Vector2(110f, 30f));
					SimpleButton simpleButton3 = new SimpleButton(this, pages[0], Translate("EDIT"), "EDIT", new Vector2(ContinueAndExitButtonsXPos - 320f - manager.rainWorld.options.SafeScreenOffset.x, Mathf.Max(manager.rainWorld.options.SafeScreenOffset.y, 15f)), new Vector2(110f, 30f));
					pages[0].subObjects.Add(simpleButton2);
					pages[0].subObjects.Add(simpleButton3);
					simpleButton2.nextSelectable[1] = simpleButton2;
					simpleButton2.nextSelectable[3] = simpleButton2;
					simpleButton3.nextSelectable[1] = simpleButton3;
					simpleButton3.nextSelectable[3] = simpleButton3;
					moveLeft = 280f;
				}
			}
			else
			{
				SimpleButton simpleButton4 = new SimpleButton(this, pages[0], Translate("PLAY"), "PLAY", new Vector2(ContinueAndExitButtonsXPos - 180f - manager.rainWorld.options.SafeScreenOffset.x, Mathf.Max(manager.rainWorld.options.SafeScreenOffset.y, 15f)), new Vector2(110f, 30f));
				pages[0].subObjects.Add(simpleButton4);
				simpleButton4.nextSelectable[1] = simpleButton4;
				simpleButton4.nextSelectable[3] = simpleButton4;
				moveLeft = 140f;
			}
		}
		SpawnExitContinueButtons();
		selectedObject = null;
		blackFade = 0f;
		lastBlackFade = 0f;
		micVolumes = new float[game.cameras.Length, game.cameras[0].virtualMicrophone.volumeGroups.Length];
		for (int i = 0; i < game.cameras.Length; i++)
		{
			for (int j = 0; j < game.cameras[0].virtualMicrophone.volumeGroups.Length; j++)
			{
				micVolumes[i, j] = game.cameras[0].virtualMicrophone.volumeGroups[j];
			}
		}
		PlaySound(SoundID.HUD_Pause_Game);
		pauseWarningActive = false;
		for (int k = 0; k < game.cameras.Length; k++)
		{
			if (game.cameras[k].hud == null || game.cameras[k].hud.textPrompt == null)
			{
				continue;
			}
			game.cameras[k].hud.textPrompt.pausedMode = true;
			if (game.IsStorySession && game.GetStorySession.saveState.cycleNumber > 0)
			{
				if (game.clock > 200 && (game.GetStorySession.RedIsOutOfCycles || (ModManager.Expedition && game.rainWorld.ExpeditionMode && game.GetStorySession.saveState.deathPersistentSaveData.karma == 0)))
				{
					pauseWarningActive = true;
					game.cameras[k].hud.textPrompt.pausedWarningText = true;
				}
				else if (game.clock > 1200)
				{
					pauseWarningActive = true;
					if (game.manager.rainWorld.progression.miscProgressionData.warnedAboutKarmaLossOnExit < 4)
					{
						game.cameras[k].hud.textPrompt.pausedWarningText = true;
						game.manager.rainWorld.progression.miscProgressionData.warnedAboutKarmaLossOnExit++;
					}
				}
			}
			else
			{
				game.cameras[k].hud.textPrompt.pausedWarningText = false;
			}
		}
		for (int l = 0; l < 4; l++)
		{
			PlayerHandler playerHandler = manager.rainWorld.GetPlayerHandler(l);
			if (playerHandler != null)
			{
				playerHandler.ControllerHandler.SetRumblePaused(paused: true);
			}
		}
		if (ModManager.Expedition && manager.rainWorld.ExpeditionMode && ExpeditionGame.activeUnlocks.Count > 0)
		{
			pages[0].subObjects.Add(new UnlocksIndicator(this, pages[0], new Vector2(683f, manager.rainWorld.screenSize.y - 60f), centered: true));
		}
		WarpInit(game);
	}

	public override void Update()
	{
		counter++;
		if (game.IsStorySession && continueButton != null && exitButton != null)
		{
			continueButton.buttonBehav.greyedOut = wantToContinue;
			exitButton.buttonBehav.greyedOut = wantToContinue || counter < 40;
		}
		else
		{
			for (int i = 0; i < pages[0].subObjects.Count; i++)
			{
				if (pages[0].subObjects[i] is SimpleButton)
				{
					(pages[0].subObjects[i] as SimpleButton).buttonBehav.greyedOut = wantToContinue;
				}
			}
		}
		if (game.IsArenaSession && game.GetArenaGameSession is SandboxGameSession && (game.GetArenaGameSession as SandboxGameSession).overlay != null)
		{
			(game.GetArenaGameSession as SandboxGameSession).overlay.Update();
		}
		bool flag = RWInput.CheckPauseButton(0);
		if (flag && !lastPauseButton && counter > 10)
		{
			wantToContinue = true;
			PlaySound(SoundID.HUD_Unpause_Game);
		}
		lastPauseButton = flag;
		lastBlackFade = blackFade;
		if (wantToContinue)
		{
			blackFade = Mathf.Max(0f, blackFade - 0.125f);
			if (blackFade <= 0f)
			{
				game.ContinuePaused();
			}
		}
		else
		{
			blackFade = Mathf.Min(1f, blackFade + 0.0625f);
		}
		for (int j = 0; j < game.cameras.Length; j++)
		{
			for (int k = 0; k < game.cameras[j].virtualMicrophone.volumeGroups.Length; k++)
			{
				if (k == 1)
				{
					game.cameras[j].virtualMicrophone.volumeGroups[k] = micVolumes[j, k];
				}
				else
				{
					game.cameras[j].virtualMicrophone.volumeGroups[k] = micVolumes[j, k] * (1f - blackFade * 0.5f);
				}
			}
		}
		base.Update();
		WarpUpdate();
	}

	public override void GrafUpdate(float timeStacker)
	{
		float num = Custom.SCurve(Mathf.Lerp(lastBlackFade, blackFade, timeStacker), 0.6f);
		blackSprite.alpha = num * 0.25f;
		base.GrafUpdate(timeStacker);
		if (game.IsArenaSession && game.GetArenaGameSession is SandboxGameSession && (game.GetArenaGameSession as SandboxGameSession).overlay != null)
		{
			(game.GetArenaGameSession as SandboxGameSession).overlay.GrafUpdate(timeStacker);
		}
		for (int i = 0; i < game.cameras.Length; i++)
		{
			game.cameras[i].virtualMicrophone.DrawUpdate(timeStacker, 1f - 0.3f * Mathf.Lerp(lastBlackFade, blackFade, timeStacker));
		}
	}

	public void SpawnExitContinueButtons()
	{
		if (confirmYesButton != null)
		{
			confirmYesButton.RemoveSprites();
			pages[0].RemoveSubObject(confirmYesButton);
		}
		confirmYesButton = null;
		if (confirmNoButton != null)
		{
			confirmNoButton.RemoveSprites();
			pages[0].RemoveSubObject(confirmNoButton);
		}
		confirmNoButton = null;
		if (confirmMessage != null)
		{
			confirmMessage.RemoveSprites();
			pages[0].RemoveSubObject(confirmMessage);
		}
		confirmMessage = null;
		continueButton = new SimpleButton(this, pages[0], Translate("CONTINUE"), "CONTINUE", new Vector2(ContinueAndExitButtonsXPos - 180.2f - moveLeft - manager.rainWorld.options.SafeScreenOffset.x, Mathf.Max(manager.rainWorld.options.SafeScreenOffset.y, 15f)), new Vector2(110f, 30f));
		pages[0].subObjects.Add(continueButton);
		exitButton = new SimpleButton(this, pages[0], Translate("EXIT"), "EXIT", new Vector2(ContinueAndExitButtonsXPos - 320.2f - moveLeft - manager.rainWorld.options.SafeScreenOffset.x, Mathf.Max(manager.rainWorld.options.SafeScreenOffset.y, 15f)), new Vector2(110f, 30f));
		pages[0].subObjects.Add(exitButton);
		selectedObject = continueButton;
		continueButton.nextSelectable[1] = continueButton;
		continueButton.nextSelectable[3] = continueButton;
		exitButton.nextSelectable[1] = exitButton;
		exitButton.nextSelectable[3] = exitButton;
	}

	public void SpawnConfirmButtons()
	{
		if (continueButton != null)
		{
			continueButton.RemoveSprites();
			pages[0].RemoveSubObject(continueButton);
		}
		continueButton = null;
		if (exitButton != null)
		{
			exitButton.RemoveSprites();
			pages[0].RemoveSubObject(exitButton);
		}
		exitButton = null;
		confirmYesButton = new SimpleButton(this, pages[0], Translate("YES"), "YES_EXIT", new Vector2(ContinueAndExitButtonsXPos - 180.2f - moveLeft - manager.rainWorld.options.SafeScreenOffset.x, Mathf.Max(manager.rainWorld.options.SafeScreenOffset.y, 15f)), new Vector2(110f, 30f));
		pages[0].subObjects.Add(confirmYesButton);
		confirmNoButton = new SimpleButton(this, pages[0], Translate("NO"), "NO_EXIT", new Vector2(ContinueAndExitButtonsXPos - 320.2f - moveLeft - manager.rainWorld.options.SafeScreenOffset.x, Mathf.Max(manager.rainWorld.options.SafeScreenOffset.y, 15f)), new Vector2(110f, 30f));
		pages[0].subObjects.Add(confirmNoButton);
		selectedObject = confirmNoButton;
		confirmMessage = new MenuLabel(this, pages[0], Translate("Really exit? Note that quitting after 30 seconds into a cycle counts as a loss."), new Vector2(confirmNoButton.pos.x, confirmNoButton.pos.y), new Vector2(10f, 30f), bigText: false);
		confirmMessage.label.alignment = FLabelAlignment.Left;
		confirmMessage.pos = new Vector2(confirmMessage.pos.x - confirmMessage.label.textRect.width - 40f, confirmMessage.pos.y);
		pages[0].subObjects.Add(confirmMessage);
		confirmYesButton.nextSelectable[1] = confirmYesButton;
		confirmYesButton.nextSelectable[3] = confirmYesButton;
		confirmNoButton.nextSelectable[1] = confirmNoButton;
		confirmNoButton.nextSelectable[3] = confirmNoButton;
	}

	public override void Singal(MenuObject sender, string message)
	{
		switch (message)
		{
		case "CONTINUE":
			wantToContinue = true;
			PlaySound(SoundID.HUD_Unpause_Game);
			break;
		case "EXIT":
		{
			bool flag = game.IsStorySession && (!ModManager.MSC || !game.rainWorld.safariMode) && pauseWarningActive;
			PlaySound(flag ? SoundID.MENU_Button_Standard_Button_Pressed : SoundID.HUD_Exit_Game);
			if (flag)
			{
				SpawnConfirmButtons();
				break;
			}
			game.ExitToMenu();
			if (game.IsArenaSession && game.GetArenaGameSession is SandboxGameSession && (game.GetArenaGameSession as SandboxGameSession).editor != null)
			{
				(game.GetArenaGameSession as SandboxGameSession).editor.SaveConfig();
			}
			ShutDownProcess();
			break;
		}
		case "NO_EXIT":
			PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
			SpawnExitContinueButtons();
			break;
		case "YES_EXIT":
			PlaySound(SoundID.HUD_Exit_Game);
			game.ExitToMenu();
			if (game.IsArenaSession && game.GetArenaGameSession is SandboxGameSession && (game.GetArenaGameSession as SandboxGameSession).editor != null)
			{
				(game.GetArenaGameSession as SandboxGameSession).editor.SaveConfig();
			}
			ShutDownProcess();
			break;
		case "RESTART":
			PlaySound(SoundID.HUD_Exit_Game);
			if (game.IsArenaSession && game.GetArenaGameSession.GameTypeSetup.gameType == ArenaSetup.GameTypeID.Sandbox)
			{
				Custom.Log("~~~~~ ARENA RESTART");
				game.GetArenaGameSession.arenaSitting.NextLevel(game.manager);
			}
			else
			{
				manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Game);
				ShutDownProcess();
			}
			break;
		case "EDIT":
			PlaySound(SoundID.HUD_Exit_Game);
			game.GetArenaGameSession.arenaSitting.sandboxPlayMode = false;
			manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Game);
			ShutDownProcess();
			break;
		case "PLAY":
			PlaySound(SoundID.HUD_Unpause_Game);
			(game.GetArenaGameSession as SandboxGameSession).editor.Play();
			break;
		}
		WarpSignal(sender, message);
	}

	public override void ShutDownProcess()
	{
		for (int i = 0; i < 4; i++)
		{
			PlayerHandler playerHandler = game.rainWorld.GetPlayerHandler(i);
			if (playerHandler != null)
			{
				playerHandler.ControllerHandler.SetRumblePaused(paused: false);
			}
		}
		for (int j = 0; j < game.cameras.Length; j++)
		{
			for (int k = 0; k < game.cameras[j].virtualMicrophone.volumeGroups.Length; k++)
			{
				game.cameras[j].virtualMicrophone.volumeGroups[k] = micVolumes[j, k];
			}
		}
		blackSprite.RemoveFromContainer();
		base.ShutDownProcess();
	}
}
