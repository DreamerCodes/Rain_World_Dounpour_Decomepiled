namespace ScavTradeInstruction;

public class ScavengerTradeInstructionTrigger : InputInstructionTrigger
{
	public PlacedObject placedObject;

	public DataPearl pearl;

	public ScavengerTradeInstructionTrigger(Room room, PlacedObject placedObject)
		: base(room)
	{
		this.placedObject = placedObject;
		instructionPos = placedObject.pos;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		AbstractCreature firstAlivePlayer = room.game.FirstAlivePlayer;
		if (room.game.Players.Count > 0 && firstAlivePlayer != null && firstAlivePlayer.realizedCreature != null && firstAlivePlayer.realizedCreature.room != null && firstAlivePlayer.realizedCreature.room != room)
		{
			activated = false;
		}
	}

	public override void CheckTrigger(AbstractCreature p)
	{
		if (p.realizedCreature == null || p.realizedCreature.room != room)
		{
			return;
		}
		for (int i = 0; i < room.abstractRoom.entities.Count; i++)
		{
			if (room.abstractRoom.entities[i] is AbstractPhysicalObject && (room.abstractRoom.entities[i] as AbstractPhysicalObject).realizedObject != null && (room.abstractRoom.entities[i] as AbstractPhysicalObject).type == AbstractPhysicalObject.AbstractObjectType.DataPearl && (room.abstractRoom.entities[i] as DataPearl.AbstractDataPearl).originRoom == room.abstractRoom.index)
			{
				activated = true;
				pearl = (room.abstractRoom.entities[i] as AbstractPhysicalObject).realizedObject as DataPearl;
				break;
			}
		}
	}
}
