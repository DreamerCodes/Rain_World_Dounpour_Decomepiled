using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CoralBrain;
using Expedition;
using HUD;
using JollyCoop;
using JollyCoop.JollyMenu;
using MoreSlugcats;
using Noise;
using RWCustom;
using UnityEngine;

public class Player : Creature, IOwnAHUD
{
	public class Tongue
	{
		public class Mode : ExtEnum<Mode>
		{
			public static readonly Mode Retracted = new Mode("Retracted", register: true);

			public static readonly Mode ShootingOut = new Mode("ShootingOut", register: true);

			public static readonly Mode AttachedToTerrain = new Mode("AttachedToTerrain", register: true);

			public static readonly Mode AttachedToObject = new Mode("AttachedToObject", register: true);

			public static readonly Mode Retracting = new Mode("Retracting", register: true);

			public Mode(string value, bool register = false)
				: base(value, register)
			{
			}
		}

		private IntVector2[] _cachedRtList = new IntVector2[100];

		public List<PlacedObject> noSpearStickZones;

		public float onRopePos = 1f;

		public int attachedTime;

		public Vector2 pos;

		public Vector2 lastPos;

		public Vector2 vel;

		public BodyChunk baseChunk;

		public Player player;

		public int tongueNum;

		public Vector2 terrainStuckPos;

		public BodyChunk attachedChunk;

		public float myMass = 0.1f;

		public bool returning;

		public float requestedRopeLength;

		public float idealRopeLength;

		public float baseIdealRopeLength;

		public float minRopeLength;

		public float maxRopeLength;

		public float elastic;

		public bool disableStick;

		public Rope rope;

		public Mode mode;

		public bool Free
		{
			get
			{
				if (!(mode == Mode.ShootingOut))
				{
					return mode == Mode.Retracting;
				}
				return true;
			}
		}

		public bool Attached
		{
			get
			{
				if (!(mode == Mode.AttachedToTerrain))
				{
					return mode == Mode.AttachedToObject;
				}
				return true;
			}
		}

		public Vector2 AttachedPos => terrainStuckPos;

		public float totalRope => 200f;

		public Tongue(Player player, int tongueNum)
		{
			this.player = player;
			this.tongueNum = tongueNum;
			mode = Mode.Retracted;
			baseChunk = player.mainBodyChunk;
			idealRopeLength = 150f;
			baseIdealRopeLength = 150f;
			minRopeLength = 50f;
			maxRopeLength = 170f;
			rope = new Rope(this.player.room, baseChunk.pos, pos, 1f);
			noSpearStickZones = new List<PlacedObject>();
		}

		public void decreaseRopeLength(float amount)
		{
			idealRopeLength = Mathf.Clamp(idealRopeLength - amount, minRopeLength, maxRopeLength);
		}

		public void increaseRopeLength(float amount)
		{
			idealRopeLength = Mathf.Clamp(idealRopeLength + amount, minRopeLength, maxRopeLength);
		}

		public void resetRopeLength()
		{
			idealRopeLength = baseIdealRopeLength;
		}

		public void NewRoom(Room newRoom)
		{
			resetRopeLength();
			mode = Mode.Retracted;
			rope = new Rope(newRoom, baseChunk.pos, pos, 1f);
			noSpearStickZones.Clear();
			for (int i = 0; i < newRoom.roomSettings.placedObjects.Count; i++)
			{
				if (newRoom.roomSettings.placedObjects[i].type == PlacedObject.Type.NoSpearStickZone)
				{
					noSpearStickZones.Add(newRoom.roomSettings.placedObjects[i]);
				}
			}
		}

		public bool isZeroGMode()
		{
			if (player.bodyMode == BodyModeIndex.ZeroG)
			{
				if (player.room != null && !(player.room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.BrokenZeroG) <= 0f) && player.room.world.rainCycle.brokenAntiGrav != null)
				{
					return player.room.world.rainCycle.brokenAntiGrav.CurrentLightsOn >= 1f;
				}
				return true;
			}
			return false;
		}

		public void Update()
		{
			_ = pos;
			lastPos = pos;
			pos += vel;
			if (mode == Mode.AttachedToTerrain && player.room != null)
			{
				for (int i = 0; i < player.room.zapCoils.Count; i++)
				{
					ZapCoil zapCoil = player.room.zapCoils[i];
					if (zapCoil.turnedOn > 0.5f && zapCoil.GetFloatRect.Vector2Inside(terrainStuckPos))
					{
						zapCoil.TriggerZap(terrainStuckPos, 4f);
						player.mainBodyChunk.vel = Custom.DegToVec(Custom.AimFromOneVectorToAnother(terrainStuckPos, player.mainBodyChunk.pos)).normalized * 12f;
						Release();
						player.room.AddObject(new ZapCoil.ZapFlash(player.mainBodyChunk.pos, 10f));
						player.Die();
					}
				}
			}
			if (Attached && disableStick)
			{
				Release();
			}
			if (isZeroGMode() && Attached && attachedTime < 3)
			{
				Release();
			}
			if (Attached)
			{
				attachedTime++;
			}
			else
			{
				attachedTime = 0;
			}
			if (mode == Mode.Retracted)
			{
				requestedRopeLength = 0f;
				pos = player.mainBodyChunk.pos;
				vel = player.mainBodyChunk.vel;
				rope.Reset();
			}
			else if (mode == Mode.ShootingOut)
			{
				requestedRopeLength = Mathf.Max(0f, requestedRopeLength - 4f);
				bool flag = false;
				if (!Custom.DistLess(baseChunk.pos, pos, 60f))
				{
					Vector2 vector = pos + vel;
					SharedPhysics.CollisionResult collisionResult = SharedPhysics.TraceProjectileAgainstBodyChunks(null, player.room, pos, ref vector, 1f, 1, baseChunk.owner, hitAppendages: false);
					if (collisionResult.chunk != null)
					{
						AttachToChunk(collisionResult.chunk);
						flag = true;
					}
				}
				if (!flag)
				{
					IntVector2? intVector = SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(player.room, lastPos, pos);
					if (intVector.HasValue)
					{
						FloatRect floatRect = Custom.RectCollision(pos, lastPos, player.room.TileRect(intVector.Value).Grow(1f));
						AttachToTerrain(new Vector2(floatRect.left, floatRect.bottom));
					}
					else
					{
						vel.y -= 0.9f * Mathf.InverseLerp(0.8f, 0f, elastic);
						if (returning)
						{
							pos += Custom.RNV() / 1000f;
							int num;
							for (num = SharedPhysics.RayTracedTilesArray(lastPos, pos, _cachedRtList); num >= _cachedRtList.Length; num = SharedPhysics.RayTracedTilesArray(lastPos, pos, _cachedRtList))
							{
								Custom.LogWarning($"Saint tongue ray tracing limit exceeded, extending cache to {_cachedRtList.Length + 100} and trying again!");
								Array.Resize(ref _cachedRtList, _cachedRtList.Length + 100);
							}
							for (int j = 0; j < num; j++)
							{
								if (player.room.GetTile(_cachedRtList[j]).horizontalBeam)
								{
									AttachToTerrain(new Vector2(Mathf.Clamp(Custom.HorizontalCrossPoint(lastPos, pos, player.room.MiddleOfTile(_cachedRtList[j]).y).x, player.room.MiddleOfTile(_cachedRtList[j]).x - 10f, player.room.MiddleOfTile(_cachedRtList[j]).x + 10f), player.room.MiddleOfTile(_cachedRtList[j]).y));
									break;
								}
								if (player.room.GetTile(_cachedRtList[j]).verticalBeam)
								{
									AttachToTerrain(new Vector2(player.room.MiddleOfTile(_cachedRtList[j]).x, Mathf.Clamp(Custom.VerticalCrossPoint(lastPos, pos, player.room.MiddleOfTile(_cachedRtList[j]).x).y, player.room.MiddleOfTile(_cachedRtList[j]).y - 10f, player.room.MiddleOfTile(_cachedRtList[j]).y + 10f)));
									break;
								}
							}
							if (Custom.DistLess(baseChunk.pos, pos, 40f))
							{
								mode = Mode.Retracted;
							}
						}
						else if (Vector2.Dot(Custom.DirVec(baseChunk.pos, pos), vel.normalized) < 0f)
						{
							returning = true;
						}
					}
				}
			}
			else if (mode == Mode.AttachedToTerrain)
			{
				pos = terrainStuckPos;
				vel *= 0f;
				if (noSpearStickZones.Count > 0 && UnityEngine.Random.value < 0.1f)
				{
					for (int k = 0; k < noSpearStickZones.Count; k++)
					{
						if (Custom.DistLess(pos, noSpearStickZones[k].pos, (noSpearStickZones[k].data as PlacedObject.ResizableObjectData).Rad))
						{
							Release();
							break;
						}
					}
				}
			}
			else if (mode == Mode.AttachedToObject)
			{
				if (attachedChunk != null)
				{
					pos = attachedChunk.pos;
					vel = attachedChunk.vel;
					if (attachedChunk.owner.room != player.room)
					{
						attachedChunk = null;
						mode = Mode.Retracting;
					}
				}
				else
				{
					mode = Mode.Retracting;
				}
			}
			else if (mode == Mode.Retracting)
			{
				mode = Mode.Retracted;
			}
			rope.Update(baseChunk.pos, pos);
			if (mode != Mode.Retracted)
			{
				Elasticity();
			}
			if (Attached)
			{
				elastic = Mathf.Max(0f, elastic - 0.05f);
				if (requestedRopeLength < idealRopeLength)
				{
					requestedRopeLength = Mathf.Min(requestedRopeLength + (1f - elastic) * 2f, idealRopeLength);
				}
				else if (requestedRopeLength > idealRopeLength)
				{
					requestedRopeLength = Mathf.Max(requestedRopeLength - (1f - elastic) * 2f, idealRopeLength);
				}
			}
		}

		private float GetTargetZeroGVelo(float baseV, float incV)
		{
			if (baseV < 0f)
			{
				return Mathf.Max(baseV + incV, -4f);
			}
			return Mathf.Min(baseV + incV, 4f);
		}

		public void Shoot(Vector2 dir)
		{
			resetRopeLength();
			if (Attached)
			{
				Release();
			}
			else if ((!ModManager.Expedition || !player.room.game.rainWorld.ExpeditionMode || !ExpeditionGame.activeUnlocks.Contains("unl-explosivejump") || (!player.input[0].pckp && !player.input[1].pckp)) && !(mode != Mode.Retracted))
			{
				mode = Mode.ShootingOut;
				player.room.PlaySound(SoundID.Tube_Worm_Shoot_Tongue, baseChunk);
				float num = player.input[0].x;
				float num2 = player.input[0].y;
				if (isZeroGMode() && (num != 0f || num2 != 0f))
				{
					dir = new Vector2(num, num2).normalized;
					player.bodyChunks[0].vel = new Vector2(GetTargetZeroGVelo(player.bodyChunks[0].vel.x, dir.x), GetTargetZeroGVelo(player.bodyChunks[0].vel.y, dir.y));
					player.bodyChunks[1].vel = new Vector2(GetTargetZeroGVelo(player.bodyChunks[1].vel.x, dir.x), GetTargetZeroGVelo(player.bodyChunks[1].vel.y, dir.y));
				}
				else
				{
					dir = AutoAim(dir);
				}
				pos = baseChunk.pos + dir * 5f;
				vel = dir * 70f;
				elastic = 1f;
				requestedRopeLength = 140f;
				returning = false;
			}
		}

		private Vector2 AutoAim(Vector2 originalDir)
		{
			float num = 230f;
			if (!SharedPhysics.RayTraceTilesForTerrain(player.room, baseChunk.pos, baseChunk.pos + originalDir * num))
			{
				return originalDir;
			}
			float num2 = Custom.VecToDeg(originalDir);
			for (float num3 = 5f; num3 < 30f; num3 += 5f)
			{
				for (float num4 = -1f; num4 <= 1f; num4 += 2f)
				{
					if (!SharedPhysics.RayTraceTilesForTerrain(player.room, baseChunk.pos, baseChunk.pos + Custom.DegToVec(num2 + num3 * num4) * num))
					{
						return Custom.DegToVec(num2 + num3 * num4);
					}
				}
			}
			return originalDir;
		}

		public void Release()
		{
			if (mode == Mode.AttachedToObject && attachedChunk != null)
			{
				player.room.PlaySound(SoundID.Tube_Worm_Detatch_Tongue_Creature, pos);
			}
			else if (mode == Mode.AttachedToTerrain)
			{
				player.room.PlaySound(SoundID.Tube_Worm_Detach_Tongue_Terrain, pos);
			}
			if (mode != Mode.Retracted)
			{
				mode = Mode.Retracting;
			}
			attachedChunk = null;
		}

		private void AttachToTerrain(Vector2 attPos)
		{
			if (!disableStick)
			{
				terrainStuckPos = attPos;
				mode = Mode.AttachedToTerrain;
				pos = terrainStuckPos;
				Attatch();
				player.room.PlaySound(SoundID.Tube_Worm_Tongue_Hit_Terrain, pos);
			}
		}

		private void AttachToChunk(BodyChunk chunk)
		{
			if (disableStick)
			{
				return;
			}
			if (chunk.owner is Creature)
			{
				Creature creature = chunk.owner as Creature;
				if (creature.abstractCreature.creatureTemplate.type == CreatureTemplate.Type.GarbageWorm || creature.abstractCreature.creatureTemplate.type == CreatureTemplate.Type.Fly || creature.abstractCreature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.Inspector || creature.abstractCreature.creatureTemplate.type == CreatureTemplate.Type.Overseer)
				{
					return;
				}
			}
			attachedChunk = chunk;
			pos = chunk.pos;
			mode = Mode.AttachedToObject;
			Attatch();
			player.room.PlaySound(SoundID.Tube_Worm_Tongue_Hit_Creature, pos);
		}

		private void Attatch()
		{
			vel *= 0f;
			elastic = 1f;
			requestedRopeLength = Vector2.Distance(baseChunk.pos, pos);
		}

		private void Elasticity()
		{
			float num = 0f;
			if (mode == Mode.AttachedToTerrain)
			{
				num = 1f;
			}
			else if (mode == Mode.AttachedToObject)
			{
				num = attachedChunk.mass / (attachedChunk.mass + baseChunk.mass);
			}
			Vector2 vector = Custom.DirVec(baseChunk.pos, rope.AConnect);
			float totalLength = rope.totalLength;
			float a = 0.7f;
			if (player.tongue.Attached)
			{
				a = Custom.LerpMap(Mathf.Abs(0.5f - onRopePos), 0.5f, 0.4f, 1.1f, 0.7f);
			}
			float num2 = RequestRope() * Mathf.Lerp(a, 1f, elastic);
			float num3 = Mathf.Lerp(0.85f, 0.25f, elastic);
			if (totalLength > num2)
			{
				baseChunk.vel += vector * (totalLength - num2) * num3 * num;
				baseChunk.pos += vector * (totalLength - num2) * num3 * num * Mathf.Lerp(1f, 0.5f, elastic);
				vector = Custom.DirVec(pos, rope.BConnect);
				if (Free)
				{
					vel += vector * (totalLength - num2) * num3 * (1f - num);
					pos += vector * (totalLength - num2) * num3 * (1f - num) * Mathf.Lerp(1f, 0.5f, elastic);
				}
				else if (mode == Mode.AttachedToObject)
				{
					attachedChunk.vel += vector * (totalLength - num2) * num3 * (1f - num);
					attachedChunk.pos += vector * (totalLength - num2) * num3 * (1f - num) * Mathf.Lerp(1f, 0.5f, elastic);
				}
			}
		}

		public float RequestRope()
		{
			if (WeightedRopeRequest() < totalRope)
			{
				return WeightedRopeRequest();
			}
			return totalRope;
		}

		private float WeightedRopeRequest()
		{
			return Mathf.Min(player.tongue.requestedRopeLength, onRopePos * totalRope);
		}
	}

	public class NPCStats
	{
		public float Met;

		public float Bal;

		public float Size;

		public float Stealth;

		public bool Dark;

		public float EyeColor;

		public float H;

		public float S;

		public float L;

		public float Wideness;

		public NPCStats(Player player)
		{
			bool malnourished = false;
			if (player.playerState is PlayerNPCState)
			{
				_ = (player.playerState as PlayerNPCState).isPup;
				malnourished = (player.playerState as PlayerNPCState).Malnourished;
			}
			UnityEngine.Random.State state = UnityEngine.Random.state;
			UnityEngine.Random.InitState(player.abstractCreature.ID.RandomSeed);
			Bal = Mathf.Pow(UnityEngine.Random.Range(0f, 1f), 1.5f);
			Met = Mathf.Pow(UnityEngine.Random.Range(0f, 1f), 1.5f);
			Stealth = Mathf.Pow(UnityEngine.Random.Range(0f, 1f), 1.5f);
			Size = Mathf.Pow(UnityEngine.Random.Range(0f, 1f), 1.5f);
			Wideness = Mathf.Pow(UnityEngine.Random.Range(0f, 1f), 1.5f);
			H = Mathf.Lerp(UnityEngine.Random.Range(0.15f, 0.58f), UnityEngine.Random.value, Mathf.Pow(UnityEngine.Random.value, 1.5f - Met));
			S = Mathf.Pow(UnityEngine.Random.Range(0f, 1f), 0.3f + Stealth * 0.3f);
			Dark = UnityEngine.Random.Range(0f, 1f) <= 0.3f + Stealth * 0.2f;
			L = Mathf.Pow(UnityEngine.Random.Range(Dark ? 0.9f : 0.75f, 1f), 1.5f - Stealth);
			EyeColor = Mathf.Pow(UnityEngine.Random.Range(0f, 1f), 2f - Stealth * 1.5f);
			UnityEngine.Random.state = state;
			if (!player.isNPC)
			{
				Bal = 0.5f;
				Met = 0.5f;
				Stealth = 0.5f;
				Wideness = 0.5f;
				Dark = false;
				EyeColor = 0f;
				H = 23f / 60f;
				S = 0.55f;
				L = 0.84f;
			}
			SlugcatStats slugcatStats = new SlugcatStats(MoreSlugcatsEnums.SlugcatStatsName.Slugpup, malnourished);
			slugcatStats.runspeedFac *= 0.85f + 0.15f * Met + 0.15f * (1f - Bal) + 0.1f * (1f - Stealth);
			slugcatStats.bodyWeightFac *= 0.85f + 0.15f * Wideness + 0.1f * Met;
			slugcatStats.generalVisibilityBonus *= 0.8f + 0.2f * (1f - Stealth) + 0.2f * Met;
			slugcatStats.visualStealthInSneakMode *= 0.75f + 0.35f * Stealth + 0.15f * (1f - Met);
			slugcatStats.loudnessFac *= 0.8f + 0.2f * Wideness + 0.2f * (1f - Stealth);
			slugcatStats.lungsFac *= 0.8f + 0.2f * (1f - Met) + 0.2f * (1f - Stealth);
			slugcatStats.poleClimbSpeedFac *= 0.85f + 0.15f * Met + 0.15f * Bal + 0.1f * (1f - Stealth);
			slugcatStats.corridorClimbSpeedFac *= 0.85f + 0.15f * Met + 0.15f * (1f - Bal) + 0.1f * (1f - Stealth);
			player.npcCharacterStats = slugcatStats;
		}
	}

	public class SlugOnBack
	{
		public Player owner;

		public Player slugcat;

		public bool increment;

		public int counter;

		public bool interactionLocked;

		public AbstractOnBackStick abstractStick;

		public bool HasASlug => slugcat != null;

		public SlugOnBack(Player owner)
		{
			this.owner = owner;
		}

		public void Update(bool eu)
		{
			if (slugcat != null)
			{
				slugcat.GoThroughFloors = true;
				if (ModManager.CoopAvailable && !slugcat.isNPC)
				{
					for (int i = 0; i < slugcat.grasps?.Length; i++)
					{
						if (slugcat.grasps[i]?.grabbed is Player)
						{
							JollyCustom.Log($"Player to back {slugcat} had another player, releasing...");
							slugcat.ReleaseGrasp(i);
						}
					}
				}
			}
			if (increment)
			{
				counter++;
				if (slugcat != null && counter > 20)
				{
					SlugToHand(eu);
					counter = 0;
				}
				else if (slugcat == null && counter > 20)
				{
					for (int j = 0; j < 2; j++)
					{
						if (owner.grasps[j] != null && owner.grasps[j].grabbed is Player)
						{
							owner.bodyChunks[0].pos += Custom.DirVec(owner.grasps[j].grabbed.firstChunk.pos, owner.bodyChunks[0].pos) * 2f;
							SlugToBack(owner.grasps[j].grabbed as Player);
							counter = 0;
							break;
						}
					}
				}
			}
			else
			{
				counter = 0;
			}
			if (!owner.input[0].pckp)
			{
				interactionLocked = false;
			}
			increment = false;
		}

		public void GraphicsModuleUpdated(bool actuallyViewed, bool eu)
		{
			if (slugcat == null)
			{
				return;
			}
			if (owner.slatedForDeletetion || slugcat.slatedForDeletetion || slugcat.grabbedBy.Count > 0 || (!slugcat.Consious && !owner.CanIPutDeadSlugOnBack(slugcat)))
			{
				if (abstractStick != null)
				{
					abstractStick.Deactivate();
				}
				ChangeOverlap(newOverlap: true);
				slugcat = null;
				return;
			}
			ChangeOverlap(newOverlap: false);
			if (!ModManager.CoopAvailable || slugcat.isSlugpup)
			{
				slugcat.bodyChunks[1].MoveFromOutsideMyUpdate(eu, (owner.graphicsModule != null) ? (owner.graphicsModule as PlayerGraphics).head.pos : owner.mainBodyChunk.pos);
				slugcat.bodyChunks[1].vel = owner.mainBodyChunk.vel;
				slugcat.bodyChunks[0].vel = Vector2.Lerp(slugcat.bodyChunks[0].vel, Vector2.Lerp(owner.mainBodyChunk.vel, new Vector2(0f, 5f), 0.5f), 0.3f);
				return;
			}
			Vector2 moveTo = Vector2.Lerp(slugcat.bodyChunks[0].pos, owner.bodyChunks[0].pos + Custom.DirVec(owner.bodyChunks[1].pos, owner.bodyChunks[0].pos) * 14f, 0.75f);
			Vector2 moveTo2 = Vector2.Lerp(slugcat.bodyChunks[1].pos, owner.bodyChunks[1].pos + Custom.DirVec(owner.bodyChunks[1].pos, owner.bodyChunks[0].pos) * 14f, 0.75f);
			slugcat.bodyChunks[0].MoveFromOutsideMyUpdate(eu, moveTo);
			slugcat.bodyChunks[1].MoveFromOutsideMyUpdate(eu, moveTo2);
			slugcat.bodyChunks[1].vel = owner.mainBodyChunk.vel;
			slugcat.bodyChunks[0].vel = Vector2.Lerp(slugcat.bodyChunks[0].vel, Vector2.Lerp(owner.mainBodyChunk.vel, new Vector2(0f, 5f), 0.5f), 0.9f);
		}

		public void SlugToHand(bool eu)
		{
			if (slugcat == null)
			{
				return;
			}
			for (int i = 0; i < 2; i++)
			{
				if (owner.grasps[i] != null && owner.Grabability(owner.grasps[i].grabbed) > ObjectGrabability.BigOneHand)
				{
					return;
				}
			}
			int num = -1;
			for (int j = 0; j < 2; j++)
			{
				if (num != -1)
				{
					break;
				}
				if (owner.grasps[j] == null)
				{
					num = j;
				}
			}
			if (num != -1)
			{
				if (owner.graphicsModule != null)
				{
					slugcat.firstChunk.MoveFromOutsideMyUpdate(eu, (owner.graphicsModule as PlayerGraphics).hands[num].pos);
				}
				ChangeOverlap(newOverlap: true);
				owner.SlugcatGrab(slugcat, num);
				slugcat = null;
				interactionLocked = true;
				owner.noPickUpOnRelease = 20;
				owner.room.PlaySound(SoundID.Slugcat_Pick_Up_Creature, owner.mainBodyChunk);
				if (abstractStick != null)
				{
					abstractStick.Deactivate();
					abstractStick = null;
				}
			}
		}

		public void CheckCircularGrabbing(Player playerToGrab, Player reference, bool slugOnBack)
		{
			if (!slugOnBack)
			{
				for (int i = 0; i < playerToGrab.grasps.Length; i++)
				{
					if (playerToGrab.grasps[i]?.grabbed is Player player)
					{
						if (player != reference)
						{
							CheckCircularGrabbing(player, reference, slugOnBack);
							continue;
						}
						JollyCustom.Log($"Player to back {playerToGrab} had another player, releasing...");
						playerToGrab.ReleaseGrasp(i);
					}
				}
				return;
			}
			SlugOnBack slugOnBack2 = playerToGrab.slugOnBack;
			if (slugOnBack2 != null && slugOnBack2.HasASlug)
			{
				if (playerToGrab.slugOnBack.slugcat != reference)
				{
					CheckCircularGrabbing(playerToGrab.slugOnBack.slugcat, reference, slugOnBack);
				}
				else
				{
					playerToGrab.slugOnBack.DropSlug();
				}
			}
		}

		public void SlugToBack(Player playerToBack)
		{
			if (slugcat != null)
			{
				return;
			}
			for (int i = 0; i < 2; i++)
			{
				if (owner.grasps[i] != null && owner.grasps[i].grabbed == playerToBack)
				{
					owner.ReleaseGrasp(i);
					break;
				}
			}
			if (ModManager.CoopAvailable && !playerToBack.isNPC)
			{
				for (int j = 0; j < playerToBack.grasps.Length; j++)
				{
					if (playerToBack.grasps[j]?.grabbed is Player)
					{
						JollyCustom.Log($"Player to back {playerToBack} had another player, releasing...");
						playerToBack.ReleaseGrasp(j);
					}
				}
				if (playerToBack.grabbedBy != null)
				{
					for (int k = 0; k < playerToBack.grabbedBy.Count; k++)
					{
						if (!(playerToBack.grabbedBy[k].grabber is Player player) || player == owner)
						{
							continue;
						}
						for (int l = 0; l < 2; l++)
						{
							if (player.grasps[l]?.grabbed == playerToBack)
							{
								player.ReleaseGrasp(l);
								break;
							}
						}
					}
				}
				CheckCircularGrabbing(playerToBack, owner, slugOnBack: false);
				CheckCircularGrabbing(playerToBack, owner, slugOnBack: true);
			}
			slugcat = playerToBack;
			ChangeOverlap(newOverlap: false);
			interactionLocked = true;
			owner.noPickUpOnRelease = 20;
			owner.room.PlaySound(SoundID.Slugcat_Pick_Up_Creature, owner.mainBodyChunk);
			if (abstractStick != null)
			{
				abstractStick.Deactivate();
			}
			abstractStick = new AbstractOnBackStick(owner.abstractPhysicalObject, playerToBack.abstractPhysicalObject);
		}

		public void DropSlug()
		{
			if (slugcat != null)
			{
				ChangeOverlap(newOverlap: true);
				slugcat.firstChunk.vel = owner.mainBodyChunk.vel + Custom.RNV() * 3f * UnityEngine.Random.value;
				slugcat.bodyChunks[1].pos += new Vector2(0f, 10f);
				slugcat = null;
				if (abstractStick != null)
				{
					abstractStick.Deactivate();
					abstractStick = null;
				}
			}
		}

		public void ChangeOverlap(bool newOverlap)
		{
			slugcat.CollideWithObjects = newOverlap;
			slugcat.canBeHitByWeapons = newOverlap;
			slugcat.onBack = (newOverlap ? null : owner);
			if (slugcat.graphicsModule != null && owner.room != null)
			{
				for (int i = 0; i < owner.room.game.cameras.Length; i++)
				{
					owner.room.game.cameras[i].MoveObjectToContainer(slugcat.graphicsModule, owner.room.game.cameras[i].ReturnFContainer((!newOverlap) ? "Background" : "Midground"));
				}
			}
		}
	}

	public abstract class PlayerController
	{
		public PlayerController()
		{
		}

		public virtual InputPackage GetInput()
		{
			return new InputPackage(gamePad: false, Options.ControlSetup.Preset.None, 0, 0, jmp: false, thrw: false, pckp: false, mp: false, crouchToggle: false);
		}
	}

	public struct InputPackage
	{
		public int x;

		public int y;

		public bool jmp;

		public bool thrw;

		public bool pckp;

		public bool mp;

		public bool gamePad;

		public Options.ControlSetup.Preset controllerType;

		public bool crouchToggle;

		public Vector2 analogueDir;

		public int downDiagonal;

		public IntVector2 IntVec => new IntVector2(x, y);

		public IntVector2 ZeroGGamePadIntVec
		{
			get
			{
				if (analogueDir.magnitude > 0.2f)
				{
					return new IntVector2((Mathf.Abs(analogueDir.x) > 0.1f) ? ((int)Mathf.Sign(analogueDir.x)) : 0, (Mathf.Abs(analogueDir.y) > 0.1f) ? ((int)Mathf.Sign(analogueDir.y)) : 0);
				}
				return IntVec;
			}
		}

		public bool AnyInput
		{
			get
			{
				if (!AnyDirectionalInput && !jmp && !thrw)
				{
					return pckp;
				}
				return true;
			}
		}

		public bool AnyDirectionalInput
		{
			get
			{
				if (x == 0 && y == 0)
				{
					return analogueDir != Vector2.zero;
				}
				return true;
			}
		}

		public InputPackage(bool gamePad, Options.ControlSetup.Preset controllerType, int x, int y, bool jmp, bool thrw, bool pckp, bool mp, bool crouchToggle)
		{
			this.gamePad = gamePad;
			this.controllerType = controllerType;
			this.x = x;
			this.y = y;
			this.jmp = jmp;
			this.thrw = thrw;
			this.pckp = pckp;
			this.mp = mp;
			this.crouchToggle = crouchToggle;
			analogueDir = new Vector2(0f, 0f);
			downDiagonal = 0;
		}
	}

	public class NullController : PlayerController
	{
		public override InputPackage GetInput()
		{
			return new InputPackage(gamePad: false, Options.ControlSetup.Preset.None, 0, 0, jmp: false, thrw: false, pckp: false, mp: false, crouchToggle: false);
		}
	}

	public class AnimationIndex : ExtEnum<AnimationIndex>
	{
		public static readonly AnimationIndex None = new AnimationIndex("None", register: true);

		public static readonly AnimationIndex CrawlTurn = new AnimationIndex("CrawlTurn", register: true);

		public static readonly AnimationIndex StandUp = new AnimationIndex("StandUp", register: true);

		public static readonly AnimationIndex DownOnFours = new AnimationIndex("DownOnFours", register: true);

		public static readonly AnimationIndex LedgeCrawl = new AnimationIndex("LedgeCrawl", register: true);

		public static readonly AnimationIndex LedgeGrab = new AnimationIndex("LedgeGrab", register: true);

		public static readonly AnimationIndex HangFromBeam = new AnimationIndex("HangFromBeam", register: true);

		public static readonly AnimationIndex GetUpOnBeam = new AnimationIndex("GetUpOnBeam", register: true);

		public static readonly AnimationIndex StandOnBeam = new AnimationIndex("StandOnBeam", register: true);

		public static readonly AnimationIndex ClimbOnBeam = new AnimationIndex("ClimbOnBeam", register: true);

		public static readonly AnimationIndex GetUpToBeamTip = new AnimationIndex("GetUpToBeamTip", register: true);

		public static readonly AnimationIndex HangUnderVerticalBeam = new AnimationIndex("HangUnderVerticalBeam", register: true);

		public static readonly AnimationIndex BeamTip = new AnimationIndex("BeamTip", register: true);

		public static readonly AnimationIndex CorridorTurn = new AnimationIndex("CorridorTurn", register: true);

		public static readonly AnimationIndex SurfaceSwim = new AnimationIndex("SurfaceSwim", register: true);

		public static readonly AnimationIndex DeepSwim = new AnimationIndex("DeepSwim", register: true);

		public static readonly AnimationIndex Roll = new AnimationIndex("Roll", register: true);

		public static readonly AnimationIndex Flip = new AnimationIndex("Flip", register: true);

		public static readonly AnimationIndex RocketJump = new AnimationIndex("RocketJump", register: true);

		public static readonly AnimationIndex BellySlide = new AnimationIndex("BellySlide", register: true);

		public static readonly AnimationIndex AntlerClimb = new AnimationIndex("AntlerClimb", register: true);

		public static readonly AnimationIndex GrapplingSwing = new AnimationIndex("GrapplingSwing", register: true);

		public static readonly AnimationIndex ZeroGSwim = new AnimationIndex("ZeroGSwim", register: true);

		public static readonly AnimationIndex ZeroGPoleGrab = new AnimationIndex("ZeroGPoleGrab", register: true);

		public static readonly AnimationIndex VineGrab = new AnimationIndex("VineGrab", register: true);

		public static readonly AnimationIndex Dead = new AnimationIndex("Dead", register: true);

		public AnimationIndex(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class BodyModeIndex : ExtEnum<BodyModeIndex>
	{
		public static readonly BodyModeIndex Default = new BodyModeIndex("Default", register: true);

		public static readonly BodyModeIndex Crawl = new BodyModeIndex("Crawl", register: true);

		public static readonly BodyModeIndex Stand = new BodyModeIndex("Stand", register: true);

		public static readonly BodyModeIndex CorridorClimb = new BodyModeIndex("CorridorClimb", register: true);

		public static readonly BodyModeIndex ClimbIntoShortCut = new BodyModeIndex("ClimbIntoShortCut", register: true);

		public static readonly BodyModeIndex WallClimb = new BodyModeIndex("WallClimb", register: true);

		public static readonly BodyModeIndex ClimbingOnBeam = new BodyModeIndex("ClimbingOnBeam", register: true);

		public static readonly BodyModeIndex Swimming = new BodyModeIndex("Swimming", register: true);

		public static readonly BodyModeIndex ZeroG = new BodyModeIndex("ZeroG", register: true);

		public static readonly BodyModeIndex Stunned = new BodyModeIndex("Stunned", register: true);

		public static readonly BodyModeIndex Dead = new BodyModeIndex("Dead", register: true);

		public BodyModeIndex(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	private enum ObjectGrabability
	{
		CantGrab,
		OneHand,
		BigOneHand,
		TwoHands,
		Drag
	}

	public class AbstractOnBackStick : AbstractPhysicalObject.AbstractObjectStick
	{
		public AbstractPhysicalObject Player
		{
			get
			{
				return A;
			}
			set
			{
				A = value;
			}
		}

		public AbstractPhysicalObject Spear
		{
			get
			{
				return B;
			}
			set
			{
				B = value;
			}
		}

		public AbstractOnBackStick(AbstractPhysicalObject player, AbstractPhysicalObject spear)
			: base(player, spear)
		{
		}

		public override string SaveToString(int roomIndex)
		{
			return roomIndex + "<stkA>sprOnBackStick<stkA>" + A.ID.ToString() + "<stkA>" + B.ID.ToString();
		}
	}

	public class SpearOnBack
	{
		public Player owner;

		public Spear spear;

		public bool increment;

		public int counter;

		public float flip;

		public bool interactionLocked;

		public AbstractOnBackStick abstractStick;

		public bool HasASpear => spear != null;

		public SpearOnBack(Player owner)
		{
			this.owner = owner;
		}

		public void Update(bool eu)
		{
			if (increment)
			{
				counter++;
				if (spear != null && counter > 20)
				{
					SpearToHand(eu);
					counter = 0;
				}
				else if (spear == null && counter > 20)
				{
					for (int i = 0; i < 2; i++)
					{
						if (owner.grasps[i] != null && owner.grasps[i].grabbed is Spear)
						{
							owner.bodyChunks[0].pos += Custom.DirVec(owner.grasps[i].grabbed.firstChunk.pos, owner.bodyChunks[0].pos) * 2f;
							SpearToBack(owner.grasps[i].grabbed as Spear);
							counter = 0;
							break;
						}
					}
				}
			}
			else
			{
				counter = 0;
			}
			if (!owner.input[0].pckp)
			{
				interactionLocked = false;
			}
			increment = false;
		}

		public void GraphicsModuleUpdated(bool actuallyViewed, bool eu)
		{
			if (spear == null)
			{
				return;
			}
			if (spear.slatedForDeletetion || spear.grabbedBy.Count > 0)
			{
				if (abstractStick != null)
				{
					abstractStick.Deactivate();
				}
				spear = null;
				return;
			}
			Vector2 vector = owner.mainBodyChunk.pos;
			Vector2 vector2 = owner.bodyChunks[1].pos;
			if (owner.graphicsModule != null)
			{
				vector = Vector2.Lerp((owner.graphicsModule as PlayerGraphics).drawPositions[0, 0], (owner.graphicsModule as PlayerGraphics).head.pos, 0.2f);
				vector2 = (owner.graphicsModule as PlayerGraphics).drawPositions[1, 0];
			}
			Vector2 vector3 = Custom.DirVec(vector2, vector);
			if (owner.Consious && owner.bodyMode != BodyModeIndex.ZeroG && owner.EffectiveRoomGravity > 0f)
			{
				if (owner.bodyMode == BodyModeIndex.Default && owner.animation == AnimationIndex.None && owner.standing && owner.bodyChunks[1].pos.y < owner.bodyChunks[0].pos.y - 6f)
				{
					flip = Custom.LerpAndTick(flip, (float)owner.input[0].x * 0.3f, 0.05f, 0.02f);
				}
				else if (owner.bodyMode == BodyModeIndex.Stand && owner.input[0].x != 0)
				{
					flip = Custom.LerpAndTick(flip, owner.input[0].x, 0.02f, 0.1f);
				}
				else
				{
					flip = Custom.LerpAndTick(flip, (float)owner.flipDirection * Mathf.Abs(vector3.x), 0.15f, 1f / 6f);
				}
				if (counter > 12 && !interactionLocked && owner.input[0].x != 0 && owner.standing)
				{
					float num = 0f;
					for (int i = 0; i < owner.grasps.Length; i++)
					{
						if (owner.grasps[i] == null)
						{
							num = ((i == 0) ? (-1f) : 1f);
							break;
						}
					}
					spear.setRotation = Custom.DegToVec(Custom.AimFromOneVectorToAnother(vector2, vector) + Custom.LerpMap(counter, 12f, 20f, 0f, 360f * num));
				}
				else
				{
					spear.setRotation = (vector3 - Custom.PerpendicularVector(vector3) * 0.9f * (1f - Mathf.Abs(flip))).normalized;
				}
				spear.ChangeOverlap(vector3.y < -0.1f && owner.bodyMode != BodyModeIndex.ClimbingOnBeam);
			}
			else
			{
				flip = Custom.LerpAndTick(flip, 0f, 0.15f, 1f / 7f);
				spear.setRotation = vector3 - Custom.PerpendicularVector(vector3) * 0.9f;
				spear.ChangeOverlap(newOverlap: false);
			}
			spear.firstChunk.MoveFromOutsideMyUpdate(eu, Vector2.Lerp(vector2, vector, 0.6f) - Custom.PerpendicularVector(vector2, vector) * 7.5f * flip);
			spear.firstChunk.vel = owner.mainBodyChunk.vel;
			spear.rotationSpeed = 0f;
		}

		public void SpearToHand(bool eu)
		{
			if (spear == null)
			{
				return;
			}
			for (int i = 0; i < 2; i++)
			{
				if (owner.grasps[i] != null && owner.Grabability(owner.grasps[i].grabbed) >= ObjectGrabability.BigOneHand)
				{
					return;
				}
			}
			int num = -1;
			for (int j = 0; j < 2; j++)
			{
				if (num != -1)
				{
					break;
				}
				if (owner.grasps[j] == null)
				{
					num = j;
				}
			}
			if (num != -1)
			{
				if (owner.graphicsModule != null)
				{
					spear.firstChunk.MoveFromOutsideMyUpdate(eu, (owner.graphicsModule as PlayerGraphics).hands[num].pos);
				}
				owner.SlugcatGrab(spear, num);
				spear = null;
				interactionLocked = true;
				owner.noPickUpOnRelease = 20;
				owner.room.PlaySound(SoundID.Slugcat_Pick_Up_Spear, owner.mainBodyChunk);
				if (abstractStick != null)
				{
					abstractStick.Deactivate();
					abstractStick = null;
				}
			}
		}

		public void SpearToBack(Spear spr)
		{
			if (spear != null)
			{
				return;
			}
			for (int i = 0; i < 2; i++)
			{
				if (owner.grasps[i] != null && owner.grasps[i].grabbed == spr)
				{
					owner.ReleaseGrasp(i);
					break;
				}
			}
			spear = spr;
			spear.ChangeMode(Weapon.Mode.OnBack);
			interactionLocked = true;
			owner.noPickUpOnRelease = 20;
			owner.room.PlaySound(SoundID.Slugcat_Stash_Spear_On_Back, owner.mainBodyChunk);
			if (abstractStick != null)
			{
				abstractStick.Deactivate();
			}
			abstractStick = new AbstractOnBackStick(owner.abstractPhysicalObject, spr.abstractPhysicalObject);
		}

		public void DropSpear()
		{
			if (spear != null)
			{
				spear.firstChunk.vel = owner.mainBodyChunk.vel + Custom.RNV() * 3f * UnityEngine.Random.value;
				spear.ChangeMode(Weapon.Mode.Free);
				spear = null;
				if (abstractStick != null)
				{
					abstractStick.Deactivate();
					abstractStick = null;
				}
			}
		}
	}

	private bool lastGlowing;

	public PhysicalObject objectPointed;

	private InputPackage pointInput;

	private float emoteSleepCounter;

	private bool jollyButtonDown;

	private int cameraSwitchDelay;

	public bool requestedCameraWithoutInput;

	private int handPointing = -1;

	private float pointCycle;

	internal bool bool1;

	public int maulTimer;

	public int scavengerImmunity;

	public bool pyroJumpped;

	public int pyroJumpCounter;

	public float pyroJumpCooldown;

	public float pyroParryCooldown;

	public int pyroJumpDropLock;

	public AncientBot myRobot;

	public bool monkAscension;

	public float maxGodTime;

	public float godTimer;

	public float godDeactiveTimer;

	public bool godRecharging;

	public float godWarmup;

	public bool hideGodPips;

	public float killFac;

	public float lastKillFac;

	public float killWait;

	public float lastKillWait;

	public bool killPressed;

	public float burstVelX;

	public float burstVelY;

	public float burstX;

	public float burstY;

	public Tongue tongue;

	public int tongueAttachTime;

	public int voidSceneTimer;

	public Vector2 wormCutsceneTarget;

	public bool forceBurst;

	public bool wormCutsceneLockon;

	public int saintWeakness;

	public string lastPingRegion;

	public float dissolved;

	public bool inVoidSea;

	public int karmaCharging;

	public NPCStats npcStats;

	private SlugcatStats npcCharacterStats;

	public SlugOnBack slugOnBack;

	public Player onBack;

	public bool smSpearSoundReady;

	public PlayerController controller;

	public RedsIllness redsIllness;

	public bool standStillOnMapButton;

	public int slowMovementStun;

	private int lastStun;

	public bool stillInStartShelter = true;

	public bool readyForWin;

	private int cantBeGrabbedCounter;

	private int goIntoCorridorClimb;

	private bool corridorDrop;

	private int verticalCorridorSlideCounter;

	private int horizontalCorridorSlideCounter;

	private IntVector2? corridorTurnDir;

	private int corridorTurnCounter;

	private int timeSinceInCorridorMode;

	private float[] dynamicRunSpeed = new float[2];

	public int jumpStun;

	public float jumpBoost;

	private int simulateHoldJumpButton;

	public int superLaunchJump;

	public int wallSlideCounter;

	private int killSuperLaunchJumpCounter;

	public int touchedNoInputCounter;

	public int forceSleepCounter;

	public int eatExternalFoodSourceCounter;

	public int dontEatExternalFoodSourceCounter;

	public Vector2? handOnExternalFoodSource;

	public int shootUpCounter;

	public int consistentDownDiagonal;

	public bool allowOutOfBounds;

	public int mushroomCounter;

	public float mushroomEffect;

	public AdrenalineEffect adrenalineEffect;

	public ChunkSoundEmitter slideLoop;

	public SoundID slideLoopSound = SoundID.None;

	public DeafLoopHolder deafLoopHolder;

	public BodyChunk jumpChunk;

	public int jumpChunkCounter;

	public bool addedSpawnRoomToDiscovery;

	private float wiggle;

	private int noWiggleCounter;

	public bool exhausted;

	public Grasp dangerGrasp;

	public int dangerGraspTime;

	public bool dangerGraspLastThrowButton;

	public bool dangerGraspPickupButton;

	public int wantToJump;

	public int canJump;

	public int wantToGrab;

	public int canWallJump;

	private int canCorridorJump;

	private int noGrabCounter;

	private int poleSkipPenalty;

	private int wantToPickUp;

	private int wantToThrow;

	private int dontGrabStuff;

	private int waterJumpDelay;

	private float swimForce;

	public float swimCycle;

	public float diveForce;

	public float sleepCurlUp;

	public int sleepCounter;

	public bool sleepWhenStill;

	public int allowRoll;

	public int rollDirection;

	public int rollCounter;

	public bool rocketJumpFromBellySlide;

	public bool flipFromSlide;

	public int initSlideCounter;

	public int slideCounter;

	public int slideDirection;

	public int eatCounter;

	public WorldCoordinate? karmaFlowerGrowPos;

	private float privSneak;

	public int forceFeetToHorizontalBeamTile;

	private PhysicalObject pickUpCandidate;

	private float[] directionBoosts;

	public bool glowing;

	public float aerobicLevel;

	public bool standing;

	private Vector2? feetStuckPos;

	private int backwardsCounter;

	private int landingDelay;

	private int crawlTurnDelay;

	public Deer.PlayerInAntlers playerInAntlers;

	public TubeWorm tubeWorm;

	public CoralCircuit.CircuitBit[] swimBits;

	public float circuitSwimResistance;

	public float curcuitJumpMeter;

	public List<Vector2> exitsToBeDiscovered;

	private bool FLYEATBUTTON;

	private IntVector2 lastWiggleDir;

	private IntVector2 wiggleDirectionCounters;

	private bool lastWiggleJump;

	public InputPackage mapInput;

	public float airInLungs;

	public bool lungsExhausted;

	public bool submerged;

	public float drown;

	public bool gourmandExhausted;

	public bool chatlog;

	public ChatlogData.ChatlogID chatlogID;

	public int chatlogCounter;

	public SlugcatStats.Name SlugCatClass;

	public int timeSinceSpawned;

	public float customPlayerGravity;

	public int reloadCounter;

	public float lastGroundY;

	public bool craftingObject;

	public int sofCooldown;

	public bool sceneFlag;

	public bool craftingTutorial;

	public int showKarmaFoodRainTime;

	public int gourmandAttackNegateTime;

	public int pullupSoftlockSafety;

	public string lastGoodTrackerSpawnRoom;

	public string lastGoodTrackerSpawnRegion;

	public WorldCoordinate lastGoodTrackerSpawnCoord;

	private int consolePipeWarpInd;

	private int immuneToFallDamage;

	private bool consoleSpawnedKarmaFlower;

	public IntVector2 zeroGPoleGrabDir;

	public AnimationIndex animation;

	private int ledgeGrabCounter;

	private bool straightUpOnHorizontalBeam;

	private Vector2 upOnHorizontalBeamPos;

	private int exitBellySlideCounter;

	public bool whiplashJump;

	public bool longBellySlide;

	public int stopRollingCounter;

	public int slideUpPole;

	public ClimbableVinesSystem.VinePosition vinePos;

	public Vector2 vineClimbCursor;

	public int vineGrabDelay;

	public BodyModeIndex bodyMode;

	public bool leftFoot;

	public int lowerBodyFramesOnGround;

	public int lowerBodyFramesOffGround;

	public int upperBodyFramesOnGround;

	public int upperBodyFramesOffGround;

	public IntVector2? dropGrabTile;

	public int switchHandsCounter;

	public int noPickUpOnRelease;

	public float switchHandsProcess;

	public int swallowAndRegurgitateCounter;

	public AbstractPhysicalObject objectInStomach;

	public int eatMeat;

	private bool WANTTOSTAND;

	public SpearOnBack spearOnBack;

	public bool ReadyForStarveJolly { get; private set; }

	public bool ReadyForWinJolly { get; private set; }

	public bool IsJollyPlayer => playerState.playerNumber != 0;

	public JollyPlayerOptions JollyOption
	{
		get
		{
			RainWorld rainWorld = Custom.rainWorld;
			if ((object)rainWorld == null)
			{
				return null;
			}
			Options options = rainWorld.options;
			if (options == null)
			{
				return null;
			}
			return options.jollyPlayerOptionsArray[playerState.playerNumber];
		}
	}

	public List<PlayerState> GetPlayerStates => base.abstractCreature.Room.world.game.Players.Select((AbstractCreature x) => x.state as PlayerState).ToList();

	public static int InitialShortcutWaitTime
	{
		get
		{
			if (!ModManager.CoopAvailable)
			{
				return 0;
			}
			if (!Custom.rainWorld.options.smartShortcuts)
			{
				return 0;
			}
			return 20 + 10 * Custom.rainWorld.options.JollyPlayerCount;
		}
	}

	public int CameraInputDelay
	{
		get
		{
			Options.JollyCameraInputSpeed jollyCameraInputSpeed = base.abstractCreature.world.game.rainWorld.options.jollyCameraInputSpeed;
			if (jollyCameraInputSpeed == Options.JollyCameraInputSpeed.FAST)
			{
				return 3;
			}
			if (jollyCameraInputSpeed == Options.JollyCameraInputSpeed.NORMAL)
			{
				return 5;
			}
			return 7;
		}
	}

	public bool DreamState
	{
		get
		{
			if (ModManager.MSC && room != null && room.game.IsStorySession && room.game.wasAnArtificerDream)
			{
				return room.abstractRoom.name != "GW_ARTYNIGHTMARE";
			}
			return false;
		}
	}

	public bool isNPC
	{
		get
		{
			if (ModManager.MSC)
			{
				return base.abstractCreature.creatureTemplate.TopAncestor().type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC;
			}
			return false;
		}
	}

	public SlugNPCAI AI
	{
		get
		{
			if (ModManager.MSC && base.abstractCreature.abstractAI != null)
			{
				return base.abstractCreature.abstractAI.RealAI as SlugNPCAI;
			}
			return null;
		}
	}

	public bool CanPutSlugToBack
	{
		get
		{
			if ((ModManager.MSC || ModManager.CoopAvailable) && slugOnBack != null && !slugOnBack.interactionLocked && slugOnBack.slugcat == null)
			{
				if (spearOnBack != null)
				{
					return !spearOnBack.HasASpear;
				}
				return true;
			}
			return false;
		}
	}

	public bool CanRetrieveSlugFromBack
	{
		get
		{
			if (!ModManager.MSC && !ModManager.CoopAvailable)
			{
				return false;
			}
			if (CanRetrieveSpearFromBack || slugOnBack == null || slugOnBack.slugcat == null || slugOnBack.interactionLocked || (base.grasps[0] != null && base.grasps[1] != null))
			{
				return false;
			}
			for (int i = 0; i < base.grasps.Length; i++)
			{
				if (base.grasps[i] != null && (Grabability(base.grasps[i].grabbed) > ObjectGrabability.BigOneHand || base.grasps[i].grabbed is Player))
				{
					return false;
				}
			}
			return true;
		}
	}

	public InputPackage[] input { get; private set; }

	public int flipDirection { get; set; }

	public int lastFlipDirection { get; private set; }

	public float Adrenaline => mushroomEffect;

	public float Wiggle => wiggle * Mathf.Pow(Mathf.InverseLerp(2f, 12f, Vector2.Distance(base.bodyChunks[0].lastPos, base.bodyChunks[0].pos) + Vector2.Distance(base.bodyChunks[1].lastPos, base.bodyChunks[1].pos)), 0.5f);

	public float GraspWiggle
	{
		get
		{
			if (!ModManager.MMF || !MMF.cfgGraspWiggling.Value)
			{
				return 0f;
			}
			return Wiggle;
		}
	}

	public SlugcatStats slugcatStats
	{
		get
		{
			if (isSlugpup)
			{
				return npcCharacterStats;
			}
			if (!ModManager.MSC || base.abstractCreature.world.game.IsStorySession)
			{
				if (ModManager.CoopAvailable && base.abstractCreature.world.game.IsStorySession && (base.abstractCreature.world.game.session as StoryGameSession).characterStatsJollyplayer != null)
				{
					return (base.abstractCreature.world.game.session as StoryGameSession).characterStatsJollyplayer[playerState.playerNumber];
				}
				return base.abstractCreature.world.game.session.characterStats;
			}
			return (base.abstractCreature.world.game.session as ArenaGameSession).characterStats_Mplayer[playerState.playerNumber];
		}
	}

	public bool Malnourished
	{
		get
		{
			if (ModManager.MSC && npcCharacterStats != null)
			{
				return npcCharacterStats.malnourished;
			}
			return base.abstractCreature.world.game.session.characterStats.malnourished;
		}
	}

	public bool Sleeping => sleepCounter != 0;

	public int FoodInStomach => playerState.foodInStomach;

	public int MaxFoodInStomach
	{
		get
		{
			if (!base.abstractCreature.world.game.IsStorySession)
			{
				return int.MaxValue;
			}
			return slugcatStats.maxFood;
		}
	}

	public int Karma
	{
		get
		{
			if (!(base.abstractCreature.world.game.session is StoryGameSession))
			{
				return 0;
			}
			if (AI != null)
			{
				return (playerState as PlayerNPCState).KarmaLevel;
			}
			return (base.abstractCreature.world.game.session as StoryGameSession).saveState.deathPersistentSaveData.karma;
		}
	}

	public int KarmaCap
	{
		get
		{
			if (!(base.abstractCreature.world.game.session is StoryGameSession))
			{
				return 4;
			}
			if (AI != null)
			{
				return (playerState as PlayerNPCState).KarmaLevel;
			}
			return (base.abstractCreature.world.game.session as StoryGameSession).saveState.deathPersistentSaveData.karmaCap;
		}
	}

	public bool KarmaIsReinforced
	{
		get
		{
			if (!(base.abstractCreature.world.game.session is StoryGameSession))
			{
				return false;
			}
			return (base.abstractCreature.world.game.session as StoryGameSession).saveState.deathPersistentSaveData.reinforcedKarma;
		}
	}

	public bool PlaceKarmaFlower
	{
		get
		{
			if (!KarmaIsReinforced && !(slugcatStats.name == SlugcatStats.Name.Yellow))
			{
				if (slugcatStats.name == SlugcatStats.Name.Red && base.abstractCreature.world.game.IsStorySession)
				{
					return base.abstractCreature.world.game.GetStorySession.RedIsOutOfCycles;
				}
				return false;
			}
			return true;
		}
	}

	public PlayerSessionRecord SessionRecord
	{
		get
		{
			if (!base.abstractCreature.world.game.IsStorySession || AI != null)
			{
				return null;
			}
			return base.abstractCreature.world.game.GetStorySession.playerSessionRecords[playerState.playerNumber];
		}
	}

	public float Sneak => Mathf.Lerp(Mathf.Pow(Mathf.InverseLerp(0f, 0.5f, privSneak), 0.5f), 0.5f, base.Submersion);

	public PlayerState playerState => base.abstractCreature.state as PlayerState;

	public int ThrowDirection
	{
		get
		{
			if (bodyMode == BodyModeIndex.Default || Mathf.Abs(base.bodyChunks[0].pos.x - base.bodyChunks[1].pos.x) < 15f)
			{
				if (input[0].x == 0)
				{
					return flipDirection;
				}
				return input[0].x;
			}
			return (int)Mathf.Sign(base.bodyChunks[0].pos.x - base.bodyChunks[1].pos.x);
		}
	}

	public int animationFrame { get; protected set; }

	public override float VisibilityBonus
	{
		get
		{
			if (ModManager.MSC && chatlog)
			{
				return -1f;
			}
			return slugcatStats.generalVisibilityBonus - slugcatStats.visualStealthInSneakMode * Sneak;
		}
	}

	public int CurrentFood => FoodInStomach;

	public InputPackage MapInput => mapInput;

	public bool RevealMap
	{
		get
		{
			if ((!ModManager.CoopAvailable || !jollyButtonDown) && input[0].mp)
			{
				return !inVoidSea;
			}
			return false;
		}
	}

	public Vector2 MapOwnerInRoomPosition
	{
		get
		{
			if (room == null && base.inShortcut && base.abstractCreature.Room.realizedRoom != null)
			{
				Vector2? vector = base.abstractCreature.Room.realizedRoom.game.shortcuts.OnScreenPositionOfInShortCutCreature(base.abstractCreature.Room.realizedRoom, this);
				if (vector.HasValue)
				{
					return vector.Value;
				}
			}
			return base.mainBodyChunk.pos;
		}
	}

	public int MapOwnerRoom => base.abstractCreature.pos.room;

	public bool MapDiscoveryActive
	{
		get
		{
			if (base.Consious && AI == null && room != null && !room.world.singleRoomWorld && base.abstractCreature.Room.realizedRoom != null && dangerGrasp == null && base.mainBodyChunk.pos.x > 0f && base.mainBodyChunk.pos.x < base.abstractCreature.Room.realizedRoom.PixelWidth && base.mainBodyChunk.pos.y > 0f)
			{
				return base.mainBodyChunk.pos.y < base.abstractCreature.Room.realizedRoom.PixelHeight;
			}
			return false;
		}
	}

	public override float EffectiveRoomGravity
	{
		get
		{
			if (ModManager.MSC && customPlayerGravity == 0f)
			{
				return 0f;
			}
			if (room != null)
			{
				return room.gravity;
			}
			return 0f;
		}
	}

	public bool Wounded
	{
		get
		{
			if (base.Consious)
			{
				return (base.State as PlayerState).permanentDamageTracking > 0.4;
			}
			return false;
		}
	}

	public bool isRivulet
	{
		get
		{
			if (!ModManager.MSC || !(SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Rivulet))
			{
				if (ModManager.MSC && ModManager.Expedition && Custom.rainWorld.ExpeditionMode)
				{
					return ExpeditionGame.activeUnlocks.Contains("unl-agility");
				}
				return false;
			}
			return true;
		}
	}

	public bool isGourmand
	{
		get
		{
			if (ModManager.MSC)
			{
				return SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Gourmand;
			}
			return false;
		}
	}

	public bool isSlugpup
	{
		get
		{
			if (ModManager.MSC)
			{
				return SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Slugpup;
			}
			return false;
		}
	}

	public bool PainJumps
	{
		get
		{
			if (ModManager.MSC && room != null && room.game.IsStorySession && SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel && !room.abstractRoom.gate && !room.abstractRoom.shelter && room.world.region != null)
			{
				return room.world.region.name == "CC";
			}
			return false;
		}
	}

	public bool CanPutSpearToBack
	{
		get
		{
			if (spearOnBack != null && !spearOnBack.interactionLocked && spearOnBack.spear == null)
			{
				if ((ModManager.MSC || ModManager.CoopAvailable) && slugOnBack != null)
				{
					return !slugOnBack.HasASlug;
				}
				return true;
			}
			return false;
		}
	}

	public bool CanRetrieveSpearFromBack
	{
		get
		{
			if (spearOnBack == null || spearOnBack.spear == null || spearOnBack.interactionLocked || (base.grasps[0] != null && base.grasps[1] != null))
			{
				return false;
			}
			for (int i = 0; i < base.grasps.Length; i++)
			{
				if (base.grasps[i] != null && Grabability(base.grasps[i].grabbed) >= ObjectGrabability.BigOneHand)
				{
					return false;
				}
			}
			return true;
		}
	}

	public void PermaDie()
	{
		JollyCustom.Log("[JOLLY] Player " + playerState.playerNumber + " permadied!");
		playerState.permaDead = true;
		if (ModManager.CoopAvailable && !isNPC && !playerState.isGhost)
		{
			Die();
		}
	}

	public void JollyUpdate(bool eu)
	{
		if (!ModManager.CoopAvailable || isNPC || DreamState)
		{
			return;
		}
		if (lastGlowing != glowing)
		{
			JollyCustom.Log("Player now glowing!");
			foreach (AbstractCreature item in room.game.Players.Where((AbstractCreature x) => x.realizedCreature != null))
			{
				(item.realizedCreature as Player).glowing = glowing;
				(item.realizedCreature as Player).lastGlowing = glowing;
			}
		}
		ReadyForWinJolly = false;
		ReadyForStarveJolly = false;
		if (room != null)
		{
			if (onBack != null)
			{
				animation = AnimationIndex.GrapplingSwing;
			}
			JollyFoodUpdate();
			JollyEmoteUpdate();
			JollyInputUpdate();
			JollyPointUpdate();
		}
	}

	private void JollyInputUpdate()
	{
		if (cameraSwitchDelay > 0)
		{
			cameraSwitchDelay--;
		}
		if (input[0].mp)
		{
			if (!input[1].mp)
			{
				jollyButtonDown = false;
				for (int i = 2; i < input.Length - 1; i++)
				{
					if (input[i].mp && !input[i + 1].mp)
					{
						jollyButtonDown = true;
						cameraSwitchDelay = -1;
					}
				}
			}
		}
		else
		{
			jollyButtonDown = false;
		}
		if (!input[0].mp && input[1].mp && cameraSwitchDelay == -1)
		{
			int num = 0;
			jollyButtonDown = false;
			for (int j = 2; j < input.Length && input[j].mp; j++)
			{
				num++;
			}
			if (num <= CameraInputDelay)
			{
				cameraSwitchDelay = 5;
			}
		}
		if (cameraSwitchDelay == 0)
		{
			TriggerCameraSwitch();
			cameraSwitchDelay = -1;
		}
		if (input[0].mp && input[1].mp && room.world.game.cameras[0].hud.owner != this)
		{
			room.world.game.cameras[0].hud.showKarmaFoodRain = true;
		}
		if (input[0].jmp && onBack != null)
		{
			onBack.slugOnBack.DropSlug();
		}
		standStillOnMapButton = room.game.cameras[0].hud != null && room.game.cameras[0].hud.owner == this;
	}

	public void TriggerCameraSwitch()
	{
		RoomCamera roomCamera = base.abstractCreature.world.game.cameras[0];
		if (roomCamera.followAbstractCreature != null && roomCamera.followAbstractCreature.realizedCreature == this)
		{
			if (Custom.rainWorld.options.cameraCycling)
			{
				int index = (playerState.playerNumber + 1) % base.abstractCreature.world.game.session.Players.Count();
				AbstractCreature cameraTarget = base.abstractCreature.world.game.Players[index];
				JollyCustom.Log("Jolly Camera: Player: " + playerState?.playerNumber + " requested camera to Player: " + index);
				roomCamera.ChangeCameraToPlayer(cameraTarget);
			}
		}
		else
		{
			AbstractCreature cameraTarget = base.abstractCreature;
			JollyCustom.Log("Jolly Camera: Player: " + playerState.playerNumber + " requested the have camera");
			roomCamera.ChangeCameraToPlayer(cameraTarget);
		}
	}

	public Vector2 PointDir()
	{
		Vector2 analogueDir = pointInput.analogueDir;
		if (analogueDir.x != 0f || analogueDir.y != 0f)
		{
			return analogueDir.normalized;
		}
		if (pointInput.ZeroGGamePadIntVec.x != 0 || pointInput.ZeroGGamePadIntVec.y != 0)
		{
			return pointInput.IntVec.ToVector2().normalized;
		}
		return Vector2.zero;
	}

	public void JollyEmoteUpdate()
	{
		if (!standing && input[0].y < 0 && !input[0].jmp && !input[0].thrw && !input[0].pckp && IsTileSolid(1, 0, -1) && (input[0].x == 0 || ((!IsTileSolid(1, -1, -1) || !IsTileSolid(1, 1, -1)) && IsTileSolid(1, input[0].x, 0))))
		{
			emoteSleepCounter += 0.028f;
		}
		else
		{
			emoteSleepCounter = 0f;
		}
		if (emoteSleepCounter > 1.4f)
		{
			sleepCurlUp = Mathf.SmoothStep(sleepCurlUp, 1f, emoteSleepCounter - 1.4f);
		}
		else
		{
			sleepCurlUp = Mathf.Max(0f, sleepCurlUp - 0.1f);
		}
	}

	public void JollyFoodUpdate()
	{
		if (this.playerState.playerNumber != 0)
		{
			PlayerState playerState = room.game.Players[0].state as PlayerState;
			this.playerState.foodInStomach = Math.Max(Math.Min(playerState.foodInStomach, MaxFoodInStomach), 0);
			this.playerState.quarterFoodPoints = playerState.quarterFoodPoints;
		}
	}

	public void SaveStomachObjectInPlayerState()
	{
		if (!ModManager.CoopAvailable || isNPC || playerState.isGhost)
		{
			return;
		}
		try
		{
			if (objectInStomach == null)
			{
				return;
			}
			if (objectInStomach is AbstractCreature)
			{
				AbstractCreature abstractCreature = objectInStomach as AbstractCreature;
				if (base.abstractCreature.world.GetAbstractRoom(abstractCreature.pos.room) == null)
				{
					abstractCreature.pos = base.coord;
				}
				playerState.swallowedItem = SaveState.AbstractCreatureToStringStoryWorld(abstractCreature);
			}
			else
			{
				playerState.swallowedItem = objectInStomach.ToString();
			}
			JollyCustom.Log($"Storing object of death player... {playerState.playerNumber}: {playerState.swallowedItem}");
		}
		catch (Exception ex)
		{
			JollyCustom.Log("Error while gathering objects in stomach! " + ex, throwException: true);
		}
	}

	public override void Destroy()
	{
		if (ModManager.CoopAvailable && !isNPC && !playerState.isGhost)
		{
			if (!playerState.permaDead)
			{
				PermaDie();
			}
			try
			{
				base.abstractCreature.world.game.GameOver(null);
			}
			catch (Exception ex)
			{
				JollyCustom.Log("Error while gameovering! " + ex);
			}
		}
		base.Destroy();
	}

	public void JollyPointUpdate()
	{
		int num = 0;
		PhysicalObject physicalObject = null;
		handPointing = -1;
		if (jollyButtonDown)
		{
			Vector2 vector = PointDir();
			if (vector == Vector2.zero)
			{
				return;
			}
			for (int num2 = 1; num2 >= 0; num2--)
			{
				if ((base.grasps[num2] == null || base.grasps[num2].grabbed is Weapon) && (base.graphicsModule as PlayerGraphics).hands[1 - num2].reachedSnapPosition)
				{
					handPointing = num2;
				}
			}
			float num3 = 100f;
			if (input[0].jmp || (objectPointed != null && objectPointed.jollyBeingPointedCounter > 10))
			{
				pointCycle += 0.3f;
				num3 = Mathf.Lerp(10f, 50f, Math.Abs(Mathf.Sin(pointCycle)));
			}
			Vector2 pos = base.mainBodyChunk.pos;
			Vector2 vector2 = new Vector2(base.mainBodyChunk.pos.x + vector.x * num3, base.mainBodyChunk.pos.y + vector.y * num3);
			(base.graphicsModule as PlayerGraphics).LookAtPoint(vector2, 10f);
			if (handPointing > -1)
			{
				(base.graphicsModule as PlayerGraphics).hands[handPointing].reachingForObject = true;
				(base.graphicsModule as PlayerGraphics).hands[handPointing].absoluteHuntPos = vector2;
			}
			float num4 = float.MaxValue;
			for (int i = 0; i < room.physicalObjects.Length; i++)
			{
				for (int j = 0; j < room.physicalObjects[i].Count; j++)
				{
					bool flag = false;
					for (int k = 0; k < room.physicalObjects[i][j].grabbedBy.Count; k++)
					{
						flag = room.physicalObjects[i][j].grabbedBy[k].grabber == this;
					}
					if (room.physicalObjects[i][j] == this || flag)
					{
						continue;
					}
					for (int l = 0; l < room.physicalObjects[i][j].bodyChunks.Length; l++)
					{
						BodyChunk bodyChunk = room.physicalObjects[i][j].bodyChunks[l];
						if (!room.VisualContact(base.mainBodyChunk.pos, bodyChunk.pos))
						{
							continue;
						}
						Vector2 pos2 = bodyChunk.pos;
						float num5 = Custom.Dist(pos2, pos);
						if (!(num5 >= num4))
						{
							float num6 = Vector2.Dot(vector, (pos2 - pos).normalized);
							float num7 = Mathf.Abs(Custom.DistanceToLine(pos2, pos, vector2));
							if (num6 > 0.75f && num7 < 50f)
							{
								physicalObject = bodyChunk.owner;
								num4 = num5;
							}
						}
					}
				}
			}
		}
		if (physicalObject != null)
		{
			if (physicalObject == objectPointed)
			{
				objectPointed.jollyBeingPointedCounter++;
			}
			else
			{
				objectPointed = physicalObject;
				objectPointed.jollyBeingPointedCounter = 0;
				num = 4;
				JollyCustom.Log("Pointed: " + objectPointed.ToString());
			}
		}
		else if (objectPointed != null && objectPointed.jollyBeingPointedCounter > 2)
		{
			objectPointed.jollyBeingPointedCounter -= 2;
		}
		if (objectPointed == null || objectPointed.jollyBeingPointedCounter <= 5 || objectPointed.jollyBeingPointedCounter >= 15)
		{
			return;
		}
		JollyCustom.Log("Pointing something scav");
		foreach (Scavenger item in room.physicalObjects.SelectMany((List<PhysicalObject> x) => x).OfType<Scavenger>().ToList())
		{
			if (num <= 0)
			{
				break;
			}
			JollyCustom.Log("found scav");
			if (room.VisualContact(item.mainBodyChunk.pos, objectPointed.bodyChunks[0].pos) && room.VisualContact(item.mainBodyChunk.pos, base.bodyChunks[0].pos) && item.AI != null)
			{
				item.AI.MakeLookHere(objectPointed.bodyChunks[0].pos);
				item.visionFactor = 0f;
				item.narrowVision = 1f;
				JollyCustom.Log($"Making {item.ToString()} ({4 - num}) look at {objectPointed.ToString()}");
				num--;
			}
		}
	}

	public void ClassMechanicsArtificer()
	{
		if (pyroJumpDropLock > 0)
		{
			pyroJumpDropLock--;
		}
		if (!(SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer) && (!ExpeditionGame.explosivejump || isSlugpup))
		{
			return;
		}
		bool flag = wantToJump > 0 && input[0].pckp;
		bool flag2 = eatMeat >= 20 || maulTimer >= 15;
		int num = Mathf.Max(1, global::MoreSlugcats.MoreSlugcats.cfgArtificerExplosionCapacity.Value - 5);
		if (pyroJumpCounter > 0 && (base.Consious || base.dead))
		{
			pyroJumpCooldown -= 1f;
			if (pyroJumpCooldown <= 0f)
			{
				if (pyroJumpCounter >= num)
				{
					pyroJumpCooldown = 40f;
				}
				else
				{
					pyroJumpCooldown = 60f;
				}
				pyroJumpCounter--;
			}
		}
		pyroParryCooldown -= 1f;
		if (pyroJumpCounter >= num)
		{
			if (UnityEngine.Random.value < 0.25f)
			{
				room.AddObject(new Explosion.ExplosionSmoke(base.mainBodyChunk.pos, Custom.RNV() * 2f * UnityEngine.Random.value, 1f));
			}
			if (UnityEngine.Random.value < 0.5f)
			{
				room.AddObject(new Spark(base.mainBodyChunk.pos, Custom.RNV(), Color.white, null, 4, 8));
			}
		}
		if (flag && !pyroJumpped && canJump <= 0 && !flag2 && (input[0].y >= 0 || (input[0].y < 0 && (bodyMode == BodyModeIndex.ZeroG || base.gravity <= 0.1f))) && base.Consious && bodyMode != BodyModeIndex.Crawl && bodyMode != BodyModeIndex.CorridorClimb && bodyMode != BodyModeIndex.ClimbIntoShortCut && animation != AnimationIndex.HangFromBeam && animation != AnimationIndex.ClimbOnBeam && bodyMode != BodyModeIndex.WallClimb && bodyMode != BodyModeIndex.Swimming && animation != AnimationIndex.AntlerClimb && animation != AnimationIndex.VineGrab && animation != AnimationIndex.ZeroGPoleGrab && onBack == null)
		{
			pyroJumpped = true;
			pyroJumpDropLock = 40;
			noGrabCounter = 5;
			Vector2 pos = base.firstChunk.pos;
			for (int i = 0; i < 8; i++)
			{
				room.AddObject(new Explosion.ExplosionSmoke(pos, Custom.RNV() * 5f * UnityEngine.Random.value, 1f));
			}
			room.AddObject(new Explosion.ExplosionLight(pos, 160f, 1f, 3, Color.white));
			for (int j = 0; j < 10; j++)
			{
				Vector2 vector = Custom.RNV();
				room.AddObject(new Spark(pos + vector * UnityEngine.Random.value * 40f, vector * Mathf.Lerp(4f, 30f, UnityEngine.Random.value), Color.white, null, 4, 18));
			}
			room.PlaySound(SoundID.Fire_Spear_Explode, pos, 0.3f + UnityEngine.Random.value * 0.3f, 0.5f + UnityEngine.Random.value * 2f);
			room.InGameNoise(new InGameNoise(pos, 8000f, this, 1f));
			int num2 = Mathf.Max(1, global::MoreSlugcats.MoreSlugcats.cfgArtificerExplosionCapacity.Value - 3);
			if (bodyMode == BodyModeIndex.ZeroG || room.gravity == 0f || base.gravity == 0f)
			{
				float num3 = input[0].x;
				float num4 = input[0].y;
				while (num3 == 0f && num4 == 0f)
				{
					num3 = ((!((double)UnityEngine.Random.value <= 0.33)) ? (((double)UnityEngine.Random.value <= 0.5) ? 1 : (-1)) : 0);
					num4 = ((!((double)UnityEngine.Random.value <= 0.33)) ? (((double)UnityEngine.Random.value <= 0.5) ? 1 : (-1)) : 0);
				}
				base.bodyChunks[0].vel.x = 9f * num3;
				base.bodyChunks[0].vel.y = 9f * num4;
				base.bodyChunks[1].vel.x = 8f * num3;
				base.bodyChunks[1].vel.y = 8f * num4;
				pyroJumpCooldown = 150f;
				pyroJumpCounter++;
			}
			else
			{
				if (input[0].x != 0)
				{
					base.bodyChunks[0].vel.y = Mathf.Min(base.bodyChunks[0].vel.y, 0f) + 8f;
					base.bodyChunks[1].vel.y = Mathf.Min(base.bodyChunks[1].vel.y, 0f) + 7f;
					jumpBoost = 6f;
				}
				if (input[0].x == 0 || input[0].y == 1)
				{
					if (pyroJumpCounter >= num2)
					{
						base.bodyChunks[0].vel.y = 16f;
						base.bodyChunks[1].vel.y = 15f;
						jumpBoost = 10f;
					}
					else
					{
						base.bodyChunks[0].vel.y = 11f;
						base.bodyChunks[1].vel.y = 10f;
						jumpBoost = 8f;
					}
				}
				if (input[0].y == 1)
				{
					base.bodyChunks[0].vel.x = 10f * (float)input[0].x;
					base.bodyChunks[1].vel.x = 8f * (float)input[0].x;
				}
				else
				{
					base.bodyChunks[0].vel.x = 15f * (float)input[0].x;
					base.bodyChunks[1].vel.x = 13f * (float)input[0].x;
				}
				animation = AnimationIndex.Flip;
				pyroJumpCounter++;
				pyroJumpCooldown = 150f;
				bodyMode = BodyModeIndex.Default;
			}
			if (pyroJumpCounter >= num2)
			{
				Stun(60 * (pyroJumpCounter - (num2 - 1)));
			}
			if (pyroJumpCounter >= global::MoreSlugcats.MoreSlugcats.cfgArtificerExplosionCapacity.Value)
			{
				PyroDeath();
			}
		}
		else if (flag && !submerged && !flag2 && (input[0].y < 0 || bodyMode == BodyModeIndex.Crawl) && (canJump > 0 || input[0].y < 0) && base.Consious && !pyroJumpped && pyroParryCooldown <= 0f)
		{
			if (canJump <= 0)
			{
				pyroJumpped = true;
				base.bodyChunks[0].vel.y = 8f;
				base.bodyChunks[1].vel.y = 6f;
				jumpBoost = 6f;
				forceSleepCounter = 0;
			}
			if (pyroJumpCounter <= num)
			{
				pyroJumpCounter += 2;
			}
			else
			{
				pyroJumpCounter++;
			}
			pyroParryCooldown = 40f;
			pyroJumpCooldown = 150f;
			Vector2 pos2 = base.firstChunk.pos;
			for (int k = 0; k < 8; k++)
			{
				room.AddObject(new Explosion.ExplosionSmoke(pos2, Custom.RNV() * 5f * UnityEngine.Random.value, 1f));
			}
			room.AddObject(new Explosion.ExplosionLight(pos2, 160f, 1f, 3, Color.white));
			for (int l = 0; l < 10; l++)
			{
				Vector2 vector2 = Custom.RNV();
				room.AddObject(new Spark(pos2 + vector2 * UnityEngine.Random.value * 40f, vector2 * Mathf.Lerp(4f, 30f, UnityEngine.Random.value), Color.white, null, 4, 18));
			}
			room.AddObject(new ShockWave(pos2, 200f, 0.2f, 6));
			room.PlaySound(SoundID.Fire_Spear_Explode, pos2, 0.3f + UnityEngine.Random.value * 0.3f, 0.5f + UnityEngine.Random.value * 2f);
			room.InGameNoise(new InGameNoise(pos2, 8000f, this, 1f));
			List<Weapon> list = new List<Weapon>();
			for (int m = 0; m < room.physicalObjects.Length; m++)
			{
				for (int n = 0; n < room.physicalObjects[m].Count; n++)
				{
					if (room.physicalObjects[m][n] is Weapon)
					{
						Weapon weapon = room.physicalObjects[m][n] as Weapon;
						if (weapon.mode == Weapon.Mode.Thrown && Custom.Dist(pos2, weapon.firstChunk.pos) < 300f)
						{
							list.Add(weapon);
						}
					}
					bool flag3 = !ModManager.CoopAvailable || Custom.rainWorld.options.friendlyFire || !(room.physicalObjects[m][n] is Player player) || player.isNPC;
					if (!(room.physicalObjects[m][n] is Creature && room.physicalObjects[m][n] != this && flag3))
					{
						continue;
					}
					Creature creature = room.physicalObjects[m][n] as Creature;
					if (!(Custom.Dist(pos2, creature.firstChunk.pos) < 200f) || (!(Custom.Dist(pos2, creature.firstChunk.pos) < 60f) && !room.VisualContact(base.abstractCreature.pos, creature.abstractCreature.pos)))
					{
						continue;
					}
					room.socialEventRecognizer.WeaponAttack(null, this, creature, hit: true);
					creature.SetKillTag(base.abstractCreature);
					if (creature is Scavenger)
					{
						(creature as Scavenger).HeavyStun(80);
					}
					else
					{
						creature.Stun(80);
					}
					creature.firstChunk.vel = Custom.DegToVec(Custom.AimFromOneVectorToAnother(pos2, creature.firstChunk.pos)) * 30f;
					if (creature is TentaclePlant)
					{
						for (int num5 = 0; num5 < creature.grasps.Length; num5++)
						{
							creature.ReleaseGrasp(num5);
						}
					}
				}
			}
			if (list.Count > 0 && room.game.IsArenaSession)
			{
				room.game.GetArenaGameSession.arenaSitting.players[0].parries++;
			}
			for (int num6 = 0; num6 < list.Count; num6++)
			{
				list[num6].ChangeMode(Weapon.Mode.Free);
				list[num6].firstChunk.vel = Custom.DegToVec(Custom.AimFromOneVectorToAnother(pos2, list[num6].firstChunk.pos)) * 20f;
				list[num6].SetRandomSpin();
			}
			int num7 = Mathf.Max(1, global::MoreSlugcats.MoreSlugcats.cfgArtificerExplosionCapacity.Value - 3);
			if (pyroJumpCounter >= num7)
			{
				Stun(60 * (pyroJumpCounter - (num7 - 1)));
			}
			if (pyroJumpCounter >= global::MoreSlugcats.MoreSlugcats.cfgArtificerExplosionCapacity.Value)
			{
				PyroDeath();
			}
		}
		if (canJump > 0 || !base.Consious || base.Stunned || animation == AnimationIndex.HangFromBeam || animation == AnimationIndex.ClimbOnBeam || bodyMode == BodyModeIndex.WallClimb || animation == AnimationIndex.AntlerClimb || animation == AnimationIndex.VineGrab || animation == AnimationIndex.ZeroGPoleGrab || bodyMode == BodyModeIndex.Swimming || ((bodyMode == BodyModeIndex.ZeroG || room.gravity <= 0.5f || base.gravity <= 0.5f) && (wantToJump == 0 || !input[0].pckp)))
		{
			pyroJumpped = false;
		}
	}

	public bool CanMaulCreature(Creature crit)
	{
		bool flag = true;
		if (ModManager.CoopAvailable && crit is Player player && (player.isNPC || !Custom.rainWorld.options.friendlyFire))
		{
			flag = false;
		}
		if (!(crit is Fly) && !crit.dead && (!(crit is IPlayerEdible) || (crit is Centipede && !(crit as Centipede).Edible) || FoodInStomach >= MaxFoodInStomach) && flag && (crit.Stunned || (!(crit is Cicada) && !(crit is Player) && IsCreatureLegalToHoldWithoutStun(crit))))
		{
			return SlugcatStats.SlugcatCanMaul(SlugCatClass);
		}
		return false;
	}

	public void MaulingUpdate(int graspIndex)
	{
		if (base.grasps[graspIndex] == null || !(base.grasps[graspIndex].grabbed is Creature) || maulTimer <= 15)
		{
			return;
		}
		if ((base.grasps[graspIndex].grabbed as Creature).abstractCreature.creatureTemplate.type == CreatureTemplate.Type.Scavenger)
		{
			base.grasps[graspIndex].grabbed.bodyChunks[0].mass = 0.5f;
			base.grasps[graspIndex].grabbed.bodyChunks[1].mass = 0.3f;
			base.grasps[graspIndex].grabbed.bodyChunks[2].mass = 0.05f;
		}
		standing = false;
		Blink(5);
		if (maulTimer % 3 == 0)
		{
			Vector2 vector = Custom.RNV() * 3f;
			base.mainBodyChunk.pos += vector;
			base.mainBodyChunk.vel += vector;
		}
		Vector2 vector2 = base.grasps[graspIndex].grabbedChunk.pos * base.grasps[graspIndex].grabbedChunk.mass;
		float num = base.grasps[graspIndex].grabbedChunk.mass;
		for (int i = 0; i < base.grasps[graspIndex].grabbed.bodyChunkConnections.Length; i++)
		{
			if (base.grasps[graspIndex].grabbed.bodyChunkConnections[i].chunk1 == base.grasps[graspIndex].grabbedChunk)
			{
				vector2 += base.grasps[graspIndex].grabbed.bodyChunkConnections[i].chunk2.pos * base.grasps[graspIndex].grabbed.bodyChunkConnections[i].chunk2.mass;
				num += base.grasps[graspIndex].grabbed.bodyChunkConnections[i].chunk2.mass;
			}
			else if (base.grasps[graspIndex].grabbed.bodyChunkConnections[i].chunk2 == base.grasps[graspIndex].grabbedChunk)
			{
				vector2 += base.grasps[graspIndex].grabbed.bodyChunkConnections[i].chunk1.pos * base.grasps[graspIndex].grabbed.bodyChunkConnections[i].chunk1.mass;
				num += base.grasps[graspIndex].grabbed.bodyChunkConnections[i].chunk1.mass;
			}
		}
		vector2 /= num;
		base.mainBodyChunk.vel += Custom.DirVec(base.mainBodyChunk.pos, vector2) * 0.5f;
		base.bodyChunks[1].vel -= Custom.DirVec(base.mainBodyChunk.pos, vector2) * 0.6f;
		if (base.graphicsModule == null)
		{
			return;
		}
		if (!Custom.DistLess(base.grasps[graspIndex].grabbedChunk.pos, (base.graphicsModule as PlayerGraphics).head.pos, base.grasps[graspIndex].grabbedChunk.rad))
		{
			(base.graphicsModule as PlayerGraphics).head.vel += Custom.DirVec(base.grasps[graspIndex].grabbedChunk.pos, (base.graphicsModule as PlayerGraphics).head.pos) * (base.grasps[graspIndex].grabbedChunk.rad - Vector2.Distance(base.grasps[graspIndex].grabbedChunk.pos, (base.graphicsModule as PlayerGraphics).head.pos));
		}
		else if (maulTimer % 5 == 3)
		{
			(base.graphicsModule as PlayerGraphics).head.vel += Custom.RNV() * 4f;
		}
		if (maulTimer > 10 && maulTimer % 8 == 3)
		{
			base.mainBodyChunk.pos += Custom.DegToVec(Mathf.Lerp(-90f, 90f, UnityEngine.Random.value)) * 4f;
			base.grasps[graspIndex].grabbedChunk.vel += Custom.DirVec(vector2, base.mainBodyChunk.pos) * 0.9f / base.grasps[graspIndex].grabbedChunk.mass;
			for (int num2 = UnityEngine.Random.Range(0, 3); num2 >= 0; num2--)
			{
				room.AddObject(new WaterDrip(Vector2.Lerp(base.grasps[graspIndex].grabbedChunk.pos, base.mainBodyChunk.pos, UnityEngine.Random.value) + base.grasps[graspIndex].grabbedChunk.rad * Custom.RNV() * UnityEngine.Random.value, Custom.RNV() * 6f * UnityEngine.Random.value + Custom.DirVec(vector2, (base.mainBodyChunk.pos + (base.graphicsModule as PlayerGraphics).head.pos) / 2f) * 7f * UnityEngine.Random.value + Custom.DegToVec(Mathf.Lerp(-90f, 90f, UnityEngine.Random.value)) * UnityEngine.Random.value * EffectiveRoomGravity * 7f, waterColor: false));
			}
		}
	}

	public void PyroDeath()
	{
		pyroJumpCounter = global::MoreSlugcats.MoreSlugcats.cfgArtificerExplosionCapacity.Value;
		Vector2 vector = Vector2.Lerp(base.firstChunk.pos, base.firstChunk.lastPos, 0.35f);
		room.AddObject(new SootMark(room, vector, 80f, bigSprite: true));
		room.AddObject(new Explosion(room, this, vector, 7, 350f, 26.2f, 2f, 280f, 0.35f, this, 0.7f, 160f, 1f));
		room.AddObject(new Explosion.ExplosionLight(vector, 280f, 1f, 7, Color.white));
		room.AddObject(new Explosion.ExplosionLight(vector, 230f, 1f, 3, new Color(1f, 1f, 1f)));
		room.AddObject(new ExplosionSpikes(room, vector, 14, 30f, 9f, 7f, 170f, Color.white));
		room.AddObject(new ShockWave(vector, 430f, 0.045f, 5));
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
				room.AddObject(new Spark(vector + vector2 * Mathf.Lerp(30f, 60f, UnityEngine.Random.value), vector2 * Mathf.Lerp(7f, 38f, UnityEngine.Random.value) + Custom.RNV() * 20f * UnityEngine.Random.value, Color.Lerp(Color.white, new Color(1f, 1f, 1f), UnityEngine.Random.value), null, 11, 28));
			}
			room.AddObject(new Explosion.FlashingSmoke(vector + vector2 * 40f * UnityEngine.Random.value, vector2 * Mathf.Lerp(4f, 20f, Mathf.Pow(UnityEngine.Random.value, 2f)), 1f + 0.05f * UnityEngine.Random.value, new Color(1f, 1f, 1f), Color.white, UnityEngine.Random.Range(3, 11)));
		}
		room.ScreenMovement(vector, default(Vector2), 1.3f);
		room.PlaySound(SoundID.Bomb_Explode, vector);
		room.InGameNoise(new InGameNoise(vector, 9000f, this, 1f));
		Die();
	}

	public void ClassMechanicsGourmand()
	{
		if (SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Gourmand && (double)aerobicLevel >= 0.95)
		{
			gourmandExhausted = true;
		}
		if (aerobicLevel < 0.4f)
		{
			gourmandExhausted = false;
		}
		if (gourmandExhausted)
		{
			slowMovementStun = Math.Max(slowMovementStun, (int)Custom.LerpMap(aerobicLevel, 0.7f, 0.4f, 6f, 0f));
			lungsExhausted = true;
		}
	}

	public bool SlugSlamConditions(PhysicalObject otherObject)
	{
		if (SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
		{
			return false;
		}
		if ((otherObject as Creature).abstractCreature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)
		{
			return false;
		}
		if (gourmandAttackNegateTime > 0)
		{
			return false;
		}
		if (base.gravity == 0f)
		{
			return false;
		}
		if (cantBeGrabbedCounter > 0)
		{
			return false;
		}
		if (forceSleepCounter > 0)
		{
			return false;
		}
		if (timeSinceInCorridorMode < 5)
		{
			return false;
		}
		if (submerged)
		{
			return false;
		}
		if (enteringShortCut.HasValue || (animation != AnimationIndex.BellySlide && canJump >= 5))
		{
			return false;
		}
		if (animation == AnimationIndex.CorridorTurn || animation == AnimationIndex.CrawlTurn || animation == AnimationIndex.ZeroGSwim || animation == AnimationIndex.ZeroGPoleGrab || animation == AnimationIndex.GetUpOnBeam || animation == AnimationIndex.ClimbOnBeam || animation == AnimationIndex.AntlerClimb || animation == AnimationIndex.BeamTip)
		{
			return false;
		}
		Vector2 vel = base.bodyChunks[0].vel;
		if (base.bodyChunks[1].vel.magnitude < vel.magnitude)
		{
			vel = base.bodyChunks[1].vel;
		}
		if (animation != AnimationIndex.BellySlide && vel.y >= -10f && vel.magnitude <= 25f)
		{
			return false;
		}
		Creature creature = otherObject as Creature;
		foreach (Grasp item in grabbedBy)
		{
			if (item.pacifying || item.grabber == creature)
			{
				return false;
			}
		}
		if (ModManager.CoopAvailable && otherObject is Player && !Custom.rainWorld.options.friendlyFire)
		{
			return false;
		}
		return true;
	}

	public AbstractPhysicalObject.AbstractObjectType CraftingResults()
	{
		if (SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
		{
			if (FoodInStomach > 0)
			{
				Grasp[] array = base.grasps;
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i] != null && array[i].grabbed is IPlayerEdible && (array[i].grabbed as IPlayerEdible).Edible)
					{
						return null;
					}
				}
				if (array[0] != null && array[0].grabbed is Spear && !(array[0].grabbed as Spear).abstractSpear.explosive)
				{
					return AbstractPhysicalObject.AbstractObjectType.Spear;
				}
				if (array[0] == null && array[1] != null && array[1].grabbed is Spear && !(array[1].grabbed as Spear).abstractSpear.explosive && objectInStomach == null)
				{
					return AbstractPhysicalObject.AbstractObjectType.Spear;
				}
			}
			return null;
		}
		return GourmandCombos.CraftingResults_ObjectData(base.grasps[0], base.grasps[1], canMakeMeals: true);
	}

	public bool GraspsCanBeCrafted()
	{
		if ((!(SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer) || !(CraftingResults() != null)) && (!(SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Gourmand) || input[0].y != 1 || !(CraftingResults() != null)))
		{
			if (ModManager.Expedition && Custom.rainWorld.ExpeditionMode && ExpeditionGame.activeUnlocks.Contains("unl-crafting") && input[0].y == 1)
			{
				return CraftingResults() != null;
			}
			return false;
		}
		return true;
	}

	public void SpitUpCraftedObject()
	{
		craftingTutorial = true;
		room.PlaySound(SoundID.Slugcat_Swallow_Item, base.mainBodyChunk);
		if (SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
		{
			for (int i = 0; i < base.grasps.Length; i++)
			{
				if (base.grasps[i] == null)
				{
					continue;
				}
				AbstractPhysicalObject abstractPhysicalObject = base.grasps[i].grabbed.abstractPhysicalObject;
				if (!(abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.Spear) || (abstractPhysicalObject as AbstractSpear).explosive)
				{
					continue;
				}
				if ((abstractPhysicalObject as AbstractSpear).electric && (abstractPhysicalObject as AbstractSpear).electricCharge > 0)
				{
					room.AddObject(new ZapCoil.ZapFlash(base.firstChunk.pos, 10f));
					room.PlaySound(SoundID.Zapper_Zap, base.firstChunk.pos, 1f, 1.5f + UnityEngine.Random.value * 1.5f);
					if (base.Submersion > 0.5f)
					{
						room.AddObject(new UnderwaterShock(room, null, base.firstChunk.pos, 10, 800f, 2f, this, new Color(0.8f, 0.8f, 1f)));
					}
					Stun(200);
					room.AddObject(new CreatureSpasmer(this, allowDead: false, 200));
					return;
				}
				ReleaseGrasp(i);
				abstractPhysicalObject.realizedObject.RemoveFromRoom();
				room.abstractRoom.RemoveEntity(abstractPhysicalObject);
				SubtractFood(1);
				AbstractSpear abstractSpear = new AbstractSpear(room.world, null, base.abstractCreature.pos, room.game.GetNewID(), explosive: true);
				room.abstractRoom.AddEntity(abstractSpear);
				abstractSpear.RealizeInRoom();
				if (FreeHand() != -1)
				{
					SlugcatGrab(abstractSpear.realizedObject, FreeHand());
				}
				return;
			}
		}
		AbstractPhysicalObject abstractPhysicalObject2 = null;
		if (GourmandCombos.CraftingResults_ObjectData(base.grasps[0], base.grasps[1], canMakeMeals: true) == AbstractPhysicalObject.AbstractObjectType.DangleFruit)
		{
			if (ModManager.Expedition && ModManager.MSC && Custom.rainWorld.ExpeditionMode && ExpeditionGame.activeUnlocks.Contains("unl-crafting") && SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Spear)
			{
				if (abstractPhysicalObject2 != null && FreeHand() != -1)
				{
					SlugcatGrab(abstractPhysicalObject2.realizedObject, FreeHand());
				}
				return;
			}
			while ((base.grasps[0] != null && base.grasps[0].grabbed is IPlayerEdible) || (base.grasps[1] != null && base.grasps[1].grabbed is IPlayerEdible))
			{
				BiteEdibleObject(eu: true);
			}
			AddFood(1);
		}
		else
		{
			abstractPhysicalObject2 = GourmandCombos.CraftingResults(this, base.grasps[0], base.grasps[1]);
			room.abstractRoom.AddEntity(abstractPhysicalObject2);
			abstractPhysicalObject2.RealizeInRoom();
			for (int j = 0; j < base.grasps.Length; j++)
			{
				AbstractPhysicalObject abstractPhysicalObject3 = base.grasps[j].grabbed.abstractPhysicalObject;
				if (room.game.session is StoryGameSession)
				{
					(room.game.session as StoryGameSession).RemovePersistentTracker(abstractPhysicalObject3);
				}
				ReleaseGrasp(j);
				for (int num = abstractPhysicalObject3.stuckObjects.Count - 1; num >= 0; num--)
				{
					if (abstractPhysicalObject3.stuckObjects[num] is AbstractPhysicalObject.AbstractSpearStick && abstractPhysicalObject3.stuckObjects[num].A.type == AbstractPhysicalObject.AbstractObjectType.Spear && abstractPhysicalObject3.stuckObjects[num].A.realizedObject != null)
					{
						(abstractPhysicalObject3.stuckObjects[num].A.realizedObject as Spear).ChangeMode(Weapon.Mode.Free);
					}
				}
				abstractPhysicalObject3.LoseAllStuckObjects();
				abstractPhysicalObject3.realizedObject.RemoveFromRoom();
				room.abstractRoom.RemoveEntity(abstractPhysicalObject3);
			}
		}
		if (abstractPhysicalObject2 != null && FreeHand() != -1)
		{
			SlugcatGrab(abstractPhysicalObject2.realizedObject, FreeHand());
		}
	}

	public bool SaintTongueCheck()
	{
		if (base.Consious && SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint && tongue.mode == Tongue.Mode.Retracted && bodyMode != BodyModeIndex.CorridorClimb && !corridorDrop && bodyMode != BodyModeIndex.ClimbIntoShortCut && bodyMode != BodyModeIndex.WallClimb && bodyMode != BodyModeIndex.Swimming && animation != AnimationIndex.VineGrab && animation != AnimationIndex.ZeroGPoleGrab)
		{
			return !monkAscension;
		}
		return false;
	}

	public void SaintStagger(int time)
	{
		if (room != null)
		{
			room.AddObject(new CreatureSpasmer(this, allowDead: false, time / 5));
			Stun(time / 5);
		}
		airInLungs *= 0.2f;
		saintWeakness += time;
		exhausted = true;
		aerobicLevel = 1f;
	}

	public void ActivateAscension()
	{
		monkAscension = true;
		wantToJump = 0;
		room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, base.mainBodyChunk, loop: false, 1f, 1f);
		room.AddObject(new ShockWave(base.bodyChunks[1].pos, 100f, 0.07f, 6));
		for (int i = 0; i < 10; i++)
		{
			room.AddObject(new WaterDrip(base.bodyChunks[1].pos, Custom.DegToVec(UnityEngine.Random.value * 360f) * Mathf.Lerp(4f, 21f, UnityEngine.Random.value), waterColor: false));
		}
	}

	public void DeactivateAscension()
	{
		room.PlaySound(SoundID.HUD_Pause_Game, base.mainBodyChunk, loop: false, 1f, 0.5f);
		monkAscension = false;
	}

	public void InitVoidWormCutscene()
	{
		controller = new NullController();
		voidSceneTimer = 1;
		wormCutsceneLockon = false;
	}

	public void InitiateDissolve()
	{
		dissolved = 0.01f;
		room.game.cameras[0].ReplaceDrawable(base.graphicsModule, base.graphicsModule);
		room.AddObject(new ShockWave(base.mainBodyChunk.pos, 200f, 0.2f, 6));
	}

	public void ClassMechanicsSaint()
	{
		bool flag = base.abstractCreature.world.game.IsStorySession && base.abstractCreature.world.game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Saint;
		if (SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint)
		{
			if (flag && room.world.region != null)
			{
				if (lastPingRegion == "")
				{
					lastPingRegion = room.world.region.name;
				}
				if (lastPingRegion != room.world.region.name && !room.abstractRoom.gate)
				{
					lastPingRegion = room.world.region.name;
					if (room != null && World.CheckForRegionGhost(MoreSlugcatsEnums.SlugcatStatsName.Saint, room.world.region.name))
					{
						GhostWorldPresence.GhostID ghostID = GhostWorldPresence.GetGhostID(room.world.region.name);
						if (!(room.game.session as StoryGameSession).saveState.deathPersistentSaveData.ghostsTalkedTo.ContainsKey(ghostID) || (room.game.session as StoryGameSession).saveState.deathPersistentSaveData.ghostsTalkedTo[ghostID] < 2)
						{
							room.AddObject(new GhostPing(room));
						}
					}
				}
			}
			if (!MMF.cfgOldTongue.Value && input[0].jmp && !input[1].jmp && !input[0].pckp && canJump <= 0 && bodyMode != BodyModeIndex.Crawl && animation != AnimationIndex.ClimbOnBeam && animation != AnimationIndex.AntlerClimb && animation != AnimationIndex.HangFromBeam && SaintTongueCheck())
			{
				Vector2 vector = new Vector2(flipDirection, 0.7f).normalized;
				if (input[0].y > 0)
				{
					vector = new Vector2(0f, 1f);
				}
				vector = (vector + base.mainBodyChunk.vel.normalized * 0.2f).normalized;
				tongue.Shoot(vector);
			}
		}
		if (SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint && (KarmaCap >= 9 || (room.game.session is ArenaGameSession && room.game.GetArenaGameSession.arenaSitting.gameTypeSetup.gameType == MoreSlugcatsEnums.GameTypeID.Challenge && room.game.GetArenaGameSession.arenaSitting.gameTypeSetup.challengeMeta.ascended)))
		{
			if (voidSceneTimer > 0 && flag)
			{
				voidSceneTimer++;
				if (!monkAscension)
				{
					ActivateAscension();
				}
				godTimer = maxGodTime;
				if (voidSceneTimer > 60)
				{
					if (!forceBurst)
					{
						burstX = 0f;
						burstY = 0f;
					}
					forceBurst = true;
					killWait = Mathf.Min(killWait + 0.035f, 1f);
				}
			}
			if (room.world.name == "HR")
			{
				maxGodTime = 560f;
			}
			if (flag && AI == null && room.game.session is StoryGameSession && !(room.game.session as StoryGameSession).saveState.deathPersistentSaveData.SaintEnlightMessage)
			{
				(room.game.session as StoryGameSession).saveState.deathPersistentSaveData.SaintEnlightMessage = true;
				room.game.cameras[0].hud.textPrompt.AddMessage(room.game.rainWorld.inGameTranslator.Translate("While in the air, tap jump and pick-up together to take flight."), 240, 480, darken: true, hideHud: true);
			}
			if (wantToJump > 0 && monkAscension)
			{
				DeactivateAscension();
				wantToJump = 0;
			}
			else if (wantToJump > 0 && input[0].pckp && canJump <= 0 && !monkAscension && !tongue.Attached && bodyMode != BodyModeIndex.Crawl && bodyMode != BodyModeIndex.CorridorClimb && bodyMode != BodyModeIndex.ClimbIntoShortCut && animation != AnimationIndex.HangFromBeam && animation != AnimationIndex.ClimbOnBeam && bodyMode != BodyModeIndex.WallClimb && bodyMode != BodyModeIndex.Swimming && base.Consious && !base.Stunned && godTimer > 0f && animation != AnimationIndex.AntlerClimb && animation != AnimationIndex.VineGrab && animation != AnimationIndex.ZeroGPoleGrab)
			{
				ActivateAscension();
			}
		}
		lastKillFac = killFac;
		lastKillWait = killWait;
		if (karmaCharging > 0)
		{
			godTimer = Mathf.Min(godTimer + 1f, maxGodTime);
			karmaCharging--;
		}
		if (monkAscension)
		{
			base.buoyancy = 0f;
			godDeactiveTimer = 0f;
			animation = AnimationIndex.None;
			bodyMode = BodyModeIndex.Default;
			if (tongue != null && tongue.Attached)
			{
				tongue.Release();
			}
			if (godWarmup > -20f)
			{
				godWarmup -= 1f;
			}
			if ((room == null || !room.game.setupValues.saintInfinitePower) && karmaCharging == 0 && godWarmup <= 0f)
			{
				godTimer -= 1f;
			}
			if (base.dead || base.stun >= 20)
			{
				DeactivateAscension();
			}
			if (godTimer <= 0f)
			{
				godRecharging = true;
				godTimer = -15f;
				DeactivateAscension();
			}
			else
			{
				godRecharging = false;
			}
			if (flag && AI == null && godTimer <= maxGodTime * 0.9f && room.game.session is StoryGameSession && !(room.game.session as StoryGameSession).saveState.deathPersistentSaveData.KarmicBurstMessage)
			{
				(room.game.session as StoryGameSession).saveState.deathPersistentSaveData.KarmicBurstMessage = true;
				room.game.cameras[0].hud.textPrompt.AddMessage(room.game.rainWorld.inGameTranslator.Translate("Hold throw and directional inputs while flying to perform an ascension."), 80, 240, darken: true, hideHud: true);
			}
			base.gravity = 0f;
			base.airFriction = 0.7f;
			float num = 2.75f;
			if (killWait >= 0.2f && !forceBurst)
			{
				base.airFriction = 0.1f;
				base.bodyChunks[0].vel = Custom.RNV() * Mathf.Lerp(0f, 20f, killWait);
				num = 0f;
			}
			if (input[0].y > 0)
			{
				base.bodyChunks[0].vel.y = base.bodyChunks[0].vel.y + num;
				base.bodyChunks[1].vel.y = base.bodyChunks[1].vel.y + (num - 1f);
			}
			else if (input[0].y < 0)
			{
				base.bodyChunks[0].vel.y = base.bodyChunks[0].vel.y - num;
				base.bodyChunks[1].vel.y = base.bodyChunks[1].vel.y - (num - 1f);
			}
			if (input[0].x > 0)
			{
				base.bodyChunks[0].vel.x = base.bodyChunks[0].vel.x + num;
				base.bodyChunks[1].vel.x = base.bodyChunks[1].vel.x + (num - 1f);
			}
			else if (input[0].x < 0)
			{
				base.bodyChunks[0].vel.x = base.bodyChunks[0].vel.x - num;
				base.bodyChunks[1].vel.x = base.bodyChunks[1].vel.x - (num - 1f);
			}
			float num2 = 10f;
			float num3 = 400f;
			float num4 = 1f;
			float num5 = 2f;
			float num6 = 0.7f;
			if (!input[0].thrw && !forceBurst)
			{
				if (voidSceneTimer == 0)
				{
					burstX *= num6;
					burstY *= num6;
				}
				burstVelX *= num6;
				burstVelY *= num6;
				killPressed = false;
				killFac *= 0.8f;
				killWait *= 0.95f;
				return;
			}
			if (!killPressed)
			{
				if (!forceBurst)
				{
					killWait = Mathf.Min(killWait + 0.035f, 1f);
					if (killWait == 1f)
					{
						killFac += 0.025f;
					}
				}
				if (input[0].x != 0)
				{
					burstVelX = Mathf.Clamp(burstVelX + (float)input[0].x * num4, 0f - num2, num2);
				}
				else if (burstVelX < 0f - num5)
				{
					burstVelX += num5;
				}
				else if (burstVelX > num5)
				{
					burstVelX -= num5;
				}
				else
				{
					burstVelX = 0f;
				}
				if (input[0].y != 0)
				{
					burstVelY = Mathf.Clamp(burstVelY + (float)input[0].y * num4, 0f - num2, num2);
				}
				else if (burstVelY < 0f - num5)
				{
					burstVelY += num5;
				}
				else if (burstVelY > num5)
				{
					burstVelY -= num5;
				}
				else
				{
					burstVelY = 0f;
				}
				if (!forceBurst)
				{
					burstX = Mathf.Clamp(burstX + burstVelX, 0f - num3, num3);
					burstY = Mathf.Clamp(burstY + burstVelY, 0f - num3, num3);
				}
				else if (flag)
				{
					float num7 = wormCutsceneTarget.x - (base.mainBodyChunk.pos.x + burstX);
					float num8 = wormCutsceneTarget.y - (base.mainBodyChunk.pos.y + burstY + 60f);
					if (Custom.DistLess(Vector2.zero, new Vector2(num7, num8), 450f))
					{
						float num9 = 0.02f;
						if (wormCutsceneLockon)
						{
							num9 = 0.25f;
						}
						if (num7 > 0f)
						{
							burstX += Mathf.Clamp(num7 * num9, 2.5f, wormCutsceneLockon ? 100f : 10f);
						}
						else
						{
							burstX += Mathf.Clamp(num7 * num9, wormCutsceneLockon ? (-100f) : (-10f), -2.5f);
						}
						if (num8 > 0f)
						{
							burstY += Mathf.Clamp(num8 * num9, 2.5f, wormCutsceneLockon ? 100f : 10f);
						}
						else
						{
							burstY += Mathf.Clamp(num8 * num9, wormCutsceneLockon ? (-100f) : (-10f), -2.5f);
						}
						if (Custom.DistLess(Vector2.zero, new Vector2(num7, num8), 40f) && killWait == 1f)
						{
							killFac += 0.025f;
							wormCutsceneLockon = true;
						}
					}
				}
			}
			if (!(killFac >= 1f))
			{
				return;
			}
			num = 60f;
			Vector2 vector2 = new Vector2(base.mainBodyChunk.pos.x + burstX, base.mainBodyChunk.pos.y + burstY + 60f);
			bool flag2 = false;
			for (int i = 0; i < room.physicalObjects.Length; i++)
			{
				for (int num10 = room.physicalObjects[i].Count - 1; num10 >= 0; num10--)
				{
					if (num10 >= room.physicalObjects[i].Count)
					{
						num10 = room.physicalObjects[i].Count - 1;
					}
					PhysicalObject physicalObject = room.physicalObjects[i][num10];
					if (physicalObject != this)
					{
						BodyChunk[] array = physicalObject.bodyChunks;
						foreach (BodyChunk bodyChunk in array)
						{
							if (!Custom.DistLess(bodyChunk.pos, vector2, num + bodyChunk.rad) || !room.VisualContact(bodyChunk.pos, vector2))
							{
								continue;
							}
							bodyChunk.vel += Custom.RNV() * 36f;
							if (physicalObject is Creature)
							{
								if (!(physicalObject as Creature).dead)
								{
									flag2 = true;
								}
								(physicalObject as Creature).Die();
							}
							if (physicalObject is SeedCob && !(physicalObject as SeedCob).AbstractCob.opened && !(physicalObject as SeedCob).AbstractCob.dead)
							{
								(physicalObject as SeedCob).spawnUtilityFoods();
							}
							if (room.game.session is StoryGameSession && physicalObject is Oracle && flag)
							{
								if ((physicalObject as Oracle).ID == MoreSlugcatsEnums.OracleID.CL && !(room.game.session as StoryGameSession).saveState.deathPersistentSaveData.ripPebbles)
								{
									(room.game.session as StoryGameSession).saveState.deathPersistentSaveData.ripPebbles = true;
									room.PlaySound(SoundID.SS_AI_Talk_1, base.mainBodyChunk, loop: false, 1f, 0.4f);
									Vector2 pos = (physicalObject as Oracle).bodyChunks[0].pos;
									room.AddObject(new ShockWave(pos, 500f, 0.75f, 18));
									room.AddObject(new Explosion.ExplosionLight(pos, 320f, 1f, 5, Color.white));
									Custom.Log("Ascend saint pebbles");
									((physicalObject as Oracle).oracleBehavior as CLOracleBehavior).dialogBox.Interrupt("...", 1);
									if (((physicalObject as Oracle).oracleBehavior as CLOracleBehavior).currentConversation != null)
									{
										((physicalObject as Oracle).oracleBehavior as CLOracleBehavior).currentConversation.Destroy();
									}
									(physicalObject as Oracle).health = 0f;
									flag2 = true;
								}
								if ((physicalObject as Oracle).ID == Oracle.OracleID.SL && !(room.game.session as StoryGameSession).saveState.deathPersistentSaveData.ripMoon && (physicalObject as Oracle).glowers > 0 && (physicalObject as Oracle).mySwarmers.Count > 0)
								{
									for (int k = 0; k < (physicalObject as Oracle).mySwarmers.Count; k++)
									{
										(physicalObject as Oracle).mySwarmers[k].ExplodeSwarmer();
									}
									(room.game.session as StoryGameSession).saveState.deathPersistentSaveData.ripMoon = true;
									Custom.Log("Ascend saint moon");
									((physicalObject as Oracle).oracleBehavior as SLOracleBehaviorHasMark).dialogBox.Interrupt("...", 1);
									if (((physicalObject as Oracle).oracleBehavior as SLOracleBehaviorHasMark).currentConversation != null)
									{
										((physicalObject as Oracle).oracleBehavior as SLOracleBehaviorHasMark).currentConversation.Destroy();
									}
									Vector2 pos2 = (physicalObject as Oracle).bodyChunks[0].pos;
									room.AddObject(new ShockWave(pos2, 500f, 0.75f, 18));
									room.AddObject(new Explosion.ExplosionLight(pos2, 320f, 1f, 5, Color.white));
									flag2 = true;
								}
							}
							if (physicalObject is Oracle && (physicalObject as Oracle).ID == MoreSlugcatsEnums.OracleID.ST && (physicalObject as Oracle).Consious)
							{
								Vector2 pos3 = (physicalObject as Oracle).bodyChunks[0].pos;
								room.AddObject(new ShockWave(pos3, 500f, 0.75f, 18));
								((physicalObject as Oracle).oracleBehavior as STOracleBehavior).AdvancePhase();
								base.bodyChunks[0].vel = Vector2.zero;
								flag2 = true;
							}
						}
					}
				}
			}
			for (int l = 0; l < room.updateList.Count; l++)
			{
				if (room.updateList[l] is Love)
				{
					Love love = room.updateList[l] as Love;
					if (love.animator != null && love.timeUntilReboot == 0 && Custom.DistLess(love.pos, vector2, 100f))
					{
						love.InitiateReboot();
						flag2 = true;
					}
				}
			}
			if (flag2 || voidSceneTimer > 0)
			{
				room.PlaySound(SoundID.Firecracker_Bang, base.mainBodyChunk, loop: false, 1f, 0.75f + UnityEngine.Random.value);
				room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, base.mainBodyChunk, loop: false, 1f, 0.5f + UnityEngine.Random.value * 0.5f);
			}
			else
			{
				room.PlaySound(SoundID.Snail_Pop, base.mainBodyChunk, loop: false, 1f, 1.5f + UnityEngine.Random.value);
			}
			for (int m = 0; m < 20; m++)
			{
				room.AddObject(new Spark(vector2, Custom.RNV() * UnityEngine.Random.value * 40f, new Color(1f, 1f, 1f), null, 30, 120));
			}
			killFac = 0f;
			killWait = 0f;
			killPressed = true;
			if (voidSceneTimer > 0)
			{
				voidSceneTimer = 0;
				DeactivateAscension();
				controller = null;
				forceBurst = false;
			}
		}
		else
		{
			if (godWarmup < 60f && godDeactiveTimer > 200f)
			{
				godWarmup += 1f;
			}
			godDeactiveTimer += 1f;
			killPressed = false;
			killFac *= 0.8f;
			killWait *= 0.5f;
			float num11 = 0.15f * (maxGodTime / 400f);
			if (godRecharging)
			{
				num11 = 0.15f * (maxGodTime / 400f);
			}
			godTimer = Mathf.Min(godTimer + num11, maxGodTime);
		}
	}

	public void TongueUpdate()
	{
		if (tongue == null || room == null)
		{
			return;
		}
		tongue.baseChunk = base.mainBodyChunk;
		if (tongue.Attached)
		{
			tongueAttachTime++;
			if (base.Stunned)
			{
				Custom.Log("Tongue stun detatch?");
				tongue.Release();
			}
			else
			{
				if (input[0].y > 0)
				{
					tongue.decreaseRopeLength(3f);
				}
				if (input[0].y < 0)
				{
					tongue.increaseRopeLength(3f);
				}
				if (input[0].jmp && !input[1].jmp && tongueAttachTime >= 2)
				{
					tongue.Release();
					if (!tongue.isZeroGMode())
					{
						float num = Mathf.Lerp(1f, 1.15f, Adrenaline);
						if (base.grasps[0] != null && HeavyCarry(base.grasps[0].grabbed) && !(base.grasps[0].grabbed is Cicada))
						{
							num += Mathf.Min(Mathf.Max(0f, base.grasps[0].grabbed.TotalMass - 0.2f) * 1.5f, 1.3f);
						}
						base.bodyChunks[0].vel.y = 8f * num;
						base.bodyChunks[1].vel.y = 7f * num;
						jumpBoost = 8f;
					}
					room.PlaySound(SoundID.Slugcat_Normal_Jump, base.mainBodyChunk, loop: false, 1f, 1f);
				}
			}
		}
		else
		{
			tongueAttachTime = 0;
		}
		tongue.Update();
		if (tongue.rope.totalLength > tongue.totalRope * 2.5f)
		{
			tongue.Release();
		}
	}

	public bool CanIPutDeadSlugOnBack(Player pickUpCandidate)
	{
		if (ModManager.CoopAvailable && pickUpCandidate != null)
		{
			if (ModManager.MSC)
			{
				return !(pickUpCandidate.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Slugpup);
			}
			return true;
		}
		return false;
	}

	public void setPupStatus(bool set)
	{
		(base.abstractCreature.state as PlayerState).isPup = set;
		if ((base.abstractCreature.state as PlayerState).isPup)
		{
			float num = 0.7f * slugcatStats.bodyWeightFac + (bool1 ? 0.18f : 0f);
			base.bodyChunks[0].mass = num / 2f;
			base.bodyChunks[1].mass = num / 2f;
			bodyChunkConnections[0] = new BodyChunkConnection(base.bodyChunks[0], base.bodyChunks[1], 12f, BodyChunkConnection.Type.Normal, 1f, 0.5f);
		}
		else
		{
			float num2 = 0.75f * slugcatStats.bodyWeightFac;
			base.bodyChunks[0].mass = num2 / 2f;
			base.bodyChunks[1].mass = num2 / 2f;
			bodyChunkConnections[0] = new BodyChunkConnection(base.bodyChunks[0], base.bodyChunks[1], 17f, BodyChunkConnection.Type.Normal, 1f, 0.5f);
		}
	}

	public void NPCForceGrab(PhysicalObject obj)
	{
		if (dontGrabStuff != 0)
		{
			return;
		}
		for (int i = 0; i < base.grasps.Length; i++)
		{
			if (base.grasps[i] == null)
			{
				SlugcatGrab(obj, i);
			}
		}
	}

	public bool NPCGrabCheck(PhysicalObject item)
	{
		if (item.room == null || item.room != room || item.grabbedBy.Count > 0)
		{
			return false;
		}
		if (item.abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.SLOracleSwarmer)
		{
			return false;
		}
		if (item.abstractPhysicalObject.type == MoreSlugcatsEnums.AbstractObjectType.MoonCloak)
		{
			return false;
		}
		int num = 0;
		if (Grabability(item) == ObjectGrabability.Drag)
		{
			float dst = float.MaxValue;
			for (int i = 0; i < item.bodyChunks.Length; i++)
			{
				if (Custom.DistLess(base.mainBodyChunk.pos, item.bodyChunks[i].pos, dst))
				{
					dst = Vector2.Distance(base.mainBodyChunk.pos, item.bodyChunks[i].pos);
					num = i;
				}
			}
		}
		if ((!(item is PlayerCarryableItem) || (item as PlayerCarryableItem).forbiddenToPlayer < 1) && Custom.DistLess(base.bodyChunks[0].pos, item.bodyChunks[num].pos, item.bodyChunks[num].rad + 40f) && (Custom.DistLess(base.bodyChunks[0].pos, item.bodyChunks[num].pos, item.bodyChunks[num].rad + 20f) || room.VisualContact(base.bodyChunks[0].pos, item.bodyChunks[num].pos)))
		{
			return CanIPickThisUp(item);
		}
		return false;
	}

	public void ClassMechanicsSpearmaster()
	{
		if ((base.stun < 1 && !base.dead) || !(SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Spear))
		{
			return;
		}
		PlayerGraphics.TailSpeckles tailSpecks = (base.graphicsModule as PlayerGraphics).tailSpecks;
		if (tailSpecks.spearProg > 0f)
		{
			tailSpecks.setSpearProgress(Mathf.Lerp(tailSpecks.spearProg, 0f, 0.05f));
			if (tailSpecks.spearProg < 0.025f)
			{
				tailSpecks.setSpearProgress(0f);
			}
		}
	}

	public void InitChatLog(ChatlogData.ChatlogID id)
	{
		chatlog = true;
		chatlogID = id;
		chatlogCounter = 0;
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			base.bodyChunks[i].vel = Vector2.zero;
		}
	}

	public void ProcessChatLog()
	{
		if (!chatlog || room == null)
		{
			return;
		}
		if (chatlogID != ChatlogData.ChatlogID.Chatlog_SI9)
		{
			mushroomCounter = 25;
			Stun(25);
			if (ModManager.CoopAvailable)
			{
				foreach (AbstractCreature alivePlayer in base.abstractCreature.world.game.AlivePlayers)
				{
					if (alivePlayer != base.abstractCreature)
					{
						alivePlayer.realizedCreature?.Stun(20);
					}
				}
			}
		}
		chatlogCounter++;
		int num = 0;
		if (chatlogCounter == 60 && room.game.cameras[0].hud.chatLog == null)
		{
			if (chatlogID == ChatlogData.ChatlogID.DevCommentaryNode)
			{
				room.game.cameras[0].hud.InitChatLog(ChatlogData.getDevComm(room.abstractRoom.name, room.game.GetStorySession.saveStateNumber));
				return;
			}
			if (ChatlogData.HasUnique(chatlogID))
			{
				room.game.cameras[0].hud.InitChatLog(ChatlogData.getChatlog(chatlogID));
				return;
			}
			foreach (ChatlogData.ChatlogID item in room.game.GetStorySession.saveState.deathPersistentSaveData.chatlogsRead)
			{
				if (item != chatlogID && !ChatlogData.HasUnique(item))
				{
					num++;
				}
			}
			room.game.cameras[0].hud.InitChatLog(ChatlogData.getChatlog(num));
		}
		else
		{
			if (room.game.cameras[0].hud.chatLog != null || chatlogCounter < 60)
			{
				return;
			}
			chatlog = false;
			if (chatlogID != ChatlogData.ChatlogID.DevCommentaryNode && !room.game.GetStorySession.saveState.deathPersistentSaveData.chatlogsRead.Contains(chatlogID))
			{
				room.game.GetStorySession.saveState.deathPersistentSaveData.chatlogsRead.Add(chatlogID);
				if (room.game.GetStorySession.saveState.miscWorldSaveData.SSaiConversationsHad == 0 && !room.game.GetStorySession.saveState.deathPersistentSaveData.prePebChatlogsRead.Contains(chatlogID))
				{
					room.game.GetStorySession.saveState.deathPersistentSaveData.prePebChatlogsRead.Add(chatlogID);
				}
			}
			if (ChatlogData.HasUnique(chatlogID))
			{
				room.game.rainWorld.progression.miscProgressionData.SetBroadcastListened(chatlogID);
				return;
			}
			foreach (ChatlogData.ChatlogID item2 in room.game.GetStorySession.saveState.deathPersistentSaveData.chatlogsRead)
			{
				if (!ChatlogData.HasUnique(item2))
				{
					num++;
				}
			}
			if (!ChatlogData.getChatlogExists(num))
			{
				ChatlogData.markAllBroadcastsRead();
			}
			if (room.game.GetStorySession.saveState.miscWorldSaveData.SSaiConversationsHad > 0)
			{
				room.game.rainWorld.progression.miscProgressionData.postPebblesBroadcasts = Mathf.Max(num, room.game.rainWorld.progression.miscProgressionData.postPebblesBroadcasts);
			}
			else
			{
				room.game.rainWorld.progression.miscProgressionData.prePebblesBroadcasts = Mathf.Max(num, room.game.rainWorld.progression.miscProgressionData.prePebblesBroadcasts);
			}
		}
	}

	public override bool CanBeGrabbed(Creature grabber)
	{
		if (cantBeGrabbedCounter > 0)
		{
			return false;
		}
		return base.CanBeGrabbed(grabber);
	}

	public void SetMalnourished(bool m)
	{
		Custom.Log(m ? "NOW MALNOURISHED" : "NO LONGER MALNOURISHED");
		if (ModManager.MSC && npcCharacterStats != null)
		{
			npcCharacterStats = new SlugcatStats(SlugCatClass, m);
			if (isSlugpup)
			{
				npcCharacterStats.bodyWeightFac = 0.45f;
			}
		}
		else
		{
			base.abstractCreature.world.game.session.characterStats = new SlugcatStats(base.abstractCreature.world.game.StoryCharacter, m);
			if (ModManager.CoopAvailable && !isNPC && base.abstractCreature.world.game.session is StoryGameSession storyGameSession)
			{
				storyGameSession.CreateJollySlugStats(m);
			}
			if (ModManager.MSC && base.abstractCreature.world.game.session.characterStats.name == MoreSlugcatsEnums.SlugcatStatsName.Spear)
			{
				slugcatStats.bodyWeightFac = 0.72f;
			}
		}
		float num = 0.7f * slugcatStats.bodyWeightFac;
		base.bodyChunks[0].mass = num / 2f;
		base.bodyChunks[1].mass = num / 2f;
	}

	public void AddFood(int add)
	{
		if (redsIllness != null)
		{
			redsIllness.AddFood(add);
		}
		else
		{
			add = Math.Min(add, MaxFoodInStomach - this.playerState.foodInStomach);
			if (ModManager.CoopAvailable && base.abstractCreature.world.game.IsStorySession && base.abstractCreature.world.game.Players[0] != base.abstractCreature && !isNPC)
			{
				PlayerState playerState = base.abstractCreature.world.game.Players[0].state as PlayerState;
				add = Math.Min(add, Math.Max(MaxFoodInStomach - playerState.foodInStomach, 0));
				JollyCustom.Log($"Player add food {this.playerState.playerNumber}. Amount to add {add}");
				playerState.foodInStomach += add;
			}
			if (base.abstractCreature.world.game.IsStorySession && AI == null)
			{
				base.abstractCreature.world.game.GetStorySession.saveState.totFood += add;
			}
			this.playerState.foodInStomach += add;
		}
		if (FoodInStomach >= MaxFoodInStomach)
		{
			this.playerState.quarterFoodPoints = 0;
		}
		if (!slugcatStats.malnourished || this.playerState.foodInStomach < ((redsIllness != null) ? redsIllness.FoodToBeOkay : slugcatStats.maxFood))
		{
			return;
		}
		if (redsIllness != null)
		{
			redsIllness.GetBetter();
			return;
		}
		if (!isSlugpup)
		{
			SetMalnourished(m: false);
		}
		if (this.playerState is PlayerNPCState)
		{
			(this.playerState as PlayerNPCState).Malnourished = false;
		}
	}

	public int FoodInRoom(bool eatAndDestroy)
	{
		if (slugcatStats.name == SlugcatStats.Name.Red)
		{
			return FoodInStomach;
		}
		return FoodInRoom(room, eatAndDestroy);
	}

	public int FoodInRoom(Room checkRoom, bool eatAndDestroy)
	{
		if (slugcatStats.name == SlugcatStats.Name.Red)
		{
			return FoodInStomach;
		}
		if (eatAndDestroy)
		{
			Custom.Log("Eat edibles in room");
		}
		if (eatAndDestroy && checkRoom.game.session is StoryGameSession && !(checkRoom.game.session as StoryGameSession).saveState.deathPersistentSaveData.reinforcedKarma)
		{
			for (int num = checkRoom.abstractRoom.entities.Count - 1; num >= 0; num--)
			{
				if (checkRoom.abstractRoom.entities[num] is AbstractPhysicalObject && (checkRoom.abstractRoom.entities[num] as AbstractPhysicalObject).realizedObject != null && (checkRoom.abstractRoom.entities[num] as AbstractPhysicalObject).type == AbstractPhysicalObject.AbstractObjectType.KarmaFlower)
				{
					(checkRoom.game.session as StoryGameSession).saveState.deathPersistentSaveData.reinforcedKarma = true;
					if (SessionRecord != null)
					{
						SessionRecord.AddEat((checkRoom.abstractRoom.entities[num] as AbstractPhysicalObject).realizedObject);
					}
					break;
				}
			}
		}
		if (FoodInStomach >= MaxFoodInStomach)
		{
			return FoodInStomach;
		}
		if (ModManager.MSC && slugcatStats.name == MoreSlugcatsEnums.SlugcatStatsName.Spear)
		{
			return FoodInStomach;
		}
		int num2 = FoodInStomach;
		int num3 = 0;
		for (int i = 0; i < 2; i++)
		{
			if (base.grasps[i] == null || !(base.grasps[i].grabbed is Fly))
			{
				continue;
			}
			PhysicalObject grabbed = base.grasps[i].grabbed;
			int num4 = SlugcatStats.NourishmentOfObjectEaten(SlugCatClass, grabbed as IPlayerEdible);
			if (num4 != -1)
			{
				for (num3 += num4; num3 >= 4; num3 -= 4)
				{
					num2++;
				}
				if (eatAndDestroy)
				{
					for (int j = 0; j < base.abstractCreature.stuckObjects.Count; j++)
					{
						if (base.abstractCreature.stuckObjects[j].A == base.abstractCreature && base.abstractCreature.stuckObjects[j].B == grabbed.abstractPhysicalObject)
						{
							base.abstractCreature.stuckObjects[j].Deactivate();
							break;
						}
					}
					if (SessionRecord != null)
					{
						SessionRecord.AddEat(grabbed);
					}
					grabbed.Destroy();
					checkRoom.RemoveObject(grabbed);
					checkRoom.abstractRoom.RemoveEntity(grabbed.abstractPhysicalObject);
					ReleaseGrasp(i);
				}
			}
			if (num2 >= MaxFoodInStomach)
			{
				return num2;
			}
		}
		if (num2 >= slugcatStats.foodToHibernate)
		{
			return num2;
		}
		if (ModManager.MMF && !eatAndDestroy && !MMF.cfgVanillaExploits.Value)
		{
			num2 = FoodInStomach;
			num3 = 0;
		}
		for (int num5 = checkRoom.abstractRoom.entities.Count - 1; num5 >= 0; num5--)
		{
			if (checkRoom.abstractRoom.entities[num5] is AbstractPhysicalObject && ObjectCountsAsFood((checkRoom.abstractRoom.entities[num5] as AbstractPhysicalObject).realizedObject))
			{
				PhysicalObject realizedObject = (checkRoom.abstractRoom.entities[num5] as AbstractPhysicalObject).realizedObject;
				int num6 = SlugcatStats.NourishmentOfObjectEaten(SlugCatClass, realizedObject as IPlayerEdible);
				if (num6 != -1)
				{
					for (num3 += num6; num3 >= 4; num3 -= 4)
					{
						num2++;
					}
					if (eatAndDestroy)
					{
						for (int k = 0; k < base.abstractCreature.stuckObjects.Count; k++)
						{
							if (base.abstractCreature.stuckObjects[k].A == base.abstractCreature && base.abstractCreature.stuckObjects[k].B == realizedObject.abstractPhysicalObject)
							{
								base.abstractCreature.stuckObjects[k].Deactivate();
								break;
							}
						}
						if (SessionRecord != null)
						{
							SessionRecord.AddEat(realizedObject);
						}
						realizedObject.Destroy();
						checkRoom.RemoveObject(realizedObject);
						checkRoom.abstractRoom.RemoveEntity(realizedObject.abstractPhysicalObject);
					}
				}
				if (num2 >= slugcatStats.foodToHibernate)
				{
					return num2;
				}
			}
		}
		return num2;
	}

	private bool ObjectCountsAsFood(PhysicalObject obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (!(obj is IPlayerEdible))
		{
			return false;
		}
		if ((obj as IPlayerEdible).FoodPoints == 0)
		{
			return false;
		}
		if (!(obj as IPlayerEdible).Edible)
		{
			return false;
		}
		if (obj is SSOracleSwarmer)
		{
			return false;
		}
		if (!(obj is Creature))
		{
			return true;
		}
		if (obj.grabbedBy.Count > 0 && obj.grabbedBy[0].grabber == this)
		{
			return true;
		}
		if ((obj as Creature).dead)
		{
			return true;
		}
		return false;
	}

	public void AerobicIncrease(float f)
	{
		aerobicLevel = Mathf.Min(1f, aerobicLevel + f / 9f);
	}

	public Player(AbstractCreature abstractCreature, World world)
		: base(abstractCreature, world)
	{
		if (ModManager.MMF && MMF.cfgKeyItemTracking.Value && world.game.IsStorySession && !world.singleRoomWorld)
		{
			lastGoodTrackerSpawnRoom = abstractCreature.Room.name;
			lastGoodTrackerSpawnRegion = world.region.name;
			lastGoodTrackerSpawnCoord = abstractCreature.pos;
		}
		customPlayerGravity = 0.9f;
		godDeactiveTimer = 400f;
		lastPingRegion = "";
		GetInitialSlugcatClass();
		if (isSlugpup)
		{
			npcStats = new NPCStats(this);
		}
		if (ModManager.MSC && SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer && base.abstractCreature.world.game.IsStorySession && base.abstractCreature.world.game.GetStorySession.saveState.deathPersistentSaveData.altEnding)
		{
			(base.abstractCreature.world.game.session as StoryGameSession).saveState.deathPersistentSaveData.karma = 0;
			(base.abstractCreature.world.game.session as StoryGameSession).saveState.deathPersistentSaveData.karmaCap = 0;
		}
		feetStuckPos = null;
		standing = false;
		animationFrame = 0;
		superLaunchJump = 0;
		directionBoosts = new float[4];
		float num = 0.7f * slugcatStats.bodyWeightFac;
		base.bodyChunks = new BodyChunk[2];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 9f, num / 2f);
		base.bodyChunks[1] = new BodyChunk(this, 1, new Vector2(0f, 0f), 8f, num / 2f);
		bodyChunkConnections = new BodyChunkConnection[1];
		bodyChunkConnections[0] = new BodyChunkConnection(base.bodyChunks[0], base.bodyChunks[1], 17f, BodyChunkConnection.Type.Normal, 1f, 0.5f);
		input = new InputPackage[10];
		for (int i = 0; i < 10; i++)
		{
			int num2 = playerState.playerNumber;
			if (ModManager.MSC && abstractCreature.world.game.IsArenaSession && abstractCreature.world.game.GetArenaGameSession.chMeta != null)
			{
				num2 = 0;
			}
			input[i] = new InputPackage(AI == null && world.game.rainWorld.options.controls[num2].gamePad, world.game.rainWorld.options.controls[num2].GetActivePreset(), 0, 0, jmp: false, thrw: false, pckp: false, mp: false, crouchToggle: false);
		}
		animation = AnimationIndex.None;
		bodyMode = BodyModeIndex.Default;
		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		bounce = 0.1f;
		surfaceFriction = 0.5f;
		collisionLayer = 1;
		base.waterFriction = 0.96f;
		base.buoyancy = 0.95f;
		airInLungs = 1f;
		flipDirection = 1;
		if ((double)UnityEngine.Random.value < 0.5)
		{
			flipDirection = -1;
		}
		room = world.GetAbstractRoom(abstractCreature.pos.room).realizedRoom;
		if (ModManager.MSC && SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint)
		{
			maxGodTime = (int)(200f + 40f * (float)Karma);
			if (room != null && room.world.name == "HR")
			{
				maxGodTime = 560f;
			}
			godTimer = maxGodTime;
			tongue = new Tongue(this, 0);
		}
		else
		{
			tongue = null;
		}
		swimBits = new CoralCircuit.CircuitBit[2];
		if (room != null)
		{
			if (AI == null)
			{
				glowing = (room.game.session is StoryGameSession && (room.game.session as StoryGameSession).saveState.theGlow) || room.game.setupValues.playerGlowing;
				if (room.game.session is StoryGameSession && (room.game.session as StoryGameSession).saveState.swallowedItems != null && playerState.playerNumber < (room.game.session as StoryGameSession).saveState.swallowedItems.Length && (room.game.session as StoryGameSession).saveState.swallowedItems[playerState.playerNumber] != "" && (room.game.session as StoryGameSession).saveState.swallowedItems[playerState.playerNumber] != "0")
				{
					string text = (room.game.session as StoryGameSession).saveState.swallowedItems[playerState.playerNumber];
					if (text.Contains("<oA>"))
					{
						objectInStomach = SaveState.AbstractPhysicalObjectFromString(world, text);
					}
					else if (text.Contains("<cA>"))
					{
						objectInStomach = SaveState.AbstractCreatureFromString(world, text, onlyInCurrentRegion: false);
					}
					if (objectInStomach != null)
					{
						objectInStomach.pos = abstractCreature.pos;
					}
				}
			}
			else
			{
				glowing = (playerState as PlayerNPCState).Glowing;
				if (room.game.session is StoryGameSession)
				{
					objectInStomach = (playerState as PlayerNPCState).StomachObject;
				}
			}
		}
		if (abstractCreature.Room.world.game.IsArenaSession)
		{
			glowing = abstractCreature.Room.world.game.GetArenaGameSession.playersGlowing;
		}
		if (world.GetAbstractRoom(abstractCreature.pos.room).shelter)
		{
			sleepCounter = 100;
			for (int j = 0; j < world.GetAbstractRoom(abstractCreature.pos.room).creatures.Count; j++)
			{
				if (world.GetAbstractRoom(abstractCreature.pos.room).creatures[j].creatureTemplate.type != CreatureTemplate.Type.Slugcat && (!ModManager.MSC || world.GetAbstractRoom(abstractCreature.pos.room).creatures[j].creatureTemplate.type != MoreSlugcatsEnums.CreatureTemplateType.SlugNPC))
				{
					sleepCounter = 0;
				}
			}
		}
		if (ModManager.MSC && SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel && abstractCreature.Room.world.game.IsStorySession)
		{
			AbstractPhysicalObject abstractPhysicalObject = new AbstractPhysicalObject(abstractCreature.Room.world, MoreSlugcatsEnums.AbstractObjectType.SingularityBomb, null, room.GetWorldCoordinate(base.mainBodyChunk.pos), abstractCreature.Room.world.game.GetNewID());
			abstractCreature.Room.AddEntity(abstractPhysicalObject);
			abstractPhysicalObject.RealizeInRoom();
		}
		if (SlugCatClass == SlugcatStats.Name.Red && !playerState.isGhost)
		{
			if (room.game.devToolsActive && room.game.rainWorld.buildType == RainWorld.BuildType.Development && room.game.manager.menuSetup.startGameCondition == ProcessManager.MenuSetup.StoryGameInitCondition.Dev && objectInStomach == null)
			{
				objectInStomach = new AbstractConsumable(world, AbstractPhysicalObject.AbstractObjectType.NSHSwarmer, null, abstractCreature.pos, world.game.GetNewID(), -1, -1, null);
			}
			spearOnBack = new SpearOnBack(this);
			if (abstractCreature.world.game.IsStorySession && abstractCreature.world.game.GetStorySession.saveState.cycleNumber >= RedsIllness.RedsCycles(abstractCreature.world.game.GetStorySession.saveState.redExtraCycles) && (!ModManager.CoopAvailable || !(abstractCreature.world.game.StoryCharacter != SlugcatStats.Name.Red)))
			{
				redsIllness = new RedsIllness(this, Math.Abs(RedsIllness.RedsCycles(abstractCreature.world.game.GetStorySession.saveState.redExtraCycles) - abstractCreature.world.game.GetStorySession.saveState.cycleNumber));
			}
			if (ModManager.Expedition && Custom.rainWorld.ExpeditionMode)
			{
				redsIllness = null;
			}
		}
		if (ModManager.Expedition && Custom.rainWorld.ExpeditionMode && ExpeditionGame.activeUnlocks.Contains("unl-backspear"))
		{
			spearOnBack = new SpearOnBack(this);
		}
		standStillOnMapButton = world.game.IsStorySession;
		if (ModManager.MSC)
		{
			if (!isSlugpup)
			{
				slugOnBack = new SlugOnBack(this);
			}
			if (AI == null)
			{
				ChatlogData.setHostPlayer(this);
			}
			if (world.game.rainWorld.setup.forcePup || (world.game.autoPupStoryCompanionPlayers && playerState.playerNumber > 0))
			{
				setPupStatus(set: true);
			}
			else if (playerState.forceFullGrown)
			{
				setPupStatus(set: false);
			}
			else if (isSlugpup)
			{
				setPupStatus(set: true);
			}
			else if (playerState.isPup)
			{
				setPupStatus(set: true);
			}
			if (isSlugpup && playerState is PlayerNPCState)
			{
				glowing = (playerState as PlayerNPCState).Glowing;
				SetMalnourished((playerState as PlayerNPCState).Malnourished || base.dead);
			}
		}
		if (ModManager.CoopAvailable)
		{
			bool1 = Custom.rainWorld.options.jollyPlayerOptionsArray[playerState.playerNumber].customPlayerName == JollyCustom.Test1();
			if (!isSlugpup)
			{
				slugOnBack = new SlugOnBack(this);
			}
			if (playerState.isPup || bool1)
			{
				setPupStatus(set: true);
			}
		}
	}

	public override void InitiateGraphicsModule()
	{
		if (base.graphicsModule == null)
		{
			base.graphicsModule = new PlayerGraphics(this);
		}
	}

	public override void RemoveGraphicsModule()
	{
	}

	public override void Deafen(int df)
	{
		if (df > 40)
		{
			base.Deafen(df / 2);
		}
	}

	public override void Update(bool eu)
	{
		if (ModManager.MSC)
		{
			if (saintWeakness > 0)
			{
				saintWeakness--;
			}
			if (scavengerImmunity > 0)
			{
				scavengerImmunity--;
			}
			for (int i = 0; i < base.grasps.Length; i++)
			{
				if (base.grasps[i] != null && base.grasps[i].grabbed is VultureMask && (base.grasps[i].grabbed as VultureMask).maskGfx.ScavKing)
				{
					scavengerImmunity = 2400;
				}
			}
			if (isNPC && base.grasps[1] != null)
			{
				ReleaseGrasp(1);
			}
			if (isSlugpup)
			{
				if (base.Consious && grabbedBy.Count > 0 && grabbedBy[0].grabber is Player)
				{
					if (base.firstChunk.grabsDisableFloors)
					{
						for (int j = 0; j < base.bodyChunks.Length; j++)
						{
							base.bodyChunks[j].grabsDisableFloors = false;
						}
					}
				}
				else if (!base.firstChunk.grabsDisableFloors)
				{
					for (int k = 0; k < base.bodyChunks.Length; k++)
					{
						base.bodyChunks[k].grabsDisableFloors = true;
					}
				}
			}
		}
		if (room != null && room.blizzard)
		{
			if (ModManager.MSC && SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
			{
				base.Hypothermia -= Mathf.Lerp(RainWorldGame.DefaultHeatSourceWarmth, 0f, HypothermiaExposure);
			}
			if (objectInStomach != null && objectInStomach.type == AbstractPhysicalObject.AbstractObjectType.Lantern)
			{
				base.Hypothermia -= Mathf.Lerp(RainWorldGame.DefaultHeatSourceWarmth, 0f, HypothermiaExposure);
			}
			if (room.game.cameras[0].ghostMode >= 1f)
			{
				HypothermiaGain = 0f;
				base.Hypothermia = Mathf.Lerp(base.Hypothermia, 0f, room.game.cameras[0].ghostMode / 100f);
			}
			if (base.Hypothermia < 0f)
			{
				base.Hypothermia = 0f;
			}
		}
		if (!addedSpawnRoomToDiscovery && room != null)
		{
			if (room.game.session is StoryGameSession && room.world.region != null && !(room.game.session as StoryGameSession).saveState.regionStates[room.world.region.regionNumber].roomsVisited.Contains(room.abstractRoom.name))
			{
				(room.game.session as StoryGameSession).saveState.regionStates[room.world.region.regionNumber].roomsVisited.Add(room.abstractRoom.name);
			}
			addedSpawnRoomToDiscovery = true;
		}
		if (!inVoidSea)
		{
			for (int l = 0; l < base.bodyChunks.Length; l++)
			{
				base.bodyChunks[l].restrictInRoomRange = base.bodyChunks[l].defaultRestrictInRoomRange;
			}
		}
		if (base.Consious)
		{
			SleepUpdate();
		}
		if (ModManager.MMF && (canJump > 0 || bodyMode == BodyModeIndex.Swimming || bodyMode == BodyModeIndex.ClimbingOnBeam || bodyMode == BodyModeIndex.ZeroG || bodyMode == BodyModeIndex.CorridorClimb))
		{
			lastGroundY = base.firstChunk.pos.y;
		}
		if (slugcatStats.name == SlugcatStats.Name.Yellow)
		{
			if (AI == null && !glowing && HologramLight.Needed(this) > 0.5f && !room.abstractRoom.shelter && !room.abstractRoom.gate && room.game.IsStorySession && room.game.GetStorySession.saveState.miscWorldSaveData.playerGuideState.likesPlayer > -0.8f && !room.game.GetStorySession.saveState.miscWorldSaveData.playerGuideState.angryWithPlayer)
			{
				HologramLight.TryCreate(this);
			}
		}
		else if (slugcatStats.name == SlugcatStats.Name.Red)
		{
			if (redsIllness != null)
			{
				redsIllness.Update();
			}
		}
		else if (ModManager.MSC && room.game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Artificer && slugcatStats.name == MoreSlugcatsEnums.SlugcatStatsName.Artificer && AI == null && room.game.cameras[0] != null && room.game.cameras[0].hud != null && room.game.cameras[0].hud.karmaMeter != null)
		{
			Scavenger scavenger = null;
			if (base.grasps.Length != 0)
			{
				for (int m = 0; m < base.grasps.Length; m++)
				{
					if (base.grasps[m] != null && base.grasps[m].grabbedChunk != null && base.grasps[m].grabbedChunk.owner is Scavenger && (base.grasps[m].grabbedChunk.owner as Scavenger).dead)
					{
						scavenger = base.grasps[m].grabbedChunk.owner as Scavenger;
						break;
					}
				}
			}
			if (scavenger != null && room.game.cameras[0].hud.karmaMeter.forceVisibleCounter < 40)
			{
				room.game.cameras[0].hud.karmaMeter.forceVisibleCounter = 40;
			}
			if (room.game.cameras[0].hud.karmaMeter.forceVisibleCounter > 0)
			{
				if (scavenger != null)
				{
					int num = Mathf.Clamp((base.abstractCreature.world.game.session as StoryGameSession).saveState.deathPersistentSaveData.karma + scavenger.abstractCreature.karmicPotential, 0, 9);
					int a = 5;
					if (num > 4)
					{
						a = Mathf.Max(6, num);
					}
					a = Mathf.Max(a, Mathf.Clamp(KarmaCap, 4, 9));
					if (num > a)
					{
						a = num;
					}
					room.game.cameras[0].hud.karmaMeter.ClearScavengerFlash();
					room.game.cameras[0].hud.karmaMeter.displayKarma = new IntVector2(num, a);
				}
				else
				{
					int x = room.game.cameras[0].hud.karmaMeter.displayKarma.x;
					int num2 = Mathf.Min(Karma, KarmaCap);
					if (x > num2)
					{
						room.game.cameras[0].hud.karmaMeter.DropScavengerFlash();
					}
					room.game.cameras[0].hud.karmaMeter.displayKarma = new IntVector2(num2, Mathf.Clamp(KarmaCap, 4, 9));
				}
				room.game.cameras[0].hud.karmaMeter.karmaSprite.element = Futile.atlasManager.GetElementWithName(KarmaMeter.KarmaSymbolSprite(small: true, room.game.cameras[0].hud.karmaMeter.displayKarma));
			}
		}
		if (MapDiscoveryActive && base.coord != lastCoord)
		{
			if (exitsToBeDiscovered == null)
			{
				if (room != null && room.shortCutsReady)
				{
					exitsToBeDiscovered = new List<Vector2>();
					for (int n = 0; n < room.shortcuts.Length; n++)
					{
						if (room.shortcuts[n].shortCutType == ShortcutData.Type.RoomExit)
						{
							exitsToBeDiscovered.Add(room.MiddleOfTile(room.shortcuts[n].StartTile));
						}
					}
				}
			}
			else if (exitsToBeDiscovered.Count > 0 && room.game.cameras[0].hud != null && room.game.cameras[0].hud.map != null && !room.CompleteDarkness(base.firstChunk.pos, 0f, 0.95f, checkForPlayers: false))
			{
				int index = UnityEngine.Random.Range(0, exitsToBeDiscovered.Count);
				if (room.ViewedByAnyCamera(exitsToBeDiscovered[index], -10f))
				{
					Vector2 pos = base.firstChunk.pos;
					for (int num3 = 0; num3 < 20; num3++)
					{
						if (Custom.DistLess(pos, exitsToBeDiscovered[index], 50f))
						{
							room.game.cameras[0].hud.map.ExternalExitDiscover((pos + exitsToBeDiscovered[index]) / 2f, room.abstractRoom.index);
							room.game.cameras[0].hud.map.ExternalOnePixelDiscover(exitsToBeDiscovered[index], room.abstractRoom.index);
							exitsToBeDiscovered.RemoveAt(index);
							break;
						}
						room.game.cameras[0].hud.map.ExternalSmallDiscover(pos, room.abstractRoom.index);
						pos += Custom.DirVec(pos, exitsToBeDiscovered[index]) * 50f;
					}
				}
			}
		}
		if (slugcatStats.malnourished || (ModManager.MSC && (saintWeakness > 0 || Wounded)))
		{
			if (aerobicLevel == 1f)
			{
				exhausted = true;
			}
			else if (aerobicLevel < 0.4f)
			{
				exhausted = false;
			}
			if (exhausted)
			{
				slowMovementStun = Math.Max(slowMovementStun, (int)Custom.LerpMap(aerobicLevel, 0.7f, 0.4f, 6f, 0f));
				if (aerobicLevel > 0.9f && UnityEngine.Random.value < 0.05f)
				{
					Stun(7);
				}
				if (aerobicLevel > 0.9f && UnityEngine.Random.value < 0.1f)
				{
					standing = false;
				}
				if (!lungsExhausted || !(animation != AnimationIndex.SurfaceSwim))
				{
					swimCycle += 0.05f;
				}
			}
			else
			{
				slowMovementStun = Math.Max(slowMovementStun, (int)Custom.LerpMap(aerobicLevel, 1f, 0.4f, 2f, 0f, 2f));
			}
		}
		else
		{
			exhausted = false;
		}
		if (lungsExhausted && (!ModManager.MSC || !gourmandExhausted))
		{
			aerobicLevel = 1f;
		}
		else if (ModManager.MSC && gourmandExhausted)
		{
			float num4 = 800f;
			float num5 = 200f;
			if (bodyMode == BodyModeIndex.Crawl)
			{
				num4 = 400f;
				num5 = 125f;
			}
			aerobicLevel = Mathf.Max(1f - airInLungs, aerobicLevel - ((!slugcatStats.malnourished) ? 1f : 1.2f) / (((input[0].x != 0 || input[0].y != 0) ? num4 : num5) * (1f + 3f * Mathf.InverseLerp(0.9f, 1f, aerobicLevel))));
		}
		else
		{
			aerobicLevel = Mathf.Max(1f - airInLungs, aerobicLevel - (slugcatStats.malnourished ? 1.2f : 1f) / (((input[0].x == 0 && input[0].y == 0) ? 400f : 1100f) * (1f + 3f * Mathf.InverseLerp(0.9f, 1f, aerobicLevel))));
		}
		if (ModManager.MSC && Wounded)
		{
			if (aerobicLevel > 0.98f)
			{
				Stun(UnityEngine.Random.Range(40, 60));
				aerobicLevel = 0.35f;
			}
			if (UnityEngine.Random.value < Mathf.Lerp(0.004f, 0.02f, (float)(base.State as PlayerState).permanentDamageTracking))
			{
				room.AddObject(new WaterDrip(base.firstChunk.pos + Custom.RNV() * UnityEngine.Random.Range(5f, 10f), default(Vector2), waterColor: false));
			}
		}
		if (room.game.devToolsActive && Input.GetKey("g") && AI == null && room.game.session is StoryGameSession)
		{
			for (int num6 = 0; num6 < ExtEnum<WinState.EndgameID>.values.Count; num6++)
			{
				WinState.EndgameID endgameID = new WinState.EndgameID(ExtEnum<WinState.EndgameID>.values.GetEntry(num6));
				if ((room.game.session as StoryGameSession).saveState.deathPersistentSaveData.winState.GetTracker(endgameID, addIfMissing: false) != null && (room.game.session as StoryGameSession).saveState.deathPersistentSaveData.winState.GetTracker(endgameID, addIfMissing: false) is WinState.IntegerTracker)
				{
					Custom.Log($"{endgameID}: {((room.game.session as StoryGameSession).saveState.deathPersistentSaveData.winState.GetTracker(endgameID, addIfMissing: false) as WinState.IntegerTracker).progress}");
				}
			}
		}
		if (room.game.IsStorySession)
		{
			if (!ModManager.MMF && room.game.cameras[0].hud != null && !room.game.cameras[0].hud.textPrompt.gameOverMode)
			{
				SessionRecord.time++;
				if (ModManager.CoopAvailable)
				{
					base.abstractCreature.world.game.GetStorySession.playerSessionRecords[0].time = Mathf.Max(base.abstractCreature.world.game.GetStorySession.playerSessionRecords[0].time, SessionRecord.time);
				}
			}
			if (AI == null && PlaceKarmaFlower && !base.dead && grabbedBy.Count == 0 && IsTileSolid(1, 0, -1) && !room.GetTile(base.bodyChunks[1].pos).DeepWater && !IsTileSolid(1, 0, 0) && !IsTileSolid(1, 0, 1) && !room.GetTile(base.bodyChunks[1].pos).wormGrass && (!room.readyForAI || !room.aimap.getAItile(room.GetTilePosition(base.bodyChunks[1].pos)).narrowSpace) && !room.abstractRoom.shelter)
			{
				karmaFlowerGrowPos = room.GetWorldCoordinate(base.bodyChunks[1].pos);
				foreach (AbstractCreature player in room.game.Players)
				{
					Creature realizedCreature = player.realizedCreature;
					if (realizedCreature != null && realizedCreature.dead)
					{
						(player.realizedCreature as Player).karmaFlowerGrowPos = karmaFlowerGrowPos;
					}
				}
			}
		}
		if (cantBeGrabbedCounter > 0)
		{
			cantBeGrabbedCounter--;
		}
		if (poleSkipPenalty > 0)
		{
			poleSkipPenalty--;
		}
		if (shootUpCounter > 0)
		{
			noGrabCounter = Math.Max(noGrabCounter, 2);
			shootUpCounter--;
			if (!input[0].jmp || input[0].y < 1 || base.mainBodyChunk.pos.y < base.mainBodyChunk.lastPos.y)
			{
				shootUpCounter = 0;
			}
		}
		if (dangerGrasp == null)
		{
			dangerGraspTime = 0;
		}
		else if (dangerGrasp.discontinued)
		{
			dangerGrasp = null;
			dangerGraspTime = 0;
		}
		else
		{
			dangerGraspTime++;
			if (dangerGraspTime == 30)
			{
				LoseAllGrasps();
			}
			else if (dangerGraspTime < 30)
			{
				int playerNumber = playerState.playerNumber;
				if (ModManager.MSC && base.abstractCreature.world.game.IsArenaSession && base.abstractCreature.world.game.GetArenaGameSession.chMeta != null)
				{
					playerNumber = 0;
				}
				InputPackage inputPackage = RWInput.PlayerInput(playerNumber);
				if (!base.dead && base.stun < 30)
				{
					if ((inputPackage.thrw && !dangerGraspLastThrowButton) || (AI != null && UnityEngine.Random.value < base.abstractCreature.personality.nervous * 0.25f))
					{
						ThrowToGetFree(eu);
					}
					if ((inputPackage.pckp && !dangerGraspPickupButton) || (AI != null && UnityEngine.Random.value < base.abstractCreature.personality.nervous * 0.5f))
					{
						DangerGraspPickup(eu);
					}
				}
				dangerGraspLastThrowButton = inputPackage.thrw;
				dangerGraspPickupButton = inputPackage.pckp;
			}
			else if (dangerGraspTime == 60 && AI == null)
			{
				room.game.GameOver(dangerGrasp);
			}
			if ((ModManager.MSC || ModManager.CoopAvailable) && slugOnBack != null && slugOnBack.slugcat != null)
			{
				slugOnBack.DropSlug();
			}
		}
		if (dontEatExternalFoodSourceCounter > 0)
		{
			dontEatExternalFoodSourceCounter--;
		}
		if (eatExternalFoodSourceCounter > 0)
		{
			eatExternalFoodSourceCounter--;
			if (eatExternalFoodSourceCounter < 1)
			{
				AddFood(1);
				dontEatExternalFoodSourceCounter = 45;
				handOnExternalFoodSource = null;
				room.PlaySound(SoundID.Slugcat_Bite_Fly, base.mainBodyChunk);
			}
		}
		if (handOnExternalFoodSource.HasValue && (!base.Consious || !Custom.DistLess(base.mainBodyChunk.pos, handOnExternalFoodSource.Value, 45f) || eatExternalFoodSourceCounter < 1))
		{
			handOnExternalFoodSource = null;
		}
		if (bodyMode == BodyModeIndex.ZeroG)
		{
			privSneak = 0.5f;
			base.bodyChunks[0].loudness = 0.5f * slugcatStats.loudnessFac;
			base.bodyChunks[1].loudness = 0.5f * slugcatStats.loudnessFac;
		}
		else
		{
			if ((!standing || bodyMode == BodyModeIndex.Crawl || bodyMode == BodyModeIndex.CorridorClimb || bodyMode == BodyModeIndex.ClimbIntoShortCut || (animation == AnimationIndex.HangFromBeam && input[0].x == 0) || (animation == AnimationIndex.ClimbOnBeam && input[0].y == 0)) && bodyMode != BodyModeIndex.Default)
			{
				privSneak = Mathf.Min(privSneak + 0.1f, 1f);
			}
			else
			{
				privSneak = Mathf.Max(privSneak - 0.04f, 0f);
			}
			base.bodyChunks[0].loudness = 1.5f * (1f - Sneak) * slugcatStats.loudnessFac;
			base.bodyChunks[1].loudness = 0.7f * (1f - Sneak) * slugcatStats.loudnessFac;
		}
		if (mushroomCounter > 0)
		{
			if (!base.inShortcut)
			{
				mushroomCounter--;
			}
			mushroomEffect = Custom.LerpAndTick(mushroomEffect, 1f, 0.05f, 0.025f);
			if (ModManager.CoopAvailable)
			{
				foreach (AbstractCreature alivePlayer in base.abstractCreature.world.game.AlivePlayers)
				{
					if (alivePlayer.realizedCreature != null)
					{
						(alivePlayer.realizedCreature as Player).mushroomEffect = Mathf.Max(mushroomEffect, (alivePlayer.realizedCreature as Player).mushroomEffect);
					}
				}
			}
		}
		else
		{
			mushroomEffect = Custom.LerpAndTick(mushroomEffect, 0f, 0.025f, 1f / 70f);
		}
		if (AI == null)
		{
			if (Adrenaline > 0f)
			{
				if (adrenalineEffect == null)
				{
					adrenalineEffect = new AdrenalineEffect(this);
					room.AddObject(adrenalineEffect);
				}
				else if (adrenalineEffect.slatedForDeletetion)
				{
					adrenalineEffect = null;
				}
			}
			if (base.Deaf > 0f && deafLoopHolder == null)
			{
				deafLoopHolder = new DeafLoopHolder(this);
				room.AddObject(deafLoopHolder);
			}
			else if (base.Deaf == 0f && deafLoopHolder != null && deafLoopHolder.deafLoop == null)
			{
				deafLoopHolder.Destroy();
				deafLoopHolder = null;
			}
		}
		SoundID soundID = SoundID.None;
		if (Adrenaline > 0.5f)
		{
			soundID = SoundID.Mushroom_Trip_LOOP;
		}
		else if (base.Stunned)
		{
			soundID = SoundID.UI_Slugcat_Stunned_LOOP;
		}
		else if (corridorDrop || verticalCorridorSlideCounter > 0 || horizontalCorridorSlideCounter > 0)
		{
			soundID = SoundID.Slugcat_Slide_In_Narrow_Corridor_LOOP;
		}
		else if (slideCounter > 0 && bodyMode == BodyModeIndex.Stand)
		{
			soundID = SoundID.Slugcat_Skid_On_Ground_LOOP;
		}
		else if (animation == AnimationIndex.Roll)
		{
			soundID = SoundID.Slugcat_Roll_LOOP;
		}
		else if (animation == AnimationIndex.ClimbOnBeam && input[0].y < 0)
		{
			soundID = SoundID.Slugcat_Slide_Down_Vertical_Beam_LOOP;
		}
		else if (animation == AnimationIndex.BellySlide)
		{
			soundID = SoundID.Slugcat_Belly_Slide_LOOP;
		}
		else if (bodyMode == BodyModeIndex.WallClimb)
		{
			soundID = SoundID.Slugcat_Wall_Slide_LOOP;
		}
		if (soundID != slideLoopSound)
		{
			if (slideLoop != null)
			{
				slideLoop.alive = false;
				slideLoop = null;
			}
			slideLoopSound = soundID;
			if (slideLoopSound != SoundID.None)
			{
				slideLoop = room.PlaySound(slideLoopSound, base.mainBodyChunk, loop: true, 1f, 1f);
				slideLoop.requireActiveUpkeep = true;
			}
		}
		if (slideLoop != null)
		{
			slideLoop.alive = true;
			if (slideLoopSound == SoundID.UI_Slugcat_Stunned_LOOP)
			{
				slideLoop.pitch = 0.5f + Mathf.InverseLerp(11f, lastStun, base.stun);
				slideLoop.volume = Mathf.Pow(Mathf.InverseLerp(8f, 27f, base.stun), 0.7f);
			}
			else if (slideLoopSound == SoundID.Slugcat_Slide_In_Narrow_Corridor_LOOP)
			{
				slideLoop.pitch = Mathf.Lerp(0.7f, 1.3f, base.mainBodyChunk.vel.magnitude / (corridorDrop ? 25f : 12.5f));
				if (verticalCorridorSlideCounter > 0)
				{
					slideLoop.volume = 1f;
				}
				else
				{
					slideLoop.volume = Mathf.Min(1f, base.mainBodyChunk.vel.magnitude / 4f);
				}
			}
			else if (slideLoopSound == SoundID.Slugcat_Belly_Slide_LOOP)
			{
				slideLoop.pitch = Mathf.Lerp(0.5f, 1.5f, Mathf.Abs(base.mainBodyChunk.vel.x) / 25.5f);
				slideLoop.volume = Mathf.Min(1f, Mathf.Abs(base.mainBodyChunk.vel.x) / 10f);
			}
			else if (slideLoopSound == SoundID.Slugcat_Roll_LOOP)
			{
				slideLoop.pitch = Mathf.Lerp(0.85f, 1.15f, 0.5f + Custom.DirVec(base.mainBodyChunk.pos, base.bodyChunks[1].pos).y * 0.5f);
				slideLoop.volume = 0.5f + Mathf.Abs(Custom.DirVec(base.mainBodyChunk.pos, base.bodyChunks[1].pos).x) * 0.5f;
			}
			else if (slideLoopSound == SoundID.Slugcat_Skid_On_Ground_LOOP)
			{
				slideLoop.pitch = Mathf.Lerp(0.7f, 1.3f, Mathf.Abs(base.mainBodyChunk.vel.x) / 9.5f);
				slideLoop.volume = Mathf.Min(1f, Mathf.Abs(base.mainBodyChunk.vel.x) / 6f);
			}
			else if (slideLoopSound == SoundID.Slugcat_Slide_Down_Vertical_Beam_LOOP)
			{
				slideLoop.pitch = Mathf.Lerp(0.7f, 1.3f, Mathf.Abs(base.mainBodyChunk.vel.y) / 4.9f);
				slideLoop.volume = Mathf.Min(1f, Mathf.Abs(base.mainBodyChunk.vel.y) / 2.5f);
			}
			else if (slideLoopSound == SoundID.Slugcat_Wall_Slide_LOOP)
			{
				slideLoop.pitch = Mathf.Lerp(0.7f, 1.3f, Mathf.Abs(base.mainBodyChunk.pos.y - base.mainBodyChunk.lastPos.y) / 1.75f);
				slideLoop.volume = Mathf.Min(1f, Mathf.Abs(base.mainBodyChunk.vel.y) * 1.5f);
			}
			else if (slideLoopSound == SoundID.Mushroom_Trip_LOOP)
			{
				slideLoop.pitch = 1f;
				slideLoop.volume = Mathf.InverseLerp(0f, 0.3f, Adrenaline);
			}
		}
		surfaceFriction = (base.Consious ? 0.5f : 0.3f);
		if (base.bodyChunks[0].ContactPoint.y != 0 && base.bodyChunks[0].ContactPoint.y == -base.bodyChunks[1].ContactPoint.y && room.GetTile((base.bodyChunks[0].pos + base.bodyChunks[1].pos) / 2f).Solid && grabbedBy.Count < 1 && !room.VisualContact(base.bodyChunks[0].pos, base.bodyChunks[1].pos) && !room.VisualContact(base.bodyChunks[0].pos + Custom.PerpendicularVector(base.bodyChunks[0].pos, base.bodyChunks[1].pos) * base.bodyChunks[0].rad, base.bodyChunks[1].pos + Custom.PerpendicularVector(base.bodyChunks[0].pos, base.bodyChunks[1].pos) * base.bodyChunks[1].rad) && !room.VisualContact(base.bodyChunks[0].pos - Custom.PerpendicularVector(base.bodyChunks[0].pos, base.bodyChunks[1].pos) * base.bodyChunks[0].rad, base.bodyChunks[1].pos - Custom.PerpendicularVector(base.bodyChunks[0].pos, base.bodyChunks[1].pos) * base.bodyChunks[1].rad))
		{
			Custom.LogWarning("WALLSTUCK");
			base.bodyChunks[1].HardSetPosition(base.bodyChunks[0].pos + Custom.DirVec(base.bodyChunks[0].pos, base.bodyChunks[1].pos) * 2f);
		}
		if (dontGrabStuff > 0)
		{
			dontGrabStuff--;
		}
		for (int num7 = 0; num7 < 4; num7++)
		{
			if (directionBoosts[num7] > 0f)
			{
				directionBoosts[num7] = Mathf.Min(directionBoosts[num7] + 0.025f, 1f);
			}
		}
		if (bodyMode == BodyModeIndex.CorridorClimb)
		{
			timeSinceInCorridorMode = 0;
		}
		else
		{
			timeSinceInCorridorMode++;
		}
		if (allowRoll > 0)
		{
			allowRoll--;
		}
		if (room.GetTile(base.bodyChunks[1].pos + new Vector2(0f, -20f)).Terrain == Room.Tile.TerrainType.Air && (!IsTileSolid(1, -1, -1) || !IsTileSolid(1, 1, -1)))
		{
			allowRoll = 15;
		}
		if (base.stun == 12)
		{
			room.PlaySound(SoundID.UI_Slugcat_Exit_Stun, base.mainBodyChunk);
		}
		bool flag = input[0].jmp && !input[1].jmp;
		if (flag)
		{
			if (base.grasps[0] != null && base.grasps[0].grabbed is TubeWorm)
			{
				flag = (base.grasps[0].grabbed as TubeWorm).JumpButton(this);
			}
			else if (base.grasps[1] != null && base.grasps[1].grabbed is TubeWorm)
			{
				flag = (base.grasps[1].grabbed as TubeWorm).JumpButton(this);
			}
		}
		if (wantToJump > 0)
		{
			wantToJump--;
		}
		else if (flag)
		{
			bool flag2 = standing && animation == AnimationIndex.None && Mathf.Abs(base.bodyChunks[1].lastPos.y - base.bodyChunks[1].pos.y) < 20f && base.bodyChunks[1].vel.y < 5f && base.bodyChunks[0].pos.y > base.bodyChunks[1].pos.y + 5f;
			if (isRivulet)
			{
				flag2 = Mathf.Abs(base.bodyChunks[1].lastPos.y - base.bodyChunks[1].pos.y) < 50f && base.bodyChunks[1].vel.y < 15f && (animation == AnimationIndex.None || animation == AnimationIndex.Flip || animation == AnimationIndex.RocketJump || animation == AnimationIndex.Roll);
			}
			if (flag2 && bodyMode == BodyModeIndex.Default && base.bodyChunks[1].ContactPoint.y == 0 && base.bodyChunks[0].ContactPoint.y == 0 && base.bodyChunks[1].ContactPoint.x == 0 && base.bodyChunks[0].ContactPoint.x == 0 && !IsTileSolid(1, -1, 0) && !IsTileSolid(1, 1, 0))
			{
				bool flag3 = room.GetTile(base.bodyChunks[1].pos).horizontalBeam && input[0].x != 0 && Mathf.Min(Mathf.Abs(room.MiddleOfTile(base.bodyChunks[1].pos).y - base.bodyChunks[1].pos.y), Mathf.Abs(room.MiddleOfTile(base.bodyChunks[1].pos).y - base.bodyChunks[1].lastPos.y)) < 7.5f;
				if (isRivulet)
				{
					flag3 = (room.GetTile(base.bodyChunks[1].pos).horizontalBeam || room.GetTile(new Vector2(base.bodyChunks[1].pos.x, base.bodyChunks[1].pos.y - 10f)).horizontalBeam) && Mathf.Min(Mathf.Abs(room.MiddleOfTile(base.bodyChunks[1].pos).y - base.bodyChunks[1].pos.y), Mathf.Abs(room.MiddleOfTile(base.bodyChunks[1].pos).y - base.bodyChunks[1].lastPos.y)) < 22.5f;
				}
				if (flag3 && input[0].y <= 0 && poleSkipPenalty < 1)
				{
					base.bodyChunks[0].vel.y = (isRivulet ? 7f : 4.5f);
					base.bodyChunks[1].vel.y = (isRivulet ? 6f : 3.5f);
					if (animation != AnimationIndex.None)
					{
						for (int num8 = 0; num8 < 7; num8++)
						{
							room.AddObject(new WaterDrip(base.mainBodyChunk.pos + new Vector2(base.mainBodyChunk.rad * base.mainBodyChunk.vel.x, 0f), Custom.DegToVec(UnityEngine.Random.value * 180f * (0f - base.mainBodyChunk.vel.x)) * Mathf.Lerp(10f, 17f, UnityEngine.Random.value), waterColor: false));
						}
					}
					float num9 = room.MiddleOfTile(base.bodyChunks[1].pos).y + 5f - base.bodyChunks[1].pos.y;
					base.bodyChunks[1].pos.y += num9;
					base.bodyChunks[0].pos.y += num9;
					jumpBoost = (isRivulet ? 7 : 6);
					poleSkipPenalty = (isRivulet ? 3 : 6);
					room.PlaySound(SoundID.Slugcat_From_Horizontal_Pole_Jump, base.bodyChunks[1]);
					wantToJump = 0;
					canJump = 0;
				}
				else
				{
					poleSkipPenalty = (isRivulet ? 6 : Math.Min(30, poleSkipPenalty + 12));
					wantToJump = 5;
				}
			}
			else
			{
				wantToJump = 5;
			}
		}
		if (bodyMode == BodyModeIndex.WallClimb)
		{
			wallSlideCounter++;
		}
		else
		{
			wallSlideCounter = 0;
		}
		if (canWallJump > 0)
		{
			canWallJump--;
		}
		else if (canWallJump < 0)
		{
			canWallJump++;
		}
		if (jumpChunkCounter > 0)
		{
			jumpChunkCounter--;
		}
		else if (jumpChunkCounter < 0)
		{
			jumpChunkCounter++;
		}
		if (noGrabCounter > 0)
		{
			noGrabCounter--;
		}
		if (waterJumpDelay > 0)
		{
			waterJumpDelay--;
		}
		if (forceFeetToHorizontalBeamTile > 0)
		{
			forceFeetToHorizontalBeamTile--;
		}
		if (canJump > 0)
		{
			canJump--;
		}
		if (slowMovementStun > 0)
		{
			slowMovementStun--;
		}
		if (backwardsCounter > 0)
		{
			backwardsCounter--;
		}
		if (landingDelay > 0)
		{
			landingDelay--;
		}
		if (verticalCorridorSlideCounter > 0)
		{
			verticalCorridorSlideCounter--;
		}
		if (horizontalCorridorSlideCounter > 0)
		{
			horizontalCorridorSlideCounter--;
		}
		if (jumpStun > 0)
		{
			jumpStun--;
		}
		else if (jumpStun < 0)
		{
			jumpStun++;
		}
		if (!base.dead)
		{
			LungUpdate();
		}
		checkInput();
		if (input[0].downDiagonal != 0 && input[0].downDiagonal == input[1].downDiagonal)
		{
			consistentDownDiagonal++;
		}
		else
		{
			consistentDownDiagonal = 0;
		}
		if (base.dead)
		{
			animation = AnimationIndex.Dead;
			bodyMode = BodyModeIndex.Dead;
		}
		else if (base.stun > 0)
		{
			animation = AnimationIndex.None;
			bodyMode = BodyModeIndex.Stunned;
		}
		if (bodyMode != BodyModeIndex.Swimming)
		{
			if (base.bodyChunks[0].ContactPoint.x != 0 && input[0].x == base.bodyChunks[0].ContactPoint.x && base.bodyChunks[0].vel.y < 0f && bodyMode != BodyModeIndex.CorridorClimb)
			{
				base.bodyChunks[0].vel.y *= Mathf.Clamp(1f - surfaceFriction * ((base.bodyChunks[0].pos.y > base.bodyChunks[1].pos.y) ? 2f : 0.5f), 0f, 1f);
			}
			if (base.bodyChunks[1].ContactPoint.x != 0 && input[0].x == base.bodyChunks[1].ContactPoint.x && base.bodyChunks[1].vel.y < 0f && bodyMode != BodyModeIndex.CorridorClimb)
			{
				base.bodyChunks[1].vel.y *= Mathf.Clamp(1f - surfaceFriction * ((base.bodyChunks[0].pos.y > base.bodyChunks[1].pos.y) ? 2f : 0.5f), 0f, 1f);
			}
		}
		if (room.GetTilePosition(base.bodyChunks[0].pos).y <= room.GetTilePosition(base.bodyChunks[1].pos).y && IsTileSolid(0, 0, 1))
		{
			standing = false;
		}
		if (room.game.devToolsActive)
		{
			bool flag4 = room.game.cameras[0].room == room || !ModManager.CoopAvailable;
			if (Input.GetKey("v") && flag4)
			{
				if (tongue != null)
				{
					tongue.resetRopeLength();
					tongue.mode = Tongue.Mode.Retracted;
					tongue.rope.Reset();
				}
				for (int num10 = 0; num10 < 2; num10++)
				{
					base.bodyChunks[num10].vel = Custom.DegToVec(UnityEngine.Random.value * 360f) * 12f;
					base.bodyChunks[num10].pos = (Vector2)Futile.mousePosition + room.game.cameras[0].pos;
					base.bodyChunks[num10].lastPos = (Vector2)Futile.mousePosition + room.game.cameras[0].pos;
				}
			}
			else if (Input.GetKey("w") && flag4)
			{
				Vector2 p = new Vector2(Futile.mousePosition.x, Futile.mousePosition.y) + room.game.cameras[0].pos;
				base.bodyChunks[1].vel += Custom.DirVec(base.bodyChunks[1].pos, p) * 7f;
			}
		}
		if (base.bodyChunks[0].pos.x == base.bodyChunks[1].pos.x && base.bodyChunks[0].vel.x == 0f && base.bodyChunks[1].vel.x == 0f && base.bodyChunks[0].pos.y < base.bodyChunks[1].pos.y)
		{
			base.bodyChunks[0].vel.x = UnityEngine.Random.Range(-1f, 1f);
			base.bodyChunks[1].vel.x = 0f - base.bodyChunks[0].vel.x;
		}
		if ((base.grasps[0] == null || base.grasps[1] == null) && room.fliesRoomAi != null && dontGrabStuff < 1 && FoodInStomach < MaxFoodInStomach)
		{
			float num11 = ((!(room.game.session is StoryGameSession) || room.world.singleRoomWorld || (room.abstractRoom.swarmRoom && room.world.regionState.SwarmRoomActive(room.abstractRoom.swarmRoomIndex))) ? 1f : 0.5f);
			foreach (Fly fly in room.fliesRoomAi.flies)
			{
				if (fly.PlayerAutoGrabable && Custom.DistLess(base.mainBodyChunk.pos, fly.mainBodyChunk.pos, (room.aimap.getAItile(fly.DangerPos).narrowSpace ? 10f : 20f) * num11) && Grabability(fly) == ObjectGrabability.OneHand && fly.grabbedBy.Count == 0)
				{
					Collide(fly, 0, 0);
					break;
				}
			}
		}
		if (input[0].x == 0 && input[0].y == 0 && !input[0].jmp && !input[0].thrw && !input[0].pckp)
		{
			touchedNoInputCounter++;
		}
		else
		{
			touchedNoInputCounter = 0;
		}
		readyForWin = false;
		UpdateMSC();
		JollyUpdate(eu);
		if ((!ModManager.MSC || (room != null && room.roomSettings.GetEffect(MoreSlugcatsEnums.RoomEffectType.RoomWrap) == null)) && !inVoidSea && !allowOutOfBounds)
		{
			float? num12 = null;
			float? num13 = null;
			for (int num14 = 0; num14 < room.cameraPositions.Length; num14++)
			{
				if (!num12.HasValue || room.cameraPositions[num14].x < num12)
				{
					num12 = room.cameraPositions[num14].x;
				}
				if (!num13.HasValue || room.cameraPositions[num14].x > num13)
				{
					num13 = room.cameraPositions[num14].x;
				}
			}
			if (num12.HasValue && base.mainBodyChunk.pos.x < num12.Value + 20f)
			{
				for (int num15 = 0; num15 < base.bodyChunks.Length; num15++)
				{
					base.bodyChunks[num15].pos.x = num12.Value + 20f;
				}
			}
			if (num13.HasValue && base.mainBodyChunk.pos.x > num13.Value + 1366f)
			{
				for (int num16 = 0; num16 < base.bodyChunks.Length; num16++)
				{
					base.bodyChunks[num16].pos.x = num13.Value + 1366f;
				}
			}
		}
		allowOutOfBounds = false;
		if (room.abstractRoom.shelter && AI == null && room.game.IsStorySession && !base.dead && !Sleeping && room.shelterDoor != null && !room.shelterDoor.Broken)
		{
			bool flag5 = true;
			if (ModManager.MSC && room.game.cameras[0].hud.foodMeter.pupBars != null)
			{
				for (int num17 = 0; num17 < room.game.cameras[0].hud.foodMeter.pupBars.Count; num17++)
				{
					FoodMeter foodMeter = room.game.cameras[0].hud.foodMeter.pupBars[num17];
					if (!foodMeter.PupHasDied && foodMeter.abstractPup.Room == room.abstractRoom && (foodMeter.PupInDanger || foodMeter.CurrentPupFood < foodMeter.survivalLimit))
					{
						flag5 = false;
						break;
					}
				}
			}
			if (flag5 && !stillInStartShelter && FoodInRoom(room, eatAndDestroy: false) >= (base.abstractCreature.world.game.GetStorySession.saveState.malnourished ? slugcatStats.maxFood : slugcatStats.foodToHibernate))
			{
				readyForWin = true;
				forceSleepCounter = 0;
			}
			else if (room.world.rainCycle.timer > room.world.rainCycle.cycleLength && (!ModManager.MSC || SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Saint))
			{
				readyForWin = true;
				forceSleepCounter = 0;
			}
			else if (input[0].y < 0 && !input[0].jmp && !input[0].thrw && !input[0].pckp && IsTileSolid(1, 0, -1) && (input[0].x == 0 || ((!IsTileSolid(1, -1, -1) || !IsTileSolid(1, 1, -1)) && IsTileSolid(1, input[0].x, 0))))
			{
				if (!flag5 && base.abstractCreature.world.game.GetStorySession.saveState.malnourished && FoodInRoom(room, eatAndDestroy: false) >= MaxFoodInStomach)
				{
					forceSleepCounter++;
				}
				else if (!base.abstractCreature.world.game.GetStorySession.saveState.malnourished && FoodInRoom(room, eatAndDestroy: false) > 0 && FoodInRoom(room, eatAndDestroy: false) < slugcatStats.foodToHibernate)
				{
					forceSleepCounter++;
				}
				else
				{
					forceSleepCounter = 0;
				}
			}
			else
			{
				forceSleepCounter = 0;
			}
			if (base.Stunned)
			{
				readyForWin = false;
			}
			if (Custom.ManhattanDistance(base.abstractCreature.pos.Tile, room.shortcuts[0].StartTile) > 6 && (!ModManager.MMF || timeSinceInCorridorMode > 10) && ShelterDoor.IsTileInsideShelterRange(room.abstractRoom, base.abstractCreature.pos.Tile))
			{
				if (readyForWin && touchedNoInputCounter > (ModManager.MMF ? 40 : 20))
				{
					if (ModManager.CoopAvailable)
					{
						ReadyForWinJolly = true;
					}
					room.shelterDoor.Close();
				}
				else if (forceSleepCounter > 260)
				{
					if (ModManager.CoopAvailable)
					{
						ReadyForStarveJolly = true;
					}
					sleepCounter = -24;
					room.shelterDoor.Close();
				}
			}
		}
		base.Update(eu);
		if (ModManager.MSC && room != null)
		{
			if (animation != AnimationIndex.HangFromBeam && animation != AnimationIndex.DeepSwim)
			{
				abstractPhysicalObject.pos.Tile = room.GetTilePosition(base.bodyChunks[1].pos);
			}
			else if (animation == AnimationIndex.BeamTip || animation == AnimationIndex.StandOnBeam)
			{
				abstractPhysicalObject.pos.Tile = room.GetTilePosition(base.bodyChunks[1].pos - new Vector2(0f, 20f));
			}
			else if (animation == AnimationIndex.HangUnderVerticalBeam)
			{
				abstractPhysicalObject.pos.Tile = room.GetTilePosition(base.bodyChunks[0].pos + new Vector2(0f, 20f));
			}
		}
		if (!base.dead && base.graphicsModule != null)
		{
			(base.graphicsModule as PlayerGraphics).head.vel += Custom.RNV() * (Mathf.InverseLerp(0f, 0.0007f, HypothermiaGain) * 2f);
		}
		base.GoThroughFloors = false;
		if (base.stun < 1 && !base.dead && !enteringShortCut.HasValue && !base.inShortcut)
		{
			MovementUpdate(eu);
		}
		if (room != null && room.game.devToolsActive)
		{
			if (Input.GetKey("q") && !FLYEATBUTTON)
			{
				AbstractCreature firstAlivePlayer = room.game.FirstAlivePlayer;
				if (!ModManager.CoopAvailable || (firstAlivePlayer != null && firstAlivePlayer == base.abstractCreature))
				{
					AddFood(1);
				}
			}
			FLYEATBUTTON = Input.GetKey("q");
		}
		bool flag6 = false;
		if (input[0].jmp && !input[1].jmp && !lastWiggleJump)
		{
			wiggle += 0.025f;
			lastWiggleJump = true;
		}
		IntVector2 intVector = wiggleDirectionCounters;
		if (input[0].x != 0 && input[0].x != input[1].x && input[0].x != lastWiggleDir.x)
		{
			flag6 = true;
			if (intVector.y > 0)
			{
				wiggle += 1f / 30f;
				wiggleDirectionCounters.y--;
			}
			lastWiggleDir.x = input[0].x;
			lastWiggleJump = false;
			if (wiggleDirectionCounters.x < 5)
			{
				wiggleDirectionCounters.x++;
			}
		}
		if (input[0].y != 0 && input[0].y != input[1].y && input[0].y != lastWiggleDir.y)
		{
			flag6 = true;
			if (intVector.x > 0)
			{
				wiggle += 1f / 30f;
				wiggleDirectionCounters.x--;
			}
			lastWiggleDir.y = input[0].y;
			lastWiggleJump = false;
			if (wiggleDirectionCounters.y < 5)
			{
				wiggleDirectionCounters.y++;
			}
		}
		if (flag6)
		{
			noWiggleCounter = 0;
		}
		else
		{
			noWiggleCounter++;
		}
		wiggle -= Custom.LerpMap(noWiggleCounter, 5f, 35f, 0f, 1f / 30f);
		if (noWiggleCounter > 20)
		{
			if (wiggleDirectionCounters.x > 0)
			{
				wiggleDirectionCounters.x--;
			}
			if (wiggleDirectionCounters.y > 0)
			{
				wiggleDirectionCounters.y--;
			}
		}
		wiggle = Mathf.Clamp(wiggle, 0f, 1f);
	}

	public override void GraphicsModuleUpdated(bool actuallyViewed, bool eu)
	{
		if ((ModManager.MSC || ModManager.CoopAvailable) && slugOnBack != null)
		{
			slugOnBack.GraphicsModuleUpdated(actuallyViewed, eu);
		}
		if (spearOnBack != null)
		{
			spearOnBack.GraphicsModuleUpdated(actuallyViewed, eu);
		}
		for (int i = 0; i < 2; i++)
		{
			if (base.grasps[i] == null)
			{
				continue;
			}
			if (base.grasps[i].grabbed is Player && !(base.grasps[i].grabbed as Creature).dead)
			{
				Vector2 vector = Custom.DirVec(base.mainBodyChunk.pos, base.grasps[i].grabbedChunk.pos);
				float num = Vector2.Distance(base.mainBodyChunk.pos, base.grasps[i].grabbedChunk.pos);
				float num2 = 15f;
				float num3 = base.grasps[i].grabbedChunk.mass / (base.mainBodyChunk.mass + base.grasps[i].grabbedChunk.mass);
				if (enteringShortCut.HasValue)
				{
					num3 = 0f;
				}
				if (num > num2)
				{
					base.mainBodyChunk.pos += vector * (num - num2) * num3 * 0.5f;
					base.mainBodyChunk.vel += vector * (num - num2) * num3 * 0.5f;
					base.grasps[i].grabbedChunk.pos -= vector * (num - num2) * (1f - num3);
					base.grasps[i].grabbedChunk.vel -= vector * (num - num2) * (1f - num3);
				}
				if (bodyMode == BodyModeIndex.ClimbingOnBeam && animation != AnimationIndex.BeamTip && animation != AnimationIndex.StandOnBeam && (base.grasps[i].grabbed as Player).bodyMode != BodyModeIndex.ClimbingOnBeam)
				{
					BodyChunk grabbedChunk = base.grasps[i].grabbedChunk;
					grabbedChunk.vel.y = grabbedChunk.vel.y + base.grasps[i].grabbed.gravity * (1f - base.grasps[i].grabbedChunk.submersion) * 0.75f;
				}
				if (Grabability(base.grasps[i].grabbed) == ObjectGrabability.Drag && num > num2 * 2f + 30f)
				{
					ReleaseGrasp(i);
				}
			}
			else if (HeavyCarry(base.grasps[i].grabbed))
			{
				Vector2 vector2 = Custom.DirVec(base.mainBodyChunk.pos, base.grasps[i].grabbedChunk.pos);
				float num4 = Vector2.Distance(base.mainBodyChunk.pos, base.grasps[i].grabbedChunk.pos);
				float num5 = 5f + base.grasps[i].grabbedChunk.rad;
				if (base.grasps[i].grabbed is Cicada)
				{
					num5 = 30f;
				}
				num5 *= Mathf.InverseLerp(25f, 15f, eatMeat);
				float num6 = base.grasps[i].grabbedChunk.mass / (base.mainBodyChunk.mass + base.grasps[i].grabbedChunk.mass);
				if (enteringShortCut.HasValue)
				{
					num6 = 0f;
				}
				else if (base.grasps[i].grabbed.TotalMass < base.TotalMass)
				{
					num6 /= 2f;
				}
				if (!enteringShortCut.HasValue || num4 > num5)
				{
					base.mainBodyChunk.pos += vector2 * (num4 - num5) * num6;
					base.mainBodyChunk.vel += vector2 * (num4 - num5) * num6;
					base.grasps[i].grabbedChunk.pos -= vector2 * (num4 - num5) * (1f - num6);
					base.grasps[i].grabbedChunk.vel -= vector2 * (num4 - num5) * (1f - num6);
				}
				if (bodyMode == BodyModeIndex.ClimbingOnBeam && animation != AnimationIndex.BeamTip && animation != AnimationIndex.StandOnBeam)
				{
					base.grasps[i].grabbedChunk.vel.y += base.grasps[i].grabbed.gravity * (1f - base.grasps[i].grabbedChunk.submersion) * 0.75f;
				}
				if (Grabability(base.grasps[i].grabbed) == ObjectGrabability.Drag && num4 > num5 * 2f + 30f)
				{
					ReleaseGrasp(i);
				}
			}
			else if (actuallyViewed)
			{
				base.grasps[i].grabbed.firstChunk.vel = (base.graphicsModule as PlayerGraphics).hands[i].vel;
				base.grasps[i].grabbed.firstChunk.MoveFromOutsideMyUpdate(eu, (base.graphicsModule as PlayerGraphics).hands[i].pos);
				if (!(base.grasps[i].grabbed is Weapon))
				{
					continue;
				}
				Vector2 vector3 = Custom.DirVec(base.mainBodyChunk.pos, base.grasps[i].grabbed.bodyChunks[0].pos) * ((i == 0) ? (-1f) : 1f);
				if (animation != AnimationIndex.HangFromBeam)
				{
					vector3 = Custom.PerpendicularVector(vector3);
				}
				if (bodyMode == BodyModeIndex.Crawl)
				{
					vector3 = Custom.DirVec(base.bodyChunks[1].pos, Vector2.Lerp(base.grasps[i].grabbed.bodyChunks[0].pos, base.bodyChunks[0].pos, 0.8f));
				}
				else if (animation == AnimationIndex.ClimbOnBeam)
				{
					vector3.y = Mathf.Abs(vector3.y);
					vector3 = Vector3.Slerp(vector3, Custom.DirVec(base.bodyChunks[1].pos, base.bodyChunks[0].pos), 0.75f);
				}
				else if (base.grasps[i].grabbed is Spear)
				{
					if (ModManager.CoopAvailable && jollyButtonDown && handPointing == i)
					{
						vector3 = PointDir();
					}
					vector3 = Vector3.Slerp(vector3, Custom.DegToVec((80f + Mathf.Cos((float)(animationFrame + (leftFoot ? 9 : 3)) / 12f * 2f * (float)Math.PI) * 4f * (base.graphicsModule as PlayerGraphics).spearDir) * (base.graphicsModule as PlayerGraphics).spearDir), Mathf.Abs((base.graphicsModule as PlayerGraphics).spearDir));
				}
				(base.grasps[i].grabbed as Weapon).setRotation = vector3;
				(base.grasps[i].grabbed as Weapon).rotationSpeed = 0f;
			}
			else
			{
				base.grasps[i].grabbed.firstChunk.pos = base.bodyChunks[0].pos;
				base.grasps[i].grabbed.firstChunk.vel = base.mainBodyChunk.vel;
			}
		}
	}

	public void checkInput()
	{
		for (int num = input.Length - 1; num > 0; num--)
		{
			input[num] = input[num - 1];
		}
		int num2 = playerState.playerNumber;
		if (ModManager.MSC && base.abstractCreature.world.game.IsArenaSession && base.abstractCreature.world.game.GetArenaGameSession.chMeta != null)
		{
			num2 = 0;
		}
		if (base.stun == 0 && !base.dead)
		{
			if (controller != null)
			{
				input[0] = controller.GetInput();
			}
			else if (AI != null)
			{
				AI.Update();
			}
			else
			{
				input[0] = RWInput.PlayerInput(num2);
			}
		}
		else
		{
			input[0] = new InputPackage(room.game.rainWorld.options.controls[num2].gamePad, room.game.rainWorld.options.controls[num2].GetActivePreset(), 0, 0, jmp: false, thrw: false, pckp: false, mp: false, crouchToggle: false);
		}
		if (AI == null)
		{
			ProcessConsoleDebugInputs();
		}
		mapInput = input[0];
		if ((standStillOnMapButton && input[0].mp && (!ModManager.CoopAvailable || !jollyButtonDown)) || Sleeping)
		{
			input[0].x = 0;
			input[0].y = 0;
			input[0].analogueDir *= 0f;
			input[0].jmp = false;
			input[0].thrw = false;
			input[0].pckp = false;
			Blink(5);
		}
		if (superLaunchJump > 10 && input[0].jmp && input[1].jmp && input[2].jmp && input[0].y < 1)
		{
			input[0].x = 0;
		}
		if (animation == AnimationIndex.Roll && input[0].x == 0 && input[0].downDiagonal != 0)
		{
			input[0].x = input[0].downDiagonal;
		}
		if (ModManager.CoopAvailable && jollyButtonDown)
		{
			pointInput = input[0];
			input[0].x = 0;
			input[0].y = 0;
			InputPackage[] array = input;
			int num3 = 0;
			input[num3].analogueDir = array[num3].analogueDir * 0f;
		}
	}

	public void Blink(int blink)
	{
		if (base.graphicsModule != null && (base.graphicsModule as PlayerGraphics).blink < blink)
		{
			(base.graphicsModule as PlayerGraphics).blink = blink;
		}
	}

	public static float PyroDeathThreshold(RainWorldGame game)
	{
		if (ModManager.CoopAvailable && ModManager.MSC && game.IsStorySession && game.StoryCharacter != MoreSlugcatsEnums.SlugcatStatsName.Artificer)
		{
			return 0f;
		}
		return 0.65f;
	}

	private void LungUpdate()
	{
		airInLungs = Mathf.Min(airInLungs, 1f - rainDeath);
		if (base.firstChunk.submersion > 0.9f && !room.game.setupValues.invincibility && !chatlog)
		{
			if (!submerged)
			{
				swimForce = Mathf.InverseLerp(0f, 8f, Mathf.Abs(base.firstChunk.vel.x));
				swimCycle = 0f;
			}
			float num = airInLungs;
			if (!ModManager.MSC || !monkAscension)
			{
				airInLungs -= 1f / (40f * (lungsExhausted ? 4.5f : 9f) * ((input[0].y == 1 && input[0].x == 0 && airInLungs < 1f / 3f) ? 1.5f : 1f) * ((float)room.game.setupValues.lungs / 100f)) * slugcatStats.lungsFac;
			}
			if (ModManager.MSC && airInLungs <= PyroDeathThreshold(room.game) && SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
			{
				Vector2 pos = base.firstChunk.pos;
				room.AddObject(new Explosion(room, this, pos, 5, 110f, 5f, 1.1f, 60f, 0.3f, this, 0.8f, 0f, 1f));
				for (int i = 0; i < 14; i++)
				{
					room.AddObject(new Explosion.ExplosionSmoke(pos, Custom.RNV() * 5f * UnityEngine.Random.value, 1f));
				}
				room.AddObject(new Explosion.ExplosionLight(pos, 160f, 1f, 3, Color.white));
				room.AddObject(new ShockWave(pos, 60f, 0.045f, 4));
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
				Die();
			}
			if (airInLungs < 2f / 3f && num >= 2f / 3f)
			{
				room.AddObject(new Bubble(base.firstChunk.pos, base.firstChunk.vel, bottomBubble: false, fakeWaterBubble: false));
			}
			bool flag = ((!ModManager.MSC) ? (room.FloatWaterLevel(base.mainBodyChunk.pos.x) - base.mainBodyChunk.pos.y < 200f) : (room.WaterLevelDisplacement(base.mainBodyChunk.pos) < 200f));
			bool flag2 = airInLungs <= 0f && input[0].y == 1 && flag;
			if (flag2)
			{
				for (int l = room.GetTilePosition(base.mainBodyChunk.pos).y; l <= room.defaultWaterLevel; l++)
				{
					if (room.GetTile(room.GetTilePosition(base.mainBodyChunk.pos).x, l).Solid)
					{
						flag2 = false;
						break;
					}
				}
			}
			if (isRivulet)
			{
				flag2 = false;
			}
			if (airInLungs <= (flag2 ? (-0.3f) : 0f) && base.mainBodyChunk.submersion == 1f && base.bodyChunks[1].submersion > 0.5f)
			{
				airInLungs = 0f;
				Stun(10);
				drown += 1f / 120f;
				if (drown >= 1f)
				{
					Die();
				}
			}
			else if (airInLungs < 1f / 3f)
			{
				if (slowMovementStun < 1)
				{
					slowMovementStun = 1;
				}
				if (ModManager.MSC && room.waterInverted)
				{
					if (UnityEngine.Random.value < 0.5f)
					{
						base.firstChunk.vel -= Custom.DegToVec(UnityEngine.Random.value * 360f) * UnityEngine.Random.value;
					}
					if (input[0].y > 1)
					{
						base.bodyChunks[1].vel *= Mathf.Lerp(1f, 0.9f, Mathf.InverseLerp(0f, 1f / 3f, airInLungs));
					}
				}
				else
				{
					if (UnityEngine.Random.value < 0.5f)
					{
						base.firstChunk.vel += Custom.DegToVec(UnityEngine.Random.value * 360f) * UnityEngine.Random.value;
					}
					if (input[0].y < 1)
					{
						base.bodyChunks[1].vel *= Mathf.Lerp(1f, 0.9f, Mathf.InverseLerp(0f, 1f / 3f, airInLungs));
					}
				}
				if ((UnityEngine.Random.value > airInLungs * 2f || lungsExhausted) && UnityEngine.Random.value > 0.5f)
				{
					room.AddObject(new Bubble(base.firstChunk.pos, base.firstChunk.vel + Custom.DegToVec(UnityEngine.Random.value * 360f) * Mathf.Lerp(6f, 0f, airInLungs), bottomBubble: false, fakeWaterBubble: false));
				}
			}
			submerged = true;
		}
		else
		{
			if (submerged && airInLungs < 1f / 3f)
			{
				lungsExhausted = true;
			}
			if (!lungsExhausted && airInLungs > 0.9f)
			{
				airInLungs = 1f;
			}
			if (airInLungs <= 0f)
			{
				airInLungs = 0f;
			}
			airInLungs += 1f / (float)(lungsExhausted ? 240 : 60);
			if (airInLungs >= 1f)
			{
				airInLungs = 1f;
				lungsExhausted = false;
				drown = 0f;
			}
			submerged = false;
			if (lungsExhausted && animation != AnimationIndex.SurfaceSwim)
			{
				swimCycle += 0.1f;
			}
		}
		if (lungsExhausted)
		{
			if (slowMovementStun < 5)
			{
				slowMovementStun = 5;
			}
			if (drown > 0f && slowMovementStun < 10)
			{
				slowMovementStun = 10;
			}
		}
	}

	private void DepleteSwarmRoom()
	{
		if (!(room.game.session is StoryGameSession) || room.world.regionState == null)
		{
			return;
		}
		int num = -1;
		if (room.abstractRoom.swarmRoom && room.world.regionState.SwarmRoomActive(room.abstractRoom.swarmRoomIndex))
		{
			num = room.abstractRoom.swarmRoomIndex;
		}
		else
		{
			float num2 = float.MaxValue;
			for (int i = 0; i < room.game.world.swarmRooms.Length; i++)
			{
				if (!room.game.world.regionState.SwarmRoomActive(i))
				{
					continue;
				}
				for (int j = 0; j < room.abstractRoom.NodesRelevantToCreature(StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Fly)); j++)
				{
					float swarmRoomDistance = room.game.world.fliesWorldAI.GetSwarmRoomDistance(new WorldCoordinate(room.abstractRoom.index, -1, -1, room.abstractRoom.CreatureSpecificToCommonNodeIndex(j, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Fly))), i);
					if (swarmRoomDistance >= 0f && swarmRoomDistance < num2)
					{
						num = i;
						num2 = swarmRoomDistance;
					}
				}
			}
		}
		if (num > -1)
		{
			room.game.world.regionState.candidatesForDepleteSwarmRooms.Add(room.world.GetAbstractRoom(room.world.swarmRooms[num]).name);
		}
	}

	public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
	{
		if (ModManager.MSC && AI != null && otherObject is Player && base.Consious && (otherObject as Player).Consious)
		{
			bool flag = base.mainBodyChunk.vel.x != 0f;
			bool flag2 = (otherObject as Player).mainBodyChunk.vel.x != 0f;
			bool flag3 = bodyMode == BodyModeIndex.ClimbingOnBeam;
			if (flag || flag2)
			{
				for (int i = 0; i < base.bodyChunks.Length; i++)
				{
					if ((otherObject as Player).abstractCreature.ID.number > base.abstractCreature.ID.number || (flag2 && !flag))
					{
						if (base.bodyChunks[i].vel.y > -3f)
						{
							BodyChunk bodyChunk = base.bodyChunks[i];
							bodyChunk.vel.y = bodyChunk.vel.y - 0.5f * (flag3 ? 0.5f : 1f);
						}
					}
					else if (base.bodyChunks[i].vel.y < 5f)
					{
						BodyChunk bodyChunk2 = base.bodyChunks[i];
						bodyChunk2.vel.y = bodyChunk2.vel.y + 2.5f * (flag3 ? 0.5f : 1f);
					}
				}
			}
		}
		if (otherObject is Creature)
		{
			HypothermiaBodyContactWarmup(this, otherObject as Creature);
			if (!isGourmand && animation == AnimationIndex.BellySlide)
			{
				(otherObject as Creature).Stun(longBellySlide ? 4 : 2);
				if (!longBellySlide && rollCounter > 11)
				{
					rollCounter = 11;
				}
				base.mainBodyChunk.vel.x += (float)rollDirection * 3f;
			}
			else if (ModManager.MSC)
			{
				if (SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Gourmand && animation == AnimationIndex.Roll && gourmandAttackNegateTime <= 0)
				{
					bool flag4 = otherObject is Player && !Custom.rainWorld.options.friendlyFire;
					if (!(otherObject as Creature).dead && (otherObject as Creature).abstractCreature.creatureTemplate.type != MoreSlugcatsEnums.CreatureTemplateType.SlugNPC && !(ModManager.CoopAvailable && flag4))
					{
						Custom.Log("SLUGROLLED! stun: 120 damage: 1");
						room.ScreenMovement(base.bodyChunks[0].pos, base.mainBodyChunk.vel * base.bodyChunks[0].mass * 5f * 0.1f, Mathf.Max((base.bodyChunks[0].mass - 30f) / 50f, 0f));
						room.PlaySound(SoundID.Slugcat_Terrain_Impact_Hard, base.mainBodyChunk);
						(otherObject as Creature).SetKillTag(base.abstractCreature);
						(otherObject as Creature).Violence(base.mainBodyChunk, new Vector2(base.mainBodyChunk.vel.x * 5f, base.mainBodyChunk.vel.y), otherObject.firstChunk, null, DamageType.Blunt, 1f, 120f);
						animation = AnimationIndex.None;
						base.mainBodyChunk.vel.Scale(new Vector2(-0.5f, -0.5f));
						rollDirection = 0;
						if (((otherObject as Creature).State is HealthState && ((otherObject as Creature).State as HealthState).ClampedHealth == 0f) || (otherObject as Creature).State.dead)
						{
							room.PlaySound(SoundID.Spear_Stick_In_Creature, base.mainBodyChunk, loop: false, 1.7f, 1f);
						}
						else
						{
							room.PlaySound(SoundID.Big_Needle_Worm_Impale_Terrain, base.mainBodyChunk, loop: false, 1.2f, 1f);
						}
						gourmandAttackNegateTime = 20;
					}
				}
				else if (SlugSlamConditions(otherObject))
				{
					float num = 4f;
					int num2 = 120;
					if (animation == AnimationIndex.BellySlide || animation == AnimationIndex.RocketJump)
					{
						num = 0.25f;
						num2 = 50;
					}
					else
					{
						float num3 = lastGroundY - base.firstChunk.pos.y;
						num2 = (int)((float)num2 * Mathf.Floor(Mathf.Abs(base.mainBodyChunk.vel.magnitude) / 7f));
						num = ((num3 < 100f) ? (num / 2f) : ((num3 < 200f) ? (num * 1f) : ((num3 < 320f) ? (num * 2f) : ((!(num3 < 600f)) ? (num * 5f) : (num * 3f)))));
					}
					if (num2 > 240)
					{
						num2 = 240;
					}
					if (num < 0f)
					{
						num = 0f;
					}
					if (num2 < 25)
					{
						num2 = 0;
					}
					if (num != 0f || num2 != 0)
					{
						bool flag5 = (otherObject as Creature).abstractCreature.creatureTemplate.smallCreature;
						if (!(otherObject as Creature).dead)
						{
							Custom.Log("SLUGSMASH! slide:", (animation == AnimationIndex.BellySlide || animation == AnimationIndex.RocketJump).ToString(), "incoming speed:", Mathf.Max(base.mainBodyChunk.vel.y, base.mainBodyChunk.vel.magnitude).ToString(), "dist:", (lastGroundY - base.firstChunk.pos.y).ToString(), "stun:", num2.ToString(), "damage:", num.ToString());
							room.ScreenMovement(base.bodyChunks[0].pos, base.mainBodyChunk.vel * num * base.bodyChunks[0].mass * 0.1f, Mathf.Max((num * base.bodyChunks[0].mass - 30f) / 50f, 0f));
							room.PlaySound(SoundID.Slugcat_Terrain_Impact_Hard, base.mainBodyChunk);
							(otherObject as Creature).SetKillTag(base.abstractCreature);
							(otherObject as Creature).Violence(base.mainBodyChunk, new Vector2(base.mainBodyChunk.vel.x, base.mainBodyChunk.vel.y), otherObject.firstChunk, null, DamageType.Blunt, num, num2);
							if (otherObject is BigJellyFish)
							{
								flag5 = true;
								(otherObject as Creature).Die();
							}
							if (((otherObject as Creature).State is HealthState && ((otherObject as Creature).State as HealthState).ClampedHealth == 0f) || (otherObject as Creature).State.dead)
							{
								room.PlaySound(SoundID.Spear_Stick_In_Creature, base.mainBodyChunk, loop: false, 1.7f, 1f);
							}
							else
							{
								room.PlaySound(SoundID.Big_Needle_Worm_Impale_Terrain, base.mainBodyChunk, loop: false, 1.2f, 1f);
							}
						}
						else
						{
							room.PlaySound(SoundID.Slugcat_Terrain_Impact_Hard, base.mainBodyChunk);
						}
						if (base.mainBodyChunk.vel.magnitude < 40f && !flag5)
						{
							base.mainBodyChunk.vel.Scale(new Vector2(-0.5f, -0.5f));
							base.bodyChunks[0].vel = base.mainBodyChunk.vel;
							base.bodyChunks[1].vel = base.mainBodyChunk.vel;
						}
						else
						{
							base.mainBodyChunk.vel.Scale(new Vector2(0.9f, 0.9f));
						}
					}
					if (animation == AnimationIndex.BellySlide)
					{
						base.mainBodyChunk.vel.x /= 3f;
						rollCounter = 99;
					}
				}
			}
			if (Sleeping)
			{
				sleepCounter = 0;
			}
		}
		if (base.Consious)
		{
			if (FoodInStomach < MaxFoodInStomach && AllowGrabbingBatflys() && otherObject is Fly && Grabability(otherObject) == ObjectGrabability.OneHand && (!ModManager.MSC || ((grabbedBy.Count == 0 || !(grabbedBy[0].grabber is Player)) && onBack == null)))
			{
				bool flag6 = false;
				for (int j = 0; j < 2; j++)
				{
					if (base.grasps[j] != null && (Grabability(base.grasps[j].grabbed) == ObjectGrabability.TwoHands || isSlugpup))
					{
						flag6 = true;
						break;
					}
				}
				if (!ModManager.MMF || !flag6)
				{
					for (int k = 0; k < 2; k++)
					{
						if ((base.grasps[k] != null && (Grabability(base.grasps[k].grabbed) == ObjectGrabability.TwoHands || isSlugpup)) || base.grasps[k] != null)
						{
							continue;
						}
						room.PlaySound((otherObject as Fly).dead ? SoundID.Fly_Caught_Dead : SoundID.Fly_Caught, otherObject.firstChunk);
						if (!(otherObject as Fly).everBeenCaughtByPlayer)
						{
							(otherObject as Fly).everBeenCaughtByPlayer = true;
							if (room.game.session is StoryGameSession)
							{
								DepleteSwarmRoom();
							}
						}
						SlugcatGrab(otherObject, k);
						break;
					}
				}
			}
			else if (wantToPickUp > 0 && CanIPickThisUp(otherObject))
			{
				if (Grabability(otherObject) == ObjectGrabability.TwoHands)
				{
					SlugcatGrab(otherObject, 0);
				}
				else if (FreeHand() >= 0)
				{
					SlugcatGrab(otherObject, FreeHand());
				}
				wantToPickUp = 0;
			}
		}
		if (jumpChunkCounter >= 0 && bodyMode == BodyModeIndex.Default && myChunk == 1 && base.bodyChunks[1].pos.y > otherObject.bodyChunks[otherChunk].pos.y - otherObject.bodyChunks[otherChunk].rad / 2f)
		{
			jumpChunkCounter = 5;
			jumpChunk = otherObject.bodyChunks[otherChunk];
		}
	}

	public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
	{
		if (speed > 12f)
		{
			Blink(Custom.IntClamp((int)speed, 12, 60) / 2);
		}
		if (input[0].downDiagonal != 0 && animation != AnimationIndex.Roll && (speed > 12f || animation == AnimationIndex.Flip || (animation == AnimationIndex.RocketJump && rocketJumpFromBellySlide)) && direction.y < 0 && allowRoll > 0 && consistentDownDiagonal > ((speed > 24f) ? 1 : 6))
		{
			if (animation == AnimationIndex.RocketJump && rocketJumpFromBellySlide)
			{
				base.bodyChunks[1].vel.y += 3f;
				base.bodyChunks[1].pos.y += 3f;
				base.bodyChunks[0].vel.y -= 3f;
				base.bodyChunks[0].pos.y -= 3f;
			}
			room.PlaySound(SoundID.Slugcat_Roll_Init, base.mainBodyChunk.pos, 1f, 1f);
			animation = AnimationIndex.Roll;
			rollDirection = input[0].downDiagonal;
			rollCounter = 0;
			base.bodyChunks[0].vel.x = Mathf.Lerp(base.bodyChunks[0].vel.x, 9f * (float)input[0].x, 0.7f);
			base.bodyChunks[1].vel.x = Mathf.Lerp(base.bodyChunks[1].vel.x, 9f * (float)input[0].x, 0.7f);
			standing = false;
		}
		else if (firstContact)
		{
			float num = (isGourmand ? 80f : 60f);
			float num2 = (isGourmand ? 40f : 35f);
			float num3 = (isGourmand ? 1f : 3f);
			float num4 = (isGourmand ? 8f : 16f);
			IntVector2 tilePosition = room.GetTilePosition(base.bodyChunks[chunk].pos);
			bool flag = false;
			if (ModManager.MMF && base.Consious && grabbedBy.Count == 0 && room.GetTile(tilePosition).Terrain == Room.Tile.TerrainType.ShortcutEntrance && shortcutDelay < 1 && !enteringShortCut.HasValue && input[0].AnyDirectionalInput)
			{
				ShortcutData shortcutData = room.shortcutData(tilePosition);
				flag = shortcutData.shortCutType == ShortcutData.Type.RoomExit || shortcutData.shortCutType == ShortcutData.Type.Normal;
			}
			if (playerInAntlers != null)
			{
				Custom.Log("Falling damaged cancelled due to DEER");
				room.PlaySound(SoundID.Slugcat_Terrain_Impact_Light, base.mainBodyChunk, loop: false, Mathf.InverseLerp(0f, 2f, speed), 3f);
			}
			else if (speed > 5f && flag)
			{
				Custom.Log("player highspeed launch or fall into door");
				if (tongue != null && tongue.Attached)
				{
					tongue.Release();
				}
				enteringShortCut = tilePosition;
			}
			else if (speed > num && immuneToFallDamage <= 0 && direction.y < 0 && (!ModManager.MSC || ((tongue == null || !tongue.Attached) && (grabbedBy.Count <= 0 || !(grabbedBy[0].grabber is Player)))))
			{
				room.PlaySound(SoundID.Slugcat_Terrain_Impact_Death, base.mainBodyChunk);
				Custom.Log("Fall damage death");
				Die();
			}
			else if (speed > num2 && immuneToFallDamage <= 0 && (!ModManager.MSC || tongue == null || !tongue.Attached))
			{
				room.PlaySound(SoundID.Slugcat_Terrain_Impact_Hard, base.mainBodyChunk);
				Stun((int)Custom.LerpMap(speed, num2, num, 40f, 140f, 2.5f));
				if (ModManager.MSC && SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel && sofCooldown < 0)
				{
					room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Inv_Hit, 0f, 1f, 0.8f + UnityEngine.Random.value * 1f);
					sofCooldown = 5;
				}
			}
			else if (direction.y < 0 && base.Consious)
			{
				room.PlaySound((standing && chunk == 1) ? SoundID.Slugcat_Floor_Impact_Standard : SoundID.Slugcat_Floor_Impact_Stealthy, base.mainBodyChunk, loop: false, Mathf.InverseLerp(8f, 11f, speed), 1f);
			}
			else if (speed < num3)
			{
				room.PlaySound(SoundID.Slugcat_Terrain_Impact_Light, base.mainBodyChunk, loop: false, Mathf.InverseLerp(0f, 2f, speed), 3f);
			}
			else if (speed < num4)
			{
				room.PlaySound(SoundID.Slugcat_Terrain_Impact_Medium, base.mainBodyChunk);
			}
			else
			{
				room.PlaySound(SoundID.Slugcat_Terrain_Impact_Hard, base.mainBodyChunk);
				if (ModManager.MSC && SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel && sofCooldown < 0)
				{
					room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Inv_Hit, 0f, 1f, 0.8f + UnityEngine.Random.value * 1f);
					sofCooldown = 5;
				}
			}
		}
		if (ModManager.MMF && MMF.cfgWallpounce.Value && bodyMode != BodyModeIndex.CorridorClimb && wantToJump > 0 && direction.x != 0 && chunk == 0 && speed > 7f && input[0].x == direction.x && !standing)
		{
			room.PlaySound(SoundID.Slugcat_Sectret_Super_Wall_Jump, base.mainBodyChunk.pos, 1f, 1f);
			base.bodyChunks[1].pos = base.bodyChunks[0].pos;
			base.bodyChunks[0].pos += new Vector2((float)direction.x * -10f, 10f);
			if (ModManager.MSC && SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
			{
				base.bodyChunks[0].vel = new Vector2((float)direction.x * -25f, 10f);
				base.bodyChunks[1].vel = new Vector2((float)direction.x * -25f, 10f);
				BodyChunk bodyChunk = base.bodyChunks[0];
				bodyChunk.vel.y = bodyChunk.vel.y + 11f;
				BodyChunk bodyChunk2 = base.bodyChunks[1];
				bodyChunk2.vel.y = bodyChunk2.vel.y + 10f;
				jumpStun = 15 * -direction.x;
			}
			else
			{
				base.bodyChunks[0].vel = new Vector2((float)direction.x * -17f, 10f);
				base.bodyChunks[1].vel = new Vector2((float)direction.x * -17f, 10f);
				jumpStun = 20 * -direction.x;
			}
			animation = AnimationIndex.RocketJump;
			room.ScreenMovement(base.mainBodyChunk.pos, new Vector2(direction.x, 0f), 0.1f);
			for (int i = 0; i < 7; i++)
			{
				room.AddObject(new WaterDrip(base.mainBodyChunk.pos + new Vector2(base.mainBodyChunk.rad * (float)direction.x, 0f), Custom.DegToVec(UnityEngine.Random.value * 180f * (0f - (float)direction.x)) * Mathf.Lerp(10f, 17f, UnityEngine.Random.value), waterColor: false));
			}
			if (tongue != null && tongue.Attached)
			{
				tongue.Release();
			}
		}
		base.TerrainImpact(chunk, direction, speed, firstContact);
	}

	public override void Die()
	{
		if ((ModManager.MSC || ModManager.CoopAvailable) && slugOnBack != null && slugOnBack.slugcat != null)
		{
			slugOnBack.DropSlug();
		}
		if (spearOnBack != null && spearOnBack.spear != null)
		{
			spearOnBack.DropSpear();
		}
		Room realizedRoom = room;
		if (realizedRoom == null)
		{
			realizedRoom = base.abstractCreature.world.GetAbstractRoom(base.abstractCreature.pos).realizedRoom;
		}
		bool flag = !base.dead;
		if (AI == null)
		{
			if (realizedRoom != null)
			{
				if (realizedRoom.game.setupValues.invincibility)
				{
					return;
				}
				if (!base.dead)
				{
					if (ModManager.CoopAvailable)
					{
						base.Die();
					}
					realizedRoom.game.GameOver(null);
					if (ModManager.MSC && SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel)
					{
						realizedRoom.PlaySound(MoreSlugcatsEnums.MSCSoundID.Inv_GO, 0f, 1.5f, 1f + UnityEngine.Random.value * 0.25f);
					}
					else
					{
						realizedRoom.PlaySound(SoundID.UI_Slugcat_Die, base.mainBodyChunk);
					}
				}
				if (PlaceKarmaFlower && realizedRoom.game.session is StoryGameSession && (!ModManager.MSC || !realizedRoom.game.wasAnArtificerDream) && !ModManager.CoopAvailable)
				{
					(realizedRoom.game.session as StoryGameSession).PlaceKarmaFlowerOnDeathSpot();
				}
			}
			else if (!base.dead && !base.abstractCreature.world.game.setupValues.invincibility)
			{
				if (ModManager.CoopAvailable)
				{
					base.Die();
				}
				base.abstractCreature.world.game.GameOver(null);
			}
			PlayerHandler playerHandler = realizedRoom.game.rainWorld.GetPlayerHandler(playerState.playerNumber);
			if (playerHandler != null)
			{
				playerHandler.ControllerHandler.OnPlayerDeath();
			}
		}
		if (ModManager.CoopAvailable)
		{
			SaveStomachObjectInPlayerState();
			JollyCustom.Log(string.Format("Player {0} Die! Reason: {1}", base.abstractCreature.ID.number, (killTag != null) ? killTag.ToString() : "other"));
		}
		base.Die();
		if (isSlugpup && room != null && room.game.rainWorld.progression.miscProgressionData.currentlySelectedSinglePlayerSlugcat == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel && flag)
		{
			AbstractPhysicalObject abstractPhysicalObject = new AbstractPhysicalObject(base.abstractCreature.Room.world, MoreSlugcatsEnums.AbstractObjectType.SingularityBomb, null, room.GetWorldCoordinate(base.mainBodyChunk.pos), base.abstractCreature.Room.world.game.GetNewID());
			base.abstractCreature.Room.AddEntity(abstractPhysicalObject);
			abstractPhysicalObject.RealizeInRoom();
			(abstractPhysicalObject.realizedObject as SingularityBomb).Explode();
		}
	}

	public override void Grabbed(Grasp grasp)
	{
		base.Grabbed(grasp);
		if (grasp.grabber is Lizard || grasp.grabber is Vulture || grasp.grabber is BigSpider || grasp.grabber is DropBug)
		{
			dangerGraspTime = 0;
			dangerGrasp = grasp;
		}
	}

	public override void Stun(int st)
	{
		if (room != null)
		{
			if (Malnourished)
			{
				st = Mathf.RoundToInt((float)st * (exhausted ? 2f : 1.5f));
			}
			if (st > UnityEngine.Random.Range(5, 12))
			{
				if (dangerGrasp == null && (!ModManager.MMF || !MMF.cfgJetfishItemProtection.Value || stunDamageType != DamageType.Blunt))
				{
					LoseAllGrasps();
				}
				standing = false;
				feetStuckPos = null;
			}
			if (st > UnityEngine.Random.Range(40, 80) && spearOnBack != null && spearOnBack.spear != null && (!ModManager.MMF || !MMF.cfgJetfishItemProtection.Value || stunDamageType != DamageType.Blunt))
			{
				spearOnBack.DropSpear();
			}
			if ((ModManager.MSC || ModManager.CoopAvailable) && st > UnityEngine.Random.Range(40, 80) && slugOnBack != null && slugOnBack.slugcat != null && stunDamageType != DamageType.Blunt)
			{
				slugOnBack.DropSlug();
			}
			if (st > base.stun && st > 10)
			{
				room.PlaySound(SoundID.UI_Slugcat_Stunned_Init, base.mainBodyChunk);
				lastStun = st;
			}
		}
		if (ModManager.MSC && Wounded)
		{
			aerobicLevel = 1f;
		}
		base.Stun(st);
	}

	public void AddQuarterFood()
	{
		if (redsIllness != null)
		{
			redsIllness.AddQuarterFood();
		}
		else if (FoodInStomach < MaxFoodInStomach)
		{
			playerState.quarterFoodPoints++;
			if (ModManager.CoopAvailable && base.abstractCreature.world.game.IsStorySession && base.abstractCreature.world.game.Players[0] != base.abstractCreature && !isNPC)
			{
				PlayerState obj = base.abstractCreature.world.game.Players[0].state as PlayerState;
				JollyCustom.Log($"Player add quarter food. Amount to add {playerState.playerNumber}");
				obj.quarterFoodPoints++;
			}
			if (playerState.quarterFoodPoints > 3)
			{
				playerState.quarterFoodPoints -= 4;
				AddFood(1);
			}
		}
	}

	private void SleepUpdate()
	{
		if (sleepCounter == 0)
		{
			if ((float)forceSleepCounter > 0f)
			{
				sleepCurlUp = Custom.LerpAndTick(sleepCurlUp, Mathf.InverseLerp(10f, 210f, forceSleepCounter), 0.04f, 0.05f);
			}
			else
			{
				sleepCurlUp = Mathf.Max(0f, sleepCurlUp - 0.1f);
				if (sleepWhenStill && touchedNoInputCounter > 2 && base.bodyChunks[1].ContactPoint.y < 0)
				{
					sleepCounter = -1;
				}
			}
		}
		else if (sleepCounter > 0)
		{
			if (sleepCounter == 100)
			{
				standing = false;
				Vector2 vector = room.MiddleOfTile(base.abstractCreature.pos);
				float num = ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f);
				base.bodyChunks[0].HardSetPosition(vector + new Vector2(bodyChunkConnections[0].distance * -0.5f * num, -10f + base.bodyChunks[0].rad));
				base.bodyChunks[1].HardSetPosition(vector + new Vector2(bodyChunkConnections[0].distance * 0.5f * num, -10f + base.bodyChunks[1].rad));
				sleepCounter = 99;
			}
			else if (sleepCounter == 99)
			{
				InputPackage inputPackage = RWInput.PlayerInput(playerState.playerNumber);
				if (!inputPackage.mp && (inputPackage.x != 0 || inputPackage.y != 0 || inputPackage.jmp || inputPackage.pckp || inputPackage.thrw))
				{
					sleepCounter = (Malnourished ? 40 : 17);
				}
				if (!IsTileSolid(0, 0, -1) || !IsTileSolid(1, 0, -1) || base.bodyChunks[0].ContactPoint.y > -1 || base.bodyChunks[1].ContactPoint.y > -1)
				{
					sleepCurlUp -= 0.05f;
					if (sleepCurlUp < -0.2f)
					{
						sleepCurlUp = 0f;
						sleepCounter = 0;
					}
				}
				else
				{
					sleepCurlUp = 1f;
				}
			}
			else
			{
				sleepCounter--;
				sleepCurlUp = Mathf.InverseLerp(10f, Malnourished ? 40 : 17, sleepCounter);
			}
			if (Malnourished)
			{
				aerobicLevel = 0.75f * (1f - sleepCurlUp);
			}
			else if (sleepCounter == 0 && !standing)
			{
				animation = AnimationIndex.StandUp;
				standing = true;
			}
		}
		else if (sleepCounter < 0)
		{
			sleepCounter--;
			if (sleepCounter == -6 && standing)
			{
				animation = AnimationIndex.DownOnFours;
				standing = false;
			}
			sleepCurlUp = Mathf.InverseLerp(-14f, -24f, sleepCounter);
		}
		if (isSlugpup)
		{
			sleepCurlUp = 0f;
		}
	}

	public override Color ShortCutColor()
	{
		if (!isSlugpup)
		{
			return PlayerGraphics.SlugcatColor((base.State as PlayerState).slugcatCharacter);
		}
		if (base.abstractCreature.ID.RandomSeed == 1000)
		{
			return new Color(0.6f, 0.7f, 0.9f);
		}
		if (base.abstractCreature.ID.RandomSeed == 1001)
		{
			if (npcStats != null)
			{
				npcStats.Dark = false;
			}
			return new Color(0.48f, 0.87f, 0.81f);
		}
		if (base.abstractCreature.ID.RandomSeed == 1002)
		{
			if (npcStats != null)
			{
				npcStats.Dark = true;
			}
			return new Color(0.43922f, 0.13725f, 0.23529f);
		}
		if (npcStats != null)
		{
			return Custom.HSL2RGB(npcStats.H, npcStats.S, Mathf.Clamp(npcStats.Dark ? (1f - npcStats.L) : npcStats.L, 0.01f, 1f), 1f);
		}
		return new Color(1f, 1f, 1f);
	}

	public override void SpitOutOfShortCut(IntVector2 pos, Room newRoom, bool spitOutAllSticks)
	{
		base.SpitOutOfShortCut(pos, newRoom, spitOutAllSticks);
		forceSleepCounter = 0;
		consolePipeWarpInd = 0;
		cantBeGrabbedCounter = 30;
		if (ModManager.MSC)
		{
			burstX = 0f;
			burstY = 0f;
			if (tongue != null)
			{
				tongue.NewRoom(newRoom);
			}
		}
		stillInStartShelter = false;
		if (newRoom.game.session is StoryGameSession && newRoom.world.region != null && AI == null)
		{
			if (!(newRoom.game.session as StoryGameSession).saveState.regionStates[newRoom.world.region.regionNumber].roomsVisited.Contains(newRoom.abstractRoom.name))
			{
				(newRoom.game.session as StoryGameSession).saveState.regionStates[newRoom.world.region.regionNumber].roomsVisited.Add(newRoom.abstractRoom.name);
			}
			if (newRoom.abstractRoom.shelter)
			{
				newRoom.game.rainWorld.progression.TempDiscoverShelter(newRoom.abstractRoom.name);
			}
		}
		else if (newRoom.game.IsArenaSession)
		{
			newRoom.game.GetArenaGameSession.PlayerSpitOutOfShortCut(base.abstractCreature);
		}
		Vector2 vector = Custom.IntVector2ToVector2(newRoom.ShorcutEntranceHoleDirection(pos));
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			base.bodyChunks[i].HardSetPosition(newRoom.MiddleOfTile(pos) - vector * (-0.5f + (float)i) * 5f);
			base.bodyChunks[i].vel = vector * 2f;
		}
		exitsToBeDiscovered = null;
		if (base.graphicsModule != null)
		{
			base.graphicsModule.Reset();
		}
		if (!ModManager.MSC)
		{
			return;
		}
		if (PainJumps)
		{
			int num = 0;
			for (int j = 0; j < newRoom.physicalObjects.Length; j++)
			{
				for (int k = 0; k < newRoom.physicalObjects[j].Count; k++)
				{
					if (newRoom.physicalObjects[j][k] is Yeek && !(newRoom.physicalObjects[j][k] as Yeek).dead)
					{
						num++;
					}
				}
			}
			if (num == 0)
			{
				AbstractCreature abstractCreature = new AbstractCreature(newRoom.world, StaticWorld.GetCreatureTemplate(MoreSlugcatsEnums.CreatureTemplateType.Yeek), null, base.abstractCreature.pos, newRoom.game.GetNewID());
				abstractCreature.saveCreature = false;
				newRoom.abstractRoom.AddEntity(abstractCreature);
				abstractCreature.RealizeInRoom();
				newRoom.AddObject(new ShockWave(new Vector2(base.mainBodyChunk.pos.x, base.mainBodyChunk.pos.y), 300f, 0.2f, 15));
			}
		}
		if (myRobot != null)
		{
			myRobot.HardSetPos(base.firstChunk.pos);
		}
	}

	public global::HUD.HUD.OwnerType GetOwnerType()
	{
		return global::HUD.HUD.OwnerType.Player;
	}

	public void PlayHUDSound(SoundID soundID)
	{
		base.abstractCreature.world.game.cameras[0].virtualMicrophone.PlaySound(soundID, 0f, 1f, 1f);
	}

	public void FoodCountDownDone()
	{
	}

	public override void NewRoom(Room newRoom)
	{
		base.NewRoom(newRoom);
		if (tongue != null)
		{
			tongue.pos = base.mainBodyChunk.pos;
			tongue.lastPos = base.mainBodyChunk.lastPos;
			tongue.NewRoom(newRoom);
		}
		if (ModManager.MMF && MMF.cfgKeyItemTracking.Value && !newRoom.abstractRoom.NOTRACKERS && newRoom.game.IsStorySession && !newRoom.world.singleRoomWorld)
		{
			lastGoodTrackerSpawnRoom = newRoom.abstractRoom.name;
			lastGoodTrackerSpawnRegion = newRoom.world.region.name;
			lastGoodTrackerSpawnCoord = base.abstractCreature.pos;
		}
	}

	public void UpdateMSC()
	{
		timeSinceSpawned++;
		immuneToFallDamage--;
		if (timeSinceSpawned == 5 && base.abstractCreature.world.game.IsStorySession && AI == null)
		{
			if (ModManager.MSC && !room.abstractRoom.shelter && room.game.globalRain.drainWorldFlood > 0f)
			{
				Custom.Log("Drainworld force cancel due to no shelter");
				room.game.globalRain.drainWorldFlood = 10f;
			}
			if (!room.game.spawnedPendingObjects && (!ModManager.MSC || !room.game.rainWorld.safariMode))
			{
				room.game.spawnedPendingObjects = true;
				WorldCoordinate atPos = new WorldCoordinate(base.abstractCreature.pos.room, base.abstractCreature.pos.x, base.abstractCreature.pos.y + 1, base.abstractCreature.pos.abstractNode);
				room.game.SpawnObjs(room.game.GetStorySession.saveState.GrabSavedObjects(base.abstractCreature, atPos));
				room.game.SpawnCritters(room.game.GetStorySession.saveState.GrabSavedCreatures(base.abstractCreature, atPos), base.abstractCreature);
				if (ModManager.MSC)
				{
					int num = room.game.world.SpawnPupNPCs();
					if (room.game.GetStorySession.saveState.miscWorldSaveData.cyclesSinceLastSlugpup < 0)
					{
						if (num == 0)
						{
							room.game.GetStorySession.saveState.miscWorldSaveData.cyclesSinceLastSlugpup = Mathf.Abs(room.game.GetStorySession.saveState.miscWorldSaveData.cyclesSinceLastSlugpup);
						}
						else
						{
							room.game.GetStorySession.saveState.miscWorldSaveData.cyclesSinceLastSlugpup = 0;
						}
					}
					else if (num == 0)
					{
						room.game.GetStorySession.saveState.miscWorldSaveData.cyclesSinceLastSlugpup++;
					}
					else
					{
						room.game.GetStorySession.saveState.miscWorldSaveData.cyclesSinceLastSlugpup = 0;
					}
				}
			}
			if (ModManager.MMF && room.abstractRoom.shelter)
			{
				for (int i = 0; i < room.physicalObjects.Length; i++)
				{
					for (int j = 0; j < room.physicalObjects[i].Count; j++)
					{
						if (room.physicalObjects[i][j].abstractPhysicalObject.type != AbstractPhysicalObject.AbstractObjectType.Creature)
						{
							room.physicalObjects[i][j].firstChunk.pos = base.firstChunk.pos;
						}
					}
				}
			}
		}
		if (AI == null)
		{
			ProcessDebugInputs();
		}
		if (!ModManager.MSC)
		{
			return;
		}
		gourmandAttackNegateTime--;
		if (showKarmaFoodRainTime > 0)
		{
			showKarmaFoodRainTime--;
		}
		sofCooldown--;
		if (inVoidSea)
		{
			for (int k = 0; k < base.grasps.Length; k++)
			{
				if (base.grasps[k] != null && base.grasps[k].grabbed != null)
				{
					ReleaseGrasp(k);
				}
			}
			Vector2 pos = base.bodyChunks[0].pos;
			if (UnityEngine.Random.value < 0.5f)
			{
				pos = base.bodyChunks[1].pos;
			}
			if (dissolved > 0f && UnityEngine.Random.value < dissolved * 2f)
			{
				room.AddObject(new VoidParticle(pos + Custom.RNV() * 12f, Custom.DegToVec(Custom.AimFromOneVectorToAnother(base.bodyChunks[0].pos, base.bodyChunks[1].pos) + (float)UnityEngine.Random.Range(-5, 5)) * UnityEngine.Random.Range(0.25f, 3f), UnityEngine.Random.Range(20f, 80f)));
			}
		}
		ProcessChatLog();
		if (AI == null)
		{
			if (room != null && room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.ElectricDeath) != null && room.game.manager.musicPlayer != null)
			{
				if ((room.world.name == "MS" || room.world.name == "LM" || room.world.name == "DM") && room.world.rainCycle.timer > room.world.rainCycle.cycleLength - 2000)
				{
					room.game.manager.musicPlayer.RequestMSSirenSong();
				}
				if (room.game.manager.musicPlayer.song != null && room.game.manager.musicPlayer.song is MSSirenSong && !(room.game.manager.musicPlayer.song as MSSirenSong).setVolume.HasValue)
				{
					(room.game.manager.musicPlayer.song as MSSirenSong).setVolume = 0.35f;
				}
			}
			if (room != null && room.game.IsStorySession && room.game.GetStorySession.saveState.wearingCloak && room.game.MoonHasRobe())
			{
				room.game.GetStorySession.saveState.wearingCloak = false;
			}
		}
		AbstractCreature firstAlivePlayer = room.game.FirstAlivePlayer;
		if (room != null && !room.game.wasAnArtificerDream && room.game.session is StoryGameSession && ((AI == null && (room.game.session as StoryGameSession).saveState.hasRobo) || (AI != null && (playerState as PlayerNPCState).Drone)) && (myRobot == null || myRobot.slatedForDeletetion) && (!ModManager.CoopAvailable || (firstAlivePlayer != null && firstAlivePlayer.realizedCreature != null && firstAlivePlayer.realizedCreature == this)))
		{
			myRobot = new AncientBot(base.mainBodyChunk.pos, new Color(1f, 0f, 0f), this, online: true);
			room.AddObject(myRobot);
		}
		ClassMechanicsSpearmaster();
		ClassMechanicsGourmand();
		ClassMechanicsArtificer();
		ClassMechanicsSaint();
		if (!monkAscension)
		{
			if (SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
			{
				base.buoyancy = 0.9f;
			}
			else
			{
				base.buoyancy = 0.95f;
			}
			base.gravity = customPlayerGravity;
			if (base.grasps[0] != null && base.grasps[0].grabbed is EnergyCell && (base.grasps[0].grabbed as EnergyCell).usingTime > 0f && base.grasps[0].grabbed.Submersion == 0f)
			{
				customPlayerGravity = 0.25f;
			}
			else
			{
				customPlayerGravity = Mathf.Lerp(customPlayerGravity, 0.9f, 0.1f);
			}
			base.airFriction = 0.999f;
		}
		TongueUpdate();
	}

	public void GetInitialSlugcatClass()
	{
		if (isNPC)
		{
			SlugCatClass = MoreSlugcatsEnums.SlugcatStatsName.Slugpup;
		}
		else if (ModManager.CoopAvailable && base.abstractCreature.Room.world.game.IsStorySession)
		{
			SlugCatClass = base.abstractCreature.world.game.rainWorld.options.jollyPlayerOptionsArray[playerState.playerNumber].playerClass;
			if (SlugCatClass == null)
			{
				SlugCatClass = base.abstractCreature.world.game.GetStorySession.saveState.saveStateNumber;
			}
		}
		else if (!ModManager.MSC || base.abstractCreature.Room.world.game.IsStorySession)
		{
			SlugCatClass = playerState.slugcatCharacter;
		}
		else
		{
			SlugCatClass = slugcatStats.name;
		}
	}

	public void SubtractFood(int sub)
	{
		if (ModManager.CoopAvailable && base.abstractCreature.world.game.IsStorySession && base.abstractCreature.world.game.Players[0] != base.abstractCreature && AI == null)
		{
			PlayerState playerState = base.abstractCreature.world.game.Players[0].state as PlayerState;
			sub = Math.Max(sub, -playerState.foodInStomach);
			playerState.foodInStomach -= sub;
			this.playerState.foodInStomach = playerState.foodInStomach;
			base.abstractCreature.world.game.GetStorySession.saveState.totFood -= sub;
			JollyCustom.Log($"Substrating food for player {this.playerState.playerNumber}");
		}
		else if (this.playerState.foodInStomach > 0)
		{
			sub = Math.Max(sub, -this.playerState.foodInStomach);
			if (AI == null && base.abstractCreature.world.game.IsStorySession)
			{
				base.abstractCreature.world.game.GetStorySession.saveState.totFood -= sub;
			}
			this.playerState.foodInStomach -= sub;
		}
	}

	public float DeathByBiteMultiplier()
	{
		if (SlugCatClass == SlugcatStats.Name.Yellow || isGourmand)
		{
			return 0f;
		}
		if (ModManager.MSC && SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint)
		{
			return 100f;
		}
		if (room != null && room.game.IsStorySession)
		{
			return 0.7f + room.game.GetStorySession.difficulty / 5f;
		}
		return 0.75f;
	}

	public bool AllowGrabbingBatflys()
	{
		if (!ModManager.MSC || SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Spear)
		{
			if (!SlugcatStats.AutoGrabBatflys(SlugCatClass))
			{
				if (room.game.IsArenaSession)
				{
					if (ModManager.MSC)
					{
						return room.game.GetArenaGameSession.arenaSitting.gameTypeSetup.gameType != MoreSlugcatsEnums.GameTypeID.Challenge;
					}
					return true;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public bool IsCreatureLegalToHoldWithoutStun(Creature grabCheck)
	{
		if (grabCheck is JetFish || grabCheck is LanternMouse || grabCheck is Fly || grabCheck is TubeWorm || grabCheck is Snail || grabCheck is EggBug || grabCheck is Player || grabCheck is Cicada || (grabCheck is Centipede && (grabCheck as Centipede).Small) || grabCheck is SmallNeedleWorm)
		{
			return true;
		}
		if (ModManager.MSC)
		{
			return grabCheck is Yeek;
		}
		return false;
	}

	public bool IsCreatureImmuneToPlayerGrabStun(Creature grabCheck)
	{
		if ((!ModManager.MSC || !(grabCheck is Yeek)) && !(grabCheck is JetFish) && !(grabCheck is Cicada))
		{
			return grabCheck is Player;
		}
		return true;
	}

	public Color? StomachGlowLightColor()
	{
		AbstractPhysicalObject abstractPhysicalObject = ((AI != null) ? (base.State as PlayerNPCState).StomachObject : objectInStomach);
		if (abstractPhysicalObject != null)
		{
			if (objectInStomach.type == AbstractPhysicalObject.AbstractObjectType.Lantern)
			{
				return new Color(1f, 0.4f, 0.3f, 0.85f);
			}
			if (objectInStomach.type == AbstractPhysicalObject.AbstractObjectType.NSHSwarmer)
			{
				return new Color(0.2f, 1f, 0.3f, 0.45f);
			}
			if (objectInStomach.type == AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer)
			{
				return new Color(1f, 1f, 1f, 0.35f);
			}
		}
		return null;
	}

	public override bool SpearStick(Weapon source, float dmg, BodyChunk chunk, Appendage.Pos appPos, Vector2 direction)
	{
		if (isGourmand && (animation == AnimationIndex.Roll || animation == AnimationIndex.BellySlide))
		{
			return false;
		}
		return true;
	}

	public void SuperHardSetPosition(Vector2 pos)
	{
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			base.bodyChunks[i].HardSetPosition(pos);
			for (int j = 0; j < 2; j++)
			{
				(base.graphicsModule as PlayerGraphics).drawPositions[i, j] = pos;
			}
		}
		base.bodyChunks[1].pos.x = base.bodyChunks[0].pos.x - 1f;
		BodyPart[] bodyParts = (base.graphicsModule as PlayerGraphics).bodyParts;
		foreach (BodyPart obj in bodyParts)
		{
			obj.pos = pos;
			obj.lastPos = pos;
		}
		if (tongue != null)
		{
			if (tongue.Attached)
			{
				tongue.Release();
			}
			tongue.pos = base.mainBodyChunk.pos;
			tongue.lastPos = base.mainBodyChunk.lastPos;
			tongue.rope.Reset(pos);
			PlayerGraphics.RopeSegment[] ropeSegments = (base.graphicsModule as PlayerGraphics).ropeSegments;
			foreach (PlayerGraphics.RopeSegment obj2 in ropeSegments)
			{
				obj2.pos = pos;
				obj2.lastPos = pos;
			}
		}
	}

	public void ProcessConsoleDebugInputs()
	{
	}

	public void ProcessDebugInputs()
	{
		if (room == null || !room.game.devToolsActive)
		{
			return;
		}
		for (int i = 256; i <= 265; i++)
		{
			KeyCode key = (KeyCode)i;
			string text = key.ToString();
			int num = int.Parse(text.Substring(text.Length - 1), NumberStyles.Any, CultureInfo.InvariantCulture);
			if (Input.GetKey(key) && num <= room.abstractRoom.exits - 1)
			{
				SuperHardSetPosition(new Vector2((float)room.LocalCoordinateOfNode(num).x * 20f, (float)room.LocalCoordinateOfNode(num).y * 20f));
			}
		}
		if (Input.GetKeyDown("0"))
		{
			if (Input.GetKey(KeyCode.LeftShift))
			{
				room.world.rainCycle.timer = room.world.rainCycle.sunDownStartTime;
			}
			else if (Input.GetKey(KeyCode.LeftAlt))
			{
				room.world.rainCycle.timer = room.world.rainCycle.sunDownStartTime + 1650;
				room.world.rainCycle.dayNightCounter = 1650;
			}
			else if (Input.GetKey(KeyCode.LeftControl))
			{
				room.world.rainCycle.timer += 4000;
			}
			else if (room.world.rainCycle.TimeUntilRain > 60)
			{
				room.world.rainCycle.timer = room.world.rainCycle.cycleLength - 60;
			}
			Custom.Log("Cycle jumper called!", room.world.rainCycle.timer.ToString());
			if (room.game.cameras[0].blizzardGraphics != null)
			{
				room.game.cameras[0].blizzardGraphics.windMapUpdate = true;
				room.game.cameras[0].blizzardGraphics.needsUpdate = true;
			}
		}
		if (Input.GetKeyDown("9"))
		{
			room.world.game.globalRain.ResetRain();
		}
		if (Input.GetKeyDown("8"))
		{
			for (int j = 0; j < room.physicalObjects.Length; j++)
			{
				for (int k = 0; k < room.physicalObjects[j].Count; k++)
				{
					if (room.physicalObjects[j][k] is Creature && !(room.physicalObjects[j][k] is Player))
					{
						(room.physicalObjects[j][k] as Creature).Die();
						if (ModManager.Expedition && room.game.rainWorld.ExpeditionMode)
						{
							(room.game.session as StoryGameSession).playerSessionRecords[0].AddKill(room.physicalObjects[j][k] as Creature);
						}
					}
				}
			}
		}
		if (ModManager.MSC && Input.GetKeyDown("5"))
		{
			AbstractPhysicalObject abstractPhysicalObject = new SpearMasterPearl.AbstractSpearMasterPearl(room.world, null, room.GetWorldCoordinate(base.mainBodyChunk.pos), room.game.GetNewID(), -1, -1, null);
			base.abstractCreature.Room.AddEntity(abstractPhysicalObject);
			abstractPhysicalObject.RealizeInRoom();
			abstractPhysicalObject.realizedObject.firstChunk.pos = base.mainBodyChunk.pos;
		}
		if (Input.GetKeyDown("6"))
		{
			AbstractPhysicalObject abstractPhysicalObject2 = new AbstractPhysicalObject(room.world, AbstractPhysicalObject.AbstractObjectType.NSHSwarmer, null, room.GetWorldCoordinate(base.mainBodyChunk.pos), room.game.GetNewID());
			base.abstractCreature.Room.AddEntity(abstractPhysicalObject2);
			abstractPhysicalObject2.RealizeInRoom();
			abstractPhysicalObject2.realizedObject.firstChunk.pos = base.mainBodyChunk.pos;
		}
		if (ModManager.MSC && Input.GetKeyDown("7"))
		{
			AbstractPhysicalObject abstractPhysicalObject3 = new AbstractPhysicalObject(room.world, MoreSlugcatsEnums.AbstractObjectType.EnergyCell, null, room.GetWorldCoordinate(base.mainBodyChunk.pos), room.game.GetNewID());
			base.abstractCreature.Room.AddEntity(abstractPhysicalObject3);
			abstractPhysicalObject3.RealizeInRoom();
			abstractPhysicalObject3.realizedObject.firstChunk.pos = base.mainBodyChunk.pos;
		}
	}

	public void UpdateAnimation()
	{
		if (longBellySlide && animation != AnimationIndex.BellySlide)
		{
			longBellySlide = false;
		}
		if (stopRollingCounter > 0 && animation != AnimationIndex.Roll)
		{
			stopRollingCounter = 0;
		}
		if (slideUpPole > 0 && animation != AnimationIndex.ClimbOnBeam)
		{
			slideUpPole = 0;
		}
		if (animation == AnimationIndex.None)
		{
			return;
		}
		if (animation == AnimationIndex.CrawlTurn)
		{
			bodyMode = BodyModeIndex.Default;
			base.bodyChunks[0].vel.x += flipDirection;
			base.bodyChunks[1].vel.x -= 2f * (float)flipDirection;
			if (input[0].x > 0 != base.bodyChunks[0].pos.x < base.bodyChunks[1].pos.x)
			{
				base.bodyChunks[0].vel.y -= 3f;
				if (base.bodyChunks[0].pos.y < base.bodyChunks[1].pos.y + 2f)
				{
					animation = AnimationIndex.None;
					base.bodyChunks[0].vel.y -= 1f;
				}
			}
			else
			{
				base.bodyChunks[0].vel.y += 2f;
			}
			if (input[0].x == 0 || IsTileSolid(1, 0, 1))
			{
				animation = AnimationIndex.None;
			}
		}
		else if (animation == AnimationIndex.StandUp)
		{
			if (standing)
			{
				base.bodyChunks[0].vel.x *= 0.7f;
				if (!IsTileSolid(0, 0, 1) && (base.bodyChunks[1].onSlope == 0 || input[0].x == 0))
				{
					bodyMode = BodyModeIndex.Stand;
					if (base.bodyChunks[0].pos.y > base.bodyChunks[1].pos.y + 3f)
					{
						animation = AnimationIndex.None;
						room.PlaySound(SoundID.Slugcat_Regain_Footing, base.bodyChunks[1]);
					}
				}
				else
				{
					animation = AnimationIndex.None;
				}
			}
			else
			{
				animation = AnimationIndex.DownOnFours;
			}
		}
		else if (animation == AnimationIndex.DownOnFours)
		{
			if (!standing)
			{
				base.bodyChunks[0].vel.y -= 2f;
				base.bodyChunks[0].vel.x += flipDirection;
				base.bodyChunks[1].vel.x -= flipDirection;
				if (base.bodyChunks[0].pos.y < base.bodyChunks[1].pos.y || base.bodyChunks[0].ContactPoint.y == -1)
				{
					animation = AnimationIndex.None;
				}
			}
			else
			{
				animation = AnimationIndex.StandUp;
			}
		}
		else if (animation == AnimationIndex.LedgeCrawl)
		{
			base.bodyChunks[0].vel.x += (float)flipDirection * 2f;
			bodyMode = BodyModeIndex.Crawl;
			if (!IsTileSolid(0, flipDirection, 0) && ((IsTileSolid(0, 0, -1) && IsTileSolid(1, 0, -1) && room.GetTilePosition(base.bodyChunks[0].pos).y == room.GetTilePosition(base.bodyChunks[0].pos).y) || (base.bodyChunks[0].ContactPoint.x == flipDirection && input[0].x != 0) || (base.bodyChunks[0].ContactPoint.y > -1 && base.bodyChunks[1].ContactPoint.y > -1)))
			{
				animation = AnimationIndex.None;
			}
		}
		else if (animation == AnimationIndex.LedgeGrab)
		{
			bodyMode = BodyModeIndex.Default;
			if (IsTileSolid(0, flipDirection, 0) && !IsTileSolid(0, flipDirection, 1))
			{
				base.bodyChunks[0].vel *= 0.5f;
				base.bodyChunks[0].pos = (base.bodyChunks[0].pos + (room.MiddleOfTile(room.GetTilePosition(base.bodyChunks[0].pos)) + new Vector2((float)flipDirection * (float)ledgeGrabCounter, 8f + (float)ledgeGrabCounter))) / 2f;
				base.bodyChunks[0].lastPos = base.bodyChunks[0].pos;
				base.bodyChunks[1].vel.x += flipDirection;
				canJump = 1;
				if (input[0].x == flipDirection || input[0].y == 1)
				{
					ledgeGrabCounter++;
					base.bodyChunks[1].vel += new Vector2(-0.5f * (float)flipDirection, -0.5f);
				}
				else if (ledgeGrabCounter > 0)
				{
					ledgeGrabCounter--;
				}
				if (input[0].y == -1 && input[1].y != -1)
				{
					base.bodyChunks[0].pos.y -= 10f;
					input[1].y = -1;
					animation = AnimationIndex.None;
					ledgeGrabCounter = 0;
				}
				else if (input[0].x == -flipDirection && input[1].x == 0)
				{
					base.bodyChunks[0].vel.y += 10f;
					animation = AnimationIndex.None;
					ledgeGrabCounter = 0;
				}
				standing = true;
			}
			else
			{
				animation = AnimationIndex.None;
				ledgeGrabCounter = 0;
			}
		}
		else if (animation == AnimationIndex.HangFromBeam)
		{
			bodyMode = BodyModeIndex.ClimbingOnBeam;
			standing = true;
			base.bodyChunks[0].vel.y = 0f;
			base.bodyChunks[0].vel.x *= 0.2f;
			base.bodyChunks[0].pos.y = room.MiddleOfTile(base.bodyChunks[0].pos).y;
			if (input[0].x != 0 && base.bodyChunks[0].ContactPoint.x != input[0].x)
			{
				if (base.bodyChunks[1].ContactPoint.x != input[0].x)
				{
					base.bodyChunks[0].vel.x += (float)input[0].x * Mathf.Lerp(1.2f, 1.4f, Adrenaline) * slugcatStats.poleClimbSpeedFac * Custom.LerpMap(slowMovementStun, 0f, 10f, 1f, 0.5f);
				}
				animationFrame++;
				if (animationFrame > 20)
				{
					animationFrame = 1;
					room.PlaySound(SoundID.Slugcat_Climb_Along_Horizontal_Beam, base.mainBodyChunk, loop: false, 1f, 1f);
					AerobicIncrease(0.05f);
				}
				base.bodyChunks[1].vel.x += (float)flipDirection * (0.5f + 0.5f * Mathf.Sin((float)animationFrame / 20f * (float)Math.PI * 2f)) * -0.5f;
			}
			else if (animationFrame < 10)
			{
				animationFrame++;
			}
			else if (animationFrame > 10)
			{
				animationFrame--;
			}
			bool flag = false;
			if (input[0].y < 0 && input[1].y == 0)
			{
				animation = AnimationIndex.None;
			}
			else if (input[0].y > 0 && input[1].y == 0)
			{
				if (room.GetTile(base.bodyChunks[0].pos).verticalBeam)
				{
					animation = AnimationIndex.ClimbOnBeam;
					if (base.bodyChunks[0].pos.x < room.MiddleOfTile(base.bodyChunks[0].pos).x)
					{
						flipDirection = -1;
					}
					else
					{
						flipDirection = 1;
					}
				}
				else
				{
					flag = true;
				}
			}
			else if (input[0].jmp && !input[1].jmp)
			{
				flag = true;
			}
			if (flag)
			{
				room.PlaySound(SoundID.Slugcat_Get_Up_On_Horizontal_Beam, base.mainBodyChunk, loop: false, 1f, 1f);
				animation = AnimationIndex.GetUpOnBeam;
				pullupSoftlockSafety = 0;
				straightUpOnHorizontalBeam = false;
				if (room.GetTile(base.bodyChunks[0].pos + new Vector2((float)flipDirection * 20f, 0f)).Terrain == Room.Tile.TerrainType.Solid || !room.GetTile(base.bodyChunks[0].pos + new Vector2((float)flipDirection * 20f, 0f)).horizontalBeam)
				{
					flipDirection = -flipDirection;
				}
				if (room.GetTile(base.bodyChunks[0].pos + new Vector2((float)flipDirection * 20f, 0f)).Terrain == Room.Tile.TerrainType.Solid || !room.GetTile(base.bodyChunks[0].pos + new Vector2((float)flipDirection * 20f, 0f)).horizontalBeam)
				{
					flipDirection = -flipDirection;
					straightUpOnHorizontalBeam = true;
				}
				if (!straightUpOnHorizontalBeam && room.GetTile(base.bodyChunks[0].pos + new Vector2((float)flipDirection * 20f, 20f)).Solid)
				{
					straightUpOnHorizontalBeam = true;
				}
				upOnHorizontalBeamPos = new Vector2(base.bodyChunks[0].pos.x, room.MiddleOfTile(base.bodyChunks[0].pos).y + 20f);
			}
			if (!room.GetTile(base.bodyChunks[0].pos).horizontalBeam)
			{
				animation = AnimationIndex.None;
			}
		}
		else if (animation == AnimationIndex.GetUpOnBeam)
		{
			pullupSoftlockSafety++;
			if (pullupSoftlockSafety > 200)
			{
				Custom.Log("Pullup softlock safety");
				room.PlaySound(SoundID.Slugcat_Turn_In_Corridor, base.mainBodyChunk, loop: false, 1f, 1f);
				pullupSoftlockSafety = 0;
				animation = AnimationIndex.None;
				return;
			}
			bodyMode = BodyModeIndex.ClimbingOnBeam;
			base.bodyChunks[0].vel.x = 0f;
			base.bodyChunks[0].vel.y = 0f;
			forceFeetToHorizontalBeamTile = 20;
			if (straightUpOnHorizontalBeam)
			{
				if (input[0].y < 0 || base.mainBodyChunk.ContactPoint.y > 0)
				{
					straightUpOnHorizontalBeam = false;
				}
				if (room.GetTile(upOnHorizontalBeamPos).Solid)
				{
					for (int num = 1; num >= -1; num -= 2)
					{
						if (!room.GetTile(upOnHorizontalBeamPos + new Vector2((float)(flipDirection * num) * 20f, 0f)).Solid)
						{
							upOnHorizontalBeamPos.x += (float)(flipDirection * num) * 20f;
							break;
						}
					}
				}
				base.mainBodyChunk.vel += Custom.DirVec(base.mainBodyChunk.pos, upOnHorizontalBeamPos) * 1.8f;
				base.bodyChunks[1].vel += Custom.DirVec(base.bodyChunks[1].pos, upOnHorizontalBeamPos + new Vector2(0f, -20f)) * 1.8f;
				if (room.GetTile(base.bodyChunks[1].pos).horizontalBeam && base.bodyChunks[1].pos.y > upOnHorizontalBeamPos.y - 25f)
				{
					noGrabCounter = 15;
					animation = AnimationIndex.StandOnBeam;
					base.bodyChunks[1].pos.y = room.MiddleOfTile(base.bodyChunks[1].pos).y + 5f;
					base.bodyChunks[1].vel.y = 0f;
				}
				else if ((!room.GetTile(base.bodyChunks[0].pos).horizontalBeam && !room.GetTile(base.bodyChunks[1].pos).horizontalBeam) || !Custom.DistLess(base.mainBodyChunk.pos, upOnHorizontalBeamPos, 25f))
				{
					animation = AnimationIndex.None;
				}
				return;
			}
			base.bodyChunks[0].pos.y = room.MiddleOfTile(base.bodyChunks[0].pos).y;
			base.bodyChunks[1].vel.y += 2f;
			base.bodyChunks[1].vel.x += (float)flipDirection * 0.5f;
			if (base.bodyChunks[1].pos.y > base.mainBodyChunk.pos.y - 15f && !room.GetTile(base.mainBodyChunk.pos + new Vector2(Mathf.Sign(base.bodyChunks[1].pos.x - base.mainBodyChunk.pos.x) * 35f, 0f)).horizontalBeam && room.GetTile(base.mainBodyChunk.pos + new Vector2(Mathf.Sign(base.bodyChunks[1].pos.x - base.mainBodyChunk.pos.x) * -15f, 0f)).horizontalBeam)
			{
				base.mainBodyChunk.vel.x -= Mathf.Sign(base.bodyChunks[1].pos.x - base.mainBodyChunk.pos.x) * 1.5f;
				base.bodyChunks[1].vel.x -= Mathf.Sign(base.bodyChunks[1].pos.x - base.mainBodyChunk.pos.x) * 0.5f;
			}
			if (base.bodyChunks[1].ContactPoint.y > 0)
			{
				if (!room.GetTile(base.mainBodyChunk.pos + new Vector2(0f, 20f)).Solid)
				{
					straightUpOnHorizontalBeam = true;
				}
				else
				{
					animation = AnimationIndex.HangFromBeam;
				}
			}
			if (base.bodyChunks[1].pos.y > base.bodyChunks[0].pos.y)
			{
				noGrabCounter = 15;
				animation = AnimationIndex.StandOnBeam;
				base.bodyChunks[1].pos.y = room.MiddleOfTile(base.bodyChunks[0].pos).y + 5f;
				base.bodyChunks[1].vel.y = 0f;
			}
			if (!room.GetTile(base.bodyChunks[0].pos).horizontalBeam)
			{
				pullupSoftlockSafety = 0;
				animation = AnimationIndex.None;
			}
		}
		else if (animation == AnimationIndex.StandOnBeam)
		{
			bodyMode = BodyModeIndex.ClimbingOnBeam;
			standing = true;
			canJump = 5;
			base.bodyChunks[1].vel.x *= 0.5f;
			if (base.bodyChunks[0].ContactPoint.y < 1 || !IsTileSolid(1, 0, 1))
			{
				base.bodyChunks[1].vel.y = 0f;
				base.bodyChunks[1].pos.y = room.MiddleOfTile(base.bodyChunks[1].pos).y + 5f;
				base.bodyChunks[0].vel.y += 2f;
				dynamicRunSpeed[0] = 2.1f * slugcatStats.runspeedFac;
				dynamicRunSpeed[1] = 2.1f * slugcatStats.runspeedFac;
				if (input[0].y < 0 && input[1].y == 0)
				{
					animation = AnimationIndex.None;
				}
			}
			else
			{
				animation = AnimationIndex.None;
			}
			if (input[0].x != 0)
			{
				animationFrame++;
			}
			else
			{
				animationFrame = 0;
			}
			if (animationFrame > 6)
			{
				animationFrame = 0;
				room.PlaySound(SoundID.Slugcat_Walk_On_Horizontal_Beam, base.mainBodyChunk, loop: false, 1f, 1f);
			}
			if (input[0].y == 1 && input[1].y == 0 && room.GetTile(room.GetTilePosition(base.bodyChunks[0].pos) + new IntVector2(0, 1)).horizontalBeam)
			{
				base.bodyChunks[0].pos.y += 8f;
				base.bodyChunks[1].pos.y += 8f;
				animation = AnimationIndex.HangFromBeam;
			}
		}
		else if (animation == AnimationIndex.ClimbOnBeam)
		{
			bodyMode = BodyModeIndex.ClimbingOnBeam;
			standing = true;
			canJump = 1;
			for (int i = 0; i < 2; i++)
			{
				if (base.bodyChunks[i].ContactPoint.x != 0)
				{
					flipDirection = -base.bodyChunks[i].ContactPoint.x;
				}
			}
			base.bodyChunks[0].vel.x = 0f;
			bool flag2 = true;
			if (!IsTileSolid(0, 0, 1) && input[0].y > 0 && (base.bodyChunks[0].ContactPoint.y < 0 || IsTileSolid(0, flipDirection, 1)))
			{
				flag2 = false;
			}
			if (flag2 && IsTileSolid(0, flipDirection, 0))
			{
				flipDirection = -flipDirection;
			}
			if (flag2)
			{
				base.bodyChunks[0].pos.x = (base.bodyChunks[0].pos.x + room.MiddleOfTile(base.bodyChunks[0].pos).x + (float)flipDirection * 5f) / 2f;
				base.bodyChunks[1].pos.x = (base.bodyChunks[1].pos.x * 7f + room.MiddleOfTile(base.bodyChunks[0].pos).x + (float)flipDirection * 5f) / 8f;
			}
			else
			{
				base.bodyChunks[0].pos.x = (base.bodyChunks[0].pos.x + room.MiddleOfTile(base.bodyChunks[0].pos).x) / 2f;
				base.bodyChunks[1].pos.x = (base.bodyChunks[1].pos.x * 7f + room.MiddleOfTile(base.bodyChunks[0].pos).x) / 8f;
			}
			base.bodyChunks[0].vel.y *= 0.5f;
			if (input[0].y > 0)
			{
				animationFrame++;
				if (animationFrame > 20)
				{
					animationFrame = 0;
					room.PlaySound(SoundID.Slugcat_Climb_Up_Vertical_Beam, base.mainBodyChunk, loop: false, 1f, 1f);
					AerobicIncrease(0.1f);
				}
				base.bodyChunks[0].vel.y += Mathf.Lerp(1f, 1.4f, Adrenaline) * slugcatStats.poleClimbSpeedFac * Custom.LerpMap(slowMovementStun, 0f, 10f, 1f, 0.2f);
			}
			else if (input[0].y < 0)
			{
				base.bodyChunks[0].vel.y -= 2.2f * (0.2f + 0.8f * EffectiveRoomGravity);
			}
			base.bodyChunks[0].vel.y += 1f + base.gravity;
			base.bodyChunks[1].vel.y -= 1f - base.gravity;
			if (slideUpPole > 0)
			{
				slideUpPole--;
				if (slideUpPole > 8)
				{
					animationFrame = 12;
				}
				if (slideUpPole == 0)
				{
					slowMovementStun = Math.Max(slowMovementStun, 16);
				}
				if (slideUpPole > 14)
				{
					base.bodyChunks[0].pos.y += 2f * (isSlugpup ? 0.3f : 1f);
					base.bodyChunks[1].pos.y += 2f * (isSlugpup ? 0.3f : 1f);
				}
				base.bodyChunks[0].vel.y += Custom.LerpMap(slideUpPole, 17f, 0f, 3f, -1.2f, 0.45f) * (isSlugpup ? 0.5f : 1f);
				base.bodyChunks[1].vel.y += Custom.LerpMap(slideUpPole, 17f, 0f, 1.5f, -1.4f, 0.45f) * (isSlugpup ? 0.5f : 1f);
			}
			base.GoThroughFloors = input[0].x == 0 && input[0].downDiagonal == 0;
			if (input[0].x != 0 && input[1].x != input[0].x && input[0].x == flipDirection && input[0].x == lastFlipDirection)
			{
				if (room.GetTile(base.bodyChunks[0].pos).horizontalBeam && !IsTileSolid(0, 0, -1))
				{
					animation = AnimationIndex.HangFromBeam;
				}
				else if (room.GetTile(base.bodyChunks[1].pos).horizontalBeam)
				{
					animation = AnimationIndex.StandOnBeam;
				}
			}
			if (input[0].x == flipDirection && input[1].x == 0 && flipDirection == lastFlipDirection && room.GetTile(room.GetTilePosition(base.bodyChunks[0].pos) + new IntVector2(flipDirection, 0)).verticalBeam)
			{
				base.bodyChunks[0].pos.x = room.MiddleOfTile(room.GetTilePosition(base.bodyChunks[0].pos) + new IntVector2(flipDirection, 0)).x - (float)flipDirection * 5f;
				flipDirection = -flipDirection;
				jumpStun = 11 * flipDirection;
			}
			if (base.bodyChunks[1].ContactPoint.y < 0 && input[0].y < 0)
			{
				room.PlaySound(SoundID.Slugcat_Regain_Footing, base.mainBodyChunk, loop: false, 1f, 1f);
				animation = AnimationIndex.StandUp;
			}
			if (!room.GetTile(base.bodyChunks[0].pos).verticalBeam)
			{
				animation = AnimationIndex.None;
				if (room.GetTile(room.GetTilePosition(base.bodyChunks[0].pos) + new IntVector2(0, -1)).verticalBeam)
				{
					room.PlaySound(SoundID.Slugcat_Get_Up_On_Top_Of_Vertical_Beam_Tip, base.mainBodyChunk, loop: false, 1f, 1f);
					animation = AnimationIndex.GetUpToBeamTip;
				}
				else if (room.GetTile(room.GetTilePosition(base.bodyChunks[0].pos) + new IntVector2(0, 1)).verticalBeam)
				{
					animation = AnimationIndex.HangUnderVerticalBeam;
				}
			}
		}
		else if (animation == AnimationIndex.GetUpToBeamTip)
		{
			bodyMode = BodyModeIndex.ClimbingOnBeam;
			standing = true;
			canJump = 5;
			base.bodyChunks[0].vel.y += base.gravity;
			base.bodyChunks[1].vel.y += base.gravity;
			Vector2 p = new Vector2(0f, 0f);
			for (int j = 0; j < 2; j++)
			{
				if (!room.GetTile(base.bodyChunks[j].pos).verticalBeam && room.GetTile(base.bodyChunks[j].pos + new Vector2(0f, -20f)).verticalBeam)
				{
					p = room.MiddleOfTile(base.bodyChunks[j].pos);
					break;
				}
			}
			if (p.x != 0f || p.y != 0f)
			{
				base.bodyChunks[0].pos.x = (base.bodyChunks[0].pos.x * 14f + p.x) / 15f;
				base.bodyChunks[1].pos.x = (base.bodyChunks[1].pos.x * 4f + p.x) / 5f;
				base.bodyChunks[0].vel.y += 0.1f;
				base.bodyChunks[1].pos.y = (base.bodyChunks[1].pos.y * 4f + p.y) / 5f;
				if (Custom.DistLess(base.bodyChunks[1].pos, p, 6f))
				{
					animation = AnimationIndex.BeamTip;
					room.PlaySound(SoundID.Slugcat_Regain_Footing, base.mainBodyChunk, loop: false, 0.3f, 1f);
				}
			}
			else
			{
				animation = AnimationIndex.None;
			}
		}
		else if (animation == AnimationIndex.BeamTip)
		{
			bodyMode = BodyModeIndex.ClimbingOnBeam;
			standing = true;
			canJump = 5;
			base.bodyChunks[1].vel *= 0.5f;
			base.bodyChunks[1].pos = (base.bodyChunks[1].pos + room.MiddleOfTile(base.bodyChunks[1].pos)) / 2f;
			base.bodyChunks[0].vel.y += 1.5f;
			base.bodyChunks[0].vel.y += (float)input[0].y * 0.1f;
			base.bodyChunks[0].vel.x += (float)input[0].x * 0.1f;
			if (input[0].y > 0 && input[1].y == 0)
			{
				base.bodyChunks[1].vel.y -= 1f;
				canJump = 0;
				animation = AnimationIndex.None;
			}
			if ((input[0].y < 0 && input[1].y == 0) || base.bodyChunks[0].pos.y < base.bodyChunks[1].pos.y || !room.GetTile(base.bodyChunks[1].pos + new Vector2(0f, -20f)).verticalBeam)
			{
				animation = AnimationIndex.None;
			}
		}
		else if (animation == AnimationIndex.HangUnderVerticalBeam)
		{
			bodyMode = BodyModeIndex.ClimbingOnBeam;
			standing = false;
			if ((input[0].y < 0 && input[1].y == 0) || base.bodyChunks[1].vel.magnitude > 10f || base.bodyChunks[0].vel.magnitude > 10f || !room.GetTile(base.bodyChunks[0].pos + new Vector2(0f, 20f)).verticalBeam)
			{
				animation = AnimationIndex.None;
				standing = true;
			}
			else
			{
				base.bodyChunks[0].pos.x = Mathf.Lerp(base.bodyChunks[0].pos.x, room.MiddleOfTile(base.bodyChunks[0].pos).x, 0.5f);
				base.bodyChunks[0].pos.y = Mathf.Max(base.bodyChunks[0].pos.y, room.MiddleOfTile(base.bodyChunks[0].pos).y + 5f + base.bodyChunks[0].vel.y);
				base.bodyChunks[0].vel.x *= 0f;
				base.bodyChunks[0].vel.y *= 0.5f;
				base.bodyChunks[1].vel.x += input[0].x;
				if (input[0].y > 0)
				{
					base.bodyChunks[0].vel.y += 2.5f;
				}
				if (room.GetTile(base.bodyChunks[0].pos).verticalBeam)
				{
					animation = AnimationIndex.ClimbOnBeam;
				}
			}
			if (input[0].jmp && !input[1].jmp)
			{
				animation = AnimationIndex.None;
				if (input[0].x == 0)
				{
					base.bodyChunks[0].pos.y += 16f;
					base.bodyChunks[0].vel.y = 10f;
					standing = true;
				}
				else
				{
					base.bodyChunks[1].vel.y += 4f;
					base.bodyChunks[1].vel.x += 2f * (float)input[0].x;
					base.bodyChunks[0].vel.y += 6f;
					base.bodyChunks[0].vel.x += 3f * (float)input[0].x;
				}
			}
		}
		else if (animation == AnimationIndex.DeepSwim)
		{
			dynamicRunSpeed[0] = 0f;
			dynamicRunSpeed[1] = 0f;
			if (base.grasps[0] != null && base.grasps[0].grabbed is JetFish && (base.grasps[0].grabbed as JetFish).Consious)
			{
				base.waterFriction = 1f;
				return;
			}
			canJump = 0;
			standing = false;
			base.GoThroughFloors = true;
			float num2 = (Mathf.Abs(Vector2.Dot(base.bodyChunks[0].vel.normalized, (base.bodyChunks[0].pos - base.bodyChunks[1].pos).normalized)) + Mathf.Abs(Vector2.Dot(base.bodyChunks[1].vel.normalized, (base.bodyChunks[0].pos - base.bodyChunks[1].pos).normalized))) / 2f;
			bool flag3 = airInLungs > 0.5f;
			if (isRivulet)
			{
				flag3 = submerged;
			}
			if (input[0].jmp && !input[1].jmp && flag3)
			{
				if (waterJumpDelay == 0)
				{
					swimCycle = 2.7f;
					Vector2 vector = Custom.DirVec(base.bodyChunks[1].pos, base.bodyChunks[0].pos);
					float num3 = 1f;
					if (ModManager.MMF && MMF.cfgFreeSwimBoosts.Value)
					{
						num3 = 0f;
					}
					if (isRivulet)
					{
						airInLungs -= 0.025f * num3;
						base.bodyChunks[0].vel += vector * ((vector.y > 0.5f) ? 300f : 50f);
					}
					else if (ModManager.MSC && SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
					{
						airInLungs -= 0.015f * num3;
						base.bodyChunks[0].vel += vector * 6f;
					}
					else
					{
						base.bodyChunks[0].vel += vector * 3f;
						airInLungs -= (ModManager.MMF ? 0.18f : 0.2f) * num3;
					}
					if (room.BeingViewed)
					{
						room.AddObject(new Bubble(base.mainBodyChunk.pos, base.mainBodyChunk.vel, bottomBubble: false, fakeWaterBubble: false));
					}
				}
				else
				{
					swimCycle = 0f;
				}
				waterJumpDelay = (isRivulet ? 10 : 20);
			}
			swimCycle += 0.01f;
			if (input[0].ZeroGGamePadIntVec.x != 0 || input[0].ZeroGGamePadIntVec.y != 0)
			{
				float value = Vector2.Angle(base.bodyChunks[0].lastPos - base.bodyChunks[1].lastPos, base.bodyChunks[0].pos - base.bodyChunks[1].pos);
				float num4 = 0.2f + Mathf.InverseLerp(0f, 12f, value) * 0.8f;
				if (slowMovementStun > 0)
				{
					num4 *= 0.5f;
				}
				num4 *= Mathf.Lerp(1f, 1.2f, Adrenaline);
				if (num4 > swimForce)
				{
					swimForce = Mathf.Lerp(swimForce, num4, 0.7f);
				}
				else
				{
					swimForce = Mathf.Lerp(swimForce, num4, 0.05f);
				}
				swimCycle += Mathf.Lerp(swimForce, 1f, 0.5f) / 10f;
				if (airInLungs < 0.5f && airInLungs > 1f / 6f)
				{
					swimCycle += 0.05f;
				}
				if (base.bodyChunks[0].ContactPoint.x != 0 || base.bodyChunks[0].ContactPoint.y != 0)
				{
					swimForce *= 0.5f;
				}
				if (swimCycle > 4f)
				{
					swimCycle = 0f;
				}
				else if (swimCycle > 3f)
				{
					base.bodyChunks[0].vel += Custom.DirVec(base.bodyChunks[1].pos, base.bodyChunks[0].pos) * 0.7f * Mathf.Lerp(swimForce, 1f, 0.5f) * base.bodyChunks[0].submersion;
				}
				Vector2 vector2 = SwimDir(normalize: true);
				if (airInLungs < 0.3f)
				{
					float num5 = airInLungs;
					if (ModManager.MMF && MMF.cfgSwimBreathLeniency.Value && num5 > 0f)
					{
						if (isRivulet)
						{
							num5 = 1f + Mathf.Log10(airInLungs + 0.2f);
						}
						else if (!ModManager.MSC || SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Artificer)
						{
							num5 = Mathf.Lerp(airInLungs, airInLungs, 0.9f + Mathf.Log(airInLungs + 0.12f));
						}
					}
					vector2 = Vector3.Slerp(vector2, new Vector2(0f, 1f), Mathf.InverseLerp(0.3f, 0f, num5));
				}
				if (ModManager.MSC && base.grasps[0] != null && base.grasps[0].grabbed is EnergyCell && (base.grasps[0].grabbed as EnergyCell).usingTime > 0f && base.grasps[0].grabbed.Submersion != 0f)
				{
					base.bodyChunks[0].vel += vector2 * 0.5f * swimForce * Mathf.Lerp(num2, 1f, 0.5f) * base.bodyChunks[0].submersion * 3f;
					base.bodyChunks[1].vel -= vector2 * 0.1f * base.bodyChunks[0].submersion;
					base.bodyChunks[0].vel += Custom.DirVec(base.bodyChunks[1].pos, base.bodyChunks[0].pos) * 0.4f * swimForce * num2 * base.bodyChunks[0].submersion * 3f;
					if (base.bodyChunks[0].vel.magnitude > 75f)
					{
						base.bodyChunks[0].vel = base.bodyChunks[0].vel.normalized * 75f;
					}
				}
				else
				{
					base.bodyChunks[0].vel += vector2 * 0.5f * swimForce * Mathf.Lerp(num2, 1f, 0.5f) * base.bodyChunks[0].submersion;
					base.bodyChunks[1].vel -= vector2 * 0.1f * base.bodyChunks[0].submersion;
					base.bodyChunks[0].vel += Custom.DirVec(base.bodyChunks[1].pos, base.bodyChunks[0].pos) * 0.4f * swimForce * num2 * base.bodyChunks[0].submersion;
				}
				if (base.bodyChunks[0].vel.magnitude < 3f)
				{
					base.bodyChunks[0].vel += vector2 * 0.2f * Mathf.InverseLerp(3f, 1.5f, base.bodyChunks[0].vel.magnitude);
					base.bodyChunks[1].vel -= vector2 * 0.1f * Mathf.InverseLerp(3f, 1.5f, base.bodyChunks[0].vel.magnitude);
				}
			}
			if (isRivulet && waterJumpDelay >= 5)
			{
				base.waterFriction = 0.99f;
			}
			else
			{
				base.waterFriction = Mathf.Lerp(0.92f, 0.96f, num2);
			}
			if (bodyMode != BodyModeIndex.Swimming)
			{
				animation = AnimationIndex.None;
			}
		}
		else if (animation == AnimationIndex.SurfaceSwim)
		{
			if (base.grasps[0] != null && base.grasps[0].grabbed is JetFish && (base.grasps[0].grabbed as JetFish).Consious)
			{
				dynamicRunSpeed[0] = 0f;
				dynamicRunSpeed[1] = 0f;
				base.waterFriction = 1f;
				return;
			}
			canJump = 0;
			swimCycle += 0.025f;
			if (isRivulet && waterJumpDelay >= 5)
			{
				base.waterFriction = 0.999f;
			}
			else
			{
				base.waterFriction = 0.96f;
			}
			swimForce *= 0.5f;
			if (input[0].y > -1 && base.bodyChunks[0].vel.y > -5f && base.bodyChunks[0].vel.y < 3f && waterJumpDelay == 0 && !input[0].jmp)
			{
				base.bodyChunks[0].vel.y *= 0.8f;
				base.bodyChunks[1].vel.y *= 0.8f;
				base.bodyChunks[0].vel.y += Mathf.Clamp((base.bodyChunks[0].pos.y - (room.FloatWaterLevel(base.bodyChunks[0].pos.x) + 15f)) * -0.1f, -0.5f, 1.5f);
				base.bodyChunks[1].vel.y -= 0.5f;
			}
			else if (input[0].y == -1)
			{
				base.bodyChunks[0].vel.y -= 0.2f;
				base.bodyChunks[1].vel.y += 0.1f;
			}
			else if (input[0].y == 1)
			{
				base.bodyChunks[0].vel.y += 0.5f;
			}
			dynamicRunSpeed[0] = (isRivulet ? 5f : 2.7f);
			dynamicRunSpeed[1] = 0f;
			if (input[0].x != 0)
			{
				float num6 = (isRivulet ? 1.5f : 1f);
				base.bodyChunks[1].vel.x -= (float)input[0].x * Mathf.Lerp(0.2f * num6, 0.3f * num6, Adrenaline);
				swimCycle += 1f / 30f;
			}
			if (input[0].jmp && !input[1].jmp)
			{
				if (waterJumpDelay == 0)
				{
					if (base.bodyChunks[0].vel.y < 2f && base.bodyChunks[1].vel.y < 2f)
					{
						base.bodyChunks[0].vel.y += (isRivulet ? 18f : 6f);
						base.bodyChunks[1].vel.y += (isRivulet ? 18f : 6f);
					}
					else
					{
						float num7 = (isRivulet ? 3f : 1f);
						base.bodyChunks[0].vel += Custom.DirVec(base.bodyChunks[1].pos, base.bodyChunks[0].pos) * 4f * num7;
						base.bodyChunks[1].vel.y += ((base.bodyChunks[1].vel.y < 4f) ? 3f : 1.5f) * num7;
					}
					waterJumpDelay = (isRivulet ? 6 : 17);
				}
				else
				{
					if (waterJumpDelay < 10 && !isRivulet)
					{
						waterJumpDelay = 10;
					}
					base.bodyChunks[0].vel += new Vector2(0f, isRivulet ? 12f : 4f);
					base.bodyChunks[1].vel.x *= 0.75f;
				}
			}
			if (bodyMode != BodyModeIndex.Swimming)
			{
				animation = AnimationIndex.None;
			}
		}
		else if (animation == AnimationIndex.Roll)
		{
			bodyMode = BodyModeIndex.Default;
			Vector2 vector3 = Custom.PerpendicularVector(base.bodyChunks[1].pos, base.bodyChunks[0].pos);
			base.bodyChunks[0].vel *= 0.9f;
			base.bodyChunks[1].vel *= 0.9f;
			base.bodyChunks[0].vel += vector3 * 2f * rollDirection;
			base.bodyChunks[1].vel -= vector3 * 2f * rollDirection;
			AerobicIncrease(0.01f);
			bool flag4 = base.bodyChunks[0].ContactPoint.x == rollDirection || base.bodyChunks[1].ContactPoint.x == rollDirection;
			if (base.bodyChunks[1].onSlope == -rollDirection || base.bodyChunks[0].onSlope == -rollDirection)
			{
				base.bodyChunks[0].pos += vector3 * rollDirection;
				base.bodyChunks[1].pos -= vector3 * rollDirection;
			}
			if (!IsTileSolid(0, 0, -1) && !IsTileSolid(1, 0, -1) && base.bodyChunks[0].ContactPoint.y >= 0 && base.bodyChunks[1].ContactPoint.y >= 0)
			{
				if (IsTileSolid(0, 0, -2) || IsTileSolid(1, 0, -2))
				{
					base.bodyChunks[0].vel *= 0.7f;
					base.bodyChunks[1].vel *= 0.7f;
					base.bodyChunks[0].pos.y -= 2.5f;
					base.bodyChunks[1].pos.y -= 2.5f;
				}
				else
				{
					flag4 = true;
				}
			}
			else
			{
				base.bodyChunks[0].vel.x += 1.1f * (float)rollDirection;
				base.bodyChunks[1].vel.x += 1.1f * (float)rollDirection;
				canJump = Math.Max(canJump, 5);
				for (int k = 0; k < 2; k++)
				{
					if (IsTileSolid(k, rollDirection, 0) && !IsTileSolid(k, rollDirection, 1) && !IsTileSolid(0, 0, 1) && !IsTileSolid(1, 0, 1))
					{
						Custom.Log("roll up ledge");
						base.bodyChunks[0].vel *= 0.7f;
						base.bodyChunks[1].vel *= 0.7f;
						base.bodyChunks[0].pos.y += 5f;
						base.bodyChunks[1].pos.y += 5f;
						base.bodyChunks[0].vel.y += base.gravity;
						base.bodyChunks[1].vel.y += base.gravity;
						flag4 = false;
						break;
					}
				}
			}
			if (flag4)
			{
				stopRollingCounter++;
			}
			else
			{
				stopRollingCounter = 0;
			}
			if ((((rollCounter > 15 && input[0].y > -1 && input[0].downDiagonal == 0) || ((float)rollCounter > 30f + 80f * Adrenaline * (isSlugpup ? 0.5f : 1f) && (!isGourmand || gourmandExhausted)) || input[0].x == -rollDirection) && base.bodyChunks[0].pos.y > base.bodyChunks[1].pos.y) || ((float)rollCounter > 60f + 80f * Adrenaline * (isSlugpup ? 0.5f : 1f) && (!isGourmand || gourmandExhausted)) || stopRollingCounter > 6)
			{
				rollDirection = 0;
				room.PlaySound(SoundID.Slugcat_Roll_Finish, base.mainBodyChunk.pos, 1f, 1f);
				animation = AnimationIndex.None;
				standing = input[0].y > -1;
			}
		}
		else if (animation == AnimationIndex.RocketJump)
		{
			bodyMode = BodyModeIndex.Default;
			standing = false;
			base.bodyChunks[1].vel *= 0.99f;
			Vector2 normalized = base.bodyChunks[0].vel.normalized;
			base.bodyChunks[0].vel += normalized;
			base.bodyChunks[1].vel -= normalized;
			base.bodyChunks[0].vel.y += 0.1f;
			base.bodyChunks[1].vel.y += 0.1f;
			if (base.bodyChunks[1].ContactPoint.x != 0 || base.bodyChunks[1].ContactPoint.y != 0)
			{
				animation = AnimationIndex.None;
			}
		}
		else if (animation == AnimationIndex.Flip)
		{
			bodyMode = BodyModeIndex.Default;
			Vector2 vector4 = Custom.PerpendicularVector(base.bodyChunks[1].pos, base.bodyChunks[0].pos);
			base.bodyChunks[0].vel -= vector4 * slideDirection * Mathf.Lerp(0.38f, 0.8f, Adrenaline) * (flipFromSlide ? 2.5f : 1f);
			base.bodyChunks[1].vel += vector4 * slideDirection * Mathf.Lerp(0.38f, 0.8f, Adrenaline) * (flipFromSlide ? 2.5f : 1f);
			standing = false;
			for (int l = 0; l < 2; l++)
			{
				if (base.bodyChunks[l].ContactPoint.x != 0 || base.bodyChunks[l].ContactPoint.y != 0)
				{
					animation = AnimationIndex.None;
					standing = base.bodyChunks[0].pos.y > base.bodyChunks[1].pos.y;
					break;
				}
			}
		}
		else if (animation == AnimationIndex.BellySlide)
		{
			bodyMode = BodyModeIndex.Default;
			if (rollCounter < 6 && !isRivulet)
			{
				base.bodyChunks[1].vel.y += 2.7f;
				base.bodyChunks[1].vel.x -= 9.1f * (float)rollDirection;
			}
			else if (IsTileSolid(1, 0, -1) || IsTileSolid(1, 0, -2))
			{
				base.bodyChunks[1].vel.y -= 0.5f;
			}
			float num8 = 14f;
			float num9 = 18.1f;
			if (isRivulet)
			{
				num8 = 20f;
				num9 = 25f;
			}
			else if (isGourmand)
			{
				if (gourmandExhausted)
				{
					num8 = 10f;
					num9 = 14f;
				}
				else
				{
					num8 = 40f;
					num9 = 45f;
				}
			}
			else if (isSlugpup)
			{
				num8 = 7f;
				num9 = 9f;
			}
			base.bodyChunks[0].vel.x += (longBellySlide ? num8 : num9) * (float)rollDirection * Mathf.Sin((float)rollCounter / (longBellySlide ? 39f : 15f) * (float)Math.PI);
			if (IsTileSolid(0, 0, -1) || IsTileSolid(0, 0, -2))
			{
				base.bodyChunks[0].vel.y -= 2.3f;
			}
			for (int m = 0; m < 2; m++)
			{
				if (base.bodyChunks[m].ContactPoint.y == 0)
				{
					base.bodyChunks[m].vel.x *= surfaceFriction;
				}
			}
			if (input[0].y < 0 && input[0].downDiagonal == 0 && input[0].x == 0 && rollCounter > 8 && room.GetTilePosition(base.bodyChunks[0].pos).y == room.GetTilePosition(base.bodyChunks[1].pos).y)
			{
				IntVector2 tilePosition = room.GetTilePosition(base.mainBodyChunk.pos);
				if (!room.GetTile(tilePosition + new IntVector2(0, -1)).Solid && room.GetTile(tilePosition + new IntVector2(-1, -1)).Solid && room.GetTile(tilePosition + new IntVector2(1, -1)).Solid)
				{
					base.bodyChunks[0].pos = room.MiddleOfTile(base.bodyChunks[0].pos) + new Vector2(0f, -20f);
					base.bodyChunks[0].vel = new Vector2(0f, -11f);
					base.bodyChunks[1].pos = Vector2.Lerp(base.bodyChunks[1].pos, base.bodyChunks[0].pos + new Vector2(0f, bodyChunkConnections[0].distance), 0.5f);
					base.bodyChunks[1].vel = new Vector2(0f, -11f);
					animation = AnimationIndex.None;
					standing = false;
					base.GoThroughFloors = true;
					rollDirection = 0;
					return;
				}
			}
			if (input[0].x != rollDirection && input[0].downDiagonal != rollDirection)
			{
				exitBellySlideCounter++;
			}
			else
			{
				exitBellySlideCounter = 0;
			}
			if (longBellySlide)
			{
				whiplashJump = false;
			}
			else if (rollCounter > 5 && input[0].x == -rollDirection)
			{
				whiplashJump = true;
			}
			int num10 = 12;
			int num11 = 34;
			if (isRivulet)
			{
				num10 = 6;
				num11 = 20;
			}
			if ((rollCounter > 8 && exitBellySlideCounter > (longBellySlide ? 16 : 6)) || rollCounter > (longBellySlide ? 39 : 15) || (!longBellySlide && rollCounter > 6 && !IsTileSolid(0, 0, -1) && !IsTileSolid(1, 0, -1)) || (input[0].jmp && !input[1].jmp && rollCounter > 0 && rollCounter < (longBellySlide ? num11 : num10)))
			{
				base.bodyChunks[0].vel.y = 0f;
				base.bodyChunks[1].vel.y = 0f;
				rollDirection = 0;
				animation = AnimationIndex.None;
				if (longBellySlide)
				{
					standing = true;
					base.bodyChunks[0].vel.y = 6f;
					base.bodyChunks[1].vel.y = 4f;
					room.PlaySound(SoundID.Slugcat_Normal_Jump, base.mainBodyChunk.pos, 0.5f, 1f);
				}
				else
				{
					standing = input[0].y == 1 && !IsTileSolid(0, 0, 1);
					for (int n = 0; n < 2; n++)
					{
						if (Mathf.Abs(base.bodyChunks[n].vel.x) > 8f)
						{
							base.bodyChunks[n].vel *= 0.5f;
						}
					}
					slowMovementStun = (standing ? 20 : 40);
					room.PlaySound(standing ? SoundID.Slugcat_Belly_Slide_Finish_Success : SoundID.Slugcat_Belly_Slide_Finish_Fail, base.mainBodyChunk.pos, 1f, 1f);
				}
				longBellySlide = false;
			}
			else
			{
				standing = false;
			}
		}
		else if (animation == AnimationIndex.CorridorTurn)
		{
			if (corridorTurnDir.HasValue && bodyMode == BodyModeIndex.CorridorClimb && corridorTurnCounter < 40)
			{
				slowMovementStun = Math.Max(10, slowMovementStun);
				base.mainBodyChunk.vel *= 0.5f;
				base.bodyChunks[1].vel *= 0.5f;
				if (corridorTurnCounter < 30)
				{
					base.mainBodyChunk.vel += Custom.DegToVec(UnityEngine.Random.value * 360f);
				}
				else
				{
					base.mainBodyChunk.vel += corridorTurnDir.Value.ToVector2() * 0.5f;
					base.bodyChunks[1].vel -= corridorTurnDir.Value.ToVector2() * 0.5f;
				}
				corridorTurnCounter++;
				return;
			}
			base.mainBodyChunk.vel += corridorTurnDir.Value.ToVector2() * 6f;
			base.bodyChunks[1].vel += corridorTurnDir.Value.ToVector2() * 5f;
			if (base.graphicsModule != null)
			{
				for (int num12 = 0; num12 < base.graphicsModule.bodyParts.Length; num12++)
				{
					base.graphicsModule.bodyParts[num12].vel -= corridorTurnDir.Value.ToVector2() * 10f;
				}
			}
			corridorTurnDir = null;
			animation = AnimationIndex.None;
			room.PlaySound(SoundID.Slugcat_Turn_In_Corridor, base.mainBodyChunk, loop: false, 1f, 1f);
		}
		else if (animation == AnimationIndex.AntlerClimb)
		{
			bodyMode = BodyModeIndex.Default;
			canJump = 5;
		}
		else if (animation == AnimationIndex.GrapplingSwing)
		{
			bodyMode = BodyModeIndex.Default;
			standing = false;
			base.mainBodyChunk.vel -= Custom.PerpendicularVector(Custom.DirVec(base.mainBodyChunk.pos, this.tubeWorm.tongues[0].AttachedPos)) * input[0].x * 0.25f;
		}
		else if (animation == AnimationIndex.ZeroGSwim)
		{
			dynamicRunSpeed[0] = 0f;
			dynamicRunSpeed[1] = 0f;
			bodyMode = BodyModeIndex.ZeroG;
			standing = false;
			circuitSwimResistance *= Mathf.InverseLerp(base.mainBodyChunk.vel.magnitude + base.bodyChunks[1].vel.magnitude, 15f, 9f);
			for (int num13 = 0; num13 < 2; num13++)
			{
				if (swimBits[num13] != null && !Custom.DistLess(base.mainBodyChunk.pos, swimBits[num13].pos, 50f))
				{
					swimBits[num13] = null;
				}
				base.bodyChunks[num13].vel *= Mathf.Lerp(1f, 0.9f, circuitSwimResistance);
				if (base.bodyChunks[num13].ContactPoint.x != 0 || base.bodyChunks[num13].ContactPoint.y != 0)
				{
					canJump = 12;
					if (base.bodyChunks[num13].lastContactPoint.x != base.bodyChunks[num13].ContactPoint.x || base.bodyChunks[num13].lastContactPoint.y != base.bodyChunks[num13].ContactPoint.y)
					{
						Blink(5);
						room.PlaySound(SoundID.Slugcat_Regain_Footing, base.mainBodyChunk);
					}
				}
			}
			bool flag5 = canJump > 0;
			if (!flag5 && (room.GetTile(base.mainBodyChunk.pos).verticalBeam || room.GetTile(base.mainBodyChunk.pos).horizontalBeam))
			{
				flag5 = true;
			}
			for (int num14 = 0; num14 < 9; num14++)
			{
				if (flag5)
				{
					break;
				}
				if (room.GetTile(base.mainBodyChunk.pos + Custom.eightDirectionsAndZero[num14].ToVector2() * 10f).Solid)
				{
					flag5 = true;
				}
			}
			swimCycle += 4f / Custom.LerpMap(base.mainBodyChunk.vel.magnitude, 0f, 2f, 120f, 60f);
			if (input[0].ZeroGGamePadIntVec.x != 0 || input[0].ZeroGGamePadIntVec.y != 0)
			{
				swimCycle += 1f / Mathf.Lerp(2f, 6f, UnityEngine.Random.value);
				Vector2 vector5 = SwimDir(normalize: false);
				base.mainBodyChunk.vel += vector5 * circuitSwimResistance * 0.5f;
				if (flag5)
				{
					base.mainBodyChunk.vel += vector5 * 0.2f;
				}
				else
				{
					base.mainBodyChunk.vel += vector5 * Custom.LerpMap(Vector2.Distance(base.mainBodyChunk.vel, base.bodyChunks[1].vel), 1f, 4f, 0.1f, Custom.LerpMap((base.mainBodyChunk.vel + base.bodyChunks[1].vel).magnitude, 4f, 8f, 0.15f, 0.1f));
				}
				base.bodyChunks[1].vel -= vector5 * 0.1f;
				for (int num15 = 0; num15 < 5; num15++)
				{
					if (room.GetTile(base.mainBodyChunk.pos + Custom.fourDirectionsAndZero[0].ToVector2() * 15f).AnyBeam)
					{
						base.mainBodyChunk.vel *= 0.8f;
						base.mainBodyChunk.vel += vector5 * 0.2f;
						break;
					}
				}
				if (canJump > 0 && wantToJump > 0)
				{
					Vector2 a = new Vector2(0f, 0f);
					int num16 = 1;
					while (num16 >= 0 && a.x == 0f && a.y == 0f)
					{
						IntVector2 tilePosition2 = room.GetTilePosition(base.bodyChunks[num16].pos);
						for (int num17 = 0; num17 < 8; num17++)
						{
							if (room.GetTile(tilePosition2 - Custom.eightDirectionsDiagonalsLast[num17]).Solid && ((a.x == 0f && a.y == 0f) || Vector2.Distance(vector5, Custom.eightDirectionsDiagonalsLast[num17].ToVector2()) < Vector2.Distance(a, vector5)))
							{
								a = Custom.eightDirectionsDiagonalsLast[num17].ToVector2();
							}
						}
						num16--;
					}
					if (a.x != 0f || a.y != 0f)
					{
						a = a.normalized;
						Vector2 a2 = Vector2.Lerp(vector5, a, 0.5f);
						a2 = Vector2.Lerp(a2, Custom.DirVec(base.bodyChunks[1].pos, base.mainBodyChunk.pos), 0.25f);
						base.mainBodyChunk.vel = Vector2.ClampMagnitude(base.mainBodyChunk.vel + a2 * 5.4f, 5.4f);
						base.bodyChunks[1].vel = Vector2.ClampMagnitude(base.bodyChunks[1].vel + a2 * 5f, 5f);
						base.mainBodyChunk.vel += a;
						room.PlaySound(SoundID.Slugcat_Normal_Jump, base.mainBodyChunk);
						canJump = 0;
						wantToJump = 0;
					}
				}
				else if (wantToJump > 0 && curcuitJumpMeter >= 3f)
				{
					Vector2 vector6 = Vector2.Lerp(vector5, Custom.DirVec(base.bodyChunks[1].pos, base.mainBodyChunk.pos), 0.5f);
					base.mainBodyChunk.vel += vector6 * 4.4f * (0.5f + 0.5f * circuitSwimResistance);
					base.bodyChunks[1].vel += vector6 * 4f * (0.5f + 0.5f * circuitSwimResistance);
					room.PlaySound(SoundID.Slugcat_Normal_Jump, base.mainBodyChunk);
					canJump = 0;
					wantToJump = 0;
					curcuitJumpMeter = -1f;
				}
				else if ((input[0].ZeroGGamePadIntVec.x != 0 || (input[0].ZeroGGamePadIntVec.y != 0 && Mathf.Sign(input[0].ZeroGGamePadIntVec.y) != Mathf.Sign(base.mainBodyChunk.vel.y))) && room.GetTile(base.mainBodyChunk.pos).horizontalBeam && (input[1].ZeroGGamePadIntVec.x == 0 || !room.GetTile(base.mainBodyChunk.lastPos).horizontalBeam))
				{
					room.PlaySound(SoundID.Slugcat_Grab_Beam, base.mainBodyChunk);
					animation = AnimationIndex.ZeroGPoleGrab;
					standing = false;
				}
				else if ((input[0].ZeroGGamePadIntVec.y != 0 || (input[0].ZeroGGamePadIntVec.x != 0 && Mathf.Sign(input[0].ZeroGGamePadIntVec.x) != Mathf.Sign(base.mainBodyChunk.vel.x))) && room.GetTile(base.mainBodyChunk.pos).verticalBeam && (input[1].ZeroGGamePadIntVec.y == 0 || !room.GetTile(base.mainBodyChunk.lastPos).verticalBeam))
				{
					room.PlaySound(SoundID.Slugcat_Grab_Beam, base.mainBodyChunk);
					animation = AnimationIndex.ZeroGPoleGrab;
					standing = true;
				}
			}
			if (swimCycle > 4f)
			{
				swimCycle = 0f;
			}
			circuitSwimResistance = 0f;
			if (curcuitJumpMeter >= 0f)
			{
				curcuitJumpMeter = Mathf.Clamp(curcuitJumpMeter - 0.5f, 0f, 4f);
			}
			else
			{
				curcuitJumpMeter = Mathf.Min(curcuitJumpMeter + 0.5f, 0f);
			}
		}
		else if (animation == AnimationIndex.ZeroGPoleGrab)
		{
			dynamicRunSpeed[0] = 0f;
			dynamicRunSpeed[1] = 0f;
			bodyMode = BodyModeIndex.ZeroG;
			base.mainBodyChunk.vel *= Custom.LerpMap(base.mainBodyChunk.vel.magnitude, 2f, 5f, 0.7f, 0.3f);
			bool flag6 = false;
			if (input[0].ZeroGGamePadIntVec.x != 0 || input[0].ZeroGGamePadIntVec.y != 0)
			{
				if (input[0].ZeroGGamePadIntVec.x != 0)
				{
					zeroGPoleGrabDir.x = input[0].ZeroGGamePadIntVec.x;
				}
				if (input[0].ZeroGGamePadIntVec.y != 0)
				{
					zeroGPoleGrabDir.y = input[0].ZeroGGamePadIntVec.y;
				}
			}
			bool flag7 = true;
			if (!room.GetTile(base.mainBodyChunk.pos).horizontalBeam && !room.GetTile(base.mainBodyChunk.pos).verticalBeam)
			{
				standing = false;
				animation = AnimationIndex.ZeroGSwim;
				flag7 = false;
			}
			else if (!room.GetTile(base.mainBodyChunk.pos).horizontalBeam && room.GetTile(base.mainBodyChunk.pos).verticalBeam)
			{
				standing = true;
			}
			else if (room.GetTile(base.mainBodyChunk.pos).horizontalBeam && !room.GetTile(base.mainBodyChunk.pos).verticalBeam)
			{
				standing = false;
			}
			else if (input[0].ZeroGGamePadIntVec.x != 0 && input[0].ZeroGGamePadIntVec.y == 0)
			{
				standing = false;
			}
			else if (input[0].ZeroGGamePadIntVec.x == 0 && input[0].ZeroGGamePadIntVec.y != 0)
			{
				standing = true;
			}
			if (!flag7)
			{
				return;
			}
			if (standing)
			{
				if (room.readyForAI && room.aimap.getAItile(base.mainBodyChunk.pos + new Vector2(0f, (float)input[0].ZeroGGamePadIntVec.y * 20f)).narrowSpace)
				{
					base.mainBodyChunk.vel.x += (room.MiddleOfTile(base.mainBodyChunk.pos).x - base.mainBodyChunk.pos.x) * 0.1f;
				}
				else
				{
					base.mainBodyChunk.vel.x += (room.MiddleOfTile(base.mainBodyChunk.pos).x + 5f * (float)zeroGPoleGrabDir.x - base.mainBodyChunk.pos.x) * 0.1f;
				}
				if (input[0].ZeroGGamePadIntVec.y != 0)
				{
					if (room.GetTile(base.mainBodyChunk.pos + new Vector2(0f, (float)input[0].ZeroGGamePadIntVec.y * 10f)).verticalBeam)
					{
						base.mainBodyChunk.vel.y += (float)input[0].ZeroGGamePadIntVec.y * 1.05f * slugcatStats.poleClimbSpeedFac;
						animationFrame++;
						if (animationFrame > 20)
						{
							animationFrame = 0;
							room.PlaySound(SoundID.Slugcat_Climb_Up_Vertical_Beam, base.mainBodyChunk, loop: false, 1f, 1f);
						}
					}
					else if (input[0].ZeroGGamePadIntVec.x != 0 || input[0].ZeroGGamePadIntVec.y != 0)
					{
						flag6 = true;
					}
				}
				if (!flag6 && room.GetTile(base.bodyChunks[1].pos).verticalBeam)
				{
					base.bodyChunks[1].vel *= 0.7f;
					base.bodyChunks[1].vel.x += (room.MiddleOfTile(base.bodyChunks[1].pos).x - 5f * (float)zeroGPoleGrabDir.x - base.bodyChunks[1].pos.x) * 0.1f;
				}
			}
			else
			{
				if (room.readyForAI && room.aimap.getAItile(base.mainBodyChunk.pos + new Vector2((float)input[0].ZeroGGamePadIntVec.x * 20f, 0f)).narrowSpace)
				{
					base.mainBodyChunk.vel.y += (room.MiddleOfTile(base.mainBodyChunk.pos).y - base.mainBodyChunk.pos.y) * 0.1f;
				}
				else
				{
					base.mainBodyChunk.vel.y += (room.MiddleOfTile(base.mainBodyChunk.pos).y + 5f * (float)zeroGPoleGrabDir.y - base.mainBodyChunk.pos.y) * 0.1f;
				}
				if (input[0].ZeroGGamePadIntVec.x != 0)
				{
					if (room.GetTile(base.mainBodyChunk.pos + new Vector2((float)input[0].ZeroGGamePadIntVec.x * 10f, 0f)).horizontalBeam)
					{
						base.mainBodyChunk.vel.x += (float)input[0].ZeroGGamePadIntVec.x * 1.05f * slugcatStats.poleClimbSpeedFac;
						animationFrame++;
						if (animationFrame > 20)
						{
							animationFrame = 0;
							room.PlaySound(SoundID.Slugcat_Climb_Up_Vertical_Beam, base.mainBodyChunk, loop: false, 1f, 1f);
						}
					}
					else if (input[0].ZeroGGamePadIntVec.x != 0 || input[0].ZeroGGamePadIntVec.y != 0)
					{
						flag6 = true;
					}
				}
				if (!flag6 && room.GetTile(base.bodyChunks[1].pos).horizontalBeam)
				{
					base.bodyChunks[1].vel *= 0.7f;
					base.bodyChunks[1].vel.y += (room.MiddleOfTile(base.bodyChunks[1].pos).y - 5f * (float)zeroGPoleGrabDir.y - base.bodyChunks[1].pos.y) * 0.1f;
				}
			}
			if (input[0].jmp && !input[1].jmp)
			{
				if (input[0].ZeroGGamePadIntVec.x != 0 || input[0].ZeroGGamePadIntVec.y != 0)
				{
					Vector2 vector7 = SwimDir(normalize: true);
					if (!flag6 && (!room.GetTile(base.mainBodyChunk.pos).horizontalBeam || !room.GetTile(base.mainBodyChunk.pos).verticalBeam))
					{
						if (standing && (float)input[0].ZeroGGamePadIntVec.x == 0f)
						{
							vector7.y *= 0.1f;
						}
						else if (!standing && (float)input[0].ZeroGGamePadIntVec.y == 0f)
						{
							vector7.x *= 0.1f;
						}
					}
					base.mainBodyChunk.vel = Vector2.ClampMagnitude(base.mainBodyChunk.vel + vector7 * 5.4f, 5.4f);
					base.bodyChunks[1].vel = Vector2.ClampMagnitude(base.bodyChunks[1].vel + vector7 * 5f, 5f);
					room.PlaySound(SoundID.Slugcat_From_Horizontal_Pole_Jump, base.mainBodyChunk);
				}
				else
				{
					room.PlaySound(SoundID.Slugcat_Climb_Along_Horizontal_Beam, base.mainBodyChunk);
				}
				standing = false;
				animation = AnimationIndex.ZeroGSwim;
			}
			if (room.readyForAI && room.aimap.getAItile(base.mainBodyChunk.pos).narrowSpace && room.aimap.getAItile(base.mainBodyChunk.pos + new Vector2((float)input[0].ZeroGGamePadIntVec.x * 20f, (float)input[0].ZeroGGamePadIntVec.y * 20f)).narrowSpace)
			{
				bodyMode = BodyModeIndex.CorridorClimb;
				animation = AnimationIndex.None;
			}
		}
		else if (animation == AnimationIndex.VineGrab)
		{
			dynamicRunSpeed[0] = 0f;
			dynamicRunSpeed[1] = 0f;
			bodyMode = BodyModeIndex.Default;
			Vector2 vector8 = SwimDir(normalize: true);
			room.climbableVines.VineBeingClimbedOn(vinePos, this);
			if (vector8.magnitude > 0f)
			{
				vineClimbCursor = Vector2.ClampMagnitude(vineClimbCursor + vector8 * Custom.LerpMap(Vector2.Dot(vector8, vineClimbCursor.normalized), -1f, 1f, 10f, 3f), 30f);
				Vector2 vector9 = room.climbableVines.OnVinePos(vinePos);
				vinePos.floatPos += room.climbableVines.ClimbOnVineSpeed(vinePos, base.mainBodyChunk.pos + vineClimbCursor) * Mathf.Lerp(2.1f, 1.5f, EffectiveRoomGravity) / room.climbableVines.TotalLength(vinePos.vine);
				vinePos.floatPos = Mathf.Clamp(vinePos.floatPos, 0f, 1f);
				room.climbableVines.PushAtVine(vinePos, (vector9 - room.climbableVines.OnVinePos(vinePos)) * 0.05f);
				IClimbableVine vineObject = room.climbableVines.GetVineObject(vinePos);
				if (vineObject != null && vineObject is ClimbableVine && UnityEngine.Random.value < 0.05f)
				{
					room.PlaySound(SoundID.Leaves, base.mainBodyChunk.pos, 0.5f, 0.5f + UnityEngine.Random.value * 1.5f);
				}
				if (vineGrabDelay == 0 && (!ModManager.MMF || !GrabbedByDaddyCorruption))
				{
					ClimbableVinesSystem.VinePosition vinePosition = room.climbableVines.VineSwitch(vinePos, base.mainBodyChunk.pos + vineClimbCursor, base.mainBodyChunk.rad);
					if (vinePosition != null)
					{
						vinePos = vinePosition;
						vineGrabDelay = 10;
					}
				}
				animationFrame++;
				if (animationFrame > 30)
				{
					animationFrame = 0;
				}
			}
			else
			{
				vineClimbCursor *= 0.8f;
			}
			base.mainBodyChunk.vel += vineClimbCursor / 190f;
			base.bodyChunks[1].vel -= vineClimbCursor / 190f;
			Vector2 p2 = room.climbableVines.OnVinePos(vinePos);
			if (input[0].ZeroGGamePadIntVec.x != 0)
			{
				zeroGPoleGrabDir.x = input[0].ZeroGGamePadIntVec.x;
			}
			if (input[0].ZeroGGamePadIntVec.y != 0)
			{
				zeroGPoleGrabDir.y = input[0].ZeroGGamePadIntVec.y;
			}
			bool flag8 = false;
			if (input[0].jmp && !input[1].jmp)
			{
				bool flag9 = false;
				Grasp[] array = base.grasps;
				foreach (Grasp grasp in array)
				{
					if (grasp == null || !(grasp.grabbed is TubeWorm))
					{
						continue;
					}
					TubeWorm tubeWorm = grasp.grabbed as TubeWorm;
					for (int num19 = 0; num19 < 2; num19++)
					{
						if (tubeWorm.tongues[num19].Attached)
						{
							flag9 = true;
							break;
						}
					}
				}
				if (!ModManager.MMF || !flag9)
				{
					flag8 = true;
					if (vector8.magnitude > 0f)
					{
						base.mainBodyChunk.vel = base.mainBodyChunk.vel + vector8.normalized * 4f;
						base.bodyChunks[1].vel = base.bodyChunks[1].vel + vector8.normalized * 3.5f;
						base.mainBodyChunk.vel = Vector2.Lerp(base.mainBodyChunk.vel, Vector2.ClampMagnitude(base.mainBodyChunk.vel, 5f), 0.5f);
						base.bodyChunks[1].vel = Vector2.Lerp(base.bodyChunks[1].vel, Vector2.ClampMagnitude(base.bodyChunks[1].vel, 5f), 0.5f);
						room.climbableVines.PushAtVine(vinePos, -vector8.normalized * 15f);
						vineGrabDelay = 10;
					}
				}
			}
			else if (!room.climbableVines.VineCurrentlyClimbable(vinePos))
			{
				flag8 = true;
				vineGrabDelay = 10;
			}
			if (!flag8 && Custom.DistLess(base.mainBodyChunk.pos, p2, 40f + room.climbableVines.VineRad(vinePos)))
			{
				room.climbableVines.ConnectChunkToVine(base.mainBodyChunk, vinePos, room.climbableVines.VineRad(vinePos));
				Vector2 vector10 = Custom.PerpendicularVector(room.climbableVines.VineDir(vinePos));
				base.bodyChunks[0].vel += vector10 * 0.2f * ((Mathf.Abs(vector10.x) > Mathf.Abs(vector10.y)) ? zeroGPoleGrabDir.x : zeroGPoleGrabDir.y);
				if (EffectiveRoomGravity == 0f)
				{
					Vector2 vector11 = room.climbableVines.OnVinePos(new ClimbableVinesSystem.VinePosition(vinePos.vine, vinePos.floatPos - 20f / room.climbableVines.TotalLength(vinePos.vine)));
					Vector2 vector12 = room.climbableVines.OnVinePos(new ClimbableVinesSystem.VinePosition(vinePos.vine, vinePos.floatPos + 20f / room.climbableVines.TotalLength(vinePos.vine)));
					if (Vector2.Distance(base.bodyChunks[1].pos, vector11) < Vector2.Distance(base.bodyChunks[1].pos, vector12))
					{
						base.bodyChunks[0].vel -= Vector2.ClampMagnitude(vector11 - base.bodyChunks[1].pos, 5f) / 20f;
						base.bodyChunks[1].vel += Vector2.ClampMagnitude(vector11 - base.bodyChunks[1].pos, 5f) / 20f;
					}
					else
					{
						base.bodyChunks[0].vel -= Vector2.ClampMagnitude(vector12 - base.bodyChunks[1].pos, 5f) / 20f;
						base.bodyChunks[1].vel += Vector2.ClampMagnitude(vector12 - base.bodyChunks[1].pos, 5f) / 20f;
					}
				}
			}
			else
			{
				animation = AnimationIndex.None;
			}
		}
		else if (animation == AnimationIndex.Dead)
		{
			bodyMode = BodyModeIndex.Dead;
		}
	}

	public void CollideWithCoralCircuitBit(int chunk, CoralCircuit.CircuitBit bit, float overLapFac)
	{
		circuitSwimResistance = Mathf.Max(circuitSwimResistance, overLapFac);
		Vector2 a = input[0].analogueDir;
		if (a.x == 0f && a.y == 0f)
		{
			a = Custom.DirVec(new Vector2(0f, 0f), new Vector2(input[0].x, input[0].y));
		}
		if (curcuitJumpMeter < 0f)
		{
			bit.circuit.Explosion(base.bodyChunks[1].pos, 40f, 12f, Vector2.Lerp(a, Custom.DirVec(base.bodyChunks[1].pos, base.bodyChunks[0].pos), 0.5f) * -6f + Custom.DirVec(base.bodyChunks[0].pos, bit.pos) * 6f);
		}
		else
		{
			bit.vel -= Vector2.Lerp(a, Custom.DirVec(base.bodyChunks[1].pos, base.bodyChunks[0].pos), 0.3f) * 3f * Mathf.InverseLerp(1.5f, 2.5f, swimCycle);
		}
		if (chunk == 0 && base.graphicsModule != null && swimBits[0] != bit && swimBits[1] != bit)
		{
			int num = ((!(Custom.DistanceToLine(bit.pos, base.bodyChunks[0].pos, base.bodyChunks[1].pos) < 0f)) ? 1 : 0);
			if (swimBits[num] == null || Vector2.Distance(bit.pos, base.mainBodyChunk.pos) < Vector2.Distance(swimBits[num].pos, base.mainBodyChunk.pos))
			{
				room.PlaySound(SoundID.Player_Coral_Circuit_Swim, base.mainBodyChunk);
				swimBits[num] = bit;
			}
		}
		curcuitJumpMeter += 1f;
	}

	public Vector2 SwimDir(bool normalize)
	{
		Vector2 result = input[0].analogueDir;
		if (result.x == 0f && result.y == 0f)
		{
			if (input[0].ZeroGGamePadIntVec.x != 0 || input[0].ZeroGGamePadIntVec.y != 0)
			{
				return input[0].IntVec.ToVector2().normalized;
			}
			return new Vector2(0f, 0f);
		}
		if (normalize)
		{
			result = result.normalized;
		}
		return result;
	}

	public void UpdateBodyMode()
	{
		diveForce = Mathf.Max(0f, diveForce - 0.05f);
		waterRetardationImmunity = Mathf.InverseLerp(0f, 0.3f, diveForce) * 0.85f;
		if (dropGrabTile.HasValue && bodyMode != BodyModeIndex.Default && bodyMode != BodyModeIndex.CorridorClimb)
		{
			dropGrabTile = null;
		}
		if (base.bodyChunks[0].ContactPoint.y < 0)
		{
			upperBodyFramesOnGround++;
			upperBodyFramesOffGround = 0;
		}
		else
		{
			upperBodyFramesOnGround = 0;
			upperBodyFramesOffGround++;
		}
		if (base.bodyChunks[1].ContactPoint.y < 0)
		{
			lowerBodyFramesOnGround++;
			lowerBodyFramesOffGround = 0;
		}
		else
		{
			lowerBodyFramesOnGround = 0;
			lowerBodyFramesOffGround++;
		}
		if (bodyMode == BodyModeIndex.Default)
		{
			if (input[0].y < 0 && (animation != AnimationIndex.Roll || input[0].x == 0))
			{
				base.GoThroughFloors = true;
				if (input[0].downDiagonal != 0 && consistentDownDiagonal > 6 && base.bodyChunks[0].ContactPoint.x == 0 && base.bodyChunks[0].ContactPoint.y == 0 && base.bodyChunks[1].ContactPoint.x == 0 && base.bodyChunks[1].ContactPoint.y == 0)
				{
					IntVector2 tilePosition = room.GetTilePosition((base.mainBodyChunk.pos.y < base.bodyChunks[1].pos.y) ? base.mainBodyChunk.pos : base.bodyChunks[1].pos);
					for (int i = 0; i < 5; i++)
					{
						tilePosition += new IntVector2(0, -1);
						if (room.GetTile(tilePosition).Terrain == Room.Tile.TerrainType.Solid)
						{
							break;
						}
						if (room.GetTile(tilePosition).Terrain == Room.Tile.TerrainType.Floor)
						{
							if (room.GetTile(tilePosition + new IntVector2(input[0].x, 0)).Terrain == Room.Tile.TerrainType.Solid && !room.GetTile(tilePosition + new IntVector2(input[0].x, 1)).Solid)
							{
								base.GoThroughFloors = false;
							}
							break;
						}
					}
				}
			}
			if (standing)
			{
				base.bodyChunks[0].vel.y += 4f * EffectiveRoomGravity;
				base.bodyChunks[1].vel.y -= 4f * EffectiveRoomGravity;
				dynamicRunSpeed[0] = 4.2f * slugcatStats.runspeedFac;
				dynamicRunSpeed[1] = 4f * slugcatStats.runspeedFac;
				if (input[0].y != 0)
				{
					dynamicRunSpeed[1] = 2f;
				}
				if (input[0].y > 0 && !IsTileSolid(0, 0, 1) && IsTileSolid(0, -1, 1) && IsTileSolid(0, 1, 1))
				{
					base.bodyChunks[0].vel.x = base.bodyChunks[0].vel.x * 0.8f - (base.bodyChunks[0].pos.x - room.MiddleOfTile(room.GetTilePosition(base.bodyChunks[0].pos)).x) * 0.4f;
				}
			}
			else
			{
				dynamicRunSpeed[0] = 4f;
				if (input[0].y != 0)
				{
					dynamicRunSpeed[0] = 2.5f;
				}
				if (animation == AnimationIndex.CrawlTurn)
				{
					dynamicRunSpeed[0] *= 0.75f;
				}
				dynamicRunSpeed[1] = dynamicRunSpeed[0];
			}
			if (input[0].y < 0 && base.mainBodyChunk.ContactPoint.y == 0 && base.mainBodyChunk.ContactPoint.x == 0 && base.bodyChunks[1].ContactPoint.y == 0 && base.bodyChunks[1].ContactPoint.x == 0 && base.mainBodyChunk.vel.y < -6f)
			{
				diveForce = Mathf.Min(1f, diveForce + 1f / 7f);
				base.mainBodyChunk.vel.y -= 1.2f * diveForce;
				base.bodyChunks[1].vel.y += 1.2f * diveForce;
			}
			if (!dropGrabTile.HasValue)
			{
				return;
			}
			if ((room.GetTilePosition(base.mainBodyChunk.pos) == dropGrabTile.Value || room.GetTilePosition(base.bodyChunks[1].pos) == dropGrabTile.Value) && !input[0].jmp && base.Consious)
			{
				base.mainBodyChunk.pos = room.MiddleOfTile(dropGrabTile.Value);
				if (room.GetTile(dropGrabTile.Value).verticalBeam)
				{
					animation = AnimationIndex.ClimbOnBeam;
				}
				else if (room.GetTile(dropGrabTile.Value).horizontalBeam)
				{
					animation = AnimationIndex.HangFromBeam;
				}
				if (base.bodyChunks[1].pos.y > base.mainBodyChunk.pos.y)
				{
					base.bodyChunks[1].vel.x += ((base.bodyChunks[1].pos.x < base.mainBodyChunk.pos.x) ? (-2f) : 2f);
					base.bodyChunks[1].pos.x += ((base.bodyChunks[1].pos.x < base.mainBodyChunk.pos.x) ? (-2f) : 2f);
				}
				dropGrabTile = null;
			}
			else if (room.GetTilePosition(base.mainBodyChunk.pos).y > dropGrabTile.Value.y + 2 || room.GetTilePosition(base.mainBodyChunk.pos).y < dropGrabTile.Value.y || room.GetTilePosition(base.bodyChunks[1].pos).y < dropGrabTile.Value.y || (room.GetTilePosition(base.mainBodyChunk.pos).x != dropGrabTile.Value.x && room.GetTilePosition(base.bodyChunks[1].pos).x != dropGrabTile.Value.x))
			{
				dropGrabTile = null;
			}
		}
		else if (bodyMode == BodyModeIndex.Crawl)
		{
			dynamicRunSpeed[0] = 2.5f;
			if (input[0].y != 0)
			{
				dynamicRunSpeed[0] = 1f;
			}
			if (input[0].x > 0 == base.bodyChunks[0].pos.x < base.bodyChunks[1].pos.x && base.bodyChunks[0].onSlope == 0 && base.bodyChunks[1].onSlope == 0)
			{
				dynamicRunSpeed[0] *= 0.75f;
				if (crawlTurnDelay > 5 && !IsTileSolid(0, 0, 1) && !IsTileSolid(1, 0, 1) && input[0].x != 0)
				{
					crawlTurnDelay = 0;
					if (!IsTileSolid(0, -1, 1) || !IsTileSolid(0, 1, 1))
					{
						animation = AnimationIndex.CrawlTurn;
					}
				}
			}
			dynamicRunSpeed[1] = dynamicRunSpeed[0];
			if (base.bodyChunks[0].onSlope != 0 || base.bodyChunks[1].onSlope != 0)
			{
				if (input[0].x > 0 == base.bodyChunks[0].pos.x < base.bodyChunks[1].pos.x)
				{
					dynamicRunSpeed[0] *= 0.5f;
				}
				else
				{
					dynamicRunSpeed[0] *= 0.75f;
				}
				if (base.bodyChunks[0].onSlope != 0)
				{
					base.bodyChunks[0].vel.y -= 1.5f;
				}
				if (base.bodyChunks[1].onSlope != 0)
				{
					base.bodyChunks[1].vel.y -= 1.5f;
				}
			}
			if (base.bodyChunks[0].ContactPoint.y > -1 && input[0].x != 0 && base.bodyChunks[1].pos.y < base.bodyChunks[0].pos.y - 3f && base.bodyChunks[1].ContactPoint.x == input[0].x)
			{
				base.bodyChunks[1].pos.y += 1f;
			}
			if (input[0].y < 0)
			{
				base.GoThroughFloors = true;
				for (int j = 0; j < 2; j++)
				{
					if (!IsTileSolid(j, 0, -1) && (IsTileSolid(j, -1, -1) || IsTileSolid(j, 1, -1)))
					{
						base.bodyChunks[j].vel.x = base.bodyChunks[j].vel.x * 0.8f - (base.bodyChunks[j].pos.x - room.MiddleOfTile(room.GetTilePosition(base.bodyChunks[j].pos)).x) * 0.4f;
						base.bodyChunks[j].vel.y -= 1f;
						break;
					}
				}
			}
			if (standing && (lowerBodyFramesOnGround >= 3 || (base.bodyChunks[1].ContactPoint.y < 0 && room.GetTile(room.GetTilePosition(base.bodyChunks[1].pos) + new IntVector2(0, -1)).Terrain != 0 && room.GetTile(room.GetTilePosition(base.bodyChunks[0].pos) + new IntVector2(0, -1)).Terrain != 0)))
			{
				room.PlaySound(SoundID.Slugcat_Stand_Up, base.mainBodyChunk);
				animation = AnimationIndex.StandUp;
				if (input[0].x == 0)
				{
					if (base.bodyChunks[1].ContactPoint.y == -1 && IsTileSolid(1, 0, -1) && !IsTileSolid(1, 0, 1))
					{
						feetStuckPos = room.MiddleOfTile(room.GetTilePosition(base.bodyChunks[1].pos)) + new Vector2(0f, -10f + base.bodyChunks[1].rad);
					}
					else if (base.bodyChunks[0].ContactPoint.y == -1 && IsTileSolid(0, 0, -1) && !IsTileSolid(0, 0, 1))
					{
						feetStuckPos = base.bodyChunks[0].pos + new Vector2(0f, -1f);
					}
				}
			}
			if (base.bodyChunks[1].onSlope != 0 && base.bodyChunks[1].ContactPoint.y > -1 && input[0].x == 0)
			{
				base.bodyChunks[0].vel.x += base.bodyChunks[1].onSlope;
			}
			if (input[0].x != 0 && Mathf.Abs(base.bodyChunks[1].pos.x - base.bodyChunks[1].lastPos.x) > 0.5f)
			{
				animationFrame++;
			}
			else
			{
				animationFrame = 0;
			}
			if (animationFrame > 10)
			{
				animationFrame = 0;
				room.PlaySound(SoundID.Slugcat_Crawling_Step, base.mainBodyChunk);
			}
		}
		else if (bodyMode == BodyModeIndex.Stand)
		{
			base.bodyChunks[0].vel.y += 1.5f * EffectiveRoomGravity;
			base.bodyChunks[1].vel.y -= 4.5f * EffectiveRoomGravity;
			if (!standing && lowerBodyFramesOnGround >= 5 && upperBodyFramesOffGround >= 5)
			{
				bool flag = true;
				if (room.GetTilePosition(base.mainBodyChunk.pos).y == room.GetTilePosition(base.bodyChunks[1].pos).y + 1 && IsTileSolid(1, flipDirection, 0) && IsTileSolid(0, flipDirection, -1))
				{
					bool flag2 = room.GetTile(room.GetTilePosition(base.bodyChunks[1].pos)).Terrain != Room.Tile.TerrainType.Slope && room.GetTile(room.GetTilePosition(base.bodyChunks[1].pos) + new IntVector2(0, -1)).Terrain != Room.Tile.TerrainType.Slope && room.GetTile(room.GetTilePosition(base.bodyChunks[1].pos) + new IntVector2(flipDirection, 0)).Terrain != Room.Tile.TerrainType.Slope;
					if (!flag2)
					{
						flag2 = IsTileSolid(0, flipDirection, 0) && IsTileSolid(1, flipDirection, 0);
					}
					if (flag2)
					{
						flag = false;
						if (room.GetTile(room.GetTilePosition(base.bodyChunks[1].pos) + new IntVector2(-flipDirection, 0)).Terrain == Room.Tile.TerrainType.Air)
						{
							if (room.GetTile(room.GetTilePosition(base.bodyChunks[1].pos) + new IntVector2(-flipDirection, -1)).Terrain != 0)
							{
								base.bodyChunks[1].vel.x += -1.1f * (float)flipDirection;
							}
							else if (input[0].x != flipDirection)
							{
								if (IsTileSolid(0, flipDirection, 1))
								{
									flipDirection *= -1;
								}
								else if (input[0].y < 0 && input[1].y < 0 && input[2].y < 0 && input[3].y < 0)
								{
									base.bodyChunks[1].vel.x += -0.4f * (float)flipDirection;
									base.bodyChunks[0].vel.y -= 1f;
								}
							}
						}
					}
				}
				if (flag)
				{
					animation = AnimationIndex.DownOnFours;
				}
			}
			dynamicRunSpeed[0] = 4.2f * slugcatStats.runspeedFac;
			dynamicRunSpeed[1] = 4f * slugcatStats.runspeedFac;
			if (input[0].y != 0)
			{
				dynamicRunSpeed[1] = 2f;
			}
			if (base.bodyChunks[1].onSlope != 0)
			{
				dynamicRunSpeed[0] *= 0.75f;
				dynamicRunSpeed[1] *= 0.75f;
				if (base.bodyChunks[0].ContactPoint.y == 1)
				{
					standing = false;
				}
				if (input[0].x == base.bodyChunks[1].onSlope)
				{
					base.bodyChunks[1].vel.y -= base.gravity;
				}
				base.bodyChunks[1].pos.y -= 2f;
			}
			if (input[0].x != 0)
			{
				if (base.bodyChunks[1].ContactPoint.x == input[0].x)
				{
					base.bodyChunks[0].vel.x += input[0].x;
					base.bodyChunks[0].vel.y += 2f * EffectiveRoomGravity;
					base.bodyChunks[1].vel.y += 2f * EffectiveRoomGravity;
				}
				animationFrame++;
			}
			else
			{
				animationFrame = 0;
			}
			if (animationFrame > 6)
			{
				animationFrame = 0;
				room.PlaySound(leftFoot ? SoundID.Slugcat_Step_A : SoundID.Slugcat_Step_B, base.mainBodyChunk, loop: false, 1f, 1f);
				leftFoot = !leftFoot;
				if (aerobicLevel < 0.7f)
				{
					AerobicIncrease(0.05f);
				}
				room.InGameNoise(new InGameNoise(base.bodyChunks[1].pos, 150f, this, 1f));
			}
			if (slideCounter > 0)
			{
				slideCounter++;
				if (slideCounter > 20 || input[0].x != -slideDirection)
				{
					slideCounter = 0;
				}
				float num = 0f - Mathf.Sin((float)slideCounter / 20f * (float)Math.PI * 0.5f) + 0.5f;
				base.mainBodyChunk.vel.x += num * 3.5f * (float)slideDirection - (float)slideDirection * ((num < 0f) ? 0.8f : 0.5f) * (isSlugpup ? 0.25f : 1f);
				base.bodyChunks[1].vel.x += num * 3.5f * (float)slideDirection + (float)slideDirection * 0.5f;
				if ((slideCounter == 4 || slideCounter == 7 || slideCounter == 11) && UnityEngine.Random.value < Mathf.InverseLerp(0f, 0.5f, room.roomSettings.CeilingDrips))
				{
					room.AddObject(new WaterDrip(base.bodyChunks[1].pos + new Vector2(0f, 0f - base.bodyChunks[1].rad + 1f), Custom.DegToVec((float)slideDirection * Mathf.Lerp(30f, 70f, UnityEngine.Random.value)) * Mathf.Lerp(6f, 11f, UnityEngine.Random.value), waterColor: false));
				}
			}
			else if (input[0].x != 0)
			{
				if (input[0].x == slideDirection)
				{
					if (initSlideCounter < 30)
					{
						initSlideCounter++;
					}
					return;
				}
				if (initSlideCounter > (isRivulet ? 5 : 10) && base.mainBodyChunk.vel.x > 0f == slideDirection > 0 && Mathf.Abs(base.mainBodyChunk.vel.x) > 1f)
				{
					slideCounter = 1;
					room.PlaySound(SoundID.Slugcat_Skid_On_Ground_Init, base.mainBodyChunk, loop: false, 1f, 1f);
				}
				else
				{
					slideDirection = input[0].x;
				}
				initSlideCounter = 0;
			}
			else if (initSlideCounter > 0)
			{
				initSlideCounter--;
			}
		}
		else if (bodyMode == BodyModeIndex.CorridorClimb)
		{
			base.GoThroughFloors = true;
			rollDirection = 0;
			if (corridorTurnDir.HasValue)
			{
				for (int k = 0; k < 2; k++)
				{
					base.bodyChunks[k].vel.y += base.gravity;
				}
				return;
			}
			if (input[0].y < 0 && !input[0].jmp && !IsTileSolid(0, 0, -1) && !IsTileSolid(0, 0, -2) && !IsTileSolid(0, 0, -3) && ((base.mainBodyChunk.pos.y < base.bodyChunks[1].pos.y && IsTileSolid(0, -1, 1) && IsTileSolid(0, 1, 1) && (!IsTileSolid(0, -1, 0) || !IsTileSolid(0, 1, 0))) || (base.mainBodyChunk.pos.y > base.bodyChunks[1].pos.y && IsTileSolid(1, -1, 1) && IsTileSolid(1, 1, 1) && (!IsTileSolid(1, -1, 0) || !IsTileSolid(1, 1, 0)))))
			{
				if (base.mainBodyChunk.pos.y < base.bodyChunks[1].pos.y)
				{
					if (room.GetTile(base.mainBodyChunk.pos).AnyBeam)
					{
						dropGrabTile = room.GetTilePosition(base.mainBodyChunk.pos);
					}
					else if (room.GetTile(base.mainBodyChunk.pos + new Vector2(0f, -20f)).AnyBeam)
					{
						dropGrabTile = room.GetTilePosition(base.mainBodyChunk.pos + new Vector2(0f, -20f));
					}
				}
				else if (room.GetTile(base.bodyChunks[1].pos).AnyBeam)
				{
					dropGrabTile = room.GetTilePosition(base.bodyChunks[1].pos);
				}
				else if (room.GetTile(base.bodyChunks[1].pos + new Vector2(0f, -20f)).AnyBeam)
				{
					dropGrabTile = room.GetTilePosition(base.bodyChunks[1].pos + new Vector2(0f, -20f));
				}
			}
			else
			{
				dropGrabTile = null;
			}
			bool flag3 = Mathf.Abs(base.bodyChunks[0].pos.x - base.bodyChunks[1].pos.x) < 5f && IsTileSolid(0, -1, 0) && IsTileSolid(0, 1, 0);
			bool flag4 = Mathf.Abs(base.bodyChunks[0].pos.y - base.bodyChunks[1].pos.y) < 7.5f && IsTileSolid(0, 0, -1) && IsTileSolid(0, 0, 1);
			bool flag5 = false;
			if (input[0].jmp && EffectiveRoomGravity > 0f && !input[1].jmp && input[0].y < 0 && !IsTileSolid(0, 0, -1) && !IsTileSolid(1, 0, -1))
			{
				corridorDrop = true;
				canCorridorJump = 0;
			}
			else if (flag3)
			{
				if (base.bodyChunks[0].pos.y > base.bodyChunks[1].pos.y)
				{
					flag5 = IsTileSolid(1, 0, -1);
				}
				else
				{
					if (EffectiveRoomGravity == 0f)
					{
						flag5 = IsTileSolid(1, 0, 1);
					}
					if (input[0].jmp && !input[1].jmp && base.bodyChunks[0].pos.y < base.bodyChunks[1].pos.y && input[0].y > 0)
					{
						corridorTurnDir = new IntVector2(0, 1);
						corridorTurnCounter = 0;
						canCorridorJump = 0;
					}
				}
			}
			else if (flag4)
			{
				if (input[0].jmp && !input[1].jmp && input[0].x != 0 && base.bodyChunks[0].pos.x < base.bodyChunks[1].pos.x == input[0].x > 0)
				{
					corridorTurnDir = new IntVector2(input[0].x, 0);
					corridorTurnCounter = 0;
					canCorridorJump = 0;
				}
				else
				{
					flag5 = IsTileSolid(1, -flipDirection, 0) && !IsTileSolid(0, flipDirection, 0);
				}
			}
			if (flag5)
			{
				canCorridorJump = 5;
			}
			else if (canCorridorJump > 0)
			{
				canCorridorJump--;
			}
			if (input[0].jmp && !input[1].jmp && slowMovementStun < 1)
			{
				if (EffectiveRoomGravity == 0f)
				{
					Vector2 vector = new Vector2(0f, 0f);
					if (flag3 && IsTileSolid(1, 0, (int)Mathf.Sign(base.bodyChunks[1].pos.y - base.bodyChunks[0].pos.y)))
					{
						vector = new Vector2(0f, 0f - Mathf.Sign(base.bodyChunks[1].pos.y - base.bodyChunks[0].pos.y));
					}
					else if (flag4 && IsTileSolid(1, (int)Mathf.Sign(base.bodyChunks[1].pos.x - base.bodyChunks[0].pos.x), 0))
					{
						vector = new Vector2(0f - Mathf.Sign(base.bodyChunks[1].pos.x - base.bodyChunks[0].pos.x), 0f);
					}
					if (vector.x != 0f || vector.y != 0f)
					{
						Vector2 pos = room.MiddleOfTile(base.bodyChunks[1].pos) - vector * 9f;
						for (int l = 0; l < 4; l++)
						{
							if (UnityEngine.Random.value < Mathf.InverseLerp(0f, 0.5f, room.roomSettings.CeilingDrips))
							{
								room.AddObject(new WaterDrip(pos, vector * 5f + Custom.RNV() * 3f, waterColor: false));
							}
						}
						room.PlaySound(SoundID.Slugcat_Corridor_Horizontal_Slide_Success, base.mainBodyChunk);
						base.bodyChunks[0].pos += 12f * vector * (isSlugpup ? 0.5f : 1f);
						base.bodyChunks[1].pos += 12f * vector * (isSlugpup ? 0.5f : 1f);
						base.bodyChunks[0].vel += 7f * Mathf.Lerp(1f, 1.2f, Adrenaline) * vector * (isSlugpup ? 0.5f : 1f);
						base.bodyChunks[1].vel += 7f * Mathf.Lerp(1f, 1.2f, Adrenaline) * vector * (isSlugpup ? 0.5f : 1f);
						horizontalCorridorSlideCounter = 25;
						slowMovementStun = 5;
					}
					else
					{
						if (flag4 && input[0].x != 0 && input[0].y == 0)
						{
							vector.x = input[0].x;
						}
						else if (flag3 && input[0].y != 0 && input[0].x == 0)
						{
							vector.y = input[0].y;
						}
						if (vector.x != 0f || vector.y != 0f)
						{
							room.PlaySound(SoundID.Slugcat_Corridor_Horizontal_Slide_Fail, base.mainBodyChunk);
							base.bodyChunks[0].vel += 6f * Mathf.Lerp(1f, 1.2f, Adrenaline) * vector;
							base.bodyChunks[1].vel += 4f * Mathf.Lerp(1f, 1.2f, Adrenaline) * vector;
							horizontalCorridorSlideCounter = 15;
							slowMovementStun = 15;
						}
					}
				}
				else if (verticalCorridorSlideCounter < 1)
				{
					if (flag3 && input[0].y > -1 && base.bodyChunks[0].pos.y > base.bodyChunks[1].pos.y)
					{
						base.bodyChunks[0].vel.y += 15f * Mathf.Lerp(1f, 1.2f, Adrenaline) * (isSlugpup ? 0.5f : 1f);
						base.bodyChunks[1].vel.y += 10f * Mathf.Lerp(1f, 1.2f, Adrenaline) * (isSlugpup ? 0.5f : 1f);
						if (canCorridorJump > 0)
						{
							shootUpCounter = 30;
							Vector2 pos2 = room.MiddleOfTile(base.bodyChunks[1].pos) + new Vector2(0f, -9f);
							for (int m = 0; m < 4; m++)
							{
								if (UnityEngine.Random.value < Mathf.InverseLerp(0f, 0.5f, room.roomSettings.CeilingDrips))
								{
									room.AddObject(new WaterDrip(pos2, new Vector2(Mathf.Lerp(-3f, 3f, UnityEngine.Random.value), 5f), waterColor: false));
								}
							}
							room.PlaySound(SoundID.Slugcat_Vertical_Chute_Jump_Success, base.mainBodyChunk);
							verticalCorridorSlideCounter = 22;
							slowMovementStun = 2;
						}
						else
						{
							room.PlaySound(SoundID.Slugcat_Vertical_Chute_Jump_Fail, base.mainBodyChunk);
							verticalCorridorSlideCounter = 34;
							slowMovementStun = 18;
						}
						canCorridorJump = 0;
					}
					else if (flag4)
					{
						flipDirection = ((base.bodyChunks[0].pos.x > base.bodyChunks[1].pos.x) ? 1 : (-1));
						if (input[0].x == flipDirection || input[0].x == 0)
						{
							if (canCorridorJump > 0 && input[0].x != 0)
							{
								Vector2 pos3 = room.MiddleOfTile(base.bodyChunks[1].pos) + new Vector2(-9f * (float)flipDirection, 0f);
								for (int n = 0; n < 4; n++)
								{
									if (UnityEngine.Random.value < Mathf.InverseLerp(0f, 0.5f, room.roomSettings.CeilingDrips))
									{
										room.AddObject(new WaterDrip(pos3, new Vector2((float)flipDirection * 5f, Mathf.Lerp(-3f, 3f, UnityEngine.Random.value)), waterColor: false));
									}
								}
								room.PlaySound(SoundID.Slugcat_Corridor_Horizontal_Slide_Success, base.mainBodyChunk);
								base.bodyChunks[0].pos.x += 12f * (float)flipDirection * (isSlugpup ? 0.5f : 1f);
								base.bodyChunks[1].pos.x += 12f * (float)flipDirection * (isSlugpup ? 0.5f : 1f);
								base.bodyChunks[0].pos.y = room.MiddleOfTile(base.bodyChunks[0].pos).y;
								base.bodyChunks[1].pos.y = base.bodyChunks[0].pos.y;
								base.bodyChunks[0].vel.x += 7f * Mathf.Lerp(1f, 1.2f, Adrenaline) * (float)flipDirection * (isSlugpup ? 0.5f : 1f);
								base.bodyChunks[1].vel.x += 7f * Mathf.Lerp(1f, 1.2f, Adrenaline) * (float)flipDirection * (isSlugpup ? 0.5f : 1f);
								horizontalCorridorSlideCounter = 25;
								slowMovementStun = 5;
							}
							else
							{
								room.PlaySound(SoundID.Slugcat_Corridor_Horizontal_Slide_Fail, base.mainBodyChunk);
								base.bodyChunks[0].vel.x += 6f * Mathf.Lerp(1f, 1.2f, Adrenaline) * (float)flipDirection;
								base.bodyChunks[1].vel.x += 4f * Mathf.Lerp(1f, 1.2f, Adrenaline) * (float)flipDirection;
								horizontalCorridorSlideCounter = 15;
								slowMovementStun = 15;
							}
						}
					}
				}
			}
			if (verticalCorridorSlideCounter == 1 || horizontalCorridorSlideCounter == 1)
			{
				slowMovementStun = 15;
			}
			float num2 = Mathf.InverseLerp(0f, 10f, Math.Max(verticalCorridorSlideCounter, horizontalCorridorSlideCounter)) * (isSlugpup ? 0.5f : 1f);
			base.bodyChunks[0].vel *= 0.9f - 0.3f * surfaceFriction * (1f - num2);
			base.bodyChunks[1].vel *= 0.9f - 0.3f * surfaceFriction * (1f - num2);
			base.bodyChunks[0].vel.y += base.gravity * Mathf.Clamp(surfaceFriction * 8f, 0.2f, 1f) * Mathf.Lerp(1f, 0.2f, Mathf.InverseLerp(0f, 10f, verticalCorridorSlideCounter));
			base.bodyChunks[1].vel.y += base.gravity * Mathf.Clamp(surfaceFriction * 8f, 0.2f, 1f) * Mathf.Lerp(1f, 0.2f, Mathf.InverseLerp(0f, 10f, verticalCorridorSlideCounter));
			base.bodyChunks[0].vel.y -= base.buoyancy * base.bodyChunks[0].submersion * (ModManager.MMF ? EffectiveRoomGravity : 1f);
			base.bodyChunks[1].vel.y -= base.buoyancy * base.bodyChunks[1].submersion * (ModManager.MMF ? EffectiveRoomGravity : 1f);
			dynamicRunSpeed[0] = 0f;
			dynamicRunSpeed[1] = 0f;
			float num3 = 2.4f * Mathf.Clamp(surfaceFriction + 0.2f, 0.2f, 0.5f) * Mathf.Lerp(1f, 1.2f, Adrenaline) * slugcatStats.corridorClimbSpeedFac;
			num3 *= Mathf.Lerp(0.1f, 1f, Mathf.InverseLerp(10f, 0f, slowMovementStun));
			if (input[0].x != 0 && input[0].y != 0)
			{
				num3 *= 0.4f;
			}
			if (input[0].x != 0 && input[0].x > 0 == base.bodyChunks[0].pos.x < base.bodyChunks[1].pos.x)
			{
				backwardsCounter += 2;
			}
			else if (input[0].y > 0 && base.bodyChunks[0].pos.y < base.bodyChunks[1].pos.y)
			{
				backwardsCounter += 2;
			}
			if (backwardsCounter > 20)
			{
				backwardsCounter = 20;
			}
			if (backwardsCounter > 10)
			{
				num3 *= 0.6f;
			}
			for (int num4 = 0; num4 < 2; num4++)
			{
				if (input[0].x != 0 && !IsTileSolid(num4, input[0].x, 0))
				{
					base.bodyChunks[num4].vel.x += num3 * (float)input[0].x;
					base.bodyChunks[num4].vel.y = base.bodyChunks[num4].vel.y * 0.8f - (base.bodyChunks[num4].pos.y - room.MiddleOfTile(room.GetTilePosition(base.bodyChunks[num4].pos)).y) * 0.2f;
					if (input[0].y == 0)
					{
						base.bodyChunks[1 - num4].vel.y = base.bodyChunks[1 - num4].vel.y * 0.8f - (base.bodyChunks[num4].pos.y - room.MiddleOfTile(room.GetTilePosition(base.bodyChunks[num4].pos)).y) * 0.2f;
					}
					break;
				}
				if (input[0].y != 0 && !IsTileSolid(num4, 0, input[0].y))
				{
					base.bodyChunks[num4].vel.y += num3 * (float)input[0].y;
					base.bodyChunks[num4].vel.x = base.bodyChunks[num4].vel.x * 0.8f - (base.bodyChunks[num4].pos.x - room.MiddleOfTile(room.GetTilePosition(base.bodyChunks[num4].pos)).x) * 0.2f;
					if (input[0].x == 0)
					{
						base.bodyChunks[1 - num4].vel.x = base.bodyChunks[1 - num4].vel.x * 0.8f - (base.bodyChunks[num4].pos.x - room.MiddleOfTile(room.GetTilePosition(base.bodyChunks[num4].pos)).x) * 0.2f;
					}
					break;
				}
			}
			base.bodyChunks[0].vel.x += num3 * (float)input[0].x * 0.1f;
			base.bodyChunks[0].vel.y += num3 * (float)input[0].y * 0.1f;
			standing = base.bodyChunks[0].pos.y > base.bodyChunks[1].pos.y + 5f;
			if ((input[0].x != 0 || input[0].y != 0) && verticalCorridorSlideCounter < 16)
			{
				animationFrame++;
			}
			else
			{
				animationFrame = 0;
			}
			if (animationFrame > 12)
			{
				animationFrame = 1;
				room.PlaySound(SoundID.Slugcat_In_Corridor_Step, base.mainBodyChunk);
			}
			if (input[0].y > 0 && (ModManager.MMF || shootUpCounter < 1) && (!IsTileSolid(0, -1, 0) || !IsTileSolid(0, 1, 0)) && room.GetTile(base.mainBodyChunk.pos).verticalBeam && (!ModManager.MSC || !monkAscension))
			{
				room.PlaySound(SoundID.Slugcat_Grab_Beam, base.mainBodyChunk, loop: false, 0.2f, 1f);
				bodyMode = BodyModeIndex.ClimbingOnBeam;
				animation = AnimationIndex.ClimbOnBeam;
			}
			else if (input[0].x != 0 && room.GetTile(base.mainBodyChunk.pos).horizontalBeam && IsTileSolid(1, 0, -1) && IsTileSolid(1, 0, 1) && !IsTileSolid(0, input[0].x, -1) && (!ModManager.MSC || !monkAscension))
			{
				bodyMode = BodyModeIndex.ClimbingOnBeam;
				animation = AnimationIndex.HangFromBeam;
			}
		}
		else
		{
			if (bodyMode == BodyModeIndex.ClimbIntoShortCut)
			{
				return;
			}
			if (bodyMode == BodyModeIndex.WallClimb)
			{
				if (base.bodyChunks[0].pos.y > base.bodyChunks[1].pos.y)
				{
					if (input[0].x != 0 && input[0].x == base.bodyChunks[0].ContactPoint.x)
					{
						canWallJump = input[0].x * -15;
					}
					canJump = 1;
				}
				if (input[0].x != 0 && IsTileSolid(0, input[0].x, 0) && IsTileSolid(1, input[0].x, 0))
				{
					base.bodyChunks[0].pos.y += base.gravity * Custom.LerpMap(wallSlideCounter, 0f, 30f, 0.8f, 0f) * EffectiveRoomGravity;
					base.bodyChunks[1].pos.y += base.gravity * Custom.LerpMap(wallSlideCounter, 0f, 30f, 0.8f, 0f) * EffectiveRoomGravity;
				}
				base.bodyChunks[1].vel.y += base.bodyChunks[1].submersion * EffectiveRoomGravity;
			}
			else if (bodyMode == BodyModeIndex.ClimbingOnBeam)
			{
				if ((animation == AnimationIndex.GetUpOnBeam || animation == AnimationIndex.StandOnBeam) && !room.GetTile(base.bodyChunks[1].pos).horizontalBeam)
				{
					if (forceFeetToHorizontalBeamTile > 0 && room.GetTile(base.bodyChunks[1].pos + new Vector2((float)flipDirection * 20f, 0f)).horizontalBeam)
					{
						base.bodyChunks[1].pos.x = room.MiddleOfTile(base.bodyChunks[1].pos + new Vector2((float)flipDirection * 20f, 0f)).x - (float)flipDirection * 8f;
						base.bodyChunks[1].vel.x = 0f;
						forceFeetToHorizontalBeamTile = 20;
					}
					else if (forceFeetToHorizontalBeamTile > 0 && room.GetTile(base.bodyChunks[1].pos - new Vector2((float)flipDirection * 20f, 0f)).horizontalBeam)
					{
						base.bodyChunks[1].pos.x = room.MiddleOfTile(base.bodyChunks[1].pos - new Vector2((float)flipDirection * 20f, 0f)).x + (float)flipDirection * 8f;
						base.bodyChunks[1].vel.x = 0f;
						forceFeetToHorizontalBeamTile = 20;
					}
					else if (!room.GetTile(base.bodyChunks[1].pos + new Vector2(0f, 20f)).horizontalBeam || !(animation == AnimationIndex.GetUpOnBeam))
					{
						animation = AnimationIndex.None;
					}
				}
				if (animation != AnimationIndex.BeamTip && animation != AnimationIndex.ClimbOnBeam && animation != AnimationIndex.GetUpOnBeam && animation != AnimationIndex.HangFromBeam && animation != AnimationIndex.StandOnBeam && animation != AnimationIndex.GetUpToBeamTip && animation != AnimationIndex.HangUnderVerticalBeam)
				{
					bodyMode = BodyModeIndex.Default;
				}
			}
			else if (bodyMode == BodyModeIndex.Swimming)
			{
				GravitateToOpening();
				base.GoThroughFloors = true;
			}
			else if (bodyMode == BodyModeIndex.ZeroG)
			{
				GravitateToOpening();
				base.GoThroughFloors = true;
				if (EffectiveRoomGravity > 0f)
				{
					bodyMode = BodyModeIndex.Default;
					animation = AnimationIndex.None;
				}
			}
			else if (!(bodyMode == BodyModeIndex.Stunned))
			{
				_ = bodyMode == BodyModeIndex.Dead;
			}
		}
	}

	private void GravitateToOpening()
	{
		if (input[0].x != 0 && input[0].y == 0)
		{
			if (IsTileSolid(0, input[0].x, 0) || !IsTileSolid(0, input[0].x, 1) || !IsTileSolid(0, input[0].x, -1) || (IsTileSolid(0, 0, 1) && IsTileSolid(0, 0, -1)))
			{
				return;
			}
			Vector2 vector = room.MiddleOfTile(room.GetTilePosition(base.bodyChunks[0].pos) + new IntVector2(input[0].x, 0));
			int num = -1;
			float dst = float.MaxValue;
			for (int i = 0; i < 2; i++)
			{
				if (Custom.DistLess(base.bodyChunks[i].pos, vector, dst))
				{
					num = i;
					dst = Vector2.Distance(base.bodyChunks[i].pos, vector);
				}
			}
			base.bodyChunks[num].vel += Custom.DirVec(base.bodyChunks[num].pos, vector) * Mathf.Lerp(0.5f, 1f, EffectiveRoomGravity);
			for (int j = 0; j < 2; j++)
			{
				base.bodyChunks[j].vel.y -= (base.bodyChunks[j].pos.y - room.MiddleOfTile(base.bodyChunks[j].pos).y) * Mathf.Lerp(0.01f, 0.1f, EffectiveRoomGravity);
				base.bodyChunks[j].vel = Vector2.Lerp(base.bodyChunks[j].vel, Vector2.ClampMagnitude(base.bodyChunks[j].vel, 2f), 0.5f);
			}
		}
		else
		{
			if (input[0].y == 0 || input[0].x != 0 || IsTileSolid(0, 0, input[0].y) || !IsTileSolid(0, 1, input[0].y) || !IsTileSolid(0, -1, input[0].y) || (IsTileSolid(0, 1, 0) && IsTileSolid(0, -1, 0)))
			{
				return;
			}
			Vector2 vector2 = room.MiddleOfTile(room.GetTilePosition(base.bodyChunks[0].pos) + new IntVector2(0, input[0].y));
			int num2 = -1;
			float dst2 = float.MaxValue;
			for (int k = 0; k < 2; k++)
			{
				if (Custom.DistLess(base.bodyChunks[k].pos, vector2, dst2))
				{
					num2 = k;
					dst2 = Vector2.Distance(base.bodyChunks[k].pos, vector2);
				}
			}
			base.bodyChunks[num2].vel += Custom.DirVec(base.bodyChunks[num2].pos, vector2) * Mathf.Lerp(0.5f, 1f, EffectiveRoomGravity);
			for (int l = 0; l < 2; l++)
			{
				base.bodyChunks[l].vel.x -= (base.bodyChunks[l].pos.x - room.MiddleOfTile(base.bodyChunks[l].pos).x) * Mathf.Lerp(0.01f, 0.1f, EffectiveRoomGravity);
				base.bodyChunks[l].vel = Vector2.Lerp(base.bodyChunks[l].vel, Vector2.ClampMagnitude(base.bodyChunks[l].vel, 2f), 0.5f);
			}
		}
	}

	public override void ReleaseGrasp(int grasp)
	{
		if (base.grasps[grasp] != null && room != null)
		{
			room.PlaySound(SoundID.Slugcat_Lose_Grasp, base.mainBodyChunk, loop: false, 1f, 1f);
		}
		base.ReleaseGrasp(grasp);
	}

	private void BiteEdibleObject(bool eu)
	{
		for (int i = 0; i < 2; i++)
		{
			if (base.grasps[i] != null && base.grasps[i].grabbed is IPlayerEdible && (base.grasps[i].grabbed as IPlayerEdible).Edible && (!ModManager.MSC || SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Spear))
			{
				if ((base.grasps[i].grabbed as IPlayerEdible).BitesLeft == 1 && SessionRecord != null)
				{
					SessionRecord.AddEat(base.grasps[i].grabbed);
				}
				if (base.grasps[i].grabbed is Creature)
				{
					(base.grasps[i].grabbed as Creature).SetKillTag(base.abstractCreature);
				}
				if (base.graphicsModule != null)
				{
					(base.graphicsModule as PlayerGraphics).BiteFly(i);
				}
				(base.grasps[i].grabbed as IPlayerEdible).BitByPlayer(base.grasps[i], eu);
				break;
			}
		}
	}

	public void ObjectEaten(IPlayerEdible edible)
	{
		if (base.graphicsModule != null)
		{
			(base.graphicsModule as PlayerGraphics).LookAtNothing();
		}
		if (ModManager.MSC && SlugcatStats.NourishmentOfObjectEaten(SlugCatClass, edible) == -1)
		{
			Stun(60);
			return;
		}
		if (AI != null)
		{
			AI.AteFood(edible as PhysicalObject);
		}
		if (room.game.IsStorySession)
		{
			int num;
			for (num = SlugcatStats.NourishmentOfObjectEaten(SlugCatClass, edible); num >= 4; num -= 4)
			{
				AddFood(1);
			}
			while (num > 0)
			{
				AddQuarterFood();
				num--;
			}
		}
		else
		{
			AddFood(edible.FoodPoints);
		}
		if ((ModManager.MSC || ModManager.CoopAvailable) && slugOnBack != null)
		{
			slugOnBack.interactionLocked = true;
		}
		if (spearOnBack != null)
		{
			spearOnBack.interactionLocked = true;
		}
	}

	private void PickupPressed()
	{
		wantToPickUp = 5;
		swallowAndRegurgitateCounter = 0;
	}

	public void GrabUpdate(bool eu)
	{
		if (spearOnBack != null)
		{
			spearOnBack.Update(eu);
		}
		if ((ModManager.MSC || ModManager.CoopAvailable) && slugOnBack != null)
		{
			slugOnBack.Update(eu);
		}
		bool num = ((input[0].x == 0 && input[0].y == 0 && !input[0].jmp && !input[0].thrw) || (ModManager.MMF && input[0].x == 0 && input[0].y == 1 && !input[0].jmp && !input[0].thrw && (bodyMode != BodyModeIndex.ClimbingOnBeam || animation == AnimationIndex.BeamTip || animation == AnimationIndex.StandOnBeam))) && (base.mainBodyChunk.submersion < 0.5f || isRivulet);
		bool flag = false;
		bool flag2 = false;
		craftingObject = false;
		int num2 = -1;
		int num3 = -1;
		bool flag3 = false;
		if (ModManager.MSC && !input[0].pckp && SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Spear)
		{
			PlayerGraphics.TailSpeckles tailSpecks = (base.graphicsModule as PlayerGraphics).tailSpecks;
			if (tailSpecks.spearProg > 0f)
			{
				tailSpecks.setSpearProgress(Mathf.Lerp(tailSpecks.spearProg, 0f, 0.05f));
				if (tailSpecks.spearProg < 0.025f)
				{
					tailSpecks.setSpearProgress(0f);
				}
			}
			else
			{
				smSpearSoundReady = false;
			}
		}
		if (input[0].pckp && !input[1].pckp && switchHandsProcess == 0f && !isSlugpup)
		{
			bool flag4 = base.grasps[0] != null || base.grasps[1] != null;
			if (base.grasps[0] != null && (Grabability(base.grasps[0].grabbed) == ObjectGrabability.TwoHands || Grabability(base.grasps[0].grabbed) == ObjectGrabability.Drag))
			{
				flag4 = false;
			}
			if (flag4)
			{
				if (switchHandsCounter == 0)
				{
					switchHandsCounter = 15;
				}
				else
				{
					room.PlaySound(SoundID.Slugcat_Switch_Hands_Init, base.mainBodyChunk);
					switchHandsProcess = 0.01f;
					wantToPickUp = 0;
					noPickUpOnRelease = 20;
				}
			}
			else
			{
				switchHandsProcess = 0f;
			}
		}
		if (switchHandsProcess > 0f)
		{
			float num4 = switchHandsProcess;
			switchHandsProcess += 1f / 12f;
			if (num4 < 0.5f && switchHandsProcess >= 0.5f)
			{
				room.PlaySound(SoundID.Slugcat_Switch_Hands_Complete, base.mainBodyChunk);
				SwitchGrasps(0, 1);
			}
			if (switchHandsProcess >= 1f)
			{
				switchHandsProcess = 0f;
			}
		}
		int num5 = -1;
		int num6 = -1;
		int num7 = -1;
		if (num)
		{
			int num8 = -1;
			if (ModManager.MSC)
			{
				for (int i = 0; i < 2; i++)
				{
					if (base.grasps[i] != null)
					{
						if (base.grasps[i].grabbed is JokeRifle)
						{
							num3 = i;
						}
						else if (JokeRifle.IsValidAmmo(base.grasps[i].grabbed))
						{
							num2 = i;
						}
					}
				}
			}
			int num9 = 0;
			while (num6 < 0 && num9 < 2 && (!ModManager.MSC || SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Spear))
			{
				if (base.grasps[num9] != null && base.grasps[num9].grabbed is IPlayerEdible && (base.grasps[num9].grabbed as IPlayerEdible).Edible)
				{
					num6 = num9;
				}
				num9++;
			}
			if ((num6 == -1 || (FoodInStomach >= MaxFoodInStomach && !(base.grasps[num6].grabbed is KarmaFlower) && !(base.grasps[num6].grabbed is Mushroom))) && (objectInStomach == null || CanPutSpearToBack || CanPutSlugToBack))
			{
				int num10 = 0;
				while (num8 < 0 && num5 < 0 && num7 < 0 && num10 < 2)
				{
					if (base.grasps[num10] != null)
					{
						if ((CanPutSlugToBack && base.grasps[num10].grabbed is Player && !(base.grasps[num10].grabbed as Player).dead) || CanIPutDeadSlugOnBack(base.grasps[num10].grabbed as Player))
						{
							num7 = num10;
						}
						else if (CanPutSpearToBack && base.grasps[num10].grabbed is Spear)
						{
							num5 = num10;
						}
						else if (CanBeSwallowed(base.grasps[num10].grabbed))
						{
							num8 = num10;
						}
					}
					num10++;
				}
			}
			if (num6 > -1 && noPickUpOnRelease < 1)
			{
				if (!input[0].pckp)
				{
					int j;
					for (j = 1; j < 10 && input[j].pckp; j++)
					{
					}
					if (j > 1 && j < 10)
					{
						PickupPressed();
					}
				}
			}
			else if (input[0].pckp && !input[1].pckp)
			{
				PickupPressed();
			}
			if (input[0].pckp)
			{
				if (ModManager.MSC && (FreeHand() == -1 || SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer) && GraspsCanBeCrafted())
				{
					craftingObject = true;
					flag2 = true;
					num6 = -1;
				}
				if (num7 > -1 || CanRetrieveSlugFromBack)
				{
					slugOnBack.increment = true;
				}
				else if (num5 > -1 || CanRetrieveSpearFromBack)
				{
					spearOnBack.increment = true;
				}
				else if ((num8 > -1 || objectInStomach != null || isGourmand) && (!ModManager.MSC || SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Spear))
				{
					flag2 = true;
				}
				if (num2 > -1 && num3 > -1)
				{
					flag3 = true;
				}
				if (ModManager.MSC && SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Spear && (base.grasps[0] == null || base.grasps[1] == null) && num6 == -1 && input[0].y == 0)
				{
					PlayerGraphics.TailSpeckles tailSpecks2 = (base.graphicsModule as PlayerGraphics).tailSpecks;
					if (tailSpecks2.spearProg == 0f)
					{
						tailSpecks2.newSpearSlot();
					}
					if (tailSpecks2.spearProg < 0.1f)
					{
						tailSpecks2.setSpearProgress(Mathf.Lerp(tailSpecks2.spearProg, 0.11f, 0.1f));
					}
					else
					{
						if (!smSpearSoundReady)
						{
							smSpearSoundReady = true;
							room.PlaySound(MoreSlugcatsEnums.MSCSoundID.SM_Spear_Pull, 0f, 1f, 1f + UnityEngine.Random.value * 0.5f);
						}
						tailSpecks2.setSpearProgress(Mathf.Lerp(tailSpecks2.spearProg, 1f, 0.05f));
					}
					if (tailSpecks2.spearProg > 0.6f)
					{
						(base.graphicsModule as PlayerGraphics).head.vel += Custom.RNV() * ((tailSpecks2.spearProg - 0.6f) / 0.4f) * 2f;
					}
					if (tailSpecks2.spearProg > 0.95f)
					{
						tailSpecks2.setSpearProgress(1f);
					}
					if (tailSpecks2.spearProg == 1f)
					{
						room.PlaySound(MoreSlugcatsEnums.MSCSoundID.SM_Spear_Grab, 0f, 1f, 0.5f + UnityEngine.Random.value * 1.5f);
						smSpearSoundReady = false;
						Vector2 pos = (base.graphicsModule as PlayerGraphics).tail[(int)((float)(base.graphicsModule as PlayerGraphics).tail.Length / 2f)].pos;
						for (int k = 0; k < 4; k++)
						{
							Vector2 vector = Custom.DirVec(pos, base.bodyChunks[1].pos);
							room.AddObject(new WaterDrip(pos + Custom.RNV() * UnityEngine.Random.value * 1.5f, Custom.RNV() * 3f * UnityEngine.Random.value + vector * Mathf.Lerp(2f, 6f, UnityEngine.Random.value), waterColor: false));
						}
						for (int l = 0; l < 5; l++)
						{
							Vector2 vector2 = Custom.RNV();
							room.AddObject(new Spark(pos + vector2 * UnityEngine.Random.value * 40f, vector2 * Mathf.Lerp(4f, 30f, UnityEngine.Random.value), Color.white, null, 4, 18));
						}
						int spearType = tailSpecks2.spearType;
						tailSpecks2.setSpearProgress(0f);
						AbstractSpear abstractSpear = new AbstractSpear(room.world, null, room.GetWorldCoordinate(base.mainBodyChunk.pos), room.game.GetNewID(), explosive: false);
						room.abstractRoom.AddEntity(abstractSpear);
						abstractSpear.pos = base.abstractCreature.pos;
						abstractSpear.RealizeInRoom();
						Vector2 pos2 = base.bodyChunks[0].pos;
						Vector2 vector3 = Custom.DirVec(base.bodyChunks[1].pos, base.bodyChunks[0].pos);
						if (Mathf.Abs(base.bodyChunks[0].pos.y - base.bodyChunks[1].pos.y) > Mathf.Abs(base.bodyChunks[0].pos.x - base.bodyChunks[1].pos.x) && base.bodyChunks[0].pos.y > base.bodyChunks[1].pos.y)
						{
							pos2 += Custom.DirVec(base.bodyChunks[1].pos, base.bodyChunks[0].pos) * 5f;
							vector3 *= -1f;
							vector3.x += 0.4f * (float)flipDirection;
							vector3.Normalize();
						}
						abstractSpear.realizedObject.firstChunk.HardSetPosition(pos2);
						abstractSpear.realizedObject.firstChunk.vel = Vector2.ClampMagnitude((vector3 * 2f + Custom.RNV() * UnityEngine.Random.value) / abstractSpear.realizedObject.firstChunk.mass, 6f);
						if (FreeHand() > -1)
						{
							SlugcatGrab(abstractSpear.realizedObject, FreeHand());
						}
						if (abstractSpear.type == AbstractPhysicalObject.AbstractObjectType.Spear)
						{
							(abstractSpear.realizedObject as Spear).Spear_makeNeedle(spearType, active: true);
							if ((base.graphicsModule as PlayerGraphics).useJollyColor)
							{
								(abstractSpear.realizedObject as Spear).jollyCustomColor = PlayerGraphics.JollyColor(playerState.playerNumber, 2);
							}
						}
						wantToThrow = 0;
					}
				}
			}
			if (num6 > -1 && wantToPickUp < 1 && (input[0].pckp || eatCounter <= 15) && base.Consious && Custom.DistLess(base.mainBodyChunk.pos, base.mainBodyChunk.lastPos, 3.6f))
			{
				if (base.graphicsModule != null)
				{
					(base.graphicsModule as PlayerGraphics).LookAtObject(base.grasps[num6].grabbed);
				}
				if (ModManager.MSC && SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint && (KarmaCap == 9 || (room.game.IsArenaSession && room.game.GetArenaGameSession.arenaSitting.gameTypeSetup.gameType != MoreSlugcatsEnums.GameTypeID.Challenge) || (room.game.session is ArenaGameSession && room.game.GetArenaGameSession.arenaSitting.gameTypeSetup.gameType == MoreSlugcatsEnums.GameTypeID.Challenge && room.game.GetArenaGameSession.arenaSitting.gameTypeSetup.challengeMeta.ascended)) && base.grasps[num6].grabbed is Fly && eatCounter < 1)
				{
					room.PlaySound(SoundID.Snail_Pop, base.mainBodyChunk, loop: false, 1f, 1.5f + UnityEngine.Random.value);
					eatCounter = 30;
					room.AddObject(new ShockWave(base.grasps[num6].grabbed.firstChunk.pos, 25f, 0.8f, 4));
					for (int m = 0; m < 5; m++)
					{
						room.AddObject(new Spark(base.grasps[num6].grabbed.firstChunk.pos, Custom.RNV() * 3f, Color.yellow, null, 25, 90));
					}
					base.grasps[num6].grabbed.Destroy();
					base.grasps[num6].grabbed.abstractPhysicalObject.Destroy();
					if (room.game.IsArenaSession)
					{
						AddFood(1);
					}
				}
				flag = true;
				if (FoodInStomach < MaxFoodInStomach || base.grasps[num6].grabbed is KarmaFlower || base.grasps[num6].grabbed is Mushroom)
				{
					flag2 = false;
					if (spearOnBack != null)
					{
						spearOnBack.increment = false;
					}
					if ((ModManager.MSC || ModManager.CoopAvailable) && slugOnBack != null)
					{
						slugOnBack.increment = false;
					}
					if (eatCounter < 1)
					{
						eatCounter = 15;
						BiteEdibleObject(eu);
					}
				}
				else if (eatCounter < 20 && room.game.cameras[0].hud != null)
				{
					room.game.cameras[0].hud.foodMeter.RefuseFood();
				}
			}
		}
		else if (input[0].pckp && !input[1].pckp)
		{
			PickupPressed();
		}
		else
		{
			if (CanPutSpearToBack)
			{
				for (int n = 0; n < 2; n++)
				{
					if (base.grasps[n] != null && base.grasps[n].grabbed is Spear)
					{
						num5 = n;
						break;
					}
				}
			}
			if (CanPutSlugToBack)
			{
				for (int num11 = 0; num11 < 2; num11++)
				{
					if (base.grasps[num11] != null && base.grasps[num11].grabbed is Player && !(base.grasps[num11].grabbed as Player).dead)
					{
						num7 = num11;
						break;
					}
				}
			}
			if (input[0].pckp && (num7 > -1 || CanRetrieveSlugFromBack))
			{
				slugOnBack.increment = true;
			}
			if (input[0].pckp && (num5 > -1 || CanRetrieveSpearFromBack))
			{
				spearOnBack.increment = true;
			}
		}
		int num12 = 0;
		if (ModManager.MMF && (base.grasps[0] == null || !(base.grasps[0].grabbed is Creature)) && base.grasps[1] != null && base.grasps[1].grabbed is Creature)
		{
			num12 = 1;
		}
		if (ModManager.MSC && SlugcatStats.SlugcatCanMaul(SlugCatClass))
		{
			if (input[0].pckp && base.grasps[num12] != null && base.grasps[num12].grabbed is Creature && (CanMaulCreature(base.grasps[num12].grabbed as Creature) || maulTimer > 0))
			{
				maulTimer++;
				(base.grasps[num12].grabbed as Creature).Stun(60);
				MaulingUpdate(num12);
				if (spearOnBack != null)
				{
					spearOnBack.increment = false;
					spearOnBack.interactionLocked = true;
				}
				if (slugOnBack != null)
				{
					slugOnBack.increment = false;
					slugOnBack.interactionLocked = true;
				}
				if (base.grasps[num12] == null || maulTimer % 40 != 0)
				{
					return;
				}
				room.PlaySound(SoundID.Slugcat_Eat_Meat_B, base.mainBodyChunk);
				room.PlaySound(SoundID.Drop_Bug_Grab_Creature, base.mainBodyChunk, loop: false, 1f, 0.76f);
				Custom.Log("Mauled target");
				if (!(base.grasps[num12].grabbed as Creature).dead)
				{
					for (int num13 = UnityEngine.Random.Range(8, 14); num13 >= 0; num13--)
					{
						room.AddObject(new WaterDrip(Vector2.Lerp(base.grasps[num12].grabbedChunk.pos, base.mainBodyChunk.pos, UnityEngine.Random.value) + base.grasps[num12].grabbedChunk.rad * Custom.RNV() * UnityEngine.Random.value, Custom.RNV() * 6f * UnityEngine.Random.value + Custom.DirVec(base.grasps[num12].grabbed.firstChunk.pos, (base.mainBodyChunk.pos + (base.graphicsModule as PlayerGraphics).head.pos) / 2f) * 7f * UnityEngine.Random.value + Custom.DegToVec(Mathf.Lerp(-90f, 90f, UnityEngine.Random.value)) * UnityEngine.Random.value * EffectiveRoomGravity * 7f, waterColor: false));
					}
					Creature creature = base.grasps[num12].grabbed as Creature;
					creature.SetKillTag(base.abstractCreature);
					creature.Violence(base.bodyChunks[0], new Vector2(0f, 0f), base.grasps[num12].grabbedChunk, null, DamageType.Bite, 1f, 15f);
					creature.stun = 5;
					if (creature.abstractCreature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.Inspector)
					{
						creature.Die();
					}
				}
				maulTimer = 0;
				wantToPickUp = 0;
				if (base.grasps[num12] != null)
				{
					TossObject(num12, eu);
					ReleaseGrasp(num12);
				}
				standing = true;
				return;
			}
			if (base.grasps[num12] != null && base.grasps[num12].grabbed is Creature && (base.grasps[num12].grabbed as Creature).Consious && !IsCreatureLegalToHoldWithoutStun(base.grasps[num12].grabbed as Creature))
			{
				Custom.Log("Lost hold of live mauling target");
				maulTimer = 0;
				wantToPickUp = 0;
				ReleaseGrasp(num12);
				return;
			}
		}
		if (input[0].pckp && base.grasps[num12] != null && base.grasps[num12].grabbed is Creature && CanEatMeat(base.grasps[num12].grabbed as Creature) && (base.grasps[num12].grabbed as Creature).Template.meatPoints > 0)
		{
			eatMeat++;
			EatMeatUpdate(num12);
			if (!ModManager.MMF)
			{
				flag = false;
				flag2 = false;
			}
			if (spearOnBack != null)
			{
				spearOnBack.increment = false;
				spearOnBack.interactionLocked = true;
			}
			if ((ModManager.MSC || ModManager.CoopAvailable) && slugOnBack != null)
			{
				slugOnBack.increment = false;
				slugOnBack.interactionLocked = true;
			}
			if (base.grasps[num12] != null && eatMeat % 80 == 0 && ((base.grasps[num12].grabbed as Creature).State.meatLeft <= 0 || FoodInStomach >= MaxFoodInStomach))
			{
				eatMeat = 0;
				wantToPickUp = 0;
				TossObject(num12, eu);
				ReleaseGrasp(num12);
				standing = true;
			}
			return;
		}
		if (!input[0].pckp && base.grasps[num12] != null && eatMeat > 60)
		{
			eatMeat = 0;
			wantToPickUp = 0;
			TossObject(num12, eu);
			ReleaseGrasp(num12);
			standing = true;
			return;
		}
		eatMeat = Custom.IntClamp(eatMeat - 1, 0, 50);
		maulTimer = Custom.IntClamp(maulTimer - 1, 0, 20);
		if (!ModManager.MMF || input[0].y == 0)
		{
			if (flag && eatCounter > 0)
			{
				if (ModManager.MSC)
				{
					if (num6 <= -1 || base.grasps[num6] == null || !(base.grasps[num6].grabbed is GooieDuck) || (base.grasps[num6].grabbed as GooieDuck).bites != 6 || timeSinceSpawned % 2 == 0)
					{
						eatCounter--;
					}
					if (num6 > -1 && base.grasps[num6] != null && base.grasps[num6].grabbed is GooieDuck && (base.grasps[num6].grabbed as GooieDuck).bites == 6 && FoodInStomach < MaxFoodInStomach)
					{
						(base.graphicsModule as PlayerGraphics).BiteStruggle(num6);
					}
				}
				else
				{
					eatCounter--;
				}
			}
			else if (!flag && eatCounter < 40)
			{
				eatCounter++;
			}
		}
		if (flag3 && input[0].y == 0)
		{
			reloadCounter++;
			if (reloadCounter > 40)
			{
				(base.grasps[num3].grabbed as JokeRifle).ReloadRifle(base.grasps[num2].grabbed);
				base.mainBodyChunk.vel.y += 4f;
				room.PlaySound(SoundID.Gate_Clamp_Lock, base.mainBodyChunk, loop: false, 0.5f, 3f + UnityEngine.Random.value);
				AbstractPhysicalObject abstractPhysicalObject = base.grasps[num2].grabbed.abstractPhysicalObject;
				ReleaseGrasp(num2);
				abstractPhysicalObject.realizedObject.RemoveFromRoom();
				abstractPhysicalObject.Room.RemoveEntity(abstractPhysicalObject);
				reloadCounter = 0;
			}
		}
		else
		{
			reloadCounter = 0;
		}
		if (ModManager.MMF && base.mainBodyChunk.submersion >= 0.5f)
		{
			flag2 = false;
		}
		if (flag2)
		{
			if (craftingObject)
			{
				swallowAndRegurgitateCounter++;
				if (swallowAndRegurgitateCounter > 105)
				{
					SpitUpCraftedObject();
					swallowAndRegurgitateCounter = 0;
				}
			}
			else if (!ModManager.MMF || input[0].y == 0)
			{
				swallowAndRegurgitateCounter++;
				if ((objectInStomach != null || isGourmand || (ModManager.MSC && SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Spear)) && swallowAndRegurgitateCounter > 110)
				{
					bool flag5 = false;
					if (isGourmand && objectInStomach == null)
					{
						flag5 = true;
					}
					if (!flag5 || (flag5 && FoodInStomach >= 1))
					{
						if (flag5)
						{
							SubtractFood(1);
						}
						Regurgitate();
					}
					else
					{
						base.firstChunk.vel += new Vector2(UnityEngine.Random.Range(-1f, 1f), 0f);
						Stun(30);
					}
					if (spearOnBack != null)
					{
						spearOnBack.interactionLocked = true;
					}
					if ((ModManager.MSC || ModManager.CoopAvailable) && slugOnBack != null)
					{
						slugOnBack.interactionLocked = true;
					}
					swallowAndRegurgitateCounter = 0;
				}
				else if (objectInStomach == null && swallowAndRegurgitateCounter > 90)
				{
					for (int num14 = 0; num14 < 2; num14++)
					{
						if (base.grasps[num14] != null && CanBeSwallowed(base.grasps[num14].grabbed))
						{
							base.bodyChunks[0].pos += Custom.DirVec(base.grasps[num14].grabbed.firstChunk.pos, base.bodyChunks[0].pos) * 2f;
							SwallowObject(num14);
							if (spearOnBack != null)
							{
								spearOnBack.interactionLocked = true;
							}
							if ((ModManager.MSC || ModManager.CoopAvailable) && slugOnBack != null)
							{
								slugOnBack.interactionLocked = true;
							}
							swallowAndRegurgitateCounter = 0;
							(base.graphicsModule as PlayerGraphics).swallowing = 20;
							break;
						}
					}
				}
			}
			else
			{
				if (swallowAndRegurgitateCounter > 0)
				{
					swallowAndRegurgitateCounter--;
				}
				if (eatCounter > 0)
				{
					eatCounter--;
				}
			}
		}
		else
		{
			swallowAndRegurgitateCounter = 0;
		}
		for (int num15 = 0; num15 < base.grasps.Length; num15++)
		{
			if (base.grasps[num15] != null && base.grasps[num15].grabbed.slatedForDeletetion)
			{
				ReleaseGrasp(num15);
			}
		}
		if (base.grasps[0] != null && Grabability(base.grasps[0].grabbed) == ObjectGrabability.TwoHands)
		{
			pickUpCandidate = null;
		}
		else
		{
			PhysicalObject physicalObject = ((dontGrabStuff < 1) ? PickupCandidate(20f) : null);
			if (pickUpCandidate != physicalObject && physicalObject != null && physicalObject is PlayerCarryableItem)
			{
				(physicalObject as PlayerCarryableItem).Blink();
			}
			pickUpCandidate = physicalObject;
		}
		if (switchHandsCounter > 0)
		{
			switchHandsCounter--;
		}
		if (wantToPickUp > 0)
		{
			wantToPickUp--;
		}
		if (wantToThrow > 0)
		{
			wantToThrow--;
		}
		if (noPickUpOnRelease > 0)
		{
			noPickUpOnRelease--;
		}
		if (input[0].thrw && !input[1].thrw && (!ModManager.MSC || !monkAscension))
		{
			wantToThrow = 5;
		}
		if (wantToThrow > 0)
		{
			if (ModManager.MSC && MMF.cfgOldTongue.Value && base.grasps[0] == null && base.grasps[1] == null && SaintTongueCheck())
			{
				Vector2 vector4 = new Vector2(flipDirection, 0.7f).normalized;
				if (input[0].y > 0)
				{
					vector4 = new Vector2(0f, 1f);
				}
				vector4 = (vector4 + base.mainBodyChunk.vel.normalized * 0.2f).normalized;
				tongue.Shoot(vector4);
				wantToThrow = 0;
			}
			else
			{
				for (int num16 = 0; num16 < 2; num16++)
				{
					if (base.grasps[num16] != null && IsObjectThrowable(base.grasps[num16].grabbed))
					{
						ThrowObject(num16, eu);
						wantToThrow = 0;
						break;
					}
				}
			}
			if ((ModManager.MSC || ModManager.CoopAvailable) && wantToThrow > 0 && slugOnBack != null && slugOnBack.HasASlug)
			{
				Player slugcat = slugOnBack.slugcat;
				slugOnBack.SlugToHand(eu);
				ThrowObject(0, eu);
				float num17 = ((ThrowDirection >= 0) ? Mathf.Max(base.bodyChunks[0].pos.x, base.bodyChunks[1].pos.x) : Mathf.Min(base.bodyChunks[0].pos.x, base.bodyChunks[1].pos.x));
				for (int num18 = 0; num18 < slugcat.bodyChunks.Length; num18++)
				{
					slugcat.bodyChunks[num18].pos.y = base.firstChunk.pos.y + 20f;
					if (ThrowDirection < 0)
					{
						if (slugcat.bodyChunks[num18].pos.x > num17 - 8f)
						{
							slugcat.bodyChunks[num18].pos.x = num17 - 8f;
						}
						if (slugcat.bodyChunks[num18].vel.x > 0f)
						{
							slugcat.bodyChunks[num18].vel.x = 0f;
						}
					}
					else if (ThrowDirection > 0)
					{
						if (slugcat.bodyChunks[num18].pos.x < num17 + 8f)
						{
							slugcat.bodyChunks[num18].pos.x = num17 + 8f;
						}
						if (slugcat.bodyChunks[num18].vel.x < 0f)
						{
							slugcat.bodyChunks[num18].vel.x = 0f;
						}
					}
				}
			}
		}
		if (wantToPickUp <= 0)
		{
			return;
		}
		bool flag6 = true;
		if (animation == AnimationIndex.DeepSwim)
		{
			if (base.grasps[0] == null && base.grasps[1] == null)
			{
				flag6 = false;
			}
			else
			{
				for (int num19 = 0; num19 < 10; num19++)
				{
					if (input[num19].y > -1 || input[num19].x != 0)
					{
						flag6 = false;
						break;
					}
				}
			}
		}
		else
		{
			for (int num20 = 0; num20 < 5; num20++)
			{
				if (input[num20].y > -1)
				{
					flag6 = false;
					break;
				}
			}
		}
		if (ModManager.MSC)
		{
			if (base.grasps[0] != null && base.grasps[0].grabbed is EnergyCell && base.mainBodyChunk.submersion > 0f)
			{
				flag6 = false;
			}
			else if (base.grasps[0] != null && base.grasps[0].grabbed is EnergyCell && canJump <= 0 && bodyMode != BodyModeIndex.Crawl && bodyMode != BodyModeIndex.CorridorClimb && bodyMode != BodyModeIndex.ClimbIntoShortCut && animation != AnimationIndex.HangFromBeam && animation != AnimationIndex.ClimbOnBeam && animation != AnimationIndex.AntlerClimb && animation != AnimationIndex.VineGrab && animation != AnimationIndex.ZeroGPoleGrab)
			{
				(base.grasps[0].grabbed as EnergyCell).Use(forced: false);
			}
		}
		if (!ModManager.MMF && base.grasps[0] != null && HeavyCarry(base.grasps[0].grabbed))
		{
			flag6 = true;
		}
		if (flag6)
		{
			int num21 = -1;
			for (int num22 = 0; num22 < 2; num22++)
			{
				if (base.grasps[num22] != null)
				{
					num21 = num22;
					break;
				}
			}
			if (num21 > -1)
			{
				wantToPickUp = 0;
				if (!ModManager.MSC || SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Artificer || !(base.grasps[num21].grabbed is Scavenger))
				{
					pyroJumpDropLock = 0;
				}
				if (pyroJumpDropLock == 0 && (!ModManager.MSC || wantToJump == 0))
				{
					ReleaseObject(num21, eu);
				}
			}
			else if (spearOnBack != null && spearOnBack.spear != null && base.mainBodyChunk.ContactPoint.y < 0)
			{
				room.socialEventRecognizer.CreaturePutItemOnGround(spearOnBack.spear, this);
				spearOnBack.DropSpear();
			}
			else if ((ModManager.MSC || ModManager.CoopAvailable) && slugOnBack != null && slugOnBack.slugcat != null && base.mainBodyChunk.ContactPoint.y < 0)
			{
				room.socialEventRecognizer.CreaturePutItemOnGround(slugOnBack.slugcat, this);
				slugOnBack.DropSlug();
				wantToPickUp = 0;
			}
			else if (ModManager.MSC && room != null && room.game.IsStorySession && room.game.GetStorySession.saveState.wearingCloak && AI == null)
			{
				room.game.GetStorySession.saveState.wearingCloak = false;
				AbstractConsumable abstractConsumable = new AbstractConsumable(room.game.world, MoreSlugcatsEnums.AbstractObjectType.MoonCloak, null, room.GetWorldCoordinate(base.mainBodyChunk.pos), room.game.GetNewID(), -1, -1, null);
				room.abstractRoom.AddEntity(abstractConsumable);
				abstractConsumable.pos = base.abstractCreature.pos;
				abstractConsumable.RealizeInRoom();
				(abstractConsumable.realizedObject as MoonCloak).free = true;
				for (int num23 = 0; num23 < abstractConsumable.realizedObject.bodyChunks.Length; num23++)
				{
					abstractConsumable.realizedObject.bodyChunks[num23].HardSetPosition(base.mainBodyChunk.pos);
				}
				dontGrabStuff = 15;
				wantToPickUp = 0;
				noPickUpOnRelease = 20;
			}
		}
		else
		{
			if (pickUpCandidate == null)
			{
				return;
			}
			if (pickUpCandidate is Spear && CanPutSpearToBack && ((base.grasps[0] != null && Grabability(base.grasps[0].grabbed) >= ObjectGrabability.BigOneHand) || (base.grasps[1] != null && Grabability(base.grasps[1].grabbed) >= ObjectGrabability.BigOneHand) || (base.grasps[0] != null && base.grasps[1] != null)))
			{
				Custom.Log("spear straight to back");
				room.PlaySound(SoundID.Slugcat_Switch_Hands_Init, base.mainBodyChunk);
				spearOnBack.SpearToBack(pickUpCandidate as Spear);
			}
			else if (CanPutSlugToBack && pickUpCandidate is Player && (!(pickUpCandidate as Player).dead || CanIPutDeadSlugOnBack(pickUpCandidate as Player)) && ((base.grasps[0] != null && (Grabability(base.grasps[0].grabbed) > ObjectGrabability.BigOneHand || base.grasps[0].grabbed is Player)) || (base.grasps[1] != null && (Grabability(base.grasps[1].grabbed) > ObjectGrabability.BigOneHand || base.grasps[1].grabbed is Player)) || (base.grasps[0] != null && base.grasps[1] != null) || bodyMode == BodyModeIndex.Crawl))
			{
				Custom.Log("slugpup/player straight to back");
				room.PlaySound(SoundID.Slugcat_Switch_Hands_Init, base.mainBodyChunk);
				slugOnBack.SlugToBack(pickUpCandidate as Player);
			}
			else
			{
				int num24 = 0;
				for (int num25 = 0; num25 < 2; num25++)
				{
					if (base.grasps[num25] == null)
					{
						num24++;
					}
				}
				if (Grabability(pickUpCandidate) == ObjectGrabability.TwoHands && num24 < 4)
				{
					for (int num26 = 0; num26 < 2; num26++)
					{
						if (base.grasps[num26] != null)
						{
							ReleaseGrasp(num26);
						}
					}
				}
				else if (num24 == 0)
				{
					for (int num27 = 0; num27 < 2; num27++)
					{
						if (base.grasps[num27] != null && base.grasps[num27].grabbed is Fly)
						{
							ReleaseGrasp(num27);
							break;
						}
					}
				}
				for (int num28 = 0; num28 < 2; num28++)
				{
					if (base.grasps[num28] != null)
					{
						continue;
					}
					if (pickUpCandidate is Creature)
					{
						room.PlaySound(SoundID.Slugcat_Pick_Up_Creature, pickUpCandidate.firstChunk, loop: false, 1f, 1f);
					}
					else if (pickUpCandidate is PlayerCarryableItem)
					{
						for (int num29 = 0; num29 < pickUpCandidate.grabbedBy.Count; num29++)
						{
							if (pickUpCandidate.grabbedBy[num29].grabber.room == pickUpCandidate.grabbedBy[num29].grabbed.room)
							{
								pickUpCandidate.grabbedBy[num29].grabber.GrabbedObjectSnatched(pickUpCandidate.grabbedBy[num29].grabbed, this);
							}
							else
							{
								Custom.LogWarning($"Item theft room mismatch? {pickUpCandidate.grabbedBy[num29].grabbed.abstractPhysicalObject}");
							}
							pickUpCandidate.grabbedBy[num29].grabber.ReleaseGrasp(pickUpCandidate.grabbedBy[num29].graspUsed);
						}
						(pickUpCandidate as PlayerCarryableItem).PickedUp(this);
					}
					else
					{
						room.PlaySound(SoundID.Slugcat_Pick_Up_Misc_Inanimate, pickUpCandidate.firstChunk, loop: false, 1f, 1f);
					}
					SlugcatGrab(pickUpCandidate, num28);
					if (pickUpCandidate.graphicsModule != null && Grabability(pickUpCandidate) < (ObjectGrabability)5)
					{
						pickUpCandidate.graphicsModule.BringSpritesToFront();
					}
					break;
				}
			}
			wantToPickUp = 0;
		}
	}

	private bool IsObjectThrowable(PhysicalObject obj)
	{
		if (!(obj is VultureMask))
		{
			if (obj is TubeWorm)
			{
				if (ModManager.MMF && room != null)
				{
					return MMF.cfgOldTongue.Value;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public void SlugcatGrab(PhysicalObject obj, int graspUsed)
	{
		if (ModManager.MSC && obj is MoonCloak && AI == null && room != null && room.game.IsStorySession && room.abstractRoom.name != "SL_AI")
		{
			AbstractPhysicalObject abstractPhysicalObject = obj.abstractPhysicalObject;
			abstractPhysicalObject.realizedObject.RemoveFromRoom();
			abstractPhysicalObject.Room.RemoveEntity(abstractPhysicalObject);
			switchHandsCounter = 0;
			wantToPickUp = 0;
			noPickUpOnRelease = 20;
			room.game.GetStorySession.saveState.wearingCloak = true;
			return;
		}
		if (obj is IPlayerEdible && (!ModManager.MMF || (obj is Creature && (obj as Creature).dead) || !(obj is Centipede) || (obj is Centipede && (obj as Centipede).Small)))
		{
			Grab(obj, graspUsed, 0, Grasp.Shareability.CanOnlyShareWithNonExclusive, 0.5f, overrideEquallyDominant: false, pacifying: true);
		}
		int chunkGrabbed = 0;
		if (Grabability(obj) == ObjectGrabability.Drag)
		{
			float dst = float.MaxValue;
			for (int i = 0; i < obj.bodyChunks.Length; i++)
			{
				if (Custom.DistLess(base.mainBodyChunk.pos, obj.bodyChunks[i].pos, dst))
				{
					dst = Vector2.Distance(base.mainBodyChunk.pos, obj.bodyChunks[i].pos);
					chunkGrabbed = i;
				}
			}
		}
		switchHandsCounter = 0;
		wantToPickUp = 0;
		noPickUpOnRelease = 20;
		if (isSlugpup)
		{
			Custom.Log("Player slugpup grab limiter");
			if (base.grasps[0] != null)
			{
				ReleaseGrasp(0);
			}
			if (base.grasps[1] != null)
			{
				ReleaseGrasp(1);
			}
			graspUsed = 0;
		}
		bool flag = true;
		if (obj is Creature)
		{
			if (IsCreatureImmuneToPlayerGrabStun(obj as Creature))
			{
				flag = false;
			}
			else if (!(obj as Creature).dead && !IsCreatureLegalToHoldWithoutStun(obj as Creature))
			{
				flag = false;
			}
		}
		Grab(obj, graspUsed, chunkGrabbed, Grasp.Shareability.CanOnlyShareWithNonExclusive, 0.5f, overrideEquallyDominant: true, (ModManager.MMF || ModManager.CoopAvailable) ? flag : (!(obj is Cicada) && !(obj is JetFish)));
	}

	public bool CanIPickThisUp(PhysicalObject obj)
	{
		if (Grabability(obj) == ObjectGrabability.CantGrab)
		{
			return false;
		}
		if (ModManager.CoopAvailable && !Custom.rainWorld.options.friendlySteal && obj.grabbedBy.Any((Grasp x) => x.grabber is Player))
		{
			return false;
		}
		if (obj is Spear)
		{
			if ((obj as Spear).mode == Weapon.Mode.OnBack)
			{
				return false;
			}
			if (((obj as Spear).mode == Weapon.Mode.Free || (obj as Spear).mode == Weapon.Mode.StuckInCreature) && CanPutSpearToBack)
			{
				return true;
			}
		}
		int num = (int)Grabability(obj);
		if (num == 2)
		{
			for (int i = 0; i < 2; i++)
			{
				if (base.grasps[i] != null && Grabability(base.grasps[i].grabbed) > ObjectGrabability.OneHand)
				{
					return false;
				}
			}
		}
		if (obj is Player player && ((ModManager.MSC && player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Slugpup) || ModManager.CoopAvailable))
		{
			if (slugOnBack != null && slugOnBack.slugcat == obj)
			{
				return false;
			}
			if (onBack == player || player.onBack != null)
			{
				return false;
			}
			for (int j = 0; j < grabbedBy.Count; j++)
			{
				if (obj == grabbedBy[0].grabber)
				{
					return false;
				}
			}
			for (int k = 0; k < 2; k++)
			{
				if (base.grasps[k] != null && base.grasps[k].grabbed is Player && base.grasps[k].grabbed != obj)
				{
					return CanPutSlugToBack;
				}
			}
		}
		if (obj is Weapon)
		{
			if ((obj as Weapon).mode == Weapon.Mode.StuckInWall && (!ModManager.MMF || !MMF.cfgDislodgeSpears.Value) && (!ModManager.MSC || SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Artificer) && (!ModManager.MSC || !(obj is Spear) || !(obj as Spear).abstractSpear.electric))
			{
				return false;
			}
			if ((obj as Weapon).mode == Weapon.Mode.Thrown)
			{
				return false;
			}
			if ((obj as Weapon).forbiddenToPlayer > 0)
			{
				return false;
			}
		}
		int num2 = 0;
		for (int l = 0; l < 2; l++)
		{
			if (base.grasps[l] != null)
			{
				if (base.grasps[l].grabbed == obj)
				{
					return false;
				}
				if (Grabability(base.grasps[l].grabbed) > ObjectGrabability.OneHand)
				{
					num2++;
				}
			}
		}
		if (num2 == 2)
		{
			return false;
		}
		if (num2 > 0 && num > 2)
		{
			return false;
		}
		return true;
	}

	private ObjectGrabability Grabability(PhysicalObject obj)
	{
		if (obj is Weapon)
		{
			if (ModManager.Expedition && Custom.rainWorld.ExpeditionMode && ExpeditionGame.activeUnlocks.Contains("unl-dualwield"))
			{
				return ObjectGrabability.OneHand;
			}
			if (ModManager.MSC && SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Spear)
			{
				return ObjectGrabability.OneHand;
			}
			if (!(obj is Spear))
			{
				return ObjectGrabability.OneHand;
			}
			return ObjectGrabability.BigOneHand;
		}
		if (obj is DataPearl)
		{
			return ObjectGrabability.OneHand;
		}
		if (obj is Fly)
		{
			if (obj.grabbedBy.Count > 0 && obj.grabbedBy[0].grabber == this)
			{
				return ObjectGrabability.OneHand;
			}
			if ((obj as Fly).shortcutDelay != 0)
			{
				return ObjectGrabability.CantGrab;
			}
			return ObjectGrabability.OneHand;
		}
		if (obj is DangleFruit)
		{
			return ObjectGrabability.OneHand;
		}
		if (obj is EggBugEgg)
		{
			return ObjectGrabability.OneHand;
		}
		if (obj is VultureGrub)
		{
			return ObjectGrabability.OneHand;
		}
		if (obj is Hazer)
		{
			if (!(obj as Hazer).spraying)
			{
				return ObjectGrabability.OneHand;
			}
			return ObjectGrabability.CantGrab;
		}
		if (obj is JellyFish)
		{
			return ObjectGrabability.OneHand;
		}
		if (obj is SwollenWaterNut)
		{
			return ObjectGrabability.OneHand;
		}
		if (obj is OracleSwarmer)
		{
			return ObjectGrabability.OneHand;
		}
		if (obj is KarmaFlower)
		{
			return ObjectGrabability.OneHand;
		}
		if (obj is Mushroom)
		{
			return ObjectGrabability.OneHand;
		}
		if (obj is Snail)
		{
			return ObjectGrabability.TwoHands;
		}
		if (obj is Lantern)
		{
			return ObjectGrabability.OneHand;
		}
		if (obj is Cicada && !(obj as Cicada).Charging && ((obj as Cicada).cantPickUpCounter == 0 || (obj as Cicada).cantPickUpPlayer != this))
		{
			return ObjectGrabability.TwoHands;
		}
		if (obj is JetFish && (obj as JetFish).grabable > 0)
		{
			return ObjectGrabability.TwoHands;
		}
		if (obj is LanternMouse)
		{
			return ObjectGrabability.TwoHands;
		}
		if (obj is EggBug)
		{
			return ObjectGrabability.TwoHands;
		}
		if (obj is TubeWorm)
		{
			if (ModManager.MMF && room != null && MMF.cfgOldTongue.Value)
			{
				return ObjectGrabability.TwoHands;
			}
			for (int i = 0; i < 2; i++)
			{
				if (base.grasps[i] != null && base.grasps[i].grabbed is TubeWorm)
				{
					return ObjectGrabability.CantGrab;
				}
			}
			return ObjectGrabability.OneHand;
		}
		if (obj is JokeRifle)
		{
			if (!ModManager.MSC)
			{
				return ObjectGrabability.OneHand;
			}
			return ObjectGrabability.BigOneHand;
		}
		if (obj is Centipede && (obj as Centipede).Small)
		{
			return ObjectGrabability.OneHand;
		}
		if (obj is Creature && !(obj as Creature).Template.smallCreature && ((obj as Creature).dead || (SlugcatStats.SlugcatCanMaul(SlugCatClass) && dontGrabStuff < 1 && obj != this && !(obj as Creature).Consious)))
		{
			return ObjectGrabability.Drag;
		}
		if (obj is VultureMask)
		{
			return ObjectGrabability.OneHand;
		}
		if (obj is SlimeMold)
		{
			if (!ModManager.MSC || !(obj as SlimeMold).JellyfishMode)
			{
				return ObjectGrabability.OneHand;
			}
			return ObjectGrabability.CantGrab;
		}
		if (obj is FlyLure)
		{
			return ObjectGrabability.OneHand;
		}
		if (obj is SmallNeedleWorm)
		{
			return ObjectGrabability.OneHand;
		}
		if (obj is NeedleEgg)
		{
			return ObjectGrabability.OneHand;
		}
		if (obj is BubbleGrass)
		{
			return ObjectGrabability.OneHand;
		}
		if (obj is NSHSwarmer)
		{
			return ObjectGrabability.OneHand;
		}
		if (obj is OverseerCarcass)
		{
			return ObjectGrabability.OneHand;
		}
		if (ModManager.MSC)
		{
			if (obj is GooieDuck)
			{
				return ObjectGrabability.OneHand;
			}
			if (obj is MoonCloak)
			{
				return ObjectGrabability.OneHand;
			}
			if (obj is BigJellyFish)
			{
				return ObjectGrabability.CantGrab;
			}
			if (obj is DandelionPeach)
			{
				return ObjectGrabability.OneHand;
			}
			if (obj is Yeek)
			{
				return ObjectGrabability.TwoHands;
			}
			if (obj is GlowWeed)
			{
				return ObjectGrabability.OneHand;
			}
			if (obj is FireEgg)
			{
				return ObjectGrabability.OneHand;
			}
			if (obj is EnergyCell)
			{
				if ((obj as EnergyCell).allowPickup)
				{
					return ObjectGrabability.TwoHands;
				}
				return ObjectGrabability.CantGrab;
			}
			if (obj is Player && obj != this && !isSlugpup && (obj as Player).SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Slugpup && !(obj as Player).playerState.forceFullGrown)
			{
				return ObjectGrabability.OneHand;
			}
		}
		if (ModManager.CoopAvailable && obj is Player player && player != this && !player.standing && !isSlugpup)
		{
			PlayerState obj2 = playerState;
			if (obj2 == null || !obj2.isGhost)
			{
				JollyPlayerOptions jollyOption = player.JollyOption;
				if (jollyOption != null && jollyOption.isPup)
				{
					return ObjectGrabability.OneHand;
				}
				return ObjectGrabability.BigOneHand;
			}
		}
		return ObjectGrabability.CantGrab;
	}

	public bool HeavyCarry(PhysicalObject obj)
	{
		if (Grabability(obj) == ObjectGrabability.Drag)
		{
			return true;
		}
		if (Grabability(obj) != ObjectGrabability.TwoHands && !(obj.TotalMass > base.TotalMass * 0.6f))
		{
			if (ModManager.CoopAvailable && obj is Player player)
			{
				return !player.isSlugpup;
			}
			return false;
		}
		return true;
	}

	public int FreeHand()
	{
		if (base.grasps[0] != null && HeavyCarry(base.grasps[0].grabbed))
		{
			return -1;
		}
		for (int i = 0; i < base.grasps.Length; i++)
		{
			if (base.grasps[i] == null && (!isSlugpup || i != 1))
			{
				return i;
			}
		}
		return -1;
	}

	public void ThrowObject(int grasp, bool eu)
	{
		if (base.grasps[grasp] == null || base.grasps[grasp].grabbed is JokeRifle)
		{
			return;
		}
		if (ModManager.MSC && SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Gourmand && base.grasps[grasp].grabbed is Spear)
		{
			aerobicLevel = 1f;
		}
		else
		{
			AerobicIncrease(0.75f);
		}
		if (ModManager.MMF && room != null && MMF.cfgOldTongue.Value && base.grasps[grasp].grabbed is TubeWorm)
		{
			(base.grasps[grasp].grabbed as TubeWorm).Use();
			return;
		}
		if (base.grasps[grasp].grabbed is Weapon)
		{
			IntVector2 throwDir = new IntVector2(ThrowDirection, 0);
			bool flag = input[0].y < 0;
			if (ModManager.MMF && MMF.cfgUpwardsSpearThrow.Value)
			{
				flag = input[0].y != 0;
			}
			if (animation == AnimationIndex.Flip && flag && input[0].x == 0)
			{
				throwDir = new IntVector2(0, (ModManager.MMF && MMF.cfgUpwardsSpearThrow.Value) ? input[0].y : (-1));
			}
			if (ModManager.MMF && bodyMode == BodyModeIndex.ZeroG && MMF.cfgUpwardsSpearThrow.Value)
			{
				int y = input[0].y;
				throwDir = ((y == 0) ? new IntVector2(ThrowDirection, 0) : new IntVector2(0, y));
			}
			Vector2 vector = base.firstChunk.pos + throwDir.ToVector2() * 10f + new Vector2(0f, 4f);
			if (room.GetTile(vector).Solid)
			{
				vector = base.mainBodyChunk.pos;
			}
			if (ModManager.MSC && base.grasps[grasp].grabbed is Spear && (base.grasps[grasp].grabbed as Spear).bugSpear)
			{
				(base.grasps[grasp].grabbed as Weapon).Thrown(this, vector, base.mainBodyChunk.pos - throwDir.ToVector2() * 10f, throwDir, Mathf.Lerp(1f, 1.5f, Adrenaline), eu);
			}
			else if (ModManager.MSC && base.grasps[grasp].grabbed is Spear && SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint && (!ModManager.Expedition || (ModManager.Expedition && !room.game.rainWorld.ExpeditionMode)))
			{
				TossObject(grasp, eu);
			}
			else if (ModManager.MSC && ((SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint && !(base.grasps[grasp].grabbed is Rock)) || gourmandExhausted))
			{
				(base.grasps[grasp].grabbed as Weapon).Thrown(this, vector, base.mainBodyChunk.pos - throwDir.ToVector2() * 10f, throwDir, Mathf.Lerp(0.5f, 0.75f, Adrenaline), eu);
			}
			else
			{
				(base.grasps[grasp].grabbed as Weapon).Thrown(this, vector, base.mainBodyChunk.pos - throwDir.ToVector2() * 10f, throwDir, Mathf.Lerp(1f, 1.5f, Adrenaline), eu);
			}
			if (ModManager.MMF && MMF.cfgUpwardsSpearThrow.Value && base.grasps[grasp].grabbed is ScavengerBomb && throwDir.y == 1 && bodyMode != BodyModeIndex.ZeroG)
			{
				(base.grasps[grasp].grabbed as ScavengerBomb).doNotTumbleAtLowSpeed = true;
				(base.grasps[grasp].grabbed as ScavengerBomb).throwModeFrames = 90;
				(base.grasps[grasp].grabbed as ScavengerBomb).firstChunk.vel *= 0.75f;
			}
			if (base.grasps[grasp].grabbed is Spear)
			{
				ThrownSpear(base.grasps[grasp].grabbed as Spear);
			}
			if (animation == AnimationIndex.BellySlide && rollCounter > 8 && rollCounter < 15)
			{
				if (throwDir.x == rollDirection && slugcatStats.throwingSkill > 0)
				{
					base.grasps[grasp].grabbed.firstChunk.vel.x += (float)throwDir.x * 15f;
					if ((base.grasps[grasp].grabbed as Weapon).HeavyWeapon)
					{
						(base.grasps[grasp].grabbed as Weapon).floorBounceFrames = 30;
						if (base.grasps[grasp].grabbed is Spear)
						{
							(base.grasps[grasp].grabbed as Spear).alwaysStickInWalls = true;
						}
						base.grasps[grasp].grabbed.firstChunk.goThroughFloors = false;
						base.grasps[grasp].grabbed.firstChunk.vel.y -= 5f;
					}
					(base.grasps[grasp].grabbed as Weapon).changeDirCounter = 0;
				}
				else if (throwDir.x == -rollDirection && !longBellySlide)
				{
					base.grasps[grasp].grabbed.firstChunk.vel.y += ((base.grasps[grasp].grabbed is Spear) ? 3f : 5f);
					(base.grasps[grasp].grabbed as Weapon).changeDirCounter = 0;
					rollCounter = 8;
					base.mainBodyChunk.pos.x += (float)rollDirection * 6f;
					room.AddObject(new ExplosionSpikes(room, base.bodyChunks[1].pos + new Vector2((float)rollDirection * -40f, 0f), 6, 5.5f, 4f, 4.5f, 21f, new Color(1f, 1f, 1f, 0.25f)));
					base.bodyChunks[1].pos.x += (float)rollDirection * 6f;
					base.bodyChunks[1].pos.y += 17f;
					base.mainBodyChunk.vel.x += (float)rollDirection * 16f;
					base.bodyChunks[1].vel.x += (float)rollDirection * 16f;
					room.PlaySound(SoundID.Slugcat_Rocket_Jump, base.mainBodyChunk, loop: false, 1f, 1f);
					exitBellySlideCounter = 0;
					longBellySlide = true;
				}
			}
			if (animation == AnimationIndex.ClimbOnBeam && ModManager.MMF && MMF.cfgClimbingGrip.Value)
			{
				base.bodyChunks[0].vel += throwDir.ToVector2() * 2f;
				base.bodyChunks[1].vel -= throwDir.ToVector2() * 8f;
			}
			else
			{
				base.bodyChunks[0].vel += throwDir.ToVector2() * 8f;
				base.bodyChunks[1].vel -= throwDir.ToVector2() * 4f;
			}
			if (base.graphicsModule != null)
			{
				(base.graphicsModule as PlayerGraphics).ThrowObject(grasp, base.grasps[grasp].grabbed);
			}
			Blink(15);
		}
		else
		{
			TossObject(grasp, eu);
		}
		dontGrabStuff = (isNPC ? 45 : 15);
		if (base.graphicsModule != null)
		{
			(base.graphicsModule as PlayerGraphics).LookAtObject(base.grasps[grasp].grabbed);
		}
		if (base.grasps[grasp].grabbed is PlayerCarryableItem)
		{
			(base.grasps[grasp].grabbed as PlayerCarryableItem).Forbid();
		}
		ReleaseGrasp(grasp);
	}

	public void ThrowToGetFree(bool eu)
	{
		int num = -1;
		for (int i = 0; i < base.grasps.Length; i++)
		{
			if (base.grasps[i] != null && base.grasps[i].grabbed is Spear)
			{
				num = i;
				break;
			}
		}
		if (num < 0)
		{
			for (int j = 0; j < base.grasps.Length; j++)
			{
				if (base.grasps[j] != null && base.grasps[j].grabbed is Weapon)
				{
					num = j;
					break;
				}
			}
		}
		if (num < 0)
		{
			for (int k = 0; k < base.grasps.Length; k++)
			{
				if (base.grasps[k] != null)
				{
					TossObject(k, eu);
					ReleaseGrasp(k);
					break;
				}
			}
			return;
		}
		Weapon weapon = base.grasps[num].grabbed as Weapon;
		if (dangerGrasp == null || (!(weapon is Spear) && UnityEngine.Random.value < 0.5f))
		{
			ThrowObject(num, eu);
			return;
		}
		BodyChunk bodyChunk = null;
		float dst = float.MaxValue;
		for (int l = 0; l < dangerGrasp.grabber.bodyChunks.Length; l++)
		{
			if (Custom.DistLess(base.mainBodyChunk.pos, dangerGrasp.grabber.bodyChunks[l].pos, dst))
			{
				bodyChunk = dangerGrasp.grabber.bodyChunks[l];
				dst = Vector2.Distance(base.mainBodyChunk.pos, dangerGrasp.grabber.bodyChunks[l].pos);
			}
		}
		if (bodyChunk == null)
		{
			return;
		}
		weapon.Thrown(this, base.mainBodyChunk.pos, base.mainBodyChunk.pos, new IntVector2((base.mainBodyChunk.pos.x < bodyChunk.pos.x) ? 1 : (-1), 0), 1f, eu);
		if (weapon is Spear)
		{
			if (ModManager.MSC && SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint)
			{
				if (ModManager.Expedition && room.game.rainWorld.ExpeditionMode)
				{
					ThrownSpear(weapon as Spear);
				}
				else
				{
					TossObject(num, eu);
				}
			}
			else
			{
				ThrownSpear(weapon as Spear);
			}
		}
		weapon.meleeHitChunk = bodyChunk;
		ReleaseGrasp(num);
	}

	public void DangerGraspPickup(bool eu)
	{
		int num = -1;
		for (int i = 0; i < base.grasps.Length; i++)
		{
			if (base.grasps[i] == null)
			{
				num = i;
			}
			else if (base.grasps[i].grabbed is Spear)
			{
				return;
			}
		}
		if (num < 0)
		{
			return;
		}
		if (spearOnBack != null && spearOnBack.HasASpear)
		{
			spearOnBack.SpearToHand(eu);
			return;
		}
		PhysicalObject physicalObject = PickupCandidate(100f);
		if (physicalObject != null)
		{
			SlugcatGrab(physicalObject, num);
			if (physicalObject is PlayerCarryableItem)
			{
				(physicalObject as PlayerCarryableItem).PickedUp(this);
			}
		}
	}

	private PhysicalObject PickupCandidate(float favorSpears)
	{
		PhysicalObject result = null;
		float num = float.MaxValue;
		for (int i = 0; i < room.physicalObjects.Length; i++)
		{
			for (int j = 0; j < room.physicalObjects[i].Count; j++)
			{
				if ((!(room.physicalObjects[i][j] is PlayerCarryableItem) || (room.physicalObjects[i][j] as PlayerCarryableItem).forbiddenToPlayer < 1) && Custom.DistLess(base.bodyChunks[0].pos, room.physicalObjects[i][j].bodyChunks[0].pos, room.physicalObjects[i][j].bodyChunks[0].rad + 40f) && (Custom.DistLess(base.bodyChunks[0].pos, room.physicalObjects[i][j].bodyChunks[0].pos, room.physicalObjects[i][j].bodyChunks[0].rad + 20f) || room.VisualContact(base.bodyChunks[0].pos, room.physicalObjects[i][j].bodyChunks[0].pos)) && CanIPickThisUp(room.physicalObjects[i][j]))
				{
					float num2 = Vector2.Distance(base.bodyChunks[0].pos, room.physicalObjects[i][j].bodyChunks[0].pos);
					if (room.physicalObjects[i][j] is Spear)
					{
						num2 -= favorSpears;
					}
					if (room.physicalObjects[i][j].bodyChunks[0].pos.x < base.bodyChunks[0].pos.x == flipDirection < 0)
					{
						num2 -= 10f;
					}
					if (num2 < num)
					{
						result = room.physicalObjects[i][j];
						num = num2;
					}
				}
			}
		}
		return result;
	}

	private void ThrownSpear(Spear spear)
	{
		if (slugcatStats.throwingSkill == 0)
		{
			spear.throwModeFrames = 18;
			spear.spearDamageBonus = 0.6f + 0.3f * Mathf.Pow(UnityEngine.Random.value, 4f);
			spear.firstChunk.vel.x *= 0.77f;
		}
		else if (ModManager.MMF && MMF.cfgUpwardsSpearThrow.Value && spear.setRotation.Value.y == 1f && bodyMode != BodyModeIndex.ZeroG)
		{
			spear.spearDamageBonus = 0.8f;
			spear.firstChunk.vel.y *= 0.87f;
		}
		else if (slugcatStats.throwingSkill == 2)
		{
			if (ModManager.MSC && SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
			{
				spear.spearDamageBonus = 3f;
				if (!gourmandExhausted)
				{
					if (canJump != 0)
					{
						animation = AnimationIndex.Roll;
					}
					else
					{
						animation = AnimationIndex.Flip;
					}
					if ((room != null && room.gravity == 0f) || Mathf.Abs(spear.firstChunk.vel.x) < 1f)
					{
						base.firstChunk.vel += spear.firstChunk.vel.normalized * 9f;
					}
					else
					{
						rollDirection = (int)Mathf.Sign(spear.firstChunk.vel.x);
						rollCounter = 0;
						base.firstChunk.vel.x += Mathf.Sign(spear.firstChunk.vel.x) * 9f;
					}
					gourmandAttackNegateTime = 80;
				}
			}
			else
			{
				spear.spearDamageBonus = 1.25f;
			}
			spear.firstChunk.vel.x *= 1.2f;
		}
		if (ModManager.MSC && gourmandExhausted)
		{
			spear.spearDamageBonus = 0.3f;
		}
	}

	private void TossObject(int grasp, bool eu)
	{
		if (base.grasps[grasp].grabbed is Creature)
		{
			room.PlaySound(SoundID.Slugcat_Throw_Creature, base.grasps[grasp].grabbedChunk, loop: false, 1f, 1f);
		}
		else
		{
			room.PlaySound(SoundID.Slugcat_Throw_Misc_Inanimate, base.grasps[grasp].grabbedChunk, loop: false, 1f, 1f);
		}
		PhysicalObject grabbed = base.grasps[grasp].grabbed;
		float num = 45f;
		float num2 = 4f;
		if (input[0].x != 0 && input[0].y == 0)
		{
			num = Custom.LerpMap(grabbed.TotalMass, 0.2f, 0.3f, 60f, 50f);
			num2 = Custom.LerpMap(grabbed.TotalMass, 0.2f, 0.3f, 12.5f, 8f, 2f);
		}
		else if (input[0].x != 0 && input[0].y == 1)
		{
			num = 25f;
			num2 = 9f;
		}
		else if (input[0].x == 0 && input[0].y == 1)
		{
			num = 5f;
			num2 = 8f;
		}
		if (Grabability(grabbed) == ObjectGrabability.OneHand)
		{
			num2 *= 2f;
			if (input[0].x != 0 && input[0].y == 0)
			{
				num = 70f;
			}
		}
		if (animation == AnimationIndex.Flip && input[0].y < 0 && input[0].x == 0)
		{
			num = 180f;
			num2 = 8f;
			for (int i = 0; i < grabbed.bodyChunks.Length; i++)
			{
				grabbed.bodyChunks[i].goThroughFloors = true;
			}
		}
		if (grabbed is PlayerCarryableItem)
		{
			num2 *= (grabbed as PlayerCarryableItem).ThrowPowerFactor;
		}
		if (grabbed is JellyFish)
		{
			(grabbed as JellyFish).Tossed(this);
		}
		if (ModManager.MSC && grabbed is Player)
		{
			num2 *= 0.35f;
		}
		if (ModManager.MSC && grabbed is FireEgg)
		{
			(grabbed as FireEgg).Tossed(this);
		}
		else if (grabbed is VultureGrub)
		{
			(grabbed as VultureGrub).InitiateSignalCountDown();
			num2 *= 0.5f;
		}
		else if (grabbed is Hazer)
		{
			(grabbed as Hazer).tossed = true;
			num2 = Mathf.Max(num2, 9f);
		}
		if (grabbed.TotalMass < base.TotalMass * 2f && ThrowDirection != 0)
		{
			float num3 = ((ThrowDirection < 0) ? Mathf.Min(base.bodyChunks[0].pos.x, base.bodyChunks[1].pos.x) : Mathf.Max(base.bodyChunks[0].pos.x, base.bodyChunks[1].pos.x));
			for (int j = 0; j < grabbed.bodyChunks.Length; j++)
			{
				if (ThrowDirection < 0)
				{
					if (grabbed.bodyChunks[j].pos.x > num3 - 8f)
					{
						grabbed.bodyChunks[j].pos.x = num3 - 8f;
					}
					if (grabbed.bodyChunks[j].vel.x > 0f)
					{
						grabbed.bodyChunks[j].vel.x = 0f;
					}
				}
				else if (ThrowDirection > 0)
				{
					if (grabbed.bodyChunks[j].pos.x < num3 + 8f)
					{
						grabbed.bodyChunks[j].pos.x = num3 + 8f;
					}
					if (grabbed.bodyChunks[j].vel.x < 0f)
					{
						grabbed.bodyChunks[j].vel.x = 0f;
					}
				}
			}
		}
		if (!HeavyCarry(grabbed) && grabbed.TotalMass < base.TotalMass * 0.75f)
		{
			for (int k = 0; k < grabbed.bodyChunks.Length; k++)
			{
				grabbed.bodyChunks[k].pos.y = base.mainBodyChunk.pos.y;
			}
		}
		if (Grabability(grabbed) == ObjectGrabability.Drag)
		{
			base.grasps[grasp].grabbedChunk.vel += Custom.DegToVec(num * (float)ThrowDirection) * num2 / Mathf.Max(0.5f, base.grasps[grasp].grabbedChunk.mass);
		}
		else
		{
			for (int l = 0; l < grabbed.bodyChunks.Length; l++)
			{
				if (grabbed.bodyChunks[l].vel.y < 0f)
				{
					grabbed.bodyChunks[l].vel.y = 0f;
				}
				grabbed.bodyChunks[l].vel = Vector2.Lerp(grabbed.bodyChunks[l].vel * 0.35f, base.mainBodyChunk.vel, Custom.LerpMap(grabbed.TotalMass, 0.2f, 0.5f, 0.6f, 0.3f));
				grabbed.bodyChunks[l].vel += Custom.DegToVec(num * (float)ThrowDirection) * Mathf.Clamp(num2 / (Mathf.Lerp(grabbed.TotalMass, 0.4f, 0.2f) * (float)grabbed.bodyChunks.Length), 4f, 14f);
				if (num2 > 4f && grabbed is LanternMouse)
				{
					(grabbed as LanternMouse).Stun(20);
				}
			}
		}
		if (base.grasps[grasp].grabbed is Snail)
		{
			(base.grasps[grasp].grabbed as Snail).triggerTicker = 40;
		}
		room.socialEventRecognizer.CreaturePutItemOnGround(base.grasps[grasp].grabbed, this);
	}

	private void ReleaseObject(int grasp, bool eu)
	{
		room.PlaySound((base.grasps[grasp].grabbed is Creature) ? SoundID.Slugcat_Lay_Down_Creature : SoundID.Slugcat_Lay_Down_Object, base.grasps[grasp].grabbedChunk, loop: false, 1f, 1f);
		room.socialEventRecognizer.CreaturePutItemOnGround(base.grasps[grasp].grabbed, this);
		if (base.grasps[grasp].grabbed is PlayerCarryableItem)
		{
			(base.grasps[grasp].grabbed as PlayerCarryableItem).Forbid();
		}
		ReleaseGrasp(grasp);
	}

	public bool CanBeSwallowed(PhysicalObject testObj)
	{
		if (ModManager.MSC && SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Spear)
		{
			return false;
		}
		if (testObj is Rock)
		{
			return true;
		}
		if (testObj is DataPearl)
		{
			return true;
		}
		if (testObj is FlareBomb)
		{
			return true;
		}
		if (testObj is Lantern)
		{
			return true;
		}
		if (testObj is FirecrackerPlant)
		{
			return true;
		}
		if (testObj is VultureGrub && !(testObj as VultureGrub).dead)
		{
			return true;
		}
		if (testObj is Hazer && !(testObj as Hazer).dead && !(testObj as Hazer).hasSprayed)
		{
			return true;
		}
		if (testObj is FlyLure)
		{
			return true;
		}
		if (testObj is ScavengerBomb)
		{
			return true;
		}
		if (testObj is PuffBall)
		{
			return true;
		}
		if (testObj is SporePlant)
		{
			return true;
		}
		if (testObj is BubbleGrass)
		{
			return true;
		}
		if (testObj is SSOracleSwarmer && FoodInStomach >= MaxFoodInStomach)
		{
			return true;
		}
		if (testObj is NSHSwarmer)
		{
			return true;
		}
		if (testObj is OverseerCarcass)
		{
			return true;
		}
		if (ModManager.MSC && testObj is FireEgg && FoodInStomach >= MaxFoodInStomach)
		{
			return true;
		}
		if (ModManager.MSC && testObj is SingularityBomb)
		{
			if (!(testObj as SingularityBomb).activateSingularity)
			{
				return !(testObj as SingularityBomb).activateSucktion;
			}
			return false;
		}
		return false;
	}

	public bool EatMeatOmnivoreGreenList(Creature crit)
	{
		if (!(crit.Template.type == CreatureTemplate.Type.Centipede) && !(crit.Template.type == CreatureTemplate.Type.Centiwing) && (!ModManager.MSC || !(crit.Template.type == MoreSlugcatsEnums.CreatureTemplateType.AquaCenti)))
		{
			return crit.Template.type == CreatureTemplate.Type.RedCentipede;
		}
		return true;
	}

	public bool CanEatMeat(Creature crit)
	{
		if (ModManager.MSC && (SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint || SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Spear))
		{
			return false;
		}
		if (EatMeatOmnivoreGreenList(crit) && crit.dead)
		{
			if (ModManager.MSC)
			{
				return pyroJumpCooldown <= 60f;
			}
			return true;
		}
		if (crit is IPlayerEdible)
		{
			return false;
		}
		if (!crit.dead)
		{
			return false;
		}
		if (slugcatStats.name == SlugcatStats.Name.Red || (ModManager.MSC && (slugcatStats.name == MoreSlugcatsEnums.SlugcatStatsName.Artificer || slugcatStats.name == MoreSlugcatsEnums.SlugcatStatsName.Gourmand || slugcatStats.name == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel)))
		{
			if (ModManager.CoopAvailable && crit is Player)
			{
				return false;
			}
			if (ModManager.MSC)
			{
				return pyroJumpCooldown <= 60f;
			}
			return true;
		}
		return false;
	}

	public void SwallowObject(int grasp)
	{
		if (grasp < 0 || base.grasps[grasp] == null)
		{
			return;
		}
		AbstractPhysicalObject abstractPhysicalObject = base.grasps[grasp].grabbed.abstractPhysicalObject;
		if (abstractPhysicalObject is AbstractSpear)
		{
			(abstractPhysicalObject as AbstractSpear).stuckInWallCycles = 0;
		}
		objectInStomach = abstractPhysicalObject;
		if (ModManager.MMF && room.game.session is StoryGameSession)
		{
			(room.game.session as StoryGameSession).RemovePersistentTracker(objectInStomach);
		}
		ReleaseGrasp(grasp);
		objectInStomach.realizedObject.RemoveFromRoom();
		objectInStomach.Abstractize(base.abstractCreature.pos);
		objectInStomach.Room.RemoveEntity(objectInStomach);
		if (ModManager.MSC && SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer && FoodInStomach > 0)
		{
			if (abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.Rock)
			{
				abstractPhysicalObject = new AbstractPhysicalObject(room.world, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null, room.GetWorldCoordinate(base.mainBodyChunk.pos), room.game.GetNewID());
				SubtractFood(1);
			}
			else if (abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.Spear && !(abstractPhysicalObject as AbstractSpear).explosive && !(abstractPhysicalObject as AbstractSpear).electric)
			{
				abstractPhysicalObject = new AbstractSpear(room.world, null, room.GetWorldCoordinate(base.mainBodyChunk.pos), room.game.GetNewID(), explosive: true);
				SubtractFood(1);
			}
			else if (abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.FlyLure)
			{
				abstractPhysicalObject = new AbstractConsumable(room.world, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant, null, room.GetWorldCoordinate(base.mainBodyChunk.pos), room.game.GetNewID(), -1, -1, null);
				SubtractFood(1);
			}
			else if (abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.FlareBomb)
			{
				abstractPhysicalObject = new AbstractPhysicalObject(room.world, AbstractPhysicalObject.AbstractObjectType.Lantern, null, room.GetWorldCoordinate(base.mainBodyChunk.pos), room.game.GetNewID());
				SubtractFood(1);
			}
		}
		objectInStomach = abstractPhysicalObject;
		objectInStomach.Abstractize(base.abstractCreature.pos);
		base.mainBodyChunk.vel.y += 2f;
		room.PlaySound(SoundID.Slugcat_Swallow_Item, base.mainBodyChunk);
	}

	public void Regurgitate()
	{
		if (ModManager.MSC && SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Spear)
		{
			objectInStomach = new SpearMasterPearl.AbstractSpearMasterPearl(room.world, null, room.GetWorldCoordinate(base.mainBodyChunk.pos), room.game.GetNewID(), -1, -1, null);
		}
		if (objectInStomach == null)
		{
			if (!isGourmand)
			{
				return;
			}
			objectInStomach = GourmandCombos.RandomStomachItem(this);
		}
		room.abstractRoom.AddEntity(objectInStomach);
		objectInStomach.pos = base.abstractCreature.pos;
		objectInStomach.RealizeInRoom();
		if (ModManager.MMF && MMF.cfgKeyItemTracking.Value && AbstractPhysicalObject.UsesAPersistantTracker(objectInStomach) && room.game.IsStorySession)
		{
			(room.game.session as StoryGameSession).AddNewPersistentTracker(objectInStomach);
			if (room.abstractRoom.NOTRACKERS)
			{
				objectInStomach.tracker.lastSeenRegion = lastGoodTrackerSpawnRegion;
				objectInStomach.tracker.lastSeenRoom = lastGoodTrackerSpawnRoom;
				objectInStomach.tracker.ChangeDesiredSpawnLocation(lastGoodTrackerSpawnCoord);
			}
		}
		Vector2 pos = base.bodyChunks[0].pos;
		Vector2 vector = Custom.DirVec(base.bodyChunks[1].pos, base.bodyChunks[0].pos);
		bool flag = false;
		if (Mathf.Abs(base.bodyChunks[0].pos.y - base.bodyChunks[1].pos.y) > Mathf.Abs(base.bodyChunks[0].pos.x - base.bodyChunks[1].pos.x) && base.bodyChunks[0].pos.y > base.bodyChunks[1].pos.y)
		{
			pos += Custom.DirVec(base.bodyChunks[1].pos, base.bodyChunks[0].pos) * 5f;
			vector *= -1f;
			vector.x += 0.4f * (float)flipDirection;
			vector.Normalize();
			flag = true;
		}
		objectInStomach.realizedObject.firstChunk.HardSetPosition(pos);
		objectInStomach.realizedObject.firstChunk.vel = Vector2.ClampMagnitude((vector * 2f + Custom.RNV() * UnityEngine.Random.value) / objectInStomach.realizedObject.firstChunk.mass, 6f);
		base.bodyChunks[0].pos -= vector * 2f;
		base.bodyChunks[0].vel -= vector * 2f;
		if (base.graphicsModule != null)
		{
			(base.graphicsModule as PlayerGraphics).head.vel += Custom.RNV() * UnityEngine.Random.value * 3f;
		}
		for (int i = 0; i < 3; i++)
		{
			room.AddObject(new WaterDrip(pos + Custom.RNV() * UnityEngine.Random.value * 1.5f, Custom.RNV() * 3f * UnityEngine.Random.value + vector * Mathf.Lerp(2f, 6f, UnityEngine.Random.value), waterColor: false));
		}
		room.PlaySound(SoundID.Slugcat_Regurgitate_Item, base.mainBodyChunk);
		if (objectInStomach.realizedObject is Hazer && base.graphicsModule != null)
		{
			(objectInStomach.realizedObject as Hazer).SpitOutByPlayer(PlayerGraphics.SlugcatColor(playerState.slugcatCharacter));
		}
		if (flag && FreeHand() > -1)
		{
			if (ModManager.MMF && ((base.grasps[0] != null) ^ (base.grasps[1] != null)) && Grabability(objectInStomach.realizedObject) == ObjectGrabability.BigOneHand)
			{
				int num = 0;
				if (FreeHand() == 0)
				{
					num = 1;
				}
				if (Grabability(base.grasps[num].grabbed) != ObjectGrabability.BigOneHand)
				{
					SlugcatGrab(objectInStomach.realizedObject, FreeHand());
				}
			}
			else
			{
				SlugcatGrab(objectInStomach.realizedObject, FreeHand());
			}
		}
		objectInStomach = null;
	}

	public void EatMeatUpdate(int graspIndex)
	{
		if (base.grasps[graspIndex] == null || !(base.grasps[graspIndex].grabbed is Creature) || eatMeat <= 20)
		{
			return;
		}
		if (ModManager.MSC)
		{
			if ((base.grasps[graspIndex].grabbed as Creature).abstractCreature.creatureTemplate.type == CreatureTemplate.Type.Scavenger)
			{
				base.grasps[graspIndex].grabbed.bodyChunks[0].mass = 0.5f;
				base.grasps[graspIndex].grabbed.bodyChunks[1].mass = 0.3f;
				base.grasps[graspIndex].grabbed.bodyChunks[2].mass = 0.05f;
			}
			if (SlugcatStats.SlugcatCanMaul(SlugCatClass) && base.grasps[graspIndex].grabbed is Vulture && base.grasps[graspIndex].grabbedChunk.index == 4 && ((base.grasps[graspIndex].grabbed as Vulture).abstractCreature.state as Vulture.VultureState).mask)
			{
				Custom.Log("Vulture mask forced off by artificer eating head");
				(base.grasps[graspIndex].grabbed as Vulture).DropMask(Custom.RNV());
				room.PlaySound(SoundID.Slugcat_Eat_Meat_B, base.mainBodyChunk);
				room.PlaySound(SoundID.Drop_Bug_Grab_Creature, base.mainBodyChunk, loop: false, 1f, 0.76f);
				for (int num = UnityEngine.Random.Range(8, 14); num >= 0; num--)
				{
					room.AddObject(new WaterDrip(Vector2.Lerp(base.grasps[graspIndex].grabbedChunk.pos, base.mainBodyChunk.pos, UnityEngine.Random.value) + base.grasps[graspIndex].grabbedChunk.rad * Custom.RNV() * UnityEngine.Random.value, Custom.RNV() * 6f * UnityEngine.Random.value + Custom.DirVec(base.grasps[graspIndex].grabbed.firstChunk.pos, (base.mainBodyChunk.pos + (base.graphicsModule as PlayerGraphics).head.pos) / 2f) * 7f * UnityEngine.Random.value + Custom.DegToVec(Mathf.Lerp(-90f, 90f, UnityEngine.Random.value)) * UnityEngine.Random.value * EffectiveRoomGravity * 7f, waterColor: false));
				}
			}
		}
		standing = false;
		Blink(5);
		if (eatMeat % 5 == 0)
		{
			Vector2 vector = Custom.RNV() * 3f;
			base.mainBodyChunk.pos += vector;
			base.mainBodyChunk.vel += vector;
		}
		Vector2 vector2 = base.grasps[graspIndex].grabbedChunk.pos * base.grasps[graspIndex].grabbedChunk.mass;
		float num2 = base.grasps[graspIndex].grabbedChunk.mass;
		for (int i = 0; i < base.grasps[graspIndex].grabbed.bodyChunkConnections.Length; i++)
		{
			if (base.grasps[graspIndex].grabbed.bodyChunkConnections[i].chunk1 == base.grasps[graspIndex].grabbedChunk)
			{
				vector2 += base.grasps[graspIndex].grabbed.bodyChunkConnections[i].chunk2.pos * base.grasps[graspIndex].grabbed.bodyChunkConnections[i].chunk2.mass;
				num2 += base.grasps[graspIndex].grabbed.bodyChunkConnections[i].chunk2.mass;
			}
			else if (base.grasps[graspIndex].grabbed.bodyChunkConnections[i].chunk2 == base.grasps[graspIndex].grabbedChunk)
			{
				vector2 += base.grasps[graspIndex].grabbed.bodyChunkConnections[i].chunk1.pos * base.grasps[graspIndex].grabbed.bodyChunkConnections[i].chunk1.mass;
				num2 += base.grasps[graspIndex].grabbed.bodyChunkConnections[i].chunk1.mass;
			}
		}
		vector2 /= num2;
		base.mainBodyChunk.vel += Custom.DirVec(base.mainBodyChunk.pos, vector2) * 0.5f;
		base.bodyChunks[1].vel -= Custom.DirVec(base.mainBodyChunk.pos, vector2) * 0.6f;
		if (base.graphicsModule == null || (base.grasps[graspIndex].grabbed as Creature).State.meatLeft <= 0 || FoodInStomach >= MaxFoodInStomach)
		{
			return;
		}
		if (!Custom.DistLess(base.grasps[graspIndex].grabbedChunk.pos, (base.graphicsModule as PlayerGraphics).head.pos, base.grasps[graspIndex].grabbedChunk.rad))
		{
			(base.graphicsModule as PlayerGraphics).head.vel += Custom.DirVec(base.grasps[graspIndex].grabbedChunk.pos, (base.graphicsModule as PlayerGraphics).head.pos) * (base.grasps[graspIndex].grabbedChunk.rad - Vector2.Distance(base.grasps[graspIndex].grabbedChunk.pos, (base.graphicsModule as PlayerGraphics).head.pos));
		}
		else if (eatMeat % 5 == 3)
		{
			(base.graphicsModule as PlayerGraphics).head.vel += Custom.RNV() * 4f;
		}
		if (eatMeat > 40 && eatMeat % 15 == 3)
		{
			base.mainBodyChunk.pos += Custom.DegToVec(Mathf.Lerp(-90f, 90f, UnityEngine.Random.value)) * 4f;
			base.grasps[graspIndex].grabbedChunk.vel += Custom.DirVec(vector2, base.mainBodyChunk.pos) * 0.9f / base.grasps[graspIndex].grabbedChunk.mass;
			for (int num3 = UnityEngine.Random.Range(0, 3); num3 >= 0; num3--)
			{
				room.AddObject(new WaterDrip(Vector2.Lerp(base.grasps[graspIndex].grabbedChunk.pos, base.mainBodyChunk.pos, UnityEngine.Random.value) + base.grasps[graspIndex].grabbedChunk.rad * Custom.RNV() * UnityEngine.Random.value, Custom.RNV() * 6f * UnityEngine.Random.value + Custom.DirVec(vector2, (base.mainBodyChunk.pos + (base.graphicsModule as PlayerGraphics).head.pos) / 2f) * 7f * UnityEngine.Random.value + Custom.DegToVec(Mathf.Lerp(-90f, 90f, UnityEngine.Random.value)) * UnityEngine.Random.value * EffectiveRoomGravity * 7f, waterColor: false));
			}
			if (SessionRecord != null)
			{
				SessionRecord.AddEat(base.grasps[graspIndex].grabbed);
			}
			(base.grasps[graspIndex].grabbed as Creature).State.meatLeft--;
			if (ModManager.MSC && (SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel || SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Gourmand) && !(base.grasps[graspIndex].grabbed is Centipede))
			{
				AddQuarterFood();
				AddQuarterFood();
			}
			else
			{
				AddFood(1);
			}
			room.PlaySound(SoundID.Slugcat_Eat_Meat_B, base.mainBodyChunk);
		}
		else if (eatMeat % 15 == 3)
		{
			room.PlaySound(SoundID.Slugcat_Eat_Meat_A, base.mainBodyChunk);
		}
	}

	public void MovementUpdate(bool eu)
	{
		DirectIntoHoles();
		if (rocketJumpFromBellySlide && animation != AnimationIndex.RocketJump)
		{
			rocketJumpFromBellySlide = false;
		}
		if (flipFromSlide && animation != AnimationIndex.Flip)
		{
			flipFromSlide = false;
		}
		if (whiplashJump && animation != AnimationIndex.BellySlide)
		{
			whiplashJump = false;
		}
		int num = input[0].x;
		if (jumpStun != 0)
		{
			num = jumpStun / Mathf.Abs(jumpStun);
		}
		lastFlipDirection = flipDirection;
		if (num != flipDirection && num != 0)
		{
			flipDirection = num;
		}
		int num2 = 0;
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				if (IsTileSolid(j, Custom.eightDirections[i].x, Custom.eightDirections[i].y) && IsTileSolid(j, Custom.eightDirections[i + 4].x, Custom.eightDirections[i + 4].y))
				{
					num2++;
				}
			}
		}
		bool flag = base.bodyChunks[1].onSlope == 0 && input[0].x == 0 && standing && base.stun < 1 && base.bodyChunks[1].ContactPoint.y == -1;
		if (feetStuckPos.HasValue && !flag)
		{
			feetStuckPos = null;
		}
		else if (!feetStuckPos.HasValue && flag)
		{
			feetStuckPos = new Vector2(base.bodyChunks[1].pos.x, room.MiddleOfTile(room.GetTilePosition(base.bodyChunks[1].pos)).y + -10f + base.bodyChunks[1].rad);
		}
		if (feetStuckPos.HasValue)
		{
			feetStuckPos = feetStuckPos.Value + new Vector2((base.bodyChunks[1].pos.x - feetStuckPos.Value.x) * (1f - surfaceFriction), 0f);
			base.bodyChunks[1].pos = feetStuckPos.Value;
			if (!IsTileSolid(1, 0, -1))
			{
				bool flag2 = IsTileSolid(1, 1, -1) && !IsTileSolid(1, 1, 0);
				bool flag3 = IsTileSolid(1, -1, -1) && !IsTileSolid(1, -1, 0);
				if (flag3 && !flag2)
				{
					feetStuckPos = feetStuckPos.Value + new Vector2(-1.6f * surfaceFriction, 0f);
				}
				else if (flag2 && !flag3)
				{
					feetStuckPos = feetStuckPos.Value + new Vector2(1.6f * surfaceFriction, 0f);
				}
				else
				{
					feetStuckPos = null;
				}
			}
		}
		if ((num2 > 1 && base.bodyChunks[0].onSlope == 0 && base.bodyChunks[1].onSlope == 0 && (!IsTileSolid(0, 0, 0) || !IsTileSolid(1, 0, 0))) || (IsTileSolid(0, -1, 0) && IsTileSolid(0, 1, 0)) || (IsTileSolid(1, -1, 0) && IsTileSolid(1, 1, 0)))
		{
			goIntoCorridorClimb++;
		}
		else
		{
			goIntoCorridorClimb = 0;
			bool num3 = base.bodyChunks[0].ContactPoint.y == -1 || base.bodyChunks[1].ContactPoint.y == -1;
			bodyMode = BodyModeIndex.Default;
			if (num3)
			{
				canJump = 5;
				if (base.bodyChunks[0].pos.y > base.bodyChunks[1].pos.y + 3f && !IsTileSolid(1, 0, 1) && animation != AnimationIndex.CrawlTurn && base.bodyChunks[0].ContactPoint.y > -1)
				{
					bodyMode = BodyModeIndex.Stand;
				}
				else
				{
					bodyMode = BodyModeIndex.Crawl;
				}
			}
			else if (jumpBoost > 0f && (input[0].jmp || simulateHoldJumpButton > 0))
			{
				jumpBoost -= 1.5f;
				base.bodyChunks[0].vel.y += (jumpBoost + 1f) * 0.3f;
				base.bodyChunks[1].vel.y += (jumpBoost + 1f) * 0.3f;
			}
			else
			{
				jumpBoost = 0f;
			}
			if (base.bodyChunks[0].ContactPoint.x != 0 && base.bodyChunks[0].ContactPoint.x == input[0].x)
			{
				if (base.bodyChunks[0].lastContactPoint.x != input[0].x)
				{
					room.PlaySound(SoundID.Slugcat_Enter_Wall_Slide, base.mainBodyChunk, loop: false, 1f, 1f);
				}
				bodyMode = BodyModeIndex.WallClimb;
			}
			if (input[0].x != 0 && base.bodyChunks[0].pos.y > base.bodyChunks[1].pos.y && animation != AnimationIndex.CrawlTurn && !IsTileSolid(0, input[0].x, 0) && IsTileSolid(1, input[0].x, 0) && base.bodyChunks[1].ContactPoint.x == input[0].x)
			{
				bodyMode = BodyModeIndex.Crawl;
				animation = AnimationIndex.LedgeCrawl;
			}
			if (input[0].y == 1 && IsTileSolid(0, 0, 1) && !IsTileSolid(1, 0, 1) && (IsTileSolid(1, -1, 1) || IsTileSolid(1, 1, 1)))
			{
				animation = AnimationIndex.None;
				base.bodyChunks[1].vel.y += 2f * EffectiveRoomGravity;
				base.bodyChunks[0].vel.x -= (base.bodyChunks[0].pos.x - base.bodyChunks[1].pos.x) * 0.25f * EffectiveRoomGravity;
				base.bodyChunks[0].vel.y -= EffectiveRoomGravity;
			}
		}
		if (input[0].y > 0 && input[0].x == 0 && bodyMode == BodyModeIndex.Default && base.firstChunk.pos.y - base.firstChunk.lastPos.y < 2f && base.bodyChunks[1].ContactPoint.y == 0 && !IsTileSolid(0, 0, 1) && IsTileSolid(0, -1, 1) && IsTileSolid(0, 1, 1) && !IsTileSolid(1, -1, 0) && !IsTileSolid(1, 1, 0) && room.GetTilePosition(base.firstChunk.pos) == room.GetTilePosition(base.bodyChunks[1].pos) + new IntVector2(0, 1) && Mathf.Abs(base.firstChunk.pos.x - room.MiddleOfTile(base.firstChunk.pos).x) < 5f && EffectiveRoomGravity > 0f)
		{
			base.firstChunk.pos.x = room.MiddleOfTile(base.firstChunk.pos).x;
			base.firstChunk.pos.y += 1f;
			base.firstChunk.vel.y += 1f;
			base.bodyChunks[1].vel.y += 1f;
			base.bodyChunks[1].pos.y += 1f;
		}
		if (input[0].y == 1 && input[1].y != 1)
		{
			if (base.bodyChunks[1].onSlope == 0 || !IsTileSolid(0, 0, 1))
			{
				standing = true;
			}
		}
		else if (input[0].y == -1 && input[1].y != -1)
		{
			if (standing && bodyMode == BodyModeIndex.Stand)
			{
				room.PlaySound(SoundID.Slugcat_Down_On_Fours, base.mainBodyChunk);
			}
			standing = false;
		}
		if (EffectiveRoomGravity > 0f && animation == AnimationIndex.ZeroGPoleGrab)
		{
			bodyMode = BodyModeIndex.ClimbingOnBeam;
			if (room.GetTile(base.mainBodyChunk.pos).horizontalBeam)
			{
				animation = AnimationIndex.HangFromBeam;
			}
			else
			{
				animation = AnimationIndex.ClimbOnBeam;
			}
		}
		if (goIntoCorridorClimb > 2 && !corridorDrop)
		{
			bodyMode = BodyModeIndex.CorridorClimb;
			animation = (corridorTurnDir.HasValue ? AnimationIndex.CorridorTurn : AnimationIndex.None);
		}
		if (corridorDrop)
		{
			bodyMode = BodyModeIndex.Default;
			animation = AnimationIndex.None;
			if (input[0].y >= 0 || goIntoCorridorClimb < 2)
			{
				corridorDrop = false;
			}
			if (base.bodyChunks[0].pos.y < base.bodyChunks[1].pos.y)
			{
				for (int k = 0; k < Custom.IntClamp((int)(base.bodyChunks[0].vel.y * -0.3f), 1, 10); k++)
				{
					if (IsTileSolid(0, 0, -k))
					{
						corridorDrop = false;
						break;
					}
				}
			}
		}
		if (bodyMode != BodyModeIndex.WallClimb || base.bodyChunks[0].submersion == 1f)
		{
			bool flag4 = input[0].y < 0 || input[0].downDiagonal != 0;
			if (ModManager.MSC && room.waterInverted)
			{
				flag4 = input[0].y > 0;
			}
			if ((base.bodyChunks[0].submersion > 0.2f || base.bodyChunks[1].submersion > 0.2f) && bodyMode != BodyModeIndex.CorridorClimb)
			{
				bool flag5;
				bool flag6;
				if (ModManager.MSC)
				{
					flag5 = room.PointSubmerged(base.bodyChunks[0].pos, 80f);
					flag6 = room.PointSubmerged(base.bodyChunks[0].pos, (!flag4) ? 30f : 10f);
				}
				else
				{
					flag5 = base.bodyChunks[0].pos.y < room.FloatWaterLevel(base.bodyChunks[0].pos.x) - 80f;
					flag6 = base.bodyChunks[0].pos.y < room.FloatWaterLevel(base.bodyChunks[0].pos.x) - (flag4 ? 10f : 30f);
				}
				if ((animation != AnimationIndex.SurfaceSwim || flag4 || flag5) && flag6 && base.bodyChunks[1].submersion > (flag4 ? (-1f) : 0.6f))
				{
					bodyMode = BodyModeIndex.Swimming;
					animation = AnimationIndex.DeepSwim;
				}
				else if ((!IsTileSolid(1, 0, -1) || base.bodyChunks[1].submersion == 1f) && animation != AnimationIndex.BeamTip && animation != AnimationIndex.ClimbOnBeam && animation != AnimationIndex.GetUpOnBeam && animation != AnimationIndex.GetUpToBeamTip && animation != AnimationIndex.HangFromBeam && animation != AnimationIndex.StandOnBeam && animation != AnimationIndex.LedgeGrab && animation != AnimationIndex.HangUnderVerticalBeam)
				{
					bodyMode = BodyModeIndex.Swimming;
					animation = AnimationIndex.SurfaceSwim;
				}
			}
		}
		if (EffectiveRoomGravity == 0f && (!ModManager.MMF || !submerged) && bodyMode != BodyModeIndex.CorridorClimb && animation != AnimationIndex.VineGrab)
		{
			bodyMode = BodyModeIndex.ZeroG;
			if (animation != AnimationIndex.ZeroGSwim && animation != AnimationIndex.ZeroGPoleGrab)
			{
				animation = ((room.GetTile(base.mainBodyChunk.pos).horizontalBeam || room.GetTile(base.mainBodyChunk.pos).verticalBeam) ? AnimationIndex.ZeroGPoleGrab : AnimationIndex.ZeroGSwim);
			}
		}
		if (playerInAntlers != null)
		{
			animation = AnimationIndex.AntlerClimb;
		}
		if (tubeWorm != null)
		{
			bool flag7 = true;
			for (int l = 0; l < base.grasps.Length && flag7; l++)
			{
				if (base.grasps[l] != null && base.grasps[l].grabbed as TubeWorm == tubeWorm)
				{
					flag7 = false;
				}
			}
			if (flag7)
			{
				tubeWorm = null;
			}
		}
		if (tubeWorm != null && tubeWorm.tongues[0].Attached && bodyMode == BodyModeIndex.Default && base.bodyChunks[1].ContactPoint.y >= 0 && (animation == AnimationIndex.GrapplingSwing || animation == AnimationIndex.None))
		{
			animation = AnimationIndex.GrapplingSwing;
		}
		else if (animation == AnimationIndex.GrapplingSwing)
		{
			animation = AnimationIndex.None;
		}
		if (vineGrabDelay > 0)
		{
			vineGrabDelay--;
		}
		if (animation != AnimationIndex.VineGrab && vineGrabDelay == 0 && room.climbableVines != null && (!ModManager.MMF || animation != AnimationIndex.ClimbOnBeam))
		{
			if (EffectiveRoomGravity > 0f && (wantToGrab > 0 || input[0].y > 0))
			{
				int num4 = Custom.IntClamp((int)(Vector2.Distance(base.mainBodyChunk.lastPos, base.mainBodyChunk.pos) / 5f), 1, 10);
				for (int m = 0; m < num4; m++)
				{
					Vector2 pos = Vector2.Lerp(base.mainBodyChunk.lastPos, base.mainBodyChunk.pos, (num4 > 1) ? ((float)m / (float)(num4 - 1)) : 0f);
					ClimbableVinesSystem.VinePosition vinePosition = room.climbableVines.VineOverlap(pos, base.mainBodyChunk.rad);
					if (vinePosition != null)
					{
						if (room.climbableVines.GetVineObject(vinePosition) is CoralNeuron)
						{
							room.PlaySound(SoundID.Grab_Neuron, base.mainBodyChunk);
						}
						else if (room.climbableVines.GetVineObject(vinePosition) is CoralStem)
						{
							room.PlaySound(SoundID.Grab_Coral_Stem, base.mainBodyChunk);
						}
						else if (room.climbableVines.GetVineObject(vinePosition) is DaddyCorruption.ClimbableCorruptionTube)
						{
							room.PlaySound(SoundID.Grab_Corruption_Tube, base.mainBodyChunk);
						}
						else if (room.climbableVines.GetVineObject(vinePosition) is ClimbableVine)
						{
							room.PlaySound(SoundID.Leaves, base.mainBodyChunk.pos, 1f, 0.75f + UnityEngine.Random.value * 0.5f);
						}
						animation = AnimationIndex.VineGrab;
						vinePos = vinePosition;
						wantToGrab = 0;
						break;
					}
				}
			}
			else if (animation != AnimationIndex.VineGrab && (input[0].x != 0 || input[0].y != 0) && EffectiveRoomGravity == 0f)
			{
				ClimbableVinesSystem.VinePosition vinePosition2 = room.climbableVines.VineOverlap(base.mainBodyChunk.pos, base.mainBodyChunk.rad);
				if (vinePosition2 != null)
				{
					if (room.climbableVines.GetVineObject(vinePosition2) is CoralNeuron)
					{
						room.PlaySound(SoundID.Grab_Neuron, base.mainBodyChunk);
					}
					else if (room.climbableVines.GetVineObject(vinePosition2) is CoralStem)
					{
						room.PlaySound(SoundID.Grab_Coral_Stem, base.mainBodyChunk);
					}
					else if (room.climbableVines.GetVineObject(vinePosition2) is DaddyCorruption.ClimbableCorruptionTube)
					{
						room.PlaySound(SoundID.Grab_Corruption_Tube, base.mainBodyChunk);
					}
					else if (room.climbableVines.GetVineObject(vinePosition2) is ClimbableVine)
					{
						room.PlaySound(SoundID.Leaves, base.mainBodyChunk.pos, 1f, 0.75f + UnityEngine.Random.value * 0.5f);
					}
					animation = AnimationIndex.VineGrab;
					vinePos = vinePosition2;
					wantToGrab = 0;
				}
			}
		}
		dynamicRunSpeed[0] = 3.6f;
		dynamicRunSpeed[1] = 3.6f;
		float num5 = 2.4f;
		UpdateAnimation();
		if (base.bodyChunks[0].ContactPoint.x == input[0].x && input[0].x != 0 && base.bodyChunks[0].pos.y > room.MiddleOfTile(room.GetTilePosition(base.bodyChunks[0].pos)).y && (bodyMode == BodyModeIndex.Default || bodyMode == BodyModeIndex.WallClimb) && !IsTileSolid(0, -input[0].x, 0) && !IsTileSolid(0, 0, -2) && !IsTileSolid(0, input[0].x, 1))
		{
			animation = AnimationIndex.LedgeGrab;
			bodyMode = BodyModeIndex.Default;
		}
		if (bodyMode == BodyModeIndex.Crawl)
		{
			crawlTurnDelay++;
		}
		else
		{
			crawlTurnDelay = 0;
		}
		if (standing && IsTileSolid(1, 0, 1))
		{
			standing = false;
		}
		if (input[0].y > 0 && input[1].y == 0 && !room.GetTile(base.bodyChunks[1].pos).verticalBeam && room.GetTile(base.bodyChunks[1].pos + new Vector2(0f, -20f)).verticalBeam)
		{
			animation = AnimationIndex.BeamTip;
			base.bodyChunks[1].vel.x = 0f;
			base.bodyChunks[1].vel.y = 0f;
			wantToGrab = -1;
		}
		UpdateBodyMode();
		int num6 = ((isSlugpup && playerState.isPup) ? 12 : 17);
		if (rollDirection != 0)
		{
			rollCounter++;
			num = rollDirection;
			bodyChunkConnections[0].distance = 10f;
			if (bodyMode != BodyModeIndex.Default || rollCounter > 200)
			{
				rollCounter = 0;
				rollDirection = 0;
			}
		}
		else
		{
			bodyChunkConnections[0].distance = num6;
		}
		bodyChunkConnections[0].type = (corridorTurnDir.HasValue ? BodyChunkConnection.Type.Pull : BodyChunkConnection.Type.Normal);
		wantToGrab = ((input[0].y > 0 && (!ModManager.MSC || !monkAscension) && !(base.Submersion > 0.9f)) ? 1 : 0);
		if (wantToGrab > 0 && noGrabCounter == 0 && (bodyMode == BodyModeIndex.Default || bodyMode == BodyModeIndex.WallClimb || bodyMode == BodyModeIndex.Stand || bodyMode == BodyModeIndex.ClimbingOnBeam || bodyMode == BodyModeIndex.Swimming) && (timeSinceInCorridorMode >= 20 || !(base.bodyChunks[1].pos.y > base.firstChunk.pos.y) || room.GetTilePosition(base.bodyChunks[0].pos).x != room.GetTilePosition(base.bodyChunks[1].pos).x) && animation != AnimationIndex.ClimbOnBeam && animation != AnimationIndex.HangFromBeam && animation != AnimationIndex.GetUpOnBeam && animation != AnimationIndex.DeepSwim && animation != AnimationIndex.HangUnderVerticalBeam && animation != AnimationIndex.GetUpToBeamTip && animation != AnimationIndex.VineGrab)
		{
			int x = room.GetTilePosition(base.bodyChunks[0].pos).x;
			int num7 = room.GetTilePosition(base.bodyChunks[0].lastPos).y;
			int num8 = room.GetTilePosition(base.bodyChunks[0].pos).y;
			if (num8 > num7)
			{
				int num9 = num7;
				num7 = num8;
				num8 = num9;
			}
			for (int num10 = num7; num10 >= num8; num10--)
			{
				if (room.GetTile(x, num10).horizontalBeam)
				{
					animation = AnimationIndex.HangFromBeam;
					room.PlaySound(SoundID.Slugcat_Grab_Beam, base.mainBodyChunk, loop: false, 1f, 1f);
					base.bodyChunks[0].vel.y = 0f;
					base.bodyChunks[1].vel.y *= 0.25f;
					base.bodyChunks[0].pos.y = room.MiddleOfTile(new IntVector2(x, num10)).y;
					break;
				}
			}
			GrabVerticalPole();
			if (animation != AnimationIndex.HangFromBeam && animation != AnimationIndex.ClimbOnBeam && room.GetTile(base.bodyChunks[0].pos + new Vector2(0f, 20f)).verticalBeam && !room.GetTile(base.bodyChunks[0].pos).verticalBeam)
			{
				base.bodyChunks[0].pos = room.MiddleOfTile(base.bodyChunks[0].pos) + new Vector2(0f, 5f);
				base.bodyChunks[0].vel *= 0f;
				base.bodyChunks[1].vel = Vector2.ClampMagnitude(base.bodyChunks[1].vel, 9f);
				animation = AnimationIndex.HangUnderVerticalBeam;
			}
		}
		bool flag8 = false;
		if (bodyMode != BodyModeIndex.CorridorClimb)
		{
			flag8 = true;
		}
		if (animation == AnimationIndex.ClimbOnBeam || animation == AnimationIndex.HangFromBeam || animation == AnimationIndex.GetUpOnBeam || animation == AnimationIndex.LedgeGrab || animation == AnimationIndex.GrapplingSwing || animation == AnimationIndex.AntlerClimb)
		{
			flag8 = false;
		}
		if (base.grasps[0] != null && HeavyCarry(base.grasps[0].grabbed))
		{
			float num11 = 1f + Mathf.Max(0f, base.grasps[0].grabbed.TotalMass - 0.2f);
			if (base.grasps[0].grabbed is Cicada)
			{
				if (bodyMode == BodyModeIndex.Default && animation == AnimationIndex.None)
				{
					base.mainBodyChunk.vel.y += (base.grasps[0].grabbed as Cicada).LiftPlayerPower * 1.2f;
					base.bodyChunks[1].vel.y += (base.grasps[0].grabbed as Cicada).LiftPlayerPower * 0.25f;
					(base.grasps[0].grabbed as Cicada).currentlyLiftingPlayer = true;
					if ((base.grasps[0].grabbed as Cicada).LiftPlayerPower > 2f / 3f)
					{
						standing = false;
					}
				}
				else
				{
					base.mainBodyChunk.vel.y += (base.grasps[0].grabbed as Cicada).LiftPlayerPower * 0.5f;
					(base.grasps[0].grabbed as Cicada).currentlyLiftingPlayer = false;
				}
				if (base.bodyChunks[1].ContactPoint.y < 0 && base.bodyChunks[1].lastContactPoint.y == 0 && (base.grasps[0].grabbed as Cicada).LiftPlayerPower > 1f / 3f)
				{
					standing = true;
				}
				num11 = 1f + Mathf.Max(0f, base.grasps[0].grabbed.TotalMass - 0.2f) * 1.5f;
				num11 = Mathf.Lerp(num11, 1f, Mathf.Pow(Mathf.InverseLerp(0.1f, 0.5f, (base.grasps[0].grabbed as Cicada).LiftPlayerPower), 0.2f));
			}
			else if (Grabability(base.grasps[0].grabbed) == ObjectGrabability.Drag)
			{
				if (bodyMode == BodyModeIndex.Default || bodyMode == BodyModeIndex.CorridorClimb || bodyMode == BodyModeIndex.Stand || bodyMode == BodyModeIndex.Crawl)
				{
					num11 = 1f;
				}
				if (room.aimap != null)
				{
					if (room.aimap.getAItile(base.mainBodyChunk.pos).narrowSpace)
					{
						base.grasps[0].grabbedChunk.vel += input[0].IntVec.ToVector2().normalized * slugcatStats.corridorClimbSpeedFac * 4f / Mathf.Max(0.75f, base.grasps[0].grabbed.TotalMass);
					}
					for (int n = 0; n < base.grasps[0].grabbed.bodyChunks.Length; n++)
					{
						if (room.aimap.getAItile(base.grasps[0].grabbed.bodyChunks[n].pos).narrowSpace)
						{
							base.grasps[0].grabbed.bodyChunks[n].vel *= 0.8f;
							base.grasps[0].grabbed.bodyChunks[n].vel.y += EffectiveRoomGravity * base.grasps[0].grabbed.gravity * 0.85f;
							base.grasps[0].grabbed.bodyChunks[n].vel += input[0].IntVec.ToVector2().normalized * slugcatStats.corridorClimbSpeedFac * 1.5f / ((float)base.grasps[0].grabbed.bodyChunks.Length * Mathf.Max(1f, (base.grasps[0].grabbed.TotalMass + 1f) / 2f));
							base.grasps[0].grabbed.bodyChunks[n].pos += input[0].IntVec.ToVector2().normalized * slugcatStats.corridorClimbSpeedFac * 1.1f / ((float)base.grasps[0].grabbed.bodyChunks.Length * Mathf.Max(1f, (base.grasps[0].grabbed.TotalMass + 2f) / 3f));
						}
					}
				}
			}
			if (shortcutDelay < 1 && !enteringShortCut.HasValue && (input[0].x == 0 || input[0].y == 0) && (input[0].x != 0 || input[0].y != 0))
			{
				for (int num12 = 0; num12 < base.grasps[0].grabbed.bodyChunks.Length; num12++)
				{
					if (room.GetTile(room.GetTilePosition(base.grasps[0].grabbed.bodyChunks[num12].pos) + input[0].IntVec).Terrain != Room.Tile.TerrainType.ShortcutEntrance || !(room.ShorcutEntranceHoleDirection(room.GetTilePosition(base.grasps[0].grabbed.bodyChunks[num12].pos) + input[0].IntVec) == new IntVector2(-input[0].x, -input[0].y)))
					{
						continue;
					}
					ShortcutData.Type shortCutType = room.shortcutData(room.GetTilePosition(base.grasps[0].grabbed.bodyChunks[num12].pos) + input[0].IntVec).shortCutType;
					if (shortCutType == ShortcutData.Type.RoomExit || shortCutType == ShortcutData.Type.Normal)
					{
						enteringShortCut = room.GetTilePosition(base.grasps[0].grabbed.bodyChunks[num12].pos) + input[0].IntVec;
						Custom.Log("player pulled into shortcut by carried object");
						if (ModManager.MSC && tongue != null && tongue.Attached)
						{
							tongue.Release();
						}
						break;
					}
				}
			}
			dynamicRunSpeed[0] /= num11;
			dynamicRunSpeed[1] /= num11;
		}
		dynamicRunSpeed[0] *= Mathf.Lerp(1f, 1.5f, Adrenaline);
		dynamicRunSpeed[1] *= Mathf.Lerp(1f, 1.5f, Adrenaline);
		num5 *= Mathf.Lerp(1f, 1.2f, Adrenaline);
		if (flag8 && (dynamicRunSpeed[0] > 0f || dynamicRunSpeed[1] > 0f))
		{
			if (slowMovementStun > 0)
			{
				dynamicRunSpeed[0] *= 0.5f + 0.5f * Mathf.InverseLerp(10f, 0f, slowMovementStun);
				dynamicRunSpeed[1] *= 0.5f + 0.5f * Mathf.InverseLerp(10f, 0f, slowMovementStun);
				num5 *= 0.4f + 0.6f * Mathf.InverseLerp(10f, 0f, slowMovementStun);
			}
			if (bodyMode == BodyModeIndex.Default && base.bodyChunks[0].ContactPoint.x == 0 && base.bodyChunks[0].ContactPoint.y == 0 && base.bodyChunks[1].ContactPoint.x == 0 && base.bodyChunks[1].ContactPoint.y == 0)
			{
				num5 *= EffectiveRoomGravity;
			}
			for (int num13 = 0; num13 < 2; num13++)
			{
				if (num < 0)
				{
					float num14 = num5 * surfaceFriction;
					if (base.bodyChunks[num13].vel.x - num14 < 0f - dynamicRunSpeed[num13])
					{
						num14 = dynamicRunSpeed[num13] + base.bodyChunks[num13].vel.x;
					}
					if (num14 > 0f)
					{
						base.bodyChunks[num13].vel.x -= num14;
					}
				}
				else if (num > 0)
				{
					float num15 = num5 * surfaceFriction;
					if (base.bodyChunks[num13].vel.x + num15 > dynamicRunSpeed[num13])
					{
						num15 = dynamicRunSpeed[num13] - base.bodyChunks[num13].vel.x;
					}
					if (num15 > 0f)
					{
						base.bodyChunks[num13].vel.x += num15;
					}
				}
				if (base.bodyChunks[0].ContactPoint.y != 0 || base.bodyChunks[1].ContactPoint.y != 0)
				{
					float num16 = 0f;
					if (input[0].x != 0)
					{
						num16 = Mathf.Clamp(base.bodyChunks[num13].vel.x, 0f - dynamicRunSpeed[num13], dynamicRunSpeed[num13]);
					}
					base.bodyChunks[num13].vel.x += (num16 - base.bodyChunks[num13].vel.x) * Mathf.Pow(surfaceFriction, 1.5f);
				}
			}
		}
		int num17 = 0;
		if (superLaunchJump > 0 && killSuperLaunchJumpCounter < 1)
		{
			num17 = 1;
		}
		if (bodyMode == BodyModeIndex.Crawl && base.bodyChunks[0].ContactPoint.y < 0 && base.bodyChunks[1].ContactPoint.y < 0)
		{
			if (input[0].x == 0 && input[0].y == 0)
			{
				num17 = 0;
				wantToJump = 0;
				if (input[0].jmp)
				{
					if (superLaunchJump < 20)
					{
						superLaunchJump++;
						if (Adrenaline == 1f && superLaunchJump < 6)
						{
							superLaunchJump = 6;
						}
					}
					else
					{
						killSuperLaunchJumpCounter = 15;
					}
				}
			}
			if (!input[0].jmp && input[1].jmp)
			{
				wantToJump = 1;
			}
		}
		if (killSuperLaunchJumpCounter > 0)
		{
			killSuperLaunchJumpCounter--;
		}
		if (simulateHoldJumpButton > 0)
		{
			simulateHoldJumpButton--;
		}
		if (canJump > 0 && wantToJump > 0)
		{
			canJump = 0;
			wantToJump = 0;
			Jump();
		}
		else if (canWallJump != 0 && wantToJump > 0 && input[0].x != -Math.Sign(canWallJump))
		{
			WallJump(Math.Sign(canWallJump));
			wantToJump = 0;
		}
		else if (jumpChunkCounter > 0 && wantToJump > 0)
		{
			jumpChunkCounter = -5;
			wantToJump = 0;
			JumpOnChunk();
		}
		if (Adrenaline > 0f)
		{
			float num18 = (isRivulet ? 16f : 8f) * Adrenaline;
			if (input[0].x < 0)
			{
				if (!IsTileSolid(0, -1, 0) && directionBoosts[0] == 1f)
				{
					directionBoosts[0] = 0f;
					base.mainBodyChunk.vel.x -= num18;
					base.bodyChunks[1].vel.x += num18 / 3f;
				}
			}
			else if (directionBoosts[0] == 0f)
			{
				directionBoosts[0] = 0.01f;
			}
			if (input[0].x > 0)
			{
				if (!IsTileSolid(0, 1, 0) && directionBoosts[1] == 1f)
				{
					directionBoosts[1] = 0f;
					base.mainBodyChunk.vel.x += num18;
					base.bodyChunks[1].vel.x -= num18 / 3f;
				}
			}
			else if (directionBoosts[1] == 0f)
			{
				directionBoosts[1] = 0.01f;
			}
			if (input[0].y < 0)
			{
				if (!IsTileSolid(0, 0, -1) && directionBoosts[2] == 1f)
				{
					directionBoosts[2] = 0f;
					base.mainBodyChunk.vel.y -= num18;
					base.bodyChunks[1].vel.y += num18 / 3f;
				}
			}
			else if (directionBoosts[2] == 0f)
			{
				directionBoosts[2] = 0.01f;
			}
			if (input[0].y > 0)
			{
				if (!IsTileSolid(0, 0, 1) && directionBoosts[3] == 1f)
				{
					directionBoosts[3] = 0f;
					base.mainBodyChunk.vel.y += num18;
					base.bodyChunks[1].vel.y -= num18;
				}
			}
			else if (directionBoosts[3] == 0f)
			{
				directionBoosts[3] = 0.01f;
			}
		}
		superLaunchJump -= num17;
		if (shortcutDelay < 1 && (!ModManager.MSC || (onBack == null && (grabbedBy.Count == 0 || !(grabbedBy[0].grabber is Player)))))
		{
			for (int num19 = 0; num19 < 2; num19++)
			{
				if (enteringShortCut.HasValue || room.GetTile(base.bodyChunks[num19].pos).Terrain != Room.Tile.TerrainType.ShortcutEntrance || !(room.shortcutData(room.GetTilePosition(base.bodyChunks[num19].pos)).shortCutType != ShortcutData.Type.DeadEnd) || !(room.shortcutData(room.GetTilePosition(base.bodyChunks[num19].pos)).shortCutType != ShortcutData.Type.CreatureHole) || !(room.shortcutData(room.GetTilePosition(base.bodyChunks[num19].pos)).shortCutType != ShortcutData.Type.NPCTransportation))
				{
					continue;
				}
				IntVector2 intVector = room.ShorcutEntranceHoleDirection(room.GetTilePosition(base.bodyChunks[num19].pos));
				if (input[0].x == -intVector.x && input[0].y == -intVector.y)
				{
					enteringShortCut = room.GetTilePosition(base.bodyChunks[num19].pos);
					if (ModManager.MSC && tongue != null && tongue.Attached)
					{
						tongue.Release();
					}
				}
			}
		}
		GrabUpdate(eu);
	}

	public void WallJump(int direction)
	{
		float num = Mathf.Lerp(1f, 1.15f, Adrenaline);
		if (PainJumps && (base.grasps[0] == null || !(base.grasps[0].grabbed is Yeek)))
		{
			gourmandExhausted = true;
			aerobicLevel = 1f;
		}
		if (exhausted)
		{
			num *= 1f - 0.5f * aerobicLevel;
		}
		bool flag = input[0].x != 0 && base.bodyChunks[0].ContactPoint.x == input[0].x && IsTileSolid(0, input[0].x, 0) && !IsTileSolid(0, input[0].x, 1);
		if (IsTileSolid(1, 0, -1) || IsTileSolid(0, 0, -1) || base.bodyChunks[1].submersion > 0.1f || flag)
		{
			if (base.bodyChunks[1].ContactPoint.y > -1 && base.bodyChunks[0].ContactPoint.y > -1 && base.Submersion == 0f)
			{
				num *= 0.7f;
			}
			base.bodyChunks[0].vel.y = (isRivulet ? 9f : 8f) * num;
			base.bodyChunks[1].vel.y = (isRivulet ? 8f : 7f) * num;
			base.bodyChunks[0].pos.y += 10f * Mathf.Min(1f, num);
			base.bodyChunks[1].pos.y += 10f * Mathf.Min(1f, num);
			room.PlaySound(SoundID.Slugcat_Normal_Jump, base.mainBodyChunk, loop: false, 1f, 1f);
			jumpBoost = 0f;
		}
		else
		{
			if (PainJumps)
			{
				base.bodyChunks[0].vel.y = 3f * num;
				base.bodyChunks[1].vel.y = 2f * num;
				base.bodyChunks[0].vel.x = 5f * num * (float)direction;
				base.bodyChunks[1].vel.x = 4f * num * (float)direction;
			}
			else if (isRivulet)
			{
				base.bodyChunks[0].vel.y = 10f * num;
				base.bodyChunks[1].vel.y = 9f * num;
				base.bodyChunks[0].vel.x = 9f * num * (float)direction;
				base.bodyChunks[1].vel.x = 7f * num * (float)direction;
			}
			else if (isSlugpup)
			{
				base.bodyChunks[0].vel.y = 6f * num;
				base.bodyChunks[1].vel.y = 5f * num;
				base.bodyChunks[0].vel.x = 5f * num * (float)direction;
				base.bodyChunks[1].vel.x = 4f * num * (float)direction;
			}
			else
			{
				base.bodyChunks[0].vel.y = 8f * num;
				base.bodyChunks[1].vel.y = 7f * num;
				base.bodyChunks[0].vel.x = 6f * num * (float)direction;
				base.bodyChunks[1].vel.x = 5f * num * (float)direction;
			}
			room.PlaySound(SoundID.Slugcat_Wall_Jump, base.mainBodyChunk, loop: false, 1f, 1f);
			standing = true;
			jumpBoost = (isRivulet ? 4 : 0);
			jumpStun = 8 * direction;
		}
		canWallJump = 0;
	}

	public void Jump()
	{
		feetStuckPos = null;
		pyroJumpDropLock = 40;
		forceSleepCounter = 0;
		if (PainJumps && (base.grasps[0] == null || !(base.grasps[0].grabbed is Yeek)))
		{
			gourmandExhausted = true;
			aerobicLevel = 1f;
		}
		float num = Mathf.Lerp(1f, 1.15f, Adrenaline);
		if (base.grasps[0] != null && HeavyCarry(base.grasps[0].grabbed) && !(base.grasps[0].grabbed is Cicada))
		{
			num += Mathf.Min(Mathf.Max(0f, base.grasps[0].grabbed.TotalMass - 0.2f) * 1.5f, 1.3f);
		}
		AerobicIncrease(isGourmand ? 0.75f : 1f);
		if (bodyMode == BodyModeIndex.WallClimb)
		{
			int direction = ((canWallJump != 0) ? Math.Sign(canWallJump) : ((base.bodyChunks[0].ContactPoint.x == 0) ? (-flipDirection) : (-base.bodyChunks[0].ContactPoint.x)));
			WallJump(direction);
		}
		else if (bodyMode == BodyModeIndex.CorridorClimb)
		{
			base.bodyChunks[0].vel.y = 6f * num;
			base.bodyChunks[1].vel.y = 5f * num;
			standing = true;
			if (isRivulet)
			{
				jumpBoost = 14f;
			}
			else if (isSlugpup)
			{
				jumpBoost = 4f;
			}
			else
			{
				jumpBoost = 8f;
			}
		}
		else if (animation == AnimationIndex.LedgeGrab)
		{
			if (input[0].x != 0)
			{
				WallJump(-input[0].x);
			}
		}
		else if (animation == AnimationIndex.ClimbOnBeam)
		{
			jumpBoost = 0f;
			if (input[0].x == 0)
			{
				if (input[0].y > 0)
				{
					if (slowMovementStun < 1 && slideUpPole < 1)
					{
						Blink(7);
						for (int i = 0; i < 2; i++)
						{
							base.bodyChunks[i].pos.y += (isSlugpup ? 2.25f : 4.5f);
							base.bodyChunks[i].vel.y += (isSlugpup ? 1f : 2f);
						}
						slideUpPole = 17;
						room.PlaySound(SoundID.Slugcat_From_Vertical_Pole_Jump, base.mainBodyChunk, loop: false, 0.8f, 1f);
					}
				}
				else
				{
					animation = AnimationIndex.None;
					base.bodyChunks[0].vel.y = 2f * num;
					if (input[0].y > -1)
					{
						base.bodyChunks[0].vel.x = 2f * (float)flipDirection * num;
					}
					room.PlaySound(SoundID.Slugcat_From_Vertical_Pole_Jump, base.mainBodyChunk, loop: false, 0.3f, 1f);
				}
				return;
			}
			animation = AnimationIndex.None;
			if (PainJumps)
			{
				base.bodyChunks[0].vel.y = 3f * num;
				base.bodyChunks[1].vel.y = 2f * num;
				base.bodyChunks[0].vel.x = 3f * (float)flipDirection * num;
				base.bodyChunks[1].vel.x = 2f * (float)flipDirection * num;
			}
			else if (isRivulet)
			{
				base.bodyChunks[0].vel.y = 9f * num;
				base.bodyChunks[1].vel.y = 8f * num;
				base.bodyChunks[0].vel.x = 9f * (float)flipDirection * num;
				base.bodyChunks[1].vel.x = 7f * (float)flipDirection * num;
			}
			else if (isSlugpup)
			{
				base.bodyChunks[0].vel.y = 7f * num;
				base.bodyChunks[1].vel.y = 6f * num;
				base.bodyChunks[0].vel.x = 5f * (float)flipDirection * num;
				base.bodyChunks[1].vel.x = 4.5f * (float)flipDirection * num;
			}
			else
			{
				base.bodyChunks[0].vel.y = 8f * num;
				base.bodyChunks[1].vel.y = 7f * num;
				base.bodyChunks[0].vel.x = 6f * (float)flipDirection * num;
				base.bodyChunks[1].vel.x = 5f * (float)flipDirection * num;
			}
			room.PlaySound(SoundID.Slugcat_From_Vertical_Pole_Jump, base.mainBodyChunk, loop: false, 1f, 1f);
		}
		else if (animation == AnimationIndex.Roll)
		{
			base.bodyChunks[1].vel *= 0f;
			base.bodyChunks[1].pos += new Vector2(5f * (float)rollDirection, 5f);
			base.bodyChunks[0].pos = base.bodyChunks[1].pos + new Vector2(5f * (float)rollDirection, 5f);
			float t = Mathf.InverseLerp(0f, 25f, rollCounter);
			base.bodyChunks[0].vel = Custom.DegToVec((float)rollDirection * Mathf.Lerp(60f, 35f, t)) * Mathf.Lerp(9.5f, 13.1f, t) * num * (isSlugpup ? 0.65f : 1f);
			base.bodyChunks[1].vel = Custom.DegToVec((float)rollDirection * Mathf.Lerp(60f, 35f, t)) * Mathf.Lerp(9.5f, 13.1f, t) * num * (isSlugpup ? 0.65f : 1f);
			base.bodyChunks[0].vel.x *= (isRivulet ? 1.5f : 1f);
			base.bodyChunks[1].vel.x *= (isRivulet ? 1.5f : 1f);
			animation = AnimationIndex.RocketJump;
			room.PlaySound(SoundID.Slugcat_Rocket_Jump, base.mainBodyChunk, loop: false, 1f, 1f);
			rollDirection = 0;
		}
		else if (animation == AnimationIndex.BellySlide)
		{
			float num2 = 9f;
			if (isRivulet)
			{
				num2 = 18f;
				if (isGourmand && ModManager.Expedition && Custom.rainWorld.ExpeditionMode && ExpeditionGame.activeUnlocks.Contains("unl-agility"))
				{
					num2 = Mathf.Lerp(14f, 9f, aerobicLevel);
					AerobicIncrease(1f);
				}
			}
			if (isSlugpup)
			{
				num2 = 6f;
			}
			if (whiplashJump || input[0].x == -rollDirection)
			{
				animation = AnimationIndex.Flip;
				standing = true;
				room.AddObject(new ExplosionSpikes(room, base.bodyChunks[1].pos + new Vector2(0f, 0f - base.bodyChunks[1].rad), 8, 7f, 5f, 5.5f, 40f, new Color(1f, 1f, 1f, 0.5f)));
				int num3 = 1;
				for (int j = 1; j < 4 && !room.GetTile(base.bodyChunks[0].pos + new Vector2((float)(j * -rollDirection) * 15f, 0f)).Solid && !room.GetTile(base.bodyChunks[0].pos + new Vector2((float)(j * -rollDirection) * 15f, 20f)).Solid; j++)
				{
					num3 = j;
				}
				base.bodyChunks[0].pos += new Vector2((float)rollDirection * (0f - ((float)num3 * 15f + 8f)), 14f);
				base.bodyChunks[1].pos += new Vector2((float)rollDirection * (0f - ((float)num3 * 15f + 2f)), 0f);
				base.bodyChunks[0].vel = new Vector2((float)rollDirection * (isRivulet ? (-11f) : (-7f)), isRivulet ? 12f : 10f);
				base.bodyChunks[1].vel = new Vector2((float)rollDirection * (isRivulet ? (-11f) : (-7f)), isRivulet ? 13f : 11f);
				rollDirection = -rollDirection;
				flipFromSlide = true;
				whiplashJump = false;
				jumpBoost = 0f;
				room.PlaySound(SoundID.Slugcat_Sectret_Super_Wall_Jump, base.mainBodyChunk, loop: false, 1f, 1f);
				if (pickUpCandidate != null && CanIPickThisUp(pickUpCandidate) && (base.grasps[0] == null || base.grasps[1] == null) && (Grabability(pickUpCandidate) == ObjectGrabability.OneHand || Grabability(pickUpCandidate) == ObjectGrabability.BigOneHand))
				{
					int graspUsed = ((base.grasps[0] != null) ? 1 : 0);
					for (int k = 0; k < pickUpCandidate.grabbedBy.Count; k++)
					{
						pickUpCandidate.grabbedBy[k].grabber.GrabbedObjectSnatched(pickUpCandidate.grabbedBy[k].grabbed, this);
						pickUpCandidate.grabbedBy[k].grabber.ReleaseGrasp(pickUpCandidate.grabbedBy[k].graspUsed);
					}
					SlugcatGrab(pickUpCandidate, graspUsed);
					if (pickUpCandidate is PlayerCarryableItem)
					{
						(pickUpCandidate as PlayerCarryableItem).PickedUp(this);
					}
					if (pickUpCandidate.graphicsModule != null)
					{
						pickUpCandidate.graphicsModule.BringSpritesToFront();
					}
				}
			}
			else
			{
				float y = 8.5f;
				if (isRivulet)
				{
					y = 10f;
				}
				if (isSlugpup)
				{
					y = 6f;
				}
				base.bodyChunks[1].pos += new Vector2(5f * (float)rollDirection, 5f);
				base.bodyChunks[0].pos = base.bodyChunks[1].pos + new Vector2(5f * (float)rollDirection, 5f);
				base.bodyChunks[1].vel = new Vector2((float)rollDirection * num2, y) * num * (longBellySlide ? 1.2f : 1f);
				base.bodyChunks[0].vel = new Vector2((float)rollDirection * num2, y) * num * (longBellySlide ? 1.2f : 1f);
				animation = AnimationIndex.RocketJump;
				rocketJumpFromBellySlide = true;
				room.PlaySound(SoundID.Slugcat_Rocket_Jump, base.mainBodyChunk, loop: false, 1f, 1f);
				rollDirection = 0;
			}
		}
		else if (animation == AnimationIndex.AntlerClimb)
		{
			animation = AnimationIndex.None;
			jumpBoost = 0f;
			base.bodyChunks[0].vel = playerInAntlers.antlerChunk.vel;
			if (!playerInAntlers.dangle)
			{
				base.bodyChunks[1].vel = playerInAntlers.antlerChunk.vel;
			}
			if (playerInAntlers.dangle)
			{
				if (input[0].x == 0)
				{
					base.bodyChunks[0].vel.y += 3f;
					base.bodyChunks[1].vel.y -= 3f;
					standing = true;
					room.PlaySound(SoundID.Slugcat_Climb_Along_Horizontal_Beam, base.mainBodyChunk, loop: false, 1f, 1f);
				}
				else
				{
					base.bodyChunks[1].vel.y += 4f;
					base.bodyChunks[1].vel.x += 2f * (float)input[0].x;
					base.bodyChunks[0].vel.y += 6f;
					base.bodyChunks[0].vel.x += 3f * (float)input[0].x;
					room.PlaySound(SoundID.Slugcat_From_Vertical_Pole_Jump, base.mainBodyChunk, loop: false, 0.15f, 1f);
				}
			}
			else if (input[0].x == 0)
			{
				if (input[0].y > 0)
				{
					base.bodyChunks[0].vel.y += 4f * num;
					base.bodyChunks[1].vel.y += 3f * num;
					jumpBoost = (isSlugpup ? 7 : 8);
					room.PlaySound(SoundID.Slugcat_From_Horizontal_Pole_Jump, base.mainBodyChunk, loop: false, 1f, 1f);
					standing = true;
				}
				else
				{
					base.bodyChunks[0].vel.y = 3f;
					base.bodyChunks[1].vel.y = -3f;
					standing = true;
					room.PlaySound(SoundID.Slugcat_Climb_Along_Horizontal_Beam, base.mainBodyChunk, loop: false, 1f, 1f);
				}
			}
			else
			{
				base.bodyChunks[0].vel.y += 8f * num;
				base.bodyChunks[1].vel.y += 7f * num;
				base.bodyChunks[0].vel.x += 6f * (float)input[0].x * num;
				base.bodyChunks[1].vel.x += 5f * (float)input[0].x * num;
				room.PlaySound(SoundID.Slugcat_From_Vertical_Pole_Jump, base.mainBodyChunk, loop: false, 1f, 1f);
			}
			Vector2 vector = base.bodyChunks[0].vel - playerInAntlers.antlerChunk.vel + (base.bodyChunks[1].vel - playerInAntlers.antlerChunk.vel) * (playerInAntlers.dangle ? 0f : 1f);
			vector -= Custom.DirVec(base.mainBodyChunk.pos, playerInAntlers.deer.mainBodyChunk.pos) * vector.magnitude;
			vector.x *= 0.1f;
			vector = Vector2.ClampMagnitude(vector, 10f);
			playerInAntlers.antlerChunk.vel -= vector * 1.2f;
			playerInAntlers.deer.mainBodyChunk.vel -= vector * 0.25f;
			playerInAntlers.playerDisconnected = true;
			playerInAntlers = null;
		}
		else
		{
			if (animation == AnimationIndex.ZeroGSwim || animation == AnimationIndex.ZeroGPoleGrab)
			{
				return;
			}
			int num4 = input[0].x;
			bool flag = false;
			if (animation == AnimationIndex.DownOnFours && base.bodyChunks[1].ContactPoint.y < 0 && input[0].downDiagonal == flipDirection)
			{
				animation = AnimationIndex.BellySlide;
				rollDirection = flipDirection;
				rollCounter = 0;
				standing = false;
				room.PlaySound(SoundID.Slugcat_Belly_Slide_Init, base.mainBodyChunk, loop: false, 1f, 1f);
				flag = true;
			}
			if (flag)
			{
				return;
			}
			animation = AnimationIndex.None;
			if (standing)
			{
				if (slideCounter > 0 && slideCounter < 10)
				{
					if (PainJumps)
					{
						base.bodyChunks[0].vel.y = 4f * num;
						base.bodyChunks[1].vel.y = 3f * num;
					}
					else
					{
						base.bodyChunks[0].vel.y = (isRivulet ? 12f : 9f) * num;
						base.bodyChunks[1].vel.y = (isRivulet ? 10f : 7f) * num;
					}
					base.bodyChunks[0].vel.x *= 0.5f;
					base.bodyChunks[1].vel.x *= 0.5f;
					base.bodyChunks[0].vel.x -= (float)slideDirection * 4f * num;
					jumpBoost = 5f;
					if (isRivulet)
					{
						jumpBoost = 9f;
						if (isGourmand && ModManager.Expedition && Custom.rainWorld.ExpeditionMode && ExpeditionGame.activeUnlocks.Contains("unl-agility"))
						{
							jumpBoost = Mathf.Lerp(8f, 2f, aerobicLevel);
							AerobicIncrease(2f);
						}
					}
					if (isSlugpup)
					{
						jumpBoost = 3f;
					}
					animation = AnimationIndex.Flip;
					room.PlaySound(SoundID.Slugcat_Flip_Jump, base.mainBodyChunk, loop: false, 1f, 1f);
					slideCounter = 0;
				}
				else
				{
					if (PainJumps)
					{
						base.bodyChunks[0].vel.y = 2f * num;
						base.bodyChunks[1].vel.y = 1f * num;
					}
					else
					{
						base.bodyChunks[0].vel.y = (isRivulet ? 6f : 4f) * num;
						base.bodyChunks[1].vel.y = (isRivulet ? 5f : 3f) * num;
					}
					jumpBoost = (isSlugpup ? 7 : 8);
					room.PlaySound((bodyMode == BodyModeIndex.ClimbingOnBeam) ? SoundID.Slugcat_From_Horizontal_Pole_Jump : SoundID.Slugcat_Normal_Jump, base.mainBodyChunk, loop: false, 1f, 1f);
				}
			}
			else
			{
				float num5 = 1.5f;
				if (superLaunchJump >= 20)
				{
					superLaunchJump = 0;
					num5 = 9f;
					if (PainJumps)
					{
						num5 = 2.5f;
					}
					else if (isRivulet)
					{
						num5 = 12f;
						if (isGourmand && ModManager.Expedition && Custom.rainWorld.ExpeditionMode && ExpeditionGame.activeUnlocks.Contains("unl-agility"))
						{
							num5 = Mathf.Lerp(8f, 3f, aerobicLevel);
						}
					}
					else if (isSlugpup)
					{
						num5 = 5.5f;
					}
					num4 = ((base.bodyChunks[0].pos.x > base.bodyChunks[1].pos.x) ? 1 : (-1));
					simulateHoldJumpButton = 6;
				}
				base.bodyChunks[0].pos.y += 6f;
				if (base.bodyChunks[0].ContactPoint.y == -1)
				{
					base.bodyChunks[0].vel.y += 3f * num;
					if (num4 == 0)
					{
						base.bodyChunks[0].vel.y += 3f * num;
					}
				}
				base.bodyChunks[1].vel.y += 4f * num;
				jumpBoost = 6f;
				if (num4 != 0 && base.bodyChunks[0].pos.x > base.bodyChunks[1].pos.x == num4 > 0)
				{
					base.bodyChunks[0].vel.x += (float)num4 * num5 * num;
					base.bodyChunks[1].vel.x += (float)num4 * num5 * num;
					room.PlaySound((num5 >= 9f) ? SoundID.Slugcat_Super_Jump : SoundID.Slugcat_Crouch_Jump, base.mainBodyChunk, loop: false, 1f, 1f);
				}
			}
			if (base.bodyChunks[1].onSlope != 0)
			{
				if (num4 == -base.bodyChunks[1].onSlope)
				{
					base.bodyChunks[1].vel.x += (float)base.bodyChunks[1].onSlope * 8f * num;
					return;
				}
				base.bodyChunks[0].vel.x += (float)base.bodyChunks[1].onSlope * 1.8f * num;
				base.bodyChunks[1].vel.x += (float)base.bodyChunks[1].onSlope * 1.2f * num;
			}
		}
	}

	public void JumpOnChunk()
	{
		if (jumpChunk == null || jumpChunk.owner.room != room)
		{
			return;
		}
		room.PlaySound(SoundID.Slugcat_Jump_On_Creature, base.bodyChunks[1]);
		float f = Mathf.Clamp(jumpChunk.owner.TotalMass / base.TotalMass, 0f, 1f);
		f = Mathf.Pow(f, 0.7f);
		base.bodyChunks[0].vel.y *= 1f - f * 0.9f;
		base.bodyChunks[1].vel.y *= 1f - f * 0.9f;
		f *= Mathf.Lerp(1f, 1.35f, Adrenaline);
		base.bodyChunks[0].vel.y += 4f * f;
		base.bodyChunks[1].vel.y += 3f * f;
		jumpBoost = 7f;
		jumpChunk.vel.y -= 7f * f * base.TotalMass / jumpChunk.mass;
		if (jumpChunk.owner is Creature)
		{
			(jumpChunk.owner as Creature).Stun(1);
		}
		if (jumpChunk.owner is Snail)
		{
			(jumpChunk.owner as Snail).clickCounter += 0.8f;
			if ((jumpChunk.owner as Snail).clickCounter > 1f)
			{
				(jumpChunk.owner as Snail).Click();
				base.bodyChunks[0].vel.y += 12f;
				base.bodyChunks[1].vel.y += 9f;
				if (base.stun > 20)
				{
					base.stun = 20;
				}
			}
		}
		jumpChunk = null;
	}

	private void GrabVerticalPole()
	{
		IntVector2 tilePosition = room.GetTilePosition(base.mainBodyChunk.pos);
		bool flag = base.bodyChunks[1].ContactPoint.y < 0 && input[0].x == 0;
		for (int i = 0; i < ((!flag) ? 1 : 3); i++)
		{
			int num = ((i > 0) ? ((i != 1) ? 1 : (-1)) : 0);
			if (!room.GetTile(tilePosition + new IntVector2(num, 0)).verticalBeam)
			{
				continue;
			}
			IntVector2 pos = tilePosition + new IntVector2(num, 0);
			room.PlaySound(SoundID.Slugcat_Grab_Beam, base.mainBodyChunk, loop: false, 1f, 1f);
			animation = AnimationIndex.ClimbOnBeam;
			if (num == 0 && Mathf.Abs(base.bodyChunks[0].vel.x) > 5f)
			{
				if (base.bodyChunks[0].vel.x < 0f)
				{
					flipDirection = -1;
				}
				else
				{
					flipDirection = 1;
				}
			}
			else if (base.bodyChunks[0].pos.x < room.MiddleOfTile(pos).x)
			{
				flipDirection = -1;
			}
			else
			{
				flipDirection = 1;
			}
			base.bodyChunks[0].vel = new Vector2(0f, 0f);
			base.bodyChunks[0].pos.x = room.MiddleOfTile(pos).x;
		}
	}

	private void DirectIntoHoles()
	{
		for (int i = 0; i < 4; i++)
		{
			if (room.GetTile(base.mainBodyChunk.pos + Custom.fourDirections[i].ToVector2() * 20f).Solid)
			{
				return;
			}
		}
		if (((input[0].x == 0 || input[0].y != 0) && (input[0].y == 0 || input[0].x != 0)) || room.GetTile(base.mainBodyChunk.pos + new Vector2(20f * (float)input[0].x, 20f * (float)input[0].y)).Solid || ((!room.GetTile(base.mainBodyChunk.pos + new Vector2(20f * (float)input[0].x, 20f * (float)input[0].y) + new Vector2(-20f, 0f)).Solid || !room.GetTile(base.mainBodyChunk.pos + new Vector2(20f * (float)input[0].x, 20f * (float)input[0].y) + new Vector2(20f, 0f)).Solid) && (!room.GetTile(base.mainBodyChunk.pos + new Vector2(20f * (float)input[0].x, 20f * (float)input[0].y) + new Vector2(0f, -20f)).Solid || !room.GetTile(base.mainBodyChunk.pos + new Vector2(20f * (float)input[0].x, 20f * (float)input[0].y) + new Vector2(0f, 20f)).Solid)))
		{
			return;
		}
		if (room.GetTile(base.mainBodyChunk.pos + new Vector2(40f * (float)input[0].x, 40f * (float)input[0].y)).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
		{
			ShortcutData.Type shortCutType = room.shortcutData(room.GetTilePosition(base.mainBodyChunk.pos + new Vector2(40f * (float)input[0].x, 40f * (float)input[0].y))).shortCutType;
			if ((ModManager.MSC && shortCutType == ShortcutData.Type.RoomExit && room.world.game.IsArenaSession && room.world.game.GetArenaGameSession.arenaSitting.gameTypeSetup.gameType == MoreSlugcatsEnums.GameTypeID.Challenge && !room.world.game.GetArenaGameSession.exitManager.ExitsOpen()) || (shortCutType != ShortcutData.Type.Normal && shortCutType != ShortcutData.Type.RoomExit))
			{
				return;
			}
		}
		base.mainBodyChunk.vel += (room.MiddleOfTile(base.mainBodyChunk.pos + new Vector2(20f * (float)input[0].x, 20f * (float)input[0].y)) - base.mainBodyChunk.pos) / 10f;
	}
}
