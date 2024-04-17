using System;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class ZapCoil : UpdatableAndDeletable, IDrawable
{
	public class ZapFlash : CosmeticSprite
	{
		private LightSource lightsource;

		private float life;

		private float lastLife;

		private float lifeTime;

		private float size;

		public ZapFlash(Vector2 initPos, float size)
		{
			this.size = size;
			lifeTime = Mathf.Lerp(1f, 4f, UnityEngine.Random.value) + 2f * size;
			life = 1f;
			lastLife = 1f;
			pos = initPos;
			lastPos = initPos;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (lightsource == null)
			{
				lightsource = new LightSource(pos, environmentalLight: false, new Color(0f, 0f, 1f), this);
				room.AddObject(lightsource);
			}
			lastLife = life;
			life -= 1f / lifeTime;
			if (lastLife < 0f)
			{
				if (lightsource != null)
				{
					lightsource.Destroy();
				}
				Destroy();
			}
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[2];
			sLeaser.sprites[0] = new FSprite("Futile_White");
			sLeaser.sprites[0].color = new Color(0f, 0f, 1f);
			sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["FlareBomb"];
			sLeaser.sprites[1] = new FSprite("Futile_White");
			sLeaser.sprites[1].color = new Color(1f, 1f, 1f);
			sLeaser.sprites[1].shader = rCam.room.game.rainWorld.Shaders["FlatLight"];
			AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Foreground"));
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
			float num = Mathf.Lerp(lastLife, life, timeStacker);
			for (int i = 0; i < 2; i++)
			{
				sLeaser.sprites[i].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x;
				sLeaser.sprites[i].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y;
			}
			if (lightsource != null)
			{
				lightsource.HardSetRad(Mathf.Lerp(0.25f, 1f, UnityEngine.Random.value * num * size) * 2400f);
				lightsource.HardSetAlpha(Mathf.Pow(num * UnityEngine.Random.value, 0.4f));
				float num2 = Mathf.Pow(num * UnityEngine.Random.value, 4f);
				lightsource.color = new Color(num2, num2, 1f);
			}
			sLeaser.sprites[0].scale = Mathf.Lerp(0.5f, 1f, UnityEngine.Random.value * num * size) * 500f / 16f;
			sLeaser.sprites[0].alpha = num * UnityEngine.Random.value;
			sLeaser.sprites[1].scale = Mathf.Lerp(0.5f, 1f, (0.5f + 0.5f * UnityEngine.Random.value) * num * size) * 400f / 16f;
			sLeaser.sprites[1].alpha = num * UnityEngine.Random.value;
		}
	}

	public IntRect rect;

	private float zapLit;

	public bool horizontalAlignment;

	public float[,] flicker;

	public float disruption;

	public float smoothDisruption;

	public float lastTurnedOn;

	public float turnedOn;

	public int turnedOffCounter;

	public bool powered;

	public RectangularDynamicSoundLoop soundLoop;

	public RectangularDynamicSoundLoop disruptedLoop;

	public FloatRect GetFloatRect => new FloatRect((float)rect.left * 20f - 2f, (float)rect.bottom * 20f - 2f, (float)rect.right * 20f + 22f, (float)rect.top * 20f + 22f);

	public ZapCoil(IntRect rect, Room room)
	{
		this.rect = rect;
		powered = true;
		if (ModManager.MSC && room.world.region != null && room.world.region.name == "MS" && !room.game.IsMoonHeartActive())
		{
			powered = false;
		}
		soundLoop = new RectangularDynamicSoundLoop(this, GetFloatRect, room);
		soundLoop.sound = SoundID.Zapper_LOOP;
		disruptedLoop = new RectangularDynamicSoundLoop(this, GetFloatRect, room);
		disruptedLoop.Volume = 0f;
		disruptedLoop.sound = SoundID.Zapper_Disrupted_LOOP;
		horizontalAlignment = rect.Width > rect.Height;
		flicker = new float[2, 4];
		disruption = -1f;
		turnedOn = 1f;
		lastTurnedOn = 1f;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		soundLoop.Update();
		disruptedLoop.Update();
		if (turnedOn > 0.5f)
		{
			for (int i = 0; i < room.physicalObjects.Length; i++)
			{
				for (int j = 0; j < room.physicalObjects[i].Count; j++)
				{
					for (int k = 0; k < room.physicalObjects[i][j].bodyChunks.Length; k++)
					{
						if ((!horizontalAlignment || room.physicalObjects[i][j].bodyChunks[k].ContactPoint.y == 0) && (horizontalAlignment || room.physicalObjects[i][j].bodyChunks[k].ContactPoint.x == 0))
						{
							continue;
						}
						Vector2 vector = room.physicalObjects[i][j].bodyChunks[k].ContactPoint.ToVector2();
						Vector2 v = room.physicalObjects[i][j].bodyChunks[k].pos + vector * (room.physicalObjects[i][j].bodyChunks[k].rad + 30f);
						if (GetFloatRect.Vector2Inside(v))
						{
							TriggerZap(room.physicalObjects[i][j].bodyChunks[k].pos + vector * room.physicalObjects[i][j].bodyChunks[k].rad, room.physicalObjects[i][j].bodyChunks[k].rad);
							room.physicalObjects[i][j].bodyChunks[k].vel -= (vector * 6f + Custom.RNV() * UnityEngine.Random.value) / room.physicalObjects[i][j].bodyChunks[k].mass;
							if (room.physicalObjects[i][j] is Creature)
							{
								(room.physicalObjects[i][j] as Creature).Die();
							}
							if (ModManager.MSC && room.physicalObjects[i][j] is ElectricSpear)
							{
								(room.physicalObjects[i][j] as ElectricSpear).Recharge();
							}
						}
					}
				}
			}
		}
		lastTurnedOn = turnedOn;
		if (UnityEngine.Random.value < 0.005f)
		{
			disruption = Mathf.Max(disruption, UnityEngine.Random.value);
		}
		disruption = Mathf.Max(0f, disruption - 1f / Mathf.Lerp(70f, 300f, UnityEngine.Random.value));
		smoothDisruption = Mathf.Lerp(smoothDisruption, disruption, 0.2f);
		float num = Mathf.InverseLerp(0.1f, 1f, smoothDisruption);
		soundLoop.Volume = (1f - num) * turnedOn;
		disruptedLoop.Volume = num * Mathf.Pow(turnedOn, 0.2f);
		for (int l = 0; l < flicker.GetLength(0); l++)
		{
			flicker[l, 1] = flicker[l, 0];
			flicker[l, 3] = Mathf.Clamp(flicker[l, 3] + Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) / 10f, 0f, 1f);
			flicker[l, 2] += 1f / Mathf.Lerp(70f, 20f, flicker[l, 3]);
			flicker[l, 0] = Mathf.Clamp(0.5f + smoothDisruption * (Mathf.Lerp(0.2f, 0.1f, flicker[l, 3]) * Mathf.Sin((float)Math.PI * 2f * flicker[l, 2]) + Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) / 20f), 0f, 1f);
		}
		if (UnityEngine.Random.value < disruption && UnityEngine.Random.value < 0.0025f)
		{
			turnedOffCounter = UnityEngine.Random.Range(10, 100);
		}
		if (!powered)
		{
			turnedOn = Mathf.Max(0f, turnedOn - 0.1f);
		}
		if (turnedOffCounter > 0)
		{
			turnedOffCounter--;
			if (UnityEngine.Random.value < 0.5f || UnityEngine.Random.value > disruption || !powered)
			{
				turnedOn = 0f;
			}
			else
			{
				turnedOn = UnityEngine.Random.value;
			}
			if (powered)
			{
				turnedOn = Mathf.Lerp(turnedOn, 1f, zapLit * UnityEngine.Random.value);
			}
			smoothDisruption = 1f;
		}
		else if (powered)
		{
			turnedOn = Mathf.Min(turnedOn + UnityEngine.Random.value / 30f, 1f);
		}
		zapLit = Mathf.Max(0f, zapLit - 0.1f);
		if (room.fullyLoaded)
		{
			disruption = Mathf.Max(disruption, room.gravity);
		}
		if (!(room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.BrokenZeroG) > 0f))
		{
			return;
		}
		int num2;
		if (room.world.rainCycle.brokenAntiGrav.to == 1f)
		{
			num2 = ((room.world.rainCycle.brokenAntiGrav.progress == 1f) ? 1 : 0);
			if (num2 != 0)
			{
				goto IL_06a9;
			}
		}
		else
		{
			num2 = 0;
		}
		disruption = 1f;
		if (powered && UnityEngine.Random.value < 0.2f)
		{
			powered = false;
		}
		goto IL_06a9;
		IL_06a9:
		if (num2 != 0 && !powered && UnityEngine.Random.value < 0.025f)
		{
			powered = true;
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		TriangleMesh.Triangle[] array = new TriangleMesh.Triangle[6];
		for (int i = 0; i < 6; i++)
		{
			array[i] = new TriangleMesh.Triangle(i, i + 1, i + 2);
		}
		TriangleMesh triangleMesh = new TriangleMesh("Futile_White", array, customColor: false);
		float num = 0.4f;
		triangleMesh.UVvertices[0] = new Vector2(0f, 0f);
		triangleMesh.UVvertices[1] = new Vector2(1f, 0f);
		triangleMesh.UVvertices[2] = new Vector2(0f, num);
		triangleMesh.UVvertices[3] = new Vector2(1f, num);
		triangleMesh.UVvertices[4] = new Vector2(0f, 1f - num);
		triangleMesh.UVvertices[5] = new Vector2(1f, 1f - num);
		triangleMesh.UVvertices[6] = new Vector2(0f, 1f);
		triangleMesh.UVvertices[7] = new Vector2(1f, 1f);
		sLeaser.sprites = new FSprite[1];
		sLeaser.sprites[0] = triangleMesh;
		sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["FlareBomb"];
		sLeaser.sprites[0].color = new Color(0f, 0f, 1f);
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		float num = Mathf.Lerp(lastTurnedOn, turnedOn, timeStacker);
		sLeaser.sprites[0].alpha = num;
		Vector2 vector = new Vector2((float)rect.left * 20f, (float)rect.bottom * 20f);
		Vector2 vector2 = new Vector2((float)(rect.right + 1) * 20f, (float)(rect.top + 1) * 20f);
		Vector2 vector3 = new Vector2((float)rect.left * 20f, (float)(rect.top + 1) * 20f);
		Vector2 vector4 = new Vector2((float)(rect.right + 1) * 20f, (float)rect.bottom * 20f);
		float num2 = 120f * num;
		float num3 = 30f;
		float num4 = Mathf.Lerp(flicker[0, 1], flicker[0, 0], timeStacker);
		float num5 = Mathf.Lerp(flicker[1, 1], flicker[1, 0], timeStacker);
		if (horizontalAlignment)
		{
			vector.x -= num3;
			vector3.x -= num3;
			vector2.x += num3;
			vector4.x += num3;
			vector.y -= num2 * num4;
			vector4.y -= num2 * num5;
			vector3.y += num2 * num4;
			vector2.y += num2 * num5;
			(sLeaser.sprites[0] as TriangleMesh).MoveVertice(0, vector - camPos);
			(sLeaser.sprites[0] as TriangleMesh).MoveVertice(1, vector3 - camPos);
			(sLeaser.sprites[0] as TriangleMesh).MoveVertice(2, vector + new Vector2(num3, 0f) - camPos);
			(sLeaser.sprites[0] as TriangleMesh).MoveVertice(3, vector3 + new Vector2(num3, 0f) - camPos);
			(sLeaser.sprites[0] as TriangleMesh).MoveVertice(4, vector4 + new Vector2(0f - num3, 0f) - camPos);
			(sLeaser.sprites[0] as TriangleMesh).MoveVertice(5, vector2 + new Vector2(0f - num3, 0f) - camPos);
			(sLeaser.sprites[0] as TriangleMesh).MoveVertice(6, vector4 - camPos);
			(sLeaser.sprites[0] as TriangleMesh).MoveVertice(7, vector2 - camPos);
		}
		else
		{
			vector.x -= num2 * num4;
			vector3.x -= num2 * num5;
			vector2.x += num2 * num5;
			vector4.x += num2 * num4;
			vector.y -= num3;
			vector4.y -= num3;
			vector3.y += num3;
			vector2.y += num3;
			(sLeaser.sprites[0] as TriangleMesh).MoveVertice(0, vector3 - camPos);
			(sLeaser.sprites[0] as TriangleMesh).MoveVertice(1, vector2 - camPos);
			(sLeaser.sprites[0] as TriangleMesh).MoveVertice(2, vector3 + new Vector2(0f, 0f - num3) - camPos);
			(sLeaser.sprites[0] as TriangleMesh).MoveVertice(3, vector2 + new Vector2(0f, 0f - num3) - camPos);
			(sLeaser.sprites[0] as TriangleMesh).MoveVertice(4, vector + new Vector2(0f, num3) - camPos);
			(sLeaser.sprites[0] as TriangleMesh).MoveVertice(5, vector4 + new Vector2(0f, num3) - camPos);
			(sLeaser.sprites[0] as TriangleMesh).MoveVertice(6, vector - camPos);
			(sLeaser.sprites[0] as TriangleMesh).MoveVertice(7, vector4 - camPos);
		}
		sLeaser.sprites[0].color = new Color(Mathf.InverseLerp(0f, 0.5f, zapLit) * num, Mathf.InverseLerp(0f, 0.5f, zapLit) * num, 1f);
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = ((!(rCam.room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.WaterLights) > 0f)) ? rCam.ReturnFContainer("Foreground") : rCam.ReturnFContainer("Water"));
		}
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i].RemoveFromContainer();
			newContatiner.AddChild(sLeaser.sprites[i]);
		}
	}

	public void TriggerZap(Vector2 zapContact, float massRad)
	{
		room.AddObject(new ZapFlash(zapContact, Mathf.InverseLerp(-0.05f, 15f, massRad)));
		disruption = Mathf.Max(disruption, Mathf.InverseLerp(-0.05f, 9f, massRad) + UnityEngine.Random.value * 0.5f);
		smoothDisruption = disruption;
		if (UnityEngine.Random.value < disruption && UnityEngine.Random.value < 0.5f)
		{
			turnedOffCounter = UnityEngine.Random.Range(2, 15);
		}
		room.PlaySound(SoundID.Zapper_Zap, zapContact, 1f, 1f);
		zapLit = 1f;
	}
}
