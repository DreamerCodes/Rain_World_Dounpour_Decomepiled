using MoreSlugcats;

namespace ArenaBehaviors;

public class StartBump : ArenaGameBehavior
{
	public int startGameCounter = 30;

	public StartBump(ArenaGameSession gameSession)
		: base(gameSession)
	{
	}

	public override void Update()
	{
		if (gameSession.counter < 5)
		{
			return;
		}
		if (base.room.ReadyForPlayer)
		{
			startGameCounter--;
		}
		if (startGameCounter == 0)
		{
			base.game.cameras[0].room.PlaySound(SoundID.UI_Multiplayer_Game_Start, 0f, 1f, 1f);
			if (ModManager.MSC && gameSession.arenaSitting.gameTypeSetup.gameType == MoreSlugcatsEnums.GameTypeID.Challenge)
			{
				base.game.cameras[0].hud.textPrompt.AddMessage(gameSession.arenaSitting.gameTypeSetup.challengeMeta.name, 20, 160, darken: false, hideHud: true);
			}
			else if (gameSession.arenaSitting.ShowLevelName)
			{
				base.game.cameras[0].hud.textPrompt.AddMessage(MultiplayerUnlocks.LevelDisplayName(gameSession.arenaSitting.GetCurrentLevel), 20, 160, darken: false, hideHud: true);
			}
			Destroy();
		}
	}
}
