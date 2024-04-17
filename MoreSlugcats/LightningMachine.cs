using System;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class LightningMachine : UpdatableAndDeletable
{
	public class Impact : CosmeticSprite
	{
		public float size;

		public float life;

		public float lastLife;

		public float lifeTime;

		public Color color;

		public bool circle;

		public Impact(Vector2 pos, float size, Color color)
		{
			base.pos = pos;
			lastPos = pos;
			this.size = size;
			this.color = color;
			life = 1f;
			lastLife = 1f;
			lifeTime = Mathf.Lerp(2f, 16f, size * UnityEngine.Random.value);
		}

		public override void Update(bool eu)
		{
			room.AddObject(new Spark(pos, Custom.RNV() * 60f * UnityEngine.Random.value, color, null, 4, 50));
			if (life <= 0f && lastLife <= 0f)
			{
				Destroy();
				return;
			}
			lastLife = life;
			life = Mathf.Max(0f, life - 1f / lifeTime);
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[4];
			sLeaser.sprites[0] = new FSprite("Futile_White");
			sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["LightSource"];
			sLeaser.sprites[1] = new FSprite("Futile_White");
			sLeaser.sprites[1].shader = rCam.room.game.rainWorld.Shaders["FlatLight"];
			sLeaser.sprites[2] = new FSprite("Futile_White");
			sLeaser.sprites[2].shader = rCam.room.game.rainWorld.Shaders["FlareBomb"];
			sLeaser.sprites[3] = new FSprite("Futile_White");
			sLeaser.sprites[3].shader = rCam.room.game.rainWorld.Shaders[circle ? "FlareBomb" : "FlatLight"];
			sLeaser.sprites[0].color = color;
			sLeaser.sprites[1].color = color;
			sLeaser.sprites[2].color = color;
			sLeaser.sprites[3].color = color;
			AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Water"));
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
			float num = Mathf.Lerp(lastLife, life, timeStacker);
			for (int i = 0; i < 4; i++)
			{
				sLeaser.sprites[i].x = pos.x - camPos.x;
				sLeaser.sprites[i].y = pos.y - camPos.y;
			}
			float num2 = Mathf.Lerp(20f, 120f, Mathf.Pow(size, 1.5f));
			sLeaser.sprites[0].scale = Mathf.Pow(Mathf.Sin(num * (float)Math.PI), 0.5f) * Mathf.Lerp(0.8f, 1.2f, UnityEngine.Random.value) * num2 * 4f / 8f;
			sLeaser.sprites[0].alpha = Mathf.Pow(Mathf.Sin(num * (float)Math.PI), 0.5f) * Mathf.Lerp(0.6f, 1f, UnityEngine.Random.value);
			sLeaser.sprites[1].scale = Mathf.Pow(Mathf.Sin(num * (float)Math.PI), 0.5f) * Mathf.Lerp(0.8f, 1.2f, UnityEngine.Random.value) * num2 * 4f / 8f;
			sLeaser.sprites[1].alpha = Mathf.Pow(Mathf.Sin(num * (float)Math.PI), 0.5f) * Mathf.Lerp(0.6f, 1f, UnityEngine.Random.value) * 0.2f;
			sLeaser.sprites[2].scale = Mathf.Lerp(0.5f, 1f, Mathf.Sin(num * (float)Math.PI)) * Mathf.Lerp(0.8f, 1.2f, UnityEngine.Random.value) * num2 / 8f;
			sLeaser.sprites[2].alpha = Mathf.Sin(num * (float)Math.PI) * UnityEngine.Random.value;
			sLeaser.sprites[3].scale = Mathf.Pow(Mathf.Sin(num * (float)Math.PI), 0.5f) * Mathf.Lerp(0.8f, 1.2f, UnityEngine.Random.value) * num2 * 0.05f * 4f / 5f;
			sLeaser.sprites[3].alpha = Mathf.Pow(Mathf.Sin(num * (float)Math.PI), 0.5f) * Mathf.Lerp(0.9f, 1f, UnityEngine.Random.value);
		}

		public Impact(Vector2 pos, float size, Color color, bool circle)
			: this(pos, size, color)
		{
			this.circle = circle;
		}
	}

	public Vector2 pos;

	public Vector2 startPoint;

	public Vector2 endPoint;

	public float chance;

	public bool permanent;

	public bool radial;

	public float width;

	public float intensity;

	public float lifeTime;

	public Vector2 rPoint;

	public float lightningParam;

	public float lightningType;

	public LightningBolt permaLightning;

	public int impactType;

	public int soundType;

	public float volume;

	private bool isTerrain;

	public DynamicSoundLoop soundLoop;

	public Color color;

	public bool random;

	public bool ready;

	private float counter;

	public bool light;

	public Vector2 rSecPoint
	{
		get
		{
			Vector2 v = pos + startPoint - pos;
			Vector2 vec = pos + endPoint - pos;
			float num = Custom.VecToDeg(v);
			float maxInclusive = Custom.Mod(Custom.VecToDeg(Custom.rotateVectorDeg(vec, 0f - num)), 360f);
			float minInclusive = Custom.Dist(pos, pos + startPoint);
			float maxInclusive2 = Custom.Dist(pos, pos + endPoint);
			float y = UnityEngine.Random.Range(minInclusive, maxInclusive2);
			Vector2 vec2 = new Vector2(0f, y);
			vec2 = Custom.rotateVectorDeg(vec2, num);
			vec2 = Custom.rotateVectorDeg(vec2, UnityEngine.Random.Range(0f, maxInclusive));
			return pos + vec2;
		}
	}

	public Vector2 Target
	{
		get
		{
			if (radial)
			{
				return Trace(pos, rPoint);
			}
			return Trace(Source, pos + endPoint);
		}
	}

	public Vector2 Source
	{
		get
		{
			if (radial)
			{
				return pos;
			}
			return pos + startPoint;
		}
	}

	public LightningMachine(Vector2 pos, Vector2 startPoint, Vector2 endPoint, float chance, bool permanent, bool radial, float width, float intensity, float lifeTime)
	{
		this.pos = pos;
		this.startPoint = startPoint;
		this.endPoint = endPoint;
		this.chance = chance;
		this.permanent = permanent;
		this.radial = !permanent && radial;
		this.width = width;
		this.intensity = intensity;
		this.lifeTime = lifeTime;
		random = false;
		soundLoop = new DisembodiedDynamicSoundLoop(this);
		soundLoop.sound = SoundID.Zapper_LOOP;
		soundLoop.Pitch = 2f;
		soundLoop.Volume = 0f;
	}

	public void PermaLightning()
	{
		if (permaLightning == null)
		{
			Strike();
			return;
		}
		soundLoop.Volume = volume * 0.5f;
		permaLightning.from = Source;
		permaLightning.target = Target;
		permaLightning.intensity = intensity;
		permaLightning.width = width * 30f;
		permaLightning.lightningParam = lightningParam;
		permaLightning.lightningType = lightningType;
	}

	public void RadialLightning()
	{
		rPoint = rSecPoint;
		Strike();
	}

	public void Strike()
	{
		float num = Custom.ExponentMap(chance, 0f, 1f, 2f);
		counter += num;
		color = Custom.HSL2RGB(lightningType - 0.0001f, 1f, 0.6f);
		if (permanent)
		{
			room.AddObject(permaLightning = new LightningBolt(Source, Target, 1, width, lifeTime, lightningParam, lightningType, light));
			return;
		}
		if (UnityEngine.Random.value <= num && random)
		{
			ready = true;
		}
		if (counter > 1f && !random)
		{
			ready = true;
			counter = 0f;
		}
		if (!ready)
		{
			return;
		}
		soundLoop.Volume = 0f;
		if (soundType == 0)
		{
			room.PlaySound(SoundID.Death_Lightning_Spark_Spontaneous, Target, volume * 0.5f, 1.4f - UnityEngine.Random.value * 0.4f);
		}
		if (soundType == 1)
		{
			room.PlaySound(SoundID.Zapper_Zap, Target, volume * 0.5f, 1.4f - UnityEngine.Random.value * 0.4f);
		}
		LightningBolt lightningBolt;
		room.AddObject(lightningBolt = new LightningBolt(Source, Target, 0, width, lifeTime, lightningParam, lightningType, light));
		lightningBolt.intensity = intensity;
		lightningBolt.lightningParam = lightningParam;
		lightningBolt.lightningType = lightningType;
		if (impactType == 1 && isTerrain)
		{
			room.AddObject(new Impact(Target, intensity * width * 0.5f, color, isTerrain));
		}
		if (impactType > 1)
		{
			room.AddObject(new Impact(Target, intensity * width * 1f, color, isTerrain));
			if (impactType == 3)
			{
				room.AddObject(new Impact(Source, intensity * width * 1f, color, isTerrain));
			}
		}
		ready = false;
	}

	public Vector2 Trace(Vector2 start, Vector2 end)
	{
		Vector2 vector = Custom.DegToVec(Custom.AimFromOneVectorToAnother(start, end));
		IntVector2? intVector = SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(room, start, end);
		if (intVector.HasValue)
		{
			isTerrain = true;
			return room.MiddleOfTile(intVector.Value) - vector * 7f;
		}
		isTerrain = false;
		return end;
	}

	public override void Update(bool eu)
	{
		soundLoop.Update();
		if (permanent)
		{
			radial = false;
			PermaLightning();
		}
		else if (permaLightning != null)
		{
			permaLightning.Destroy();
			permaLightning = null;
		}
		else if (radial)
		{
			RadialLightning();
		}
		else
		{
			Strike();
		}
		base.Update(eu);
	}
}
