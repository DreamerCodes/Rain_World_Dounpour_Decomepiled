using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Expedition;
using JollyCoop;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class SlugcatStats
{
	public class Name : ExtEnum<Name>
	{
		public static readonly Name White = new Name("White", register: true);

		public static readonly Name Yellow = new Name("Yellow", register: true);

		public static readonly Name Red = new Name("Red", register: true);

		public static readonly Name Night = new Name("Night", register: true);

		public Name(string value, bool register = false)
			: base(value, register)
		{
		}

		public static void Init()
		{
			ExtEnum<Name>.values = new ExtEnumType();
			ExtEnum<Name>.values.AddEntry(White.value);
			ExtEnum<Name>.values.AddEntry(Yellow.value);
			ExtEnum<Name>.values.AddEntry(Red.value);
			ExtEnum<Name>.values.AddEntry(Night.value);
		}

		public static Name ArenaColor(int playerIndex)
		{
			return playerIndex switch
			{
				0 => White, 
				1 => Yellow, 
				2 => Red, 
				3 => Night, 
				_ => null, 
			};
		}
	}

	public Name name = Name.White;

	public int maxFood;

	public int foodToHibernate;

	public float runspeedFac = 1f;

	public float bodyWeightFac = 1f;

	public float generalVisibilityBonus;

	public float visualStealthInSneakMode = 0.5f;

	public float loudnessFac = 1f;

	public float lungsFac;

	public bool malnourished;

	public int throwingSkill;

	public float poleClimbSpeedFac;

	public float corridorClimbSpeedFac;

	public static IntVector2 SlugcatFoodMeter(Name slugcat)
	{
		if (slugcat == Name.White)
		{
			return new IntVector2(7, 4);
		}
		if (slugcat == Name.Yellow)
		{
			return new IntVector2(5, 3);
		}
		if (slugcat == Name.Red)
		{
			return new IntVector2(9, 6);
		}
		if (ModManager.MSC)
		{
			if (slugcat == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel)
			{
				return new IntVector2(12, 12);
			}
			if (slugcat == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
			{
				return new IntVector2(6, 5);
			}
			if (slugcat == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
			{
				return new IntVector2(9, 6);
			}
			if (slugcat == MoreSlugcatsEnums.SlugcatStatsName.Saint)
			{
				return new IntVector2(5, 4);
			}
			if (slugcat == MoreSlugcatsEnums.SlugcatStatsName.Spear)
			{
				return new IntVector2(10, 5);
			}
			if (slugcat == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
			{
				return new IntVector2(11, 7);
			}
			if (slugcat == MoreSlugcatsEnums.SlugcatStatsName.Slugpup)
			{
				return new IntVector2(3, 2);
			}
		}
		return new IntVector2(7, 4);
	}

	public SlugcatStats(Name slugcat, bool malnourished)
	{
		this.malnourished = malnourished;
		name = slugcat;
		IntVector2 intVector = SlugcatFoodMeter(slugcat);
		maxFood = intVector.x;
		foodToHibernate = intVector.y;
		if (malnourished)
		{
			foodToHibernate = maxFood;
		}
		lungsFac = 1f;
		poleClimbSpeedFac = 1f;
		corridorClimbSpeedFac = 1f;
		if (slugcat == Name.White)
		{
			throwingSkill = 1;
		}
		else if (slugcat == Name.Yellow)
		{
			bodyWeightFac = 0.95f;
			generalVisibilityBonus = -0.1f;
			visualStealthInSneakMode = 0.6f;
			loudnessFac = 0.75f;
			lungsFac = ((ModManager.MMF && MMF.cfgMonkBreathTime.Value) ? 0.8f : 1.2f);
			throwingSkill = 0;
		}
		else if (slugcat == Name.Red)
		{
			runspeedFac = 1.2f;
			bodyWeightFac = 1.12f;
			generalVisibilityBonus = 0.1f;
			visualStealthInSneakMode = 0.3f;
			loudnessFac = 1.35f;
			throwingSkill = 2;
			poleClimbSpeedFac = 1.25f;
			corridorClimbSpeedFac = 1.2f;
		}
		if (ModManager.MSC)
		{
			if (slugcat == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
			{
				bodyWeightFac = 1.35f;
				poleClimbSpeedFac = 0.8f;
				corridorClimbSpeedFac = 0.86f;
				throwingSkill = 2;
				loudnessFac = 1.5f;
				generalVisibilityBonus = 0.3f;
				visualStealthInSneakMode = 0.2f;
			}
			else if (slugcat == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
			{
				bodyWeightFac = 0.95f;
				throwingSkill = 1;
				lungsFac = 0.15f;
				runspeedFac = 1.75f;
				poleClimbSpeedFac = 1.8f;
				corridorClimbSpeedFac = 1.6f;
			}
			else if (slugcat == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
			{
				runspeedFac = 1.2f;
				bodyWeightFac = 1.12f;
				generalVisibilityBonus = 0.1f;
				visualStealthInSneakMode = 0.3f;
				loudnessFac = 1.35f;
				throwingSkill = 2;
				poleClimbSpeedFac = 1.25f;
				corridorClimbSpeedFac = 1.2f;
			}
			else if (slugcat == MoreSlugcatsEnums.SlugcatStatsName.Saint)
			{
				throwingSkill = 0;
			}
			else if (slugcat == MoreSlugcatsEnums.SlugcatStatsName.Spear)
			{
				runspeedFac = 1.2f;
				bodyWeightFac = 0.85f;
				generalVisibilityBonus = 0.1f;
				visualStealthInSneakMode = 0.3f;
				loudnessFac = 1.35f;
				throwingSkill = 2;
				poleClimbSpeedFac = 1.25f;
				corridorClimbSpeedFac = 1.2f;
			}
			else if (slugcat == MoreSlugcatsEnums.SlugcatStatsName.Slugpup)
			{
				bodyWeightFac = 0.65f;
				generalVisibilityBonus = -0.2f;
				visualStealthInSneakMode = 0.6f;
				loudnessFac = 0.5f;
				lungsFac = 0.8f;
				throwingSkill = 0;
				poleClimbSpeedFac = 0.8f;
				corridorClimbSpeedFac = 0.8f;
				runspeedFac = 0.8f;
			}
			else if (ModManager.Expedition && Custom.rainWorld.ExpeditionMode && ModManager.MSC && slugcat == MoreSlugcatsEnums.SlugcatStatsName.Saint)
			{
				throwingSkill = 0;
			}
		}
		if (ModManager.Expedition && ModManager.MSC && Custom.rainWorld.ExpeditionMode && ExpeditionGame.activeUnlocks.Contains("unl-agility"))
		{
			lungsFac = 0.15f;
			runspeedFac = 1.75f;
			poleClimbSpeedFac = 1.8f;
			corridorClimbSpeedFac = 1.6f;
		}
		if (malnourished)
		{
			throwingSkill = 0;
			if (ModManager.MSC && slugcat == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
			{
				bodyWeightFac = 0.91f;
				runspeedFac = 1.27f;
				poleClimbSpeedFac = 1.1f;
				corridorClimbSpeedFac = 1.2f;
			}
			else if (ModManager.MSC && slugcat == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
			{
				bodyWeightFac = 1.15f;
				runspeedFac = 0.875f;
				poleClimbSpeedFac = 0.75f;
				corridorClimbSpeedFac = 0.81f;
				throwingSkill = 2;
			}
			else if (ModManager.Expedition && Custom.rainWorld.ExpeditionMode && ExpeditionGame.activeUnlocks.Contains("unl-agility"))
			{
				runspeedFac = 1.27f;
				poleClimbSpeedFac = 1.1f;
				corridorClimbSpeedFac = 1.2f;
			}
			else
			{
				bodyWeightFac = Mathf.Min(bodyWeightFac, 0.9f);
				runspeedFac = 0.875f;
				poleClimbSpeedFac = 0.8f;
				corridorClimbSpeedFac = 0.86f;
			}
		}
	}

	public static int NourishmentOfObjectEaten(Name slugcatIndex, IPlayerEdible eatenobject)
	{
		if (ModManager.MSC && slugcatIndex == MoreSlugcatsEnums.SlugcatStatsName.Saint && (eatenobject is JellyFish || eatenobject is Centipede || eatenobject is Fly || eatenobject is VultureGrub || eatenobject is SmallNeedleWorm || eatenobject is Hazer))
		{
			return -1;
		}
		int num = 0;
		if (slugcatIndex == Name.Red || (ModManager.MSC && slugcatIndex == MoreSlugcatsEnums.SlugcatStatsName.Artificer))
		{
			bool flag = true;
			if (eatenobject is Centipede || eatenobject is VultureGrub || eatenobject is Hazer || eatenobject is EggBugEgg || eatenobject is SmallNeedleWorm || eatenobject is JellyFish)
			{
				flag = false;
			}
			num = ((!flag) ? (num + 4 * eatenobject.FoodPoints) : (num + eatenobject.FoodPoints));
		}
		else if (!ModManager.MSC || slugcatIndex != MoreSlugcatsEnums.SlugcatStatsName.Spear)
		{
			num = ((!ModManager.MSC || !(eatenobject is GlowWeed)) ? (num + 4 * eatenobject.FoodPoints) : (num + 2));
		}
		return num;
	}

	public static bool PearlsGivePassageProgress(StoryGameSession session)
	{
		if (session.saveState.deathPersistentSaveData.theMark)
		{
			if (ModManager.Expedition && ModManager.MSC && session.game.rainWorld.ExpeditionMode && session.saveStateNumber != MoreSlugcatsEnums.SlugcatStatsName.Saint)
			{
				return true;
			}
			if (ModManager.MSC && session.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Saint)
			{
				return false;
			}
			if (session.saveStateNumber == Name.Red || (ModManager.MSC && (session.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Artificer || session.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Spear || session.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)))
			{
				return true;
			}
			if (session.saveState.miscWorldSaveData.EverMetMoon && session.saveState.miscWorldSaveData.SLOracleState.SpeakingTerms)
			{
				return true;
			}
		}
		return false;
	}

	public static float SpearSpawnExplosiveRandomChance(Name index)
	{
		if (ModManager.MSC && (index == MoreSlugcatsEnums.SlugcatStatsName.Spear || index == MoreSlugcatsEnums.SlugcatStatsName.Artificer))
		{
			return 0.012f;
		}
		if (index == Name.Red)
		{
			return 0.008f;
		}
		if (ModManager.MSC && index == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
		{
			return 0.001f;
		}
		return 0f;
	}

	public static float SpearSpawnElectricRandomChance(Name index)
	{
		if (!ModManager.MSC)
		{
			return 0f;
		}
		if (index == Name.Red)
		{
			return 0.011f;
		}
		if (index == MoreSlugcatsEnums.SlugcatStatsName.Spear)
		{
			return 0.082f;
		}
		if (index == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
		{
			return 0.065f;
		}
		if (index == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
		{
			return 0.008f;
		}
		return 0f;
	}

	public static float SpearSpawnModifier(Name index, float originalSpearChance)
	{
		if (index == Name.Red)
		{
			return Mathf.Pow(originalSpearChance, 0.85f);
		}
		if (ModManager.MSC && index == MoreSlugcatsEnums.SlugcatStatsName.Spear)
		{
			return Mathf.Pow(originalSpearChance, 0.9f);
		}
		if (ModManager.MSC && index == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
		{
			return Mathf.Pow(originalSpearChance, 0.8f);
		}
		if (ModManager.MSC && index == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
		{
			return Mathf.Pow(originalSpearChance, 0.95f);
		}
		if (ModManager.MSC && index == MoreSlugcatsEnums.SlugcatStatsName.Saint)
		{
			return Mathf.Pow(originalSpearChance, 1.4f);
		}
		return originalSpearChance;
	}

	public static bool AutoGrabBatflys(Name slugcatNum)
	{
		if (!ModManager.MMF)
		{
			return true;
		}
		if (slugcatNum == Name.Red && !MMF.cfgHunterBatflyAutograb.Value)
		{
			return true;
		}
		if (slugcatNum == Name.Red || (ModManager.MSC && (slugcatNum == MoreSlugcatsEnums.SlugcatStatsName.Artificer || slugcatNum == MoreSlugcatsEnums.SlugcatStatsName.Saint || slugcatNum == MoreSlugcatsEnums.SlugcatStatsName.Spear)))
		{
			return false;
		}
		return true;
	}

	public static int SlugcatStartingKarma(Name slugcatNum)
	{
		if (ModManager.MSC && slugcatNum == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
		{
			return 0;
		}
		if (ModManager.MSC && slugcatNum == MoreSlugcatsEnums.SlugcatStatsName.Saint)
		{
			return 1;
		}
		return 4;
	}

	public static bool SlugcatCanMaul(Name slugcatNum)
	{
		if (!ModManager.MSC)
		{
			return false;
		}
		if (!(slugcatNum == MoreSlugcatsEnums.SlugcatStatsName.Artificer))
		{
			return slugcatNum == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel;
		}
		return true;
	}

	public static string getSlugcatName(Name i)
	{
		if (i == null)
		{
			return "Random";
		}
		if (i == Name.White)
		{
			return "Survivor";
		}
		if (i == Name.Yellow)
		{
			return "Monk";
		}
		if (i == Name.Red)
		{
			return "Hunter";
		}
		if (ModManager.MSC && i == MoreSlugcatsEnums.SlugcatStatsName.Spear)
		{
			return "Spearmaster";
		}
		return i.value;
	}

	public static List<string> SlugcatStoryRegions(Name i)
	{
		string[] source = ((!ModManager.MSC) ? new string[12]
		{
			"SU", "HI", "DS", "CC", "GW", "SH", "SL", "SI", "LF", "UW",
			"SS", "SB"
		} : ((i == MoreSlugcatsEnums.SlugcatStatsName.Rivulet) ? new string[14]
		{
			"SU", "HI", "DS", "CC", "GW", "SH", "VS", "SL", "SI", "LF",
			"UW", "RM", "SB", "MS"
		} : ((i == MoreSlugcatsEnums.SlugcatStatsName.Artificer) ? new string[14]
		{
			"SU", "HI", "DS", "CC", "GW", "SH", "VS", "LM", "SI", "LF",
			"UW", "SS", "SB", "LC"
		} : ((i == MoreSlugcatsEnums.SlugcatStatsName.Saint) ? new string[12]
		{
			"SU", "HI", "UG", "CC", "GW", "VS", "CL", "SL", "SI", "LF",
			"SB", "HR"
		} : ((i == MoreSlugcatsEnums.SlugcatStatsName.Spear) ? new string[14]
		{
			"SU", "HI", "DS", "CC", "GW", "SH", "VS", "LM", "SI", "LF",
			"UW", "SS", "SB", "DM"
		} : ((!(i == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)) ? new string[13]
		{
			"SU", "HI", "DS", "CC", "GW", "SH", "VS", "SL", "SI", "LF",
			"UW", "SS", "SB"
		} : new string[14]
		{
			"SU", "HI", "DS", "CC", "GW", "SH", "VS", "SL", "SI", "LF",
			"UW", "SS", "SB", "OE"
		}))))));
		return source.ToList();
	}

	[Obsolete("Renamed to SlugcatStoryRegions")]
	public static string[] getSlugcatStoryRegions(Name i)
	{
		return SlugcatStoryRegions(i).ToArray();
	}

	public static List<string> SlugcatOptionalRegions(Name i)
	{
		string[] source = ((!ModManager.MSC) ? new string[0] : ((!(i == Name.White) && !(i == Name.Yellow)) ? ((!(i == Name.Red) && !(i == MoreSlugcatsEnums.SlugcatStatsName.Saint) && !(i == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)) ? new string[0] : new string[1] { "MS" }) : new string[2] { "OE", "MS" }));
		return source.ToList();
	}

	[Obsolete("Renamed to SlugcatOptionalRegions")]
	public static string[] getSlugcatOptionalRegions(Name i)
	{
		return SlugcatOptionalRegions(i).ToArray();
	}

	public static LinkedList<Name> SlugcatTimelineOrder()
	{
		Name[] collection = ((!ModManager.MSC) ? new Name[3]
		{
			Name.Red,
			Name.White,
			Name.Yellow
		} : new Name[9]
		{
			MoreSlugcatsEnums.SlugcatStatsName.Spear,
			MoreSlugcatsEnums.SlugcatStatsName.Artificer,
			MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel,
			Name.Red,
			MoreSlugcatsEnums.SlugcatStatsName.Gourmand,
			Name.White,
			Name.Yellow,
			MoreSlugcatsEnums.SlugcatStatsName.Rivulet,
			MoreSlugcatsEnums.SlugcatStatsName.Saint
		});
		return new LinkedList<Name>(collection);
	}

	[Obsolete("Renamed to SlugcatTimelineOrder")]
	public static Name[] getSlugcatTimelineOrder()
	{
		return SlugcatTimelineOrder().ToArray();
	}

	public static bool HiddenOrUnplayableSlugcat(Name i)
	{
		if (i == Name.Night)
		{
			return true;
		}
		if (ModManager.MSC && i == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel)
		{
			return true;
		}
		if (ModManager.MSC && i == MoreSlugcatsEnums.SlugcatStatsName.Slugpup)
		{
			return true;
		}
		if (ModManager.JollyCoop && i == JollyEnums.Name.JollyPlayer1)
		{
			return true;
		}
		if (ModManager.JollyCoop && i == JollyEnums.Name.JollyPlayer2)
		{
			return true;
		}
		if (ModManager.JollyCoop && i == JollyEnums.Name.JollyPlayer3)
		{
			return true;
		}
		if (ModManager.JollyCoop && i == JollyEnums.Name.JollyPlayer4)
		{
			return true;
		}
		return false;
	}

	public static bool SlugcatUnlocked(Name i, RainWorld rainWorld)
	{
		if (ModManager.MSC && global::MoreSlugcats.MoreSlugcats.chtUnlockCampaigns.Value)
		{
			return true;
		}
		if (ModManager.Expedition && rainWorld.ExpeditionMode && ExpeditionGame.unlockedExpeditionSlugcats.Contains(i))
		{
			return true;
		}
		bool num = ModManager.MSC && (File.Exists((Custom.RootFolderDirectory() + Path.DirectorySeparatorChar + "unlockmsc.txt").ToLowerInvariant()) || rainWorld.setup.unlockMSCCharacters);
		bool redUnlocked = rainWorld.progression.miscProgressionData.redUnlocked;
		bool flag = ModManager.MSC && (rainWorld.progression.miscProgressionData.beaten_Hunter || rainWorld.progression.miscProgressionData.beaten_Gourmand || rainWorld.progression.miscProgressionData.beaten_Artificer);
		bool flag2 = ModManager.MSC && rainWorld.progression.miscProgressionData.beaten_Rivulet && rainWorld.progression.miscProgressionData.beaten_SpearMaster;
		if (num)
		{
			return true;
		}
		if (ModManager.MSC && rainWorld.setup.betaTestSlugcat == i)
		{
			return true;
		}
		if ((i == Name.Red || (ModManager.MSC && i == MoreSlugcatsEnums.SlugcatStatsName.Gourmand) || (ModManager.MSC && i == MoreSlugcatsEnums.SlugcatStatsName.Artificer)) && redUnlocked)
		{
			return true;
		}
		if (ModManager.MSC && (i == MoreSlugcatsEnums.SlugcatStatsName.Spear || i == MoreSlugcatsEnums.SlugcatStatsName.Rivulet) && flag)
		{
			return true;
		}
		if (ModManager.MSC && i == MoreSlugcatsEnums.SlugcatStatsName.Saint && flag2)
		{
			return true;
		}
		if (i != Name.Red && (!ModManager.MSC || (i != MoreSlugcatsEnums.SlugcatStatsName.Gourmand && i != MoreSlugcatsEnums.SlugcatStatsName.Artificer && i != MoreSlugcatsEnums.SlugcatStatsName.Rivulet && i != MoreSlugcatsEnums.SlugcatStatsName.Spear && i != MoreSlugcatsEnums.SlugcatStatsName.Saint)))
		{
			return true;
		}
		return false;
	}

	public static bool IsSlugcatFromMSC(Name i)
	{
		if (!(i.value == "Rivulet") && !(i.value == "Artificer") && !(i.value == "Saint") && !(i.value == "Spear") && !(i.value == "Gourmand"))
		{
			return i.value == "Inv";
		}
		return true;
	}
}
