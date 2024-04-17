using System;
using Noise;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class EnergyCell : PhysicalObject, IDrawable
{
	public Color color;

	public float scale;

	public float roll;

	private Vector2 velocity;

	private new float gravity;

	private int stage;

	private Vector2 target;

	public float moveToTarget;

	public float FXCounter;

	private LightSource halo;

	public LightningMachine lightningMachine;

	public Vector3 Hsl;

	public bool customAnimation;

	public float recharging;

	public float usingTime;

	public float chargeTime;

	public Color explodeColor;

	private bool touchedGround;

	private bool isStabilized;

	public bool allowStabilization;

	public float chargeDuration => 20f;

	public bool allowPickup => stage == -1;

	public EnergyCell(AbstractPhysicalObject abstractPhysicalObject)
		: base(abstractPhysicalObject)
	{
		stage = -1;
		Hsl = new Vector3(0.61f, 1f, 0.51f);
		color = Custom.HSL2RGB(Hsl.x - 0.0001f, Hsl.y, Hsl.z);
		collisionLayer = 1;
		scale = 10f;
		base.bodyChunks = new BodyChunk[1];
		base.bodyChunks[0] = new BodyChunk(this, 0, default(Vector2), scale, 0.1f);
		base.bodyChunks[0].collideWithSlopes = true;
		bodyChunkConnections = new BodyChunkConnection[0];
		base.CollideWithObjects = false;
		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		bounce = 0.5f;
		surfaceFriction = 0.49f;
		base.waterFriction = 0.94f;
		base.buoyancy = 0.99f;
		allowStabilization = false;
		Shader.SetGlobalVector(RainWorld.ShadPropEnergyCellCoreCol, color);
		explodeColor = new Color(0.2f, 0.2f, 1f);
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		rCam.ReturnFContainer("Items").AddChild(sLeaser.sprites[0]);
		rCam.ReturnFContainer("ForegroundLights").AddChild(sLeaser.sprites[2]);
		rCam.ReturnFContainer("ForegroundLights").AddChild(sLeaser.sprites[1]);
		rCam.ReturnFContainer("HUD").AddChild(sLeaser.sprites[3]);
		rCam.ReturnFContainer("Foreground").AddChild(sLeaser.sprites[4]);
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}

	public virtual void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = default(Vector2);
		vector.x = Mathf.Lerp(base.firstChunk.lastPos.x, base.firstChunk.pos.x, timeStacker) - camPos.x;
		vector.y = Mathf.Lerp(base.firstChunk.lastPos.y, base.firstChunk.pos.y, timeStacker) - camPos.y;
		Vector2 vector2 = Custom.EncodeFloatRG(Custom.Mod((sLeaser.sprites[0].rotation + roll) / 360f, 1f));
		Vector2 vector3 = ((stage == 3 || customAnimation) ? Custom.EncodeFloatRG(moveToTarget) : new Vector2(0f, 0f));
		float num = ((stage == 3) ? (scale + scale * 0.5f * moveToTarget) : scale);
		float scaleX = ((stage == 3) ? (num * Mathf.Lerp(0.95f, 1f, Mathf.Clamp(moveToTarget - 0.85f, 0f, 1f) * 20f)) : (num * 0.95f));
		float rotation = sLeaser.sprites[0].rotation + roll;
		sLeaser.sprites[0].x = vector.x;
		sLeaser.sprites[0].y = vector.y;
		sLeaser.sprites[0].rotation = rotation;
		sLeaser.sprites[0].scale = num;
		sLeaser.sprites[0].scaleX = scaleX;
		sLeaser.sprites[0].color = new Color(vector2.x, vector2.y, vector3.x);
		sLeaser.sprites[0].alpha = vector3.y;
		sLeaser.sprites[1].x = vector.x;
		sLeaser.sprites[1].y = vector.y;
		sLeaser.sprites[1].rotation = rotation;
		sLeaser.sprites[1].scale = num;
		sLeaser.sprites[1].scaleX = scaleX;
		sLeaser.sprites[1].color = new Color(vector2.x, vector2.y, vector3.x);
		sLeaser.sprites[1].alpha = vector3.y;
		sLeaser.sprites[2].x = vector.x;
		sLeaser.sprites[2].y = vector.y;
		sLeaser.sprites[2].scale = ((stage >= 3) ? (moveToTarget * 100f) : 0f);
		sLeaser.sprites[2].alpha = 0.5f + moveToTarget * 0.5f;
		sLeaser.sprites[3].x = vector.x;
		sLeaser.sprites[3].y = vector.y;
		sLeaser.sprites[4].scale = (isStabilized ? Mathf.Lerp(1.9f, 0f, chargeTime / chargeDuration) : 0f);
		sLeaser.sprites[4].x = vector.x;
		sLeaser.sprites[4].y = vector.y;
		if (usingTime > 0f)
		{
			sLeaser.sprites[3].alpha = Mathf.Lerp(sLeaser.sprites[3].alpha, 0.5f, 0.15f);
		}
		else
		{
			sLeaser.sprites[3].alpha = Mathf.Lerp(sLeaser.sprites[3].alpha, 0f, 0.15f);
		}
		if (recharging > 0f)
		{
			sLeaser.sprites[2].color = Color.Lerp(sLeaser.sprites[2].color, Color.black, 0.15f);
		}
		else if (usingTime > 0f)
		{
			sLeaser.sprites[2].color = Color.Lerp(sLeaser.sprites[2].color, Color.white, 0.15f);
		}
		else if (chargeTime > 0f)
		{
			sLeaser.sprites[2].color = Color.Lerp(color, Color.white, chargeTime / chargeDuration);
		}
		else
		{
			sLeaser.sprites[2].color = Color.Lerp(sLeaser.sprites[2].color, color, 0.05f);
		}
		Shader.SetGlobalVector(RainWorld.ShadPropEnergyCellCoreCol, sLeaser.sprites[2].color);
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
		if (stage >= 2 && sLeaser.sprites[0].isVisible)
		{
			sLeaser.sprites[0].isVisible = false;
			sLeaser.sprites[1].isVisible = true;
			if (stage == 2)
			{
				stage = 3;
				moveToTarget = 0f;
			}
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[5];
		sLeaser.sprites[0] = new FSprite("Futile_White");
		sLeaser.sprites[0].scale = scale;
		sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["EnergyCell"];
		sLeaser.sprites[0].isVisible = true;
		sLeaser.sprites[1] = new FSprite("Futile_White");
		sLeaser.sprites[1].scale = scale;
		sLeaser.sprites[1].shader = rCam.room.game.rainWorld.Shaders["EnergyCell"];
		sLeaser.sprites[1].isVisible = false;
		sLeaser.sprites[2] = new FSprite("Futile_White");
		sLeaser.sprites[2].shader = rCam.room.game.rainWorld.Shaders["LightSource"];
		sLeaser.sprites[3] = new FSprite("Futile_White");
		sLeaser.sprites[3].shader = rCam.room.game.rainWorld.Shaders["GravityDisruptor"];
		sLeaser.sprites[3].scale = 10f;
		sLeaser.sprites[3].alpha = 0f;
		sLeaser.sprites[4] = new FSprite("Futile_White");
		sLeaser.sprites[4].shader = rCam.room.game.rainWorld.Shaders["EnergySwirl"];
		sLeaser.sprites[4].color = color;
		sLeaser.sprites[4].alpha = 0.2f;
		sLeaser.sprites[4].scale = 0f;
		AddToContainer(sLeaser, rCam, null);
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		BodyChunk bodyChunk = base.bodyChunks[0];
		float num = bodyChunk.onSlope;
		IntVector2 contactPoint = bodyChunk.ContactPoint;
		bool flag = false;
		if (recharging > 0f)
		{
			recharging -= 1f;
			if (recharging <= 0f)
			{
				room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Core_Ready, base.firstChunk.pos, 1f, 1f);
				for (int i = 0; i < 5; i++)
				{
					room.AddObject(new Spark(base.firstChunk.pos + Custom.RNV() * UnityEngine.Random.value * 5f, Custom.RNV() * Mathf.Lerp(4f, 30f, UnityEngine.Random.value), Color.white, null, 8, 32));
				}
			}
		}
		if (usingTime > 0f)
		{
			usingTime -= 1f;
			customAnimation = true;
			moveToTarget = 0.1f;
			if (usingTime <= 0f)
			{
				customAnimation = false;
				moveToTarget = 0f;
				if (usingTime <= 0f)
				{
					recharging = 400f;
				}
				else
				{
					recharging = 100f;
				}
				usingTime = 0f;
				room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Core_Off, base.firstChunk.pos, 1f, 1f);
			}
		}
		if (chargeTime > 0f)
		{
			chargeTime -= 1f;
			customAnimation = true;
			moveToTarget = Mathf.Lerp(0.05f, 0.1f, chargeTime / chargeDuration);
			if (chargeTime >= chargeDuration)
			{
				Use(forced: false);
			}
			if (chargeTime >= chargeDuration || chargeTime <= 0f)
			{
				customAnimation = false;
				moveToTarget = 0f;
				chargeTime = 0f;
			}
		}
		if (usingTime <= 0f && recharging <= 0f && grabbedBy.Count > 0 && grabbedBy[0].grabber is Player && (grabbedBy[0].grabber as Player).input[0].pckp)
		{
			chargeTime += 2f;
		}
		if (allowStabilization)
		{
			bool flag2 = room.RayTraceTilesForTerrain((int)bodyChunk.pos.x / 20, (int)bodyChunk.pos.y / 20, (int)bodyChunk.pos.x / 20, (int)bodyChunk.pos.y / 20 - 15);
			if (grabbedBy.Count == 0 && touchedGround && contactPoint != new IntVector2(0, -1) && flag2)
			{
				if (!isStabilized)
				{
					chargeTime = 10f;
					isStabilized = true;
					room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Core_Off, base.firstChunk.pos, 0.5f, 0.5f);
					base.gravity = (0f - base.gravity) * 2f;
				}
				bodyChunk.vel = new Vector2(bodyChunk.vel.x * 0.7f, bodyChunk.vel.y * 0.7f);
				base.gravity *= 0.7f;
				roll = Mathf.Lerp(roll, Mathf.Sign(roll) * 0.1f, 0.1f);
				return;
			}
		}
		isStabilized = false;
		if (grabbedBy.Count > 0)
		{
			touchedGround = false;
		}
		if (usingTime > 0f && grabbedBy.Count > 0 && base.Submersion == 0f)
		{
			base.gravity = 0f;
		}
		else
		{
			base.gravity = 0.9f;
		}
		if (grabbedBy.Count == 0 && stage < 0)
		{
			flag = true;
		}
		if (num == 0f && flag)
		{
			velocity = bodyChunk.vel;
			gravity = 0f;
		}
		if (num != 0f && flag)
		{
			gravity += 0.1f;
			velocity = new Vector2(velocity.y * bounce * (0f - num) + velocity.x, Mathf.Abs(velocity.x) * bounce) * (0.7f + 0.3f * Mathf.Abs(Mathf.Clamp(velocity.y, -1f, 0f)));
			velocity -= new Vector2(0f, gravity);
		}
		if (flag)
		{
			bodyChunk.vel = velocity;
		}
		if (stage == 0)
		{
			velocity = bodyChunk.pos;
			stage = 1;
			FXCounter = 0f;
			if (lightningMachine == null)
			{
				room.AddObject(lightningMachine = new LightningMachine(bodyChunk.pos, new Vector2(-1f, 1000f), new Vector2(1f, 1000f), 0f, permanent: false, radial: true, 0.5f, 0.5f, 0.3f));
				lightningMachine.light = true;
				lightningMachine.random = true;
				lightningMachine.lightningParam = 1f;
				lightningMachine.lightningType = Hsl.x;
				lightningMachine.volume = 0.5f;
				lightningMachine.impactType = 1;
			}
		}
		if (stage == 1)
		{
			lightningMachine.pos = bodyChunk.pos;
			lightningMachine.chance = 0.05f;
			if (FXCounter < 10f)
			{
				lightningMachine.chance = 0.3f * (1f - FXCounter / 10f);
				lightningMachine.startPoint = new Vector2(-100f, -500f);
				lightningMachine.endPoint = new Vector2(100f, -500f);
				FXCounter += 1.5000001f;
			}
			float num2 = Mathf.SmoothStep(0f, 1f, moveToTarget);
			bodyChunk.setPos = new Vector2(Mathf.SmoothStep(velocity.x, target.x, num2), Mathf.Lerp(velocity.y, target.y, num2));
			roll += Mathf.Lerp(2f * num2, 0f, num2);
			moveToTarget += 0.010833333f;
			if (moveToTarget > 1f)
			{
				stage = 2;
				moveToTarget = 0f;
				FXCounter = 0f;
				room.AddObject(halo = new LightSource(bodyChunk.pos, environmentalLight: false, color, this));
				halo.affectedByPaletteDarkness = 0f;
				halo.rad = 0f;
				halo.flat = true;
				room.AddObject(new LightningMachine.Impact(bodyChunk.pos, 10f, color));
			}
		}
		if (stage == 3)
		{
			lightningMachine.chance = Custom.LerpQuadEaseIn(0.1f, 0.9f, moveToTarget);
			if (FXCounter < 1f)
			{
				halo.rad += 1f;
				lightningMachine.chance = 1f - FXCounter / 1f;
				FXCounter += 6.666667f;
			}
			bodyChunk.setPos = target;
			bodyChunk.vel *= 0f;
			moveToTarget = Mathf.Clamp(moveToTarget + 0.0009666667f, 0f, 0.9999f);
			roll += moveToTarget * 0.8f;
			halo.rad = halo.rad + (1f - moveToTarget) * 0.05f + 0.1f * moveToTarget;
			halo.rad = Mathf.Min(halo.rad, 300f);
			halo.alpha = Mathf.Lerp(0.7f, 0.5f, halo.Rad / 200f);
		}
		if (contactPoint == new IntVector2(0, -1))
		{
			touchedGround = true;
		}
		if (contactPoint != new IntVector2(0, 0) && flag)
		{
			roll = velocity.x * (float)Math.PI * 0.2f * (0f - Mathf.Sign(contactPoint.y));
		}
		else
		{
			roll = Mathf.Lerp(roll, 0f, 0.1f);
		}
	}

	public void FireUp(Vector2 target)
	{
		this.target = ((stage < 0) ? target : this.target);
		this.target = room.MiddleOfTile(room.GetTilePosition(this.target));
		stage = ((stage >= 0) ? stage : 0);
		moveToTarget = 0f;
		base.CollideWithTerrain = false;
		base.CollideWithSlopes = false;
	}

	public void Use(bool forced)
	{
		if (!((recharging <= 0f && usingTime <= 0f) || forced))
		{
			return;
		}
		usingTime = 600f;
		room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Core_On, base.firstChunk.pos, 1f, 0.75f + UnityEngine.Random.value);
		room.InGameNoise(new InGameNoise(base.firstChunk.pos, 1000f, this, 1f));
		if (forced)
		{
			return;
		}
		for (int i = 0; i < room.physicalObjects.Length; i++)
		{
			for (int j = 0; j < room.physicalObjects[i].Count; j++)
			{
				if (room.physicalObjects[i][j] is EnergyCell && room.physicalObjects[i][j] != this)
				{
					LightningBolt lightningBolt = new LightningBolt(base.firstChunk.pos, room.physicalObjects[i][j].firstChunk.pos, 0, 0.4f, 0.35f, 0.64f, 0.64f, light: true);
					lightningBolt.intensity = 1f;
					lightningBolt.color = Color.blue;
					room.PlaySound(SoundID.Death_Lightning_Spark_Spontaneous, room.physicalObjects[i][j].firstChunk.pos, 0.5f, 1.4f - UnityEngine.Random.value * 0.4f);
					room.AddObject(lightningBolt);
					(room.physicalObjects[i][j] as EnergyCell).Use(forced: true);
				}
			}
		}
	}

	public void KeepOff()
	{
		if (usingTime > 0f)
		{
			room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Core_Off, base.firstChunk.pos, 1f, 1f);
			customAnimation = false;
			moveToTarget = 0f;
		}
		usingTime = 0f;
		recharging = 40f;
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		base.PlaceInRoom(placeRoom);
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			base.bodyChunks[i].HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
		}
	}

	public void Explode()
	{
		Custom.Log("SINGULARITY EXPLODE");
		Vector2 vector = Vector2.Lerp(base.firstChunk.pos, base.firstChunk.lastPos, 0.35f);
		room.AddObject(new SingularityBomb.SparkFlash(base.firstChunk.pos, 300f, new Color(0f, 0f, 1f)));
		room.AddObject(new Explosion(room, this, vector, 7, 450f, 6.2f, 10f, 280f, 0.25f, null, 0.3f, 160f, 1f));
		room.AddObject(new Explosion(room, this, vector, 7, 2000f, 4f, 0f, 400f, 0.25f, null, 0.3f, 200f, 1f));
		room.AddObject(new Explosion.ExplosionLight(vector, 280f, 1f, 7, explodeColor));
		room.AddObject(new Explosion.ExplosionLight(vector, 230f, 1f, 3, new Color(1f, 1f, 1f)));
		room.AddObject(new Explosion.ExplosionLight(vector, 2000f, 2f, 60, explodeColor));
		room.AddObject(new ShockWave(vector, 750f, 1.485f, 300, highLayer: true));
		room.AddObject(new ShockWave(vector, 3000f, 1.185f, 180));
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
			room.AddObject(new SingularityBomb.BombFragment(vector, Custom.DegToVec(((float)k + UnityEngine.Random.value) / 6f * 360f) * Mathf.Lerp(18f, 38f, UnityEngine.Random.value)));
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
				if (room.physicalObjects[m][n] is Creature && Custom.Dist(room.physicalObjects[m][n].firstChunk.pos, base.firstChunk.pos) < 750f)
				{
					(room.physicalObjects[m][n] as Creature).Die();
				}
			}
		}
		FirecrackerPlant.ScareObject scareObject = new FirecrackerPlant.ScareObject(base.firstChunk.pos);
		scareObject.fearRange = 12000f;
		scareObject.fearScavs = true;
		scareObject.lifeTime = -600;
		room.AddObject(scareObject);
		room.InGameNoise(new InGameNoise(base.firstChunk.pos, 12000f, this, 1f));
		room.AddObject(new UnderwaterShock(room, null, base.firstChunk.pos, 10, 1200f, 2f, null, new Color(0.8f, 0.8f, 1f)));
		Destroy();
		abstractPhysicalObject.Destroy();
	}
}
