public class FTouchSlot
{
	public int index;

	public FTouch touch;

	public bool doesHaveTouch;

	public FCapturedTouchableInterface touchable;

	public bool isSingleTouchable = true;

	public FTouchSlot(int index)
	{
		this.index = index;
	}
}
