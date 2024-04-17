public class UpdatableAndDeletable
{
	public bool evenUpdate;

	public Room room;

	public bool slatedForDeletetion { get; set; }

	public virtual void Update(bool eu)
	{
		evenUpdate = eu;
	}

	public virtual void PausedUpdate()
	{
	}

	public void RemoveFromRoom()
	{
		room = null;
	}

	public virtual void Destroy()
	{
		slatedForDeletetion = true;
	}
}
