using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class InjuryTracker : AIModule
{
	private float sCruveSlope;

	public InjuryTracker(ArtificialIntelligence AI, float sCruveSlope)
		: base(AI)
	{
		this.sCruveSlope = sCruveSlope;
	}

	public override float Utility()
	{
		if (ModManager.MMF && MMF.cfgNoArenaFleeing.Value && AI.creature.world.game.IsArenaSession)
		{
			return 0f;
		}
		if (ModManager.MSC && AI.creature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.ScavengerKing)
		{
			return 0f;
		}
		return Custom.SCurve(Mathf.InverseLerp(0.2f, 0.9f, 1f - Mathf.Clamp((AI.creature.state as HealthState).health, 0f, 1f)), sCruveSlope);
	}
}
