using UnityEngine;

namespace ArenaBehaviors;

public class RespawnFlies : ArenaGameBehavior
{
	public RespawnFlies(ArenaGameSession gameSession)
		: base(gameSession)
	{
	}

	public override void Update()
	{
		if ((gameSession.earlyRain != null && gameSession.earlyRain.earlyRainCounter > -1) || Random.value > 0.0125f || (ModManager.MSC && base.world.rainCycle.preTimer > 0) || base.world.rainCycle.TimeUntilRain < 800 || !base.world.GetAbstractRoom(0).realizedRoom.quantifiedCreaturesPlaced || base.game.pauseMenu != null)
		{
			return;
		}
		int num = Random.Range(gameSession.Players.Count, Random.Range(gameSession.Players.Count, Random.Range(6, 10)));
		int num2 = 0;
		for (int i = 0; i < base.world.GetAbstractRoom(0).creatures.Count; i++)
		{
			if (base.world.GetAbstractRoom(0).creatures[i].creatureTemplate.type == CreatureTemplate.Type.Fly && base.world.GetAbstractRoom(0).creatures[i].state.alive)
			{
				num2++;
				if (num2 >= num)
				{
					num2 = -1;
					break;
				}
			}
		}
		if (num2 > -1)
		{
			base.world.fliesWorldAI.RespawnOneFly();
		}
	}
}
