using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class JokeRifle : PhysicalObject, IDrawable
{
	public class AbstractRifle : AbstractPhysicalObject
	{
		public class AmmoType : ExtEnum<AmmoType>
		{
			public static readonly AmmoType Rock = new AmmoType("Rock", register: true);

			public static readonly AmmoType Grenade = new AmmoType("Grenade", register: true);

			public static readonly AmmoType Firecracker = new AmmoType("Firecracker", register: true);

			public static readonly AmmoType Pearl = new AmmoType("Pearl", register: true);

			public static readonly AmmoType Light = new AmmoType("Light", register: true);

			public static readonly AmmoType Ash = new AmmoType("Ash", register: true);

			public static readonly AmmoType Bees = new AmmoType("Bees", register: true);

			public static readonly AmmoType Void = new AmmoType("Void", register: true);

			public static readonly AmmoType Fruit = new AmmoType("Fruit", register: true);

			public static readonly AmmoType Noodle = new AmmoType("Noodle", register: true);

			public static readonly AmmoType FireEgg = new AmmoType("FireEgg", register: true);

			public static readonly AmmoType Singularity = new AmmoType("Singularity", register: true);

			public AmmoType(string value, bool register = false)
				: base(value, register)
			{
			}
		}

		public AmmoType ammoStyle;

		public Dictionary<AmmoType, int> ammo;

		public AbstractRifle(World world, JokeRifle realizedObject, WorldCoordinate pos, EntityID ID, AmmoType ammoType)
			: base(world, MoreSlugcatsEnums.AbstractObjectType.JokeRifle, realizedObject, pos, ID)
		{
			ammoStyle = ammoType;
			ammo = new Dictionary<AmmoType, int>();
			foreach (string entry in ExtEnum<AmmoType>.values.entries)
			{
				ammo[new AmmoType(entry)] = 0;
			}
		}

		public string AmmoToString()
		{
			return string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat("" + ammo[AmmoType.Rock] + "<JRa>", ammo[AmmoType.Grenade].ToString(), "<JRa>"), ammo[AmmoType.Firecracker].ToString(), "<JRa>"), ammo[AmmoType.Pearl].ToString(), "<JRa>"), ammo[AmmoType.Light].ToString(), "<JRa>"), ammo[AmmoType.Ash].ToString(), "<JRa>"), ammo[AmmoType.Bees].ToString(), "<JRa>"), ammo[AmmoType.Void].ToString(), "<JRa>"), ammo[AmmoType.Fruit].ToString(), "<JRa>"), ammo[AmmoType.Noodle].ToString(), "<JRa>"), ammo[AmmoType.FireEgg].ToString(), "<JRa>"), ammo[AmmoType.Singularity].ToString());
		}

		public void AmmoFromString(string ammoStr)
		{
			string[] array = Regex.Split(ammoStr, "<JRa>");
			ammo[AmmoType.Rock] = int.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
			ammo[AmmoType.Grenade] = int.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			ammo[AmmoType.Firecracker] = int.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			ammo[AmmoType.Pearl] = int.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			ammo[AmmoType.Light] = int.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
			ammo[AmmoType.Ash] = int.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture);
			ammo[AmmoType.Bees] = int.Parse(array[6], NumberStyles.Any, CultureInfo.InvariantCulture);
			ammo[AmmoType.Void] = int.Parse(array[7], NumberStyles.Any, CultureInfo.InvariantCulture);
			ammo[AmmoType.Fruit] = int.Parse(array[8], NumberStyles.Any, CultureInfo.InvariantCulture);
			ammo[AmmoType.Noodle] = int.Parse(array[9], NumberStyles.Any, CultureInfo.InvariantCulture);
			ammo[AmmoType.FireEgg] = int.Parse(array[10], NumberStyles.Any, CultureInfo.InvariantCulture);
			ammo[AmmoType.Singularity] = int.Parse(array[11], NumberStyles.Any, CultureInfo.InvariantCulture);
		}

		public int currentAmmo()
		{
			return ammo[ammoStyle];
		}

		public void setCurrentAmmo(int amount)
		{
			ammo[ammoStyle] = amount;
		}

		public override string ToString()
		{
			string baseString = string.Format(CultureInfo.InvariantCulture, "{0}<oA>{1}<oA>{2}<oA>{3}<oA>{4}", ID.ToString(), type.ToString(), pos.SaveToString(), ammoStyle.ToString(), AmmoToString());
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "<oA>", unrecognizedAttributes);
		}
	}

	public Vector2 aimDir;

	public int counter;

	public LightSource light;

	public int lastShotTime;

	public FirecrackerPlant.ScareObject scareObj;

	public bool initialShot;

	public Vector2 firePos => base.firstChunk.pos + aimDir * 25f;

	public AbstractRifle abstractRifle => abstractPhysicalObject as AbstractRifle;

	public JokeRifle(AbstractPhysicalObject abstractPhysicalObject, World world)
		: base(abstractPhysicalObject)
	{
		base.bodyChunks = new BodyChunk[1];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 5f, 0.07f);
		aimDir = new Vector2(1f, 1f);
		bodyChunkConnections = new BodyChunkConnection[0];
		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		bounce = 0.4f;
		surfaceFriction = 0.4f;
		collisionLayer = 2;
		base.waterFriction = 0.98f;
		base.buoyancy = 0.4f;
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		base.PlaceInRoom(placeRoom);
		base.firstChunk.pos = placeRoom.MiddleOfTile(abstractPhysicalObject.pos);
		base.firstChunk.lastPos = base.firstChunk.pos;
		light = new LightSource(base.firstChunk.pos, environmentalLight: false, new Color(1f, 0.8f, 0.5f), this);
		placeRoom.AddObject(light);
	}

	public override void Update(bool eu)
	{
		if (ModManager.MSC)
		{
			MoreSlugcatsUpdate(eu);
		}
		else
		{
			counter--;
			if (grabbedBy.Count > 0)
			{
				aimDir = Custom.DirVec(grabbedBy[0].grabber.mainBodyChunk.pos, Futile.mousePosition);
				base.firstChunk.vel += aimDir * 10f;
				grabbedBy[0].grabber.mainBodyChunk.vel -= aimDir * 0.9f;
				if (counter < 1)
				{
					counter = 6;
					grabbedBy[0].grabber.mainBodyChunk.vel -= aimDir * 2.5f;
					for (int i = 0; i < 10; i++)
					{
						room.AddObject(new Spark(base.firstChunk.pos + aimDir * 25f, Custom.DegToVec(360f * Random.value) * 5f * Random.value + aimDir * 100f * Random.value, new Color(1f, 0.8f, 0.5f), null, 3, 8));
					}
				}
			}
			light.setPos = base.firstChunk.pos + aimDir * (12 - counter) * 12f;
			light.setRad = (float)counter * 20f;
			light.setAlpha = (float)counter / 6f;
		}
		base.Update(eu);
	}

	public override void Grabbed(Creature.Grasp grasp)
	{
		base.Grabbed(grasp);
	}

	public virtual void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1];
		sLeaser.sprites[0] = new FSprite("JokeRifle");
		sLeaser.sprites[0].anchorY = 0.8f;
		AddToContainer(sLeaser, rCam, null);
	}

	public virtual void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		sLeaser.sprites[0].x = Mathf.Lerp(base.firstChunk.lastPos.x, base.firstChunk.pos.x, timeStacker) - camPos.x;
		sLeaser.sprites[0].y = Mathf.Lerp(base.firstChunk.lastPos.y, base.firstChunk.pos.y, timeStacker) - camPos.y;
		sLeaser.sprites[0].rotation = Custom.AimFromOneVectorToAnother(new Vector2(0f, 0f), aimDir) - 90f;
		if (ModManager.MSC)
		{
			if (sLeaser.sprites[0].rotation < 0f)
			{
				sLeaser.sprites[0].scaleY = -1f;
			}
			else
			{
				sLeaser.sprites[0].scaleY = 1f;
			}
		}
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public virtual void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		sLeaser.sprites[0].color = palette.blackColor;
	}

	public virtual void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Items");
		}
		sLeaser.sprites[0].RemoveFromContainer();
		newContatiner.AddChild(sLeaser.sprites[0]);
	}

	public static bool IsValidAmmo(PhysicalObject obj)
	{
		if (!(obj is Rock) && !(obj is FirecrackerPlant) && !(obj is DataPearl) && !(obj is FlareBomb) && !(obj is ScavengerBomb) && !(obj is PuffBall) && !(obj is SporePlant) && !(obj is DangleFruit) && !(obj is NeedleEgg) && (!(obj is FireEgg) || (obj as FireEgg).activeCounter != 0) && !(obj is SingularityBomb))
		{
			return obj is EnergyCell;
		}
		return true;
	}

	public static AbstractRifle.AmmoType AmmoTypeFromObject(PhysicalObject obj)
	{
		if (obj is FirecrackerPlant)
		{
			return AbstractRifle.AmmoType.Firecracker;
		}
		if (obj is DataPearl)
		{
			return AbstractRifle.AmmoType.Pearl;
		}
		if (obj is FlareBomb)
		{
			return AbstractRifle.AmmoType.Light;
		}
		if (obj is PuffBall)
		{
			return AbstractRifle.AmmoType.Ash;
		}
		if (obj is ScavengerBomb)
		{
			return AbstractRifle.AmmoType.Grenade;
		}
		if (obj is SporePlant)
		{
			return AbstractRifle.AmmoType.Bees;
		}
		if (obj is DangleFruit)
		{
			return AbstractRifle.AmmoType.Fruit;
		}
		if (obj is NeedleEgg)
		{
			return AbstractRifle.AmmoType.Noodle;
		}
		if (obj is FireEgg)
		{
			return AbstractRifle.AmmoType.FireEgg;
		}
		if (obj is SingularityBomb || obj is EnergyCell)
		{
			return AbstractRifle.AmmoType.Singularity;
		}
		return AbstractRifle.AmmoType.Rock;
	}

	public void MoreSlugcatsUpdate(bool eu)
	{
		counter--;
		if (lastShotTime <= 6)
		{
			light.setPos = base.firstChunk.pos + aimDir * (12 - (6 - lastShotTime)) * 12f;
			light.setRad = (float)(6 - lastShotTime) * 20f;
			light.setAlpha = (float)(6 - lastShotTime) / 6f;
		}
		lastShotTime++;
		aimDir = AimDirection();
		if (ShouldUse())
		{
			Use(eu);
		}
		else
		{
			initialShot = true;
		}
		if (scareObj != null && scareObj.slatedForDeletetion)
		{
			scareObj = null;
		}
	}

	public void ReloadRifle(PhysicalObject obj)
	{
		if (IsValidAmmo(obj))
		{
			_ = abstractRifle.ammoStyle;
			abstractRifle.ammoStyle = AmmoTypeFromObject(obj);
			int num = 0;
			num = ((abstractRifle.ammoStyle == AbstractRifle.AmmoType.Firecracker) ? 50 : ((abstractRifle.ammoStyle == AbstractRifle.AmmoType.Pearl) ? 1 : ((abstractRifle.ammoStyle == AbstractRifle.AmmoType.Light) ? 10 : ((abstractRifle.ammoStyle == AbstractRifle.AmmoType.Fruit) ? 10 : ((abstractRifle.ammoStyle == AbstractRifle.AmmoType.Ash) ? 5 : ((abstractRifle.ammoStyle == AbstractRifle.AmmoType.Grenade) ? 1 : ((abstractRifle.ammoStyle == AbstractRifle.AmmoType.Bees) ? 50 : ((abstractRifle.ammoStyle == AbstractRifle.AmmoType.Noodle) ? ((int)(Random.value * 2f) + 2) : ((abstractRifle.ammoStyle == AbstractRifle.AmmoType.FireEgg) ? 3 : ((!(abstractRifle.ammoStyle == AbstractRifle.AmmoType.Singularity)) ? 20 : ((!(obj is EnergyCell)) ? 1 : 5)))))))))));
			abstractRifle.setCurrentAmmo(abstractRifle.currentAmmo() + num);
			lastShotTime = 7;
		}
	}

	public Vector2 AimDirection()
	{
		if (grabbedBy.Count > 0 && grabbedBy[0].grabber is Player)
		{
			Vector2 zero = Vector2.zero;
			Player.InputPackage inputPackage = (grabbedBy[0].grabber as Player).input[0];
			if (inputPackage.thrw || (inputPackage.x == 0 && inputPackage.y == 0))
			{
				return aimDir;
			}
			return Custom.DirVec(grabbedBy[0].grabber.mainBodyChunk.pos, grabbedBy[0].grabber.mainBodyChunk.pos + new Vector2(inputPackage.x, inputPackage.y));
		}
		return Vector2.one;
	}

	public bool ShouldUse()
	{
		if (grabbedBy.Count > 0 && grabbedBy[0].grabber is Player && (grabbedBy[0].grabber as Player).input[0].thrw)
		{
			return true;
		}
		return false;
	}

	public bool DoesBulletSpawn(AbstractRifle.AmmoType type)
	{
		if (type != AbstractRifle.AmmoType.Firecracker && type != AbstractRifle.AmmoType.Grenade && type != AbstractRifle.AmmoType.Pearl && type != AbstractRifle.AmmoType.Bees && type != AbstractRifle.AmmoType.Noodle && type != AbstractRifle.AmmoType.FireEgg)
		{
			return type != AbstractRifle.AmmoType.Singularity;
		}
		return false;
	}

	public void SetBulletType(AbstractBullet bullet)
	{
		bullet.SetBulletType(abstractRifle.ammoStyle);
	}

	public void Use(bool eu)
	{
		base.firstChunk.vel += aimDir * 10f;
		if (counter >= 1 || abstractRifle.currentAmmo() <= 0)
		{
			return;
		}
		if (abstractRifle.ammoStyle == AbstractRifle.AmmoType.Firecracker)
		{
			counter = 6;
		}
		else if (abstractRifle.ammoStyle == AbstractRifle.AmmoType.Pearl)
		{
			counter = 3;
		}
		else if (abstractRifle.ammoStyle == AbstractRifle.AmmoType.Light)
		{
			counter = 60;
		}
		else if (abstractRifle.ammoStyle == AbstractRifle.AmmoType.Ash)
		{
			counter = 60;
		}
		else if (abstractRifle.ammoStyle == AbstractRifle.AmmoType.Grenade)
		{
			counter = 60;
		}
		else if (abstractRifle.ammoStyle == AbstractRifle.AmmoType.Bees)
		{
			counter = 3;
		}
		else if (abstractRifle.ammoStyle == AbstractRifle.AmmoType.Rock)
		{
			counter = 8;
		}
		else if (abstractRifle.ammoStyle == AbstractRifle.AmmoType.Void)
		{
			counter = 6;
		}
		else if (abstractRifle.ammoStyle == AbstractRifle.AmmoType.Fruit)
		{
			counter = 16;
		}
		else if (abstractRifle.ammoStyle == AbstractRifle.AmmoType.Noodle)
		{
			counter = 40;
		}
		else if (abstractRifle.ammoStyle == AbstractRifle.AmmoType.FireEgg)
		{
			counter = 11;
		}
		else if (abstractRifle.ammoStyle == AbstractRifle.AmmoType.Singularity)
		{
			counter = 110;
		}
		float num = 2.5f;
		if (abstractRifle.ammoStyle == AbstractRifle.AmmoType.Firecracker)
		{
			num = 2.5f;
		}
		else if (abstractRifle.ammoStyle == AbstractRifle.AmmoType.Pearl)
		{
			num = 4.5f;
		}
		else if (abstractRifle.ammoStyle == AbstractRifle.AmmoType.Light)
		{
			num = 8f;
		}
		else if (abstractRifle.ammoStyle == AbstractRifle.AmmoType.Ash)
		{
			num = 15f;
		}
		else if (abstractRifle.ammoStyle == AbstractRifle.AmmoType.Grenade)
		{
			num = 40f;
		}
		else if (abstractRifle.ammoStyle == AbstractRifle.AmmoType.Bees)
		{
			num = 0.5f;
		}
		else if (abstractRifle.ammoStyle == AbstractRifle.AmmoType.Rock)
		{
			num = 4f;
		}
		else if (abstractRifle.ammoStyle == AbstractRifle.AmmoType.Void)
		{
			num = 1f;
		}
		else if (abstractRifle.ammoStyle == AbstractRifle.AmmoType.Fruit)
		{
			num = 8f;
		}
		else if (abstractRifle.ammoStyle == AbstractRifle.AmmoType.Noodle)
		{
			num = 20f;
		}
		else if (abstractRifle.ammoStyle == AbstractRifle.AmmoType.FireEgg)
		{
			num = 15f;
		}
		else if (abstractRifle.ammoStyle == AbstractRifle.AmmoType.Singularity)
		{
			num = 40f;
		}
		if (abstractRifle.ammoStyle == AbstractRifle.AmmoType.Firecracker)
		{
			Color color = new Color(1f, 0.8f, 0.5f);
			for (int i = 0; i < 10; i++)
			{
				room.AddObject(new Spark(firePos, Custom.DegToVec(360f * Random.value) * 5f * Random.value + aimDir * 100f * Random.value, color, null, 3, 8));
			}
			light.color = color;
			Color color2 = new Color(1f, 0.4f, 0.3f);
			if (initialShot || Random.value < 0.25f)
			{
				room.AddObject(new Explosion.FlashingSmoke(firePos, Custom.RNV() * 5f * Random.value, 1f, new Color(1f, 1f, 1f), color2, 5));
				Explosion.ExplosionLight obj = new Explosion.ExplosionLight(firePos, Mathf.Lerp(50f, 150f, Random.value), 0.5f, 4, color2);
				room.AddObject(obj);
				room.PlaySound(SoundID.Firecracker_Bang, firePos);
				for (int j = 0; j < room.abstractRoom.creatures.Count; j++)
				{
					if (room.abstractRoom.creatures[j].realizedCreature != null && room.abstractRoom.creatures[j].realizedCreature != grabbedBy[0].grabber && room.abstractRoom.creatures[j].realizedCreature.room == room && !room.abstractRoom.creatures[j].realizedCreature.dead)
					{
						room.abstractRoom.creatures[j].realizedCreature.Deafen((int)Custom.LerpMap(Vector2.Distance(firePos, room.abstractRoom.creatures[j].realizedCreature.mainBodyChunk.pos), 180f, 520f, 110f, 0f));
						room.abstractRoom.creatures[j].realizedCreature.Stun((int)Custom.LerpMap(Vector2.Distance(firePos, room.abstractRoom.creatures[j].realizedCreature.mainBodyChunk.pos), 180f, 520f, 10f, 0f));
					}
				}
			}
			if (scareObj == null)
			{
				scareObj = new FirecrackerPlant.ScareObject(firePos);
				room.AddObject(scareObj);
			}
			else
			{
				scareObj.pos = firePos;
			}
			scareObj.lifeTime = 400;
		}
		else if (abstractRifle.ammoStyle == AbstractRifle.AmmoType.Pearl)
		{
			Color color3 = new Color(0.5f, 1f, 1f);
			for (int k = 0; k < 10; k++)
			{
				room.AddObject(new Spark(firePos, Custom.DegToVec(360f * Random.value) * 5f * Random.value + aimDir * 100f * Random.value, color3, null, 3, 8));
			}
			light.color = color3;
		}
		else
		{
			Color color4 = new Color(1f, 0.8f, 0.5f);
			if (abstractRifle.ammoStyle != AbstractRifle.AmmoType.Void)
			{
				for (int l = 0; l < 5; l++)
				{
					room.AddObject(new Spark(firePos, Custom.DegToVec(360f * Random.value) * 3.5f * Random.value + aimDir * 100f * Random.value, color4, null, 2, 6));
				}
			}
			light.color = color4;
		}
		float num2 = 1f;
		if (aimDir.y == 0f)
		{
			num2 = 2f;
		}
		if (abstractRifle.ammoStyle == AbstractRifle.AmmoType.Void)
		{
			num2 = 0.35f;
		}
		if (abstractRifle.ammoStyle == AbstractRifle.AmmoType.Bees)
		{
			for (int m = 0; m < 3; m++)
			{
				SporePlant.Bee bee = new SporePlant.Bee(null, angry: true, firePos, aimDir * num2 * 40f, SporePlant.Bee.Mode.Hunt);
				bee.forceAngry = true;
				bee.ignoreCreature = grabbedBy[0].grabber;
				room.AddObject(bee);
			}
		}
		if (abstractRifle.ammoStyle == AbstractRifle.AmmoType.Noodle)
		{
			AbstractCreature abstractCreature = new AbstractCreature(room.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.SmallNeedleWorm), null, room.GetWorldCoordinate(firePos), room.game.GetNewID());
			room.abstractRoom.AddEntity(abstractCreature);
			abstractCreature.RealizeInRoom();
			BodyChunk[] array = (abstractCreature.realizedCreature as SmallNeedleWorm).bodyChunks;
			for (int n = 0; n < array.Length; n++)
			{
				array[n].HardSetPosition(firePos);
			}
			(abstractCreature.realizedCreature as SmallNeedleWorm).firstChunk.HardSetPosition(firePos);
			(abstractCreature.realizedCreature as SmallNeedleWorm).Scream();
			(abstractCreature.realizedCreature as SmallNeedleWorm).mainBodyChunk.vel = aimDir * num2 * 40f;
			room.socialEventRecognizer.CreaturePutItemOnGround(abstractCreature.realizedCreature, grabbedBy[0].grabber);
		}
		if (abstractRifle.ammoStyle == AbstractRifle.AmmoType.Singularity)
		{
			AbstractPhysicalObject abstractPhysicalObject = new AbstractPhysicalObject(room.world, MoreSlugcatsEnums.AbstractObjectType.SingularityBomb, null, room.GetWorldCoordinate(firePos), room.game.GetNewID());
			room.abstractRoom.AddEntity(abstractPhysicalObject);
			abstractPhysicalObject.RealizeInRoom();
			(abstractPhysicalObject.realizedObject as SingularityBomb).firstChunk.HardSetPosition(firePos);
			(abstractPhysicalObject.realizedObject as SingularityBomb).thrownBy = grabbedBy[0].grabber;
			(abstractPhysicalObject.realizedObject as SingularityBomb).firstChunk.vel = aimDir * num2 * 40f;
			(abstractPhysicalObject.realizedObject as SingularityBomb).ignited = true;
			(abstractPhysicalObject.realizedObject as SingularityBomb).CreateFear();
		}
		if (abstractRifle.ammoStyle == AbstractRifle.AmmoType.FireEgg)
		{
			FireEgg.AbstractBugEgg abstractBugEgg = new FireEgg.AbstractBugEgg(room.world, null, room.GetWorldCoordinate(firePos), room.game.GetNewID(), Mathf.Lerp(0.35f, 0.6f, Custom.ClampedRandomVariation(0.5f, 0.5f, 2f)));
			room.abstractRoom.AddEntity(abstractBugEgg);
			abstractBugEgg.RealizeInRoom();
			(abstractBugEgg.realizedObject as FireEgg).firstChunk.HardSetPosition(firePos);
			(abstractBugEgg.realizedObject as FireEgg).thrownBy = grabbedBy[0].grabber;
			(abstractBugEgg.realizedObject as FireEgg).firstChunk.vel = aimDir * num2 * 40f;
			(abstractBugEgg.realizedObject as FireEgg).activeCounter = 1;
		}
		if (abstractRifle.ammoStyle == AbstractRifle.AmmoType.Grenade)
		{
			AbstractPhysicalObject abstractPhysicalObject2 = new AbstractPhysicalObject(room.world, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null, room.GetWorldCoordinate(firePos), room.game.GetNewID());
			room.abstractRoom.AddEntity(abstractPhysicalObject2);
			abstractPhysicalObject2.RealizeInRoom();
			(abstractPhysicalObject2.realizedObject as ScavengerBomb).firstChunk.HardSetPosition(firePos);
			(abstractPhysicalObject2.realizedObject as ScavengerBomb).thrownBy = grabbedBy[0].grabber;
			(abstractPhysicalObject2.realizedObject as ScavengerBomb).firstChunk.vel = aimDir * num2 * 40f;
			(abstractPhysicalObject2.realizedObject as ScavengerBomb).ignited = true;
			(abstractPhysicalObject2.realizedObject as ScavengerBomb).InitiateBurn();
		}
		if (DoesBulletSpawn(abstractRifle.ammoStyle))
		{
			int timeToLive = 200;
			if (abstractRifle.ammoStyle == AbstractRifle.AmmoType.Light)
			{
				timeToLive = 1200;
			}
			else if (abstractRifle.ammoStyle == AbstractRifle.AmmoType.Ash)
			{
				timeToLive = 150;
			}
			else if (abstractRifle.ammoStyle == AbstractRifle.AmmoType.Void)
			{
				timeToLive = 40;
			}
			AbstractBullet abstractBullet = new AbstractBullet(room.world, null, room.GetWorldCoordinate(firePos), room.game.GetNewID(), null, timeToLive);
			SetBulletType(abstractBullet);
			room.abstractRoom.AddEntity(abstractBullet);
			abstractBullet.RealizeInRoom();
			Creature shotBy = null;
			if (grabbedBy.Count > 0)
			{
				shotBy = grabbedBy[0].grabber;
			}
			(abstractBullet.realizedObject as Bullet).Shoot(shotBy, firePos, aimDir, num2, eu);
			(abstractBullet.realizedObject as Bullet).setPosAndTail(firePos);
			if (abstractRifle.ammoStyle != AbstractRifle.AmmoType.Void)
			{
				room.PlaySound(SoundID.Slugcat_Throw_Rock, base.firstChunk);
			}
		}
		if (abstractRifle.ammoStyle == AbstractRifle.AmmoType.Pearl)
		{
			room.PlaySound(SoundID.HUD_Food_Meter_Fill_Plop_A, grabbedBy[0].grabber.mainBodyChunk, loop: false, 1f, 3.5f + Random.value * 2f);
		}
		else if (abstractRifle.ammoStyle == AbstractRifle.AmmoType.Void)
		{
			room.PlaySound(SoundID.HUD_Unpause_Game, grabbedBy[0].grabber.mainBodyChunk, loop: false, Random.value * 0.3f + 0.2f, 4.5f + Random.value * 2f);
		}
		else
		{
			room.PlaySound(SoundID.Fire_Spear_Pop, grabbedBy[0].grabber.mainBodyChunk, loop: false, 1f, 3.5f + Random.value * 2f);
		}
		grabbedBy[0].grabber.mainBodyChunk.vel -= aimDir * num;
		if (abstractRifle.ammoStyle != AbstractRifle.AmmoType.Pearl)
		{
			abstractRifle.setCurrentAmmo(abstractRifle.currentAmmo() - 1);
		}
		lastShotTime = 0;
		initialShot = false;
	}
}
