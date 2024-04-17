using System;
using RWCustom;

namespace Menu.Remix;

public static class OptionalText
{
	public enum ID
	{
		OpCheckBox_MouseTuto,
		OpCheckBox_NonMouseTuto,
		OpComboBox_MouseOpenTuto,
		OpComboBox_MouseUseTuto,
		OpComboBox_MouseSearchTuto,
		OpComboBox_NonMouseOpenTuto,
		OpComboBox_NonMouseUseTuto,
		OpDragger_MouseTuto,
		OpDragger_NonMouseGrabTuto,
		OpDragger_NonMouseUseTuto,
		OpKeyBinder_ErrorConflictOtherModDefault,
		OpKeyBinder_ErrorConflictVanilla,
		OpKeyBinder_ErrorConflictOtherMod,
		OpKeyBinder_ErrorConflictCurrMod,
		OpKeyBinder_MouseSelectTuto,
		OpKeyBinder_MouseBindTuto,
		OpKeyBinder_MouseJoystickBindTuto,
		OpKeyBinder_NonMouseSelectTuto,
		OpKeyBinder_NonMouseBindTuto,
		OpKeyBinder_NonMouseJoystickBindTuto,
		OpRadioButton_MouseTuto,
		OpRadioButton_NonMouseTuto,
		OpSlider_MouseTutoHrzt,
		OpSlider_MouseTutoVrtc,
		OpSlider_NonMouseGrabTuto,
		OpSlider_NonMouseAdjustTuto,
		OpTextBox_MouseTutoGrab,
		OpTextBox_MouseTutoType,
		OpTextBox_NonMouseTuto,
		OpUpdown_MouseTuto,
		OpUpdown_NonMouseGrab,
		OpUpdown_NonMouseUse,
		OpColorPicker_MouseTypeTuto,
		OpColorPicker_MouseRGBTuto,
		OpColorPicker_MouseHSLTuto,
		OpColorPicker_MousePLTTuto,
		OpColorPicker_NonMouseModeSelect,
		OpColorPicker_NonMouseSliders,
		OpScrollBox_MouseTuto,
		OpScrollBox_MouseTutoSlidebar,
		OpScrollBox_NonMouseTuto,
		OpSimpleButton_MouseTuto,
		OpSimpleButton_NonMouseTuto,
		OpHoldButton_MouseTuto,
		OpHoldButton_NonMouseTuto,
		ModdingMenu_ControlJump,
		ModdingMenu_ControlThrow,
		ModdingMenu_ControlMap,
		ModdingMenu_ControlMove,
		ModdingMenu_ControlPckp,
		ModdingMenu_PckpTuto0,
		ModdingMenu_PckpTuto1,
		ModdingMenu_PckpTuto2,
		ModdingMenu_PckpTuto3,
		ConfigContainer_AlertPasteNoncosmetic,
		ConfigContainer_AlertPasteCosmetic,
		ConfigContainer_AlertCopyNoncosmetic,
		ConfigContainer_AlertCopyCosmetic,
		ConfigMenuTab_ApplyButton_Label,
		ConfigMenuTab_ApplyButton_Desc,
		ConfigMenuTab_BackButton_Label,
		ConfigMenuTab_BackButton_Desc,
		ConfigMenuTab_BackHoldButton_Desc,
		ConfigMenuTab_RevertButton_Label,
		ConfigMenuTab_RevertButton_Desc,
		ConfigMenuTab_RevertHoldButton_Label,
		ConfigMenuTab_RevertHoldButton_Desc,
		ConfigMenuTab_SaveButton_Label,
		ConfigMenuTab_SaveButton_Desc,
		ConfigMenuTab_ResetButton_Label,
		ConfigMenuTab_ResetButton_Desc,
		ConfigMenuTab_ModPercentageButton_Label,
		ConfigMenuTab_ModPercentageButton_MouseEnableAll,
		ConfigMenuTab_ModPercentageButton_NonMouseEnableAll,
		ConfigMenuTab_ModPercentageButton_MouseDisableAll,
		ConfigMenuTab_ModPercentageButton_NonMouseDisableAll,
		MenuModStat_ModID,
		MenuModStat_ModTargetVersion,
		MenuModStat_ModsDownpour,
		MenuModStat_ModsRegion,
		MenuModStat_ModsInterface,
		MenuModStat_ModsFail,
		MenuModStat_MapTuto,
		MenuModStat_ConsoleExplain0,
		MenuModList_AskModApply,
		MenuModList_SwapFailed,
		MenuModList_DummyDesc,
		MenuModList_ModButton_NameAndAuthor,
		MenuModList_ModButton_Display,
		MenuModList_ModButton_Configure,
		MenuModList_ModButton_Enable_Mouse,
		MenuModList_ModButton_Disable_Mouse,
		MenuModList_ModButton_Enable_NonMouse,
		MenuModList_ModButton_Disable_NonMouse,
		MenuModList_ModButton_Enable_NonMouse_Console,
		MenuModList_ModButton_Disable_NonMouse_Console,
		MenuModList_ModButton_NonMouse_Configure,
		MenuModList_ModButton_NonMouse_Display,
		MenuModList_ModButton_NonMouse_Configure_Console,
		MenuModList_ModButton_NonMouse_Display_Console,
		MenuModList_ModButton_NonMouse_Blank_Console,
		MenuModList_ModButton_RainWorldDummy,
		MenuModList_ListButton_Stat,
		MenuModList_ListButton_ScrollUp,
		MenuModList_ListButton_ScrollDw,
		MenuModList_ListButton_SwapUp,
		MenuModList_ListButton_SwapDw,
		MenuModList_ListButton_Expand,
		MenuModList_ListButton_Collapse,
		MenuModList_ListSlider_Desc,
		MenuModList_SearchBox_Desc,
		MenuModList_SearchBox_Empty,
		MenuModList_SearchBox_Query,
		ConfigTabController_TabSelectButton_UnnamedTab,
		ConfigTabController_TabSelectButton_NamedTab,
		ConfigTabController_TabScrollButton_Up,
		ConfigTabController_TabScrollButton_Dw,
		Connector_ResetConfig_Alert,
		Connector_SaveConfig_Alert,
		Connector_UnnamedTab,
		Connector_ChangeActiveTab_Alert,
		Connector_ChangeActiveMod_Alert,
		OIError_Message_Initialize,
		OIError_Message_Update,
		OIError_GeneralAdvice
	}

