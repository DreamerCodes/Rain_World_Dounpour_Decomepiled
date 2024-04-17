using HUD;
using UnityEngine;

namespace MoreSlugcats;

public class HypothermiaMeter : HudPart
{
	public int tutorialTimer;

	public Vector2 pos;

	public Vector2 lastPos;

	public int remainVisibleCounter;

	public float fade;

	public float lastFade;

	public HUDCircle[] circles;

	private bool Show
	{
		get
		{
			if (!hud.showKarmaFoodRain)
			{
				return hud.owner.RevealMap;
			}
			return true;
		}
	}

	public HypothermiaMeter(global::HUD.HUD hud, FContainer fContainer)
		: base(hud)
	{
		pos = new Vector2(80f, 20f);
		lastPos = pos;
		circles = new HUDCircle[10];
		fade = 0f;
		lastFade = 0f;
		for (int i = 0; i < circles.Length; i++)
		{
			circles[i] = new HUDCircle(hud, HUDCircle.SnapToGraphic.smallEmptyCircle, fContainer, 0);
			circles[i].fade = 0f;
			circles[i].lastFade = 0f;
		}
	}

	public override void Update()
	{
		float hypothermia = (hud.owner as Player).Hypothermia;
		if (hud.foodMeter != null)
		{
			pos.x = hud.foodMeter.pos.x;
			if (hypothermia <= 0.05f && hud.rainWorld.processManager.currentMainLoop is RainWorldGame && (hud.rainWorld.processManager.currentMainLoop as RainWorldGame).cameras[0].room != null && (hud.rainWorld.processManager.currentMainLoop as RainWorldGame).cameras[0].room.roomSettings.DangerType != MoreSlugcatsEnums.RoomRainDangerType.Blizzard)
			{
				fade = Mathf.Lerp(fade, 0f, 0.1f);
			}
			else
			{
				fade = Mathf.Lerp(fade, Show ? hud.foodMeter.fade : 0f, (fade < hud.foodMeter.fade) ? 0.15f : 0.005f);
			}
		}
		lastPos = pos;
		lastFade = fade;
		if (hud.HideGeneralHud)
		{
			fade = 0f;
		}
		float num = 1f / (float)circles.Length;
		float num2 = Mathf.InverseLerp(1.1f, 0f, hypothermia);
		for (int i = 0; i < circles.Length; i++)
		{
			float value = num2 - num * (float)i;
			circles[i].Update();
			circles[i].thickness = Mathf.Lerp(6f, 1f, Mathf.InverseLerp(num, 0f, value));
			circles[i].fade = Mathf.Lerp(circles[i].fade, Mathf.InverseLerp(num * (float)i, num * ((float)i + 1f), fade), 0.1f);
			circles[i].snapGraphic = HUDCircle.SnapToGraphic.smallEmptyCircle;
			circles[i].snapRad = 0.45f;
			circles[i].snapThickness = 0.45f;
			circles[i].rad = Mathf.Lerp(0.1f, 5f, circles[i].fade);
			circles[i].pos = pos + new Vector2((float)i * 21.6f, 0f);
		}
		if (tutorialTimer > 0)
		{
			tutorialTimer--;
		}
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

	public void ShowTutorial()
	{
		tutorialTimer = 4800;
	}
}
