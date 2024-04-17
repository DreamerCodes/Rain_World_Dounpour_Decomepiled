using RWCustom;
using UnityEngine;

public class PlayerPushback : UpdatableAndDeletable
{
	public PlacedObject placedObj;

	public Vector2 Pos => placedObj.pos;

	public float Rad => (placedObj.data as PlacedObject.ResizableObjectData).handlePos.magnitude;

	public Vector2 Dir => (placedObj.data as PlacedObject.ResizableObjectData).handlePos.normalized;

	public PlayerPushback(Room room, PlacedObject placedObj)
	{
		base.room = room;
		this.placedObj = placedObj;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		for (int i = 0; i < room.game.Players.Count; i++)
		{
			if (room.game.Players[i].realizedCreature == null || room.game.Players[i].realizedCreature.room != room || !room.game.Players[i].realizedCreature.Consious || room.game.Players[i].realizedCreature.grabbedBy.Count != 0)
			{
				continue;
			}
			for (int j = 0; j < room.game.Players[i].realizedCreature.bodyChunks.Length; j++)
			{
				if (Custom.DistLess(Custom.RestrictInRect(room.game.Players[i].realizedCreature.bodyChunks[j].pos, room.RoomRect), Pos, Rad))
				{
					room.game.Players[i].realizedCreature.bodyChunks[j].vel += Dir * 5f * Mathf.InverseLerp(Rad, Rad - 60f, Vector2.Distance(Custom.RestrictInRect(room.game.Players[i].realizedCreature.bodyChunks[j].pos, room.RoomRect), Pos));
				}
			}
		}
	}
}
