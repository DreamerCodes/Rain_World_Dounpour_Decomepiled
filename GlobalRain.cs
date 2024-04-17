using System;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class GlobalRain
{
	public class DeathRain
	{
		public class DeathRainMode : ExtEnum<DeathRainMode>
		{
			public static readonly DeathRainMode None = new DeathRainMode("None", register: true);

			public static readonly DeathRainMode CalmBeforeStorm = new DeathRainMode("CalmBeforeStorm", register: true);

			public static readonly DeathRainMode GradeABuildUp = new DeathRainMode("GradeABuildUp", register: true);

			public static readonly DeathRainMode GradeAPlateu = new DeathRainMode("GradeAPlateu", register: true);

			public static readonly DeathRainMode GradeBBuildUp = new DeathRainMode("GradeBBuildUp", register: true);

			public static readonly DeathRainMode GradeBPlateu = new DeathRainMode("GradeBPlateu", register: true);

			public static readonly DeathRainMode FinalBuildUp = new DeathRainMode("FinalBuildUp", register: true);

			public static readonly DeathRainMode Mayhem = new DeathRainMode("Mayhem", register: true);

			public static readonly DeathRainMode AlternateBuildUp = new DeathRainMode("AlternateBuildUp", register: true);

			public DeathRainMode(string value, bool register = false)
				: base(value, register)
			{
			}
		}

		public GlobalRain globalRain;

		public DeathRainMode deathRainMode = DeathRainMode.None;

		private float timeInThisMode;

		private float progression;

		private float calmBeforeStormSunlight;

		public DeathRain(GlobalRain globalRain)
		{
			this.globalRain = globalRain;
			NextDeathRainMode();
		}

		public void DeathRainUpdate()
		{
			progression += 1f / timeInThisMode * (globalRain.game.IsArenaSession ? 3.2f : 1f);
			bool flag = false;
			if (progression > 1f)
			{
				progression = 1f;
				flag = true;
			}
			if (deathRainMode == DeathRainMode.CalmBeforeStorm)
			{
				globalRain.RumbleSound = Mathf.Max(globalRain.RumbleSound - 0.025f, 0f);
			}
			else
			{
				globalRain.RumbleSound = Mathf.Lerp(globalRain.RumbleSound, 1f - Mathf.InverseLerp(0f, 0.6f, globalRain.game.world.rainCycle.RainApproaching), 0.2f);
			}
			if (deathRainMode == DeathRainMode.CalmBeforeStorm)
			{
				globalRain.Intensity = Mathf.Pow(Mathf.InverseLerp(0.15f, 0f, progression), 1.5f) * 0.24f;
				globalRain.ShaderLight = -1f + 0.3f * Mathf.Sin(Mathf.InverseLerp(0.03f, 0.8f, progression) * (float)Math.PI) * calmBeforeStormSunlight;
				globalRain.bulletRainDensity = Mathf.Pow(Mathf.InverseLerp(0.3f, 1f, progression), 8f);
			}
			else if (deathRainMode == DeathRainMode.GradeABuildUp)
			{
				globalRain.Intensity = progression * 0.6f;
				globalRain.MicroScreenShake = progression * 1.5f;
				globalRain.bulletRainDensity = 1f - progression;
			}
			else if (!(deathRainMode == DeathRainMode.GradeAPlateu))
			{
				if (deathRainMode == DeathRainMode.GradeBBuildUp)
				{
					globalRain.Intensity = Mathf.Lerp(0.6f, 0.71f, progression);
					globalRain.MicroScreenShake = Mathf.Lerp(1.5f, 2.1f, progression);
					globalRain.ScreenShake = progression * 1.2f;
				}
				else if (!(deathRainMode == DeathRainMode.GradeBPlateu))
				{
					if (deathRainMode == DeathRainMode.FinalBuildUp)
					{
						globalRain.Intensity = Mathf.Lerp(0.71f, 1f, progression);
						globalRain.MicroScreenShake = Mathf.Lerp(2.1f, 4f, Mathf.Pow(progression, 1.2f));
						globalRain.ScreenShake = Mathf.Lerp(1.2f, 3f, progression);
					}
					else if (deathRainMode == DeathRainMode.AlternateBuildUp)
					{
						globalRain.Intensity = Mathf.Lerp(0.24f, 0.6f, progression);
						globalRain.MicroScreenShake = 1f + progression * 0.5f;
					}
					else if (ModManager.MSC && deathRainMode == MoreSlugcatsEnums.DeathRainMode.Pulses)
					{
						float num3;
						if (progression <= 0.9f)
						{
							float num = (1f - progression) * 50f;
							float num2 = 0.4f + Mathf.Sin(progression / (num / 3f));
							num3 = progression * timeInThisMode / timeInThisMode + Mathf.Sin(progression * timeInThisMode / num) / (timeInThisMode / (progression * timeInThisMode));
							if (progression > 0.6f && Mathf.Abs(num3 - num2) < 0.1f)
							{
								num3 *= num2;
							}
							globalRain.bulletRainDensity = Mathf.Lerp(0.1f, 1f, num2 - 0.4f);
							num3 = Mathf.Clamp(num3, progression * 0.6f, 1f);
						}
						else
						{
							globalRain.bulletRainDensity = 0f;
							num3 = 1f;
						}
						float t = 0.25f * Mathf.InverseLerp(0f, 0.1f, progression);
						globalRain.Intensity = Mathf.Lerp(globalRain.Intensity, Mathf.Lerp(0f, 0.75f, num3), t);
						globalRain.MicroScreenShake = (1f + progression * 0.65f) * (num3 + 0.25f);
						globalRain.ScreenShake = Mathf.Lerp(globalRain.ScreenShake, globalRain.MicroScreenShake, 0.3f);
					}
				}
			}
			if (flag)
			{
				NextDeathRainMode();
			}
		}

		public void NextDeathRainMode()
		{
			if (deathRainMode == DeathRainMode.Mayhem)
			{
				return;
			}
			if (ModManager.MSC && deathRainMode == DeathRainMode.None && globalRain.game.IsStorySession && globalRain.game.cameras[0].room != null && globalRain.game.cameras[0].room.abstractRoom.name == "MS_CORE")
			{
				deathRainMode = DeathRainMode.GradeAPlateu;
				Custom.Log("RainMode: MS_CORE FORCED standard");
			}
			else if (ModManager.MSC && globalRain.game.IsStorySession && globalRain.game.GetStorySession.saveState != null && globalRain.game.GetStorySession.saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Rivulet && globalRain.game.IsMoonActive() && !globalRain.game.GetStorySession.saveState.deathPersistentSaveData.altEnding)
			{
				deathRainMode = DeathRainMode.GradeAPlateu;
				Custom.Log("RainMode: rivulet epilogue forced rain type to standard");
			}
			else if (deathRainMode == DeathRainMode.None && (UnityEngine.Random.value < 0.7f || globalRain.game.IsArenaSession))
			{
				if (!ModManager.MSC || UnityEngine.Random.value < 0.7f)
				{
					deathRainMode = DeathRainMode.AlternateBuildUp;
				}
				else
				{
					deathRainMode = MoreSlugcatsEnums.DeathRainMode.Pulses;
				}
			}
			else if (deathRainMode == DeathRainMode.AlternateBuildUp)
			{
				deathRainMode = DeathRainMode.GradeAPlateu;
			}
			else if (ModManager.MSC && deathRainMode == MoreSlugcatsEnums.DeathRainMode.Pulses)
			{
				deathRainMode = DeathRainMode.FinalBuildUp;
			}
			else
			{
				string entry = ExtEnum<DeathRainMode>.values.GetEntry(deathRainMode.Index + 1);
				if (entry == null)
				{
					deathRainMode = DeathRainMode.Mayhem;
				}
				else
				{
					deathRainMode = new DeathRainMode(entry);
				}
			}
			progression = 0f;
			if (deathRainMode == DeathRainMode.CalmBeforeStorm)
			{
				timeInThisMode = Mathf.Lerp(400f, 800f, UnityEngine.Random.value);
				calmBeforeStormSunlight = ((UnityEngine.Random.value < 0.5f) ? 0f : UnityEngine.Random.value);
			}
			else if (deathRainMode == DeathRainMode.GradeABuildUp)
			{
				timeInThisMode = 6f;
				globalRain.ShaderLight = -1f;
			}
			else if (deathRainMode == DeathRainMode.GradeAPlateu)
			{
				timeInThisMode = Mathf.Lerp(400f, 600f, UnityEngine.Random.value);
			}
			else if (deathRainMode == DeathRainMode.GradeBBuildUp)
			{
				timeInThisMode = ((UnityEngine.Random.value < 0.5f) ? 100f : Mathf.Lerp(50f, 300f, UnityEngine.Random.value));
			}
			else if (deathRainMode == DeathRainMode.GradeBPlateu)
			{
				timeInThisMode = ((UnityEngine.Random.value < 0.5f) ? 100f : Mathf.Lerp(50f, 300f, UnityEngine.Random.value));
			}
			else if (deathRainMode == DeathRainMode.FinalBuildUp)
			{
				timeInThisMode = ((UnityEngine.Random.value < 0.5f) ? Mathf.Lerp(300f, 500f, UnityEngine.Random.value) : Mathf.Lerp(100f, 800f, UnityEngine.Random.value));
			}
			else if (deathRainMode == DeathRainMode.AlternateBuildUp)
			{
				timeInThisMode = Mathf.Lerp(400f, 1200f, UnityEngine.Random.value);
			}
			else if (ModManager.MSC && deathRainMode == MoreSlugcatsEnums.DeathRainMode.Pulses)
			{
				timeInThisMode = Mathf.Lerp(1000f, 2600f, UnityEngine.Random.value);
			}
		}
	}

	public RainWorldGame game;

	public float lastRainDirection;

	public float rainDirection;

	private float rainDirectionGetTo;

	public float Intensity;

	public float ShaderLight = -1f;

	public float ScreenShake;

	public float MicroScreenShake;

	public float RumbleSound;

	public float bulletRainDensity;

	public DeathRain deathRain;

	public float flood;

	public float floodSpeed;

	public int bulletTimer;

	public int heavyTimer;

	public float floodLerpSpeed;

	public int waterFluxTicker;

	public float preCycleRainPulse_Intensity;

	public float preCycleRainPulse_Scale;

	public bool forceSlowFlood;

	public float drainWorldDrainSpeed;

	public float drainWorldFlood;

	public int drainWorldFastDrainCounter;

	public bool drainWorldFloodFlag;

	public float OutsidePushAround => Mathf.Pow(Mathf.InverseLerp(0.35f, 0.7f, Intensity), 0.8f);

	public float InsidePushAround => Mathf.Pow(Mathf.InverseLerp(0.63f, 0.98f, Intensity), 3.5f);

	public bool AnyPushAround
	{
		get
		{
			if (!(OutsidePushAround > 0f))
			{
				return InsidePushAround > 0f;
			}
			return true;
		}
	}

	public GlobalRain(RainWorldGame game)
	{
		this.game = game;
		preCycleRainPulse_Intensity = 0f;
		preCycleRainPulse_Scale = 0f;
		if (ModManager.MSC)
		{
			DrainWorldFloodInit(new WorldCoordinate(0, 0, 0, -1));
		}
	}

	public void Update()
	{
		if (UnityEngine.Random.value < 0.025f)
		{
			rainDirectionGetTo = Mathf.Lerp(-1f, 1f, UnityEngine.Random.value);
		}
		lastRainDirection = rainDirection;
		rainDirection = Mathf.Lerp(rainDirection, rainDirectionGetTo, 0.01f);
		if (rainDirection < rainDirectionGetTo)
		{
			rainDirection = Mathf.Min(rainDirection + 0.0125f, rainDirectionGetTo);
		}
		else if (rainDirection > rainDirectionGetTo)
		{
			rainDirection = Mathf.Max(rainDirection - 0.0125f, rainDirectionGetTo);
		}
		waterFluxTicker++;
		floodLerpSpeed = 0.2f;
		if (deathRain != null)
		{
			deathRain.DeathRainUpdate();
			bool flag = ((!ModManager.MSC) ? ((int)deathRain.deathRainMode > (int)DeathRain.DeathRainMode.GradeABuildUp) : (!forceSlowFlood));
			if (game.IsStorySession && flag)
			{
				if (ModManager.MSC)
				{
					float num = 0.5f + Mathf.Sin(flood / 100f) / 2f;
					num /= 3f;
					if (deathRain.deathRainMode == DeathRain.DeathRainMode.AlternateBuildUp)
					{
						num += Mathf.InverseLerp(-1000f, 0f, game.world.rainCycle.TimeUntilRain) * 0.7f;
						num += Mathf.InverseLerp(-6000f, 0f, game.world.rainCycle.TimeUntilRain) * 5f;
					}
					else
					{
						num += Mathf.InverseLerp(-8000f, 0f, game.world.rainCycle.TimeUntilRain) * 0.7f;
						num += Mathf.InverseLerp(-1300f, 0f, game.world.rainCycle.TimeUntilRain) * 5f;
					}
					floodSpeed = Mathf.Min(1.1f + num, floodSpeed + 0.025f);
				}
				else
				{
					floodSpeed = Mathf.Min(0.8f, floodSpeed + 0.0025f);
				}
			}
			else if ((ModManager.MSC || game.IsArenaSession) && (int)deathRain.deathRainMode >= (int)DeathRain.DeathRainMode.GradeABuildUp)
			{
				floodSpeed = Mathf.Min(1.8f, floodSpeed + 1f / 150f);
			}
			flood += floodSpeed;
			if (ModManager.MSC && Mathf.Max(game.cameras[0].room.world.RoomToWorldPos(game.cameras[0].pos, game.cameras[0].room.abstractRoom.index).y - 500f, game.cameras[0].room.world.RoomToWorldPos(new Vector2(0f, 0f), game.cameras[0].room.abstractRoom.index).y) <= flood)
			{
				forceSlowFlood = true;
			}
			return;
		}
		Intensity = Mathf.InverseLerp(600f, 200f, game.world.rainCycle.TimeUntilRain) * 0.24f;
		if (ModManager.MSC)
		{
			flood = 1f + drainWorldFlood;
			floodLerpSpeed = 1f;
		}
		bulletRainDensity = 0f;
		if (game.cameras.Length == 0 || game.cameras[0].room == null)
		{
			return;
		}
		Room room = game.cameras[0].room;
		float num2 = room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.LightRain);
		float effectAmount = room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.HeavyRain);
		float num3 = room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.HeavyRainFlux);
		float num4 = room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.BulletRain);
		float effectAmount2 = room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.BulletRainFlux);
		if (ModManager.MSC && preCycleRainPulse_Scale != 0f)
		{
			float num5 = num2;
			float num6 = num3;
			float num7 = num4;
			num2 = (1f + Mathf.Clamp(preCycleRainPulse_Intensity, 0f, 1f)) * preCycleRainPulse_Scale;
			num3 = Mathf.Clamp(num2 - 0.9f, 0f, 0.8f);
			if (room.roomSettings.DangerType == RoomRain.DangerType.Rain)
			{
				num4 = Mathf.Clamp(num2 - 0.9f, 0f, 1f);
			}
			if (num3 > 0.4f)
			{
				num4 = 0f;
			}
			if (num2 < num5)
			{
				num2 = num5;
			}
			if (num3 < num6)
			{
				num3 = num6;
			}
			if (num2 < num7)
			{
				num2 = num7;
			}
		}
		float num8 = effectAmount;
		if (num3 > 0f)
		{
			float num9 = 1200f * num3;
			float num10 = 60f;
			heavyTimer = (heavyTimer + 1) % (int)(num10 * 2f + num9 * 2f);
			if ((float)heavyTimer < num10)
			{
				num8 *= (float)heavyTimer / num10;
			}
			else if ((float)heavyTimer >= num10 + num9 && (float)heavyTimer < num10 * 2f + num9)
			{
				num8 *= 1f - ((float)heavyTimer - (num9 + num10)) / num10;
			}
			else if ((float)heavyTimer >= num10 * 2f + num9)
			{
				num8 = 0f;
			}
		}
		if (num8 > 0f)
		{
			Intensity = (1f + num8 * 4f) * 0.24f;
			RumbleSound = num8 * 0.2f;
			ScreenShake = num8;
		}
		else if (num2 > 0f)
		{
			Intensity = num2 * 0.24f;
		}
		float num11 = num4;
		if (effectAmount2 > 0f)
		{
			float num12 = 1200f * effectAmount2;
			float num13 = 60f;
			bulletTimer = (bulletTimer + 1) % (int)(num13 * 2f + num12 * 2f);
			if ((float)bulletTimer < num13)
			{
				num11 *= (float)bulletTimer / num13;
			}
			else if ((float)bulletTimer >= num13 + num12 && (float)bulletTimer < num13 * 2f + num12)
			{
				num11 *= 1f - ((float)bulletTimer - (num12 + num13)) / num13;
			}
			else if ((float)bulletTimer >= num13 * 2f + num12)
			{
				num11 = 0f;
			}
		}
		if (num11 > 0f)
		{
			bulletRainDensity = num11;
		}
	}

	public void InitDeathRain()
	{
		deathRain = new DeathRain(this);
	}

	public bool DrainWorldFloodInit(WorldCoordinate StartPos)
	{
		drainWorldDrainSpeed = 0.45f;
		if (game.IsArenaSession)
		{
			drainWorldFlood = 0f;
			return false;
		}
		if (StartPos == new WorldCoordinate(0, 0, 0, -1) || game.cameras[0].room == null || game.cameras[0].room.world == null)
		{
			drainWorldFlood = 0f;
		}
		else
		{
			drainWorldFlood = game.cameras[0].room.world.RoomToWorldPos(new Vector2(StartPos.x * 20, (float)(StartPos.y * 20) - UnityEngine.Random.Range(140f, 180f)), StartPos.room).y;
		}
		Custom.Log("Drainworld flood set to", drainWorldFlood.ToString());
		return true;
	}

	public bool DrainWorldPositionFlooded(WorldCoordinate pos)
	{
		if (game.world == null)
		{
			return false;
		}
		if (game.world.GetAbstractRoom(pos.room) == null)
		{
			return drainWorldFlood > 0f;
		}
		if (game != null && game.world != null && drainWorldFlood > 0f)
		{
			return drainWorldFlood >= game.world.RoomToWorldPos(new Vector2(pos.x * 20, pos.y * 20), pos.room).y;
		}
		return false;
	}

	public void ResetRain()
	{
		if (deathRain != null)
		{
			deathRain.globalRain = null;
		}
		deathRain = null;
		forceSlowFlood = false;
		RumbleSound = 0f;
		MicroScreenShake = 0f;
		ScreenShake = 0f;
		ShaderLight = -1f;
		floodSpeed = 0f;
		drainWorldFlood = 0f;
		game.world.rainCycle.timer = 0;
		game.world.rainCycle.deathRainHasHit = false;
		game.cameras[0].hud.rainMeter.ResetHalfTime();
	}

	public void FloodHelpUp()
	{
		drainWorldFlood = 0f;
		if (game.IsStorySession)
		{
			if (game.cameras[0] != null && game.cameras[0].room != null)
			{
				if (flood == 1f && game.world.rainCycle.TimeUntilRain == 0)
				{
					Custom.Log("First flood frame! Set flood to float minimum!");
					flood = float.MinValue;
				}
				float y = game.cameras[0].room.world.RoomToWorldPos(game.cameras[0].pos, game.cameras[0].room.abstractRoom.index).y;
				y += UnityEngine.Random.Range(-4000f, -4100f);
				if (flood < y)
				{
					flood = y;
				}
				Custom.Log("RAIN HELPED FLOOD UP!", flood.ToString());
			}
		}
		else if (game.cameras[0] != null && game.cameras[0].room != null)
		{
			if (flood == 1f && game.world.rainCycle.TimeUntilRain == 0)
			{
				Custom.Log("First flood frame! Set flood to float minimum!");
				flood = float.MinValue;
			}
			float y2 = game.cameras[0].room.world.RoomToWorldPos(game.cameras[0].pos, game.cameras[0].room.abstractRoom.index).y;
			y2 += UnityEngine.Random.Range(-10f, -20f);
			if (flood < y2)
			{
				flood = y2;
			}
			Custom.Log("RAIN HELPED FLOOD UP!", flood.ToString());
		}
	}
}
