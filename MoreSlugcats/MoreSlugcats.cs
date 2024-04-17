using System.Collections.Generic;

namespace MoreSlugcats;

public class MoreSlugcats
{
	public static string MOD_ID = "moreslugcats";

	public static Configurable<bool> cfgDisablePrecycles;

	public static Configurable<bool> cfgDisablePrecycleFloods;

	public static Configurable<bool> cfgArtificerCorpseMaxKarma;

	public static Configurable<bool> cfgArtificerCorpseNoKarmaLoss;

	public static Configurable<int> cfgArtificerExplosionCapacity;

	public static Configurable<bool> chtUnlockCampaigns;

	public static Configurable<bool> chtUnlockSafari;

	public static Configurable<bool> chtUnlockChallenges;

	public static Configurable<bool> chtUnlockArenas;

	public static Configurable<bool> chtUnlockCreatures;

	public static Configurable<bool> chtUnlockItems;

	public static Configurable<bool> chtUnlockClasses;

	public static Configurable<bool> chtUnlockCollections;

	public static Configurable<bool> chtUnlockSlugpups;

	public static Configurable<bool> chtUnlockOuterExpanse;

	public static Configurable<bool> chtUnlockDevCommentary;

	public static void OnInit()
	{
		OptionInterface optionInterface = new MoreSlugcatsOptionInterface();
		MachineConnector.SetRegisteredOI(MOD_ID, optionInterface);
		optionInterface.config.configurables.Clear();
		cfgDisablePrecycles = optionInterface.config.Bind("cfgDisablePrecycles", defaultValue: false, new ConfigurableInfo("Prevents the chance of waking up early before the rain has stopped", null, "", "Disable shelter failures"));
		cfgDisablePrecycleFloods = optionInterface.config.Bind("cfgDisablePrecycleFloods", defaultValue: false, new ConfigurableInfo("Prevents the chance of the region being flooded during a shelter failure scenario", null, "", "Disable precycle flooding"));
		cfgArtificerCorpseMaxKarma = optionInterface.config.Bind("cfgArtificerCorpseMaxKarma", defaultValue: false, new ConfigurableInfo("If enabled, all scavenger corpses in Artificer's campaign always have the maximum possible karma value", null, "", "Scavenger corpses have max karma"));
		cfgArtificerCorpseNoKarmaLoss = optionInterface.config.Bind("cfgArtificerCorpseNoKarmaLoss", defaultValue: false, new ConfigurableInfo("If enabled, scavenger corpses in Artificer's campaign don't lose karma after each cycle", null, "", "Lossless scavenger corpses"));
		cfgArtificerExplosionCapacity = optionInterface.config.Bind("cfgArtificerExplosionCapacity", 10, new ConfigurableInfo("The maximum number of subsequent explosion actions Artificer can perform before dying", new ConfigAcceptableRange<int>(1, 999), "", "Artificer Explosion Capacity"));
		chtUnlockCampaigns = optionInterface.config.Bind("chtUnlockCampaigns", defaultValue: false, new ConfigurableInfo("(Normally requires completing previous campaigns to unlock)", null, "", "Unlock all campaigns"));
		chtUnlockSafari = optionInterface.config.Bind("chtUnlockSafari", defaultValue: false, new ConfigurableInfo("(Normally requires finding Red unlock tokens in the campaign maps)", null, "", "Unlock all safari regions"));
		chtUnlockClasses = optionInterface.config.Bind("chtUnlockClasses", defaultValue: false, new ConfigurableInfo("(Normally requires finding Green unlock tokens in the campaign maps)", null, "", "Unlock all arena characters"));
		chtUnlockChallenges = optionInterface.config.Bind("chtUnlockChallenges", defaultValue: false, new ConfigurableInfo("(Normally requires completing campaigns and clearing previous challenges)", null, "", "Unlock all challenges"));
		chtUnlockArenas = optionInterface.config.Bind("chtUnlockArenas", defaultValue: false, new ConfigurableInfo("(Normally requires finding Orange unlock tokens in the campaign maps)", null, "", "Unlock all arenas"));
		chtUnlockCreatures = optionInterface.config.Bind("chtUnlockCreatures", defaultValue: false, new ConfigurableInfo("(Normally requires finding Blue unlock tokens in the campaign maps)", null, "", "Unlock all sandbox creatures"));
		chtUnlockItems = optionInterface.config.Bind("chtUnlockItems", defaultValue: false, new ConfigurableInfo("(Normally requires finding Blue unlock tokens in the campaign maps)", null, "", "Unlock all sandbox items"));
		chtUnlockCollections = optionInterface.config.Bind("chtUnlockCollections", defaultValue: false, new ConfigurableInfo("(Normally requires delivering pearls and finding broadcast tokens in the campaign maps)", null, "", "Unlock all collections"));
		chtUnlockSlugpups = optionInterface.config.Bind("chtUnlockSlugpups", defaultValue: false, new ConfigurableInfo("(Normally requires completing Gourmand's food tracker)", null, "", "Unlock Slugpups"));
		chtUnlockOuterExpanse = optionInterface.config.Bind("chtUnlockOuterExpanse", defaultValue: false, new ConfigurableInfo("(Normally requires completing Gourmand's campaign)", null, "", "Unlock the Outer Expanse gate"));
		chtUnlockDevCommentary = optionInterface.config.Bind("chtUnlockDevCommentary", defaultValue: false, new ConfigurableInfo("(Normally requires completing all Challenge Mode challenges)", null, "", "Unlock Developer Commentary mode"));
		if (!MultiplayerUnlocks.CreatureUnlockList.Contains(MoreSlugcatsEnums.SandboxUnlockID.SpitLizard))
		{
			MultiplayerUnlocks.CreatureUnlockList.Insert(MultiplayerUnlocks.CreatureUnlockList.IndexOf(MultiplayerUnlocks.SandboxUnlockID.CyanLizard), MoreSlugcatsEnums.SandboxUnlockID.SpitLizard);
		}
		if (!MultiplayerUnlocks.CreatureUnlockList.Contains(MoreSlugcatsEnums.SandboxUnlockID.ZoopLizard))
		{
			MultiplayerUnlocks.CreatureUnlockList.Insert(MultiplayerUnlocks.CreatureUnlockList.IndexOf(MultiplayerUnlocks.SandboxUnlockID.CyanLizard), MoreSlugcatsEnums.SandboxUnlockID.ZoopLizard);
		}
		if (!MultiplayerUnlocks.CreatureUnlockList.Contains(MoreSlugcatsEnums.SandboxUnlockID.EelLizard))
		{
			MultiplayerUnlocks.CreatureUnlockList.Insert(MultiplayerUnlocks.CreatureUnlockList.IndexOf(MultiplayerUnlocks.SandboxUnlockID.Fly), MoreSlugcatsEnums.SandboxUnlockID.EelLizard);
		}
		if (!MultiplayerUnlocks.CreatureUnlockList.Contains(MoreSlugcatsEnums.SandboxUnlockID.JungleLeech))
		{
			MultiplayerUnlocks.CreatureUnlockList.Insert(MultiplayerUnlocks.CreatureUnlockList.IndexOf(MultiplayerUnlocks.SandboxUnlockID.PoleMimic), MoreSlugcatsEnums.SandboxUnlockID.JungleLeech);
		}
		if (!MultiplayerUnlocks.CreatureUnlockList.Contains(MoreSlugcatsEnums.SandboxUnlockID.ScavengerElite))
		{
			MultiplayerUnlocks.CreatureUnlockList.Insert(MultiplayerUnlocks.CreatureUnlockList.IndexOf(MultiplayerUnlocks.SandboxUnlockID.VultureGrub), MoreSlugcatsEnums.SandboxUnlockID.ScavengerElite);
		}
		if (!MultiplayerUnlocks.CreatureUnlockList.Contains(MoreSlugcatsEnums.SandboxUnlockID.AquaCenti))
		{
			MultiplayerUnlocks.CreatureUnlockList.Insert(MultiplayerUnlocks.CreatureUnlockList.IndexOf(MultiplayerUnlocks.SandboxUnlockID.TubeWorm), MoreSlugcatsEnums.SandboxUnlockID.AquaCenti);
		}
		if (!MultiplayerUnlocks.CreatureUnlockList.Contains(MoreSlugcatsEnums.SandboxUnlockID.MotherSpider))
		{
			MultiplayerUnlocks.CreatureUnlockList.Insert(MultiplayerUnlocks.CreatureUnlockList.IndexOf(MultiplayerUnlocks.SandboxUnlockID.MirosBird), MoreSlugcatsEnums.SandboxUnlockID.MotherSpider);
		}
		if (!MultiplayerUnlocks.CreatureUnlockList.Contains(MoreSlugcatsEnums.SandboxUnlockID.MirosVulture))
		{
			MultiplayerUnlocks.CreatureUnlockList.Insert(MultiplayerUnlocks.CreatureUnlockList.IndexOf(MultiplayerUnlocks.SandboxUnlockID.BrotherLongLegs), MoreSlugcatsEnums.SandboxUnlockID.MirosVulture);
		}
		if (!MultiplayerUnlocks.CreatureUnlockList.Contains(MoreSlugcatsEnums.SandboxUnlockID.TerrorLongLegs))
		{
			MultiplayerUnlocks.CreatureUnlockList.Insert(MultiplayerUnlocks.CreatureUnlockList.IndexOf(MultiplayerUnlocks.SandboxUnlockID.Deer), MoreSlugcatsEnums.SandboxUnlockID.TerrorLongLegs);
		}
		if (!MultiplayerUnlocks.CreatureUnlockList.Contains(MoreSlugcatsEnums.SandboxUnlockID.Inspector))
		{
			MultiplayerUnlocks.CreatureUnlockList.Insert(MultiplayerUnlocks.CreatureUnlockList.IndexOf(MultiplayerUnlocks.SandboxUnlockID.Deer), MoreSlugcatsEnums.SandboxUnlockID.Inspector);
		}
		if (!MultiplayerUnlocks.CreatureUnlockList.Contains(MoreSlugcatsEnums.SandboxUnlockID.FireBug))
		{
			MultiplayerUnlocks.CreatureUnlockList.Insert(MultiplayerUnlocks.CreatureUnlockList.IndexOf(MultiplayerUnlocks.SandboxUnlockID.DropBug), MoreSlugcatsEnums.SandboxUnlockID.FireBug);
		}
		if (!MultiplayerUnlocks.CreatureUnlockList.Contains(MoreSlugcatsEnums.SandboxUnlockID.SlugNPC))
		{
			MultiplayerUnlocks.CreatureUnlockList.Insert(MultiplayerUnlocks.CreatureUnlockList.IndexOf(MultiplayerUnlocks.SandboxUnlockID.BigNeedleWorm), MoreSlugcatsEnums.SandboxUnlockID.SlugNPC);
		}
		if (!MultiplayerUnlocks.CreatureUnlockList.Contains(MoreSlugcatsEnums.SandboxUnlockID.Yeek))
		{
			MultiplayerUnlocks.CreatureUnlockList.Insert(MultiplayerUnlocks.CreatureUnlockList.IndexOf(MultiplayerUnlocks.SandboxUnlockID.BigEel), MoreSlugcatsEnums.SandboxUnlockID.Yeek);
		}
		if (!MultiplayerUnlocks.CreatureUnlockList.Contains(MoreSlugcatsEnums.SandboxUnlockID.BigJelly))
		{
			MultiplayerUnlocks.CreatureUnlockList.Add(MoreSlugcatsEnums.SandboxUnlockID.BigJelly);
		}
		if (!MultiplayerUnlocks.ItemUnlockList.Contains(MoreSlugcatsEnums.SandboxUnlockID.ElectricSpear))
		{
			MultiplayerUnlocks.ItemUnlockList.Insert(MultiplayerUnlocks.ItemUnlockList.IndexOf(MultiplayerUnlocks.SandboxUnlockID.ScavengerBomb), MoreSlugcatsEnums.SandboxUnlockID.ElectricSpear);
		}
		if (!MultiplayerUnlocks.ItemUnlockList.Contains(MoreSlugcatsEnums.SandboxUnlockID.LillyPuck))
		{
			MultiplayerUnlocks.ItemUnlockList.Insert(MultiplayerUnlocks.ItemUnlockList.IndexOf(MultiplayerUnlocks.SandboxUnlockID.ScavengerBomb), MoreSlugcatsEnums.SandboxUnlockID.LillyPuck);
		}
		if (!MultiplayerUnlocks.ItemUnlockList.Contains(MoreSlugcatsEnums.SandboxUnlockID.Pearl))
		{
			MultiplayerUnlocks.ItemUnlockList.Insert(MultiplayerUnlocks.ItemUnlockList.IndexOf(MultiplayerUnlocks.SandboxUnlockID.ScavengerBomb), MoreSlugcatsEnums.SandboxUnlockID.Pearl);
		}
		if (!MultiplayerUnlocks.ItemUnlockList.Contains(MoreSlugcatsEnums.SandboxUnlockID.SingularityBomb))
		{
			MultiplayerUnlocks.ItemUnlockList.Insert(MultiplayerUnlocks.ItemUnlockList.IndexOf(MultiplayerUnlocks.SandboxUnlockID.SporePlant), MoreSlugcatsEnums.SandboxUnlockID.SingularityBomb);
		}
		if (!MultiplayerUnlocks.ItemUnlockList.Contains(MoreSlugcatsEnums.SandboxUnlockID.FireEgg))
		{
			MultiplayerUnlocks.ItemUnlockList.Insert(MultiplayerUnlocks.ItemUnlockList.IndexOf(MultiplayerUnlocks.SandboxUnlockID.SporePlant), MoreSlugcatsEnums.SandboxUnlockID.FireEgg);
		}
		if (!MultiplayerUnlocks.ItemUnlockList.Contains(MoreSlugcatsEnums.SandboxUnlockID.HellSpear))
		{
			MultiplayerUnlocks.ItemUnlockList.Insert(MultiplayerUnlocks.ItemUnlockList.IndexOf(MoreSlugcatsEnums.SandboxUnlockID.LillyPuck), MoreSlugcatsEnums.SandboxUnlockID.HellSpear);
		}
		if (!MultiplayerUnlocks.ItemUnlockList.Contains(MoreSlugcatsEnums.SandboxUnlockID.VultureMask))
		{
			MultiplayerUnlocks.ItemUnlockList.Insert(MultiplayerUnlocks.ItemUnlockList.IndexOf(MultiplayerUnlocks.SandboxUnlockID.FlyLure), MoreSlugcatsEnums.SandboxUnlockID.VultureMask);
		}
		if (!MultiplayerUnlocks.ItemUnlockList.Contains(MoreSlugcatsEnums.SandboxUnlockID.GooieDuck))
		{
			MultiplayerUnlocks.ItemUnlockList.Insert(MultiplayerUnlocks.ItemUnlockList.IndexOf(MultiplayerUnlocks.SandboxUnlockID.WaterNut), MoreSlugcatsEnums.SandboxUnlockID.GooieDuck);
		}
		if (!MultiplayerUnlocks.ItemUnlockList.Contains(MoreSlugcatsEnums.SandboxUnlockID.DandelionPeach))
		{
			MultiplayerUnlocks.ItemUnlockList.Insert(MultiplayerUnlocks.ItemUnlockList.IndexOf(MultiplayerUnlocks.SandboxUnlockID.FirecrackerPlant), MoreSlugcatsEnums.SandboxUnlockID.DandelionPeach);
		}
		if (!MultiplayerUnlocks.ItemUnlockList.Contains(MoreSlugcatsEnums.SandboxUnlockID.GlowWeed))
		{
			MultiplayerUnlocks.ItemUnlockList.Insert(MultiplayerUnlocks.ItemUnlockList.IndexOf(MultiplayerUnlocks.SandboxUnlockID.SlimeMold), MoreSlugcatsEnums.SandboxUnlockID.GlowWeed);
		}
		if (!MultiplayerUnlocks.ItemUnlockList.Contains(MoreSlugcatsEnums.SandboxUnlockID.EnergyCell))
		{
			MultiplayerUnlocks.ItemUnlockList.Add(MoreSlugcatsEnums.SandboxUnlockID.EnergyCell);
		}
		if (!MultiplayerUnlocks.ItemUnlockList.Contains(MoreSlugcatsEnums.SandboxUnlockID.JokeRifle))
		{
			MultiplayerUnlocks.ItemUnlockList.Add(MoreSlugcatsEnums.SandboxUnlockID.JokeRifle);
		}
		List<CreatureTemplate.Type> list = new List<CreatureTemplate.Type>(WinState.lizardsOrder);
		if (!list.Contains(CreatureTemplate.Type.RedLizard))
		{
			list.Add(CreatureTemplate.Type.RedLizard);
		}
		if (!list.Contains(CreatureTemplate.Type.CyanLizard))
		{
			list.Add(CreatureTemplate.Type.CyanLizard);
		}
		if (!list.Contains(MoreSlugcatsEnums.CreatureTemplateType.SpitLizard))
		{
			list.Add(MoreSlugcatsEnums.CreatureTemplateType.SpitLizard);
		}
		if (!list.Contains(MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard))
		{
			list.Add(MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard);
		}
		WinState.lizardsOrder = list.ToArray();
		WinState.GourmandPassageTracker = new WinState.GourmandTrackerData[22]
		{
			new WinState.GourmandTrackerData(AbstractPhysicalObject.AbstractObjectType.SlimeMold, null),
			new WinState.GourmandTrackerData(AbstractPhysicalObject.AbstractObjectType.DangleFruit, null),
			new WinState.GourmandTrackerData(null, new CreatureTemplate.Type[1] { CreatureTemplate.Type.Fly }),
			new WinState.GourmandTrackerData(AbstractPhysicalObject.AbstractObjectType.Mushroom, null),
			new WinState.GourmandTrackerData(null, new CreatureTemplate.Type[1] { CreatureTemplate.Type.BlackLizard }),
			new WinState.GourmandTrackerData(AbstractPhysicalObject.AbstractObjectType.WaterNut, null),
			new WinState.GourmandTrackerData(AbstractPhysicalObject.AbstractObjectType.JellyFish, null),
			new WinState.GourmandTrackerData(null, new CreatureTemplate.Type[1] { CreatureTemplate.Type.JetFish }),
			new WinState.GourmandTrackerData(MoreSlugcatsEnums.AbstractObjectType.GlowWeed, null),
			new WinState.GourmandTrackerData(null, new CreatureTemplate.Type[2]
			{
				CreatureTemplate.Type.Salamander,
				MoreSlugcatsEnums.CreatureTemplateType.EelLizard
			}),
			new WinState.GourmandTrackerData(null, new CreatureTemplate.Type[1] { CreatureTemplate.Type.Snail }),
			new WinState.GourmandTrackerData(null, new CreatureTemplate.Type[1] { CreatureTemplate.Type.Hazer }),
			new WinState.GourmandTrackerData(null, new CreatureTemplate.Type[1] { CreatureTemplate.Type.EggBug }),
			new WinState.GourmandTrackerData(MoreSlugcatsEnums.AbstractObjectType.LillyPuck, null),
			new WinState.GourmandTrackerData(null, new CreatureTemplate.Type[1] { CreatureTemplate.Type.YellowLizard }),
			new WinState.GourmandTrackerData(null, new CreatureTemplate.Type[1] { CreatureTemplate.Type.TubeWorm }),
			new WinState.GourmandTrackerData(AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer, null),
			new WinState.GourmandTrackerData(null, new CreatureTemplate.Type[1] { CreatureTemplate.Type.Centiwing }),
			new WinState.GourmandTrackerData(MoreSlugcatsEnums.AbstractObjectType.DandelionPeach, null),
			new WinState.GourmandTrackerData(null, new CreatureTemplate.Type[1] { CreatureTemplate.Type.CyanLizard }),
			new WinState.GourmandTrackerData(MoreSlugcatsEnums.AbstractObjectType.GooieDuck, null),
			new WinState.GourmandTrackerData(null, new CreatureTemplate.Type[2]
			{
				CreatureTemplate.Type.RedCentipede,
				MoreSlugcatsEnums.CreatureTemplateType.AquaCenti
			})
		};
	}

