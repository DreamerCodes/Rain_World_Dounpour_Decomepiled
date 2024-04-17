using RWCustom;

public class BringGuideToPlayerEvent
{
	public static OverseerAbstractAI BringGuide(World world, float minLike)
	{
		if (world.game == null || world.overseersWorldAI == null || world.overseersWorldAI.playerGuide == null)
		{
			return null;
		}
		float likesPlayer = (world.game.session as StoryGameSession).saveState.miscWorldSaveData.playerGuideState.likesPlayer;
		if (likesPlayer < minLike)
		{
			return null;
		}
		(world.overseersWorldAI.playerGuide.abstractAI as OverseerAbstractAI).goToPlayer = true;
		(world.overseersWorldAI.playerGuide.abstractAI as OverseerAbstractAI).playerGuideCounter = (int)Custom.LerpMap(likesPlayer, -0.5f, 1f, 400f, 1800f);
		return world.overseersWorldAI.playerGuide.abstractAI as OverseerAbstractAI;
	}
}
