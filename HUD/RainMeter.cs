using System;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace HUD;

public class RainMeter : HudPart
{
	public Vector2 pos;

	public Vector2 lastPos;

	public int remainVisibleCounter;

	public float fade;

	public float lastFade;

	private float plop;

	public HUDCircle[] circles;

	private float fRain;

	public int halfTimeBlink;

	private bool halfTimeShown;

	public int tickCounter;

	public float tickPulse;

	public int timePerCircle;

	private bool Show
	{
		get
		{
			if (halfTimeBlink <= 0 && !hud.showKarmaFoodRain)
			{
				return hud.owner.RevealMap;
			}
			return true;
		}
	}

	public RainMeter(HUD hud, FContainer fContainer)
		: base(hud)
	{
		lastPos = pos;
		timePerCircle = 1200;
		int num = (hud.owner as Player).room.world.rainCycle.cycleLength / timePerCircle;
		if (num > 30)
		{
			num = 30;
			timePerCircle = (hud.owner as Player).room.world.rainCycle.cycleLength / num;
		}
		circles = new HUDCircle[num];
		for (int i = 0; i < circles.Length; i++)
		{
			circles[i] = new HUDCircle(hud, HUDCircle.SnapToGraphic.smallEmptyCircle, fContainer, 0);
		}
		if (ModManager.MSC && (hud.owner as Player).SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint && hud.map.RegionName != "HR")
		{
			halfTimeShown = true;
		}
	}

