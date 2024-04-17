using System;
using RWCustom;
using UnityEngine;

public class MouseGraphics : GraphicsModule, ILookingAtCreatures
{
	public CreatureLooker creatureLooker;

	public BodyPart head;

	private BodyPart tail;

	private Vector2 lookDir;

	private Vector2 lastLookDir;

	private float profileFac;

	private float lastProfileFac;

	private float backToCam;

	private float lastBackToCam;

	public int ouchEyes;

	public Vector2[,] ropePositions;

	private Limb[,] limbs;

	private int blink;

	private float lastRunCycle;

	private float runCycle;

	private bool handsOutWhileHanging;

	private bool hangBackwards;

	private Color blackColor;

	public LightSource lightSource;

	private float lightMode;

	private float flickeringFac;

	private float flicker;

	private float flickerDuration;

	private float charging;

	private float superCharge;

	private float breath;

	private bool lastConsious;

	private float unconsiousProfileFac;

	private float litRoom;

	private ChunkDynamicSoundLoop soundLoop;

	public LanternMouse mouse => base.owner as LanternMouse;

	public int HeadSprite => 14;

	public int TotalSprites => 21;

	public float LightStrength => (1f - Mathf.Clamp(charging, 0f, 1f) * 0.6f) * Battery;

	private float Battery => Custom.SCurve(Mathf.InverseLerp(0f, 4000f, mouse.State.battery), 0.2f);

	public Color BodyColor
	{
		get
		{
			HSLColor from = new HSLColor(mouse.iVars.color.hue, 1f - flicker * 0.3f, 0.9f - flicker * 0.5f);
			if (charging > 0f)
			{
				from = HSLColor.Lerp(from, new HSLColor(mouse.iVars.color.hue, 0.05f, 0.1f), charging);
			}
			if (Battery < 1f)
			{
				from = HSLColor.Lerp(from, new HSLColor(mouse.iVars.color.hue, 0f, 0.1f), 1f - Battery);
			}
			return Color.Lerp(from.rgb, blackColor, litRoom * 0.6f);
		}
	}

	public Color DecalColor
	{
		get
		{
			HSLColor from = new HSLColor(mouse.iVars.color.hue, 0.3f, 0.45f * (1f - flicker));
			if (charging > 0f)
			{
				from = HSLColor.Lerp(from, new HSLColor(mouse.iVars.color.hue, Mathf.Clamp(charging, 0f, 1f), 0f), Mathf.InverseLerp(0f, 0.3f, charging));
				if (charging > 0.3f)
				{
					from = HSLColor.Lerp(from, new HSLColor(mouse.iVars.color.hue, Mathf.Clamp(charging, 0f, 1f), 0.5f), Mathf.InverseLerp(0.3f, 1f, charging));
				}
			}
			if (Battery < 1f)
			{
				from = HSLColor.Lerp(from, new HSLColor(mouse.iVars.color.hue, 0.6f, 0.08f), 1f - Battery);
			}
			return Color.Lerp(from.rgb, blackColor, litRoom * 0.7f);
		}
	}

	public Color EyesColor
	{
		get
		{
			if (charging == 0f)
			{
				return blackColor;
			}
			return Color.Lerp(blackColor, new Color(1f, 1f, 1f), Mathf.InverseLerp(0f, 0.25f, charging));
		}
	}

	public int RopeSprite(int ropeSegment)
	{
		return ropeSegment;
	}

	public int BodySprite(int bodySegment)
	{
		return 3 + bodySegment;
	}

	public int LimbSprite(int side, int limb)
	{
		return 5 + side + limb + limb;
	}

	public int BackSpotSprite(int pos, int side)
	{
		return 10 + side + pos + pos;
	}

	public int EyeASprite(int eye)
	{
		return 15 + eye;
	}

	public int EyeBSprite(int eye)
	{
		return 17 + eye;
	}

	public int EyeCSprite(int eye)
	{
		return 19 + eye;
	}

