using RWCustom;
using UnityEngine;

public class RoomBorderPushBack : UpdatableAndDeletable
{
	public float margin;

	public float leftmostCameraPos;

	public float rightmostCameraPos;

	public float[] pushPower;

	public RoomBorderPushBack(Room room)
	{
		base.room = room;
		margin = (room.game.rainWorld.options.ScreenSize.x - 1400f) / 2f;
		margin -= 100f;
		leftmostCameraPos = float.MaxValue;
		rightmostCameraPos = float.MinValue;
		for (int i = 0; i < room.cameraPositions.Length; i++)
		{
			leftmostCameraPos = Mathf.Min(leftmostCameraPos, room.cameraPositions[i].x);
			rightmostCameraPos = Mathf.Max(rightmostCameraPos, room.cameraPositions[i].x);
		}
		pushPower = new float[room.game.Players.Count];
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (pushPower.Length < room.game.Players.Count)
		{
			pushPower = new float[room.game.Players.Count];
		}
		for (int i = 0; i < room.game.Players.Count; i++)
		{
			bool flag = false;
			if (room.game.Players[i].realizedCreature != null && room.game.Players[i].realizedCreature.room == room && room.game.Players[i].realizedCreature.grabbedBy.Count == 0)
			{
				if (room.game.Players[i].realizedCreature.mainBodyChunk.pos.x < leftmostCameraPos - margin)
				{
					PushAtCreature(3f * Mathf.InverseLerp(0.3f, 1f, pushPower[i]), room.game.Players[i].realizedCreature);
					flag = true;
				}
				else if (room.game.Players[i].realizedCreature.mainBodyChunk.pos.x > rightmostCameraPos + 1400f + margin)
				{
					PushAtCreature(-3f * Mathf.InverseLerp(0.3f, 1f, pushPower[i]), room.game.Players[i].realizedCreature);
					flag = true;
				}
			}
			pushPower[i] = Custom.LerpAndTick(pushPower[i], flag ? 1f : 0f, 0.01f, 1f / (flag ? 600f : 80f));
		}
	}

	private void PushAtCreature(float psh, Creature crit)
	{
		for (int i = 0; i < crit.bodyChunks.Length; i++)
		{
			crit.bodyChunks[i].vel.x += psh;
		}
		for (int j = 0; j < crit.grasps.Length; j++)
		{
			if (crit.grasps[j] != null)
			{
				for (int k = 0; k < crit.grasps[j].grabbed.bodyChunks.Length; k++)
				{
					crit.grasps[j].grabbed.bodyChunks[k].vel.x += psh;
				}
			}
		}
	}
}
