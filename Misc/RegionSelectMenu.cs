public class RegionSelectMenu : SelectionMenu
{
	private string[] startRooms;

	public RegionSelectMenu(RainWorld rw)
		: base("Region Select: ", new string[12]
		{
			"Suburban", "Drainage System", "Heavy Industrial", "Chimney Canopy", "Sky Islands", "Garbage Wastes", "Shadow Urban", "Shoreline", "Linear Farms", "Underhang/The Wall",
			"Subterranean", "Superstructure"
		})
	{
		startRooms = new string[12];
		startRooms[0] = "SU_A22";
		startRooms[1] = "DS_B02";
		startRooms[2] = "HI_B04";
		startRooms[3] = "CC_S01r";
		startRooms[4] = "SI_A02";
		startRooms[5] = "GW_B01";
		startRooms[6] = "SH_D01";
		startRooms[7] = "SL_A08";
		startRooms[8] = "LF_C01";
		startRooms[9] = "UW_D02";
		startRooms[10] = "SB_H03";
		startRooms[11] = "SS_A08";
	}

	protected override void Select()
	{
	}
}
