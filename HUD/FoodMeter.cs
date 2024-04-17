using System;
using System.Collections.Generic;
using Menu;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace HUD;

public class FoodMeter : HudPart
{
	public class MeterCircle
	{
		public FoodMeter meter;

		public HUDCircle[] circles;

		public FSprite gradient;

		public int number;

		public float[,] rads;

		public bool plopped;

		public bool foodPlopped;

		public bool eaten;

		public int foodPlopDelay;

		public int eatCounter;

		public float slowXAdd;

		public float lastSlowXAdd;

		public MeterCircle(FoodMeter meter, int number)
		{
			this.meter = meter;
			this.number = number;
			slowXAdd = XAdd(1f);
			lastSlowXAdd = slowXAdd;
		}

		public Vector2 DrawPos(float timeStacker)
		{
			return meter.DrawPos(timeStacker) + new Vector2(Mathf.Lerp(lastSlowXAdd, slowXAdd, timeStacker), 0f);
		}

		public float XAdd(float timeStacker)
		{
			return meter.CircleDistance(timeStacker) * (float)number + ((number >= meter.ShowSurvivalLimit) ? (meter.CircleDistance(timeStacker) / 2f) : 0f);
		}

		public void AddGradient()
		{
			gradient = new FSprite("Futile_White");
			gradient.shader = meter.hud.rainWorld.Shaders["FlatLight"];
			gradient.color = new Color(0f, 0f, 0f);
			meter.fContainer.AddChild(gradient);
		}

		public void AddCircles()
		{
			circles = new HUDCircle[2];
			circles[0] = new HUDCircle(meter.hud, HUDCircle.SnapToGraphic.FoodCircleA, meter.fContainer, 0);
			circles[1] = new HUDCircle(meter.hud, HUDCircle.SnapToGraphic.FoodCircleB, meter.fContainer, 0);
			circles[0].rad = 0f;
			circles[0].lastRad = 0f;
			circles[1].rad = 0f;
			circles[1].lastRad = 0f;
			rads = new float[circles.Length, 2];
			for (int i = 0; i < rads.GetLength(0); i++)
			{
				rads[i, 0] = circles[i].snapRad;
			}
		}

		public void InitPlop()
		{
			rads[0, 0] = circles[0].snapRad - 1.5f;
			rads[0, 1] += 0.5f;
			rads[1, 1] += 0.5f;
			plopped = true;
		}

		public void FoodPlop()
		{
			foodPlopped = true;
			rads[1, 1] += 2f;
			foodPlopDelay = 16;
			meter.hud.PlaySound(SoundID.HUD_Food_Meter_Fill_Plop_A);
		}

		public void QuarterCirclePlop()
		{
			rads[0, 0] += 1.5f;
			meter.hud.PlaySound(SoundID.HUD_Food_Meter_Fill_Quarter_Plop);
			meter.hud.fadeCircles.Add(new FadeCircle(meter.hud, 10f, 4f, 0.82f, 14f, 4f, DrawPos(1f), meter.fContainer));
		}

		public void EatFade()
		{
			eatCounter = 50;
			eaten = true;
			rads[0, 0] = circles[0].snapRad + 1.5f;
			rads[0, 1] += 0.6f;
			meter.hud.PlaySound(SoundID.HUD_Food_Meter_Deplete_Plop_A);
		}

