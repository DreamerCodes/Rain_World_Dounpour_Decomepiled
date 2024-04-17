using RWCustom;
using Smoke;
using UnityEngine;

namespace MoreSlugcats;

public class Bullet : Weapon
{
	public SporesSmoke ashTrail;

	private LightSource light;

	private bool didFirstPuff;

	public float darkness;

	public float lastDarkness;

	public AbstractBullet abstractBullet => abstractPhysicalObject as AbstractBullet;

	public Bullet(AbstractPhysicalObject abstractPhysicalObject, World world)
		: base(abstractPhysicalObject, world)
	{
		base.bodyChunks = new BodyChunk[1];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 5f, 0.07f);
		bodyChunkConnections = new BodyChunkConnection[0];
		if (abstractBullet.bulletType == JokeRifle.AbstractRifle.AmmoType.Void)
		{
			base.airFriction = 0.999f;
			base.waterFriction = 0.999f;
			surfaceFriction = 0.999f;
			base.gravity = 0.001f;
			bounce = 0.999f;
			base.buoyancy = 0.001f;
		}
		else
		{
			base.airFriction = 0.999f;
			if (abstractBullet.bulletType == JokeRifle.AbstractRifle.AmmoType.Light)
			{
				base.gravity = 0.5f;
			}
			else
			{
				base.gravity = 0.9f;
			}
			bounce = 0.4f;
			surfaceFriction = 0.4f;
			base.waterFriction = 0.98f;
			base.buoyancy = 0.4f;
		}
		collisionLayer = 2;
		base.firstChunk.loudness = 9f;
		tailPos = base.firstChunk.pos;
		soundLoop = new ChunkDynamicSoundLoop(base.firstChunk);
	}

	public override void HitWall()
	{
		if (room.BeingViewed)
		{
			for (int i = 0; i < 7; i++)
			{
				room.AddObject(new Spark(base.firstChunk.pos + throwDir.ToVector2() * (base.firstChunk.rad - 1f), Custom.DegToVec(Random.value * 360f) * 10f * Random.value + -throwDir.ToVector2() * 10f, new Color(1f, 1f, 1f), null, 2, 4));
			}
		}
		room.ScreenMovement(base.firstChunk.pos, throwDir.ToVector2() * 1.5f, 0f);
		if (abstractBullet.bulletType == JokeRifle.AbstractRifle.AmmoType.Fruit)
		{
			room.PlaySound(SoundID.Slime_Mold_Terrain_Impact, base.firstChunk);
		}
		else
		{
			room.PlaySound(SoundID.Rock_Hit_Wall, base.firstChunk);
		}
		SetRandomSpin();
		ChangeMode(Mode.Free);
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		base.forbiddenToPlayer = 10;
		soundLoop.sound = SoundID.None;
		abstractBullet.timeToLive--;
		if (abstractBullet.timeToLive <= 0)
		{
			if (abstractBullet.bulletType == JokeRifle.AbstractRifle.AmmoType.Ash)
			{
				CreatePuffExplosion();
			}
			if (abstractBullet.bulletType == JokeRifle.AbstractRifle.AmmoType.Light)
			{
				CreateFlashBang();
			}
			Destroy();
		}
		if (base.firstChunk.vel.magnitude > 5f)
		{
			if (base.firstChunk.ContactPoint.y < 0)
			{
				soundLoop.sound = SoundID.Rock_Skidding_On_Ground_LOOP;
			}
			else
			{
				soundLoop.sound = SoundID.Rock_Through_Air_LOOP;
			}
			soundLoop.Volume = Mathf.InverseLerp(5f, 15f, base.firstChunk.vel.magnitude);
		}
		soundLoop.Update();
		if (abstractBullet.bulletType == JokeRifle.AbstractRifle.AmmoType.Ash)
		{
			if (ashTrail == null)
			{
				ashTrail = new SporesSmoke(room);
			}
			else
			{
				ashTrail.Update(eu);
				if (room.ViewedByAnyCamera(base.firstChunk.pos, 300f))
				{
					ashTrail.EmitSmoke(base.firstChunk.pos, Custom.RNV(), new Color(0.9f, 1f, 0.8f));
				}
				if (ashTrail.Dead)
				{
					ashTrail = null;
				}
			}
		}
		else if (abstractBullet.bulletType == JokeRifle.AbstractRifle.AmmoType.Light)
		{
			if (light != null)
			{
				if (room.Darkness(base.firstChunk.pos) == 0f)
				{
					light.Destroy();
				}
				else
				{
					light.setPos = base.firstChunk.pos + base.firstChunk.vel;
					light.setAlpha = ((base.mode == Mode.Thrown) ? Mathf.Lerp(0.5f, 1f, Random.value) : 1f);
					if (base.mode == Mode.Thrown)
					{
						light.setRad = Mathf.Lerp(60f, 290f, Random.value);
					}
					else
					{
						light.setRad = 500f;
					}
					light.color = new Color(0.75f, 1f, 1f);
				}
				if (light.slatedForDeletetion || light.room != room)
				{
					light = null;
				}
			}
			else if (room.Darkness(base.firstChunk.pos) > 0f)
			{
				light = new LightSource(base.firstChunk.pos, environmentalLight: false, new Color(0.75f, 1f, 1f), this);
				room.AddObject(light);
			}
		}
		if (abstractBullet.bulletType == JokeRifle.AbstractRifle.AmmoType.Void)
		{
			if (base.mode != Mode.Thrown)
			{
				ChangeMode(Mode.Thrown);
			}
			exitThrownModeSpeed = 0f;
		}
		if (base.firstChunk.ContactPoint.y != 0)
		{
			rotationSpeed = (rotationSpeed * 2f + base.firstChunk.vel.x * 5f) / 3f;
		}
	}

	public void CreatePuffExplosion()
	{
		AbstractConsumable abstractConsumable = new AbstractConsumable(room.world, AbstractPhysicalObject.AbstractObjectType.PuffBall, null, room.GetWorldCoordinate(base.firstChunk.pos), room.game.GetNewID(), -1, -1, null);
		room.abstractRoom.AddEntity(abstractConsumable);
		abstractConsumable.RealizeInRoom();
		(abstractConsumable.realizedObject as PuffBall).firstChunk.pos = base.firstChunk.pos;
		(abstractConsumable.realizedObject as PuffBall).firstChunk.lastPos = base.firstChunk.lastPos;
		(abstractConsumable.realizedObject as PuffBall).Explode();
	}

	public void CreateExplosion()
	{
		AbstractPhysicalObject abstractPhysicalObject = new AbstractPhysicalObject(room.world, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null, room.GetWorldCoordinate(base.firstChunk.pos), room.game.GetNewID());
		room.abstractRoom.AddEntity(abstractPhysicalObject);
		abstractPhysicalObject.RealizeInRoom();
		(abstractPhysicalObject.realizedObject as ScavengerBomb).firstChunk.pos = base.firstChunk.pos;
		(abstractPhysicalObject.realizedObject as ScavengerBomb).firstChunk.lastPos = base.firstChunk.lastPos;
		(abstractPhysicalObject.realizedObject as ScavengerBomb).Explode(null);
	}

	public void CreateFlashBang()
	{
		AbstractConsumable abstractConsumable = new AbstractConsumable(room.world, AbstractPhysicalObject.AbstractObjectType.FlareBomb, null, room.GetWorldCoordinate(base.firstChunk.pos), room.game.GetNewID(), -1, -1, null);
		room.abstractRoom.AddEntity(abstractConsumable);
		abstractConsumable.RealizeInRoom();
		(abstractConsumable.realizedObject as FlareBomb).firstChunk.pos = base.firstChunk.pos;
		(abstractConsumable.realizedObject as FlareBomb).firstChunk.lastPos = base.firstChunk.lastPos;
		(abstractConsumable.realizedObject as FlareBomb).StartBurn();
	}

	public override bool HitSomething(SharedPhysics.CollisionResult result, bool eu)
	{
		if (result.obj == null)
		{
			return false;
		}
		if (result.obj is Player && abstractBullet.bulletType == JokeRifle.AbstractRifle.AmmoType.Void)
		{
			return false;
		}
		vibrate = 20;
		ChangeMode(Mode.Free);
		if (result.obj is Creature)
		{
			if (abstractBullet.bulletType == JokeRifle.AbstractRifle.AmmoType.Ash && !didFirstPuff)
			{
				CreatePuffExplosion();
				didFirstPuff = true;
			}
			if (abstractBullet.bulletType == JokeRifle.AbstractRifle.AmmoType.Grenade)
			{
				CreateExplosion();
				Destroy();
			}
			float num = 1f;
			float damage = 0.01f;
			float stunBonus = 45f;
			if (abstractBullet.bulletType == JokeRifle.AbstractRifle.AmmoType.Rock)
			{
				num = 7f;
				damage = 0.1f;
			}
			else if (abstractBullet.bulletType == JokeRifle.AbstractRifle.AmmoType.Fruit)
			{
				num = 15f;
				stunBonus = 150f;
			}
			if (abstractBullet.bulletType == JokeRifle.AbstractRifle.AmmoType.Void)
			{
				damage = 1f;
			}
			BodyChunk source = base.firstChunk;
			if (thrownBy != null)
			{
				source = thrownBy.firstChunk;
			}
			(result.obj as Creature).Violence(source, base.firstChunk.vel * base.firstChunk.mass * num, result.chunk, result.onAppendagePos, Creature.DamageType.Blunt, damage, stunBonus);
		}
		else if (result.chunk != null)
		{
			result.chunk.vel += base.firstChunk.vel * base.firstChunk.mass / result.chunk.mass;
		}
		else if (result.onAppendagePos != null)
		{
			(result.obj as IHaveAppendages).ApplyForceOnAppendage(result.onAppendagePos, base.firstChunk.vel * base.firstChunk.mass);
		}
		base.firstChunk.vel = base.firstChunk.vel * -0.5f + Custom.DegToVec(Random.value * 360f) * Mathf.Lerp(0.1f, 0.4f, Random.value) * base.firstChunk.vel.magnitude;
		if (abstractBullet.bulletType == JokeRifle.AbstractRifle.AmmoType.Void)
		{
			Destroy();
		}
		if (abstractBullet.bulletType == JokeRifle.AbstractRifle.AmmoType.Fruit)
		{
			room.PlaySound(SoundID.Red_Lizard_Spit_Hit_NPC, base.firstChunk);
		}
		else
		{
			room.PlaySound(SoundID.Rock_Hit_Creature, base.firstChunk);
		}
		if (result.chunk != null)
		{
			room.AddObject(new ExplosionSpikes(room, result.chunk.pos + Custom.DirVec(result.chunk.pos, result.collisionPoint) * result.chunk.rad, 5, 2f, 4f, 4.5f, 30f, new Color(1f, 1f, 1f, 0.5f)));
		}
		SetRandomSpin();
		return true;
	}

	public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
	{
		if (firstContact)
		{
			if (abstractBullet.bulletType == JokeRifle.AbstractRifle.AmmoType.Ash && !didFirstPuff)
			{
				CreatePuffExplosion();
				didFirstPuff = true;
			}
			if (abstractBullet.bulletType == JokeRifle.AbstractRifle.AmmoType.Grenade)
			{
				CreateExplosion();
				Destroy();
			}
			if (abstractBullet.bulletType == JokeRifle.AbstractRifle.AmmoType.Fruit)
			{
				room.PlaySound(SoundID.Slime_Mold_Terrain_Impact, base.firstChunk);
			}
		}
		if (abstractBullet.bulletType == JokeRifle.AbstractRifle.AmmoType.Void)
		{
			Destroy();
		}
		base.TerrainImpact(chunk, direction, speed, firstContact);
	}

	public override void PickedUp(Creature upPicker)
	{
		Destroy();
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[2];
		Random.State state = Random.state;
		Random.InitState(abstractPhysicalObject.ID.RandomSeed);
		if (abstractBullet.bulletType == JokeRifle.AbstractRifle.AmmoType.Void)
		{
			sLeaser.sprites[0] = new FSprite("FoodCircleA");
		}
		else if (abstractBullet.bulletType == JokeRifle.AbstractRifle.AmmoType.Fruit)
		{
			sLeaser.sprites[0] = new FSprite("DangleFruit0A");
		}
		else
		{
			sLeaser.sprites[0] = new FSprite("Circle4");
		}
		Random.state = state;
		TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[1]
		{
			new TriangleMesh.Triangle(0, 1, 2)
		};
		TriangleMesh triangleMesh = new TriangleMesh("Futile_White", tris, customColor: false);
		sLeaser.sprites[1] = triangleMesh;
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
		if (vibrate > 0)
		{
			vector += Custom.DegToVec(Random.value * 360f) * 2f * Random.value;
		}
		sLeaser.sprites[0].x = vector.x - camPos.x;
		sLeaser.sprites[0].y = vector.y - camPos.y;
		lastDarkness = darkness;
		darkness = rCam.room.Darkness(vector) * (1f - rCam.room.LightSourceExposure(vector));
		if (darkness != lastDarkness)
		{
			ApplyPalette(sLeaser, rCam, rCam.currentPalette);
		}
		Vector3 vector2 = Vector3.Slerp(lastRotation, rotation, timeStacker);
		sLeaser.sprites[0].rotation = Custom.AimFromOneVectorToAnother(new Vector2(0f, 0f), vector2);
		if (base.mode == Mode.Thrown && abstractBullet.bulletType != JokeRifle.AbstractRifle.AmmoType.Void)
		{
			sLeaser.sprites[1].isVisible = true;
			Vector2 vector3 = Vector2.Lerp(tailPos, base.firstChunk.lastPos, timeStacker);
			Vector2 vector4 = Custom.PerpendicularVector((vector - vector3).normalized);
			(sLeaser.sprites[1] as TriangleMesh).MoveVertice(0, vector + vector4 * 2f - camPos);
			(sLeaser.sprites[1] as TriangleMesh).MoveVertice(1, vector - vector4 * 2f - camPos);
			(sLeaser.sprites[1] as TriangleMesh).MoveVertice(2, vector3 - camPos);
		}
		else
		{
			sLeaser.sprites[1].isVisible = false;
		}
		if (blink > 0)
		{
			if (blink > 1 && Random.value < 0.5f)
			{
				sLeaser.sprites[0].color = new Color(1f, 1f, 1f);
			}
			else
			{
				sLeaser.sprites[0].color = color;
			}
		}
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		if (abstractBullet.bulletType == JokeRifle.AbstractRifle.AmmoType.Light)
		{
			color = Color.white;
		}
		else if (abstractBullet.bulletType == JokeRifle.AbstractRifle.AmmoType.Grenade)
		{
			color = Color.red;
		}
		else if (abstractBullet.bulletType == JokeRifle.AbstractRifle.AmmoType.Void)
		{
			color = Color.yellow;
		}
		else if (abstractBullet.bulletType == JokeRifle.AbstractRifle.AmmoType.Fruit)
		{
			color = Color.Lerp(new Color(0f, 0f, 1f), palette.blackColor, darkness);
		}
		else
		{
			color = palette.blackColor;
		}
		sLeaser.sprites[0].color = color;
		sLeaser.sprites[1].color = color;
	}
}
