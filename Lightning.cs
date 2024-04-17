using System;
using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class Lightning : UpdatableAndDeletable, IRunDuringDialog
{
	public class LightningSource
	{
		private struct ThunderSound
		{
			public int countDown;

			public float distance;

			public float power;

			public float pan;

			public ThunderSound(int countDown, float distance, float power, float pan)
			{
				this.countDown = countDown;
				this.distance = distance;
				this.power = power;
				this.pan = pan;
			}
		}

		public Lightning owner;

		public int wait;

		public int thunder;

		public int thunderLength;

		public float randomLevel;

		public int randomLevelChange;

		public bool frontLightningShowInBkg;

		public float lastIntensity;

		public float intensity;

		public float distance;

		public float pan;

		public float power;

		private bool isBkg;

		private float loopVol;

		private float loopPitch;

		private float loopPan;

		private float noiseCounter;

		private float noiseLevel;

		public DisembodiedDynamicSoundLoop soundLoop;

		private List<ThunderSound> thunderSounds;

		public float tF => 1f - (float)thunder / (float)thunderLength;

		public LightningSource(Lightning owner, bool isBkg)
		{
			this.owner = owner;
			this.isBkg = isBkg;
			thunderSounds = new List<ThunderSound>();
		}

		public void Reset()
		{
			wait = (int)(Mathf.Lerp(10f, isBkg ? 120 : 440, UnityEngine.Random.value) * Mathf.Lerp(1.5f, 1f, owner.intensity) * (1f - owner.CycleEndIntensityBoost));
			frontLightningShowInBkg = UnityEngine.Random.value < 1f / 3f;
			if (isBkg)
			{
				distance = Mathf.Lerp(Mathf.Lerp(1f, 0.5f, owner.intensity), 1f, Mathf.Pow(UnityEngine.Random.value, Mathf.Lerp(0.5f, 2f, owner.intensity)));
				power = UnityEngine.Random.value;
			}
			else
			{
				distance = Mathf.Lerp(Mathf.Lerp(0.25f, 0f, owner.intensity), 0.25f, Mathf.Pow(UnityEngine.Random.value, Mathf.Lerp(0.5f, 2f, owner.intensity)));
				power = Mathf.Lerp(0.7f, 1f, UnityEngine.Random.value);
			}
			pan = Mathf.Lerp(-1f, 1f, UnityEngine.Random.value);
			pan = Mathf.Lerp(pan, 0f, (1f - distance) * Mathf.Pow(UnityEngine.Random.value, 0.5f));
			thunderLength = UnityEngine.Random.Range(1, (int)Mathf.Lerp(10f, 32f, power));
		}

		public void Update()
		{
			owner.room.MakeBackgroundNoise(Mathf.Pow(noiseCounter, 1.4f) * Mathf.Pow(Mathf.InverseLerp(0.1f, 0.7f, noiseLevel), 0.5f));
			if (noiseCounter > 0f)
			{
				noiseCounter = Mathf.Max(0f, noiseCounter - 1f / 120f);
			}
			else
			{
				noiseLevel = 0f;
			}
			if (!owner.bkgOnly)
			{
				if (soundLoop == null)
				{
					soundLoop = new DisembodiedDynamicSoundLoop(owner);
					soundLoop.sound = SoundID.Electricity_Loop;
				}
				else
				{
					soundLoop.Volume = loopVol;
					soundLoop.Pitch = loopPitch;
					soundLoop.Pan = loopPan;
					soundLoop.Update();
				}
			}
			randomLevelChange--;
			if (randomLevelChange < 1)
			{
				randomLevelChange = UnityEngine.Random.Range(1, 6);
				randomLevel = UnityEngine.Random.value;
			}
			if (wait > 0)
			{
				wait--;
				if (wait < 1)
				{
					thunder = thunderLength;
					if (!isBkg || (UnityEngine.Random.value * 0.5f < power * (1f - distance) && !owner.bkgOnly))
					{
						thunderSounds.Add(new ThunderSound((int)(distance * 100f), distance, power, pan));
					}
				}
			}
			else
			{
				thunder--;
				if (thunder < 1)
				{
					Reset();
				}
			}
			ThunderSound thunderSound = default(ThunderSound);
			bool flag = false;
			int num = int.MaxValue;
			for (int num2 = thunderSounds.Count - 1; num2 >= 0; num2--)
			{
				ThunderSound value = thunderSounds[num2];
				value.countDown--;
				thunderSounds[num2] = value;
				if (thunderSounds[num2].countDown < num)
				{
					num = thunderSounds[num2].countDown;
					thunderSound = thunderSounds[num2];
					flag = true;
				}
				if (thunderSounds[num2].countDown < 10 && !owner.bkgOnly)
				{
					owner.room.ScreenMovement(null, new Vector2(0f, 0f), Mathf.Pow((1f - thunderSounds[num2].distance) * thunderSounds[num2].power * owner.intensity, 3f) * Custom.LerpMap(thunderSounds[num2].countDown, 10f, 0f, 0f, 0.9f * Mathf.InverseLerp(0.5f, 1f, owner.intensity)));
				}
				if (thunderSounds[num2].countDown < 1 && !owner.bkgOnly)
				{
					float num3 = (1f - thunderSounds[num2].distance) * thunderSounds[num2].power * Mathf.Pow(Mathf.InverseLerp(0.2f, 1f, owner.intensity), 1.3f);
					float effectAmount = owner.room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.ExtraLoudThunder);
					owner.room.PlaySound(SoundID.Thunder, pan, num3, 1f);
					if (effectAmount > 0f)
					{
						owner.room.PlaySound(SoundID.Thunder_Close, pan, num3 * effectAmount, 1f);
					}
					noiseCounter = 1f;
					noiseLevel = (1f - thunderSounds[num2].distance) * thunderSounds[num2].power * Mathf.Pow(Mathf.InverseLerp(0.2f, 1f, owner.intensity), 1.3f);
					thunderSounds.RemoveAt(num2);
				}
			}
			if (flag)
			{
				loopVol = Mathf.Lerp(loopVol, (1f - thunderSound.distance) * thunderSound.power * owner.intensity, 0.2f);
				loopPitch = Mathf.Lerp(loopPitch, Mathf.Lerp(0.5f, 2f, Mathf.InverseLerp(40f, 5f, num)), 0.2f);
				loopPan = Mathf.Lerp(loopVol, thunderSound.pan, 0.2f);
			}
			else
			{
				loopVol = Mathf.Lerp(loopVol, (1f - distance) * power * owner.intensity * Mathf.InverseLerp(40f, 5f, wait), 0.04f);
				loopPitch = Mathf.Lerp(loopPitch, 1f, 0.5f);
				loopPan = Mathf.Lerp(loopVol, pan, 0.1f);
			}
			loopVol *= Mathf.InverseLerp(0f, 1200f, owner.room.world.rainCycle.TimeUntilRain);
			lastIntensity = intensity;
			if (thunder < 1)
			{
				intensity = 0f;
			}
			else
			{
				intensity = Mathf.Pow(randomLevel, Mathf.Lerp(3f, 0.1f, Mathf.Sin(tF * (float)Math.PI)));
			}
			if (owner.CycleEndIntensityBoost > 0f)
			{
				intensity = Mathf.Lerp(intensity, UnityEngine.Random.value, owner.CycleEndIntensityBoost);
			}
		}

		public float LightIntensity(float timeStacker)
		{
			if (!isBkg && owner.bkgOnly)
			{
				return 0f;
			}
			float num = Mathf.Lerp(lastIntensity, intensity, timeStacker);
			if (UnityEngine.Random.value < 1f / 3f)
			{
				num = Mathf.Lerp(num, (UnityEngine.Random.value < 0.5f) ? 1f : 0f, UnityEngine.Random.value * num);
			}
			return Custom.SCurve(num, 0.5f);
		}
	}

	public float intensity;

	public LightningSource[] lightningSources;

	public Color[] bkgGradient;

	public Color backgroundColor;

	public bool bkgOnly;

	public float CycleEndIntensityBoost
	{
		get
		{
			if (!ModManager.MSC || room.world.region == null || room.world.region.name != "RM" || (room.world.region.name == "RM" && room.abstractRoom.name == "GATE_UW_SS"))
			{
				return Mathf.InverseLerp(room.game.world.rainCycle.cycleLength - 400, room.game.world.rainCycle.cycleLength + 800, room.game.world.rainCycle.timer);
			}
			return 0f;
		}
	}

	public Lightning(Room room, float intensity, bool bkgOnly)
	{
		base.room = room;
		this.intensity = intensity;
		this.bkgOnly = bkgOnly;
		lightningSources = new LightningSource[2];
		for (int i = 0; i < 2; i++)
		{
			lightningSources[i] = new LightningSource(this, i == 1);
		}
		bkgGradient = new Color[2];
		bkgGradient[0] = new Color(10f / 51f, 0.23529412f, 40f / 51f);
		bkgGradient[1] = new Color(18f / 85f, 1f, 19f / 85f);
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		for (int i = 0; i < 2; i++)
		{
			lightningSources[i].Update();
		}
	}

	public float CurrentLightLevel(float timeStacker)
	{
		return Mathf.Lerp(-1f, 1f, lightningSources[0].LightIntensity(timeStacker) * intensity * Mathf.Pow(Mathf.InverseLerp(1.1f, 0f, lightningSources[1].distance) * lightningSources[1].power, 0.4f));
	}

	public Color CurrentBackgroundColor(float timeStacker, RoomPalette pal)
	{
		float num = lightningSources[1].LightIntensity(timeStacker);
		if (lightningSources[0].frontLightningShowInBkg)
		{
			num = Mathf.Max(num, lightningSources[0].LightIntensity(timeStacker));
		}
		int num2 = Custom.IntClamp(Mathf.FloorToInt(Mathf.Pow(num, 10f) * (float)bkgGradient.Length), 0, bkgGradient.Length - 1);
		return Color.Lerp(pal.skyColor, bkgGradient[num2], Mathf.Pow(Mathf.Pow(num, 3f) * Mathf.InverseLerp(1.1f, 0f, lightningSources[1].distance) * lightningSources[1].power * intensity, 1.2f));
	}

	public Color CurrentFogColor(float timeStacker, RoomPalette pal)
	{
		float f = Mathf.Max(lightningSources[0].LightIntensity(timeStacker) * 0.5f, lightningSources[1].LightIntensity(timeStacker));
		int num = Custom.IntClamp(Mathf.FloorToInt(Mathf.Pow(f, 10f) * (float)bkgGradient.Length), 0, bkgGradient.Length - 1);
		f = Mathf.Pow(lightningSources[0].LightIntensity(timeStacker) * Mathf.InverseLerp(1.1f, 0f, lightningSources[0].distance) * lightningSources[0].power, 1.5f) * 0.5f + Mathf.Pow(lightningSources[1].LightIntensity(timeStacker) * Mathf.InverseLerp(1.1f, 0f, lightningSources[1].distance) * lightningSources[1].power, 1.2f) * 0.5f;
		return Color.Lerp(pal.fogColor, bkgGradient[num], f * intensity);
	}
}