	public static string[] engText;

	internal static void _Initialize()
	{
		if (engText == null)
		{
			engText = new string[Enum.GetNames(typeof(ID)).Length];
			engText[0] = "Click to toggle";
			engText[1] = "Press JUMP to toggle";
			engText[2] = "Click to open the list";
			engText[3] = "Double click the main box to search";
			engText[4] = "Type keyword or initial with keyboard to search";
			engText[5] = "Press JUMP to open the list";
			engText[6] = "Use joystick to scroll, Press JUMP to select";
			engText[7] = "Hold mouse button and drag up or down to adjust value";
			engText[8] = "Press JUMP to interact";
			engText[9] = "Use joystick to adjust, press JUMP to set";
			engText[10] = "Conflicting default button with <ModID>";
			engText[11] = "Conflicting button with vanilla control options";
			engText[12] = "Conflicting button with <AnotherModID>";
			engText[13] = "[<ConflictButton>] button is already in use";
			engText[14] = "Click to change button binding";
			engText[15] = "Press a button to bind (ESC to unbind)";
			engText[16] = "Press a button to bind (ESC to unbind, CTRL + no to set controller number)";
			engText[17] = "Press JUMP to change Button binding";
			engText[18] = "Press a button to bind (PAUSE to unbind)";
			engText[19] = "Press a button to bind (PAUSE to unbind, START to set controller number)";
			engText[20] = "Click to choose this option";
			engText[21] = "Press JUMP to choose this option";
			engText[22] = "Hold your mouse button and drag left or right to adjust value";
			engText[23] = "Hold your mouse button and drag up or down to adjust value";
			engText[24] = "Press JUMP to interact";
			engText[25] = "Use joystick to adjust, press JUMP to apply";
			engText[26] = "Click to begin typing";
			engText[27] = "Use keyboard to type, press Enter to apply";
			engText[28] = "This cannot be interacted with a joystick";
			engText[29] = "Click and type numbers, use arrows or scrollwheel to adjust";
			engText[30] = "Press JUMP to interact";
			engText[31] = "Use joystick to adjust, press JUMP to apply";
			engText[32] = "Type hex code with keyboard for desired color";
			engText[33] = "Select color with red, green, blue slider";
			engText[34] = "Select color with hue saturation square and lightness slider";
			engText[35] = "Select a color from palette";
			engText[36] = "Press JUMP to select mode";
			engText[37] = "With joystick, up or down to choose, left or right to adjust. Press JUMP to apply";
			engText[38] = "Use scroll wheel to see more";
			engText[39] = "Use scroll wheel or scrollbar to see more";
			engText[40] = "Press JUMP and use joystick to see more";
			engText[41] = "Click mouse button to trigger";
			engText[42] = "Press JUMP button to trigger";
			engText[43] = "Hold mouse button to trigger";
			engText[44] = "Hold JUMP button to trigger";
			engText[45] = "Select / Confirm";
			engText[46] = "Back / Cancel";
			engText[47] = "View controls";
			engText[48] = "Move / Adjust";
			engText[49] = "Clipboard";
			engText[50] = "Pick up clipboard interactions:";
			engText[51] = "Select a configurable element with Jump Button first";
			engText[52] = "Hold Pick-Up Button and press Jump Button to copy to clipboard";
			engText[53] = "Hold Pick-Up Button and press Throw Button to paste from clipboard";
			engText[54] = "Pasted <Text> from clipboard to <ObjectName>";
			engText[55] = "Pasted <Text> from clipboard";
			engText[56] = "Copied <Text> to clipboard from <ObjectName>";
			engText[57] = "Copied <Text> to clipboard";
			engText[58] = "APPLY MODS";
			engText[59] = "Apply currently selected mods";
			engText[60] = "EXIT";
			engText[61] = "Return to main menu";
			engText[62] = "Return to main menu (without applying mods!)";
			engText[63] = "BACK";
			engText[64] = "Return to mod list";
			engText[65] = "REVERT";
			engText[66] = "Discard current changes to mod's configurations and return";
			engText[67] = "APPLY";
			engText[68] = "Save current mod's configurations and return";
			engText[69] = "RESET CONFIG";
			engText[70] = "Reset current mod's configurations";
			engText[71] = "Enabled Mods: <CountModEnabled> / <CountModTotal>";
			engText[72] = "Click to enable all mods";
			engText[73] = "Press JUMP to enable all mods";
			engText[74] = "Click to disable all mods";
			engText[75] = "Press JUMP to disable all mods";
			engText[76] = "ID: <ModID>";
			engText[77] = "Target game version: <TargetVersion> (Current game version: <GameVersion>)";
			engText[78] = "Pre-packaged mods:";
			engText[79] = "Enabled region mods: <Number>";
			engText[80] = "Enabled mods with interface: <Number1> (Configurable: <Number2>)";
			engText[81] = "Installed mods without its requirements:";
			engText[82] = "Hold <Button> to check controls";
			engText[83] = "In this menu, you can choose which Remix to activate and configure them.";
			engText[84] = "Press APPLY MODS first to configure mods";
			engText[85] = "Cannot load a required mod before its dependent mod";
			engText[86] = "The base version of Rain World";
			engText[87] = "<ModName> by <ModAuthor>";
			engText[88] = "Display <Mod>";
			engText[89] = "Configure <Mod>";
			engText[90] = "Enable <Mod>";
			engText[91] = "Disable <Mod>";
			engText[92] = "Press JUMP to enable, hold JUMP and use joystick to switch order";
			engText[93] = "Press JUMP to disable, hold JUMP and use joystick to switch order";
			engText[94] = "Press JUMP to enable, press RIGHT BUTTON to return";
			engText[95] = "Press JUMP to disable, press RIGHT BUTTON to return";
			engText[96] = "Press JUMP to configure <Mod>";
			engText[97] = "Press JUMP to display <Mod>";
			engText[98] = "Press JUMP to configure, press LEFT BUTTON to toggle";
			engText[99] = "Press JUMP to display, press LEFT BUTTON to toggle";
			engText[100] = "Press LEFT BUTTON to toggle <Mod>";
			engText[101] = "Represents Rain World base game, initially loaded before all the mods above";
			engText[102] = "Display mods statistics";
			engText[103] = "Press or hold mouse button to scroll the mod list up";
			engText[104] = "Press or hold mouse button to scroll the mod list down";
			engText[105] = "Press mouse button to delay the mod's load order than the mod on top";
			engText[106] = "Press mouse button to hasten the mod's load order than the mod at below";
			engText[107] = "Expand the mod list to show thumbnails";
			engText[108] = "Collapse the mod list to show more mods at once";
			engText[109] = "Hold your mouse button on the circle and drag up or down to scroll the mod list";
			engText[110] = "Click and type to search mods";
			engText[111] = "Type to search...";
			engText[112] = "Search query: <Query>";
			engText[113] = "Switch to tab no. <TabIndex>";
			engText[114] = "Switch to tab <TabName>";
			engText[115] = "Press or hold mouse button to scroll the tab list up";
			engText[116] = "Press or hold mouse button to scroll the tab list down";
			engText[117] = "Restored the configuration of <ModName> to default";
			engText[118] = "Saved the current configuration of <ModName>";
			engText[120] = "Switched the currently active tab to <TabName>";
			engText[119] = "no. <TabIndex>";
			engText[121] = "Switched the currently viewing mod to <ModName>";
			engText[122] = "There was an issue initializing OptionInterface.";
			engText[123] = "There was an issue running OptionInterface.";
			engText[124] = "This issue may be resolved by downloading the latest version of the mod.";
		}
	}

