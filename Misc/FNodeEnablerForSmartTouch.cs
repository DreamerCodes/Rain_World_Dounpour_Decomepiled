public class FNodeEnablerForSmartTouch : FNodeEnabler
{
	public FSmartTouchableInterface smartTouchable;

	public FNodeEnablerForSmartTouch(FNode node)
	{
		smartTouchable = node as FSmartTouchableInterface;
		if (smartTouchable == null)
		{
			throw new FutileException("Trying to enable single touch on a node that doesn't implement FSmartTouchableInterface");
		}
	}

	public override void Connect()
	{
		Futile.touchManager.AddSmartTouchTarget(smartTouchable);
	}

	public override void Disconnect()
	{
		Futile.touchManager.RemoveSmartTouchTarget(smartTouchable);
	}
}
