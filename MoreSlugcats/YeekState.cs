using UnityEngine;

namespace MoreSlugcats;

public class YeekState : HealthState
{
	private int hungerCounterMax;

	private int PreviousCycleHungerStart;

	public YeekState(AbstractCreature creature)
		: base(creature)
	{
		PreviousCycleHungerStart = 0;
		NewMaxHunger();
	}

	public float HungerIntensity(int CycleTimer)
	{
		return Mathf.InverseLerp(PreviousCycleHungerStart, PreviousCycleHungerStart + hungerCounterMax, CycleTimer);
	}

	public void Feed(int CycleTimer)
	{
		PreviousCycleHungerStart = CycleTimer;
		NewMaxHunger();
	}

	private void NewMaxHunger()
	{
		hungerCounterMax = Random.Range(2000, 5000);
	}
}
