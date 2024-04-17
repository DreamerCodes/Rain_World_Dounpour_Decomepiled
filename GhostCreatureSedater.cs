using System;
using MoreSlugcats;

public class GhostCreatureSedater : UpdatableAndDeletable
{
	private WorldCoordinate? den;

	public GhostCreatureSedater(Room spawnRoom)
	{
		for (int i = 0; i < spawnRoom.abstractRoom.nodes.Length; i++)
		{
			if (spawnRoom.abstractRoom.nodes[i].type == AbstractRoomNode.Type.Den)
			{
				den = new WorldCoordinate(spawnRoom.abstractRoom.index, -1, -1, i);
				break;
			}
		}
		for (int j = 0; j < spawnRoom.abstractRoom.creatures.Count; j++)
		{
			if (spawnRoom.abstractRoom.creatures[j].creatureTemplate.type != CreatureTemplate.Type.Slugcat)
			{
				spawnRoom.abstractRoom.creatures[j].InDen = true;
				spawnRoom.abstractRoom.creatures[j].remainInDenCounter = -1;
			}
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (ModManager.MSC && room.game.rainWorld.safariMode)
		{
			return;
		}
		for (int i = 0; i < room.abstractRoom.creatures.Count; i++)
		{
			if (ModManager.MMF && den.HasValue && room.abstractRoom.creatures[i].creatureTemplate.type != CreatureTemplate.Type.Slugcat && room.abstractRoom.creatures[i].creatureTemplate.UseAnyRoomBorderExit)
			{
				if (room.abstractRoom.creatures[i].realizedCreature != null)
				{
					room.abstractRoom.creatures[i].realizedCreature.Destroy();
				}
				room.abstractRoom.creatures[i].Move(den.Value);
				room.abstractRoom.creatures[i].Room.MoveEntityToDen(room.abstractRoom.creatures[i]);
			}
			else if (room.abstractRoom.creatures[i].realizedCreature != null && room.abstractRoom.creatures[i].creatureTemplate.type != CreatureTemplate.Type.Slugcat && (!ModManager.MSC || room.abstractRoom.creatures[i].creatureTemplate.type != MoreSlugcatsEnums.CreatureTemplateType.SlugNPC))
			{
				room.abstractRoom.creatures[i].realizedCreature.stun = Math.Max(room.abstractRoom.creatures[i].realizedCreature.stun, 20);
				bool flag = !room.abstractRoom.creatures[i].realizedCreature.dead && room.abstractRoom.creatures[i].realizedCreature.grabbedBy.Count == 0 && room.abstractRoom.creatures[i].pos.abstractNode != -1 && room.abstractRoom.nodes[room.abstractRoom.creatures[i].pos.abstractNode].type != AbstractRoomNode.Type.Exit;
				if (den.HasValue && (!ModManager.MMF || flag))
				{
					room.abstractRoom.creatures[i].Move(den.Value);
					room.abstractRoom.creatures[i].Room.MoveEntityToDen(room.abstractRoom.creatures[i]);
				}
			}
		}
	}
}
