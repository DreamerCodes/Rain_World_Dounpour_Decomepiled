using System.Globalization;
using Kittehface.Build;
using Kittehface.Framework20;
using Rewired;
using UnityEngine;
using UnityEngine.UI;

public class DebugPanel : MonoBehaviour
{
	private class FPS
	{
		private float fpsSum;

		private int fpsSamples;

		private float fpsMin = 1f;

		private float fpsMax;

		private float displayFPS;

		private float displayMax;

		private float displayMin;

		public string FPSDisplay { get; private set; }

		public void Update(float deltaTime)
		{
			fpsSum += deltaTime;
			fpsSamples++;
			if (fpsMin > Time.deltaTime)
			{
				fpsMin = Time.deltaTime;
			}
			if (fpsMax < Time.deltaTime)
			{
				fpsMax = Time.deltaTime;
			}
			if (fpsSamples > 100)
			{
				float num = fpsSum / (float)fpsSamples;
				displayFPS = 1f / num;
				displayMax = 1f / fpsMin;
				displayMin = 1f / fpsMax;
				fpsSum = 0f;
				fpsSamples = 0;
				fpsMin = 1f;
				fpsMax = 0f;
				FPSDisplay = string.Format(CultureInfo.InvariantCulture, "FPS: {0:0.0} Max: {1:0.0} Min: {2:0.0}", displayFPS, displayMax, displayMin);
			}
		}
	}

	[SerializeField]
	private Text informationText;

	[SerializeField]
	private bool enable;

	private int needsUpdate = 1;

	private const int NeedsUpdateFrameDelay = 2;

	private string infoText = "";

	private FPS fps = new FPS();

	private void Awake()
	{
		ResolutionPerformanceManager.OnResolutionChanged += ResolutionPerformanceManager_OnResolutionChanged;
		InitializeSwitch();
		if (!Utilities.isDebugBuild || !enable)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void InitializeSwitch()
	{
	}

	private void OnDestroy()
	{
		ResolutionPerformanceManager.OnResolutionChanged -= ResolutionPerformanceManager_OnResolutionChanged;
	}

	private void Update()
	{
		fps.Update(Time.deltaTime);
	}

	private void LateUpdate()
	{
		if (!Platform.initialized)
		{
			return;
		}
		if (needsUpdate > 0)
		{
			if (needsUpdate == 1)
			{
				UpdateText();
			}
			needsUpdate--;
		}
		informationText.text = infoText + "\n" + fps.FPSDisplay;
	}

	private void UpdateText()
	{
		if (null != informationText)
		{
			string text = "";
			text += Screen.currentResolution.ToString();
			infoText = text;
		}
	}

	private void RequestUpdate()
	{
		needsUpdate = 2;
	}

	private void ResolutionPerformanceManager_OnResolutionChanged()
	{
		RequestUpdate();
	}

	private void Rewired_ControllerPreDisconnectEvent(ControllerStatusChangedEventArgs args)
	{
		RequestUpdate();
	}

	private void Rewired_ControllerConnectedEvent(ControllerStatusChangedEventArgs args)
	{
		RequestUpdate();
	}

	private void UserInput_OnControllerConfigurationChanged()
	{
		RequestUpdate();
	}
}
