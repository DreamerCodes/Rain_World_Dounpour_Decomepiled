public class FNodeEnablerForResize : FNodeEnabler
{
	public FScreen.ScreenResizeDelegate handleResizeCallback;

	public FNodeEnablerForResize(FScreen.ScreenResizeDelegate handleResizeCallback)
	{
		this.handleResizeCallback = handleResizeCallback;
	}

	public override void Connect()
	{
		Futile.screen.SignalResize += handleResizeCallback;
	}

	public override void Disconnect()
	{
		Futile.screen.SignalResize -= handleResizeCallback;
	}
}
