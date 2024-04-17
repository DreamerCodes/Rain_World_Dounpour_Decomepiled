using System;
using System.Collections.Generic;
using System.Globalization;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class SporePlant : Weapon, IDrawable
{
	public class AbstractSporePlant : AbstractConsumable
	{
		public bool used;

		public bool pacified;

		public AbstractSporePlant(World world, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID, int originRoom, int placedObjectIndex, PlacedObject.ConsumableObjectData consumableData, bool used, bool pacified)
			: base(world, AbstractObjectType.SporePlant, realizedObject, pos, ID, originRoom, placedObjectIndex, consumableData)
		{
			this.used = used;
			this.pacified = pacified;
		}

		public override string ToString()
		{
			string baseString = string.Format(CultureInfo.InvariantCulture, "{0}<oA>{1}<oA>{2}<oA>{3}<oA>{4}<oA>{5}<oA>{6}", ID.ToString(), type.ToString(), pos.SaveToString(), originRoom, placedObjectIndex, used ? "1" : "0", pacified ? "1" : "0");
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "<oA>", unrecognizedAttributes);
		}
	}

	public class Bee : UpdatableAndDeletable, IDrawable, Explosion.IReactToExplosions
	{
		public class Mode : ExtEnum<Mode>
		{
			public static readonly Mode LostHive = new Mode("LostHive", register: true);

			public static readonly Mode BuzzAroundHive = new Mode("BuzzAroundHive", register: true);

			public static readonly Mode GetBackToHive = new Mode("GetBackToHive", register: true);

			public static readonly Mode FollowPath = new Mode("FollowPath", register: true);

			public static readonly Mode Hover = new Mode("Hover", register: true);

			public static readonly Mode Hunt = new Mode("Hunt", register: true);

			public Mode(string value, bool register = false)
				: base(value, register)
			{
			}
		}

		public Vector2 pos;

		public Vector2 lastPos;

		public Vector2 lastLastPos;

		public Vector2 lastLastLastPos;

		public Vector2 vel;

		public Vector2 flyDir;

		public Vector2 lastFlyDir;

		public Vector2 hoverPos;

		public BodyChunk huntChunk;

		public Mode mode;

		public SporePlant owner;

		public float flySpeed;

		public List<IntVector2> path;

		public int travelDist;

		public int inModeCounter;

		private int group;

		private float blinkFreq;

		private float blink;

		private float lastBlink;

		public float life;

		public float lifeTime;

		private float boostTrail;

		private float lastBoostTrail;

		public bool angry;

		private SharedPhysics.TerrainCollisionData scratchTerrainCollisionData;

		public Creature ignoreCreature;

		public bool forceAngry;

		private List<DebugSprite> dbSprts;

		private Color blackColor;

		public void DebugShowPath()
		{
			dbSprts = new List<DebugSprite>();
			for (int i = 0; i < path.Count; i++)
			{
				DebugSprite debugSprite = new DebugSprite(room.MiddleOfTile(path[i]), new FSprite("pixel"), room);
				room.AddObject(debugSprite);
				debugSprite.sprite.scale = 16f;
				debugSprite.sprite.alpha = 0.4f;
				dbSprts.Add(debugSprite);
			}
		}

		public Bee(SporePlant owner, bool angry, Vector2 pos, Vector2 vel, Mode initMode)
		{
			this.owner = owner;
			this.pos = pos;
			lastPos = pos;
			lastLastPos = lastPos;
			this.vel = vel;
			hoverPos = pos;
			this.angry = angry;
			ChangeMode(initMode);
			flyDir = Custom.RNV();
			blink = UnityEngine.Random.value;
			lastBlink = blink;
			flySpeed = Mathf.Lerp(0.1f, 1.1f, UnityEngine.Random.value);
			group = UnityEngine.Random.Range(0, 4);
			life = 1f;
			lifeTime = Mathf.Lerp(300f, 600f, Custom.ClampedRandomVariation(0.5f, 0.5f, 0.3f));
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			inModeCounter++;
			lastLastLastPos = lastLastPos;
			lastLastPos = lastPos;
			lastPos = pos;
			pos += vel;
			vel *= 0.9f;
			flyDir.Normalize();
			lastFlyDir = flyDir;
			vel += flyDir * flySpeed;
			flyDir += Custom.RNV() * UnityEngine.Random.value * ((mode == Mode.LostHive) ? 1.2f : 0.6f);
			lastBlink = blink;
			blink += blinkFreq;
			lastBoostTrail = boostTrail;
			boostTrail = Mathf.Max(0f, boostTrail - 0.3f);
			SharedPhysics.TerrainCollisionData cd = scratchTerrainCollisionData.Set(pos, lastPos, vel, 1f, new IntVector2(0, 0), goThroughFloors: true);
			SharedPhysics.VerticalCollision(room, cd);
			SharedPhysics.HorizontalCollision(room, cd);
			pos = cd.pos;
			vel = cd.vel;
			if (mode != Mode.BuzzAroundHive || mode != Mode.GetBackToHive)
			{
				life -= 1f / lifeTime;
			}
			if (life < 0.2f * UnityEngine.Random.value)
			{
				vel.y -= Mathf.InverseLerp(0.2f, 0f, life);
				if (life <= 0f && (cd.contactPoint.y < 0 || pos.y < -100f))
				{
					Destroy();
				}
				flySpeed = Mathf.Min(flySpeed, Mathf.Max(0f, life) * 3f);
				if (room.water && pos.y < room.FloatWaterLevel(pos.x))
				{
					Destroy();
				}
				return;
			}
			if (room.water && pos.y < room.FloatWaterLevel(pos.x))
			{
				pos.y = room.FloatWaterLevel(pos.x) + 1f;
				vel.y += 1f;
				flyDir.y += 1f;
			}
			if (cd.contactPoint.x != 0)
			{
				flyDir.x -= cd.contactPoint.x;
			}
			if (cd.contactPoint.y != 0)
			{
				flyDir.y -= cd.contactPoint.y;
			}
			if (owner != null)
			{
				if (UnityEngine.Random.value < Mathf.Pow(owner.angry, 7f) / 40f)
				{
					angry = true;
				}
				if (UnityEngine.Random.value < 0.025f && (mode == Mode.BuzzAroundHive || mode == Mode.GetBackToHive) && (!Custom.DistLess(pos, owner.firstChunk.pos, 300f) || owner.room != room))
				{
					LoseOwner();
				}
			}
			if (huntChunk != null && mode != Mode.Hunt)
			{
				ChangeMode(Mode.Hunt);
			}
			if (mode != Mode.LostHive && owner == null && !forceAngry)
			{
				LoseOwner();
			}
			if (forceAngry)
			{
				angry = true;
				if (huntChunk == null)
				{
					mode = Mode.Hover;
				}
				else
				{
					mode = Mode.Hunt;
				}
			}
			if (mode == Mode.LostHive)
			{
				blinkFreq = Custom.LerpAndTick(blinkFreq, 1f / 30f, 0.05f, 1f / 30f);
				flySpeed = Custom.LerpAndTick(flySpeed, 0.9f, 0.08f, UnityEngine.Random.value / 30f);
				if (UnityEngine.Random.value < 0.2f && (cd.contactPoint.x != 0 || cd.contactPoint.y != 0))
				{
					Destroy();
				}
			}
			else if (mode == Mode.BuzzAroundHive || mode == Mode.GetBackToHive)
			{
				blinkFreq = Custom.LerpAndTick(blinkFreq, angry ? (1f / 6f) : 0f, 0.05f, 0.0125f);
				if (UnityEngine.Random.value < 0.0125f)
				{
					blinkFreq = Mathf.Max(blinkFreq, UnityEngine.Random.value / 30f);
				}
				float num = Mathf.InverseLerp(10f, Mathf.Min(15f + Vector2.Distance(owner.firstChunk.lastPos, owner.firstChunk.pos) * 10f, 150f), Vector2.Distance(pos, owner.firstChunk.pos));
				if (UnityEngine.Random.value < 0.0025f)
				{
					ChangeMode(Mode.GetBackToHive);
				}
				if ((cd.contactPoint.x != 0 || cd.contactPoint.y != 0) && (!Custom.DistLess(pos, owner.firstChunk.pos, 300f) || (UnityEngine.Random.value < 0.1f && !Custom.DistLess(pos, owner.firstChunk.pos, 50f) && !room.VisualContact(pos, owner.firstChunk.pos))))
				{
					LoseOwner();
					return;
				}
				if (num > 0f)
				{
					flySpeed = Custom.LerpAndTick(flySpeed, Mathf.Clamp(Vector2.Distance(owner.firstChunk.lastPos, owner.firstChunk.pos) * num * Mathf.InverseLerp(-1f, 1f, Vector2.Dot(flyDir.normalized, Custom.DirVec(pos, owner.firstChunk.pos))), 0.4f, 1.1f), 0.08f, UnityEngine.Random.value / 30f);
					flyDir += Custom.DirVec(pos, owner.firstChunk.pos) * num * UnityEngine.Random.value;
				}
				else if (mode == Mode.GetBackToHive)
				{
					flySpeed = Custom.LerpAndTick(flySpeed, Mathf.Clamp(Mathf.InverseLerp(-1f, 1f, Vector2.Dot(flyDir.normalized, Custom.DirVec(pos, owner.firstChunk.pos))), 0.4f, 1.1f), 0.08f, UnityEngine.Random.value / 30f);
					flyDir = Vector2.Lerp(flyDir, Custom.DirVec(pos, owner.firstChunk.pos), 0.6f);
					vel *= 0.8f;
					if (Custom.DistLess(pos, owner.firstChunk.pos, 6f))
					{
						Destroy();
					}
				}
			}
			else if (mode == Mode.FollowPath)
			{
				blinkFreq = Custom.LerpAndTick(blinkFreq, 1f / 3f, 0.05f, 1f / 30f);
				vel *= 0.9f;
				if (inModeCounter > UnityEngine.Random.Range(0, 15))
				{
					if (path == null || path.Count == 0)
					{
						ChangeMode(Mode.Hover);
						path = null;
						return;
					}
					flySpeed = Custom.LerpAndTick(flySpeed, Custom.LerpMap(((float)travelDist + Vector2.Distance(room.MiddleOfTile(path[path.Count - 1]), pos) / 20f) / 2f, 2f, 20f, 0.4f, 3.2f, 0.6f), 0.08f, UnityEngine.Random.value / 30f);
					flyDir = Vector2.Lerp(flyDir, Custom.DirVec(pos, room.MiddleOfTile(path[path.Count - 1])), Mathf.Lerp(0.5f, 1f, UnityEngine.Random.value));
					if (UnityEngine.Random.value < 1f / 60f)
					{
						room.PlaySound(SoundID.Spore_Bee_Angry_Buzz, pos, Custom.LerpMap(life, 0f, 0.25f, 0.1f, 0.5f) + UnityEngine.Random.value * 0.5f, Custom.LerpMap(life, 0f, 0.5f, 0.8f, 0.9f, 0.4f));
					}
					if (room.GetTilePosition(pos) == path[path.Count - 1])
					{
						path.RemoveAt(path.Count - 1);
					}
					if (path.Count > 1)
					{
						int num2 = UnityEngine.Random.Range(1, path.Count);
						if (room.VisualContact(pos, room.MiddleOfTile(path[num2])))
						{
							for (int num3 = path.Count - 1; num3 > num2; num3--)
							{
								path.RemoveAt(num2);
							}
						}
					}
					if ((UnityEngine.Random.value < 0.05f && (cd.contactPoint.x != 0 || cd.contactPoint.y != 0)) || (room.water && pos.y < room.FloatWaterLevel(pos.x) + 5f))
					{
						ChangeMode(Mode.Hover);
						path = null;
						return;
					}
				}
			}
			else if (mode == Mode.Hover)
			{
				vel *= 0.82f;
				flySpeed = 0f;
				if (owner != null)
				{
					vel += Vector2.ClampMagnitude(hoverPos + owner.HoverOffset(group) - pos, 60f) / 20f * 3f;
				}
				flyDir += vel * 0.5f;
				vel += Custom.RNV() * 0.3f;
				flyDir.y += 0.75f;
				blinkFreq = Custom.LerpAndTick(blinkFreq, 1f / 6f, 0.05f, 1f / 30f);
				if (blink % 4f > (float)group)
				{
					blink -= 0.01f;
				}
				if (UnityEngine.Random.value < 0.0025f)
				{
					room.AddObject(new BeeSpark(pos));
				}
				if (UnityEngine.Random.value < 1f / 60f)
				{
					room.PlaySound(SoundID.Spore_Bee_Angry_Buzz, pos, Custom.LerpMap(life, 0f, 0.25f, 0.1f, 0.5f) + UnityEngine.Random.value * 0.5f, Custom.LerpMap(life, 0f, 0.5f, 0.8f, 0.9f, 0.4f));
				}
				if (owner != null && owner.bees.Count > 1)
				{
					Bee bee = owner.bees[UnityEngine.Random.Range(0, owner.bees.Count)];
					if (bee != this && (bee.mode == Mode.Hover || bee.huntChunk != null) && Custom.DistLess(pos, bee.pos, (bee.mode == Mode.Hover && bee.huntChunk == null) ? 60f : 300f) && room.VisualContact(pos, bee.pos))
					{
						if (bee.huntChunk != null && bee.huntChunk.owner.TotalMass > 0.3f && UnityEngine.Random.value < CareAboutChunk(bee.huntChunk))
						{
							if (HuntChunkIfPossible(bee.huntChunk))
							{
								return;
							}
							if (Vector2.Distance(bee.pos, bee.huntChunk.pos) < Vector2.Distance(hoverPos, bee.huntChunk.pos))
							{
								hoverPos = bee.pos;
							}
						}
						else if (bee.mode == Mode.Hover && UnityEngine.Random.value < 0.1f)
						{
							Vector2 vector = hoverPos;
							hoverPos = bee.hoverPos;
							bee.hoverPos = vector;
						}
					}
				}
			}
			else if (mode == Mode.Hunt)
			{
				blinkFreq = Custom.LerpAndTick(blinkFreq, 1f / 3f, 0.05f, 1f / 30f);
				float num4 = Mathf.InverseLerp(-1f, 1f, Vector2.Dot(flyDir.normalized, Custom.DirVec(pos, huntChunk.pos)));
				flySpeed = Custom.LerpAndTick(flySpeed, Mathf.Clamp(Mathf.InverseLerp(huntChunk.rad, huntChunk.rad + 110f, Vector2.Distance(pos, huntChunk.pos)) * 2f + num4, 0.4f, 2.2f), 0.08f, UnityEngine.Random.value / 30f);
				flySpeed = Custom.LerpAndTick(flySpeed, Custom.LerpMap(Vector2.Dot(flyDir.normalized, Custom.DirVec(pos, huntChunk.pos)), -1f, 1f, 0.4f, 1.8f), 0.08f, UnityEngine.Random.value / 30f);
				vel *= 0.9f;
				flyDir = Vector2.Lerp(flyDir, Custom.DirVec(pos, huntChunk.pos), UnityEngine.Random.value * 0.4f);
				if (UnityEngine.Random.value < 1f / 30f)
				{
					room.PlaySound(SoundID.Spore_Bee_Angry_Buzz, pos, Custom.LerpMap(life, 0f, 0.25f, 0.1f, 1f), Custom.LerpMap(life, 0f, 0.5f, 0.8f, 1.2f, 0.25f));
				}
				if (UnityEngine.Random.value < 0.1f && lastBoostTrail <= 0f && num4 > 0.7f && Custom.DistLess(pos, huntChunk.pos, huntChunk.rad + 150f) && !Custom.DistLess(pos, huntChunk.pos, huntChunk.rad + 50f) && room.VisualContact(pos, huntChunk.pos))
				{
					Vector2 vector2 = Vector3.Slerp(Custom.DirVec(pos, huntChunk.pos), flyDir.normalized, 0.5f);
					float num5 = Vector2.Distance(pos, huntChunk.pos) - huntChunk.rad;
					Vector2 b = pos + vector2 * num5;
					if (num5 > 30f && !room.GetTile(b).Solid && !room.PointSubmerged(b) && room.VisualContact(pos, b))
					{
						boostTrail = 1f;
						pos = b;
						vel = vector2 * 10f;
						flyDir = vector2;
						room.AddObject(new BeeSpark(lastPos));
						room.PlaySound(SoundID.Spore_Bee_Dash, lastPos);
						room.PlaySound(SoundID.Spore_Bee_Spark, pos, 0.2f, 1.5f);
					}
				}
				for (int i = 0; i < huntChunk.owner.bodyChunks.Length; i++)
				{
					if (Custom.DistLess(pos, huntChunk.owner.bodyChunks[i].pos, huntChunk.owner.bodyChunks[i].rad))
					{
						Attach(huntChunk.owner.bodyChunks[i]);
						return;
					}
				}
				if (!Custom.DistLess(pos, huntChunk.pos, huntChunk.rad + 400f) || (UnityEngine.Random.value < 0.1f && huntChunk.submersion > 0.8f) || ObjectAlreadyStuck(huntChunk.owner) || !room.VisualContact(pos, huntChunk.pos))
				{
					huntChunk = null;
					ChangeMode(Mode.Hover);
					return;
				}
			}
			if (angry && huntChunk == null)
			{
				LookForRandomCreatureToHunt();
			}
		}

		private float CareAboutChunk(BodyChunk chunk)
		{
			if (owner == null)
			{
				return 1f;
			}
			float num = Vector2.Distance(chunk.pos, owner.firstChunk.pos);
			if (owner.swarmPos.HasValue)
			{
				num = Mathf.Min(num, Vector2.Distance(chunk.pos, owner.swarmPos.Value));
			}
			if (num < 190f)
			{
				return 1f;
			}
			return Mathf.InverseLerp(Custom.LerpMap(chunk.owner.TotalMass, 0.3f, 2f, 191f, 420f, 0.45f), 190f, num);
		}

		private bool LookForRandomCreatureToHunt()
		{
			if (ModManager.MMF && !MMF.cfgVanillaExploits.Value && room.abstractRoom.gate && room.regionGate.waitingForWorldLoader)
			{
				return false;
			}
			if (huntChunk != null)
			{
				return false;
			}
			if (room.abstractRoom.creatures.Count > 0)
			{
				AbstractCreature abstractCreature = room.abstractRoom.creatures[UnityEngine.Random.Range(0, room.abstractRoom.creatures.Count)];
				if (abstractCreature.realizedCreature != null && abstractCreature.realizedCreature.room == room && (ignoreCreature == null || abstractCreature.realizedCreature != ignoreCreature) && SporePlantInterested(abstractCreature.realizedCreature.Template.type))
				{
					for (int i = 0; i < abstractCreature.realizedCreature.bodyChunks.Length; i++)
					{
						if (Custom.DistLess(pos, abstractCreature.realizedCreature.bodyChunks[i].pos, abstractCreature.realizedCreature.bodyChunks[i].rad))
						{
							Attach(abstractCreature.realizedCreature.bodyChunks[i]);
							return true;
						}
					}
					return HuntChunkIfPossible(abstractCreature.realizedCreature.bodyChunks[UnityEngine.Random.Range(0, abstractCreature.realizedCreature.bodyChunks.Length)]);
				}
			}
			if (UnityEngine.Random.value < 0.1f && owner != null && owner.attachedBees.Count > 0)
			{
				AttachedBee attachedBee = owner.attachedBees[UnityEngine.Random.Range(0, owner.attachedBees.Count)];
				if (!attachedBee.slatedForDeletetion && attachedBee.attachedChunk != null)
				{
					return HuntChunkIfPossible(attachedBee.attachedChunk.owner.bodyChunks[UnityEngine.Random.Range(0, attachedBee.attachedChunk.owner.bodyChunks.Length)]);
				}
			}
			return false;
		}

		private bool HuntChunkIfPossible(BodyChunk potentialHuntChunk)
		{
			if (UnityEngine.Random.value > 0.2f + 0.8f * CareAboutChunk(potentialHuntChunk))
			{
				return false;
			}
			if (ObjectAlreadyStuck(potentialHuntChunk.owner))
			{
				return false;
			}
			if (potentialHuntChunk.submersion > 0.9f)
			{
				return false;
			}
			if (potentialHuntChunk.owner is Creature && !SporePlantInterested((potentialHuntChunk.owner as Creature).Template.type))
			{
				return false;
			}
			if (potentialHuntChunk != null && Custom.DistLess(pos, potentialHuntChunk.pos, 300f) && room.VisualContact(pos, potentialHuntChunk.pos))
			{
				huntChunk = potentialHuntChunk;
				ChangeMode(Mode.Hunt);
				return true;
			}
			return false;
		}

		private bool ObjectAlreadyStuck(PhysicalObject obj)
		{
			if (obj.TotalMass > 0.5f)
			{
				return false;
			}
			int num = 0;
			int num2 = Mathf.RoundToInt(Custom.LerpMap(obj.TotalMass, 0f, 0.5f, 3f, 30f, 1.5f));
			for (int i = 0; i < obj.abstractPhysicalObject.stuckObjects.Count; i++)
			{
				if (obj.abstractPhysicalObject.stuckObjects[i] is AttachedBee.BeeStick)
				{
					num++;
					if (num >= num2)
					{
						return true;
					}
				}
			}
			return false;
		}

		public void Attach(BodyChunk chunk)
		{
			if (!base.slatedForDeletetion)
			{
				AttachedBee attachedBee = new AttachedBee(room, new AbstractPhysicalObject(room.world, AbstractPhysicalObject.AbstractObjectType.AttachedBee, null, room.GetWorldCoordinate(pos), room.game.GetNewID()), chunk, pos, Custom.DirVec(lastLastPos, pos), life, lifeTime, boostTrail > 0f);
				room.AddObject(attachedBee);
				if (owner != null)
				{
					owner.attachedBees.Add(attachedBee);
				}
				room.PlaySound(SoundID.Spore_Bee_Attach_Creature, chunk);
				Destroy();
			}
		}

		public void ChangeMode(Mode newMode)
		{
			if (mode == newMode)
			{
				return;
			}
			if (newMode == Mode.Hover)
			{
				hoverPos = room.MiddleOfTile(pos) + new Vector2(Mathf.Lerp(-9f, 9f, UnityEngine.Random.value), Mathf.Lerp(-9f, 9f, UnityEngine.Random.value));
				if (room.water && room.PointSubmerged(hoverPos))
				{
					hoverPos.y = room.FloatWaterLevel(hoverPos.x) + 5f;
				}
				angry = true;
			}
			else if (newMode == Mode.FollowPath)
			{
				angry = true;
			}
			mode = newMode;
			inModeCounter = 0;
		}

		public void LoseOwner()
		{
			ChangeMode(Mode.LostHive);
			if (owner != null)
			{
				owner.RemoveBee(this);
			}
		}

		public override void Destroy()
		{
			base.Destroy();
			if (owner != null)
			{
				owner.RemoveBee(this);
			}
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[2];
			sLeaser.sprites[0] = new FSprite("bee");
			sLeaser.sprites[1] = new FSprite("pixel");
			AddToContainer(sLeaser, rCam, null);
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
			Vector2 vector2 = Vector2.Lerp(lastLastLastPos, lastLastPos, timeStacker);
			sLeaser.sprites[0].x = vector.x - camPos.x;
			sLeaser.sprites[0].y = vector.y - camPos.y;
			float num = Mathf.Lerp(lastBoostTrail, boostTrail, timeStacker);
			Vector2 v = Vector3.Slerp(Vector3.Slerp(lastFlyDir.normalized, flyDir.normalized, timeStacker), Custom.DirVec(vector2, vector), 0.5f + 0.5f * num);
			sLeaser.sprites[0].rotation = Custom.VecToDeg(v);
			float num2 = Custom.Decimal(Mathf.Lerp(lastBlink, blink, timeStacker));
			float num3 = Mathf.InverseLerp(0f, 0.5f, life);
			if ((blinkFreq > 0f && num2 < 0.5f * num3) || num > 0f)
			{
				float a = Mathf.Clamp(Mathf.Sin(Mathf.InverseLerp(0f, 0.5f * num3, num2) * (float)Math.PI), 0f, 1f);
				a = Mathf.Max(a, num);
				sLeaser.sprites[1].x = vector.x - camPos.x;
				sLeaser.sprites[1].y = vector.y - camPos.y;
				sLeaser.sprites[1].rotation = Custom.AimFromOneVectorToAnother(vector, vector2);
				sLeaser.sprites[1].scaleY = (Vector2.Distance(vector, vector2) + 2f) * Mathf.Pow(a, 0.25f);
				sLeaser.sprites[1].anchorY = 0f;
				sLeaser.sprites[1].color = Color.Lerp(new Color(1f, 0f, 0f), new Color(1f, 1f, 1f), Mathf.Pow(a, 3f) * num3);
				sLeaser.sprites[1].isVisible = true;
				sLeaser.sprites[0].color = Color.Lerp(blackColor, new Color(1f, 0f, 0f), Mathf.Pow(Mathf.InverseLerp(0.5f, 1f, a), 3f));
			}
			else
			{
				sLeaser.sprites[1].isVisible = false;
				sLeaser.sprites[0].color = blackColor;
			}
			if (base.slatedForDeletetion || room != rCam.room)
			{
				sLeaser.CleanSpritesAndRemove();
			}
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			blackColor = palette.blackColor;
		}

		public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			if (newContatiner == null)
			{
				newContatiner = rCam.ReturnFContainer("Items");
			}
			for (int i = 0; i < sLeaser.sprites.Length; i++)
			{
				sLeaser.sprites[i].RemoveFromContainer();
				newContatiner.AddChild(sLeaser.sprites[i]);
			}
		}

		public void Explosion(Explosion explosion)
		{
			if (Custom.DistLess(pos, explosion.pos, explosion.rad))
			{
				vel += Custom.DirVec(explosion.pos, pos) * Mathf.Min(20f, explosion.force * Mathf.InverseLerp(explosion.rad, explosion.rad / 2f, Vector2.Distance(pos, explosion.pos)) * 2f);
				life -= 1f;
			}
		}
	}

	public class AttachedBee : PhysicalObject, IDrawable
	{
		public class BeeStick : AbstractPhysicalObject.AbstractObjectStick
		{
			public BeeStick(AbstractPhysicalObject A, AbstractPhysicalObject B)
				: base(A, B)
			{
			}

			public override string SaveToString(int roomIndex)
			{
				return roomIndex + "<stkA>beeStk<stkA>" + A.ID.ToString() + "<stkA>" + B.ID.ToString();
			}
		}

		public BodyChunk attachedChunk;

		public Vector2 attachedPos;

		public Vector2 rot;

		public Vector2 lastRot;

		public Vector2 idealStingerDir;

		private bool stingerOut;

		private bool lastStingerOut;

		private Vector2 stingerAttachPos;

		private float ropeLength;

		private float shrinkTo;

		public int stingerWaitCounter;

		public Vector2[,] stinger;

		private bool popped;

		public float life;

		public float lifeTime;

		public AttachedBee(Room room, AbstractPhysicalObject abstrObj, BodyChunk attachedChunk, Vector2 beePos, Vector2 beeDir, float life, float lifeTime, bool beeBosting)
			: base(abstrObj)
		{
			this.attachedChunk = attachedChunk;
			this.life = life;
			this.lifeTime = lifeTime;
			base.bodyChunks = new BodyChunk[1];
			base.bodyChunks[0] = new BodyChunk(this, 0, beePos, 1f, 0.01f);
			bodyChunkConnections = new BodyChunkConnection[0];
			base.airFriction = 0.999f;
			base.gravity = 0.5f;
			bounce = 0.2f;
			surfaceFriction = 0.7f;
			collisionLayer = 0;
			base.waterFriction = 0.95f;
			base.buoyancy = 1.1f;
			base.CollideWithTerrain = false;
			idealStingerDir = Custom.DegToVec(Mathf.Lerp(-60f, 60f, UnityEngine.Random.value));
			attachedPos = Custom.RotateAroundOrigo((beePos - attachedChunk.pos) * 1f, 0f - Custom.VecToDeg(attachedChunk.Rotation));
			stinger = new Vector2[3, 3];
			if (beeBosting)
			{
				attachedChunk.vel += beeDir.normalized * 0.4f / attachedChunk.mass;
				if (UnityEngine.Random.value < 0.5f)
				{
					if (attachedChunk.owner is Creature)
					{
						(attachedChunk.owner as Creature).Violence(null, null, attachedChunk, null, Creature.DamageType.Stab, 0.002f, 10f * UnityEngine.Random.value * UnityEngine.Random.value);
					}
					popped = true;
					room.AddObject(new BeeSpark(beePos));
					room.PlaySound(SoundID.Spore_Bee_Spark, base.firstChunk);
				}
			}
			new BeeStick(abstractPhysicalObject, attachedChunk.owner.abstractPhysicalObject);
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			lastRot = rot;
			lastStingerOut = stingerOut;
			float num = 0f;
			if (attachedChunk != null)
			{
				Vector2 vector = attachedChunk.pos;
				if (attachedChunk.owner is Player && attachedChunk.owner.graphicsModule != null)
				{
					vector = Vector2.Lerp(attachedChunk.pos, (attachedChunk.owner.graphicsModule as PlayerGraphics).drawPositions[attachedChunk.index, 0], (attachedChunk.index == 0) ? 0.8f : 0.2f);
				}
				Vector2 vector2 = vector + Custom.RotateAroundOrigo(attachedPos, Custom.VecToDeg(attachedChunk.Rotation));
				float num2 = 3f;
				float num3 = Vector2.Distance(base.bodyChunks[0].pos, (vector + vector2) / 2f);
				rot = Custom.DirVec(base.bodyChunks[0].pos, vector2);
				if (num3 > num2)
				{
					base.bodyChunks[0].pos -= rot * (num2 - num3);
					base.bodyChunks[0].vel -= rot * (num2 - num3);
				}
				if (!stingerOut)
				{
					base.bodyChunks[0].vel += Custom.RNV() * UnityEngine.Random.value * 0.4f;
					stingerWaitCounter++;
					if (UnityEngine.Random.value < Mathf.InverseLerp(0f, 30f, stingerWaitCounter) / 10f && attachedChunk.owner.room == room && (!(attachedChunk.owner is Creature) || (!(attachedChunk.owner as Creature).enteringShortCut.HasValue && !(attachedChunk.owner as Creature).inShortcut)))
					{
						Vector2 vector3 = Vector3.Slerp(Custom.DirVec(vector, vector2), new Vector2(0f, 1f), 0.4f);
						Vector2 vector4 = vector + (Vector2)Vector3.Slerp(Vector3.Slerp(vector3, idealStingerDir, 0.2f), Custom.RNV(), 0.2f) * Mathf.Lerp(30f, 160f, UnityEngine.Random.value);
						IntVector2? intVector = ((UnityEngine.Random.value < 0.5f) ? SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(room, vector2, vector4) : SharedPhysics.RayTraceTilesForTerrainReturnFirstSolidOrPole(room, vector2, vector4));
						if (intVector.HasValue && !Custom.DistLess(base.firstChunk.pos, room.MiddleOfTile(intVector.Value), room.GetTile(intVector.Value).Solid ? (UnityEngine.Random.value * 50f) : 80f))
						{
							if (room.GetTile(intVector.Value).Solid)
							{
								FloatRect floatRect = Custom.RectCollision(vector4, vector2, room.TileRect(intVector.Value));
								stingerAttachPos = new Vector2(floatRect.left, floatRect.bottom);
							}
							else
							{
								stingerAttachPos = room.MiddleOfTile(intVector.Value);
								if (room.GetTile(intVector.Value).horizontalBeam)
								{
									stingerAttachPos.x += Mathf.Lerp(-10f, 10f, UnityEngine.Random.value);
								}
								if (room.GetTile(intVector.Value).verticalBeam)
								{
									stingerAttachPos.y += Mathf.Lerp(-10f, 10f, UnityEngine.Random.value);
								}
							}
							ropeLength = Mathf.Max(40f, Vector2.Distance(attachedChunk.pos, stingerAttachPos) + 5f);
							shrinkTo = Mathf.Max(40f, ropeLength * 0.5f);
							room.PlaySound(SoundID.Spore_Bee_Attach_Wall, stingerAttachPos);
							if (!popped)
							{
								popped = true;
								if (attachedChunk.owner is Creature)
								{
									(attachedChunk.owner as Creature).Violence(null, null, attachedChunk, null, Creature.DamageType.Stab, 0.002f, 10f * UnityEngine.Random.value * UnityEngine.Random.value);
								}
								attachedChunk.vel += Custom.DirVec(stingerAttachPos, attachedChunk.pos) * 0.4f / attachedChunk.mass;
								room.AddObject(new BeeSpark(base.firstChunk.pos));
								room.PlaySound(SoundID.Spore_Bee_Spark, base.firstChunk);
							}
							stingerOut = true;
						}
					}
				}
				else
				{
					if (attachedChunk.owner is Creature && (attachedChunk.owner as Creature).enteringShortCut.HasValue)
					{
						stingerOut = false;
					}
					else
					{
						base.firstChunk.vel += Custom.DirVec(vector, stingerAttachPos) * 10f;
						num3 = Vector2.Distance(attachedChunk.pos, stingerAttachPos);
						num = Mathf.InverseLerp(ropeLength * 1.6f + 40f, ropeLength * 2f + 90f, num3);
						ropeLength = Mathf.Max(shrinkTo, ropeLength - 0.4f);
						if (UnityEngine.Random.value < num)
						{
							BreakStinger();
						}
						else if (num3 > ropeLength)
						{
							Vector2 vector5 = Custom.DirVec(attachedChunk.pos, stingerAttachPos);
							attachedChunk.vel -= vector5 * Mathf.Min((ropeLength - num3) * 0.0045f, 0.75f) / attachedChunk.mass;
						}
					}
					if (attachedChunk != null && attachedChunk.owner.slatedForDeletetion)
					{
						BreakStinger();
					}
				}
			}
			else if (stingerOut)
			{
				rot = Custom.DirVec(base.bodyChunks[0].pos, stingerAttachPos);
				float num4 = Vector2.Distance(base.bodyChunks[0].pos, stingerAttachPos);
				if (num4 > ropeLength)
				{
					Vector2 vector6 = Custom.DirVec(base.bodyChunks[0].pos, stingerAttachPos);
					base.bodyChunks[0].vel -= vector6 * (ropeLength - num4) * 0.45f;
					base.bodyChunks[0].pos -= vector6 * (ropeLength - num4) * 0.45f;
				}
				ropeLength = Mathf.Max(0f, ropeLength - 2f);
				if (ropeLength == 0f)
				{
					stingerOut = false;
				}
			}
			else if (base.firstChunk.ContactPoint.y < 0 || base.firstChunk.pos.y < -100f || base.firstChunk.submersion > 0f)
			{
				Destroy();
			}
			if (stingerOut)
			{
				float num5 = Vector2.Distance(base.firstChunk.pos, stingerAttachPos);
				for (int i = 0; i < stinger.GetLength(0); i++)
				{
					stinger[i, 2] = stinger[i, 0];
					stinger[i, 0] += stinger[i, 1];
					stinger[i, 1] *= 0.95f;
					Vector2 vector7 = Vector2.Lerp(base.firstChunk.pos, stingerAttachPos, Mathf.Pow(Mathf.InverseLerp(-1f, stinger.GetLength(0), i), 0.5f));
					stinger[i, 1] += (vector7 - stinger[i, 0]) * 0.4f;
					stinger[i, 0] += (vector7 - stinger[i, 0]) * Mathf.Max(Mathf.InverseLerp(num5 / 100f, num5 / 15f, Vector2.Distance(stinger[i, 0], vector7)), num);
				}
				if (life <= 0f)
				{
					BreakStinger();
				}
			}
			else
			{
				for (int j = 0; j < stinger.GetLength(0); j++)
				{
					stinger[j, 2] = stinger[j, 0];
					stinger[j, 0] = base.bodyChunks[0].pos;
					stinger[j, 1] *= 0f;
				}
			}
			life -= 1f / (lifeTime * (stingerOut ? 1f : 2f));
		}

		public void BreakStinger()
		{
			if (attachedChunk != null)
			{
				ropeLength = Mathf.Clamp(Vector2.Distance(base.firstChunk.pos, stingerAttachPos), ropeLength - 30f, ropeLength + 30f);
				attachedChunk = null;
				base.CollideWithTerrain = true;
				base.gravity = 0.9f;
				room.PlaySound(SoundID.Spore_Bee_Fall_Off, base.firstChunk);
				if (UnityEngine.Random.value < 0.5f)
				{
					stingerOut = false;
				}
			}
		}

		public override void HitByExplosion(float hitFac, Explosion explosion, int hitChunk)
		{
			base.HitByExplosion(hitFac, explosion, hitChunk);
			life -= 1f;
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[5];
			sLeaser.sprites[0] = new FSprite("bee");
			for (int i = 1; i < 5; i++)
			{
				sLeaser.sprites[i] = new FSprite("pixel");
				sLeaser.sprites[i].anchorY = 0f;
			}
			AddToContainer(sLeaser, rCam, null);
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
			if (attachedChunk != null)
			{
				Vector2 vector2 = Vector2.Lerp(attachedChunk.lastPos, attachedChunk.pos, timeStacker);
				Vector2 v = ((attachedChunk.rotationChunk == null) ? attachedChunk.Rotation : Custom.DirVec(vector2, Vector2.Lerp(attachedChunk.rotationChunk.lastPos, attachedChunk.rotationChunk.pos, timeStacker)));
				if (attachedChunk.owner is Player && attachedChunk.owner.graphicsModule != null)
				{
					vector2 = Vector2.Lerp(vector2, Vector2.Lerp((attachedChunk.owner.graphicsModule as PlayerGraphics).drawPositions[attachedChunk.index, 1], (attachedChunk.owner.graphicsModule as PlayerGraphics).drawPositions[attachedChunk.index, 0], timeStacker), (attachedChunk.index == 0) ? 0.8f : 0.2f);
				}
				Vector2 vector3 = vector2 + Custom.RotateAroundOrigo(attachedPos, Custom.VecToDeg(v));
				if (!Custom.DistLess(vector, vector3, 3f))
				{
					vector = vector3 + Custom.DirVec(vector3, vector) * 3f;
				}
			}
			sLeaser.sprites[0].x = vector.x - camPos.x;
			sLeaser.sprites[0].y = vector.y - camPos.y;
			sLeaser.sprites[0].rotation = Custom.VecToDeg(Vector3.Slerp(lastRot, rot, timeStacker));
			float num = Mathf.Lerp(lastStingerOut ? 1f : 0f, stingerOut ? 1f : 0f, timeStacker);
			if (num > 0f)
			{
				for (int i = 0; i < stinger.GetLength(0) + 1; i++)
				{
					Vector2 b = ((i == 0) ? vector : Vector2.Lerp(stinger[i - 1, 2], stinger[i - 1, 0], timeStacker));
					b = Vector2.Lerp(vector, b, num);
					sLeaser.sprites[i + 1].x = b.x - camPos.x;
					sLeaser.sprites[i + 1].y = b.y - camPos.y;
					Vector2 vector4 = ((i == stinger.GetLength(0)) ? (vector4 = stingerAttachPos) : Vector2.Lerp(stinger[i, 2], stinger[i, 0], timeStacker));
					vector4 = Vector2.Lerp(vector, vector4, num);
					sLeaser.sprites[i + 1].rotation = Custom.AimFromOneVectorToAnother(b, vector4);
					sLeaser.sprites[i + 1].scaleY = Vector2.Distance(b, vector4) + 1f;
					sLeaser.sprites[i + 1].isVisible = true;
				}
			}
			else
			{
				for (int j = 0; j < stinger.GetLength(0) + 1; j++)
				{
					sLeaser.sprites[j + 1].isVisible = false;
				}
			}
			if (base.slatedForDeletetion || room != rCam.room)
			{
				sLeaser.CleanSpritesAndRemove();
			}
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			for (int i = 0; i < sLeaser.sprites.Length; i++)
			{
				sLeaser.sprites[i].color = palette.blackColor;
			}
		}

		public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			bool flag = UnityEngine.Random.value < 0.5f;
			if (newContatiner == null)
			{
				newContatiner = rCam.ReturnFContainer(flag ? "Items" : "Background");
			}
			for (int i = 0; i < sLeaser.sprites.Length; i++)
			{
				sLeaser.sprites[i].RemoveFromContainer();
				newContatiner.AddChild(sLeaser.sprites[i]);
			}
		}
	}

	public class Stalk : UpdatableAndDeletable, IDrawable
	{
		public SporePlant sporePlant;

		public Vector2 stuckPos;

		public float stalkLength;

		public Vector2[,] segs;

		private float connRad;

		private float sinCycles;

		private float sinSide;

		private Vector2 fruitPos;

		public Vector2 stalkDirVec;

		public Vector2 baseDirVec;

		private float coil;

		private float coilGoal;

		private float coilGoalGoal;

		private float coilSin;

		private float coilSinMode;

		private float coilSinModeGoal;

		private float coilSinSpeed;

		public float[,] stickies;

		public Stalk(SporePlant sporePlant, Room room, Vector2 fruitPos)
		{
			this.sporePlant = sporePlant;
			this.fruitPos = fruitPos;
			sporePlant.firstChunk.HardSetPosition(fruitPos);
			UnityEngine.Random.State state = UnityEngine.Random.state;
			UnityEngine.Random.InitState(sporePlant.abstractPhysicalObject.ID.RandomSeed);
			stalkDirVec = Custom.DegToVec(Mathf.Lerp(30f, 110f, UnityEngine.Random.value) * ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f));
			sinSide = ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f);
			stuckPos.x = fruitPos.x;
			stalkLength = -1f;
			List<Stalk> list = new List<Stalk>();
			for (int i = 0; i < room.updateList.Count; i++)
			{
				if (room.updateList[i] is SporePlant && (room.updateList[i] as SporePlant).stalk != null && (room.updateList[i] as SporePlant).stalk != this)
				{
					list.Add((room.updateList[i] as SporePlant).stalk);
				}
			}
			int num = room.GetTilePosition(fruitPos).x;
			for (int num2 = room.GetTilePosition(fruitPos).y; num2 >= 0; num2--)
			{
				if (room.GetTile(num, num2).Solid)
				{
					bool flag = false;
					if (list.Count > 0)
					{
						for (int j = -1; j < 2; j += 2)
						{
							if (flag)
							{
								break;
							}
							for (int num3 = Math.Min(20, Math.Abs(room.GetTilePosition(fruitPos).y - num2)); num3 >= 0; num3--)
							{
								if (room.GetTile(num + j, num2).Solid && !room.GetTile(num + j, num2 + 1).Solid)
								{
									for (int k = 0; k < list.Count; k++)
									{
										if (room.GetTilePosition(list[k].stuckPos) == new IntVector2(num, num2))
										{
											flag = true;
											stuckPos = list[k].stuckPos;
											break;
										}
									}
									if (flag)
									{
										break;
									}
									num += j;
								}
							}
						}
					}
					if (flag)
					{
						break;
					}
					num = room.GetTilePosition(fruitPos).x;
					for (int num4 = UnityEngine.Random.Range(0, Math.Min(20, Math.Abs(room.GetTilePosition(fruitPos).y - num2))); num4 >= 0; num4--)
					{
						if (room.GetTile(num + (int)sinSide, num2).Solid && !room.GetTile(num + (int)sinSide, num2 + 1).Solid)
						{
							num += (int)sinSide;
						}
					}
					stuckPos = room.MiddleOfTile(num, num2) + new Vector2(Mathf.Lerp(-5f, 5f, UnityEngine.Random.value), 5f);
					break;
				}
			}
			stalkLength = Mathf.Abs(stuckPos.y - fruitPos.y) * 1.1f + 30f;
			baseDirVec = Custom.DirVec(stuckPos, fruitPos);
			segs = new Vector2[Math.Max(1, (int)(stalkLength / 8f)), 3];
			for (int l = 0; l < segs.GetLength(0); l++)
			{
				float t = (float)l / (float)(segs.GetLength(0) - 1);
				segs[l, 0] = Vector2.Lerp(stuckPos, fruitPos, t);
				segs[l, 1] = segs[l, 0];
			}
			connRad = stalkLength / Mathf.Pow(segs.GetLength(0), 1.1f);
			sinCycles = stalkLength / 40f * Mathf.Lerp(0.75f, 1.25f, UnityEngine.Random.value);
			stickies = new float[UnityEngine.Random.Range(segs.GetLength(0) / 3, (int)((float)segs.GetLength(0) / 0.75f)), 3];
			for (int m = 0; m < stickies.GetLength(0); m++)
			{
				stickies[m, 0] = Mathf.Pow(UnityEngine.Random.value, 1.5f);
				stickies[m, 1] = UnityEngine.Random.value;
				stickies[m, 2] = -1f;
				if (!(UnityEngine.Random.value > 0.2f) || !(UnityEngine.Random.value > Mathf.Pow(Mathf.InverseLerp(0.1f, 0.5f, stickies[m, 0]), 1.5f)))
				{
					continue;
				}
				IntVector2 tilePosition = room.GetTilePosition(stuckPos);
				int p = ((!(UnityEngine.Random.value < 0.5f)) ? 1 : (-1));
				for (int num5 = UnityEngine.Random.Range(0, UnityEngine.Random.Range(2, 6)); num5 >= 0; num5--)
				{
					if (room.GetTile(tilePosition + new IntVector2(p, 1)).Solid)
					{
						tilePosition += new IntVector2(p, 1);
					}
					else
					{
						if (!room.GetTile(tilePosition + new IntVector2(p, 0)).Solid)
						{
							break;
						}
						tilePosition += new IntVector2(p, 0);
					}
				}
				stickies[m, 1] = room.MiddleOfTile(tilePosition).x + Mathf.Lerp(-10f, 10f, UnityEngine.Random.value);
				stickies[m, 2] = room.MiddleOfTile(tilePosition).y + 10f;
			}
			UnityEngine.Random.state = state;
			coilGoalGoal = UnityEngine.Random.value;
			coilGoal = coilGoalGoal;
			coilSinSpeed = UnityEngine.Random.value;
			coilSinModeGoal = UnityEngine.Random.value;
			coilSinMode = coilSinModeGoal;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (stalkLength == -1f)
			{
				Destroy();
				return;
			}
			if (sporePlant != null)
			{
				sporePlant.firstChunk.vel.y += sporePlant.gravity;
				sporePlant.firstChunk.vel *= 0.8f;
				sporePlant.firstChunk.vel -= (sporePlant.firstChunk.pos - fruitPos) / 30f;
				sporePlant.firstChunk.vel.y += 1.05f;
				segs[segs.GetLength(0) - 1, 2].y -= 1.9499999f;
			}
			else
			{
				segs[segs.GetLength(0) - 1, 2] -= (baseDirVec + stalkDirVec) * 2f;
				segs[segs.GetLength(0) - 1, 2] -= (segs[segs.GetLength(0) - 1, 0] - fruitPos) / 30f;
			}
			for (int i = 0; i < segs.GetLength(0); i++)
			{
				segs[i, 1] = segs[i, 0];
			}
			ConnectSegments(dir: true);
			ConnectSegments(dir: false);
			Vector2 vector = Custom.PerpendicularVector(baseDirVec) * sinSide;
			for (int j = 0; j < segs.GetLength(0); j++)
			{
				float num = (float)j / (float)(segs.GetLength(0) - 1);
				float num2 = Mathf.Pow(coil, Mathf.Lerp(0.5f, 3.5f, num));
				segs[j, 0] += segs[j, 2];
				segs[j, 2] *= 0.94f;
				segs[j, 2].y += 3.6f * Mathf.Pow(Mathf.Clamp(1f - Mathf.Sin(num * (float)Math.PI), 0f, 1f), 3f) * (1f - num2);
				segs[j, 2] += Custom.DirVec(segs[j, 0], stuckPos) * Mathf.Lerp(-0.5f, 0.7f, num2) * num;
				segs[j, 2] += Mathf.Sin(num * (float)Math.PI * Mathf.Lerp(1.25f, 0.75f, num2) * sinCycles) * vector * Mathf.Lerp(3f, 5f, num2) * Mathf.Sin(Mathf.Pow(num, 0.5f) * (float)Math.PI);
				segs[j, 2] -= baseDirVec * Mathf.Pow(Mathf.InverseLerp(0.75f, 1f, num), 0.5f) * 1.5f;
				segs[j, 2].y += Mathf.InverseLerp(stuckPos.y + stalkLength / 4f, stuckPos.y, segs[j, 0].y) * 5f;
				if (j > 1)
				{
					Vector2 vector2 = Custom.DirVec(segs[j, 0], segs[j - 2, 0]);
					segs[j, 2] -= vector2 * 0.6f;
					segs[j - 2, 2] += vector2 * 0.6f;
				}
			}
			coil = Custom.LerpAndTick(coil, Mathf.Lerp(coilGoal, 0.5f + 0.5f * Mathf.Sin(coilSin), Custom.SCurve(coilSinMode, 0.2f)), 0.06f, 1f / Mathf.Lerp(stalkLength, 40f, 0.75f));
			coilSin += Mathf.Lerp(0.007f, 0.013f, coilSinSpeed);
			coilGoal = Custom.LerpAndTick(coilGoal, coilGoalGoal, 0.001f, 1f / 120f);
			coilSinMode = Custom.LerpAndTick(coilSinMode, coilSinModeGoal, 0.001f, 0.0013888889f);
			if (UnityEngine.Random.value < 0.0033333334f)
			{
				coilGoalGoal = ((sporePlant != null) ? UnityEngine.Random.value : 1f);
			}
			if (UnityEngine.Random.value < 0.0033333334f)
			{
				coilSinSpeed = ((sporePlant != null) ? UnityEngine.Random.value : 0f);
			}
			if (UnityEngine.Random.value < 0.0033333334f)
			{
				coilSinModeGoal = ((sporePlant != null) ? UnityEngine.Random.value : 0f);
			}
			ConnectSegments(dir: false);
			ConnectSegments(dir: true);
			if (sporePlant != null)
			{
				sporePlant.setRotation = Custom.DirVec(segs[segs.GetLength(0) - 1, 0], sporePlant.firstChunk.pos);
				if (!Custom.DistLess(sporePlant.firstChunk.pos, stuckPos, stalkLength * 1.4f + 10f) || sporePlant.grabbedBy.Count > 0 || sporePlant.slatedForDeletetion || sporePlant.room != room || !room.VisualContact(stuckPos + new Vector2(0f, 10f), sporePlant.firstChunk.pos))
				{
					sporePlant.AbstrSporePlant.Consume();
					sporePlant = null;
				}
			}
		}

		private void ConnectSegments(bool dir)
		{
			int num = ((!dir) ? (segs.GetLength(0) - 1) : 0);
			bool flag = false;
			while (!flag)
			{
				if (num == 0)
				{
					if (!Custom.DistLess(segs[num, 0], stuckPos, connRad))
					{
						Vector2 vector = Custom.DirVec(segs[num, 0], stuckPos) * (Vector2.Distance(segs[num, 0], stuckPos) - connRad);
						segs[num, 0] += vector;
						segs[num, 2] += vector;
					}
				}
				else
				{
					if (!Custom.DistLess(segs[num, 0], segs[num - 1, 0], connRad))
					{
						Vector2 vector2 = Custom.DirVec(segs[num, 0], segs[num - 1, 0]) * (Vector2.Distance(segs[num, 0], segs[num - 1, 0]) - connRad);
						segs[num, 0] += vector2 * 0.5f;
						segs[num, 2] += vector2 * 0.5f;
						segs[num - 1, 0] -= vector2 * 0.5f;
						segs[num - 1, 2] -= vector2 * 0.5f;
					}
					if (num == segs.GetLength(0) - 1 && sporePlant != null)
					{
						Vector2 vector3 = Custom.DirVec(segs[num, 0], sporePlant.firstChunk.pos) * (Vector2.Distance(segs[num, 0], sporePlant.firstChunk.pos) - 12f);
						segs[num, 0] += vector3 * 0.65f;
						segs[num, 2] += vector3 * 0.65f;
						sporePlant.firstChunk.vel -= vector3 * 0.35f;
					}
				}
				num += (dir ? 1 : (-1));
				if (dir && num >= segs.GetLength(0))
				{
					flag = true;
				}
				else if (!dir && num < 0)
				{
					flag = true;
				}
			}
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1 + stickies.Length];
			sLeaser.sprites[0] = TriangleMesh.MakeLongMesh(segs.GetLength(0), pointyTip: false, customColor: true);
			for (int i = 0; i < stickies.Length; i++)
			{
				sLeaser.sprites[i + 1] = new FSprite("pixel");
				sLeaser.sprites[i + 1].anchorY = 0f;
			}
			AddToContainer(sLeaser, rCam, null);
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = stuckPos;
			float num = 1.5f;
			for (int i = 0; i < segs.GetLength(0); i++)
			{
				float f = (float)i / (float)(segs.GetLength(0) - 1);
				float num2 = Mathf.Lerp(Custom.LerpMap(i, 1f, 5f, 4f, 1f), Mathf.Lerp(stalkLength / 40f, 3f, 0.5f), Mathf.Sin(Mathf.Pow(f, Mathf.Lerp(3f / (float)segs.GetLength(0), 0.125f, 0.5f)) * (float)Math.PI));
				Vector2 vector2 = Vector2.Lerp(segs[i, 1], segs[i, 0], timeStacker);
				if (i == segs.GetLength(0) - 1 && sporePlant != null)
				{
					vector2 = Vector2.Lerp(sporePlant.firstChunk.lastPos, sporePlant.firstChunk.pos, timeStacker) - sporePlant.GetRotat(timeStacker) * 6f;
				}
				Vector2 vector3 = Custom.PerpendicularVector((vector - vector2).normalized);
				vector2 = new Vector2(Mathf.Floor(vector2.x) + 0.5f, Mathf.Floor(vector2.y) + 0.5f);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4, vector - vector3 * num - camPos);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 1, vector + vector3 * num - camPos);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 2, vector2 - vector3 * num2 - camPos);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 3, vector2 + vector3 * num2 - camPos);
				vector = vector2;
				num = num2;
			}
			for (int j = 0; j < stickies.GetLength(0); j++)
			{
				Vector2 vector4 = OnStalkPos(stickies[j, 0], timeStacker);
				Vector2 vector5 = ((!(stickies[j, 2] > -1f)) ? OnStalkPos(stickies[j, 1], timeStacker) : new Vector2(stickies[j, 1], stickies[j, 2]));
				sLeaser.sprites[j + 1].x = vector4.x - camPos.x;
				sLeaser.sprites[j + 1].y = vector4.y - camPos.y;
				sLeaser.sprites[j + 1].scaleY = Vector2.Distance(vector4, vector5);
				sLeaser.sprites[j + 1].rotation = Custom.AimFromOneVectorToAnother(vector4, vector5);
			}
			if (base.slatedForDeletetion || room != rCam.room)
			{
				sLeaser.CleanSpritesAndRemove();
			}
		}

		public Vector2 OnStalkPos(float f, float timeStacker)
		{
			f *= (float)(segs.GetLength(0) - 1);
			int num = OnStalkIndex(f);
			int num2 = Custom.IntClamp(num + 1, 0, segs.GetLength(0) - 1);
			return Vector2.Lerp(Vector2.Lerp(segs[num, 1], segs[num, 0], timeStacker), Vector2.Lerp(segs[num2, 1], segs[num2, 0], timeStacker), Mathf.InverseLerp(num, num2, f));
		}

		public int OnStalkIndex(float f)
		{
			return Custom.IntClamp(Mathf.FloorToInt(f), 0, segs.GetLength(0) - 1);
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			for (int i = 0; i < sLeaser.sprites.Length; i++)
			{
				sLeaser.sprites[i].color = palette.blackColor;
			}
		}

		public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			for (int i = 0; i < sLeaser.sprites.Length; i++)
			{
				sLeaser.sprites[i].RemoveFromContainer();
				rCam.ReturnFContainer("Background").AddChild(sLeaser.sprites[i]);
			}
		}
	}

	public class BeeSpark : CosmeticSprite
	{
		private float lastLife;

		private float life;

		private float lifeTime;

		private float randomRotat;

		public BeeSpark(Vector2 pos)
		{
			base.pos = pos;
			lastPos = pos;
			lifeTime = Mathf.Lerp(2f, 6f, UnityEngine.Random.value);
			life = 1f;
			lastLife = 1f;
			randomRotat = UnityEngine.Random.value;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			lastLife = life;
			life -= 1f / lifeTime;
			if (lastLife < 0f)
			{
				Destroy();
			}
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[3];
			sLeaser.sprites[0] = new FSprite("Futile_White");
			sLeaser.sprites[0].color = new Color(1f, 0f, 0f);
			sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["FlatLight"];
			sLeaser.sprites[1] = new FSprite("pixel");
			sLeaser.sprites[2] = new FSprite("pixel");
			AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Foreground"));
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			for (int i = 0; i < sLeaser.sprites.Length; i++)
			{
				sLeaser.sprites[i].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x;
				sLeaser.sprites[i].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y;
			}
			float num = Mathf.Lerp(lastLife, life, timeStacker);
			float value = Mathf.Pow(num, 3f) * UnityEngine.Random.value;
			float num2 = Mathf.Pow(Mathf.InverseLerp(0f, 0.5f, value), 0.4f);
			sLeaser.sprites[1].color = new Color(1f, num2, num2);
			sLeaser.sprites[1].scaleX = 18f * UnityEngine.Random.value * num;
			sLeaser.sprites[1].rotation = 360f * (2f * Custom.SCurve(num, 0.3f) + randomRotat);
			sLeaser.sprites[2].color = new Color(1f, num2, num2);
			sLeaser.sprites[2].scaleY = 18f * UnityEngine.Random.value * num;
			sLeaser.sprites[2].rotation = 360f * (2f * Custom.SCurve(num, 0.3f) + randomRotat);
			sLeaser.sprites[0].alpha = UnityEngine.Random.value * num;
			sLeaser.sprites[0].scale = (26f * UnityEngine.Random.value + 18f * Mathf.Pow(UnityEngine.Random.value, 3f)) * num / 10f;
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
	}

	public Stalk stalk;

	public List<Bee> bees;

	public List<AttachedBee> attachedBees;

	private Vector2? swarmPos;

	public bool deployOnCollision;

	public float scalesOpen;

	public float scalesOpenGoal;

	public float lastScalesOpen;

	public float swallowed;

	public int releaseBeesCounter;

	public int releaseBeesDelay;

	public List<IntVector2> swarmTrail;

	public List<IntVector2> possibleDestinations;

	private IntVector2 fillMatrixOffset;

	public int[,] floodFillMatrix;

	public float hoverOffsetCounter;

	private int hoverPhase;

	private float angry;

	public float[][,] scales;

	public Color colorA;

	public Color colorB;

	private Color blackColor;

	public AbstractSporePlant AbstrSporePlant => abstractPhysicalObject as AbstractSporePlant;

	public override int DefaultCollLayer => 1;

	public bool Used
	{
		get
		{
			return AbstrSporePlant.used;
		}
		set
		{
			AbstrSporePlant.used = value;
		}
	}

	public bool Pacified
	{
		get
		{
			return AbstrSporePlant.pacified;
		}
		set
		{
			AbstrSporePlant.pacified = value;
		}
	}

	public bool Swarming
	{
		get
		{
			if (releaseBeesCounter > 0)
			{
				return possibleDestinations.Count > 0;
			}
			return false;
		}
	}

	public bool UsableAsWeapon
	{
		get
		{
			if (Pacified && !Used)
			{
				return !Swarming;
			}
			return false;
		}
	}

	private int CobSprite => 0;

	private int TotScales
	{
		get
		{
			int num = 0;
			for (int i = 0; i < scales.Length; i++)
			{
				num += scales[i].GetLength(0);
			}
			return num;
		}
	}

	private int TotalSprites => 1 + TotScales * 2;

	public Vector2 GetRotat(float timeStacker)
	{
		return Vector3.Slerp(lastRotation, rotation, timeStacker);
	}

	public static bool SporePlantInterested(CreatureTemplate.Type tp)
	{
		if (tp != CreatureTemplate.Type.Overseer && tp != CreatureTemplate.Type.PoleMimic)
		{
			if (ModManager.MSC)
			{
				return tp != MoreSlugcatsEnums.CreatureTemplateType.Inspector;
			}
			return true;
		}
		return false;
	}

	public Vector2 HoverOffset(int group)
	{
		return ((group + hoverPhase) % 4) switch
		{
			0 => new Vector2(0f - Mathf.Lerp(-25f, 25f, Mathf.PingPong(hoverOffsetCounter * 100f, 100f) / 100f), Mathf.Lerp(-15f, 15f, Mathf.PingPong(hoverOffsetCounter * 100f, 100f) / 100f)), 
			1 => new Vector2(0f - Mathf.Lerp(-25f, 25f, Mathf.PingPong(hoverOffsetCounter * 100f, 100f) / 100f), 0f - Mathf.Lerp(-15f, 15f, Mathf.PingPong(hoverOffsetCounter * 100f, 100f) / 100f)), 
			2 => new Vector2(Mathf.Lerp(-25f, 25f, Mathf.PingPong(hoverOffsetCounter * 100f, 100f) / 100f), 0f - Mathf.Lerp(-15f, 15f, Mathf.PingPong(hoverOffsetCounter * 100f, 100f) / 100f)), 
			_ => new Vector2(Mathf.Lerp(-25f, 25f, Mathf.PingPong(hoverOffsetCounter * 100f, 100f) / 100f), Mathf.Lerp(-15f, 15f, Mathf.PingPong(hoverOffsetCounter * 100f, 100f) / 100f)), 
		};
	}

	public SporePlant(AbstractPhysicalObject abstractPhysicalObject, World world)
		: base(abstractPhysicalObject, world)
	{
		base.bodyChunks = new BodyChunk[1];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 8f, 0.2f);
		bodyChunkConnections = new BodyChunkConnection[0];
		bees = new List<Bee>();
		attachedBees = new List<AttachedBee>();
		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		bounce = 0.2f;
		surfaceFriction = 0.7f;
		collisionLayer = ((!Used) ? 1 : 0);
		base.waterFriction = 0.95f;
		base.buoyancy = 1.1f;
		canBeHitByWeapons = !Used;
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(abstractPhysicalObject.ID.RandomSeed);
		scales = new float[UnityEngine.Random.Range(5, 7)][,];
		for (int i = 0; i < scales.Length; i++)
		{
			float num = Mathf.InverseLerp(0f, scales.Length - 1, i);
			scales[i] = new float[Mathf.RoundToInt(Mathf.Lerp(4f - 2f * num, 5f, Mathf.Sin(Mathf.Pow(num, 0.5f) * (float)Math.PI))), 3];
			for (int j = 0; j < scales[i].GetLength(0); j++)
			{
				scales[i][j, 2] = UnityEngine.Random.value;
			}
		}
		UnityEngine.Random.state = state;
		hoverOffsetCounter = UnityEngine.Random.value * 100f;
		hoverPhase = 10000 + UnityEngine.Random.Range(0, 4);
	}

	public void PuffBallSpores(Vector2 pos, float rad)
	{
		if (Custom.DistLess(pos, base.firstChunk.pos, rad + 20f))
		{
			Pacify();
		}
		for (int i = 0; i < bees.Count; i++)
		{
			if (Custom.DistLess(pos, bees[i].pos, rad + 20f))
			{
				bees[i].life -= 0.5f;
			}
		}
	}

	public void Pacify()
	{
		Pacified = true;
		angry = 0f;
		deployOnCollision = false;
		releaseBeesDelay = 0;
		releaseBeesCounter = 0;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (Used || Pacified)
		{
			angry = Mathf.Max(0f, angry - 0.0032258064f);
		}
		else if (deployOnCollision || Swarming)
		{
			angry = Custom.LerpAndTick(angry, 1f, 0.08f, 1f / 30f);
		}
		else
		{
			float num = 1f;
			if (!room.ViewedByAnyCamera(base.firstChunk.pos, 200f))
			{
				num = 0f;
				for (int i = 0; i < room.game.cameras.Length; i++)
				{
					if (room.game.cameras[i].room == room)
					{
						num = Mathf.Max(num, Mathf.InverseLerp(3000f, 700f, Vector2.Distance(room.cameraPositions[room.game.cameras[i].currentCameraPosition] + new Vector2((ModManager.MMF ? room.game.rainWorld.options.ScreenSize.x : 1024f) / 2f, 384f), base.firstChunk.pos)));
					}
				}
				num = Mathf.Lerp(0.25f, 0.75f, num);
			}
			bool flag = true;
			if (grabbedBy.Count > 0)
			{
				flag = false;
			}
			else
			{
				for (int j = 0; j < room.abstractRoom.creatures.Count && flag; j++)
				{
					if (room.abstractRoom.creatures[j].creatureTemplate.quantified || !SporePlantInterested(room.abstractRoom.creatures[j].creatureTemplate.type) || room.abstractRoom.creatures[j].realizedCreature == null || room.abstractRoom.creatures[j].realizedCreature.dead)
					{
						continue;
					}
					for (int k = 0; k < room.abstractRoom.creatures[j].realizedCreature.bodyChunks.Length; k++)
					{
						if (Custom.DistLess(base.firstChunk.pos, room.abstractRoom.creatures[j].realizedCreature.bodyChunks[k].pos, 300f * num))
						{
							flag = false;
							break;
						}
					}
				}
			}
			if (flag)
			{
				angry = Mathf.Max(0f, angry - 0.0025f);
			}
			else
			{
				float num2 = 0f;
				if (grabbedBy.Count > 0)
				{
					num2 = 1f;
				}
				else if (room.abstractRoom.creatures.Count > 0)
				{
					Creature realizedCreature = room.abstractRoom.creatures[UnityEngine.Random.Range(0, room.abstractRoom.creatures.Count)].realizedCreature;
					if (realizedCreature != null && !realizedCreature.Template.quantified && !realizedCreature.dead)
					{
						float num3 = Custom.LerpMap(realizedCreature.TotalMass, 0f, 1.5f, 40f, 400f, 0.25f) * num;
						for (int l = 0; l < realizedCreature.bodyChunks.Length; l++)
						{
							if (Custom.DistLess(base.firstChunk.pos, realizedCreature.bodyChunks[l].pos, num3) && room.VisualContact(base.firstChunk.pos, realizedCreature.bodyChunks[l].pos))
							{
								for (int m = 0; m < realizedCreature.bodyChunks.Length; m++)
								{
									num2 = Mathf.Max(num2, Mathf.InverseLerp(num3, num3 * 0.5f, Vector2.Distance(base.firstChunk.pos, realizedCreature.bodyChunks[l].pos)));
								}
								break;
							}
						}
					}
					num2 *= (float)room.abstractRoom.creatures.Count * Mathf.Lerp(num, 1f, 0.5f);
				}
				angry = Mathf.Min(1f, angry + num2 / 120f);
			}
			if (base.firstChunk.submersion > 0.5f)
			{
				Pacify();
			}
		}
		if (angry >= 1f && !Pacified && !deployOnCollision)
		{
			BeeTrigger();
		}
		lastScalesOpen = scalesOpen;
		if (Used)
		{
			scalesOpen = Custom.LerpAndTick(scalesOpen, 1f, 0.06f, 1f / 140f);
		}
		else
		{
			scalesOpen = Custom.LerpAndTick(scalesOpen, scalesOpenGoal, 0.04f, 1f / Mathf.Lerp(110f, 40f, angry));
			if (Pacified)
			{
				scalesOpenGoal = 0f;
			}
			else if (UnityEngine.Random.value < 1f / Mathf.Lerp(90f, 5f, angry))
			{
				scalesOpenGoal = Mathf.Lerp(0f, 0.2f + 0.4f * angry, Mathf.Pow(UnityEngine.Random.value, 2f - angry));
			}
		}
		for (int n = 0; n < scales.Length; n++)
		{
			float num4 = Mathf.InverseLerp(0f, scales.Length - 1, n);
			for (int num5 = 0; num5 < scales[n].GetLength(0); num5++)
			{
				scales[n][num5, 1] = scales[n][num5, 0];
				if (Pacified)
				{
					scales[n][num5, 0] = 0f;
					continue;
				}
				scales[n][num5, 0] = Mathf.Lerp(Mathf.Max(0f, scales[n][num5, 0] - 1f / (4f + 14f * angry)), UnityEngine.Random.value, 0.5f * angry * num4);
				if (n > 0 && !Used && UnityEngine.Random.value < 1f / Mathf.Lerp(80f - 60f * angry, 30f - 28f * angry, num4))
				{
					scales[n][num5, 0] = Mathf.Min(1f, UnityEngine.Random.value * 2f * num4) * Mathf.Pow(num4, 0.8f - 0.6f * angry);
				}
				scales[n][num5, 1] = Mathf.Clamp(scales[n][num5, 1] + Mathf.Lerp(-0.6f, 0.6f, UnityEngine.Random.value) * angry, 0f, 1f);
			}
		}
		if (Used)
		{
			base.forbiddenToPlayer = 10;
		}
		hoverOffsetCounter += 1f / 12f;
		if (UnityEngine.Random.value < 1f / 48f)
		{
			hoverPhase += ((!(UnityEngine.Random.value < 0.5f)) ? 1 : (-1));
		}
		if (room.game.devToolsActive && Input.GetKey("b"))
		{
			base.firstChunk.vel += Custom.DirVec(base.firstChunk.pos, Futile.mousePosition) * 3f;
		}
		if (grabbedBy.Count > 0)
		{
			rotation = Custom.PerpendicularVector(Custom.DirVec(base.firstChunk.pos, grabbedBy[0].grabber.mainBodyChunk.pos));
			rotation.y = Mathf.Abs(rotation.y);
		}
		else if (setRotation.HasValue)
		{
			rotation = setRotation.Value;
			setRotation = null;
		}
		if (!Used && UnityEngine.Random.value < 1f / Mathf.Lerp(100f, 20f, angry) && (float)bees.Count < UnityEngine.Random.Range(0f, 4f + 3f * angry))
		{
			AddBee(Bee.Mode.BuzzAroundHive);
		}
		if (base.firstChunk.ContactPoint.y != 0)
		{
			rotationSpeed = (rotationSpeed * 2f + base.firstChunk.vel.x * 5f) / 3f;
			base.firstChunk.vel.x *= 0.8f;
		}
		if (room != null)
		{
			bool flag2 = true;
			if (ModManager.MMF && !MMF.cfgVanillaExploits.Value && room.abstractRoom.gate && room.regionGate.waitingForWorldLoader)
			{
				flag2 = false;
			}
			if (flag2)
			{
				if (releaseBeesDelay > 0 && base.firstChunk.submersion < 0.5f)
				{
					releaseBeesDelay--;
					if (releaseBeesDelay == 0)
					{
						ReleaseBees();
					}
				}
				if (Swarming)
				{
					if (room.GetTilePosition(base.firstChunk.pos) != swarmTrail[swarmTrail.Count - 1] && !room.GetTile(base.firstChunk.pos).Solid)
					{
						swarmTrail.Add(room.GetTilePosition(base.firstChunk.pos));
					}
					base.firstChunk.vel += Custom.RNV() * 1.5f * UnityEngine.Random.value;
					rotationSpeed += Mathf.Lerp(-14f, 14f, UnityEngine.Random.value);
					if (releaseBeesCounter > 0)
					{
						releaseBeesCounter--;
						AddDestinationBee();
						if (releaseBeesCounter > 20)
						{
							AddDestinationBee();
						}
					}
				}
				else if (deployOnCollision)
				{
					if (base.firstChunk.ContactPoint.x != 0 || base.firstChunk.ContactPoint.y != 0)
					{
						BeeTrigger();
					}
					if (UnityEngine.Random.value < 0.25f && bees.Count < 7)
					{
						AddBee(Bee.Mode.BuzzAroundHive);
					}
				}
			}
		}
		for (int num6 = attachedBees.Count - 1; num6 >= 0; num6--)
		{
			if (attachedBees[num6].slatedForDeletetion || attachedBees[num6].attachedChunk == null)
			{
				attachedBees.RemoveAt(num6);
			}
		}
		bool flag3 = false;
		if (grabbedBy.Count > 0 && grabbedBy[0].grabber is Player && (grabbedBy[0].grabber as Player).swallowAndRegurgitateCounter > 50 && (grabbedBy[0].grabber as Player).objectInStomach == null && (grabbedBy[0].grabber as Player).input[0].pckp)
		{
			int num7 = -1;
			for (int num8 = 0; num8 < 2; num8++)
			{
				if ((grabbedBy[0].grabber as Player).grasps[num8] != null && (grabbedBy[0].grabber as Player).CanBeSwallowed((grabbedBy[0].grabber as Player).grasps[num8].grabbed))
				{
					num7 = num8;
					break;
				}
			}
			if (num7 > -1 && (grabbedBy[0].grabber as Player).grasps[num7] != null && (grabbedBy[0].grabber as Player).grasps[num7].grabbed == this)
			{
				flag3 = true;
			}
		}
		swallowed = Custom.LerpAndTick(swallowed, flag3 ? 1f : 0f, 0.05f, 0.05f);
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		base.PlaceInRoom(placeRoom);
		if ((!AbstrSporePlant.isConsumed && AbstrSporePlant.placedObjectIndex >= 0 && AbstrSporePlant.placedObjectIndex < placeRoom.roomSettings.placedObjects.Count) || AbstrSporePlant.placedObjectIndex == -2 || (ModManager.MMF && room.game.IsArenaSession && (MMF.cfgSandboxItemStems.Value || room.game.GetArenaGameSession.chMeta != null) && grabbedBy.Count == 0 && AbstrSporePlant.stuckObjects.Count == 0 && room.game.GetArenaGameSession.counter < 10))
		{
			base.firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
			stalk = new Stalk(this, placeRoom, base.firstChunk.pos);
			placeRoom.AddObject(stalk);
			for (int num = UnityEngine.Random.Range(0, 4); num >= 0; num--)
			{
				AddBee(Bee.Mode.BuzzAroundHive);
			}
		}
		else
		{
			base.firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
			rotation = Custom.RNV();
			lastRotation = rotation;
		}
	}

	public override void HitByWeapon(Weapon weapon)
	{
		base.HitByWeapon(weapon);
		if (!(weapon is PuffBall))
		{
			BeeTrigger();
		}
	}

	public override void HitByExplosion(float hitFac, Explosion explosion, int hitChunk)
	{
		base.HitByExplosion(hitFac, explosion, hitChunk);
		if (UnityEngine.Random.value < hitFac)
		{
			BeeTrigger();
		}
	}

	public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
	{
		base.TerrainImpact(chunk, direction, speed, firstContact);
		if (deployOnCollision)
		{
			BeeTrigger();
		}
	}

	public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
	{
		base.Collide(otherObject, myChunk, otherChunk);
		if (!Pacified && !Used)
		{
			angry = Mathf.Min(1f, angry + UnityEngine.Random.value / 12f);
			if (angry >= 1f)
			{
				BeeTrigger();
			}
		}
	}

	public override bool HitSomething(SharedPhysics.CollisionResult result, bool eu)
	{
		if (result.obj == null)
		{
			return false;
		}
		bool result2 = base.HitSomething(result, eu);
		base.firstChunk.vel = Vector2.ClampMagnitude(base.firstChunk.vel, 4f);
		if (deployOnCollision)
		{
			BeeTrigger();
		}
		return result2;
	}

	public void BeeTrigger()
	{
		Pacified = false;
		bool flag = true;
		if (ModManager.MMF)
		{
			if (room == null)
			{
				flag = false;
			}
			else if (!MMF.cfgVanillaExploits.Value && room.abstractRoom.gate && room.regionGate.waitingForWorldLoader)
			{
				flag = false;
			}
		}
		if (flag && releaseBeesDelay == 0)
		{
			releaseBeesDelay = 3;
		}
	}

	public override void Thrown(Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 throwDir, float frc, bool eu)
	{
		base.Thrown(thrownBy, thrownPos, firstFrameTraceFromPos, throwDir, frc, eu);
		if (!Used)
		{
			deployOnCollision = true;
		}
		Pacified = false;
	}

	public void ReleaseBees()
	{
		deployOnCollision = false;
		if (Used)
		{
			return;
		}
		float num = float.MaxValue;
		IntVector2 intVector = room.GetTilePosition(base.firstChunk.pos);
		for (int i = 0; i < 5; i++)
		{
			float num2 = Vector2.Distance(base.firstChunk.pos, room.MiddleOfTile(room.GetTilePosition(base.firstChunk.pos) + Custom.eightDirectionsAndZero[i])) + Vector2.Distance(base.firstChunk.pos, room.MiddleOfTile(room.GetTilePosition(base.firstChunk.lastPos) + Custom.eightDirectionsAndZero[i]));
			if (room.GetTile(room.GetTilePosition(base.firstChunk.pos) + Custom.eightDirectionsAndZero[i]).Terrain == Room.Tile.TerrainType.Air && num2 < num)
			{
				intVector = room.GetTilePosition(base.firstChunk.pos) + Custom.eightDirectionsAndZero[i];
				num = num2;
			}
		}
		if (room.GetTile(intVector).Solid)
		{
			return;
		}
		swarmPos = base.firstChunk.pos;
		Custom.Log("RELEASE BEES");
		Used = true;
		ChangeCollisionLayer(0);
		canBeHitByWeapons = false;
		floodFillMatrix = new int[17, 17];
		IntVector2 intVector2 = new IntVector2(floodFillMatrix.GetLength(0) / 2, floodFillMatrix.GetLength(1) / 2);
		fillMatrixOffset = intVector - intVector2;
		for (int j = 0; j < floodFillMatrix.GetLength(0); j++)
		{
			for (int k = 0; k < floodFillMatrix.GetLength(1); k++)
			{
				if (!Custom.DistLess(new Vector2(j, k), intVector2.ToVector2(), (float)floodFillMatrix.GetLength(0) / 2f) || (room.water && intVector.y - intVector2.y + k <= room.defaultWaterLevel) || (room.GetTile(intVector.x - intVector2.x + j, intVector.y - intVector2.y + k).Terrain != 0 && room.GetTile(intVector.x - intVector2.x + j, intVector.y - intVector2.y + k).Terrain != Room.Tile.TerrainType.Floor))
				{
					floodFillMatrix[j, k] = -1;
				}
			}
		}
		List<IntVector2> list = new List<IntVector2> { intVector };
		possibleDestinations = new List<IntVector2>();
		SetFloodFillMatrixValue(intVector.x, intVector.y, 1);
		int num3 = 0;
		while (list.Count > 0 && num3 < 10000)
		{
			int index = UnityEngine.Random.Range(0, list.Count);
			IntVector2 intVector3 = list[index];
			list.RemoveAt(index);
			int floodFillMatrixValue = GetFloodFillMatrixValue(intVector3.x, intVector3.y);
			for (int l = 0; l < 4; l++)
			{
				if (GetFloodFillMatrixValue(intVector3.x + Custom.fourDirections[l].x, intVector3.y + Custom.fourDirections[l].y) == 0)
				{
					SetFloodFillMatrixValue(intVector3.x + Custom.fourDirections[l].x, intVector3.y + Custom.fourDirections[l].y, floodFillMatrixValue + 1);
					list.Add(intVector3 + Custom.fourDirections[l]);
					possibleDestinations.Add(intVector3 + Custom.fourDirections[l]);
				}
			}
			num3++;
		}
		if (possibleDestinations.Count == 0)
		{
			possibleDestinations = null;
			return;
		}
		swarmTrail = new List<IntVector2> { intVector };
		for (int m = 0; m < 10; m++)
		{
			AddDestinationBee();
		}
		releaseBeesCounter = 40;
		room.PlaySound(SoundID.Spore_Bees_Emerge, base.firstChunk);
	}

	public int GetFloodFillMatrixValue(int x, int y)
	{
		x -= fillMatrixOffset.x;
		y -= fillMatrixOffset.y;
		if (x < 0 || x >= floodFillMatrix.GetLength(0) || y < 0 || y >= floodFillMatrix.GetLength(1))
		{
			return -1;
		}
		return floodFillMatrix[x, y];
	}

	public void SetFloodFillMatrixValue(int x, int y, int value)
	{
		x -= fillMatrixOffset.x;
		y -= fillMatrixOffset.y;
		if (x >= 0 && x < floodFillMatrix.GetLength(0) && y >= 0 && y < floodFillMatrix.GetLength(1))
		{
			floodFillMatrix[x, y] = value;
		}
	}

	public Bee AddBee(Bee.Mode mode)
	{
		Bee bee = new Bee(this, Swarming || deployOnCollision, base.firstChunk.pos, base.firstChunk.vel, mode);
		bees.Add(bee);
		room.AddObject(bee);
		return bee;
	}

	public void AddDestinationBee()
	{
		if (possibleDestinations == null || possibleDestinations.Count == 0)
		{
			return;
		}
		Bee bee = AddBee(Bee.Mode.FollowPath);
		int index = UnityEngine.Random.Range(0, possibleDestinations.Count);
		for (int i = 0; i < 10; i++)
		{
			int num = UnityEngine.Random.Range(0, possibleDestinations.Count);
			if (GetFloodFillMatrixValue(possibleDestinations[num].x, possibleDestinations[num].y) > GetFloodFillMatrixValue(possibleDestinations[index].x, possibleDestinations[index].y))
			{
				index = num;
			}
		}
		List<IntVector2> list = new List<IntVector2> { possibleDestinations[index] };
		IntVector2 item = possibleDestinations[index];
		possibleDestinations.RemoveAt(index);
		if (possibleDestinations.Count == 0)
		{
			for (int j = 0; j < floodFillMatrix.GetLength(0); j++)
			{
				for (int k = 0; k < floodFillMatrix.GetLength(1); k++)
				{
					if (floodFillMatrix[j, k] > 0)
					{
						possibleDestinations.Add(new IntVector2(j, k) + fillMatrixOffset);
					}
				}
			}
		}
		for (int l = 0; l < 100; l++)
		{
			int num2 = int.MaxValue;
			int num3 = UnityEngine.Random.Range(0, 4);
			for (int m = 0; m < 4; m++)
			{
				int floodFillMatrixValue = GetFloodFillMatrixValue(item.x + Custom.fourDirections[m].x, item.y + Custom.fourDirections[m].y);
				if (floodFillMatrixValue > 0 && floodFillMatrixValue < num2)
				{
					num3 = m;
					num2 = floodFillMatrixValue;
				}
			}
			item += Custom.fourDirections[num3];
			if (GetFloodFillMatrixValue(item.x, item.y) < 2)
			{
				break;
			}
			list.Add(item);
		}
		for (int n = 0; n < swarmTrail.Count; n++)
		{
			list.Add(swarmTrail[n]);
		}
		bee.travelDist = list.Count;
		if (list.Count > 2)
		{
			int num4 = list.Count / 2;
			if (room.VisualContact(bee.pos, room.MiddleOfTile(list[num4])))
			{
				for (int num5 = list.Count - 1; num5 > num4; num5--)
				{
					list.RemoveAt(num4);
				}
			}
		}
		bee.path = list;
	}

	public void RemoveBee(Bee beeToRemove)
	{
		for (int num = bees.Count - 1; num >= 0; num--)
		{
			if (bees[num] == beeToRemove)
			{
				bees[num].owner = null;
				bees.RemoveAt(num);
			}
		}
	}

	private int ScaleSprite(int row, int scale, int part)
	{
		int num = 0;
		for (int i = 0; i < row; i++)
		{
			num += scales[i].GetLength(0);
		}
		return 1 + (num + scale) * 2 + part;
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[TotalSprites];
		sLeaser.sprites[CobSprite] = new FSprite("Circle20");
		for (int num = TotScales * 2 - 1; num >= 0; num--)
		{
			sLeaser.sprites[num + 1] = new FSprite("Circle20");
			sLeaser.sprites[num + 1].anchorY = 0f;
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
		Vector2 vector2 = Vector3.Slerp(lastRotation, rotation, timeStacker);
		Vector2 vector3 = -Custom.PerpendicularVector(vector2);
		float num = base.firstChunk.rad * Mathf.Lerp(Pacified ? 0.95f : (1f + 0.1f * angry), 0.6f, swallowed);
		float num2 = Mathf.Min(Mathf.Lerp(lastScalesOpen, scalesOpen, timeStacker), 1f - swallowed);
		sLeaser.sprites[CobSprite].x = vector.x - camPos.x;
		sLeaser.sprites[CobSprite].y = vector.y - camPos.y;
		sLeaser.sprites[CobSprite].rotation = Custom.VecToDeg(vector2);
		sLeaser.sprites[CobSprite].scaleX = Mathf.Lerp(6f, 2f, num2) / 20f;
		sLeaser.sprites[CobSprite].scaleY = num * 2f / 20f;
		Color color = colorA;
		if (blink > 1 && UnityEngine.Random.value < 0.5f)
		{
			color = new Color(1f, 1f, 1f);
		}
		if (blink > 0 && UnityEngine.Random.value < 0.5f)
		{
			sLeaser.sprites[1].color = new Color(1f, 1f, 1f);
		}
		else
		{
			sLeaser.sprites[1].color = base.color;
		}
		for (int i = 0; i < scales.Length; i++)
		{
			float num3 = Mathf.InverseLerp(0f, scales.Length - 1, i);
			for (int j = 0; j < scales[i].GetLength(0); j++)
			{
				float num4 = Mathf.InverseLerp(0f, scales[i].GetLength(0) - 1, j);
				for (int k = 0; k < 2; k++)
				{
					int num5 = ScaleSprite(i, j, k);
					Vector2 vector4 = vector;
					vector4 += vector2 * (-1f + 1.5f * Mathf.Pow(num3, 1.5f)) * num;
					vector4 += vector3 * (-1f + 2f * num4) * num * Mathf.Lerp(0.06f, 0.5f, Mathf.Sin(Mathf.Pow(num3, 0.5f) * (float)Math.PI));
					sLeaser.sprites[num5].x = vector4.x - camPos.x;
					sLeaser.sprites[num5].y = vector4.y - camPos.y;
					sLeaser.sprites[num5].scaleX = Mathf.Lerp(Mathf.Lerp(7f, 4f, num3) * Mathf.Lerp(0.4f, 1f, Mathf.Pow(Mathf.Clamp(Mathf.Sin(num4 * (float)Math.PI), 0f, 1f), 0.5f)), 2f, num2) * ((k == 1) ? (1f + 0.1f * num3) : 1f) / 20f;
					sLeaser.sprites[num5].scaleY = (0.25f + 0.75f * Mathf.Pow(num3, 0.35f)) * Mathf.Lerp(num * 0.85f, num * 1.15f, Mathf.Sin(num3 * (float)Math.PI)) * Mathf.Lerp(1f, 1.4f, num2) * ((k == 1) ? 1.2f : 1f) / 20f;
					sLeaser.sprites[num5].rotation = Custom.VecToDeg(vector2) + (-1f + 2f * num4 + Mathf.Lerp(-0.25f, 0.25f, scales[i][j, 2])) * Mathf.Lerp(45f + 100f * num2, 10f + 30f * num2, Mathf.Pow(num3, Mathf.Lerp(0.75f, 4f, num2)));
					if (k == 0)
					{
						sLeaser.sprites[num5].color = BlinkCol(color, Custom.SCurve(Mathf.Lerp(scales[i][j, 1], scales[i][j, 0], timeStacker), 0.4f));
					}
					else
					{
						sLeaser.sprites[num5].color = Color.Lerp(color, colorB, num3 * (Pacified ? 0.3f : (0.6f + 0.4f * angry)));
					}
				}
			}
		}
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public Color BlinkCol(Color frameColA, float f)
	{
		if (f < 0.5f)
		{
			return Color.Lerp(frameColA, new Color(1f, 0f, 0f), Mathf.InverseLerp(0f, 0.5f, f));
		}
		return Color.Lerp(new Color(1f, 0f, 0f), new Color(1f, 1f, 1f), Mathf.Pow(Mathf.InverseLerp(0.5f, 1f, f), 3f));
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i].color = palette.blackColor;
		}
		blackColor = palette.blackColor;
		colorA = blackColor;
		colorB = new Color(1f, 0f, 0f);
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Items");
		}
		sLeaser.sprites[0].RemoveFromContainer();
		newContatiner.AddChild(sLeaser.sprites[0]);
		for (int num = sLeaser.sprites.Length - 1; num >= 1; num--)
		{
			sLeaser.sprites[num].RemoveFromContainer();
			newContatiner.AddChild(sLeaser.sprites[num]);
		}
	}
}
