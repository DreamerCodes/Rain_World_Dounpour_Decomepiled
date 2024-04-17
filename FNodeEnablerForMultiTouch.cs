public class FNodeEnablerForMultiTouch : FNodeEnabler
{
	public FMultiTouchableInterface multiTouchable;

	public FNodeEnablerForMultiTouch(FNode node)
	{
		multiTouchable = node as FMultiTouchableInterface;
		if (multiTouchable == null)
		{
			throw new FutileException("Trying to enable multi touch on a node that doesn't implement FMultiTouchableInterface");
		}
	}

	public override void Connect()
	{
		Futile.touchManager.AddMultiTouchTarget(multiTouchable);
	}

	public override void Disconnect()
	{
		Futile.touchManager.RemoveMultiTouchTarget(multiTouchable);
	}
}
