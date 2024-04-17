public class CompetitiveGameSession : ArenaGameSession
{
	public CompetitiveGameSession(RainWorldGame game)
		: base(game)
	{
	}

	public override void SpawnCreatures()
	{
		base.SpawnCreatures();
		if (ModManager.MSC)
		{
			ArenaCreatureSpawner.allowLockedCreatures = false;
		}
		ArenaCreatureSpawner.SpawnArenaCreatures(game, base.GameTypeSetup.wildLifeSetting, ref arenaSitting.creatures, ref arenaSitting.multiplayerUnlocks);
	}

	public override void Initiate()
	{
		SpawnPlayers(base.room, null);
		base.Initiate();
		AddHUD();
	}

	public override void Update()
	{
		base.Update();
	}

	public override bool ShouldSessionEnd()
	{
		return thisFrameActivePlayers == 0;
	}
}
