public class FNodeEnablerForUpdate : FNodeEnabler
{
	public Futile.FutileUpdateDelegate handleUpdateCallback;

	public FNodeEnablerForUpdate(Futile.FutileUpdateDelegate handleUpdateCallback)
	{
		this.handleUpdateCallback = handleUpdateCallback;
	}

	public override void Connect()
	{
		Futile.instance.SignalUpdate += handleUpdateCallback;
	}

	public override void Disconnect()
	{
		Futile.instance.SignalUpdate -= handleUpdateCallback;
	}
}
