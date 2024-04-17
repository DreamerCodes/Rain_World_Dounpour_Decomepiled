using System.Collections.Generic;
using System.Globalization;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class AbstractPhysicalObject : AbstractWorldEntity
{
	public class AbstractObjectType : ExtEnum<AbstractObjectType>
	{
		public static readonly AbstractObjectType Creature = new AbstractObjectType("Creature", register: true);

		public static readonly AbstractObjectType Rock = new AbstractObjectType("Rock", register: true);

		public static readonly AbstractObjectType Spear = new AbstractObjectType("Spear", register: true);

		public static readonly AbstractObjectType FlareBomb = new AbstractObjectType("FlareBomb", register: true);

		public static readonly AbstractObjectType VultureMask = new AbstractObjectType("VultureMask", register: true);

		public static readonly AbstractObjectType PuffBall = new AbstractObjectType("PuffBall", register: true);

		public static readonly AbstractObjectType DangleFruit = new AbstractObjectType("DangleFruit", register: true);

		public static readonly AbstractObjectType Oracle = new AbstractObjectType("Oracle", register: true);

		public static readonly AbstractObjectType PebblesPearl = new AbstractObjectType("PebblesPearl", register: true);

		public static readonly AbstractObjectType SLOracleSwarmer = new AbstractObjectType("SLOracleSwarmer", register: true);

		public static readonly AbstractObjectType SSOracleSwarmer = new AbstractObjectType("SSOracleSwarmer", register: true);

		public static readonly AbstractObjectType DataPearl = new AbstractObjectType("DataPearl", register: true);

		public static readonly AbstractObjectType SeedCob = new AbstractObjectType("SeedCob", register: true);

		public static readonly AbstractObjectType WaterNut = new AbstractObjectType("WaterNut", register: true);

		public static readonly AbstractObjectType JellyFish = new AbstractObjectType("JellyFish", register: true);

		public static readonly AbstractObjectType Lantern = new AbstractObjectType("Lantern", register: true);

		public static readonly AbstractObjectType KarmaFlower = new AbstractObjectType("KarmaFlower", register: true);

		public static readonly AbstractObjectType Mushroom = new AbstractObjectType("Mushroom", register: true);

		public static readonly AbstractObjectType VoidSpawn = new AbstractObjectType("VoidSpawn", register: true);

		public static readonly AbstractObjectType FirecrackerPlant = new AbstractObjectType("FirecrackerPlant", register: true);

		public static readonly AbstractObjectType SlimeMold = new AbstractObjectType("SlimeMold", register: true);

		public static readonly AbstractObjectType FlyLure = new AbstractObjectType("FlyLure", register: true);

		public static readonly AbstractObjectType ScavengerBomb = new AbstractObjectType("ScavengerBomb", register: true);

		public static readonly AbstractObjectType SporePlant = new AbstractObjectType("SporePlant", register: true);

		public static readonly AbstractObjectType AttachedBee = new AbstractObjectType("AttachedBee", register: true);

		public static readonly AbstractObjectType EggBugEgg = new AbstractObjectType("EggBugEgg", register: true);

		public static readonly AbstractObjectType NeedleEgg = new AbstractObjectType("NeedleEgg", register: true);

		public static readonly AbstractObjectType DartMaggot = new AbstractObjectType("DartMaggot", register: true);

		public static readonly AbstractObjectType BubbleGrass = new AbstractObjectType("BubbleGrass", register: true);

		public static readonly AbstractObjectType NSHSwarmer = new AbstractObjectType("NSHSwarmer", register: true);

		public static readonly AbstractObjectType OverseerCarcass = new AbstractObjectType("OverseerCarcass", register: true);

		public static readonly AbstractObjectType CollisionField = new AbstractObjectType("CollisionField", register: true);

		public static readonly AbstractObjectType BlinkingFlower = new AbstractObjectType("BlinkingFlower", register: true);

		public AbstractObjectType(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public abstract class AbstractObjectStick
	{
		public AbstractPhysicalObject A;

		public AbstractPhysicalObject B;

		protected string[] unrecognizedAttributes;

		public AbstractObjectStick(AbstractPhysicalObject A, AbstractPhysicalObject B)
		{
			this.A = A;
			this.B = B;
			A.stuckObjects.Add(this);
			B.stuckObjects.Add(this);
		}

		public void Deactivate()
		{
			A.stuckObjects.Remove(this);
			B.stuckObjects.Remove(this);
		}

		public virtual string SaveToString(int roomIndex)
		{
			return "";
		}

		public static void FromString(string[] splt, AbstractRoom room)
		{
			EntityID entityID = EntityID.FromString(splt[2]);
			EntityID entityID2 = EntityID.FromString(splt[3]);
			AbstractPhysicalObject abstractPhysicalObject = null;
			AbstractPhysicalObject abstractPhysicalObject2 = null;
			for (int i = 0; i < room.entities.Count; i++)
			{
				if (abstractPhysicalObject != null && abstractPhysicalObject2 != null)
				{
					break;
				}
				if (room.entities[i] is AbstractPhysicalObject)
				{
					if (room.entities[i].ID == entityID)
					{
						abstractPhysicalObject = room.entities[i] as AbstractPhysicalObject;
					}
					else if (room.entities[i].ID == entityID2)
					{
						abstractPhysicalObject2 = room.entities[i] as AbstractPhysicalObject;
					}
				}
			}
			if (abstractPhysicalObject == null || abstractPhysicalObject2 == null)
			{
				Custom.LogWarning("Abstract stick recreation failed");
				return;
			}
			switch (splt[1])
			{
			case "gripStk":
				if (abstractPhysicalObject is AbstractCreature)
				{
					new CreatureGripStick(abstractPhysicalObject as AbstractCreature, abstractPhysicalObject2, int.Parse(splt[4], NumberStyles.Any, CultureInfo.InvariantCulture), splt[5] == "1").unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(splt, 6);
				}
				break;
			case "sprLdgStk":
				new AbstractSpearStick(abstractPhysicalObject, abstractPhysicalObject2, int.Parse(splt[4], NumberStyles.Any, CultureInfo.InvariantCulture), int.Parse(splt[5], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(splt[6], NumberStyles.Any, CultureInfo.InvariantCulture)).unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(splt, 7);
				break;
			case "sprLdgAppStk":
				new AbstractSpearAppendageStick(abstractPhysicalObject, abstractPhysicalObject2, int.Parse(splt[4], NumberStyles.Any, CultureInfo.InvariantCulture), int.Parse(splt[5], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(splt[6], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(splt[7], NumberStyles.Any, CultureInfo.InvariantCulture)).unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(splt, 8);
				break;
			case "sprImplStk":
				new ImpaledOnSpearStick(abstractPhysicalObject, abstractPhysicalObject2, int.Parse(splt[4], NumberStyles.Any, CultureInfo.InvariantCulture), int.Parse(splt[5], NumberStyles.Any, CultureInfo.InvariantCulture)).unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(splt, 6);
				break;
			case "sprOnBackStick":
				new Player.AbstractOnBackStick(abstractPhysicalObject, abstractPhysicalObject2).unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(splt, 4);
				break;
			}
		}
	}

	public class CreatureGripStick : AbstractObjectStick
	{
		public int grasp;

		public bool carry;

		public CreatureGripStick(AbstractCreature creature, AbstractPhysicalObject carried, int grasp, bool carry)
			: base(creature, carried)
		{
			this.grasp = grasp;
			this.carry = carry;
		}

		public override string SaveToString(int roomIndex)
		{
			return SaveUtils.AppendUnrecognizedStringAttrs(string.Format(CultureInfo.InvariantCulture, "{0}<stkA>gripStk<stkA>{1}<stkA>{2}<stkA>{3}<stkA>{4}", roomIndex, A.ID.ToString(), B.ID.ToString(), grasp, carry ? "1" : "0"), "<stkA>", unrecognizedAttributes);
		}
	}

	public class AbstractSpearStick : AbstractObjectStick
	{
		public int chunk;

		public int bodyPart;

		public float angle;

		public AbstractPhysicalObject Spear => A;

		public AbstractPhysicalObject LodgedIn => B;

		public AbstractSpearStick(AbstractPhysicalObject spear, AbstractPhysicalObject stuckIn, int chunk, int bodyPart, float angle)
			: base(spear, stuckIn)
		{
			this.chunk = chunk;
			this.bodyPart = bodyPart;
			this.angle = angle;
		}

		public override string SaveToString(int roomIndex)
		{
			return SaveUtils.AppendUnrecognizedStringAttrs(string.Format(CultureInfo.InvariantCulture, "{0}<stkA>sprLdgStk<stkA>{1}<stkA>{2}<stkA>{3}<stkA>{4}<stkA>{5}", roomIndex, A.ID.ToString(), B.ID.ToString(), chunk, bodyPart, angle), "<stkA>", unrecognizedAttributes);
		}
	}

	public class AbstractSpearAppendageStick : AbstractObjectStick
	{
		public int appendage;

		public int prevSeg;

		public float distanceToNext;

		public float angle;

		public AbstractPhysicalObject Spear => A;

		public AbstractPhysicalObject LodgedIn => B;

		public AbstractSpearAppendageStick(AbstractPhysicalObject spear, AbstractPhysicalObject stuckIn, int appendage, int prevSeg, float distanceToNext, float angle)
			: base(spear, stuckIn)
		{
			this.appendage = appendage;
			this.prevSeg = prevSeg;
			this.distanceToNext = distanceToNext;
			this.angle = angle;
		}

		public override string SaveToString(int roomIndex)
		{
			return SaveUtils.AppendUnrecognizedStringAttrs(string.Format(CultureInfo.InvariantCulture, "{0}<stkA>sprLdgAppStk<stkA>{1}<stkA>{2}<stkA>{3}<stkA>{4}<stkA>{5}<stkA>{6}", roomIndex, A.ID.ToString(), B.ID.ToString(), appendage, prevSeg, distanceToNext, angle), "<stkA>", unrecognizedAttributes);
		}
	}

	public class ImpaledOnSpearStick : AbstractObjectStick
	{
		public int chunk;

		public int onSpearPosition;

		public AbstractPhysicalObject Spear => A;

		public AbstractPhysicalObject ObjectOnSpear => B;

		public ImpaledOnSpearStick(AbstractPhysicalObject spear, AbstractPhysicalObject stuckIn, int chunk, int onSpearPosition)
			: base(spear, stuckIn)
		{
			this.chunk = chunk;
			this.onSpearPosition = onSpearPosition;
		}

		public override string SaveToString(int roomIndex)
		{
			return SaveUtils.AppendUnrecognizedStringAttrs(string.Format(CultureInfo.InvariantCulture, "{0}<stkA>sprImplStk<stkA>{1}<stkA>{2}<stkA>{3}<stkA>{4}", roomIndex, A.ID.ToString(), B.ID.ToString(), chunk, onSpearPosition), "<stkA>", unrecognizedAttributes);
		}
	}

	public PhysicalObject realizedObject;

	public List<AbstractObjectStick> stuckObjects;

	public bool destroyOnAbstraction;

	public string[] unrecognizedAttributes;

	public PersistentObjectTracker tracker;

	public AbstractObjectType type;

	public AbstractPhysicalObject(World world, AbstractObjectType type, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID)
		: base(world, pos, ID)
	{
		this.type = type;
		this.realizedObject = realizedObject;
		stuckObjects = new List<AbstractObjectStick>();
	}

	public override void Update(int time)
	{
		base.Update(time);
		if (!ModManager.MMF || !MMF.cfgKeyItemTracking.Value || tracker == null || base.Room == null || base.Room.realizedRoom == null || !base.Room.realizedRoom.shortCutsReady || base.Room.gate || base.Room.NOTRACKERS || (tracker.desiredSpawnLocation.room == base.Room.index && tracker.realizedThisCycle))
		{
			return;
		}
		Room realizedRoom = base.Room.realizedRoom;
		IntVector2 intVector = new IntVector2(0, 0);
		bool flag = false;
		if (base.Room.shelter)
		{
			for (int i = 0; i < world.game.Players.Count; i++)
			{
				if (world.game.Players[i].Room.name == base.Room.name)
				{
					intVector = new IntVector2(world.game.Players[i].pos.Tile.x, world.game.Players[i].pos.Tile.y + 1);
					flag = true;
					break;
				}
			}
		}
		if (ModManager.MSC && tracker.obj.type == MoreSlugcatsEnums.AbstractObjectType.HalcyonPearl && (tracker.lastSeenRoom == "RM_AI" || tracker.lastSeenRoom == "CL_AI"))
		{
			intVector = ((!(tracker.lastSeenRoom == "RM_AI")) ? new IntVector2(126, 6) : new IntVector2(76, 40));
			flag = true;
		}
		if (!flag)
		{
			int j = 0;
			Custom.Log("Persistant tracker guessing a good location to spawn for next cycle in this room! [dry doors]");
			for (; j < realizedRoom.abstractRoom.nodes.Length && (realizedRoom.abstractRoom.nodes[j].type == AbstractRoomNode.Type.Exit || realizedRoom.abstractRoom.nodes[j].type == AbstractRoomNode.Type.Den); j++)
			{
			}
			for (int k = 0; k < 1000; k++)
			{
				int node = (int)(Random.value * (float)j);
				ShortcutData shortcutData = realizedRoom.ShortcutLeadingToNode(node);
				intVector = shortcutData.StartTile + realizedRoom.ShorcutEntranceHoleDirection(shortcutData.StartTile) * (int)(Random.value * 10f);
				intVector += Custom.PerpIntVec(realizedRoom.ShorcutEntranceHoleDirection(shortcutData.StartTile) * (int)(Random.value * 6f - 3f));
				if (!realizedRoom.IsPositionInsideBoundries(intVector) || intVector.y <= 0 || realizedRoom.GetTile(intVector).Solid)
				{
					continue;
				}
				if (realizedRoom.abstractRoom.shelter)
				{
					int num = 0;
					if (!realizedRoom.GetTile(intVector + new IntVector2(0, -1)).Solid)
					{
						num++;
					}
					if (!realizedRoom.GetTile(intVector + new IntVector2(0, 1)).Solid)
					{
						num++;
					}
					if (!realizedRoom.GetTile(intVector + new IntVector2(-1, 0)).Solid)
					{
						num++;
					}
					if (!realizedRoom.GetTile(intVector + new IntVector2(1, 0)).Solid)
					{
						num++;
					}
					if (num >= 3)
					{
						flag = true;
						break;
					}
					continue;
				}
				bool flag2 = true;
				bool flag3 = realizedRoom.water && intVector.y <= realizedRoom.defaultWaterLevel;
				for (int l = -1; l < 2; l++)
				{
					if (!realizedRoom.GetTile(intVector + new IntVector2(l, -1)).Solid)
					{
						flag2 = false;
						break;
					}
				}
				if (flag2 && !flag3)
				{
					flag = true;
					break;
				}
			}
		}
		if (!flag)
		{
			Custom.Log("Persistant tracker guessing a good location to spawn for next cycle in this room! [dry solids]");
			List<IntVector2> list = new List<IntVector2>();
			for (int m = 0; m < realizedRoom.TileWidth; m++)
			{
				for (int n = ((!realizedRoom.water) ? 1 : (realizedRoom.defaultWaterLevel + 1)); n < realizedRoom.TileHeight; n++)
				{
					IntVector2 intVector2 = new IntVector2(m, n);
					if (!realizedRoom.IsPositionInsideBoundries(intVector2) || intVector2.y <= 0 || realizedRoom.GetTile(intVector2).Solid)
					{
						continue;
					}
					bool flag4 = true;
					for (int num2 = -1; num2 < 2; num2++)
					{
						if (!realizedRoom.GetTile(intVector2 + new IntVector2(num2, -1)).Solid)
						{
							flag4 = false;
							break;
						}
					}
					if (flag4)
					{
						list.Add(intVector2);
					}
				}
			}
			if (list.Count > 0)
			{
				intVector = list[Random.Range(0, list.Count)];
				flag = true;
			}
		}
		if (!flag)
		{
			Custom.Log("Persistant tracker guessing a good location to spawn for next cycle in this room! [ANY solids]");
			List<IntVector2> list2 = new List<IntVector2>();
			for (int num3 = 0; num3 < realizedRoom.TileWidth; num3++)
			{
				for (int num4 = 1; num4 < realizedRoom.TileHeight; num4++)
				{
					IntVector2 intVector3 = new IntVector2(num3, num4);
					if (realizedRoom.IsPositionInsideBoundries(intVector3) && intVector3.y > 0 && !realizedRoom.GetTile(intVector3).Solid && realizedRoom.GetTile(intVector3 + new IntVector2(0, -1)).Solid)
					{
						list2.Add(intVector3);
					}
				}
			}
			if (list2.Count > 0)
			{
				intVector = list2[Random.Range(0, list2.Count)];
				flag = true;
			}
		}
		if (!flag)
		{
			Custom.Log("Persistant tracker guessing a good location to spawn for next cycle in this room! [ANYWHERE]");
			List<IntVector2> list3 = new List<IntVector2>();
			for (int num5 = 0; num5 < realizedRoom.TileWidth; num5++)
			{
				for (int num6 = 1; num6 < realizedRoom.TileHeight; num6++)
				{
					IntVector2 item = new IntVector2(num5, num6);
					if (realizedRoom.IsPositionInsideBoundries(item) && !realizedRoom.GetTile(item).Solid)
					{
						list3.Add(item);
					}
				}
			}
			if (list3.Count > 0)
			{
				intVector = list3[Random.Range(0, list3.Count)];
				flag = true;
			}
		}
		tracker.lastSeenRoom = base.Room.name;
		tracker.lastSeenRegion = base.Room.world.name;
		tracker.ChangeDesiredSpawnLocation(new WorldCoordinate(realizedRoom.abstractRoom.index, intVector.x, intVector.y, -1));
		tracker.realizedThisCycle = true;
	}

	public virtual void Move(WorldCoordinate newCoord)
	{
		if (newCoord.CompareDisregardingTile(pos))
		{
			return;
		}
		timeSpentHere = 0;
		if (newCoord.room != pos.room)
		{
			ChangeRooms(newCoord);
		}
		if (!newCoord.TileDefined && pos.room == newCoord.room)
		{
			newCoord.Tile = pos.Tile;
		}
		pos = newCoord;
		for (int i = 0; i < stuckObjects.Count; i++)
		{
			AbstractObjectStick abstractObjectStick = stuckObjects[i];
			if (!ModManager.MMF || abstractObjectStick.A != this)
			{
				abstractObjectStick.A.Move(newCoord);
			}
			if (!ModManager.MMF || abstractObjectStick.B != this)
			{
				abstractObjectStick.B.Move(newCoord);
			}
		}
	}

	public virtual void ChangeRooms(WorldCoordinate newCoord)
	{
		world.GetAbstractRoom(pos).RemoveEntity(this);
		world.GetAbstractRoom(newCoord).AddEntity(this);
	}

	public virtual void Realize()
	{
		if (realizedObject != null)
		{
			return;
		}
		if (type == AbstractObjectType.Rock)
		{
			realizedObject = new Rock(this, world);
		}
		else if (type == AbstractObjectType.Spear)
		{
			if ((this as AbstractSpear).explosive)
			{
				realizedObject = new ExplosiveSpear(this, world);
			}
			else if (ModManager.MSC && (this as AbstractSpear).electric)
			{
				realizedObject = new ElectricSpear(this, world);
			}
			else
			{
				realizedObject = new Spear(this, world);
			}
			if ((this as AbstractSpear).needle)
			{
				(realizedObject as Spear).Spear_makeNeedle(Random.Range(0, 3), active: false);
			}
		}
		else if (type == AbstractObjectType.FlareBomb)
		{
			realizedObject = new FlareBomb(this, world);
		}
		else if (type == AbstractObjectType.PuffBall)
		{
			realizedObject = new PuffBall(this, world);
		}
		else if (type == AbstractObjectType.DangleFruit)
		{
			realizedObject = new DangleFruit(this);
		}
		else if (type == AbstractObjectType.PebblesPearl)
		{
			realizedObject = new PebblesPearl(this, world);
		}
		else if (type == AbstractObjectType.SLOracleSwarmer)
		{
			realizedObject = new SLOracleSwarmer(this, world);
		}
		else if (type == AbstractObjectType.SSOracleSwarmer)
		{
			realizedObject = new SSOracleSwarmer(this, world);
		}
		else if (type == AbstractObjectType.DataPearl)
		{
			realizedObject = new DataPearl(this, world);
		}
		else if (type == AbstractObjectType.SeedCob)
		{
			realizedObject = new SeedCob(this);
		}
		else if (type == AbstractObjectType.WaterNut)
		{
			if ((this as WaterNut.AbstractWaterNut).swollen)
			{
				realizedObject = new SwollenWaterNut(this);
			}
			else
			{
				realizedObject = new WaterNut(this);
			}
		}
		else if (type == AbstractObjectType.JellyFish)
		{
			realizedObject = new JellyFish(this);
		}
		else if (type == AbstractObjectType.Lantern)
		{
			realizedObject = new Lantern(this);
		}
		else if (type == AbstractObjectType.KarmaFlower)
		{
			realizedObject = new KarmaFlower(this);
		}
		else if (type == AbstractObjectType.Mushroom)
		{
			realizedObject = new Mushroom(this);
		}
		else if (type == AbstractObjectType.VoidSpawn)
		{
			realizedObject = new VoidSpawn(this, (base.Room.realizedRoom != null) ? base.Room.realizedRoom.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.VoidMelt) : 0f, base.Room.realizedRoom != null && VoidSpawnKeeper.DayLightMode(base.Room.realizedRoom));
		}
		else if (type == AbstractObjectType.FirecrackerPlant)
		{
			realizedObject = new FirecrackerPlant(this, world);
		}
		else if (type == AbstractObjectType.VultureMask)
		{
			realizedObject = new VultureMask(this, world);
		}
		else if (type == AbstractObjectType.SlimeMold)
		{
			realizedObject = new SlimeMold(this);
		}
		else if (type == AbstractObjectType.FlyLure)
		{
			realizedObject = new FlyLure(this, world);
		}
		else if (type == AbstractObjectType.ScavengerBomb)
		{
			realizedObject = new ScavengerBomb(this, world);
		}
		else if (type == AbstractObjectType.SporePlant)
		{
			realizedObject = new SporePlant(this, world);
		}
		else if (type == AbstractObjectType.EggBugEgg)
		{
			realizedObject = new EggBugEgg(this);
		}
		else if (type == AbstractObjectType.NeedleEgg)
		{
			realizedObject = new NeedleEgg(this);
		}
		else if (type == AbstractObjectType.DartMaggot)
		{
			realizedObject = new DartMaggot(this);
		}
		else if (type == AbstractObjectType.BubbleGrass)
		{
			realizedObject = new BubbleGrass(this);
		}
		else if (type == AbstractObjectType.NSHSwarmer)
		{
			realizedObject = new NSHSwarmer(this);
		}
		else if (type == AbstractObjectType.OverseerCarcass)
		{
			realizedObject = new OverseerCarcass(this, world);
		}
		else if (type == AbstractObjectType.CollisionField)
		{
			CollisionField.AbstractCollisionField abstractCollisionField = this as CollisionField.AbstractCollisionField;
			realizedObject = new CollisionField(this, abstractCollisionField.fieldType, abstractCollisionField.radius, abstractCollisionField.liveTime);
		}
		else if (type == AbstractObjectType.BlinkingFlower)
		{
			realizedObject = new BlinkingFlower(this);
		}
		for (int i = 0; i < stuckObjects.Count; i++)
		{
			if (stuckObjects[i].A.realizedObject == null && stuckObjects[i].A != this)
			{
				stuckObjects[i].A.Realize();
			}
			if (stuckObjects[i].B.realizedObject == null && stuckObjects[i].B != this)
			{
				stuckObjects[i].B.Realize();
			}
		}
		if (ModManager.MSC)
		{
			MSCItemsRealizer();
		}
	}

	public virtual void RealizeInRoom()
	{
		if (InDen)
		{
			return;
		}
		Realize();
		if (world.GetAbstractRoom(pos).realizedRoom == null)
		{
			Custom.LogWarning($"TRYING TO REALIZE IN NON REALIZED ROOM! {type}");
			if (this is AbstractCreature)
			{
				Custom.LogWarning($"creature type: {(this as AbstractCreature).creatureTemplate.type}");
			}
			return;
		}
		if (!pos.TileDefined)
		{
			pos.Tile = base.Room.realizedRoom.LocalCoordinateOfNode(pos.abstractNode).Tile;
		}
		List<AbstractPhysicalObject> allConnectedObjects = GetAllConnectedObjects();
		for (int i = 0; i < allConnectedObjects.Count; i++)
		{
			allConnectedObjects[i].pos = pos;
			if (allConnectedObjects[i].realizedObject != null)
			{
				allConnectedObjects[i].realizedObject.PlaceInRoom(base.Room.realizedRoom);
			}
		}
		if (ModManager.MSC && world.game.IsArenaSession && world.game.GetArenaGameSession.chMeta != null && world.game.GetArenaGameSession.chMeta.challengeNumber == 23 && realizedObject is SlimeMold)
		{
			(realizedObject as SlimeMold).big = true;
		}
	}

	public override void Abstractize(WorldCoordinate coord)
	{
		base.Abstractize(coord);
		timeSpentHere = 0;
		Move(coord);
		if (realizedObject != null && realizedObject.room != null)
		{
			realizedObject.room.RemoveObject(realizedObject);
		}
		realizedObject = null;
		if (destroyOnAbstraction)
		{
			Destroy();
		}
	}

	public override void IsEnteringDen(WorldCoordinate den)
	{
		if (den.room != pos.room || den.abstractNode != pos.abstractNode)
		{
			Move(den);
		}
		base.IsEnteringDen(den);
		for (int num = stuckObjects.Count - 1; num >= 0; num--)
		{
			if (num < stuckObjects.Count)
			{
				AbstractObjectStick abstractObjectStick = stuckObjects[num];
				if (den.room != abstractObjectStick.A.pos.room || den.abstractNode != abstractObjectStick.A.pos.abstractNode)
				{
					abstractObjectStick.A.Move(den);
				}
				if (den.room != abstractObjectStick.B.pos.room || den.abstractNode != abstractObjectStick.B.pos.abstractNode)
				{
					abstractObjectStick.B.Move(den);
				}
				if (!abstractObjectStick.A.InDen)
				{
					base.Room.MoveEntityToDen(abstractObjectStick.A);
				}
				if (!abstractObjectStick.B.InDen)
				{
					base.Room.MoveEntityToDen(abstractObjectStick.B);
				}
			}
		}
	}

	public override void IsExitingDen()
	{
		base.IsExitingDen();
		for (int i = 0; i < stuckObjects.Count; i++)
		{
			if (stuckObjects[i].A.InDen)
			{
				base.Room.MoveEntityOutOfDen(stuckObjects[i].A);
			}
			if (stuckObjects[i].B.InDen)
			{
				base.Room.MoveEntityOutOfDen(stuckObjects[i].B);
			}
		}
	}

	public override void Destroy()
	{
		LoseAllStuckObjects();
		base.Destroy();
	}

	public override string ToString()
	{
		string baseString = string.Format(CultureInfo.InvariantCulture, "{0}<oA>{1}<oA>{2}", ID.ToString(), type.ToString(), pos.SaveToString());
		baseString = SaveState.SetCustomData(this, baseString);
		return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "<oA>", unrecognizedAttributes);
	}

	public List<AbstractPhysicalObject> GetAllConnectedObjects()
	{
		List<AbstractPhysicalObject> l = new List<AbstractPhysicalObject> { this };
		AddConnected(ref l);
		return l;
	}

	private void AddConnected(ref List<AbstractPhysicalObject> l)
	{
		for (int i = 0; i < stuckObjects.Count; i++)
		{
			if (!l.Contains(stuckObjects[i].A))
			{
				l.Add(stuckObjects[i].A);
				stuckObjects[i].A.AddConnected(ref l);
			}
			if (!l.Contains(stuckObjects[i].B))
			{
				l.Add(stuckObjects[i].B);
				stuckObjects[i].B.AddConnected(ref l);
			}
		}
	}

	public void LoseAllStuckObjects()
	{
		for (int num = stuckObjects.Count - 1; num >= 0; num--)
		{
			stuckObjects[num].Deactivate();
		}
	}

	public static bool UsesAPersistantTracker(AbstractPhysicalObject abs)
	{
		if (!ModManager.MMF || !MMF.cfgKeyItemTracking.Value)
		{
			return false;
		}
		if (ModManager.MMF && ((abs is DataPearl.AbstractDataPearl && DataPearl.PearlIsNotMisc((abs as DataPearl.AbstractDataPearl).dataPearlType)) || abs.type == AbstractObjectType.NSHSwarmer))
		{
			return true;
		}
		if (ModManager.MSC && (abs.type == MoreSlugcatsEnums.AbstractObjectType.JokeRifle || (abs.type == AbstractObjectType.VultureMask && (abs as VultureMask.AbstractVultureMask).scavKing) || abs.type == MoreSlugcatsEnums.AbstractObjectType.EnergyCell))
		{
			return true;
		}
		return false;
	}

	private void MSCItemsRealizer()
	{
		if (type == MoreSlugcatsEnums.AbstractObjectType.SingularityBomb)
		{
			realizedObject = new SingularityBomb(this, world);
		}
		else if (type == MoreSlugcatsEnums.AbstractObjectType.Spearmasterpearl)
		{
			realizedObject = new SpearMasterPearl(this, world);
		}
		else if (type == MoreSlugcatsEnums.AbstractObjectType.HalcyonPearl)
		{
			realizedObject = new HalcyonPearl(this, world);
		}
		else if (type == MoreSlugcatsEnums.AbstractObjectType.FireEgg)
		{
			realizedObject = new FireEgg(this);
		}
		else if (type == MoreSlugcatsEnums.AbstractObjectType.EnergyCell)
		{
			realizedObject = new EnergyCell(this);
		}
		else if (type == MoreSlugcatsEnums.AbstractObjectType.Seed)
		{
			realizedObject = new SlimeMold(this);
			(realizedObject as SlimeMold).bites = 1;
		}
		else if (type == MoreSlugcatsEnums.AbstractObjectType.GooieDuck)
		{
			realizedObject = new GooieDuck(this);
		}
		else if (type == MoreSlugcatsEnums.AbstractObjectType.LillyPuck)
		{
			realizedObject = new LillyPuck(this, world);
		}
		else if (type == MoreSlugcatsEnums.AbstractObjectType.GlowWeed)
		{
			realizedObject = new GlowWeed(this);
		}
		else if (type == MoreSlugcatsEnums.AbstractObjectType.MoonCloak)
		{
			realizedObject = new MoonCloak(this);
		}
		else if (type == MoreSlugcatsEnums.AbstractObjectType.DandelionPeach)
		{
			realizedObject = new DandelionPeach(this);
		}
		else if (type == MoreSlugcatsEnums.AbstractObjectType.HRGuard)
		{
			realizedObject = new HRGuardManager(this);
		}
		else if (type == MoreSlugcatsEnums.AbstractObjectType.JokeRifle)
		{
			realizedObject = new JokeRifle(this, world);
		}
		else if (type == MoreSlugcatsEnums.AbstractObjectType.Bullet)
		{
			realizedObject = new Bullet(this, world);
		}
	}
}
