using MoreSlugcats;
using Noise;
using RWCustom;
using Smoke;
using UnityEngine;

public class ScavengerBomb : Weapon, IProvideWarmth
{
	public class BombFragment : CosmeticSprite
	{
		public float rotation;

		public float lastRotation;

		public float rotVel;

		public BombFragment(Vector2 pos, Vector2 vel)
		{
			base.pos = pos + vel * 2f;
			lastPos = pos;
			base.vel = vel;
			rotation = Random.value * 360f;
			lastRotation = rotation;
			rotVel = Mathf.Lerp(-26f, 26f, Random.value);
		}

		public override void Update(bool eu)
		{
			vel *= 0.998f;
			vel.y -= room.gravity * 0.9f;
			lastRotation = rotation;
			rotation += rotVel * vel.magnitude;
			if (Vector2.Distance(lastPos, pos) > 18f && room.GetTile(pos).Solid && !room.GetTile(lastPos).Solid)
			{
				IntVector2? intVector = SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(room, room.GetTilePosition(lastPos), room.GetTilePosition(pos));
				FloatRect floatRect = Custom.RectCollision(pos, lastPos, room.TileRect(intVector.Value).Grow(2f));
				pos = floatRect.GetCorner(FloatRect.CornerLabel.D);
				bool flag = false;
				if (floatRect.GetCorner(FloatRect.CornerLabel.B).x < 0f)
				{
					vel.x = Mathf.Abs(vel.x) * Mathf.Lerp(0.15f, 0.7f, Random.value);
					flag = true;
				}
				else if (floatRect.GetCorner(FloatRect.CornerLabel.B).x > 0f)
				{
					vel.x = (0f - Mathf.Abs(vel.x)) * Mathf.Lerp(0.15f, 0.7f, Random.value);
					flag = true;
				}
				else if (floatRect.GetCorner(FloatRect.CornerLabel.B).y < 0f)
				{
					vel.y = Mathf.Abs(vel.y) * Mathf.Lerp(0.15f, 0.7f, Random.value);
					flag = true;
				}
				else if (floatRect.GetCorner(FloatRect.CornerLabel.B).y > 0f)
				{
					vel.y = (0f - Mathf.Abs(vel.y)) * Mathf.Lerp(0.15f, 0.7f, Random.value);
					flag = true;
				}
				if (flag)
				{
					rotVel *= 0.8f;
					rotVel += Mathf.Lerp(-1f, 1f, Random.value) * 4f * Random.value;
				}
			}
			if ((room.GetTile(pos).Solid && room.GetTile(lastPos).Solid) || pos.y < -100f)
			{
				Destroy();
			}
			base.Update(eu);
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite("SpearFragment" + (1 + Random.Range(0, 2)));
			sLeaser.sprites[0].scaleX = ((Random.value < 0.5f) ? (-1f) : 1f);
			sLeaser.sprites[0].scaleY = ((Random.value < 0.5f) ? (-1f) : 1f) * 0.4f;
			AddToContainer(sLeaser, rCam, null);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
			sLeaser.sprites[0].x = vector.x - camPos.x;
			sLeaser.sprites[0].y = vector.y - camPos.y;
			sLeaser.sprites[0].rotation = Mathf.Lerp(lastRotation, rotation, timeStacker);
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}

		public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			sLeaser.sprites[0].color = palette.blackColor;
		}

