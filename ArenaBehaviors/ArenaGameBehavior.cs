namespace ArenaBehaviors;

public abstract class ArenaGameBehavior
{
	public ArenaGameSession gameSession;

	public bool slatedForDeletion;

	public RainWorldGame game => gameSession.game;

	public World world => gameSession.game.world;

	public Room room => gameSession.game.world.GetAbstractRoom(0).realizedRoom;

	public ArenaGameBehavior(ArenaGameSession gameSession)
	{
		this.gameSession = gameSession;
	}

	public virtual void Initiate()
	{
	}

	public virtual void Update()
	{
	}

	public virtual void Destroy()
	{
		slatedForDeletion = true;
	}
}
