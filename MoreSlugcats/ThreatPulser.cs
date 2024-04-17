using System;
using HUD;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class ThreatPulser : HudPart
{
	public Vector2 pos;

	public Vector2 lastPos;

	public int remainVisibleCounter;

	public float fade;

	public float lastFade;

	public float lastThreat;

	public float pulseTimer;

	public int firstRevealTime;

	public HUDCircle[] circles;

	public Player hudPlayer => hud.owner as Player;

	private float Threat
	{
		get
		{
			if (hudPlayer.abstractCreature.world.game.GameOverModeActive)
			{
				return 0f;
			}
			if (hudPlayer.abstractCreature.world.game.manager.musicPlayer != null)
			{
				return hudPlayer.abstractCreature.world.game.manager.musicPlayer.threatTracker.currentMusicAgnosticThreat;
			}
			if (hudPlayer.abstractCreature.world.game.manager.fallbackThreatDetermination == null)
			{
				hudPlayer.abstractCreature.world.game.manager.fallbackThreatDetermination = new ThreatDetermination(0);
			}
			return hudPlayer.abstractCreature.world.game.manager.fallbackThreatDetermination.currentMusicAgnosticThreat;
		}
	}

	private bool Show
	{
		get
		{
			bool flag = hudPlayer != null && hudPlayer.airInLungs < 1f;
			if (!MMF.cfgBreathTimeVisualIndicator.Value)
			{
				flag = false;
			}
			if (flag)
			{
				return false;
			}
			if (hudPlayer != null)
			{
				return Threat > 0f;
			}
			return false;
		}
	}

	public ThreatPulser(global::HUD.HUD hud, FContainer fContainer)
		: base(hud)
	{
		circles = new HUDCircle[5];
		pos = new Vector2(hud.rainWorld.options.ScreenSize.x / 2f - (float)circles.Length * 21.6f / 2f, 40f);
		lastPos = pos;
		fade = 0f;
		lastFade = 0f;
		for (int i = 0; i < circles.Length; i++)
		{
			circles[i] = new HUDCircle(hud, HUDCircle.SnapToGraphic.smallEmptyCircle, fContainer, 1);
			circles[i].fade = 0f;
			circles[i].lastFade = 0f;
		}
	}

	public override void Update()
	{
		float threat = Threat;
		if (threat == 0f)
		{
			fade = Mathf.Lerp(fade, 0f, 0.2f);
		}
		else
		{
			fade = Mathf.Lerp(fade, Show ? 1f : 0f, 0.2f);
		}
		lastPos = pos;
		lastFade = fade;
		float num = 1f / (float)circles.Length;
		float num2 = Mathf.InverseLerp(0f, 1.1f, threat);
		int num3 = Math.Min(circles.Length, (int)(num2 * (float)circles.Length) + 1);
		pos.x = hud.rainWorld.options.ScreenSize.x / 2f - (float)num3 * 21.6f / 2f;
		firstRevealTime--;
		if (lastThreat <= 0f && threat > 0f)
		{
			firstRevealTime = 40;
		}
		if (firstRevealTime > 0)
		{
			pulseTimer += 0.29999998f;
		}
		else
		{
			pulseTimer += 0.03f * (float)num3;
		}
		float num4 = Custom.LerpExpEaseIn(0f, 1f, Mathf.Abs(Mathf.Sin(pulseTimer))) * ((firstRevealTime > 0) ? 0.6f : (0.1f + 0.12f * (float)num3));
		for (int i = 0; i < num3; i++)
		{
			float value = num2 - num * (float)i;
			circles[i].Update();
			circles[i].fade = Mathf.Lerp(circles[i].fade, Mathf.InverseLerp(num * (float)i, num * ((float)i + 1f), fade), 0.1f);
			circles[i].snapGraphic = HUDCircle.SnapToGraphic.smallEmptyCircle;
			circles[i].snapRad = 0.45f;
			circles[i].snapThickness = 0.45f;
			circles[i].rad = Mathf.Lerp(0.1f, 5f, circles[i].fade) * (1f + num4);
			circles[i].thickness = Mathf.Lerp(circles[i].rad + 1f, 1f, Mathf.InverseLerp(num, 0f, value));
			circles[i].pos = pos + new Vector2((float)i * 21.6f, 0f);
		}
		for (int j = num3; j < circles.Length; j++)
		{
			circles[j].Update();
			circles[j].thickness = 0f;
			circles[j].fade = 0f;
		}
		lastThreat = threat;
	}

	public Vector2 DrawPos(float timeStacker)
	{
		return Vector2.Lerp(lastPos, pos, timeStacker);
	}

	public override void Draw(float timeStacker)
	{
		for (int i = 0; i < circles.Length; i++)
		{
			circles[i].Draw(timeStacker);
		}
	}
}
