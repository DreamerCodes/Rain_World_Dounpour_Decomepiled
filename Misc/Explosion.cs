using System;
using Expedition;
using MoreSlugcats;
using Noise;
using RWCustom;
using UnityEngine;

public class Explosion : UpdatableAndDeletable
{
	public interface IReactToExplosions
	{
		void Explosion(Explosion explosion);
	}

	public class ExplosionSmoke : CosmeticSprite
	{
		public float life;

		protected float lastLife;

		public float lifeTime;

		public float rotation;

		public float lastRotation;

		public float rotVel;

		public Vector2 getToPos;

		public float rad;

		public Color colorA;

		public Color colorB;

		public ExplosionSmoke(Vector2 pos, Vector2 vel, float size)
		{
			life = size;
			lastLife = size;
			lastPos = pos;
			base.vel = vel;
			getToPos = pos + new Vector2(Mathf.Lerp(-50f, 50f, UnityEngine.Random.value), Mathf.Lerp(-100f, 400f, UnityEngine.Random.value));
			base.pos = pos + vel.normalized * 60f * UnityEngine.Random.value;
			rad = Mathf.Lerp(0.6f, 1.5f, UnityEngine.Random.value) * size;
			rotation = UnityEngine.Random.value * 360f;
			lastRotation = rotation;
			rotVel = Mathf.Lerp(-6f, 6f, UnityEngine.Random.value);
			lifeTime = Mathf.Lerp(170f, 400f, UnityEngine.Random.value);
		}