	public static string GetText(ID id)
	{
		string text = Custom.rainWorld.inGameTranslator.Translate(id.ToString());
		if (!string.IsNullOrEmpty(text) && !(text == id.ToString()) && !(text == "!NO TRANSLATION!"))
		{
			return text;
		}
		return OptionInterface.Translate(engText[(int)id]);
	}

	public static string GetButtonName_Map(bool holdText = false)
	{
		Options.ControlSetup.Preset activePreset = Custom.rainWorld.options.controls[0].GetActivePreset();
		string text = ((!(activePreset == Options.ControlSetup.Preset.PS4DualShock) && !(activePreset == Options.ControlSetup.Preset.PS5DualSense)) ? ((activePreset == Options.ControlSetup.Preset.XBox) ? (holdText ? "Hold RB" : "RB") : ((!(activePreset == Options.ControlSetup.Preset.SwitchHandheld) && !(activePreset == Options.ControlSetup.Preset.SwitchDualJoycon) && !(activePreset == Options.ControlSetup.Preset.SwitchProController)) ? ((activePreset == Options.ControlSetup.Preset.SwitchSingleJoyconL || activePreset == Options.ControlSetup.Preset.SwitchSingleJoyconR) ? (holdText ? "switch_controls_singlejoycon_hold_map" : "switch_controls_singlejoycon_map") : ((!(activePreset == Options.ControlSetup.Preset.KeyboardSinglePlayer)) ? "Map Button" : (holdText ? "Hold Space" : "Space"))) : (holdText ? "switch_controls_hold_map" : "switch_controls_map"))) : (holdText ? "Hold R1" : "R1"));
		return OptionInterface.Translate(text);
	}

