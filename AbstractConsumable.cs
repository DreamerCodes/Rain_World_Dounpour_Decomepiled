using System.Globalization;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class AbstractConsumable : AbstractPhysicalObject
{
	public int originRoom;

	public int placedObjectIndex;

	public bool isConsumed = true;

	public int minCycles;

	public int maxCycles;

	public bool isFresh;

	public static bool IsTypeConsumable(AbstractObjectType type)
	{
		if (type == AbstractObjectType.DangleFruit)
		{
			return true;
		}
		if (type == AbstractObjectType.DataPearl)
		{
			return true;
		}
		if (type == AbstractObjectType.FlareBomb)
		{
			return true;
		}
		if (type == AbstractObjectType.PuffBall)
		{
			return true;
		}
		if (type == AbstractObjectType.WaterNut)
		{
			return true;
		}
		if (type == AbstractObjectType.KarmaFlower)
		{
			return true;
		}
		if (type == AbstractObjectType.Mushroom)
		{
			return true;
		}
		if (type == AbstractObjectType.FirecrackerPlant)
		{
			return true;
		}
		if (type == AbstractObjectType.SlimeMold)
		{
			return true;
		}
		if (type == AbstractObjectType.JellyFish)
		{
			return true;
		}
		if (type == AbstractObjectType.FlyLure)
		{
			return true;
		}
		if (type == AbstractObjectType.NeedleEgg)
		{
			return true;
		}
		if (ModManager.MSC)
		{
			if (type == MoreSlugcatsEnums.AbstractObjectType.Seed)
			{
				return true;
			}
			if (type == MoreSlugcatsEnums.AbstractObjectType.GooieDuck)
			{
				return true;
			}
			if (type == MoreSlugcatsEnums.AbstractObjectType.LillyPuck)
			{
				return true;
			}
			if (type == MoreSlugcatsEnums.AbstractObjectType.GlowWeed)
			{
				return true;
			}
			if (type == MoreSlugcatsEnums.AbstractObjectType.MoonCloak)
			{
				return true;
			}
			if (type == MoreSlugcatsEnums.AbstractObjectType.DandelionPeach)
			{
				return true;
			}
			if (type == MoreSlugcatsEnums.AbstractObjectType.HRGuard)
			{
				return true;
			}
		}
		return false;
	}

	public AbstractConsumable(World world, AbstractObjectType type, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID, int originRoom, int placedObjectIndex, PlacedObject.ConsumableObjectData consumableData)
		: base(world, type, realizedObject, pos, ID)
	{
		this.originRoom = originRoom;
		this.placedObjectIndex = placedObjectIndex;
		if (consumableData != null)
		{
			minCycles = consumableData.minRegen;
			maxCycles = consumableData.maxRegen;
		}
		else
		{
			minCycles = -1;
			maxCycles = -1;
		}
		isFresh = true;
	}

	public virtual void Consume()
	{
		if (!isConsumed)
		{
			isConsumed = true;
			Custom.Log($"CONSUMED: {type}");
			if (world.game.session is StoryGameSession)
			{
				(world.game.session as StoryGameSession).saveState.ReportConsumedItem(world, type == AbstractObjectType.KarmaFlower, originRoom, placedObjectIndex, (minCycles > 0) ? Random.Range(minCycles, maxCycles + 1) : (-1));
			}
		}
	}

	public override string ToString()
	{
		string baseString = string.Format(CultureInfo.InvariantCulture, "{0}<oA>{1}<oA>{2}<oA>{3}<oA>{4}", ID.ToString(), type.ToString(), pos.SaveToString(), originRoom, placedObjectIndex);
		baseString = SaveState.SetCustomData(this, baseString);
		return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "<oA>", unrecognizedAttributes);
	}
}
