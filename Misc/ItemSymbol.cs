using Menu;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class ItemSymbol : IconSymbol
{
	public AbstractPhysicalObject.AbstractObjectType itemType => iconData.itemType;

	public ItemSymbol(IconSymbolData iconData, FContainer container)
		: base(iconData, container)
	{
		myColor = ColorForItem(itemType, iconData.intData);
		spriteName = SpriteNameForItem(itemType, iconData.intData);
		graphWidth = Futile.atlasManager.GetElementWithName(spriteName).sourcePixelSize.x;
	}

	public static string SpriteNameForItem(AbstractPhysicalObject.AbstractObjectType itemType, int intData)
	{
		if (itemType == AbstractPhysicalObject.AbstractObjectType.Rock)
		{
			return "Symbol_Rock";
		}
		if (itemType == AbstractPhysicalObject.AbstractObjectType.Spear)
		{
			if (ModManager.MSC && intData == 3)
			{
				return "Symbol_HellSpear";
			}
			if (ModManager.MSC && intData == 2)
			{
				return "Symbol_ElectricSpear";
			}
			if (intData == 1)
			{
				return "Symbol_FireSpear";
			}
			return "Symbol_Spear";
		}
		if (itemType == AbstractPhysicalObject.AbstractObjectType.ScavengerBomb)
		{
			return "Symbol_StunBomb";
		}
		if (itemType == AbstractPhysicalObject.AbstractObjectType.SporePlant)
		{
			return "Symbol_SporePlant";
		}
		if (itemType == AbstractPhysicalObject.AbstractObjectType.Lantern)
		{
			return "Symbol_Lantern";
		}
		if (itemType == AbstractPhysicalObject.AbstractObjectType.FlareBomb)
		{
			return "Symbol_FlashBomb";
		}
		if (itemType == AbstractPhysicalObject.AbstractObjectType.PuffBall)
		{
			return "Symbol_PuffBall";
		}
		if (itemType == AbstractPhysicalObject.AbstractObjectType.WaterNut)
		{
			return "Symbol_WaterNut";
		}
		if (itemType == AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant)
		{
			return "Symbol_Firecracker";
		}
		if (itemType == AbstractPhysicalObject.AbstractObjectType.DangleFruit)
		{
			return "Symbol_DangleFruit";
		}
		if (itemType == AbstractPhysicalObject.AbstractObjectType.BubbleGrass)
		{
			return "Symbol_BubbleGrass";
		}
		if (itemType == AbstractPhysicalObject.AbstractObjectType.SlimeMold)
		{
			return "Symbol_SlimeMold";
		}
		if (itemType == AbstractPhysicalObject.AbstractObjectType.Mushroom)
		{
			return "Symbol_Mushroom";
		}
		if (itemType == AbstractPhysicalObject.AbstractObjectType.JellyFish)
		{
			return "Symbol_JellyFish";
		}
		if (itemType == AbstractPhysicalObject.AbstractObjectType.VultureMask)
		{
			if (ModManager.MSC && intData == 2)
			{
				return "Symbol_ChieftainMask";
			}
			if (intData != 1)
			{
				return "Kill_Vulture";
			}
			return "Kill_KingVulture";
		}
		if (itemType == AbstractPhysicalObject.AbstractObjectType.FlyLure)
		{
			return "Symbol_FlyLure";
		}
		if (itemType == AbstractPhysicalObject.AbstractObjectType.SLOracleSwarmer || itemType == AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer || itemType == AbstractPhysicalObject.AbstractObjectType.NSHSwarmer)
		{
			return "Symbol_Neuron";
		}
		if (itemType == AbstractPhysicalObject.AbstractObjectType.EggBugEgg)
		{
			return "Symbol_EggBugEgg";
		}
		if (itemType == AbstractPhysicalObject.AbstractObjectType.OverseerCarcass)
		{
			return "Kill_Overseer";
		}
		if (itemType == AbstractPhysicalObject.AbstractObjectType.DataPearl || itemType == AbstractPhysicalObject.AbstractObjectType.PebblesPearl)
		{
			return "Symbol_Pearl";
		}
		if (itemType == AbstractPhysicalObject.AbstractObjectType.NeedleEgg)
		{
			return "needleEggSymbol";
		}
		if (ModManager.MSC && (itemType == MoreSlugcatsEnums.AbstractObjectType.Spearmasterpearl || itemType == MoreSlugcatsEnums.AbstractObjectType.HalcyonPearl))
		{
			return "Symbol_Pearl";
		}
		if (ModManager.MSC && itemType == MoreSlugcatsEnums.AbstractObjectType.EnergyCell)
		{
			return "Symbol_EnergyCell";
		}
		if (ModManager.MSC && itemType == MoreSlugcatsEnums.AbstractObjectType.GooieDuck)
		{
			return "Symbol_GooieDuck";
		}
		if (ModManager.MSC && itemType == MoreSlugcatsEnums.AbstractObjectType.GlowWeed)
		{
			return "Symbol_GlowWeed";
		}
		if (ModManager.MSC && itemType == MoreSlugcatsEnums.AbstractObjectType.LillyPuck)
		{
			return "Symbol_LillyPuck";
		}
		if (ModManager.MSC && itemType == MoreSlugcatsEnums.AbstractObjectType.DandelionPeach)
		{
			return "Symbol_DandelionPeach";
		}
		if (ModManager.MSC && itemType == MoreSlugcatsEnums.AbstractObjectType.MoonCloak)
		{
			return "Symbol_MoonCloak";
		}
		if (ModManager.MSC && itemType == MoreSlugcatsEnums.AbstractObjectType.FireEgg)
		{
			return "Symbol_FireEgg";
		}
		if (ModManager.MSC && itemType == MoreSlugcatsEnums.AbstractObjectType.JokeRifle)
		{
			return "Symbol_JokeRifle";
		}
		if (ModManager.MSC && itemType == MoreSlugcatsEnums.AbstractObjectType.Seed)
		{
			return "Symbol_Seed";
		}
		if (ModManager.MSC && itemType == MoreSlugcatsEnums.AbstractObjectType.SingularityBomb)
		{
			return "Symbol_Singularity";
		}
		return "Futile_White";
	}

	public static Color ColorForItem(AbstractPhysicalObject.AbstractObjectType itemType, int intData)
	{
		if (itemType == AbstractPhysicalObject.AbstractObjectType.SporePlant || itemType == AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant)
		{
			return new Color(58f / 85f, 8f / 51f, 0.11764706f);
		}
		if (itemType == AbstractPhysicalObject.AbstractObjectType.ScavengerBomb)
		{
			return new Color(46f / 51f, 0.05490196f, 0.05490196f);
		}
		if (itemType == AbstractPhysicalObject.AbstractObjectType.Spear)
		{
			if (intData == 1)
			{
				return new Color(46f / 51f, 0.05490196f, 0.05490196f);
			}
			if (ModManager.MSC && intData == 2)
			{
				return new Color(0f, 0f, 1f);
			}
			if (ModManager.MSC && intData == 3)
			{
				return new Color(1f, 0.47058824f, 0.47058824f);
			}
		}
		else
		{
			if (itemType == AbstractPhysicalObject.AbstractObjectType.Lantern)
			{
				return new Color(1f, 0.57254905f, 27f / 85f);
			}
			if (itemType == AbstractPhysicalObject.AbstractObjectType.FlareBomb)
			{
				return new Color(11f / 15f, 58f / 85f, 1f);
			}
			if (itemType == AbstractPhysicalObject.AbstractObjectType.SlimeMold)
			{
				return new Color(1f, 0.6f, 0f);
			}
			if (itemType == AbstractPhysicalObject.AbstractObjectType.BubbleGrass)
			{
				return new Color(0.05490196f, 0.69803923f, 0.23529412f);
			}
			if (itemType == AbstractPhysicalObject.AbstractObjectType.DangleFruit)
			{
				return new Color(0f, 0f, 1f);
			}
			if (itemType == AbstractPhysicalObject.AbstractObjectType.Mushroom)
			{
				return Color.white;
			}
			if (itemType == AbstractPhysicalObject.AbstractObjectType.WaterNut)
			{
				return new Color(0.05f, 0.3f, 0.7f);
			}
			if (itemType == AbstractPhysicalObject.AbstractObjectType.EggBugEgg)
			{
				return new Color(0f, 1f, 0.47058824f);
			}
			if (itemType == AbstractPhysicalObject.AbstractObjectType.FlyLure)
			{
				return new Color(0.6784314f, 4f / 15f, 18f / 85f);
			}
			if (itemType == AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer)
			{
				return Color.white;
			}
			if (itemType == AbstractPhysicalObject.AbstractObjectType.NSHSwarmer)
			{
				return new Color(0f, 1f, 0.3f);
			}
			if (itemType == AbstractPhysicalObject.AbstractObjectType.NeedleEgg)
			{
				return new Color(49f / 85f, 0.16078432f, 0.2509804f);
			}
			if (itemType == AbstractPhysicalObject.AbstractObjectType.PebblesPearl)
			{
				if (intData != 1)
				{
					if (intData == 2)
					{
						return global::Menu.Menu.MenuRGB(global::Menu.Menu.MenuColors.DarkGrey);
					}
					if (intData < 0)
					{
						return new Color(0f, 0.45490196f, 0.6392157f);
					}
					return new Color(1f, 0.47843137f, 0.007843138f);
				}
				return new Color(0.7f, 0.7f, 0.7f);
			}
			if (itemType == AbstractPhysicalObject.AbstractObjectType.DataPearl || (ModManager.MSC && itemType == MoreSlugcatsEnums.AbstractObjectType.HalcyonPearl))
			{
				if (intData > 1 && intData < ExtEnum<DataPearl.AbstractDataPearl.DataPearlType>.values.Count)
				{
					Color a = DataPearl.UniquePearlMainColor(new DataPearl.AbstractDataPearl.DataPearlType(ExtEnum<DataPearl.AbstractDataPearl.DataPearlType>.values.GetEntry(intData)));
					Color? color = DataPearl.UniquePearlHighLightColor(new DataPearl.AbstractDataPearl.DataPearlType(ExtEnum<DataPearl.AbstractDataPearl.DataPearlType>.values.GetEntry(intData)));
					a = ((!color.HasValue) ? Color.Lerp(a, Color.white, 0.15f) : Custom.Screen(a, color.Value * Custom.QuickSaturation(color.Value) * 0.5f));
					if (a.r < 0.1f && a.g < 0.1f && a.b < 0.1f)
					{
						a = Color.Lerp(a, global::Menu.Menu.MenuRGB(global::Menu.Menu.MenuColors.MediumGrey), 0.3f);
					}
					return a;
				}
				if (intData == 1)
				{
					return new Color(1f, 0.6f, 0.9f);
				}
				return new Color(0.7f, 0.7f, 0.7f);
			}
			if (ModManager.MSC && itemType == MoreSlugcatsEnums.AbstractObjectType.Spearmasterpearl)
			{
				return Color.Lerp(new Color(0.45f, 0.01f, 0.04f), Color.white, 0.15f);
			}
			if (ModManager.MSC && itemType == MoreSlugcatsEnums.AbstractObjectType.EnergyCell)
			{
				return new Color(0.01961f, 0.6451f, 0.85f);
			}
			if (ModManager.MSC && itemType == MoreSlugcatsEnums.AbstractObjectType.SingularityBomb)
			{
				return new Color(0.01961f, 0.6451f, 0.85f);
			}
			if (ModManager.MSC && itemType == MoreSlugcatsEnums.AbstractObjectType.GooieDuck)
			{
				return new Color(38f / 85f, 46f / 51f, 0.76862746f);
			}
			if (ModManager.MSC && itemType == MoreSlugcatsEnums.AbstractObjectType.LillyPuck)
			{
				return new Color(0.17058827f, 0.9619608f, 0.9986275f);
			}
			if (ModManager.MSC && itemType == MoreSlugcatsEnums.AbstractObjectType.GlowWeed)
			{
				return new Color(0.94705886f, 1f, 0.26862746f);
			}
			if (ModManager.MSC && itemType == MoreSlugcatsEnums.AbstractObjectType.DandelionPeach)
			{
				return new Color(0.59f, 0.78f, 0.96f);
			}
			if (ModManager.MSC && itemType == MoreSlugcatsEnums.AbstractObjectType.MoonCloak)
			{
				return new Color(0.95f, 1f, 0.96f);
			}
			if (ModManager.MSC && itemType == MoreSlugcatsEnums.AbstractObjectType.FireEgg)
			{
				return new Color(1f, 0.47058824f, 0.47058824f);
			}
		}
		return global::Menu.Menu.MenuRGB(global::Menu.Menu.MenuColors.MediumGrey);
	}

	public static IconSymbolData? SymbolDataFromItem(AbstractPhysicalObject item)
	{
		if (item.type == AbstractPhysicalObject.AbstractObjectType.Spear)
		{
			if ((item as AbstractSpear).stuckInWall)
			{
				return null;
			}
			int intData = 0;
			if (ModManager.MSC && (item as AbstractSpear).hue != 0f)
			{
				intData = 3;
			}
			else if (ModManager.MSC && (item as AbstractSpear).electric)
			{
				intData = 2;
			}
			else if ((item as AbstractSpear).explosive)
			{
				intData = 1;
			}
			return new IconSymbolData(CreatureTemplate.Type.StandardGroundCreature, item.type, intData);
		}
		if (item.type == AbstractPhysicalObject.AbstractObjectType.VultureMask)
		{
			int intData2 = 0;
			if (ModManager.MSC && (item as VultureMask.AbstractVultureMask).scavKing)
			{
				intData2 = 2;
			}
			else if ((item as VultureMask.AbstractVultureMask).king)
			{
				intData2 = 1;
			}
			return new IconSymbolData(CreatureTemplate.Type.StandardGroundCreature, item.type, intData2);
		}
		if (item.type == AbstractPhysicalObject.AbstractObjectType.PebblesPearl)
		{
			return new IconSymbolData(CreatureTemplate.Type.StandardGroundCreature, item.type, (item as PebblesPearl.AbstractPebblesPearl).color);
		}
		if (item.type == AbstractPhysicalObject.AbstractObjectType.DataPearl)
		{
			return new IconSymbolData(CreatureTemplate.Type.StandardGroundCreature, item.type, (int)(item as DataPearl.AbstractDataPearl).dataPearlType);
		}
		if (item.type == AbstractPhysicalObject.AbstractObjectType.AttachedBee || item.type == AbstractPhysicalObject.AbstractObjectType.Creature || item.type == AbstractPhysicalObject.AbstractObjectType.DartMaggot || item.type == AbstractPhysicalObject.AbstractObjectType.KarmaFlower || item.type == AbstractPhysicalObject.AbstractObjectType.Oracle || item.type == AbstractPhysicalObject.AbstractObjectType.SeedCob || item.type == AbstractPhysicalObject.AbstractObjectType.VoidSpawn)
		{
			return null;
		}
		return new IconSymbolData(CreatureTemplate.Type.StandardGroundCreature, item.type, 0);
	}
}
