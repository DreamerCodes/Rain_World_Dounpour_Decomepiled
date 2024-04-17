using System;
using RWCustom;
using UnityEngine;

public class BulletDrip : UpdatableAndDeletable, IDrawable
{
	public RoomRain roomRain;

	public Vector2 pos;

	public Vector2 skyPos;

	public int counter;

	public float lastFalling = 1f;

	public float falling = 1f;

	public bool moveTip;

	public float fallSpeed;

	public int delay;

	public BulletDrip(RoomRain roomRain)
	{
		this.roomRain = roomRain;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		lastFalling = falling;
		falling = Mathf.Min(1f, falling + fallSpeed);
		moveTip = false;
		if (lastFalling >= 1f)
		{
			delay--;
			if (delay < 1)
			{
				Strike();
			}
		}
	}

	private void Strike()
	{
		int num = UnityEngine.Random.Range(0, room.TileWidth);
		if (room.GetTile(num, room.TileHeight - 1).Solid)
		{
			return;
		}
		int num2 = room.roomRain.rainReach[num];
		if (num2 >= room.TileHeight)
		{
			return;
		}
		if (num2 == 0)
		{
			num2 = -10;
		}
		pos = room.MiddleOfTile(num, num2) + new Vector2(Mathf.Lerp(-10f, 10f, UnityEngine.Random.value), 10f);
		skyPos = room.MiddleOfTile(num, num2 + room.TileHeight + 50) + new Vector2(Mathf.Lerp(-30f, 30f, UnityEngine.Random.value) - 30f * roomRain.globalRain.rainDirection, 0f);
		falling = 0f;
		lastFalling = 0f;
		moveTip = true;
		fallSpeed = 1f / Mathf.Lerp(0.2f, 1.8f, UnityEngine.Random.value);
		delay = UnityEngine.Random.Range(0, 60 - (int)(roomRain.globalRain.bulletRainDensity * 60f));
		SharedPhysics.CollisionResult collisionResult = SharedPhysics.TraceProjectileAgainstBodyChunks(null, room, skyPos, ref pos, 0.5f, 1, null, hitAppendages: false);
		if (room.water && room.PointSubmerged(pos))
		{
			pos.y = room.FloatWaterLevel(pos.x) - 30f;
			room.waterObject.WaterfallHitSurface(pos.x, pos.x, 1f);
			room.PlaySound(SoundID.Small_Object_Into_Water_Fast, pos);
		}
		else
		{
			room.PlaySound(SoundID.Bullet_Drip_Strike, pos);
		}
		if (collisionResult.chunk != null)
		{
			pos = collisionResult.collisionPoint;
			collisionResult.chunk.vel.y -= 2f / collisionResult.chunk.mass;
			if (collisionResult.chunk.owner is Creature)
			{
				(collisionResult.chunk.owner as Creature).Stun(UnityEngine.Random.Range(0, 4));
			}
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[2];
		sLeaser.sprites[0] = new FSprite("Futile_White");
		sLeaser.sprites[0].scaleX = 0.125f;
		sLeaser.sprites[0].anchorY = 0f;
		sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["BulletRain"];
		sLeaser.sprites[1] = new FSprite("RainSplash");
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = pos;
		if (moveTip)
		{
			vector = Vector2.Lerp(skyPos, pos, timeStacker);
		}
		Vector2 vector2 = Vector2.Lerp(skyPos, pos, Mathf.Lerp(lastFalling, falling, timeStacker));
		sLeaser.sprites[0].x = vector.x - camPos.x;
		sLeaser.sprites[0].y = vector.y - camPos.y;
		sLeaser.sprites[0].rotation = Custom.AimFromOneVectorToAnother(vector, vector2);
		sLeaser.sprites[0].scaleY = Vector2.Distance(vector, vector2) / 16f;
		sLeaser.sprites[1].x = vector.x - camPos.x;
		sLeaser.sprites[1].y = vector.y - camPos.y;
		sLeaser.sprites[1].scale = Mathf.Sin(Mathf.Lerp(lastFalling, falling, timeStacker) * (float)Math.PI) * 0.4f;
		sLeaser.sprites[1].rotation = UnityEngine.Random.value * 360f;
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
		rCam.ReturnFContainer("GrabShaders").AddChild(sLeaser.sprites[0]);
		rCam.ReturnFContainer("Items").AddChild(sLeaser.sprites[1]);
	}
}
