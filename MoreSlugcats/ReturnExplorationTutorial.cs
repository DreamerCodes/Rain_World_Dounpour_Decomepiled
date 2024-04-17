namespace MoreSlugcats;

public class ReturnExplorationTutorial : UpdatableAndDeletable
{
	public int message;

	public ReturnExplorationTutorial(Room room)
	{
		base.room = room;
		if (room.game.session.Players[0].pos.room != room.abstractRoom.index)
		{
			Destroy();
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (room.game.session.Players[0].realizedCreature != null && room.game.cameras[0].hud != null && room.game.cameras[0].hud.textPrompt != null && room.game.cameras[0].hud.textPrompt.messages.Count < 1)
		{
			switch (message)
			{
			case 0:
				room.game.cameras[0].hud.textPrompt.AddMessage(room.game.manager.rainWorld.inGameTranslator.Translate("There are many paths to explore in this world."), 120, 160, darken: false, hideHud: true);
				message++;
				break;
			case 1:
				room.game.cameras[0].hud.textPrompt.AddMessage(room.game.manager.rainWorld.inGameTranslator.Translate("If you are struggling, try returning to a previous region."), 0, 160, darken: false, hideHud: true);
				message++;
				break;
			case 2:
				room.game.cameras[0].hud.textPrompt.AddMessage(room.game.manager.rainWorld.inGameTranslator.Translate("You may find a new path forward, or discover something that makes your survival here easier."), 0, 160, darken: false, hideHud: true);
				message++;
				break;
			case 3:
				room.game.cameras[0].hud.textPrompt.AddMessage(room.game.manager.rainWorld.inGameTranslator.Translate("However, there is nothing that stops you from overcoming this region's challenges as you are right now."), 0, 160, darken: false, hideHud: true);
				message++;
				break;
			case 4:
				room.game.rainWorld.progression.miscProgressionData.returnExplorationTutorialCounter = -1;
				Destroy();
				break;
			}
		}
	}
}