	public MouseGraphics(PhysicalObject ow)
		: base(ow, internalContainers: false)
	{
		creatureLooker = new CreatureLooker(this, mouse.AI.tracker, mouse, 0.2f, 50);
		ropePositions = new Vector2[4, 2];
		head = new BodyPart(this);
		tail = new BodyPart(this);
		tail.rad = 1f;
		bodyParts = new BodyPart[6];
		limbs = new Limb[2, 2];
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				limbs[i, j] = new Limb(this, mouse.bodyChunks[i], i + j + j, 1f, 0.5f, 0.9f, 8f, 0.9f);
				bodyParts[i + j + j] = limbs[i, j];
			}
		}
		bodyParts[4] = head;
		bodyParts[5] = tail;
		Reset();
		ResetUnconsiousProfile();
		handsOutWhileHanging = UnityEngine.Random.value < 0.125f;
		hangBackwards = UnityEngine.Random.value < 1f / 17f;
		if (mouse.ropeAttatchedPos.HasValue)
		{
			SpawnAsHanging();
		}
	}

	public void SpawnAsHanging()
	{
		lightMode = 1f;
		charging = 0f;
		superCharge = 0f;
	}

	public override void Update()
	{
		if (soundLoop == null)
		{
			Reset();
		}
		soundLoop.Update();
		if (lightMode > charging && lightMode > 0.5f)
		{
			soundLoop.sound = SoundID.Mouse_Light_On_LOOP;
			soundLoop.Volume = 1f - 0.6f * flicker;
			soundLoop.Pitch = 1f - 0.3f * Mathf.Pow(flicker, 0.6f);
		}
		else if (charging > 0f)
		{
			soundLoop.sound = SoundID.Mouse_Charge_LOOP;
			soundLoop.Volume = charging / 2f;
			soundLoop.Pitch = Custom.LerpMap(charging, 0f, 2f, 0.5f, 1.5f);
		}
		else
		{
			soundLoop.sound = SoundID.None;
			soundLoop.Volume = 0f;
		}
		base.Update();
		lastProfileFac = profileFac;
		litRoom = Mathf.Pow(1f - Mathf.InverseLerp(0f, 0.5f, mouse.room.Darkness(mouse.mainBodyChunk.pos)), 3f);
		if (mouse.Consious)
		{
			profileFac = mouse.profileFac;
			if (charging > 0f)
			{
				charging += 1f / Mathf.Lerp(60f, 200f, UnityEngine.Random.value);
				if (charging >= 2f)
				{
					mouse.room.PlaySound(SoundID.Mouse_Light_Switch_On, mouse.mainBodyChunk);
					charging = 0f;
					lightMode = 1f;
					superCharge = 1f;
					flickeringFac = ((UnityEngine.Random.value < 0.5f) ? 0f : 1f);
					flickerDuration = Mathf.Lerp(30f, 220f, UnityEngine.Random.value);
					for (int i = 0; i < 20; i++)
					{
						Vector2 vector = Custom.DegToVec(360f * UnityEngine.Random.value);
						mouse.room.AddObject(new MouseSpark(mouse.mainBodyChunk.pos + vector * 9f, mouse.mainBodyChunk.vel + vector * 36f * UnityEngine.Random.value, 20f, BodyColor));
					}
				}
			}
			else if (mouse.ropeAttatchedPos.HasValue && lightMode < 0.5f)
			{
				charging = 0.001f;
			}
			if (!mouse.ropeAttatchedPos.HasValue)
			{
				lightMode = Mathf.Lerp(lightMode, 0f, 0.05f);
				if (UnityEngine.Random.value < Mathf.Pow(mouse.AI.fear * mouse.runSpeed, 3f))
				{
					mouse.room.AddObject(new MouseSpark(mouse.mainBodyChunk.pos, mouse.mainBodyChunk.vel + Custom.DegToVec(360f * UnityEngine.Random.value) * 6f * UnityEngine.Random.value, 40f, BodyColor));
				}
			}
			if (!mouse.Sleeping && UnityEngine.Random.value < 0.0016666667f)
			{
				handsOutWhileHanging = UnityEngine.Random.value < 0.125f;
				hangBackwards = UnityEngine.Random.value < 1f / 17f;
			}
		}
		else
		{
			if (lastConsious)
			{
				ResetUnconsiousProfile();
			}
			profileFac = unconsiousProfileFac;
			charging = Mathf.Lerp(charging, 0f, 1f / 30f);
			if (!mouse.dead && !mouse.carried)
			{
				flickeringFac = 1f;
				flickerDuration = Mathf.Lerp(10f, 30f, UnityEngine.Random.value);
				if (UnityEngine.Random.value < 0.1f)
				{
					flicker = Mathf.Max(flicker, UnityEngine.Random.value);
				}
				lightMode = Mathf.Lerp(lightMode, 0.3f, 1f / 120f);
			}
			else
			{
				lightMode = Mathf.Lerp(lightMode, Mathf.Lerp(0.3f, 0.7f, Battery), 1f / 120f);
			}
		}
		lastConsious = mouse.Consious;
		if (flickeringFac > 0f)
		{
			flickeringFac = Mathf.Max(flickeringFac - 1f / flickerDuration, 0f);
			if (UnityEngine.Random.value < 1f / 15f && UnityEngine.Random.value < flickeringFac)
			{
				flicker = Mathf.Pow(UnityEngine.Random.value, 1f - flickeringFac);
				mouse.room.PlaySound(SoundID.Mouse_Light_Flicker, mouse.mainBodyChunk.pos, flicker, 1f + (0.5f - flicker));
			}
		}
		else if (!mouse.dead && UnityEngine.Random.value < 0.0033333334f)
		{
			flickeringFac = UnityEngine.Random.value;
			flickerDuration = Mathf.Lerp(30f, 120f, UnityEngine.Random.value);
		}
		if (flicker > 0f)
		{
			flicker = Mathf.Max(flicker - 1f / 15f, 0f);
		}
		if (!mouse.dead)
		{
			breath += 1f / Mathf.Lerp(Mathf.Lerp(40f, 20f, mouse.AI.fear), 180f, mouse.fallAsleep);
		}
		if (lightSource != null)
		{
			lightSource.stayAlive = true;
			lightSource.setPos = Vector2.Lerp(mouse.mainBodyChunk.pos, mouse.bodyChunks[1].pos, 0.3f);
			lightSource.setRad = Mathf.Lerp(Mathf.Lerp(80f, 280f, lightMode * (1f - flicker)), 40f, Mathf.Clamp(charging, 0f, 1f)) * (1f + Mathf.Sin(breath * (float)Math.PI * 2f) * Mathf.Lerp(0.05f, 0.2f, mouse.fallAsleep)) * Battery;
			lightSource.setAlpha = Mathf.Lerp(Mathf.Lerp(0.8f, 0.3f, lightMode), 1f, Custom.SCurve(superCharge, 0.3f) * 0.5f) * LightStrength * (1f - flicker * 0.4f);
			lightSource.color = Custom.HSL2RGB(mouse.iVars.color.hue, 1f * Battery, 0.8f - 0.2f * flicker);
			if (lightSource.slatedForDeletetion || mouse.room.Darkness(mouse.mainBodyChunk.pos) == 0f)
			{
				lightSource = null;
			}
		}
		else if (mouse.room.Darkness(mouse.mainBodyChunk.pos) > 0f)
		{
			lightSource = new LightSource(head.pos, environmentalLight: false, new Color(1f, 1f, 1f), mouse);
			lightSource.requireUpKeep = true;
			mouse.room.AddObject(lightSource);
		}
		lastRunCycle = runCycle;
		runCycle = mouse.runCycle;
		lastBackToCam = backToCam;
		if ((mouse.ropeAttatchedPos.HasValue && !hangBackwards) || mouse.currentlyClimbingCorridor)
		{
			backToCam = Mathf.Min(backToCam + 0.05f, 1f);
		}
		else
		{
			backToCam = Mathf.Max(backToCam - 0.05f, 0f);
		}
		if (backToCam > 0f)
		{
			profileFac = Mathf.Sin(backToCam * (float)Math.PI);
		}
		if (ouchEyes > 0)
		{
			ouchEyes--;
		}
		creatureLooker.Update();
		lastLookDir = lookDir;
		if (mouse.Consious && creatureLooker.lookCreature != null)
		{
			if (creatureLooker.lookCreature.VisualContact)
			{
				lookDir = Vector2.ClampMagnitude((creatureLooker.lookCreature.representedCreature.realizedCreature.DangerPos - head.pos) / 70f, 1f);
			}
			else
			{
				lookDir = Vector2.ClampMagnitude((mouse.room.MiddleOfTile(creatureLooker.lookCreature.BestGuessForPosition()) - head.pos) / 70f, 1f);
			}
		}
		else
		{
			lookDir *= 0.9f;
		}
		lookDir *= 1f - mouse.fallAsleep;
		head.Update();
		head.lastPos = head.pos;
		head.pos += head.vel;
		Vector2 vector2 = mouse.mainBodyChunk.pos + Custom.DirVec(mouse.bodyChunks[1].pos, mouse.mainBodyChunk.pos) * (3f + 3f * Mathf.Abs(profileFac));
		head.ConnectToPoint(vector2, 4f, push: false, 0f, mouse.mainBodyChunk.vel, 0.5f, 0.1f);
		head.vel += (vector2 - head.pos) / 6f;
		head.vel += lookDir;
		if (!mouse.Consious)
		{
			head.vel.y -= 0.6f;
		}
		tail.Update();
		tail.lastPos = tail.pos;
		if (mouse.ropeAttatchedPos.HasValue)
		{
			tail.pos = mouse.bodyChunks[1].pos + Vector2.ClampMagnitude(mouse.ropeBends[mouse.ropeBends.Count - 1] - mouse.bodyChunks[1].pos, Mathf.Lerp(12f, 30f, mouse.ropeStretch));
			tail.vel *= 0f;
		}
		else
		{
			tail.pos += tail.vel;
			vector2 = mouse.bodyChunks[1].pos + Custom.DirVec(mouse.mainBodyChunk.pos, mouse.bodyChunks[1].pos) * 8f;
			tail.ConnectToPoint(vector2, 7f, push: false, 0f, mouse.bodyChunks[1].vel, 0.1f, 0f);
			if (!mouse.sitting)
			{
				tail.vel += (vector2 - tail.pos) / 46f;
			}
			else
			{
				tail.vel.x += Mathf.Sign(tail.pos.x - mouse.bodyChunks[1].pos.x) * 7f;
			}
			if (mouse.Consious && !mouse.sitting)
			{
				tail.pos += Custom.DegToVec(UnityEngine.Random.value * 360f) * 2f * UnityEngine.Random.value;
			}
			else
			{
				tail.vel.y -= 0.6f;
			}
			tail.PushOutOfTerrain(mouse.room, mouse.bodyChunks[1].pos);
		}
		blink--;
		if ((blink < 0 && UnityEngine.Random.value < 0.2f && UnityEngine.Random.value > mouse.fallAsleep) || blink < -15)
		{
			blink = UnityEngine.Random.Range(3, 4 + (int)(100f * (1f - mouse.fallAsleep)));
		}
		if (mouse.voiceCounter > 0)
		{
			blink = Mathf.Max(blink, 1);
		}
		for (int j = 0; j < 4; j++)
		{
			ropePositions[j, 1] = ropePositions[j, 0];
			if (mouse.ropeBends == null)
			{
				ropePositions[j, 0] = tail.pos;
			}
			else if (j < mouse.ropeBends.Count)
			{
				ropePositions[j, 0] = mouse.ropeBends[j];
			}
			else
			{
				ropePositions[j, 0] = tail.pos;
			}
		}
		Vector2 vector3 = Custom.DirVec(mouse.bodyChunks[1].pos, mouse.bodyChunks[0].pos);
		Vector2 vector4 = Custom.PerpendicularVector(vector3) * Mathf.Lerp(1f, -1f, backToCam);
		for (int k = 0; k < 2; k++)
		{
			for (int l = 0; l < 2; l++)
			{
				limbs[k, l].Update();
				limbs[k, l].ConnectToPoint(mouse.bodyChunks[k].pos, 12f, push: false, 0f, mouse.bodyChunks[k].vel, 0f, 0f);
				limbs[k, l].vel.y -= 0.6f;
				if (!limbs[k, l].retract)
				{
					limbs[k, l].mode = Limb.Mode.Dangle;
				}
				if (mouse.Consious && !mouse.ropeAttatchedPos.HasValue && !mouse.sitting)
				{
					float num = Mathf.Sin((runCycle + (float)limbs[k, l].limbNumber / 4f) * (float)Math.PI * 2f);
					Vector2 goalPos = mouse.bodyChunks[k].pos + vector3 * 8f * (0.3f + 0.7f * num) + vector4 * 4f * ((l == 0) ? (-1f) : 1f);
					limbs[k, l].FindGrip(mouse.room, mouse.bodyChunks[k].pos, mouse.bodyChunks[k].pos, 15f, goalPos, 2, 2, behindWalls: false);
					limbs[k, l].pos += vector3 * (2f + num * 8f) * Mathf.Lerp(0.5f, 2f, mouse.AI.fear);
					limbs[k, l].pos -= vector4 * ((l == 0) ? (-1f) : 1f) * 3f * Mathf.Cos((runCycle + (float)limbs[k, l].limbNumber / 4f) * (float)Math.PI * 2f) * Mathf.Lerp(0.5f, 2f, mouse.AI.fear);
				}
				if (!ShouldThisLimbRetract(k, l))
				{
					limbs[k, l].retract = false;
					if (mouse.sitting)
					{
						limbs[k, l].vel.y -= 2f;
						limbs[k, l].vel -= vector4 * ((l == 0) ? (-1f) : 1f) * 7f;
					}
				}
				if (limbs[k, l].mode == Limb.Mode.Dangle)
				{
					if (mouse.Consious && mouse.AI.fear > 0.1f)
					{
						limbs[k, l].vel += vector3 * UnityEngine.Random.value * 4f + Custom.DegToVec(UnityEngine.Random.value * 360f) * mouse.AI.fear * 8f;
						limbs[k, l].pos += Custom.DegToVec(UnityEngine.Random.value * 360f) * mouse.AI.fear * 8f;
					}
					else if (ShouldThisLimbRetract(k, l))
					{
						limbs[k, l].retract = true;
					}
					else if (mouse.ropeAttatchedPos.HasValue)
					{
						limbs[k, l].vel += vector4 * 0.6f * ((l == 0) ? (-1f) : 1f);
					}
					else
					{
						limbs[k, l].vel += vector4 * 0.6f * ((l == 0) ? (-1f) : 1f);
						limbs[k, l].vel += vector3 * 0.6f * ((k == 0) ? (-1f) : 1f);
					}
				}
			}
		}
	}

	private bool ShouldThisLimbRetract(int pos, int side)
	{
		if (mouse.ropeAttatchedPos.HasValue && (pos == 1 || !handsOutWhileHanging))
		{
			return true;
		}
		if (mouse.sitting && pos == 0)
		{
			return true;
		}
		return false;
	}

	public override void Reset()
	{
		base.Reset();
		BodyPart[] array = bodyParts;
		foreach (BodyPart obj in array)
		{
			obj.vel *= 0f;
			obj.pos = mouse.mainBodyChunk.pos;
			obj.lastPos = obj.pos;
		}
		soundLoop = new ChunkDynamicSoundLoop(mouse.mainBodyChunk);
	}

	public void ResetUnconsiousProfile()
	{
		if (UnityEngine.Random.value < 0.5f)
		{
			unconsiousProfileFac = 0f;
		}
		else
		{
			unconsiousProfileFac = Mathf.Lerp(-1f, 1f, UnityEngine.Random.value);
		}
	}

	public void TerrainImpact(float speed)
	{
		if (Battery > 0.7f)
		{
			for (int i = 0; i < UnityEngine.Random.Range(2, 6); i++)
			{
				mouse.room.AddObject(new MouseSpark(mouse.mainBodyChunk.pos, mouse.mainBodyChunk.vel + Custom.DegToVec(360f * UnityEngine.Random.value) * speed * UnityEngine.Random.value, 40f, BodyColor));
			}
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[TotalSprites];
		for (int i = 0; i < 3; i++)
		{
			sLeaser.sprites[RopeSprite(i)] = new CustomFSprite("pixel");
			sLeaser.sprites[RopeSprite(i)].anchorY = 0f;
		}
		for (int j = 3; j < TotalSprites; j++)
		{
			sLeaser.sprites[j] = new FSprite("pixel");
		}
		sLeaser.sprites[EyeASprite(1)].scaleX = -1f;
		sLeaser.sprites[EyeBSprite(1)].scaleX = -1f;
		for (int k = 0; k < 2; k++)
		{
			sLeaser.sprites[EyeBSprite(k)].color = new Color(0.2f, 0f, 0f);
			sLeaser.sprites[EyeASprite(k)].color = new Color(0.8f, 0.2f, 0.2f);
		}
		for (int l = 0; l < 2; l++)
		{
			for (int m = 0; m < 2; m++)
			{
				sLeaser.sprites[LimbSprite(l, m)].element = Futile.atlasManager.GetElementWithName("mouse" + ((l == 1) ? "Hind" : "Front") + "Leg");
				sLeaser.sprites[LimbSprite(l, m)].anchorY = 0.1f;
				sLeaser.sprites[BackSpotSprite(l, m)].element = Futile.atlasManager.GetElementWithName("mouseSpot");
				sLeaser.sprites[BackSpotSprite(l, m)].color = new Color(1f, 0f, 0f);
			}
		}
		sLeaser.sprites[BodySprite(0)].element = Futile.atlasManager.GetElementWithName("mouseBodyA");
		sLeaser.sprites[BodySprite(1)].element = Futile.atlasManager.GetElementWithName("mouseBodyB");
		sLeaser.sprites[BodySprite(1)].anchorY = 0f;
		sLeaser.sprites[HeadSprite].element = Futile.atlasManager.GetElementWithName("mouseHead0");
		AddToContainer(sLeaser, rCam, null);
		base.InitiateSprites(sLeaser, rCam);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		sLeaser.sprites[HeadSprite].color = BodyColor;
		for (int i = 0; i < 2; i++)
		{
			sLeaser.sprites[BodySprite(i)].color = BodyColor;
			sLeaser.sprites[EyeASprite(i)].color = DecalColor;
			sLeaser.sprites[EyeBSprite(i)].color = EyesColor;
			for (int j = 0; j < 2; j++)
			{
				sLeaser.sprites[BackSpotSprite(i, j)].color = DecalColor;
				sLeaser.sprites[LimbSprite(i, j)].color = BodyColor;
			}
		}
		Vector2 vector = Vector2.Lerp(mouse.bodyChunks[1].lastPos, mouse.bodyChunks[1].pos, timeStacker);
		Vector2 vector2 = Vector2.Lerp(mouse.bodyChunks[0].lastPos, mouse.bodyChunks[0].pos, timeStacker);
		float num = Mathf.Lerp(lastRunCycle, runCycle, timeStacker);
		if (charging > 0f)
		{
			vector += Custom.DegToVec(UnityEngine.Random.value * 360f) * 1.5f * Mathf.Clamp(charging, 0f, 1f);
			vector2 += Custom.DegToVec(UnityEngine.Random.value * 360f) * 1.5f * Mathf.Clamp(charging, 0f, 1f);
		}
		else if (mouse.shrinkingRope)
		{
			vector.x += Mathf.Sin(num * (float)Math.PI * 5f) * 3f;
		}
		Vector2 vector3 = Vector2.Lerp(tail.lastPos, tail.pos, timeStacker);
		Vector2 vector4 = Vector2.Lerp(head.lastPos, head.pos, timeStacker);
		if (mouse.Consious && mouse.voiceCounter > 0)
		{
			vector4 += Custom.RNV() * UnityEngine.Random.value * 2f;
		}
		if (mouse.sitting)
		{
			vector.y -= 4f;
		}
		float num2 = Mathf.Lerp(lastProfileFac, profileFac, timeStacker);
		float num3 = Mathf.Lerp(lastBackToCam, backToCam, timeStacker);
		Vector2 vector5 = Custom.DirVec(vector, vector2);
		Vector2 vector6 = Custom.PerpendicularVector(vector5);
		if (!mouse.ropeAttatchedPos.HasValue)
		{
			vector += vector6 * profileFac * Mathf.Sin(num * (float)Math.PI * 2f) * 4f * mouse.runSpeed;
			vector += vector5 * profileFac * Mathf.Sin(num * (float)Math.PI * 2f) * 3f;
			vector2 += vector5 * Mathf.Cos(num * (float)Math.PI * 2f) * 4f * mouse.runSpeed;
			vector2 += vector6 * Mathf.Sin(num * (float)Math.PI * 2f) * 4f * (1f - Mathf.Abs(profileFac));
		}
		float rotation = Custom.AimFromOneVectorToAnother(vector, vector2);
		for (int k = 0; k < 2; k++)
		{
			for (int l = 0; l < 2; l++)
			{
				Vector2 vector7 = Vector2.Lerp(limbs[k, l].lastPos, limbs[k, l].pos, timeStacker);
				Vector2 vector8 = ((k == 0) ? vector2 : vector);
				vector8 += vector6 * ((l == 0) ? (-1f) : 1f) * 3f * (1f - Mathf.Abs(num2));
				if (k == 1)
				{
					vector8 -= vector5 * 2f;
				}
				if (!Custom.DistLess(vector7, vector8, 19f))
				{
					vector7 = vector8 + Custom.DirVec(vector8, vector7) * 19f;
				}
				sLeaser.sprites[LimbSprite(k, l)].x = vector7.x - camPos.x;
				sLeaser.sprites[LimbSprite(k, l)].y = vector7.y - camPos.y;
				sLeaser.sprites[LimbSprite(k, l)].rotation = Custom.AimFromOneVectorToAnother(vector7, vector8);
				sLeaser.sprites[LimbSprite(k, l)].scaleY = Mathf.Lerp(Vector2.Distance(vector7, vector8) / 18f, 1f, 0.2f);
				if (mouse.ropeAttatchedPos.HasValue)
				{
					sLeaser.sprites[LimbSprite(k, l)].scaleX = ((l == 0 != hangBackwards) ? (-1f) : 1f);
				}
				else
				{
					sLeaser.sprites[LimbSprite(k, l)].scaleX = Mathf.Sign(Custom.DistanceToLine(vector7, vector2 - vector6 * ((l == 0) ? (-1f) : 1f), vector - vector6 * ((l == 0) ? (-1f) : 1f)));
				}
				sLeaser.sprites[LimbSprite(k, l)].isVisible = limbs[k, l].mode != Limb.Mode.Retracted;
				float num4 = Mathf.Abs(num2);
				float num5 = Mathf.Max(num4, Mathf.InverseLerp(0.5f, 0.7f, num3));
				num4 = Mathf.Lerp(Mathf.Lerp(1.2f, 0.8f, num4), num4, num3);
				if (num5 < 0.1f)
				{
					sLeaser.sprites[BackSpotSprite(k, l)].isVisible = false;
					continue;
				}
				Vector2 a = Vector2.Lerp(vector2, vector, 0.5f);
				a = ((k != 0) ? Vector2.Lerp(a, vector3, 0.3f) : Vector2.Lerp(a, vector4, 0.3f));
				a += vector6 * ((l == 0) ? (-1f) : 1f) * ((k == 1) ? 4f : 3f) * (1f - num4);
				if (num3 > 0.01f && num3 < 0.99f)
				{
					float num6 = Mathf.Sin(num3 * (float)Math.PI);
					num4 = Mathf.Lerp(num4, 1f, num6);
					num5 *= 1f - num6;
				}
				if (l == 0 == num2 < 0f)
				{
					a += vector6 * Mathf.Pow(num4, 0.5f) * Mathf.Sign(num2) * ((k == 1) ? 6f : 5f);
					num5 *= 1f - Mathf.Pow(num4, 1.5f);
				}
				else
				{
					a += vector6 * Mathf.Pow(num4, 1.2f) * Mathf.Sign(num2) * ((k == 1) ? 4f : 3f);
				}
				sLeaser.sprites[BackSpotSprite(k, l)].isVisible = true;
				sLeaser.sprites[BackSpotSprite(k, l)].scaleX = num5 * ((l == 1) ? (-1f) : 1f);
				sLeaser.sprites[BackSpotSprite(k, l)].x = a.x - camPos.x;
				sLeaser.sprites[BackSpotSprite(k, l)].y = a.y - camPos.y;
				sLeaser.sprites[BackSpotSprite(k, l)].rotation = rotation;
			}
		}
		sLeaser.sprites[BodySprite(1)].x = Mathf.Lerp(vector.x, vector2.x, 0.5f) - camPos.x;
		sLeaser.sprites[BodySprite(1)].y = Mathf.Lerp(vector.y, vector2.y, 0.5f) - camPos.y;
		sLeaser.sprites[BodySprite(1)].rotation = Custom.AimFromOneVectorToAnother(Vector2.Lerp(vector, vector2, 0.34f), vector3);
		sLeaser.sprites[BodySprite(1)].scaleY = (Vector2.Distance(Vector2.Lerp(vector, vector2, 0.34f), vector3) + 5f) / 24f;
		sLeaser.sprites[BodySprite(1)].scaleX = 1f / (1f + mouse.ropeStretch * 0.2f);
		sLeaser.sprites[BodySprite(0)].x = Mathf.Lerp(vector.x, vector2.x, 0.75f) - camPos.x;
		sLeaser.sprites[BodySprite(0)].y = Mathf.Lerp(vector.y, vector2.y, 0.75f) - camPos.y;
		sLeaser.sprites[BodySprite(0)].rotation = Custom.AimFromOneVectorToAnother(Vector2.Lerp(vector2, vector4, 0.3f), vector);
		float ropeLength = mouse.ropeLength;
		float num7 = 0f;
		Vector2 vector9 = Vector2.Lerp(ropePositions[0, 1], ropePositions[0, 0], timeStacker);
		for (int m = 0; m < 3; m++)
		{
			Vector2 vector10 = Vector2.Lerp(ropePositions[m + 1, 1], ropePositions[m + 1, 0], timeStacker);
			Vector2 vector11 = Custom.DirVec(vector9, vector10);
			Vector2 vector12 = Custom.PerpendicularVector(vector11);
			(sLeaser.sprites[RopeSprite(m)] as CustomFSprite).MoveVertice(0, vector10 + vector11 - vector12 * 0.5f - camPos);
			(sLeaser.sprites[RopeSprite(m)] as CustomFSprite).MoveVertice(1, vector10 + vector11 + vector12 * 0.5f - camPos);
			(sLeaser.sprites[RopeSprite(m)] as CustomFSprite).MoveVertice(2, vector9 - vector11 + vector12 * 0.5f - camPos);
			(sLeaser.sprites[RopeSprite(m)] as CustomFSprite).MoveVertice(3, vector9 - vector11 - vector12 * 0.5f - camPos);
			float num8 = Mathf.InverseLerp(0f, ropeLength, num7);
			num7 += Vector2.Distance(vector10, vector9);
			float num9 = Mathf.InverseLerp(0f, ropeLength, num7);
			for (int n = 0; n < 4; n++)
			{
				(sLeaser.sprites[RopeSprite(m)] as CustomFSprite).verticeColors[n] = Color.Lerp(blackColor, BodyColor, (n < 2) ? num9 : num8);
			}
			vector9 = vector10;
		}
		float num10 = Custom.AimFromOneVectorToAnother(Vector2.Lerp(vector, vector2, 0.5f), vector4) - 180f * num3;
		num10 -= 90f * num2;
		sLeaser.sprites[HeadSprite].x = vector4.x - camPos.x;
		sLeaser.sprites[HeadSprite].y = vector4.y - camPos.y;
		Vector2 vector13 = Vector2.Lerp(lastLookDir, lookDir, timeStacker);
		Vector2 vector14 = Custom.RotateAroundOrigo(vector13, 0f - num10);
		sLeaser.sprites[BodySprite(0)].scaleY = Mathf.Lerp(Mathf.Lerp(1.2f, 0.8f, num3), Mathf.Lerp(0.8f, 1.2f, num3), Mathf.InverseLerp(1f, -1f, vector14.y));
		int num11 = Mathf.FloorToInt(Mathf.Abs(num2) * 4.9f);
		if (num11 > 0)
		{
			sLeaser.sprites[HeadSprite].element = Futile.atlasManager.GetElementWithName("mouseHead" + num11);
		}
		else if (vector14.y > 0.75f)
		{
			sLeaser.sprites[HeadSprite].element = Futile.atlasManager.GetElementWithName("mouseHeadUp");
		}
		else if (vector14.y < -0.75f)
		{
			sLeaser.sprites[HeadSprite].element = Futile.atlasManager.GetElementWithName("mouseHeadDown");
		}
		else
		{
			sLeaser.sprites[HeadSprite].element = Futile.atlasManager.GetElementWithName("mouseHead0");
		}
		sLeaser.sprites[HeadSprite].scaleX = ((num2 > 0f) ? 1f : (-1f));
		sLeaser.sprites[HeadSprite].rotation = num10;
		for (int num12 = 0; num12 < 2; num12++)
		{
			bool flag = true;
			bool flag2 = true;
			float num13 = num10;
			Vector2 vec = new Vector2(4f, -2f);
			if (num12 == 0 == vector14.x < 0f)
			{
				sLeaser.sprites[EyeASprite(num12)].scaleX = Mathf.Lerp(1f, 0f, Mathf.InverseLerp(0.1f, 0.4f, Mathf.Abs(num2))) * ((num12 == 1) ? (-1f) : 1f);
				sLeaser.sprites[EyeBSprite(num12)].scaleX = Mathf.Lerp(1f, 0f, Mathf.InverseLerp(0.1f, 0.4f, Mathf.Abs(num2))) * ((num12 == 1) ? (-1f) : 1f);
				vec.x += 5f * Mathf.Abs(num2);
				vec.y += 2.5f * Mathf.Abs(num2);
				num13 += Mathf.Sign(vector14.x) * Mathf.InverseLerp(0.1f, 0.4f, Mathf.Abs(num2)) * -25f;
				flag = Mathf.Abs(num2) < 0.1f;
				flag2 = Mathf.Abs(num2) < 0.2f;
			}
			else
			{
				if (Mathf.Abs(num2) > 0.1f)
				{
					sLeaser.sprites[EyeASprite(num12)].scaleX = Mathf.Lerp(1f, 0.85714f, Mathf.Abs(num2)) * ((num12 == 1) ? (-1f) : 1f);
					sLeaser.sprites[EyeBSprite(num12)].scaleX = Mathf.Lerp(1f, 0.85714f, Mathf.Abs(num2)) * ((num12 == 1) ? (-1f) : 1f);
					flag = false;
				}
				vec.x -= 4.5f * Mathf.Pow(Mathf.Abs(num2), 0.6f);
			}
			vec.x *= ((num12 == 0) ? (-1f) : 1f);
			if (Mathf.Abs(vector14.y) > 0.75f)
			{
				sLeaser.sprites[EyeASprite(num12)].scaleY = 0.85714f;
				sLeaser.sprites[EyeBSprite(num12)].scaleY = 0.85714f;
				flag = false;
				vec.y += vector14.y * ((vector14.y > 0f) ? 2f : 1f);
			}
			else
			{
				sLeaser.sprites[EyeASprite(num12)].scaleY = 1f;
				sLeaser.sprites[EyeBSprite(num12)].scaleY = 1f;
			}
			Vector2 vector15 = vector4 + vector13;
			vector15 += Custom.RotateAroundOrigo(vec, num10);
			int num14 = 1;
			if (!mouse.Consious)
			{
				num14 = ((!mouse.dead) ? 2 : 5);
			}
			else if (ouchEyes > 0)
			{
				num14 = 2;
			}
			else if (mouse.Sleeping)
			{
				num14 = 4;
			}
			else if (mouse.wakeUp < 1f)
			{
				num14 = ((mouse.wakeUp < 0.75f) ? 4 : 3);
				if (lookDir.magnitude > 0.3f)
				{
					if (vector15.x < vector4.x == lookDir.x < 0f)
					{
						num14 = 1;
					}
				}
				else
				{
					num14 = 3;
				}
			}
			else
			{
				num14 = ((blink > 0) ? 1 : 3);
			}
			sLeaser.sprites[EyeASprite(num12)].x = vector15.x - camPos.x;
			sLeaser.sprites[EyeASprite(num12)].y = vector15.y - camPos.y;
			sLeaser.sprites[EyeBSprite(num12)].x = vector15.x - camPos.x;
			sLeaser.sprites[EyeBSprite(num12)].y = vector15.y - camPos.y;
			if (flag2 && num14 == 1)
			{
				sLeaser.sprites[EyeCSprite(num12)].isVisible = true;
				sLeaser.sprites[EyeCSprite(num12)].x = vector15.x - camPos.x - 1f;
				sLeaser.sprites[EyeCSprite(num12)].y = vector15.y - camPos.y + 1f;
			}
			else
			{
				sLeaser.sprites[EyeCSprite(num12)].isVisible = false;
			}
			if (flag && num14 == 1)
			{
				sLeaser.sprites[EyeASprite(num12)].rotation = 0f;
				sLeaser.sprites[EyeBSprite(num12)].rotation = 0f;
			}
			else
			{
				sLeaser.sprites[EyeASprite(num12)].rotation = num13;
				sLeaser.sprites[EyeBSprite(num12)].rotation = num13;
			}
			sLeaser.sprites[EyeASprite(num12)].element = Futile.atlasManager.GetElementWithName("mouseEyeA" + num14);
			sLeaser.sprites[EyeBSprite(num12)].element = Futile.atlasManager.GetElementWithName("mouseEyeB" + num14);
		}
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		blackColor = palette.blackColor;
		base.ApplyPalette(sLeaser, rCam, palette);
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		sLeaser.RemoveAllSpritesFromContainer();
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Midground");
		}
		for (int i = 0; i < TotalSprites; i++)
		{
			newContatiner.AddChild(sLeaser.sprites[i]);
		}
	}

	public float CreatureInterestBonus(Tracker.CreatureRepresentation crit, float score)
	{
		if (crit.representedCreature.creatureTemplate.type == CreatureTemplate.Type.LanternMouse)
		{
			if (crit.representedCreature.realizedCreature != null && (crit.representedCreature.realizedCreature as LanternMouse).ropeAttatchedPos.HasValue)
			{
				return 0f;
			}
			score *= 0.04f;
		}
		return score;
	}

	public Tracker.CreatureRepresentation ForcedLookCreature()
	{
		return null;
	}

	public void LookAtNothing()
	{
	}
}
