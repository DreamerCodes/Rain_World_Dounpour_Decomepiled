namespace MoreSlugcats;

public class HypothermiaTutorial : UpdatableAndDeletable
{
	public int message;

	public HypothermiaTutorial(Room room)
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
				room.game.cameras[0].hud.textPrompt.AddMessage(room.game.manager.rainWorld.inGameTranslator.Translate("Only warm objects can rejuvenate your body heat in this extreme cold, but they will not last forever!"), 120, 160, darken: false, ModManager.MMF);
				message++;
				break;
			case 1:
				room.game.rainWorld.progression.miscProgressionData.starvationTutorialCounter = -1;
				Destroy();
				break;
			}
		}
	}
}
