namespace ArenaBehaviors;

public class EarlyRain : ArenaGameBehavior
{
	public int earlyRainCounter = -1;

	public EarlyRain(ArenaGameSession gameSession)
		: base(gameSession)
	{
	}

	public override void Update()
	{
		if (gameSession.sessionEnded)
		{
			return;
		}
		if (gameSession.Players.Count > 1 && gameSession.thisFrameActivePlayers == 1 && gameSession.endSessionCounter == -1)
		{
			if (earlyRainCounter == -1)
			{
				earlyRainCounter = ((gameSession.Players.Count > 2) ? 15 : 120);
			}
			else
			{
				if (earlyRainCounter <= 0)
				{
					return;
				}
				earlyRainCounter--;
				if (earlyRainCounter == 0)
				{
					if (gameSession.noRain != null)
					{
						gameSession.RemoveBehavior(gameSession.noRain);
					}
					gameSession.game.world.rainCycle.ArenaEndSessionRain();
				}
			}
		}
		else
		{
			earlyRainCounter = -1;
		}
	}
}
