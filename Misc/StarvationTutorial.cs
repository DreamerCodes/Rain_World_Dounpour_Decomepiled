public class StarvationTutorial : UpdatableAndDeletable
{
	public int message;

	public StarvationTutorial(Room room)
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
				room.game.cameras[0].hud.textPrompt.AddMessage(room.game.manager.rainWorld.inGameTranslator.Translate("It is possible to hibernate with less than sufficient food"), 120, 160, darken: false, ModManager.MMF);
				message++;
				break;
			case 1:
				room.game.cameras[0].hud.textPrompt.AddMessage(room.game.manager.rainWorld.inGameTranslator.Translate("However, the game will not be saved"), 0, 160, darken: false, ModManager.MMF);
				message++;
				break;
			case 2:
				room.game.cameras[0].hud.textPrompt.AddMessage(room.game.manager.rainWorld.inGameTranslator.Translate("There are also other significant drawbacks"), 0, 120, darken: false, ModManager.MMF);
				message++;
				break;
			case 3:
				room.game.cameras[0].hud.textPrompt.AddMessage(room.game.manager.rainWorld.inGameTranslator.Translate("In a shelter, hold DOWN for several seconds to force hibernate"), 0, 160, darken: false, ModManager.MMF);
				message++;
				break;
			case 4:
				room.game.rainWorld.progression.miscProgressionData.starvationTutorialCounter = -1;
				Destroy();
				break;
			}
		}
	}
}
