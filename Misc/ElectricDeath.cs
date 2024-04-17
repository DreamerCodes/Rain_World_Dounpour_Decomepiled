using System;
using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class ElectricDeath : UpdatableAndDeletable, IDrawable, INotifyWhenRoomIsReady
{
	public class LightFlash
	{
		public ElectricDeath owner;

		public int sprite;

		public Vector2 pos;

		public float lifeTime;

		public float rad;

		public float life;

		public float lastLife;

		public LightFlash(ElectricDeath owner, int sprite)
		{
			this.owner = owner;
			this.sprite = sprite;
		}

		public void Reset()
		{
			pos.x = owner.room.game.rainWorld.screenSize.x * UnityEngine.Random.value;
			pos.y = owner.room.game.rainWorld.screenSize.y * UnityEngine.Random.value;
			lifeTime = Mathf.Lerp(3f, Mathf.Lerp(34f, 12f, owner.Intensity), UnityEngine.Random.value);
			rad = Mathf.Lerp(40f, 800f, UnityEngine.Random.value * Mathf.Lerp(0.5f, 1f, owner.Intensity));
			life = 1f;
			lastLife = 1f;
		}

		public void Update()
		{
			if (life <= 0f && lastLife <= 0f)
			{
				Reset();
				return;
			}
			lastLife = life;
			life = Mathf.Max(0f, life - 1f / lifeTime);
		}

		public void Draw(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			sLeaser.sprites[sprite].x = pos.x;
			sLeaser.sprites[sprite].y = pos.y;
			float num = Mathf.Lerp(lastLife, life, timeStacker);
			sLeaser.sprites[sprite].scale = Mathf.Lerp(0.5f, 1f, Mathf.Sin(num * (float)Math.PI)) * Mathf.Lerp(0.8f, 1.2f, UnityEngine.Random.value) * rad / 8f;
			sLeaser.sprites[sprite].alpha = Mathf.Sin(num * (float)Math.PI) * Mathf.Lerp(0.6f, 1f, UnityEngine.Random.value) * 0.6f * owner.Intensity;
		}
	}

	public class SparkFlash : CosmeticSprite
	{
		public float size;

		public float life;

		public float lastLife;

		public float lifeTime;

		public SparkFlash(Vector2 pos, float size)
		{
			base.pos = pos;
			lastPos = pos;
			this.size = size;
			life = 1f;
			lastLife = 1f;
			lifeTime = Mathf.Lerp(2f, 16f, size * UnityEngine.Random.value);
		}

		public override void Update(bool eu)
		{
			room.AddObject(new Spark(pos, Custom.RNV() * 60f * UnityEngine.Random.value, new Color(0f, 1f, 0f), null, 4, 50));
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
			sLeaser.sprites = new FSprite[3];
			sLeaser.sprites[0] = new FSprite("Futile_White");
			sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["LightSource"];
			sLeaser.sprites[0].color = new Color(0f, 1f, 0f);
			sLeaser.sprites[1] = new FSprite("Futile_White");
			sLeaser.sprites[1].shader = rCam.room.game.rainWorld.Shaders["FlatLight"];
			sLeaser.sprites[1].color = new Color(0f, 1f, 0f);
			sLeaser.sprites[2] = new FSprite("Futile_White");
			sLeaser.sprites[2].shader = rCam.room.game.rainWorld.Shaders["FlareBomb"];
			sLeaser.sprites[2].color = new Color(0f, 1f, 0f);
			AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Water"));
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
			float num = Mathf.Lerp(lastLife, life, timeStacker);
			for (int i = 0; i < 3; i++)
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
		}
	}

	private RoomSettings.RoomEffect effect;

	public List<IntVector2> closeToWallTiles;

	public LightFlash[] flashes;

	public Vector3 color;

	public Vector3 lastColor;

	public Vector3 getToColor;

	private float sin;

	private float lastSin;

	public DisembodiedDynamicSoundLoop soundLoop;

	public DisembodiedDynamicSoundLoop soundLoop2;

	private RainCycle cycle => room.game.world.rainCycle;

	public float Intensity
	{
		get
		{
			if (ModManager.MSC && room != null && room.game.IsStorySession && room.game.GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel)
			{
				return effect.amount * Mathf.Pow(Mathf.InverseLerp(cycle.cycleLength - 400, cycle.cycleLength + 1200, cycle.timer), 2.2f);
			}
			if (ModManager.MSC)
			{
				float num = Mathf.Clamp(Mathf.Log((float)cycle.preTimer / (float)cycle.maxPreTimer + 0.1f) + 1f, 0f, 1f);
				float a = num * 0.4f;
				a = Mathf.Lerp(a, Mathf.Clamp(cycle.preCycleRain_Intensity, 0f, 2f) / 1.3f, 0.7f);
				a *= num;
				float b = Mathf.Pow(Mathf.InverseLerp(cycle.cycleLength - 400, cycle.cycleLength + 1200, cycle.timer), 2.2f);
				return effect.amount * Mathf.Max(a, b);
			}
			return effect.amount * Mathf.Pow(Mathf.InverseLerp(cycle.cycleLength - 400, cycle.cycleLength + 1200, cycle.timer), 2.2f);
		}
	}

	public ElectricDeath(RoomSettings.RoomEffect effect, Room room)
	{
		this.effect = effect;
		flashes = new LightFlash[9];
		for (int i = 0; i < flashes.Length; i++)
		{
			flashes[i] = new LightFlash(this, i + 1);
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (Intensity == 0f)
		{
			return;
		}
		if (soundLoop == null)
		{
			soundLoop = new DisembodiedDynamicSoundLoop(this);
			soundLoop.sound = SoundID.Death_Lightning_Heavy_Lightning_LOOP;
			soundLoop.Volume = 0f;
		}
		else
		{
			soundLoop.Update();
			soundLoop.Volume = Intensity;
		}
		if (soundLoop2 == null)
		{
			soundLoop2 = new DisembodiedDynamicSoundLoop(this);
			soundLoop2.sound = SoundID.Death_Lightning_Early_Sizzle_LOOP;
			soundLoop2.Volume = 0f;
		}
		else
		{
			soundLoop2.Update();
			soundLoop2.Volume = Mathf.Pow(Intensity, 0.1f) * Mathf.Lerp(0.5f + 0.5f * Mathf.Sin(sin * (float)Math.PI * 2f), 0f, Mathf.Pow(Intensity, 8f));
		}
		lastSin = sin;
		sin += Intensity * 0.1f;
		if (closeToWallTiles != null && closeToWallTiles.Count > 0 && room.BeingViewed && UnityEngine.Random.value < Mathf.InverseLerp(1000f, 9120f, room.TileWidth * room.TileHeight) * Intensity)
		{
			IntVector2 pos = closeToWallTiles[UnityEngine.Random.Range(0, closeToWallTiles.Count)];
			Vector2 pos2 = room.MiddleOfTile(pos) + new Vector2(Mathf.Lerp(-10f, 10f, UnityEngine.Random.value), Mathf.Lerp(-10f, 10f, UnityEngine.Random.value));
			float num = UnityEngine.Random.value * Intensity;
			if (room.ViewedByAnyCamera(pos2, 50f))
			{
				room.AddObject(new SparkFlash(pos2, num));
			}
			room.PlaySound(SoundID.Death_Lightning_Spark_Spontaneous, pos2, num, 1f);
		}
		for (int i = 1; i < flashes.Length; i++)
		{
			flashes[i].Update();
		}
		lastColor = color;
		color = Vector3.Lerp(color, getToColor, UnityEngine.Random.value * 0.3f);
		if (UnityEngine.Random.value < 1f / 3f)
		{
			getToColor.x = UnityEngine.Random.value;
		}
		else if (UnityEngine.Random.value < 1f / 3f)
		{
			getToColor.y = UnityEngine.Random.value;
		}
		else if (UnityEngine.Random.value < 1f / 3f)
		{
			getToColor.z = UnityEngine.Random.value;
		}
		if (!(Intensity > 0.5f) || !(UnityEngine.Random.value < Custom.LerpMap(Intensity, 0.5f, 1f, 0f, 0.5f)))
		{
			return;
		}
		for (int j = 0; j < room.physicalObjects.Length; j++)
		{
			for (int k = 0; k < room.physicalObjects[j].Count; k++)
			{
				for (int l = 0; l < room.physicalObjects[j][k].bodyChunks.Length; l++)
				{
					if (!(UnityEngine.Random.value < Custom.LerpMap(Intensity, 0.5f, 1f, 0f, 0.5f)) || (room.physicalObjects[j][k].bodyChunks[l].ContactPoint.x == 0 && room.physicalObjects[j][k].bodyChunks[l].ContactPoint.y == 0 && (!room.GetTile(room.physicalObjects[j][k].bodyChunks[l].pos).AnyBeam || (ModManager.MSC && cycle.preTimer > 0))))
					{
						continue;
					}
					float num2 = Mathf.Pow(UnityEngine.Random.value, 0.9f) * Mathf.InverseLerp(0.5f, 1f, Intensity);
					room.AddObject(new SparkFlash(room.physicalObjects[j][k].bodyChunks[l].pos + room.physicalObjects[j][k].bodyChunks[l].rad * room.physicalObjects[j][k].bodyChunks[l].ContactPoint.ToVector2(), Mathf.Pow(num2, 0.5f)));
					Vector2 vector = -(room.physicalObjects[j][k].bodyChunks[l].ContactPoint.ToVector2() + Custom.RNV()).normalized;
					vector *= 22f * num2 / room.physicalObjects[j][k].bodyChunks[l].mass;
					room.physicalObjects[j][k].bodyChunks[l].vel += vector;
					room.PlaySound(SoundID.Death_Lightning_Spark_Object, room.physicalObjects[j][k].bodyChunks[l].pos, num2, 1f);
					if (room.physicalObjects[j][k] is Creature)
					{
						float damage = num2 * 1.8f;
						if (ModManager.MSC && cycle.preTimer > 0)
						{
							damage = 0f;
						}
						if (ModManager.MMF && cycle.TimeUntilRain <= -1500 && Intensity >= 0.5f)
						{
							damage = Mathf.Pow(UnityEngine.Random.value, 0.9f) * 2f;
						}
						(room.physicalObjects[j][k] as Creature).Violence(null, null, room.physicalObjects[j][k].bodyChunks[l], null, Creature.DamageType.Electric, damage, num2 * 40f);
					}
					if (ModManager.MSC && room.physicalObjects[j][k] is ElectricSpear)
					{
						if ((room.physicalObjects[j][k] as ElectricSpear).abstractSpear.electricCharge == 0)
						{
							(room.physicalObjects[j][k] as ElectricSpear).Recharge();
						}
						else
						{
							(room.physicalObjects[j][k] as ElectricSpear).ExplosiveShortCircuit();
						}
					}
				}
			}
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1 + flashes.Length];
		sLeaser.sprites[0] = new FSprite("Futile_White");
		sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["ElectricDeath"];
		sLeaser.sprites[0].scaleX = 87.5f;
		sLeaser.sprites[0].scaleY = 50f;
		for (int i = 1; i < 10; i++)
		{
			sLeaser.sprites[i] = new FSprite("Futile_White");
			sLeaser.sprites[i].shader = rCam.room.game.rainWorld.Shaders["LightSource"];
			sLeaser.sprites[i].color = new Color(0f, 1f, 0f);
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		sLeaser.sprites[0].x = rCam.room.game.rainWorld.screenSize.x / 2f;
		sLeaser.sprites[0].y = rCam.room.game.rainWorld.screenSize.y / 2f;
		sLeaser.sprites[0].color = new Color(Mathf.Lerp(lastColor.x, color.x, timeStacker), Mathf.Lerp(lastColor.y, color.y, timeStacker), Mathf.Lerp(lastColor.z, color.z, timeStacker));
		sLeaser.sprites[0].alpha = Mathf.Pow(Intensity, 0.5f) * Mathf.Lerp(0.5f + 0.5f * Mathf.Sin(Mathf.Lerp(lastSin, sin, timeStacker) * (float)Math.PI * 2f), 1f, Mathf.Pow(Intensity, 4f));
		for (int i = 1; i < flashes.Length; i++)
		{
			flashes[i].Draw(sLeaser, rCam, timeStacker, camPos);
		}
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
			newContatiner = rCam.ReturnFContainer("Water");
		}
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i].RemoveFromContainer();
			newContatiner.AddChild(sLeaser.sprites[i]);
		}
	}

	public void ShortcutsReady()
	{
	}

	public void AIMapReady()
	{
		closeToWallTiles = new List<IntVector2>();
		for (int i = 0; i < room.TileWidth; i++)
		{
			for (int j = 0; j < room.TileHeight; j++)
			{
				if (room.aimap.getTerrainProximity(i, j) == 1)
				{
					closeToWallTiles.Add(new IntVector2(i, j));
				}
			}
		}
	}
}
