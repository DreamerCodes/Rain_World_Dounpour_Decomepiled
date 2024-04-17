using System;
using RWCustom;
using UnityEngine;

public class UnderwaterShock : UpdatableAndDeletable
{
	public class Flash : CosmeticSprite
	{
		private float rad;

		private float life;

		private float lastLife;

		private int lifeTime;

		private float alpha;

		private Color lightColor;

		public Flash(Vector2 pos, float rad, float alpha, int lifeTime, Color lightColor)
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
			if (lastLife < 0f)
			{
				Destroy();
			}
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite("Futile_White");
			sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["FlatLight"];
			sLeaser.sprites[0].color = lightColor;
			AddToContainer(sLeaser, rCam, null);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			sLeaser.sprites[0].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x;
			sLeaser.sprites[0].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y;
			float num = Mathf.Lerp(lastLife, life, timeStacker);
			sLeaser.sprites[0].alpha = num * UnityEngine.Random.value * alpha * 0.5f;
			sLeaser.sprites[0].scale = Mathf.Pow(num, 0.5f) * rad * UnityEngine.Random.value / 8f;
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

	public float damage;

	public Vector2 pos;

	public PhysicalObject expemtObject;

	public Creature killTagHolder;

	private Color color;

	public UnderwaterShock(Room room, PhysicalObject expemtObject, Vector2 pos, int lifeTime, float rad, float damage, Creature killTagHolder, Color color)
	{
		base.room = room;
		this.expemtObject = expemtObject;
		this.pos = pos;
		this.lifeTime = lifeTime;
		this.rad = rad;
		this.damage = damage;
		this.killTagHolder = killTagHolder;
		this.color = color;
		room.AddObject(new Flash(pos, rad * 2f + 100f, 1f, lifeTime, color));
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		float num = rad * (0.25f + 0.75f * Mathf.Sin(Mathf.InverseLerp(0f, lifeTime, frame) * (float)Math.PI));
		for (int i = 0; i < room.abstractRoom.creatures.Count; i++)
		{
			if (room.abstractRoom.creatures[i].realizedCreature == null || room.abstractRoom.creatures[i].realizedCreature == expemtObject || !(room.abstractRoom.creatures[i].realizedCreature.Submersion > 0f))
			{
				continue;
			}
			float num2 = 0f;
			for (int j = 0; j < room.abstractRoom.creatures[i].realizedCreature.bodyChunks.Length; j++)
			{
				if (Custom.DistLess(pos, room.abstractRoom.creatures[i].realizedCreature.bodyChunks[j].pos, num))
				{
					num2 = Mathf.Max(num2, Custom.LerpMap(Vector2.Distance(pos, room.abstractRoom.creatures[i].realizedCreature.bodyChunks[j].pos), num / 2f, num, 1f, 0f, 0.5f) * room.abstractRoom.creatures[i].realizedCreature.bodyChunks[j].submersion);
				}
			}
			if (room.abstractRoom.shelter)
			{
				num2 = 0f;
			}
			if (num2 > 0f)
			{
				for (int k = 0; k < room.abstractRoom.creatures[i].realizedCreature.bodyChunks.Length; k++)
				{
					room.abstractRoom.creatures[i].realizedCreature.bodyChunks[k].vel += Custom.RNV() * num2 * Mathf.Min(5f, room.abstractRoom.creatures[i].realizedCreature.bodyChunks[k].rad);
				}
				if (UnityEngine.Random.value < 0.25f)
				{
					room.AddObject(new Flash(room.abstractRoom.creatures[i].realizedCreature.bodyChunks[UnityEngine.Random.Range(0, room.abstractRoom.creatures[i].realizedCreature.bodyChunks.Length)].pos, room.abstractRoom.creatures[i].realizedCreature.TotalMass * 60f * num2 + 140f, Mathf.Pow(num2, 0.2f), lifeTime - frame, color));
				}
				room.abstractRoom.creatures[i].realizedCreature.Violence(null, null, room.abstractRoom.creatures[i].realizedCreature.bodyChunks[UnityEngine.Random.Range(0, room.abstractRoom.creatures[i].realizedCreature.bodyChunks.Length)], null, Creature.DamageType.Electric, damage * num2, damage * num2 * 240f + 30f);
			}
		}
		frame++;
		if (frame > lifeTime)
		{
			Destroy();
		}
	}
}
