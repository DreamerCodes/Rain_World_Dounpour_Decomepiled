using System;
using RWCustom;
using UnityEngine;

public class AntiGravity : UpdatableAndDeletable
{
	public class BrokenAntiGravity
	{
		public int counter;

		public bool on;

		public float from;

		public float to;

		public float progress;

		public float lights;

		public float lightsGetTo;

		public int cycleMin;

		public int cycleMax;

		public RainWorldGame game;

		public float CurrentAntiGravity => Mathf.Lerp(from, to, Custom.SCurve(Mathf.InverseLerp(0.35f, 1f, progress), 0.5f));

		public float CurrentLightsOn => lights;

		public BrokenAntiGravity(int cycleMin, int cycleMax, RainWorldGame game)
		{
			this.cycleMin = cycleMin;
			this.cycleMax = cycleMax;
			this.game = game;
			if (ModManager.MSC)
			{
				if (this.game.world.name == "MS" || this.game.world.name == "SL")
				{
					if (game.IsMoonHeartActive())
					{
						on = true;
					}
					else
					{
						on = false;
						progress = 1f;
					}
				}
				else if (this.game.IsStorySession && this.game.world.name == "RM" && this.game.GetStorySession.saveState.miscWorldSaveData.pebblesEnergyTaken)
				{
					on = false;
				}
				else if (this.game.IsStorySession)
				{
					on = UnityEngine.Random.value < 0.5f;
				}
				else
				{
					on = true;
				}
			}
			else
			{
				on = UnityEngine.Random.value < 0.5f;
			}
			from = (on ? 1f : 0f);
			to = (on ? 1f : 0f);
			lights = to;
		}

		public void Update()
		{
			bool flag = false;
			if (ModManager.MSC)
			{
				if (game.world == null)
				{
					return;
				}
				if (game.IsStorySession && game.world.name == "RM" && game.GetStorySession.saveState.miscWorldSaveData.pebblesEnergyTaken)
				{
					counter = 10;
					progress = 0f;
					from = 0f;
					to = 0f;
					flag = true;
				}
				if (game.world.name != "MS" && game.world.name != "SL")
				{
					if ((game.world.name == "DM" || game.world.name == "LM") && game.world.rainCycle != null)
					{
						if ((float)game.world.rainCycle.preTimer > 0f)
						{
							if (on)
							{
								counter = 0;
							}
						}
						else if (game.world.rainCycle.AmountLeft > 0f || on)
						{
							counter--;
						}
					}
					else
					{
						counter--;
					}
				}
				else if (!game.IsMoonHeartActive())
				{
					counter = 100;
				}
				else
				{
					counter--;
				}
			}
			else
			{
				counter--;
			}
			if (counter < 1)
			{
				on = !on;
				counter = UnityEngine.Random.Range(cycleMin * 40, cycleMax * 40);
				from = to;
				to = (on ? 1f : 0f);
				progress = 0f;
				for (int i = 0; i < game.cameras.Length; i++)
				{
					if (game.cameras[i].room != null && game.cameras[i].room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.BrokenZeroG) > 0f)
					{
						game.cameras[i].room.PlaySound(on ? SoundID.Broken_Anti_Gravity_Switch_On : SoundID.Broken_Anti_Gravity_Switch_Off, 0f, game.cameras[i].room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.BrokenZeroG), 1f);
					}
				}
			}
			if (progress < 1f && !flag)
			{
				progress = Mathf.Min(1f, progress + 1f / 120f);
				if (UnityEngine.Random.value < 0.125f)
				{
					lightsGetTo = Mathf.Lerp(from, to, Mathf.Pow(UnityEngine.Random.value * Mathf.Pow(Mathf.InverseLerp(0f, 0.5f, progress), 0.5f), Custom.LerpMap(progress, 0f, 0.6f, 1f, 0f)));
				}
			}
			else
			{
				lightsGetTo = to;
			}
			lights = Custom.LerpAndTick(lights, lightsGetTo, 0.15f, 0.00083333335f);
			if (!(progress > 0f) || !(progress < 1f) || flag)
			{
				return;
			}
			for (int j = 0; j < game.cameras.Length; j++)
			{
				if (game.cameras[j].room != null && game.cameras[j].room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.BrokenZeroG) > 0f)
				{
					game.cameras[j].room.ScreenMovement(null, new Vector2(0f, 0f), game.cameras[j].room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.BrokenZeroG) * 0.5f * Mathf.Sin(progress * (float)Math.PI));
				}
			}
		}
	}

	public bool active = true;

	public AntiGravity(Room room)
	{
		base.room = room;
		if (room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.BrokenZeroG) > 0f && room.world.rainCycle.brokenAntiGrav == null)
		{
			room.world.rainCycle.brokenAntiGrav = new BrokenAntiGravity(room.game.setupValues.gravityFlickerCycleMin, room.game.setupValues.gravityFlickerCycleMax, room.game);
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (active)
		{
			float num = room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.ZeroG);
			if (num < 1f)
			{
				num = Mathf.Lerp(0f, 0.85f, num);
			}
			if (room.world.rainCycle.brokenAntiGrav != null)
			{
				num = Mathf.Max(num, room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.BrokenZeroG) * room.world.rainCycle.brokenAntiGrav.CurrentAntiGravity);
			}
			room.gravity = 1f - num;
		}
	}
}
