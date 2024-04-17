using System;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class ColdRoom : UpdatableAndDeletable
{
	public class ColdBreath : CosmeticSprite
	{
		public Vector2 velo;

		public float life;

		public float lifeTime;

		public float startAlpha;

		public ColdBreath(Vector2 position, Vector2 velocity, float lifeTime)
		{
			pos = position;
			lastPos = position;
			velo = velocity;
			life = lifeTime;
			this.lifeTime = lifeTime;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			pos += velo;
			velo -= velo * 0.1f;
			startAlpha = 1f;
			life -= 1f;
			if (life <= 0f)
			{
				Destroy();
			}
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			if (UnityEngine.Random.value < 0.2f)
			{
				sLeaser.sprites[0] = new FSprite("Futile_White");
				sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["FlatLight"];
				startAlpha = 0.5f;
			}
			else
			{
				sLeaser.sprites[0] = new FSprite("Pebble" + UnityEngine.Random.Range(1, 10));
				sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["Hologram"];
			}
			sLeaser.sprites[0].rotation = UnityEngine.Random.value * 360f;
			AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("ForegroundLights"));
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = new Vector2(Mathf.Lerp(lastPos.x, pos.x, timeStacker), Mathf.Lerp(lastPos.y, pos.y, timeStacker)) - camPos;
			sLeaser.sprites[0].x = vector.x;
			sLeaser.sprites[0].y = vector.y;
			sLeaser.sprites[0].alpha = Mathf.Lerp(0f, startAlpha, (life - timeStacker) / lifeTime);
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
	}

	public ColdRoom(Room room)
	{
		base.room = room;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		AbstractCreature firstAlivePlayer = room.game.FirstAlivePlayer;
		if (room.game.Players.Count <= 0 || firstAlivePlayer == null || firstAlivePlayer.Room != room.abstractRoom)
		{
			return;
		}
		for (int i = 0; i < room.abstractRoom.creatures.Count; i++)
		{
			if (room.abstractRoom.creatures[i] == null || room.abstractRoom.creatures[i].realizedCreature == null || room.abstractRoom.creatures[i].realizedCreature.inShortcut || !(room.abstractRoom.creatures[i].realizedCreature.Submersion < 0.5f))
			{
				continue;
			}
			Creature realizedCreature = room.abstractRoom.creatures[i].realizedCreature;
			if (!(realizedCreature is Player))
			{
				continue;
			}
			Vector2 pos = realizedCreature.firstChunk.pos;
			if (realizedCreature.graphicsModule == null)
			{
				continue;
			}
			PlayerGraphics playerGraphics = realizedCreature.graphicsModule as PlayerGraphics;
			float num = Mathf.Sin(playerGraphics.breath * (float)Math.PI * 2f);
			float num2 = Mathf.Sin(playerGraphics.lastBreath * (float)Math.PI * 2f);
			if (playerGraphics != null && num < num2 && num < 0.5f && num > -0.5f)
			{
				Vector2 vector = playerGraphics.lookDirection * 8f;
				Vector2 vector2 = new Vector2(0f, 5f);
				if ((realizedCreature as Player).bodyMode == Player.BodyModeIndex.Crawl)
				{
					vector = playerGraphics.lookDirection * 16f;
					vector2.x = (float)(realizedCreature as Player).flipDirection * 20f;
				}
				room.AddObject(new ColdBreath(pos + vector2 + vector, Custom.RNV() * 0.2f + vector * 0.1f + realizedCreature.firstChunk.vel * 0.25f, UnityEngine.Random.value * 20f + 5f));
			}
		}
	}
}
