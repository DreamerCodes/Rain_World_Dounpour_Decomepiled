using System;
using CoralBrain;
using Noise;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class SingularityBomb : Weapon, IOwnMycelia
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
			rotation = UnityEngine.Random.value * 360f;
			lastRotation = rotation;
			rotVel = Mathf.Lerp(-26f, 26f, UnityEngine.Random.value);
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
					vel.x = Mathf.Abs(vel.x) * Mathf.Lerp(0.15f, 0.7f, UnityEngine.Random.value);
					flag = true;
				}
				else if (floatRect.GetCorner(FloatRect.CornerLabel.B).x > 0f)
				{
					vel.x = (0f - Mathf.Abs(vel.x)) * Mathf.Lerp(0.15f, 0.7f, UnityEngine.Random.value);
					flag = true;
				}
				else if (floatRect.GetCorner(FloatRect.CornerLabel.B).y < 0f)
				{
					vel.y = Mathf.Abs(vel.y) * Mathf.Lerp(0.15f, 0.7f, UnityEngine.Random.value);
					flag = true;
				}
				else if (floatRect.GetCorner(FloatRect.CornerLabel.B).y > 0f)
				{
					vel.y = (0f - Mathf.Abs(vel.y)) * Mathf.Lerp(0.15f, 0.7f, UnityEngine.Random.value);
					flag = true;
				}
				if (flag)
				{
					rotVel *= 0.8f;
					rotVel += Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 4f * UnityEngine.Random.value;
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
			sLeaser.sprites[0] = new FSprite("SpearFragment" + (1 + UnityEngine.Random.Range(0, 2)));
			sLeaser.sprites[0].scaleX = ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f);
			sLeaser.sprites[0].scaleY = ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f) * 0.4f;
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

	public class SparkFlash : CosmeticSprite
	{
		public float size;

		public float life;

		public float lastLife;

		public float lifeTime;

		public Color col;

		public SparkFlash(Vector2 pos, float size)
		{
			base.pos = pos;
			lastPos = pos;
			this.size = size;
			life = 1f;
			lastLife = 1f;
			lifeTime = Mathf.Lerp(2f, 16f, size * UnityEngine.Random.value);
			col = new Color(0f, 1f, 0f);
		}

		public SparkFlash(Vector2 pos, float size, Color col)
		{
			base.pos = pos;
			lastPos = pos;
			this.size = size;
			life = 1f;
			lastLife = 1f;
			lifeTime = Mathf.Lerp(2f, 16f, size * UnityEngine.Random.value);
			this.col = col;
		}

		public override void Update(bool eu)
		{
			room.AddObject(new Spark(pos, Custom.RNV() * 60f * UnityEngine.Random.value, col, null, 4, 50));
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
			sLeaser.sprites[0].color = col;
			sLeaser.sprites[1] = new FSprite("Futile_White");
			sLeaser.sprites[1].shader = rCam.room.game.rainWorld.Shaders["FlatLight"];
			sLeaser.sprites[1].color = col;
			sLeaser.sprites[2] = new FSprite("Futile_White");
			sLeaser.sprites[2].shader = rCam.room.game.rainWorld.Shaders["FlareBomb"];
			sLeaser.sprites[2].color = col;
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

	public Color explodeColor;

	public bool ignited;

	public bool activateSingularity;

	public bool activateSucktion;

	public bool animate;

	public float counter;

	public float moveUp;

	public float singularityX;

	public float rad;

	public Vector2 rotpos1;

	public Vector2 rotpos2;

	public Vector2 floatLocation;

	public bool zeroMode;

	private Oracle scareOracle;

	private FirecrackerPlant.ScareObject scareObj;

	public CoralNeuronSystem neuronSystem;

	public Mycelium[] connections;

	public NSHSwarmer.Shape holoShape;

	public float[,] directionsPower;

	public float holoFade;

	public float holoTime;

	public float holoStart;

	public float holoEnd;

	public LightningMachine activateLightning;

	public Room OwnerRoom => room;

	public SingularityBomb(AbstractPhysicalObject abstractPhysicalObject, World world)
		: base(abstractPhysicalObject, world)
	{
		if (world.game.IsStorySession && world.game.GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel)
		{
			zeroMode = true;
		}
		if (zeroMode)
		{
			explodeColor = new Color(1f, 0.2f, 0.2f);
		}
		else
		{
			explodeColor = new Color(0.2f, 0.2f, 1f);
		}
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
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(abstractPhysicalObject.ID.RandomSeed);
		holoFade = 0f;
		if (zeroMode)
		{
			connections = new Mycelium[0];
			holoShape = null;
		}
		else
		{
			connections = new Mycelium[UnityEngine.Random.Range(8, 12)];
			for (int i = 0; i < connections.Length; i++)
			{
				connections[i] = new Mycelium(null, this, i, 20f, base.firstChunk.pos);
				connections[i].useStaticCulling = false;
				connections[i].color = new Color(0f, 0f, 0.1f);
			}
			holoShape = new NSHSwarmer.Shape(null, NSHSwarmer.Shape.ShapeType.Shell, new Vector3(0f, 0f, 0f), 15f, 15f);
		}
		directionsPower = new float[12, 3];
		UnityEngine.Random.state = state;
	}

	public override void Update(bool eu)
	{
		Creature creature = thrownBy;
		base.Update(eu);
		for (int i = 0; i < directionsPower.GetLength(0); i++)
		{
			directionsPower[i, 1] = directionsPower[i, 0];
			directionsPower[i, 0] = Custom.LerpAndTick(directionsPower[i, 0], directionsPower[i, 2], 0.03f, 1f / 15f);
			directionsPower[i, 2] = 0f;
		}
		if (base.mode == Mode.Thrown)
		{
			holoFade = 0f;
			ResetHoloDisplayTime();
		}
		else
		{
			holoTime += 1f;
			if (holoTime >= holoStart)
			{
				holoFade = Mathf.Lerp(holoFade, UnityEngine.Random.Range(0.2f, 1f), 0.1f);
			}
			else
			{
				holoFade = Mathf.Lerp(holoFade, 0f, 0.2f);
			}
			if (holoTime >= holoEnd)
			{
				ResetHoloDisplayTime();
			}
		}
		if (holoShape != null)
		{
			holoShape.Update(changeLikely: false, 0f, holoFade, base.firstChunk.pos - base.firstChunk.lastPos, 0f, ref directionsPower);
		}
		if (neuronSystem == null)
		{
			FirstLinkCoralSystem(new CoralNeuronSystem());
		}
		for (int j = 0; j < connections.Length; j++)
		{
			connections[j].Update();
			connections[j].points[1, 2] += Custom.DegToVec((float)j / (float)connections.Length * 360f);
		}
		thrownBy = creature;
		rotpos1 = new Vector2(Mathf.Sin(Mathf.Pow(counter, 2f) / 10f / 57.29578f) * (70f - counter / 100f * 70f), Mathf.Cos(Mathf.Pow(counter, 2f) / 15f / 57.29578f) * (70f - counter / 100f * 70f));
		rotpos2 = new Vector2((0f - Mathf.Sin(Mathf.Pow(counter, 2f) / 10f / 57.29578f)) * (120f - counter / 100f * 120f), (0f - Mathf.Cos(Mathf.Pow(counter, 2f) / 15f / 57.29578f)) * (120f - counter / 100f * 120f));
		if (activateSingularity)
		{
			if (scareOracle != null && scareOracle.oracleBehavior is SLOracleBehavior && (scareOracle.oracleBehavior as SLOracleBehavior).dangerousSingularity == null)
			{
				(scareOracle.oracleBehavior as SLOracleBehavior).dangerousSingularity = this;
				Custom.Log("Notified SL oracle of dangerous bomb in room!");
			}
			if (scareOracle != null && scareOracle.oracleBehavior is SSOracleBehavior && (scareOracle.oracleBehavior as SSOracleBehavior).dangerousSingularity == null)
			{
				(scareOracle.oracleBehavior as SSOracleBehavior).dangerousSingularity = this;
				(scareOracle.oracleBehavior as SSOracleBehavior).NewAction(MoreSlugcatsEnums.SSOracleBehaviorAction.ThrowOut_Singularity);
				Custom.Log("Notified SS oracle of dangerous bomb in room!");
			}
			if (counter < 1f)
			{
				base.gravity = 0f;
				base.firstChunk.vel = new Vector2(0f, 0f);
				singularityX = base.firstChunk.pos.x;
				moveUp = 8f;
			}
			counter += 1f;
			if (counter == 1f)
			{
				room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Singularity, base.firstChunk.pos, 1.5f, 0.55f + UnityEngine.Random.value * 0.2f);
			}
			if (moveUp > 0.01f)
			{
				moveUp = Mathf.Lerp(moveUp, 0f, 0.1f);
				base.firstChunk.pos += new Vector2(0f, moveUp);
			}
			if (counter > 40f)
			{
				activateSingularity = false;
				activateSucktion = true;
				floatLocation = base.firstChunk.pos;
				counter = 0f;
			}
		}
		if (activateSucktion)
		{
			counter += 1f;
			if (activateLightning == null && !zeroMode && counter < 100f)
			{
				activateLightning = new LightningMachine(base.firstChunk.pos, new Vector2(base.firstChunk.pos.x, base.firstChunk.pos.y), new Vector2(base.firstChunk.pos.x, base.firstChunk.pos.y + 10f), 0f, permanent: false, radial: true, 0.3f, 1f, 1f);
				activateLightning.volume = 0.8f;
				activateLightning.impactType = 3;
				activateLightning.lightningType = 0.65f;
				room.AddObject(activateLightning);
			}
			if (counter > 100f)
			{
				if (counter > 120f)
				{
					Destroy();
				}
				else
				{
					for (int k = 0; k < room.physicalObjects.Length; k++)
					{
						for (int l = 0; l < room.physicalObjects[k].Count; l++)
						{
							for (int m = 0; m < room.physicalObjects[k][l].bodyChunks.Length; m++)
							{
								BodyChunk bodyChunk = room.physicalObjects[k][l].bodyChunks[m];
								if (Vector2.Distance(base.firstChunk.pos, bodyChunk.pos) < 350f && bodyChunk != base.firstChunk)
								{
									bodyChunk.vel = new Vector2(0f, 0f);
								}
								if (room.physicalObjects[k][l] is SLOracleSwarmer && Custom.Dist(room.physicalObjects[k][l].firstChunk.pos, base.firstChunk.pos) < 350f)
								{
									Custom.Log("Killing oracle swarmers");
									(room.physicalObjects[k][l] as SLOracleSwarmer).ExplodeSwarmer();
								}
							}
						}
					}
					if (counter == 101f)
					{
						Explode();
						if (activateLightning != null)
						{
							activateLightning.Destroy();
							activateLightning = null;
						}
					}
				}
			}
			else
			{
				base.firstChunk.pos = floatLocation;
				base.firstChunk.vel = new Vector2(0f, 0f);
				for (int n = 0; n < room.physicalObjects.Length; n++)
				{
					for (int num = 0; num < room.physicalObjects[n].Count; num++)
					{
						for (int num2 = 0; num2 < room.physicalObjects[n][num].bodyChunks.Length; num2++)
						{
							BodyChunk bodyChunk2 = room.physicalObjects[n][num].bodyChunks[num2];
							if (Vector2.Distance(base.firstChunk.pos, bodyChunk2.pos) < 350f && bodyChunk2 != base.firstChunk)
							{
								bodyChunk2.vel += (base.firstChunk.pos - bodyChunk2.pos) * bodyChunk2.mass * counter / 100f;
								if (bodyChunk2.vel.magnitude > 50f)
								{
									bodyChunk2.vel = bodyChunk2.vel.normalized * 50f;
								}
							}
						}
					}
				}
				room.AddObject(new Explosion.ExplosionLight(base.firstChunk.pos, counter * 5f, 1f, 1, explodeColor));
				Vector2 vector = new Vector2(Mathf.Sin(Mathf.Atan2(rotpos1.y, rotpos1.x)), Mathf.Sin(Mathf.Atan2(rotpos1.y, rotpos1.x)));
				Vector2 vector2 = new Vector2(0f - Mathf.Sin(Mathf.Atan2(rotpos2.y, rotpos2.x)), 0f - Mathf.Sin(Mathf.Atan2(rotpos2.y, rotpos2.x)));
				room.AddObject(new Spark(base.firstChunk.pos + rotpos1 * 2f, -vector * 6f, explodeColor, null, 11, 28));
				room.AddObject(new Spark(base.firstChunk.pos + rotpos2 * 2f, -vector2 * 6f, explodeColor, null, 11, 28));
				room.AddObject(new ShockWave(base.firstChunk.pos, 3f * counter, 0.45f, 1));
			}
		}
		if (activateLightning != null)
		{
			float num3 = Mathf.Clamp(counter / 50f, 0.2f, 1f);
			activateLightning.startPoint = new Vector2(Mathf.Lerp(base.firstChunk.pos.x, 150f, num3 * 2f - 2f), base.firstChunk.pos.y);
			activateLightning.endPoint = new Vector2(Mathf.Lerp(base.firstChunk.pos.x, 150f, num3 * 2f - 2f), base.firstChunk.pos.y + 10f);
			activateLightning.chance = Mathf.Lerp(0f, 0.7f, num3);
		}
		soundLoop.sound = SoundID.None;
		if (base.mode == Mode.Free && collisionLayer != 1)
		{
			ChangeCollisionLayer(1);
		}
		else if (base.mode != Mode.Free && collisionLayer != 2)
		{
			ChangeCollisionLayer(0);
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
		if (scareObj != null)
		{
			scareObj.pos = base.firstChunk.pos;
		}
	}

	public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
	{
		base.TerrainImpact(chunk, direction, speed, firstContact);
		if (ignited && !activateSingularity && !activateSucktion)
		{
			activateSingularity = true;
			CreateFear();
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
		if (result.chunk != null)
		{
			result.chunk.vel += base.firstChunk.vel * base.firstChunk.mass / result.chunk.mass;
		}
		else if (result.onAppendagePos != null)
		{
			(result.obj as IHaveAppendages).ApplyForceOnAppendage(result.onAppendagePos, base.firstChunk.vel * base.firstChunk.mass);
		}
		if (!activateSingularity && !activateSucktion)
		{
			activateSingularity = true;
			CreateFear();
		}
		return true;
	}

	public override void Thrown(Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 throwDir, float frc, bool eu)
	{
		base.Thrown(thrownBy, thrownPos, firstFrameTraceFromPos, throwDir, frc, eu);
		room?.PlaySound(SoundID.Slugcat_Throw_Bomb, base.firstChunk);
		ignited = true;
		CreateFear();
	}

	public override void PickedUp(Creature upPicker)
	{
		room.PlaySound(SoundID.Slugcat_Pick_Up_Bomb, base.firstChunk);
	}

	public override void HitByWeapon(Weapon weapon)
	{
		base.HitByWeapon(weapon);
		if (!activateSingularity && !activateSucktion)
		{
			activateSingularity = true;
			CreateFear();
		}
	}

	public void Explode()
	{
		Custom.Log("SINGULARITY EXPLODE");
		if (thrownBy != null)
		{
			Custom.Log($"Thrower : {thrownBy.abstractCreature}");
		}
		else
		{
			Custom.Log("Thrower : NULL");
		}
		Vector2 vector = Vector2.Lerp(base.firstChunk.pos, base.firstChunk.lastPos, 0.35f);
		room.AddObject(new SparkFlash(base.firstChunk.pos, 300f, new Color(0f, 0f, 1f)));
		room.AddObject(new Explosion(room, this, vector, 7, 450f, 6.2f, 10f, 280f, 0.25f, thrownBy, 0.3f, 160f, 1f));
		room.AddObject(new Explosion(room, this, vector, 7, 2000f, 4f, 0f, 400f, 0.25f, thrownBy, 0.3f, 200f, 1f));
		room.AddObject(new Explosion.ExplosionLight(vector, 280f, 1f, 7, explodeColor));
		room.AddObject(new Explosion.ExplosionLight(vector, 230f, 1f, 3, new Color(1f, 1f, 1f)));
		room.AddObject(new Explosion.ExplosionLight(vector, 2000f, 2f, 60, explodeColor));
		room.AddObject(new ShockWave(vector, 350f, 0.485f, 300, highLayer: true));
		room.AddObject(new ShockWave(vector, 2000f, 0.185f, 180));
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
				room.AddObject(new Spark(vector + vector2 * Mathf.Lerp(30f, 60f, UnityEngine.Random.value), vector2 * Mathf.Lerp(7f, 38f, UnityEngine.Random.value) + Custom.RNV() * 20f * UnityEngine.Random.value, Color.Lerp(explodeColor, new Color(1f, 1f, 1f), UnityEngine.Random.value), null, 11, 28));
			}
			room.AddObject(new Explosion.FlashingSmoke(vector + vector2 * 40f * UnityEngine.Random.value, vector2 * Mathf.Lerp(4f, 20f, Mathf.Pow(UnityEngine.Random.value, 2f)), 1f + 0.05f * UnityEngine.Random.value, new Color(1f, 1f, 1f), explodeColor, UnityEngine.Random.Range(3, 11)));
		}
		for (int k = 0; k < 6; k++)
		{
			room.AddObject(new BombFragment(vector, Custom.DegToVec(((float)k + UnityEngine.Random.value) / 6f * 360f) * Mathf.Lerp(18f, 38f, UnityEngine.Random.value)));
		}
		room.ScreenMovement(vector, default(Vector2), 0.9f);
		for (int l = 0; l < abstractPhysicalObject.stuckObjects.Count; l++)
		{
			abstractPhysicalObject.stuckObjects[l].Deactivate();
		}
		room.PlaySound(SoundID.Bomb_Explode, vector);
		room.InGameNoise(new InGameNoise(vector, 9000f, this, 1f));
		for (int m = 0; m < room.physicalObjects.Length; m++)
		{
			for (int n = 0; n < room.physicalObjects[m].Count; n++)
			{
				if (room.physicalObjects[m][n] is Creature && Custom.Dist(room.physicalObjects[m][n].firstChunk.pos, base.firstChunk.pos) < 350f)
				{
					if (thrownBy != null)
					{
						(room.physicalObjects[m][n] as Creature).killTag = thrownBy.abstractCreature;
					}
					(room.physicalObjects[m][n] as Creature).Die();
				}
				if (room.physicalObjects[m][n] is ElectricSpear)
				{
					if ((room.physicalObjects[m][n] as ElectricSpear).abstractSpear.electricCharge == 0)
					{
						(room.physicalObjects[m][n] as ElectricSpear).Recharge();
					}
					else
					{
						(room.physicalObjects[m][n] as ElectricSpear).ExplosiveShortCircuit();
					}
				}
			}
		}
		CreateFear();
		scareObj.lifeTime = -600;
		scareObj.fearRange = 12000f;
		room.InGameNoise(new InGameNoise(base.firstChunk.pos, 12000f, this, 1f));
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		if (zeroMode)
		{
			sLeaser.sprites = new FSprite[2];
			sLeaser.sprites[1] = new FSprite("EZ0");
			sLeaser.sprites[0] = new FSprite("EZ1");
		}
		else
		{
			sLeaser.sprites = new FSprite[connections.Length + 5 + holoShape.LinesCount];
			for (int i = 0; i < connections.Length; i++)
			{
				connections[i].InitiateSprites(i, sLeaser, rCam);
			}
			TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[1]
			{
				new TriangleMesh.Triangle(0, 1, 2)
			};
			TriangleMesh triangleMesh = new TriangleMesh("Futile_White", tris, customColor: true);
			sLeaser.sprites[connections.Length] = triangleMesh;
			sLeaser.sprites[connections.Length + 1] = new FSprite("Circle20");
			sLeaser.sprites[connections.Length + 1].color = Custom.HSL2RGB(0.6638889f, 0.5f, 0.1f);
			sLeaser.sprites[connections.Length + 1].scale = 0.7f;
			sLeaser.sprites[connections.Length + 2] = new FSprite("Circle20");
			sLeaser.sprites[connections.Length + 2].color = Custom.HSL2RGB(0.6638889f, 1f, 0.35f);
			sLeaser.sprites[connections.Length + 2].scale = 0.3f;
			sLeaser.sprites[connections.Length + 3] = new FSprite("Circle20");
			sLeaser.sprites[connections.Length + 3].color = Custom.HSL2RGB(0.6638889f, 0.5f, 0.1f);
			sLeaser.sprites[connections.Length + 3].scale = 0.3f;
			sLeaser.sprites[connections.Length + 4] = new FSprite("Circle20");
			sLeaser.sprites[connections.Length + 4].scale = 0.15f;
			for (int num = holoShape.LinesCount - 1; num >= 0; num--)
			{
				sLeaser.sprites[connections.Length + 5 + num] = new FSprite("pixel");
				sLeaser.sprites[connections.Length + 5 + num].anchorY = 0f;
				sLeaser.sprites[connections.Length + 5 + num].shader = rCam.game.rainWorld.Shaders["HologramBehindTerrain"];
				sLeaser.sprites[connections.Length + 5 + num].color = Custom.HSL2RGB(0.6638889f, 1f, 0.35f);
			}
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 pointsVec = Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
		if (vibrate > 0)
		{
			pointsVec += Custom.DegToVec(UnityEngine.Random.value * 360f) * 2f * UnityEngine.Random.value;
		}
		Vector2 p = Vector3.Slerp(lastRotation, rotation, timeStacker);
		float num = Custom.VecToDeg(Vector3.Slerp(lastRotation, rotation, timeStacker));
		if (zeroMode)
		{
			for (int i = 0; i < sLeaser.sprites.Length; i++)
			{
				sLeaser.sprites[i].rotation = Custom.AimFromOneVectorToAnother(new Vector2(0f, 0f), p);
				sLeaser.sprites[i].x = pointsVec.x - camPos.x;
				sLeaser.sprites[i].y = pointsVec.y - camPos.y;
			}
		}
		if (!zeroMode)
		{
			if (base.mode == Mode.Thrown)
			{
				sLeaser.sprites[connections.Length].isVisible = true;
				Vector2 vector = Vector2.Lerp(tailPos, base.firstChunk.lastPos, timeStacker);
				Vector2 vector2 = Custom.PerpendicularVector((pointsVec - vector).normalized);
				(sLeaser.sprites[connections.Length] as TriangleMesh).MoveVertice(0, pointsVec + vector2 * 2f - camPos);
				(sLeaser.sprites[connections.Length] as TriangleMesh).MoveVertice(1, pointsVec - vector2 * 2f - camPos);
				(sLeaser.sprites[connections.Length] as TriangleMesh).MoveVertice(2, vector - camPos);
				float r = UnityEngine.Random.Range(0f, 0.7f);
				Color color = Color.Lerp(base.color, new Color(r, r, UnityEngine.Random.Range(0.4f, 1f)), 0.4f);
				(sLeaser.sprites[connections.Length] as TriangleMesh).verticeColors[0] = color;
				(sLeaser.sprites[connections.Length] as TriangleMesh).verticeColors[1] = color;
				(sLeaser.sprites[connections.Length] as TriangleMesh).verticeColors[2] = explodeColor;
			}
			else
			{
				sLeaser.sprites[connections.Length].isVisible = false;
			}
		}
		if (blink > 0)
		{
			if (blink > 1 && UnityEngine.Random.value < 0.5f)
			{
				UpdateColor(sLeaser, new Color(1f, 1f, 1f));
			}
			else
			{
				UpdateColor(sLeaser, base.color);
			}
		}
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
		if (UnityEngine.Random.Range(0f, 1f) < 0.01f)
		{
			animate = !animate;
		}
		if (animate && rad > 15f)
		{
			rad -= 0.7f;
		}
		if (!animate && rad < 25f)
		{
			rad += 0.7f;
		}
		if (!zeroMode)
		{
			float f = 1f;
			f = Mathf.Lerp(0.2f, 1f, Mathf.Abs(f)) * Mathf.Sign(f);
			sLeaser.sprites[connections.Length + 1].x = pointsVec.x - camPos.x;
			sLeaser.sprites[connections.Length + 1].y = pointsVec.y - camPos.y;
			sLeaser.sprites[connections.Length + 1].rotation = num;
			sLeaser.sprites[connections.Length + 1].scaleX = 0.7f * f;
			sLeaser.sprites[connections.Length + 2].x = pointsVec.x - camPos.x - 0.75f - 1.5f * Mathf.Abs(f);
			sLeaser.sprites[connections.Length + 2].y = pointsVec.y - camPos.y + 0.75f + 1.5f * Mathf.Abs(f);
			sLeaser.sprites[connections.Length + 2].rotation = num;
			sLeaser.sprites[connections.Length + 2].scaleX = 0.3f * f;
			sLeaser.sprites[connections.Length + 3].x = pointsVec.x - camPos.x - 0.85f;
			sLeaser.sprites[connections.Length + 3].y = pointsVec.y - camPos.y + 0.85f;
			sLeaser.sprites[connections.Length + 3].rotation = num;
			sLeaser.sprites[connections.Length + 3].scaleX = 0.3f * f;
			sLeaser.sprites[connections.Length + 4].x = pointsVec.x - camPos.x - 0.75f;
			sLeaser.sprites[connections.Length + 4].y = pointsVec.y - camPos.y + 0.75f;
			sLeaser.sprites[connections.Length + 4].rotation = num;
			sLeaser.sprites[connections.Length + 4].scaleX = 0.15f * f;
			Color color2 = Custom.HSL2RGB(UnityEngine.Random.Range(0.55f, 0.7f), UnityEngine.Random.Range(0.8f, 1f), UnityEngine.Random.Range(0.3f, 0.6f));
			sLeaser.sprites[connections.Length + 4].color = color2;
			float fade = holoFade;
			float pointsWeight = 1f;
			float maxDist = 40f;
			int sprite = connections.Length + 5;
			for (int num2 = holoShape.LinesCount - 1; num2 >= 0; num2--)
			{
				sLeaser.sprites[connections.Length + 5 + num2].color = color2;
			}
			holoShape.Draw(sLeaser, rCam, timeStacker, pointsVec, camPos, ref sprite, 0f, 0f, fade, shakeErr: false, ref pointsVec, ref pointsWeight, ref maxDist, ref directionsPower);
		}
		for (int j = 0; j < connections.Length; j++)
		{
			connections[j].DrawSprites(j, sLeaser, rCam, timeStacker, camPos);
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		if (zeroMode)
		{
			color = new Color(0.6196f, 0.1098f, 0.1255f);
		}
		else
		{
			color = palette.blackColor;
		}
		UpdateColor(sLeaser, color);
	}

	public void UpdateColor(RoomCamera.SpriteLeaser sLeaser, Color col)
	{
		if (zeroMode)
		{
			sLeaser.sprites[connections.Length + 1].color = Color.white;
			sLeaser.sprites[connections.Length].color = col;
		}
	}

	public override void NewRoom(Room newRoom)
	{
		base.NewRoom(newRoom);
		scareOracle = null;
		foreach (PhysicalObject item in room.physicalObjects[1])
		{
			if (item is Oracle)
			{
				scareOracle = item as Oracle;
				break;
			}
		}
		for (int i = 0; i < connections.Length; i++)
		{
			connections[i].Reset(base.firstChunk.pos);
		}
		UpdateCoralNeuronSystem();
	}

	public void CreateFear()
	{
		if (scareObj == null)
		{
			scareObj = new FirecrackerPlant.ScareObject(base.firstChunk.pos);
			scareObj.fearRange = 8000f;
			scareObj.fearScavs = true;
			room.AddObject(scareObj);
			room.InGameNoise(new InGameNoise(base.firstChunk.pos, 8000f, this, 1f));
		}
	}

	public Vector2 ConnectionPos(int index, float timeStacker)
	{
		return base.firstChunk.pos;
	}

	public Vector2 ResetDir(int index)
	{
		return default(Vector2);
	}

	private void UpdateCoralNeuronSystem()
	{
		if (neuronSystem != null && base.graphicsModule != null)
		{
			for (int i = 0; i < connections.Length; i++)
			{
				neuronSystem.mycelia.Remove(connections[i]);
			}
		}
		neuronSystem = null;
		for (int num = room.updateList.Count - 1; num >= 0; num--)
		{
			if (room.updateList[num] is CoralNeuronSystem)
			{
				FirstLinkCoralSystem(room.updateList[num] as CoralNeuronSystem);
				break;
			}
		}
		if (base.graphicsModule != null)
		{
			(base.graphicsModule as InspectorGraphics).UpdateNeuronSystemForMycelia();
		}
	}

	public void FirstLinkCoralSystem(CoralNeuronSystem system)
	{
		neuronSystem = system;
		if (zeroMode)
		{
			return;
		}
		for (int i = 0; i < connections.Length; i++)
		{
			if (connections[i].system == null)
			{
				connections[i].ConnectSystem(neuronSystem);
			}
		}
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Items");
		}
		if (zeroMode)
		{
			for (int num = sLeaser.sprites.Length - 1; num >= 0; num--)
			{
				sLeaser.sprites[num].RemoveFromContainer();
				newContatiner.AddChild(sLeaser.sprites[num]);
			}
		}
		else
		{
			for (int i = 0; i < sLeaser.sprites.Length; i++)
			{
				sLeaser.sprites[i].RemoveFromContainer();
				newContatiner.AddChild(sLeaser.sprites[i]);
			}
		}
	}

	public void ResetHoloDisplayTime()
	{
		holoTime = 0f;
		holoStart = UnityEngine.Random.Range(20f, 400f);
		holoEnd = holoStart + UnityEngine.Random.Range(80f, 600f);
	}
}
