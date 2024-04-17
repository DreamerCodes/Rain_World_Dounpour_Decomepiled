using System;
using System.Linq;
using Menu.Remix.MixedUI;
using RWCustom;
using UnityEngine;

namespace Menu.Remix;

public static class ConfigConnector
{
	public static bool InModdingMenu => ModdingMenu.instance != null;

	public static void ShowDescription(string text)
	{
		if (InModdingMenu)
		{
			ModdingMenu.instance.ShowDescription(text);
		}
	}

	public static void ShowAlert(string text)
	{
		if (InModdingMenu)
		{
			ModdingMenu.instance.ShowAlert(text);
		}
	}

	public static void RequestReloadMenu()
	{
		if (InModdingMenu)
		{
			ModdingMenu.instance.Singal(null, "RELOAD");
		}
	}

	public static void ResetConfig()
	{
		if (InModdingMenu)
		{
			ModdingMenu.instance.Singal(null, "RESET");
			ShowAlert(OptionalText.GetText(OptionalText.ID.Connector_ResetConfig_Alert).Replace("<ModName>", ConfigContainer.ActiveInterface.mod.LocalizedName));
		}
	}

	public static void SaveConfig(bool silent = false)
	{
		if (InModdingMenu)
		{
			ConfigContainer.ActiveInterface._SaveConfigFile();
			if (!silent)
			{
				ShowAlert(OptionalText.GetText(OptionalText.ID.Connector_SaveConfig_Alert).Replace("<ModName>", ConfigContainer.ActiveInterface.mod.LocalizedName));
				ModdingMenu.instance.PlaySound(SoundID.MENU_Continue_From_Sleep_Death_Screen);
			}
		}
	}

	public static bool TryChangeActiveTab(int index, bool silent = false)
	{
		if (!InModdingMenu || index < 0)
		{
			return false;
		}
		if (index >= ConfigContainer.ActiveInterface.Tabs.Length)
		{
			return false;
		}
		ConfigContainer._ChangeActiveTab(index);
		if (!silent)
		{
			string text = ConfigContainer.ActiveInterface.Tabs[index].name;
			if (string.IsNullOrEmpty(text))
			{
				text = OptionalText.GetText(OptionalText.ID.Connector_UnnamedTab).Replace("<TabIndex>", index.ToString());
			}
			ShowAlert(OptionalText.GetText(OptionalText.ID.Connector_ChangeActiveTab_Alert).Replace("<TabName>", text));
			ConfigContainer.menuTab.tabCtrler.ScrollToShow(index);
		}
		return true;
	}

	public static bool TryChangeActiveMod(string modIDquery)
	{
		if (!InModdingMenu)
		{
			return false;
		}
		string text = "";
		int num = int.MaxValue;
		string[] optItfID = ConfigContainer.OptItfID;
		foreach (string text2 in optItfID)
		{
			if (text2.ToLower() == modIDquery.ToLower())
			{
				text = text2;
				break;
			}
			if (text2.ToLower().Contains(modIDquery.ToLower()) && modIDquery.Length - text2.Length < num)
			{
				text = text2;
				num = modIDquery.Length - text2.Length;
			}
		}
		if (string.IsNullOrEmpty(text))
		{
			return false;
		}
		MenuModList.ModButton modButton = ConfigContainer.menuTab.modList.GetModButton(text);
		if (modButton == null)
		{
			return false;
		}
		if (!modButton.selectEnabled)
		{
			return false;
		}
		if (modButton.type == MenuModList.ModButton.ItfType.Blank && !modButton.itf.HasConfigurables())
		{
			return false;
		}
		int num2 = ConfigContainer.FindItfIndex(text);
		ConfigContainer._ChangeActiveMod(num2);
		ShowAlert(OptionalText.GetText(OptionalText.ID.Connector_ChangeActiveMod_Alert).Replace("<ModName>", ConfigContainer.ActiveInterface.mod.LocalizedName));
		ConfigContainer.menuTab.modList.ScrollToShow(num2);
		return true;
	}

	public static void MuteMenu(bool mute)
	{
		ConfigContainer.mute = mute;
	}

	public static void FocusNewElement(UIfocusable focusable)
	{
		if (InModdingMenu)
		{
			ConfigContainer.instance._FocusNewElement(focusable);
		}
	}

	public static void FocusNewElementInDirection(IntVector2 direction)
	{
		if (InModdingMenu)
		{
			ConfigContainer.instance._FocusNewElementInDirection(direction);
		}
	}

	public static void ResetMenuNextFocusable()
	{
		if (InModdingMenu)
		{
			ConfigContainer.menuTab._ClearCustomNextFocusable();
		}
	}

	public static void MutualVerticalRevertButtonBind(UIfocusable up)
	{
		if (InModdingMenu)
		{
			ConfigContainer.menuTab._SetRevertButtonUpPointer(up);
			up?.SetNextFocusable(UIfocusable.NextDirection.Down, FocusMenuPointer.GetPointer(FocusMenuPointer.MenuUI.RevertButton));
		}
	}

	public static void MutualVerticalSaveButtonBind(UIconfig up)
	{
		if (InModdingMenu)
		{
			ConfigContainer.menuTab.SaveButton.SetNextFocusable(UIfocusable.NextDirection.Up, up);
			up?.SetNextFocusable(UIfocusable.NextDirection.Down, FocusMenuPointer.GetPointer(FocusMenuPointer.MenuUI.SaveButton));
		}
	}

