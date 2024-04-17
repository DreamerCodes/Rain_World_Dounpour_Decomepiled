using System;
using UnityEngine;

public class ResolutionPerformanceManager : MonoBehaviour
{
	private static ResolutionPerformanceManager instance;

	private const int NormalModeScreenWidth = 1280;

	private const int NormalModeScreenHeight = 720;

	private const int BoostModeScreenWidth = 1920;

	private const int BoostModeScreenHeight = 1080;

	public static event Action OnResolutionChanged;

	private void Awake()
	{
		if (instance != null)
		{
			if (instance != this)
			{
				UnityEngine.Object.Destroy(base.gameObject);
				return;
			}
		}
		else
		{
			instance = this;
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}
		_ = Screen.currentResolution;
	}

	private void OnDestroy()
	{
	}

	public static void EnableEnhancedPerformanceConfiguration(bool enabled)
	{
	}

	private void UpdateResolution()
	{
	}
}
