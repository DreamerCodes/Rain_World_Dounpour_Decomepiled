using MoreSlugcats;

namespace Menu;

public class GhostEncounterScreen : KarmaLadderScreen
{
	public bool IsAnyGhostScreen
	{
		get
		{
			if (!IsGhostIncreaseKarmaCapScreen && !IsKarmaToMaxScreen && !IsKarmaToMinScreen)
			{
				return IsVengeanceGhostScreen;
			}
			return true;
		}
	}

	public bool IsGhostIncreaseKarmaCapScreen => ID == ProcessManager.ProcessID.GhostScreen;

	public bool IsKarmaToMaxScreen => ID == ProcessManager.ProcessID.KarmaToMaxScreen;

	public bool IsKarmaToMinScreen
	{
		get
		{
			if (ModManager.MSC)
			{
				return ID == MoreSlugcatsEnums.ProcessID.KarmaToMinScreen;
			}
			return false;
		}
	}

	public bool IsVengeanceGhostScreen
	{
		get
		{
			if (ModManager.MSC)
			{
				return ID == MoreSlugcatsEnums.ProcessID.VengeanceGhostScreen;
			}
			return false;
		}
	}

	public override bool LadderInCenter => true;

	public GhostEncounterScreen(ProcessManager manager, ProcessManager.ProcessID ID)
		: base(manager, ID)
	{
		mySoundLoopID = SoundID.MENU_Death_Screen_LOOP;
		PlaySound(SoundID.MENU_Enter_Death_Screen);
	}

	public override void GetDataFromGame(SleepDeathScreenDataPackage package)
	{
		if (IsGhostIncreaseKarmaCapScreen)
		{
			preGhostEncounterKarmaCap = package.karma.y - 1;
			if (preGhostEncounterKarmaCap == 5)
			{
				preGhostEncounterKarmaCap--;
			}
		}
		if (ModManager.Expedition && manager.rainWorld.ExpeditionMode)
		{
			package.karma.y = 4;
			package.karma.x = 4;
		}
		base.GetDataFromGame(package);
	}
}