	public override void Update()
	{
		bool flag = (hud.owner as Player).room != null && (hud.owner as Player).room.game.setupValues.disableRain;
		if (ModManager.MSC && (hud.owner as Player).room != null && (hud.owner as Player).SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint && hud.map.RegionName != "HR")
		{
			halfTimeShown = true;
		}
		if (ModManager.MSC && (hud.owner as Player).inVoidSea)
		{
			halfTimeShown = true;
		}
		lastPos = pos;
		pos = hud.karmaMeter.pos;
		if (!halfTimeShown && !flag && (hud.owner as Player).room != null && (hud.owner as Player).room.world.rainCycle.AmountLeft < 0.5f && (hud.owner as Player).room.roomSettings.DangerType != RoomRain.DangerType.None && (!ModManager.MMF || !(hud.owner as Player).room.world.rainCycle.RegionHidesTimer))
		{
			halfTimeBlink = 220;
			halfTimeShown = true;
		}
		lastFade = fade;
		if (remainVisibleCounter > 0)
		{
			remainVisibleCounter--;
		}
		if (halfTimeBlink > 0)
		{
			halfTimeBlink--;
			hud.karmaMeter.forceVisibleCounter = Math.Max(hud.karmaMeter.forceVisibleCounter, 10);
		}
		if (ModManager.MMF && MMF.cfgTickTock.Value)
		{
			tickPulse = Mathf.Lerp(tickPulse, 0f, 0.1f);
		}
		else
		{
			tickPulse = 0f;
		}
		if ((hud.karmaMeter.fade > 0f && Show) || remainVisibleCounter > 0)
		{
			fade = Mathf.Min(1f, fade + 1f / 30f);
			if (ModManager.MMF && MMF.cfgTickTock.Value && (hud.owner as Player).room != null && hud.owner.RevealMap && (hud.owner as Player).room.world.rainCycle.AmountLeft > 0f && !(hud.owner as Player).room.world.rainCycle.RegionHidesTimer)
			{
				tickCounter++;
				if (tickCounter % 240 == 0)
				{
					(hud.owner as Player).room.PlaySound(MMFEnums.MMFSoundID.Tick, 0f, 0.85f, 1f);
					tickPulse = 1f;
				}
				if (tickCounter % 240 == 120)
				{
					(hud.owner as Player).room.PlaySound(MMFEnums.MMFSoundID.Tock, 0f, 0.85f, 1f);
					tickPulse = 1f;
				}
			}
		}
		else
		{
			fade = Mathf.Max(0f, fade - 0.1f);
		}
		if (hud.HideGeneralHud)
		{
			fade = 0f;
		}
		if (fade >= 0.7f)
		{
			plop = Mathf.Min(1f, plop + 0.05f);
		}
		else
		{
			plop = 0f;
		}
		if (flag)
		{
			fRain = 1f;
		}
		else if ((hud.owner as Player).room != null)
		{
			fRain = (hud.owner as Player).room.world.rainCycle.AmountLeft;
		}
		bool flag2 = ModManager.MMF && MMF.cfgHideRainMeterNoThreat.Value && (hud.owner as Player).room != null && (hud.owner as Player).room.world.rainCycle.RegionHidesTimer && (hud.owner as Player).room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.DayNight) == 0f;
		for (int i = 0; i < circles.Length; i++)
		{
			circles[i].Update();
			if (fade > 0f || lastFade > 0f)
			{
				float num = (float)i / (float)(circles.Length - 1);
				float value = Mathf.InverseLerp((float)i / (float)circles.Length, (float)(i + 1) / (float)circles.Length, fRain);
				float num2 = Mathf.InverseLerp(0.5f, 0.475f, Mathf.Abs(0.5f - Mathf.InverseLerp(1f / 30f, 1f, value)));
				if (flag2)
				{
					circles[i].rad = (3f * Mathf.Pow(fade, 2f) + Mathf.InverseLerp(0.075f, 0f, Mathf.Abs(1f - num - Mathf.Lerp((1f - fRain) * fade - 0.075f, 1.075f, Mathf.Pow(plop, 0.85f)))) * 2f * fade) * Mathf.InverseLerp(0f, 1f / 30f, 1f);
					circles[i].thickness = 1f;
					circles[i].snapGraphic = HUDCircle.SnapToGraphic.smallEmptyCircle;
					circles[i].snapRad = 3f;
					circles[i].snapThickness = 1f;
				}
				else
				{
					if (halfTimeBlink > 0)
					{
						num2 = Mathf.Max(num2, (halfTimeBlink % 15 < 7) ? 0f : 1f);
					}
					circles[i].rad = ((2f + num2) * Mathf.Pow(fade, 2f) + Mathf.InverseLerp(0.075f, 0f, Mathf.Abs(1f - num - Mathf.Lerp((1f - fRain) * fade - 0.075f, 1.075f, Mathf.Pow(plop, 0.85f)))) * 2f * fade) * Mathf.InverseLerp(0f, 1f / 30f, value);
					if (num2 == 0f)
					{
						circles[i].thickness = -1f;
						circles[i].snapGraphic = HUDCircle.SnapToGraphic.Circle4;
						circles[i].snapRad = 2f;
						circles[i].snapThickness = -1f;
					}
					else
					{
						circles[i].thickness = Mathf.Lerp(3.5f, 1f, num2);
						circles[i].snapGraphic = HUDCircle.SnapToGraphic.smallEmptyCircle;
						circles[i].snapRad = 3f;
						circles[i].snapThickness = 1f;
					}
				}
				circles[i].pos = pos + Custom.DegToVec((1f - (float)i / (float)circles.Length) * 360f * Custom.SCurve(Mathf.Pow(fade, 1.5f - num), 0.6f)) * (hud.karmaMeter.Radius + 8.5f + num2 + 4f * tickPulse);
			}
			else
			{
				circles[i].rad = 0f;
			}
		}
	}

	public Vector2 DrawPos(float timeStacker)
	{
		return Vector2.Lerp(lastPos, pos, timeStacker);
	}

	public override void Draw(float timeStacker)
	{
		if (!ModManager.MSC || !(hud.owner.GetOwnerType() == HUD.OwnerType.Player) || !((hud.owner as Player).SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint) || !(hud.map.RegionName != "HR"))
		{
			for (int i = 0; i < circles.Length; i++)
			{
				circles[i].Draw(timeStacker);
			}
		}
	}

	public void ResetHalfTime()
	{
		halfTimeShown = false;
		halfTimeBlink = 0;
	}

	public void SuppressHalfTime()
	{
		halfTimeShown = true;
	}
}
