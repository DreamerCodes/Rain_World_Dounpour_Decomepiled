namespace MoreSlugcats;

public class MaulingTutorial : UpdatableAndDeletable
{
	public int message;

	public MaulingTutorial(Room room)
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
				room.game.cameras[0].hud.textPrompt.AddMessage(room.game.manager.rainWorld.inGameTranslator.Translate("Artificer can maul creatures while they are still alive."), 120, 160, darken: false, hideHud: true);
				message++;
				break;
			case 1:
				room.game.cameras[0].hud.textPrompt.AddMessage(room.game.manager.rainWorld.inGameTranslator.Translate("Hold the EAT button while a stunned creature is being held."), 0, 160, darken: false, hideHud: true);
				message++;
				break;
			case 2:
				room.game.rainWorld.progression.miscProgressionData.starvationTutorialCounter = -1;
				Destroy();
				break;
			}
		}
	}
}
