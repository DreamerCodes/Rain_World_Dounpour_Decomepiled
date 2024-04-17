using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class StowawayBugState : HealthState
{
	public Vector2 HomePos;

	public Vector2 aimPos;

	private AbstractCreature creature;

	private bool debugForceAwake;

	private int digestionTimeStart;

	private int digestionLength;

	public StowawayBugState(AbstractCreature creature)
		: base(creature)
	{
		this.creature = creature;
		debugForceAwake = false;
	}

	public bool AwakeThisCycle(int cycle)
	{
		bool flag = debugForceAwake || creature.ID.RandomSeed * 42 % 13 == cycle % 32;
		if (!flag)
		{
			while (!flag)
			{
				flag = debugForceAwake || creature.ID.RandomSeed * 42 % 13 == cycle % 32;
				cycle++;
			}
			Custom.Log("Stowaway not awake, will wake on cycle", cycle.ToString());
			return false;
		}
		digestionTimeStart = 0;
		digestionLength = Random.Range(100, 6000);
		return true;
	}

	public void StartDigestion(int cycleTime)
	{
		digestionTimeStart = cycleTime;
		digestionLength = Random.Range(1000, 3000);
	}

	public bool CurrentlyDigesting(int cycleTime)
	{
		return cycleTime > digestionTimeStart + digestionLength;
	}
}