	public static string GetButtonName_PickUp()
	{
		Options.ControlSetup.Preset activePreset = Custom.rainWorld.options.controls[0].GetActivePreset();
		string text;
		if (activePreset == Options.ControlSetup.Preset.PS4DualShock || activePreset == Options.ControlSetup.Preset.PS5DualSense)
		{
			text = "Square";
		}
		else
		{
			if (activePreset == Options.ControlSetup.Preset.XBox)
			{
				return "X";
			}
			text = ((activePreset == Options.ControlSetup.Preset.SwitchHandheld || activePreset == Options.ControlSetup.Preset.SwitchDualJoycon || activePreset == Options.ControlSetup.Preset.SwitchProController) ? "switch_controls_pickup" : ((activePreset == Options.ControlSetup.Preset.SwitchSingleJoyconL) ? "switch_controls_singlejoyconl_pickup" : ((activePreset == Options.ControlSetup.Preset.SwitchSingleJoyconR) ? "switch_controls_singlejoyconr_pickup" : ((!(activePreset == Options.ControlSetup.Preset.KeyboardSinglePlayer)) ? "Pick-Up Button" : "Shift"))));
		}
		return OptionInterface.Translate(text);
	}

	public static string GetButtonName_Jump()
	{
		Options.ControlSetup.Preset activePreset = Custom.rainWorld.options.controls[0].GetActivePreset();
		string text;
		if (activePreset == Options.ControlSetup.Preset.PS4DualShock || activePreset == Options.ControlSetup.Preset.PS5DualSense)
		{
			text = "Cross";
		}
		else
		{
			if (activePreset == Options.ControlSetup.Preset.XBox)
			{
				return "A";
			}
			if (activePreset == Options.ControlSetup.Preset.SwitchHandheld || activePreset == Options.ControlSetup.Preset.SwitchDualJoycon || activePreset == Options.ControlSetup.Preset.SwitchProController)
			{
				text = "switch_controls_jump";
			}
			else if (activePreset == Options.ControlSetup.Preset.SwitchSingleJoyconL)
			{
				text = "switch_controls_singlejoyconl_jump";
			}
			else if (activePreset == Options.ControlSetup.Preset.SwitchSingleJoyconR)
			{
				text = "switch_controls_singlejoyconr_jump";
			}
			else
			{
				if (activePreset == Options.ControlSetup.Preset.KeyboardSinglePlayer)
				{
					return "Z";
				}
				text = "Jump Button";
			}
		}
		return OptionInterface.Translate(text);
	}

