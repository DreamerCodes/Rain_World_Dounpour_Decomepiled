using System;
using System.Collections.Generic;
using JollyCoop;
using Menu;
using RWCustom;
using UnityEngine;

namespace HUD;

public class TextPrompt : HudPart
{
	public class InfoID : ExtEnum<InfoID>
	{
		public static readonly InfoID Nothing = new InfoID("Nothing", register: true);

		public static readonly InfoID Message = new InfoID("Message", register: true);

		public static readonly InfoID Paused = new InfoID("Paused", register: true);

		public static readonly InfoID GameOver = new InfoID("GameOver", register: true);

		public static readonly InfoID CyclesTickUp = new InfoID("CyclesTickUp", register: true);

		public InfoID(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class Message
	{
		public string text;

		public int wait;

		public int time;

		public bool fadeOut;

		public bool darken;

		public bool hideHud;

		public Message(string text, int wait, int time, bool darken, bool hideHud)
		{
			this.text = text;
			this.wait = wait;
			this.time = time;
			this.darken = darken;
			this.hideHud = hideHud;
		}
	}

	public class SymbolsMessage : Message
	{
		public List<MultiplayerUnlocks.SandboxUnlockID> iconIDs;

		public float iconsX;

		public SymbolsMessage(string text, int wait, int time, bool darken, bool hideHud, float iconsX, List<MultiplayerUnlocks.SandboxUnlockID> iconIDs)
			: base(text, wait, time, darken, hideHud)
		{
			this.iconsX = iconsX;
			this.iconIDs = iconIDs;
		}
	}

	public class MusicMessage : Message
	{
		public MusicMessage(string text, int wait, int time, bool darken, bool hideHud)
			: base(text, wait, time, darken, hideHud)
		{
		}
	}

	private FLabel label;

	private FSprite fullscreenFade;

	private FSprite[] sprites;

	private FSprite musicSprite;

	private bool lastAnyKey;

	private int restartNotAllowed;

	public int foodInStomach;

	public int deathRoom;

	public Vector2 deathPos;

	public bool gameOverMode;

	public bool playGameOverSound;

	public bool pausedMode;

	public bool pausedWarningText;

	public Creature.Grasp dependentOnGrasp;

	public List<Message> messages;

	public float show;

	public float lastShow;

	public float pauseModeHeight;

	public float lastPauseModeHeight;

	public bool hideHud;

	public SubregionTracker subregionTracker;

	public float foodVisibleMode;

	public float lastFoodVisibleMode;

	public float sin;

	public float lastSin;

	private string gameOverString;

	private string messageString;

	public InfoID currentlyShowing = InfoID.Nothing;

	public int cycleTick = -1;

	public bool[] defaultPauseControls;

	public bool[] defaultMapControls;

	public float symbolsX;

	public List<IconSymbol> symbols = new List<IconSymbol>();

	public Options.ControlSetup.Preset lastControllerType;

	public TextPrompt(HUD hud)
		: base(hud)
	{
		sprites = new FSprite[2];
		for (int i = 0; i < 2; i++)
		{
			sprites[i] = new FSprite("pixel");
			sprites[i].color = new Color(0f, 0f, 0f);
			sprites[i].scaleX = 1400f;
			sprites[i].anchorY = ((i == 0) ? 0f : 1f);
			sprites[i].x = hud.rainWorld.screenSize.x / 2f;
			sprites[i].y = ((i == 0) ? (hud.rainWorld.screenSize.y + 1f) : (-1f));
			sprites[i].scaleY = 100f;
			hud.fContainers[1].AddChild(sprites[i]);
		}
		fullscreenFade = new FSprite("Futile_White");
		fullscreenFade.color = new Color(0f, 0f, 0f);
		fullscreenFade.scaleX = 87.5f;
		fullscreenFade.scaleY = 50f;
		fullscreenFade.x = hud.rainWorld.screenSize.x / 2f;
		fullscreenFade.y = hud.rainWorld.screenSize.y / 2f;
		hud.fContainers[1].AddChild(fullscreenFade);
		label = new FLabel(Custom.GetDisplayFont(), "");
		label.x = hud.rainWorld.options.SafeScreenOffset.x;
		label.y = hud.rainWorld.options.SafeScreenOffset.y;
		hud.fContainers[1].AddChild(label);
		restartNotAllowed = 40;
		lastAnyKey = true;
		messages = new List<Message>();
		if (hud.owner.GetOwnerType() == HUD.OwnerType.Player && (hud.owner as Player).abstractCreature.world.region != null)
		{
			subregionTracker = new SubregionTracker(this);
		}
		defaultPauseControls = new bool[hud.rainWorld.options.controls.Length];
		defaultMapControls = new bool[hud.rainWorld.options.controls.Length];
		for (int j = 0; j < defaultPauseControls.Length; j++)
		{
			defaultPauseControls[j] = hud.rainWorld.options.controls[j].GameActionMatchesTemplate(5);
			defaultMapControls[j] = hud.rainWorld.options.controls[j].GameActionMatchesTemplate(11);
		}
		lastControllerType = hud.rainWorld.options.controls[0].GetActivePreset();
		UpdateGameOverString(lastControllerType);
	}

	public override void Update()
	{
		lastShow = show;
		lastPauseModeHeight = pauseModeHeight;
		pauseModeHeight = Custom.LerpAndTick(pauseModeHeight, (pausedMode || (messages.Count > 0 && messages[0] is SymbolsMessage)) ? 1f : 0f, 0.01f, 1f / 30f);
		lastSin = sin;
		if (gameOverMode || (messages.Count > 0 && messages[0].darken))
		{
			sin += 1f;
		}
		lastFoodVisibleMode = foodVisibleMode;
		bool flag = false;
		for (int i = 0; i < symbols.Count; i++)
		{
			symbols[i].Update();
		}
		InfoID infoID = InfoID.Nothing;
		if (gameOverMode)
		{
			infoID = InfoID.GameOver;
			if (currentlyShowing == InfoID.GameOver && playGameOverSound && restartNotAllowed < 20)
			{
				hud.PlaySound(SoundID.HUD_Game_Over_Prompt);
				playGameOverSound = false;
			}
			if (ModManager.CoopAvailable)
			{
				if (dependentOnGrasp != null && hud.owner is Player && (hud.owner as Player).abstractCreature.world.game.AlivePlayers.Count > 0 && dependentOnGrasp.discontinued)
				{
					JollyCustom.Log("[Jolly] Gameover discontinued since there are players who can help");
					gameOverMode = false;
					return;
				}
			}
			else if (dependentOnGrasp != null && hud.owner is Player && !(hud.owner as Player).dead && dependentOnGrasp.discontinued)
			{
				gameOverMode = false;
				return;
			}
			bool flag2 = false;
			for (int j = 0; j < hud.rainWorld.options.controls.Length; j++)
			{
				flag2 = ((hud.rainWorld.options.controls[j].gamePad || !defaultMapControls[j]) ? (flag2 || hud.rainWorld.options.controls[j].GetButton(5) || RWInput.CheckPauseButton(0, inMenu: false)) : (flag2 || hud.rainWorld.options.controls[j].GetButton(11)));
			}
			if (restartNotAllowed > 0)
			{
				restartNotAllowed--;
			}
			else
			{
				if (flag2 && !lastAnyKey)
				{
					(hud.rainWorld.processManager.currentMainLoop as RainWorldGame).GoToDeathScreen();
				}
				lastAnyKey = flag2;
			}
		}
		else if (cycleTick > -1)
		{
			if (!(hud.owner is Player) || !(hud.owner as Player).room.game.IsStorySession)
			{
				cycleTick = -1;
			}
			else
			{
				infoID = InfoID.CyclesTickUp;
				if (currentlyShowing == InfoID.CyclesTickUp)
				{
					cycleTick++;
					int num = RedsIllness.RedsCycles(extraCycles: false) - (hud.owner as Player).room.game.GetStorySession.saveState.cycleNumber;
					num += (int)Custom.LerpMap(cycleTick, 60f, 120f, 0f, RedsIllness.RedsCycles(extraCycles: true) - RedsIllness.RedsCycles(extraCycles: false));
					label.text = hud.rainWorld.inGameTranslator.Translate("Cycle") + " " + num;
					if (cycleTick > 180)
					{
						cycleTick = -1;
					}
				}
			}
		}
		else if (pausedMode)
		{
			if (hud.rainWorld.processManager.currentMainLoop is RainWorldGame && (hud.rainWorld.processManager.currentMainLoop as RainWorldGame).pauseMenu == null)
			{
				pausedMode = false;
			}
			else
			{
				infoID = InfoID.Paused;
			}
		}
		else
		{
			if (subregionTracker != null)
			{
				subregionTracker.Update();
			}
			infoID = ((messages.Count <= 0 || messages[0].fadeOut || messages[0].wait > 0) ? InfoID.Nothing : InfoID.Message);
			if (messages.Count > 0)
			{
				if (!messages[0].hideHud && hud.foodMeter != null && hud.foodMeter.fade > 0f)
				{
					flag = true;
				}
				if (messages[0].wait > 0)
				{
					messages[0].wait--;
					if (messages.Count > 1)
					{
						messages[0].wait = 0;
					}
				}
				else if (show == 1f)
				{
					messages[0].time--;
					if (hud.owner.RevealMap)
					{
						messages[0].time = Custom.IntClamp(messages[0].time - 4, 0, 20);
					}
					if (messages.Count > 1)
					{
						messages[0].time--;
					}
					if (messages[0].time < 1)
					{
						messages[0].fadeOut = true;
					}
				}
				else if (messages[0].fadeOut && show == 0f)
				{
					messages.RemoveAt(0);
					if (messages.Count > 0)
					{
						InitNextMessage();
					}
				}
			}
		}
		if (currentlyShowing == InfoID.GameOver)
		{
			Options.ControlSetup.Preset activePreset = hud.rainWorld.options.controls[0].GetActivePreset();
			if (activePreset != lastControllerType)
			{
				UpdateGameOverString(activePreset);
				lastControllerType = activePreset;
				label.text = hud.rainWorld.inGameTranslator.Translate(gameOverString);
			}
		}
		if (infoID == currentlyShowing)
		{
			if (currentlyShowing == InfoID.Nothing)
			{
				show = 0f;
			}
			else
			{
				show = Mathf.Min(1f, show + 1f / 30f);
			}
		}
		else
		{
			show = Mathf.Max(0f, show - 1f / 30f);
			if (show == 0f && lastShow == 0f)
			{
				currentlyShowing = infoID;
				if (currentlyShowing == InfoID.Message)
				{
					label.text = messageString;
				}
				else if (currentlyShowing == InfoID.Paused)
				{
					if (pausedWarningText)
					{
						if ((hud.owner as Player).abstractCreature.world.game.IsStorySession && ((hud.owner as Player).abstractCreature.world.game.GetStorySession.RedIsOutOfCycles || (ModManager.Expedition && hud.rainWorld.ExpeditionMode && (hud.owner as Player).abstractCreature.world.game.GetStorySession.saveState.deathPersistentSaveData.karma == 0)))
						{
							label.text = hud.rainWorld.inGameTranslator.Translate("Paused - Warning! Quitting without saving in a shelter will permanently end your game");
						}
						else
						{
							label.text = hud.rainWorld.inGameTranslator.Translate("Paused - Warning! Quitting after 30 seconds into a cycle counts as a loss");
						}
					}
					else
					{
						label.text = hud.rainWorld.inGameTranslator.Translate("Paused");
					}
				}
				else if (currentlyShowing == InfoID.GameOver)
				{
					label.text = hud.rainWorld.inGameTranslator.Translate(gameOverString);
				}
				else if (!(currentlyShowing == InfoID.CyclesTickUp))
				{
					label.text = "";
				}
			}
		}
		foodVisibleMode = Custom.LerpAndTick(foodVisibleMode, flag ? 1f : 0f, 0.05f, 0.05f);
	}

	public void UpdateGameOverString(Options.ControlSetup.Preset controllerType)
	{
		gameOverString = "Game Over - ";
		if (controllerType == Options.ControlSetup.Preset.PS4DualShock || controllerType == Options.ControlSetup.Preset.PS5DualSense)
		{
			gameOverString += (defaultPauseControls[0] ? "Press OPTIONS to restart" : "Press PAUSE BUTTON to restart");
		}
		else if (controllerType == Options.ControlSetup.Preset.XBox)
		{
			gameOverString += (defaultPauseControls[0] ? "Press VIEW to restart" : "Press PAUSE BUTTON to restart");
		}
		else if (controllerType == Options.ControlSetup.Preset.SwitchHandheld || controllerType == Options.ControlSetup.Preset.SwitchDualJoycon || controllerType == Options.ControlSetup.Preset.SwitchProController || controllerType == Options.ControlSetup.Preset.SwitchSingleJoyconR)
		{
			gameOverString += (defaultPauseControls[0] ? "Press + Button to restart" : "Press PAUSE BUTTON to restart");
		}
		else if (controllerType == Options.ControlSetup.Preset.SwitchSingleJoyconL)
		{
			gameOverString += (defaultPauseControls[0] ? "Press - Button to restart" : "Press PAUSE BUTTON to restart");
		}
		else
		{
			gameOverString += (defaultMapControls[0] ? "Press SPACE to restart" : "Press PAUSE BUTTON to restart");
		}
	}

	public float LowerBorderHeight(float timeStacker)
	{
		float num = Mathf.Lerp(30f + hud.rainWorld.options.SafeScreenOffset.x, 60f + hud.rainWorld.options.SafeScreenOffset.y, Mathf.Lerp(lastPauseModeHeight, pauseModeHeight, timeStacker)) * Custom.SCurve(Mathf.InverseLerp(0f, 0.5f, Mathf.Lerp(lastShow, show, timeStacker)), 0.5f);
		if (hud.foodMeter != null)
		{
			return Mathf.Lerp(num, hud.foodMeter.pos.y + 30f + ((hud.karmaMeter != null) ? Mathf.Max(0f, hud.karmaMeter.rad * hud.karmaMeter.fade - 2.5f) : 0f), Custom.SCurve(Mathf.Lerp(lastFoodVisibleMode, foodVisibleMode, timeStacker), 0.7f));
		}
		return num;
	}

	public override void Draw(float timeStacker)
	{
		base.Draw(timeStacker);
		float value = Mathf.Lerp(lastShow, show, timeStacker);
		if (!pausedMode && !gameOverMode && messages.Count > 0 && messages[0].darken)
		{
			fullscreenFade.isVisible = true;
			fullscreenFade.alpha = Custom.SCurve(Mathf.InverseLerp(0f, 0.5f, value), 0.5f) * 0.25f;
		}
		else
		{
			fullscreenFade.isVisible = false;
		}
		if (gameOverMode || cycleTick > -1 || (messages.Count > 0 && messages[0].darken))
		{
			label.color = Color.Lerp(global::Menu.Menu.MenuRGB(global::Menu.Menu.MenuColors.MediumGrey), global::Menu.Menu.MenuRGB(global::Menu.Menu.MenuColors.White), 0.5f + 0.5f * Mathf.Sin(Mathf.Lerp(lastSin, sin, timeStacker) / 60f * (float)Math.PI * 2f));
		}
		else
		{
			label.color = global::Menu.Menu.MenuRGB(global::Menu.Menu.MenuColors.MediumGrey);
		}
		label.alignment = FLabelAlignment.Left;
		if (hud.foodMeter != null && (lastFoodVisibleMode > 0f || foodVisibleMode > 0f))
		{
			label.x = hud.rainWorld.options.SafeScreenOffset.x + Mathf.Lerp(20f, hud.foodMeter.pos.x + hud.foodMeter.TotalWidth(timeStacker) + 25f, Custom.SCurve(Mathf.Lerp(lastFoodVisibleMode, foodVisibleMode, timeStacker), 0.7f)) + 0.01f;
		}
		else
		{
			label.x = hud.rainWorld.options.SafeScreenOffset.x + 20.01f;
		}
		label.y = hud.rainWorld.options.SafeScreenOffset.y + 15.01f;
		if (currentlyShowing == InfoID.Message && messages.Count > 0 && messages[0] is SymbolsMessage)
		{
			label.y += 15f;
		}
		label.alpha = Custom.SCurve(Mathf.InverseLerp(0.5f, 1f, value), 0.8f);
		if (messages.Count > 0 && messages[0] is MusicMessage)
		{
			musicSprite.x = label.x + 9f + 0.01f;
			musicSprite.y = label.y - 2f + 0.01f;
			musicSprite.alpha = label.alpha;
			musicSprite.color = label.color;
			sprites[0].y = hud.rainWorld.screenSize.y + 30f + hud.rainWorld.options.SafeScreenOffset.y;
		}
		else
		{
			sprites[0].y = hud.rainWorld.screenSize.y - (30f + hud.rainWorld.options.SafeScreenOffset.y) * Custom.SCurve(Mathf.InverseLerp(0f, 0.5f, value), 0.5f);
		}
		sprites[1].y = LowerBorderHeight(timeStacker);
		for (int i = 0; i < symbols.Count; i++)
		{
			symbolsX = label.textRect.width + 45f;
			symbols[i].Draw(timeStacker, new Vector2(symbolsX + (float)i * 35f + hud.rainWorld.options.SafeScreenOffset.x, label.y - 2.5f));
			symbols[i].symbolSprite.alpha = Custom.SCurve(Mathf.InverseLerp(0.75f, 1f, value), 0.6f) * ((currentlyShowing == InfoID.Message && messages.Count > 0 && messages[0] is SymbolsMessage) ? 1f : 0f);
		}
	}

	public override void ClearSprites()
	{
		base.ClearSprites();
		label.RemoveFromContainer();
		for (int i = 0; i < 2; i++)
		{
			sprites[i].RemoveFromContainer();
		}
		for (int j = 0; j < symbols.Count; j++)
		{
			symbols[j].RemoveSprites();
		}
		if (musicSprite != null)
		{
			musicSprite.RemoveFromContainer();
		}
	}

	public void EnterGameOverMode(Creature.Grasp dependentOnGrasp, int foodInStomach, int deathRoom, Vector2 deathPos)
	{
		restartNotAllowed = 40;
		this.dependentOnGrasp = dependentOnGrasp;
		this.foodInStomach = foodInStomach;
		this.deathRoom = deathRoom;
		this.deathPos = deathPos;
		gameOverMode = true;
		playGameOverSound = true;
		if (ModManager.CoopAvailable && dependentOnGrasp == null)
		{
			playGameOverSound = false;
		}
	}

	public void AddMessage(string text, int wait, int time, bool darken, bool hideHud)
	{
		messages.Add(new Message(text, wait, time, darken, hideHud));
		if (messages.Count == 1)
		{
			InitNextMessage();
		}
	}

	public void AddMessage(string text, int wait, int time, bool darken, bool hideHud, float iconsX, List<MultiplayerUnlocks.SandboxUnlockID> iconIDs)
	{
		messages.Add(new SymbolsMessage(text, wait, time, darken, hideHud, iconsX, iconIDs));
		if (messages.Count == 1)
		{
			InitNextMessage();
		}
	}

	public void AddMusicMessage(string text, int time)
	{
		messages.Add(new MusicMessage("    ~ " + text, 0, time, darken: false, hideHud: false));
		if (messages.Count == 1)
		{
			InitNextMessage();
		}
	}

	private void InitNextMessage()
	{
		messageString = messages[0].text;
		for (int i = 0; i < symbols.Count; i++)
		{
			symbols[i].RemoveSprites();
		}
		symbols.Clear();
		if (messages[0] is SymbolsMessage)
		{
			symbolsX = (messages[0] as SymbolsMessage).iconsX;
			for (int j = 0; j < (messages[0] as SymbolsMessage).iconIDs.Count; j++)
			{
				symbols.Add(IconSymbol.CreateIconSymbol(MultiplayerUnlocks.SymbolDataForSandboxUnlock((messages[0] as SymbolsMessage).iconIDs[j]), hud.fContainers[1]));
			}
			for (int k = 0; k < symbols.Count; k++)
			{
				symbols[k].Show(showShadowSprites: false);
			}
		}
		else if (messages[0] is MusicMessage && musicSprite == null)
		{
			musicSprite = new FSprite("musicSymbol");
			musicSprite.x = -1000f;
			hud.fContainers[1].AddChild(musicSprite);
		}
		hideHud = messages[0].hideHud;
	}
}
