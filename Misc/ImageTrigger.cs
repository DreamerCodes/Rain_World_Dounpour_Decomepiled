public static class ImageTrigger
{
	public static void AttemptTriggerFire(RainWorldGame game, Room room, ActiveTriggerChecker triggerChecker, ShowProjectedImageEvent imgEvent)
	{
		if (game.session is StoryGameSession)
		{
			if (imgEvent.afterEncounter != (game.session as StoryGameSession).saveState.miscWorldSaveData.SLOracleState.playerEncounters > 0)
			{
				triggerChecker.Destroy();
				return;
			}
			if ((game.session as StoryGameSession).saveState.miscWorldSaveData.playerGuideState.HasImageBeenShownInRoom(room.abstractRoom.name))
			{
				triggerChecker.Destroy();
				return;
			}
		}
		Overseer overseer = null;
		for (int i = 0; i < room.abstractRoom.creatures.Count; i++)
		{
			if (room.abstractRoom.creatures[i].creatureTemplate.type == CreatureTemplate.Type.Overseer && (room.abstractRoom.creatures[i].abstractAI as OverseerAbstractAI).playerGuide && room.abstractRoom.creatures[i].realizedCreature != null)
			{
				overseer = room.abstractRoom.creatures[i].realizedCreature as Overseer;
				break;
			}
		}
		if (overseer == null || overseer.AI == null || overseer.AI.communication == null)
		{
			return;
		}
		if (imgEvent.onlyWhenShowingDirection)
		{
			Player player;
			if (ModManager.CoopAvailable)
			{
				AbstractCreature firstAlivePlayer = room.game.FirstAlivePlayer;
				if (game.AlivePlayers.Count == 0 || firstAlivePlayer == null || firstAlivePlayer.realizedCreature == null)
				{
					return;
				}
				player = firstAlivePlayer.realizedCreature as Player;
			}
			else
			{
				if (game.Players.Count == 0 || game.Players[0].realizedCreature == null)
				{
					return;
				}
				player = game.Players[0].realizedCreature as Player;
			}
			if (!overseer.AI.communication.AnyProgressionDirection(player))
			{
				return;
			}
		}
		overseer.AI.communication.imageToShow = imgEvent;
		overseer.AI.communication.showImageRoom = room.abstractRoom.index;
		triggerChecker.Destroy();
	}
}
