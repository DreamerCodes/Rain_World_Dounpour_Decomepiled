using MoreSlugcats;
using UnityEngine;

public class DropBugAbstractAI : AbstractCreatureAI
{
	public DropBugAbstractAI(World world, AbstractCreature parent)
		: base(world, parent)
	{
		if (!world.singleRoomWorld && Random.value < 0.25f && world.GetAbstractRoom(parent.pos) != null && !world.GetAbstractRoom(parent.pos).shelter)
		{
			AbstractPhysicalObject abstractPhysicalObject = null;
			if (Random.value < 0.5f)
			{
				abstractPhysicalObject = new AbstractConsumable(world, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null, parent.pos, world.game.GetNewID(), -1, -1, null);
			}
			else if (ModManager.MSC && Random.value < 0.25f && (world.region.name == "SH" || world.region.name == "MS" || world.region.name == "SL" || world.region.name == "VS" || world.region.name == "SB"))
			{
				abstractPhysicalObject = new AbstractConsumable(world, MoreSlugcatsEnums.AbstractObjectType.GlowWeed, null, parent.pos, world.game.GetNewID(), -1, -1, null);
			}
			else if (Random.value < 0.5f)
			{
				abstractPhysicalObject = new AbstractSpear(world, null, parent.pos, world.game.GetNewID(), Random.value < 0.5f);
			}
			else if (Random.value < 0.5f)
			{
				abstractPhysicalObject = new AbstractPhysicalObject(world, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null, parent.pos, world.game.GetNewID());
			}
			else if (Random.value < 0.5f)
			{
				abstractPhysicalObject = new DataPearl.AbstractDataPearl(world, AbstractPhysicalObject.AbstractObjectType.DataPearl, null, parent.pos, world.game.GetNewID(), -1, -1, null, DataPearl.AbstractDataPearl.DataPearlType.Misc);
			}
			else if (Random.value < 0.5f && world.game.StoryCharacter != SlugcatStats.Name.Red && (!ModManager.Expedition || !world.game.rainWorld.ExpeditionMode))
			{
				abstractPhysicalObject = new AbstractConsumable(world, AbstractPhysicalObject.AbstractObjectType.KarmaFlower, null, parent.pos, world.game.GetNewID(), -1, -1, null);
			}
			if (abstractPhysicalObject != null)
			{
				new AbstractPhysicalObject.CreatureGripStick(parent, abstractPhysicalObject, 0, carry: true);
			}
		}
	}

	public override bool DoIwantToDropThisItemInDen(AbstractPhysicalObject item)
	{
		return item is AbstractCreature;
	}
}
