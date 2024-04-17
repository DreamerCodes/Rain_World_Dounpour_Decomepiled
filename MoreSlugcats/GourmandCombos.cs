using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public static class GourmandCombos
{
	public struct CraftDat
	{
		public AbstractPhysicalObject.AbstractObjectType type;

		public CreatureTemplate.Type crit;

		public bool enabled;

		public CraftDat(AbstractPhysicalObject.AbstractObjectType typeResult, CreatureTemplate.Type critResult)
		{
			enabled = true;
			type = AbstractPhysicalObject.AbstractObjectType.Creature;
			crit = CreatureTemplate.Type.StandardGroundCreature;
			if (critResult != null)
			{
				crit = critResult;
			}
			else if (typeResult != null)
			{
				type = typeResult;
			}
			else
			{
				enabled = false;
			}
		}
	}

	public static Dictionary<AbstractPhysicalObject.AbstractObjectType, int> objectsLibrary;

	public static Dictionary<CreatureTemplate.Type, int> critsLibrary;

	public static CraftDat[,] craftingGrid_ObjectsOnly;

	public static CraftDat[,] craftingGrid_CritterObjects;

	public static CraftDat[,] craftingGrid_CrittersOnly;

	public static bool showDebug;

	static GourmandCombos()
	{
		int num = 0;
		objectsLibrary = new Dictionary<AbstractPhysicalObject.AbstractObjectType, int>();
		objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Rock] = num;
		num++;
		objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FlareBomb] = num;
		num++;
		objectsLibrary[AbstractPhysicalObject.AbstractObjectType.VultureMask] = num;
		num++;
		objectsLibrary[AbstractPhysicalObject.AbstractObjectType.PuffBall] = num;
		num++;
		objectsLibrary[AbstractPhysicalObject.AbstractObjectType.DangleFruit] = num;
		num++;
		objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SLOracleSwarmer] = num;
		num++;
		objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer] = num;
		num++;
		objectsLibrary[AbstractPhysicalObject.AbstractObjectType.DataPearl] = num;
		num++;
		objectsLibrary[AbstractPhysicalObject.AbstractObjectType.WaterNut] = num;
		num++;
		objectsLibrary[AbstractPhysicalObject.AbstractObjectType.JellyFish] = num;
		num++;
		objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Lantern] = num;
		num++;
		objectsLibrary[AbstractPhysicalObject.AbstractObjectType.KarmaFlower] = num;
		num++;
		objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Mushroom] = num;
		num++;
		objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant] = num;
		num++;
		objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SlimeMold] = num;
		num++;
		objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FlyLure] = num;
		num++;
		objectsLibrary[AbstractPhysicalObject.AbstractObjectType.ScavengerBomb] = num;
		num++;
		objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SporePlant] = num;
		num++;
		objectsLibrary[AbstractPhysicalObject.AbstractObjectType.EggBugEgg] = num;
		num++;
		objectsLibrary[AbstractPhysicalObject.AbstractObjectType.NeedleEgg] = num;
		num++;
		objectsLibrary[AbstractPhysicalObject.AbstractObjectType.BubbleGrass] = num;
		num++;
		objectsLibrary[AbstractPhysicalObject.AbstractObjectType.OverseerCarcass] = num;
		num++;
		objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.SingularityBomb] = num;
		num++;
		objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.FireEgg] = num;
		num++;
		objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.Seed] = num;
		num++;
		objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GooieDuck] = num;
		num++;
		objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.LillyPuck] = num;
		num++;
		objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GlowWeed] = num;
		num++;
		objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.DandelionPeach] = num;
		num++;
		int num2 = 0;
		critsLibrary = new Dictionary<CreatureTemplate.Type, int>();
		critsLibrary[CreatureTemplate.Type.VultureGrub] = num2;
		num2++;
		critsLibrary[CreatureTemplate.Type.SmallCentipede] = num2;
		num2++;
		critsLibrary[CreatureTemplate.Type.SmallNeedleWorm] = num2;
		num2++;
		critsLibrary[CreatureTemplate.Type.Hazer] = num2;
		num2++;
		critsLibrary[CreatureTemplate.Type.Fly] = num2;
		num2++;
		craftingGrid_ObjectsOnly = new CraftDat[num, num];
		craftingGrid_CritterObjects = new CraftDat[num2, num];
		craftingGrid_CrittersOnly = new CraftDat[num2, num2];
		InitCraftingLibrary();
	}

	public static CraftDat GetLibraryData(AbstractPhysicalObject.AbstractObjectType objectA, AbstractPhysicalObject.AbstractObjectType objectB)
	{
		if (objectsLibrary.ContainsKey(objectA) && objectsLibrary.ContainsKey(objectB))
		{
			int num = objectsLibrary[objectA];
			int num2 = objectsLibrary[objectB];
			return craftingGrid_ObjectsOnly[num, num2];
		}
		return new CraftDat(null, null);
	}

	public static CraftDat GetLibraryData(CreatureTemplate.Type critterA, AbstractPhysicalObject.AbstractObjectType objectB)
	{
		if (critsLibrary.ContainsKey(critterA) && objectsLibrary.ContainsKey(objectB))
		{
			int num = critsLibrary[critterA];
			int num2 = objectsLibrary[objectB];
			return craftingGrid_CritterObjects[num, num2];
		}
		return new CraftDat(null, null);
	}

	public static CraftDat GetLibraryData(CreatureTemplate.Type critterA, CreatureTemplate.Type critterB)
	{
		if (critsLibrary.ContainsKey(critterA) && critsLibrary.ContainsKey(critterB))
		{
			int num = critsLibrary[critterA];
			int num2 = critsLibrary[critterB];
			return craftingGrid_CrittersOnly[num, num2];
		}
		return new CraftDat(null, null);
	}

	public static AbstractPhysicalObject.AbstractObjectType CraftingResults_ObjectData(Creature.Grasp graspA, Creature.Grasp graspB, bool canMakeMeals)
	{
		if (graspA == null || graspB == null)
		{
			return null;
		}
		if (graspA.grabbed is IPlayerEdible && !(graspA.grabbed as IPlayerEdible).Edible)
		{
			return null;
		}
		if (graspB.grabbed is IPlayerEdible && !(graspB.grabbed as IPlayerEdible).Edible)
		{
			return null;
		}
		AbstractPhysicalObject.AbstractObjectType result = null;
		CraftDat filteredLibraryData = GetFilteredLibraryData(graspA, graspB);
		if (filteredLibraryData.enabled)
		{
			result = filteredLibraryData.type;
		}
		return result;
	}

	public static CreatureTemplate.Type CraftingResults_CreatureData(Creature.Grasp graspA, Creature.Grasp graspB)
	{
		if (graspA.grabbed is Creature && graspB.grabbed is Creature)
		{
			return GetLibraryData((graspA.grabbed as Creature).abstractCreature.creatureTemplate.type, (graspB.grabbed as Creature).abstractCreature.creatureTemplate.type).crit;
		}
		if (graspA.grabbed is Creature)
		{
			return GetLibraryData((graspA.grabbed as Creature).abstractCreature.creatureTemplate.type, graspB.grabbed.abstractPhysicalObject.type).crit;
		}
		if (graspB.grabbed is Creature)
		{
			return GetLibraryData((graspB.grabbed as Creature).abstractCreature.creatureTemplate.type, graspA.grabbed.abstractPhysicalObject.type).crit;
		}
		return GetLibraryData(graspA.grabbed.abstractPhysicalObject.type, graspB.grabbed.abstractPhysicalObject.type).crit;
	}

	public static AbstractPhysicalObject CraftingResults(PhysicalObject crafter, Creature.Grasp graspA, Creature.Grasp graspB)
	{
		AbstractPhysicalObject.AbstractObjectType abstractObjectType = CraftingResults_ObjectData(graspA, graspB, canMakeMeals: true);
		Custom.Log("CRAFTING INPUT", graspA.grabbed.abstractPhysicalObject.type.ToString(), "+", graspB.grabbed.abstractPhysicalObject.type.ToString());
		if (abstractObjectType == null)
		{
			return new AbstractPhysicalObject(crafter.room.world, AbstractPhysicalObject.AbstractObjectType.Rock, null, crafter.abstractPhysicalObject.pos, crafter.room.game.GetNewID());
		}
		Custom.Log($"CRAFTING RESULTS {abstractObjectType}");
		if (abstractObjectType == AbstractPhysicalObject.AbstractObjectType.Creature)
		{
			CreatureTemplate.Type type = CraftingResults_CreatureData(graspA, graspB);
			return new AbstractCreature(crafter.room.world, StaticWorld.GetCreatureTemplate(type), null, crafter.abstractPhysicalObject.pos, crafter.room.game.GetNewID());
		}
		if (abstractObjectType == AbstractPhysicalObject.AbstractObjectType.DataPearl)
		{
			return new DataPearl.AbstractDataPearl(crafter.room.world, AbstractPhysicalObject.AbstractObjectType.DataPearl, null, crafter.abstractPhysicalObject.pos, crafter.room.game.GetNewID(), -1, -1, null, DataPearl.AbstractDataPearl.DataPearlType.Misc);
		}
		if (abstractObjectType == AbstractPhysicalObject.AbstractObjectType.OverseerCarcass)
		{
			return new OverseerCarcass.AbstractOverseerCarcass(crafter.room.world, null, crafter.abstractPhysicalObject.pos, crafter.room.game.GetNewID(), Color.white, 0);
		}
		if (abstractObjectType == AbstractPhysicalObject.AbstractObjectType.SporePlant)
		{
			return new SporePlant.AbstractSporePlant(crafter.room.world, null, crafter.abstractPhysicalObject.pos, crafter.room.game.GetNewID(), -1, -1, null, used: false, pacified: true)
			{
				isFresh = false,
				isConsumed = true
			};
		}
		if (abstractObjectType == AbstractPhysicalObject.AbstractObjectType.VultureMask)
		{
			return new VultureMask.AbstractVultureMask(crafter.room.world, null, crafter.abstractPhysicalObject.pos, crafter.room.game.GetNewID(), Random.Range(0, 4000), king: false);
		}
		if (abstractObjectType == AbstractPhysicalObject.AbstractObjectType.WaterNut)
		{
			return new WaterNut.AbstractWaterNut(crafter.room.world, null, crafter.abstractPhysicalObject.pos, crafter.room.game.GetNewID(), -1, -1, null, swollen: true)
			{
				isFresh = false,
				isConsumed = true
			};
		}
		if (abstractObjectType == AbstractPhysicalObject.AbstractObjectType.BubbleGrass)
		{
			return new BubbleGrass.AbstractBubbleGrass(crafter.room.world, null, crafter.abstractPhysicalObject.pos, crafter.room.game.GetNewID(), 1f, -1, -1, null)
			{
				isFresh = false,
				isConsumed = true
			};
		}
		if (abstractObjectType == MoreSlugcatsEnums.AbstractObjectType.LillyPuck)
		{
			return new LillyPuck.AbstractLillyPuck(crafter.room.world, null, crafter.abstractPhysicalObject.pos, crafter.room.game.GetNewID(), 3, -1, -1, null)
			{
				isFresh = false,
				isConsumed = true
			};
		}
		if (abstractObjectType == MoreSlugcatsEnums.AbstractObjectType.FireEgg)
		{
			return new FireEgg.AbstractBugEgg(crafter.room.world, null, crafter.abstractPhysicalObject.pos, crafter.room.game.GetNewID(), Random.value);
		}
		if (AbstractConsumable.IsTypeConsumable(abstractObjectType))
		{
			return new AbstractConsumable(crafter.room.world, abstractObjectType, null, crafter.abstractPhysicalObject.pos, crafter.room.game.GetNewID(), -1, -1, null)
			{
				isFresh = false,
				isConsumed = true
			};
		}
		return new AbstractPhysicalObject(crafter.room.world, abstractObjectType, null, crafter.abstractPhysicalObject.pos, crafter.room.game.GetNewID());
	}

	public static void InitCraftingLibrary()
	{
		int tableSelect = 0;
		AbstractPhysicalObject.AbstractObjectType rock = AbstractPhysicalObject.AbstractObjectType.Rock;
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Rock], tableSelect, null, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FlareBomb], tableSelect, AbstractPhysicalObject.AbstractObjectType.Lantern, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.VultureMask], tableSelect, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.PuffBall], tableSelect, AbstractPhysicalObject.AbstractObjectType.SporePlant, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.DangleFruit], tableSelect, AbstractPhysicalObject.AbstractObjectType.FlareBomb, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer], tableSelect, AbstractPhysicalObject.AbstractObjectType.OverseerCarcass, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.DataPearl], tableSelect, null, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.WaterNut], tableSelect, AbstractPhysicalObject.AbstractObjectType.JellyFish, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.JellyFish], tableSelect, MoreSlugcatsEnums.AbstractObjectType.LillyPuck, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Lantern], tableSelect, AbstractPhysicalObject.AbstractObjectType.FlareBomb, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.KarmaFlower], tableSelect, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Mushroom], tableSelect, AbstractPhysicalObject.AbstractObjectType.PuffBall, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant], tableSelect, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SlimeMold], tableSelect, AbstractPhysicalObject.AbstractObjectType.Lantern, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FlyLure], tableSelect, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.ScavengerBomb], tableSelect, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SporePlant], tableSelect, AbstractPhysicalObject.AbstractObjectType.PuffBall, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.EggBugEgg], tableSelect, AbstractPhysicalObject.AbstractObjectType.NeedleEgg, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.NeedleEgg], tableSelect, null, CreatureTemplate.Type.Fly);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.BubbleGrass], tableSelect, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.OverseerCarcass], tableSelect, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.SingularityBomb], tableSelect, MoreSlugcatsEnums.AbstractObjectType.FireEgg, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.FireEgg], tableSelect, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.Seed], tableSelect, null, CreatureTemplate.Type.Fly);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GooieDuck], tableSelect, AbstractPhysicalObject.AbstractObjectType.SporePlant, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.LillyPuck], tableSelect, AbstractPhysicalObject.AbstractObjectType.Lantern, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GlowWeed], tableSelect, AbstractPhysicalObject.AbstractObjectType.Lantern, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.DandelionPeach], tableSelect, AbstractPhysicalObject.AbstractObjectType.PuffBall, null);
		rock = AbstractPhysicalObject.AbstractObjectType.FlareBomb;
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FlareBomb], tableSelect, null, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.VultureMask], tableSelect, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.PuffBall], tableSelect, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.DangleFruit], tableSelect, AbstractPhysicalObject.AbstractObjectType.SlimeMold, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer], tableSelect, AbstractPhysicalObject.AbstractObjectType.OverseerCarcass, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.DataPearl], tableSelect, null, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.WaterNut], tableSelect, AbstractPhysicalObject.AbstractObjectType.JellyFish, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.JellyFish], tableSelect, MoreSlugcatsEnums.AbstractObjectType.LillyPuck, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Lantern], tableSelect, MoreSlugcatsEnums.AbstractObjectType.GlowWeed, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.KarmaFlower], tableSelect, AbstractPhysicalObject.AbstractObjectType.Lantern, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Mushroom], tableSelect, AbstractPhysicalObject.AbstractObjectType.PuffBall, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant], tableSelect, AbstractPhysicalObject.AbstractObjectType.Lantern, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SlimeMold], tableSelect, AbstractPhysicalObject.AbstractObjectType.Lantern, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FlyLure], tableSelect, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.ScavengerBomb], tableSelect, AbstractPhysicalObject.AbstractObjectType.Lantern, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SporePlant], tableSelect, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.EggBugEgg], tableSelect, AbstractPhysicalObject.AbstractObjectType.SporePlant, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.NeedleEgg], tableSelect, null, CreatureTemplate.Type.VultureGrub);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.BubbleGrass], tableSelect, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.OverseerCarcass], tableSelect, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.SingularityBomb], tableSelect, MoreSlugcatsEnums.AbstractObjectType.FireEgg, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.FireEgg], tableSelect, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.Seed], tableSelect, null, CreatureTemplate.Type.VultureGrub);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GooieDuck], tableSelect, AbstractPhysicalObject.AbstractObjectType.Lantern, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.LillyPuck], tableSelect, AbstractPhysicalObject.AbstractObjectType.Lantern, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GlowWeed], tableSelect, AbstractPhysicalObject.AbstractObjectType.Lantern, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.DandelionPeach], tableSelect, AbstractPhysicalObject.AbstractObjectType.SporePlant, null);
		rock = AbstractPhysicalObject.AbstractObjectType.VultureMask;
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.VultureMask], tableSelect, null, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.PuffBall], tableSelect, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.DangleFruit], tableSelect, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer], tableSelect, AbstractPhysicalObject.AbstractObjectType.OverseerCarcass, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.DataPearl], tableSelect, null, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.WaterNut], tableSelect, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.JellyFish], tableSelect, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Lantern], tableSelect, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.KarmaFlower], tableSelect, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Mushroom], tableSelect, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant], tableSelect, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SlimeMold], tableSelect, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FlyLure], tableSelect, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.ScavengerBomb], tableSelect, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SporePlant], tableSelect, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.EggBugEgg], tableSelect, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.NeedleEgg], tableSelect, null, CreatureTemplate.Type.VultureGrub);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.BubbleGrass], tableSelect, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.OverseerCarcass], tableSelect, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.SingularityBomb], tableSelect, MoreSlugcatsEnums.AbstractObjectType.FireEgg, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.FireEgg], tableSelect, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.Seed], tableSelect, null, CreatureTemplate.Type.VultureGrub);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GooieDuck], tableSelect, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.LillyPuck], tableSelect, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GlowWeed], tableSelect, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.DandelionPeach], tableSelect, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
		rock = AbstractPhysicalObject.AbstractObjectType.PuffBall;
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.PuffBall], tableSelect, null, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.DangleFruit], tableSelect, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer], tableSelect, AbstractPhysicalObject.AbstractObjectType.OverseerCarcass, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.DataPearl], tableSelect, null, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.WaterNut], tableSelect, MoreSlugcatsEnums.AbstractObjectType.GooieDuck, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.JellyFish], tableSelect, MoreSlugcatsEnums.AbstractObjectType.GooieDuck, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Lantern], tableSelect, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.KarmaFlower], tableSelect, AbstractPhysicalObject.AbstractObjectType.NeedleEgg, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Mushroom], tableSelect, MoreSlugcatsEnums.AbstractObjectType.GooieDuck, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant], tableSelect, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SlimeMold], tableSelect, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FlyLure], tableSelect, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.ScavengerBomb], tableSelect, MoreSlugcatsEnums.AbstractObjectType.GooieDuck, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SporePlant], tableSelect, MoreSlugcatsEnums.AbstractObjectType.GooieDuck, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.EggBugEgg], tableSelect, MoreSlugcatsEnums.AbstractObjectType.GooieDuck, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.NeedleEgg], tableSelect, null, CreatureTemplate.Type.SmallCentipede);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.BubbleGrass], tableSelect, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.OverseerCarcass], tableSelect, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.SingularityBomb], tableSelect, MoreSlugcatsEnums.AbstractObjectType.FireEgg, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.FireEgg], tableSelect, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.Seed], tableSelect, null, CreatureTemplate.Type.SmallCentipede);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GooieDuck], tableSelect, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.LillyPuck], tableSelect, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GlowWeed], tableSelect, MoreSlugcatsEnums.AbstractObjectType.GooieDuck, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.DandelionPeach], tableSelect, MoreSlugcatsEnums.AbstractObjectType.GooieDuck, null);
		rock = AbstractPhysicalObject.AbstractObjectType.DangleFruit;
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.DangleFruit], tableSelect, null, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.DataPearl], tableSelect, null, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.WaterNut], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.JellyFish], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Lantern], tableSelect, AbstractPhysicalObject.AbstractObjectType.FlareBomb, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.KarmaFlower], tableSelect, MoreSlugcatsEnums.AbstractObjectType.Seed, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Mushroom], tableSelect, MoreSlugcatsEnums.AbstractObjectType.GooieDuck, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant], tableSelect, AbstractPhysicalObject.AbstractObjectType.SporePlant, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SlimeMold], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FlyLure], tableSelect, MoreSlugcatsEnums.AbstractObjectType.GooieDuck, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.ScavengerBomb], tableSelect, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SporePlant], tableSelect, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.EggBugEgg], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.NeedleEgg], tableSelect, null, CreatureTemplate.Type.Fly);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.BubbleGrass], tableSelect, MoreSlugcatsEnums.AbstractObjectType.LillyPuck, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.OverseerCarcass], tableSelect, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.SingularityBomb], tableSelect, MoreSlugcatsEnums.AbstractObjectType.FireEgg, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.FireEgg], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.Seed], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GooieDuck], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.LillyPuck], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GlowWeed], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.DandelionPeach], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		rock = AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer;
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer], tableSelect, null, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.DataPearl], tableSelect, AbstractPhysicalObject.AbstractObjectType.OverseerCarcass, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.WaterNut], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.JellyFish], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Lantern], tableSelect, AbstractPhysicalObject.AbstractObjectType.OverseerCarcass, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.KarmaFlower], tableSelect, AbstractPhysicalObject.AbstractObjectType.OverseerCarcass, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Mushroom], tableSelect, AbstractPhysicalObject.AbstractObjectType.OverseerCarcass, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant], tableSelect, AbstractPhysicalObject.AbstractObjectType.OverseerCarcass, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SlimeMold], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FlyLure], tableSelect, AbstractPhysicalObject.AbstractObjectType.OverseerCarcass, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.ScavengerBomb], tableSelect, AbstractPhysicalObject.AbstractObjectType.OverseerCarcass, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SporePlant], tableSelect, AbstractPhysicalObject.AbstractObjectType.OverseerCarcass, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.EggBugEgg], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.NeedleEgg], tableSelect, AbstractPhysicalObject.AbstractObjectType.OverseerCarcass, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.BubbleGrass], tableSelect, AbstractPhysicalObject.AbstractObjectType.OverseerCarcass, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.OverseerCarcass], tableSelect, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.SingularityBomb], tableSelect, AbstractPhysicalObject.AbstractObjectType.OverseerCarcass, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.FireEgg], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.Seed], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GooieDuck], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.LillyPuck], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GlowWeed], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.DandelionPeach], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		rock = AbstractPhysicalObject.AbstractObjectType.DataPearl;
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.DataPearl], tableSelect, null, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.WaterNut], tableSelect, null, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.JellyFish], tableSelect, null, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Lantern], tableSelect, null, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.KarmaFlower], tableSelect, null, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Mushroom], tableSelect, null, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant], tableSelect, null, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SlimeMold], tableSelect, null, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FlyLure], tableSelect, null, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.ScavengerBomb], tableSelect, null, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SporePlant], tableSelect, null, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.EggBugEgg], tableSelect, null, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.NeedleEgg], tableSelect, null, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.BubbleGrass], tableSelect, null, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.OverseerCarcass], tableSelect, null, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.SingularityBomb], tableSelect, null, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.FireEgg], tableSelect, null, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.Seed], tableSelect, null, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GooieDuck], tableSelect, null, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.LillyPuck], tableSelect, null, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GlowWeed], tableSelect, null, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.DandelionPeach], tableSelect, null, null);
		rock = AbstractPhysicalObject.AbstractObjectType.WaterNut;
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.WaterNut], tableSelect, null, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.JellyFish], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Lantern], tableSelect, MoreSlugcatsEnums.AbstractObjectType.GlowWeed, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.KarmaFlower], tableSelect, MoreSlugcatsEnums.AbstractObjectType.Seed, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Mushroom], tableSelect, AbstractPhysicalObject.AbstractObjectType.NeedleEgg, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant], tableSelect, AbstractPhysicalObject.AbstractObjectType.BubbleGrass, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SlimeMold], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FlyLure], tableSelect, AbstractPhysicalObject.AbstractObjectType.BubbleGrass, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.ScavengerBomb], tableSelect, null, CreatureTemplate.Type.Snail);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SporePlant], tableSelect, null, CreatureTemplate.Type.Hazer);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.EggBugEgg], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.NeedleEgg], tableSelect, null, CreatureTemplate.Type.Hazer);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.BubbleGrass], tableSelect, MoreSlugcatsEnums.AbstractObjectType.LillyPuck, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.OverseerCarcass], tableSelect, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.SingularityBomb], tableSelect, MoreSlugcatsEnums.AbstractObjectType.FireEgg, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.FireEgg], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.Seed], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GooieDuck], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.LillyPuck], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GlowWeed], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.DandelionPeach], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		rock = AbstractPhysicalObject.AbstractObjectType.JellyFish;
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.JellyFish], tableSelect, null, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Lantern], tableSelect, MoreSlugcatsEnums.AbstractObjectType.GlowWeed, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.KarmaFlower], tableSelect, AbstractPhysicalObject.AbstractObjectType.NeedleEgg, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Mushroom], tableSelect, MoreSlugcatsEnums.AbstractObjectType.GooieDuck, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant], tableSelect, AbstractPhysicalObject.AbstractObjectType.FlyLure, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SlimeMold], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FlyLure], tableSelect, AbstractPhysicalObject.AbstractObjectType.BubbleGrass, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.ScavengerBomb], tableSelect, null, CreatureTemplate.Type.Snail);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SporePlant], tableSelect, null, CreatureTemplate.Type.Hazer);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.EggBugEgg], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.NeedleEgg], tableSelect, null, CreatureTemplate.Type.Snail);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.BubbleGrass], tableSelect, MoreSlugcatsEnums.AbstractObjectType.LillyPuck, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.OverseerCarcass], tableSelect, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.SingularityBomb], tableSelect, MoreSlugcatsEnums.AbstractObjectType.FireEgg, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.FireEgg], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.Seed], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GooieDuck], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.LillyPuck], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GlowWeed], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.DandelionPeach], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		rock = AbstractPhysicalObject.AbstractObjectType.Lantern;
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Lantern], tableSelect, null, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.KarmaFlower], tableSelect, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Mushroom], tableSelect, AbstractPhysicalObject.AbstractObjectType.PuffBall, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant], tableSelect, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SlimeMold], tableSelect, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FlyLure], tableSelect, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.ScavengerBomb], tableSelect, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SporePlant], tableSelect, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.EggBugEgg], tableSelect, AbstractPhysicalObject.AbstractObjectType.NeedleEgg, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.NeedleEgg], tableSelect, null, CreatureTemplate.Type.VultureGrub);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.BubbleGrass], tableSelect, MoreSlugcatsEnums.AbstractObjectType.GlowWeed, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.OverseerCarcass], tableSelect, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.SingularityBomb], tableSelect, MoreSlugcatsEnums.AbstractObjectType.FireEgg, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.FireEgg], tableSelect, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.Seed], tableSelect, null, CreatureTemplate.Type.VultureGrub);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GooieDuck], tableSelect, MoreSlugcatsEnums.AbstractObjectType.LillyPuck, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.LillyPuck], tableSelect, MoreSlugcatsEnums.AbstractObjectType.GlowWeed, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GlowWeed], tableSelect, MoreSlugcatsEnums.AbstractObjectType.LillyPuck, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.DandelionPeach], tableSelect, MoreSlugcatsEnums.AbstractObjectType.GlowWeed, null);
		rock = AbstractPhysicalObject.AbstractObjectType.KarmaFlower;
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.KarmaFlower], tableSelect, null, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Mushroom], tableSelect, AbstractPhysicalObject.AbstractObjectType.NeedleEgg, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant], tableSelect, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SlimeMold], tableSelect, MoreSlugcatsEnums.AbstractObjectType.Seed, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FlyLure], tableSelect, MoreSlugcatsEnums.AbstractObjectType.Seed, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.ScavengerBomb], tableSelect, MoreSlugcatsEnums.AbstractObjectType.FireEgg, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SporePlant], tableSelect, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.EggBugEgg], tableSelect, AbstractPhysicalObject.AbstractObjectType.NeedleEgg, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.NeedleEgg], tableSelect, AbstractPhysicalObject.AbstractObjectType.OverseerCarcass, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.BubbleGrass], tableSelect, MoreSlugcatsEnums.AbstractObjectType.Seed, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.OverseerCarcass], tableSelect, AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.SingularityBomb], tableSelect, MoreSlugcatsEnums.AbstractObjectType.FireEgg, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.FireEgg], tableSelect, MoreSlugcatsEnums.AbstractObjectType.SingularityBomb, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.Seed], tableSelect, AbstractPhysicalObject.AbstractObjectType.OverseerCarcass, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GooieDuck], tableSelect, MoreSlugcatsEnums.AbstractObjectType.Seed, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.LillyPuck], tableSelect, MoreSlugcatsEnums.AbstractObjectType.Seed, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GlowWeed], tableSelect, MoreSlugcatsEnums.AbstractObjectType.Seed, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.DandelionPeach], tableSelect, MoreSlugcatsEnums.AbstractObjectType.Seed, null);
		rock = AbstractPhysicalObject.AbstractObjectType.Mushroom;
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Mushroom], tableSelect, null, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant], tableSelect, null, CreatureTemplate.Type.SmallCentipede);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SlimeMold], tableSelect, null, CreatureTemplate.Type.SmallNeedleWorm);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FlyLure], tableSelect, null, CreatureTemplate.Type.Fly);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.ScavengerBomb], tableSelect, AbstractPhysicalObject.AbstractObjectType.SporePlant, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SporePlant], tableSelect, MoreSlugcatsEnums.AbstractObjectType.GooieDuck, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.EggBugEgg], tableSelect, MoreSlugcatsEnums.AbstractObjectType.GooieDuck, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.NeedleEgg], tableSelect, null, CreatureTemplate.Type.SmallCentipede);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.BubbleGrass], tableSelect, null, CreatureTemplate.Type.Hazer);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.OverseerCarcass], tableSelect, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.SingularityBomb], tableSelect, null, CreatureTemplate.Type.SmallCentipede);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.FireEgg], tableSelect, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.Seed], tableSelect, null, CreatureTemplate.Type.SmallCentipede);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GooieDuck], tableSelect, AbstractPhysicalObject.AbstractObjectType.PuffBall, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.LillyPuck], tableSelect, MoreSlugcatsEnums.AbstractObjectType.GooieDuck, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GlowWeed], tableSelect, MoreSlugcatsEnums.AbstractObjectType.GooieDuck, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.DandelionPeach], tableSelect, MoreSlugcatsEnums.AbstractObjectType.GooieDuck, null);
		rock = AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant;
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant], tableSelect, null, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SlimeMold], tableSelect, AbstractPhysicalObject.AbstractObjectType.Lantern, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FlyLure], tableSelect, AbstractPhysicalObject.AbstractObjectType.SporePlant, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.ScavengerBomb], tableSelect, AbstractPhysicalObject.AbstractObjectType.Lantern, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SporePlant], tableSelect, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.EggBugEgg], tableSelect, AbstractPhysicalObject.AbstractObjectType.SporePlant, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.NeedleEgg], tableSelect, null, CreatureTemplate.Type.SmallCentipede);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.BubbleGrass], tableSelect, null, CreatureTemplate.Type.Snail);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.OverseerCarcass], tableSelect, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.SingularityBomb], tableSelect, MoreSlugcatsEnums.AbstractObjectType.FireEgg, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.FireEgg], tableSelect, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.Seed], tableSelect, null, CreatureTemplate.Type.SmallCentipede);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GooieDuck], tableSelect, AbstractPhysicalObject.AbstractObjectType.SporePlant, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.LillyPuck], tableSelect, AbstractPhysicalObject.AbstractObjectType.SporePlant, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GlowWeed], tableSelect, AbstractPhysicalObject.AbstractObjectType.SporePlant, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.DandelionPeach], tableSelect, AbstractPhysicalObject.AbstractObjectType.SporePlant, null);
		rock = AbstractPhysicalObject.AbstractObjectType.SlimeMold;
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SlimeMold], tableSelect, null, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FlyLure], tableSelect, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.ScavengerBomb], tableSelect, AbstractPhysicalObject.AbstractObjectType.Lantern, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SporePlant], tableSelect, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.EggBugEgg], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.NeedleEgg], tableSelect, null, CreatureTemplate.Type.SmallCentipede);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.BubbleGrass], tableSelect, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.OverseerCarcass], tableSelect, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.SingularityBomb], tableSelect, MoreSlugcatsEnums.AbstractObjectType.FireEgg, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.FireEgg], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.Seed], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GooieDuck], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.LillyPuck], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GlowWeed], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.DandelionPeach], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		rock = AbstractPhysicalObject.AbstractObjectType.FlyLure;
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FlyLure], tableSelect, null, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.ScavengerBomb], tableSelect, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SporePlant], tableSelect, AbstractPhysicalObject.AbstractObjectType.PuffBall, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.EggBugEgg], tableSelect, MoreSlugcatsEnums.AbstractObjectType.GooieDuck, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.NeedleEgg], tableSelect, null, CreatureTemplate.Type.Fly);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.BubbleGrass], tableSelect, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.OverseerCarcass], tableSelect, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.SingularityBomb], tableSelect, MoreSlugcatsEnums.AbstractObjectType.FireEgg, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.FireEgg], tableSelect, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.Seed], tableSelect, null, CreatureTemplate.Type.Fly);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GooieDuck], tableSelect, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.LillyPuck], tableSelect, null, CreatureTemplate.Type.Hazer);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GlowWeed], tableSelect, null, CreatureTemplate.Type.SmallCentipede);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.DandelionPeach], tableSelect, null, CreatureTemplate.Type.Fly);
		rock = AbstractPhysicalObject.AbstractObjectType.ScavengerBomb;
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.ScavengerBomb], tableSelect, null, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SporePlant], tableSelect, AbstractPhysicalObject.AbstractObjectType.PuffBall, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.EggBugEgg], tableSelect, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.NeedleEgg], tableSelect, null, CreatureTemplate.Type.SmallCentipede);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.BubbleGrass], tableSelect, AbstractPhysicalObject.AbstractObjectType.PuffBall, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.OverseerCarcass], tableSelect, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.SingularityBomb], tableSelect, MoreSlugcatsEnums.AbstractObjectType.FireEgg, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.FireEgg], tableSelect, MoreSlugcatsEnums.AbstractObjectType.SingularityBomb, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.Seed], tableSelect, null, CreatureTemplate.Type.SmallCentipede);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GooieDuck], tableSelect, AbstractPhysicalObject.AbstractObjectType.PuffBall, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.LillyPuck], tableSelect, AbstractPhysicalObject.AbstractObjectType.SporePlant, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GlowWeed], tableSelect, AbstractPhysicalObject.AbstractObjectType.Lantern, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.DandelionPeach], tableSelect, AbstractPhysicalObject.AbstractObjectType.PuffBall, null);
		rock = AbstractPhysicalObject.AbstractObjectType.SporePlant;
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SporePlant], tableSelect, null, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.EggBugEgg], tableSelect, AbstractPhysicalObject.AbstractObjectType.PuffBall, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.NeedleEgg], tableSelect, null, CreatureTemplate.Type.TubeWorm);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.BubbleGrass], tableSelect, AbstractPhysicalObject.AbstractObjectType.PuffBall, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.OverseerCarcass], tableSelect, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.SingularityBomb], tableSelect, MoreSlugcatsEnums.AbstractObjectType.FireEgg, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.FireEgg], tableSelect, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.Seed], tableSelect, null, CreatureTemplate.Type.TubeWorm);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GooieDuck], tableSelect, AbstractPhysicalObject.AbstractObjectType.PuffBall, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.LillyPuck], tableSelect, AbstractPhysicalObject.AbstractObjectType.JellyFish, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GlowWeed], tableSelect, AbstractPhysicalObject.AbstractObjectType.JellyFish, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.DandelionPeach], tableSelect, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
		rock = AbstractPhysicalObject.AbstractObjectType.EggBugEgg;
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.EggBugEgg], tableSelect, null, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.NeedleEgg], tableSelect, null, CreatureTemplate.Type.SmallNeedleWorm);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.BubbleGrass], tableSelect, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.OverseerCarcass], tableSelect, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.SingularityBomb], tableSelect, MoreSlugcatsEnums.AbstractObjectType.FireEgg, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.FireEgg], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.Seed], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GooieDuck], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.LillyPuck], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GlowWeed], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.DandelionPeach], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		rock = AbstractPhysicalObject.AbstractObjectType.NeedleEgg;
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.NeedleEgg], tableSelect, null, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.BubbleGrass], tableSelect, null, CreatureTemplate.Type.Hazer);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.OverseerCarcass], tableSelect, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.SingularityBomb], tableSelect, null, CreatureTemplate.Type.VultureGrub);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.FireEgg], tableSelect, null, CreatureTemplate.Type.VultureGrub);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.Seed], tableSelect, null, CreatureTemplate.Type.SmallNeedleWorm);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GooieDuck], tableSelect, null, CreatureTemplate.Type.TubeWorm);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.LillyPuck], tableSelect, null, CreatureTemplate.Type.Hazer);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GlowWeed], tableSelect, null, CreatureTemplate.Type.Hazer);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.DandelionPeach], tableSelect, null, CreatureTemplate.Type.SmallNeedleWorm);
		rock = AbstractPhysicalObject.AbstractObjectType.BubbleGrass;
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.BubbleGrass], tableSelect, null, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.OverseerCarcass], tableSelect, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.SingularityBomb], tableSelect, MoreSlugcatsEnums.AbstractObjectType.FireEgg, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.FireEgg], tableSelect, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.Seed], tableSelect, null, CreatureTemplate.Type.Hazer);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GooieDuck], tableSelect, AbstractPhysicalObject.AbstractObjectType.PuffBall, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.LillyPuck], tableSelect, AbstractPhysicalObject.AbstractObjectType.JellyFish, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GlowWeed], tableSelect, MoreSlugcatsEnums.AbstractObjectType.LillyPuck, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.DandelionPeach], tableSelect, MoreSlugcatsEnums.AbstractObjectType.GlowWeed, null);
		rock = AbstractPhysicalObject.AbstractObjectType.OverseerCarcass;
		SetLibraryData(objectsLibrary[rock], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.OverseerCarcass], tableSelect, null, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.SingularityBomb], tableSelect, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.FireEgg], tableSelect, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.Seed], tableSelect, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GooieDuck], tableSelect, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.LillyPuck], tableSelect, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GlowWeed], tableSelect, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.DandelionPeach], tableSelect, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
		rock = MoreSlugcatsEnums.AbstractObjectType.SingularityBomb;
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.SingularityBomb], tableSelect, null, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.FireEgg], tableSelect, AbstractPhysicalObject.AbstractObjectType.KarmaFlower, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.Seed], tableSelect, MoreSlugcatsEnums.AbstractObjectType.FireEgg, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GooieDuck], tableSelect, MoreSlugcatsEnums.AbstractObjectType.FireEgg, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.LillyPuck], tableSelect, MoreSlugcatsEnums.AbstractObjectType.FireEgg, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GlowWeed], tableSelect, MoreSlugcatsEnums.AbstractObjectType.FireEgg, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.DandelionPeach], tableSelect, MoreSlugcatsEnums.AbstractObjectType.FireEgg, null);
		rock = MoreSlugcatsEnums.AbstractObjectType.FireEgg;
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.FireEgg], tableSelect, null, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.Seed], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GooieDuck], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.LillyPuck], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GlowWeed], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.DandelionPeach], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		rock = MoreSlugcatsEnums.AbstractObjectType.Seed;
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.Seed], tableSelect, null, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GooieDuck], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.LillyPuck], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GlowWeed], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.DandelionPeach], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		rock = MoreSlugcatsEnums.AbstractObjectType.GooieDuck;
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GooieDuck], tableSelect, null, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.LillyPuck], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GlowWeed], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.DandelionPeach], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		rock = MoreSlugcatsEnums.AbstractObjectType.LillyPuck;
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.LillyPuck], tableSelect, null, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GlowWeed], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.DandelionPeach], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		rock = MoreSlugcatsEnums.AbstractObjectType.GlowWeed;
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GlowWeed], tableSelect, null, null);
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.DandelionPeach], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		rock = MoreSlugcatsEnums.AbstractObjectType.DandelionPeach;
		SetLibraryData(objectsLibrary[rock], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.DandelionPeach], tableSelect, null, null);
		tableSelect = 1;
		CreatureTemplate.Type fly = CreatureTemplate.Type.Fly;
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Rock], tableSelect, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FlareBomb], tableSelect, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.VultureMask], tableSelect, MoreSlugcatsEnums.AbstractObjectType.Seed, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.PuffBall], tableSelect, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.DangleFruit], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.DataPearl], tableSelect, null, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.WaterNut], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.JellyFish], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Lantern], tableSelect, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.KarmaFlower], tableSelect, AbstractPhysicalObject.AbstractObjectType.FlyLure, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Mushroom], tableSelect, MoreSlugcatsEnums.AbstractObjectType.GooieDuck, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant], tableSelect, AbstractPhysicalObject.AbstractObjectType.SporePlant, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SlimeMold], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FlyLure], tableSelect, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.ScavengerBomb], tableSelect, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SporePlant], tableSelect, AbstractPhysicalObject.AbstractObjectType.FlyLure, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.EggBugEgg], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.NeedleEgg], tableSelect, MoreSlugcatsEnums.AbstractObjectType.Seed, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.BubbleGrass], tableSelect, AbstractPhysicalObject.AbstractObjectType.FlyLure, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.OverseerCarcass], tableSelect, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.SingularityBomb], tableSelect, MoreSlugcatsEnums.AbstractObjectType.FireEgg, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.FireEgg], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.Seed], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GooieDuck], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.LillyPuck], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GlowWeed], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.DandelionPeach], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		fly = CreatureTemplate.Type.SmallCentipede;
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Rock], tableSelect, AbstractPhysicalObject.AbstractObjectType.SporePlant, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FlareBomb], tableSelect, AbstractPhysicalObject.AbstractObjectType.SporePlant, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.VultureMask], tableSelect, MoreSlugcatsEnums.AbstractObjectType.Seed, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.PuffBall], tableSelect, AbstractPhysicalObject.AbstractObjectType.SporePlant, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.DangleFruit], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.DataPearl], tableSelect, null, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.WaterNut], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.JellyFish], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Lantern], tableSelect, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.KarmaFlower], tableSelect, AbstractPhysicalObject.AbstractObjectType.PuffBall, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Mushroom], tableSelect, MoreSlugcatsEnums.AbstractObjectType.GooieDuck, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant], tableSelect, AbstractPhysicalObject.AbstractObjectType.SporePlant, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SlimeMold], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FlyLure], tableSelect, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.ScavengerBomb], tableSelect, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SporePlant], tableSelect, null, CreatureTemplate.Type.TubeWorm);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.EggBugEgg], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.NeedleEgg], tableSelect, MoreSlugcatsEnums.AbstractObjectType.Seed, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.BubbleGrass], tableSelect, AbstractPhysicalObject.AbstractObjectType.JellyFish, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.OverseerCarcass], tableSelect, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.SingularityBomb], tableSelect, MoreSlugcatsEnums.AbstractObjectType.FireEgg, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.FireEgg], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.Seed], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GooieDuck], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.LillyPuck], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GlowWeed], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.DandelionPeach], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		fly = CreatureTemplate.Type.VultureGrub;
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Rock], tableSelect, AbstractPhysicalObject.AbstractObjectType.Lantern, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FlareBomb], tableSelect, AbstractPhysicalObject.AbstractObjectType.Lantern, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.VultureMask], tableSelect, MoreSlugcatsEnums.AbstractObjectType.Seed, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.PuffBall], tableSelect, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.DangleFruit], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.DataPearl], tableSelect, null, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.WaterNut], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.JellyFish], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Lantern], tableSelect, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.KarmaFlower], tableSelect, AbstractPhysicalObject.AbstractObjectType.VultureMask, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Mushroom], tableSelect, MoreSlugcatsEnums.AbstractObjectType.GooieDuck, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant], tableSelect, AbstractPhysicalObject.AbstractObjectType.SporePlant, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SlimeMold], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FlyLure], tableSelect, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.ScavengerBomb], tableSelect, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SporePlant], tableSelect, AbstractPhysicalObject.AbstractObjectType.FlareBomb, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.EggBugEgg], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.NeedleEgg], tableSelect, MoreSlugcatsEnums.AbstractObjectType.Seed, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.BubbleGrass], tableSelect, MoreSlugcatsEnums.AbstractObjectType.GlowWeed, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.OverseerCarcass], tableSelect, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.SingularityBomb], tableSelect, MoreSlugcatsEnums.AbstractObjectType.FireEgg, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.FireEgg], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.Seed], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GooieDuck], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.LillyPuck], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GlowWeed], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.DandelionPeach], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		fly = CreatureTemplate.Type.SmallNeedleWorm;
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Rock], tableSelect, AbstractPhysicalObject.AbstractObjectType.PuffBall, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FlareBomb], tableSelect, AbstractPhysicalObject.AbstractObjectType.PuffBall, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.VultureMask], tableSelect, MoreSlugcatsEnums.AbstractObjectType.Seed, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.PuffBall], tableSelect, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.DangleFruit], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.DataPearl], tableSelect, null, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.WaterNut], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.JellyFish], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Lantern], tableSelect, AbstractPhysicalObject.AbstractObjectType.NeedleEgg, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.KarmaFlower], tableSelect, AbstractPhysicalObject.AbstractObjectType.NeedleEgg, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Mushroom], tableSelect, AbstractPhysicalObject.AbstractObjectType.PuffBall, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant], tableSelect, AbstractPhysicalObject.AbstractObjectType.SporePlant, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SlimeMold], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FlyLure], tableSelect, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.ScavengerBomb], tableSelect, AbstractPhysicalObject.AbstractObjectType.PuffBall, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SporePlant], tableSelect, AbstractPhysicalObject.AbstractObjectType.PuffBall, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.EggBugEgg], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.NeedleEgg], tableSelect, MoreSlugcatsEnums.AbstractObjectType.Seed, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.BubbleGrass], tableSelect, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.OverseerCarcass], tableSelect, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.SingularityBomb], tableSelect, MoreSlugcatsEnums.AbstractObjectType.FireEgg, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.FireEgg], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.Seed], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GooieDuck], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.LillyPuck], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GlowWeed], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.DandelionPeach], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		fly = CreatureTemplate.Type.Hazer;
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Rock], tableSelect, MoreSlugcatsEnums.AbstractObjectType.LillyPuck, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FlareBomb], tableSelect, MoreSlugcatsEnums.AbstractObjectType.GlowWeed, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.VultureMask], tableSelect, MoreSlugcatsEnums.AbstractObjectType.Seed, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.PuffBall], tableSelect, AbstractPhysicalObject.AbstractObjectType.BubbleGrass, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.DangleFruit], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.DataPearl], tableSelect, null, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.WaterNut], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.JellyFish], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Lantern], tableSelect, null, CreatureTemplate.Type.Snail);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.KarmaFlower], tableSelect, AbstractPhysicalObject.AbstractObjectType.BubbleGrass, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Mushroom], tableSelect, AbstractPhysicalObject.AbstractObjectType.BubbleGrass, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant], tableSelect, AbstractPhysicalObject.AbstractObjectType.JellyFish, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SlimeMold], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FlyLure], tableSelect, AbstractPhysicalObject.AbstractObjectType.BubbleGrass, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.ScavengerBomb], tableSelect, AbstractPhysicalObject.AbstractObjectType.FlareBomb, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SporePlant], tableSelect, AbstractPhysicalObject.AbstractObjectType.JellyFish, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.EggBugEgg], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.NeedleEgg], tableSelect, MoreSlugcatsEnums.AbstractObjectType.Seed, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.BubbleGrass], tableSelect, MoreSlugcatsEnums.AbstractObjectType.GlowWeed, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[AbstractPhysicalObject.AbstractObjectType.OverseerCarcass], tableSelect, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.SingularityBomb], tableSelect, MoreSlugcatsEnums.AbstractObjectType.FireEgg, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.FireEgg], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.Seed], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GooieDuck], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.LillyPuck], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.GlowWeed], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.DandelionPeach], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		Custom.Log("--Setup Creature + Creature table");
		tableSelect = 2;
		fly = CreatureTemplate.Type.Fly;
		SetLibraryData(critsLibrary[fly], critsLibrary[CreatureTemplate.Type.Fly], tableSelect, null, null);
		SetLibraryData(critsLibrary[fly], critsLibrary[CreatureTemplate.Type.VultureGrub], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], critsLibrary[CreatureTemplate.Type.SmallCentipede], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], critsLibrary[CreatureTemplate.Type.SmallNeedleWorm], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], critsLibrary[CreatureTemplate.Type.Hazer], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		fly = CreatureTemplate.Type.VultureGrub;
		SetLibraryData(critsLibrary[fly], critsLibrary[CreatureTemplate.Type.VultureGrub], tableSelect, null, null);
		SetLibraryData(critsLibrary[fly], critsLibrary[CreatureTemplate.Type.SmallCentipede], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], critsLibrary[CreatureTemplate.Type.SmallNeedleWorm], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], critsLibrary[CreatureTemplate.Type.Hazer], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		fly = CreatureTemplate.Type.SmallCentipede;
		SetLibraryData(critsLibrary[fly], critsLibrary[CreatureTemplate.Type.SmallCentipede], tableSelect, null, null);
		SetLibraryData(critsLibrary[fly], critsLibrary[CreatureTemplate.Type.SmallNeedleWorm], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		SetLibraryData(critsLibrary[fly], critsLibrary[CreatureTemplate.Type.Hazer], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		fly = CreatureTemplate.Type.SmallNeedleWorm;
		SetLibraryData(critsLibrary[fly], critsLibrary[CreatureTemplate.Type.SmallNeedleWorm], tableSelect, null, null);
		SetLibraryData(critsLibrary[fly], critsLibrary[CreatureTemplate.Type.Hazer], tableSelect, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
		fly = CreatureTemplate.Type.Hazer;
		SetLibraryData(critsLibrary[fly], critsLibrary[CreatureTemplate.Type.Hazer], tableSelect, null, null);
	}

	public static CraftDat GetFilteredLibraryData(Creature.Grasp graspA, Creature.Grasp graspB)
	{
		AbstractPhysicalObject.AbstractObjectType abstractObjectType = graspA.grabbed.abstractPhysicalObject.type;
		AbstractPhysicalObject.AbstractObjectType abstractObjectType2 = graspB.grabbed.abstractPhysicalObject.type;
		if (abstractObjectType == AbstractPhysicalObject.AbstractObjectType.WaterNut && graspA.grabbed is WaterNut)
		{
			abstractObjectType = AbstractPhysicalObject.AbstractObjectType.Rock;
		}
		if (abstractObjectType2 == AbstractPhysicalObject.AbstractObjectType.WaterNut && graspB.grabbed is WaterNut)
		{
			abstractObjectType2 = AbstractPhysicalObject.AbstractObjectType.Rock;
		}
		if (abstractObjectType == AbstractPhysicalObject.AbstractObjectType.PebblesPearl)
		{
			abstractObjectType = AbstractPhysicalObject.AbstractObjectType.DataPearl;
		}
		if (abstractObjectType2 == AbstractPhysicalObject.AbstractObjectType.PebblesPearl)
		{
			abstractObjectType2 = AbstractPhysicalObject.AbstractObjectType.DataPearl;
		}
		if (abstractObjectType == AbstractPhysicalObject.AbstractObjectType.SLOracleSwarmer)
		{
			abstractObjectType = AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer;
		}
		if (abstractObjectType2 == AbstractPhysicalObject.AbstractObjectType.SLOracleSwarmer)
		{
			abstractObjectType2 = AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer;
		}
		if (graspA.grabbed is Creature && graspB.grabbed is Creature)
		{
			return GetLibraryData((graspA.grabbed as Creature).abstractCreature.creatureTemplate.type, (graspB.grabbed as Creature).abstractCreature.creatureTemplate.type);
		}
		if (graspA.grabbed is Creature)
		{
			return GetLibraryData((graspA.grabbed as Creature).abstractCreature.creatureTemplate.type, abstractObjectType2);
		}
		if (graspB.grabbed is Creature)
		{
			return GetLibraryData((graspB.grabbed as Creature).abstractCreature.creatureTemplate.type, abstractObjectType);
		}
		return GetLibraryData(abstractObjectType, abstractObjectType2);
	}

	public static void SetLibraryData(AbstractPhysicalObject.AbstractObjectType objectA, AbstractPhysicalObject.AbstractObjectType objectB, AbstractPhysicalObject.AbstractObjectType resultType, CreatureTemplate.Type resultCritter)
	{
		SetLibraryData(objectsLibrary[objectA], objectsLibrary[objectB], 0, resultType, resultCritter);
	}

	public static void SetLibraryData(CreatureTemplate.Type critterA, AbstractPhysicalObject.AbstractObjectType objectB, AbstractPhysicalObject.AbstractObjectType resultType, CreatureTemplate.Type resultCritter)
	{
		SetLibraryData(critsLibrary[critterA], objectsLibrary[objectB], 1, resultType, resultCritter);
	}

	public static void SetLibraryData(CreatureTemplate.Type critterA, CreatureTemplate.Type critterB, AbstractPhysicalObject.AbstractObjectType resultType, CreatureTemplate.Type resultCritter)
	{
		SetLibraryData(critsLibrary[critterA], critsLibrary[critterB], 2, resultType, resultCritter);
	}

	public static void SetLibraryData(int x, int y, int tableSelect, AbstractPhysicalObject.AbstractObjectType resultType, CreatureTemplate.Type resultCritter)
	{
		if (showDebug)
		{
			Custom.Log($"CRAFTTABLE: T {tableSelect} X {x} Y {y} = {resultType} - {resultCritter}");
		}
		switch (tableSelect)
		{
		case 0:
			craftingGrid_ObjectsOnly[x, y] = new CraftDat(resultType, resultCritter);
			craftingGrid_ObjectsOnly[y, x] = new CraftDat(resultType, resultCritter);
			break;
		case 1:
			craftingGrid_CritterObjects[x, y] = new CraftDat(resultType, resultCritter);
			break;
		case 2:
			craftingGrid_CrittersOnly[x, y] = new CraftDat(resultType, resultCritter);
			craftingGrid_CrittersOnly[y, x] = new CraftDat(resultType, resultCritter);
			break;
		}
	}

	public static AbstractPhysicalObject RandomStomachItem(PhysicalObject caller)
	{
		float value = Random.value;
		AbstractPhysicalObject abstractPhysicalObject;
		if (value <= 25f / 76f)
		{
			abstractPhysicalObject = new AbstractConsumable(caller.room.world, AbstractPhysicalObject.AbstractObjectType.FlyLure, null, caller.room.GetWorldCoordinate(caller.firstChunk.pos), caller.room.game.GetNewID(), -1, -1, null);
		}
		else if (value <= 65f / 152f)
		{
			abstractPhysicalObject = new AbstractConsumable(caller.room.world, AbstractPhysicalObject.AbstractObjectType.Mushroom, null, caller.room.GetWorldCoordinate(caller.firstChunk.pos), caller.room.game.GetNewID(), -1, -1, null);
		}
		else if (value <= 77f / 152f)
		{
			abstractPhysicalObject = new AbstractConsumable(caller.room.world, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant, null, caller.room.GetWorldCoordinate(caller.firstChunk.pos), caller.room.game.GetNewID(), -1, -1, null);
		}
		else if (value <= 93f / 152f)
		{
			abstractPhysicalObject = new WaterNut.AbstractWaterNut(caller.room.world, null, caller.room.GetWorldCoordinate(caller.firstChunk.pos), caller.room.game.GetNewID(), -1, -1, null, swollen: false);
		}
		else if (value <= 101f / 152f)
		{
			abstractPhysicalObject = new AbstractCreature(caller.room.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.VultureGrub), null, caller.room.GetWorldCoordinate(caller.firstChunk.pos), caller.room.game.GetNewID());
		}
		else if (value <= 111f / 152f)
		{
			abstractPhysicalObject = new AbstractCreature(caller.room.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Snail), null, caller.room.GetWorldCoordinate(caller.firstChunk.pos), caller.room.game.GetNewID());
		}
		else if (value <= 121f / 152f)
		{
			abstractPhysicalObject = new AbstractCreature(caller.room.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Hazer), null, caller.room.GetWorldCoordinate(caller.firstChunk.pos), caller.room.game.GetNewID());
		}
		else if (value <= 63f / 76f)
		{
			abstractPhysicalObject = new AbstractConsumable(caller.room.world, AbstractPhysicalObject.AbstractObjectType.PuffBall, null, caller.room.GetWorldCoordinate(caller.firstChunk.pos), caller.room.game.GetNewID(), -1, -1, null);
		}
		else if (value <= 129f / 152f)
		{
			abstractPhysicalObject = new AbstractPhysicalObject(caller.room.world, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null, caller.room.GetWorldCoordinate(caller.firstChunk.pos), caller.room.game.GetNewID());
		}
		else if (value <= 139f / 152f)
		{
			abstractPhysicalObject = new BubbleGrass.AbstractBubbleGrass(caller.room.world, null, caller.room.GetWorldCoordinate(caller.firstChunk.pos), caller.room.game.GetNewID(), 1f, -1, -1, null);
		}
		else if (value <= 71f / 76f)
		{
			abstractPhysicalObject = new SporePlant.AbstractSporePlant(caller.room.world, null, caller.room.GetWorldCoordinate(caller.firstChunk.pos), caller.room.game.GetNewID(), -1, -1, null, used: false, (double)Random.value < 0.5);
		}
		else if (!(value <= 71f / 152f))
		{
			abstractPhysicalObject = ((value <= 0.4736842f) ? new AbstractConsumable(caller.room.world, AbstractPhysicalObject.AbstractObjectType.KarmaFlower, null, caller.room.GetWorldCoordinate(caller.firstChunk.pos), caller.room.game.GetNewID(), -1, -1, null) : ((value <= 151f / 152f) ? new AbstractPhysicalObject(caller.room.world, AbstractPhysicalObject.AbstractObjectType.Lantern, null, caller.room.GetWorldCoordinate(caller.firstChunk.pos), caller.room.game.GetNewID()) : ((value <= 121f / 152f) ? new AbstractCreature(caller.room.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.TubeWorm), null, caller.room.GetWorldCoordinate(caller.firstChunk.pos), caller.room.game.GetNewID()) : ((!(value <= 0.8f)) ? ((AbstractPhysicalObject)new DataPearl.AbstractDataPearl(caller.room.world, AbstractPhysicalObject.AbstractObjectType.DataPearl, null, caller.room.GetWorldCoordinate(caller.firstChunk.pos), caller.room.game.GetNewID(), -1, -1, null, DataPearl.AbstractDataPearl.DataPearlType.Misc)) : ((AbstractPhysicalObject)new VultureMask.AbstractVultureMask(caller.room.world, null, caller.room.GetWorldCoordinate(caller.firstChunk.pos), caller.room.game.GetNewID(), caller.abstractPhysicalObject.ID.RandomSeed, (double)Random.value <= 0.05))))));
		}
		else
		{
			Color color = new Color(1f, 0.8f, 0.3f);
			int ownerIterator = 1;
			if (Random.value <= 0.35f)
			{
				color = new Color(38f / 85f, 46f / 51f, 0.76862746f);
				ownerIterator = 0;
			}
			else if (Random.value <= 0.05f)
			{
				color = new Color(0f, 1f, 0f);
				ownerIterator = 2;
			}
			abstractPhysicalObject = new OverseerCarcass.AbstractOverseerCarcass(caller.room.world, null, caller.abstractPhysicalObject.pos, caller.room.game.GetNewID(), color, ownerIterator);
		}
		if (AbstractConsumable.IsTypeConsumable(abstractPhysicalObject.type))
		{
			(abstractPhysicalObject as AbstractConsumable).isFresh = false;
			(abstractPhysicalObject as AbstractConsumable).isConsumed = true;
		}
		return abstractPhysicalObject;
	}
}
