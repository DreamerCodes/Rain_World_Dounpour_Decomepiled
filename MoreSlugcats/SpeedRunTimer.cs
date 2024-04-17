using System;
using HUD;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class SpeedRunTimer : HudPart
{
	public class CampaignTimeTracker
	{
		public double UndeterminedFreeTime { get; set; }

		public double CompletedFreeTime { get; set; }

		public double LostFreeTime { get; set; }

		public double TotalFreeTime => CompletedFreeTime + LostFreeTime + UndeterminedFreeTime;

		public TimeSpan TotalFreeTimeSpan => TimeSpan.FromMilliseconds(TotalFreeTime);

		public double UndeterminedFixedTime { get; set; }

		public double CompletedFixedTime { get; set; }

		public double LostFixedTime { get; set; }

		public double TotalFixedTime => CompletedFixedTime + LostFixedTime + UndeterminedFixedTime;

		public TimeSpan TotalFixedTimeSpan => TimeSpan.FromMilliseconds(TotalFixedTime);

		public void WipeTimes()
		{
			UndeterminedFreeTime = 0.0;
			CompletedFreeTime = 0.0;
			LostFreeTime = 0.0;
			UndeterminedFixedTime = 0.0;
			CompletedFixedTime = 0.0;
			LostFixedTime = 0.0;
		}

		public void ConvertUndeterminedToLostTime()
		{
			LostFreeTime += UndeterminedFreeTime;
			UndeterminedFreeTime = 0.0;
			LostFixedTime += UndeterminedFixedTime;
			UndeterminedFixedTime = 0.0;
		}

		public void ConvertUndeterminedToCompletedTime()
		{
			CompletedFreeTime += UndeterminedFreeTime;
			UndeterminedFreeTime = 0.0;
			CompletedFixedTime += UndeterminedFixedTime;
			UndeterminedFixedTime = 0.0;
		}

		public void LoadOldTimings(int gameTimeAlive, int gameTimeDead)
		{
			double num = (double)gameTimeAlive * 1000.0;
			double num2 = (double)gameTimeDead * 1000.0;
			CompletedFreeTime = num;
			LostFreeTime = num2;
			CompletedFixedTime = num;
			LostFixedTime = num2;
		}
	}

	public Vector2 pos;

	public Vector2 lastPos;

	public FLabel timeLabel;

	public int remainVisibleCounter;

	public float fade;

	public float lastFade;

	public SpeedRunTimer(global::HUD.HUD hud, PlayerSpecificMultiplayerHud multiHud, FContainer fContainer)
		: base(hud)
	{
		lastPos = pos;
		timeLabel = new FLabel(Custom.GetDisplayFont(), "");
		fContainer.AddChild(timeLabel);
	}

	public static string TimeFormat(TimeSpan timeSpan)
	{
		return string.Format("{0:D3}h:{1:D2}m:{2:D2}s", new object[3]
		{
			timeSpan.Hours + 24 * timeSpan.Days,
			timeSpan.Minutes,
			timeSpan.Seconds
		});
	}

	private bool Show()
	{
		return hud.showKarmaFoodRain;
	}

	public override void Update()
	{
		lastPos = pos;
		pos = new Vector2((float)(int)(hud.rainWorld.options.ScreenSize.x / 2f) + 0.2f, (float)(int)(hud.rainWorld.options.ScreenSize.y - (30f + 10f * fade)) + 0.2f);
		lastFade = fade;
		if (remainVisibleCounter > 0)
		{
			remainVisibleCounter--;
		}
		if (Show() || remainVisibleCounter > 0)
		{
			fade = Mathf.Max(Mathf.Min(1f, fade + 0.1f), hud.foodMeter.fade);
		}
		else
		{
			fade = Mathf.Max(0f, fade - 0.1f);
		}
		if (hud.HideGeneralHud)
		{
			fade = 0f;
		}
		if (hud.owner is Player player && player.abstractCreature.world != null && player.abstractCreature.world.game != null && player.abstractCreature.world.game.IsStorySession)
		{
			CampaignTimeTracker campaignTimeTracker = GetCampaignTimeTracker(player.abstractCreature.world.game.GetStorySession.saveStateNumber);
			if (campaignTimeTracker != null && !RainWorld.lockGameTimer)
			{
				timeLabel.text = campaignTimeTracker.TotalFreeTimeSpan.GetIGTFormat(includeMilliseconds: true);
			}
		}
		pos.x -= 95f;
	}

	public Vector2 DrawPos(float timeStacker)
	{
		return Vector2.Lerp(lastPos, pos, timeStacker);
	}

	public override void Draw(float timeStacker)
	{
		float alpha = Mathf.Max(0.2f, Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(lastFade, fade, timeStacker)), 1.5f));
		timeLabel.alignment = FLabelAlignment.Left;
		timeLabel.x = DrawPos(timeStacker).x;
		timeLabel.y = DrawPos(timeStacker).y;
		timeLabel.alpha = alpha;
	}

	public override void ClearSprites()
	{
		base.ClearSprites();
		timeLabel.RemoveFromContainer();
	}

	public static double GetTimerTickIncrement(RainWorldGame game, double dt)
	{
		double num = 0.0;
		if (game.cameras[0].hud.textPrompt.gameOverMode)
		{
			if (!game.Players[0].state.dead || (ModManager.CoopAvailable && game.AlivePlayers.Count > 0))
			{
				num += dt * 1000.0;
			}
		}
		else if (!game.cameras[0].voidSeaMode)
		{
			num += dt * 1000.0;
		}
		return num;
	}

	public static CampaignTimeTracker GetCampaignTimeTracker(SlugcatStats.Name? slugcat)
	{
		if (slugcat == null)
		{
			return null;
		}
		if (!Custom.rainWorld.progression.miscProgressionData.campaignTimers.TryGetValue(slugcat.value, out var value))
		{
			value = new CampaignTimeTracker();
			Custom.rainWorld.progression.miscProgressionData.campaignTimers.Add(slugcat.value, value);
		}
		return value;
	}
}
