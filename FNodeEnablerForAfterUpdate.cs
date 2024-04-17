public class FNodeEnablerForAfterUpdate : FNodeEnabler
{
	public Futile.FutileUpdateDelegate handleUpdateCallback;

	public FNodeEnablerForAfterUpdate(Futile.FutileUpdateDelegate handleUpdateCallback)
	{
		this.handleUpdateCallback = handleUpdateCallback;
	}

	public override void Connect()
	{
		Futile.instance.SignalAfterUpdate += handleUpdateCallback;
	}

	public override void Disconnect()
	{
		Futile.instance.SignalAfterUpdate -= handleUpdateCallback;
	}
}
