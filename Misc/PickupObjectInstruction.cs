using UnityEngine;

public class PickupObjectInstruction : InputInstructionTrigger
{
	public PhysicalObject item;

	public bool playerHasPickedUpAnItem;

	public bool markInstructionAsFollowed;

	public PickupObjectInstruction(Room room)
		: base(room)
	{
		base.room = room;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (room.game.Players.Count < 1 || room.game.Players[0].realizedCreature == null || room.game.Players[0].realizedCreature.room == null)
		{
			return;
		}
		if (room.game.Players[0].realizedCreature.room != room)
		{
			Destroy();
			return;
		}
		Player player = room.game.Players[0].realizedCreature as Player;
		if (playerHasPickedUpAnItem)
		{
			instructionPos = player.mainBodyChunk.pos;
			if (item == null || item.grabbedBy.Count == 0 || player.enteringShortCut.HasValue)
			{
				completed = true;
				Destroy();
			}
			return;
		}
		for (int i = 0; i < player.grasps.Length; i++)
		{
			if (player.grasps[i] != null && (player.grasps[i].grabbed.abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.Rock || player.grasps[i].grabbed.abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.Spear))
			{
				item = player.grasps[i].grabbed;
				playerHasPickedUpAnItem = true;
				markInstructionAsFollowed = true;
				return;
			}
		}
		float num = float.MaxValue;
		for (int j = 0; j < room.abstractRoom.entities.Count; j++)
		{
			if (room.abstractRoom.entities[j] is AbstractPhysicalObject && ((room.abstractRoom.entities[j] as AbstractPhysicalObject).type == AbstractPhysicalObject.AbstractObjectType.Rock || (room.abstractRoom.entities[j] as AbstractPhysicalObject).type == AbstractPhysicalObject.AbstractObjectType.Spear) && (room.abstractRoom.entities[j] as AbstractPhysicalObject).realizedObject != null && ((room.abstractRoom.entities[j] as AbstractPhysicalObject).realizedObject as Weapon).mode == Weapon.Mode.Free && (room.abstractRoom.entities[j] as AbstractPhysicalObject).realizedObject.grabbedBy.Count == 0)
			{
				float num2 = Vector2.Distance((room.abstractRoom.entities[j] as AbstractPhysicalObject).realizedObject.firstChunk.pos, player.mainBodyChunk.pos);
				if (room.ViewedByAnyCamera((room.abstractRoom.entities[j] as AbstractPhysicalObject).realizedObject.firstChunk.pos, -20f))
				{
					num2 /= 2f;
				}
				if ((room.abstractRoom.entities[j] as AbstractPhysicalObject).realizedObject == item)
				{
					num2 /= 2f;
				}
				if (num2 < num)
				{
					num = num2;
					item = (room.abstractRoom.entities[j] as AbstractPhysicalObject).realizedObject;
				}
			}
		}
		if (item == null)
		{
			Destroy();
		}
		else
		{
			instructionPos = item.firstChunk.pos;
		}
	}

	public override void CheckTrigger(AbstractCreature p)
	{
		activated = item != null;
	}
}
