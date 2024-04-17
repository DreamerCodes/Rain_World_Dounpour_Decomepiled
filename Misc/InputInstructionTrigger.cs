using UnityEngine;

public abstract class InputInstructionTrigger : UpdatableAndDeletable
{
	public Vector2 instructionPos;

	public bool activated;

	public bool completed;

	public int timeSinceActivated;

	public InputInstructionTrigger(Room room)
	{
		base.room = room;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (completed)
		{
			Destroy();
		}
		else if (activated)
		{
			timeSinceActivated++;
			for (int i = 0; i < room.abstractRoom.creatures.Count; i++)
			{
				if (room.abstractRoom.creatures[i].creatureTemplate.type == CreatureTemplate.Type.Overseer && (room.abstractRoom.creatures[i].abstractAI as OverseerAbstractAI).playerGuide && room.abstractRoom.creatures[i].abstractAI.RealAI != null && (room.abstractRoom.creatures[i].abstractAI.RealAI as OverseerAI).communication != null)
				{
					(room.abstractRoom.creatures[i].abstractAI.RealAI as OverseerAI).communication.PlayerNeedsInputInstruction(this);
					break;
				}
			}
		}
		else if (room.game.Players.Count > 0)
		{
			CheckTrigger(room.game.Players[0]);
		}
	}

	public virtual void CheckTrigger(AbstractCreature p)
	{
	}
}
