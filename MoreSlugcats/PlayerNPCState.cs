using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace MoreSlugcats;

public class PlayerNPCState : PlayerState
{
	public bool Malnourished;

	public bool DieOfStarvation;

	public int KarmaLevel;

	public bool HasMark;

	public bool Glowing;

	public bool Drone;

	public bool HasCloak;

	private AbstractCreature player;

	public AbstractPhysicalObject StomachObject;

	public PlayerNPCState(AbstractCreature abstractCreature, int playerNumber)
		: base(abstractCreature, playerNumber, MoreSlugcatsEnums.SlugcatStatsName.Slugpup, isGhost: false)
	{
		player = abstractCreature;
	}

	public override string ToString()
	{
		string text = BaseSaveString();
		text = text + "Food<cC>" + foodInStomach + "<cB>";
		text = text + "Malnourished<cC>" + Malnourished + "<cB>";
		text = text + "Pup<cC>" + isPup + "<cB>";
		text = text + "Glow<cC>" + Glowing + "<cB>";
		text = text + "Mark<cC>" + HasMark + "<cB>";
		text = text + "Karma<cC>" + KarmaLevel + "<cB>";
		text = text + "Drone<cC>" + Drone + "<cB>";
		text = text + "Cloak<cC>" + HasCloak + "<cB>";
		text = text + "FullGrown<cC>" + forceFullGrown + "<cB>";
		text = text + "Stomach<cC>" + ((StomachObject != null) ? StomachObject.ToString() : "NULL") + "<cB>";
		foreach (KeyValuePair<string, string> unrecognizedSaveString in unrecognizedSaveStrings)
		{
			text = text + unrecognizedSaveString.Key + "<cC>" + unrecognizedSaveString.Value + "<cB>";
		}
		return text;
	}

	public override void LoadFromString(string[] s)
	{
		base.LoadFromString(s);
		for (int i = 0; i < s.Length - 1; i++)
		{
			string[] array = Regex.Split(s[i], "<cC>");
			switch (array[0])
			{
			case "Food":
				foodInStomach = int.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "Malnourished":
				Malnourished = bool.Parse(array[1]);
				break;
			case "Pup":
				isPup = bool.Parse(array[1]);
				break;
			case "FullGrown":
				forceFullGrown = bool.Parse(array[1]);
				break;
			case "Drone":
				Drone = bool.Parse(array[1]);
				break;
			case "Glow":
				Glowing = bool.Parse(array[1]);
				break;
			case "Mark":
				HasMark = bool.Parse(array[1]);
				break;
			case "Karma":
				KarmaLevel = int.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "Cloak":
				HasCloak = bool.Parse(array[1]);
				break;
			case "Stomach":
			{
				string text = array[1];
				if (text != "NULL")
				{
					if (text.Contains("<oA>"))
					{
						StomachObject = SaveState.AbstractPhysicalObjectFromString(player.Room.world, text);
					}
					else if (text.Contains("<cA>"))
					{
						StomachObject = SaveState.AbstractCreatureFromString(player.Room.world, text, onlyInCurrentRegion: false);
					}
				}
				break;
			}
			default:
				if (array.Length >= 2)
				{
					unrecognizedSaveStrings[array[0]] = array[1];
				}
				break;
			}
		}
		unrecognizedSaveStrings.Remove("Food");
		unrecognizedSaveStrings.Remove("Malnourished");
		unrecognizedSaveStrings.Remove("Pup");
		unrecognizedSaveStrings.Remove("FullGrown");
		unrecognizedSaveStrings.Remove("Drone");
		unrecognizedSaveStrings.Remove("Glow");
		unrecognizedSaveStrings.Remove("Mark");
		unrecognizedSaveStrings.Remove("Karma");
		unrecognizedSaveStrings.Remove("Cloak");
		unrecognizedSaveStrings.Remove("Stomach");
	}

	public override void CycleTick()
	{
		if (player.world.game.IsStorySession)
		{
			if (player.world.game.manager.menuSetup.startGameCondition == ProcessManager.MenuSetup.StoryGameInitCondition.FastTravel || player.world.game.manager.menuSetup.startGameCondition == ProcessManager.MenuSetup.StoryGameInitCondition.StartWithFastTravel || player.world.game.GetStorySession.saveState.justBeatGame)
			{
				foodInStomach = SlugcatStats.SlugcatFoodMeter(MoreSlugcatsEnums.SlugcatStatsName.Slugpup).y;
				Malnourished = false;
			}
			else
			{
				foodInStomach -= (Malnourished ? SlugcatStats.SlugcatFoodMeter(MoreSlugcatsEnums.SlugcatStatsName.Slugpup).x : SlugcatStats.SlugcatFoodMeter(MoreSlugcatsEnums.SlugcatStatsName.Slugpup).y);
				if (foodInStomach < 0)
				{
					if (Malnourished)
					{
						player.Die();
					}
					foodInStomach = 0;
					Malnourished = true;
				}
				else
				{
					Malnourished = false;
				}
			}
		}
		else
		{
			foodInStomach = 0;
			Malnourished = false;
		}
		base.CycleTick();
	}
}
