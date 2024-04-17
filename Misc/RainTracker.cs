using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class RainTracker : AIModule
{
	private RainCycle rainCycle;

	public RainTracker(ArtificialIntelligence AI)
		: base(AI)
	{
		rainCycle = AI.creature.world.rainCycle;
	}

	public override void Update()
	{
	}

	public override float Utility()
	{
		if (AI.creature != null && (AI.creature.nightCreature || AI.creature.ignoreCycle))
		{
			return 0f;
		}
		if (ModManager.MMF && AI.creature != null && AI.creature.realizedCreature != null && AI.creature.realizedCreature.GrabbedByDaddyCorruption)
		{
			return 1f;
		}
		if (ModManager.MMF && MMF.cfgDeerBehavior.Value && AI.creature != null && AI.creature.realizedCreature != null && AI.creature.realizedCreature is Deer && (AI.creature.realizedCreature as Deer).playersInAntlers.Count > 0)
		{
			return 0f;
		}
		if (ModManager.MSC && rainCycle.preTimer > 0)
		{
			float num = ((AI.creature == null || AI.creature.Room.realizedRoom == null) ? 0f : ((!(AI.creature.Room.realizedRoom.roomSettings.DangerType == RoomRain.DangerType.FloodAndRain) && !(AI.creature.Room.realizedRoom.roomSettings.DangerType == RoomRain.DangerType.Rain)) ? 0f : Mathf.Clamp((1.75f + rainCycle.preCycleRain_Intensity) / 4f, 0f, 1f)));
			if (num < 0.6f)
			{
				num = 0f;
			}
			return num;
		}
		if (AI.creature.world.game.IsStorySession)
		{
			return Mathf.InverseLerp(1f, 0f, Custom.SCurve(Mathf.InverseLerp(800f, 4000f, rainCycle.TimeUntilRain), 0.1f));
		}
		return Mathf.InverseLerp(1f, 0f, Custom.SCurve(Mathf.InverseLerp(80f, 400f, rainCycle.TimeUntilRain), 0.5f));
	}
}