	public static void MutualVerticalResetConfigButtonBind(UIconfig up)
	{
		if (InModdingMenu)
		{
			ConfigContainer.menuTab.ResetConfigButton.SetNextFocusable(UIfocusable.NextDirection.Up, up);
			up?.SetNextFocusable(UIfocusable.NextDirection.Down, FocusMenuPointer.GetPointer(FocusMenuPointer.MenuUI.ResetConfigButton));
		}
	}

	public static void SetMenuButtonsUpPointer(UIconfig up)
	{
		if (InModdingMenu)
		{
			ConfigContainer.menuTab._SetRevertButtonUpPointer(up);
			ConfigContainer.menuTab.SaveButton.SetNextFocusable(UIfocusable.NextDirection.Up, up);
			ConfigContainer.menuTab.ResetConfigButton.SetNextFocusable(UIfocusable.NextDirection.Up, up);
		}
	}

	public static void MutualHorizontalCurrentTabButtonBind(UIconfig right)
	{
		if (InModdingMenu)
		{
			ConfigContainer.menuTab.tabCtrler.GetCurrentTabButton().SetNextFocusable(UIfocusable.NextDirection.Right, right);
			right?.SetNextFocusable(UIfocusable.NextDirection.Left, FocusMenuPointer.GetPointer(FocusMenuPointer.MenuUI.CurrentTabButton));
		}
	}

	public static bool HasDialogBox(bool any = false)
	{
		if (!InModdingMenu)
		{
			return false;
		}
		if (!any)
		{
			return ModdingMenu.instance.modCalledDialog != null;
		}
		return ModdingMenu.instance.HasDialogBox;
	}

	public static void CreateDialogBoxNotify(string text, Action action = null)
	{
		if (InModdingMenu)
		{
			ModdingMenu.instance._CloseModCalledDialog();
			Vector2 size = DialogBoxNotify.CalculateDialogBoxSize(text);
			DialogBoxNotify dialogBoxNotify = new DialogBoxNotify(ModdingMenu.instance, ModdingMenu.instance.pages[0], text, "MOD_DIALOG_0", new Vector2(Custom.rainWorld.options.ScreenSize.x / 2f - size.x / 2f + (1366f - Custom.rainWorld.options.ScreenSize.x) / 2f, 384f - size.y / 2f), size, forceWrapping: true);
			ModdingMenu.instance.modCalledDialog = dialogBoxNotify;
			ModdingMenu.instance.modCalledDialogActions = new Action[1] { action };
			ModdingMenu.instance.pages[0].subObjects.Add(dialogBoxNotify);
		}
	}

	public static void CreateDialogBoxMultibutton(string text, string[] buttonTexts, Action[] actions)
	{
		if (InModdingMenu)
		{
			ModdingMenu.instance._CloseModCalledDialog();
			Vector2 size = DialogBoxMultiButtonNotify.CalculateDialogBoxSize(text, buttonTexts.Length);
			string[] array = Enumerable.Repeat("MOD_DIALOG_", buttonTexts.Length).ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i] += i.ToString("0");
			}
			DialogBoxMultiButtonNotify dialogBoxMultiButtonNotify = new DialogBoxMultiButtonNotify(ModdingMenu.instance, ModdingMenu.instance.pages[0], text, array, buttonTexts, new Vector2(Custom.rainWorld.options.ScreenSize.x / 2f - size.x / 2f + (1366f - Custom.rainWorld.options.ScreenSize.x) / 2f, 384f - size.y / 2f), size, forceWrapping: true);
			ModdingMenu.instance.modCalledDialog = dialogBoxMultiButtonNotify;
			ModdingMenu.instance.modCalledDialogActions = actions;
			ModdingMenu.instance.pages[0].subObjects.Add(dialogBoxMultiButtonNotify);
		}
	}

	public static void CreateDialogBoxYesNo(string text, Action YesAction, Action NoAction = null)
	{
		if (InModdingMenu)
		{
			CreateDialogBoxMultibutton(text, new string[2]
			{
				ModdingMenu.instance.Translate("YES"),
				ModdingMenu.instance.Translate("NO")
			}, new Action[2] { YesAction, NoAction });
		}
	}

	public static void CreateDialogBoxAsyncWait(string text)
	{
		if (InModdingMenu)
		{
			ModdingMenu.instance._CloseModCalledDialog();
			DialogBoxAsyncWait dialogBoxAsyncWait = new DialogBoxAsyncWait(ModdingMenu.instance, ModdingMenu.instance.pages[0], text, new Vector2(Custom.rainWorld.options.ScreenSize.x / 2f - 300f + (1366f - Custom.rainWorld.options.ScreenSize.x) / 2f, 224f), new Vector2(600f, 320f), forceWrapping: true);
			ModdingMenu.instance.modCalledDialog = dialogBoxAsyncWait;
			ModdingMenu.instance.modCalledDialogActions = new Action[0];
			ModdingMenu.instance.pages[0].subObjects.Add(dialogBoxAsyncWait);
		}
	}

	public static void UpdateDialogBoxAsyncWait(string text)
	{
		if (InModdingMenu && ModdingMenu.instance.modCalledDialog is DialogBoxAsyncWait)
		{
			(ModdingMenu.instance.modCalledDialog as DialogBoxAsyncWait).SetText(text);
		}
	}

	public static void CloseDialogBox()
	{
		if (InModdingMenu)
		{
			ModdingMenu.instance._CloseModCalledDialog();
		}
	}
}
