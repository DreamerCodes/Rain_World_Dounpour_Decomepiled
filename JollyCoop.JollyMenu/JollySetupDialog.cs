using System;
using System.Collections.Generic;
using System.Globalization;
using JollyCoop.JollyManual;
using Kittehface.Framework20;
using Menu;
using Menu.Remix;
using RWCustom;
using UnityEngine;

namespace JollyCoop.JollyMenu;

public class JollySetupDialog : Dialog
{
	public SimpleButton cancelButton;

	public OptionInterface oi;

	public DialogNotify instructionsDialog;

	public ColorChangeDialog colorDialog;

	private bool lastPauseButton;

	private float targetAlpha;

	private float currentAlpha;

	private float lastAlpha;

	public float uAlpha;

	private bool closing;

	private bool opening;

	private bool requestingControllerConnections;

	public JollySlidingMenu slidingMenu;

	public SlugcatStats.Name currentSlugcatPageName;

	public MenuTabWrapper tabWrapper;

	public Dictionary<string, string> elementDescription;

	public Dictionary<JollyEnums.JollyManualPages, int> manualTopics;

	private bool requestingInputMenu;

	public Options Options => manager.rainWorld.options;

	public JollyPlayerOptions JollyOptions(int index)
	{
		return Options.jollyPlayerOptionsArray[index];
	}

	public JollySetupDialog(SlugcatStats.Name name, ProcessManager manager, Vector2 closeButtonPos)
		: base(manager)
	{
		base.manager = manager;
		currentSlugcatPageName = name;
		targetAlpha = 1f;
		closing = false;
		opening = true;
		requestingInputMenu = false;
		elementDescription = new Dictionary<string, string>();
		oi = MachineConnector.GetRegisteredOI(JollyCoop.MOD_ID);
		AddCancelButton(closeButtonPos);
		slidingMenu = new JollySlidingMenu(pos: new Vector2(0f, Options.ScreenSize.y + 100f), menu: this, owner: pages[0]);
		pages[0].subObjects.Add(slidingMenu);
		manualTopics = new Dictionary<JollyEnums.JollyManualPages, int>
		{
			{
				JollyEnums.JollyManualPages.Introduction,
				1
			},
			{
				JollyEnums.JollyManualPages.Difficulties,
				1
			},
			{
				JollyEnums.JollyManualPages.Surviving_a_cycle,
				1
			},
			{
				JollyEnums.JollyManualPages.Camera,
				2
			},
			{
				JollyEnums.JollyManualPages.Piggybacking,
				1
			},
			{
				JollyEnums.JollyManualPages.Pointing,
				1
			},
			{
				JollyEnums.JollyManualPages.Selecting_a_slugcat,
				1
			}
		};
		JollyCustom.Log("Opening jolly dialog!!!");
	}

	private void AddCancelButton(Vector2 pos)
	{
		cancelButton = new SimpleButton(this, pages[0], Translate("CLOSE"), "CANCEL", pos, new Vector2(110f, 30f));
		pages[0].subObjects.Add(cancelButton);
	}

	private void PlayDialogCloseSound()
	{
		PlaySound(SoundID.MENU_Remove_Level);
	}

