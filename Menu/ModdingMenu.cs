using System;
using System.Collections.Generic;
using System.Linq;
using Menu.Remix;
using Menu.Remix.MixedUI;
using RWCustom;
using UnityEngine;

namespace Menu;

public class ModdingMenu : Menu
{
	public ControlMap _controlMap;

	private FSprite _controlDarkSprite;

	internal float _blackFade;

	private float _lastBlackFade;

	internal SteamWorkshopUploader workshopUploader;

	private DialogBoxMultiButtonNotify workshopUploadConfirmDialog;

	private const string workshopUploadSignal = "WORKSHOP_UPLOAD";

	private const string workshopCancelSignal = "WORKSHOP_CANCEL";

	private ModManager.Mod modToUpload;

	private const int _DASinit = 20;

	private const int _DASdelay = 6;

	private string description = "";

	private string lastDescription = "";

	private MenuLabel alertLabel;

	private string alertText = "";

	private float alertLabelFade;

	private float lastAlertLabelFade;

	private float alertLabelSin;

	public static ModdingMenu instance;

	private readonly FSprite darkSprite;

	internal ConfigContainer cfgContainer;

	internal bool isReload;

	internal ConfigContainer.Mode? lastMode;

	private ModManager.ModApplyer modApplyer;

	private string modApplyerError;

	private ModManager.ModApplyFallbackStep modApplyerFallbackStep;

	private bool modApplyerRequiresRestart;

	private List<ModManager.Mod> fallbackActiveMods;

	private List<int> fallbackLoadOrder;

	private DialogBoxAsyncWait applyingModsDialog;

	private DialogBoxNotify requiresRestartDialog;

	private DialogBoxNotify applyingModsErrorDialog;

	internal MenuDialogBox modCalledDialog;

	internal Action[] modCalledDialogActions;

	internal const string restartSignal = "RESTART";

	internal const string modDialog = "MOD_DIALOG_";

	internal const string reloadSignal = "RELOAD";

	internal const string errorSignal = "AFTERERROR";

	public static int DASinit => UIelement.FrameMultiply(20);

	public static int DASdelay => UIelement.FrameMultiply(6);

	internal bool HasDialogBox { get; private set; }

	public ModdingMenu(ProcessManager manager)
		: base(manager, ProcessManager.ProcessID.ModdingMenu)
	{
		instance = this;
		pages.Add(new Page(this, null, "hub", 0));
		scene = new InteractiveMenuScene(this, pages[0], ModManager.MMF ? manager.rainWorld.options.subBackground : MenuScene.SceneID.Landscape_SU);
		pages[0].subObjects.Add(scene);
		darkSprite = new FSprite("pixel")
		{
			color = new Color(0.01f, 0.01f, 0.01f),
			anchorX = 0f,
			anchorY = 0f,
			scaleX = 1368f,
			scaleY = 770f,
			x = -1f,
			y = -1f,
			alpha = 0.85f
		};
		pages[0].Container.AddChild(darkSprite);
		alertLabel = new MenuLabel(this, pages[0], "", new Vector2(383f, 735f), new Vector2(600f, 30f), bigText: false);
		pages[0].subObjects.Add(alertLabel);
	}

	internal void DisplayWorkshopUploadConfirmDialog(ModManager.Mod mod)
	{
		modToUpload = mod;
		workshopUploadConfirmDialog = new DialogBoxMultiButtonNotify(this, pages[0], Translate("mod_workshop_upload_ask"), new string[2] { "WORKSHOP_UPLOAD", "WORKSHOP_CANCEL" }, new string[2]
		{
			Translate("UPLOAD"),
			Translate("CANCEL")
		}, new Vector2(manager.rainWorld.options.ScreenSize.x / 2f - 240f + (1366f - manager.rainWorld.options.ScreenSize.x) / 2f, 224f), new Vector2(480f, 320f));
		pages[0].subObjects.Add(workshopUploadConfirmDialog);
	}

	internal void ShowDescription(string text)
	{
		description = text;
	}

	internal void ShowAlert(string text)
	{
		alertText = text;
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		alertLabel.label.alpha = Custom.SCurve(Mathf.Clamp01(Mathf.Lerp(lastAlertLabelFade, alertLabelFade, timeStacker)), 0.3f);
		if (lastAlertLabelFade > 0f)
		{
			alertLabel.label.color = Color.Lerp(Menu.MenuRGB(MenuColors.White), Menu.MenuRGB(MenuColors.MediumGrey), 0.5f + 0.5f * Mathf.Sin((alertLabelSin + timeStacker) / 4f));
		}
		if (_controlDarkSprite != null)
		{
			_controlDarkSprite.alpha = Custom.SCurve(Mathf.Lerp(_lastBlackFade, _blackFade, timeStacker), 0.6f) * 0.8f;
		}
	}

