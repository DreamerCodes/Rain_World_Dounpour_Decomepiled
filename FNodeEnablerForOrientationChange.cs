public class FNodeEnablerForOrientationChange : FNodeEnabler
{
	public FScreen.ScreenOrientationChangeDelegate handleOrientationChangeCallback;

	public FNodeEnablerForOrientationChange(FScreen.ScreenOrientationChangeDelegate handleOrientationChangeCallback)
	{
		this.handleOrientationChangeCallback = handleOrientationChangeCallback;
	}

	public override void Connect()
	{
		Futile.screen.SignalOrientationChange += handleOrientationChangeCallback;
	}

	public override void Disconnect()
	{
		Futile.screen.SignalOrientationChange -= handleOrientationChangeCallback;
	}
}