		public override void Update(bool eu)
		{
			vel *= 0.9f;
			vel += Custom.DirVec(pos, getToPos) * UnityEngine.Random.value * 0.04f;
			lastRotation = rotation;
			rotation += rotVel * vel.magnitude;
			lastLife = life;
			life -= 1f / lifeTime;
			if (room.GetTile(pos).Solid && !room.GetTile(lastPos).Solid)
			{
				IntVector2? intVector = SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(room, room.GetTilePosition(lastPos), room.GetTilePosition(pos));
				FloatRect floatRect = Custom.RectCollision(pos, lastPos, room.TileRect(intVector.Value).Grow(2f));
				pos = floatRect.GetCorner(FloatRect.CornerLabel.D);
				if (floatRect.GetCorner(FloatRect.CornerLabel.B).x < 0f)
				{
					vel.x = Mathf.Abs(vel.x);
				}
				else if (floatRect.GetCorner(FloatRect.CornerLabel.B).x > 0f)
				{
					vel.x = 0f - Mathf.Abs(vel.x);
				}
				else if (floatRect.GetCorner(FloatRect.CornerLabel.B).y < 0f)
				{
					vel.y = Mathf.Abs(vel.y);
				}
				else if (floatRect.GetCorner(FloatRect.CornerLabel.B).y > 0f)
				{
					vel.y = 0f - Mathf.Abs(vel.y);
				}
			}
			if (lastLife <= 0f)
			{
				Destroy();
			}
			base.Update(eu);
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[2];
			for (int i = 0; i < 2; i++)
			{
				sLeaser.sprites[i] = new FSprite("Futile_White");
				sLeaser.sprites[i].shader = rCam.room.game.rainWorld.Shaders["FireSmoke"];
			}
			AddToContainer(sLeaser, rCam, null);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
			float num = Mathf.Lerp(lastLife, life, timeStacker);
			for (int i = 0; i < 2; i++)
			{
				sLeaser.sprites[i].x = vector.x - camPos.x;
				sLeaser.sprites[i].y = vector.y - camPos.y;
				sLeaser.sprites[i].rotation = Mathf.Lerp(lastRotation, rotation, timeStacker);
				sLeaser.sprites[i].scale = 11f * rad * ((num > 0.5f) ? Custom.LerpMap(num, 1f, 0.5f, 0.5f, 1f) : Mathf.Sin(num * (float)Math.PI)) * ((i == 0) ? 1.1f : 0.9f);
				float alpha = Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(lastLife, life, timeStacker)), 1.8f) * ((i == 0) ? 0.8f : 0.6f);
				sLeaser.sprites[i].alpha = alpha;
			}
			sLeaser.sprites[0].color = Color.Lerp(colorB, colorA, 0.2f + 0.8f * Mathf.Pow(num, 0.5f));
			sLeaser.sprites[1].color = Color.Lerp(colorB, colorA, num);
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}

		public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			Color pixel = palette.texture.GetPixel(0, 1);
			colorA = Color.Lerp(pixel, palette.fogColor, 0.1f);
			colorB = Color.Lerp(pixel, palette.fogColor, 0.4f);
		}

		public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			base.AddToContainer(sLeaser, rCam, newContatiner);
		}
	}

	public class FlashingSmoke : ExplosionSmoke
	{
		public Color whiteColor;

		public Color effectColor;

		public float col;

		public float lastCol;

		public int colorFadeTime;

		public FlashingSmoke(Vector2 pos, Vector2 vel, float size, Color whiteColor, Color effectColor, int colorFadeTime)
			: base(pos, vel, size)
		{
			this.whiteColor = whiteColor;
			this.effectColor = effectColor;
			this.colorFadeTime = colorFadeTime;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			lastCol = col;
			col += 1f;
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
			float num = Mathf.Lerp(lastLife, life, timeStacker);
			float num2 = Mathf.InverseLerp(colorFadeTime, 0.5f, Mathf.Lerp(lastCol, col, timeStacker));
			sLeaser.sprites[0].color = Color.Lerp(Color.Lerp(colorB, colorA, 0.2f + 0.8f * Mathf.Pow(num, 0.5f)), Color.Lerp(effectColor, whiteColor, Mathf.Pow(num2, 1.2f)), num2);
			sLeaser.sprites[1].color = Color.Lerp(Color.Lerp(colorB, colorA, num), Color.Lerp(effectColor, whiteColor, Mathf.Pow(num2, 0.6f)), num2);
		}
	}

	public class ExplosionLight : CosmeticSprite
	{
		private float rad;

		private float life;

		private float lastLife;

		private int lifeTime;

		private float alpha;

		private Color lightColor;

		public ExplosionLight(Vector2 pos, float rad, float alpha, int lifeTime, Color lightColor)
		{
			base.pos = pos;
			lastPos = pos;
			this.rad = rad;
			this.alpha = alpha;
			this.lifeTime = lifeTime;
			this.lightColor = lightColor;
			life = 1f;
			lastLife = 0f;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			lastLife = life;
			life -= 1f / (float)lifeTime;
			if (!(lastLife < 0f))
			{
				return;
			}
			if (ModManager.MSC && room != null && room.blizzard && room.abstractRoom.creatures != null)
			{
				foreach (AbstractCreature creature in room.abstractRoom.creatures)
				{
					if (creature != null && creature.realizedCreature != null && Vector2.Distance(creature.realizedCreature.firstChunk.pos, pos) < rad + rad * 0.15f)
					{
						creature.Hypothermia = Mathf.Lerp(creature.Hypothermia, 0f, 0.04f);
						Custom.Log("Explosion restored hypothermia in", creature.ToString(), "to", creature.Hypothermia.ToString());
					}
				}
			}
			Destroy();
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[3];
			sLeaser.sprites[0] = new FSprite("Futile_White");
			sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["FlatLight"];
			sLeaser.sprites[0].color = new Color(0f, 0f, 0f);
			for (int i = 1; i < 3; i++)
			{
				sLeaser.sprites[i] = new FSprite("Futile_White");
				sLeaser.sprites[i].shader = rCam.room.game.rainWorld.Shaders["LightSource"];
				sLeaser.sprites[i].color = lightColor;
			}
			AddToContainer(sLeaser, rCam, null);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			for (int i = 0; i < 3; i++)
			{
				sLeaser.sprites[i].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x;
				sLeaser.sprites[i].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y;
			}
			float num = Mathf.Lerp(lastLife, life, timeStacker);
			sLeaser.sprites[0].alpha = num * alpha * 0.5f;
			sLeaser.sprites[0].scale = Mathf.Pow(num, 0.5f) * rad / 8f;
			for (int j = 1; j < 3; j++)
			{
				sLeaser.sprites[j].alpha = Mathf.Pow(num, 0.5f) * alpha;
				sLeaser.sprites[j].scale = Mathf.Pow(num, 0.5f) * rad / 8f;
			}
			sLeaser.sprites[1].color = lightColor;
			sLeaser.sprites[2].color = Color.Lerp(lightColor, new Color(1f, 1f, 1f), UnityEngine.Random.value * Mathf.Pow(num, 0.5f));
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}

		public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
		}

		public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			if (newContatiner == null)
			{
				newContatiner = rCam.ReturnFContainer("Bloom");
			}
			base.AddToContainer(sLeaser, rCam, newContatiner);
		}
	}

	public int frame;

	public int lifeTime;

	public float rad;

	public float force;

	public float damage;

	public float stun;

	public float deafen;

	public Vector2 pos;

	public PhysicalObject sourceObject;

	public Creature killTagHolder;

	public float killTagHolderDmgFactor;

	public float minStun;

	public float backgroundNoise;

	private bool explosionReactorsNotified;

	public Explosion(Room room, PhysicalObject sourceObject, Vector2 pos, int lifeTime, float rad, float force, float damage, float stun, float deafen, Creature killTagHolder, float killTagHolderDmgFactor, float minStun, float backgroundNoise)
	{
		base.room = room;
		this.sourceObject = sourceObject;
		this.pos = pos;
		this.lifeTime = lifeTime;
		this.rad = rad;
		this.force = force;
		this.damage = damage;
		this.stun = stun;
		this.deafen = deafen;
		this.killTagHolder = killTagHolder;
		this.killTagHolderDmgFactor = killTagHolderDmgFactor;
		this.minStun = minStun;
		this.backgroundNoise = backgroundNoise;
	}

	private Vector2 PushAngle(Vector2 A, Vector2 B)
	{
		return Vector3.Slerp((B - A).normalized, new Vector2(0f, 1f), 0.2f);
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (!explosionReactorsNotified)
		{
			explosionReactorsNotified = true;
			for (int i = 0; i < room.updateList.Count; i++)
			{
				if (room.updateList[i] is IReactToExplosions)
				{
					(room.updateList[i] as IReactToExplosions).Explosion(this);
				}
			}
			if (room.waterObject != null)
			{
				room.waterObject.Explosion(this);
			}
			if (sourceObject != null)
			{
				room.InGameNoise(new InGameNoise(pos, backgroundNoise * 2700f, sourceObject, backgroundNoise * 6f));
			}
		}
		room.MakeBackgroundNoise(backgroundNoise);
		float num = rad * (0.25f + 0.75f * Mathf.Sin(Mathf.InverseLerp(0f, lifeTime, frame) * (float)Math.PI));
		for (int j = 0; j < room.physicalObjects.Length; j++)
		{
			for (int k = 0; k < room.physicalObjects[j].Count; k++)
			{
				if (sourceObject == room.physicalObjects[j][k] || room.physicalObjects[j][k].slatedForDeletetion)
				{
					continue;
				}
				float num2 = 0f;
				float num3 = float.MaxValue;
				int num4 = -1;
				for (int l = 0; l < room.physicalObjects[j][k].bodyChunks.Length; l++)
				{
					float num5 = Vector2.Distance(pos, room.physicalObjects[j][k].bodyChunks[l].pos);
					num3 = Mathf.Min(num3, num5);
					if (!(num5 < num))
					{
						continue;
					}
					float num6 = Mathf.InverseLerp(num, num * 0.25f, num5);
					if (!room.VisualContact(pos, room.physicalObjects[j][k].bodyChunks[l].pos))
					{
						num6 -= 0.5f;
					}
					if (num6 > 0f)
					{
						float num7 = force;
						if (ModManager.MSC && room.physicalObjects[j][k] is Player && (room.physicalObjects[j][k] as Player).SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
						{
							num7 *= 0.25f;
						}
						room.physicalObjects[j][k].bodyChunks[l].vel += PushAngle(pos, room.physicalObjects[j][k].bodyChunks[l].pos) * (num7 / room.physicalObjects[j][k].bodyChunks[l].mass) * num6;
						room.physicalObjects[j][k].bodyChunks[l].pos += PushAngle(pos, room.physicalObjects[j][k].bodyChunks[l].pos) * (num7 / room.physicalObjects[j][k].bodyChunks[l].mass) * num6 * 0.1f;
						if (num6 > num2)
						{
							num2 = num6;
							num4 = l;
						}
					}
				}
				if (room.physicalObjects[j][k] == killTagHolder)
				{
					num2 *= killTagHolderDmgFactor;
				}
				if (deafen > 0f && room.physicalObjects[j][k] is Creature)
				{
					(room.physicalObjects[j][k] as Creature).Deafen((int)Custom.LerpMap(num3, num * 1.5f * deafen, num * Mathf.Lerp(1f, 4f, deafen), 650f * deafen, 0f));
				}
				if (num4 <= -1)
				{
					continue;
				}
				if (room.physicalObjects[j][k] is Creature)
				{
					for (int m = 0; (float)m < Math.Min(Mathf.Round(num2 * damage * 2f), 8f); m++)
					{
						Vector2 p = room.physicalObjects[j][k].bodyChunks[num4].pos + Custom.RNV() * room.physicalObjects[j][k].bodyChunks[num4].rad * UnityEngine.Random.value;
						room.AddObject(new WaterDrip(p, Custom.DirVec(pos, p) * force * UnityEngine.Random.value * num2, waterColor: false));
					}
					if (killTagHolder != null && room.physicalObjects[j][k] != killTagHolder)
					{
						(room.physicalObjects[j][k] as Creature).SetKillTag(killTagHolder.abstractCreature);
					}
					float num8 = damage;
					if ((room.physicalObjects[j][k] is Player && (room.physicalObjects[j][k] as Player).SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer) || (ModManager.Expedition && room.game.rainWorld.ExpeditionMode && ExpeditionGame.activeUnlocks.Contains("unl-explosionimmunity")))
					{
						num8 *= 0.2f;
					}
					if (room.physicalObjects[j][k] is Player && (room.physicalObjects[j][k] as Player).SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint)
					{
						num8 *= 4f;
						if (ModManager.Expedition && room.game.rainWorld.ExpeditionMode && ExpeditionGame.activeUnlocks.Contains("unl-explosionimmunity"))
						{
							num8 /= 4f;
						}
					}
					(room.physicalObjects[j][k] as Creature).Violence(null, null, room.physicalObjects[j][k].bodyChunks[num4], null, Creature.DamageType.Explosion, num2 * num8 / (((room.physicalObjects[j][k] as Creature).State is HealthState) ? ((float)lifeTime) : 1f), num2 * stun);
					if (minStun > 0f && (!ModManager.MSC || !(room.physicalObjects[j][k] is Player) || (room.physicalObjects[j][k] as Player).SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Artificer || (ModManager.Expedition && room.game.rainWorld.ExpeditionMode && ExpeditionGame.activeUnlocks.Contains("unl-explosionimmunity"))))
					{
						(room.physicalObjects[j][k] as Creature).Stun((int)(minStun * Mathf.InverseLerp(0f, 0.5f, num2)));
					}
					if ((room.physicalObjects[j][k] as Creature).graphicsModule != null && (room.physicalObjects[j][k] as Creature).graphicsModule.bodyParts != null)
					{
						for (int n = 0; n < (room.physicalObjects[j][k] as Creature).graphicsModule.bodyParts.Length; n++)
						{
							float num9 = force;
							if ((ModManager.MSC && room.physicalObjects[j][k] is Player && (room.physicalObjects[j][k] as Player).SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer) || (ModManager.Expedition && room.game.rainWorld.ExpeditionMode && ExpeditionGame.activeUnlocks.Contains("unl-explosionimmunity")))
							{
								num9 *= 0.25f;
							}
							(room.physicalObjects[j][k] as Creature).graphicsModule.bodyParts[n].pos += PushAngle(pos, (room.physicalObjects[j][k] as Creature).graphicsModule.bodyParts[n].pos) * num2 * num9 * 5f;
							(room.physicalObjects[j][k] as Creature).graphicsModule.bodyParts[n].vel += PushAngle(pos, (room.physicalObjects[j][k] as Creature).graphicsModule.bodyParts[n].pos) * num2 * num9 * 5f;
							if ((room.physicalObjects[j][k] as Creature).graphicsModule.bodyParts[n] is Limb)
							{
								((room.physicalObjects[j][k] as Creature).graphicsModule.bodyParts[n] as Limb).mode = Limb.Mode.Dangle;
							}
						}
					}
				}
				room.physicalObjects[j][k].HitByExplosion(num2, this, num4);
			}
		}
		frame++;
		if (frame > lifeTime)
		{
			Destroy();
		}
	}
}
