using System;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class RainCycle
{
	public World world;

	public AntiGravity.BrokenAntiGravity brokenAntiGrav;

	public int timer;

	public int cycleLength;

	public int baseCycleLength;

	private bool storyMode;

	private bool speedUpToRain;

	public int rainbowSeed;

	public bool deathRainHasHit;

	private int startUpTicks;

	public int pause;

	public int sunDownStartTime;

	public int dayNightCounter;

	public int duskPalette;

	public int nightPalette;

	public int preTimer;

	public int maxPreTimer;

	public float preCycleRainPulse_WaveA;

	public float preCycleRainPulse_WaveB;

	public float preCycleRainPulse_WaveC;

	public bool challengeForcedPrecycle;

	public FiltrationPowerController filtrationPowerBehavior;

	public int TimeUntilRain => cycleLength - timer;

	public float AmountLeft => (float)(cycleLength - timer) / (float)cycleLength;

	public float RainApproaching
	{
		get
		{
			if (!world.game.IsStorySession)
			{
				return Mathf.InverseLerp(0f, 400f, TimeUntilRain);
			}
			if (ModManager.MSC && preTimer > 0)
			{
				return 1f - Mathf.Clamp((float)preTimer * 4f / (float)maxPreTimer, 0f, 1f);
			}
			return Mathf.InverseLerp(0f, 2400f, TimeUntilRain);
		}
	}

	private float LightChangeBecauseOfRain => Mathf.InverseLerp(0.4f, 1f, RainApproaching);

	public float ShaderLight
	{
		get
		{
			if (RainGameOver)
			{
				return world.game.globalRain.ShaderLight;
			}
			if (storyMode || (ModManager.MSC && challengeForcedPrecycle))
			{
				return -1f + Mathf.Lerp(CycleStartUp, 1f - ProximityToMiddleOfCycle, 0.2f) * 2f * Mathf.InverseLerp(0.4f, 1f, LightChangeBecauseOfRain);
			}
			return Custom.LerpMap(TimeUntilRain, 200f, 880f, -1f, 1f);
		}
	}

	public float RainDarkPalette
	{
		get
		{
			if (storyMode)
			{
				return Mathf.InverseLerp(1f, 0f, LightChangeBecauseOfRain);
			}
			return Mathf.InverseLerp(1000f, 400f, TimeUntilRain);
		}
	}

	public float ScreenShake
	{
		get
		{
			if (ModManager.MSC && preTimer > 0)
			{
				return Mathf.Clamp(preCycleRain_Intensity, 0.15f, 1f) / 3f;
			}
			if (RainGameOver)
			{
				return world.game.globalRain.ScreenShake;
			}
			return Mathf.Pow(1f - Mathf.InverseLerp(0f, 0.2f, RainApproaching), 2f);
		}
	}

	public float MicroScreenShake
	{
		get
		{
			if (ModManager.MSC && preTimer > 0)
			{
				return preCycleRain_Intensity / 5f;
			}
			if (RainGameOver)
			{
				return world.game.globalRain.MicroScreenShake;
			}
			return Mathf.Pow(1f - Mathf.InverseLerp(0f, 0.6f, RainApproaching), 1.5f);
		}
	}

	public bool RainGameOver => timer >= cycleLength;

	public float CycleStartUp
	{
		get
		{
			if (startUpTicks <= 0)
			{
				return 1f;
			}
			return Mathf.InverseLerp(0f, startUpTicks, timer);
		}
	}

	public float CycleProgression => Mathf.InverseLerp(0f, cycleLength, timer);

	public float ProximityToMiddleOfCycle => Mathf.Abs((float)timer - (float)cycleLength / 2f) / ((float)cycleLength / 2f);

	public bool MusicAllowed
	{
		get
		{
			if (world.game.IsArenaSession || TimeUntilRain >= 2400)
			{
				return true;
			}
			if (ModManager.MSC && world.game.IsStorySession && (world.game.GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Saint || world.game.GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Rivulet))
			{
				return true;
			}
			if (world.game.IsStorySession && world.region != null && (world.region.name == "SS" || (ModManager.MSC && (world.region.name == "RM" || world.region.name == "DM" || world.region.name == "LC" || world.region.name == "OE"))))
			{
				return true;
			}
			return false;
		}
	}

	public float DustStormProgress => Mathf.InverseLerp(1000f, 300f, timer);

	public int TimeUntilSunset => sunDownStartTime - timer;

	public float preCycleRain_Intensity
	{
		get
		{
			if (!ModManager.MSC || preTimer == 0 || maxPreTimer == 0)
			{
				return 0f;
			}
			float num = 1f - Mathf.Pow(Mathf.InverseLerp(maxPreTimer, 0f, preTimer), 24f);
			return (Mathf.Sin(preCycleRainPulse_WaveA) + Mathf.Sin(preCycleRainPulse_WaveB) / 2f + Mathf.Cos(preCycleRainPulse_WaveC) * ((float)(preTimer / maxPreTimer) * 2f)) * num;
		}
	}

	public bool RegionHidesTimer
	{
		get
		{
			if (!world.game.IsStorySession)
			{
				return false;
			}
			if (ModManager.MMF && !MMF.cfgHideRainMeterNoThreat.Value)
			{
				return false;
			}
			if (ModManager.MSC && world.game.GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Saint)
			{
				return false;
			}
			if (world.region == null)
			{
				return false;
			}
			if (world.region.name == "SS" || (ModManager.MSC && (world.region.name == "RM" || world.region.name == "LC")))
			{
				return true;
			}
			if (world.region.name == "UW" && world.game.cameras[0].room != null)
			{
				if (world.RoomToWorldPos(new Vector2(0f, 0f), world.game.cameras[0].room.abstractRoom.index).y > 27635f)
				{
					return true;
				}
				return false;
			}
			if (world.region.name == "SB" && world.game.cameras[0].room != null)
			{
				if (world.RoomToWorldPos(new Vector2(0f, 0f), world.game.cameras[0].room.abstractRoom.index).y < 10689f)
				{
					return true;
				}
				return false;
			}
			return false;
		}
	}

	public RainCycle(World world, float minutes)
	{
		this.world = world;
		cycleLength = (int)(minutes * 40f * 60f);
		baseCycleLength = cycleLength;
		storyMode = world.game.IsStorySession;
		duskPalette = 23;
		nightPalette = 10;
		maxPreTimer = 0;
		preTimer = maxPreTimer;
		if (ModManager.MSC && world.game.IsStorySession && world.region != null)
		{
			if (((world.game.GetStorySession.saveState.cycleNumber > 0 || world.game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel) && !world.game.setupValues.disableRain && (!world.game.rainWorld.safariMode || !world.game.rainWorld.safariRainDisable)) || world.game.setupValues.forcePrecycles)
			{
				float num;
				if (world.game.wasAnArtificerDream || world.region.regionParams.earlyCycleChance <= 0f || world.game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Saint || ((world.game.StoryCharacter == SlugcatStats.Name.White || world.game.StoryCharacter == SlugcatStats.Name.Yellow) && world.game.GetStorySession.saveState.cycleNumber < 18))
				{
					num = -1f;
				}
				else if (!(world.game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Rivulet))
				{
					num = ((!(world.game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel)) ? (world.region.regionParams.earlyCycleChance * 100f) : 100f);
				}
				else
				{
					num = (world.game.GetStorySession.saveState.miscWorldSaveData.pebblesEnergyTaken ? 8f : ((!(world.region.name == "VS") && !(world.region.name == "SL") && !(world.region.name == "SH") && !(world.region.name == "GW")) ? 25f : 40f));
					if (world.game.IsMoonActive() && !world.game.GetStorySession.saveState.deathPersistentSaveData.altEnding)
					{
						Custom.Log("Rivulet Kill precycle during epilogue");
						num = -1f;
					}
				}
				if (world.game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel && (world.region.name == "GW" || world.region.name == "UW"))
				{
					num = 0f;
				}
				if (global::MoreSlugcats.MoreSlugcats.cfgDisablePrecycles.Value)
				{
					num = 0f;
				}
				UnityEngine.Random.State state = UnityEngine.Random.state;
				world.game.GetStorySession.SetRandomSeedToCycleSeed(1);
				float num2 = UnityEngine.Random.Range(0, 100);
				Custom.Log("Chance value was:", num2.ToString());
				Custom.Log("Needed to be under", num.ToString());
				if (num2 < num || world.game.setupValues.forcePrecycles)
				{
					maxPreTimer = (int)UnityEngine.Random.Range(4800f, 12000f);
					Custom.Log("SUCCESS precycle length is", maxPreTimer.ToString());
					preTimer = maxPreTimer;
					preCycleRainPulse_WaveA = 0f;
					preCycleRainPulse_WaveB = 0f;
					preCycleRainPulse_WaveC = (float)Math.PI / 2f;
					world.game.globalRain.preCycleRainPulse_Scale = 1f;
				}
				UnityEngine.Random.state = state;
			}
			if (world.game.GetStorySession.saveState.cycleNumber > 0 && !world.game.setupValues.disableRain && !world.game.rainWorld.safariMode && !global::MoreSlugcats.MoreSlugcats.cfgDisablePrecycleFloods.Value && world.game.StoryCharacter != MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel && UnityEngine.Random.value < world.region.regionParams.earlyCycleFloodChance && maxPreTimer > 0)
			{
				if (world.region.name == "MS" && world.game.StoryCharacter != MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
				{
					world.game.globalRain.drainWorldFlood = 0f;
				}
				else
				{
					world.game.globalRain.drainWorldFlood = 99000f;
				}
			}
		}
		if (ModManager.MSC && world.game.IsArenaSession && world.game.GetArenaGameSession.arenaSitting.gameTypeSetup.gameType == MoreSlugcatsEnums.GameTypeID.Challenge && world.game.GetArenaGameSession.arenaSitting.gameTypeSetup.challengeMeta.precycle)
		{
			maxPreTimer = (int)UnityEngine.Random.Range(4800f, 12000f);
			preTimer = maxPreTimer;
			preCycleRainPulse_WaveA = 0f;
			preCycleRainPulse_WaveB = 0f;
			preCycleRainPulse_WaveC = (float)Math.PI / 2f;
			world.game.globalRain.preCycleRainPulse_Scale = 1f;
			challengeForcedPrecycle = true;
		}
		sunDownStartTime = (int)Mathf.Lerp(baseCycleLength, (float)world.game.rainWorld.setup.cycleTimeMax * 60f, UnityEngine.Random.Range(0.02f, 0.045f));
		if (ModManager.MMF && world.game.IsStorySession)
		{
			sunDownStartTime = (int)((float)sunDownStartTime * MMF.cfgRainTimeMultiplier.Value);
		}
		cycleLength = GetDesiredCycleLength();
		for (int i = 0; i < 4; i++)
		{
			PlayerHandler playerHandler = world.game.rainWorld.GetPlayerHandler(i);
			if (playerHandler != null)
			{
				playerHandler.ControllerHandler.WarningSetUp(cycleLength);
			}
		}
		if (world.game.setupValues.cycleStartUp)
		{
			if (world.game.IsStorySession)
			{
				if (world.game.manager.menuSetup.startGameCondition != ProcessManager.MenuSetup.StoryGameInitCondition.Dev && (world.game.session as StoryGameSession).saveState.cycleNumber > 0)
				{
					startUpTicks = 2400;
				}
			}
			else if (world.game.IsArenaSession && world.game.GetArenaGameSession.GameTypeSetup.gameType != ArenaSetup.GameTypeID.Sandbox)
			{
				startUpTicks = 600;
			}
		}
		rainbowSeed = UnityEngine.Random.Range(0, 10000);
	}

	public int GetDesiredCycleLength()
	{
		int num = baseCycleLength;
		if (ModManager.MSC && !world.singleRoomWorld && world.game.IsStorySession && (world.game.session as StoryGameSession).saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Rivulet && !world.game.GetStorySession.saveState.miscWorldSaveData.pebblesEnergyTaken)
		{
			num = ((!(world.region.name == "VS") && !(world.region.name == "UW") && !(world.region.name == "SH") && !(world.region.name == "SB") && !(world.region.name == "SL")) ? ((int)((float)num * 0.33f)) : ((int)((float)num * 0.5f)));
		}
		if (ModManager.MSC && world.game.IsStorySession && (world.game.session as StoryGameSession).saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Saint && world.region != null && world.region.name != "HR" && !(world.game.session as StoryGameSession).saveState.malnourished)
		{
			num = (int)((float)num * 0.7f);
		}
		if (ModManager.MMF && world.game.IsStorySession)
		{
			num = (int)((float)num * MMF.cfgRainTimeMultiplier.Value);
		}
		return num;
	}

	public void Update()
	{
		if (world.game.AllowRainCounterToTick())
		{
			if (pause > 0)
			{
				pause--;
			}
			else if (!ModManager.MSC || preTimer <= 0 || world.game.IsArenaSession)
			{
				if (ModManager.MSC && speedUpToRain && world.game.IsStorySession && world.game.GetStorySession.saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Rivulet && world.game.IsMoonActive() && !world.game.GetStorySession.saveState.deathPersistentSaveData.altEnding)
				{
					Custom.Log("Rivulet kill ticker");
					if (world.game.cameras[0].room != null && world.game.cameras[0].room.abstractRoom.name == "SL_ACCSHAFT")
					{
						timer += 4;
					}
					else if (world.game.cameras[0].room != null && world.game.cameras[0].room.abstractRoom.name == "SL_ROOF04")
					{
						timer += 5;
					}
					else if (world.game.cameras[0].room != null && world.game.cameras[0].room.abstractRoom.name == "SL_MOONTOP")
					{
						timer += 10;
					}
					else
					{
						timer += 3;
					}
				}
				else if (speedUpToRain && !deathRainHasHit)
				{
					if (timer < cycleLength - 800)
					{
						timer += 3;
					}
					else if (timer < cycleLength)
					{
						timer++;
					}
				}
				timer++;
				if (ModManager.MSC && preTimer <= 0)
				{
					if (Mathf.Abs(world.game.globalRain.preCycleRainPulse_Scale) > 0.003f)
					{
						world.game.globalRain.preCycleRainPulse_Scale *= 0.999f;
					}
					else
					{
						world.game.globalRain.preCycleRainPulse_Scale = 0f;
					}
				}
			}
			if (ModManager.MSC && pause <= 0)
			{
				if (!world.game.IsArenaSession && preTimer > 0)
				{
					preTimer--;
				}
				preCycleRainPulse_WaveA += 0.006f;
				preCycleRainPulse_WaveB += 0.01f;
				preCycleRainPulse_WaveC += 0.003f;
				world.game.globalRain.preCycleRainPulse_Intensity = preCycleRain_Intensity;
				if (world.game.globalRain.drainWorldFastDrainCounter > 0)
				{
					world.game.globalRain.drainWorldFastDrainCounter--;
				}
				if (world.game.globalRain.drainWorldFlood > 0f)
				{
					world.game.globalRain.drainWorldFlood -= world.game.globalRain.drainWorldDrainSpeed * (1f + Mathf.InverseLerp(0f, cycleLength / 2, timer) + Mathf.InverseLerp(cycleLength / 2, cycleLength, timer) + 6f * Mathf.InverseLerp((float)(cycleLength / 8) * 7f, cycleLength, timer)) * (0.66f + Mathf.Sin(timer / 30) / 3f);
					if (world.game.IsStorySession)
					{
						if (world.game.GetStorySession.saveStateNumber != MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
						{
							world.game.globalRain.drainWorldFlood -= 0.522f;
						}
						else
						{
							world.game.globalRain.drainWorldFlood -= 0.21f;
						}
					}
					world.game.globalRain.drainWorldFlood += Mathf.Pow(world.game.globalRain.Intensity * 2f, 4f) * 1.85f;
					world.game.globalRain.drainWorldFlood -= ((world.game.GetStorySession.saveStateNumber != MoreSlugcatsEnums.SlugcatStatsName.Rivulet) ? 0.55f : 0.61f) * Mathf.InverseLerp(0f, 200f, world.game.globalRain.drainWorldFastDrainCounter);
				}
				if (world.game.globalRain.drainWorldFlood <= 0f)
				{
					world.game.globalRain.drainWorldFlood = 0f;
				}
			}
			if (pause <= 0 && world.game.IsStorySession && world.game.GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Saint && world.game.cameras[0] != null && timer != 0 && timer % (cycleLength / 3) == 0)
			{
				world.game.cameras[0].SaintJourneyScreenshot();
			}
		}
		if (timer >= sunDownStartTime)
		{
			dayNightCounter++;
			if (ModManager.MSC && dayNightCounter == 2640 && world.region != null && world.region.name == "LC" && world.game.manager.musicPlayer != null && world.game.world.rainCycle.MusicAllowed)
			{
				MusicEvent musicEvent = new MusicEvent();
				musicEvent.cyclesRest = 5;
				musicEvent.stopAtDeath = false;
				musicEvent.stopAtGate = true;
				musicEvent.songName = "RW_27 - Train Tunnels";
				world.game.manager.musicPlayer.GameRequestsSong(musicEvent);
			}
		}
		for (int i = 0; i < 4; i++)
		{
			PlayerHandler playerHandler = world.game.rainWorld.GetPlayerHandler(i);
			if (!(playerHandler != null))
			{
				continue;
			}
			Player player = world.game.RealizedPlayerOfPlayerNumber(i);
			if (player != null)
			{
				bool flag = true;
				if (world.game.IsArenaSession && world.game.GetArenaGameSession.noRain != null)
				{
					flag = false;
				}
				if (flag)
				{
					flag = player.abstractCreature.Room.realizedRoom == null || player.abstractCreature.Room.realizedRoom.roomSettings.DangerType != RoomRain.DangerType.None;
				}
				playerHandler.ControllerHandler.UpdateWarning(timer, TimeUntilRain, flag, !world.game.IsArenaSession);
			}
		}
		if (!deathRainHasHit && timer >= cycleLength)
		{
			bool flag2 = true;
			if (ModManager.MSC && world.game.IsStorySession && world.region != null && world.region.name == "RM")
			{
				flag2 = false;
			}
			if (flag2)
			{
				RainHit();
				deathRainHasHit = true;
			}
		}
		if (ModManager.MSC && deathRainHasHit && world.game.IsStorySession && world.region != null && world.region.name == "RM")
		{
			deathRainHasHit = false;
			world.game.globalRain.deathRain = null;
			world.game.globalRain.RumbleSound = 0f;
			world.game.globalRain.MicroScreenShake = 0f;
			world.game.globalRain.ScreenShake = 0f;
			world.game.globalRain.ShaderLight = -1f;
			world.game.globalRain.floodSpeed = 0f;
			world.game.globalRain.deathRain = null;
		}
		if (brokenAntiGrav != null)
		{
			brokenAntiGrav.Update();
		}
		if (filtrationPowerBehavior != null)
		{
			filtrationPowerBehavior.Update();
		}
		if (!MusicAllowed && world.game.manager.musicPlayer != null && world.game.cameras[0].room.roomSettings.DangerType != RoomRain.DangerType.None && (world.game.manager.musicPlayer.song != null || world.game.manager.musicPlayer.nextSong != null))
		{
			world.game.manager.musicPlayer.RainRequestStopSong();
		}
	}

	public void ArenaEndSessionRain()
	{
		speedUpToRain = true;
		timer = Math.Max(timer, cycleLength - 2000);
	}

	private void RainHit()
	{
		world.game.globalRain.InitDeathRain();
		if (ModManager.MSC)
		{
			world.game.globalRain.FloodHelpUp();
		}
	}
}