	public static void OnDisable(ProcessManager manager)
	{
		MultiplayerUnlocks.CreatureUnlockList.Remove(MoreSlugcatsEnums.SandboxUnlockID.SpitLizard);
		MultiplayerUnlocks.CreatureUnlockList.Remove(MoreSlugcatsEnums.SandboxUnlockID.ZoopLizard);
		MultiplayerUnlocks.CreatureUnlockList.Remove(MoreSlugcatsEnums.SandboxUnlockID.EelLizard);
		MultiplayerUnlocks.CreatureUnlockList.Remove(MoreSlugcatsEnums.SandboxUnlockID.JungleLeech);
		MultiplayerUnlocks.CreatureUnlockList.Remove(MoreSlugcatsEnums.SandboxUnlockID.ScavengerElite);
		MultiplayerUnlocks.CreatureUnlockList.Remove(MoreSlugcatsEnums.SandboxUnlockID.AquaCenti);
		MultiplayerUnlocks.CreatureUnlockList.Remove(MoreSlugcatsEnums.SandboxUnlockID.MotherSpider);
		MultiplayerUnlocks.CreatureUnlockList.Remove(MoreSlugcatsEnums.SandboxUnlockID.MirosVulture);
		MultiplayerUnlocks.CreatureUnlockList.Remove(MoreSlugcatsEnums.SandboxUnlockID.TerrorLongLegs);
		MultiplayerUnlocks.CreatureUnlockList.Remove(MoreSlugcatsEnums.SandboxUnlockID.Inspector);
		MultiplayerUnlocks.CreatureUnlockList.Remove(MoreSlugcatsEnums.SandboxUnlockID.FireBug);
		MultiplayerUnlocks.CreatureUnlockList.Remove(MoreSlugcatsEnums.SandboxUnlockID.SlugNPC);
		MultiplayerUnlocks.CreatureUnlockList.Remove(MoreSlugcatsEnums.SandboxUnlockID.Yeek);
		MultiplayerUnlocks.CreatureUnlockList.Remove(MoreSlugcatsEnums.SandboxUnlockID.BigJelly);
		MultiplayerUnlocks.ItemUnlockList.Remove(MoreSlugcatsEnums.SandboxUnlockID.ElectricSpear);
		MultiplayerUnlocks.ItemUnlockList.Remove(MoreSlugcatsEnums.SandboxUnlockID.LillyPuck);
		MultiplayerUnlocks.ItemUnlockList.Remove(MoreSlugcatsEnums.SandboxUnlockID.Pearl);
		MultiplayerUnlocks.ItemUnlockList.Remove(MoreSlugcatsEnums.SandboxUnlockID.SingularityBomb);
		MultiplayerUnlocks.ItemUnlockList.Remove(MoreSlugcatsEnums.SandboxUnlockID.FireEgg);
		MultiplayerUnlocks.ItemUnlockList.Remove(MoreSlugcatsEnums.SandboxUnlockID.HellSpear);
		MultiplayerUnlocks.ItemUnlockList.Remove(MoreSlugcatsEnums.SandboxUnlockID.VultureMask);
		MultiplayerUnlocks.ItemUnlockList.Remove(MoreSlugcatsEnums.SandboxUnlockID.GooieDuck);
		MultiplayerUnlocks.ItemUnlockList.Remove(MoreSlugcatsEnums.SandboxUnlockID.DandelionPeach);
		MultiplayerUnlocks.ItemUnlockList.Remove(MoreSlugcatsEnums.SandboxUnlockID.GlowWeed);
		MultiplayerUnlocks.ItemUnlockList.Remove(MoreSlugcatsEnums.SandboxUnlockID.EnergyCell);
		MultiplayerUnlocks.ItemUnlockList.Remove(MoreSlugcatsEnums.SandboxUnlockID.JokeRifle);
		List<CreatureTemplate.Type> list = new List<CreatureTemplate.Type>(WinState.lizardsOrder);
		if (list.Contains(CreatureTemplate.Type.RedLizard))
		{
			list.Remove(CreatureTemplate.Type.RedLizard);
		}
		if (list.Contains(CreatureTemplate.Type.CyanLizard))
		{
			list.Remove(CreatureTemplate.Type.CyanLizard);
		}
		if (list.Contains(MoreSlugcatsEnums.CreatureTemplateType.SpitLizard))
		{
			list.Remove(MoreSlugcatsEnums.CreatureTemplateType.SpitLizard);
		}
		if (list.Contains(MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard))
		{
			list.Remove(MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard);
		}
		WinState.lizardsOrder = list.ToArray();
		WinState.GourmandPassageTracker = new WinState.GourmandTrackerData[0];
	}
}
