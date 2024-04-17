using System;
using JollyCoop;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class RedsIllness
{
	public class RedsIllnessEffect : CosmeticSprite
	{
		private RedsIllness illness;

		public float fade;

		public float lastFade;

		public float viableFade;

		public float lastViableFade;

		private float rot;

		private float lastRot;

		private float rotDir;

		private float sin;

		public float fluc;

		public float fluc1;

		public float fluc2;

		public float fluc3;

		public DisembodiedDynamicSoundLoop soundLoop;

		public float TotFade(float timeStacker)
		{
			return Mathf.Lerp(lastFade, fade, timeStacker) * Mathf.Lerp(lastViableFade, viableFade, timeStacker);
		}

		public RedsIllnessEffect(RedsIllness illness, Room room)
		{
			this.illness = illness;
			base.room = room;
			rotDir = ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f);
		}

		public static bool CanShowPlayer(Player player)
		{
			if (!player.inShortcut && !player.dead && player.room != null)
			{
				return player.room.ViewedByAnyCamera(player.firstChunk.pos, 100f);
			}
			return false;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (room == null)
			{
				return;
			}
			lastFade = fade;
			lastViableFade = viableFade;
			lastRot = rot;
			sin += 1f / Mathf.Lerp(120f, 30f, fluc3);
			fluc = Custom.LerpAndTick(fluc, fluc1, 0.02f, 1f / 60f);
			fluc1 = Custom.LerpAndTick(fluc1, fluc2, 0.02f, 1f / 60f);
			fluc2 = Custom.LerpAndTick(fluc2, fluc3, 0.02f, 1f / 60f);
			if (Mathf.Abs(fluc2 - fluc3) < 0.01f)
			{
				fluc3 = UnityEngine.Random.value;
			}
			fade = Mathf.Pow(illness.CurrentFitIntensity * (0.85f + 0.15f * Mathf.Sin(sin * (float)Math.PI * 2f)), Mathf.Lerp(1.5f, 0.5f, fluc));
			rot += rotDir * fade * (0.5f + 0.5f * fluc) * 7f * (0.1f + 0.9f * Mathf.InverseLerp(1f, 4f, Vector2.Distance(illness.player.firstChunk.lastLastPos, illness.player.firstChunk.pos)));
			if (!CanShowPlayer(illness.player) || illness.player.room != room || illness.effect != this)
			{
				viableFade = Mathf.Max(0f, viableFade - 1f / 30f);
				if (viableFade <= 0f && lastViableFade <= 0f)
				{
					illness.AbortFit();
					Destroy();
				}
			}
			else
			{
				viableFade = Mathf.Min(1f, viableFade + 1f / 30f);
				pos = (room.game.Players[0].realizedCreature.firstChunk.pos * 2f + room.game.Players[0].realizedCreature.bodyChunks[1].pos) / 3f;
			}
			if (fade == 0f && lastFade > 0f)
			{
				rotDir = ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f);
			}
			if (soundLoop == null && fade > 0f)
			{
				soundLoop = new DisembodiedDynamicSoundLoop(this);
				soundLoop.sound = SoundID.Reds_Illness_LOOP;
				soundLoop.VolumeGroup = 1;
			}
			else if (soundLoop != null)
			{
				soundLoop.Update();
				soundLoop.Volume = Custom.LerpAndTick(soundLoop.Volume, Mathf.Pow((fade + illness.CurrentFitIntensity) / 2f, 0.5f), 0.06f, 1f / 7f);
			}
		}

		public override void Destroy()
		{
			base.Destroy();
			if (soundLoop != null && soundLoop.emitter != null)
			{
				soundLoop.emitter.slatedForDeletetion = true;
			}
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			base.InitiateSprites(sLeaser, rCam);
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite("Futile_White");
			sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["RedsIllness"];
			AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("GrabShaders"));
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
			float num = TotFade(timeStacker);
			if (num == 0f)
			{
				sLeaser.sprites[0].isVisible = false;
				return;
			}
			sLeaser.sprites[0].isVisible = true;
			sLeaser.sprites[0].x = Mathf.Clamp(Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x, 0f, rCam.sSize.x);
			sLeaser.sprites[0].y = Mathf.Clamp(Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y, 0f, rCam.sSize.y);
			sLeaser.sprites[0].rotation = Mathf.Lerp(lastRot, rot, timeStacker);
			sLeaser.sprites[0].scaleX = (rCam.sSize.x * (6f - 3f * num) + 2f) / 16f;
			sLeaser.sprites[0].scaleY = (rCam.sSize.x * (6f - 3f * num) + 2f) / 16f;
			sLeaser.sprites[0].color = new Color(num, num, 0f, 0f);
		}
	}

	public Player player;

	public int cycle;

	public bool init;

	private float floatFood;

	public int counter;

	public bool fadeOutSlow;

	private bool curedForTheCycle;

	public RedsIllnessEffect effect;

	public float fit;

	public float fitLength;

	public float fitSeverity;

	private float totFoodCounter;

	public float Severity => Mathf.Lerp(Custom.LerpMap(cycle, 0f, 6f, 0f, 1f, 0.75f), 0.75f, Mathf.InverseLerp(2720f, 120f, counter) * 0.5f);

	public float CurrentFitIntensity => Mathf.Pow(Mathf.Clamp01(Mathf.Sin(fit * (float)Math.PI) * 1.2f), 1.6f) * fitSeverity;

	private float FoodFac => Mathf.Max(0.1f, 1f / ((float)cycle * 0.5f + 2f));

	public int FoodToBeOkay
	{
		get
		{
			if (player.abstractCreature.world.game.GetStorySession.saveState.malnourished)
			{
				return Math.Max(cycle + 1, player.slugcatStats.maxFood);
			}
			return cycle + 1;
		}
	}

	public float TimeFactor => 1f - 0.9f * Mathf.Max(Mathf.Max(fadeOutSlow ? Mathf.Pow(Mathf.InverseLerp(0f, 0.5f, player.abstractCreature.world.game.manager.fadeToBlack), 0.65f) : 0f, Mathf.InverseLerp(40f * Mathf.Lerp(12f, 21f, Severity), 40f, counter) * Mathf.Lerp(0.2f, 0.5f, Severity)), CurrentFitIntensity * 0.5f);

	public static int RedsCycles(bool extraCycles)
	{
		int num = 19;
		int num2 = 5;
		if (ModManager.MMF && MMF.cfgHunterCycles != null && MMF.cfgHunterBonusCycles != null)
		{
			num = MMF.cfgHunterCycles.Value - 1;
			num2 = MMF.cfgHunterBonusCycles.Value;
		}
		if (!extraCycles)
		{
			return num;
		}
		return num + num2;
	}

	public RedsIllness(Player player, int cycle)
	{
		this.player = player;
		this.cycle = cycle;
		floatFood = player.playerState.foodInStomach;
		if (ModManager.CoopAvailable)
		{
			JollyCustom.Log($"Applying redillness to player {player}");
		}
	}

	public void Update()
	{
		if (player.room == null)
		{
			return;
		}
		if (!init)
		{
			init = true;
			player.SetMalnourished(m: true);
		}
		counter++;
		if (fit > 0f)
		{
			fit += 1f / fitLength;
			player.aerobicLevel = Mathf.Max(player.aerobicLevel, Mathf.Pow(CurrentFitIntensity, 1.5f));
			if (CurrentFitIntensity > 0.7f)
			{
				player.Blink(6);
			}
			if (fit > 1f)
			{
				fit = 0f;
			}
		}
		else if (!curedForTheCycle && UnityEngine.Random.value < 1f / (60f + Mathf.Lerp(0.1f, 0.001f, Severity) * (float)counter))
		{
			fitSeverity = Custom.SCurve(Mathf.Pow(UnityEngine.Random.value, Mathf.Lerp(3.4f, 0.4f, Severity)), 0.7f);
			fitLength = Mathf.Lerp(80f, 240f, Mathf.Pow(UnityEngine.Random.value, Mathf.Lerp(1.6f, 0.4f, (fitSeverity + Severity) / 2f)));
			fitSeverity = Mathf.Pow(fitSeverity, Mathf.Lerp(1.4f, 0.4f, Severity));
			fit += 1f / fitLength;
		}
		if (effect == null && CurrentFitIntensity > 0f && RedsIllnessEffect.CanShowPlayer(player))
		{
			effect = new RedsIllnessEffect(this, player.room);
			player.room.AddObject(effect);
		}
		else if (effect != null && (!RedsIllnessEffect.CanShowPlayer(player) || effect.slatedForDeletetion))
		{
			effect = null;
		}
	}

	public void GetBetter()
	{
		Custom.Log("Player now feels better!");
		curedForTheCycle = true;
		player.SetMalnourished(m: false);
	}

	public void AddFood(int i)
	{
		float num = Math.Min((float)i * FoodFac, player.slugcatStats.maxFood - player.playerState.foodInStomach);
		totFoodCounter += num / FoodFac;
		floatFood += num;
		UpdateFood();
	}

	public void AddQuarterFood()
	{
		float num = Math.Min(0.25f * FoodFac, player.slugcatStats.maxFood - player.playerState.foodInStomach);
		totFoodCounter += num / FoodFac;
		floatFood += num;
		UpdateFood();
	}

	private void UpdateFood()
	{
		player.playerState.foodInStomach = Custom.IntClamp(Mathf.FloorToInt(floatFood), 0, player.slugcatStats.maxFood);
		if (player.playerState.foodInStomach >= player.slugcatStats.maxFood)
		{
			player.playerState.quarterFoodPoints = 0;
		}
		else
		{
			player.playerState.quarterFoodPoints = Mathf.FloorToInt(Custom.Decimal(floatFood) * 4f);
		}
		while (totFoodCounter >= 1f)
		{
			totFoodCounter -= 1f;
			player.abstractCreature.world.game.GetStorySession.saveState.totFood++;
		}
	}

	public void AbortFit()
	{
		fit = 0f;
	}
}
