public interface FSingleTouchableInterface : FCapturedTouchableInterface
{
	bool HandleSingleTouchBegan(FTouch touch);

	void HandleSingleTouchMoved(FTouch touch);

	void HandleSingleTouchEnded(FTouch touch);

	void HandleSingleTouchCanceled(FTouch touch);
}
