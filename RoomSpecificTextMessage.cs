using MoreSlugcats;

public class RoomSpecificTextMessage : UpdatableAndDeletable
{
	public RoomSpecificTextMessage(Room room)
	{
		base.room = room;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		string name = room.abstractRoom.name;
		if (name != null && name == "SL_C05" && room.game.session is StoryGameSession && (!ModManager.MMF || MMF.cfgExtraTutorials.Value) && (!ModManager.MSC || room.game.StoryCharacter == SlugcatStats.Name.White || room.game.StoryCharacter == SlugcatStats.Name.Yellow) && !(room.game.session as StoryGameSession).saveState.deathPersistentSaveData.DangleFruitInWaterMessage)
		{
			room.game.cameras[0].hud.textPrompt.AddMessage(room.game.rainWorld.inGameTranslator.Translate("Dropping food in the water might attract creatures."), 0, 200, darken: true, hideHud: true);
			(room.game.session as StoryGameSession).saveState.deathPersistentSaveData.DangleFruitInWaterMessage = true;
		}
	}
}
