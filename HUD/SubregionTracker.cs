using RWCustom;

namespace HUD;

public class SubregionTracker
{
	public TextPrompt textPrompt;

	public int lastRegion;

	public int lastShownRegion;

	public int counter;

	public bool showCycleNumber;

	private bool DEVBOOL;

	public bool PreRegionBump => counter == 1;

	public bool RegionBump => counter == 75;

	public SubregionTracker(TextPrompt textPrompt)
	{
		this.textPrompt = textPrompt;
		showCycleNumber = true;
	}

	public void Update()
	{
		Player player = textPrompt.hud.owner as Player;
		int num = 0;
		if (player.room != null && !player.room.world.singleRoomWorld && player.room.world.region != null)
		{
			for (int i = 1; i < player.room.world.region.subRegions.Count; i++)
			{
				if (player.room.abstractRoom.subregionName == player.room.world.region.subRegions[i])
				{
					num = i;
					break;
				}
			}
		}
		if (!DEVBOOL && num != 0 && player.room.game.manager.menuSetup.startGameCondition == ProcessManager.MenuSetup.StoryGameInitCondition.Dev)
		{
			lastShownRegion = num;
			DEVBOOL = true;
		}
		if (num != lastShownRegion && player.room != null && num != 0 && lastRegion == num && textPrompt.show == 0f)
		{
			counter++;
			if (counter > 80)
			{
				if ((num > 1 || lastShownRegion == 0 || (player.room.world.region.name != "SS" && (!ModManager.MSC || player.room.world.region.name != "DM"))) && num < player.room.world.region.subRegions.Count)
				{
					if (showCycleNumber && player.room.game.IsStorySession && player.room.game.manager.menuSetup.startGameCondition == ProcessManager.MenuSetup.StoryGameInitCondition.Load)
					{
						int num2 = player.room.game.GetStorySession.saveState.cycleNumber;
						if ((player.room.game.session as StoryGameSession).saveState.saveStateNumber == SlugcatStats.Name.Red && !Custom.rainWorld.ExpeditionMode)
						{
							num2 = RedsIllness.RedsCycles(player.room.game.GetStorySession.saveState.redExtraCycles) - num2;
						}
						string s = player.room.world.region.subRegions[num];
						if (num < player.room.world.region.altSubRegions.Count && player.room.world.region.altSubRegions[num] != null)
						{
							s = player.room.world.region.altSubRegions[num];
						}
						textPrompt.AddMessage(textPrompt.hud.rainWorld.inGameTranslator.Translate("Cycle") + " " + num2 + " ~ " + textPrompt.hud.rainWorld.inGameTranslator.Translate(s), 0, 160, darken: false, hideHud: true);
					}
					else
					{
						string s2 = player.room.world.region.subRegions[num];
						if (num < player.room.world.region.altSubRegions.Count && player.room.world.region.altSubRegions[num] != null)
						{
							s2 = player.room.world.region.altSubRegions[num];
						}
						textPrompt.AddMessage(textPrompt.hud.rainWorld.inGameTranslator.Translate(s2), 0, 160, darken: false, hideHud: true);
					}
				}
				showCycleNumber = false;
				lastShownRegion = num;
			}
		}
		else
		{
			counter = 0;
		}
		lastRegion = num;
	}
}
