using UnityEngine;

namespace ArenaBehaviors;

public class NoRain : ArenaGameBehavior
{
	public NoRain(ArenaGameSession gameSession)
		: base(gameSession)
	{
	}

	public override void Update()
	{
		gameSession.game.world.rainCycle.cycleLength = 4800;
		gameSession.game.world.rainCycle.timer = Mathf.Min(gameSession.game.world.rainCycle.timer, 2400);
	}
}