	protected override void Init()
	{
		base.Init();
		if (!isReload)
		{
			LabelTest.Initialize(this);
		}
		cfgContainer = new ConfigContainer(this, pages[0]);
		pages[0].subObjects.Add(cfgContainer);
		_controlDarkSprite = new FSprite("pixel")
		{
			color = new Color(0.01f, 0.01f, 0.01f),
			anchorX = 0f,
			anchorY = 0f,
			scaleX = 1368f,
			scaleY = 770f,
			x = -1f,
			y = -1f,
			alpha = 0f
		};
		pages[0].Container.AddChild(_controlDarkSprite);
		_blackFade = 0f;
		_lastBlackFade = 0f;
		Options.ControlSetup.Preset xBox = Options.ControlSetup.Preset.XBox;
		_controlMap = new ControlMap(this, pages[0], new Vector2(manager.rainWorld.screenSize.x / 3f + (1466f - manager.rainWorld.screenSize.x), 350f), xBox, showPickupInstructions: false);
		pages[0].subObjects.Add(_controlMap);
	}

	public override void Update()
	{
		if (!string.IsNullOrEmpty(description) && string.IsNullOrEmpty(UpdateInfoText()))
		{
			infoLabelFade = 1f;
			infoLabel.text = description;
			if (lastDescription != description)
			{
				infolabelDirty = false;
				infoLabelSin = 0f;
			}
		}
		lastDescription = description;
		lastInfoLabelFade = infoLabelFade;
		if (!string.IsNullOrEmpty(alertText))
		{
			alertLabelFade = 2f;
			lastAlertLabelFade = 2f;
			alertLabelSin = 0f;
			alertLabel.text = alertText;
			alertText = null;
		}
		lastAlertLabelFade = alertLabelFade;
		alertLabelFade = Mathf.Max(0f, alertLabelFade - 1f / Mathf.Lerp(1f, 100f, Mathf.Clamp01(alertLabelFade)));
		alertLabelSin += alertLabelFade;
		_lastBlackFade = _blackFade;
		if (input.mp && !ConfigContainer.holdElement)
		{
			_blackFade = Mathf.Min(1f, _blackFade + 0.0625f);
		}
		else
		{
			_blackFade = Mathf.Max(0f, _blackFade - 0.125f);
		}
		base.Update();
		framesPerSecond = 40;
		if (modApplyer != null)
		{
			modApplyer.Update();
			if (modApplyer.IsFinished())
			{
				if (!modApplyer.WasSuccessful() && modApplyerFallbackStep != ModManager.ModApplyFallbackStep.Vanilla)
				{
					if (modApplyerFallbackStep == ModManager.ModApplyFallbackStep.None)
					{
						modApplyerFallbackStep = ModManager.ModApplyFallbackStep.PreviousEnabled;
						ModManager.ActiveMods = fallbackActiveMods;
						bool[] array = new bool[ModManager.InstalledMods.Count];
						for (int i = 0; i < ModManager.InstalledMods.Count; i++)
						{
							if (ConfigContainer.menuTab.modList.GetModButton(ModManager.InstalledMods[i].id) == null)
							{
								array[i] = false;
								continue;
							}
							for (int j = 0; j < ModManager.ActiveMods.Count; j++)
							{
								if (ModManager.ActiveMods[j].id == ModManager.InstalledMods[i].id)
								{
									array[i] = true;
									break;
								}
							}
						}
						modApplyerError = modApplyer.applyError;
						modApplyer = new ModManager.ModApplyer(manager, array.ToList(), fallbackLoadOrder);
						modApplyer.Start(filesInBadState: false);
					}
					else if (modApplyerFallbackStep == ModManager.ModApplyFallbackStep.PreviousEnabled)
					{
						modApplyerFallbackStep = ModManager.ModApplyFallbackStep.Vanilla;
						ModManager.ActiveMods = new List<ModManager.Mod>();
						bool[] source = new bool[ModManager.InstalledMods.Count];
						int[] source2 = new int[ModManager.InstalledMods.Count];
						modApplyerError = modApplyer.applyError;
						modApplyer = new ModManager.ModApplyer(manager, source.ToList(), source2.Reverse().ToList());
						modApplyer.Start(filesInBadState: false);
					}
				}
				else
				{
					manager.rainWorld.options.Save();
					if (applyingModsDialog != null)
					{
						pages[0].subObjects.Remove(applyingModsDialog);
						applyingModsDialog.RemoveSprites();
						applyingModsDialog = null;
						HasDialogBox = false;
					}
					modApplyerRequiresRestart = modApplyer.RequiresRestart();
					if (modApplyerError != null)
					{
						applyingModsErrorDialog = new DialogBoxNotify(this, pages[0], Translate("mod_menu_error") + " " + modApplyerError, "AFTERERROR", new Vector2(manager.rainWorld.options.ScreenSize.x / 2f - 240f + (1366f - manager.rainWorld.options.ScreenSize.x) / 2f, 224f), new Vector2(480f, 320f), forceWrapping: true);
						pages[0].subObjects.Add(applyingModsErrorDialog);
						HasDialogBox = true;
					}
					else
					{
						AfterModApplyingActions();
					}
					modApplyer = null;
				}
			}
			else
			{
				applyingModsDialog.SetText(Translate("mod_menu_apply_mods") + Environment.NewLine + modApplyer.statusText);
			}
		}
		if (modCalledDialog != null || requiresRestartDialog != null || applyingModsErrorDialog != null)
		{
			HasDialogBox = true;
		}
		if (workshopUploader != null)
		{
			HasDialogBox = true;
			workshopUploader.Update();
			if (workshopUploader.readyToDispose)
			{
				workshopUploader = null;
				HasDialogBox = false;
			}
		}
	}

