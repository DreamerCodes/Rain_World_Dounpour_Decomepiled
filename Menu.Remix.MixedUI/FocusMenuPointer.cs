using System.Collections.Generic;
using RWCustom;

namespace Menu.Remix.MixedUI;

public class FocusMenuPointer : FocusPointer
{
	public enum MenuUI
	{
		RevertButton,
		SaveButton,
		ResetConfigButton,
		CurrentTabButton,
		_ApplyButton,
		_BackButton,
		_ModButtonPointer,
		_ModStatsRightPointer,
		_BackButtonUpPointer,
		_ModStatsButton,
		_ModListExpandButton,
		_LastActiveModButtonOrTop,
		_LastActiveModButtonOrBottom
	}

	public readonly MenuUI menuUI;

	private static Dictionary<MenuUI, FocusMenuPointer> pointers;

	private static ConfigMenuTab MenuTab => ConfigContainer.menuTab;

	private static MenuModList ModList => ConfigContainer.menuTab.modList;

	private static UIfocusable FocusedElement => ConfigContainer.FocusedElement;

	private static OptionInterface ActiveInterface => ConfigContainer.ActiveInterface;

	private FocusMenuPointer(MenuUI menuUI)
	{
		this.menuUI = menuUI;
	}

	public static FocusMenuPointer GetPointer(MenuUI menuUI)
	{
		if (pointers == null)
		{
			pointers = new Dictionary<MenuUI, FocusMenuPointer>();
			string[] enumNames = typeof(MenuUI).GetEnumNames();
			for (int i = 0; i < enumNames.Length; i++)
			{
				MenuUI key = Custom.ParseEnum<MenuUI>(enumNames[i]);
				pointers.Add(key, new FocusMenuPointer(key));
			}
		}
		return pointers[menuUI];
	}

	internal static void _ClearPointers()
	{
		pointers = null;
	}

	public override UIfocusable GetPointed(NextDirection dir)
	{
		switch (menuUI)
		{
		case MenuUI.RevertButton:
			return MenuTab.RevertButton;
		case MenuUI.SaveButton:
			return MenuTab.SaveButton;
		case MenuUI.ResetConfigButton:
			return MenuTab.ResetConfigButton;
		case MenuUI.CurrentTabButton:
			return MenuTab.tabCtrler.GetCurrentTabButton();
		case MenuUI._ApplyButton:
			return MenuTab.ApplyButton;
		case MenuUI._BackButton:
			return MenuTab.BackButton;
		case MenuUI._ModButtonPointer:
			return dir switch
			{
				NextDirection.Back => MenuTab.BackButton, 
				NextDirection.Left => FocusedElement, 
				NextDirection.Right => ModList._roleButtons[0], 
				_ => ModList.visibleModButtons[Custom.IntClamp((FocusedElement as MenuModList.ModButton).viewIndex + ((dir != NextDirection.Up) ? 1 : (-1)), 0, ModList.visibleModButtons.Count - 1)], 
			};
		case MenuUI._ModStatsRightPointer:
			if (ActiveInterface is InternalOI_Stats internalOI_Stats && ConfigContainer.ActiveTabIndex == 1)
			{
				if (!internalOI_Stats.btnAuthorExtra.IsInactive)
				{
					return internalOI_Stats.btnAuthorExtra;
				}
				if (internalOI_Stats.boxDescription != null)
				{
					return internalOI_Stats.boxDescription;
				}
				if (!internalOI_Stats.btnWorkshopUpload.IsInactive)
				{
					return internalOI_Stats.btnWorkshopUpload;
				}
			}
			if (ConfigContainer.instance._Mode == ConfigContainer.Mode.ModConfig)
			{
				return null;
			}
			return MenuTab.ApplyButton;
		case MenuUI._BackButtonUpPointer:
			if (ActiveInterface is InternalOI_Stats && ConfigContainer.ActiveTabIndex == 1)
			{
				if ((ActiveInterface as InternalOI_Stats).boxDescription != null)
				{
					return (ActiveInterface as InternalOI_Stats).boxDescription;
				}
				if (!(ActiveInterface as InternalOI_Stats).btnAuthorExtra.IsInactive)
				{
					return (ActiveInterface as InternalOI_Stats).btnAuthorExtra;
				}
			}
			if (ConfigContainer.instance._Mode == ConfigContainer.Mode.ModConfig)
			{
				return null;
			}
			return MenuTab.modList._roleButtons[5];
		case MenuUI._ModStatsButton:
			return ModList._roleButtons[0];
		case MenuUI._ModListExpandButton:
			return ModList._roleButtons[5];
		case MenuUI._LastActiveModButtonOrTop:
		case MenuUI._LastActiveModButtonOrBottom:
			return GetLastActiveModButton(menuUI == MenuUI._LastActiveModButtonOrTop);
		default:
			return null;
		}
	}

	private MenuModList.ModButton GetLastActiveModButton(bool top)
	{
		if (ActiveInterface is InternalOI_Stats internalOI_Stats && internalOI_Stats.previewMod != ActiveInterface.mod)
		{
			return ModList.GetModButton(internalOI_Stats.previewMod.id);
		}
		if (!top)
		{
			return ModList.GetModButtonAtBottom();
		}
		return ModList.GetModButtonAtTop();
	}
}
