public interface FSmartTouchableInterface : FCapturedTouchableInterface
{
	bool HandleSmartTouchBegan(int touchIndex, FTouch touch);

	void HandleSmartTouchMoved(int touchIndex, FTouch touch);

	void HandleSmartTouchEnded(int touchIndex, FTouch touch);

	void HandleSmartTouchCanceled(int touchIndex, FTouch touch);
}