	public override void Singal(MenuObject sender, string message)
	{
		if (requestingControllerConnections)
		{
			return;
		}
		base.Singal(sender, message);
		if (message.StartsWith("JOLLYCOLORDIALOG"))
		{
			int num = int.Parse(message.Split('G')[1], NumberFormatInfo.InvariantInfo);
			if (num < slidingMenu.playerSelector.Length)
			{
				JollyCustom.Log("Changing color for player " + num);
				SlugcatStats.Name name = Options.jollyPlayerOptionsArray[num].playerClass ?? currentSlugcatPageName;
				List<string> names = PlayerGraphics.ColoredBodyPartList(name);
				colorDialog = new ColorChangeDialog(this, name, num, manager, names);
				manager.ShowDialog(colorDialog);
			}
			return;
		}
		switch (message)
		{
		case "CANCEL":
			PlaySound(SoundID.MENU_Switch_Page_Out);
			RequestClose();
			break;
		case "INFO_COLOR":
		{
			string text3 = Translate(message);
			Vector2 vector3 = DialogBoxNotify.CalculateDialogBoxSize(text3);
			instructionsDialog = new DialogNotify(Custom.ReplaceLineDelimeters(text3), Translate("COLOR INFORMATION"), new Vector2(vector3.x, vector3.y + 80f), manager, PlayDialogCloseSound, longLabel: true);
			manager.ShowDialog(instructionsDialog);
			break;
		}
		case "INFO_DIFF":
		{
			string text2 = Translate(message);
			Vector2 vector2 = DialogBoxNotify.CalculateDialogBoxSize(text2);
			instructionsDialog = new DialogNotify(Custom.ReplaceLineDelimeters(text2), Translate("DIFFICULTY INFORMATION"), new Vector2(vector2.x, vector2.y + 80f), manager, PlayDialogCloseSound, longLabel: true);
			manager.ShowDialog(instructionsDialog);
			break;
		}
		case "INFO_CAMERA":
		{
			string text = Translate(message);
			Vector2 vector = DialogBoxNotify.CalculateDialogBoxSize(text);
			instructionsDialog = new DialogNotify(Custom.ReplaceLineDelimeters(text), Translate("CAMERA INFORMATION"), new Vector2(vector.x, vector.y + 80f), manager, PlayDialogCloseSound, longLabel: true);
			manager.ShowDialog(instructionsDialog);
			break;
		}
		case "INPUT":
			RequestInputMenu();
			break;
		case "JOLLY_MANUAL":
		{
			JollyManualDialog dialog = new JollyManualDialog(manager, manualTopics);
			PlaySound(SoundID.MENU_Player_Join_Game);
			manager.ShowDialog(dialog);
			break;
		}
		}
	}

	private void RequestInputMenu()
	{
		requestingInputMenu = true;
		RequestClose();
	}

	public override void Update()
	{
		base.Update();
		lastAlpha = currentAlpha;
		currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, 0.2f);
		bool flag = RWInput.CheckPauseButton(0);
		if (flag && !lastPauseButton)
		{
			PlaySound(SoundID.MENU_Switch_Page_Out);
			RequestClose();
		}
		lastPauseButton = flag;
		if (closing && Math.Abs(currentAlpha - targetAlpha) < 0.09f)
		{
			Options.Save();
			manager.StopSideProcess(this);
			closing = false;
			if (requestingInputMenu)
			{
				manager.RequestMainProcessSwitch(ProcessManager.ProcessID.InputOptions);
				PlaySound(SoundID.MENU_Switch_Page_In);
				manager.rainWorld.options.Save();
			}
		}
	}

	private void UserInput_OnControllerConfigurationChanged()
	{
		UserInput.OnControllerConfigurationChanged -= UserInput_OnControllerConfigurationChanged;
		requestingControllerConnections = false;
	}

	public void RequestClose()
	{
		if (!closing)
		{
			closing = true;
			targetAlpha = 0f;
			if (manager.rainWorld.options.playersBeforeEnterJollyMenu != manager.rainWorld.options.JollyPlayerCount)
			{
				manager.rainWorld.options.jollyControllersNeedUpdating = true;
			}
		}
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		if (opening || closing)
		{
			uAlpha = Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(lastAlpha, currentAlpha, timeStacker)), 1.5f);
			darkSprite.alpha = uAlpha * 0.92f;
		}
		slidingMenu.pos.y = Mathf.Lerp(Options.ScreenSize.y + 100f, 0f, (uAlpha < 0.999f) ? uAlpha : 1f);
	}

	public override void ShutDownProcess()
	{
		base.ShutDownProcess();
		darkSprite.RemoveFromContainer();
		slidingMenu.RemoveSprites();
	}

	public int GetFileIndex(SlugcatStats.Name name)
	{
		int result = 4;
		if (name == SlugcatStats.Name.White)
		{
			result = 0;
		}
		if (name == SlugcatStats.Name.Yellow)
		{
			result = 1;
		}
		if (name == SlugcatStats.Name.Red)
		{
			result = 2;
		}
		return result;
	}

	public override string UpdateInfoText()
	{
		string text = null;
		if (selectedObject is SimpleButton simpleButton)
		{
			text = simpleButton.signalText;
		}
		if (selectedObject is SymbolButton symbolButton)
		{
			text = symbolButton.signalText;
		}
		if (text != null && elementDescription.TryGetValue(text, out var value))
		{
			return value;
		}
		return base.UpdateInfoText();
	}
}
