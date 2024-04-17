using RWCustom;
using UnityEngine;

public class SuperJumpInstruction : InputInstructionTrigger
{
	public PlacedObject placedObject;

	public Vector2 TrgA => placedObject.pos + (placedObject.data as PlacedObject.QuadObjectData).handles[2];

	public Vector2 TrgB => placedObject.pos + (placedObject.data as PlacedObject.QuadObjectData).handles[1];

	public SuperJumpInstruction(Room room, PlacedObject placedObject)
		: base(room)
	{
		this.placedObject = placedObject;
		instructionPos = placedObject.pos;
	}

	public override void CheckTrigger(AbstractCreature p)
	{
		if (p.realizedCreature == null || p.realizedCreature.room != room)
		{
			return;
		}
		Player player = p.realizedCreature as Player;
		if (!(player.mainBodyChunk.lastPos.y < player.mainBodyChunk.pos.y))
		{
			Vector2 v = Custom.LineIntersection(player.mainBodyChunk.lastPos, player.mainBodyChunk.pos, TrgA, TrgB);
			if (Custom.IsPointBetweenPoints(TrgA, TrgB, v) && Custom.IsPointBetweenPoints(player.mainBodyChunk.lastPos, player.mainBodyChunk.pos, v))
			{
				activated = true;
				Custom.Log("Instruction Activated");
			}
		}
	}
}
