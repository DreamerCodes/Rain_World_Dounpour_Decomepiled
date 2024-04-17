using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class ScavengerTreasury : UpdatableAndDeletable
{
	public PlacedObject placedObj;

	public List<AbstractPhysicalObject> property;

	public List<IntVector2> tiles;

	public float Rad => (placedObj.data as PlacedObject.ResizableObjectData).handlePos.magnitude;

	public ScavengerTreasury(Room room, PlacedObject placedObj)
	{
		base.room = room;
		this.placedObj = placedObj;
		property = new List<AbstractPhysicalObject>();
		tiles = new List<IntVector2>();
		IntVector2 tilePosition = room.GetTilePosition(placedObj.pos);
		int num = (int)(Rad / 20f);
		for (int i = tilePosition.x - num; i <= tilePosition.x + num; i++)
		{
			for (int j = tilePosition.y - num; j <= tilePosition.y + num; j++)
			{
				if (Custom.DistLess(room.MiddleOfTile(i, j), placedObj.pos, Rad) && !room.GetTile(i, j).Solid && !room.GetTile(i, j + 1).Solid && room.GetTile(i, j - 1).Solid)
				{
					tiles.Add(new IntVector2(i, j));
				}
			}
		}
		if (!room.abstractRoom.firstTimeRealized)
		{
			return;
		}
		bool flag = room.world.region != null && (room.world.region.name == "SH" || room.world.region.name == "SB");
		float num2 = 0f;
		float num3 = 0f;
		if (ModManager.MSC && room.game.IsStorySession)
		{
			if (room.game.GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Artificer || room.game.GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Spear)
			{
				num2 = 0.5f;
				num3 = 0.75f;
			}
			if (room.game.GetStorySession.saveStateNumber == SlugcatStats.Name.Red || room.game.GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
			{
				num2 = 0.05f;
				num3 = 0.25f;
			}
			if (room.game.GetStorySession.saveStateNumber == SlugcatStats.Name.White || room.game.GetStorySession.saveStateNumber == SlugcatStats.Name.Yellow)
			{
				num2 = 0.02f;
				num3 = 0.01f;
			}
			if (room.world.region != null && room.world.region.name == "LC")
			{
				num2 = 0.75f;
				num3 = 1f;
			}
		}
		for (int k = 0; k < tiles.Count; k++)
		{
			if (!(Random.value < Mathf.InverseLerp(Rad, Rad / 5f, Vector2.Distance(room.MiddleOfTile(tiles[k]), placedObj.pos))))
			{
				continue;
			}
			AbstractPhysicalObject abstractPhysicalObject = null;
			if (Random.value < 0.1f)
			{
				abstractPhysicalObject = new DataPearl.AbstractDataPearl(room.world, AbstractPhysicalObject.AbstractObjectType.DataPearl, null, room.GetWorldCoordinate(tiles[k]), room.game.GetNewID(), -1, -1, null, DataPearl.AbstractDataPearl.DataPearlType.Misc);
			}
			else if (Random.value < 1f / 7f)
			{
				abstractPhysicalObject = new AbstractPhysicalObject(room.world, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null, room.GetWorldCoordinate(tiles[k]), room.game.GetNewID());
			}
			else if (Random.value < 1f / (flag ? 5f : 20f))
			{
				abstractPhysicalObject = new AbstractPhysicalObject(room.world, AbstractPhysicalObject.AbstractObjectType.Lantern, null, room.GetWorldCoordinate(tiles[k]), room.game.GetNewID());
			}
			else
			{
				abstractPhysicalObject = new AbstractSpear(room.world, null, room.GetWorldCoordinate(tiles[k]), room.game.GetNewID(), Random.value < 0.75f);
				if (ModManager.MSC && Random.value < num2)
				{
					(abstractPhysicalObject as AbstractSpear).explosive = false;
					(abstractPhysicalObject as AbstractSpear).electric = true;
					if (Random.value >= num3)
					{
						(abstractPhysicalObject as AbstractSpear).electricCharge = 0;
					}
				}
			}
			property.Add(abstractPhysicalObject);
			if (abstractPhysicalObject != null)
			{
				room.abstractRoom.entities.Add(abstractPhysicalObject);
			}
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (property.Count < 1)
		{
			return;
		}
		AbstractPhysicalObject abstractPhysicalObject = property[Random.Range(0, property.Count)];
		if (abstractPhysicalObject.slatedForDeletion)
		{
			property.Remove(abstractPhysicalObject);
		}
		else
		{
			if (abstractPhysicalObject.realizedObject == null)
			{
				return;
			}
			if (abstractPhysicalObject.realizedObject.room != room)
			{
				property.Remove(abstractPhysicalObject);
			}
			else
			{
				if (abstractPhysicalObject.realizedObject.grabbedBy.Count <= 0)
				{
					return;
				}
				if (abstractPhysicalObject.realizedObject.grabbedBy[0].grabber is Player)
				{
					room.socialEventRecognizer.AddStolenProperty(abstractPhysicalObject.ID);
					for (int i = 0; i < room.abstractRoom.creatures.Count; i++)
					{
						if (room.abstractRoom.creatures[i].creatureTemplate.type == CreatureTemplate.Type.Scavenger && room.abstractRoom.creatures[i].realizedCreature != null && room.abstractRoom.creatures[i].realizedCreature.Consious)
						{
							float num = room.game.session.creatureCommunities.LikeOfPlayer(CreatureCommunities.CommunityID.Scavengers, room.game.world.RegionNumber, (abstractPhysicalObject.realizedObject.grabbedBy[0].grabber as Player).playerState.playerNumber);
							if (num < 0.9f)
							{
								room.game.session.creatureCommunities.InfluenceLikeOfPlayer(CreatureCommunities.CommunityID.Scavengers, room.game.world.RegionNumber, (abstractPhysicalObject.realizedObject.grabbedBy[0].grabber as Player).playerState.playerNumber, Custom.LerpMap(num, -0.5f, 0.9f, -0.3f, 0f), 0.5f, 0f);
								Custom.Log("treasury theft noticed!");
							}
						}
					}
				}
				property.Remove(abstractPhysicalObject);
			}
		}
	}
}
