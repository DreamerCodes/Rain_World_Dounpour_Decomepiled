public class FNodeEnablerForLateUpdate : FNodeEnabler
{
	public Futile.FutileUpdateDelegate handleUpdateCallback;

	public FNodeEnablerForLateUpdate(Futile.FutileUpdateDelegate handleUpdateCallback)
	{
		this.handleUpdateCallback = handleUpdateCallback;
	}

	public override void Connect()
	{
		Futile.instance.SignalLateUpdate += handleUpdateCallback;
	}

	public override void Disconnect()
	{
		Futile.instance.SignalLateUpdate -= handleUpdateCallback;
	}
}