		public void Update()
		{
			lastSlowXAdd = slowXAdd;
			slowXAdd = Custom.LerpAndTick(slowXAdd, XAdd(1f), 0.06f, 2f);
			for (int i = 0; i < rads.GetLength(0); i++)
			{
				if (!plopped)
				{
					rads[i, 0] = circles[i].snapRad / 2f;
					rads[i, 1] = 0f;
					continue;
				}
				rads[i, 0] += rads[i, 1];
				rads[i, 1] *= ((rads[i, 0] < circles[i].snapRad) ? 0.8f : 0.95f);
				rads[i, 1] += (circles[i].snapRad - rads[i, 0]) * 0.2f;
				rads[i, 0] = Custom.LerpAndTick(rads[i, 0], circles[i].snapRad, 0.0001f, 0.2f);
			}
			float num = 1f;
			circles[0].color = 0;
			if (meter.IsPupFoodMeter)
			{
				circles[1].color = 0;
			}
			if (meter.IsPupFoodMeter && meter.notInShelter > 0)
			{
				if (meter.timeCounter % 40 > 20)
				{
					rads[0, 0] *= 0.96f;
					rads[0, 1] *= 0.96f;
					rads[1, 0] *= 0.96f;
					rads[1, 1] *= 0.96f;
				}
				num = 0.65f + 0.35f * Mathf.Sin((float)meter.timeCounter / 20f * (float)Math.PI);
				circles[1].fade = meter.fade * num;
			}
			else if (meter.IsPupFoodMeter && meter.hud.owner.GetOwnerType() == HUD.OwnerType.Player && meter.PupInDanger && meter.deathFade == 0f)
			{
				if (meter.timeCounter % 20 > 10)
				{
					rads[0, 0] *= 0.96f;
					circles[0].color = 1;
					rads[0, 1] *= 0.96f;
					circles[1].color = 1;
				}
				num = 0.65f + 0.35f * Mathf.Sin((float)meter.timeCounter / 20f * (float)Math.PI * 2f);
			}
			else if ((!meter.IsPupFoodMeter || !meter.PupInDanger) && (!meter.IsPupFoodMeter || !meter.PupHasDied) && number < meter.ShowSurvivalLimit && number >= meter.showCount && !eaten && ((meter.hud.owner.GetOwnerType() == HUD.OwnerType.Player && (meter.hud.owner as Player).room != null && (meter.hud.owner as Player).room.abstractRoom.shelter && !(meter.hud.owner as Player).room.world.brokenShelters[(meter.hud.owner as Player).room.abstractRoom.shelterIndex] && !(meter.hud.owner as Player).stillInStartShelter && !(meter.hud.owner as Player).readyForWin) || (meter.hud.owner.GetOwnerType() == HUD.OwnerType.SleepScreen && (meter.hud.owner as SleepAndDeathScreen).goalMalnourished && meter.sleepScreenPhase < 3)))
			{
				if (meter.timeCounter % 20 > 10)
				{
					rads[0, 0] *= 0.96f;
					circles[0].color = 1;
				}
				num = 0.65f + 0.35f * Mathf.Sin((float)meter.timeCounter / 20f * (float)Math.PI * 2f);
			}
			for (int j = 0; j < circles.Length; j++)
			{
				circles[j].Update();
				circles[j].pos = DrawPos(1f);
				circles[j].rad = rads[j, 0];
			}
			circles[0].fade = (plopped ? (meter.fade * num) : 0f);
			if (eaten)
			{
				eatCounter--;
				if (eatCounter < 1)
				{
					foodPlopped = false;
					eaten = false;
				}
				circles[1].fade = Mathf.Pow(Mathf.InverseLerp(0f, 35f, eatCounter), 1.2f) * meter.fade;
				if (eatCounter > 30)
				{
					rads[0, 0] = Custom.LerpAndTick(rads[0, 0], 13f + 0.5f * Mathf.Sin(Mathf.InverseLerp(30f, 50f, eatCounter) * (float)Math.PI), 0.02f, 1.5f + 0.5f * Mathf.InverseLerp(30f, 50f, eatCounter));
					rads[0, 1] *= 0f;
				}
				else if (eatCounter == 29)
				{
					rads[0, 1] = -1.5f;
					meter.hud.PlaySound(SoundID.HUD_Food_Meter_Deplete_Plop_B);
				}
				rads[1, 0] = Custom.LerpMap(eatCounter, 40f, 0f, circles[1].snapRad, circles[1].snapRad / 2f);
			}
			else
			{
				if (meter.refuseCounter > 0)
				{
					rads[0, 0] += Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 0.5f;
					rads[1, 0] += Mathf.Lerp(-0.25f, 1f, UnityEngine.Random.value);
					rads[1, 1] += UnityEngine.Random.value * 0.4f;
					if (rads[1, 0] + 1f > rads[0, 0])
					{
						rads[0, 0] = rads[1, 0] + 1f;
						rads[0, 1] += 0.2f + UnityEngine.Random.value * 0.4f;
					}
				}
				circles[1].fade = ((plopped && foodPlopped) ? meter.fade : 0f);
			}
			if (foodPlopDelay > 0)
			{
				foodPlopDelay--;
				if (foodPlopDelay == 12)
				{
					rads[0, 0] = circles[0].snapRad + 2f;
					rads[0, 1] += 1f;
					meter.hud.PlaySound(SoundID.HUD_Food_Meter_Fill_Plop_B);
				}
				else if (foodPlopDelay == 0)
				{
					meter.hud.fadeCircles.Add(new FadeCircle(meter.hud, 10f, 10f, 0.82f, 30f, 4f, DrawPos(1f), meter.fContainer));
					meter.hud.PlaySound(SoundID.HUD_Food_Meter_Fill_Fade_Circle);
				}
			}
			if (meter.lastFade == 0f)
			{
				plopped = false;
			}
			circles[0].visible = plopped;
			circles[1].visible = plopped && foodPlopped;
		}

