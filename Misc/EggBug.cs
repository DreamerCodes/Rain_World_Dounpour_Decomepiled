using System;
using System.Collections.Generic;
using MoreSlugcats;
using Noise;
using RWCustom;
using Smoke;
using UnityEngine;

public class EggBug : InsectoidCreature, Weapon.INotifyOfFlyingWeapons
{
	public EggBugAI AI;

	private int footingCounter;

	public int outOfWaterFooting;

	public int specialMoveCounter;

	public IntVector2 specialMoveDestination;

	private MovementConnection lastFollowedConnection;

	public float runSpeed;

	public float runCycle;

	public bool currentlyClimbingCorridor;

	public bool sitting;

	public int noJumps;

	public int shake;

	public Vector2 travelDir;

	public Vector2 antennaDir;

	public Vector2 awayFromTerrainDir;

	public float antennaAttention;

	public float hue;

	public bool dropEggs;

	public float[,] spineExtensions;

	public bool[,] stabSpine;

	public int stabTicker;

	public FireSmoke bleedSmoke;

	public Vector2 jumpAtPos;

	private int overdeadTicker;

	private int nextStab;

	private int stabCount;

	public bool dropSpears;

	public int eggsLeft;

	public int kickDelay;

	public int timeWithoutEggs;

	public new HealthState State => base.abstractCreature.state as HealthState;

	public bool Footing
	{
		get
		{
			if (footingCounter <= 20)
			{
				return outOfWaterFooting > 0;
			}
			return true;
		}
	}

	public bool FireBug
	{
		get
		{
			if (ModManager.MSC)
			{
				return base.abstractCreature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.FireBug;
			}
			return false;
		}
	}

