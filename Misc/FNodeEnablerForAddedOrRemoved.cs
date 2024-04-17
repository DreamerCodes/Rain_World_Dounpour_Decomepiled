public class FNodeEnablerForAddedOrRemoved : FNodeEnabler
{
	public delegate void Delegate(bool wasAdded);

	public Delegate handleAddedOrRemovedCallback;

	public FNodeEnablerForAddedOrRemoved(Delegate handleAddedOrRemovedCallback)
	{
		this.handleAddedOrRemovedCallback = handleAddedOrRemovedCallback;
	}

	public override void Connect()
	{
		handleAddedOrRemovedCallback(wasAdded: true);
	}

	public override void Disconnect()
	{
		handleAddedOrRemovedCallback(wasAdded: false);
	}
}
