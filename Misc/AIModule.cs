public class AIModule
{
	public ArtificialIntelligence AI;

	public AIModule(ArtificialIntelligence AI)
	{
		this.AI = AI;
	}

	public virtual void Update()
	{
	}

	public virtual void NewRoom(Room room)
	{
	}

	public virtual float Utility()
	{
		return 0f;
	}
}