	public EggBug(AbstractCreature abstractCreature, World world)
		: base(abstractCreature, world)
	{
		float num = 0.4f;
		base.bodyChunks = new BodyChunk[2];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 5f, num * (1f / 3f));
		base.bodyChunks[1] = new BodyChunk(this, 1, new Vector2(0f, 0f), 8f, num * (2f / 3f));
		bodyChunkConnections = new BodyChunkConnection[1];
		bodyChunkConnections[0] = new BodyChunkConnection(base.bodyChunks[0], base.bodyChunks[1], FireBug ? 24f : 12f, BodyChunkConnection.Type.Normal, 1f, 0.5f);
		dropEggs = abstractCreature.state.alive;
		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		bounce = 0.1f;
		surfaceFriction = 0.4f;
		collisionLayer = 1;
		base.waterFriction = 0.96f;
		base.buoyancy = 0.95f;
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(abstractCreature.ID.RandomSeed);
		hue = Mathf.Lerp(FireBug ? 0.35f : (-0.15f), FireBug ? 0.6f : 0.1f, Custom.ClampedRandomVariation(0.5f, 0.5f, 2f));
		UnityEngine.Random.state = state;
		if (FireBug)
		{
			spineExtensions = new float[2, 3];
			stabSpine = new bool[2, 3];
			dropSpears = abstractCreature.state.alive;
			eggsLeft = 6;
			nextStab = UnityEngine.Random.Range(15, 60);
		}
	}

	public override Color ShortCutColor()
	{
		return Custom.HSL2RGB(Custom.Decimal(hue + (FireBug ? EggBugGraphics.HUE_OFF : 1.5f)), 1f, 0.5f);
	}

	public override void InitiateGraphicsModule()
	{
		if (base.graphicsModule == null)
		{
			base.graphicsModule = new EggBugGraphics(this);
		}
		base.graphicsModule.Reset();
	}

	private void DropEggs()
	{
		if (FireBug && eggsLeft <= 0)
		{
			return;
		}
		dropEggs = false;
		if (base.graphicsModule != null)
		{
			if (FireBug)
			{
				Vector2 pos = base.firstChunk.pos;
				room.AddObject(new Explosion(room, this, pos, 5, 200f, 10f, 0.25f, 60f, 0.3f, this, 0.8f, 0f, 0.7f));
				for (int i = 0; i < 14; i++)
				{
					room.AddObject(new Explosion.ExplosionSmoke(pos, Custom.RNV() * 5f * UnityEngine.Random.value, 1f));
				}
				room.AddObject(new Explosion.ExplosionLight(pos, 160f, 1f, 3, Color.white));
				room.AddObject(new ShockWave(pos, 300f, 0.165f, 4));
				for (int j = 0; j < 20; j++)
				{
					Vector2 vector = Custom.RNV();
					room.AddObject(new Spark(pos + vector * UnityEngine.Random.value * 40f, vector * Mathf.Lerp(4f, 30f, UnityEngine.Random.value), Color.white, null, 4, 18));
				}
				room.ScreenMovement(pos, default(Vector2), 0.7f);
				for (int k = 0; k < abstractPhysicalObject.stuckObjects.Count; k++)
				{
					abstractPhysicalObject.stuckObjects[k].Deactivate();
				}
				room.PlaySound(SoundID.Fire_Spear_Explode, pos);
				room.InGameNoise(new InGameNoise(pos, 8000f, this, 1f));
			}
			for (int l = 0; l < 2; l++)
			{
				for (int m = 0; m < 3; m++)
				{
					int num = l * 3 + m;
					if (!FireBug || eggsLeft > num)
					{
						Vector2 newPos = (base.graphicsModule as EggBugGraphics).EggAttachPos(l, m, 1f);
						if (FireBug)
						{
							FireEgg.AbstractBugEgg abstractBugEgg = new FireEgg.AbstractBugEgg(room.world, null, base.abstractCreature.pos, room.game.GetNewID(), hue);
							room.abstractRoom.AddEntity(abstractBugEgg);
							abstractBugEgg.RealizeInRoom();
							abstractBugEgg.realizedObject.firstChunk.HardSetPosition(newPos);
							abstractBugEgg.realizedObject.firstChunk.vel = (base.graphicsModule as EggBugGraphics).eggs[l, m].vel + Custom.DegToVec(180f * Mathf.Pow(UnityEngine.Random.value, 3f) * ((UnityEngine.Random.value >= 0.5f) ? 1f : (-1f))) * 4f * Mathf.Pow(UnityEngine.Random.value, 0.3f);
							(abstractBugEgg.realizedObject as FireEgg).setRotation = Custom.RNV();
							(abstractBugEgg.realizedObject as FireEgg).swell = 0f;
							(abstractBugEgg.realizedObject as FireEgg).rotVel = Mathf.Lerp(-20f, 20f, UnityEngine.Random.value);
							(abstractBugEgg.realizedObject as FireEgg).liquid = 1f;
						}
						else
						{
							EggBugEgg.AbstractBugEgg abstractBugEgg2 = new EggBugEgg.AbstractBugEgg(room.world, null, base.abstractCreature.pos, room.game.GetNewID(), hue);
							room.abstractRoom.AddEntity(abstractBugEgg2);
							abstractBugEgg2.RealizeInRoom();
							abstractBugEgg2.realizedObject.firstChunk.HardSetPosition(newPos);
							abstractBugEgg2.realizedObject.firstChunk.vel = (base.graphicsModule as EggBugGraphics).eggs[l, m].vel + Custom.DegToVec(180f * Mathf.Pow(UnityEngine.Random.value, 3f) * ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f)) * 16f * Mathf.Pow(UnityEngine.Random.value, 0.3f);
							(abstractBugEgg2.realizedObject as EggBugEgg).setRotation = Custom.RNV();
							(abstractBugEgg2.realizedObject as EggBugEgg).swell = 0f;
							(abstractBugEgg2.realizedObject as EggBugEgg).rotVel = Mathf.Lerp(-20f, 20f, UnityEngine.Random.value);
							(abstractBugEgg2.realizedObject as EggBugEgg).liquid = 1f;
						}
					}
				}
			}
		}
		else
		{
			for (int n = 0; n < (FireBug ? eggsLeft : 6); n++)
			{
				Vector2 newPos2 = base.bodyChunks[1].pos + Custom.RNV() * 4f * UnityEngine.Random.value;
				if (FireBug)
				{
					FireEgg.AbstractBugEgg abstractBugEgg3 = new FireEgg.AbstractBugEgg(room.world, null, base.abstractCreature.pos, room.game.GetNewID(), hue);
					room.abstractRoom.AddEntity(abstractBugEgg3);
					abstractBugEgg3.RealizeInRoom();
					abstractBugEgg3.realizedObject.firstChunk.HardSetPosition(newPos2);
					abstractBugEgg3.realizedObject.firstChunk.vel = Vector2.zero;
					(abstractBugEgg3.realizedObject as FireEgg).setRotation = Custom.RNV();
					(abstractBugEgg3.realizedObject as FireEgg).swell = 0f;
					(abstractBugEgg3.realizedObject as FireEgg).rotVel = Mathf.Lerp(-20f, 20f, UnityEngine.Random.value);
				}
				else
				{
					EggBugEgg.AbstractBugEgg abstractBugEgg4 = new EggBugEgg.AbstractBugEgg(room.world, null, base.abstractCreature.pos, room.game.GetNewID(), hue);
					room.abstractRoom.AddEntity(abstractBugEgg4);
					abstractBugEgg4.RealizeInRoom();
					abstractBugEgg4.realizedObject.firstChunk.HardSetPosition(newPos2);
					abstractBugEgg4.realizedObject.firstChunk.vel = base.bodyChunks[1].vel + Custom.DegToVec(180f * Mathf.Pow(UnityEngine.Random.value, 3f) * ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f)) * 16f * Mathf.Pow(UnityEngine.Random.value, 0.3f);
					(abstractBugEgg4.realizedObject as EggBugEgg).setRotation = Custom.RNV();
					(abstractBugEgg4.realizedObject as EggBugEgg).swell = 0f;
					(abstractBugEgg4.realizedObject as EggBugEgg).rotVel = Mathf.Lerp(-20f, 20f, UnityEngine.Random.value);
				}
			}
		}
		eggsLeft = 0;
		room.PlaySound(SoundID.Egg_Bug_Drop_Eggs, base.mainBodyChunk);
	}

	public override void Update(bool eu)
	{
		if (room.game.devToolsActive && Input.GetKey("b") && room.game.cameras[0].room == room)
		{
			base.bodyChunks[0].vel += Custom.DirVec(base.bodyChunks[0].pos, (Vector2)Futile.mousePosition + room.game.cameras[0].pos) * 14f;
			Stun(12);
		}
		if (!base.dead && State.health < 0f && UnityEngine.Random.value < 0f - State.health && UnityEngine.Random.value < 0.025f)
		{
			Die();
		}
		if (!base.dead && UnityEngine.Random.value * 0.7f > State.health && UnityEngine.Random.value < 0.125f)
		{
			Stun(UnityEngine.Random.Range(1, UnityEngine.Random.Range(1, 27 - Custom.IntClamp((int)(20f * State.health), 0, 10))));
		}
		if (!base.dead && State.health > 0f && State.health < 1f && UnityEngine.Random.value < 0.02f && poison < 0.1f)
		{
			State.health = Mathf.Min(1f, State.health + 1f / Mathf.Lerp(140f, 50f, State.health));
		}
		if (outOfWaterFooting > 0)
		{
			outOfWaterFooting--;
		}
		if (noJumps > 0 && Footing)
		{
			noJumps--;
		}
		if (dropEggs && room != null && (base.dead || (FireBug && base.LickedByPlayer != null)))
		{
			DropEggs();
		}
		if (FireBug)
		{
			if (eggsLeft == 0)
			{
				timeWithoutEggs++;
			}
			if (dropSpears && base.dead && room != null)
			{
				DropSpears();
			}
			if (eggsLeft < 6 && !base.dead)
			{
				if (bleedSmoke == null)
				{
					bleedSmoke = new FireSmoke(room);
				}
			}
			else if (bleedSmoke != null)
			{
				bleedSmoke = null;
			}
			if (bleedSmoke != null)
			{
				bleedSmoke.Update(eu);
				if (room.ViewedByAnyCamera(base.firstChunk.pos, 300f))
				{
					bleedSmoke.EmitSmoke(base.mainBodyChunk.pos, Custom.RNV(), Custom.HSL2RGB(Custom.Decimal(hue + EggBugGraphics.HUE_OFF), 1f, 0.5f), 25);
				}
				if (bleedSmoke.Dead)
				{
					bleedSmoke = null;
				}
			}
		}
		if (!base.dead && base.stun > UnityEngine.Random.Range(20, 80))
		{
			shake = Math.Max(shake, 10);
			for (int i = 0; i < base.bodyChunks.Length; i++)
			{
				if (base.bodyChunks[i].ContactPoint.x != 0 || base.bodyChunks[i].ContactPoint.y != 0)
				{
					base.bodyChunks[i].vel += (Custom.RNV() - base.bodyChunks[i].ContactPoint.ToVector2()) * UnityEngine.Random.value * 3f;
				}
			}
		}
		if (shake > 0)
		{
			shake--;
			if (!base.dead)
			{
				for (int j = 0; j < base.bodyChunks.Length; j++)
				{
					if (room.aimap.TileAccessibleToCreature(base.bodyChunks[j].pos, base.Template))
					{
						base.bodyChunks[j].vel += Custom.RNV() * 2f;
					}
				}
			}
		}
		base.Update(eu);
		if (base.graphicsModule != null && room != null && Footing && !room.aimap.TileAccessibleToCreature(base.mainBodyChunk.pos, base.Template) && !room.aimap.TileAccessibleToCreature(base.bodyChunks[1].pos, base.Template))
		{
			for (int k = 0; k < 2; k++)
			{
				for (int l = 0; l < 2; l++)
				{
					if ((base.graphicsModule as EggBugGraphics).legs[k, l].reachedSnapPosition && UnityEngine.Random.value < 0.5f && !Custom.DistLess(base.mainBodyChunk.pos, (base.graphicsModule as EggBugGraphics).legs[k, l].absoluteHuntPos, (base.graphicsModule as EggBugGraphics).legLength) && Custom.DistLess(base.mainBodyChunk.pos, (base.graphicsModule as EggBugGraphics).legs[k, l].absoluteHuntPos, (base.graphicsModule as EggBugGraphics).legLength + 15f))
					{
						Vector2 vector = Custom.DirVec(base.mainBodyChunk.pos, (base.graphicsModule as EggBugGraphics).legs[k, l].absoluteHuntPos) * (Vector2.Distance(base.mainBodyChunk.pos, (base.graphicsModule as EggBugGraphics).legs[k, l].absoluteHuntPos) - (base.graphicsModule as EggBugGraphics).legLength);
						base.mainBodyChunk.pos += vector;
						base.mainBodyChunk.vel += vector;
					}
				}
			}
		}
		if (room == null)
		{
			return;
		}
		sitting = false;
		currentlyClimbingCorridor = false;
		antennaAttention = Mathf.Max(0f, antennaAttention - 1f / 60f);
		if (grabbedBy.Count > 0)
		{
			if (!base.dead)
			{
				for (int m = 0; m < base.bodyChunks.Length; m++)
				{
					base.bodyChunks[m].vel += Custom.RNV() * 2f;
				}
				AI.Update();
			}
			footingCounter = 0;
			travelDir *= 0f;
		}
		if (base.Consious)
		{
			footingCounter++;
			Act();
		}
		else
		{
			footingCounter = 0;
		}
		if (FireBug)
		{
			if (base.grasps[0] != null)
			{
				CarryObject(eu);
			}
			for (int n = 0; n < 2; n++)
			{
				for (int num = 0; num < 3; num++)
				{
					int num2 = n * 3 + num;
					if (eggsLeft <= num2)
					{
						if (stabSpine[n, num])
						{
							spineExtensions[n, num] = Mathf.Lerp(spineExtensions[n, num], 0f, 0.5f);
							if (spineExtensions[n, num] <= 0.2f)
							{
								stabSpine[n, num] = false;
							}
						}
						else
						{
							spineExtensions[n, num] = Mathf.Lerp(spineExtensions[n, num], 1f, 0.25f);
						}
					}
					if (base.dead)
					{
						spineExtensions[n, num] = 0f;
					}
				}
			}
		}
		if (Footing)
		{
			for (int num3 = 0; num3 < 2; num3++)
			{
				base.bodyChunks[num3].vel *= 0.8f;
				base.bodyChunks[num3].vel.y += base.gravity;
			}
		}
		travelDir *= (sitting ? 0.5f : 0.995f);
		if (!base.Consious || Footing || !(AI.behavior == EggBugAI.Behavior.Flee))
		{
			return;
		}
		for (int num4 = 0; num4 < 2; num4++)
		{
			if (room.aimap.TileAccessibleToCreature(room.GetTilePosition(base.bodyChunks[num4].pos), base.Template))
			{
				base.bodyChunks[num4].vel += Custom.DegToVec(UnityEngine.Random.value * 360f) * UnityEngine.Random.value * 5f;
			}
		}
	}

	private void Swim()
	{
		base.bodyChunks[0].vel *= 1f - 0.05f * base.bodyChunks[0].submersion;
		base.bodyChunks[1].vel *= 1f - 0.1f * base.bodyChunks[1].submersion;
		base.GoThroughFloors = true;
		MovementConnection movementConnection = (AI.pathFinder as StandardPather).FollowPath(room.GetWorldCoordinate(base.bodyChunks[0].pos), actuallyFollowingThisPath: true);
		if (movementConnection == default(MovementConnection))
		{
			movementConnection = (AI.pathFinder as StandardPather).FollowPath(room.GetWorldCoordinate(base.bodyChunks[1].pos), actuallyFollowingThisPath: true);
		}
		if (movementConnection == default(MovementConnection) && Math.Abs(base.abstractCreature.pos.y - room.defaultWaterLevel) < 4)
		{
			movementConnection = (AI.pathFinder as StandardPather).FollowPath(new WorldCoordinate(base.abstractCreature.pos.room, base.abstractCreature.pos.x, room.defaultWaterLevel, base.abstractCreature.pos.abstractNode), actuallyFollowingThisPath: true);
		}
		if (base.safariControlled && (movementConnection == default(MovementConnection) || !AllowableControlledAIOverride(movementConnection.type)))
		{
			movementConnection = default(MovementConnection);
			if (inputWithDiagonals.HasValue)
			{
				MovementConnection.MovementType type = MovementConnection.MovementType.Standard;
				if (room.GetTile(base.mainBodyChunk.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
				{
					type = MovementConnection.MovementType.ShortCut;
				}
				if (inputWithDiagonals.Value.AnyDirectionalInput)
				{
					movementConnection = new MovementConnection(type, room.GetWorldCoordinate(base.mainBodyChunk.pos), room.GetWorldCoordinate(base.mainBodyChunk.pos + new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y) * 40f), 2);
				}
			}
		}
		if (movementConnection != default(MovementConnection))
		{
			if (base.graphicsModule != null)
			{
				(base.graphicsModule as EggBugGraphics).flip = Mathf.Lerp((base.graphicsModule as EggBugGraphics).flip, Mathf.Sign(room.MiddleOfTile(movementConnection.StartTile).x - room.MiddleOfTile(movementConnection.DestTile).x), 0.25f);
				for (int i = 0; i < 2; i++)
				{
					for (int j = 0; j < 2; j++)
					{
						(base.graphicsModule as EggBugGraphics).legs[i, j].vel += Custom.DirVec(base.bodyChunks[1].pos, base.mainBodyChunk.pos) * Mathf.Lerp(-10f, 10f, UnityEngine.Random.value);
					}
				}
			}
			if (movementConnection.StartTile.y == movementConnection.DestTile.y && movementConnection.DestTile.y == room.defaultWaterLevel)
			{
				base.mainBodyChunk.vel.x -= Mathf.Sign(room.MiddleOfTile(movementConnection.StartTile).x - room.MiddleOfTile(movementConnection.DestTile).x) * 1.6f * base.bodyChunks[0].submersion;
				base.bodyChunks[1].vel.x += Mathf.Sign(room.MiddleOfTile(movementConnection.StartTile).x - room.MiddleOfTile(movementConnection.DestTile).x) * 0.5f * base.bodyChunks[1].submersion;
				footingCounter = 0;
				return;
			}
			base.bodyChunks[0].vel *= 0.9f;
			base.mainBodyChunk.vel += Custom.DirVec(base.mainBodyChunk.pos, room.MiddleOfTile(movementConnection.destinationCoord)) * 1.4f;
			if (!base.safariControlled || base.Submersion < 0.5f)
			{
				footingCounter = Math.Max(footingCounter, 25);
				Run(movementConnection);
				outOfWaterFooting = 20;
			}
			else
			{
				base.mainBodyChunk.vel *= 0.75f;
				footingCounter = 0;
				Run(movementConnection);
				outOfWaterFooting = 0;
			}
		}
		else
		{
			base.mainBodyChunk.vel.y += 0.5f;
		}
	}

	private void Act()
	{
		if (base.Submersion > 0.3f)
		{
			Swim();
			AI.Update();
			return;
		}
		if (specialMoveCounter > 0)
		{
			specialMoveCounter--;
			MoveTowards(room.MiddleOfTile(specialMoveDestination));
			travelDir = Vector2.Lerp(travelDir, Custom.DirVec(base.mainBodyChunk.pos, room.MiddleOfTile(specialMoveDestination)), 0.4f);
			if (Custom.DistLess(base.mainBodyChunk.pos, room.MiddleOfTile(specialMoveDestination), 5f))
			{
				specialMoveCounter = 0;
			}
		}
		else
		{
			if (!room.aimap.TileAccessibleToCreature(base.mainBodyChunk.pos, base.Template) && !room.aimap.TileAccessibleToCreature(base.bodyChunks[1].pos, base.Template))
			{
				footingCounter = Custom.IntClamp(footingCounter - 3, 0, 35);
			}
			if (!base.safariControlled && ((FireBug && base.grasps[0] != null) || ((!FireBug || eggsLeft > 0) && (room.GetWorldCoordinate(base.mainBodyChunk.pos) == AI.pathFinder.GetDestination || room.GetWorldCoordinate(base.bodyChunks[1].pos) == AI.pathFinder.GetDestination) && AI.threatTracker.Utility() < 0.5f)))
			{
				sitting = true;
				base.GoThroughFloors = false;
			}
			else
			{
				MovementConnection movementConnection = (AI.pathFinder as StandardPather).FollowPath(room.GetWorldCoordinate(base.mainBodyChunk.pos), actuallyFollowingThisPath: true);
				if (movementConnection == default(MovementConnection))
				{
					movementConnection = (AI.pathFinder as StandardPather).FollowPath(room.GetWorldCoordinate(base.bodyChunks[1].pos), actuallyFollowingThisPath: true);
				}
				if (base.safariControlled && (movementConnection == default(MovementConnection) || !AllowableControlledAIOverride(movementConnection.type)))
				{
					movementConnection = default(MovementConnection);
					if (inputWithDiagonals.HasValue)
					{
						MovementConnection.MovementType type = MovementConnection.MovementType.Standard;
						if (room.GetTile(base.mainBodyChunk.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
						{
							type = MovementConnection.MovementType.ShortCut;
						}
						if (inputWithDiagonals.Value.AnyDirectionalInput)
						{
							movementConnection = new MovementConnection(type, room.GetWorldCoordinate(base.mainBodyChunk.pos), room.GetWorldCoordinate(base.mainBodyChunk.pos + new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y) * 40f), 2);
						}
						if (inputWithDiagonals.Value.y < 0)
						{
							base.GoThroughFloors = true;
						}
						else
						{
							base.GoThroughFloors = false;
						}
						if (inputWithDiagonals.Value.pckp)
						{
							sitting = true;
							base.GoThroughFloors = false;
						}
						if (inputWithDiagonals.Value.jmp && !lastInputWithDiagonals.Value.jmp)
						{
							Vector2 vector = travelDir * 40f;
							if (inputWithDiagonals.Value.AnyDirectionalInput)
							{
								vector = new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y) * 40f;
							}
							vector.y = 0f;
							TryJump(base.mainBodyChunk.pos + vector);
						}
					}
				}
				if (movementConnection != default(MovementConnection))
				{
					Run(movementConnection);
					travelDir = Vector2.Lerp(travelDir, Custom.DirVec(base.mainBodyChunk.pos, room.MiddleOfTile(movementConnection.destinationCoord)), 0.4f);
				}
				else
				{
					base.GoThroughFloors = false;
				}
			}
		}
		if (FireBug && eggsLeft <= 0 && base.grasps[0] == null)
		{
			sitting = false;
		}
		AI.Update();
		float num = runCycle;
		if (base.Consious && !Custom.DistLess(base.mainBodyChunk.pos, base.mainBodyChunk.lastPos, 5f))
		{
			runCycle += runSpeed / 10f;
		}
		if (num < Mathf.Floor(runCycle))
		{
			room.PlaySound(SoundID.Egg_Bug_Scurry, base.mainBodyChunk);
		}
		if (sitting)
		{
			Vector2 vector2 = new Vector2(0f, 0f);
			for (int i = 0; i < 8; i++)
			{
				if (room.GetTile(base.abstractCreature.pos.Tile + Custom.eightDirections[i]).Solid)
				{
					vector2 -= Custom.eightDirections[i].ToVector2();
				}
			}
			awayFromTerrainDir = Vector2.Lerp(awayFromTerrainDir, vector2.normalized, 0.1f);
		}
		else
		{
			awayFromTerrainDir *= 0.7f;
		}
	}

	public void TryJump(Vector2 awayFromPoint)
	{
		if (base.Consious)
		{
			Squirt(0.5f + 0.5f * AI.fear);
		}
		if (base.Consious && noJumps <= 0 && (room.aimap.TileAccessibleToCreature(base.bodyChunks[0].pos, base.Template) || room.aimap.TileAccessibleToCreature(base.bodyChunks[1].pos, base.Template)) && !room.aimap.getAItile(base.bodyChunks[1].pos).narrowSpace)
		{
			room.PlaySound(SoundID.Egg_Bug_Scurry, base.mainBodyChunk);
			Vector2 vector = Custom.DirVec(awayFromPoint, (base.bodyChunks[0].pos + base.bodyChunks[1].pos) / 2f);
			vector += Custom.RNV() * 0.3f;
			vector.Normalize();
			vector = Vector3.Slerp(vector, new Vector2(0f, 1f), Custom.LerpMap(vector.y, -0.5f, 0.5f, 0.7f, 0.3f));
			base.bodyChunks[0].vel *= 0.5f;
			base.bodyChunks[1].vel *= 0.5f;
			base.bodyChunks[0].vel += vector * 17f + Custom.RNV() * 5f * UnityEngine.Random.value;
			base.bodyChunks[1].vel += vector * 17f + Custom.RNV() * 5f * UnityEngine.Random.value;
			footingCounter = 0;
			Vector2 vector2 = Custom.PerpendicularVector(vector) * ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f);
			base.bodyChunks[0].vel += vector2 * 11f;
			base.bodyChunks[1].vel -= vector2 * 11f;
			if (!base.safariControlled)
			{
				noJumps = 90;
			}
		}
	}

	public void Squirt(float intensity)
	{
		if (base.graphicsModule != null && (!FireBug || eggsLeft > 0))
		{
			(base.graphicsModule as EggBugGraphics).Squirt(intensity);
		}
	}

	private void Run(MovementConnection followingConnection)
	{
		if (followingConnection.destinationCoord.y > followingConnection.startCoord.y && room.aimap.getAItile(followingConnection.destinationCoord).acc != AItile.Accessibility.Climb)
		{
			currentlyClimbingCorridor = true;
		}
		if (followingConnection.type == MovementConnection.MovementType.ReachUp)
		{
			(AI.pathFinder as StandardPather).pastConnections.Clear();
		}
		if (followingConnection.type == MovementConnection.MovementType.ShortCut || followingConnection.type == MovementConnection.MovementType.NPCTransportation)
		{
			enteringShortCut = followingConnection.StartTile;
			if (base.abstractCreature.controlled)
			{
				bool flag = false;
				List<IntVector2> list = new List<IntVector2>();
				ShortcutData[] shortcuts = room.shortcuts;
				for (int i = 0; i < shortcuts.Length; i++)
				{
					ShortcutData shortcutData = shortcuts[i];
					if (shortcutData.shortCutType == ShortcutData.Type.NPCTransportation && shortcutData.StartTile != followingConnection.StartTile)
					{
						list.Add(shortcutData.StartTile);
					}
					if (shortcutData.shortCutType == ShortcutData.Type.NPCTransportation && shortcutData.StartTile == followingConnection.StartTile)
					{
						flag = true;
					}
				}
				if (flag)
				{
					if (list.Count > 0)
					{
						list.Shuffle();
						NPCTransportationDestination = room.GetWorldCoordinate(list[0]);
					}
					else
					{
						NPCTransportationDestination = followingConnection.destinationCoord;
					}
				}
			}
			else if (followingConnection.type == MovementConnection.MovementType.NPCTransportation)
			{
				NPCTransportationDestination = followingConnection.destinationCoord;
			}
		}
		else if (followingConnection.type == MovementConnection.MovementType.OpenDiagonal || followingConnection.type == MovementConnection.MovementType.ReachOverGap || followingConnection.type == MovementConnection.MovementType.ReachUp || followingConnection.type == MovementConnection.MovementType.ReachDown || followingConnection.type == MovementConnection.MovementType.SemiDiagonalReach)
		{
			specialMoveCounter = 30;
			specialMoveDestination = followingConnection.DestTile;
		}
		else
		{
			Vector2 vector = room.MiddleOfTile(followingConnection.DestTile);
			_ = lastFollowedConnection;
			if (lastFollowedConnection.type == MovementConnection.MovementType.ReachUp)
			{
				base.mainBodyChunk.vel += Custom.DirVec(base.mainBodyChunk.pos, vector) * 4f;
			}
			if (Footing)
			{
				for (int j = 0; j < 2; j++)
				{
					if (followingConnection.startCoord.x == followingConnection.destinationCoord.x)
					{
						base.bodyChunks[j].vel.x += Mathf.Min((vector.x - base.bodyChunks[j].pos.x) / 8f, 1.2f);
					}
					else if (followingConnection.startCoord.y == followingConnection.destinationCoord.y)
					{
						base.bodyChunks[j].vel.y += Mathf.Min((vector.y - base.bodyChunks[j].pos.y) / 8f, 1.2f);
					}
				}
			}
			_ = lastFollowedConnection;
			if ((Footing || room.aimap.TileAccessibleToCreature(base.mainBodyChunk.pos, base.Template)) && ((followingConnection.startCoord.x != followingConnection.destinationCoord.x && lastFollowedConnection.startCoord.x == lastFollowedConnection.destinationCoord.x) || (followingConnection.startCoord.y != followingConnection.destinationCoord.y && lastFollowedConnection.startCoord.y == lastFollowedConnection.destinationCoord.y)))
			{
				base.mainBodyChunk.vel *= 0.7f;
				base.bodyChunks[1].vel *= 0.5f;
			}
			if (followingConnection.type == MovementConnection.MovementType.DropToFloor)
			{
				footingCounter = 0;
			}
			MoveTowards(vector);
		}
		lastFollowedConnection = followingConnection;
	}

	private void MoveTowards(Vector2 moveTo)
	{
		Vector2 vector = Custom.DirVec(base.mainBodyChunk.pos, moveTo);
		if (!Footing)
		{
			vector *= 0.3f;
		}
		if (IsTileSolid(1, 0, -1) && (((double)vector.x < -0.5 && base.mainBodyChunk.pos.x > base.bodyChunks[1].pos.x + 5f) || ((double)vector.x > 0.5 && base.mainBodyChunk.pos.x < base.bodyChunks[1].pos.x - 5f)))
		{
			base.mainBodyChunk.vel.x -= ((vector.x < 0f) ? (-1f) : 1f) * 1.3f;
			base.bodyChunks[1].vel.x += ((vector.x < 0f) ? (-1f) : 1f) * 0.5f;
			if (!IsTileSolid(0, 0, 1))
			{
				base.mainBodyChunk.vel.y += 3.2f;
			}
		}
		float num = 0.6f;
		if (base.graphicsModule != null)
		{
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 2; j++)
				{
					if ((base.graphicsModule as EggBugGraphics).legs[i, j].OverLappingHuntPos)
					{
						num += 0.1f;
					}
				}
			}
		}
		else
		{
			num = 0.85f;
		}
		num = Mathf.Pow(num, 0.6f);
		base.mainBodyChunk.vel += vector * 6.2f * runSpeed * num;
		base.bodyChunks[1].vel -= vector * 1f * runSpeed * num;
		base.GoThroughFloors = moveTo.y < base.mainBodyChunk.pos.y - 5f;
	}

	public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
	{
		base.Collide(otherObject, myChunk, otherChunk);
		if (otherObject is EggBug && (otherObject as EggBug).FireBug == FireBug)
		{
			AI.CollideWithKin(otherObject as EggBug);
			if (base.bodyChunks[myChunk].pos.y > otherObject.bodyChunks[otherChunk].pos.y)
			{
				base.bodyChunks[myChunk].vel.y += 2f;
				otherObject.bodyChunks[otherChunk].vel.y -= 2f;
			}
		}
	}

	public override void Die()
	{
		base.Die();
		if (dropEggs && room != null)
		{
			DropEggs();
		}
		if (FireBug && dropSpears && room != null)
		{
			DropSpears();
		}
	}

	public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos hitAppendage, DamageType type, float damage, float stunBonus)
	{
		if (!FireBug || timeWithoutEggs >= 10)
		{
			base.Violence(source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
		}
		if (damage > 0.05f || (FireBug && (type == DamageType.Blunt || type == DamageType.Stab)))
		{
			Squirt(Mathf.InverseLerp(0.2f, 1f, damage));
			if (FireBug && dropEggs && room != null)
			{
				DropEggs();
			}
		}
	}

	public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
	{
		base.TerrainImpact(chunk, direction, speed, firstContact);
		if (speed > 2f && UnityEngine.Random.value * 7f < speed)
		{
			Squirt(Mathf.InverseLerp(2f, 14f, speed));
		}
	}

	public override void SpitOutOfShortCut(IntVector2 pos, Room newRoom, bool spitOutAllSticks)
	{
		base.SpitOutOfShortCut(pos, newRoom, spitOutAllSticks);
		Vector2 vector = Custom.IntVector2ToVector2(newRoom.ShorcutEntranceHoleDirection(pos));
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			base.bodyChunks[i].pos = newRoom.MiddleOfTile(pos) - vector * (-1.5f + (float)i) * 15f;
			base.bodyChunks[i].lastPos = newRoom.MiddleOfTile(pos);
			base.bodyChunks[i].vel = vector * 2f;
		}
		if (base.graphicsModule != null)
		{
			base.graphicsModule.Reset();
		}
	}

	public void Suprise(Vector2 surprisePos)
	{
		if (!base.Consious)
		{
			return;
		}
		if (Custom.DistLess(surprisePos, base.mainBodyChunk.pos, 300f))
		{
			for (int i = 0; i < base.bodyChunks.Length; i++)
			{
				if (room.aimap.TileAccessibleToCreature(base.bodyChunks[i].pos, base.Template))
				{
					base.bodyChunks[i].vel += (Custom.RNV() * 4f + Custom.DirVec(surprisePos, base.bodyChunks[i].pos) * 2f) * (0.5f + 0.5f * AI.fear);
				}
			}
		}
		shake = Math.Max(shake, UnityEngine.Random.Range(5, 15));
		AI.fear = Custom.LerpAndTick(AI.fear, 1f, 0.3f, 1f / 7f);
		Squirt(AI.fear);
	}

	public void FlyingWeapon(Weapon weapon)
	{
		bool flag = ((!ModManager.MMF) ? (weapon.firstChunk.pos.x > weapon.firstChunk.lastPos.x && base.firstChunk.pos.x > weapon.firstChunk.pos.x) : ((weapon.firstChunk.pos.x > weapon.firstChunk.lastPos.x && base.firstChunk.pos.x > weapon.firstChunk.pos.x) || (weapon.firstChunk.pos.x < weapon.firstChunk.lastPos.x && base.firstChunk.pos.x < weapon.firstChunk.pos.x)));
		if (flag && Custom.DistLess(base.mainBodyChunk.pos, weapon.firstChunk.pos + new Vector2(Mathf.Sign(weapon.firstChunk.pos.x - weapon.firstChunk.lastPos.x) * 300f, 0f), 300f))
		{
			if (weapon.firstChunk.pos.y <= base.firstChunk.pos.y + 50f && (!FireBug || eggsLeft == 0))
			{
				TryJump(weapon.firstChunk.pos);
			}
			else
			{
				Squirt(1f);
			}
		}
	}

	private void DropSpears()
	{
		dropSpears = false;
		if (base.graphicsModule != null)
		{
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					Vector2 newPos = (base.graphicsModule as EggBugGraphics).EggAttachPos(i, j, 1f);
					AbstractSpear abstractSpear = new AbstractSpear(room.world, null, base.abstractCreature.pos, room.game.GetNewID(), explosive: false, hue);
					room.abstractRoom.AddEntity(abstractSpear);
					abstractSpear.RealizeInRoom();
					abstractSpear.realizedObject.firstChunk.HardSetPosition(newPos);
					abstractSpear.realizedObject.firstChunk.vel = (base.graphicsModule as EggBugGraphics).eggs[i, j].vel + Custom.DegToVec(180f * Mathf.Pow(UnityEngine.Random.value, 3f) * ((UnityEngine.Random.value >= 0.5f) ? 1f : (-1f))) * 16f * Mathf.Pow(UnityEngine.Random.value, 0.3f);
					(abstractSpear.realizedObject as Spear).setRotation = Custom.RNV();
					(abstractSpear.realizedObject as Spear).SetRandomSpin();
				}
			}
		}
		else
		{
			for (int k = 0; k < 6; k++)
			{
				Vector2 newPos2 = base.bodyChunks[1].pos + Custom.RNV() * 4f * UnityEngine.Random.value;
				AbstractSpear abstractSpear2 = new AbstractSpear(room.world, null, base.abstractCreature.pos, room.game.GetNewID(), explosive: false, hue);
				room.abstractRoom.AddEntity(abstractSpear2);
				abstractSpear2.RealizeInRoom();
				abstractSpear2.realizedObject.firstChunk.HardSetPosition(newPos2);
				abstractSpear2.realizedObject.firstChunk.vel = base.bodyChunks[1].vel + Custom.DegToVec(180f * Mathf.Pow(UnityEngine.Random.value, 3f) * ((UnityEngine.Random.value >= 0.5f) ? 1f : (-1f))) * 16f * Mathf.Pow(UnityEngine.Random.value, 0.3f);
				(abstractSpear2.realizedObject as Spear).setRotation = Custom.RNV();
				(abstractSpear2.realizedObject as Spear).SetRandomSpin();
			}
		}
		room.PlaySound(SoundID.Egg_Bug_Drop_Eggs, base.mainBodyChunk);
	}

	public void PopEgg()
	{
		if (eggsLeft > 0)
		{
			eggsLeft--;
			Suprise(base.mainBodyChunk.pos);
		}
	}

	private void CarryObject(bool eu)
	{
		if (base.grasps[0].grabbed.room == null || room == null || base.grasps[0].grabbed.room.abstractRoom.index != room.abstractRoom.index)
		{
			LoseAllGrasps();
			stabCount = 0;
			return;
		}
		if (UnityEngine.Random.value < 0.025f && (!(base.grasps[0].grabbed is Creature) || AI.DynamicRelationship((base.grasps[0].grabbed as Creature).abstractCreature).type != CreatureTemplate.Relationship.Type.Eats))
		{
			LoseAllGrasps();
			stabCount = 0;
			return;
		}
		Vector2 vector = base.bodyChunks[0].pos + Custom.DirVec(base.bodyChunks[1].pos, base.bodyChunks[0].pos) * 1f;
		PhysicalObject grabbed = base.grasps[0].grabbed;
		Vector2 vector2 = grabbed.bodyChunks[base.grasps[0].chunkGrabbed].vel - base.mainBodyChunk.vel;
		float mass = grabbed.bodyChunks[base.grasps[0].chunkGrabbed].mass;
		if (mass <= base.mainBodyChunk.mass / 2f)
		{
			mass /= 2f;
		}
		grabbed.bodyChunks[base.grasps[0].chunkGrabbed].vel = base.mainBodyChunk.vel;
		if (!enteringShortCut.HasValue && (vector2.magnitude * grabbed.bodyChunks[base.grasps[0].chunkGrabbed].mass > 30f || !Custom.DistLess(vector, grabbed.bodyChunks[base.grasps[0].chunkGrabbed].pos, 70f + grabbed.bodyChunks[base.grasps[0].chunkGrabbed].rad)))
		{
			LoseAllGrasps();
			stabCount = 0;
		}
		else
		{
			grabbed.bodyChunks[base.grasps[0].chunkGrabbed].MoveFromOutsideMyUpdate(eu, vector);
		}
		if (base.grasps[0] != null)
		{
			for (int i = 0; i < 2; i++)
			{
				base.grasps[0].grabbed.PushOutOf(base.bodyChunks[i].pos, base.bodyChunks[i].rad, base.grasps[0].chunkGrabbed);
			}
		}
		stabTicker++;
		if (grabbed is Creature && stabTicker >= nextStab)
		{
			int num = UnityEngine.Random.Range(0, 2);
			int num2 = UnityEngine.Random.Range(0, 3);
			if (!stabSpine[num, num2])
			{
				stabSpine[num, num2] = true;
				if (stabCount < 2 || UnityEngine.Random.value < 0.5f)
				{
					(grabbed as Creature).Violence(base.mainBodyChunk, Custom.DirVec(base.mainBodyChunk.pos, grabbed.bodyChunks[base.grasps[0].chunkGrabbed].pos) * 2.5f, grabbed.bodyChunks[base.grasps[0].chunkGrabbed], null, DamageType.Stab, 0.5f, 0f);
				}
				else
				{
					(grabbed as Creature).Violence(base.mainBodyChunk, Custom.DirVec(base.mainBodyChunk.pos, grabbed.bodyChunks[base.grasps[0].chunkGrabbed].pos) * 10f, grabbed.bodyChunks[base.grasps[0].chunkGrabbed], null, DamageType.Stab, 1.5f, 0f);
				}
				room.AddObject(new CreatureSpasmer(grabbed.bodyChunks[base.grasps[0].chunkGrabbed].owner as Creature, allowDead: false, 54));
				room.AddObject(new CreatureSpasmer(this, allowDead: true, 164));
				base.firstChunk.vel += Custom.RNV() * 5f;
				room.PlaySound(SoundID.Spear_Stick_In_Creature, base.firstChunk);
				if (room.BeingViewed)
				{
					for (int j = 0; j < 8; j++)
					{
						room.AddObject(new WaterDrip(grabbed.bodyChunks[base.grasps[0].chunkGrabbed].pos, -base.firstChunk.vel * UnityEngine.Random.value * 0.5f + Custom.DegToVec(360f * UnityEngine.Random.value) * base.firstChunk.vel.magnitude * UnityEngine.Random.value * 0.5f, waterColor: false));
					}
				}
				nextStab = UnityEngine.Random.Range(3, 30);
				stabTicker = 0;
				stabCount++;
			}
		}
		if (grabbed is Creature && (grabbed as Creature).dead)
		{
			overdeadTicker++;
			if (overdeadTicker > 180)
			{
				LoseAllGrasps();
				stabCount = 0;
				overdeadTicker = 0;
			}
		}
	}
}
