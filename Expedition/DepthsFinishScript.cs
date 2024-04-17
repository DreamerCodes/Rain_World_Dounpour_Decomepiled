using MoreSlugcats;

namespace Expedition;

public class DepthsFinishScript : UpdatableAndDeletable
{
	public bool triggered;

	public DepthsFinishScript(Room room)
	{
		base.room = room;
		ExpLog.Log("DepthsFinishScript added to: " + room.abstractRoom.name);
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (room == null)
		{
			return;
		}
		if (room.shortCutsReady && room.abstractRoom.name == "SB_A14")
		{
			room.shortcuts[0].shortCutType = ShortcutData.Type.DeadEnd;
		}
		if (triggered)
		{
			return;
		}
		for (int i = 0; i < room.updateList.Count; i++)
		{
			if (room.updateList[i] is RoomSpecificScript.SB_A14KarmaIncrease)
			{
				(room.updateList[i] as RoomSpecificScript.SB_A14KarmaIncrease).Destroy();
			}
			if (ModManager.MSC && room.updateList[i] is MSCRoomSpecificScript.VS_E05WrapAround)
			{
				(room.updateList[i] as MSCRoomSpecificScript.VS_E05WrapAround).Destroy();
			}
			if (ExpeditionData.activeMission != "")
			{
				break;
			}
			if (room.abstractRoom.name == "SB_A14" && room.updateList[i] is Player && (room.updateList[i] as Player).mainBodyChunk.pos.x < 550f)
			{
				ExpeditionGame.voidSeaFinish = true;
				ExpeditionGame.expeditionComplete = true;
				triggered = true;
			}
			if (room.abstractRoom.name == "SB_E05SAINT" && room.updateList[i] is Player && (room.updateList[i] as Player).mainBodyChunk.pos.y < 0f)
			{
				ExpeditionGame.voidSeaFinish = true;
				ExpeditionGame.expeditionComplete = true;
				triggered = true;
			}
		}
	}
}