	private void AfterModApplyingActions()
	{
		if (modApplyerRequiresRestart)
		{
			for (int i = 0; i < pages[0].subObjects.Count; i++)
			{
				if (pages[0].subObjects[i] is ButtonTemplate)
				{
					(pages[0].subObjects[i] as ButtonTemplate).buttonBehav.greyedOut = true;
				}
			}
			requiresRestartDialog = new DialogBoxNotify(this, pages[0], Translate("mod_menu_restart"), "RESTART", new Vector2(manager.rainWorld.options.ScreenSize.x / 2f - 240f + (1366f - manager.rainWorld.options.ScreenSize.x) / 2f, 224f), new Vector2(480f, 320f));
			pages[0].subObjects.Add(requiresRestartDialog);
			HasDialogBox = true;
		}
		else
		{
			PlaySound(SoundID.MENU_Switch_Page_Out);
			ConfigContainer.menuTab.modList.RefreshSavedSelections();
			ConfigContainer.menuTab.modList.RefreshAllButtons();
			_SwitchToMainMenu();
		}
	}

	private void _SwitchToMainMenu()
	{
		cfgContainer._FarewellFreeze();
		manager.RequestMainProcessSwitch(ProcessManager.ProcessID.MainMenu);
	}

	public override void CommunicateWithUpcomingProcess(MainLoopProcess nextProcess)
	{
		base.CommunicateWithUpcomingProcess(nextProcess);
		if (nextProcess is ModdingMenu)
		{
			ModdingMenu moddingMenu = nextProcess as ModdingMenu;
			moddingMenu.isReload = true;
			if (cfgContainer._Mode != 0)
			{
				moddingMenu.lastMode = cfgContainer._Mode;
			}
		}
	}

	public override string UpdateInfoText()
	{
		return base.UpdateInfoText();
	}

	internal void _CloseModCalledDialog()
	{
		if (modCalledDialog != null)
		{
			pages[0].subObjects.Remove(modCalledDialog);
			modCalledDialog.RemoveSprites();
			modCalledDialog = null;
			HasDialogBox = false;
		}
	}

	public override void ShutDownProcess()
	{
		darkSprite.RemoveFromContainer();
		cfgContainer._ShutdownConfigContainer();
		base.ShutDownProcess();
		instance = null;
	}

