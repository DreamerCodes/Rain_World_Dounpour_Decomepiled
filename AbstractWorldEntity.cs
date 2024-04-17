public class AbstractWorldEntity
{
	public WorldCoordinate pos;

	public World world;

	public int timeSpentHere;

	public bool evenUpdate;

	public bool InDen;

	public bool slatedForDeletion;

	public EntityID ID;

	public AbstractRoom Room => world.GetAbstractRoom(pos.room);

	public AbstractWorldEntity(World world, WorldCoordinate pos, EntityID ID)
	{
		this.world = world;
		this.pos = pos;
		this.ID = ID;
	}

	public virtual void Update(int time)
	{
		timeSpentHere += time;
	}

	public virtual void IsEnteringDen(WorldCoordinate den)
	{
		InDen = true;
	}

	public virtual void IsExitingDen()
	{
		InDen = false;
	}

	public virtual void Abstractize(WorldCoordinate coord)
	{
	}

	public virtual void Destroy()
	{
		slatedForDeletion = true;
	}
}