	public static string GetButtonName_Throw()
	{
		Options.ControlSetup.Preset activePreset = Custom.rainWorld.options.controls[0].GetActivePreset();
		string text;
		if (activePreset == Options.ControlSetup.Preset.PS4DualShock || activePreset == Options.ControlSetup.Preset.PS5DualSense)
		{
			text = "Square";
		}
		else
		{
			if (activePreset == Options.ControlSetup.Preset.XBox)
			{
				return "B";
			}
			if (activePreset == Options.ControlSetup.Preset.SwitchHandheld || activePreset == Options.ControlSetup.Preset.SwitchDualJoycon || activePreset == Options.ControlSetup.Preset.SwitchProController)
			{
				text = "switch_controls_throw";
			}
			else if (activePreset == Options.ControlSetup.Preset.SwitchSingleJoyconL)
			{
				text = "switch_controls_singlejoyconl_throw";
			}
			else if (activePreset == Options.ControlSetup.Preset.SwitchSingleJoyconR)
			{
				text = "switch_controls_singlejoyconr_throw";
			}
			else
			{
				if (activePreset == Options.ControlSetup.Preset.KeyboardSinglePlayer)
				{
					return "X";
				}
				text = "Throw Button";
			}
		}
		return OptionInterface.Translate(text);
	}
}
