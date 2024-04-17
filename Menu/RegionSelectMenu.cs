using UnityEngine;

namespace Menu;

public class RegionSelectMenu : Menu
{
	private string[,] regions;

	public RegionSelectMenu(ProcessManager manager)
		: base(manager, ProcessManager.ProcessID.RegionSelect)
	{
		pages.Add(new Page(this, null, "main", 0));
		regions = new string[12, 2]
		{
			{ "SUBURBAN", "SU_A22" },
			{ "HEAVY INDUSTRIAL", "HI_B04" },
			{ "DRAINAGE SYSTEM", "DS_B02" },
			{ "CHIMNEY CANOPY", "CC_A16" },
			{ "LINEAR FARMS", "LF_A14" },
			{ "SKY ISLANDS", "SI_B13" },
			{ "SHADOW URBAN", "SH_D01" },
			{ "GARBAGE WASTES", "GW_B01" },
			{ "SHORELINE", "SL_A08" },
			{ "THE WALL", "UW_D02" },
			{ "SUBTERRANEAN", "SB_F03" },
			{ "SUPER STRUCTURE", "SS_A08" }
		};
		for (int i = 0; i < regions.GetLength(0); i++)
		{
			pages[0].subObjects.Add(new SimpleButton(this, pages[0], regions[i, 0], regions[i, 0], new Vector2(608f, 640f - 40f * (float)i), new Vector2(150f, 30f)));
		}
		pages[0].subObjects.Add(new SimpleButton(this, pages[0], "BACK", "BACK", new Vector2(200f, 50f), new Vector2(110f, 30f)));
		mySoundLoopID = SoundID.MENU_Main_Menu_LOOP;
	}

	public override void Singal(MenuObject sender, string message)
	{
		for (int i = 0; i < regions.GetLength(0); i++)
		{
			if (message == regions[i, 0])
			{
				manager.menuSetup.startGameCondition = ProcessManager.MenuSetup.StoryGameInitCondition.RegionSelect;
				manager.menuSetup.regionSelectRoom = regions[i, 1];
				manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Game);
				PlaySound(SoundID.MENU_Continue_Game);
				return;
			}
		}
		if (message != null && message == "BACK")
		{
			manager.RequestMainProcessSwitch(ProcessManager.ProcessID.MainMenu);
			PlaySound(SoundID.MENU_Switch_Page_Out);
		}
	}
}
