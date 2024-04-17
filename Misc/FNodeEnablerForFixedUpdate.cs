public class FNodeEnablerForFixedUpdate : FNodeEnabler
{
	public Futile.FutileUpdateDelegate handleUpdateCallback;

	public FNodeEnablerForFixedUpdate(Futile.FutileUpdateDelegate handleUpdateCallback)
	{
		this.handleUpdateCallback = handleUpdateCallback;
	}

	public override void Connect()
	{
		Futile.instance.SignalFixedUpdate += handleUpdateCallback;
	}

	public override void Disconnect()
	{
		Futile.instance.SignalFixedUpdate -= handleUpdateCallback;
	}
}