		public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			base.AddToContainer(sLeaser, rCam, newContatiner);
		}
	}

	public Color explodeColor = new Color(1f, 0.4f, 0.3f);

	public bool ignited;

	public float[] spikes;

	public BombSmoke smoke;

	private float burn;

	public bool explosionIsForShow;

	public override bool HeavyWeapon => true;

	float IProvideWarmth.warmth => RainWorldGame.DefaultHeatSourceWarmth * 0.15f;

	Room IProvideWarmth.loadedRoom => room;

	float IProvideWarmth.range => 40f;

	public void InitiateBurn()
	{
		if (burn == 0f)
		{
			burn = Random.value;
			room.PlaySound(SoundID.Fire_Spear_Ignite, base.firstChunk, loop: false, 0.5f, 1.4f);
			base.firstChunk.vel += Custom.RNV() * Random.value * 6f;
		}
		else
		{
			burn = Mathf.Min(burn, Random.value);
		}
	}

	public ScavengerBomb(AbstractPhysicalObject abstractPhysicalObject, World world)
		: base(abstractPhysicalObject, world)
	{
		base.bodyChunks = new BodyChunk[1];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 5.5f, 0.2f);
		bodyChunkConnections = new BodyChunkConnection[0];
		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		bounce = 0.4f;
		surfaceFriction = 0.4f;
		collisionLayer = 1;
		base.waterFriction = 0.98f;
		base.buoyancy = 0.4f;
		base.firstChunk.loudness = 4f;
		tailPos = base.firstChunk.pos;
		soundLoop = new ChunkDynamicSoundLoop(base.firstChunk);
		Random.State state = Random.state;
		Random.InitState(abstractPhysicalObject.ID.RandomSeed);
		spikes = new float[Random.Range(3, 8)];
		for (int i = 0; i < spikes.Length; i++)
		{
			spikes[i] = ((float)i + Random.value) * (360f / (float)spikes.Length);
		}
		Random.state = state;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		soundLoop.sound = SoundID.None;
		if (base.mode == Mode.Free && collisionLayer != 1)
		{
			ChangeCollisionLayer(1);
		}
		else if (base.mode != Mode.Free && collisionLayer != 2)
		{
			ChangeCollisionLayer(2);
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
		if (base.firstChunk.ContactPoint.y != 0)
		{
			rotationSpeed = (rotationSpeed * 2f + base.firstChunk.vel.x * 5f) / 3f;
		}
		if (base.Submersion >= 0.2f && room.waterObject.WaterIsLethal && burn == 0f)
		{
			ignited = true;
			base.buoyancy = 0.9f;
			base.firstChunk.vel *= 0.2f;
			burn = 0.8f + Random.value * 0.2f;
		}
		if (ignited || burn > 0f)
		{
			if (base.Submersion == 1f && !room.waterObject.WaterIsLethal)
			{
				ignited = false;
				burn = 0f;
			}
			if (ignited && burn == 0f && base.mode != Mode.Thrown)
			{
				burn = 0.5f + Random.value * 0.5f;
			}
			for (int i = 0; i < 3; i++)
			{
				room.AddObject(new Spark(Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, Random.value), base.firstChunk.vel * 0.1f + Custom.RNV() * 3.2f * Random.value, explodeColor, null, 7, 30));
			}
			if (smoke == null)
			{
				smoke = new BombSmoke(room, base.firstChunk.pos, base.firstChunk, explodeColor);
				room.AddObject(smoke);
			}
		}
		else
		{
			if (smoke != null)
			{
				smoke.Destroy();
			}
			smoke = null;
		}
		if (burn > 0f)
		{
			burn -= 1f / 30f;
			if (burn <= 0f)
			{
				Explode(null);
			}
		}
	}

	public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
	{
		base.TerrainImpact(chunk, direction, speed, firstContact);
		if ((floorBounceFrames <= 0 || (direction.x != 0 && room.GetTile(base.firstChunk.pos).Terrain != Room.Tile.TerrainType.Slope)) && ignited)
		{
			Explode(null);
		}
	}

	public override bool HitSomething(SharedPhysics.CollisionResult result, bool eu)
	{
		if (result.obj == null)
		{
			return false;
		}
		vibrate = 20;
		ChangeMode(Mode.Free);
		if (result.obj is Creature)
		{
			(result.obj as Creature).Violence(base.firstChunk, base.firstChunk.vel * base.firstChunk.mass, result.chunk, result.onAppendagePos, Creature.DamageType.Explosion, 0.8f, 85f);
		}
		else if (result.chunk != null)
		{
			result.chunk.vel += base.firstChunk.vel * base.firstChunk.mass / result.chunk.mass;
		}
		else if (result.onAppendagePos != null)
		{
			(result.obj as IHaveAppendages).ApplyForceOnAppendage(result.onAppendagePos, base.firstChunk.vel * base.firstChunk.mass);
		}
		Explode(result.chunk);
		return true;
	}

	public override void Thrown(Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 throwDir, float frc, bool eu)
	{
		base.Thrown(thrownBy, thrownPos, firstFrameTraceFromPos, throwDir, frc, eu);
		room?.PlaySound(SoundID.Slugcat_Throw_Bomb, base.firstChunk);
		ignited = true;
	}

	public override void PickedUp(Creature upPicker)
	{
		room.PlaySound(SoundID.Slugcat_Pick_Up_Bomb, base.firstChunk);
	}

	public override void HitByWeapon(Weapon weapon)
	{
		if (weapon.mode == Mode.Thrown && thrownBy == null && weapon.thrownBy != null)
		{
			thrownBy = weapon.thrownBy;
		}
		base.HitByWeapon(weapon);
		InitiateBurn();
	}

	public override void WeaponDeflect(Vector2 inbetweenPos, Vector2 deflectDir, float bounceSpeed)
	{
		base.WeaponDeflect(inbetweenPos, deflectDir, bounceSpeed);
		if (Random.value < 0.5f)
		{
			Explode(null);
			return;
		}
		ignited = true;
		InitiateBurn();
	}

	public override void HitByExplosion(float hitFac, Explosion explosion, int hitChunk)
	{
		base.HitByExplosion(hitFac, explosion, hitChunk);
		if (Random.value < hitFac)
		{
			if (thrownBy == null)
			{
				thrownBy = explosion.killTagHolder;
			}
			InitiateBurn();
		}
	}

	public void Explode(BodyChunk hitChunk)
	{
		if (base.slatedForDeletetion)
		{
			return;
		}
		Vector2 vector = Vector2.Lerp(base.firstChunk.pos, base.firstChunk.lastPos, 0.35f);
		room.AddObject(new SootMark(room, vector, 80f, bigSprite: true));
		if (!explosionIsForShow)
		{
			room.AddObject(new Explosion(room, this, vector, 7, 250f, 6.2f, 2f, 280f, 0.25f, thrownBy, 0.7f, 160f, 1f));
		}
		room.AddObject(new Explosion.ExplosionLight(vector, 280f, 1f, 7, explodeColor));
		room.AddObject(new Explosion.ExplosionLight(vector, 230f, 1f, 3, new Color(1f, 1f, 1f)));
		room.AddObject(new ExplosionSpikes(room, vector, 14, 30f, 9f, 7f, 170f, explodeColor));
		room.AddObject(new ShockWave(vector, 330f, 0.045f, 5));
		for (int i = 0; i < 25; i++)
		{
			Vector2 vector2 = Custom.RNV();
			if (room.GetTile(vector + vector2 * 20f).Solid)
			{
				if (!room.GetTile(vector - vector2 * 20f).Solid)
				{
					vector2 *= -1f;
				}
				else
				{
					vector2 = Custom.RNV();
				}
			}
			for (int j = 0; j < 3; j++)
			{
				room.AddObject(new Spark(vector + vector2 * Mathf.Lerp(30f, 60f, Random.value), vector2 * Mathf.Lerp(7f, 38f, Random.value) + Custom.RNV() * 20f * Random.value, Color.Lerp(explodeColor, new Color(1f, 1f, 1f), Random.value), null, 11, 28));
			}
			room.AddObject(new Explosion.FlashingSmoke(vector + vector2 * 40f * Random.value, vector2 * Mathf.Lerp(4f, 20f, Mathf.Pow(Random.value, 2f)), 1f + 0.05f * Random.value, new Color(1f, 1f, 1f), explodeColor, Random.Range(3, 11)));
		}
		if (smoke != null)
		{
			for (int k = 0; k < 8; k++)
			{
				smoke.EmitWithMyLifeTime(vector + Custom.RNV(), Custom.RNV() * Random.value * 17f);
			}
		}
		for (int l = 0; l < 6; l++)
		{
			room.AddObject(new BombFragment(vector, Custom.DegToVec(((float)l + Random.value) / 6f * 360f) * Mathf.Lerp(18f, 38f, Random.value)));
		}
		room.ScreenMovement(vector, default(Vector2), 1.3f);
		for (int m = 0; m < abstractPhysicalObject.stuckObjects.Count; m++)
		{
			abstractPhysicalObject.stuckObjects[m].Deactivate();
		}
		room.PlaySound(SoundID.Bomb_Explode, vector);
		room.InGameNoise(new InGameNoise(vector, 9000f, this, 1f));
		bool flag = hitChunk != null;
		for (int n = 0; n < 5; n++)
		{
			if (room.GetTile(vector + Custom.fourDirectionsAndZero[n].ToVector2() * 20f).Solid)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			if (smoke == null)
			{
				smoke = new BombSmoke(room, vector, null, explodeColor);
				room.AddObject(smoke);
			}
			if (hitChunk != null)
			{
				smoke.chunk = hitChunk;
			}
			else
			{
				smoke.chunk = null;
				smoke.fadeIn = 1f;
			}
			smoke.pos = vector;
			smoke.stationary = true;
			smoke.DisconnectSmoke();
		}
		else if (smoke != null)
		{
			smoke.Destroy();
		}
		Destroy();
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[spikes.Length + 4];
		Random.State state = Random.state;
		Random.InitState(abstractPhysicalObject.ID.RandomSeed);
		for (int i = 0; i < 2; i++)
		{
			sLeaser.sprites[i] = new FSprite("pixel");
			sLeaser.sprites[i].scaleX = Mathf.Lerp(1f, 2f, Mathf.Pow(Random.value, 1.8f));
			sLeaser.sprites[i].scaleY = Mathf.Lerp(4f, 7f, Random.value);
		}
		for (int j = 0; j < spikes.Length; j++)
		{
			sLeaser.sprites[2 + j] = new FSprite("pixel");
			sLeaser.sprites[2 + j].scaleX = Mathf.Lerp(1.5f, 3f, Random.value);
			sLeaser.sprites[2 + j].scaleY = Mathf.Lerp(5f, 7f, Random.value);
			sLeaser.sprites[2 + j].anchorY = 0f;
		}
		sLeaser.sprites[spikes.Length + 2] = new FSprite("Futile_White");
		sLeaser.sprites[spikes.Length + 2].shader = rCam.game.rainWorld.Shaders["JaggedCircle"];
		sLeaser.sprites[spikes.Length + 2].scale = (base.firstChunk.rad + 0.75f) / 10f;
		Random.state = state;
		sLeaser.sprites[spikes.Length + 2].alpha = Mathf.Lerp(0.2f, 0.4f, Random.value);
		TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[1]
		{
			new TriangleMesh.Triangle(0, 1, 2)
		};
		TriangleMesh triangleMesh = new TriangleMesh("Futile_White", tris, customColor: true);
		sLeaser.sprites[spikes.Length + 3] = triangleMesh;
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
		if (vibrate > 0)
		{
			vector += Custom.DegToVec(Random.value * 360f) * 2f * Random.value;
		}
		Vector2 vector2 = Vector3.Slerp(lastRotation, rotation, timeStacker);
		sLeaser.sprites[2].rotation = Custom.AimFromOneVectorToAnother(new Vector2(0f, 0f), vector2);
		sLeaser.sprites[spikes.Length + 2].x = vector.x - camPos.x;
		sLeaser.sprites[spikes.Length + 2].y = vector.y - camPos.y;
		for (int i = 0; i < spikes.Length; i++)
		{
			sLeaser.sprites[2 + i].x = vector.x - camPos.x;
			sLeaser.sprites[2 + i].y = vector.y - camPos.y;
			sLeaser.sprites[2 + i].rotation = Custom.VecToDeg(vector2) + spikes[i];
		}
		Color a = Color.Lerp(explodeColor, new Color(1f, 0f, 0f), 0.5f + 0.2f * Mathf.Pow(Random.value, 0.2f));
		a = Color.Lerp(a, new Color(1f, 1f, 1f), Mathf.Pow(Random.value, ignited ? 3f : 30f));
		for (int j = 0; j < 2; j++)
		{
			sLeaser.sprites[j].x = vector.x - camPos.x;
			sLeaser.sprites[j].y = vector.y - camPos.y;
			sLeaser.sprites[j].rotation = Custom.VecToDeg(vector2) + (float)j * 90f;
			sLeaser.sprites[j].color = a;
		}
		if (base.mode == Mode.Thrown)
		{
			sLeaser.sprites[spikes.Length + 3].isVisible = true;
			Vector2 vector3 = Vector2.Lerp(tailPos, base.firstChunk.lastPos, timeStacker);
			Vector2 vector4 = Custom.PerpendicularVector((vector - vector3).normalized);
			(sLeaser.sprites[spikes.Length + 3] as TriangleMesh).MoveVertice(0, vector + vector4 * 2f - camPos);
			(sLeaser.sprites[spikes.Length + 3] as TriangleMesh).MoveVertice(1, vector - vector4 * 2f - camPos);
			(sLeaser.sprites[spikes.Length + 3] as TriangleMesh).MoveVertice(2, vector3 - camPos);
			(sLeaser.sprites[spikes.Length + 3] as TriangleMesh).verticeColors[0] = color;
			(sLeaser.sprites[spikes.Length + 3] as TriangleMesh).verticeColors[1] = color;
			(sLeaser.sprites[spikes.Length + 3] as TriangleMesh).verticeColors[2] = explodeColor;
		}
		else
		{
			sLeaser.sprites[spikes.Length + 3].isVisible = false;
		}
		if (blink > 0)
		{
			if (blink > 1 && Random.value < 0.5f)
			{
				UpdateColor(sLeaser, base.blinkColor);
			}
			else
			{
				UpdateColor(sLeaser, color);
			}
		}
		else if (sLeaser.sprites[spikes.Length + 2].color != color)
		{
			UpdateColor(sLeaser, color);
		}
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		color = palette.blackColor;
		UpdateColor(sLeaser, color);
	}

	public void UpdateColor(RoomCamera.SpriteLeaser sLeaser, Color col)
	{
		sLeaser.sprites[spikes.Length + 2].color = col;
		for (int i = 0; i < spikes.Length; i++)
		{
			sLeaser.sprites[2 + i].color = col;
		}
	}

	Vector2 IProvideWarmth.Position()
	{
		return base.firstChunk.pos;
	}
}
