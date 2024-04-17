using System;
using UnityEngine;

public class FDelayedCallback
{
	public float delayTime;

	public float timeRemaining;

	public Action func;

	public float percentComplete => Mathf.Clamp01(1f - timeRemaining / delayTime);

	public FDelayedCallback(Action func, float delayTime)
	{
		this.func = func;
		this.delayTime = delayTime;
		timeRemaining = delayTime;
	}
}
