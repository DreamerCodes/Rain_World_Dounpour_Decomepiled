using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace DevInterface;

public class DevTools
{
	public static string MOD_ID = "devtools";

	public static string ValidationString()
	{
		string text = "[" + MOD_ID + "]  ";
		bool[] array = new bool[25]
		{
			Custom.rainWorld.setup.invincibility,
			Custom.rainWorld.setup.world,
			Custom.rainWorld.setup.alwaysTravel,
			Custom.rainWorld.setup.cleanMaps,
			Custom.rainWorld.setup.cleanSpawns,
			Custom.rainWorld.setup.disableRain,
			Custom.rainWorld.setup.forcePrecycles,
			Custom.rainWorld.setup.forcePup,
			Custom.rainWorld.setup.loadGame,
			Custom.rainWorld.setup.loadProg,
			Custom.rainWorld.setup.lockTravel,
			Custom.rainWorld.setup.playerGlowing,
			Custom.rainWorld.setup.randomStart,
			Custom.rainWorld.setup.revealMap,
			Custom.rainWorld.setup.startScreen,
			Custom.rainWorld.setup.theMark,
			Custom.rainWorld.setup.cycleStartUp,
			Custom.rainWorld.setup.devToolsActive,
			Custom.rainWorld.setup.lizardLaserEyes,
			Custom.rainWorld.setup.multiUseGates,
			Custom.rainWorld.setup.saintInfinitePower,
			Custom.rainWorld.setup.testMoonCloak,
			Custom.rainWorld.setup.testMoonFixed,
			Custom.rainWorld.setup.worldCreaturesSpawn,
			Custom.rainWorld.setup.unlockMSCCharacters
		};
		Dictionary<string, int> dictionary = new Dictionary<string, int>
		{
			{
				"black",
				Custom.rainWorld.setup.black
			},
			{
				"blue",
				Custom.rainWorld.setup.blue
			},
			{
				"centipedes",
				Custom.rainWorld.setup.centipedes
			},
			{
				"cicadas",
				Custom.rainWorld.setup.cicadas
			},
			{
				"deers",
				Custom.rainWorld.setup.deers
			},
			{
				"dropbugs",
				Custom.rainWorld.setup.dropbugs
			},
			{
				"flies",
				Custom.rainWorld.setup.flies
			},
			{
				"ghosts",
				Custom.rainWorld.setup.ghosts
			},
			{
				"green",
				Custom.rainWorld.setup.green
			},
			{
				"leeches",
				Custom.rainWorld.setup.leeches
			},
			{
				"overseers",
				Custom.rainWorld.setup.overseers
			},
			{
				"pink",
				Custom.rainWorld.setup.pink
			},
			{
				"reds",
				Custom.rainWorld.setup.reds
			},
			{
				"salamanders",
				Custom.rainWorld.setup.salamanders
			},
			{
				"scavengers",
				Custom.rainWorld.setup.scavengers
			},
			{
				"snails",
				Custom.rainWorld.setup.snails
			},
			{
				"spears",
				Custom.rainWorld.setup.spears
			},
			{
				"spiders",
				Custom.rainWorld.setup.spiders
			},
			{
				"vultures",
				Custom.rainWorld.setup.vultures
			},
			{
				"white",
				Custom.rainWorld.setup.white
			},
			{
				"yellows",
				Custom.rainWorld.setup.yellows
			},
			{
				"bigEels",
				Custom.rainWorld.setup.bigEels
			},
			{
				"bigSpiders",
				Custom.rainWorld.setup.bigSpiders
			},
			{
				"centiWings",
				Custom.rainWorld.setup.centiWings
			},
			{
				"cheatKarma",
				Custom.rainWorld.setup.cheatKarma
			},
			{
				"cyanLizards",
				Custom.rainWorld.setup.cyanLizards
			},
			{
				"eggBugs",
				Custom.rainWorld.setup.eggBugs
			},
			{
				"fireSpears",
				Custom.rainWorld.setup.fireSpears
			},
			{
				"garbageWorms",
				Custom.rainWorld.setup.garbageWorms
			},
			{
				"jetFish",
				Custom.rainWorld.setup.jetFish
			},
			{
				"kingVultures",
				Custom.rainWorld.setup.kingVultures
			},
			{
				"lanternMice",
				Custom.rainWorld.setup.lanternMice
			},
			{
				"mirosBirds",
				Custom.rainWorld.setup.mirosBirds
			},
			{
				"needleWorms",
				Custom.rainWorld.setup.needleWorms
			},
			{
				"poleMimics",
				Custom.rainWorld.setup.poleMimics
			},
			{
				"proceedLineages",
				Custom.rainWorld.setup.proceedLineages
			},
			{
				"redCentis",
				Custom.rainWorld.setup.redCentis
			},
			{
				"scavBombs",
				Custom.rainWorld.setup.scavBombs
			},
			{
				"scavengersShy",
				Custom.rainWorld.setup.scavengersShy
			},
			{
				"scavengersLikePlayer",
				Custom.rainWorld.setup.scavengersLikePlayer
			},
			{
				"scavLanterns",
				Custom.rainWorld.setup.scavLanterns
			},
			{
				"seaLeeches",
				Custom.rainWorld.setup.seaLeeches
			},
			{
				"smallCentipedes",
				Custom.rainWorld.setup.smallCentipedes
			},
			{
				"spitterSpiders",
				Custom.rainWorld.setup.spitterSpiders
			},
			{
				"templeGuards",
				Custom.rainWorld.setup.templeGuards
			},
			{
				"tentaclePlants",
				Custom.rainWorld.setup.tentaclePlants
			},
			{
				"tubeWorms",
				Custom.rainWorld.setup.tubeWorms
			},
			{
				"broLongLegs",
				Custom.rainWorld.setup.broLongLegs
			},
			{
				"daddyLongLegs",
				Custom.rainWorld.setup.daddyLongLegs
			},
			{
				"defaultSettingsScreen",
				Custom.rainWorld.setup.defaultSettingsScreen
			},
			{
				"smallNeedleWorms",
				Custom.rainWorld.setup.smallNeedleWorms
			}
		};
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < array.Length; i++)
		{
			num += (int)Mathf.Pow(2f, i % 4) * (array[i] ? 1 : 0);
			if (i % 4 == 3 || i == array.Length - 1)
			{
				text += num.ToString("X");
				num2++;
				if (num2 % 4 == 0)
				{
					text += "  ";
				}
				num = 0;
			}
		}
		text += " ";
		foreach (KeyValuePair<string, int> item in dictionary)
		{
			if (item.Value != 0)
			{
				text = text + item.Key + item.Value + " ";
			}
		}
		if (Custom.rainWorld.setup.startMap != "")
		{
			text = text + Custom.rainWorld.setup.startMap + " ";
		}
		if (Custom.rainWorld.setup.lungs != 128)
		{
			text = text + "lungs" + Custom.rainWorld.setup.lungs + " ";
		}
		if (Custom.rainWorld.setup.artificerDreamTest != -1)
		{
			text = text + "artificerDreamTest" + Custom.rainWorld.setup.artificerDreamTest + " ";
		}
		if (Custom.rainWorld.setup.betaTestSlugcat != null)
		{
			text = text + "betaTestSlugcat" + Custom.rainWorld.setup.betaTestSlugcat.value + " ";
		}
		if (Custom.rainWorld.setup.cycleTimeMin != 400)
		{
			text = text + "cycleTimeMin" + Custom.rainWorld.setup.cycleTimeMin + " ";
		}
		if (Custom.rainWorld.setup.cycleTimeMax != 800)
		{
			text = text + "cycleTimeMax" + Custom.rainWorld.setup.cycleTimeMax + " ";
		}
		if (Custom.rainWorld.setup.gravityFlickerCycleMin != 8)
		{
			text = text + "gravityFlickerCycleMin" + Custom.rainWorld.setup.gravityFlickerCycleMin + " ";
		}
		if (Custom.rainWorld.setup.gravityFlickerCycleMax != 18)
		{
			text = text + "gravityFlickerCycleMax" + Custom.rainWorld.setup.gravityFlickerCycleMax + " ";
		}
		if (Custom.rainWorld.setup.singlePlayerChar != -1)
		{
			text = text + "singlePlayerChar" + Custom.rainWorld.setup.singlePlayerChar + " ";
		}
		if (Custom.rainWorld.setup.slugPupsMax != -1)
		{
			text = text + "slugPupsMax" + Custom.rainWorld.setup.slugPupsMax + " ";
		}
		return text;
	}
}