	public override void Singal(MenuObject sender, string message)
	{
		base.Singal(sender, message);
		if (workshopUploader != null)
		{
			workshopUploader.Singal(sender, message);
		}
		if (message == null)
		{
			return;
		}
		switch (message)
		{
		case "APPLYMODS":
		{
			if (modApplyer != null)
			{
				return;
			}
			PlaySound(SoundID.MENU_Switch_Arena_Gametype);
			manager.rainWorld.progression.SaveProgression(saveMaps: false, saveMiscProg: true);
			fallbackLoadOrder = new List<int>();
			bool[] array = new bool[ModManager.InstalledMods.Count];
			int[] array2 = new int[ModManager.InstalledMods.Count];
			for (int i = 0; i < ModManager.InstalledMods.Count; i++)
			{
				MenuModList.ModButton modButton = ConfigContainer.menuTab.modList.GetModButton(ModManager.InstalledMods[i].id);
				if (modButton == null)
				{
					array[i] = false;
					array2[i] = 0;
				}
				else
				{
					array[i] = modButton.selectEnabled;
					array2[i] = modButton.selectOrder;
					fallbackLoadOrder.Add(ModManager.InstalledMods[i].loadOrder);
				}
			}
			if (applyingModsDialog == null)
			{
				applyingModsDialog = new DialogBoxAsyncWait(this, pages[0], Translate("mod_menu_apply_mods") + Environment.NewLine + " ", new Vector2(manager.rainWorld.options.ScreenSize.x / 2f - 240f + (1366f - manager.rainWorld.options.ScreenSize.x) / 2f, 224f), new Vector2(480f, 320f));
				pages[0].subObjects.Add(applyingModsDialog);
				HasDialogBox = true;
			}
			modApplyerFallbackStep = ModManager.ModApplyFallbackStep.None;
			modApplyerError = null;
			modApplyerRequiresRestart = false;
			fallbackActiveMods = new List<ModManager.Mod>(ModManager.ActiveMods);
			int highest = array2.Max((int t) => t);
			modApplyer = new ModManager.ModApplyer(manager, array.ToList(), array2.Select((int t) => highest - t).ToList());
			modApplyer.Start(filesInBadState: false);
			return;
		}
		case "ENABLEALL":
			ConfigContainer.menuTab.modList._SetEnableToAllMods(enable: true);
			PlaySound(SoundID.MENU_Checkbox_Check);
			return;
		case "DISABLEALL":
			ConfigContainer.menuTab.modList._SetEnableToAllMods(enable: false);
			PlaySound(SoundID.MENU_Checkbox_Uncheck);
			return;
		case "EXIT":
			PlaySound(SoundID.MENU_Switch_Page_Out);
			_SwitchToMainMenu();
			return;
		case "REVERT":
			cfgContainer._SwitchMode(ConfigContainer.Mode.ModView);
			return;
		case "APPLY":
			ConfigContainer.ActiveInterface._SaveConfigFile();
			PlaySound(SoundID.MENU_Continue_From_Sleep_Death_Screen);
			cfgContainer._SwitchMode(ConfigContainer.Mode.ModView);
			return;
		case "RESET":
			ConfigContainer._ResetCurrentConfig();
			PlaySound(SoundID.MENU_Remove_Level);
			return;
		case "RELOAD":
			PlaySound(SoundID.MENU_Switch_Page_In);
			cfgContainer._FarewellFreeze();
			manager.RequestMainProcessSwitch(ProcessManager.ProcessID.ModdingMenu);
			return;
		}
		if (!message.StartsWith("MOD_DIALOG_"))
		{
			switch (message)
			{
			case "RESTART":
				Application.Quit();
				break;
			case "AFTERERROR":
				PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
				pages[0].subObjects.Remove(applyingModsErrorDialog);
				applyingModsErrorDialog.RemoveSprites();
				applyingModsErrorDialog = null;
				AfterModApplyingActions();
				break;
			case "WORKSHOP_UPLOAD":
				PlaySound(SoundID.MENU_Continue_Game);
				if (workshopUploadConfirmDialog != null)
				{
					pages[0].subObjects.Remove(workshopUploadConfirmDialog);
					workshopUploadConfirmDialog.RemoveSprites();
					workshopUploadConfirmDialog = null;
				}
				if (workshopUploader == null)
				{
					workshopUploader = new SteamWorkshopUploader(this, modToUpload);
				}
				break;
			case "WORKSHOP_CANCEL":
				PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
				if (workshopUploadConfirmDialog != null)
				{
					pages[0].subObjects.Remove(workshopUploadConfirmDialog);
					workshopUploadConfirmDialog.RemoveSprites();
					workshopUploadConfirmDialog = null;
				}
				break;
			}
		}
		else
		{
			int num = int.Parse(message.Substring("MOD_DIALOG_".Length));
			Action[] array3 = modCalledDialogActions;
			if (((array3 != null) ? array3[num] : null) != null)
			{
				modCalledDialogActions[num]();
			}
			_CloseModCalledDialog();
		}
	}
}
