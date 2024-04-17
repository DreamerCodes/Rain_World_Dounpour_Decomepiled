using System.Collections.Generic;

public class GarbageWormState : CreatureState
{
	public float bodySize = 1f;

	public List<EntityID> angryAt;

	public GarbageWormState(AbstractCreature crit, float bodySize)
		: base(crit)
	{
		this.bodySize = bodySize;
		angryAt = new List<EntityID>();
	}
}
