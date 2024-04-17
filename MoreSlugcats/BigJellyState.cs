using UnityEngine;

namespace MoreSlugcats;

public class BigJellyState : HealthState
{
	public Vector2 HomePos;

	public Vector2 DriftPos;

	public bool bodyReleasedGoo;

	public Vector2[] deadArmDriftPos;

	public BigJellyState(AbstractCreature creature)
		: base(creature)
	{
		bodyReleasedGoo = false;
		HomePos = Vector2.zero;
		DriftPos = Vector2.zero;
	}
}
