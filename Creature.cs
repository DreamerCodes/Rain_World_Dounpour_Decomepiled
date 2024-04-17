using System;
using System.Collections.Generic;
using MoreSlugcats;
using Noise;
using RWCustom;
using Smoke;
using UnityEngine;

public abstract class Creature : PhysicalObject
{
	public class DamageType : ExtEnum<DamageType>
	{
		public static readonly DamageType Blunt = new DamageType("Blunt", register: true);

		public static readonly DamageType Stab = new DamageType("Stab", register: true);

		public static readonly DamageType Bite = new DamageType("Bite", register: true);

		public static readonly DamageType Water = new DamageType("Water", register: true);

		public static readonly DamageType Explosion = new DamageType("Explosion", register: true);

		public static readonly DamageType Electric = new DamageType("Electric", register: true);

		public static readonly DamageType None = new DamageType("None", register: true);

		public DamageType(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class Grasp
	{
		public class Shareability : ExtEnum<Shareability>
		{
			public static readonly Shareability NonExclusive = new Shareability("NonExclusive", register: true);

			public static readonly Shareability CanOnlyShareWithNonExclusive = new Shareability("CanOnlyShareWithNonExclusive", register: true);

			public static readonly Shareability CanNotShare = new Shareability("CanNotShare", register: true);

			public Shareability(string value, bool register = false)
				: base(value, register)
			{
			}
		}

		public Creature grabber;

		public PhysicalObject grabbed;

		public int graspUsed;

		public int chunkGrabbed;

		public bool discontinued;

		public float dominance;

		public bool pacifying;

		public Shareability shareability;

		public BodyChunk grabbedChunk => grabbed.bodyChunks[chunkGrabbed];

		public Grasp(Creature grabber, PhysicalObject grabbed, int graspUsed, int chunkGrabbed, Shareability shareability, float dominance, bool pacifying)
		{
			this.grabber = grabber;
			this.grabbed = grabbed;
			this.graspUsed = graspUsed;
			this.chunkGrabbed = chunkGrabbed;
			discontinued = false;
			this.dominance = dominance;
			this.shareability = shareability;
			this.pacifying = pacifying;
		}

		public void Release()
		{
			discontinued = true;
			grabbed.grabbedBy.Remove(this);
			grabber.grasps[graspUsed] = null;
			if (grabber.graphicsModule != null)
			{
				grabber.graphicsModule.ReleaseSpecificInternallyContainedObjectSprites(grabbed);
			}
			grabber.abstractCreature.DropCarriedObject(graspUsed);
		}

		public bool ShareabilityConflict(Shareability other)
		{
			if (shareability == Shareability.CanNotShare || other == Shareability.CanNotShare)
			{
				return true;
			}
			if (shareability == Shareability.CanOnlyShareWithNonExclusive && other != Shareability.NonExclusive)
			{
				return true;
			}
			if (other == Shareability.CanOnlyShareWithNonExclusive && shareability != Shareability.NonExclusive)
			{
				return true;
			}
			return false;
		}
	}

	public int blind;

	protected int deaf;

	public AbstractCreature killTag;

	public int killTagCounter;

	public bool leechedOut;

	public int newToRoomInvinsibility;

	public float rainDeath;

	public int shortcutDelay;

	public IntVector2? enteringShortCut;

	private float shortCutRadAdd;

	public WorldCoordinate NPCTransportationDestination = new WorldCoordinate(-1, -1, -1, -1);

	public WorldCoordinate lastCoord;

	public bool lavaContact;

	public int lavaContactCount;

	public bool GrabbedByDaddyCorruption;

	public DamageType stunDamageType;

	public float HypothermiaGain;

	public float HypothermiaExposure;

	private int HypothermiaStunDelayCounter;

	public Player.InputPackage? inputWithoutDiagonals;

	public Player.InputPackage? lastInputWithoutDiagonals;

	public Player.InputPackage? inputWithDiagonals;

	public Player.InputPackage? lastInputWithDiagonals;

	public bool freezeControls;

	public bool protectDeathRecursionFlag;

	public int stun { get; set; }

	public bool Stunned => stun >= 10;

	public bool Blinded => blind > 0;

	public float Deaf
	{
		get
		{
			if (deaf == 0)
			{
				return 0f;
			}
			return Mathf.Pow(Mathf.InverseLerp(0f, 120f, deaf), 0.2f);
		}
	}

	public int mainBodyChunkIndex { get; set; }

	public BodyChunk mainBodyChunk => base.bodyChunks[mainBodyChunkIndex];

	public bool dead { get; protected set; }

	public bool Consious
	{
		get
		{
			if (!Stunned)
			{
				return !dead;
			}
			return false;
		}
	}

	public virtual Vector2 VisionPoint => mainBodyChunk.pos;

	public bool inShortcut { get; set; }

	public Grasp[] grasps { get; protected set; }

	public virtual Vector2 DangerPos => mainBodyChunk.pos;

	public AbstractCreature abstractCreature => (AbstractCreature)abstractPhysicalObject;

	public CreatureTemplate Template => abstractCreature.creatureTemplate;

	public CreatureState State => abstractCreature.state;

	public WorldCoordinate coord => abstractCreature.pos;

	public bool safariControlled
	{
		get
		{
			if (ModManager.MSC)
			{
				return abstractCreature.controlled;
			}
			return false;
		}
	}

	public float Hypothermia
	{
		get
		{
			return abstractCreature.Hypothermia;
		}
		set
		{
			abstractCreature.Hypothermia = value;
		}
	}

	public bool WormGrassGooduckyImmune
	{
		get
		{
			if (grasps != null)
			{
				Grasp[] array = grasps;
				foreach (Grasp grasp in array)
				{
					if (grasp != null && grasp.grabbed.abstractPhysicalObject.type == MoreSlugcatsEnums.AbstractObjectType.GooieDuck)
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	public Creature(AbstractCreature abstractCreature, World world)
		: base(abstractCreature)
	{
		dead = State.dead;
		stun = 0;
		enteringShortCut = null;
		inShortcut = false;
		shortcutDelay = 0;
		mainBodyChunkIndex = 0;
		if (Template.grasps > 0)
		{
			grasps = new Grasp[Template.grasps];
		}
		stunDamageType = DamageType.None;
		if (ModManager.MSC)
		{
			inputWithoutDiagonals = null;
			lastInputWithoutDiagonals = null;
			inputWithDiagonals = null;
			lastInputWithDiagonals = null;
		}
	}

	public override void RecreateSticksFromAbstract()
	{
		base.RecreateSticksFromAbstract();
		for (int num = abstractCreature.stuckObjects.Count - 1; num >= 0; num--)
		{
			if (abstractCreature.stuckObjects[num] is AbstractPhysicalObject.CreatureGripStick && abstractCreature.stuckObjects[num].A == abstractCreature && !Grab(abstractCreature.stuckObjects[num].B.realizedObject, (abstractCreature.stuckObjects[num] as AbstractPhysicalObject.CreatureGripStick).grasp, UnityEngine.Random.Range(0, abstractCreature.stuckObjects[num].B.realizedObject.bodyChunks.Length), Grasp.Shareability.CanNotShare, 0.5f, overrideEquallyDominant: true, pacifying: true))
			{
				abstractCreature.stuckObjects[num].Deactivate();
			}
		}
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		placeRoom.AddObject(this);
		BodyChunk[] array = base.bodyChunks;
		foreach (BodyChunk obj in array)
		{
			obj.pos = placeRoom.MiddleOfTile(abstractCreature.pos.Tile) + Custom.DegToVec(UnityEngine.Random.value * 360f);
			obj.lastPos = obj.pos;
			obj.lastLastPos = obj.pos;
			obj.setPos = null;
			obj.vel *= 0f;
		}
		if (grasps != null)
		{
			Grasp[] array2 = grasps;
			foreach (Grasp grasp in array2)
			{
				if (grasp != null)
				{
					placeRoom.AddObject(grasp.grabbed);
					for (int j = 0; j < grasp.grabbed.bodyChunks.Length; j++)
					{
						grasp.grabbed.bodyChunks[j].pos = mainBodyChunk.pos;
						grasp.grabbed.bodyChunks[j].lastPos = mainBodyChunk.pos;
						grasp.grabbed.bodyChunks[j].lastLastPos = mainBodyChunk.pos;
						grasp.grabbed.bodyChunks[j].setPos = null;
						grasp.grabbed.bodyChunks[j].vel *= 0f;
					}
				}
			}
		}
		NewRoom(placeRoom);
	}

	public override void NewRoom(Room newRoom)
	{
		if (abstractCreature.abstractAI != null && abstractCreature.abstractAI.RealAI != null)
		{
			abstractCreature.abstractAI.RealAI.NewRoom(newRoom);
		}
		if (base.graphicsModule != null)
		{
			base.graphicsModule.Reset();
		}
		newToRoomInvinsibility = 40;
	}

	public override void Update(bool eu)
	{
		if (coord.x != lastCoord.x || coord.y != lastCoord.y || coord.room != lastCoord.room)
		{
			NewTile();
		}
		if (ModManager.MSC)
		{
			SafariControlInputUpdate(0);
		}
		lastCoord = abstractCreature.pos;
		if (abstractCreature.realizedCreature != this)
		{
			Custom.LogWarning("ABSTRACT CREATURE REALIZED CREATURE MISMATCH!");
			if (abstractCreature.realizedCreature != null)
			{
				abstractCreature.realizedCreature.Destroy();
			}
			abstractCreature.realizedCreature = this;
		}
		if (newToRoomInvinsibility > 0)
		{
			newToRoomInvinsibility--;
		}
		if (stun > 0)
		{
			stun--;
		}
		if (stun < 10)
		{
			for (int i = 0; i < grabbedBy.Count; i++)
			{
				if (grabbedBy[i].pacifying)
				{
					stun = 10;
					break;
				}
			}
		}
		if (shortcutDelay > 0)
		{
			shortcutDelay--;
		}
		if (blind > 0)
		{
			blind--;
		}
		if (deaf > 0)
		{
			deaf--;
		}
		if (ModManager.MSC)
		{
			HypothermiaUpdate();
		}
		if (base.Submersion > 0.1f && room.waterObject != null && room.waterObject.WaterIsLethal && !abstractCreature.lavaImmune)
		{
			if (base.Submersion > 0.2f)
			{
				if (this is Player && !dead)
				{
					if (ModManager.MSC && (this as Player).SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
					{
						(this as Player).pyroJumpCounter++;
						if ((this as Player).pyroJumpCounter >= global::MoreSlugcats.MoreSlugcats.cfgArtificerExplosionCapacity.Value)
						{
							(this as Player).PyroDeath();
						}
					}
					else
					{
						(this as Player).Die();
					}
				}
				else if (State is HealthState || (State is HealthState && (State as HealthState).health > 1f))
				{
					if (!dead)
					{
						Violence(null, new Vector2(0f, 5f), base.firstChunk, null, DamageType.Explosion, 0.2f, 0.1f);
					}
				}
				else if (!dead)
				{
					Die();
				}
				if (lavaContactCount == 0)
				{
					mainBodyChunk.vel.y = 35f;
					room.AddObject(new Smolder(room, base.firstChunk.pos, base.firstChunk, null));
				}
				else if (lavaContactCount == 1)
				{
					mainBodyChunk.vel.y = 20f;
				}
				else if (lavaContactCount == 2)
				{
					mainBodyChunk.vel.y = 15f;
				}
				else if (lavaContactCount == 3)
				{
					mainBodyChunk.vel.y = 5f;
					room.AddObject(new Smolder(room, base.firstChunk.pos, base.firstChunk, null));
				}
				if (lavaContactCount < ((this is Player) ? 400 : 30))
				{
					lavaContactCount++;
					room.AddObject(new Explosion.ExplosionSmoke(base.firstChunk.pos, Custom.RNV() * 5f * UnityEngine.Random.value, 1f));
				}
				if (!lavaContact)
				{
					if (lavaContactCount <= 3)
					{
						for (int j = 0; j < 14 + (3 - lavaContactCount) * 5; j++)
						{
							Vector2 vector = Custom.RNV();
							room.AddObject(new Spark(base.firstChunk.pos + vector * UnityEngine.Random.value * 40f, vector * Mathf.Lerp(4f, 30f, UnityEngine.Random.value), Color.white, null, 8, 24));
						}
					}
					room.PlaySound(SoundID.Firecracker_Burn, base.firstChunk.pos, 0.5f, 0.5f + UnityEngine.Random.value * 1.5f);
					lavaContact = true;
					lavaContactCount++;
				}
			}
			else if (lavaContactCount == 0)
			{
				lavaContactCount++;
				room.AddObject(new Smolder(room, base.firstChunk.pos, base.firstChunk, null));
				room.PlaySound(SoundID.Firecracker_Burn, base.firstChunk.pos, 0.5f, 0.3f + UnityEngine.Random.value * 0.6f);
				lavaContact = true;
			}
		}
		else
		{
			lavaContact = false;
		}
		if (killTagCounter > 0 && !State.dead && (!(State is HealthState) || !((State as HealthState).health <= 0f)))
		{
			killTagCounter--;
			if (killTagCounter < 1)
			{
				killTag = null;
			}
		}
		rainDeath = Mathf.Clamp(rainDeath - 0.0125f, 0f, 1f);
		if (ModManager.MSC && State is HealthState && room.world.game.IsArenaSession && room.world.game.GetArenaGameSession.chMeta != null && room.world.game.GetArenaGameSession.chMeta.invincibleCreatures)
		{
			(State as HealthState).health = 1f;
		}
		if (!dead && State is HealthState && (State as HealthState).health < 0f && UnityEngine.Random.value < 0f - (State as HealthState).health && UnityEngine.Random.value < 0.025f)
		{
			Die();
		}
		float num = 0f - base.bodyChunks[0].restrictInRoomRange + 1f;
		if (this is Player && base.bodyChunks[0].restrictInRoomRange == base.bodyChunks[0].defaultRestrictInRoomRange)
		{
			num = ((!((this as Player).bodyMode == Player.BodyModeIndex.WallClimb)) ? Mathf.Max(num, -500f) : Mathf.Max(num, -250f));
		}
		if (base.bodyChunks[0].pos.y < num + 10f && !Template.canFly && grabbedBy.Count > 0)
		{
			Custom.Log("FORCE CREATURE RELEASE UNDER ROOM");
			while (grabbedBy.Count > 0)
			{
				grabbedBy[0].Release();
			}
		}
		if (base.bodyChunks[0].pos.y < num && (!room.water || room.waterInverted || room.defaultWaterLevel < -10) && (!Template.canFly || Stunned || dead) && (this is Player || !room.game.IsArenaSession || room.game.GetArenaGameSession.chMeta == null || !room.game.GetArenaGameSession.chMeta.oobProtect))
		{
			Custom.Log($"{abstractCreature} Fell out of room!");
			if (ModManager.CoopAvailable && State is PlayerState)
			{
				(this as Player).PermaDie();
			}
			Die();
			Destroy();
			abstractCreature.Destroy();
		}
		if (enteringShortCut.HasValue)
		{
			shortCutRadAdd += 0.2f;
			if (base.graphicsModule != null)
			{
				base.graphicsModule.Update();
			}
			int num2 = 0;
			Vector2 vector2 = room.MiddleOfTile(enteringShortCut.Value) + Custom.IntVector2ToVector2(room.ShorcutEntranceHoleDirection(enteringShortCut.Value)) * -5f;
			List<AbstractPhysicalObject> allConnectedObjects = abstractCreature.GetAllConnectedObjects();
			for (int k = 0; k < allConnectedObjects.Count; k++)
			{
				if (allConnectedObjects[k].realizedObject == null)
				{
					continue;
				}
				for (int l = 0; l < allConnectedObjects[k].realizedObject.bodyChunks.Length; l++)
				{
					allConnectedObjects[k].realizedObject.bodyChunks[l].vel *= 0.06f;
					if (!Custom.DistLess(allConnectedObjects[k].realizedObject.bodyChunks[l].pos, vector2, Mathf.Max(10f, shortCutRadAdd)))
					{
						allConnectedObjects[k].realizedObject.bodyChunks[l].lastPos = allConnectedObjects[k].realizedObject.bodyChunks[l].pos;
						allConnectedObjects[k].realizedObject.bodyChunks[l].pos += Custom.DirVec(allConnectedObjects[k].realizedObject.bodyChunks[l].pos, vector2) * 4.5f;
						if (allConnectedObjects[k] == abstractCreature)
						{
							num2++;
						}
					}
					else
					{
						allConnectedObjects[k].realizedObject.bodyChunks[l].pos = vector2;
						allConnectedObjects[k].realizedObject.bodyChunks[l].lastPos = vector2;
					}
				}
			}
			if (num2 == 0)
			{
				SuckedIntoShortCut(enteringShortCut.Value, carriedByOther: false);
			}
			if (base.graphicsModule != null)
			{
				base.graphicsModule.SuckedIntoShortCut(vector2);
			}
			if (appendages != null)
			{
				for (int m = 0; m < appendages.Count; m++)
				{
					appendages[m].Update();
				}
			}
			if (Stunned)
			{
				Custom.Log($"{abstractCreature} cancel shortcut enter");
				enteringShortCut = null;
			}
		}
		else
		{
			shortCutRadAdd = 0.9f;
			base.Update(eu);
		}
	}

	public virtual void NewTile()
	{
		if (room.shortcutsBlinking == null)
		{
			return;
		}
		for (int i = 0; i < 4; i++)
		{
			for (int j = 1; j < 4; j++)
			{
				if (room.GetTile(coord.Tile + Custom.fourDirections[i] * j).Terrain != Room.Tile.TerrainType.ShortcutEntrance || !(room.ShorcutEntranceHoleDirection(coord.Tile + Custom.fourDirections[i] * j) == Custom.fourDirections[i] * -1) || Custom.ManhattanDistance(lastCoord.Tile, coord.Tile + Custom.fourDirections[i] * j) < Custom.ManhattanDistance(coord.Tile, coord.Tile + Custom.fourDirections[i] * j))
				{
					continue;
				}
				int shortcut = Array.IndexOf(room.shortcutsIndex, room.shortcutData(coord.Tile + Custom.fourDirections[i] * j).DestTile);
				int secondary = -1;
				if (room.shortcutData(coord.Tile + Custom.fourDirections[i] * j).shortCutType == ShortcutData.Type.RoomExit)
				{
					int destNode = room.shortcutData(coord.Tile + Custom.fourDirections[i] * j).destNode;
					if (destNode > -1 && destNode < room.abstractRoom.connections.Length && room.abstractRoom.connections[destNode] > -1)
					{
						Room realizedRoom = room.world.GetAbstractRoom(room.abstractRoom.connections[destNode]).realizedRoom;
						if (realizedRoom != null && realizedRoom.BeingViewed)
						{
							int num = room.world.GetAbstractRoom(room.abstractRoom.connections[destNode]).ExitIndex(room.abstractRoom.index);
							if (num > -1)
							{
								IntVector2 startTile = realizedRoom.ShortcutLeadingToNode(num).StartTile;
								realizedRoom.BlinkShortCut(realizedRoom.shortcutsIndex.IndexfOf(startTile), -1, Mathf.Lerp(Template.bodySize, 1.5f, 0.5f));
								for (int k = 0; k < room.game.cameras.Length; k++)
								{
									if (room.game.cameras[k].room == realizedRoom)
									{
										room.game.cameras[k].shortcutGraphics.ColorEntrance(Array.IndexOf(realizedRoom.shortcutsIndex, startTile), ShortCutColor());
									}
								}
							}
						}
					}
				}
				else
				{
					secondary = Array.IndexOf(room.shortcutsIndex, room.shortcutData(coord.Tile + Custom.fourDirections[i] * j).StartTile);
				}
				room.BlinkShortCut(shortcut, secondary, 1f);
			}
		}
	}

	public virtual void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos hitAppendage, DamageType type, float damage, float stunBonus)
	{
		if (source != null && source.owner is Creature)
		{
			SetKillTag((source.owner as Creature).abstractCreature);
		}
		if (directionAndMomentum.HasValue)
		{
			if (hitChunk != null)
			{
				hitChunk.vel += Vector2.ClampMagnitude(directionAndMomentum.Value / hitChunk.mass, 10f);
			}
			else if (hitAppendage != null && this is IHaveAppendages)
			{
				(this as IHaveAppendages).ApplyForceOnAppendage(hitAppendage, directionAndMomentum.Value);
			}
		}
		float num = damage / Template.baseDamageResistance;
		float num2 = (damage * 30f + stunBonus) / Template.baseStunResistance;
		if (State is HealthState)
		{
			num2 *= 1.5f + Mathf.InverseLerp(0.5f, 0f, (State as HealthState).health) * UnityEngine.Random.value;
		}
		if (type.Index != -1)
		{
			if (Template.damageRestistances[type.Index, 0] > 0f)
			{
				num /= Template.damageRestistances[type.Index, 0];
			}
			if (Template.damageRestistances[type.Index, 1] > 0f)
			{
				num2 /= Template.damageRestistances[type.Index, 1];
			}
		}
		if (ModManager.MSC)
		{
			if (room != null && room.world.game.IsArenaSession && room.world.game.GetArenaGameSession.chMeta != null && room.world.game.GetArenaGameSession.chMeta.resistMultiplier > 0f && !(this is Player))
			{
				num /= room.world.game.GetArenaGameSession.chMeta.resistMultiplier;
			}
			if (room != null && room.world.game.IsArenaSession && room.world.game.GetArenaGameSession.chMeta != null && room.world.game.GetArenaGameSession.chMeta.invincibleCreatures && !(this is Player))
			{
				num = 0f;
			}
		}
		stunDamageType = type;
		Stun((int)num2);
		stunDamageType = DamageType.None;
		if (State is HealthState)
		{
			(State as HealthState).health -= num;
			if (Template.quickDeath && (UnityEngine.Random.value < 0f - (State as HealthState).health || (State as HealthState).health < -1f || ((State as HealthState).health < 0f && UnityEngine.Random.value < 0.33f)))
			{
				Die();
			}
		}
		if (num >= Template.instantDeathDamageLimit)
		{
			Die();
		}
	}

	public void SetKillTag(AbstractCreature killer)
	{
		if (!State.dead && (!(State is HealthState) || !((State as HealthState).health <= 0f)))
		{
			killTagCounter = Math.Max(killTagCounter, 200);
			killTag = killer;
		}
	}

	public virtual void Stun(int st)
	{
		stun = Mathf.Max(stun, st);
	}

	public virtual void Blind(int blnd)
	{
		blind = Math.Max(blind, blnd);
	}

	public virtual void Deafen(int df)
	{
		deaf = Math.Max(Math.Max(deaf, df), Math.Min(500, deaf + df / 4));
	}

	public virtual bool SpearStick(Weapon source, float dmg, BodyChunk chunk, Appendage.Pos appPos, Vector2 direction)
	{
		return true;
	}

	public virtual void Die()
	{
		if (!dead)
		{
			Custom.LogImportant("Die!", Template.name);
			if (ModManager.MSC && room != null && room.world.game.IsArenaSession && room.world.game.GetArenaGameSession.chMeta != null && (room.world.game.GetArenaGameSession.chMeta.secondaryWinMethod == ChallengeInformation.ChallengeMeta.WinCondition.PROTECT || room.world.game.GetArenaGameSession.chMeta.winMethod == ChallengeInformation.ChallengeMeta.WinCondition.PROTECT))
			{
				bool flag = false;
				if (room.world.game.GetArenaGameSession.chMeta.protectCreature == null || room.world.game.GetArenaGameSession.chMeta.protectCreature == "")
				{
					if (!(this is Player))
					{
						flag = true;
					}
				}
				else if (Template.name.ToLower() == room.world.game.GetArenaGameSession.chMeta.protectCreature.ToLower() || abstractCreature.creatureTemplate.type.value.ToLower() == room.world.game.GetArenaGameSession.chMeta.protectCreature.ToLower())
				{
					flag = true;
				}
				if (protectDeathRecursionFlag)
				{
					flag = false;
				}
				if (flag)
				{
					for (int i = 0; i < room.world.game.Players.Count; i++)
					{
						if (room.world.game.Players[i].realizedCreature != null && !room.world.game.Players[i].realizedCreature.dead)
						{
							room.world.game.Players[i].realizedCreature.protectDeathRecursionFlag = true;
							room.world.game.Players[i].realizedCreature.Die();
						}
					}
				}
			}
			if (killTag != null && killTag.realizedCreature != null)
			{
				Room realizedRoom = room;
				if (realizedRoom == null)
				{
					realizedRoom = abstractCreature.Room.realizedRoom;
				}
				if (realizedRoom != null && realizedRoom.socialEventRecognizer != null)
				{
					realizedRoom.socialEventRecognizer.Killing(killTag.realizedCreature, this);
				}
				if (abstractCreature.world.game.IsArenaSession && killTag.realizedCreature is Player)
				{
					abstractCreature.world.game.GetArenaGameSession.Killing(killTag.realizedCreature as Player, this);
				}
			}
		}
		dead = true;
		LoseAllGrasps();
		abstractCreature.Die();
	}

	public virtual void SpitOutOfShortCut(IntVector2 pos, Room newRoom, bool spitOutAllSticks)
	{
		if (enteringShortCut.HasValue)
		{
			Custom.LogWarning("Double shortcut entry issue!");
			enteringShortCut = null;
		}
		shortcutDelay = 20;
		inShortcut = false;
		if (!spitOutAllSticks)
		{
			return;
		}
		List<AbstractPhysicalObject> allConnectedObjects = abstractCreature.GetAllConnectedObjects();
		for (int i = 0; i < allConnectedObjects.Count; i++)
		{
			if (allConnectedObjects[i].realizedObject != null)
			{
				for (int j = 0; j < allConnectedObjects[i].realizedObject.bodyChunks.Length; j++)
				{
					allConnectedObjects[i].realizedObject.bodyChunks[j].HardSetPosition(newRoom.MiddleOfTile(pos) + Custom.RNV());
					allConnectedObjects[i].realizedObject.bodyChunks[j].vel *= 0f;
				}
				Room room = allConnectedObjects[i].realizedObject.room;
				newRoom.AddObject(allConnectedObjects[i].realizedObject);
				if (allConnectedObjects[i].realizedObject is Creature)
				{
					(allConnectedObjects[i].realizedObject as Creature).SpitOutOfShortCut(pos, newRoom, spitOutAllSticks: false);
				}
				if (newRoom != room)
				{
					allConnectedObjects[i].realizedObject.NewRoom(newRoom);
				}
			}
		}
	}

	private void SuckedIntoShortCut(IntVector2 entrancePos, bool carriedByOther)
	{
		if (base.room == null)
		{
			Custom.Log(Template.name, "Attempted suck into shortcut but had no assigned room. Carried by other:", carriedByOther.ToString());
			return;
		}
		if (base.room.GetTile(entrancePos).Terrain != Room.Tile.TerrainType.ShortcutEntrance)
		{
			Custom.Log(Template.name, "Attempted suck into a non-existent shortcut entrance. Carried by other:", carriedByOther.ToString());
			Custom.Log($"room: {base.room.abstractRoom.name} ps: {entrancePos}");
			return;
		}
		if (base.room.shortcutData(entrancePos).shortCutType == ShortcutData.Type.Normal)
		{
			abstractCreature.pos.Tile = base.room.shortcutData(entrancePos).DestTile;
		}
		if (!carriedByOther)
		{
			base.room.game.shortcuts.SuckInCreature(this, base.room, base.room.shortcutData(entrancePos));
			if (ModManager.MMF)
			{
				bool enteringShortcut = !base.room.shortcutData(entrancePos).ToNode || base.room.abstractRoom.nodes[base.room.shortcutData(entrancePos).destNode].type == AbstractRoomNode.Type.Exit;
				bool enteringDen = base.room.shortcutData(entrancePos).ToNode && base.room.abstractRoom.nodes[base.room.shortcutData(entrancePos).destNode].type == AbstractRoomNode.Type.Den;
				ReleaseDoorForbiddenCreatures(enteringShortcut, enteringDen);
			}
		}
		enteringShortCut = null;
		List<AbstractPhysicalObject> allConnectedObjects = abstractCreature.GetAllConnectedObjects();
		Room room = base.room;
		for (int i = 0; i < allConnectedObjects.Count; i++)
		{
			if (allConnectedObjects[i].realizedObject != null)
			{
				if (allConnectedObjects[i].realizedObject is Creature)
				{
					(allConnectedObjects[i].realizedObject as Creature).inShortcut = true;
				}
				room.RemoveObject(allConnectedObjects[i].realizedObject);
			}
		}
	}

	public void FlyIntoRoom(WorldCoordinate entrancePos, Room newRoom)
	{
		abstractCreature.pos = entrancePos;
		PlaceInRoom(newRoom);
		Custom.Log(Template.name, "ENTER ROOM FROM BORDER");
		shortcutDelay = 360;
		inShortcut = false;
	}

	public void FlyAwayFromRoom(bool carriedByOther)
	{
		if (grasps != null)
		{
			Grasp[] array = grasps;
			foreach (Grasp grasp in array)
			{
				if (grasp != null)
				{
					if (grasp.grabbed is Creature)
					{
						(grasp.grabbed as Creature).FlyAwayFromRoom(carriedByOther: true);
					}
					else if (room != null)
					{
						room.RemoveObject(grasp.grabbed);
					}
				}
			}
		}
		if (room != null)
		{
			room.RemoveObject(this);
		}
	}

	public virtual void Abstractize()
	{
	}

	public virtual void LoseAllGrasps()
	{
		if (Template.grasps > 0)
		{
			for (int i = 0; i < grasps.Length; i++)
			{
				ReleaseGrasp(i);
			}
		}
	}

	public BodyPart BodyPartByIndex(int index)
	{
		if (base.graphicsModule == null || base.graphicsModule.bodyParts == null || index < 0 || index >= base.graphicsModule.bodyParts.Length)
		{
			return null;
		}
		return base.graphicsModule.bodyParts[index];
	}

	public virtual void ReleaseGrasp(int grasp)
	{
		if (grasps[grasp] != null)
		{
			grasps[grasp].Release();
		}
	}

	public void SwitchGrasps(int fromGrasp, int toGrasp)
	{
		Grasp grasp = grasps[fromGrasp];
		Grasp grasp2 = grasps[toGrasp];
		grasps[fromGrasp] = grasp2;
		grasps[toGrasp] = grasp;
		UpdateGraspIndexes();
	}

	protected void UpdateGraspIndexes()
	{
		for (int i = 0; i < grasps.Length; i++)
		{
			if (grasps[i] != null)
			{
				grasps[i].graspUsed = i;
			}
		}
		for (int j = 0; j < abstractCreature.stuckObjects.Count; j++)
		{
			if (!(abstractCreature.stuckObjects[j] is AbstractPhysicalObject.CreatureGripStick) || abstractCreature.stuckObjects[j].A != abstractCreature)
			{
				continue;
			}
			for (int k = 0; k < grasps.Length; k++)
			{
				if (grasps[k] != null && grasps[k].grabbed.abstractPhysicalObject == abstractCreature.stuckObjects[j].B)
				{
					(abstractCreature.stuckObjects[j] as AbstractPhysicalObject.CreatureGripStick).grasp = k;
					break;
				}
			}
		}
	}

	public virtual bool CanBeGrabbed(Creature grabber)
	{
		return true;
	}

	public virtual bool Grab(PhysicalObject obj, int graspUsed, int chunkGrabbed, Grasp.Shareability shareability, float dominance, bool overrideEquallyDominant, bool pacifying)
	{
		if (grasps == null || graspUsed < 0 || graspUsed > grasps.Length)
		{
			return false;
		}
		if (obj.slatedForDeletetion || (obj is Creature && !(obj as Creature).CanBeGrabbed(this)))
		{
			return false;
		}
		if (grasps[graspUsed] != null && grasps[graspUsed].grabbed == obj)
		{
			ReleaseGrasp(graspUsed);
			grasps[graspUsed] = new Grasp(this, obj, graspUsed, chunkGrabbed, shareability, dominance, pacifying: true);
			obj.Grabbed(grasps[graspUsed]);
			new AbstractPhysicalObject.CreatureGripStick(abstractCreature, obj.abstractPhysicalObject, graspUsed, pacifying || obj.TotalMass < base.TotalMass);
			return true;
		}
		foreach (Grasp item in obj.grabbedBy)
		{
			if (item.grabber == this || (item.ShareabilityConflict(shareability) && ((overrideEquallyDominant && item.dominance == dominance) || item.dominance > dominance)))
			{
				return false;
			}
		}
		for (int num = obj.grabbedBy.Count - 1; num >= 0; num--)
		{
			if (obj.grabbedBy[num].ShareabilityConflict(shareability))
			{
				obj.grabbedBy[num].Release();
			}
		}
		if (grasps[graspUsed] != null)
		{
			ReleaseGrasp(graspUsed);
		}
		grasps[graspUsed] = new Grasp(this, obj, graspUsed, chunkGrabbed, shareability, dominance, pacifying);
		obj.Grabbed(grasps[graspUsed]);
		new AbstractPhysicalObject.CreatureGripStick(abstractCreature, obj.abstractPhysicalObject, graspUsed, pacifying || obj.TotalMass < base.TotalMass);
		return true;
	}

	public virtual void GrabbedObjectSnatched(PhysicalObject grabbedObject, Creature thief)
	{
	}

	public virtual Color ShortCutColor()
	{
		return Template.shortcutColor;
	}

	public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
	{
		base.TerrainImpact(chunk, direction, speed, firstContact);
		if (!firstContact)
		{
			return;
		}
		if (room.game.globalRain.AnyPushAround && room.roomRain != null && direction.y < 0)
		{
			room.roomRain.CreatureSmashedInGround(this, speed);
		}
		float num = speed * Mathf.Lerp(base.bodyChunks[chunk].mass, 1f, 0.75f);
		if (!(num > 10f))
		{
			return;
		}
		for (int i = 0; i < Math.Min(5, (int)((1f + Mathf.Lerp(num, 1f, 0.8f) * 0.4f) * room.roomSettings.CeilingDrips)); i++)
		{
			Vector2 vel = Custom.DegToVec(UnityEngine.Random.value * 360f) * UnityEngine.Random.value * Mathf.Clamp(Mathf.Lerp(speed, 5f, 0.5f), 3f, 15f);
			if (direction.x != 0)
			{
				vel.x = Mathf.Abs(vel.x) * (0f - (float)direction.x);
			}
			if (direction.y != 0)
			{
				vel.y = Mathf.Abs(vel.y) * (0f - (float)direction.y);
			}
			room.AddObject(new WaterDrip(base.bodyChunks[chunk].pos + IntVector2.ToVector2(direction) * base.bodyChunks[chunk].rad * 0.9f, vel, waterColor: false));
		}
	}

	public override void PushOutOf(Vector2 pos, float rad, int exceptedChunk)
	{
		base.PushOutOf(pos, rad, exceptedChunk);
	}

	public virtual void HeardNoise(InGameNoise noise)
	{
		if (!(noise == default(InGameNoise)) && noise.sourceObject != null && noise.sourceObject.room != null && Template.AI && noise.sourceObject != this && Consious && abstractCreature.abstractAI != null && abstractCreature.abstractAI.RealAI != null)
		{
			abstractCreature.abstractAI.RealAI.HeardNoise(noise);
		}
	}

	public override string ToString()
	{
		return abstractCreature.ToString();
	}

	public void ReleaseDoorForbiddenCreatures(bool enteringShortcut, bool enteringDen)
	{
		if ((ModManager.MMF && MMF.cfgVanillaExploits.Value) || (!enteringShortcut && !enteringDen))
		{
			return;
		}
		List<AbstractPhysicalObject> allConnectedObjects = abstractCreature.GetAllConnectedObjects();
		List<AbstractCreature> list = new List<AbstractCreature>();
		List<AbstractPhysicalObject> list2 = new List<AbstractPhysicalObject>();
		foreach (AbstractPhysicalObject item in allConnectedObjects)
		{
			if (item is AbstractCreature)
			{
				if ((item as AbstractCreature).creatureTemplate.forbidStandardShortcutEntry && enteringShortcut)
				{
					list.Add(item as AbstractCreature);
				}
				else if ((item as AbstractCreature).creatureTemplate.abstractImmobile)
				{
					list.Add(item as AbstractCreature);
				}
			}
			list2.Add(item);
		}
		if (list.Count <= 0)
		{
			return;
		}
		do
		{
			if (list[0].realizedCreature != null)
			{
				Creature realizedCreature = list[0].realizedCreature;
				while (realizedCreature.grabbedBy.Count > 0)
				{
					for (int i = 0; i < realizedCreature.grabbedBy.Count; i++)
					{
						if (realizedCreature.grabbedBy[i] != null && list2.Contains(realizedCreature.grabbedBy[i].grabber.abstractCreature))
						{
							Custom.Log("-Creatures entering a shortcut while grabbing", list[0].ToString(), "forbidden from shortcuts, has released it!");
							realizedCreature.grabbedBy[i].Release();
							i = 0;
							if (realizedCreature.grabbedBy.Count == 0)
							{
								break;
							}
						}
					}
				}
				if (realizedCreature.grasps != null)
				{
					for (int j = 0; j < realizedCreature.grasps.Length; j++)
					{
						if (realizedCreature.grasps[j] != null && list2.Contains(realizedCreature.grasps[j].grabber.abstractCreature))
						{
							Custom.Log(list[0].ToString(), "while grabbing something entering a shortcut is forbidden from entering a shortcut!");
							realizedCreature.grasps[j].Release();
							j = 0;
							if (realizedCreature.grasps.Length == 0)
							{
								break;
							}
						}
					}
				}
			}
			else
			{
				for (int k = 0; k < list[0].stuckObjects.Count; k++)
				{
					if (list2.Contains(list[0].stuckObjects[k].A) || list2.Contains(list[0].stuckObjects[k].B))
					{
						Custom.Log("-Attempt to abstract link movement a shortcutForbidden", list[0].ToString(), "into a shortcut!");
						list[0].stuckObjects[k].Deactivate();
						k = 0;
						if (list[0].stuckObjects.Count == 0)
						{
							break;
						}
					}
				}
			}
			list.RemoveAt(0);
		}
		while (list.Count > 0);
	}

	public virtual bool AllowableControlledAIOverride(MovementConnection.MovementType movementType)
	{
		if (movementType != MovementConnection.MovementType.NPCTransportation && movementType != MovementConnection.MovementType.ShortCut && movementType != MovementConnection.MovementType.BetweenRooms && movementType != MovementConnection.MovementType.BigCreatureShortCutSqueeze && movementType != MovementConnection.MovementType.OffScreenMovement && movementType != MovementConnection.MovementType.OffScreenUnallowed && movementType != MovementConnection.MovementType.OutsideRoom && movementType != MovementConnection.MovementType.RegionTransportation && movementType != MovementConnection.MovementType.SeaHighway && movementType != MovementConnection.MovementType.SideHighway)
		{
			return movementType == MovementConnection.MovementType.SkyHighway;
		}
		return true;
	}

	private void HypothermiaUpdate()
	{
		HypothermiaGain = 0f;
		if (abstractCreature.creatureTemplate.type == CreatureTemplate.Type.Overseer)
		{
			HypothermiaExposure = 0f;
			Hypothermia = 0f;
			return;
		}
		if (ModManager.MSC && room.blizzardGraphics != null && room.roomSettings.DangerType == MoreSlugcatsEnums.RoomRainDangerType.Blizzard && room.world.rainCycle.CycleProgression > 0f)
		{
			foreach (IProvideWarmth blizzardHeatSource in room.blizzardHeatSources)
			{
				float num = Vector2.Distance(base.firstChunk.pos, blizzardHeatSource.Position());
				if (abstractCreature.Hypothermia > 0.001f && blizzardHeatSource.loadedRoom == room && num < blizzardHeatSource.range)
				{
					float num2 = Mathf.InverseLerp(blizzardHeatSource.range, blizzardHeatSource.range * 0.2f, num);
					abstractCreature.Hypothermia -= Mathf.Lerp(blizzardHeatSource.warmth * num2, 0f, HypothermiaExposure);
					if (abstractCreature.Hypothermia < 0f)
					{
						abstractCreature.Hypothermia = 0f;
					}
				}
			}
			if (!dead)
			{
				HypothermiaGain = Mathf.Lerp(0f, RainWorldGame.DefaultHeatSourceWarmth * 0.1f, Mathf.InverseLerp(0.1f, 0.95f, room.world.rainCycle.CycleProgression));
				if (!abstractCreature.HypothermiaImmune)
				{
					float num3 = (float)room.world.rainCycle.cycleLength + (float)RainWorldGame.BlizzardHardEndTimer(room.game.IsStorySession);
					HypothermiaGain += Mathf.Lerp(0f, RainWorldGame.BlizzardMaxColdness, Mathf.InverseLerp(0f, num3, room.world.rainCycle.timer));
					HypothermiaGain += Mathf.Lerp(0f, 50f, Mathf.InverseLerp(num3, num3 * 5f, room.world.rainCycle.timer));
				}
				Color blizzardPixel = room.blizzardGraphics.GetBlizzardPixel((int)(mainBodyChunk.pos.x / 20f), (int)(mainBodyChunk.pos.y / 20f));
				HypothermiaGain += blizzardPixel.g / Mathf.Lerp(9100f, 5350f, Mathf.InverseLerp(0f, (float)room.world.rainCycle.cycleLength + 4300f, room.world.rainCycle.timer));
				HypothermiaGain += blizzardPixel.b / 8200f;
				HypothermiaExposure = blizzardPixel.g;
				if (base.Submersion >= 0.1f)
				{
					HypothermiaExposure = 1f;
				}
				HypothermiaGain += base.Submersion / 7000f;
				HypothermiaGain = Mathf.Lerp(0f, HypothermiaGain, Mathf.InverseLerp(-0.5f, room.game.IsStorySession ? 1f : 3.6f, room.world.rainCycle.CycleProgression));
				HypothermiaGain *= Mathf.InverseLerp(50f, -10f, base.TotalMass);
			}
			else
			{
				HypothermiaExposure = 1f;
				HypothermiaGain = Mathf.Lerp(0f, 4E-05f, Mathf.InverseLerp(0.8f, 1f, room.world.rainCycle.CycleProgression));
				HypothermiaGain += base.Submersion / 6000f;
				HypothermiaGain += Mathf.InverseLerp(50f, -10f, base.TotalMass) / 1000f;
			}
			if (Hypothermia > 1.5f)
			{
				HypothermiaGain *= 2.3f;
			}
			else if (Hypothermia > 0.8f)
			{
				HypothermiaGain *= 0.5f;
			}
			if (abstractCreature.HypothermiaImmune)
			{
				HypothermiaGain /= 80f;
			}
			HypothermiaGain = Mathf.Clamp(HypothermiaGain, -1f, 0.0055f);
			Hypothermia += HypothermiaGain;
			if (Hypothermia >= 0.8f && Consious && room != null && !room.abstractRoom.shelter)
			{
				if (HypothermiaGain > 0.0003f)
				{
					if (HypothermiaStunDelayCounter < 0)
					{
						int st = (int)Mathf.Lerp(5f, 60f, Mathf.Pow(Hypothermia / 2f, 8f));
						HypothermiaStunDelayCounter = (int)UnityEngine.Random.Range(300f - Hypothermia * 120f, 500f - Hypothermia * 100f);
						Stun(st);
					}
				}
				else
				{
					HypothermiaStunDelayCounter = UnityEngine.Random.Range(200, 500);
				}
			}
			if (Hypothermia >= 1f && (float)stun > 50f && !dead)
			{
				Die();
				return;
			}
		}
		else
		{
			if (Hypothermia > 2f)
			{
				Hypothermia = 2f;
			}
			Hypothermia = Mathf.Lerp(Hypothermia, 0f, 0.001f);
			HypothermiaExposure = 0f;
		}
		if (room != null && !room.abstractRoom.shelter)
		{
			HypothermiaStunDelayCounter--;
		}
	}

	public bool HypothermiaBodyContactWarmup(Creature self, Creature other)
	{
		bool flag = false;
		if (other.Hypothermia < self.Hypothermia || other.abstractCreature.creatureTemplate.BlizzardAdapted)
		{
			flag = true;
		}
		if (flag)
		{
			if (!other.abstractCreature.creatureTemplate.BlizzardAdapted)
			{
				self.Hypothermia = Mathf.Lerp(self.Hypothermia, other.Hypothermia, 0.004f);
			}
			else
			{
				self.Hypothermia = Mathf.Lerp(self.Hypothermia, 0f, 0.004f);
			}
			other.Hypothermia = Mathf.Lerp(other.Hypothermia, self.Hypothermia, 0.012f);
		}
		return flag;
	}

	public void SafariControlInputUpdate(int playerIndex)
	{
		if ((abstractCreature.controlled || (this is Overseer && (abstractCreature.abstractAI as OverseerAbstractAI).safariOwner)) && room != null && !freezeControls)
		{
			lastInputWithoutDiagonals = inputWithoutDiagonals;
			inputWithoutDiagonals = RWInput.PlayerInput(playerIndex);
			if (inputWithoutDiagonals.HasValue)
			{
				Player.InputPackage value = inputWithoutDiagonals.Value;
				if (value.y != 0 && value.x != 0)
				{
					if (UnityEngine.Random.value < 0.5f)
					{
						value.x = 0;
					}
					else
					{
						value.y = 0;
					}
				}
				inputWithoutDiagonals = value;
			}
			lastInputWithDiagonals = inputWithDiagonals;
			inputWithDiagonals = RWInput.PlayerInput(playerIndex);
			if (!lastInputWithoutDiagonals.HasValue)
			{
				lastInputWithoutDiagonals = inputWithoutDiagonals;
			}
			if (!lastInputWithDiagonals.HasValue)
			{
				lastInputWithDiagonals = inputWithDiagonals;
			}
		}
		else
		{
			inputWithoutDiagonals = null;
			lastInputWithoutDiagonals = null;
			inputWithDiagonals = null;
			lastInputWithDiagonals = null;
		}
	}
}