		public void Draw(float timeStacker)
		{
			gradient.scale = 6.25f;
			gradient.alpha = 0.1f * Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(meter.lastFade, meter.fade, timeStacker)), 2f);
			Vector2 vector = DrawPos(timeStacker);
			gradient.x = vector.x;
			gradient.y = vector.y;
			for (int i = 0; i < circles.Length; i++)
			{
				circles[i].Draw(timeStacker);
				if (meter.IsPupFoodMeter)
				{
					circles[i].sprite.element = Futile.atlasManager.GetElementWithName(circles[i].snapGraphic.ToString());
					circles[i].sprite.scale = circles[i].rad / circles[i].snapRad;
					circles[i].sprite.alpha = 1f;
					circles[i].sprite.shader = circles[i].basicShader;
					circles[i].sprite.alpha = Mathf.Lerp(circles[i].lastFade, circles[i].fade, timeStacker);
					circles[i].sprite.color = Custom.FadableVectorCircleColors[circles[i].color];
					circles[i].sprite.scale /= 2f;
				}
				circles[i].sprite.x = vector.x;
				circles[i].sprite.y = vector.y;
				if (meter.IsPupFoodMeter)
				{
					circles[i].sprite.color = Color.Lerp(Color.Lerp(circles[i].sprite.color, Custom.HSL2RGB(meter.pup.npcStats.H, Mathf.Lerp(meter.pup.npcStats.S, 1f, 0.8f), meter.pup.npcStats.Dark ? 0.3f : 0.7f), 0.5f - (float)circles[i].color * 0.5f), new Color(0.6f, 0.6f, 0.6f), meter.deathFade);
				}
			}
		}
	}

	public class QuarterPipShower
	{
		public FoodMeter owner;

		public FSprite quarterPips;

		public int displayQuarterFood;

		public int quarterPipDelay;

		public float quarterPipSin;

		public float lastQuarterPipSin;

		public float lightUp;

		public float lastLightUp;

		public QuarterPipShower(FoodMeter owner)
		{
			this.owner = owner;
			quarterPips = new FSprite("pixel");
			owner.fContainer.AddChild(quarterPips);
		}

		public void Reset()
		{
			if (owner.IsPupFoodMeter)
			{
				displayQuarterFood = (owner.pup.State as PlayerNPCState).quarterFoodPoints;
			}
			else
			{
				displayQuarterFood = (owner.hud.owner as Player).playerState.quarterFoodPoints;
			}
		}

		public void Update()
		{
			lastQuarterPipSin = quarterPipSin;
			lastLightUp = lightUp;
			if (displayQuarterFood > 0)
			{
				quarterPipSin += 1f;
			}
			lightUp *= 0.95f;
			if ((owner.IsPupFoodMeter ? (owner.pup.State as PlayerNPCState).quarterFoodPoints : (owner.hud.owner as Player).playerState.quarterFoodPoints) <= displayQuarterFood)
			{
				return;
			}
			owner.visibleCounter = 80;
			if (owner.fade < 0.5f)
			{
				quarterPipDelay = 20;
				return;
			}
			if (quarterPipDelay > 0)
			{
				quarterPipDelay--;
				return;
			}
			quarterPipDelay = 20;
			displayQuarterFood++;
			lightUp = 1f;
			if (owner.showCount < owner.circles.Count)
			{
				owner.circles[owner.showCount].QuarterCirclePlop();
			}
		}

		public void Draw(float timeStacker)
		{
			if (displayQuarterFood < 1 || displayQuarterFood > 3)
			{
				quarterPips.isVisible = false;
				return;
			}
			if (owner.showCount >= owner.circles.Count)
			{
				quarterPips.isVisible = false;
				return;
			}
			quarterPips.isVisible = true;
			quarterPips.element = Futile.atlasManager.GetElementWithName("QuarterPips" + displayQuarterFood);
			quarterPips.alpha = Mathf.Lerp(owner.circles[owner.showCount].circles[0].lastFade, owner.circles[owner.showCount].circles[0].fade, timeStacker) * Mathf.Lerp(Mathf.Lerp(0.2f, 0.5f, Mathf.Pow(Mathf.Clamp01(0.5f + 0.5f * Mathf.Sin(Mathf.Lerp(lastQuarterPipSin, quarterPipSin, timeStacker) / 20f)), 0.4f)), 0.6f, Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(lastLightUp, lightUp, timeStacker)), 2f));
			quarterPips.scale = Mathf.Lerp(owner.circles[owner.showCount].circles[0].lastRad, owner.circles[owner.showCount].circles[0].rad, timeStacker) / owner.circles[owner.showCount].circles[0].snapRad;
			quarterPips.x = owner.circles[owner.showCount].DrawPos(timeStacker).x;
			quarterPips.y = owner.circles[owner.showCount].DrawPos(timeStacker).y;
			quarterPips.color = Custom.FadableVectorCircleColors[owner.circles[owner.showCount].circles[0].color];
		}
	}

	public FSprite lineSprite;

	public FSprite darkFade;

	public List<MeterCircle> circles;

	public Vector2 pos;

	public Vector2 lastPos;

	public float fade;

	public float lastFade;

	public int lastCount;

	public int visibleCounter;

	public int showCount;

	public int showCountDelay;

	public int initPlopCircle;

	public int initPlopDelay;

	public int refuseCounter;

	public int eatCircles;

	public int eatCircleDelay;

	public int timeCounter;

	public float downInCorner;

	public int maxFood;

	public int survivalLimit;

	public QuarterPipShower quarterPipShower;

	public int sleepScreenPhase;

	private float lastShowSurvLim;

	private float showSurvLim;

	private float survLimFrom;

	private float survLimTo;

	private float survLimProg;

	public float forceSleep;

	public List<FoodMeter> pupBars;

	private Player pup;

	public AbstractCreature abstractPup;

	private int pupNumber;

	private int notInShelter;

	private float deathFade;

	private bool showKarmaChange;

	public FContainer fContainer => hud.fContainers[1];

	public int ShowSurvivalLimit => Mathf.RoundToInt(showSurvLim);

	public bool IsPupFoodMeter => pup != null;

	public bool PupHasDied
	{
		get
		{
			if (abstractPup != null)
			{
				return abstractPup.state.dead;
			}
			return true;
		}
	}

	public int CurrentPupFood => (abstractPup.state as PlayerState).foodInStomach;

	public bool PupInDanger
	{
		get
		{
			if (pup != null)
			{
				return pup.dangerGrasp != null;
			}
			bool flag = false;
			for (int i = 0; i < abstractPup.stuckObjects.Count; i++)
			{
				if (abstractPup.stuckObjects[i] is AbstractPhysicalObject.CreatureGripStick && abstractPup.stuckObjects[i].B == abstractPup == (abstractPup.stuckObjects[i] as AbstractPhysicalObject.CreatureGripStick).carry && (abstractPup.stuckObjects[i].A as AbstractCreature).creatureTemplate.type == CreatureTemplate.Type.Slugcat)
				{
					flag = true;
				}
			}
			if (abstractPup != null && !flag)
			{
				return abstractPup.PacifiedBecauseCarried;
			}
			return false;
		}
	}

	public float CircleDistance(float timeStacker)
	{
		if (!IsPupFoodMeter)
		{
			return 30f;
		}
		return Mathf.Lerp(20f, 15f, deathFade);
	}

	public Vector2 DrawPos(float timeStacker)
	{
		return Vector2.Lerp(lastPos, pos, timeStacker);
	}

	public float TotalWidth(float timeStacker)
	{
		return circles[circles.Count - 1].DrawPos(timeStacker).x - circles[0].DrawPos(timeStacker).x + circles[0].circles[0].snapRad;
	}

	public void MoveSurvivalLimit(float to, bool smooth)
	{
		if (to != survLimTo)
		{
			if (smooth)
			{
				survLimFrom = showSurvLim;
				survLimTo = to;
				survLimProg = 0f;
			}
			else
			{
				showSurvLim = to;
				lastShowSurvLim = to;
				survLimTo = to;
				survLimFrom = to;
				survLimProg = 1f;
			}
		}
	}

	public FoodMeter(HUD hud, int maxFood, int survivalLimit, Player associatedPup = null, int pupNumber = 0)
		: base(hud)
	{
		pup = associatedPup;
		if (pup != null)
		{
			abstractPup = pup.abstractCreature;
		}
		this.pupNumber = pupNumber;
		pos = new Vector2(Mathf.Max(50f, hud.rainWorld.options.SafeScreenOffset.x + 5.5f), Mathf.Max(25f, hud.rainWorld.options.SafeScreenOffset.y + 17.25f));
		lastPos = pos;
		this.maxFood = (IsPupFoodMeter ? pup.slugcatStats.maxFood : maxFood);
		this.survivalLimit = (IsPupFoodMeter ? pup.slugcatStats.foodToHibernate : survivalLimit);
		showSurvLim = this.survivalLimit;
		lastShowSurvLim = this.survivalLimit;
		survLimFrom = this.survivalLimit;
		survLimTo = this.survivalLimit;
		survLimProg = 1f;
		circles = new List<MeterCircle>();
		for (int i = 0; i < this.maxFood; i++)
		{
			circles.Add(new MeterCircle(this, i));
			circles[i].AddGradient();
		}
		darkFade = new FSprite("Futile_White");
		darkFade.shader = hud.rainWorld.Shaders["FlatLight"];
		darkFade.color = new Color(0f, 0f, 0f);
		fContainer.AddChild(darkFade);
		lineSprite = new FSprite("pixel");
		lineSprite.scaleX = (IsPupFoodMeter ? 1.5f : 2f);
		lineSprite.scaleY = (IsPupFoodMeter ? 18.5f : 34.5f);
		fContainer.AddChild(lineSprite);
		for (int j = 0; j < circles.Count; j++)
		{
			circles[j].AddCircles();
		}
		if (hud.owner is Player)
		{
			quarterPipShower = new QuarterPipShower(this);
			TrySpawnPupBars();
		}
		if (IsPupFoodMeter || hud.owner.GetOwnerType() == HUD.OwnerType.Player)
		{
			lastCount = (IsPupFoodMeter ? pup.CurrentFood : hud.owner.CurrentFood);
		}
		else if (hud.owner.GetOwnerType() == HUD.OwnerType.SleepScreen)
		{
			lastCount = Math.Min(hud.owner.CurrentFood + survivalLimit, maxFood);
			eatCircleDelay = 65;
			eatCircles = survivalLimit;
		}
		else if (hud.owner.GetOwnerType() == HUD.OwnerType.DeathScreen)
		{
			lastCount = hud.owner.CurrentFood;
			eatCircleDelay = 80;
			eatCircles = 0;
		}
		else if (hud.owner.GetOwnerType() == HUD.OwnerType.CharacterSelect)
		{
			lastCount = hud.owner.CurrentFood;
		}
		NewShowCount(lastCount);
	}

	public void NewShowCount(int cnt)
	{
		showCount = cnt;
		lastCount = cnt;
		for (int i = 0; i < circles.Count; i++)
		{
			circles[i].foodPlopped = i < cnt;
		}
	}

	public void TrySpawnPupBars()
	{
		if (!ModManager.MSC || IsPupFoodMeter || pupBars != null || (hud.owner as Player).room == null || !(hud.owner as Player).room.game.spawnedPendingObjects)
		{
			return;
		}
		int num = 1;
		pupBars = new List<FoodMeter>();
		for (int i = 0; i < (hud.owner as Player).abstractCreature.Room.creatures.Count; i++)
		{
			if ((hud.owner as Player).abstractCreature.Room.creatures[i].state.alive && (hud.owner as Player).abstractCreature.Room.creatures[i].creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)
			{
				FoodMeter foodMeter = new FoodMeter(hud, 0, 0, (hud.owner as Player).abstractCreature.Room.creatures[i].realizedCreature as Player, num);
				hud.AddPart(foodMeter);
				pupBars.Add(foodMeter);
				num++;
			}
		}
	}

	public override void Update()
	{
		for (int num = circles.Count - 1; num >= 0; num--)
		{
			circles[num].Update();
		}
		if (hud.owner is Player)
		{
			TrySpawnPupBars();
		}
		if ((IsPupFoodMeter && abstractPup != null && lastCount != CurrentPupFood) || (!IsPupFoodMeter && lastCount != hud.owner.CurrentFood))
		{
			if (hud.karmaMeter != null && fade == 0f && hud.karmaMeter.fade == 0f)
			{
				downInCorner = 1f;
			}
			lastCount = (IsPupFoodMeter ? CurrentPupFood : hud.owner.CurrentFood);
			visibleCounter = 200;
		}
		if (IsPupFoodMeter && abstractPup != null && abstractPup.realizedCreature != null && abstractPup.realizedCreature != pup)
		{
			pup = abstractPup.realizedCreature as Player;
		}
		lastFade = fade;
		lastPos = pos;
		lastShowSurvLim = showSurvLim;
		if (showSurvLim != survLimTo)
		{
			survLimProg = Mathf.Min(1f, survLimProg + 1f / ((2f + Mathf.Abs(survLimFrom - survLimTo)) * 20f));
			showSurvLim = Mathf.Lerp(survLimFrom, survLimTo, Custom.SCurve(survLimProg, 0.65f));
		}
		if (IsPupFoodMeter || hud.owner.GetOwnerType() == HUD.OwnerType.Player)
		{
			GameUpdate();
		}
		else if (hud.owner.GetOwnerType() == HUD.OwnerType.SleepScreen)
		{
			SleepUpdate();
		}
		else if (hud.owner.GetOwnerType() == HUD.OwnerType.DeathScreen)
		{
			DeathUpdate();
		}
		else if (hud.owner.GetOwnerType() == HUD.OwnerType.CharacterSelect)
		{
			CharSelectUpdate();
		}
		if (fade > 0f)
		{
			if (lastFade == 0f)
			{
				initPlopCircle = -1;
				initPlopDelay = 0;
			}
			if (initPlopCircle < circles.Count)
			{
				initPlopDelay--;
				if (initPlopDelay < 1)
				{
					if (initPlopCircle >= 0)
					{
						circles[initPlopCircle].InitPlop();
					}
					initPlopCircle++;
					initPlopDelay = Math.Max(2, initPlopCircle / 3);
				}
			}
		}
		if (fade > 0f)
		{
			timeCounter++;
		}
		if (refuseCounter > 0)
		{
			refuseCounter--;
		}
		if (notInShelter > 0)
		{
			notInShelter--;
		}
		if (hud.HideGeneralHud)
		{
			fade = 0f;
		}
	}

	private void GameUpdate()
	{
		if (IsPupFoodMeter && ((PupInDanger && deathFade == 0f) || (deathFade > 0f && deathFade < 1f)))
		{
			if (fade < 1f)
			{
				fade = Mathf.Min(1f, fade + 0.1f);
			}
			else
			{
				fade = Mathf.Max(1f, fade - 0.1f);
			}
		}
		if (hud.owner.RevealMap || refuseCounter > 0 || hud.showKarmaFoodRain || (IsPupFoodMeter && notInShelter > 0))
		{
			float num = Mathf.Max((visibleCounter > 0) ? 1f : 0f, 1f);
			if (hud.showKarmaFoodRain)
			{
				num = 1f;
			}
			if (fade < num)
			{
				fade = Mathf.Min(num, fade + 0.1f);
			}
			else
			{
				fade = Mathf.Max(num, fade - 0.1f);
			}
			UpdateShowCount();
		}
		else
		{
			if (visibleCounter > 0)
			{
				if (!ModManager.MMF || !hud.HideGeneralHud)
				{
					visibleCounter--;
				}
				fade = Mathf.Min(1f, fade + 0.1f);
			}
			else
			{
				fade = Mathf.Max(0f, fade - 0.0125f);
			}
			if (fade == 1f)
			{
				UpdateShowCount();
			}
			else if (fade == 0f)
			{
				showCountDelay = 15;
			}
		}
		if (IsPupFoodMeter && PupHasDied)
		{
			deathFade = Mathf.Lerp(deathFade, 1f, 0.05f);
			if (deathFade > 0.98f)
			{
				deathFade = 1f;
			}
		}
		else if (IsPupFoodMeter)
		{
			deathFade = Mathf.Lerp(deathFade, 0f, 0.1f);
			if (deathFade < 0.02f)
			{
				deathFade = 0f;
			}
		}
		if (quarterPipShower != null)
		{
			quarterPipShower.Update();
		}
		if (downInCorner > 0f && hud.karmaMeter.AnyVisibility)
		{
			downInCorner = Mathf.Max(0f, downInCorner - 0.0625f);
		}
		else if (fade > 0f && hud.karmaMeter.fade == 0f && !hud.karmaMeter.AnyVisibility)
		{
			downInCorner = Mathf.Min(1f, downInCorner + 0.0625f);
		}
		if (IsPupFoodMeter && (hud.owner as Player).room != null && (hud.owner as Player).room.abstractRoom.shelter && (pup.room == null || pup.room != (hud.owner as Player).room))
		{
			notInShelter = 10;
		}
		if ((hud.owner as Player).forceSleepCounter > 0)
		{
			forceSleep = Custom.LerpAndTick(forceSleep, Mathf.Pow(Mathf.InverseLerp(10f, 260f, (hud.owner as Player).forceSleepCounter), 0.75f), 0.014f, 1f / Mathf.Lerp(180f, 4f, forceSleep));
		}
		else if ((hud.owner as Player).room != null && (hud.owner as Player).room.abstractRoom.shelter && !(hud.owner as Player).inShortcut && (hud.owner as Player).input[0].y < 0 && !(hud.owner as Player).input[0].jmp && !(hud.owner as Player).input[0].thrw && !(hud.owner as Player).input[0].pckp && (hud.owner as Player).IsTileSolid(1, 0, -1) && ((hud.owner as Player).input[0].x == 0 || ((!(hud.owner as Player).IsTileSolid(1, -1, -1) || !(hud.owner as Player).IsTileSolid(1, 1, -1)) && (hud.owner as Player).IsTileSolid(1, (hud.owner as Player).input[0].x, 0))))
		{
			forceSleep = Mathf.Max(-1f, forceSleep - 1f / 60f);
		}
		else
		{
			forceSleep = Mathf.Max(0f, forceSleep - 1f / 30f);
		}
		if (IsPupFoodMeter)
		{
			MoveSurvivalLimit(Mathf.Lerp(survivalLimit, pup.CurrentFood, forceSleep), smooth: false);
			float num2 = pupNumber;
			if (hud.gourmandmeter != null)
			{
				num2 += (float)hud.gourmandmeter.visibleRows;
			}
			pos = Vector2.Lerp(new Vector2(Mathf.Max(50f, hud.rainWorld.options.SafeScreenOffset.x + 5.5f), Mathf.Max(25f, hud.rainWorld.options.SafeScreenOffset.y + 17.25f) + 5f + 25f * num2), hud.karmaMeter.pos + new Vector2(0f, 25f * num2) + Custom.DegToVec(Mathf.Lerp(90f, 135f, downInCorner)) * (hud.karmaMeter.Radius + 22f + Custom.SCurve(Mathf.Pow(hud.rainMeter.fade, 0.4f), 0.5f) * 8f), Custom.SCurve(1f - downInCorner, 0.5f));
		}
		else
		{
			MoveSurvivalLimit(Mathf.Lerp(survivalLimit, hud.owner.CurrentFood, forceSleep), smooth: false);
			pos = Vector2.Lerp(new Vector2(Mathf.Max(50f, hud.rainWorld.options.SafeScreenOffset.x + 5.5f), Mathf.Max(25f, hud.rainWorld.options.SafeScreenOffset.y + 17.25f)), hud.karmaMeter.pos + Custom.DegToVec(Mathf.Lerp(90f, 135f, downInCorner)) * (hud.karmaMeter.Radius + 22f + Custom.SCurve(Mathf.Pow(hud.rainMeter.fade, 0.4f), 0.5f) * 8f), Custom.SCurve(1f - downInCorner, 0.5f));
		}
	}

	private void UpdateShowCount()
	{
		bool flag = true;
		if (ModManager.MMF && showCount < circles.Count && circles[showCount].eaten)
		{
			flag = false;
		}
		int num = (IsPupFoodMeter ? pup.CurrentFood : hud.owner.CurrentFood);
		if (showCount < num && flag)
		{
			if (showCountDelay == 0)
			{
				showCountDelay = 10;
				if (showCount >= 0 && showCount < circles.Count && !circles[showCount].foodPlopped)
				{
					circles[showCount].FoodPlop();
				}
				showCount++;
				if (quarterPipShower != null)
				{
					quarterPipShower.Reset();
				}
			}
			else
			{
				showCountDelay--;
			}
		}
		else if (showCount > num)
		{
			if (eatCircleDelay == 0)
			{
				eatCircleDelay = 40;
			}
			eatCircleDelay--;
			if (eatCircleDelay < 1)
			{
				circles[showCount - 1].EatFade();
				showCount--;
				eatCircleDelay = 0;
			}
		}
	}

	public void RefuseFood()
	{
		refuseCounter = 10;
	}

	private void SleepUpdate()
	{
		int num = eatCircleDelay;
		if ((hud.owner as SleepAndDeathScreen).AllowFoodMeterTick && showSurvLim == survLimTo)
		{
			if (RWInput.PlayerInput(0).mp)
			{
				eatCircleDelay -= 2;
			}
			eatCircleDelay--;
		}
		switch (sleepScreenPhase)
		{
		case 0:
			if (eatCircles > 0 && showCount - 1 >= 0)
			{
				if (eatCircleDelay < 1)
				{
					circles[showCount - 1].EatFade();
					eatCircles--;
					showCount--;
					eatCircleDelay = ((maxFood > 7) ? 20 : 40);
				}
			}
			else
			{
				if ((hud.owner as SleepAndDeathScreen).startMalnourished || (hud.owner as SleepAndDeathScreen).goalMalnourished)
				{
					sleepScreenPhase = 1;
				}
				else
				{
					sleepScreenPhase = 3;
				}
				eatCircleDelay = 80;
			}
			fade = Custom.LerpAndTick(fade, 0.5f, 0.04f, 1f / 30f);
			break;
		case 1:
			if (eatCircleDelay <= 0 && num > 0)
			{
				if ((hud.owner as SleepAndDeathScreen).goalMalnourished)
				{
					MoveSurvivalLimit(maxFood, smooth: true);
				}
				else
				{
					MoveSurvivalLimit(survivalLimit, smooth: true);
				}
				sleepScreenPhase = 2;
			}
			fade = Custom.LerpAndTick(fade, 0.5f, 0.04f, 1f / 30f);
			break;
		case 2:
			if (showSurvLim == survLimTo)
			{
				sleepScreenPhase = 3;
				eatCircleDelay = 80;
			}
			fade = Custom.LerpAndTick(fade, 0.5f, 0.04f, 1f / 30f);
			break;
		case 3:
			if (eatCircleDelay <= 0 && num > 0)
			{
				hud.owner.FoodCountDownDone();
			}
			fade = Custom.LerpAndTick(fade, Custom.LerpMap(eatCircleDelay, 80f, 20f, 0.5f, 0.1f * (1f - (hud.owner as SleepAndDeathScreen).StarveLabelAlpha(1f))), 0.04f, 1f / 30f);
			break;
		}
		float num2 = Custom.SCurve(Mathf.InverseLerp(-30f, -60f, eatCircleDelay), 0.5f);
		pos.y = Mathf.Max(Mathf.Lerp(450f, 33f, num2), hud.rainWorld.options.SafeScreenOffset.y + 17.25f);
		pos.x = (hud.owner as SleepAndDeathScreen).FoodMeterXPos(num2);
	}

	private void DeathUpdate()
	{
		if ((hud.owner as SleepAndDeathScreen).AllowFoodMeterTick)
		{
			pos.y = Mathf.Max(hud.rainWorld.options.SafeScreenOffset.y + 25.25f, 33f);
			pos.x = Mathf.Max(hud.rainWorld.options.SafeScreenOffset.x - 44.5f, 0f) + (hud.owner as SleepAndDeathScreen).FoodMeterXPos(1f);
			fade = Custom.LerpAndTick(fade, 0.1f, 0.04f, 1f / 30f);
			if (RWInput.PlayerInput(0).mp)
			{
				eatCircleDelay -= 2;
			}
			eatCircleDelay--;
			if (eatCircleDelay < 0 && !showKarmaChange)
			{
				showKarmaChange = true;
				hud.owner.FoodCountDownDone();
			}
		}
	}

	private void CharSelectUpdate()
	{
	}

	public override void Draw(float timeStacker)
	{
		float num = Mathf.Lerp(lastFade, fade, timeStacker);
		for (int num2 = circles.Count - 1; num2 >= 0; num2--)
		{
			circles[num2].Draw(timeStacker);
		}
		lineSprite.x = DrawPos(timeStacker).x + CircleDistance(timeStacker) * (Mathf.Lerp(lastShowSurvLim, showSurvLim, timeCounter) - 0.25f);
		if (hud.owner is Player && (((hud.owner as Player).forceSleepCounter > 10 && (hud.owner as Player).forceSleepCounter < 260) || forceSleep < 0f))
		{
			lineSprite.x += ((timeCounter % 4 < 2) ? (-1.5f) : 1.5f) * Mathf.InverseLerp(0.2f, 0.9f, Mathf.Abs(forceSleep));
		}
		lineSprite.y = DrawPos(timeStacker).y;
		lineSprite.alpha = num;
		if (forceSleep < 0f)
		{
			lineSprite.color = Color.Lerp(Color.white, Color.red, Mathf.Abs(forceSleep) * (0.5f + 0.5f * Mathf.Sin(((float)timeCounter + timeStacker) / 30f * (float)Math.PI * 2f)));
		}
		else
		{
			lineSprite.color = Color.white;
		}
		if (!IsPupFoodMeter)
		{
			darkFade.x = DrawPos(timeStacker).x + CircleDistance(timeStacker) * (float)circles.Count * 0.35f;
			darkFade.y = DrawPos(timeStacker).y - 70f;
			darkFade.scaleX = 37.5f;
			darkFade.scaleY = 18.75f;
			darkFade.alpha = 0.2f * Mathf.Pow(num, 2f);
		}
		if (quarterPipShower != null)
		{
			quarterPipShower.Draw(timeStacker);
		}
	}
}
