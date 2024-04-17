using System;
using System.Collections.Generic;
using Rewired;
using RWCustom;
using UnityEngine;

namespace Menu;

public class InteractiveMenuScene : MenuScene
{
	public float getToFocus;

	public List<float> idleDepths;

	public Vector2 camVel;

	public Vector2 camGetTo;

	public Vector2 camGetTo2;

	public float idleCamSpeed;

	public float idleCamSpeedGetTo;

	public float mouseActive;

	public float gamePadActive;

	public int mouseInActiveCounter;

	public int gamePadInActiveCounter;

	public int timeSinceMouseOnButtonCounter;

	public int timer = -1;

	public FSprite[] dbSprts;

	public InteractiveMenuScene(Menu menu, MenuObject owner, SceneID sceneID)
		: base(menu, owner, sceneID)
	{
		getToFocus = UnityEngine.Random.value;
		camGetTo = Custom.RNV() * UnityEngine.Random.value;
		camPos = camGetTo;
		lastCamPos = camGetTo;
		camGetTo2 = camGetTo;
	}

	public override void Update()
	{
		base.Update();
		if (sceneID == SceneID.RedsDeathStatisticsBkg)
		{
			float value = Mathf.InverseLerp(0f, 160f, timer);
			if (!flatMode)
			{
				depthIllustrations[5].setAlpha = Mathf.InverseLerp(0.1f, 0.4f, value) * Mathf.InverseLerp(0.7f, 0.4f, value);
				depthIllustrations[6].setAlpha = Mathf.InverseLerp(0.1f, 0.4f, value) * Mathf.InverseLerp(0.7f, 0.4f, value) * 0.5f;
				depthIllustrations[7].setAlpha = Mathf.InverseLerp(0.4f, 0.7f, value) * Mathf.InverseLerp(1f, 0.7f, value);
				depthIllustrations[8].setAlpha = Mathf.InverseLerp(0.4f, 0.7f, value) * Mathf.InverseLerp(1f, 0.7f, value) * 0.5f;
				depthIllustrations[9].setAlpha = Mathf.InverseLerp(0.7f, 1f, value);
				depthIllustrations[10].setAlpha = Mathf.InverseLerp(0.7f, 1f, value) * 0.75f;
			}
		}
		else if (sceneID == SceneID.NewDeath)
		{
			float value = Mathf.InverseLerp(0f, 160f, timer);
			if (flatMode)
			{
				flatIllustrations[1].setAlpha = value;
			}
			else
			{
				depthIllustrations[7].setAlpha = Mathf.InverseLerp(0.1f, 0.4f, value) * Mathf.InverseLerp(0.7f, 0.4f, value);
				depthIllustrations[8].setAlpha = Mathf.InverseLerp(0.1f, 0.4f, value) * Mathf.InverseLerp(0.7f, 0.4f, value) * 0.5f;
				depthIllustrations[9].setAlpha = Mathf.InverseLerp(0.4f, 0.7f, value) * Mathf.InverseLerp(1f, 0.7f, value);
				depthIllustrations[10].setAlpha = Mathf.InverseLerp(0.4f, 0.7f, value) * Mathf.InverseLerp(1f, 0.7f, value) * 0.5f;
				depthIllustrations[11].setAlpha = Mathf.InverseLerp(0.7f, 1f, value);
				depthIllustrations[12].setAlpha = Mathf.InverseLerp(0.7f, 1f, value) * 0.75f;
			}
		}
		else if (sceneID == SceneID.Dream_Iggy_Image)
		{
			if (!flatMode)
			{
				depthIllustrations[5].setAlpha = Mathf.Lerp(0.6f, 1f, Mathf.Pow(UnityEngine.Random.value, 0.4f)) * 0.36f;
				depthIllustrations[4].setAlpha = Mathf.Lerp(0.6f, 1f, Mathf.Pow(UnityEngine.Random.value, 0.4f));
				depthIllustrations[3].setAlpha = Mathf.Pow(UnityEngine.Random.value, 1.4f) * 0.5f;
				depthIllustrations[2].setAlpha = Mathf.Lerp(0.6f, 1f, Mathf.Pow(UnityEngine.Random.value, 0.4f)) * 0.5f;
			}
		}
		else if (sceneID == SceneID.Dream_Pebbles)
		{
			if (!flatMode)
			{
				depthIllustrations[depthIllustrations.Count - 2].setAlpha = Mathf.Lerp(0.8f, 1f, Mathf.Pow(UnityEngine.Random.value, 0.4f));
			}
		}
		else if (sceneID == SceneID.Dream_Sleep)
		{
			if (!flatMode)
			{
				depthIllustrations[depthIllustrations.Count - 1].setAlpha = 0.7f;
				depthIllustrations[depthIllustrations.Count - 4].setAlpha = 0f;
			}
		}
		else if (sceneID == SceneID.Dream_Sleep_Fade)
		{
			if (timer < 0)
			{
				timer++;
			}
			float value = Custom.SCurve(Mathf.InverseLerp(60f, 160f, timer), 0.6f);
			if (flatMode)
			{
				flatIllustrations[1].setAlpha = value;
			}
			else
			{
				depthIllustrations[depthIllustrations.Count - 1].setAlpha = 0.1f + Mathf.Pow(1f - value, 0.5f) * 0.6f;
				depthIllustrations[depthIllustrations.Count - 2].setAlpha = Mathf.Pow(1f - value, 0.5f);
				depthIllustrations[depthIllustrations.Count - 3].setAlpha = Mathf.Pow(1f - value, 0.5f);
				depthIllustrations[depthIllustrations.Count - 4].setAlpha = Mathf.Pow(value, 0.5f);
				depthIllustrations[depthIllustrations.Count - 5].setAlpha = 0.5f + 0.5f * Mathf.Pow(1f - value, 0.5f);
			}
		}
		else if (sceneID == SceneID.Dream_Acceptance)
		{
			if (timer < 0)
			{
				timer++;
			}
			float value = Custom.SCurve(Mathf.InverseLerp(0f, 240f, timer), 0.8f);
			if (!flatMode)
			{
				depthIllustrations[0].setAlpha = 0.1f + 0.26f * Mathf.Pow(value, 0.5f);
				depthIllustrations[1].setAlpha = Mathf.Pow(value, 1.5f);
			}
		}
		else if (sceneID == SceneID.Dream_Moon_Betrayal)
		{
			if (menu is DreamScreen && !flatMode)
			{
				depthIllustrations[depthIllustrations.Count - 2].alpha = (((menu as DreamScreen).MoonBetrayalNeurons >= 1) ? 1f : 0f);
				depthIllustrations[depthIllustrations.Count - 1].alpha = (((menu as DreamScreen).MoonBetrayalNeurons >= 2) ? 1f : 0f);
				depthIllustrations[depthIllustrations.Count - 4].alpha = (((menu as DreamScreen).MoonBetrayalNeurons >= 3) ? 1f : 0f);
				depthIllustrations[depthIllustrations.Count - 3].alpha = (((menu as DreamScreen).MoonBetrayalNeurons >= 4) ? 1f : 0f);
			}
		}
		else if (sceneID == SceneID.Void_Slugcat_Upright)
		{
			if (!flatMode)
			{
				depthIllustrations[depthIllustrations.Count - 2].alpha = 0f;
			}
		}
		else if (sceneID == SceneID.Void_Slugcat_Down && !flatMode)
		{
			depthIllustrations[depthIllustrations.Count - 1].alpha = 0f;
		}
		lastFocus = focus;
		float num = Mathf.InverseLerp(0.4f, 1f, Mathf.Sin(Mathf.InverseLerp(10f, 200f, mouseInActiveCounter) * (float)Math.PI)) * Mathf.InverseLerp(40f, 100f, timeSinceMouseOnButtonCounter);
		focus = Custom.LerpAndTick(focus, getToFocus, Mathf.Lerp(0.011f, 0.05f, num), 1f / Mathf.Lerp(3000f, 900f, num));
		if (UnityEngine.Random.value < 0.02f && Math.Abs(focus - getToFocus) < 0.002f)
		{
			if (idleDepths != null && idleDepths.Count > 0)
			{
				getToFocus = Mathf.InverseLerp(1f, 10f, idleDepths[UnityEngine.Random.Range(0, idleDepths.Count)] + Mathf.Pow(UnityEngine.Random.value, 1.5f) * 0.5f);
			}
			else if (depthIllustrations.Count > 0)
			{
				getToFocus = Mathf.InverseLerp(1f, 10f, depthIllustrations[UnityEngine.Random.Range(0, depthIllustrations.Count)].depth + Mathf.Pow(UnityEngine.Random.value, 1.5f) * 0.5f);
			}
		}
		if (num > 0.3f && mouseInActiveCounter > 20)
		{
			float num2 = float.MaxValue;
			for (int i = 0; i < depthIllustrations.Count; i++)
			{
				float value = depthIllustrations[i].DepthAtPosition(menu.mousePosition, devtool: false);
				if (value > -1f && value < num2)
				{
					num2 = value;
				}
			}
			getToFocus = Mathf.InverseLerp(1f, 10f, num2);
		}
		lastCamPos = camPos;
		camPos += camVel;
		camPos *= Custom.LerpMap(camPos.magnitude, 0.8f, 1f, 1f, 0.96f);
		camPos = Vector2.ClampMagnitude(camPos, 1f);
		camVel *= Custom.LerpMap(Vector2.Distance(menu.lastMousePos, menu.mousePosition), 200f, 0f, 0.98f, Mathf.Lerp(0.99f, 0.9f, mouseActive));
		camVel *= Custom.LerpMap(Vector2.Distance(camPos, (camGetTo + camGetTo2) / 2f), 0f, 0.2f, Mathf.Max(0.9f, mouseActive, gamePadActive), 0.999f);
		if (UnityEngine.Random.value < 1f / (Custom.DistLess(camPos, camGetTo, 0.2f) ? 10f : 300f))
		{
			camGetTo = Custom.RNV() * UnityEngine.Random.value;
		}
		if (UnityEngine.Random.value < 0.2f)
		{
			camGetTo += Custom.RNV() * UnityEngine.Random.value * 0.05f;
		}
		camGetTo2 = Custom.MoveTowards(camGetTo2, camGetTo, Custom.LerpMap(Vector2.Distance(camGetTo2, camGetTo), 0f, 1.5f, 0.01f + 0.01f * idleCamSpeed, 0.002f));
		Vector2 vector = default(Vector2);
		int num3 = 0;
		Options.ControlSetup.Preset activePreset = menu.manager.rainWorld.options.controls[num3].GetActivePreset();
		if (activePreset != Options.ControlSetup.Preset.KeyboardSinglePlayer && activePreset != Options.ControlSetup.Preset.None)
		{
			Rewired.Player player = menu.manager.rainWorld.options.controls[num3].player;
			if (player != null)
			{
				vector = new Vector2(player.GetAxisRaw(6), player.GetAxisRaw(7));
				if (num3 < menu.manager.rainWorld.options.controls.Length)
				{
					if (menu.manager.rainWorld.options.controls[num3].xInvert)
					{
						vector.x *= -1f;
					}
					if (menu.manager.rainWorld.options.controls[num3].yInvert)
					{
						vector.y *= -1f;
					}
				}
			}
		}
		vector = Vector2.ClampMagnitude(vector, 1f);
		camVel += (new Vector2(-1f + 2f * Custom.LerpMap(menu.mousePosition.x, 0f, 1366f, 0f, 1f), -1f + 2f * Custom.LerpMap(menu.mousePosition.y, 0f, 768f, 0f, 1f)) - camPos) * 0.01f * mouseActive * (1f - gamePadActive);
		camVel += (vector - camPos) * 0.01f * Custom.LerpMap(vector.magnitude + Mathf.InverseLerp(0.3f, 0.1f, Vector2.Distance(camPos, vector)), 0.7f, 1f, 1f, 0.2f) * gamePadActive * (1f - mouseActive);
		camVel += Vector2.ClampMagnitude((camGetTo2 + camGetTo) / 2f - camPos, 0.02f) * Mathf.Lerp(0.005f, 0.025f, idleCamSpeed) * (1f - mouseActive) * (1f - gamePadActive);
		idleCamSpeed = Custom.LerpAndTick(idleCamSpeed, idleCamSpeedGetTo, 0.002f, 1f / 120f);
		if (UnityEngine.Random.value < 0.0125f)
		{
			idleCamSpeedGetTo = UnityEngine.Random.value;
		}
		if (camPos.magnitude > 0.9f)
		{
			camVel -= camPos * Mathf.InverseLerp(0.9f, 1f, camPos.magnitude) * 0.05f;
			if (camGetTo.magnitude > 0.9f)
			{
				camGetTo *= 0.95f;
			}
		}
		if (vector.magnitude > 0.1f)
		{
			gamePadInActiveCounter = 0;
		}
		else
		{
			gamePadInActiveCounter++;
		}
		if (gamePadInActiveCounter > 100)
		{
			gamePadActive *= 0.99f;
		}
		else
		{
			gamePadActive = Custom.LerpAndTick(gamePadActive, Mathf.InverseLerp(0f, 0.2f, vector.magnitude), 0.1f, 0.025f);
		}
		if (Vector2.Distance(menu.lastMousePos, menu.mousePosition) > 5f)
		{
			mouseInActiveCounter = 0;
		}
		else
		{
			mouseInActiveCounter++;
		}
		if (mouseInActiveCounter > 100)
		{
			mouseActive *= 0.99f;
		}
		else
		{
			mouseActive = Custom.LerpAndTick(mouseActive, Mathf.InverseLerp(3f, 20f, Vector2.Distance(menu.lastMousePos, menu.mousePosition)), 0.1f, 0.025f);
		}
		if (timer >= 0)
		{
			timer++;
		}
		timeSinceMouseOnButtonCounter++;
		if (menu.manager.menuesMouseMode && menu.selectedObject != null)
		{
			timeSinceMouseOnButtonCounter = 0;
		}
	}
}
