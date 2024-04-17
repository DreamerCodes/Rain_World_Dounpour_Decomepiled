using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class Rock : Weapon
{
	public override bool HeavyWeapon => true;

	public Rock(AbstractPhysicalObject abstractPhysicalObject, World world)
		: base(abstractPhysicalObject, world)
	{
		base.bodyChunks = new BodyChunk[1];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 5f, 0.07f);
		bodyChunkConnections = new BodyChunkConnection[0];
		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		bounce = 0.4f;
		surfaceFriction = 0.4f;
		collisionLayer = 2;
		base.waterFriction = 0.98f;
		base.buoyancy = 0.4f;
		base.firstChunk.loudness = 9f;
		tailPos = base.firstChunk.pos;
		soundLoop = new ChunkDynamicSoundLoop(base.firstChunk);
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		soundLoop.sound = SoundID.None;
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
		if (base.firstChunk.ContactPoint.y != 0)
		{
			rotationSpeed = (rotationSpeed * 2f + base.firstChunk.vel.x * 5f) / 3f;
		}
	}

	public override bool HitSomething(SharedPhysics.CollisionResult result, bool eu)
	{
		if (result.obj == null)
		{
			return false;
		}
		if (thrownBy is Scavenger && (thrownBy as Scavenger).AI != null)
		{
			(thrownBy as Scavenger).AI.HitAnObjectWithWeapon(this, result.obj);
		}
		vibrate = 20;
		ChangeMode(Mode.Free);
		if (result.obj is Creature)
		{
			float stunBonus = 45f;
			if (ModManager.MMF && MMF.cfgIncreaseStuns.Value && (result.obj is Cicada || result.obj is LanternMouse || (ModManager.MSC && result.obj is Yeek)))
			{
				stunBonus = 90f;
			}
			if (ModManager.MSC && room.game.IsArenaSession && room.game.GetArenaGameSession.chMeta != null)
			{
				stunBonus = 90f;
			}
			(result.obj as Creature).Violence(base.firstChunk, base.firstChunk.vel * base.firstChunk.mass, result.chunk, result.onAppendagePos, Creature.DamageType.Blunt, 0.01f, stunBonus);
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
		room.PlaySound(SoundID.Rock_Hit_Creature, base.firstChunk);
		if (result.chunk != null)
		{
			room.AddObject(new ExplosionSpikes(room, result.chunk.pos + Custom.DirVec(result.chunk.pos, result.collisionPoint) * result.chunk.rad, 5, 2f, 4f, 4.5f, 30f, new Color(1f, 1f, 1f, 0.5f)));
		}
		SetRandomSpin();
		return true;
	}

	public override void Thrown(Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 throwDir, float frc, bool eu)
	{
		base.Thrown(thrownBy, thrownPos, firstFrameTraceFromPos, throwDir, frc, eu);
		room?.PlaySound(SoundID.Slugcat_Throw_Rock, base.firstChunk);
	}

	public override void PickedUp(Creature upPicker)
	{
		room.PlaySound(SoundID.Slugcat_Pick_Up_Rock, base.firstChunk);
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[2];
		Random.State state = Random.state;
		Random.InitState(abstractPhysicalObject.ID.RandomSeed);
		sLeaser.sprites[0] = new FSprite("Pebble" + Random.Range(1, 15));
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
		Vector3 vector2 = Vector3.Slerp(lastRotation, rotation, timeStacker);
		sLeaser.sprites[0].rotation = Custom.AimFromOneVectorToAnother(new Vector2(0f, 0f), vector2);
		if (base.mode == Mode.Thrown)
		{
			sLeaser.sprites[1].isVisible = true;
			Vector2 vector3 = Vector2.Lerp(tailPos, base.firstChunk.lastPos, timeStacker);
			Vector2 vector4 = Custom.PerpendicularVector((vector - vector3).normalized);
			(sLeaser.sprites[1] as TriangleMesh).MoveVertice(0, vector + vector4 * 3f - camPos);
			(sLeaser.sprites[1] as TriangleMesh).MoveVertice(1, vector - vector4 * 3f - camPos);
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
				sLeaser.sprites[0].color = base.blinkColor;
			}
			else
			{
				sLeaser.sprites[0].color = color;
			}
		}
		else if (sLeaser.sprites[0].color != color)
		{
			sLeaser.sprites[0].color = color;
		}
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		color = palette.blackColor;
		sLeaser.sprites[0].color = color;
		sLeaser.sprites[1].color = color;
	}
}
