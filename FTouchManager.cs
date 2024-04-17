using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class FTouchManager
{
	public const int SLOT_COUNT = 12;

	public static bool shouldMouseEmulateTouch = true;

	public static bool isEnabled = true;

	private List<FMultiTouchableInterface> _multiTouchables = new List<FMultiTouchableInterface>();

	private List<FCapturedTouchableInterface> _capturedTouchables = new List<FCapturedTouchableInterface>();

	private bool _needsPrioritySort;

	private Vector2 _previousMousePosition = new Vector2(0f, 0f);

	private FTouchSlot[] _touchSlots;

	private FTouch scratchMouseTouch = new FTouch();

	private FTouch[] scratchTouches;

	private List<FMultiTouchableInterface> tempMultiTouchables = new List<FMultiTouchableInterface>();

	private List<FCapturedTouchableInterface> tempCapturedTouchables = new List<FCapturedTouchableInterface>();

	public FTouchManager()
	{
		Input.multiTouchEnabled = true;
		_touchSlots = new FTouchSlot[12];
		for (int i = 0; i < 12; i++)
		{
			_touchSlots[i] = new FTouchSlot(i);
		}
	}

	public bool DoesTheSingleTouchableExist()
	{
		return _touchSlots[0].doesHaveTouch;
	}

	public void Update()
	{
		if (!isEnabled)
		{
			return;
		}
		if (_needsPrioritySort)
		{
			UpdatePrioritySorting();
		}
		for (int i = 0; i < _multiTouchables.Count; i++)
		{
			tempMultiTouchables.Add(_multiTouchables[i]);
		}
		for (int j = 0; j < _capturedTouchables.Count; j++)
		{
			tempCapturedTouchables.Add(_capturedTouchables[j]);
		}
		float num = 1f / Futile.displayScale;
		float num2 = (0f - Futile.screen.originX) * (float)Futile.screen.pixelWidth;
		float num3 = (0f - Futile.screen.originY) * (float)Futile.screen.pixelHeight;
		bool flag = false;
		if (shouldMouseEmulateTouch)
		{
			scratchMouseTouch.position.Set((Input.mousePosition.x + num2) * num, (Input.mousePosition.y + num3) * num);
			scratchMouseTouch.fingerId = 0;
			scratchMouseTouch.tapCount = 1;
			scratchMouseTouch.deltaTime = Time.deltaTime;
			if (Input.GetMouseButtonDown(0))
			{
				scratchMouseTouch.deltaPosition.Set(0f, 0f);
				_previousMousePosition = scratchMouseTouch.position;
				scratchMouseTouch.phase = TouchPhase.Began;
				flag = true;
			}
			else if (Input.GetMouseButtonUp(0))
			{
				scratchMouseTouch.deltaPosition.Set(scratchMouseTouch.position.x - _previousMousePosition.x, scratchMouseTouch.position.y - _previousMousePosition.y);
				_previousMousePosition = scratchMouseTouch.position;
				scratchMouseTouch.phase = TouchPhase.Ended;
				flag = true;
			}
			else if (Input.GetMouseButton(0))
			{
				scratchMouseTouch.deltaPosition.Set(scratchMouseTouch.position.x - _previousMousePosition.x, scratchMouseTouch.position.y - _previousMousePosition.y);
				_previousMousePosition = scratchMouseTouch.position;
				scratchMouseTouch.phase = TouchPhase.Moved;
				flag = true;
			}
		}
		int num4 = Input.touchCount;
		int num5 = 0;
		if (flag)
		{
			num4++;
		}
		if (scratchTouches == null || scratchTouches.Length < num4)
		{
			scratchTouches = new FTouch[num4];
			for (int k = 0; k < scratchTouches.Length; k++)
			{
				scratchTouches[k] = new FTouch();
			}
		}
		if (flag)
		{
			scratchTouches[0].Copy(scratchMouseTouch);
			num5 = 1;
		}
		for (int l = 0; l < Input.touchCount; l++)
		{
			Touch touch = Input.GetTouch(l);
			scratchTouches[l + num5].Copy(touch, num2, num3, num);
		}
		int count = tempCapturedTouchables.Count;
		for (int m = 0; m < 12; m++)
		{
			FTouchSlot fTouchSlot = _touchSlots[m];
			if (!fTouchSlot.doesHaveTouch)
			{
				continue;
			}
			bool flag2 = false;
			for (int n = 0; n < num4; n++)
			{
				FTouch fTouch = scratchTouches[n];
				if (fTouchSlot.touch.fingerId == fTouch.fingerId)
				{
					flag2 = true;
					fTouchSlot.touch = fTouch;
					fTouch.slot = fTouchSlot;
					break;
				}
			}
			if (!flag2)
			{
				fTouchSlot.doesHaveTouch = false;
				fTouchSlot.touchable = null;
			}
		}
		for (int num6 = 0; num6 < 12; num6++)
		{
			FTouchSlot fTouchSlot2 = _touchSlots[num6];
			if (!fTouchSlot2.doesHaveTouch)
			{
				for (int num7 = 0; num7 < num4; num7++)
				{
					FTouch fTouch2 = scratchTouches[num7];
					if (fTouch2.slot == null)
					{
						fTouchSlot2.touch = fTouch2;
						fTouchSlot2.doesHaveTouch = true;
						fTouch2.slot = fTouchSlot2;
						break;
					}
				}
			}
			if (fTouchSlot2.doesHaveTouch)
			{
				if (fTouchSlot2.touch.phase == TouchPhase.Began)
				{
					for (int num8 = 0; num8 < count; num8++)
					{
						FCapturedTouchableInterface fCapturedTouchableInterface = tempCapturedTouchables[num8];
						FSingleTouchableInterface fSingleTouchableInterface = fCapturedTouchableInterface as FSingleTouchableInterface;
						if (fTouchSlot2.index == 0 && fSingleTouchableInterface != null && fSingleTouchableInterface.HandleSingleTouchBegan(fTouchSlot2.touch))
						{
							fTouchSlot2.isSingleTouchable = true;
							fTouchSlot2.touchable = fCapturedTouchableInterface;
							break;
						}
						if (fCapturedTouchableInterface is FSmartTouchableInterface fSmartTouchableInterface && fSmartTouchableInterface.HandleSmartTouchBegan(fTouchSlot2.index, fTouchSlot2.touch))
						{
							fTouchSlot2.isSingleTouchable = false;
							fTouchSlot2.touchable = fCapturedTouchableInterface;
							break;
						}
					}
				}
				else if (fTouchSlot2.touch.phase == TouchPhase.Moved)
				{
					if (fTouchSlot2.touchable != null)
					{
						if (fTouchSlot2.isSingleTouchable)
						{
							(fTouchSlot2.touchable as FSingleTouchableInterface).HandleSingleTouchMoved(fTouchSlot2.touch);
						}
						else
						{
							(fTouchSlot2.touchable as FSmartTouchableInterface).HandleSmartTouchMoved(fTouchSlot2.index, fTouchSlot2.touch);
						}
					}
				}
				else if (fTouchSlot2.touch.phase == TouchPhase.Ended)
				{
					if (fTouchSlot2.touchable != null)
					{
						if (fTouchSlot2.isSingleTouchable)
						{
							(fTouchSlot2.touchable as FSingleTouchableInterface).HandleSingleTouchEnded(fTouchSlot2.touch);
						}
						else
						{
							(fTouchSlot2.touchable as FSmartTouchableInterface).HandleSmartTouchEnded(fTouchSlot2.index, fTouchSlot2.touch);
						}
					}
					fTouchSlot2.touchable = null;
					fTouchSlot2.doesHaveTouch = false;
				}
				else
				{
					if (fTouchSlot2.touch.phase != TouchPhase.Canceled)
					{
						continue;
					}
					if (fTouchSlot2.touchable != null)
					{
						if (fTouchSlot2.isSingleTouchable)
						{
							(fTouchSlot2.touchable as FSingleTouchableInterface).HandleSingleTouchCanceled(fTouchSlot2.touch);
						}
						else
						{
							(fTouchSlot2.touchable as FSmartTouchableInterface).HandleSmartTouchCanceled(fTouchSlot2.index, fTouchSlot2.touch);
						}
					}
					fTouchSlot2.touchable = null;
					fTouchSlot2.doesHaveTouch = false;
				}
			}
			else
			{
				fTouchSlot2.touchable = null;
				fTouchSlot2.doesHaveTouch = false;
			}
		}
		if (num4 > 0)
		{
			int count2 = tempMultiTouchables.Count;
			for (int num9 = 0; num9 < count2; num9++)
			{
				tempMultiTouchables[num9].HandleMultiTouch(scratchTouches);
			}
		}
		tempMultiTouchables.Clear();
		tempCapturedTouchables.Clear();
	}

	public void HandleDepthChange()
	{
		_needsPrioritySort = true;
	}

	private static int CapturablePriorityComparison(FCapturedTouchableInterface a, FCapturedTouchableInterface b)
	{
		return b.touchPriority - a.touchPriority;
	}

	private void UpdatePrioritySorting()
	{
		_needsPrioritySort = false;
		_capturedTouchables.Sort(CapturablePriorityComparison);
	}

	public void AddSingleTouchTarget(FSingleTouchableInterface touchable)
	{
		if (!_capturedTouchables.Contains(touchable))
		{
			_capturedTouchables.Add(touchable);
			_needsPrioritySort = true;
		}
	}

	public void RemoveSingleTouchTarget(FSingleTouchableInterface touchable)
	{
		_capturedTouchables.Remove(touchable);
	}

	public void AddMultiTouchTarget(FMultiTouchableInterface touchable)
	{
		if (!_multiTouchables.Contains(touchable))
		{
			_multiTouchables.Add(touchable);
		}
	}

	public void RemoveMultiTouchTarget(FMultiTouchableInterface touchable)
	{
		_multiTouchables.Remove(touchable);
	}

	public void AddSmartTouchTarget(FSmartTouchableInterface touchable)
	{
		if (!_capturedTouchables.Contains(touchable))
		{
			_capturedTouchables.Add(touchable);
			_needsPrioritySort = true;
		}
	}

	public void RemoveSmartTouchTarget(FSmartTouchableInterface touchable)
	{
		_capturedTouchables.Remove(touchable);
	}

	public void LogAllListeners()
	{
		StringBuilder stringBuilder = new StringBuilder("MultiTouchables(" + _multiTouchables.Count + "): ");
		for (int i = 0; i < _multiTouchables.Count; i++)
		{
			stringBuilder.Append(_multiTouchables[i]);
			if (i < _multiTouchables.Count - 1)
			{
				stringBuilder.Append(", ");
			}
		}
		stringBuilder = new StringBuilder("CapturedTouchables(" + _capturedTouchables.Count + "): ");
		for (int j = 0; j < _capturedTouchables.Count; j++)
		{
			stringBuilder.Append(_capturedTouchables[j]);
			if (j < _capturedTouchables.Count - 1)
			{
				stringBuilder.Append(", ");
			}
		}
	}
}
