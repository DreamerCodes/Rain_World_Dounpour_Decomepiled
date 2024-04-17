public class FNodeEnablerForSingleTouch : FNodeEnabler
{
	public FSingleTouchableInterface singleTouchable;

	public FNodeEnablerForSingleTouch(FNode node)
	{
		singleTouchable = node as FSingleTouchableInterface;
		if (singleTouchable == null)
		{
			throw new FutileException("Trying to enable single touch on a node that doesn't implement FSingleTouchableInterface");
		}
	}

	public override void Connect()
	{
		Futile.touchManager.AddSingleTouchTarget(singleTouchable);
	}

	public override void Disconnect()
	{
		Futile.touchManager.RemoveSingleTouchTarget(singleTouchable);
	}
}
